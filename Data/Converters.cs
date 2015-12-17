using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagingSIMS.Data.Converters
{
    internal static class ColorConversion
    {
        public static double[] RGBtoHSL(double R, double G, double B)
        {
            double max = Math.Max(R, G);
            max = Math.Max(max, B);
            double min = Math.Min(R, G);
            min = Math.Min(min, B);

            double chroma = max - min;
            double h1;                               //[0,6)
            double h;                                //[0,360)
            if (chroma == 0)
            {
                h1 = 0;
            }
            else if (max == R)
            {
                h1 = ((G - B) / chroma) % 6f;
            }
            else if (max == G)
            {
                h1 = ((B - R) / chroma) + 2f;
            }
            else
            {
                h1 = ((R - G) / chroma) + 4f;
            }
            h = h1 * 60f;

            double l = (max + min) / (2f * 255f);    //[0,1)

            double s;                                //[0,1)
            if (chroma == 0)
            {
                s = 0;
            }
            else
            {
                s = (chroma / (1f - Math.Abs((2f * l) - 1f)) / 255f);
                if (s > 1f && s < 1.01f) s = 1f;
            }

            double[] returnValues = new double[3] { h, s, l };

            return returnValues;
        }
        public static double[] HSLtoRGB(double H, double S, double L)
        {
            double chroma = (1f - Math.Abs((2f * L) - 1f)) * S;

            double h1 = H / 60f;

            double x = chroma * (1f - Math.Abs((h1 % 2f) - 1f));

            double r1 = 0;
            double g1 = 0;
            double b1 = 0;
            if (double.IsNaN(H))
            {
                r1 = 0;
                g1 = 0;
                b1 = 0;
            }
            else if (h1 >= 0 && h1 < 1)
            {
                r1 = chroma;
                g1 = x;
                b1 = 0;
            }
            else if (h1 >= 1 && h1 < 2)
            {
                r1 = x;
                g1 = chroma;
                b1 = 0;
            }
            else if (h1 >= 2 && h1 < 3)
            {
                r1 = 0;
                g1 = chroma;
                b1 = x;
            }
            else if (h1 >= 3 && h1 < 4)
            {
                r1 = 0;
                g1 = x;
                b1 = chroma;
            }
            else if (h1 >= 4 && h1 < 5)
            {
                r1 = x;
                g1 = 0;
                b1 = chroma;
            }
            else if (h1 >= 5 && h1 < 6)
            {
                r1 = chroma;
                g1 = 0;
                b1 = x;
            }

            double m = L - (chroma / 2f);

            double r = r1 + m;
            double g = g1 + m;
            double b = b1 + m;

            double[] returnValues = new double[3] { r * 255f, g * 255f, b * 255f };

            return returnValues;
        }

        public static double[] RGBtoIHS(double R, double G, double B)
        {
            double max = Math.Max(R, G);
            max = Math.Max(max, B);
            double min = Math.Min(R, G);
            min = Math.Min(min, B);

            double chroma = max - min;
            double h1;                               //[0,6)
            double h;                                //[0,360)
            if (chroma == 0)
            {
                h1 = 0;
            }
            else if (max == R)
            {
                h1 = ((G - B) / chroma) % 6f;
            }
            else if (max == G)
            {
                h1 = ((B - R) / chroma) + 2f;
            }
            else
            {
                h1 = ((R - G) / chroma) + 4f;
            }
            h = h1 * 60f;

            double i = (R + G + B) / (3f * 255f); ;    //[0,1)

            double s;                                //[0,1)
            if (chroma == 0)
            {
                s = 0;
            }
            else
            {
                s = (chroma / (1f - Math.Abs((2f * i) - 1f)) / 255f);
                if (s > 1f && s < 1.01f) s = 1f;
            }

            double[] returnValues = new double[3] { h, s, i };

            return returnValues;
        }
        public static double[] IHStoRGB(double H, double S, double I)
        {
            double chroma = (1f - Math.Abs((2f * I) - 1f)) * S;

            double h1 = H / 60f;

            double x = chroma * (1f - Math.Abs((h1 % 2f) - 1f));

            double r1 = 0;
            double g1 = 0;
            double b1 = 0;
            if (double.IsNaN(H))
            {
                r1 = 0;
                g1 = 0;
                b1 = 0;
            }
            else if (h1 >= 0 && h1 < 1)
            {
                r1 = chroma;
                g1 = x;
                b1 = 0;
            }
            else if (h1 >= 1 && h1 < 2)
            {
                r1 = x;
                g1 = chroma;
                b1 = 0;
            }
            else if (h1 >= 2 && h1 < 3)
            {
                r1 = 0;
                g1 = chroma;
                b1 = x;
            }
            else if (h1 >= 3 && h1 < 4)
            {
                r1 = 0;
                g1 = x;
                b1 = chroma;
            }
            else if (h1 >= 4 && h1 < 5)
            {
                r1 = x;
                g1 = 0;
                b1 = chroma;
            }
            else if (h1 >= 5 && h1 < 6)
            {
                r1 = chroma;
                g1 = 0;
                b1 = x;
            }

            double m = I - (chroma / 2f);

            double r = r1 + m;
            double g = g1 + m;
            double b = b1 + m;

            double[] returnValues = new double[3] { r * 255f, g * 255f, b * 255f };

            return returnValues;
        }
    }
}
