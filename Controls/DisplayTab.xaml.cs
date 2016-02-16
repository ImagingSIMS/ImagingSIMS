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

using Microsoft.Win32;

using ImagingSIMS.Common;
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for DisplayTab.xaml
    /// </summary>
    public partial class DisplayTab : UserControl, IHistory
    {
        ProgressWindow pw;

        DisplaySeries _images;

        Stack<DisplaySeries> _undoHistory;
        Stack<DisplaySeries> _redoHistory;

        public DisplaySeries CurrentSeries
        {
            get { return _images; }
            set 
            { 
                _images = value;
                ItemsControl.ItemsSource = CurrentSeries.Images;
            }
        }
        public ListBox ItemsControl
        {
            get { return itemsControl; }
        }

        public bool CanUndo()
        {
            if (_undoHistory == null) return false;
            return _undoHistory.Count > 0;
        }
        public bool CanRedo()
        {
            if (_redoHistory == null) return false;
            return _redoHistory.Count > 0;
        }

        public static readonly DependencyProperty BackColorProperty = DependencyProperty.Register("BackColor",
            typeof(Color), typeof(DisplayTab));

        public Color BackColor
        {
            get { return (Color)GetValue(BackColorProperty); }
            set { SetValue(BackColorProperty, value); }
        }

        public DisplayTab()
        {
            InitializeComponent();

            CurrentSeries = new DisplaySeries();

            BackColor = Color.FromArgb(255, 0, 0, 0);

            _redoHistory = new Stack<DisplaySeries>();
            _undoHistory = new Stack<DisplaySeries>();
        }

        public void ChangeSeries(DisplaySeries Series)
        {
            _redoHistory.Clear();
            _undoHistory.Push(CurrentSeries);
            CurrentSeries = Series;
        }
        public void Undo()
        {
            DisplaySeries series = _undoHistory.Pop();
            _redoHistory.Push(CurrentSeries);
            CurrentSeries = series;
        }
        public void Redo()
        {
            DisplaySeries series = _redoHistory.Pop();
            _undoHistory.Push(CurrentSeries);
            CurrentSeries = series;
        }

        private DisplayImage GetImage(Button Source)
        {
            StackPanel s1 = (StackPanel)Source.Parent;
            if (s1 == null) return null;

            StackPanel s2 = (StackPanel)s1.Parent;
            if (s2 == null) return null;

            Grid g = (Grid)s2.Parent;
            if (g == null) return null;

            foreach (object o in g.Children)
            {
                if (o is ContentControl)
                {
                    DisplayImage image = ((ContentControl)o).Content as DisplayImage;
                    if (image == null) continue;

                    return image;
                }
            }

            return null;
        }
        
        private void DockPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DockPanel dp = (DockPanel)sender;
                if (dp == null) return;

                if (dp.Children == null || dp.Children.Count == 0) return;

                Grid g = (Grid)dp.Children[0];
                if (g == null) return;

                foreach (object o in g.Children)
                {
                    if (o is ContentControl)
                    {
                        DisplayImage image = ((ContentControl)o).Content as DisplayImage;
                        if (image == null) continue;

                        DataObject obj = new DataObject("DisplayImage", image);
                        DragDrop.DoDragDrop(this, obj, DragDropEffects.Copy);
                        return;
                    }
                }
            }
            else
            {
                base.OnMouseMove(e);
            }
        }
        private void contentButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            DisplayImage image = GetImage(sender as Button);
            if (image == null) return;

            if (!CurrentSeries.Images.Contains(image))
            {
                DialogBox db = new DialogBox("Could not remove the image from the collection.", "Image not found.", "Remove",
                    DialogBoxIcon.WarningGray);
                db.ShowDialog();
                return;
            }
            try
            {
                CurrentSeries.Images.Remove(image);
            }
            catch (Exception ex)
            {
                DialogBox db = new DialogBox("Could not remove the image from the collection.", ex.Message, "Remove",
                       DialogBoxIcon.WarningGray);
                db.ShowDialog();
            }
        }
        private void contentButtonSave_Click(object sender, RoutedEventArgs e)
        {
            DisplayImage image = GetImage(sender as Button);
            if (image == null) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap Images (.bmp)|*.bmp";
            Nullable<bool> result = sfd.ShowDialog();
            if (result != true) return;

            DialogBox db;
            try
            {
                SaveImage(image, sfd.FileName);
            }
            catch (Exception ex)
            {
                db = new DialogBox("Could not save the image.", ex.Message, "Save", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            db = new DialogBox("Image saved successfully!", sfd.FileName, "Save", DialogBoxIcon.Ok);
            db.ShowDialog();
        }
        private void contentButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            DisplayImage image = GetImage(sender as Button);
            if (image == null) return;

            BitmapSource bs = image.Source as BitmapSource;
            if (bs == null) throw new ArgumentException("Invalid ImageSource");

            Clipboard.SetImage(bs);
        }

        private void SaveImage(DisplayImage image, string path)
        {
            BitmapSource bs = image.Source as BitmapSource;
            if (bs == null) throw new ArgumentException("Invalid ImageSource");

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bs));

            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
        private void CopyImage(DisplayImage image)
        {
            BitmapSource bs = image.Source as BitmapSource;
            if (bs == null) throw new ArgumentException("Invalid ImageSource");

            Clipboard.SetImage(bs);
        }
        private void RotateImage(DisplayImage image, bool clockwise)
        {
            BitmapSource bs = (BitmapSource)image.Source;
            TransformedBitmap transformedImage = new TransformedBitmap();
            transformedImage.BeginInit();
            transformedImage.Source = bs;
            if (clockwise) transformedImage.Transform = new RotateTransform(90);
            else transformedImage.Transform = new RotateTransform(270);
            transformedImage.EndInit();

            image.Source = transformedImage;
        }
        private void FlipImage(DisplayImage image, bool horizontal)
        {
            BitmapSource bs = (BitmapSource)image.Source;
            TransformedBitmap transformedImage = new TransformedBitmap();
            transformedImage.BeginInit();
            transformedImage.Source = bs;
            if (horizontal) transformedImage.Transform = new ScaleTransform(-1, 1);
            else transformedImage.Transform = new ScaleTransform(1, -1);
            transformedImage.EndInit();

            image.Source = transformedImage;
        }
        private void ImageToRGB(DisplayImage image)
        {
            BitmapSource bs = (BitmapSource)image.Source;
            if (bs.Format == PixelFormats.Bgra32)
            {
                int width = (int)bs.Width;
                int height = (int)bs.Height;

                PixelFormat pf = PixelFormats.Bgr24;

                int rawStride = (width * pf.BitsPerPixel) / 8;

                byte[] source = new byte[height * ((width * PixelFormats.Bgra32.BitsPerPixel) / 8)];
                bs.CopyPixels(source, (width * PixelFormats.Bgra32.BitsPerPixel) / 8, 0);
                byte[] rawImage = new byte[height * rawStride];

                for (int i = 0; i < width * height; i++)
                {
                    int sourceStart = i * 4;
                    int destStart = i * 3;

                    byte b = source[sourceStart + 0];
                    byte g = source[sourceStart + 1];
                    byte r = source[sourceStart + 2];
                    byte a = source[sourceStart + 3];

                    rawImage[destStart + 0] = (byte)((float)b * (float)a / 255f);
                    rawImage[destStart + 1] = (byte)((float)g * (float)a / 255f);
                    rawImage[destStart + 2] = (byte)((float)r * (float)a / 255f);
                }

                BitmapSource converted = BitmapSource.Create(width,
                    height, 96, 96, pf, null, rawImage, rawStride);

                image.Source = converted;
            }
        }

        public void CallEvent(ImageTabEvent EventType)
        {
            switch (EventType)
            {
                case ImageTabEvent.Copy:
                    Copy();
                    break;
                case ImageTabEvent.FlipHorizontal:
                    Flip(true);
                    break;
                case ImageTabEvent.FlipVertical:
                    Flip(false);
                    break;
                case ImageTabEvent.RotateClock:
                    Rotate(true);
                    break;
                case ImageTabEvent.RotateCounter:
                    Rotate(false);
                    break;
                case ImageTabEvent.ToRGB:
                    ToRGB();
                    break;
                case ImageTabEvent.FilterHighPass:
                    DoFilter(ImageTabEvent.FilterHighPass);
                    break;
                case ImageTabEvent.FilterLowPass:
                    DoFilter(ImageTabEvent.FilterLowPass);
                    break;
                case ImageTabEvent.FilterMean:
                    DoFilter(ImageTabEvent.FilterMean);
                    break;
                case ImageTabEvent.FilterMedian:
                    DoFilter(ImageTabEvent.FilterMedian);
                    break;
                case ImageTabEvent.FilterGaussian:
                    DoFilter(ImageTabEvent.FilterGaussian);
                    break;
                case ImageTabEvent.Save:
                    Save();
                    break;
                case ImageTabEvent.Overlay:
                    DoOverlay();
                    break;
                case ImageTabEvent.Resize:
                    DoResize();
                    break;
            }
        }
        public void CallEvent(ImageTabEvent EventType, int Parameter)
        {
            switch (EventType)
            {
                case ImageTabEvent.SliceXZ:
                    DoSlice(EventType, Parameter);
                    break;
                case ImageTabEvent.SliceYZ:
                    DoSlice(EventType, Parameter);
                    break;
            }
        }

        private void Copy()
        {
            if (itemsControl.SelectedItems.Count == 0)
            {
                DialogBox db = new DialogBox("No images selected.", "Select one or more images to copy and try again.",
                    "Copy", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            foreach (DisplayImage image in itemsControl.SelectedItems)
            {
                CopyImage(image);
                ClosableTabItem.SendStatusUpdate(this, "Image(s) copied to clipboard.");
            }
        }
        private void Flip(bool horizontal)
        {
            if (itemsControl.SelectedItems.Count == 0)
            {
                DialogBox db = new DialogBox("No images selected.", "Select one or more images to flip and try again.",
                    "Filp", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            foreach (DisplayImage image in itemsControl.SelectedItems)
            {
                FlipImage(image, horizontal);
            }            
        }
        private void Rotate(bool clockwise)
        {
            if (itemsControl.SelectedItems.Count == 0)
            {
                DialogBox db = new DialogBox("No images selected.", "Select one or more images to rotate and try again.",
                    "Rotate", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            foreach (DisplayImage image in itemsControl.SelectedItems)
            {
                RotateImage(image, clockwise);
            }
        }
        private void ToRGB()
        {
            if (itemsControl.SelectedItems.Count == 0)
            {
                DialogBox db = new DialogBox("No images selected.", "Select one or more images to convert and try again.",
                    "Convert", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            foreach (DisplayImage image in itemsControl.SelectedItems)
            {
                ImageToRGB(image);
            }
        }
        private void Save()
        {
            if (itemsControl.SelectedItems.Count == 0)
            {
                DialogBox db = new DialogBox("No images selected.", "Select one or more images to save and try again.",
                    "Save", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap Images (.bmp)|*.bmp";
            Nullable<bool> result = sfd.ShowDialog();

            if (result != true) return;

            bool multiple = itemsControl.SelectedItems.Count > 0;
            int counter = 0;

            foreach (DisplayImage image in itemsControl.SelectedItems)
            {
                string filePath = sfd.FileName;
                if (multiple)
                {
                    filePath = filePath.Insert(filePath.Length - 4, 
                        "_" + (++counter).ToString());
                }

                SaveImage(image, filePath);
            }
        }
        private void DoResize()
        {
            if (itemsControl.SelectedItems.Count == 0)
            {
                DialogBox db = new DialogBox("No images selected.", "Select one or more images to resize and try again.",
                    "Resize", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            foreach (DisplayImage image in itemsControl.SelectedItems)
            {
                BitmapSource src = image.Source as BitmapSource;
                if (src == null) continue;

                ResizeDialogWindow rdw = new ResizeDialogWindow(new ResizeDialogArgs(src));
                if (rdw.ShowDialog() == true)
                {
                    ResizeDialogArgs result = rdw.ResizeResult;

                    if (!result.DoCrop)
                    {
                        try
                        {
                            Data3D original = ImageHelper.ConvertToData3D((BitmapSource)result.ImageToResize);
                            Data3D resized = ImageHelper.Upscale(original, result.ResizedWidth, result.ResizedHeight);

                            BitmapSource resizedImage = ImageHelper.CreateImage(resized);
                            CurrentSeries.Images.Add(new DisplayImage(resizedImage, image.Title + " - Resized"));
                        }
                        catch (Exception ex)
                        {
                            DialogBox.Show("Could not resize image " + image.Title, ex.Message, "Resize", DialogBoxIcon.Error);
                        }
                    }
                    else
                    {
                        try
                        {
                            BitmapSource original = image.Source as BitmapSource;
                            BitmapSource cropped = original.Crop(result.CropStartX, result.CropStartY, result.ResizedWidth, result.ResizedHeight);
                            CurrentSeries.Images.Add(new DisplayImage(cropped, image.Title + " - Cropped"));
                        }
                        catch (Exception ex)
                        {
                            DialogBox.Show("Could not crop image " + image.Title, ex.Message, "Crop", DialogBoxIcon.Error);
                        }
                    }
                    
                }
            }
        }

        private void DoFilter(ImageTabEvent EventType)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.ProgressChanged += bw_ProgressChanged;
            bw.DoWork += bw_DoWork;

            string progressMessage = "";

            switch (EventType)
            {
                case ImageTabEvent.FilterHighPass:
                    progressMessage = "Performing High Pass Filter. Please wait.";
                    break;
                case ImageTabEvent.FilterLowPass:
                    progressMessage = "Performing Low Pass Filter. Please wait.";
                    break;
                case ImageTabEvent.FilterMean:
                    progressMessage = "Performing Mean Filter. Please wait.";
                    break;
                case ImageTabEvent.FilterMedian:
                    progressMessage = "Performing Median Filter. Please wait.";
                    break;
                case ImageTabEvent.FilterGaussian:
                    progressMessage = "Performing Gaussian Filter. Please wait.";
                    break;
            }

            int numImages = itemsControl.SelectedItems.Count;

            if (numImages == 0)
            {
                DialogBox db = new DialogBox("No input images selected.", "Click OK to select all images and perform desired filter or cancel to return.",
                    "Filter", DialogBoxIcon.Help, true);
                Nullable<bool> result = db.ShowDialog();

                if (result != true) return;

                if (itemsControl.Items.Count == 0)
                {
                    DialogBox db_1 = new DialogBox("No images are available to select.", "Load or create an image series and try again.", "Filter", DialogBoxIcon.Error);
                    db_1.ShowDialog();
                    return;
                }

                itemsControl.SelectAll();

                numImages = itemsControl.SelectedItems.Count;
            }

            BitmapSource[] sources = new BitmapSource[numImages];
            string[] titles = new string[numImages];

            for (int i = 0; i < numImages; i++)
            {
                sources[i] = ((DisplayImage)itemsControl.SelectedItems[i]).Source.CloneCurrentValue() as BitmapSource;
                sources[i].Freeze();
                titles[i] = ((DisplayImage)itemsControl.SelectedItems[i]).Title;
            }

            pw = new ProgressWindow(progressMessage, "Filtering");
            pw.Show();

            FilterArgs a = new FilterArgs()
            {
                SourcesToFilter = sources,
                Titles = titles,
                FilterType = EventType
            };

            bw.RunWorkerAsync(a);
        }
        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            FilterArgs a = (FilterArgs)e.Argument;
            BitmapSource[] filtered;
            BackgroundWorker bw = sender as BackgroundWorker;

            switch (a.FilterType)
            {
                case ImageTabEvent.FilterHighPass:
                    filtered = Filter.HighPassFilter(a.SourcesToFilter, bw);
                    break;
                case ImageTabEvent.FilterLowPass:
                    filtered = Filter.LowPassFilter(a.SourcesToFilter, bw);
                    break;
                case ImageTabEvent.FilterMean:
                    filtered = Filter.MeanSmooth(a.SourcesToFilter, bw);
                    break;
                case ImageTabEvent.FilterMedian:
                    filtered = Filter.MedianSmooth(a.SourcesToFilter, bw);
                    break;
                case ImageTabEvent.FilterGaussian:
                    filtered = Filter.GaussianFilter(a.SourcesToFilter, bw);
                    break;
                default:
                    throw new ArgumentException("Invalid filter selection.");
            }

            for (int i = 0; i < filtered.Length; i++)
            {
                filtered[i].Freeze();
            }
            object[] results = new object[3]
            {
                filtered, a.Titles, a.FilterType
            };
            e.Result = results;
        }
        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pw.UpdateProgress(e.ProgressPercentage);
        }
        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            bw.RunWorkerCompleted -= bw_RunWorkerCompleted;
            bw.ProgressChanged -= bw_ProgressChanged;
            bw.DoWork -= bw_DoWork;

            object[] result = (object[])e.Result;

            BitmapSource[] sources = (BitmapSource[])result[0];
            string[] titles = (string[])result[1];
            ImageTabEvent filterType = (ImageTabEvent)result[2];

            DisplayImage[] filtered = new DisplayImage[sources.Length];

            for (int i = 0; i < sources.Length; i++)
            {
                filtered[i] = new DisplayImage(sources[i], titles[i]);
            }

            ChangeSeries(new DisplaySeries(filtered));

            string filter = String.Empty;
            switch(filterType)
            {
                case ImageTabEvent.FilterHighPass:
                    filter = "High pass";
                    break;
                case ImageTabEvent.FilterLowPass:
                    filter = "Low pass";
                    break;
                case ImageTabEvent.FilterMedian:
                    filter = "Median";
                    break;
                case ImageTabEvent.FilterMean:
                    filter = "Mean";
                    break;
            }
            pw.ProgressFinished("Filter complete!");
            ClosableTabItem.SendStatusUpdate(this, filter + " filter complete.");
            bw.Dispose();
        }
  
        private void DoSlice(ImageTabEvent dimension, int pixel)
        {
            BitmapSource slice = null;
            BitmapSource[] selected = new BitmapSource[itemsControl.SelectedItems.Count];
            for (int i = 0; i < itemsControl.SelectedItems.Count; i++)
            {
                DisplayImage image = (DisplayImage)itemsControl.SelectedItems[i];
                if (image == null) continue;

                BitmapSource bs = (BitmapSource)image.Source;
                if (bs == null) continue;

                selected[i] = bs;
            }
            try
            {
                if (dimension == ImageTabEvent.SliceXZ)
                {
                    slice = ImageHelper.GetXZ(selected, pixel);
                }
                else if (dimension == ImageTabEvent.SliceYZ)
                {
                    slice = ImageHelper.GetYZ(selected, pixel);
                }
            }
            catch (NullReferenceException NRex)
            {
                DialogBox db = new DialogBox("There was a problem with one or more of the images.", NRex.Message,
                    "Slice", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }
            catch (IndexOutOfRangeException IOORex)
            {
                DialogBox db = new DialogBox("There was a problem with one or more of the image dimensions.", IOORex.Message,
                       "Slice", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }
            catch (Exception ex)
            {
                DialogBox db = new DialogBox("There was a problem slicing the image series.", ex.Message,
                       "Slice", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            if (slice != null)
            {
                DisplayImage image = new DisplayImage();
                image.Source = slice;
                CurrentSeries.Images.Add(image);
            }
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isCtrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (e.Key == Key.A && isCtrlDown)
            {
                itemsControl.SelectAll();
                return;
            }
        }
        private void DoOverlay()
        {
            int imgCount = itemsControl.SelectedItems.Count;
            if (imgCount == 0)
            {
                DialogBox db = new DialogBox("No images selected", "Select two or more images to overlay and try again.",
                    "Overlay", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }
            if (imgCount == 1)
            {
                DialogBox db = new DialogBox("Only one image selected", "Select two or more images to overlay and try again.",
                    "Overlay", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }
            BitmapSource[] toOverlay = new BitmapSource[imgCount];

            int i = 0;
            foreach (DisplayImage obj in itemsControl.SelectedItems)
            {
                BitmapSource bs = obj.Source as BitmapSource;
                if (bs == null) throw new ArgumentException("Invalid ImageSource");

                toOverlay[i] = bs;
                i++;
            }

            int width = -1;
            int height = -1;
            bool proceed = true;

            foreach (BitmapSource bs in toOverlay)
            {
                if (width == -1) width = (int)bs.PixelWidth;
                if (height == -1) height = (int)bs.PixelHeight;

                if ((int)bs.Width != width || (int)bs.Height != height)
                {
                    proceed = false;
                    break;
                }
            }

            if (!proceed)
            {
                DialogBox db = new DialogBox("Invalid image dimensions",
                    "One or more images selected does not match the rest of the collection in width or height.",
                    "Overlay", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            BitmapSource overlay = Overlay.CreateOverlay(toOverlay);
            CurrentSeries.Images.Add(new DisplayImage(overlay, "Overlay Image"));
            ClosableTabItem.SendStatusUpdate(this, "Overlay complete.");
        }

        public void AddImage(DisplayImage Image)
        {
            CurrentSeries.Images.Add(Image);
        }

        private void buttonSelectAll_Click(object sender, RoutedEventArgs e)
        {
            itemsControl.SelectAll();
        }
        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            itemsControl.UnselectAll();
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (System.IO.Path.GetExtension(file) == ".bmp")
                    {
                        DisplayImage image = new DisplayImage();
                        BitmapImage src = new BitmapImage();
                        src.BeginInit();
                        src.UriSource = new Uri(file, UriKind.Absolute);
                        src.CacheOption = BitmapCacheOption.OnLoad;
                        src.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        src.EndInit();

                        image.Source = src;
                        image.Stretch = Stretch.Uniform;

                        image.Title = System.IO.Path.GetFileNameWithoutExtension(file);

                        CurrentSeries.Images.Add(image);
                    }
                }
                e.Handled = true;
            }
        }

        private void menuItemSelectAll_Click(object sender, RoutedEventArgs e)
        {
            itemsControl.SelectAll();
        }
        private void menuItemClearSelection_Click(object sender, RoutedEventArgs e)
        {
            itemsControl.UnselectAll();
        }

        public DisplaySeries CreateImageSeries()
        {
            int numImages = itemsControl.SelectedItems.Count;

            if (numImages == 0)
            {
                DialogBox db = new DialogBox("No input images selected.", "Click OK to select all images and create an image series or cancel to return.",
                    "Create Series", DialogBoxIcon.Help, true);
                Nullable<bool> result = db.ShowDialog();

                if (result != true) return null;

                if (itemsControl.Items.Count == 0)
                {
                    DialogBox db_1 = new DialogBox("No images are available to select.", "Load or create an image series and try again.", "Create Series", DialogBoxIcon.Error);
                    db_1.ShowDialog();
                    return null;
                }

                itemsControl.SelectAll();

                numImages = itemsControl.SelectedItems.Count;
            }

            BitmapSource[] sources = new BitmapSource[numImages];
            string[] titles = new string[numImages];
            DisplaySeries series = new DisplaySeries();

            for (int i = 0; i < numImages; i++)
            {
                DisplayImage img = ((DisplayImage)itemsControl.SelectedItems[i]).Clone();
                series.Images.Add(img);                
            }

            TextEntryDialog te = new TextEntryDialog("Please enter a name for the new image series.");
            Nullable<bool> result_te = te.ShowDialog();

            if (result_te != true) return null;

            if(te.EnteredText == String.Empty)
            {
                DialogBox db = new DialogBox("Invalid series name.", "Please try again and enter a value for the series name.", "Image Series", DialogBoxIcon.Error);
                db.ShowDialog();
                return null;
            }

            series.SeriesName = te.EnteredText;

            return series;
        }
    }

    public enum ImageTabEvent
    {
        Copy, ToRGB, FlipHorizontal, FlipVertical, RotateClock, RotateCounter, Empty,
        FilterMean, FilterMedian, FilterHighPass, FilterLowPass, FilterGaussian, SliceXZ, SliceYZ, ZCorrect, Save,
        Overlay, Resize
    }

    internal struct FilterArgs
    {
        public BitmapSource[] SourcesToFilter;
        public string[] Titles;
        public ImageTabEvent FilterType;
    }
}
