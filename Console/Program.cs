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

using ImagingSIMS.Data;
using ImagingSIMS.Data.Colors;
using ImagingSIMS.Data.Converters;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Device = SharpDX.Direct3D11.Device;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press Enter to begin...");
            Console.ReadLine();

            int numTested = 0;
            int numHslFailed = 0;
            int numIhsFailed = 0;

            for (int r = 0; r < 256; r++)
            {
                for (int g = 0; g < 255; g++)
                {
                    for (int b = 0; b < 255; b++)
                    { 
                        numTested++;

                        RGB rgb = new RGB(r, g, b);
                        var hsl = ColorConversion.RGBtoHSL(rgb.Normalize());
                        var ihs = ColorConversion.RGBtoIHS(rgb.Normalize());

                        var hslBack = ColorConversion.HSLtoRGB(hsl);
                        var ihsBack = ColorConversion.IHStoRGB(ihs);

                        if (!hslBack.IsClose(rgb, 1.5))
                        {
                            Console.WriteLine($"HSL failed-- R:{r} G:{g} B:{b} ({hslBack.R},{hslBack.G},{hslBack.B})");
                            numHslFailed++;
                        }
                        if (!ihsBack.IsClose(rgb))
                        {
                            Console.WriteLine($"IHS failed-- R:{r} G:{g} B:{b} ({ihsBack.R},{ihsBack.G},{ihsBack.B})");
                            numIhsFailed++;
                        }
                    }
                }
            }

            Console.WriteLine($"Failed HSL: {(numHslFailed * 100d / numTested).ToString("0.0")}% IHS: {(numIhsFailed * 100d / numTested).ToString("0.0")}%");

            //IntPtr windowHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            //var desc = new SwapChainDescription()
            //{
            //    BufferCount = 1,
            //    ModeDescription = new ModeDescription(100, 100,
            //        new Rational(60, 1), Format.R8G8B8A8_UNorm),
            //    IsWindowed = true,
            //    OutputHandle = windowHandle,
            //    SampleDescription = new SampleDescription(1, 0),
            //    SwapEffect = SwapEffect.Discard,
            //    Usage = Usage.RenderTargetOutput,
            //};
            //Device device;
            //SwapChain swapChain;

            //Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, desc, out device, out swapChain);
            //int[,] testSpec = TestSpec.Generate();

            //var byteCode = ShaderBytecode.CompileFromFile("SpectrumView.hlsl", "cs_5_0", ShaderFlags.None, EffectFlags.None);
            //ComputeShader shader = new ComputeShader(device, byteCode);

            //int numTested = 0;
            //int numPassed = 0;
            //int numFailed = 0;

            //List<double[]> failed = new List<double[]>();

            //for (int r = 0; r < 256; r++)
            //{
            //    for (int g = 0; g < 256; g++)
            //    {
            //        for (int b = 0; b < 256; b++)
            //        {
            //            if (r% 5 != 0 || g % 5 != 0 || b % 5 != 0)
            //            {
            //                continue;
            //            }
            //            double[] hsl = ColorConversion.RGBtoHSL(r, g, b);

            //            double[] rgb = ColorConversion.HSLtoRGB(hsl[0], hsl[1], hsl[2]);

            //            if (Math.Abs(rgb[0] -  r) < 0.1d && Math.Abs(rgb[1] - g) < 0.1d && Math.Abs(rgb[2] - b) < 0.1d)
            //            {
            //                numPassed++;
            //            }
            //            else
            //            {
            //                numFailed++;
            //                failed.Add(new double[]
            //                {
            //                    r, g, b,
            //                    rgb[0], rgb[1], rgb[2],
            //                    hsl[0], hsl[1], hsl[2]
            //                });
            //            }

            //            numTested++;
            //        }
            //    }
            //}

            //Console.WriteLine($"Finished testing {numTested} color combinations.");
            //Console.WriteLine($"Number passed: {numPassed}; Number failed {numFailed}.");
            //Console.WriteLine("Press enter to save failing combinations.");
            //Console.ReadLine();

            //using (StreamWriter sw = new StreamWriter(@"D:\failingcolors.csv"))
            //{
            //    foreach (double[] fail in failed)
            //    {
            //        sw.WriteLine($"{fail[0]},{fail[1]},{fail[2]},{fail[3]},{fail[4]},{fail[5]},{fail[6]},{fail[7]},{fail[8]}");
            //    }
            //}

            //float result = 0f / 1f;
            //Console.WriteLine(result);

            Console.Write("Press Enter to exit.");
            Console.ReadLine();
        }

        private static int readLine(string line, out int type)
        {
            string[] parts = line.Split(' ');

            if (parts == null || parts.Length == 0)
            {
                type = 0;
                return 0;
            }

            type = int.Parse(parts[0]);

            if (parts.Length == 1 || parts[1] == null) return 0;

            return int.Parse(parts[1]);
        }
    }

    internal static class TestSpec
    {
        public static int[,] Generate()
        {
            int[,] spec = new int[10000, 2];

            Random r = new Random();
            int startTime = 5256;
            for (int x = 0; x < 10000; x++)
            {
                spec[x, 0] = startTime + x;
                spec[x, 1] = r.Next(0, 1000);
            }

            return spec;
        }
    }
}
