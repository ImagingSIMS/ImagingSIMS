using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImagingSIMS.Data.Imaging
{
    public class CPUImageGenerator : BaseImageGenerator
    {
        public override BitmapSource Create(Data3D channelData)
        {
            int sizeX = channelData.Width;
            int sizeY = channelData.Height;

            int pos = 0;

            PixelFormat pf = PixelFormats.Bgr32;
            int byteStride = 4;
            if (channelData.LayerMaximum(3) != float.MinValue)
            {
                pf = PixelFormats.Bgra32;
                byteStride = 4;
            }

            int rawStride = (sizeX * pf.BitsPerPixel) / 8;
            byte[] rawImage = new byte[rawStride * sizeY];

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    for (int i = 0; i < byteStride; i++)
                    {
                        rawImage[pos + i] = (byte)channelData[x, y, i];
                    }
                    pos += byteStride;
                }
            }

            return BitmapSource.Create(sizeX, sizeY, 96, 96, pf, null, rawImage, rawStride);
        }
        public override BitmapSource[] Create(ImageComponent[] components, ImagingParameters parameters)
        {
            int numComps = components.Length;
            int numLayers = components[0].NumberTables;
            int sizeX = components[0].Width;
            int sizeY = components[0].Height;

            int numReturnImages = numLayers;
            if (parameters.TotalIon) numReturnImages++;
            BitmapSource[] result = new BitmapSource[numReturnImages];

            float[,] normValues = new float[numLayers, numComps];
            float[,] totalIon = new float[sizeX, sizeY];
            switch (parameters.NormalizationMethod)
            {
                case NormalizationMethod.Both:
                    for (int x = 0; x < numLayers; x++)
                    {
                        for (int y = 0; y < numComps; y++)
                        {
                            normValues[x, y] = components[y].LayerMaximum(x);
                        }
                    }
                    break;
                case NormalizationMethod.Layered:
                    for (int i = 0; i < numLayers; i++)
                    {
                        float max2 = 0;
                        for (int j = 0; j < numComps; j++)
                        {
                            if (components[j].LayerMaximum(i) > max2)
                                max2 = components[j].LayerMaximum(i);
                        }
                        for (int y = 0; y < numComps; y++)
                        {
                            normValues[i, y] = max2;
                        }
                    }
                    break;
                case NormalizationMethod.Single:
                    float max3 = 0;
                    foreach (ImageComponent c in components)
                    {
                        if (c.Maximum > max3) max3 = c.Maximum;
                    }
                    for (int x = 0; x < numLayers; x++)
                    {
                        for (int y = 0; y < numComps; y++)
                        {
                            normValues[x, y] = max3;
                        }
                    }
                    break;
            }

            for (int i = 0; i < numLayers; i++)
            {
                int pos = 0;

                PixelFormat pf = PixelFormats.Bgr32;

                int rawStride = (sizeX * pf.BitsPerPixel) / 8;
                rawStride = rawStride + (rawStride % 4) * 4;
                byte[] rawImage = new byte[rawStride * sizeY];

                for (int y = 0; y < sizeY; y++)
                {
                    for (int x = 0; x < sizeX; x++)
                    {
                        float sum = 0;
                        for (int z = 0; z < numComps; z++)
                        {
                            sum += (components[z].Data[i][x, y] * 255f) / normValues[i, z];
                        }

                        totalIon[x, y] += sum;

                        if (sum == 0)
                        {
                            pos += 4;
                            continue;
                        }

                        byte r = 0;
                        byte g = 0;
                        byte b = 0;
                        for (int z = 0; z < numComps; z++)
                        {
                            float value = (components[z].Data[i][x, y] * 255f) / normValues[i, z];
                            float weight = (value / sum);


                            r += (byte)((weight * value * components[z].PixelColor.R) / 255f);
                            g += (byte)((weight * value * components[z].PixelColor.G) / 255f);
                            b += (byte)((weight * value * components[z].PixelColor.B) / 255f);
                        }


                        if (parameters.SqrtEnhance)
                        {
                            rawImage[pos + 0] = (byte)((Math.Sqrt(b)) * (Math.Sqrt(255)));
                            rawImage[pos + 1] = (byte)((Math.Sqrt(g)) * (Math.Sqrt(255)));
                            rawImage[pos + 2] = (byte)((Math.Sqrt(r)) * (Math.Sqrt(255)));
                        }
                        else
                        {
                            rawImage[pos + 0] = b;
                            rawImage[pos + 1] = g;
                            rawImage[pos + 2] = r;
                        }

                        pos += 4;
                    }
                }
                result[i] = BitmapSource.Create(sizeX, sizeY, 96, 96, pf, null, rawImage, rawStride);
            }

            if (parameters.TotalIon)
            {
                Data2D d = new Data2D(totalIon);
                result[result.Length - 1] = Create(d, ColorScaleTypes.Gray);
            }
            return result;
        }
        public override BitmapSource Create(Data2D data, Color solidColor)
        {
            int sizeX = data.Width;
            int sizeY = data.Height;

            int pos = 0;

            PixelFormat pf = PixelFormats.Bgr32;

            int rawStride = (sizeX * pf.BitsPerPixel) / 8;
            rawStride = rawStride + (rawStride % 4) * 4;
            byte[] rawImage = new byte[rawStride * sizeY];

            float max = data.Maximum;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    Color c = ColorScales.Solid(data[x, y], max, solidColor);

                    rawImage[pos + 0] = c.B;
                    rawImage[pos + 1] = c.G;
                    rawImage[pos + 2] = c.R;

                    pos += 4;
                }
            }
            return BitmapSource.Create(sizeX, sizeY, 96, 96, pf, null, rawImage, rawStride);
        }
        public override BitmapSource Create(Data2D data, ColorScaleTypes scale)
        {
            int sizeX = data.Width;
            int sizeY = data.Height;

            int pos = 0;

            PixelFormat pf = PixelFormats.Bgr32;

            int rawStride = (sizeX * pf.BitsPerPixel) / 8;
            rawStride = rawStride + (rawStride % 4) * 4;
            byte[] rawImage = new byte[rawStride * sizeY];

            float max = data.Maximum;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    Color c = ColorScales.FromScale(data[x, y], max, scale);

                    rawImage[pos + 0] = c.B;
                    rawImage[pos + 1] = c.G;
                    rawImage[pos + 2] = c.R;

                    pos += 4;
                }
            }
            return BitmapSource.Create(sizeX, sizeY, 96, 96, pf, null, rawImage, rawStride);
        }
        public override BitmapSource Create(Data2D data, Color solidColor, float saturation)
        {
            int sizeX = data.Width;
            int sizeY = data.Height;

            int pos = 0;

            PixelFormat pf = PixelFormats.Bgr32;

            int rawStride = (sizeX * pf.BitsPerPixel) / 8;
            rawStride = rawStride + (rawStride % 4) * 4;
            byte[] rawImage = new byte[rawStride * sizeY];

            float max = saturation;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    Color c = ColorScales.Solid(data[x, y], max, solidColor);

                    rawImage[pos + 0] = c.B;
                    rawImage[pos + 1] = c.G;
                    rawImage[pos + 2] = c.R;

                    pos += 4;
                }
            }
            return BitmapSource.Create(sizeX, sizeY, 96, 96, pf, null, rawImage, rawStride);
        }
        public override BitmapSource Create(Data2D data, ColorScaleTypes scale, float saturation)
        {
            int sizeX = data.Width;
            int sizeY = data.Height;

            int pos = 0;

            PixelFormat pf = PixelFormats.Bgr32;

            int rawStride = (sizeX * pf.BitsPerPixel) / 8;
            rawStride = rawStride + (rawStride % 4) * 4;
            byte[] rawImage = new byte[rawStride * sizeY];

            float max = saturation;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    Color c = ColorScales.FromScale(data[x, y], max, scale);

                    rawImage[pos + 0] = c.B;
                    rawImage[pos + 1] = c.G;
                    rawImage[pos + 2] = c.R;

                    pos += 4;
                }
            }
            return BitmapSource.Create(sizeX, sizeY, 96, 96, pf, null, rawImage, rawStride);
        }
        public override BitmapSource Create(Data2D data, Color solidColor, float saturation, float threshold)
        {
            int sizeX = data.Width;
            int sizeY = data.Height;

            int pos = 0;

            PixelFormat pf = PixelFormats.Bgr32;

            int rawStride = (sizeX * pf.BitsPerPixel) / 8;
            rawStride = rawStride + (rawStride % 4) * 4;
            byte[] rawImage = new byte[rawStride * sizeY];

            float max = saturation;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    float value = data[x, y] >= threshold ? data[x, y] : 0;

                    Color c = ColorScales.FromScale(value, max, ColorScaleTypes.Solid, solidColor);

                    rawImage[pos + 0] = c.B;
                    rawImage[pos + 1] = c.G;
                    rawImage[pos + 2] = c.R;


                    pos += 4;
                }
            }
            return BitmapSource.Create(sizeX, sizeY, 96, 96, pf, null, rawImage, rawStride);
        }
        public override BitmapSource Create(Data2D data, ColorScaleTypes scale, float saturation, float threshold)
        {
            int sizeX = data.Width;
            int sizeY = data.Height;

            int pos = 0;

            PixelFormat pf = PixelFormats.Bgr32;

            int rawStride = (sizeX * pf.BitsPerPixel) / 8;
            rawStride = rawStride + (rawStride % 4) * 4;
            byte[] rawImage = new byte[rawStride * sizeY];

            float max = saturation;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    float value = data[x, y] >= threshold ? data[x, y] : 0;

                    Color c = ColorScales.FromScale(value, max, scale);

                    rawImage[pos + 0] = c.B;
                    rawImage[pos + 1] = c.G;
                    rawImage[pos + 2] = c.R;

                    pos += 4;
                }
            }
            return BitmapSource.Create(sizeX, sizeY, 96, 96, pf, null, rawImage, rawStride);
        }
    }
}
