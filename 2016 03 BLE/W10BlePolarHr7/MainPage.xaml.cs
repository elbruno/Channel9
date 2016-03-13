using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using Windows.UI.Core;
using W10BlePolarHr7.Annotations;

namespace W10BlePolarHr7
{
    public sealed partial class MainPage : INotifyPropertyChanged
    {
        private DeviceInformation _devicePolarHr;
        private DeviceInformation _devicePolarBattery;

        public MainPage()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await GetHrAndBatteryDevice();
        }

        private async Task<bool> GetHrAndBatteryDevice()
        {
            StatusInformation = "Start search for devices, Battery";
            var devices = await DeviceInformation.FindAllAsync(
                GattDeviceService.GetDeviceSelectorFromUuid(GattServiceUuids.Battery));
            if (null == devices || devices.Count <= 0) return true;
            foreach (var device in devices.Where(device => device.Name == "Polar H7 498C1817"))
            {
                _devicePolarBattery = device;
                StatusInformation2 = "Found battery device";
                await DisplayBatteryLevel();
                break;
            }

            StatusInformation = "Start search for devices, HR";
            devices = await DeviceInformation.FindAllAsync(
                GattDeviceService.GetDeviceSelectorFromUuid(GattServiceUuids.HeartRate));
            if (null == devices || devices.Count <= 0) return true;
            foreach (var device in devices.Where(device => device.Name == "Polar H7 498C1817"))
            {
                _devicePolarHr = device;
                StatusInformation2 = "Found hr device";
                await SuscribeToHrValues();
                break;
            }

            StatusInformation = $"Found HR [{(_devicePolarHr != null)}] Battery [{(_devicePolarBattery != null)}]";
            return (_devicePolarHr != null && _devicePolarBattery != null);
        }

        private async Task DisplayBatteryLevel()
        {
            var service = await GattDeviceService.FromIdAsync(_devicePolarBattery.Id);
            var characteristics = service?.GetAllCharacteristics();
            if (characteristics == null || characteristics.Count <= 0) return;
            var characteristic = characteristics[0];
            try
            {
                var batteryLevelValue = await characteristic.ReadValueAsync();
                var arrayLenght = (int)batteryLevelValue.Value.Length;
                var batteryData = new byte[arrayLenght];
                DataReader.FromBuffer(batteryLevelValue.Value).ReadBytes(batteryData);

                // battery level is on first element
                var batteryLevel = batteryData[0];
                Debug.WriteLine(batteryLevel);
                BatteryValue = $"{batteryLevel} %";
            }
            catch (Exception exception)
            {
                StatusInformation2 = exception.ToString();
            }
        }

        private async Task SuscribeToHrValues()
        {
            var service = await GattDeviceService.FromIdAsync(_devicePolarHr.Id);
            if (null == service) return;

            var characteristics = service.GetAllCharacteristics();
            if (null == characteristics || characteristics.Count <= 0) return;
            foreach (var characteristic in characteristics)
            {
                try
                {
                    characteristic.ValueChanged += GattCharacteristic_ValueChanged;
                    await
                        characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.Notify);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
            }
        }

        private void GattCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            if (args == null) return;
            if (args.CharacteristicValue.Length == 0) return;

            var arrayLenght = (int)args.CharacteristicValue.Length;
            var hrData = new byte[arrayLenght];
            DataReader.FromBuffer(args.CharacteristicValue).ReadBytes(hrData);

            //Convert to string  
            var hrValue = ProcessData(hrData);
            Debug.WriteLine(hrValue);
            HeartRateValue = hrValue.ToString();

        }

        private int ProcessData(byte[] data)
        {
            // Heart Rate profile defined flag values
            const byte HEART_RATE_VALUE_FORMAT = 0x01;
            const byte ENERGY_EXPANDED_STATUS = 0x08;

            byte currentOffset = 0;
            byte flags = data[currentOffset];
            bool isHeartRateValueSizeLong = ((flags & HEART_RATE_VALUE_FORMAT) != 0);

            currentOffset++;

            ushort heartRateMeasurementValue = 0;

            if (isHeartRateValueSizeLong)
            {
                heartRateMeasurementValue = (ushort)((data[currentOffset + 1] << 8) + data[currentOffset]);
                currentOffset += 2;
            }
            else
            {
                heartRateMeasurementValue = data[currentOffset];
            }

            return heartRateMeasurementValue;
        }


        #region Properties and Property Changed

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            });
        }

        private string _statusInformation;
        public string StatusInformation
        {
            get { return _statusInformation; }
            set
            {
                if (value == _statusInformation) return;
                _statusInformation = value;
                OnPropertyChanged();
            }
        }

        private string _statusInformation2;

        public string StatusInformation2
        {
            get { return _statusInformation2; }
            set
            {
                if (value == _statusInformation2) return;
                _statusInformation2 = value;
                OnPropertyChanged();
            }
        }

        private string _heartRateValue;

        public string HeartRateValue
        {
            get { return _heartRateValue; }
            set
            {
                if (value == _heartRateValue) return;
                _heartRateValue = value;
                OnPropertyChanged();
            }
        }

        private string _batteryValue;

        public string BatteryValue
        {
            get { return _batteryValue; }
            set
            {
                if (value == _batteryValue) return;
                _batteryValue = value;
                OnPropertyChanged();
            }
        }
        #endregion

    }
}
