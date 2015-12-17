using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ImagingSIMS.Data.Imaging;
using ImagingSIMS.Data.Isosurfacing;
using ImagingSIMS.Data.Rendering;

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for CreateRenderObjectTab.xaml
    /// </summary>
    public partial class RenderObjectTab : UserControl
    {
        Workspace _currentWorkspace;

        public static readonly DependencyProperty PixelSizeProperty = DependencyProperty.Register("PixelSize",
            typeof(float), typeof(RenderObjectTab));
        public static readonly DependencyProperty PixelDepthProperty = DependencyProperty.Register("PixelDepth",
            typeof(float), typeof(RenderObjectTab));
        public static readonly DependencyProperty ZSpacingProperty = DependencyProperty.Register("ZSpacing",
            typeof(float), typeof(RenderObjectTab));
        public static readonly DependencyProperty VolumeThresholdProperty = DependencyProperty.Register("VolumeThreshold",
            typeof(int), typeof(RenderObjectTab));
        public static readonly DependencyProperty IsoThresholdProperty = DependencyProperty.Register("IsoThreshold",
            typeof(int), typeof(RenderObjectTab));
        public static readonly DependencyProperty IsoValueProperty = DependencyProperty.Register("IsoValue",
            typeof(int), typeof(RenderObjectTab));

        public float PixelSize
        {
            get { return (float)GetValue(PixelSizeProperty); }
            set { SetValue(PixelSizeProperty, value); }
        }
        public float PixelDepth
        {
            get { return (float)GetValue(PixelDepthProperty); }
            set { SetValue(PixelDepthProperty, value); }
        }
        public float ZSpacing
        {
            get { return (float)GetValue(ZSpacingProperty); }
            set { SetValue(ZSpacingProperty, value); }
        }
        public int VolumeThreshold
        {
            get { return (int)GetValue(VolumeThresholdProperty); }
            set { SetValue(VolumeThresholdProperty, value); }
        }
        public int IsoThreshold
        {
            get { return (int)GetValue(IsoThresholdProperty); }
            set { SetValue(IsoThresholdProperty, value); }
        }
        public int IsoValue
        {
            get { return (int)GetValue(IsoValueProperty); }
            set { SetValue(IsoValueProperty, value); }
        }

        public RenderObjectTab()
        {
            InitializeComponent();

            CreateDefaults();
        }
        public RenderObjectTab(Workspace Workspace)
        {
            InitializeComponent();

            CreateDefaults();

            _currentWorkspace = Workspace;
        }

        private void CreateDefaults()
        {
            VolumeThreshold = 20;
            PixelSize = 1.0f;
            PixelDepth = 1.0f;
            ZSpacing = 0.0f;
            IsoThreshold = 20;
            IsoValue = 30;
        }

        public void SelectVolumeTab()
        {
            tabControl.SelectedItem = tabVolume;
        }
        public void SelectIsoTab()
        {
            tabControl.SelectedItem = tabIso;
        }

        private void Create(object sender, RoutedEventArgs e)
        {
            //Validation
            if (listComps.SelectedItems.Count == 0)
            {
                DialogBox db = new DialogBox("No components selected.", 
                    "Select one or more components to create a render object.", "Create", DialogBoxIcon.Stop);
                db.ShowDialog();
                return;
            }

            TabItem selected = (TabItem)tabControl.SelectedItem;
            if (selected == tabVolume)
            {
                foreach (object obj in listComps.SelectedItems)
                {
                    ImageComponent c = (ImageComponent)obj;
                    if (c == null) continue;

                    Volume v = new Volume(c.Data, colorPickerVolume.SelectedColor);
                    v.VolumeName = c.ComponentName + " Volume";
                    v.Threshold = VolumeThreshold;
                    v.PixelSize = PixelSize;
                    v.PixelDepth = PixelDepth;
                    v.ZSpacing = ZSpacing;
                    _currentWorkspace.Volumes.Add(v);

                    ClosableTabItem.SendStatusUpdate(this, string.Format("Volume {0} created.", v.VolumeName));
                }
            }
            else if (selected == tabIso)
            {
                foreach (object obj in listComps.SelectedItems)
                {
                    ImageComponent c = (ImageComponent)obj;
                    if (c == null) continue;

                    Isosurface iso = new Isosurface(c.ComponentName + " Isosurface", colorPickerIso.SelectedColor,
                        new Data3D(c.Data), IsoValue, IsoThreshold);
                    //_currentWorkspace.Isosurfaces.Add(iso);
                }
            }
        }
        private void CMEditComponent(object sender, RoutedEventArgs e)
        {
        }
    }
}
