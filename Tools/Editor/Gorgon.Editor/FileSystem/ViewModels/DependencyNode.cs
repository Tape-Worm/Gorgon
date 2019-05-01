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
using System.Threading;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Content;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.UI;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// A node for a file system file.
    /// </summary>
    internal class DependencyNode
        : ViewModelBase<FileExplorerNodeParameters>, IFileExplorerNodeVm, IContentFile
    {
        #region Variables.
        // The content file dependant.
        private readonly IContentFile _content;
        // The node represented by this dependency..
        private readonly IFileExplorerNodeVm _node;
        // The parent of this node.
        private IFileExplorerNodeVm _parent;
        #endregion

        #region Events.
        /// <summary>
        /// Event triggered by deleting the node.
        /// </summary>
        public event EventHandler Deleted;
        /// <summary>
        /// Event triggered by renaming the node.
        /// </summary>
        public event EventHandler<ContentFileRenamedEventArgs> Renamed;

        /// <summary>Event triggered if the dependencies list for this file is updated.</summary>
        public event EventHandler DependenciesUpdated;

        /// <summary>Event triggered if the content was closed with the <see cref="IContentFile.CloseContent"/> method.</summary>
        public event EventHandler Closed;
        #endregion

        #region Properties.        
        /// <summary>Property to return the name of this object.</summary>
        public string Name => _node.Name;

        /// <summary>
        /// Property to return the parent node for this node.
        /// </summary>
        public IFileExplorerNodeVm Parent
        {
            get => _parent;
            private set 
            {
                if (_parent == value)
                {
                    return;
                }

                OnPropertyChanging();
                _parent = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return whether to allow child node creation for this node.
        /// </summary>
        public bool AllowChildCreation => false;

        /// <summary>
        /// Property to return the type of node.
        /// </summary>
        public NodeType NodeType => NodeType.Link;

        /// <summary>
        /// Property to return the full path to the node.
        /// </summary>
        public string FullPath => _content.Path;

        /// <summary>
        /// Property to return the path to the linked node.
        /// </summary>
        public string LinkPath => Parent.FullPath + "/" + _content.Name;

        /// <summary>
        /// Property to return the image name to use for the node type.
        /// </summary>
        public string ImageName => Metadata.ContentMetadata.SmallIconID.ToString("N");

        /// <summary>
        /// Property to return whether or not the allow this node to be deleted.
        /// </summary>
        public bool AllowDelete => true;

        /// <summary>Property to return whether this node represents content or not.</summary>
        public bool IsContent => true;

        /// <summary>Property to return the path to the file.</summary>
        string IContentFile.Path => FullPath;

        /// <summary>Property to return the extension for the file.</summary>
        string IContentFile.Extension => Path.GetExtension(_content.Name);

        /// <summary>Property to return the plugin associated with the file.</summary>
        ContentPlugIn IContentFile.ContentPlugIn => Metadata?.ContentMetadata as ContentPlugIn;

        /// <summary>Property to set or return the metadata for the node.</summary>
        public ProjectItemMetadata Metadata
        {
            get => _content.Metadata;
            set =>  _node.Metadata = value;
        }

        /// <summary>Property to set or return whether the node is open for editing.</summary>
        public bool IsOpen
        {
            get => _content.IsOpen;
            set => _content.IsOpen = value;
        }

        /// <summary>Property to return the physical path to the node.</summary>
        public string PhysicalPath => _node.PhysicalPath;

        /// <summary>Property to set or return whether the file has changes.</summary>
        public bool IsChanged
        {
            get => false;
            set
            {
                // Empty by default.
            }
        }

        /// <summary>Property to set or return whether this node is visible.</summary>
        public bool Visible
        {
            get => _node.Visible;
            set => _node.Visible = value;
        }

        /// <summary>Property to set or return whether to mark this node as "cut" or not.</summary>
        public bool IsCut
        {
            get => false;
            set
            {
                // Empty by default.
            }
        }

        /// <summary>Property to return the child nodes for this node.</summary>
        public ObservableCollection<IFileExplorerNodeVm> Children
        {
            get;
        }

        /// <summary>Property to set or return whether the node is in an expanded state or not (if it has children).</summary>
        public bool IsExpanded
        {
            get => false;
            set
            {
                // These node types do not expand or contract.
            }
        }

        /// <summary>Property to return the list of items dependant upon this node</summary>
        public ObservableCollection<IFileExplorerNodeVm> Dependencies => _node.Dependencies;

        /// <summary>Property to return the list of items dependant upon this node</summary>        
        IReadOnlyList<IContentFile> IContentFile.Dependencies => _content.Dependencies;
        #endregion

        #region Methods.
        /// <summary>Handles the PropertyChanged event of the Node control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void Node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Parent):
                    return;
            }

            NotifyPropertyChanged(e.PropertyName);
        }

        /// <summary>Handles the Deleted event of the Content control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Content_Deleted(object sender, EventArgs e)
        {
            Parent.Dependencies.Remove(this);
            EventHandler handler = Deleted;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Handles the Closed event of the Content control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Content_Closed(object sender, EventArgs e)
        {
            EventHandler handler = Closed;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Handles the Renamed event of the Content control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ContentFileRenamedEventArgs"/> instance containing the event data.</param>
        private void Content_Renamed(object sender, ContentFileRenamedEventArgs e) => NotifyPropertyChanged(nameof(Name));

        /// <summary>Handles the DependenciesUpdated event of the Content control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Content_DependenciesUpdated(object sender, EventArgs e)
        {
            EventHandler handler = DependenciesUpdated;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(FileExplorerNodeParameters injectionParameters)
        {
            // Not used.
        }

        /// <summary>Function called to refresh the underlying data for the node.</summary>
        public void Refresh()
        {
            NotifyPropertyChanging(nameof(PhysicalPath));

            try
            {
                _node.Refresh();
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
        public void RenameNode(string newName)
        {
            var args = new ContentFileRenamedEventArgs(Name, newName);

            NotifyPropertyChanging(nameof(Name));

            _node.RenameNode(newName);

            NotifyPropertyChanged(nameof(Name));
            
            EventHandler<ContentFileRenamedEventArgs> handler = Renamed;
            handler?.Invoke(this, args);
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
        public async Task DeleteNodeAsync(Action<IFileExplorerNodeVm> onDeleted = null, CancellationToken? cancelToken = null)
        {
            // Destroy the source node.
            try
            {
                await _node.DeleteNodeAsync(onDeleted, cancelToken);
            }
            finally
            {
                // And always remove us from the dependency list.
                // Worst case is that the source file isn't deleted, and we temporarily lose the linkage.
                Parent.Dependencies.Remove(this);                
            }
        }            

        /// <summary>
        /// Function to copy the file node into another node.
        /// </summary>
        /// <param name="copyNodeData">The data containing information about what to copy.</param>
        /// <returns>The newly copied node.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="copyNodeData"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentMissingException">Thrown when the the <see cref="CopyNodeData.Destination"/> member of the <paramref name="copyNodeData"/> parameter are <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the destination node in <paramref name="copyNodeData"/> is unable to create child nodes.</exception>
        public Task<IFileExplorerNodeVm> CopyNodeAsync(CopyNodeData copyNodeData) => Task.FromResult<IFileExplorerNodeVm>(null);

        /// <summary>
        /// Function to move this node into another node.
        /// </summary>
        /// <param name="copyNodeData">The parameters used for moving the node.</param>
        /// <returns>The udpated node.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="copyNodeData"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentMissingException">Thrown when the <see cref="CopyNodeData.Destination"/> in the <paramref name="copyNodeData"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <see cref="CopyNodeData.Destination"/> in the <paramref name="copyNodeData"/> parameter is unable to create child nodes.</exception>
        public IFileExplorerNodeVm MoveNode(CopyNodeData copyNodeData) => null;

        /// <summary>
        /// Function to export the contents of this node to the physical file system.
        /// </summary>
        /// <param name="exportNodeData">The parameters ued for exporting the node.</param>
        /// <returns>A task for asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="exportNodeData"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentMissingException">Thrown when the the <see cref="CopyNodeData.Destination"/> member of the <paramref name="exportNodeData"/> parameter are <b>null</b>.</exception>
        public Task ExportAsync(ExportNodeData exportNodeData) => _node.ExportAsync(exportNodeData);

        /// <summary>Function to open the file for reading.</summary>
        /// <returns>A stream containing the file data.</returns>
        Stream IContentFile.OpenRead() => _content.OpenRead();

        /// <summary>
        /// Function to open the file for writing.
        /// </summary>
        /// <param name="append">[Optional] <b>true</b> to append data to the end of the file, or <b>false</b> to overwrite.</param>
        /// <returns>A stream to write the file data into.</returns>
        Stream IContentFile.OpenWrite(bool append) => _content.OpenWrite(append);
        
        /// <summary>Function to notify that the metadata should be refreshed.</summary>
        void IContentFile.RefreshMetadata() => NotifyPropertyChanged(nameof(Metadata));

        /// <summary>Function called to refresh the underlying data for the node.</summary>
        void IContentFile.Refresh()
        {
            Refresh();
            NotifyPropertyChanged(nameof(Metadata));
        }

        /// <summary>
        /// Function to notify that the content should close if it's open.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will close the content forcefully, that is, it will not prompt to save and any changes will be lost.
        /// </para>
        /// </remarks>
        void IContentFile.CloseContent() => _content.CloseContent();

        /// <summary>
        /// Function to retrieve the size of the data on the physical file system.
        /// </summary>        
        /// <returns>The size of the data on the physical file system, in bytes.</returns>
        /// <remarks>
        /// <para>
        /// For nodes with children, this will sum up the size of each item in the <see cref="Children"/> list.  For items that do not have children, then only the size of the immediate item is returned.
        /// </para>
        /// </remarks>
        public long GetSizeInBytes() => _node.GetSizeInBytes();

        /// <summary>Function to notify that the parent of this node was moved.</summary>
        /// <param name="newNode">The new node representing this node under the new parent.</param>
        /// <exception cref="ArgumentNullException">newNode</exception>
        public void NotifyParentMoved(IFileExplorerNodeVm newNode)
        {
            if (newNode == null)
            {
                throw new ArgumentNullException(nameof(newNode));
            }
            
            // Not used... yet.
        }

        /// <summary>Function to determine if this node is an ancestor of the specified node.</summary>
        /// <param name="node">The node to look for.</param>
        /// <returns>
        ///   <b>true</b> if the node is an ancestor, <b>false</b> if not.</returns>
        public bool IsAncestorOf(IFileExplorerNodeVm node)
        {
            IFileExplorerNodeVm parentOfNode = node.Parent;

            // We're at the root, so we have nothing.
            if (parentOfNode == null)
            {
                return false;
            }

            while ((parentOfNode != null) && (parentOfNode.Parent != Parent))
            {
                parentOfNode = parentOfNode.Parent;

                if (parentOfNode == this)
                {
                    return true;
                }
            }

            return parentOfNode == this;
        }

        /// <summary>Function called when the associated view is unloaded.</summary>
        public override void OnUnload()
        {
            _node.PropertyChanged -= Node_PropertyChanged;
            _content.Renamed -= Content_Renamed;
            _content.Deleted -= Content_Deleted;
            _content.Closed -= Content_Closed;
            _content.DependenciesUpdated -= Content_DependenciesUpdated;
            base.OnUnload();
        }

        /// <summary>Function to link a content file to be dependant upon this content.</summary>
        /// <param name="child">The child content to link to this content.</param>
        void IContentFile.LinkContent(IContentFile child) => _content.LinkContent(child);

        /// <summary>Function to unlink a content file from being dependant upon this content.</summary>
        /// <param name="child">The child content to unlink from this content.</param>
        void IContentFile.UnlinkContent(IContentFile child) => _content.UnlinkContent(child);

        /// <summary>Function to remove all child dependency links from this content.</summary>
        void IContentFile.ClearLinks() => _content.ClearLinks();

        /// <summary>Function to persist the metadata for content.</summary>
        void IContentFile.SaveMetadata() => _content.SaveMetadata();
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="DependencyNode"/> class.</summary>
        /// <param name="node">The node that this dependency represents.</param>
        public DependencyNode(IFileExplorerNodeVm parent, IFileExplorerNodeVm node)
        {
            _parent = parent;
            _content = (IContentFile)node;
            _node = node;
            _node.PropertyChanged += Node_PropertyChanged;
            Children = new ObservableCollection<IFileExplorerNodeVm>();
            _content.Renamed += Content_Renamed;
            _content.Deleted += Content_Deleted;
            _content.Closed += Content_Closed;
            _content.DependenciesUpdated += Content_DependenciesUpdated;
        }
        #endregion
    }
}
