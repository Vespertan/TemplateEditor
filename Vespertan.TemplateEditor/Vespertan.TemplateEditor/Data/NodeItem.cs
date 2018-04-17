using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml;
using System.Xml.Linq;

namespace Vespertan.TemplateEditor
{
    public class NodeItem : VesBindableBase
    {
        private string _name;
        public string Name { get { return _name; } set { SetProperty(ref _name, value); } }

        private string _fullName;
        public string FullName { get { return _fullName; } set { SetProperty(ref _fullName, value); } }

        private string _parentFullName;
        public string ParentFullName { get { return _parentFullName; } set { SetProperty(ref _parentFullName, value); } }

        private string _fileContent;
        public string FileContent
        {
            get { return _fileContent ?? (PreviewType == "Text" || PreviewType == "VSTemplate" ? (_fileContent = GetFileContent()) : null); }
            set { SetProperty(ref _fileContent, value); }
        }

        private IEnumerable<XElement> _xDocument;
        public IEnumerable<XElement> XDocument
        {
            get
            {
                if (PreviewType == "VSTemplate")
                {
                    if (_xDocument != null)
                    {
                        _xDocument.First().Document.Changed -= XDocument_Changed;
                    }
                    var xDocument = System.Xml.Linq.XDocument.Parse(FileContent);
                    xDocument.Changed += XDocument_Changed;
                    _xDocument = xDocument.Elements();
                }
                return _xDocument;
            }
            set { SetProperty(ref _xDocument, value); }
        }

        private void XDocument_Changed(object sender, XObjectChangeEventArgs e)
        {
            FileContent = _xDocument.First().Document.ToString(SaveOptions.None);
        }

        private ObservableCollection<NodeItem> _children = new ObservableCollection<NodeItem>();
        public ObservableCollection<NodeItem> Children { get { return _children; } set { SetProperty(ref _children, value); } }

        private bool _isDir;
        public bool IsDir { get { return _isDir; } set { SetProperty(ref _isDir, value); } }

        private bool _isModifed;
        public bool IsModifed { get { return _isModifed; } set { SetProperty(ref _isModifed, value); } }

        private bool _isRenameing;
        public bool IsRenameing { get { return _isRenameing; } set { SetProperty(ref _isRenameing, value); } }

        private string _newName;
        public string NewName { get { return _newName; } set { SetProperty(ref _newName, value); } }

        private string _previewType;
        public string PreviewType { get { return _previewType; } set { SetProperty(ref _previewType, value); } }

        public List<NodeItem> GetAllChildren()
        {
            var lst = new List<NodeItem>();
            var stack = new Stack<NodeItem>();

            stack.Push(this);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                foreach (var ch in node.Children)
                {
                    if (ch.Children.Count > 0)
                    {
                        stack.Push(ch);
                    }
                    lst.Add(ch);
                }
            }

            return lst;
        }

        public string GetFileContent()
        {
            if (!IsDir)
            {
                using (var fs = new FileStream(FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    return sr.ReadToEnd();
                }
            }

            return null;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(PreviewType))
            {
                if (PreviewType == "VSTemplate")
                {
                    OnPropertyChanged(nameof(XDocument));
                }
            }
        }
    }
}
