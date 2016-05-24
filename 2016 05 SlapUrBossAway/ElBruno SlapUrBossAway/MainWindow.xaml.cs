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
        private LyncClient _client;
        private ElBrunoListener _listener;
        private Controller _controller;
        private bool _swipeDetected;

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
            InitSlapMedia();
            BossEmail = "your boss email here @ email.com";
            _timer.Elapsed += TimerElapsed;
            _timer.Enabled = true;
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer.Elapsed -= TimerElapsed;
            _timer.Stop();
        }

        private void InitSlapMedia()
        {
            var slapGif = AppDomain.CurrentDomain.BaseDirectory + @"\BarneySlapBet.gif";
            MediaAnimagedGif.Source = new Uri(slapGif, UriKind.RelativeOrAbsolute);
            MediaAnimagedGif.MediaEnded += MediaAnimagedGif_MediaEnded;
        }
        private void MediaAnimagedGif_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaAnimagedGif.Visibility = Visibility.Collapsed;
        }

        void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(CheckLyncState));
        }
        void CheckLyncState()
        {
            foreach (var conversation in _client.ConversationManager.Conversations)
            {
                foreach (var participant in conversation.Participants)
                {
                    if (!participant.Contact.Uri.ToLower().Contains(BossEmail.ToLower()) || !_swipeDetected) continue;
                    conversation.End();
                    _swipeDetected = false;
                    MediaAnimagedGif.Visibility = Visibility.Visible;
                    MediaAnimagedGif.Position = TimeSpan.Zero;
                    MediaAnimagedGif.Play();
                    Debug.WriteLine("Boss slapped !!!");
                    return;
                }
            }

        }

        private void InitLeapMotion()
        {
            _listener = new ElBrunoListener();
            _controller = new Controller();
            _controller.AddListener(_listener);
            _listener.OnSwipeDetected += _listener_OnSwipeDetected;

        }

        private void _listener_OnSwipeDetected(bool swipeDetected)
        {
            _swipeDetected = swipeDetected;
            Debug.WriteLine($"Swipe Detected: {_swipeDetected}");
        }

        private void InitLyncClient()
        {
            try
            {
                _client = LyncClient.GetClient();
            }
            catch
            {
                MessageBox.Show("Microsoft Skype for Bussines does not appear to be running. Please start S4B.");
            }
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
