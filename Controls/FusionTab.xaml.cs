using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Microsoft.Win32;
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Common.Registry;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Fusion;
using ImagingSIMS.Data.Fusion.Analysis;
using ImagingSIMS.Data.Imaging;
using ImagingSIMS.ImageRegistration;
using Fusion = ImagingSIMS.Data.Fusion.Fusion;

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for FusionTab.xaml
    /// </summary>
    public partial class FusionTab : UserControl, IDisposable
    {
        public static readonly DependencyProperty LowResImageProperty = DependencyProperty.Register("LowResImage",
            typeof(FusionImageDisplayItem), typeof(FusionTab), new FrameworkPropertyMetadata(new FusionImageDisplayItem(FusionImageType.LowRes)));
        public static readonly DependencyProperty HighResImageProperty = DependencyProperty.Register("HighResImage",
            typeof(FusionImageDisplayItem), typeof(FusionTab), new FrameworkPropertyMetadata(new FusionImageDisplayItem(FusionImageType.HighRes)));
        public static readonly DependencyProperty FusedImageProperty = DependencyProperty.Register("FusedImage",
            typeof(FusionImageDisplayItem), typeof(FusionTab), new FrameworkPropertyMetadata(new FusionImageDisplayItem(FusionImageType.Fused)));
        public static readonly DependencyProperty AnalysisResultsProperty = DependencyProperty.Register("AnalysisResults",
            typeof(string), typeof(FusionTab));
        public static readonly DependencyProperty ShiftWindowSizeProperty = DependencyProperty.Register("ShiftWindowSize",
            typeof(int), typeof(FusionTab), new FrameworkPropertyMetadata(11));
        public static readonly DependencyProperty IsRegistrationEnabledProperty = DependencyProperty.Register("IsRegistrationEnabled",
            typeof(bool), typeof(FusionTab), new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty IsRegisteringProperty = DependencyProperty.Register("IsRegistering",
            typeof(bool), typeof(FusionTab));
        public static readonly DependencyProperty CanUndoRegistrationProperty = DependencyProperty.Register("CanUndoRegistration",
            typeof(bool), typeof(FusionTab));
        public static readonly DependencyProperty IsRegisteredProperty = DependencyProperty.Register("IsRegistered",
            typeof(bool), typeof(FusionTab));
        public static readonly DependencyProperty RegistrationResultsProperty = DependencyProperty.Register("RegistrationResults",
            typeof(RegistrationResult), typeof(FusionTab));

        public FusionImageDisplayItem LowResImage
        {
            get { return (FusionImageDisplayItem)GetValue(LowResImageProperty); }
            set { SetValue(LowResImageProperty, value); }
        }
        public FusionImageDisplayItem HighResImage
        {
            get { return (FusionImageDisplayItem)GetValue(HighResImageProperty); }
            set { SetValue(HighResImageProperty, value); }
        }
        public FusionImageDisplayItem FusedImage
        {
            get { return (FusionImageDisplayItem)GetValue(FusedImageProperty); }
            set { SetValue(FusedImageProperty, value); }
        }

        public RegistrationParametersViewItem RegistrationParameters
        {
            get { return registrationParametersControl.RegistrationParameters; }
            set { registrationParametersControl.RegistrationParameters = value; }
        }
        public event EventHandler PointBasedChanged;

        public string AnalysisResults
        {
            get { return (string)GetValue(AnalysisResultsProperty); }
            set { SetValue(AnalysisResultsProperty, value); }
        }

        public int ShiftWindowSize
        {
            get { return (int)GetValue(ShiftWindowSizeProperty); }
            set { SetValue(ShiftWindowSizeProperty, value); }
        }

        public bool IsRegistrationEnabled
        {
            get { return (bool)GetValue(IsRegistrationEnabledProperty); }
            set { SetValue(IsRegistrationEnabledProperty, value); }
        }
        public bool IsRegistering
        {
            get { return (bool)GetValue(IsRegisteringProperty); }
            set { SetValue(IsRegisteringProperty, value); }
        }
        public bool CanUndoRegistration
        {
            get { return (bool)GetValue(CanUndoRegistrationProperty); }
            set { SetValue(CanUndoRegistrationProperty, value); }
        }
        public bool IsRegistered
        {
            get { return (bool)GetValue(IsRegisteredProperty); }
            set { SetValue(IsRegisteredProperty, value); }
        }
        public RegistrationResult RegistrationResults
        {
            get { return (RegistrationResult)GetValue(RegistrationResultsProperty); }
            set { SetValue(RegistrationResultsProperty, value); }
        }

        public FusionTab()
        {
            HighResImage = new FusionImageDisplayItem(FusionImageType.HighRes);
            LowResImage = new FusionImageDisplayItem(FusionImageType.LowRes);
            FusedImage = new FusionImageDisplayItem(FusionImageType.Fused);

            RegistrationResults = new RegistrationResult();

            LowResImage.ImageSourceChanged += inputImage_ImageSourceChanged;
            HighResImage.ImageSourceChanged += inputImage_ImageSourceChanged;

            InitializeComponent();
            
            RegistrationParameters.PropertyChanged += RegistrationParameters_PropertyChanged;
        }

        void RegistrationParameters_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PointBased" && PointBasedChanged != null)
            {
                PointBasedChanged(this, EventArgs.Empty);
            }
        }

        void _overlayWindow_ParametersGenerated(object sender, ParametersGeneratedEventArgs e)
        {
            RegistrationParameters parameters = e.Paramaters;

            registrationParametersControl.RegistrationParameters.Angle = parameters.Angle;
            registrationParametersControl.RegistrationParameters.Scale = parameters.Scale;
            registrationParametersControl.RegistrationParameters.TranslationX = parameters.TranslationX;
            registrationParametersControl.RegistrationParameters.TranslationY = parameters.TranslationY;
        }

        private void inputImage_ImageSourceChanged(object sender, EventArgs e)
        {
            this.HighResImage.IsRegistered = false;
            this.LowResImage.IsRegistered = false;

            IsRegistered = false;

            if (_overlayWindow != null)
            {
                _overlayWindow.FixedImageSource = LowResImage.ImageSource;
                _overlayWindow.MovingImageSource = HighResImage.ImageSource;
            }
        }

        FusionImageDisplayItem _lastLowRes;
        FusionImageDisplayItem _lastHighRes;

        Registration _imageRegistration;
        ImageOverlayWindow _overlayWindow;
        private async void registerImages_Click(object sender, RoutedEventArgs e)
        {
            if(HighResImage.IsRegistered && LowResImage.IsRegistered)
            {
                if (DialogBox.Show("Images are already registered.",
                    "Click OK to register anyway or Cancel to return", "Registration", DialogBoxIcon.Help, true) != true) return;
            }
            //Check input parameters
            ImageRegistrationTypes regType = RegistrationParameters.RegType;
            if (regType == ImageRegistrationTypes.NoRegistration)
            {
                DialogBox.Show("No registration method selected.",
                    "Please choose one of the available registration methods and try again.", "Registration", DialogBoxIcon.Error);
                return;
            }
            if (HighResImage.ImageSource == null)
            {
                DialogBox db = new DialogBox("No high resolution image loaded.",
                    "Drop or load a high resolution image into the tab and try again.", "Registration", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }
            if (LowResImage.ImageSource == null)
            {
                DialogBox db = new DialogBox("No low resolution image loaded.",
                    "Drop or load a low resolution image into the tab and try again.", "Registration", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }
            if (RegistrationParameters.MaxIterations < 0)
            {
                DialogBox.Show("Invalid maximum iterations.",
                    "Set the maximum number of iterations to a value greater than 0 to perform automatic registration or to 0 to specify a transform.", 
                    "Registration", DialogBoxIcon.Error);
                return;
            }
            //if ((regType == ImageRegistrationTypes.CenterRigid2D || regType == ImageRegistrationTypes.CenterSimilarity)
            //    && RegistrationParameters.Angle == 0.0d)
            //{
            //    if (DialogBox.Show("Initial angle set to 0 degrees.",
            //        "Rotational registration is selected with an initial angle of 0 degrees. Click OK to proceed or Cancel to return and specify an approximate angle for registration.",
            //        "Registration", DialogBoxIcon.RedQuestion, true) != true) return;
            //}
            //if (regType == ImageRegistrationTypes.CenterSimilarity && RegistrationParameters.Scale == 1.0d)
            //{
            //    if (DialogBox.Show("Initial scale set to 1.0x.",
            //        "Scaled registration is selected with an initial scale of 1.0x. Click OK to proceed or Cancel to return and specify an approximate scale for registration.",
            //        "Registration", DialogBoxIcon.RedQuestion, true) != true) return;
            //}

            if (RegistrationParameters.MaxStepLength < 0)
            {
                DialogBox.Show("Invalid maximum step length.",
                   "Set the maximum step length for multi modal registration to a value greater than 0.",
                   "Registration", DialogBoxIcon.Error);
                return;
            }
            if (RegistrationParameters.MinStepLength < 0)
            {
                DialogBox.Show("Invalid minimum step length.",
                   "Set the minimum step length for multi modal registration to a value greater than 0.",
                   "Registration", DialogBoxIcon.Error);
                return;
            }

            if (RegistrationParameters.MultiModal)
            {
                if (RegistrationParameters.NumberBins < 0)
                {
                    DialogBox.Show("Invalid number of bins.",
                       "Set the number of bins for multi modal registration to a value greater than 0.",
                       "Registration", DialogBoxIcon.Error);
                    return;
                }
                if (RegistrationParameters.NumberSamples < 0)
                {
                    DialogBox.Show("Invalid number of samples.",
                       "Set the number of samples for multi modal registration to a value greater than 0.",
                       "Registration", DialogBoxIcon.Error);
                    return;
                }                
            }

            ObservableCollection<Point> fixedPointSet = new ObservableCollection<Point>();
            ObservableCollection<Point> movingPointSet = new ObservableCollection<Point>();

            if (RegistrationParameters.PointBased)
            {
                //Moving image
                switch (highResImage.PointSource)
                {
                    case PointSource.Selection:
                        movingPointSet = highResImage.SelectedPointsNormalized;
                        break;
                    case PointSource.LastSet:
                        try
                        {
                            movingPointSet = PointSet.PointSetFromFile(Registration.MovingPointSetLocation);
                        }
                        catch (FileNotFoundException fnfEx) 
                        {
                            DialogBox.Show("Moving points file not found.",
                                string.Format("Could not find file {0} to open. If Clear App Data is set to true, the file could have been deleted on last exit.", 
                                fnfEx.Message), "Point Set", DialogBoxIcon.Error);
                            return;
                        }
                        catch (Exception ex)
                        {
                            DialogBox.Show("Could not open recent points file.",
                                ex.Message, "Point Set", DialogBoxIcon.Error);
                            return;
                        }
                        break;
                    case PointSource.FromFile:
                        try
                        {
                            OpenFileDialog ofd = new OpenFileDialog();
                            ofd.Title = "Moving Image Point Set (High Res)";
                            ofd.Filter = "Moving Point Set File (.pts)|*.pts";

                            if (ofd.ShowDialog() != true) return;
                            movingPointSet = PointSet.PointSetFromFile(ofd.FileName);
                        }
                        catch (FileNotFoundException fnfEx)
                        {
                            DialogBox.Show("Moving points file not found.",
                                   string.Format("Could not find file {0} to open. If Clear App Data is set to true, the file could have been deleted on last exit.", 
                                   fnfEx.Message), "Point Set", DialogBoxIcon.Error);
                            return;
                        }
                        catch (Exception ex)
                        {
                            DialogBox.Show("Could not open points file.",
                                ex.Message, "Point Set", DialogBoxIcon.Error);
                            return;
                        }
                        break;
                }
                if (movingPointSet.Count == 0)
                {
                    DialogBox.Show("No points selected.",
                        "Select or load one or more points in the low resolution image or opt for non-point based registration.",
                        "Registration", DialogBoxIcon.Error);
                    return;
                }

                //Fixed image
                switch (lowResImage.PointSource)
                {
                    case PointSource.Selection:
                        fixedPointSet = lowResImage.SelectedPointsNormalized;
                        break;
                    case PointSource.LastSet:
                        try
                        {
                            fixedPointSet = PointSet.PointSetFromFile(Registration.FixedPointSetLocation);
                        }
                        catch (FileNotFoundException fnfEx)
                        {
                            DialogBox.Show("Points file not found.",
                                string.Format("Could not find file {0} to open.", fnfEx.Message), "Point Set", DialogBoxIcon.Error);
                            return;
                        }
                        catch (Exception ex)
                        {
                            DialogBox.Show("Could not open recent points file.",
                                ex.Message, "Point Set", DialogBoxIcon.Error);
                            return;
                        }
                        break;
                    case PointSource.FromFile:
                        try
                        {
                            OpenFileDialog ofd = new OpenFileDialog();
                            ofd.Title = "Fixed Image Point Set (Low Res)";
                            ofd.Filter = "Fixed Point Set File (.pts)|*.pts";

                            if (ofd.ShowDialog() != true) return;
                            fixedPointSet = PointSet.PointSetFromFile(ofd.FileName);
                        }
                        catch (FileNotFoundException fnfEx)
                        {
                            DialogBox.Show("Points file not found.",
                                   string.Format("Could not find file {0} to open.", fnfEx.Message), "Point Set", DialogBoxIcon.Error);
                            return;
                        }
                        catch (Exception ex)
                        {
                            DialogBox.Show("Could not open points file.",
                                ex.Message, "Point Set", DialogBoxIcon.Error);
                            return;
                        }
                        break;
                }
                if (fixedPointSet.Count == 0)
                {
                    DialogBox.Show("No points selected.",
                        "Select one or more points in the high resolution image or opt for non-point based registration.",
                        "Registration", DialogBoxIcon.Error);
                    return;
                }
            }

            _imageRegistration = new Registration(registrationProgressTextBox);            

            try
            {
                int resizedWidth = (int)HighResImage.ImageSource.PixelWidth;
                int resizedHeight = (int)HighResImage.ImageSource.PixelHeight;

                BitmapSource resizedLowRes = ImageHelper.CreateImage(ImageHelper.Upscale(ImageHelper.ConvertToData3D(LowResImage.ImageSource),
                    resizedWidth, resizedHeight));

                // If point based, resize normalized point selections to reflect resized dimensions.
                if (RegistrationParameters.PointBased)
                {
                    if (!isNormalized(fixedPointSet))
                    {
                        for (int i = 0; i < fixedPointSet.Count; i++)
                        {
                            Point current = fixedPointSet[i];
                            Point resized = new Point(current.X * resizedWidth, current.Y * resizedHeight);
                            fixedPointSet.Insert(i, resized);
                            fixedPointSet.Remove(current);
                        }
                    }

                    if (!isNormalized(movingPointSet))
                    {
                        for (int i = 0; i < movingPointSet.Count; i++)
                        {
                            Point current = movingPointSet[i];
                            Point resized = new Point(current.X * resizedWidth, current.Y * resizedHeight);
                            movingPointSet.Insert(i, resized);
                            movingPointSet.Remove(current);
                        }
                    }
                    
                }

                RegistrationParameters finalParams = RegistrationParameters.ToRegistrationParameters();

                // Get and scale accordingly the defined ROIs
                Point fixedROITopLeft = lowResImage.ROITopLeftNormalized;
                Point fixedROIBotRight = lowResImage.ROIBottomRightNormalized;
                finalParams.SetROIFixed(fixedROITopLeft.X * resizedWidth, fixedROITopLeft.Y * resizedHeight,
                    fixedROIBotRight.X * resizedWidth, fixedROIBotRight.Y * resizedHeight);

                Point movingROITopLeft = highResImage.ROITopLeftNormalized;
                Point movingROIBotRight = highResImage.ROIBottomRightNormalized;
                finalParams.SetROIMoving(movingROITopLeft.X * resizedWidth, movingROITopLeft.Y * resizedHeight,
                    movingROIBotRight.X * resizedWidth, movingROIBotRight.Y * resizedHeight);

                _imageRegistration.InitializeRegistration(regType, resizedLowRes, HighResImage.ImageSource, 
                    fixedPointSet, movingPointSet, finalParams);
            }
            catch (Exception ex)
            {
                DialogBox.Show("Could not initialize image registration.", ex.Message, "Image Registration", DialogBoxIcon.Error);
                return;
            }

            tabControlOutputs.SelectedIndex = 0;

            IsRegistering = true;
            await _imageRegistration.RegisterAsync();
            IsRegistering = false;

            if (_imageRegistration.RegistrationSucceeded)
            {
                _imageRegistration.LoadRegisteredImages();

                _lastHighRes = HighResImage.Clone();
                _lastLowRes = LowResImage.Clone();
                CanUndoRegistration = true;

                HighResImage.ImageSource = _imageRegistration.RegisteredMovingImage;
                LowResImage.ImageSource = _imageRegistration.RegisteredFixedImage;
               
                HighResImage.IsRegistered = true;
                LowResImage.IsRegistered = true;

                RegistrationResults = _imageRegistration.RegistrationResults;
                IsRegistered = true;
            }

            _imageRegistration = null;
        }
        private void cancelRegistration_Click(object sender, RoutedEventArgs e)
        {
            if (_imageRegistration == null) return;
            _imageRegistration.RequestCancel();
        }
        private void undoRegistration_Click(object sender, RoutedEventArgs e)
        {
            if (_lastHighRes == null || _lastLowRes == null)
            {
                DialogBox.Show("Could not undo the image registration.", 
                    "The previous images are missing.", "Undo", DialogBoxIcon.Error);
            }

            HighResImage = _lastHighRes;
            LowResImage = _lastLowRes;

            _lastLowRes = null;
            _lastHighRes = null;

            IsRegistered = false;

            CanUndoRegistration = false;

            if (_overlayWindow != null)
            {
                _overlayWindow.FixedImageSource = LowResImage.ImageSource;
                _overlayWindow.MovingImageSource = HighResImage.ImageSource;
            }
        }
        private async void doFusion_Click(object sender, RoutedEventArgs e)
        {
            if (HighResImage.ImageSource == null)
            {
                DialogBox db = new DialogBox("No high resolution image loaded.",
                    "Drop or load a high resolution image into the tab and try again.", "Fusion", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }
            if (LowResImage.ImageSource == null)
            {
                DialogBox db = new DialogBox("No low resolution image loaded.",
                    "Drop or load a low resolution image into the tab and try again.", "Fusion", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }
            if (HighResImage.ImageSource.PixelWidth < LowResImage.ImageSource.PixelWidth ||
                HighResImage.ImageSource.PixelHeight < LowResImage.ImageSource.PixelHeight)
            {
                DialogBox db = new DialogBox("Invalid image size.",
                    "One or both of the dimensions of the high resolution image is(are) smaller than the low resolution image.", "Fusion", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            FusionType fusionType = (FusionType)comboFusionMethod.SelectedItem;

            if ((!HighResImage.IsRegistered && !LowResImage.IsRegistered) && 
                !SettingsManager.RegSettings.SuppressRegistrationWarnings)
            {
                if (fusionType != FusionType.HSLShift)
                {
                    if (DialogBox.Show("Input images are not registered.", "Click OK to proceed with fusion anyway or Cancel to return.",
                        "Registration", DialogBoxIcon.Help, true) != true) return;
                }
            }

            Data2D highRes = new Data2D();
            if (radioColor.IsChecked == true)
            {
                highRes = ImageHelper.ConvertToData2D(HighResImage.ImageSource,
                    Data2DConverionType.Color, colorSelector.SelectedColor);
            }
            else if (radioGray.IsChecked == true)
            {
                highRes = ImageHelper.ConvertToData2D(HighResImage.ImageSource, Data2DConverionType.Grayscale);
            }
            else if (radioThermal.IsChecked == true)
            {
                highRes = ImageHelper.ConvertToData2D(HighResImage.ImageSource, Data2DConverionType.Thermal);
            }
            Data3D lowRes = ImageHelper.ConvertToData3D(LowResImage.ImageSource);

            if (fusionType == FusionType.HSLShift && ShiftWindowSize <= 0)
            {
                DialogBox.Show("Invalid window size.", "Window size must be greater than 0.",
                    "Fusion", DialogBoxIcon.Error);
                return;
            }

            Fusion fusion;

            switch (fusionType)
            {
                case FusionType.HSL:
                    fusion = new HSLFusion(highRes, lowRes);
                    break;
                case FusionType.WeightedAverage:
                    fusion = new WeightedAverageFusion(highRes, lowRes);
                    break;
                case FusionType.HSLSmooth:
                    fusion = new HSLSmoothFusion(highRes, lowRes);
                    break;
                case FusionType.Adaptive:
                    fusion = new AdaptiveIHSFusion(highRes, lowRes);
                    break;
                case FusionType.HSLShift:
                    fusion = new HSLShiftFusion(highRes, lowRes);
                    ((HSLShiftFusion)fusion).WindowSize = ShiftWindowSize;
                    break;
                default:
                    Mouse.OverrideCursor = Cursors.Arrow;
                    return;
            }

            try
            {
                if (fusion is Pansharpening)
                    ((Pansharpening)fusion).CheckFusion();

                Mouse.OverrideCursor = Cursors.Wait;

                Data3D result = await fusion.DoFusionAsync();
                FusedImage.ImageSource = ImageHelper.CreateImage(result);

                AnalysisResults = "";

                if (fusion is HSLShiftFusion)
                {
                    if (((HSLShiftFusion)fusion).ShiftCalculationCompleted)
                    {
                        Point shift = ((HSLShiftFusion)fusion).ShiftSize;
                        Point newCenter = ((HSLShiftFusion)fusion).NewCenter;
                        double distance = ((HSLShiftFusion)fusion).DistanceToCenter;

                        textBoxCC.Text = string.Format("Shift results:\nShift size: ({0},{1})\nNew center: ({2},{3})\nDistance to center: {4}",
                            shift.X, shift.Y, newCenter.X, newCenter.Y, distance);
                        tabControlOutputs.SelectedIndex = 1;
                    }
                }

                updateClosableTabItem("Fusion complete.");                
            }
            catch (Exception ex)
            {
                DialogBox db = new DialogBox("There was a problem fusing the two images.", ex.Message, "Fusion", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }
        private async void buttonQps_Click(object sender, RoutedEventArgs e)
        {
            if (FusedImage.ImageSource == null)
            {
                DialogBox db = new DialogBox("No fused image to analyze.", "You must first perform image fusion to analyze the result.",
                    "QPS", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }
            if (LowResImage.ImageSource == null)
            {
                DialogBox db = new DialogBox("No low resolution image to compare.", "You must have a low resolution image to compare the result to.",
                    "QPS", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }
            if (HighResImage.ImageSource == null)
            {
                DialogBox db = new DialogBox("No high resolution image to compare.", "You must have a high resolution image to compare the result to.",
                    "QPS", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            CrossCorrelationResults results = await CrossCorrelation.AnalyzeAsync(LowResImage.ImageSource,
                FusedImage.ImageSource);

            string message = string.Format(
                "Results\n" +
                "---------------------------\n" +
                "ccRed =    {0}\n" +
                "ccGreen =  {1}\n" +
                "ccBlue =   {2}\n" +
                "ccAvg =    {3}\n",
                results.R.ToString("0.0000"),
                results.G.ToString("0.0000"),
                results.B.ToString("0.0000"),
                ((results.R + results.G + results.B) / 3).ToString("0.0000"));

            textBoxCC.Text = message;
            tabControlOutputs.SelectedIndex = 1;

            updateClosableTabItem("QPS analysis complete.");
        }
        private void buttonQpsCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(textBoxCC.Text);
        }

        private void saveImage_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap Images (.bmp)|*.bmp";
            Nullable<bool> result = sfd.ShowDialog();
            if (result != true) return;

            string error = "";
            Button b = (Button)sender;

            using (var fileStream = new FileStream(sfd.FileName, FileMode.Create))
            {
                //if (b == hrSave)
                //{
                //    try
                //    {
                //        BitmapEncoder encoder = new PngBitmapEncoder();
                //        encoder.Frames.Add(BitmapFrame.Create(HighResImage.ImageSource));
                //        encoder.Save(fileStream);
                //    }
                //    catch (Exception ex)
                //    {
                //        error = ex.Message;
                //    }

                //}
                //else if (b == lrSave)
                //{
                //    try
                //    {
                //        BitmapEncoder encoder = new PngBitmapEncoder();
                //        encoder.Frames.Add(BitmapFrame.Create(LowResImage.ImageSource));
                //        encoder.Save(fileStream);
                //    }
                //    catch (Exception ex)
                //    {
                //        error = ex.Message;
                //    }
                //} else
                if (b == fSave)
                {
                    try
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(FusedImage.ImageSource));
                        encoder.Save(fileStream);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message;
                    }
                }
                else error = "Event sender not converted.";

                DialogBox db;
                if (error == "")
                {
                    db = new DialogBox("File saved successfully!", System.IO.Path.GetFileName(sfd.FileName), "Save", DialogBoxIcon.Ok);
                    db.ShowDialog();
                }
                else
                {
                    db = new DialogBox("Could not save the image.", error, "Save", DialogBoxIcon.Error);
                    db.ShowDialog();
                }
            }
        }
        private void copyImage_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;

            //if (b == hrCopy)
            //{
            //    if (HighResImage.ImageSource != null)
            //    {
            //        Clipboard.SetImage(HighResImage.ImageSource);
            //        updateClosableTabItem("High resolution image copied to clipboard.");
            //    }
            //}
            //else if (b == lrCopy)
            //{
            //    if (LowResImage.ImageSource != null)
            //    {
            //        Clipboard.SetImage(LowResImage.ImageSource);
            //        updateClosableTabItem("Low resolution image copied to clipboard.");
            //    }
            //} else
            if (b == fCopy)
            {
                if (FusedImage.ImageSource != null)
                {
                    Clipboard.SetImage(FusedImage.ImageSource);
                    updateClosableTabItem("Fused image copied to clipboard.");
                }
            }
        }
        private void removeImage_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            //if (b == hrRemove)
            //{
            //    HighResImage.ImageSource = null;
            //}
            //else if (b == lrRemove)
            //{
            //    LowResImage.ImageSource = null;
            //} else
            if (b == fRemove)
            {
                FusedImage.ImageSource = null;
                AnalysisResults = "";
            }
        }

        private void copy()
        {

        }
        private void flip(bool horizontal)
        {
            //foreach (object obj in itemsControl.SelectedItems)
            //{
            //    SEM sem = (SEM)obj;
            //    if (sem == null) return;

            //    Image image = sem.SEMImage;
            //    BitmapSource bs = (BitmapSource)image.Source;
            //    TransformedBitmap transformedImage = new TransformedBitmap();
            //    transformedImage.BeginInit();
            //    transformedImage.Source = bs;
            //    if (Horizontal) transformedImage.Transform = new ScaleTransform(-1, 1);
            //    else transformedImage.Transform = new ScaleTransform(1, -1);
            //    transformedImage.EndInit();

            //    image.Source = transformedImage;
            //}
        }
        private void rotate(bool clockwise)
        {
            //foreach (object obj in itemsControl.SelectedItems)
            //{
            //    SEM sem = (SEM)obj;
            //    if (sem == null) return;

            //    Image image = sem.SEMImage;
            //    BitmapSource bs = (BitmapSource)image.Source;
            //    TransformedBitmap transformedImage = new TransformedBitmap();
            //    transformedImage.BeginInit();
            //    transformedImage.Source = bs;
            //    if (Clockwise) transformedImage.Transform = new RotateTransform(90);
            //    else transformedImage.Transform = new RotateTransform(270);
            //    transformedImage.EndInit();

            //    image.Source = transformedImage;
            //}
        }

        public void SetLowRes(BitmapSource ImageSource)
        {
            this.LowResImage.ImageSource = ImageSource;
        }
        public void SetHighRes(BitmapSource ImageSource)
        {
            this.HighResImage.ImageSource = ImageSource;
        }
        public void CallSave(FusionSaveParameter Parameter)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap Images (.bmp)|*.bmp";
            Nullable<bool> result = sfd.ShowDialog();
            if (result != true) return;

            string baseFileName = sfd.FileName;

            switch (Parameter)
            {
                case FusionSaveParameter.Fused:
                    using (FileStream stream = new FileStream(baseFileName, FileMode.Create))
                    {
                        try
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(FusedImage.ImageSource));
                            encoder.Save(stream);

                            DialogBox db = new DialogBox("File saved successfully!",
                            System.IO.Path.GetFileName(sfd.FileName), "Save", DialogBoxIcon.Ok);
                            db.ShowDialog();
                        }
                        catch (Exception ex)
                        {
                            DialogBox db = new DialogBox("Could not save the image.", ex.Message, "Save", DialogBoxIcon.Error);
                            db.ShowDialog();
                            return;
                        }
                    }
                    break;
                case FusionSaveParameter.HighRes:
                    using (FileStream stream = new FileStream(baseFileName, FileMode.Create))
                    {
                        try
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(HighResImage.ImageSource));
                            encoder.Save(stream);

                            DialogBox db = new DialogBox("File saved successfully!",
                            System.IO.Path.GetFileName(sfd.FileName), "Save", DialogBoxIcon.Ok);
                            db.ShowDialog();
                        }
                        catch (Exception ex)
                        {
                            DialogBox db = new DialogBox("Could not save the image.", ex.Message, "Save", DialogBoxIcon.Error);
                            db.ShowDialog();
                            return;
                        }
                    }
                    break;
                case FusionSaveParameter.LowRes:
                    using (FileStream stream = new FileStream(baseFileName, FileMode.Create))
                    {
                        try
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(LowResImage.ImageSource));
                            encoder.Save(stream);

                            DialogBox db = new DialogBox("File saved successfully!",
                            System.IO.Path.GetFileName(sfd.FileName), "Save", DialogBoxIcon.Ok);
                            db.ShowDialog();
                        }
                        catch (Exception ex)
                        {
                            DialogBox db = new DialogBox("Could not save the image.", ex.Message, "Save", DialogBoxIcon.Error);
                            db.ShowDialog();
                            return;
                        }
                    }
                    break;
                case FusionSaveParameter.Series:
                    List<KeyValuePair<BitmapSource, string>> errors = new List<KeyValuePair<BitmapSource, string>>();
                    using (FileStream stream = new FileStream(baseFileName.Insert(baseFileName.Length - 4, "-Low"), FileMode.Create))
                    {
                        try
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(LowResImage.ImageSource));
                            encoder.Save(stream);
                        }
                        catch (Exception ex)
                        {
                            errors.Add(new KeyValuePair<BitmapSource, string>(LowResImage.ImageSource, ex.Message));
                        }
                    }
                    using (FileStream stream = new FileStream(baseFileName.Insert(baseFileName.Length - 4, "-High"), FileMode.Create))
                    {
                        try
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(HighResImage.ImageSource));
                            encoder.Save(stream);
                        }
                        catch (Exception ex)
                        {
                            errors.Add(new KeyValuePair<BitmapSource, string>(HighResImage.ImageSource, ex.Message));
                        }
                    }
                    using (FileStream stream = new FileStream(baseFileName.Insert(baseFileName.Length - 4, "-Fused"), FileMode.Create))
                    {
                        try
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(FusedImage.ImageSource));
                            encoder.Save(stream);
                        }
                        catch (Exception ex)
                        {
                            errors.Add(new KeyValuePair<BitmapSource, string>(FusedImage.ImageSource, ex.Message));
                        }
                    }

                    if (errors.Count == 0)
                    {
                        DialogBox db = new DialogBox("File(s) saved successfully!",
                            System.IO.Path.GetFileName(sfd.FileName), "Save", DialogBoxIcon.Ok);
                        db.ShowDialog();
                    }
                    else
                    {
                        string errorList = "";
                        foreach (KeyValuePair<BitmapSource, string> kvp in errors)
                        {
                            errorList += kvp.Value;
                            errorList += "\n";
                        }
                        errorList.Remove(errorList.Length - 2);

                        DialogBox db = new DialogBox("The images listed below could not be saved. Other images not listed were saved successfully.",
                            errorList, "Save", DialogBoxIcon.Information);
                        db.ShowDialog();
                    }
                    break;
            }
        }
        public void CallEvent(ImageTabEvent EventType)
        {
            switch (EventType)
            {
                case ImageTabEvent.Copy:
                    copy();
                    break;
                case ImageTabEvent.FlipHorizontal:
                    flip(true);
                    break;
                case ImageTabEvent.FlipVertical:
                    flip(false);
                    break;
                case ImageTabEvent.RotateClock:
                    rotate(true);
                    break;
                case ImageTabEvent.RotateCounter:
                    rotate(false);
                    break;
            }
        }

        public void OpenOverlay()
        {
            _overlayWindow = new ImageOverlayWindow();

            _overlayWindow.FixedImageSource = LowResImage.ImageSource;
            _overlayWindow.MovingImageSource = HighResImage.ImageSource;

            _overlayWindow.ParametersGenerated += _overlayWindow_ParametersGenerated;
            _overlayWindow.Closed += _overlayWindow_Closed;

            _overlayWindow.Show();
           
        }

        void _overlayWindow_Closed(object sender, EventArgs e)
        {
            _overlayWindow.ParametersGenerated -= _overlayWindow_ParametersGenerated;
            _overlayWindow.Closed -= _overlayWindow_Closed;
            _overlayWindow = null;
        }

        private bool isNormalized(ObservableCollection<Point> pointSet)
        {
            foreach (Point p in pointSet)
            {
                if (p.X > 1d || p.Y > 1d) return true;
            }
            return false;
        }

        private void updateClosableTabItem(string message)
        {
            if (ClosableTabItem.IsClosableTabItemHosted(this))
            {
                ClosableTabItem.SendStatusUpdate(this, message);
            }
        }

        private void fusedImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (FusedImage.ImageSource == null) return;

                DataObject obj = new DataObject("BitmapSource", FusedImage.ImageSource);
                DragDrop.DoDragDrop(this, obj, DragDropEffects.Copy);
            }
        }

        ~FusionTab()
        {
            Dispose(false);
        }
        bool disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                HighResImage.ImageSourceChanged -= inputImage_ImageSourceChanged;
                LowResImage.ImageSourceChanged -= inputImage_ImageSourceChanged;
                HighResImage = null;
                LowResImage = null;

                if (_overlayWindow != null)
                {
                    _overlayWindow.ParametersGenerated -= _overlayWindow_ParametersGenerated;
                    _overlayWindow.Closed -= _overlayWindow_Closed;
                    _overlayWindow = null;
                }
            }
            disposed = true;
        }
    }

    public class FusionImageDisplayItem : INotifyPropertyChanged
    {
        BitmapSource _imageSource;
        FusionImageType _imageType;
        bool _isRegistered;

        public BitmapSource ImageSource
        {
            get { return _imageSource; }
            set
            {
                if (_imageSource != value)
                {
                    _imageSource = value;
                    NotifyPropertyChanged("ImageSource");
                    OnImageSourceChanged();
                }
            }
        }
        public FusionImageType ImageType
        {
            get { return _imageType; }
            set
            {
                if (_imageType != value)
                {
                    _imageType = value;
                    NotifyPropertyChanged("ImageType");
                }
            }
        }
        public bool IsRegistered
        {
            get { return _isRegistered; }
            set
            {
                if (_isRegistered != value)
                {
                    _isRegistered = value;
                    NotifyPropertyChanged("IsRegistered");
                }
            }
        }

        public FusionImageDisplayItem(FusionImageType Type)
        {
            this.ImageSource = null;
            this.ImageType = Type;
            this.IsRegistered = false;
        }
        public FusionImageDisplayItem(BitmapSource ImageSource, FusionImageType Type)
        {
            this.ImageSource = ImageSource;
            this.ImageType = Type;
            this.IsRegistered = false;
        }
        public FusionImageDisplayItem(BitmapSource ImageSource, FusionImageType Type, bool IsRegistered)
        {
            this.ImageSource = ImageSource;
            this.ImageType = Type;
            this.IsRegistered = IsRegistered;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event EventHandler ImageSourceChanged;
        private void OnImageSourceChanged()
        {
            if (ImageSourceChanged != null)
            {
                ImageSourceChanged(this, EventArgs.Empty);
            }
        }

        public FusionImageDisplayItem Clone()
        {
            return new FusionImageDisplayItem(this.ImageSource.Clone(), this.ImageType, this.IsRegistered);
        }
    }

    public enum FusionImageType
    {
        LowRes, HighRes, Fused
    }

    public enum FusionSaveParameter
    {
        Series, HighRes, LowRes, Fused, None
    }

    public class FusionEnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;

            try
            {
                FusionType ft = (FusionType)value;
                if (ft == FusionType.HSLShift) return Visibility.Visible;
                else return Visibility.Collapsed;
            }
            catch (Exception)
            {
                return Visibility.Collapsed;
            }
        }
        public object ConvertBack(object value, Type targetType, object argument, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }    
}
