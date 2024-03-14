#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: February 14, 2017 6:17:54 PM
// 
#endregion

using System.Drawing.Text;

namespace Gorgon.Graphics.Fonts;

/// <summary>
/// Defines a structure to hold GDI font data used for rendering glyphs.
/// </summary>
internal class GdiFontData
    : IDisposable
{
    #region Properties.
    /// <summary>
    /// Property to return the font used to draw the glyphs.
    /// </summary>
    public Font Font
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the string format used when performing glyph measuring.
    /// </summary>
    public StringFormat StringFormat
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the string format used when drawing a glyph.
    /// </summary>
    /// <remarks>
    /// This used because some glyphs are being clipped when they have overhang on the left boundary.
    /// </remarks>
    public StringFormat DrawFormat
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the height of the font, in pixels.
    /// </summary>
    public float FontHeight
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the height of a line using the font, in pixels.
    /// </summary>
    public float LineHeight => (FontHeight * Font.FontFamily.GetLineSpacing(Font.Style)) / Font.FontFamily.GetEmHeight(Font.Style);

    /// <summary>
    /// Property to return the ascent for the font, in pixels.
    /// </summary>
    public float Ascent => (FontHeight * Font.FontFamily.GetCellAscent(Font.Style)) / Font.FontFamily.GetEmHeight(Font.Style);

    /// <summary>
    /// Property to return the descent for the font, in pixels.
    /// </summary>
    public float Descent => (FontHeight * Font.FontFamily.GetCellDescent(Font.Style)) / Font.FontFamily.GetEmHeight(Font.Style);
    #endregion

    #region Methods.
    /// <summary>
    /// Function to build out the font data.
    /// </summary>
    /// <param name="graphics">The graphics context to use.</param>
    /// <param name="fontInfo">The information used to generate the font.</param>
    /// <param name="externalFonts">The external fonts provided by an application.</param>
    /// <returns>A new <see cref="GdiFontData"/> object.</returns>
    public static GdiFontData GetFontData(System.Drawing.Graphics graphics, IGorgonFontInfo fontInfo, PrivateFontCollection externalFonts)
    {
        var result = new GdiFontData();

        CharacterRange[] range = [
                        new(0, 1)
                    ];

        System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular;

        switch (fontInfo.FontStyle)
        {
            case GorgonFontStyle.Bold:
                style = System.Drawing.FontStyle.Bold;
                break;
            case GorgonFontStyle.Italics:
                style = System.Drawing.FontStyle.Italic;
                break;
            case GorgonFontStyle.BoldItalics:
                style = System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic;
                break;
        }

        FontFamily fontFamily = (externalFonts is not null ? externalFonts.Families.Concat(FontFamily.Families) : FontFamily.Families)
                                .FirstOrDefault(item => string.Equals(fontInfo.FontFamilyName, item.Name, StringComparison.InvariantCultureIgnoreCase));

        // If we cannot locate the font family by name, then fall back.
        fontFamily ??= FontFamily.GenericSerif;


        // Scale the font appropriately.
        if (fontInfo.FontHeightMode == GorgonFontHeightMode.Points)
        {
            // Convert the internal font to pixel size.
            result.Font = new Font(fontFamily,
                                   (fontInfo.Size * graphics.DpiY) / 72.0f,
                                   style,
                                   GraphicsUnit.Pixel);
        }
        else
        {
            result.Font = new Font(fontFamily, fontInfo.Size, style, GraphicsUnit.Pixel);
        }

        result.FontHeight = result.Font.GetHeight(graphics);

        result.StringFormat = new StringFormat(StringFormat.GenericTypographic)
        {
            FormatFlags = StringFormatFlags.NoFontFallback | StringFormatFlags.MeasureTrailingSpaces,
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Near
        };
        result.StringFormat.SetMeasurableCharacterRanges(range);

        // Create a separate drawing format because some glyphs are being clipped when they have overhang
        // on the left boundary.
        result.DrawFormat = new StringFormat(StringFormat.GenericDefault)
        {
            FormatFlags = StringFormatFlags.NoFontFallback | StringFormatFlags.MeasureTrailingSpaces,
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Near
        };
        result.DrawFormat.SetMeasurableCharacterRanges(range);

        return result;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary> 
    public void Dispose()
    {
        DrawFormat?.Dispose();
        StringFormat?.Dispose();
        Font?.Dispose();
    }
    #endregion
}
