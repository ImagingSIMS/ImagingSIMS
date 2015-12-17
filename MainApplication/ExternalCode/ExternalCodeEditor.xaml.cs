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

using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace ImagingSIMS.MainApplication.ExternalCode
{
    /// <summary>
    /// Interaction logic for ExternalCodeEditor.xaml
    /// </summary>
    public partial class ExternalCodeEditor : UserControl
    {
        public static readonly DependencyProperty InputTextProperty = DependencyProperty.Register("InputText",
            typeof(string), typeof(ExternalCodeEditor));
        public static readonly DependencyProperty ErrorTextProperty = DependencyProperty.Register("ErrorText",
            typeof(string), typeof(ExternalCodeEditor));
        public static readonly DependencyProperty HelpTextProperty = DependencyProperty.Register("HelpText",
            typeof(string), typeof(ExternalCodeEditor));

        public string InputText
        {
            get { return (string)GetValue(InputTextProperty); }
            set { SetValue(InputTextProperty, value); }
        }
        public string ErrorText
        {
            get { return (string)GetValue(ErrorTextProperty); }
            set { SetValue(ErrorTextProperty, value); }
        }
        public string HelpText
        {
            get { return (string)GetValue(HelpTextProperty); }
            set { SetValue(HelpTextProperty, value); }
        }

        public ExternalCodeEditor()
        {
            InputText = String.Empty;
            ErrorText = String.Empty;
            HelpText = String.Empty;

            InputText = "using System;\r\n\r\nnamespace UserDefinedCode\r\n{\r\n\tpublic class Program\r\n\t{\r\n\t\tpublic static void Main()\r\n\t\t{\r\n\t\t\tint i = 0;\r\n\t\t}\r\n\t}\r\n}";

            InitializeComponent();
        }

        public CompilerResults CompileCode()
        {
            return compileCode();
        }
        private CompilerResults compileCode()
        {
            var focusedElement = Keyboard.FocusedElement as FrameworkElement;
            if (focusedElement != null && focusedElement == inputTextBox)
            {
                var expression = focusedElement.GetBindingExpression(TextBox.TextProperty);
                if (expression != null)
                    expression.UpdateSource();
            }
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();

            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, InputText);

            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();
                foreach (CompilerError error in results.Errors)
                {
                    sb.AppendLine(String.Format("Error ({0}) Line {1}: {2}", error.ErrorNumber, error.Line, error.ErrorText));
                }

                ErrorText = sb.ToString();

                tabControl.SelectedIndex = 0;
            }
            else
            {
                ErrorText = String.Empty;
            }

            return results;
        }
    }
}
