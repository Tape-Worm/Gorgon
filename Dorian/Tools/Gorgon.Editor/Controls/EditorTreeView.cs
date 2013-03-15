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

namespace GorgonLibrary.Editor
{
    /// <summary>
    /// Custom treeview for the editor.
    /// </summary>
    class EditorTreeView
        : TreeView
    {
        #region Variables.
        private bool _disposed = false;                 // Flag to indicate that the object was disposed.
        private Font _openContent = null;               // Font used for open content items.
        private Brush _selectBrush = null;              // Brush used for selection background.
        private Pen _focusPen = null;                   // Pen used for focus.
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
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                var node = this.GetNodeAt(e.Location);

                if (node != null)
                {
                    this.SelectedNode = node;
                }
            }
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
            position.X = position.X + currentImage.Width + 2;			

            // Do not re-draw text when in focus mode only (it looks awful).
            if (e.State != TreeNodeStates.Focused)
            {
                TextRenderer.DrawText(e.Graphics, node.Text, font, position, node.ForeColor);
            }

			//return;
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
