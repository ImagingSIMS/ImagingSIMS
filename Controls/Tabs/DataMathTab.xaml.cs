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
using ImagingSIMS.Common;
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

            if (!GetIsOpValid(leftTermType, rightTermType, ViewModel.Operation))
            {
                e.CanExecute = false;
                return;
            }

            bool leftIsOk = false;
            bool rightIsOk = false;

            // Only need to perform check on data table since scalar can be 0 for maybe some odd calculation
            if (leftTermType == TermType.Data)
            {
                if (leftIndex == -1) leftIsOk = false;
                else
                    leftIsOk = !ViewModel.DataVariables[leftIndex].Equals(Data2D.Empty);
            }
            else leftIsOk = true;

            if (rightTermType == TermType.Data)
            {
                if (rightIndex == -1) rightIsOk = false;
                else
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

            if (leftTermType == TermType.Unknown || rightTermType == TermType.Unknown)
            {
                DialogBox.Show("Could not determine the type of the input variables.",
                    "Check the variable selections.", "Data Math", DialogIcon.Error);
                return;
            }
            if (leftTermType == TermType.None)
            {
                DialogBox.Show("Left term cannot be None type.",
                    "Select either a data or scalar variable for the left term.", "Data Math", DialogIcon.Error);
                return;
            }

            if (leftTermType == TermType.Data)
            {
                var leftData = ViewModel.DataVariables[leftIndex];
                if (rightTermType == TermType.Data)
                {
                    var rightData = ViewModel.DataVariables[rightIndex];
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
                        case MathOperations.Abs:
                            DialogBox.Show("Cannot perform |x| operation.",
                                "A None type must be specified for the right term.", "Data Math", DialogIcon.Error);
                            return;
                        case MathOperations.OneOver:
                            DialogBox.Show("Cannot perform 1/x operation.",
                                "A None type must be specified for the right term.", "Data Math", DialogIcon.Error);
                            return;
                        case MathOperations.Power:
                            DialogBox.Show("Cannot perform x^y operation.",
                                "A scalar type must be specified for the right term.", "Data Math", DialogIcon.Error);
                            return;
                        case MathOperations.Sqrt:
                            DialogBox.Show("Cannot perform \u229Ax operation.",
                                "A None type must be specified for the right term.", "Data Math", DialogIcon.Error);
                            return;
                    }
                }
                else if (rightTermType == TermType.Scalar)
                {
                    var rightScalar = ViewModel.ScalarFactors[rightIndex];
                    switch (op)
                    {
                        case MathOperations.Add:
                            result = leftData + rightScalar;
                            break;
                        case MathOperations.Divide:
                            result = leftData / rightScalar;
                            break;
                        case MathOperations.Multiply:
                            result = leftData * rightScalar;
                            break;
                        case MathOperations.Subtract:
                            result = leftData - rightScalar;
                            break;
                        case MathOperations.Abs:
                            DialogBox.Show("Cannot perform |x| operation.",
                                "A None type must be specified for the right term.", "Data Math", DialogIcon.Error);
                            return;
                        case MathOperations.OneOver:
                            DialogBox.Show("Cannot perform 1/x operation.",
                                "A None type must be specified for the right term.", "Data Math", DialogIcon.Error);
                            return;
                        case MathOperations.Power:
                            result = Data2D.Pow(leftData, (float)rightScalar);
                            break;
                        case MathOperations.Sqrt:
                            DialogBox.Show("Cannot perform \u229Ax operation.",
                                "A None type must be specified for the right term.", "Data Math", DialogIcon.Error);
                            return;
                    }
                }
                else if (rightTermType == TermType.None)
                {
                    switch (op)
                    {
                        case MathOperations.Add:
                            DialogBox.Show("Cannot perform x+y operation.",
                                "A data or scalar type must be specified for the right term.", "Data Math", DialogIcon.Error);
                            return;
                        case MathOperations.Divide:
                            DialogBox.Show("Cannot perform x/y operation.",
                                "A data or scalar type must be specified for the right term.", "Data Math", DialogIcon.Error);
                            return;
                        case MathOperations.Multiply:
                            DialogBox.Show("Cannot perform x*y operation.",
                                "A data or scalar type must be specified for the right term.", "Data Math", DialogIcon.Error);
                            return;
                        case MathOperations.Subtract:
                            DialogBox.Show("Cannot perform x-y operation.",
                                "A data or scalar type must be specified for the right term.", "Data Math", DialogIcon.Error);
                            return;
                        case MathOperations.Abs:
                            result = Data2D.Abs(leftData);
                            break;
                        case MathOperations.OneOver:
                            result = Data2D.OneOver(leftData);
                            break;
                        case MathOperations.Power:
                            DialogBox.Show("Cannot perform x^y operation.",
                                "A data or scalar type must be specified for the right term.", "Data Math", DialogIcon.Error);
                            return;
                        case MathOperations.Sqrt:
                            result = Data2D.Sqrt(leftData);
                            break;
                    }
                }
            }
            else if (leftTermType == TermType.Scalar)
            {
                var leftScalar = ViewModel.ScalarFactors[leftIndex];
                if (rightTermType == TermType.Data)
                {
                    var rightData = ViewModel.DataVariables[rightIndex];
                    switch (op)
                    {
                        case MathOperations.Add:
                            result = leftScalar + rightData;
                            break;
                        case MathOperations.Divide:
                            result = leftScalar / rightData;
                            break;
                        case MathOperations.Multiply:
                            result = leftScalar * rightData;
                            break;
                        case MathOperations.Subtract:
                            result = leftScalar - rightData;
                            break;
                        case MathOperations.Abs:
                            DialogBox.Show("Cannot perform |x| operation.",
                                "A data type must be specified for the left term.", "Data Math", DialogIcon.Error);
                            return;
                        case MathOperations.OneOver:
                            DialogBox.Show("Cannot perform 1/x operation.",
                                "A data type must be specified for the left term.", "Data Math", DialogIcon.Error);
                            return;
                        case MathOperations.Power:
                            DialogBox.Show("Cannot perform x^y operation.",
                                "A data type must be specified for the left term.", "Data Math", DialogIcon.Error);
                            return;
                        case MathOperations.Sqrt:
                            DialogBox.Show("Cannot perform \u229Ax operation.",
                                "A data type must be specified for the left term.", "Data Math", DialogIcon.Error);
                            return;
                    }
                }
                else if (rightTermType == TermType.Scalar)
                {
                    DialogBox.Show("Cannot perform scalar-scalar operation in this context.",
                        "Choose a data table to perform the math operation on.", "Data Math", DialogIcon.Error);
                    return;
                }
                else if (rightTermType == TermType.None)
                {
                    DialogBox.Show("Cannot perform scalar-none operation in this context.",
                        "Choose a data table to perform the math operation on.", "Data Math", DialogIcon.Error);
                }
            }                       

            StringBuilder sb = new StringBuilder();

            if (ViewModel.Operation == MathOperations.Sqrt)
            {
                sb.Append($"\u229A({ViewModel.DataVariables[leftIndex].DataName})");
            }

            else if (ViewModel.Operation == MathOperations.OneOver)
            {
                sb.Append($"1/({ViewModel.DataVariables[leftIndex].DataName})");
            }

            else if(ViewModel.Operation == MathOperations.Abs)
            {
                sb.Append($"|{ViewModel.DataVariables[leftIndex].DataName}|");
            }

            else
            {
                if (leftTermType == TermType.Data)
                {
                    sb.Append($"{ViewModel.DataVariables[leftIndex].DataName} ");
                }
                else if (leftTermType == TermType.Scalar)
                {
                    sb.Append($"{ViewModel.ScalarFactors[leftIndex]} ");
                }

                sb.Append($"{EnumEx.Get(ViewModel.Operation)} ");

                if(rightTermType == TermType.Data)
                {
                    sb.Append($"{ViewModel.DataVariables[rightIndex].DataName} ");
                }
                else if(rightTermType == TermType.Scalar)
                {
                    sb.Append($"{ViewModel.ScalarFactors[rightIndex]} ");
                }
            }

            result.DataName = sb.ToString();

            ViewModel.Result = result;

            sb.Append("\n");
            ViewModel.OperationHistory = ViewModel.OperationHistory + sb.ToString();           
        }
        private bool GetIsOpValid(TermType leftTerm, TermType rightTerm, MathOperations op)
        {
            // These will never pass check
            if (leftTerm == TermType.Unknown || rightTerm == TermType.Unknown) return false;
            if (leftTerm == TermType.None) return false;
            if (leftTerm == TermType.None && rightTerm == TermType.None) return false;

            if (leftTerm == TermType.Data)
            {
                if (rightTerm == TermType.Data)
                {
                    return op == MathOperations.Add || op == MathOperations.Divide ||
                        op == MathOperations.Multiply || op == MathOperations.Subtract;
                }
                else if (rightTerm == TermType.Scalar)
                {
                    return op == MathOperations.Add || op == MathOperations.Divide ||
                        op == MathOperations.Multiply || op == MathOperations.Subtract ||
                        op == MathOperations.Power;
                }
                else if (rightTerm == TermType.None)
                {
                    return op == MathOperations.Abs || op == MathOperations.OneOver ||
                        op == MathOperations.Sqrt;
                }
            }
            else if (leftTerm == TermType.Scalar)
            {
                if (rightTerm == TermType.Data)
                {
                    return op == MathOperations.Add || op == MathOperations.Divide ||
                        op == MathOperations.Multiply || op == MathOperations.Subtract;
                }
                else if (rightTerm == TermType.Scalar) return false;
                else if (rightTerm == TermType.None) return false;
            }

            return false;
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

        private void ResultImage_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                var dataObj = new DataObject("Data2D", ViewModel.Result);
                DragDrop.DoDragDrop(sender as DependencyObject, dataObj, DragDropEffects.Copy);
            }
        }
    }
}
