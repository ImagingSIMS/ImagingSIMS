using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ImagingSIMS.Data.Spectra;

namespace ImagingSIMS.Controls.BaseControls.SpectrumView
{
    /// <summary>
    /// Interaction logic for SpecChart.xaml
    /// </summary>
    public partial class SpecChart : UserControl
    {
        bool _firstResize;

        Color _lineColor;

        double _minX;
        double _maxX;
        double _minY;
        double _maxY;

        double _viewMinX;
        double _viewMaxX;
        double _viewMinY;
        double _viewMaxY;
        bool _isUpdatingYAxis;

        public double InitialMinX { get; private set; }
        public double InitialMaxX { get; private set; }
        public double InitialMinY { get; private set; }
        public double InitialMaxY { get; private set; }

        float[] _masses;
        uint[] _values;

        double _dataMin;
        double _dataMax;
        int _dataLength;
        Dictionary<int, int> _dataLookup;

        List<Point> _dataPoints;

        Point _selectStart;
        Point _selectEnd;
        bool _isSelecting;
        bool _isMassSelecting;

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title",
            typeof(string), typeof(SpecChart));
        public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register("SelectionStart",
            typeof(double), typeof(SpecChart));
        public static readonly DependencyProperty SelectionEndProperty = DependencyProperty.Register("SelectionEnd",
            typeof(double), typeof(SpecChart));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public double SelectionStart
        {
            get { return (double)GetValue(SelectionStartProperty); }
            set { SetValue(SelectionStartProperty, value); }
        }
        public double SelectionEnd
        {
            get { return (double)GetValue(SelectionEndProperty); }
            set { SetValue(SelectionEndProperty, value); }
        }

        public double AxisXMinimum { get { return axisX.Minimum; } }
        public double AxisXMaximim { get { return axisX.Maximum; } }

        public static readonly RoutedEvent RangeUpdatedEvent = EventManager.RegisterRoutedEvent("RangeUpdated", RoutingStrategy.Bubble,
            typeof(RangeUpdatedRoutedEventHandler), typeof(SpecChart));
        public static readonly RoutedEvent SelectionRangeUpdatedEvent = EventManager.RegisterRoutedEvent("SelectionRangeUpdated", RoutingStrategy.Bubble,
            typeof(RangeUpdatedRoutedEventHandler), typeof(SpecChart));

        public event RangeUpdatedRoutedEventHandler RangeUpdated
        {
            add { AddHandler(RangeUpdatedEvent, value); }
            remove { RemoveHandler(RangeUpdatedEvent, value); }
        }
        public event RangeUpdatedRoutedEventHandler SelectionRangeUpdated
        {
            add { AddHandler(SelectionRangeUpdatedEvent, value); }
            remove { RemoveHandler(SelectionRangeUpdatedEvent, value); }
        }

        public SpecChart()
        {
            _isUpdatingYAxis = false;
            Title = "";
            _lineColor = Color.FromArgb(255, 143, 0, 204);
            _isSelecting = false;
            _isMassSelecting = false;

            InitializeComponent();

            //axisX.Minimum = 0;
            //axisX.Maximum = 1000;
            axisX.Maximum = 1000;
            axisX.Minimum = 0;
            axisX.Title = "m/z";

            //axisY.Minimum = 0;
            //axisY.Maximum = 1000;
            axisY.Maximum = 1000;
            axisY.Minimum = 0;
            axisY.Title = "Intensity";

            _firstResize = true;
        }

        public void SetData(Spectrum Spectrum)
        {
            //_data = Spectrum.ToDoubleArray();
            _values = Spectrum.GetSpectrum(out _masses);

            if (Spectrum.Name != null && Spectrum.Name != "") Title = Spectrum.Name;

            _minX = _masses[0];
            _maxX = _masses[(_masses.Length) - 1];
            _minY = 0;
            _maxY = 0;

            _dataMin = _minX;
            _dataMax = _maxX;
            _dataLength = _masses.Length;
            _dataLookup = new Dictionary<int, int>();

            int counter = 0;
            for (int i = 0; i < _dataLength; i++)
            {
                double value = _values[i];
                double massValue = _masses[i];
                if (massValue >= counter)
                {
                    _dataLookup.Add(counter, i);
                    counter++;
                }
                if (value > _maxY) _maxY = value;
            }

            axisX.Minimum = _minX;
            axisX.Maximum = _maxX;
            axisY.Minimum = _minY;
            axisY.Maximum = _maxY;

            _viewMinX = _minX;
            _viewMaxX = _maxX;
            _viewMinY = _minY;
            _viewMaxY = _maxY;

            InitialMinX = _minX;
            InitialMaxX = _maxX;
            InitialMinY = _minY;
            InitialMaxY = _maxY;
        }
        public void Redraw()
        {
            Resize(_viewMinX, _viewMaxX, 0, _viewMaxY, false);
        }

        public void Reset()
        {
            Resize(InitialMinX, InitialMaxX, InitialMinY, InitialMaxY, false);
        }
        public void Resize(double MinimumX, double MaximumX)
        {
            Resize(MinimumX, MaximumX, _viewMinY, _viewMaxY, false);
        }
        public void Resize(double MinimumX, double MaximumX, double MinimumY, double MaximumY, bool IsKeyDown)
        {
            double prevXMax = axisX.Maximum;
            double prevXMin = axisX.Minimum;
            double prevYMax = axisY.Maximum;
            double prevYMin = axisY.Minimum;

            double xAxisWidth = axisX.ActualWidth;
            double yAxisHeight = axisY.ActualHeight;

            bool resizeXAxis = (MinimumX != prevXMin && MaximumX != prevXMax);
            if(resizeXAxis)
            {
                axisX.SetMinMax(MinimumX, MaximumX);
            }

            double xAxisRange = axisX.Maximum - axisX.Minimum;
            double yAxisRange = axisY.Maximum - axisY.Minimum;

            if (xAxisRange < 0.1d) return;

            if (chartArea.Children.Count > 0) chartArea.Children.Clear();

            int xPoints = (int)xAxisWidth;

            UInt64 counts = 0;

            ConcurrentDictionary<double, int> points = new ConcurrentDictionary<double, int>();            

            Parallel.For(0, xPoints, i =>
                {
                    if (i + 1 >= xPoints) return;
                    double valX = axisX.ValueFromCoordinate(i);
                    double valXp1 = axisX.ValueFromCoordinate(i + 1);

                    int startIndex = _dataLookup[(int)valX];
                    if (startIndex >= _masses.Length) return;
                    if (startIndex - 1 >= 0)
                    {
                        startIndex--;
                    }

                    double valueSum = 0;
                    int valueCount = 0;
                    for (int j = startIndex; j < _dataLength; j++)
                    {
                        double massValue = _masses[j];
                        if (massValue > valXp1) break;
                        if (massValue >= valX && massValue <= valXp1)
                        {
                            valueSum += _values[j];
                            valueCount++;
                        }
                    }

                    if (valueCount > 0) points.TryAdd(valX, (int)valueSum);

                    counts += (ulong)valueSum;
                });

            axisY.Minimum = 0;
            axisY.Maximum = MaximumY;

            double chartBottom = (double)chartArea.ActualHeight;

            Polyline pl = new Polyline();
            pl.Stroke = new SolidColorBrush(_lineColor);
            pl.StrokeThickness = 1;

            var sorted = from pair in points
                         orderby pair.Key ascending
                         select pair;

            foreach(KeyValuePair<double, int> pair in sorted)
            {
                Point p = new Point();

                p.X = ((pair.Key - MinimumX) * xAxisWidth) / xAxisRange;
                p.Y = (chartBottom - ((pair.Value / MaximumY) * yAxisHeight));

                if (p.Y < 5) p.Y = 5;

                pl.Points.Add(p);
            }
            chartArea.Children.Add(pl);

            _viewMinX = axisX.Minimum;
            _viewMinY = axisY.Minimum;
            _viewMaxX = axisX.Maximum;
            _viewMaxY = axisY.Maximum;

            labelCount.Content = string.Format("Counts: {0}", counts.ToString());

            if(resizeXAxis)
            {
                RaiseEvent(new RangeUpdatedRoutedEventArgs(axisX.Minimum, axisX.Maximum, 
                    prevXMin, prevXMax, IsKeyDown, RangeUpdatedEvent, this));
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            this.Focus();
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            chartArea.Children.Clear();
        }
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            if (!_firstResize)
            {
                Resize(_viewMinX, _viewMaxX, 0, _viewMaxY, false);
            }
            else
            {
                Resize(_viewMinX, _viewMaxX, 0, _viewMaxY, false);
            }
            _firstResize = false;
        }

        private void mouseArea_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Focus();
            gridLineX.Visibility = Visibility.Visible;
            gridLineY.Visibility = Visibility.Visible;
            labelCoord.Visibility = Visibility.Visible;
            Point p = e.GetPosition(mouseArea);
            labelCoord.Content = string.Format("X:{0} Y:{1}", axisX.ValueFromCoordinate((double)p.X).ToString("0.000"),
                    axisY.ValueFromCoordinate((double)(axisY.ActualHeight - p.Y)).ToString("0"));
            labelCoord.Margin = GetLabelPosition(labelCoord, p);
        }
        private void mouseArea_MouseLeave(object sender, MouseEventArgs e)
        {
            gridLineX.Visibility = Visibility.Collapsed;
            gridLineY.Visibility = Visibility.Collapsed;
            labelCoord.Visibility = Visibility.Collapsed;

            _isMassSelecting = false;
            _isSelecting = false;
        }
        private void mouseArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (gridLineX.Visibility == Visibility.Visible)
            {
                SelectionMouseMove(e);
            }
            else mouseSelection.Visibility = Visibility.Collapsed;

        }
        private void mouseArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UIElement element = (UIElement)sender;
            element.CaptureMouse();

            SelectionMouseClick(e);
        }
        private void mouseArea_MouseUp(object sender, MouseButtonEventArgs e)
        {
            UIElement element = (UIElement)sender;
            element.ReleaseMouseCapture();

            SelectionMouseUp(e);
        }
        private void mouseArea_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double yMax = getNewYMaxValue(e.Delta);

            if (yMax >= (double)Int32.MaxValue || yMax <= 50)
            {
                _isUpdatingYAxis = false;
                return;
            }

            Resize(axisX.Minimum, axisX.Maximum, 0, yMax, false);

            if (gridLineX.Visibility == Visibility.Visible)
            {
                Point p = e.GetPosition(mouseArea);

                gridLineX.Y1 = p.Y;
                gridLineX.Y2 = p.Y;
                gridLineY.X1 = p.X;
                gridLineY.X2 = p.X;

                labelCoord.Content = string.Format("X:{0} Y:{1}", axisX.ValueFromCoordinate((double)p.X).ToString("0.000"),
                    axisY.ValueFromCoordinate((double)(axisY.ActualHeight - p.Y)).ToString("0"));
                labelCoord.Margin = GetLabelPosition(labelCoord, p);
            }
        }

        private double getNewYMaxValue(double ticks)
        {
            double yMax = axisY.Maximum;
            if (ticks > 0)
            {
                yMax *= (9d / 10d);
            }
            else if (ticks < 0)
            {
                yMax *= (10d / 9d);
            }

            return yMax;
        }

        private void SelectionMouseClick(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                Style style = this.Resources["MassSelectRect"] as Style;
                mouseSelection.Style = style;
                _isMassSelecting = true;
                _selectStart = e.GetPosition(mouseArea);
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                Style style = this.Resources["ZoomSelectRect"] as Style;
                mouseSelection.Style = style;
                _isSelecting = true;
                _selectStart = e.GetPosition(mouseArea);
            }
        }
        private void SelectionMouseMove(MouseEventArgs e)
        {
            Point p = e.GetPosition(mouseArea);

            gridLineX.Y1 = p.Y;
            gridLineX.Y2 = p.Y;
            gridLineY.X1 = p.X;
            gridLineY.X2 = p.X;

            labelCoord.Content = string.Format("X:{0} Y:{1}", axisX.ValueFromCoordinate((double)p.X).ToString("0.000"),
                axisY.ValueFromCoordinate((double)(axisY.ActualHeight - p.Y)).ToString("0"));
            labelCoord.Margin = GetLabelPosition(labelCoord, p);

            if (_isSelecting || _isMassSelecting)
            {

                mouseSelection.Visibility = Visibility.Visible;
                double x1 = _selectStart.X;
                double x2 = p.X;
                double y1 = _selectStart.Y;
                double y2 = p.Y;

                double marginX = 0;
                double marginY = 0;
                if (x1 >= x2)
                {
                    marginX = x2;
                }
                else
                {
                    marginX = x1;
                }
                if (y1 >= y2)
                {
                    marginY = y2;
                }
                else
                {
                    marginY = y1;
                }

                mouseSelection.Margin = new Thickness(marginX, marginY, 0, 0);
                mouseSelection.Width = Math.Abs(x1 - x2);
                mouseSelection.Height = Math.Abs(y1 - y2);
            }
        }
        private void SelectionMouseUp(MouseEventArgs e)
        {
            mouseSelection.Visibility = Visibility.Collapsed;
            _selectEnd = e.GetPosition(mouseArea);

            double x1 = _selectStart.X;
            double x2 = _selectEnd.X;
            double y1 = _selectStart.Y;
            double y2 = _selectEnd.Y;

            double startX = 0;
            double endX = 0;
            double endY = 0;

            double yAxisHeight = axisY.ActualHeight;

            if (Math.Abs(x1 - x2) <= 5 || Math.Abs(y1 - y2) <= 5) return;

            if (x1 > x2)
            {
                startX = axisX.ValueFromCoordinate((double)x2);
                endX = axisX.ValueFromCoordinate((double)x1);
            }
            else
            {
                startX = axisX.ValueFromCoordinate((double)x1);
                endX = axisX.ValueFromCoordinate((double)x2);
            }
            if (y1 > y2)
            {
                endY = axisY.ValueFromCoordinate((double)(Math.Abs(yAxisHeight - y1)));
            }
            else
            {
                endY = axisY.ValueFromCoordinate((double)(Math.Abs(yAxisHeight - y2)));
            }

            if (endY <= 50) endY = 50;
            else if (endY >= Int32.MaxValue)
            {
                endY = ((double)Int32.MaxValue / 10f);
            }

            if (!Keyboard.IsKeyDown(Key.Escape))
            {
                if (_isSelecting)
                {
                    Resize((double)startX, (double)endX, 0, (double)endY, false);
                }
                else if (_isMassSelecting)
                {
                    MassSelect((double)startX, (double)endX);
                }
            }
            _isSelecting = false;
            _isMassSelecting = false;
        }

        private void MassSelect(double StartX, double EndX)
        {
            SelectionStart = StartX;
            SelectionEnd = EndX;

            RaiseEvent(new RangeUpdatedRoutedEventArgs(StartX, EndX, SelectionRangeUpdatedEvent, this));
        }

        private Thickness GetLabelPosition(Label Coord, Point P)
        {
            Thickness t = new Thickness(0);
            double labelWidth = (double)Coord.ActualWidth;
            if ((P.X + 3 + labelWidth) >= axisX.ActualWidth - 5)
            {
                t.Left = (P.X - 3 - labelWidth);
            }
            else
            {
                t.Left = (P.X + 3);
            }

            if ((P.Y - 20) <= 5)
            {
                t.Top = (P.Y + 3);
            }
            else
            {
                t.Top = (P.Y - 20);
            }

            return t;
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                Resize(_minX, _maxX, _minY, _maxY, false);
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                double axisRange = _viewMaxX - _viewMinX;
                double newMax = _viewMaxX - (axisRange * 0.1f);
                double newMin = _viewMinX - (axisRange * 0.1f);

                if (newMin <= _minX) return;
                if (newMax >= _maxX) return;
                Resize(newMin, newMax, 0, _viewMaxY, true);
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                double axisRange = _viewMaxX - _viewMinX;
                double newMax = _viewMaxX + (axisRange * 0.1f);
                double newMin = _viewMinX + (axisRange * 0.1f);

                if (newMin <= _minX) return;
                if (newMax >= _maxX) return;
                Resize(newMin, newMax, 0, _viewMaxY, true);
                e.Handled = true;
            }
            else if(e.Key == Key.Down)
            {
                int ticks = -30;
                double currentAxis = axisY.Maximum;
                double ratio = 1f + (ticks / 120.0f);

                double newYMax = axisY.Maximum / ratio;
                if (newYMax >= (double)Int32.MaxValue)
                {
                    _isUpdatingYAxis = false;
                    return;
                }
                if (newYMax <= 50)
                {
                    _isUpdatingYAxis = false;
                    return;
                }

                Resize(axisX.Minimum, axisX.Maximum, 0, newYMax, false);
            }
            else if (e.Key == Key.Up)
            {
                int ticks = 30;
                double currentAxis = axisY.Maximum;
                double ratio = 1f + (ticks / 120.0f);

                double newYMax = axisY.Maximum / ratio;
                if (newYMax >= (double)Int32.MaxValue)
                {
                    _isUpdatingYAxis = false;
                    return;
                }
                if (newYMax <= 50)
                {
                    _isUpdatingYAxis = false;
                    return;
                }

                Resize(axisX.Minimum, axisX.Maximum, 0, newYMax, false);
            }
        }

        #region AxisY Stylus Control
        bool _isAxisYStylusDown = false;
        Point _prevAxisYStylusPoint;

        private void axisY_StylusMove(object sender, StylusEventArgs e)
        {
            if (!_isAxisYStylusDown) return;

            Point currentStylusPoint = e.GetPosition(axisY);

            double delta = currentStylusPoint.Y - _prevAxisYStylusPoint.Y;
            double currentAxis = axisY.Maximum;
            double ratio = 1f - (delta / 60.0f);

            double newYMax = axisY.Maximum / ratio;
            if (newYMax >= (double)Int32.MaxValue)
            {
                _isUpdatingYAxis = false;
                return;
            }
            if (newYMax <= 50)
            {
                _isUpdatingYAxis = false;
                return;
            }

            Resize(axisX.Minimum, axisX.Maximum, 0, newYMax, false);

            if (gridLineX.Visibility == Visibility.Visible)
            {
                Point p = e.GetPosition(mouseArea);

                gridLineX.Y1 = p.Y;
                gridLineX.Y2 = p.Y;
                gridLineY.X1 = p.X;
                gridLineY.X2 = p.X;

                labelCoord.Content = string.Format("X:{0} Y:{1}", axisX.ValueFromCoordinate((double)p.X).ToString("0.000"),
                    axisY.ValueFromCoordinate((double)(axisY.ActualHeight - p.Y)).ToString("0"));
                labelCoord.Margin = GetLabelPosition(labelCoord, p);
            }
            _prevAxisYStylusPoint = currentStylusPoint;
        }
        private void axisY_StylusDown(object sender, StylusDownEventArgs e)
        {
            _isAxisYStylusDown = true;
            _prevAxisYStylusPoint = e.GetPosition(axisY);
        }
        private void axisY_StylusUp(object sender, StylusEventArgs e)
        {
            _isAxisYStylusDown = false;
        }
        #endregion
    }

    public delegate void RangeUpdatedRoutedEventHandler(object sender, RangeUpdatedRoutedEventArgs e);
    public class RangeUpdatedRoutedEventArgs : RoutedEventArgs
    {
        public double LastMassStart { get; private set; }
        public double LastMassEnd { get; private set; }
        public double MassStart { get; private set; }
        public double MassEnd { get; private set; }
        public bool IsKeyDown { get; private set; }

        public RangeUpdatedRoutedEventArgs(double MassStart, double MassEnd,
            RoutedEvent Event, object Source)
            : base(Event, Source)
        {
            this.MassStart = MassStart;
            this.MassEnd = MassEnd;
        }
        public RangeUpdatedRoutedEventArgs(double MassStart, double MassEnd, bool IsKeyDown,
            RoutedEvent Event, object Source)
            : base(Event, Source)
        {
            this.MassStart = MassStart;
            this.MassEnd = MassEnd;
            this.IsKeyDown = IsKeyDown;
        }
        public RangeUpdatedRoutedEventArgs(double MassStart, double MassEnd, double LastMassStart, double LastMassEnd,
            bool IsKeyDown, RoutedEvent Event, object Source)
            : base(Event, Source)
        {
            this.LastMassStart = LastMassStart;
            this.LastMassEnd = LastMassEnd;
            this.MassStart = MassStart;
            this.MassEnd = MassEnd;
            this.IsKeyDown = IsKeyDown;
        }
        public RangeUpdatedRoutedEventArgs(double MassStart, double MassEnd, double LastMassStart, double LastMassEnd,
            RoutedEvent Event, object Source)
            : base(Event, Source)
        {
            this.LastMassStart = LastMassStart;
            this.LastMassEnd = LastMassEnd;
            this.MassStart = MassStart;
            this.MassEnd = MassEnd;
        }
    }
}
