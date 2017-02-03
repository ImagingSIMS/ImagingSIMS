using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SharpDX.Direct3D11;

namespace ImagingSIMS.Data.Imaging
{
    public class GPUImageGenerator : IImageGenerator
    {
        private Device _device;

        public BitmapSource Create()
        {
            throw new NotImplementedException();
        }
    }
}
