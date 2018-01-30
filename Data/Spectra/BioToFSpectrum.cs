using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using ImagingSIMS.Common.Math;

namespace ImagingSIMS.Data.Spectra
{
    public class BioToFSpectrum : Spectrum
    {
        int[,] _array;
        BioToFParameters _parameters;

        readonly int _headerLength = 4656;
        // Include the 12 bytes for OS2 style date after the header
        readonly int _headerLengthOS2 = 1588 + 12;

        //readonly int binSize = 1; //Default to 1 ns bin size

        List<FlightTimeArray[,]> _xyts;

        public double TotalCounts
        {
            get
            {
                if (_xyts == null || _xyts.Count == 0) return 0;

                double totalCounts = 0;
                foreach (FlightTimeArray[,] fta in _xyts)
                {
                    foreach (FlightTimeArray ft in fta)
                    {
                        totalCounts += ft.CountsAtPixel;
                    }
                }
                return totalCounts;
            }
        }

        /// <summary>
        /// Gets an array of flight times and intensities.
        /// </summary>
        public int[,] Spectrum { get { return _array; } }
        /// <summary>
        /// Gets the first flight time in the spectrum.
        /// </summary>
        public int StartTime { get { return startTime; } }
        /// <summary>
        /// Gets the last flight time in the spectrum.
        /// </summary>
        public int EndTime { get { return endTime; } }
        /// <summary>
        /// Gets the range of flight times in the spectrum.
        /// </summary>
        public int TimeLength { get { return timeLength; } }

        /// <summary>
        /// Gets the slope value for mass calibration.
        /// </summary>
        public double MassSlope { get { return _parameters.Slope; } }
        /// <summary>
        /// Gets the intercept value for mass calibration.
        /// </summary>
        public double MassInt { get { return _parameters.Intercept; } }

        private int startTime
        {
            get { return _parameters.Bin1; }
        }
        private int endTime
        {
            get { return _parameters.Bin2; }
        }
        private int startBin
        {
            get { return _parameters.Bin1; }
        }
        private int endBin
        {
            get { return _parameters.Bin2; }
        }
        private double startMass
        {
            get { return TimeToMass(_parameters.Bin1); }
        }
        private double endMass
        {
            get { return TimeToMass(_parameters.Bin2); }
        }
        private int timeLength
        {
            get { return endTime - startTime; }
        }

        public BioToFSpectrum(string Name)
            : base(Name)
        {
            _specType = SpectrumType.BioToF;
        }

        public override void LoadFromFile(string FilePath, BackgroundWorker bw)
        {
            LoadFromFile(new string[1] { FilePath }, bw);
        }
        public override void LoadFromFile(string[] FilePaths, BackgroundWorker bw)
        {
            _isLoading = true;

            _xyts = new List<FlightTimeArray[,]>();

            doLoad(FilePaths, bw);

            _isLoading = false;
        }
        public void LoadFromFile(string FilePath, BackgroundWorker bw, bool IsOS2, string HeaderPath = null)
        {
            LoadFromFile(new string[1] { FilePath }, bw, IsOS2, HeaderPath);
        }
        public void LoadFromFile(string[] FilePaths, BackgroundWorker bw, bool IsOS2, string HeaderPath = null)
        {
            _isLoading = true;

            _xyts = new List<FlightTimeArray[,]>();

            doLoad(FilePaths, bw, IsOS2, HeaderPath);

            _isLoading = false;
        }

        public override Data2D FromMassRange(MassRange MassRange, out float Max, BackgroundWorker bw = null)
        {
            Data2D dt = new Data2D(_sizeX, _sizeY);

            List<Data2D> dts = FromMassRange(MassRange, "preview", true, bw);
            Max = -1;

            if (bw != null && bw.CancellationPending) return dt;

            float max = 0;
            for (int x = 0; x < _sizeX; x++)
            {
                for (int y = 0; y < _sizeY; y++)
                {
                    float sum = 0;
                    for (int z = 0; z < dts.Count; z++)
                    {
                        sum += dts[z][x, y];
                    }
                    dt[x, y] = sum;
                    if (sum > max) max = sum;
                }
                if (bw != null && bw.CancellationPending) return dt;
            }

            Max = max;
            dt.DataName = string.Format("{0} {1}-{2}", Name, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"));
            return dt;
        }
        public override Data2D FromMassRange(MassRange MassRange, BackgroundWorker bw = null)
        {
            Data2D dt = new Data2D(_sizeX, _sizeY);

            List<Data2D> dts = FromMassRange(MassRange, "preview", true, bw);

            if (bw != null && bw.CancellationPending) return dt;

            for (int x = 0; x < _sizeX; x++)
            {
                for (int y = 0; y < _sizeY; y++)
                {
                    float sum = 0;
                    for (int z = 0; z < dts.Count; z++)
                    {
                        sum += dts[z][x, y];
                    }
                    dt[x, y] = sum;
                }
                if (bw != null && bw.CancellationPending) return dt;
            }
            dt.DataName = string.Format("{0} {1}-{2}", Name, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"));
            return dt;
        }
        public override Data2D FromMassRange(MassRange MassRange, int Layer, BackgroundWorker bw = null)
        {
            if (_xyts.Count == 0)
            {
                throw new IndexOutOfRangeException("No xyt data has been loaded into memory.");
            }
            if (Layer >= SizeZ)
                throw new ArgumentException(string.Format("Layer {0} does not exist in the spectrum (Number layers: {1}).", Layer, SizeZ));

            List<Data2D> returnTables = new List<Data2D>();

            FlightTimeArray[,] xyt = _xyts[Layer];

            if (bw != null && bw.CancellationPending) return null;

            Data2D dt = new Data2D(_sizeX, _sizeY);

            for (int x = 0; x < _sizeX; x++)
            {
                if (bw != null && bw.CancellationPending) return null;

                for (int y = 0; y < _sizeY; y++)
                {
                    dt[x, y] = xyt[x, y].GetNumberCounts(MassToTime(_parameters.Slope, _parameters.Intercept, MassRange.StartMass),
                        MassToTime(_parameters.Slope, _parameters.Intercept, MassRange.EndMass));
                }
            }

            if (bw != null && bw.CancellationPending) return null;

            return dt;
        }
        public override List<Data2D> FromMassRange(MassRange MassRange, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null)
        {
            if (_xyts.Count == 0)
            {
                throw new IndexOutOfRangeException("No xyt data has been loaded into memory.");
            }

            List<Data2D> returnTables = new List<Data2D>();

            int i = 0;
            foreach (FlightTimeArray[,] xyt in _xyts)
            {
                if (bw != null && bw.CancellationPending) return returnTables;

                Data2D dt = new Data2D(_sizeX, _sizeY);

                for (int x = 0; x < _sizeX; x++)
                {
                    if (bw != null && bw.CancellationPending) return returnTables;

                    for (int y = 0; y < _sizeY; y++)
                    {
                        dt[x, y] = xyt[x, y].GetNumberCounts(MassToTime(_parameters.Slope, _parameters.Intercept, MassRange.StartMass),
                            MassToTime(_parameters.Slope, _parameters.Intercept, MassRange.EndMass));
                    }
                }

                if (bw != null && bw.CancellationPending) return returnTables;

                if (!OmitNumbering)
                    dt.DataName = string.Format("{0} {1}-{2} ({3})", TableBaseName, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"), ++i);
                else
                    dt.DataName = string.Format("{0} {1}-{2}", TableBaseName, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"));

                returnTables.Add(dt);
            }
            return returnTables;
        }
        public override Data2D FromMassRange(MassRange MassRange, int Layer, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null)
        {
            if (_xyts.Count == 0)
            {
                throw new IndexOutOfRangeException("No xyt data has been loaded into memory.");
            }
            if (Layer >= SizeZ)
                throw new ArgumentException(string.Format("Layer {0} does not exist in the spectrum (Number layers: {1}).", Layer, SizeZ));

            List<Data2D> returnTables = new List<Data2D>();

            FlightTimeArray[,] xyt = _xyts[Layer];

            if (bw != null && bw.CancellationPending) return null;

            Data2D dt = new Data2D(_sizeX, _sizeY);

            for (int x = 0; x < _sizeX; x++)
            {
                if (bw != null && bw.CancellationPending) return null;

                for (int y = 0; y < _sizeY; y++)
                {
                    dt[x, y] = xyt[x, y].GetNumberCounts(MassToTime(_parameters.Slope, _parameters.Intercept, MassRange.StartMass),
                        MassToTime(_parameters.Slope, _parameters.Intercept, MassRange.EndMass));
                }
            }

            if (bw != null && bw.CancellationPending) return null;

            if (!OmitNumbering)
                dt.DataName = string.Format("{0} {1}-{2} ({3})", TableBaseName, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"), Layer + 1);
            else
                dt.DataName = string.Format("{0} {1}-{2}", TableBaseName, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"));

            return dt;
        }

        private void doLoad(string[] FilePaths)
        {
            doLoad(FilePaths, null);
        }
        private void doLoad(string[] FilePaths, BackgroundWorker bw)
        {
            if (bw != null) bw.ReportProgress(0);

            int numFiles = FilePaths.Length;

            // Open each file and get header information
            BioToFParameters[] parameters = new BioToFParameters[numFiles];
            for (int i = 0; i < numFiles; i++)
            {
                using (Stream stream = File.OpenRead(FilePaths[i]))
                {
                    byte[] header = new byte[_headerLength];
                    stream.Read(header, 0, _headerLength);
                    parameters[i] = new BioToFParameters(header);
                }
            }

            // Only need to check for parameter consistency across files if
            // there are multiple files being read in.
            if (numFiles > 1)
            {
                // Check dimensions
                int pixels = -1;
                for (int i = 0; i < numFiles; i++)
                {
                    if (pixels == -1) pixels = parameters[i].Imaging_Pixels;

                    if (parameters[i].Imaging_Pixels != pixels)
                        throw new ArgumentException("Pixel dimensions do not match across all files.");
                }

                // Check mass calibration
                double slope = Double.NaN;
                double intercept = Double.NaN;
                for (int i = 0; i < numFiles; i++)
                {
                    if (Double.IsNaN(slope)) slope = parameters[i].Slope;
                    if (Double.IsNaN(intercept)) intercept = parameters[i].Intercept;

                    if (parameters[i].Slope != slope)
                        throw new ArgumentException("Mass calibration values do not match across all files.");
                    if (parameters[i].Intercept != intercept)
                        throw new ArgumentException("Mass calibration values do not match across all files.");
                }

                // Check file type
                int fileType = -1;
                for (int i = 0; i < numFiles; i++)
                {
                    if (fileType == -1) fileType = parameters[i].FileType;

                    if (parameters[i].FileType != fileType)
                        throw new ArgumentException("File type does not match across all files.");
                }

                // Check TD/TDC acquisition
                int td = -1;
                int tdc = -1;
                for (int i = 0; i < numFiles; i++)
                {
                    if (td == -1) td = parameters[i].TD;
                    if (tdc == -1) tdc = parameters[i].TDC;

                    if (parameters[i].TD != td)
                        throw new ArgumentException("Detector flags do not match across all files.");
                    if (parameters[i].TDC != tdc)
                        throw new ArgumentException("Detector flags do not match across all files.");
                }
            }

            // Set spectrum parameters to one instance of read in parameters.
            // Important parameters were verified in previous steps, so it shouldn't
            // matter which paramter struct is preserved.
            _parameters = parameters[0];

            // Determine total number of steps needed for read in
            // Total steps = read bytes + convert to ints + create FlightTimeArray
            // (3 steps for each file) plus one step at end for summing of FTAs
            int totalSteps = (numFiles * 3) + 1;
            int stepCt = 0;

            // Check if file is .dat (TOF) type. In which case, set # pixels to just 1
            bool isTOF = _parameters.FileType == 22;
            if (isTOF)
            {
                _parameters.Imaging_Pixels = 1;
            }

            foreach (string fileName in FilePaths)
            {
                if (bw != null) bw.ReportProgress(0);

                double bodyLength;
                byte[] bufferBody;
                using (Stream stream = File.OpenRead(fileName))
                {
                    bodyLength = stream.Length - _headerLength;
                    bufferBody = new byte[(int)bodyLength];

                    stream.Position = _headerLength;
                    stream.Read(bufferBody, 0, bufferBody.Length);
                }

                // Update progress after read in stream
                if (bw != null) bw.ReportProgress(Percentage.GetPercent(++stepCt, totalSteps));

                int[] bufferInts = new int[(int)bodyLength / (sizeof(int))];

                for (int ct = 0; ct < bufferInts.Length; ct++)
                {
                    int value = BitConverter.ToInt32(bufferBody, (ct * sizeof(int)));
                    bufferInts[ct] = value;
                }

                // Update progress after conversion to ints
                if (bw != null) bw.ReportProgress(Percentage.GetPercent(++stepCt, totalSteps));

                FlightTimeArray[,] readIn = new FlightTimeArray[_parameters.Imaging_Pixels, _parameters.Imaging_Pixels];

                int pos = 0;

                bool isTD = _parameters.TD == 1;
                for (int x = 0; x < _parameters.Imaging_Pixels; x++)
                {
                    for (int y = 0; y < _parameters.Imaging_Pixels; y++)
                    {
                        if (isTOF)
                        {
                            Dictionary<int, int> values = new Dictionary<int, int>();
                            for (int i = 0; i < bufferInts.Length; i++)
                            {
                                int intensity = bufferInts[i];
                                if (intensity != 0)
                                {
                                    values.Add(i, intensity);
                                }
                            }

                            int[] times = values.Keys.ToArray<int>();
                            int[] intensities = values.Values.ToArray<int>();

                            readIn[x, y] = new FlightTimeArray(times, intensities);
                            continue;
                        }

                        int xytLength = bufferInts[pos];
                        pos++;
                        int[] xyt = new int[xytLength];
                        for (int z = 0; z < xytLength; z++)
                        {
                            xyt[z] = bufferInts[pos];
                            pos++;
                        }

                        if (isTD)
                        {
                            int xytiTotal = bufferInts[pos];
                            pos++;

                            int[] xyti = new int[xytLength];

                            for (int z = 0; z < xytLength; z++)
                            {
                                xyti[z] = bufferInts[pos];
                                pos++;
                            }

                            readIn[x, y] = new FlightTimeArray(xyt, xyti);
                        }

                        else readIn[x, y] = new FlightTimeArray(xyt);
                    }
                }

                _xyts.Add(readIn);

                // Update progress after converting to FlightTimeArrays
                if (bw != null) bw.ReportProgress(Percentage.GetPercent(++stepCt, totalSteps));
            }

            // TODO: Get real start and stop times/masses
            // _startTime = (int)MassToTime(_massSlope, _massInt, 0);
            // _endTime = (int)MassToTime(_massSlope, _massInt, 1000);            

            _startMass = (float)startMass;
            _endMass = (float)endMass;

            if (bw != null) bw.ReportProgress(0);
            _array = new int[timeLength, 2];

            for (int x = 0; x < timeLength; x++)
            {
                _array[x, 0] = x + startTime;
            }

            _sizeX = _xyts[0].GetLength(0);
            _sizeY = _xyts[0].GetLength(1);
            _sizeZ = FilePaths.Length;

            foreach (FlightTimeArray[,] spectrum in _xyts)
            {
                for (int x = 0; x < spectrum.GetLength(0); x++)
                {
                    for (int y = 0; y < spectrum.GetLength(1); y++)
                    {
                        for (int z = 0; z < spectrum[x, y].FlightTimeLength; z++)
                        {
                            int time;
                            int intensity = spectrum[x, y].GetIntensity(z, out time);
                            if (time > startTime && time <= endTime)
                            {
                                _array[time - startTime - 1, 1] += intensity;
                            }
                        }
                    }
                }
            }

            // Update progress: load process complete
            if (bw != null) bw.ReportProgress(100);
        }
        private void doLoad(string[] filePaths, bool isOS2, string headerFile = null)
        {
            doLoad(filePaths, null, isOS2, headerFile);
        }
        private void doLoad(string[] filePaths, BackgroundWorker bw, bool isOS2, string headerFile = null)
        {
            if (bw != null) bw.ReportProgress(0);

            int numFiles = filePaths.Length;

            if (headerFile != null)
            {
                using (Stream stream = File.OpenRead(headerFile))
                {
                    byte[] header = new byte[_headerLength];
                    stream.Read(header, 0, _headerLength);
                    _parameters = new BioToFParameters(header);
                }
            }
            else
            {
                _parameters = new BioToFParameters();
            }

            // SEt parameters for OS2 file
            _parameters.FileType = 25;
            _parameters.Imaging_Pixels = 128;
            _parameters.TD = 0;
            _parameters.Slope = 0.058105655;
            _parameters.Intercept = -0.05;
            _parameters.BinStart = 30;
            _parameters.BinEnd = 65536;
            _parameters.Bin1 = 30;
            _parameters.Bin2 = 65536;
            _parameters.Detector = 1408;

            // Determine total number of steps needed for read in
            // Total steps = read bytes + convert to ints + create FlightTimeArray
            // (3 steps for each file) plus one step at end for summing of FTAs
            int totalSteps = (numFiles * 3) + 1;
            int stepCt = 0;

            // Check if file is .dat (TOF) type. In which case, set # pixels to just 1
            bool isTOF = _parameters.FileType == 22;
            if (isTOF)
            {
                _parameters.Imaging_Pixels = 1;
            }

            foreach (string fileName in filePaths)
            {
                if (bw != null) bw.ReportProgress(0);

                double bodyLength;
                byte[] bufferBody;
                using (Stream stream = File.OpenRead(fileName))
                {
                    bodyLength = stream.Length - _headerLengthOS2;
                    bufferBody = new byte[(int)bodyLength];

                    stream.Position = _headerLengthOS2;
                    stream.Read(bufferBody, 0, bufferBody.Length);
                }

                // Update progress after read in stream
                if (bw != null) bw.ReportProgress(Percentage.GetPercent(++stepCt, totalSteps));

                int[] bufferInts = new int[(int)bodyLength / (sizeof(int))];

                for (int ct = 0; ct < bufferInts.Length; ct++)
                {
                    int value = BitConverter.ToInt32(bufferBody, (ct * sizeof(int)));
                    bufferInts[ct] = value;
                }

                // Update progress after conversion to ints
                if (bw != null) bw.ReportProgress(Percentage.GetPercent(++stepCt, totalSteps));

                FlightTimeArray[,] readIn = new FlightTimeArray[_parameters.Imaging_Pixels, _parameters.Imaging_Pixels];

                int pos = 0;

                bool isTD = _parameters.TD == 1;
                for (int x = 0; x < _parameters.Imaging_Pixels; x++)
                {
                    for (int y = 0; y < _parameters.Imaging_Pixels; y++)
                    {
                        if (isTOF)
                        {
                            Dictionary<int, int> values = new Dictionary<int, int>();
                            for (int i = 0; i < bufferInts.Length; i++)
                            {
                                int intensity = bufferInts[i];
                                if (intensity != 0)
                                {
                                    values.Add(i, intensity);
                                }
                            }

                            int[] times = values.Keys.ToArray<int>();
                            int[] intensities = values.Values.ToArray<int>();

                            readIn[x, y] = new FlightTimeArray(times, intensities);
                            continue;
                        }

                        int xytLength = bufferInts[pos];
                        pos++;
                        int[] xyt = new int[xytLength];
                        for (int z = 0; z < xytLength; z++)
                        {
                            xyt[z] = bufferInts[pos];
                            pos++;
                        }

                        if (isTD)
                        {
                            int xytiTotal = bufferInts[pos];
                            pos++;

                            int[] xyti = new int[xytLength];

                            for (int z = 0; z < xytLength; z++)
                            {
                                xyti[z] = bufferInts[pos];
                                pos++;
                            }

                            readIn[x, y] = new FlightTimeArray(xyt, xyti);
                        }

                        else readIn[x, y] = new FlightTimeArray(xyt);
                    }
                }

                _xyts.Add(readIn);

                // Update progress after converting to FlightTimeArrays
                if (bw != null) bw.ReportProgress(Percentage.GetPercent(++stepCt, totalSteps));
            }

            // TODO: Get real start and stop times/masses
            // _startTime = (int)MassToTime(_massSlope, _massInt, 0);
            // _endTime = (int)MassToTime(_massSlope, _massInt, 1000);            

            _startMass = (float)startMass;
            _endMass = (float)endMass;

            if (bw != null) bw.ReportProgress(0);
            _array = new int[timeLength, 2];

            for (int x = 0; x < timeLength; x++)
            {
                _array[x, 0] = x + startTime;
            }

            _sizeX = _xyts[0].GetLength(0);
            _sizeY = _xyts[0].GetLength(1);
            _sizeZ = filePaths.Length;

            foreach (FlightTimeArray[,] spectrum in _xyts)
            {
                for (int x = 0; x < spectrum.GetLength(0); x++)
                {
                    for (int y = 0; y < spectrum.GetLength(1); y++)
                    {
                        for (int z = 0; z < spectrum[x, y].FlightTimeLength; z++)
                        {
                            int time;
                            int intensity = spectrum[x, y].GetIntensity(z, out time);
                            if (time > startTime && time <= endTime)
                            {
                                _array[time - startTime - 1, 1] += intensity;
                            }
                        }
                    }
                }
            }

            // Update progress: load process complete
            if (bw != null) bw.ReportProgress(100);
        }

        public double TimeToMass(double Time)
        {
            return _parameters.Slope * ((Time / 1000) + _parameters.Intercept) * ((Time / 1000) + _parameters.Intercept);
        }
        public int MassToTime(double MassValue)
        {
            return (int)((Math.Sqrt(MassValue / _parameters.Slope) - _parameters.Intercept) * 1000);
        }
        public double MassToTime(double MassSlope, double MassInt, double MassValue)
        {
            return ((Math.Sqrt(MassValue / MassSlope) - MassInt) * 1000);
        }
        public int MassToBin(double MassValue)
        {
            return MassToTime(MassValue);
        }

        public double MassValue(int Index)
        {
            return TimeToMass(_array[Index, 0] / 1000);
        }

        public override double[,] ToDoubleArray()
        {
            double[,] returnArray = new double[_array.Length / 2, 2];
            for (int i = 0; i < _array.Length / 2; i++)
            {
                returnArray[i, 0] = (double)(TimeToMass((double)_array[i, 0]));
                returnArray[i, 1] = _array[i, 1];
            }
            return returnArray;
        }
        public override uint[] GetSpectrum(out float[] Masses)
        {
            float[] massesOut = new float[_array.Length / 2];
            uint[] valuesOut = new uint[_array.Length / 2];

            for (int i = 0; i < _array.Length / 2; i++)
            {
                massesOut[i] = (float)(TimeToMass((double)_array[i, 0]));
                valuesOut[i] = (uint)_array[i, 1];
            }

            if (_masses == null) Masses = massesOut;
            else Masses = _masses;

            return valuesOut;
        }

        public override double[,] GetPxMMatrix()
        {
            return GetPxMMatrix(new double[] { 0.5d }, new double[] { 0.5d }, StartMass, EndMass);
        }
        public override double[,] GetPxMMatrix(double[] binCenters, double[] binWidths)
        {
            return GetPxMMatrix(binCenters, binWidths, StartMass, EndMass);
        }
        public override double[,] GetPxMMatrix(double[] binCenters, double[] binWidths, double startMass, double endMass)
        {
            if (binCenters.Length != binWidths.Length)
                throw new ArgumentException("Numbers of centers and widths do not match");

            foreach (var center in binCenters)
            {
                if (center < 0d || center > 1.0d)
                    throw new ArgumentException("Bin centers must be between 0 and 1");
            }

            if (startMass >= endMass)
                throw new ArgumentException("Invalid mass range");

            int layerLinearLength = SizeX * SizeY;
            int numMassChannels = binCenters.Length * (int)(endMass - startMass + 1);

            double[,] matrix = new double[layerLinearLength * SizeZ, numMassChannels];

            for (int m = (int)startMass; m < endMass; m++)
            {
                for (int i = 0; i < binCenters.Length; i++)
                {
                    for (int z = 0; z < SizeZ; z++)
                    {
                        int massPosition = m * binCenters.Length + i;
                        var intensities = FromMassRange(new MassRange(m - binCenters[i], m + binCenters[i]));

                        for (int x = 0; x < intensities.Width; x++)
                        {
                            for (int y = 0; y < intensities.Height; y++)
                            {
                                int pixel = z * intensities.Width * intensities.Height + x * intensities.Height + y;
                                matrix[pixel, massPosition] = intensities[x, y];
                            }
                        }
                    }
                }
            }

            return matrix;
        }

        /// <summary>
        /// Removes mass spectra from unwanted pixels.
        /// </summary>
        /// <param name="PixelsToKeep">Pixels in the image to keep.</param>
        /// <param name="Layer">Depth layer to perform crop operation on.</param>
        /// <returns>Cropped Bio-ToF spectrum.</returns>
        public BioToFSpectrum Crop(List<Point> PixelsToKeep, int Layer)
        {
            BioToFSpectrum cropped = new BioToFSpectrum(this.Name + " - Cropped");
            cropped._parameters = _parameters;
            cropped._startMass = _startMass;
            cropped._endMass = _endMass;
            cropped._sizeX = _sizeX;
            cropped._sizeY = _sizeY;
            cropped._sizeZ = _sizeZ;
            cropped._specType = _specType;

            //Crop all layers
            if (Layer == -1)
            {
                cropped._sizeZ = this._sizeZ;
                cropped._xyts = new List<FlightTimeArray[,]>();

                for (int z = 0; z < SizeZ; z++)
                {
                    FlightTimeArray[,] xyt = new FlightTimeArray[SizeX, SizeY];
                    for (int x = 0; x < SizeX; x++)
                    {
                        for (int y = 0; y < SizeY; y++)
                        {
                            xyt[x, y] = FlightTimeArray.Empty;
                        }
                    }

                    foreach (Point p in PixelsToKeep)
                    {
                        int x = (int)p.X;
                        int y = (int)p.Y;

                        xyt[x, y] = this._xyts[z][x, y].Copy();
                    }

                    cropped._xyts.Add(xyt);
                }
            }

            //Only crop single layer
            else
            {
                cropped._sizeZ = 1;
                cropped._xyts = new List<FlightTimeArray[,]>();

                FlightTimeArray[,] xyt = new FlightTimeArray[SizeX, SizeY];
                for (int x = 0; x < SizeX; x++)
                {
                    for (int y = 0; y < SizeY; y++)
                    {
                        xyt[x, y] = FlightTimeArray.Empty;
                    }
                }

                foreach (Point p in PixelsToKeep)
                {
                    int x = (int)p.X;
                    int y = (int)p.Y;

                    xyt[x, y] = this._xyts[Layer][x, y].Copy();
                }

                cropped._xyts.Add(xyt);
            }

            cropped._array = new int[cropped.timeLength, 2];

            for (int x = 0; x < cropped.timeLength; x++)
            {
                cropped._array[x, 0] = x + cropped.startTime;
            }

            foreach (FlightTimeArray[,] spectrum in cropped._xyts)
            {
                for (int x = 0; x < cropped._sizeX; x++)
                {
                    for (int y = 0; y < cropped._sizeY; y++)
                    {
                        for (int z = 0; z < spectrum[x, y].FlightTimeLength; z++)
                        {
                            int time;
                            int intensity = spectrum[x, y].GetIntensity(z, out time);
                            if (time > cropped.startTime && time <= cropped.endTime)
                            {
                                cropped._array[time - cropped.startTime - 1, 1] += intensity;
                            }
                        }
                    }
                }
            }

            return cropped;
        }
        /// <summary>
        /// Removes mass spectra from unwanted pixels.
        /// </summary>
        /// <param name="PixelsToKeep">Pixels in the image to keep.</param>
        /// <param name="Layer">Depth layer to perform crop operation on.</param>
        /// <param name="ResizeBuffer">Padding to add to the resized cropped image.</param>
        /// <returns>Cropped and resized Bio-ToF spectrum</returns>
        public BioToFSpectrum CropAndResize(List<Point> PixelsToKeep, int Layer, int ResizeBuffer)
        {
            BioToFSpectrum cropped = new BioToFSpectrum(this.Name + " - Cropped");
            cropped._parameters = _parameters;
            cropped._startMass = _startMass;
            cropped._endMass = _endMass;
            cropped._sizeX = _sizeX;
            cropped._sizeY = _sizeY;
            cropped._sizeZ = _sizeZ;
            cropped._specType = _specType;

            //Determine resize start and end points
            int resizeStartX;
            int resizeStartY;
            int resizeEndX;
            int resizeEndY;
            int resizeWidth;
            int resizeHeight;

            List<int> pixelsX = new List<int>();
            List<int> pixelsY = new List<int>();
            foreach (Point p in PixelsToKeep)
            {
                pixelsX.Add((int)p.X);
                pixelsY.Add((int)p.Y);
            }
            resizeStartX = Math.Max(0, pixelsX.Min() - ResizeBuffer);
            resizeEndX = Math.Min(this.SizeX, pixelsX.Max() + ResizeBuffer);
            resizeStartY = Math.Max(0, pixelsY.Min() - ResizeBuffer);
            resizeEndY = Math.Min(this.SizeY, pixelsY.Max() + ResizeBuffer);
            resizeWidth = (resizeEndX - resizeStartX) + 1;
            resizeHeight = (resizeEndY - resizeStartY) + 1;

            cropped._sizeX = resizeWidth;
            cropped._sizeY = resizeHeight;

            //Crop all layers
            if (Layer == -1)
            {
                cropped._sizeZ = this._sizeZ;
                cropped._xyts = new List<FlightTimeArray[,]>();

                for (int z = 0; z < SizeZ; z++)
                {
                    FlightTimeArray[,] xyt = new FlightTimeArray[resizeWidth, resizeHeight];
                    for (int x = 0; x < resizeWidth; x++)
                    {
                        for (int y = 0; y < resizeHeight; y++)
                        {
                            xyt[x, y] = FlightTimeArray.Empty;
                        }
                    }

                    foreach (Point p in PixelsToKeep)
                    {
                        int origX = (int)p.X;
                        int origY = (int)p.Y;
                        int cropX = origX - resizeStartX;
                        int cropY = origY - resizeStartY;

                        xyt[cropX, cropY] = this._xyts[z][origX, origY].Copy();
                    }

                    cropped._xyts.Add(xyt);
                }
            }

            //Only crop single layer
            else
            {
                cropped._sizeZ = 1;
                cropped._xyts = new List<FlightTimeArray[,]>();

                FlightTimeArray[,] xyt = new FlightTimeArray[resizeWidth, resizeHeight];
                for (int x = 0; x < resizeWidth; x++)
                {
                    for (int y = 0; y < resizeHeight; y++)
                    {
                        xyt[x, y] = FlightTimeArray.Empty;
                    }
                }

                foreach (Point p in PixelsToKeep)
                {
                    int origX = (int)p.X;
                    int origY = (int)p.Y;
                    int cropX = origX - resizeStartX;
                    int cropY = origY - resizeStartY;

                    xyt[cropX, cropY] = this._xyts[Layer][origX, origY].Copy();
                }

                cropped._xyts.Add(xyt);
            }

            cropped._array = new int[cropped.timeLength, 2];

            for (int x = 0; x < cropped.timeLength; x++)
            {
                cropped._array[x, 0] = x + cropped.startTime;
            }

            foreach (FlightTimeArray[,] spectrum in cropped._xyts)
            {
                for (int x = 0; x < cropped._sizeX; x++)
                {
                    for (int y = 0; y < cropped._sizeY; y++)
                    {
                        for (int z = 0; z < spectrum[x, y].FlightTimeLength; z++)
                        {
                            int time;
                            int intensity = spectrum[x, y].GetIntensity(z, out time);
                            if (time > cropped.startTime && time <= cropped.endTime)
                            {
                                cropped._array[time - cropped.startTime - 1, 1] += intensity;
                            }
                        }
                    }
                }
            }

            return cropped;
        }

        public void Save(string filePath)
        {
            bool isTD = _parameters.TD == 1;
            for (int z = 0; z < _xyts.Count; z++)
            {
                string layerFilePath = string.Empty;
                if (_xyts.Count > 1) layerFilePath = filePath.Insert(filePath.Length - 4, " - " + (z + 1).ToString());
                else layerFilePath = filePath;

                using (Stream stream = File.OpenWrite(layerFilePath))
                {
                    BinaryWriter bw = new BinaryWriter(stream);

                    byte[] header = _parameters.ToByteArray();
                    bw.Write(header);

                    FlightTimeArray[,] currentLayer = _xyts[z];

                    for (int x = 0; x < _parameters.Imaging_Pixels; x++)
                    {
                        for (int y = 0; y < _parameters.Imaging_Pixels; y++)
                        {
                            bw.Write(currentLayer[x, y].FlightTimeLength);
                            for (int i = 0; i < currentLayer[x, y].FlightTimeLength; i++)
                            {
                                bw.Write(currentLayer[x, y].GetFlightTime(i));
                            }
                            if (isTD)
                            {
                                for (int i = 0; i < currentLayer[x, y].FlightTimeLength; i++)
                                {
                                    bw.Write(currentLayer[x, y].GetIntensity(i));
                                }
                            }
                        }
                    }
                }
            }
        }
        public void SaveHeader(string filePath)
        {
            using (Stream stream = File.OpenWrite(filePath))
            {
                BinaryWriter bw = new BinaryWriter(stream);

                byte[] header = _parameters.ToByteArray();
                bw.Write(header);
            }
        }

        public override void SaveText(string path)
        {
            SaveText(path, 1);
        }
        public override void SaveText(string path, int binSize)
        {
            float[] masses;
            uint[] intensities = GetSpectrum(out masses);

            using (StreamWriter sw = new StreamWriter(path))
            {
                int numDataPoints = masses.Length / binSize;
                if (masses.Length % binSize != 0) numDataPoints++;

                int ct = 0;
                for (int i = 0; i < numDataPoints; i++)
                {
                    uint sum = 0;
                    for (int j = 0; j < binSize; j++)
                    {
                        if (ct >= intensities.Length) break;

                        sum += intensities[i * binSize + j];
                        ct++;
                    }
                    sw.WriteLine(string.Format("{0},{1}", masses[i * binSize], sum));
                }
            }
        }

        public void SaveXYZT(string fileName)
        {
            using (Stream stream = File.OpenWrite(fileName))
            {
                BinaryWriter bw = new BinaryWriter(stream);

                bw.Write(SizeX);
                bw.Write(SizeY);
                bw.Write(SizeZ);

                for (int x = 0; x < SizeX; x++)
                {
                    for (int y = 0; y < SizeY; y++)
                    {
                        for (int z = 0; z < SizeZ; z++)
                        {
                            FlightTimeArray fta = _xyts[z][x, y];

                            int[] intensities = fta.Intensities;
                            int[] flightTimes = fta.FlightTimes;

                            bw.Write(flightTimes.Length);

                            for (int w = 0; w < flightTimes.Length; w++)
                            {
                                bw.Write(TimeToMass(flightTimes[w]));
                                bw.Write(intensities[w]);
                            }
                        }
                    }
                }
            }
        }

        #region ISavable

        // Layout:
        // (SpectrumType)       SpectrumType
        // (string)             SpectrumName
        // (int)                BioToFParameters size
        // (BioToFParamaters)   Paramaters
        // (int)                Width
        // (int)                Height
        // (int)                Depth
        // (FlightTimeArray[,]) Data
        // (int[,])             Array

        public override byte[] ToByteArray()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(ms);

                // Offset start of writing to later go back and 
                // prefix the stream with the size of the stream written
                bw.Seek(sizeof(int), SeekOrigin.Begin);

                // Write contents of object
                bw.Write((int)SpectrumType);
                bw.Write(Name);

                // Need to include the size of the BioToFParameters
                bw.Write(System.Runtime.InteropServices.Marshal.SizeOf(_parameters));
                bw.Write(_parameters.ToByteArray());

                bw.Write(SizeX);
                bw.Write(SizeY);
                bw.Write(SizeZ);

                bool isTD = _parameters.TD == 1;
                for (int z = 0; z < SizeZ; z++)
                {
                    FlightTimeArray[,] currentLayer = _xyts[z];

                    for (int x = 0; x < SizeX; x++)
                    {
                        for (int y = 0; y < SizeY; y++)
                        {
                            bw.Write(currentLayer[x, y].FlightTimeLength);
                            for (int i = 0; i < currentLayer[x, y].FlightTimeLength; i++)
                            {
                                bw.Write(currentLayer[x, y].GetFlightTime(i));
                            }
                            if (isTD)
                            {
                                for (int i = 0; i < currentLayer[x, y].FlightTimeLength; i++)
                                {
                                    bw.Write(currentLayer[x, y].GetIntensity(i));
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < timeLength; i++)
                {
                    bw.Write(_array[i, 0]);
                    bw.Write(_array[i, 1]);
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

        // Layout:
        // (SpectrumType)       SpectrumType
        // (string)             SpectrumName
        // (int)                BioToFParameters size
        // (BioToFParamaters)   Paramaters
        // (int)                Width
        // (int)                Height
        // (int)                Depth
        // (FlightTimeArray[,]  Data
        // (int[,])             Array

        public override void FromByteArray(byte[] array)
        {
            using (MemoryStream ms = new MemoryStream(array))
            {
                BinaryReader br = new BinaryReader(ms);

                SpectrumType spectrumType = (SpectrumType)br.ReadInt32();
                string spectrumName = br.ReadString();

                int paramsSize = br.ReadInt32();
                byte[] buffer = br.ReadBytes(paramsSize);
                BioToFParameters parameters = new BioToFParameters(buffer);

                int width = br.ReadInt32();
                int height = br.ReadInt32();
                int depth = br.ReadInt32();

                this.Name = spectrumName;
                this._specType = spectrumType;
                this._parameters = parameters;
                this._sizeX = width;
                this._sizeY = height;
                this._sizeZ = depth;

                bool isTD = _parameters.TD == 1;

                _xyts = new List<FlightTimeArray[,]>();

                for (int z = 0; z < depth; z++)
                {
                    FlightTimeArray[,] currentLayer = new FlightTimeArray[width, height];
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            int length = br.ReadInt32();

                            int[] xyt = new int[length];
                            for (int i = 0; i < length; i++)
                            {
                                xyt[i] = br.ReadInt32();
                            }

                            if (isTD)
                            {
                                int[] xyti = new int[length];
                                for (int i = 0; i < length; i++)
                                {
                                    xyti[i] = br.ReadInt32();
                                }

                                currentLayer[x, y] = new FlightTimeArray(xyt, xyti);
                            }
                            else
                            {
                                currentLayer[x, y] = new FlightTimeArray(xyt);
                            }

                        }
                    }

                    _xyts.Add(currentLayer);
                }

                _array = new int[timeLength, 2];
                for (int i = 0; i < timeLength; i++)
                {
                    _array[i, 0] = br.ReadInt32();
                    _array[i, 1] = br.ReadInt32();
                }

                _startMass = (float)startMass;
                _endMass = (float)endMass;
            }
        }
        #endregion
    }

    public class FlightTimeArray
    {
        int[] _flightTimes;
        int[] _intensities;

        int _totalCounts;

        public int[] FlightTimes
        {
            get { return _flightTimes; }
        }
        public int[] Intensities
        {
            get { return _intensities; }
        }

        public int CountsAtPixel
        {
            get { return _totalCounts; }
        }
        public int FlightTimeLength
        {
            get
            {
                if (_flightTimes == null) return 0;
                return _flightTimes.Length;
            }
        }

        public int this[int index]
        {
            get
            {
                return GetIntensity(index);
            }
        }
        public int GetIntensity(int index)
        {
            if (_intensities == null) return 0;
            return _intensities[index];
        }
        public int GetIntensity(int index, out int flightTime)
        {
            flightTime = 0;

            if (_flightTimes == null) return 0;

            flightTime = _flightTimes[index];
            return GetIntensity(index);
        }
        public int GetFlightTime(int index)
        {
            if (_flightTimes == null) return 0;
            return _flightTimes[index];
        }

        public FlightTimeArray(int[] flightTimes, int[] intensities)
        {
            _flightTimes = flightTimes;
            _intensities = intensities;

            _totalCounts = getTotalCounts();
        }
        public FlightTimeArray(int[] flightTimes)
        {
            _flightTimes = convertFlightTimes(out _intensities, flightTimes);

            _totalCounts = getTotalCounts();
        }

        private int getTotalCounts()
        {
            int counts = 0;
            for (int i = 0; i < FlightTimeLength; i++)
            {
                counts += _flightTimes[i];
            }
            return counts;
        }
        private int[] convertFlightTimes(out int[] intensities, int[] flightTimes)
        {
            List<int> temp = new List<int>();
            foreach (int flightTime in flightTimes)
            {
                if (!temp.Contains(flightTime)) temp.Add(flightTime);
            }

            int[] convertedFlightTimes = temp.ToArray<int>();

            intensities = new int[convertedFlightTimes.Length];
            foreach (int flightTime in flightTimes)
            {
                int index = temp.IndexOf(flightTime);
                intensities[index]++;
            }

            return convertedFlightTimes;
        }

        public int GetNumberCounts(int StartBin, int EndBin)
        {
            if (_flightTimes == null || _intensities == null) return 0;

            int count = 0;
            for (int i = 0; i < FlightTimeLength; i++)
            {
                int time = _flightTimes[i];

                if (time >= StartBin && time <= EndBin)
                {
                    count += _intensities[i];
                }
            }

            return count;
        }
        public int GetNumberCounts(double StartTime, double EndTime)
        {
            if (_flightTimes == null || _intensities == null) return 0;

            int count = 0;
            for (int i = 0; i < FlightTimeLength; i++)
            {
                int time = _flightTimes[i];

                if (time >= StartTime && time <= EndTime)
                {
                    count += _intensities[i];
                }
            }
            return count;
        }

        public static FlightTimeArray Empty
        {
            get
            {
                return new FlightTimeArray(null, null);
            }
        }
        public override string ToString()
        {
            return string.Format("FlightTimeArray - Counts: {0}", CountsAtPixel);
        }

        public FlightTimeArray Copy()
        {
            return new FlightTimeArray(_flightTimes, _intensities);
        }
    }
}
