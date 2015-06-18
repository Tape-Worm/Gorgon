#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Friday, April 13, 2012 7:12:15 AM
// 
#endregion

using System;
using System.Drawing;
using System.Globalization;
using Gorgon.Core;
using Gorgon.Graphics.Properties;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A glyph used to define a character in the font.
	/// </summary>
	public sealed class GorgonGlyph
		: IGorgonNamedObject
	{
		#region Properties.
		/// <summary>
		/// Property to return the character that this glyph represents.
		/// </summary>
		public char Character
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the texture that the glyph can be found on.
		/// </summary>
		public GorgonTexture2D Texture
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the coordinates, in pixel space, of the glyph.
		/// </summary>
		public Rectangle GlyphCoordinates
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the texture coordinates for the glyph.
		/// </summary>
		public RectangleF TextureCoordinates
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the horizontal advance for the glyph.
		/// </summary>
		/// <remarks>
        /// This value defines the exact width of the glyph and is a total of the left bearing (A), black box width (B), and right bearing (C) for a glyph.
		/// </remarks>
		public int Advance
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the offset of the glyph.
		/// </summary>
		/// <remarks>The offset of a glyph is the distance from the glyph black box (i.e. the bounds of the glyph pixels) to the actual starting point of the glyph.  For example, if 'g' has an offset of 
		/// 4, 6 and we change the offset to 3, 4 then the 'g' glyph will be shifted left by 1 pixel and up by 2 pixels when rendering.</remarks>
		public Point Offset
		{
			get;
			set;
		}

        /// <summary>
        /// Property to return whether this texture is external to the font or not.
        /// </summary>
        [Obsolete("TODO: Get rid of this, Gorgon fonts will now copy the texture to take ownership of it. This flag is no longer necessary")]
	    public bool IsExternalTexture
	    {
	        get;
	        internal set;
	    }
		#endregion

		#region Methods.
		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return Character.GetHashCode();
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GORGFX_FONT_GLYPH_TOSTR, Character);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGlyph"/> class.
		/// </summary>
		/// <param name="character">The character that the glyph represents.</param>
		/// <param name="texture">The texture that the glyph can be found on.</param>
		/// <param name="glyphCoordinates">Coordinates on the texture to indicate where the glyph is stored.</param>
		/// <param name="offset">The <see cref="Offset"/> of the glyph.</param>
		/// <param name="advance">Advancement width for the glyph.</param>
		/// <remarks>The <paramref name="glyphCoordinates"/> parameter is in pixel coordinates (i.e. 0 .. Width/Height).</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is NULL (<i>Nothing</i> in VB.Net).
		/// </exception>
		public GorgonGlyph(char character, GorgonTexture2D texture, Rectangle glyphCoordinates, Point offset, int advance)
		{
			if (texture == null)
			{
				throw new ArgumentNullException("texture");
			}

			Character = character;
			GlyphCoordinates = glyphCoordinates;
			TextureCoordinates = RectangleF.FromLTRB(glyphCoordinates.Left / (float)texture.Settings.Width,
												glyphCoordinates.Top / (float)texture.Settings.Height,
												glyphCoordinates.Right / (float)texture.Settings.Width,
												glyphCoordinates.Bottom / (float)texture.Settings.Height);
			Texture = texture;
			Offset = offset;
			Advance = advance;
		}
		#endregion

		#region IGorgonNamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		/// <value></value>
		string IGorgonNamedObject.Name
		{
			get 
			{
				return Character.ToString(CultureInfo.CurrentCulture);
			}
		}
		#endregion
	}
}
