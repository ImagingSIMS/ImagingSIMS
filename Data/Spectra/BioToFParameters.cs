using System;

namespace ImagingSIMS.Data.Spectra
{
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
}
