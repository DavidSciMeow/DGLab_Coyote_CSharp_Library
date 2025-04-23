using System.Collections.ObjectModel;
using System.ComponentModel;


namespace UIHostCoyoteDevice
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _isAnyPluginEnabled;
        private bool _isDeviceConnected;
        private bool _isSecondPageActive; // 第二页是否处于活动状态
        private bool _isThirdPageActive;  // 第三页是否处于活动状态

        public ObservableCollection<PluginModel> Plugins { get; set; } = new();
        public SliderViewModel SliderViewModel { get; set; } = new SliderViewModel();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public bool IsDeviceConnected
        {
            get => _isDeviceConnected;
            set
            {
                if (_isDeviceConnected != value)
                {
                    _isDeviceConnected = value;
                    OnPropertyChanged(nameof(IsDeviceConnected));
                    UpdatePageStates();
                }
            }
        }

        public bool IsAnyPluginEnabled
        {
            get => _isAnyPluginEnabled;
            set
            {
                if (_isAnyPluginEnabled != value)
                {
                    _isAnyPluginEnabled = value;
                    OnPropertyChanged(nameof(IsAnyPluginEnabled));
                    UpdatePageStates();
                }
            }
        }

        public bool IsSecondPageEnabled => IsDeviceConnected && (!_isThirdPageActive || !_isAnyPluginEnabled);
        public bool IsThirdPageEnabled => IsDeviceConnected && (!_isSecondPageActive);

        public void ActivateSecondPage()
        {
            _isSecondPageActive = true;
            _isThirdPageActive = false;
            OnPropertyChanged(nameof(IsSecondPageEnabled));
            OnPropertyChanged(nameof(IsThirdPageEnabled));
        }

        public void ActivateThirdPage()
        {
            _isSecondPageActive = false;
            _isThirdPageActive = true;
            OnPropertyChanged(nameof(IsSecondPageEnabled));
            OnPropertyChanged(nameof(IsThirdPageEnabled));
        }

        public void ResetPages()
        {
            _isSecondPageActive = false;
            _isThirdPageActive = false;
            OnPropertyChanged(nameof(IsSecondPageEnabled));
            OnPropertyChanged(nameof(IsThirdPageEnabled));
        }

        private void UpdatePageStates()
        {
            OnPropertyChanged(nameof(IsSecondPageEnabled));
            OnPropertyChanged(nameof(IsThirdPageEnabled));
        }
    }
}