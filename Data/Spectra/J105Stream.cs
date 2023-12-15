using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImagingSIMS.Common.Math;

namespace ImagingSIMS.Data.Spectra
{
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

                ReadMasses();
                ParseImageDimensions();
                ReadImageDetails();
                ReadDetails();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed to open J105 stream. " + ex.Message + "\n" + ex.StackTrace);
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
        private void ParseImageDimensions()
        {
            var entries = _zipArchive.Entries.Where(e => e.FullName.Contains("Spectr"));
            var depths = entries.Select(e => e.FullName.Split('\\').ToList()).Select(p => p[1]).Distinct().Select(d => int.Parse(d.Replace("Z", "")));
            J105Parameters.DepthLo = depths.Min();
            J105Parameters.DepthHi = depths.Max();
            var tilesY = entries.Select(e => e.FullName.Split('\\').ToList()).Select(p => p[2]).Distinct().Select(d => int.Parse(d.Replace("TY", "")));
            J105Parameters.StageScanDimensionY = tilesY.Max() + 1;
            var tilesX = entries.Select(e => e.FullName.Split('\\').ToList()).Select(p => p[3]).Distinct().Select(d => int.Parse(d.Replace("TX", "")));
            J105Parameters.StageScanDimensionX = tilesX.Max() + 1;
            var rasterY = entries.Select(e => e.FullName.Split('\\').ToList()).Select(p => p[4]).Distinct().Select(d => int.Parse(d.Replace("RY", "")));
            J105Parameters.RasterScanDimensionY = rasterY.Max() + 1;
            var rasterX = entries.Select(e => e.FullName.Split('\\').ToList()).Select(p => p[5]).Distinct().Select(d => int.Parse(d.Replace("RX", "")));
            J105Parameters.RasterScanDimensionX = rasterX.Max() + 1;
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
                // This parses the V2 version from newer files
                if (s.Contains("ExperimentDetailsVersion"))
                {
                    if (s.Contains("."))
                    {
                        J105Parameters.Version = Version.Parse(s);
                    }
                    else
                    {
                        var v = ValueFromLine(s);
                        J105Parameters.Version = new Version(v, 0, 0, 0);
                    }
                }
                //if (s.Contains("DepthLo"))
                //{
                //    J105Parameters.DepthLo = ValueFromLine(s);
                //    continue;
                //}
                //if (s.Contains("DepthHi"))
                //{
                //    J105Parameters.DepthHi = ValueFromLine(s);
                //    continue;
                //}
                //if (s.Contains("StageScanDimensionX"))
                //{
                //    J105Parameters.StageScanDimensionX = ValueFromLine(s);
                //    continue;
                //}
                //if (s.Contains("StageScanDimensionY"))
                //{
                //    J105Parameters.StageScanDimensionY = ValueFromLine(s);
                //    continue;
                //}
                //if (s.Contains("RasterScanDimensionX"))
                //{
                //    J105Parameters.RasterScanDimensionX = ValueFromLine(s);
                //    continue;
                //}
                //if (s.Contains("RasterScanDimensionY"))
                //{
                //    J105Parameters.RasterScanDimensionY = ValueFromLine(s);
                //    continue;
                //}
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
                //if (s.Contains("SpectrumLength"))
                //{
                //    J105Parameters.SpectrumLength = ValueFromLine(s);
                //    continue;
                //}
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
                // This parses the V2 version info from older files
                if (s.Contains("Versin"))
                {
                    char[] delim = new char[1] { '=' };
                    J105Parameters.Version = Version.Parse(s.Split(delim)[1]);
                    break;
                }
            }
        }
        private void ReadMasses()
        {
            _massCalibration = new MassCalibration();

            ZipArchiveEntry entryMass = _zipArchive.GetEntry(StreamFilePaths.MTMass);
            ZipArchiveEntry entryTime = _zipArchive.GetEntry(StreamFilePaths.MTTime);

            int mc = (int)(entryMass.Length / 4L);
            int tc = (int)(entryTime.Length / 4L);
            if (mc != tc) throw new ArgumentException(string.Format("Mass ({0}) and time ({1}) lengths do not match", mc, tc));

            J105Parameters.SpectrumLength = (int)mc;

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

            using (StreamWriter sw = new StreamWriter(entry.Open()))
            {
                sw.WriteLine("DepthLo=" + J105Parameters.DepthLo);
                sw.WriteLine("DepthHi=" + J105Parameters.DepthHi);
                //sw.WriteLine("StageScanDimensionX=1");
                //sw.WriteLine("StageScanDimensionY=1");
                //sw.WriteLine("RasterScanDimensionX=" + pixelsX);
                //sw.WriteLine("RasterScanDimensionY=" + pixelsY);
                sw.WriteLine("WidthInMicrons=" + J105Parameters.WidthMicrons);
                sw.WriteLine("HeightInMicrons=" + J105Parameters.HeightMicrons);
                //sw.WriteLine("SpectrumLength=" + J105Parameters.SpectrumLength);
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

            IndexRange indexRange = _massCalibration.GetIndexRange(startMass, endMass);

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
            : base()
        {

        }

        public MassCalibration(int size)
            : base(size)
        {

        }

        public IndexRange GetIndexRange(float StartMass, float EndMass)
        {
            IndexRange mr = new IndexRange();
            for (int i = 0; i < this.Count; i++)
            {
                if (i - 1 <= 0) continue;
                if (this[i].Mass >= StartMass && this[i - 1].Mass < StartMass)
                {
                    mr.StartIndex = this[i].Index;
                }

                if (i + 1 >= this.Count) continue;
                if (this[i].Mass <= EndMass && this[i + 1].Mass > EndMass)
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
        public IndexRange GetIndexRange(float StartMass, float EndMass)
        {
            IndexRange determinedRange = new IndexRange();

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
}
