using Still.MAX31855;
using System;
using Windows.Devices.Gpio;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Still
{
    public sealed partial class MainPage : Page
    {
        private Thermocouple _Thermocouple;
        private static GpioPin _gpioPin;
        private static readonly int _fanPin = 17;

        public MainPage()
        {
            this.InitializeComponent();
            Init();
        }

        private async void Init()
        {
            ViewModel = new Readings()
            {
                Preset = 160
            };

            GpioController controller = await GpioController.GetDefaultAsync();
            if (controller == null)
            {
                return;
            }

            _gpioPin = controller.OpenPin(_fanPin);
            _gpioPin.SetDriveMode(GpioPinDriveMode.Output);

            _Thermocouple = new Thermocouple(0, Spi.Spi0);
            _Thermocouple.TempChanged += Thermocouple_TempChanged;
            _Thermocouple.StateChanged += Thermocouple_StateChanged;
        }

        private void Thermocouple_StateChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ViewModel.State = (sender as Thermocouple).State;
        }

        private void Thermocouple_TempChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            double temp = (sender as Thermocouple).Farenheit;
            if (double.IsNaN(temp))
            {
                return;
            }

            ViewModel.TempF = (sender as Thermocouple).Farenheit;

            if (temp < ViewModel.Preset)
            {
                _gpioPin.Write(GpioPinValue.High);
                ViewModel.HeaterStatus = HeaterStatus.On;
            }
            else
            {
                _gpioPin.Write(GpioPinValue.Low);
                ViewModel.HeaterStatus = HeaterStatus.Off;
            }

            if (temp < 75)
            {
                stillImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/0.png"));
            }
            else if (temp > 50 && temp < 75)
            {
                //25
                stillImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/25.png"));
            }
            else if (temp > 75 && temp < 125)
            {
                //50
                stillImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/50.png"));
            }
            else if (temp > 125 && temp < 160)
            {
                //75
                stillImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/75.png"));
            }
            else
            {
                //100
                stillImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/100.png"));
            }
        }

        public Readings ViewModel { get; set; }

        private void Preset_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            DisplaySubscribeDialog();
        }

        bool closeClicked = false;

        private async void DisplaySubscribeDialog()
        {
            ContentDialog subscribeDialog = new ContentDialog
            {
                Title = "Preset",
                Content = "Set the Preset",
                CloseButtonText = "Ok",
                PrimaryButtonText = "+",
                SecondaryButtonText = "-"
            };

            subscribeDialog.SecondaryButtonClick += SubscribeDialog_SecondaryButtonClick;
            subscribeDialog.PrimaryButtonClick += SubscribeDialog_PrimaryButtonClick;
            subscribeDialog.CloseButtonClick += SubscribeDialog_CloseButtonClick;
            subscribeDialog.Closing += SubscribeDialog_Closing;
            closeClicked = false;

            ContentDialogResult result = await subscribeDialog.ShowAsync();
        }

        private void SubscribeDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            closeClicked = true;
        }

        private void SubscribeDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (!closeClicked)
            {
                args.Cancel = true;
            }
        }

        private void SubscribeDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.Preset += 1;
        }

        private void SubscribeDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.Preset -= 1;
        }
    }
}