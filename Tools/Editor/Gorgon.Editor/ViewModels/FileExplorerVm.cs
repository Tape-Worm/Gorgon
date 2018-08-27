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
using System.Threading.Tasks;
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
        : ViewModelBase, IFileExplorerVm
    {
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
        public IEditorCommand<object> DeleteNodeCommand
        {
            get;
        }
        #endregion

        #region Methods.
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
            try
            {
                bool hasChildren = SelectedNode.Children.Count != 0;
                string message = hasChildren ? string.Format(Resources.GOREDIT_CONFIRM_DELETE_CHILDREN, SelectedNode.FullPath)
                                                : string.Format(Resources.GOREDIT_CONFIRM_DELETE_NO_CHILDREN, SelectedNode.FullPath);

                if (_messageService.ShowConfirmation(message) == MessageResponse.No)
                {
                    return;
                }

                ShowWaitPanel(string.Format(Resources.GOREDIT_TEXT_DELETING, SelectedNode.FullPath));

                await SelectedNode.DeleteNodeAsync(_fileSystemService);

                // Update metadata.
                IncludedFileSystemPathMetadata[] included = _project.Metadata.IncludedPaths
                                                                            .Where(item => item.Path.StartsWith(SelectedNode.FullPath, StringComparison.OrdinalIgnoreCase))
                                                                            .ToArray();

                for (int i = 0; i < included.Length; ++i)
                {
                    _project.Metadata.IncludedPaths.Remove(included[i]);
                }                
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex, string.Format(Resources.GOREDIT_ERR_DELETE, SelectedNode.FullPath));
            }
            finally
            {
                HideWaitPanel();
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
                DirectoryInfo newDir = _fileSystemService.CreateDirectory(node.FullPath);

                // Update the metadata 
                var metaData = new IncludedFileSystemPathMetadata(_project.ProjectWorkSpace, newDir);
                _project.Metadata.IncludedPaths[metaData.Path] = metaData;

                // Create the node for the directory.
                IFileExplorerNodeVm newNode = _factory.CreateFileExplorerDirectoryNodeVm(_project, node, newDir);

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

                SelectedNode.RenameNode(_fileSystemService, args.NewName);
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
        /// Function to initialize the view model.
        /// </summary>
        /// <param name="factory">The factory used to build view models.</param>
        /// <param name="project">The project data.</param>
        /// <param name="fileSystemService">The file system service for the project.</param>
        /// <param name="rootNode">The root node for this file system.</param>
        /// <param name="messageService">The message display service to use.</param>
        /// <param name="busyService">The busy state service to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        public void Initialize(ViewModelFactory factory, IProject project, IFileSystemService fileSystemService, IFileExplorerNodeVm rootNode, IMessageDisplayService messageService, IBusyStateService busyService)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _project = project ?? throw new ArgumentNullException(nameof(project));
            _fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _busyService = busyService ?? throw new ArgumentNullException(nameof(busyService));
            RootNode = rootNode ?? throw new ArgumentNullException(nameof(rootNode));
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
        }
        #endregion
    }
}
