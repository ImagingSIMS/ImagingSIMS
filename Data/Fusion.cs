using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImagingSIMS.Common.Math;
using ImagingSIMS.Data.Converters;
using ImagingSIMS.Data.Imaging;

using Matrix = DotNumerics.LinearAlgebra.Matrix;

namespace ImagingSIMS.Data.Fusion
{
    public abstract class Fusion
    {
        protected const double Zero = 0d;
        protected const double OneThird = 0.333333d;
        protected const double TwoThirds = 0.666666d;
        protected const double One = 1.0d;

        static double OneOverSqrt3 = 1 / (Math.Sqrt(3));
        static double OneOverSqrt6 = 1 / (Math.Sqrt(6));
        static double OneOverSqrt2 = 1 / (Math.Sqrt(2));

        protected bool _isResized;
        public bool IsResized
        {
            get { return _isResized; }
            set { _isResized = value; }
        }

        public abstract Data3D DoFusion();
        public abstract Task<Data3D> DoFusionAsync();
    }
    public abstract class Pansharpening : Fusion
    {
        protected int _highResSizeX;
        protected int _highResSizeY;
        protected int _lowResSizeX;
        protected int _lowResSizeY;

        protected Data2D _gray;
        protected Data3D _color;
        protected Data3D _colorNotResized;

        protected double[] _bandMeans;

        protected virtual float _red(int x, int y)
        {
            if (_color == null) return 0;
            return _color[x, y, 2];
        }
        protected virtual float _green(int x, int y)
        {
            if (_color == null) return 0;
            return _color[x, y, 1];
        }
        protected virtual float _blue(int x, int y)
        {
            if (_color == null) return 0;
            return _color[x, y, 0];
        }
        protected virtual float _alpha(int x, int y)
        {
            if (_color == null) return 0;
            return _color[x, y, 3];
        }

        public void CheckFusion()
        {
            if (_gray == null || _color == null) throw new ArgumentNullException("Data not loaded.");
            if (_gray.Maximum == 0) throw new ArgumentException("Grayscale image has no values.");
            if (_gray.Width < _color.Width || _gray.Height < _color.Height) throw new ArgumentException("Grayscale image is smaller than the color image.");
            if (!_isResized) throw new ArgumentException("Images have not been resized.");
        }

        protected void SetSizes()
        {
            _highResSizeX = _gray.Width;
            _highResSizeY = _gray.Height;
            _lowResSizeX = _color.Width;
            _lowResSizeY = _color.Height;

            Resize();
        }
        protected void Resize()
        {
            //if (_highResSizeX == _lowResSizeX && _highResSizeY == _lowResSizeY &&
            //    _highResSizeX != 0 && _highResSizeY != 0)
            //{
            //    _isResized = true;
            //    return;
            //}

            _colorNotResized = _color;
            _color = ImageHelper.Upscale(_color, _highResSizeX, _highResSizeY);
            _isResized = true;
        }

        protected void SetMeans()
        {
            _bandMeans = new double[3];
            _bandMeans[0] = _color.Layers[0].Mean;
            _bandMeans[1] = _color.Layers[1].Mean;
            _bandMeans[2] = _color.Layers[2].Mean;
        }

        public Pansharpening(BitmapSource HighRes, BitmapSource LowRes)
        {
            _gray = ImageHelper.ConvertToData2D(HighRes);
            _color = ImageHelper.ConvertToData3D(LowRes);

            SetMeans();

            SetSizes();
        }
        public Pansharpening(float[,] HighRes, float[,] LowRes, Color LowResBaseColor)
        {
            _gray = new Data2D(HighRes);

            int sizeX = LowRes.GetLength(0);
            int sizeY = LowRes.GetLength(1);

            Data2D[] channels = new Data2D[4];
            for (int i = 0; i < 4; i++)
            {
                channels[i] = new Data2D(sizeX, sizeY);
            }

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    float val = LowRes[x, y];
                    channels[0][x, y] = (val * LowResBaseColor.B) / 255f;
                    channels[1][x, y] = (val * LowResBaseColor.G) / 255f;
                    channels[2][x, y] = (val * LowResBaseColor.R) / 255f;
                    channels[3][x, y] = 0;
                }
            }

            _color = new Data3D(channels);

            SetMeans();

            SetSizes();
        }
        public Pansharpening(Data2D HighRes, Data3D LowRes)
        {
            _gray = HighRes;
            _color = LowRes;

            SetMeans();

            SetSizes();
        }
    }

    public class WeightedAverageFusion : Pansharpening
    {
        public WeightedAverageFusion(BitmapSource HighRes, BitmapSource LowRes)
            : base(HighRes, LowRes)
        {
        }
        public WeightedAverageFusion(float[,] HighRes, float[,] LowRes, Color LowResBaseColor)
            : base(HighRes, LowRes, LowResBaseColor)
        {
        }
        public WeightedAverageFusion(Data2D HighRes, Data3D LowRes)
            : base(HighRes, LowRes)
        {
        }

        public override Data3D DoFusion()
        {
            Data2D[] result = new Data2D[4];
            for (int i = 0; i < 4; i++)
            {
                result[i] = new Data2D(_highResSizeX, _highResSizeY);
            }

            bool useAlpha = _color.LayerMaximum(3) != 0;

            for (int x = 0; x < _highResSizeX; x++)
            {
                for (int y = 0; y < _highResSizeY; y++)
                {
                    if (useAlpha)
                    {
                        result[0][x, y] = ((_color[x, y, 3] * _color[x, y, 0]) + ((1 - (_color[x, y, 3] / 255f)) * _gray[x, y])) / 2;
                        result[1][x, y] = ((_color[x, y, 3] * _color[x, y, 1]) + ((1 - (_color[x, y, 3] / 255f)) * _gray[x, y])) / 2;
                        result[2][x, y] = ((_color[x, y, 3] * _color[x, y, 2]) + ((1 - (_color[x, y, 3] / 255f)) * _gray[x, y])) / 2;
                    }
                    else
                    {
                        float colorGray = (_color[x, y, 0] + _color[x, y, 1] + _color[x, y, 2]) / 3;
                        result[0][x, y] = ((colorGray * _color[x, y, 0]) + ((1 - (colorGray / 255f)) * _gray[x, y])) / 2;
                        result[1][x, y] = ((colorGray * _color[x, y, 1]) + ((1 - (colorGray / 255f)) * _gray[x, y])) / 2;
                        result[2][x, y] = ((colorGray * _color[x, y, 2]) + ((1 - (colorGray / 255f)) * _gray[x, y])) / 2;
                    }
                }
            }

            return new Data3D(result);
        }
        public override async Task<Data3D> DoFusionAsync()
        {
            return await Task<Data3D>.Run(() => DoFusion()); 
        }
    }

    public class HSLFusion : Pansharpening
    {
        public HSLFusion(BitmapSource HighRes, BitmapSource LowRes)
            : base(HighRes, LowRes)
        {
        }
        public HSLFusion(float[,] HighRes, float[,] LowRes, Color LowResBaseColor)
            : base(HighRes, LowRes, LowResBaseColor)
        {
        }
        public HSLFusion(Data2D HighRes, Data3D LowRes)
            : base(HighRes, LowRes)
        {
        }

        public override Data3D DoFusion()
        {
            // Create matrices of each HSL component
            double[,] hslH = new double[_highResSizeX, _highResSizeY];
            double[,] hslS = new double[_highResSizeX, _highResSizeY];
            double[,] hslL = new double[_highResSizeX, _highResSizeY];

            for (int x = 0; x < _highResSizeX; x++)
            {
                for (int y = 0; y < _highResSizeY; y++)
                {
                    double B = _color[x, y, 0];
                    double G = _color[x, y, 1];
                    double R = _color[x, y, 2];

                    double[] hsl = ColorConversion.RGBtoHSL(R, G, B);

                    hslH[x, y] = hsl[0];
                    hslS[x, y] = hsl[1];
                    hslL[x, y] = hsl[2];
                }
            }

            double u1, u0, s1, s0;

            double sumI = 0;
            double sumP = 0;

            // Square intensity values for both panchromatic and 
            // multispectral image
            double[,] matrixI2 = new double[_highResSizeX, _highResSizeY];
            double[,] matrixP2 = new double[_highResSizeX, _highResSizeY];

            for (int x = 0; x < _highResSizeX; x++)
            {
                for (int y = 0; y < _highResSizeY; y++)
                {
                    matrixI2[x, y] = hslL[x, y] * hslL[x, y];

                    matrixP2[x, y] = _gray[x, y] * _gray[x, y];

                    sumI += matrixI2[x, y];
                    sumP += matrixP2[x, y];
                }
            }

            // Calculate averages for each image
            u0 = (double)(sumI / (double)(_highResSizeX * _highResSizeY));
            u1 = (double)(sumP / (double)(_highResSizeX * _highResSizeY));

            // Calculate standard deviations for each image
            double sumSqI = 0;
            double sumSqP = 0;
            for (int x = 0; x < _highResSizeX; x++)
            {
                for (int y = 0; y < _highResSizeY; y++)
                {
                    double i2 = matrixI2[x, y];
                    double p2 = matrixP2[x, y];

                    sumSqI += (i2 - u0) * (i2 - u0);
                    sumSqP += (p2 - u1) * (p2 - u1);
                }
            }

            s0 = (double)Math.Sqrt(sumSqI / (_highResSizeX * _highResSizeY));
            s1 = (double)Math.Sqrt(sumSqP / (_highResSizeX * _highResSizeY));

            Data2D[] fused = new Data2D[4];
            for (int i = 0; i < 4; i++)
            {
                fused[i] = new Data2D(_highResSizeX, _highResSizeY);
            }

            //0 = sims I component
            //1 = sem
            double max = 0;
            double[,] Iadj = new double[_highResSizeX, _highResSizeY];
            for (int x = 0; x < _highResSizeX; x++)
            {
                for (int y = 0; y < _highResSizeY; y++)
                {

                    //double v = ((2 * matrixP2[x, y] / 3) + (1 * matrixI2[x, y] / 3));
                    double P2 = ((s0 / s1) * (matrixP2[x, y] - u1 + s1)) + u0 - s0;
                    //double P2 = ((s0 / s1) * (v - u1 + s1)) + u0 - s0;

                    Iadj[x, y] = (double)Math.Sqrt(P2);

                    // Handle cases where result is invalid
                    if (double.IsNaN(Iadj[x, y]))
                    {
                        Iadj[x, y] = 0;
                    }
                    if (Iadj[x, y] > max) max = Iadj[x, y];

                }
            }

            // Scale if necessary
            if (max > 1)
            {
                for (int x = 0; x < _highResSizeX; x++)
                {
                    for (int y = 0; y < _highResSizeY; y++)
                    {
                        double[] rgb = ColorConversion.HSLtoRGB(hslH[x, y], hslS[x, y], Iadj[x, y] / max);
                        fused[0][x, y] = (float)rgb[2];
                        fused[1][x, y] = (float)rgb[1];
                        fused[2][x, y] = (float)rgb[0];
                    }
                }
            }
            else
            {
                for (int x = 0; x < _highResSizeX; x++)
                {
                    for (int y = 0; y < _highResSizeY; y++)
                    {
                        double[] rgb = ColorConversion.HSLtoRGB(hslH[x, y], hslS[x, y], Iadj[x, y]);
                        fused[0][x, y] = (float)rgb[2];
                        fused[1][x, y] = (float)rgb[1];
                        fused[2][x, y] = (float)rgb[0];
                    }
                }
            }

            return new Data3D(fused);
        }
        public override async Task<Data3D> DoFusionAsync()
        {
            return await Task<Data3D>.Run(() => DoFusion()); 
        }
    }

    public class HSLShiftFusion : Pansharpening
    {
        public int WindowSize = 11;

        public bool ShiftCalculationCompleted;
        public Point NewCenter;
        public Point ShiftSize;
        public double DistanceToCenter;

        public HSLShiftFusion(BitmapSource HighRes, BitmapSource LowRes)
            : base(HighRes, LowRes)
        {
        }
        public HSLShiftFusion(float[,] HighRes, float[,] LowRes, Color LowResBaseColor)
            : base(HighRes, LowRes, LowResBaseColor)
        {
        }
        public HSLShiftFusion(Data2D HighRes, Data3D LowRes)
            : base(HighRes, LowRes)
        {
        }

        public override Data3D DoFusion()
        {
            ShiftCalculationCompleted = false;

            int offset = WindowSize / 2;

            Data2D crossCorrelation = new Data2D(WindowSize, WindowSize); ;

            Data2D panCropped = _gray.Crop(offset, offset, _gray.Width - WindowSize, _gray.Height - WindowSize);
            Data2D colGray = (_color.Layers[0] + _color.Layers[1] + _color.Layers[2]) / 3;

            Parallel.For(0, WindowSize, x =>
            {
                Parallel.For(0, WindowSize, y =>
                {
                    Data2D grayCropped = colGray.Crop(x, y, colGray.Width - WindowSize, colGray.Height - WindowSize);

                    crossCorrelation[x, y] = (float)Analysis.CrossCorrelation.Analyze(grayCropped, panCropped);
                });
            });

            //crossCorrelation.Save(@"E:\crosscorrelation.txt", FileType.CSV);

            List<Point> points;
            float maxCorrelation = crossCorrelation.GetMaximum(out points);

            Point centerOfImage = new Point(offset + 1, offset + 1);
            float distanceToCenter = -1;
            Point newCenter = new Point(-1, -1);
            foreach (Point p in points)
            {
                if (distanceToCenter == -1)
                {
                    newCenter = p;
                    distanceToCenter = (float)Math.Sqrt(((newCenter.X - centerOfImage.X) * (newCenter.X - centerOfImage.X)) +
                        ((newCenter.Y - centerOfImage.Y) * (newCenter.Y - centerOfImage.Y)));
                }
                else
                {
                    float distance = (float)Math.Sqrt(((p.X - centerOfImage.X) * (p.X - centerOfImage.X)) +
                        ((p.Y - centerOfImage.Y) * (p.Y - centerOfImage.Y)));
                    if (distance < distanceToCenter)
                    {
                        newCenter = p;
                        distanceToCenter = distance;
                    }
                }
            }

            if (newCenter == new Point(-1, -1))
            {
                newCenter = centerOfImage;
            }

            //Data2D grayCropped = colGray.Crop(x, y, colGray.Width - windowSize, colGray.Height - windowSize);

            Data2D[] cropped = new Data2D[4];
            for (int i = 0; i < 4; i++)
            {
                cropped[i] = _color.Layers[i].Crop((int)newCenter.X, (int)newCenter.Y,
                    _color.Layers[i].Width - WindowSize, _color.Layers[i].Height - WindowSize);
            }

            Data3D cropped3d = new Data3D(cropped);

            ShiftCalculationCompleted = true;
            Point originalCenter = new Point(offset, offset);
            ShiftSize = new Point(Math.Abs(originalCenter.X - newCenter.X), Math.Abs(originalCenter.Y - newCenter.Y));
            NewCenter = newCenter;
            DistanceToCenter = distanceToCenter;

            HSLFusion hsl = new HSLFusion(panCropped, cropped3d);
            hsl.CheckFusion();
            return hsl.DoFusion();
        }
        public override async Task<Data3D> DoFusionAsync()
        {
            return await Task<Data3D>.Run(() => DoFusion()); 
        }
    }

    public class AdaptiveIHSFusion : Pansharpening
    {
        double _lambda = Math.Pow(10, -9);
        double _epsilon = Math.Pow(10, -10);

        protected Data2D _intensityBand;

        public AdaptiveIHSFusion(BitmapSource HighRes, BitmapSource LowRes)
            : base(HighRes, LowRes)
        {
        }
        public AdaptiveIHSFusion(float[,] HighRes, float[,] LowRes, Color LowResBaseColor)
            : base(HighRes, LowRes, LowResBaseColor)
        {
        }
        public AdaptiveIHSFusion(Data2D HighRes, Data3D LowRes)
            : base(HighRes, LowRes)
        {
        }

        public void SetParameters(double Lambda, double Epsilon)
        {
            _lambda = Lambda;
            _epsilon = Epsilon;
        }

        public override Data3D DoFusion()
        {
            Data2D matrixR = _color.Layers[2];
            Data2D matrixG = _color.Layers[1];
            Data2D matrixB = _color.Layers[0];
            Data2D matrixP = _gray;

            //Get maximum values for each color band and scale
            float[] bandCoefficients = new float[3];
            Data2D[] matrixArray = new Data2D[3] { matrixR, matrixG, matrixB };

            for (int i = 0; i < 3; i++)
            {
                matrixArray[i].Normalize(out bandCoefficients[i]);
            }

            //Get maximum value for panchromatic and scale
            float panCoefficient = _gray.Maximum;
            matrixP /= panCoefficient;

            //Data2D edgeData = SobelEdgeDetection.Detect(matrixP, 1.6f);
            Data2D edgeData = ExponentialEdgeDetection.Detect(matrixP);
            //return Imaging.CreateColorScaleImage(edgeData, ColorScaleTypes.Gray);

            //Calculate alpha via Adaptive Coefficients
            double[,] findAlpha = new double[3, 1] { { 1 }, { 1 }, { 1 } };
            double[,] a = new double[3, 3];
            double[,] b = new double[3, 1];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    a[i, j] = (matrixArray[i] * matrixArray[j]).TotalCounts;
                }
                b[i, 0] = (matrixP * matrixArray[i]).TotalCounts;
            }

            float tau = 5f;
            float gamma1 = 1f / 200000f;
            float gamma2 = 1f;
            int iterations = 150000;

            double[,] eye = new double[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
            Matrix mEye = new Matrix(eye);

            Matrix mFindAlpha = new Matrix(findAlpha);
            Matrix mA = new Matrix(a);
            Matrix mB = new Matrix(b);
            Matrix mInv = (mEye + 2 * tau * gamma1 * mA);
            mInv = mInv.Inverse();

            for (int i = 0; i < iterations; i++)
            {
                mFindAlpha = mInv * (mFindAlpha + 2d * tau * gamma2 * MatrixMax(-mFindAlpha, 0) + 2 * tau * gamma1 * mB);
            }

            Data2D matrixI = new Data2D(_highResSizeX, _highResSizeY);
            for (int x = 0; x < _highResSizeX; x++)
            {
                for (int y = 0; y < _highResSizeY; y++)
                {
                    matrixI[x, y] = (float)((matrixR[x, y] * mFindAlpha[0, 0]) + 
                        (matrixG[x, y] * mFindAlpha[1, 0]) + (matrixB[x, y] * mFindAlpha[2, 0]));
                }
            }

            double uP, uI, sP, sI;

            double sumI = 0;
            double sumP = 0;

            double[,] matrixI2 = new double[_highResSizeX, _highResSizeY];
            double[,] matrixP2 = new double[_highResSizeX, _highResSizeY];

            for (int x = 0; x < _highResSizeX; x++)
            {
                for (int y = 0; y < _highResSizeY; y++)
                {
                    matrixI2[x, y] = matrixI[x, y] * matrixI[x, y];

                    matrixP2[x, y] = _gray[x, y] * _gray[x, y];

                    sumI += matrixI2[x, y];
                    sumP += matrixP2[x, y];
                }
            }

            uI = (double)(sumI / (double)(_highResSizeX * _highResSizeY));
            uP = (double)(sumP / (double)(_highResSizeX * _highResSizeY));

            double sumSqI = 0;
            double sumSqP = 0;
            for (int x = 0; x < _highResSizeX; x++)
            {
                for (int y = 0; y < _highResSizeY; y++)
                {
                    double i2 = matrixI2[x, y];
                    double p2 = matrixP2[x, y];

                    sumSqI += (i2 - uI) * (i2 - uI);
                    sumSqP += (p2 - uP) * (p2 - uP);
                }
            }

            sI = (double)Math.Sqrt(sumSqI / (_highResSizeX * _highResSizeY));
            sP = (double)Math.Sqrt(sumSqP / (_highResSizeX * _highResSizeY));

            for (int x = 0; x < _highResSizeX; x++)
            {
                for (int y = 0; y < _highResSizeY; y++)
                {
                    matrixP[x, y] = (float)((matrixP[x, y] - uP) * sI / sP + uI);
                }
            }

            Data2D[] fusedColorData = new Data2D[4];
            for (int i = 0; i < 4; i++)
            {
                fusedColorData[i] = new Data2D(_highResSizeX, _highResSizeY);
            }

            //BGRA
            for (int x = 0; x < _highResSizeX; x++)
            {
                for (int y = 0; y < _highResSizeY; y++)
                {
                    fusedColorData[0][x, y] = matrixB[x, y] + edgeData[x, y] * (matrixP[x, y] - matrixI[x, y]);
                    fusedColorData[1][x, y] = matrixG[x, y] + edgeData[x, y] * (matrixP[x, y] - matrixI[x, y]);
                    fusedColorData[2][x, y] = matrixR[x, y] + edgeData[x, y] * (matrixP[x, y] - matrixI[x, y]);
                }
            }

            //fusedColorData[] BGRA bandCoefficients[]RGB
            fusedColorData[0] *= bandCoefficients[2];
            fusedColorData[1] *= bandCoefficients[1];
            fusedColorData[2] *= bandCoefficients[0];

            return new Data3D(fusedColorData);
        }
        public override async Task<Data3D> DoFusionAsync()
        {
            return await Task<Data3D>.Run(() => DoFusion()); 
        }

        private Matrix MatrixMax(Matrix m, double s)
        {
            double[,] matrix = m.CopyToArray();

            for (int x = 0; x < matrix.GetLength(0); x++)
            {
                for (int y = 0; y < matrix.GetLength(1); y++)
                {
                    matrix[x, y] = Math.Max(matrix[x,y], s);
                }
            }

            return new Matrix(matrix);
        }
    }

    public class GramSchmidtFusion : Pansharpening
    {
        public GramSchmidtFusion(BitmapSource HighRes, BitmapSource LowRes)
            : base(HighRes, LowRes)
        {

        }
        public GramSchmidtFusion(Data2D HighRes, Data3D LowRes)
            : base(HighRes, LowRes)
        {

        }
        public GramSchmidtFusion(float[,] HighRes, float[,] LowRes, Color LowResBaseColor)
            : base(HighRes, LowRes, LowResBaseColor)
        {

        }

        private Data2D gramSchmidtTransform(Data2D d1, Data2D d2)
        {
            Data2D transformed = new Data2D(_lowResSizeX, _lowResSizeY);

            float[,] covariance = MathEx.Covariance(d1.ToVector(), d2.ToVector());
            float[,] covarianceSelf = MathEx.Covariance(d1.ToVector(), d1.ToVector());

            for (int x = 0; x < _lowResSizeX; x++)
            {
                for (int y = 0; y < _lowResSizeY; y++)
                {
                    transformed[x, y] = d2[x, y] - (covariance[x, y] * d1[x, y] / covarianceSelf[x, y]);
                }
            }

            return transformed;
        }
        /// <summary>
        /// Performs a Gram-Schmidt transform on a series of multispectral bands against a panchromatic band.
        /// </summary>
        /// <param name="d">Array of data bands. First element in the array should be the panchromatic band 
        /// followed by n multispectral bands.</param>
        /// <returns>n multipspectral bands transformed</returns>
        private Data2D[] gramSchmidtTransform(Data2D[] d)
        {
            Data2D[] transformed = new Data2D[d.Length - 1];

            for (int i = 0; i < d.Length - 1; i++)
            {
                for (int j = 0; j < i; j++)
                {

                }                
            }

            return transformed;
        }

        public override Data3D DoFusion()
        {
            Data2D simulatedPan = new Data2D(_lowResSizeX, _lowResSizeY);
            for (int x = 0; x < _lowResSizeX; x++)
            {
                for (int y = 0; y < _lowResSizeY; y++)
                {
                    simulatedPan[x, y] = (_colorNotResized[x, y, 0] + _colorNotResized[x, y, 1] + _colorNotResized[x, y, 2]) / 3f;
                }
            }

            Data2D dcfPan = dcFree(simulatedPan);
            Data2D dcfRed = dcFree(_colorNotResized.Layers[2]);
            Data2D dcfGreen = dcFree(_colorNotResized.Layers[1]);
            Data2D dcfBlue = dcFree(_colorNotResized.Layers[0]);

            Data2D ms1P = gramSchmidtTransform(dcfPan, dcfRed);
            Data2D ms2P = gramSchmidtTransform(dcfRed, dcfGreen);
            Data2D ms3P = gramSchmidtTransform(dcfGreen, dcfBlue);

            

            Data2D[] fused = new Data2D[4];
            for (int i = 0; i < 4; i++)
            {
                fused[i] = new Data2D(_highResSizeX, _highResSizeY);
            }

            return new Data3D(fused);
        }
        public override Task<Data3D> DoFusionAsync()
        {
            throw new NotImplementedException();
        }

        private Data2D dcFree(Data2D data)
        {
            float mean = data.Mean;
            Data2D dcFree = new Data2D(data.Width, data.Height);

            for (int x = 0; x < data.Width; x++)
            {
                for (int y = 0; y < data.Height; y++)
                {
                    dcFree[x, y] = data[x, y] - mean;
                }
            }

            return dcFree;
        }
    }

    public class HSLSmoothFusion : HSLFusion
    {
        public HSLSmoothFusion(BitmapSource HighRes, BitmapSource LowRes)
            : base(HighRes, LowRes) { }
        public HSLSmoothFusion(float[,] HighRes, float[,] LowRes, Color LowResBaseColor)
            : base(HighRes, LowRes, LowResBaseColor) { }
        public HSLSmoothFusion(Data2D HighRes, Data3D LowRes)
            : base(HighRes, LowRes) { }

        public override Data3D DoFusion()
        {
            double[,] hslH = new double[_highResSizeX, _highResSizeY];
            double[,] hslS = new double[_highResSizeX, _highResSizeY];
            double[,] hslL = new double[_highResSizeX, _highResSizeY];

            for (int x = 0; x < _highResSizeX; x++)
            {
                for (int y = 0; y < _highResSizeY; y++)
                {
                    double B = _color[x, y, 0];
                    double G = _color[x, y, 1];
                    double R = _color[x, y, 2];

                    double[] hsl = ColorConversion.RGBtoHSL(R, G, B);

                    hslH[x, y] = hsl[0];
                    hslS[x, y] = hsl[1];
                    hslL[x, y] = hsl[2];
                }
            }

            double u1, u0, s1, s0;

            double sumI = 0;
            double sumPS = 0;

            double[,] matrixI2 = new double[_highResSizeX, _highResSizeY];
            double[,] matrixP2 = new double[_highResSizeX, _highResSizeY];
            double[,] matrixPS2 = new double[_highResSizeX, _highResSizeY];

            double[,] smoothed = Filter.MeanSmooth(_gray);

            for (int x = 0; x < _highResSizeX; x++)
            {
                for (int y = 0; y < _highResSizeY; y++)
                {
                    matrixI2[x, y] = hslL[x, y] * hslL[x, y];
                    matrixP2[x, y] = _gray[x, y] * _gray[x, y];
                    matrixPS2[x, y] = smoothed[x, y] * smoothed[x, y];

                    sumI += matrixI2[x, y];
                    sumPS += matrixPS2[x, y];
                }
            }

            u0 = (double)(sumI / (double)(_highResSizeX * _highResSizeY));
            u1 = (double)(sumPS / (double)(_highResSizeX * _highResSizeY));

            double sumSqI = 0;
            double sumSqPS = 0;
            for (int x = 0; x < _highResSizeX; x++)
            {
                for (int y = 0; y < _highResSizeY; y++)
                {
                    double i2 = matrixI2[x, y];
                    double p2 = matrixPS2[x, y];

                    sumSqI += (i2 - u0) * (i2 - u0);
                    sumSqPS += (p2 - u1) * (p2 - u1);
                }
            }

            s0 = (double)Math.Sqrt(sumSqI / (_highResSizeX * _highResSizeY));
            s1 = (double)Math.Sqrt(sumSqPS / (_highResSizeX * _highResSizeY));

            Data2D[] fused = new Data2D[4];
            for (int i = 0; i < 4; i++)
            {
                fused[i] = new Data2D(_highResSizeX, _highResSizeY);
            }

            double max = 0;
            double[,] Iadj = new double[_highResSizeX, _highResSizeY];
            for (int x = 0; x < _highResSizeX; x++)
            {
                for (int y = 0; y < _highResSizeY; y++)
                {
                    double I2 = matrixI2[x, y];
                    double P2 = ((s0 / s1) * (matrixP2[x, y] - u1 + s1)) + u0 - s0;
                    double PS2 = ((s0 / s1) * (matrixPS2[x, y] - u1 + s1)) + u0 - s0;

                    Iadj[x, y] = (double)Math.Sqrt(P2 * I2 / PS2);

                    if (double.IsNaN(Iadj[x, y]))
                    {
                        Iadj[x, y] = 0;
                    }
                    if (Iadj[x, y] > max) max = Iadj[x, y];

                }
            }

            if (max > 1)
            {
                for (int x = 0; x < _highResSizeX; x++)
                {
                    for (int y = 0; y < _highResSizeY; y++)
                    {
                        double[] rgb = ColorConversion.HSLtoRGB(hslH[x, y], hslS[x, y], Iadj[x, y] / max);
                        fused[0][x, y] = (float)rgb[2];
                        fused[1][x, y] = (float)rgb[1];
                        fused[2][x, y] = (float)rgb[0];
                    }
                }
            }
            else
            {
                for (int x = 0; x < _highResSizeX; x++)
                {
                    for (int y = 0; y < _highResSizeY; y++)
                    {
                        double[] rgb = ColorConversion.HSLtoRGB(hslH[x, y], hslS[x, y], Iadj[x, y]);
                        fused[0][x, y] = (float)rgb[2];
                        fused[1][x, y] = (float)rgb[1];
                        fused[2][x, y] = (float)rgb[0];
                    }
                }
            }

            return new Data3D(fused);
        }
        public override async Task<Data3D> DoFusionAsync()
        {
            return await Task<Data3D>.Run(() => DoFusion()); 
        }
    }

    public class PCAFusion : Pansharpening
    {
        public PCAFusion(BitmapSource highRes, BitmapSource lowRes)
            : base(highRes, lowRes)
        {

        }
        public PCAFusion(float[,] highRes, float[,] lowRes, Color lowResBaseColor)
            :base(highRes, lowRes, lowResBaseColor)
        {

        }
        public PCAFusion(Data2D highRes, Data3D lowRes)
            :base(highRes, lowRes)
        {

        }
        
        public override Data3D DoFusion()
        {
            
        }

        public async override Task<Data3D> DoFusionAsync()
        {
            return await Task.Run(() => DoFusion());
        }
    }

    public static class SobelEdgeDetection
    {
        static float[,] sobelX = new float[3, 3]
        {
            {-1f, 0f, 1f},
            {-2f, 0f, 2f},
            {-1f, 0f, 1f}
        };
        static float[,] sobelY = new float[3, 3]
        {
            {-1f, -2f, -1f},
            {0f, 0f, 0f},
            {1f, 2f, 1f}
        };

        public static Data2D Detect(Data2D Original, float Sigma = 1.0f)
        {
            Data2D filtered = Filter.GaussianFilter(Original, Sigma);

            int width = filtered.Width;
            int height = filtered.Height;

            Data2D sobel = new Data2D(width, height);
            Data2D gradX = new Data2D(width, height);
            Data2D gradY = new Data2D(width, height);

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {

                    float px = (sobelX[0, 0] * filtered[x - 1, y - 1]) + (sobelX[0, 1] * filtered[x, y - 1]) +
                        (sobelX[0, 2] * filtered[x + 1, y - 1]) + (sobelX[1, 0] * filtered[x - 1, y]) +
                        (sobelX[1, 1] * filtered[x, y]) + (sobelX[1, 2] * filtered[x + 1, y]) +
                        (sobelX[2, 0] * filtered[x - 1, y + 1]) + (sobelX[2, 1] * filtered[x, y + 1]) +
                        (sobelX[2, 2] * filtered[x + 1, y + 1]);

                    float py = (sobelY[0, 0] * filtered[x - 1, y - 1]) + (sobelY[0, 1] * filtered[x, y - 1]) +
                        (sobelY[0, 2] * filtered[x + 1, y - 1]) + (sobelY[1, 0] * filtered[x - 1, y]) +
                        (sobelY[1, 1] * filtered[x, y]) + (sobelY[1, 2] * filtered[x + 1, y]) +
                        (sobelY[2, 0] * filtered[x - 1, y + 1]) + (sobelY[2, 1] * filtered[x, y + 1]) +
                        (sobelY[2, 2] * filtered[x + 1, y + 1]);

                    gradX[x, y] = px;
                    gradY[x, y] = py;
                }
            }

            Data2D sobelMag = new Data2D(width, height);
            Data2D sobelDir = new Data2D(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float gX = gradX[x, y];
                    float gY = gradY[x, y];
                    
                    sobelMag[x, y] = (float)Math.Sqrt((gX * gX) + (gY * gY));
                    float dir = (float)Math.Atan2(gX, gY);

                    if ((dir < 22.5f && dir >= 0) || (dir >= 157.5f && dir < 202.5f) ||
                        (dir >= 337.5f && dir <= 360f))
                        sobelDir[x, y] = 0;
                    else if ((dir >= 22.5f && dir < 67.5f) || (dir >= 202.5f && dir < 247.5f))
                        sobelDir[x, y] = 45f;
                    else if ((dir >= 67.5f && dir < 112.5f) || (dir >= 247.5f && dir < 292.5f))
                        sobelDir[x, y] = 90f;
                    else
                        sobelDir[x, y] = 135f;
                }
            }

            Data2D sobelMagSup = new Data2D(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    sobelMagSup[x, y] = sobelMag[x, y];
                }
            }


            //http://pythongeek.blogspot.com/2012/06/canny-edge-detection.html
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (sobelDir[x, y] == 0)
                    {
                        if (sobelMag[x, y] <= sobelMag[x, y + 1] ||
                        sobelMag[x, y] <= sobelMag[x, y - 1])
                            sobelMagSup[x, y] = 0;
                    }
                    else if (sobelDir[x, y] == 45)
                    {
                        if (sobelMag[x, y] < sobelMag[x - 1, y + 1] ||
                            sobelMag[x, y] <= sobelMag[x + 1, y - 1])
                            sobelMagSup[x, y] = 0;
                    }
                    else if (sobelDir[x, y] == 90)
                    {
                        if (sobelMag[x, y] <= sobelMag[x + 1, y] ||
                            sobelMag[x, y] <= sobelMag[x - 1, y])
                            sobelMagSup[x, y] = 0;
                    }
                    else
                    {
                        if (sobelMag[x, y] <= sobelMag[x + 1, y + 1] ||
                            sobelMag[x, y] <= sobelMag[x - 1, y - 1])
                            sobelMagSup[x, y] = 0;
                    }
                }
            }

            float sobelMagSupMax = sobelMagSup.Maximum;
            float th = 0.2f * sobelMagSupMax;
            float tl = 0.1f * sobelMagSupMax;

            Data2D gnh = new Data2D(width, height);
            Data2D gnl = new Data2D(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (sobelMagSup[x, y] >= th) gnh[x, y] = sobelMagSup[x, y];
                    if (sobelMagSup[x, y] >= tl) gnl[x, y] = sobelMagSup[x, y];
                }
            }
            gnl += (gnh * -1);

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (gnh[x, y] != 0)
                    {
                        gnh[x, y] = 1;
                        traverse(x, y, ref gnh, ref gnl);
                    }
                }
            }
            gnh.Refresh();
            return gnh;
        }

        private static void traverse(int i, int j, ref Data2D gnh, ref Data2D gnl)
        {
            for (int k = 0; k < 8; k++)
            {
                if (gnh[i + traverseX[k], j + traverseY[k]] == 0 
                    && gnl[i + traverseX[k], j + traverseY[k]] != 0)
                {
                    gnh[i + traverseX[k], j + traverseY[k]] = 1;
                    traverse(i + traverseX[k], j + traverseY[k], ref gnh, ref gnl);
                }
            }
        }
        private static int[] traverseX = new int[8] { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static int[] traverseY = new int[8] { -1, -1, -1, 0, 0, 1, 1, 1 };
    }
    public static class ExponentialEdgeDetection
    {
        public static Data2D Detect(Data2D Original, 
            double Lambda = 0.000000001d, double Epsilon = 0.0000000001d)
        {
            Data2D fx = gradientFx(Original);
            Data2D fy = gradientFy(Original);

            //using (System.IO.StreamWriter stream = new System.IO.StreamWriter(@"E:\fx.txt"))
            //{                
            //    for (int y = 0; y < fx.Height; y++)
            //    {
            //        StringBuilder sb = new StringBuilder();
            //        for (int x = 0; x < fx.Width; x++)
            //        {
            //            sb.Append(fx[x, y].ToString() + ",");
            //        }
            //        sb.Remove(sb.Length - 1, 1);
            //        stream.WriteLine(sb);
            //    }                
            //}
            //using (System.IO.StreamWriter stream = new System.IO.StreamWriter(@"E:\fy.txt"))
            //{
            //    for (int y = 0; y < fx.Height; y++)
            //    {
            //        StringBuilder sb = new StringBuilder();
            //        for (int x = 0; x < fx.Width; x++)
            //        {
            //            sb.Append(fy[x, y].ToString() + ",");
            //        }
            //        sb.Remove(sb.Length - 1, 1);
            //        stream.WriteLine(sb);
            //    }
            //}
            Data2D u = Data2D.Sqrt(Data2D.Squared(fx) + Data2D.Squared(fy));

            Data2D ones = Data2D.Ones(Original.Width, Original.Height);
            Data2D uAbs = Data2D.Abs(u);
            Data2D lambda = new Data2D(Original.Width, Original.Height, (float)Lambda);
            Data2D f = Data2D.Exp((lambda / ((uAbs * uAbs * uAbs * uAbs) + Epsilon)) * -1f);

            return f;
        }

        private static Data2D gradientFx(Data2D d)
        {
            Data2D gradX = new Data2D(d.Width, d.Height);
            for (int x = 0; x < d.Width; x++)
            {
                for (int y = 0; y < d.Height; y++)
                {
                    float grad = 0;
                    if (x == 0)
                    {
                        grad = (d[x + 1, y] - d[x, y]) / 1f;
                    }
                    else if (x == d.Width - 1)
                    {
                        grad = (d[x, y] - d[x - 1, y]) / 1f;
                    }
                    else
                    {
                        grad = (d[x + 1, y] - d[x - 1, y]) / 2f;
                    }

                    gradX[x, y] = grad;
                }
            }
            return gradX;
        }
        private static Data2D gradientFy(Data2D d)
        {
            Data2D gradY = new Data2D(d.Width, d.Height);
            for (int x = 0; x < d.Width; x++)
            {
                for (int y = 0; y < d.Height; y++)
                {
                    float grad = 0;
                    if (y == 0)
                    {
                        grad = (d[x, y + 1] - d[x, y]) / 1f;
                    }
                    else if (y == d.Height - 1)
                    {
                        grad = (d[x, y] - d[x, y - 1]) / 1f;
                    }
                    else
                    {
                        grad = (d[x, y + 1] - d[x, y - 1]) / 2f;
                    }

                    gradY[x, y] = grad;
                }
            }
            return gradY;
        }
    }

    public enum FusionType
    {
        HSL, 

        [Description("HSL Shift")]
        HSLShift,
        
        [Description("HSL Smooth")]
        HSLSmooth, 
        
        [Description("Weighted Average")]
        WeightedAverage,

        Adaptive,

        PCA
    }


    namespace Analysis
    {
        public static class CrossCorrelation
        {
            public static CrossCorrelationResults Analyze(BitmapSource LowRes, BitmapSource HighRes, BackgroundWorker bw)
            {
                Data3D lowRes = ImageHelper.ConvertToData3D(LowRes);
                Data3D highRes = ImageHelper.ConvertToData3D(HighRes);

                double r = AnalyzeCC(lowRes.Layers[2], highRes.Layers[2]);
                if (bw != null) bw.ReportProgress(Percentage.GetPercent(1, 3));
                double g = AnalyzeCC(lowRes.Layers[1], highRes.Layers[1]);
                if (bw != null) bw.ReportProgress(Percentage.GetPercent(2, 3));
                double b = AnalyzeCC(lowRes.Layers[0], highRes.Layers[0]);
                if (bw != null) bw.ReportProgress(Percentage.GetPercent(3, 3));

                return new CrossCorrelationResults()
                {
                    R = r,
                    G = g,
                    B = b
                };
            }
            public static double Analyze(Data2D LowRes, Data2D HighRes)
            {
                return AnalyzeCC(LowRes, HighRes);
            }
            private static double AnalyzeCC(Data2D LowRes, Data2D HighRes)
            {
                int sizeX = HighRes.Width;
                int sizeY = HighRes.Height;

                Data2D Resized = LowRes.Resize(sizeX, sizeY);

                double sumHigh = 0;
                double sumResi = 0;

                for (int x = 0; x < sizeX; x++)
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                        sumHigh += HighRes[x, y];
                        sumResi += Resized[x, y];
                    }
                }

                //sumHigh = 0;
                //sumResi = 0;

                double meanHigh = sumHigh / (sizeX * sizeY);
                double meanResi = sumResi / (sizeX * sizeY);

                //Term1 = Ai - uA
                //Term2 = Bi - uB
                //TermA = Term1 * Term2
                //TermB = Term1^2
                //TermC = Term2^2

                double termA = 0;
                double termB = 0;
                double termC = 0;

                for (int x = 0; x < sizeX; x++)
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                        double term1 = HighRes[x, y] - meanHigh;
                        double term2 = Resized[x, y] - meanResi;

                        termA += (term1 * term2);
                        termB += (term1 * term1);
                        termC += (term2 * term2);
                    }
                }

                double cc = termA / Math.Sqrt(termB * termC);
                return cc;
            }
            private static double AnalyzeCCNew(Data2D LowRes, Data2D HighRes)
            {
                int sizeX = HighRes.Width;
                int sizeY = HighRes.Height;

                Data2D Resized = LowRes.Resize(sizeX, sizeY);

                float meanHigh = HighRes.Mean;
                float meanLow = LowRes.Mean;

                float num = 0;
                float sHigh = 0;
                float sLow = 0;
                for (int x = 0; x < sizeX; x++)
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                        num += ((HighRes[x, y] - meanHigh) * (LowRes[x, y] - meanLow));
                        sHigh += ((HighRes[x, y] - meanHigh) * (HighRes[x, y] - meanHigh));
                        sLow += ((LowRes[x, y] - meanLow) * (LowRes[x, y] - meanLow));
                    }
                }

                float denom = (float)Math.Sqrt(sHigh * sLow);
                return num / denom;
            }
            public static async Task<CrossCorrelationResults> AnalyzeAsync(BitmapSource LowRes, BitmapSource HighRes)
            {
                Data3D lowRes = ImageHelper.ConvertToData3D(LowRes);
                Data3D highRes = ImageHelper.ConvertToData3D(HighRes);

                double[] cc = await Task<double[]>.Run(() =>
                    {
                        double[] results = new double[3];
                        for (int i = 0; i < 3; i++)
                        {
                            results[i] = AnalyzeCC(lowRes.Layers[i], highRes.Layers[i]);
                        }
                        return results;
                    });

                return new CrossCorrelationResults()
                {
                    R = cc[2],
                    G = cc[1],
                    B = cc[0]
                };
            }
        }

        public struct CrossCorrelationResults
        {
            public double R;
            public double G;
            public double B;
        }
        public static class QPS
        {
            public static QPSResults Analyze(BitmapSource Panchromatic,
                BitmapSource MultiSpectral, BitmapSource Sharpened, BackgroundWorker bw, int WindowSize = 1)
            {
                Data2D panchromatic = ImageHelper.ConvertToData2D(Panchromatic);
                Data3D ms = ImageHelper.ConvertToData3D(MultiSpectral);
                Data3D sharp = ImageHelper.ConvertToData3D(Sharpened);

                int highX = sharp.Width;
                int highY = sharp.Height;
                int lowX = ms.Width;
                int lowY = ms.Height;

                double total = lowX + lowX + highX;
                double pos = 0;

                Data3D sharpDown = new Data3D(lowX, lowY, 4);

                double factorX = highX / lowX;
                double factorY = highY / lowY;

                for (int x = 0; x < lowX; x++)
                {
                    for (int y = 0; y < lowY; y++)
                    {
                        int pixelX = (int)((double)x * factorX);
                        int pixelY = (int)((double)y * factorY);

                        sharpDown[x, y] = sharp[pixelX, pixelY];                        
                    }
                    pos++;
                    bw.ReportProgress(Percentage.GetPercent(pos, total));
                }

                double qRedSum = 0;
                double qGreenSum = 0;
                double qBlueSum = 0;

                double count = 0;

                Parallel.For(0, lowX, x =>
                {
                    Parallel.For(0, lowY, y =>
                    {
                        int startX = x - (WindowSize / 2);
                        int endX = x + (WindowSize / 2);
                        int startY = y - (WindowSize / 2);
                        int endY = y + (WindowSize / 2);

                        if (startX < 0 || endX >= lowX || startY < 0 || endY >= lowY) return;

                        double qRed = 0;
                        double qBlue = 0;
                        double qGreen = 0;

                        double _sumMsRed = 0;
                        double _sumSharpRed = 0;
                        double _sumMsGreen = 0;
                        double _sumSharpGreen = 0;
                        double _sumMsBlue = 0;
                        double _sumSharpBlue = 0;

                        double _sumRR = 0;
                        double _sumGG = 0;
                        double _sumBB = 0;

                        for (int i = 0; i < WindowSize; i++)
                        {
                            for (int j = 0; j < WindowSize; j++)
                            {
                                _sumMsRed += ms[startX + i, startY + j, 2];
                                _sumMsGreen += ms[startX + i, startY + j, 1];
                                _sumMsBlue += ms[startX + i, startY + j, 0];
                                _sumSharpRed += sharp[startX + i, startY + j, 2];
                                _sumSharpGreen += sharp[startX + i, startY + j, 1];
                                _sumSharpBlue += sharp[startX + i, startY + j, 0];

                                _sumRR += (ms[startX + i, startY + j, 2] * sharpDown[startX + i, startY + j, 2]);
                                _sumGG += (ms[startX + i, startY + j, 1] * sharpDown[startX + i, startY + j, 1]);
                                _sumBB += (ms[startX + i, startY + j, 0] * sharpDown[startX + i, startY + j, 0]);
                            }
                        }

                        double _size = WindowSize * WindowSize;
                        double _mean_MsRed = _sumMsRed / _size;
                        double _mean_MsGreen = _sumMsGreen / _size;
                        double _mean_MsBlue = _sumMsBlue / _size;
                        double _mean_SharpRed = _sumSharpRed / _size;
                        double _mean_SharpGreen = _sumSharpGreen / _size;
                        double _mean_SharpBlue = _sumSharpBlue / _size;

                        double _meanRR = _sumRR / _size;
                        double _meanGG = _sumGG / _size;
                        double _meanBB = _sumBB / _size;

                        double _diffMsRed = 0;
                        double _diffSharpRed = 0;
                        double _diffMsGreen = 0;
                        double _diffSharpGreen = 0;
                        double _diffMsBlue = 0;
                        double _diffSharpBlue = 0;

                        for (int i = 0; i < WindowSize; i++)
                        {
                            for (int j = 0; j < WindowSize; j++)
                            {
                                _diffMsRed += ((ms[startX + i, startY + j, 2] - _mean_MsRed) *
                                    (ms[startX + i, startY + j, 2] - _mean_MsRed));
                                _diffMsGreen += ((ms[startX + i, startY + j, 1] - _mean_MsGreen) *
                                    (ms[startX + i, startY + j, 1] - _mean_MsGreen));
                                _diffMsBlue += ((ms[startX + i, startY + j, 0] - _mean_MsBlue) *
                                    (ms[startX + i, startY + j, 0] - _mean_MsBlue));

                                _diffSharpRed += ((sharpDown[startX + i, startY + j, 2] - _mean_SharpRed) *
                                    (sharpDown[startX + i, startY + j, 2] - _mean_SharpRed));
                                _diffSharpGreen += ((sharpDown[startX + i, startY + j, 1] - _mean_SharpGreen) *
                                    (sharpDown[startX + i, startY + j, 1] - _mean_SharpGreen));
                                _diffSharpBlue += ((sharpDown[startX + i, startY + j, 0] - _mean_SharpBlue) *
                                    (sharpDown[startX + i, startY + j, 0] - _mean_SharpBlue));
                            }
                        }

                        double _varianceMsRed = (_diffMsRed / _size);
                        double _varianceMsGreen = (_diffMsGreen / _size);
                        double _varianceMsBlue = (_diffMsBlue / _size);
                        double _varianceSharpRed = (_diffSharpRed / _size);
                        double _varianceSharpGreen = (_diffSharpGreen / _size);
                        double _varianceSharpBlue = (_diffSharpBlue / _size);

                        double _covR = (_meanRR - (_mean_MsRed * _mean_SharpRed));
                        double _covG = (_meanGG - (_mean_MsGreen * _mean_SharpGreen));
                        double _covB = (_meanBB - (_mean_MsBlue * _mean_SharpBlue));

                        qRed = CalculateQualityIndex(_varianceMsRed, _varianceSharpRed, _mean_MsRed, _mean_SharpRed, _covR);
                        qGreen = CalculateQualityIndex(_varianceMsGreen, _varianceSharpGreen, _mean_MsGreen, _mean_SharpGreen, _covG);
                        qBlue = CalculateQualityIndex(_varianceMsBlue, _varianceSharpBlue, _mean_MsBlue, _mean_SharpBlue, _covB);

                        if (double.IsNaN(qRed)) qRed = 0;
                        if (double.IsNaN(qGreen)) qGreen = 0;
                        if (double.IsNaN(qBlue)) qBlue = 0;

                        qRedSum += qRed;
                        qGreenSum += qGreen;
                        qBlueSum += qBlue;

                        count++;
                    });
                    pos++;
                    bw.ReportProgress(Percentage.GetPercent(pos, total));
                });

                double qRedFinal = (double)(qRedSum / count);
                double qGreenFinal = (double)(qGreenSum / count);
                double qBlueFinal = (double)(qBlueSum / count);

                count = 0;

                double ccRedSum = 0;
                double ccGreenSum = 0;
                double ccBlueSum = 0;

                Parallel.For(0, highX, x =>
                {
                    Parallel.For(0, highY, y =>
                    {
                        int startX = x - (WindowSize / 2);
                        int endX = x + (WindowSize / 2);
                        int startY = y - (WindowSize / 2);
                        int endY = y + (WindowSize / 2);

                        if (startX < 0 || endX >= highX || startY < 0 || endY >= highY) return;

                        double _sumPan = 0;
                        double _sumRed = 0;
                        double _sumGreen = 0;
                        double _sumBlue = 0;

                        double ccRed, ccBlue, ccGreen;

                        for (int i = 0; i < WindowSize; i++)
                        {
                            for (int j = 0; j < WindowSize; j++)
                            {
                                _sumPan += panchromatic[startX + i, startY + j];
                                _sumRed += sharp[startX + i, startY + j, 2];
                                _sumGreen += sharp[startX + i, startY + j, 1];
                                _sumBlue += sharp[startX + i, startY + j, 0];
                            }
                        }

                        double size = WindowSize * WindowSize;

                        double _meanPan = _sumPan / size;
                        double _meanRed = _sumRed / size;
                        double _meanGreen = _sumGreen / size;
                        double _meanBlue = _sumBlue / size;

                        double _numRed = 0;
                        double _numGreen = 0;
                        double _numBlue = 0;
                        double _denRed1 = 0;
                        double _denGreen1 = 0;
                        double _denBlue1 = 0;
                        double _denRed2 = 0;
                        double _denGreen2 = 0;
                        double _denBlue2 = 0;

                        for (int i = 0; i < WindowSize; i++)
                        {
                            for (int j = 0; j < WindowSize; j++)
                            {
                                _numRed += ((panchromatic[startX + i, startY + j] - _meanPan) * (sharp[startX + i, startY + j, 2] - _meanRed));
                                _numGreen += ((panchromatic[startX + i, startY + j] - _meanPan) * (sharp[startX + i, startY + j, 1] - _meanGreen));
                                _numBlue += ((panchromatic[startX + i, startY + j] - _meanPan) * (sharp[startX + i, startY + j, 0] - _meanBlue));

                                _denRed1 += ((panchromatic[startX + i, startY + j] - _meanPan) * (panchromatic[startX + i, startY + j] - _meanPan));
                                _denGreen1 += ((panchromatic[startX + i, startY + j] - _meanPan) * (panchromatic[startX + i, startY + j] - _meanPan));
                                _denBlue1 += ((panchromatic[startX + i, startY + j] - _meanPan) * (panchromatic[startX + i, startY + j] - _meanPan));

                                _denRed2 += ((sharp[startX + i, startY + j, 2] - _meanRed) * (sharp[startX + i, startY + j, 2] - _meanRed));
                                _denGreen2 += ((sharp[startX + i, startY + j, 1] - _meanGreen) * (sharp[startX + i, startY + j, 1] - _meanGreen));
                                _denBlue2 += ((sharp[startX + i, startY + j, 0] - _meanBlue) * (sharp[startX + i, startY + j, 0] - _meanBlue));
                            }
                        }

                        ccRed = (double)(_numRed / Math.Sqrt(_denRed1 * _denRed2));
                        ccGreen = (double)(_numGreen / Math.Sqrt(_denGreen1 * _denGreen2));
                        ccBlue = (double)(_numBlue / Math.Sqrt(_denBlue1 * _denBlue2));

                        if (double.IsNaN(ccRed)) ccRed = 0;
                        if (double.IsNaN(ccGreen)) ccGreen = 0;
                        if (double.IsNaN(ccBlue)) ccBlue = 0;

                        ccRedSum += ccRed;
                        ccGreenSum += ccGreen;
                        ccBlueSum += ccBlue;

                        count++;
                    });
                    pos++;
                    bw.ReportProgress(Percentage.GetPercent(pos, total));
                });

                double ccRedFinal = (double)(ccRedSum / count);
                double ccGreenFinal = (double)(ccGreenSum / count);
                double ccBlueFinal = (double)(ccBlueSum / count);

                double qpsFinal = ((qRedFinal + qBlueFinal + qGreenFinal) / 3) * ((ccRedFinal + ccBlueFinal + ccGreenFinal) / 3);

                double[] result = new double[7]
                {
                    qRedFinal, qGreenFinal, qBlueFinal,
                    ccRedFinal, ccGreenFinal, ccBlueFinal,
                    qpsFinal
                };

                return new QPSResults(result);
            }

            private static double CalculateQualityIndex(double SigmaF, double SigmaG, double MuF, double MuG, double Covariance)
            {
                double _sigmaFG = Covariance;

                double _term1 = _sigmaFG / (SigmaF * SigmaG);
                double _term2 = (2d * MuF * MuG) / (MuF + MuG);
                double _term3 = (2d * SigmaF * SigmaG) / (((MuF * MuF) + (MuG * MuG)) * (SigmaF + SigmaG));

                return (double)(_term1 * _term2 * _term3);
            }
        }

        public struct QPSResults
        {
            public double qR;
            public double qG;
            public double qB;
            public double ccR;
            public double ccG;
            public double ccB;
            public double QPS;

            public QPSResults(double[] values)
            {
                qR = values[0];
                qG = values[1];
                qB = values[2];
                ccR = values[3];
                ccG = values[4];
                ccB = values[5];
                QPS = values[6];
            }
        }
    }
}
