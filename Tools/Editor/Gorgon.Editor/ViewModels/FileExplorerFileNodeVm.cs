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
// Created: September 4, 2018 10:48:10 PM
// 
#endregion

using System;
using System.IO;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Editor.Services;
using System.Threading;
using Gorgon.Editor.Properties;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// A node for a file system file.
    /// </summary>
    internal class FileExplorerFileNodeVm
        : FileExplorerNodeCommon
    {
        #region Properties.
        /// <summary>
        /// Property to return whether to allow child node creation for this node.
        /// </summary>
        public override bool AllowChildCreation => false;

        /// <summary>
        /// Property to return the type of node.
        /// </summary>
        public override NodeType NodeType => NodeType.File;

        /// <summary>
        /// Property to return the full path to the node.
        /// </summary>
        public override string FullPath => (Parent == null ? "/" : Parent.FullPath) + Name;

        /// <summary>
        /// Property to return the image name to use for the node type.
        /// </summary>
        public override string ImageName => "generic_file_20x20.png";

        /// <summary>
        /// Property to return whether or not the allow this node to be deleted.
        /// </summary>
        public override bool AllowDelete => true;
        #endregion

        #region Methods.
        /// <summary>
        /// Function called when there is a conflict when copying or moving files.
        /// </summary>
        /// <param name="sourceItem">The file being copied/moved.</param>
        /// <param name="destItem">The destination file that is conflicting.</param>
        /// <param name="allowCancel"><b>true</b> to allow cancel support, or <b>false</b> to only use yes/no prompts.</param>
        /// <returns>A resolution for the conflict.</returns>
        private FileSystemConflictResolution FileSystemConflictHandler(string sourceItem, string destItem, bool allowCancel)
        {
            bool isBusy = BusyService.IsBusy;

            // Reset the busy state.  The dialog will disrupt it anyway.
            BusyService.SetIdle();

            MessageResponse response = MessageDisplay.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_FILE_EXISTS,
                                                                                    sourceItem, destItem), allowCancel: allowCancel);

            try
            {
                if (response == MessageResponse.Cancel)
                {
                    return FileSystemConflictResolution.Cancel;
                }

                return response == MessageResponse.Yes ? FileSystemConflictResolution.Overwrite : FileSystemConflictResolution.Rename;
            }
            finally
            {
                // Restore the busy state if we originally had it active.
                if (isBusy)
                {
                    BusyService.SetBusy();
                }
            }
        }

        /// <summary>
        /// Function to rename the node.
        /// </summary>
        /// <param name="fileSystemService">The file system service to use when renaming.</param>
        /// <param name="newName">The new name for the node.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystemService"/>, or the <paramref name="newName"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="newName"/> parameter is empty.</exception>
        public override void RenameNode(IFileSystemService fileSystemService, string newName)
        {
            if (fileSystemService == null)
            {
                throw new ArgumentNullException(nameof(newName));
            }

            if (newName == null)
            {
                throw new ArgumentNullException(nameof(newName));
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentEmptyException(nameof(newName));
            }

            PhysicalPath = fileSystemService.RenameFile(PhysicalPath, newName);
            Name = newName;
            NotifyPropertyChanged(nameof(FullPath));
        }

        /// <summary>
        /// Function to delete the node.
        /// </summary>
        /// <param name="fileSystemService">The file system service to use when deleting.</param>
        /// <param name="onDeleted">[Optional] A function to call when a node or a child node is deleted.</param>
        /// <param name="cancelToken">[Optional] A cancellation token used to cancel the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystemService" /> parameter is <b>null</b>.</exception>        
        /// <remarks>
        /// <para>
        /// The <paramref name="onDeleted"/> parameter is not used for this type.
        /// </para>
        /// </remarks>
        public override Task DeleteNodeAsync(IFileSystemService fileSystemService, Action<FileSystemInfo> onDeleted = null, CancellationToken? cancelToken = null)
        {
            if (fileSystemService == null)
            {
                throw new ArgumentNullException(nameof(fileSystemService));
            }

            // There's no need to make this asynchronous, nor have a status display associated with it.  
            var tcs = new TaskCompletionSource<object>();

            // Delete the physical object first. If we fail here, our node will survive.
            fileSystemService.DeleteFile(PhysicalPath);

            // Drop us from the parent list.
            // This will begin a chain reaction that will remove us from the UI.
            Parent.Children.Remove(this);
            Parent = null;

            return tcs.Task;
        }

        /// <summary>
        /// Function to copy this node to another node.
        /// </summary>
        /// <param name="fileSystemService">The file system service to use when copying.</param>
        /// <param name="newPath">The node that will receive the the copy of this node.</param>
        /// <returns>The new node for the copied node.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystemService"/>, or the <paramref name="destNode"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="destNode"/> is unable to create child nodes.</exception>
        public override IFileExplorerNodeVm CopyNode(IFileSystemService fileSystemService, IFileExplorerNodeVm destNode)
        {
            if (fileSystemService == null)
            {
                throw new ArgumentNullException(nameof(fileSystemService));
            }

            if (destNode == null)
            {
                throw new ArgumentNullException(nameof(destNode));
            }

            if (destNode == null)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GOREDIT_ERR_NODE_CANNOT_CREATE_CHILDREN, destNode.Name));
            }

            FileSystemConflictResolution resolution = FileSystemConflictResolution.Exception;
            string newPath = Path.Combine(destNode.PhysicalPath, Name);

            var result = new FileExplorerFileNodeVm(this)
            {                
                IsExpanded = false,
                Name = Name,
                Parent = destNode,
                PhysicalPath = newPath,
                Included = Included
            };

            // Renames a node that is in conflict when the file is the same in the source and dest, or if the user chooses to not overwrite.
            void RenameNodeInConflict()
            {
                string newName = fileSystemService.GenerateFileName(result.PhysicalPath);
                newPath = Path.Combine(destNode.PhysicalPath, newName);
                result.Name = newName;
                result.PhysicalPath = newPath;
            }

            // If we're trying to copy over ourselves (which is not possible obviously), then just auto-rename.
            if (string.Equals(FullPath, result.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                RenameNodeInConflict();
            }

            if (fileSystemService.FileExists(newPath))
            {
                resolution = FileSystemConflictHandler(FullPath, result.FullPath, true);

                switch (resolution)
                {
                    case FileSystemConflictResolution.Rename:
                    case FileSystemConflictResolution.RenameAll:
                        RenameNodeInConflict();
                        break;
                    case FileSystemConflictResolution.Cancel:
                        return null;
                    case FileSystemConflictResolution.Exception:
                        throw new IOException(string.Format(Resources.GOREDIT_ERR_NODE_EXISTS, Name));
                }
            }

            fileSystemService.CopyFile(PhysicalPath, result.PhysicalPath);

            return result;
        }

        /// <summary>
        /// Function to move this node to another node.
        /// </summary>
        /// <param name="fileSystemService">The file system service to use when copying.</param>
        /// <param name="newPath">The node that will receive the the copy of this node.</param>
        /// <returns>The new node for the copied node.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileSystemService"/>, or the <paramref name="destNode"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="destNode"/> is unable to create child nodes.</exception>
        public override IFileExplorerNodeVm MoveNode(IFileSystemService fileSystemService, IFileExplorerNodeVm destNode)
        {
            if (fileSystemService == null)
            {
                throw new ArgumentNullException(nameof(fileSystemService));
            }

            if (destNode == null)
            {
                throw new ArgumentNullException(nameof(destNode));
            }

            if (destNode == null)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GOREDIT_ERR_NODE_CANNOT_CREATE_CHILDREN, destNode.Name));
            }

            if (string.Equals(Parent.FullPath, destNode.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            FileSystemConflictResolution resolution = FileSystemConflictResolution.Exception;
            string newPath = Path.Combine(destNode.PhysicalPath, Name);

            var result = new FileExplorerFileNodeVm(this)
            {
                IsExpanded = false,
                Name = Name,
                Parent = destNode,
                PhysicalPath = newPath,
                Included = Included
            };

            if (fileSystemService.FileExists(newPath))
            {
                resolution = FileSystemConflictHandler(FullPath, result.FullPath, false);

                switch (resolution)
                {
                    case FileSystemConflictResolution.Rename:
                    case FileSystemConflictResolution.RenameAll:
                    case FileSystemConflictResolution.Cancel:
                        return null;
                    case FileSystemConflictResolution.Exception:
                        throw new IOException(string.Format(Resources.GOREDIT_ERR_NODE_EXISTS, Name));
                }
            }

            fileSystemService.MoveFile(PhysicalPath, result.PhysicalPath);

            return result;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="FileExplorerFileNodeVm"/> class.
        /// </summary>
        /// <param name="copy">The node to copy.</param>
        internal FileExplorerFileNodeVm(FileExplorerFileNodeVm copy)
            : base(copy)
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileExplorerFileNodeVm" /> class.
        /// </summary>
        public FileExplorerFileNodeVm()
        {            
        }
        #endregion
    }
}
