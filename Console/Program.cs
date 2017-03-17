using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using ImagingSIMS.Direct3DRendering;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Converters;
using ImagingSIMS.Data.Spectra;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Device = SharpDX.Direct3D11.Device;
using ImagingSIMS.Common;
using ImagingSIMS.Data.Imaging;

using Accord;
using Accord.Imaging;
using Accord.Math;

using System.Globalization;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Press Enter to begin...");
            //Console.ReadLine();

            var inputFixedImage = ImageHelper.BitmapSourceFromFile(@"C:\Users\taro148\AppData\Roaming\ImagingSIMS\plugins\imageregistration\transfer\inputFixedImage.bmp");
            var inputMovingImage = ImageHelper.BitmapSourceFromFile(@"C:\Users\taro148\AppData\Roaming\ImagingSIMS\plugins\imageregistration\transfer\inputMovingImage.bmp");
            var fixedPointList = PointSet.PointSetFromFile(@"C:\Users\taro148\AppData\Roaming\ImagingSIMS\plugins\imageregistration\transfer\fixedPointList.pts");
            var movingPointList = PointSet.PointSetFromFile(@"C:\Users\taro148\AppData\Roaming\ImagingSIMS\plugins\imageregistration\transfer\movingPointsList.pts");

            var matrixFixedImage = ImageHelper.ConvertToData3D(inputFixedImage);
            var matrixMovingImage = ImageHelper.ConvertToData3D(inputMovingImage);

            int numPoints = fixedPointList.Count;

            double[,] fixedPointsNorm = new double[fixedPointList.Count, 2];
            double[,] movingPointsNorm = new double[movingPointList.Count, 2];

            double[,] fixedPoints = new double[fixedPointList.Count, 2];
            double[,] movingPoints = new double[movingPointList.Count, 2];

            IntPoint[] fixedIntPoints = new IntPoint[numPoints];
            IntPoint[] movingIntPoints = new IntPoint[numPoints];

            for (int i = 0; i < fixedPointList.Count; i++)
            {
                fixedPointsNorm[i, 0] = fixedPointList[i].X;
                fixedPointsNorm[i, 1] = fixedPointList[i].Y;

                movingPointsNorm[i, 0] = movingPointList[i].X;
                movingPointsNorm[i, 1] = movingPointList[i].Y;

                fixedPoints[i, 0] = fixedPointList[i].X * matrixFixedImage.Width;
                fixedPoints[i, 1] = fixedPointList[i].Y * matrixFixedImage.Height;

                movingPoints[i, 0] = movingPointList[i].X * matrixMovingImage.Width;
                movingPoints[i, 1] = movingPointList[i].Y * matrixMovingImage.Height;

                fixedIntPoints[i] = new IntPoint((int)(fixedPointList[i].X * matrixFixedImage.Width), (int)(fixedPointList[i].Y * matrixFixedImage.Height));
                movingIntPoints[i] = new IntPoint((int)(movingPointList[i].X * matrixMovingImage.Width), (int)(movingPointList[i].Y * matrixMovingImage.Height));
            }

            RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.001, 0.99);
            var transform = ransac.Estimate(fixedIntPoints, movingIntPoints).ToDoubleArray();

            int transformedWidth = matrixFixedImage.Width;
            int transformedHeight = matrixFixedImage.Height;

            var matrixTransformed = new Data3D(transformedWidth, transformedHeight, 4);
            for (int x = 0; x < transformedWidth; x++)
            {
                for (int y = 0; y < transformedHeight; y++)
                {
                    matrixTransformed[x, y, 3] = 255;

                    var point = new double[] { x, y, 1 };

                    var transformedPoint = transform.Dot(point);

                    var hx = transformedPoint[0] / transformedPoint[2];
                    var hy = transformedPoint[1] / transformedPoint[2];

                    if (hx < 0 || hx >= transformedWidth || hy < 0 || hy >= transformedHeight) continue;

                    for (int z = 0; z < 3; z++)
                    {
                        matrixTransformed[x, y, z] = matrixMovingImage.Layers[z].Sample(hx, hy);
                    }
                }
            }

            var bsTransformed = ImageHelper.CreateImage(matrixTransformed);
            bsTransformed.Save(@"C:\Users\taro148\AppData\Roaming\ImagingSIMS\plugins\imageregistration\transfer\transformed.bmp");

            Console.Write("Press Enter to exit.");
            Console.ReadLine();
        }

        private static void main_MatrixMath()
        {
            var inputFixedImage = ImageHelper.BitmapSourceFromFile(@"C:\Users\taro148\AppData\Roaming\ImagingSIMS\plugins\imageregistration\transfer\inputFixedImage.bmp");
            var inputMovingImage = ImageHelper.BitmapSourceFromFile(@"C:\Users\taro148\AppData\Roaming\ImagingSIMS\plugins\imageregistration\transfer\inputMovingImage.bmp");
            var fixedPointList = PointSet.PointSetFromFile(@"C:\Users\taro148\AppData\Roaming\ImagingSIMS\plugins\imageregistration\transfer\fixedPointList.pts");
            var movingPointList = PointSet.PointSetFromFile(@"C:\Users\taro148\AppData\Roaming\ImagingSIMS\plugins\imageregistration\transfer\movingPointsList.pts");

            var matrixFixedImage = ImageHelper.ConvertToData3D(inputFixedImage);
            var matrixMovingImage = ImageHelper.ConvertToData3D(inputMovingImage);

            double[,] fixedPointsNorm = new double[fixedPointList.Count, 2];
            double[,] movingPointsNorm = new double[movingPointList.Count, 2];

            double[,] fixedPoints = new double[fixedPointList.Count, 2];
            double[,] movingPoints = new double[movingPointList.Count, 2];
            for (int i = 0; i < fixedPointList.Count; i++)
            {
                fixedPointsNorm[i, 0] = fixedPointList[i].X;
                fixedPointsNorm[i, 1] = fixedPointList[i].Y;

                movingPointsNorm[i, 0] = movingPointList[i].X;
                movingPointsNorm[i, 1] = movingPointList[i].Y;

                fixedPoints[i, 0] = fixedPointList[i].X * matrixFixedImage.Width;
                fixedPoints[i, 1] = fixedPointList[i].Y * matrixFixedImage.Height;

                movingPoints[i, 0] = movingPointList[i].X * matrixMovingImage.Width;
                movingPoints[i, 1] = movingPointList[i].Y * matrixMovingImage.Height;
            }

            // [row, col]

            var numPoints = fixedPointList.Count;

            var uv_mat = new double[3, 3];
            var uv_vec = new double[3];
            var xy_mat = new double[3, 3];
            var xy_vec = new double[3];

            for (int i = 0; i < 3; i++)
            {
                xy_mat[0, i] = fixedPointList[i].X;
                xy_mat[1, i] = fixedPointList[i].Y;
                xy_mat[2, i] = 1;

                uv_mat[0, i] = movingPointList[i].X;
                uv_mat[1, i] = movingPointList[i].Y;
                uv_mat[2, i] = 1;
            }

            xy_vec[0] = fixedPointList[3].X;
            xy_vec[1] = fixedPointList[3].Y;
            xy_vec[2] = 1;

            uv_vec[0] = movingPointList[3].X;
            uv_vec[1] = movingPointList[3].Y;
            uv_vec[2] = 1;

            // lambda, mu, tao
            var xy_coeffs = xy_mat.Solve(xy_vec);
            var uv_coeffs = uv_mat.Solve(uv_vec);

            var xy_basisMat = new double[3, 3];
            var uv_basisMat = new double[3, 3];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    xy_basisMat[i, j] = xy_coeffs[j] * xy_mat[i, j];
                    uv_basisMat[i, j] = uv_coeffs[j] * uv_mat[i, j];
                }
            }

            var combined = uv_basisMat.DotWithTransposed(xy_basisMat);

            var dFixed = ImageHelper.ConvertToData3D(inputFixedImage);
            var dMoving = ImageHelper.ConvertToData3D(inputMovingImage);

            var transformed = new Data3D(dFixed.Width, dFixed.Height, 4);
            for (int x = 0; x < dFixed.Width; x++)
            {
                for (int y = 0; y < dFixed.Height; y++)
                {
                    var inputCoord = new double[] { x, y, 1 };
                    var transformedCoord = combined.Dot(inputCoord);

                    double tx = transformedCoord[0] * matrixFixedImage.Width / transformedCoord[2];
                    double ty = transformedCoord[1] * matrixFixedImage.Height / transformedCoord[2];

                    int xx = (int)tx;
                    int yy = (int)ty;

                    if (xx < 0 || xx >= dFixed.Width - 1 || yy < 0 || yy >= dFixed.Height)
                        continue;

                    for (int z = 0; z < 3; z++)
                    {
                        transformed[xx, yy, z] = dFixed[x, y, z];
                    }

                    //double xDiff = tx - xx;
                    //double yDiff = ty - yy;

                    //for (int z = 0; z < 4; z++)
                    //{

                    //}
                }
            }

            var bsTransformed = ImageHelper.CreateImage(transformed);
            bsTransformed.Save(@"C:\Users\taro148\AppData\Roaming\ImagingSIMS\plugins\imageregistration\transfer\transformed.bmp");
        }
        private static void main_loadNanoSIMS()
        {
            string filePath = @"D:\Data\NanoSIMS\20141230_HL_Pilot_8_2.im";
            using (Stream stream = File.OpenRead(filePath))
            {
                BinaryReader br = new BinaryReader(stream);

                string tempString = string.Empty;

                CamecaNanoSIMSHeader header = new CamecaNanoSIMSHeader();

                header.Realease = br.ReadInt32();
                header.AnalysisType = br.ReadInt32();
                header.HeaderSize = br.ReadInt32();
                header.SampleType = br.ReadInt32();
                header.DataInlcuded = br.ReadInt32();
                header.PositionX = br.ReadInt32();
                header.PositionY = br.ReadInt32();
                tempString = new string(br.ReadChars(32));
                header.AnalysisName = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);
                tempString = new string(br.ReadChars(16));
                header.UserName = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);
                header.PositionZ = br.ReadInt32();

                int unusedInt = br.ReadInt32();
                unusedInt = br.ReadInt32();
                unusedInt = br.ReadInt32();

                tempString = new string(br.ReadChars(16));
                string date = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);
                tempString = new string(br.ReadChars(16));
                string time = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);

                header.AnalysisTime = CamecaNanoSIMSHeaderPart.ParseDateAndTimeStrings(date, time);

                CamecaNanoSIMSMaskImage mask = new CamecaNanoSIMSMaskImage();

                tempString = new string(br.ReadChars(16));
                mask.FileName = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);
                mask.AnalysisDuration = br.ReadInt32();
                mask.CycleNumber = br.ReadInt32();
                mask.ScanType = br.ReadInt32();
                mask.Magnification = br.ReadInt16();
                mask.SizeType = br.ReadInt16();
                mask.SizeDetector = br.ReadInt16();

                short unusedShort = br.ReadInt16();
                mask.BeamBlanking = br.ReadInt32();
                mask.Sputtering = br.ReadInt32();
                mask.SputteringDuration = br.ReadInt32();
                mask.AutoCalibrationInAnalysis = br.ReadInt32();

                CamecaNanoSIMSAutoCal autoCal = new CamecaNanoSIMSAutoCal();
                tempString = new string(br.ReadChars(64));
                autoCal.Mass = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);
                autoCal.Begin = br.ReadInt32();
                autoCal.Period = br.ReadInt32();
                mask.AutoCal = autoCal;

                mask.SigReference = br.ReadInt32();

                CamecaNanoSIMSSigRef sigRef = new CamecaNanoSIMSSigRef();
                CamecaNanoSIMSPolyatomic polyatomic = new CamecaNanoSIMSPolyatomic();
                polyatomic.FlagNumeric = br.ReadInt32();
                polyatomic.NumericValue = br.ReadInt32();
                polyatomic.NumberElements = br.ReadInt32();
                polyatomic.NumberCharges = br.ReadInt32();
                polyatomic.Charge = new string(br.ReadChars(1));
                tempString = new string(br.ReadChars(64));
                polyatomic.MassLabel = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);
                polyatomic.Tablets = new CamecaNanoSIMSTablets[5];
                for (int i = 0; i < 5; i++)
                {
                    polyatomic.Tablets[i] = new CamecaNanoSIMSTablets()
                    {
                        NumberElements = br.ReadInt32(),
                        NumberIsotopes = br.ReadInt32(),
                        Quantity = br.ReadInt32()
                    };
                }

                string unusedString = new string(br.ReadChars(3));

                sigRef.Polyatomic = polyatomic;
                sigRef.Detector = br.ReadInt32();
                sigRef.Offset = br.ReadInt32();
                sigRef.Quantity = br.ReadInt32();

                mask.SigRef = sigRef;
                mask.NumberMasses = br.ReadInt32();

                int tabMassPointer = 0;
                int numberTabMasses = 10;
                if (header.Realease >= 4108)
                {
                    numberTabMasses = 60;
                }

                for (int i = 0; i < numberTabMasses; i++)
                {
                    tabMassPointer = br.ReadInt32();
                    if (tabMassPointer > 0)
                    {
                        Console.WriteLine($"i {i}: {tabMassPointer}");
                    }
                }

                string[] massNames = new string[mask.NumberMasses];
                string[] massSymbols = new string[mask.NumberMasses];
                CamecaNanoSIMSTabMass[] masses = new CamecaNanoSIMSTabMass[mask.NumberMasses];
                for (int i = 0; i < mask.NumberMasses; i++)
                {
                    CamecaNanoSIMSTabMass mass = new CamecaNanoSIMSTabMass();
                    unusedInt = br.ReadInt32();
                    unusedInt = br.ReadInt32();
                    mass.Amu = br.ReadDouble();
                    mass.MatrixOrTrace = br.ReadInt32();
                    mass.Detector = br.ReadInt32();
                    mass.WaitingTime = br.ReadDouble();
                    mass.CountingTime = br.ReadDouble();
                    mass.Offset = br.ReadInt32();
                    mass.MagField = br.ReadInt32();

                    CamecaNanoSIMSPolyatomic poly = new CamecaNanoSIMSPolyatomic();
                    poly.FlagNumeric = br.ReadInt32();
                    poly.NumericValue = br.ReadInt32();
                    poly.NumberElements = br.ReadInt32();
                    poly.NumberCharges = br.ReadInt32();
                    poly.Charge = new string(br.ReadChars(1));
                    tempString = new string(br.ReadChars(64));
                    poly.MassLabel = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);
                    poly.Tablets = new CamecaNanoSIMSTablets[5];
                    for (int j = 0; j < 5; j++)
                    {
                        poly.Tablets[j] = new CamecaNanoSIMSTablets()
                        {
                            NumberElements = br.ReadInt32(),
                            NumberIsotopes = br.ReadInt32(),
                            Quantity = br.ReadInt32()
                        };
                    }
                    unusedString = new string(br.ReadChars(3));
                    mass.Polyatomic = poly;

                    massNames[i] = mass.Amu.ToString("0.00");
                    massSymbols[i] = string.IsNullOrEmpty(mass.Polyatomic.MassLabel) ? "-" : mass.Polyatomic.MassLabel;

                    masses[i] = mass;
                }

                if (header.Realease >= 4018)
                {
                    // Read metadata v7
                    long posPolyList = 652 + 288 * mask.NumberMasses;
                    long posNbPoly = posPolyList + 16;
                    br.BaseStream.Seek(posNbPoly, SeekOrigin.Begin);
                    header.NumberPolyatomics = br.ReadInt32();

                    long posMaskNano = 676 + 288 * mask.NumberMasses + 144 * header.NumberPolyatomics;
                    long posNbField = posMaskNano + 4 * 24;
                    br.BaseStream.Seek(posNbField, SeekOrigin.Begin);
                    header.NumberMagneticFields = br.ReadInt32();

                    long posBFieldNano = 2228 + 288 * mask.NumberMasses + 144 * header.NumberPolyatomics;
                    long posnNbField = posBFieldNano + 4;
                    br.BaseStream.Seek(posnNbField, SeekOrigin.Begin);
                    header.MagneticField = br.ReadInt32();

                    long posTabTrolley = posBFieldNano + 10 * 4 + 2 * 8;
                    header.Radii = new double[12];
                    StringBuilder sbRadius = new StringBuilder();
                    for (int i = 0; i < header.Radii.Length; i++)
                    {
                        long posRadius = posTabTrolley + i * 208 + 64 + 8;
                        br.BaseStream.Seek(posRadius, SeekOrigin.Begin);
                        header.Radii[i] = br.ReadDouble();
                        sbRadius.Append($"{header.Radii[i].ToString("0.00")} - ");
                    }
                    sbRadius = sbRadius.Remove(sbRadius.Length - 2, 2);
                    header.Radius = sbRadius.ToString();

                    long posAnalParam = 2228 + 288 * mask.NumberMasses + 144 * header.NumberPolyatomics + 2840 * header.NumberMagneticFields;
                    long posComment = posAnalParam + 16 + 4 + 4 + 4 + 4;
                    br.BaseStream.Seek(posComment, SeekOrigin.Begin);

                    var bytes = br.ReadBytes(256);
                    tempString = new string(br.ReadChars(256));
                    header.Comments = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);

                    long posAnalPrimary = posAnalParam + 16 + 4 + 4 + 4 + 4 + 256;
                    long posPrimCurrentT0 = posAnalPrimary + 8;
                    br.BaseStream.Seek(posPrimCurrentT0, SeekOrigin.Begin);
                    header.PrimaryCurrentT0 = br.ReadInt32();
                    header.PrimaryCurrentEnd = br.ReadInt32();

                    long posPrimL1 = posAnalPrimary + 8 + 4 + 4 + 4;
                    br.BaseStream.Seek(posPrimL1, SeekOrigin.Begin);
                    header.PrimaryL1 = br.ReadInt32();

                    long posD1Pos = posAnalPrimary + 8 + 4 + 4 + 4 + 4 + 4 + 4 * 10 + 4 + 4 * 10;
                    br.BaseStream.Seek(posD1Pos, SeekOrigin.Begin);
                    header.PositionD1 = br.ReadInt32();

                    long sizeApPrimary = 552;
                    long posApSecondary = posAnalPrimary + sizeApPrimary;
                    long posPrimL0 = posApSecondary - (67 * 4 + 4 * 10 + 4 + 4 + 4 + 4);
                    br.BaseStream.Seek(posPrimL0, SeekOrigin.Begin);
                    header.PrimaryL0 = br.ReadInt32();
                    header.CsHV = br.ReadInt32();

                    long posESPos = posApSecondary + 8;
                    br.BaseStream.Seek(posESPos, SeekOrigin.Begin);
                    header.PositionES = br.ReadInt32();

                    long posASPos = posESPos + 4 + 40 + 40;
                    br.BaseStream.Seek(posASPos, SeekOrigin.Begin);
                    header.PositionAS = br.ReadInt32();
                }

                long offset = header.HeaderSize - CamecaNanoSIMSHeaderImage.STRUCT_SIZE;
                br.BaseStream.Seek(offset, SeekOrigin.Begin);

                CamecaNanoSIMSHeaderImage headerImage = new CamecaNanoSIMSHeaderImage();
                headerImage.SizeSelf = br.ReadInt32();
                headerImage.Type = br.ReadInt16();
                headerImage.Width = br.ReadInt16();
                headerImage.Height = br.ReadInt16();
                headerImage.PixelDepth = br.ReadInt16();

                headerImage.NumberMasses = br.ReadInt16();
                if (headerImage.NumberMasses < 1) headerImage.NumberMasses = 1;

                headerImage.Depth = br.ReadInt16();
                headerImage.Raster = br.ReadInt32();
                tempString = new string(br.ReadChars(64));
                headerImage.Nickname = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);

                int numMasses = headerImage.NumberMasses;
                int numImages = headerImage.Depth;

                List<Data3D> readIn = new List<Data3D>();
                for (int j = 0; j < numMasses; j++)
                {
                    readIn.Add(new Data3D(headerImage.Width, headerImage.Height, headerImage.Depth));
                }

                bool is16bit = headerImage.PixelDepth == 2;

                long theoreticalSize = headerImage.Width * headerImage.Height * headerImage.Depth * headerImage.NumberMasses * headerImage.PixelDepth + header.HeaderSize;
                long fileSize = br.BaseStream.Length;

                bool isOk = theoreticalSize == fileSize;

                for (int i = 0; i < numImages; i++)
                {
                    for (int j = 0; j < numMasses; j++)
                    {
                        for (int x = 0; x < headerImage.Width; x++)
                        {
                            for (int y = 0; y < headerImage.Height; y++)
                            {
                                if (is16bit)
                                    readIn[j][x, y, i] = br.ReadInt16();
                                else readIn[j][x, y, i] = br.ReadInt32();
                            }
                        }
                    }
                }

                int zzz = 0;
            }
        }
        private static Data2D normalizeScorePlot(Data2D matrix)
        {
            var norm = new Data2D(matrix.Width, matrix.Height);

            var max = getMaxValue(matrix);
            var min = getMinValue(matrix);

            if (max == 0) max = 1;
            if (min == 0) min = -1;

            for (int x = 0; x < matrix.Width; x++)
            {
                for (int y = 0; y < matrix.Height; y++)
                {
                    if (matrix[x, y] > 0) norm[x, y] = matrix[x, y] / max;
                    else if (matrix[x, y] < 0) norm[x, y] = -matrix[x, y] / min;
                }
            }

            return norm;
        }
        private static float getMinValue(Data2D matrix)
        {
            var min = float.MaxValue;

            for (int x = 0; x < matrix.Width; x++)
            {
                for (int y = 0; y < matrix.Height; y++)
                {
                    if (matrix[x, y] < min) min = matrix[x, y];
                }
            }

            return min;
        }
        private static float getMaxValue(Data2D matrix)
        {
            var max = float.MinValue;

            for (int x = 0; x < matrix.Width; x++)
            {
                for (int y = 0; y < matrix.Height; y++)
                {
                    if (matrix[x, y] > max) max = matrix[x, y];
                }
            }

            return max;
        }

        private static int readLine(string line, out int type)
        {
            string[] parts = line.Split(' ');

            if (parts == null || parts.Length == 0)
            {
                type = 0;
                return 0;
            }

            type = int.Parse(parts[0]);

            if (parts.Length == 1 || parts[1] == null) return 0;

            return int.Parse(parts[1]);
        }

        private static void linearRegression()
        {
            Random r = new Random(27826464);
            double[] n = new double[20];
            double[] w = new double[20];
            double[,] x = new double[3, 20];
            double[] y = new double[20];
            for (int i = 0; i < 20; i++)
            {
                n[i] = i;
                w[i] = 1;

                double xx = i / 10.0;
                double term = xx;

                x[0, i] = 1;
                for (int j = 1; j < 3; j++)
                {
                    x[j, i] = term;
                    term *= xx;
                }
                y[i] = 4.0 + 3.0 * i / 10.0 + r.NextDouble() / 10.0;
            }

            LinearRegression linReg = new LinearRegression();
            linReg.Regress(y, x, w);
            int a = 0;
        }
        private static void checkColors()
        {
            double delta = 1d;
            for (int r = 0; r < 255; r++)
            {
                for (int g = 0; g < 255; g++)
                {
                    for (int b = 0; b < 255; b++)
                    {
                        double[] rgb = new double[] { r, g, b };
                        double[] ihs = ColorConversion.RGBtoIHS(rgb[0] / 255, rgb[1] / 255, rgb[2] / 255);
                        double[] returned = ColorConversion.IHStoRGB(ihs[1], ihs[2], ihs[0]);

                        if (Math.Abs(rgb[0] - (returned[0] * 255)) < delta && 
                            Math.Abs(rgb[1] - (returned[1] * 255)) < delta && 
                            Math.Abs(rgb[2] - (returned[2] * 255)) < delta)
                        {
                            continue;
                        }
                        else
                        {
                            Console.WriteLine($"Convert failed {r} {g} {b}");
                        }
                    }
                }
            }
        }
    }

    internal abstract class CamecaNanoSIMSHeaderPart
    {
        public static DateTime ParseDateAndTimeStrings(string date, string time)
        {
            string formatted = $"{date} - {time}";
            return DateTime.ParseExact(formatted, "dd.MM.yy - H:m", CultureInfo.InvariantCulture);
        }
        public static string RemovePadCharacters(string padded)
        {
            return padded.Replace("\0", "");
        }
    }
    internal class CamecaNanoSIMSHeader
    {
        public int Realease { get; set; }
        public int AnalysisType { get; set; }
        public int HeaderSize { get; set; }
        public int SampleType { get; set; }
        public int DataInlcuded { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public string AnalysisName { get; set; }
        public string UserName { get; set; }
        public int PositionZ { get; set; }
        public DateTime AnalysisTime { get; set; }

        // AnalysisType:    MIMS_IMAGE = 27
        //                  MIMS_LINE_SCAN_IMAGE = 39
        //                  MIMS_SAMPLE_STAGE_IMAGE = 41

        // v7 Meta Data:
        public int NumberPolyatomics { get; set; }
        public int NumberMagneticFields { get; set; }
        public int MagneticField { get; set; }
        public double[] Radii { get; set; }
        public string Radius { get; set; }
        public string Comments { get; set; }
        public int PrimaryCurrentT0 { get; set; }
        public int PrimaryCurrentEnd { get; set; }
        public int PrimaryL1 { get; set; }
        public int PositionD1 { get; set; }
        public int PrimaryL0 { get; set; }
        public int CsHV { get; set; }
        public int PositionES { get; set; }
        public int PositionAS { get; set; }
    }

    internal class CamecaNanoSIMSMaskImage
    {
        public string FileName { get; set; }
        public int AnalysisDuration { get; set; }
        public int CycleNumber { get; set; }
        public int ScanType { get; set; }
        public short Magnification { get; set; }
        public short SizeType { get; set; }
        public short SizeDetector { get; set; }
        public int BeamBlanking { get; set; }
        public int Sputtering { get; set; }
        public int SputteringDuration { get; set; }
        public int AutoCalibrationInAnalysis { get; set; }
        public CamecaNanoSIMSAutoCal AutoCal { get; set; }
        public int SigReference { get; set; }
        public CamecaNanoSIMSSigRef SigRef { get; set; }
        public int NumberMasses { get; set; }
    }

    internal class CamecaNanoSIMSAutoCal
    {
        public string Mass { get; set; }
        public int Begin { get; set; }
        public int Period { get; set; }
    }

    internal class CamecaNanoSIMSSigRef
    {
        public CamecaNanoSIMSPolyatomic Polyatomic { get; set; }
        public int Detector { get; set; }
        public int Offset { get; set; }
        public int Quantity { get; set; }
    }

    internal class CamecaNanoSIMSPolyatomic
    {
        public int FlagNumeric { get; set; }
        public int NumericValue { get; set; }
        public int NumberElements { get; set; }
        public int NumberCharges { get; set; }
        public string Charge { get; set; }
        public string MassLabel { get; set; }
        public CamecaNanoSIMSTablets[] Tablets { get; set; }
    }

    internal class CamecaNanoSIMSTablets
    {
        public int NumberElements { get; set; }
        public int NumberIsotopes { get; set; }
        public int Quantity { get; set; }
    }

    internal class CamecaNanoSIMSTabMass
    {
        public double Amu { get; set; }
        public int MatrixOrTrace { get; set; }
        public int Detector { get; set; }
        public double WaitingTime { get; set; }
        public double CountingTime { get; set; }
        public int Offset { get; set; }
        public int MagField { get; set; }
        public CamecaNanoSIMSPolyatomic Polyatomic { get; set; }
    }

    internal class CamecaNanoSIMSHeaderImage
    {
        public const int STRUCT_SIZE = 84;

        public int SizeSelf { get; set; }
        public short Type { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
        public short PixelDepth { get; set; }
        public short NumberMasses { get; set; }
        public short Depth { get; set; }
        public int Raster { get; set; }
        public string Nickname { get; set; }
    }

    public class LinearRegression
    {
        double[,] _v;
        double[] _c;
        double[] _sec;
        double _rysq;
        double _sdv;
        double _fReg;
        double[] _yCalc;
        double[] _dY;

        public double FisherF { get { return _fReg; } }
        public double CorrelationCoefficient { get { return _rysq; } }
        public double StandardDeviation { get { return _sdv; } }
        public double[] CalculatedValues { get { return _yCalc; } }
        public double[] Residuals { get { return _dY; } }
        public double[] Coefficients { get { return _c; } }
        public double[] CoefficientsStandardError { get { return _sec; } }
        public double[,] VarianceMatrix { get { return _v; } }

        public bool Regress(double[] y, double[,] x, double[] w)
        {
            // y[j]     j-th observed data point
            // x[i,j]   j-th value of the ith independent variable
            // w[h]     j-th weight value

            int m = y.Length;
            int n = x.Length / m;
            int ndf = m - n;

            _yCalc = new double[m];
            _dY = new double[m];

            if (ndf < 1) return false;

            _v = new double[n, n];
            _c = new double[n];
            _sec = new double[n];

            double[] b = new double[n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    _v[i, j] = 0;
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    _v[i, j] = 0;
                    for (int k = 0; k < m; k++)
                    {
                        _v[i, j] = _v[i, j] + w[k] * x[i, k] * x[j, k];
                    }
                    b[i] = 0;
                    for (int k = 0; k < m; k++)
                    {
                        b[i] = b[i] + w[k] * x[i, k] * y[k];
                    }
                }
            }

            if (!SymmetricMatrixInvert(_v)) return false;

            for (int i = 0; i < n; i++)
            {
                _c[i] = 0;
                for (int j = 0; j < n; j++)
                {
                    _c[i] = _c[i] + _v[i, j] * b[j];
                }
            }

            double tss = 0;
            double rss = 0;
            double ybar = 0;
            double wsum = 0;

            for (int k = 0; k < m; k++)
            {
                ybar = ybar + w[k] * y[k];
                wsum = wsum + w[k];
            }
            ybar = ybar / wsum;

            for (int k = 0; k < m; k++)
            {
                _yCalc[k] = 0;
                for (int i = 0; i < n; i++)
                {
                    _yCalc[k] = _yCalc[k] + _c[i] * x[i, k];
                }
                _dY[k] = _yCalc[k] - y[k];
                tss = tss + w[k] * (y[k] - ybar) * (y[k] - ybar);
                rss = rss + w[k] * _dY[k] * _dY[k];
            }

            double ssq = rss / ndf;
            _rysq = 1 - rss / tss;
            _fReg = 9999999;
            if (_rysq < 0.9999999)
            {
                _fReg = _rysq / (1 - _rysq) * ndf / (n - 1);
            }
            _sdv = Math.Sqrt(ssq);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    _v[i, j] = _v[i, j] * ssq;
                    _sec[i] = Math.Sqrt(_v[i, i]);
                }
            }

            return true;
        }

        public bool SymmetricMatrixInvert(double[,] v)
        {
            int n = (int)Math.Sqrt(v.Length);
            double[] t = new double[n];
            double[] q = new double[n];
            double[] r = new double[n];
            double ab = 0;
            int k = 0;
            int l = 0;
            int m = 0;

            for (m = 0; m < n; m++)
            {
                r[m] = 1;
            }
            for (m = 0; m < n; m++)
            {
                double big = 0;
                for (l = 0; l < n; l++)
                {
                    ab = Math.Abs(v[l, l]);
                    if ((ab > big) && (r[l] != 0))
                    {
                        big = ab;
                        k = l;
                    }
                }
                if (big == 0) return false;

                r[k] = 0;
                q[k] = 1 / v[k, k];
                t[k] = 1;
                v[k, k] = 0;
                if (k != 0)
                {
                    for (l = 0; l < k; l++)
                    {
                        t[l] = v[l, k];
                        if (r[l] == 0)
                            q[l] = v[l, k] * q[k];
                        else
                            q[l] = -v[l, k] * q[k];
                        v[l, k] = 0;
                    }
                }
                if ((k + 1) < n)
                {
                    for (l = k + 1; l < n; l++)
                    {
                        if (r[l] != 0)
                            t[l] = v[k, l];
                        else
                            t[l] = -v[k, l];
                        q[l] = -v[k, l] * q[k];
                        v[k, l] = 0;
                    }
                }
                for (l = 0; l < n; l++)
                {
                    for (k = l; k < n; k++)
                    {
                        v[l, k] = v[l, k] + t[l] * q[k];
                    }
                }
            }
            m = n;
            l = n - 1;
            for (k = 1; k < n; k++)
            {
                m = m - 1;
                l = l - 1;
                for (int j = 0; j <= l; j++)
                {
                    v[m, j] = v[j, m];
                }

            }
            return true;
        }
    }

    //public class Cameca1280Spectrum
    //{
    //    internal class Species
    //    {
    //        internal int Cycles;
    //        internal int SizeInBytes;
    //        internal int PixelEncoding;
    //        internal double Mass;
    //        internal string Label;
    //        internal double WaitTime;
    //        internal double CountTime;
    //        internal double WellTime;
    //        internal double ExtraTime;
    //    }

    //    private static int getXmlIndex(byte[] buffer)
    //    {
    //        int i = 2;
    //        int length = buffer.Length;
    //        while (i < length)
    //        {
    //            switch (buffer[i])
    //            {
    //                case 120:
    //                    if (buffer[i - 1] == 63 && buffer[i - 2] == 60)
    //                    {
    //                        return i - 2;
    //                    }
    //                    i += 3;
    //                    continue;
    //                case 63:
    //                    i += 1;
    //                    continue;
    //                case 60:
    //                    i += 2;
    //                    continue;
    //                default:
    //                    i += 3;
    //                    continue;
    //            }
    //        }
    //        return -1;
    //    }
    //    public void LoadFromFile(string fileName)
    //    {
    //        int dataBufferSize = 0;
    //        int xmlBufferSize = 0;

    //        byte[] fullBuffer;
    //        byte[] dataBuffer;
    //        byte[] xmlBuffer;

    //        byte[] xmlStartSequence = new byte[] { 60, 63, 120 };

    //        using (Stream stream = File.OpenRead(fileName))
    //        {
    //            BinaryReader br = new BinaryReader(stream);

    //            fullBuffer = br.ReadBytes((int)stream.Length);

    //            // Get size of binary section at top of file
    //            //int startIndexOfXml = 0;
    //            //for (int i = 0; i < stream.Length - 2; i++)
    //            //{
    //            //    if (fullBuffer[i] == 60 && fullBuffer[i + 1] == 63 && fullBuffer[i + 2] == 120)
    //            //        startIndexOfXml = i;
    //            //}
    //            int startIndexOfXml = getXmlIndex(fullBuffer);
                


    //            // Split buffer into two parts: binary data and
    //            // XML experiment details sections


    //            // First 24 bytes are garbage, so skip those
    //            dataBufferSize = startIndexOfXml - 24;
    //            // Last byte of the file is a null byte so don't include it
    //            // with the XML details buffer
    //            xmlBufferSize = (int)stream.Length - startIndexOfXml - 1;

    //            dataBuffer = new byte[dataBufferSize];
    //            xmlBuffer = new byte[xmlBufferSize];

    //            Array.Copy(fullBuffer, 24, dataBuffer, 0, dataBufferSize);
    //            Array.Copy(fullBuffer, startIndexOfXml, xmlBuffer, 0, xmlBufferSize);
    //        }            

    //        fullBuffer = null;

    //        XmlDocument docDetails = new XmlDocument();
    //        string xml = Encoding.UTF8.GetString(xmlBuffer);
    //        docDetails.LoadXml(xml);

    //        int imageSize = 0;
    //        List<Species> loadedSpecies = new List<Species>();

    //        XmlNode nodeSize = docDetails.SelectSingleNode("/IMP/LOADEDFILE/PROPERTIES/DEFACQPARAMIMDATA/m_nSize");
    //        imageSize = int.Parse(nodeSize.InnerText);

    //        XmlNodeList nodeSpeciesList = docDetails.SelectNodes("IMP/LOADEDFILE/SPECIES");
    //        foreach(XmlNode nodeSpecies in nodeSpeciesList)
    //        {
    //            Species species = new Species();

    //            XmlNode nodeNumberCycles = nodeSpecies.SelectSingleNode("n_AcquiredCycleNb");
    //            species.Cycles = int.Parse(nodeNumberCycles.InnerText);

    //            XmlNode nodeSizeBytes = nodeSpecies.SelectSingleNode("SIZE");
    //            species.SizeInBytes = int.Parse(nodeSizeBytes.InnerText);

    //            XmlNode nodePixelEncoding = nodeSpecies.SelectSingleNode("PROPERTIES/COMMON_TO_ALL_SPECIESPCTRS/n_EncodedPixelType");
    //            species.PixelEncoding = int.Parse(nodePixelEncoding.InnerText);

    //            XmlNode nodeMass = nodeSpecies.SelectSingleNode("PROPERTIES/DPSPECIESDATA/d_Mass");
    //            species.Mass = double.Parse(nodeMass.InnerText);

    //            XmlNode nodeLabel = nodeSpecies.SelectSingleNode("PROPERTIES/DPSPECIESDATA/psz_MatrixSpecies");
    //            species.Label = nodeLabel.InnerText;

    //            XmlNode nodeWaitTime = nodeSpecies.SelectSingleNode("PROPERTIES/DPSPECIESDATA/d_WaitTime");
    //            species.WaitTime = double.Parse(nodeWaitTime.InnerText);

    //            XmlNode nodeCountTime = nodeSpecies.SelectSingleNode("PROPERTIES/DPSPECIESDATA/d_CountTime");
    //            species.CountTime = double.Parse(nodeCountTime.InnerText);

    //            XmlNode nodeWellTime = nodeSpecies.SelectSingleNode("PROPERTIES/DPSPECIESDATA/d_WellTime");
    //            species.WellTime = double.Parse(nodeWellTime.InnerText);

    //            XmlNode nodeExtraTime = nodeSpecies.SelectSingleNode("PROPERTIES/DPSPECIESDATA/d_ExtraTime");
    //            species.ExtraTime = double.Parse(nodeExtraTime.InnerText);

    //            loadedSpecies.Add(species);
    //        }

    //        int pixelType = loadedSpecies[0].PixelEncoding;
    //        int integerSize = 0;

    //        // Only know that 0 corresponds to Int16
    //        if (pixelType == 0) integerSize = 2;

    //        float[] values = new float[imageSize * imageSize * loadedSpecies.Count * loadedSpecies[0].Cycles];
    //        for (int i = 0; i < values.Length; i++)
    //        {
    //            //Int16
    //            if (pixelType == 0)
    //            {
    //                values[i] = BitConverter.ToInt16(dataBuffer, i * integerSize);
    //            }
    //        }


    //        float[,,] matrix = new float[imageSize, imageSize, loadedSpecies.Count];

    //        int pos = 0;

    //        for (int z = 0; z < loadedSpecies.Count; z++)
    //        {
    //            for (int a = 0; a < loadedSpecies[0].Cycles; a++)
    //            {
    //                for (int y = 0; y < imageSize; y++)
    //                {
    //                    for (int x = 0; x < imageSize; x++)
    //                    {
    //                        matrix[x, y, z] += values[pos++];
    //                    }
    //                }
    //            }
    //        }
            

    //        using(StreamWriter sw = new StreamWriter(@"G:\si - ta - 0310rae@1_1.csv"))
    //        {
    //            for (int y = 0; y < imageSize; y++)
    //            {
    //                StringBuilder sb = new StringBuilder();
    //                for (int x = 0; x < imageSize; x++)
    //                {
    //                    sb.Append($"{matrix[x, y, 0]},");
    //                }
    //                sb.Remove(sb.Length - 1, 1);
    //                sw.WriteLine(sb.ToString());
    //            }
    //        }
    //        using (StreamWriter sw = new StreamWriter(@"G:\si - ta - 0310rae@1_2.csv"))
    //        {
    //            for (int y = 0; y < imageSize; y++)
    //            {
    //                StringBuilder sb = new StringBuilder();
    //                for (int x = 0; x < imageSize; x++)
    //                {
    //                    sb.Append($"{matrix[x, y, 1]},");
    //                }
    //                sb.Remove(sb.Length - 1, 1);
    //                sw.WriteLine(sb.ToString());
    //            }
    //        }

    //        int j = 0;
    //        //foreach(XmlNode nodeDoc in docDetails.ChildNodes)
    //        //{
    //        //    // Get IMP node
    //        //    if(nodeDoc.Name == "IMP")
    //        //    {
    //        //        // Get LOADEDFILE node
    //        //        foreach(XmlNode nodeImp in nodeDoc.ChildNodes)
    //        //        {
    //        //            if (nodeImp.Name == "LOADEDFILE")
    //        //            {

    //        //                // Get PROPERTIES node
    //        //                foreach (XmlNode nodeLoadedFile in nodeImp.ChildNodes)
    //        //                {
    //        //                    if(nodeLoadedFile.Name == "PROPERTIES")
    //        //                    {

    //        //                        // Get DEFACQPARAMIMDATA node
    //        //                        foreach(XmlNode nodeProperties in nodeLoadedFile.ChildNodes)
    //        //                        {
    //        //                            if(nodeProperties.Name == "DEFACQPARAMIMDATA")
    //        //                            {

    //        //                                // Get m_nSize node
    //        //                                foreach(XmlNode nodeImageData in nodeProperties.ChildNodes)
    //        //                                {
    //        //                                    if(nodeImageData.Name == "m_nSize")
    //        //                                    {
    //        //                                        imageSize = int.Parse(nodeImageData.FirstChild.Value);
    //        //                                    }
    //        //                                }
    //        //                            }
    //        //                        }

    //        //                    }
    //        //                }

    //        //                // Get SPECIES nodes
    //        //                foreach(XmlNode nodeLoadedFile in nodeImp.ChildNodes)
    //        //                {
    //        //                    if(nodeLoadedFile.Name == "SPECIES")
    //        //                    {
    //        //                        Species s = new Species();

    //        //                        foreach (XmlNode nodeSpecies in nodeLoadedFile.ChildNodes)
    //        //                        {
    //        //                            if(nodeSpecies.Name == "n_AcquiredCycleNb")
    //        //                            {
    //        //                                s.Cycles = int.Parse(nodeSpecies.FirstChild.Value);
    //        //                            }

    //        //                            if(nodeSpecies.Name == "SIZE")
    //        //                            {
    //        //                                s.SizeInBytes = int.Parse(nodeSpecies.FirstChild.Value);
    //        //                            }

    //        //                            if(nodeSpecies.Name == "PROPERTIES")
    //        //                            {

    //        //                                // Get PROPERTIES node
    //        //                                foreach(XmlNode nodeProperties in nodeSpecies.ChildNodes)
    //        //                                {
    //        //                                    if(nodeProperties.Name == "d_Mass")
    //        //                                    {
    //        //                                        s.Mass = double.Parse(nodeProperties.FirstChild.Value);
    //        //                                    }
    //        //                                    if(nodeProperties.Name == "d_WaitTime")
    //        //                                    {
    //        //                                        s.WaitTime = double.Parse(nodeProperties.FirstChild.Value);
    //        //                                    }
    //        //                                    if(nodeProperties.Name == "d_CountTime")
    //        //                                    {
    //        //                                        s.CountTime = double.Parse(nodeProperties.FirstChild.Value);
    //        //                                    }
    //        //                                    if(nodeProperties.Name == "d_WellTime")
    //        //                                    {
    //        //                                        s.WellTime = double.Parse(nodeProperties.FirstChild.Value);
    //        //                                    }
    //        //                                    if(nodeProperties.Name == "d_ExtraTime")
    //        //                                    {
    //        //                                        s.ExtraTime = double.Parse(nodeProperties.FirstChild.Value);
    //        //                                    }
    //        //                                    if(nodeProperties.Name == "psz_MatrixSpecies")
    //        //                                    {
    //        //                                        s.Label = nodeProperties.FirstChild.Value;
    //        //                                    }
    //        //                                }
    //        //                            }
    //        //                        }

    //        //                        species.Add(s);
    //        //                    }

    //        //                }

    //        //            }

    //        //        }
    //        //    }
    //        //}

    //    }
    //}

    //internal static class TestSpec
    //{
    //    public static int[,] Generate()
    //    {
    //        int[,] spec = new int[10000, 2];

    //        Random r = new Random();
    //        int startTime = 5256;
    //        for (int x = 0; x < 10000; x++)
    //        {
    //            spec[x, 0] = startTime + x;
    //            spec[x, 1] = r.Next(0, 1000);
    //        }

    //        return spec;
    //    }
    //}
}
