using System.ComponentModel;

namespace UIHostCoyoteDevice
{
    public class SliderViewModel : INotifyPropertyChanged
    {
        private double _saValue;
        private double _sbValue;
        private double _fa1Value;
        private double _fa2Value;
        private double _fa3Value;
        private double _fa4Value;
        private double _ia1Value;
        private double _ia2Value;
        private double _ia3Value;
        private double _ia4Value;
        private double _fb1Value;
        private double _fb2Value;
        private double _fb3Value;
        private double _fb4Value;
        private double _ib1Value;
        private double _ib2Value;
        private double _ib3Value;
        private double _ib4Value;

        private bool _isFreqALocked;
        private bool _isInstALocked;
        private bool _isFreqBLocked;
        private bool _isInstBLocked;

        public double SAValue
        {
            get => _saValue;
            set
            {
                _saValue = value;
                OnPropertyChanged(nameof(SAValue));
                if (IsInstALocked)
                {
                    IA1Value = value;
                    IA2Value = value;
                    IA3Value = value;
                    IA4Value = value;
                }
            }
        }

        public double SBValue
        {
            get => _sbValue;
            set
            {
                _sbValue = value;
                OnPropertyChanged(nameof(SBValue));
                if (IsInstBLocked)
                {
                    IB1Value = value;
                    IB2Value = value;
                    IB3Value = value;
                    IB4Value = value;
                }
            }
        }

        public double FA1Value
        {
            get => _fa1Value;
            set
            {
                _fa1Value = value;
                OnPropertyChanged(nameof(FA1Value));
                if (IsFreqALocked)
                {
                    FA2Value = value;
                    FA3Value = value;
                    FA4Value = value;
                }
            }
        }

        public double FA2Value
        {
            get => _fa2Value;
            set
            {
                _fa2Value = value;
                OnPropertyChanged(nameof(FA2Value));
            }
        }

        public double FA3Value
        {
            get => _fa3Value;
            set
            {
                _fa3Value = value;
                OnPropertyChanged(nameof(FA3Value));
            }
        }

        public double FA4Value
        {
            get => _fa4Value;
            set
            {
                _fa4Value = value;
                OnPropertyChanged(nameof(FA4Value));
            }
        }

        public double IA1Value
        {
            get => _ia1Value;
            set
            {
                _ia1Value = value;
                OnPropertyChanged(nameof(IA1Value));
            }
        }

        public double IA2Value
        {
            get => _ia2Value;
            set
            {
                _ia2Value = value;
                OnPropertyChanged(nameof(IA2Value));
            }
        }

        public double IA3Value
        {
            get => _ia3Value;
            set
            {
                _ia3Value = value;
                OnPropertyChanged(nameof(IA3Value));
            }
        }

        public double IA4Value
        {
            get => _ia4Value;
            set
            {
                _ia4Value = value;
                OnPropertyChanged(nameof(IA4Value));
            }
        }

        public double FB1Value
        {
            get => _fb1Value;
            set
            {
                _fb1Value = value;
                OnPropertyChanged(nameof(FB1Value));
                if (IsFreqBLocked)
                {
                    FB2Value = value;
                    FB3Value = value;
                    FB4Value = value;
                }
            }
        }

        public double FB2Value
        {
            get => _fb2Value;
            set
            {
                _fb2Value = value;
                OnPropertyChanged(nameof(FB2Value));
            }
        }

        public double FB3Value
        {
            get => _fb3Value;
            set
            {
                _fb3Value = value;
                OnPropertyChanged(nameof(FB3Value));
            }
        }

        public double FB4Value
        {
            get => _fb4Value;
            set
            {
                _fb4Value = value;
                OnPropertyChanged(nameof(FB4Value));
            }
        }

        public double IB1Value
        {
            get => _ib1Value;
            set
            {
                _ib1Value = value;
                OnPropertyChanged(nameof(IB1Value));
            }
        }

        public double IB2Value
        {
            get => _ib2Value;
            set
            {
                _ib2Value = value;
                OnPropertyChanged(nameof(IB2Value));
            }
        }

        public double IB3Value
        {
            get => _ib3Value;
            set
            {
                _ib3Value = value;
                OnPropertyChanged(nameof(IB3Value));
            }
        }

        public double IB4Value
        {
            get => _ib4Value;
            set
            {
                _ib4Value = value;
                OnPropertyChanged(nameof(IB4Value));
            }
        }

        public bool IsFreqALocked
        {
            get => _isFreqALocked;
            set
            {
                _isFreqALocked = value;
                OnPropertyChanged(nameof(IsFreqALocked));
            }
        }

        public bool IsInstALocked
        {
            get => _isInstALocked;
            set
            {
                _isInstALocked = value;
                OnPropertyChanged(nameof(IsInstALocked));
            }
        }

        public bool IsFreqBLocked
        {
            get => _isFreqBLocked;
            set
            {
                _isFreqBLocked = value;
                OnPropertyChanged(nameof(IsFreqBLocked));
            }
        }

        public bool IsInstBLocked
        {
            get => _isInstBLocked;
            set
            {
                _isInstBLocked = value;
                OnPropertyChanged(nameof(IsInstBLocked));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}