using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;
using ImagingSIMS.Data.ClusterIdentification;
using ImagingSIMS.Controls.Tabs;
using System.Windows;
using ImagingSIMS.Data.Fusion;

namespace ImagingSIMS.Controls.Converters
{
    public class BoolToScrollTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                bool input = (bool)value;

                if (input)
                {
                    return "Disable Adjustment";
                }
                else
                {
                    return "Enable Adjustment";
                }
            }
            catch (InvalidCastException ICex)
            {
                string inner = "Please try again.";
                if (ICex.InnerException != null)
                {
                    inner = ICex.InnerException.Message;
                }
                DialogBox db = new DialogBox(ICex.Message, inner,
                    "Bitmap Display", DialogIcon.Error);
                db.ShowDialog();
                return "Error in operation";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                string input = value.ToString();
                if (input == null)
                {
                    throw new InvalidCastException("The input argument cannot be converted to string.");
                }

                if (input == "Disable Adjustment")
                {
                    return true;
                }
                else if (input == "Enable Adjustment")
                {
                    return false;
                }
                else
                {
                    return false;
                }
            }
            catch (InvalidCastException ICex)
            {
                string inner = "Please try again.";
                if (ICex.InnerException != null)
                {
                    inner = ICex.InnerException.Message;
                }
                DialogBox db = new DialogBox(ICex.Message, inner,
                    "Bitmap Display", DialogIcon.Error);
                db.ShowDialog();
                return false;
            }
        }
    }

    public class TabToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
               System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null || parameter == null) return false;

                ClosableTabItem cti = (ClosableTabItem)value;
                TabType tab = cti.TabType;
                string p = (string)parameter;

                char delim = '|';
                string[] parts = p.Split(delim);

                foreach (string s in parts)
                {
                    TabType param = (TabType)Enum.Parse(typeof(TabType), s);
                    if (param == tab) return true;
                }
                return false;                
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }
    public class HasImageToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
               System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null) return false;

                System.Windows.Media.Imaging.BitmapSource source = (System.Windows.Media.Imaging.BitmapSource)value;
                return value != null;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }
    public class BoolVisInvertedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
               System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null) return false;

                bool b = (bool)value;

                if (b)
                {
                    return System.Windows.Visibility.Collapsed;
                }

                else return System.Windows.Visibility.Visible;
            }
            catch (InvalidCastException)
            {
                return System.Windows.Visibility.Collapsed;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return System.Windows.Visibility.Collapsed;
        }
    }

    public class HasTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
               System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null) return false;
                
                string s = (string)value;
                if (s == null) return false;

                if (s == "") return false;
                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
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

    public class Data3DToBitmapSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;

            try
            {
                Data3D d = (Data3D)value;
                if (d == null) return null;

                if (d.Depth != 4) return null;

                return ImageHelper.CreateImage(d);
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

    public class BoolInvertConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
               System.Globalization.CultureInfo culture)
        {
            try
            {
                bool b = (bool)value;

                return !b;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
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
    public class Data2DToMinMaxStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;

            try
            {
                Data2D d = (Data2D)value;
                if (d == null) return null;

                return string.Format("Minimum: {0} Maximum: {1}", d.Minimum, d.Maximum);
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
    public class Data3DToSizeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;

            try
            {
                Data3D d = (Data3D)value;
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

    public sealed class EnumDescriptionConverter : IValueConverter
    {
        private string GetEnumDescription(Enum enumObj)
        {
            System.Reflection.FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

            if (fieldInfo == null) return enumObj.ToString();

            object[] attribArray = fieldInfo.GetCustomAttributes(false);

            if (attribArray == null || attribArray.Length == 0)
            {
                return enumObj.ToString();
            }
            else
            {
                foreach (object attrib in attribArray)
                {
                    System.ComponentModel.DescriptionAttribute desc = attrib as System.ComponentModel.DescriptionAttribute;
                    if (desc == null) continue;

                    return desc.Description;
                }

                return enumObj.ToString();
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Enum myEnum = (Enum)value;
            string description = GetEnumDescription(myEnum);
            return description;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Empty;
        }
    }

    public class DisplayImageToDimensionsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;

            try
            {
                DisplayImage i = (DisplayImage)value;
                if (i == null) return null;

                System.Windows.Media.Imaging.BitmapSource src = i.Source as System.Windows.Media.Imaging.BitmapSource;
                if (src == null) return null;

                return string.Format("{0} x {1}", src.PixelWidth.ToString("0"), src.PixelHeight.ToString("0"));
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
    public class BitmapSourceToDimensionsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;

            try
            {
                System.Windows.Media.Imaging.BitmapSource bs = (System.Windows.Media.Imaging.BitmapSource)value;
                if (bs == null) return null;

                return string.Format("{0} x {1}", bs.PixelWidth.ToString("0"),  bs.PixelHeight.ToString("0"));
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
    public class RegTypeToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;

            try
            {
                var regType = ((ImageRegistration.ImageRegistrationTypes)value).ToString();

                string[] validTypes = ((string)argument).Split('|');
                if (validTypes == null) return false;
                
                foreach (string s in validTypes)
                {
                    if (regType.Contains(s)) 
                        return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public object ConvertBack(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }
    public class FoundClustersToMaskImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;

            try
            {
                FoundClusters foundClusters = value as FoundClusters;
                if (foundClusters == null) return null;

                bool[,] mask = foundClusters.MaskArray;
                return ImageHelper.CreateColorScaleImage((Data2D)mask, ColorScaleTypes.Gray);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class FoundClustersToColorMaskImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;

            try
            {
                FoundClusters foundClusters = value as FoundClusters;
                if (foundClusters == null) return null;

                Data3D d = foundClusters.ColorMask;
                return ImageHelper.CreateImage(d);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class ColorScaleToBitmapSourceConverter : IMultiValueConverter
    {
        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // [0]:  (ColorScaleTypes)colorScale
            // [1]:  (Color)solidColorScale
            // [2]:  (float)dataMaximum
            // [3]:  (int)saturation
            try
            {
                ColorScaleTypes colorScale = (ColorScaleTypes)values[0];
                System.Windows.Media.Color solidColorScale = (System.Windows.Media.Color)values[1];
                float dataMaximum = (float)values[2];
                double saturation = (double)values[3];

                // Using dimensions 200 x 20 pixels to match the size of the image control
                Data2D d = new Data2D(200, 20);
                for (int i = 0; i < 200; i++)
                {
                    float value = dataMaximum * i / 200f;
                    for (int j = 0; j < 20; j++)
                    {
                        d[i, j] = value;
                    }
                }

                if (colorScale == ColorScaleTypes.Solid)
                {
                    return ImageHelper.CreateSolidColorImage(d, solidColorScale, (float)saturation);
                }
                else
                    return ImageHelper.CreateColorScaleImage(d, colorScale, (float)saturation);
            }
            catch (Exception)
            {
                return null;
            }
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DataMaximumToSliderRangeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                float maximum = (float)value;
                string param = (string)parameter;
                param = param.ToLower();

                bool isLargeChange = false;
                bool isSmallChange = false;
                bool isMaximum = false;
                bool isMinimum = false;

                switch (param)
                {
                    case "large":
                        isLargeChange = true;
                        break;
                    case "small":
                        isSmallChange = true;
                        break;
                    case "maximum":
                        isMaximum = true;
                        break;
                    case "minimum":
                        isMinimum = true;
                        break;
                }

                // Check maximum and set interval appropriately
                if(maximum > 100)
                {
                    if (isLargeChange) return 25;
                    else if (isSmallChange) return 1;
                    else if (isMaximum) return maximum * 2.5;
                    else if (isMinimum) return 1;
                }
                if(maximum > 10)
                {
                    if (isLargeChange) return 2;
                    else if (isSmallChange) return 0.5;
                    else if (isMaximum) return maximum * 2.5;
                    else if (isMinimum) return 1;
                }
                if(maximum > 1)
                {
                    if (isLargeChange) return 0.1f;
                    else if (isSmallChange) return 0.01f;
                    else if (isMaximum) return maximum * 2.5;
                    else if (isMinimum) return 1;
                }
                else
                {
                    if (isLargeChange) return 0.01f;
                    else if (isSmallChange) return 0.001f;
                    else if (isMaximum) return maximum * 2.5;
                    else if (isMinimum) return 0.00001f;
                }

                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
    public class NumberClustersToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FoundClusters foundClusters = value as FoundClusters;

            if (foundClusters == null) return false;

            return foundClusters.NumberClusters > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorScaleToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
               System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null) return false;
                ColorScaleTypes c = (ColorScaleTypes)value;

                return c == ColorScaleTypes.Solid;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }
    public class MaskToImageSourceConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Data2D d = value as Data2D;
            if (d == null) return null;

            return ImageHelper.CreateColorScaleImage(d, ColorScaleTypes.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
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

    public class FusionEnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;

            try
            {
                FusionType ft = (FusionType)value;
                if (ft == FusionType.HSLShift) return Visibility.Visible;
                else return Visibility.Collapsed;
            }
            catch (Exception)
            {
                return Visibility.Collapsed;
            }
        }
        public object ConvertBack(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
