using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;

namespace ImagingSIMS.Common
{
    public static class FileDialogs
    {
        public static OpenFileDialog OpenImageDialog
        {
            get
            {
                return new OpenFileDialog()
                {
                    Filter = "Image Files (.bmp, .jpg, .jpeg, .png)|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff|" +
                            "Bitmap Images (.bmp)|*.bmp|JPEG Images (.jpg, .jpeg)|*.jpg;*.jpeg|PNG Images (.png)|*.png|Tiff Images (.tif, .tiff)|*.tif;*.tiff",
                    Title = "Open Image File"
                };            
            }
        }
        public static SaveFileDialog SaveImageDialog
        {
            get
            {
                return new SaveFileDialog()
                {
                    Filter = "Bitmap Images (.bmp)|*.bmp",
                    Title = "Save Image File"
                };
            }
        }
    }
}
