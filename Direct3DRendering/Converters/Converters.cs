using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ImagingSIMS.Common.Controls;
using SharpDX;

using Color = SharpDX.Color;

namespace Direct3DRendering.Converters
{
    public class BoolVisInvertedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Windows.Visibility.Collapsed;
        }
    }
    public class RenderTypeToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                RenderType r = (RenderType)value;

                string p = (string)parameter;
                string[] parameters = p.Split('|');

                foreach (var renderType in parameters)
                {
                    if (r.ToString() == renderType)
                        return true;
                }

                return false;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class RenderTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                RenderType r = (RenderType)value;

                string p = (string)parameter;
                string[] parameters = p.Split('|');

                foreach (var renderType in parameters)
                {
                    if (r.ToString() == renderType)
                        return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
            catch (InvalidCastException)
            {
                return Visibility.Collapsed;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class Vector3ToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null) return "";

                Vector3 v = (Vector3)value;

                string p = (string)parameter;

                if (p == null || p == "Position")
                {
                    return string.Format("Camera Position: ({0}, {1}, {2})",
                        v.X.ToString("0.000"), v.Y.ToString("0.000"), v.Z.ToString("0.000"));
                }
                else if (p == "Direction")
                {
                    return string.Format("Camera Direction: ({0}, {1}, {2})",
                           v.X.ToString("0.000"), v.Y.ToString("0.000"), v.Z.ToString("0.000"));
                }
                else if (p == "Up")
                {
                    return string.Format("Camera Up: ({0}, {1}, {2})",
                           v.X.ToString("0.000"), v.Y.ToString("0.000"), v.Z.ToString("0.000"));
                }
                return null;

            }
            catch (InvalidCastException)
            {
                return "";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class FPSToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double d = (double)value;

                return string.Format("FPS: {0}", d.ToString("0"));

            }
            catch (InvalidCastException)
            {
                return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class TrainglesToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                int v = (int)value;

                return $"Number Triangles: {v}";
            }
            catch (Exception)
            {
                return "Number Triangles: {?}";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class VolumeToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Renderer renderer = value as Renderer;
                if (renderer == null) return false;

                if (renderer.RenderType != RenderType.Volume) return false;

                int index = int.Parse((string)parameter);

                VolumeRenderer volumeRenderer = (VolumeRenderer)renderer;
                int numVolumes = (int)volumeRenderer.VolumeParams.NumVolumes;

                return (index + 1) <= numVolumes;

            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class VolumeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Windows.Media.Color c = System.Windows.Media.Color.FromArgb(255, 128, 128, 128);
            try
            {
                if (value == null) return new System.Windows.Media.SolidColorBrush(c);

                Renderer renderer = value as Renderer;
                if (renderer == null) return new System.Windows.Media.SolidColorBrush(c);

                if (renderer.RenderType != RenderType.Volume) return new System.Windows.Media.SolidColorBrush(c);

                int index = int.Parse((string)parameter);

                VolumeRenderer volumeRenderer = (VolumeRenderer)renderer;

                SharpDX.Vector4 color = volumeRenderer.VolumeParams.GetColor(index);

                c = System.Windows.Media.Color.FromArgb((byte)(color.W * 255f),
                    (byte)(color.X * 255f), (byte)(color.Y * 255f), (byte)(color.Z * 255f));

                return new System.Windows.Media.SolidColorBrush(c);

            }
            catch (InvalidCastException)
            {
                return new System.Windows.Media.SolidColorBrush(c);
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class RenderObjectToVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Renderer renderer = value as Renderer;
                if (renderer == null) return false;

                int index = int.Parse((string)parameter);

                if (renderer.RenderType == RenderType.Isosurface)
                {
                    IsosurfaceRenderer isoRenderer = (IsosurfaceRenderer)renderer;
                    int numVolumes = (int)isoRenderer.IsosurfaceParams.NumberIsosurfaces;

                    if (index + 1 <= numVolumes)
                        return Visibility.Visible;

                    else return Visibility.Collapsed;
                }

                else if (renderer.RenderType == RenderType.Volume)
                {
                    VolumeRenderer volumeRenderer = (VolumeRenderer)renderer;
                    int numVolumes = (int)volumeRenderer.VolumeParams.NumVolumes;

                    if (index + 1 <= numVolumes)
                        return Visibility.Visible;

                    else return Visibility.Collapsed;
                }

                else return Visibility.Collapsed;

            }
            catch (InvalidCastException)
            {
                return Visibility.Collapsed;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class IsosurfaceToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Windows.Media.Color c = System.Windows.Media.Color.FromArgb(255, 128, 128, 128);
            try
            {
                if (value == null) return new System.Windows.Media.SolidColorBrush(c);

                Renderer renderer = value as Renderer;
                if (renderer == null) return new System.Windows.Media.SolidColorBrush(c);

                if (renderer.RenderType != RenderType.Isosurface) return new System.Windows.Media.SolidColorBrush(c);

                int index = int.Parse((string)parameter);

                IsosurfaceRenderer isoRenderer = (IsosurfaceRenderer)renderer;

                Vector4 color = isoRenderer.IsosurfaceParams.GetColor(index);

                c = System.Windows.Media.Color.FromArgb((byte)(color.W * 255f),
                    (byte)(color.X * 255f), (byte)(color.Y * 255f), (byte)(color.Z * 255f));

                return new System.Windows.Media.SolidColorBrush(c);

            }
            catch (InvalidCastException)
            {
                return new System.Windows.Media.SolidColorBrush(c);
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SharpDXColorToMediaColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                SharpDX.Color sharpdx = (SharpDX.Color)value;
                System.Windows.Media.Color media = System.Windows.Media.Color.FromArgb(sharpdx.A, sharpdx.R, sharpdx.G, sharpdx.B);
                return media;
            }
            catch (InvalidCastException)
            {
                return System.Windows.Media.Color.FromArgb(255, 0, 0, 0);
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                System.Windows.Media.Color media = (System.Windows.Media.Color)value;
                SharpDX.Color sharpdx = new SharpDX.Color(media.R, media.G, media.B, media.A);
                return sharpdx;
            }
            catch (InvalidCastException)
            {
                return new SharpDX.Color(0, 0, 0, 255);
            }
        }
    }
    public class SharpDXColorToNotifiableColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Color c = (Color)value;

                return ImagingSIMS.Common.Controls.NotifiableColor.FromArgb(c.A, c.R, c.G, c.B);
            }
            catch (Exception)
            {
                return ImagingSIMS.Common.Controls.NotifiableColor.FromArgb(255, 0, 0, 0);
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                ImagingSIMS.Common.Controls.NotifiableColor c =
                    (ImagingSIMS.Common.Controls.NotifiableColor)value;

                return new Color(c.R, c.G, c.B, c.A);
            }
            catch (Exception)
            {
                return new Color(0, 0, 0, 255);
            }
        }
    }
    public class NotifiableColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                NotifiableColor c = (NotifiableColor)value;

                return new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B));
            }
            catch (Exception)
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                System.Windows.Media.SolidColorBrush scb =
                    (System.Windows.Media.SolidColorBrush)value;

                return NotifiableColor.FromArgb(scb.Color.A, scb.Color.R, scb.Color.G, scb.Color.B);
            }

            catch (Exception)
            {
                return NotifiableColor.FromArgb(0, 0, 0, 0);
            }
        }
    }
    public class ArrayToIndexedValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Array array = value as Array;
                if (array == null) return null;

                int index = (int)parameter;

                if (index < 0 || index >= array.Length) return null;

                return array.GetValue(index);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool b = (bool)value;

                if (b)
                    return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 33, 255, 33));
                else return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 33, 33));
            }
            catch (Exception)
            {
                return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
            }           
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
