using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using ImagingSIMS.Data.Imaging;
using ImagingSIMS.Data.Rendering;

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

            return ImageGenerator.Instance.Create(cropped);
        }

        public static float[] ToFloatArray(this Color c)
        {
            return new float[4] { c.B, c.G, c.R, c.A };
        }

        public static bool EnsureDimensions(this IEnumerable<Data2D> collection)
        {
            int width = 0;
            int height = 0;

            foreach (var d in collection)
            {
                if (width == 0)
                    width = d.Width;
                if (height == 0)
                    height = d.Height;

                if (width != d.Width || height != d.Height)
                    return false;
            }

            return true;
        }
        public static bool EnsureDimensions(this IEnumerable<Data2D> collection, 
            out int width, out int height)
        {
            width = 0;
            height = 0;

            foreach (var d in collection)
            {
                if (width == 0)
                    width = d.Width;
                if (height == 0)
                    height = d.Height;

                if (width != d.Width || height != d.Height)
                    return false;
            }

            return true;
        }
        public static bool EnsureDimensions(this IEnumerable<Data3D> collection)
        {
            int width = 0;
            int height = 0;
            int depth = 0;

            foreach (var d in collection)
            {
                if (width == 0)
                    width = d.Width;
                if (height == 0)
                    height = d.Height;
                if (depth == 0)
                    depth = d.Depth;

                if (width != d.Width || height != d.Height ||  depth != d.Depth)
                    return false;
            }

            return true;
        }
        public static bool EnsureDimensions(this IEnumerable<Data3D> collection, 
            out int width, out int height, out int depth)
        {
            width = 0;
            height = 0;
            depth = 0;

            foreach (var d in collection)
            {
                if (width == 0)
                    width = d.Width;
                if (height == 0)
                    height = d.Height;
                if (depth == 0)
                    depth = d.Depth;

                if (width != d.Width || height != d.Height || depth != d.Depth)
                    return false;
            }

            return true;
        }
        public static bool EnsureDimensions(this IEnumerable<Volume> collection)
        {
            int width = 0;
            int height = 0;
            int depth = 0;

            foreach (var v in collection)
            {
                if (width == 0)
                    width = v.Width;
                if (height == 0)
                    height = v.Height;
                if (depth == 0)
                    depth = v.Depth;

                if (width != v.Width || height != v.Height || depth != v.Depth)
                    return false;
            }

            return true;
        }
        public static bool EnsureDimensions(this IEnumerable<Volume> collection,
            out int width, out int height, out int depth)
        {
            width = 0;
            height = 0;
            depth = 0;

            foreach (var v in collection)
            {
                if (width == 0)
                    width = v.Width;
                if (height == 0)
                    height = v.Height;
                if (depth == 0)
                    depth = v.Depth;

                if (width != v.Width || height != v.Height || depth != v.Depth)
                    return false;
            }

            return true;
        }
    }
    
    public static class MatrixExtensionMethods
    {
        /// <summary>
        /// Upscales the 2D doule matrix to the specified dimensions. Can not be used to downsample a matrix.
        /// </summary>
        /// <param name="matrix">2D double matrix to upscale.</param>
        /// <param name="targetWidth">Upscaled width.</param>
        /// <param name="targetHeight">Upscaled height.</param>
        /// <param name="useBilinear">True to use bilinear interpolation, false to use bicubic interpolation.</param>
        /// <returns>2D double matrix with the target dimensions.</returns>
        public static double[,] Upscale(this double[,] matrix, int targetWidth, int targetHeight, bool useBilinear = true)
        {
            int lowResSizeX = matrix.GetLength(0);
            int lowResSizeY = matrix.GetLength(1);
            int highResSizeX = targetWidth;
            int highResSizeY = targetHeight;

            if (lowResSizeX > highResSizeX || lowResSizeY > highResSizeY)
                throw new ArgumentException("Cannot downscale data using this function.");

            if (useBilinear)
                return UpscaleBilinear(matrix, lowResSizeX, lowResSizeY, highResSizeX, highResSizeY);
            else
                return UpscaleBicubic(matrix, lowResSizeX, lowResSizeY, highResSizeX, highResSizeY);
        }
        private static double[,] UpscaleBilinear(double[,] matrix, int lowResSizeX, int lowResSizeY, int highResSizeX, int highResSizeY)
        {
            double[,] resized = new double[highResSizeX, highResSizeY];

            double A, B, C, D;
            int x, y;
            double xRatio = (lowResSizeX - 1d) / highResSizeX;
            double yRatio = (lowResSizeY - 1d) / highResSizeY;
            double xDiff, yDiff;

            for (int i = 0; i < highResSizeX; i++)
            {
                for (int j = 0; j < highResSizeY; j++)
                {
                    x = (int)(xRatio * i);
                    y = (int)(yRatio * j);
                    xDiff = (xRatio * i) - x;
                    yDiff = (yRatio * j) - y;

                    int x1 = x + 1;
                    if (x1 >= highResSizeX) x1 = x;
                    int y1 = y + 1;
                    if (y1 >= highResSizeY) y1 = y;

                    A = matrix[x, y];
                    B = matrix[x + 1, y];
                    C = matrix[x, y + 1];
                    D = matrix[x + 1, y + 1];

                    resized[i, j] = (A * (1d - xDiff) * (1d - yDiff)) + (B * (xDiff) * (1d - yDiff)) +
                        (C * (yDiff) * (1d - xDiff)) + (D * (xDiff) * (yDiff));
                }
            }

            return resized;
        }
        private static double[,] UpscaleBicubic(double[,] matrix, int lowResSizeX, int lowResSizeY, int highResSizeX, int highResSizeY)
        {
            double[,] resized = new double[highResSizeX, highResSizeY];

            double xRatio = (lowResSizeX - 1d) / highResSizeX;
            double yRatio = (lowResSizeY - 1d) / highResSizeY;

            for (int i = 0; i < highResSizeX; i++)
            {
                for (int j = 0; j < highResSizeY; j++)
                {
                    int x = (int)(xRatio * i);
                    int y = (int)(yRatio * j);

                    int[] xCoords = new int[]
                    {
                        x, Math.Min(x + 1, x), Math.Min(x + 2, x), Math.Min(x + 3, x)
                    };
                    int[] yCoords = new int[]
                    {
                        y, Math.Min(y + 1, y), Math.Min(y + 2, y), Math.Min(y + 3, y)
                    };

                    double[] xv = new double[4];
                    for (int k = 0; k < 4; k++)
                    {
                        xv[k] = InterpolateCubic(matrix[xCoords[0], yCoords[k]],
                            matrix[xCoords[1], yCoords[k]], matrix[xCoords[2], yCoords[k]], matrix[xCoords[3], yCoords[k]], xRatio);
                    }

                    resized[i, j] = InterpolateCubic(xv[0], xv[1], xv[2], xv[3], yRatio);
                }
            }

            return resized;
        }
        private static double InterpolateCubic(double v0, double v1, double v2, double v3, double frac)
        {
            double a = (v3 - v2) - (v0 - v1);
            double b = (v0 - v1) - a;
            double c = v2 - v0;
            double d = v1;

            return d + frac * (c + frac * (b + frac * a));
        }
    }
}
