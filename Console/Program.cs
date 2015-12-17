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

using Direct3DRendering;
using ImagingSIMS.Data;

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

            IntPtr windowHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(100, 100,
                    new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = windowHandle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput,
            };
            Device device;
            SwapChain swapChain;

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, desc, out device, out swapChain);
            int[,] testSpec = TestSpec.Generate();

            var byteCode = ShaderBytecode.CompileFromFile("SpectrumView.hlsl", "cs_5_0", ShaderFlags.None, EffectFlags.None);
            ComputeShader shader = new ComputeShader(device, byteCode);

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
