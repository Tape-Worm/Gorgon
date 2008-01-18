#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Wednesday, November 21, 2007 12:32:55 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using GorgonLibrary;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Object representing a glyph for a font.
	/// </summary>
	public class Glyph
	{
		#region Variables.
		private Drawing.RectangleF _imageDims = Drawing.RectangleF.Empty;		// Position and dimensions of the glyph on the image.
		private Drawing.RectangleF _glyphDims = Drawing.Rectangle.Empty;		// Position and dimensions of the glyph.
		private Drawing.Size _size = Drawing.Size.Empty;						// Glyph size.
		private char _glyph;													// Character that this glyph represents.
		private System.Drawing.Font _gdiFont = null;							// GDI font for this glyph.
		private Image _glyphImage = null;										// Image that the glyph belongs to.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the image that owns the glyph.
		/// </summary>
		public Image GlyphImage
		{
			get
			{
				return _glyphImage;
			}
		}

		/// <summary>
		/// Property to return the dimensions of the glyph on the image.
		/// </summary>
		public Drawing.RectangleF ImageDimensions
		{
			get
			{
				return _imageDims;
			}
		}

		/// <summary>
		/// Property to return the glyph dimensions.
		/// </summary>
		public Drawing.RectangleF GlyphDimensions
		{
			get
			{
				return _glyphDims;
			}
		}

		/// <summary>
		/// Property to return actual glyph size.
		/// </summary>
		public Drawing.Size Size
		{
			get
			{
				return _size;
			}
		}

		/// <summary>
		/// Property to return the character represented by this glyph.
		/// </summary>
		public char GlyphCharacter
		{
			get
			{
				return _glyph;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the glyph size.
		/// </summary>
		private void GetGlyphSize()
		{
			float width = 0.0f;		// Glyph width.

			// Determine size.
			using (Drawing.Bitmap bmp = new Drawing.Bitmap(1, 1))
			{
				using (Drawing.Graphics g = Drawing.Graphics.FromImage(bmp))
				{
					_size.Height = (int)Math.Ceiling(_gdiFont.GetHeight(g));
					width = g.MeasureString(_glyph.ToString(), _gdiFont, 16384, Drawing.StringFormat.GenericTypographic).Width;
					if (width <= 0)
						width = _gdiFont.SizeInPoints * 0.5f;
					_size.Width = (int)Math.Ceiling(width);
				}
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Glyph"/> class.
		/// </summary>
		/// <param name="character">The character that this glyph will represent.</param>
		/// <param name="gdiFont">Font to be used.</param>
		/// <param name="glyphImage">Image used to store the glyph.</param>
		/// <param name="glyphDimensions">Glyph dimensions.</param>
		/// <param name="imageDimensions">The dimensions of the glyph on the image.</param>
		internal Glyph(char character, Drawing.Font gdiFont, Image glyphImage, Drawing.RectangleF glyphDimensions, Drawing.RectangleF imageDimensions)
		{
			_glyphImage = glyphImage;
			_glyph = character;
			_imageDims = imageDimensions;
			_glyphDims = glyphDimensions;
			_gdiFont = gdiFont;

			// Retrieve the glyph size.
			GetGlyphSize();
		}
		#endregion
	}
}
