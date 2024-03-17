
// 
// Gorgon
// Copyright (C) 2019 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: November 24, 2019 7:59:40 PM
// 


using System.ComponentModel;

namespace Gorgon.Editor.Views;

/// <summary>
/// An extended tree
/// </summary>
internal class TreeEx
    : TreeView
{

    /// <summary>
    /// The event fired when the node edit is canceled.
    /// </summary>
    public event EventHandler EditCanceled;



    // Text box used to rename a node.
    private TextBox _renameBox;
    // The node being renamed.
    private TreeNode _renameNode;
    // The points for the expand/collapse icon polygon.
    private readonly PointF[] _expandIconPoints = new PointF[3];



    /// <summary>Gets or sets a value indicating whether the label text of the tree nodes can be edited.</summary>
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool LabelEdit
    {
        get => false;
        set
        {
        }
    }

    /// <summary>Gets or sets a value indicating whether plus-sign (+) and minus-sign (-) buttons are displayed next to tree nodes that contain child tree nodes.</summary>
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool ShowPlusMinus
    {
        get => false;
        set
        {
        }
    }

    /// <summary>Gets or sets a value indicating whether lines are drawn between the tree nodes that are at the root of the tree view.</summary>
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool ShowRootLines
    {
        get => false;
        set
        {
        }
    }

    /// <summary>Gets or sets the color of the lines connecting the nodes of the <see cref="TreeView"/> control.</summary>
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Color LineColor
    {
        get => BackColor;
        set
        {
        }
    }


    /// <summary>Gets or sets a value indicating whether the selected tree node remains highlighted even when the tree view has lost the focus.</summary>
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool HideSelection
    {
        get => true;
        set
        {
        }
    }

    /// <summary>Gets or sets a value indicating whether a tree node label takes on the appearance of a hyperlink as the mouse pointer passes over it.</summary>
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool HotTracking
    {
        get => false;
        set
        {
        }
    }

    /// <summary>Gets or sets a value indicating whether lines are drawn between tree nodes in the tree view control.</summary>
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool ShowLines
    {
        get => false;
        set
        {
        }
    }

    /// <summary>Gets or sets a value indicating whether the selection highlight spans the width of the tree view control.</summary>
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool FullRowSelect
    {
        get => true;
        set
        {
            // Not used.
        }
    }

    /// <summary>Gets or sets a value indicating whether check boxes are displayed next to the tree nodes in the tree view control.</summary>
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool CheckBoxes
    {
        get => false;
        set
        {
            // Not used.
        }
    }

    /// <summary>Gets or sets the mode in which the control is drawn.</summary>
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new TreeViewDrawMode DrawMode
    {
        get => TreeViewDrawMode.OwnerDrawAll;
        set
        {
            // Not used.
        }
    }

    /// <summary>
    /// Property to set or return the foreground color for a selected node.
    /// </summary>
    [Category("Appearance"), Description("The foreground color for a selected node."), DefaultValue(typeof(Color), "White"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color SelectedNodeForeColor
    {
        get;
        set;
    } = Color.White;

    /// <summary>
    /// Property to set or return the background color for a selected node.
    /// </summary>
    [Category("Appearance"), Description("The background color for a selected node."), DefaultValue(typeof(Color), "SteelBlue"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color SelectedNodeBackColor
    {
        get;
        set;
    } = Color.SteelBlue;

    /// <summary>
    /// Property to return whether a node is editing or not.
    /// </summary>
    [Browsable(false)]
    public bool IsEditing => _renameNode is not null;



    /// <summary>
    /// Handles the LostFocus event of the _renameBox control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    /// <exception cref="NotSupportedException"></exception>
    private void RenameBox_LostFocus(object sender, EventArgs e) => HideRenameBox(true);

    /// <summary>
    /// Handles the KeyDown event of the _renameBox control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
    /// <exception cref="NotSupportedException"></exception>
    private void RenameBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        HideRenameBox(false);
        e.Handled = true;
    }

    /// <summary>
    /// Function to show a rename textbox for renaming a tree node.
    /// </summary>
    /// <param name="node">The node being renamed.</param>
    private void ShowRenameBox(TreeNode node)
    {
        if (node is null)
        {
            return;
        }

        _renameNode = node;

        if (_renameBox is null)
        {
            _renameBox = new TextBox
            {
                Name = Name + "_RenameEditBox",
                Visible = false,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Height = node.Bounds.Height,
                AcceptsTab = false,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            Controls.Add(_renameBox);

            _renameBox.KeyDown += RenameBox_KeyDown;
            _renameBox.LostFocus += RenameBox_LostFocus;
        }

        _renameBox.Text = node.Text;
        _renameBox.Top = node.Bounds.Top + 1;
        _renameBox.Left = 3;
        _renameBox.Width = ClientSize.Width - 6;
        _renameBox.Visible = true;
        _renameBox.Select();
        _renameBox.SelectAll();
    }

    /// <summary>
    /// Function to hide the textbox used for renaming a node.
    /// </summary>
    /// <param name="cancel"><b>true</b> to cancel the renaming operation, <b>false</b> to continue it.</param>
    private void HideRenameBox(bool cancel)
    {
        if ((_renameBox is null) || (!_renameBox.Visible))
        {
            return;
        }

        TreeNode node = Interlocked.Exchange(ref _renameNode, null);

        if (node is null)
        {
            return;
        }

        if (!cancel)
        {
            NodeLabelEditEventArgs args = new(node, _renameBox.Text);
            OnAfterLabelEdit(args);

            if (!args.CancelEdit)
            {
                node.Text = _renameBox.Text;
            }
        }
        else
        {
            EventHandler handler = EditCanceled;
            handler?.Invoke(this, EventArgs.Empty);
        }

        Select();
        _renameBox.Visible = false;
        _renameNode = null;
    }

    /// <summary>
    /// Sends the specified message to the default window procedure.
    /// </summary>
    /// <param name="m">The Windows <see cref="Message" /> to process.</param>
    protected override void DefWndProc(ref Message m)
    {
        const int WM_LBUTTONDBLCLK = 0x203;

        // Override the double click so that the tree doesn't expand on file nodes.
        if (m.Msg == WM_LBUTTONDBLCLK)
        {
            Point mousePos = PointToClient(Cursor.Position);
            OnDoubleClick(EventArgs.Empty);
            OnMouseDoubleClick(new MouseEventArgs(MouseButtons.Left, 2, mousePos.X, mousePos.Y, 0));
            return;
        }

        base.DefWndProc(ref m);
    }

    /// <summary>Releases the unmanaged resources used by the <see cref="TreeView"/> and optionally releases the managed resources.</summary>
    /// <param name="disposing">
    ///   <span class="keyword">
    ///     <span class="languageSpecificText">
    ///       <span class="cs">true</span>
    ///       <span class="vb">True</span>
    ///       <span class="cpp">true</span>
    ///     </span>
    ///   </span>
    ///   <span class="nu">
    ///     <span class="keyword">true</span> (<span class="keyword">True</span> in Visual Basic)</span> to release both managed and unmanaged resources; <span class="keyword"><span class="languageSpecificText"><span class="cs">false</span><span class="vb">False</span><span class="cpp">false</span></span></span><span class="nu"><span class="keyword">false</span> (<span class="keyword">False</span> in Visual Basic)</span> to release only unmanaged resources.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_renameBox is not null)
            {
                _renameBox.KeyDown -= RenameBox_KeyDown;
                _renameBox.LostFocus -= RenameBox_LostFocus;
                _renameBox.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    /// <summary>Processes a command key.</summary>
    /// <param name="msg">A <see cref="Message"/>, passed by reference, that represents the window message to process.</param>
    /// <param name="keyData">One of the <see cref="Keys"/> values that represents the key to process.</param>
    /// <returns>
    ///   <span class="keyword">
    ///     <span class="languageSpecificText">
    ///       <span class="cs">true</span>
    ///       <span class="vb">True</span>
    ///       <span class="cpp">true</span>
    ///     </span>
    ///   </span>
    ///   <span class="nu">
    ///     <span class="keyword">true</span> (<span class="keyword">True</span> in Visual Basic)</span> if the character was processed by the control; otherwise, <span class="keyword"><span class="languageSpecificText"><span class="cs">false</span><span class="vb">False</span><span class="cpp">false</span></span></span><span class="nu"><span class="keyword">false</span> (<span class="keyword">False</span> in Visual Basic)</span>.
    /// </returns>
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if ((!(_renameBox?.Visible ?? false)) || (!_renameBox.Focused))
        {
            if ((keyData == Keys.F2) && (SelectedNode is not null))
            {
                TreeNode node = SelectedNode;
                NodeLabelEditEventArgs args = new(node);
                OnBeforeLabelEdit(args);

                if (args.CancelEdit)
                {
                    return true;
                }

                ShowRenameBox(node);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        switch (keyData)
        {
            case Keys.Escape:
                HideRenameBox(true);
                return true;
            default:
                return base.ProcessCmdKey(ref msg, keyData);
        }
    }

    /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.MouseDown"/> event.</summary>
    /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
    protected override void OnMouseDown(MouseEventArgs e)
    {
        TreeViewCancelEventArgs args = null;
        TreeViewHitTestInfo hit = HitTest(e.Location);

        if ((SelectedNode is not null) && (hit.Node is null))
        {
            args = new TreeViewCancelEventArgs(null, false, TreeViewAction.ByMouse);
            OnBeforeSelect(args);

            if (args.Cancel)
            {
                base.OnMouseDown(e);
                return;
            }
        }

        SelectedNode = hit.Node;

        if (args is not null)
        {
            OnAfterSelect(new TreeViewEventArgs(null, TreeViewAction.ByMouse));
        }

        base.OnMouseDown(e);
    }

    /// <summary>Raises the <see cref="Control.GotFocus"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnGotFocus(EventArgs e)
    {
        Refresh();
        base.OnGotFocus(e);
    }

    /// <summary>Raises the <see cref="Control.LostFocus"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnLostFocus(EventArgs e)
    {
        Refresh();
        base.OnLostFocus(e);
    }

    /// <summary>Raises the <see cref="TreeView.DrawNode"/> event.</summary>
    /// <param name="e">A <see cref="DrawTreeNodeEventArgs"/> that contains the event data.</param>
    protected override void OnDrawNode(DrawTreeNodeEventArgs e)
    {
        Point position = e.Bounds.Location;
        Color foreColor;

        if (((e.Bounds.Width == 0) || (e.Bounds.Height == 0)) && (e.Bounds.X == 0) && (e.Bounds.Y == 0))
        {
            return;
        }

        if (e.Node is null)
        {
            e.DrawDefault = true;
            return;
        }

        Image nodeIcon = null;

        foreColor = (e.Node is DirectoryTreeNode dirNode) ? dirNode.ForeColor : e.Node.ForeColor;

        if ((ImageList is not null) && (ImageList.Images.Count > 0)
            && ((ImageIndex > -1) || (e.Node.ImageIndex > -1)))
        {
            int index = e.Node.ImageIndex > -1 ? e.Node.ImageIndex : ImageIndex;
            nodeIcon = ImageList.Images[index];
        }

        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

        position.X += (e.Node.Level * ((nodeIcon?.Width ?? 16) + 3)) + 8;
        float heightMid = ItemHeight * 0.5f;

        if ((e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected)
        {
            float cr = (SelectedNodeBackColor.R / 255.0f);
            float cg = (SelectedNodeBackColor.G / 255.0f);
            float cb = (SelectedNodeBackColor.B / 255.0f);

            if (e.Node.BackColor != Color.Empty)
            {
                cr = cr * 0.5f + (e.Node.BackColor.R / 255.0f) * 0.5f;
                cg = cg * 0.5f + (e.Node.BackColor.G / 255.0f) * 0.5f;
                cb = cb * 0.5f + (e.Node.BackColor.B / 255.0f) * 0.5f;
            }

            Color combined = Color.FromArgb((int)(cr * 255), (int)(cg * 255), (int)(cb * 255));

            if (Focused)
            {
                using SolidBrush brush = new(combined);
                e.Graphics.FillRectangle(brush, e.Bounds);
            }
            else
            {
                using Pen pen = new(SelectedNodeBackColor);
                Rectangle r = new(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height);

                if (e.Node.BackColor != Color.Empty)
                {
                    using SolidBrush brush = new(combined);
                    e.Graphics.FillRectangle(brush, r);
                }

                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

                e.Graphics.DrawRectangle(pen, r);
            }

            cr = (SelectedNodeForeColor.R / 255.0f);
            cg = (SelectedNodeForeColor.G / 255.0f);
            cb = (SelectedNodeForeColor.B / 255.0f);

            if (foreColor != Color.Empty)
            {
                cr = cr * 0.5f + (foreColor.R / 255.0f) * 0.5f;
                cg = cg * 0.5f + (foreColor.G / 255.0f) * 0.5f;
                cb = cb * 0.5f + (foreColor.B / 255.0f) * 0.5f;
            }

            foreColor = Color.FromArgb((int)(cr * 255), (int)(cg * 255), (int)(cb * 255));
        }
        else
        {
            using SolidBrush brush = new(e.Node.BackColor);
            e.Graphics.FillRectangle(brush, e.Bounds);
        }

        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        if (e.Node.Nodes.Count > 0)
        {
            if (!e.Node.IsExpanded)
            {
                _expandIconPoints[0] = new PointF(position.X + 1, position.Y + heightMid - 6);
                _expandIconPoints[1] = new PointF(position.X + 7, position.Y + heightMid - 1);
                _expandIconPoints[2] = new PointF(position.X + 1, position.Y + heightMid + 4);
                e.Graphics.DrawPolygon(Pens.White, _expandIconPoints);
            }
            else
            {
                _expandIconPoints[0] = new PointF(position.X + 7, position.Y + heightMid - 4);
                _expandIconPoints[1] = new PointF(position.X + 7, position.Y + heightMid + 4);
                _expandIconPoints[2] = new PointF(position.X, position.Y + heightMid + 4);
                e.Graphics.FillPolygon(Brushes.White, _expandIconPoints);
            }
        }

        position.X += 19;

        if (nodeIcon is not null)
        {
            e.Graphics.DrawImage(nodeIcon,
                                 new Rectangle(position.X, position.Y + (int)(heightMid - nodeIcon.Size.Height * 0.5f), nodeIcon.Size.Width, nodeIcon.Size.Height),
                                 0,
                                 0,
                                 nodeIcon.Width,
                                 nodeIcon.Height,
                                 GraphicsUnit.Pixel);

            position.X += nodeIcon.Width + 3;
        }
        else
        {
            position.X += 19;
        }

        Font nodeFont = e.Node.NodeFont ?? Font;
        TextRenderer.DrawText(e.Graphics, e.Node.Text, nodeFont, new Rectangle(position.X, e.Bounds.Top, e.Bounds.Width - position.X, ItemHeight), foreColor, TextFormatFlags.VerticalCenter);
    }

    /// <summary>
    /// Function to begin editing of a node.
    /// </summary>
    public void BeginEdit()
    {
        TreeNode node = SelectedNode;

        if ((node is null) || (IsEditing))
        {
            return;
        }

        ShowRenameBox(node);
    }

    /// <summary>
    /// Function end and commit the editing of a node.
    /// </summary>
    public void EndEdit()
    {
        if (!IsEditing)
        {
            return;
        }

        HideRenameBox(false);
    }

    /// <summary>
    /// Function cancel the editing of a node.
    /// </summary>
    public void CancelEdit()
    {
        if (!IsEditing)
        {
            return;
        }

        HideRenameBox(true);
    }



    /// <summary>Initializes a new instance of the <see cref="TreeEx"/> class.</summary>
    public TreeEx()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        base.DrawMode = TreeViewDrawMode.OwnerDrawAll;

        CheckBoxes = false;
        base.HideSelection = false;
        ShowPlusMinus = true;
        ShowRootLines = ShowLines = false;
        FullRowSelect = true;
    }

}
