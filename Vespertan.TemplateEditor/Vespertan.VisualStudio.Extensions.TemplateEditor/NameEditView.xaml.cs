using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Vespertan.VisualStudio.Extensions.TemplateEditor
{
    /// <summary>
    /// Interaction logic for NameEditView.xaml
    /// </summary>
    public partial class NameEditView : UserControl, INotifyPropertyChanged
    {
        public NameEditView()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string _nameValue;

        public string NameValue
        {
            get { return _nameValue; }
            set
            {
                if (_nameValue != value)
                {
                    _nameValue = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NameValue)));
                }
            }
        }

    }
}
