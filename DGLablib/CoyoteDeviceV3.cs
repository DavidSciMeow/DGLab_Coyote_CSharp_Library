using System.Diagnostics;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace DGLablib
{
    /// <summary>
    /// ����3�豸��
    /// </summary>
    public class CoyoteDeviceV3 : IDisposable
    {
        /// <summary>
        /// ����������豸
        /// </summary>
        private readonly BluetoothLEDevice _device;
        /// <summary>
        /// ���Ա��洮
        /// </summary>
        private readonly Dictionary<string, GattCharacteristic> _characteristics = [];
        /// <summary>
        /// �ϴ�������ַ���
        /// </summary>
        private string lastoutput = "";
        /// <summary>
        /// �����ѹ����
        /// </summary>
        private Task InputVoltTask = null!;
        /// <summary>
        /// ����ȡ����
        /// </summary>
        public CancellationTokenSource _cancellationTokenSource;
        private bool disposedValue;

        /// <summary>
        /// ���յ�֪ͨ�ص�
        /// </summary>
        /// <param name="uuid">֪ͨ����uuid</param>
        /// <param name="data">����</param>
        public delegate void NotificationReceivedHandler(Guid uuid, byte[] data);
        /// <summary>
        /// ���յ�֪ͨ�ص�
        /// </summary>
        public event NotificationReceivedHandler? NotificationReceived;
        /// <summary>
        /// B1��Ϣ�ص�
        /// </summary>
        /// <param name="number">���к�</param>
        /// <param name="Volt">��ѹ[A,B]</param>
        public delegate void B1MessageReceivedHandler(byte number, byte[] Volt);
        /// <summary>
        /// ���յ�B1��Ϣ
        /// </summary>
        public event B1MessageReceivedHandler? B1MessageReceived;
        /// <summary>
        /// ����BE��Ϣ�ص�
        /// </summary>
        /// <param name="Parameter">�ش���byte��</param>
        public delegate void BEMessageReceivedHandler(byte[] Parameter);
        /// <summary>
        /// ���յ�BE��Ϣ
        /// </summary>
        public event BEMessageReceivedHandler? BEMessageReceived;

        /// <summary>
        /// ��ص��� (0-100)
        /// </summary>
        public async Task<byte?> GetBatteryLevel() => (await ReadCharacteristicAsync(CoyoteV3.BatteryData.ToString()))?[0] ?? null;
        /// <summary>
        /// �豸��
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// �豸ID
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// ��ǰ����
        /// </summary>
        public WaveformV3 WaveNow { get; set; } = new();
        /// <summary>
        /// Ĭ��ʵ����
        /// </summary>
        /// <param name="device"></param>
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
                                    B1MessageReceived?.Invoke(input[1], [0, 0]);
                                }
                            };
                        }
                    }
                }
            }

            SetWaveBFAsync(new WaveformBF(200)).GetAwaiter().GetResult();
        }
        /// <summary>
        /// �����ѹ����
        /// </summary>
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
        /// <summary>
        /// ֹͣ��ѹ����
        /// </summary>
        /// <returns>�����Ƿ�ɹ�����</returns>
        public bool Stop()
        {
            _cancellationTokenSource.Cancel();
            InputVoltTask?.Wait();
            return InputVoltTask?.IsCompleted ?? false;
        }
        /// <summary>
        /// ��ʼ��ѹ����
        /// </summary>
        /// <returns>�����Ƿ�ɹ���ʼ</returns>
        public bool Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            InputVoltTask = Task.Factory.StartNew(Input, _cancellationTokenSource.Token);
            return InputVoltTask.Status == TaskStatus.Running;
        }
        /// <summary>
        /// д������ֵ
        /// </summary>
        /// <param name="characteristicUuid">����UUID</param>
        /// <param name="data">����</param>
        /// <returns>�Ƿ�д��ɹ�</returns>
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
        /// <summary>
        /// ��ȡ����ֵ
        /// </summary>
        /// <param name="characteristicUuid">����UUID</param>
        /// <returns>���ص�Byte��</returns>
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
            return [];
        }
        /// <summary>
        /// ��������֪ͨģʽ
        /// </summary>
        /// <param name="characteristicUuid">����UUID</param>
        /// <param name="enable">�Ƿ�����</param>
        /// <returns>�����Ƿ�ɹ�</returns>
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
        /// <summary>
        /// ��ȡ�豸֪ͨ����
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> ReadNotify() => ReadCharacteristicAsync(CoyoteV3.CharacteristicNotify.ToString());
        /// <summary>
        /// д������
        /// </summary>
        /// <param name="command">������</param>
        /// <returns>�Ƿ����óɹ�</returns>
        public Task<bool> WriteComaandAsync(byte[] command) => WriteCharacteristicAsync(CoyoteV3.CharacteristicWrite.ToString(), command);
        /// <summary>
        /// B0����
        /// </summary>
        /// <param name="command">���</param>
        /// <returns>�Ƿ����óɹ�</returns>
        public Task<bool> SetWaveformAsync(WaveformV3 command) => WriteComaandAsync(command);
        /// <summary>
        /// BF����
        /// </summary>
        /// <param name="command">���</param>
        /// <returns>�Ƿ����óɹ�</returns>
        public Task<bool> SetWaveBFAsync(WaveformBF command) => WriteComaandAsync(command);
        /// <summary>
        /// ɨ�������豸
        /// </summary>
        /// <returns></returns>
        public static async Task<List<CoyoteDeviceV3>> ScanAll()
        {
            if (!await Util.IsBluetoothEnabledAsync()) await Util.RequestEnableBluetoothAsync();
            List<CoyoteDeviceV3> device = [];
            foreach (var i in await DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelectorFromConnectionStatus(BluetoothConnectionStatus.Connected)))
            {
                var ledev = await BluetoothLEDevice.FromIdAsync(i.Id);
                Debug.WriteLine($"Found : {ledev.Name.Trim()}");
                if (ledev.Name.Trim().Equals(CoyoteV3.Name.Trim()))
                {
                    device.Add(new CoyoteDeviceV3(ledev));
                    Debug.WriteLine($"Founded ! : {ledev.Name.Trim()}");
                }
            }
            foreach (var i in await DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelectorFromConnectionStatus(BluetoothConnectionStatus.Disconnected)))
            {
                var ledev = await BluetoothLEDevice.FromIdAsync(i.Id);
                Debug.WriteLine($"Found : {ledev.Name.Trim()}");
                if (ledev.Name.Trim().Equals(CoyoteV3.Name.Trim()))
                {
                    device.Add(new CoyoteDeviceV3(ledev));
                    Debug.WriteLine($"Founded ! : {ledev.Name.Trim()}");
                }
            }
            return device;
        }
        /// <summary>
        /// Ĭ�ϵ�ɨ���豸����(ɨ��һ��)
        /// </summary>
        /// <returns>�������н���3ʵ��</returns>
        public static async Task<CoyoteDeviceV3?> ScanFirst()
        {
            if (!await Util.IsBluetoothEnabledAsync()) await Util.RequestEnableBluetoothAsync();
            foreach (var i in await DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelectorFromConnectionStatus(BluetoothConnectionStatus.Connected)))
            {
                var ledev = await BluetoothLEDevice.FromIdAsync(i.Id);
                Debug.WriteLine($"Found : {ledev.Name.Trim()}");
                if (ledev.Name.Trim().Equals(CoyoteV3.Name.Trim()))
                {
                    Debug.WriteLine($"Founded ! : {ledev.Name.Trim()}");
                    return new CoyoteDeviceV3(ledev);
                }
            }
            foreach (var i in await DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelectorFromConnectionStatus(BluetoothConnectionStatus.Disconnected)))
            {
                var ledev = await BluetoothLEDevice.FromIdAsync(i.Id);
                Debug.WriteLine($"Found : {ledev.Name.Trim()}");
                if (ledev.Name.Trim().Equals(CoyoteV3.Name.Trim()))
                {
                    Debug.WriteLine($"Founded ! : {ledev.Name.Trim()}");
                    return new CoyoteDeviceV3(ledev);
                }
            }
            return null;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"><inheritdoc/></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _device.Dispose();
                    _cancellationTokenSource.Cancel();
                    InputVoltTask.Dispose();
                    SetNotifyAsync(CoyoteV3.CharacteristicNotify.ToString(), false).GetAwaiter().GetResult();
                    Debug.WriteLine("dispose compelte");
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CoyoteDeviceV3()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}