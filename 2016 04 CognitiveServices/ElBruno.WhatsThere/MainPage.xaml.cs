using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using ElBruno.WhatsThere.Annotations;
using ElBruno.WhatsThere.CognitiveServices;
using Microsoft.ProjectOxford.Vision;

namespace ElBruno.WhatsThere
{
    public sealed partial class MainPage : INotifyPropertyChanged
    {
        private string _analysisResult;
        private readonly SpeechSynthesizer _synth;
        private string _versionInformation;

        public MainPage()
        {
            InitializeComponent();
            DataContext = this;
            _synth = new SpeechSynthesizer();

            var version = Package.Current.Id.Version;
            VersionInformation = $"{Package.Current.DisplayName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private async void ButtonAnalyze_Click(object sender, RoutedEventArgs e)
        {
            var photoFile = await TakeWebCamPictureAndReturnFile(true);
            AnalysisResult = await PerformImageAnalysisAsync(photoFile);

            try
            {
                var synthesisStream = await _synth.SynthesizeTextToStreamAsync(AnalysisResult);
                MediaElementSpeech.AutoPlay = true;
                MediaElementSpeech.SetSource(synthesisStream, synthesisStream.ContentType);
                MediaElementSpeech.Play();
            }
            catch
            {
                // ignored
            }
        }

        private async Task<string> PerformImageAnalysisAsync(StorageFile file)
        {
            string desc = "I can't process this image";
            try
            {
                var imageInfo = await FileActions.GetImageInfoForRendering(file.Path);
                NewImageSizeWidth = 100;
                NewImageSizeHeight = NewImageSizeWidth * imageInfo.Item2 / imageInfo.Item1;
                var newSourceFile = await FileActions.CreateCopyOfSelectedImage(file);
                var uriSource = new Uri(newSourceFile.Path);
                SelectedFileBitmapImage = new BitmapImage(uriSource);

                var visionServiceClient = new VisionServiceClient(Keys.Vision);
                var buffer = await FileIO.ReadBufferAsync(newSourceFile);
                using (var stream = buffer.AsStream())
                {
                    var analysisResult = await visionServiceClient.DescribeAsync(stream);
                    if (analysisResult != null && analysisResult.Description.Captions.Length > 0)
                        desc = analysisResult.Description.Captions[0].Text;
                }
            }
            catch
            {
                // ignored
            }
            return desc;

        }

        public async Task<StorageFile> TakeWebCamPictureAndReturnFile(bool takeSilentPicture)
        {
            StorageFile file;
            if (takeSilentPicture)
            {
                var takePhotoManager = new MediaCapture();
                await takePhotoManager.InitializeAsync();
                var imgFormat = ImageEncodingProperties.CreateJpeg();
                file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("CameraPhoto.jpg", CreationCollisionOption.ReplaceExisting);
                await takePhotoManager.CapturePhotoToStorageFileAsync(imgFormat, file);
            }
            else
            {
                var dialog = new CameraCaptureUI();
                dialog.PhotoSettings.AllowCropping = false;
                dialog.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.HighestAvailable;
                file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Photo);
            }
            return file;
        }

    #region Property Management

    public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string AnalysisResult
        {
            get { return _analysisResult; }
            set
            {
                if (value == _analysisResult) return;
                _analysisResult = value;
                OnPropertyChanged();
            }
        }

        public string VersionInformation
        {
            get { return _versionInformation; }
            set
            {
                if (value == _versionInformation) return;
                _versionInformation = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage _selectedFileBitmapImage;
        public BitmapImage SelectedFileBitmapImage
        {
            get
            {
                return _selectedFileBitmapImage;
            }
            set
            {
                if (value == _selectedFileBitmapImage) return;
                _selectedFileBitmapImage = value;
                OnPropertyChanged();
            }
        }

        private int _newImageSizeWidth;
        public int NewImageSizeWidth
        {
            get { return _newImageSizeWidth; }
            set
            {
                if (value == _newImageSizeWidth) return;
                _newImageSizeWidth = value;
                OnPropertyChanged();
            }
        }
        private int _newImageSizeHeight;
        public int NewImageSizeHeight
        {
            get { return _newImageSizeHeight; }
            set
            {
                if (value == _newImageSizeHeight) return;
                _newImageSizeHeight = value;
                OnPropertyChanged();
            }
        }
        #endregion
    }
}