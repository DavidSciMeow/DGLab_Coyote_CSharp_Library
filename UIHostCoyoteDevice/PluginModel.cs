using DGLablib;
using DGLablib.PluginContracts;
using System.ComponentModel;
using System.Threading;

namespace UIHostCoyoteDevice
{
    public class PluginModel : INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private string? _description;
        public string? Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        private IPlugin _plugin;
        public IPlugin Plugin
        {
            get => _plugin;
            set
            {
                _plugin = value;
                OnPropertyChanged(nameof(Plugin));
            }
        }

        private Task? PluginTask { get; set; }
        public CancellationTokenSource? CancellationTokenSource { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public PluginModel(IPlugin plugin)
        {
            CancellationTokenSource = new CancellationTokenSource();
            _name = plugin.Name;
            _description = plugin.Description;
            _plugin = plugin;

            if (MainWindow.CoyoteDevice != null)
            {
                PluginTask = new Task(() => plugin.Init(MainWindow.CoyoteDevice, CancellationTokenSource.Token), CancellationTokenSource.Token);
            }
            else
            {
                PluginTask = null;
            }
        }

        public void Start()
        {
            if (MainWindow.CoyoteDevice != null)
            {
                if (PluginTask != null && !PluginTask.IsCompleted)
                {
                    PluginTask.Start();
                }
                else
                {
                    CancellationTokenSource = new CancellationTokenSource();
                    PluginTask = new Task(() => _plugin.Init(MainWindow.CoyoteDevice, CancellationTokenSource.Token), CancellationTokenSource.Token);
                }
            }
            else
            {
                throw new Exception("Error Start Coyote Device");
            }
        }
        public void Stop()
        {
            if (MainWindow.CoyoteDevice != null && CancellationTokenSource != null)
            {
                _plugin.Stop(MainWindow.CoyoteDevice, CancellationTokenSource.Token);

                if (PluginTask?.IsCanceled ?? true)
                {
                    return;
                }

                if (PluginTask != null && !PluginTask.IsCompleted)
                {
                    CancellationTokenSource.Cancel();
                    PluginTask.Wait();
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}