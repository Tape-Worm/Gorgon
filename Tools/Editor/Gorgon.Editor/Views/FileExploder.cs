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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.ViewModels;
using System.Diagnostics;

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
                    // Ensure all of our events are removed. Minimizes the off chance of an event leak.
                    foreach (IFileExplorerNodeVm node in _revNodeLinks.Keys)
                    {
                        UnassignNodeEvents(node.Children);
                    }

                    // All items are gone so the node linkages can go too.
                    _revNodeLinks.Clear();
                    _nodeLinks.Clear();

                    ValidateMenuItems(DataContext);
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
                || (!DataContext.IncludeExcludeCommand.CanExecute(ItemIncludeInProject.Checked)))
            {
                return;
            }

            DataContext.IncludeExcludeCommand.Execute(ItemIncludeInProject.Checked);                        
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
                || (!DataContext.DeleteNodeCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.DeleteNodeCommand.Execute(null);
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
            if (dataContext?.SelectedNode == null)
            {
                MenuSep1.Visible = false;
                MenuSep2.Visible = false;
                ItemCreateDirectory.Visible = true;
                ItemDelete.Visible = false;
                ItemRename.Visible = false;
                ItemIncludeInProject.Visible = false;
                return;
            }

            MenuSep1.Visible = true;
            ItemRename.Visible = true;
            ItemIncludeInProject.Visible = true;

            ItemCreateDirectory.Visible = dataContext.CreateNodeCommand?.CanExecute(null) ?? false;
            ItemDelete.Visible = dataContext.DeleteNodeCommand?.CanExecute(null) ?? false;
            ItemIncludeInProject.Checked = dataContext.SelectedNode.Included;

            MenuSep2.Visible = ItemCreateDirectory.Visible;
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
                MenuOperations.Show(this, Cursor.Position);
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
                    TreeFileSystem.SelectedNode?.BeginEdit();
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
            foreach (KryptonTreeNode treeNode in e.Node.Nodes.OfType<KryptonTreeNode>())
            {
                _nodeLinks.TryGetValue(treeNode, out IFileExplorerNodeVm fsNode);
                // Remove from our tree node linkage so we don't keep old nodes alive.                
                _nodeLinks.Remove(treeNode);

                if (fsNode != null)
                {
                    _revNodeLinks.Remove(fsNode);
                }
            }

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

            FillTree(e.Node.Nodes, parentNode.Children);
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

            if ((node.Children.Count > 0) && (!treeNode.IsExpanded))
            {
                treeNode.Nodes.Add(new KryptonTreeNode("DummyNode_Should_Not_See"));
            }
        }

        /// <summary>
        /// Function to populate the tree with file system nodes.
        /// </summary>
        /// <param name="treeNodes">The node collection on the tree to update.</param>
        /// <param name="nodes">The nodes used to populate.</param>
        private void FillTree(TreeNodeCollection treeNodes,IReadOnlyList<IFileExplorerNodeVm> nodes)
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

            for (int i = 0; i < nodes.Count; ++i)
            {
                IFileExplorerNodeVm node = nodes[i];

                AssignNodeEvents(node.Children);

                node.PropertyChanged += Node_PropertyChanged;
            }            

            nodes.CollectionChanged += Nodes_CollectionChanged;
        }

        /// <summary>
        /// Function to unassign events assigned to the various tree nodes.
        /// </summary>
        /// <param name="nodes">The nodes to evaluate.</param>
        private void UnassignNodeEvents(ObservableCollection<IFileExplorerNodeVm> nodes)
        {
            if ((nodes == null) || (nodes.Count == 0))
            {
                return;
            }

            nodes.CollectionChanged -= Nodes_CollectionChanged;

            for (int i = 0; i < nodes.Count; ++i)
            {
                IFileExplorerNodeVm node = nodes[i];

                node.PropertyChanged -= Node_PropertyChanged;

                UnassignNodeEvents(node.Children);
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

            if ((IsDesignTime) || (DataContext == null))
            {
                return;
            }

            DataContext.OnLoad();
        }

        /// <summary>
        /// Function to delete a file or directory.
        /// </summary>
        public void Delete()
        {
            if ((ItemDelete.Visible) && (ItemDelete.Enabled))
            {
                ItemDelete.PerformClick();
            }
        }

        /// <summary>
        /// Function to create a new directory.
        /// </summary>
        public void CreateDirectory()
        {
            if ((ItemCreateDirectory.Visible) && (ItemCreateDirectory.Enabled))
            {
                ItemCreateDirectory.PerformClick();
            }
        }

        /// <summary>
        /// Function to rename a node.
        /// </summary>
        public void RenameNode()
        {
            if ((ItemRename.Visible) && (ItemRename.Enabled))
            {
                ItemRename.PerformClick();
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
                        
            _excludedFont = new Font(KryptonManager.CurrentGlobalPalette
                                                   .GetContentShortTextFont((PaletteContentStyle)TreeFileSystem.ItemStyle, PaletteState.Normal),
                                     FontStyle.Italic);
        }
        #endregion
    }
}
