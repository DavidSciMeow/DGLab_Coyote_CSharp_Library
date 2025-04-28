using DGLablib.PluginContracts;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace UIHostCoyoteDevice
{
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly Dictionary<string, string> _settings;
        public SettingsWindow(IPlugin plugin)
        {
            InitializeComponent();
            _settings = plugin.Settings;
            DataContext = this;
            GenerateSettingsUI();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void GenerateSettingsUI()
        {
            foreach (var kvp in _settings)
            {
                // 创建一个 StackPanel 用于每个设置项
                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };

                // 创建 Label 显示 Key
                var label = new Label { Content = kvp.Key, Width = 150, VerticalAlignment = VerticalAlignment.Center };

                // 根据 Value 类型动态生成控件
                FrameworkElement inputControl = kvp.Value switch
                {
                    string => new TextBox { Text = kvp.Value.ToString(), Width = 200, Tag = kvp.Key },
                    //int => new TextBox { Text = kvp.Value.ToString(), Width = 200, Tag = kvp.Key },
                    //bool => new CheckBox { IsChecked = (bool)kvp.Value, Tag = kvp.Key },
                    _ => new TextBox { Text = kvp.Value?.ToString() ?? string.Empty, Width = 200, Tag = kvp.Key }
                };

                // 监听控件的更改事件
                if (inputControl is TextBox textBox)
                {
                    textBox.TextChanged += OnSettingChanged;
                }
                else if (inputControl is CheckBox checkBox)
                {
                    checkBox.Checked += OnSettingChanged;
                    checkBox.Unchecked += OnSettingChanged;
                }

                // 添加控件到 StackPanel
                stackPanel.Children.Add(label);
                stackPanel.Children.Add(inputControl);

                // 添加 StackPanel 到主布局
                SettingsPanel.Children.Add(stackPanel);
            }
        }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // 自动保存逻辑
            if (sender is TextBox textBox && textBox.DataContext is KeyValuePair<string, object> setting)
            {
                _settings[setting.Key] = textBox.Text;
                MessageBox.Show($"设置 {setting.Key} 已更新为 {textBox.Text}！");
            }
        }
        private void OnSettingChanged(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string key)
            {
                if (element is TextBox textBox)
                {
                    _settings[key] = textBox.Text; // 更新 Settings 中的值
                }
                //else if (element is CheckBox checkBox)
                //{
                //    _settings[key] = checkBox.IsChecked ?? false; // 更新 Settings 中的值
                //}
            }
        }
    }
}