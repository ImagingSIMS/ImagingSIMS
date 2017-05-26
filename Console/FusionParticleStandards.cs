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

            // 2 px - 20 px
            //List<Tuple<int, int, int>> particleInfos = new List<Tuple<int, int, int>>()
            //{
            //    // Row 1
            //    new Tuple<int, int, int>(2, 40, 30),
            //    new Tuple<int, int, int>(3, 105, 30),
            //    new Tuple<int, int, int>(4, 170, 30),
            //    new Tuple<int, int, int>(5, 235, 30),

            //    // Row 2
            //    new Tuple<int, int, int>(9, 225, 85),
            //    new Tuple<int, int, int>(8, 160, 85),
            //    new Tuple<int, int, int>(7, 95, 85),
            //    new Tuple<int, int, int>(6, 30, 85),

            //    // Row 3
            //    new Tuple<int, int, int>(10, 60, 145),
            //    new Tuple<int, int, int>(12, 130, 145),
            //    new Tuple<int, int, int>(14, 200, 145),

            //    // Row 4
            //    new Tuple<int, int, int>(16, 30, 215),
            //    new Tuple<int, int, int>(18, 105, 215),
            //    new Tuple<int, int, int>(20, 180, 215),               

            //};

            // 10 px - 50 px
            //List<Tuple<int, int, int>> particleInfos = new List<Tuple<int, int, int>>()
            //{
            //    // Row 1
            //    new Tuple<int, int, int>(10, 27, 25),
            //    new Tuple<int, int, int>(15, 80, 30),
            //    new Tuple<int, int, int>(20, 142, 35),
            //    new Tuple<int, int, int>(25, 215, 40),

            //    // Row 2
            //    new Tuple<int, int, int>(35, 50, 98),
            //    new Tuple<int, int, int>(40, 200, 128),

            //    // Row 3
            //    new Tuple<int, int, int>(50, 105, 193),

            //};

            // 40 px - 60 px
            List<Tuple<int, int, int>> particleInfos = new List<Tuple<int, int, int>>()
            {
                new Tuple<int, int, int>(40, 55, 55),
                new Tuple<int, int, int>(50, 190, 65),
                new Tuple<int, int, int>(60, 75, 180),                
            };

            foreach (var info in particleInfos)
            {
                int radius = (int)(.9326 * info.Item1 + 12.885);
                simsParticles.Add(new SIMSParticle(radius, info.Item2, info.Item3, imageSize, imageSize));                
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

            var bsSIMS = ImageGenerator.Instance.Create(simsData, ColorScaleTypes.ThermalWarm);
            var bsSEM = ImageGenerator.Instance.Create(semData, ColorScaleTypes.Gray);

            bsSIMS.Save(outputFolder + "SIMS.bmp");
            bsSEM.Save(outputFolder + "SEM.bmp");

            simsData.Save(outputFolder + "SIMS.txt", ImagingSIMS.Data.Spectra.FileType.CSV);
            semData.Save(outputFolder + "SEM.txt", ImagingSIMS.Data.Spectra.FileType.CSV);
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
        public static double NextDouble()
        {
            return _random.NextDouble();
        }

        public static double NextGaussian(double mean=0, double stdDev = 0.2)
        {
            var u1 = 1 - _random.NextDouble();
            var u2 = 1 - _random.NextDouble();

            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            var randNormal = mean + stdDev * randStdNormal;

            return randNormal;
        }
        public static double NextGaussianNormal(double mean=0, double stdDev=0.2)
        {
            var randNormal = -1d;
            while (randNormal < 0 || randNormal >= 1)
            {
                var u1 = 1 - _random.NextDouble();
                var u2 = 1 - _random.NextDouble();

                var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                randNormal = mean + stdDev * randStdNormal;
            }

            return randNormal;
        }
        private static double Gaussian(double x, double mu, double sigmaSquared)
        {
            return (Math.Pow(Math.E, -Math.Pow(x - mu, 2) / (2 * sigmaSquared)) / Math.Sqrt(2 * Math.PI * sigmaSquared));
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

            //for (int i = 0; i < 100; i++)
            //{
            //    double distance  = i / 100d;
            //    Console.WriteLine($"i {i} distance {Gaussian(distance, 0, 0.2)}");
            //}
        }

        public override void GeneratePixels()
        {
            int idealCounts = (int)(Math.PI * Math.Pow(Radius, 2) * RandomGenerator.Next(50, 80));

            double sigmaSquared = .2;
            double mu = 0;

            while(idealCounts > 0)
            {
                int x = RandomGenerator.Next(CenterX - Radius, CenterX + Radius);
                int y = RandomGenerator.Next(CenterY - Radius, CenterY + Radius);

                if (!IsInCircle(x, y, CenterX, CenterY, Radius)) continue;
                if (x < 0 || x >= ImageSizeX || y < 0 || y >= ImageSizeY) continue;

                int counts = RandomGenerator.Next(10, 30);
                counts += RandomGenerator.Next(-(int)Math.Sqrt(counts), (int)Math.Sqrt(counts));

                double distanceToCenter = Math.Sqrt(Math.Pow(CenterX - x, 2) + Math.Pow(CenterY - y, 2)) / Radius;
                //counts = (int)(counts / Math.Max(Math.Min(distanceToCenter, 1.3), 0.5));
                counts = (int)(counts * Gaussian(distanceToCenter, mu, sigmaSquared));

                idealCounts -= counts;
                Matrix[x, y] += counts;
            }

            TotalCounts = (int)Matrix.TotalCounts;
        }

        private double Gaussian(double x, double mu, double sigmaSquared)
        {
            //return (1 / Math.Sqrt(2 * Math.PI * sigmaSquared)) * Math.Pow(Math.E, -(Math.Pow(x - mu, 2) / 2 * sigmaSquared));
            return (Math.Pow(Math.E, -Math.Pow(x - mu, 2) / (2 * sigmaSquared)) / Math.Sqrt(2 * Math.PI * sigmaSquared));
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
            for (int x = 0; x < ImageSizeX; x++)
            {
                for (int y = 0; y < ImageSizeY; y++)
                {
                    if (!IsInCircle(x, y, CenterX, CenterY, Radius)) continue;

                    double distanceToCenter = Math.Sqrt(Math.Pow(CenterX - x, 2) + Math.Pow(CenterY - y, 2)) / Radius;
                    int counts = (int)(Math.Sqrt(1 - Math.Pow(distanceToCenter, 2)) * 1000);

                    Matrix[x, y] = counts;
                }
            }

            TotalCounts = (int)Matrix.TotalCounts;
        }
    }
}
