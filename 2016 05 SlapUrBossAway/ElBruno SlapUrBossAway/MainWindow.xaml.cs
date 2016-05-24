using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using ElBruno.SlapUrBossAway.Annotations;
using Leap;
using Microsoft.Lync.Model;

namespace ElBruno.SlapUrBossAway
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly Timer _timer = new Timer(1000);
        private string _bossEmail;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindowLoaded;
            Unloaded += MainWindow_Unloaded;
            DataContext = this;
        }

        void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            InitLyncClient();
            InitLeapMotion();
            BossEmail = "brunocapuano@msn.com";
            _timer.Elapsed += TimerElapsed;
            _timer.Enabled = true;
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer.Elapsed -= TimerElapsed;
            _timer.Stop();
        }

        void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(CheckLyncState));
        }
        void CheckLyncState()
        {
        }

        private void InitLeapMotion()
        {
        }

        private void InitLyncClient()
        {
        }

        #region Notify PropertyChanged and Properties

        public string BossEmail
        {
            get { return _bossEmail; }
            set
            {
                if (value == _bossEmail) return;
                _bossEmail = value;
                OnPropertyChanged(nameof(BossEmail));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion  
    }
}
