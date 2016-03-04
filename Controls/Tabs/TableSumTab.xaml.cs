using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using ImagingSIMS.Common.Math;
using ImagingSIMS.Controls.Tabs;
using ImagingSIMS.Data;

namespace ImagingSIMS.Controls.Tabs
{
    /// <summary>
    /// Interaction logic for TableSelectorTab.xaml
    /// </summary>
    public partial class TableSumTab : UserControl
    {
        BackgroundWorker bw;
        ProgressWindow pw;

        public TableSumTab()
        {
            InitializeComponent();

            Cross1 = new ObservableCollection<Data2D>();
            Cross2 = new ObservableCollection<Data2D>();
            Cross3 = new ObservableCollection<Data2D>();
            Cross4 = new ObservableCollection<Data2D>();
            Cross5 = new ObservableCollection<Data2D>();
            Cross6 = new ObservableCollection<Data2D>();
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

            for (int i = 0; i < 6; i++)
            {
                ListView dest = ((TabItem)tabDest.Items[i]).Content as ListView;
                if (dest == null) return;
                dest.ItemsSource = _collections[i];
            }
        }

        private void buttonDoSum_Click(object sender, RoutedEventArgs e)
        {
            if (tbBaseName.Text == null || tbBaseName.Text == "")
            {
                DialogBox db = new DialogBox("Base name missing.", "Enter a base name for the summed tables.", "Sum", DialogIcon.Error);
                db.ShowDialog();
                return;
            }

            bool doVarSum = checkDoVariable.IsChecked == true;
            bool doCrossSum = checkDoCross.IsChecked == true;

            int varLength = 0;
            if (doVarSum)
            {
                if (!int.TryParse(tbSum.Text, out varLength))
                {
                    DialogBox db = new DialogBox("Invalid variable sum length", "Enter an integer value for the variable sum length.", "Sum", DialogIcon.Error);
                    db.ShowDialog();
                    return;
                }
            }

            TableSumDoWorkEventArgs a = new TableSumDoWorkEventArgs();
            a.RemoveTables = checkRemoveOriginal.IsChecked == true;
            a.TableName = tbBaseName.Text;
            a.SumLength = varLength;

            int numCrosses = 0;
            if (comboCross.SelectedIndex >= 0) numCrosses = int.Parse(((ComboBoxItem)comboCross.SelectedItem).Content.ToString());
            if (numCrosses == 0) numCrosses = 1;

            for (int i = 0; i < numCrosses; i++)
            {
                if (_collections[i].Count == 0)
                {
                    DialogBox db = new DialogBox("Invalid cross lenghts.",
                        "One or more crosses does not have enough tables assigned. Make sure each cross has two or more tables.",
                        "Sum", DialogIcon.Error);
                    db.ShowDialog();
                    return;
                }
            }

            int tableCount = Cross1.Count;
            int sizeX = Cross1[0].Width;
            int sizeY = Cross1[0].Height;
            for (int i = 0; i < numCrosses; i++)
            {
                if (_collections[i].Count != tableCount)
                {
                    DialogBox db = new DialogBox("Invalid cross lengths.", "Not all crosses have the same table length. Ensure each have the same number of tables.",
                        "Sum", DialogIcon.Error);
                    db.ShowDialog();
                    return;
                }
                for (int j = 0; j < _collections[i].Count; j++)
                {
                    if (_collections[i][j].Width != sizeX || _collections[i][j].Height != sizeY)
                    {
                        DialogBox db = new DialogBox("Invalid data dimensions.", "Not all selected data are of the same dimesions. Ensure same size data sets are selected.",
                          "Sum", DialogIcon.Error);
                        db.ShowDialog();
                        return;
                    }
                }
            }

            List<Data2D>[] toSum = new List<Data2D>[numCrosses];
            for (int i = 0; i < numCrosses; i++)
            {
                toSum[i] = new List<Data2D>(_collections[i]);
            }

            a.CrossTablesToSum = toSum;

            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.ProgressChanged += bw_ProgressChanged;
            bw.DoWork += bw_DoWork;

            pw = new ProgressWindow("Summing tables. Please wait.", "Sum");
            pw.Show();

            foreach (ObservableCollection<Data2D> c in _collections)
            {
                c.Clear();
            }

            bw.RunWorkerAsync(a);
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            TableSumDoWorkEventArgs a = (TableSumDoWorkEventArgs)e.Argument;

            int numCrosses = a.CrossTablesToSum.Length;
            int sumLength = a.CrossTablesToSum[0].Count;
            int varLength = a.SumLength;

            if (varLength == 0) varLength = sumLength;

            int numSegments = sumLength / varLength;
            int numRemaining = sumLength % varLength;

            int sizeX = a.CrossTablesToSum[0][0].Width;
            int sizeY = a.CrossTablesToSum[0][0].Height;

            List<Data2D> results = new List<Data2D>();

            int totalSteps = numSegments * varLength * numCrosses * sizeX;
            int progress = 0;

            for (int i = 0; i < numSegments; i++)
            {
                Data2D result = new Data2D(sizeX, sizeY);
                result.DataName = a.TableName + "_" + i.ToString();

                for (int j = 0; j < varLength; j++)
                {
                    for (int k = 0; k < numCrosses; k++)
                    {
                        for (int x = 0; x < sizeX; x++)
                        {
                            for (int y = 0; y < sizeY; y++)
                            {
                                result[x, y] += a.CrossTablesToSum[k][(i * varLength) + j][x, y];
                            }
                            progress++;
                            worker.ReportProgress(Percentage.GetPercent(progress, totalSteps));
                        }
                    }
                }
                results.Add(result);
            }

            //BUG HERE!
            if (numRemaining > 0)
            {
                Data2D result = new Data2D(sizeX, sizeY);
                result.DataName = a.TableName + "_" + numSegments.ToString();

                for (int j = 0; j < numRemaining; j++)
                {
                    for (int k = 0; k < numCrosses; k++)
                    {
                        for (int x = 0; x < sizeX; x++)
                        {
                            for (int y = 0; y < sizeY; y++)
                            {
                                result[x, y] += _collections[k][numSegments + j][x, y];
                            }
                            progress++;
                            worker.ReportProgress(Percentage.GetPercent(progress, totalSteps));
                        }
                    }
                }
                results.Add(result);
            }

            TableSumCompletedArgs r = new TableSumCompletedArgs();
            r.RemoveTables = a.RemoveTables;
            r.TablesToRemove = a.CrossTablesToSum;
            r.ReturnTables = results;

            e.Result = r;
        }
        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pw.UpdateProgress(e.ProgressPercentage);
        }
        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TableSumCompletedArgs a = (TableSumCompletedArgs)e.Result;
            AvailableHost.AvailableTablesSource.AddTables(a.ReturnTables);
            if (a.RemoveTables)
            {
                List<Data2D> toRemove = new List<Data2D>();
                for (int i = 0; i < a.TablesToRemove.Length; i++)
                {
                    for (int j = 0; j < a.TablesToRemove[i].Count; j++)
                    {
                        toRemove.Add(a.TablesToRemove[i][j]);
                    }
                }
                AvailableHost.AvailableTablesSource.RemoveTables(toRemove);
            }

            pw.ProgressFinished("Sum complete!");
            ClosableTabItem.SendStatusUpdate(this, "Sum complete.");
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
        private void ComboCrossCheckChanged(object sender, RoutedEventArgs e)
        {
            if (checkDoCross.IsChecked != true)
            {
                comboCross.SelectedIndex = 0;
                tabDest.SelectedIndex = 0;
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            List<KeyValuePair<Data2D, string>> notAdded = new List<KeyValuePair<Data2D, string>>();
            ListView dest = ((TabItem)tabDest.SelectedItem).Content as ListView;
            if (dest == null) return;
            ObservableCollection<Data2D> destCollection = dest.ItemsSource as ObservableCollection<Data2D>;
            if (destCollection == null) return;

            //foreach (object obj in listAvailable.SelectedItems)
            //{
            //    Data2D data = obj as Data2D;
            //    if (data == null) continue;
            foreach(Data2D data in AvailableHost.AvailableTablesSource.GetSelectedTables())
            { 
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

                DialogBox db = new DialogBox("The following tables were not added to the cross:", list, "Sum", DialogIcon.Warning);
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

                DialogBox db = new DialogBox("The following tables were not removed from the cross:", list, "Sum", DialogIcon.Warning);
                db.ShowDialog();
            }
        }
    }

    struct TableSumDoWorkEventArgs
    {
        public string TableName;
        public bool RemoveTables;
        public int SumLength;
        public List<Data2D>[] CrossTablesToSum;
    }
    struct TableSumCompletedArgs
    {
        public bool RemoveTables;
        public List<Data2D>[] TablesToRemove;
        public List<Data2D> ReturnTables;
    }

    enum SumType { Sum, VarSum, CrossSum, CrossVarSum }
}
