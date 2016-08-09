using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ImagingSIMS.Common.Controls.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string s = (string)value;

                if (!string.IsNullOrEmpty(s))
                    return Visibility.Visible;

                return Visibility.Collapsed;
            }
            catch (Exception)
            {
                return Visibility.Collapsed;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
