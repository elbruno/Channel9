using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ElBruno.WhatsThere
{
    public class FileActions
    {
        public static async void ClearTempFolder()
        {
            var files = await ApplicationData.Current.TemporaryFolder.GetFilesAsync();
            for (var i = 1; i < files.Count; i++)
            {
                await files[i].DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        public static async Task<StorageFile> CreateCopyOfSelectedImage(StorageFile file)
        {
            var newSourceFileName = $@"{Guid.NewGuid()}.jpg";
            var newSourceFile =
                await
                    ApplicationData.Current.TemporaryFolder.CreateFileAsync(newSourceFileName,
                        CreationCollisionOption.ReplaceExisting);
            await file.CopyAndReplaceAsync(newSourceFile);

            return newSourceFile;
        }

        public static async Task<Tuple<int, int>> GetImageInfoForRendering(string imageFilePath)
        {
            try
            {
                var sampleFile = await StorageFile.GetFileFromPathAsync(imageFilePath);
                var file = await sampleFile.OpenAsync(FileAccessMode.ReadWrite);
                var decoder = await BitmapDecoder.CreateAsync(file);
                var pixelWidth = int.Parse(decoder.PixelWidth.ToString());
                var pixelHeight = int.Parse(decoder.PixelHeight.ToString());
                return new Tuple<int, int>(pixelWidth, pixelHeight);
            }
            catch
            {
                return new Tuple<int, int>(0, 0);
            }
        }

        public static async Task<StorageFile> SelectImageWithFileOpenPicker()
        {
            var openPicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                ViewMode = PickerViewMode.Thumbnail
            };

            openPicker.FileTypeFilter.Clear();
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".jpg");

            var file = await openPicker.PickSingleFileAsync();
            return file;
        }

    }
}
