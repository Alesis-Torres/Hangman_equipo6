using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HangmanClient.Model.DTO
{
    [DataContract]
    public class CategoryDTO
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public byte[] ImageBytes { get; set; }

        public BitmapImage Image
        {
            get
            {
                try
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
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error creando imagen: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
