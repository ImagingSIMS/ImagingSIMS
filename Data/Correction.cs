using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using ImagingSIMS.Common;
using ImagingSIMS.Data.Rendering;

namespace ImagingSIMS.Data
{
    //public static class _ZCorrection
    //{
    //    public static List<Data2D> ZCorrect(List<Data2D> Data, int Threshold)
    //    {
    //        int sizeX = Data[0].Width;
    //        int sizeY = Data[0].Height;
    //        int sizeZ = Data.Count;

    //        bool[, ,] threshMatrix = new bool[sizeX, sizeY, sizeZ];
    //        for (int x = 0; x < sizeX; x++)
    //        {
    //            for (int y = 0; y < sizeY; y++)
    //            {
    //                for (int z = 0; z < sizeZ; z++)
    //                {
    //                    threshMatrix[x, y, z] = Data[z][x, y] >= Threshold;
    //                }
    //            }
    //        }

    //        Data3D correctedMatrix = new Data3D(sizeX, sizeY, sizeZ);

    //        for (int x = 0; x < sizeX; x++)
    //        {
    //            for (int y = 0; y < sizeY; y++)
    //            {
    //                //Get layer of first true
    //                int position = sizeZ - 1;
    //                while (!threshMatrix[x, y, position])
    //                {
    //                    position--;
    //                    if (position < 0) break;
    //                }
    //                int firstPosition = position;
    //                int difference = (sizeZ - 1) - firstPosition;
    //                //Shift to bottom most
    //                for (int a = firstPosition; a >= 0; a--)
    //                {
    //                    for (int i = 0; i < 4; i++)
    //                    {
    //                        correctedMatrix[x, y, a + difference] = Data[a][x, y];
    //                    }
    //                }
    //            }
    //        }

    //        return correctedMatrix.ToArray();
    //    }
    //    public static List<Data2D> ZCorrect(List<Data2D> Data, int Threshold, BackgroundWorker bw)
    //    {
    //        int sizeX = Data[0].Width;
    //        int sizeY = Data[0].Height;
    //        int sizeZ = Data.Count;

    //        bool[, ,] threshMatrix = new bool[sizeX, sizeY, sizeZ];
    //        for (int x = 0; x < sizeX; x++)
    //        {
    //            for (int y = 0; y < sizeY; y++)
    //            {
    //                for (int z = 0; z < sizeZ; z++)
    //                {
    //                    threshMatrix[x, y, z] = Data[z][x, y] >= Threshold;
    //                }
    //            }
    //        }

    //        Data3D correctedMatrix = new Data3D(sizeX, sizeY, sizeZ);

    //        int totalSteps = sizeX * sizeY;
    //        int pos = 0;

    //        for (int x = 0; x < sizeX; x++)
    //        {
    //            for (int y = 0; y < sizeY; y++)
    //            {
    //                //Get layer of first true
    //                int position = sizeZ - 1;
    //                while (!threshMatrix[x, y, position])
    //                {
    //                    position--;
    //                    if (position < 0) break;
    //                }
    //                int firstPosition = position;
    //                int difference = (sizeZ - 1) - firstPosition;
    //                //Shift to bottom most
    //                for (int a = firstPosition; a >= 0; a--)
    //                {
    //                    for (int i = 0; i < 4; i++)
    //                    {
    //                        correctedMatrix[x, y, a + difference] = Data[a][x, y];
    //                    }
    //                }
    //                pos++;
    //            }
    //            bw.ReportProgress(Percentage.GetPercent(pos, totalSteps));
    //        }

    //        return correctedMatrix.ToArray();
    //    }

    //    public static List<Data2D> ZCorrect(List<Data2D> Data, _ZCorrectionBase Mask)
    //    {
    //        int sizeX = Data[0].Width;
    //        int sizeY = Data[0].Height;
    //        int sizeZ = Data.Count;

    //        Data3D correctedMatrix = new Data3D(sizeX, sizeY, sizeZ);

    //        for (int x = 0; x < sizeX; x++)
    //        {
    //            for (int y = 0; y < sizeY; y++)
    //            {
    //                int firstPosition = Mask[x, y];
    //                int difference = (sizeZ - 1) - firstPosition;
    //                //Shift to bottom most
    //                for (int a = firstPosition; a >= 0; a--)
    //                {
    //                    for (int i = 0; i < 4; i++)
    //                    {
    //                        correctedMatrix[x, y, a + difference] = Data[a][x, y];
    //                    }
    //                }
    //            }
    //        }

    //        return correctedMatrix.ToArray();
    //    }
    //    public static List<Data2D> ZCorrect(List<Data2D> Data, _ZCorrectionBase Mask, BackgroundWorker bw)
    //    {
    //        int sizeX = Data[0].Width;
    //        int sizeY = Data[0].Height;
    //        int sizeZ = Data.Count;

    //        Data3D correctedMatrix = new Data3D(sizeX, sizeY, sizeZ);

    //        int totalSteps = sizeX * sizeY;
    //        int pos = 0;

    //        for (int x = 0; x < sizeX; x++)
    //        {
    //            for (int y = 0; y < sizeY; y++)
    //            {
    //                int firstPosition = Mask[x, y];
    //                int difference = (sizeZ - 1) - firstPosition;
    //                //Shift to bottom most
    //                for (int a = firstPosition; a >= 0; a--)
    //                {
    //                    correctedMatrix[x, y, a + difference] = Data[a][x, y];
    //                }
    //                pos++;
    //            }
    //            bw.ReportProgress(Percentage.GetPercent(pos, totalSteps));
    //        }

    //        return correctedMatrix.ToArray();
    //    }
    //}

    //public class _ZCorrectionBase : Data
    //{
    //    int _width;
    //    int _height;
    //    int _depth;

    //    int[,] _mask;

    //    int _threshold;

    //    public int this[int x, int y]
    //    {
    //        get { return _mask[x, y]; }
    //    }

    //    public string MaskName
    //    {
    //        get { return _name; }
    //        set
    //        {
    //            _name = value;
    //            NotifyPropertyChanged("MaskName");
    //        }
    //    }
    //    public int Threshold
    //    {
    //        get { return _threshold; }
    //        set
    //        {
    //            _threshold = value;
    //            NotifyPropertyChanged("Threshold");
    //        }
    //    }


    //    public _ZCorrectionBase(string MaskName, List<Data2D> Data, int Threshold)
    //    {
    //        this.MaskName = MaskName;
    //        this.Threshold = Threshold;

    //        _width = Data[0].Width;
    //        _height = Data[0].Height;
    //        _depth = Data.Count;

    //        foreach (Data2D d in Data)
    //        {
    //            if (d.Width != _width || d.Height != _height)
    //            {
    //                throw new ArgumentException("Not all tables have the same dimensions.");
    //            }
    //        }

    //        _mask = new int[_width, _height];

    //        for (int x = 0; x < _width; x++)
    //        {
    //            for (int y = 0; y < _height; y++)
    //            {
    //                int index = 0;

    //                //for (int z = _depth - 1; z >=0 ; z--)
    //                for (int z = 0; z < _depth; z++)
    //                {
    //                    if (Data[z][x, y] >= Threshold)
    //                    {
    //                        index = z;
    //                        break;
    //                    }
    //                }

    //                _mask[x, y] = index;
    //            }
    //        }
    //    }

    //    private bool VerifyDimensions(List<Data2D> Data)
    //    {
    //        int depth = Data.Count;

    //        foreach (Data2D d in Data)
    //        {
    //            if (d.Width != _width || d.Height != _height)
    //            {
    //                return false;
    //            }
    //        }

    //        return depth == _depth;
    //    }
    //}

    public class ZCorrection : DependencyObject
    {
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name",
            typeof(string), typeof(ZCorrection));
        public static readonly DependencyProperty DepthProperty = DependencyProperty.Register("Depth",
            typeof(float), typeof(ZCorrection));

        Data2D _relief;
        //float _threshold;

        public int Width { get { return _relief.Width; } }
        public int Height { get { return _relief.Height; } }

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public int Depth 
        {
            get { return (int)GetValue(DepthProperty); }
            set { SetValue(DepthProperty, value); }
        }

        public ZCorrection(Data2D Relief, string Name, int Depth)
        {
            _relief = Relief;

            this.Name = Name;
            this.Depth = Depth;
        }

        /// <summary>
        /// Creates a ZCorrection matrix from specified 3D data set which is the highest index (deepest) 
        /// of signal that occurs above specified threshold value at each pixel.
        /// </summary>
        /// <param name="Data">3D data set to create mask from.</param>
        /// <param name="Threshold">Minimum signal value.</param>
        /// <param name="Name">Name for the new object.</param>
        /// <returns>ZCorrection matrix to use for performing correction operation.</returns>
        public ZCorrection FromSignal(List<Data2D> Data, float Threshold, string Name)
        {
            if (!CheckDimensions(Data)) throw new ArgumentException("Not all tables have the same dimensions.");

            int width = Data[0].Width;
            int height = Data[0].Height;
            int depth = Data.Count;

            bool[, ,] matrix = new bool[width, height, depth];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        matrix[x, y, z] = Data[z][x, y] >= Threshold;
                    }
                }
            }

            Data2D mask = new Data2D(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = depth - 1; z >= 0; z++) 
                    {
                        if (matrix[x, y, z])
                        {
                            mask[x, y] = z;
                            continue;
                        }
                    }
                }
            }
            
            return new ZCorrection(mask, Name, depth);
        }
        /// <summary>
        /// Creates a ZCorrection matrix from specified 3D data set which is the lowest index (shallowest) 
        /// of signal that occurs above specified threshold value at each pixel.
        /// </summary>
        /// <param name="Data">3D data set to create mask from.</param>
        /// <param name="Threshold">Minimum signal value.</param>
        /// <param name="Name">Name for the new object.</param>
        /// <returns>ZCorrection matrix to use for performing correction operation.</returns>
        public ZCorrection FromSubstrate(List<Data2D> Data, float Threshold, string Name)
        {
            if (!CheckDimensions(Data)) throw new ArgumentException("Not all tables have the same dimensions.");

            int width = Data[0].Width;
            int height = Data[0].Height;
            int depth = Data.Count;

            bool[, ,] matrix = new bool[width, height, depth];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        matrix[x, y, z] = Data[z][x, y] >= Threshold;
                    }
                }
            }

            Data2D mask = new Data2D(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        if (matrix[x, y, z])
                        {
                            mask[x, y] = z;
                            continue;
                        }
                    }
                }
            }

            return new ZCorrection(mask, Name, depth);
        }

        public List<Data2D> Correct(List<Data2D> DataToCorrect)
        {
            if (!CheckDimensions(DataToCorrect)) throw new ArgumentException("Not all input tables match in size.");

            int width = DataToCorrect[0].Width;
            int height = DataToCorrect[0].Height;
            int depth = DataToCorrect.Count;

            if (width != Width || height != Height || depth != Depth) 
                throw new ArgumentException("Input data dimensions do not match mask dimensions.");

            List<Data2D> corrected = new List<Data2D>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {

                }
            }

            return corrected;
        }

        private bool CheckDimensions(List<Data2D> Data)
        {
            int width = -1;
            int height = -1;

            foreach(Data2D d in Data)
            {
                if (width == -1) width = d.Width;
                if (height == -1) height = d.Height;

                if (width != d.Width || height == d.Height) return false;
            }

            return true;
        }

        public override string ToString()
        {
            return string.Format("{0} - Depth: {1}", this.Name, this.Depth);
        }
    }

    public static class ZCorrection3D
    {
        public static Task<List<Volume>> CorrectAsync(List<Volume> volumes, float threshold)
        {
            return Task.Run<List<Volume>>(() => Correct(volumes, threshold));
        }
        public static List<Volume> Correct(List<Volume> volumes, float threshold)
        {
            int width;
            int height;
            int depth;

            bool[, ,] matrix;

            // Check that dimensions match across all input volumes
            int targetWidth = -1;
            int targetHeight = -1;
            int targetDepth = -1;

            foreach (Volume v in volumes)
            {
                if (targetWidth == -1 && targetHeight == -1 && targetDepth == -1)
                {
                    targetWidth = v.Width;
                    targetHeight = v.Height;
                    targetDepth = v.Depth;
                    continue;
                }

                if (targetWidth != v.Width || targetHeight != v.Height || targetDepth != v.Depth)
                {
                    throw new ArgumentException("Invalid dimensions. Dimensions do not match");
                }
            }

            width = targetWidth;
            height = targetHeight;
            depth = targetDepth;

            // Create matrix of voxels that are at or above specified threshold
            // These are voxels that contain information
            matrix = new bool[width, height, depth];

            foreach (Volume v in volumes)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int z = 0; z < depth; z++)
                        {
                            matrix[x, y, z] = v[x, y, z] >= threshold;
                        }
                    }
                }
            }

            // Create the mask with the number of voxels to shift down
            // at each pixel
            Data2D shiftMask = new Data2D(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = depth - 1; z >= 0; z--)
                    {
                        if (matrix[x, y, z])
                        {
                            shiftMask[x, y] = z;
                            break;
                        }
                    }
                }
            }

            // Iterate through pixels in x and y dimensions and shift voxels down
            // according to the shift mask
            List<Volume> corrected = new List<Volume>();

            foreach (Volume v in volumes)
            {
                Data3D originalData = v.Data;
                Data3D correctedData = new Data3D(width, height, depth);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int maskValue = (int)shiftMask[x, y];
                        int toShift = depth - maskValue;

                        for (int z = 0; z < depth; z++)
                        {
                            int newDepth = z + toShift;
                            if (newDepth >= depth) break;

                            correctedData[x, y, newDepth] = originalData[x, y, z];
                        }
                    }
                }

                corrected.Add(new Volume(correctedData, v.DataColor, 
                    v.VolumeName + string.Format(" - Z-Corrected ({0})", threshold)));
            }            

            return corrected;

            //Data3D maskVolume = new Data3D(width, height, depth);
            //for (int x = 0; x < width; x++)
            //{
            //    for (int y = 0; y < height; y++)
            //    {
            //        maskVolume[x, y, (int)shiftMask[x, y]] = 255;
            //    }
            //}
            //For testing: To return a volume that is the mask, use the following lines:
            //List<Volume> temp = new List<Volume>();
            //temp.Add(new Volume(maskVolume, volumes[0].DataColor, "Test"));
            //return temp;
        }
    }

    //1. Create base that is highest index of substrate in each voxel column
    //2. Shift lowest occuring voxel of non-substrate to index one less than base value

}
