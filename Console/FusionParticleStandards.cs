using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImagingSIMS.Common;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;

namespace ConsoleApp
{
    public class FusionParticleStandards
    {
        public static void Run()
        {
            var outputFolder = "D:\\Data\\FusionParticles\\";

            List<SIMSParticle> simsParticles = new List<SIMSParticle>();
            List<SEMParticle> semParticles = new List<SEMParticle>();

            // Based on 256 x 256 pixels FOV
            int imageSize = 256;
            int hrFactor = 2;
            int[] radii = { 5, 8, 10, 15, 20, 25, 30, 40, 50, 65 };
            int[] centerX = { 240, 225, 150, 115, 115, 170, 200, 50, 190, 70 };
            int[] centerY = { 200, 225, 65, 85, 25, 225, 40, 50, 140, 180 };

            List<Tuple<int, int, int>> particleInfos = new List<Tuple<int, int, int>>()
            {
                new Tuple<int, int, int>(5, 240, 200),
                new Tuple<int, int, int>(8, 225, 225),
                new Tuple<int, int, int>(10, 150, 65),
                new Tuple<int, int, int>(15, 115, 85),
                new Tuple<int, int, int>(20, 115, 25),
                new Tuple<int, int, int>(25, 170, 225),
                new Tuple<int, int, int>(30, 200, 40),
                new Tuple<int, int, int>(40, 50, 50),
                new Tuple<int, int, int>(50, 190, 140),
                new Tuple<int, int, int>(65, 70, 180)
            };

            foreach (var info in particleInfos)
            {
                simsParticles.Add(new SIMSParticle(info.Item1, info.Item2, info.Item3, imageSize, imageSize));
                semParticles.Add(new SEMParticle(info.Item1 * hrFactor, info.Item2 * hrFactor, info.Item3 * hrFactor, imageSize * hrFactor, imageSize * hrFactor));
            }

            var simsData = new Data2D(imageSize, imageSize);
            var semData = new Data2D(imageSize * hrFactor, imageSize * hrFactor);

            foreach (var particle in simsParticles)
            {
                particle.GeneratePixels();
                simsData += particle.Matrix;
            }
            foreach (var particle in semParticles)
            {
                particle.GeneratePixels();
                semData += particle.Matrix;
            }

            var bsSIMS = ImageGenerator.Instance.Create(simsData, ColorScaleTypes.ThermalCold);
            var bsSEM = ImageGenerator.Instance.Create(semData, ColorScaleTypes.ThermalCold);

            bsSIMS.Save(outputFolder + "SIMS.bmp");
            bsSEM.Save(outputFolder + "SEM.bmp");

            //var testDataSIMS = new Data2D(256, 256);

            //for (int x = 0; x < testDataSIMS.Width; x++)
            //{
            //    for (int y = 0; y < testDataSIMS.Height; y++)
            //    {
            //        foreach (var particle in simsParticles)
            //        {
            //            if (IsInCircle(x, y, particle.CenterX, particle.CenterY, particle.Radius))
            //            {
            //                testDataSIMS[x, y] = 1;
            //                break;
            //            }
            //        }
            //    }
            //}

            //var bsTestDataSIMS = ImageGenerator.Instance.Create(testDataSIMS, ColorScaleTypes.ThermalCold);
            //bsTestDataSIMS.Save(outputFolder + "circles_sims.bmp");

            //var testDataSEM = new Data2D(512, 512);

            //for (int x = 0; x < testDataSEM.Width; x++)
            //{
            //    for (int y = 0; y < testDataSEM.Height; y++)
            //    {
            //        foreach (var particle in semParticles)
            //        {
            //            if (IsInCircle(x, y, particle.CenterX, particle.CenterY, particle.Radius))
            //            {
            //                testDataSEM[x, y] = 1;
            //                break;
            //            }
            //        }
            //    }
            //}

            //var bsTestDataSEM = ImageGenerator.Instance.Create(testDataSEM, ColorScaleTypes.ThermalCold);
            //bsTestDataSEM.Save(outputFolder + "circles_sem.bmp");            
        }

        private static bool IsInCircle(int x, int y, int cX, int cY, int radius)
        {
            return Math.Sqrt(Math.Pow(cX - x, 2) + Math.Pow(cY - y, 2)) <= radius;
        }
    }

    internal static class RandomGenerator
    {
        static Random _random = new Random();

        public static int Next()
        {
            return _random.Next();
        }
        public static int Next(int max)
        {
            return _random.Next(max);
        }
        public static int Next(int min, int max)
        {
            return _random.Next(min, max);
        }
    }

    internal abstract class Particle
    {
        public int ImageSizeX { get; set; }
        public int ImageSizeY { get; set; }
        public int Radius { get; set; }
        public int CenterX { get; set; }
        public int CenterY { get; set; }
        public int TotalCounts { get; set; }
        public Data2D Matrix { get; set; }

        public Particle(int radius, int centerX, int centerY, int imageSizeX, int imageSizeY)
        {
            Radius = radius;
            CenterX = centerX;
            CenterY = centerY;
            ImageSizeX = imageSizeX;
            ImageSizeY = imageSizeY;

            Matrix = new Data2D(imageSizeX, imageSizeY);
        }

        public abstract void GeneratePixels();

        protected static bool IsInCircle(int x, int y, int cX, int cY, int radius)
        {
            return Math.Sqrt(Math.Pow(cX - x, 2) + Math.Pow(cY - y, 2)) <= radius;
        }
    }

    internal class SIMSParticle : Particle
    {
        public SIMSParticle(int radius, int centerX, int centerY, int imageSizeX, int imageSizeY) 
            : base(radius, centerX, centerY, imageSizeX, imageSizeY)
        {
        }

        public override void GeneratePixels()
        {
            int idealCounts = (int)(Math.PI * Math.Pow(Radius, 2) * RandomGenerator.Next(50, 80));

            while(idealCounts > 0)
            {
                int x = RandomGenerator.Next(CenterX - Radius, CenterX + Radius);
                int y = RandomGenerator.Next(CenterY - Radius, CenterY + Radius);

                if (!IsInCircle(x, y, CenterX, CenterY, Radius)) continue;

                int counts = RandomGenerator.Next(10, 30);
                counts += RandomGenerator.Next(-(int)Math.Sqrt(counts), (int)Math.Sqrt(counts));

                double distanceToCenter = Math.Sqrt(Math.Pow(CenterX - x, 2) + Math.Pow(CenterY - y, 2)) / Radius;
                counts = (int)(counts / Math.Max(Math.Min(distanceToCenter, 1.3), 0.5));

                idealCounts -= counts;
                Matrix[x, y] += counts;
            }

            TotalCounts = (int)Matrix.TotalCounts;
        }
    }
    internal class SEMParticle : SIMSParticle
    {
        public SEMParticle(int radius, int centerX, int centerY, int imageSizeX, int imageSizeY) 
            : base(radius, centerX, centerY, imageSizeX, imageSizeY)
        {
        }

        public override void GeneratePixels()
        {
            //int idealCounts = (int)(Math.PI * Math.Pow(Radius, 2) * RandomGenerator.Next(13000, 15000));

            //while (idealCounts > 0)
            //{
            //    int x = RandomGenerator.Next(CenterX - Radius, CenterX + Radius);
            //    int y = RandomGenerator.Next(CenterY - Radius, CenterY + Radius);

            //    if (!IsInCircle(x, y, CenterX, CenterY, Radius)) continue;

            //    int counts = 100;
            //    //counts += RandomGenerator.Next(-(int)Math.Sqrt(counts), (int)Math.Sqrt(counts));

            //    double distanceToCenter = Math.Sqrt(Math.Pow(CenterX - x, 2) + Math.Pow(CenterY - y, 2)) / Radius;
            //    counts = (int)(counts / Math.Max(Math.Min(distanceToCenter, 1.2), 0.7));

            //    idealCounts -= counts;
            //    Matrix[x, y] += counts;
            //}

            for (int x = 0; x < ImageSizeX; x++)
            {
                for (int y = 0; y < ImageSizeY; y++)
                {
                    if (!IsInCircle(x, y, CenterX, CenterY, Radius)) continue;

                    double distanceToCenter = Math.Sqrt(Math.Pow(CenterX - x, 2) + Math.Pow(CenterY - y, 2)) / Radius;
                    //int counts = (int)(Math.Sqrt(100 / Math.Max(Math.Min(distanceToCenter, 1.0), 0.5)));
                    int counts = (int)(Math.Sqrt(1 - Math.Max(Math.Min(distanceToCenter, 1.0), 0.7)) * 1000);

                    Matrix[x, y] = counts;
                    //Matrix[x, y] = (float)distanceToCenter;
                }
            }

            TotalCounts = (int)Matrix.TotalCounts;
        }
    }
}
