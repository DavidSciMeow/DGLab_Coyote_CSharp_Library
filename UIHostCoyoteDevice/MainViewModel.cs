using System.Collections.ObjectModel;
using System.ComponentModel;


namespace UIHostCoyoteDevice
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<PluginModel> Plugins { get; set; } = [];
        public SliderViewModel SliderViewModel { get; set; } = new SliderViewModel();
        private bool _isDeviceConnected;
        public bool IsDeviceConnected
        {
            get => _isDeviceConnected;
            set
            {
                if (_isDeviceConnected != value)
                {
                    _isDeviceConnected = value;
                    OnPropertyChanged(nameof(IsDeviceConnected));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}