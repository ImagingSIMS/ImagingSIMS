using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using ImagingSIMS.Common;
using ImagingSIMS.Common.Math;
using ImagingSIMS.Data.Converters;

namespace ImagingSIMS.Data.Imaging
{
    public static class ImageHelper
    {
        const float Zero = 0.0f;
        const float OneThird = 0.333333f;
        const float TwoThirds = 0.666666f;
        const float One = 1.0f;

        /// <summary>
        /// Creates a series of images from the specified image components.
        /// </summary>
        /// <param name="Components">Array of image components to generate images.</param>
        /// <param name="Parameters">Image parameters.</param>
        /// <returns></returns>
        public static BitmapSource[] CreateImage(ImageComponent[] Components, ImageParameters Parameters)
        {
            int numComps = Components.Length;
            int numLayers = Components[0].NumberTables;
            int sizeX = Components[0].Width;
            int sizeY = Components[0].Height;

            int numReturnImages = numLayers;
            if (Parameters.TotalIon) numReturnImages++;
            BitmapSource[] result = new BitmapSource[numReturnImages];

            float[,] normValues = new float[numLayers, numComps];
            float[,] totalIon = new float[sizeX, sizeY];
            switch (Parameters.NormalizationMethod)
            {
                case NormalizationMethod.Both:
                    for (int x = 0; x < numLayers; x++)
                    {
                        for (int y = 0; y < numComps; y++)
                        {
                            normValues[x, y] = Components[y].LayerMaximum(x);
                        }
                    }
                    break;
                case NormalizationMethod.Layered:
                    for (int i = 0; i < numLayers; i++)
                    {
                        float max2 = 0;
                        for (int j = 0; j < numComps; j++)
                        {
                            if (Components[j].LayerMaximum(i) > max2)
                                max2 = Components[j].LayerMaximum(i);
                        }
                        for (int y = 0; y < numComps; y++)
                        {
                            normValues[i, y] = max2;
                        }
                    }
                    break;
                case NormalizationMethod.Single:
                    float max3 = 0;
                    foreach (ImageComponent c in Components)
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
                            sum += (Components[z].Data[i][x, y] * 255f) / normValues[i, z];
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
                            float value = (Components[z].Data[i][x, y] * 255f) / normValues[i, z];
                            float weight = (value / sum);


                            r += (byte)((weight * value * Components[z].PixelColor.R) / 255f);
                            g += (byte)((weight * value * Components[z].PixelColor.G) / 255f);
                            b += (byte)((weight * value * Components[z].PixelColor.B) / 255f);
                        }


                        if (Parameters.SqrtEnhance)
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

            if (Parameters.TotalIon)
            {
                Data2D d = new Data2D(totalIon);
                result[result.Length - 1] = ImageHelper.CreateColorScaleImage(d, ColorScaleTypes.Gray);
            }
            return result;
        }
        /// <summary>
        /// Converts the specified BitmapSource to a Data2D array which is the equivalent of the grayscale image.
        /// </summary>
        /// <param name="BitmapSource">BitmapSource to convert.</param>
        /// <returns>Data2D array which is the grayscale values if the input image is RGB or the alpha values if the image is ARGB.</returns>
        public static Data2D ConvertToData2D(BitmapSource BitmapSource)
        {
            PixelFormat pf = BitmapSource.Format;

            int sizeX = BitmapSource.PixelWidth;
            int sizeY = BitmapSource.PixelHeight;

            int stride = (sizeX * pf.BitsPerPixel) / 8;
            int size = sizeY * stride;
            byte[] pixels = new byte[size];
            BitmapSource.CopyPixels(pixels, stride, 0);

            Data2D array = new Data2D(sizeX, sizeY);

            int pos = 0;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    if(pf == PixelFormats.Indexed8)
                    {
                        Color c = BitmapSource.Palette.Colors[pixels[pos]];

                        array[x, y] = MathEx.Average(c.R, c.G, c.B);

                        pos += 1;
                    }
                    else if (pf == PixelFormats.Gray8)
                    {
                        Color c = BitmapSource.Palette.Colors[pixels[pos]];

                        array[x, y] = MathEx.Average(c.R, c.G, c.B);

                        pos += 1;
                    }
                    else if (pf == PixelFormats.Bgr24)
                    {
                        byte b = pixels[pos + 0];      //Blue
                        byte g = pixels[pos + 1];      //Green
                        byte r = pixels[pos + 2];      //Red

                        array[x, y] = MathEx.Average(r, g, b);

                        pos += 3;
                    }
                    else if (pf == PixelFormats.Bgr32)
                    {
                        byte b = pixels[pos + 0];      //Blue
                        byte g = pixels[pos + 1];      //Green
                        byte r = pixels[pos + 2];      //Red

                        array[x, y] = MathEx.Average(r, g, b);

                        pos += 4;
                    }
                    else if (pf == PixelFormats.Bgra32)
                    {
                        //byte a = pixels[pos + 3];      //Alpha

                        //array[x, y] = a;
                        byte b = pixels[pos + 0];      //Blue
                        byte g = pixels[pos + 1];      //Green
                        byte r = pixels[pos + 2];      //Red

                        array[x, y] = MathEx.Average(r, g, b);

                        pos += 4;
                    }

                    else throw new FormatException($"Invalid image format type: {pf.ToString()}.");
                }
            }

            return array;
        }
        /// <summary>
        /// Converts the specified BitmapSource to a Data2D array based on the specified parameters.
        /// </summary>
        /// <param name="BitmapSource">BitmapSource to convert.</param>
        /// <param name="Conversion">Conversion type to implement.</param>
        /// <param name="Color">If the conversion parameter is Data2DConversionType.Color, this is the base color to compare to.</param>
        /// <returns>Data2D array.</returns>
        public static Data2D ConvertToData2D(BitmapSource BitmapSource, Data2DConverionType Conversion, Color? Color = null)
        {
            PixelFormat pf = BitmapSource.Format;

            int sizeX = BitmapSource.PixelWidth;
            int sizeY = BitmapSource.PixelHeight;

            int stride = (sizeX * pf.BitsPerPixel) / 8;
            int size = sizeY * stride;
            byte[] pixels = new byte[size];
            BitmapSource.CopyPixels(pixels, stride, 0);

            Data2D array = new Data2D(sizeX, sizeY);

            Color color = new Color();
            if (Color.HasValue)
            {
                color = Color.Value;
            }

            int pos = 0;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    byte b = 0;
                    byte g = 0;
                    byte r = 0;

                    if (pf == PixelFormats.Indexed8)
                    {
                        Color c = BitmapSource.Palette.Colors[pixels[pos]];
                        b = c.B;
                        g = c.G;
                        r = c.R;

                        pos += 1;
                    }
                    else if (pf == PixelFormats.Gray8)
                    {
                        Color c = BitmapSource.Palette.Colors[pixels[pos]];
                        b = c.B;
                        g = c.G;
                        r = c.R;

                        pos += 1;
                    }
                    else if (pf == PixelFormats.Bgr24)
                    {
                        b = pixels[pos + 0];      //Blue
                        g = pixels[pos + 1];      //Green
                        r = pixels[pos + 2];      //Red

                        pos += 3;
                    }
                    else if (pf == PixelFormats.Bgr32)
                    {
                        b = pixels[pos + 0];      //Blue
                        g = pixels[pos + 1];      //Green
                        r = pixels[pos + 2];      //Red

                        pos += 4;
                    }
                    else if (pf == PixelFormats.Bgra32)
                    {
                        b = pixels[pos + 0];      //Blue
                        g = pixels[pos + 1];      //Green
                        r = pixels[pos + 2];      //Red

                        pos += 4;
                    }

                    else throw new FormatException($"Invalid image format type: {pf.ToString()}.");

                    switch (Conversion)
                    {
                        case Data2DConverionType.Color:
                            array[x, y] = GetFromColor(new byte[3] { b, g, r }, color);
                            break;
                        case Data2DConverionType.Grayscale:
                            array[x, y] = GetFromGray(new byte[3] { b, g, r });
                            break;
                        case Data2DConverionType.Thermal:
                            array[x, y] = GetFromThermal(new byte[3] { b, g, r });
                            break;
                    }
                }
            }

            return array;
        }
        /// <summary>
        /// Converts a Data3D representing image data to a Data2D grayscale image.
        /// </summary>
        /// <param name="Source">Data3D object containing color channel data.</param>
        /// <returns>Grayscale version of the Data3D image.</returns>
        public static Data2D ConvertToData2D(Data3D Source)
        {
            int sizeX = Source.Width;
            int sizeY = Source.Height;

            Data2D array = new Data2D(sizeX, sizeY);

            if (Source.Depth == 3 || Source.Depth == 4)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                        array[x, y] = GetFromGray(new byte[3] 
                        { (byte)Source[x, y, 0], (byte)Source[x, y, 1], (byte)Source[sizeY, y, 2] });
                    }
                }

                return array;
            }

            else throw new ArgumentException("Invalid number of color channels.");           
        }

        /// <summary>
        /// Converts a thermal color value to a single byte value.
        /// </summary>
        /// <param name="b">3 byte array representing the thermal color.</param>
        /// <returns>Byte value of color.</returns>
        private static byte GetFromThermal(byte[] b)
        {
            return (MathEx.Average(b[0], b[1], b[2]));
        }
        /// <summary>
        /// Converts a gray color value to a single byte value.
        /// </summary>
        /// <param name="b">3 byte array representing the gray color.</param>
        /// <returns>Byte value of color.</returns>
        private static byte GetFromGray(byte[] b)
        {
            return (byte)((.299f * b[2]) + (.587f * b[1]) + (.114f * b[0]));
        }
        /// <summary>
        /// Converts a color value to a single byte value.
        /// </summary>
        /// <param name="b">3 byte array representing the color.</param>
        /// <param name="c">The color representing the maximum color (c = 255).</param>
        /// <returns>Byte value of color.</returns>
        private static byte GetFromColor(byte[] b, Color c)
        {
            List<float> ratios = new List<float>();

            if (c.B > 0) ratios.Add(b[0] / (float)c.B);
            if (c.G > 0) ratios.Add(b[1] / (float)c.G);
            if (c.R > 0) ratios.Add(b[2] / (float)c.R);

            return (byte)(MathEx.Average(ratios.ToArray<float>()) * 255f);
        }
        /// <summary>
        /// Converts the specified BitmapSource to a set of Data2D arrays, one for each color channel.
        /// </summary>
        /// <param name="BitmapSource">BitmapSource to convert.</param>
        /// <returns>Data3D which is a set of 4 Data2D arrays. Data3D[0] = blue, Data3D[1] = green, Data[2] = red, Data3D[3] = alpha. 
        /// If the original BitmapSource is RGB32, the Data3D[3] will be initialized but have a value of 0 at each pixel.</returns>
        public static Data3D ConvertToData3D(BitmapSource BitmapSource)
        {
            PixelFormat pf = BitmapSource.Format;

            int sizeX = BitmapSource.PixelWidth;
            int sizeY = BitmapSource.PixelHeight;

            int stride = (sizeX * pf.BitsPerPixel) / 8;
            int size = sizeY * stride;
            byte[] pixels = new byte[size];
            BitmapSource.CopyPixels(pixels, stride, 0);
            
            Data2D[] arrays = new Data2D[4];
            for (int i = 0; i < 4; i++)
            {
                arrays[i] = new Data2D(sizeX, sizeY);
            }

            int pos = 0;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    if (pf == PixelFormats.Indexed8)
                    {
                        Color c = BitmapSource.Palette.Colors[pixels[pos]];

                        arrays[0][x, y] = c.B;
                        arrays[1][x, y] = c.G;
                        arrays[2][x, y] = c.R;

                        pos += 1;
                    }
                    else if (pf == PixelFormats.Gray8)
                    {
                        Color c = BitmapSource.Palette.Colors[pixels[pos]];

                        arrays[0][x, y] = c.B;
                        arrays[1][x, y] = c.G;
                        arrays[2][x, y] = c.R;

                        pos += 1;
                    }
                    else if (pf == PixelFormats.Bgr24)
                    {
                        arrays[0][x, y] = pixels[pos + 0];      //Blue
                        arrays[1][x, y] = pixels[pos + 1];      //Green
                        arrays[2][x, y] = pixels[pos + 2];      //Red
                        arrays[3][x, y] = 255f;                 //Alpha

                        pos += 3;
                    }
                    else if (pf == PixelFormats.Bgr32)
                    {
                        arrays[0][x, y] = pixels[pos + 0];      //Blue
                        arrays[1][x, y] = pixels[pos + 1];      //Green
                        arrays[2][x, y] = pixels[pos + 2];      //Red
                        arrays[3][x, y] = 255f;                 //Alpha

                        pos += 4;
                    }
                    else if (pf == PixelFormats.Bgra32)
                    {
                        arrays[0][x, y] = pixels[pos + 0];      //Blue
                        arrays[1][x, y] = pixels[pos + 1];      //Green
                        arrays[2][x, y] = pixels[pos + 2];      //Red
                        arrays[3][x, y] = pixels[pos + 3];      //Alpha

                        pos += 4;
                    }

                    else throw new FormatException($"Invalid image format type: {pf.ToString()}.");
                }
            }
            //int pos = 0;
            //for (int y = 0; y < sizeY; y++)
            //{
            //    for (int x = 0; x < sizeX; x++)
            //    {
            //        arrays[0][x, y] = pixels[pos + 0];      //Blue
            //        arrays[1][x, y] = pixels[pos + 1];      //Green
            //        arrays[2][x, y] = pixels[pos + 2];      //Red

            //        if (pf == PixelFormats.Bgr24)
            //        {
            //            arrays[3][x, y] = 255f;
            //            pos += 3;
            //        }
            //        else if (pf == PixelFormats.Bgr32)
            //        {
            //            arrays[3][x, y] = 255f;
            //            pos += 4;
            //        }
            //        else if (pf == PixelFormats.Bgra32)
            //        {
            //            arrays[3][x, y] = pixels[pos + 3];  //Alpha
            //            pos += 4;
            //        }
            //    }
            //}

            return new Data3D(arrays);
        }
        /// <summary>
        /// Creates an image from color channel data in the form of a Data3D object. If the alpha channel (Data3D[3]) is blank, 
        /// the image will be PixelFormats.BGR32 and PixelFormats.BGRA32 if not.
        /// </summary>
        /// <param name="ChannelData">Color channel data.</param>
        /// <returns>A BitmapSource from the specified data.</returns>
        public static BitmapSource CreateImage(Data3D ChannelData)
        {
            int sizeX = ChannelData.Width;
            int sizeY = ChannelData.Height;

            int pos = 0;            

            PixelFormat pf = PixelFormats.Bgr32;
            int byteStride = 4;
            if (ChannelData.LayerMaximum(3) != 0) 
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
                        rawImage[pos + i] = (byte)ChannelData[x, y, i];
                    }
                    pos += byteStride;
                }
            }

            return BitmapSource.Create(sizeX, sizeY, 96, 96, pf, null, rawImage, rawStride);
        }
        /// <summary>
        /// Converts a Data2D matrix into an image using the specified color scale.
        /// </summary>
        /// <param name="Data">Intensity matrix to create image with.</param>
        /// <param name="Scale">Color scale type.</param>
        /// <returns></returns>
        public static BitmapSource CreateColorScaleImage(Data2D Data, ColorScaleTypes Scale)
        {
            int sizeX = Data.Width;
            int sizeY = Data.Height;

            int pos = 0;

            PixelFormat pf = PixelFormats.Bgr32;

            int rawStride = (sizeX * pf.BitsPerPixel) / 8;
            rawStride = rawStride + (rawStride % 4) * 4;
            byte[] rawImage = new byte[rawStride * sizeY];

            float max = Data.Maximum;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    Color c = ColorScales.FromScale(Data[x, y], max, Scale);

                    rawImage[pos + 0] = c.B;
                    rawImage[pos + 1] = c.G;
                    rawImage[pos + 2] = c.R;

                    pos += 4;
                }
            }
            return BitmapSource.Create(sizeX, sizeY, 96, 96, pf, null, rawImage, rawStride);
        }
        public static BitmapSource CreateColorScaleImage(Data2D Data, float Saturation, ColorScaleTypes Scale)
        {
            int sizeX = Data.Width;
            int sizeY = Data.Height;

            int pos = 0;

            PixelFormat pf = PixelFormats.Bgr32;

            int rawStride = (sizeX * pf.BitsPerPixel) / 8;
            rawStride = rawStride + (rawStride % 4) * 4;
            byte[] rawImage = new byte[rawStride * sizeY];

            float max = Saturation;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    Color c = ColorScales.FromScale(Data[x, y], max, Scale);

                    rawImage[pos + 0] = c.B;
                    rawImage[pos + 1] = c.G;
                    rawImage[pos + 2] = c.R;

                    pos += 4;
                }
            }
            return BitmapSource.Create(sizeX, sizeY, 96, 96, pf, null, rawImage, rawStride);
        }
        public static BitmapSource CreateSolidColorImage(Data2D Data, Color SolidColor)
        {
            int sizeX = Data.Width;
            int sizeY = Data.Height;

            int pos = 0;

            PixelFormat pf = PixelFormats.Bgr32;

            int rawStride = (sizeX * pf.BitsPerPixel) / 8;
            rawStride = rawStride + (rawStride % 4) * 4;
            byte[] rawImage = new byte[rawStride * sizeY];

            float max = Data.Maximum;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    Color c = ColorScales.Solid(Data[x, y], max, SolidColor);

                    rawImage[pos + 0] = c.B;
                    rawImage[pos + 1] = c.G;
                    rawImage[pos + 2] = c.R;

                    pos += 4;
                }
            }
            return BitmapSource.Create(sizeX, sizeY, 96, 96, pf, null, rawImage, rawStride);
        }
        public static BitmapSource CreateSolidColorImage(Data2D Data, float Saturation, Color SolidColor)
        {
            int sizeX = Data.Width;
            int sizeY = Data.Height;

            int pos = 0;

            PixelFormat pf = PixelFormats.Bgr32;

            int rawStride = (sizeX * pf.BitsPerPixel) / 8;
            rawStride = rawStride + (rawStride % 4) * 4;
            byte[] rawImage = new byte[rawStride * sizeY];

            float max = Saturation;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    Color c = ColorScales.Solid(Data[x, y], max, SolidColor);

                    rawImage[pos + 0] = c.B;
                    rawImage[pos + 1] = c.G;
                    rawImage[pos + 2] = c.R;

                    pos += 4;
                }
            }
            return BitmapSource.Create(sizeX, sizeY, 96, 96, pf, null, rawImage, rawStride);
        }

        public static BitmapSource GetXZ(BitmapSource[] Images, int YCoord)
        {
            int sizeX = (int)Images[0].Width;
            int sizeY = (int)Images[0].Height;
            int sizeZ = Images.Length;

            Data3D[] images = new Data3D[sizeZ];
            for (int i = 0; i < sizeZ; i++)
            {
                images[i] = ConvertToData3D(Images[i]);
            }

            Data2D[] result = new Data2D[4];
            for (int i = 0; i < 4; i++)
            {
                result[i] = new Data2D(sizeX, sizeZ);
            }

            for (int x = 0; x < sizeX; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        result[i][x, z] = images[z][x, YCoord, i];
                    }
                }
            }

            return CreateImage(new Data3D(result));
        }
        public static BitmapSource GetYZ(BitmapSource[] Images, int XCoord)
        {
            int sizeX = (int)Images[0].Width;
            int sizeY = (int)Images[0].Height;
            int sizeZ = Images.Length;

            Data3D[] images = new Data3D[sizeZ];
            for (int i = 0; i < sizeZ; i++)
            {
                images[i] = ConvertToData3D(Images[i]);
            }

            Data2D[] result = new Data2D[4];
            for (int i = 0; i < 4; i++)
            {
                result[i] = new Data2D(sizeX, sizeZ);
            }

            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        result[i][y, z] = images[z][XCoord, y, z];
                    }
                }
            }

            return CreateImage(new Data3D(result));
        }

        public static Data3D Upscale(Data3D ToResize, int TargetWidth, int TargetHeight)
        {
            int lowResSizeX = ToResize.Width;
            int lowResSizeY = ToResize.Height;
            int highResSizeX = TargetWidth;
            int highResSizeY = TargetHeight;

            if (lowResSizeX > highResSizeX || lowResSizeY > highResSizeY)
                throw new ArgumentException("Cannot downscale data using this function.");

            Data2D[] resized = new Data2D[4];
            for (int i = 0; i < 4; i++)
            {
                resized[i] = new Data2D(highResSizeX, highResSizeY);
            }

            float A, B, C, D;
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

                    for (int k = 0; k < 4; k++)
                    {
                        int x1 = x + 1;
                        if (x1 >= highResSizeX) x1 = x;
                        int y1 = y + 1;
                        if (y1 >= highResSizeY) y1 = y;

                        A = ToResize[x, y, k];
                        B = ToResize[x + 1, y, k];
                        C = ToResize[x, y + 1, k];
                        D = ToResize[x + 1, y + 1, k];

                        resized[k][i, j] = (A * (1f - xDiff) * (1f - yDiff)) + (B * (xDiff) * (1f - yDiff)) +
                            (C * (yDiff) * (1f - xDiff)) + (D * (xDiff) * (yDiff));
                    }
                }
            }

            return new Data3D(resized);
        }
        public static Data2D Upscale(Data2D ToResize, int TargetWidth, int TargetHeight)
        {
            int lowResSizeX = ToResize.Width;
            int lowResSizeY = ToResize.Height;
            int highResSizeX = TargetWidth;
            int highResSizeY = TargetHeight;

            if (lowResSizeX > highResSizeX || lowResSizeY > highResSizeY)
                throw new ArgumentException("Cannot downscale data using this function.");

            Data2D resized = new Data2D(highResSizeX, highResSizeY);

            float A, B, C, D;
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

                    A = ToResize[x, y];
                    B = ToResize[x + 1, y];
                    C = ToResize[x, y + 1];
                    D = ToResize[x + 1, y + 1];

                    resized[i, j] = (A * (1f - xDiff) * (1f - yDiff)) + (B * (xDiff) * (1f - yDiff)) +
                        (C * (yDiff) * (1f - xDiff)) + (D * (xDiff) * (yDiff));
                }
            }

            return resized;
        }
        public static Color[,] Upscale(Color[,] ToResize, int TargetWidth, int TargetHeight)
        {
            int lowResSizeX = ToResize.GetLength(0);
            int lowResSizeY = ToResize.GetLength(1);
            int highResSizeX = TargetWidth;
            int highResSizeY = TargetHeight;

            if (lowResSizeX > highResSizeX || lowResSizeY > highResSizeY)
                throw new ArgumentException("Cannot downscale data using this function.");

            Color[,] resized = new Color[highResSizeX, highResSizeY];

            Color A, B, C, D;
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

                    A = ToResize[x, y];
                    B = ToResize[x + 1, y];
                    C = ToResize[x, y + 1];
                    D = ToResize[x + 1, y + 1];

                    resized[i, j] = (A * (1f - xDiff) * (1f - yDiff)) + (B * (xDiff) * (1f - yDiff)) +
                        (C * (yDiff) * (1f - xDiff)) + (D * (xDiff) * (yDiff));
                    
                }
            }

            return resized;
        }
        public static SharpDX.Color[,] Upscale(SharpDX.Color[,] ToResize, int TargetWidth, int TargetHeight)
        {
            int lowResSizeX = ToResize.GetLength(0);
            int lowResSizeY = ToResize.GetLength(1);
            int highResSizeX = TargetWidth;
            int highResSizeY = TargetHeight;

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

                    A = ToResize[x, y];
                    B = ToResize[x + 1, y];
                    C = ToResize[x, y + 1];
                    D = ToResize[x + 1, y + 1];

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

    public struct ImageParameters
    {
        public bool SqrtEnhance { get; set; }
        public bool TotalIon { get; set; }
        public NormalizationMethod NormalizationMethod { get; set; }
    }
    public class DisplayImage : Image
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title",
            typeof(string), typeof(DisplayImage));
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description",
            typeof(string), typeof(DisplayImage));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public DisplayImage()
            : base()
        {
            byte[] pixels = new byte[3] { 0, 0, 0 };
            this.Source = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgr24, null, pixels, 3);
            this.Source.Changed += Source_Changed;

            Source_Changed(this, EventArgs.Empty);
        }
        public DisplayImage(ImageSource Source, string Title)
            : base()
        {
            this.Title = Title;

            if (Source.IsFrozen)
            {
                this.Source = Source.Clone();
            }
            else
            {
                this.Source = Source;
            }
            this.Source.Changed += Source_Changed;

            Source_Changed(this, EventArgs.Empty);
        }

        void Source_Changed(object sender, EventArgs e)
        {
            if (Source == null) return;

            BitmapSource source = Source as BitmapSource;

            if (source == null)
            {
                Description = string.Format("{0}\n{1}x{2} px; {3}",
                  Title, "?", "?", "?");
            }
            else
            {
                PixelFormat format = ((BitmapSource)Source).Format;
                int width = source.PixelWidth;
                int height = source.PixelHeight;

                Description = string.Format("{0}\n{1}x{2} px; {3}",
                    Title, width, height, format.ToString());
            }            
        }

        public DisplayImage Clone()
        {
            DisplayImage img = new DisplayImage(this.Source.CloneCurrentValue(), this.Title);
            img.Description = this.Description;
            return img;
        }

        public int Hash
        {
            get { return this.GetHashCode(); }
        }
    }
    public sealed class DisplaySeries : Data, ISavable
    {
        ObservableCollection<DisplayImage> _images;

        public string SeriesName
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged("SeriesName");
                }
            }
        }
        public ObservableCollection<DisplayImage> Images
        {
            get { return _images; }
            set
            {
                if (_images != value)
                {
                    _images = value;
                    NotifyPropertyChanged("Images");
                }
            }
        }

        int _numberImages;
        public int NumberImages
        {
            get { return _numberImages; }
            set
            {
                if (_numberImages != value)
                {
                    _numberImages = value;
                    this.NotifyPropertyChanged("NumberImages");
                }
            }
        }

        public DisplaySeries()
        {
            this.Images = new ObservableCollection<DisplayImage>();

            this.Images.CollectionChanged += Images_CollectionChanged;
        }
        public DisplaySeries(DisplayImage[] Images)
        {
            this.Images = new ObservableCollection<DisplayImage>();

            this.Images.CollectionChanged += Images_CollectionChanged;

            foreach (DisplayImage i in Images)
            {
                _images.Add(i);
            }
        }

        void Images_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NumberImages = Images.Count;
        }

        #region ISavable

        // Layout:
        // (string)         SeriesName
        // (int)            NumberImages
        // (DisplayImage)   Images
        //      -(sting)    -Title
        //      -(string)   -Description
        //      -(int)      -Width
        //      -(int)      -Height
        //      -(int)      -Depth
        //      -(Data3D)   -Matrix
        public byte[] ToByteArray()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(ms);

                // Offset start of writing to later go back and 
                // prefix the stream with the size of the stream written
                bw.Seek(sizeof(int), SeekOrigin.Begin);

                // Write contents of object

                bw.Write(SeriesName);
                bw.Write(NumberImages);

                for (int i = 0; i < NumberImages; i++)
                {
                    DisplayImage image = Images[i];
                    bw.Write(image.Title);
                    bw.Write(image.Description);

                    Data3D d = Imaging.ImageHelper.ConvertToData3D(image.Source as BitmapSource);

                    bw.Write(d.Width);
                    bw.Write(d.Height);
                    bw.Write(d.Depth);

                    for (int z = 0; z < d.Depth; z++)
                    {
                        for (int x = 0; x < d.Width; x++)
                        {
                            for (int y = 0; y < d.Height; y++)
                            {
                                if (d[x, y, z] == 0) bw.Write(false);
                                else
                                {
                                    bw.Write(true);
                                    bw.Write(d[x, y, z]);
                                }
                            }
                        }
                    }
                }

                // Return to beginning of writer and write the length
                bw.Seek(0, SeekOrigin.Begin);
                bw.Write((int)bw.BaseStream.Length);

                // Return to start of memory stream and 
                // return byte array of the stream
                ms.Seek(0, SeekOrigin.Begin - sizeof(int));
                return ms.ToArray();
            }
        }

        public void FromByteArray(byte[] array)
        {
            using (MemoryStream ms = new MemoryStream(array))
            {
                BinaryReader br = new BinaryReader(ms);

                string seriesName = br.ReadString();
                int numberImages = br.ReadInt32();

                this.SeriesName = seriesName;

                for (int i = 0; i < numberImages; i++)
                {
                    string title = br.ReadString();
                    string description = br.ReadString();

                    int width = br.ReadInt32();
                    int height = br.ReadInt32();
                    int depth = br.ReadInt32();

                    Data3D d = new Data3D(width, height, depth);
                    for (int z = 0; z < depth; z++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                if (br.ReadBoolean())
                                {
                                    d[x, y, z] = br.ReadSingle();
                                }
                            }
                        }
                    }

                    BitmapSource bs = ImageHelper.CreateImage(d);
                    DisplayImage image = new DisplayImage(bs, title);
                    image.Description = description;
                    _images.Add(image);
                }
            }
        }
        #endregion
    }

    public sealed class ImageSeries : Data
    {
        ObservableCollection<Image> _images;
        string[] _titles;

        public string SeriesName
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged("SeriesName");
                }
            }
        }
        public ObservableCollection<Image> Images
        {
            get { return _images; }
            set
            {
                if (_images != value)
                {
                    _images = value;
                    NotifyPropertyChanged("Images");
                }
            }
        }
        public string[] Titles
        {
            get { return _titles; }
            set
            {
                if (_titles != value)
                {
                    _titles = value;
                    NotifyPropertyChanged("Titles");
                }
            }
        }
        public int NumberImages
        {
            get
            {
                if (_images == null) return 0;
                return _images.Count;
            }
        }

        public ImageSeries()
        {
            _images = new ObservableCollection<Image>();
        }
        public ImageSeries(Image[] Images)
        {
            _images = new ObservableCollection<Image>();
            foreach (Image i in Images)
            {
                _images.Add(i);
            }
            NotifyPropertyChanged("NumberImages");
        }
        public ImageSeries(Image[] Images, string[] Titles)
        {
            _images = new ObservableCollection<Image>();
            foreach (Image i in Images)
            {
                _images.Add(i);
            }
            this.Titles = Titles;
            NotifyPropertyChanged("NumberImages");
        }

        public void AddImage(Image Image)
        {
            _images.Add(Image);
            NotifyPropertyChanged("NumberImages");
        }
        public void AddImages(Image[] Images)
        {
            foreach (Image i in Images)
            {
                _images.Add(i);
            }
            NotifyPropertyChanged("NumberImages");
        }
    }

    public enum NormalizationMethod
    {
        Single, Layered, Both
    }

    public static class CrossSection
    {
        public static BitmapSource GetXZ(BitmapSource[] ImageArray, int YCoord)
        {
            Data3D[] _data = new Data3D[ImageArray.Length];
            for (int i = 0; i < ImageArray.Length; i++)
            {
                _data[i] = ImageHelper.ConvertToData3D(ImageArray[i]);
            }

            int sizeX = _data[0].Width;
            int sizeY = _data[0].Height;
            int sizeZ = _data.Length;

            Data2D[] data = new Data2D[4];
            for (int i = 0; i < 4; i++)
            {
                data[i] = new Data2D(sizeX, sizeZ);
            }

            for (int x = 0; x < sizeX; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        data[i][x, z] = data[z][x, YCoord];
                    }
                }
            }

            return ImageHelper.CreateImage(new Data3D(data));
        }
        public static BitmapSource GetYZ(BitmapSource[] ImageArray, int XCoord)
        {
            Data3D[] _data = new Data3D[ImageArray.Length];
            for (int i = 0; i < ImageArray.Length; i++)
            {
                _data[i] = ImageHelper.ConvertToData3D(ImageArray[i]);
            }

            int sizeX = _data[0].Width;
            int sizeY = _data[0].Height;
            int sizeZ = _data.Length;

            Data2D[] data = new Data2D[4];
            for (int i = 0; i < 4; i++)
            {
                data[i] = new Data2D(sizeY, sizeZ);
            }

            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        data[i][y, z] = data[z][XCoord, y];
                    }
                }
            }

            return ImageHelper.CreateImage(new Data3D(data));
        }
    }

    public static class Overlay
    {
        public static BitmapSource CreateOverlay(BitmapSource[] Images)
        {
            Data3D[] images = new Data3D[Images.Length];
            for (int i = 0; i < Images.Length; i++)
            {
                images[i] = ImageHelper.ConvertToData3D(Images[i]);
            }

            int width = Images[0].PixelWidth;
            int height = Images[0].PixelHeight;
            int length = Images.Length;

            Data2D[] overlay = new Data2D[4];
            for (int i = 0; i < 4; i++)
            {
                overlay[i] = new Data2D(width, height);
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color[] pixels = new Color[length];
                    for (int z = 0; z < length; z++)
                    {
                        pixels[z] = Color.FromArgb((byte)images[z][x, y, 3], (byte)images[z][x, y, 2],
                            (byte)images[z][x, y, 1], (byte)images[z][x, y, 0]);
                    }
                    Color pixel = OverlayColors(pixels);
                    overlay[0][x, y] = pixel.B;
                    overlay[1][x, y] = pixel.G;
                    overlay[2][x, y] = pixel.R;
                    overlay[3][x, y] = 255f;
                }
            }

            return ImageHelper.CreateImage(new Data3D(overlay));
        }

        public static Color OverlayColors(Color[] Colors)
        {
            List<LAB> colors = new List<LAB>();
            foreach (Color c in Colors)
            {
                colors.Add(RGBtoLAB(new RGB(c.R, c.G, c.B)));
            }

            LAB overlay = new LAB();

            double sumL = 0;
            double sumA = 0;
            double sumB = 0;

            for (int j = 0; j < colors.Count; j++)
            {
                sumL += colors[j].l;
                sumA += colors[j].a;
                sumB += colors[j].b;
            }

            overlay.l = sumL / colors.Count;
            overlay.a = sumA / colors.Count;
            overlay.b = sumB / colors.Count;
            RGB rgb = LABtoRGB(overlay);
            return rgb.Color;
        }

        private static LAB RGBtoLAB(RGB RGB)
        {
            return XYZtoLAB(RGBtoXYZ(RGB));
        }
        private static RGB LABtoRGB(LAB LAB)
        {
            return XYZtoRGB(LABtoXYZ(LAB));
        }
        private static XYZ RGBtoXYZ(RGB RGB)
        {
            // normalize red, green, blue values
            double rLinear = (double)RGB.r / 255.0;
            double gLinear = (double)RGB.g / 255.0;
            double bLinear = (double)RGB.b / 255.0;

            // convert to a sRGB form
            double r = (rLinear > 0.04045) ? Math.Pow((rLinear + 0.055) / (
                1 + 0.055), 2.2) : (rLinear / 12.92);
            double g = (gLinear > 0.04045) ? Math.Pow((gLinear + 0.055) / (
                1 + 0.055), 2.2) : (gLinear / 12.92);
            double b = (bLinear > 0.04045) ? Math.Pow((bLinear + 0.055) / (
                1 + 0.055), 2.2) : (bLinear / 12.92);

            // converts
            return new XYZ(
                (r * 0.4124 + g * 0.3576 + b * 0.1805),
                (r * 0.2126 + g * 0.7152 + b * 0.0722),
                (r * 0.0193 + g * 0.1192 + b * 0.9505)
                );
        }
        private static LAB XYZtoLAB(XYZ XYZ)
        {
            LAB lab = LAB.Empty;

            lab.l = 116.0 * Fxyz(XYZ.y / XYZ.D65.y) - 16;
            lab.a = 500.0 * (Fxyz(XYZ.x / XYZ.D65.x) - Fxyz(XYZ.y / XYZ.D65.y));
            lab.b = 200.0 * (Fxyz(XYZ.y / XYZ.D65.y) - Fxyz(XYZ.z / XYZ.D65.z));

            return lab;
        }
        private static XYZ LABtoXYZ(LAB LAB)
        {
            double delta = 6.0 / 29.0;

            double fy = (LAB.l + 16) / 116.0;
            double fx = fy + (LAB.a / 500.0);
            double fz = fy - (LAB.b / 200.0);

            return new XYZ(
                (fx > delta) ? XYZ.D65.x * (fx * fx * fx) : (fx - 16.0 / 116.0) * 3 * (
                    delta * delta) * XYZ.D65.x,
                (fy > delta) ? XYZ.D65.y * (fy * fy * fy) : (fy - 16.0 / 116.0) * 3 * (
                    delta * delta) * XYZ.D65.y,
                (fz > delta) ? XYZ.D65.z * (fz * fz * fz) : (fz - 16.0 / 116.0) * 3 * (
                    delta * delta) * XYZ.D65.z
                );
        }
        private static RGB XYZtoRGB(XYZ XYZ)
        {
            double[] Clinear = new double[3];
            Clinear[0] = XYZ.x * 3.2410 - XYZ.y * 1.5374 - XYZ.z * 0.4986;  // red
            Clinear[1] = -XYZ.x * 0.9692 + XYZ.y * 1.8760 - XYZ.z * 0.0416; // green
            Clinear[2] = XYZ.x * 0.0556 - XYZ.y * 0.2040 + XYZ.z * 1.0570;  // blue

            for (int i = 0; i < 3; i++)
            {
                Clinear[i] = (Clinear[i] <= 0.0031308) ? 12.92 * Clinear[i] : (
                    1 + 0.055) * Math.Pow(Clinear[i], (1.0 / 2.4)) - 0.055;
            }

            return new RGB(
                Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                    Clinear[0] * 255.0))),
                Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                    Clinear[1] * 255.0))),
                Convert.ToInt32(Double.Parse(String.Format("{0:0.00}",
                    Clinear[2] * 255.0)))
                );
        }

        private static double Fxyz(double t)
        {
            return ((t > 0.008856) ? Math.Pow(t, (1.0 / 3.0)) : (7.787 * t + 16.0 / 116.0));
        }
        private static double[] ColorToArray(Color Color)
        {
            return new double[3] { Color.R, Color.G, Color.B };
        }
        private static Color ArrayToColor(double[] Array)
        {
            return Color.FromRgb((byte)Array[0], (byte)Array[1], (byte)Array[2]);
        }
    }

    internal struct RGB
    {
        private double red;
        private double green;
        private double blue;

        public double r
        {
            get { return red; }
            set
            {
                red = (value > 255) ? 255 : ((value < 0) ? 0 : value);
            }
        }
        public double g
        {
            get { return green; }
            set
            {
                green = (value > 255) ? 255 : ((value < 0) ? 0 : value);
            }
        }
        public double b
        {
            get { return blue; }
            set
            {
                blue = (value > 255) ? 255 : ((value < 0) ? 0 : value);
            }
        }

        public RGB(double r, double g, double b)
        {
            red = (r > 255) ? 255 : ((r < 0) ? 0 : r);
            green = (g > 255) ? 255 : ((g < 0) ? 0 : g);
            blue = (b > 255) ? 255 : ((b < 0) ? 0 : b);
        }
        public RGB(Color c)
        {
            red = (c.R > 255) ? 255 : ((c.R < 0) ? 0 : c.R);
            green = (c.G > 255) ? 255 : ((c.G < 0) ? 0 : c.G);
            blue = (c.B > 255) ? 255 : ((c.B < 0) ? 0 : c.B);
        }
        public Color Color
        {
            get 
            { 
                return Color.FromArgb(0, (byte)r, (byte)g, (byte)b);
            }
        }
    }
    internal struct XYZ
    {
        private double X;
        private double Y;
        private double Z;

        public double x
        {
            get { return X; }
            set
            {
                X = (value > 0.9505) ? 0.9505 : ((value < 0) ? 0 : value);
            }
        }
        public double y
        {
            get { return Y; }
            set
            {
                Y = (value > 0.9505) ? 0.9505 : ((value < 0) ? 0 : value);
            }
        }
        public double z
        {
            get { return Z; }
            set
            {
                Z = (value > 1.089) ? 1.089 : ((value < 0) ? 0 : value);
            }

        }

        /// <summary>
        /// Gets an empty CIEXYZ structure.
        /// </summary>
        public static readonly XYZ Empty = new XYZ();
        /// <summary>
        /// Gets the CIE D65 (white) structure.
        /// </summary>
        public static readonly XYZ D65 = new XYZ(0.9505, 1.0, 1.0890);

        public XYZ(double x, double y, double z)
        {
            X = (x > 0.9505) ? 0.9505 : ((x < 0) ? 0 : x);
            Y = (y > 1.0) ? 1.0 : ((y < 0) ? 0 : y);
            Z = (z > 1.089) ? 1.089 : ((z < 0) ? 0 : z);
        }
    }
    internal struct LAB
    {
        public double l;
        public double a;
        public double b;

        /// <summary>
        /// Gets an empty CIELab structure.
        /// </summary>
        public static readonly LAB Empty = new LAB();

        public LAB(double l, double a, double b)
        {
            this.l = l;
            this.a = a;
            this.b = b;
        }
    }

    public class EditableImage : Image
    {
        Data3D _visibleData;

        Dictionary<Point, float> _history;

        public readonly static DependencyProperty DataSourceProperty = DependencyProperty.Register("DataSource",
            typeof(Data2D), typeof(EditableImage));
        public readonly static DependencyProperty SelectionColorProperty = DependencyProperty.Register("SelectionColor",
            typeof(Color), typeof(EditableImage));
        public static readonly DependencyProperty PreserveHighlightProperty = DependencyProperty.Register("PreserveHighlight",
            typeof(bool), typeof(EditableImage));

        public Data2D DataSource
        {
            get { return (Data2D)GetValue(DataSourceProperty); }
            set 
            { 
                SetValue(DataSourceProperty, value);
                if (!PreserveHighlight) _history.Clear();
            }
        }
        public Color SelectionColor
        {
            get { return (Color)GetValue(SelectionColorProperty); }
            set { SetValue(SelectionColorProperty, value); }
        }
        public bool PreserveHighlight
        {
            get { return (bool)GetValue(PreserveHighlightProperty); }
            set { SetValue(PreserveHighlightProperty, value); }
        }

        public List<Point> HighlightedPoints
        {
            get
            {
                if (_history == null) return null;

                List<Point> points = new List<Point>();
                foreach (KeyValuePair<Point, float> kvp in _history)
                {
                    points.Add(kvp.Key);
                }

                return points;
            }
        }

        public EditableImage()
            : base()
        {
            _history = new Dictionary<Point, float>();
        }

        private BitmapSource UpdateImage()
        {
            BitmapSource bs = ImageHelper.CreateColorScaleImage(DataSource, ColorScaleTypes.ThermalWarm);

            _visibleData = ImageHelper.ConvertToData3D(bs);

            return bs;
        }

        public void ChangeDataSource(Data2D DataSource)
        {
            this.DataSource = DataSource;
            Source = UpdateImage();
        }
        public void Undo()
        {
            int max = (int)DataSource.Maximum;
            foreach (KeyValuePair<Point, float> kvp in _history)
            {
                Color c = ColorScales.ThermalWarm(kvp.Value, max);
                _visibleData[kvp.Key] = c;
            }
            _history.Clear();

            Source = ImageHelper.CreateImage(_visibleData);
        }
        public void CommitChanges(CorrectionOperation Operation, int Iterations = 1)
        {
            for (int i = 0; i < Iterations; i++)
            {
                foreach (KeyValuePair<Point, float> kvp in _history)
                {
                    int x = (int)kvp.Key.X;
                    int y = (int)kvp.Key.Y;

                    DataSource[x, y] = Operation(x, y, DataSource);
                }
            }

            DataSource.Refresh();

            _history.Clear();

            BitmapSource bs = ImageHelper.CreateColorScaleImage(DataSource, ColorScaleTypes.ThermalWarm);
            _visibleData = ImageHelper.ConvertToData3D(bs);

            Source = bs;
        }

        private float AveragingOperation(int x, int y, Data2D data)
        {
            int startX = Math.Max(x - 2, 0);
            int startY = Math.Max(y - 2, 0);
            int endX = Math.Min(x + 2, data.Width - 1);
            int endY = Math.Min(y + 2, data.Height - 1);

            List<float> values = new List<float>();
            for (int iX = startX; iX < endX; iX++)
            {
                for (int iY = startY; iY < endY; iY++)
                {
                    values.Add(data[iX, iY]);
                }
            }

            return MathEx.Average(values.ToArray<float>());
        }

        public void Highlight(int x, int y)
        {
            float original = DataSource[x, y];

            Point toHighlight = new Point(x, y);
            if (!_history.ContainsKey(toHighlight))
            {
                _history.Add(toHighlight, original);

                doHighlight(x, y);

                Source = ImageHelper.CreateImage(_visibleData);
            }
        }
        public void Highlight(int x, int y, int BrushSize)
        {
            for (int a = 0; a < BrushSize; a++)
            {
                for (int b = 0; b < BrushSize; b++)
                {
                    int xIndex = (x - (BrushSize / 2)) + a;
                    int yIndex = (y - (BrushSize / 2)) + b;

                    if (xIndex < 0 || xIndex >= DataSource.Width ||
                        yIndex < 0 || yIndex >= DataSource.Height) continue;

                    float original = DataSource[xIndex, yIndex];

                    Point toHighlight = new Point(xIndex, yIndex);
                    if (!_history.ContainsKey(toHighlight))
                    {
                        _history.Add(toHighlight, original);

                        doHighlight(xIndex, yIndex);
                    }
                }
            }
            Source = ImageHelper.CreateImage(_visibleData);   
        }
        private void doHighlight(int x, int y)
        {
            _visibleData[x, y] = new float[4] { SelectionColor.B, SelectionColor.G, SelectionColor.R, 255 };
        }

        public void UnHighlight(int x, int y, int BrushSize)
        {
            for (int a = 0; a < BrushSize; a++)
            {
                for (int b = 0; b < BrushSize; b++)
                {
                    int xIndex = (x - (BrushSize / 2)) + a;
                    int yIndex = (y - (BrushSize / 2)) + b;

                    if (xIndex < 0 || xIndex >= DataSource.Width ||
                        yIndex < 0 || yIndex >= DataSource.Height) continue;

                    float original = DataSource[xIndex, yIndex];

                    Point toUnHighlight = new Point(xIndex, yIndex);
                    if (!_history.ContainsKey(toUnHighlight))
                    {
                        //_history.Remove(toUnHighlight, original);

                        unHighlight(toUnHighlight, original);
                    }
                }
            }
            Source = ImageHelper.CreateImage(_visibleData);   
        }
        private void unHighlight(Point location, float value)
        {
            int max = (int)DataSource.Maximum;
            Color c = ColorScales.ThermalWarm(value, max);
            _visibleData[location] = c;
        }

        public void ClearPoints()
        {
            foreach(Point p in _history.Keys)
            {
                unHighlight(p, _history[p]);
            }

            _history.Clear();

            Source = ImageHelper.CreateImage(_visibleData);
        }

        public void SetMaskPoints(bool[,] maskData)
        {
            foreach (Point p in _history.Keys)
            {
                float value = _history[p];
                int max = (int)DataSource.Maximum;
                Color c = ColorScales.ThermalWarm(value, max);
                _visibleData[p] = c;
            }

            _history.Clear();

            int width = maskData.GetLength(0);
            int height = maskData.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if(maskData[x,y])
                    {
                        float original = DataSource[x, y];
                        Point toHighlight = new Point(x, y);

                        _history.Add(toHighlight, original);
                        doHighlight(x, y);
                    }
                }
            }

            Source = ImageHelper.CreateImage(_visibleData);
        }
    }
    public delegate float CorrectionOperation(int x, int y, Data2D data);

    public enum CorrectionOperationMethod
    {
        Zero,

        Max,

        [Description("Average 3x3")]
        Average3x3,

        [Description("Average 5x5")]
        Average5x5,

        [Description("Average 7x7")]
        Average7x7,

        [Description("Average 3x3 Top/Bottom")]
        Average3x3TopBot,

        [Description("Average 5x5 Top/Bottom")]
        Average5x5TopBot,

        [Description("Average 7x7 Top/Bottom")]
        Average7x7TopBot,

        [Description("Average 3x3 Left/Right")]
        Average3x3LeftRight,

        [Description("Average 5x5 Left/Right")]
        Average5x5LeftRight,

        [Description("Average 7x7 Left/Right")]
        Average7x7LeftRight,
    }
    public static class CorrectionOperations
    {
        public static CorrectionOperation GetOperation(CorrectionOperationMethod Method)
        {
            switch (Method)
            {
                case CorrectionOperationMethod.Zero:
                    return ZeroOperation;
                case CorrectionOperationMethod.Max:
                    return MaxOperation;
                case CorrectionOperationMethod.Average3x3:
                    return Average3x3Operation;
                case CorrectionOperationMethod.Average5x5:
                    return Average5x5Operation;
                case CorrectionOperationMethod.Average7x7:
                    return Average7x7Operation;
                case CorrectionOperationMethod.Average3x3TopBot:
                    return Average3x3TBOperation;
                case CorrectionOperationMethod.Average5x5TopBot:
                    return Average5x5TBOperation;
                case CorrectionOperationMethod.Average7x7TopBot:
                    return Average7x7TBOperation;
                case CorrectionOperationMethod.Average3x3LeftRight:
                    return Average3x3LROperation;
                case CorrectionOperationMethod.Average5x5LeftRight:
                    return Average5x5LROperation;
                case CorrectionOperationMethod.Average7x7LeftRight:
                    return Average7x7LROperation;
                default:
                    return ZeroOperation;
            }
        }

        public static float ZeroOperation(int x, int y, Data2D data = null)
        {
            return 0;
        }
        public static float MaxOperation(int x, int y, Data2D data)
        {
            return data.Maximum;
        }
        public static float Average3x3Operation(int x, int y, Data2D data)
        {
            int startX = Math.Max(x - 1, 0);
            int startY = Math.Max(y - 1, 0);
            int endX = Math.Min(x + 1, data.Width - 1);
            int endY = Math.Min(y + 1, data.Height - 1);

            List<float> values = new List<float>();
            for (int iX = startX; iX < endX; iX++)
            {
                for (int iY = startY; iY < endY; iY++)
                {
                    values.Add(data[iX, iY]);
                }
            }

            return MathEx.Average(values.ToArray<float>());
        }
        public static float Average5x5Operation(int x, int y, Data2D data)
        {
            int startX = Math.Max(x - 2, 0);
            int startY = Math.Max(y - 2, 0);
            int endX = Math.Min(x + 2, data.Width - 1);
            int endY = Math.Min(y + 2, data.Height - 1);

            List<float> values = new List<float>();
            for (int iX = startX; iX < endX; iX++)
            {
                for (int iY = startY; iY < endY; iY++)
                {
                    values.Add(data[iX, iY]);
                }
            }

            return MathEx.Average(values.ToArray<float>());
        }
        public static float Average7x7Operation(int x, int y, Data2D data)
        {
            int startX = Math.Max(x - 3, 0);
            int startY = Math.Max(y - 3, 0);
            int endX = Math.Min(x + 3, data.Width - 1);
            int endY = Math.Min(y + 3, data.Height - 1);

            List<float> values = new List<float>();
            for (int iX = startX; iX < endX; iX++)
            {
                for (int iY = startY; iY < endY; iY++)
                {
                    values.Add(data[iX, iY]);
                }
            }

            return MathEx.Average(values.ToArray<float>());
        }
        public static float Average3x3TBOperation(int x, int y, Data2D data)
        {
            int startX = Math.Max(x - 1, 0);
            int startY = Math.Max(y - 1, 0);
            int endX = Math.Min(x + 1, data.Width - 1);
            int endY = Math.Min(y + 1, data.Height - 1);

            List<float> values = new List<float>();
            for (int iX = startX; iX < endX; iX++)
            {
                for (int iY = startY; iY < endY; iY++)
                {
                    if (iY == y) continue;
                    values.Add(data[iX, iY]);
                }
            }

            return MathEx.Average(values.ToArray<float>());
        }
        public static float Average5x5TBOperation(int x, int y, Data2D data)
        {
            int startX = Math.Max(x - 2, 0);
            int startY = Math.Max(y - 2, 0);
            int endX = Math.Min(x + 2, data.Width - 1);
            int endY = Math.Min(y + 2, data.Height - 1);

            List<float> values = new List<float>();
            for (int iX = startX; iX < endX; iX++)
            {
                for (int iY = startY; iY < endY; iY++)
                {
                    if (iY == y) continue;
                    values.Add(data[iX, iY]);
                }
            }

            return MathEx.Average(values.ToArray<float>());
        }
        public static float Average7x7TBOperation(int x, int y, Data2D data)
        {
            int startX = Math.Max(x - 3, 0);
            int startY = Math.Max(y - 3, 0);
            int endX = Math.Min(x + 3, data.Width - 1);
            int endY = Math.Min(y + 3, data.Height - 1);

            List<float> values = new List<float>();
            for (int iX = startX; iX < endX; iX++)
            {
                for (int iY = startY; iY < endY; iY++)
                {
                    if (iY == y) continue;
                    values.Add(data[iX, iY]);
                }
            }

            return MathEx.Average(values.ToArray<float>());
        }
        public static float Average3x3LROperation(int x, int y, Data2D data)
        {
            int startX = Math.Max(x - 1, 0);
            int startY = Math.Max(y - 1, 0);
            int endX = Math.Min(x + 1, data.Width - 1);
            int endY = Math.Min(y + 1, data.Height - 1);

            List<float> values = new List<float>();
            for (int iX = startX; iX < endX; iX++)
            {
                for (int iY = startY; iY < endY; iY++)
                {
                    if (iX == x) continue;
                    values.Add(data[iX, iY]);
                }
            }

            return MathEx.Average(values.ToArray<float>());
        }
        public static float Average5x5LROperation(int x, int y, Data2D data)
        {
            int startX = Math.Max(x - 2, 0);
            int startY = Math.Max(y - 2, 0);
            int endX = Math.Min(x + 2, data.Width - 1);
            int endY = Math.Min(y + 2, data.Height - 1);

            List<float> values = new List<float>();
            for (int iX = startX; iX < endX; iX++)
            {
                for (int iY = startY; iY < endY; iY++)
                {
                    if (iX == x) continue;
                    values.Add(data[iX, iY]);
                }
            }

            return MathEx.Average(values.ToArray<float>());
        }
        public static float Average7x7LROperation(int x, int y, Data2D data)
        {
            int startX = Math.Max(x - 3, 0);
            int startY = Math.Max(y - 3, 0);
            int endX = Math.Min(x + 3, data.Width - 1);
            int endY = Math.Min(y + 3, data.Height - 1);

            List<float> values = new List<float>();
            for (int iX = startX; iX < endX; iX++)
            {
                for (int iY = startY; iY < endY; iY++)
                {
                    if (iX == x) continue;
                    values.Add(data[iX, iY]);
                }
            }

            return MathEx.Average(values.ToArray<float>());
        }
    }

    public enum Data2DConverionType
    {
        Grayscale, Thermal, Color, None
    }

    public class HistogramMatching
    {
        int[] _histogramGray;
        int[] _histogramColor;

        float[] _distGray;
        float[] _distColor;

        Data2D _grayImage;
        Data3D _colorImage;

        float[] _band1;
        float[] _band2;

        public HistogramMatching(Data2D grayImage, Data3D colorImage)
        {
            _grayImage = grayImage;
            _colorImage = colorImage;

            _histogramGray = getHistogram(grayImage);
            _histogramColor = getHistogram(colorImage);

            _distGray = cumulativeDistribution(_histogramGray);
            _distColor = cumulativeDistribution(_histogramColor);
        }
        public HistogramMatching(float[] band1, float[] band2)
        {
            _band1 = band1;
            _band2 = band2;

            _histogramGray = getHistogram(band1);
            _histogramColor = getHistogram(band2);

            _distGray = cumulativeDistribution(_histogramGray);
            _distColor = cumulativeDistribution(_histogramColor);
        }


        public float[] Match1D()
        {
            int length = _band1.Length;

            float[] matched = new float[length];

            int[] lookup = new int[256];
            for (int i = 0; i < lookup.Length; i++)
            {
                float grayValue = _distGray[i];
                int index = 0;
                for (; index < _distColor.Length; index++)
                {
                    //Handle case where first value is already larger than gray
                    if (_distColor[index] > grayValue) break;

                    //Handle case where loop has reached the end of the array and
                    //can't compare to next value
                    if (index + 1 >= 256) break;

                    //Check that gray value is at correct index
                    if (_distColor[index] <= grayValue && _distColor[index + 1] > grayValue) break;
                }
                lookup[i] = index;
            }

            for (int i = 0; i < length; i++)
            {
                int original = (int)_band1[i];
                if (original < 0) original = 0;
                if (original >= 256) original = 255;

                matched[i] = lookup[original];
            }

            return matched;
        }
        public Data2D Match2D()
        {
            Data2D matched = new Data2D(_grayImage.Width, _grayImage.Height);

            int[] lookup = new int[256];
            for (int i = 0; i < lookup.Length; i++)
            {
                float grayValue = _distGray[i];
                int index = 0;
                for (; index < _distColor.Length; index++)
                {
                    //Handle case where first value is already larger than gray
                    if (_distColor[index] > grayValue) break;

                    //Handle case where loop has reached the end of the array and
                    //can't compare to next value
                    if (index + 1 >= 256) break;

                    //Check that gray value is at correct index
                    if (_distColor[index] <= grayValue && _distColor[index + 1] > grayValue) break;
                }
                lookup[i] = index;
            }

            for (int x = 0; x < matched.Width; x++)
            {
                for (int y = 0; y < matched.Height; y++)
                {
                    int original = (int)_grayImage[x, y];
                    if (original < 0) original = 0;
                    if (original >= 256) original = 255;

                    matched[x, y] = lookup[original];
                }
            }

            return matched;
        }

        private int[] getHistogram(Data2D grayImage)
        {
            int[] histogram = new int[256];

            for (int x = 0; x < grayImage.Width; x++)
            {
                for (int y = 0; y < grayImage.Height; y++)
                {
                    int value = (int)grayImage[x, y];
                    if (value < 0) value = 0;
                    if (value >= 256) value = 255;

                    histogram[value]++;
                }
            }

            return histogram;
        }
        private int[] getHistogram(Data3D colorImage)
        {
            int[] histogram = new int[256];

            for (int x = 0; x < colorImage.Width; x++)
            {
                for (int y = 0; y < colorImage.Height; y++)
                {
                    float[] color = colorImage[x, y];

                    double[] hsl = ColorConversion.RGBtoHSL(color[0], color[1], color[2]);
                    //Lightness is returned as double [0,1] so
                    //multiply by 255 to scale correctly
                    int value = (int)(hsl[2] * 255f);
                    if (value < 0) value = 0;
                    if (value >= 256) value = 255;

                    histogram[value]++;
                }
            }

            return histogram;
        }
        private int[] getHistogram(float[] band)
        {
            int[] histogram = new int[256];
            int length = band.GetLength(0);

            for (int i = 0; i < length; i++)
            {
                int value = (int)band[i];
                if (value < 0) value = 0;
                if (value >= 256) value = 255;

                histogram[value]++;
            }

            return histogram;
        }

        private float[] cumulativeDistribution(int[] histogram)
        {
            int[] cdf = new int[histogram.Length];
            float[] cdfNorm = new float[histogram.Length];

            int sum = 0;
            for (int i = 0; i < histogram.Length; i++)
            {
                sum += histogram[i];
                cdf[i] = sum;
            }

            for (int i = 0; i < histogram.Length; i++)
            {
                cdfNorm[i] = cdf[i] / (float)sum;
            }

            return cdfNorm;
        }
    }

    public static class Histogram
    {
        public static int[] Get(Data2D grayImage)
        {
            int[] histogram = new int[256];

            for (int x = 0; x < grayImage.Width; x++)
            {
                for (int y = 0; y < grayImage.Height; y++)
                {
                    int value = (int)grayImage[x, y];
                    if (value < 0) value = 0;
                    if (value >= 256) value = 255;

                    histogram[value]++;
                }
            }

            return histogram;
        }
        public static int[] Get(Data3D colorImage)
        {
            int[] histogram = new int[256];

            for (int x = 0; x < colorImage.Width; x++)
            {
                for (int y = 0; y < colorImage.Height; y++)
                {
                    float[] color = colorImage[x, y];

                    double[] hsl = ColorConversion.RGBtoHSL(color[0], color[1], color[2]);
                    //Lightness is returned as double [0,1] so
                    //multiply by 255 to scale correctly
                    int value = (int)(hsl[2] * 255f);
                    if (value < 0) value = 0;
                    if (value >= 256) value = 255;

                    histogram[value]++;
                }
            }

            return histogram;
        }
    }
}
