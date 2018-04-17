using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Vespertan.VisualStudio.Extensions.TemplateEditor
{
    public class CommitValueCommand : ICommand
    {
        private static CommitValueCommand _instance;
        public static CommitValueCommand Command => _instance ?? (_instance = new CommitValueCommand());

        public event EventHandler CanExecuteChanged = delegate { };

        public bool CanExecute(object parameter)
        {

            return true;
        }

        public void Execute(object parameter)
        {
            if (Keyboard.FocusedElement is TextBox textBox)
            {
                BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty).UpdateSource();
            }
            //for combobox etc.
            else if (Keyboard.FocusedElement is Selector selector)
            {
                BindingOperations.GetBindingExpression(selector, Selector.SelectedValueProperty).UpdateSource();
            }
        }
    }
}
