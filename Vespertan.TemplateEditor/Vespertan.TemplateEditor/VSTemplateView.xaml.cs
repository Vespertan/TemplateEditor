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
using System.Xml.Linq;

namespace Vespertan.TemplateEditor
{
    /// <summary>
    /// Interaction logic for VSTemplateView.xaml
    /// </summary>
    public partial class VSTemplateView : UserControl
    {
        public VSTemplateView()
        {
            InitializeComponent();
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var xElement = (XElement)textBox.DataContext;
            var readerXml = xElement.CreateReader(ReaderOptions.None);
            readerXml.MoveToContent();

            textBox.Text = readerXml.ReadInnerXml().Replace(" xmlns=\"http://schemas.microsoft.com/developer/vstemplate/2005\"", null);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = (TextBox)sender;
                var xml = XElement.Parse("<x xmlns=\"http://schemas.microsoft.com/developer/vstemplate/2005\">" + textBox.Text + "</x>", LoadOptions.PreserveWhitespace);
                var xElement = ((XElement)textBox.DataContext);
                xElement.RemoveNodes();
                xElement.Add(xml.Nodes());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot update value of node WizardData. Value is not valid XML node\n{ex.Message}");
            }
        }
    }
}
