using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using ImagingSIMS.Data.Imaging;

namespace ImagingSIMS.Data
{
    public static class ExtensionMethods
    {
        public static BitmapSource Crop(this BitmapSource src, int startX, int startY, int width, int height)
        {
            int originalWidth = (int)src.PixelWidth;
            int originalHeight = (int)src.PixelHeight;

            if (width > originalWidth || height > originalHeight)
            {
                throw new ArgumentException("Invalid dimensions. Cannot make image bigger with Crop function.");
            }

            if (width + startX > originalWidth || height + startY > originalHeight)
            {
                throw new ArgumentException("Invalid crop size. Crop falls outside the bounds of the original image.");
            }

            PixelFormat pf = src.Format;
            int originalStride = (originalWidth * pf.BitsPerPixel) / 8;
            int byteSize = originalHeight * originalStride;
            byte[] pixels = new byte[byteSize];
            src.CopyPixels(pixels, originalStride, 0);

            Data3D originalData = new Data3D(originalWidth, originalHeight, 4);
            int pos = 0;
            for (int y = 0; y < originalHeight; y++)
            {
                for (int x = 0; x < originalWidth; x++)
                {
                    // B
                    originalData[x, y, 0] = pixels[pos + 0];
                    // G
                    originalData[x, y, 1] = pixels[pos + 1];
                    // R
                    originalData[x, y, 2] = pixels[pos + 2];                    

                    if (pf == PixelFormats.Bgr24)
                    {
                        pos += 3;
                    }
                    else if (pf == PixelFormats.Bgr32)
                    {
                        pos += 4;
                    }
                    else if (pf == PixelFormats.Bgra32)
                    {
                        // A
                        originalData[x, y, 3] = pixels[pos + 3];
                        pos += 4;
                    }
                }
            }

            Data3D cropped = originalData.Crop(startX, startY, width, height);

            return ImageHelper.CreateImage(cropped);
        }

        public static float[] ToFloatArray(this Color c)
        {
            return new float[4] { c.B, c.G, c.R, c.A };
        }
    }
}
