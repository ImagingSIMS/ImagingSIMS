using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ImagingSIMS.Data
{
    public static class PointSet
    {
        public static void PointSetToFile(ObservableCollection<Point> points, string filename)
        {
            char delim = ' ';

            using (var fileStream = new System.IO.FileStream(filename, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fileStream))
                {
                    foreach (Point p in points)
                    {
                        float x = (float)p.X;
                        float y = (float)p.Y;

                        sw.Write(x);
                        sw.Write(delim);
                        sw.Write(y);
                        sw.Write(delim);
                    }
                }
            }
        }
        public static ObservableCollection<Point> PointSetFromFile(string filename)
        {
            if (!File.Exists(filename)) throw new FileNotFoundException(Path.GetFileName(filename));

            string readin = "";
            using (StreamReader sr = new StreamReader(filename))
            {
                readin = sr.ReadToEnd();
            }

            ObservableCollection<Point> points = new ObservableCollection<Point>();
            string[] parts = readin.Split(' ');

            for (int i = 0; i < parts.Length / 2; i++)
            {
                int x = 2 * i;
                int y = 2 * i + 1;

                double X = double.Parse(parts[x]);
                double Y = double.Parse(parts[y]);

                points.Add(new Point(X, Y));
            }

            return points;
        }
    }
}
