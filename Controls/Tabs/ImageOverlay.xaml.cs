using Accord.IO;
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Controls.ViewModels;
using ImagingSIMS.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace ImagingSIMS.Controls.Tabs
{
    /// <summary>q
    /// Interaction logic for ImageOverlay.xaml
    /// </summary>
    public partial class ImageOverlay : UserControl, INotifyPropertyChanged
    {
        ImageOverlayViewModel _viewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        public static readonly DependencyProperty WorkspaceProperty = DependencyProperty.Register(
            "Workspace", typeof(Workspace), typeof(ImageOverlay));

        public Workspace Workspace
        {
            get { return (Workspace)GetValue(WorkspaceProperty); }
            set { SetValue(WorkspaceProperty, value); }
        }

        public ImageOverlayViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                if (_viewModel != value)
                {
                    _viewModel = value;
                    NotifyPropertyChanged();
                }
            }

        }


        public ImageOverlay(Workspace workspace)
        {
            ViewModel = new ImageOverlayViewModel();
            Workspace = workspace;

            DataContext = ViewModel;

            InitializeComponent();
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void buttonAddImage_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddImage();
        }
        private void buttonRemoveImage_Click(object sender, RoutedEventArgs e)
        {
            var settings = ((Button)sender).DataContext as OverlaySettingsViewModel;
            ViewModel.RemoveImage(settings);
        }
        private void comboBoxImage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var widths = ViewModel.Images.Select(i => i.Image?.Width)
                .Where(w => w.HasValue).Distinct();
            var heights = ViewModel.Images.Select(i => i.Image?.Height)
                .Where(h => h.HasValue).Distinct();

            if (widths.Count() > 1 || heights.Count() > 1)
            {
                DialogBox.Show(
                    "Invalid image shape",
                    "Not all selected images have the same image dimensions. Only images of the same dimensions can be overlaid.",
                    "Error",
                    DialogIcon.Error);
                return;
            }
        }

        private void contentButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap Images (.bmp)|*.bmp|PNG Images (.png)|*.png|Jpeg Images (.jpeg,.jpeg)|*.jpg;*.jpeg|Tiff Images (.tiff,.tif)|*.tif;*.tiff";

            if (sfd.ShowDialog() != true) return;

            BitmapSource source = ViewModel.OverlayImage as BitmapSource;
            if(source == null) return;

            source.Save(sfd.FileName);
        }
        private void contentButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource bs = ViewModel.OverlayImage as BitmapSource;
            if (bs == null) return;

            Clipboard.SetImage(bs);
        }
    }
}
