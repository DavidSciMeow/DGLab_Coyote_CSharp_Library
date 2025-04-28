using DGLablib;
using System.Windows;


namespace UIHostCoyoteDevice
{
    public partial class MainWindow : Window
    {
        string? b1buf = null;
        private async void OnScanClick(object sender, RoutedEventArgs e)
        {
            ScanButton.IsEnabled = false;
            LoadingAnimation.IsActive = true; // 显示加载动画
            Say("正在搜索设备");

            // 设置超时时间为两分钟
            var timeout = TimeSpan.FromMinutes(2);
            using var cts = new CancellationTokenSource(timeout);

            try
            {
                // 开始扫描任务，支持取消
                CoyoteDevice = await Task.Run(CoyoteDeviceV3.ScanFirst, cts.Token);

                if (CoyoteDevice == null)
                {
                    Say("\n无设备可用");
                    ScanButton.IsEnabled = true;
                }
                else
                {
                    Say($"设备 {CoyoteDevice.Name} : {CoyoteDevice.Id} 已获取");
                    Say($"设备电量:{await CoyoteDevice.GetBatteryLevel()}");

                    CoyoteDevice.B1MessageReceived += (s, e) =>
                    {
                        var p = $"通道输出信息: 序列号:{s}, 通道强度 A/B [{e[0]}]/[{e[1]}]";
                        if (b1buf == p) return;
                        b1buf = p;
                        Say(p);
                    };

                    ViewModel.IsDeviceConnected = true;
                }
            }
            catch (OperationCanceledException)
            {
                Say("\n扫描超时，未找到设备");
                ScanButton.IsEnabled = true;
            }
            finally
            {
                LoadingAnimation.IsActive = false;
            }
        }
    }
}