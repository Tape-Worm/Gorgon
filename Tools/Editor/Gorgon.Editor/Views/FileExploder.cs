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
// Created: September 4, 2018 3:13:35 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.ViewModels;
using System.Diagnostics;
using Gorgon.Collections;
using Gorgon.Editor.Content;
using System.Threading.Tasks;
using Gorgon.UI;

namespace Gorgon.Editor.Views
{
    /// <summary>
    /// The file explorer used to manipulate files for the project.
    /// </summary>
    internal partial class FileExploder
        : EditorBaseControl, IDataContext<IFileExplorerVm>
    {
        #region Events.
        /// <summary>
        /// Event triggered when a rename operation is completed.
        /// </summary>
        public event EventHandler RenameEnd;
        /// <summary>
        /// Event triggered when a rename operation is started.
        /// </summary>
        public event EventHandler RenameBegin;
        #endregion

        #region Variables.
        // A list of file system nodes linked to our actual tree nodes.
        private readonly Dictionary<KryptonTreeNode, IFileExplorerNodeVm> _nodeLinks = new Dictionary<KryptonTreeNode, IFileExplorerNodeVm>();
        // The reverse of the above.
        private readonly Dictionary<IFileExplorerNodeVm, KryptonTreeNode> _revNodeLinks = new Dictionary<IFileExplorerNodeVm, KryptonTreeNode>();
        // The node being renamed.
        private IFileExplorerNodeVm _renameNode;
        // Font used for excluded items.
        private Font _excludedFont;
        // Font used for open items.
        private Font _openFont;
        // The context handler for clipboard operations.
        private IClipboardHandler _clipboardContext;
        // A drag and drop handler interface.
        private IDragDropHandler<IFileExplorerNodeDragData> _nodeDragDropHandler;
        // A drag and drop handler interface.
        private IDragDropHandler<ViewModels.IExplorerFilesDragData> _explorerDragDropHandler;
        // The drag hilight background color.
        private Color _dragBackColor;
        // The drag hilight foreground color.
        private Color _dragForeColor;
        // The drag data for dropping files from explorer.
        private ExplorerFileDragData _explorerDragData;
        // The nodes returned from a search.
        private IReadOnlyList<IFileExplorerNodeVm> _searchNodes;
		// The background color for the list view header.
        private readonly SolidBrush _listViewHeaderBackColor;
        // The background color for the list view header.
        private readonly SolidBrush _listViewHeaderForeColor;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether or not the control is in the middle of a renaming operation.
        /// </summary>
        [Browsable(false)]
        public bool IsRenaming => _renameNode != null;

        /// <summary>
        /// Property to return the data context assigned to this view.
        /// </summary>
        [Browsable(false)]
        public IFileExplorerVm DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to locate an image index for a node based on the node image name.
        /// </summary>
        /// <param name="node">The node to evaluate.</param>
        /// <returns>The image index of the node, or -1 if not found.</returns>
        private int FindImageIndexFromNode(IFileExplorerNodeVm node)
        {
            const string defaultDirKey = "folder_20x20.png";
            const string defaultFileKey = "generic_file_20x20.png";

            int imageIndex = TreeImages.Images.IndexOfKey(node.ImageName);

            if (imageIndex == -1)
            {
                imageIndex = node.NodeType == NodeType.Directory
                                    ? TreeImages.Images.IndexOfKey(defaultDirKey)
                                    : TreeImages.Images.IndexOfKey(defaultFileKey);
            }

            return imageIndex;
        }

        /// <summary>
        /// Function to unassign the linkages for a node.
        /// </summary>
        /// <param name="node"></param>
        private void RemoveNodeLinks(IFileExplorerNodeVm node)
        {
            if (node == null)
            {
                return;
            }

            UnassignNodeEvents(node.Dependencies);
            UnassignNodeEvents(node.Children);            
            node.PropertyChanged -= Node_PropertyChanged;
            _revNodeLinks.Remove(node);
        }

        /// <summary>
        /// Function to delete a node from the tree.
        /// </summary>
        /// <param name="deleteNode">The node to delete.</param>
        private void RemoveNode(IFileExplorerNodeVm deleteNode)
        {
            if (_searchNodes != null)
            {
				// If the search pane is active, and the node being deleted, or any of its children are in the list, then we should remove it.
                ListViewItem searchItem = ListSearchResults.Items.OfType<ListViewItem>().FirstOrDefault(item => item.Tag == deleteNode);

                if (searchItem != null)
                {
                    ListSearchResults.Items.Remove(searchItem);
                }

                foreach (IFileExplorerNodeVm childNode in deleteNode.Children.Traverse(n => n.Children))
                {
                    searchItem = ListSearchResults.Items.OfType<ListViewItem>().FirstOrDefault(item => item.Tag == deleteNode);
                    if (searchItem != null)
                    {
                        ListSearchResults.Items.Remove(searchItem);
                    }
                }
            }

            // This node is not on the tree, so just leave.  There's nothing we can do here.
            if (!_revNodeLinks.TryGetValue(deleteNode, out KryptonTreeNode deleteTreeNode))
            {
                // If we have a collapsed parent node, we won't have a link for the item in our tree node links.
                // So, in that case, get the parent node and remove any dummy nodes (if the parent is collapsed) and we're the only child.
                if ((_revNodeLinks.TryGetValue(deleteNode.Parent, out KryptonTreeNode parentTreeNode))
                    && (!parentTreeNode.IsExpanded)
                    && (deleteNode.Parent.Children.Count == 0))
                {
                    parentTreeNode.Nodes.Clear();
                }

                return;
            }

            // Let us control next/prev node selection.
            TreeFileSystem.AfterSelect -= TreeFileSystem_AfterSelect;

            try
            {
                // Immediately turn off events for this node.  We don't need some phantom event ruining everything.
                RemoveNodeLinks(deleteNode);

                var parentTreeNode = deleteTreeNode.Parent as KryptonTreeNode;
                var nextSibling = deleteTreeNode.NextNode as KryptonTreeNode;
#pragma warning disable IDE0019 // Use pattern matching  Seriously, this breaks so very fucking badly.  Did no one test these idiotic suggestions??
                var prevSibling = deleteTreeNode.PrevNode as KryptonTreeNode;
#pragma warning restore IDE0019 // Use pattern matching

                TreeNodeCollection nodes = parentTreeNode?.Nodes ?? TreeFileSystem.Nodes;
                nodes.Remove(deleteTreeNode);

                // Remove from linkage.
                _nodeLinks.Remove(deleteTreeNode);

                // Figure out who gets selected next.
                if ((parentTreeNode != null) && (parentTreeNode.Nodes.Count == 0))
                {
                    SelectNode(parentTreeNode);
                    parentTreeNode.EnsureVisible();
                    parentTreeNode.Collapse();
                    return;
                }

                if (prevSibling != null)
                {
                    SelectNode(prevSibling);
                    prevSibling.EnsureVisible();
                    return;
                }

                SelectNode(nextSibling);
                nextSibling?.EnsureVisible();
            }
            finally
            {
                TreeFileSystem.AfterSelect += TreeFileSystem_AfterSelect;
            }
        }

        /// <summary>
        /// Function to add a new node to the tree.
        /// </summary>
        /// <param name="newNode">The new node to add.</param>
        private void AddNode(IFileExplorerNodeVm newNode)
        {
            if (!newNode.Visible)
            {
                return;
            }

            // Always add the new node to the search list (when it's active), even if it doesn't meet the criteria.
            // Otherwise, it'd be confusing to not see the new node after creation.  This is how Visual Studio's 
            // solution explorer tree works as well.
            if (_searchNodes != null)
            {
                ListViewItem item = null;

                if (ListSearchResults.Items.ContainsKey(newNode.FullPath))
                {
                    item = ListSearchResults.Items[newNode.FullPath];
                    item.Tag = newNode;
                    UpdateSearchItemVisualState(item, newNode);
                }
                else
                {
                    item = new ListViewItem
                    {
                        Name = newNode.FullPath,
                        Text = newNode.Name,
                        Tag = newNode
                    };

                    UpdateSearchItemVisualState(item, newNode);

                    item.SubItems.Add(new ListViewItem.ListViewSubItem
                    {
                        Text = newNode.FullPath,
                        ForeColor = Color.Silver
                    });

                    ListSearchResults.Items.Add(item);
                }
            }

            // Locate the tree node that has our parent node.
            KryptonTreeNode parentTreeNode = null;
            IFileExplorerNodeVm parent = newNode.Parent ?? DataContext.RootNode;			

            if (parent != DataContext.RootNode)
            {
                _revNodeLinks.TryGetValue(parent, out parentTreeNode);
            }
            TreeNodeCollection nodes = parentTreeNode?.Nodes ?? TreeFileSystem.Nodes;

            KryptonTreeNode newTreeNode = null;

            if (((parentTreeNode == null) && (parent == DataContext.RootNode)) || (parentTreeNode?.IsExpanded ?? false))
            {
                newNode.PropertyChanged += Node_PropertyChanged;
                newNode.Children.CollectionChanged += Nodes_CollectionChanged;
                newNode.Dependencies.CollectionChanged += Nodes_CollectionChanged;

                int imageIndex = FindImageIndexFromNode(newNode);
                newTreeNode = new KryptonTreeNode(newNode.Name, imageIndex, imageIndex);

                UpdateNodeVisualState(newTreeNode, newNode);

                _nodeLinks[newTreeNode] = newNode;
                _revNodeLinks[newNode] = newTreeNode;
                nodes.Add(newTreeNode);
                SelectNode(newTreeNode);
                newTreeNode.EnsureVisible();
                return;
            }

            if (parentTreeNode.Nodes.Count == 0)
            {
                parentTreeNode.Nodes.Add(new KryptonTreeNode("DUMMY_NODE_SHOULD_NOT_SEE_ME"));
            }

            if (!parentTreeNode.IsExpanded)
            {
                return;
            }            

            if (!_revNodeLinks.TryGetValue(newNode, out newTreeNode))
            {
                return;
            }

            SelectNode(newTreeNode);
            newTreeNode.EnsureVisible();
        }

        /// <summary>
        /// Function to remove the nodes from the tree.
        /// </summary>
        /// <param name="nodes">The node collection being cleared.</param>
        private void ClearNodeBranch(ObservableCollection<IFileExplorerNodeVm> nodes)
        {
            // Find the owner of the collection.
            IFileExplorerNodeVm node = _revNodeLinks.FirstOrDefault(item => (item.Key.Children == nodes) || (item.Key.Dependencies == nodes)).Key;

            if ((node == null) && (nodes != DataContext.RootNode.Children))
            {
                return;
            }

            nodes.CollectionChanged -= Nodes_CollectionChanged;            

            if ((node != null) && (_revNodeLinks.TryGetValue(node, out KryptonTreeNode treeNode)))
            {
                SelectNode(treeNode);

                if (treeNode.IsExpanded)
                {
                    // When the node is collapsed, all cached child nodes will be removed, and all events unassigned.
                    // Thus, there is no need for us to manually clean up here.
                    treeNode.Collapse();
                }
                else
                {
                    // Otherwise, if the node is collapsed, then just reassign the event.
                    nodes.CollectionChanged += Nodes_CollectionChanged;
                }

                treeNode.Nodes.Clear();
            }
            else
            {
                // Ensure all nodes are unassigned from event handlers.
                // We do this because the node list is cleared at this point, and the only copy of remaining visible nodes 
                // is stored in the cache.  If we do not do this, we end up with phantom events until the objects are 
                // garbage collected.
                foreach (IFileExplorerNodeVm cachedNode in _revNodeLinks.Keys)
                {
                    UnassignNodeEvents(cachedNode.Children);
                    UnassignNodeEvents(cachedNode.Dependencies);

                    if (cachedNode != DataContext.RootNode)
                    {
                        cachedNode.PropertyChanged -= Node_PropertyChanged;
                    }                    
                }

                _revNodeLinks.Clear();
                _nodeLinks.Clear();
                TreeFileSystem.Nodes.Clear();
                
                AssignNodeEvents(DataContext.RootNode.Children);
            }

            ValidateMenuItems(DataContext);
        }

        /// <summary>
        /// Handles the CollectionChanged event of the Nodes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void Nodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddNode(e.NewItems.OfType<IFileExplorerNodeVm>().First());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveNode(e.OldItems.OfType<IFileExplorerNodeVm>().First());
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ClearNodeBranch((ObservableCollection<IFileExplorerNodeVm>)sender);
                    break;
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the Node control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void Node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = (IFileExplorerNodeVm)sender;

            Debug.Assert(node != null, "Node property changed, but no actual node was present.");

            _revNodeLinks.TryGetValue(node, out KryptonTreeNode treeNode);
            ListViewItem searchItem = ListSearchResults.Items.OfType<ListViewItem>().FirstOrDefault(item => item.Tag == node);			

            switch (e.PropertyName)
            {
                case nameof(IFileExplorerNodeVm.Name):
                    if (treeNode != null)
                    {
                        treeNode.Text = node.Name;
                    }

                    if (searchItem != null)
                    {
                        searchItem.Text = node.Name;
                    }
                    break;
                case nameof(IFileExplorerNodeVm.IsExpanded):
                    if (treeNode != null)
                    {
                        if (node.IsExpanded)
                        {
                            treeNode.Expand();
                        }
                        else
                        {
                            treeNode.Collapse();
                        }
                    }
                    break;
                case nameof(IFileExplorerNodeVm.Visible):
                    UpdateNodeVisibility(DataContext, node, treeNode);
                    break;
                case nameof(IFileExplorerNodeVm.Metadata):
                case nameof(IFileExplorerNodeVm.IsChanged):
                case nameof(IFileExplorerNodeVm.IsOpen):
                case nameof(IFileExplorerNodeVm.ImageName):
                case nameof(IFileExplorerNodeVm.IsCut):
                    if (treeNode != null)
                    {
                        UpdateNodeVisualState(treeNode, node);
                    }

                    if (searchItem != null)
                    {
                        UpdateSearchItemVisualState(searchItem, node);
                    }
                    break;
            }
        }

        /// <summary>
        /// Function to show or hide a node (and its children) based on node visibility.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        /// <param name="node">The node that has had its visibility state changed.</param>
        /// <param name="treeNode">The tree node affected by the update.</param>
        private void UpdateNodeVisibility(IFileExplorerVm dataContext, IFileExplorerNodeVm node, KryptonTreeNode treeNode)
        {
            // Our job is already done.
            if (((!node.Visible) && (treeNode == null))
                || ((node.Visible) && (treeNode != null))
                || (node.Metadata == null)
                || (dataContext == null))
            {
                return;
            }

            TreeFileSystem.BeginUpdate();

            try
            {
                if (!node.Visible)
                {
                    treeNode?.Remove();
                    return;
                }

                var treeNodes = new List<KryptonTreeNode>();                
                for (IFileExplorerNodeVm parentNode = node; (parentNode.Visible) && (parentNode != DataContext.RootNode); parentNode = parentNode.Parent)
                {
                    // If the node isn't populated yet, then do so now.
                    if (!_revNodeLinks.TryGetValue(parentNode, out KryptonTreeNode parentTreeNode))
                    {
                        int imageIndex = FindImageIndexFromNode(parentNode);

                        parentTreeNode = new KryptonTreeNode(parentNode.Name)
                        {
                            ImageIndex = imageIndex,
                            SelectedImageIndex = imageIndex
                        };

                        UpdateNodeVisualState(parentTreeNode, parentNode);
                        parentTreeNode.Nodes.Clear();
                        _revNodeLinks[parentNode] = parentTreeNode;
                        _nodeLinks[parentTreeNode] = parentNode;
                    }
                    treeNodes.Add(parentTreeNode);
                }

                if (treeNodes.Count == 0)
                {
                    return;
                }

                TreeNodeCollection parentNodes = TreeFileSystem.Nodes;

                for (int i = treeNodes.Count - 1; i >= 0; --i)
                {
                    KryptonTreeNode theNode = treeNodes[i];

                    if (!parentNodes.Contains(theNode))
                    {
                        parentNodes.Add(theNode);
                    }
                                        
                    theNode.Nodes.Clear();
                    parentNodes = theNode.Nodes;
                    theNode.Expand();
                }
            }
            finally
            {
                TreeFileSystem.EndUpdate();
            }
        }

        /// <summary>
        /// Handles the Click event of the MenuItemExportTo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemExportTo_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            IFileExplorerNodeVm selectedNode;

            if (_searchNodes == null)
            {
                if (TreeFileSystem.SelectedNode is KryptonTreeNode treeNode)
                {
                    if (!_nodeLinks.TryGetValue(treeNode, out selectedNode))
                    {
                        return;
                    }
                }
                else
                {
                    selectedNode = DataContext.RootNode;
                }
            }
            else
            {
                if (ListSearchResults.SelectedItems.Count == 0)
                {
                    selectedNode = DataContext.RootNode;
                }
                else
                {
                    selectedNode = ListSearchResults.SelectedItems[0].Tag as IFileExplorerNodeVm;
                }
            }

            if ((DataContext?.ExportNodeToCommand == null)
                || (!DataContext.ExportNodeToCommand.CanExecute(selectedNode)))
            {
                return;
            }

            DataContext.ExportNodeToCommand.Execute(selectedNode);
        }

        /// <summary>
        /// Handles the Click event of the ItemRename control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ItemRename_Click(object sender, EventArgs e)
        {
            if (_searchNodes == null)
            {
                TreeFileSystem.SelectedNode?.BeginEdit();
                return;
            }            

            if (ListSearchResults.SelectedItems.Count != 1)
            {
                return;
            }

            ListSearchResults.SelectedItems[0].BeginEdit();
        }

        /// <summary>
        /// Handles the Click event of the ItemDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ItemDelete_Click(object sender, EventArgs e)
        {
            IFileExplorerNodeVm node;

            // If we're renaming, then ignore our delete.
            if (_searchNodes == null)
            {
                if ((TreeFileSystem.SelectedNode != null) && (TreeFileSystem.SelectedNode.IsEditing))
                {
                    return;
                }

                if (!_nodeLinks.TryGetValue((KryptonTreeNode)TreeFileSystem.SelectedNode, out node))
                {
                    return;
                }
            }
            else
            {
                if (ListSearchResults.SelectedItems.Count == 0)
                {
                    return;
                }

                node = (IFileExplorerNodeVm)ListSearchResults.SelectedItems[0].Tag;
            }


            var args = new DeleteNodeArgs(node);

            if ((DataContext?.DeleteNodeCommand == null)
                || (!DataContext.DeleteNodeCommand.CanExecute(args)))
            {
                return;
            }

            DataContext.DeleteNodeCommand.Execute(args);
        }

        /// <summary>
        /// Handles the Click event of the MenuItemCut control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemCut_Click(object sender, EventArgs e)
        {
            if (_clipboardContext == null)
            {
                return;
            }

            _clipboardContext.Cut();
        }

        /// <summary>
        /// Handles the Click event of the MenuItemCopy control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemCopy_Click(object sender, EventArgs e)
        {
            if (_clipboardContext == null)
            {
                return;
            }

            _clipboardContext.Copy();
        }

        /// <summary>
        /// Handles the Click event of the MenuItemPaste control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemPaste_Click(object sender, EventArgs e)
        {
            if (_clipboardContext == null)
            {
                return;
            }

            _clipboardContext.Paste();
        }

        /// <summary>
        /// Handles the Click event of the MenuItemMoveTo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemMoveTo_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            if (!(MenuCopyMove.Tag is IFileExplorerNodeDragData data))
            {
                MenuCopyMove.Tag = null;
                return;
            }

            var args = new CopyNodeArgs(data.Node, data.TargetNode);

            if (!DataContext.MoveNodeCommand.CanExecute(args))
            {
                return;
            }
            DataContext.MoveNodeCommand.Execute(args);

            MenuCopyMove.Tag = null;
        }

        /// <summary>
        /// Handles the Click event of the MenuItemCopyTo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemCopyTo_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            if (!(MenuCopyMove.Tag is IFileExplorerNodeDragData data))
            {
                MenuCopyMove.Tag = null;
                return;
            }

            var args = new CopyNodeArgs(data.Node, data.TargetNode);

            if (!DataContext.CopyNodeCommand.CanExecute(args))
            {
                return;
            }
            DataContext.CopyNodeCommand.Execute(args);

            MenuCopyMove.Tag = null;
        }

        /// <summary>
        /// Handles the Click event of the ItemCreateDirectory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ItemCreateDirectory_Click(object sender, EventArgs e)
        {
            if (DataContext?.CreateNodeCommand == null)
            {
                return;
            }

            IFileExplorerNodeVm parentNode = null;

            if (TreeFileSystem.SelectedNode != null)
            {
                _nodeLinks.TryGetValue((KryptonTreeNode)TreeFileSystem.SelectedNode, out parentNode);
            }

            var args = new CreateNodeArgs(parentNode);

            if (!DataContext.CreateNodeCommand.CanExecute(args))
            {
                return;
            }

            DataContext.CreateNodeCommand.Execute(args);

            if (args.Cancel)
            {
                return;
            }

            Debug.Assert(TreeFileSystem.SelectedNode != null, "The node needs to be selected after adding.");

            // Default to renaming once the node is added.
            TreeFileSystem.SelectedNode.BeginEdit();
        }

        /// <summary>
        /// Function to validate the menu items for the tree context menu.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void ValidateMenuItems(IFileExplorerVm dataContext)
        {
            if (dataContext == null)
            {
                foreach (ToolStripMenuItem item in MenuOptions.Items.OfType<ToolStripMenuItem>())
                {
                    item.Available = false;
                }

                return;
            }

            MenuItemExportTo.Enabled = dataContext.RootNode.Children.Count > 0;

            if (dataContext.SelectedNode == null)
            {
                MenuSepContent.Available = MenuItemOpenContent.Available = false;                
                MenuItemExportTo.Available = dataContext.SearchResults == null;
                MenuItemCopy.Available = false;
                MenuItemCut.Available = false;
                MenuSepEdit.Available = MenuItemPaste.Available = _clipboardContext?.CanPaste() ?? false;
                MenuItemPaste.Enabled = _clipboardContext?.CanPaste() ?? false;
                MenuSepOrganize.Available = false;
                MenuSepNew.Available = MenuItemCreateDirectory.Available = dataContext?.CreateNodeCommand?.CanExecute(null) ?? false;                
                MenuItemDelete.Available = false;
                MenuItemRename.Available = false;
                
                return;
            }

            MenuSepEdit.Available = (_clipboardContext != null) && ((_clipboardContext.CanPaste()) || (_clipboardContext.CanCut()) || (_clipboardContext.CanCopy()));
            MenuSepOrganize.Available = true;
            MenuItemRename.Available = true;
            MenuItemExportTo.Available = true;

            MenuItemCopy.Available = _clipboardContext?.CanCopy() ?? false;
            MenuItemCut.Available = _clipboardContext?.CanCut() ?? false;
            MenuItemPaste.Available = _clipboardContext != null;
            MenuItemPaste.Enabled = _clipboardContext?.CanPaste() ?? false;

            MenuItemCreateDirectory.Available = dataContext.CreateNodeCommand?.CanExecute(null) ?? false;
            MenuItemDelete.Available = dataContext.DeleteNodeCommand?.CanExecute(null) ?? false;

            MenuSepContent.Available = MenuItemOpenContent.Available = dataContext.OpenContentFileCommand?.CanExecute(dataContext.SelectedNode as IContentFile) ?? false;

            MenuSepNew.Visible = MenuItemCreateDirectory.Available;
        }


        /// <summary>Handles the Click event of the MenuItemOpenContent control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MenuItemOpenContent_Click(object sender, EventArgs e)
        {
            var selectedFile = DataContext?.SelectedNode as IContentFile;

            if ((DataContext?.OpenContentFileCommand == null) || (!DataContext.OpenContentFileCommand.CanExecute(selectedFile)))
            {
                return;
            }

            DataContext.OpenContentFileCommand.Execute(selectedFile);
        }

        /// <summary>
        /// Handles the Opening event of the MenuOperations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void MenuOperations_Opening(object sender, CancelEventArgs e) => ValidateMenuItems(DataContext);

        /// <summary>
        /// Function to perform a node selection operation.
        /// </summary>
        /// <param name="treeNode">The node that is selected.</param>
        private void SelectNode(KryptonTreeNode treeNode)
        {
            IFileExplorerNodeVm node = null;
            TreeFileSystem.SelectedNode = treeNode;

            if (DataContext?.SelectNodeCommand == null)
            {
                return;
            }

            if (treeNode != null)
            {
                _nodeLinks.TryGetValue(treeNode, out node);                                
            }

            if (!DataContext.SelectNodeCommand.CanExecute(node))
            {
                return;
            }

            DataContext.SelectNodeCommand.Execute(node);

            ValidateMenuItems(DataContext);
        }

        /// <summary>
        /// Function to import files into the file explorer from an external source.
        /// </summary>
        /// <returns>A task for asynchronous operation.</returns>
        public async Task ImportFilesAsync()
        {
            if ((DataContext?.ImportIntoNodeCommand == null)
                || (!DataContext.ImportIntoNodeCommand.CanExecute(DataContext.SelectedNode ?? DataContext.RootNode)))
            {
                return;
            }

            IFileExplorerNodeVm currentNode = DataContext.SelectedNode ?? DataContext.RootNode;

            await DataContext.ImportIntoNodeCommand.ExecuteAsync(currentNode);
        }

        /// <summary>
        /// Function to handle explorer file data.
        /// </summary>
        /// <param name="e">The event parameters to use.</param>
        private void ExplorerFiles_DragDrop(DragEventArgs e)
        {
            if ((_explorerDragDropHandler == null) || (_explorerDragData == null))
            {
                return;
            }

            if (_explorerDragData.TargetTreeNode != null)
            {
                UpdateNodeVisualState(_explorerDragData.TargetTreeNode, _explorerDragData.TargetNode);
                _explorerDragData.TargetTreeNode.BackColor = Color.Empty;
            }

            // We're not allowed here, so just cancel the operation.
            if (e.Effect == DragDropEffects.None)
            {
                return;
            }

            SelectNode(_explorerDragData.TargetTreeNode);

            _explorerDragDropHandler.Drop(_explorerDragData);
        }

        /// <summary>
        /// Handles the DragDrop event of the TreeFileSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void TreeFileSystem_DragDrop(object sender, DragEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

#pragma warning disable IDE0019 // Use pattern matching (it breaks the code)
            var data = e.Data.GetData(typeof(TreeNodeDragData)) as TreeNodeDragData;
#pragma warning restore IDE0019 // Use pattern matching

            if ((data == null)
                && (!e.Data.GetDataPresent(DataFormats.FileDrop)))
            {
                return;
            }

            if (data == null)
            {
                ExplorerFiles_DragDrop(e);
                return;
            }

            if (data.TargetTreeNode != null)
            {
                UpdateNodeVisualState(data.TargetTreeNode, data.TargetNode);
                data.TargetTreeNode.BackColor = Color.Empty;                
            }

            // We're not allowed here, so just cancel the operation.
            if (e.Effect == DragDropEffects.None)
            {
                return;
            }

            SelectNode(data.TargetTreeNode);

            if (data.DragOperation == (DragOperation.Copy | DragOperation.Move))
            {
                MenuCopyMove.Tag = data;
                MenuCopyMove.Show(this, PointToClient(Cursor.Position));
                return;
            }

            _nodeDragDropHandler?.Drop(data);
        }

        /// <summary>
        /// Handles the DragLeave event of the TreeFileSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TreeFileSystem_DragLeave(object sender, EventArgs e)
        {
            if (_explorerDragData == null)
            {
                return;
            }

            if (_explorerDragData.TargetTreeNode != null)
            {
                UpdateNodeVisualState(_explorerDragData.TargetTreeNode, _explorerDragData.TargetNode);
                _explorerDragData.TargetTreeNode.BackColor = Color.Empty;
            }

            _explorerDragData = null;
        }

        /// <summary>
        /// Handles the DragEnter event of the TreeFileSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void TreeFileSystem_DragEnter(object sender, DragEventArgs e)
        {
            if ((!e.Data.GetDataPresent(DataFormats.FileDrop, false))
                || (_explorerDragDropHandler == null))
            {
                return;
            }

            _explorerDragData = new ExplorerFileDragData(e.Data.GetData(DataFormats.FileDrop, false) as string[], DragOperation.Copy);

            TreeViewHitTestInfo hitResult = TreeFileSystem.HitTest(TreeFileSystem.PointToClient(new Point(e.X, e.Y)));

            var targetTreeNode = hitResult.Node as KryptonTreeNode;
            IFileExplorerNodeVm targetNode;

            if (targetTreeNode == null)
            {
                targetNode = DataContext.RootNode;
            }
            else
            {
                if (!_nodeLinks.TryGetValue((KryptonTreeNode)hitResult.Node, out targetNode))
                {
                    return;
                }

                targetTreeNode.BackColor = _dragBackColor;
                targetTreeNode.ForeColor = _dragForeColor;
            }

            _explorerDragData.TargetNode = targetNode;
            _explorerDragData.TargetTreeNode = targetTreeNode;

            e.Effect = DragDropEffects.Copy;
        }

        /// <summary>
        /// Function to handle explorer file data.
        /// </summary>
        /// <param name="e">The event parameters to use.</param>
        private void ExplorerFiles_DragOver(DragEventArgs e)
        {
            if ((_explorerDragDropHandler == null) || (_explorerDragData == null))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            if (_explorerDragData.TargetTreeNode != null)
            {
                UpdateNodeVisualState(_explorerDragData.TargetTreeNode, _explorerDragData.TargetNode);
                _explorerDragData.TargetTreeNode.BackColor = Color.Empty;
            }

            TreeViewHitTestInfo hitResult = TreeFileSystem.HitTest(TreeFileSystem.PointToClient(new Point(e.X, e.Y)));

            var targetTreeNode = hitResult.Node as KryptonTreeNode;
            IFileExplorerNodeVm targetNode;

            if (targetTreeNode == null)
            {
                targetNode = DataContext.RootNode;
            }
            else
            {
                if (!_nodeLinks.TryGetValue((KryptonTreeNode)hitResult.Node, out targetNode))
                {
                    return;
                }

                targetTreeNode.BackColor = _dragBackColor;
                targetTreeNode.ForeColor = _dragForeColor;
            }

            _explorerDragData.TargetTreeNode = targetTreeNode;
            _explorerDragData.TargetNode = targetNode;

            if (!_explorerDragDropHandler.CanDrop(_explorerDragData))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.Copy;
        }

        /// <summary>Handles the KeyUp event of the ListSearchResults control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void ListSearchResults_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F2:
                    if ((ListSearchResults.SelectedItems.Count == 1) && (_renameNode == null))
                    {
                        ListSearchResults.SelectedItems[0].BeginEdit();
                    }
                    break;
                case Keys.Delete:
                    if ((ListSearchResults.SelectedItems.Count == 1) && (_renameNode == null))
                    {						
                        MenuItemDelete.PerformClick();
                    }
                    break;
            }
        }

        /// <summary>Handles the BeforeLabelEdit event of the ListSearchResults control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="LabelEditEventArgs"/> instance containing the event data.</param>
        private void ListSearchResults_BeforeLabelEdit(object sender, LabelEditEventArgs e)
        {
            // Is this even possible?
            ListViewItem item = ListSearchResults.Items[e.Item];
            _renameNode = (IFileExplorerNodeVm)item.Tag;

            EventHandler handler = RenameBegin;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Handles the AfterLabelEdit event of the ListSearchResults control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="LabelEditEventArgs"/> instance containing the event data.</param>
        private void ListSearchResults_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            ListSearchResults.BeforeLabelEdit -= ListSearchResults_BeforeLabelEdit;
            ListSearchResults.AfterLabelEdit -= ListSearchResults_AfterLabelEdit;

            try
            {
                // If we didn't change the name, or there's no node being renamed, then leave.
                if ((DataContext?.RenameNodeCommand == null) || (_renameNode == null))
                {
                    e.CancelEdit = true;
                    return;
                }

                string oldName = _renameNode.Name;
                var args = new FileExplorerNodeRenameArgs(_renameNode, oldName, e.Label);

                if (!DataContext.RenameNodeCommand.CanExecute(args))
                {
                    e.CancelEdit = true;
                    return;
                }

                DataContext.RenameNodeCommand.Execute(args);

                e.CancelEdit = args.Cancel;
            }
            finally
            {
                // We're no longer renaming anything.
                _renameNode = null;

                EventHandler handler = RenameEnd;
                handler?.Invoke(this, EventArgs.Empty);

                ListSearchResults.BeforeLabelEdit += ListSearchResults_BeforeLabelEdit;
                ListSearchResults.AfterLabelEdit += ListSearchResults_AfterLabelEdit;
            }
        }

        /// <summary>Handles the DoubleClick event of the ListSearchResults control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ListSearchResults_DoubleClick(object sender, EventArgs e)
        {
            ListViewHitTestInfo info = ListSearchResults.HitTest(ListSearchResults.PointToClient(Cursor.Position));

            if (info?.Item == null)
            {
                return;
            }

            var contentFile = info.Item.Tag as IContentFile;

            if ((DataContext?.OpenContentFileCommand == null) || (!DataContext.OpenContentFileCommand.CanExecute(contentFile)))
            {
                return;
            }

            DataContext.OpenContentFileCommand.Execute(contentFile);
        }

        /// <summary>Handles the SelectedIndexChanged event of the ListSearchResults control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ListSearchResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            IFileExplorerNodeVm node = null;

            if (DataContext?.SelectNodeCommand == null)
            {
                return;
            }

            ListViewItem item = null;

            if (ListSearchResults.SelectedItems.Count > 0)
            {
                item = ListSearchResults.SelectedItems[0];
            }

            if (item != null)
            {
                node = (IFileExplorerNodeVm)item.Tag;
            }

            if (!DataContext.SelectNodeCommand.CanExecute(node))
            {
                return;
            }

            DataContext.SelectNodeCommand.Execute(node);

            ValidateMenuItems(DataContext);
        }

        /// <summary>Handles the DrawColumnHeader event of the ListSearchResults control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DrawListViewColumnHeaderEventArgs"/> instance containing the event data.</param>
        private void ListSearchResults_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.PaintNcHeader(_listViewHeaderBackColor);

            if (_listViewHeaderForeColor != null)
            {
                TextRenderer.DrawText(e.Graphics,
                    e.Header.Text,
                    ListSearchResults.Font,
                    e.Bounds,
                    Color.FromArgb(ListSearchResults.ForeColor.R / 2, ListSearchResults.ForeColor.G / 2, ListSearchResults.ForeColor.B / 2),
					TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
            }
        }

        /// <summary>Handles the DrawItem event of the ListSearchResults control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DrawListViewItemEventArgs"/> instance containing the event data.</param>
        private void ListSearchResults_DrawItem(object sender, DrawListViewItemEventArgs e) => e.DrawDefault = false;

        /// <summary>Handles the DrawSubItem event of the ListSearchResults control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DrawListViewSubItemEventArgs"/> instance containing the event data.</param>
        private void ListSearchResults_DrawSubItem(object sender, DrawListViewSubItemEventArgs e) => e.DrawDefault = true;

        /// <summary>Handles the NodeMouseDoubleClick event of the TreeFileSystem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [TreeNodeMouseClickEventArgs] instance containing the event data.</param>
        private void TreeFileSystem_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            _nodeLinks.TryGetValue((KryptonTreeNode)e.Node, out IFileExplorerNodeVm node);

            var contentFile = node as IContentFile;

            if ((DataContext?.OpenContentFileCommand == null) || (!DataContext.OpenContentFileCommand.CanExecute(contentFile)))
            {
                return;
            }

            DataContext.OpenContentFileCommand.Execute(contentFile);
        }

        /// <summary>
        /// Handles the DragOver event of the TreeFileSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void TreeFileSystem_DragOver(object sender, DragEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

#pragma warning disable IDE0019 // Use pattern matching (it breaks the code)
            var data = e.Data.GetData(typeof(TreeNodeDragData)) as TreeNodeDragData;
#pragma warning restore IDE0019 // Use pattern matching

            if ((data == null)
                && (!e.Data.GetDataPresent(DataFormats.FileDrop)))
            {
                return;
            }

            if (data == null)
            {
                ExplorerFiles_DragOver(e);
                return;
            }

            if (data.TargetTreeNode != null)
            {
                // Reset the background color.
                UpdateNodeVisualState(data.TargetTreeNode, data.TargetNode);
                data.TargetTreeNode.BackColor = Color.Empty;
            }

            // We won't allow links to be dragged into other nodes.
            if (data.Node.NodeType == NodeType.Link)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            TreeViewHitTestInfo hitResult = TreeFileSystem.HitTest(TreeFileSystem.PointToClient(new Point(e.X, e.Y)));
            IFileExplorerNodeVm targetNode;

            data.TargetTreeNode = hitResult.Node as KryptonTreeNode;

            if (hitResult.Node == null)
            {
                targetNode = DataContext.RootNode;
            }
            else
            {
                if (!_nodeLinks.TryGetValue((KryptonTreeNode)hitResult.Node, out targetNode))
                {
                    return;
                }

                data.TargetTreeNode.BackColor = _dragBackColor;
                data.TargetTreeNode.ForeColor = _dragForeColor;
            }

            data.TargetNode = targetNode;

            if (((e.KeyState & 8) == 8) && (data.DragOperation == DragOperation.Move))
            {
                data.DragOperation = DragOperation.Copy;
            }
            else if (((e.KeyState & 8) != 8) && (data.DragOperation == DragOperation.Copy))
            {
                data.DragOperation = DragOperation.Move;
            }

            if (!(_nodeDragDropHandler?.CanDrop(data) ?? false))
            {
                if (data.DragOperation != (DragOperation.Copy | DragOperation.Move))
                {
                    e.Effect = DragDropEffects.None;
                    return;
                }

                MenuItemMoveTo.Available = false;
            }
            else
            {
                MenuItemMoveTo.Available = true;
            }

            DragDropEffects effects = DragDropEffects.None;

            if ((data.DragOperation & DragOperation.Copy) == DragOperation.Copy)
            {
                effects |= DragDropEffects.Copy;
            }

            if ((data.DragOperation & DragOperation.Move) == DragOperation.Move)
            {
                effects |= DragDropEffects.Move;
            }

            e.Effect = effects;
        }

        /// <summary>Handles the ItemDrag event of the ListSearchResults control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemDragEventArgs"/> instance containing the event data.</param>
        private void ListSearchResults_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            if (!(e.Item is ListViewItem item))
            {
                return;
            }

            var dragData = new ListViewItemDragData(item, (IFileExplorerNodeVm)item.Tag, DragOperation.Copy);
            item.Selected = true;

            var data = new DataObject();
            data.SetData(dragData);

            if (dragData is IContentFileDragData contentFileData)
            {
                contentFileData.Cancel = false;
                data.SetData(typeof(IContentFileDragData).FullName, true, contentFileData);
            }

            ListSearchResults.DoDragDrop(data, DragDropEffects.Move | DragDropEffects.Copy);
        }

        /// <summary>
        /// Handles the ItemDrag event of the TreeFileSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemDragEventArgs"/> instance containing the event data.</param>
        private void TreeFileSystem_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            if (!(e.Item is KryptonTreeNode treeNode))
            {
                return;
            }

            // Find the actual file system node.
            if (!_nodeLinks.TryGetValue(treeNode, out IFileExplorerNodeVm node))
            {
                return;
            }
            
            var dragData = new TreeNodeDragData(treeNode, node, e.Button == MouseButtons.Right ? (DragOperation.Copy | DragOperation.Move) : DragOperation.Move);
            SelectNode(treeNode);
            var data = new DataObject();
            data.SetData(dragData);

            if (dragData is IContentFileDragData contentFileData)
            {
                contentFileData.Cancel = false;
                data.SetData(typeof(IContentFileDragData).FullName, true, contentFileData);
            }

            TreeFileSystem.DoDragDrop(data, DragDropEffects.Move | DragDropEffects.Copy);

            if (dragData.TargetTreeNode == null)
            {
                return;
            }

            if (dragData.TargetNode == null)
            {
                dragData.TargetTreeNode.ForeColor = Color.Empty;
                dragData.TargetTreeNode.BackColor = Color.Empty;
                return;
            }

            UpdateNodeVisualState(dragData.TargetTreeNode, dragData.TargetNode);
            dragData.TargetTreeNode.BackColor = Color.Empty;            
        }

        /// <summary>Handles the BeforeSelect event of the TreeFileSystem control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TreeViewCancelEventArgs"/> instance containing the event data.</param>
        private void TreeFileSystem_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (!(e.Node is KryptonTreeNode treeNode))
            {
                return;
            }

            UpdateSelectedColor(treeNode);
        }

        /// <summary>
        /// Handles the AfterSelect event of the TreeFileSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TreeViewEventArgs"/> instance containing the event data.</param>
        private void TreeFileSystem_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeFileSystem.AfterSelect -= TreeFileSystem_AfterSelect;
            try
            {
                SelectNode(e.Node as KryptonTreeNode);                
            }
            finally
            {
                TreeFileSystem.AfterSelect += TreeFileSystem_AfterSelect;
            }
        }

        /// <summary>
        /// Handles the MouseUp event of the TreeFileSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void TreeFileSystem_MouseUp(object sender, MouseEventArgs e)
        {
            TreeViewHitTestInfo hitResult = TreeFileSystem.HitTest(e.Location);
            TreeFileSystem.AfterSelect -= TreeFileSystem_AfterSelect;
            try
            {
                SelectNode(hitResult?.Node as KryptonTreeNode);

                if (e.Button == MouseButtons.Right)
                {
                    MenuOptions.Show(this, PointToClient(Cursor.Position));
                }
            }
            finally
            {
                TreeFileSystem.AfterSelect += TreeFileSystem_AfterSelect;
            }
        }

        /// <summary>
        /// Handles the KeyUp event of the TreeFileSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void TreeFileSystem_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F2:
                    if ((TreeFileSystem.SelectedNode != null) && (!TreeFileSystem.SelectedNode.IsEditing))
                    {
                        TreeFileSystem.SelectedNode?.BeginEdit();
                    }
                    break;
                case Keys.Delete:
                    if ((TreeFileSystem.SelectedNode != null) && (!TreeFileSystem.SelectedNode.IsEditing))
                    {
                        MenuItemDelete.PerformClick();
                    }
                    break;
                case Keys.F5:
                    RefreshTreeBranch(true);
                    break;
            }
        }

        /// <summary>
        /// Handles the BeforeLabelEdit event of the TreeFileSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NodeLabelEditEventArgs"/> instance containing the event data.</param>
        private void TreeFileSystem_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            // Is this even possible?
            if (e.Node == null)
            {
                e.CancelEdit = true;
                return;
            }

            if (!_nodeLinks.TryGetValue((KryptonTreeNode)e.Node, out _renameNode))
            {
                e.CancelEdit = true;
                return;
            }

            EventHandler handler = RenameBegin;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles the AfterLabelEdit event of the TreeFileSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NodeLabelEditEventArgs"/> instance containing the event data.</param>
        private void TreeFileSystem_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            TreeFileSystem.BeforeLabelEdit -= TreeFileSystem_BeforeLabelEdit;
            TreeFileSystem.AfterLabelEdit -= TreeFileSystem_AfterLabelEdit;

            try
            {
                // If we didn't change the name, or there's no node being renamed, then leave.
                if ((DataContext?.RenameNodeCommand == null) || (e.Node == null))
                {
                    e.CancelEdit = true;
                    return;
                }

                string oldName = _renameNode.Name;
                var args = new FileExplorerNodeRenameArgs(_renameNode, oldName, e.Label);

                if (!DataContext.RenameNodeCommand.CanExecute(args))
                {
                    e.CancelEdit = true;
                    return;
                }

                DataContext.RenameNodeCommand.Execute(args);

                e.CancelEdit = args.Cancel;
            }
            finally
            {
                // We're no longer renaming anything.
                _renameNode = null;

                EventHandler handler = RenameEnd;
                handler?.Invoke(this, EventArgs.Empty);

                TreeFileSystem.AfterLabelEdit += TreeFileSystem_AfterLabelEdit;
                TreeFileSystem.BeforeLabelEdit += TreeFileSystem_BeforeLabelEdit;
            }
        }

        /// <summary>
        /// Handles the AfterExpand event of the TreeFileSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TreeViewEventArgs"/> instance containing the event data.</param>
        private void TreeFileSystem_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            // Turn off notification so we don't get a doubling up effect.
            if (!_nodeLinks.TryGetValue((KryptonTreeNode)e.Node, out IFileExplorerNodeVm node))
            {
                return;
            }

            node.PropertyChanged -= Node_PropertyChanged;
            try
            {
                node.IsExpanded = true;
            }
            finally
            {
                node.PropertyChanged += Node_PropertyChanged;
            }
        }

        /// <summary>
        /// Handles the BeforeCollapse event of the TreeFileSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TreeViewCancelEventArgs"/> instance containing the event data.</param>
        private void TreeFileSystem_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            // Turn off notification so we don't get a doubling up effect.
            if (!_nodeLinks.TryGetValue((KryptonTreeNode)e.Node, out IFileExplorerNodeVm node))
            {
                return;
            }

            node.PropertyChanged -= Node_PropertyChanged;
            try
            {
                node.IsExpanded = false;
            }
            finally
            {
                node.PropertyChanged += Node_PropertyChanged;
            }
        }

        /// <summary>
        /// Handles the AfterCollapse event of the TreeFileSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TreeViewEventArgs"/> instance containing the event data.</param>
        private void TreeFileSystem_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
            {
                return;
            }

            // Do not hang on to tree nodes that don't need to be kept.
            foreach (KryptonTreeNode treeNode in e.Node.Nodes.OfType<KryptonTreeNode>().Traverse(n => n.Nodes.OfType<KryptonTreeNode>()))
            {
                if (_nodeLinks.TryGetValue(treeNode, out IFileExplorerNodeVm fsNode))
                {
                    // Remove from our tree node linkage so we don't keep old nodes alive.                                                        
                    RemoveNodeLinks(fsNode);
                }

                _nodeLinks.Remove(treeNode);
            }

            e.Node.Nodes.Clear();

            if ((!_nodeLinks.TryGetValue((KryptonTreeNode)e.Node, out IFileExplorerNodeVm node))
                || ((node.Children.Count == 0) && (node.Dependencies.Count == 0)))
            {
                return;
            }

            e.Node.Nodes.Add(new KryptonTreeNode("DUMMY_NODE_SHOULD_NOT_SEE_ME"));
        }

        /// <summary>
        /// Handles the PropertyChanging event of the DataContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IFileExplorerVm.SearchResults):
                    _searchNodes = null;
                    break;
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the DataContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            KryptonTreeNode node = null;
            ListViewItem searchItem = null;

            switch (e.PropertyName)
            {
                case nameof(IFileExplorerVm.SearchResults):
                    _searchNodes = DataContext.SearchResults;

                    if (_searchNodes == null)
                    {
                        TextSearch.Text = string.Empty;
                    }

                    FillSearchResults();

                    if (_searchNodes == null)
                    {
                        ListSearchResults.SendToBack();
                    }
                    else
                    {
                        ListSearchResults.BringToFront();
                    }                    
                    break;
                case nameof(IFileExplorerVm.SelectedNode):
                    if (DataContext.SelectedNode != null)
                    {
                        _revNodeLinks.TryGetValue(DataContext.SelectedNode, out node);

                        if (_searchNodes != null)
                        {
                            searchItem = ListSearchResults.Items.OfType<ListViewItem>().FirstOrDefault(item => item.Tag == DataContext.SelectedNode);
                        }
                    }
                    else if (_searchNodes != null)
                    {
                        ListSearchResults.SelectedItems.Clear();
                    }

                    if ((searchItem != null) && (!searchItem.Selected))
                    {
                        searchItem.Selected = true;
                    }

                    if (TreeFileSystem.SelectedNode == node)
                    {
                        ValidateMenuItems(DataContext);
                        return;
                    }

                    SelectNode(node);
                    break;
            }
            ValidateMenuItems(DataContext);
        }

        /// <summary>
        /// Handles the BeforeExpand event of the TreeFileSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TreeViewCancelEventArgs"/> instance containing the event data.</param>
        private void TreeFileSystem_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node == null)
            {
                return;
            }

            if (DataContext?.SearchResults != null)
            {
                return;
            }

            e.Node.Nodes.Clear();

            if (!_nodeLinks.TryGetValue((KryptonTreeNode)e.Node, out IFileExplorerNodeVm parentNode))
            {
                return;
            }

            // Turn off events for this nodes children since they'll be destroyed anyway, and we really don't want to trigger the events during a refresh.
            UnassignNodeEvents(parentNode.Dependencies);
            UnassignNodeEvents(parentNode.Children);

            if (parentNode.Children.Count > 0)
            {
                FillTree(e.Node.Nodes, parentNode.Children, false);
            }
            else
            {
                FillTree(e.Node.Nodes, parentNode.Dependencies, false);
            }            

            AssignNodeEvents(parentNode.Children);
            AssignNodeEvents(parentNode.Dependencies);
        }

        /// <summary>
        /// Function to send the search command to the view model.
        /// </summary>
        /// <param name="searchText">The text to search for.</param>
        private void SendSearchCommand(string searchText)
        {
            if ((DataContext?.SearchCommand == null) || (!DataContext.SearchCommand.CanExecute(searchText)))
            {
                ValidateMenuItems(DataContext);
                return;
            }

            DataContext.SearchCommand.Execute(searchText);
            ValidateMenuItems(DataContext);
        }

        /// <summary>Handles the KeyUp event of the TextSearch control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void TextSearch_KeyUp(object sender, KeyEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.Enter:
                    SendSearchCommand(TextSearch.Text);
                    break;
                case Keys.Escape:
                    SendSearchCommand(null);
                    break;
            }
        }

        /// <summary>Handles the Search event of the TextSearch control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:Gorgon.Windows.UI.GorgonSearchEventArgs"/> instance containing the event data.</param>
        private void TextSearch_Search(object sender, Windows.UI.GorgonSearchEventArgs e) => SendSearchCommand(e.SearchText);
        
        /// <summary>Handles the Click event of the ButtonClearSearch control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonClearSearch_Click(object sender, EventArgs e)
        {
            TextSearch.Text = string.Empty;
            SendSearchCommand(null);
            SelectNextControl(this, true, true, true, true);
        }

        /// <summary>
        /// Function to update the visual state of the node based on the node data.
        /// </summary>
        /// <param name="searchItem">The item to update.</param>
        /// <param name="node">The file system node to evaluate.</param>
        private void UpdateSearchItemVisualState(ListViewItem searchItem, IFileExplorerNodeVm node)
        {
            if (node == null)
            {
                searchItem.Font = _excludedFont;
                searchItem.ForeColor = Color.DimGray;
                return;
            }

            if (node.Metadata == null)
            {
                searchItem.Font = _excludedFont;
                searchItem.ForeColor = Color.DimGray;
            }
            else
            {
                searchItem.ForeColor = node.IsChanged ? Color.LightGreen : Color.Empty;

                if (node.IsOpen)
                {
                    searchItem.Font = _openFont;
                }
                else
                {
                    searchItem.Font = null;
                }
            }

            if (node.IsCut)
            {
                // Make the text translucent if we're in cut mode.
                searchItem.ForeColor = Color.FromArgb(128, searchItem.ForeColor);
            }

            // Check for an icon.            
            if (node.Metadata?.ContentMetadata != null)
            {
                Image icon = node.Metadata.ContentMetadata.GetSmallIcon();

                // This ID has not been registered in the image list, do so now.
                if ((!TreeImages.Images.ContainsKey(node.ImageName))
                    && (icon != null))
                {
                    TreeImages.Images.Add(node.ImageName, icon);
                }
            }
            else if (node.IsContent)
            {
                searchItem.Font = _excludedFont;
                searchItem.ForeColor = Color.DimGray;
            }

            int imageIndex = FindImageIndexFromNode(node);
            if (imageIndex != -1)
            {
                searchItem.ImageIndex = imageIndex;
            }
        }

        /// <summary>
        /// Function to update the visual state of the node based on the node data.
        /// </summary>
        /// <param name="treeNode">The tree node to update.</param>
        /// <param name="node">The file system node to evaluate.</param>
        private void UpdateNodeVisualState(KryptonTreeNode treeNode, IFileExplorerNodeVm node)
        {
            TreeFileSystem.BeforeExpand -= TreeFileSystem_BeforeExpand;
            try
            {
                if (node == null)
                {
                    treeNode.NodeFont = _excludedFont;
                    treeNode.ForeColor = Color.DimGray;
                    return;
                }

                if (node.Metadata == null)
                {
                    treeNode.NodeFont = _excludedFont;
                    treeNode.ForeColor = Color.DimGray;
                }
                else
                {
                    treeNode.ForeColor = node.IsChanged ? Color.LightGreen : Color.Empty;

                    if (node.IsOpen)
                    {
                        treeNode.NodeFont = _openFont;
                    }
                    else
                    {
                        treeNode.NodeFont = null;
                    }
                }

                if (node.IsCut)
                {
                    // Make the text translucent if we're in cut mode.
                    treeNode.ForeColor = Color.FromArgb(128, treeNode.ForeColor);
                }

                // Check for an icon.            
                if (node.Metadata?.ContentMetadata != null)
                {
                    string imageName = node.ImageName;
                    Image icon = node.Metadata.ContentMetadata.GetSmallIcon();

                    // This ID has not been registered in the image list, do so now.
                    if ((!TreeImages.Images.ContainsKey(node.ImageName))
                        && (icon != null))
                    {
                        TreeImages.Images.Add(node.ImageName, icon);
                    }
                }
                else if (node.IsContent)
                {
                    treeNode.NodeFont = _excludedFont;
                    treeNode.ForeColor = Color.DimGray;
                }

                int imageIndex = FindImageIndexFromNode(node);
                if (imageIndex != -1)
                {
                    treeNode.ImageIndex = treeNode.SelectedImageIndex = imageIndex;
                }

                if (((node.Children.Count > 0) || (node.Dependencies.Count > 0)) && (!treeNode.IsExpanded))
                {
                    treeNode.Nodes.Add(new KryptonTreeNode("DUMMY_NODE_SHOULD_NOT_SEE_ME"));
                }

                if (treeNode.IsSelected)
                {
                    UpdateSelectedColor(treeNode);
                }
            }
            finally
            {
                TreeFileSystem.BeforeExpand += TreeFileSystem_BeforeExpand;
            }
        }

        /// <summary>
        /// Function to update the selected color for a node based on state information.
        /// </summary>
        /// <param name="node">The node to evaluate.</param>
        private void UpdateSelectedColor(KryptonTreeNode node)
        {
            if (node.NodeFont == _excludedFont)
            {
                TreeFileSystem.StateCheckedNormal.Node.Back.Color1 = Color.FromArgb(_dragBackColor.R / 2, _dragBackColor.G / 2, _dragBackColor.B / 2);
            }
            else
            {
                TreeFileSystem.StateCheckedNormal.Node.Back.Color1 = _dragBackColor;
            }

            TreeFileSystem.StateCheckedNormal.Node.Content.ShortText.Color1 = node.ForeColor;
            TreeFileSystem.StateCheckedNormal.Node.Content.ShortText.Font = node.NodeFont;
        }

        /// <summary>
        /// Function to refresh the tree, or a branch on the tree.
        /// </summary>
        /// <param name="rebuildFileSystemNodes"><b>true</b> to rebuild the underlying file system hierarchy before populating the branch, or <b>false</b> to just repopulate the branch.</param>
        private void RefreshTreeBranch(bool rebuildFileSystemNodes)
        {
            if (DataContext == null)
            {
                return;
            }

            if (TreeFileSystem.SelectedNode == null)
            {
                _nodeLinks.Clear();
                _revNodeLinks.Clear();

                UnassignNodeEvents(DataContext.RootNode.Children);

                if ((rebuildFileSystemNodes) && (DataContext.RefreshNodeCommand != null) && (DataContext.RefreshNodeCommand.CanExecute(DataContext.RootNode)))
                {
                    DataContext.RefreshNodeCommand.Execute(DataContext.RootNode);
                }

                FillTree(TreeFileSystem.Nodes, DataContext.RootNode.Children, true);

                AssignNodeEvents(DataContext.RootNode.Children);
                return;
            }

            bool isExpanded = TreeFileSystem.SelectedNode.IsExpanded;
            TreeFileSystem.SelectedNode.Collapse();

            if ((TreeFileSystem.SelectedNode.Nodes.Count == 0) || (!isExpanded))
            {
                return;
            }

            if (!_nodeLinks.TryGetValue((KryptonTreeNode)TreeFileSystem.SelectedNode, out IFileExplorerNodeVm node))
            {
                return;
            }

            TreeFileSystem.AfterSelect -= TreeFileSystem_AfterSelect;
            TreeFileSystem.BeforeExpand -= TreeFileSystem_BeforeExpand;
            node.Children.CollectionChanged -= Nodes_CollectionChanged;
            node.PropertyChanged -= Node_PropertyChanged;

            try
            {                
                if ((rebuildFileSystemNodes) && (DataContext.RefreshNodeCommand != null) && (DataContext.RefreshNodeCommand.CanExecute(node)))
                {
                    DataContext.RefreshNodeCommand.Execute(node);
                }
            }
            finally
            {
                node.PropertyChanged += Node_PropertyChanged;
                node.Children.CollectionChanged += Nodes_CollectionChanged;
                TreeFileSystem.BeforeExpand += TreeFileSystem_BeforeExpand;
                TreeFileSystem.AfterSelect += TreeFileSystem_AfterSelect;
            }

            TreeFileSystem.SelectedNode.Expand();
        }

		/// <summary>
        /// Function to fill the search result list view.
        /// </summary>
        private void FillSearchResults()
        {
            if (DataContext == null)
            {
                return;
            }

            ListSearchResults.BeginUpdate();

            try
            {
                ListSearchResults.Items.Clear();

                if ((_searchNodes == null) || (_searchNodes.Count == 0))
                {
                    return;
                }

                foreach (IFileExplorerNodeVm node in _searchNodes)
                {
                    var item = new ListViewItem
                    {
                        Name = node.FullPath,
                        Text = node.Name,
						Tag = node
                    };

                    if (node == DataContext.SelectedNode)
                    {
                        item.Selected = true;
                    }

                    UpdateSearchItemVisualState(item, node);

                    item.SubItems.Add(new ListViewItem.ListViewSubItem
                    {
                        Text = node.FullPath,
                        ForeColor = Color.Silver						
                    });

                    ListSearchResults.Items.Add(item);
                }
            }
            finally
            {                
                ListSearchResults.EndUpdate();

                ListSearchResults.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            }
        }

        /// <summary>
        /// Function to populate the tree with file system nodes.
        /// </summary>
        /// <param name="treeNodes">The node collection on the tree to update.</param>
        /// <param name="nodes">The nodes used to populate.</param>
        /// <param name="doNotExpand"><b>true</b> to keep nodes from expanding if their node state is set to expanded, <b>false</b> to automatically expand.</param>
        private void FillTree(TreeNodeCollection treeNodes, IReadOnlyList<IFileExplorerNodeVm> nodes, bool doNotExpand)
        {		
            TreeFileSystem.BeginUpdate();

            try
            {
                treeNodes.Clear();

                if (nodes.Count == 0)
                {
                    return;
                }

                IEnumerable<IFileExplorerNodeVm> nodeList = nodes.OrderBy(item => item.NodeType)
																 .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase);

                // Sort the nodes by type and then by name (I knew that NodeType enum would come in handy one day).
                foreach (IFileExplorerNodeVm node in nodeList)
                {
                    if (!node.Visible)
                    {
                        continue;
                    }

                    if ((!_revNodeLinks.TryGetValue(node, out KryptonTreeNode treeNode))
                        || (!_nodeLinks.ContainsKey(treeNode)))
                    {
                        int imageIndex = FindImageIndexFromNode(node);

                        treeNode = new KryptonTreeNode(node.Name)
                        {
                            ImageIndex = imageIndex,
                            SelectedImageIndex = imageIndex
                        };

                        UpdateNodeVisualState(treeNode, node);

                        _nodeLinks[treeNode] = node;
                        _revNodeLinks[node] = treeNode;
                    }
                                        
                    if (!treeNodes.Contains(treeNode))
                    {
						// Remove the parent event, it will be re-added after.
                        treeNodes.Add(treeNode);
                    }                    

                    if (node.IsExpanded)
                    {
                        if (doNotExpand)
                        {
                            node.IsExpanded = false;
                        }
                        else
                        {
                            treeNode.Expand();
                        }
                    }
                }
            }
            finally
            {
                TreeFileSystem.EndUpdate();
            }
        }

        /// <summary>
        /// Function to reset the view back to its original state when the data context is reset.
        /// </summary>
        private void ResetDataContext()
        {
            DataContext?.OnUnload();

            _searchNodes = null;
            _clipboardContext = null;
            _nodeDragDropHandler = null;
            _explorerDragDropHandler = null;

            _nodeLinks.Clear();
            _revNodeLinks.Clear();
            TreeFileSystem.Nodes.Clear();
        }

        /// <summary>
        /// Function to unassign events assigned to the various tree nodes.
        /// </summary>
        /// <param name="nodes">The nodes to evaluate.</param>
        private void AssignNodeEvents(ObservableCollection<IFileExplorerNodeVm> nodes)
        {
            if (nodes == null)
            {
                return;
            }

            foreach (IFileExplorerNodeVm node in nodes.Traverse(n => n.Children))
            {
                if (!_revNodeLinks.ContainsKey(node))
                {
                    continue;
                }

                node.PropertyChanged += Node_PropertyChanged;
                node.Children.CollectionChanged += Nodes_CollectionChanged;
                node.Dependencies.CollectionChanged += Nodes_CollectionChanged;
            }

            nodes.CollectionChanged += Nodes_CollectionChanged;            
        }

        /// <summary>
        /// Function to unassign events assigned to the various tree nodes.
        /// </summary>
        /// <param name="nodes">The nodes to evaluate.</param>
        private void UnassignNodeEvents(ObservableCollection<IFileExplorerNodeVm> nodes)
        {
            if (nodes == null)
            {
                return;
            }

            nodes.CollectionChanged -= Nodes_CollectionChanged;

            foreach (IFileExplorerNodeVm node in nodes.Traverse(n => n.Children))
            {
                if (!_revNodeLinks.ContainsKey(node))
                {
                    continue;
                }

                node.Dependencies.CollectionChanged -= Nodes_CollectionChanged;
                node.Children.CollectionChanged -= Nodes_CollectionChanged;
                node.PropertyChanged -= Node_PropertyChanged;
            }
        }

        /// <summary>
        /// Function unassign events after the data context is unassigned.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext == null)
            {
                return;
            }

            if (DataContext.RootNode != null)
            {
                DataContext.RootNode.Children.CollectionChanged -= Nodes_CollectionChanged;
                UnassignNodeEvents(DataContext.RootNode.Children);
            }

            DataContext.PropertyChanging += DataContext_PropertyChanging;
            DataContext.PropertyChanged += DataContext_PropertyChanged;
        }

        /// <summary>
        /// Function to initialize the view with the data context.
        /// </summary>
        /// <param name="dataContext">The data context to initialize with.</param>
        private void InitializeFromDataContext(IFileExplorerVm dataContext)
        {
            try
            {
                if (dataContext == null)
                {
                    ResetDataContext();
                    return;
                }

                DataContext?.OnUnload();

                _clipboardContext = dataContext as IClipboardHandler;
                _nodeDragDropHandler = dataContext;
                _explorerDragDropHandler = dataContext;

                // Always remove any dummy nodes.
                TreeFileSystem.Nodes.Clear();

                // We do not add the root node (really no point).
                _searchNodes = dataContext.SearchResults;
                FillTree(TreeFileSystem.Nodes, dataContext.RootNode.Children, false);

                if (_searchNodes != null)
                {
                    FillSearchResults();
                }

                AssignNodeEvents(dataContext.RootNode.Children);
            }
            finally
            {
                ValidateMenuItems(dataContext);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (IsDesignTime)
            {
                return;
            }            

            _excludedFont = new Font(KryptonManager.CurrentGlobalPalette
                                                   .GetContentShortTextFont((PaletteContentStyle)TreeFileSystem.ItemStyle, PaletteState.Normal),
                                     FontStyle.Italic);
            _openFont = new Font(KryptonManager.CurrentGlobalPalette
                                                   .GetContentShortTextFont((PaletteContentStyle)TreeFileSystem.ItemStyle, PaletteState.Normal),
                                                   FontStyle.Bold);

            _dragBackColor = KryptonManager.CurrentGlobalPalette.GetBackColor1(PaletteBackStyle.ButtonListItem, PaletteState.Tracking);
            _dragForeColor = KryptonManager.CurrentGlobalPalette.GetContentShortTextColor1(PaletteContentStyle.ButtonListItem, PaletteState.Tracking);

            DataContext?.OnLoad();
        }

        /// <summary>
        /// Function to expand the currently selected file system node.
        /// </summary>
        public void Expand()
        {
            if (DataContext?.SelectedNode == null)
            {
                return;
            }

            if (!_revNodeLinks.TryGetValue(DataContext.SelectedNode, out KryptonTreeNode treeNode))
            {
                return;
            }

            treeNode.Expand();
        }

        /// <summary>
        /// Function to collapse the currently selected file system node.
        /// </summary>
        public void Collapse()
        {
            if (DataContext?.SelectedNode == null)
            {
                return;
            }

            if (!_revNodeLinks.TryGetValue(DataContext.SelectedNode, out KryptonTreeNode treeNode))
            {
                return;
            }

            treeNode.Collapse();
        }
        
        /// <summary>
        /// Function to delete a file or directory.
        /// </summary>
        public void Delete()
        {
            if ((MenuItemDelete.Available) && (MenuItemDelete.Enabled))
            {
                MenuItemDelete.PerformClick();
            }
        }

        /// <summary>
        /// Function to create a new directory.
        /// </summary>
        public void CreateDirectory()
        {
            if ((MenuItemCreateDirectory.Available) && (MenuItemCreateDirectory.Enabled))
            {
                MenuItemCreateDirectory.PerformClick();
            }
        }

        /// <summary>
        /// Function to rename a node.
        /// </summary>
        public void RenameNode()
        {
            if ((MenuItemRename.Available) && (MenuItemRename.Enabled))
            {
                MenuItemRename.PerformClick();
            }
        }

        /// <summary>
        /// Function to assign a data context to the view as a view model.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>
        /// <para>
        /// Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.
        /// </para>
        /// </remarks>
        public void SetDataContext(IFileExplorerVm dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);
            DataContext = dataContext;

            if ((IsDesignTime) || (DataContext == null))
            {
                return;
            }

            DataContext.PropertyChanging += DataContext_PropertyChanging;
            DataContext.PropertyChanged += DataContext_PropertyChanged;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="FileExploder"/> class.
        /// </summary>
        public FileExploder()
        {
            InitializeComponent();

            if (IsDesignTime)
            {
                return;
            }

            _listViewHeaderBackColor = new SolidBrush(ListSearchResults.BackColor);
            _listViewHeaderForeColor = new SolidBrush(Color.FromArgb(ListSearchResults.ForeColor.R / 2, ListSearchResults.ForeColor.G / 2, ListSearchResults.ForeColor.B / 2));

            // Bug in krypton treeview:
            TreeFileSystem.TreeView.MouseUp += TreeFileSystem_MouseUp;
            TreeFileSystem.TreeViewNodeSorter = new FileSystemNodeComparer(_nodeLinks);            
        }
        #endregion
    }
}
