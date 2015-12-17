using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImagingSIMS.Data
{
    public class SEM : Data, ISavable
    {
        const float Zero = 0.0f;
        const float OneThird = 0.333333f;
        const float TwoThirds = 0.666666f;
        const float One = 1.0f;

        const int _headerLength = 4656;

        int _matrixSize;
        int _maxValue;
        int[,] _semArray;
        int _intensityScale;

        Image _semImage;

        public int MatrixSize { get { return _matrixSize; } }
        public int MaxRawValue { get { return _maxValue; } }
        public int IntensityScale
        {
            get { return _intensityScale; }
            set
            {
                if (_intensityScale != value)
                {
                    _intensityScale = value;
                    NotifyPropertyChanged("IntensityScale");
                    RedrawImage(value);
                }
            }
        }

        public Image SEMImage
        {
            get { return _semImage; }
            set
            {
                if (_semImage != value)
                {
                    _semImage = value;
                    NotifyPropertyChanged("SEMImage");
                }
            }
        }

        public string SEMName
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged("SEMName");
                }
            }
        }

        public SEM()
        {
            SEMImage = new Image();
        }
        public SEM(string FilePath)
        {
            SEMImage = new Image();
            LoadFromFile(FilePath);
        }

        public void LoadFromFile(string FileName)
        {
            using (Stream stream = File.OpenRead(FileName))
            {
                BinaryReader br = new BinaryReader(stream);
                _matrixSize = (int)Math.Sqrt((stream.Length - _headerLength) / sizeof(int));

                _semArray = new int[_matrixSize, _matrixSize];
                _maxValue = 0;

                br.BaseStream.Position = _headerLength - 1;
                for (int x = 0; x < _matrixSize; x++)
                {
                    for (int y = 0; y < _matrixSize; y++)
                    {
                        _semArray[x, y] = br.ReadInt32();
                        if (_semArray[x, y] > _maxValue) _maxValue = _semArray[x, y];
                    }
                }
            }
            SEMName = Path.GetFileNameWithoutExtension(FileName);

            RedrawImage();
        }

        public void RedrawImage()
        {
            SEMImage.Source = GetImage();
            IntensityScale = MaxRawValue;
        }
        public void RedrawImage(int IntensityScale)
        {
            SEMImage.Source = GetImage(IntensityScale);
        }

        private BitmapSource GetImage()
        {
            PixelFormat pf = PixelFormats.Bgr24;

            int rawStride = (_matrixSize * pf.BitsPerPixel) / 8;
            byte[] rawImage = new byte[rawStride * _matrixSize];

            int pos = 0;
            for (int x = 0; x < _matrixSize; x++)
            {
                for (int y = 0; y < _matrixSize; y++)
                {
                    int val = _semArray[x, y];
                    Color pixel = ThermalColor(val, MaxRawValue);

                    rawImage[pos + 0] = pixel.B;
                    rawImage[pos + 1] = pixel.G;
                    rawImage[pos + 2] = pixel.R;

                    pos += 3;
                }
            }
            return BitmapSource.Create(MatrixSize, MatrixSize, 96, 96, pf, null, rawImage, rawStride);
        }
        private BitmapSource GetImage(int IntensityValue)
        {
            PixelFormat pf = PixelFormats.Bgr24;

            int rawStride = (_matrixSize * pf.BitsPerPixel) / 8;
            byte[] rawImage = new byte[rawStride * _matrixSize];

            int pos = 0;
            for (int x = 0; x < _matrixSize; x++)
            {
                for (int y = 0; y < _matrixSize; y++)
                {
                    int val = _semArray[x, y];
                    Color pixel = ThermalColor(val, IntensityValue);

                    rawImage[pos + 0] = pixel.B;
                    rawImage[pos + 1] = pixel.G;
                    rawImage[pos + 2] = pixel.R;

                    pos += 3;
                }
            }
            return BitmapSource.Create(MatrixSize, MatrixSize, 96, 96, pf, null, rawImage, rawStride);
        }

        private static Color ThermalColor(int Value, int Maximum)
        {
            double ratio = (double)Value / (double)Maximum;

            if (ratio > Zero && ratio <= OneThird)
            {
                return Color.FromArgb(255, (byte)(ratio * 768), 0, 0);
            }
            else if (ratio > OneThird && ratio <= TwoThirds)
            {
                return Color.FromArgb(255, 255, (byte)((768 * ratio) - 256), 0);
            }
            else if (ratio > TwoThirds && ratio < One)
            {
                return Color.FromArgb(255, 255, 255, (byte)((768 * ratio) - 512));
            }
            else if (ratio >= One)
            {
                return Color.FromArgb(255, 255, 255, 255);
            }
            else
            {
                return Color.FromArgb(255, 0, 0, 0);
            }
        }

        #region ISavable

        // Layout:
        // (sting)  SEMName
        // (int)    MatrixSize
        // (int)    MaxValue
        // (int)    IntensityScale
        public byte[] ToByteArray()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(ms);

                // Offset start of writing to later go back and 
                // prefix the stream with the size of the stream written
                bw.Seek(sizeof(int), SeekOrigin.Begin);

                // Write contents of object
                bw.Write(SEMName);
                bw.Write(MatrixSize);
                bw.Write(MaxRawValue);
                bw.Write(IntensityScale);

                for (int x = 0; x < _matrixSize; x++)
                {
                    for (int y = 0; y < _matrixSize; y++)
                    {
                        if (_semArray[x, y] == 0) bw.Write(false);
                        else
                        {
                            bw.Write(true);
                            bw.Write(_semArray[x, y]);
                        }
                    }
                }

                // Return to beginning of writer and write the length
                bw.Seek(0, SeekOrigin.Begin);
                bw.Write((int)bw.BaseStream.Length - sizeof(int));

                // Return to start of memory stream and 
                // return byte array of the stream
                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }

        public void FromByteArray(byte[] array)
        {
            using (MemoryStream ms = new MemoryStream(array))
            {
                BinaryReader br = new BinaryReader(ms);

                string semName = br.ReadString();
                int matrixSize = br.ReadInt32();
                int maxValue = br.ReadInt32();
                int intensityScale = br.ReadInt32();

                SEMName = semName;
                _matrixSize = matrixSize;
                _maxValue = maxValue;
                IntensityScale = intensityScale;

                _semArray = new int[_matrixSize, _matrixSize];
                for (int x = 0; x < _matrixSize; x++)
                {
                    for (int y = 0; y < _matrixSize; y++)
                    {
                        if (br.ReadBoolean())
                        {
                            _semArray[x, y] = br.ReadInt32();
                        }
                    }
                }
            }

            RedrawImage();
        }
        #endregion
    }
}
