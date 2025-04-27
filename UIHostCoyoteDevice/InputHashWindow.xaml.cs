using System.Windows;

namespace UIHostCoyoteDevice
{
    /// <summary>
    /// Interaction logic for InputHashWindow.xaml
    /// </summary>
    public partial class InputHashWindow : Window
    {
        public string? HashInput { get; private set; }

        public InputHashWindow()
        {
            InitializeComponent();
        }

        private void OnConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            HashInput = HashInputTextBox.Text;
            DialogResult = true; // 关闭窗口并返回结果
        }
    }
}
