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
using System.Linq;
using Gorgon.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Gorgon.Editor.UI;

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
        // The list of content file dependant upon this file.
        private ObservableCollection<IContentFile> _dependencies;
        #endregion

        #region Events.
        /// <summary>Event triggered if this content file was deleted.</summary>
        public event EventHandler Deleted;

        /// <summary>Event triggered if this content file was renamed.</summary>
        public event EventHandler<ContentFileRenamedEventArgs> Renamed;
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

        /// <summary>Property to return the list of items dependant upon this node</summary>
        IList<IContentFile> IContentFile.Dependencies => _dependencies;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to synchronize between two collections when one of the collections has changed.
        /// </summary>
        /// <typeparam name="T">The type of data in the destination collection.</typeparam>
        /// <param name="dest">The destination collection.</param>
        /// <param name="e">The event parameters from the collection changed event.</param>
        private void HandleCollectionSync<T>(ObservableCollection<T> dest, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (T node in e.NewItems.OfType<T>())
                    {
                        if (dest.Contains(node))
                        {
                            continue;
                        }

                        dest.Add(node);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (T node in e.OldItems.OfType<T>())
                    {
                        if (!dest.Contains(node))
                        {
                            continue;
                        }

                        dest.Remove(node);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (dest.Count > 0)
                    {
                        dest.Clear();
                    }
                    break;
            }
        }

        /// <summary>Handles the CollectionChanged event of the Dependencies control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void ContentDependencies_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => HandleCollectionSync(Dependencies, e);

        /// <summary>Handles the CollectionChanged event of the Dependencies control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void Dependencies_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => HandleCollectionSync(_dependencies, e);

        /// <summary>Function to retrieve the physical file system object for this node.</summary>
        /// <param name="path">The path to the physical file system object.</param>
        /// <returns>Information about the physical file system object.</returns>
        protected override FileSystemInfo OnGetFileSystemObject(string path)
        {
            _fileInfo = new FileInfo(path);
            return _fileInfo;
        }

        /// <summary>
        /// Function to rename a node that is in conflict when the file is the same in the source and dest, or if the user chooses to not overwrite.
        /// </summary>
        /// <param name="path">The original path to the file.</param>
        /// <param name="destPath">The destination path.</param>
        /// <returns>The updated file path.</returns>
        public string RenameNodeInConflict(string path, string destPath)
        {
            string newName = FileSystemService.GenerateFileName(path);
            return Path.Combine(destPath, newName);
        }

        /// <summary>Function called to refresh the underlying data for the node.</summary>
        public override void Refresh()
        {
            NotifyPropertyChanging(nameof(PhysicalPath));

            try
            {
                // If the node is desynced with the actual physical path, then we need to refresh based on where the parent is located.
                if (!_fileInfo.FullName.StartsWith(Parent.PhysicalPath, StringComparison.OrdinalIgnoreCase))
                {
                    GetFileSystemObject(Path.Combine(Parent.PhysicalPath, Name));
                }
                else
                {
                    _fileInfo.Refresh();
                }
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
                string oldName = Name;

                FileSystemService.RenameFile(_fileInfo, newName);
                Refresh();
                EventHandler<ContentFileRenamedEventArgs> handler = Renamed;
                handler?.Invoke(this, new ContentFileRenamedEventArgs(oldName, _fileInfo.Name));
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
        /// The <paramref name="onDeleted"/> parameter passes the node that being deleted, so callers can use that information for their own purposes.
        /// </para>
        /// <para>
        /// This implmentation does not delete the underlying file outright, it instead moves it into the recycle bin so the user can undo the delete if needed.
        /// </para>
        /// </remarks>
        public override Task DeleteNodeAsync(Action<IFileExplorerNodeVm> onDeleted = null, CancellationToken? cancelToken = null)
        {
            try
            {
                // Delete the physical object first. If we fail here, our node will survive.
                FileSystemService.DeleteFile(_fileInfo);

                IFileExplorerNodeVm openDependant = Dependencies.FirstOrDefault(item => item.IsOpen);
                EventHandler handler = Deleted;
                handler?.Invoke(this, EventArgs.Empty);

                // If this node is open, then we need to notify the content that we just deleted the node it is associated with.
                if (IsOpen)
                {
                    IsOpen = false;
                }

                // If we have a source file, remove it.
                if ((Metadata != null) 
                    && (Metadata.Attributes.TryGetValue(ContentImportPlugin.ImportOriginalFileNameAttr, out string sourcePath)))
                {                    
                    var sourceFile = new FileInfo(Path.Combine(Project.SourceDirectory.FullName, sourcePath));
                    
                    if (sourceFile.Exists)
                    {
                        Program.Log.Print($"{FullPath} has a source file that it was imported from: {sourceFile.Name}. This file will be deleted as well.", LoggingLevel.Verbose);

                        try
                        {
                            sourceFile.Delete();
                        }
                        catch(Exception ex)
                        {
                            Program.Log.Print($"[ERROR] Could not delete source file {sourcePath}.", LoggingLevel.Verbose);
                            Program.Log.LogException(ex);
                        }
                    }
                }
                
                NotifyPropertyChanging(nameof(FullPath));

                OnUnload();
                
                // Drop us from the parent list.
                // This will begin a chain reaction that will remove us from the UI.
                Parent.Children.Remove(this);
                Parent = null;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Function to copy the file node into another node.
        /// </summary>
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

            FileSystemConflictResolution? conflictResolution = copyNodeData.DefaultResolution;
            string newPath = Path.Combine(copyNodeData.Destination.PhysicalPath, Name);
            bool isConflictDueToOpenFile = false;

            // If we attempt to copy over ourselves, just default to rename.
            if (string.Equals(newPath, _fileInfo.FullName, StringComparison.OrdinalIgnoreCase))
            {
                newPath = RenameNodeInConflict(newPath, copyNodeData.Destination.PhysicalPath);
            }

            var destFile = new FileInfo(newPath);
            // A duplicate node that we are conflicting with.
            IFileExplorerNodeVm dupeNode = null;
            long copied = copyNodeData.BytesCopied;

            // Check for a duplicate file and determine how to proceed.
            if (FileSystemService.FileExists(destFile))
            {
                // If we choose to overwrite.
                dupeNode = copyNodeData.Destination.Children.FirstOrDefault(item => string.Equals(item.Name, Name, StringComparison.OrdinalIgnoreCase));

                // If by some weird circumstance we can't find the node with the same name, then just default to overwrite it as it's not managed by the application, and if it's under 
                // the file system root, it really should be ours.
                if (dupeNode != null)
                {
                    // If we previously gave a conflict resolution, and it indicated all operations should continue then skip the call back since we have our answer.
                    // Otherwise, determine how to resolve the conflict.
                    // If the file is open in the editor then we have no choice but to force the resolver to run because we cannot mess with an open file.
                    isConflictDueToOpenFile = (dupeNode.IsOpen) && (conflictResolution == FileSystemConflictResolution.OverwriteAll);
                    if ((copyNodeData.ConflictHandler != null) && ((isConflictDueToOpenFile) 
                        || ((conflictResolution != FileSystemConflictResolution.OverwriteAll) && (conflictResolution != FileSystemConflictResolution.RenameAll))))
                    {                       
                        conflictResolution = copyNodeData.ConflictHandler(this, dupeNode, true, copyNodeData.UseToAllInConflictHandler);
                    }
                    else if (copyNodeData.ConflictHandler == null)
                    {
                        // Default to exception if we have a conflict.
                        conflictResolution = FileSystemConflictResolution.Exception;
                    }
                }
                else
                {
                    conflictResolution = FileSystemConflictResolution.Overwrite;
                }

                switch (conflictResolution)
                {
                    case FileSystemConflictResolution.Rename:
                    case FileSystemConflictResolution.RenameAll:
                        newPath = RenameNodeInConflict(newPath, copyNodeData.Destination.PhysicalPath);
                        destFile = new FileInfo(newPath);
                        dupeNode = null;
                        break;
                    case FileSystemConflictResolution.Skip:
                        if (isConflictDueToOpenFile)
                        {
                            // Reset back to overwrite if we hit an open file (just so we don't get the resolver again).
                            copyNodeData.DefaultResolution = FileSystemConflictResolution.OverwriteAll;
                        }
                        else
                        {
                            copyNodeData.DefaultResolution = conflictResolution;
                        }
                        return null;
                    case FileSystemConflictResolution.Cancel:                        
                        copyNodeData.DefaultResolution = conflictResolution;
                        return null;
                    case FileSystemConflictResolution.Exception:
                    case null:
                        throw new IOException(string.Format(Resources.GOREDIT_ERR_NODE_EXISTS, Name));
                }
            }
            
            if ((conflictResolution == FileSystemConflictResolution.RenameAll)
                || (conflictResolution == FileSystemConflictResolution.OverwriteAll))
            {
                copyNodeData.DefaultResolution = conflictResolution;
            }

            // Now that the duplicate check is done, we can actually copy the file.
            // Unlike copying directories, we don't have to worry about digging down through the hierarchy and reporting back.
            // However, if the file is large, it will take time to copy, so we'll send notifcation back to indicate how much data we've copied.
            void ReportProgress(long bytesCopied, long bytesTotal) => copyNodeData.CopyProgress?.Invoke(this, copied + bytesCopied, copyNodeData.TotalSize ?? bytesTotal);

            try
            {
                await Task.Run(() => FileSystemService.CopyFile(_fileInfo, destFile, ReportProgress, copyNodeData.CancelToken, copyNodeData.WriteBuffer));
                copyNodeData.BytesCopied += _fileInfo.Length;
                destFile.Refresh();
            }
            catch
            {
                destFile.Refresh();
                // Clean up the partially copied file.
                if (destFile.Exists)
                {
                    destFile.Delete();
                }
                throw;
            }

            if (dupeNode == null)
            {
                // Once the file is actually on the file system, make a node and attach it to the parent.            
                return ViewModelFactory.CreateFileExplorerFileNodeVm(Project, FileSystemService, copyNodeData.Destination, destFile, new ProjectItemMetadata(Metadata));
            }

            // If we've overwritten a node, then just refresh its state and return it.  There's no need to re-add it to the list at this point.
            dupeNode.Refresh();
            return dupeNode;
        }

        /// <summary>
        /// Function to move this node into another node.
        /// </summary>
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

            FileSystemConflictResolution? conflictResolution = copyNodeData.DefaultResolution;
            string newPath = Path.Combine(copyNodeData.Destination.PhysicalPath, Name);
            bool isConflictDueToOpenFile = false;

            // Renames a node that is in conflict when the file is the same in the source and dest, or if the user chooses to not overwrite.
            string RenameNodeInConflict(string path)
            {
                string newName = FileSystemService.GenerateFileName(path);
                return Path.Combine(copyNodeData.Destination.PhysicalPath, newName);
            }

            // If we attempt to copy over ourselves, just cancel.
            if (string.Equals(newPath, _fileInfo.FullName, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var destFile = new FileInfo(newPath);
            // A duplicate node that we are conflicting with.
            IFileExplorerNodeVm dupeNode = null;

            // Check for a duplicate file and determine how to proceed.
            if (FileSystemService.FileExists(destFile))
            {
                // If we choose to overwrite.
                dupeNode = copyNodeData.Destination.Children.FirstOrDefault(item => string.Equals(item.Name, Name, StringComparison.OrdinalIgnoreCase));

                // If by some weird circumstance we can't find the node with the same name, then just default to overwrite it as it's not managed by the application, and if it's under 
                // the file system root, it really should be ours.
                if (dupeNode != null)
                {
                    // If we previously gave a conflict resolution, and it indicated all operations should continue then skip the call back since we have our answer.
                    // Otherwise, determine how to resolve the conflict.
                    // If the file is open in the editor then we have no choice but to force the resolver to run because we cannot mess with an open file.
                    isConflictDueToOpenFile = (dupeNode.IsOpen) && (conflictResolution == FileSystemConflictResolution.OverwriteAll);
                    if ((copyNodeData.ConflictHandler != null) && ((isConflictDueToOpenFile)
                        || ((conflictResolution != FileSystemConflictResolution.OverwriteAll) && (conflictResolution != FileSystemConflictResolution.RenameAll))))
                    {
                        conflictResolution = copyNodeData.ConflictHandler(this, dupeNode, true, copyNodeData.UseToAllInConflictHandler);
                    }
                    else if (copyNodeData.ConflictHandler == null)
                    {
                        // Default to exception if we have a conflict.
                        conflictResolution = FileSystemConflictResolution.Exception;
                    }
                }
                else
                {
                    conflictResolution = FileSystemConflictResolution.Overwrite;
                }

                switch (conflictResolution)
                {
                    case FileSystemConflictResolution.Rename:
                    case FileSystemConflictResolution.RenameAll:
                        newPath = RenameNodeInConflict(newPath);
                        destFile = new FileInfo(newPath);
                        dupeNode = null;
                        break;
                    case FileSystemConflictResolution.Skip:
                        if (isConflictDueToOpenFile)
                        {
                            // Reset back to overwrite if we hit an open file (just so we don't get the resolver again).
                            copyNodeData.DefaultResolution = FileSystemConflictResolution.OverwriteAll;
                        }
                        else
                        {
                            copyNodeData.DefaultResolution = conflictResolution;
                        }
                        return null;
                    case FileSystemConflictResolution.Cancel:
                        copyNodeData.DefaultResolution = conflictResolution;
                        return null;
                    case FileSystemConflictResolution.Exception:
                    case null:
                        throw new IOException(string.Format(Resources.GOREDIT_ERR_NODE_EXISTS, Name));
                }
            }

            if ((conflictResolution == FileSystemConflictResolution.RenameAll)
                || (conflictResolution == FileSystemConflictResolution.OverwriteAll))
            {
                copyNodeData.DefaultResolution = conflictResolution;
            }

            FileSystemService.MoveFile(_fileInfo, destFile);
            destFile.Refresh();

            // Extract us from our parent.
            Parent.Children.Remove(this);

            // Update our parent.
            Parent = copyNodeData.Destination;

            // If we don't have a conflict, then just place us under the parent.
            if (dupeNode == null)
            {
                copyNodeData.Destination.Children.Add(this);
                Refresh();
                NotifyPropertyChanged(nameof(FullPath));
                NotifyPropertyChanged(nameof(PhysicalPath));
                // Once the file is actually on the file system, make a node and attach it to the parent.            
                return this;
            }

            // If we've overwritten a node, then just refresh its state and return it.  There's no need to re-add it to the list at this point.
            dupeNode.Refresh();
            NotifyPropertyChanged(nameof(FullPath));
            NotifyPropertyChanged(nameof(PhysicalPath));
            return dupeNode;
        }

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

            FileSystemConflictResolution? conflictResolution = exportNodeData.DefaultResolution;
            string newPath = Path.Combine(exportNodeData.Destination.FullName, Name);
            long copied = exportNodeData.BytesCopied;

            // If we attempt to copy over ourselves, just default to rename.
            if (string.Equals(newPath, _fileInfo.FullName, StringComparison.OrdinalIgnoreCase))
            {
                newPath = RenameNodeInConflict(newPath, exportNodeData.Destination.FullName);
            }

            var destFile = new FileInfo(newPath);

            // Check for a duplicate file and determine how to proceed.
            if (destFile.Exists)
            {
                // If we previously gave a conflict resolution, and it indicated all operations should continue then skip the call back since we have our answer.
                // Otherwise, determine how to resolve the conflict.
                // If the file is open in the editor then we have no choice but to force the resolver to run because we cannot mess with an open file.                
                if ((exportNodeData.ConflictHandler != null) && (conflictResolution != FileSystemConflictResolution.OverwriteAll) && (conflictResolution != FileSystemConflictResolution.RenameAll))
                {
                    conflictResolution = exportNodeData.ConflictHandler(this, destFile, true, exportNodeData.UseToAllInConflictHandler);
                }
                else if (exportNodeData.ConflictHandler == null)
                {
                    // Default to exception if we have a conflict.
                    conflictResolution = FileSystemConflictResolution.Exception;
                }

                switch (conflictResolution)
                {
                    case FileSystemConflictResolution.Rename:
                    case FileSystemConflictResolution.RenameAll:
                        newPath = RenameNodeInConflict(newPath, exportNodeData.Destination.FullName);
                        destFile = new FileInfo(newPath);
                        break;
                    case FileSystemConflictResolution.Cancel:
                        exportNodeData.DefaultResolution = conflictResolution;
                        return;
                    case FileSystemConflictResolution.Exception:
                    case null:
                        throw new IOException(string.Format(Resources.GOREDIT_ERR_NODE_EXISTS, Name));
                }
            }

            if ((conflictResolution == FileSystemConflictResolution.RenameAll)
                || (conflictResolution == FileSystemConflictResolution.OverwriteAll))
            {
                exportNodeData.DefaultResolution = conflictResolution;
            }

            // Now that the duplicate check is done, we can actually copy the file.
            // Unlike copying directories, we don't have to worry about digging down through the hierarchy and reporting back.
            // However, if the file is large, it will take time to copy, so we'll send notifcation back to indicate how much data we've copied.
            void ReportProgress(long bytesCopied, long bytesTotal) => exportNodeData.CopyProgress?.Invoke(this, copied + bytesCopied, exportNodeData.TotalSize ?? bytesTotal);

            try
            {
                await Task.Run(() => FileSystemService.ExportFile(_fileInfo, destFile, ReportProgress, exportNodeData.CancelToken, exportNodeData.WriteBuffer));
                exportNodeData.BytesCopied += _fileInfo.Length;
                destFile.Refresh();
            }
            catch
            {
                destFile.Refresh();
                // Clean up the partially copied file.
                if (destFile.Exists)
                {
                    destFile.Delete();
                }
                throw;
            }
        }

        /// <summary>Function to open the file for reading.</summary>
        /// <returns>A stream containing the file data.</returns>
        Stream IContentFile.OpenRead() => File.Open(PhysicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);

        /// <summary>
        /// Function to open the file for writing.
        /// </summary>
        /// <param name="append">[Optional] <b>true</b> to append data to the end of the file, or <b>false</b> to overwrite.</param>
        /// <returns>A stream to write the file data into.</returns>
        Stream IContentFile.OpenWrite(bool append) => File.Open(PhysicalPath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None);


        /// <summary>Function to notify that the metadata should be refreshed.</summary>
        void IContentFile.RefreshMetadata() => NotifyPropertyChanged(nameof(Metadata));

        /// <summary>Function called to refresh the underlying data for the node.</summary>
        void IContentFile.Refresh()
        {
            Refresh();
            NotifyPropertyChanged(nameof(Metadata));
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
        public override long GetSizeInBytes() => _fileInfo.Length;

        /// <summary>Function called when the associated view is unloaded.</summary>
        public override void OnUnload()
        {
            foreach (IFileExplorerNodeVm dep in Dependencies)
            {
                dep.OnUnload();
            }
            _dependencies.CollectionChanged -= ContentDependencies_CollectionChanged;
            Dependencies.CollectionChanged -= Dependencies_CollectionChanged;
            base.OnUnload();
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
            _dependencies = new ObservableCollection<IContentFile>();
            _dependencies.CollectionChanged += ContentDependencies_CollectionChanged;
            Dependencies.CollectionChanged += Dependencies_CollectionChanged;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileExplorerFileNodeVm" /> class.
        /// </summary>
        public FileExplorerFileNodeVm()
        {
            _dependencies = new ObservableCollection<IContentFile>();
            _dependencies.CollectionChanged += ContentDependencies_CollectionChanged;
            Dependencies.CollectionChanged += Dependencies_CollectionChanged;
        }
        #endregion
    }
}
