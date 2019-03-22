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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Data;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Timing;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The file explorer view model.
    /// </summary>
    internal class FileExplorerVm
        : ViewModelBase<FileExplorerParameters>, IFileExplorerVm, IClipboardHandler, IContentFileManager
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

        /// <summary>
        /// Event triggered when the file system is changed.
        /// </summary>
        public event EventHandler FileSystemChanged;
        #endregion

        #region Variables.
        // A lookup used to find nodes.
        private Dictionary<string, IFileExplorerNodeVm> _nodePathLookup = new Dictionary<string, IFileExplorerNodeVm>(StringComparer.OrdinalIgnoreCase);

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
        // The clipboard service to use.
        private IClipboardService _clipboard;
        // The directory locator service.
        private IDirectoryLocateService _directoryLocator;
        // The application settings.
        private EditorSettings _settings;
        // The content plugin service.
        private IContentPluginManagerService _contentPlugins;
        // The search results for a search query.
        private List<IFileExplorerNodeVm> _searchResults;
        // The system used to search through the file system.
        private ISearchService<IFileExplorerNodeVm> _searchSystem;
        // The file scanning service used to determine if a file is associated with a plugin or not.
        private IFileScanService _fileScanner;
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
        public IEditorCommand<DeleteNodeArgs> DeleteNodeCommand
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

        /// <summary>
        /// Property to return the command used to copy a single node.
        /// </summary>
        public IEditorCommand<CopyNodeArgs> CopyNodeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to export a node's contents to the physical file system
        /// </summary>
        public IEditorCommand<IFileExplorerNodeVm> ExportNodeToCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to move a single node.
        /// </summary>
        public IEditorCommand<CopyNodeArgs> MoveNodeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to import files and directories into the specified node.
        /// </summary>
        public IEditorAsyncCommand<IFileExplorerNodeVm> ImportIntoNodeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to delete the file system.
        /// </summary>
        public IEditorCommand<object> DeleteFileSystemCommand
        {
            get;
        }

        /// <summary>
        /// Property to set or return the command to execute when a content node is opened.
        /// </summary>
        public IEditorCommand<IContentFile> OpenContentFileCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the list of search results for a filtered node list.
        /// </summary>
        public IReadOnlyList<IFileExplorerNodeVm> SearchResults => _searchResults;

        /// <summary>
        /// Property to return the command used to perform a search for files.
        /// </summary>
        public IEditorCommand<string> SearchCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute to create a new, empty content file.
        /// </summary>
        public IEditorCommand<CreateContentFileArgs> CreateContentFileCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to ensure that a node can be moved (or copied).
        /// </summary>
        /// <param name="node">The node being moved/copied.</param>
        /// <param name="targetNode">The target node for the moved/copied node.</param>
        /// <param name="isCopy"><b>true</b> if the operation is a copy operation, <b>false</b> if it is a move operation.</param>
        /// <returns><b>true</b> if the node can be moved/copied, <b>false</b> if not.</returns>
        private bool CheckNodeMovement(IFileExplorerNodeVm node, IFileExplorerNodeVm targetNode, bool isCopy)
        {
            if ((_searchResults != null)
                || (node == null)
                || (targetNode == null))
            {
                return false;
            }

            if (!targetNode.AllowChildCreation)
            {
                return false;
            }

            if (isCopy)
            {
                return true;
            }

            if (targetNode == node)
            {
                return false;
            }

            if (node.Parent == targetNode)
            {
                return false;
            }

#pragma warning disable IDE0046 // Convert to conditional expression
            if (node.IsAncestorOf(targetNode))
            {
                return false;
            }
#pragma warning restore IDE0046 // Convert to conditional expression

            return true;
        }

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
        private bool CanCreateNode(CreateNodeArgs args) => (SearchResults == null) && ((SelectedNode == null) || (SelectedNode.AllowChildCreation));

        /// <summary>
        /// Function to determine if a file system node can be deleted.
        /// </summary>
        /// <param name="args">The node to delete.</param>
        /// <returns><b>true</b> if the node can be deleted, <b>false</b> if not.</returns>
        private bool CanDeleteNode(DeleteNodeArgs args) => (SelectedNode != null) && (SelectedNode.AllowDelete);

        /// <summary>
        /// Function to remove the specified node, and any children from the cache.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        private void RemoveFromCache(IFileExplorerNodeVm node)
        {
            if (node == RootNode)
            {
                ClearFromCache(node);
                return;
            }

            IEnumerable<IFileExplorerNodeVm> children = node.Children.Traverse(n => n.Children);

            foreach (IFileExplorerNodeVm child in children)
            {
                child.Children.CollectionChanged -= Children_CollectionChanged;
                child.PropertyChanging -= Node_PropertyChanging;
                child.PropertyChanged -= Node_PropertyChanged;
                _nodePathLookup.Remove(child.FullPath);
            }

            node.Children.CollectionChanged -= Children_CollectionChanged;
            node.PropertyChanging -= Node_PropertyChanging;
            node.PropertyChanged -= Node_PropertyChanged;
            _nodePathLookup.Remove(node.FullPath);
        }

        /// <summary>
        /// Function to delete the selected node.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        private async void DoDeleteNode(DeleteNodeArgs args)
        {
            bool itemsDeleted = false;
            var cancelSource = new CancellationTokenSource();

            try
            {
                bool hasChildren = args.Node.Children.Count != 0;
                bool hasDependants = args.Node.Dependencies.Count != 0;

                string message = hasChildren ? string.Format(Resources.GOREDIT_CONFIRM_DELETE_CHILDREN, args.Node.FullPath)
                                                : string.Format(Resources.GOREDIT_CONFIRM_DELETE_NO_CHILDREN, args.Node.FullPath);

                if (hasDependants)
                {
                    message = string.Format(Resources.GOREDIT_CONFIRM_DELETE_NODE_DEPENDANTS, args.Node.FullPath);

                    if (args.Node.Dependencies.Any(item => item.IsOpen && item.IsChanged))
                    {
                        message = string.Format(Resources.GOREDIT_CONFIRM_DELETE_NODE_DEPENDANT_OPEN, args.Node.FullPath);
                    }
                }
                else
                {
                    if (((args.Node.IsOpen) && (args.Node.IsChanged))
                        || ((hasChildren) && (args.Node.Children.Traverse(n => n.Children).Any(item => item.IsOpen && item.IsChanged))))
                    {
                        message = string.Format(Resources.GOREDIT_CONFIRM_FILE_OPEN_DELETE, args.Node.FullPath);
                    }
                }

                if (_messageService.ShowConfirmation(message) != MessageResponse.Yes)
                {
                    return;
                }

                if (!hasChildren)
                {
                    ShowWaitPanel(Resources.GOREDIT_TEXT_DELETING);
                    string path = args.Node.FullPath;

                    await args.Node.DeleteNodeAsync();

                    if ((_searchResults != null) && (_searchResults.Contains(args.Node)))
                    {
                        _searchResults.Remove(args.Node);
                    }

                    // Notify that we have file system changes when done.
                    itemsDeleted = true;
                    return;
                }

                // Function to update the delete progress information and handle metadata update.
                void UpdateDeleteProgress(IFileExplorerNodeVm fileSystemItem)
                {
                    UpdateMarequeeProgress($"{fileSystemItem.FullPath}", Resources.GOREDIT_TEXT_DELETING, cancelSource.Cancel);

                    // Notify that we have file system changes when done.
                    itemsDeleted = true;

                    // Give our UI time to update.  
                    // We do this here so the user is able to click the Cancel button should they need it.
                    Thread.Sleep(16);
                }

                var deletedNodes = new List<IFileExplorerNodeVm>();

                if (_searchResults != null)
                {
                    foreach (IFileExplorerNodeVm node in args.Node.Children.Traverse(n => n.Children))
                    {
                        if (_searchResults.Contains(node))
                        {
                            deletedNodes.Add(node);
                        }
                    }
                }

                await args.Node.DeleteNodeAsync(UpdateDeleteProgress, cancelSource.Token);

                // Update the search list.
                if (_searchResults == null)
                {
                    return;
                }

                foreach (IFileExplorerNodeVm node in deletedNodes)
                {
                    _searchResults.Remove(node);
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_DELETE, args.Node));
            }
            finally
            {
                HideProgress();
                HideWaitPanel();

                // We have to use this flag because the callback is on a separate thread and we'll end up with a cross thread call if we trigger this event from there.
                if (itemsDeleted)
                {
                    OnFileSystemChanged();
                }

                cancelSource.Dispose();
            }
        }

        /// <summary>
        /// Function to create a node under the selected node.
        /// </summary>
        /// <param name="args">Arguments for the command.</param>
        private void DoCreateNode(CreateNodeArgs args)
        {
            try
            {
                IFileExplorerNodeVm parent = args.ParentNode ?? RootNode;
                _factory.CreateNewDirectoryNode(_project, _fileSystemService, parent);

                OnFileSystemChanged();
            }
            catch (Exception ex)
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
        /// Function to determine if a node can be renamed or not.
        /// </summary>
        /// <param name="args">The arguments for renaming.</param>
        /// <returns><b>true</b> if the node can be renamed, or <b>false</b> if not.</returns>
        private bool CanRenameNode(FileExplorerNodeRenameArgs args) => (SelectedNode != null)
                    && (!string.IsNullOrWhiteSpace(args.NewName))
                    && (!string.Equals(args.NewName, args.OldName, StringComparison.CurrentCulture));

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
                IFileExplorerNodeVm sibling = args.Node.Parent.Children.FirstOrDefault(item => (string.Equals(item.Name, args.NewName, StringComparison.CurrentCultureIgnoreCase)) && (item != args.Node));

                if (sibling != null)
                {
                    _messageService.ShowError(string.Format(Resources.GOREDIT_ERR_NODE_EXISTS, args.NewName));
                    args.Cancel = true;
                    return;
                }

                args.Node.RenameNode(args.NewName);

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
        /// Function to determine if a content file can be created.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        /// <returns><b>true</b> if the content file can be created, <b>false</b> if not.</returns>
        private bool CanCreateContentFile(CreateContentFileArgs args) => !string.IsNullOrWhiteSpace(args.Name);

        /// <summary>
        /// Function to create a new content file.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        private void DoCreateContentFile(CreateContentFileArgs args)
        {
            FileInfo file = null;
            
            // Find out which directory is selected.
            IFileExplorerNodeVm parent = SelectedNode;

            try
            {
                if (parent == null)
                {
                    parent = RootNode;
                }
                else
                {
                    while (!parent.AllowChildCreation)
                    {
                        parent = parent.Parent;
                    }
                }

                int count = 0;
                string name = args.Name;

                while (parent.Children.Any(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)))
                {
                    name = $"{args.Name} ({++count})";
                }

                // Update the name if we renamed it.
                args.Name = name;

                var physicalParent = new DirectoryInfo(parent.PhysicalPath);
                file = _fileSystemService.CreateEmptyFile(physicalParent, args.Name);
                                
                SelectedNode = args.Node = _factory.CreateFileExplorerFileNodeVm(_project, _fileSystemService, parent, file);
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_CREATING_CONTENT_FILE, args.Name));
                args.Cancel = true;

                // Clean up should we mess up.
                if ((file != null) && (file.Exists))
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception subEx)
                    {
                        Log.Print($"Could not delete the working file for '{args.Name}'", LoggingLevel.Simple);
                        Log.LogException(subEx);
                    }
                }
            }
        }

        /// <summary>
        /// Function to perform a refresh of a node.
        /// </summary>
        /// <param name="node">The node to refresh.</param>
        /// <param name="deepScanForAssociation"><b>true</b> to force a deep scan for file associations (much slower), <b>false</b> to do a surface scan.</param>
        private void RefreshNode(IFileExplorerNodeVm node, bool deepScanForAssociation)
        {
            IFileExplorerNodeVm nodeToRefresh = node ?? RootNode;

            // Check for open content.
            string openContentPath = null;
            var changedFilePaths = new HashSet<string>();
            var searchNodes = new List<string>();
            var depNodes = new List<IContentFile>();

            // Reset the metadata list.  The enumeration method requires that we have this list up to date.
            _project.ProjectItems.Clear();

            foreach (IFileExplorerNodeVm child in nodeToRefresh.Children.Traverse(n => n.Children))
            {
                if (_searchResults?.Remove(child) ?? false)
                {
                    searchNodes.Add(child.FullPath);
                }

                if (child.Metadata != null)
                {
                    _project.ProjectItems[child.FullPath] = child.Metadata;

                    foreach (IFileExplorerNodeVm fileNode in child.Dependencies)
                    {
                        if (fileNode is IContentFile depNode)
                        {
                            fileNode.OnUnload();
                            depNodes.Add(depNode);
                        }
                    }
                }

                if (child.IsOpen)
                {
                    openContentPath = child.FullPath;
                }

                if ((child.IsChanged) && (!changedFilePaths.Contains(child.FullPath)))
                {
                    changedFilePaths.Add(child.FullPath);
                }
            }

            nodeToRefresh.Children.Clear();

            var parent = new DirectoryInfo(nodeToRefresh == RootNode ? _project.FileSystemDirectory.FullName : nodeToRefresh.PhysicalPath);

            if (!parent.Exists)
            {
                return;
            }

            _factory.EnumerateFileSystemObjects(parent.FullName, _project, _fileSystemService, nodeToRefresh);

            // We don't need the metadata list now, all objects have their metadata assigned at this point.
            _project.ProjectItems.Clear();

            // Rebuild file associations.
            foreach (IFileExplorerNodeVm child in nodeToRefresh.Children.Traverse(n => n.Children))
            {
                if (searchNodes.Any(item => string.Equals(child.FullPath, item, StringComparison.OrdinalIgnoreCase)))
                {
                    if (_searchResults == null)
                    {
                        _searchResults = new List<IFileExplorerNodeVm>();
                    }

                    _searchResults.Add(child);
                }

                // Restore the open flag.
                if ((openContentPath != null) && (string.Equals(child.FullPath, openContentPath, StringComparison.OrdinalIgnoreCase)))
                {
                    child.IsOpen = true;
                }

                if (changedFilePaths.Contains(child.FullPath))
                {
                    child.IsChanged = true;
                }
            }

            // Rebuild dependencies.
            foreach (IContentFile dep in depNodes)
            {
                SetupDependencyNodes(dep);                
            }
        }

        /// <summary>
        /// Function to refresh the nodes under the specified node, or for the entire tree.
        /// </summary>
        /// <param name="node">The node to refresh.</param>
        private async void DoRefreshSelectedNodeAsync(IFileExplorerNodeVm node)
        {
            _busyService.SetBusy();

            try
            {
                RefreshNode(node, false);
                _busyService.SetIdle();

                // Scan the files now for updated plugin information.
                await ScanFilesAsync(node);

                OnFileSystemChanged();
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
        /// Function to determine if a node can be copied.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        /// <returns><b>true</b> if the node can be copied, <b>false</b> if not.</returns>
        private bool CanCopyNode(CopyNodeArgs args) => CheckNodeMovement(args?.Source, args?.Dest, true);

        /// <summary>
        /// Function to determine if a node can be moved.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        /// <returns><b>true</b> if the node can be moved, <b>false</b> if not.</returns>
        private bool CanMoveNode(CopyNodeArgs args) => CheckNodeMovement(args?.Source, args?.Dest, false);

        /// <summary>
        /// Function to move a node.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        private void DoMoveNode(CopyNodeArgs args)
        {
            _busyService.SetBusy();

            try
            {
                MoveNode(args.Source, args.Dest);
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_CANNOT_MOVE, args.Source.Name, args.Dest.FullPath));
            }
            finally
            {
                _busyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to determine if a node can have files and/or directories imported.
        /// </summary>
        /// <param name="node">The node to evaluate.</param>
        /// <returns><b>true</b> if the node is able to import file/directory data, or <b>false</b> if not.</returns>
        private bool CanImportIntoNode(IFileExplorerNodeVm node) => (node == null) || (node.AllowChildCreation);

        /// <summary>
        /// Function to determine if a node can be exported.
        /// </summary>
        /// <param name="node">The node to evaluate.</param>
        /// <returns><b>true</b> if the node can be exported, <b>false</b> if not.</returns>
        private bool CanExportNode(IFileExplorerNodeVm node) => ((node == null) && (RootNode.Children.Count > 0) && (_searchResults == null))
                                                                || (node.Children.Count > 0)
                                                                || ((node != null) && (!node.AllowChildCreation));

        /// <summary>
        /// Function to allow a user to resolve a confict between files with the same name.
        /// </summary>
        /// <param name="sourceItem">The file being copied.</param>
        /// <param name="destItem">The destination file.</param>
        /// <param name="usePhysicalPath"><b>true</b> to display the physical path for the destination, or <b>false</b> to display the virtual path.</param>
        /// <returns>A <see cref="FileSystemConflictResolution"/> value that indicates how to proceed.</returns>
        private FileSystemConflictResolution ResolveImportConflict(FileSystemInfo sourceItem, FileSystemInfo destItem)
        {
            MessageResponse response = _messageService.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_FILE_EXISTS, sourceItem.Name, destItem.ToFileSystemPath(_project.FileSystemDirectory)), toAll: true, allowCancel: true);

            switch (response)
            {
                case MessageResponse.Yes:
                    return FileSystemConflictResolution.Overwrite;
                case MessageResponse.YesToAll:
                    return FileSystemConflictResolution.OverwriteAll;
                case MessageResponse.No:
                    return FileSystemConflictResolution.Rename;
                case MessageResponse.NoToAll:
                    return FileSystemConflictResolution.RenameAll;
                default:
                    return FileSystemConflictResolution.Cancel;
            }
        }

        /// <summary>
        /// Function to prepare the file import by retrieving all of the file system objects, and their children, in the paths list.
        /// </summary>
        /// <param name="paths">The list of paths to import.</param>
        /// <returns>A tuple containing the list of directories to import, and the list of files to import.</returns>
        private (List<DirectoryInfo> directories, List<FileInfo> files) PrepFileImport(IReadOnlyList<string> paths)
        {
            var directories = new List<DirectoryInfo>();
            var files = new List<FileInfo>();

            bool IsDirectoryValid(DirectoryInfo directory) => (((directory.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                                                            && ((directory.Attributes & FileAttributes.System) != FileAttributes.System)
                                                            && ((directory.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden));
            bool IsFileValid(FileInfo file) => ((!string.Equals(file.Name, CommonEditorConstants.EditorMetadataFileName, StringComparison.OrdinalIgnoreCase))
                                                            && ((file.Attributes & FileAttributes.Directory) != FileAttributes.Directory)
                                                            && ((file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                                                            && ((file.Attributes & FileAttributes.System) != FileAttributes.System));

            // Convert the list of paths into file system objects so we can easily manipulate them.
            foreach (string path in paths.OrderBy(item => item.Length))
            {
                // Convert the path to either a directory or file object.
                if (Directory.Exists(path))
                {
                    var directory = new DirectoryInfo(path);

                    if (!IsDirectoryValid(directory))
                    {
                        continue;
                    }

                    directories.Add(directory);

                    // Get all files and sub directories under this single directory.
                    foreach (DirectoryInfo subDir in directory.GetDirectories("*", SearchOption.AllDirectories))
                    {
                        if (IsDirectoryValid(subDir))
                        {
                            directories.Add(subDir);
                        }
                    }

                    foreach (FileInfo file in directory.GetFiles("*", SearchOption.AllDirectories))
                    {
                        if (IsFileValid(file))
                        {
                            files.Add(file);
                        }
                    }
                }
                else if (File.Exists(path))
                {
                    var file = new FileInfo(path);

                    if (IsFileValid(file))
                    {
                        files.Add(file);
                    }
                }
            }

            return (directories, files);
        }

        /// <summary>
        /// Function to perform a custom import on a file.
        /// </summary>
        /// <param name="file">The file being imported.</param>
        /// <param name="fileSystem">The file system being imported.</param>
        /// <param name="cancelToken">The token used to cancel the operation.</param>
        /// <returns>The importer used to import, and the imported file.</returns>
        private async Task<(IEditorContentImporter importer, FileInfo importedFile)> CustomImportFileAsync(FileInfo file, IGorgonFileSystem fileSystem, CancellationToken cancelToken)
        {
            FileInfo result;
            IEditorContentImporter importer = _factory.ContentImporterPlugins.GetContentImporter(file, fileSystem);

            if (importer == null)
            {
                return (null, file);
            }

            try
            {
                result = await Task.Run(() => importer.ImportData(_project.TempDirectory, cancelToken), cancelToken);

                // Do not import the file if we don't process it.
                if (result == null)
                {
                    return (null, null);
                }
            }
            catch (Exception ex)
            {
                // If we failed to import for some reason, log the error and read the file as-is.
                Program.Log.Print($"Error importing file '{file.FullName}'.", Diagnostics.LoggingLevel.Simple);
                Program.Log.LogException(ex);

                result = file;
            }

            return (importer, result);
        }

        /// <summary>
        /// Function to perform the import of the directories/files into the specified node.
        /// </summary>
        /// <param name="node">The node that will recieve the import.</param>
        /// <param name="parentDir">The parent directory for all imported items.</param>
        /// <param name="items">The paths to the items to import.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private async Task ImportAsync(IFileExplorerNodeVm node, DirectoryInfo parentDir, IReadOnlyList<string> items)
        {
            byte[] writeBuffer = new byte[81920];
            FileInfo currentFile = null;
            long totalByteCount = 0;
            long totalBytesCopied = 0;
            var cancelSource = new CancellationTokenSource();
            IGorgonFileSystem importFileSystem = new GorgonFileSystem(Log);
            (IEditorContentImporter importer, FileInfo updatedFile) importResult = default;

            void cancelAction() => cancelSource.Cancel();

            void UpdateImportProgress(long bytesCopied, long totalBytes)
            {
                if (currentFile == null)
                {
                    return;
                }

                float percent = (totalBytesCopied + bytesCopied) / (float)totalByteCount;
                UpdateProgress(new ProgressPanelUpdateArgs
                {
                    Title = Resources.GOREDIT_TEXT_IMPORTING,
                    CancelAction = cancelAction,
                    Message = currentFile.FullName.Ellipses(65, true),
                    PercentageComplete = percent
                });
            }

            try
            {
                UpdateProgress(new ProgressPanelUpdateArgs
                {
                    CancelAction = cancelAction,
                    Title = Resources.GOREDIT_TEXT_IMPORTING,
                    PercentageComplete = 0,
                    Message = string.Empty
                });

                (List<DirectoryInfo> directories, List<FileInfo> files) = PrepFileImport(items);

                // No directories or files to import, so we're done.
                if ((directories.Count == 0) && (files.Count == 0))
                {
                    return;
                }

                totalByteCount = files.Count > 0 ? files.Sum(item => item.Length) : 0;

                // Some things will require that we have the file system available to us for reading while importing.
                importFileSystem.Mount(_project.FileSystemDirectory.FullName.FormatDirectory(Path.DirectorySeparatorChar));

                // Allow some time for the UI to update.
                await Task.Delay(500);

                // Collapse the destination node prior to importing.
                // This allows us to update the file system without updating the UI, which will keep us from having cross threading issues.
                node.IsExpanded = false;

                // Create the directory structure first.
                foreach (DirectoryInfo directory in directories)
                {
                    // Create the directory (if needed).
                    var newDir = new DirectoryInfo(directory.FullName.Replace(parentDir.FullName.FormatDirectory(Path.DirectorySeparatorChar), node.PhysicalPath));
                    newDir.Create();

                    if (cancelSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                }

                FileSystemConflictResolution resolution = FileSystemConflictResolution.Skip;
                var importedFiles = new Dictionary<string, FileInfo>(StringComparer.OrdinalIgnoreCase);

                // Finally, copy the files.
                foreach (FileInfo file in files)
                {
                    if (cancelSource.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    currentFile = file;
                    var newFile = new FileInfo(currentFile.FullName.Replace(parentDir.FullName.FormatDirectory(Path.DirectorySeparatorChar), node.PhysicalPath));

                    importResult = await CustomImportFileAsync(currentFile, importFileSystem, cancelSource.Token);

                    if (importResult.updatedFile == null)
                    {
                        continue;
                    }

                    if ((importResult.importer != null) && (importResult.updatedFile != null))
                    {
                        // Update the total size by the new imported file.
                        if (file.Length != importResult.updatedFile.Length)
                        {
                            totalByteCount -= file.Length;
                            totalByteCount += importResult.updatedFile.Length;
                        }

                        // Update the output and current file to match the newly imported data.
                        newFile = new FileInfo(Path.Combine(newFile.Directory.FullName, importResult.updatedFile.Name));
                        currentFile = importResult.updatedFile;
                    }

                    // We have a conflict, try to find the node that is conflict.
                    if (newFile.Exists)
                    {
                        IFileExplorerNodeVm dupeNode = node.Children.Traverse(n => n.Children).FirstOrDefault(n => string.Equals(n.PhysicalPath, newFile.FullName, StringComparison.OrdinalIgnoreCase));
                        FileSystemConflictResolution currentResolution = resolution;

                        if ((dupeNode != null)
                            && (((dupeNode.IsOpen) && (resolution == FileSystemConflictResolution.OverwriteAll))
                                || ((resolution != FileSystemConflictResolution.OverwriteAll) && (resolution != FileSystemConflictResolution.RenameAll))))
                        {
                            currentResolution = FileSystemConflictHandler(dupeNode, dupeNode, true, true);
                        }

                        switch (currentResolution)
                        {
                            case FileSystemConflictResolution.Overwrite:
                            case FileSystemConflictResolution.OverwriteAll:
                                break;
                            case FileSystemConflictResolution.Rename:
                            case FileSystemConflictResolution.RenameAll:
                                string newFilePath = _fileSystemService.GenerateFileName(newFile.FullName);
                                newFile = new FileInfo(Path.Combine(newFile.Directory.FullName, newFilePath));
                                break;
                            case FileSystemConflictResolution.Skip:
                                currentResolution = FileSystemConflictResolution.OverwriteAll;
                                continue;
                            case FileSystemConflictResolution.Cancel:
                                return;
                        }

                        if ((currentResolution == FileSystemConflictResolution.OverwriteAll) || (currentResolution == FileSystemConflictResolution.RenameAll))
                        {
                            resolution = currentResolution;
                        }
                    }

                    await Task.Run(() => _fileSystemService.ImportFile(currentFile, newFile, UpdateImportProgress, cancelSource.Token, writeBuffer), cancelSource.Token);
                    totalBytesCopied += file.Length;

                    // Don't leave working files lying around.
                    if ((importResult.importer != null) && (importResult.updatedFile != null))
                    {
                        // This is the destination file for our backup of the imported file.
                        string sourcePath = Path.Combine(_project.SourceDirectory.FullName, Guid.NewGuid().ToString("N")) + file.Extension;
                        FileInfo copy = file.CopyTo(sourcePath, true);

                        // Record this import so we can update the node attributes after reload.
                        importedFiles[newFile.FullName] = copy;

                        if ((importResult.importer.NeedsCleanup) && (importResult.updatedFile.Exists))
                        {
                            importResult.updatedFile.Delete();
                            importResult.updatedFile.Refresh();
                        }
                    }
                }

                UpdateProgress(new ProgressPanelUpdateArgs
                {
                    CancelAction = null,
                    Title = Resources.GOREDIT_TEXT_IMPORTING,
                    PercentageComplete = 1
                });

                // Give a little delay so we can update the UI.
                await Task.Delay(500);

                // This will probably take a while to run for large file systems. But, because it modifies the UI, we can't put it on background thread.
                // We may have to refactor this later so we can thread it and give feed back.
                RefreshNode(node, true);

                await ScanFilesAsync(node);

                // Update the import attribute for the node. This only applies to content nodes.
                foreach (IFileExplorerNodeVm contentNode in node.Children.Traverse(n => n.Children).Where(item => item.IsContent))
                {
                    if (!importedFiles.TryGetValue(contentNode.PhysicalPath, out FileInfo sourcePath))
                    {
                        continue;
                    }

                    contentNode.Metadata.Attributes[ContentImportPlugin.ImportOriginalFileNameAttr] = sourcePath.Name;
                }

                node.IsExpanded = true;
                SelectedNode = node;

                OnFileSystemChanged();
            }
            finally
            {
                if ((importResult.importer != null) && (importResult.updatedFile != null) && (importResult.importer.NeedsCleanup) && (importResult.updatedFile.Exists))
                {
                    importResult.updatedFile.Delete();
                }
                HideProgress();
                cancelSource?.Dispose();
            }
        }

        /// <summary>
        /// Function to import a files and directories into the specified node.
        /// </summary>
        /// <param name="node">The node to update.</param>
        private async Task DoImportIntoNodeAsync(IFileExplorerNodeVm node)
        {
            DirectoryInfo sourceDir = null;

            if (node == null)
            {
                node = RootNode;
            }

            try
            {
                sourceDir = _directoryLocator.GetDirectory(new DirectoryInfo(_settings.LastOpenSavePath.FormatDirectory(Path.DirectorySeparatorChar)), Resources.GOREDIT_TEXT_IMPORT_FROM);

                if (sourceDir == null)
                {
                    return;
                }

                await ImportAsync(node, sourceDir, sourceDir.GetFileSystemInfos().Select(item => item.FullName).ToArray());

                OnFileSystemChanged();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_IMPORT, node.FullPath));
            }
            finally
            {
                HideProgress();
            }
        }

        /// <summary>
        /// Function to export a node and its contents to the physical file system.
        /// </summary>
        /// <param name="node">The node to export.</param>
        private async void DoExportNodeAsync(IFileExplorerNodeVm node)
        {
            var cancelSource = new CancellationTokenSource();

            if (node == null)
            {
                node = RootNode;
            }

            DirectoryInfo destDir = null;

            try
            {
                destDir = _directoryLocator.GetDirectory(new DirectoryInfo(_settings.LastOpenSavePath.FormatDirectory(Path.DirectorySeparatorChar)), Resources.GOREDIT_TEXT_EXPORT_TO);

                if (destDir == null)
                {
                    return;
                }

                void cancelAction() => cancelSource.Cancel();

                UpdateProgress(new ProgressPanelUpdateArgs
                {
                    CancelAction = cancelAction,
                    Title = Resources.GOREDIT_TEXT_COPYING,
                    PercentageComplete = 0,
                    Message = string.Empty
                });

                // Function to update our progress meter and provide updates to our caching (and inclusion functionality).
                void ExportProgress(IFileExplorerNodeVm sourceItem, long bytesCopied, long totalBytes)
                {
                    float percent = bytesCopied / (float)totalBytes;
                    UpdateProgress(new ProgressPanelUpdateArgs
                    {
                        Title = Resources.GOREDIT_TEXT_EXPORTING,
                        CancelAction = cancelAction,
                        Message = sourceItem.FullPath.Ellipses(65, true),
                        PercentageComplete = percent
                    });
                }

                await node.ExportAsync(new ExportNodeData
                {
                    CancelToken = cancelSource.Token,
                    ConflictHandler = ExportSystemConflictHandler,
                    CopyProgress = ExportProgress,
                    Destination = destDir
                });

                _settings.LastOpenSavePath = destDir.FullName.FormatDirectory(Path.DirectorySeparatorChar);
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_EXPORT, destDir?.FullName ?? Resources.GOREDIT_TEXT_UNKNOWN));
            }
            finally
            {
                cancelSource.Dispose();
                HideProgress();
            }
        }

        /// <summary>
        /// Function to copy a node.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        private async void DoCopyNode(CopyNodeArgs args)
        {
            try
            {
                await CopyNodeAsync(args.Source, args.Dest);
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_REFRESH);
            }
        }

        /// <summary>
        /// Function to determine if the entire file system can be deleted or not.
        /// </summary>
        /// <returns><b>true</b> if the file system can be deleted, <b>false</b> if not.</returns>
        private bool CanDeleteFileSystem() => RootNode.Children.Count > 0;

        /// <summary>
        /// Function to delete the entire file system.
        /// </summary>
        private async void DoDeleteFileSystemAsync()
        {
            try
            {
                IFileExplorerNodeVm openNode = RootNode.Children.Traverse(n => n.Children).FirstOrDefault(n => n.IsOpen);

                // If we have an open node, and it has unsaved changes, prompt with:
                //
                // "There is a file open in the editor that has unsaved changes.
                // Deleting this file will result in the loss of these changes.
                // 
                // Are you sure you wish to delete this file?
                //
                // Yes/No" (no cancel because if we say no, we stop deleting).
                if (openNode != null)
                {
                    // TODO: Check for changes and prompt.
                }

                if (_messageService.ShowConfirmation(Resources.GOREDIT_CONFIRM_DELETE_ALL) == MessageResponse.No)
                {
                    return;
                }

                UpdateMarequeeProgress(Resources.GOREDIT_TEXT_CLEARING_FILESYSTEM);

                // Delete all files and directories.
                await Task.Run(() => _fileSystemService.DeleteAll());

                RootNode.Children.Clear();
                _searchResults = null;
                NotifyPropertyChanged(nameof(SearchResults));
                OnFileSystemChanged();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_DELETE_ALL);
            }
            finally
            {
                HideProgress();
            }
        }

        /// <summary>
        /// Function to perform a basic search for files.
        /// </summary>
        /// <param name="searchText">The text to search for.</param>
        private void DoSearch(string searchText)
        {
            _busyService.SetBusy();

            try
            {
                // Reset the selected node
                IFileExplorerNodeVm selected = SelectedNode;
                SelectedNode = null;

                NotifyPropertyChanging(nameof(SearchResults));
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    _searchResults = null;
                }
                else
                {
                    _searchResults = _searchSystem.Search(searchText)?.ToList();
                }
                NotifyPropertyChanged(nameof(SearchResults));

                if ((selected != null) && ((_searchResults == null) || (_searchResults.Contains(selected))))
                {
                    if ((!selected.AllowChildCreation) && (!selected.Parent.IsExpanded))
                    {
                        IFileExplorerNodeVm parent = selected.Parent;

                        while (parent != null)
                        {
                            parent.IsExpanded = true;
                            parent = parent.Parent;
                        }
                    }

                    SelectedNode = selected;
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_SEARCH, searchText));
            }
            finally
            {
                _busyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to copy a node to another node.
        /// </summary>
        /// <param name="source">The source node to copy.</param>
        /// <param name="dest">The destination that will receive the copy.</param>
        /// <returns><b>true</b> if the node was moved, <b>false</b> if not.</returns>
        private bool MoveNode(IFileExplorerNodeVm source, IFileExplorerNodeVm dest)
        {
            // Locate the next level that allows child creation if this one cannot.
            while (!dest.AllowChildCreation)
            {
                dest = dest.Parent;

                Debug.Assert(dest != null, "No container node found.");
            }

            // If we move to the same location, then we are done. No sense in doing extra for no reason.
            if (string.Equals(source.Parent.FullPath, dest.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            bool sourceIsExpanded = (source.AllowChildCreation) && (source.Children.Count > 0) && (source.IsExpanded);
            string originalPath = source.FullPath;
            (string path, IFileExplorerNodeVm node)[] originalChildPaths = null;

            if ((source.AllowChildCreation) && (source.Children.Count > 0))
            {
                originalChildPaths = source.Children.Traverse(n => n.Children)
                                                    .Select(item => (item.FullPath, item))
                                                    .ToArray();
            }

            // Collapse the node prior to moving it.
            // This is just to mitigate the possibility of event oversubscription (shouldn't happen, but paranoia being what it is...)
            if (sourceIsExpanded)
            {
                source.IsExpanded = false;
            }

            IFileExplorerNodeVm updatedNode = source.MoveNode(new CopyNodeData()
            {
                Destination = dest,
                UseToAllInConflictHandler = (source.Children.Count > 0) && (dest.AllowChildCreation),
                ConflictHandler = FileSystemConflictHandler
            });

            // If we cancelled, or there was an error, then leave.
            if (updatedNode == null)
            {
                return false;
            }

            _nodePathLookup.Remove(originalPath);

            if ((originalChildPaths != null) && (originalChildPaths.Length > 0))
            {
                foreach ((string path, IFileExplorerNodeVm node) in originalChildPaths)
                {
                    if (node.IsOpen)
                    {
                        node.NotifyParentMoved(node);
                    }

                    _nodePathLookup.Remove(path);
                    node.Refresh();
                    _nodePathLookup[node.FullPath] = node;
                }
            }

            // Expand our destination parent.
            dest.IsExpanded = true;

            // If this node is not registered, then add it now.
            _nodePathLookup[source.FullPath] = source;

            SelectedNode = source;

            if (sourceIsExpanded)
            {
                source.IsExpanded = true;
            }

            OnFileSystemChanged();

            return true;
        }

        /// <summary>
        /// Function called when there is a conflict when copying or moving files.
        /// </summary>
        /// <param name="sourceItem">The file being copied/moved.</param>
        /// <param name="destItem">The destination file that is conflicting.</param>
        /// <param name="allowCancel"><b>true</b> to allow cancel support, or <b>false</b> to only use yes/no prompts.</param>
        /// <param name="toAll"><b>true</b> to apply the resolution to all items after the first resolution is decided, or <b>false</b> to prompt the user each time a conflict arises.</param>
        /// <returns>A resolution for the conflict.</returns>
        private FileSystemConflictResolution FileSystemConflictHandler(IFileExplorerNodeVm sourceItem, IFileExplorerNodeVm destItem, bool allowCancel, bool toAll)
        {
            bool isBusy = _busyService.IsBusy;

            // Reset the busy state.  The dialog will disrupt it anyway.
            _busyService.SetIdle();
            MessageResponse response;

            try
            {
                if (destItem.IsOpen)
                {
                    response = _messageService.ShowConfirmation(string.Format(Resources.GOREDIT_MSG_OPEN_CONTENT_CANT_OVERWRITE, sourceItem.Name), allowCancel: allowCancel);

                    switch (response)
                    {
                        case MessageResponse.Yes:
                            return FileSystemConflictResolution.Rename;
                        case MessageResponse.No:
                            return FileSystemConflictResolution.Skip;
                        default:
                            return FileSystemConflictResolution.Cancel;
                    }
                }

                response = _messageService.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_FILE_EXISTS, sourceItem.Name, destItem.Parent.FullPath.Ellipses(65, true)), toAll: toAll, allowCancel: allowCancel);

                switch (response)
                {
                    case MessageResponse.Yes:
                        return FileSystemConflictResolution.Overwrite;
                    case MessageResponse.YesToAll:
                        return FileSystemConflictResolution.OverwriteAll;
                    case MessageResponse.No:
                        return FileSystemConflictResolution.Rename;
                    case MessageResponse.NoToAll:
                        return FileSystemConflictResolution.RenameAll;
                    default:
                        return FileSystemConflictResolution.Cancel;
                }
            }
            finally
            {
                // Restore the busy state if we originally had it active.
                if (isBusy)
                {
                    _busyService.SetBusy();
                }
            }
        }

        /// <summary>
        /// Function called when there is a conflict when copying or moving files.
        /// </summary>
        /// <param name="sourceItem">The file being copied/moved.</param>
        /// <param name="destItem">The destination file that is conflicting.</param>
        /// <param name="allowCancel"><b>true</b> to allow cancel support, or <b>false</b> to only use yes/no prompts.</param>
        /// <param name="toAll"><b>true</b> to apply the resolution to all items after the first resolution is decided, or <b>false</b> to prompt the user each time a conflict arises.</param>
        /// <returns>A resolution for the conflict.</returns>
        private FileSystemConflictResolution ExportSystemConflictHandler(IFileExplorerNodeVm sourceItem, FileInfo destItem, bool allowCancel, bool toAll)
        {
            bool isBusy = _busyService.IsBusy;

            // Reset the busy state.  The dialog will disrupt it anyway.
            _busyService.SetIdle();
            MessageResponse response;

            try
            {
                response = _messageService.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_FILE_EXISTS, sourceItem.Name, destItem.Directory.FullName.Ellipses(65, true)), toAll: toAll, allowCancel: allowCancel);

                switch (response)
                {
                    case MessageResponse.Yes:
                        return FileSystemConflictResolution.Overwrite;
                    case MessageResponse.YesToAll:
                        return FileSystemConflictResolution.OverwriteAll;
                    case MessageResponse.No:
                        return FileSystemConflictResolution.Rename;
                    case MessageResponse.NoToAll:
                        return FileSystemConflictResolution.RenameAll;
                    default:
                        return FileSystemConflictResolution.Cancel;
                }
            }
            finally
            {
                // Restore the busy state if we originally had it active.
                if (isBusy)
                {
                    _busyService.SetBusy();
                }
            }
        }

        /// <summary>
        /// Function to copy a node to another node.
        /// </summary>
        /// <param name="source">The source node to copy.</param>
        /// <param name="dest">The destination that will receive the copy.</param>
        /// <returns>A task for async operation.</returns>
        private async Task CopyNodeAsync(IFileExplorerNodeVm source, IFileExplorerNodeVm dest)
        {
            var cancelSource = new CancellationTokenSource();

            try
            {
                // Locate the next level that allows child creation if this one cannot.
                while (!dest.AllowChildCreation)
                {
                    dest = dest.Parent;

                    Debug.Assert(dest != null, "No container node found.");
                }

                void CancelAction() => cancelSource.Cancel();

                UpdateProgress(new ProgressPanelUpdateArgs
                {
                    CancelAction = CancelAction,
                    Title = Resources.GOREDIT_TEXT_COPYING,
                    PercentageComplete = 0,
                    Message = string.Empty
                });

                // Collapse the node during copy.
                dest.IsExpanded = false;

                void CopyProgress(IFileExplorerNodeVm sourceItem, long bytesCopied, long totalBytes)
                {
                    float percent = bytesCopied / (float)totalBytes;

                    UpdateProgress(new ProgressPanelUpdateArgs
                    {
                        Title = Resources.GOREDIT_TEXT_COPYING,
                        CancelAction = CancelAction,
                        Message = sourceItem.FullPath.Ellipses(65, true),
                        PercentageComplete = percent
                    });
                }

                var data = new CopyNodeData
                {
                    CancelToken = cancelSource.Token,
                    CopyProgress = CopyProgress,
                    DefaultResolution = null,
                    Destination = dest,
                    ConflictHandler = FileSystemConflictHandler,
                    UseToAllInConflictHandler = source.AllowChildCreation
                };

                IFileExplorerNodeVm fileNode = await source.CopyNodeAsync(data);

                if ((fileNode == null) || (cancelSource.Token.IsCancellationRequested))
                {
                    return;
                }

                _nodePathLookup[fileNode.FullPath] = fileNode;

                if (!fileNode.Parent.IsExpanded)
                {
                    fileNode.Parent.IsExpanded = true;
                }

                SelectedNode = fileNode;

                OnFileSystemChanged();
            }
            finally
            {
                cancelSource.Dispose();
                HideProgress();
            }
        }

        /// <summary>
        /// Function to set up dependencies on the given node.
        /// </summary>
        /// <param name="node">The node to set up.</param>
        private void SetupDependencyNodes(IContentFile node)
        {
            if (node == null)
            {
                return;
            }

            var dependencyPaths = node.Metadata.Dependencies.ToDictionary(k => k.Key, v => v.Value, StringComparer.OrdinalIgnoreCase);

            if (dependencyPaths.Count == 0)
            {
                return;
            }

            Log.Print("Scanning node dependencies... This may take a bit...", LoggingLevel.Intermediate);

            if (!(node is IContentFile contentFile))
            {
                return;
            }

            foreach (KeyValuePair<string, string> path in dependencyPaths)
            {
                if (!_nodePathLookup.TryGetValue(path.Value, out IFileExplorerNodeVm parentNode))
                {
                    Log.Print($"[WARNING] The node '{node.Path}' has a dependency on '{path}', but no file system node was found that represents that path.", LoggingLevel.Simple);
                    continue;
                }

                Log.Print($"Found node dependency for '{path}'.", LoggingLevel.Verbose);
                DependencyNode depNode = _factory.CreateDependencyNode(_project, _fileSystemService, parentNode, contentFile);

                // Ensure that we don't already have this node in as a dependency, if we do, then dump it.
                IFileExplorerNodeVm existingDep = parentNode.Dependencies.FirstOrDefault(item => string.Equals(depNode.FullPath, item.FullPath, StringComparison.OrdinalIgnoreCase));
                if (existingDep != null)
                {
                    parentNode.Dependencies.Remove(existingDep);
                }

                parentNode.Dependencies.Add(depNode);
                _nodePathLookup[depNode.FullPath] = depNode;
            }
        }

        /// <summary>
        /// Function to enumerate all child nodes and cache them in a look up for quick access.
        /// </summary>
        /// <param name="parent">The parent node to start enumerating from.</param>
        private void EnumerateChildren(IFileExplorerNodeVm parent)
        {
            if (parent == RootNode)
            {
                _nodePathLookup["/"] = RootNode;
            }
            else
            {
                _nodePathLookup[parent.FullPath] = parent;
            }

            foreach (IFileExplorerNodeVm node in parent.Children.Traverse(n => n.Children))
            {
                _nodePathLookup[node.FullPath] = node;

                node.Children.CollectionChanged += Children_CollectionChanged;
                node.PropertyChanged += Node_PropertyChanged;
                node.PropertyChanging += Node_PropertyChanging;
            }

            // This is painful, but necessary as we need our node dictionary set up prior to using this.
            // The only other option is to traverse the tree while in the loop, and that's even worse than this.
            foreach (IContentFile content in parent.Children.Traverse(n => n.Children).OfType<IContentFile>())
            {
                SetupDependencyNodes(content);
            }

            SetupDependencyNodes(parent as IContentFile);

            parent.Children.CollectionChanged += Children_CollectionChanged;
            parent.PropertyChanged += Node_PropertyChanged;
            parent.PropertyChanging += Node_PropertyChanging;
        }

        /// <summary>
        /// Function called when a property changes on a node.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private void Node_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            var node = (IFileExplorerNodeVm)sender;

            switch (e.PropertyName)
            {
                case nameof(IFileExplorerNodeVm.FullPath):
                case nameof(IFileExplorerNodeVm.Name):
                    // Remove any children at this point.  They'll be re-added after the rename is complete.
                    foreach (IFileExplorerNodeVm child in node.Children.Traverse(n => n.Children))
                    {
                        _nodePathLookup.Remove(child.FullPath);
                    }

                    _nodePathLookup.Remove(node.FullPath);
                    break;
            }
        }

        /// <summary>
        /// Function called when a property changes on a node.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event parameters.</param>
        private void Node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = (IFileExplorerNodeVm)sender;

            switch (e.PropertyName)
            {
                case nameof(IFileExplorerNodeVm.FullPath):
                case nameof(IFileExplorerNodeVm.Name):
                    _nodePathLookup[node.FullPath] = node;

                    // Re-add any children at this point.
                    foreach (IFileExplorerNodeVm child in node.Children.Traverse(n => n.Children))
                    {
                        _nodePathLookup[child.FullPath] = child;
                    }
                    break;
            }
        }

        /// <summary>
        /// Function to clear a node's children from the cache.
        /// </summary>
        /// <param name="nodeToClear">The node to clear.</param>
        private void ClearFromCache(IFileExplorerNodeVm nodeToClear)
        {
            // Use the cache because the observable collection will have been cleared by this time, and this is the 
            // only way to locate the children of the cleared node.
            IFileExplorerNodeVm[] nodes = _nodePathLookup.Where(item => nodeToClear.IsAncestorOf(item.Value))
                                            .Select(item => item.Value)
                                            .ToArray();

            foreach (IFileExplorerNodeVm node in nodes)
            {
                node.Children.CollectionChanged -= Children_CollectionChanged;
                node.PropertyChanging -= Node_PropertyChanging;
                node.PropertyChanged -= Node_PropertyChanged;

                _nodePathLookup.Remove(node.FullPath);
            }
        }

        /// <summary>
        /// Function to scan the files added to the file system for plugin association.
        /// </summary>
        /// <param name="node">The node that was added to the file system.</param>
        /// <returns><b>true</b> if an association was made, or <b>false</b> if there were no changes.</returns>
        private async Task ScanFilesAsync(IFileExplorerNodeVm node)
        {
            try
            {
                // If we sent a non-content node, and there are no children to process, then move on.
                if ((!node.IsContent) && (node.Children.Count == 0))
                {
                    return;
                }

                UpdateProgress(new ProgressPanelUpdateArgs
                {
                    CancelAction = null,
                    IsMarquee = false,
                    Title = Resources.GOREDIT_TEXT_SCANNING
                });

                void UpdateScanProgress(string scanNode, int fileNumber, int fileCount)
                {
                    float percentComplete = (float)fileNumber / fileCount;
                    UpdateProgress(new ProgressPanelUpdateArgs
                    {
                        CancelAction = null,
                        IsMarquee = false,
                        Message = scanNode.Ellipses(65, true),
                        Title = Resources.GOREDIT_TEXT_SCANNING,
                        PercentageComplete = percentComplete
                    });
                }

                bool result = await Task.Run(() => _fileScanner.Scan(node, this, UpdateScanProgress, true, true));

                if (!result)
                {
                    return;
                }

                // Update the node display to ensure that we see the changes.
                if (node is IContentFile contentFile)
                {
                    contentFile.RefreshMetadata();

                    SetupDependencyNodes(contentFile);
                }
                else
                {
                    foreach (IContentFile file in node.Children.Traverse(n => n.Children).OfType<IContentFile>())
                    {
                        file.RefreshMetadata();

                        if (file.Metadata.Dependencies.Count > 0)
                        {
                            SetupDependencyNodes(file);
                        }
                    }
                }
            }
            finally
            {
                HideProgress();
            }
        }

        /// <summary>
        /// Function called when a child node collection is updated.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    IFileExplorerNodeVm added = e.NewItems.OfType<IFileExplorerNodeVm>().First();
                    EnumerateChildren(added);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    IFileExplorerNodeVm removed = e.OldItems.OfType<IFileExplorerNodeVm>().First();
                    RemoveFromCache(removed);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    IFileExplorerNodeVm parentNode = _nodePathLookup.Where(item => item.Value.Children == sender).First().Value;
                    Debug.Assert(parentNode != null, "Cannot locate the parent node!");
                    ClearFromCache(parentNode);
                    break;
            }
        }

        /// <summary>
        /// Function to inject dependencies for the view model.
        /// </summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.</remarks>
        protected override void OnInitialize(FileExplorerParameters injectionParameters)
        {
            _settings = injectionParameters.Settings ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.Settings), nameof(injectionParameters));
            _factory = injectionParameters.ViewModelFactory ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.ViewModelFactory), nameof(injectionParameters));
            _project = injectionParameters.Project ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.Project), nameof(injectionParameters));
            _fileSystemService = injectionParameters.FileSystemService ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.FileSystemService), nameof(injectionParameters));
            _messageService = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.MessageDisplay), nameof(injectionParameters));
            _busyService = injectionParameters.BusyService ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.BusyService), nameof(injectionParameters));
            RootNode = injectionParameters.RootNode ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.RootNode), nameof(injectionParameters));
            _clipboard = injectionParameters.ClipboardService ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.ClipboardService), nameof(injectionParameters));
            _directoryLocator = injectionParameters.DirectoryLocator ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.DirectoryLocator), nameof(injectionParameters));
            _contentPlugins = injectionParameters.ContentPlugins ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.ContentPlugins), nameof(injectionParameters));
            _searchSystem = injectionParameters.FileSearch ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.FileSearch), nameof(injectionParameters));
            _fileScanner = injectionParameters.ViewModelFactory.FileScanService;

            EnumerateChildren(RootNode);
        }

        /// <summary>
        /// Function to return whether or not the item can use the cut functionality for the clipboard.
        /// </summary>
        /// <returns><b>true</b> if the clipboard handler can cut an item, <b>false</b> if not.</returns>
        bool IClipboardHandler.CanCut() => (SelectedNode != null) && (SelectedNode.AllowDelete) && (SearchResults?.Count == 0);

        /// <summary>
        /// Function to return whether or not the item can use the copy functionality for the clipboard.
        /// </summary>
        /// <returns><b>true</b> if the clipboard handler can copy an item, <b>false</b> if not.</returns>
        bool IClipboardHandler.CanCopy() => SelectedNode != null && (SearchResults?.Count == 0);

        /// <summary>
        /// Function to return whether or not the item can use the paste functionality for the clipboard.
        /// </summary>
        /// <returns><b>true</b> if the clipboard handler can paste an item, <b>false</b> if not.</returns>
        bool IClipboardHandler.CanPaste()
        {
            if ((_searchResults != null) || (!_clipboard.IsType<FileSystemClipboardData>()))
            {
                return false;
            }

            IFileExplorerNodeVm targetNode = _selectedNode ?? RootNode;

            if (!targetNode.AllowChildCreation)
            {
                return false;
            }

            FileSystemClipboardData data = _clipboard.GetData<FileSystemClipboardData>();

            if (data.Copy)
            {
                return !string.IsNullOrWhiteSpace(data.FullPath);
            }

#pragma warning disable IDE0046 // Convert to conditional expression
            if (!_nodePathLookup.TryGetValue(data.FullPath, out IFileExplorerNodeVm pasteNode))
            {
                return false;
            }
#pragma warning restore IDE0046 // Convert to conditional expression

            return CheckNodeMovement(pasteNode, targetNode, data.Copy);
        }

        /// <summary>
        /// Function to store an item to copy onto the clipboard for cutting.
        /// </summary>
        void IClipboardHandler.Cut()
        {
            EventHandler handler = _clipboardUpdated;

            try
            {
                _clipboard.CopyItem(new FileSystemClipboardData
                {
                    Copy = false,
                    FullPath = SelectedNode.FullPath
                });

                SelectedNode.IsCut = true;

                handler?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_COPYING_FILE_OR_DIR_CLIPBOARD, SelectedNode.Name));
            }
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

                SelectedNode.IsCut = false;

                handler?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_COPYING_FILE_OR_DIR_CLIPBOARD, SelectedNode.Name));
            }
        }

        /// <summary>
        /// Function to paste an item from the clipboard.
        /// </summary>
        async void IClipboardHandler.Paste()
        {
            EventHandler handler = _clipboardUpdated;

            try
            {
                if (!_clipboard.IsType<FileSystemClipboardData>())
                {
                    return;
                }

                FileSystemClipboardData data = _clipboard.GetData<FileSystemClipboardData>();
                IFileExplorerNodeVm destNode = SelectedNode ?? RootNode;

                if (!_nodePathLookup.TryGetValue(data.FullPath, out IFileExplorerNodeVm sourceNode))
                {
                    _messageService.ShowError(string.Format(Resources.GOREDIT_ERR_NODE_NOT_EXIST, data.FullPath));
                    return;
                }

                if (data.Copy)
                {
                    await CopyNodeAsync(sourceNode, destNode);
                }
                else
                {
                    _busyService.SetBusy();

                    if (MoveNode(sourceNode, destNode))
                    {
                        sourceNode.IsCut = false;
                        _clipboard.Clear();
                        handler?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_PASTE_FILE_OR_DIR);
            }
            finally
            {
                _busyService.SetIdle();
            }
        }

        /// <summary>
        /// Function called when the associated view is unloaded.
        /// </summary>
        public override void OnUnload()
        {
            foreach (IFileExplorerNodeVm node in RootNode.Children.Traverse(n => n.Children))
            {
                node.OnUnload();
            }

            _clipboardUpdated = null;
        }

        /// <summary>
        /// Function to drop the payload for a drag drop operation.
        /// </summary>
        /// <param name="dragData">The drag/drop data.</param>
        /// <param name="afterDrop">[Optional] The method to execute after the drop operation is completed.</param>
        async void IDragDropHandler<IFileExplorerNodeDragData>.Drop(IFileExplorerNodeDragData dragData, Action afterDrop)
        {
            try
            {
                if (dragData.DragOperation == DragOperation.Copy)
                {
                    await CopyNodeAsync(dragData.Node, dragData.TargetNode);
                    afterDrop?.Invoke();
                    return;
                }

                _busyService.SetBusy();
                MoveNode(dragData.Node, dragData.TargetNode);
                afterDrop?.Invoke();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_CANNOT_COPY, dragData.Node.Name, dragData.TargetNode.FullPath));
            }
            finally
            {
                _busyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to determine if an object can be dropped.
        /// </summary>
        /// <param name="dragData">The drag/drop data.</param>
        /// <returns>System.Boolean.</returns>
        bool IDragDropHandler<IFileExplorerNodeDragData>.CanDrop(IFileExplorerNodeDragData dragData)
        {
            try
            {
                return CheckNodeMovement(dragData.Node, dragData.TargetNode, dragData.DragOperation == DragOperation.Copy);
            }
            catch (Exception ex)
            {
                Program.Log.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// Function to drop the payload for a drag drop operation.
        /// </summary>
        /// <param name="dragData">The drag/drop data.</param>
        /// <param name="afterDrop">[Optional] The method to execute after the drop operation is completed.</param>
        async void IDragDropHandler<IExplorerFilesDragData>.Drop(IExplorerFilesDragData dragData, Action afterDrop)
        {
            try
            {
                if ((dragData.DragOperation != DragOperation.Copy)
                    || (dragData.ExplorerPaths == null)
                    || (dragData.ExplorerPaths.Count == 0))
                {
                    return;
                }

                // Get the list of paths, ordered from shortest to longest and formatted without a trailing separator.
                string[] orderedPaths = dragData.ExplorerPaths.OrderBy(item => item.Length)
                    .Select(item => item.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                    .ToArray();

                // Get the common directory name.
                var parentDirectory = new DirectoryInfo(Path.GetDirectoryName(dragData.ExplorerPaths[0]));

                await ImportAsync(dragData.TargetNode, parentDirectory, dragData.ExplorerPaths);
                afterDrop?.Invoke();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_IMPORT, dragData.TargetNode.FullPath));
            }
            finally
            {
                HideProgress();
            }
        }

        /// <summary>
        /// Function to determine if an object can be dropped.
        /// </summary>
        /// <param name="dragData">The drag/drop data.</param>
        /// <returns>System.Boolean.</returns>
        bool IDragDropHandler<IExplorerFilesDragData>.CanDrop(IExplorerFilesDragData dragData)
        {
            try
            {
                return _searchResults != null
                    ? false
                    : !dragData.TargetNode.AllowChildCreation ? false : !dragData.ExplorerPaths.Any(item => item.StartsWith(RootNode.PhysicalPath));
            }
            catch (Exception ex)
            {
                Program.Log.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// Function to run the custom importers over the files in the file system.
        /// </summary>
        public async Task RunImportersAsync(CancellationToken cancelToken)
        {
            IGorgonFileSystem importFileSystem = new GorgonFileSystem(Log);

            // Some things will require that we have the file system available to us for reading while importing.
            importFileSystem.Mount(_project.FileSystemDirectory.FullName.FormatDirectory(Path.DirectorySeparatorChar));

            foreach (IFileExplorerNodeVm node in RootNode.Children.Traverse(n => n.Children).Where(item => item.IsContent))
            {
                // Do not update nodes that are open in the editor.
                if (node.IsOpen)
                {
                    continue;
                }

                var content = node as IContentFile;
                (IEditorContentImporter importer, FileInfo outputFile) importResult = default;
                try
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return;
                    }

                    // This file is already imported, so there's no point in trying to import it again.
                    if (node.Metadata.Attributes.ContainsKey(ContentImportPlugin.ImportOriginalFileNameAttr))
                    {
                        continue;
                    }

                    var sourceFile = new FileInfo(node.PhysicalPath);
                    importResult = await CustomImportFileAsync(new FileInfo(node.PhysicalPath), importFileSystem, cancelToken);

                    if ((importResult.outputFile == null) || (importResult.importer == null))
                    {
                        continue;
                    }

                    string newName = importResult.outputFile.Name;
                    // Rename the node to match whatever we imported.
                    if (node.Parent.Children.Any(item => string.Equals(importResult.outputFile.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        newName = _fileSystemService.GenerateFileName(importResult.outputFile.FullName);
                    }

                    // First, copy the old file into the source directory.
                    string srcPath = Path.Combine(_project.SourceDirectory.FullName, Guid.NewGuid().ToString("N")) + sourceFile.Extension;
                    sourceFile.CopyTo(srcPath, true);

                    node.RenameNode(newName);

                    // Copy over the contents of the old file.
                    importResult.outputFile.CopyTo(node.PhysicalPath, true);

                    // Record the name of the source file.
                    var destFile = new FileInfo(srcPath);
                    node.Metadata.Attributes[ContentImportPlugin.ImportOriginalFileNameAttr] = destFile.Name;

                    // Update the metadata information.
                    content?.RefreshMetadata();
                }
                finally
                {
                    if ((importResult.outputFile != null) && (importResult.importer != null) && (importResult.outputFile.Exists) && (importResult.importer.NeedsCleanup))
                    {
                        importResult.outputFile.Delete();
                    }
                }
            }
        }

        /// <summary>
        /// Function to retrieve a file based on the path specified.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>A <see cref="IContentFile"/> if found, <b>null</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        IContentFile IContentFileManager.GetFile(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentEmptyException(nameof(path));
            }

            return !_nodePathLookup.TryGetValue(path, out IFileExplorerNodeVm node) ? null : node as IContentFile;
        }
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
            DeleteNodeCommand = new EditorCommand<DeleteNodeArgs>(DoDeleteNode, CanDeleteNode);
            RefreshNodeCommand = new EditorCommand<IFileExplorerNodeVm>(DoRefreshSelectedNodeAsync);
            CopyNodeCommand = new EditorCommand<CopyNodeArgs>(DoCopyNode, CanCopyNode);
            ExportNodeToCommand = new EditorCommand<IFileExplorerNodeVm>(DoExportNodeAsync, CanExportNode);
            MoveNodeCommand = new EditorCommand<CopyNodeArgs>(DoMoveNode, CanMoveNode);
            DeleteFileSystemCommand = new EditorCommand<object>(DoDeleteFileSystemAsync, CanDeleteFileSystem);
            ImportIntoNodeCommand = new EditorAsyncCommand<IFileExplorerNodeVm>(DoImportIntoNodeAsync, CanImportIntoNode);
            SearchCommand = new EditorCommand<string>(DoSearch);
            CreateContentFileCommand = new EditorCommand<CreateContentFileArgs>(DoCreateContentFile, CanCreateContentFile);
        }
        #endregion
    }
}
