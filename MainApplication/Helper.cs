using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using ImagingSIMS.Controls;
using ImagingSIMS.Controls.Tabs;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;
using ImagingSIMS.Data.Spectra;

namespace ImagingSIMS.MainApplication
{
    public static class Helper
    {
        public static T FindUpVisualTree<T>(DependencyObject initial) where T : DependencyObject
        {
            DependencyObject current = initial;

            while (current != null && current.GetType() != typeof(T))
            {
                current = VisualTreeHelper.GetParent(current);
            }
            return current as T;
        }
    }

    internal struct LoadMSArguments
    {
        string _fileName;
        public string FileName
        {
            get
            {
                if ((_fileName == null || _fileName == "") && (FileNames != null && FileNames.Length != 0))
                {
                    return FileNames[0];
                }
                return _fileName;
            }
            set
            {
                _fileName = value;
            }

        }
        public string[] FileNames;
        public SpectrumType Type;
        public int NumberFiles;
        public bool SaveQuickLoadFile;
    }

    public class BoolVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            string v = value.ToString();
            if (v == "true" || v == "True" || v == "false" || v == "False")
            {
                if (v == "true" || v == "True") return Visibility.Visible;
                else return Visibility.Collapsed;
            }
            if ((bool)value)
            {
                return Visibility.Visible;
            }
            else return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Visible)
            {
                return true;
            }
            else return false;
        }
    }
    public class ShortPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            string v = (string)value;
            if (v == null) return String.Empty;

            return System.IO.Path.GetFileName(v);
        }
        public object ConvertBack(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            return String.Empty;
        }
    }
    public class SpecCanNavigateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            if (value == null || argument == null) return false;
            ClosableTabItem cti = (ClosableTabItem)value;
            if (cti == null) return false;

            TabType tab = cti.TabType;
            if (cti.TabType != TabType.Spectrum) return false;

            SpectrumTab st = (SpectrumTab)cti.Content;
            if (st == null) return false;

            string arg = (string)argument;

            if (arg == "Back")
            {
                return st.CanHistoryBack;
            }
            else if (arg == "Forward")
            {
                return st.CanHistoryForward;
            }
            else if (arg == "Reset")
            {
                return true;
            }
            else return false;
        }
        public object ConvertBack(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }
    public class HasItemsToThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            if (value == null) return new Thickness(0);

            try
            {
                bool hasItems = (bool)value;

                if (hasItems)
                {
                    return new Thickness(0, 2, 0, 0);
                }
                else return new Thickness(0);
            }
            catch(Exception)
            {
                return new Thickness(0);
            }
        }
        public object ConvertBack(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }

    public class Data2DToBitmapSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;

            try
            {
                Data2D d = (Data2D)value;
                if (d == null) return null;

                return ImageHelper.CreateColorScaleImage(d, ColorScaleTypes.ThermalWarm);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }

    public class Data2DToSizeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;

            try
            {
                Data2D d = (Data2D)value;
                if (d == null) return null;

                return string.Format("{0} x {1}", d.Width, d.Height);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }

    public static class TitleBuilder
    {
        public static string Create(string[] Params, char Delimiter)
        {
            string result = String.Empty;

            for (int i = 0; i < Params.Length - 1; i++)
            {
                result += Params[i];
                result += Delimiter;
            }

            result += Params[Params.Length - 1];

            return result;
        }
        public static string Create(string[] Params, char Delimiter, int MaxCharacters)
        {
            string result = Create(Params, Delimiter);
            return Shorten(result, MaxCharacters);
        }

        private static string Shorten(string title, int chars)
        {
            string result = String.Empty;

            if (title.Length <= chars) result = title;
            else
            {
                int left = chars - 6;
                for (int i = 0; i < left; i++)
                {
                    result += title[i];
                }
                result += "...";
                for (int i = 0; i < 3; i++)
                {
                    int index = title.Length - 3 + i;
                    result += title[index];
                }
            }

            return result;
        }
    }
}
