#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: August 28, 2021 9:17:30 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// A combo box for displaying fonts.
/// </summary>
internal class ComboFonts
    : ComboBox
{
    #region Properties.
    /// <summary>
    /// N/A
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public new DrawMode DrawMode
    {
        get => DrawMode.OwnerDrawVariable;
        // ReSharper disable once ValueParameterNotUsed
        set
        {
        }
    }

    /// <summary>
    /// N/A
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public new bool Sorted => true;
    #endregion

    #region Methods.
    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.ComboBox.MeasureItem" /> event.
    /// </summary>
    /// <param name="e">The <see cref="T:System.Windows.Forms.MeasureItemEventArgs" /> that was raised.</param>
    protected override void OnMeasureItem(MeasureItemEventArgs e)
    {
        base.OnMeasureItem(e);

        if ((e.Index < 0)
            || (e.Index >= Items.Count))
        {
            return;
        }

        e.ItemHeight = 20;
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.ComboBox.DrawItem"/> event.
    /// </summary>
    /// <param name="e">A <see cref="T:System.Windows.Forms.DrawItemEventArgs"/> that contains the event data.</param>
    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        base.OnDrawItem(e);

        TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.Left
                                | TextFormatFlags.SingleLine
                                | TextFormatFlags.VerticalCenter;

        if ((e.Index < 0)
            || (e.Index >= Items.Count))
        {
            return;
        }

        e.DrawBackground();

        if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
        {
            e.DrawFocusRectangle();
        }

        string fontName = Items[e.Index].ToString();

        if (!FontEditorPlugin.CachedFonts.TryGetValue(fontName, out Font font))
        {
            font = Font;
        }

        e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        Size measure = TextRenderer.MeasureText(e.Graphics, fontName, Font, e.Bounds.Size, flags);
        var textBounds = new Rectangle(e.Bounds.Width - measure.Width + e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
        var fontBounds = new Rectangle(e.Bounds.Left, e.Bounds.Top, textBounds.X - 2, e.Bounds.Height);
        TextRenderer.DrawText(e.Graphics, fontName, font, fontBounds, Enabled ? e.ForeColor : Color.DimGray, e.BackColor, flags);
        TextRenderer.DrawText(e.Graphics, fontName, Font, textBounds, Enabled ? e.ForeColor : Color.DimGray, e.BackColor, flags);
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.ComboBox.DropDown"/> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
    protected override void OnDropDown(EventArgs e)
    {
        base.OnDropDown(e);

        if (DesignMode)
        {
            return;
        }

        string previousSelection = Text;
        RefreshFonts(false);
        Text = previousSelection;
    }

    /// <summary>
    /// Function to refresh the font list.
    /// </summary>
    /// <param name="clear"><b>true</b> to clear the initial list, <b>false</b> to just merge new entries.</param>
    public void RefreshFonts(bool clear)
    {
        IEnumerable<string> families = FontFamily.Families.Select(item => item.Name)
                                                          .OrderBy(item => item, StringComparer.CurrentCultureIgnoreCase);

        if (clear)
        {
            Items.Clear();
        }
        else
        {
            IEnumerable<string> items = Items.Cast<string>()
                                             .Where(item => !families.Any(subItem => string.Equals(subItem, item, StringComparison.CurrentCultureIgnoreCase)))
                                             .ToArray();
            
            foreach (string item in items)
            {
                Items.Remove(item);
            }

            families = families.Where(item => !Items.Cast<string>().Any(subItem => string.Equals(subItem, item, StringComparison.CurrentCultureIgnoreCase)))
                               .OrderBy(item => item, StringComparer.CurrentCultureIgnoreCase);                
        }

        foreach (string font in families)
        {
            if (Items.Contains(font))
            {
                continue;
            }

            Items.Add(font);
        }
    }
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="ComboFonts"/> class.
    /// </summary>
    public ComboFonts()
    {
        base.DrawMode = DrawMode.OwnerDrawVariable;
        base.Sorted = true;
    }
    #endregion
}
