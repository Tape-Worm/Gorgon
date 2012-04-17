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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimMath;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A glyph used to define a character in the font.
	/// </summary>
	public struct GorgonGlyph
		: INamedObject, IEquatable<GorgonGlyph>
	{
		#region Variables.
		private string _character;
		private GorgonTexture2D _texture;
		private RectangleF _glyphCoordinates;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the character that this glyph represents.
		/// </summary>
		public string Character
		{
			get
			{
				return _character;
			}
		}

		/// <summary>
		/// Property to return the texture that the glyph can be found on.
		/// </summary>
		public GorgonTexture2D Texture
		{
			get
			{
				return _texture;
			}
		}

		/// <summary>
		/// Property to return the coordinates, in texel space, of the glyph.
		/// </summary>
		public RectangleF GlyphCoordinates
		{
			get
			{
				return _glyphCoordinates;
			}
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
			return _character.GetHashCode();
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonGlyph)
				return GorgonGlyph.Equals(this, (GorgonGlyph)obj);
			else
				return false;
		}

		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="left">Left value to compare.</param>
		/// <param name="right">Right value to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(GorgonGlyph left, GorgonGlyph right)
		{
			return string.Compare(left._character, right._character, false) == 0;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return "Gorgon Font Glyph: " + _character;
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(GorgonGlyph left, GorgonGlyph right)
		{
			return GorgonGlyph.Equals(left, right);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(GorgonGlyph left, GorgonGlyph right)
		{
			return !GorgonGlyph.Equals(left, right);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGlyph"/> class.
		/// </summary>
		/// <param name="character">The character that the glyph represents.</param>
		/// <param name="texture">The texture that the glyph can be found on.</param>
		/// <param name="glyphCoordinates">Coordinates on the texture to indicate where the glyph is stored.</param>
		/// <remarks>The <paramref name="glyphCoordinates"/> parameter must be in texel coordinates (i.e. 0.0f .. 1.0f).</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="character"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="texture"/> parameter is NULL.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the character parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the character length is not exactly 1 character.</para>
		/// </exception>
		public GorgonGlyph(string character, GorgonTexture2D texture, RectangleF glyphCoordinates)
		{
			GorgonDebug.AssertParamString(character, "character");
			GorgonDebug.AssertNull<GorgonTexture2D>(texture, "texture");

			if (character.Length != 1)
				throw new ArgumentException("The character '" + character + "' must only have a length of 1.", "character");

			_character = character;
			_glyphCoordinates = glyphCoordinates;
			_texture = texture;
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		/// <value></value>
		string INamedObject.Name
		{
			get 
			{
				return Character;
			}
		}
		#endregion

		#region IEquatable<GorgonGlyph> Members
		/// <summary>
		/// Function to determine if this instance and another are equal.
		/// </summary>
		/// <param name="other">The other instance to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public bool Equals(GorgonGlyph other)
		{
			return GorgonGlyph.Equals(this, other);
		}
		#endregion
	}
}
