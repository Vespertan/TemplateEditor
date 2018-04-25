using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Vespertan.TemplateEditor
{
    public class BoolToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string backValue)
            {
                if (backValue.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
                else if (backValue.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "true" : "false";
            }
            else
            {
                return null;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
