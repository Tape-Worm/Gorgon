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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Editor.Content;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.IO;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// A node for a file system directory.
    /// </summary>
    internal class FileExplorerDirectoryNodeVm
        : FileExplorerNodeCommon
    {
        #region Variables.
        // The physical directory represented by this node.
        private DirectoryInfo _directoryInfo;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether to allow child node creation for this node.
        /// </summary>
        public override bool AllowChildCreation => true;

        /// <summary>
        /// Property to return the type of node.
        /// </summary>
        public override NodeType NodeType => NodeType.Directory;

        /// <summary>
        /// Property to return the full path to the node.
        /// </summary>
        public override string FullPath => Parent == null ? "/" : (Parent.FullPath + Name).FormatDirectory('/');

        /// <summary>
        /// Property to return the image name to use for the node type.
        /// </summary>
        public override string ImageName => "folder_20x20.png";

        /// <summary>
        /// Property to return whether or not the allow this node to be deleted.
        /// </summary>
        public override bool AllowDelete => true;

        /// <summary>Property to return whether this node represents content or not.</summary>
        public override bool IsContent => false;

        /// <summary>Property to return the physical path to the node.</summary>
        public override string PhysicalPath => Parent == null ? null : Path.Combine(Parent.PhysicalPath, Name).FormatDirectory(Path.DirectorySeparatorChar);

        /// <summary>Property to set or return whether the node is open for editing.</summary>
        /// <remarks>Directory nodes cannot be opened for editing, thus this property will always return <strong>false</strong>.</remarks>
        public override bool IsOpen
        {
            get => false;
            set
            {
                // Intentionally left blank.
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to perform a refresh of this node and any children.
        /// </summary>
        private void DoRefresh()
        {
            // If the node is desynced with the actual physical path
            if (!Parent.PhysicalPath.StartsWith(_directoryInfo.FullName, StringComparison.OrdinalIgnoreCase))
            {
                GetFileSystemObject(Path.Combine(Parent.PhysicalPath, Name));
            }
            else
            {
                _directoryInfo?.Refresh();
            }            

            // Refresh all child nodes.
            foreach (IFileExplorerNodeVm node in Children.Traverse(n => n.Children))
            {
                node.Refresh();
            }
        }

        /// <summary>Function to retrieve the physical file system object for this node.</summary>
        /// <param name="path">The path to the physical file system object.</param>
        /// <returns>Information about the physical file system object.</returns>
        protected override FileSystemInfo OnGetFileSystemObject(string path)
        {
            _directoryInfo = new DirectoryInfo(path);
            return _directoryInfo;
        }

        /// <summary>Function called to refresh the underlying data for the node.</summary>
        public override void Refresh()
        {
            NotifyPropertyChanging(nameof(PhysicalPath));

            try
            {
                DoRefresh();
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
        /// Function to delete the node.
        /// </summary>
        /// <param name="onDeleted">[Optional] A function to call when a node or a child node is deleted.</param>
        /// <param name="cancelToken">[Optional] A cancellation token used to cancel the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="onDeleted"/> parameter passes the node that being deleted, so callers can use that information for their own purposes.
        /// </para>
        /// <para>
        /// This implmentation does not delete the underlying directory outright, it instead moves it into the recycle bin so the user can undo the delete if needed.
        /// </para>
        /// </remarks>
        public override async Task DeleteNodeAsync(Action<IFileExplorerNodeVm> onDeleted = null, CancellationToken? cancelToken = null)
        {
            IFileExplorerNodeVm rootNode = GetRoot();
            
            // Callback used to notify for delete progress.
            void ProgressUpdate(FileSystemInfo info)
            {
                string path = info.ToFileSystemPath(Project.FileSystemDirectory);
                IFileExplorerNodeVm deletedNode;

                if (string.Equals(info.FullName, _directoryInfo.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    deletedNode = this;
                }
                else
                {                    
                    deletedNode = Children.Traverse(n => n.Children)
                                          .FirstOrDefault(item => string.Equals(path, item.FullPath, StringComparison.OrdinalIgnoreCase));
                }

                if (deletedNode == null)
                {
                    return;
                }

                onDeleted?.Invoke(deletedNode);
            }

            if (Children.Traverse(n => n.Children).Where(item => item.IsOpen).FirstOrDefault() is IContentFile openChild)
            {
                openChild.CloseContent();
            }

            // Delete the physical objects first. If we fail here, our node will survive.
            // We do this asynchronously because deleting a directory with a lot of files may take a while, especially since we are dumping to the recycle bin.
            bool dirDeleted = await Task.Run(() => FileSystemService.DeleteDirectory(_directoryInfo, ProgressUpdate, cancelToken ?? CancellationToken.None));

            // If, for some reason, our directory was not deleted, then do not remove the node.
            if (!dirDeleted)
            {
                return;
            }

            NotifyPropertyChanging(nameof(FullPath));

            // Drop us from the parent list.
            // This will begin a chain reaction that will remove us from the UI.
            Parent.Children.Remove(this);
            Parent = null;

            OnUnload();
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

            // Remove the previous project item.
            try
            {
                FileSystemService.RenameDirectory(_directoryInfo, newName);
                DoRefresh();
            }
            finally
            {
                NotifyPropertyChanged(nameof(Name));
            }            
        }

        /// <summary>Function to copy the file node into another node.</summary>
        /// <param name="copyNodeData">The data containing information about what to copy.</param>
        /// <returns>The newly copied node.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="copyNodeData"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentMissingException">Thrown when the the <see cref="CopyNodeData.Destination"/> member of the <paramref name="copyNodeData"/> parameter are <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the destination node in <paramref name="copyNodeData"/> is unable to create child nodes.</exception>
        public override async Task<IFileExplorerNodeVm> CopyNodeAsync(CopyNodeData copyNodeData)
        {
            if (copyNodeData == null)
            {
                throw new ArgumentNullException(nameof(copyNodeData));
            }

            if (copyNodeData.Destination == null)
            {
                throw new ArgumentMissingException(nameof(CopyNodeData.Destination), nameof(copyNodeData));
            }

            if (!copyNodeData.Destination.AllowChildCreation)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GOREDIT_ERR_NODE_CANNOT_CREATE_CHILDREN, copyNodeData.Destination.Name));
            }

            string newPath = Path.Combine(copyNodeData.Destination.PhysicalPath, Name);
            var destDir = new DirectoryInfo(newPath);
            IFileExplorerNodeVm result = null;
            long bytesTotal = copyNodeData.TotalSize ?? GetSizeInBytes();
            // Define a write buffer here so we don't create a new one for each file copied.
            byte[] writeBuffer = new byte[81920]; 

            try
            {
                // If the directory does not exist, then create it now.
                if (!FileSystemService.DirectoryExists(destDir))
                {
                    destDir.Create();
                    destDir.Refresh();

                    result = ViewModelFactory.CreateFileExplorerDirectoryNodeVm(Project, FileSystemService, copyNodeData.Destination, destDir, false);
                    result.IsExpanded = false;
                }
                else
                {
                    result = copyNodeData.Destination.Children.First(item => string.Equals(item.Name, Name, StringComparison.OrdinalIgnoreCase));
                }

                foreach (IFileExplorerNodeVm child in Children.OrderBy(n => n.NodeType))
                {
                    if ((copyNodeData.CancelToken.IsCancellationRequested)
                        || (copyNodeData.DefaultResolution == FileSystemConflictResolution.Cancel))
                    {
                        return result;
                    }

                    var childCopyData = new CopyNodeData
                    {
                        Destination = result,
                        CancelToken = copyNodeData.CancelToken,
                        ConflictHandler = copyNodeData.ConflictHandler,
                        CopyProgress = copyNodeData.CopyProgress,
                        DefaultResolution = copyNodeData.DefaultResolution,
                        TotalSize = bytesTotal,
                        BytesCopied = copyNodeData.BytesCopied,
                        UseToAllInConflictHandler = true,
                        WriteBuffer = writeBuffer
                    };

                    await child.CopyNodeAsync(childCopyData);
                    copyNodeData.BytesCopied = childCopyData.BytesCopied;

                    switch (childCopyData.DefaultResolution)
                    {
                        case FileSystemConflictResolution.OverwriteAll:
                        case FileSystemConflictResolution.RenameAll:
                        case FileSystemConflictResolution.Cancel:
                            copyNodeData.DefaultResolution = childCopyData.DefaultResolution;
                            break;
                        default:
                            copyNodeData.DefaultResolution = null;
                            break;
                    }
                }

                // Only after we're done copying will we update the collection of our parent.
                if (!copyNodeData.Destination.Children.Contains(result))
                {
                    copyNodeData.Destination.Children.Add(result);
                }

                return result;
            }
            catch
            {
                // If we screw up, then undo our change.
                destDir.Refresh();
                if ((destDir.Exists) 
                    && (!copyNodeData.Destination.Children.Any(item => string.Equals(item.PhysicalPath, destDir.FullName.FormatDirectory(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))))
                {
                    destDir.Delete(true);
                }
                throw;
            }
        }

        /// <summary>Function to move this node into another node.</summary>
        /// <param name="copyNodeData">The parameters used for moving the node.</param>
        /// <returns>The udpated node.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="copyNodeData"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentMissingException">Thrown when the <see cref="CopyNodeData.Destination"/> in the <paramref name="copyNodeData"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <see cref="CopyNodeData.Destination"/> in the <paramref name="copyNodeData"/> parameter is unable to create child nodes.</exception>
        public override IFileExplorerNodeVm MoveNode(CopyNodeData copyNodeData)
        {
            if (copyNodeData == null)
            {
                throw new ArgumentNullException(nameof(copyNodeData));
            }

            if (copyNodeData.Destination == null)
            {
                throw new ArgumentMissingException(nameof(CopyNodeData.Destination), nameof(copyNodeData));
            }

            if (!copyNodeData.Destination.AllowChildCreation)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GOREDIT_ERR_NODE_CANNOT_CREATE_CHILDREN, copyNodeData.Destination.Name));
            }

            // No point in moving the node onto itself.
            if (string.Equals(Parent.FullPath, copyNodeData.Destination.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            string newPath = Path.Combine(copyNodeData.Destination.PhysicalPath, Name);
            var destDir = new DirectoryInfo(newPath);
            IFileExplorerNodeVm dupeNode = null;
            FileSystemService.MoveDirectory(_directoryInfo, newPath);

            if (FileSystemService.DirectoryExists(destDir))
            {
                dupeNode = copyNodeData.Destination.Children.FirstOrDefault(item => string.Equals(Name, item.Name, StringComparison.OrdinalIgnoreCase));
            }

            Parent.Children.Remove(this);
            Parent = copyNodeData.Destination;
            if (dupeNode == null)
            {
                copyNodeData.Destination.Children.Add(this);
                Refresh();
            }
            else
            {
                dupeNode.Refresh();
            }            

            return dupeNode ?? this;
        }

        /// <summary>
        /// Function to retrieve the size of the data on the physical file system.
        /// </summary>        
        /// <returns>The size of the data on the physical file system, in bytes.</returns>
        /// <remarks>
        /// <para>
        /// For nodes with children, this will sum up the size of each item in the <see cref="Children"/> list.  For items that do not have children, then only the size of the immediate item is returned.
        /// </para>
        /// </remarks>
        public override long GetSizeInBytes() => Children.Count == 0 ? 0 : Children.Traverse(n => n.Children).Sum(n => n.GetSizeInBytes());
        
        /// <summary>
        /// Function to export the contents of this node to the physical file system.
        /// </summary>
        /// <param name="exportNodeData">The parameters ued for exporting the node.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="exportNodeData"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentMissingException">Thrown when the the <see cref="CopyNodeData.Destination"/> member of the <paramref name="exportNodeData"/> parameter are <b>null</b>.</exception>
        public async override Task ExportAsync(ExportNodeData exportNodeData)
        {
            if (exportNodeData == null)
            {
                throw new ArgumentNullException(nameof(exportNodeData));
            }

            if (exportNodeData.Destination == null)
            {
                throw new ArgumentMissingException(nameof(CopyNodeData.Destination), nameof(exportNodeData));
            }

            string newPath;

            if (Parent != null)
            {
                newPath = Path.Combine(exportNodeData.Destination.FullName, Name);
            }
            else
            {
                newPath = exportNodeData.Destination.FullName;
            }
            
            var destDir = new DirectoryInfo(newPath);
            long bytesTotal = exportNodeData.TotalSize ?? GetSizeInBytes();
            // Define a write buffer here so we don't create a new one for each file copied.
            byte[] writeBuffer = new byte[81920];

            // If the directory does not exist, then create it now.
            if (!destDir.Exists)
            {
                destDir.Create();
                destDir.Refresh();
            }

            foreach (IFileExplorerNodeVm child in Children.OrderBy(n => n.NodeType))
            {
                if (exportNodeData.CancelToken.IsCancellationRequested)
                {
                    return;
                }

                var childCopyData = new ExportNodeData
                {
                    Destination = destDir,
                    CancelToken = exportNodeData.CancelToken,
                    ConflictHandler = exportNodeData.ConflictHandler,
                    CopyProgress = exportNodeData.CopyProgress,
                    DefaultResolution = exportNodeData.DefaultResolution,
                    TotalSize = bytesTotal,
                    BytesCopied = exportNodeData.BytesCopied,
                    UseToAllInConflictHandler = true,
                    WriteBuffer = writeBuffer
                };

                await child.ExportAsync(childCopyData);
                exportNodeData.BytesCopied = childCopyData.BytesCopied;

                switch (childCopyData.DefaultResolution)
                {
                    case FileSystemConflictResolution.OverwriteAll:
                    case FileSystemConflictResolution.RenameAll:
                    case FileSystemConflictResolution.Cancel:
                        exportNodeData.DefaultResolution = childCopyData.DefaultResolution;
                        break;
                    default:
                        exportNodeData.DefaultResolution = null;
                        break;
                }
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="FileExplorerDirectoryNodeVm"/> class.
        /// </summary>
        /// <param name="copy">The node to copy.</param>
        internal FileExplorerDirectoryNodeVm(FileExplorerDirectoryNodeVm copy)
            : base(copy)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileExplorerDirectoryNodeVm" /> class.
        /// </summary>
        public FileExplorerDirectoryNodeVm()
        {
        }
        #endregion
    }
}
