using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ImagingSIMS.Data.Imaging
{
    // To add a new color scale:
    // 1. Add a static function to the ColorScales class which takes the value
    //      and returns the appropriate color.
    // 2. Add a new option to the enum ColorScaleTypes and an optional Description
    //      attribute if the name is more than one word.
    // 3. Add the new enum option to the switch statement in FromScale to return
    //      the new color scale.
    //
    //  Color scale options in the ImagingSIMS controls are dynamically generated 
    //      from the enum options specifed in ColorScaleTypes. It is not necessary
    //      to alter those functions to include the new color scale.
    public static class ColorScales
    {
        const double Zero = 0.0d;
        const double One = 1.0d;
        const double OneThird = 0.333333d;
        const double TwoThirds = 0.666666d;
        const double OneQuarter = 0.25d;
        const double OneHalf = 0.5d;
        const double ThreeQuarters = 0.75d;
        const double OneFifth = 0.2d;
        const double TwoFifths = 0.4d;
        const double ThreeFifths = 0.6d;
        const double FourFifths = 0.8d;
        const double OneEighth = 0.125d;
        const double ThreeEighths = 0.375d;
        const double FiveEighths = 0.625d;
        const double SevenEighths = 0.875d;
        const double OneSeventh = 0.142857d;
        const double TwoSevenths = 0.285714;
        const double ThreeSevenths = 0.428571d;
        const double FourSevenths = 0.571429d;
        const double FiveSevenths = 0.714286d;
        const double SixSevenths = 0.857143d;

        public static Color ThermalWarm(double Value, double Max)
        {
            double ratio = Value / Max;

            if (ratio == Zero) return Color.FromArgb(255, 0, 0, 0);
            if (ratio < OneThird)
            {
                return Color.FromArgb(255, (byte)(ratio * 768d), 0, 0);
            }
            else if (ratio < TwoThirds)
            {
                return Color.FromArgb(255, 255, (byte)((ratio * 768d) - 256d), 0);
            }
            else if (ratio < One)
            {
                return Color.FromArgb(255, 255, 255, (byte)((ratio * 768d) - 512d));
            }
            else if (ratio >= One)
            {
                return Color.FromArgb(255, 255, 255, 255);
            }

            else return Color.FromArgb(255, 0, 0, 0);
        }
        public static Color ThermalCold(double Value, double Maximum)
        {
            double ratio = Value / Maximum;

            if (ratio == Zero) return Color.FromArgb(255, 0, 0, 0);
            if (ratio < OneThird)
            {
                return Color.FromArgb(255, 0, 0, (byte)(ratio * 768d));
            }
            else if (ratio < TwoThirds)
            {
                return Color.FromArgb(255, 0, (byte)((ratio * 768d) - 256d), 255);
            }
            else if (ratio < One)
            {
                return Color.FromArgb(255, (byte)((ratio * 768d) - 512d), 255, 255);
            }
            else if (ratio >= One)
            {
                return Color.FromArgb(255, 255, 255, 255);
            }

            else return Color.FromArgb(255, 0, 0, 0);
        }
        public static Color Neon(double Value, double Maximum)
        {
            double ratio = Value / Maximum;

            if (ratio == Zero) return Color.FromArgb(255, 0, 0, 0);
            if (ratio < OneThird)
            {
                return Color.FromArgb(255, (byte)(ratio * 768d), (byte)(255 - (ratio * 768d)), 255);
            }
            else if (ratio < TwoThirds)
            {
                return Color.FromArgb(255, 255, (byte)((ratio * 768d) - 256d), (byte)(255d - ((ratio * 768d) - 256d)));
            }
            else if (ratio < One)
            {
                return Color.FromArgb(255, 255, 255, (byte)((ratio * 768d) - 512d));
            }
            else if (ratio >= One)
            {
                return Color.FromArgb(255, 255, 255, 255);
            }

            else
            {
                return Color.FromArgb(255, 0, 0, 0);
            }
        }
        public static Color Retro(double Value, double Maximum)
        {
            double ratio = Value / Maximum;

            if (ratio == Zero) return Color.FromArgb(255, 0, 0, 0);
            if (ratio < OneThird)
            {
                return Color.FromArgb(255, (byte)(255 - (ratio * 768d)), (byte)(ratio * 768d), 255);
            }
            else if (ratio < TwoThirds)
            {
                return Color.FromArgb(255, (byte)((ratio * 768d) - 256d), 255, (byte)(255d - ((ratio * 768d) - 256d)));
            }
            else if (ratio < One)
            {
                return Color.FromArgb(255, 255, 255, (byte)((ratio * 768d) - 512d));
            }
            else if (ratio >= One)
            {
                return Color.FromArgb(255, 255, 255, 255);
            }

            else
            {
                return Color.FromArgb(255, 0, 0, 0);
            }
        }
        public static Color Gray(double Value, double Maximum)
        {
            double ratio = Value / Maximum;

            if (ratio == Zero) return Color.FromArgb(255, 0, 0, 0);
            if (ratio >= One) return Color.FromArgb(255, 255, 255, 255);

            byte value = (byte)(ratio * 255d);

            return Color.FromArgb(255, value, value, value);
        }
        public static Color Solid(double Value, double Maximum, Color C)
        {
            double ratio = Value / Maximum;

            if (ratio == Zero) return Color.FromArgb(255, 0, 0, 0);
            if (ratio >= One) return C;

            return Color.FromArgb(255, (byte)(C.R * ratio), (byte)(C.G * ratio), (byte)(C.B * ratio));
        }
        public static Color GreenWhite(double Value, double Maximum)
        {
            double ratio = Value / Maximum;

            if (ratio == Zero) return Color.FromArgb(255, 0, 0, 0);
            if (ratio < OneHalf)
            {
                return Color.FromArgb(255, 0, (byte)(ratio * 512d ), 0);
            }
            else if (ratio < One)
            {
                return Color.FromArgb(255, (byte)(ratio * 512d - 256d), 255, (byte)(ratio * 512d - 256d));
            }
            else if (ratio >= One)
            {
                return Color.FromArgb(255, 255, 255, 255);
            }
            else return Color.FromArgb(255, 0, 0, 0);
        }
        public static Color RedWhite(double Value, double Maximum)
        {
            double ratio = Value / Maximum;

            if (ratio == Zero) return Color.FromArgb(255, 0, 0, 0);
            if (ratio < OneHalf)
            {
                return Color.FromArgb(255, (byte)(ratio * 512d), 0, 0);
            }
            else if (ratio < One)
            {
                return Color.FromArgb(255, 255, (byte)(ratio * 512d - 256d), (byte)(ratio * 512d - 256d));
            }
            else if (ratio >= One)
            {
                return Color.FromArgb(255, 255, 255, 255);
            }
            else return Color.FromArgb(255, 0, 0, 0);
        }
        public static Color BlueWhite(double Value, double Maximum)
        {
            double ratio = Value / Maximum;

            if (ratio == Zero) return Color.FromArgb(255, 0, 0, 0);
            if (ratio < OneHalf)
            {
                return Color.FromArgb(255, 0, 0, (byte)(ratio * 512d));
            }
            else if (ratio < One)
            {
                return Color.FromArgb(255, (byte)(ratio * 512d - 256d), (byte)(ratio * 512d - 256d), 255);
            }
            else if (ratio >= One)
            {
                return Color.FromArgb(255, 255, 255, 255);
            }
            else return Color.FromArgb(255, 0, 0, 0);
        }
        public static Color MagentaWhite(double Value, double Maximum)
        {
            double ratio = Value / Maximum;

            if (ratio == Zero) return Color.FromArgb(255, 0, 0, 0);
            if (ratio < OneHalf)
            {
                return Color.FromArgb(255, (byte)(ratio * 512d), 0, (byte)(ratio * 512d));
            }
            else if (ratio < One)
            {
                return Color.FromArgb(255, 255, (byte)(ratio * 512d - 256d), 255);
            }
            else if (ratio >= One)
            {
                return Color.FromArgb(255, 255, 255, 255);
            }
            else return Color.FromArgb(255, 0, 0, 0);
        }
        public static Color YellowWhite(double Value, double Maximum)
        {
            double ratio = Value / Maximum;

            if (ratio == Zero) return Color.FromArgb(255, 0, 0, 0);
            if (ratio < OneHalf)
            {
                return Color.FromArgb(255, (byte)(ratio * 512d), (byte)(ratio * 512d), 0);
            }
            else if (ratio < One)
            {
                return Color.FromArgb(255, 255, 255, (byte)(ratio * 512d - 256d));
            }
            else if (ratio >= One)
            {
                return Color.FromArgb(255, 255, 255, 255);
            }
            else return Color.FromArgb(255, 0, 0, 0);
        }
        public static Color CyanWhite(double Value, double Maximum)
        {
            double ratio = Value / Maximum;

            if (ratio == Zero) return Color.FromArgb(255, 0, 0, 0);
            if (ratio < OneHalf)
            {
                return Color.FromArgb(255, 0, (byte)(ratio * 512d), (byte)(ratio * 512d));
            }
            else if (ratio < One)
            {
                return Color.FromArgb(255, (byte)(ratio * 512d - 256d), 255, 255);
            }
            else if (ratio >= One)
            {
                return Color.FromArgb(255, 255, 255, 255);
            }
            else return Color.FromArgb(255, 0, 0, 0);
        }
        public static Color Rainbow(double Value, double Maximum)
        {
            double ratio = Value / Maximum;

            if (ratio == Zero) return Color.FromArgb(255, 0, 0, 0);

            if(ratio < OneSeventh) // black to blue
            {
                return Color.FromArgb(255, 0, 0, (byte)(ratio * 1792d));
            }
            else if(ratio < TwoSevenths) // blue to cyan
            {
                return Color.FromArgb(255, 0, (byte)(ratio * 1792d - 256d), 255);
            }
            else if(ratio < ThreeSevenths) // cyan to green
            {
                return Color.FromArgb(255, 0, 255, (byte)(255 - (ratio * 1792d - 512d)));
            }
            else if(ratio < FourSevenths) // green to yellow
            {
                return Color.FromArgb(255, (byte)(ratio * 1792d - 768d), 255, 0);
            }
            else if(ratio < FiveSevenths) // yellow to red
            {
                return Color.FromArgb(255, 255, (byte)(255 - (ratio * 1792d - 1024d)), 0);
            }
            else if(ratio < SixSevenths) // red to magenta
            {
                return Color.FromArgb(255, 255, 0, (byte)(ratio * 1792d - 1280d));
            }
            else if(ratio < One) // magenta to white
            {
                return Color.FromArgb(255, 255, (byte)(ratio * 1792d - 1536d), 255);
            }

            else if(ratio >= One)
            {
                return Color.FromArgb(255, 255, 255, 255);
            }
            else return Color.FromArgb(255, 0, 0, 0);
        }
        public static Color HeatMap(double Value, double Maximum)
        {
            double ratio = Value / Maximum;

            if (ratio == Zero) return Color.FromArgb(255, 0, 0, 255);
            if (ratio < OneThird) // blue to cyan
            {
                return Color.FromArgb(255, 0, (byte)(ratio * 768d), 255);
            }
            else if (ratio < TwoThirds) // cyan to yellow
            {
                return Color.FromArgb(255, (byte)(ratio * 768d - 256d), 255, (byte)(255 - (ratio * 768d - 256d)));
            }
            else if(ratio < One) // yellow to red
            {
                return Color.FromArgb(255, 255, (byte)(255 - (ratio * 768d - 512d)), 0);
            }
            else if (ratio >= One)
            {
                return Color.FromArgb(255, 255, 0, 0);
            }
            else return Color.FromArgb(255, 0, 0, 0);
        }
        public static Color Viridis(double Value, double Maximum)
        {
            double ratio = Value / Maximum;

            if (ratio == Zero) return Color.FromArgb(255, 68, 1, 84);

            double remainder = ratio;
            if(ratio < OneEighth) // (68, 1, 84) to (71, 44, 122)
            {
                return BetweenRange(Color.FromArgb(255, 68, 1, 84), Color.FromArgb(255, 71, 44, 122), remainder, OneEighth);
            }
            if (ratio < OneQuarter) // (71, 44, 122) to (59, 81, 139)
            {
                remainder -= OneEighth;
                return BetweenRange(Color.FromArgb(255, 71, 44, 122), Color.FromArgb(255, 59, 81, 139), remainder, OneEighth);
            }
            if (ratio < ThreeEighths) // (59, 81, 139) to (44, 113, 142)
            {
                remainder -= OneQuarter;
                return BetweenRange(Color.FromArgb(255, 59, 81, 139), Color.FromArgb(255, 44, 113, 142), remainder, OneEighth);
            }
            if (ratio < OneHalf) // (44, 113, 142) to (33, 144, 141)
            {
                remainder -= ThreeEighths;
                return BetweenRange(Color.FromArgb(255, 44, 113, 142), Color.FromArgb(255, 33, 144, 141), remainder, OneEighth);
            }
            if (ratio < FiveEighths) // (33, 144, 141) to (39, 173, 129)
            {
                remainder -= OneHalf;
                return BetweenRange(Color.FromArgb(255, 33, 144, 141), Color.FromArgb(255, 39, 173, 129), remainder, OneEighth);
            }
            if (ratio < ThreeQuarters) // (39, 173, 129) to (99, 200, 99)
            {
                remainder -= FiveEighths;
                return BetweenRange(Color.FromArgb(255, 39, 173, 129), Color.FromArgb(255, 99, 200, 99), remainder, OneEighth);
            }
            if (ratio < SevenEighths) // (99, 200, 99) to (170, 220, 50)
            {
                remainder -= ThreeQuarters;
                return BetweenRange(Color.FromArgb(255, 99, 200, 99), Color.FromArgb(255, 170, 220, 50), remainder, OneEighth);
            }
            if (ratio < One) // (170, 220, 50) to (253, 231, 32)
            {
                remainder -= SevenEighths;
                return BetweenRange(Color.FromArgb(255, 170, 220, 50), Color.FromArgb(255, 253, 231, 32), remainder, OneEighth);
            }
            if (ratio >= One)
            {
                return Color.FromArgb(255, 253, 231, 37);
            }

            return Color.FromArgb(255, 0, 0, 0);
        }

        public static Color FromScale(double Value, double Maximum, ColorScaleTypes Type, 
            Color SolidColor = new Color())
        {
            switch (Type)
            {
                case ColorScaleTypes.ThermalWarm:
                    return ThermalWarm(Value, Maximum);
                case ColorScaleTypes.ThermalCold:
                    return ThermalCold(Value, Maximum);
                case ColorScaleTypes.Neon:
                    return Neon(Value, Maximum);
                case ColorScaleTypes.Retro:
                    return Retro(Value, Maximum);
                case ColorScaleTypes.Gray:
                    return Gray(Value, Maximum);
                case ColorScaleTypes.Solid:
                    return Solid(Value, Maximum, SolidColor);
                case ColorScaleTypes.GreenWhite:
                    return GreenWhite(Value, Maximum);
                case ColorScaleTypes.BlueWhite:
                    return BlueWhite(Value, Maximum);
                case ColorScaleTypes.RedWhite:
                    return RedWhite(Value, Maximum);
                case ColorScaleTypes.MagentaWhite:
                    return MagentaWhite(Value, Maximum);
                case ColorScaleTypes.CyanWhite:
                    return CyanWhite(Value, Maximum);
                case ColorScaleTypes.YellowWhite:
                    return YellowWhite(Value, Maximum);
                case ColorScaleTypes.Rainbow:
                    return Rainbow(Value, Maximum);
                case ColorScaleTypes.HeatMap:
                    return HeatMap(Value, Maximum);
                case ColorScaleTypes.Viridis:
                    return Viridis(Value, Maximum);
                default: return ThermalWarm(Value, Maximum);
            }
        }

        private static Color BetweenRange(Color color1, Color color2, double value, double range)
        {
            return Color.FromArgb(255,
                (byte)(color1.R + ((color2.R - color1.R) * (value / range))),
                (byte)(color1.G + ((color2.G - color1.G) * (value / range))),
                (byte)(color1.B + ((color2.B - color1.B) * (value / range)))
                );
        }
    }

    public enum ColorScaleTypes
    {
        Solid,

        [Description("Thermal Warm")]
        ThermalWarm,

        [Description("Thermal Cold")]
        ThermalCold,

        Neon,

        Retro,

        Gray,

        [Description("Red-White")]
        RedWhite,

        [Description("Green-White")]
        GreenWhite,

        [Description("Blue-White")]
        BlueWhite,

        [Description("Magenta-White")]
        MagentaWhite,

        [Description("Yellow-White")]
        YellowWhite,

        [Description("Cyan-White")]
        CyanWhite,

        Rainbow,

        [Description("Heat Map")]
        HeatMap,

        Viridis,
    }
}
