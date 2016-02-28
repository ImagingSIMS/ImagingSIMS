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

using ImagingSIMS.Common;
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Controls.Tabs;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;
using ImagingSIMS.Data.Rendering;

namespace ImagingSIMS.Controls.Tabs
{
    /// <summary>
    /// Interaction logic for VolumeTab.xaml
    /// </summary>
    public partial class VolumeTab : UserControl
    {
        public Volume CreatedVolume
        {
            get;
            set;
        }
        public static readonly DependencyProperty VolumeNameProperty = DependencyProperty.Register("VolumeName",
            typeof(string), typeof(VolumeTab));
        public static readonly DependencyProperty RepeatLayersProperty = DependencyProperty.Register("RepeatLayers",
            typeof(bool), typeof(VolumeTab));
        public static readonly DependencyProperty LayerLengthProperty = DependencyProperty.Register("LayerLength",
            typeof(int), typeof(VolumeTab));
        public static readonly DependencyProperty AddBlankLayersProperty = DependencyProperty.Register("AddBlankLayers",
            typeof(bool), typeof(VolumeTab));
        public static readonly DependencyProperty BlankLayerLengthProperty = DependencyProperty.Register("BlankLayerLength",
            typeof(int), typeof(VolumeTab));

        public static readonly RoutedEvent VolumeCreatedEvent = EventManager.RegisterRoutedEvent("VolumeCreated",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VolumeTab));

        public string VolumeName
        {
            get { return (string)GetValue(VolumeNameProperty); }
            set { SetValue(VolumeNameProperty, value); }
        }
        public bool RepeatLayers
        {
            get { return (bool)GetValue(RepeatLayersProperty); }
            set { SetValue(RepeatLayersProperty, value); }
        }
        public int LayerLength
        {
            get { return (int)GetValue(LayerLengthProperty); }
            set { SetValue(LayerLengthProperty, value); }
        }
        public bool AddBlankLayers
        {
            get { return (bool)GetValue(AddBlankLayersProperty); }
            set { SetValue(AddBlankLayersProperty, value); }
        }
        public int BlankLayerLength
        {
            get { return (int)GetValue(BlankLayerLengthProperty); }
            set { SetValue(BlankLayerLengthProperty, value); }
        }

        public event RoutedEventHandler VolumeCreated
        {
            add { AddHandler(VolumeCreatedEvent, value); }
            remove { RemoveHandler(VolumeCreatedEvent, value); }
        }

        public VolumeTab()
        {
            RepeatLayers = false;
            LayerLength = 1;
            AddBlankLayers = false;
            BlankLayerLength = 1;

            InitializeComponent();

            colorPicker.SetColor(Color.FromArgb(255, 255, 255, 255));
        }

        public void Create(object sender, RoutedEventArgs e)
        {
            if (VolumeName == null || VolumeName == "")
            {
                DialogBox db = new DialogBox("No volume name specified.", "Please enter a name for the volume and try again.",
                    "Create", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }
            if (colorPicker.SelectedColor == Color.FromArgb(255, 0, 0, 0))
            {
                DialogBox db = new DialogBox("Black? Really?", "Are you sure you want to color your model black?.",
                    "Create", DialogBoxIcon.Help, true);
                Nullable<bool> result = db.ShowDialog();
                if (result != true) return;
            }

            if (tabControl.SelectedIndex == 0) CreateFromData();
            else CreateFromImages();
        }

        private void CreateFromData()
        {
            List<Data2D> selected = AvailableHost.AvailableTablesSource.GetSelectedTables();

            if (selected.Count == 0)
            {
                DialogBox db = new DialogBox("No tables selected",
                    "Select one or more data tables to create a volume.", "Create", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            if (RepeatLayers && LayerLength <= 0)
            {
                DialogBox.Show("Invalid layer length value.",
                    "Enter a repeating length greater than 0 and try again.", "Volume", DialogBoxIcon.Error);
                return;
            }
            if (AddBlankLayers && LayerLength <= 0)
            {
                DialogBox.Show("Invalid blank layer length value.",
                "Enter a blank layer length value greater tahn 0 and try again.", "Volume", DialogBoxIcon.Error);
                return;
            }

            List<Data2D> tables = new List<Data2D>();
            int numberTables = selected.Count;

            int targetWidth = -1;
            int targetHeight = -1;

            foreach (Data2D d in selected)
            {
                if (targetWidth == -1 && targetHeight == -1)
                {
                    targetWidth = d.Width;
                    targetHeight = d.Height;

                    continue;
                }

                if (targetWidth != d.Width || targetHeight != d.Height)
                {
                    DialogBox.Show("Invalid data dimensions.",
                        "One or more data tables has dimensions that do not match the rest.", "Volume", DialogBoxIcon.Error);
                    return;
                }
            }

            for (int i = 0; i < numberTables; i++)
            {
                Data2D d = selected[i];

                if (!RepeatLayers)
                {
                    tables.Add(d);
                }
                else
                {
                    for (int j = 0; j < LayerLength; j++)
                    {
                        tables.Add(d);
                    }
                }

                if (AddBlankLayers)
                {
                    // Don't add blank layer after final data layer
                    if (i == numberTables - 1) continue;

                    for (int j = 0; j < BlankLayerLength; j++)
                    {
                        tables.Add(new Data2D(targetWidth, targetHeight, 0));
                    }
                }
            }

            Volume v = new Volume(tables.ToArray<Data2D>(), colorPicker.SelectedColor, VolumeName);
            CreatedVolume = v;
            this.RaiseEvent(new RoutedEventArgs(VolumeCreatedEvent, this));
            ClosableTabItem.SendStatusUpdate(this, string.Format("Volume {0} created.", VolumeName));
        }
        private void CreateFromImages()
        {
            List<DisplaySeries> selected = AvailableHost.AvailableImageSeriesSource.GetSelectedImageSeries();
            if (selected.Count == 0)
            {
                DialogBox db = new DialogBox("No image series selected",
                       "Select an image series to create a volume.", "Create", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            if (RepeatLayers && LayerLength <= 0)
            {
                DialogBox.Show("Invalid layer length value.",
                    "Enter a repeating length greater than 0 and try again.", "Volume", DialogBoxIcon.Error);
                return;
            }
            if (AddBlankLayers && LayerLength <= 0)
            {
                DialogBox.Show("Invalid blank layer length value.",
                "Enter a blank layer length value greater tahn 0 and try again.", "Volume", DialogBoxIcon.Error);
                return;
            }

            List<Data2D> convertedTables = new List<Data2D>();
            List<Data2D> finalTables = new List<Data2D>();

            int targetWidth = -1;
            int targetHeight = -1;

            foreach (DisplaySeries series in selected)
            {
                foreach (DisplayImage image in series.Images)
                {
                    Data2D d = ImageHelper.ConvertToData2D(image.Source as BitmapSource);
                    convertedTables.Add(d);

                    if (targetWidth == -1 && targetHeight == -1)
                    {
                        targetWidth = d.Width;
                        targetHeight = d.Height;

                        continue;
                    }

                    if(targetWidth !=d.Width ||targetHeight !=d.Height)
                    {
                        DialogBox.Show("Invalid data dimensions.",
                        "One or more data tables has dimensions that do not match the rest.", "Volume", DialogBoxIcon.Error);
                        return;
                    }
                }
            }

            int numImages = convertedTables.Count;
            for (int i = 0; i < numImages; i++)
            {
                Data2D d = convertedTables[i];

                if (!RepeatLayers)
                {
                    finalTables.Add(d);
                }
                else
                {
                    for (int j = 0; j < LayerLength; j++)
                    {
                        finalTables.Add(d);
                    }
                }

                if (AddBlankLayers)
                {
                    // Don't add blank layer after final data layer
                    if (i == numImages - 1) continue;

                    for (int j = 0; j < BlankLayerLength; j++)
                    {
                        finalTables.Add(new Data2D(targetWidth, targetHeight, 0));
                    }
                }
            }
            
            Volume v = new Volume(finalTables.ToArray<Data2D>(), colorPicker.SelectedColor, VolumeName);
            CreatedVolume = v;
            this.RaiseEvent(new RoutedEventArgs(VolumeCreatedEvent, this));
            ClosableTabItem.SendStatusUpdate(this, string.Format("Volume {0} created.", VolumeName));
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
    }
}
