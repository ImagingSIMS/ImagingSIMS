using System;
using System.Collections.Generic;
using System.Text;

namespace ImagingSIMS.Data.Spectra
{
    public class MassRange : IComparable
    {
        public double StartMass;
        public double EndMass;

        public MassRange()
        {

        }
        public MassRange(double startMass, double endMass)
        {
            StartMass = startMass;
            EndMass = endMass;
        }
        public override string ToString()
        {
            return StartMass.ToString("0.000") + "-" + EndMass.ToString("0.000");
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj.GetType() != typeof(MassRange))
                throw new ArgumentException("Cannot compare different types.");

            MassRange m = (MassRange)obj;

            if (StartMass < m.StartMass) return -1;
            else if (StartMass == m.StartMass) return 0;
            else return 1;
        }

        public static List<MassRange> ParseString(string customRange, double binWidth)
        {
            List<MassRange> ranges = new List<MassRange>();

            // If empty string, return blank collection
            if (String.IsNullOrEmpty(customRange))
                return ranges;

            // Remove any spaces that may have been inadvertently entered
            string toParse = customRange;
            int index = toParse.IndexOf(' ');
            while (index != -1)
            {
                toParse = toParse.Remove(index, 1);
                index = toParse.IndexOf(' ');
            }

            string[] rangeString = toParse.Split(';');

            if (rangeString.Length == 0)
            {
                throw new ArgumentException("No string found.");
            }

            foreach (string s in rangeString)
            {
                var range = new MassRange();
                if (!s.Contains("-"))
                {
                    if (binWidth <= 0)
                    {
                        throw new ArgumentException("Centroid mass was specified with a bin width less than or equal to zero. Please enter a positive bin width.");
                    }
                    try
                    {
                        var centroid = double.Parse(s);
                        range.StartMass = centroid - binWidth;
                        range.EndMass = centroid + binWidth;
                    }
                    catch(Exception ex)
                    {
                        throw new ArgumentException($"Could not parse the specified centroid ({s}).", ex);

                    }
                }

                else
                {
                    string[] rangeValues = s.Split('-');

                    if (rangeValues.Length != 2)
                        throw new ArgumentException("Invalid number of masses in the specified range.");

                    try
                    {
                        range.StartMass = double.Parse(rangeValues[0]);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"Could not parse the specified start mass ({rangeValues[0]}).", ex);
                    }
                    try
                    {
                        range.EndMass = double.Parse(rangeValues[1]);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"Could not parse the specified end mass ({rangeValues[1]}).", ex);
                    }
                }

                ranges.Add(range);
            }

            return ranges;
        }
        public static string CreateString(List<MassRange> ranges)
        {
            StringBuilder sb = new StringBuilder();

            foreach (MassRange range in ranges)
            {
                sb.AppendFormat("{0}-{1};",
                    range.StartMass.ToString("0.000"), range.EndMass.ToString("0.000"));
            }

            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
    }
}
