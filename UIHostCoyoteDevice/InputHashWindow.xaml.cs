using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
