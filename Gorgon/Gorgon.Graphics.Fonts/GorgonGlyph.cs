﻿#region MIT.
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
using System.Globalization;
using DX = SharpDX;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts.Properties;
using Gorgon.Math;
using SharpDX.DXGI;

namespace Gorgon.Graphics.Fonts
{
	/// <summary>
	/// A glyph used to define a character in a <see cref="GorgonFont"/>.
	/// </summary>
	public sealed class GorgonGlyph
		: IGorgonNamedObject
	{
		#region Properties.
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		string IGorgonNamedObject.Name => Character.ToString(CultureInfo.CurrentCulture);

		/// <summary>
		/// Property to return the character that this glyph represents.
		/// </summary>
		public char Character
		{
			get;
		}

		/// <summary>
		/// Property to return the texture that the glyph can be found on.
		/// </summary>
		public GorgonTextureView TextureView
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the coordinates, in pixel space, of the glyph.
		/// </summary>
		public DX.Rectangle GlyphCoordinates
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the coordinates, in pixel space, of the glyph outline (if applicable).
		/// </summary>
		/// <remarks>
		/// This property will only be assigned if a <see cref="GorgonFont"/> has an outline.
		/// </remarks>
		public DX.Rectangle OutlineCoordinates
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the texture coordinates for the glyph.
		/// </summary>
		public DX.RectangleF TextureCoordinates
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the texture coordinates for the glyph outline (if applicable).
		/// </summary>
		/// <remarks>
		/// This property will only be assigned if a <see cref="GorgonFont"/> has an outline.
		/// </remarks>
		public DX.RectangleF OutlineTextureCoordinates
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the array index for the backing texture array.
		/// </summary>
		public int TextureIndex
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
		/// <remarks>
		/// The offset of a glyph is the distance from the glyph black box (i.e. the bounds of the glyph pixels) to the actual starting point of the glyph.  For example, if 'g' has an offset of 
		/// 4, 6 and we change the offset to 3, 4 then the 'g' glyph will be shifted left by 1 pixel and up by 2 pixels when rendering.
		/// </remarks>
		public DX.Point Offset
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the offset of the glyph outline (if applicable).
		/// </summary>
		/// <remarks>
		/// <para>
		/// The offset of a glyph is the distance from the glyph outline black box (i.e. the bounds of the glyph pixels) to the actual starting point of the glyph outline.  For example, if 'g' has an offset of 
		/// 4, 6 and we change the offset to 3, 4 then the 'g' glyph outline will be shifted left by 1 pixel and up by 2 pixels when rendering.
		/// </para>
		/// <para>
		/// This property will only be assigned if a <see cref="GorgonFont"/> has an outline.
		/// </para>
		/// </remarks>
		public DX.Point OutlineOffset
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to change the texture pointed at by this glyph.
		/// </summary>
		/// <param name="texture">The texture to assign to the glyph.</param>
		/// <param name="glyphCoordinates">The coordinates of the glyph on the texture, in pixels.</param>
		/// <param name="outlineCoordinates">The coordinates of the glyph outline on the texture, in pixels.</param>
		/// <param name="textureArrayIndex">The array index on the 2D texture array to use for this glyph.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="texture"/> is not a 2D texture.
		/// <para>-or-</para>
		/// <para>Thrown if the <paramref name="texture"/> format is not <c>R8G8B8A8_UNorm</c>, <c>R8G8B8A8_UNorm_SRgb</c>, <c>Format.B8G8R8A8_UNorm</c> or <c>B8G8R8A8_UNorm_SRgb</c>.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// This allows an application to point a glyph at a new <see cref="GorgonTexture"/> for custom bitmap glyphs, or allows the region on the texture that contains the glyph to be modified. This 
		/// allows applications to create custom characters in fonts that the font generation code cannot produce.
		/// </para>
		/// <para>
		/// This should only be used when generating a font, or modifying a font and discarding it when done. When the font is persisted to a file or other stream, the font texture will be saved with 
		/// that data. Thus, the best practice, if saving the font, is to create the font with the custom texture, then save it and only load it from a stream/disk file after. 
		/// </para>
		/// <para>
		/// If the <see cref="GorgonFont"/> for this glyph does not have an outline, then the <paramref name="outlineCoordinates"/> will be set to empty.
		/// </para>
		/// </remarks>
		public void UpdateTexture(GorgonTexture texture, DX.Rectangle glyphCoordinates, DX.Rectangle outlineCoordinates, int textureArrayIndex)
		{
			if (texture == null)
			{
				throw new ArgumentNullException(nameof(texture));
			}

			if ((texture.DefaultShaderResourceView == TextureView)
			    && (glyphCoordinates == GlyphCoordinates)
				&& (outlineCoordinates == OutlineCoordinates)
				&& (TextureIndex == textureArrayIndex))
			{
				return;
			}

			if (texture.DefaultShaderResourceView != TextureView)
			{
				if (texture.Info.TextureType != TextureType.Texture2D)
				{
					throw new ArgumentException(Resources.GORGFX_ERR_FONT_GLYPH_IMAGE_NOT_2D, nameof(texture));
				}

				if ((texture.Info.Format != Format.R8G8B8A8_UNorm)
				    && (texture.Info.Format != Format.R8G8B8A8_UNorm_SRgb)
				    && (texture.Info.Format != Format.B8G8R8A8_UNorm)
				    && (texture.Info.Format != Format.B8G8R8A8_UNorm_SRgb))
				{
					throw new ArgumentException(Resources.GORGFX_ERR_GLYPH_TEXTURE_FORMAT_INVALID, nameof(texture));
				}

				TextureView = texture.DefaultShaderResourceView;
			}

			// Ensure that this index is valid.
			textureArrayIndex = textureArrayIndex.Max(0).Min(TextureView?.Texture.Info.ArrayCount - 1 ?? 0);

			TextureIndex = textureArrayIndex;
			GlyphCoordinates = glyphCoordinates;
			OutlineCoordinates = outlineCoordinates;
			TextureCoordinates = new DX.RectangleF
			{
				Left = glyphCoordinates.Left / (float)texture.Info.Width,
				Top = glyphCoordinates.Top / (float)texture.Info.Height,
				Right = glyphCoordinates.Right / (float)texture.Info.Width,
				Bottom = glyphCoordinates.Bottom / (float)texture.Info.Height
			};

			if (OutlineCoordinates.IsEmpty)
			{
				return;
			}

			OutlineTextureCoordinates = new DX.RectangleF
			                            {
				                            Left = outlineCoordinates.Left / (float)texture.Info.Width,
				                            Top = outlineCoordinates.Top / (float)texture.Info.Height,
				                            Right = outlineCoordinates.Right / (float)texture.Info.Width,
				                            Bottom = outlineCoordinates.Bottom / (float)texture.Info.Height,
			                            };
		}

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
		/// Returns a <see cref="string"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="string"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GORGFX_TOSTR_FONT_GLYPH, Character);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGlyph"/> class.
		/// </summary>
		/// <param name="character">The character that the glyph represents.</param>
		/// <param name="advance">Advancement width for the glyph.</param>
		/// <remarks>
		/// <para>
		/// This is used by the font generation routine to populate glyph data first, and assign a texture after.
		/// </para>
		/// </remarks>
		internal GorgonGlyph(char character, int advance)
		{
			Character = character;
			Advance = advance;
		}
		#endregion
	}
}
