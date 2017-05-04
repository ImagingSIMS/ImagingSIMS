using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class LinearRegression
    {
        double[,] _v;
        double[] _c;
        double[] _sec;
        double _rysq;
        double _sdv;
        double _fReg;
        double[] _yCalc;
        double[] _dY;

        public double FisherF { get { return _fReg; } }
        public double CorrelationCoefficient { get { return _rysq; } }
        public double StandardDeviation { get { return _sdv; } }
        public double[] CalculatedValues { get { return _yCalc; } }
        public double[] Residuals { get { return _dY; } }
        public double[] Coefficients { get { return _c; } }
        public double[] CoefficientsStandardError { get { return _sec; } }
        public double[,] VarianceMatrix { get { return _v; } }

        public bool Regress(double[] y, double[,] x, double[] w)
        {
            // y[j]     j-th observed data point
            // x[i,j]   j-th value of the ith independent variable
            // w[h]     j-th weight value

            int m = y.Length;
            int n = x.Length / m;
            int ndf = m - n;

            _yCalc = new double[m];
            _dY = new double[m];

            if (ndf < 1) return false;

            _v = new double[n, n];
            _c = new double[n];
            _sec = new double[n];

            double[] b = new double[n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    _v[i, j] = 0;
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    _v[i, j] = 0;
                    for (int k = 0; k < m; k++)
                    {
                        _v[i, j] = _v[i, j] + w[k] * x[i, k] * x[j, k];
                    }
                    b[i] = 0;
                    for (int k = 0; k < m; k++)
                    {
                        b[i] = b[i] + w[k] * x[i, k] * y[k];
                    }
                }
            }

            if (!SymmetricMatrixInvert(_v)) return false;

            for (int i = 0; i < n; i++)
            {
                _c[i] = 0;
                for (int j = 0; j < n; j++)
                {
                    _c[i] = _c[i] + _v[i, j] * b[j];
                }
            }

            double tss = 0;
            double rss = 0;
            double ybar = 0;
            double wsum = 0;

            for (int k = 0; k < m; k++)
            {
                ybar = ybar + w[k] * y[k];
                wsum = wsum + w[k];
            }
            ybar = ybar / wsum;

            for (int k = 0; k < m; k++)
            {
                _yCalc[k] = 0;
                for (int i = 0; i < n; i++)
                {
                    _yCalc[k] = _yCalc[k] + _c[i] * x[i, k];
                }
                _dY[k] = _yCalc[k] - y[k];
                tss = tss + w[k] * (y[k] - ybar) * (y[k] - ybar);
                rss = rss + w[k] * _dY[k] * _dY[k];
            }

            double ssq = rss / ndf;
            _rysq = 1 - rss / tss;
            _fReg = 9999999;
            if (_rysq < 0.9999999)
            {
                _fReg = _rysq / (1 - _rysq) * ndf / (n - 1);
            }
            _sdv = Math.Sqrt(ssq);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    _v[i, j] = _v[i, j] * ssq;
                    _sec[i] = Math.Sqrt(_v[i, i]);
                }
            }

            return true;
        }

        public bool SymmetricMatrixInvert(double[,] v)
        {
            int n = (int)Math.Sqrt(v.Length);
            double[] t = new double[n];
            double[] q = new double[n];
            double[] r = new double[n];
            double ab = 0;
            int k = 0;
            int l = 0;
            int m = 0;

            for (m = 0; m < n; m++)
            {
                r[m] = 1;
            }
            for (m = 0; m < n; m++)
            {
                double big = 0;
                for (l = 0; l < n; l++)
                {
                    ab = Math.Abs(v[l, l]);
                    if ((ab > big) && (r[l] != 0))
                    {
                        big = ab;
                        k = l;
                    }
                }
                if (big == 0) return false;

                r[k] = 0;
                q[k] = 1 / v[k, k];
                t[k] = 1;
                v[k, k] = 0;
                if (k != 0)
                {
                    for (l = 0; l < k; l++)
                    {
                        t[l] = v[l, k];
                        if (r[l] == 0)
                            q[l] = v[l, k] * q[k];
                        else
                            q[l] = -v[l, k] * q[k];
                        v[l, k] = 0;
                    }
                }
                if ((k + 1) < n)
                {
                    for (l = k + 1; l < n; l++)
                    {
                        if (r[l] != 0)
                            t[l] = v[k, l];
                        else
                            t[l] = -v[k, l];
                        q[l] = -v[k, l] * q[k];
                        v[k, l] = 0;
                    }
                }
                for (l = 0; l < n; l++)
                {
                    for (k = l; k < n; k++)
                    {
                        v[l, k] = v[l, k] + t[l] * q[k];
                    }
                }
            }
            m = n;
            l = n - 1;
            for (k = 1; k < n; k++)
            {
                m = m - 1;
                l = l - 1;
                for (int j = 0; j <= l; j++)
                {
                    v[m, j] = v[j, m];
                }

            }
            return true;
        }
    }
}
