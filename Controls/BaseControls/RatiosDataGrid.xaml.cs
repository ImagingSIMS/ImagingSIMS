using System;
using System.Collections.Generic;
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
    /// Interaction logic for RatiosDataGrid.xaml
    /// </summary>
    public partial class RatiosDataGrid : UserControl
    {
        public RatiosDataGrid()
        {
            InitializeComponent();
        }

        public void SetValues(string[] labels, float[] values)
        {
            float[,] ratios = new float[values.Length, values.Length];

            for (int x = 0; x < values.Length; x++)
            {
                for (int y = 0; y < values.Length; y++)
                {
                    ratios[x, y] = values[y] / values[x];
                }
            }

            SetValues(labels, ratios);
        }
        public void SetValues(string[] labels, float[,] ratios)
        {
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
            grid.Children.Clear();

            int numberValues = labels.Length;

            for (int i = 0; i < numberValues + 1; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            }

            // Column headers
            for (int y = 0; y < numberValues; y++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = labels[y];
                tb.SetValue(Grid.RowProperty, 0);
                tb.SetValue(Grid.ColumnProperty, y + 1);
                grid.Children.Add(tb);           
            }

            // Row headers
            for (int x = 0; x < numberValues; x++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = labels[x];
                tb.SetValue(Grid.RowProperty, x + 1);
                tb.SetValue(Grid.ColumnProperty, 0);
                grid.Children.Add(tb);
            }

            for (int x = 0; x < numberValues; x++)
            {
                for (int y = 0; y < numberValues; y++)
                {
                    TextBlock tb = new TextBlock();
                    tb.Text = ratios[x, y].ToString("0.0000");
                    tb.SetValue(Grid.RowProperty, x + 1);
                    tb.SetValue(Grid.ColumnProperty, y + 1);
                    grid.Children.Add(tb);
                }
            }
        }

        public void CopyRatios()
        {
            StringBuilder sb = new StringBuilder();

            int numCols = grid.ColumnDefinitions.Count;
            int numRows = grid.RowDefinitions.Count;

            for (int x = 0; x < numRows; x++)
            {
                for (int y = 0; y < numCols; y++)
                {
                    TextBlock tb = grid.Children.Cast<UIElement>().FirstOrDefault(t => Grid.GetRow(t) == x && Grid.GetColumn(t) == y) as TextBlock;
                    sb.Append($"{tb?.Text}\t");
                }
                sb.Append("\n");
            }

            sb.Remove(sb.Length - 1, 1);

            Clipboard.SetText(sb.ToString());
        }
    }
}
