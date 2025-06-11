using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;

namespace HangmanClient.Model.DTO
{
    [DataContract]
    public class WordDTO
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
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
    }
}