using DGLablib;
using System.Windows;
using System.Windows.Controls;

namespace UIHostCoyoteDevice
{
    public partial class MainWindow : Window
    {
        CoyoteDeviceV3? CoyoteDevice;
        string? b1buf = null;
        string? nbuf = null;

        public MainWindow()
        {
            InitializeComponent();
            StopButton.IsEnabled = false;
        }

        private async void Say(string s) =>
            await Dispatcher.InvokeAsync(() =>
            {
                OutputTextBox.Text += $"{s}\n";
                OutputTextBox.ScrollToEnd();
            });

        private void OnUpdateClick(object sender, RoutedEventArgs e)
        {
            if (CoyoteDevice != null)
            {
                var viewModel = (SliderViewModel)DataContext;
                CoyoteDevice.WaveNow = new(
                    (byte)viewModel.SAValue,
                    (byte)viewModel.SBValue,
                    [(byte)viewModel.FA1Value, (byte)viewModel.FA2Value, (byte)viewModel.FA3Value, (byte)viewModel.FA4Value],
                    [(byte)viewModel.IA1Value, (byte)viewModel.IA2Value, (byte)viewModel.IA3Value, (byte)viewModel.IA4Value],
                    [(byte)viewModel.FB1Value, (byte)viewModel.FB2Value, (byte)viewModel.FB3Value, (byte)viewModel.FB4Value],
                    [(byte)viewModel.IB1Value, (byte)viewModel.IB2Value, (byte)viewModel.IB3Value, (byte)viewModel.IB4Value]);
            }
            else
            {
                Say("No Device Avaliable");
            }
        }

        private void OnOneShotClick(object sender, RoutedEventArgs e)
        {
            if (CoyoteDevice != null)
            {
                var viewModel = (SliderViewModel)DataContext;
                CoyoteDevice.SetWaveformAsync(new(
                    (byte)viewModel.SAValue,
                    (byte)viewModel.SBValue,
                    [(byte)viewModel.FA1Value, (byte)viewModel.FA2Value, (byte)viewModel.FA3Value, (byte)viewModel.FA4Value],
                    [(byte)viewModel.IA1Value, (byte)viewModel.IA2Value, (byte)viewModel.IA3Value, (byte)viewModel.IA4Value],
                    [(byte)viewModel.FB1Value, (byte)viewModel.FB2Value, (byte)viewModel.FB3Value, (byte)viewModel.FB4Value],
                    [(byte)viewModel.IB1Value, (byte)viewModel.IB2Value, (byte)viewModel.IB3Value, (byte)viewModel.IB4Value]));
            }
            else
            {
                Say("No Device Avaliable");
            }
        }

        private void OnStartClick(object sender, RoutedEventArgs e)
        {
            if (CoyoteDevice != null)
            {
                StartButton.IsEnabled = false;
                OneShotButton.IsEnabled = false;
                StopButton.IsEnabled = true;
                var viewModel = (SliderViewModel)DataContext;
                CoyoteDevice.WaveNow = new(
                    (byte)viewModel.SAValue,
                    (byte)viewModel.SBValue,
                    [(byte)viewModel.FA1Value, (byte)viewModel.FA2Value, (byte)viewModel.FA3Value, (byte)viewModel.FA4Value],
                    [(byte)viewModel.IA1Value, (byte)viewModel.IA2Value, (byte)viewModel.IA3Value, (byte)viewModel.IA4Value],
                    [(byte)viewModel.FB1Value, (byte)viewModel.FB2Value, (byte)viewModel.FB3Value, (byte)viewModel.FB4Value],
                    [(byte)viewModel.IB1Value, (byte)viewModel.IB2Value, (byte)viewModel.IB3Value, (byte)viewModel.IB4Value]);
                CoyoteDevice.Start();
            }
            else
            {
                Say("No Device Avaliable");
            }
        }

        private void OnStopClick(object sender, RoutedEventArgs e)
        {
            CoyoteDevice?.Stop();
            StartButton.IsEnabled = true;
            OneShotButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

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
            CoyoteDevice.NotificationReceived += (s, e) =>
            {
                var p = $"Debug N -- Notification received: {BitConverter.ToString(e)}";
                if (nbuf == p) return;
                nbuf = p;
                Say(p);
            };
            CoyoteDevice.B1MessageReceived += (s, e) =>
            {
                var p = $"通道输出信息: 序列号:{s}, 通道强度 A/B [{e[0]}]/[{e[1]}]";
                if (b1buf == p) return;
                b1buf = p;
                Say(p);
            };
        }

        private void OnOnlyAChecked(object sender, RoutedEventArgs e)
        {
            bool state = !(OnlyA?.IsChecked ?? false);
            iB1?.SetCurrentValue(Slider.ValueProperty, 0.0);
            iB2?.SetCurrentValue(Slider.ValueProperty, 0.0);
            iB3?.SetCurrentValue(Slider.ValueProperty, 0.0);
            iB4?.SetCurrentValue(Slider.ValueProperty, 0.0);
            if (iB1 != null) iB1.IsEnabled = state;
            if (iB2 != null) iB2.IsEnabled = state;
            if (iB3 != null) iB3.IsEnabled = state;
            if (iB4 != null) iB4.IsEnabled = state;
            fB1?.SetCurrentValue(Slider.ValueProperty, 0.0);
            fB2?.SetCurrentValue(Slider.ValueProperty, 0.0);
            fB3?.SetCurrentValue(Slider.ValueProperty, 0.0);
            fB4?.SetCurrentValue(Slider.ValueProperty, 0.0);
            if (fB1 != null) fB1.IsEnabled = state;
            if (fB2 != null) fB2.IsEnabled = state;
            if (fB3 != null) fB3.IsEnabled = state;
            if (fB4 != null) fB4.IsEnabled = state;
            SB?.SetCurrentValue(Slider.ValueProperty, 0.0);
            if (SB != null) SB.IsEnabled = state;
        }

        private void OnExportClick(object sender, RoutedEventArgs e)
        {
            var viewModel = (SliderViewModel)DataContext;
            var str = SB64.EncryptAndEncode(CoyoteDevice?.WaveNow ?? 
                new WaveformV3(
                    (byte)viewModel.SAValue, 
                    (byte)viewModel.SBValue,
                    [(byte)viewModel.FA1Value, (byte)viewModel.FA2Value, (byte)viewModel.FA3Value, (byte)viewModel.FA4Value],
                    [(byte)viewModel.IA1Value, (byte)viewModel.IA2Value, (byte)viewModel.IA3Value, (byte)viewModel.IA4Value],
                    [(byte)viewModel.FB1Value, (byte)viewModel.FB2Value, (byte)viewModel.FB3Value, (byte)viewModel.FB4Value],
                    [(byte)viewModel.IB1Value, (byte)viewModel.IB2Value, (byte)viewModel.IB3Value, (byte)viewModel.IB4Value]
                    ));
            Say($"波形哈希: -> {str}");
        }

        private void OnInputHashButtonClick(object sender, RoutedEventArgs e)
        {
            var inputWindow = new InputHashWindow();
            // 弹出输入窗口
            if (inputWindow.ShowDialog() == true)
            {
                string? encryptedHash = inputWindow.HashInput;

                if (!string.IsNullOrEmpty(encryptedHash))
                {
                    try
                    {
                        // 解密 WaveformV3
                        var decryptedWaveform = SB64.DecodeAndDecrypt<WaveformV3>(encryptedHash);

                        // 更新 SliderViewModel 的属性
                        var viewModel = (SliderViewModel)DataContext;
                        viewModel.SAValue = decryptedWaveform.StrengthA;
                        viewModel.SBValue = decryptedWaveform.StrengthB;
                        viewModel.FA1Value = decryptedWaveform.FrequencyA[0];
                        viewModel.FA2Value = decryptedWaveform.FrequencyA[1];
                        viewModel.FA3Value = decryptedWaveform.FrequencyA[2];
                        viewModel.FA4Value = decryptedWaveform.FrequencyA[3];
                        viewModel.IA1Value = decryptedWaveform.IntensityA[0];
                        viewModel.IA2Value = decryptedWaveform.IntensityA[1];
                        viewModel.IA3Value = decryptedWaveform.IntensityA[2];
                        viewModel.IA4Value = decryptedWaveform.IntensityA[3];
                        viewModel.FB1Value = decryptedWaveform.FrequencyB[0];
                        viewModel.FB2Value = decryptedWaveform.FrequencyB[1];
                        viewModel.FB3Value = decryptedWaveform.FrequencyB[2];
                        viewModel.FB4Value = decryptedWaveform.FrequencyB[3];
                        viewModel.IB1Value = decryptedWaveform.IntensityB[0];
                        viewModel.IB2Value = decryptedWaveform.IntensityB[1];
                        viewModel.IB3Value = decryptedWaveform.IntensityB[2];
                        viewModel.IB4Value = decryptedWaveform.IntensityB[3];
                    }
                    catch
                    {
                        MessageBox.Show("解密失败，请检查输入的哈希值是否正确。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}