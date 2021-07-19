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
// Created: July 7, 2020 12:31:02 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Math;

namespace Gorgon.Editor.UI.Controls
{
    /// <summary>
    /// A control used to pick an alpha (transparency) value.
    /// </summary>
    public partial class AlphaPicker 
        : UserControl
    {
        #region Variables.
        // The position of the cursor indicator.
        private float _alphaValue;
        // The points for the cursor polygon.
        private readonly PointF[] _cursor = new PointF[3];
        // Flag to indicate that the numeric control events are enabled.
        private int _numericEventsEnabled = 1;
        #endregion

        #region Events.
        // Event triggered when the alpha value was changed.
        private event EventHandler AlphaValueChangedEvent;

        /// <summary>
        /// Event triggered when the alpha value was changed.
        /// </summary>
        [Category("Property Changed"), Description("Event triggered when the AlphaValue property is changed.")]
        public event EventHandler AlphaValueChanged
        {
            add
            {
                if (value is null)
                {
                    AlphaValueChangedEvent = null;
                    return;
                }

                AlphaValueChangedEvent += value;
            }
            remove
            {
                if (value is null)
                {
                    return;
                }

                AlphaValueChangedEvent -= value;
            }
        }
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the alpha value.
        /// </summary>
        [Browsable(true), Description("Sets the alpha value."), Category("Data")]
        public int AlphaValue
        {
            get => (int)(_alphaValue * 255);
            set => SetAlphaValue(value / 255.0f);
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to disable the events on the numeric control.
        /// </summary>
        private void DisableNumericEvents()
        {
            if (Interlocked.Exchange(ref _numericEventsEnabled, 0) == 0)
            {
                return;
            }

            NumericAlpha.ValueChanged -= NumericAlpha_ValueChanged;
        }

        /// <summary>
        /// Function to enable the events on the numeric control.
        /// </summary>
        private void EnableNumericEvents()
        {
            if (Interlocked.Exchange(ref _numericEventsEnabled, 1) == 1)
            {
                return;
            }

            NumericAlpha.ValueChanged += NumericAlpha_ValueChanged;
        }

        /// <summary>Handles the ValueChanged event of the NumericAlpha control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericAlpha_ValueChanged(object sender, EventArgs e) => SetAlphaValue((float)(NumericAlpha.Value / 255));

        /// <summary>Sets the alpha value.</summary>
        /// <param name="alphaValue">The alpha value.</param>
        private void SetAlphaValue(float alphaValue)
        {
            DisableNumericEvents();

            try
            {
                _alphaValue = alphaValue.Max(0).Min(1.0f);
                NumericAlpha.Value = (int)(_alphaValue * 255);

                PanelIndicator.Invalidate();

                AlphaValueChangedEvent?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                EnableNumericEvents();
            }
        }

        /// <summary>Handles the Paint event of the PanelAlpha control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void PanelAlpha_Paint(object sender, PaintEventArgs e)
        {
            using var brush = new LinearGradientBrush(PanelAlpha.ClientRectangle, Color.FromArgb(0, 0, 0, 0), Color.White, LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(brush, PanelAlpha.ClientRectangle);
        }

        /// <summary>Handles the Paint event of the PanelIndicator control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void PanelIndicator_Paint(object sender, PaintEventArgs e)
        {
            float dpiScale = e.Graphics.DpiY / 96.0f;
            float cursorPosition = (_alphaValue * PanelAlpha.ClientSize.Width) + PanelAlpha.ClientRectangle.X + PanelAlpha.Margin.Left;
            _cursor[0] = new PointF(cursorPosition, 0);
            _cursor[1] = new PointF(cursorPosition + (5 * dpiScale), 5 * dpiScale);
            _cursor[2] = new PointF(cursorPosition - (5 * dpiScale), 5 * dpiScale);

            e.Graphics.FillPolygon(Brushes.White, _cursor);
        }

        /// <summary>Handles the MouseDown event of the PanelAlpha control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void PanelAlpha_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.None)
            {
                return;
            }

            SetAlphaValue(e.X / (float)PanelAlpha.ClientSize.Width);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="AlphaPicker"/> class.</summary>
        public AlphaPicker() => InitializeComponent();
        #endregion

    }
}
