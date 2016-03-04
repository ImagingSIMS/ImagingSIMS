using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

using ImagingSIMS.Common.Dialogs;

namespace ImagingSIMS.Common.Controls
{
    /// <summary>
    /// Interaction logic for BitmapDisplay.xaml
    /// </summary>
    public partial class BitmapDisplay : UserControl, System.ComponentModel.INotifyPropertyChanged,
        IDisposable
    {
        WriteableBitmap wBmp;
        BitmapSource bs;
        Bitmap bmp;
        bool _hasImage;
        bool _enableAdjustment;
        bool _canEnableAdjustment;
        bool _canRotateFlip;
        bool _canConvertRGB;

        byte[, ,] _intensityMatrix;
        int _currentScale = 255;
        BackgroundWorker bw_RescaleIntensity;

        public Visibility BorderVisibility
        {
            get { return border1.Visibility; }
            set { border1.Visibility = value; border2.Visibility = value; }
        }
        public ImageType ImageType
        { get; private set; }
        public bool ContextMenuIsEnabled { get; set; }
        public bool HasImage
        {
            get { return _hasImage; }
            set
            {
                if (_hasImage != value)
                {
                    _hasImage = value;
                    this.OnPropertyChanged("HasImage");
                }
            }
        }
        public bool EnableAdjustment
        {
            get { return _enableAdjustment; }
            set
            {
                if (_enableAdjustment != value)
                {
                    _enableAdjustment = value;
                    this.OnPropertyChanged("EnableAdjustment");
                }
            }
        }
        public bool CanEnableAdjustment
        {
            get { return _canEnableAdjustment; }
            set
            {
                if (_canEnableAdjustment != value)
                {
                    _canEnableAdjustment = value;
                    this.OnPropertyChanged("CanEnableAdjustment");
                }
            }
        }
        public bool CanRotateFlip
        {
            get { return _canRotateFlip; }
            set
            {
                if (_canRotateFlip != value)
                {
                    _canRotateFlip = value;
                    this.OnPropertyChanged("CanRotateFlip");
                }
            }
        }
        public bool CanConvertRGB
        {
            get { return _canConvertRGB; }
            set
            {
                if (_canConvertRGB != value)
                {
                    _canConvertRGB = value;
                    this.OnPropertyChanged("CanConvertRGB");
                }
            }
        }

        public bool IsARGB
        {
            get
            {
                if (bmp != null)
                {
                    return bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                }
                else return false;
            }
        }
        public bool IsRGB
        {
            get
            {
                if (bmp != null)
                {
                    return bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                }
                else return false;
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BitmapDisplay()
        {
            InitializeComponent();
            CanRotateFlip = true;
            CanConvertRGB = true;

            Loaded += BitmapDisplay_Loaded;
            SizeChanged += BitmapDisplay_SizeChanged;
        }

        void BitmapDisplay_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }
        void BitmapDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (bmp != null) bmp.Dispose();
                if (bw_RescaleIntensity != null) bw_RescaleIntensity.Dispose();
            }
        }

        public void ShowBorder(bool Show)
        {
            if (!Show)
            {
                border1.Visibility = Visibility.Hidden;
                border2.Visibility = Visibility.Hidden;
            }
            else
            {
                border1.Visibility = Visibility.Visible;
                border2.Visibility = Visibility.Visible;
            }
        }

        public void SetImage(Bitmap Image)
        {
            bmp = Image;
            bs = ConvertToBitmapSource(bmp);

            display.Width = bmp.Width;
            display.Height = bmp.Height;

            this.Width = display.Width + 6;
            this.Height = display.Height + 6;

            display.Source = bs;
            display.Stretch = Stretch.Uniform;

            ImageType = ImageType.Bitmap;
            HasImage = true;

            CanEnableAdjustment = true;
            _intensityMatrix = CreateIntensityMatrix(Image);
            bw_RescaleIntensity = new BackgroundWorker();
            bw_RescaleIntensity.WorkerReportsProgress = false;
            bw_RescaleIntensity.WorkerSupportsCancellation = false;
            bw_RescaleIntensity.DoWork += bw_RescaleIntensity_DoWork;
            bw_RescaleIntensity.RunWorkerCompleted += bw_RescaleIntensity_RunWorkerCompleted;

            CanConvertRGB = bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb;
        }
        public void SetImage(BitmapSource Image)
        {
            bs = Image;

            display.Width = bs.Width;
            display.Height = bs.Height;

            this.Width = display.Width + 6;
            this.Height = display.Height + 6;

            display.Source = bs;
            display.Stretch = Stretch.Uniform;

            ImageType = ImageType.BitmapSource;
            HasImage = true;

            CanConvertRGB = bs.Format == PixelFormats.Bgra32;
        }
        public void SetImage(WriteableBitmap Image)
        {
            wBmp = Image;

            this.Width = display.Width + 6;
            this.Height = display.Height + 6;

            display.Source = wBmp;
            display.Stretch = Stretch.Uniform;

            display.Width = Image.Width;
            display.Height = Image.Height;

            ImageType = ImageType.WriteableBitmap;
            HasImage = true;

            CanConvertRGB = wBmp.Format == PixelFormats.Bgra32;
        }
        public void SetImage(Bitmap Image, int FixedSize)
        {
            bmp = Image;
            bs = ConvertToBitmapSource(bmp);

            display.Width = FixedSize;
            display.Height = FixedSize;

            this.Width = FixedSize;
            this.Height = FixedSize;

            display.Source = bs;
            display.Stretch = Stretch.Uniform;

            ImageType = ImageType.Bitmap;
            HasImage = true;

            CanEnableAdjustment = true;
            _intensityMatrix = CreateIntensityMatrix(Image);
            bw_RescaleIntensity = new BackgroundWorker();
            bw_RescaleIntensity.WorkerReportsProgress = false;
            bw_RescaleIntensity.WorkerSupportsCancellation = false;
            bw_RescaleIntensity.DoWork += bw_RescaleIntensity_DoWork;
            bw_RescaleIntensity.RunWorkerCompleted += bw_RescaleIntensity_RunWorkerCompleted;

            CanConvertRGB = bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb;
        }
        public void SetImage(BitmapSource Image, int FixedSize)
        {
            bs = Image;

            display.Width = FixedSize;
            display.Height = FixedSize;

            this.Width = FixedSize;
            this.Height = FixedSize;

            display.Source = bs;
            display.Stretch = Stretch.Uniform;

            ImageType = ImageType.BitmapSource;
            HasImage = true;

            CanConvertRGB = bs.Format == PixelFormats.Bgra32;
        }
        public void SetImage(WriteableBitmap Image, int FixedSize)
        {
            wBmp = Image;

            this.Width = FixedSize;
            this.Height = FixedSize;

            display.Source = wBmp;
            display.Stretch = Stretch.Uniform;

            display.Width = FixedSize;
            display.Height = FixedSize;

            ImageType = ImageType.WriteableBitmap;
            HasImage = true;

            CanConvertRGB = wBmp.Format == PixelFormats.Bgra32;
        }
        public void ClearImage()
        {
            display.Source = null;

            ImageType = ImageType.None;
            HasImage = false;
        }

        public Bitmap GetImage()
        {
            return bmp;
        }
        public WriteableBitmap GetWritableBitmap()
        {
            return wBmp;
        }
        public BitmapSource GetBitmapSource()
        {
            return bs;
        }

        public void SetBackground(System.Windows.Media.Color Color)
        {
            this.Background = new SolidColorBrush(Color);
        }
        public void SetBackground(System.Drawing.Color Color)
        {
            this.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(Color.A,
                Color.R, Color.G, Color.B));
        }

        public void SetSize(System.Windows.Size DesiredSize)
        {
            if (border1.Visibility == Visibility.Visible)
            {
                this.Width = DesiredSize.Width + 6;
                this.Height = DesiredSize.Height + 6;
            }
            else
            {
                this.Width = DesiredSize.Width;
                this.Height = DesiredSize.Height;
            }
        }

        private BitmapSource ConvertToBitmapSource(Bitmap ImageToConvert)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ImageToConvert.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
               System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }

        private void cmSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (bmp != null)
            {
                sfd.Filter = "Bitmap Images (.bmp)|*.bmp";
            }
            else if (bs != null)
            {
                sfd.Filter = "Bitmap Images (.bmp)|*.bmp";
            }
            else if (wBmp != null)
            {
                sfd.Filter = "PNG Images (.png)|*png";
            }

            Nullable<bool> result = sfd.ShowDialog();
            if (result == true)
            {
                try
                {
                    if (bmp != null)
                    {
                        bmp.Save(sfd.FileName);
                    }
                    else if (bs != null)
                    {
                        using (System.IO.FileStream s = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create))
                        {
                            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bs));

                            encoder.Save(s);
                        }
                    }
                    else if (wBmp != null)
                    {
                        using (System.IO.FileStream s = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create))
                        {
                            PngBitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(wBmp));

                            encoder.Save(s);
                        }
                    }

                    DialogBox db = new DialogBox("Image saved successfully!",
                       sfd.FileName, "Save", DialogIcon.Ok);
                    db.ShowDialog();
                }
                catch (Exception ex)
                {
                    string inner = "";
                    if (ex.InnerException != null)
                    {
                        inner = ex.InnerException.Message;
                    }
                    DialogBox db = new DialogBox(ex.Message, inner,
                        "Save", DialogIcon.Ok);
                    db.ShowDialog();
                }
            }
        }

        private void cmCopy_Click(object sender, RoutedEventArgs e)
        {
            if (bs == null)
            {
                if (bmp == null)
                {
                    DialogBox db = new DialogBox("Incorrect image format.",
                        "Only bitmap images can be copied.", "Copy", DialogIcon.Error);
                    db.ShowDialog();
                    return;
                }
                Clipboard.SetImage(ConvertToBitmapSource(bmp));
                return;
            }
            Clipboard.SetImage(bs);
        }

        private void imagePanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!ContextMenuIsEnabled)
            {
                if (e.ChangedButton == MouseButton.Right)
                {
                    cMenu.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                }
            }
            if (e.ChangedButton == MouseButton.Left)
            {
            }
        }

        private void cmRotateClock_Click(object sender, RoutedEventArgs e)
        {
            Rotate(true);
        }
        private void cmRotateCounter_Click(object sender, RoutedEventArgs e)
        {
            Rotate(false);
        }
        private void cmFlipHoriz_Click(object sender, RoutedEventArgs e)
        {
            Flip(false);
        }
        private void cmFlipVert_Click(object sender, RoutedEventArgs e)
        {
            Flip(true);
        }

        private void Rotate(bool Clockwise)
        {
            if (Clockwise)
            {
                switch (ImageType)
                {
                    case ImageType.WriteableBitmap:
                        break;
                    case ImageType.Bitmap:
                        Bitmap temp = new Bitmap(bmp.Width, bmp.Height);
                        for (int x = 0; x < temp.Width; x++)
                        {
                            for (int y = 0; y < temp.Height; y++)
                            {
                                System.Drawing.Color c = bmp.GetPixel(x, bmp.Height - y - 1);
                                temp.SetPixel(y, x, c);
                            }
                        }
                        SetImage(temp, (int)this.ActualWidth);
                        break;
                    case ImageType.BitmapSource:
                        break;
                }
            }
            else
            {
                switch (ImageType)
                {
                    case ImageType.WriteableBitmap:
                        break;
                    case ImageType.Bitmap:
                        Bitmap temp = new Bitmap(bmp.Width, bmp.Height);
                        for (int x = 0; x < temp.Width; x++)
                        {
                            for (int y = 0; y < temp.Height; y++)
                            {
                                System.Drawing.Color c = bmp.GetPixel(bmp.Width - x - 1, y);
                                temp.SetPixel(y, x, c);
                            }
                        }
                        SetImage(temp, (int)this.ActualWidth);
                        break;
                    case ImageType.BitmapSource:
                        break;
                }
            }
        }
        private void Flip(bool Vertical)
        {
            if (Vertical)
            {
                switch (ImageType)
                {
                    case ImageType.WriteableBitmap:
                        break;
                    case ImageType.Bitmap:
                        Bitmap temp = new Bitmap(bmp.Width, bmp.Height);
                        for (int x = 0; x < temp.Width; x++)
                        {
                            for (int y = 0; y < temp.Height; y++)
                            {
                                System.Drawing.Color c = bmp.GetPixel(x, bmp.Height - y - 1);
                                temp.SetPixel(x, y, c);
                            }
                        }
                        SetImage(temp, (int)this.ActualWidth);
                        break;
                    case ImageType.BitmapSource:
                        break;
                }
            }
            else
            {
                switch (ImageType)
                {
                    case ImageType.WriteableBitmap:
                        break;
                    case ImageType.Bitmap:
                        Bitmap temp = new Bitmap(bmp.Width, bmp.Height);
                        for (int x = 0; x < temp.Width; x++)
                        {
                            for (int y = 0; y < temp.Height; y++)
                            {
                                System.Drawing.Color c = bmp.GetPixel(bmp.Width - x - 1, y);
                                temp.SetPixel(x, y, c);
                            }
                        }
                        SetImage(temp, (int)this.ActualWidth);
                        break;
                    case ImageType.BitmapSource:
                        break;
                }
            }
        }

        private void cmConvertRGB_Click(object sender, RoutedEventArgs e)
        {
            Bitmap temp = new Bitmap(bmp.Width, bmp.Height);
            for (int x = 0; x < temp.Width; x++)
            {
                for (int y = 0; y < temp.Height; y++)
                {
                    System.Drawing.Color c = bmp.GetPixel(x, y);
                    temp.SetPixel(x, y, ToRGB(c, System.Drawing.Color.Black));
                }
            }
            SetImage(temp, (int)this.ActualWidth);
            CanConvertRGB = false;
        }
        private void cmConvertARGB_Click(object sender, RoutedEventArgs e)
        {

        }

        private System.Drawing.Color ToRGB(System.Drawing.Color Foreground, System.Drawing.Color Background)
        {
            if (Foreground.A == 255) return Foreground;

            var alpha = Foreground.A / 255.0;
            var diff = 1.0 - alpha;
            return System.Drawing.Color.FromArgb(255,
                (byte)(Foreground.R * alpha + Background.R * diff),
                (byte)(Foreground.G * alpha + Background.G * diff),
                (byte)(Foreground.B * alpha + Background.B * diff));
        }

        private void imagePanel_MouseMove(object sender, MouseEventArgs e)
        {
            DockPanel dp = sender as DockPanel;
            if (dp != null && e.LeftButton == MouseButtonState.Pressed)
            {
                if (bmp != null)
                {
                    DataObject data = new DataObject(DataFormats.Bitmap, bmp);
                    DragDrop.DoDragDrop(dp, data, DragDropEffects.Copy);
                }
            }
        }

        private byte[, ,] CreateIntensityMatrix(Bitmap Image)
        {
            byte[, ,] returnMatrix = new byte[(int)Image.Width, (int)Image.Height, 4];
            if (IsARGB)
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    for (int y = 0; y < Image.Height; y++)
                    {
                        System.Drawing.Color c = Image.GetPixel(x, y);
                        returnMatrix[x, y, 0] = c.A;
                        returnMatrix[x, y, 1] = c.R;
                        returnMatrix[x, y, 2] = c.G;
                        returnMatrix[x, y, 3] = c.B;
                    }
                }
            }
            else
            {
                for (int x = 0; x < Image.Width; x++)
                {
                    for (int y = 0; y < Image.Height; y++)
                    {
                        System.Drawing.Color c = Image.GetPixel(x, y);
                        byte a = c.A;
                        returnMatrix[x, y, 0] = 0;
                        returnMatrix[x, y, 1] = c.R;
                        returnMatrix[x, y, 2] = c.G;
                        returnMatrix[x, y, 3] = c.B;
                    }
                }
            }
            return returnMatrix;
        }
        private void cmEnableScroll_Click(object sender, RoutedEventArgs e)
        {
            if (CanEnableAdjustment)
            {
                bool current = EnableAdjustment;
                EnableAdjustment = !current;
            }
        }
        private void imagePanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (CanEnableAdjustment && EnableAdjustment && _intensityMatrix != null && bw_RescaleIntensity != null)
            {
                if (!bw_RescaleIntensity.IsBusy)
                {
                    RescaleImageParameters r = new RescaleImageParameters();
                    r.CurrentScaleValue = _currentScale;
                    int ticks = e.Delta;
                    double newScaleValue = _currentScale + (2f * (double)e.Delta);

                    //if (newScaleValue > 255) newScaleValue = 255;
                    if (newScaleValue <= 0) newScaleValue = 1;
                    if (newScaleValue == _currentScale) return;

                    r.NewScaleValue = (int)newScaleValue;
                    r.IntensityMatrix = _intensityMatrix;
                    r.ImageToRescale = bmp;

                    r.IsARGB = bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb;

                    bw_RescaleIntensity.RunWorkerAsync(r);
                }
            }
        }
        void bw_RescaleIntensity_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RescaleImageReturn r = (RescaleImageReturn)e.Result;

            if (r.Succeeded)
            {
                _currentScale = r.NewScaleValue;
                bmp = r.ReturnImage;
                bs = ConvertToBitmapSource(bmp);

                display.Source = bs;
                display.Stretch = Stretch.Uniform;
            }
            else
            {
                DialogBox db = new DialogBox("Could not rescale the image.",
                    r.ErrorMessage, "Image Display", DialogIcon.Error);
                db.ShowDialog();
            }
        }

        void bw_RescaleIntensity_DoWork(object sender, DoWorkEventArgs e)
        {
            RescaleImageParameters r = (RescaleImageParameters)e.Argument;
            RescaleImageReturn rir = new RescaleImageReturn();
            rir.Succeeded = false;

            try
            {
                Bitmap returnImage;
                if (r.IsARGB)
                {
                    returnImage = new Bitmap(r.ImageToRescale.Width, r.ImageToRescale.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    for (int x = 0; x < r.ImageToRescale.Width; x++)
                    {
                        for (int y = 0; y < r.ImageToRescale.Height; y++)
                        {
                            System.Drawing.Color c = r.ImageToRescale.GetPixel(x, y);

                            double newA = (r.IntensityMatrix[x, y, 0] / (double)r.NewScaleValue) * 255f;
                            if (newA > 255) newA = 255;
                            if (newA < 0) newA = 0;
                            returnImage.SetPixel(x, y, System.Drawing.Color.FromArgb((byte)newA,
                                c.R, c.G, c.B));
                        }
                    }
                }
                else
                {
                    returnImage = new Bitmap(r.ImageToRescale.Width, r.ImageToRescale.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    for (int x = 0; x < r.ImageToRescale.Width; x++)
                    {
                        for (int y = 0; y < r.ImageToRescale.Height; y++)
                        {
                            double newR = (r.IntensityMatrix[x, y, 1] / (double)r.NewScaleValue) * 255f;
                            if (newR > 255) newR = 255;
                            if (newR < 0) newR = 0;
                            double newG = (r.IntensityMatrix[x, y, 2] / (double)r.NewScaleValue) * 255f;
                            if (newG > 255) newG = 255;
                            if (newG < 0) newG = 0;
                            double newB = (r.IntensityMatrix[x, y, 3] / (double)r.NewScaleValue) * 255f;
                            if (newB > 255) newB = 255;
                            if (newB < 0) newB = 0;
                            returnImage.SetPixel(x, y, System.Drawing.Color.FromArgb((byte)newR,
                                (byte)newG, (byte)newB));
                        }
                    }
                }
                rir.NewScaleValue = r.NewScaleValue;
                rir.ReturnImage = returnImage;
                rir.Succeeded = true;
            }
            catch (Exception ex)
            {
                rir.Succeeded = false;
                rir.ErrorMessage = ex.Message;
            }
            e.Result = rir;
        }
    }

    public enum ImageType { WriteableBitmap, Bitmap, BitmapSource, None }

    struct RescaleImageParameters
    {
        public int CurrentScaleValue;
        public int NewScaleValue;
        public Bitmap ImageToRescale;
        public byte[, ,] IntensityMatrix;
        public bool IsARGB;
    }
    struct RescaleImageReturn
    {
        public bool Succeeded;
        public string ErrorMessage;
        public int NewScaleValue;
        public Bitmap ReturnImage;
    }

    public class BoolToScrollTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                bool input = (bool)value;

                if (input)
                {
                    return "Disable Adjustment";
                }
                else
                {
                    return "Enable Adjustment";
                }
            }
            catch (InvalidCastException ICex)
            {
                string inner = "Please try again.";
                if (ICex.InnerException != null)
                {
                    inner = ICex.InnerException.Message;
                }
                DialogBox db = new DialogBox(ICex.Message, inner,
                    "Bitmap Display", DialogIcon.Error);
                db.ShowDialog();
                return "Error in operation";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            try
            {
                string input = value.ToString();
                if (input == null)
                {
                    throw new InvalidCastException("The input argument cannot be converted to string.");
                }

                if (input == "Disable Adjustment")
                {
                    return true;
                }
                else if (input == "Enable Adjustment")
                {
                    return false;
                }
                else
                {
                    return false;
                }
            }
            catch (InvalidCastException ICex)
            {
                string inner = "Please try again.";
                if (ICex.InnerException != null)
                {
                    inner = ICex.InnerException.Message;
                }
                DialogBox db = new DialogBox(ICex.Message, inner,
                    "Bitmap Display", DialogIcon.Error);
                db.ShowDialog();
                return false;
            }
        }
    }
}
