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
using ImagingSIMS.Controls.BaseControls;
using ImagingSIMS.Controls.ViewModels;
using ImagingSIMS.Data;

namespace ImagingSIMS.Controls.Tabs
{
    /// <summary>
    /// Interaction logic for DataMathTab.xaml
    /// </summary>
    public partial class DataMathTab : UserControl, IDroppableTab
    {
        DataMathViewModel _viewModel;

        public DataMathViewModel ViewModel
        {
            get { return _viewModel; }
            set { _viewModel = value; }
        }

        public DataMathTab()
        {
            ViewModel = new DataMathViewModel();
            DataContext = ViewModel;
            
            InitializeComponent();
        }

        private void StackPanelDataVariables_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("Data2D")) return;

            var data = e.Data.GetData("Data2D") as Data2D;
            if (data == null) return;

            int targetIndex = ((IndexedStackPanel)sender).Index;

            ViewModel.DataVariables[targetIndex] = data;
        }
        private void PerformOperation_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            int leftIndex = GetTermIndex(ViewModel.LeftTerm);
            int rightIndex = GetTermIndex(ViewModel.RightTerm);
            TermType leftTermType = GetTermType(ViewModel.LeftTerm);
            TermType rightTermType = GetTermType(ViewModel.RightTerm);

            if (leftTermType == TermType.Unknown || rightTermType == TermType.Unknown ||
                leftIndex == -1 || rightIndex == -1)
            {
                e.CanExecute = false;
                return;
            }

            bool leftIsOk = false;
            bool rightIsOk = false;

            // Only need to perform check on data table since scalar can be 0 for maybe some odd calculation
            if (leftTermType == TermType.Data)
            {
                leftIsOk = !ViewModel.DataVariables[leftIndex].Equals(Data2D.Empty);
            }
            else leftIsOk = true;

            if (rightTermType == TermType.Data)
            {
                rightIsOk = !ViewModel.DataVariables[rightIndex].Equals(Data2D.Empty);
            }
            else rightIsOk = true;

            e.CanExecute = leftIsOk && rightIsOk;
        }
        private void PerformOperation_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int leftIndex = GetTermIndex(ViewModel.LeftTerm);
            int rightIndex = GetTermIndex(ViewModel.RightTerm);
            TermType leftTermType = GetTermType(ViewModel.LeftTerm);
            TermType rightTermType = GetTermType(ViewModel.RightTerm);

            MathOperations op = ViewModel.Operation;

            var result = Data2D.Empty;

            if (leftTermType == TermType.Data && rightTermType == TermType.Data)
            {
                var leftData = ViewModel.DataVariables[leftIndex];
                var rightData = ViewModel.DataVariables[rightIndex];

                if (op == MathOperations.Power)
                {
                    DialogBox.Show("Operation not supported for given input.",
                        "1/x must be used with left data and right scalar inputs.", "Data Math", DialogIcon.Error);
                    return;
                }

                switch (op)
                {
                    case MathOperations.Add:
                        result = leftData + rightData;
                        break;
                    case MathOperations.Divide:
                        result = leftData / rightData;
                        break;
                    case MathOperations.Multiply:
                        result = leftData * rightData;
                        break;
                    case MathOperations.Subtract:
                        result = leftData - rightData;
                        break;
                }
            }
            else if (leftTermType == TermType.Data && rightTermType == TermType.Scalar)
            {
                var leftData = ViewModel.DataVariables[leftIndex];
                var rightScalar = ViewModel.ScalarFactors[rightIndex];

                switch (op)
                {
                    case MathOperations.Add:
                        result = leftData + rightScalar;
                        break;
                    case MathOperations.Divide:
                        result = leftData + rightScalar;
                        break;
                    case MathOperations.Multiply:
                        result = leftData * rightScalar;
                        break;
                    case MathOperations.Power:
                        
                        break;
                    case MathOperations.Subtract:
                        break;
                }
            }
            else if (leftTermType == TermType.Scalar && rightTermType == TermType.Data)
            {
                var leftScalar = ViewModel.ScalarFactors[leftIndex];
                var rightData = ViewModel.DataVariables[rightIndex];

                switch (op)
                {
                    case MathOperations.Add:
                        break;
                    case MathOperations.Divide:
                        break;
                    case MathOperations.Multiply:
                        break;
                    case MathOperations.Power:
                        break;
                    case MathOperations.Subtract:
                        break;
                }
            }
            else
            {
                DialogBox.Show("Cannot perform scalar-scalar operation in this context.", 
                    "Choose a data table to perform the math operation on.", "Data Math", DialogIcon.Error);
                return;
            }
        }
        private void AddResultToWorkspace_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel.Result != null && !ViewModel.Result.Equals(Data2D.Empty);
        }
        private void AddResultToWorkspace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModel.Result == null || ViewModel.Result.Equals(Data2D.Empty)) return;

            AvailableHost.AvailableTablesSource.AddTable(ViewModel.Result);
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

            ViewModel.DataVariables[index] = Data2D.Empty;
        }

        private TermType GetTermType(TermIdentity term)
        {
            var memberInfo = typeof(TermIdentity).GetMember(term.ToString());
            if (memberInfo == null) return TermType.Unknown;

            var attribute = memberInfo[0].GetCustomAttributes(typeof(MathTermTypeAttribute), false);
            if (attribute == null) return TermType.Unknown;

            return ((MathTermTypeAttribute)attribute[0]).TermType;
        }
        private int GetTermIndex(TermIdentity term)
        {
            var memberInfo = typeof(TermIdentity).GetMember(term.ToString());
            if (memberInfo == null) return -1;

            var attribute = memberInfo[0].GetCustomAttributes(typeof(MathTermIndexAttribute), false);
            if (attribute == null) return -1;

            return ((MathTermIndexAttribute)attribute[0]).Index;
        }

        public void HandleDragDrop(object sender, DragEventArgs e)
        {
            // Implement IDroppableTab so tab can receive data tables from MainWindow
            // but don't handle here since the drop target StackPanel will need to handle
            e.Handled = false;
        }

        private void ClearHistory_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !string.IsNullOrEmpty(ViewModel.OperationHistory);
        }
        private void ClearHistory_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.OperationHistory = string.Empty;
        }
    }
}
