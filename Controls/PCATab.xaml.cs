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

using ImagingSIMS.Data.Spectra;
using ImagingSIMS.Data.PCA;
using System.Collections.ObjectModel;

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for PCATab.xaml
    /// </summary>
    public partial class PCATab : UserControl
    {
        public static readonly DependencyProperty OriginalSpectrumProperty = DependencyProperty.Register("OriginalSpectrum",
            typeof(Spectrum), typeof(PCATab));

        public Spectrum OriginalSpectrum
        {
            get { return (Spectrum)GetValue(OriginalSpectrumProperty); }
            set
            {
                SetValue(OriginalSpectrumProperty, value);
                Spectrum s = value as Spectrum;
                if(s!= null)
                {
                    //specChartOriginal.SetData(s);
                }
            }
        }


        public PCATab()
        {
            InitializeComponent();
        }

        private void identifyPeaks_Click(object sender, RoutedEventArgs e)
        {
            double[,] spectrum = OriginalSpectrum.ToDoubleArray();

            //ObservableCollection<Peak> foundPeaks = Peak.IdentifyPeaks(spectrum, .414, 25, 18, 18);
            ObservableCollection<Peak> foundPeaks = Peak.IdentifyPeaks(spectrum, 20, 0.2);
            listViewFoundPeaks.ItemsSource = foundPeaks;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }
    }
}
