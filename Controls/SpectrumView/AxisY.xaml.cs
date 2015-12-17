using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImagingSIMS.Controls.SpectrumView
{
    /// <summary>
    /// Interaction logic for AxisY.xaml
    /// </summary>
    public partial class AxisY : UserControl, INotifyPropertyChanged
    {
        string _value1;
        string _value2;
        string _value3;
        string _value4;
        string _value5;
        string _value6;

        double _minimum;
        double _maximum;

        string _title;

        public string Value1
        {
            get { return _value1; }
            set
            {
                _value1 = value;
                OnChanged("Value1");
            }
        }
        public string Value2
        {
            get { return _value2; }
            set
            {
                _value2 = value;
                OnChanged("Value2");
            }
        }
        public string Value3
        {
            get { return _value3; }
            set
            {
                _value3 = value;
                OnChanged("Value3");
            }
        }
        public string Value4
        {
            get { return _value4; }
            set
            {
                _value4 = value;
                OnChanged("Value4");
            }
        }
        public string Value5
        {
            get { return _value5; }
            set
            {
                _value5 = value;
                OnChanged("Value5");
            }
        }
        public string Value6
        {
            get { return _value6; }
            set
            {
                _value6 = value;
                OnChanged("Value6");
            }
        }

        public double Minimum
        {
            get { return _minimum; }
            set
            {
                _minimum = value;
                CalculateAxisValues();
            }
        }
        public double Maximum
        {
            get { return _maximum; }
            set
            {
                _maximum = value;
                CalculateAxisValues();
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnChanged("Title");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnChanged(string PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        public AxisY()
        {
            _value1 = "xxxxx";
            _value2 = "xxxxx";
            _value3 = "xxxxx";
            _value4 = "xxxxx";
            _value5 = "xxxxx";
            _value6 = "xxxxx";

            _title = "Title";

            InitializeComponent();

            Loaded += AxisY_Loaded;
            SizeChanged += AxisY_SizeChanged;
        }

        void AxisY_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }
        void AxisY_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }

        public void SetMinMax(double Minimum, double Maximum)
        {
            _minimum = Minimum;
            _maximum = Maximum;
            CalculateAxisValues();
        }
        private void CalculateAxisValues()
        {
            double[] newValues = new double[6];
            double range = _maximum - _minimum;
            //if (range < 0)
            //{
            //    throw new ArgumentException("The intensity scale is invalid. Minimum > maximum");
            //}

            double increment = range / 6d;

            for (int i = 0; i < 6; i++)
            {
                newValues[i] = _minimum + (increment * (double)(i + 0.5));
            }

            string format = "";

            if (newValues[5] < 100)
            {
                format = "0.0";
            }
            else format = "0";

            Value1 = newValues[0].ToString(format);
            Value2 = newValues[1].ToString(format);
            Value3 = newValues[2].ToString(format);
            Value4 = newValues[3].ToString(format);
            Value5 = newValues[4].ToString(format);
            Value6 = newValues[5].ToString(format);
        }

        public double ValueFromCoordinate(double Pixel)
        {
            double valueRange = _maximum - _minimum;

            return (((Pixel / (double)this.ActualHeight) * (valueRange)) + _minimum);
        }
    }
}
