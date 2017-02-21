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
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Controls.ViewModels;

namespace ImagingSIMS.Controls.Tabs
{
    /// <summary>
    /// Interaction logic for DataMathTab.xaml
    /// </summary>
    public partial class DataMathTab : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof(DataMathViewModel), typeof(DataMathTab));

        public DataMathViewModel ViewModel
        {
            get { return (DataMathViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        //DataMathViewModel _viewModel;

        //public DataMathViewModel ViewModel
        //{
        //    get { return _viewModel; }
        //    set { _viewModel = value; }
        //}

        public DataMathTab()
        {
            ViewModel = new DataMathViewModel();
            DataContext = ViewModel;
            
            InitializeComponent();
        }

        private void PerformOperation_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

        }
        private void PerformOperation_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
        private void AddResultToWorkspace_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

        }
        private void AddResultToWorkspace_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
        private void AssignVariable_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int index = 0;
            try
            {
                index = int.Parse((string)e.Parameter);
            }
            catch(Exception ex)
            {
                DialogBox.Show("Could not determine the variable to assign to.", ex.Message, "Data Math", DialogIcon.Error);
                return;
            }

            var selectedTable = AvailableHost.AvailableTablesSource.GetSelectedTables().FirstOrDefault();
            if(selectedTable == null)
            {
                DialogBox.Show("No table selected.", "Select a table from the workspace to add.", "Data Math", DialogIcon.Error);
                return;
            }

            ViewModel.DataVariables[index] = selectedTable;
        }
        private void ClearVariable_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int index = 0;
            try
            {
                index = int.Parse((string)e.Parameter);
            }
            catch (Exception ex)
            {
                DialogBox.Show("Could not determine the variable to remove.", ex.Message, "Data Math", DialogIcon.Error);
                return;
            }

            ViewModel.DataVariables[index] = null;
        }
    }
}
