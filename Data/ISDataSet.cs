using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using ImagingSIMS.Data.Spectra;

namespace ImagingSIMS.Data
{
    public class FlatArray<T>
    {
        T[] _array;
        int[] _sizes;

        public T this[int x, int y]
        {
            get { return _array[(x * _sizes[1]) + y]; }
            set { _array[(x * _sizes[1]) + y] = value; }
        }

        public FlatArray(int Width, int Height)
        {
            _sizes = new int[2] { Width, Height };

            _array = new T[_sizes[0] * _sizes[1]];
        }
        public FlatArray(T[,] Matrix)
        {
            int width = Matrix.GetLength(0);
            int height = Matrix.GetLength(1);

            _sizes = new int[2] { width, height };

            _array = new T[_sizes[0] * _sizes[1]];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    this[x, y] = Matrix[x, y];
                }
            }
        }

        public int GetLength(int Dimension)
        {
            return _sizes[Dimension];
        }

        public T[,] ToArray()
        {
            T[,] array = new T[_sizes[0], _sizes[1]];
            for (int x = 0; x < _sizes[0]; x++)
            {
                for (int y = 0; y < _sizes[1]; y++)
                {
                    array[x, y] = this[x, y];
                }
            }
            return array;
        }

        public override bool Equals(object obj)
        {
            FlatArray<T> f = obj as FlatArray<T>;
            if (f == null) return false;

            var sizesEqual = Enumerable.SequenceEqual(f._sizes, _sizes);
            var arraysEqual = Enumerable.SequenceEqual(f._array, _array);
            return sizesEqual && arraysEqual;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public sealed class Data2D : Data, ISavable
    {
        int _uniqueID;
        public int UniqueID
        {
            get { return _uniqueID; }
            set { _uniqueID = value; }
        }

        static readonly char[] _delimiters = new char[] { ',', '\t' };
        FlatArray<float> _matrix;
        float _max = float.MinValue;
        float _min = float.MaxValue;

        public float this[int x, int y]
        {
            get { return _matrix[x, y]; }
            set
            {
                _matrix[x, y] = value;
                if (value > _max) _max = value;
                if (value < _min) _min = value;
            }
        }
        public float Maximum
        {
            get { return _max; }
        }
        public float Minimum
        {
            get
            {
                return _min;
            }
        }

        public string DataName
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged("DataName");
                }
            }
        }
        public int Width
        {
            get { return _matrix.GetLength(0); }
        }
        public int Height
        {
            get { return _matrix.GetLength(1); }
        }

        public double TotalCounts
        {
            get
            {
                if (_matrix == null) return 0;

                double counts = 0;
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        counts += _matrix[x, y];
                    }
                }

                return counts;
            }
        }

        float _mean = float.NaN;
        public float Mean
        {
            get
            {
                if (float.IsNaN(_mean))
                {
                    _mean = (float)(TotalCounts / (double)(Width * Height));
                }
                return _mean;
            }
        }
        public float StdDev
        {
            get
            {
                float mean = Mean;
                float s = 0;

                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        s += ((_matrix[x, y] - mean) * (_matrix[x, y] - mean));
                    }
                }

                return (float)Math.Sqrt(s / (double)(Width * Height));
            }
        }

        List<float[]> _nonSparseMatrix;
        public List<float[]> NonSparseMatrix
        {
            get
            {
                if (_nonSparseMatrix == null)
                {
                    _nonSparseMatrix = getNonSparseMatrix();
                }

                return _nonSparseMatrix;
            }

        }
        float _nonSparseMean = float.NaN;
        public float NonSparseMean
        {
            get
            {
                if (float.IsNaN(_nonSparseMean))
                {
                    float sum = 0;
                    foreach (float[] pixel in NonSparseMatrix)
                    {
                        sum += pixel[2];
                    }
                    _nonSparseMean = sum / NonSparseMatrix.Count;
                }

                return _nonSparseMean;
            }
        }
        public float NonSparseStdDev
        {
            get
            {
                float mean = NonSparseMean;
                float s = 0;

                foreach (float[] pixel in NonSparseMatrix)
                {
                    s += ((pixel[2] - mean) * (pixel[2] - mean));
                }

                return (float)Math.Sqrt(s / NonSparseMatrix.Count);
            }
        }

        private List<float[]> getNonSparseMatrix()
        {
            List<float[]> nonSparse = new List<float[]>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (this[x, y] != 0) nonSparse.Add(new float[] { x, y, this[x, y] });
                }
            }
            return nonSparse;
        }

        public static Data2D Empty
        {
            get
            {
                return new Data2D(0, 0)
                {
                    DataName = "Empty"
                };
            }
        }

        public Data2D()
        {
            _matrix = new FlatArray<float>(1, 1);
        }
        public Data2D(int Width, int Height, float DefaultValue)
        {
            _matrix = new FlatArray<float>(Width, Height);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _matrix[x, y] = DefaultValue;
                }
            }
            _max = DefaultValue;
        }
        public Data2D(int Width, int Height)
        {
            _matrix = new FlatArray<float>(Width, Height);
        }
        public Data2D(string LoadPath, FileType FileType)
        {
            try
            {
                switch (FileType)
                {
                    case FileType.BioToF:
                        LoadBioToF(LoadPath);
                        break;
                    case FileType.J105:
                        LoadJ105(LoadPath);
                        break;
                    case FileType.QStar:
                        throw new ArgumentException("QStar data is not supported");
                }
            }
            catch (IOException IOex)
            {
                throw new IOException(FileType.ToString(), IOex);
            }
        }
        public Data2D(FlatArray<float> Data)
        {
            _matrix = Data;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (_matrix[x, y] > _max) _max = _matrix[x, y];
                }
            }
        }
        public Data2D(float[,] Data)
        {
            _matrix = new FlatArray<float>(Data);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (_matrix[x, y] > _max) _max = _matrix[x, y];
                }
            }
        }
        public Data2D(DataTable Data)
        {
            _matrix = new FlatArray<float>(Data.Columns.Count, Data.Rows.Count);
            for (int x = 0; x < Data.Columns.Count; x++)
            {
                for (int y = 0; y < Data.Rows.Count; y++)
                {
                    _matrix[x, y] = float.Parse(Data.Rows[y][x].ToString());
                    if (_matrix[x, y] > _max) _max = _matrix[x, y];
                }
            }
        }

        private void LoadBioToF(string LoadPath)
        {
            try
            {
                int[] dimensions = BioToFMatrixSize(LoadPath);

                _matrix = new FlatArray<float>(dimensions[0], dimensions[1]);
                using (StreamReader sr = new StreamReader(LoadPath))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        char delimiter = ' ';
                        string[] lineDelim = line.Split(delimiter);

                        int xCoord = int.Parse(lineDelim[0]);
                        int yCoord = int.Parse(lineDelim[1]);

                        int coord;
                        if (int.TryParse(lineDelim[2], out coord))
                        {
                            _matrix[xCoord, yCoord] = coord;
                            if (_matrix[xCoord, yCoord] > _max) _max = _matrix[xCoord, yCoord];
                        }
                        else
                        {
                            throw new ArgumentException(string.Format("The data file {0} being imported does not match a compatible file type.",
                                LoadPath));
                        }
                    }
                }

                DataName = Path.GetFileNameWithoutExtension(LoadPath);
            }
            catch (FormatException)
            {
                throw new IOException("The input file was an incorrect format.");
            }
        }
        private void LoadJ105(string LoadPath)
        {
            int ct = 0;
            try
            {
                int[] dimensions = J105MatrixSize(LoadPath);

                int w = dimensions[0];
                int h = dimensions[1];

                _matrix = new FlatArray<float>(w, h);

                using (StreamReader sr = new StreamReader(LoadPath))
                {
                    int x = 0;
                    int y = 0;

                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        char delimiter = ',';
                        string[] lineDelim = line.Split(delimiter);

                        foreach (string st in lineDelim)
                        {
                            _matrix[x, y] = (int)double.Parse(st);
                            if (x != w - 1 && y != h - 1)
                            {
                                if (_matrix[x, y] > _max) _max = _matrix[x, y];
                            }
                            if (_matrix[x, y] > 0) ct++;
                            x++;
                        }
                        y++;
                        x = 0;
                    }
                }

                _matrix[w - 1, h - 1] = 0;

                DataName = Path.GetFileNameWithoutExtension(LoadPath);
            }
            catch (FormatException)
            {
                throw new IOException("The input file was an incorrect format.");
            }
        }
        private int[] J105MatrixSize(string Path)
        {
            int ctX = 0;
            int ctY = 0;

            using (StreamReader sr = new StreamReader(Path))
            {
                string line = sr.ReadLine();
                foreach (char c in line)
                {
                    if (c == ',') ctX++;
                }
                ctX++;
                ctY++;

                while (!sr.EndOfStream)
                {
                    sr.ReadLine();
                    ctY++;
                }
            }

            int[] dimensions = new int[2] { ctX, ctY };
            return dimensions;
        }
        private int[] BioToFMatrixSize(string Path)
        {
            int ctX = 0;
            int ctY = 0;

            using (StreamReader sr = new StreamReader(Path))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] lineDelim = new string[3];
                    char delimiter = ' ';
                    lineDelim = line.Split(delimiter);

                    int xCoord = int.Parse(lineDelim[0]);
                    int yCoord = int.Parse(lineDelim[1]);

                    if (xCoord > ctX) ctX = xCoord;
                    if (yCoord > ctY) ctY = yCoord;
                }
            }

            int[] dimensions = new int[2] { ctX + 1, ctY + 1 };
            return dimensions;
        }

        public DataTable ToDataTable()
        {
            DataTable dt = new DataTable();
            dt.TableName = _name;
            for (int x = 0; x < Width; x++)
            {
                DataColumn dc = new DataColumn();
                dt.Columns.Add(dc);
            }
            for (int y = 0; y < Height; y++)
            {
                DataRow dr = dt.NewRow();
                for (int x = 0; x < Width; x++)
                {
                    dr[x] = _matrix[x, y];
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
        public DataTable ToDataTable(string TableName)
        {
            DataTable dt = new DataTable();
            dt.TableName = TableName;
            for (int x = 0; x < Width; x++)
            {
                DataColumn dc = new DataColumn();
                dt.Columns.Add(dc);
            }
            for (int y = 0; y < Height; y++)
            {
                DataRow dr = dt.NewRow();
                for (int x = 0; x < Width; x++)
                {
                    dr[x] = _matrix[x, y];
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        public override string ToString()
        {
            return string.Format("{0} W:{1} H:{2}", _name, Width, Height);
        }
        public override bool Equals(object obj)
        {
            Data2D d = obj as Data2D;
            if (d == null) return false;

            return d.DataName == DataName && d._matrix.Equals(_matrix);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = 31 * hash + _name.GetHashCode();
                hash = 31 * hash + _matrix.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Normalize the data to the maximum value so that all values are in the range of 0f to 1f.
        /// </summary>
        public void Normalize()
        {
            float max = Maximum;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _matrix[x, y] /= max;
                }
            }
            Refresh();
        }
        /// <summary>
        /// Normalize the data to the maximum value so that all values are in the range of 0f to 1f.
        /// </summary>
        /// <param name="OriginalMaximum">The maximum value for which all data points were normalized to.</param>
        public void Normalize(out float OriginalMaximum)
        {
            float max = Maximum;
            OriginalMaximum = max;

            if (max == 0) return;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _matrix[x, y] /= max;
                }
            }
            Refresh();
        }

        /// <summary>
        /// Determines the maximum value in the matrix and updates the Maximum property. Useful after method operations 
        /// were performed on the data set which do not necessarily update the Maximum property.
        /// </summary>
        public void Refresh()
        {
            float max = -1;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (_matrix[x, y] > max) max = _matrix[x, y];
                }
            }
            _max = max;
            _mean = Mean;

            _nonSparseMatrix = getNonSparseMatrix();

            float sum = 0;
            for (int i = 0; i < _nonSparseMatrix.Count; i++)
            {
                sum += _nonSparseMatrix[i][2];
            }
            _nonSparseMean = sum / _nonSparseMatrix.Count;
        }

        public Data2D Resize(int HighX, int HighY)
        {
            int highX = HighX;
            int highY = HighY;
            int lowX = this.Width;
            int lowY = this.Height;

            Data2D Resized = new Data2D(highX, highY);
            Resized.DataName = this.DataName;

            float A, B, C, D;
            int a, b;
            float xRatio = ((float)lowX - 1f) / (float)highX;
            float yRatio = ((float)lowY - 1f) / (float)highY;
            float xDiff, yDiff;

            for (int i = 0; i < highX; i++)
            {
                for (int j = 0; j < highY; j++)
                {
                    a = (int)(xRatio * i);
                    b = (int)(yRatio * j);
                    xDiff = (xRatio * i) - a;
                    yDiff = (yRatio * j) - b;

                    int x1 = a + 1;
                    if (x1 >= highX) x1 = a;
                    int y1 = b + 1;
                    if (y1 >= highY) y1 = b;

                    A = this[a, b];
                    B = this[a + 1, b];
                    C = this[a, b + 1];
                    D = this[a + 1, b + 1];

                    Resized[i, j] = (A * (1f - xDiff) * (1f - yDiff)) + (B * (xDiff) * (1f - yDiff)) +
                        (C * (yDiff) * (1f - xDiff)) + (D * (xDiff) * (yDiff));
                }
            }

            return Resized;
        }

        public static Data2D Sum(List<Data2D> Data)
        {
            int sizeX = 0;
            int sizeY = 0;
            int sizeZ = Data.Count;
            foreach (Data2D d in Data)
            {
                if (d.Width > sizeX) sizeX = d.Width;
                if (d.Height > sizeY) sizeY = d.Height;
            }

            float[,] summed = new float[sizeX, sizeY];
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    for (int z = 0; z < sizeZ; z++)
                    {
                        summed[x, y] += Data[z][x, y];
                    }
                }
            }

            return new Data2D(summed);
        }
        public static Data2D Sum(Data2D[] Data)
        {
            int sizeX = 0;
            int sizeY = 0;
            int sizeZ = Data.Length;
            foreach (Data2D d in Data)
            {
                if (d.Width > sizeX) sizeX = d.Width;
                if (d.Height > sizeY) sizeY = d.Height;
            }

            float[,] summed = new float[sizeX, sizeY];
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    for (int z = 0; z < sizeZ; z++)
                    {
                        summed[x, y] += Data[z][x, y];
                    }
                }
            }

            return new Data2D(summed);
        }

        public void Save(string SavePath, FileType FileType)
        {
            try
            {
                switch (FileType)
                {
                    case FileType.BioToF:
                        SaveBioToF(SavePath);
                        break;
                    case FileType.CSV:
                        SaveCSV(SavePath);
                        break;
                    case FileType.J105:
                        SaveJ105(SavePath);
                        break;
                    case FileType.QStar:
                        throw new NotSupportedException("QStar data is not supported.");
                    default:
                        SaveCSV(SavePath);
                        break;
                }
            }
            catch (IOException IOex)
            {
                throw new IOException("Error saving file as J105 Type", IOex);
            }
        }
        private void SaveJ105(string Path)
        {
            using (StreamWriter sw = new StreamWriter(Path))
            {
                for (int y = 0; y < Height; y++)
                {
                    string line = "";
                    for (int x = 0; x < Width; x++)
                    {
                        line += _matrix[x, y].ToString("0.0") + ",";
                    }
                    line = line.Remove(line.Length - 1);
                    sw.WriteLine(line);
                }
            }
        }
        private void SaveBioToF(string Path)
        {
            using (StreamWriter sw = new StreamWriter(Path))
            {
                for (int y = 0; y < Height; y++)
                {
                    string line = "";
                    for (int x = 0; x < Width; x++)
                    {
                        //line += _matrix[x, y].ToString("0.0") + ",";
                        line += string.Format("{0} {1} {2}", x.ToString("000"), y.ToString("000"), _matrix[x, y].ToString("0."));
                    }
                    line = line.Remove(line.Length - 1);
                    sw.WriteLine(line);
                }
            }
        }
        private void SaveCSV(string Path)
        {
            using (StreamWriter sw = new StreamWriter(Path))
            {
                for (int y = 0; y < Height; y++)
                {
                    string line = "";
                    for (int x = 0; x < Width; x++)
                    {
                        line += _matrix[x, y].ToString() + ",";
                    }
                    line = line.Remove(line.Length - 1);
                    sw.WriteLine(line);
                }
            }
        }

        public float GetMaximum(out List<Point> Locations)
        {
            List<Point> locations = new List<Point>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (_matrix[x, y] == _max) locations.Add(new Point(x, y));
                }
            }

            Locations = locations;
            return _max;
        }

        public Data2D Crop(int StartX, int StartY, int LengthX = -1, int LengthY = -1)
        {
            int cropLength = LengthX;
            int cropHeight = LengthY;
            if (LengthX == -1) cropLength = Width - StartX - 1;
            if (LengthY == -1) cropHeight = Height - StartY - 1;

            if (StartX + cropLength >= Width || StartY + cropHeight >= Height)
                throw new ArgumentException("Invalid dimensions.");

            Data2D d = new Data2D(cropLength, cropHeight);

            for (int x = 0; x < cropLength; x++)
            {
                for (int y = 0; y < cropHeight; y++)
                {
                    d[x, y] = this[x + StartX, y + StartY];
                }
            }

            return d;
        }
        public Data2D Crop(Rect Rectangle)
        {
            int cropLength = (int)Rectangle.Width;
            int cropHeight = (int)Rectangle.Height;
            int startX = (int)Rectangle.X;
            int startY = (int)Rectangle.Y;

            if (startX + cropLength >= Width || startY + cropHeight >= Height)
                throw new ArgumentException("Invalid dimensions.");

            Data2D d = new Data2D(cropLength, cropHeight);

            for (int x = 0; x < cropLength; x++)
            {
                for (int y = 0; y < cropHeight; y++)
                {
                    d[x, y] = this[x + startX, y + startY];
                }
            }

            return d;
        }

        public void FlipHorizontal()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width / 2; x++)
                {
                    float temp = _matrix[x, y];
                    _matrix[x, y] = _matrix[Width - 1 - x, y];
                    _matrix[Width - 1 - x, y] = temp;
                }
            }
        }
        public void FlipVertical()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height / 2; y++)
                {
                    float temp = _matrix[x, y];
                    _matrix[x, y] = _matrix[x, Height - 1 - y];
                    _matrix[x, Height - 1 - y] = temp;
                }
            }
        }

        public static async Task<Data2D> LoadData2DAsync(string LoadPath, FileType FileType)
        {
            try
            {
                //List of text lines in .txt file
                List<string> lines = new List<string>();

                //Populate lines List with file data async
                using (StreamReader sr = new StreamReader(LoadPath))
                {
                    while (!sr.EndOfStream)
                    {
                        lines.Add(await sr.ReadLineAsync());
                    }
                }

                //try
                //{
                Data2D d = new Data2D();
                switch (FileType)
                {
                    case FileType.BioToF:
                        d = FromBioToF(lines);
                        break;
                    case FileType.J105:
                        d = FromJ105(lines);
                        break;
                    case FileType.QStar:
                        throw new ArgumentException("QStar data is not supported");
                    case FileType.CSV:
                        d = FromCSV(lines);
                        break;
                }

                //Set Data2D name
                d.DataName = Path.GetFileNameWithoutExtension(LoadPath);
                return d;
                //}
                //catch (Exception ex)
                //{
                //    throw ex;
                //}
            }
            catch (FormatException)
            {
                throw new IOException("The input file was an incorrect format.");
            }
        }
        private static Data2D FromBioToF(List<string> FileLines)
        {
            //Get matrix dimensions

            int w = 0;
            int h = 0;

            foreach (string line in FileLines)
            {
                string[] lineDelim = new string[3];
                char delimiter = ' ';
                lineDelim = line.Split(delimiter);

                int xCoord = int.Parse(lineDelim[0]);
                int yCoord = int.Parse(lineDelim[1]);

                if (xCoord > w) w = xCoord;
                if (yCoord > h) h = yCoord;
            }

            //Max values are zero-based indices, so to get width and height need to add one.
            w = w + 1;
            h = h + 1;

            //Create new data object
            Data2D d = new Data2D(w, h);

            //Populate Data2D with data from file
            foreach (string line in FileLines)
            {
                char delimiter = ' ';
                string[] lineDelim = line.Split(delimiter);

                int xCoord = int.Parse(lineDelim[0]);
                int yCoord = int.Parse(lineDelim[1]);

                int coord;
                if (int.TryParse(lineDelim[2], out coord))
                {
                    d[xCoord, yCoord] = coord;
                }
                else
                {
                    throw new ArgumentException(string.Format("The data file being imported does not match a compatible file type."));
                }
            }

            return d;
        }
        private static Data2D FromJ105(List<string> FileLines)
        {
            //Get matrix dimensions
            int w = 0;
            int h = 0;

            string firstLine = FileLines[0];

            foreach (char c in firstLine)
            {
                //if (c == ',') w++;
                if (_delimiters.Contains(c)) w++;
            }

            //Last value in line does not have comma, so add one to account and get actual width
            w = w + 1;

            //Height is just the number of lines in file
            h = FileLines.Count;

            //Create Data2D object
            Data2D d = new Data2D(w, h);

            //Populate Data2D matrix with data from file
            int x = 0;
            int y = 0;
            foreach (string line in FileLines)
            {
                //char delimiter = ',';
                //string[] lineDelim = line.Split(delimiter);
                string[] lineDelim = line.Split(_delimiters);

                foreach (string st in lineDelim)
                {
                    //For J105 only: Last data point is sum of all other data points.
                    //Leads to improper data scaling during imaging, so skip assignment.
                    if (x == w - 1 && y == h - 1) continue;

                    d[x, y] = (int)double.Parse(st);
                    x++;
                }
                y++;
                x = 0;
            }

            //Return object
            return d;
        }
        private static Data2D FromCSV(List<string> FileLines)
        {
            //Get matrix dimensions
            int w = 0;
            int h = 0;

            string firstLine = FileLines[0];

            foreach (char c in firstLine)
            {
                if (c == ',') w++;
            }

            //Last value in line does not have comma, so add one to account and get actual width
            w = w + 1;

            //Height is just the number of lines in file
            h = FileLines.Count;

            //Create Data2D object
            Data2D d = new Data2D(w, h);

            //Populate Data2D matrix with data from file
            int x = 0;
            int y = 0;
            foreach (string line in FileLines)
            {
                char delimiter = ',';
                string[] lineDelim = line.Split(delimiter);

                foreach (string st in lineDelim)
                {
                    d[x, y] = float.Parse(st);
                    x++;
                }
                y++;
                x = 0;
            }

            //Return object
            return d;
        }
        public static explicit operator int[,] (Data2D d)
        {
            int[,] matrix = new int[d.Width, d.Height];
            for (int x = 0; x < d.Width; x++)
            {
                for (int y = 0; y < d.Height; y++)
                {
                    matrix[x, y] = (int)d[x, y];
                }
            }
            return matrix;
        }
        public static explicit operator float[,] (Data2D d)
        {
            float[,] matrix = new float[d.Width, d.Height];
            for (int x = 0; x < d.Width; x++)
            {
                for (int y = 0; y < d.Height; y++)
                {
                    matrix[x, y] = d[x, y];
                }
            }
            return matrix;
        }
        public static explicit operator double[,] (Data2D d)
        {
            double[,] matrix = new double[d.Width, d.Height];
            for (int x = 0; x < d.Width; x++)
            {
                for (int y = 0; y < d.Height; y++)
                {
                    matrix[x, y] = d[x, y];
                }
            }
            return matrix;
        }
        public static explicit operator Data2D(float[,] d)
        {
            return new Data2D(d);
        }
        public static explicit operator Data2D(double[,] d)
        {
            int width = d.GetLength(0);
            int height = d.GetLength(1);

            Data2D data = new Data2D(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    data[x, y] = (float)d[x, y];
                }
            }

            return data;
        }
        public static explicit operator Data2D(int[,] d)
        {
            int width = d.GetLength(0);
            int height = d.GetLength(1);

            Data2D data = new Data2D(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    data[x, y] = (float)d[x, y];
                }
            }

            return data;
        }
        public static explicit operator Data2D(bool[,] b)
        {
            int width = b.GetLength(0);
            int height = b.GetLength(1);

            Data2D data = new Data2D(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (b[x, y]) data[x, y] = 1;
                }
            }

            return data;
        }

        public static Data2D Sqrt(Data2D a)
        {
            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = (float)Math.Sqrt(a[x, y]);
                }
            }

            return d;
        }
        public static Data2D Exp(Data2D a)
        {
            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = (float)Math.Exp(a[x, y]);
                }
            }

            return d;
        }
        public static Data2D Squared(Data2D a)
        {
            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = a[x, y] * a[x, y];
                }
            }

            return d;
        }
        public static Data2D Abs(Data2D a)
        {
            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = Math.Abs(a[x, y]);
                }
            }

            return d;
        }
        public static Data2D OneOver(Data2D a)
        {
            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = 1f / a[x, y];
                }
            }

            return d;
        }
        public static Data2D Ones(int Width, int Height)
        {
            return new Data2D(Width, Height, 1f);
        }
        public static Data2D Zeros(int Width, int Height)
        {
            return new Data2D(Width, Height);
        }

        public static Data2D Rescale(Data2D d, float minimum, float maximum)
        {
            float dataMinimum = d.Minimum;
            float dataMaximum = d.Maximum;
            float dataRange = d.Maximum - d.Minimum;

            float range = maximum - minimum;

            Data2D r = new Data2D(d.Width, d.Height);

            for (int x = 0; x < d.Width; x++)
            {
                for (int y = 0; y < d.Height; y++)
                {
                    r[x, y] = (range * (d[x, y] - dataMinimum) / dataRange) + minimum;
                }
            }

            return r;
        }

        public float[] ToVector()
        {
            float[] vector = new float[Width * Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    vector[(x * Width) + Height] = _matrix[x, y];
                }
            }
            return vector;
        }

        public string GenerateStatistics()
        {
            StringBuilder sb = new StringBuilder();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    sb.AppendLine(this[x, y].ToString());
                }
            }

            return sb.ToString();
        }
        public Task<string> GenerateStatisticsAsync()
        {
            return Task<string>.Run(() => GenerateStatistics());
        }

        public string GenerateStatistics(Data2D mask)
        {
            if (mask == null)
                throw new ArgumentException("No mask specified.");

            if (this.Width != mask.Width || this.Height != mask.Height)
            {
                throw new ArgumentException("Invalid dimensions.");
            }

            StringBuilder sb = new StringBuilder();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (mask[x, y] > 0)
                    {
                        sb.AppendLine(this[x, y].ToString());
                    }
                }
            }

            return sb.ToString();
        }
        public Task<string> GenerateStatisticsAsync(Data2D mask)
        {
            return Task<string>.Run(() => GenerateStatistics(mask));
        }

        public Data2D ExpandIntensity(int windowSize = 5)
        {
            Data2D d = new Data2D(Width, Height);
            d.DataName = this.DataName;

            Data2D denominator = new Data2D(Width, Height);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (this[x, y] > 0)
                    {
                        int startX = x - (windowSize / 2);
                        int startY = y - (windowSize / 2);

                        for (int a = 0; a < windowSize; a++)
                        {
                            for (int b = 0; b < windowSize; b++)
                            {
                                int pixelX = startX + a;
                                int pixelY = startY + b;

                                if (pixelX < 0 || pixelX >= Width ||
                                    pixelY < 0 || pixelY >= Height) continue;

                                d[pixelX, pixelY] += this[x, y];
                                denominator[pixelX, pixelY]++;
                            }
                        }
                    }
                }
            }

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (denominator[x, y] != 0)
                    {
                        d[x, y] /= denominator[x, y];
                    }
                }
            }

            return d;
        }

        /// <summary>
        /// Resizes the array to the specified width and height. Can only be used 
        /// to increase the matrix dimensions.
        /// </summary>
        /// <param name="targetWidth">Width of the new matrix.</param>
        /// <param name="targetHeight">Height of the new matrix.</param>
        /// <param name="useBilinear">True for bilinear interpolation or false for bicubic interpolation.</param>
        /// <returns>Matrix resized to the specified dimensions.</returns>
        public Data2D Upscale(int targetWidth, int targetHeight, bool useBilinear = true)
        {
            int lowResSizeX = Width;
            int lowResSizeY = Height;
            int highResSizeX = targetWidth;
            int highResSizeY = targetHeight;

            if (lowResSizeX > highResSizeX || lowResSizeY > highResSizeY)
                throw new ArgumentException("Cannot downscale data using this function.");

            if (useBilinear)
                return UpscaleBilinear(lowResSizeX, lowResSizeY, highResSizeX, highResSizeY);
            else
                return UpscaleBicubic(lowResSizeX, lowResSizeY, highResSizeX, highResSizeY);
        }

        private Data2D UpscaleBilinear(int lowResSizeX, int lowResSizeY, int highResSizeX, int highResSizeY)
        {
            Data2D resized = new Data2D(highResSizeX, highResSizeY);

            float A, B, C, D;
            int x, y;
            float xRatio = (lowResSizeX - 1f) / highResSizeX;
            float yRatio = (lowResSizeY - 1f) / highResSizeY;
            float xDiff, yDiff;

            for (int i = 0; i < highResSizeX; i++)
            {
                for (int j = 0; j < highResSizeY; j++)
                {
                    x = (int)(xRatio * i);
                    y = (int)(yRatio * j);
                    xDiff = (xRatio * i) - x;
                    yDiff = (yRatio * j) - y;

                    int x1 = x + 1;
                    if (x1 >= highResSizeX) x1 = x;
                    int y1 = y + 1;
                    if (y1 >= highResSizeY) y1 = y;

                    A = _matrix[x, y];
                    B = _matrix[x + 1, y];
                    C = _matrix[x, y + 1];
                    D = _matrix[x + 1, y + 1];

                    resized[i, j] = (A * (1f - xDiff) * (1f - yDiff)) + (B * (xDiff) * (1f - yDiff)) +
                        (C * (yDiff) * (1f - xDiff)) + (D * (xDiff) * (yDiff));
                }
            }

            return resized;
        }
        private Data2D UpscaleBicubic(int lowResSizeX, int lowResSizeY, int highResSizeX, int highResSizeY)
        {
            Data2D resized = new Data2D(highResSizeX, highResSizeY);

            float xRatio = (lowResSizeX - 1f) / highResSizeX;
            float yRatio = (lowResSizeY - 1f) / highResSizeY;

            for (int i = 0; i < highResSizeX; i++)
            {
                for (int j = 0; j < highResSizeY; j++)
                {
                    int x = (int)(xRatio * i);
                    int y = (int)(yRatio * j);

                    int[] xCoords = new int[]
                    {
                        x, Math.Min(x + 1, x), Math.Min(x + 2, x), Math.Min(x + 3, x)
                    };
                    int[] yCoords = new int[]
                    {
                        y, Math.Min(y + 1, y), Math.Min(y + 2, y), Math.Min(y + 3, y)
                    };

                    float[] xv = new float[4];
                    for (int k = 0; k < 4; k++)
                    {
                        xv[k] = InterpolateCubic(_matrix[xCoords[0], yCoords[k]], 
                            _matrix[xCoords[1], yCoords[k]], _matrix[xCoords[2], yCoords[k]], _matrix[xCoords[3], yCoords[k]], xRatio);
                    }

                    resized[i, j] = InterpolateCubic(xv[0], xv[1], xv[2], xv[3], yRatio);
                }
            }
            
            return resized;
        }
        private float InterpolateCubic(float v0, float v1, float v2, float v3, float frac)
        {
            float a = (v3 - v2) - (v0 - v1);
            float b = (v0 - v1) - a;
            float c = v2 - v0;
            float d = v1;

            return d + frac * (c + frac * (b + frac * a));
        }

        #region Operator Overloads
        public static Data2D operator +(Data2D a, Data2D b)
        {
            if (a.Width != b.Width || a.Height != b.Height)
                throw new ArgumentException("Invalid matrix dimensions. Both matrices must be of size M x N.");

            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = a[x, y] + b[x, y];
                }
            }

            return d;
        }
        public static Data2D operator +(Data2D a, float s)
        {
            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = s + a[x, y];
                }
            }

            return d;
        }
        public static Data2D operator +(Data2D a, double s)
        {
            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = (float)s + a[x, y];
                }
            }

            return d;
        }
        public static Data2D operator -(Data2D a, Data2D b)
        {
            if (a.Width != b.Width || a.Height != b.Height)
                throw new ArgumentException("Invalid matrix dimensions. Both matrices must be of size M x N.");

            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = a[x, y] - b[x, y];
                }
            }

            return d;
        }
        public static Data2D operator -(Data2D a, float s)
        {
            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = s - a[x, y];
                }
            }

            return d;
        }
        public static Data2D operator -(Data2D a, double s)
        {
            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = (float)s - a[x, y];
                }
            }

            return d;
        }
        public static Data2D operator -(float s, Data2D a)
        {
            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = s - a[x, y];
                }
            }

            return d;
        }
        public static Data2D operator -(double s, Data2D a)
        {
            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = (float)s - a[x, y];
                }
            }

            return d;
        }
        public static Data2D operator *(Data2D a, Data2D b)
        {
            if (a.Width != b.Width || a.Height != b.Height)
                throw new ArgumentException("Invalid matrix dimensions. Both matrices must be of size M x N.");

            Data2D d = new Data2D(a.Width, b.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = a[x, y] * b[x, y];
                }
            }

            return d;
        }
        public static Data2D operator *(Data2D a, float s)
        {
            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = s * a[x, y];
                }
            }

            return d;
        }
        public static Data2D operator *(Data2D a, double s)
        {
            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = (float)s * a[x, y];
                }
            }

            return d;
        }
        public static Data2D operator /(Data2D a, Data2D b)
        {
            if (a.Width != b.Width || a.Height != b.Height)
                throw new ArgumentException("Invalid matrix dimensions. Both matrices must be of size M x N.");

            Data2D d = new Data2D(a.Width, b.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = a[x, y] / b[x, y];
                }
            }

            return d;
        }
        public static Data2D operator /(Data2D a, float s)
        {
            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = a[x, y] / s;
                }
            }

            return d;
        }
        public static Data2D operator /(Data2D a, double s)
        {
            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = a[x, y] / (float)s;
                }
            }

            return d;
        }
        public static Data2D operator /(float s, Data2D a)
        {
            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = s / a[x, y];
                }
            }

            return d;
        }
        public static Data2D operator /(double s, Data2D a)
        {
            Data2D d = new Data2D(a.Width, a.Height);

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] = (float)s / a[x, y];
                }
            }

            return d;
        }
        public static Data2D operator ++(Data2D a)
        {
            Data2D d = new Data2D(a._matrix);
            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y]++;
                }
            }
            return d;
        }
        public static Data2D operator --(Data2D a)
        {
            Data2D d = new Data2D(a._matrix);
            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y]--;
                }
            }
            return d;
        }
        public static Data2D operator +(Data2D a)
        {
            Data2D d = new Data2D(a._matrix);
            return d;
        }
        public static Data2D operator -(Data2D a)
        {
            Data2D d = new Data2D(a._matrix);
            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    d[x, y] *= -1;
                }
            }
            return d;
        }
        #endregion

        #region ISavable

        // Layout:
        // (int)    UniqueID
        // (string) DataName
        // (int)    Width
        // (int)    Height
        // (float)  Minimum
        // (float)  Maximum
        // (float)  Mean
        // (float)  StdDev
        // (float)  _matrix

        public byte[] ToByteArray()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(ms);

                // Offset start of writing to later go back and 
                // prefix the stream with the size of the stream written
                bw.Seek(sizeof(int), SeekOrigin.Begin);

                // Write contents of object
                bw.Write(UniqueID);
                bw.Write(DataName);
                bw.Write(Width);
                bw.Write(Height);
                bw.Write(Minimum);
                bw.Write(Maximum);
                bw.Write(Mean);
                bw.Write(StdDev);
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        float value = this[x, y];
                        if (value > 0)
                        {
                            bw.Write(true);
                            bw.Write(value);
                        }
                        else bw.Write(false);
                    }
                }

                // Return to beginning of writer and write the length
                bw.Seek(0, SeekOrigin.Begin);
                bw.Write((int)bw.BaseStream.Length - sizeof(int));

                // Return to start of memory stream and 
                // return byte array of the stream
                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }

        public void FromByteArray(byte[] array)
        {
            using (MemoryStream ms = new MemoryStream(array))
            {
                BinaryReader br = new BinaryReader(ms);

                int uniqueID = br.ReadInt32();
                string dataName = br.ReadString();
                int width = br.ReadInt32();
                int height = br.ReadInt32();
                float minimum = br.ReadSingle();
                float maximum = br.ReadSingle();
                float mean = br.ReadSingle();
                float stdDev = br.ReadSingle();

                _matrix = new FlatArray<float>(width, height);
                this.DataName = dataName;
                this.UniqueID = _uniqueID;
                this._mean = mean;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (br.ReadBoolean())
                        {
                            this[x, y] = br.ReadSingle();
                        }
                    }
                }
            }
        }
        #endregion
    }

    public class Data3D : Data
    {
        List<Data2D> _layers;

        int _width;
        int _height;

        public int Width
        {
            get { return _width; }
        }
        public int Height
        {
            get { return _height; }
        }
        public int Depth
        {
            get { return _layers.Count; }
        }

        public float this[int x, int y, int z]
        {
            get
            {
                if (_layers == null || _layers.Count == 0) return 0;
                return _layers[z][x, y];
            }
            set
            {
                if (_layers == null || _layers.Count == 0) return;
                _layers[z][x, y] = value;
            }
        }
        public float[] this[int x, int y]
        {
            get
            {
                if (_layers == null || _layers.Count == 0) return new float[1] { 0 };

                float[] returnValues = new float[_layers.Count];

                for (int i = 0; i < _layers.Count; i++)
                {
                    returnValues[i] = _layers[i][x, y];
                }

                return returnValues;
            }
            set
            {
                if (_layers == null || _layers.Count == 0) return;

                if (value.Length != _layers.Count)
                {
                    throw new ArgumentException("Invalid dimension length.");
                }

                for (int i = 0; i < _layers.Count; i++)
                {
                    _layers[i][x, y] = value[i];
                }
            }
        }
        public Color this[Point p]
        {
            get
            {
                if (_layers == null || _layers.Count != 4)
                    throw new ArgumentException("Invalid dimension length.");

                int x = (int)p.X;
                int y = (int)p.Y;

                return Color.FromArgb((byte)_layers[3][x, y], (byte)_layers[2][x, y],
                    (byte)_layers[1][x, y], (byte)_layers[0][x, y]);
            }
            set
            {
                if (_layers == null || _layers.Count != 4)
                    throw new ArgumentException("Invalid dimension length.");

                int x = (int)p.X;
                int y = (int)p.Y;

                _layers[0][x, y] = value.B;
                _layers[1][x, y] = value.G;
                _layers[2][x, y] = value.R;
                _layers[3][x, y] = value.A;
            }
        }

        public float SingluarMaximum
        {
            get
            {
                if (_layers == null || _layers.Count == 0) throw new ArgumentException("No data loaded.");
                float max = 0;
                for (int i = 0; i < _layers.Count; i++)
                {
                    if (_layers[i].Maximum > max) max = _layers[i].Maximum;
                }
                return max;
            }
        }
        public float LayerMaximum(int Layer)
        {
            if (_layers == null || _layers.Count == 0) throw new ArgumentException("No data loaded.");
            return _layers[Layer].Maximum;
        }
        public Data2D[] Layers
        {
            get { return _layers.ToArray<Data2D>(); }
        }
        public float SingularMinimum
        {
            get
            {
                if (_layers == null || _layers.Count == 0) throw new ArgumentException("No data loaded.");
                float min = float.MaxValue;
                for (int i = 0; i < _layers.Count; i++)
                {
                    if (_layers[i].Minimum < min) min = _layers[i].Minimum;
                }
                return min;
            }
        }

        public string DataName
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged("DataName");
                }
            }
        }

        public Data2D Summed
        {
            get
            {
                float[,] summed = new float[Width, Height];

                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        float sum = 0;
                        for (int z = 0; z < Depth; z++)
                        {
                            sum += this[x, y, z];
                        }
                        summed[x, y] = sum;
                    }
                }

                return new Data2D(summed);
            }
        }
        public int NumberTables
        {
            get
            {
                if (_layers == null) return 0;
                return _layers.Count;
            }
        }

        public Data3D()
        {
        }
        public Data3D(Data2D[] Layers)
        {
            AddLayers(Layers);
            DataName = Layers[0].DataName;
        }
        public Data3D(List<Data2D> Layers)
        {
            AddLayers(Layers);
            DataName = Layers[0].DataName;
        }
        public Data3D(int SizeX, int SizeY, int SizeZ)
        {
            for (int z = 0; z < SizeZ; z++)
            {
                AddLayer(new Data2D(SizeX, SizeY));
            }
        }
        public Data3D(float[,,] Matrix)
        {
            int sizeX = Matrix.GetLength(0);
            int sizeY = Matrix.GetLength(1);
            int sizeZ = Matrix.GetLength(2);

            for (int z = 0; z < sizeZ; z++)
            {
                Data2D d = new Data2D(sizeX, sizeY);
                for (int x = 0; x < sizeX; x++)
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                        d[x, y] = Matrix[x, y, z];
                    }
                }
                AddLayer(d);
            }
        }

        public void AddLayer(Data2D Layer)
        {
            if (_width == 0 || _height == 0)
            {
                _width = Layer.Width;
                _height = Layer.Height;
            }
            else
            {
                if (_width != Layer.Width || _height != Layer.Height)
                {
                    throw new ArgumentException("The layer being added does not match the current dimensions.");
                }
            }

            if (_layers == null || _layers.Count == 0)
            {
                _layers = new List<Data2D>();
            }
            _layers.Add(Layer);
        }
        public void AddLayers(Data2D[] Layers)
        {
            if (_width == 0 || _height == 0)
            {
                _width = Layers[0].Width;
                _height = Layers[0].Height;
            }
            else
            {
                foreach (Data2D layer in Layers)
                {
                    if (_width != layer.Width || _height != layer.Height)
                    {
                        throw new ArgumentException("One or more layer being added does not match the current dimensions.");
                    }
                }
            }

            if (_layers == null || _layers.Count == 0)
            {
                _layers = new List<Data2D>();
            }

            _layers.AddRange(Layers);
        }
        public void AddLayers(List<Data2D> Layers)
        {
            if (_width == 0 || _height == 0)
            {
                _width = Layers[0].Width;
                _height = Layers[0].Height;
            }
            else
            {
                foreach (Data2D layer in Layers)
                {
                    if (_width != layer.Width || _height != layer.Height)
                    {
                        throw new ArgumentException("One or more layer being added does not match the current dimensions.");
                    }
                }
            }

            if (_layers == null || _layers.Count == 0)
            {
                _layers = new List<Data2D>();
            }

            _layers.AddRange(Layers);
        }

        public int[,,] ToIntArray()
        {
            int[,,] returnArray = new int[Width, Height, Depth];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        returnArray[x, y, z] = (int)_layers[z][x, y];
                    }
                }
            }
            return returnArray;
        }
        public int[,,] ToIntArray(bool Normalize)
        {
            int[,,] returnArray = new int[Width, Height, Depth];
            float max = SingluarMaximum;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        returnArray[x, y, z] = (int)(_layers[z][x, y] / max);
                    }
                }
            }
            return returnArray;
        }
        public int[,,] ToIntArray(int NormalizationValue)
        {
            int[,,] returnArray = new int[Width, Height, Depth];
            float max = SingluarMaximum;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        returnArray[x, y, z] = (int)(_layers[z][x, y] / max);
                    }
                }
            }
            return returnArray;
        }
        public float[,,] ToFloatArray()
        {
            float[,,] returnArray = new float[Width, Height, Depth];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        returnArray[x, y, z] = _layers[z][x, y];
                    }
                }
            }
            return returnArray;
        }
        public float[,,] ToFloatArray(bool Normalize)
        {
            float[,,] returnArray = new float[Width, Height, Depth];
            float max = SingluarMaximum;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        returnArray[x, y, z] = _layers[z][x, y] / max;
                    }
                }
            }

            return returnArray;
        }
        public float[,,] ToFloatArray(float NormalizationValue)
        {
            float[,,] returnArray = new float[Width, Height, Depth];
            float max = NormalizationValue;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        returnArray[x, y, z] = _layers[z][x, y] / max;
                    }
                }
            }

            return returnArray;
        }

        public void FlipHorizontal()
        {
            foreach (var layer in _layers)
            {
                layer.FlipHorizontal();
            }
        }
        public void FlipVertical()
        {
            foreach (var layer in _layers)
            {
                layer.FlipVertical();
            }
        }

        public List<Data2D> ToArray()
        {
            return _layers;
        }

        public Data3D Smooth(int windowSize)
        {
            Data3D d = new Data3D(Width, Height, Depth);

            int startOffset = windowSize / 2;
            int endOffset = windowSize - startOffset;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        int sX = Math.Max(0, x - startOffset);
                        int sY = Math.Max(0, y - startOffset);
                        int sZ = Math.Max(0, z - startOffset);

                        int eX = Math.Min(x + endOffset, Width);
                        int eY = Math.Min(y + endOffset, Height);
                        int eZ = Math.Min(z + endOffset, Depth);

                        List<float> values = new List<float>();
                        for (int a = sX; a < eX; a++)
                        {
                            for (int b = sY; b < eY; b++)
                            {
                                for (int c = sZ; c < eZ; c++)
                                {
                                    values.Add(this[a, b, c]);
                                }

                            }
                        }

                        d[x, y, z] = values.Average();
                        //float sum = 0;
                        //int count = 0;

                        //for (int a = 0; a < windowSize; a++)
                        //{
                        //    for (int b = 0; b < windowSize; b++)
                        //    {
                        //        for (int c = 0; c < windowSize; c++)
                        //        {
                        //            int xx = x - (windowSize / 2) + a;
                        //            int yy = y - (windowSize / 2) + b;
                        //            int zz = z - (windowSize / 2) + c;

                        //            if (xx < 0 || xx >= Width ||
                        //                yy < 0 || yy >= Height ||
                        //                zz < 0 || zz >= Depth)
                        //                continue;

                        //            sum += this[xx, yy, zz];
                        //            count++;
                        //        }
                        //    }
                        //}

                        //d[x, y, z] = sum / count;
                    }
                }
            }

            return d;
        }
        public async Task<Data3D> SmoothAsync(int windowSize)
        {
            return await Task.Run(() => Smooth(windowSize));
        }

        public Data3D Crop(int StartX, int StartY, int LengthX = -1, int LengthY = -1)
        {
            Data2D[] d = new Data2D[this.Depth];
            for (int i = 0; i < this.Depth; i++)
            {
                d[i] = this.Layers[i].Crop(StartX, StartY, LengthX, LengthY);
            }
            return new Data3D(d);
        }
        public Data3D Crop(Rect Rectangle)
        {
            Data2D[] d = new Data2D[this.Depth];
            for (int i = 0; i < this.Depth; i++)
            {
                d[i] = this.Layers[i].Crop(Rectangle);
            }
            return new Data3D(d);
        }

        public Data3D Slice(int layerStart, int layerEnd, int binSize = 1)
        {
            int depth = (layerEnd - layerStart + 1) / binSize;
            if ((layerEnd - layerStart + 1) % binSize != 0)
                depth += 1;

            if (depth == 0)
                throw new ArgumentException("Invalid depth dimension");

            Data3D d = new Data3D(_width, _height, depth);

            d.DataName = $"{DataName}: ({layerStart + 1}-{layerEnd + 1})";

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        float sum = 0;
                        for (int i = 0; i < binSize; i++)
                        {
                            int pos = layerStart + (z * binSize + i);
                            if (pos >= Depth) break;

                            sum += this[x, y, pos];
                        }
                        d[x, y, z] = sum;
                    }
                }
            }

            return d;
        }
        public async Task<Data3D> SliceAsync(int layerStart, int layerEnd, int binSize = 1)
        {
            return await Task.Run(() => Slice(layerStart, layerEnd, binSize));
        }

        public Data2D FromLayers(int startLayer, int endLayer)
        {
            Data2D d = new Data2D(Width, Height);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = startLayer; z <= endLayer; z++)
                    {
                        d[x, y] += this[x, y, z];
                    }
                }
            }

            d.DataName = $"{DataName}: ({startLayer + 1}-{endLayer + 1})";

            return d;
        }
        public async Task<Data2D> FromLayersAsync(int startLayer, int endLayer)
        {
            return await Task.Run(() => FromLayers(startLayer, endLayer));
        }

        public Data3D ExpandIntensity(int windowSize = 5)
        {
            Data2D[] layers = new Data2D[Depth];
            for (int z = 0; z < Depth; z++)
            {
                layers[z] = this.Layers[z].ExpandIntensity(windowSize);
            }
            return new Data3D(layers);
        }
        public async Task<Data3D> ExpandIntensityAsync(int windowSize = 5)
        {
            return await Task.Run(() => ExpandIntensity(windowSize));
        }

        public static explicit operator Color[,] (Data3D d)
        {
            Color[,] c = new Color[d.Width, d.Height];
            for (int x = 0; x < d.Width; x++)
            {
                for (int y = 0; y < d.Height; y++)
                {
                    //Data3D color array returns:
                    //f[0] = blue
                    //f[1] = green
                    //f[2] = red
                    //f[3] = alpha
                    float[] f = d[x, y];
                    c[x, y] = Color.FromArgb((byte)f[3], (byte)f[2], (byte)f[1], (byte)f[0]);
                }
            }
            return c;
        }
        public static explicit operator SharpDX.Color[,] (Data3D d)
        {
            SharpDX.Color[,] c = new SharpDX.Color[d.Width, d.Height];
            for (int x = 0; x < d.Width; x++)
            {
                for (int y = 0; y < d.Height; y++)
                {
                    //Data3D color array returns:
                    //f[0] = blue
                    //f[1] = green
                    //f[2] = red
                    //f[3] = alpha
                    float[] f = d[x, y];
                    c[x, y] = new SharpDX.Color(f[2] / 255f, f[1] / 255f, f[0] / 255f, f[3] / 255f);
                }
            }
            return c;
        }

        public static Data3D BlankColorData(int width, int height)
        {
            Data3D d = new Data3D(width, height, 4);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    d[x, y] = new float[] { 0, 0, 0, 255 };
                }
            }
            return d;
        }
        public static Data3D Rescale(Data3D d, float minimum, float maximum)
        {

            float dataMinimum = float.MaxValue;
            float dataMaximum = float.MinValue;

            foreach (var layer in d.Layers)
            {
                if (layer.Minimum < dataMinimum) dataMinimum = layer.Minimum;
                if (layer.Maximum > dataMaximum) dataMaximum = layer.Maximum;
            }

            float dataRange = dataMaximum - dataMinimum;
            float range = maximum - minimum;

            Data3D r = new Data3D(d.Width, d.Height, d.Depth);
            for (int z = 0; z < d.Layers.Length; z++)
            {
                for (int x = 0; x < d.Width; x++)
                {
                    for (int y = 0; y < d.Height; y++)
                    {
                        r[x, y, z] = (range * (d[x, y, z] - dataMinimum) / dataRange) + minimum;
                    }
                }
            }

            return r;
        }

        public static Data3D Rescale(Data3D d, float minimum, float maximum, int startLayer, int endLayer)
        {
            if (startLayer < 0 || startLayer >= d.Layers.Length - 1)
                throw new ArgumentException("Invalid layer specification: start layer.");
            if (endLayer >= d.Layers.Length)
                throw new ArgumentException("Invalid layer specification: end layer.");
            if (startLayer >= endLayer)
                throw new ArgumentException("Invalid layer specification: start layer is larger than end layer.");

            float dataMinimum = float.MaxValue;
            float dataMaximum = float.MinValue;

            for (int z = startLayer; z < endLayer; z++)
            {
                Data2D layer = d.Layers[z];
                if (layer.Minimum < dataMinimum) dataMinimum = layer.Minimum;
                if (layer.Maximum > dataMaximum) dataMaximum = layer.Maximum;
            }

            float dataRange = dataMaximum - dataMinimum;
            float range = maximum - minimum;

            Data3D r = new Data3D(d.Width, d.Height, d.Depth);
            for (int z = 0; z < startLayer; z++)
            {
                for (int x = 0; x < d.Width; x++)
                {
                    for (int y = 0; y < d.Height; y++)
                    {
                        r[x, y, z] = d[x, y, z];
                    }
                }
            }

            for (int z = startLayer; z < endLayer; z++)
            {
                for (int x = 0; x < d.Width; x++)
                {
                    for (int y = 0; y < d.Height; y++)
                    {
                        r[x, y, z] = (range * (d[x, y, z] - dataMinimum) / dataRange) + minimum;
                    }
                }
            }

            for (int z = endLayer; z < d.Depth; z++)
            {
                for (int x = 0; x < d.Width; x++)
                {
                    for (int y = 0; y < d.Height; y++)
                    {
                        r[x, y, z] = d[x, y, z];
                    }
                }
            }

            return r;
        }

        /// <summary>
        /// Resizes the array to the specified width and height. Can only be used 
        /// to increase the matrix dimensions.
        /// </summary>
        /// <param name="targetWidth">Width of the new matrix.</param>
        /// <param name="targetHeight">Height of the new matrix.</param>
        /// <param name="useBilinear">True for bilinear interpolation or false for bicubic interpolation.</param>
        /// <returns>Matrix resized to the specified dimensions.</returns>
        public Data3D Upscale(int targetWidth, int targetHeight, bool useBilinear = true)
        {
            int lowResSizeX = Width;
            int lowResSizeY = Height;
            int highResSizeX = targetWidth;
            int highResSizeY = targetHeight;

            if (lowResSizeX > highResSizeX || lowResSizeY > highResSizeY)
                throw new ArgumentException("Cannot downscale data using this function.");

            Data2D[] resized = new Data2D[Depth];
            for (int i = 0; i < 4; i++)
            {
                resized[i] = Layers[i].Upscale(highResSizeX, highResSizeY, useBilinear);
            }

            return new Data3D(resized);
        }
    }
}
