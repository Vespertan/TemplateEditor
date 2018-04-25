using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Shell;
using System.Xml;
using System.Collections.Specialized;
using System.Reflection;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using System.Threading;
using System.Windows.Threading;
using System.Xml.Linq;

namespace Vespertan.TemplateEditor
{
    public class EditorToolWindowControlViewModel : VesBindableBase
    {
        public EditorToolWindowControlViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        private Dispatcher _dispatcher;
        private FileSystemWatcher _watcher;

        #region Properties

        private string _templatePath;
        public string TemplatePath { get { return _templatePath; } set { SetProperty(ref _templatePath, value); } }

        private string _templateTempDir;
        public string TemplateTempDir { get { return _templateTempDir; } set { SetProperty(ref _templateTempDir, value); } }

        private ObservableCollection<NodeItem> _templateNodeItemList;
        public ObservableCollection<NodeItem> TemplateNodeItemList { get { return _templateNodeItemList; } set { SetProperty(ref _templateNodeItemList, value); } }

        private NodeItem _templateNodeItemCurrent;
        public NodeItem TemplateNodeItemCurrent { get { return _templateNodeItemCurrent; } set { SetProperty(ref _templateNodeItemCurrent, value); } }

        private NodeItem _templateNodeItemRoot;
        public NodeItem TemplateNodeItemRoot { get { return _templateNodeItemRoot; } set { SetProperty(ref _templateNodeItemRoot, value); } }

        private ObservableCollection<NameValue> _inputParametersList;
        public ObservableCollection<NameValue> InputParameterList { get { return _inputParametersList; } set { SetProperty(ref _inputParametersList, value); } }

        private ObservableCollection<NameValue> _customParametersList;
        public ObservableCollection<NameValue> CustomParameterList { get { return _customParametersList; } set { SetProperty(ref _customParametersList, value); } }

        private ObservableCollection<NameValue> _evaluatedParameterList;
        public ObservableCollection<NameValue> EvaluatedParameterList { get { return _evaluatedParameterList; } set { SetProperty(ref _evaluatedParameterList, value); } }

        private ObservableCollection<string> _previewTypeList = new ObservableCollection<string> { "Text", "Image", "VSTemplate" };
        public ObservableCollection<string> PreviewTypeList { get { return _previewTypeList; } set { SetProperty(ref _previewTypeList, value); } }

        private string _previewType;
        public string PreviewType { get { return _previewType; } set { SetProperty(ref _previewType, value); } }

        private EnvDTE.DTE _dte;
        private EnvDTE.DTE DTE => _dte ?? (_dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(Microsoft.VisualStudio.Shell.Interop.SDTE)));

        public string ProjectItemTemplatesLocation => (string)DTE.Properties["Environment", "ProjectsAndSolution"].Item("ProjectItemTemplatesLocation").Value;

        public string ProjectTemplatesLocation => (string)DTE.Properties["Environment", "ProjectsAndSolution"].Item("ProjectTemplatesLocation").Value;

        private XElement _xElementCurrent;
        public XElement XElementCurrent { get { return _xElementCurrent; } set { SetProperty(ref _xElementCurrent, value); } }


        #endregion

        #region Commands+

        #region TemplateOpenCommand

        private VesDelegateCommand _templateOpenCommand;
        public VesDelegateCommand TemplateOpenCommand => _templateOpenCommand ?? (_templateOpenCommand = new VesDelegateCommand(TemplateOpen, CanTemplateOpen));

        private void TemplateOpen()
        {
            TemplateCloseCommand.ExecuteIfCan();
            if (TemplateNodeItemRoot != null)
            {
                return;
            }
            TemplateOpen(ProjectItemTemplatesLocation + @"\KalView.zip");
            return;
            var ofd = new OpenFileDialog();
            ofd.Filter = "zip files|*.zip";
            if (ofd.ShowDialog() == true)
            {
                TemplateOpen(ofd.FileName);
            }
        }

        private void TemplateOpen(string fileName)
        {
            TemplatePath = fileName;
            TemplateTempDir = Directory.CreateDirectory($"{Path.GetTempPath()}\\{Path.GetRandomFileName()}").FullName;
            ZipFile.ExtractToDirectory(TemplatePath, TemplateTempDir);
            var files = Directory.GetFiles(TemplateTempDir, "*", SearchOption.AllDirectories);

            var nodes = new ObservableCollection<NodeItem>();
            nodes.Add(new NodeItem { Name = Path.GetFileName(TemplatePath), FullName = TemplateTempDir, IsDir = true });
            foreach (var f in files)
            {
                var n = new NodeItem
                {
                    Name = Path.GetFileName(f),
                    ParentFullName = Path.GetDirectoryName(f),
                    FullName = f,
                    IsDir = Directory.Exists(f)
                };

                if (!n.IsDir)
                {
                    n.PreviewType = GetPreviewType(n.FullName);
                }
                n.PropertyChanged += NodeItem_PropertyChanged;
                nodes[0].Children.Add(n);
            }

            TemplateNodeItemList = nodes;
            TemplateNodeItemRoot = nodes[0];

            _watcher = new FileSystemWatcher();
            _watcher.Path = TemplateTempDir;
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            _watcher.Changed += Watcher_Changed;
            _watcher.Deleted += Watcher_Changed;
            _watcher.Renamed += Watcher_Changed;
            _watcher.Created += Watcher_Changed;
            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;
            InputParameterListRefreshCommand.ExecuteIfCan();
        }

        private static string GetPreviewType(string path)
        {
            var imageExtensions = new string[]{
                        ".ABC",".ANI",".CAL",".CLP",".CMP",".CMW",".CUR",".DIC",".EMF",".EPS",".EXIF",".FLC",".GIF",".HDP",".ICA",".ICO",".IFF",".IMG",".ITG",".JB2",
                        ".JPEG",".JP2",".JPG",".MAC",".MNG",".MRC",".MSP",".PBM",".PCX",".PNG",".PSD",".RAS",".SGI",".SMP",".TGA",".TIFF",".WMF",".WPG",".XPM",".XPS",".XWD"};
            var extension = Path.GetExtension(path);
            if (extension.Equals(".vstemplate", StringComparison.InvariantCultureIgnoreCase))
            {
                return "VSTemplate";
            }
            else if (imageExtensions.Any(p => p.Equals(extension, StringComparison.InvariantCultureIgnoreCase)))
            {
                return "Image";
            }
            else
            {
                return "Text";
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                var nodeItem = TemplateNodeItemRoot.GetAllChildren().FirstOrDefault(p => p.FullName == e.FullPath);
                TemplatePreviewFileCommand.ExecuteIfCan(nodeItem);
            }
            else if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                var nodeItem = TemplateNodeItemRoot.GetAllChildren().FirstOrDefault(p => p.FullName == e.FullPath);
                _dispatcher.Invoke(() => TemplateNodeItemDeleteCommand.ExecuteIfCan(nodeItem));
            }
            else if (e.ChangeType == WatcherChangeTypes.Created)
            {
                _dispatcher.Invoke(() => TemplateNodeItemAddCommand.ExecuteIfCan(e.FullPath));
            }
            else if (e.ChangeType == WatcherChangeTypes.Renamed)
            {
                var e2 = (RenamedEventArgs)e;
                var nodeItem = TemplateNodeItemRoot.GetAllChildren().FirstOrDefault(p => p.FullName == e2.OldFullPath);
                if (nodeItem != null)
                {
                    TemplateNodeItemRename(nodeItem, e2.Name, false);
                }
            }
        }

        private bool CanTemplateOpen()
        {
            return true;
        }

        #endregion

        #region TemplateSaveCommand

        private VesDelegateCommand _templateSaveCommand;
        public VesDelegateCommand TemplateSaveCommand => _templateSaveCommand ?? (_templateSaveCommand = new VesDelegateCommand(TemplateSave, CanTemplateSave))
            .ObservesProperty(() => TemplateNodeItemRoot.IsModifed);

        private void TemplateSave()
        {
            foreach (var node in TemplateNodeItemRoot.GetAllChildren())
            {
                if (node.IsModifed)
                {
                    TemplatePreviewSaveCommand.ExecuteIfCan(node);
                }
            }

            if (File.Exists(TemplatePath))
            {
                File.Delete(TemplatePath);
            }
            ZipFile.CreateFromDirectory(TemplateTempDir, TemplatePath);
            TemplateNodeItemRoot.IsModifed = false;
        }

        private bool CanTemplateSave()
        {
            return TemplateNodeItemRoot?.IsModifed == true;
        }

        #endregion

        #region TemplateCloseCommand

        private VesDelegateCommand _templateCloseCommand;
        public VesDelegateCommand TemplateCloseCommand => _templateCloseCommand ?? (_templateCloseCommand = new VesDelegateCommand(TemplateClose, CanTemplateClose))
            .ObservesProperty(() => TemplateNodeItemRoot);

        private void TemplateClose()
        {
            if (TemplateNodeItemRoot.IsModifed)
            {
                var result = MessageBox.Show("Save changes?", "Confirm", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    TemplateSaveCommand.ExecuteIfCan();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            _watcher.EnableRaisingEvents = false;
            TemplateNodeItemRoot.GetAllChildren().ForEach(p => p.PropertyChanged -= NodeItem_PropertyChanged);
            TemplateNodeItemRoot.PropertyChanged -= NodeItem_PropertyChanged;
            Directory.Delete(TemplateTempDir, true);
            TemplateTempDir = null;
            TemplatePath = null;
            TemplateNodeItemList = null;
            TemplateNodeItemRoot = null;
        }

        private bool CanTemplateClose()
        {
            return TemplateNodeItemRoot != null;
        }

        #endregion

        #region TemplateInstallCommand

        private VesDelegateCommand _templateInstallCommand;
        public VesDelegateCommand TemplateInstallCommand => _templateInstallCommand ?? (_templateInstallCommand = new VesDelegateCommand(TemplateInstall, CanTemplateInstall))
            .ObservesProperty(() => TemplateNodeItemRoot);

        private void TemplateInstall()
        {
            TemplateSaveCommand.ExecuteIfCan();

            var currentProcess = Process.GetCurrentProcess();

            var proc = new Process();
            proc.StartInfo = new ProcessStartInfo(currentProcess.MainModule.FileName, "/installvstemplates");
            var vsMajorVersion = currentProcess.MainModule.FileVersionInfo.FileMajorPart.ToString();
            proc.Start();
            var executed = proc.WaitForExit(10000);
            if (!executed)
            {
                MessageBox.Show($"Cannot execute {currentProcess.MainModule.FileName} /installvstemplates");
                return;
            }

            var vsAppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Microsoft\VisualStudio";
            var dirs = Directory.GetDirectories(vsAppDataDir, $"{vsMajorVersion}*", SearchOption.TopDirectoryOnly);
            List<string> vsCacheDir = new List<string>();
            foreach (var d in dirs)
            {
                if (Regex.IsMatch(d, vsMajorVersion + @"[.]\d_[a-zA-Z0-9]{8}(Exp)?$"))
                {
                    vsCacheDir.Add(d);
                }
            }

            if (vsCacheDir.Count == 0)
            {
                MessageBox.Show("Template installed.\nCache not found.");
                return;
            }

            foreach (var dir in dirs)
            {
                var vsCacheTemplateDir = $"{dir}\\ItemTemplatesCache\\{TemplateNodeItemRoot.Name}";
                if (!Directory.Exists(vsCacheTemplateDir))
                {
                    MessageBox.Show("Template installed.\nCache not found.");
                    continue;
                }

                try
                {
                    Directory.Delete(vsCacheTemplateDir, true);
                }
                catch
                {
                    MessageBox.Show("Template installed.\nCache not updated (in use). Please restart Visual Studio to changes make effect.");
                    continue;
                }

                Copy(TemplateTempDir, vsCacheTemplateDir);

                MessageBox.Show("Template installed.\nCache updated");
            }
        }

        private bool CanTemplateInstall()
        {
            return TemplateNodeItemRoot != null;
        }

        #endregion

        #region TemplateNodeItemDeleteCommand

        private VesDelegateCommand<NodeItem> _templateNodeItemDeleteCommand;
        public VesDelegateCommand<NodeItem> TemplateNodeItemDeleteCommand => _templateNodeItemDeleteCommand ?? (_templateNodeItemDeleteCommand = new VesDelegateCommand<NodeItem>(TemplateNodeItemDelete, CanTemplateNodeItemDelete))
            .ObservesProperty(() => TemplateNodeItemCurrent);

        private void TemplateNodeItemDelete(NodeItem param)
        {
            if (param.IsDir)
            {
                if (Directory.Exists(param.FullName))
                {
                    _watcher.EnableRaisingEvents = false;
                    Directory.Delete(param.FullName, true);
                    _watcher.EnableRaisingEvents = true;
                }
            }
            else
            {
                if (File.Exists(param.FullName))
                {
                    _watcher.EnableRaisingEvents = false;
                    File.Delete(param.FullName);
                    _watcher.EnableRaisingEvents = true;
                }
            }

            var parent = GetParentNodeItem(param);
            parent.Children.Remove(param);
        }


        private bool CanTemplateNodeItemDelete(NodeItem param)
        {
            return param != null;
        }

        #endregion

        #region TemplateNodeItemAddCommand

        private VesDelegateCommand<string> _templateNodeItemAddCommand;
        public VesDelegateCommand<string> TemplateNodeItemAddCommand => _templateNodeItemAddCommand ?? (_templateNodeItemAddCommand = new VesDelegateCommand<string>(TemplateNodeItemAdd, CanTemplateNodeItemAdd));

        private void TemplateNodeItemAdd(string param)
        {
            var parentDir = Path.GetDirectoryName(param);
            var isDir = Directory.Exists(param);

            if (isDir)
            {
                if (!Directory.Exists(param))
                {
                    _watcher.EnableRaisingEvents = false;
                    Directory.CreateDirectory(param);
                    _watcher.EnableRaisingEvents = true;
                }
            }
            else
            {
                if (!File.Exists(param))
                {
                    _watcher.EnableRaisingEvents = false;
                    File.WriteAllText(param, string.Empty);
                    _watcher.EnableRaisingEvents = true;
                }
            }

            var parentNode = TemplateNodeItemRoot.FullName == parentDir ? TemplateNodeItemRoot : TemplateNodeItemRoot.GetAllChildren().Where(p => p.FullName == parentDir).Single();
            parentNode.Children.Add(new NodeItem
            {
                IsDir = isDir,
                FullName = param,
                Name = Path.GetFileName(param),
                ParentFullName = parentNode.FullName,
                PreviewType = isDir ? string.Empty : GetPreviewType(param)
            });
        }


        private bool CanTemplateNodeItemAdd(string param)
        {
            return param != null;
        }

        #endregion


        #region TemplateNodeItemOpenCommand

        private VesDelegateCommand<NodeItem> _templateNodeItemOpenCommand;
        public VesDelegateCommand<NodeItem> TemplateNodeItemOpenCommand => _templateNodeItemOpenCommand ?? (_templateNodeItemOpenCommand = new VesDelegateCommand<NodeItem>(TemplateNodeItemOpen, CanTemplateNodeItemOpen))
            .ObservesProperty(() => TemplateNodeItemCurrent);

        private void TemplateNodeItemOpen(NodeItem param)
        {
            if (param == TemplateNodeItemRoot)
            {
                Process.Start("explorer.exe", param.FullName);
            }
            else
            {
                DTE.ExecuteCommand("File.OpenFile", param.FullName);
            }
        }

        private bool CanTemplateNodeItemOpen(NodeItem param)
        {
            return param != null;
        }

        #endregion

        #region TemplateNodeItemRenameEndCommand

        private VesDelegateCommand<NodeItem> _templateNodeItemReanmeEndCommand;
        public VesDelegateCommand<NodeItem> TemplateNodeItemRenameEndCommand => _templateNodeItemReanmeEndCommand ?? (_templateNodeItemReanmeEndCommand = new VesDelegateCommand<NodeItem>(TemplateNodeItemRenameEnd, CanTemplateNodeItemRenameEnd));

        private void TemplateNodeItemRenameEnd(NodeItem nodeItem)
        {
            if (nodeItem.IsRenameing)
            {
                if (nodeItem.NewName != nodeItem.Name)
                {
                    TemplateNodeItemRename(nodeItem, nodeItem.NewName, true);
                }
                nodeItem.IsRenameing = false;
                nodeItem.NewName = null;
            }
        }

        private bool CanTemplateNodeItemRenameEnd(NodeItem param)
        {
            return param != null;
        }

        #endregion

        #region TemplateNodeItemRenameBeginCommand

        private VesDelegateCommand<NodeItem> _templateNodeItemRenameBeginCommand;
        public VesDelegateCommand<NodeItem> TemplateNodeItemRenameBeginCommand => _templateNodeItemRenameBeginCommand ?? (_templateNodeItemRenameBeginCommand = new VesDelegateCommand<NodeItem>(TemplateNodeItemRenameBegin, CanTemplateNodeItemRenameBegin))
            .ObservesProperty(() => TemplateNodeItemCurrent);

        private void TemplateNodeItemRenameBegin(NodeItem param)
        {
            if (!param.IsRenameing)
            {
                param.NewName = param.Name;
                param.IsRenameing = true;
            }
        }

        private void TemplateNodeItemRename(NodeItem nodeItem, string newName, bool changeFile)
        {
            var parent = GetParentNodeItem(nodeItem);
            newName = Path.GetFileName(newName);
            if (parent.Children.Any(p => p.Name == newName))
            {
                MessageBox.Show($"File with name '{newName}' already exists");
                return;
            }

            var newFullName = Directory.GetParent(nodeItem.FullName) + "\\" + newName;

            if (changeFile)
            {
                try
                {
                    if (nodeItem.IsDir)
                    {
                        Directory.Move(nodeItem.FullName, newFullName);

                        foreach (var item in nodeItem.GetAllChildren())
                        {
                            item.FullName = item.FullName.Replace(nodeItem.FullName, newFullName);
                        }
                    }
                    else
                    {
                        File.Move(nodeItem.FullName, newFullName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Cannot rename item.\n{ex.Message}");
                    return;
                }
            }

            nodeItem.FullName = newFullName;
            nodeItem.Name = newName;
        }

        private bool CanTemplateNodeItemRenameBegin(NodeItem param)
        {
            return param != null && TemplateNodeItemCurrent != TemplateNodeItemRoot;
        }

        #endregion

        #region TemplateNodeItemRenameCancelCommand

        private VesDelegateCommand<NodeItem> _templateNodeItemRenameCancelCommand;
        public VesDelegateCommand<NodeItem> TemplateNodeItemRenameCancelCommand => _templateNodeItemRenameCancelCommand ?? (_templateNodeItemRenameCancelCommand = new VesDelegateCommand<NodeItem>(TemplateNodeItemRenameCancel, CanTemplateNodeItemRenameCancel));

        private void TemplateNodeItemRenameCancel(NodeItem param)
        {
            param.IsRenameing = false;
            param.NewName = null;
        }

        private bool CanTemplateNodeItemRenameCancel(NodeItem param)
        {
            return param != null;
        }

        #endregion

        #region TemplatePreviewFileCommand

        private VesDelegateCommand<NodeItem> _templatePreviewFileCommand;
        public VesDelegateCommand<NodeItem> TemplatePreviewFileCommand => _templatePreviewFileCommand ?? (_templatePreviewFileCommand = new VesDelegateCommand<NodeItem>(TemplatePreviewFile, CanTemplatePreviewFile));

        private void TemplatePreviewFile(NodeItem param)
        {
            if (!param.IsDir)
            {
                using (var fs = new FileStream(param.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    param.FileContent = sr.ReadToEnd();
                }

                param.IsModifed = false;

            }
        }

        private void NodeItem_PropertyChanged(object s, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var nodeItem = (s as NodeItem);
            if (e.PropertyName == nameof(NodeItem.FileContent))
            {
                nodeItem.IsModifed = true;
                TemplateNodeItemList[0].IsModifed = true;
            }
        }

        private bool CanTemplatePreviewFile(NodeItem param)
        {
            return param != null;
        }

        #endregion

        #region TemplatePreviewSaveCommand

        private VesDelegateCommand<NodeItem> _templatePreviewSaveCommand;
        public VesDelegateCommand<NodeItem> TemplatePreviewSaveCommand => _templatePreviewSaveCommand ?? (_templatePreviewSaveCommand = new VesDelegateCommand<NodeItem>(TemplatePreviewSave, CanTemplatePreviewSave))
            .ObservesProperty(() => TemplateNodeItemCurrent.IsModifed);

        private void TemplatePreviewSave(NodeItem nodeItem)
        {
            if (nodeItem?.FileContent != null)
            {
                File.WriteAllText(nodeItem.FullName, nodeItem.FileContent);
                nodeItem.IsModifed = false;
            }
        }

        private bool CanTemplatePreviewSave(NodeItem nodeItem)
        {
            return nodeItem?.IsModifed == true;
        }

        #endregion

        #region TemplatePreviewDiscardCommand

        private VesDelegateCommand<NodeItem> _templatePreviewDiscardCommand;
        public VesDelegateCommand<NodeItem> TemplatePreviewDiscardCommand => _templatePreviewDiscardCommand ?? (_templatePreviewDiscardCommand = new VesDelegateCommand<NodeItem>(TemplatePreviewDiscard, CanTemplatePreviewDiscard))
             .ObservesProperty(() => TemplateNodeItemCurrent.IsModifed);

        private void TemplatePreviewDiscard(NodeItem nodeItem)
        {
            if (nodeItem?.FileContent != null)
            {
                nodeItem.FileContent = File.ReadAllText(nodeItem.FullName);
                nodeItem.IsModifed = false;
            }
        }

        private bool CanTemplatePreviewDiscard(NodeItem nodeItem)
        {
            return nodeItem?.IsModifed == true;
        }

        #endregion

        #region TemplateCreateCommand

        private VesDelegateCommand _templateCreateCommand;
        public VesDelegateCommand TemplateCreateCommand => _templateCreateCommand ?? (_templateCreateCommand = new VesDelegateCommand(TemplateCreate, CanTemplateCreate));

        private void TemplateCreate()
        {
            if (TemplateNodeItemRoot?.IsModifed == true)
            {
                var result = MessageBox.Show($"Save template '{TemplateNodeItemRoot.Name}' changes?", "Save template", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
                else if (result == MessageBoxResult.Yes)
                {
                    TemplateSaveCommand.ExecuteIfCan();
                }
                TemplateCloseCommand.ExecuteIfCan();
            }

            var sfd = new SaveFileDialog();
            sfd.Filter = "Zip files (*.zip)|*.zip";
            if (sfd.ShowDialog() != true)
            {
                return;
            }


            try
            {
                File.WriteAllBytes(sfd.FileName, Properties.Resources.template);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            TemplateOpen(sfd.FileName);
        }

        private bool CanTemplateCreate()
        {
            return true;
        }

        #endregion

        #region InputParameterListRefreshCommand

        private VesDelegateCommand _inputParameterListCommand;
        public VesDelegateCommand InputParameterListRefreshCommand => _inputParameterListCommand ?? (_inputParameterListCommand = new VesDelegateCommand(InputParameterListRefresh, CanInputParameterListRefresh));

        private void InputParameterListRefresh()
        {
            var lst = new ObservableCollection<NameValue>();
            lst.Add(new NameValue { Name = "$clrversion$" });
            lst.Add(new NameValue { Name = "$itemname$" });
            lst.Add(new NameValue { Name = "$machinename$" });
            lst.Add(new NameValue { Name = "$projectname$" });
            lst.Add(new NameValue { Name = "$registeredorganization$" });
            lst.Add(new NameValue { Name = "$rootnamespace$" });
            lst.Add(new NameValue { Name = "$safeitemname$" });
            lst.Add(new NameValue { Name = "$safeprojectname$" });
            lst.Add(new NameValue { Name = "$time$" });
            lst.Add(new NameValue { Name = "$SpecificSolutionName$" });
            lst.Add(new NameValue { Name = "$userdomain$" });
            lst.Add(new NameValue { Name = "$username$" });
            lst.Add(new NameValue { Name = "$webnamespace$" });
            lst.Add(new NameValue { Name = "$year$" });
            lst.Add(new NameValue { Name = "$runsilent$" });
            lst.Add(new NameValue { Name = "$solutiondirectory$" });
            lst.Add(new NameValue { Name = "$rootname$" });
            lst.Add(new NameValue { Name = "$targetframeworkversion$" });
            lst.Add(new NameValue { Name = "$guid1$" });
            lst.Add(new NameValue { Name = "$guid2$" });
            lst.Add(new NameValue { Name = "$guid3$" });
            lst.Add(new NameValue { Name = "$guid4$" });
            lst.Add(new NameValue { Name = "$guid5$" });
            lst.Add(new NameValue { Name = "$guid6$" });
            lst.Add(new NameValue { Name = "$guid7$" });
            lst.Add(new NameValue { Name = "$guid8$" });
            lst.Add(new NameValue { Name = "$guid9$" });
            lst.Add(new NameValue { Name = "$guid10$" });
            InputParameterList = lst;
        }

        private bool CanInputParameterListRefresh()
        {
            return true;
        }

        #endregion

        #region CustomParameterListRefreshCommand

        private VesDelegateCommand _customParameterListRefreshCommand;
        public VesDelegateCommand CustomParameterListRefreshCommand => _customParameterListRefreshCommand ?? (_customParameterListRefreshCommand = new VesDelegateCommand(CustomParameterListRefresh, CanCustomParameterListRefresh));

        private void CustomParameterListRefresh()
        {
            var item = TemplateNodeItemRoot?.GetAllChildren().FirstOrDefault(p => p.Name.EndsWith(".vstemplate"));
            if (item == null)
            {
                return;
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(item.FileContent);
            var nsm = new XmlNamespaceManager(xmlDocument.NameTable);
            nsm.AddNamespace("x", "http://schemas.microsoft.com/developer/vstemplate/2005");
            var lst = new ObservableCollection<NameValue>();
            foreach (XmlNode xmlNode in xmlDocument.SelectNodes("//x:CustomParameters/x:CustomParameter", nsm))
            {
                var nameValue = new NameValue
                {
                    Name = xmlNode.SelectSingleNode("@Name", nsm).Value,
                    Value = xmlNode.SelectSingleNode("@Value", nsm).Value
                };
                nameValue.PropertyChanged += NameValue_PropertyChanged;
                lst.Add(nameValue);
            }

            CustomParameterList = lst;
            CustomParameterList.CollectionChanged += CustomParameterList_CollectionChanged;
        }

        private void NameValue_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateCustomParameters();
        }

        private void CustomParameterList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                (e.NewItems[0] as NameValue).PropertyChanged += NameValue_PropertyChanged;
                //UpdateCustomParameters();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                UpdateCustomParameters();
            }
        }

        private void UpdateCustomParameters()
        {
            var item = TemplateNodeItemRoot?.GetAllChildren().FirstOrDefault(p => p.Name.EndsWith(".vstemplate"));
            if (item == null)
            {
                return;
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;

            xmlDocument.LoadXml(item.FileContent);
            var nsm = new XmlNamespaceManager(xmlDocument.NameTable);
            nsm.AddNamespace("x", "http://schemas.microsoft.com/developer/vstemplate/2005");
            var lst = new ObservableCollection<NameValue>();


            var cpList = xmlDocument.SelectNodes("//x:CustomParameters", nsm);
            XmlNode cpsNode;
            if (cpList.Count == 1)
            {
                cpsNode = cpList[0];
            }
            else
            {
                cpsNode = xmlDocument.SelectSingleNode("//x:TemplateContent", nsm).AppendChild(xmlDocument.CreateNode(XmlNodeType.Element, "CustomParameters", string.Empty));
            }

            cpsNode.RemoveAll();
            foreach (var cp in CustomParameterList)
            {
                var na = xmlDocument.CreateAttribute("Name");
                na.Value = cp.Name;
                var va = xmlDocument.CreateAttribute("Value");
                va.Value = cp.Value;
                cpsNode.AppendChild(xmlDocument.CreateWhitespace("\n      "));
                var cpNode = cpsNode.AppendChild(xmlDocument.CreateNode(XmlNodeType.Element, "CustomParameter", "http://schemas.microsoft.com/developer/vstemplate/2005"));
                cpNode.Attributes.Append(na);
                cpNode.Attributes.Append(va);
            }
            cpsNode.AppendChild(xmlDocument.CreateWhitespace("\n    "));


            item.FileContent = xmlDocument.InnerXml;
        }

        private bool CanCustomParameterListRefresh()
        {
            return true;
        }

        #endregion

        #region TemplateGeneratePreviewCommand

        private VesDelegateCommand _templateGaneratePreviewCommand;
        public VesDelegateCommand TemplateGeneratePreviewCommand => _templateGaneratePreviewCommand ?? (_templateGaneratePreviewCommand = new VesDelegateCommand(TemplateGeneratePreview, CanTemplateGeneratePreview));

        private void TemplateGeneratePreview()
        {
            if (DTE.SelectedItems.Count == 0)
            {
                return;
            }
            EnvDTE80.Solution2 solution2 = (EnvDTE80.Solution2)DTE.Solution;
            var projectItemTemplate = solution2.GetProjectItemTemplate("KalView", "CSharp");

            try
            {
                if (DTE.SelectedItems.Item(1).ProjectItem != null)
                {
                    DTE.SelectedItems.Item(1).ProjectItem.ProjectItems.AddFromTemplate(projectItemTemplate, "Kiszka.cs");
                }
                else
                {
                    DTE.SelectedItems.Item(1).Project.ProjectItems.AddFromTemplate(projectItemTemplate, "Kiszka.cs");
                }

                var lst = new ObservableCollection<NameValue>();
                var eList = WizardInfoWrapper.EvaluatedReplacementDictionary ?? new Dictionary<string, string>();
                InputParameterListRefreshCommand.ExecuteIfCan();
                foreach (var item in eList)
                {
                    var inputParameter = InputParameterList.FirstOrDefault(p => p.Name == item.Key);
                    if (inputParameter == null)
                    {
                        lst.Add(new NameValue { Name = item.Key, Value = item.Value });
                    }
                    else
                    {
                        inputParameter.Value = item.Value;
                    }
                }
                EvaluatedParameterList = lst;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool CanTemplateGeneratePreview()
        {
            return true;
        }

        #endregion

        #region TestCommand

        private VesDelegateCommand _testCommand;
        public VesDelegateCommand TestCommand => _testCommand ?? (_testCommand = new VesDelegateCommand(Test, CanTest));

        private void Test()
        {

        }

        private bool CanTest()
        {
            return true;
        }

        #endregion

        #region XElementCurrentAddCommand

        private VesDelegateCommand<string> _xElementCurrentAddCommand;
        public VesDelegateCommand<string> XElementCurrentAddCommand => _xElementCurrentAddCommand ?? (_xElementCurrentAddCommand = new VesDelegateCommand<string>(XElementCurrentAdd, CanXElementCurrentAdd))
            .ObservesProperty(() => XElementCurrent);

        private void XElementCurrentAdd(string param)
        {
            if (XElementCurrent == null)
            {
                return;
            }

            XElementCurrent.Add(new XElement(param));
        }

        private bool CanXElementCurrentAdd(string param)
        {
            return true;
        }

        #endregion

        #region XElementCurrentAddUniqueCommand

        private VesDelegateCommand<string> _xElementCurrentAddUniqueCommand;
        public VesDelegateCommand<string> XElementCurrentAddUniqueCommand => _xElementCurrentAddUniqueCommand ?? (_xElementCurrentAddUniqueCommand = new VesDelegateCommand<string>(XElementCurrentAddUnique, CanXElementCurrentAddUnique))
            .ObservesProperty(() => XElementCurrent);

        private void XElementCurrentAddUnique(string param)
        {
            if (XElementCurrent == null)
            {
                return;
            }
            if (XElementCurrent.Element(param) == null)
            {
                XElementCurrent.Add(new XElement(param));

            }
        }

        private bool CanXElementCurrentAddUnique(string param)
        {
            return true;
        }

        #endregion

        #region XElementCurrentRemoveCommand

        private VesDelegateCommand<string> _xElementCurrentRemoveCommand;
        public VesDelegateCommand<string> XElementCurrentRemoveCommand => _xElementCurrentRemoveCommand ?? (_xElementCurrentRemoveCommand = new VesDelegateCommand<string>(XElementCurrentRemove, CanXElementCurrentRemove));

        private void XElementCurrentRemove(string param)
        {
            if (XElementCurrent == null)
            {
                return;
            }

            XElementCurrent.Element(param)?.Remove();
        }

        private bool CanXElementCurrentRemove(string param)
        {
            return true;
        }

        #endregion

        #endregion

        public NodeItem GetParentNodeItem(NodeItem nodeItem)
        {
            var parent = TemplateNodeItemRoot.GetAllChildren().FirstOrDefault(p => p.FullName == nodeItem.ParentFullName);
            if (parent == null)
            {
                if (TemplateNodeItemRoot.FullName == nodeItem.ParentFullName)
                {
                    parent = TemplateNodeItemRoot;
                }
                else
                {
                    return null;
                }
            }
            return parent;
        }

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

    }
}
