using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Vespertan.VisualStudio.Extensions.TemplateEditor
{
    public class ReadOnlyPropertyBinding
    {
        public static readonly DependencyProperty ReadOnlyPropertyProperty = DependencyProperty.RegisterAttached(
           "ReadOnlyProperty",
           typeof(object),
           typeof(ReadOnlyPropertyBinding),
           new PropertyMetadata(OnReadOnlyPropertyPropertyChanged));

        public static void SetReadOnlyProperty(DependencyObject element, object value)
        {
            element.SetValue(ReadOnlyPropertyProperty, value);
        }

        public static object GetReadOnlyProperty(DependencyObject element)
        {
            return element.GetValue(ReadOnlyPropertyProperty);
        }

        private static void OnReadOnlyPropertyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            SetModelProperty(obj, e.NewValue);
        }


        public static readonly DependencyProperty ModelPropertyProperty = DependencyProperty.RegisterAttached(
           "ModelProperty",
           typeof(object),
           typeof(ReadOnlyPropertyBinding),
           new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static void SetModelProperty(DependencyObject element, object value)
        {
            element.SetValue(ModelPropertyProperty, value);
        }

        public static object GetModelProperty(DependencyObject element)
        {
            return element.GetValue(ModelPropertyProperty);
        }
    }
}
