using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ImagingSIMS.Data.Spectra.CamecaHeaders
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