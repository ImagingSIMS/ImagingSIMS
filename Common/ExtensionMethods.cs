using System;
using System.Collections.Generic;
using System.IO;
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
            var extension = Path.GetExtension(filePath);
            BitmapEncoder encoder;

            switch (extension)
            {
                case ".bmp":
                    encoder = new BmpBitmapEncoder();
                    break;
                case ".png":
                    encoder = new PngBitmapEncoder();
                    break;
                case ".jpg":
                    encoder = new JpegBitmapEncoder();
                    break;
                case ".jpeg":
                    encoder = new JpegBitmapEncoder();
                    break;
                case ".tif":
                    encoder = new TiffBitmapEncoder();
                    break;
                case ".tiff":
                    encoder = new TiffBitmapEncoder();
                    break;
                default:
                    throw new ArgumentException("Unsupported file extension");
            }

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

        public static double[] ToDoubleArray(this float[] floatArray)
        {
            double[] a = new double[floatArray.Length];

            for (int i = 0; i < floatArray.Length; i++)
            {
                a[i] = (double)(float)floatArray.GetValue(i);
            }

            return a;
        }
        public static double[,] ToDoubleArray(this float[,] floatArray)
        {
            int length0 = floatArray.GetLength(0);
            int length1 = floatArray.GetLength(1);

            double[,] a = new double[length0, length1];

            for (int i = 0; i < length0; i++)
            {
                for (int j = 0; j < length1; j++)
                {
                    a[i, j] = (double)(float)floatArray.GetValue(i, j);
                }
            }

            return a;
        }
        public static float[] ToFloatArray(this double[] doubleArray)
        {
            float[] a = new float[doubleArray.Length];

            for (int i = 0; i < doubleArray.Length; i++)
            {
                a[i] = (float)(double)doubleArray.GetValue(i);
            }

            return a;
        }
        public static float[,] ToDoubleArray(this double[,] doubleArray)
        {
            int length0 = doubleArray.GetLength(0);
            int length1 = doubleArray.GetLength(1);

            float[,] a = new float[length0, length1];

            for (int i = 0; i < length0; i++)
            {
                for (int j = 0; j < length1; j++)
                {
                    a[i, j] = (float)(double)doubleArray.GetValue(i, j);
                }
            }

            return a;
        }
    }
}
