using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagingSIMS.Data
{
    public class Data4D : Data
    {
        List<float[, ,]> _matrix;
        float _max;

        int _width;
        int _height;
        int _depth;

        public override string SizeString
        {
            get
            {
                return $"W: {Width} H: {Height} D: {Depth} N: {_matrix.Count}";
            } 
        }

        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="z">The z-coordinate.</param>
        /// <param name="w">The w-coordinate.</param>
        /// <returns>The value at the specified location</returns>
        public float this[int x, int y, int z, int w]
        {
            get { return _matrix[w][x, y, z]; }
            set
            {
                _matrix[w][x, y, z] = value;
                if (value > _max) _max = value;
            }
        }
        /// <summary>
        /// Gets the maximum value of the data set.
        /// </summary>
        public float Maximum
        {
            get { return _max; }
        }

        /// <summary>
        /// Gets or sets the name of the data set.
        /// </summary>
        public string DataName
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        /// <summary>
        /// Gets the width (x-dimension) of the data set.
        /// </summary>
        public int Width
        {
            get { return _width; }
        }
        /// <summary>
        /// Gets the height (y-dimension) of the data set.
        /// </summary>
        public int Height
        {
            get { return _height; }
        }
        /// <summary>
        /// Gets the depth (z-dimension) of the data set.
        /// </summary>
        public int Depth
        {
            get { return _depth; }
        }
        /// <summary>
        /// Gets the spissitude (w-dimension) of the data set.
        /// </summary>
        public int Spissitude
        {
            get { return _matrix.Count; }
        }

        /// <summary>
        /// Creates a blank Data4D with the specified dimensions.
        /// </summary>
        /// <param name="Width">The width of the data set.</param>
        /// <param name="Height">The height of the data set.</param>
        /// <param name="Depth">The depth of the data set.</param>
        public Data4D(int Width, int Height, int Depth)
        {
            _matrix = new List<float[, ,]>();

            _width = Width;
            _height = Height;
            _depth = Depth;
        }
        /// <summary>
        /// Creates a Data4D object with a spissitude of 1.
        /// </summary>
        /// <param name="Data">3D data matrix.</param>
        public Data4D(float[, ,] Data)
        {
            _matrix = new List<float[, ,]>();
            _matrix.Add(Data);

            _width = Data.GetLength(0);
            _height = Data.GetLength(1);
            _depth = Data.GetLength(2);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        if (Data[x, y, z] > _max) _max = Data[x, y, z];
                    }
                }
            }
        }
        /// <summary>
        /// Creates a Data4D object with a spissitude of 1.
        /// </summary>
        /// <param name="Data">3D data matrix.</param>
        public Data4D(List<Data2D> Data)
        {
            _matrix = new List<float[, ,]>();

            _width = Data[0].Width;
            _height = Data[0].Height;
            _depth = Data.Count;

            _matrix.Add(ConvertData2D(Data, out _max));
        }
        /// <summary>
        /// Creates a Data4D object with a spissitude of 1.
        /// </summary>
        /// <param name="Data">3D data matrix.</param>
        public Data4D(Data2D[] Data)
        {
            _matrix = new List<float[, ,]>();

            _width = Data[0].Width;
            _height = Data[0].Height;
            _depth = Data.Length;

            _matrix.Add(ConvertData2D(Data, out _max));
        }
        /// <summary>
        /// Creates a Data4D object from the specified 4D data matrix.
        /// </summary>
        /// <param name="Data">List of a List of 2D data matrices.</param>
        public Data4D(List<List<Data2D>> Data)
        {
            _width = Data[0][0].Width;
            _height = Data[0][0].Height;
            _depth = Data[0].Count;

            foreach (List<Data2D> list in Data)
            {
                if (
                    _width != list[0].Width ||
                    _height != list[0].Height ||
                    _depth != list.Count
                    )
                    throw new ArgumentException("Not all elements of the 4D array are the same dimensions.");
            }

            _matrix = new List<float[, ,]>();            

            for (int w = 0; w < Data.Count; w++)
            {
                float maximum = 0;
                _matrix.Add(ConvertData2D(Data[w], out maximum));
                if (maximum > _max) _max = maximum;
            }
        }
        /// <summary>
        /// Creates a Data4D object from the specified 4D data matrix.
        /// </summary>
        /// <param name="Data">List of a List of 2D data matrices.</param>
        public Data4D(List<Data2D>[] Data)
        {
            _width = Data[0][0].Width;
            _height = Data[0][0].Height;
            _depth = Data[0].Count;

            foreach (List<Data2D> list in Data)
            {
                if (
                    _width != list[0].Width ||
                    _height != list[0].Height ||
                    _depth != list.Count
                    )
                    throw new ArgumentException("Not all elements of the 4D array are the same dimensions.");
            }

            _matrix = new List<float[, ,]>();

            for (int w = 0; w < Data.Length; w++)
            {
                float maximum = 0;
                _matrix.Add(ConvertData2D(Data[w], out maximum));
                if (maximum > _max) _max = maximum;
            }
        }

        /// <summary>
        /// Converts a set of 2D data sets to float matrix for adding to the 4D data set.
        /// </summary>
        /// <param name="Data">List of Data2D to convert.</param>
        /// <param name="max">The maximum value found in the array.</param>
        /// <returns>3D float matrix.</returns>
        private float[, ,] ConvertData2D(List<Data2D> Data, out float max)
        {
            int width = Data[0].Width;
            int height = Data[0].Height;
            int depth = Data.Count;

            float[, ,] newMatrix = new float[Width, Height, Depth];
            float maximum = 0;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        newMatrix[x, y, z] = Data[z][x, y];
                        if (newMatrix[x, y, z] > maximum) maximum = newMatrix[x, y, z];
                    }
                }
            }

            max = maximum;

            return newMatrix;
        }
        /// <summary>
        /// Converts a set of 2D data sets to float matrix for adding to the 4D data set.
        /// </summary>
        /// <param name="Data">Data2D array to convert.</param>
        /// <param name="max">The maximum value found in the array.</param>
        /// <returns>3D float matrix.</returns>
        private float[, ,] ConvertData2D(Data2D[] Data, out float max)
        {
            int width = Data[0].Width;
            int height = Data[0].Height;
            int depth = Data.Length;

            float[, ,] newMatrix = new float[Width, Height, Depth];
            float maximum = 0;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        newMatrix[x, y, z] = Data[z][x, y];
                        if (newMatrix[x, y, z] > maximum) maximum = newMatrix[x, y, z];
                    }
                }
            }

            max = maximum;

            return newMatrix;
        }

        /// <summary>
        /// Converts the Data4D object to a new float array.
        /// </summary>
        /// <returns>4D float array.</returns>
        public float[,,,] ToArray()
        {
            float[, , ,] returnMatrix = new float[Width, Height, Depth, Spissitude];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        for (int w = 0; w < Spissitude; w++)
                        {
                            returnMatrix[x, y, z, w] = _matrix[w][x, y, z];
                        }
                    }
                }
            }
            return returnMatrix;
        }
        /// <summary>
        /// Converts the Data4D object to a new float array normalized to the maximum value.
        /// </summary>
        /// <returns>Normalized 4D float array.</returns>
        public float[, , ,] ToNormalizedArray()
        {
            float[, , ,] returnMatrix = new float[Width, Height, Depth, Spissitude];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        for (int w = 0; w < Spissitude; w++)
                        {
                            returnMatrix[x, y, z, w] = _matrix[w][x, y, z] / _max;
                        }
                    }
                }
            }
            return returnMatrix;
        }
        /// <summary>
        /// Converts the Data4D object to a new float array normalized to the specified value.
        /// </summary>
        /// <param name="NormalizedValue">Value for nomralization.</param>
        /// <returns>Normalized 4D float array.</returns>
        public float[, , ,] ToNormalizedArray(float NormalizedValue)
        {
            float[, , ,] returnMatrix = new float[Width, Height, Depth, Spissitude];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        for (int w = 0; w < Spissitude; w++)
                        {
                            returnMatrix[x, y, z, w] = _matrix[w][x, y, z] / NormalizedValue;
                        }
                    }
                }
            }
            return returnMatrix;
        }

        /// <summary>
        /// Determines if the target data set matches the dimensions of the current Data4D set.
        /// </summary>
        /// <param name="Target">Target data set.</param>
        /// <returns>True if all three dimensions match, false otherwise.</returns>
        protected bool CheckDimensions(Data3D Target)
        {
            return (
                this.Width == Target.Width &&
                this.Height == Target.Height &&
                this.Depth == Target.Depth
                );
        }
        /// <summary>
        /// Determines if the target data set matches the dimensions of the current Data4D set.
        /// </summary>
        /// <param name="Target">Target data set.</param>
        /// <returns>True if all three dimensions match, false otherwise.</returns>
        protected bool CheckDimensions(float[, ,] Target)
        {
            return (
                this.Width == Target.GetLength(0) &&
                this.Height == Target.GetLength(1) &&
                this.Depth == Target.GetLength(2)
                );
        }
        /// <summary>
        /// Determines if the target data set matches the dimensions of the current Data4D set.
        /// </summary>
        /// <param name="Target">Target data set.</param>
        /// <returns>True if all three dimensions match, false otherwise.</returns>
        protected bool CheckDimensions(List<Data2D> Target)
        {
            return (
                this.Width == Target[0].Width &&
                this.Height == Target[0].Height &&
                this.Depth == Target.Count
                );
        }

        /// <summary>
        /// Adds a 3D data set to the current Data4D object.
        /// </summary>
        /// <param name="Data">Data set to add.</param>
        public void AddData(float[, ,] Data)
        {
            if (!CheckDimensions(Data))
                throw new ArgumentException("The target dimensions do not match the current object's dimensions.");

            _matrix.Add(Data);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        if (Data[x, y, z] > _max) _max = Data[x, y, z];
                    }
                }
            }
        }
        /// <summary>
        /// Adds a 3D data set to the current Data4D object.
        /// </summary>
        /// <param name="Data">Data set to add.</param>
        public void AddData(List<Data2D> Data)
        {
            if (!CheckDimensions(Data))
                throw new ArgumentException("The target dimensions do not match the current object's dimensions.");

            float maximum = 0;
            _matrix.Add(ConvertData2D(Data, out maximum));
        }
    }

    public class DataVolume : Data4D
    {
        List<MassIdentifier> _identifiers;

        public DataVolume(int Width, int Height, int Depth)
            : base(Width, Height, Depth)
        {
            _identifiers = new List<MassIdentifier>();
        }
        public DataVolume(float[, ,] Data, MassIdentifier Identity)
            : base(Data)
        {
            _identifiers = new List<MassIdentifier>();
            _identifiers.Add(Identity);
        }
        public DataVolume(List<Data2D> Data, MassIdentifier Identity)
            : base(Data)
        {
            _identifiers = new List<MassIdentifier>();
            _identifiers.Add(Identity);
        }

        public void AddData(float[, ,] Data, MassIdentifier Identity)
        {
            base.AddData(Data);
            _identifiers.Add(Identity);
        }
        public void AddData(List<Data2D> Data, MassIdentifier Identity)
        {
            base.AddData(Data);
            _identifiers.Add(Identity);
        }
    }

    public struct MassIdentifier
    {
        public string Name;
        public float MassStart;
        public float MassEnd;
    }
}
