using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImagingSIMS.Common
{
    public static class ExtensionMethods
    {
        public static void Save(this BitmapSource img, string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));

            using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        public static void Load(this BitmapSource img, string filePath)
        {
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();            
            bmp.UriSource = new Uri(filePath, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bmp.EndInit();

            img = bmp as BitmapSource;
        }
    }
}
