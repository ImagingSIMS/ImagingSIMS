using System.Windows;
using System.Windows.Controls;

namespace ImagingSIMS.Controls.BaseControls
{
    public class IndexedStackPanel : StackPanel
    {
        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register("Index",
            typeof(int), typeof(IndexedStackPanel));
        public static readonly DependencyProperty IsDragTargetProperty = DependencyProperty.Register("IsDragTarget",
            typeof(bool), typeof(IndexedStackPanel));
        public static readonly DependencyProperty CanDropProperty = DependencyProperty.Register("CanDrop",
            typeof(bool), typeof(IndexedStackPanel));

        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }
        public bool IsDragTarget
        {
            get { return (bool)GetValue(IsDragTargetProperty); }
            set { SetValue(IsDragTargetProperty, value); }
        }
        public bool CanDrop
        {
            get { return (bool)GetValue(CanDropProperty); }
            set { SetValue(CanDropProperty, value); }
        }

        public IndexedStackPanel()
        {
            DragEnter += IndexedStackPanel_DragEnter;
            DragLeave += IndexedStackPanel_DragLeave;
            Drop += IndexedStackPanel_Drop;
        }

        private void IndexedStackPanel_Drop(object sender, DragEventArgs e)
        {
            if (!CanDrop) return;

            IsDragTarget = false;
        }
        private void IndexedStackPanel_DragLeave(object sender, DragEventArgs e)
        {
            if (!CanDrop) return;

            IsDragTarget = false;
        }
        private void IndexedStackPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (!CanDrop) return;

            IsDragTarget = true;
        }
    }
}
