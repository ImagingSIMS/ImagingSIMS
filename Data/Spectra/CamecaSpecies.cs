using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ImagingSIMS.Data.Spectra
{
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

                    for (int i = startIndex; i <= endIndex; i++)
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
}
