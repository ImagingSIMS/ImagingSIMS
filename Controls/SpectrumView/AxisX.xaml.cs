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

namespace ImagingSIMS.Controls.BaseControls.SpectrumView
{
    /// <summary>
    /// Interaction logic for AxisX.xaml
    /// </summary>
    public partial class AxisX : UserControl, INotifyPropertyChanged
    {
        string _value1;
        string _value2;
        string _value3;
        string _value4;
        string _value5;
        string _value6;
        string _value7;
        string _value8;
        string _value9;
        string _value10;
        string _value11;
        string _value12;
        string _value13;
        string _value14;
        string _value15;
        string _value16;
        string _value17;
        string _value18;
        string _value19;
        string _value20;

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
        public string Value7
        {
            get { return _value7; }
            set
            {
                _value7 = value;
                OnChanged("Value7");
            }
        }
        public string Value8
        {
            get { return _value8; }
            set
            {
                _value8 = value;
                OnChanged("Value8");
            }
        }
        public string Value9
        {
            get { return _value9; }
            set
            {
                _value9 = value;
                OnChanged("Value9");
            }
        }
        public string Value10
        {
            get { return _value10; }
            set
            {
                _value10 = value;
                OnChanged("Value10");
            }
        }
        public string Value11
        {
            get { return _value11; }
            set
            {
                _value11 = value;
                OnChanged("Value11");
            }
        }
        public string Value12
        {
            get { return _value12; }
            set
            {
                _value12 = value;
                OnChanged("Value12");
            }
        }
        public string Value13
        {
            get { return _value13; }
            set
            {
                _value13 = value;
                OnChanged("Value13");
            }
        }
        public string Value14
        {
            get { return _value14; }
            set
            {
                _value14 = value;
                OnChanged("Value14");
            }
        }
        public string Value15
        {
            get { return _value15; }
            set
            {
                _value15 = value;
                OnChanged("Value15");
            }
        }
        public string Value16
        {
            get { return _value16; }
            set
            {
                _value16 = value;
                OnChanged("Value16");
            }
        }
        public string Value17
        {
            get { return _value17; }
            set
            {
                _value17 = value;
                OnChanged("Value17");
            }
        }
        public string Value18
        {
            get { return _value18; }
            set
            {
                _value18 = value;
                OnChanged("Value18");
            }
        }
        public string Value19
        {
            get { return _value19; }
            set
            {
                _value19 = value;
                OnChanged("Value19");
            }
        }
        public string Value20
        {
            get { return _value20; }
            set
            {
                _value20 = value;
                OnChanged("Value20");
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

        public AxisX()
        {
            Value1 = "xxx.xx";
            Value2 = "xxx.xx";
            Value3 = "xxx.xx";
            Value4 = "xxx.xx";
            Value5 = "xxx.xx";
            Value6 = "xxx.xx";
            Value7 = "xxx.xx";
            Value8 = "xxx.xx";
            Value9 = "xxx.xx";
            Value10 = "xxx.xx";
            Value11 = "xxx.xx";
            Value12 = "xxx.xx";
            Value13 = "xxx.xx";
            Value14 = "xxx.xx";
            Value15 = "xxx.xx";
            Value16 = "xxx.xx";
            Value17 = "xxx.xx";
            Value18 = "xxx.xx";
            Value19 = "xxx.xx";
            Value20 = "xxx.xx";

            _title = "Title";

            InitializeComponent();

            Loaded += AxisX_Loaded;
            SizeChanged += AxisX_SizeChanged;
        }

        void AxisX_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }
        void AxisX_Loaded(object sender, RoutedEventArgs e)
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
            double[] newValues = new double[20];
            double range = _maximum - _minimum;
            //if (range < 0)
            //{
            //    throw new ArgumentException("The intensity scale is invalid. Minimum > maximum");
            //}

            double increment = range / 20d;

            for (int i = 0; i < 20; i++)
            {
                newValues[i] = _minimum + (increment * (double)(i + 0.5));
            }

            string format = "";
            if (range <= 3)
            {
                format = "0.000";
            }
            else
            {
                format = "0.00";
            }

            Value1 = newValues[0].ToString(format);
            Value2 = newValues[1].ToString(format);
            Value3 = newValues[2].ToString(format);
            Value4 = newValues[3].ToString(format);
            Value5 = newValues[4].ToString(format);
            Value6 = newValues[5].ToString(format);
            Value7 = newValues[6].ToString(format);
            Value8 = newValues[7].ToString(format);
            Value9 = newValues[8].ToString(format);
            Value10 = newValues[9].ToString(format);
            Value11 = newValues[10].ToString(format);
            Value12 = newValues[11].ToString(format);
            Value13 = newValues[12].ToString(format);
            Value14 = newValues[13].ToString(format);
            Value15 = newValues[14].ToString(format);
            Value16 = newValues[15].ToString(format);
            Value17 = newValues[16].ToString(format);
            Value18 = newValues[17].ToString(format);
            Value19 = newValues[18].ToString(format);
            Value20 = newValues[19].ToString(format);
        }

        public double ValueFromCoordinate(double Pixel)
        {
            double valueRange = _maximum - _minimum;

            return (((Pixel / (double)this.ActualWidth) * (valueRange)) + _minimum);
        }
    }
}