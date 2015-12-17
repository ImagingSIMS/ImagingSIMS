using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using SharpDX;

namespace Direct3DRendering
{
    public class Vector3ToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
               System.Globalization.CultureInfo culture)
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
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class FPSToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
               System.Globalization.CultureInfo culture)
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
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class VolumeToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
               System.Globalization.CultureInfo culture)
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
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class VolumeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
               System.Globalization.CultureInfo culture)
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
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    public class SharpDXColorToMediaColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
               System.Globalization.CultureInfo culture)
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
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
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
            switch(VolumeNumber)
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

    [StructLayout(LayoutKind.Explicit, Size = 256)]
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
        private float IsosurfaceValue0;

        [FieldOffset(144)]
        private float IsosurfaceValue1;

        [FieldOffset(160)]
        private float IsosurfaceValue2;

        [FieldOffset(176)]
        private float IsosurfaceValue3;

        [FieldOffset(192)]
        private float IsosurfaceValue4;

        [FieldOffset(208)]
        private float IsosurfaceValue5;

        [FieldOffset(224)]
        private float IsosurfaceValue6;

        [FieldOffset(240)]
        private float IsosurfaceValue7;

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

                    IsosurfaceValue0 = 0,
                    IsosurfaceValue1 = 0,
                    IsosurfaceValue2 = 0,
                    IsosurfaceValue3 = 0,
                    IsosurfaceValue4 = 0,
                    IsosurfaceValue5 = 0,
                    IsosurfaceValue6 = 0,
                    IsosurfaceValue7 = 0,
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
        public void UpdateValue(int IsosurfaceNumber, float Value)
        {
            switch (IsosurfaceNumber)
            {
                case 0:
                    IsosurfaceValue0 = Value;
                    break;
                case 1:
                    IsosurfaceValue1 = Value;
                    break;
                case 2:
                    IsosurfaceValue2 = Value;
                    break;
                case 3:
                    IsosurfaceValue3 = Value;
                    break;
                case 4:
                    IsosurfaceValue4 = Value;
                    break;
                case 5:
                    IsosurfaceValue5 = Value;
                    break;
                case 6:
                    IsosurfaceValue6 = Value;
                    break;
                case 7:
                    IsosurfaceValue7 = Value;
                    break;
            }
        }
        public float GetValue(int IsosurfaceNumber)
        {
            switch (IsosurfaceNumber)
            {
                case 0:
                    return IsosurfaceValue0;
                case 1:
                    return IsosurfaceValue1;
                case 2:
                    return IsosurfaceValue2;
                case 3:
                    return IsosurfaceValue3;
                case 4:
                    return IsosurfaceValue4;
                case 5:
                    return IsosurfaceValue5;
                case 6:
                    return IsosurfaceValue6;
                case 7:
                    return IsosurfaceValue7;
            }
            return 0f;
        }
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
    }
}
