using System;
using System.Collections.Generic;
using System.ComponentModel;
using ImagingSIMS.Common.Math;

namespace ImagingSIMS.Data.Spectra
{
    public abstract class Spectrum : IDisposable, INotifyPropertyChanged, ISavable, IWorkspaceData
    {
        protected string _name;

        protected int _sizeX;
        protected int _sizeY;
        protected int _sizeZ;

        protected SpectrumType _specType;

        protected bool _isLoading;

        protected uint[] _intensities;
        protected float[] _masses;

        /// <summary>
        /// Gets the name of the spectrum.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        /// <summary>
        /// Gets the width of the spectrum.
        /// </summary>
        public int SizeX
        {
            get { return _sizeX; }
        }
        /// <summary>
        /// Gets the height of the spectrum.
        /// </summary>
        public int SizeY
        {
            get { return _sizeY; }
        }
        /// <summary>
        /// Gets the depth of the spectrum.
        /// </summary>
        public int SizeZ
        {
            get { return _sizeZ; }
        }

        protected float _startMass;
        protected float _endMass;
        /// <summary>
        /// Gets the start mass of the spectrum.
        /// </summary>
        public float StartMass
        {
            get
            {
                return _startMass;
            }
        }

        /// <summary>
        /// Gets the end mass of the spectrum.
        /// </summary>
        public float EndMass
        {
            get
            {
                return _endMass;
            }
        }

        /// <summary>
        /// Gets the data type of the spectrum.
        /// </summary>
        public SpectrumType SpectrumType
        {
            get { return _specType; }
        }

        /// <summary>
        /// Gets a string representation of the spectrum dimensions as "{Width}x{Height}x{Depth}"
        /// </summary>
        public string Dimensions
        {
            get { return string.Format("{0}x{1}x{2}", SizeX, SizeY, SizeZ); }
        }

        /// <summary>
        /// Gets a boolean value if data is currently being loaded into the spectrum.
        /// </summary>
        public bool IsLoading
        {
            get { return _isLoading; }
        }

        /// <summary>
        /// Disposes the spectrum.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        /// <summary>
        /// Initializes a new instance of the Spectrum class.
        /// </summary>
        /// <param name="Name">Name of the spectrum.</param>
        public Spectrum(string Name)
        {
            _name = Name;
        }

        /// <summary>
        /// Load a mass spectrum from disk.
        /// </summary>
        /// <param name="FilePath">File location to load.</param>
        /// <param name="bw">BackgroundWorker instance for the method to update.</param>
        public abstract void LoadFromFile(string FilePath, BackgroundWorker bw);
        /// <summary>
        /// Load a mass spectra series from disk.
        /// </summary>
        /// <param name="FilePaths">Array of file locations to load.</param>
        /// <param name="bw">BackgroundWorker instance for the method to update.</param>
        public abstract void LoadFromFile(string[] FilePaths, BackgroundWorker bw);

        /// <summary>
        /// Compiles a two dimensional matrix of intensities within the specified mass range.
        /// </summary>
        /// <param name="MassRange">The desired mass range.</param>
        /// <param name="bw">BackgroundWorker instance for the method to update.</param>
        /// <returns>A two dimensional intensity matrix.</returns>
        public abstract Data2D FromMassRange(MassRange MassRange, BackgroundWorker bw = null);
        /// <summary>
        /// Compiles a two dimensional matrix of intensities within the specified mass range.
        /// </summary>
        /// <param name="MassRange">The desired mass range.</param>
        /// <param name="Layer">Layer of spectrum to sample.</param>
        /// <param name="bw">BackgroundWorker instance for the method to update.</param>
        /// <returns>A two dimensional intensity matrix series.</returns>
        public abstract Data2D FromMassRange(MassRange MassRange, int Layer, BackgroundWorker bw = null);
        /// <summary>
        /// Compiles a two dimensional matrix of intensities within the specified mass range.
        /// </summary>
        /// <param name="MassRange">The desired mass range.</param>
        /// <param name="Max">The maximum intensity in the DataTable.</param>
        /// <param name="bw">BackgroundWorker instance for the method to update.</param>
        /// <returns>A two dimensional intensity matrix.</returns>
        public abstract Data2D FromMassRange(MassRange MassRange, out float Max, BackgroundWorker bw = null);
        /// <summary>
        /// Compiles a series of two dimensional matrices of intensities for each mass spectrum within the specified mass range.
        /// </summary>
        /// <param name="MassRange">The desired mass range.</param>
        /// <param name="TableBaseName">Base name for the table series.</param>
        /// <param name="bw">BackgroundWorker instance for the method to update.</param>
        /// <returns>A two dimensional intensity matrix series.</returns>
        public abstract List<Data2D> FromMassRange(MassRange MassRange, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null);
        /// <summary>
        /// Compiles a series of two dimensional matrices of intensities for each mass spectrum within the specified mass range.
        /// </summary>
        /// <param name="MassRange">The desired mass range.</param>
        /// <param name="Layer">Layer of spectrum to sample.</param>
        /// <param name="TableBaseName">Base name for the table series.</param>
        /// <param name="bw">BackgroundWorker instance for the method to update.</param>
        /// <returns>A two dimensional intensity matrix series.</returns>
        public abstract Data2D FromMassRange(MassRange MassRange, int Layer, string TableBaseName, bool OmitNumbering, BackgroundWorker bw = null);
        /// <summary>
        /// Creates a depth profile (intensity as a function of layer number) of the the given spectrum within the specified mass range.
        /// </summary>
        /// <param name="MassRange">The desired mass range.</param>
        /// <param name="bw">BackgroundWorker instance for the method to update.</param>
        /// <returns>Double array of intensity as a function of depth.</returns>
        public double[] CreateDepthProfile(MassRange MassRange, BackgroundWorker bw = null)
        {
            double[] depthProfile = new double[this.SizeZ];
            int totalSteps = this.SizeZ;

            for (int z = 0; z < this.SizeZ; z++)
            {
                Data2D layer = FromMassRange(MassRange, z, "", true, bw);

                depthProfile[z] = layer.TotalCounts;

                if (bw != null) bw.ReportProgress(Percentage.GetPercent(z, totalSteps));
            }

            if (bw != null) bw.ReportProgress(100);
            return depthProfile;
        }

        /// <summary>
        /// Converts the spectrum to a two dimensional double array. 
        /// </summary>
        /// <returns>Two dimensional array. Column 0 is mass values and column 1 is intensities.</returns>
        public abstract double[,] ToDoubleArray();
        /// <summary>
        /// Gets a series of intensites and mass values for display.
        /// </summary>
        /// <param name="Masses">Array of mass values.</param>
        /// <returns>Array of intensites.</returns>
        public abstract uint[] GetSpectrum(out float[] Masses);

        /// <summary>
        /// Creates a p x m matrix from the spectra with default mass binnings (if necessary).
        /// </summary>
        /// <returns>Two dimensional matrix of size p x m where p is the number of pixels and m is the number of mass channels.</returns>
        public abstract double[,] GetPxMMatrix();
        /// <summary>
        /// Creates a p x m matrix from the spectra with default mass binnings (if necessary).
        /// </summary>
        /// <param name="binCenters">Array of mass centers to bin and include.</param>
        /// <param name="binWidths">Array of bin widths. Must correspond to number of centers specified.</param>
        /// <returns>Two dimensional matrix of size p x m where p is the number of pixels and m is the number of mass channels.</returns>
        public abstract double[,] GetPxMMatrix(double[] binCenters, double[] binWidths);
        /// <summary>
        /// Creates a p x m matrix from the spectra with default mass binnings (if necessary).
        /// </summary>
        /// <param name="binCenters">Array of mass centers to bin and include.</param>
        /// <param name="binWidths">Array of bin widths. Must correspond to number of centers specified.</param>
        /// <param name="startMass">Start of mass range to create matrix from.</param>
        /// <param name="endMass">End of mass range to create matrix from.</param>
        /// <returns>Two dimensional matrix of size p x m where p is the number of pixels and m is the number of mass channels.</returns>
        public abstract double[,] GetPxMMatrix(double[] binCenters, double[] binWidths, double startMass, double endMass);

        /// <summary>
        /// Saves the spectrum as an ASCII text file with the format: mass,intensity
        /// </summary>
        /// <param name="filePath">Path to save file to.</param>
        public abstract void SaveText(string filePath);
        /// <summary>
        /// Saves the spectrum as an ASCII text file with the format: mass,intensity
        /// </summary>
        /// <param name="filePath">Path to save file to.</param>
        /// <param name="binSize">Number of bins to sum together.</param>
        public abstract void SaveText(string filePath, int binSize);

        public abstract byte[] ToByteArray();
        public abstract void FromByteArray(byte[] array);
    }
}
