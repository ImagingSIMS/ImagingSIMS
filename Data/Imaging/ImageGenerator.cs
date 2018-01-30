using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        static BaseImageGenerator _generator;

        public static BaseImageGenerator Instance
        {
            get { return _generator; }
        }

        static ImageGenerator()
        {
            // Uncomment when GPU is fully implemented
            if (SupportsComputShader())
                _generator = new GPUImageGenerator();
            else _generator = new CPUImageGenerator();

            //_generator = new CPUImageGenerator();
        }

        static bool SupportsComputShader()
        {
            Device device = new Device(DriverType.Hardware, DeviceCreationFlags.None);
            return device.CheckFeatureSupport(Feature.ComputeShaders);
        }
    }

    public enum ImageGeneratorType { CPU, GPU }
    public static class SmartImageGenerator
    {
        static CPUImageGenerator _cpuImageGenerator;
        static GPUImageGenerator _gpuImageGenerator;

        static Dictionary<int, ImageGeneratorType> _times;
        static bool _supportsGPU;

        static SmartImageGenerator()
        {
            _supportsGPU = SupportsComputeShader();

            _cpuImageGenerator = new CPUImageGenerator();
            if (_supportsGPU)
            {
                _gpuImageGenerator = new GPUImageGenerator();
                CalculateImageTimes();
            }
        }

        private static void CalculateImageTimes()
        {
            int[] pixelsToCheck = new int[]
                { 32,64,128,256,1024,2048 };

            Stopwatch timer = new Stopwatch();

            foreach (int pixels in pixelsToCheck)
            {
                Data2D d = new Data2D(pixels, pixels);

                timer.Reset();
                timer.Start();

                var cpuImage = _cpuImageGenerator.Create(d, ColorScaleTypes.ThermalCold);
                long cpuTime = timer.ElapsedMilliseconds;

                timer.Reset();
                timer.Start();

                var gpuImage = _gpuImageGenerator.Create(d, ColorScaleTypes.ThermalCold);
                long gpuTime = timer.ElapsedMilliseconds;

                _times.Add(pixels * pixels, cpuTime < gpuTime ? ImageGeneratorType.CPU : ImageGeneratorType.GPU);
            }
        }

        static bool SupportsComputeShader()
        {
            Device device = new Device(DriverType.Hardware, DeviceCreationFlags.None);
            return device.CheckFeatureSupport(Feature.ComputeShaders);
        }

        public static BaseImageGenerator GetImageGenerator()
        {
            return _cpuImageGenerator;
        }
        public static BaseImageGenerator GetImageGenerator(int pixelsX, int pixelsY)
        {
            if (!_supportsGPU) return _cpuImageGenerator;

            int numPixels = pixelsX * pixelsY;
            if (_times.ContainsKey(numPixels))
                return GetImageGenerator(_times[numPixels]);

            for (int i = 0; i < _times.Count - 1; i++)
            {
                if (numPixels > _times.Keys.ElementAt(i) && numPixels < _times.Keys.ElementAt(i))
                    return GetImageGenerator(_times[i + 1]);
            }

            return _cpuImageGenerator;
        }

        public static BaseImageGenerator GetImageGenerator(ImageGeneratorType generatorType)
        {
            switch (generatorType)
            {
                case ImageGeneratorType.CPU:
                    return _cpuImageGenerator;
                case ImageGeneratorType.GPU:
                    return _gpuImageGenerator;
                default:
                    return _cpuImageGenerator;
            }
        }
    }
}
