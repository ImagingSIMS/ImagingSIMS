using ImagingSIMS.Common;
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ImagingSIMS.Controls.ViewModels;

namespace ImagingSIMS.Controls.BaseControls
{
    /// <summary>
    /// Interaction logic for RegImage.xaml
    /// </summary>
    public partial class ControlPointImage : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof(ControlPointImageViewModel), typeof(ControlPointImage));

        public ControlPointImageViewModel ViewModel
        {
            get { return (ControlPointImageViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public IEnumerable<ControlPoint> SelectedPoints
        {
            get
            {
                return ViewModel.ControlPoints.Select(p => new ControlPoint()
                {
                    Id = p.Id,
                    X = p.CoordX,
                    Y = p.CoordY
                });
            }
        }

        public ControlPointImage()
        {
            ViewModel = new ControlPointImageViewModel();

            InitializeComponent();
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var image = sender as Image;
            if (image == null) return;

            ViewModel.VisualWidth = image.ActualWidth;
            ViewModel.VisualHeight = image.ActualHeight;

            ViewModel.RepositionPoints();
            ViewModel.RepoisitonRregionOfInterest();
        }

        private void ControlPointSelection_ControlPointDragging(object sender, ControlPointDragRoutedEventArgs e)
        {
            var selection = e.Source as ControlPointSelection;
            if (selection == null) return;

            var point = selection.ControlPoint;

            // TODO: Drag
            var coord = e.DragArgs.GetPosition(gridImageHost);

            point.VisualX = coord.X - (ControlPointSelection.TargetWidth / 2);
            point.VisualY = coord.Y - (ControlPointSelection.TargetHeight / 2);
            ViewModel.SetMatrixCoordinates(point);
        }
        private void ControlPointSelection_ControlPointRemoved(object sender, RoutedEventArgs e)
        {
            var selection = e.Source as ControlPointSelection;
            if (selection == null) return;

            var point = selection.ControlPoint;

            if (ViewModel.ControlPoints.Contains(point))
            {
                ViewModel.ControlPoints.Remove(point);
            }
        }

        private void ClearSelection_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewModel == null) e.CanExecute = false;

            if (ViewModel.SelectionMode == RegistrationImageSelectionMode.ControlPoints)
                e.CanExecute = ViewModel.ControlPoints.Count > 0;

            else if (ViewModel.SelectionMode == RegistrationImageSelectionMode.ROI)
                e.CanExecute = ViewModel.RegionOfInterest.HasRegionOfInterest;
        }
        private void ClearSelection_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModel.SelectionMode == RegistrationImageSelectionMode.ControlPoints)
                ViewModel.ControlPoints.Clear();

            else if (ViewModel.SelectionMode == RegistrationImageSelectionMode.ROI)
                ViewModel.RegionOfInterest.ClearRegionOfInterest();
        }

        private void ResetSaturation_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.Saturation = ViewModel.InitialSaturation;
        }

        private void regImageControl_DragEnter(object sender, DragEventArgs e)
        {
            ViewModel.IsDropTarget = true;
        }
        private void regImageControl_DragLeave(object sender, DragEventArgs e)
        {
            ViewModel.IsDropTarget = false;
        }
        private void regImageControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Bitmap))
            {
                var bs = e.Data.GetData(DataFormats.Bitmap) as BitmapSource;
                if (bs == null) return;

                ViewModel.ChangeDataSource(ImageGenerator.Instance.ConvertToData2D(bs, Data2DConverionType.Grayscale));

                e.Handled = true;
            }

            else if (e.Data.GetDataPresent("DisplayImage"))
            {
                var image = e.Data.GetData("DisplayImage") as DisplayImage;
                if (image == null) return;

                var bs = image.Source as BitmapSource;
                if (bs == null) return;

                ViewModel.ChangeDataSource(ImageGenerator.Instance.ConvertToData2D(bs, Data2DConverionType.Grayscale));

                e.Handled = true;
            }

            if (e.Data.GetDataPresent("Data2D"))
            {
                var data = e.Data.GetData("Data2D") as Data2D;
                if (data == null) return;

                ViewModel.ChangeDataSource(data);

                e.Handled = true;
            }

            ViewModel.IsDropTarget = false;
        }

        bool _isRoiDragging;
        private void gridImageHost_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var coord = e.GetPosition(gridImageHost);

                if (ViewModel.SelectionMode == RegistrationImageSelectionMode.ControlPoints)
                {

                    ViewModel.AddNewPoint(coord);
                }
                else
                {
                    ViewModel.RegionOfInterest.Reset(coord);
                    ViewModel.SetMatrixCoordinates(ViewModel.RegionOfInterest);
                    _isRoiDragging = true;
                }
            }
        }
        private void gridImageHost_MouseMove(object sender, MouseEventArgs e)
        {
            if (ViewModel.SelectionMode == RegistrationImageSelectionMode.ROI && _isRoiDragging)
            {
                var coord = e.GetPosition(gridImageHost);
                ViewModel.RegionOfInterest.DragTo(coord);
                ViewModel.SetMatrixCoordinates(ViewModel.RegionOfInterest);
            }
        }
        private void gridImageHost_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(ViewModel.SelectionMode == RegistrationImageSelectionMode.ROI)
            {
                _isRoiDragging = false;
            }
        }
        private void gridImageHost_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ViewModel.SelectionMode == RegistrationImageSelectionMode.ROI)
            {
                _isRoiDragging = false;
            }
        }

        private void cmCopyScale_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource source = imageColorScale.Source as BitmapSource;
            if (source == null) return;

            Clipboard.SetImage(source);
        }
        private void cmCopyImage_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource source = ViewModel.DisplayImage;
            if (source == null) return;

            Clipboard.SetImage(source);
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel.DisplayImage != null;
        }
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModel.DisplayImage == null) return;

            var sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap Images (.bmp)|*.bmp";
            if (sfd.ShowDialog() != true) return;

            ViewModel.DisplayImage.Save(sfd.FileName);

            DialogBox.Show("Image saved successfully!", sfd.FileName, "Save", DialogIcon.Ok);
        }
        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel.DisplayImage != null;
        }
        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(ViewModel.DisplayImage != null)
            {
                Clipboard.SetImage(ViewModel.DisplayImage);
            }
        }

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel.UndoHistory.Count > 0;
        }
        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel.RedoHistory.Count > 0;
        }
        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var undo = ViewModel.UndoHistory.Pop();
            if (undo == null) return;

            ViewModel.RedoHistory.Push(ViewModel.DataSource);
            ViewModel.DataSource = undo;
        }
        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var redo = ViewModel.RedoHistory.Pop();
            if (redo == null) return;

            ViewModel.UndoHistory.Push(ViewModel.DataSource);
            ViewModel.DataSource = redo;
        }

        private void AddTable_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel.DataSource != null;
        }
        private void AddTable_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var tableToAdd = new Data2D(ViewModel.DataSource.Width, ViewModel.DataSource.Height)
            {
                DataName = ViewModel.AddTableName
            };
            for (int x = 0; x < ViewModel.DataSource.Width; x++)
            {
                for (int y = 0; y < ViewModel.DataSource.Height; y++)
                {
                    tableToAdd[x, y] = ViewModel.DataSource[x, y];
                }
            }
            AvailableHost.AvailableTablesSource.AddTable(tableToAdd);
        }
    }

    public enum RegistrationImageSelectionMode
    {
        [Description("Control Points")]
        ControlPoints,

        ROI
    }
}
