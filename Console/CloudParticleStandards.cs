using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImagingSIMS.Common;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;

namespace ConsoleApp
{
    public class CloudParticleStandards
    {
        public static void Run()
        {
            Console.WriteLine("Loading shape prototpes");
            var prototypes = new ParticlePrototypes();
            var filePaths = Directory.GetFiles(@"D:\Data\Particles-pikOvLitR\").Where(p => p.EndsWith(".txt"));
            prototypes.LoadFromFiles(filePaths);

            Console.WriteLine("Creating target FOVs");
            int imageSizeX = 256;
            int imageSizeY = 256;
            int numFovsX = 10;
            int numFovsY = 10;
            FieldsOfView fovs = new FieldsOfView(imageSizeX, imageSizeY, numFovsX, numFovsY);

            Console.WriteLine("Randomizing particle shapes from prototypes");
            int numParticles = 1500;
            int minSize = 25;
            int maxSize = 200;
            int overlapThreshold = 60;
            List<CloudParticle> particles = new List<CloudParticle>();
            for (int i = 0; i < numParticles; i++)
            {
                var particle = new CloudParticle(i);
                particle.RandomizePrototypeShape(prototypes.SelectRandom(), minSize, maxSize);
                particles.Add(particle);
            }

            Console.WriteLine("Assigning particles to FOVs");
            foreach (var particle in particles)
            {
                int ox = RandomGenerator.Next(0, imageSizeX);
                int oy = RandomGenerator.Next(0, imageSizeY);

                // Find offset such that particle is within image bounds
                while (ox < 5 || ox + particle.SizeX >= imageSizeX - 5 ||
                    oy < 5 || oy + particle.SizeY >= imageSizeY - 5)
                {
                    ox = RandomGenerator.Next(0, imageSizeX);
                    oy = RandomGenerator.Next(0, imageSizeY);
                }

                particle.OffsetX = ox;
                particle.OffsetY = oy;

                var fovTracker = new AssignTracker(numFovsX, numFovsY);

                int a = RandomGenerator.Next(0, numFovsX);
                int b = RandomGenerator.Next(0, numFovsY);

                // Try to assign particle to FOV
                while (!fovs[a, b].TryAssignParticle(particle, overlapThreshold))
                {
                    // If no more FOVs are left to try, 
                    if (!fovTracker.MarkAsChecked(a, b)) break;

                    a = RandomGenerator.Next(0, numFovsX);
                    b = RandomGenerator.Next(0, numFovsY);
                }
            }

            var notAssigned = particles.Where(p => !p.IsAssigned);
            if(notAssigned.Count() > 0)
            {
                Console.WriteLine($"Warning: {notAssigned.Count()} particles were not assigned");
            }

            double[] isotopeRatios = new double[]
            {
                0.0072614, 0.015565, 0.18109, 0.9997, 6.148
            };

            List<Tuple<int, double, double, double>> distribution = new List<Tuple<int, double, double, double>>();

            Console.WriteLine("Generating isotopic distributions");
            var assigned = particles.Where(p => p.IsAssigned);
            foreach (var particle in assigned)
            {
                var ratio = isotopeRatios[RandomGenerator.Next(0, isotopeRatios.Length)];
                particle.AssignCounts(ratio);

                distribution.Add(new Tuple<int, double, double, double>(
                    particle.Id, particle.Matrix235.TotalCounts, particle.Matrix238.TotalCounts, particle.Matrix235.TotalCounts + particle.Matrix238.TotalCounts));
            }

            Console.WriteLine("Saving shape matrix images");
            string outputFolder = @"D:\Data\CloudParticles\";
            for (int x = 0; x < numFovsX; x++)
            {
                for (int y = 0; y < numFovsY; y++)
                {
                    var totalMatrix = fovs[x, y].Counts;
                    var bsTotalMatrix = ImageGenerator.Instance.Create(totalMatrix, ColorScaleTypes.ThermalCold);
                    bsTotalMatrix.Save(outputFolder + $"X {x} Y {y}.bmp");
                }
            }

            using (StreamWriter sw = new StreamWriter(File.OpenWrite(outputFolder + "distributions.csv")))
            {
                sw.WriteLine("Id,235,238,Total");

                foreach (var item in distribution)
                {
                    sw.WriteLine($"{item.Item1},{item.Item2},{item.Item3},{item.Item4}");
                }
            }

        }
    }

    internal class AssignTracker
    {
        bool[,] _checked;

        public AssignTracker(int sizeX, int sizeY)
        {
            _checked = new bool[sizeX, sizeY];
        }

        public bool MarkAsChecked(int x, int y)
        {
            _checked[x, y] = true;

            foreach (var b in _checked)
            {
                if (!b) return true;
            }

            return false;
        }
    }

    internal class FieldsOfView
    {
        public int FovsX { get; private set; }
        public int FovsY { get; private set; }

        public FieldOfView this[int x, int y]
        {
            get { return _fovs[x, y]; }
        }

        private FieldOfView[,] _fovs;
        
        public FieldsOfView(int fovSizeX, int fovSizeY, int fovsX, int fovsY)
        {
            FovsX = fovsX;
            FovsY = fovsY;

            _fovs = new FieldOfView[fovsX, fovsY];
            for (int x = 0; x < fovsX; x++)
            {
                for (int y = 0; y < fovsY; y++)
                {
                    _fovs[x, y] = new FieldOfView(fovSizeX, fovSizeY);
                }
            }
        }
    }
    internal class FieldOfView
    {
        public int SizeX { get; private set; }
        public int SizeY { get; private set; }
        public List<CloudParticle> Particles { get; private set; }

        public Data2D ShapeMatrix
        {
            get
            {
                Data2D matrix = new Data2D(SizeX, SizeY);
                foreach (var particle in Particles)
                {
                    for (int x = 0; x < particle.SizeX; x++)
                    {
                        for (int y = 0; y < particle.SizeY; y++)
                        {
                            int a = particle.OffsetX + x;
                            int b = particle.OffsetY + y;
                            matrix[a, b] += particle.ShapeMatrix[x, y];
                        }
                    }
                }
                return matrix;
            }
        }
        public Data2D Counts235
        {
            get
            {
                Data2D matrix = new Data2D(SizeX, SizeY);
                foreach (var particle in Particles)
                {
                    for (int x = 0; x < particle.SizeX; x++)
                    {
                        for (int y = 0; y < particle.SizeY; y++)
                        {
                            int a = particle.OffsetX + x;
                            int b = particle.OffsetY + y;
                            matrix[a, b] += particle.Matrix235[x, y];
                        }
                    }
                }
                return matrix;
            }
        }
        public Data2D Counts238
        {
            get
            {
                Data2D matrix = new Data2D(SizeX, SizeY);
                foreach (var particle in Particles)
                {
                    for (int x = 0; x < particle.SizeX; x++)
                    {
                        for (int y = 0; y < particle.SizeY; y++)
                        {
                            int a = particle.OffsetX + x;
                            int b = particle.OffsetY + y;
                            matrix[a, b] += particle.Matrix238[x, y];
                        }
                    }
                }
                return matrix;
            }
        }
        public Data2D Counts
        {
            get
            {
                Data2D matrix = new Data2D(SizeX, SizeY);
                foreach (var particle in Particles)
                {
                    for (int x = 0; x < particle.SizeX; x++)
                    {
                        for (int y = 0; y < particle.SizeY; y++)
                        {
                            int a = particle.OffsetX + x;
                            int b = particle.OffsetY + y;
                            matrix[a, b] += particle.Matrix235[x, y];
                            matrix[a, b] += particle.Matrix238[x, y];
                        }
                    }
                }
                return matrix;
            }
        }
        public Data2D GroundTruthImage
        {
            get
            {
                Data2D matrix = new Data2D(SizeX, SizeY);
                int particleCounter = 1;
                foreach (var particle in Particles)
                {
                    for (int x = 0; x < particle.SizeX; x++)
                    {
                        for (int y = 0; y < particle.SizeY; y++)
                        {
                            int a = particle.OffsetX + x;
                            int b = particle.OffsetY + y;
                            if(particle.ShapeMatrix[x,y] == 1)
                            {
                                matrix[a, b] = particleCounter;
                            }
                        }
                    }
                    particleCounter++;
                }
                return matrix;
            }
        }

        public FieldOfView(int sizeX, int sizeY)
        {
            SizeX = sizeX;
            SizeY = sizeY;

            Particles = new List<CloudParticle>();
        }

        public bool TryAssignParticle(CloudParticle particle, int overlapThreshold)
        {
            foreach (var current in Particles)
            {
                if (CloudParticle.Overlaps(particle, current, overlapThreshold)) return false;
                //if (CloudParticle.OverlapPercentage(particle, current) > overlapThreshold) return false;
            }

            particle.IsAssigned = true;
            Particles.Add(particle);
            return true;
        }
    }

    internal class ParticlePrototypes : List<CloudParticlePrototype>
    {
        internal ParticlePrototypes() : base()
        {

        }

        internal void LoadFromFiles(IEnumerable<string> filePaths)
        {
            int prototypeSize = 200;

            foreach (var filePath in filePaths)
            {
                var match = Regex.Match(Path.GetFileNameWithoutExtension(filePath), @"\d+");

                int index = int.Parse(match.Value);
                bool[,] matrix = new bool[prototypeSize, prototypeSize];

                using (StreamReader reader = new StreamReader(File.OpenRead(filePath)))
                {
                    // Read line by line and delimit
                    for (int x = 0; x < prototypeSize; x++)
                    {
                        var line = reader.ReadLine();
                        var splits = line.Split(',');

                        for (int y = 0; y < prototypeSize; y++)
                        {
                            matrix[y, x] = int.Parse(splits[y]) == 1;
                        }
                    }
                }

                Add(new CloudParticlePrototype(index, matrix));
            }
        }
        public CloudParticlePrototype SelectRandom()
        {
            int r = RandomGenerator.Next(0, Count);
            return this[r];
        }
    }

    class CloudParticlePrototype
    {
        public int Id { get; set; }
        public bool[,] Mask { get; set; }
        public int SizeX { get; set; }
        public int SizeY { get; set; }

        public int MaskSizeX
        {
            get { return Mask.GetLength(0); }
        }
        public int MaskSizeY
        {
            get { return Mask.GetLength(1); }
        }

        public CloudParticlePrototype(int index, bool[,] matrix)
        {
            Id = index;
            Mask = matrix;

            SizeX = matrix.GetLength(0);
            SizeY = matrix.GetLength(1);
        }

        public CloudParticlePrototype GetDownscaled(double ratio)
        {
            var mask = GetDownscaledMask(ratio);
            return new CloudParticlePrototype(Id, mask);
        }
        private bool[,] GetDownscaledMask(double ratio)
        {
            int rescaledSizeX = (int)(MaskSizeX * ratio);
            int rescaledSizeY = (int)(MaskSizeY * ratio);

            int windowX = 2;
            int windowY = 2;

            bool[,] rescaled = new bool[rescaledSizeX, rescaledSizeY];

            for (int x = 0; x < rescaledSizeX; x++)
            {
                for (int y = 0; y < rescaledSizeY; y++)
                {
                    int x1 = (int)(x / ratio - windowX).Clamp(0, MaskSizeX - 1);
                    int y1 = (int)(y / ratio - windowY).Clamp(0, MaskSizeY - 1);
                    int x2 = (int)(x / ratio + windowX).Clamp(0, MaskSizeX - 1);
                    int y2 = (int)(y / ratio + windowY).Clamp(0, MaskSizeY - 1);

                    int sum = 0;
                    int ct = 0;
                    for (int a = x1; a <= x2; a++)
                    {
                        for (int b = y1; b <= y2; b++)
                        {
                            ct++;

                            if (Mask[a, b]) sum++;
                        }
                    }

                    rescaled[x, y] = sum / ct > 0.5f;
                }
            }

            return rescaled;
        }
    }

    class CloudParticle
    {
        public int Id { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int SizeX { get; set; }
        public int SizeY { get; set; }
        public bool IsAssigned { get; set; }
        public Data2D ShapeMatrix { get; set; }
        public Data2D Matrix235 { get; set; }
        public Data2D Matrix238 { get; set; }
        public Data2D Matrix
        {
            get { return Matrix235 + Matrix238; }
        }

        public CloudParticle(int id)
        {
            Id = id;
        }

        public void RandomizePrototypeShape(CloudParticlePrototype prototype, int minSize, int maxSize)
        {

            double newSize = RandomGenerator.NextGaussianNormal(0, 0.4) * maxSize + minSize;
            double ratio = newSize / prototype.SizeX;

            var cloud = prototype.GetDownscaled(ratio);

            ShapeMatrix = new Data2D(cloud.MaskSizeX, cloud.MaskSizeY);
            for (int x = 0; x < cloud.MaskSizeX; x++)
            {
                for (int y = 0; y < cloud.MaskSizeY; y++)
                {
                    if (cloud.Mask[x, y]) ShapeMatrix[x, y] = 1;
                }
            }

            int flipType = RandomGenerator.Next(0, 5);
            // O is no transform, 1-4 vary below
            switch (flipType)
            {
                case 1:
                    ShapeMatrix.FlipHorizontal();
                    break;
                case 2:
                    ShapeMatrix.FlipVertical();
                    break;
                case 3:
                    ShapeMatrix.FlipDiagonal(false);
                    break;
                case 4:
                    ShapeMatrix.FlipDiagonal(true);
                    break;
            }

            SizeX = ShapeMatrix.Width;
            SizeY = ShapeMatrix.Height;

            Matrix235 = new Data2D(SizeX, SizeY);
            Matrix238 = new Data2D(SizeX, SizeY);
        }

        public static bool Overlaps(CloudParticle particle, CloudParticle target, int overlapPercent)
        {
            int intLeft = Math.Max(particle.OffsetX, target.OffsetX);
            int intRight = Math.Min(particle.OffsetX + particle.SizeX, target.OffsetX + target.SizeX);
            int intTop = Math.Max(particle.OffsetY, target.OffsetY);
            int intBot = Math.Min(particle.OffsetY + particle.SizeY, target.OffsetY + target.SizeY);

            double areaOverlap = (intRight - intLeft) * (intBot - intTop);
            if (areaOverlap <= 0) return false;

            double areaParticle = particle.SizeX * particle.SizeY;
            double areaTarget = target.SizeX * target.SizeY;

            return areaOverlap * 100 / areaTarget > overlapPercent || areaOverlap * 100 / areaParticle > overlapPercent;
        }
        public static double OverlapPercentage(CloudParticle particle, CloudParticle target)
        {
            int intLeft = Math.Max(particle.OffsetX, target.OffsetX);
            int intRight = Math.Min(particle.OffsetX + particle.SizeX, target.OffsetX + target.SizeX);
            int intTop = Math.Max(particle.OffsetY, target.OffsetY);
            int intBot = Math.Min(particle.OffsetY + particle.SizeY, target.OffsetY + target.SizeY);

            double areaOverlap = (intRight - intLeft) * (intBot - intTop);
            if (areaOverlap <= 0) return 0;

            double areaParticle = particle.SizeX * particle.SizeY;
            double areaTarget = target.SizeX * target.SizeY;

            return areaOverlap * 100 / (areaParticle + areaTarget - areaOverlap);
        }   

        public void AssignCounts(double ratio)
        {
            int centerX = 0;
            int centerY = 0;
            int numPixels = 0;

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    if (ShapeMatrix[x, y] == 1)
                    {
                        centerX += x;
                        centerY += y;
                        numPixels++;
                    }
                }
            }

            centerX /= numPixels;
            centerY /= numPixels;

            double radius = 0;

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    if (ShapeMatrix[x, y] == 0) continue;

                    var r = Math.Sqrt(Math.Pow(centerX - x, 2) + Math.Pow(centerY - y, 2));
                    if (r > radius) radius = r;
                }
            }

            int idealCounts = numPixels * RandomGenerator.Next(150, 200);

            while (idealCounts > 0)
            {
                int x = RandomGenerator.Next(0, SizeX);
                int y = RandomGenerator.Next(0, SizeY);

                if (ShapeMatrix[x, y] == 0) continue;

                double distanceToCenter = Math.Sqrt(Math.Pow(centerX - x, 2) + Math.Pow(centerY - y, 2)) / radius;
                double countsAtPixel = Gaussian(distanceToCenter, 0, 0.3) * RandomGenerator.Next(150, 200);
                int counts235 = (int)(countsAtPixel / ((1 / ratio) + 1));
                int counts238 = (int)(countsAtPixel - counts235);

                int actualCounts235 = RandomGenerator.Next(counts235 - (int)Math.Sqrt(counts235), counts235 + (int)Math.Sqrt(counts235));
                int actualCounts238 = RandomGenerator.Next(counts238 - (int)Math.Sqrt(counts238), counts238 + (int)Math.Sqrt(counts238));
                Matrix235[x, y] += actualCounts235;
                Matrix238[x, y] += actualCounts238;

                idealCounts -= actualCounts235 + actualCounts238;
            }
        }

        private double Gaussian(double x, double mu, double sigmaSquared)
        {
            return (Math.Pow(Math.E, -Math.Pow(x - mu, 2) / (2 * sigmaSquared)) / Math.Sqrt(2 * Math.PI * sigmaSquared));
        }
        private double DistanceToCenter(int x, int y)
        {
            return Math.Sqrt(Math.Pow(SizeX / 2 - x, 2) + Math.Pow(SizeY / 2 - y, 2));
        }
    }
}
