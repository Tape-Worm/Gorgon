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
// Created: December 11, 2018 8:38:27 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Gorgon.UI;

/// <summary>
/// An enhancement to the standard text box to show a cue value when no text is available.
/// </summary>
public class GorgonCueTextBox
    : TextBox
{
    #region Constants.
    // Paint message.
    private const int WM_PAINT = 0x000F;
    #endregion

    #region Variables.
    // Text for the cue.
    private string _cueText = string.Empty;
    // Font used to draw the cue.
    private Font _cueFont;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to set or return the text for the textbox cue.
    /// </summary>
    [Browsable(true), Category("Appearance"), Description("Sets the value to display when no text is displayed in the text box.")]
    public string CueText
    {
        get => _cueText;
        set
        {
            value ??= string.Empty;

            _cueText = value;
            Invalidate();
        }
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to create the font used for the cue text.
    /// </summary>
    private void CreateCueFont()
    {
        _cueFont?.Dispose();
        _cueFont = new Font(Font, FontStyle.Italic);
    }

    /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.FontChanged"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        CreateCueFont();
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.FontChanged"/> event when the <see cref="P:System.Windows.Forms.Control.Font"/> property value of the control's container changes.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnParentFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);

        if (Font != Parent?.Font)
        {
            return;
        }
        
        CreateCueFont();
    }

    /// <summary>
    /// Processes Windows messages.
    /// </summary>
    /// <param name="m">A Windows Message object.</param>
    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case WM_PAINT:
                base.WndProc(ref m);

                if ((!string.IsNullOrEmpty(Text))
                    || (string.IsNullOrWhiteSpace(_cueText))
                    || (Focused))                    
                {                        
                    break;
                }

                using (System.Drawing.Graphics g = CreateGraphics())
                {
                    // If the font has not been created, then create it now.
                    _cueFont ??= new Font(Font, FontStyle.Italic);
                    using var brush = new SolidBrush(BackColor);
                    g.FillRectangle(brush, ClientRectangle);
                    TextRenderer.DrawText(g, _cueText, _cueFont, ClientRectangle, Color.FromKnownColor(KnownColor.GrayText), TextFormatFlags.Left);
                }
                break;
            default:
                base.WndProc(ref m);
                break;
        }
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.Leave" /> event.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
    protected override void OnLeave(EventArgs e)
    {
        Invalidate();
        base.OnLeave(e);
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.Enter" /> event.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
    protected override void OnEnter(EventArgs e)
    {
        Invalidate();
        if (!string.IsNullOrEmpty(Text))
        {
            SelectAll();
        }
        base.OnEnter(e);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="TextBox" /> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cueFont?.Dispose();
            _cueFont = null;
        }

        base.Dispose(disposing);
    }
    #endregion
}
