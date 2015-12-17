using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ImagingSIMS.Data.Imaging
{
    public static class StackedImage
    {
        public static void Save(Data3D imageToStack, string filePath, int numberLayers = 4)
        {
            using(BinaryWriter bw = new BinaryWriter(File.OpenWrite(filePath)))
            {       
                for (int z = 0; z < numberLayers; z++)
                {
                    for (int x = 0; x < imageToStack.Width; x++)
                    {
                        for (int y = 0; y < imageToStack.Height; y++)
                        {
                            Color c = imageToStack[new Point(x, y)];

                            bw.Write(c.R);
                            bw.Write(c.G);
                            bw.Write(c.B);
                        }
                    }
                }                
            }
        }
    }
}
