using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using ImagingSIMS.Direct3DRendering;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Converters;
using ImagingSIMS.Data.Spectra;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Device = SharpDX.Direct3D11.Device;
using ImagingSIMS.Common;
using ImagingSIMS.Data.Spectra;
using ImagingSIMS.Data.Imaging;

using Accord.Math.Decompositions;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Press Enter to begin...");
            Console.ReadLine();

            var specFileName = @"D:\Data\Al-Li Alloy\low-current-1@1.imp";
            var spec = new Cameca1280Spectrum("low-current-1@1");
            spec.LoadFromFile(specFileName, null);

            var species = spec.Species;
            var components = new Dictionary<CamecaSpecies, Data2D>();

            //foreach (var s in species)
            //{
            //    Data2D summed = new Data2D(spec.SizeX, spec.SizeY);

            //    for (int z = 0; z < spec.SizeZ; z++)
            //    {
            //        var layer = spec.FromSpecies(s, z, "layer", false, null);
            //        if(layer.Maximum != 50f)
            //        {
            //            summed += layer;
            //        }                    
            //    }

            //    using (Stream stream = File.OpenWrite($@"D:\Data\Al-Li Alloy\{s.Mass.ToString()}.dat"))
            //    {
            //        BinaryWriter bw = new BinaryWriter(stream);

            //        for (int x = 0; x < summed.Width; x++)
            //        {
            //            for (int y = 0; y < summed.Height; y++)
            //            {
            //                bw.Write((double)summed[x, y]);
            //            }
            //        }
            //    }
            //}

            var directoryName = Path.GetDirectoryName(specFileName);

            var summedSpecies = new List<Data2D>();
            foreach (var s in species)
            {
                var max = 0f;
                summedSpecies.Add(spec.FromSpecies(s, out max));                
            }

            double[,] data = new double[spec.SizeX * spec.SizeY, species.Count];
            for (int i = 0; i < species.Count; i++)
            {
                for (int x = 0; x < spec.SizeX; x++)
                {
                    for (int y = 0; y < spec.SizeY; y++)
                    {
                        data[x * spec.SizeY + y, i] = summedSpecies[i][x, y];
                    }
                }
            }

            var svd = new SingularValueDecomposition(data, true, true, false, false);
            var u = svd.LeftSingularVectors;        // ->Scores
            var sigma = svd.Diagonal;               // 
            var coeff = svd.RightSingularVectors;   // Loadings [m x m]

            var scores = new double[spec.SizeX * spec.SizeY, species.Count];
            for (int x = 0; x < spec.SizeX * spec.SizeY; x++)
            {
                for (int y = 0; y < species.Count; y++)
                {
                    scores[x, y] = u[x, y] * sigma[y];
                }
            }

            for (int i = 0; i < species.Count; i++)
            {
                var scorePlot = new Data2D(spec.SizeX, spec.SizeY);

                for (int x = 0; x < spec.SizeX; x++)
                {
                    for (int y = 0; y < spec.SizeY; y++)
                    {
                        scorePlot[x, y] = (float)scores[x * spec.SizeY + y, i];
                    }
                }

                scorePlot = normalizeScorePlot(scorePlot);

                var bsScore = ImageHelper.CreateColorScaleImage(scorePlot, ColorScaleTypes.RedBlackBlue);
                bsScore.Save(Path.Combine(directoryName, $"cs-{i + 1}.bmp"));
            }            

            Console.Write("Press Enter to exit.");
            Console.ReadLine();
        }

        private static Data2D normalizeScorePlot(Data2D matrix)
        {
            var norm = new Data2D(matrix.Width, matrix.Height);

            var max = getMaxValue(matrix);
            var min = getMinValue(matrix);

            if (max == 0) max = 1;
            if (min == 0) min = -1;

            for (int x = 0; x < matrix.Width; x++)
            {
                for (int y = 0; y < matrix.Height; y++)
                {
                    if (matrix[x, y] > 0) norm[x, y] = matrix[x, y] / max;
                    else if (matrix[x, y] < 0) norm[x, y] = -matrix[x, y] / min;
                }
            }

            return norm;
        }
        private static float getMinValue(Data2D matrix)
        {
            var min = float.MaxValue;

            for (int x = 0; x < matrix.Width; x++)
            {
                for (int y = 0; y < matrix.Height; y++)
                {
                    if (matrix[x, y] < min) min = matrix[x, y];
                }
            }

            return min;
        }
        private static float getMaxValue(Data2D matrix)
        {
            var max = float.MinValue;

            for (int x = 0; x < matrix.Width; x++)
            {
                for (int y = 0; y < matrix.Height; y++)
                {
                    if (matrix[x, y] > max) max = matrix[x, y];
                }
            }

            return max;
        }

        private static int readLine(string line, out int type)
        {
            string[] parts = line.Split(' ');

            if (parts == null || parts.Length == 0)
            {
                type = 0;
                return 0;
            }

            type = int.Parse(parts[0]);

            if (parts.Length == 1 || parts[1] == null) return 0;

            return int.Parse(parts[1]);
        }
    }

    //public class Cameca1280Spectrum
    //{
    //    internal class Species
    //    {
    //        internal int Cycles;
    //        internal int SizeInBytes;
    //        internal int PixelEncoding;
    //        internal double Mass;
    //        internal string Label;
    //        internal double WaitTime;
    //        internal double CountTime;
    //        internal double WellTime;
    //        internal double ExtraTime;
    //    }

    //    private static int getXmlIndex(byte[] buffer)
    //    {
    //        int i = 2;
    //        int length = buffer.Length;
    //        while (i < length)
    //        {
    //            switch (buffer[i])
    //            {
    //                case 120:
    //                    if (buffer[i - 1] == 63 && buffer[i - 2] == 60)
    //                    {
    //                        return i - 2;
    //                    }
    //                    i += 3;
    //                    continue;
    //                case 63:
    //                    i += 1;
    //                    continue;
    //                case 60:
    //                    i += 2;
    //                    continue;
    //                default:
    //                    i += 3;
    //                    continue;
    //            }
    //        }
    //        return -1;
    //    }
    //    public void LoadFromFile(string fileName)
    //    {
    //        int dataBufferSize = 0;
    //        int xmlBufferSize = 0;

    //        byte[] fullBuffer;
    //        byte[] dataBuffer;
    //        byte[] xmlBuffer;

    //        byte[] xmlStartSequence = new byte[] { 60, 63, 120 };

    //        using (Stream stream = File.OpenRead(fileName))
    //        {
    //            BinaryReader br = new BinaryReader(stream);

    //            fullBuffer = br.ReadBytes((int)stream.Length);

    //            // Get size of binary section at top of file
    //            //int startIndexOfXml = 0;
    //            //for (int i = 0; i < stream.Length - 2; i++)
    //            //{
    //            //    if (fullBuffer[i] == 60 && fullBuffer[i + 1] == 63 && fullBuffer[i + 2] == 120)
    //            //        startIndexOfXml = i;
    //            //}
    //            int startIndexOfXml = getXmlIndex(fullBuffer);
                


    //            // Split buffer into two parts: binary data and
    //            // XML experiment details sections


    //            // First 24 bytes are garbage, so skip those
    //            dataBufferSize = startIndexOfXml - 24;
    //            // Last byte of the file is a null byte so don't include it
    //            // with the XML details buffer
    //            xmlBufferSize = (int)stream.Length - startIndexOfXml - 1;

    //            dataBuffer = new byte[dataBufferSize];
    //            xmlBuffer = new byte[xmlBufferSize];

    //            Array.Copy(fullBuffer, 24, dataBuffer, 0, dataBufferSize);
    //            Array.Copy(fullBuffer, startIndexOfXml, xmlBuffer, 0, xmlBufferSize);
    //        }            

    //        fullBuffer = null;

    //        XmlDocument docDetails = new XmlDocument();
    //        string xml = Encoding.UTF8.GetString(xmlBuffer);
    //        docDetails.LoadXml(xml);

    //        int imageSize = 0;
    //        List<Species> loadedSpecies = new List<Species>();

    //        XmlNode nodeSize = docDetails.SelectSingleNode("/IMP/LOADEDFILE/PROPERTIES/DEFACQPARAMIMDATA/m_nSize");
    //        imageSize = int.Parse(nodeSize.InnerText);

    //        XmlNodeList nodeSpeciesList = docDetails.SelectNodes("IMP/LOADEDFILE/SPECIES");
    //        foreach(XmlNode nodeSpecies in nodeSpeciesList)
    //        {
    //            Species species = new Species();

    //            XmlNode nodeNumberCycles = nodeSpecies.SelectSingleNode("n_AcquiredCycleNb");
    //            species.Cycles = int.Parse(nodeNumberCycles.InnerText);

    //            XmlNode nodeSizeBytes = nodeSpecies.SelectSingleNode("SIZE");
    //            species.SizeInBytes = int.Parse(nodeSizeBytes.InnerText);

    //            XmlNode nodePixelEncoding = nodeSpecies.SelectSingleNode("PROPERTIES/COMMON_TO_ALL_SPECIESPCTRS/n_EncodedPixelType");
    //            species.PixelEncoding = int.Parse(nodePixelEncoding.InnerText);

    //            XmlNode nodeMass = nodeSpecies.SelectSingleNode("PROPERTIES/DPSPECIESDATA/d_Mass");
    //            species.Mass = double.Parse(nodeMass.InnerText);

    //            XmlNode nodeLabel = nodeSpecies.SelectSingleNode("PROPERTIES/DPSPECIESDATA/psz_MatrixSpecies");
    //            species.Label = nodeLabel.InnerText;

    //            XmlNode nodeWaitTime = nodeSpecies.SelectSingleNode("PROPERTIES/DPSPECIESDATA/d_WaitTime");
    //            species.WaitTime = double.Parse(nodeWaitTime.InnerText);

    //            XmlNode nodeCountTime = nodeSpecies.SelectSingleNode("PROPERTIES/DPSPECIESDATA/d_CountTime");
    //            species.CountTime = double.Parse(nodeCountTime.InnerText);

    //            XmlNode nodeWellTime = nodeSpecies.SelectSingleNode("PROPERTIES/DPSPECIESDATA/d_WellTime");
    //            species.WellTime = double.Parse(nodeWellTime.InnerText);

    //            XmlNode nodeExtraTime = nodeSpecies.SelectSingleNode("PROPERTIES/DPSPECIESDATA/d_ExtraTime");
    //            species.ExtraTime = double.Parse(nodeExtraTime.InnerText);

    //            loadedSpecies.Add(species);
    //        }

    //        int pixelType = loadedSpecies[0].PixelEncoding;
    //        int integerSize = 0;

    //        // Only know that 0 corresponds to Int16
    //        if (pixelType == 0) integerSize = 2;

    //        float[] values = new float[imageSize * imageSize * loadedSpecies.Count * loadedSpecies[0].Cycles];
    //        for (int i = 0; i < values.Length; i++)
    //        {
    //            //Int16
    //            if (pixelType == 0)
    //            {
    //                values[i] = BitConverter.ToInt16(dataBuffer, i * integerSize);
    //            }
    //        }


    //        float[,,] matrix = new float[imageSize, imageSize, loadedSpecies.Count];

    //        int pos = 0;

    //        for (int z = 0; z < loadedSpecies.Count; z++)
    //        {
    //            for (int a = 0; a < loadedSpecies[0].Cycles; a++)
    //            {
    //                for (int y = 0; y < imageSize; y++)
    //                {
    //                    for (int x = 0; x < imageSize; x++)
    //                    {
    //                        matrix[x, y, z] += values[pos++];
    //                    }
    //                }
    //            }
    //        }
            

    //        using(StreamWriter sw = new StreamWriter(@"G:\si - ta - 0310rae@1_1.csv"))
    //        {
    //            for (int y = 0; y < imageSize; y++)
    //            {
    //                StringBuilder sb = new StringBuilder();
    //                for (int x = 0; x < imageSize; x++)
    //                {
    //                    sb.Append($"{matrix[x, y, 0]},");
    //                }
    //                sb.Remove(sb.Length - 1, 1);
    //                sw.WriteLine(sb.ToString());
    //            }
    //        }
    //        using (StreamWriter sw = new StreamWriter(@"G:\si - ta - 0310rae@1_2.csv"))
    //        {
    //            for (int y = 0; y < imageSize; y++)
    //            {
    //                StringBuilder sb = new StringBuilder();
    //                for (int x = 0; x < imageSize; x++)
    //                {
    //                    sb.Append($"{matrix[x, y, 1]},");
    //                }
    //                sb.Remove(sb.Length - 1, 1);
    //                sw.WriteLine(sb.ToString());
    //            }
    //        }

    //        int j = 0;
    //        //foreach(XmlNode nodeDoc in docDetails.ChildNodes)
    //        //{
    //        //    // Get IMP node
    //        //    if(nodeDoc.Name == "IMP")
    //        //    {
    //        //        // Get LOADEDFILE node
    //        //        foreach(XmlNode nodeImp in nodeDoc.ChildNodes)
    //        //        {
    //        //            if (nodeImp.Name == "LOADEDFILE")
    //        //            {

    //        //                // Get PROPERTIES node
    //        //                foreach (XmlNode nodeLoadedFile in nodeImp.ChildNodes)
    //        //                {
    //        //                    if(nodeLoadedFile.Name == "PROPERTIES")
    //        //                    {

    //        //                        // Get DEFACQPARAMIMDATA node
    //        //                        foreach(XmlNode nodeProperties in nodeLoadedFile.ChildNodes)
    //        //                        {
    //        //                            if(nodeProperties.Name == "DEFACQPARAMIMDATA")
    //        //                            {

    //        //                                // Get m_nSize node
    //        //                                foreach(XmlNode nodeImageData in nodeProperties.ChildNodes)
    //        //                                {
    //        //                                    if(nodeImageData.Name == "m_nSize")
    //        //                                    {
    //        //                                        imageSize = int.Parse(nodeImageData.FirstChild.Value);
    //        //                                    }
    //        //                                }
    //        //                            }
    //        //                        }

    //        //                    }
    //        //                }

    //        //                // Get SPECIES nodes
    //        //                foreach(XmlNode nodeLoadedFile in nodeImp.ChildNodes)
    //        //                {
    //        //                    if(nodeLoadedFile.Name == "SPECIES")
    //        //                    {
    //        //                        Species s = new Species();

    //        //                        foreach (XmlNode nodeSpecies in nodeLoadedFile.ChildNodes)
    //        //                        {
    //        //                            if(nodeSpecies.Name == "n_AcquiredCycleNb")
    //        //                            {
    //        //                                s.Cycles = int.Parse(nodeSpecies.FirstChild.Value);
    //        //                            }

    //        //                            if(nodeSpecies.Name == "SIZE")
    //        //                            {
    //        //                                s.SizeInBytes = int.Parse(nodeSpecies.FirstChild.Value);
    //        //                            }

    //        //                            if(nodeSpecies.Name == "PROPERTIES")
    //        //                            {

    //        //                                // Get PROPERTIES node
    //        //                                foreach(XmlNode nodeProperties in nodeSpecies.ChildNodes)
    //        //                                {
    //        //                                    if(nodeProperties.Name == "d_Mass")
    //        //                                    {
    //        //                                        s.Mass = double.Parse(nodeProperties.FirstChild.Value);
    //        //                                    }
    //        //                                    if(nodeProperties.Name == "d_WaitTime")
    //        //                                    {
    //        //                                        s.WaitTime = double.Parse(nodeProperties.FirstChild.Value);
    //        //                                    }
    //        //                                    if(nodeProperties.Name == "d_CountTime")
    //        //                                    {
    //        //                                        s.CountTime = double.Parse(nodeProperties.FirstChild.Value);
    //        //                                    }
    //        //                                    if(nodeProperties.Name == "d_WellTime")
    //        //                                    {
    //        //                                        s.WellTime = double.Parse(nodeProperties.FirstChild.Value);
    //        //                                    }
    //        //                                    if(nodeProperties.Name == "d_ExtraTime")
    //        //                                    {
    //        //                                        s.ExtraTime = double.Parse(nodeProperties.FirstChild.Value);
    //        //                                    }
    //        //                                    if(nodeProperties.Name == "psz_MatrixSpecies")
    //        //                                    {
    //        //                                        s.Label = nodeProperties.FirstChild.Value;
    //        //                                    }
    //        //                                }
    //        //                            }
    //        //                        }

    //        //                        species.Add(s);
    //        //                    }

    //        //                }

    //        //            }

    //        //        }
    //        //    }
    //        //}

    //    }
    //}

    //internal static class TestSpec
    //{
    //    public static int[,] Generate()
    //    {
    //        int[,] spec = new int[10000, 2];

    //        Random r = new Random();
    //        int startTime = 5256;
    //        for (int x = 0; x < 10000; x++)
    //        {
    //            spec[x, 0] = startTime + x;
    //            spec[x, 1] = r.Next(0, 1000);
    //        }

    //        return spec;
    //    }
    //}
}
