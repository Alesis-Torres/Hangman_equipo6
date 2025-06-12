using System.IO;
using System.Windows.Media.Imaging;

namespace HangmanClient.Model.ViewModels
{
    public class WordViewModel
    {
        public string Name { get; set; }
        public byte[] ImageBytes { get; set; }

        public BitmapImage Image
        {
            get
            {
                if (ImageBytes == null || ImageBytes.Length == 0)
                    return null;

                using (var ms = new MemoryStream(ImageBytes))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    return image;
                }
            }
        }

        public WordViewModel(GameServiceReference.WordDTO dto)
        {
            Name = dto.Name;
            ImageBytes = dto.ImageBytes;
        }
    }
}
