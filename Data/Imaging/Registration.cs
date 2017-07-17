using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Accord;
using Accord.Imaging;
using Accord.Math;
using Accord.Math.Distances;
using Accord.Math.Metrics;

namespace ImagingSIMS.Data.Imaging
{
    public static class PointRegistration
    {
        public static Data2D Register(Data2D fixedImage,
            Data2D movingImage, IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints)
        {
            List<IntPoint> fixedPointsCorrected = new List<IntPoint>();
            List<IntPoint> movingPointsCorrected = new List<IntPoint>();

            if (fixedImage.Width != movingImage.Width || fixedImage.Height != movingImage.Height)
            {
                if (fixedImage.Width <= movingImage.Width && fixedImage.Height <= movingImage.Height)
                {               
                    float ratioX = movingImage.Width / fixedImage.Width;
                    float ratioY = movingImage.Height / fixedImage.Height;

                    fixedImage = fixedImage.Resize(movingImage.Width, movingImage.Height);

                    foreach (var point in fixedPoints)
                    {
                        fixedPointsCorrected.Add(new IntPoint((int)(point.X * ratioX), (int)(point.Y * ratioY)));
                    }


                    movingPointsCorrected.AddRange(movingPoints);
                }
                else if (movingImage.Width <= fixedImage.Width && movingImage.Height <= fixedImage.Height)
                {             
                    float ratioX = fixedImage.Width / movingImage.Width;
                    float ratioY = fixedImage.Height / movingImage.Height;

                    movingImage = movingImage.Resize(fixedImage.Width, fixedImage.Height);

                    foreach (var point in movingPoints)
                    {
                        movingPointsCorrected.Add(new IntPoint((int)(point.X * ratioX), (int)(point.Y * ratioY)));
                    }

                    fixedPointsCorrected.AddRange(fixedPoints);
                }
                else throw new ArgumentException("Invalid image dimensions. Cannot resize when one dimension is smaller than the other");
            }

            else
            {
                fixedPointsCorrected.AddRange(fixedPoints);
                movingPointsCorrected.AddRange(movingPoints);
            }

            RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.001, 0.99);
            var transform = ransac.Estimate(fixedPointsCorrected.ToArray(), movingPointsCorrected.ToArray()).ToDoubleArray();

            int transformedWidth = fixedImage.Width;
            int transformedHeight = fixedImage.Height;

            var transformed = new Data2D(transformedWidth, transformedHeight);

            for (int x = 0; x < transformedWidth; x++)
            {
                for (int y = 0; y < transformedHeight; y++)
                {
                    var point = new double[] { x, y, 1 };

                    var transformedPoint = transform.Dot(point);

                    var hx = transformedPoint[0] / transformedPoint[2];
                    var hy = transformedPoint[1] / transformedPoint[2];

                    if (hx < 0 || hx >= transformedWidth || hy < 0 || hy >= transformedHeight) continue;

                    transformed[x, y] = movingImage.Sample(hx, hy);
                }
            }

            return transformed;
        }
        public static Task<Data2D> RegisterAsync(Data2D fixedImage,
            Data2D movingImage, IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints)
        {
            return Task.Run(() => Register(fixedImage, movingImage, fixedPoints, movingPoints));
        }

        public static Data3D Register(Data3D fixedImage,
            Data3D movingImage, IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints)
        {
            if (fixedImage.Width != movingImage.Width || fixedImage.Height != movingImage.Height)
            {
                if (fixedImage.Width <= movingImage.Width && fixedImage.Height <= movingImage.Height)
                {
                    for (int z = 0; z < fixedImage.Depth; z++)
                    {
                        fixedImage.Layers[z] = fixedImage.Layers[z].Resize(movingImage.Width, movingImage.Height);
                    }                    
                }
                else if (movingImage.Width <= fixedImage.Width && movingImage.Height <= fixedImage.Height)
                {
                    for (int z = 0; z < movingImage.Depth; z++)
                    {
                        movingImage.Layers[z] = movingImage.Layers[z].Resize(fixedImage.Width, fixedImage.Height);
                    }                    
                }
                else throw new ArgumentException("Invalid image dimensions. Cannot resize when one dimension is smaller than the other");
            }

            RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.001, 0.99);
            var transform = ransac.Estimate(fixedPoints.ToArray(), movingPoints.ToArray()).ToDoubleArray();

            int transformedWidth = fixedImage.Width;
            int transformedHeight = fixedImage.Height;
            int transformedDepth = movingImage.Depth;

            var transformed = new Data3D(transformedWidth, transformedHeight, transformedDepth);

            for (int x = 0; x < transformedWidth; x++)
            {
                for (int y = 0; y < transformedHeight; y++)
                {
                    var point = new double[] { x, y, 1 };

                    var transformedPoint = transform.Dot(point);

                    var hx = transformedPoint[0] / transformedPoint[2];
                    var hy = transformedPoint[1] / transformedPoint[2];

                    if (hx < 0 || hx >= transformedWidth || hy < 0 || hy >= transformedHeight) continue;

                    for (int z = 0; z < transformedDepth; z++)
                    {
                        transformed[x, y, z] = movingImage.Layers[z].Sample(hx, hy);
                    }                    
                }
            }

            return transformed;
        }
        public static Task<Data3D> RegisterAsync(Data3D fixedImage,
            Data3D movingImage, IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints)
        {
            return Task.Run(() => Register(fixedImage, movingImage, fixedPoints, movingPoints));
        }

        public static IntPoint Convert(this System.Windows.Point p)
        {
            return new IntPoint((int)p.X, (int)p.Y);
        }
        public static IEnumerable<IntPoint> Convert(this IEnumerable<System.Windows.Point> points)
        {
            return points.Select(p => p.Convert());
        }
    }
}