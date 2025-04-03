using System.Reflection.PortableExecutable;
using System.Text;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using System.Threading;

namespace DGLablib
{
    public class CoyoteDeviceV3
    {
        private BluetoothLEDevice _device;
        private readonly Dictionary<string, GattCharacteristic> _characteristics = new();
        private bool disposedValue;
        public CancellationTokenSource _cancellationTokenSource;

        public delegate void NotificationReceivedHandler(Guid uuid, byte[] data);
        public event NotificationReceivedHandler? NotificationReceived;
        public delegate void B1MessageReceivedHandler(byte number, byte[] Volt);
        public event B1MessageReceivedHandler? B1MessageReceived;
        public delegate void BEMessageReceivedHandler(byte[] Parameter);
        public event BEMessageReceivedHandler? BEMessageReceived;

        public byte BatteryLevel => ReadCharacteristicAsync(CoyoteV3.BatteryData.ToString()).GetAwaiter().GetResult()[0];
        public string Name { get; }
        public string Id { get; }
        public WaveformV3 WaveNow { get; set; } = new();
        public Task InputVoltTask { get; private set; } = null!;
        private string lastoutput = "";

        private CoyoteDeviceV3(BluetoothLEDevice device)
        {
            _device = device;
            Name = device.Name;
            Id = device.DeviceId;
            _cancellationTokenSource = new CancellationTokenSource();

            var services = _device.GetGattServicesAsync().GetAwaiter().GetResult();
            foreach (var service in services.Services)
            {
                var characteristics = service.GetCharacteristicsAsync().GetAwaiter().GetResult();
                foreach (var characteristic in characteristics.Characteristics)
                {
                    _characteristics.Add(characteristic.Uuid.ToString(), characteristic);
                    if (characteristic.Uuid.ToString() == "0000150b-0000-1000-8000-00805f9b34fb")
                    {
                        if (SetNotifyAsync(characteristic.Uuid.ToString(), true).GetAwaiter().GetResult())
                        {
                            characteristic.ValueChanged += (sender, args) =>
                            {
                                var reader = DataReader.FromBuffer(args.CharacteristicValue);
                                byte[] input = new byte[reader.UnconsumedBufferLength];
                                reader.ReadBytes(input);
                                NotificationReceived?.Invoke(characteristic.Uuid, input);

                                if (input[0] == 0xB1)
                                {
                                    B1MessageReceived?.Invoke(input[1], input[2..4]);
                                }
                                else if (input[0] == 0xBE)
                                {
                                    BEMessageReceived?.Invoke(input[1..]);
                                }
                                else if (input[0] == 0xE0)
                                {
                                    B1MessageReceived?.Invoke(input[1], new byte[] { 0, 0 });
                                }
                            };
                        }
                    }
                }
            }
        }

        private async void Input()
        {
            Console.WriteLine("Input Volt Task Start");
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (await WriteComaandAsync(WaveNow))
                {
                    if (WaveNow.ToString() != lastoutput)
                    {
                        lastoutput = WaveNow.ToString();
                        Console.WriteLine($"Input Wave Now: {WaveNow}");
                    }
                    Task.Delay(WaveNow.MilisecondLastA > WaveNow.MilisecondLastB ? WaveNow.MilisecondLastA : WaveNow.MilisecondLastB).Wait();
                }
            }
            Console.WriteLine("Input Volt Task Ended");
        }

        public bool Stop()
        {
            _cancellationTokenSource.Cancel();
            InputVoltTask?.Wait();
            return InputVoltTask?.IsCompleted ?? false;
        }

        public bool Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            InputVoltTask = Task.Factory.StartNew(Input, _cancellationTokenSource.Token);
            return InputVoltTask.Status == TaskStatus.Running;
        }

        public async Task<bool> WriteCharacteristicAsync(string characteristicUuid, byte[] data)
        {
            if (_characteristics.TryGetValue(characteristicUuid, out var characteristic))
            {
                var writer = new DataWriter();
                writer.WriteBytes(data);
                var result = await characteristic.WriteValueAsync(writer.DetachBuffer());
                return result == GattCommunicationStatus.Success;
            }
            return false;
        }
        public async Task<byte[]> ReadCharacteristicAsync(string characteristicUuid)
        {
            if (_characteristics.TryGetValue(characteristicUuid, out var characteristic))
            {
                var result = await characteristic.ReadValueAsync();
                if (result.Status == GattCommunicationStatus.Success)
                {
                    var reader = DataReader.FromBuffer(result.Value);
                    byte[] input = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(input);
                    return input;
                }
            }
            return Array.Empty<byte>();
        }
        public async Task<bool> SetNotifyAsync(string characteristicUuid, bool enable)
        {
            if (_characteristics.TryGetValue(characteristicUuid, out var characteristic))
            {
                try
                {
                    if (_device.ConnectionStatus != BluetoothConnectionStatus.Connected)
                    {
                        Console.WriteLine("Device is not connected.");
                        return false;
                    }
                    var status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(enable ? GattClientCharacteristicConfigurationDescriptorValue.Notify : GattClientCharacteristicConfigurationDescriptorValue.None);
                    return status == GattCommunicationStatus.Success;
                }
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine($"Operation was canceled: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return false;
                }
            }
            return false;
        }

        public Task<byte[]> ReadNotify() => ReadCharacteristicAsync(CoyoteV3.CharacteristicNotify.ToString());

        public Task<bool> WriteComaandAsync(byte[] command) => WriteCharacteristicAsync(CoyoteV3.CharacteristicWrite.ToString(), command);
        public Task<bool> SetWaveformAsync(WaveformV3 command) => WriteComaandAsync(command);
        public Task<bool> SetWaveBFAsync(WaveformBF command) => WriteComaandAsync(command);

        public static async Task<List<CoyoteDeviceV3>> Scan()
        {
            var devices = new List<CoyoteDeviceV3>();
            var selector = BluetoothLEDevice.GetDeviceSelector();
            var deviceInfos = await DeviceInformation.FindAllAsync(selector);

            foreach (var deviceInfo in deviceInfos)
            {
                if (deviceInfo.Name.Equals(CoyoteV3.Name) || deviceInfo.Name.Equals(CoyoteV3.WirelessSensorName))
                {
                    var device = await BluetoothLEDevice.FromIdAsync(deviceInfo.Id);
                    if (device != null)
                    {
                        var coyoteDevice = new CoyoteDeviceV3(device);
                        devices.Add(coyoteDevice);
                    }
                }
            }

            return devices;
        }

        ~CoyoteDeviceV3()
        {
            _cancellationTokenSource.Cancel();
            InputVoltTask.Dispose();
            SetNotifyAsync(CoyoteV3.CharacteristicNotify.ToString(), false).GetAwaiter().GetResult();
        }
    }
}