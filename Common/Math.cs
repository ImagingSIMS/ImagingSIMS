using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagingSIMS.Common.Math
{
    public static class Percentage
    {
        public static int GetPercent(int Numerator, int Denominator)
        {
            return (int)(((double)Numerator * 100d) / (double)Denominator);
        }
        public static int GetPercent(double Numerator, int Denominator)
        {
            return (int)((Numerator * 100d) / (double)Denominator);
        }
        public static int GetPercent(int Numerator, double Denominator)
        {
            return (int)(((double)Numerator * 100d) / Denominator);
        }
        public static int GetPercent(double Numerator, double Denominator)
        {
            return (int)((Numerator * 100d) / Denominator);
        }
        public static int GetPercent(float Numerator, int Denominator)
        {
            return (int)((Numerator * 100f) / (float)Denominator);
        }
        public static int GetPercent(int Numerator, float Denominator)
        {
            return (int)(((float)Numerator * 100f) / Denominator);
        }
        public static int GetPercent(float Numerator, float Denominator)
        {
            return (int)((Numerator * 100f) / Denominator);
        }
        public static int GetPercent(long Numerator, long Denominator)
        {
            return (int)((Numerator * 100L) / Denominator);
        }
    }
    public static class MathEx
    {
        public static byte Average(params byte[] args)
        {
            double sum = 0;
            foreach (byte b in args)
            {
                sum += (double)b;
            }
            return (byte)(sum / (double)args.Length);
        }
        public static float Average(params float[] args)
        {
            double sum = 0;
            foreach (float f in args)
            {
                sum += (double)f;
            }
            return (float)(sum / (double)args.Length);
        }
        public static double Average(params double[] args)
        {
            double sum = 0;
            foreach (double d in args)
            {
                sum += (double)d;
            }
            return (double)(sum / (double)args.Length);
        }
        public static double Average(List<double> args)
        {
            double sum = 0;
            foreach (double d in args)
            {
                sum += (double)d;
            }
            return (double)(sum / (double)args.Count);
        }
        public static float[,] Covariance(float[] v1, float[] v2)
        {
            if (v1.Length != v2.Length)
                throw new ArgumentException("Vectors are not the same size.");

            float meanV1 = 0;
            float meanV2 = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                meanV1 += v1[i];
                meanV2 += v2[i];
            }

            meanV1 /= v1.Length;
            meanV2 /= v2.Length;

            float[,] covariance = new float[v1.Length, v1.Length];
            for (int x = 0; x < v1.Length; x++)
            {
                for (int y = 0; y < v1.Length; y++)
                {
                    covariance[x, y] = (v1[x] - meanV1) * (v2[y] - meanV2);
                }
            }
            return covariance;
        }
    }
}
