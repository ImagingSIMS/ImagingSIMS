using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using ImagingSIMS.Direct3DRendering;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Converters;
using ImagingSIMS.Data.Spectra;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Device = SharpDX.Direct3D11.Device;
using ImagingSIMS.Common;
using ImagingSIMS.Data.Imaging;

using Accord;
using Accord.Imaging;
using Accord.Math;

using System.Globalization;

namespace ConsoleApp
{
    partial class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Press Enter to begin...");
            //Console.ReadLine();

            FusionParticleStandards.Run();
            //CloudParticleStandards.Run();

            Console.Write("Press Enter to exit.");
            Console.ReadLine();
        }     
    }
}

