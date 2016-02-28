using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Windows.Shapes;
using ImagingSIMS.Common.Registry;
using Newtonsoft.Json;

namespace ImagingSIMS.MainApplication
{
    /// <summary>
    /// Interaction logic for ChangeWindow.xaml
    /// </summary>
    public partial class ChangeWindow : Window
    {
        public static readonly DependencyProperty VersionTextProperty = DependencyProperty.Register("VersionText",
            typeof(string), typeof(ChangeWindow));
        public static readonly DependencyProperty ItemsToShowProperty = DependencyProperty.Register("ItemsToShow",
            typeof(VersionItems), typeof(ChangeWindow));
        
        public string VersionText
        {
            get { return (string)GetValue(VersionTextProperty); }
            set { SetValue(VersionTextProperty, value); }
        }

        List<VersionItem> _itemsToShow;

        public VersionItems ItemsToShow
        {
            get { return (VersionItems)GetValue(ItemsToShowProperty); }
            set { SetValue(ItemsToShowProperty, value); }
        }

        public ChangeWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public static void CheckAndShow(Version currentVersion, string changeLogPath)
        {
            if (currentVersion <= SettingsManager.RegSettings.VersionLastReported)
                return;

            string versionInfo = string.Empty;
            using (StreamReader sr = new StreamReader(changeLogPath))
            {
                versionInfo = sr.ReadToEnd();
            }

            List<VersionItem> toShow = new List<VersionItem>();
            if (!string.IsNullOrEmpty(versionInfo))
            {
                VersionItem[] versionItems = JsonConvert.DeserializeObject<VersionItem[]>(versionInfo);                
                foreach(VersionItem vi in versionItems)
                {
                    if(vi.Version >= currentVersion)
                    {
                        toShow.Add(vi);
                    }
                }
            }

            if (toShow.Count == 0) return;

            toShow.Sort();

            ChangeWindow window = new ChangeWindow();
            window.ItemsToShow = new VersionItems(toShow);
            window.VersionText = currentVersion.ToString();
            window.Show();

            SettingsManager.RegSettings.VersionLastReported = currentVersion;
        }
    }

    public class VersionItem : IComparable<VersionItem>
    {
        public Version Version { get; set; }
        public string[] Updates { get; set; }

        public int CompareTo(VersionItem other)
        {
            if (other.Version > Version)
                return -1;

            if (other.Version < Version)
                return 1;

            else return 0;
        }
    }
    public class VersionItems : ObservableCollection<VersionItem>
    {
        public VersionItems(List<VersionItem> items)
            : base()
        {
            foreach (VersionItem item in items)
            {
                Add(item);
            }
        }
    }
}
