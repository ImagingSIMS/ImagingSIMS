using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using ImagingSIMS.Data.Spectra.CamecaHeaders;

namespace ImagingSIMS.Data.Spectra
{
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
                if (_isDeadTimeCorrected != value)
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
            if (parameters.Count > 1)
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

                CamecaNanoSIMSTabMassCollection masses = new CamecaNanoSIMSTabMassCollection();

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
                && x.Masses.Equals(y.Masses) && x.AnalysisType == y.AnalysisType;

            if (!equateSizeZ) return isEqual;

            return isEqual && x.SizeZ == y.SizeZ;
        }
    }
}
