using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using ImagingSIMS.Common;
using ImagingSIMS.Common.Math;

namespace ImagingSIMS.Data.Imaging
{
    //public static class Filter
    //{
    //    public static BitmapSource[] MedianSmooth(BitmapSource[] InputImages, BackgroundWorker bw_sender)
    //    {
    //        Data3D[] inputData = new Data3D[InputImages.Length];
    //        for (int i = 0; i < inputData.Length; i++)
    //        {
    //            InputImages[i].Freeze();
    //            BitmapSource bs = InputImages[i];
    //            if (bs == null) throw new ArgumentException("Invalid ImageSource.");

    //            inputData[i] = ImageHelper.ConvertToData3D(bs);
    //        }

    //        int szX = inputData[0].Width;
    //        int szY = inputData[0].Height;
    //        int szZ = inputData.Length;

    //        int totalSteps = szX * szZ;
    //        int count = 0;

    //        Data3D[] smoothed = new Data3D[szZ];

    //        for (int z = 0; z < szZ; z++)
    //        {

    //            Data2D[] layers = new Data2D[4];
    //            for (int i = 0; i < 4; i++)
    //            {
    //                layers[i] = new Data2D(szX, szY);
    //            }

    //            for (int x = 0; x < szX; x++)
    //            {
    //                for (int y = 0; y < szY; y++)
    //                {
    //                    int startX = x - 2;
    //                    int startY = y - 2;
    //                    int startZ = z - 1;

    //                    List<float> valA = new List<float>();
    //                    List<float> valR = new List<float>();
    //                    List<float> valG = new List<float>();
    //                    List<float> valB = new List<float>();

    //                    for (int a = 0; a < 5; a++)
    //                    {
    //                        for (int b = 0; b < 5; b++)
    //                        {
    //                            for (int c = 0; c < 3; c++)
    //                            {
    //                                int locX = startX + a;
    //                                int locY = startY + b;
    //                                int locZ = startZ + c;

    //                                if (locX < 0 || locX >= szX | locY < 0 || locY >= szY ||
    //                                    locZ < 0 || locZ >= szZ)
    //                                {
    //                                }
    //                                else
    //                                {
    //                                    valB.Add(inputData[locZ][locX, locY, 0]);
    //                                    valG.Add(inputData[locZ][locX, locY, 1]);
    //                                    valR.Add(inputData[locZ][locX, locY, 2]);
    //                                    valA.Add(inputData[locZ][locX, locY, 3]);
    //                                }

    //                            }

    //                        }
    //                    }
    //                    Color color = MedianColor(valA, valR, valG, valB);
    //                    layers[0][x, y] = color.B;
    //                    layers[1][x, y] = color.G;
    //                    layers[2][x, y] = color.R;
    //                    layers[3][x, y] = color.A;
    //                }
    //                count++;
    //                bw_sender.ReportProgress(Percentage.GetPercent(count, totalSteps));
    //            }
    //            smoothed[z] = new Data3D(layers);
    //        }

    //        BitmapSource[] corrected = new BitmapSource[InputImages.Length];
    //        for (int z = 0; z < smoothed.Length; z++)
    //        {
    //            corrected[z] = ImageHelper.CreateImage(smoothed[z]);
    //        }

    //        return corrected;
    //    }
    //    public static BitmapSource[] MeanSmooth(BitmapSource[] InputImages, BackgroundWorker bw_sender)
    //    {
    //        Data3D[] inputData = new Data3D[InputImages.Length];
    //        for (int i = 0; i < inputData.Length; i++)
    //        {
    //            InputImages[i].Freeze();
    //            BitmapSource bs = InputImages[i];
    //            if (bs == null) throw new ArgumentException("Invalid ImageSource.");

    //            inputData[i] = ImageHelper.ConvertToData3D(bs);
    //        }
    //        int szX = inputData[0].Width;
    //        int szY = inputData[0].Height;
    //        int szZ = inputData.Length;

    //        int totalSteps = szX * szZ;
    //        int count = 0;

    //        Data3D[] smoothed = new Data3D[szZ];

    //        for (int z = 0; z < szZ; z++)
    //        {

    //            Data2D[] layers = new Data2D[4];
    //            for (int i = 0; i < 4; i++)
    //            {
    //                layers[i] = new Data2D(szX, szY);
    //            }

    //            for (int x = 0; x < szX; x++)
    //            {
    //                for (int y = 0; y < szY; y++)
    //                {
    //                    int startX = x - 2;
    //                    int startY = y - 2;
    //                    int startZ = z - 1;

    //                    List<float> valA = new List<float>();
    //                    List<float> valR = new List<float>();
    //                    List<float> valG = new List<float>();
    //                    List<float> valB = new List<float>();

    //                    for (int a = 0; a < 5; a++)
    //                    {
    //                        for (int b = 0; b < 5; b++)
    //                        {
    //                            for (int c = 0; c < 3; c++)
    //                            {
    //                                int locX = startX + a;
    //                                int locY = startY + b;
    //                                int locZ = startZ + c;

    //                                if (locX < 0 || locX >= szX | locY < 0 || locY >= szY ||
    //                                    locZ < 0 || locZ >= szZ)
    //                                {
    //                                }
    //                                else
    //                                {
    //                                    valB.Add(inputData[locZ][locX, locY, 0]);
    //                                    valG.Add(inputData[locZ][locX, locY, 1]);
    //                                    valR.Add(inputData[locZ][locX, locY, 2]);
    //                                    valA.Add(inputData[locZ][locX, locY, 3]);
    //                                }
    //                            }
    //                        }
    //                    }
    //                    Color color = MeanColor(valA, valR, valG, valB);
    //                    layers[0][x, y] = color.B;
    //                    layers[1][x, y] = color.G;
    //                    layers[2][x, y] = color.R;
    //                    layers[3][x, y] = color.A;
    //                }
    //                count++;
    //                bw_sender.ReportProgress(Percentage.GetPercent(count, totalSteps));
    //            }
    //            smoothed[z] = new Data3D(layers);
    //        }

    //        BitmapSource[] corrected = new BitmapSource[InputImages.Length];
    //        for (int z = 0; z < smoothed.Length; z++)
    //        {
    //            corrected[z] = ImageHelper.CreateImage(smoothed[z]);
    //        }

    //        return corrected;
    //    }
    //    public static double[,] MeanSmooth(Double[,] Input)
    //    {
    //        int sizeX = Input.GetLength(0);
    //        int sizeY = Input.GetLength(1);

    //        double[,] returnData = new double[sizeX, sizeY];

    //        for (int x = 0; x < sizeX; x++)
    //        {
    //            for (int y = 0; y < sizeY; y++)
    //            {
    //                int startX = x - 3;
    //                int startY = y - 3;

    //                List<double> values = new List<double>();

    //                for (int a = 0; a < 7; a++)
    //                {
    //                    for (int b = 0; b < 7; b++)
    //                    {
    //                        int locX = startX + a;
    //                        int locY = startY + b;

    //                        if (locX < 0 || locX >= sizeX || 
    //                            locY < 0 || locY >= sizeY) 
    //                            continue;

    //                        values.Add(Input[locX, locY]);
    //                    }
    //                }

    //                returnData[x, y] = AverageD(values);
    //            }
    //        }

    //        return returnData;
    //    }
    //    public static double[,] MeanSmooth(Data2D Input)
    //    {
    //        int sizeX = Input.Width;
    //        int sizeY = Input.Height;

    //        double[,] returnData = new double[sizeX, sizeY];

    //        for (int x = 0; x < sizeX; x++)
    //        {
    //            for (int y = 0; y < sizeY; y++)
    //            {
    //                int startX = x - 3;
    //                int startY = y - 3;

    //                List<double> values = new List<double>();

    //                for (int a = 0; a < 7; a++)
    //                {
    //                    for (int b = 0; b < 7; b++)
    //                    {
    //                        int locX = startX + a;
    //                        int locY = startY + b;

    //                        if (locX < 0 || locX >= sizeX ||
    //                            locY < 0 || locY >= sizeY)
    //                            continue;

    //                        values.Add((double)Input[locX, locY]);
    //                    }
    //                }

    //                returnData[x, y] = AverageD(values);
    //            }
    //        }

    //        return returnData;
    //    }
    //    public static BitmapSource[] HighPassFilter(BitmapSource[] InputImages, BackgroundWorker bw_sender)
    //    {
    //        Data3D[] inputData = new Data3D[InputImages.Length];
    //        for (int i = 0; i < inputData.Length; i++)
    //        {
    //            InputImages[i].Freeze();
    //            BitmapSource bs = InputImages[i];
    //            if (bs == null) throw new ArgumentException("Invalid ImageSource.");

    //            inputData[i] = ImageHelper.ConvertToData3D(bs);
    //        }

    //        int szX = inputData[0].Width;
    //        int szY = inputData[0].Height;

    //        int totalSteps = szX * inputData.Length;
    //        int count = 0;

    //        Data3D[] smoothed = new Data3D[inputData.Length];

    //        for (int z = 0; z < inputData.Length; z++)
    //        {
    //            Data2D[] img = new Data2D[4];
    //            for (int i = 0; i < 4; i++)
    //            {
    //                img[i] = new Data2D(szX, szY);
    //            }

    //            for (int x = 0; x < inputData[z].Width; x++)
    //            {
    //                for (int y = 0; y < inputData[z].Height; y++)
    //                {
    //                    int startX = x - 1;
    //                    int startY = y - 1;

    //                    float valA = 0;
    //                    float valR = 0;
    //                    float valG = 0;
    //                    float valB = 0;

    //                    for (int a = 0; a < 3; a++)
    //                    {
    //                        for (int b = 0; b < 3; b++)
    //                        {
    //                            int locX = startX + a;
    //                            int locY = startY + b;

    //                            //If pixel is outside bounds of image, use boundary pixels
    //                            //http://en.wikipedia.org/wiki/File:Extend_Edge-Handling.png
    //                            if (locX < 0) locX = 0;
    //                            else if (locX >= szX) locX = szX - 1;
    //                            if (locY < 0) locY = 0;
    //                            else if (locY >= szY) locY = szY - 1;

    //                            float B = inputData[z][locX, locY, 0];
    //                            float G = inputData[z][locX, locY, 1];
    //                            float R = inputData[z][locX, locY, 2];
    //                            float A = inputData[z][locX, locY, 3];

    //                            float filterValue = highPassKernel[a, b];
    //                            valA += A * filterValue;
    //                            valR += R * filterValue;
    //                            valG += G * filterValue;
    //                            valB += B * filterValue;
    //                        }
    //                    }

    //                    img[0][x, y] = valB;
    //                    img[1][x, y] = valG;
    //                    img[2][x, y] = valR;
    //                    img[3][x, y] = valA;
    //                }
    //                count++;
    //                if(bw_sender!=null)
    //                    bw_sender.ReportProgress(Percentage.GetPercent(count, totalSteps));
    //            }

    //            smoothed[z] = new Data3D(img);
    //        }

    //        BitmapSource[] corrected = new BitmapSource[InputImages.Length];
    //        for (int z = 0; z < smoothed.Length; z++)
    //        {
    //            corrected[z] = ImageHelper.CreateImage(smoothed[z]);
    //        }

    //        return corrected;
    //    }
    //    public static BitmapSource[] LowPassFilter(BitmapSource[] InputImages, BackgroundWorker bw_sender)
    //    {
    //        Data3D[] inputData = new Data3D[InputImages.Length];
    //        for (int i = 0; i < inputData.Length; i++)
    //        {
    //            InputImages[i].Freeze();
    //            BitmapSource bs = InputImages[i];
    //            if (bs == null) throw new ArgumentException("Invalid ImageSource.");

    //            inputData[i] = ImageHelper.ConvertToData3D(bs);
    //        }

    //        int szX = inputData[0].Width;
    //        int szY = inputData[0].Height;

    //        int totalSteps = szX * inputData.Length;
    //        int count = 0;

    //        Data3D[] smoothed = new Data3D[inputData.Length];

    //        for (int z = 0; z < inputData.Length; z++)
    //        {
    //            Data2D[] img = new Data2D[4];
    //            for (int i = 0; i < 4; i++)
    //            {
    //                img[i] = new Data2D(szX, szY);
    //            }

    //            for (int x = 0; x < inputData[z].Width; x++)
    //            {
    //                for (int y = 0; y < inputData[z].Height; y++)
    //                {
    //                    int startX = x - 1;
    //                    int startY = y - 1;

    //                    float valA = 0;
    //                    float valR = 0;
    //                    float valG = 0;
    //                    float valB = 0;

    //                    for (int a = 0; a < 3; a++)
    //                    {
    //                        for (int b = 0; b < 3; b++)
    //                        {
    //                            int locX = startX + a;
    //                            int locY = startY + b;

    //                            //If pixel is outside bounds of image, use boundary pixels
    //                            //http://en.wikipedia.org/wiki/File:Extend_Edge-Handling.png
    //                            if (locX < 0) locX = 0;
    //                            else if (locX >= szX) locX = szX - 1;
    //                            if (locY < 0) locY = 0;
    //                            else if (locY >= szY) locY = szY - 1;

    //                            float B = inputData[z][locX, locY, 0];
    //                            float G = inputData[z][locX, locY, 1];
    //                            float R = inputData[z][locX, locY, 2];
    //                            float A = inputData[z][locX, locY, 3];

    //                            float filterValue = lowPassKernel[a, b] / 16f;

    //                            valA += A * filterValue;
    //                            valR += R * filterValue;
    //                            valG += G * filterValue;
    //                            valB += B * filterValue;                                
    //                        }
    //                    }
    //                    img[0][x, y] = valB;
    //                    img[1][x, y] = valG;
    //                    img[2][x, y] = valR;
    //                    img[3][x, y] = valA;
    //                }
    //                count++;
    //                if (bw_sender != null)
    //                    bw_sender.ReportProgress(Percentage.GetPercent(count, totalSteps));
    //            }

    //            smoothed[z] = new Data3D(img);
    //        }

    //        BitmapSource[] corrected = new BitmapSource[InputImages.Length];
    //        for (int z = 0; z < smoothed.Length; z++)
    //        {
    //            corrected[z] = ImageHelper.CreateImage(smoothed[z]);
    //        }

    //        return corrected;
    //    }
    //    public static BitmapSource[] GaussianFilter(BitmapSource[] InputImages, BackgroundWorker bw_sender, float Sigma = 1.0f)
    //    {
    //        Data3D[] inputData = new Data3D[InputImages.Length];
    //        for (int i = 0; i < inputData.Length; i++)
    //        {
    //            InputImages[i].Freeze();
    //            BitmapSource bs = InputImages[i];
    //            if (bs == null) throw new ArgumentException("Invalid ImageSource.");

    //            inputData[i] = ImageHelper.ConvertToData3D(bs);
    //        }

    //        int szX = inputData[0].Width;
    //        int szY = inputData[0].Height;

    //        int totalSteps = szX * inputData.Length;
    //        int count = 0;

    //        Data3D[] smoothed = new Data3D[inputData.Length];

    //        Kernel kernel = customGaussianKernel(Sigma);

    //        for (int z = 0; z < inputData.Length; z++)
    //        {
    //            Data2D[] img = new Data2D[4];
    //            for (int i = 0; i < 4; i++)
    //            {
    //                img[i] = new Data2D(szX, szY);
    //            }

    //            for (int x = 0; x < inputData[z].Width; x++)
    //            {
    //                for (int y = 0; y < inputData[z].Height; y++)
    //                {
    //                    int startX = x - 3;
    //                    int startY = y - 3;

    //                    float valA = 0;
    //                    float valR = 0;
    //                    float valG = 0;
    //                    float valB = 0;

    //                    for (int a = 0; a < 5; a++)
    //                    {
    //                        for (int b = 0; b < 5; b++)
    //                        {
    //                            int locX = startX + a;
    //                            int locY = startY + b;

    //                            //If pixel is outside bounds of image, use boundary pixels
    //                            //http://en.wikipedia.org/wiki/File:Extend_Edge-Handling.png
    //                            if (locX < 0) locX = 0;
    //                            else if (locX >= szX) locX = szX - 1;
    //                            if (locY < 0) locY = 0;
    //                            else if (locY >= szY) locY = szY - 1;

    //                            float B = inputData[z][locX, locY, 0];
    //                            float G = inputData[z][locX, locY, 1];
    //                            float R = inputData[z][locX, locY, 2];
    //                            float A = inputData[z][locX, locY, 3];

    //                            float filterValue = kernel[a, b];
    //                            if (!kernel.IsNormalized) filterValue /= kernel.NormalizationFactor;

    //                            valA += A * filterValue;
    //                            valR += R * filterValue;
    //                            valG += G * filterValue;
    //                            valB += B * filterValue;
    //                        }
    //                    }
    //                    img[0][x, y] = valB;
    //                    img[1][x, y] = valG;
    //                    img[2][x, y] = valR;
    //                    img[3][x, y] = valA;
    //                }
    //                count++;
    //                if (bw_sender != null)
    //                    bw_sender.ReportProgress(Percentage.GetPercent(count, totalSteps));
    //            }

    //            smoothed[z] = new Data3D(img);
    //        }

    //        BitmapSource[] corrected = new BitmapSource[InputImages.Length];
    //        for (int z = 0; z < smoothed.Length; z++)
    //        {
    //            corrected[z] = ImageHelper.CreateImage(smoothed[z]);
    //        }

    //        return corrected;
    //    }

    //    public static Data2D GaussianFilter(Data2D InputData, float Sigma = 1.0f)
    //    {
    //        Data2D filtered = new Data2D(InputData.Width, InputData.Height);

    //        int szX = InputData.Width;
    //        int szY = InputData.Height;

    //        Kernel kernel = customGaussianKernel(Sigma);

    //        for (int x = 0; x < szX; x++)
    //        {
    //            for (int y = 0; y < szY; y++)
    //            {
    //                int startX = x - 3;
    //                int startY = y - 3;

    //                float val = 0;
    //                for (int a = 0; a < 5; a++)
    //                {
    //                    for (int b = 0; b < 5; b++)
    //                    {
    //                        int locX = startX + a;
    //                        int locY = startY + b;

    //                        //If pixel is outside bounds of image, use boundary pixels
    //                        //http://en.wikipedia.org/wiki/File:Extend_Edge-Handling.png
    //                        if (locX < 0) locX = 0;
    //                        else if (locX >= szX) locX = szX - 1;
    //                        if (locY < 0) locY = 0;
    //                        else if (locY >= szY) locY = szY - 1;

    //                        float V = InputData[locX, locY];

    //                        float filterValue = kernel[a, b];
    //                        if (!kernel.IsNormalized) filterValue /= kernel.NormalizationFactor;

    //                        val += V * filterValue;
    //                    }
    //                }
    //                filtered[x, y] = val;
    //            }
    //        }
    //        return filtered;
    //    }

    //    private static Kernel lowPassKernel = new Kernel(new float[3, 3]
    //    {
    //        {1f, 2f, 1f},
    //        {2f, 4f, 2f},
    //        {1f, 2f, 1f}
    //    });
    //    private static Kernel highPassKernel = new Kernel(new float[3, 3]
    //    {
    //        {-1f, -1f, -1f},
    //        {-1f, +8f, -1f},
    //        {-1f, -1f, -1f}
    //    });
    //    private static Kernel gaussianKernel = new Kernel(new float[5, 5]
    //    {
    //        {1f, 4f, 7f, 4f, 1f},
    //        {4f, 16f, 26f, 16f, 4f},
    //        {7f, 26f, 41f, 26f, 7f},
    //        {4f, 16f, 26f, 16f, 4f},
    //        {1f, 4f, 7f, 4f, 1f}
    //    });
    //    private static Kernel customGaussianKernel(float sigma)
    //    {
    //        float[,] h1 = new float[5, 5]
    //        {
    //            {-2, -1, 0, 1, 2},
    //            {-2, -1, 0, 1, 2},
    //            {-2, -1, 0, 1, 2},
    //            {-2, -1, 0, 1, 2},
    //            {-2, -1, 0, 1, 2},
    //        };
    //        float[,] h2 = new float[5, 5]
    //        {
    //            {-2, -2, -2, -2, -2},
    //            {-1, -1, -1, -1, -1},
    //            {0, 0, 0, 0, 0},
    //            {1, 1, 1, 1, 1},
    //            {2, 2, 2, 2, 2}
    //        };

    //        float[,] hg = new float[5, 5];
    //        float sum = 0;
    //        for (int x = 0; x < 5; x++)
    //        {
    //            for (int y = 0; y < 5; y++)
    //            {
    //                hg[x, y] = (float)Math.Exp(-((h1[x, y] * h1[x, y]) + (h2[x, y] * h2[x, y])) / (2 * sigma * sigma));
    //                sum += hg[x, y];
    //            }
    //        }

    //        float[,] h = new float[5, 5];
    //        for (int x = 0; x < 5; x++)
    //        {
    //            for (int y = 0; y < 5; y++)
    //            {
    //                h[x, y] = hg[x, y] / sum;
    //            }
    //        }

    //        return new Kernel(h);
    //    }

    //    private static Color MedianColor(List<float> A, List<float> R, List<float> G, List<float> B)
    //    {
    //        A.Sort();
    //        R.Sort();
    //        G.Sort();
    //        B.Sort();

    //        byte a;
    //        byte r;
    //        byte g;
    //        byte b;

    //        int countA = A.Count;
    //        if (countA != 0)
    //        {
    //            if (countA % 2 != 0)
    //            {
    //                a = (byte)(A[(countA / 2) + 1]);
    //            }
    //            else
    //            {
    //                a = (byte)((A[countA / 2] + A[(countA / 2) + 1]) / 2);
    //            }
    //        }
    //        else a = 0;

    //        int countR = R.Count;
    //        if (countR != 0)
    //        {
    //            if (countR % 2 != 0)
    //            {
    //                r = (byte)(R[(countR / 2) + 1]);
    //            }
    //            else
    //            {
    //                r = (byte)((R[countR / 2] + R[(countR / 2) + 1]) / 2);
    //            }
    //        }
    //        else r = 0;

    //        int countG = G.Count;
    //        if (countG != 0)
    //        {
    //            if (countG % 2 != 0)
    //            {
    //                g = (byte)(G[(countG / 2) + 1]);
    //            }
    //            else
    //            {
    //                g = (byte)((G[countG / 2] + G[(countG / 2) + 1]) / 2);
    //            }
    //        }
    //        else g = 0;

    //        int countB = B.Count;
    //        if (countB != 0)
    //        {
    //            if (countB % 2 != 0)
    //            {
    //                b = (byte)(B[(countB / 2) + 1]);
    //            }
    //            else
    //            {
    //                b = (byte)((B[countB / 2] + B[(countB / 2) + 1]) / 2);
    //            }
    //        }
    //        else b = 0;

    //        return Color.FromArgb(a, r, g, b);
    //    }
    //    private static int Average(List<float> Values)
    //    {
    //        float sum = 0;
    //        foreach (int i in Values)
    //        {
    //            sum += i;
    //        }
    //        return (int)(sum / (float)Values.Count);
    //    }
    //    private static double AverageD(List<double> Values)
    //    {
    //        double sum = 0;
    //        foreach (double d in Values)
    //        {
    //            sum += d;
    //        }
    //        return sum / (double)Values.Count;
    //    }
    //    private static Color MeanColor(List<float> A, List<float> R, List<float> G, List<float> B)
    //    {
    //        return Color.FromArgb((byte)Average(A), (byte)Average(R), (byte)Average(G), (byte)Average(B));
    //    }
    //}

    public static class Filter
    {
        public static Data3D DoFilter(Data3D input, FilterType filter)
        {
            Data3D filtered = new Data3D();

            for (int z = 0; z < input.Depth; z++)
            {
                filtered.AddLayer(DoFilter(input.Layers[z], filter));
            }

            return filtered;
        }
        public static Data2D DoFilter(Data2D input, FilterType filter)
        {
            return getFilterMethod(filter)(input);
        }
        public static BitmapSource DoFilter(BitmapSource input, FilterType filter)
        {
            Data3D filtered = DoFilter(ImageHelper.ConvertToData3D(input), filter);
            return ImageHelper.CreateImage(filtered);

        }
        public static BitmapSource[] DoFilter(BitmapSource[] input, FilterType filter)
        {
            BitmapSource[] output = new BitmapSource[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                output[i] = DoFilter(input[i], filter);
            }

            return output;
        }

        private static Data2D meanSmooth(Data2D input)
        {
            int sizeX = input.Width;
            int sizeY = input.Height;

            Data2D filtered = new Data2D(sizeX, sizeY);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    int startX = x - 3;
                    int startY = y - 3;

                    List<float> values = new List<float>();

                    for (int a = 0; a < 7; a++)
                    {
                        for (int b = 0; b < 7; b++)
                        {
                            int locX = startX + a;
                            int locY = startY + b;

                            if (locX < 0 || locX >= sizeX ||
                                locY < 0 || locY >= sizeY)
                                continue;

                            values.Add(input[locX, locY]);
                        }
                    }

                    filtered[x, y] = values.Average();
                }
            }

            return filtered;
        }
        private static Data2D medianSmooth(Data2D input)
        {
            int sizeX = input.Width;
            int sizeY = input.Height;

            Data2D filtered = new Data2D(sizeX, sizeY);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    int startX = x - 3;
                    int startY = y - 3;

                    List<float> values = new List<float>();

                    for (int a = 0; a < 7; a++)
                    {
                        for (int b = 0; b < 7; b++)
                        {
                            int locX = startX + a;
                            int locY = startY + b;

                            if (locX < 0 || locX >= sizeX ||
                                locY < 0 || locY >= sizeY)
                                continue;

                            values.Add(input[locX, locY]);
                        }
                    }

                    if (values.Count == 0)
                    {
                        filtered[x, y] = 0;
                    }
                    else
                    {
                        values.Sort();
                        int middle = values.Count / 2;
                        if (values.Count % 2 == 0)
                        {
                            filtered[x, y] = (values[middle] + values[middle + 1]) / 2;
                        }
                        else
                        {
                            filtered[x, y] = values[middle];
                        }
                    }
                }
            }

            return filtered;
        }
        private static Data2D gaussian(Data2D input)
        {
            int sizeX = input.Width;
            int sizeY = input.Height;

            Kernel kernel = gaussianKernel;

            Data2D filtered = new Data2D(sizeX, sizeY);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    int startX = x - 2;
                    int startY = y - 2;

                    float val = 0;
                    for (int a = 0; a < 5; a++)
                    {
                        for (int b = 0; b < 5; b++)
                        {
                            int locX = startX + a;
                            int locY = startY + b;

                            //If pixel is outside bounds of image, use boundary pixels
                            //http://en.wikipedia.org/wiki/File:Extend_Edge-Handling.png
                            if (locX < 0) locX = 0;
                            else if (locX >= sizeX) locX = sizeX - 1;
                            if (locY < 0) locY = 0;
                            else if (locY >= sizeY) locY = sizeY - 1;

                            float V = input[locX, locY];

                            float filterValue = kernel[a, b];
                            if (!kernel.IsNormalized) filterValue /= kernel.NormalizationFactor;

                            val += V * filterValue;
                        }
                    }
                    filtered[x, y] = val;
                }
            }
            return filtered;
        }
        private static Data2D lowPass(Data2D input)
        {
            int sizeX = input.Width;
            int sizeY = input.Height;

            Kernel kernel = lowPassKernel;

            Data2D filtered = new Data2D(sizeX, sizeY);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    int startX = x - 1;
                    int startY = y - 1;

                    float val = 0;
                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            int locX = startX + a;
                            int locY = startY + b;

                            //If pixel is outside bounds of image, use boundary pixels
                            //http://en.wikipedia.org/wiki/File:Extend_Edge-Handling.png
                            if (locX < 0) locX = 0;
                            else if (locX >= sizeX) locX = sizeX - 1;
                            if (locY < 0) locY = 0;
                            else if (locY >= sizeY) locY = sizeY - 1;

                            float V = input[locX, locY];

                            float filterValue = kernel[a, b];
                            if (!kernel.IsNormalized) filterValue /= kernel.NormalizationFactor;

                            val += V * filterValue;
                        }
                    }
                    filtered[x, y] = val;
                }
            }
            return filtered;
        }
        private static Data2D highPass(Data2D input)
        {
            int sizeX = input.Width;
            int sizeY = input.Height;

            Kernel kernel = highPassKernel;

            Data2D filtered = new Data2D(sizeX, sizeY);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    int startX = x - 1;
                    int startY = y - 1;

                    float val = 0;
                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            int locX = startX + a;
                            int locY = startY + b;

                            //If pixel is outside bounds of image, use boundary pixels
                            //http://en.wikipedia.org/wiki/File:Extend_Edge-Handling.png
                            if (locX < 0) locX = 0;
                            else if (locX >= sizeX) locX = sizeX - 1;
                            if (locY < 0) locY = 0;
                            else if (locY >= sizeY) locY = sizeY - 1;

                            float V = input[locX, locY];

                            float filterValue = kernel[a, b];
                            if (!kernel.IsNormalized) filterValue /= kernel.NormalizationFactor;

                            val += V * filterValue;
                        }
                    }
                    filtered[x, y] = val;
                }
            }
            return filtered;
        }

        private static FilterMethod getFilterMethod(FilterType filter)
    {
        switch (filter)
        {
            case FilterType.Gauissian:
                return gaussian;
            case FilterType.HighPass:
                return highPass;
            case FilterType.LowPass:
                return lowPass;
            case FilterType.MeanSmooth:
                return meanSmooth;
            case FilterType.MedianSmooth:
                return medianSmooth;
            default:
                throw new ArgumentException("Invalid filter type.");
        }
    }

        private static Kernel lowPassKernel = new Kernel(new float[3, 3]
        {
            {1f, 2f, 1f},
            {2f, 4f, 2f},
            {1f, 2f, 1f}
        });
        private static Kernel highPassKernel = new Kernel(new float[3, 3]
        {
            {-1f, -1f, -1f},
            {-1f, +8f, -1f},
            {-1f, -1f, -1f}
        });
        private static Kernel gaussianKernel = new Kernel(new float[5, 5]
        {
            {1f, 4f, 7f, 4f, 1f},
            {4f, 16f, 26f, 16f, 4f},
            {7f, 26f, 41f, 26f, 7f},
            {4f, 16f, 26f, 16f, 4f},
            {1f, 4f, 7f, 4f, 1f}
        });
    }
    public enum FilterType
    {
        MeanSmooth,
        MedianSmooth,
        HighPass,
        LowPass,
        Gauissian
    }
    delegate Data2D FilterMethod(Data2D input);

    public class Kernel
    {
        float[,] _values;

        public int Width
        {
            get { return _values.GetLength(0); }
        }
        public int Height
        {
            get { return _values.GetLength(1); }
        }
        public bool IsNormalized
        {
            get
            {
                return Math.Abs(1.0f - Sum) <= 0.1d;
            }
        }
        public float Max
        {
            get
            {
                float max = 0;
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (_values[x, y] > max) max = _values[x, y];
                    }
                }
                return max;
            }
        }
        public float Sum
        {
            get
            {
                float sum = 0;
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        sum += _values[x, y];
                    }
                }
                return sum;
            }
        }
        public float NormalizationFactor
        {
            get { return Sum; }
        }

        public float this[int x, int y]
        {
            get { return _values[x, y]; }
        }
        public Kernel(float[,] Values)
        {
            _values = Values;
        }
    }
}