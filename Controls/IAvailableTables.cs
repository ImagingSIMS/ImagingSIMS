using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImagingSIMS.Data;

namespace ImagingSIMS.Controls
{
    public interface IAvailableTables
    {
        void SetTables(ObservableCollection<Data2D> tables);
    }

    public class AvailableTablesTab : DependencyObject, IAvailableTables
    {
        public static readonly DependencyProperty AvailableTablesProperty = DependencyProperty.Register("AvailableTables",
            typeof(ObservableCollection<Data2D>), typeof(AvailableTablesTab));

        public ObservableCollection<Data2D> AvailableTables
        {
            get { return (ObservableCollection<Data2D>)GetValue(AvailableTablesProperty); }
            set { SetValue(AvailableTablesProperty, value); }
        }

        public void SetTables(ObservableCollection<Data2D> tables)
        {
            AvailableTables = tables;
        }
    }
}
