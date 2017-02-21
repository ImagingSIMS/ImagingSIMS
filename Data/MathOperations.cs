using System.ComponentModel;

namespace ImagingSIMS.Data
{
    public enum MathOperations
    {
        [Description("+")]
        Add,

        [Description("-")]
        Subtract,

        [Description("*")]
        Multiply,

        [Description("/")]
        Divide,

        [Description("^")]
        Power,

        [Description("1/x")]
        OneOver,
    }
}
