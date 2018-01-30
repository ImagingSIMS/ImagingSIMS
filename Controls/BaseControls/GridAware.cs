﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ImagingSIMS.Controls.BaseControls
{
    public class GridAwareItemsControl : ItemsControl
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            ContentPresenter container = (ContentPresenter)base.GetContainerForItemOverride();
            if (ItemTemplate == null)
            {
                return container;
            }

            FrameworkElement content = (FrameworkElement)ItemTemplate.LoadContent();
            BindingExpression rowBinding = content.GetBindingExpression(Grid.RowProperty);
            BindingExpression columnBinding = content.GetBindingExpression(Grid.ColumnProperty);

            if (rowBinding != null)
            {
                container.SetBinding(Grid.RowProperty, rowBinding.ParentBinding);
            }

            if (columnBinding != null)
            {
                container.SetBinding(Grid.ColumnProperty, columnBinding.ParentBinding);
            }

            return container;
        }
    }
    public class GridAutoLayout
    {
        public static int GetNumberOfColumns(DependencyObject obj)
        {
            return (int)obj.GetValue(NumberOfColumnsProperty);
        }

        public static void SetNumberOfColumns(DependencyObject obj, int value)
        {
            obj.SetValue(NumberOfColumnsProperty, value);
        }

        // Using a DependencyProperty as the backing store for NumberOfColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumberOfColumnsProperty =
            DependencyProperty.RegisterAttached("NumberOfColumns", typeof(int), typeof(GridAutoLayout), new PropertyMetadata(1, NumberOfColumnsUpdated));

        public static int GetNumberOfRows(DependencyObject obj)
        {
            return (int)obj.GetValue(NumberOfRowsProperty);
        }

        public static void SetNumberOfRows(DependencyObject obj, int value)
        {
            obj.SetValue(NumberOfRowsProperty, value);
        }

        // Using a DependencyProperty as the backing store for NumberOfRows.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumberOfRowsProperty =
            DependencyProperty.RegisterAttached("NumberOfRows", typeof(int), typeof(GridAutoLayout), new PropertyMetadata(1, NumberOfRowsUpdated));

        private static void NumberOfRowsUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Grid grid = (Grid)d;

            grid.RowDefinitions.Clear();
            for (int i = 0; i < (int)e.NewValue; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(1, GridUnitType.Star),
                    MinHeight = 25
                });
            }
        }

        private static void NumberOfColumnsUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Grid grid = (Grid)d;

            grid.ColumnDefinitions.Clear();
            for (int i = 0; i < (int)e.NewValue; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(1, GridUnitType.Star),
                    MinWidth = 25
                });
            }
        }
    }
}
