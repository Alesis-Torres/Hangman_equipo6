using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HangmanClient.Model.ViewModels
{
    class CategoryViewModel
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

        public CategoryViewModel(GameServiceReference.CategoryDTO dto)
        {
            Name = dto.Name;
            ImageBytes = dto.ImageBytes;
        }
    }
}
