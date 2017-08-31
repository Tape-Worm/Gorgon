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

using System;
using Drawing = System.Drawing;

namespace Gorgon.Graphics.Fonts
{
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
		public Drawing.Font Font
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the string format used when performing glyph measuring.
		/// </summary>
		public Drawing.StringFormat StringFormat
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
		public Drawing.StringFormat DrawFormat
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
		/// <returns>A new <see cref="GdiFontData"/> object.</returns>
		public static GdiFontData GetFontData(Drawing.Graphics graphics, IGorgonFontInfo fontInfo)
		{
			GdiFontData result = new GdiFontData();

			Drawing.CharacterRange[] range = {
				            new Drawing.CharacterRange(0, 1)
			            };

			Drawing.FontStyle style = Drawing.FontStyle.Regular;

			switch (fontInfo.FontStyle)
			{
				case FontStyle.Bold:
					style = Drawing.FontStyle.Bold;
					break;
				case FontStyle.Italics:
					style = Drawing.FontStyle.Italic;
					break;
				case FontStyle.BoldItalics:
					style = Drawing.FontStyle.Bold | Drawing.FontStyle.Italic;
					break;
			}

			// Scale the font appropriately.
			if (fontInfo.FontHeightMode == FontHeightMode.Points)
			{
				// Convert the internal font to pixel size.
				result.Font = new Drawing.Font(fontInfo.FontFamilyName,
				                               (fontInfo.Size * graphics.DpiY) / 72.0f,
				                               style,
				                               Drawing.GraphicsUnit.Pixel);
			}
			else
			{
				result.Font = new Drawing.Font(fontInfo.FontFamilyName, fontInfo.Size, style, Drawing.GraphicsUnit.Pixel);
			}

			result.FontHeight = result.Font.GetHeight(graphics);

			result.StringFormat = new Drawing.StringFormat(Drawing.StringFormat.GenericTypographic)
			                      {
				                      FormatFlags = Drawing.StringFormatFlags.NoFontFallback | Drawing.StringFormatFlags.MeasureTrailingSpaces,
				                      Alignment = Drawing.StringAlignment.Near,
				                      LineAlignment = Drawing.StringAlignment.Near
			                      };
			result.StringFormat.SetMeasurableCharacterRanges(range);

			// Create a separate drawing format because some glyphs are being clipped when they have overhang
			// on the left boundary.
			result.DrawFormat = new Drawing.StringFormat(Drawing.StringFormat.GenericDefault)
			                    {
				                    FormatFlags = Drawing.StringFormatFlags.NoFontFallback | Drawing.StringFormatFlags.MeasureTrailingSpaces,
				                    Alignment = Drawing.StringAlignment.Near,
				                    LineAlignment = Drawing.StringAlignment.Near
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
}