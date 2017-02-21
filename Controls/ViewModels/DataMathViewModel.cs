using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImagingSIMS.Data;

namespace ImagingSIMS.Controls.ViewModels
{
    public class DataMathViewModel : INotifyPropertyChanged
    {
        Data2D[] _dataVariables;
        double[] _scalarFactors;
        TermIdentity _leftTerm;
        TermIdentity _rightTerm;
        Data2D _result;

        public Data2D[] DataVariables
        {
            get { return _dataVariables; }
            set
            {
                if (_dataVariables != value)
                {
                    _dataVariables = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double[] ScalarFactors
        {
            get { return _scalarFactors; }
            set
            {
                if (_scalarFactors != value)
                {
                    _scalarFactors = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public TermIdentity LeftTerm
        {
            get { return _leftTerm; }
            set
            {
                if (_leftTerm != value)
                {
                    _leftTerm = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public TermIdentity RightTerm
        {
            get { return _rightTerm; }
            set
            {
                if (_rightTerm != value)
                {
                    _rightTerm = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Data2D Result
        {
            get { return _result; }
            set
            {
                if (_result != value)
                {
                    _result = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public DataMathViewModel()
        {
            DataVariables = new Data2D[8];
            ScalarFactors = new double[8];
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MathTermTypeAttribute : Attribute
    {
        public TermType TermType { get; set; }
        public bool IsData { get { return TermType == TermType.Data; } }
        public bool IsScalar { get { return TermType == TermType.Scalar; } }

        public MathTermTypeAttribute(TermType termType)
        {
            TermType = termType;
        }
    }

    public enum TermType
    {
        Data,

        Scalar,
    }

    public enum TermIdentity
    {
        [Description("Variable 1")]
        [MathTermType(TermType.Data)]
        Variable1,

        [Description("Variable 1")]
        [MathTermType(TermType.Data)]
        Variable2,

        [Description("Variable 1")]
        [MathTermType(TermType.Data)]
        Variable3,

        [Description("Variable 1")]
        [MathTermType(TermType.Data)]
        Variable4,

        [Description("Variable 1")]
        [MathTermType(TermType.Data)]
        Variable5,

        [Description("Variable 1")]
        [MathTermType(TermType.Data)]
        Variable6,

        [Description("Variable 1")]
        [MathTermType(TermType.Data)]
        Variable7,

        [Description("Variable 1")]
        [MathTermType(TermType.Data)]
        Variable8,

        [Description("Scalar 1")]
        [MathTermType(TermType.Scalar)]
        Scalar1,

        [Description("Scalar 2")]
        [MathTermType(TermType.Scalar)]
        Scalar2,

        [Description("Scalar 3")]
        [MathTermType(TermType.Scalar)]
        Scalar3,

        [Description("Scalar 4")]
        [MathTermType(TermType.Scalar)]
        Scalar4,

        [Description("Scalar 5")]
        [MathTermType(TermType.Scalar)]
        Scalar5,

        [Description("Scalar 6")]
        [MathTermType(TermType.Scalar)]
        Scalar6,

        [Description("Scalar 7")]
        [MathTermType(TermType.Scalar)]
        Scalar7,

        [Description("Scalar 8")]
        [MathTermType(TermType.Scalar)]
        Scalar8,
    }
}
