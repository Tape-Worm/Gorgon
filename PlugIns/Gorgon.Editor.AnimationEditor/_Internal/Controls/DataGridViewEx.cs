#region MIT
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
// Created: June 10, 2020 12:23:33 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Math;

namespace Gorgon.Editor.AnimationEditor
{
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
        private readonly List<DataGridViewCell> _dragCells = new();
        // Flag to indicate that the selection changed event should be fired or not.
        private int _noFireSelectEvent;
        #endregion

        #region Events.
        // The true event that is fired when selected cells are dragged.        
        private event EventHandler<CellsDragEventArgs> CellsDragEvent;

        /// <summary>
        /// Event fired when cells are dragged.
        /// </summary>
        [Category("Action"), Description("The event fired when the selected cells are dragged.")]
        public event EventHandler<CellsDragEventArgs> CellsDrag
        {
            add
            {
                if (value is null)
                {
                    CellsDragEvent = null;
                    return;
                }

                CellsDragEvent += value;
            }
            remove
            {
                if (value is null)
                {
                    return;
                }

                CellsDragEvent -= value;
            }
        }
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the message to display when no data is present.
        /// </summary>
        [Browsable(true), Description("Message to display when no data is present."), Category("Appearance")]
        public string NoDataMessage
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether dragging of selected cells is allowed or not.
        /// </summary>
        [Browsable(true), Description("Enables dragging of selected cells."), Category("Behavior"), DefaultValue(false)]
        public bool AllowCellDragging
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>Raises the <see cref="Control.MouseMove"/> event.</summary>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if ((AllowCellDragging) && (SelectedCells.Count > 0) && (_dragRegion != Rectangle.Empty) && (!_dragRegion.Contains(e.Location)))
            {
                _dragCells.AddRange(SelectedCells.OfType<DataGridViewCell>());

                EventHandler<CellsDragEventArgs> handler = CellsDragEvent;
                handler?.Invoke(this, new CellsDragEventArgs(_dragCells, e.Button));

                _dragRegion = Rectangle.Empty;
                _dragCells.Clear();                
            }

            // Disable the "drag to select".
            if ((SelectedCells.Count > 0) && (e.Button != MouseButtons.None))
            {                
                return;
            }

            base.OnMouseMove(e);
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.DataGridView.SelectionChanged"/> event.</summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains information about the event.</param>
        protected override void OnSelectionChanged(EventArgs e)
        {
            if (_noFireSelectEvent != 0)
            {
                return;
            }
            base.OnSelectionChanged(e);
        }

        /// <summary>Raises the <see cref="Control.MouseDown"/> event.</summary>
        /// <param name="e">A <see cref="MouseEventArgs"/> that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if ((IsCurrentCellInEditMode) || (!AllowCellDragging))
            {
                base.OnMouseDown(e);
                return;
            }

            Focus();

            _dragCells.Clear();
            _dragRegion = Rectangle.Empty;
            _passThruEventArgs = null;

            HitTestInfo hit = HitTest(e.X, e.Y);

            if ((hit.RowIndex == -1) || (hit.ColumnIndex == -1) || (Columns[hit.ColumnIndex].Frozen) || (!Columns[hit.ColumnIndex].ReadOnly))
            {
                base.OnMouseDown(e);
                return;
            }            

            DataGridViewCell cell = Rows[hit.RowIndex].Cells[hit.ColumnIndex];

            if ((!cell.Selected)
                && (SelectedCells.Count > 0)
                && ((ModifierKeys & Keys.Shift) != Keys.Shift)
                && ((ModifierKeys & Keys.Control) != Keys.Control))
            {
                Interlocked.Exchange(ref _noFireSelectEvent, 1);
                ClearSelection();
                Interlocked.Exchange(ref _noFireSelectEvent, 0);

                cell.Selected = true;                
            }

            if ((SelectedCells.Count > 0) && (cell.Selected))
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
            if (AllowCellDragging)
            {
                _dragCells.Clear();
                _dragRegion = Rectangle.Empty;

                if ((_passThruEventArgs is not null) && (e.Button != MouseButtons.Right))
                {
                    // Fire the actual event if we're dragging.
                    base.OnMouseDown(_passThruEventArgs);
                    _passThruEventArgs = null;
                }
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

        /// <summary>Raises the <see cref="Control.Paint"/> event.</summary>
        /// <param name="e">A <see cref="PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if ((string.IsNullOrWhiteSpace(NoDataMessage)) || (Rows is null) || (Font is null) || (Rows.Count != 0))
            {
                return;
            }
                        
            SizeF textSize = e.Graphics.MeasureString(NoDataMessage, Font, new SizeF(ClientSize.Width, ClientSize.Height));
            var pos = new PointF(ClientSize.Width * 0.5f - textSize.Width * 0.5f, ClientSize.Height * 0.5f - textSize.Height * 0.5f);
            using Brush brush = new SolidBrush(ForeColor);
            e.Graphics.DrawString(NoDataMessage, Font, brush, pos);
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
                CellsDragEvent = null;
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="DataGridViewEx"/> class.</summary>
        public DataGridViewEx() => DoubleBuffered = true;
        #endregion
    }
}
