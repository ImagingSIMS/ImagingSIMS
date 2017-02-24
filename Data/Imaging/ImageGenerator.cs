using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace ImagingSIMS.Data.Imaging
{
    public static class ImageGenerator
    {
        // TODO:
        //else if (pf == PixelFormats.Gray16)
        //{
        //    byte pixelVal = (byte)(BitConverter.ToInt16(pixels, pos) / 2);

        //    b = pixelVal;
        //    g = pixelVal;
        //    r = pixelVal;
        //    pos += 2;
        //}

        //else if (pf == PixelFormats.Gray16)
        //{
        //    array[x, y] = BitConverter.ToUInt16(pixels, pos);

        //    pos += 2;
        //}

        static IImageGenerator _generator;

        static ImageGenerator()
        {
            if (SupportsComputShader())
                _generator = new GPUImageGenerator();
            else _generator = new CPUImageGenerator();
        }

        static bool SupportsComputShader()
        {
            Device device = new Device(DriverType.Hardware, DeviceCreationFlags.None);
            return device.CheckFeatureSupport(Feature.ComputeShaders);
        }
    }
}
