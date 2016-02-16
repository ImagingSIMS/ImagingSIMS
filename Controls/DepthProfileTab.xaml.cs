using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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

using ImagingSIMS.Common;
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Data;

using Microsoft.Win32;

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for TableSelectorTab.xaml
    /// </summary>
    public partial class DepthProfileTab : UserControl
    {
        public static readonly DependencyProperty StaggerLayersProperty = DependencyProperty.Register("StaggerLayers",
            typeof(bool), typeof(DepthProfileTab));
        public bool StaggerLayers
        {
            get { return (bool)GetValue(StaggerLayersProperty); }
            set { SetValue(StaggerLayersProperty, value); }
        }

        public ObservableCollection<Data2D> AvailableTables
        {
            get;
            set;
        }
        public DepthProfileTab()
        {
            AvailableTables = new ObservableCollection<Data2D>();

            InitializeComponent();

            Cross1 = new ObservableCollection<Data2D>();
            Cross2 = new ObservableCollection<Data2D>();
            Cross3 = new ObservableCollection<Data2D>();
            Cross4 = new ObservableCollection<Data2D>();
            Cross5 = new ObservableCollection<Data2D>();
            Cross6 = new ObservableCollection<Data2D>();
        }
        public void SetData(ObservableCollection<Data2D> AvailableTables)
        {
            this.AvailableTables = AvailableTables;
            listAvailable.ItemsSource = this.AvailableTables;
        }

        public ObservableCollection<Data2D> Cross1 { get; set; }
        public ObservableCollection<Data2D> Cross2 { get; set; }
        public ObservableCollection<Data2D> Cross3 { get; set; }
        public ObservableCollection<Data2D> Cross4 { get; set; }
        public ObservableCollection<Data2D> Cross5 { get; set; }
        public ObservableCollection<Data2D> Cross6 { get; set; }

        ObservableCollection<Data2D>[] _collections;

        void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }
        void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            _collections = new ObservableCollection<Data2D>[6];
            _collections[0] = Cross1;
            _collections[1] = Cross2;
            _collections[2] = Cross3;
            _collections[3] = Cross4;
            _collections[4] = Cross5;
            _collections[5] = Cross6;

            comboCross.SelectedIndex = 0;

            for (int i = 0; i < 6; i++)
            {
                ListView dest = ((TabItem)tabDest.Items[i]).Content as ListView;
                if (dest == null) return;
                dest.ItemsSource = _collections[i];
            }
        }

        private void comboCross_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int numberCrosses = int.Parse(((ComboBoxItem)comboCross.SelectedItem).Content.ToString());
            if (numberCrosses == -1) return;

            int j = 0;
            for (int i = 0; i < numberCrosses; i++)
            {
                ((TabItem)tabDest.Items[i]).IsEnabled = true;
                j++;
            }
            for (int i = j; i < 6; i++)
            {
                ((TabItem)tabDest.Items[i]).IsEnabled = false;
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            List<KeyValuePair<Data2D, string>> notAdded = new List<KeyValuePair<Data2D, string>>();
            ListView dest = ((TabItem)tabDest.SelectedItem).Content as ListView;
            if (dest == null) return;
            ObservableCollection<Data2D> destCollection = dest.ItemsSource as ObservableCollection<Data2D>;
            if (destCollection == null) return;

            foreach (object obj in listAvailable.SelectedItems)
            {
                Data2D data = obj as Data2D;
                if (data == null) continue;

                try
                {
                    if (destCollection.Contains(data)) throw new ArgumentException("Cross already contains selected data.");
                    destCollection.Add(data);
                }
                catch (ArgumentException ARex)
                {
                    notAdded.Add(new KeyValuePair<Data2D, string>(data, ARex.Message));
                }
                catch (Exception ex)
                {
                    notAdded.Add(new KeyValuePair<Data2D, string>(data, ex.Message));
                }
            }

            if (notAdded.Count > 0)
            {
                string list = "";
                foreach (KeyValuePair<Data2D, string> kvp in notAdded)
                {
                    list += string.Format("{0}: {1}\n", kvp.Key.ToString(), kvp.Value);
                }
                list.Remove(list.Length - 2, 2);

                DialogBox db = new DialogBox("The following tables were not added to the cross:", list, "Sum", DialogBoxIcon.Warning);
                db.ShowDialog();
            }
        }
        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            List<KeyValuePair<Data2D, string>> notRemoved = new List<KeyValuePair<Data2D, string>>();
            ListView dest = ((TabItem)tabDest.SelectedItem).Content as ListView;
            if (dest == null) return;
            ObservableCollection<Data2D> destCollection = dest.ItemsSource as ObservableCollection<Data2D>;
            if (destCollection == null) return;

            Data2D[] toRemove = new Data2D[dest.SelectedItems.Count];
            int i = 0;
            foreach (object obj in dest.SelectedItems)
            {
                toRemove[i] = (Data2D)dest.SelectedItems[i];
                i++;
            }

            for (int j = 0; j < toRemove.Length; j++)
            {
                try
                {
                    if (!destCollection.Contains(toRemove[j])) throw new ArgumentException("The selected data was not found the the cross.");
                    destCollection.Remove(toRemove[j]);
                }
                catch (ArgumentException ARex)
                {
                    notRemoved.Add(new KeyValuePair<Data2D, string>(toRemove[j], ARex.Message));
                }
                catch (Exception ex)
                {
                    notRemoved.Add(new KeyValuePair<Data2D, string>(toRemove[j], ex.Message));
                }
            }

            if (notRemoved.Count > 0)
            {
                string list = "";
                foreach (KeyValuePair<Data2D, string> kvp in notRemoved)
                {
                    list += string.Format("{0}: {1}\n", kvp.Key.ToString(), kvp.Value);
                }
                list.Remove(list.Length - 2, 2);

                DialogBox db = new DialogBox("The following tables were not removed from the cross:", list, "Sum", DialogBoxIcon.Warning);
                db.ShowDialog();
            }
        }

        private async void buttonGenerateDepthProfile_Click(object sender, RoutedEventArgs e)
        {
            int numCrosses = int.Parse(((ComboBoxItem)comboCross.SelectedItem).Content.ToString());

            int numTablesPerLayers = -1;
            for (int i = 0; i < numCrosses; i++)
            {
                int numInLayer = _collections[i].Count;

                if (numInLayer == 0)
                {
                    DialogBox.Show("No tables selected for a cross.",
                        string.Format("Cross number {0} does not have any tables added.", (i + 1).ToString()), "Depth Profile", DialogBoxIcon.Error);
                    return;
                }

                if (numTablesPerLayers == -1)
                {
                    numTablesPerLayers = numInLayer;
                    continue;
                }

                if (numTablesPerLayers != numInLayer)
                {
                    DialogBox.Show("Cross lengths don't match.",
                        "Each cross does not have the same number of tables. Make sure all dimensions match.", "Depth Profile", DialogBoxIcon.Error);
                    return;
                }
            }

            int targetWidth = -1;
            int targetHeight = -1;
            for (int i = 0; i < numCrosses; i++)
            {
                ObservableCollection<Data2D> cross = _collections[i];

                for (int j = 0; j < cross.Count; j++)
                {
                    Data2D d = cross[j];

                    if (targetWidth == -1 && targetHeight == -1)
                    {
                        targetWidth = d.Width;
                        targetHeight = d.Height;

                        continue;
                    }

                    if (targetWidth != d.Width || targetHeight != d.Height)
                    {
                        if (DialogBox.Show("Not all table dimensions are the same.",
                            "This is OK since tables will be summed to generate the depth profile. Click OK to proceed or Cancel to return",
                            "Depth Profile", DialogBoxIcon.Information) != true) return;
                    }
                }   
            }

            Data2D[,] toGenerate = new Data2D[numCrosses, numTablesPerLayers];


            for (int i = 0; i < numCrosses; i++)
            {
                for (int j = 0; j < numTablesPerLayers; j++)
                {
                    toGenerate[i, j] = _collections[i][j];
                }
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Depth Profiles (.txt, .csv)|*.txt;*.csv";
            if (sfd.ShowDialog() != true) return;
            
            await generateDepthProfileAsync(toGenerate, sfd.FileName, StaggerLayers);

            DialogBox.Show("Depth profile saved successfully!", 
                sfd.FileName, "Depth Profile", DialogBoxIcon.Ok);
        }

        private Task generateDepthProfileAsync(Data2D[,] tables, string pathToSave, bool doStagger)
        {
            return Task.Run(() => generateDepthProfile(tables, pathToSave, doStagger));
        }
        private void generateDepthProfile(Data2D[,] tables, string pathToSave, bool doStagger)
        {
            int numCrosses = tables.GetLength(0);
            int numTablesPer = tables.GetLength(1);

            double[,] depthProfile = new double[numCrosses, numTablesPer];

            for (int i = 0; i < numCrosses; i++)
            {
                for (int j = 0; j < numTablesPer; j++)
                {
                    depthProfile[i, j] = tables[i, j].TotalCounts;
                }
            }

            using (StreamWriter sw = new StreamWriter(pathToSave))
            {
                if (!doStagger)
                {
                    for (int i = 0; i < numCrosses; i++)
                    {
                        for (int j = 0; j < numTablesPer; j++)
                        {
                            sw.WriteLine(depthProfile[i, j]);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < numTablesPer; j++)
                    {
                        for (int i = 0; i < numCrosses; i++)
                        {
                            StringBuilder sb = new StringBuilder();
                            for (int a = 0; a < i; a++)
                            {
                                sb.Append("0,");
                            }
                            sb.Append(depthProfile[i, j] + ",");
                            for (int b = i + 1; b < numCrosses; b++)
                            {
                                sb.Append("0,");
                            }
                            sb.Remove(sb.Length - 1, 1);
                            sw.WriteLine(sb.ToString());
                        }
                    }
                }
            }
        }
    }
}
