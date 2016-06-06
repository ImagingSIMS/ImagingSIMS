using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using ImagingSIMS.Common.Controls;
using SharpDX;

namespace Direct3DRendering
{
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

    [StructLayout(LayoutKind.Explicit, Size = 192)]
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

        [FieldOffset(160)]
        public Vector4 MinClipCoords;

        [FieldOffset(176)]
        public Vector4 MaxClipCoords;
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

    [StructLayout(LayoutKind.Explicit, Size = 544)]
    public struct LightingParams
    {
        [FieldOffset(0)]
        public Vector4 AmbientLightColor;

        [FieldOffset(16)]
        public float AmbientLightIntensity;

        [FieldOffset(20)]
        public float EnableAmbientLighting;

        [FieldOffset(24)]
        public float EnableDirectionalLighting;

        [FieldOffset(28)]
        public float EnableSpecularLighting;

        // These are really padded in cbuffer
        [FieldOffset(32)]
        private float directionalEnabled0;
        
        [FieldOffset(48)]
        private float directionalEnabled1;

        [FieldOffset(64)]
        private float directionalEnabled2;

        [FieldOffset(80)]
        private float directionalEnabled3;

        [FieldOffset(96)]
        private float directionalEnabled4;

        [FieldOffset(112)]
        private float directionalEnabled5;

        [FieldOffset(128)]
        private float directionalEnabled6;

        [FieldOffset(144)]
        private float directionalEnabled7;

        [FieldOffset(160)]
        private Vector4 directionalDirection0;

        [FieldOffset(176)]
        private Vector4 directionalDirection1;

        [FieldOffset(192)]
        private Vector4 directionalDirection2;

        [FieldOffset(208)]
        private Vector4 directionalDirection3;

        [FieldOffset(224)]
        private Vector4 directionalDirection4;

        [FieldOffset(240)]
        private Vector4 directionalDirection5;

        [FieldOffset(256)]
        private Vector4 directionalDirection6;

        [FieldOffset(272)]
        private Vector4 directionalDirection7;

        [FieldOffset(288)]
        private Vector4 directionalColor0;

        [FieldOffset(304)]
        private Vector4 directionalColor1;

        [FieldOffset(320)]
        private Vector4 directionalColor2;

        [FieldOffset(336)]
        private Vector4 directionalColor3;

        [FieldOffset(352)]
        private Vector4 directionalColor4;

        [FieldOffset(368)]
        private Vector4 directionalColor5;

        [FieldOffset(384)]
        private Vector4 directionalColor6;

        [FieldOffset(400)]
        private Vector4 directionalColor7;

        // These are actually float4 in cbuffer
        [FieldOffset(416)]
        private float directionalIntensity0;

        [FieldOffset(432)]
        private float directionalIntensity1;

        [FieldOffset(448)]
        private float directionalIntensity2;

        [FieldOffset(464)]
        private float directionalIntensity3;

        [FieldOffset(480)]
        private float directionalIntensity4;

        [FieldOffset(496)]
        private float directionalIntensity5;

        [FieldOffset(512)]
        private float directionalIntensity6;

        [FieldOffset(528)]
        private float directionalIntensity7;

        public static LightingParams Empty
        {
            get
            {
                return new LightingParams()
                {
                    directionalDirection0 = new Vector4(0),
                    directionalDirection1 = new Vector4(0),
                    directionalDirection2 = new Vector4(0),
                    directionalDirection3 = new Vector4(0),
                    directionalDirection4 = new Vector4(0),
                    directionalDirection5 = new Vector4(0),
                    directionalDirection6 = new Vector4(0),
                    directionalDirection7 = new Vector4(0),

                    directionalColor0 = new Vector4(0.0f),
                    directionalColor1 = new Vector4(0.0f),
                    directionalColor2 = new Vector4(0.0f),
                    directionalColor3 = new Vector4(0.0f),
                    directionalColor4 = new Vector4(0.0f),
                    directionalColor5 = new Vector4(0.0f),
                    directionalColor6 = new Vector4(0.0f),
                    directionalColor7 = new Vector4(0.0f)
                };
            }
        }

        public void UpdateDirectionalLight(int index, bool isEnabled)
        {
            switch (index)
            {
                case 0:
                    directionalEnabled0 = isEnabled ? 1.0f : 0.0f;
                    break;
                case 1:
                    directionalEnabled1 = isEnabled ? 1.0f : 0.0f;
                    break;
                case 2:
                    directionalEnabled2 = isEnabled ? 1.0f : 0.0f;
                    break;
                case 3:
                    directionalEnabled3 = isEnabled ? 1.0f : 0.0f;
                    break;
                case 4:
                    directionalEnabled4 = isEnabled ? 1.0f : 0.0f;
                    break;
                case 5:
                    directionalEnabled5 = isEnabled ? 1.0f : 0.0f;
                    break;
                case 6:
                    directionalEnabled6 = isEnabled ? 1.0f : 0.0f;
                    break;
                case 7:
                    directionalEnabled7 = isEnabled ? 1.0f : 0.0f;
                    break;
            }
        }
        public void UpdateDirectionalLight(int index, Vector4 direction)
        {
            switch (index)
            {
                case 0:
                    directionalDirection0 = direction;
                    break;
                case 1:
                    directionalDirection1 = direction;
                    break;
                case 2:
                    directionalDirection2 = direction;
                    break;
                case 3:
                    directionalDirection3 = direction;
                    break;
                case 4:
                    directionalDirection4 = direction;
                    break;
                case 5:
                    directionalDirection5 = direction;
                    break;
                case 6:
                    directionalDirection6 = direction;
                    break;
                case 7:
                    directionalDirection7 = direction;
                    break;
            }
        }
        public void UpdateDirectionalLight(int index, NotifiableColor color)
        {
            switch (index)
            {
                case 0:
                    directionalColor0 = color.ToVector4();
                    break;
                case 1:
                    directionalColor1 = color.ToVector4();
                    break;
                case 2:
                    directionalColor2 = color.ToVector4();
                    break;
                case 3:
                    directionalColor3 = color.ToVector4();
                    break;
                case 4:
                    directionalColor4 = color.ToVector4();
                    break;
                case 5:
                    directionalColor5 = color.ToVector4();
                    break;
                case 6:
                    directionalColor6 = color.ToVector4();
                    break;
                case 7:
                    directionalColor7 = color.ToVector4();
                    break;
            }
        }
        public void UpdateDirectionalLight(int index, Color color)
        {
            switch (index)
            {
                case 0:
                    directionalColor0 = color.ToVector4();
                    break;
                case 1:
                    directionalColor1 = color.ToVector4();
                    break;
                case 2:
                    directionalColor2 = color.ToVector4();
                    break;
                case 3:
                    directionalColor3 = color.ToVector4();
                    break;
                case 4:
                    directionalColor4 = color.ToVector4();
                    break;
                case 5:
                    directionalColor5 = color.ToVector4();
                    break;
                case 6:
                    directionalColor6 = color.ToVector4();
                    break;
                case 7:
                    directionalColor7 = color.ToVector4();
                    break;
            }
        }
        public void UpdateDirectionalLight(int index, float intensity)
        {
            switch (index)
            {
                case 0:
                    directionalIntensity0 = intensity;
                    break;
                case 1:
                    directionalIntensity1 = intensity;
                    break;
                case 2:
                    directionalIntensity2 = intensity;
                    break;
                case 3:
                    directionalIntensity3 = intensity;
                    break;
                case 4:
                    directionalIntensity4 = intensity;
                    break;
                case 5:
                    directionalIntensity5 = intensity;
                    break;
                case 6:
                    directionalIntensity6 = intensity;
                    break;
                case 7:
                    directionalIntensity7 = intensity;
                    break;
            }
        }
        public void UpdateDirectionalLight(int index, bool isEnabled, Vector4 direction, NotifiableColor color, float intensity)
        {
            UpdateDirectionalLight(index, isEnabled);
            UpdateDirectionalLight(index, direction);
            UpdateDirectionalLight(index, color);
            UpdateDirectionalLight(index, intensity);
        }
        public void UpdateDirectionalLight(int index, bool isEnabled, Vector4 direction, Color color, float intensity)
        {
            UpdateDirectionalLight(index, isEnabled);
            UpdateDirectionalLight(index, direction);
            UpdateDirectionalLight(index, color);
            UpdateDirectionalLight(index, intensity);
        }
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