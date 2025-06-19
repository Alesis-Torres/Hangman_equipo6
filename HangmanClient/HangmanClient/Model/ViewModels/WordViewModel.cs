using GameServiceReference;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HangmanClient.Model.ViewModels
{
    public class WordViewModel
    {
        public string Name { get; set; }
        public byte[] ImageBytes { get; set; }

        public WordViewModel(WordDTO dto)
        {
            Name = dto.Name;
            ImageBytes = dto.ImageBytes;
        }

        public ImageSource Image
        {
            get
            {
                if (ImageBytes == null || ImageBytes.Length == 0)
                    return null;

                try
                {
                    using (var ms = new MemoryStream(ImageBytes))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = ms;
                        image.EndInit();
                        image.Freeze();
                        return image;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}