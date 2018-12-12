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
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Content;
using Gorgon.Editor.Metadata;
using System.Collections.Generic;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// A node for a file system file.
    /// </summary>
    internal class FileExplorerFileNodeVm
        : FileExplorerNodeCommon, IContentFile
    {
        #region Variables.
        // Flag to indicate whether the node is open for editing.
        private bool _isOpen;
        // The file system information object.
        private FileInfo _fileInfo;
        #endregion

        #region Events.
        /// <summary>Event triggered if this content file was deleted.</summary>
        public event EventHandler Deleted;

        /// <summary>
        /// Event triggered if this content file was moved in the file system.
        /// </summary>
        public event EventHandler<ContentFileMovedEventArgs> Moved;

        /// <summary>
        /// Event triggered if this content file is excluded from the project.
        /// </summary>
        public event EventHandler Excluded;
        #endregion

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
        public override string ImageName => Metadata?.ContentMetadata == null ? "generic_file_20x20.png" : Metadata.ContentMetadata.SmallIconID.ToString("N");

        /// <summary>
        /// Property to return whether or not the allow this node to be deleted.
        /// </summary>
        public override bool AllowDelete => true;

        /// <summary>Property to return whether this node represents content or not.</summary>
        public override bool IsContent => true;

        /// <summary>Property to return the path to the file.</summary>
        string IContentFile.Path => FullPath;

        /// <summary>Property to return the extension for the file.</summary>
        string IContentFile.Extension => Path.GetExtension(Name);

        /// <summary>Property to return the plugin associated with the file.</summary>
        ContentPlugin IContentFile.ContentPlugin => Metadata?.ContentMetadata as ContentPlugin;

        /// <summary>Property to set or return the metadata for the node.</summary>
        public override ProjectItemMetadata Metadata
        {
            get => base.Metadata;
            set
            {
                if (base.Metadata == value)
                {
                    return;
                }

                base.Metadata = value;

                if (value == null)
                {
                    EventHandler handler = Excluded;
                    handler?.Invoke(this, EventArgs.Empty);
                }

                NotifyPropertyChanged(nameof(ImageName));
            }
        }

        /// <summary>Property to set or return whether the node is open for editing.</summary>
        public override bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (_isOpen == value)
                {
                    return;
                }

                OnPropertyChanging();
                _isOpen = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the physical path to the node.</summary>
        public override string PhysicalPath => Parent == null ? null : Path.Combine(Parent.PhysicalPath, Name);
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
                return response == MessageResponse.Cancel
                    ? FileSystemConflictResolution.Cancel
                    : response == MessageResponse.Yes ? FileSystemConflictResolution.Overwrite : FileSystemConflictResolution.Rename;
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


        /// <summary>Function called when the parent of this node is moved.</summary>
        /// <param name="newNode">The new node representing this node under the new parent.</param>
        protected override void OnNotifyParentMoved(IFileExplorerNodeVm newNode)
        {
            if ((!(newNode is IContentFile contentFile)) || (!contentFile.IsOpen))
            {
                return;
            }

            var args = new ContentFileMovedEventArgs(contentFile);
            EventHandler<ContentFileMovedEventArgs> handler = Moved;
            Moved?.Invoke(this, args);
        }

        /// <summary>Function to assign the appropriate content plug in to a node.</summary>
        /// <param name="plugins">The plugins.</param>
        /// <param name="deepScan"><b>true</b> to perform a more in depth scan for the associated plug in type, <b>false</b> to use the node metadata exclusively.</param>
        /// <returns><b>true</b> if a plug in was assigned, <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// If the <paramref name="deepScan" /> parameter is set to <b>true</b>, then the lookup for the plug ins will involve opening the file using each plug in to find a matching plug in for the node
        /// file type. This, obviously, is much slower, so should only be used when the node metadata is not sufficient for association information.
        /// </para>
        /// </remarks>
        protected override bool OnAssignContentPlugin(IContentPluginManagerService plugins, bool deepScan) => plugins.AssignContentPlugin(this, !deepScan);

        /// <summary>Function to retrieve the physical file system object for this node.</summary>
        /// <param name="path">The path to the physical file system object.</param>
        /// <returns>Information about the physical file system object.</returns>
        protected override FileSystemInfo GetFileSystemObject(string path)
        {
            _fileInfo = new FileInfo(path);
            return _fileInfo;
        }

        /// <summary>Function called to refresh the underlying data for the node.</summary>
        public override void Refresh()
        {
            NotifyPropertyChanging(nameof(PhysicalPath));

            try
            {
                _fileInfo.Refresh();
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
            }
            finally
            {
                NotifyPropertyChanged(nameof(PhysicalPath));
            }
        }

        /// <summary>
        /// Function to rename the node.
        /// </summary>
        /// <param name="newName">The new name for the node.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="newName"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="newName"/> parameter is empty.</exception>
        public override void RenameNode(string newName)
        {
            if (newName == null)
            {
                throw new ArgumentNullException(nameof(newName));
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentEmptyException(nameof(newName));
            }

            NotifyPropertyChanging(nameof(Name));

            try
            {
                FileSystemService.RenameFile(_fileInfo, newName);
                Refresh();
            }
            finally
            {
                NotifyPropertyChanged(nameof(Name));
            }            
        }

        /// <summary>
        /// Function to delete the node.
        /// </summary>
        /// <param name="onDeleted">[Optional] A function to call when a node or a child node is deleted.</param>
        /// <param name="cancelToken">[Optional] A cancellation token used to cancel the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="onDeleted"/> parameter is not used for this type.
        /// </para>
        /// <para>
        /// This implmentation does not delete the underlying file outright, it instead moves it into the recycle bin so the user can undo the delete if needed.
        /// </para>
        /// </remarks>
        public override Task DeleteNodeAsync(Action<FileSystemInfo> onDeleted = null, CancellationToken? cancelToken = null)
        {
            try
            {
                // Delete the physical object first. If we fail here, our node will survive.
                FileSystemService.DeleteFile(_fileInfo);
                
                NotifyPropertyChanging(nameof(FullPath));

                // Drop us from the parent list.
                // This will begin a chain reaction that will remove us from the UI.
                Parent.Children.Remove(this);
                Parent = null;
                
                // If this node is open in an editor, then we need to notify the editor that we just deleted the node.
                if (IsOpen)
                {
                    EventHandler deleteEvent = Deleted;
                    Deleted?.Invoke(this, EventArgs.Empty);
                    IsOpen = false;
                }
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Function to copy this node to another node.
        /// </summary>
        /// <param name="newPath">The node that will receive the the copy of this node.</param>
        /// <param name="onCopy">[Optional] The method to call when a file is about to be copied.</param>
        /// <param name="cancelToken">[Optional] A token used to cancel the operation.</param>
        /// <returns>The new node for the copied node.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destNode"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="destNode"/> is unable to create child nodes.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="onCopy" /> callback method sends the file system item being copied, the destination file system item, the current item #, and the total number of items to copy.
        /// </para>
        /// </remarks>
        public override Task<IFileExplorerNodeVm> CopyNodeAsync(IFileExplorerNodeVm destNode, Action<FileSystemInfo, FileSystemInfo, int, int> onCopy = null, CancellationToken? cancelToken = null)
        {
            if (destNode == null)
            {
                throw new ArgumentNullException(nameof(destNode));
            }

            if (!destNode.AllowChildCreation)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GOREDIT_ERR_NODE_CANNOT_CREATE_CHILDREN, destNode.Name));
            }

            var tcs = new TaskCompletionSource<IFileExplorerNodeVm>();

            FileSystemConflictResolution resolution = FileSystemConflictResolution.Exception;
            string newPath = Path.Combine(destNode.PhysicalPath, Name);

            var result = new FileExplorerFileNodeVm(this)
            {                
                _fileInfo = new FileInfo(newPath),
                IsExpanded = false,
                Parent = destNode,
                Metadata = Metadata != null ? new ProjectItemMetadata(Metadata) : null
            };

            // Renames a node that is in conflict when the file is the same in the source and dest, or if the user chooses to not overwrite.
            void RenameNodeInConflict()
            {
                string newName = FileSystemService.GenerateFileName(result.PhysicalPath);
                newPath = Path.Combine(destNode.PhysicalPath, newName);
                result._fileInfo = new FileInfo(newPath);
            }

            // If we're trying to copy over ourselves (which is not possible obviously), then just auto-rename.
            if (string.Equals(FullPath, result.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                RenameNodeInConflict();
            }

            if (FileSystemService.FileExists(newPath))
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

            FileSystemService.CopyFile(_fileInfo, result.PhysicalPath);

            // TODO: Create a copy of any links for this node.
            // TODO: Can we make use of the onCopy method?
            tcs.SetResult(result);

            return tcs.Task;
        }

        /// <summary>Function to move this node to another node.</summary>
        /// <param name="destNode">The node that will receive this node as a child.</param>
        /// <returns><b>true</b> if the node was moved, <b>false</b> if it was cancelled or had an error moving.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destNode" /> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="destNode" /> is unable to create child nodes.</exception>
        public override bool MoveNode(IFileExplorerNodeVm destNode)
        {
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
                return false;
            }

            FileSystemConflictResolution resolution = FileSystemConflictResolution.Exception;
            string newPath = Path.Combine(destNode.PhysicalPath, Name);

            if (FileSystemService.FileExists(newPath))
            {
                resolution = FileSystemConflictHandler(FullPath, newPath, false);

                switch (resolution)
                {
                    case FileSystemConflictResolution.Rename:
                    case FileSystemConflictResolution.RenameAll:
                    case FileSystemConflictResolution.Cancel:
                        return false;
                    case FileSystemConflictResolution.Exception:
                        throw new IOException(string.Format(Resources.GOREDIT_ERR_NODE_EXISTS, Name));
                }
            }

            FileSystemService.MoveFile(_fileInfo, newPath);

            if (IsOpen)
            {
                var args = new ContentFileMovedEventArgs(this);
                EventHandler<ContentFileMovedEventArgs> handler = Moved;
                Moved?.Invoke(this, args);
            }

            NotifyPropertyChanged(FullPath);
            NotifyPropertyChanged(PhysicalPath);

            return true;
        }

        /// <summary>
        /// Function to export the contents of this node to the physical file system.
        /// </summary>
        /// <param name="destPath">The path to the directory on the physical file system that will receive the contents.</param>
        /// <param name="onCopy">[Optional] The method to call when a file is about to be copied.</param>
        /// <param name="cancelToken">[Optional] A token used to cancel the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <remarks>The <paramref name="onCopy" /> callback method sends the file system node being copied, the destination file system node, the current item #, and the total number of items to copy.</remarks>
        public override Task ExportAsync(string destPath, Action<FileSystemInfo, FileSystemInfo, int, int> onCopy = null, CancellationToken? cancelToken = null)
        {
            if (destPath == null)
            {
                throw new ArgumentNullException(nameof(destPath));
            }

            if (string.IsNullOrWhiteSpace(destPath))
            {
                throw new ArgumentEmptyException(nameof(destPath));
            }

            // Progress update
            void ProgressUpdate(FileSystemInfo file) => onCopy?.Invoke(file, null, 1, 1);

            return FileSystemService.ExportFileAsync(PhysicalPath, destPath, ProgressUpdate);
        }

        /// <summary>Function to open the file for reading.</summary>
        /// <returns>A stream containing the file data.</returns>
        Stream IContentFile.OpenRead() => File.OpenRead(PhysicalPath);

        /// <summary>Function to notify that the metadata should be refreshed.</summary>
        void IContentFile.RefreshMetadata() => NotifyPropertyChanged(nameof(Metadata));
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
