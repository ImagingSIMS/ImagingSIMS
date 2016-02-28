using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

using ImagingSIMS.Common;
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Controls.BaseControls.SpectrumView;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Spectra;

namespace ImagingSIMS.Controls.Tabs
{
    /// <summary>
    /// Interaction logic for SpectrumTab.xaml
    /// </summary>
    public partial class SpectrumTab : UserControl
    {
        bool _firstLoaded;

        Spectrum spectrum;
        string baseName;

        public static readonly DependencyProperty CanHistoryBackProperty = DependencyProperty.Register("CanHistoryBack",
            typeof(bool), typeof(SpectrumTab));
        public static readonly DependencyProperty CanHistoryForwardProperty = DependencyProperty.Register("CanHistoryForward",
            typeof(bool), typeof(SpectrumTab));

        public bool CanHistoryBack
        {
            get { return (bool)GetValue(CanHistoryBackProperty); }
            set { SetValue(CanHistoryBackProperty, value); }
        }
        public bool CanHistoryForward
        {
            get { return (bool)GetValue(CanHistoryForwardProperty); }
            set { SetValue(CanHistoryForwardProperty, value); }
        }

        public double StartMass { get; private set; }
        public double EndMass { get; private set; }

        public Spectrum Spectrum
        {
            get { return spectrum; }
        }

        public static readonly DependencyProperty PreviewVisibilityProperty = DependencyProperty.Register("PreviewVisibility",
            typeof(Visibility), typeof(SpectrumTab));
        public Visibility PreviewVisibility
        {
            get { return (Visibility)GetValue(PreviewVisibilityProperty); }
            set { SetValue(PreviewVisibilityProperty, value); }
        }

        public SpectrumTab()
        {
            _firstLoaded = true;

            InitializeComponent();

            _undoHistory = new Stack<double[]>();
            _redoHistory = new Stack<double[]>();

            PreviewVisibility = Visibility.Collapsed;

            SizeChanged += SpectrumDisplay_SizeChanged;

            AddHandler(SpecChart.RangeUpdatedEvent, new RangeUpdatedRoutedEventHandler(specChart_RangeUpdated));
            AddHandler(SpecChart.SelectionRangeUpdatedEvent, new RangeUpdatedRoutedEventHandler(specChart_SelectionRangeUpdated));

        }

        void SpectrumDisplay_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }

        async void specChart_RangeUpdated(object sender, RangeUpdatedRoutedEventArgs e)
        {
            double start = (double)e.MassStart;
            double end = (double)e.MassEnd;

            if (!e.IsKeyDown)
            {
                UpdateHistory(start, end);
            }

            if (imagePreview.Visibility == Visibility.Visible && imagePreview.LivePreview)
            {
                await imagePreview.CreateImageAsync(start, end);
            }
        }
        void specChart_SelectionRangeUpdated(object sender, RangeUpdatedRoutedEventArgs e)
        {
            StartMass = e.MassStart;
            EndMass = e.MassEnd;
        }

        public void SetData(string BaseName, Spectrum Spectrum)
        {
            baseName = BaseName;
            spectrum = Spectrum;

            imagePreview.SetData(spectrum);
            specChart.SetData(Spectrum);

            _currentPosition = new double[2] { specChart.InitialMinX, specChart.InitialMaxX };
        }

        public void ClearResources()
        {
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            if (!_firstLoaded)
            {
                specChart.Redraw();
            }
            _firstLoaded = false;
        }

        #region History
        Stack<double[]> _undoHistory;
        Stack<double[]> _redoHistory;
        double[] _currentPosition;
        bool _skipAddToHistory;

        private void UpdateHistory(double MinX, double MaxX)
        {
            if (!_skipAddToHistory)
            {
                double[] newRange = new double[2];
                newRange[0] = MinX;
                newRange[1] = MaxX;

                _undoHistory.Push(_currentPosition);
                _currentPosition = newRange;
                _redoHistory.Clear();
            }

            updateCanHistory();

            _skipAddToHistory = false;
        }
        public void HistoryBack()
        {
            _skipAddToHistory = true;

            try
            {
                _redoHistory.Push(_currentPosition);
                double[] newPosition = _undoHistory.Pop();
                _currentPosition = newPosition;

                specChart.Resize(newPosition[0], newPosition[1]);
            }
            catch(Exception ex)
            {
                _skipAddToHistory = false;
                DialogBox.Show("Cannot perform specified navigation.", ex.Message, "Back", DialogBoxIcon.Warning);
            }      
        }
        public void HistoryForward()
        {
            _skipAddToHistory = true;

            try
            {
                _undoHistory.Push(_currentPosition);
                double[] newPosition = _redoHistory.Pop();
                _currentPosition = newPosition;

                specChart.Resize(newPosition[0], newPosition[1]);
            }
            catch (Exception ex)
            {
                _skipAddToHistory = false;
                DialogBox.Show("Cannot perform specified navigation.", ex.Message, "Forward", DialogBoxIcon.Warning);
            }      
        }
        public void Reset()
        {
            _skipAddToHistory = true;

            specChart.Reset();
        }
        private void updateCanHistory()
        {
            CanHistoryBack = _undoHistory.Count > 0;
            CanHistoryForward = _redoHistory.Count > 0;
        }
        #endregion
        public async Task CreatePreview()
        {
            if (imagePreview.Visibility == Visibility.Collapsed) imagePreview.Visibility = Visibility.Visible;

            double startMass = specChart.AxisXMinimum;
            double endMass = specChart.AxisXMaximim;

            await imagePreview.CreateImageAsync(startMass, endMass);
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key == Key.Back)
            {
                HistoryBack();
            }
            else if (e.Key == Key.PageUp)
            {
                HistoryForward();
            }
            else if (e.Key == Key.PageDown)
            {
                HistoryBack();
            }
            else e.Handled = false;
        }

        #region Printing
        public void Print()
        {
            PrintDialog pd = new PrintDialog();
            pd.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;

            if (pd.ShowDialog() != true) return;

            pd.PrintVisual(CreatePrintDocument(), Spectrum.Name);
            //pd.PrintDocument(((IDocumentPaginatorSource)CreateDocument()).DocumentPaginator, Spectrum.Name);
        }

        private Grid CreatePrintDocument()
        {
            Grid g = new Grid();
            //g.Width = 800;
            //g.Height = 600;

            RowDefinition row1 = new RowDefinition();
            row1.Height = GridLength.Auto;
            g.RowDefinitions.Add(row1);
            RowDefinition row2 = new RowDefinition();
            row2.Height = new GridLength(1.0, GridUnitType.Star);
            g.RowDefinitions.Add(row2);

            TextBlock tb = new TextBlock();
            tb.Text = Spectrum.Name;
            tb.FontSize = 18;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            g.Children.Add(tb);

            SpecChart s = new SpecChart();
            s.SetData(spectrum);
            s.Resize(specChart.AxisXMinimum, specChart.AxisXMaximim, 0, specChart.axisY.Maximum, false);
            g.Children.Add(s);
          
            Grid.SetRow(tb, 0);
            Grid.SetRow(s, 1);

            return g;
        }

        private FixedDocument CreateDocument()
        {
            FixedDocument fixedDoc = new FixedDocument();
            PageContent pageContent = new PageContent();
            FixedPage fixedPage = new FixedPage();

            Grid g = new Grid();
            //g.Width = 800;
            //g.Height = 600;

            RowDefinition row1 = new RowDefinition();
            row1.Height = GridLength.Auto;
            g.RowDefinitions.Add(row1);
            RowDefinition row2 = new RowDefinition();
            row2.Height = new GridLength(1.0, GridUnitType.Star);
            g.RowDefinitions.Add(row2);

            TextBlock tb = new TextBlock();
            tb.Text = Spectrum.Name;
            tb.FontSize = 18;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            g.Children.Add(tb);

            SpecChart s = new SpecChart();
            s.SetData(spectrum);
            s.Resize(specChart.AxisXMinimum, specChart.AxisXMaximim, 0, specChart.axisY.Maximum, false);
            g.Children.Add(s);

            Grid.SetRow(tb, 0);
            Grid.SetRow(s, 1);

            fixedPage.Children.Add(g);
            ((System.Windows.Markup.IAddChild)pageContent).AddChild(fixedPage);
            fixedDoc.Pages.Add(pageContent);

            return fixedDoc;
        }
        #endregion
    }
}
