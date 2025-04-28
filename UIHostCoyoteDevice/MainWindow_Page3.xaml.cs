using DGLablib.PluginContracts;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows;
using System.Windows.Controls;


namespace UIHostCoyoteDevice
{
    public partial class MainWindow : Window
    {
        [ImportMany] public IEnumerable<IPlugin> Ipl { get; set; } = null!;
        public void LoadPlugins()
        {
            try
            {
                string pluginDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
                var catalog = new DirectoryCatalog(pluginDirectory);
                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);

                ViewModel.Plugins.Clear();
                Ipl = Ipl.GroupBy(plugin => plugin.Name).Select(group => group.First());

                foreach (var plugin in Ipl)
                {
                    if (plugin != null)
                    {
                        ViewModel.Plugins.Add(new PluginModel(plugin));
                    }
                }

                Say("插件扫描完成！");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"扫描插件时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void OnLoadPluginsClick(object sender, RoutedEventArgs e) => LoadPlugins();
        private void OnSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: PluginModel pluginModel } && pluginModel.Plugin != null)
            {
                // 检查插件是否有设置
                if (pluginModel.Plugin != null)
                {
                    // 打开设置窗口
                    var settingsWindow = new SettingsWindow(pluginModel.Plugin);
                    settingsWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show($"插件 {pluginModel.Name} 没有可配置的设置。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void OnPluginEnabledChanged(object sender, RoutedEventArgs e)
        {
            {
                if (sender is Button { Tag: PluginModel pluginModel } && pluginModel.Plugin != null)
                {
                    var settingsWindow = new SettingsWindow(pluginModel.Plugin);
                    settingsWindow.ShowDialog();
                }
            }
            { 
                if (sender is CheckBox { DataContext: PluginModel pluginModel })
                {
                    if (pluginModel.IsEnabled)
                    {
                        if (ViewModel.IsDeviceConnected)
                        {
                            ViewModel.ActivateThirdPage();
                            Say("第三页已启用，第二页已禁用。");
                        }
                        else
                        {
                            pluginModel.IsEnabled = false; // 回退状态
                            Say("设备未连接，无法启用第三页。");
                        }

                        try
                        {
                            pluginModel.Start();
                            Say($"插件 {pluginModel.Name} 已启用。");
                        }
                        catch (Exception ex)
                        {
                            Say($"启用插件 {pluginModel.Name} 时出错: {ex.Message}");
                            pluginModel.IsEnabled = false; // 回退状态
                        }
                    }
                    else
                    {
                        if (!ViewModel.Plugins.Any(p => p.IsEnabled))
                        {
                            ViewModel.ResetPages();
                            Say("所有插件已禁用，第二页已启用。");
                        }

                        try
                        {
                            pluginModel.Stop();
                            try
                            {
                                Say($"插件 {pluginModel.Name} 的任务已退出。");
                            }
                            catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException))
                            {
                                Say($"插件 {pluginModel.Name} 的任务已被取消。");
                            }
                            Say($"插件 {pluginModel.Name} 已禁用。");
                        }
                        catch (Exception ex)
                        {
                            Say($"禁用插件 {pluginModel.Name} 时出错: {ex.Message}");
                        }
                    }

                    // 更新 IsAnyPluginEnabled 状态
                    ViewModel.IsAnyPluginEnabled = ViewModel.Plugins.Any(p => p.IsEnabled);
                }
            }
        }
    }
}