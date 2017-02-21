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
        ObservableCollection<Data2D> _dataVariables;
        ObservableCollection<double> _scalarFactors;
        TermIdentity _leftTerm;
        TermIdentity _rightTerm;
        MathOperations _operation;
        string _operationHistory;
        Data2D _result;

        public ObservableCollection<Data2D> DataVariables
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
        public ObservableCollection<double> ScalarFactors
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
        public MathOperations Operation
        {
            get { return _operation; }
            set
            {
                if(_operation != value)
                {
                    _operation = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string OperationHistory
        {
            get { return _operationHistory; }
            set
            {
                if (_operationHistory != value)
                {
                    _operationHistory = value;
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
            DataVariables = new ObservableCollection<Data2D>()
            {
                Data2D.Empty, Data2D.Empty, Data2D.Empty, Data2D.Empty,
                Data2D.Empty, Data2D.Empty, Data2D.Empty, Data2D.Empty
            };
            ScalarFactors = new ObservableCollection<double>();
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
    public class MathTermIndexAttribute : Attribute
    {
        public int Index { get; set; }

        public MathTermIndexAttribute(int index)
        {
            Index = index;
        }
    }

    public enum TermType
    {
        Data,

        Scalar,

        Unknown,
    }

    public enum TermIdentity
    {
        [Description("Variable 1")]
        [MathTermType(TermType.Data)]
        [MathTermIndex(0)]
        Variable1,

        [Description("Variable 1")]
        [MathTermType(TermType.Data)]
        [MathTermIndex(1)]
        Variable2,

        [Description("Variable 1")]
        [MathTermType(TermType.Data)]
        [MathTermIndex(2)]
        Variable3,

        [Description("Variable 1")]
        [MathTermType(TermType.Data)]
        [MathTermIndex(3)]
        Variable4,

        [Description("Variable 1")]
        [MathTermType(TermType.Data)]
        [MathTermIndex(4)]
        Variable5,

        [Description("Variable 1")]
        [MathTermType(TermType.Data)]
        [MathTermIndex(5)]
        Variable6,

        [Description("Variable 1")]
        [MathTermType(TermType.Data)]
        [MathTermIndex(6)]
        Variable7,

        [Description("Variable 1")]
        [MathTermType(TermType.Data)]
        [MathTermIndex(7)]
        Variable8,

        [Description("Scalar 1")]
        [MathTermType(TermType.Scalar)]
        [MathTermIndex(0)]
        Scalar1,

        [Description("Scalar 2")]
        [MathTermType(TermType.Scalar)]
        [MathTermIndex(1)]
        Scalar2,

        [Description("Scalar 3")]
        [MathTermType(TermType.Scalar)]
        [MathTermIndex(2)]
        Scalar3,

        [Description("Scalar 4")]
        [MathTermType(TermType.Scalar)]
        [MathTermIndex(3)]
        Scalar4,

        [Description("Scalar 5")]
        [MathTermType(TermType.Scalar)]
        [MathTermIndex(4)]
        Scalar5,

        [Description("Scalar 6")]
        [MathTermType(TermType.Scalar)]
        [MathTermIndex(5)]
        Scalar6,

        [Description("Scalar 7")]
        [MathTermType(TermType.Scalar)]
        [MathTermIndex(6)]
        Scalar7,

        [Description("Scalar 8")]
        [MathTermType(TermType.Scalar)]
        [MathTermIndex(7)]
        Scalar8,
    }
}
