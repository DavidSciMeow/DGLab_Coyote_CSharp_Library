using DGLablib;
using System.Windows;


namespace UIHostCoyoteDevice
{
    public partial class MainWindow : Window
    {
        string? b1buf = null;
        string? nbuf = null;
        private async void OnScanClick(object sender, RoutedEventArgs e)
        {
            ScanButton.IsEnabled = false;
            Say("正在搜索设备");
            CoyoteDevice = await Task.Run(CoyoteDeviceV3.ScanFirst);
            if (CoyoteDevice == null)
            {
                Say("\n\n无设备可用");
                return;
            }
            Say($"设备 {CoyoteDevice.Name} : {CoyoteDevice.Id} 已获取");
            Say($"设备电量:{await CoyoteDevice.GetBatteryLevel()}");
            //CoyoteDevice.NotificationReceived += (s, e) =>
            //{
            //    var p = $"Debug N -- Notification received: {BitConverter.ToString(e)}";
            //    if (nbuf == p) return;
            //    nbuf = p;
            //    Say(p);
            //};
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
}