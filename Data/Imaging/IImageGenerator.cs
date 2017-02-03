using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImagingSIMS.Data.Imaging
{
    internal interface IImageGenerator
    {
        BitmapSource Create();
    }
}
