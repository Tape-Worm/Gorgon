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
// Created: Friday, April 13, 2012 6:51:08 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Collections;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Provides functionality for creating, reading, and saving bitmap fonts.
	/// </summary>
	public sealed class GorgonFont
		: GorgonNamedObject, IDisposable
	{
		#region Classes.
		/// <summary>
		/// A collection of glyphs for the font.
		/// </summary>
		public class GlyphCollection
			: GorgonBaseNamedObjectCollection<GorgonGlyph>
		{
			#region Variables.
			private GorgonFont _font = null;			// Font that owns this collection.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to set or return a glyph in the collection by its character representation.
			/// </summary>
			public GorgonGlyph this[string character]
			{
				get
				{
					return GetItem(character);
				}
				set
				{
					if (string.IsNullOrEmpty(value.Character))
						throw new ArgumentException("There is no character associated with the glyph.");

					if (value.Texture == null)
						throw new ArgumentException("The glyph does not have a texture assigned to it.", "glyph");

					if (Contains(character))
						SetItem(character, value);
					else
						AddItem(value);
				}
			}

			/// <summary>
			/// Property to return a glyph in the list by index.
			/// </summary>
			public GorgonGlyph this[int index]
			{
				get
				{
					return GetItem(index);
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to return the index of a glyph character in the collection.
			/// </summary>
			/// <param name="character">Character to return the index of.</param>
			/// <returns>The index of the glyph character if found, -1 if not.</returns>
			public int IndexOf(string character)
			{
				if (Contains(character))
					return IndexOf(this[character]);

				return -1;
			}

			/// <summary>
			/// Function to add a new glyph to the collection.
			/// </summary>
			/// <param name="glyph">Glyph to add.</param>
			/// <exception cref="System.ArgumentException">Thrown when the <paramref name="glyph"/> parameter already exists in this collection.</exception>
			public void Add(GorgonGlyph glyph)
			{
				if (string.IsNullOrEmpty(glyph.Character))
					throw new ArgumentException("There is no character associated with the glyph.");

				if (Contains(glyph))
					throw new ArgumentException("The glyph for character '" + glyph.Character + "' already exists in this collection.", "glyph");

				if (glyph.Texture == null)
					throw new ArgumentException("The glyph does not have a texture assigned to it.", "glyph");

				// Add the texture automatically if it does not exist in the collection.
				if (!_font.Textures.Contains(glyph.Texture.Name))
					_font.Textures.Add(glyph.Texture);
				
				AddItem(glyph);
			}

			/// <summary>
			/// Function to remove a glyph from the list.
			/// </summary>
			/// <param name="glyph">The texture to remove.</param>
			/// <exception cref="System.Collections.Generic.KeyNotFoundException">The glyph does not exist in the collection.</exception>
			public void Remove(GorgonGlyph glyph)
			{
				if (!Contains(glyph))
					throw new KeyNotFoundException("The glyph for character '" + glyph.Character + "' does not exist in this collection.");

				RemoveItem(glyph);
			}

			/// <summary>
			/// Function to remove a glyph from the list.
			/// </summary>
			/// <param name="character">Character represented by the glyph to remove.</param>
			/// <exception cref="System.ArgumentNullException">Thrown when then <paramref name="character"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
			/// <exception cref="System.Collections.Generic.KeyNotFoundException">The character glyph does not exist in the collection.</exception>
			public void Remove(string character)
			{
				GorgonDebug.AssertParamString(character, "name");

				if (!Contains(character))
					throw new System.Collections.Generic.KeyNotFoundException("The glyph for character '" + character + "' does not exist in this collection.");

				RemoveItem(character);
			}

			/// <summary>
			/// Function to clear all character glyphs from this collection.
			/// </summary>
			public void Clear()
			{
				ClearItems();
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="GlyphCollection"/> class.
			/// </summary>
			/// <param name="font">Fonr that owns this glyph collection.</param>
			internal GlyphCollection(GorgonFont font)
				: base(true)
			{
				_font = font;
			}
			#endregion
		}

		/// <summary>
		/// A collection of textures used for the font.
		/// </summary>
		public class FontTextureCollection
			: GorgonBaseNamedObjectDictionary<GorgonTexture2D>
		{
			#region Properties.
			/// <summary>
			/// Property to return a texture from the collection by name.
			/// </summary>
			public GorgonTexture2D this[string name]
			{
				get
				{
					return GetItem(name);
				}
				set
				{
					if (value == null)
					{
						if (Contains(name))
							RemoveItem(GetItem(name));
						else
							throw new KeyNotFoundException("The texture '" + name + "' was not found in this collection.");

						return;
					}

					// Remove the previous reference, and update it and change its name to the current reference.
					if (Contains(name))
						SetItem(name, value);
					else
						AddItem(value);
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to add a texture to the list.
			/// </summary>
			/// <param name="texture">Texture to add.</param>
			/// <exception cref="System.ArgumentNullException">Thrown when then <paramref name="texture"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.ArgumentException">Thrown when the texture parameter already exists in this collection.</exception>
			public void Add(GorgonTexture2D texture)
			{
				GorgonDebug.AssertNull<GorgonTexture2D>(texture, "texture");

				if ((Contains(texture.Name)) || (Contains(texture)))
					throw new ArgumentException("The texture '" + texture.Name + "' already exists in this collection.", "texture");

				AddItem(texture);
			}

			/// <summary>
			/// Function to add a list of items to the collection.
			/// </summary>
			/// <param name="items">Items to add.</param>
			/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="items"/> parameter is NULL (Nothing in VB.Net).</exception>
			public void AddRange(IEnumerable<GorgonTexture2D> items)
			{
				GorgonDebug.AssertNull<IEnumerable<GorgonTexture2D>>(items, "items");

				foreach (var item in items)
					Add(item);
			}

			/// <summary>
			/// Function to remove a texture from the list.
			/// </summary>
			/// <param name="texture">The texture to remove.</param>
			/// <exception cref="System.ArgumentNullException">Thrown when then <paramref name="texture"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.Collections.Generic.KeyNotFoundException">The texture does not exist in the collection.</exception>
			public void Remove(GorgonTexture2D texture)
			{
				GorgonDebug.AssertNull<GorgonTexture2D>(texture, "texture");

				if (!Contains(texture))					
					throw new System.Collections.Generic.KeyNotFoundException("The texture '" + texture.Name + "' does not exist in this collection.");

				RemoveItem(texture);
			}

			/// <summary>
			/// Function to remove a texture from the list.
			/// </summary>
			/// <param name="name">Name of the texture to remove.</param>
			/// <exception cref="System.ArgumentNullException">Thrown when then <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
			/// <exception cref="System.Collections.Generic.KeyNotFoundException">The texture does not exist in the collection.</exception>
			public void Remove(string name)
			{
				GorgonDebug.AssertParamString(name, "name");

				if (!Contains(name))
					throw new System.Collections.Generic.KeyNotFoundException("The texture '" + name + "' does not exist in this collection.");

				RemoveItem(name);
			}

			/// <summary>
			/// Function to remove all textures from the collection.
			/// </summary>
			public void Clear()
			{
				ClearItems();
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="FontTextureCollection"/> class.
			/// </summary>
			internal FontTextureCollection()
				: base(false)
			{
			}
			#endregion
		}
		#endregion

		#region Variables.
		private bool _disposed = false;							// Flag to indicate that the object was disposed.
		private IList<GorgonTexture2D> _textures = null;		// List of textures for the font.
		private Size _textureSize = new Size(512, 512);			// Texture size for each new glyph page.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the graphics interface that created this object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the textures for this font.
		/// </summary>
		public FontTextureCollection Textures
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the glyphs for this font.
		/// </summary>
		/// <remarks>A glyph is a graphical representation of a character.  For Gorgon, this means a glyph for a specific character will point to a region of texels on a texture.
		/// <para>Note that the glyph for a character is not required to represent the exact character (for example, the character "A" could map to the "V" character on the texture).  This 
		/// will allow mapping of symbols to a character representation.</para>
		/// </remarks>
		public GlyphCollection Glyphs
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the default texture size for texture glyph pages.
		/// </summary>
		/// <remarks>Use this decrease or increase the number of textures used for a font.  If the number of glyphs cannot fit onto a single texture, a new texture will be created to 
		/// store the remaining glyphs.  This value will control the width and height of the textures created.
		/// <para>To retrieve the count of textures used, call the <see cref="P:GorgonLibrary.Graphics.GorgonFont.FontTextureCollection.Count">Count</see> property on the <see cref="P:GorgonLibrary.Graphics.GorgonFont.Textures">Textures</see> property.</para>
		/// <para>The default size is 512x512 and the minimum size is 256x256.</para>
		/// </remarks>
		public Size DefaultGlyphTextureSize
		{
			get
			{
				return _textureSize;
			}
			set
			{
				if (_textureSize.Width < 256)
					_textureSize.Width = 256;
				if (_textureSize.Height < 256)
					_textureSize.Height = 256;

				if (_textureSize.Width >= Graphics.Textures.MaxWidth)
					_textureSize.Width = Graphics.Textures.MaxWidth - 1;

				if (_textureSize.Width >= Graphics.Textures.MaxHeight)
					_textureSize.Width = Graphics.Textures.MaxHeight - 1;

				_textureSize = value;
			}
		}
		#endregion

		#region Methods.

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFont"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that created this object.</param>
		/// <param name="name">The name of the font.</param>
		internal GorgonFont(GorgonGraphics graphics, string name)
			: base(name)
		{
			Graphics = graphics;
			Textures = new FontTextureCollection();
			Glyphs = new GlyphCollection(this);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
				}

				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
