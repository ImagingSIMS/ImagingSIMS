using System;
using System.Collections.Generic;
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

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for TestControl.xaml
    /// </summary>
    public partial class TestControl : UserControl
    {
        public TestControl()
        {
            InitializeComponent();
        }

        public void ShowPrint(bool ShowPrint)
        {
            ResourceDictionary dictionary = new ResourceDictionary();

            if(ShowPrint)
            {
                dictionary.Source = new Uri("pack://application:,,,/SpectrumView/Themes/SpecChartPrinting.xaml", UriKind.Absolute);
            }
            else
            {
                dictionary.Source = new Uri("pack://application:,,,/SpectrumView/Themes/SpecChartStandard.xaml", UriKind.Absolute);
            }

            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(dictionary);
        }
    }
}
