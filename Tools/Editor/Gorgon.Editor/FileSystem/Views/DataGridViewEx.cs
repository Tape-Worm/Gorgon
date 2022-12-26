﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: January 11, 2020 3:32:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Math;

namespace Gorgon.Editor.Views;

/// <summary>
/// A data grid view with extended functionality.
/// </summary>
internal class DataGridViewEx
    : DataGridView
{
    #region Variables.
    // The region for dragging.
    private Rectangle _dragRegion;
    // Pass through event arguments.
    private MouseEventArgs _passThruEventArgs;
    // The list of rows for dragging.
    private readonly List<DataGridViewRow> _dragRows = new();
    #endregion

    #region Events.
    // Event fired when rows are dragged.
    private event EventHandler<RowsDragEventArgs> RowsDragEvent;

    /// <summary>
    /// Event fired when rows are dragged.
    /// </summary>
    [Category("Action"), Description("The event fired when the selected rows are dragged.")]
    public event EventHandler<RowsDragEventArgs> RowsDrag
    {
        add
        {
            if (value is null)
            {
                RowsDragEvent = null;
                return;
            }

            RowsDragEvent += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }

            RowsDragEvent -= value;
        }
    }
    #endregion

    #region Methods.
    /// <summary>Processes keys used for navigating in the <see cref="DataGridView"/>.</summary>
    /// <param name="e">Contains information about the key that was pressed.</param>
    /// <returns>
    ///   <span class="keyword">
    ///     <span class="languageSpecificText">
    ///       <span class="cs">true</span>
    ///       <span class="vb">True</span>
    ///       <span class="cpp">true</span>
    ///     </span>
    ///   </span>
    ///   <span class="nu">
    ///     <span class="keyword">true</span> (<span class="keyword">True</span> in Visual Basic)</span> if the key was processed; otherwise, <span class="keyword"><span class="languageSpecificText"><span class="cs">false</span><span class="vb">False</span><span class="cpp">false</span></span></span><span class="nu"><span class="keyword">false</span> (<span class="keyword">False</span> in Visual Basic)</span>.
    /// </returns>
    protected override bool ProcessDataGridViewKey(KeyEventArgs e)
    {
        if (!IsCurrentCellInEditMode)
        {
            return base.ProcessDataGridViewKey(e);
        }

        // Stop these keys while editing. The only thing that should be commiting edited rows is the Enter key, or row focus is changed.
        switch (e.KeyCode)
        {
            case Keys.End:
            case Keys.Left:
            case Keys.Right:
                e.SuppressKeyPress = true;
                e.Handled = true;
                return true;
        }

        return base.ProcessDataGridViewKey(e);
    }

    /// <summary>Raises the <see cref="Control.MouseMove"/> event.</summary>
    /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        if ((SelectedRows.Count > 0) && (_dragRegion != Rectangle.Empty) && (!_dragRegion.Contains(e.Location)))
        {
            _dragRows.AddRange(SelectedRows.OfType<DataGridViewRow>());

            EventHandler<RowsDragEventArgs> handler = RowsDragEvent;
            handler?.Invoke(this, new RowsDragEventArgs(_dragRows, e.Button));

            _dragRegion = Rectangle.Empty;
            _dragRows.Clear();
        }

        base.OnMouseMove(e);
    }

    /// <summary>Raises the <see cref="Control.MouseDown"/> event.</summary>
    /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (IsCurrentCellInEditMode)
        {
            base.OnMouseDown(e);
            return;    
        }

        Focus();

        _dragRows.Clear();
        _dragRegion = Rectangle.Empty;
        _passThruEventArgs = null;

        HitTestInfo hit = HitTest(e.X, e.Y);

        if (hit.RowIndex == -1)
        {
            base.OnMouseDown(e);
            return;
        }

        DataGridViewRow row = Rows[hit.RowIndex];            

        if ((!row.Selected) 
            && (SelectedRows.Count > 0) 
            && ((ModifierKeys & Keys.Shift) != Keys.Shift) 
            && ((ModifierKeys & Keys.Control) != Keys.Control))
        {
            ClearSelection();
            row.Selected = true;
        }            

        if ((SelectedRows.Count > 0) && (row.Selected))
        {
            _passThruEventArgs = e;
            float dpiScale = DeviceDpi / 96.0f;
            var dragSize = new Size((int)(SystemInformation.DragSize.Width * dpiScale).FastCeiling() * 2, (int)(SystemInformation.DragSize.Height * dpiScale).FastCeiling() * 2);
            var dragLocation = new Point((int)(e.Location.X - dragSize.Width / 2.0f).FastFloor(), (int)(e.Location.Y - dragSize.Height / 2.0f).FastFloor());
            _dragRegion = new Rectangle(dragLocation, dragSize);
            return;
        }
        
        base.OnMouseDown(e);
    }

    /// <summary>Raises the <see cref="Control.MouseUp"/> event.</summary>
    /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
    protected override void OnMouseUp(MouseEventArgs e)
    {
        _dragRows.Clear();
        _dragRegion = Rectangle.Empty;

        if ((_passThruEventArgs is not null) && (e.Button != MouseButtons.Right))
        {
            // Fire the actual event if we're dragging.
            base.OnMouseDown(_passThruEventArgs);
            _passThruEventArgs = null;
        }

        base.OnMouseUp(e);
    }

    /// <summary>Raises the <see cref="Control.LostFocus"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnLostFocus(EventArgs e)
    {
        Refresh();
        base.OnLostFocus(e);
    }

    /// <summary>Raises the <see cref="Control.GotFocus"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnGotFocus(EventArgs e)
    {
        Refresh();            
        base.OnGotFocus(e);
    }

    /// <summary>Raises the <see cref="DataGridView.CellPainting"/> event.</summary>
    /// <param name="e">A <see cref="DataGridViewCellPaintingEventArgs"/> that contains the event data.</param>
    protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
    {
        base.OnCellPainting(e);
        if ((Focused) 
            || ((e.State & DataGridViewElementStates.Selected) != DataGridViewElementStates.Selected) 
            || ((e.PaintParts & DataGridViewPaintParts.Background) != DataGridViewPaintParts.Background)
            || (!Columns[e.ColumnIndex].Visible)
            || (IsCurrentCellInEditMode))
        {                
            return;
        }

        e.PaintBackground(new Rectangle(0, e.CellBounds.Y, ClientSize.Width - 1, e.CellBounds.Height), false);            

        using (var pen = new Pen(e.CellStyle.SelectionBackColor))
        {                
            var r = new Rectangle(e.ClipBounds.X, e.CellBounds.Y, e.ClipBounds.Width - 1, e.CellBounds.Height - 1);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            e.Graphics.DrawRectangle(pen, r);
        }
        
        e.PaintContent(e.ClipBounds);

        e.Handled = true;
    }

    /// <summary>Releases the unmanaged resources used by the <see cref="Control"/> and its child controls and optionally releases the managed resources.</summary>
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
            RowsDragEvent = null;
        }

        base.Dispose(disposing);
    }
    #endregion
}
