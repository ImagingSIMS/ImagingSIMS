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
using ImagingSIMS.Common.Math;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;
using ImagingSIMS.ImageRegistration;

namespace ImagingSIMS.Controls.Tabs
{
    /// <summary>
    /// Interaction logic for DataRegistrationTab.xaml
    /// </summary>
    public partial class DataRegistrationTab : UserControl
    {
        public static readonly DependencyProperty TransformParametersProperty = DependencyProperty.Register("TransformParameters",
            typeof(TransformParameters), typeof(DataRegistrationTab));
        public static readonly DependencyProperty MovingImageProperty = DependencyProperty.Register("MovingImage",
            typeof(BitmapSource), typeof(DataRegistrationTab));
        public static readonly DependencyProperty FixedImageProperty = DependencyProperty.Register("FixedImage",
            typeof(BitmapSource), typeof(DataRegistrationTab));

        public TransformParameters TransformParameters
        {
            get { return (TransformParameters)GetValue(TransformParametersProperty); }
            set { SetValue(TransformParametersProperty, value); }
        }
        public BitmapSource MovingImage
        {
            get { return (BitmapSource)GetValue(MovingImageProperty); }
            set { SetValue(MovingImageProperty, value); }
        }
        public BitmapSource FixedImage
        {
            get { return (BitmapSource)GetValue(FixedImageProperty); }
            set { SetValue(FixedImageProperty, value); }
        }

        ObservableCollection<Data2D> _availableTables;
        ObservableCollection<Data2D> _selectedTablesMoving;
        ObservableCollection<Data2D> _selectedTablesFixed;

        public ObservableCollection<Data2D> SelectedTablesMoving
        {
            get { return _selectedTablesMoving; }
            set { _selectedTablesMoving = value; }
        }
        public ObservableCollection<Data2D> SelectedTablesFixed
        {
            get { return _selectedTablesFixed; }
            set { _selectedTablesFixed = value; }
        }
        public ObservableCollection<Data2D> AvailableTables
        {
            get { return _availableTables; }
            set { _availableTables = value; }
        }

        public DataRegistrationTab()
        {
            SelectedTablesMoving = new ObservableCollection<Data2D>();
            SelectedTablesFixed = new ObservableCollection<Data2D>();
            AvailableTables = new ObservableCollection<Data2D>();

            TransformParameters = new ImageRegistration.TransformParameters()
            {
                TransformType = ImageRegistrationTypes.Translation,
                TranslationX = 0,
                TranslationY = 0,
                Angle = 0,
                Scale = 0,
                RotationCenterX = 0,
                RotationCenterY = 0
            };

            InitializeComponent();
        }

        public void SetAvailableTables(ObservableCollection<Data2D> Tables)
        {
            this.AvailableTables = Tables;
        }
        public void SetRegisteredImages(BitmapSource MovingImage, BitmapSource FixedImage)
        {
            this.MovingImage = MovingImage;
            this.FixedImage = FixedImage;
        }
        public void SetTransform(RegistrationResult RegistrationResults)
        {
            TransformParameters = new TransformParameters();

            TransformParameters.TranslationX = RegistrationResults.TranslationX;
            TransformParameters.TranslationY = RegistrationResults.TranslationY;
            TransformParameters.Angle = RegistrationResults.Angle;
            TransformParameters.Scale = RegistrationResults.Scale;
            TransformParameters.RotationCenterX = RegistrationResults.RotationCenterX;
            TransformParameters.RotationCenterY = RegistrationResults.RotationCenterY;

            if (TransformParameters.Scale != 1)
            {
                TransformParameters.TransformType = ImageRegistrationTypes.CenterSimilarity;
            }
            else if (TransformParameters.Angle != 0)
            {
                TransformParameters.TransformType = ImageRegistrationTypes.CenterRigid2D;
            }
            else TransformParameters.TransformType = ImageRegistrationTypes.Translation;
        }
        public void SetMovingImage(BitmapSource MovingImage)
        {
            this.MovingImage = MovingImage;
        }
        public void SetFixedImage(BitmapSource FixedImage)
        {
            this.FixedImage = FixedImage;
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b == null) return;

            if (b == buttonAddFixed) addFixedTables();
            else if (b == buttonAddMoving) addMovingTables();
        }
        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b == null) return;

            if (b == buttonRemoveFixed) removeFixedTables();
            else if (b == buttonRemoveMoving) removeMovingTables();
        }

        private void addFixedTables()
        {
            List<KeyValuePair<Data2D, string>> notAdded = new List<KeyValuePair<Data2D, string>>();

            foreach (Data2D d in AvailableHost.AvailableTablesSource.GetSelectedTables())
            {
                try
                {
                    if (SelectedTablesFixed.Contains(d))
                        throw new ArgumentException("Selected list already contains this table.");
                    SelectedTablesFixed.Add(d);
                }
                catch (ArgumentException ARex)
                {
                    notAdded.Add(new KeyValuePair<Data2D, string>(d, ARex.Message));
                }
                catch (Exception ex)
                {
                    notAdded.Add(new KeyValuePair<Data2D, string>(d, ex.Message));
                }
            }

            if (notAdded.Count > 0)
            {
                string list = "";
                foreach (KeyValuePair<Data2D, string> kvp in notAdded)
                {
                    list += string.Format("{0}: {1}\n", kvp.Key.ToString(), kvp.Value);
                }
                list.Remove(list.Length - 2, 2);

                DialogBox.Show("The following tables were not added to the selected list:", list, "Transform", DialogIcon.Warning);
            }
        }
        private void addMovingTables()
        {
            List<KeyValuePair<Data2D, string>> notAdded = new List<KeyValuePair<Data2D, string>>();

            foreach (Data2D d in AvailableHost.AvailableTablesSource.GetSelectedTables())
            {
                try
                {
                    if (SelectedTablesMoving.Contains(d))
                        throw new ArgumentException("Selected list already contains this table.");
                    SelectedTablesMoving.Add(d);
                }
                catch (ArgumentException ARex)
                {
                    notAdded.Add(new KeyValuePair<Data2D, string>(d, ARex.Message));
                }
                catch (Exception ex)
                {
                    notAdded.Add(new KeyValuePair<Data2D, string>(d, ex.Message));
                }
            }

            if (notAdded.Count > 0)
            {
                string list = "";
                foreach (KeyValuePair<Data2D, string> kvp in notAdded)
                {
                    list += string.Format("{0}: {1}\n", kvp.Key.ToString(), kvp.Value);
                }
                list.Remove(list.Length - 2, 2);

                DialogBox.Show("The following tables were not added to the selected list:", list, "Transform", DialogIcon.Warning);
            }
        }
        private void removeFixedTables()
        {
            List<KeyValuePair<Data2D, string>> notRemoved = new List<KeyValuePair<Data2D, string>>();

            Data2D[] toRemove = new Data2D[listViewSelectedTablesFixed.SelectedItems.Count];
            int i = 0;
            foreach (object obj in listViewSelectedTablesFixed.SelectedItems)
            {
                toRemove[i] = listViewSelectedTablesFixed.SelectedItems[i] as Data2D;
                i++;
            }

            for (int j = 0; j < toRemove.Length; j++)
            {
                try
                {
                    if (toRemove[j] == null)
                        throw new ArgumentNullException("Could not convert item to data table.");
                    if (!SelectedTablesFixed.Contains(toRemove[j]))
                        throw new ArgumentException("Selected list does not contain the table.");
                    SelectedTablesFixed.Remove(toRemove[j]);
                }
                catch (ArgumentNullException ANex)
                {
                    notRemoved.Add(new KeyValuePair<Data2D, string>(toRemove[j], ANex.Message));
                }
                catch (ArgumentException ARex)
                {
                    notRemoved.Add(new KeyValuePair<Data2D, string>(toRemove[j], ARex.Message));
                }
                catch (Exception ex)
                {
                    notRemoved.Add(new KeyValuePair<Data2D, string>(toRemove[j], ex.Message));
                }
            }

            if (notRemoved.Count > 0)
            {
                string list = "";
                foreach (KeyValuePair<Data2D, string> kvp in notRemoved)
                {
                    list += string.Format("{0}: {1}\n", kvp.Key.ToString(), kvp.Value);
                }
                list.Remove(list.Length - 2, 2);

                DialogBox.Show("The following tables were not removed from the selected tables list:", list, "Transform", DialogIcon.Warning);
            }
        }
        private void removeMovingTables()
        {
            List<KeyValuePair<Data2D, string>> notRemoved = new List<KeyValuePair<Data2D, string>>();

            Data2D[] toRemove = new Data2D[listViewSelectedTablesMoving.SelectedItems.Count];
            int i = 0;
            foreach (object obj in listViewSelectedTablesMoving.SelectedItems)
            {
                toRemove[i] = listViewSelectedTablesMoving.SelectedItems[i] as Data2D;
                i++;
            }

            for (int j = 0; j < toRemove.Length; j++)
            {
                try
                {
                    if (toRemove[j] == null)
                        throw new ArgumentNullException("Could not convert item to data table.");
                    if (!SelectedTablesMoving.Contains(toRemove[j]))
                        throw new ArgumentException("Selected list does not contain the table.");
                    SelectedTablesMoving.Remove(toRemove[j]);
                }
                catch (ArgumentNullException ANex)
                {
                    notRemoved.Add(new KeyValuePair<Data2D, string>(toRemove[j], ANex.Message));
                }
                catch (ArgumentException ARex)
                {
                    notRemoved.Add(new KeyValuePair<Data2D, string>(toRemove[j], ARex.Message));
                }
                catch (Exception ex)
                {
                    notRemoved.Add(new KeyValuePair<Data2D, string>(toRemove[j], ex.Message));
                }
            }

            if (notRemoved.Count > 0)
            {
                string list = "";
                foreach (KeyValuePair<Data2D, string> kvp in notRemoved)
                {
                    list += string.Format("{0}: {1}\n", kvp.Key.ToString(), kvp.Value);
                }
                list.Remove(list.Length - 2, 2);

                DialogBox.Show("The following tables were not removed from the selected tables list:", list, "Transform", DialogIcon.Warning);
            }
        }

        private void buttonResetParameters_Click(object sender, RoutedEventArgs e)
        {
            TransformParameters.TranslationX = 0;
            TransformParameters.TranslationY = 0;
            TransformParameters.Angle = 0;
            TransformParameters.RotationCenterX = 0;
            TransformParameters.RotationCenterY = 0;
            TransformParameters.Scale = 1;
        }
        private void buttonInvertParameters_Click(object sender, RoutedEventArgs e)
        {
            TransformParameters.Invert();
        }
        private async void buttonTransform_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTablesFixed.Count == 0 && _selectedTablesMoving.Count == 0)
            {
                DialogBox.Show("No tables selected.", "Add one or more tables to the moving and/or fixed list and try again.",
                    "Transform", DialogIcon.Error);
                return;
            }
            // Do dimension check and find target width and height
            int targetWidth = -1;
            int targetHeight = -1;

            foreach (Data2D d in _selectedTablesMoving)
            {
                if (targetWidth == -1 && targetHeight == -1)
                {
                    targetWidth = d.Width;
                    targetHeight = d.Height;

                    continue;
                }

                if (targetWidth != d.Width || targetHeight != d.Height)
                {
                    DialogBox.Show("Dimensions are not valid.",
                        "One or more of the selected moving tables does not match the dimensions of the others.", 
                        "Transform", DialogIcon.Error);
                    return;
                }
            }
            // Set up ProgressWindow to track overall progress
            ProgressWindow pw = new ProgressWindow("Transforming selected tables. Please wait...", "Transform");
            pw.Show();

            int numTablesTransformed = 0;
            int numTablesToTransform = _selectedTablesFixed.Count + _selectedTablesMoving.Count;

            // Transform each moving data table

            List<Data2D> failed = new List<Data2D>();

            foreach (Data2D d in _selectedTablesMoving)
            {
                DataTransformWrapper transform = new DataTransformWrapper();
                transform.InitializeDataTransform(TransformParameters, d, true);

                await transform.TransformAsync();

                if (transform.TransformSucceeded)
                {
                    Data2D result = transform.FinalizeDataTransform();
                    AvailableTables.Add(result);
                }

                else
                {
                    failed.Add(d);
                }

                numTablesTransformed++;
                pw.UpdateProgress(Percentage.GetPercent(numTablesTransformed, numTablesToTransform));
            }

            // Transform each fixed data table with upscaling to match moving dimensions
            // and setting isMovingImage to false

            foreach (Data2D d in _selectedTablesFixed)
            {
                DataTransformWrapper transform = new DataTransformWrapper();

                Data2D upscaled = d.Resize(targetWidth, targetHeight);

                transform.InitializeDataTransform(TransformParameters, upscaled, false);

                await transform.TransformAsync();

                if (transform.TransformSucceeded)
                {
                    Data2D result = transform.FinalizeDataTransform();
                    AvailableTables.Add(result);
                }

                else
                {
                    failed.Add(d);
                }

                numTablesTransformed++;
                pw.UpdateProgress(Percentage.GetPercent(numTablesTransformed, numTablesToTransform));
            }

            pw.ProgressFinished();
            pw.Close();
            pw = null;

            if (failed.Count > 0)
            {
                string line = "";
                foreach (Data2D d in failed)
                {
                    line += d.DataName + "\n";
                }
                line = line.Remove(line.Length - 2, 2);

                DialogBox.Show("The following tables could not be transformed:", 
                    line, "Transform", DialogIcon.Warning);
            }
        }

        private void movingImage_Drop(object sender, DragEventArgs e)
        {
            doDrop(e, true);
        }
        private void fixedImage_Drop(object sender, DragEventArgs e)
        {
            doDrop(e, false);
        }

        private void doDrop(DragEventArgs e, bool moving)
        {
            BitmapSource bs;

            if (e.Data.GetDataPresent(DataFormats.Bitmap))
            {
                bs = e.Data.GetData(DataFormats.Bitmap) as BitmapSource;                
            }
            else if (e.Data.GetDataPresent("DisplayImage"))
            {
                DisplayImage image = e.Data.GetData("DisplayImage") as DisplayImage;
                if (image == null) return;

                bs = image.Source as BitmapSource;
            }
            else if (e.Data.GetDataPresent("Data2D"))
            {
                Data2D data = e.Data.GetData("Data2D") as Data2D;
                if(data==null)return;

                bs = ImageHelper.CreateColorScaleImage(data, ColorScaleTypes.ThermalWarm);
            }
            else return;

            if (bs == null) return;

            if (moving)
            {
                MovingImage = bs;
            }
            else FixedImage = bs;

            e.Handled = true;
        }
    }    
}
