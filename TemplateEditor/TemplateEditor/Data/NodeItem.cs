using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TemplateEditor
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
        public string FileContent { get { return _fileContent; } set { SetProperty(ref _fileContent, value); } }

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
    }
}
