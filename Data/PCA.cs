using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ImagingSIMS.Data.Spectra;

using IronPython.Hosting;

namespace ImagingSIMS.Data.PCA
{
    public class PCA
    {
        Spectrum _originalSpectrum;
        double[,] _totalSpectrum;
        double[,] _totalSpectrumSmoothed;

        public Spectrum OriginalSpectrum
        {
            get { return _originalSpectrum; }
        }

        public PCA(Spectrum original)
        {
            _originalSpectrum = original;
        }



        private double smoothSpectrum(double[,] original)
        {
            int specLength = original.GetLength(1);

            return 0;
        }

        public static bool VerifyPython()
        {
            var py = Python.CreateEngine();
            var scriptScope = py.CreateScope();
            var scriptSource = py.CreateScriptSourceFromFile("PCA.py");
            try
            {
                Func<double[], double> verifyPython;
                if (!scriptScope.TryGetVariable<Func<double[], double>>("verifyPython", out verifyPython))
                {
                    return false;
                }

                double[] testValues = { 1, 4 };
                double result = verifyPython(testValues);

                return result == testValues[0] + testValues[1];
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }

    public class PeakIdentification
    {

    }
}
