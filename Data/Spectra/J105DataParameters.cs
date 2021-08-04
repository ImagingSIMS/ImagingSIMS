using System;

namespace ImagingSIMS.Data.Spectra
{
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
            set 
            {
                if (value.Revision != -1) _version = value;
                else
                {
                    _version = new Version(value.Major, value.Minor, value.Build, 1);
                }
            }
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
}
