using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateEditor
{
    public class NameValue : VesBindableBase
    {
        private string _name;
        public string Name { get { return _name; } set { SetProperty(ref _name, value); } }

        private string _value;
        public string Value { get { return _value; } set { SetProperty(ref _value, value); } }

    }
}
