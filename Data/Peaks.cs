using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ImagingSIMS.Common.Math;

namespace ImagingSIMS.Data.Analysis
{
    public class Peak
    {
        double _mz;
        double _start;
        double _end;
        double _max;
        double _integrated;
        double _width;

        List<double[]> points;

        public double Mz { get { return _mz; } }
        public double Start { get { return _start; } }
        public double End { get { return _end; } }
        public double Width { get { return _width; } }
        public double MaxIntensity { get { return _max; } }
        public double IntegratedIntensity { get { return _integrated; } }

        public Peak()
        {
            points = new List<double[]>();
        }

        public void AddPoint(double Mz, double Intensity)
        {
            points.Add(new double[2] { Mz, Intensity });
        }
        public void FinalizePeak()
        {
            double start = 10000;
            double end = 0;

            double max = 0;
            double maxMz = 0;
            double integrated = 0;

            for (int i = 0; i < points.Count; i++)
            {
                double[] point = points[i];
                double mass = point[0];
                double intensity = point[1];

                if (mass < start) start = mass;
                if (mass > end) end = mass;

                if (intensity > max)
                {
                    max = intensity;
                    maxMz = mass;
                }

                integrated += intensity;
            }

            _mz = maxMz;
            _start = start;
            _end = end;
            _width = end - start;
            _max = max;
            _integrated = integrated;
        }

        public override string ToString()
        {
            return string.Format("m/z {0}; Max {1}", _mz.ToString("0.00"), _max.ToString("0"));
        }

        public static ObservableCollection<Peak> Find(double[,] Spectrum, int Threshold, BackgroundWorker bw)
        {
            if (Spectrum == null) throw new ArgumentNullException("No input data.");

            int sizeY = Spectrum.GetLength(0);

            ObservableCollection<Peak> peaks = new ObservableCollection<Peak>();

            ApplyThreshold(ref Spectrum, Threshold);

            for (int x = 0; x < sizeY; x++)
            {
                if (x + 1 == sizeY) break;

                double delta = Spectrum[x + 1, 1] - Spectrum[x, 1];
                if (delta <= 0) continue;

                int positiveCount = 0;
                int totalCount = 0;
                for (int i = 0; i < 10; i++)
                {
                    if (x + i + 1 == sizeY) break;
                    double d = Spectrum[x + i + 1, 1] - Spectrum[x + i, 1];
                    if (d > 0) positiveCount++;
                    totalCount++;
                }
                double percent = (double)positiveCount / (double)totalCount;
                if (percent >= 0.5d)
                {
                    int basePosition = 0;

                    bool proceed = true;
                    Peak peak = new Peak();
                    while (proceed)
                    {
                        basePosition++;
                        if (x + basePosition == sizeY)
                        {
                            proceed = false;
                            break;
                        }
                        peak.AddPoint(Spectrum[x + basePosition, 0], Spectrum[x + basePosition, 1]);
                        double intensity = Spectrum[x + basePosition, 1];

                        if (intensity <= 0) proceed = false;
                    }
                    peak.FinalizePeak();
                    if (peak.Mz >= 5)
                    {
                        peaks.Add(peak);
                    }
                    x += basePosition;
                }
                if (bw != null) bw.ReportProgress(ImagingSIMS.Common.Math.Percentage.GetPercent(x, sizeY));
            }

            return peaks;
        }

        private static void ApplyThreshold(ref double[,] Data, int Threshold)
        {
            int length = Data.GetLength(0);
            for (int x = 0; x < length; x++)
            {
                if (Data[x, 1] <= Threshold) Data[x, 1] = Threshold;
            }
        }
    }
}

namespace ImagingSIMS.Data.Spectra
{
    public class Peak
    {
        double _mz;
        double _start;
        double _end;
        double _max;
        double _integrated;
        double _width;
        int _hmStart;
        int _hmEnd;

        double[] _masses;
        double[] _intensities;

        public double Mz { get { return _mz; } }
        public double Start { get { return _start; } }
        public double End { get { return _end; } }
        public double Width { get { return _width; } }
        public double MaxIntensity { get { return _max; } }
        public double IntegratedIntensity { get { return _integrated; } }
        public double[] Masses { get { return _masses; } }
        public double[] Intensities { get { return _intensities; } }

        public double HalfMaxStartMass
        {
            get { return _masses[_hmStart]; }
        }
        public double HalfMaxEndMass
        {
            get { return _masses[_hmEnd]; }
        }
        public int HalfMaxStart { get { return _hmStart; } }
        public int HalfMaxEnd { get { return _hmEnd; } }
        public double FWHM
        {
            get { return HalfMaxEndMass - HalfMaxStartMass; }
        }
        public double IntensityFWHM
        {
            get
            {
                if (_intensities == null || _intensities.Length == 0)
                    return 0;

                double integrated = 0;
                for (int i = _hmStart; i <= _hmEnd; i++)
                {
                    integrated += _intensities[i];
                }
                return integrated;
            }
        }

        public Peak()
        {

        }

        public Peak(double[] masses, double[] intensities)
        {
            if (masses.Length != intensities.Length)
                throw new ArgumentException("Invalid parameters. Length of masses and intensities does not match.");

            double integratedSum = 0;
            double max = 0;
            int indexOfMax = 0;

            for (int i = 0; i < intensities.Length; i++)
            {
                integratedSum += intensities[i];
                if (intensities[i] > max)
                {
                    max = intensities[i];
                    indexOfMax = i;
                }
            }

            _mz = masses[indexOfMax];
            _start = masses[0];
            _end = masses[masses.Length - 1];
            _max = max;
            _integrated = integratedSum;
            _width = _end - _start;

            _masses = masses;
            _intensities = intensities;

            calculateFWHM();
        }
        public static Peak FromArrays(double[] masses, double[] intensities)
        {
            return new Peak(masses, intensities);
        }

        // FWHM
        private void calculateFWHM()
        {
            if (_intensities == null || _intensities.Length == 0)
                throw new ArgumentNullException("Intensity information missing.");

            int length = _intensities.Length;
            double halfMax = _max / 2;
            for (int i = 0; i < length; i++)
            {
                if (_intensities[i] == halfMax)
                {
                    _hmStart = i;
                    break;
                }
                if(i + 1 == length)
                {
                    _hmStart = 0;
                    break;
                }
                else if (_intensities[i] < halfMax && _intensities[i + 1] >= halfMax)
                {
                    if (_intensities[i + 1] == halfMax)
                    {
                        _hmStart = i + 1;
                        break;
                    }
                    else
                    {
                        _hmStart = i;
                        break;
                    }
                }
            }

            for (int i = length - 1; i > _hmStart; i--)
            {
                if (_intensities[i] == halfMax)
                {
                    _hmEnd = i;
                    break;
                }
                if(i-1 < 0)
                {
                    _hmEnd = length - 1;
                    break;
                }
                else if(_intensities[i]<halfMax && _intensities[i-1]>= halfMax)
                {
                    if(_intensities[i-1]== halfMax)
                    {
                        _hmEnd = i - 1;
                        break;
                    }
                    else
                    {
                        _hmEnd = i;
                        break;
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"m/z: {_mz.ToString("0.000")}; Start: {_start.ToString("0.000")}; End: {_end.ToString("0.000")}; Max: {_max.ToString("0")}; Integrated: {_integrated.ToString("0")}; Width: {_width.ToString("0.000")}";
        }

        // 1. Look for slope up
        // 2. Find maximum
        // 3. Look for where peak goes back down below set threshold (i.e. 80% maximum)
        // 4. Run gaussian fit to determine peak properties

        public static ObservableCollection<Peak> IdentifyPeaks(double[,] spectrum, double ampThreshold, double peakRetreatThreshold)
        {
            ObservableCollection<Peak> foundPeaks = new ObservableCollection<Peak>();

            int specLength = spectrum.GetLength(0);

            double[] intensities = new double[specLength];
            double[] masses = new double[specLength];
            for (int i = 0; i < specLength; i++)
            {
                intensities[i] = spectrum[i, 1];
                masses[i] = spectrum[i, 0];
            }

            double[] smoothedIntensities = smoothArray(intensities);
            //double[] derivative1D = getDerivative(intensities);
            double[] derivative1D = getDerivative(smoothedIntensities);
            derivative1D = smoothArray(derivative1D);

            for (int i = 0; i < specLength; i++)
            {
                if (derivative1D[i] <= 0) continue;

                // Peak begins when slope is positive
                int startIndex = i;
                // Find when slope switches from positive to negative to indicate local maximum
                double slope = derivative1D[i];
                while (slope > 0)
                {
                    i++;
                    if (i >= specLength) break;

                    if (slope == 0 && derivative1D[i + 1] == 0) break;
                    slope = derivative1D[i];
                }

                int indexOfPeak = i;
                double maximum = intensities[i];

                // Check to see if it is a local maximum by continuing through and testing
                // that the value is above the retreat threshold
                double value = intensities[i];
                while(value / maximum > peakRetreatThreshold)
                {
                    if(value > maximum)
                    {
                        maximum = value;
                        indexOfPeak = i;
                    }

                    i++;
                    if (i >= specLength) break;
                    value = intensities[i];
                }

                // Check that the height of the peak is above the threshold value set
                if (maximum < ampThreshold) continue;

                // Keep iterating until the next negative slope is found otherwise cannot
                // extrapolate down to zero
                double retreatSlope = derivative1D[i];
                while(retreatSlope >= 0)
                {
                    i++;
                    if (i >= specLength) break;
                    retreatSlope = derivative1D[i];
                }

                // Given the slope at the breaking condition, extrapolate down to intensity is zero
                // to determine the end point of the peak
                int retreatSlopeIndex = i;
                int endIndex = (int)(-value / retreatSlope) + retreatSlopeIndex;
                if (endIndex > specLength - 1 || endIndex < 0) endIndex = specLength - 1;

                // Go backward from max and find where peak retreats below threshold
                int x = indexOfPeak;
                value = intensities[x];
                while(value / maximum > peakRetreatThreshold)
                {
                    x--;
                    if (x <= startIndex) break;
                    value = intensities[x];
                }

                // Extrapolate to intensity is zero to determine start point of the peak                
                retreatSlope = derivative1D[x];
                while (retreatSlope <= 0)
                {
                    x--;
                    // Break if no suitable slope was found for extrapolation before reaching start
                    // of identified peak. In this case, just use the original index that triggered
                    // the start of the peak identificaiton
                    if (x <= startIndex) break;
                    retreatSlope = derivative1D[x];
                }

                // If the break condition wasn't triggered, then extrapolate and determine the new
                // starting index
                if (x > startIndex)
                {
                    retreatSlopeIndex = x;
                    int newStartIndex = retreatSlopeIndex - (int)(value / retreatSlope);
                    startIndex = newStartIndex > startIndex ? newStartIndex : startIndex;
                }
                // Determine peak range and extract values from spectrum
                int peakWidth = endIndex - startIndex;
                double[] peakValues = new double[peakWidth];
                double[] peakMasses = new double[peakWidth];
                for (int j = 0; j < peakWidth; j++)
                {
                    peakValues[j] = spectrum[startIndex + j, 1];
                    peakMasses[j] = spectrum[startIndex + j, 0];
                }


                // Fit a gaussian curve to the peak values to determine peak properties
                GaussianFit fit = gaussFit(peakMasses, peakValues);
                if (fit.IntegratedIntensity == 0) continue;

                // If peak is valid, advance the position to the extrapolated end point
                i = endIndex;

                // Add peak to collection
                foundPeaks.Add(Peak.FromArrays(peakMasses, peakValues));
            }

            return foundPeaks;
        }

        private static GaussianFit gaussFit(double[] x, double[] y)
        {
            int rangeLength = x.GetLength(0);

            double sum = 0;
            for (int i = 0; i < rangeLength; i++)
            {
                sum += y[i];
            }

            // If sum is 0, then no data in this range
            if (sum == 0) return new GaussianFit();

            double avg = sum / rangeLength;
            double integratedIntensity = sum;
            sum = 0;
            for (int i = 0; i < rangeLength; i++)
            {
                sum += (y[i] - avg) * (y[i] - avg);
            }
            double stdDev = Math.Sqrt(sum / rangeLength);

            double[] fitted = new double[rangeLength];
            for (int i = 0; i < rangeLength; i++)
            {
                fitted[i] = ((1 / (stdDev * Math.Sqrt(2 * Math.PI))) * 
                    Math.Pow(Math.E, -(((y[i] - avg) * (y[i] - avg)) / (2 * stdDev * stdDev))));
            }

            int indexOfMax = 0;
            double maxValue = 0;
            for (int i = 0; i < rangeLength; i++)
            {
                if(fitted[i]> maxValue)
                {
                    maxValue = fitted[i];
                    indexOfMax = i;
                }
            }

            return new GaussianFit()
            {
                Height = maxValue,
                Width = x[rangeLength - 1] - x[0],
                Mz = x[indexOfMax],
                IntegratedIntensity = integratedIntensity
            };
        }

        internal struct GaussianFit
        {
            internal double Height;
            internal double Mz;
            internal double Width;
            internal double IntegratedIntensity;
        }
        
        private static double[] smoothArray(double[] array)
        {
            int length = array.GetLength(0);
            double[] smoothed = new double[length];

            // Fill in data that will be skipped in loop
            smoothed[0] = array[0];
            smoothed[1] = array[1];
            smoothed[length - 2] = array[length - 2];
            smoothed[length - 1] = array[length - 1];

            for (int i = 2; i < length - 2; i++)
            {
                double sum = 0;
                sum += array[i - 2] + array[i + 2];
                sum += 2 * (array[i - 1] + array[i + 1]);
                sum += 3 * array[i];
                smoothed[i] = sum / 9d;
            }

            return smoothed;
        }

        private static double[] getDerivative(double[] array)
        {
            int length = array.GetLength(0);
            double[] derivative = new double[length];

            for (int i = 0; i < length - 1; i++)
            {
                derivative[i] = array[i + 1] - array[i];
            }

            return derivative;
        }
    }
}
