using DGLablib;
using DGLablib.PluginContracts;
using System.ComponentModel;
using System.Threading;

namespace UIHostCoyoteDevice
{
    public class PluginModel : INotifyPropertyChanged
    {
        private string? _name;
        public string? Name
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

        private IPlugin? _plugin;
        public IPlugin? Plugin
        {
            get => _plugin;
            set
            {
                _plugin = value;
                OnPropertyChanged(nameof(Plugin));
            }
        }

        public Task? PluginTask { get; set; }
        public CancellationTokenSource? CancellationTokenSource { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}