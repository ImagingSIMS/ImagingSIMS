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
using ImagingSIMS.Data;
using ImagingSIMS.Data.Spectra;

namespace ImagingSIMS.Controls.Tabs
{
    /// <summary>
    /// Interaction logic for SpectrumCropTab.xaml
    /// </summary>
    public partial class SpectrumCropTab : UserControl
    {
        bool _isHighlighting;
        double scale;

        Point? lastCenterPositionOnTarget;
        Point? lastMousePositionOnTarget;
        Point? lastDragPoint;

        public static readonly DependencyProperty TransformedWidthProperty = DependencyProperty.Register("TransformedWidth",
            typeof(double), typeof(SpectrumCropTab));
        public static readonly DependencyProperty TransformedHeightProperty = DependencyProperty.Register("TransformedHeight",
            typeof(double), typeof(SpectrumCropTab));
        public static readonly DependencyProperty CropAllLayersProperty = DependencyProperty.Register("CropAllLayers",
            typeof(bool), typeof(SpectrumCropTab));
        public static readonly DependencyProperty ActiveLayerProperty = DependencyProperty.Register("ActiveLayer",
            typeof(int), typeof(SpectrumCropTab));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
            typeof(string), typeof(SpectrumCropTab));
        public static readonly DependencyProperty Text2Property = DependencyProperty.Register("Text2",
            typeof(string), typeof(SpectrumCropTab));
        public static readonly DependencyProperty Text3Property = DependencyProperty.Register("Text3",
            typeof(string), typeof(SpectrumCropTab));
        public static readonly DependencyProperty BrushSizeProperty = DependencyProperty.Register("BrushSize",
            typeof(int), typeof(SpectrumCropTab));
        public static readonly DependencyProperty ShowLivePreviewProperty = DependencyProperty.Register("ShowLivePreview",
            typeof(bool), typeof(SpectrumCropTab));
        public static readonly DependencyProperty ResizeCroppedProperty = DependencyProperty.Register("ResizeCropped",
            typeof(bool), typeof(SpectrumCropTab));
        public static readonly DependencyProperty ResizeBufferProperty = DependencyProperty.Register("ResizeBuffer",
            typeof(int), typeof(SpectrumCropTab), new FrameworkPropertyMetadata(2));
        public static readonly DependencyProperty CropRectangleProperty = DependencyProperty.Register("CropRectangle",
            typeof(string), typeof(SpectrumCropTab));

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
        public bool CropAllLayers
        {
            get { return (bool)GetValue(CropAllLayersProperty); }
            set { SetValue(CropAllLayersProperty, value); }
        }
        public int ActiveLayer
        {
            get { return (int)GetValue(ActiveLayerProperty); }
            set { SetValue(ActiveLayerProperty, value); }
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
        public string Text3
        {
            get { return (string)GetValue(Text3Property); }
            set { SetValue(Text3Property, value); }
        }
        public int BrushSize
        {
            get { return (int)GetValue(BrushSizeProperty); }
            set { SetValue(BrushSizeProperty, value); }
        }
        public bool ShowLivePreview
        {
            get { return (bool)GetValue(ShowLivePreviewProperty); }
            set { SetValue(ShowLivePreviewProperty, value); }
        }
        public bool ResizeCropped
        {
            get { return (bool)GetValue(ResizeCroppedProperty); }
            set { SetValue(ResizeCroppedProperty, value); }
        }
        public int ResizeBuffer
        {
            get { return (int)GetValue(ResizeBufferProperty); }
            set { SetValue(ResizeBufferProperty, value); }
        }
        public string CropRectangle
        {
            get { return (string)GetValue(CropRectangleProperty);}
            set { SetValue(CropRectangleProperty, value); }
        }

        public SpectrumCropTab()
        {
            BrushSize = 1;
            CropAllLayers = true;
            ActiveLayer = 1;
            ShowLivePreview = true;

            InitializeComponent();
        }

        #region Designer Properties
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
        #endregion

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
                xPixel, yPixel, e.GetPosition(scrollViewer).X.ToString("0.0"), e.GetPosition(scrollViewer).Y.ToString("0.0"));
            Text3 = string.Format("Pixels highlighted: {0}", imagePreview.HighlightedPoints.Count);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdatePreview();
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }
        private void listViewSpectra_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
        }

        Spectrum _lastViewed;
        private void UpdatePreview()
        {
            if (ShowLivePreview != true) return;

            Spectrum s = (Spectrum)listViewSpectra.SelectedItem;
            if (s == null) return;

            if (_lastViewed != null && _lastViewed == s) return;

            _lastViewed = s;

            Mouse.OverrideCursor = Cursors.Wait;
            Data2D d = new Data2D(0, 0);
            if(CropAllLayers)
            {
                float max = 0;
                d = s.FromMassRange(new MassRangePair(s.StartMass, s.EndMass), out max);
            }
            else
            {
                int layer = ActiveLayer - 1;

                if (layer >= s.SizeZ)
                {
                    ActiveLayer = 1;
                    layer = ActiveLayer - 1;
                }
                d = s.FromMassRange(new MassRangePair(s.StartMass, s.EndMass), ActiveLayer - 1, "Preview", true);
            }
            imagePreview.ChangeDataSource(d);
            Text3 = string.Format("Pixels highlighted: {0}", imagePreview.HighlightedPoints.Count);
            //imagePreview.Data2DSource = d;

            scale = 1;

            TransformedWidth = scrollViewer.Width;
            TransformedHeight = scrollViewer.Height;

            Mouse.OverrideCursor = Cursors.Arrow;
        }

        ProgressWindow pw;
        private void buttonCommit_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            if (btn == btnBrushCommit)
                commitBrush();
            else if (btn == btnRectCommit)
                commitRect();
        }

        void commitBrush()
        {
            List<Point> highlightedPoints = imagePreview.HighlightedPoints;
            Spectrum toCrop = (Spectrum)listViewSpectra.SelectedItem;
            if (toCrop == null || highlightedPoints == null) return;

            ObservableCollection<Spectrum> spectra = listViewSpectra.ItemsSource as ObservableCollection<Spectrum>;

            int layer = -1;
            if (!CropAllLayers) layer = ActiveLayer - 1;

            string v2Path = "";

            if (toCrop.SpectrumType == SpectrumType.J105)
            {
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.Filter = "Ionoptika compressed V2 files (.IonoptikaIA2DspectrV2)|*.IonoptikaIA2DspectrV2";
                Nullable<bool> result = sfd.ShowDialog();
                if (result != true)
                {
                    return;
                }
                v2Path = sfd.FileName;
            }

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = false;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.ProgressChanged += bw_ProgressChanged;
            bw.DoWork += bw_DoWork;

            pw = new ProgressWindow("Performing crop. Please wait.", "Crop", true);
            pw.Show();

            //args[0] = (Spectrum) spectrum to crop
            //args[1] = (int) layer to crop
            //args[2] = (List<Point>) highlighted points
            //args[3] = (string) J105 V2 path to create
            //args[4] = (bool) resize spectrum
            //args[5] = (int) resize buffer

            object[] args = new object[6]
            {
                toCrop, layer, highlightedPoints, v2Path, ResizeCropped, ResizeBuffer
            };

            bw.RunWorkerAsync(args);
        }
        void commitRect()
        {
            if (string.IsNullOrEmpty(CropRectangle))
            {
                Dialog.Show("No coordinates entered.", 
                    "List the rectangle coordiantes separated by a comma and try again.", "Crop", DialogIcon.Stop);
                return;
            }
            //Parse coordinates first and create pixels to keep
            string[] parts = CropRectangle.Split(',');
            if (parts.Length != 4)
            {
                DialogBox.Show("Incorrect number of parameters.",
                    "Please enter four coordinates in the format Start X,End X,StartY,End Y", "Crop", DialogIcon.Error);
                return;
            }

            int startX = 0;
            int endX = 0;
            int startY = 0;
            int endY = 0;

            try
            {
                startX = int.Parse(parts[0]);
                endX = int.Parse(parts[1]);
                startY = int.Parse(parts[2]);
                endY = int.Parse(parts[3]);
            }
            catch(Exception)
            {
                DialogBox.Show("Could not parse rectangle coordinates.",
                    "Please enter four coordinates in the format Start X,End X,StartY,End Y", "Crop", DialogIcon.Error);
                return;
            }

            if (startX >= endX)
            {
                DialogBox.Show("Invalid X coordinates.",
                    "End X coordinate is less than or equal to start X coordinate.", "Crop", DialogIcon.Error);
                return;
            }
            if (startY >= endY)
            {
                DialogBox.Show("Invalid Y coordinates.",
                    "End Y coordinate is less than or equal to start Y coordinate.", "Crop", DialogIcon.Error);
                return;
            }

            List<Point> highlightedPoints = new List<Point>();

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    highlightedPoints.Add(new Point(x, y));
                }
            }

            Spectrum toCrop = (Spectrum)listViewSpectra.SelectedItem;
            if (toCrop == null || highlightedPoints == null) return;

            ObservableCollection<Spectrum> spectra = listViewSpectra.ItemsSource as ObservableCollection<Spectrum>;

            int layer = -1;
            if (!CropAllLayers) layer = ActiveLayer - 1;

            string v2Path = "";

            if (toCrop.SpectrumType == SpectrumType.J105)
            {
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.Filter = "Ionoptika compressed V2 files (.IonoptikaIA2DspectrV2)|*.IonoptikaIA2DspectrV2";
                Nullable<bool> result = sfd.ShowDialog();
                if (result != true)
                {
                    return;
                }
                v2Path = sfd.FileName;
            }

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = false;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.ProgressChanged += bw_ProgressChanged;
            bw.DoWork += bw_DoWork;

            pw = new ProgressWindow("Performing crop. Please wait.", "Crop", true);
            pw.Show();

            //args[0] = (Spectrum) spectrum to crop
            //args[1] = (int) layer to crop
            //args[2] = (List<Point>) highlighted points
            //args[3] = (string) J105 V2 path to create
            //args[4] = (bool) resize spectrum
            //args[5] = (int) resize buffer

            object[] args = new object[6]
            {
                toCrop, layer, highlightedPoints, v2Path, ResizeCropped, ResizeBuffer
            };

            bw.RunWorkerAsync(args);
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            //args[0] = (Spectrum) spectrum to crop
            //args[1] = (int) layer to crop
            //args[2] = (List<Point>) highlighted points
            //args[3] = (string) J105 V2 path to create
            //args[4] = (bool) resize spectrum
            //args[5] = (int) resize buffer

            Spectrum toCrop = (Spectrum)((object[])e.Argument)[0];
            int layer = (int)((object[])e.Argument)[1];
            List<Point> highlightedPoints = (List<Point>)((object[])e.Argument)[2];
            string v2Path = (string)((object[])e.Argument)[3];
            bool resize = (bool)((object[])e.Argument)[4];
            int resizebuffer = (int)((object[])e.Argument)[5];

            if (toCrop.SpectrumType == SpectrumType.BioToF)
            {
                try
                {
                    BioToFSpectrum cropped;

                    if (resize) cropped = ((BioToFSpectrum)toCrop).CropAndResize(highlightedPoints, layer, resizebuffer);
                    else cropped = ((BioToFSpectrum)toCrop).Crop(highlightedPoints, layer);
                    e.Result = cropped;
                }
                catch (Exception ex)
                {
                    e.Result = ex;
                }
            }
            else if (toCrop.SpectrumType == SpectrumType.J105)
            {
                try
                {
                    J105Spectrum cropped;

                    if (resize) cropped = ((J105Spectrum)toCrop).CropAndResize(highlightedPoints, layer, v2Path, resizebuffer);
                    else cropped = ((J105Spectrum)toCrop).Crop(highlightedPoints, layer, v2Path);

                    e.Result = cropped;
                }
                catch (Exception ex)
                {
                    e.Result = ex;
                }
            }
            else if(toCrop.SpectrumType == SpectrumType.Cameca1280)
            {
                try
                {
                    Cameca1280Spectrum cropped;

                    if (resize) cropped = ((Cameca1280Spectrum)toCrop).CropAndResize(highlightedPoints, layer, resizebuffer);
                    else cropped = ((Cameca1280Spectrum)toCrop).Crop(highlightedPoints, layer);

                    e.Result = cropped;
                }
                catch(Exception ex)
                {
                    e.Result = ex;
                }
            }
        }
        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
        }
        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (pw != null)
            {
                pw.Close();
                pw = null;
            }

            if (e.Result == null)
            {
                DialogBox db = new DialogBox("There was an error cropping the spectrum.", "No return value from the BackgroundWorker",
                    "Crop", DialogIcon.Error);
                db.ShowDialog();
                return;
            }
            Spectrum cropped = e.Result as Spectrum;
            if (cropped == null)
            {
                Exception ex = e.Result as Exception;
                if (ex != null)
                {
                    string message = ex.Message;
                    if (ex.InnerException != null) message += "\n" + ex.InnerException.Message;

                    DialogBox db = new DialogBox("There was an error cropping the spectrum.", message,
                        "Crop", DialogIcon.Error);
                    db.ShowDialog();
                    return;
                }
                else
                {
                    DialogBox db = new DialogBox("There was an error cropping the spectrum.", "No return Spectrum and no Exception thrown.",
                        "Crop", DialogIcon.Error);
                    db.ShowDialog();
                    return;
                }
            }

            try
            {
                ObservableCollection<Spectrum> spectra = listViewSpectra.ItemsSource as ObservableCollection<Spectrum>;
                spectra.Add(cropped);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null) message += "\n" + ex.InnerException.Message;

                DialogBox db = new DialogBox("There was an error addind the spectrum to the collection.", message,
                    "Crop", DialogIcon.Error);
                db.ShowDialog();
                return;
            }
        }

        void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            imagePreview.ClearPoints();
        }

        public bool DropMask(bool[,] maskData)
        {
            int width = maskData.GetLength(0);
            int height = maskData.GetLength(1);

            Data2D d = imagePreview.DataSource;
            if (d == null)
            {
                DialogBox.Show("No spectrum selected.", 
                    "Select a mass spectrum and generate a preview image and try again.", "Mask Drop", DialogIcon.Error);
                return false;
            }

            if (d.Width != width || d.Height != height)
            {
                DialogBox.Show("Invalid mask dimensions.", 
                    "The mask width and height does not match the selected spectrum dimensions.", "Mask Drop", DialogIcon.Error);
                return false;
            }

            try
            {
                imagePreview.SetMaskPoints(maskData);
                return true;
            }
            catch(Exception ex)
            {
                DialogBox.Show("Could not drop mask data.", ex.Message, "Mask Drop", DialogIcon.Error);
                return false;
            }
        }
    }
}
