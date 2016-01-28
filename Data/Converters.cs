using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagingSIMS.Data.Converters
{
    public static class ColorConversion
    {
        public static double[] RGBtoHSL(double R, double G, double B)
        {
            double r = R / 255d;
            double g = G / 255d;
            double b = B / 255d;

            double min = Math.Min(r, g);
            min = Math.Min(min, b);
            double max = Math.Max(r, g);
            max = Math.Max(max, b);

            double l = (min + max) / 2d;             //[0, 1)

            double chroma = max - min;
            double s;                               //[0, 1)
            if (chroma == 0)
                s = 0;
            else
            {
                if (l < 0.5d)
                {
                    s = chroma / (max + min);
                }
                else
                {
                    s = chroma / (2d - max - min);
                }
            }

            double h = 0;                               //[0, 360)
            if (max == r)
            {
                h = (g - b) / chroma;
            }
            else if (max == g)
            {
                h = 2d + (b - r) / chroma;
            }
            else if (max == b)
            {
                h = 4d + (r - g) / chroma;
            }

            h *= 60d;
            if (h < 0) h += 360d;

            return new double[] { h, s, l };
        }
        public static double[] HSLtoRGB(double H, double S, double L)
        {
            double t1 = 0;

            if (L < 0.5d)
                t1 = L * (1d + S);
            else t1 = L + S - (L * S);

            double t2 = 2d * L - t1;

            double h = H / 360d;

            double tR = h + 0.333d;
            double tG = h;
            double tB = h - 0.333d;

            if (tR < 0) tR += 1d;
            else if (tR > 1) tR -= 1d;

            if (tG < 0) tG += 1d;
            else if (tG > 1) tG -= 1d;

            if (tB < 0) tB += 1d;
            else if (tB > 1) tB -= 1d;

            double r = 0;
            double g = 0;
            double b = 0;

            // Red
            if (tR * 6d < 1d)
            {
                r = t2 + (t1 - t2) * 6d * tR;
            }
            else if (tR * 2d < 1d)
            {
                r = t1;
            }
            else if (tR * 3d < 2d)
            {
                r = t2 + (t1 - t2) * (0.666d - tR) * 6d;
            }
            else r = t2;

            // Green
            if (tG * 6d < 1d)
            {
                g = t2 + (t1 - t2) * 6d * tG;
            }
            else if (tG * 2d < 1d)
            {
                g = t1;
            }
            else if (tG * 3d < 2d)
            {
                g = t2 + (t1 - t2) * (0.666d - tG) * 6d;
            }
            else g = t2;

            // Blue
            if (tB * 6d < 1d)
            {
                b = t2 + (t1 - t2) * 6d * tB;
            }
            else if (tB * 2d < 1d)
            {
                b = t1;
            }
            else if (tB * 3d < 2d)
            {
                b = t2 + (t1 - t2) * (0.666d - tB) * 6d;
            }
            else b = t2;

            r *= 255d;
            g *= 255d;
            b *= 255d;

            return new double[] { r, g, b };
        }

        //public static double[] RGBtoHSL(double R, double G, double B)
        //{
        //    double max = Math.Max(R, G);
        //    max = Math.Max(max, B);
        //    double min = Math.Min(R, G);
        //    min = Math.Min(min, B);

        //    double chroma = max - min;
        //    double h1;                               //[0,6)
        //    double h;                                //[0,360)
        //    if (chroma == 0)
        //    {
        //        h1 = 0;
        //    }
        //    else if (max == R)
        //    {
        //        //h1 = ((G - B) / chroma) % 6f;
        //        h1 = ((G - B) / chroma);
        //    }
        //    else if (max == G)
        //    {
        //        h1 = ((B - R) / chroma) + 2f;
        //    }
        //    else
        //    {
        //        h1 = ((R - G) / chroma) + 4f;
        //    }
        //    h = h1 * 60f;

        //    double l = (max + min) / (2f * 255f);    //[0,1)

        //    double s;                                //[0,1)
        //    if (chroma == 0)
        //    {
        //        s = 0;
        //    }
        //    else
        //    {
        //        s = (chroma / (1f - Math.Abs((2f * l) - 1f)) / 255f);
        //        if (s > 1f && s < 1.01f) s = 1f;
        //    }

        //    double[] returnValues = new double[3] { h, s, l};

        //    return returnValues;
        //}
        //public static double[] HSLtoRGB(double H, double S, double L)
        //{
        //    double chroma = (1f - Math.Abs((2f * L) - 1f)) * S;

        //    double h1 = H / 60f;

        //    double x = chroma * (1f - Math.Abs((h1 % 2f) - 1f));

        //    double r1 = 0;
        //    double g1 = 0;
        //    double b1 = 0;
        //    if (double.IsNaN(H))
        //    {
        //        r1 = 0;
        //        g1 = 0;
        //        b1 = 0;
        //    }
        //    else if (h1 >= 0 && h1 < 1)
        //    {
        //        r1 = chroma;
        //        g1 = x;
        //        b1 = 0;
        //    }
        //    else if (h1 >= 1 && h1 < 2)
        //    {
        //        r1 = x;
        //        g1 = chroma;
        //        b1 = 0;
        //    }
        //    else if (h1 >= 2 && h1 < 3)
        //    {
        //        r1 = 0;
        //        g1 = chroma;
        //        b1 = x;
        //    }
        //    else if (h1 >= 3 && h1 < 4)
        //    {
        //        r1 = 0;
        //        g1 = x;
        //        b1 = chroma;
        //    }
        //    else if (h1 >= 4 && h1 < 5)
        //    {
        //        r1 = x;
        //        g1 = 0;
        //        b1 = chroma;
        //    }
        //    else if (h1 >= 5 && h1 < 6)
        //    {
        //        r1 = chroma;
        //        g1 = 0;
        //        b1 = x;
        //    }

        //    double m = L - (chroma / 2f);

        //    double r = r1 + m;
        //    double g = g1 + m;
        //    double b = b1 + m;

        //    double[] returnValues = new double[3] { r * 255f, g * 255f, b * 255f };

        //    return returnValues;
        //}

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
