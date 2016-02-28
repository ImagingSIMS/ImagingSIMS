using System;
using System.Collections.Generic;
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
    /// Interaction logic for ComponentTab.xaml
    /// </summary>
    public partial class ComponentTab : UserControl
    {
        bool _isUpdate;
        ImageComponent _originalComponent;
        ImageComponent _newComponent;

        public ImageComponent OriginalComponent
        {
            get { return _originalComponent; }
        }
        public ImageComponent NewComponent
        {
            get { return _newComponent; }
        }

        public static readonly RoutedEvent ComponentCreatedEvent = EventManager.RegisterRoutedEvent("ComponentCreated",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ComponentTab));
        public static readonly RoutedEvent ComponentUpdatedEvent = EventManager.RegisterRoutedEvent("ComponentUpdated",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ComponentTab));

        public event RoutedEventHandler ComponentCreated
        {
            add { AddHandler(ComponentCreatedEvent, value); }
            remove { RemoveHandler(ComponentCreatedEvent, value); }
        }
        public event RoutedEventHandler ComponentUpdated
        {
            add { AddHandler(ComponentUpdatedEvent, value); }
            remove { RemoveHandler(ComponentUpdatedEvent, value); }
        }

        public static readonly DependencyProperty ComponentNameProperty = DependencyProperty.Register("ComponentName", typeof(string),
            typeof(ComponentTab));
        public string ComponentName
        {
            get { return (string)GetValue(ComponentNameProperty); }
            set { SetValue(ComponentNameProperty, value); }
        }

        public ComponentTab()
        {
            InitializeComponent();

            ComponentName = "";

            colorPicker.SetColor(Color.FromArgb(255, 0, 0, 0));
            colorPicker.LockAlpha();

            Loaded += ComponentTab_Loaded;
        }

        void ComponentTab_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isUpdate)
            {
                ComponentName = _originalComponent.ComponentName;
                colorPicker.SetColor(_originalComponent.PixelColor);

                Data2D[] selected = _originalComponent.Data;
                AvailableTablesHost.AvailableTablesSource.SelectTables(selected, true);
            }
        }

        public void DoUpdate(ImageComponent ComponentToUpdate)
        {
            _isUpdate = true;
            _originalComponent = ComponentToUpdate;            
        }

        private void buttonAccept_Click(object sender, RoutedEventArgs e)
        {
            if (ComponentName == "")
            {
                DialogBox db = new DialogBox("Invalid component title.", "Please enter a title for this component.",
                       "Component Builder", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }
            Color color = colorPicker.SelectedColor;
            if (color.R == 0 && color.G == 0 && color.B == 0)
            {
                DialogBox db = new DialogBox("Are you sure you want to set the component color to black?",
                    "The default color is black. Click OK confirm this choice, or click Cancel to choose a different color.",
                    "Component Builder", DialogBoxIcon.Help, true);
                Nullable<bool> result = db.ShowDialog();
                if (result == false)
                {
                    return;
                }
            }
            List<Data2D> selected = AvailableTablesHost.AvailableTablesSource.GetSelectedTables();
            if (selected.Count == 0)
            {
                DialogBox db = new DialogBox("No tables are selected.", "Please add at least one data table to the selected tables list.",
                       "Component Builder", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            int width = selected[0].Width;
            int height = selected[0].Height;
            Data3D data = new Data3D();
            foreach (Data2D d in selected)
            {
                if (d == null) continue;
                if (d.Height != height || d.Width != width)
                {
                    DialogBox db = new DialogBox("Invalid data dimensions.", "Not all selected data sets match in pixel dimension.",
                      "Component Builder", DialogBoxIcon.Error);
                    db.ShowDialog();
                    return;
                }
                data.AddLayer(d);
            }

            _newComponent = new ImageComponent(ComponentName, color, data);

            if (_isUpdate)
            {
                try
                {
                    this.RaiseEvent(new RoutedEventArgs(ComponentUpdatedEvent, this));
                    ClosableTabItem.SendStatusUpdate(this, string.Format("Component {0} updated.", ComponentName));
                }
                catch (ArgumentException Aex)
                {
                    DialogBox db = new DialogBox("There was a problem updating the component.", 
                        Aex.Message, "Component", DialogBoxIcon.Error);
                    db.ShowDialog();
                }
            }
            else
            {
                ClosableTabItem.SendStatusUpdate(this, string.Format("Component {0} created.", ComponentName));
                this.RaiseEvent(new RoutedEventArgs(ComponentCreatedEvent, this));                
            }
        }
    }
}
