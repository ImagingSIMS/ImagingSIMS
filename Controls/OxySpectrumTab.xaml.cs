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

using OxyPlot;
using OxyPlot.Series;

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for OxySpectrumTab.xaml
    /// </summary>
    public partial class OxySpectrumTab : UserControl
    {
        public static readonly DependencyProperty SpectrumModelProperty = DependencyProperty.Register("SpectrumModel",
            typeof(SpectrumViewModel), typeof(OxySpectrumTab));

        public SpectrumViewModel SpectrumModel
        {
            get { return (SpectrumViewModel)GetValue(SpectrumModelProperty); }
            set { SetValue(SpectrumModelProperty, value); }
        }
        public OxySpectrumTab()
        {
            InitializeComponent();

            SpectrumModel = new SpectrumViewModel();
        }
    }

    public class SpectrumViewModel
    {
        public PlotModel Model { get; private set; }

        public SpectrumViewModel()
        {
            this.Model = new PlotModel { Title = "Test Data Set" };
            this.Model.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "Cosine"));
        }

        public SpectrumViewModel(Spectrum spectrum)
        {
            this.Model = new PlotModel { Title = spectrum.Name };
            //this.Model.Series.Add(new linese)
        }
    }
}
