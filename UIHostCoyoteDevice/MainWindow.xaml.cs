using DGLablib;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace UIHostCoyoteDevice
{
    public partial class MainWindow : Window
    {
        CoyoteDeviceV3? CoyoteDevice;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void OnOneShotClick(object sender, RoutedEventArgs e)
        {
            CoyoteDevice?.SetWaveformAsync(new(
                    byte.Parse(SA.Text),
                    byte.Parse(SB.Text),
                    [byte.Parse(fA1.Text), byte.Parse(fA2.Text), byte.Parse(fA3.Text), byte.Parse(fA4.Text)],
                    [byte.Parse(iA1.Text), byte.Parse(iA2.Text), byte.Parse(iA3.Text), byte.Parse(iA4.Text)],
                    [byte.Parse(fB1.Text), byte.Parse(fB2.Text), byte.Parse(fB3.Text), byte.Parse(fB4.Text)],
                    [byte.Parse(iB1.Text), byte.Parse(iB2.Text), byte.Parse(iB3.Text), byte.Parse(iB4.Text)]));
        }
        private void OnStartClick(object sender, RoutedEventArgs e)
        {
            if (CoyoteDevice != null)
            {
                CoyoteDevice.WaveNow = new(
                    byte.Parse(SA.Text),
                    byte.Parse(SB.Text),
                    [byte.Parse(fA1.Text), byte.Parse(fA2.Text), byte.Parse(fA3.Text), byte.Parse(fA4.Text)],
                    [byte.Parse(iA1.Text), byte.Parse(iA2.Text), byte.Parse(iA3.Text), byte.Parse(iA4.Text)],
                    [byte.Parse(fB1.Text), byte.Parse(fB2.Text), byte.Parse(fB3.Text), byte.Parse(fB4.Text)],
                    [byte.Parse(iB1.Text), byte.Parse(iB2.Text), byte.Parse(iB3.Text), byte.Parse(iB4.Text)]);
            }
            CoyoteDevice?.Start();
        }
        private void OnStopClick(object sender, RoutedEventArgs e) => CoyoteDevice?.Stop();
        private void OnScanClick(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                var selector = BluetoothLEDevice.GetDeviceSelector();
                var deviceWatcher = DeviceInformation.CreateWatcher(selector);
                var devlist = new List<DeviceInformation>();
                bool enuming = true;
                deviceWatcher.Added += async (watcher, deviceInfo) =>
                {
                    devlist.Add(deviceInfo);
                    await Dispatcher.InvokeAsync(() =>
                    {
                        OutputTextBox.Text += $"Device Found...{deviceInfo.Name} {deviceInfo.Id}\n";
                        OutputTextBox.ScrollToEnd();
                    });
                };

                deviceWatcher.Removed += async (watcher, deviceInfoUpdate) =>
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        OutputTextBox.Text += $"Device Removed...{deviceInfoUpdate.Id}\n";
                        OutputTextBox.ScrollToEnd();
                    });
                };

                deviceWatcher.EnumerationCompleted += async (watcher, obj) =>
                {
                    enuming = false;
                    await Dispatcher.InvokeAsync(() =>
                    {
                        OutputTextBox.Text += "Device enumeration completed.\n";
                        OutputTextBox.ScrollToEnd();
                    });
                };
                deviceWatcher.Start();
                while (enuming) Task.Delay(100).Wait();

                await Dispatcher.InvokeAsync(() =>
                {
                    OutputTextBox.Text += "Scanning for Coyote devices...\n";
                    OutputTextBox.ScrollToEnd();
                });

                var devices = await CoyoteDeviceV3.Scan();

                if (devices.Count == 0)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        OutputTextBox.Text += "No Coyote devices found.\n";
                        OutputTextBox.ScrollToEnd();
                    });
                    return;
                }

                await Dispatcher.InvokeAsync(() =>
                {
                    OutputTextBox.Text += $"Found {devices.Count} Coyote device(s):\n";
                    for (int i = 0; i < devices.Count; i++)
                    {
                        OutputTextBox.Text += $"{i + 1}. {devices[i].Name}\n";
                    }
                    OutputTextBox.ScrollToEnd();
                });

                CoyoteDevice = devices[0];
                await CoyoteDevice.SetWaveBFAsync(new WaveformBF(200));
                //CoyoteDevice.NotificationReceived += (s, e) =>
                //{
                //    Dispatcher.Invoke(() =>
                //    {
                //        OutputTextBox.Text += $"N -- Notification received: {BitConverter.ToString(e)}\n";
                //        OutputTextBox.ScrollToEnd();
                //    });
                //};
                //CoyoteDevice.B1MessageReceived += (s, e) =>
                //{
                //    Dispatcher.Invoke(() =>
                //    {
                //        OutputTextBox.Text += $"Number:{s}, intA/B [{e[0]}][{e[1]}]\n";
                //        OutputTextBox.ScrollToEnd();
                //    });
                //};
            });
        }
    }
}