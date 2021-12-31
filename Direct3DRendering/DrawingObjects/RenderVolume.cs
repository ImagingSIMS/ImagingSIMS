using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Device = SharpDX.Direct3D11.Device;

namespace ImagingSIMS.Direct3DRendering.DrawingObjects
{
    public class RenderVolume
    {
        float[, ,] _data;
        Color _color;
        string _name;

        public int Width
        {
            get { return _data.GetLength(0); }
        }
        public int Height
        {
            get { return _data.GetLength(1); }
        }
        public int Depth
        {
            get { return _data.GetLength(2); }
        }

        public float this[int x, int y, int z]
        {
            get { return _data[x, y, z]; }
            set { _data[x, y, z] = value; }
        }
        public float[,,]Data
        {
            get { return _data; }
        }
        public Color Color
        {
            get { return _color; }
        }
        public string Name
        {
            get { return _name; }
        }

        public RenderVolume(float[, ,] VolumeData, System.Windows.Media.Color VolumeColor, string name)
        {
            _data = VolumeData;
            _color = VolumeColor.ToSharpDXColor();
            _name = name;
        }
        public RenderVolume(float[, ,] VolumeData, Color VolumeColor, string name)
        {
            _data = VolumeData;
            _color = VolumeColor;
            _name = name;
        }

        public Texture3D CreateTexture(Device Device)
        {
            Texture3DDescription desc = new Texture3DDescription()
            {
                Width = Width,
                Height = Height,
                Depth = Depth,
                MipLevels = 1,
                Format = Format.R32_Float,
                Usage = ResourceUsage.Immutable,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None
            };

            float max = 0;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        if (Data[x, y, z] > max) max = Data[x, y, z];
                    }
                }
            }

            int index = 0;
            float[] buffer = new float[Width * Height * Depth];

            for (int z = 0; z < Depth; z++)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        buffer[index] = Data[x, y, z] / max;
                        index++;
                    }
                }
            }

            DataStream stream = DataStream.Create<float>(buffer, true, true);

            return new Texture3D(Device, desc, new[] { new DataBox(stream.DataPointer, Width * sizeof(float), Width * Height * sizeof(float)) });
        }
        public Texture3D CreateTexture(Device Device, out ShaderResourceView srvVolume)
        {
            Texture3DDescription desc = new Texture3DDescription()
            {
                Width = Width,
                Height = Height,
                Depth = Depth,
                MipLevels = 1,
                Format = Format.R32_Float,
                Usage = ResourceUsage.Immutable,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None
            };

            float max = 0;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        if (Data[x, y, z] > max) max = Data[x, y, z];
                    }
                }
            }

            int index = 0;
            float[] buffer = new float[Width * Height * Depth];

            for (int z = 0; z < Depth; z++)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        buffer[index] = Data[x, y, z] / max;
                        index++;
                    }
                }
            }

            DataStream stream = DataStream.Create<float>(buffer, true, true);

            Texture3D tex = new Texture3D(Device, desc, new[] { new DataBox(stream.DataPointer, Width * sizeof(float), Width * Height * sizeof(float)) });
            srvVolume = new ShaderResourceView(Device, tex);
            return tex;
        }

    }
}
