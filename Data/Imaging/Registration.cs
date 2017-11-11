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
using ImagingSIMS.Common.Math;

namespace ImagingSIMS.Data.Imaging
{
    public static class PointExtensionMethods
    {
        private static IntPoint ConvertToIntPoint(this System.Windows.Point p)
        {
            return new IntPoint((int)p.X, (int)p.Y);
        }
        public static IEnumerable<IntPoint> ConvertToIntPoint(this IEnumerable<System.Windows.Point> points)
        {
            return points.Select(p => p.ConvertToIntPoint());
        }
    }
    public class PointRegistrationResult<T> where T : IDataObject
    {
        public T InputFixedImage { get; set; }
        public T InputMovingImage { get; set; }
        public T ScaledFixedImage { get; set; }
        public T ScaledMovingImage { get; set; }
        public T Result { get; set; }
        public PointRegistration Transform { get; set; }

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
        Ransac, Affine, Projective
    }

    public static class PointRegistrationGenerator
    {
        public static PointRegistration GetRegistrationClass(PointRegistrationType registrationType)
        {
            switch (registrationType)
            {
                case PointRegistrationType.Affine:
                    return new AffineRegistration();
                case PointRegistrationType.Projective:
                    return new ProjectiveRegistration();
                case PointRegistrationType.Ransac:
                    return new RansacRegistration();
                default:
                    return new ProjectiveRegistration();
            }
        }
    }

    public abstract class PointRegistration
    {
        public int TargetWidth { get; set; }
        public int TargetHeight { get; set; }

        public abstract PointRegistrationResult<Data2D> Register(Data2D fixedImage, Data2D movingImage, 
            IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints, IProgress<int> progressReporter = null);
        public async Task<PointRegistrationResult<Data2D>> RegisterAsync(Data2D fixedImage, Data2D movingImage, 
            IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints, IProgress<int> progressReporter = null)
        {
            return await Task.Run(() => Register(fixedImage, movingImage, fixedPoints, movingPoints, progressReporter));
        }

        public abstract PointRegistrationResult<Data3D> Register(Data3D fixedImage, Data3D movingImage, 
            IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints, IProgress<int> progressReporter = null);
        public async Task<PointRegistrationResult<Data3D>> RegisterAsync(Data3D fixedImage, Data3D movingImage, 
            IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints, IProgress<int> progressReporter = null)
        {
            return await Task.Run(() => Register(fixedImage, movingImage, fixedPoints, movingPoints, progressReporter));
        }

        public abstract Data2D Transform(Data2D data, string tableName, IProgress<int> progressReporter = null);
        public async Task<Data2D> TransformAsync(Data2D data, string tableName, IProgress<int> progressReporter = null)
        {
            return await Task.Run(() => Transform(data, tableName, progressReporter));
        }

        public abstract Data3D Transform(Data3D data, string tableName, IProgress<int> progressReporter = null);
        public async Task<Data3D> TransformAsync(Data3D data, string tableName, IProgress<int> progressReporter = null)
        {
            return await Task.Run(() => Transform(data, tableName, progressReporter));
        }
    }

    public class RansacRegistration : PointRegistration
    {
        private double[,] CalculatedTransform { get; set; }

        public override PointRegistrationResult<Data2D> Register(Data2D fixedImage, Data2D movingImage,
            IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints, IProgress<int> progressReporter = null)
        {
            List<IntPoint> fixedPointsCorrected = new List<IntPoint>();
            List<IntPoint> movingPointsCorrected = new List<IntPoint>();

            var result = new PointRegistrationResult<Data2D>(fixedImage, movingImage);

            if (fixedImage.Width != movingImage.Width || fixedImage.Height != movingImage.Height)
            {
                if (fixedImage.Width <= movingImage.Width && fixedImage.Height <= movingImage.Height)
                {
                    float ratioX = (float)movingImage.Width / fixedImage.Width;
                    float ratioY = (float)movingImage.Height / fixedImage.Height;

                    fixedImage = fixedImage.Resize(movingImage.Width, movingImage.Height);

                    foreach (var point in fixedPoints)
                    {
                        fixedPointsCorrected.Add(new IntPoint((int)(point.X * ratioX), (int)(point.Y * ratioY)));
                    }

                    movingPointsCorrected.AddRange(movingPoints);
                }
                else if (movingImage.Width <= fixedImage.Width && movingImage.Height <= fixedImage.Height)
                {
                    float ratioX = (float)fixedImage.Width / movingImage.Width;
                    float ratioY = (float)fixedImage.Height / movingImage.Height;

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

            CalculatedTransform = transform;

            int transformedWidth = fixedImage.Width;
            int transformedHeight = fixedImage.Height;

            TargetWidth = transformedWidth;
            TargetHeight = transformedHeight;

            var transformed = Transform(result.ScaledMovingImage, movingImage.DataName + " - Registered", progressReporter);

            result.Result = transformed;
            result.Transform = this;

            return result;
        }
        public override PointRegistrationResult<Data3D> Register(Data3D fixedImage, Data3D movingImage, 
            IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints, IProgress<int> progressReporter = null)
        {
            List<IntPoint> fixedPointsCorrected = new List<IntPoint>();
            List<IntPoint> movingPointsCorrected = new List<IntPoint>();

            var result = new PointRegistrationResult<Data3D>(fixedImage, movingImage);

            if (fixedImage.Width != movingImage.Width || fixedImage.Height != movingImage.Height)
            {
                if (fixedImage.Width <= movingImage.Width && fixedImage.Height <= movingImage.Height)
                {
                    float ratioX = (float)movingImage.Width / fixedImage.Width;
                    float ratioY = (float)movingImage.Height / fixedImage.Height;

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
                    float ratioX = (float)fixedImage.Width / movingImage.Width;
                    float ratioY = (float)fixedImage.Height / movingImage.Height;

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

            CalculatedTransform = transform;

            int transformedWidth = fixedImage.Width;
            int transformedHeight = fixedImage.Height;
            int transformedDepth = movingImage.Depth;

            TargetWidth = transformedWidth;
            TargetHeight = transformedHeight;

            var transformed = Transform(result.ScaledMovingImage, movingImage.DataName + " - Registered", progressReporter);

            result.Result = transformed;
            result.Transform = this;

            return result;
        }

        public override Data2D Transform(Data2D data, string tableName, IProgress<int> progressReporter = null)
        {
            int transformedWidth = data.Width;
            int transformedHeight = data.Height;

            var transformed = new Data2D(transformedWidth, transformedHeight)
            {
                DataName = tableName
            };

            for (int x = 0; x < transformedWidth; x++)
            {
                for (int y = 0; y < transformedHeight; y++)
                {
                    var point = new double[] { x, y, 1 };

                    var transformedPoint = CalculatedTransform.Dot(point);

                    var hx = transformedPoint[0] / transformedPoint[2];
                    var hy = transformedPoint[1] / transformedPoint[2];

                    if (hx < 0 || hx >= transformedWidth || hy < 0 || hy >= transformedHeight) continue;

                    transformed[x, y] = data.Sample(hx, hy);
                }

                progressReporter?.Report(Percentage.GetPercent(x, transformedWidth));
            }

            return transformed;
        }
        public override Data3D Transform(Data3D data, string tableName, IProgress<int> progressReporter = null)
        {
            int transformedWidth = data.Width;
            int transformedHeight = data.Height;
            int transformedDepth = data.Depth;

            var transformed = new Data3D(transformedWidth, transformedHeight, transformedDepth)
            {
                DataName = tableName
            };

            for (int x = 0; x < transformedWidth; x++)
            {
                for (int y = 0; y < transformedHeight; y++)
                {
                    var point = new double[] { x, y, 1 };

                    var transformedPoint = CalculatedTransform.Dot(point);

                    var hx = transformedPoint[0] / transformedPoint[2];
                    var hy = transformedPoint[1] / transformedPoint[2];

                    if (hx < 0 || hx >= transformedWidth || hy < 0 || hy >= transformedHeight) continue;

                    for (int z = 0; z < transformedDepth; z++)
                    {
                        transformed[x, y, z] = data.Layers[z].Sample(hx, hy);
                    }
                }

                progressReporter?.Report(Percentage.GetPercent(x, transformedWidth));
            }

            return transformed;
        }
    }

    public class ProjectiveRegistration : PointRegistration
    {
        public ProjectiveTransform CalculatedTransform { get; set; }

        public override PointRegistrationResult<Data2D> Register(Data2D fixedImage, Data2D movingImage, 
            IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints, IProgress<int> progressReporter = null)
        {
            List<IntPoint> fixedPointsCorrected = new List<IntPoint>();
            List<IntPoint> movingPointsCorrected = new List<IntPoint>();

            var result = new PointRegistrationResult<Data2D>(fixedImage, movingImage);

            if (fixedImage.Width != movingImage.Width || fixedImage.Height != movingImage.Height)
            {
                if (fixedImage.Width <= movingImage.Width && fixedImage.Height <= movingImage.Height)
                {
                    float ratioX = (float)movingImage.Width / fixedImage.Width;
                    float ratioY = (float)movingImage.Height / fixedImage.Height;

                    fixedImage = fixedImage.Resize(movingImage.Width, movingImage.Height);

                    foreach (var point in fixedPoints)
                    {
                        fixedPointsCorrected.Add(new IntPoint((int)(point.X * ratioX), (int)(point.Y * ratioY)));
                    }

                    movingPointsCorrected.AddRange(movingPoints);
                }
                else if (movingImage.Width <= fixedImage.Width && movingImage.Height <= fixedImage.Height)
                {
                    float ratioX = (float)fixedImage.Width / movingImage.Width;
                    float ratioY = (float)fixedImage.Height / movingImage.Height;

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

            ProjectiveTransform transform = new ProjectiveTransform();
            transform.CalculateTransform(fixedPointsCorrected, movingPointsCorrected);

            CalculatedTransform = transform;

            int transformedWidth = fixedImage.Width;
            int transformedHeight = fixedImage.Height;

            TargetWidth = transformedWidth;
            TargetHeight = transformedHeight;

            var transformed = Transform(result.ScaledMovingImage, movingImage.DataName + " - Registered", progressReporter);

            result.Result = transformed;
            result.Transform = this;

            return result;
        }
        public override PointRegistrationResult<Data3D> Register(Data3D fixedImage, Data3D movingImage, 
            IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints, IProgress<int> progressReporter = null)
        {
            List<IntPoint> fixedPointsCorrected = new List<IntPoint>();
            List<IntPoint> movingPointsCorrected = new List<IntPoint>();

            var result = new PointRegistrationResult<Data3D>(fixedImage, movingImage);

            if (fixedImage.Width != movingImage.Width || fixedImage.Height != movingImage.Height)
            {
                if (fixedImage.Width <= movingImage.Width && fixedImage.Height <= movingImage.Height)
                {
                    float ratioX = (float)movingImage.Width / fixedImage.Width;
                    float ratioY = (float)movingImage.Height / fixedImage.Height;

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
                    float ratioX = (float)fixedImage.Width / movingImage.Width;
                    float ratioY = (float)fixedImage.Height / movingImage.Height;

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

            ProjectiveTransform transform = new ProjectiveTransform();
            transform.CalculateTransform(fixedPointsCorrected, movingPointsCorrected);

            CalculatedTransform = transform;

            int transformedWidth = fixedImage.Width;
            int transformedHeight = fixedImage.Height;
            int transformedDepth = movingImage.Depth;

            TargetWidth = transformedWidth;
            TargetHeight = transformedHeight;

            var transformed = Transform(result.ScaledMovingImage, movingImage.DataName + " - Registered", progressReporter);

            result.Result = transformed;
            result.Transform = this;

            return result;
        }

        public override Data2D Transform(Data2D data, string tableName, IProgress<int> progressReporter = null)
        {
            int transformedWidth = data.Width;
            int transformedHeight = data.Height;

            var transformed = new Data2D(transformedWidth, transformedHeight)
            {
                DataName = tableName
            };

            for (int x = 0; x < transformedWidth; x++)
            {
                for (int y = 0; y < transformedHeight; y++)
                {
                    var transformedPoint = CalculatedTransform.TransformCoordinate(new IntPoint(x, y));

                    if (transformedPoint.X < 0 || transformedPoint.X >= transformedWidth || transformedPoint.Y < 0 || transformedPoint.Y >= transformedHeight) continue;

                    transformed[x, y] = data.Sample(transformedPoint.X, transformedPoint.Y);
                }

                progressReporter?.Report(Percentage.GetPercent(x, transformedWidth));
            }

            return transformed;
        }
        public override Data3D Transform(Data3D data, string tableName, IProgress<int> progressReporter = null)
        {
            int transformedWidth = data.Width;
            int transformedHeight = data.Height;
            int transformedDepth = data.Depth;

            var transformed = new Data3D(transformedWidth, transformedHeight, transformedDepth)
            {
                DataName = tableName
            };

            for (int x = 0; x < transformedWidth; x++)
            {
                for (int y = 0; y < transformedHeight; y++)
                {
                    var transformedPoint = CalculatedTransform.TransformCoordinate(new IntPoint(x, y));

                    if (transformedPoint.X < 0 || transformedPoint.X >= transformedWidth || transformedPoint.Y < 0 || transformedPoint.Y >= transformedHeight) continue;

                    for (int z = 0; z < transformedDepth; z++)
                    {
                        transformed[x, y, z] = data.Layers[z].Sample(transformedPoint.X, transformedPoint.Y);
                    }

                    progressReporter?.Report(Percentage.GetPercent(x, transformedWidth));
                }
            }

            return transformed;
        }
    }

    public class ProjectiveTransform
    {
        public IEnumerable<IntPoint> FixedPoints { get; set; }
        public IEnumerable<IntPoint> MovingPoints { get; set; }
        public double[,] Transform { get; set; }

        public ProjectiveTransform()
        {

        }
        public void CalculateTransform(IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints)
        {
            FixedPoints = fixedPoints;
            MovingPoints = movingPoints;

            int numPoints = FixedPoints.Count();

            double[] U = new double[numPoints * 2];
            double[,] X = new double[numPoints * 2, 8];

            // uv: moving points
            // xy: fixed points

            // u = [x y 1 0 0 0 -ux -uy] * [A B C D E F G H]'
            // v = [0 0 0 x y 1 -vx -vy] * [A B C D E F G H]'

            for (int i = 0; i < numPoints; i++)
            {
                var fixedPoint = fixedPoints.ElementAt(i);
                var movingPoint = movingPoints.ElementAt(i);

                U[i] = movingPoint.X;
                U[numPoints + i] = movingPoint.Y;

                X[i, 0] = fixedPoint.X;
                X[i, 1] = fixedPoint.Y;
                X[i, 2] = 1;
                X[i, 6] = -movingPoint.X * fixedPoint.X;
                X[i, 7] = -movingPoint.X * fixedPoint.Y;

                X[numPoints + i, 3] = fixedPoint.X;
                X[numPoints + i, 4] = fixedPoint.Y;
                X[numPoints + i, 5] = 1;
                X[numPoints + i, 6] = -movingPoint.Y * fixedPoint.X;
                X[numPoints + i, 7] = -movingPoint.Y * fixedPoint.Y;
            }

            var tVec = X.Solve(U);
            tVec = AppendVectorWithOne(tVec);

            var tInv = tVec.Reshape(3, 3);
            Transform = tInv.Inverse();
            Transform = Transform.Inverse();
            Transform = Transform.Divide(Transform[2, 2]);
        }

        private double[] AppendVectorWithOne(double[] vector)
        {
            var result = new double[vector.Length + 1];
            Array.Copy(vector, 0, result, 0, vector.Length);
            result[vector.Length] = 1;
            return result;
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

    public class AffineRegistration : PointRegistration
    {
        public AffineTransform CalculatedTransform { get; set; }

        public override PointRegistrationResult<Data2D> Register(Data2D fixedImage, Data2D movingImage, 
            IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints, IProgress<int> progressReporter = null)
        {
            List<IntPoint> fixedPointsCorrected = new List<IntPoint>();
            List<IntPoint> movingPointsCorrected = new List<IntPoint>();

            var result = new PointRegistrationResult<Data2D>(fixedImage, movingImage);

            if (fixedImage.Width != movingImage.Width || fixedImage.Height != movingImage.Height)
            {
                if (fixedImage.Width <= movingImage.Width && fixedImage.Height <= movingImage.Height)
                {
                    float ratioX = (float)movingImage.Width / fixedImage.Width;
                    float ratioY = (float)movingImage.Height / fixedImage.Height;

                    fixedImage = fixedImage.Resize(movingImage.Width, movingImage.Height);

                    foreach (var point in fixedPoints)
                    {
                        fixedPointsCorrected.Add(new IntPoint((int)(point.X * ratioX), (int)(point.Y * ratioY)));
                    }

                    movingPointsCorrected.AddRange(movingPoints);
                }
                else if (movingImage.Width <= fixedImage.Width && movingImage.Height <= fixedImage.Height)
                {
                    float ratioX = (float)fixedImage.Width / movingImage.Width;
                    float ratioY = (float)fixedImage.Height / movingImage.Height;

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

            CalculatedTransform = transform;

            int transformedWidth = fixedImage.Width;
            int transformedHeight = fixedImage.Height;

            TargetWidth = transformedWidth;
            TargetHeight = transformedHeight;

            var transformed = Transform(result.ScaledMovingImage, movingImage.DataName + " - Registered", progressReporter);

            result.Result = transformed;
            result.Transform = this;

            return result;
        }
        public override PointRegistrationResult<Data3D> Register(Data3D fixedImage, Data3D movingImage, 
            IEnumerable<IntPoint> fixedPoints, IEnumerable<IntPoint> movingPoints, IProgress<int> progressReporter = null)
        {
            List<IntPoint> fixedPointsCorrected = new List<IntPoint>();
            List<IntPoint> movingPointsCorrected = new List<IntPoint>();

            var result = new PointRegistrationResult<Data3D>(fixedImage, movingImage);

            if (fixedImage.Width != movingImage.Width || fixedImage.Height != movingImage.Height)
            {
                if (fixedImage.Width <= movingImage.Width && fixedImage.Height <= movingImage.Height)
                {
                    float ratioX = (float)movingImage.Width / fixedImage.Width;
                    float ratioY = (float)movingImage.Height / fixedImage.Height;

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
                    float ratioX = (float)fixedImage.Width / movingImage.Width;
                    float ratioY = (float)fixedImage.Height / movingImage.Height;

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

            AffineTransform transform = new Imaging.AffineTransform();
            transform.CalculateTransform(fixedPointsCorrected, movingPointsCorrected);

            CalculatedTransform = transform;

            int transformedWidth = fixedImage.Width;
            int transformedHeight = fixedImage.Height;
            int transformedDepth = movingImage.Depth;

            TargetWidth = transformedWidth;
            TargetHeight = transformedHeight;

            var transformed = Transform(result.ScaledMovingImage, movingImage.DataName + " - Registered", progressReporter);

            result.Result = transformed;
            result.Transform = this;

            return result;
        }

        public override Data2D Transform(Data2D data, string tableName, IProgress<int> progressReporter = null)
        {
            int transformedWidth = data.Width;
            int transformedHeight = data.Height;

            var transformed = new Data2D(transformedWidth, transformedHeight)
            {
                DataName = tableName
            };

            for (int x = 0; x < transformedWidth; x++)
            {
                for (int y = 0; y < transformedHeight; y++)
                {
                    var transformedPoint = CalculatedTransform.TransformCoordinate(new IntPoint(x, y));

                    if (transformedPoint.X < 0 || transformedPoint.X >= transformedWidth || transformedPoint.Y < 0 || transformedPoint.Y >= transformedHeight) continue;

                    transformed[x, y] = data.Sample(transformedPoint.X, transformedPoint.Y);
                }

                progressReporter?.Report(Percentage.GetPercent(x, transformedWidth));
            }

            return transformed;
        }
        public override Data3D Transform(Data3D data, string tableName, IProgress<int> progressReporter = null)
        {
            int transformedWidth = data.Width;
            int transformedHeight = data.Height;
            int transformedDepth = data.Depth;

            var transformed = new Data3D(transformedWidth, transformedHeight, transformedDepth)
            {
                DataName = tableName
            };

            for (int x = 0; x < transformedWidth; x++)
            {
                for (int y = 0; y < transformedHeight; y++)
                {
                    var transformedPoint = CalculatedTransform.TransformCoordinate(new IntPoint(x, y));

                    if (transformedPoint.X < 0 || transformedPoint.X >= transformedWidth || transformedPoint.Y < 0 || transformedPoint.Y >= transformedHeight) continue;

                    for (int z = 0; z < transformedDepth; z++)
                    {
                        transformed[x, y, z] = data.Layers[z].Sample(transformedPoint.X, transformedPoint.Y);
                    }
                }

                progressReporter?.Report(Percentage.GetPercent(x, transformedWidth));
            }

            return transformed;
        }
    }

    public class AffineTransform
    {
        // --> https://stackoverflow.com/questions/22954239/given-three-points-compute-affine-transformation

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

            var solution = X.Solve(xPrime);

            var affine = new double[3, 3];
            affine[0, 0] = solution[0];
            affine[0, 1] = solution[1];
            affine[0, 2] = solution[2];
            affine[1, 0] = solution[3];
            affine[1, 1] = solution[4];
            affine[1, 2] = solution[5];
            affine[2, 2] = 1;

            Transform = affine;
            Transform[2, 2] = 1;
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