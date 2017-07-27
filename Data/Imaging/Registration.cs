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
    public static class PointExtensionMethods
    {
        private static IntPoint Convert(this System.Windows.Point p)
        {
            return new IntPoint((int)p.X, (int)p.Y);
        }
        public static IEnumerable<IntPoint> Convert(this IEnumerable<System.Windows.Point> points)
        {
            return points.Select(p => p.Convert());
        }
    }
    public class PointRegistrationResult<T> where T : IDataObject
    {
        public T InputFixedImage { get; set; }
        public T InputMovingImage { get; set; }
        public T ScaledFixedImage { get; set; }
        public T ScaledMovingImage { get; set; }
        public T Result { get; set; }

        public PointRegistrationResult()
        {

        }
        public PointRegistrationResult(T inputFixedImage, T inputMovingImage)
        {
            InputFixedImage = inputFixedImage;
            InputMovingImage = inputMovingImage;
        }

        public PointRegistrationResult(T inputFixedImage, T inputMovingImage, 
            T scaledFixedImage, T scaledMovingImage, T result) : this(inputFixedImage, inputMovingImage)
        {
            ScaledFixedImage = scaledFixedImage;
            ScaledMovingImage = scaledMovingImage;
            Result = result;
        }
    }

    public enum PointRegistrationType
    {
        Ransac, Affine
    }

    public static class PointRegistrationGenerator
    {
        public static PointRegistration GetRegistrationClass(int numPoints)
        {
            if (numPoints == 3) return new AffineRegistration();
            else if (numPoints > 4) return new RansacRegistration();

            else throw new ArgumentException("Invlaid number of points for registration");
        }
    }

    public abstract class PointRegistration
    {
        public abstract PointRegistrationResult<Data2D> Register(Data2D fixedImage,
            Data2D movingImage, IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints);
        public async Task<PointRegistrationResult<Data2D>> RegisterAsync(Data2D fixedImage,
            Data2D movingImage, IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints)
        {
            return await Task.Run(() => Register(fixedImage, movingImage, fixedPoints, movingPoints));
        }

        public abstract PointRegistrationResult<Data3D> Register(Data3D fixedImage,
            Data3D movingImage, IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints);
        public async Task<PointRegistrationResult<Data3D>> RegisterAsync(Data3D fixedImage,
            Data3D movingImage, IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints)
        {
            return await Task.Run(() => Register(fixedImage, movingImage, fixedPoints, movingPoints));
        }
    }

    public class RansacRegistration : PointRegistration
    {
        public override PointRegistrationResult<Data2D> Register(Data2D fixedImage,
            Data2D movingImage, IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints)
        {
            List<IntPoint> fixedPointsCorrected = new List<IntPoint>();
            List<IntPoint> movingPointsCorrected = new List<IntPoint>();

            var result = new PointRegistrationResult<Data2D>(fixedImage, movingImage);

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

            result.ScaledFixedImage = fixedImage;
            result.ScaledMovingImage = movingImage;
            
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

            result.Result = transformed;

            return result;
        }

        public override PointRegistrationResult<Data3D> Register(Data3D fixedImage,
            Data3D movingImage, IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints)
        {
            List<IntPoint> fixedPointsCorrected = new List<IntPoint>();
            List<IntPoint> movingPointsCorrected = new List<IntPoint>();

            var result = new PointRegistrationResult<Data3D>(fixedImage, movingImage);

            if (fixedImage.Width != movingImage.Width || fixedImage.Height != movingImage.Height)
            {
                if (fixedImage.Width <= movingImage.Width && fixedImage.Height <= movingImage.Height)
                {
                    float ratioX = movingImage.Width / fixedImage.Width;
                    float ratioY = movingImage.Height / fixedImage.Height;

                    for (int z = 0; z < fixedImage.Depth; z++)
                    {
                        fixedImage.Layers[z] = fixedImage.Layers[z].Resize(movingImage.Width, movingImage.Height);
                    }

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

                    for (int z = 0; z < movingImage.Depth; z++)
                    {
                        movingImage.Layers[z] = movingImage.Layers[z].Resize(fixedImage.Width, fixedImage.Height);
                    }

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

            result.ScaledFixedImage = fixedImage;
            result.ScaledMovingImage = movingImage;

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

            result.Result = transformed;

            return result;
        }
    }

    public class AffineRegistration : PointRegistration
    {
        // https://stackoverflow.com/questions/2755771/affine-transformation-algorithm

        //      fixed       moving
        //   [x1 x2 x3]   [u1 u2 u3]
        // M [y1 y2 y3] = [v1 v2 v3]
        //   [1  1  1 ]

        // M = moving * inv(fixed)

        // --> https://stackoverflow.com/questions/22954239/given-three-points-compute-affine-transformation


        public override PointRegistrationResult<Data2D> Register(Data2D fixedImage,
            Data2D movingImage, IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints)
        {
            List<IntPoint> fixedPointsCorrected = new List<IntPoint>();
            List<IntPoint> movingPointsCorrected = new List<IntPoint>();

            var result = new PointRegistrationResult<Data2D>(fixedImage, movingImage);

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

            result.ScaledFixedImage = fixedImage;
            result.ScaledMovingImage = movingImage;

            AffineTransform transform = new Imaging.AffineTransform();
            transform.CalculateTransform(fixedPointsCorrected, movingPointsCorrected);

            int transformedWidth = fixedImage.Width;
            int transformedHeight = fixedImage.Height;

            var transformed = new Data2D(transformedWidth, transformedHeight);

            for (int x = 0; x < transformedWidth; x++)
            {
                for (int y = 0; y < transformedHeight; y++)
                { 
                    var transformedPoint = transform.TransformCoordinate(new IntPoint(x, y));

                    if (transformedPoint.X < 0 || transformedPoint.X >= transformedWidth || transformedPoint.Y < 0 || transformedPoint.Y >= transformedHeight) continue;

                    transformed[x, y] = result.ScaledMovingImage.Sample(transformedPoint.X, transformedPoint.Y);
                }
            }

            result.Result = transformed;

            return result;
        }
        public override PointRegistrationResult<Data3D> Register(Data3D fixedImage,
            Data3D movingImage, IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints)
        {
            throw new NotImplementedException();
        }
   
    }

    public class AffineTransform
    {
        public IEnumerable<IntPoint> FixedPoints { get; set; }
        public IEnumerable<IntPoint> MovingPoints { get; set; }
        public double[,] Transform { get; set; }

        public AffineTransform()
        {

        }
        public void CalculateTransform(IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints)
        {          
            FixedPoints = fixedPoints;
            MovingPoints = movingPoints;

            int numPoints = FixedPoints.Count();

            double[,] X = new double[numPoints * 2, 6];
            double[] xPrime = new double[numPoints * 2];

            for (int i = 0; i < numPoints; i++)
            {
                var xExpanded = ExpandCoordinate(FixedPoints.ElementAt(i));
                var xPrimePoint = MovingPoints.ElementAt(i);

                for (int c = 0; c < 6; c++)
                {
                    X[i, c] = xExpanded[0, c];
                    X[numPoints + i, c] = xExpanded[1, c];
                }

                xPrime[i] = xPrimePoint.X;
                xPrime[numPoints + i] = xPrimePoint.Y;
            }

            var solution = xPrime.Dot(X.PseudoInverse().Transpose());
            var affine = new double[3, 3];
            affine[0, 0] = solution[0];
            affine[0, 1] = solution[1];
            affine[0, 2] = solution[2];
            affine[1, 0] = solution[3];
            affine[1, 1] = solution[4];
            affine[1, 2] = solution[5];
            affine[2, 2] = 1;
            Transform = affine.Inverse();
            Transform[2, 2] = 1;
            Transform = Transform.Transpose();
        }

        public DoublePoint TransformCoordinate(IntPoint point)
        {
            if (Transform == null) throw new ArgumentException("Transform not calculated");

            double[] pointVector = new double[] { point.X, point.Y, 1 };
            var transformed = Transform.Dot(pointVector);

            return new DoublePoint(transformed[0] / transformed[2], transformed[1] / transformed[2]);
        }

        private double[,] ExpandCoordinate(IntPoint point)
        {
            double[,] expanded = new double[2, 6];

            expanded[0, 0] = point.X;
            expanded[0, 1] = point.Y;
            expanded[0, 2] = 1;

            expanded[1, 3] = point.X;
            expanded[1, 4] = point.Y;
            expanded[1, 5] = 1;

            return expanded;
        }
    }
}