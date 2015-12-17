using System.Windows;
using System.Windows.Controls;

using ImagingSIMS.Data;
using System.IO;

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for StartupTab.xaml
    /// </summary>
    public partial class StartupTab : UserControl
    {
        public static readonly RoutedEvent RecentFileClickedEvent = EventManager.RegisterRoutedEvent("RecentFileClicked",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StartupTab));
        public static readonly RoutedEvent RecentFileRemoveClickedEvent = EventManager.RegisterRoutedEvent("RecentFileRemoveClicked",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(StartupTab));

        public event RoutedEventHandler RecentFileClicked
        {
            add { AddHandler(RecentFileClickedEvent, value); }
            remove { RemoveHandler(RecentFileClickedEvent,value); }
        }
        public event RoutedEventHandler RecentFileRemoveClicked
        {
            add { AddHandler(RecentFileRemoveClickedEvent, value); }
            remove { RemoveHandler(RecentFileRemoveClickedEvent, value); }
        }

        public StartupTab()
        {
            InitializeComponent();
        }
        

        private void FileItem_FileClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            FileItem fi = sender as FileItem;
            RoutedEventArgs args = new RoutedEventArgs(RecentFileClickedEvent, fi);
            RaiseEvent(args);
        }

        private void FileItem_RemoveFileClicked(object sender, RoutedEventArgs e)
        {
            FileItem fi = sender as FileItem;
            RoutedEventArgs args = new RoutedEventArgs(RecentFileRemoveClickedEvent, fi);
            RaiseEvent(args);
        }
    }
}
