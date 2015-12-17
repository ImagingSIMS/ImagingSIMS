using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ImagingSIMS.Data.Imaging
{
    public sealed class ImageComponent : ISObject, ISavable
    {
        string _name;
        Color _pixelColor;
        Data3D _data3D;

        public string ComponentName
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged("ComponentName");
                }
            }
        }
        public int NumberTables
        {
            get 
            {
                if (_data3D == null) return 0;
                return _data3D.Depth; 
            }
        }
        public Color PixelColor
        {
            get { return _pixelColor; }
            set
            {
                if (_pixelColor != value)
                {
                    _pixelColor = value;
                    NotifyPropertyChanged("PixelColor");
                }
            }
        }
        public Data2D[] Data
        {
            get { return _data3D.Layers; }
        }
        public int Width
        {
            get { return _data3D.Width; }
        }
        public int Height
        {
            get { return _data3D.Height; }
        }
        public float Maximum
        {
            get
            {
                if (Data == null) return 0;
                float max = 0;
                foreach (Data2D d in Data)
                {
                    if (d.Maximum > max) max = d.Maximum;
                }
                return max;
            }
        }
        public float LayerMaximum(int Layer)
        {
            if (Data == null) return 0;
            return Data[Layer].Maximum;
        }

        public ImageComponent()
        {
            _pixelColor = Color.FromArgb(0, 0, 0, 0);
        }
        public ImageComponent(string Name, Color PixelColor)
        {
            this.ComponentName = Name;
            this.PixelColor = PixelColor;
        }
        public ImageComponent(string Name, Color PixelColor, Data3D Data)
        {
            this.ComponentName = Name;
            this.PixelColor = PixelColor;
            _data3D = Data;
            NotifyPropertyChanged("NumberTables");
        }

        public void AddData(Data2D Data)
        {
            if (_data3D == null) _data3D = new Data3D(new Data2D[1] { Data });
            else _data3D.AddLayer(Data);

            NotifyPropertyChanged("NumberTables");
        }
        public void AddData(Data2D[] Data)
        {
            _data3D.AddLayers(Data);
            NotifyPropertyChanged("NumberTables");
        }

        #region ISavable
        
        // Layout:
        // (string) ComponentName
        // (byte)   PixelColor.A
        // (byte)   PixelColor.R
        // (byte)   PixelColor.G
        // (byte)   PixelColor.B
        // (int)    Data.Length
        // (Data2D) Data layers

        public byte[] ToByteArray()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(ms);

                // Offset start of writing to later go back and 
                // prefix the stream with the size of the stream written
                bw.Seek(sizeof(int), SeekOrigin.Begin);

                // Write contents of object
                bw.Write(ComponentName);
                bw.Write(PixelColor.A);
                bw.Write(PixelColor.R);
                bw.Write(PixelColor.G);
                bw.Write(PixelColor.B);

                bw.Write(Data.Length);
                for (int i = 0; i < Data.Length; i++)
                {
                    bw.Write(Data[i].ToByteArray());
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

                string componentName = br.ReadString();
                byte colorA = br.ReadByte();
                byte colorR = br.ReadByte();
                byte colorG = br.ReadByte();
                byte colorB = br.ReadByte();

                this.ComponentName = ComponentName;
                this.PixelColor = Color.FromArgb(colorA, colorR, colorG, colorB);

                int dataLength = br.ReadInt32();
                for (int i = 0; i < dataLength; i++)
                {
                    long dataSize = br.ReadInt64();
                    byte[] buffer = br.ReadBytes((int)dataSize);
                    Data2D d = new Data2D();
                    d.FromByteArray(buffer);
                    AddData(d);
                }
            }
        }
        #endregion
    }
}
