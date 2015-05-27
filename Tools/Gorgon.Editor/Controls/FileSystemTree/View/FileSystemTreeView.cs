#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Wednesday, March 13, 2013 7:29:54 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Editor.Properties;
using Gorgon.IO;
using Gorgon.Math;

namespace Gorgon.Editor
{
	/// <summary>
	/// Tree node editing state.
	/// </summary>
	enum NodeEditState
	{
		/// <summary>
		/// No state.
		/// </summary>
		None = 0,
		/// <summary>
		/// Creating a directory.
		/// </summary>
		CreateDirectory = 1,
		/// <summary>
		/// Renaming a directory.
		/// </summary>
		RenameDirectory = 2,
		/// <summary>
		/// Creating a file.
		/// </summary>
		CreateFile = 3,
		/// <summary>
		/// Renaming a file.
		/// </summary>
		RenameFile = 4
	}

	/// <summary>
	/// The view used to represent the tree that will display the contents of the file system in the main UI.
	/// </summary>
	sealed class FileSystemTreeView
        : TreeView, IFileSystemView
    {
        #region Variables.
        private bool _disposed;							// Flag to indicate that the object was disposed.
        private Font _openContent;						// Font used for open content items.
	    private Font _excludedContent;					// Font used for excluded content items.
        private Brush _selectBrush;						// Brush used for selection background.
        private Pen _focusPen;							// Pen used for focus.
		private TextBox _renameBox;						// Text box used to rename a node.
		private FileSystemTreeNode _editNode;			// Node being edited.
	    private ImageAttributes _fadeAttributes;		// Attributes for faded items.	    
	    private ImageAttributes _linkAttributes;		// Attributes for linked items.
	    private ImageAttributes _expandIconAttributes;	// Attributes for the expand/contract icon color.
		private IContentData _content;					// Current content.
		// The editor theme.
		private EditorTheme _theme;
		// The root node for the tree.
		private FileSystemRootNode _rootNode;
        #endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the theme for the tree view.
		/// </summary>
		[Browsable(false)]
	    public EditorTheme Theme
	    {
			get
			{
				return _theme;
			}
			set
			{
				if (_expandIconAttributes != null)
				{
					_expandIconAttributes.Dispose();
					_expandIconAttributes = null;
				}

				if (value == null)
				{
					value = new EditorTheme();
				}

				_theme = value;
				BackColor = Theme.ContentPanelBackground;
				ForeColor = Theme.ForeColor;
				Refresh();
			}
	    }

		/// <summary>
		/// Property to return the selected editor tree node.
		/// </summary>
		[Browsable(false)]
	    public new FileSystemTreeNode SelectedNode
	    {
			get
			{
				return base.SelectedNode as FileSystemTreeNode;
			}
			set
			{
				base.SelectedNode = value;
			}
	    }

		/// <summary>
        /// Gets or sets the mode in which the control is drawn.
        /// </summary>
        /// <returns>One of the <see cref="T:System.Windows.Forms.TreeViewDrawMode" /> values. The default is <see cref="F:System.Windows.Forms.TreeViewDrawMode.Normal" />.</returns>
        ///   <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   </PermissionSet>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new TreeViewDrawMode DrawMode
        {
            get
            {
                return TreeViewDrawMode.OwnerDrawAll;
            }
        }

		/// <summary>
		/// Gets or sets a value indicating whether lines are drawn between the tree nodes that are at the root of the tree view.
		/// </summary>
		/// <PermissionSet>
		///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		/// </PermissionSet>
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new bool ShowRootLines
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets the collection of tree nodes that are assigned to the tree view control.
		/// </summary>
		[Browsable(false)]
		public new TreeNodeCollection Nodes
		{
			get
			{
				return base.Nodes;
			}
		}
        #endregion

        #region Methods.
		/// <summary>
		/// Function to retrieve the proper node foreground color based on state.
		/// </summary>
		/// <param name="node">Node to evaluate.</param>
		/// <returns>The foreground color of the node, based on state.</returns>
		private Color GetNodeForeColor(FileSystemTreeNode node)
		{
			if (node == null)
			{
				return Theme.DisabledColor;
			}

			if ((node.IsCut) || (node.IsSelected))
			{
				return Theme.HilightForeColor;
			}

			var args = new GetNodeStateEventArgs(node.NodeType);
			if (GetNodeState != null)
			{
				GetNodeState(this, args);
			}

			switch (node.NodeType)
			{
				case NodeType.Root:
					return args.HasChanges ? Theme.HilightBackColor : Theme.ForeColor;
				case NodeType.Dependency:
				case NodeType.Directory:
				case NodeType.File:
					return Theme.ForeColor;
				default:
					return Theme.DisabledColor;
			}

			/*FileSystemDependencyNode dependencyNode = node as FileSystemDependencyNode;

		    if (dependencyNode != null)
			{
				// We will hard code the fore color for broken links to be red instead of relying on the theme.
				// This may change in the future.
				return dependencyNode.IsBroken ? Theme.BrokenDependencyLinkForeColor : (node.IsSelected ? Theme.HilightForeColor : Theme.ForeColorInactive);
			}

			FileSystemFileNode fileNode = node as FileSystemFileNode;

			// Disable a file node if it has no file attached.
#warning We have plug-in in here, but I think we should only look up the plug-in in the content controller.  So we should probably not allow this node to have a file connection if no plug-in can be found for it.
			if ((fileNode != null) && (fileNode.File == null) && (fileNode.PlugIn == null))
			{
				return Theme.DisabledColor;
			}

			if (node.IsSelected)
			{
				return Theme.HilightForeColor;
			}


			// Disable directory node if no directory is attached.
			FileSystemDirectoryNode directoryNode = node as FileSystemDirectoryNode;
			if ((directoryNode != null) && (directoryNode.Directory == null))
			{
				return Theme.DisabledColor;
			}

			return Theme.ForeColor;*/
		}

		/// <summary>
		/// Function to create the color matrix required to color the icons.
		/// </summary>
	    private void CreateColorMatrix()
	    {
			if (_expandIconAttributes == null)
			{
				var colorMatrix = new ColorMatrix(new[]
				                                 {
					                                 new[]
					                                 {
						                                 Theme.ForeColor.R / 255.0f,
						                                 0.0f,
						                                 0.0f,
						                                 0.0f,
						                                 0.0f
					                                 },
					                                 new[]
					                                 {
						                                 0.0f,
						                                 Theme.ForeColor.G / 255.0f,
						                                 0.0f,
						                                 0.0f,
						                                 0.0f
					                                 },
					                                 new[]
					                                 {
						                                 0.0f,
						                                 0.0f,
						                                 Theme.ForeColor.B / 255.0f,
						                                 0.0f,
						                                 0.0f
					                                 },
					                                 new[]
					                                 {
						                                 0.0f,
						                                 0.0f,
						                                 0.0f,
						                                 1.0f,
						                                 0.0f
					                                 },
					                                 new[]
					                                 {
						                                 0.0f,
						                                 0.0f,
						                                 0.0f,
						                                 0.0f,
						                                 1.0f
					                                 }
				                                 });

				_expandIconAttributes = new ImageAttributes();
				_expandIconAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			}

			if (_fadeAttributes == null)
			{
				var fadeMatrix = new ColorMatrix(new[]
				                                 {
					                                 new[]
					                                 {
						                                 1.0f,
						                                 0.0f,
						                                 0.0f,
						                                 0.0f,
						                                 0.0f
					                                 },
					                                 new[]
					                                 {
						                                 0.0f,
						                                 1.0f,
						                                 0.0f,
						                                 0.0f,
						                                 0.0f
					                                 },
					                                 new[]
					                                 {
						                                 0.0f,
						                                 0.0f,
						                                 1.0f,
						                                 0.0f,
						                                 0.0f
					                                 },
					                                 new[]
					                                 {
						                                 0.0f,
						                                 0.0f,
						                                 0.0f,
						                                 0.25f,
						                                 0.0f
					                                 },
					                                 new[]
					                                 {
						                                 0.0f,
						                                 0.0f,
						                                 0.0f,
						                                 0.0f,
						                                 1.0f
					                                 }
				                                 });

				_fadeAttributes = new ImageAttributes();
				_fadeAttributes.SetColorMatrix(fadeMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			}

			if (_linkAttributes == null)
			{
				return;
			}

			var linkMatrix = new ColorMatrix(new[]
			                                 {
				                                 new[]
				                                 {
					                                 1.0f,
					                                 0.0f,
					                                 0.0f,
					                                 0.0f,
					                                 0.0f
				                                 },
				                                 new[]
				                                 {
					                                 0.0f,
					                                 1.0f,
					                                 0.0f,
					                                 0.0f,
					                                 0.0f
				                                 },
				                                 new[]
				                                 {
					                                 0.0f,
					                                 0.0f,
					                                 1.0f,
					                                 0.0f,
					                                 0.0f
				                                 },
				                                 new[]
				                                 {
					                                 0.0f,
					                                 0.0f,
					                                 0.0f,
					                                 0.7f,
					                                 0.0f
				                                 },
				                                 new[]
				                                 {
					                                 0.0f,
					                                 0.0f,
					                                 0.0f,
					                                 0.0f,
					                                 1.0f
				                                 }
			                                 });


			_linkAttributes = new ImageAttributes();
			_linkAttributes.SetColorMatrix(linkMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
	    }

		/// <summary>
		/// Handles the LostFocus event of the _renameBox control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		private void _renameBox_LostFocus(object sender, EventArgs e)
		{
			HideRenameBox(false);
		}

		/// <summary>
		/// Handles the KeyDown event of the _renameBox control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		private void _renameBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Enter)
			{
				return;
			}

			HideRenameBox(false);
			e.Handled = true;
		}

	    /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.TreeView" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
	                if (_expandIconAttributes != null)
	                {
		                _expandIconAttributes.Dispose();
		                _expandIconAttributes = null;
	                }

	                if (_linkAttributes != null)
	                {
		                _linkAttributes.Dispose();
		                _linkAttributes = null;
	                }

					if (_fadeAttributes != null)
					{
						_fadeAttributes.Dispose();
						_fadeAttributes = null;
					}

					if (_renameBox != null)
					{
						_renameBox.KeyDown -= _renameBox_KeyDown;
						_renameBox.Dispose();
					}

                    if (_selectBrush != null)
                    {
                        _selectBrush.Dispose();
                    }

                    if (_focusPen != null)
                    {
                        _focusPen.Dispose();
                    }

	                if (_excludedContent != null)
	                {
		                _excludedContent.Dispose();
	                }

                    if (_openContent != null)
                    {
                        _openContent.Dispose();
                    }
                }

                _selectBrush = null;
                _focusPen = null;
	            _excludedContent = null;
                _openContent = null;
                _disposed = true;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                base.OnMouseDown(e);
                return;
            }

            SelectedNode = GetNodeAt(e.Location) as FileSystemTreeNode;

            base.OnMouseDown(e);
        }

        /// <summary>
		/// Sends the specified message to the default window procedure.
		/// </summary>
		/// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message" /> to process.</param>
	    protected override void DefWndProc(ref Message m)
	    {
		    const int leftDoubleClick = 0x203;

			// Override the double click so that the tree doesn't expand on file nodes.
			if (m.Msg == leftDoubleClick)
			{
				if ((SelectedNode.NodeType == NodeType.Root)
				    || ((SelectedNode.NodeType & NodeType.Directory) != NodeType.Directory))
				{
					return;
				}
			}

		    base.DefWndProc(ref m);
	    }

		/// <summary>
		/// Overrides <see cref="M:System.Windows.Forms.Control.OnHandleCreated(System.EventArgs)" />.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			if (DesignMode)
			{
				Theme = new EditorTheme();
				return;
			}

			// Set the default sorter.
			TreeViewNodeSorter = new FileSystemTreeNodeComparer();
		}

		/// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.FontChanged" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            if (_openContent != null)
            {
                _openContent.Dispose();
                _openContent = null;
            }

            _openContent = new Font(Font, FontStyle.Bold);

		    if (_excludedContent != null)
		    {
			    _excludedContent.Dispose();
			    _excludedContent = null;
		    }

			_excludedContent = new Font(Font, FontStyle.Italic);
        }

		/// <summary>
		/// Function to retrieve the size of the icon to display based on the current DPI.
		/// </summary>
		/// <param name="g">Graphics interface.</param>
		/// <param name="iconImageSize">The icon size.</param>
		/// <returns>The size scaled to the correct DPI.</returns>
		private static Size GetIconSize(System.Drawing.Graphics g, Size iconImageSize)
		{
			if ((g.DpiX.EqualsEpsilon(96.0f))
			    && (g.DpiY.EqualsEpsilon(96.0f)))
			{
				return iconImageSize;
			}

			// Calculate size of image in inches in standard DPI (96 DPI for Windows).
			var imageSizeStdDPI = new SizeF(iconImageSize.Width / 96.0f, iconImageSize.Height / 96.0f);

			return new Size(((int)(imageSizeStdDPI.Width * g.DpiX).FastCeiling()).Max(iconImageSize.Width),
			                ((int)(imageSizeStdDPI.Height * g.DpiY).FastCeiling()).Max(iconImageSize.Height));
		}

		/// <summary>
		/// Function to draw the expand/contract icon for a node.
		/// </summary>
		/// <param name="g">Graphics interface.</param>
		/// <param name="node">The node that has the icon.</param>
		/// <param name="position">Horizontal position of the icon.</param>
	    private void DrawExpandContractIcon(System.Drawing.Graphics g, FileSystemTreeNode node, ref Point position)
		{
			// No child nodes, so leave.
			if (node.Nodes.Count == 0)
			{
				position = new Point(position.X + 16, position.Y);
				return;
			}
			
			Image expandContractIcon = node.IsExpanded ? Resources.tree_expand_16x16 : Resources.tree_collapse_16x16;
			
			g.DrawImage(expandContractIcon,
			            new Rectangle(position, expandContractIcon.Size),
			            0,
			            0,
			            expandContractIcon.Width,
			            expandContractIcon.Height,
			            GraphicsUnit.Pixel,
			            _expandIconAttributes);

			position = new Point(position.X + expandContractIcon.Width, position.Y);
		}

		/// <summary>
		/// Function to draw the node icon and text.
		/// </summary>
		/// <param name="g">Graphics interface.</param>
		/// <param name="node">The node that has the icon.</param>
		/// <param name="drawText"><c>true</c> to draw the text, <c>false</c> to skip.</param>
		/// <param name="position">Horizontal position of the icon.</param>
		private void DrawNodeIconAndText(System.Drawing.Graphics g, FileSystemTreeNode node, bool drawText, ref Point position)
		{
			Font textFont = GetNodeFont(node);
			Image currentIcon = node.IsExpanded ? node.ExpandedImage : node.CollapsedImage;

			if ((currentIcon == null)
			    && (node.ExpandedImage == null))
			{
				currentIcon = node.CollapsedImage;
			}

			Debug.Assert(currentIcon != null, "Node does not have an icon.");

			// Draw the icon.
			Size scaledIconSize = GetIconSize(g, currentIcon.Size);

			g.DrawImage(currentIcon,
				        new Rectangle(position, scaledIconSize),
				        0,
				        0,
				        currentIcon.Width,
				        currentIcon.Height,
				        GraphicsUnit.Pixel,
				        node.IsCut ? _fadeAttributes : null);


			// Draw the link icon overlay if we have a dependency.
			if ((node.NodeType & NodeType.Dependency) == NodeType.Dependency)
			{
				Size scaledOverlaySize = GetIconSize(g, Resources.linked_file_8x8.Size);

				g.DrawImage(Resources.linked_file_8x8,
				            new Rectangle(position.X,
				                          position.Y + scaledIconSize.Height - scaledOverlaySize.Height,
				                          scaledOverlaySize.Width,
				                          scaledOverlaySize.Height),
				            0,
				            0,
				            Resources.linked_file_8x8.Width,
				            Resources.linked_file_8x8.Height,
				            GraphicsUnit.Pixel,
				            _linkAttributes);
			}

			int offset = (scaledIconSize.Width / currentIcon.Width) * 2;
			position = new Point(position.X + scaledIconSize.Width + offset, position.Y);

			if (!drawText)
			{
				return;
			}

			TextRenderer.DrawText(g, node.Text, textFont, position, GetNodeForeColor(node));
		}

		/// <summary>
		/// Function to retrieve the correct font depending on node state.
		/// </summary>
		/// <param name="node">The node to evaluate.</param>
		/// <returns>The appropriate font.</returns>
	    private Font GetNodeFont(FileSystemTreeNode node)
	    {
		    Font result = node.NodeFont ?? Font;
		    var nodeFile = node as FileSystemFileNode;
			var rootNode = node as FileSystemRootNode;

			// If the node is a file then we override some of the styles.
			if (nodeFile == null)
			{
				return result;
			}

			// If the node does not have a file attached or the meta data does not contain this file, then 
			// mark it as excluded.
#warning We should derive this from the file item attached to the node via a property.
			if ((nodeFile.File == null)) //|| (!EditorMetaDataFile.Files.Contains(nodeFile.File.FullPath)))
			{
				// Create the excluded content font if it doesn't exist, or the node font was changed.
				if ((_excludedContent != null) && (_excludedContent.FontFamily.Name == result.FontFamily.Name) && (_excludedContent.Size.EqualsEpsilon(result.Size)))
				{
					return _excludedContent;
				}

				if (_excludedContent != null)
				{
					_excludedContent.Dispose();
				}

				_excludedContent = new Font(result, FontStyle.Bold);

				return _excludedContent;
			}

			// If we don't have currently loaded content, or the node does not own the content file, then return the regular font.
			if ((_content == null)) // || (nodeFile.File != _content.SomeFile))
			{
				return result;
			}

			// Create the open content font if it's been changed or doesn't exist.
			if ((_openContent != null) && (_openContent.FontFamily.Name == result.FontFamily.Name) && (_openContent.Size.EqualsEpsilon(result.Size)))
			{
				return _openContent;
			}

			if (_openContent != null)
			{
				_openContent.Dispose();
			}

			_openContent = new Font(result, FontStyle.Bold);

			return _openContent;
	    }
            	
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.TreeView.DrawNode" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.DrawTreeNodeEventArgs" /> that contains the event data.</param>
        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
	        var node = e.Node as FileSystemTreeNode;
	        Point position = e.Bounds.Location;

            if ((e.Bounds.Width == 0) || (e.Bounds.Height == 0) && ((e.Bounds.X == 0) && (e.Bounds.Y == 0)))
            {
                return;
            }

            if (node == null)
            {
                e.DrawDefault = true;
                return;
            }

			// Update the color matrix if necessary.
			CreateColorMatrix();

            // Create graphics resources.
            if (_selectBrush == null)
            {
                _selectBrush = new SolidBrush(Theme.HilightBackColor);
            }

            if (_focusPen == null)
            {
                _focusPen = new Pen(Theme.WindowBorderActive)
                            {
	                            DashStyle = DashStyle.DashDot
                            };
            }

            // Draw selection rectangle.
            if ((e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected)
            {
                e.Graphics.FillRectangle(_selectBrush, e.Bounds);
            }

            // Draw a focus rectangle only when focused, not when selected.
            if (e.State == TreeNodeStates.Focused)
            {
                var rect = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
                e.Graphics.DrawRectangle(_focusPen, rect);
            }

	        position = new Point(node.Level * 16 + position.X, position.Y);

            // Draw the icon to expand/contract child nodes (if said nodes exist).
			DrawExpandContractIcon(e.Graphics, node, ref position);

			// Draw the node.
			DrawNodeIconAndText(e.Graphics, node, e.State != TreeNodeStates.Focused && _editNode != node, ref position);
        }

		/// <summary>
		/// Processes a command key.
		/// </summary>
		/// <param name="msg">A <see cref="T:System.Windows.Forms.Message" />, passed by reference, that represents the window message to process.</param>
		/// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys" /> values that represents the key to process.</param>
		/// <returns>
		/// true if the character was processed by the control; otherwise, false.
		/// </returns>
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if ((_renameBox == null) || (!_renameBox.Visible) || (!_renameBox.Focused))
			{
				return base.ProcessCmdKey(ref msg, keyData);
			}

			switch (keyData)
			{
				case Keys.Escape:
					HideRenameBox(true);
					return true;
				case Keys.Delete:
					int selectionStart = _renameBox.SelectionStart;
					int selectionLength = _renameBox.SelectionLength == 0 ? 1 : _renameBox.SelectionLength;

					if (selectionStart < _renameBox.TextLength)
					{
						_renameBox.Text = _renameBox.Text.Remove(selectionStart, selectionLength);
						_renameBox.SelectionStart = selectionStart;
					}
					return true;
				default:
					return base.ProcessCmdKey(ref msg, keyData);
			}
		}

		/// <summary>
		/// Function to show the rename text box.
		/// </summary>
		/// <param name="node">Node to edit.</param>
		public void ShowRenameBox(FileSystemTreeNode node)
		{
			if (node == null)
			{
				return;
			}

			if (_renameBox == null)
			{
				_renameBox = new TextBox
				             {
					             Name = Name + "_EditBox",
					             Visible = false,
					             BorderStyle = BorderStyle.None,
					             BackColor = Color.White,
					             ForeColor = Color.Black,
					             Height = node.Bounds.Height,
					             AcceptsTab = false,
					             Anchor = AnchorStyles.Left | AnchorStyles.Right
				             };
				Controls.Add(_renameBox);
			}

			Image nodeImage = node.IsExpanded ? node.ExpandedImage : node.CollapsedImage;
			Size expandContractScale;
			Size iconScale;

			// Wipe out the background.
			using (var g = CreateGraphics())
			{
				// Create graphics resources.
				if (_selectBrush == null)
				{
					_selectBrush = new SolidBrush(Theme.HilightBackColor);
				}

				g.FillRectangle(_selectBrush, new Rectangle(0, node.Bounds.Y, ClientSize.Width, node.Bounds.Height));

				iconScale = GetIconSize(g, nodeImage.Size);
			}

			Point nodePosition = node.Bounds.Location;

			nodePosition.X += node.Level * 16 + 21;
			nodePosition.Y++;

			int offset = iconScale.Width / nodeImage.Width;
			nodePosition.X += iconScale.Width + (offset * 2);

			_renameBox.Location = nodePosition;
			_renameBox.Width = ClientSize.Width - nodePosition.X;
			_renameBox.Text = node.Text;

			var editArgs = new NodeLabelEditEventArgs(node, node.Text)
			               {
				               CancelEdit = false
			               };

			OnBeforeLabelEdit(editArgs);

			if (editArgs.CancelEdit)
			{
				return;
			}

			_editNode = node;
			_editNode.Redraw();
			_renameBox.Visible = true;
			_renameBox.Focus();

			if (node.Text.Length > 0)
			{
				_renameBox.Select(0, node.Text.Length);
			}

			_renameBox.KeyDown += _renameBox_KeyDown;
			_renameBox.LostFocus += _renameBox_LostFocus;
		}

		/// <summary>
		/// Function to hide the rename box.
		/// </summary>
		/// <param name="canceled"><c>true</c> if the edit was canceled, <c>false</c> if not.</param>
		public void HideRenameBox(bool canceled)
		{
			if (_renameBox == null)
			{
				return;
			}

			var editNode = _editNode;

			_renameBox.KeyDown -= _renameBox_KeyDown;
			_renameBox.LostFocus -= _renameBox_LostFocus;
			_renameBox.Visible = false;
			_editNode = null;

			var eventArgs = new NodeLabelEditEventArgs(editNode, canceled ? editNode.Text : _renameBox.Text);
			OnAfterLabelEdit(eventArgs);

			if (!eventArgs.CancelEdit)
			{
				editNode.Text = eventArgs.Label;
			}
		}

		/// <summary>
		/// Function to retrieve the node associated with the current content.
		/// </summary>
		/// <param name="file">File associated with the node.</param>
		/// <returns>The node associated with the current content.</returns>
	    public FileSystemFileNode GetCurrentContentNode(GorgonFileSystemFileEntry file)
	    {
			// Find our node that corresponds to this content.
			return (from treeNode in AllNodes()
			        let fileNode = treeNode as FileSystemFileNode
			        where fileNode != null && fileNode.NodeType == NodeType.File && fileNode.File == file
			        select fileNode).FirstOrDefault();
	    }

		/// <summary>
		/// Function to return an iterator that will search through all nodes in the tree.
		/// </summary>
		/// <returns>An enumerator for all the nodes in the tree.</returns>
	    public IEnumerable<FileSystemTreeNode> AllNodes()
		{
			return Nodes.AllNodes();
	    }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemTreeView"/> class.
        /// </summary>
		public FileSystemTreeView()
        {
	        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            base.DrawMode = TreeViewDrawMode.OwnerDrawAll;
        }
        #endregion

		#region IFileSystemView Members
		#region Events
		/// <summary>
		/// Event fired to retrieve the state of the data linked to a node.
		/// </summary>
		public event EventHandler<GetNodeStateEventArgs> GetNodeState;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clear the nodes from the tree.
		/// </summary>
		public void Clear()
		{
			_rootNode = null;
			Nodes.Clear();
		}

		/// <summary>
		/// Function to add a new root node to the tree.
		/// </summary>
		/// <param name="fileSystemName">Name of the file system that's loaded (or created).</param>
		public void AddFileSystemRoot(string fileSystemName)
		{
			Clear();
			_rootNode = new FileSystemRootNode(fileSystemName);
			Nodes.Add(_rootNode);
		}
		#endregion
		#endregion
	}
}
