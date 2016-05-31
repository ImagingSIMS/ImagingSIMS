using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using ImagingSIMS.Common.Controls;
using SharpDX;

namespace Direct3DRendering
{
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


    public enum RenderType
    {
        Volume, HeightMap, Isosurface, NotSpecified
    }
    public enum MoveDirection
    {
        Right, Left, Forward, Backward, Original, None
    }
    public enum RotateDirection
    {
        Right, Left, Up, Down, Reverse, Clockwise, CounterClockwise, None
    }

    [StructLayout(LayoutKind.Explicit, Size = 160)]
    public struct RenderParams
    {
        [FieldOffset(0)]
        public Matrix WorldProjView;

        [FieldOffset(64)]
        public Vector2 InvWindowSize;

        [FieldOffset(72)]
        public float Brightness;

        [FieldOffset(76)]
        public float ClipDistance;

        [FieldOffset(80)]
        public Vector3 CameraPositon;

        [FieldOffset(92)]
        private float padding0;

        [FieldOffset(96)]
        public Vector3 CameraDirection;

        [FieldOffset(108)]
        private float padding1;

        [FieldOffset(112)]
        public Vector3 CameraUp;

        [FieldOffset(124)]
        private float padding2;

        [FieldOffset(128)]
        public Vector4 NearClipPlane;

        [FieldOffset(144)]
        public Vector4 FarClipPlane;
    }

    [StructLayout(LayoutKind.Explicit, Size = 192)]
    public struct VolumeParams
    {
        [FieldOffset(0)]
        public Vector4 VolumeScaleStart;

        [FieldOffset(16)]
        private Vector4 VolumeColor0;

        [FieldOffset(32)]
        private Vector4 VolumeColor1;

        [FieldOffset(48)]
        private Vector4 VolumeColor2;

        [FieldOffset(64)]
        private Vector4 VolumeColor3;

        [FieldOffset(80)]
        private Vector4 VolumeColor4;

        [FieldOffset(96)]
        private Vector4 VolumeColor5;

        [FieldOffset(112)]
        private Vector4 VolumeColor6;

        [FieldOffset(128)]
        private Vector4 VolumeColor7;

        [FieldOffset(144)]
        public Vector4 VolumeScale;

        [FieldOffset(160)]
        public Vector4 VolumeScaleDenominator;

        [FieldOffset(176)]
        public uint NumVolumes;

        [FieldOffset(180)]
        private float padding0;

        [FieldOffset(184)]
        private float padding1;

        [FieldOffset(188)]
        private float padding2;

        public static VolumeParams Empty
        {
            get
            {
                VolumeParams v = new VolumeParams()
                {
                    NumVolumes = 0,
                    padding0 = 0,
                    padding1 = 0,
                    padding2 = 0,
                    VolumeScaleStart = Vector4.Zero,
                    VolumeColor0 = Vector4.Zero,
                    VolumeColor1 = Vector4.Zero,
                    VolumeColor2 = Vector4.Zero,
                    VolumeColor3 = Vector4.Zero,
                    VolumeColor4 = Vector4.Zero,
                    VolumeColor5 = Vector4.Zero,
                    VolumeColor6 = Vector4.Zero,
                    VolumeColor7 = Vector4.Zero,
                    VolumeScale = Vector4.Zero,
                    VolumeScaleDenominator = Vector4.Zero
                };
                return v;
            }
        }

        public void UpdateColor(int VolumeNumber, Vector4 Color)
        {
            switch (VolumeNumber)
            {
                case 0:
                    VolumeColor0 = Color;
                    break;
                case 1:
                    VolumeColor1 = Color;
                    break;
                case 2:
                    VolumeColor2 = Color;
                    break;
                case 3:
                    VolumeColor3 = Color;
                    break;
                case 4:
                    VolumeColor4 = Color;
                    break;
                case 5:
                    VolumeColor5 = Color;
                    break;
                case 6:
                    VolumeColor6 = Color;
                    break;
                case 7:
                    VolumeColor7 = Color;
                    break;
            }
        }
        public Vector4 GetColor(int VolumeNumber)
        {
            switch (VolumeNumber)
            {
                case 0:
                    return VolumeColor0;
                case 1:
                    return VolumeColor1;
                case 2:
                    return VolumeColor2;
                case 3:
                    return VolumeColor3;
                case 4:
                    return VolumeColor4;
                case 5:
                    return VolumeColor5;
                case 6:
                    return VolumeColor6;
                case 7:
                    return VolumeColor7;
            }
            return new Vector4(0);
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 144)]
    public struct IsosurfaceParams
    {
        [FieldOffset(0)]
        private Vector4 IsosurfaceColor0;

        [FieldOffset(16)]
        private Vector4 IsosurfaceColor1;

        [FieldOffset(32)]
        private Vector4 IsosurfaceColor2;

        [FieldOffset(48)]
        private Vector4 IsosurfaceColor3;

        [FieldOffset(64)]
        private Vector4 IsosurfaceColor4;

        [FieldOffset(80)]
        private Vector4 IsosurfaceColor5;

        [FieldOffset(96)]
        private Vector4 IsosurfaceColor6;

        [FieldOffset(112)]
        private Vector4 IsosurfaceColor7;

        [FieldOffset(128)]
        public float NumberIsosurfaces;

        [FieldOffset(132)]
        private float padding0;

        [FieldOffset(136)]
        private float padding1;

        [FieldOffset(140)]
        private float padding2;

        public static IsosurfaceParams Empty
        {
            get
            {
                return new IsosurfaceParams()
                {
                    IsosurfaceColor0 = new Vector4(0),
                    IsosurfaceColor1 = new Vector4(0),
                    IsosurfaceColor2 = new Vector4(0),
                    IsosurfaceColor3 = new Vector4(0),
                    IsosurfaceColor4 = new Vector4(0),
                    IsosurfaceColor5 = new Vector4(0),
                    IsosurfaceColor6 = new Vector4(0),
                    IsosurfaceColor7 = new Vector4(0),
                };
            }
        }

        public void UpdateColor(int IsosurfaceNumber, Vector4 Color)
        {
            switch (IsosurfaceNumber)
            {
                case 0:
                    IsosurfaceColor0 = Color;
                    break;
                case 1:
                    IsosurfaceColor1 = Color;
                    break;
                case 2:
                    IsosurfaceColor2 = Color;
                    break;
                case 3:
                    IsosurfaceColor3 = Color;
                    break;
                case 4:
                    IsosurfaceColor4 = Color;
                    break;
                case 5:
                    IsosurfaceColor5 = Color;
                    break;
                case 6:
                    IsosurfaceColor6 = Color;
                    break;
                case 7:
                    IsosurfaceColor7 = Color;
                    break;
            }
        }
        public Vector4 GetColor(int IsosurfaceNumber)
        {
            switch (IsosurfaceNumber)
            {
                case 0:
                    return IsosurfaceColor0;
                case 1:
                    return IsosurfaceColor1;
                case 2:
                    return IsosurfaceColor2;
                case 3:
                    return IsosurfaceColor3;
                case 4:
                    return IsosurfaceColor4;
                case 5:
                    return IsosurfaceColor5;
                case 6:
                    return IsosurfaceColor6;
                case 7:
                    return IsosurfaceColor7;
            }
            return new Vector4(0);
        }
        //public void UpdateValue(int IsosurfaceNumber, float Value)
        //{
        //    switch (IsosurfaceNumber)
        //    {
        //        case 0:
        //            IsosurfaceValue0 = Value;
        //            break;
        //        case 1:
        //            IsosurfaceValue1 = Value;
        //            break;
        //        case 2:
        //            IsosurfaceValue2 = Value;
        //            break;
        //        case 3:
        //            IsosurfaceValue3 = Value;
        //            break;
        //        case 4:
        //            IsosurfaceValue4 = Value;
        //            break;
        //        case 5:
        //            IsosurfaceValue5 = Value;
        //            break;
        //        case 6:
        //            IsosurfaceValue6 = Value;
        //            break;
        //        case 7:
        //            IsosurfaceValue7 = Value;
        //            break;
        //    }
        //}
        //public float GetValue(int IsosurfaceNumber)
        //{
        //    switch (IsosurfaceNumber)
        //    {
        //        case 0:
        //            return IsosurfaceValue0;
        //        case 1:
        //            return IsosurfaceValue1;
        //        case 2:
        //            return IsosurfaceValue2;
        //        case 3:
        //            return IsosurfaceValue3;
        //        case 4:
        //            return IsosurfaceValue4;
        //        case 5:
        //            return IsosurfaceValue5;
        //        case 6:
        //            return IsosurfaceValue6;
        //        case 7:
        //            return IsosurfaceValue7;
        //    }
        //    return 0f;
        //}
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct HeightMapParams
    {
        [FieldOffset(0)]
        public float DepthScale;

        [FieldOffset(4)]
        private float padding1;

        [FieldOffset(8)]
        private float padding2;

        [FieldOffset(12)]
        private float padding3;
    }

    public static class FeatureLevelHelper
    {
        public static string GetVertexShaderLevel(SharpDX.Direct3D.FeatureLevel featureLevel)
        {
            switch (featureLevel)
            {
                case SharpDX.Direct3D.FeatureLevel.Level_10_0:
                    return "vs_4_0";
                case SharpDX.Direct3D.FeatureLevel.Level_10_1:
                    return "vs_4_0";
                case SharpDX.Direct3D.FeatureLevel.Level_11_0:
                    return "vs_5_0";
                default:
                    throw new SharpDXException(string.Format("The feature level ({0}) of the hardware is not supported.",
                        featureLevel.ToString()));
            }
        }
        public static string GetPixelShaderLevel(SharpDX.Direct3D.FeatureLevel featureLevel)
        {
            switch (featureLevel)
            {
                case SharpDX.Direct3D.FeatureLevel.Level_10_0:
                    return "ps_4_0";
                case SharpDX.Direct3D.FeatureLevel.Level_10_1:
                    return "ps_4_0";
                case SharpDX.Direct3D.FeatureLevel.Level_11_0:
                    return "ps_5_0";
                default:
                    throw new SharpDXException(string.Format("The feature level ({0}) of the hardware is not supported.",
                        featureLevel.ToString()));
            }
        }
        public static string GetShader(SharpDX.Direct3D.FeatureLevel featureLevel, string shaderBaseName)
        {
            switch (featureLevel)
            {
                case SharpDX.Direct3D.FeatureLevel.Level_10_0:
                    return shaderBaseName + "_40";
                case SharpDX.Direct3D.FeatureLevel.Level_10_1:
                    return shaderBaseName + "_40";
                case SharpDX.Direct3D.FeatureLevel.Level_11_0:
                    return shaderBaseName + "_50";
                default:
                    throw new SharpDXException(string.Format("The feature level ({0}) of the hardware is not supported.",
                        featureLevel.ToString()));
            }
        }
    }
    internal static class Percentage
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

    public static class ExtensionMethods
    {
        public static Vector4 ToVector4(this Plane plane)
        {
            return new Vector4(plane.ToArray());
        }
        public static Vector4 ToVector4(this NotifiableColor c)
        {
            return new Vector4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }
        public static NotifiableColor ToNotifiableColor(this Color c)
        {
            return NotifiableColor.FromArgb(c.A, c.R, c.G, c.B);
        }
        public static Color ToSharpDXColor(this NotifiableColor c)
        {
            return new Color(c.R, c.G, c.B, c.A);
        }

        public static Color ToSharpDXColor(this System.Windows.Media.Color c)
        {
            return new Color(c.R, c.G, c.B, c.A);
        }
        public static System.Windows.Media.Color ToMediaColor(this Color c)
        {
            return System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
        }
    }
}