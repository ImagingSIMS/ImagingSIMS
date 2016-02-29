using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ImagingSIMS.Data;

namespace ImagingSIMS.Controls.BaseControls
{
    public class ListViewDD : ListView
    {
        ObservableCollection<Data2D> _tables;

        public ObservableCollection<Data2D> Tables
        {
            get { return _tables; }
            set { _tables = value; }
        }

        public ListViewDD()
            : base()
        {
            _tables = new ObservableCollection<Data2D>();
            AllowDrop = true;
            Drop += ListViewDD_Drop;
        }

        void ListViewDD_Drop(object sender, DragEventArgs e)
        {
            
        }

        protected override System.Windows.DependencyObject GetContainerForItemOverride()
        {
            return new ListViewItemDD();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
        }
    }
    public class ListViewItemDD : ListViewItem
    {
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DataObject obj = new DataObject(DataFormats.FileDrop, this.Content);
                DragDrop.DoDragDrop(this, obj, DragDropEffects.Copy);
            }
            else
            {
                base.OnMouseMove(e);
            }
        }
    }
}
