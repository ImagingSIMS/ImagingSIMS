using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using ImagingSIMS.Common;
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Controls.Tabs;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;

namespace ImagingSIMS.Controls.Tabs
{
    /// <summary>
    /// Interaction logic for DataCorrectionTab.xaml
    /// </summary>
    public partial class DataCorrectionTab : UserControl
    {
        bool _isHighlighting;
        double scale;

        Point? lastCenterPositionOnTarget;
        Point? lastMousePositionOnTarget;
        Point? lastDragPoint;

        public static readonly DependencyProperty TransformedWidthProperty = DependencyProperty.Register("TransformedWidth",
            typeof(double), typeof(DataCorrectionTab));
        public static readonly DependencyProperty TransformedHeightProperty = DependencyProperty.Register("TransformedHeight",
            typeof(double), typeof(DataCorrectionTab));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
            typeof(string), typeof(DataCorrectionTab));
        public static readonly DependencyProperty Text2Property = DependencyProperty.Register("Text2",
            typeof(string), typeof(DataCorrectionTab));
        public static readonly DependencyProperty BrushSizeProperty = DependencyProperty.Register("BrushSize",
            typeof(int), typeof(DataCorrectionTab));
        public static readonly DependencyProperty CorrectionMethodProperty = DependencyProperty.Register("CorrectionMethod",
            typeof(CorrectionOperationMethod), typeof(DataCorrectionTab));

        public double TransformedWidth
        {
            get { return (double)GetValue(TransformedWidthProperty); }
            set { SetValue(TransformedWidthProperty, value); }
        }
        public double TransformedHeight
        {
            get { return (double)GetValue(TransformedHeightProperty); }
            set { SetValue(TransformedHeightProperty, value); }
        }
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public string Text2
        {
            get { return (string)GetValue(Text2Property); }
            set { SetValue(Text2Property, value); }
        }
        public int BrushSize
        {
            get { return (int)GetValue(BrushSizeProperty); }
            set { SetValue(BrushSizeProperty, value); }
        }
        public CorrectionOperationMethod CorrectionMethod
        {
            get { return (CorrectionOperationMethod)GetValue(CorrectionMethodProperty); }
            set { SetValue(CorrectionMethodProperty, value); }
        }

        public DataCorrectionTab()
        {
            InitializeComponent();

            BrushSize = 1;
            CorrectionMethod = CorrectionOperationMethod.Zero;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!lastMousePositionOnTarget.HasValue)
                {
                    if (lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow = scrollViewer.TranslatePoint(centerOfViewport, imagePreview);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(imagePreview);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / imagePreview.Width;
                    double multiplicatorY = e.ExtentHeight / imagePreview.Height;

                    double newOffsetX = scrollViewer.HorizontalOffset - dXInTargetPixels * multiplicatorX;
                    double newOffsetY = scrollViewer.VerticalOffset - dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            if (scrollViewer == null) return;

            lastMousePositionOnTarget = Mouse.GetPosition(imagePreview);

            double originalWidth = imagePreview.ActualWidth / scale;
            double originalHeight = imagePreview.ActualHeight / scale;

            if (e.Delta > 0)
            {
                scale += 0.25f;
            }
            if (e.Delta < 0)
            {
                scale -= 0.25f;
            }

            if (scale < 1) scale = 1;

            TransformedWidth = originalWidth * scale;
            TransformedHeight = originalHeight * scale;

            var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, imagePreview);
        }

        private void scrollViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isHighlighting = true;

            Data2D d = imagePreview.DataSource;
            if (d != null)
            {
                int width = d.Width;
                int height = d.Height;

                int xPixel = (int)(((e.GetPosition(scrollViewer).X + scrollViewer.HorizontalOffset) * (width / scrollViewer.ActualWidth)) / scale);
                int yPixel = (int)(((e.GetPosition(scrollViewer).Y + scrollViewer.VerticalOffset) * (height / scrollViewer.ActualHeight)) / scale);

                Text2 = string.Format("Highlighting X:{0} Y:{1}", xPixel, yPixel);
                if (xPixel < 0 || xPixel >= d.Width || yPixel < 0 || yPixel >= d.Height) return;

                imagePreview.Highlight(xPixel, yPixel, BrushSize);
            }
        }
        private void scrollViewer_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(scrollViewer);
            if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y < scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
            {
                scrollViewer.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                Mouse.Capture(scrollViewer);
            }
        }

        private void scrollViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isHighlighting = false;
        }
        private void scrollViewer_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Arrow;
            scrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
        }

        private void ScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            int xPixel = 0;
            int yPixel = 0;

            Data2D d = imagePreview.DataSource;
            if (d != null)
            {
                int width = d.Width;
                int height = d.Height;

                xPixel = (int)(((e.GetPosition(scrollViewer).X + scrollViewer.HorizontalOffset) * (width / scrollViewer.ActualWidth)) / scale);
                yPixel = (int)(((e.GetPosition(scrollViewer).Y + scrollViewer.VerticalOffset) * (height / scrollViewer.ActualHeight)) / scale);

                if (_isHighlighting)
                {
                    if (xPixel < 0 || xPixel >= d.Width || yPixel < 0 || yPixel >= d.Height) return;

                    Text2 = string.Format("Highlighting X:{0} Y:{1}", xPixel, yPixel);
                    imagePreview.Highlight(xPixel, yPixel, BrushSize);  
                }
            }

            if (lastDragPoint.HasValue)
            {
                Point posNow = e.GetPosition(scrollViewer);

                double dX = posNow.X - lastDragPoint.Value.X;
                double dY = posNow.Y - lastDragPoint.Value.Y;

                lastDragPoint = posNow;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
            }
            
            Text = string.Format("Data Position X:{0} Y:{1}\nControl Position X:{2} Y:{3}", 
                xPixel, yPixel, e.GetPosition(scrollViewer).X, e.GetPosition(scrollViewer).Y);
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            Data2D d = AvailableHost.AvailableTablesSource.GetSelectedTables().FirstOrDefault();
            if (d == null) return;

            imagePreview.ChangeDataSource(d);

            scale = 1;

            TransformedWidth = scrollViewer.Width;
            TransformedHeight = scrollViewer.Height;
        }

        private void buttonCommit_Click(object sender, RoutedEventArgs e)
        {
            Data2D d = imagePreview.DataSource;
            if (d == null)
            {
                DialogBox db = new DialogBox("No data table selected.", "Use the ListBox to select a data table to correct.",
                    "Correction", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            Data2D corrected;

            try
            {
                imagePreview.CommitChanges(CorrectionOperations.GetOperation(CorrectionMethod));
                corrected = imagePreview.DataSource;
            }
            catch (Exception ex)
            {
                DialogBox db = new DialogBox("Could not perform the correction.", ex.Message,
                       "Correction", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            AvailableHost.AvailableTablesSource.ReplaceTable(d, corrected);

            imagePreview.ChangeDataSource(corrected);

            ClosableTabItem.SendStatusUpdate(this, "Data correction complete.");
        }
        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            imagePreview.ClearPoints();
        }
    }
}
