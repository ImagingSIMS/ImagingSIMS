using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Controls.ViewModels;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Rendering;

namespace ImagingSIMS.Controls.Tabs
{
    /// <summary>
    /// Interaction logic for ImageStitchTab.xaml
    /// </summary>
    public partial class ImageStitchTab : UserControl
    {
        public static readonly DependencyProperty MaxColumnsProperty = DependencyProperty.Register("MaxColumns",
            typeof(int), typeof(ImageStitchTab), new PropertyMetadata(3, maxColumnsChangedCallback));
        public static readonly DependencyProperty MaxRowsProperty = DependencyProperty.Register("MaxRows",
            typeof(int), typeof(ImageStitchTab), new PropertyMetadata(3, maxRowsChangedCallback));
        public static readonly DependencyProperty OutputNameProperty = DependencyProperty.Register("OutputName",
            typeof(string), typeof(ImageStitchTab));
        public static readonly DependencyProperty WorkspaceProperty = DependencyProperty.Register("Workspace",
            typeof(Workspace), typeof(ImageStitchTab));

        public int MaxColumns
        {
            get { return (int)GetValue(MaxColumnsProperty); }
            set { SetValue(MaxColumnsProperty, value); }
        }
        public int MaxRows
        {
            get { return (int)GetValue(MaxRowsProperty); }
            set { SetValue(MaxRowsProperty, value); }
        }
        public string OutputName
        {
            get { return (string)GetValue(OutputNameProperty); }
            set { SetValue(OutputNameProperty, value); }
        }

        public int CurrentRows
        {
            get
            {
                int maxYIndex = (from item in ItemsToStitch
                                   select item.IndexY).Max();
                return maxYIndex + 1;
            }
        }
        public int CurrentColumns
        {
            get
            {
                int maxXIndex = (from item in ItemsToStitch
                                   select item.IndexX).Max();
                return maxXIndex + 1;
            }
        }

        public ObservableCollection<ImageStitchItemViewModel> ItemsToStitch { get; set; }

        public List<int> AvailableDimensions { get; set; }
        public Workspace Workspace
        {
            get { return (Workspace)GetValue(WorkspaceProperty); }
            set { SetValue(WorkspaceProperty, value); }
        }

        public ImageStitchTab()
        {
            AvailableDimensions = new List<int>()
            {
                1, 2, 3, 4,
                5, 6, 7, 8,
                9, 10, 11, 12
            };

            OutputName = string.Empty;

            ItemsToStitch = new ObservableCollection<ImageStitchItemViewModel>();

            InitializeComponent();

            initializeItems();
        }

        private void initializeItems()
        {
            for (int x = 0; x < MaxRows; x++)
            {
                for (int y = 0; y < MaxColumns; y++)
                {
                    var viewModel = new ImageStitchItemViewModel(x, y);
                    viewModel.OffsetChanged += ViewModel_OffsetChanged;
                    ItemsToStitch.Add(viewModel);
                }
            }
        }

        public static void maxColumnsChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ImageStitchTab ist = sender as ImageStitchTab;
            if (ist != null)
                ist.onMaxColumnsChanged();
        }
        private void onMaxColumnsChanged()
        {
            int currentCols = CurrentColumns;
            int currentRows = CurrentRows;

            int newCols = MaxColumns;

            if (newCols == currentCols)
                return;

            if (newCols > currentCols)
            {
                for (int x = currentCols; x < newCols; x++)
                {
                    for (int y = 0; y < currentRows; y++)
                    {
                        var viewModel = new ImageStitchItemViewModel(x, y);
                        viewModel.OffsetX = ItemsToStitch.Where(item => item.IndexY == y).Select(item => item.OffsetX).Max();
                        viewModel.OffsetChanged += ViewModel_OffsetChanged;
                        ItemsToStitch.Add(viewModel);
                    }
                }
            }
            else if (newCols < currentCols)
            {
                for (int x = newCols; x < currentCols; x++)
                {
                    var toRemove = (from item in ItemsToStitch
                                    where item.IndexX == x
                                    select item).ToArray();
                    for (int i = 0; i < toRemove.Length; i++)
                    {
                        toRemove[i].OffsetChanged -= ViewModel_OffsetChanged;
                        ItemsToStitch.Remove(toRemove[i]);
                    }
                }
            }
        }
        public static void maxRowsChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ImageStitchTab ist = sender as ImageStitchTab;
            if (ist != null)
                ist.onMaxRowsChanged();
        }
        private void onMaxRowsChanged()
        {
            int currentCols = CurrentColumns;
            int currentRows = CurrentRows;

            int newRows = MaxRows;

            if (newRows == currentRows)
                return;

            if (newRows > currentRows)
            {
                for (int y = currentRows; y < newRows; y++)
                {
                    for (int x = 0; x < currentCols; x++)
                    {
                        var viewModel = new ImageStitchItemViewModel(x, y);
                        viewModel.OffsetY = ItemsToStitch.Where(item => item.IndexX == x).Select(item => item.OffsetY).Max();
                        viewModel.OffsetChanged += ViewModel_OffsetChanged;
                        ItemsToStitch.Add(viewModel);
                    }
                }
            }
            else if (newRows < currentRows)
            {
                for (int y = newRows; y < currentRows; y++)
                {
                    var toRemove = (from item in ItemsToStitch
                                    where item.IndexY == y
                                    select item).ToArray();
                    for (int i = 0; i < toRemove.Length; i++)
                    {
                        toRemove[i].OffsetChanged -= ViewModel_OffsetChanged;
                        ItemsToStitch.Remove(toRemove[i]);
                    }
                }
            }
        }

        private void ViewModel_OffsetChanged(object sender, OffsetChangedEventArgs e)
        {
            ImageStitchItemViewModel model = sender as ImageStitchItemViewModel;
            if (model == null) return;

            int col = model.IndexX;
            int row = model.IndexY;

            if (e.IsChangedX)
            {
                var toUpdate = ItemsToStitch.Where(item => item.IndexY == row);
                foreach (var item in toUpdate)
                {
                    item.OffsetX= model.OffsetX;
                }
            }
            else if (e.IsChangedY)
            {
                var toUpdate = ItemsToStitch.Where(item => item.IndexX == col);
                foreach (var item in toUpdate)
                {
                    item.OffsetY = model.OffsetY;
                }
            }
        }

        private void buttonStitch_Click(object sender, RoutedEventArgs e)
        {
            int currentCols = CurrentColumns;
            int currentRows = CurrentRows;

            // Check for and warn of grid locations with no data
            List<Point> missingItems = new List<Point>();

            for (int x = 0; x < currentCols; x++)
            {
                for (int y = 0; y < currentRows; y++)
                {
                    var i = (from item in ItemsToStitch
                                where item.IndexX == x && item.IndexY == y
                                select item).First();

                    if (i.DataItem == null)
                        missingItems.Add(new Point(x, y));
                }
            }

            if(missingItems.Count > 0)
            {
                string message = string.Empty;
                foreach(Point p in missingItems)
                {
                    message += $"{p.X + 1}, {p.Y + 1}\n";
                }
                message = message.Remove(message.Length - 1, 1);
                if (DialogBox.Show(
                    "The following location(s) is(are) missing data. Click OK to proceed(and fill those pixels with blank values) or Cancel to return.",
                    message, "Stitch", DialogIcon.Error, true) == false) return;
            }

            // Validate that dimensions are the same for all tables
            int width = 0;
            int height = 0;
            int depth = 0;

            for (int x = 0; x < currentCols; x++)
            {
                for (int y = 0; y < currentRows; y++)
                {
                    var i = (from item in ItemsToStitch
                             where item.IndexX == x && item.IndexY == y
                             select item).First();

                    if(i.DataItem != null)
                    {
                        if(width == 0 && height == 0 && depth == 0)
                        {
                            width = i.DataItem.Width;
                            height = i.DataItem.Height;
                            depth = i.DataItem.Depth;
                        }

                        if(width != i.DataItem.Width || height != i.DataItem.Height || depth != i.DataItem.Depth)
                        {
                            DialogBox.Show("Invalid dimensions.", 
                                "One or more tables to be stitched does not match the dimensions of the others.", "Stitch", DialogIcon.Error);
                            return;
                        }
                    }
                }
            }

            // Validate output name is set
            if(string.IsNullOrEmpty(OutputName))
            {
                DialogBox.Show("Missing output table name.", 
                    "Please specify a name for the stitched table.", "Stitch", DialogIcon.Error);
                return;
            }

            // Ensure no specified offsets are negative
            if(ItemsToStitch.Where(item=>item.OffsetX < 0 || item.OffsetY<0).Count() > 0)
            {
                DialogBox.Show("One or more offsets are negative.",
                    "Only positive offsets can be used. If necessary, switch the sign of the negative offset(s) and compensate with the offsets of other rows and columns",
                    "Stitch", DialogIcon.Error);
                return;
            }

            int maxOffsetX = ItemsToStitch.Select(item => item.OffsetX).Max();
            int maxOffsetY = ItemsToStitch.Select(item => item.OffsetY).Max();

            int newWidth = width * currentCols + maxOffsetX;
            int newHeight = height * currentRows + maxOffsetY;

            bool isSingleLayer = depth == 1;           

            if (isSingleLayer)
            {
                Data2D stitched = new Data2D(newWidth, newHeight);
                stitched.DataName = OutputName;

                for (int i = 0; i < currentCols; i++)
                {
                    for (int j = 0; j < currentRows; j++)
                    {
                        var data = (from item in ItemsToStitch
                                    where item.IndexX == i && item.IndexY == j
                                    select item).First();

                        int startX = i * width + data.OffsetX;
                        int startY = j * height + data.OffsetY;

                        for (int x = 0; x < width; x++)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                float value = data.DataItem != null ? data.DataItem[x, y, 0] : 0;
                                stitched[startX + x, startY + y] = value;
                            }
                        }
                    }
                }

                Workspace.Data.Add(stitched);
            }

            else
            {
                Data3D stitched = new Data3D(newWidth, newHeight, depth);
                stitched.DataName = OutputName;

                for (int i = 0; i < currentCols; i++)
                {
                    for (int j = 0; j < currentRows; j++)
                    {
                        var data = (from item in ItemsToStitch
                                    where item.IndexX == i && item.IndexY == j
                                    select item).First();

                        int startX = i * width + data.OffsetX;
                        int startY = j * height + data.OffsetY;

                        for (int x = 0; x < width; x++)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                for (int z = 0; z < depth; z++)
                                {
                                    float value = data.DataItem != null ? data.DataItem[x, y, z] : 0;
                                    stitched[startX + x, startY + y, z] = value;
                                }                                
                            }
                        }
                    }
                }

                Volume volume = new Volume(stitched, Color.FromArgb(255, 255, 255, 255));
                Workspace.Volumes.Add(volume);
            }
        }
        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            int currentCols = CurrentColumns;
            int currentRows = CurrentRows;

            for (int x = 0; x < currentCols; x++)
            {
                for (int y = 0; y < currentRows; y++)
                {
                    var i = (from item in ItemsToStitch
                             where item.IndexX == x && item.IndexY == y
                             select item).First();

                    if (i.DataItem != null)
                        i.DataItem = null;
                }
            }
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            Border border = sender as Border;
            if (border == null) return;

            int row = Grid.GetRow(border);
            int column = Grid.GetColumn(border);

            ImageStitchItemViewModel toDrop = (from item in ItemsToStitch
                                               where item.IndexY == row && item.IndexX == column
                                               select item).First();

            if (toDrop == null) return;

            if (e.Data.GetDataPresent("Data2D"))
            {
                Data2D d = e.Data.GetData("Data2D") as Data2D;
                if (d == null) return;

                toDrop.DataItem = new Data3D(new Data2D[] { d });
            }
            else if (e.Data.GetDataPresent("Volume"))
            {
                var volume = e.Data.GetData("Volume") as Volume;
                if (volume == null) return;

                toDrop.DataItem = volume.Data;
            }
        }

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Right)
            {
                Border border = sender as Border;
                if (border == null) return;

                int row = Grid.GetRow(border);
                int column = Grid.GetColumn(border);


                ImageStitchItemViewModel toRemove = (from item in ItemsToStitch
                                                   where item.IndexX == column && item.IndexY == row
                                                   select item).First();

                toRemove.DataItem = null;

                e.Handled = true;
            }
        }
    }
}