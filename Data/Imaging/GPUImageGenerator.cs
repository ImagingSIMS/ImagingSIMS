using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace ImagingSIMS.Data.Imaging
{
    public class GPUImageGenerator : BaseImageGenerator
    {
        private static Device _device;

        static GPUImageGenerator()
        {
            _device = new Device(DriverType.Hardware, DeviceCreationFlags.None);            
        }

        public override BitmapSource Create(Data3D channelData)
        {
            throw new NotImplementedException();
        }
        public override BitmapSource Create(Data2D data, Color solidColor)
        {
            throw new NotImplementedException();
        }
        public override BitmapSource[] Create(ImageComponent[] components, ImagingParameters parameters)
        {
            throw new NotImplementedException();
        }
        public override BitmapSource Create(Data2D data, ColorScaleTypes scale)
        {
            throw new NotImplementedException();
        }
        public override BitmapSource Create(Data2D data, Color solidColor, float saturation)
        {
            throw new NotImplementedException();
        }
        public override BitmapSource Create(Data2D data, ColorScaleTypes scale, float saturation)
        {
            throw new NotImplementedException();
        }
        public override BitmapSource Create(Data2D data, Color solidColor, float saturation, float threshold)
        {
            throw new NotImplementedException();
        }
        public override BitmapSource Create(Data2D data, ColorScaleTypes scale, float saturation, float threshold)
        {
            throw new NotImplementedException();
        }
    }
}
