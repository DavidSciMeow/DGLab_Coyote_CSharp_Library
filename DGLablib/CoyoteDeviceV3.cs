using System.Diagnostics;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace DGLablib
{
    /// <summary>
    /// 郊狼3设备类
    /// </summary>
    public class CoyoteDeviceV3 : IDisposable
    {
        /// <summary>
        /// 具体请求的设备
        /// </summary>
        private readonly BluetoothLEDevice _device;
        /// <summary>
        /// 特性保存串
        /// </summary>
        private readonly Dictionary<string, GattCharacteristic> _characteristics = [];
        /// <summary>
        /// 上次输出的字符串
        /// </summary>
        private string lastoutput = "";
        /// <summary>
        /// 输入电压任务
        /// </summary>
        private Task InputVoltTask = null!;
        /// <summary>
        /// 任务取消标
        /// </summary>
        public CancellationTokenSource _cancellationTokenSource;
        private bool disposedValue;

        /// <summary>
        /// 接收到通知回调
        /// </summary>
        /// <param name="uuid">通知特性uuid</param>
        /// <param name="data">数据</param>
        public delegate void NotificationReceivedHandler(Guid uuid, byte[] data);
        /// <summary>
        /// 接收到通知回调
        /// </summary>
        public event NotificationReceivedHandler? NotificationReceived;
        /// <summary>
        /// B1消息回调
        /// </summary>
        /// <param name="number">序列号</param>
        /// <param name="Volt">电压[A,B]</param>
        public delegate void B1MessageReceivedHandler(byte number, byte[] Volt);
        /// <summary>
        /// 接收到B1消息
        /// </summary>
        public event B1MessageReceivedHandler? B1MessageReceived;
        /// <summary>
        /// 接收BE消息回调
        /// </summary>
        /// <param name="Parameter">回传的byte串</param>
        public delegate void BEMessageReceivedHandler(byte[] Parameter);
        /// <summary>
        /// 接收到BE消息
        /// </summary>
        public event BEMessageReceivedHandler? BEMessageReceived;

        /// <summary>
        /// 电池电量 (0-100)
        /// </summary>
        public async Task<byte?> GetBatteryLevel() => (await ReadCharacteristicAsync(CoyoteV3.BatteryData.ToString()))?[0] ?? null;
        /// <summary>
        /// 设备名
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// 设备ID
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// 当前波形
        /// </summary>
        public WaveformV3 WaveNow { get; set; } = new();
        /// <summary>
        /// 默认实例化
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
        /// 输入电压任务
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
        /// 停止电压输入
        /// </summary>
        /// <returns>任务是否成功结束</returns>
        public bool Stop()
        {
            _cancellationTokenSource.Cancel();
            InputVoltTask?.Wait();
            return InputVoltTask?.IsCompleted ?? false;
        }
        /// <summary>
        /// 开始电压输入
        /// </summary>
        /// <returns>任务是否成功开始</returns>
        public bool Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            InputVoltTask = Task.Factory.StartNew(Input, _cancellationTokenSource.Token);
            return InputVoltTask.Status == TaskStatus.Running;
        }
        /// <summary>
        /// 写入特性值
        /// </summary>
        /// <param name="characteristicUuid">特性UUID</param>
        /// <param name="data">数据</param>
        /// <returns>是否写入成功</returns>
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
        /// 读取特性值
        /// </summary>
        /// <param name="characteristicUuid">特性UUID</param>
        /// <returns>返回的Byte串</returns>
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
        /// 设置特性通知模式
        /// </summary>
        /// <param name="characteristicUuid">特性UUID</param>
        /// <param name="enable">是否启动</param>
        /// <returns>设置是否成功</returns>
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
        /// 读取设备通知数据
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> ReadNotify() => ReadCharacteristicAsync(CoyoteV3.CharacteristicNotify.ToString());
        /// <summary>
        /// 写入命令
        /// </summary>
        /// <param name="command">命令组</param>
        /// <returns>是否设置成功</returns>
        public Task<bool> WriteComaandAsync(byte[] command) => WriteCharacteristicAsync(CoyoteV3.CharacteristicWrite.ToString(), command);
        /// <summary>
        /// B0命令
        /// </summary>
        /// <param name="command">命令串</param>
        /// <returns>是否设置成功</returns>
        public Task<bool> SetWaveformAsync(WaveformV3 command) => WriteComaandAsync(command);
        /// <summary>
        /// BF命令
        /// </summary>
        /// <param name="command">命令串</param>
        /// <returns>是否设置成功</returns>
        public Task<bool> SetWaveBFAsync(WaveformBF command) => WriteComaandAsync(command);
        /// <summary>
        /// 扫描所有设备
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
        /// 默认的扫描设备函数(扫描一个)
        /// </summary>
        /// <returns>返回所有郊狼3实例</returns>
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