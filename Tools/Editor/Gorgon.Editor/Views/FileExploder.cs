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
        // The context handler for clipboard operations.
        private IClipboardHandler _clipboardContext;
        // A drag and drop handler interface.
        private IDragDropHandler<IFileExplorerNodeDragData> _dragDropHandler;
        // The drag hilight background color.
        private Color _dragBackColor;
        // The drag hilight foreground color.
        private Color _dragForeColor;
        // Flag to indicate that a node is being dragged.
        private bool _isDragging;
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
        /// Function to delete a node from the tree.
        /// </summary>
        /// <param name="deleteNode">The node to delete.</param>
        private void RemoveNode(IFileExplorerNodeVm deleteNode)
        {
            // This node is not on the tree, so just leave.  There's nothing we can do here.
            if (!_revNodeLinks.TryGetValue(deleteNode, out KryptonTreeNode deleteTreeNode))
            {
                return;
            }

            // Let us control next/prev node selection.
            TreeFileSystem.AfterSelect -= TreeFileSystem_AfterSelect;

            try
            {
                // Immediately turn off events for this node.  We don't need some phantom event ruining everything.
                UnassignNodeEvents(deleteNode.Children);
                deleteNode.PropertyChanged -= Node_PropertyChanged;

                var parentTreeNode = deleteTreeNode.Parent as KryptonTreeNode;
                var nextSibling = deleteTreeNode.NextNode as KryptonTreeNode;
#pragma warning disable IDE0019 // Use pattern matching  Seriously, this break so very fucking badly.  Did no one test these idiotic suggestions??
                var prevSibling = deleteTreeNode.PrevNode as KryptonTreeNode;
#pragma warning restore IDE0019 // Use pattern matching

                TreeNodeCollection nodes = parentTreeNode?.Nodes ?? TreeFileSystem.Nodes;
                nodes.Remove(deleteTreeNode);

                // Remove from linkage.
                _nodeLinks.Remove(deleteTreeNode);
                _revNodeLinks.Remove(deleteNode);
               
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
        /// <param name="parent">The parent of the new node.</param>
        /// <param name="newNode">The new node to add.</param>
        private void AddNode(IFileExplorerNodeVm parent, IFileExplorerNodeVm newNode)
        {
            // Locate the tree node that has our parent node.
            KryptonTreeNode parentTreeNode = null;

            if (parent != null)
            {
                _revNodeLinks.TryGetValue(parent, out parentTreeNode);
            }
            TreeNodeCollection nodes = parentTreeNode?.Nodes ?? TreeFileSystem.Nodes;
            
            int imageIndex = FindImageIndexFromNode(newNode);
            KryptonTreeNode newTreeNode = null;

            newNode.PropertyChanged += Node_PropertyChanged;
            newNode.Children.CollectionChanged += Nodes_CollectionChanged;

            if ((parentTreeNode == null) || (parentTreeNode.IsExpanded))
            {
                newTreeNode = new KryptonTreeNode(newNode.Name, imageIndex, imageIndex);

                UpdateNodeVisualState(newTreeNode, newNode);

                nodes.Add(newTreeNode);
                _nodeLinks[newTreeNode] = newNode;
                _revNodeLinks[newNode] = newTreeNode;
                SelectNode(newTreeNode);
                newTreeNode.EnsureVisible();
                return;
            }

            if (parentTreeNode.Nodes.Count == 0)
            {
                parentTreeNode.Nodes.Add("DummyNode_Should_Not_See");
            }
            
            // This will fill the parent node with children and update our linkages.
            parentTreeNode.Expand();
            
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
            IFileExplorerNodeVm node = _revNodeLinks.FirstOrDefault(item => item.Key.Children == nodes).Key;

            if (node == null)
            {
                return;
            }

            TreeNodeCollection nodeList = null;

            if (_revNodeLinks.TryGetValue(node, out KryptonTreeNode treeNode))
            {
                SelectNode(treeNode);

                if (treeNode.IsExpanded)
                {
                    treeNode.Collapse();
                }
                
                nodeList = treeNode.Nodes;
            }
            else
            {
                UnassignNodeEvents(DataContext.RootNode.Children);
                AssignNodeEvents(DataContext.RootNode.Children);
                _revNodeLinks.Clear();
                _nodeLinks.Clear();
                nodeList = TreeFileSystem.Nodes;
            }

            nodeList.Clear();

            ValidateMenuItems(DataContext);
        }

        /// <summary>
        /// Handles the CollectionChanged event of the Nodes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void Nodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {                       
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddNode(DataContext.SelectedNode, e.NewItems.OfType<IFileExplorerNodeVm>().First());
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

            switch (e.PropertyName)
            {
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
                case nameof(IFileExplorerNodeVm.IsCut):
                case nameof(IFileExplorerNodeVm.Included):
                    if (treeNode != null)
                    {
                        UpdateNodeVisualState(treeNode, node);
                    }                    
                    break;
            }
        }

        /// <summary>
        /// Handles the Click event of the ItemIncludeInProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ItemIncludeInProject_Click(object sender, EventArgs e)
        {
            if ((DataContext?.SelectedNode == null) 
                || (DataContext?.IncludeExcludeCommand == null) 
                || (!DataContext.IncludeExcludeCommand.CanExecute(MenuItemIncludeInProject.Checked)))
            {
                return;
            }

            DataContext.IncludeExcludeCommand.Execute(MenuItemIncludeInProject.Checked);                        
        }

        /// <summary>
        /// Handles the Click event of the ItemRename control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ItemRename_Click(object sender, EventArgs e) => TreeFileSystem.SelectedNode?.BeginEdit();
        
        /// <summary>
        /// Handles the Click event of the ItemDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ItemDelete_Click(object sender, EventArgs e)
        {
            if ((DataContext?.DeleteNodeCommand == null)
                || (!DataContext.DeleteNodeCommand.CanExecute(null))
                // If we're renaming, then ignore our delete.
                || ((TreeFileSystem.SelectedNode != null) && (TreeFileSystem.SelectedNode.IsEditing)))
            {
                return;
            }
            
            DataContext.DeleteNodeCommand.Execute(null);
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

            var args = new CreateNodeArgs();

            if (!DataContext.CreateNodeCommand.CanExecute(args))
            {
                return;
            }

            DataContext.CreateNodeCommand.Execute(args);

            if ((!args.RenameAfterCreate) || (args.Cancel))
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

            if (dataContext.SelectedNode == null)
            {
                MenuSepEdit.Available = _clipboardContext?.CanPaste() ?? false;
                MenuItemCopy.Available = false;
                MenuItemCut.Available = false;
                MenuItemPaste.Available = MenuSepEdit.Visible;
                MenuItemPaste.Enabled = MenuSepEdit.Visible;
                MenuSepOrganize.Available = false;
                MenuSepNew.Available = MenuSepEdit.Available;
                MenuItemCreateDirectory.Available = true;
                MenuItemDelete.Available = false;
                MenuItemRename.Available = false;
                MenuItemIncludeInProject.Available = false;
                return;
            }

            MenuSepEdit.Available = (_clipboardContext != null) && ((_clipboardContext.CanPaste()) || (_clipboardContext.CanCut()) || (_clipboardContext.CanCopy()));
            MenuSepOrganize.Available = true;
            MenuItemRename.Available = true;
            MenuItemIncludeInProject.Available = true;

            MenuItemCopy.Available = _clipboardContext?.CanCopy() ?? false;
            MenuItemCut.Available = _clipboardContext?.CanCut() ?? false;
            MenuItemPaste.Available = _clipboardContext != null;
            MenuItemPaste.Enabled = _clipboardContext?.CanPaste() ?? false;

            MenuItemCreateDirectory.Available = dataContext.CreateNodeCommand?.CanExecute(null) ?? false;
            MenuItemDelete.Available = dataContext.DeleteNodeCommand?.CanExecute(null) ?? false;
            MenuItemIncludeInProject.Checked = dataContext.SelectedNode.Included;

            MenuSepNew.Visible = MenuItemCreateDirectory.Available;
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

            if (!(e.Data.GetData(typeof(TreeNodeDragData)) is TreeNodeDragData data))
            {
                return;
            }

            if (data.TargetTreeNode != null)
            {
                data.TargetTreeNode.BackColor = Color.Empty;
                data.TargetTreeNode.ForeColor = Color.Empty;
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

            _dragDropHandler?.Drop(data);
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

            if (!(e.Data.GetData(typeof(TreeNodeDragData)) is TreeNodeDragData data))
            {
                return;
            }

            if (data.TargetTreeNode != null)
            {
                // Reset the background color.
                data.TargetTreeNode.BackColor = Color.Empty;
                data.TargetTreeNode.ForeColor = Color.Empty;
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

            if (!(_dragDropHandler?.CanDrop(data) ?? false))
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
            
            _isDragging = true;
            var dragData = new TreeNodeDragData(treeNode, node, e.Button == MouseButtons.Right ? (DragOperation.Copy | DragOperation.Move) : DragOperation.Move);
            TreeFileSystem.DoDragDrop(dragData, DragDropEffects.Move | DragDropEffects.Copy);

            if (dragData.TargetTreeNode == null)
            {
                return;
            }

            dragData.TargetTreeNode.BackColor = Color.Empty;
            dragData.TargetTreeNode.ForeColor = Color.Empty;
        }

        /// <summary>
        /// Handles the AfterSelect event of the TreeFileSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TreeViewEventArgs"/> instance containing the event data.</param>
        private void TreeFileSystem_AfterSelect(object sender, TreeViewEventArgs e) => SelectNode(e.Node as KryptonTreeNode);

        /// <summary>
        /// Handles the MouseUp event of the TreeFileSystem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void TreeFileSystem_MouseUp(object sender, MouseEventArgs e)
        {
            TreeViewHitTestInfo hitResult = TreeFileSystem.HitTest(e.Location);
            SelectNode(hitResult?.Node as KryptonTreeNode);
                        
            if (e.Button == MouseButtons.Right)
            {
                MenuOptions.Show(this, PointToClient(Cursor.Position));
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
                    RefreshTree();
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
            try
            {
                // If we didn't change the name, or there's no node being renamed, then leave.
                if ((DataContext?.RenameNodeCommand== null) || (e.Node == null))
                {
                    e.CancelEdit = true;
                    return;
                }

                var args = new FileExplorerNodeRenameArgs(_renameNode.Name, e.Label);

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
                // Revert the node name back (why the actual control doesn't do this is beyond me).
                if ((e.CancelEdit) && (e.Node != null) && (_renameNode != null))
                {
                    //e.Node.Name = _renameNode.Name;
                }

                // We're no longer renaming anything.
                _renameNode = null;

                EventHandler handler = RenameEnd;
                handler?.Invoke(this, EventArgs.Empty);
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

            // Recursively remove all links.
            void RemoveLinksRecursive(IEnumerable<KryptonTreeNode> treeNodes)
            {
                // Do not hang on to tree nodes that don't need to be kept.
                foreach (KryptonTreeNode treeNode in treeNodes)
                {
                    if (treeNode.Nodes.Count > 0)
                    {
                        RemoveLinksRecursive(treeNode.Nodes.OfType<KryptonTreeNode>());
                    }

                    _nodeLinks.TryGetValue(treeNode, out IFileExplorerNodeVm fsNode);
                    // Remove from our tree node linkage so we don't keep old nodes alive.                
                    _nodeLinks.Remove(treeNode);

                    if (fsNode != null)
                    {
                        _revNodeLinks.Remove(fsNode);
                    }
                }
            }

            RemoveLinksRecursive(e.Node.Nodes.OfType<KryptonTreeNode>());
            e.Node.Nodes.Clear();

            if ((!_nodeLinks.TryGetValue((KryptonTreeNode)e.Node, out IFileExplorerNodeVm node))
                || (node.Children.Count == 0))
            {
                return;
            }

            e.Node.Nodes.Add(new KryptonTreeNode("DummyNode_Should_Not_See"));
        }

        /// <summary>
        /// Handles the PropertyChanging event of the DataContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            
        }

        /// <summary>
        /// Handles the PropertyChanged event of the DataContext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IFileExplorerVm.SelectedNode):
                    KryptonTreeNode node = null;

                    if (DataContext.SelectedNode != null)
                    {
                        _revNodeLinks.TryGetValue(DataContext.SelectedNode, out node);
                    }

                    if (TreeFileSystem.SelectedNode == node)
                    {
                        ValidateMenuItems(DataContext);
                        return;
                    }

                    SelectNode(node);
                    break;
            }
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

            e.Node.Nodes.Clear();            

            if (!_nodeLinks.TryGetValue((KryptonTreeNode)e.Node, out IFileExplorerNodeVm parentNode))
            {
                return;
            }

            // Turn off events for this nodes children since they'll be destroyed anyway, and we really don't want to trigger the events during a refresh.
            UnassignNodeEvents(parentNode.Children);

            if ((DataContext?.RefreshNodeCommand != null) && (DataContext.RefreshNodeCommand.CanExecute(parentNode)))
            {
                DataContext.RefreshNodeCommand.Execute(parentNode);
            }

            FillTree(e.Node.Nodes, parentNode.Children);

            AssignNodeEvents(parentNode.Children);
        }

        /// <summary>
        /// Function to update the visual state of the node based on the node data.
        /// </summary>
        /// <param name="treeNode">The tree node to update.</param>
        /// <param name="node">The file system node to evaluate.</param>
        private void UpdateNodeVisualState(KryptonTreeNode treeNode, IFileExplorerNodeVm node)
        {
            if (node == null)
            {
                treeNode.NodeFont = _excludedFont;
                treeNode.ForeColor = Color.DimGray;
                return;
            }

            treeNode.NodeFont = node.Included ? null : _excludedFont;
            treeNode.ForeColor = node.Included ? Color.Empty : Color.DimGray;

            if (node.IsCut)
            {
                // Make the text translucent if we're in cut mode.
                treeNode.ForeColor = Color.FromArgb(128, treeNode.ForeColor);
            }

            if ((node.Children.Count > 0) && (!treeNode.IsExpanded))
            {
                treeNode.Nodes.Add(new KryptonTreeNode("DummyNode_Should_Not_See"));
            }
        }

        /// <summary>
        /// Function to refresh the tree, or a branch on the tree.
        /// </summary>
        private void RefreshTree()
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

                if ((DataContext.RefreshNodeCommand != null) && (DataContext.RefreshNodeCommand.CanExecute(DataContext.RootNode)))
                {
                    DataContext.RefreshNodeCommand.Execute(DataContext.RootNode);
                }

                FillTree(TreeFileSystem.Nodes, DataContext.RootNode.Children);

                AssignNodeEvents(DataContext.RootNode.Children);
                return;
            }

            bool isExpanded = TreeFileSystem.SelectedNode.IsExpanded;
            TreeFileSystem.SelectedNode.Collapse();

            // Collapsing the node will clear all links for any children.
            if ((TreeFileSystem.SelectedNode.Nodes.Count == 0) || (!isExpanded))
            {
                return;
            }
            
            TreeFileSystem.SelectedNode.Expand();
        }

        /// <summary>
        /// Function to populate the tree with file system nodes.
        /// </summary>
        /// <param name="treeNodes">The node collection on the tree to update.</param>
        /// <param name="nodes">The nodes used to populate.</param>
        private void FillTree(TreeNodeCollection treeNodes, IReadOnlyList<IFileExplorerNodeVm> nodes)
        {
            TreeFileSystem.BeginUpdate();

            try
            {
                treeNodes.Clear();

                if (nodes.Count == 0)
                {
                    return;
                }

                // Sort the nodes so that directories are on top, and files are on bottom.
                // Also sort by name.
                IEnumerable<IFileExplorerNodeVm> sortedNodes = nodes.Where(item => item.NodeType == NodeType.Directory)
                                                                    .OrderBy(item => item.Name)
                                                                    .Concat(nodes.Where(item => item.NodeType == NodeType.File)
                                                                                 .OrderBy(item => item.Name));

                foreach (IFileExplorerNodeVm node in sortedNodes)
                {
                    int imageIndex = FindImageIndexFromNode(node);

                    var treeNode = new KryptonTreeNode(node.Name)
                                   {
                                       ImageIndex = imageIndex, SelectedImageIndex = imageIndex
                                   };

                    UpdateNodeVisualState(treeNode, node);

                    treeNodes.Add(treeNode);

                    _nodeLinks[treeNode] = node;
                    _revNodeLinks[node] = treeNode;
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
            _clipboardContext = null;
            _dragDropHandler = null;

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
                node.PropertyChanged += Node_PropertyChanged;
                node.Children.CollectionChanged += Nodes_CollectionChanged;
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

                _clipboardContext = dataContext as IClipboardHandler;
                _dragDropHandler = dataContext;

                // Always remove any dummy nodes.
                TreeFileSystem.Nodes.Clear();

                // We do not add the root node (really no point).
                FillTree(TreeFileSystem.Nodes, dataContext.RootNode.Children);

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
        /// Function to refresh the entire file system.
        /// </summary>
        public void RefreshFileSystem()
        {
            TreeFileSystem.BeforeExpand -= TreeFileSystem_BeforeExpand;
            TreeFileSystem.AfterCollapse -= TreeFileSystem_AfterCollapse;
            TreeFileSystem.AfterSelect -= TreeFileSystem_AfterSelect;

            try
            {
                SelectNode(null);

                UnassignNodeEvents(DataContext?.RootNode?.Children);
                
                TreeFileSystem.Nodes.Clear();
                _nodeLinks.Clear();
                _revNodeLinks.Clear();

                if ((DataContext?.RootNode == null) || (DataContext.RefreshNodeCommand == null) || (!DataContext.RefreshNodeCommand.CanExecute(DataContext.RootNode)))
                {
                    return;
                }

                DataContext.RefreshNodeCommand.Execute(DataContext.RootNode);
                FillTree(TreeFileSystem.Nodes, DataContext.RootNode.Children);

                AssignNodeEvents(DataContext.RootNode.Children);                
            }
            finally
            {
                TreeFileSystem.BeforeExpand += TreeFileSystem_BeforeExpand;
                TreeFileSystem.AfterCollapse += TreeFileSystem_AfterCollapse;
                TreeFileSystem.AfterSelect += TreeFileSystem_AfterSelect;

                ValidateMenuItems(DataContext);
            }
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

            // Bug in krypton treeview:
            TreeFileSystem.TreeView.MouseUp += TreeFileSystem_MouseUp;
        }
        #endregion
    }
}
