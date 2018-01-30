using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImagingSIMS.Common.Math;

namespace ImagingSIMS.Data.Imaging
{
    public abstract class BaseImageGenerator
    {
        public Data2D ConvertToData2D(BitmapSource bitmapSource)
        {
            PixelFormat pf = bitmapSource.Format;

            int sizeX = bitmapSource.PixelWidth;
            int sizeY = bitmapSource.PixelHeight;

            int stride = (sizeX * pf.BitsPerPixel) / 8;
            int size = sizeY * stride;
            byte[] pixels = new byte[size];
            bitmapSource.CopyPixels(pixels, stride, 0);

            Data2D array = new Data2D(sizeX, sizeY);

            int pos = 0;
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    if (pf == PixelFormats.Indexed8)
                    {
                        Color c = bitmapSource.Palette.Colors[pixels[pos]];

                        array[x, y] = MathEx.Average(c.R, c.G, c.B);

                        pos += 1;
                    }
                    else if (pf == PixelFormats.Gray8)
                    {
                        array[x, y] = pixels[pos];

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
        public Task<Data2D> ConvertToData2DAsync(BitmapSource bitmapSource)
        {
            return Task.Run(() => ConvertToData2D(bitmapSource));
        }
        public Data2D ConvertToData2D(BitmapSource bitmapSource, Data2DConverionType conversion, Color? color = null)
        {
            PixelFormat pf = bitmapSource.Format;

            int sizeX = bitmapSource.PixelWidth;
            int sizeY = bitmapSource.PixelHeight;

            int stride = (sizeX * pf.BitsPerPixel) / 8;
            int size = sizeY * stride;
            byte[] pixels = new byte[size];
            bitmapSource.CopyPixels(pixels, stride, 0);

            Data2D array = new Data2D(sizeX, sizeY);

            Color newColor = new Color();
            if (color.HasValue)
            {
                newColor = color.Value;
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
                        Color c = bitmapSource.Palette.Colors[pixels[pos]];
                        b = c.B;
                        g = c.G;
                        r = c.R;

                        pos += 1;
                    }
                    else if (pf == PixelFormats.Gray8)
                    {
                        b = pixels[pos];
                        g = pixels[pos];
                        r = pixels[pos];

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

                    switch (conversion)
                    {
                        case Data2DConverionType.Color:
                            array[x, y] = GetFromColor(new byte[3] { b, g, r }, newColor);
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
        public Task<Data2D> ConvertToData2DAsync(BitmapSource bitmapSource, Data2DConverionType conversion, Color? color = null)
        {
            return Task.Run(() => ConvertToData2D(bitmapSource, conversion, color));
        }
        public Data2D ConvertToData2D(Data3D source)
        {
            int sizeX = source.Width;
            int sizeY = source.Height;

            Data2D array = new Data2D(sizeX, sizeY);

            if (source.Depth == 3 || source.Depth == 4)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                        array[x, y] = GetFromGray(new byte[3]
                        { (byte)source[x, y, 0], (byte)source[x, y, 1], (byte)source[sizeY, y, 2] });
                    }
                }

                return array;
            }

            else throw new ArgumentException("Invalid number of color channels.");
        }
        public Task<Data2D> ConvertToData2DAsync(Data3D source)
        {
            return Task.Run(() => ConvertToData2D(source));
        }

        public Data3D ConvertToData3D(BitmapSource bitmapSource)
        {
            PixelFormat pf = bitmapSource.Format;

            int sizeX = bitmapSource.PixelWidth;
            int sizeY = bitmapSource.PixelHeight;

            int stride = (sizeX * pf.BitsPerPixel) / 8;
            int size = sizeY * stride;
            byte[] pixels = new byte[size];
            bitmapSource.CopyPixels(pixels, stride, 0);

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
                        Color c = bitmapSource.Palette.Colors[pixels[pos]];

                        arrays[0][x, y] = c.B;
                        arrays[1][x, y] = c.G;
                        arrays[2][x, y] = c.R;

                        pos += 1;
                    }
                    else if (pf == PixelFormats.Gray8)
                    {
                        Color c = bitmapSource.Palette.Colors[pixels[pos]];

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

            return new Data3D(arrays);
        }
        public Task<Data3D> ConvertToData3DAsync(BitmapSource bitmapSource)
        {
            return Task.Run(() => ConvertToData3D(bitmapSource));
        }

        public BitmapSource Create(Data3D channelData)
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
        public Task<BitmapSource> CreateAsync(Data3D channelData)
        {
            return Task.Run(() => Create(channelData));
        }
        public abstract BitmapSource Create(Data2D data, ColorScaleTypes scale);
        public Task<BitmapSource> CreateAsync(Data2D data, ColorScaleTypes scale)
        {
            return Task.Run(() => Create(data, scale));
        }
        public abstract BitmapSource Create(Data2D data, ColorScaleTypes scale, float saturation);
        public Task<BitmapSource> CreateAsync(Data2D data, ColorScaleTypes scale, float saturation)
        {
            return Task.Run(() => Create(data, scale, saturation));
        }
        public abstract BitmapSource Create(Data2D data, ColorScaleTypes scale, float saturation, float threshold);
        public Task<BitmapSource> CreateAsync(Data2D data, ColorScaleTypes scale, float saturation, float threshold)
        {
            return Task.Run(() => Create(data, scale, saturation, threshold));
        }
        public abstract BitmapSource Create(Data2D data, Color solidColor);
        public Task<BitmapSource> CreateAsync(Data2D data, Color solidColor)
        {
            return Task.Run(() => Create(data, solidColor));
        }
        public abstract BitmapSource Create(Data2D data, Color solidColor, float saturation);
        public Task<BitmapSource> CreateAsync(Data2D data, Color solidColor, float saturation)
        {
            return Task.Run(() => Create(data, solidColor, saturation));
        }
        public abstract BitmapSource Create(Data2D data, Color solidColor, float saturation, float threshold);
        public Task<BitmapSource> CreateAsync(Data2D data, Color solidColor, float saturation, float threhsold)
        {
            return Task.Run(() => Create(data, solidColor, saturation, threhsold));
        }

        public abstract BitmapSource[] Create(ImageComponent[] components, ImagingParameters parameters);
        public Task<BitmapSource[]> CreateAsync(ImageComponent[] components, ImagingParameters parameters)
        {
            return Task.Run(() => Create(components, parameters));
        }

        public BitmapSource SliceXZ(BitmapSource[] images, int yCoord)
        {
            int sizeX = (int)images[0].Width;
            int sizeY = (int)images[0].Height;
            int sizeZ = images.Length;

            Data3D[] slices = new Data3D[sizeZ];
            for (int i = 0; i < sizeZ; i++)
            {
                slices[i] = ConvertToData3D(images[i]);
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
                        result[i][x, z] = slices[z][x, yCoord, i];
                    }
                }
            }

            return Create(new Data3D(result));
        }
        public Task<BitmapSource> SliceXZAsync(BitmapSource[] images, int yCoord)
        {
            return Task.Run(() => SliceXZ(images, yCoord));
        }
        public BitmapSource SliceYZ(BitmapSource[] images, int xCoord)
        {
            int sizeX = (int)images[0].Width;
            int sizeY = (int)images[0].Height;
            int sizeZ = images.Length;

            Data3D[] slices = new Data3D[sizeZ];
            for (int i = 0; i < sizeZ; i++)
            {
                slices[i] = ConvertToData3D(images[i]);
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
                        result[i][y, z] = slices[z][xCoord, y, z];
                    }
                }
            }

            return Create(new Data3D(result));
        }
        public Task<BitmapSource> SliceYZAsync(BitmapSource[] images, int xCoord)
        {
            return Task.Run(() => SliceYZ(images, xCoord));
        }

        public BitmapSource FromFile(string fileName)
        {
            BitmapImage src = new BitmapImage();

            src.BeginInit();
            src.UriSource = new Uri(fileName, UriKind.Absolute);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            src.EndInit();

            return src;
        }
        public Task<BitmapSource> FromFileAsync(string fileName)
        {
            return Task.Run(() => FromFile(fileName));
        }

        /// <summary>
        /// Converts a thermal color value to a single byte value.
        /// </summary>
        /// <param name="b">3 byte array representing the thermal color.</param>
        /// <returns>Byte value of color.</returns>
        protected static byte GetFromThermal(byte[] b)
        {
            return (MathEx.Average(b[0], b[1], b[2]));
        }
        /// <summary>
        /// Converts a gray color value to a single byte value.
        /// </summary>
        /// <param name="b">3 byte array representing the gray color.</param>
        /// <returns>Byte value of color.</returns>
        protected static byte GetFromGray(byte[] b)
        {
            return (byte)((.299f * b[2]) + (.587f * b[1]) + (.114f * b[0]));
        }
        /// <summary>
        /// Converts a color value to a single byte value.
        /// </summary>
        /// <param name="b">3 byte array representing the color.</param>
        /// <param name="c">The color representing the maximum color (c = 255).</param>
        /// <returns>Byte value of color.</returns>
        protected static byte GetFromColor(byte[] b, Color c)
        {
            List<float> ratios = new List<float>();

            if (c.B > 0) ratios.Add(b[0] / (float)c.B);
            if (c.G > 0) ratios.Add(b[1] / (float)c.G);
            if (c.R > 0) ratios.Add(b[2] / (float)c.R);

            return (byte)(MathEx.Average(ratios.ToArray<float>()) * 255f);
        }
    }
}
