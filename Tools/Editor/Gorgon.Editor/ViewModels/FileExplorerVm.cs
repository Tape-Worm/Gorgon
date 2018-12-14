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
using Gorgon.Editor.Content;
using Gorgon.Editor.Data;
using Gorgon.Editor.Metadata;
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
        /// Property to return the command to execute when including or excluding all nodes.
        /// </summary>
        public IEditorCommand<IncludeExcludeArgs> IncludeExcludeAllCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when including or excluding a node.
        /// </summary>
        public IEditorCommand<IncludeExcludeArgs> IncludeExcludeCommand
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
        public IEditorCommand<IContentFile> OpenContentFile
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether to show excluded nodes or not.
        /// </summary>
        public bool ShowExcluded
        {
            get => _project.ShowExternalItems;
            set
            {
                if (_project.ShowExternalItems == value)
                {
                    return;
                }

                OnPropertyChanging();
                _project.ShowExternalItems = value;
                OnPropertyChanged();
            }
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

                string message = hasChildren ? string.Format(Resources.GOREDIT_CONFIRM_DELETE_CHILDREN, args.Node.FullPath)
                                                : string.Format(Resources.GOREDIT_CONFIRM_DELETE_NO_CHILDREN, args.Node.FullPath);

                // If we have an open node, and it has unsaved changes, prompt with:
                //
                // "There is a file open in the editor that has unsaved changes.
                // Deleting this file will result in the loss of these changes.
                // 
                // Are you sure you wish to delete this file?
                //
                // Yes/No"
                if (args.Node.IsOpen)
                {
                    // TODO: Update the message string.  We'll let the prompt below handle everything.  Better than having multiple prompts.
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

                    // Remove this item from the metadata.
                    itemsDeleted = true;
                    return;
                }                               

                // Function to update the delete progress information and handle metadata update.
                void UpdateDeleteProgress(FileSystemInfo fileSystemItem)
                {
                    string path = fileSystemItem.ToFileSystemPath(_project.ProjectWorkSpace);
                    UpdateMarequeeProgress($"{path}", Resources.GOREDIT_TEXT_DELETING, cancelSource.Cancel);

                    // Remove this item from the metadata.
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
        /// Function to determine if all nodes can be included/excluded.
        /// </summary>
        /// <param name="include">Not used.</param>
        /// <returns><b>true</b> if all nodes can be included or excluded, <b>false</b> if not.</returns>
        private bool CanIncludeExcludeAll(IncludeExcludeArgs args) => (RootNode.Children.Count != 0);

        /// <summary>
        /// Function to update the metadata state for a node.
        /// </summary>
        /// <param name="node">The node to update.</param>
        /// <param name="metaData">The metadata to assign or remove.</param>
        private void UpdateMetadataForNode(IFileExplorerNodeVm node, ProjectItemMetadata metaData)
        {
            node.Metadata = metaData;

            if ((node.Metadata != null) && (node.IsContent))
            {
                if (node.AssignContentPlugin(_contentPlugins, true))
                {
                    OnFileSystemChanged();
                }
            }
            else if (node.Metadata?.ContentMetadata != null)
            {
                node.Metadata.ContentMetadata = null;
                OnFileSystemChanged();
            }
        }
                
        /// <summary>
        /// Function to include or exclude all nodes in the project.
        /// </summary>
        /// <param name="args"><b>true</b> to include, <b>false</b> to exclude.</param>
        private void DoIncludeExcludeAll(IncludeExcludeArgs args)
        {
            _busyService.SetBusy();
            try
            {
                if (!args.Include)
                {
                    IFileExplorerNodeVm openContent = RootNode.Children.Traverse(n => n.Children).FirstOrDefault(item => item.IsOpen);
                    // TODO: Ask for permission to exclude if we have changes.
                    if ((openContent != null) && (openContent.IsChanged))
                    {
                        args.Cancel = true;
                        return;
                    }
                }

                // Add child nodes.
                foreach (IFileExplorerNodeVm child in RootNode.Children.Traverse(n => n.Children))
                {
                    UpdateMetadataForNode(child, args.Include ? new ProjectItemMetadata() : null);
                }

                OnFileSystemChanged();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_INCLUDE_ALL);
                args.Cancel = true;
            }
            finally
            {
                _busyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to include or exclude a node from the project.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        private void DoIncludeExcludeNode(IncludeExcludeArgs args)
        {
            _busyService.SetBusy();
            try
            {
                if ((!args.Include) && (((args.Node.IsOpen) && (args.Node.IsChanged)) 
                                        || (args.Node.Children.Any(item => (item.IsOpen) && (item.IsChanged)))))
                {
                    // TODO: Ask for permission to exclude if we have changes.
                    args.Cancel = true;
                    return;
                }

                UpdateMetadataForNode(args.Node, args.Include ? new ProjectItemMetadata() : null);

                // If our parent node is not included, and we've included this node, then we'll include it now.
                // If we exclude the node, we'll leave the parent included. This is the behavior in Visual Studio, so we'll mimic that here.
                if ((args.Node.Parent != null) && (args.Node.Parent.Metadata == null) && (args.Include))
                {
                    UpdateMetadataForNode(args.Node.Parent, args.Include ? new ProjectItemMetadata() : null);
                }

                // Add child nodes.
                foreach (IFileExplorerNodeVm child in args.Node.Children.Traverse(n => n.Children))
                {
                    UpdateMetadataForNode(child, args.Include ? new ProjectItemMetadata() : null);
                }

                OnFileSystemChanged();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_INCLUDE_NODE, args.Node.Name));
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

            var parent = new DirectoryInfo(nodeToRefresh == RootNode ? _project.ProjectWorkSpace.FullName : nodeToRefresh.PhysicalPath);

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

                if (child.Metadata != null)
                {
                    child.AssignContentPlugin(_contentPlugins, deepScanForAssociation);
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
                RefreshNode(node, false);
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
        private bool CanCopyNode(CopyNodeArgs args) => args?.Source != null;

        /// <summary>
        /// Function to determine if a node can be moved.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        /// <returns><b>true</b> if the node can be moved, <b>false</b> if not.</returns>
        private bool CanMoveNode(CopyNodeArgs args) => (args.Source == args.Dest) || (string.Equals(args.Source.FullPath, args.Dest.FullPath, StringComparison.OrdinalIgnoreCase))
                ? false
                : !args.Dest.IsAncestorOf(args.Source);

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
            MessageResponse response = _messageService.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_FILE_EXISTS, sourceItem.Name, destItem.ToFileSystemPath(_project.ProjectWorkSpace)), toAll: true, allowCancel: true);

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
        /// Function to perform the import of the directories/files into the specified node.
        /// </summary>
        /// <param name="node">The node that will recieve the import.</param>
        /// <param name="items">The paths to the items to import.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private async Task ImportAsync(IFileExplorerNodeVm node, IReadOnlyList<string> items)
        {
            var cancelSource = new CancellationTokenSource();
            var metadata = new Dictionary<string, ProjectItemMetadata>();

            try
            {
                void cancelAction() => cancelSource.Cancel();

                UpdateProgress(new ProgressPanelUpdateArgs
                {
                    CancelAction = cancelAction,
                    Title = Resources.GOREDIT_TEXT_COPYING,
                    PercentageComplete = 0,
                    Message = string.Empty
                });

                // Function to update our progress meter.
                void UpdateCopyProgress(FileSystemInfo sourceItem, FileSystemInfo destItem, int currentItemNumber, int totalItemNumber)
                {
                    float percent = currentItemNumber / (float)totalItemNumber;
                    UpdateProgress(new ProgressPanelUpdateArgs
                    {
                        Title = Resources.GOREDIT_TEXT_COPYING,
                        CancelAction = cancelAction,
                        Message = sourceItem.FullName.Ellipses(65, true),
                        PercentageComplete = percent
                    });
                }

                var importArgs = new ImportArgs(items, node.PhysicalPath)
                {
                    ConflictResolver = ResolveImportConflict,
                    OnImportFile = UpdateCopyProgress,
                };

                await _fileSystemService.ImportIntoDirectoryAsync(importArgs, cancelSource.Token);

                if (cancelSource.Token.IsCancellationRequested)
                {
                    return;
                }

                HideProgress();

                UpdateMarequeeProgress(Resources.GOREDIT_TEXT_SCANNING);                

                if (node.Metadata == null)
                {
                    UpdateMetadataForNode(node, new ProjectItemMetadata());
                }

                await Task.Run(() =>
                {
                    // Retrieve the nodes generated by the import.  None of these will have metadata, so we don't need to scan the items yet.
                    if (node != RootNode)
                    {
                        RefreshNode(node, false);
                    }
                    else
                    {
                        RefreshNode(null, false);
                    }

                    if (cancelSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    
                    foreach (IFileExplorerNodeVm child in node.Children.Traverse(n => n.Children))
                    {
                        if (cancelSource.Token.IsCancellationRequested)
                        {
                            return;
                        }

                        child.Metadata = new ProjectItemMetadata();

                        if (child.IsContent)
                        {
                            UpdateMarequeeProgress(child.Name.Ellipses(65, true), Resources.GOREDIT_TEXT_SCANNING);
                            child.AssignContentPlugin(_contentPlugins, true);
                        }
                    }
                }, cancelSource.Token);
            }
            finally
            {
                cancelSource.Dispose();                
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

                await ImportAsync(node, sourceDir.GetFileSystemInfos().Select(item => item.FullName).ToArray());

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
                void UpdateCopyProgress(FileSystemInfo sourceItem, FileSystemInfo destItem, int currentItemNumber, int totalItemNumber)
                {
                    float percent = currentItemNumber / (float)totalItemNumber;

                    if ((destItem == null) || (destItem == sourceItem))
                    {
                        UpdateMarequeeProgress(sourceItem.ToFileSystemPath(_project.ProjectWorkSpace).Ellipses(65, true), Resources.GOREDIT_TEXT_COPYING);
                    }
                    else
                    {
                        UpdateProgress(new ProgressPanelUpdateArgs
                        {
                            Title = Resources.GOREDIT_TEXT_COPYING,
                            CancelAction = cancelAction,
                            Message = sourceItem.ToFileSystemPath(_project.ProjectWorkSpace).Ellipses(65, true),
                            PercentageComplete = percent
                        });
                    }
                }

                await node.ExportAsync(destDir.FullName, UpdateCopyProgress, cancelSource.Token);

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
                _searchResults = _searchSystem.Search(searchText)?.ToList();                
                NotifyPropertyChanged(nameof(SearchResults));

                if ((selected != null) && ((_searchResults == null) || (_searchResults.Contains(selected))))
                {
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

            string originalPath = source.FullPath;
            (string path, IFileExplorerNodeVm node)[] originalChildPaths = null;

            if ((source.AllowChildCreation) && (source.Children.Count > 0))
            {
                originalChildPaths = source.Children.Traverse(n => n.Children)
                                                    .Select(item => (item.FullPath, item))
                                                    .ToArray();
            }

            // If we cancelled, or there was an error, then leave.
            if (!source.MoveNode(dest))
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

            SelectedNode = dest;

            // This will refresh the nodes under the destination.
            dest.IsExpanded = true;

            // If this node is not registered, then add it now.
            _nodePathLookup[source.FullPath] = source;

            SelectedNode = source;

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

                if (fileNode.Children.Count > 0)
                {
                    foreach (IFileExplorerNodeVm child in fileNode.Children.Traverse(n => n.Children))
                    {
                        child.AssignContentPlugin(_contentPlugins);
                    }
                }

                fileNode.AssignContentPlugin(_contentPlugins);

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

                if (node.Metadata != null)
                {
                    node.AssignContentPlugin(_contentPlugins);
                }
            }

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

            EnumerateChildren(RootNode);

            // Scan for associations.  If this file was upgraded, then we'll need to add new associations.
            foreach (IFileExplorerNodeVm node in RootNode.Children.Traverse(n => n.Children))
            {
                node.AssignContentPlugin(_contentPlugins, true);
            }            
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
            if ((SearchResults != null) || (!_clipboard.IsType<FileSystemClipboardData>()))
            {
                return false;
            }

            FileSystemClipboardData data = _clipboard.GetData<FileSystemClipboardData>();

            return !string.IsNullOrWhiteSpace(data.FullPath);
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
        public override void OnUnload() => _clipboardUpdated = null;

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
                if (_searchResults != null)
                {
                    return false;
                }

                // We cannot drop on ourselves, if we're in move mode.
                return !dragData.TargetNode.AllowChildCreation
                    ? false
                    : dragData.DragOperation == DragOperation.Copy
                    ? true
                    : (dragData.TargetNode != dragData.Node)
                     || ((dragData.Node.Parent != dragData.TargetNode)
                            && (!dragData.TargetNode.Children.Contains(dragData.Node))
                            && (!dragData.TargetNode.IsAncestorOf(dragData.Node)));
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

                await ImportAsync(dragData.TargetNode, dragData.ExplorerPaths);
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
            IncludeExcludeCommand = new EditorCommand<IncludeExcludeArgs>(DoIncludeExcludeNode, _ => SelectedNode != null);
            RefreshNodeCommand = new EditorCommand<IFileExplorerNodeVm>(DoRefreshSelectedNode);
            CopyNodeCommand = new EditorCommand<CopyNodeArgs>(DoCopyNode, CanCopyNode);
            ExportNodeToCommand = new EditorCommand<IFileExplorerNodeVm>(DoExportNodeAsync, CanExportNode);
            MoveNodeCommand = new EditorCommand<CopyNodeArgs>(DoMoveNode, CanMoveNode);
            IncludeExcludeAllCommand = new EditorCommand<IncludeExcludeArgs>(DoIncludeExcludeAll, CanIncludeExcludeAll);
            DeleteFileSystemCommand = new EditorCommand<object>(DoDeleteFileSystemAsync, CanDeleteFileSystem);
            ImportIntoNodeCommand = new EditorAsyncCommand<IFileExplorerNodeVm>(DoImportIntoNodeAsync, CanImportIntoNode);
            SearchCommand = new EditorCommand<string>(DoSearch);
        }
        #endregion
    }
}
