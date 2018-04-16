using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Vespertan.VisualStudio.Extensions.TemplateEditor
{
    public class XElementToXPathConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is XElement xElement)
            {
                var namespaceManager = new XmlNamespaceManager(new NameTable());
                var rootNode = xElement.Document.FirstNode as XElement;
                foreach (var att in rootNode.Attributes().Where(p=>p.IsNamespaceDeclaration))
                {
                    if (att.Name.LocalName == "xmlns")
                    {
                        namespaceManager.AddNamespace("default", att.Value);
                    }
                    else
                    {
                        namespaceManager.AddNamespace(att.Name.LocalName, att.Value);
                    }
                }
                
                return xElement.XPathSelectElements(parameter as string, namespaceManager);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
