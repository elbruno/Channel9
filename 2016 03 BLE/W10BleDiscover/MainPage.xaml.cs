using System;
using System.Diagnostics;
using Windows.Devices.Enumeration;

namespace W10BleDiscover
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var bts = await DeviceInformation.FindAllAsync();
            var i = 0;
            foreach (var di in bts)
            {
                i++;
                Debug.WriteLine("{0} found. {1}", i, di.Name);
                Debug.WriteLine("{0} - {1}{2}", i.ToString("0000"), di.Id, Environment.NewLine);
            }


        }
    }
}
