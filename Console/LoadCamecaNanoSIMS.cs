using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImagingSIMS.Data;

namespace ConsoleApp
{
    partial class Program
    {
        private static void main_loadNanoSIMS()
        {
            string filePath = @"D:\Data\NanoSIMS\20141230_HL_Pilot_8_2.im";
            using (Stream stream = File.OpenRead(filePath))
            {
                BinaryReader br = new BinaryReader(stream);

                string tempString = string.Empty;

                CamecaNanoSIMSHeader header = new CamecaNanoSIMSHeader();

                header.Realease = br.ReadInt32();
                header.AnalysisType = br.ReadInt32();
                header.HeaderSize = br.ReadInt32();
                header.SampleType = br.ReadInt32();
                header.DataInlcuded = br.ReadInt32();
                header.PositionX = br.ReadInt32();
                header.PositionY = br.ReadInt32();
                tempString = new string(br.ReadChars(32));
                header.AnalysisName = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);
                tempString = new string(br.ReadChars(16));
                header.UserName = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);
                header.PositionZ = br.ReadInt32();

                int unusedInt = br.ReadInt32();
                unusedInt = br.ReadInt32();
                unusedInt = br.ReadInt32();

                tempString = new string(br.ReadChars(16));
                string date = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);
                tempString = new string(br.ReadChars(16));
                string time = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);

                header.AnalysisTime = CamecaNanoSIMSHeaderPart.ParseDateAndTimeStrings(date, time);

                CamecaNanoSIMSMaskImage mask = new CamecaNanoSIMSMaskImage();

                tempString = new string(br.ReadChars(16));
                mask.FileName = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);
                mask.AnalysisDuration = br.ReadInt32();
                mask.CycleNumber = br.ReadInt32();
                mask.ScanType = br.ReadInt32();
                mask.Magnification = br.ReadInt16();
                mask.SizeType = br.ReadInt16();
                mask.SizeDetector = br.ReadInt16();

                short unusedShort = br.ReadInt16();
                mask.BeamBlanking = br.ReadInt32();
                mask.Sputtering = br.ReadInt32();
                mask.SputteringDuration = br.ReadInt32();
                mask.AutoCalibrationInAnalysis = br.ReadInt32();

                CamecaNanoSIMSAutoCal autoCal = new CamecaNanoSIMSAutoCal();
                tempString = new string(br.ReadChars(64));
                autoCal.Mass = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);
                autoCal.Begin = br.ReadInt32();
                autoCal.Period = br.ReadInt32();
                mask.AutoCal = autoCal;

                mask.SigReference = br.ReadInt32();

                CamecaNanoSIMSSigRef sigRef = new CamecaNanoSIMSSigRef();
                CamecaNanoSIMSPolyatomic polyatomic = new CamecaNanoSIMSPolyatomic();
                polyatomic.FlagNumeric = br.ReadInt32();
                polyatomic.NumericValue = br.ReadInt32();
                polyatomic.NumberElements = br.ReadInt32();
                polyatomic.NumberCharges = br.ReadInt32();
                polyatomic.Charge = new string(br.ReadChars(1));
                tempString = new string(br.ReadChars(64));
                polyatomic.MassLabel = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);
                polyatomic.Tablets = new CamecaNanoSIMSTablets[5];
                for (int i = 0; i < 5; i++)
                {
                    polyatomic.Tablets[i] = new CamecaNanoSIMSTablets()
                    {
                        NumberElements = br.ReadInt32(),
                        NumberIsotopes = br.ReadInt32(),
                        Quantity = br.ReadInt32()
                    };
                }

                string unusedString = new string(br.ReadChars(3));

                sigRef.Polyatomic = polyatomic;
                sigRef.Detector = br.ReadInt32();
                sigRef.Offset = br.ReadInt32();
                sigRef.Quantity = br.ReadInt32();

                mask.SigRef = sigRef;
                mask.NumberMasses = br.ReadInt32();

                int tabMassPointer = 0;
                int numberTabMasses = 10;
                if (header.Realease >= 4108)
                {
                    numberTabMasses = 60;
                }

                for (int i = 0; i < numberTabMasses; i++)
                {
                    tabMassPointer = br.ReadInt32();
                    if (tabMassPointer > 0)
                    {
                        Console.WriteLine($"i {i}: {tabMassPointer}");
                    }
                }

                string[] massNames = new string[mask.NumberMasses];
                string[] massSymbols = new string[mask.NumberMasses];
                CamecaNanoSIMSTabMass[] masses = new CamecaNanoSIMSTabMass[mask.NumberMasses];
                for (int i = 0; i < mask.NumberMasses; i++)
                {
                    CamecaNanoSIMSTabMass mass = new CamecaNanoSIMSTabMass();
                    unusedInt = br.ReadInt32();
                    unusedInt = br.ReadInt32();
                    mass.Amu = br.ReadDouble();
                    mass.MatrixOrTrace = br.ReadInt32();
                    mass.Detector = br.ReadInt32();
                    mass.WaitingTime = br.ReadDouble();
                    mass.CountingTime = br.ReadDouble();
                    mass.Offset = br.ReadInt32();
                    mass.MagField = br.ReadInt32();

                    CamecaNanoSIMSPolyatomic poly = new CamecaNanoSIMSPolyatomic();
                    poly.FlagNumeric = br.ReadInt32();
                    poly.NumericValue = br.ReadInt32();
                    poly.NumberElements = br.ReadInt32();
                    poly.NumberCharges = br.ReadInt32();
                    poly.Charge = new string(br.ReadChars(1));
                    tempString = new string(br.ReadChars(64));
                    poly.MassLabel = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);
                    poly.Tablets = new CamecaNanoSIMSTablets[5];
                    for (int j = 0; j < 5; j++)
                    {
                        poly.Tablets[j] = new CamecaNanoSIMSTablets()
                        {
                            NumberElements = br.ReadInt32(),
                            NumberIsotopes = br.ReadInt32(),
                            Quantity = br.ReadInt32()
                        };
                    }
                    unusedString = new string(br.ReadChars(3));
                    mass.Polyatomic = poly;

                    massNames[i] = mass.Amu.ToString("0.00");
                    massSymbols[i] = string.IsNullOrEmpty(mass.Polyatomic.MassLabel) ? "-" : mass.Polyatomic.MassLabel;

                    masses[i] = mass;
                }

                if (header.Realease >= 4018)
                {
                    // Read metadata v7
                    long posPolyList = 652 + 288 * mask.NumberMasses;
                    long posNbPoly = posPolyList + 16;
                    br.BaseStream.Seek(posNbPoly, SeekOrigin.Begin);
                    header.NumberPolyatomics = br.ReadInt32();

                    long posMaskNano = 676 + 288 * mask.NumberMasses + 144 * header.NumberPolyatomics;
                    long posNbField = posMaskNano + 4 * 24;
                    br.BaseStream.Seek(posNbField, SeekOrigin.Begin);
                    header.NumberMagneticFields = br.ReadInt32();

                    long posBFieldNano = 2228 + 288 * mask.NumberMasses + 144 * header.NumberPolyatomics;
                    long posnNbField = posBFieldNano + 4;
                    br.BaseStream.Seek(posnNbField, SeekOrigin.Begin);
                    header.MagneticField = br.ReadInt32();

                    long posTabTrolley = posBFieldNano + 10 * 4 + 2 * 8;
                    header.Radii = new double[12];
                    StringBuilder sbRadius = new StringBuilder();
                    for (int i = 0; i < header.Radii.Length; i++)
                    {
                        long posRadius = posTabTrolley + i * 208 + 64 + 8;
                        br.BaseStream.Seek(posRadius, SeekOrigin.Begin);
                        header.Radii[i] = br.ReadDouble();
                        sbRadius.Append($"{header.Radii[i].ToString("0.00")} - ");
                    }
                    sbRadius = sbRadius.Remove(sbRadius.Length - 2, 2);
                    header.Radius = sbRadius.ToString();

                    long posAnalParam = 2228 + 288 * mask.NumberMasses + 144 * header.NumberPolyatomics + 2840 * header.NumberMagneticFields;
                    long posComment = posAnalParam + 16 + 4 + 4 + 4 + 4;
                    br.BaseStream.Seek(posComment, SeekOrigin.Begin);

                    var bytes = br.ReadBytes(256);
                    tempString = new string(br.ReadChars(256));
                    header.Comments = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);

                    long posAnalPrimary = posAnalParam + 16 + 4 + 4 + 4 + 4 + 256;
                    long posPrimCurrentT0 = posAnalPrimary + 8;
                    br.BaseStream.Seek(posPrimCurrentT0, SeekOrigin.Begin);
                    header.PrimaryCurrentT0 = br.ReadInt32();
                    header.PrimaryCurrentEnd = br.ReadInt32();

                    long posPrimL1 = posAnalPrimary + 8 + 4 + 4 + 4;
                    br.BaseStream.Seek(posPrimL1, SeekOrigin.Begin);
                    header.PrimaryL1 = br.ReadInt32();

                    long posD1Pos = posAnalPrimary + 8 + 4 + 4 + 4 + 4 + 4 + 4 * 10 + 4 + 4 * 10;
                    br.BaseStream.Seek(posD1Pos, SeekOrigin.Begin);
                    header.PositionD1 = br.ReadInt32();

                    long sizeApPrimary = 552;
                    long posApSecondary = posAnalPrimary + sizeApPrimary;
                    long posPrimL0 = posApSecondary - (67 * 4 + 4 * 10 + 4 + 4 + 4 + 4);
                    br.BaseStream.Seek(posPrimL0, SeekOrigin.Begin);
                    header.PrimaryL0 = br.ReadInt32();
                    header.CsHV = br.ReadInt32();

                    long posESPos = posApSecondary + 8;
                    br.BaseStream.Seek(posESPos, SeekOrigin.Begin);
                    header.PositionES = br.ReadInt32();

                    long posASPos = posESPos + 4 + 40 + 40;
                    br.BaseStream.Seek(posASPos, SeekOrigin.Begin);
                    header.PositionAS = br.ReadInt32();
                }

                long offset = header.HeaderSize - CamecaNanoSIMSHeaderImage.STRUCT_SIZE;
                br.BaseStream.Seek(offset, SeekOrigin.Begin);

                CamecaNanoSIMSHeaderImage headerImage = new CamecaNanoSIMSHeaderImage();
                headerImage.SizeSelf = br.ReadInt32();
                headerImage.Type = br.ReadInt16();
                headerImage.Width = br.ReadInt16();
                headerImage.Height = br.ReadInt16();
                headerImage.PixelDepth = br.ReadInt16();

                headerImage.NumberMasses = br.ReadInt16();
                if (headerImage.NumberMasses < 1) headerImage.NumberMasses = 1;

                headerImage.Depth = br.ReadInt16();
                headerImage.Raster = br.ReadInt32();
                tempString = new string(br.ReadChars(64));
                headerImage.Nickname = CamecaNanoSIMSHeaderPart.RemovePadCharacters(tempString);

                int numMasses = headerImage.NumberMasses;
                int numImages = headerImage.Depth;

                List<Data3D> readIn = new List<Data3D>();
                for (int j = 0; j < numMasses; j++)
                {
                    readIn.Add(new Data3D(headerImage.Width, headerImage.Height, headerImage.Depth));
                }

                bool is16bit = headerImage.PixelDepth == 2;

                long theoreticalSize = headerImage.Width * headerImage.Height * headerImage.Depth * headerImage.NumberMasses * headerImage.PixelDepth + header.HeaderSize;
                long fileSize = br.BaseStream.Length;

                bool isOk = theoreticalSize == fileSize;

                for (int i = 0; i < numImages; i++)
                {
                    for (int j = 0; j < numMasses; j++)
                    {
                        for (int x = 0; x < headerImage.Width; x++)
                        {
                            for (int y = 0; y < headerImage.Height; y++)
                            {
                                if (is16bit)
                                    readIn[j][x, y, i] = br.ReadInt16();
                                else readIn[j][x, y, i] = br.ReadInt32();
                            }
                        }
                    }
                }

                int zzz = 0;
            }
        }
    }

    class LoadCamecaNanoSIMS
    {
    }

    internal abstract class CamecaNanoSIMSHeaderPart
    {
        public static DateTime ParseDateAndTimeStrings(string date, string time)
        {
            string formatted = $"{date} - {time}";
            return DateTime.ParseExact(formatted, "dd.MM.yy - H:m", CultureInfo.InvariantCulture);
        }
        public static string RemovePadCharacters(string padded)
        {
            return padded.Replace("\0", "");
        }
    }
    internal class CamecaNanoSIMSHeader
    {
        public int Realease { get; set; }
        public int AnalysisType { get; set; }
        public int HeaderSize { get; set; }
        public int SampleType { get; set; }
        public int DataInlcuded { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public string AnalysisName { get; set; }
        public string UserName { get; set; }
        public int PositionZ { get; set; }
        public DateTime AnalysisTime { get; set; }

        // AnalysisType:    MIMS_IMAGE = 27
        //                  MIMS_LINE_SCAN_IMAGE = 39
        //                  MIMS_SAMPLE_STAGE_IMAGE = 41

        // v7 Meta Data:
        public int NumberPolyatomics { get; set; }
        public int NumberMagneticFields { get; set; }
        public int MagneticField { get; set; }
        public double[] Radii { get; set; }
        public string Radius { get; set; }
        public string Comments { get; set; }
        public int PrimaryCurrentT0 { get; set; }
        public int PrimaryCurrentEnd { get; set; }
        public int PrimaryL1 { get; set; }
        public int PositionD1 { get; set; }
        public int PrimaryL0 { get; set; }
        public int CsHV { get; set; }
        public int PositionES { get; set; }
        public int PositionAS { get; set; }
    }

    internal class CamecaNanoSIMSMaskImage
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
    }

    internal class CamecaNanoSIMSAutoCal
    {
        public string Mass { get; set; }
        public int Begin { get; set; }
        public int Period { get; set; }
    }

    internal class CamecaNanoSIMSSigRef
    {
        public CamecaNanoSIMSPolyatomic Polyatomic { get; set; }
        public int Detector { get; set; }
        public int Offset { get; set; }
        public int Quantity { get; set; }
    }

    internal class CamecaNanoSIMSPolyatomic
    {
        public int FlagNumeric { get; set; }
        public int NumericValue { get; set; }
        public int NumberElements { get; set; }
        public int NumberCharges { get; set; }
        public string Charge { get; set; }
        public string MassLabel { get; set; }
        public CamecaNanoSIMSTablets[] Tablets { get; set; }
    }

    internal class CamecaNanoSIMSTablets
    {
        public int NumberElements { get; set; }
        public int NumberIsotopes { get; set; }
        public int Quantity { get; set; }
    }

    internal class CamecaNanoSIMSTabMass
    {
        public double Amu { get; set; }
        public int MatrixOrTrace { get; set; }
        public int Detector { get; set; }
        public double WaitingTime { get; set; }
        public double CountingTime { get; set; }
        public int Offset { get; set; }
        public int MagField { get; set; }
        public CamecaNanoSIMSPolyatomic Polyatomic { get; set; }
    }

    internal class CamecaNanoSIMSHeaderImage
    {
        public const int STRUCT_SIZE = 84;

        public int SizeSelf { get; set; }
        public short Type { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
        public short PixelDepth { get; set; }
        public short NumberMasses { get; set; }
        public short Depth { get; set; }
        public int Raster { get; set; }
        public string Nickname { get; set; }
    }
}
