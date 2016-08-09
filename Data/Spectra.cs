using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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

namespace ImagingSIMS.Data.Spectra
{
    public abstract class Spectrum : IDisposable, INotifyPropertyChanged, ISavable
    {
        protected string _name;

        protected int _sizeX;
        protected int _sizeY;
        protected int _sizeZ;

        protected SpectrumType _specType;

        protected bool _isLoading;

        protected uint[] _intensities;
        protected float[] _masses;

        /// <summary>
        /// Gets the name of the spectrum.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        /// <summary>
        /// Gets the width of the spectrum.
        /// </summary>
        public int SizeX
        {
            get { return _sizeX; }
        }
        /// <summary>
        /// Gets the height of the spectrum.
        /// </summary>
        public int SizeY
        {
            get { return _sizeY; }
        }
        /// <summary>
        /// Gets the depth of the spectrum.
        /// </summary>
        public int SizeZ
        {
            get { return _sizeZ; }
        }

        protected float _startMass;
        protected float _endMass;
        /// <summary>
        /// Gets the start mass of the spectrum.
        /// </summary>
        public float StartMass
        {
            get
            {
                return _startMass;
            }
        }

        /// <summary>
        /// Gets the end mass of the spectrum.
        /// </summary>
        public float EndMass
        {
            get
            {
                return _endMass;
            }
        }

        /// <summary>
        /// Gets the data type of the spectrum.
        /// </summary>
        public SpectrumType SpectrumType
        {
            get { return _specType; }
        }

        /// <summary>
        /// Gets a string representation of the spectrum dimensions as "{Width}x{Height}x{Depth}"
        /// </summary>
        public string Dimensions
        {
            get { return string.Format("{0}x{1}x{2}", SizeX, SizeY, SizeZ); }
        }

        /// <summary>
        /// Gets a boolean value if data is currently being loaded into the spectrum.
        /// </summary>
        public bool IsLoading
        {
            get { return _isLoading; }
        }

        /// <summary>
        /// Disposes the spectrum.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        /// <summary>
        /// Initializes a new instance of the Spectrum class.
        /// </summary>
        /// <param name="Name">Name of the spectrum.</param>
        public Spectrum(string Name)
        {
            _name = Name;
        }

        /// <summary>
        /// Load a mass spectrum from disk.
        /// </summary>
        /// <param name="FilePath">File location to load.</param>
        /// <param name="bw">BackgroundWorker instance for the method to update.</param>
        public abstract void LoadFromFile(string FilePath, BackgroundWorker bw);
        /// <summary>
        /// Load a mass spectra series from disk.
        /// </summary>
        /// <param name="FilePaths">Array of file locations to load.</param>
        /// <param name="bw">BackgroundWorker instance for the method to update.</param>
        public abstract void LoadFromFile(string[] FilePaths, BackgroundWorker bw);

        /// <summary>
        /// Compiles a two dimensional matrix of intensities within the specified mass range.
        /// </summary>
        /// <param name="MassRange">The desired mass range.</param>
        /// <param name="Max">The maximum intensity in the DataTable.</param>
        /// <param name="bw">BackgroundWorker instance for the method to update.</param>
        /// <returns>A two dimensional intensity matrix.</returns>
        public abstract Data2D FromMassRange(MassRangePair MassRange, out float Max, BackgroundWorker bw = null);
        /// <summary>
        /// Compiles a series of two dimensional matrices of intensities for each mass spectrum within the specified mass range.
        /// </summary>
        /// <param name="MassRange">The desired mass range.</param>
        /// <param name="TableBaseName">Base name for the table series.</param>
        /// <param name="bw">BackgroundWorker instance for the method to update.</param>
        /// <returns>A two dimensional intensity matrix series.</returns>
        public abstract List<Data2D> FromMassRange(MassRangePair MassRange, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null);
        /// <summary>
        /// Compiles a series of two dimensional matrices of intensities for each mass spectrum within the specified mass range.
        /// </summary>
        /// <param name="MassRange">The desired mass range.</param>
        /// <param name="Layer">Layer of spectrum to sample.</param>
        /// <param name="TableBaseName">Base name for the table series.</param>
        /// <param name="bw">BackgroundWorker instance for the method to update.</param>
        /// <returns>A two dimensional intensity matrix series.</returns>
        public abstract Data2D FromMassRange(MassRangePair MassRange, int Layer, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null);
        /// <summary>
        /// Creates a depth profile (intensity as a function of layer number) of the the given spectrum within the specified mass range.
        /// </summary>
        /// <param name="MassRange">The desired mass range.</param>
        /// <param name="bw">BackgroundWorker instance for the method to update.</param>
        /// <returns>Double array of intensity as a function of depth.</returns>
        public double[] CreateDepthProfile(MassRangePair MassRange, BackgroundWorker bw = null)
        {            
            double[] depthProfile = new double[this.SizeZ];
            int totalSteps = this.SizeZ;

            for (int z = 0; z < this.SizeZ; z++)
            {
                Data2D layer = FromMassRange(MassRange, z, "", true, bw);

                depthProfile[z] = layer.TotalCounts;

                if (bw != null) bw.ReportProgress(Percentage.GetPercent(z, totalSteps));
            }

            if (bw != null) bw.ReportProgress(100);
            return depthProfile;
        }

        /// <summary>
        /// Converts the spectrum to a two dimensional double array. 
        /// </summary>
        /// <returns>Two dimensional array. Column 0 is mass values and column 1 is intensities.</returns>
        public abstract double[,] ToDoubleArray();
        /// <summary>
        /// Gets a series of intensites and mass values for display.
        /// </summary>
        /// <param name="Masses">Array of mass values.</param>
        /// <returns>Array of intensites.</returns>
        public abstract uint[] GetSpectrum(out float[] Masses);

        /// <summary>
        /// Saves the spectrum as an ASCII text file with the format: mass,intensity
        /// </summary>
        /// <param name="filePath">Path to save file to.</param>
        public abstract void SaveText(string filePath);
        /// <summary>
        /// Saves the spectrum as an ASCII text file with the format: mass,intensity
        /// </summary>
        /// <param name="filePath">Path to save file to.</param>
        /// <param name="binSize">Number of bins to sum together.</param>
        public abstract void SaveText(string filePath, int binSize);

        public abstract byte[] ToByteArray();
        public abstract void FromByteArray(byte[] array);
    }

    public class J105Spectrum : Spectrum
    {
        J105Stream _stream;

        /// <summary>
        /// Number of X pixels per tile
        /// </summary>
        int _pixelsX;
        /// <summary>
        /// Number of Y pixels per tile
        /// </summary>
        int _pixelsY;
        /// <summary>
        /// Number of X tiles
        /// </summary>
        int _tilesX;
        /// <summary>
        /// Number of Y tiles
        /// </summary>
        int _tilesY;

        /// <summary>
        /// Gets the underlying stream object.
        /// </summary>
        public J105Stream J105Stream
        {
            get { return _stream; }
        }

        /// <summary>
        /// Creates a new instance of J105Spectrum.
        /// </summary>
        /// <param name="Name">Name of the spectrum.</param>
        public J105Spectrum(string Name)
            : base(Name)
        {
            _specType = SpectrumType.J105;
        }
        protected override void Dispose(bool disposing)
        {
            if (_stream != null)
            {
                _stream.CloseStream();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Loads a spectrum from disk.
        /// </summary>
        /// <param name="FilePath">Path of the spectrum file.</param>
        /// <param name="bw">BackgroundWorker for reporting progress.</param>
        public override void LoadFromFile(string FilePath, BackgroundWorker bw = null)
        {
            Trace.WriteLine(string.Format("Loading {0} from disk.", FilePath));
            if (_stream == null || !_stream.IsStreamOpen)
            {
                _stream = new J105Stream(FilePath); 
                _stream.IsStreamOpenChanged += _stream_IsStreamOpenChanged;

                if (!_stream.OpenStream())
                {
                    Trace.WriteLine("The attempt to open the stream failed.");
                    throw new J105StreamException("The attempt to open the spectrum stream failed.");
                }                
            }
            else if (!_stream.StreamPath.Contains(FilePath))
            {
                Trace.WriteLine("The specified zip file is not the current file. Dispose and create a new J105Stream instance.");
                throw new ArgumentException("The specified zip file is not the current file. Dispose and create a new J105Stream instance.");
            }

            Trace.WriteLine(string.Format("Spectrum {0} contains QuickLoad: {1}.", Name, _stream.HasQuickLoadFile));
            if (_stream.HasQuickLoadFile)
            {
                _intensities = _stream.LoadFromQuickLoad();
                _masses = _stream.MassValues;
            }

            else
            {
                _masses = _stream.MassValues;
                _intensities = _stream.GetTotalSpectrum(bw);
            }

            Trace.WriteLine("Masses and intensities loaded.");

            _pixelsX = _stream.J105Parameters.RasterScanDimensionX;
            _pixelsY = _stream.J105Parameters.RasterScanDimensionY;
            _tilesX = _stream.J105Parameters.StageScanDimensionX;
            _tilesY = _stream.J105Parameters.StageScanDimensionY;

            _sizeX = _stream.J105Parameters.RasterScanDimensionX * _stream.J105Parameters.StageScanDimensionX;
            _sizeY = _stream.J105Parameters.RasterScanDimensionY * _stream.J105Parameters.StageScanDimensionY;
            _sizeZ = _stream.J105Parameters.NumberLayers;

            Trace.WriteLine(string.Format("SizeX: {0} SizeY: {1} SizeZ: {2}.", SizeX, SizeY, SizeZ));

            _startMass = _masses[0];
            _endMass = _masses[_masses.Length - 1];
        }

        void _stream_IsStreamOpenChanged(object sender, EventArgs e)
        {
            if (_stream == null) return;
        }
        /// <summary>
        /// Loads a spectrum from disk. This method is overridden from the base Spectrum class and is not valid for a J105Spectrum.
        /// </summary>
        /// <param name="FilePaths">Paths of the spectra files.</param>
        /// <param name="bw">BackgroundWorker for reporting progress.</param>
        public override void LoadFromFile(string[] FilePaths, BackgroundWorker bw)
        {
            throw new ArgumentException("J105 spectra only support a single file path.");
        }

        public override Data2D FromMassRange(MassRangePair MassRange, out float Max, BackgroundWorker bw = null)
        {
            Data2D dt = new Data2D(_sizeX, _sizeY);

            double totalSteps = _sizeX;

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
                if(bw != null) bw.ReportProgress(Percentage.GetPercent(x, totalSteps));
            }
            dt.DataName = string.Format("{0} {1}-{2}", Name, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"));
            Max = max;
            return dt;
        }
        public override List<Data2D> FromMassRange(MassRangePair MassRange, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null)
        {
            if (!_stream.IsStreamOpen) throw new ArgumentException("No V2 stream has been initialized.");

            int ct = 0;
            double totalSteps = _sizeZ * _tilesX * _pixelsX;

            if (bw != null) bw.ReportProgress(0);

            List<Data2D> returnTables = new List<Data2D>();

            int i = 0;
            for (int z = 0; z < _sizeZ; z++)
            {
                if (bw != null && bw.CancellationPending) return returnTables;

                Data2D dt = new Data2D(_sizeX, _sizeY);

                for (int tx = 0; tx < _tilesX; tx++)
                {
                    for (int x = 0; x < _pixelsX; x++)
                    {
                        int xIndex = (tx * _pixelsX) + x;
                        Parallel.For(0, _tilesY,
                            ty =>
                            {
                                //for (int ty = 0; ty < _tilesY; ty++)
                                //{
                                //for (int y = 0; y < _pixelsY; y++)
                                Parallel.For(0, _pixelsY,
                                    y =>
                                    {
                                        int yIndex = (ty * _pixelsY) + y;

                                        dt[xIndex, yIndex] = _stream.IntensityFromMassRange(z, ty, tx, y, x, 
                                            (float)MassRange.StartMass, (float)MassRange.EndMass);
                                    }
                                );
                                //}
                                ct++;
                                if (bw != null) bw.ReportProgress(Percentage.GetPercent(ct, totalSteps));
                                //if (bw.CancellationPending) return returnTables;
                            });
                        if (bw != null && bw.CancellationPending) return returnTables;
                    }
                }
                if (!OmitNumbering)
                    dt.DataName = string.Format("{0} {1}-{2} ({3})", TableBaseName, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"), ++i);
                else
                    dt.DataName = string.Format("{0} {1}-{2}", TableBaseName, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"));
                returnTables.Add(dt);
            }

            return returnTables;
        }
        public override Data2D FromMassRange(MassRangePair MassRange, int Layer, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null)
        {
            if (!_stream.IsStreamOpen) throw new ArgumentException("No V2 stream has been initialized.");
            if (Layer >= SizeZ)
                throw new ArgumentException(string.Format("Layer {0} does not exist in the spectrum (Number layers: {1}).", Layer, SizeZ));

            int ct = 0;
            double totalSteps = _sizeZ * _tilesX * _pixelsX;

            if (bw != null) bw.ReportProgress(0);

            if (bw != null && bw.CancellationPending) return null;

            Data2D dt = new Data2D(_sizeX, _sizeY);

            for (int tx = 0; tx < _tilesX; tx++)
            {
                for (int x = 0; x < _pixelsX; x++)
                {
                    int xIndex = (tx * _pixelsX) + x;
                    Parallel.For(0, _tilesY,
                        ty =>
                        {
                            //for (int ty = 0; ty < _tilesY; ty++)
                            //{
                            //for (int y = 0; y < _pixelsY; y++)
                            Parallel.For(0, _pixelsY,
                                y =>
                                {
                                    int yIndex = (ty * _pixelsY) + y;

                                    dt[xIndex, yIndex] = _stream.IntensityFromMassRange(Layer, ty, tx, y, x, (float)MassRange.StartMass, (float)MassRange.EndMass);
                                }
                            );
                            //}
                            ct++;
                            if (bw != null) bw.ReportProgress(Percentage.GetPercent(ct, totalSteps));
                            //if (bw.CancellationPending) return returnTables;
                        });
                    if (bw != null && bw.CancellationPending) return null;
                }
            }
            if (!OmitNumbering)
                dt.DataName = string.Format("{0} {1}-{2} ({3})", TableBaseName, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"), Layer + 1);
            else
                dt.DataName = string.Format("{0} {1}-{2}", TableBaseName, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"));

            return dt;
        }
        
        public override double[,] ToDoubleArray()
        {
            double[,] returnArray = new double[_masses.Length, 2];
            for (int i = 0; i < _masses.Length; i++)
            {
                returnArray[i, 0] = _masses[i];
                returnArray[i, 1] = _intensities[i];
            }
            return returnArray;
        }
        public override uint[] GetSpectrum(out float[] Masses)
        {
            Masses = _masses;
            return _intensities;
        }

        /// <summary>
        /// Saves a total spectrum as an array for quick viewing of the spectrum.
        /// </summary>
        public void SaveQuickLoad()
        {
            if (_stream == null)
            {
                throw new ArgumentException("No J105 Stream has been initialized.");
            }

            if (_intensities == null) throw new ArgumentException("No J105 spectrum has been loaded.");

            Trace.WriteLine("Saving QuickLoad file.");
            _stream.SaveQuickLoad(_intensities);

            //Close stream to commit changes to disk and reload
            Trace.WriteLine("QuickLoad saved. Closing ZipArchive.");
            _stream.CloseStream();
            Trace.WriteLine("Reopening ZipArchive.");
            bool reopened = _stream.OpenStream();
            Trace.WriteLine(string.Format("ZipArchive reopened: {0}.", reopened));
        }
        /// <summary>
        /// Updates the saved QuickLoad spectrum.
        /// </summary>
        /// <param name="TotalSpectrum">Spectrum to save.</param>
        public void UpdateQuickLoad(uint[] TotalSpectrum)
        {
            if (_stream == null)
            {
                throw new ArgumentException("No J105 Stream has been initialized.");
            }            

            Trace.WriteLine("Saving QuickLoad file.");
            _stream.SaveQuickLoad(TotalSpectrum);
            Trace.WriteLine("QuickLoad file saved.");
        }

        /// <summary>
        /// Removes mass spectra from unwanted pixels.
        /// </summary>
        /// <param name="PixelsToKeep">Pixels in the image to keep.</param>
        /// <param name="Layer">Depth layer to perform crop operation on.</param>
        /// <param name="NewFilePath">Path to save the cropped compressed file to.</param>
        /// <returns>Cropped J105 spectrum.</returns>
        public J105Spectrum Crop(List<Point> PixelsToKeep, int Layer, string NewFilePath)
        {
            string newName = Path.GetFileNameWithoutExtension(NewFilePath);
            string oldPath = J105Stream.StreamPath;
            string newPath = NewFilePath;

            //Close ZipArchive for Copy operation
            Trace.WriteLine("Closing ZipArchive.");
            J105Stream.CloseStream();
            Trace.WriteLine("Copying V2 zip file.");
            File.Copy(oldPath, newPath, true);
            //Reopen ZipArchive after completed
            Trace.WriteLine("Reopening ZipArchive.");
            bool reopened = J105Stream.OpenStream();
            Trace.WriteLine(string.Format("ZipArchive reopened: {0}.", reopened));

            //Create new spectrum from copied V2 file and load ZipArchive
            J105Spectrum cropped = new J105Spectrum(newName);
            Trace.WriteLine(string.Format("Opening cropped spectrum {0}", cropped.Name));
            cropped.LoadFromFile(newPath, null);
            Trace.WriteLine(string.Format("Spectrum {0} loaded from disk.", cropped.Name));

            long total = _tilesY * _tilesX * _pixelsY * _pixelsX;
            long pos = 0;

            //For pixels not being kept set intensity of each mass in spectrum to zero
            //for (int x = 0; x < this.SizeX; x++)
            Parallel.For(0, this.SizeX, x =>
            {
                //for (int y = 0; y < this.SizeY; y++)
                Parallel.For(0, this.SizeY, y =>
                {
                    Point location = new Point((double)x, (double)y);

                    int xTile = x / this._pixelsX;
                    int xRaster = x % this._pixelsX;
                    int yTile = y / this._pixelsY;
                    int yRaster = y % this._pixelsY;

                    bool crop = !PixelsToKeep.Contains(location);
                    if (crop)
                    {
                        if (Layer == -1)
                        {
                            for (int z = 0; z < this.SizeZ; z++)
                            {
                                cropped.J105Stream.WriteValues(z, yTile, xTile, yRaster, xRaster, null);
                            }
                        }
                        else
                        {
                            cropped.J105Stream.WriteValues(Layer, yTile, xTile, yRaster, xRaster, null);
                        }
                    }
                    Interlocked.Increment(ref pos);
                    Trace.WriteLine(string.Format("TileX: {0} TileY: {1} RasterX: {2} RasterY: {3} | Cropped: {4}. {5}% Complete.",
                        xTile, yTile, xRaster, yRaster, crop, Percentage.GetPercent(pos, total)));
                });
            });

            //Reset the stream to commit changes to disk and reload total spectrum
            Trace.WriteLine("Crop complete. Closing ZipArchive.");
            cropped.J105Stream.CloseStream();
            Trace.WriteLine("Reopening ZipArchive.");
            reopened = cropped.J105Stream.OpenStream();
            Trace.WriteLine(string.Format("ZipArchive reopened: {0}.", reopened));
            cropped._intensities = cropped._stream.GetTotalSpectrum();

            return cropped;
        }
        /// <summary>
        /// Removes mass spectra from unwanted pixels.
        /// </summary>
        /// <param name="PixelsToKeep">Pixels in the image to keep.</param>
        /// <param name="Layer">Depth layer to perform crop operation on.</param>
        /// <param name="NewFilePath">Path to save the cropped compressed file to.</param>
        /// <param name="ResizeBuffer">Padding to add to the resized cropped image.</param>
        /// <returns>Cropped J105 spectrum.</returns>
        public J105Spectrum CropAndResize(List<Point> PixelsToKeep, int Layer, string NewFilePath, int ResizeBuffer)
        {
            string newName = Path.GetFileNameWithoutExtension(NewFilePath);
            string oldPath = J105Stream.StreamPath;
            string newPath = NewFilePath;

            //Close ZipArchive for Copy operation
            Trace.WriteLine("Closing ZipArchive.");
            J105Stream.CloseStream();
            Trace.WriteLine("Copying V2 zip file.");
            File.Copy(oldPath, newPath, true);
            //Reopen ZipArchive after completed
            Trace.WriteLine("Reopening ZipArchive.");
            bool reopened = J105Stream.OpenStream();
            Trace.WriteLine(string.Format("ZipArchive reopened: {0}.", reopened));

            //Create new spectrum from copied V2 file and load ZipArchive
            J105Spectrum cropped = new J105Spectrum(newName);
            Trace.WriteLine(string.Format("Opening cropped spectrum {0}", cropped.Name));
            cropped.LoadFromFile(newPath, null);
            Trace.WriteLine(string.Format("Spectrum {0} loaded from disk.", cropped.Name));

            long total = _tilesY * _tilesX * _pixelsY * _pixelsX;
            long pos = 0;

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

            //Essentially resizing to 1 x 1 tiled image with number of pixels equal
            //to the resized width and height
            cropped._pixelsX = resizeWidth;
            cropped._pixelsY = resizeHeight;
            cropped._tilesX = 1;
            cropped._tilesY = 1;
            cropped._sizeX = resizeWidth * cropped._tilesX;
            cropped._sizeY = resizeHeight * cropped._tilesY;
            
            //Write new image parameters file with new dimensions
            cropped.J105Stream.UpdateImageDetails(resizeWidth, resizeHeight);

            //Delete all spectra in copied archive
            for (int tx = 0; tx < _tilesX; tx++)
            {
                for (int x = 0; x < _pixelsX; x++)
                {
                    for (int ty = 0; ty < _tilesY; ty++)
                    {
                        for (int y = 0; y < _pixelsY; y++)
                        {
                            for (int z = 0; z < _sizeZ; z++)
                            {
                                cropped.J105Stream.ClearSpectrum(z, tx, ty, x, y);
                            }
                        }
                    }
                }
            }

            //For pixels being kept, move to new pixel location due to resizing
            //for (int x = 0; x < this.SizeX; x++)
            Parallel.For(0, resizeWidth, x =>
            {
                //for (int y = 0; y < this.SizeY; y++)
                Parallel.For(0, resizeHeight, y =>
                {
                    //Point location = new Point((double)x, (double)y);

                    int origX = x + resizeStartX;
                    int origY = y + resizeStartY;
                    int cropX = x;
                    int cropY = y;

                    int xTile = origX / this._pixelsX;
                    int xRaster = origX % this._pixelsX;
                    int yTile = origY / this._pixelsY;
                    int yRaster = origY % this._pixelsY;

                    bool keepPixel = PixelsToKeep.Contains(new Point(origX, origY));
                    if (keepPixel)
                    {
                        if (Layer == -1)
                        {
                            for (int z = 0; z < this.SizeZ; z++)
                            {
                                uint[] array = J105Stream.GetValues(z, yTile, xTile, yRaster, xRaster);
                                cropped.J105Stream.WriteValues(z, 0, 0, cropY, cropX, array);
                            }
                        }
                        else
                        {
                            uint[] array = J105Stream.GetValues(Layer, yTile, xTile, yRaster, xRaster);
                            cropped.J105Stream.WriteValues(Layer, 0, 0, cropY, cropX, array);
                        }
                    }
                    else
                    {
                        if (Layer == -1)
                        {
                            for (int z = 0; z < this.SizeZ; z++)
                            {
                                cropped.J105Stream.WriteValues(z, 0, 0, cropY, cropX, null);
                            }
                        }
                        else
                        {
                            cropped.J105Stream.WriteValues(Layer, 0, 0, cropY, cropX, null);
                        }                        
                    }
                    Interlocked.Increment(ref pos);
                    Trace.WriteLine(string.Format("TileX: {0} TileY: {1} RasterX: {2} RasterY: {3} | Kept: {4}. {5}% Complete.",
                        xTile, yTile, xRaster, yRaster, keepPixel, Percentage.GetPercent(pos, total)));
                });
            });

           

            //New depth of the cropped spectrum is only 1
            if (Layer != -1)
                cropped._sizeZ = 1;            
            else 
                cropped._sizeZ = cropped._stream.J105Parameters.NumberLayers;

            if (J105Stream.HasQuickLoadFile)
            {
                Trace.WriteLine("Recalculating total spectrum.");
                uint[] totalSpectrum = cropped.J105Stream.GetTotalSpectrum();

                Trace.WriteLine("Updating QuickLoad file.");
                cropped.UpdateQuickLoad(totalSpectrum);
            }
            else
            {
                Trace.Write("Recalculating total spectrum.");
                uint[] totalSpectrum = cropped.J105Stream.GetTotalSpectrum();

                Trace.WriteLine("Saving QuickLoad file.");
                cropped.J105Stream.SaveQuickLoad(totalSpectrum);
            }

            //Reset the stream to commit changes to disk and reload total spectrum
            Trace.WriteLine("Crop complete. Closing ZipArchive.");
            cropped.J105Stream.CloseStream();
            Trace.WriteLine("Reopening ZipArchive.");
            reopened = cropped.J105Stream.OpenStream();
            Trace.WriteLine(string.Format("ZipArchive reopened: {0}.", reopened));

            cropped._intensities = cropped._stream.LoadFromQuickLoad();
            Trace.WriteLine("Masses and intensities loaded.");

            //cropped._pixelsX = cropped._stream.J105Parameters.RasterScanDimensionX;
            //cropped._pixelsY = cropped._stream.J105Parameters.RasterScanDimensionY;
            //cropped._tilesX = cropped._stream.J105Parameters.StageScanDimensionX;
            //cropped._tilesY = cropped._stream.J105Parameters.StageScanDimensionY;

            //cropped._sizeX = cropped._stream.J105Parameters.RasterScanDimensionX * _stream.J105Parameters.StageScanDimensionX;
            //cropped._sizeY = cropped._stream.J105Parameters.RasterScanDimensionY * _stream.J105Parameters.StageScanDimensionY;
            

            cropped._startMass = cropped._masses[0];
            cropped._endMass = cropped._masses[_masses.Length - 1];

            return cropped;
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

        #region ISavable

        // Layout:
        // (string) SpectrumName
        // (string) FilePath
        // (bool)   HasQuickLoad
        // 
        public override byte[] ToByteArray()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(ms);

                // Offset start of writing to later go back and 
                // prefix the stream with the size of the stream written
                bw.Seek(sizeof(int), SeekOrigin.Begin);

                // Write contents of object


                // Return to beginning of writer and write the length
                bw.Seek(0, SeekOrigin.Begin);
                bw.Write((int)bw.BaseStream.Length);

                // Return to start of memory stream and 
                // return byte array of the stream
                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();

                //bw.Write(J105Parameters.DepthLo);
                //bw.Write(J105Parameters.DepthHi);
                //bw.Write(J105Parameters.StageScanDimensionX);
                //bw.Write(J105Parameters.StageScanDimensionY);
                //bw.Write(J105Parameters.RasterScanDimensionX);
                //bw.Write(J105Parameters.RasterScanDimensionY);
                //bw.Write(J105Parameters.WidthMicrons);
                //bw.Write(J105Parameters.HeightMicrons);
                //bw.Write(J105Parameters.SpectrumLength);

                //bw.Write(J105Parameters.Version.Major);
                //bw.Write(J105Parameters.Version.Minor);
                //bw.Write(J105Parameters.Version.Build);
                //bw.Write(J105Parameters.Version.Revision);
                //bw.Write(J105Parameters.XYSubSample);
                //bw.Write(J105Parameters.IntensityNormalization);

                //for (int i = 0; i < _massCalibration.Count; i++)
                //{
                //    bw.Write(_massCalibration[i].Index);
                //    bw.Write(_massCalibration[i].Mass);
                //    bw.Write(_massCalibration[i].Time);
                //    bw.Write(Intensities[i]);
                //}
            }
        }
        public override void FromByteArray(byte[] array)
        {

        }
        #endregion
    }

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

        public override Data2D FromMassRange(MassRangePair MassRange, out float Max, BackgroundWorker bw = null)
        {
            Data2D dt = new Data2D(_sizeX, _sizeY);

            List<Data2D> dts = FromMassRange(MassRange, "preview", true, bw);
            Max = -1;

            if (bw !=null && bw.CancellationPending) return dt;

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
        public override List<Data2D> FromMassRange(MassRangePair MassRange, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null)
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
                        dt[x,y] = xyt[x, y].GetNumberCounts(MassToTime(_parameters.Slope, _parameters.Intercept, MassRange.StartMass),
                            MassToTime(_parameters.Slope, _parameters.Intercept, MassRange.EndMass));
                    }
                }

                if (bw != null && bw.CancellationPending) return returnTables;

                if (!OmitNumbering)
                    dt.DataName = string.Format("{0} {1}-{2} ({3})", TableBaseName, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"), ++i);
                else
                    dt.DataName = string.Format("{0} {1}-{2}", TableBaseName, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"));

                returnTables.Add(dt);

                if (bw != null)
                    bw.ReportProgress(i * 100 / _xyts.Count);
            }
            return returnTables;
        }
        public override Data2D FromMassRange(MassRangePair MassRange, int Layer, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null)
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
                for (int x = 0; x <cropped._sizeX; x++)
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

                                currentLayer[x,y] = new FlightTimeArray(xyt, xyti);
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

    public class Cameca1280Spectrum : Spectrum
    {
        List<CamecaSpecies> _species;

        // _matrix[layer][species][x,y]
        List<Data2D[]> _matrix;

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

        private void setMatrixValue(int x, int y, int z, int species, float value)
        {
            _matrix[z][species][x, y] = value;
        }
        private float getMatrixValue(int x, int y, int z, int species)
        {
            return _matrix[z][species][x, y];
        }
        private float getMatrixValue(int x, int y, int z, MassRangePair range)
        {
            float value = 0;
            for (int i = 0; i < _species.Count; i++)
            {
                if(_species[i].Mass >= range.StartMass && _species[i].Mass < range.EndMass)
                {
                    value += getMatrixValue(x, y, z, i);
                }

            }

            return value;
        }

        public Cameca1280Spectrum(string spectrumName)
            : base(spectrumName)
        {
            _specType = SpectrumType.Cameca1280;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        
        public Data2D FromSpecies(CamecaSpecies species, out float Max, BackgroundWorker bw = null)
        {
            Data2D d = FromMassRange(new MassRangePair(species.Mass - 0.1d, species.Mass + 0.1d), out Max, bw);
            return d;
        }
        public async Task<Data2D> FromSpeciesAsync(CamecaSpecies species)
        {
            float tableMax = 0;
            Data2D d = await Task.Run(() => (FromSpecies(species, out tableMax, null)));
            return d;
        }
        public List<Data2D> FromSpecies(CamecaSpecies species, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null)
        {
            return FromMassRange(new MassRangePair(species.Mass - 0.1d, species.Mass + 0.1d), TableBaseName, OmitNumbering, bw);
        }
        public async Task<List<Data2D>> FromSpeciesAsync(CamecaSpecies species, string tableBaseName, bool omitNumbering)
        {
            return await Task.Run(() => FromSpecies(species, tableBaseName, omitNumbering, null));
        }
        public Data2D FromSpecies(CamecaSpecies species, int Layer, string TableBaseName, bool OmitNumbering, BackgroundWorker bw= null)
        {
            return FromMassRange(new MassRangePair(species.Mass - 0.1d, species.Mass + 0.1d), Layer, TableBaseName, OmitNumbering, bw);
        }
        public async Task<Data2D> FromSpeciesAsync(CamecaSpecies species, int layer, string tableBaseName, bool omitNumbering)
        {
            return await Task.Run(() => FromSpecies(species, layer, tableBaseName, omitNumbering, null));
        }
        public Data3D FromSpecies(CamecaSpecies species, string tableName, BackgroundWorker bw = null)
        {
            return FromMassRange(new MassRangePair(species.Mass - 0.1d, species.Mass + 0.1d), tableName, bw);
        }
        public async Task<Data3D> FromSpeciesAsync(CamecaSpecies species, string tableName)
        {
            return await Task.Run(() => FromSpecies(species, tableName, null));
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

        public override void LoadFromFile(string[] FilePaths, BackgroundWorker bw)
        {
            doLoad(FilePaths, bw);
        }
        public override void LoadFromFile(string FilePath, BackgroundWorker bw)
        {
            doLoad(new string[] { FilePath }, bw);
        }
        private void doLoad(string[] filePaths, BackgroundWorker bw)
        {
            int imageSize = 0;
            _species = new List<CamecaSpecies>();
            int[] numCyclesForFile = new int[filePaths.Length];

            _matrix = new List<Data2D[]>();

            int totalSteps = filePaths.Length * 2;
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
                    bw.ReportProgress(i * 100 / totalSteps);
            }

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
                float[,,,] tempValues = new float[_species.Count, numCyclesForFile[i], imageSize, imageSize];

                for (int s = 0; s < _species.Count; s++)
                {
                    for (int z = 0; z < numCyclesForFile[i]; z++)
                    {
                        for (int y = 0; y < imageSize; y++)
                        {
                            for (int x = 0; x < imageSize; x++)
                            {
                                tempValues[s, z, y, x] = values[pos++];
                            }
                        }

                    }
                }

                for (int z = 0; z < numCyclesForFile[i]; z++)
                {
                    Data2D[] layer = new Data2D[_species.Count];
                    for (int s = 0; s < _species.Count; s++)
                    {
                        CamecaSpecies species = _species[s];
                        Data2D layerSpecies = new Data2D(imageSize, imageSize);
                        for (int y = 0; y < imageSize; y++)
                        {
                            for (int x = 0; x < imageSize; x++)
                            {
                                layerSpecies[x, y] = tempValues[s, z, y, x];
                            }
                        }
                        layer[s] = layerSpecies;
                    }
                    _matrix.Add(layer);
                }

                if (bw != null)
                    bw.ReportProgress((filePaths.Length + i) * 100 / totalSteps);
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

    public class MassCalibrationException : Exception
    {
        bool missingSlope;
        bool missingInt;
        bool negativeSlope;

        string reason;

        public bool MissingSlope { get { return missingSlope; } }
        public bool MissingInt { get { return missingInt; } }
        public bool NegativeSlope { get { return negativeSlope; } }
        public string Reason { get { return reason; } }

        public MassCalibrationException(string Message)
            : base(Message)
        {
            reason = "None specified.";
        }
        public MassCalibrationException(string Message, bool MissingSlope, bool MissingIntercept)
            : base(Message)
        {
            missingSlope = MissingSlope;
            missingInt = MissingIntercept;

            if (missingSlope && missingInt)
            {
                reason = "Slope and y-intercept parameters are missing.";
            }
            else
            {
                if (missingSlope)
                {
                    reason = "Slope parameter is missing.";
                }
                else if (missingInt)
                {
                    reason = "Y-intercept parameter is missing.";
                }
            }
        }
        public MassCalibrationException(string Message, bool NegativeSlope)
            : base(Message)
        {
            negativeSlope = NegativeSlope;

            reason = "A negative slope value was entered.";
        }
    }
    struct LoadWorkerArguments
    {
        public string[] FilePaths;
        public int BinStart;
        public int BinEnd;

        public void LoadWorkerResults()
        {
            BinStart = 0;
            BinEnd = 0;
            FilePaths = null;
        }
    }

    public delegate void MSLoadUpdatedEventHandler(object sender, MSLoadUpdatedEventArgs args);
    public class MSLoadUpdatedEventArgs : EventArgs
    {
        public int Percentage;
        public EventArgs e;

        public MSLoadUpdatedEventArgs(int Percentage, EventArgs e)
            : base()
        {
            this.e = e;
            this.Percentage = Percentage;
        }
    }

    public struct BioToFParameters
    {
        private const int _headerLength = 4656;

        public int Detector;            //KORE=1408 DA500=1034
        public int DiskFlag;            //TRUE/FALSE
        public int E;                   //RECOMPUTE=1000 NO_RECOMPUTE=1001
        public int FileType;            //TOF=22 XYI=24 BMP=23 XYT=25 
        public int IonSign;             //POS=37 NEG=38
        public int Laser;               //LASER=5 (OR TRUE/FALSE?)
        public int Mode;                //CREATEDEPTHP=96 CREATEMALDIDP=97 CREATEMATRIX=98
        public int SIMS;                //SIMS=6 (OR TRUE/FALSE?)
        public int TD;                  //TRUE/FALSE?
        public int TDC;                 //TRUE/FALSE?
        public int Imaging_Pixels;
        public int Imaging_Repeats;
        public int Imaging_WhiteCount;
        public int Imaging_BaselineSubtraction;
        public int Imaging_ShotsPerPixel;
        public int Imaging_RasterStepped;
        public int Imaging_LineByLineDisplay;
        public int Imaging_DEM;
        public int Imaging_AcquireSIMS_SEM;
        public int Imaging_ShowGridFlag;
        public int Imaging_RetroTDCFlag;
        public Int64 Time;
        public double Version;
        public double AnodeVoltage;
        public double ExtractorVoltage;
        public double StigAmplitude;
        public double StigAngle;
        public double AlignX;
        public double AlignY;
        public double Lens1Voltage;
        public double Lens2Voltage;
        public int BlankVoltage;
        public int Gain;
        public int FrameRate;
        public int ScanRot;
        public int YPixel;
        public int XPixel;
        public int Angle;
        public int Resolution;
        public int ScanType;
        public int OffsetX;
        public int OffsetY;
        public int BeamPositionX;
        public int BeamPositionY;
        public double Slope;
        public double Intercept;
        public double SlopeSIMS_TDC;
        public double InterceptSIMS_TDC;
        public double SlopeSIMS_TD;
        public double InterceptSIMS_TD;
        public double SlopeLaser_TDC;
        public double InterceptLaser_TDC;
        public double SlopeLaser_TD;
        public double InterceptLaser_TD;
        public int SEM_WC;
        public int SEM_Pixels;
        public int SEM_Repeats;
        public int SEM_ConvPerPixel;
        public int SEM_RasterStepped;
        public int SEM_LineByLineDisplay;
        public TimeDelay TimingsSIMS_TDC;
        public TimeDelay TimingsSIMS_TD;
        public TimeDelay TimingsLaser_TDC;
        public TimeDelay TimingsLaser_TD;
        public int IonPulseWidth;
        public int StageDelay;
        public int LaserDelay;
        public int LaserFireDelay;
        public PulseFlag PulseFlag;
        public int RepRate;
        public int TrigMode;
        public int ChopperDelay;
        public int Bin1;            // Hardware bins -> use this
        public int Bin2;            // Hardware bins -> use this
        public int BinEnd;          // Software bins
        public int BinStart;        // Software bins
        public int Cycl;
        public int CyclesTotal;
        public double DA500Res;
        public double KoreRes;
        public int Maximum;
        public bool TDtoTDC;
        public int StartBin;
        public int TimeEnd;
        public int TimeMask;
        public int TimerFrequency;
        public int TimeStart;
        public int TDCStartTrigger;
        public int TDCStopTrigger;
        public double TDCStartThresh;
        public double TDCStopThresh;
        public int TDSingleIonPeakIntegral;
        public int TDDescriminator;
        public int StagePositive;
        public int StageNegative;
        public int Retard_s;
        public int Retard_l;
        public int Reflect_s;
        public int Reflect_l;
        public int Lens1;
        public int Lens2;
        public int Lens2Polarity;
        public int MCPGain;
        public int PostAcceleration;
        public int Grid;

        public BioToFParameters(byte[] buffer)
        {
            Detector = BitConverter.ToInt32(buffer, 0);
            DiskFlag = BitConverter.ToInt32(buffer, 4);
            E = BitConverter.ToInt32(buffer, 8);
            FileType = BitConverter.ToInt32(buffer, 12);
            IonSign = BitConverter.ToInt32(buffer, 16);
            Laser = BitConverter.ToInt32(buffer, 20);
            Mode = BitConverter.ToInt32(buffer, 24);
            SIMS = BitConverter.ToInt32(buffer, 28);
            TD = BitConverter.ToInt32(buffer, 32);
            TDC = BitConverter.ToInt32(buffer, 36);

            Imaging_Pixels = BitConverter.ToInt32(buffer, 128);
            Imaging_Repeats = BitConverter.ToInt32(buffer, 132);
            Imaging_WhiteCount = BitConverter.ToInt32(buffer, 136);
            Imaging_BaselineSubtraction = BitConverter.ToInt32(buffer, 140);
            Imaging_ShotsPerPixel = BitConverter.ToInt32(buffer, 144);
            Imaging_RasterStepped = BitConverter.ToInt32(buffer, 148);
            Imaging_LineByLineDisplay = BitConverter.ToInt32(buffer, 152);
            Imaging_DEM = BitConverter.ToInt32(buffer, 156);
            Imaging_AcquireSIMS_SEM = BitConverter.ToInt32(buffer, 160);
            Imaging_ShowGridFlag = BitConverter.ToInt32(buffer, 176);
            Imaging_RetroTDCFlag = BitConverter.ToInt32(buffer, 180);

            Time = BitConverter.ToInt64(buffer, 1656);
            Version = BitConverter.ToDouble(buffer, 1664);

            AnodeVoltage = BitConverter.ToDouble(buffer, 2696);
            ExtractorVoltage = BitConverter.ToDouble(buffer, 2704);
            StigAmplitude = BitConverter.ToDouble(buffer, 2712);
            StigAngle = BitConverter.ToDouble(buffer, 2720);
            AlignX = BitConverter.ToDouble(buffer, 2728);
            AlignY = BitConverter.ToDouble(buffer, 2736);
            Lens1Voltage = BitConverter.ToDouble(buffer, 2744);
            Lens2Voltage = BitConverter.ToDouble(buffer, 2752);
            BlankVoltage = BitConverter.ToInt32(buffer, 2760);
            Gain = BitConverter.ToInt32(buffer, 2764);
            FrameRate = BitConverter.ToInt32(buffer, 2768);
            ScanRot = BitConverter.ToInt32(buffer, 2772);
            YPixel = BitConverter.ToInt32(buffer, 2776);
            XPixel = BitConverter.ToInt32(buffer, 2780);
            Angle = BitConverter.ToInt32(buffer, 2784);
            Resolution = BitConverter.ToInt32(buffer, 2788);
            ScanType = BitConverter.ToInt32(buffer, 2792);
            OffsetX = BitConverter.ToInt32(buffer, 2796);
            OffsetY = BitConverter.ToInt32(buffer, 2800);
            BeamPositionX = BitConverter.ToInt32(buffer, 2804);
            BeamPositionY = BitConverter.ToInt32(buffer, 2808);

            Slope = BitConverter.ToDouble(buffer, 3216);
            Intercept = BitConverter.ToDouble(buffer, 3224);
            SlopeSIMS_TDC = BitConverter.ToDouble(buffer, 3232);
            InterceptSIMS_TDC = BitConverter.ToDouble(buffer, 3240);
            SlopeSIMS_TD = BitConverter.ToDouble(buffer, 3248);
            InterceptSIMS_TD = BitConverter.ToDouble(buffer, 3256);
            SlopeLaser_TDC = BitConverter.ToDouble(buffer, 3264);
            InterceptLaser_TDC = BitConverter.ToDouble(buffer, 3272);
            SlopeLaser_TD = BitConverter.ToDouble(buffer, 3280);
            InterceptLaser_TD = BitConverter.ToDouble(buffer, 3288);

            SEM_WC = BitConverter.ToInt32(buffer, 3392);
            SEM_Pixels = BitConverter.ToInt32(buffer, 3396);
            SEM_Repeats = BitConverter.ToInt32(buffer, 3400);
            SEM_ConvPerPixel = BitConverter.ToInt32(buffer, 3404);
            SEM_RasterStepped = BitConverter.ToInt32(buffer, 3408);
            SEM_LineByLineDisplay = BitConverter.ToInt32(buffer, 3412);

            TimingsSIMS_TDC = new TimeDelay(buffer, 3444);
            TimingsSIMS_TD = new TimeDelay(buffer, 3468);
            TimingsLaser_TDC = new TimeDelay(buffer, 3492);
            TimingsLaser_TD = new TimeDelay(buffer, 3516);
            IonPulseWidth = BitConverter.ToInt32(buffer, 3540);
            StageDelay = BitConverter.ToInt32(buffer, 3544);
            LaserDelay = BitConverter.ToInt32(buffer, 3548);
            LaserFireDelay = BitConverter.ToInt32(buffer, 3552);
            PulseFlag = new PulseFlag(buffer, 3556);
            RepRate = BitConverter.ToInt32(buffer, 3568);
            TrigMode = BitConverter.ToInt32(buffer, 3572);
            ChopperDelay = BitConverter.ToInt32(buffer, 3576);

            Bin1 = BitConverter.ToInt32(buffer, 3940);
            Bin2 = BitConverter.ToInt32(buffer, 3944);
            BinEnd = BitConverter.ToInt32(buffer, 3948);
            BinStart = BitConverter.ToInt32(buffer, 3952);
            Cycl = BitConverter.ToInt32(buffer, 3956);
            CyclesTotal = BitConverter.ToInt32(buffer, 3960);
            DA500Res = BitConverter.ToDouble(buffer, 3968);
            KoreRes = BitConverter.ToDouble(buffer, 3976);
            Maximum = BitConverter.ToInt32(buffer, 3984);
            TDtoTDC = BitConverter.ToBoolean(buffer, 3988);

            StartBin = BitConverter.ToInt32(buffer, 3996);
            TimeEnd = BitConverter.ToInt32(buffer, 4000);
            TimeMask = BitConverter.ToInt32(buffer, 4004);
            TimerFrequency = BitConverter.ToInt32(buffer, 4008);
            TimeStart = BitConverter.ToInt32(buffer, 4012);
            TDCStartTrigger = BitConverter.ToInt32(buffer, 4016);
            TDCStopTrigger = BitConverter.ToInt32(buffer, 4020);
            TDCStartThresh = BitConverter.ToDouble(buffer, 4024);
            TDCStopThresh = BitConverter.ToDouble(buffer, 4032);
            TDSingleIonPeakIntegral = BitConverter.ToInt32(buffer, 4040);
            TDDescriminator = BitConverter.ToInt32(buffer, 4044);

            StagePositive = BitConverter.ToInt32(buffer, 4216);
            StageNegative = BitConverter.ToInt32(buffer, 4220);
            Retard_s = BitConverter.ToInt32(buffer, 4224);
            Retard_l = BitConverter.ToInt32(buffer, 4228);
            Reflect_s = BitConverter.ToInt32(buffer, 4232);
            Reflect_l = BitConverter.ToInt32(buffer, 4236);
            Lens1 = BitConverter.ToInt32(buffer, 4240);
            Lens2 = BitConverter.ToInt32(buffer, 4244);
            Lens2Polarity = BitConverter.ToInt32(buffer, 4248);
            MCPGain = BitConverter.ToInt32(buffer, 4252);
            PostAcceleration = BitConverter.ToInt32(buffer, 4256);
            Grid = BitConverter.ToInt32(buffer, 4260);
        }

        public byte[] ToByteArray()
        {
            byte[] array = new byte[_headerLength];

            BitConverter.GetBytes(Detector).CopyTo(array, 0);
            BitConverter.GetBytes(DiskFlag).CopyTo(array, 4);
            BitConverter.GetBytes(E).CopyTo(array, 8);
            BitConverter.GetBytes(FileType).CopyTo(array, 12);
            BitConverter.GetBytes(IonSign).CopyTo(array, 16);
            BitConverter.GetBytes(Laser).CopyTo(array, 20);
            BitConverter.GetBytes(Mode).CopyTo(array, 24);
            BitConverter.GetBytes(SIMS).CopyTo(array, 28);
            BitConverter.GetBytes(TD).CopyTo(array, 32);
            BitConverter.GetBytes(TDC).CopyTo(array, 36);

            BitConverter.GetBytes(Imaging_Pixels).CopyTo(array, 128);
            BitConverter.GetBytes(Imaging_Repeats).CopyTo(array, 132);
            BitConverter.GetBytes(Imaging_WhiteCount).CopyTo(array, 136);
            BitConverter.GetBytes(Imaging_BaselineSubtraction).CopyTo(array, 140);
            BitConverter.GetBytes(Imaging_ShotsPerPixel).CopyTo(array, 144);
            BitConverter.GetBytes(Imaging_RasterStepped).CopyTo(array, 148);
            BitConverter.GetBytes(Imaging_LineByLineDisplay).CopyTo(array, 152);
            BitConverter.GetBytes(Imaging_DEM).CopyTo(array, 156);
            BitConverter.GetBytes(Imaging_AcquireSIMS_SEM).CopyTo(array, 160);
            BitConverter.GetBytes(Imaging_ShowGridFlag).CopyTo(array, 176);
            BitConverter.GetBytes(Imaging_RetroTDCFlag).CopyTo(array, 180);

            BitConverter.GetBytes(Time).CopyTo(array, 1656);
            BitConverter.GetBytes(Version).CopyTo(array, 1664);

            BitConverter.GetBytes(AnodeVoltage).CopyTo(array, 2696);
            BitConverter.GetBytes(ExtractorVoltage).CopyTo(array, 2704);
            BitConverter.GetBytes(StigAmplitude).CopyTo(array, 2712);
            BitConverter.GetBytes(StigAngle).CopyTo(array, 2720);
            BitConverter.GetBytes(AlignX).CopyTo(array, 2728);
            BitConverter.GetBytes(AlignY).CopyTo(array, 2736);
            BitConverter.GetBytes(Lens1Voltage).CopyTo(array, 2744);
            BitConverter.GetBytes(Lens2Voltage).CopyTo(array, 2752);
            BitConverter.GetBytes(BlankVoltage).CopyTo(array, 2760);
            BitConverter.GetBytes(Gain).CopyTo(array, 2764);
            BitConverter.GetBytes(FrameRate).CopyTo(array, 2768);
            BitConverter.GetBytes(ScanRot).CopyTo(array, 2772);
            BitConverter.GetBytes(YPixel).CopyTo(array, 2776);
            BitConverter.GetBytes(XPixel).CopyTo(array, 2780);
            BitConverter.GetBytes(Angle).CopyTo(array, 2784);
            BitConverter.GetBytes(Resolution).CopyTo(array, 2788);
            BitConverter.GetBytes(ScanType).CopyTo(array, 2792);
            BitConverter.GetBytes(OffsetX).CopyTo(array, 2796);
            BitConverter.GetBytes(OffsetY).CopyTo(array, 2800);
            BitConverter.GetBytes(BeamPositionX).CopyTo(array, 2804);
            BitConverter.GetBytes(BeamPositionY).CopyTo(array, 2808);

            BitConverter.GetBytes(Slope).CopyTo(array, 3216);
            BitConverter.GetBytes(Intercept).CopyTo(array, 3224);
            BitConverter.GetBytes(SlopeSIMS_TDC).CopyTo(array, 3232);
            BitConverter.GetBytes(InterceptSIMS_TDC).CopyTo(array, 3240);
            BitConverter.GetBytes(SlopeSIMS_TD).CopyTo(array, 3248);
            BitConverter.GetBytes(InterceptSIMS_TD).CopyTo(array, 3256);
            BitConverter.GetBytes(SlopeLaser_TDC).CopyTo(array, 3264);
            BitConverter.GetBytes(InterceptLaser_TDC).CopyTo(array, 3272);
            BitConverter.GetBytes(SlopeLaser_TD).CopyTo(array, 3280);
            BitConverter.GetBytes(InterceptLaser_TD).CopyTo(array, 3288);

            BitConverter.GetBytes(SEM_WC).CopyTo(array, 3392);
            BitConverter.GetBytes(SEM_Pixels).CopyTo(array, 3396);
            BitConverter.GetBytes(SEM_Repeats).CopyTo(array, 3400);
            BitConverter.GetBytes(SEM_ConvPerPixel).CopyTo(array, 3404);
            BitConverter.GetBytes(SEM_RasterStepped).CopyTo(array, 3408);
            BitConverter.GetBytes(SEM_LineByLineDisplay).CopyTo(array, 3412);

            TimingsSIMS_TDC.ToByteArray().CopyTo(array, 3444);
            TimingsSIMS_TD.ToByteArray().CopyTo(array, 3468);
            TimingsLaser_TDC.ToByteArray().CopyTo(array, 3492);
            TimingsLaser_TD.ToByteArray().CopyTo(array, 3516);
            BitConverter.GetBytes(IonPulseWidth).CopyTo(array, 3540);
            BitConverter.GetBytes(StageDelay).CopyTo(array, 3544);
            BitConverter.GetBytes(LaserDelay).CopyTo(array, 3548);
            BitConverter.GetBytes(LaserFireDelay).CopyTo(array, 3552);
            PulseFlag.ToByteArray().CopyTo(array, 3556);
            BitConverter.GetBytes(RepRate).CopyTo(array, 3568);
            BitConverter.GetBytes(TrigMode).CopyTo(array, 3572);
            BitConverter.GetBytes(ChopperDelay).CopyTo(array, 3576);

            BitConverter.GetBytes(Bin1).CopyTo(array, 3940);
            BitConverter.GetBytes(Bin2).CopyTo(array, 3944);
            BitConverter.GetBytes(BinEnd).CopyTo(array, 3948);
            BitConverter.GetBytes(BinStart).CopyTo(array, 3952);
            BitConverter.GetBytes(Cycl).CopyTo(array, 3956);
            BitConverter.GetBytes(CyclesTotal).CopyTo(array, 3960);
            BitConverter.GetBytes(DA500Res).CopyTo(array, 3968);
            BitConverter.GetBytes(KoreRes).CopyTo(array, 3976);
            BitConverter.GetBytes(Maximum).CopyTo(array, 3984);
            BitConverter.GetBytes(TDtoTDC).CopyTo(array, 3988);

            BitConverter.GetBytes(StartBin).CopyTo(array, 3996);
            BitConverter.GetBytes(TimeEnd).CopyTo(array, 4000);
            BitConverter.GetBytes(TimeMask).CopyTo(array, 4004);
            BitConverter.GetBytes(TimerFrequency).CopyTo(array, 4008);
            BitConverter.GetBytes(TimeStart).CopyTo(array, 4012);
            BitConverter.GetBytes(TDCStartTrigger).CopyTo(array, 4016);
            BitConverter.GetBytes(TDCStopTrigger).CopyTo(array, 4020);
            BitConverter.GetBytes(TDCStartThresh).CopyTo(array, 4024);
            BitConverter.GetBytes(TDCStopThresh).CopyTo(array, 4032);
            BitConverter.GetBytes(TDSingleIonPeakIntegral).CopyTo(array, 4040);
            BitConverter.GetBytes(TDDescriminator).CopyTo(array, 4044);

            BitConverter.GetBytes(StagePositive).CopyTo(array, 4216);
            BitConverter.GetBytes(StageNegative).CopyTo(array, 4220);
            BitConverter.GetBytes(Retard_s).CopyTo(array, 4224);
            BitConverter.GetBytes(Retard_l).CopyTo(array, 4228);
            BitConverter.GetBytes(Reflect_s).CopyTo(array, 4232);
            BitConverter.GetBytes(Reflect_l).CopyTo(array, 4236);
            BitConverter.GetBytes(Lens1).CopyTo(array, 4240);
            BitConverter.GetBytes(Lens2).CopyTo(array, 4244);
            BitConverter.GetBytes(Lens2Polarity).CopyTo(array, 4248);
            BitConverter.GetBytes(MCPGain).CopyTo(array, 4252);
            BitConverter.GetBytes(PostAcceleration).CopyTo(array, 4256);
            BitConverter.GetBytes(Grid).CopyTo(array, 4260);

            return array;
        }
    }
    public struct TimeDelay
    {
        int A;
        int B;
        int C;
        int D;
        int E;
        int F;

        public TimeDelay(byte[] array)
        {
            A = BitConverter.ToInt32(array, 0);
            B = BitConverter.ToInt32(array, 4);
            C = BitConverter.ToInt32(array, 8);
            D = BitConverter.ToInt32(array, 12);
            E = BitConverter.ToInt32(array, 16);
            F = BitConverter.ToInt32(array, 20);
        }
        public TimeDelay(byte[] array, int offset)
        {
            A = BitConverter.ToInt32(array, offset + 0);
            B = BitConverter.ToInt32(array, offset + 4);
            C = BitConverter.ToInt32(array, offset + 8);
            D = BitConverter.ToInt32(array, offset + 12);
            E = BitConverter.ToInt32(array, offset + 16);
            F = BitConverter.ToInt32(array, offset + 20);
        }

        public byte[] ToByteArray()
        {
            byte[] array = new byte[6 * sizeof(int)];

            BitConverter.GetBytes(A).CopyTo(array, 0);
            BitConverter.GetBytes(B).CopyTo(array, 4);
            BitConverter.GetBytes(C).CopyTo(array, 8);
            BitConverter.GetBytes(D).CopyTo(array, 12);
            BitConverter.GetBytes(E).CopyTo(array, 16);
            BitConverter.GetBytes(F).CopyTo(array, 20);

            return array;
        }
    }
    public struct PulseFlag
    {
        int Stage;
        int Laser;
        int IonPulse;

        public PulseFlag(byte[] array)
        {
            Stage = BitConverter.ToInt32(array, 0);
            Laser = BitConverter.ToInt32(array, 4);
            IonPulse = BitConverter.ToInt32(array, 8);
        }
        public PulseFlag(byte[] array, int offset)
        {
            Stage = BitConverter.ToInt32(array, offset + 0);
            Laser = BitConverter.ToInt32(array, offset + 4);
            IonPulse = BitConverter.ToInt32(array, offset + 8);
        }

        public byte[] ToByteArray()
        {
            byte[] array = new byte[3 * sizeof(int)];

            BitConverter.GetBytes(Stage).CopyTo(array, 0);
            BitConverter.GetBytes(Laser).CopyTo(array, 4);
            BitConverter.GetBytes(IonPulse).CopyTo(array, 8);

            return array;
        }
    }

    public enum SpectrumType
    {
        J105, BioToF, Cameca1280, Generic, None
    }

    public delegate void SpectrumLoadedEventHandler(object sender, SpectrumLoadedEvenArgs e);
    public class SpectrumLoadedEvenArgs : EventArgs
    {
        public SpectrumLoadedEvenArgs()
            : base()
        {
        }
    }
    public delegate void TablesGeneratedEventHandler(object sender, TablesGeneratedEventArgs e);
    public class TablesGeneratedEventArgs : EventArgs
    {
        public TablesGeneratedEventArgs()
            : base()
        {
        }
    }

    public partial class J105Stream
    {
        public bool IsStreamValid
        {
            get { return GetIsStreamValid(_streamPath); }
        }
        public float[] MassValues
        {
            get
            {
                if (_massCalibration == null) return null;

                float[] massValues = new float[_massCalibration.Count];
                for (int i = 0; i < _massCalibration.Count; i++)
                {
                    massValues[i] = _massCalibration[i].Mass;
                }

                return massValues;
            }
        }
        public float[] TimeValues
        {
            get
            {
                if (_massCalibration == null) return null;

                float[] timeValues = new float[_massCalibration.Count];
                for (int i = 0; i < _massCalibration.Count; i++)
                {
                    timeValues[i] = _massCalibration[i].Time;
                }

                return timeValues;
            }
        }

        string _streamPath;
        bool _isStreamOpen;
        J105DataParameters _j105Parameters;
        bool _hasQuickLoadFile;
        ZipArchive _zipArchive;

        MassCalibration _massCalibration;

        public string StreamPath
        {
            get { return _streamPath; }
            private set { _streamPath = value; }
        }
        public bool IsStreamOpen
        {
            get { return _isStreamOpen; }
            private set
            {
                _isStreamOpen = value;
                if (IsStreamOpenChanged != null)
                {
                    IsStreamOpenChanged(this, EventArgs.Empty);
                }
            }
        }
        public J105DataParameters J105Parameters
        {
            get { return _j105Parameters; }
            private set { _j105Parameters = value; }
        }
        public bool HasQuickLoadFile
        {
            get { return _hasQuickLoadFile; }
        }

        public int SpectrumLength
        {
            get
            {
                if (_j105Parameters == null) return 0;
                else return _j105Parameters.SpectrumLength;
            }
        }

        public event EventHandler IsStreamOpenChanged;

        public J105Stream(string FilePath)
        {
            IsStreamOpen = false;
            J105Parameters = new J105DataParameters();

            _streamPath = FilePath;

            if (!IsStreamValid)
            {
                throw new J105StreamException("Could not initalize the stream because the given path is not valid.");
            }
        }

        private bool GetIsStreamValid(string FilePath)
        {
            try
            {
                if (!File.Exists(_streamPath))
                {
                    if (!_streamPath.EndsWith(".zip"))
                    {
                        string addZip = _streamPath + ".zip";
                        if (File.Exists(addZip))
                        {
                            _streamPath = addZip;
                        }
                        else
                        {
                            throw new FileNotFoundException("Could not locate the specified V2 file.");
                        }
                    }
                    else
                    {
                        string minusZip = _streamPath.Remove(_streamPath.Length - 4, 4);
                        if (File.Exists(minusZip))
                        {
                            _streamPath = minusZip;
                        }
                        else
                        {
                            throw new FileNotFoundException("Could not locate the specified V2 file.");
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool OpenStream()
        {
            try
            {
                _zipArchive = ZipFile.Open(_streamPath, ZipArchiveMode.Update);
                _hasQuickLoadFile = CheckForQuickLoad();

                ReadImageDetails();
                ReadDetails();
                ReadMasses();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed to open J105 stream. " + ex.Message);
                return false;
            }
            IsStreamOpen = true;
            return true;
        }
        public void CloseStream()
        {
            if (_zipArchive != null)
            {
                _zipArchive.Dispose();
                _zipArchive = null;
            }
        }

        private bool CheckForQuickLoad()
        {
            ZipArchiveEntry entry = _zipArchive.GetEntry(StreamFilePaths.QuickLoadFile);
            return entry != null;
        }
        private void ReadImageDetails()
        {
            ZipArchiveEntry entry = _zipArchive.GetEntry(StreamFilePaths.ImageDetails);
            List<string> lines = new List<string>();

            using (StreamReader stream = new StreamReader(entry.Open()))
            {
                while (!stream.EndOfStream)
                {
                    lines.Add(stream.ReadLine());
                }
            }

            if (lines.Count == 0) throw new ArgumentException("Could not read details from " + StreamFilePaths.ImageDetails);

            foreach (string s in lines)
            {
                if (s.Contains("DepthLo"))
                {
                    J105Parameters.DepthLo = ValueFromLine(s);
                    continue;
                }
                if (s.Contains("DepthHi"))
                {
                    J105Parameters.DepthHi = ValueFromLine(s);
                    continue;
                }
                if (s.Contains("StageScanDimensionX"))
                {
                    J105Parameters.StageScanDimensionX = ValueFromLine(s);
                    continue;
                }
                if (s.Contains("StageScanDimensionY"))
                {
                    J105Parameters.StageScanDimensionY = ValueFromLine(s);
                    continue;
                }
                if (s.Contains("RasterScanDimensionX"))
                {
                    J105Parameters.RasterScanDimensionX = ValueFromLine(s);
                    continue;
                }
                if (s.Contains("RasterScanDimensionY"))
                {
                    J105Parameters.RasterScanDimensionY = ValueFromLine(s);
                    continue;
                }
                if (s.Contains("WidthInMicrons"))
                {
                    J105Parameters.WidthMicrons = ValueFromLine(s);
                    continue;
                }
                if (s.Contains("HeightInMicrons"))
                {
                    J105Parameters.HeightMicrons = ValueFromLine(s);
                    continue;
                }
                if (s.Contains("SpectrumLength"))
                {
                    J105Parameters.SpectrumLength = ValueFromLine(s);
                    continue;
                }
                if (s.Contains("WidthInPixels"))
                {
                    J105Parameters.WidthPixels = ValueFromLine(s);
                    continue;
                }
                if (s.Contains("HeightInPixels"))
                {
                    J105Parameters.HeightPixels = ValueFromLine(s);
                    continue;
                }
            }
        }
        private void ReadDetails()
        {
            ZipArchiveEntry entry = _zipArchive.GetEntry(StreamFilePaths.DetailsFile);
            List<string> lines = new List<string>();
            using (StreamReader stream = new StreamReader(entry.Open()))
            {
                while (!stream.EndOfStream)
                {
                    lines.Add(stream.ReadLine());
                }
            }

            if (lines.Count == 0) throw new ArgumentException("Could not read details from " + StreamFilePaths.DetailsFile);

            foreach (string s in lines)
            {
                if (s.Contains("Versin"))
                {
                    char[] delim = new char[1] { '.' };
                    string[] parts = s.Split(delim);

                    int[] values = new int[4];
                    values[0] = ValueFromLine(parts[0]);
                    for (int i = 1; i < 4; i++)
                    {
                        int t;
                        if (int.TryParse(parts[i], out t)) values[i] = t;
                        else values[i] = -1;
                    }
                    J105Parameters.Version = new Version(values[0], values[1], values[2], values[3]);
                    break;
                }
                else if (s.Contains("XYSubSample"))
                {
                    J105Parameters.XYSubSample = ValueFromLine(s);
                    break;
                }
                else if (s.Contains("IntensityNormalization"))
                {
                    J105Parameters.IntensityNormalization = ValueFromLine(s);
                    break;
                }
            }
        }
        private void ReadMasses()
        {
            _massCalibration = new MassCalibration();

            ZipArchiveEntry entryMass = _zipArchive.GetEntry(StreamFilePaths.MTMass);
            ZipArchiveEntry entryTime = _zipArchive.GetEntry(StreamFilePaths.MTTime);

            long mc = entryMass.Length / 4L;
            long tc = entryTime.Length / 4L;
            if (mc != tc) throw new ArgumentException(string.Format("Mass ({0}) and time ({1}) lengths do not match", mc, tc));

            long num = mc;

            using (BinaryReader brMass = new BinaryReader(entryMass.Open()))
            {
                using (BinaryReader brTime = new BinaryReader(entryTime.Open()))
                {
                    for (int index = 0; (long)index < num; index++)
                    {
                        float mass = brMass.ReadSingle();
                        float time = brTime.ReadSingle();
                        _massCalibration.Add(new MassCalibrationValue(index, (float)mass, (float)time));
                    }
                }
            }
        }

        public void UpdateImageDetails(int pixelsX, int pixelsY)
        {
            ZipArchiveEntry entry = _zipArchive.GetEntry(StreamFilePaths.ImageDetails);
            List<string> lines = new List<string>();

            //J105Parameters.StageScanDimensionX = 1;
            //J105Parameters.StageScanDimensionY = 1;
            //J105Parameters.RasterScanDimensionX = pixelsX;
            //J105Parameters.RasterScanDimensionY = pixelsY;
            //J105Parameters.WidthPixels = pixelsX;
            //J105Parameters.HeightPixels = pixelsY;

            using (StreamWriter sw = new StreamWriter(entry.Open()))
            {
                sw.WriteLine("DepthLo=" + J105Parameters.DepthLo);
                sw.WriteLine("DepthHi=" + J105Parameters.DepthHi);
                sw.WriteLine("StageScanDimensionX=1");
                sw.WriteLine("StageScanDimensionY=1");
                sw.WriteLine("RasterScanDimensionX=" + pixelsX);
                sw.WriteLine("RasterScanDimensionY=" + pixelsY);
                sw.WriteLine("WidthInMicrons=" + J105Parameters.WidthMicrons);
                sw.WriteLine("HeightInMicrons=" + J105Parameters.HeightMicrons);
                sw.WriteLine("SpectrumLength=" + J105Parameters.SpectrumLength);
                sw.WriteLine("WidthInPixels=" + pixelsX);
                sw.WriteLine("HeightInPixels=" + pixelsY);
            }

            ReadImageDetails();
        }
        public void ClearSpectrum(int Z, int TY, int TX, int RY, int RX)
        {
            ZipArchiveEntry spectrum = _zipArchive.GetEntry(StreamFilePaths.PixelPath(Z, TY, TX, RY, RX));
            if (spectrum != null) spectrum.Delete();
        }

        public uint[] GetTotalSpectrum()
        {
            uint[] returnValues = new uint[SpectrumLength];

            int zCount = J105Parameters.NumberLayers;
            int yTiles = J105Parameters.StageScanDimensionY;
            int xTiles = J105Parameters.StageScanDimensionX;
            int yPixels = J105Parameters.RasterScanDimensionY;
            int xPixels = J105Parameters.RasterScanDimensionX;

            for (int z = 0; z < zCount; z++)
            {
                for (int a = 0; a < xTiles; a++)
                {
                    for (int b = 0; b < yTiles; b++)
                    {
                        Parallel.For(0, xPixels,
                            x =>
                            {
                                Parallel.For(0, yPixels,
                                    y =>
                                    {
                                        uint[] values = GetValues(z, b, a, y, x);
                                        if (values == null) return;

                                        for (int i = 0; i < values.Length; i++)
                                        {
                                            returnValues[i] += values[i];
                                        }
                                    });
                            });
                    }
                }
            }
            return returnValues;
        }
        public uint[] GetTotalSpectrum(BackgroundWorker bw)
        {
            uint[] returnValues = new uint[SpectrumLength];

            int zCount = J105Parameters.NumberLayers;
            int yTiles = J105Parameters.StageScanDimensionY;
            int xTiles = J105Parameters.StageScanDimensionX;
            int yPixels = J105Parameters.RasterScanDimensionY;
            int xPixels = J105Parameters.RasterScanDimensionX;

            long total = zCount * yTiles * xTiles * yPixels * xPixels;
            long pos = 0;

            for (int z = 0; z < zCount; z++)
            {
                for (int a = 0; a < xTiles; a++)
                {
                    for (int b = 0; b < yTiles; b++)
                    {
                        Parallel.For(0, xPixels,
                            x =>
                            {
                                Parallel.For(0, yPixels,
                                    y =>
                                    {
                                        uint[] values = GetValues(z, b, a, y, x);
                                        if (values == null) return;

                                        for (int i = 0; i < values.Length; i++)
                                        {
                                            returnValues[i] += values[i];
                                        }
                                        Interlocked.Increment(ref pos);
                                        if (bw != null) bw.ReportProgress(Percentage.GetPercent(pos, total));
                                    });
                            });
                        
                    }
                }
            }
            return returnValues;
        }

        public int IntensityFromMassRange(int Z, int TY, int TX, int RY, int RX,
            float StartMass, float EndMass)
        {
            return IntensityFromMassRange(StartMass, EndMass, GetValues(Z, TY, TX, RY, RX));
        }
        private int IntensityFromMassRange(float startMass, float endMass, uint[] values)
        {
            if (values == null) return 0;

            MassRange indexRange = _massCalibration.GetIndexRange(startMass, endMass);

            uint intensity = 0;
            for (int i = indexRange.StartIndex; i <= indexRange.EndIndex; i++)
            {
                intensity += values[i];
            }
            if (intensity >= int.MaxValue) return int.MaxValue;
            else return (int)intensity;
        }

        public uint[] GetValues(int Z, int TY, int TX, int RY, int RX)
        {
            string entryName = StreamFilePaths.PixelPath(Z, TY, TX, RY, RX);

            uint[] returnArray;
            byte[] buffer;

            Monitor.Enter(_zipArchive);
            try
            {
                ZipArchiveEntry entry = _zipArchive.GetEntry(entryName);

                using (Stream stream = entry.Open())
                {
                    buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                }
            }
            catch (NullReferenceException)
            {
                return null;
            }
            finally { Monitor.Exit(_zipArchive); }

            //if (buffer == null) return null;

            int count = 0;
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                BinaryReader7Bit br = new BinaryReader7Bit(ms);
                count = br.Read7BitEncodedInt();

                returnArray = new uint[SpectrumLength];
                for (int i = 0; i < count; i++)
                {
                    int index = br.Read7BitEncodedInt();
                    int value = br.Read7BitEncodedInt();

                    returnArray[Convert.ToUInt32(index)] = Convert.ToUInt32(value);
                }
            }
            return returnArray;
        }
        public uint[] GetValues(int Z, int TY, int TX, int RY, int RX, out byte[] Buffer)
        {
            string entryName = StreamFilePaths.PixelPath(Z, TY, TX, RY, RX);

            uint[] returnArray;
            byte[] buffer;

            Monitor.Enter(_zipArchive);
            try
            {
                ZipArchiveEntry entry = _zipArchive.GetEntry(entryName);

                using (Stream stream = entry.Open())
                {
                    buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                }
            }
            finally { Monitor.Exit(_zipArchive); }

            Buffer = buffer;
            if (buffer == null) return null;

            int count = 0;
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                BinaryReader7Bit br = new BinaryReader7Bit(ms);
                count = br.Read7BitEncodedInt();

                returnArray = new uint[SpectrumLength];
                for (int i = 0; i < count; i++)
                {
                    int index = br.Read7BitEncodedInt();
                    int value = br.Read7BitEncodedInt();

                    returnArray[Convert.ToUInt32(index)] = Convert.ToUInt32(value);
                }
            }
            return returnArray;
        }

        public void WriteValues(int Z, int TY, int TX, int RY, int RX, uint[] Array)
        {
            string entryName = StreamFilePaths.PixelPath(Z, TY, TX, RY, RX);

            //Get length of Array. If Array is null, then length is 0 so
            //a blank file with array length 0 will be written
            int length = 0;
            if (Array != null) length = Array.Length;

            //Get count of indicies with nonzero intensity
            int count = 0;
            for (int i = 0; i < length; i++)
            {
                if (Array[i] != 0) count++;
            }

            //Create buffer for output
            //byte[] buffer = new byte[sizeof(int) * (2 * count + 1)];
            using (MemoryStream ms = new MemoryStream(4 * (1 + 2 * count)))
            {
                BinaryWriter7Bit bw = new BinaryWriter7Bit(ms);

                //Write number of nonzero index/value pairs
                bw.Write7BitInteger(count);

                for (int i = 0; i < length; i++)
                {
                    //Write each index/value pair if intensity is nonzero
                    if (Array[i] > 0)
                    {
                        bw.Write7BitInteger(i);
                        bw.Write7BitInteger(Convert.ToInt32(Array[i]));
                    }
                }

                //Lock ZipArchive
                Monitor.Enter(_zipArchive);
                try
                {
                    //If entry exits, delete first
                    ZipArchiveEntry currentEntry = _zipArchive.GetEntry(entryName);
                    if (currentEntry != null) currentEntry.Delete();

                    //Create new entry
                    ZipArchiveEntry entry = _zipArchive.CreateEntry(entryName);

                    //Write buffer
                    byte[] buffer = ms.ToArray();
                    using (Stream stream = entry.Open())
                    {
                        stream.Write(buffer, 0, buffer.Length);
                    }
                }
                //Unlock ZipArchive
                finally { Monitor.Exit(_zipArchive); }
            }
        }

        private int ValueFromLine(string line)
        {
            char[] delim = new char[1] { '=' };
            string[] parts = line.Split(delim);

            int value;

            if (int.TryParse(parts[1], out value)) return value;

            return -1;
        }

        public void SaveQuickLoad()
        {
            uint[] intensities = GetTotalSpectrum();

            if (intensities == null || intensities.Length == 0)
            {
                throw new J105StreamException("Intensities data not valid.");
            }
            if (_massCalibration == null || _massCalibration.Count == 0)
            {
                throw new J105StreamException("Mass values not valid.");
            }
            if (intensities.Length != _massCalibration.Count)
            {
                throw new J105StreamException("Data mismatch between intensities length and mass values length.");
            }

            //If QuickLoad already exists, delete file before creating new entry.
            ZipArchiveEntry currentEntry = _zipArchive.GetEntry(StreamFilePaths.QuickLoadFile);
            if (currentEntry != null)
            {
                currentEntry.Delete();
            }
            ZipArchiveEntry entry = _zipArchive.CreateEntry(StreamFilePaths.QuickLoadFile);

            using (Stream stream = entry.Open())
            {
                BinaryWriter bw = new BinaryWriter(stream);

                bw.Write(J105Parameters.DepthLo);
                bw.Write(J105Parameters.DepthHi);
                bw.Write(J105Parameters.StageScanDimensionX);
                bw.Write(J105Parameters.StageScanDimensionY);
                bw.Write(J105Parameters.RasterScanDimensionX);
                bw.Write(J105Parameters.RasterScanDimensionY);
                bw.Write(J105Parameters.WidthMicrons);
                bw.Write(J105Parameters.HeightMicrons);
                bw.Write(J105Parameters.SpectrumLength);

                bw.Write(J105Parameters.Version.Major);
                bw.Write(J105Parameters.Version.Minor);
                bw.Write(J105Parameters.Version.Build);
                bw.Write(J105Parameters.Version.Revision);
                bw.Write(J105Parameters.XYSubSample);
                bw.Write(J105Parameters.IntensityNormalization);

                for (int i = 0; i < _massCalibration.Count; i++)
                {
                    bw.Write(_massCalibration[i].Index);
                    bw.Write(_massCalibration[i].Mass);
                    bw.Write(_massCalibration[i].Time);
                    bw.Write(intensities[i]);
                }
            }
        }

        public void SaveQuickLoad(uint[] Intensities)
        {
            if (Intensities == null || Intensities.Length == 0)
            {
                throw new J105StreamException("Intensities data not valid.");
            }
            if (_massCalibration == null || _massCalibration.Count == 0)
            {
                throw new J105StreamException("Mass values not valid.");
            }
            if (Intensities.Length != _massCalibration.Count)
            {
                throw new J105StreamException("Data mismatch between intensities length and mass values length.");
            }

            //If QuickLoad already exists, delete file before creating new entry.
            ZipArchiveEntry currentEntry = _zipArchive.GetEntry(StreamFilePaths.QuickLoadFile);
            if (currentEntry != null)
            {
                currentEntry.Delete();
            }
            ZipArchiveEntry entry = _zipArchive.CreateEntry(StreamFilePaths.QuickLoadFile);

            using (Stream stream = entry.Open())
            {
                BinaryWriter bw = new BinaryWriter(stream);

                bw.Write(J105Parameters.DepthLo);
                bw.Write(J105Parameters.DepthHi);
                bw.Write(J105Parameters.StageScanDimensionX);
                bw.Write(J105Parameters.StageScanDimensionY);
                bw.Write(J105Parameters.RasterScanDimensionX);
                bw.Write(J105Parameters.RasterScanDimensionY);
                bw.Write(J105Parameters.WidthMicrons);
                bw.Write(J105Parameters.HeightMicrons);
                bw.Write(J105Parameters.SpectrumLength);

                bw.Write(J105Parameters.Version.Major);
                bw.Write(J105Parameters.Version.Minor);
                bw.Write(J105Parameters.Version.Build);
                bw.Write(J105Parameters.Version.Revision);
                bw.Write(J105Parameters.XYSubSample);
                bw.Write(J105Parameters.IntensityNormalization);

                for (int i = 0; i < _massCalibration.Count; i++)
                {
                    bw.Write(_massCalibration[i].Index);
                    bw.Write(_massCalibration[i].Mass);
                    bw.Write(_massCalibration[i].Time);
                    bw.Write(Intensities[i]);
                }
            } 
        }
        public uint[] LoadFromQuickLoad()
        {
            uint[] intensities;
            float[] masses;

            ZipArchiveEntry entry = _zipArchive.GetEntry(StreamFilePaths.QuickLoadFile);

            if (entry == null) throw new J105StreamException("No quickload file was found.");

            using (Stream stream = entry.Open())
            {
                BinaryReader br = new BinaryReader(stream);

                J105DataParameters j = new J105DataParameters()
                {
                    DepthLo = br.ReadInt32(),
                    DepthHi = br.ReadInt32(),
                    StageScanDimensionX = br.ReadInt32(),
                    StageScanDimensionY = br.ReadInt32(),
                    RasterScanDimensionX = br.ReadInt32(),
                    RasterScanDimensionY = br.ReadInt32(),
                    WidthMicrons = br.ReadInt32(),
                    HeightMicrons = br.ReadInt32(),
                    SpectrumLength = br.ReadInt32(),

                    Version = new Version(br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32()),
                    XYSubSample = br.ReadInt32(),
                    IntensityNormalization = br.ReadInt32()
                };

                J105Parameters = j;

                intensities = new uint[SpectrumLength];
                masses = new float[SpectrumLength];

                _massCalibration = new MassCalibration();

                for (int i = 0; i < SpectrumLength; i++)
                {
                    int index = br.ReadInt32();
                    float mass = br.ReadSingle();
                    float time = br.ReadSingle();
                    uint intensity = br.ReadUInt32();

                    masses[i] = mass;
                    intensities[i] = intensity;

                    _massCalibration.Add(new MassCalibrationValue(index, mass, time));
                }
            }

            return intensities;
        }
    }
   
    internal class MassCalibration : List<MassCalibrationValue>
    {
        public MassCalibration()
            :base()
        {

        }

        public MassCalibration(int size)
            :base(size)
        {

        }

        public MassRange GetIndexRange(float StartMass, float EndMass)
        {
            MassRange mr = new MassRange();
            for (int i = 0; i < this.Count; i++)
            {
                if (i - 1 <= 0) continue;
                if (this[i].Mass >= StartMass && this[i - 1].Mass < StartMass)
                {
                    mr.StartIndex = this[i].Index;
                }

                if (i + 1 >= this.Count) continue;
                if(this[i].Mass <= EndMass && this[i+1].Mass > EndMass)
                {
                    mr.EndIndex = this[i].Index;
                    break;
                }
            }


            if (mr.StartIndex == -1)
            {
                if (StartMass == this[0].Mass) mr.StartIndex = this[0].Index;
            }
            if (mr.EndIndex == -1)
            {
                if (EndMass == this[this.Count - 1].Mass) mr.EndIndex = this[this.Count - 1].Index;
            }
            if (mr.StartIndex == -1 || mr.EndIndex == -1) 
                throw new ArgumentException("Could not determine the mass range.");

            return mr;
        }
    }
    
    internal struct MassCalibrationValue
    {
        int _index;
        float _mass;
        float _time;

        public int Index
        {
            get { return _index; }
        }
        public float Mass
        {
            get { return _mass; }
        }
        public float Time
        {
            get { return _time; }
        }
        public float C
        {
            get { return _mass / (_time * _time); }
        }

        public MassCalibrationValue(int Index, float Mass, float Time)
        {
            _index = Index;
            _mass = Mass;
            _time = Time;
        }

        public override string ToString()
        {
            return string.Format("Index: {0} Mass: {1} Time: {2}", _index, _mass, _time);
        }
    }

    internal class MassLookupHelper
    {
        float[] _massValues;
        Dictionary<int, int> _lookup;

        public float[] MassValues { get { return _massValues; } }
        public int this[int mass]
        {
            get { return _lookup[mass]; }
        }

        public MassLookupHelper(float[] MassValues)
        {
            _massValues = MassValues;
            _lookup = new Dictionary<int, int>();

            int counter = 0;
            for (int i = 0; i < _massValues.Length; i++)
            {
                float value = _massValues[i];

                if (value > counter * 10)
                {
                    _lookup.Add(counter, i);
                    counter++;
                }
            }
        }
        public MassRange GetIndexRange(float StartMass, float EndMass)
        {
            MassRange determinedRange = new MassRange();

            int[] keys = _lookup.Keys.ToArray<int>();

            int lookupStartIndex = -1;

            for (int i = 0; i < keys.Length; i++)
            {
                if (i >= keys.Length - 1) continue;

                if (_massValues[_lookup[i]] <= StartMass &&
                    _massValues[_lookup[i + 1]] > StartMass)
                {
                    lookupStartIndex = i;

                    for (int j = lookupStartIndex; j < _massValues.Length; j++)
                    {
                        if (_massValues[j] <= StartMass &&
                            _massValues[j + 1] > StartMass)
                        {
                            determinedRange.StartIndex = j;
                            break;
                        }
                    }

                    break;
                }
            }

            if (lookupStartIndex < 0) throw new ArgumentException("Could not find mass range in lookup table.");

            int lookupEndIndex = -1;

            for (int i = lookupStartIndex; i < _lookup.Values.Count; i++)
            {
                if (i == 0) continue;

                if (_massValues[_lookup[i]] >= EndMass &&
                    _massValues[_lookup[i - 1]] < EndMass)
                {
                    lookupEndIndex = i;

                    for (int j = lookupEndIndex; j < _massValues.Length; j++)
                    {
                        if (_massValues[j] >= EndMass && 
                            _massValues[j - 1] < EndMass)
                        {
                            determinedRange.EndIndex = j;
                            break;
                        }
                    }

                    break;
                }
            }

            return determinedRange;
        }
    }

    internal static class StreamFilePaths
    {
        internal static string DetailsFile { get { return @"details.txt"; } }
        internal static string MTMass { get { return @"MassTime\mass"; } }
        internal static string MTTime { get { return @"MassTime\time"; } }
        internal static string MCMass { get { return @"MassCalibration\mass"; } }
        internal static string MCTime { get { return @"MassCalibration\time"; } }
        internal static string ImageDetails { get { return @"ExperimentDetails\ImageDetails.txt"; } }

        internal static string PixelPath(int z, int ty, int tx, int ry, int rx)
        {
            return string.Format(@"Spectr\Z{0}\TY{1}\TX{2}\RY{3}\RX{4}", (z + 1).ToString("D6"), ty.ToString("D6"), tx.ToString("D6"),
                ry.ToString("D6"), rx.ToString("D6"));
        }

        internal static string QuickLoadFile { get { return Path.Combine(@"IS3\QuickLoad\ql.is3"); } }
    }

    internal class BinaryReader7Bit : BinaryReader
    {
        public BinaryReader7Bit(Stream input)
            : base(input)
        {
        }
        public BinaryReader7Bit(Stream input, Encoding encoding)
            : base(input, encoding)
        {
        }
        public BinaryReader7Bit(Stream input, Encoding encoding, bool leaveOpen)
            : base(input, encoding, leaveOpen)
        {
        }

        public new int Read7BitEncodedInt()
        {
            return base.Read7BitEncodedInt();
        }
    }
    internal class BinaryWriter7Bit : BinaryWriter
    {
        public BinaryWriter7Bit(Stream input)
            : base(input)
        {

        }
        public BinaryWriter7Bit(Stream input, Encoding encoding)
            : base(input, encoding)
        {
        }
        public BinaryWriter7Bit(Stream input, Encoding encoding, bool leaveOpen)
            : base(input, encoding, leaveOpen)
        {
        }

        public void Write7BitInteger(int integer)
        {
            base.Write7BitEncodedInt(integer);
        }
    }

    public class J105DataParameters
    {
        Version _version;
        int _xySubSample;
        int _intensityNormalization;
        int _depthLo;
        int _depthHi;
        int _stageScanDimensionX;
        int _stageScanDimensionY;
        int _rasterScanDimensionX;
        int _rasterScanDimensionY;
        int _widthMicrons;
        int _heightMicrons;
        int _spectrumLength;
        int _numLayers;
        int _widthPixels;
        int _heightPixels;

        public Version Version
        {
            get { return _version; }
            set { _version = value; }
        }
        public int XYSubSample
        {
            get { return _xySubSample; }
            set { _xySubSample = value; }
        }
        public int IntensityNormalization
        {
            get { return _intensityNormalization; }
            set { _intensityNormalization = value; }
        }
        public int DepthLo
        {
            get { return _depthLo; }
            set { _depthLo = value; }
        }
        public int DepthHi
        {
            get { return _depthHi; }
            set { _depthHi = value; }
        }
        public int StageScanDimensionX
        {
            get { return _stageScanDimensionX; }
            set { _stageScanDimensionX = value; }
        }
        public int StageScanDimensionY
        {
            get { return _stageScanDimensionY; }
            set { _stageScanDimensionY = value; }
        }
        public int RasterScanDimensionX
        {
            get { return _rasterScanDimensionX; }
            set { _rasterScanDimensionX = value; }
        }
        public int RasterScanDimensionY
        {
            get { return _rasterScanDimensionY; }
            set { _rasterScanDimensionY = value; }
        }
        public int WidthMicrons
        {
            get { return _widthMicrons; }
            set { _widthMicrons = value; }
        }
        public int HeightMicrons
        {
            get { return _heightMicrons; }
            set { _heightMicrons = value; }
        }
        public int SpectrumLength
        {
            get { return _spectrumLength; }
            set { _spectrumLength = value; }
        }
        public int NumberLayers
        {
            //get { return _numLayers; }
            get { return _depthHi; }
            set { _numLayers = value; }
        }
        public int WidthPixels
        {
            get { return _widthPixels; }
            set { _widthPixels = value; }
        }
        public int HeightPixels
        {
            get { return _heightPixels; }
            set { _heightPixels = value; }
        }

        public J105DataParameters()
        {
            _numLayers = 1;
        }
    }
    
    public class MassRangePair : IComparable
    {
        public double StartMass;
        public double EndMass;

        public MassRangePair()
        {

        }
        public MassRangePair(double startMass, double endMass)
        {
            this.StartMass = startMass;
            this.EndMass = endMass;
        }
        public override string ToString()
        {
            return StartMass.ToString("0.000") + "-" + EndMass.ToString("0.000");
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj.GetType() != typeof(MassRangePair))
                throw new ArgumentException("Cannot compare different types.");

            MassRangePair m = (MassRangePair)obj;

            if (this.StartMass < m.StartMass) return -1;
            else if (this.StartMass == m.StartMass) return 0;
            else return 1;
        }

        public static List<MassRangePair> ParseString(string customRange)
        {
            List<MassRangePair> ranges = new List<MassRangePair>();

            // If empty string, return blank collection
            if (String.IsNullOrEmpty(customRange))
                return ranges;

            // Remove any spaces that may have been inadvertently entered
            string toParse = customRange;
            int index = toParse.IndexOf(' ');
            while (index != -1)
            {
                toParse = toParse.Remove(index, 1);
                index = toParse.IndexOf(' ');
            }

            string[] rangeString = toParse.Split(';');

            if (rangeString.Length == 0)
            {
                throw new ArgumentException("No string found.");
            }

            foreach (string s in rangeString)
            {
                if (!s.Contains("-"))
                {
                    throw new ArgumentException($"Specified range ({s}) does not contain a valid separtor character (-)");
                }

                string[] rangeValues = s.Split('-');

                if (rangeValues.Length != 2)
                    throw new ArgumentException("Invalid number of masses in the specified range.");

                MassRangePair range = new MassRangePair();
                try
                {
                    range.StartMass = double.Parse(rangeValues[0]);
                }
                catch(Exception ex)
                {
                    throw new ArgumentException($"Could not parse the specified start mass ({rangeValues[0]}).", ex);
                }
                try
                {
                    range.EndMass = double.Parse(rangeValues[1]);
                }
                catch(Exception ex)
                {
                    throw new ArgumentException($"Could not parse the specified end mass ({rangeValues[1]}).", ex);
                }

                ranges.Add(range);
            }

            return ranges;
        }
        public static string CreateString(List<MassRangePair> ranges)
        {
            StringBuilder sb = new StringBuilder();

            foreach(MassRangePair range in ranges)
            {
                sb.AppendFormat("{0}-{1};", 
                    range.StartMass.ToString("0.000"), range.EndMass.ToString("0.000"));
            }

            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
    }
    internal class MassRange
    {
        int _startIndex;
        int _endIndex;

        public MassRange()
        {
            _startIndex = -1;
            _endIndex = -1;
        }
        public MassRange(int StartIndex, int EndIndex)
        {
            _startIndex = StartIndex;
            _endIndex = EndIndex;
        }

        public bool IsInRange(uint Index)
        {
            if (_startIndex < 0 || _endIndex < 0)
            {
                throw new ArgumentException("Invalid mass range.");
            }

            return (Index >= _startIndex && Index <= _endIndex);
        }

        public int StartIndex 
        { 
            get 
            { 
                return _startIndex; 
            }
            set
            {
                _startIndex = value;
            }
        }
        public int EndIndex 
        { 
            get 
            { 
                return _endIndex; 
            }
            set
            {
                _endIndex = value;
            }
        }
    }

    public class J105StreamException : Exception
    {
        public J105StreamException()
            : base("Unspecified J105Stream exception")
        {
        }
        public J105StreamException(string Message)
            : base(Message)
        {
        }
        public J105StreamException(string Message, Exception InnerException)
            : base(Message, InnerException)
        {
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

            return species;
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
}