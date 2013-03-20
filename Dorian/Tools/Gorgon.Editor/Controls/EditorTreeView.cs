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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;

namespace GorgonLibrary.Editor
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
    /// Custom treeview for the editor.
    /// </summary>
    class EditorTreeView
        : TreeView
    {
        #region Variables.
        private bool _disposed = false;                     // Flag to indicate that the object was disposed.
        private Font _openContent = null;                   // Font used for open content items.
        private Brush _selectBrush = null;                  // Brush used for selection background.
        private Pen _focusPen = null;                       // Pen used for focus.
		private TextBox _renameBox = null;				    // Text box used to rename a node.
		private EditorTreeNode _editNode = null;		    // Node being edited.
        #endregion

		#region Properties.
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
            set
            {
            }
        }
        #endregion

        #region Methods.
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

                    if (_openContent != null)
                    {
                        _openContent.Dispose();
                    }
                }

                _selectBrush = null;
                _focusPen = null;
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
			base.OnMouseDown(e);

			var node = GetNodeAt(e.Location) as EditorTreeNode;

			if ((node != null) && (node != SelectedNode))
			{
				SelectedNode = node;
			}
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

            _openContent = new Font(this.Font, FontStyle.Bold);
        }
            	
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.TreeView.DrawNode" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.DrawTreeNodeEventArgs" /> that contains the event data.</param>
        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            Image currentImage = null;
            Image plusMinusImage = null;
            Font font = null;
            EditorTreeNode node = e.Node as EditorTreeNode;
            TreeNodeFile nodeFile = null;
            Point position = e.Bounds.Location;
            Size size = e.Bounds.Size;

            if ((e.Bounds.Width == 0) || (e.Bounds.Height == 0) && ((e.Bounds.X == 0) && (e.Bounds.Y == 0)))
            {
                return;
            }

            if (node == null)
            {
                e.DrawDefault = true;
                return;
            }

            // Create graphics resources.
            if (_selectBrush == null)
            {
                _selectBrush = new SolidBrush(DarkFormsRenderer.MenuHilightBackground);
            }

            if (_focusPen == null)
            {
                _focusPen = new Pen(DarkFormsRenderer.BorderColor);
                _focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
            }

            font = node.NodeFont;

            // Use parent font if no font is assigned.
            if (font == null)
            {
                font = this.Font;
            }

            // Shift the position.
            position.X = position.X + (e.Node.Level * 16);

            if (node.IsExpanded)
            {
                currentImage = node.ExpandedImage;
                plusMinusImage = Properties.Resources.tree_expand_16x16;

                if (currentImage == null)
                {
                    currentImage = node.CollapsedImage;
                }
            }
            else
            {
                plusMinusImage = Properties.Resources.tree_collapse_16x16;
                currentImage = node.CollapsedImage;
            }

            nodeFile = e.Node as TreeNodeFile;

            if ((Program.CurrentContent != null) && (nodeFile != null) && (Program.CurrentContent.File == nodeFile.File))
            {
                // Create the open content font if it's been changed or doesn't exist.
                if ((_openContent == null) || (_openContent.FontFamily.Name != font.FontFamily.Name) || (_openContent.Size != font.Size))
                {
                    if (_openContent != null)
                    {
                        _openContent.Dispose();
                    }
                    _openContent = new Font(font, FontStyle.Bold);
                }

                font = _openContent;
            }

            // Draw selection rectangle.
            if ((e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected)
            {
                e.Graphics.FillRectangle(_selectBrush, e.Bounds);
            }

            // Draw a focus rectangle only when focused, not when selected.
            if (e.State == TreeNodeStates.Focused)
            {
                Rectangle rect = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
                e.Graphics.DrawRectangle(_focusPen, rect);
            }			

            // Check for child nodes.
            position.X = position.X + 8;

            if (node.Nodes.Count > 0)
            {
                e.Graphics.DrawImage(plusMinusImage, new Rectangle(position, plusMinusImage.Size));
            }

            position.X = position.X + 16;

            // Draw the icon.
            e.Graphics.DrawImage(currentImage, new Rectangle(position, currentImage.Size));

            // Offset.
			if (currentImage != null)
			{
				position.X = position.X + currentImage.Width + 2;
			}

            // Do not re-draw text when in focus mode only (it looks awful).
            if ((e.State != TreeNodeStates.Focused) && (_editNode != node))
            {
                TextRenderer.DrawText(e.Graphics, node.Text, font, position, node.ForeColor);
            }
        }		

		/// <summary>
		/// Function to show the rename text box.
		/// </summary>
		/// <param name="node">Node to edit.</param>
		internal void ShowRenameBox(EditorTreeNode node)
		{
			Point nodePosition = Point.Empty;

			if (node == null)
			{
				return;
			}

			if (_renameBox == null)
			{
				_renameBox = new TextBox();
				_renameBox.Name = this.Name + "_EditBox";
				_renameBox.Visible = false;
				_renameBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
				_renameBox.BackColor = Color.White;
				_renameBox.ForeColor = Color.Black;
				_renameBox.Height = node.Bounds.Height;
				_renameBox.AcceptsTab = false;
				_renameBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
				this.Controls.Add(_renameBox);
			}

			// Wipe out the background.
			using (var g = this.CreateGraphics())
			{
				// Create graphics resources.
				if (_selectBrush == null)
				{
					_selectBrush = new SolidBrush(DarkFormsRenderer.MenuHilightBackground);
				}

				g.FillRectangle(_selectBrush, new Rectangle(0, node.Bounds.Y, ClientSize.Width, node.Bounds.Height));
			}

			nodePosition = node.Bounds.Location;
			nodePosition.X += node.Level * 16 + 24;
			nodePosition.Y++;
			if (node.CollapsedImage != null)
			{
				nodePosition.X += node.CollapsedImage.Width + 2;
			}

			_renameBox.Location = nodePosition;
			_renameBox.Width = ClientSize.Width - nodePosition.X;
			_renameBox.Text = node.Text;

			NodeLabelEditEventArgs editArgs = new NodeLabelEditEventArgs(node, node.Text);
			editArgs.CancelEdit = false;

			OnBeforeLabelEdit(editArgs);

			if (!editArgs.CancelEdit)
			{
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
			if ((_renameBox != null) && (_renameBox.Visible) && (_renameBox.Focused))
			{
				if (keyData == Keys.Escape)
				{
					HideRenameBox(true);
					return true;
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		/// <summary>
		/// Handles the LostFocus event of the _renameBox control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void _renameBox_LostFocus(object sender, EventArgs e)
		{
			HideRenameBox(false);
		}	

		/// <summary>
		/// Handles the KeyDown event of the _renameBox control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void _renameBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				HideRenameBox(false);
				e.Handled = true;
				return;
			}
		}

		/// <summary>
		/// Function to hide the rename box.
		/// </summary>
		/// <param name="canceled">TRUE if the edit was canceled, FALSE if not.</param>
		internal void HideRenameBox(bool canceled)
		{
			if (_renameBox != null)
			{
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
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorTreeView"/> class.
        /// </summary>
        public EditorTreeView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            base.DrawMode = TreeViewDrawMode.OwnerDrawAll;
        }
        #endregion
    }
}
