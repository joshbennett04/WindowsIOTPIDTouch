using Still.MAX31855;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Still
{
    public class Readings : INotifyPropertyChanged
    {
        private double _TempF = 160;
        private double _Preset = 220;
        private HeaterStatus _HeaterStatus = HeaterStatus.Off;
        private ProbeState _State = ProbeState.Disconnected;
        private string _TempString;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public string TempString
        {
            get => _TempString;
            set
            {
                _TempString = value;
                OnPropertyChanged();
            }
        }

        public double TempF
        {
            get => _TempF;
            set
            {
                TempString = $"{Math.Round(_TempF)}";
                _TempF = value;
                OnPropertyChanged();
            }
        }
        public double Preset
        {
            get => _Preset;
            set
            {
                _Preset = value;
                OnPropertyChanged();
            }
        }

        public HeaterStatus HeaterStatus
        {
            get => _HeaterStatus;
            set
            {
                _HeaterStatus = value;
                OnPropertyChanged();
            }
        }

        public ProbeState State
        {
            get => _State;
            set
            {
                _State = value;
                OnPropertyChanged();
            }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
