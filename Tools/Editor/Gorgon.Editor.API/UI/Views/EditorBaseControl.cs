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
// Created: August 26, 2018 9:49:35 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;

namespace Gorgon.Editor.UI.Views
{
    /// <summary>
    /// The base user control for editor UI panes.
    /// </summary> 
    public partial class EditorBaseControl 
        : UserControl
    {
        #region Variables.
        // The theme palette.
        private IPalette _palette;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether or not the control is being used in a designer.
        /// </summary>
        [Browsable(false)]
        public bool IsDesignTime
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Handles the GlobalPaletteChanged event of the KryptonManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void KryptonManager_GlobalPaletteChanged(object sender, EventArgs e)
        {
            if (_palette != null)
            {
                _palette.PalettePaint -= Palette_PalettePaint;
            }

            _palette = KryptonManager.CurrentGlobalPalette;

            if (_palette != null)
            {
                _palette.PalettePaint += Palette_PalettePaint;
            }

            Invalidate();
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data. </param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_palette == null)
            {
                return;
            }

            PaletteState state = Enabled ? PaletteState.Normal : PaletteState.Disabled;
            Color backColor = _palette.GetBackColor1(PaletteBackStyle.ControlClient, state);
            using (var backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, e.ClipRectangle);
            }            
        }

        /// <summary>
        /// Handles the PalettePaint event of the Palette control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaletteLayoutEventArgs"/> instance containing the event data.</param>
        private void Palette_PalettePaint(object sender, PaletteLayoutEventArgs e) => Invalidate();
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorBaseControl"/> class.
        /// </summary>
        public EditorBaseControl()
        {
            IsDesignTime = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            InitializeComponent();

            _palette = KryptonManager.CurrentGlobalPalette;

            if (_palette != null)
            {
                _palette.PalettePaint += Palette_PalettePaint;
            }

            KryptonManager.GlobalPaletteChanged += KryptonManager_GlobalPaletteChanged;
        }
        #endregion
    }
}
