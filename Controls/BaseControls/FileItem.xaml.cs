using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ImagingSIMS.Controls.BaseControls
{
    /// <summary>
    /// Interaction logic for FileItem.xaml
    /// </summary>
    public partial class FileItem : UserControl
    {
        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register("FilePath",
            typeof(string), typeof(FileItem));
        public static readonly DependencyProperty ShowGlyphProperty = DependencyProperty.Register("ShowGlyph",
            typeof(bool), typeof(FileItem));
        public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register("ShowIcon",
            typeof(bool), typeof(FileItem));

        public static readonly RoutedEvent FileClickedEvent = EventManager.RegisterRoutedEvent("FileClicked",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FileItem));
        public static readonly RoutedEvent RemoveFileClickedEvent = EventManager.RegisterRoutedEvent("RemoveFileClicked",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FileItem));

        public event RoutedEventHandler FileClicked
        {
            add { AddHandler(FileClickedEvent, value); }
            remove { RemoveHandler(FileClickedEvent, value); }
        }
        public event RoutedEventHandler RemoveFileClicked
        {
            add { AddHandler(RemoveFileClickedEvent, value); }
            remove { RemoveHandler(RemoveFileClickedEvent, value); }
        }

        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }
        public bool ShowGlyph
        {
            get { return (bool)GetValue(ShowGlyphProperty); }
            set { SetValue(ShowGlyphProperty, value); }
        }
        public bool ShowIcon
        {
            get { return (bool)GetValue(ShowIconProperty); }
            set { SetValue(ShowIconProperty, value); }
        }

        public FileItem()
        {
            InitializeComponent();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                RoutedEventArgs args = new RoutedEventArgs(FileClickedEvent, this);
                RaiseEvent(args);
            }
        }

        private void menuItemRemove_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            RoutedEventArgs args = new RoutedEventArgs(RemoveFileClickedEvent, this);
            RaiseEvent(args);
        }

        private void menuItemOpenLocation_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo("explorer.exe", $"/select,\"{FilePath}\"");
            p.Start();
        }
    }
}
