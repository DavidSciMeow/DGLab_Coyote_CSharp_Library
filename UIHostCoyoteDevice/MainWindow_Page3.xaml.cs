using DGLablib.PluginContracts;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace UIHostCoyoteDevice
{
    public partial class MainWindow : Window
    {
        [ImportMany] public IEnumerable<IPlugin> _ipl { get; set; } = null!;
        private void OnLoadPluginsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string pluginDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
                var catalog = new DirectoryCatalog(pluginDirectory);
                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);
                ViewModel.Plugins.Clear();
                _ipl = [.. _ipl.GroupBy(plugin => plugin.Name).Select(group => group.First())];
                foreach (var plugin in _ipl)
                {
                    if (plugin != null && CoyoteDevice != null)
                    {
                        var cancellationTokenSource = new CancellationTokenSource();
                        var t = new PluginModel
                        {
                            Name = plugin.Name,
                            Description = plugin.Description,
                            IsEnabled = false,
                            PluginTask = new Task(() => //挂载用户回调
                            {
                                plugin.Say = (str) => Say(str);
                                plugin.SetWave = (waveform) => CoyoteDevice.WaveNow = waveform;
                                CoyoteDevice?.Start();
                                plugin.Init(cancellationTokenSource.Token);
                            }, cancellationTokenSource.Token),
                            CancellationTokenSource = cancellationTokenSource
                        };
                        ViewModel.Plugins.Add(t);
                    }
                }
                MessageBox.Show("插件扫描完成！");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"扫描插件时出错: {ex.Message}");
            }
        }
        private void OnPluginEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox { DataContext: PluginModel pluginModel })
            {
                if (pluginModel.IsEnabled)
                {
                    try
                    {
                        pluginModel.PluginTask?.Start();
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
                    try
                    {
                        pluginModel.CancellationTokenSource?.Cancel();
                        if (CoyoteDevice != null) CoyoteDevice.WaveNow = new DGLablib.WaveformV3();
                        try
                        {
                            pluginModel.PluginTask?.Wait(); // 等待任务完成或取消
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
            }
        }
    }
}