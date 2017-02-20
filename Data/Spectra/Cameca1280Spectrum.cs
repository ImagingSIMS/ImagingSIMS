using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ImagingSIMS.Data.Spectra
{
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

                if (!int.TryParse(nodeSize.InnerText, out size))
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
                foreach (XmlNode node in nodeSpeciesList)
                {
                    try
                    {
                        speciesInFile.Add(CamecaSpecies.FromXmlNode(node));
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"Could not parse a species from file {Path.GetFileName(filePaths[i])}.", ex);
                    }
                }

                int numCycles = 0;
                foreach (CamecaSpecies s in speciesInFile)
                {
                    if (numCycles == 0) numCycles = s.Cycles;

                    if (s.Cycles != numCycles)
                        throw new ArgumentException("Invalid species. The number of cycles do not match in the image.");
                }

                numCyclesForFile[i] = numCycles;

                if (_species.Count == 0)
                {
                    _species.AddRange(speciesInFile);
                }
                else
                {
                    if (_species.Count != speciesInFile.Count)
                        throw new ArgumentException("Invalid spectrum. Number of species do not match across files.");

                    CamecaSpeciesIdentityComparer identityComparer = new CamecaSpeciesIdentityComparer();

                    foreach (CamecaSpecies s in speciesInFile)
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
}
