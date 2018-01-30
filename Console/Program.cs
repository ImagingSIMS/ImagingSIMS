using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImagingSIMS.Common;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Converters;
using ImagingSIMS.Data.Imaging;
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
            Console.WriteLine("Press any key to begin");
            Console.ReadLine();

            var rand = new Random();

            var data = new Data2D(512, 512);
            for (int x = 0; x < data.Width; x++)
            {
                for (int y = 0; y < data.Height; y++)
                {
                    data[x, y] = rand.Next(100);
                }
            }

            var cpuGenerator = new CPUImageGenerator();
            var gpuGenerator = new GPUImageGenerator();

            foreach (ColorScaleTypes colorScale in Enum.GetValues(typeof(ColorScaleTypes)))
            {
                var resultsCpu = new List<long>();
                var resultsGpu = new List<long>();

                var numTests = 1000;

                var stopWatch = new Stopwatch();

                if (colorScale == ColorScaleTypes.Solid)
                {
                    for (int i = 0; i < numTests; i++)
                    {
                        stopWatch.Restart();
                        var bs = cpuGenerator.Create(data, System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                        var elapsed = stopWatch.ElapsedMilliseconds;
                        resultsCpu.Add(elapsed);
                    }

                    // Throw one test away because the first call will need to compile the shader
                    var discard = gpuGenerator.Create(data, System.Windows.Media.Color.FromArgb(255, 255, 255, 255));

                    for (int i = 0; i < numTests; i++)
                    {
                        stopWatch.Restart();
                        var bs = gpuGenerator.Create(data, System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                        var elapsed = stopWatch.ElapsedMilliseconds;
                        resultsGpu.Add(elapsed);
                    }
                }
                else
                {
                    for (int i = 0; i < numTests; i++)
                    {
                        stopWatch.Restart();
                        var bs = cpuGenerator.Create(data, colorScale);
                        var elapsed = stopWatch.ElapsedMilliseconds;
                        resultsCpu.Add(elapsed);
                    }

                    // Throw one test away because the first call will need to compile the shader
                    var discard = gpuGenerator.Create(data, colorScale);

                    for (int i = 0; i < numTests; i++)
                    {
                        stopWatch.Restart();
                        var bs = gpuGenerator.Create(data, colorScale);
                        var elapsed = stopWatch.ElapsedMilliseconds;
                        resultsGpu.Add(elapsed);
                    }
                }

                var meanCpu = resultsCpu.Average();
                var meanGpu = resultsGpu.Average();

                Console.WriteLine($"Color scale: {colorScale}\t\tNumber tests: {numTests} CPU: {meanCpu.ToString()} GPU: {meanGpu.ToString()}");
            }

            Console.Write("Press any key to exit.");
            Console.ReadLine();
        }     
    }
}

