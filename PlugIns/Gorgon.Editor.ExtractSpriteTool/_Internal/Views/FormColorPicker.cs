#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: April 27, 2019 6:49:36 PM
// 
#endregion

using System;
using System.ComponentModel;
using ComponentFactory.Krypton.Toolkit;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics;

namespace Gorgon.Editor.ExtractSpriteTool
{
    /// <summary>
    /// A color picker dialog.
    /// </summary>
    internal partial class FormColorPicker
        : KryptonForm
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the original color.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GorgonColor OriginalColor
        {
            get => Picker.OriginalColor;
            set => Picker.OriginalColor = value;
        }

        /// <summary>
        /// Property to set or return the selected color.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GorgonColor Color
        {
            get => Picker.SelectedColor;
            set => Picker.SelectedColor = value;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the ColorChanged event of the Picker control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ColorChangedEventArgs"/> instance containing the event data.</param>
        private void Picker_ColorChanged(object sender, ColorChangedEventArgs e) => ButtonOk.Enabled = Picker.SelectedColor != Picker.OriginalColor;

        /// <summary>Raises the Load event.</summary>
        /// <param name="e">An EventArgs containing event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ButtonOk.Enabled = Picker.SelectedColor != Picker.OriginalColor;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ExtractSpriteTool.FormColorPicker"/> class.</summary>
        public FormColorPicker() => InitializeComponent();
        #endregion
    }
}
