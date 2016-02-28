using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImagingSIMS.Controls.BaseControls.SpectrumView
{
    /// <summary>
    /// Interaction logic for TestControl.xaml
    /// </summary>
    public partial class TestControl : UserControl
    {
        bool _isStandard;

        public TestControl()
        {
            _isStandard = true;

            InitializeComponent();
        }

        public void SwitchTheme()
        {
            string fileName;

            if (_isStandard)
            {
                fileName = Environment.CurrentDirectory +
                           @"\SpectrumView\Themes\SpecChartPrinting.xaml";
            }
            else
            {
                fileName = Environment.CurrentDirectory +
                       @"\SpectrumView\Themes\SpecChartStandard.xaml";
            }

            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                // Read in ResourceDictionary File
                ResourceDictionary dic = (ResourceDictionary)XamlReader.Load(fs);
                // Clear any previous dictionaries loaded
                Resources.MergedDictionaries.Clear();
                // Add in newly loaded Resource Dictionary
                Resources.MergedDictionaries.Add(dic);
            }

            _isStandard = !_isStandard;
        }
    }
}
