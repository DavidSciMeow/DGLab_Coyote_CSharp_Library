using DGLablib.PluginContracts;
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
    public partial class SettingsWindow : Window
    {
        private readonly PluginBase Plg;
        public SettingsWindow(PluginBase plg)
        {
            InitializeComponent();
            Plg = plg;
            // 动态生成控件
            foreach (var setting in Plg.Settings)
            {
                var label = new Label { Content = setting.Key, Margin = new Thickness(0, 5, 0, 0) };

                FrameworkElement inputControl;

                // 根据类型生成控件
                switch (setting.Value)
                {
                    case int _:
                        inputControl = CreateTextBox<int>(setting, int.TryParse);
                        break;
                    case double _:
                        inputControl = CreateTextBox<double>(setting, double.TryParse);
                        break;
                    case bool boolValue:
                        inputControl = CreateCheckBox(setting, boolValue);
                        break;
                    case string _:
                        inputControl = CreateTextBox(setting, (string text, out string result) =>
                        {
                            result = text;
                            return true;
                        });
                        break;
                    default:
                        MessageBox.Show($"不支持的设置类型: {setting.Value.GetType()}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                }

                SettingsPanel.Children.Add(label);
                SettingsPanel.Children.Add(inputControl);
            }
        }

        private TextBox CreateTextBox<T>(KeyValuePair<string, object> setting, TryParseHandler<T> tryParse)
        {
            var textBox = new TextBox
            {
                Text = setting.Value.ToString(),
                Margin = new Thickness(0, 0, 0, 10)
            };

            textBox.LostFocus += (s, e) =>
            {
                if (tryParse(textBox?.Text ?? "", out var newValue))
                {
                    Plg.UpdateSetting(setting.Key, newValue!);
                }
                else
                {
                    MessageBox.Show($"无效的值: {textBox.Text}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    textBox.Text = setting.Value.ToString(); // 恢复原值
                }
            };

            return textBox;
        }

        private CheckBox CreateCheckBox(KeyValuePair<string, object> setting, bool initialValue)
        {
            var checkBox = new CheckBox
            {
                IsChecked = initialValue,
                Margin = new Thickness(0, 0, 0, 10)
            };

            checkBox.Checked += (s, e) => Plg.UpdateSetting(setting.Key, true);
            checkBox.Unchecked += (s, e) => Plg.UpdateSetting(setting.Key, false);

            return checkBox;
        }

        private delegate bool TryParseHandler<T>(string input, out T result);
    }
}
