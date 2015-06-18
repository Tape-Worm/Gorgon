#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Saturday, October 12, 2013 9:03:05 PM
// 
#endregion

using System.Drawing;

namespace Gorgon.Graphics
{
	/// <summary>
	/// The type of glyph brush to use when painting the glyphs for the font.
	/// </summary>
	public enum GlyphBrushType
	{
		/// <summary>
		/// A solid color.
		/// </summary>
		Solid = 0,
		/// <summary>
		/// Texture.
		/// </summary>
		Texture = 1,
		/// <summary>
		/// Hatch pattern.
		/// </summary>
		Hatched = 2,
		/// <summary>
		/// Linear gradient colors.
		/// </summary>
		LinearGradient = 3,
		/// <summary>
		/// Path gradient colors.
		/// </summary>
		PathGradient = 4
	}

	/// <summary>
	/// A brush used to paint the glyphs when generating a font.
	/// </summary>
	public abstract class GorgonGlyphBrush
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of brush.
		/// </summary>
		public abstract GlyphBrushType BrushType
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a glyph brush object.
		/// </summary>
		/// <param name="type">Type of glyph brush object.</param>
		/// <param name="graphics">Graphics interface required by texture brush type.</param>
		/// <returns>The glyph brush object.</returns>
		internal static GorgonGlyphBrush CreateBrush(GlyphBrushType type, GorgonGraphics graphics)
		{
			switch (type)
			{
				case GlyphBrushType.PathGradient:
					return new GorgonGlyphPathGradientBrush();
				case GlyphBrushType.LinearGradient:
					return new GorgonGlyphLinearGradientBrush();
				case GlyphBrushType.Texture:
					return new GorgonGlyphTextureBrush(graphics);
				case GlyphBrushType.Hatched:
					return new GorgonGlyphHatchBrush();
				default:
					return new GorgonGlyphSolidBrush();
			}
		}

		/// <summary>
		/// Function to convert this brush to the equivalent GDI+ brush type.
		/// </summary>
		/// <returns>The GDI+ brush type for this object.</returns>
		abstract internal Brush ToGDIBrush();
		#endregion
	}
}
