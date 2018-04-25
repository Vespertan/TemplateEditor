using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;

namespace Vespertan.TemplateEditor
{
    public static class TextChangedAction
    {
        #region AddXElementIfNotExists

        public static string GetAddXElementIfNotExists(DependencyObject obj)
        {
            return (string)obj.GetValue(AddXElementIfNotExistsProperty);
        }

        public static void SetAddXElementIfNotExists(DependencyObject obj, string value)
        {
            obj.SetValue(AddXElementIfNotExistsProperty, value);
        }

        public static readonly DependencyProperty AddXElementIfNotExistsProperty =
            DependencyProperty.RegisterAttached("AddXElementIfNotExists", typeof(string), typeof(TextChangedAction), new PropertyMetadata(null, AddXElementIfNotExistsPropertyChanged));

        private static void AddXElementIfNotExistsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CheckBox checkBox)
            {
                checkBox.Click += CheckBox_Checked_Add;
            }
            else if (d is FrameworkElement frameworkElement)
            {
                frameworkElement.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(TextBox_TextChanged_Add));
            }
        }

        private static void CheckBox_Checked_Add(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)e.OriginalSource;
            var parentNode = (XElement)checkBox.DataContext;
            var frameworkElement = (FrameworkElement)e.Source;
            var nodeName = GetAddXElementIfNotExistsRemoveEmpty(frameworkElement);

            if (parentNode.Element(nodeName) == null)
            {
                parentNode.Add(new XElement(nodeName) { Value = checkBox.IsChecked == null ? "" : (checkBox.IsChecked == true ? "true" : "false") });
                BindingOperations.GetBindingExpression(frameworkElement, CheckBox.IsCheckedProperty).UpdateTarget();
            }
        }

        private static void TextBox_TextChanged_Add(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)e.OriginalSource;
            var parentNode = (XElement)textBox.DataContext;
            var frameworkElement = (FrameworkElement)e.Source;
            var nodeName = GetAddXElementIfNotExists(frameworkElement);
            if (parentNode.Element(nodeName) == null)
            {
                parentNode.Add(new XElement(nodeName) { Value = textBox.Text });
                switch (frameworkElement)
                {
                    case ComboBox comboBox:
                        BindingOperations.GetBindingExpression(frameworkElement, ComboBox.TextProperty).UpdateTarget();
                        break;
                    case TextBox textBox1:
                        BindingOperations.GetBindingExpression(frameworkElement, TextBox.TextProperty).UpdateTarget();
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region AddXElementIfNotExistsRemoveEmpty

        public static string GetAddXElementIfNotExistsRemoveEmpty(DependencyObject obj)
        {
            return (string)obj.GetValue(AddXElementIfNotExistsRemoveEmptyProperty);
        }

        public static void SetAddXElementIfNotExistsRemoveEmpty(DependencyObject obj, string value)
        {
            obj.SetValue(AddXElementIfNotExistsRemoveEmptyProperty, value);
        }

        public static readonly DependencyProperty AddXElementIfNotExistsRemoveEmptyProperty =
            DependencyProperty.RegisterAttached("AddXElementIfNotExistsRemoveEmpty", typeof(string), typeof(TextChangedAction), new PropertyMetadata(null, AddXElementIfNotExistsRemoveEmptyPropertyChanged));

        private static void AddXElementIfNotExistsRemoveEmptyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CheckBox checkBox)
            {
                checkBox.Click += CheckBox_Checked_AddRemove;
            }
            else if (d is FrameworkElement frameworkElement)
            {
                frameworkElement.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(TextBox_TextChanged_AddRemove));
            }

        }

        private static void CheckBox_Checked_AddRemove(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)e.OriginalSource;
            var parentNode = (XElement)checkBox.DataContext;
            var frameworkElement = (FrameworkElement)e.Source;
            var nodeName = GetAddXElementIfNotExistsRemoveEmpty(frameworkElement);

            if (parentNode.Element(nodeName) == null)
            {
                parentNode.Add(new XElement(nodeName) { Value = checkBox.IsChecked == null ? "" : (checkBox.IsChecked == true ? "true" : "false") });
                BindingOperations.GetBindingExpression(frameworkElement, CheckBox.IsCheckedProperty).UpdateTarget();
            }
            else if (checkBox.IsChecked == null)
            {
                parentNode.Element(nodeName).Remove();
            }
        }

        private static void TextBox_TextChanged_AddRemove(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)e.OriginalSource;
            var parentNode = (XElement)textBox.DataContext;
            var frameworkElement = (FrameworkElement)e.Source;
            var nodeName = GetAddXElementIfNotExistsRemoveEmpty(frameworkElement);

            if (parentNode.Element(nodeName) == null)
            {
                parentNode.Add(new XElement(nodeName) { Value = textBox.Text });
                switch (frameworkElement)
                {
                    case ComboBox comboBox:
                        BindingOperations.GetBindingExpression(frameworkElement, ComboBox.TextProperty).UpdateTarget();
                        break;
                    case TextBox textBox1:
                        BindingOperations.GetBindingExpression(frameworkElement, TextBox.TextProperty).UpdateTarget();
                        break;
                    default:
                        break;
                }

            }
            else if (textBox.Text.Length == 0)
            {
                parentNode.Element(nodeName).Remove();
            }
        }

        #endregion

        #region AddXAttributeIfNotExists

        public static string GetAddXAttributeIfNotExists(DependencyObject obj)
        {
            return (string)obj.GetValue(AddXAttributeIfNotExistsProperty);
        }

        public static void SetAddXAttributeIfNotExists(DependencyObject obj, string value)
        {
            obj.SetValue(AddXAttributeIfNotExistsProperty, value);
        }

        public static readonly DependencyProperty AddXAttributeIfNotExistsProperty =
            DependencyProperty.RegisterAttached("AddXAttributeIfNotExists", typeof(string), typeof(TextChangedAction), new PropertyMetadata(null, AddXAttributeIfNotExistsPropertyChanged));

        private static void AddXAttributeIfNotExistsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement frameworkElement)
            {
                frameworkElement.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(TextBox_TextChanged_Add_Att));
            }
        }

        private static void TextBox_TextChanged_Add_Att(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)e.OriginalSource;
            var parentNode = (XElement)textBox.DataContext;
            var frameworkElement = (FrameworkElement)e.Source;
            var nodeName = GetAddXAttributeIfNotExists(frameworkElement);
            if (parentNode == null)
            {
                return;
            }
            else if (parentNode.Attribute(nodeName) == null)
            {
                parentNode.Add(new XAttribute(nodeName, textBox.Text));
            }
        }

        #endregion

        #region AddXAttributeIfNotExistsRemoveEmpty

        public static string GetAddXAttributeIfNotExistsRemoveEmpty(DependencyObject obj)
        {
            return (string)obj.GetValue(AddXAttributeIfNotExistsRemoveEmptyProperty);
        }

        public static void SetAddXAttributeIfNotExistsRemoveEmpty(DependencyObject obj, string value)
        {
            obj.SetValue(AddXAttributeIfNotExistsRemoveEmptyProperty, value);
        }

        public static readonly DependencyProperty AddXAttributeIfNotExistsRemoveEmptyProperty =
            DependencyProperty.RegisterAttached("AddXAttributeIfNotExistsRemoveEmpty", typeof(string), typeof(TextChangedAction), new PropertyMetadata(null, AddXAttributeIfNotExistsRemoveEmptyPropertyChanged));

        private static void AddXAttributeIfNotExistsRemoveEmptyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement frameworkElement)
            {
                frameworkElement.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(TextBox_TextChanged_AddRemove_Att));
            }
        }

        private static void TextBox_TextChanged_AddRemove_Att(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)e.OriginalSource;
            var parentNode = (XElement)textBox.DataContext;
            var frameworkElement = (FrameworkElement)e.Source;
            var nodeName = GetAddXAttributeIfNotExistsRemoveEmpty(frameworkElement);
            if (parentNode == null)
            {
                return;
            }
            else if (parentNode.Attribute(nodeName) == null)
            {
                parentNode.Add(new XAttribute(nodeName, textBox.Text));
                textBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
            else if (textBox.Text.Length == 0)
            {
                parentNode.Attribute(nodeName).Remove();
            }
        }

        #endregion

    }
}
