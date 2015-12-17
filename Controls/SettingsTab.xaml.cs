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
using ImagingSIMS.Common.Registry;

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for SettingsTab.xaml
    /// </summary>
    public partial class SettingsTab : UserControl
    {
        bool _initShowStartup;
        bool _initHideLoad;
        bool _initSaveJ105;
        bool _initClearPlugin;
        DefaultProgram _initDefaultProgram;
        bool _initStartTrace;

        RegSettings _reg;

        public RegSettings RegSettings
        {
            get { return _reg; }
            set
            {
                if (_reg != value)
                {
                    _reg = value;
                }
            }
        }
        public SettingsTab()
        {
            InitializeComponent();

            Loaded += SettingsTab_Loaded;
            SizeChanged += SettingsTab_SizeChanged;
        }
        public SettingsTab(RegSettings Registry)
        {
            RegSettings = Registry;

            InitializeComponent();

            Loaded += SettingsTab_Loaded;
            SizeChanged += SettingsTab_SizeChanged;

            setInitial();
        }

        void SettingsTab_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }
        void SettingsTab_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }
        
        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegSettings.SaveSettings();

                setInitial();

                ClosableTabItem.SendStatusUpdate(this, "Settings saved to registry.");
            }
            catch (Exception ex)
            {
                DialogBox db = new DialogBox("Registry settings could not be saved.", ex.Message,
                    "Settings", DialogBoxIcon.Stop);
                db.ShowDialog();
                return;
            }
        }
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            OnCancel();

            ClosableTabItem cti = ClosableTabItem.GetTabItem(this);
            if (cti != null)
            {
                cti.Close();
            }
        }
        public void OnCancel()
        {
            restoreInitial();
        }
        private void setInitial()
        {
            _initHideLoad = RegSettings.HideLoadDialog;
            _initSaveJ105 = RegSettings.SaveQuickLoad;
            _initShowStartup = RegSettings.ShowStartup;
            _initClearPlugin = RegSettings.ClearPluginData;
            _initDefaultProgram = RegSettings.DefaultProgram;
            _initStartTrace = RegSettings.StartWithTrace;
        }
        private void restoreInitial()
        {
            RegSettings.HideLoadDialog = _initHideLoad;
            RegSettings.SaveQuickLoad = _initSaveJ105;
            RegSettings.ShowStartup = _initShowStartup;
            RegSettings.ClearPluginData = _initClearPlugin;
            RegSettings.DefaultProgram = _initDefaultProgram;
            RegSettings.StartWithTrace = _initStartTrace;
        }

        private void checkBoxStartTrace_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            CheckBox cb = sender as CheckBox;
            if (cb == null) return;

            if (cb.IsChecked == true)
            {
                traceMessage.Visibility = Visibility.Visible;
            }
            else traceMessage.Visibility = Visibility.Hidden;
        }
    }

}
