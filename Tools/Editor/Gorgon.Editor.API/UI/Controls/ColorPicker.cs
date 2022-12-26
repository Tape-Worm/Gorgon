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
// Created: March 28, 2019 9:37:16 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Gorgon.Graphics;

namespace Gorgon.Editor.UI.Controls;

/// <summary>
/// A control used to select a color value.
/// </summary>
public partial class ColorPicker
    : UserControl
{
    #region Events.
    // Event triggered when the color is changed on the control.
    private event EventHandler<ColorChangedEventArgs> ColorChangedEvent;

    /// <summary>
    /// Event triggered when the color is changed on the control.
    /// </summary>
    [Browsable(true), Category("Behavior"), Description("Triggered with the color was changed on the control.")]
    public event EventHandler<ColorChangedEventArgs> ColorChanged
    {
        add
        {
            if (value is null)
            {
                ColorChangedEvent = null;
                return;
            }

            ColorChangedEvent += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }

            ColorChangedEvent -= value;
        }
    }
    #endregion

    #region Properties.
    /// <summary>
    /// Property to set or return the color selection.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public GorgonColor SelectedColor
    {
        get => Picker.SelectedColor;
        set => Picker.SelectedColor = value;
    }

    /// <summary>
    /// Property to set or return the original color.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public GorgonColor OriginalColor
    {
        get => Picker.OldColor;
        set => Picker.OldColor = value;
    }
    #endregion

    #region Methods.
    /// <summary>Handles the ColorChanged event of the Picker control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Picker_ColorChanged(object sender, EventArgs e)
    {
        EventHandler<ColorChangedEventArgs> handler = ColorChangedEvent;
        handler?.Invoke(this, new ColorChangedEventArgs(SelectedColor, OriginalColor));
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="ColorPicker"/> class.</summary>
    public ColorPicker() => InitializeComponent();
    #endregion
}
