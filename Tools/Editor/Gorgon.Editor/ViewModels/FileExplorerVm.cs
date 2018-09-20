#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: September 4, 2018 10:43:51 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Editor.Data;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The file explorer view model.
    /// </summary>
    internal class FileExplorerVm
        : ViewModelBase<FileExplorerParameters>, IFileExplorerVm, IClipboardHandler
    {
        #region Events.
        // Event triggered when the clipboard is updated from the file explorer.
        private event EventHandler _clipboardUpdated;

        /// <summary>
        /// Event triggered when data is stored or cleared on the clipboard.
        /// </summary>
        event EventHandler IClipboardHandler.DataUpdated
        {
            add => _clipboardUpdated += value;
            remove => _clipboardUpdated -= value;
        }
        #endregion

        #region Variables.
        // Illegal file name characters.
        private static readonly HashSet<char> _illegalChars = new HashSet<char>(Path.GetInvalidFileNameChars()
                                                                             .Concat(Path.GetInvalidPathChars())
                                                                             .Distinct());
        // The currently selected file system node.
        private IFileExplorerNodeVm _selectedNode;
        // The message display service.
        private IMessageDisplayService _messageService;
        // The busy state service.
        private IBusyStateService _busyService;
        // The project data
        private IProject _project;
        // The factory used to build view models.
        private ViewModelFactory _factory;
        // The file system service for the project.
        private IFileSystemService _fileSystemService;
        // The manager used to handle project metadata.
        private IMetadataManager _metaDataManager;
        // The clipboard service to use.
        private IClipboardService _clipboard;
        #endregion

        #region Events.
        /// <summary>
        /// Event triggered when the file system is changed.
        /// </summary>
        public event EventHandler FileSystemChanged;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the currently selected node for the file system.
        /// </summary>
        public IFileExplorerNodeVm SelectedNode
        {
            get => _selectedNode;
            private set
            {
                if (_selectedNode == value)
                {
                    return;
                }

                OnPropertyChanging();
                _selectedNode = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the command to execute when including or excluding a node.
        /// </summary>
        public IEditorCommand<bool> IncludeExcludeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when a node is selected.
        /// </summary>
        public IEditorCommand<IFileExplorerNodeVm> SelectNodeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when creating a node.
        /// </summary>
        public IEditorCommand<CreateNodeArgs> CreateNodeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the root node for the file system.
        /// </summary>
        public IFileExplorerNodeVm RootNode
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the command used to rename the selected node.
        /// </summary>
        public IEditorCommand<FileExplorerNodeRenameArgs> RenameNodeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to delete the selected node.
        /// </summary>
        public IEditorCommand<object> DeleteNodeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to refresh the specified node children.
        /// </summary>
        public IEditorCommand<IFileExplorerNodeVm> RefreshNodeCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function called when the file system was changed.
        /// </summary>
        private void OnFileSystemChanged()
        {
            EventHandler handler = FileSystemChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function to determine if a node can be created under the selected node.
        /// </summary>
        /// <param name="args">Arguments for the command.</param>
        /// <returns><b>true</b> if the node can be created, <b>false</b> if not.</returns>
        private bool CanCreateNode(CreateNodeArgs args) => ((!(args?.Cancel ?? false)) && ((SelectedNode == null) || (SelectedNode.AllowChildCreation)));

        /// <summary>
        /// Function to determine if a file system node can be deleted.
        /// </summary>
        /// <returns><b>true</b> if the node can be deleted, <b>false</b> if not.</returns>
        private bool CanDeleteNode() => SelectedNode?.AllowDelete ?? false;

        /// <summary>
        /// Function to delete the selected node.
        /// </summary>
        private async void DoDeleteNode()
        {
            var cancelSource = new CancellationTokenSource();

            try
            {
                bool hasChildren = SelectedNode.Children.Count != 0;
                string message = hasChildren ? string.Format(Resources.GOREDIT_CONFIRM_DELETE_CHILDREN, SelectedNode.FullPath)
                                                : string.Format(Resources.GOREDIT_CONFIRM_DELETE_NO_CHILDREN, SelectedNode.FullPath);

                if (_messageService.ShowConfirmation(message) == MessageResponse.No)
                {
                    return;
                }

                if (!hasChildren)
                {
                    _metaDataManager.DeleteFromIncludeMetadata(SelectedNode.FullPath);
                    await SelectedNode.DeleteNodeAsync(_fileSystemService);
                    return;
                }

                // Function to update the delete progress information and handle metadata update.
                void UpdateDeleteProgress(FileSystemInfo fileSystemItem)
                {
                    string path = fileSystemItem.ToFileSystemPath(_project.ProjectWorkSpace);
                    UpdateMarequeeProgress($"{path}", Resources.GOREDIT_TEXT_DELETING, cancelSource.Cancel);

                    if (_metaDataManager.PathInProject(path))
                    {
                        _metaDataManager.DeleteFromIncludeMetadata(path);
                        OnFileSystemChanged();
                    }

                    // Give our UI time to update.  
                    // We do this here so the user is able to click the Cancel button should they need it.
                    Task.Delay(16).Wait();
                }

                await SelectedNode.DeleteNodeAsync(_fileSystemService, UpdateDeleteProgress, cancelSource.Token);
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_DELETE, SelectedNode.FullPath));
            }
            finally
            {
                HideProgress();
                cancelSource.Dispose();
            }
        }

        /// <summary>
        /// Function to create a node under the selected node.
        /// </summary>
        /// <param name="args">Arguments for the command.</param>
        private void DoCreateNode(CreateNodeArgs args)
        {
            IFileExplorerNodeVm node = SelectedNode ?? RootNode;

            try
            {
                DirectoryInfo newDir = _fileSystemService.CreateDirectory(node.PhysicalPath);

                // Update the metadata 
                var metaData = new IncludedFileSystemPathMetadata(newDir.ToFileSystemPath(_project.ProjectWorkSpace));
                _project.Metadata.IncludedPaths[metaData.Path] = metaData;

                // Create the node for the directory.
                IFileExplorerNodeVm newNode = _factory.CreateFileExplorerDirectoryNodeVm(_project, node, _metaDataManager, newDir);

                node.Children.Add(newNode);
            }
            catch(Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_CANNOT_CREATE_DIR);
                args.Cancel = true;
            }            
        }

        /// <summary>
        /// Function to handle a node selection.
        /// </summary>
        /// <param name="node">The node that was selected.</param>
        private void DoSelectNode(IFileExplorerNodeVm node)
        {
            _busyService.SetBusy();

            try
            {
                SelectedNode = node;
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_NODE_SELECTION, node?.Name ?? string.Empty));
            }
            finally
            {
                _busyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to include or exclude a node from the project.
        /// </summary>
        /// <param name="include"><b>true</b> to include, <b>false</b> to exclude.</param>
        private void DoIncludeExcludeNode(bool include)
        {
            IFileExplorerNodeVm node = SelectedNode ?? RootNode;
            var paths = new List<string>();

            _busyService.SetBusy();
            try
            {
                // Function to set the include/exclude flag recursively.
                void SetIncludeExclude(IReadOnlyList<IFileExplorerNodeVm> children)
                {
                    foreach (IFileExplorerNodeVm child in children)
                    {                       
                        child.Included = include;
                        paths.Add(child.FullPath);

                        if (child.Children.Count > 0)
                        {
                            SetIncludeExclude(child.Children);
                        }
                    }
                }

                paths.Add(SelectedNode.FullPath);
                node.Included = include;

                // If our parent node is not included, and we've included this node, then we'll include it now.
                // If we exclude the node, we'll leave the parent included. This is the behavior in Visual Studio, so we'll mimic that here.
                if ((node.Parent != null) && (!node.Parent.Included) && (include))
                {
                    paths.Add(node.Parent.FullPath);
                    node.Parent.Included = true;
                }

                // Add child nodes.
                SetIncludeExclude(node.Children);

                if (include)
                {
                    _metaDataManager.IncludePaths(paths);
                }
                else
                {
                    _metaDataManager.ExcludePaths(paths);
                }

                OnFileSystemChanged();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_INCLUDE_NODE, node?.Name));
            }
            finally
            {
                _busyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to determine if a node can be renamed or not.
        /// </summary>
        /// <param name="args">The arguments for renaming.</param>
        /// <returns><b>true</b> if the node can be renamed, or <b>false</b> if not.</returns>
        private bool CanRenameNode(FileExplorerNodeRenameArgs args) => (SelectedNode != null)
                    && (!string.IsNullOrWhiteSpace(args.NewName))
                    && (!string.Equals(args.NewName, args.OldName, StringComparison.CurrentCultureIgnoreCase));

        /// <summary>
        /// Function to rename the node and its backing file on the file system.
        /// </summary>
        /// <param name="args">The arguments for renaming.</param>
        private void DoRenameNode(FileExplorerNodeRenameArgs args)
        {
            _busyService.SetBusy();

            try
            {
                // Ensure the name is valid.
                if (args.NewName.Any(item => _illegalChars.Contains(item)))
                {
                    _messageService.ShowError(string.Format(Resources.GOREDIT_ERR_NODE_ILLEGAL_CHARS, string.Join(", ", _illegalChars)));
                    args.Cancel = true;
                    return;
                }

                // Check for other files or subdirectories with the same name.
                IFileExplorerNodeVm sibling = SelectedNode.Parent.Children.FirstOrDefault(item => (string.Equals(item.Name, args.NewName, StringComparison.CurrentCultureIgnoreCase)) && (item != this));

                if (sibling != null)
                {
                    _messageService.ShowError(string.Format(Resources.GOREDIT_ERR_NODE_EXISTS, args.NewName));
                    args.Cancel = true;
                    return;
                }

                string oldPath = SelectedNode.FullPath;

                SelectedNode.RenameNode(_fileSystemService, args.NewName);

                string newPath = SelectedNode.FullPath;

                if (!_metaDataManager.PathInProject(oldPath))
                {
                    return;
                }

                _metaDataManager.RenameIncludedPaths(oldPath, newPath);
                OnFileSystemChanged();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_RENAMING_FILE);
                args.Cancel = true;
            }
            finally
            {
                _busyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to refresh the nodes under the specified node, or for the entire tree.
        /// </summary>
        /// <param name="node">The node to refresh.</param>
        private void DoRefreshSelectedNode(IFileExplorerNodeVm node)
        {
            _busyService.SetBusy();

            try
            {
                IFileExplorerNodeVm nodeToRefresh = node ?? RootNode;

                nodeToRefresh.Children.Clear();

                var parent = new DirectoryInfo(nodeToRefresh == RootNode ? _project.ProjectWorkSpace.FullName : nodeToRefresh.PhysicalPath);

                if (!parent.Exists)
                {
                    return;
                }

                foreach (DirectoryInfo rootDir in _metaDataManager.GetIncludedDirectories(parent.FullName))
                {
                    nodeToRefresh.Children.Add(_factory.CreateFileExplorerDirectoryNodeVm(_project, nodeToRefresh, _metaDataManager, rootDir));
                }

                foreach (FileInfo file in _metaDataManager.GetIncludedFiles(parent.FullName))
                {
                    nodeToRefresh.Children.Add(_factory.CreateFileExplorerFileNodeVm(_project, nodeToRefresh, file));
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_REFRESH);
            }
            finally
            {
                _busyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to inject dependencies for the view model.
        /// </summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.</remarks>
        protected override void OnInitialize(FileExplorerParameters injectionParameters)
        {
            _factory = injectionParameters.ViewModelFactory ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.ViewModelFactory), nameof(injectionParameters));
            _project = injectionParameters.Project ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.Project), nameof(injectionParameters));
            _metaDataManager = injectionParameters.MetadataManager ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.MetadataManager), nameof(injectionParameters));
            _fileSystemService = injectionParameters.FileSystemService ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.FileSystemService), nameof(injectionParameters));
            _messageService = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.MessageDisplay), nameof(injectionParameters));
            _busyService = injectionParameters.BusyState ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.BusyState), nameof(injectionParameters));
            RootNode = injectionParameters.RootNode ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.RootNode), nameof(injectionParameters));
            _clipboard = injectionParameters.ClipboardService ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.ClipboardService), nameof(injectionParameters));
        }

        /// <summary>
        /// Function to return whether or not the item can use the cut functionality for the clipboard.
        /// </summary>
        /// <returns><b>true</b> if the clipboard handler can cut an item, <b>false</b> if not.</returns>
        bool IClipboardHandler.CanCut() => (SelectedNode != null) && (SelectedNode.AllowDelete);

        /// <summary>
        /// Function to return whether or not the item can use the copy functionality for the clipboard.
        /// </summary>
        /// <returns><b>true</b> if the clipboard handler can copy an item, <b>false</b> if not.</returns>
        bool IClipboardHandler.CanCopy() => SelectedNode != null;

        /// <summary>
        /// Function to return whether or not the item can use the paste functionality for the clipboard.
        /// </summary>
        /// <returns><b>true</b> if the clipboard handler can paste an item, <b>false</b> if not.</returns>
        bool IClipboardHandler.CanPaste()
        {
            // TODO: We need a little more sophisticated means of determining if we can paste or not.
            if ((!_clipboard.IsType<FileSystemClipboardData>()) || 
                ((SelectedNode != null) && (!SelectedNode.AllowChildCreation)))
            {
                return false;
            }

            FileSystemClipboardData data = _clipboard.GetData<FileSystemClipboardData>();

            if (string.IsNullOrWhiteSpace(data.FullPath))
            {
                return false;
            }

            // Do not allow us to cut and paste into ourselves.  That way lies madness.
            return ((SelectedNode == null) || ((!string.Equals(SelectedNode.FullPath, data.FullPath, StringComparison.OrdinalIgnoreCase))));
        }

        /// <summary>
        /// Function to store an item to copy onto the clipboard.
        /// </summary>
        void IClipboardHandler.Copy()
        {
            EventHandler handler = _clipboardUpdated;

            try
            {
                _clipboard.CopyItem(new FileSystemClipboardData
                {
                    Copy = true,
                    FullPath = SelectedNode.FullPath
                });

                handler?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_COPYING_FILE_OR_DIR, SelectedNode.Name));
            }
        }

        /// <summary>
        /// Function called when the associated view is unloaded.
        /// </summary>
        public override void OnUnload() => _clipboardUpdated = null;
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="FileExplorerVm"/> class.
        /// </summary>
        public FileExplorerVm()
        {
            SelectNodeCommand = new EditorCommand<IFileExplorerNodeVm>(DoSelectNode);
            CreateNodeCommand = new EditorCommand<CreateNodeArgs>(DoCreateNode, CanCreateNode);
            RenameNodeCommand = new EditorCommand<FileExplorerNodeRenameArgs>(DoRenameNode, CanRenameNode);
            DeleteNodeCommand = new EditorCommand<object>(DoDeleteNode, CanDeleteNode);
            IncludeExcludeCommand = new EditorCommand<bool>(DoIncludeExcludeNode);
            RefreshNodeCommand = new EditorCommand<IFileExplorerNodeVm>(DoRefreshSelectedNode);
        }
        #endregion
    }
}
