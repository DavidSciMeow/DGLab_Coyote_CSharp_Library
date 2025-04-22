using DGLablib;
using System.Windows;


namespace UIHostCoyoteDevice
{
    public partial class MainWindow : Window
    {
        CoyoteDeviceV3? CoyoteDevice;
        public MainViewModel ViewModel { get; set; } = new MainViewModel();
        public MainWindow()
        {
            InitializeComponent();
            StopButton.IsEnabled = false;
            DataContext = ViewModel;
        }
        private async void Say(string s)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                OutputTextBox.Text += $"{s}\n";
                OutputTextBox.ScrollToEnd();
            });
        }
    }
}