using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using Windows.UI.Xaml;

namespace Still.MAX31855
{
    public class Thermocouple
    {
        private SpiDevice _thermoCouple;
        private double _probeTemperature;
        private Faults _fault = Faults.Ok;
        private readonly int _chipSelect;
        private readonly string _controllerName;
        private const double Calibration = 0;
        public event PropertyChangedEventHandler TempChanged;
        public event PropertyChangedEventHandler StateChanged;
        private double _f;
        private bool _Connected = false;
        private ProbeState _ProbeState;

        public ProbeState State
        {
            get
            {
                return _ProbeState;
            }
            set
            {
                if (_ProbeState != value)
                {
                    _ProbeState = value;
                    NotifyStateChanged();
                }
            }
        }

        private void NotifyTempChanged([CallerMemberName] string propertyName = "")
        {
            TempChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NotifyStateChanged([CallerMemberName] string propertyName = "")
        {
            StateChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public Thermocouple(int spiChipSelectLine, Spi spiControllerName)
        {
            _chipSelect = spiChipSelectLine;
            _controllerName = spiControllerName == Spi.Spi0 ? "SPI0" : "SPI1";
            Init();

            var probeTimer = new DispatcherTimer();
            probeTimer.Tick += ProbeTimer_Tick;
            probeTimer.Interval = new TimeSpan(0, 0, 1);
            probeTimer.Start();
        }

        private void ProbeTimer_Tick(object sender, object e)
        {
            RefreshData();
        }

        public double InternalCelcius { get; private set; }

        public double Farenheit
        {
            get
            {
                if (double.IsNaN(_probeTemperature))
                {
                    return double.NaN;
                }
                var temp = _probeTemperature * 9.0 / 5.0 + 32 + Calibration;

                if (temp.Equals(_f))
                {
                    return temp;
                }

                //NotifyTempChanged();
                return _f = temp;
            }
        }

        private async void RefreshData()
        {
            try
            {
                if (_thermoCouple == null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));

                    if (State != ProbeState.DeviceNotFound)
                    {
                        System.Diagnostics.Debug.WriteLine("Probe Disconnected");
                    }

                    State = ProbeState.DeviceNotFound;
                    return;
                }
                else
                {
                    if (_Connected != true)
                    {
                        _Connected = true;
                    }
                }

                var raw = new byte[4];
                _thermoCouple.Read(raw);
                Array.Reverse(raw);
                var data = BitConverter.ToInt32(raw, 0);

                switch (data & 0x07)
                {
                    case 0x0:
                        State = ProbeState.Connected;
                        _fault = Faults.Ok;
                        break;
                    case 0x1:
                        _fault = Faults.OpenCircuit;

                        if (State != ProbeState.OpenCircuit)
                        {
                            System.Diagnostics.Debug.WriteLine("Open Circuit");
                        }

                        State = ProbeState.OpenCircuit;

                        if (_Connected != false)
                        {
                            _Connected = false;
                        }
                        _probeTemperature = 0;
                        NotifyTempChanged();
                        break;
                    case 0x2:
                        _fault = Faults.ShortToGnd;

                        if (State != ProbeState.GNDShort)
                        {
                            System.Diagnostics.Debug.WriteLine("GND Short");
                        }

                        State = ProbeState.GNDShort;
                        break;
                    case 0x4:
                        _fault = Faults.ShortToVcc;

                        if (State != ProbeState.VCCShort)
                        {
                            System.Diagnostics.Debug.WriteLine("VCC Short");
                        }

                        State = ProbeState.VCCShort;
                        break;
                    default:
                        _fault = Faults.GeneralFault;

                        if (State != ProbeState.Faulted)
                        {
                            System.Diagnostics.Debug.WriteLine("Faulted");
                        }

                        State = ProbeState.Faulted;
                        break;
                }

                if (_fault != Faults.Ok)
                {
                    _probeTemperature = double.NaN;
                    InternalCelcius = double.NaN;
                    return;
                }

                data >>= 4;
                InternalCelcius = (data & 0x7FF) * 0.0625;

                if ((data & 0x800) != 0)
                {
                    InternalCelcius += -128;
                }

                data >>= 14;
                _probeTemperature = (data & 0x1FFF) * 0.25;

                if (_probeTemperature == 0)
                {
                    State = ProbeState.OpenCircuit;
                }

                if ((data & 0x02000) != 0)
                {
                    _probeTemperature += -2048;
                }

                NotifyTempChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex.Message);
            }
        }

        private async void Init()
        {
            var spi = new SpiConnectionSettings(_chipSelect)
            {
                ClockFrequency = 5000000,
                Mode = SpiMode.Mode0
            };

            string spiAqs = SpiDevice.GetDeviceSelector(_controllerName);
            var devicesInfo = await DeviceInformation.FindAllAsync(spiAqs);

            if (devicesInfo.Count == 0)
            {
                return;
            }
            _thermoCouple = await SpiDevice.FromIdAsync(devicesInfo[0].Id, spi);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
