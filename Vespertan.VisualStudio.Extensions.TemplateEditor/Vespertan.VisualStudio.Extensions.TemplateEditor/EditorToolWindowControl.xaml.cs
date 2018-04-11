namespace Vespertan.VisualStudio.Extensions.TemplateEditor
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for EditorToolWindowControl.
    /// </summary>
    public partial class EditorToolWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorToolWindowControl"/> class.
        /// </summary>
        public EditorToolWindowControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "EditorToolWindow");
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var nodeItem = ((sender as TextBox).DataContext as NodeItem);
            if (nodeItem.NewName == nodeItem.Name)
            {
                nodeItem.IsRenameing = false;
                nodeItem.NewName = null;
            }
        }
    }
}