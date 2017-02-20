using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ImagingSIMS.Data.Spectra
{
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
        protected float getMatrixValue(int x, int y, int z, MassRange range)
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
            Data2D d = FromMassRange(new MassRange(species.Mass - 0.1d, species.Mass + 0.1d));
            return d;
        }
        public Data2D FromSpecies(CamecaSpecies species, out float Max, BackgroundWorker bw = null)
        {
            Data2D d = FromMassRange(new MassRange(species.Mass - 0.1d, species.Mass + 0.1d), out Max, bw);
            return d;
        }
        public ICollection<Data2D> FromSpecies(CamecaSpecies species, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null)
        {
            return FromMassRange(new MassRange(species.Mass - 0.1d, species.Mass + 0.1d), TableBaseName, OmitNumbering, bw);
        }
        public Data2D FromSpecies(CamecaSpecies species, int Layer, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null)
        {
            return FromMassRange(new MassRange(species.Mass - 0.1d, species.Mass + 0.1d), Layer, TableBaseName, OmitNumbering, bw);
        }
        public Data3D FromSpecies(CamecaSpecies species, string tableName, BackgroundWorker bw = null)
        {
            return FromMassRange(new MassRange(species.Mass - 0.1d, species.Mass + 0.1d), tableName, bw);
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
        public override List<Data2D> FromMassRange(MassRange MassRange, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null)
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
        public override Data2D FromMassRange(MassRange MassRange, int Layer, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null)
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
        public Data3D FromMassRange(MassRange MassRange, string TableName, BackgroundWorker bw = null)
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
}
