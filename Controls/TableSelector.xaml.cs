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

using ImagingSIMS.Data;

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for TableSelector.xaml
    /// </summary>
    public partial class TableSelector : UserControl
    {
        public ObservableCollection<Data2D> AvailableTables;
        public ObservableCollection<Data2D> SelectedTables;

        public TableSelector()
        {
            InitializeComponent();

            AvailableTables = new ObservableCollection<Data2D>();
            SelectedTables = new ObservableCollection<Data2D>();
        }
    }
}
