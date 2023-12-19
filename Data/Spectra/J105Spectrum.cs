using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ImagingSIMS.Common.Math;

namespace ImagingSIMS.Data.Spectra
{
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

        public override Data2D FromMassRange(MassRange MassRange, BackgroundWorker bw = null)
        {
            Data2D dt = new Data2D(_sizeX, _sizeY);

            double totalSteps = _sizeX;

            List<Data2D> dts = FromMassRange(MassRange, "preview", true, bw);

            if (bw != null && bw.CancellationPending) return dt;

            bw?.ReportProgress(0);

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
                bw?.ReportProgress(Percentage.GetPercent(x, totalSteps));
            }
            dt.DataName = string.Format("{0} {1}-{2}", Name, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"));
            return dt;
        }
        public override Data2D FromMassRange(MassRange MassRange, int Layer, BackgroundWorker bw = null)
        {
            if (!_stream.IsStreamOpen) throw new ArgumentException("No V2 stream has been initialized.");
            if (Layer >= SizeZ)
                throw new ArgumentException(string.Format("Layer {0} does not exist in the spectrum (Number layers: {1}).", Layer, SizeZ));

            int ct = 0;
            double totalSteps = _tilesX * _pixelsX;

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
                            Parallel.For(0, _pixelsY,
                                y =>
                                {
                                    int yIndex = (ty * _pixelsY) + y;
                                    // int yIndex = ((_tilesY - ty - 1) * _pixelsY) + y;

                                    dt[xIndex, yIndex] = _stream.IntensityFromMassRange(Layer, ty, tx, y, x, (float)MassRange.StartMass, (float)MassRange.EndMass);
                                }
                            );
                            ct++;
                            if (bw != null) bw.ReportProgress(Percentage.GetPercent(ct, totalSteps));
                        });
                    if (bw != null && bw.CancellationPending) return null;
                }
            }

            return dt;
        }
        public override Data2D FromMassRange(MassRange MassRange, out float Max, BackgroundWorker bw = null)
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
                if (bw != null) bw.ReportProgress(Percentage.GetPercent(x, totalSteps));
            }
            dt.DataName = string.Format("{0} {1}-{2}", Name, MassRange.StartMass.ToString("0.00"), MassRange.EndMass.ToString("0.00"));
            Max = max;
            return dt;
        }
        public override List<Data2D> FromMassRange(MassRange MassRange, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null)
        {
            if (!_stream.IsStreamOpen) throw new ArgumentException("No V2 stream has been initialized.");

            int ct = 0;
            double totalSteps = _sizeZ * _tilesX * _tilesY * _pixelsX;

            if (bw != null) bw.ReportProgress(0);

            List<Data2D> returnTables = new List<Data2D>();

            int i = 0;
            for (int z = 0; z < _sizeZ; z++)
            {
                if (bw != null && bw.CancellationPending) return returnTables;

                Data2D dt = new Data2D(_sizeX, _sizeY);

                for (int tx = 0; tx < _tilesX; tx++)
                {
                    for (int ty = 0; ty < _tilesY; ty++)
                    {
                        Parallel.For(0, _pixelsX, x =>
                        {
                            int xIndex = (tx * _pixelsX) + x;
                            Parallel.For(0, _pixelsY, y =>
                            {
                                // int yIndex = ((_tilesY - ty - 1) * _pixelsY) + y;
                                int yIndex = (ty * _pixelsY) + y;
                                dt[xIndex, yIndex] = _stream.IntensityFromMassRange(z, ty, tx, y, x, (float)MassRange.StartMass, (float)MassRange.EndMass);
                            });
                            ct++;
                            if (bw != null) bw.ReportProgress(Percentage.GetPercent(ct, totalSteps));
                            if (bw != null && bw.CancellationPending) return;
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
        public override Data2D FromMassRange(MassRange MassRange, int Layer, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null)
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
                            Parallel.For(0, _pixelsY,
                                y =>
                                {
                                    // int yIndex = ((_tilesY - ty - 1) * _pixelsY) + y;
                                    int yIndex = (ty * _pixelsY) + y;
                                    dt[xIndex, yIndex] = _stream.IntensityFromMassRange(Layer, ty, tx, y, x, (float)MassRange.StartMass, (float)MassRange.EndMass);
                                }
                            );
                            ct++;
                            if (bw != null) bw.ReportProgress(Percentage.GetPercent(ct, totalSteps));
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
}
