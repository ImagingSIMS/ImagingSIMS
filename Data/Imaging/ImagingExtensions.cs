using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace ImagingSIMS.Data.Imaging
{
    public static class ImagingExtensions
    {
        /// <summary>
        /// Rescales bilinearly a 2D SharpDX.Color matrix.
        /// </summary>
        /// <param name="toResize">Color matrix to resize.</param>
        /// <param name="targetWidth">Output matrix width.</param>
        /// <param name="targetHeight">Output matrix height</param>
        /// <returns>Upsacled color matrix with the specified dimensions.</returns>
        public static Color[,] Upscale(this Color[,] toResize, int targetWidth, int targetHeight)
        {
            int lowResSizeX = toResize.GetLength(0);
            int lowResSizeY = toResize.GetLength(1);
            int highResSizeX = targetWidth;
            int highResSizeY = targetHeight;

            if (lowResSizeX > highResSizeX || lowResSizeY > highResSizeY)
                throw new ArgumentException("Cannot downscale data using this function.");

            SharpDX.Color[,] resized = new SharpDX.Color[highResSizeX, highResSizeY];

            SharpDX.Color A, B, C, D;
            int x, y;
            float xRatio = ((float)lowResSizeX - 1f) / (float)highResSizeX;
            float yRatio = ((float)lowResSizeY - 1f) / (float)highResSizeY;
            float xDiff, yDiff;

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

                    A = toResize[x, y];
                    B = toResize[x + 1, y];
                    C = toResize[x, y + 1];
                    D = toResize[x + 1, y + 1];

                    float a, r, g, b;
                    a = (A.A * (1f - xDiff) * (1f - yDiff)) + (B.A * (xDiff) * (1f - yDiff)) +
                        (C.A * (yDiff) * (1f - xDiff)) + (D.A * (xDiff) * (yDiff));
                    r = (A.R * (1f - xDiff) * (1f - yDiff)) + (B.R * (xDiff) * (1f - yDiff)) +
                        (C.R * (yDiff) * (1f - xDiff)) + (D.R * (xDiff) * (yDiff));
                    g = (A.G * (1f - xDiff) * (1f - yDiff)) + (B.G * (xDiff) * (1f - yDiff)) +
                        (C.G * (yDiff) * (1f - xDiff)) + (D.G * (xDiff) * (yDiff));
                    b = (A.B * (1f - xDiff) * (1f - yDiff)) + (B.B * (xDiff) * (1f - yDiff)) +
                        (C.B * (yDiff) * (1f - xDiff)) + (D.B * (xDiff) * (yDiff));

                    resized[i, j] = new SharpDX.Color((byte)r, (byte)g, (byte)b, (byte)a);

                }
            }

            return resized;
        }
    }
}
