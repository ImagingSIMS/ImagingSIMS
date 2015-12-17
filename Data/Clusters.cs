using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ImagingSIMS.Data
{
    namespace ClusterIdentification
    {
        internal struct ClusterPoint
        {
            public int X;
            public int Y;

            public override string ToString()
            {
                return string.Format("X: {0} Y: {1}", X, Y);
            }
            internal ClusterPoint(int x, int y)
            {
                X = x;
                Y = y;
            }

            public override bool Equals(object obj)
            {
                try
                {
                    ClusterPoint pt = (ClusterPoint)obj;
                    return pt.X == X && pt.Y == Y;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            public override int GetHashCode()
            {
                int hash = 13;
                hash = (hash * 7) + X.GetHashCode();
                hash = (hash * 7) + Y.GetHashCode();
                return hash;
            }
        }
        public class Cluster : INotifyPropertyChanged
        {
            int _clusterNumber;
            Color _color;

            List<ClusterPoint> _pixels;

            public event PropertyChangedEventHandler PropertyChanged;
            public void NotifyPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            internal List<ClusterPoint> Pixels
            {
                get { return _pixels; }
                set { _pixels = value; }
            }

            public Cluster()
            {
                _pixels = new List<ClusterPoint>();
            }
            internal Cluster(int[] xCoords, int[] yCoords)
            {
                _pixels = new List<ClusterPoint>();

                for (int i = 0; i < xCoords.Length; i++)
                {
                    _pixels.Add(new ClusterPoint() { X = xCoords[i], Y = yCoords[i] });
                }
            }
            internal Cluster(List<ClusterPoint> pixels)
            {
                _pixels = pixels;
            }

            internal int numberPixels
            {
                get
                {
                    return _pixels.Count;
                }
            }
            internal ClusterPoint center
            {
                get
                {
                    float xSum = 0;
                    float ySum = 0;

                    foreach (ClusterPoint pixel in _pixels)
                    {
                        xSum += pixel.X;
                        ySum += pixel.Y;
                    }

                    return new ClusterPoint()
                    {
                        X = (int)(xSum / (float)numberPixels),
                        Y = (int)(ySum / (float)numberPixels)
                    };
                }
            }
            public int CenterX
            {
                get
                {
                    float xSum = 0;
                    foreach (ClusterPoint pixel in _pixels)
                    {
                        xSum += pixel.X;
                    }

                    return (int)(xSum / (float)numberPixels);
                }
            }
            public int CenterY
            {
                get
                {
                    float ySum = 0;
                    foreach (ClusterPoint pixel in _pixels)
                    {
                        ySum += pixel.Y;
                    }

                    return (int)(ySum / (float)numberPixels);
                }
            }
            public int NumberPixels
            {
                get { return numberPixels; }
            }
            public int ClusterNumber
            {
                get { return _clusterNumber; }
                set
                {
                    if(_clusterNumber != value)
                    {
                        _clusterNumber = value;
                        NotifyPropertyChanged("ClusterNumber");
                    }
                }
            }

            public Color Color
            {
                get { return _color; }
                set
                {
                    if(_color != value)
                    {
                        _color = value;
                        NotifyPropertyChanged("Color");
                    }
                }
            }

            internal bool overlaps(Cluster cluster)
            {
                foreach (ClusterPoint point in cluster.Pixels)
                {
                    if (containsPixel(point))
                        return true;
                }
                return false;
            }
            internal bool containsPixel(ClusterPoint pixel)
            {
                return _pixels.Contains(pixel);
            }
            internal void merge(Cluster toMerge)
            {
                foreach (ClusterPoint pixel in toMerge._pixels)
                {
                    if (!_pixels.Contains(pixel))
                    {
                        _pixels.Add(pixel);
                    }
                }
            }

            public static Cluster MergeClusters(List<Cluster> clusters)
            {
                List<ClusterPoint> points = new List<ClusterPoint>();

                foreach(Cluster cluster in clusters)
                {
                    points.AddRange(cluster.Pixels);
                }

                Cluster merged = new Cluster(points);

                return merged;
            }
        }

        public class FoundClusters : INotifyPropertyChanged
        {
            int _width;
            int _height;

            FoundClusterCollection _clusters;
            Data3D _colorMask;

            public FoundClusterCollection Clusters
            {
                get { return _clusters; }
                set { _clusters = value; }
            }
            public Data3D ColorMask
            {
                get { return _colorMask; }
                set
                {
                    if(_colorMask!= value)
                    {
                        _colorMask = value;
                        NotifyPropertyChanged("ColorMask");
                    }
                }
            }

            public int NumberClusters
            {
                get { return _clusters.Count; }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            public void NotifyPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            private FoundClusters(int width, int height)
            {
                _width = width;
                _height = height;

                Clusters = new FoundClusterCollection();

                _clusters.CollectionChanged += _clusters_CollectionChanged;
            }
            private FoundClusters(int width, int height, FoundClusterCollection clusters)
            {
                _width = width;
                _height = height;

                _clusters = new FoundClusterCollection();
                _clusters.CollectionChanged += _clusters_CollectionChanged;

                foreach(Cluster c in clusters)
                {
                    Clusters.Add(c);
                }
            }

            public static FoundClusters FindClusters(bool[,] matrix, CancellationTokenSource token, int minPixelArea = 50, int intraparticleDistance = 10)
            {
                int width = matrix.GetLength(0);
                int height = matrix.GetLength(1);

                int numberPixels = 0;

                List<int> xx = new List<int>();
                List<int> yy = new List<int>();

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (matrix[x, y])
                        {
                            xx.Add(x);
                            yy.Add(y);
                            numberPixels++;
                        }
                    }
                }

                int[] clusters = new int[numberPixels];
                bool[] pixelsToCheck = new bool[numberPixels];
                int[] pixelList = new int[numberPixels];

                for (int i = 0; i < numberPixels; i++)
                {
                    clusters[i] = i;
                    pixelsToCheck[i] = true;
                    pixelList[i] = i;
                }

                int counter = 0;

                List<Cluster> foundClusters = new List<Cluster>();

                for (int i = 0; i < numberPixels; i++)
                {
                    if (pixelsToCheck[i])
                    {
                        if (token.IsCancellationRequested)
                            throw new OperationCanceledException("Operation cancelled by user.");

                        pixelsToCheck[i] = false;
                        counter++;

                        ClusterPoint iCoords = new ClusterPoint(xx[i], yy[i]);

                        List<ClusterPoint> pixelsInCluster = new List<ClusterPoint>();
                        List<int> indiciesInCluster = new List<int>();

                        pixelsInCluster.Add(iCoords);

                        for (int j = 0; j < numberPixels; j++)
                        {
                            if (!pixelsToCheck[j]) continue;

                            if (token.IsCancellationRequested)
                                throw new OperationCanceledException("Operation cancelled by user.");

                            ClusterPoint jCoords = new ClusterPoint(xx[j], yy[j]);
                            foreach (ClusterPoint pixel in pixelsInCluster)
                            {
                                if (pixel.Equals(jCoords)) continue;

                                if (addToCluster(jCoords, pixel, intraparticleDistance))
                                {
                                    pixelsInCluster.Add(jCoords);
                                    pixelsToCheck[j] = false;
                                    indiciesInCluster.Add(j);
                                    counter++;
                                    break;
                                }
                            }
                        }

                        if (pixelsInCluster.Count >= minPixelArea)
                        {
                            foundClusters.Add(new Cluster(pixelsInCluster));
                        }

                        else
                        {
                            // Reset those pixels not deemed in because of condition to be able
                            // to be re-evaluated
                            foreach (int index in indiciesInCluster)
                            {
                                pixelsToCheck[index] = true;
                            }
                        }
                    }
                }

                // Assign a color for visualization and the cluster number to finalize the results collection
                FoundClusterCollection finalizedClusters = new FoundClusterCollection();
                for (int i = 0; i < foundClusters.Count; i++)
                {
                    //foundClusters[i].ClusterNumber = (i + 1);
                    //foundClusters[i].Color = ClusterColorGenerator.getColor(i);
                    finalizedClusters.Add(foundClusters[i]);
                }
                return new FoundClusters(width, height, finalizedClusters);
            }

            private static bool addToCluster(ClusterPoint point1, ClusterPoint point2, int intraparticleDistance)
            {
                int dx = point2.X - point1.X;
                int dy = point2.Y - point1.Y;

                double r = Math.Sqrt((dx * dx) + (dy * dy));
                return r <= intraparticleDistance;
            }

            public static Task<FoundClusters> FindClustersAsync(bool[,] matrix, CancellationTokenSource token, int minPixelArea = 50, int interparticleDistance = 10)
            {
                return Task.Run(() => FindClusters(matrix, token, minPixelArea, interparticleDistance));
            }

            public bool[,] MaskArray
            {
                get
                {
                    bool[,] mask = new bool[_width, _height];
                    foreach (Cluster cluster in _clusters)
                    {
                        foreach (ClusterPoint point in cluster.Pixels)
                        {
                            int x = point.X;
                            int y = point.Y;

                            if (x >= _width || y >= _height)
                                continue;

                            mask[x, y] = true;
                        }
                    }

                    return mask;
                }
            }
            public void SaveTextArray(string filePath)
            {
                using (Stream stream = File.Open(filePath, FileMode.Create))
                {
                    StreamWriter sw = new StreamWriter(stream);

                    bool[,] array = MaskArray;

                    // Writes array size
                    sw.Write(array.GetLength(0).ToString() + "\t");

                    for (int x = 0; x < array.GetLength(0); x++)
                    {
                        for (int y = 0; y < array.GetLength(1); y++)
                        {
                            sw.Write((array[x, y] ? 1 : 0) + "\t");
                        }
                    }
                }
            }
            public void SaveBinaryArray(string filePath)
            {
                int[,] mask = new int[_width, _height];

                // Creates an array for each pixel in the image and assigns the cluster number
                // at each pixel. 0 indicates no cluster found at that pixel. Values greater
                // than 0 indicate the identity of the cluster that pixel belongs to.
                foreach (Cluster cluster in Clusters)
                {
                    foreach (ClusterPoint point in cluster.Pixels)
                    {
                        mask[point.X, point.Y] = cluster.ClusterNumber;
                    }
                }

                using (Stream stream = File.Open(filePath, FileMode.Create))
                {
                    BinaryWriter bw = new BinaryWriter(stream);

                    // Write the pixel dimension then the number of clusters to expect. Finally,
                    // the mask created earlier is written.
                    bw.Write((int)Math.Max(_width, _height));
                    bw.Write(Clusters.Count);
                    
                    for (int x = 0; x < _width; x++)
                    {
                        for (int y = 0; y < _height; y++)
                        {
                            bw.Write(mask[x, y]);
                        }
                    }
                }
            }
            public List<CountedClusterStatistic> GenerateStats(Data2D data)
            {
                if (_width != data.Width || _height != data.Height)
                    throw new ArgumentException("Invalid data dimensions.");

                List<CountedClusterStatistic> stats = new List<CountedClusterStatistic>();

                List<CountedCluster> countedClusters = new List<CountedCluster>();
                foreach (Cluster cluster in _clusters)
                {
                    countedClusters.Add(new CountedCluster(cluster, data));
                }

                int i = 0;
                foreach (CountedCluster cluster in countedClusters)
                {
                    CountedClusterStatistic stat = new CountedClusterStatistic()
                    {
                        ClusterNumber = ++i,
                        CenterX = cluster.Center.X,
                        CenterY = cluster.Center.Y,
                        Minimum = cluster.Min,
                        Maximum = cluster.Max,
                        Average = cluster.Average,
                        StdDev = cluster.StdDev,
                        NumberPixels = cluster.NumberPixels,
                        TotalCounts = cluster.Sum
                    };

                    stats.Add(stat);
                }

                return stats;
            }
            public static float WeightedAverage(List<CountedClusterStatistic> clusterStats)
            {
                float numerator = 0;
                float totalPixels = 0;
                for (int i = 0; i < clusterStats.Count; i++)
                {
                    numerator += (float)clusterStats[i].TotalCounts;
                    totalPixels += (float)clusterStats[i].NumberPixels;
                }

                return numerator / totalPixels;
            }
            public static float WeightedStdDev(List<CountedClusterStatistic> clusterStats)
            {
                float wAvg = WeightedAverage(clusterStats);

                float numerator = 0;
                float denominator = 0;
                float numberNonZero = 0;
                for (int i = 0; i < clusterStats.Count; i++)
                {
                    numerator += (float)clusterStats[i].NumberPixels * (float)Math.Pow((clusterStats[i].Average - wAvg), 2);
                    denominator += (float)clusterStats[i].NumberPixels;

                    if (clusterStats[i].NumberPixels != 0) numberNonZero++;
                }

                return (float)Math.Sqrt(numerator / (((numberNonZero - 1) / numberNonZero) * denominator));
            }
            public static float WeightedStdDev(List<CountedClusterStatistic> clusterStats, float wAvg)
            {

                float numerator = 0;
                float denominator = 0;
                float numberNonZero = 0;
                for (int i = 0; i < clusterStats.Count; i++)
                {
                    numerator += (float)clusterStats[i].NumberPixels * (float)Math.Pow((clusterStats[i].Average - wAvg), 2);
                    denominator += (float)clusterStats[i].NumberPixels;

                    if (clusterStats[i].NumberPixels != 0) numberNonZero++;
                }

                return (float)Math.Sqrt(numerator / (((numberNonZero - 1) / numberNonZero) * denominator));
            }

            public string GenerateStatistics(Data2D data)
            {
                List<CountedCluster> countedClusters = new List<CountedCluster>();
                foreach (Cluster cluster in _clusters)
                {
                    countedClusters.Add(new CountedCluster(cluster, data));
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("Cluster\tCenter\tMin\tMax\tAverage\tStdDev\tPixels\n");

                int counter = 0;
                foreach (CountedCluster cluster in countedClusters)
                {
                    ClusterPoint center = cluster.Center;
                    sb.AppendFormat("{0}\t({1}, {2})\t{3}\t{4}\t{5}\t{6}\t{7}\n", 
                        ++counter, center.X, center.Y, cluster.Min, cluster.Max, 
                        cluster.Average, cluster.StdDev, cluster.NumberPixels);
                }

                sb.Append("\n\n");
                sb.AppendFormat("Weighted Average: {0}\n", weightedAverage(countedClusters));
                sb.AppendFormat("Weighted StdDev: {0}\n", weightedStdDev(countedClusters));

                return sb.ToString();
            }
            public Task<string> GenerateStatisticsAsync(Data2D data)
            {
                return Task<string>.Run(() => GenerateStatistics(data));
            }
            private float weightedAverage(List<CountedCluster> countedClusters)
            {
                float numerator = 0;
                float totalPixels = 0;
                for (int i = 0; i < countedClusters.Count; i++)
                {
                    numerator += (float)countedClusters[i].Sum;
                    totalPixels += (float)countedClusters[i].NumberPixels;
                }

                return numerator / totalPixels;
            }
            private float weightedStdDev(List<CountedCluster> countedClusters)
            {
                float wAvg = weightedAverage(countedClusters);

                float numerator = 0;
                float denominator = 0;
                float numberNonZero = 0;
                for (int i = 0; i < countedClusters.Count; i++)
                {
                    numerator += (float)countedClusters[i].NumberPixels * (float)Math.Pow((countedClusters[i].Average - wAvg), 2);
                    denominator += (float)countedClusters[i].NumberPixels;

                    if (countedClusters[i].NumberPixels != 0) numberNonZero++;
                }

                return (float)Math.Sqrt(numerator / (((numberNonZero - 1) / numberNonZero) * denominator));
            }

            private void _clusters_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                UpdateColorMask();
            }
            private void UpdateColorMask()
            {
                Data3D mask = Data3D.BlankColorData(_width, _height);

                foreach (Cluster cluster in _clusters)
                {
                    float[] clusterColor = cluster.Color.ToFloatArray();

                    foreach (ClusterPoint point in cluster.Pixels)
                    {
                        int x = point.X;
                        int y = point.Y;

                        if (x >= _width || y >= _height)
                            continue;

                        mask[x, y] = clusterColor;
                    }
                }

                ColorMask = mask;
            }

            public Cluster FindClusterByPoint(int x, int y)
            {
                if (Clusters.Count == 0)
                    return null;

                if (x < 0 || y < 0 ||
                    x > _width || y > _height)
                    return null;

                foreach(Cluster c in Clusters)
                {
                    if (c.Pixels.Contains(new ClusterPoint(x, y)))
                        return c;
                }

                return null;
            }

            public static FoundClusters Empty
            {
                get { return new FoundClusters(0, 0); }
            }
        }

        public class FoundClusterCollection : ObservableCollection<Cluster>
        {
            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    this[i].ClusterNumber = (i + 1);
                    this[i].Color = ClusterColorGenerator.getColor(i);
                }

                base.OnCollectionChanged(e);
            }
        }

        internal struct PixelValue
        {
            internal int X;
            internal int Y;
            internal int Counts;
        }
        internal class CountedCluster
        {
            List<PixelValue> _pixelValues;

            internal int NumberPixels
            {
                get { return _pixelValues.Count; }
            }
            internal int Max
            {
                get
                {
                    int max = int.MinValue;
                    foreach (PixelValue pixel in _pixelValues)
                    {
                        max = Math.Max(max, pixel.Counts);
                    }
                    return max;
                }
            }
            internal int Min
            {
                get
                {
                    int min = int.MaxValue;
                    foreach (PixelValue pixel in _pixelValues)
                    {
                        min = Math.Min(min, pixel.Counts);
                    }
                    return min;
                }
            }
            internal int Sum
            {
                get
                {
                    int sum = 0;
                    foreach (PixelValue pixel in _pixelValues)
                    {
                        sum += pixel.Counts;
                    }
                    return sum;
                }
            }
            internal float Average
            {
                get
                {
                    float sum = (float)Sum;
                    float num = (float)NumberPixels;
                    return sum / num;
                }
            }
            internal float StdDev
            {
                get
                {
                    float average = Average;
                    double sum = 0;
                    foreach (PixelValue pixel in _pixelValues)
                    {
                        sum += Math.Pow((pixel.Counts - average), 2);
                    }

                    double variance = sum / (double)NumberPixels;

                    return (float)Math.Sqrt(variance);
                }
            }
            internal ClusterPoint Center
            {
                get
                {
                    float xSum = 0;
                    float ySum = 0;

                    foreach (PixelValue pixel in _pixelValues)
                    {
                        xSum += pixel.X;
                        ySum += pixel.Y;
                    }

                    return new ClusterPoint()
                    {
                        X = (int)(xSum / NumberPixels),
                        Y = (int)(ySum / NumberPixels)
                    };
                }
            }

            internal CountedCluster(Cluster cluster, Data2D data)
            {
                _pixelValues = new List<PixelValue>();

                foreach (ClusterPoint point in cluster.Pixels)
                {
                    int x = point.X;
                    int y = point.Y;

                    if (x >= data.Width || y >= data.Height)
                        throw new ArgumentException("Point does not fall within the data set.");

                    _pixelValues.Add(new PixelValue()
                    {
                        X = x,
                        Y = y,
                        Counts = (int)data[x, y]
                    });
                }
            }
        }

        public class CountedClusterStatistic
        {
            public int ClusterNumber { get; set; }
            public int CenterX { get; set; }
            public int CenterY { get; set; }
            public int NumberPixels { get; set; }
            public int Minimum { get; set; }
            public int Maximum { get; set; }
            public int TotalCounts { get; set; }
            public float Average { get; set; }
            public float StdDev { get; set; }
        }

        internal static class ClusterColorGenerator
        {
            private static Color[] _colors = {
                                                 Color.FromArgb(255, 141, 211, 199),
                                                 Color.FromArgb(255, 255, 255, 179),
                                                 Color.FromArgb(255, 190, 186, 218),
                                                 Color.FromArgb(255, 251, 128, 114),
                                                 Color.FromArgb(255, 128, 177, 211),
                                                 Color.FromArgb(255, 253, 180, 98),
                                                 Color.FromArgb(255, 179, 222, 105),
                                                 Color.FromArgb(255, 252, 205, 229),
                                                 Color.FromArgb(255, 217, 217, 217),
                                                 Color.FromArgb(255, 188, 128, 189),
                                                 Color.FromArgb(255, 204, 235, 197),
                                                 Color.FromArgb(255, 255, 237, 111),
                                             };
            private static int _colorArraySize = _colors.Length;

            internal static Color getColor(int number)
            {
                return _colors[number % _colorArraySize];
            }
        }
    }
}
