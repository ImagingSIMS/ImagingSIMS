using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

using ImagingSIMS.Data.Imaging;

namespace ImagingSIMS.Data.Rendering
{
    public class HeightMap
    {
        Data2D _heightData;
        Color[,] _colorData;

        private int colorDataWidth
        {
            get
            {
                if (_colorData == null) return 0;
                return _colorData.GetLength(0);
            }
        }
        private int colorDataHeight
        {
            get
            {
                if (_colorData == null) return 0;
                return _colorData.GetLength(1);
            }
        }
        private int heightDataWidth
        {
            get
            {
                if (_heightData == null) return 0;
                return _heightData.Width;
            }
        }
        private int heightDataHeight
        {
            get
            {
                if (_heightData == null) return 0;
                return _heightData.Height;
            }
        }

        public HeightMap(Data2D HeightData, Data3D ColorData)
        {
            _heightData = HeightData;
            _colorData = (Color[,])ColorData;

            Resize();            
        }
        public HeightMap(Data2D HeightData, Color[,] ColorData)
        {
            _heightData = HeightData;
            _colorData = ColorData;

            Resize();
        }

        private void NormalizeColorData()
        {
            for (int x = 0; x < colorDataWidth; x++)
            {
                for (int y = 0; y < colorDataHeight; y++)
                {
                    Color c = _colorData[x, y];
                    _colorData[x, y] = new Color((float)c.R / 255f, (float)c.G / 255f, (float)c.B / 255f, (float)c.A / 255f);
                }
            }
        }

        private void Resize()
        {
            if (colorDataWidth < heightDataWidth && colorDataHeight < heightDataHeight)
            {
                _colorData = _colorData.Upscale(heightDataWidth, heightDataHeight);
            }
            else if(colorDataWidth > heightDataWidth && colorDataHeight > heightDataHeight)
            {
                _heightData = _heightData.Upscale(colorDataWidth, colorDataHeight);
            }
            else if(colorDataWidth != heightDataWidth && colorDataHeight != heightDataHeight)
            {
                throw new ArgumentException("Invalid dimensions for resizing.");
            }
        }

        public float[,] CorrectedHeightData
        {
            get
            {
                return (float[,])_heightData;
            }
        }
        public Color[,] CorrectedColorData
        {
            get
            {
                //NormalizeColorData();
                return _colorData;
            }
        }
    }
}
