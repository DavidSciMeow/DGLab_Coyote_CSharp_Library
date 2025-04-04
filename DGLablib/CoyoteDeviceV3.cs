using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace DGLablib
{
    /// <summary>
    /// 郊狼3设备类
    /// </summary>
    public class CoyoteDeviceV3
    {
        /// <summary>
        /// 具体请求的设备
        /// </summary>
        private BluetoothLEDevice _device;
        /// <summary>
        /// 特性保存串
        /// </summary>
        private readonly Dictionary<string, GattCharacteristic> _characteristics = new();
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
        public byte BatteryLevel => ReadCharacteristicAsync(CoyoteV3.BatteryData.ToString()).GetAwaiter().GetResult()[0];
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
                                    B1MessageReceived?.Invoke(input[1], new byte[] { 0, 0 });
                                }
                            };
                        }
                    }
                }
            }
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
            return Array.Empty<byte>();
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
        /// 默认的扫描设备函数
        /// </summary>
        /// <returns>返回所有郊狼3实例</returns>
        public static async Task<List<CoyoteDeviceV3>> Scan()
        {
            var devices = new List<CoyoteDeviceV3>();
            var selector = BluetoothLEDevice.GetDeviceSelector();
            var devices_general = new List<DeviceInformation>();
            var deviceWatcher = DeviceInformation.CreateWatcher(selector);
            bool enuming = true;
            deviceWatcher.Added += (watcher, deviceInfo) => devices_general.Add(deviceInfo);
            deviceWatcher.EnumerationCompleted += (watcher, obj) => enuming = false;
            deviceWatcher.Start();
            while (enuming) Task.Delay(100).Wait();
            //var deviceInfos = await DeviceInformation.FindAllAsync(selector);

            foreach (var deviceInfo in devices_general)
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
        /// <summary>
        /// 析构函数
        /// </summary>
        ~CoyoteDeviceV3()
        {
            _cancellationTokenSource.Cancel();
            InputVoltTask.Dispose();
            SetNotifyAsync(CoyoteV3.CharacteristicNotify.ToString(), false).GetAwaiter().GetResult();
        }
    }
}