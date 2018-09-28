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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Editor.Data;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.IO;

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
        // The manager used to handle project metadata.
        private IMetadataManager _metaDataManager;
        // The clipboard service to use.
        private IClipboardService _clipboard;
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

        /// <summary>
        /// Property to return the command used to copy a single node.
        /// </summary>
        public IEditorCommand<CopyNodeArgs> CopyNodeCommand
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
        /// Function to remove the specified node, and any children from the cache.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        /// <param name="removeSelf"><b>true</b> to remove the specified node along with its children, or <b>false</b> to only remove children.</param>
        private void RemoveFromCache(IFileExplorerNodeVm node, bool removeSelf)
        {
            IFileExplorerNodeVm[] children = node.Children.Traverse(n => n.Children)
                                                          .ToArray();

            foreach (IFileExplorerNodeVm child in children)
            {
                child.Children.CollectionChanged -= Children_CollectionChanged;
                _nodePathLookup.Remove(child.FullPath);
            }

            if ((node == RootNode) || (!removeSelf))
            {
                return;
            }

            node.Children.CollectionChanged -= Children_CollectionChanged;
            _nodePathLookup.Remove(node.FullPath);
        }

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
                    if (_metaDataManager.PathInProject(SelectedNode.FullPath))
                    {
                        _metaDataManager.DeleteFromIncludeMetadata(SelectedNode.FullPath);
                        OnFileSystemChanged();
                    }

                    await SelectedNode.DeleteNodeAsync();
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

                await SelectedNode.DeleteNodeAsync(UpdateDeleteProgress, cancelSource.Token);
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
                IFileExplorerNodeVm newNode = _factory.CreateFileExplorerDirectoryNodeVm(_project, _fileSystemService, node, _metaDataManager, newDir);

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
                foreach (IFileExplorerNodeVm child in node.Children.Traverse(n => n.Children))
                {
                    child.Included = include;
                    paths.Add(child.FullPath);
                }
                
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
            IFileExplorerNodeVm selected = SelectedNode;

            _busyService.SetBusy();
            
            try
            {
                // We need to update the cache so that all paths are changed for children under the selected nodes.
                RemoveFromCache(selected, true);

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
                
                SelectedNode.RenameNode(args.NewName);
                
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
                EnumerateChildren(selected);
                _busyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to perform a refresh of a node.
        /// </summary>
        /// <param name="node">The node to refresh.</param>
        private void RefreshNode(IFileExplorerNodeVm node)
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
                nodeToRefresh.Children.Add(_factory.CreateFileExplorerDirectoryNodeVm(_project, _fileSystemService, nodeToRefresh, _metaDataManager, rootDir));
            }

            foreach (FileInfo file in _metaDataManager.GetIncludedFiles(parent.FullName))
            {
                nodeToRefresh.Children.Add(_factory.CreateFileExplorerFileNodeVm(_project, _fileSystemService, nodeToRefresh, file));
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
                RefreshNode(node);
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
        private bool CanMoveNode(CopyNodeArgs args)
        {
            if ((args.Source == args.Dest) || (string.Equals(args.Source.FullPath, args.Dest.FullPath, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            return !args.Source.IsAncestorOf(args.Dest);
        }

        /// <summary>
        /// Function to move a node.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        private void DoMoveNode(CopyNodeArgs args)
        {
            _busyService.SetBusy();

            try
            {
                MoveSingleNode(args.Source, args.Dest);


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
        /// Function to copy a node.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        private async void DoCopyNode(CopyNodeArgs args)
        {
            try
            {
                await CopySingleNodeAsync(args.Source, args.Dest);
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, Resources.GOREDIT_ERR_REFRESH);
            }
        }

        /// <summary>
        /// Function to remap a node to the a node path.
        /// </summary>
        /// <param name="source">The source node to remap.</param>
        /// <param name="dest">The base node used to replace.</param>
        /// <returns>The remapped path.</returns>
        private string RemapNodePath(IFileExplorerNodeVm source, IFileExplorerNodeVm dest)
        {
            if (string.Equals(source.FullPath, dest.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                return source.FullPath;
            }

            if (source.Parent == null)
            {
                return "/";
            }

            return Path.Combine(dest.FullPath, source.FullPath.Substring(source.Parent.FullPath.Length));
        }

        /// <summary>
        /// Function to copy a node to another node.
        /// </summary>
        /// <param name="source">The source node to copy.</param>
        /// <param name="dest">The destination that will receive the copy.</param>
        /// <returns><b>true</b> if the node was moved, <b>false</b> if not.</returns>
        private bool MoveSingleNode(IFileExplorerNodeVm source, IFileExplorerNodeVm dest)
        {
            IFileExplorerNodeVm newNode = null;

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

            newNode = source.MoveNode(dest);

            if (newNode == null)
            {
                return false;
            }

            var includedPaths = new List<string>();
            var excludedPaths = new List<string>();

            if ((newNode.Included) && (!_metaDataManager.PathInProject(newNode.FullPath)))
            {
                includedPaths.Add(newNode.FullPath);
                excludedPaths.Add(source.FullPath);                
            }

            // Because the move operation is a single operation with no callback, we have to regenerate the paths to associate them 
            // with our new new node's children. 
            if ((source.Children.Count > 0) && (newNode.AllowChildCreation))
            {
                includedPaths.AddRange(source.Children.Traverse(p => p.Children)
                                                      .Where(item => item.Included)
                                                      .Select(item => RemapNodePath(item, newNode))
                                                      .Where(item => !_metaDataManager.PathInProject(item)));
                excludedPaths.AddRange(source.Children.Traverse(p => p.Children)
                                                      .Where(item => item.Included)
                                                      .Select(item => item.FullPath));
            }

            if ((includedPaths.Count > 0) || (excludedPaths.Count > 0))
            {
                _metaDataManager.ExcludePaths(excludedPaths);
                _metaDataManager.IncludePaths(includedPaths);
                OnFileSystemChanged();
            }

            // Refresh the new node so we can capture the child nodes.
            if (source.Children.Count > 0)
            {
                RefreshNode(newNode);
            }

            SelectedNode = dest;

            // This will refresh the nodes under the destination.
            if (!dest.IsExpanded)
            {
                newNode.Parent.IsExpanded = true;
            }

            // If this node is not registered, then add it now.
            if (!_nodePathLookup.TryGetValue(newNode.FullPath, out IFileExplorerNodeVm prevNode))
            {
                dest.Children.Add(newNode);
                prevNode = newNode;
            }
            
            // Delete this node now that we've got our item moved.
            source.Parent.Children.Remove(source);

            SelectedNode = prevNode;

            return true;
        }

        /// <summary>
        /// Function to copy a node to another node.
        /// </summary>
        /// <param name="source">The source node to copy.</param>
        /// <param name="dest">The destination that will receive the copy.</param>
        /// <returns>A task for async operation.</returns>
        private async Task CopySingleNodeAsync(IFileExplorerNodeVm source, IFileExplorerNodeVm dest)
        {
            var cancelSource = new CancellationTokenSource();

            try
            {   
                IFileExplorerNodeVm newNode = null;

                // Locate the next level that allows child creation if this one cannot.
                while (!dest.AllowChildCreation)
                {
                    dest = dest.Parent;

                    Debug.Assert(dest != null, "No container node found.");
                }

                var includedDestItems = new HashSet<string>();

                void CancelAction() => cancelSource.Cancel();

                UpdateProgress(new ProgressPanelUpdateArgs
                {
                    CancelAction = CancelAction,
                    Title = Resources.GOREDIT_TEXT_COPYING,
                    PercentageComplete = 0,
                    Message = string.Empty
                });

                // Function to update our progress meter and provide updates to our caching (and inclusion functionality).
                void UpdateCopyProgress(FileSystemInfo sourceItem, FileSystemInfo destItem, int currentItemNumber, int totalItemNumber)
                {
                    string sourceItemPath = sourceItem.ToFileSystemPath(_project.ProjectWorkSpace);
                    string destItemPath = destItem.ToFileSystemPath(_project.ProjectWorkSpace);

                    IFileExplorerNodeVm sourceNode = _nodePathLookup[sourceItemPath];
                    if ((sourceNode.Included) && (!_metaDataManager.PathInProject(destItemPath)))
                    {
                        includedDestItems.Add(destItemPath);
                    }

                    float percent = currentItemNumber / (float)totalItemNumber;
                    UpdateProgress(new ProgressPanelUpdateArgs
                    {
                        Title = Resources.GOREDIT_TEXT_COPYING,
                        CancelAction = CancelAction,
                        Message = sourceItemPath.Ellipses(65, true),
                        PercentageComplete = percent
                    });
                }

                newNode = await source.CopyNodeAsync(dest, UpdateCopyProgress, cancelSource.Token);

                if (newNode == null)
                {                    
                    return;
                }

                if ((newNode.Included) && (!_metaDataManager.PathInProject(newNode.FullPath)))
                {
                    includedDestItems.Add(newNode.FullPath);
                }

                if (includedDestItems.Count > 0)
                {
                    _metaDataManager.IncludePaths(includedDestItems.ToArray());
                    OnFileSystemChanged();
                }

                // If the source has children, then rescan our new node to pick up any children that were copied.
                if (source.Children.Count > 0)
                {
                    RefreshNode(newNode);
                }

                SelectedNode = dest;

                // This will refresh the nodes under the destination.
                if ((!dest.IsExpanded) && (dest.Children.Count > 0))
                {
                    dest.IsExpanded = true;
                }

                // If this node is not registered, then add it now.
                if (!_nodePathLookup.TryGetValue(newNode.FullPath, out IFileExplorerNodeVm prevNode))
                {
                    dest.Children.Add(newNode);
                    prevNode = newNode;
                }

                SelectedNode = prevNode;
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
            }

            parent.Children.CollectionChanged += Children_CollectionChanged;
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
                    RemoveFromCache(removed, true);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    IFileExplorerNodeVm parentNode = _nodePathLookup.Where(item => item.Value.Children == sender).First().Value;
                    Debug.Assert(parentNode != null, "Cannot locate the parent node!");
                    RemoveFromCache(parentNode, false);
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
            _factory = injectionParameters.ViewModelFactory ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.ViewModelFactory), nameof(injectionParameters));
            _project = injectionParameters.Project ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.Project), nameof(injectionParameters));
            _metaDataManager = injectionParameters.MetadataManager ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.MetadataManager), nameof(injectionParameters));
            _fileSystemService = injectionParameters.FileSystemService ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.FileSystemService), nameof(injectionParameters));
            _messageService = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.MessageDisplay), nameof(injectionParameters));
            _busyService = injectionParameters.BusyState ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.BusyState), nameof(injectionParameters));
            RootNode = injectionParameters.RootNode ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.RootNode), nameof(injectionParameters));
            _clipboard = injectionParameters.ClipboardService ?? throw new ArgumentMissingException(nameof(FileExplorerParameters.ClipboardService), nameof(injectionParameters));

            EnumerateChildren(RootNode);
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
            if (!_clipboard.IsType<FileSystemClipboardData>())
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
                    await CopySingleNodeAsync(sourceNode, destNode);
                }
                else
                {
                    _busyService.SetBusy();

                    if (MoveSingleNode(sourceNode, destNode))
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
        async void IDragDropHandler<IFileExplorerNodeDragData>.Drop(IFileExplorerNodeDragData dragData)
        {
            try
            {
                if (dragData.DragOperation == DragOperation.Copy)
                {
                    await CopySingleNodeAsync(dragData.Node, dragData.TargetNode);
                    return;
                }

                _busyService.SetBusy();
                MoveSingleNode(dragData.Node, dragData.TargetNode);
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
                // We cannot drop on ourselves, if we're in move mode.
                if (!dragData.TargetNode.AllowChildCreation)
                {
                    return false;
                }

                if (dragData.DragOperation == DragOperation.Copy)
                {
                    return true;
                }

                return (dragData.TargetNode != dragData.Node)
                    && (dragData.Node.Parent != dragData.TargetNode)
                    && (!dragData.TargetNode.Children.Contains(dragData.Node))
                    && (!dragData.TargetNode.IsAncestorOf(dragData.Node));
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
            DeleteNodeCommand = new EditorCommand<object>(DoDeleteNode, CanDeleteNode);
            IncludeExcludeCommand = new EditorCommand<bool>(DoIncludeExcludeNode);
            RefreshNodeCommand = new EditorCommand<IFileExplorerNodeVm>(DoRefreshSelectedNode);
            CopyNodeCommand = new EditorCommand<CopyNodeArgs>(DoCopyNode, CanCopyNode);
            MoveNodeCommand = new EditorCommand<CopyNodeArgs>(DoMoveNode, CanMoveNode);
        }
        #endregion
    }
}
