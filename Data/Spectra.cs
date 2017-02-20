using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using ImagingSIMS.Common;
using ImagingSIMS.Common.Math;
using ImagingSIMS.Data.Spectra.CamecaHeaders;

namespace ImagingSIMS.Data.Spectra
{

    //struct LoadWorkerArguments
    //{
    //    public string[] FilePaths;
    //    public int BinStart;
    //    public int BinEnd;

    //    public void LoadWorkerResults()
    //    {
    //        BinStart = 0;
    //        BinEnd = 0;
    //        FilePaths = null;
    //    }
    //}

    //public delegate void MSLoadUpdatedEventHandler(object sender, MSLoadUpdatedEventArgs args);
    //public class MSLoadUpdatedEventArgs : EventArgs
    //{
    //    public int Percentage;
    //    public EventArgs e;


    public interface ICamecaSpectrum
    {
        Data2D FromSpecies(CamecaSpecies species, BackgroundWorker bw = null);
        Task<Data2D> FromSpeciesAsync(CamecaSpecies species);
        Data2D FromSpecies(CamecaSpecies species, out float max, BackgroundWorker bw = null);
        ICollection<Data2D> FromSpecies(CamecaSpecies species, string tableBaseName, bool omitNumbering, BackgroundWorker bw = null);
        Task<ICollection<Data2D>> FromSpeciesAsync(CamecaSpecies species, string tableBaseName, bool omitNumbering);
        Data2D FromSpecies(CamecaSpecies species, int layer, string tableBaseName, bool omitNumbering, BackgroundWorker bw = null);
        Task<Data2D> FromSpeciesAsync(CamecaSpecies species, int layer, string tableBaseName, bool omitNumbering);
        Data3D FromSpecies(CamecaSpecies species, string tableName, BackgroundWorker bw = null);
        Task<Data3D> FromSpeciesAsync(CamecaSpecies species, string tableName);
    }

    public abstract class CamecaSpectrum : Spectrum, ICamecaSpectrum
    {
        protected List<CamecaSpecies> _species;

        // _matrix[layer][species][x,y]
        protected List<Data2D[]> _matrix;

        public List<CamecaSpecies> Species
        {
            get { return _species; }
        }

        public int NumberSpecies
        {
            get
            {
                if (_species == null) return 0;
                return _species.Count;
            }
        }

        protected void setMatrixValue(int x, int y, int z, int species, float value)
        {
            _matrix[z][species][x, y] = value;
        }
        protected float getMatrixValue(int x, int y, int z, int species)
        {
            return _matrix[z][species][x, y];
        }
        protected float getMatrixValue(int x, int y, int z, MassRangePair range)
        {
            float value = 0;
            for (int i = 0; i < _species.Count; i++)
            {
                if (_species[i].Mass >= range.StartMass && _species[i].Mass < range.EndMass)
                {
                    value += getMatrixValue(x, y, z, i);
                }

            }

            return value;
        }

        public CamecaSpectrum(string Name) : base(Name)
        {
        }

        public Data2D FromSpecies(CamecaSpecies species, BackgroundWorker bw = null)
        {
            Data2D d = FromMassRange(new MassRangePair(species.Mass - 0.1d, species.Mass + 0.1d));
            return d;
        }
        public Data2D FromSpecies(CamecaSpecies species, out float Max, BackgroundWorker bw = null)
        {
            Data2D d = FromMassRange(new MassRangePair(species.Mass - 0.1d, species.Mass + 0.1d), out Max, bw);
            return d;
        }
        public ICollection<Data2D> FromSpecies(CamecaSpecies species, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null)
        {
            return FromMassRange(new MassRangePair(species.Mass - 0.1d, species.Mass + 0.1d), TableBaseName, OmitNumbering, bw);
        }
        public Data2D FromSpecies(CamecaSpecies species, int Layer, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null)
        {
            return FromMassRange(new MassRangePair(species.Mass - 0.1d, species.Mass + 0.1d), Layer, TableBaseName, OmitNumbering, bw);
        }
        public Data3D FromSpecies(CamecaSpecies species, string tableName, BackgroundWorker bw = null)
        {
            return FromMassRange(new MassRangePair(species.Mass - 0.1d, species.Mass + 0.1d), tableName, bw);
        }

        public async Task<Data2D> FromSpeciesAsync(CamecaSpecies species)
        {
            return await Task.Run(() => FromSpecies(species));
        }
        public async Task<Data3D> FromSpeciesAsync(CamecaSpecies species, string tableName)
        {
            return await Task.Run(() => FromSpecies(species, tableName));
        }
        public async Task<ICollection<Data2D>> FromSpeciesAsync(CamecaSpecies species, string tableBaseName, bool omitNumbering)
        {
            return await Task.Run(() => FromSpecies(species, tableBaseName, omitNumbering));
        }
        public async Task<Data2D> FromSpeciesAsync(CamecaSpecies species, int layer, string tableBaseName, bool omitNumbering)
        {
            return await Task.Run(() => FromSpecies(species, layer, tableBaseName, omitNumbering));
        }

        public override Data2D FromMassRange(MassRangePair MassRange, out float Max, BackgroundWorker bw = null)
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
        public override Data2D FromMassRange(MassRangePair MassRange, BackgroundWorker bw = null)
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
        public override Data2D FromMassRange(MassRangePair MassRange, int Layer, BackgroundWorker bw = null)
        {
            if (_matrix == null || _matrix.Count == 0)
            {
                throw new IndexOutOfRangeException("No xyt data has been loaded into memory.");
            }
            if (Layer >= SizeZ)
                throw new ArgumentException(string.Format("Layer {0} does not exist in the spectrum (Number layers: {1}).", Layer, SizeZ));


            Data2D d = new Data2D(SizeX, SizeY);
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    d[x, y] = getMatrixValue(x, y, Layer, MassRange);
                }
            }

            if (bw != null && bw.CancellationPending) return null;

            return d;
        }
        public override List<Data2D> FromMassRange(MassRangePair MassRange, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null)
        {
            if (_matrix == null || _matrix.Count == 0)
            {
                throw new IndexOutOfRangeException("No xyt data has been loaded into memory.");
            }

            List<Data2D> returnTables = new List<Data2D>();

            int i = 0;
            for (int z = 0; z < _sizeZ; z++)
            {
                Data2D d = new Data2D(SizeX, SizeY);

                for (int x = 0; x < SizeX; x++)
                {
                    for (int y = 0; y < SizeY; y++)
                    {
                        d[x, y] = getMatrixValue(x, y, z, MassRange);
                    }
                }

                if (bw != null && bw.CancellationPending) return returnTables;

                if (!OmitNumbering)
                    d.DataName = string.Format("{0} {1}-{2} ({3})", TableBaseName, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"), ++i);
                else
                    d.DataName = string.Format("{0} {1}-{2}", TableBaseName, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"));

                returnTables.Add(d);

                if (bw != null)
                    bw.ReportProgress(z * 100 / _sizeZ);
            }

            return returnTables;
        }
        public override Data2D FromMassRange(MassRangePair MassRange, int Layer, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null)
        {
            if (_matrix == null || _matrix.Count == 0)
            {
                throw new IndexOutOfRangeException("No xyt data has been loaded into memory.");
            }
            if (Layer >= SizeZ)
                throw new ArgumentException(string.Format("Layer {0} does not exist in the spectrum (Number layers: {1}).", Layer, SizeZ));


            Data2D d = new Data2D(SizeX, SizeY);
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    d[x, y] = getMatrixValue(x, y, Layer, MassRange);
                }
            }

            if (bw != null && bw.CancellationPending) return null;

            if (!OmitNumbering)
                d.DataName = string.Format("{0} {1}-{2} ({3})", TableBaseName, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"), Layer + 1);
            else
                d.DataName = string.Format("{0} {1}-{2}", TableBaseName, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"));


            return d;
        }
        public Data3D FromMassRange(MassRangePair MassRange, string TableName, BackgroundWorker bw = null)
        {
            Data3D d = new Data3D(FromMassRange(MassRange, "temp", true, bw));
            d.DataName = TableName;
            return d;
        }

        public override void LoadFromFile(string[] FilePaths, BackgroundWorker bw)
        {
            DoLoad(FilePaths, bw);
        }
        public override void LoadFromFile(string FilePath, BackgroundWorker bw)
        {
            DoLoad(new string[] { FilePath }, bw);
        }

        protected abstract void DoLoad(string[] filePaths, BackgroundWorker bw);

        public override uint[] GetSpectrum(out float[] Masses)
        {
            uint[] intensities = new uint[_species.Count];
            float[] masses = new float[_species.Count];

            for (int s = 0; s < NumberSpecies; s++)
            {
                masses[s] = (float)_species[s].Mass;

                for (int z = 0; z < SizeZ; z++)
                {
                    for (int x = 0; x < SizeX; x++)
                    {
                        for (int y = 0; y < SizeY; y++)
                        {
                            intensities[s] += (uint)getMatrixValue(x, y, z, s);
                        }
                    }
                }
            }

            Masses = masses;
            return intensities;
        }

        public override void SaveText(string filePath)
        {
            SaveText(filePath, 1);
        }
        public override void SaveText(string filePath, int binSize)
        {
            float[] masses;
            uint[] intensities = GetSpectrum(out masses);

            using (StreamWriter sw = new StreamWriter(filePath))
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

        public override double[,] ToDoubleArray()
        {
            double[,] matrix = new double[SizeX, SizeY];

            for (int s = 0; s < NumberSpecies; s++)
            {
                for (int z = 0; z < SizeZ; z++)
                {
                    for (int x = 0; x < SizeX; x++)
                    {
                        for (int y = 0; y < SizeY; y++)
                        {
                            matrix[x, y] += (double)getMatrixValue(x, y, z, s);
                        }
                    }
                }
            }

            return matrix;
        }

        protected double[,] getPxMMatrix(List<CamecaSpecies> species)
        {
            int layerLinearLength = SizeX * SizeY;
            int numMassChannels = species.Count;

            double[,] matrix = new double[layerLinearLength * SizeZ, numMassChannels];

            for (int m = 0; m < species.Count; m++)
            {
                for (int z = 0; z < SizeZ; z++)
                {
                    int massPosition = m;
                    var intensities = FromSpecies(species[m]);

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

            return matrix;
        }
        public override double[,] GetPxMMatrix()
        {
            return getPxMMatrix(Species);
        }
        public override double[,] GetPxMMatrix(double[] binCenters, double[] binWidths)
        {
            if (binWidths != null || binCenters != null)
                Trace.WriteLine("WARNING: CAMECA spectra cannot specify mass binning for PxM matrix.");

            return getPxMMatrix(Species);
        }
        public override double[,] GetPxMMatrix(double[] binCenters, double[] binWidths, double startMass, double endMass)
        {
            if (binWidths != null || binCenters != null)
                Trace.WriteLine("WARNING: CAMECA spectra cannot specify mass binning for PxM matrix.");

            List<CamecaSpecies> species = new List<CamecaSpecies>();
            foreach (var s in Species)
            {
                if (s.Mass >= startMass && s.Mass < endMass)
                    species.Add(s);
            }

            return getPxMMatrix(species);
        }

        /// <summary>
        /// Removes mass spectra from unwanted pixels.
        /// </summary>
        /// <param name="PixelsToKeep">Pixels in the image to keep.</param>
        /// <returns>Cropped Bio-ToF spectrum.</returns>
        public Cameca1280Spectrum Crop(List<Point> PixelsToKeep, int Layer)
        {
            Cameca1280Spectrum cropped = new Cameca1280Spectrum(_name + " - Cropped");

            cropped._startMass = _startMass;
            cropped._endMass = _endMass;
            cropped._sizeX = _sizeX;
            cropped._sizeY = _sizeY;
            cropped._sizeZ = _sizeZ;
            cropped._specType = _specType;

            cropped._species = _species;

            cropped._matrix = new List<Data2D[]>();

            // Crop all layers
            if (Layer == -1)
            {
                for (int z = 0; z < _sizeZ; z++)
                {
                    Data2D[] layer = new Data2D[_species.Count];
                    for (int s = 0; s < _species.Count; s++)
                    {
                        layer[s] = new Data2D(_sizeX, _sizeY);
                    }

                    foreach (var pixel in PixelsToKeep)
                    {
                        int x = (int)pixel.X;
                        int y = (int)pixel.Y;

                        for (int s = 0; s < _species.Count; s++)
                        {
                            layer[s][x, y] = _matrix[z][s][x, y];
                        }
                    }

                    cropped._matrix.Add(layer);
                }
            }
            //Only crop single layer
            else
            {
                cropped._sizeZ = 1;

                Data2D[] layer = new Data2D[_species.Count];
                for (int s = 0; s < _species.Count; s++)
                {
                    layer[s] = new Data2D(_sizeX, _sizeY);
                }

                foreach (var pixel in PixelsToKeep)
                {
                    int x = (int)pixel.X;
                    int y = (int)pixel.Y;

                    for (int s = 0; s < _species.Count; s++)
                    {
                        layer[s][x, y] = _matrix[Layer][s][x, y];
                    }
                }

                cropped._matrix.Add(layer);
            }

            return cropped;
        }
        /// <summary>
        /// Removes mass spectra from unwanted pixels.
        /// </summary>
        /// <param name="PixelsToKeep">Pixels in the image to keep.</param>
        /// <param name="ResizeBuffer">Padding to add to the resized cropped image.</param>
        /// <returns>Cropped and resized Bio-ToF spectrum</returns>
        public Cameca1280Spectrum CropAndResize(List<Point> PixelsToKeep, int Layer, int ResizeBuffer)
        {
            Cameca1280Spectrum cropped = new Cameca1280Spectrum(_name + " - Cropped");

            cropped._startMass = _startMass;
            cropped._endMass = _endMass;
            cropped._sizeX = _sizeX;
            cropped._sizeY = _sizeY;
            cropped._sizeZ = _sizeZ;
            cropped._specType = _specType;

            cropped._species = _species;

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

            resizeStartX = Math.Max(0, (int)PixelsToKeep.Min(p => p.X) - ResizeBuffer);
            resizeEndX = Math.Min(_sizeX, (int)PixelsToKeep.Max(p => p.X) + ResizeBuffer);
            resizeStartY = Math.Max(0, (int)PixelsToKeep.Min(p => p.Y) - ResizeBuffer);
            resizeEndY = Math.Min(_sizeY, (int)PixelsToKeep.Max(p => p.Y) + ResizeBuffer);
            resizeWidth = resizeEndX - resizeStartX + 1;
            resizeHeight = resizeEndY - resizeStartY + 1;

            cropped._sizeX = resizeWidth;
            cropped._sizeY = resizeHeight;

            cropped._matrix = new List<Data2D[]>();

            if (Layer == -1)
            {
                for (int z = 0; z < _sizeZ; z++)
                {
                    Data2D[] layer = new Data2D[_species.Count];
                    for (int s = 0; s < _species.Count; s++)
                    {
                        layer[s] = new Data2D(_sizeX, _sizeY);
                    }

                    foreach (var pixel in PixelsToKeep)
                    {
                        int origX = (int)pixel.X;
                        int origY = (int)pixel.Y;
                        int cropX = origX - resizeStartX;
                        int cropY = origY - resizeStartY;

                        for (int s = 0; s < _species.Count; s++)
                        {
                            layer[s][cropX, cropY] = _matrix[z][s][origX, origY];
                        }
                    }

                    cropped._matrix.Add(layer);
                }
            }
            else
            {
                cropped._sizeZ = 1;

                Data2D[] layer = new Data2D[_species.Count];
                for (int s = 0; s < _species.Count; s++)
                {
                    layer[s] = new Data2D(_sizeX, _sizeY);
                }

                foreach (var pixel in PixelsToKeep)
                {
                    int origX = (int)pixel.X;
                    int origY = (int)pixel.Y;
                    int cropX = origX - resizeStartX;
                    int cropY = origY - resizeStartY;

                    for (int s = 0; s < _species.Count; s++)
                    {
                        layer[s][cropX, cropY] = _matrix[Layer][s][origX, origY];
                    }
                }

                cropped._matrix.Add(layer);
            }

            return cropped;
        }

        // Layout
        // (int)                    SpectrumType
        // (string)                 Name
        // (int)                    SizeX
        // (int)                    SizeY
        // (int)                    SizeZ
        // (int)                    Number species
        // (List<CamecaSpecies>)    Species
        // (List<Data2D[]>)         Matrix ([SizeZ][NumberLayers])
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

                bw.Write(SizeX);
                bw.Write(SizeY);
                bw.Write(SizeZ);

                bw.Write(NumberSpecies);
                for (int i = 0; i < NumberSpecies; i++)
                {
                    bw.Write(Species[i].ToByteArray());
                }

                for (int i = 0; i < SizeZ; i++)
                {
                    Data2D[] layer = _matrix[i];

                    for (int j = 0; j < NumberSpecies; j++)
                    {
                        bw.Write(layer[j].ToByteArray());
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

        // Layout
        // (int)                    SpectrumType
        // (string)                 Name
        // (int)                    SizeX
        // (int)                    SizeY
        // (int)                    SizeZ
        // (int)                    Number species
        // (List<CamecaSpecies>)    Species
        // (List<Data2D[]>)         Matrix ([SizeZ][NumberLayers])
        public override void FromByteArray(byte[] array)
        {
            using (MemoryStream ms = new MemoryStream(array))
            {
                BinaryReader br = new BinaryReader(ms);

                _specType = (SpectrumType)br.ReadInt32();
                Name = br.ReadString();

                _sizeX = br.ReadInt32();
                _sizeY = br.ReadInt32();
                _sizeZ = br.ReadInt32();

                int numSpecies = br.ReadInt32();

                for (int i = 0; i < numSpecies; i++)
                {
                    int size = br.ReadInt32();
                    CamecaSpecies species = new CamecaSpecies();
                    species.FromByteArray(br.ReadBytes(size));
                    Species.Add(species);
                }

                for (int i = 0; i < SizeZ; i++)
                {
                    Data2D[] layer = new Data2D[numSpecies];

                    for (int j = 0; j < numSpecies; j++)
                    {
                        int size = br.ReadInt32();
                        Data2D d = new Data2D();
                        d.FromByteArray(br.ReadBytes(size));
                        layer[j] = d;
                    }

                    _matrix.Add(layer);
                }
            }
        }

        /// <summary>
        /// Analyzes a layer to determine if all value are the same (equal to maximum) which indicates something went wrong during acquisition. 
        /// Check returns false if the maximum is zero.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns>True if all values are the same and greater than zero, false otherwise. </returns>
        protected bool checkForSaturatedLayer(Data2D layer)
        {
            var max = layer.Maximum;
            if (max == 0) return false;

            for (int x = 0; x < layer.Width; x++)
            {
                for (int y = 0; y < layer.Height; y++)
                {
                    if (layer[x, y] != max)
                        return false;
                }
            }
            return true;
        }
    }

    public class Cameca1280Spectrum : CamecaSpectrum
    {
        public Cameca1280Spectrum(string spectrumName)
            : base(spectrumName)
        {
            _specType = SpectrumType.Cameca1280;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
          
        protected override void DoLoad(string[] filePaths, BackgroundWorker bw)
        {
            int imageSize = 0;
            _species = new List<CamecaSpecies>();
            int[] numCyclesForFile = new int[filePaths.Length];

            _matrix = new List<Data2D[]>();

            int totalSteps = filePaths.Length;
            if (bw != null)
                bw.ReportProgress(0);

            // Verify all input files have consistent paramters
            for (int i = 0; i < filePaths.Length; i++)
            {
                byte[] fullBuffer;
                int startIndexOfXml = 0;
                int streamLength = 0;
                
                using (Stream stream = File.OpenRead(filePaths[i]))
                {
                    BinaryReader br = new BinaryReader(stream);
                    streamLength = (int)stream.Length;
                    fullBuffer = br.ReadBytes(streamLength);
                }

                startIndexOfXml = getXmlIndex(fullBuffer);
                int xmlBufferSize = streamLength - startIndexOfXml - 1;
                byte[] xmlBuffer = new byte[xmlBufferSize];
                Array.Copy(fullBuffer, startIndexOfXml, xmlBuffer, 0, xmlBufferSize);

                List<CamecaSpecies> speciesInFile = new List<CamecaSpecies>();

                XmlDocument docDetails = new XmlDocument();
                string xml = Encoding.UTF8.GetString(xmlBuffer);
                docDetails.LoadXml(xml);

                XmlNode nodeSize = docDetails.SelectSingleNode("/IMP/LOADEDFILE/PROPERTIES/DEFACQPARAMIMDATA/m_nSize");
                int size = 0;

                if(!int.TryParse(nodeSize.InnerText, out size))
                    throw new ArgumentException($"Unable to parse the image size from file {Path.GetFileName(filePaths[i])}.");

                if (size <= 0 || size == int.MaxValue)
                    throw new ArgumentException($"Unable to parse the image size from file {Path.GetFileName(filePaths[i])}.");

                if (imageSize == 0)
                    imageSize = size;
                else
                {
                    if (imageSize != size)
                        throw new ArgumentException("Invalid image size. Dimension does not match across all files.");
                }

                XmlNodeList nodeSpeciesList = docDetails.SelectNodes("IMP/LOADEDFILE/SPECIES");
                foreach(XmlNode node in nodeSpeciesList)
                {
                    try
                    {
                        speciesInFile.Add(CamecaSpecies.FromXmlNode(node));
                    }
                    catch(Exception ex)
                    {
                        throw new ArgumentException($"Could not parse a species from file {Path.GetFileName(filePaths[i])}.", ex);
                    }
                }

                int numCycles = 0;
                foreach(CamecaSpecies s in speciesInFile)
                {
                    if (numCycles == 0) numCycles = s.Cycles;

                    if (s.Cycles != numCycles)
                        throw new ArgumentException("Invalid species. The number of cycles do not match in the image.");
                }

                numCyclesForFile[i] = numCycles;

                if(_species.Count == 0)
                {
                    _species.AddRange(speciesInFile);
                }
                else
                {
                    if (_species.Count != speciesInFile.Count)
                        throw new ArgumentException("Invalid spectrum. Number of species do not match across files.");

                    CamecaSpeciesIdentityComparer identityComparer = new CamecaSpeciesIdentityComparer();

                    foreach(CamecaSpecies s in speciesInFile)
                    {
                        if (!_species.Contains(s, identityComparer))
                            throw new ArgumentException("Invalid species list. One or more species is not present in all of the files.");
                    }
                }

                if (bw != null)
                    bw.ReportProgress((i + 1) * 100 / totalSteps);
            }

            // Reset progress and start counting layers read in
            if (bw != null) bw.ReportProgress(0);

            totalSteps = numCyclesForFile.Sum();
            int cycleCounter = 0;

            // Read in binary data for files and populate matrices
            for (int i = 0; i < filePaths.Length; i++)
            {
                byte[] fullBuffer;
                int dataBufferSize = 0;

                using (Stream stream = File.OpenRead(filePaths[i]))
                {
                    BinaryReader br = new BinaryReader(stream);

                    fullBuffer = br.ReadBytes((int)stream.Length);

                    // Get size of binary section at top of file
                    int startIndexOfXml = getXmlIndex(fullBuffer);

                    // First 24 bytes are garbage, so skip those
                    dataBufferSize = startIndexOfXml - 24;
                }

                byte[] dataBuffer = new byte[dataBufferSize];
                Array.Copy(fullBuffer, 24, dataBuffer, 0, dataBufferSize);

                int pixelType = _species[0].PixelEncoding;
                int integerSize = 0;

                // Only know that 0 corresponds to Int16
                if (pixelType == 0) integerSize = 2;

                float[] values = new float[imageSize * imageSize * _species.Count * numCyclesForFile[i]];
                for (int j = 0; j < values.Length; j++)
                {
                    //Int16
                    if (pixelType == 0)
                    {
                        values[j] = BitConverter.ToInt16(dataBuffer, j * integerSize);
                    }
                }

                int pos = 0;
                int numRecords = _species.Count * numCyclesForFile[i];
                float[,,] tempValues = new float[numRecords, imageSize, imageSize];
                for (int r = 0; r < numRecords; r++)
                {
                    for (int y = 0; y < imageSize; y++)
                    {
                        for (int x = 0; x < imageSize; x++)
                        {
                            tempValues[r, y, x] = values[pos++];
                        }
                    }
                }

                // _matrix[layer][species][x,y]
                for (int z = 0; z < numCyclesForFile[i]; z++)
                {
                    Data2D[] layer = new Data2D[_species.Count];
                    for (int r = 0; r < _species.Count; r++)
                    {
                        int recordNum = _species[r].RecordIds[z];
                        Data2D layerSpecies = new Data2D(imageSize, imageSize);
                        for (int y = 0; y < imageSize; y++)
                        {
                            for (int x = 0; x < imageSize; x++)
                            {
                                layerSpecies[x, y] = tempValues[recordNum, y, x];
                            }
                        }
                        // If layer is not saturated with same value, add to species 
                        // otherwise add blank layer
                        if (!checkForSaturatedLayer(layerSpecies))
                        {
                            layer[r] = layerSpecies;
                        }
                        else layer[r] = new Data2D(imageSize, imageSize);
                    }
                    _matrix.Add(layer);

                    cycleCounter++;
                    if (bw != null)
                        bw.ReportProgress(cycleCounter * 100 / totalSteps);
                }
            }

            _sizeX = imageSize;
            _sizeY = imageSize;
            _sizeZ = _matrix.Count;

            _intensities = GetSpectrum(out _masses);

            _startMass = (float)_species.Min(s => s.Mass);
            _endMass = (float)_species.Max(s => s.Mass);
        }

        private static int getXmlIndex(byte[] buffer)
        {
            int i = 2;
            int length = buffer.Length;
            while (i < length)
            {
                switch (buffer[i])
                {
                    case 120:
                        if (buffer[i - 1] == 63 && buffer[i - 2] == 60)
                        {
                            return i - 2;
                        }
                        i += 3;
                        continue;
                    case 63:
                        i += 1;
                        continue;
                    case 60:
                        i += 2;
                        continue;
                    default:
                        i += 3;
                        continue;
                }
            }
            return -1;
        }
    }

    public class CamecaNanoSIMSSpectrum : CamecaSpectrum
    {
        protected readonly float DeadTime = (float)44e-9;
        protected readonly float Csc = (float)((1 / 1.6f) * Math.Pow(10, 7));

        bool _isDeadTimeCorrected;
        CamecaNanoSIMSParameters _headerInfo;

        public bool IsDeadTimeCorrected
        {
            get { return _isDeadTimeCorrected; }
            set
            {
                if(_isDeadTimeCorrected != value)
                {
                    _isDeadTimeCorrected = value;
                    OnPropertyChanged("IsDeadTimeCorrected");
                }
            }
        }

        public CamecaNanoSIMSSpectrum(string Name) : base(Name)
        {
            _specType = SpectrumType.CamecaNanoSIMS;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override void DoLoad(string[] filePaths, BackgroundWorker bw)
        {
            List<CamecaNanoSIMSParameters> parameters = new List<CamecaNanoSIMSParameters>();

            int totalSteps = filePaths.Length;
            int[] numLayersPerFile = new int[filePaths.Length];

            for (int i = 0; i < filePaths.Length; i++)
            {
                parameters.Add(ParseSpectrumParameters(filePaths[i]));

                numLayersPerFile[i] = parameters[i].SizeZ;

                if (bw != null)
                    bw.ReportProgress((i + 1) * 100 / totalSteps);

            }

            if (parameters.Count == 0) throw new ArgumentNullException("No header information loaded");

            // Only validate parameters if more than one file is being loaded
            if(parameters.Count > 1)
            {
                foreach (var p in parameters)
                {
                    bool isEqual = CompareHeaders(p, parameters[0]);

                    if (!isEqual) throw new ArgumentException($"Files {p.AnalysisName} and {parameters[0].AnalysisName} do not match paramaters");
                }
            }

            var headerInfo = parameters[0];

            // Populate species with mass info
            _species = new List<CamecaSpecies>();
            foreach (var mass in headerInfo.Masses)
            {
                CamecaSpecies species = new CamecaSpecies()
                {
                    CountTime = mass.CountingTime,
                    Label = mass.Polyatomic.MassLabel,
                    Mass = mass.Amu,
                    WaitTime = mass.WaitingTime,
                };
                _species.Add(species);
            }

            // Reset progress and count each layer read in
            totalSteps = numLayersPerFile.Sum();
            int posCounter = 0;

            if (bw != null)
                bw.ReportProgress(0);

            // Load data
            _matrix = new List<Data2D[]>();

            for (int i = 0; i < filePaths.Length; i++)
            {
                var fileHeader = parameters[i];
                using (Stream stream = File.OpenRead(filePaths[i]))
                {
                    var reader = new BinaryReader(stream);
                    reader.BaseStream.Seek(fileHeader.Header.HeaderSize, SeekOrigin.Begin);

                    int numberMasses = fileHeader.NumberMasses;

                    for (int z = 0; z < fileHeader.SizeZ; z++)
                    {
                        Data2D[] layer = new Data2D[numberMasses];
                        for (int r = 0; r < numberMasses; r++)
                        {
                            Data2D layerSpecies = new Data2D(fileHeader.SizeX, fileHeader.SizeY);
                            for (int y = 0; y < fileHeader.SizeY; y++)
                            {
                                for (int x = 0; x < fileHeader.SizeX; x++)
                                {
                                    if (fileHeader.HeaderImage.PixelDepth == CamecaNanoSIMSPixelDepth.b16)
                                        layerSpecies[x, y] = reader.ReadInt16();
                                    else if (fileHeader.HeaderImage.PixelDepth == CamecaNanoSIMSPixelDepth.b32)
                                        layerSpecies[x, y] = reader.ReadInt32();
                                }
                            }
                            layer[r] = layerSpecies;
                        }
                        _matrix.Add(layer);

                        posCounter++;

                        if (bw != null)
                            bw.ReportProgress(posCounter * 100 / totalSteps);
                    }
                }
            }

            _sizeX = headerInfo.SizeX;
            _sizeY = headerInfo.SizeY;
            _sizeZ = _matrix.Count;

            _intensities = GetSpectrum(out _masses);

            _startMass = (float)_species.Min(s => s.Mass);
            _endMass = (float)_species.Max(s => s.Mass);

            _headerInfo = headerInfo;
        }

        public void DeadTimeCorrect()
        {
            if (_isDeadTimeCorrected) return;

            var masses = _headerInfo.Masses.ToList();

            for (int z = 0; z < _sizeZ; z++)
            {
                for (int i = 0; i < NumberSpecies; i++)
                {
                    float dwellTime = (float)masses[i].CountingTime / (_sizeX * _sizeY);
                    dwellTime /= 1000;

                    for (int x = 0; x < _sizeX; x++)
                    {
                        for (int y = 0; y < _sizeY; y++)
                        {
                            _matrix[z][i][x, y] /= ((1 - _matrix[z][i][x, y] * DeadTime) / dwellTime);
                        }
                    }
                }
            }

            IsDeadTimeCorrected = true;
        }
        public void RemoveDeadTimeCorrection()
        {
            if (!_isDeadTimeCorrected) return;

            var masses = _headerInfo.Masses.ToList();

            for (int z = 0; z < _sizeZ; z++)
            {
                for (int i = 0; i < NumberSpecies; i++)
                {
                    float dwellTime = (float)masses[i].CountingTime / (_sizeX * _sizeY);
                    dwellTime /= 1000;

                    for (int x = 0; x < _sizeX; x++)
                    {
                        for (int y = 0; y < SizeY; y++)
                        {
                            _matrix[z][i][x, y] /= ((_matrix[z][i][x, y] * DeadTime) + dwellTime);
                        }
                    }
                }
            }
        }

        private CamecaNanoSIMSParameters ParseSpectrumParameters(string filePath)
        {
            using (Stream stream = File.OpenRead(filePath))
            {
                BinaryReader reader = new BinaryReader(stream);

                List<CamecaNanoSIMSTabMass> masses = new List<CamecaNanoSIMSTabMass>();

                var header = new CamecaNanoSIMSHeader();
                header.ParseFromStream(reader);

                var mask = new CamecaNanoSIMSMaskImage();
                mask.ParseFromStream(reader);

                int tabMass = 0;
                int numberTabMass = 10;
                if (header.Release >= 4108) numberTabMass = 60;
                for (int i = 0; i < numberTabMass; i++)
                {
                    tabMass = reader.ReadInt32();
                }

                int numberMasses = mask.NumberMasses;

                var massNames = new string[numberMasses];
                var massSymbols = new string[numberMasses];
                for (int i = 0; i < numberMasses; i++)
                {
                    var mass = new CamecaNanoSIMSTabMass();
                    mass.ParseFromStream(reader);

                    massNames[i] = mass.Amu.ToString("0.00");
                    massSymbols[i] = string.IsNullOrEmpty(mass.Polyatomic.MassLabel) ? "-" : mass.Polyatomic.MassLabel;

                    masses.Add(mass);
                }

                if (header.Release >= 4108)
                {
                    header.ParseV7MetaDataFromStream(reader, numberMasses, true);
                }

                var headerImage = new CamecaNanoSIMSHeaderImage();
                headerImage.ParseFromStream(reader);

                int numberImages = headerImage.Depth;

                long theoreticalSize = headerImage.Width * headerImage.Height
                    * headerImage.Depth * headerImage.NumberMasses * (long)headerImage.PixelDepth + header.HeaderSize;
                if (theoreticalSize != reader.BaseStream.Length)
                {
                    throw new ArgumentException("Expected file size does not match the actual file length");
                }

                if (headerImage.PixelDepth != CamecaNanoSIMSPixelDepth.b16 && headerImage.PixelDepth != CamecaNanoSIMSPixelDepth.b32)
                    throw new ArgumentException("Invalid pixel depth");

                return new CamecaNanoSIMSParameters()
                {
                    Header = header,
                    HeaderImage = headerImage,
                    MaskImage = mask,
                    Masses = masses,
                };
            }  
        }
        private bool CompareHeaders(CamecaNanoSIMSParameters x, CamecaNanoSIMSParameters y, bool equateSizeZ = false)
        {
            bool isEqual = false;

            isEqual = x.SizeX == y.SizeX && x.SizeY == y.SizeY 
                && x.Masses == y.Masses && x.AnalysisType == y.AnalysisType;

            if (!equateSizeZ) return isEqual;

            return isEqual && x.SizeZ == y.SizeZ;
        }
    }

    
    public struct CamecaSpecies : ISavable
    {
        public int Cycles;
        public int SizeInBytes;
        public int PixelEncoding;
        public double Mass;
        public string Label;
        public double WaitTime;
        public double CountTime;
        public double WellTime;
        public double ExtraTime;
        public int[] RecordIds;

        public static CamecaSpecies FromXmlNode(XmlNode node)
        {
            CamecaSpecies species = new CamecaSpecies();

            XmlNode nodeNumberCycles = node.SelectSingleNode("n_AcquiredCycleNb");
            species.Cycles = int.Parse(nodeNumberCycles.InnerText);

            XmlNode nodeSizeBytes = node.SelectSingleNode("SIZE");
            species.SizeInBytes = int.Parse(nodeSizeBytes.InnerText);

            XmlNode nodePixelEncoding = node.SelectSingleNode("PROPERTIES/COMMON_TO_ALL_SPECIESPCTRS/n_EncodedPixelType");
            species.PixelEncoding = int.Parse(nodePixelEncoding.InnerText);

            XmlNode nodeMass = node.SelectSingleNode("PROPERTIES/DPSPECIESDATA/d_Mass");
            species.Mass = double.Parse(nodeMass.InnerText);

            XmlNode nodeLabel = node.SelectSingleNode("PROPERTIES/DPSPECIESDATA/psz_MatrixSpecies");
            species.Label = nodeLabel.InnerText;

            XmlNode nodeWaitTime = node.SelectSingleNode("PROPERTIES/DPSPECIESDATA/d_WaitTime");
            species.WaitTime = double.Parse(nodeWaitTime.InnerText);

            XmlNode nodeCountTime = node.SelectSingleNode("PROPERTIES/DPSPECIESDATA/d_CountTime");
            species.CountTime = double.Parse(nodeCountTime.InnerText);

            XmlNode nodeWellTime = node.SelectSingleNode("PROPERTIES/DPSPECIESDATA/d_WellTime");
            species.WellTime = double.Parse(nodeWellTime.InnerText);

            XmlNode nodeExtraTime = node.SelectSingleNode("PROPERTIES/DPSPECIESDATA/d_ExtraTime");
            species.ExtraTime = double.Parse(nodeExtraTime.InnerText);

            XmlNode nodeRecord = node.SelectSingleNode("RECORD");
            species.RecordIds = recordFromString(nodeRecord.InnerText);

            return species;
        }

        private static int[] recordFromString(string record)
        {
            List<int> values = new List<int>();

            string[] segments = record.Split(';');

            foreach (var segment in segments)
            {
                if (segment.StartsWith("[") && segment.EndsWith("]"))
                {
                    var rangeString = segment.Replace("[", string.Empty);
                    rangeString = rangeString.Replace("]", string.Empty);

                    int hyphenIndex = rangeString.IndexOf('-');
                    int startIndex = int.Parse(rangeString.Substring(0, hyphenIndex));
                    int endIndex = int.Parse(rangeString.Substring(hyphenIndex + 1));

                    for (int i = startIndex; i <= endIndex ; i++)
                    {
                        values.Add(i);
                    }
                }
                else
                {
                    values.Add(int.Parse(segment));
                }
            }

            return values.ToArray();
        }

        // Layout:
        // (int)    Cycles
        // (int)    SizeInBytes
        // (int)    PixelEncoding
        // (double) Mass
        // (string) Label
        // (double) WaitTime
        // (double) CountTime
        // (double) WellTime
        // (double) ExtraTime
        // (int)    RecordIds.Length
        // (int[])  RecordIds

        public byte[] ToByteArray()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(ms);

                // Offset start of writing to later go back and 
                // prefix the stream with the size of the stream written
                bw.Seek(sizeof(int), SeekOrigin.Begin);

                // Write contents of object
                bw.Write(Cycles);
                bw.Write(SizeInBytes);
                bw.Write(PixelEncoding);
                bw.Write(Mass);
                bw.Write(Label);
                bw.Write(WaitTime);
                bw.Write(CountTime);
                bw.Write(WellTime);
                bw.Write(ExtraTime);
                bw.Write(RecordIds.Length);
                for (int i = 0; i < RecordIds.Length; i++)
                {
                    bw.Write(RecordIds[i]);
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
        // (int)    Cycles
        // (int)    SizeInBytes
        // (int)    PixelEncoding
        // (double) Mass
        // (string) Label
        // (double) WaitTime
        // (double) CountTime
        // (double) WellTime
        // (double) ExtraTime
        public void FromByteArray(byte[] array)
        {
            using (MemoryStream ms = new MemoryStream(array))
            {
                BinaryReader br = new BinaryReader(ms);

                Cycles = br.ReadInt32();
                SizeInBytes = br.ReadInt32();
                PixelEncoding = br.ReadInt32();
                Mass = br.ReadDouble();
                Label = br.ReadString();
                WaitTime = br.ReadDouble();
                CountTime = br.ReadDouble();
                WellTime = br.ReadDouble();
                ExtraTime = br.ReadDouble();

                int recordLength = br.ReadInt32();
                RecordIds = new int[recordLength];
                for (int i = 0; i < recordLength; i++)
                {
                    RecordIds[i] = br.ReadInt32();
                }
            }
        }
    }
    internal class CamecaSpeciesIdentityComparer : IEqualityComparer<CamecaSpecies>
    {
        public bool Equals(CamecaSpecies x, CamecaSpecies y)
        {
            return x.Label == y.Label && x.Mass == y.Mass;
        }

        public int GetHashCode(CamecaSpecies obj)
        {
            return obj.GetHashCode();
        }
    }

    namespace CamecaHeaders
    {
        internal class CamecaNanoSIMSTabMassIdentityComparer : IEqualityComparer<CamecaNanoSIMSTabMass>
        {
            public bool Equals(CamecaNanoSIMSTabMass x, CamecaNanoSIMSTabMass y)
            {
                return x.Amu == y.Amu && x.Polyatomic.MassLabel == y.Polyatomic.MassLabel && x.Polyatomic.Charge == y.Polyatomic.Charge;
            }

            public int GetHashCode(CamecaNanoSIMSTabMass obj)
            {
                return obj.GetHashCode();
            }
        }

        internal interface ICamecaNanoSIMSHeaderPart
        {
            void ParseFromStream(BinaryReader reader);
        }

        internal abstract class CamecaNanoSIMSHeaderPart : ICamecaNanoSIMSHeaderPart
        {
            /// <summary>
            /// Reads the header properties from the given stream.
            /// </summary>
            /// <param name="reader">Reader for the file stream.</param>
            public abstract void ParseFromStream(BinaryReader reader);

            protected static DateTime ParseDateAndTimeStrings(string date, string time)
            {
                string formatted = $"{date} - {time}";
                return DateTime.ParseExact(formatted, "dd.MM.yy - H:m", CultureInfo.InvariantCulture);
            }
            protected static string RemovePadCharacters(string padded)
            {
                return padded.Replace("\0", "");
            }
            protected static string ReadString(BinaryReader reader, int numberChars)
            {
                try
                {
                    var chars = reader.ReadChars(numberChars);
                    return RemovePadCharacters(new string(chars));
                }
                catch (ArgumentException)
                {
                    return "--invalid buffer--";
                }

            }
        }

        internal class CamecaNanoSIMSParameters
        {
            public CamecaNanoSIMSHeader Header { get; set; }
            public CamecaNanoSIMSHeaderImage HeaderImage { get; set; }
            public CamecaNanoSIMSMaskImage MaskImage { get; set; }
            public ICollection<CamecaNanoSIMSTabMass> Masses { get; set; }

            public int Release { get { return Header.Release; } }
            public CamecaNanoSIMSAnalysisType AnalysisType { get { return Header.AnalysisType; } }
            public string AnalysisName { get { return Header.AnalysisName; } }
            public DateTime AnalysisTime { get { return Header.AnalysisTime; } }
            public int SizeX { get { return HeaderImage.Width; } }
            public int SizeY { get { return HeaderImage.Height; } }
            public int SizeZ { get { return HeaderImage.Depth; } }
            public int NumberMasses { get { return HeaderImage.NumberMasses; } }

            public bool IsEmpty
            {
                get { return Release == -1 && Masses.Count == 0; }
            }
            
            public CamecaNanoSIMSParameters()
            {
                Header = new CamecaNanoSIMSHeader();
                HeaderImage = new CamecaNanoSIMSHeaderImage();
                MaskImage = new CamecaNanoSIMSMaskImage();

                Header.Release = -1;

                Masses = new List<CamecaNanoSIMSTabMass>();
            }
        }

        internal class CamecaNanoSIMSHeader : CamecaNanoSIMSHeaderPart
        {
            public int Release { get; set; }
            public CamecaNanoSIMSAnalysisType AnalysisType { get; set; }
            public int HeaderSize { get; set; }
            public int SampleType { get; set; }
            public int DataIncluded { get; set; }
            public int PositionX { get; set; }
            public int PositionY { get; set; }
            public string AnalysisName { get; set; }
            public string UserName { get; set; }
            public int PositionZ { get; set; }
            public DateTime AnalysisTime { get; set; }

            // v7 Meta Data:
            public int NumberPolyatomics { get; set; }
            public int NumberMagneticFields { get; set; }
            public int MagneticField { get; set; }
            public double[] TrolleyRadii { get; set; }
            public string TrolleyRadius { get; set; }
            public string Comments { get; set; }
            public int PrimaryCurrentT0 { get; set; }
            public int PrimaryCurrentEnd { get; set; }
            public int PrimaryL1 { get; set; }
            public int PositionD1 { get; set; }
            public int PrimaryL0 { get; set; }
            public int CsHV { get; set; }
            public int EntranceSlitPosition { get; set; }
            public int ApertureSlitPosition { get; set; }

            /// <summary>
            /// Reads the header properties from the given stream.
            /// </summary>
            /// <param name="reader">Reader for the file stream.</param>
            public override void ParseFromStream(BinaryReader reader)
            {
                // Should be at start of stream, so set to be safe
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                Release = reader.ReadInt32();
                AnalysisType = (CamecaNanoSIMSAnalysisType)reader.ReadInt32();
                HeaderSize = reader.ReadInt32();
                SampleType = reader.ReadInt32();
                DataIncluded = reader.ReadInt32();
                PositionX = reader.ReadInt32();
                PositionY = reader.ReadInt32();

                AnalysisName = ReadString(reader, 32);
                UserName = ReadString(reader, 16);

                PositionZ = reader.ReadInt32();

                var unused = reader.ReadInt32();
                unused = reader.ReadInt32();
                unused = reader.ReadInt32();

                var date = ReadString(reader, 16);
                var time = ReadString(reader, 16);
                AnalysisTime = ParseDateAndTimeStrings(date, time);
            }

            /// <summary>
            /// Reads v7 metadata properties from the specified stream.
            /// </summary>
            /// <param name="reader">Reader for the file stream.</param>
            /// <param name="numberMassChannels">Number of mass channels in file.</param>
            /// <param name="seekToEnd">If true, sets the reader position to the end of the v7 metadata.</param>
            public void ParseV7MetaDataFromStream(BinaryReader reader, int numberMassChannels, bool seekToEnd)
            {
                long offsetPolyList = 652 + 288 * numberMassChannels;
                long offsetNbPoly = offsetPolyList + 16;
                reader.BaseStream.Seek(offsetNbPoly, SeekOrigin.Begin);
                NumberPolyatomics = reader.ReadInt32();

                long offsetMaskNano = 676 + 288 * numberMassChannels + 144 * NumberPolyatomics;
                long offsetNbField = offsetMaskNano + 4 * 24;
                reader.BaseStream.Seek(offsetNbField, SeekOrigin.Begin);
                NumberMagneticFields = reader.ReadInt32();

                long offsetBFieldNano = 2228 + 288 * numberMassChannels + 144 * NumberPolyatomics;
                long offsetnNbField = offsetBFieldNano + 4;
                reader.BaseStream.Seek(offsetnNbField, SeekOrigin.Begin);
                MagneticField = reader.ReadInt32();

                long offsetTabTrolley = offsetBFieldNano + 10 * 4 + 2 * 8;
                TrolleyRadii = new double[12];
                StringBuilder sbRadius = new StringBuilder();
                for (int i = 0; i < TrolleyRadii.Length; i++)
                {
                    long offsetRadius = offsetTabTrolley + i * 208 + 64 + 8;
                    reader.BaseStream.Seek(offsetRadius, SeekOrigin.Begin);
                    TrolleyRadii[i] = reader.ReadDouble();

                    sbRadius.Append($"{TrolleyRadii[i].ToString("0.00")} - ");
                }

                sbRadius = sbRadius.Remove(sbRadius.Length - 2, 2);
                TrolleyRadius = sbRadius.ToString();

                long offsetAnalParam = 2228 + 288 * numberMassChannels + 144 * NumberPolyatomics + 2840 * NumberMagneticFields;
                long offsetComment = offsetAnalParam + 16 + 4 + 4 + 4 + 4;
                reader.BaseStream.Seek(offsetComment, SeekOrigin.Begin);
                Comments = ReadString(reader, 256);

                long offsetAnalPrimary = offsetAnalParam + 16 + 4 + 4 + 4 + 4 + 256;
                long offsetPrimCurrentT0 = offsetAnalPrimary + 8;
                reader.BaseStream.Seek(offsetPrimCurrentT0, SeekOrigin.Begin);
                PrimaryCurrentT0 = reader.ReadInt32();
                PrimaryCurrentEnd = reader.ReadInt32();

                long offsetPrimL1 = offsetAnalPrimary + 8 + 4 + 4 + 4;
                reader.BaseStream.Seek(offsetPrimL1, SeekOrigin.Begin);
                PrimaryL1 = reader.ReadInt32();

                long offsetD1Pos = offsetAnalPrimary + 8 + 4 + 4 + 4 + 4 + 4 + 4 * 10 + 4 + 4 * 10;
                reader.BaseStream.Seek(offsetD1Pos, SeekOrigin.Begin);
                PositionD1 = reader.ReadInt32();

                long sizeApPrimary = 552;
                long offsetApSecondary = offsetAnalPrimary + sizeApPrimary;
                long offsetPrimL0 = offsetApSecondary - (67 * 4 + 4 * 10 + 4 + 4 + 4 + 4);
                reader.BaseStream.Seek(offsetPrimL0, SeekOrigin.Begin);
                PrimaryL0 = reader.ReadInt32();
                CsHV = reader.ReadInt32();

                long offsetEsPos = offsetApSecondary + 8;
                reader.BaseStream.Seek(offsetEsPos, SeekOrigin.Begin);
                EntranceSlitPosition = reader.ReadInt32();

                long offsetAsPos = offsetEsPos + 4 + 40 + 40;
                reader.BaseStream.Seek(offsetAsPos, SeekOrigin.Begin);
                ApertureSlitPosition = reader.ReadInt32();

                if (seekToEnd) reader.BaseStream.Seek(HeaderSize - CamecaNanoSIMSHeaderImage.STRUCT_SIZE, SeekOrigin.Begin);
            }
        }
        internal class CamecaNanoSIMSMaskImage : CamecaNanoSIMSHeaderPart
        {
            public string FileName { get; set; }
            public int AnalysisDuration { get; set; }
            public int CycleNumber { get; set; }
            public int ScanType { get; set; }
            public short Magnification { get; set; }
            public short SizeType { get; set; }
            public short SizeDetector { get; set; }
            public int BeamBlanking { get; set; }
            public int Sputtering { get; set; }
            public int SputteringDuration { get; set; }
            public int AutoCalibrationInAnalysis { get; set; }
            public CamecaNanoSIMSAutoCal AutoCal { get; set; }
            public int SigReference { get; set; }
            public CamecaNanoSIMSSigRef SigRef { get; set; }
            public int NumberMasses { get; set; }

            /// <summary>
            /// Reads the header properties from the given stream.
            /// </summary>
            /// <param name="reader">Reader for the file stream.</param>
            public override void ParseFromStream(BinaryReader reader)
            {
                FileName = ReadString(reader, 16);
                AnalysisDuration = reader.ReadInt32();
                CycleNumber = reader.ReadInt32();
                ScanType = reader.ReadInt32();
                Magnification = reader.ReadInt16();
                SizeType = reader.ReadInt16();
                SizeDetector = reader.ReadInt16();

                short unused = reader.ReadInt16();

                BeamBlanking = reader.ReadInt32();
                Sputtering = reader.ReadInt32();
                SputteringDuration = reader.ReadInt32();
                AutoCalibrationInAnalysis = reader.ReadInt32();

                AutoCal = new CamecaNanoSIMSAutoCal();
                AutoCal.ParseFromStream(reader);

                SigReference = reader.ReadInt32();

                SigRef = new CamecaNanoSIMSSigRef();
                SigRef.ParseFromStream(reader);

                NumberMasses = reader.ReadInt32();
            }
        }
        internal class CamecaNanoSIMSAutoCal : CamecaNanoSIMSHeaderPart
        {
            public string Mass { get; set; }
            public int Begin { get; set; }
            public int Period { get; set; }

            /// <summary>
            /// Reads the header properties from the given stream.
            /// </summary>
            /// <param name="reader">Reader for the file stream.</param>
            public override void ParseFromStream(BinaryReader reader)
            {
                Mass = ReadString(reader, 64);
                Begin = reader.ReadInt32();
                Period = reader.ReadInt32();
            }
        }
        internal class CamecaNanoSIMSSigRef : CamecaNanoSIMSHeaderPart
        {
            public CamecaNanoSIMSPolyatomic Polyatomic { get; set; }
            public int Detector { get; set; }
            public int Offset { get; set; }
            public int Quantity { get; set; }

            /// <summary>
            /// Reads the header properties from the given stream.
            /// </summary>
            /// <param name="reader">Reader for the file stream.</param>
            public override void ParseFromStream(BinaryReader reader)
            {
                Polyatomic = new CamecaNanoSIMSPolyatomic();
                Polyatomic.ParseFromStream(reader);

                Detector = reader.ReadInt32();
                Offset = reader.ReadInt32();
                Quantity = reader.ReadInt32();
            }
        }
        internal class CamecaNanoSIMSPolyatomic : CamecaNanoSIMSHeaderPart
        {
            public const int NUMBER_TABLETS = 5;

            public int FlagNumeric { get; set; }
            public int NumericValue { get; set; }
            public int NumberElements { get; set; }
            public int NumberCharges { get; set; }
            public string Charge { get; set; }
            public string MassLabel { get; set; }
            public CamecaNanoSIMSTablet[] Tablets { get; set; }

            /// <summary>
            /// Reads the header properties from the given stream.
            /// </summary>
            /// <param name="reader">Reader for the file stream.</param>
            public override void ParseFromStream(BinaryReader reader)
            {
                FlagNumeric = reader.ReadInt32();
                NumericValue = reader.ReadInt32();
                NumberElements = reader.ReadInt32();
                NumberCharges = reader.ReadInt32();
                Charge = ReadString(reader, 1);
                MassLabel = ReadString(reader, 64);

                Tablets = new CamecaNanoSIMSTablet[NUMBER_TABLETS];
                for (int i = 0; i < NUMBER_TABLETS; i++)
                {
                    var tablet = new CamecaNanoSIMSTablet();
                    tablet.ParseFromStream(reader);
                    Tablets[i] = tablet;
                }

                var unused = ReadString(reader, 3);
            }
        }
        internal class CamecaNanoSIMSTablet : CamecaNanoSIMSHeaderPart
        {
            public int NumberElements { get; set; }
            public int NumberIsotopes { get; set; }
            public int Quantity { get; set; }

            /// <summary>
            /// Reads the header properties from the given stream.
            /// </summary>
            /// <param name="reader">Reader for the file stream.</param>
            public override void ParseFromStream(BinaryReader reader)
            {
                NumberElements = reader.ReadInt32();
                NumberIsotopes = reader.ReadInt32();
                Quantity = reader.ReadInt32();
            }
        }
        internal class CamecaNanoSIMSTabMass : CamecaNanoSIMSHeaderPart
        {
            public double Amu { get; set; }
            public int MatrixOrTrace { get; set; }
            public int Detector { get; set; }
            public double WaitingTime { get; set; }
            public double CountingTime { get; set; }
            public int Offset { get; set; }
            public int MagField { get; set; }
            public CamecaNanoSIMSPolyatomic Polyatomic { get; set; }

            /// <summary>
            /// Reads the header properties from the given stream.
            /// </summary>
            /// <param name="reader">Reader for the file stream.</param>
            public override void ParseFromStream(BinaryReader reader)
            {
                int unused = reader.ReadInt32();
                unused = reader.ReadInt32();
                Amu = reader.ReadDouble();
                MatrixOrTrace = reader.ReadInt32();
                Detector = reader.ReadInt32();
                WaitingTime = reader.ReadDouble();
                CountingTime = reader.ReadDouble();
                Offset = reader.ReadInt32();
                MagField = reader.ReadInt32();

                Polyatomic = new CamecaNanoSIMSPolyatomic();
                Polyatomic.ParseFromStream(reader);
            }
        }
        internal class CamecaNanoSIMSHeaderImage : CamecaNanoSIMSHeaderPart
        {
            public const int STRUCT_SIZE = 84;

            public int SizeSelf { get; set; }
            public short Type { get; set; }
            public short Width { get; set; }
            public short Height { get; set; }
            public CamecaNanoSIMSPixelDepth PixelDepth { get; set; }
            public short NumberMasses { get; set; }
            public short Depth { get; set; }
            public int Raster { get; set; }
            public string Nickname { get; set; }

            /// <summary>
            /// Reads the header properties from the given stream.
            /// </summary>
            /// <param name="reader">Reader for the file stream.</param>
            public override void ParseFromStream(BinaryReader reader)
            {
                SizeSelf = reader.ReadInt32();
                Type = reader.ReadInt16();
                Width = reader.ReadInt16();
                Height = reader.ReadInt16();
                PixelDepth = (CamecaNanoSIMSPixelDepth)reader.ReadInt16();

                NumberMasses = reader.ReadInt16();
                if (NumberMasses < 1) NumberMasses = 1;

                Depth = reader.ReadInt16();
                Raster = reader.ReadInt32();
                Nickname = ReadString(reader, 64);
            }
        }

        internal enum CamecaNanoSIMSPixelDepth
        {
            b16 = 2,
            b32 = 4,
        }
        internal enum CamecaNanoSIMSAnalysisType
        {
            MIMSImage = 27,
            MIMSLineScan = 39,
            MIMSSampleStageImage = 41,
        }
    }
}