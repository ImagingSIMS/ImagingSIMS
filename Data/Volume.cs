using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ImagingSIMS.Data.Rendering
{
    public class Volume : Data, ISavable
    {
        Data3D _data;
        Color _dataColor;

        int _threshold;
        float _pixelDepth;
        float _pixelSize;
        float _zSpacing;
        int _isoValue;

        public float this[int x, int y, int z]
        {
            get
            {
                if (_data == null) return 0;
                if (x >= _data.Width || y >= _data.Height || z >= _data.Depth)
                    throw new ArgumentException("Invalid index.");
                return _data[x, y, z];
            }
        }

        public string VolumeName
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged("VolumeName");
                }
            }
        }

        public int Width
        {
            get { return _data.Width; }
        }
        public int Height
        {
            get { return _data.Height; }
        }
        public int Depth
        {
            get { return _data.Depth; }
        }

        public int Threshold
        {
            get { return _threshold; }
            set
            {
                if (_threshold != value)
                {
                    _threshold = value;
                    NotifyPropertyChanged("Threshold");
                }
            }
        }
        public float PixelDepth
        {
            get { return _pixelDepth; }
            set
            {
                if (_pixelDepth != value)
                {
                    _pixelDepth = value;
                    NotifyPropertyChanged("PixelDepth");
                }
            }
        }
        public float PixelSize
        {
            get { return _pixelSize; }
            set
            {
                if (_pixelSize != value)
                {
                    _pixelSize = value;
                    NotifyPropertyChanged("PixelSize");
                }
            }
        }
        public float ZSpacing
        {
            get { return _zSpacing; }
            set
            {
                if (_zSpacing != value)
                {
                    _zSpacing = value;
                    NotifyPropertyChanged("ZSpacing");
                }
            }
        }
        public int IsoValue
        {
            get { return _isoValue; }
            set
            {
                if(_isoValue != value)
                {
                    _isoValue = value;
                    NotifyPropertyChanged("IsoValue");
                }
            }
        }

        public Data3D Data
        {
            get { return _data; }
            private set
            {
                if (_data != value)
                {
                    _data = value;
                    NotifyPropertyChanged("Width");
                    NotifyPropertyChanged("Height");
                    NotifyPropertyChanged("Depth");
                    NotifyPropertyChanged("Dimensions");
                }
            }
        }
        public Color DataColor
        {
            get { return _dataColor; }
            set
            {
                if (_dataColor != value)
                {
                    _dataColor = value;
                    NotifyPropertyChanged("DataColor");
                }
            }
        }
        public string Dimensions
        {
            get { return string.Format("{0}x{1}x{2}", Width, Height, Depth); }
        }

        public Volume()
        {
        }
        public Volume(Data3D Data, Color DataColor)
        {
            this.Data = Data;
            this.DataColor = DataColor;
            VolumeName = Data.DataName;
        }
        public Volume(Data3D Data, Color DataColor, string VolumeName)
        {
            this.Data = Data;
            this.DataColor = DataColor;
            this.VolumeName = VolumeName;
        }
        public Volume(Data2D[] Data, Color DataColor)
        {
            this.Data = new Data3D(Data);
            this.DataColor = DataColor;
            VolumeName = "";
        }
        public Volume(Data2D[] Data, Color DataColor, string VolumeName)
        {
            this.Data = new Data3D(Data);
            this.DataColor = DataColor;
            this.VolumeName = VolumeName;
        }
        public Volume(string FilePath)
        {
            using (Stream s = File.OpenRead(FilePath))
            {
                Data = new Data3D();

                BinaryReader br = new BinaryReader(s);

                VolumeName = br.ReadString();

                int width = br.ReadInt32();
                int height = br.ReadInt32();
                int depth = br.ReadInt32();

                DataColor = Color.FromArgb(br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte());

                for (int z = 0; z < depth; z++)
                {
                    Data2D d = new Data2D(width, height);
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            if (br.ReadBoolean())
                            {
                                d[x, y] = br.ReadSingle();
                            }
                            else d[x, y] = 0;
                        }
                    }
                    Data.AddLayer(d);
                }
            }
        }

        public void Save(string FilePath)
        {
            using (Stream s = File.OpenWrite(FilePath))
            {
                BinaryWriter bw = new BinaryWriter(s);

                bw.Write(VolumeName);

                bw.Write(Width);
                bw.Write(Height);
                bw.Write(Depth);

                bw.Write(DataColor.A);
                bw.Write(DataColor.R);
                bw.Write(DataColor.G);
                bw.Write(DataColor.B);

                for (int z = 0; z < Depth; z++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        for (int y = 0; y < Height; y++)
                        {

                            if (Data[x, y, z] == 0) bw.Write(false);
                            else
                            {
                                bw.Write(true);
                                bw.Write(Data[x, y, z]);
                            }
                        }
                    }
                }
            }
        }
        public void SaveRawVolume(string FilePath)
        {
            using (Stream s = File.OpenWrite(FilePath))
            {
                BinaryWriter bw = new BinaryWriter(s);
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        for (int z = 0; z < Depth; z++)
                        {
                            bw.Write(Data[x, y, z]);
                        }
                    }
                }
            }
        }
        public static float[, ,] ToArray(Volume Volume)
        {
            return Volume._data.ToFloatArray();
        }

        #region ISavable

        // Layout
        // (string) VolumeName
        // (int)    Width
        // (int)    Height
        // (int)    Depth
        // (byte)   ColorA
        // (byte)   ColorR
        // (byte)   ColorG
        // (byte)   ColorB
        // (Data3D) Data
        public byte[] ToByteArray()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(ms);

                // Offset start of writing to later go back and 
                // prefix the stream with the size of the stream written
                bw.Seek(sizeof(int), SeekOrigin.Begin);

                // Write contents of object
                bw.Write(VolumeName);
                bw.Write(Width);
                bw.Write(Height);
                bw.Write(Depth);
                bw.Write(DataColor.A);
                bw.Write(DataColor.R);
                bw.Write(DataColor.G);
                bw.Write(DataColor.B);
                bw.Write(IsoValue);

                for (int z = 0; z < Depth; z++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            if (this[x, y, z] == 0) bw.Write(false);
                            else
                            {
                                bw.Write(true);
                                bw.Write(this[x, y, z]);
                            }

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

                string volumeName = br.ReadString();
                int width = br.ReadInt32();
                int height = br.ReadInt32();
                int depth = br.ReadInt32();
                byte colorA = br.ReadByte();
                byte colorR = br.ReadByte();
                byte colorG = br.ReadByte();
                byte colorB = br.ReadByte();
                int isoValue = br.ReadInt32();

                this.VolumeName = volumeName;
                this.DataColor = Color.FromArgb(colorA, colorR, colorG, colorB);
                this.IsoValue = isoValue;

                this._data = new Data3D(width, height, depth);
                for (int z = 0; z < depth; z++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            if (br.ReadBoolean())
                            {
                                this._data[x, y, z] = br.ReadSingle();
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
