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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using GorgonLibrary.Collections;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.IO;
using GorgonLibrary.Native;
using SlimMath;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Provides functionality for creating, reading, and saving bitmap fonts.
	/// </summary>
	public sealed class GorgonFont
		: GorgonNamedObject, IDisposable, INotifier
	{
		#region Constants.
		/// <summary>
		/// Header for a Gorgon font file.
		/// </summary>
		public const string FileHeader = "GORFNT10";
		#endregion

		#region Classes.
		/// <summary>
		/// A collection of glyphs for the font.
		/// </summary>
		public class GlyphCollection
			: IDictionary<char, GorgonGlyph>, IEnumerable<GorgonGlyph>
		{
			#region Variables.
			private GorgonFont _font = null;				// Font that owns this collection.
			private SortedList<char, GorgonGlyph> _list;	// List.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to set or return a glyph in the collection by its character representation.
			/// </summary>
			public GorgonGlyph this[char character]
			{
				get
				{
					return _list[character];
				}
				set
				{
					_list[character] = value;
				}
			}

			/// <summary>
			/// Property to return a glyph in the list by index.
			/// </summary>
			public GorgonGlyph this[int index]
			{
				get
				{
					return _list.Values[index];
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to return whether the character exists in this collection.
			/// </summary>
			/// <param name="character">The character to find.</param>
			/// <returns>TRUE if found, FALSE if not.</returns>
			public bool Contains(char character)
			{
				return _list.ContainsKey(character);
			}

			/// <summary>
			/// Function to return whether a glyph exists in this collection or not.
			/// </summary>
			/// <param name="glyph">Glyph to find.</param>
			/// <returns>TRUE if the glyph was found, FALSE if not.</returns>
			/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="glyph"/> parameter is NULL (Nothing in VB.Net).</exception>
			public bool Contains(GorgonGlyph glyph)
			{
				GorgonDebug.AssertNull<GorgonGlyph>(glyph, "glyph");
				return _list.ContainsValue(glyph);
			}

			/// <summary>
			/// Function to return the index of a character in the collection.
			/// </summary>
			/// <param name="character">Character to find.</param>
			/// <returns>The index of the character if found, -1 if not.</returns>
			public int Indexof(char character)
			{
				return _list.IndexOfKey(character);
			}

			/// <summary>
			/// Function to return the index of a glyph in the collection.
			/// </summary>
			/// <param name="glyph">Glyph to find.</param>
			/// <returns>The index of the glyph if found, -1 if not.</returns>
			/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="glyph"/> parameter is NULL (Nothing in VB.Net).</exception>
			public int IndexOf(GorgonGlyph glyph)
			{
				GorgonDebug.AssertNull<GorgonGlyph>(glyph, "glyph");
				return _list.IndexOfValue(glyph);
			}
			
			/// <summary>
			/// Function to add a new glyph to the collection.
			/// </summary>
			/// <param name="glyph">Glyph to add.</param>
			/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="glyph"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.ArgumentException">Thrown when the <paramref name="glyph"/> parameter already exists in this collection.</exception>
			public void Add(GorgonGlyph glyph)
			{
				GorgonDebug.AssertNull<GorgonGlyph>(glyph, "glyph");

				if (Contains(glyph.Character))
					throw new ArgumentException("The glyph for character '" + glyph.Character + "' already exists in this collection.", "glyph");

				if (glyph.Texture == null)
					throw new ArgumentException("The glyph does not have a texture assigned to it.", "glyph");

				_list.Add(glyph.Character, glyph);
			}

			/// <summary>
			/// Function to remove a glyph from the list.
			/// </summary>
			/// <param name="glyph">The texture to remove.</param>
			/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="glyph"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.Collections.Generic.KeyNotFoundException">The glyph does not exist in the collection.</exception>
			public void Remove(GorgonGlyph glyph)
			{
				GorgonDebug.AssertNull<GorgonGlyph>(glyph, "glyph");

				if (!Contains(glyph.Character))
					throw new KeyNotFoundException("The glyph for character '" + glyph.Character + "' does not exist in this collection.");

				_list.Remove(glyph.Character);
			}

			/// <summary>
			/// Function to remove a glyph from the list.
			/// </summary>
			/// <param name="character">Character represented by the glyph to remove.</param>
			/// <exception cref="System.ArgumentNullException">Thrown when then <paramref name="character"/> parameter is NULL (Nothing in VB.Net).</exception>
			/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
			/// <exception cref="System.Collections.Generic.KeyNotFoundException">The character glyph does not exist in the collection.</exception>
			public void Remove(char character)
			{
				_list.Remove(character);
			}

			/// <summary>
			/// Function to remove a glyph by index.
			/// </summary>
			/// <param name="index">The index of the glyph to remove.</param>
			/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is less than 0 or greater than the number of items in the collection.</exception>
			public void Remove(int index)
			{
				GorgonDebug.AssertParamRange(index, 0, _list.Count, "index");

				_list.RemoveAt(index);
			}

			/// <summary>
			/// Function to clear all character glyphs from this collection.
			/// </summary>
			public void Clear()
			{
				_list.Clear();
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="GlyphCollection"/> class.
			/// </summary>
			/// <param name="font">Fonr that owns this glyph collection.</param>
			internal GlyphCollection(GorgonFont font)
			{
				_font = font;
				_list = new SortedList<char, GorgonGlyph>(255);
			}
			#endregion

			#region IEnumerable<GorgonGlyph> Members
			/// <summary>
			/// Returns an enumerator that iterates through the collection.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
			/// </returns>
			public IEnumerator<GorgonGlyph> GetEnumerator()
			{
				foreach (KeyValuePair<char, GorgonGlyph> item in _list)
					yield return item.Value;
			}
			#endregion

			#region IEnumerable Members
			/// <summary>
			/// Returns an enumerator that iterates through a collection.
			/// </summary>
			/// <returns>
			/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
			/// </returns>
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
			#endregion

			#region IDictionary<char,GorgonGlyph> Members
			#region Properties.
			/// <summary>
			/// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
			/// </summary>
			/// <returns>
			/// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
			///   </returns>
			ICollection<char> IDictionary<char, GorgonGlyph>.Keys
			{
				get
				{
					return _list.Keys;
				}
			}

			/// <summary>
			/// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
			/// </summary>
			/// <returns>
			/// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
			///   </returns>
			ICollection<GorgonGlyph> IDictionary<char, GorgonGlyph>.Values
			{
				get
				{
					return _list.Values;
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
			/// </summary>
			/// <param name="key">The object to use as the key of the element to add.</param>
			/// <param name="value">The object to use as the value of the element to add.</param>
			/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
			///   </exception>
			///   
			/// <exception cref="T:System.ArgumentException">
			/// An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
			///   </exception>
			///   
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
			///   </exception>
			void IDictionary<char, GorgonGlyph>.Add(char key, GorgonGlyph value)
			{
				Add(value);
			}

			/// <summary>
			/// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
			/// </summary>
			/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</param>
			/// <returns>
			/// true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
			/// </returns>
			/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
			///   </exception>
			bool IDictionary<char, GorgonGlyph>.ContainsKey(char key)
			{
				return Contains(key);
			}

			/// <summary>
			/// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
			/// </summary>
			/// <param name="key">The key of the element to remove.</param>
			/// <returns>
			/// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
			/// </returns>
			/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
			///   </exception>
			///   
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
			///   </exception>
			bool IDictionary<char, GorgonGlyph>.Remove(char key)
			{
				return _list.Remove(key);
			}

			/// <summary>
			/// Gets the value associated with the specified key.
			/// </summary>
			/// <param name="key">The key whose value to get.</param>
			/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param>
			/// <returns>
			/// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
			/// </returns>
			/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
			///   </exception>
			public bool TryGetValue(char key, out GorgonGlyph value)
			{
				return _list.TryGetValue(key, out value);
			}
			#endregion
			#endregion

			#region ICollection<KeyValuePair<char,GorgonGlyph>> Members
			#region Properties.
			/// <summary>
			/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </summary>
			/// <returns>
			/// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			///   </returns>
			public int Count
			{
				get
				{
					return _list.Count;
				}
			}

			/// <summary>
			/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
			/// </summary>
			/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
			///   </returns>
			public bool IsReadOnly
			{
				get
				{
					return false;
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </summary>
			/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
			///   </exception>
			void ICollection<KeyValuePair<char, GorgonGlyph>>.Add(KeyValuePair<char, GorgonGlyph> item)
			{
				Add(item.Value);
			}

			/// <summary>
			/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
			/// </summary>
			/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
			/// <returns>
			/// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
			/// </returns>
			bool ICollection<KeyValuePair<char, GorgonGlyph>>.Contains(KeyValuePair<char, GorgonGlyph> item)
			{
				return Contains(item.Key);
			}

			/// <summary>
			/// Copies to.
			/// </summary>
			/// <param name="array">The array.</param>
			/// <param name="arrayIndex">Index of the array.</param>
			void ICollection<KeyValuePair<char, GorgonGlyph>>.CopyTo(KeyValuePair<char, GorgonGlyph>[] array, int arrayIndex)
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </summary>
			/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
			/// <returns>
			/// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </returns>
			/// <exception cref="T:System.NotSupportedException">
			/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
			///   </exception>
			bool ICollection<KeyValuePair<char, GorgonGlyph>>.Remove(KeyValuePair<char, GorgonGlyph> item)
			{
				return _list.Remove(item.Key);
			}
			#endregion
			#endregion

			#region IEnumerable<KeyValuePair<char,GorgonGlyph>> Members
			/// <summary>
			/// Returns an enumerator that iterates through the collection.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
			/// </returns>
			IEnumerator<KeyValuePair<char, GorgonGlyph>> IEnumerable<KeyValuePair<char, GorgonGlyph>>.GetEnumerator()
			{
				foreach (var item in _list)
					yield return item;
			}
			#endregion
		}

		/// <summary>
		/// A collection of textures used for the font.
		/// </summary>
		public class FontTextureCollection
			: GorgonBaseNamedObjectCollection<GorgonTexture2D>
		{
			#region Variables.
			private GorgonFont _font = null;			// Font that owns this collection.
			#endregion

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

			/// <summary>
			/// Property to return a texture from the collection by its index.
			/// </summary>
			public GorgonTexture2D this[int index]
			{
				get
				{
					return GetItem(index);
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Removes the item.
			/// </summary>
			/// <param name="item">The item.</param>
			protected override void RemoveItem(GorgonTexture2D item)
			{
				base.RemoveItem(item);

				// Remove internal textures.
				if (_font._textures.Contains(item))
				{
					_font._textures.Remove(item);
					item.Dispose();
				}
			}

			/// <summary>
			/// Removes the item.
			/// </summary>
			/// <param name="name">The name.</param>
			protected override void RemoveItem(string name)
			{
				// Remove internal textures.
				if (_font._textures.Contains(this[name]))
				{
					_font._textures.Remove(this[name]);
					this[name].Dispose();
				}
				
				base.RemoveItem(name);
			}

			/// <summary>
			/// Function add a texture to the collection and bind it to the internal font texture list.
			/// </summary>
			/// <param name="texture">Texture to add.</param>
			internal void AddBind(GorgonTexture2D texture)
			{
				Add(texture);
				_font._textures.Add(texture);
			}

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
			/// <param name="index">Index of the texture to remove.</param>
			/// <exception cref="System.ArgumentOutOfRangeException">Thrown when then <paramref name="index"/> parameter is less than 0 or larger than than Count-1 of textures.</exception>
			public void Remove(int index)
			{
				GorgonDebug.AssertParamRange(index, 0, Count, "index");
				
				RemoveItem(index);							
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
				foreach (var item in this)
				{
					if (_font._textures.Contains(item))
					{
						_font._textures.Remove(item);
						item.Dispose();
					}
				}

				ClearItems();
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="FontTextureCollection"/> class.
			/// </summary>
			/// <param name="font">Font that owns this collection.</param>
			internal FontTextureCollection(GorgonFont font)
				: base(false)
			{
				_font = font;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private bool _disposed = false;									// Flag to indicate that the object was disposed.
		private IList<GorgonTexture2D> _textures = null;				// List of internal textures for the font.
		private GorgonTexture2DSettings _textureSettings = null;		// Settings for the texture.
		private Bitmap _charBitmap = null;								// Bitmap used for character cropping.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a list of kerning pairs.
		/// </summary>
		public IDictionary<GorgonKerningPair, int> KerningPairs
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Property to return the font settings.
		/// </summary>
		public GorgonFontSettings Settings
		{
			get;
			private set;
		}

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
		/// Property to return the ascent for the font, in pixels.
		/// </summary>
		public float Ascent
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the descent for the font, in pixels.
		/// </summary>
		public float Descent
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the line height, in pixels, for the font.
		/// </summary>
		public float LineHeight
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the font height, in pixels.
		/// </summary>
		/// <remarks>This is not the same as line height, the line height is a combination of ascent, descent and internal/external leading space.</remarks>
		public float FontHeight
		{
			get;
			internal set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to save the font to a stream.
		/// </summary>
		/// <param name="stream">Stream to write into.</param>
		/// <param name="externalTextures">TRUE to save the textures as external files, FALSE to bundle them with the font.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL.</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream parameter does not allow for writing.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the externalTextures parameter is TRUE and the stream is not a file stream.</exception>
		/// <remarks>The <paramref name="externalTextures"/> parameter will only work on file streams, if the stream is not a file stream, then an exception will be thrown.</remarks>
		private void Save(Stream stream, bool externalTextures)
		{
			int textureCounter = 0;

			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (!stream.CanWrite)
				throw new IOException("The stream is not writable.");

			if ((externalTextures) && (!(stream is FileStream)))
				throw new ArgumentException("The stream is not a file stream, external textures cannot be saved.", "externalTextures");

			// Output the font in chunked format.
            using (GorgonChunkWriter chunk = new GorgonChunkWriter(stream))
            {
                string characterList = string.Join(string.Empty, Settings.Characters);

                chunk.Begin(FileHeader);

                // Write font information.
                chunk.Begin("FONTDATA");
                chunk.WriteString(Settings.FontFamilyName);
                chunk.WriteFloat(Settings.Size);
                chunk.Write<FontHeightMode>(Settings.FontHeightMode);
                chunk.Write<FontStyle>(Settings.FontStyle);
                chunk.WriteChar(Settings.DefaultCharacter);
                chunk.WriteString(characterList);
                chunk.WriteFloat(FontHeight);
                chunk.WriteFloat(LineHeight);
                chunk.WriteFloat(Ascent);
                chunk.WriteFloat(Descent);
                chunk.End();

                // Write rendering information.
                chunk.Begin("RNDRDATA");
                chunk.Write<FontAntiAliasMode>(Settings.AntiAliasingMode);
                chunk.WriteInt32(Settings.BaseColors.Count);
                for (int i = 0; i < Settings.BaseColors.Count; i++)
                {
                    chunk.Write<GorgonColor>(Settings.BaseColors[i]);
                }
                chunk.Write<GorgonColor>(Settings.OutlineColor);
                chunk.WriteInt32(Settings.OutlineSize);
                chunk.WriteInt32(Settings.TextContrast);
                chunk.End();

                // Write texture information.
                chunk.Begin("TXTRDATA");
                chunk.WriteInt32(Settings.PackingSpacing);
                chunk.WriteSize(Settings.TextureSize);
                chunk.WriteInt32(Textures.Count);
                chunk.End();

                // Write out actual textures.
                if (!externalTextures)
                {
                    chunk.Begin("TXTRINTL");
                }
                else
                {
                    chunk.Begin("TXTREXTL");
                }
                textureCounter = 0;
                foreach (var texture in Textures)
                {
                    if (!externalTextures)
                    {
                        byte[] textureData = texture.Save(ImageFileFormat.PNG);
                        chunk.WriteString(texture.Name);
						chunk.WriteInt32(textureData.Length);
                        chunk.Write(textureData, 0, textureData.Length);
                    }
                    else
                    {
                        FileStream fileStream = (FileStream)stream;
                        string path = Path.GetDirectoryName(fileStream.Name);
                        string textureFileName = Path.GetFileNameWithoutExtension(fileStream.Name) + "Texture_" + textureCounter.ToString("0000") + ".png";

                        textureFileName = textureFileName.FormatFileName().Replace(' ', '_');

                        // Write out the file in the same directory as the font info.
                        texture.Save(path.FormatDirectory(Path.DirectorySeparatorChar) + textureFileName, ImageFileFormat.PNG);

                        chunk.WriteString(texture.Name);
                        chunk.WriteString(textureFileName);
                    }
                    textureCounter++;
                }
                chunk.End();

                // Write out glyph data.
                var textureGlyphs = from GorgonGlyph glyph in Glyphs
                                    group glyph by glyph.Texture;

                chunk.Begin("GLYFDATA");
                chunk.WriteInt32(textureGlyphs.Count());
                foreach (var group in textureGlyphs)
                {
                    chunk.WriteString(group.Key.Name);
                    chunk.WriteInt32(group.Count());
                    foreach (var glyph in group)
                    {
                        chunk.WriteChar(glyph.Character);
                        chunk.WriteRectangle(glyph.GlyphCoordinates);
                        chunk.Write<Vector2>(glyph.Offset);
                        chunk.Write<Vector3>(glyph.Advance);
                    }
                }
                chunk.End();

                // Write out optional kerning information.
                if (KerningPairs.Count > 0)
                {
                    chunk.Begin("KERNDATA");
                    chunk.WriteInt32(this.KerningPairs.Count);
                    foreach (var kernInfo in KerningPairs)
                    {
                        chunk.WriteChar(kernInfo.Key.LeftCharacter);
                        chunk.WriteChar(kernInfo.Key.RightCharacter);
                        chunk.WriteInt32(kernInfo.Value);
                    }
                    chunk.End();
                }
            }
		}

		/// <summary>
		/// Function to draw the glyph character onto the bitmap.
		/// </summary>
		/// <param name="graphics">Graphics interface.</param>
		/// <param name="bitmap">Bitmap to write onto.</param>
		/// <param name="font">Font to use.</param>
		/// <param name="format">Formatter for the string.</param>
		/// <param name="character">Character to write.</param>
		/// <param name="position">Position on the bitmap.</param>
		private void DrawGlyphCharacter(System.Drawing.Graphics graphics, Bitmap bitmap, Font font, StringFormat format, char character, Rectangle position)
		{
			Brush brush = null;
			Brush outlineBrush = null;
			SmoothingMode smoothMode = graphics.SmoothingMode;
			RectangleF result = position;

			try
			{
				graphics.CompositingQuality = CompositingQuality.HighQuality;

				if (Settings.BaseColors.Count < 2)
					brush = new SolidBrush(Settings.BaseColors[0]);
				else
				{
					LinearGradientBrush gradBrush = null;

					gradBrush = new LinearGradientBrush(position, Settings.BaseColors[0], Settings.BaseColors[Settings.BaseColors.Count - 1], LinearGradientMode.Vertical);

					ColorBlend blends = new ColorBlend(Settings.BaseColors.Count);
					blends.Positions = new float[Settings.BaseColors.Count];
					blends.Colors = (from colors in Settings.BaseColors
									select colors.ToColor()).ToArray();					

					for (int i = 0; i < Settings.BaseColors.Count; i++)
					{
						blends.Positions[i] = (float)i / (Settings.BaseColors.Count - 1);
					}
					gradBrush.InterpolationColors = blends;
					brush = gradBrush;
				}

				if ((Settings.OutlineSize > 0) && (Settings.OutlineColor.Alpha > 0.0f))
				{
					outlineBrush = new SolidBrush(Settings.OutlineColor);

					// This may not be 100% accurate, but it works and works well.
					for (int x = 0; x <= Settings.OutlineSize * 2; x++)
					{
						for (int y = 0; y <= Settings.OutlineSize * 2; y++)
						{
							Rectangle offsetRect = position;
							offsetRect.Offset(x, y);
							graphics.DrawString(character.ToString(), font, outlineBrush, offsetRect, format);
						}
					}

					position.Offset(Settings.OutlineSize, Settings.OutlineSize);
				}

				graphics.DrawString(character.ToString(), font, brush, position, format);
				graphics.Flush();
			}
			finally
			{
				graphics.SmoothingMode = smoothMode;
				if (outlineBrush != null)
					outlineBrush.Dispose();
				if (brush != null)
					brush.Dispose();

				outlineBrush = null;
				brush = null;
			}
		}

		/// <summary>
		/// Function to determine if a bitmap is empty.
		/// </summary>
		/// <param name="pixels">Pixels to evaluate.</param>
		/// <param name="x">Horizontal position.</param>
		/// <returns>TRUE if empty, FALSE if not.</returns>
		private unsafe bool IsBitmapColumnEmpty(BitmapData pixels, int x)
		{
			int* pixel = (int *)pixels.Scan0.ToPointer() + x;
			int* offset = null;
			
			for (int y = 0; y < pixels.Height; y++)
			{
				if (((*pixel >> 24) & 0xff) != 0)
					return false;

				pixel += pixels.Width;
			}

			return true;
		}

		/// <summary>
		/// Function to determine if a bitmap is empty.
		/// </summary>
		/// <param name="pixels">Pixels to evaluate.</param>
		/// <param name="y">Vertical position.</param>
		/// <returns>TRUE if empty, FALSE if not.</returns>
		private unsafe bool IsBitmapRowEmpty(BitmapData pixels, int y)
		{
			int* pixel = (int*)pixels.Scan0.ToPointer();

			pixel += y * pixels.Width;
			for (int x = 0; x < pixels.Width; x++)
			{
				if (((*pixel >> 24) & 0xff) != 0)
					return false;
				pixel++;
			}

			return true;
		}

		/// <summary>
		/// Function to return the character bounding rectangles.
		/// </summary>
		/// <param name="g">Graphics interface to use.</param>
		/// <param name="font">Font to apply.</param>
		/// <param name="format">Format for the font.</param>
		/// <param name="drawFormat">The string format used to draw the glyph.</param>
		/// <param name="c">Character to evaluate.</param>
		/// <returns>A rectangle for the bounding box and offset of the character.</returns>
		private Tuple<Rectangle, Vector2, char> GetCharRect(System.Drawing.Graphics g, Font font, StringFormat format, StringFormat drawFormat, char c)
		{
			Region[] size = null;
			Region[] defaultSize = null;
			RectangleF result = RectangleF.Empty;
			char currentCharacter = c;
			BitmapData pixels = null;

			try
			{
				// Try to get the character size.
				size = g.MeasureCharacterRanges(currentCharacter.ToString(), font, new RectangleF(0, 0, Settings.TextureSize.Width, Settings.TextureSize.Height), format);
				defaultSize = g.MeasureCharacterRanges(Settings.DefaultCharacter.ToString(), font, new RectangleF(0, 0, Settings.TextureSize.Width, Settings.TextureSize.Height), format);

				// If the character doesn't exist, then return an empty value.
				if ((size.Length == 0) && (defaultSize.Length == 0))
					return new Tuple<Rectangle, Vector2, char>(Rectangle.Empty, Vector2.Zero, currentCharacter);

				// If we didn't get a size, but we have a default, then use that.
				if ((size.Length == 0) && (defaultSize.Length > 0))
				{
					currentCharacter = Settings.DefaultCharacter;
					size = defaultSize;
				}

				result = size[0].GetBounds(g);

				if ((result.Width < 0.1f) || (result.Height < 0.1f))
				{
					currentCharacter = Settings.DefaultCharacter;
					size = defaultSize;
					result = size[0].GetBounds(g);
					size[0].Dispose();
					size = null;

					if ((result.Width < 0.1f) || (result.Height < 0.1f))
						return new Tuple<Rectangle, Vector2, char>(Rectangle.Empty, Vector2.Zero, c);
				}

				// Don't apply outline padding to whitespace.
				if ((Settings.OutlineSize > 0) && (Settings.OutlineColor.Alpha > 0.0f) && (!char.IsWhiteSpace(c)))
				{
					result.Width += Settings.OutlineSize * 3;
					result.Height += Settings.OutlineSize * 3;
				}
				else
				{
					result.Width += 1;
					result.Height += 1;
				}

				result.Width = (float)System.Math.Ceiling(result.Width);
				result.Height = (float)System.Math.Ceiling(result.Height);

				// Update the cached character bitmap size.
				if ((result.Width * 2 > _charBitmap.Width) || (result.Height > _charBitmap.Height))
				{
					_charBitmap.Dispose();
					_charBitmap = new Bitmap((int)result.Width * 2, (int)result.Height, PixelFormat.Format32bppArgb);					
				}

				Point cropTL = new Point(0, 0);
				Point cropRB = new Point(_charBitmap.Width - 1, _charBitmap.Height - 1);

				// Perform cropping.
				using (System.Drawing.Graphics charGraphics = System.Drawing.Graphics.FromImage(_charBitmap))
				{
					charGraphics.Clear(Color.FromArgb(0, 0, 0, 0));

					charGraphics.PageUnit = g.PageUnit;
					charGraphics.TextContrast = g.TextContrast;
					charGraphics.TextRenderingHint = g.TextRenderingHint;
					
					// Draw the character.
					DrawGlyphCharacter(charGraphics, _charBitmap, font, drawFormat, currentCharacter, new Rectangle(0, 0, _charBitmap.Width, _charBitmap.Height));

					// We use unsafe mode to scan pixels, it's much faster.
					pixels = _charBitmap.LockBits(new Rectangle(0, 0, _charBitmap.Width, _charBitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

					// Crop left and right.
					while ((cropTL.X < cropRB.X) && (IsBitmapColumnEmpty(pixels, cropTL.X)))
						cropTL.X++;

					while ((cropRB.X > cropTL.X) && (IsBitmapColumnEmpty(pixels, cropRB.X)))
						cropRB.X--;

					// Crop top and bottom.
					while ((cropTL.Y < cropRB.Y) && (IsBitmapRowEmpty(pixels, cropTL.Y)))
						cropTL.Y++;

					while ((cropRB.Y > cropTL.Y) && (IsBitmapRowEmpty(pixels, cropRB.Y)))
						cropRB.Y--;

					// If we have a 0 width/height rectangle, then reset to calculated size.
					if (cropTL.X == cropRB.X)
					{
						cropTL.X = (int)result.X;
						cropRB.X = (int)result.Width - 1;
					}

					if (cropTL.Y == cropRB.Y)
					{
						cropTL.Y = (int)result.Y;
						cropRB.Y = (int)result.Height - 1;
					}					
				}

				return new Tuple<Rectangle, Vector2, char>(Rectangle.FromLTRB(cropTL.X, cropTL.Y, cropRB.X, cropRB.Y), cropTL, currentCharacter);
			}
			finally
			{
				if (pixels != null)
					_charBitmap.UnlockBits(pixels);

				if (defaultSize != null)
				{
					foreach (var item in defaultSize)
						item.Dispose();
				}

				if (size != null)
				{
					foreach(var item in size)
						item.Dispose();
				}
			}
		}

		/// <summary>
		/// Function to copy bitmap data to a texture.
		/// </summary>
		/// <param name="bitmap">Bitmap to copy.</param>
		/// <param name="texture">Texture to receive the data.</param>
		private unsafe void CopyBitmap(Bitmap bitmap, GorgonTexture2D texture)
		{
			BitmapData sourcePixels = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			try
			{
				int* pixels = (int*)sourcePixels.Scan0.ToPointer();
				int* offset = null;

				using (GorgonDataStream stream = new GorgonDataStream(texture.SizeInBytes))
				{
					GorgonTexture2DData data = new GorgonTexture2DData(stream, sourcePixels.Stride);

					for (int y = 0; y < bitmap.Height; y++)
					{
						offset = pixels + (y * bitmap.Width);
						for (int x = 0; x < bitmap.Width; x++)
						{
							stream.Write(GorgonColor.FromABGR(*offset).ToARGB());
							offset++;
						}
					}

					stream.Position = 0;
					texture.UpdateSubResource(data);
				}
			}
			finally
			{
				if (sourcePixels != null)
					bitmap.UnlockBits(sourcePixels);
			}
		}

		/// <summary>
		/// Function to save the font to a stream.
		/// </summary>
		/// <param name="stream">Stream to write into.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream parameter does not allow for writing.</exception>
		public void Save(Stream stream)
		{
			Save(stream, false);
		}

		/// <summary>
		/// Function to save the font to a file.
		/// </summary>
		/// <param name="fileName">File name and path of the font to save.</param>
		/// <param name="externalTextures">TRUE to save the textures external to the font file, FALSE to bundle together with the font file.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileName"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fileName parameter is an empty string.</exception>
		/// <remarks>Saving the textures externally with the <paramref name="externalTextures"/> parameter set to TRUE is good for altering the textures in an image 
		/// editing application.  Ultimately, it is recommended that the textures be bundled with the font by setting externalTextures to FALSE.</remarks>
		public void Save(string fileName, bool externalTextures)
		{
			FileStream stream = null;

			GorgonDebug.AssertParamString(fileName, "fileName");

			try
			{
				stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
				Save(stream, externalTextures);
			}
			finally			
			{
				if (stream != null)
					stream.Dispose();
			}
		}

		/// <summary>
		/// Function to save the font to a file.
		/// </summary>
		/// <param name="fileName">File name and path of the font to save.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileName"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the fileName parameter is an empty string.</exception>
		/// <remarks>This overload will always save the textures with the font.</remarks>
		public void Save(string fileName)
		{
			Save(fileName, false);
		}

		/// <summary>
		/// Function to create or update the font.
		/// </summary>
		/// <param name="settings">Font settings to use.</param>
		/// <remarks>
		/// This is used to generate a new set of font textures, and essentially "create" the font object.
		/// <para>This method will clear all the glyphs and textures in the font and rebuild the font with the specified parameters.</para>
		/// <para>Internal textures used by the glyph will be destroyed.  However, if there's a user defined texture or glyph using a user defined texture, then it will not be destroyed 
		/// and clean up will be the responsibility of the user.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the texture size in the settings exceeds that of the capabilities of the feature level.
		/// <para>-or-</para>
		/// <para>Thrown when the font family name is NULL or Empty.</para>
		/// </exception>
		public void Update(GorgonFontSettings settings)
		{
			Font newFont = null;
			FontFamily family = null;
			Bitmap tempBitmap = null;
			StringFormat stringFormat = null;
			StringFormat drawFormat = null;
			System.Drawing.Graphics graphics = null;
			IDictionary<char, ABC> charABC = null;
			IList<KERNINGPAIR> kernPairs = null;
			List<char> availableCharacters = null;
			CharacterRange[] range = new[] { new CharacterRange(0, 1) };

			GorgonDebug.AssertNull<GorgonFontSettings>(settings, "settings");
			GorgonDebug.AssertParamString(settings.FontFamilyName, "Settings.FontFamilyName");

#if DEBUG
			if (settings.TextureSize.Width >= Graphics.Textures.MaxWidth)
				throw new ArgumentException("The texture width '" + settings.TextureSize.Width.ToString() + "' is too large for the current feature level.");

			if (settings.TextureSize.Height >= Graphics.Textures.MaxHeight)
				throw new ArgumentException("The texture height '" + settings.TextureSize.Height.ToString() + "' is too large for the current feature level.");

			if (!settings.Characters.Contains(settings.DefaultCharacter))
				throw new ArgumentException("The default character '" + settings.DefaultCharacter.ToString() + "' does not exist in the font character set.");
#endif

			try
			{
				Settings = settings;

				// Remove all previous textures.
				Textures.Clear();
				// Remove all previous glyphs.
				Glyphs.Clear();
				// Remove all kerning pairs.
				KerningPairs.Clear();

				// Create the font and the rasterizing surface.				
				tempBitmap = new Bitmap(Settings.TextureSize.Width, Settings.TextureSize.Height, PixelFormat.Format32bppArgb);
				
				graphics = System.Drawing.Graphics.FromImage(tempBitmap);
				graphics.PageUnit = GraphicsUnit.Pixel;
				graphics.TextContrast = Settings.TextContrast;
				switch (Settings.AntiAliasingMode)
				{
					case FontAntiAliasMode.AntiAliasHQ:
						graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
						break;
					case FontAntiAliasMode.AntiAlias:
						graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
						break;
					default:
						graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
						break;
				}

				
				// Scale the font appropriately.
				if (Settings.FontHeightMode == FontHeightMode.Points)
					newFont = new Font(Settings.FontFamilyName, (Settings.Size * graphics.DpiY) / 72.0f, Settings.FontStyle, GraphicsUnit.Pixel);
				else
					newFont = new Font(Settings.FontFamilyName, Settings.Size, Settings.FontStyle, GraphicsUnit.Pixel);

				FontHeight = newFont.GetHeight(graphics);
				family = newFont.FontFamily;

				// Get font metrics.
				LineHeight = (FontHeight * family.GetLineSpacing(newFont.Style)) / family.GetEmHeight(newFont.Style);
				Ascent = (FontHeight * family.GetCellAscent(newFont.Style)) / family.GetEmHeight(newFont.Style);
				Descent = (FontHeight * family.GetCellDescent(newFont.Style)) / family.GetEmHeight(newFont.Style);

				stringFormat = new StringFormat(StringFormat.GenericTypographic);
				stringFormat.FormatFlags = StringFormatFlags.NoFontFallback | StringFormatFlags.MeasureTrailingSpaces;
				stringFormat.Alignment = StringAlignment.Near;
				stringFormat.LineAlignment = StringAlignment.Near;
				stringFormat.SetMeasurableCharacterRanges(range);

				// Create a separate drawing format because some glyphs are being clipped when they have overhang
				// on the left boundary.
				drawFormat = new StringFormat(StringFormat.GenericDefault);
				drawFormat.FormatFlags = StringFormatFlags.NoFontFallback | StringFormatFlags.MeasureTrailingSpaces;
				drawFormat.Alignment = StringAlignment.Near;
				drawFormat.LineAlignment = StringAlignment.Near;
				drawFormat.SetMeasurableCharacterRanges(range);

				// Remove control characters and anything below a space.
				availableCharacters = (from chars in Settings.Characters
									   where (!Char.IsControl(chars)) && (!Glyphs.Contains(chars) && (Convert.ToInt32(chars) >= 32) && (!char.IsWhiteSpace(chars)))
									   select chars).ToList();

				// Ensure the default character is there.
				if (!availableCharacters.Contains(settings.DefaultCharacter))
					availableCharacters.Insert(0, settings.DefaultCharacter);

				Win32API.SetActiveFont(graphics, newFont);
				charABC = Win32API.GetCharABCWidths(availableCharacters.First(), availableCharacters.Last());
				if (settings.UseKerningPairs)
				{
					kernPairs = Win32API.GetKerningPairs();

					// Add our kerning pairs.
					if (kernPairs.Count > 0)
					{
						foreach (KERNINGPAIR pair in kernPairs)
						{
							char first = Convert.ToChar(pair.First);
							char second = Convert.ToChar(pair.Second);

							if (((availableCharacters.Contains(first)) || (availableCharacters.Contains(second))) && (pair.KernAmount != 0))
								KerningPairs.Add(new GorgonKerningPair(Convert.ToChar(first), Convert.ToChar(second)), pair.KernAmount);
						}
					}
				}

				Win32API.RestoreActiveObject();

				// Default to the line height size.
				_charBitmap = new Bitmap((int)(System.Math.Ceiling(LineHeight)), (int)(System.Math.Ceiling(LineHeight)));

				// Sort by size.
				List<Tuple<char, int>> charsizes = new List<Tuple<char,int>>();
				foreach (char c in availableCharacters)
				{
					Tuple<Rectangle, Vector2, char> charBounds = GetCharRect(graphics, newFont, stringFormat, drawFormat, c);
					charsizes.Add(new Tuple<char, int>(c, charBounds.Item1.Width * charBounds.Item1.Height));
				}
				
				availableCharacters =	(from sortedChar in charsizes
										orderby sortedChar.Item2 descending, Convert.ToInt32(sortedChar.Item2) ascending 
										select sortedChar.Item1).ToList();

				while (availableCharacters.Count > 0)
				{
					GorgonTexture2D currentTexture = null;

					// Resort our characters from lowest to highest since we've altered the list.
					var characters = availableCharacters.ToArray();

					graphics.Clear(Color.FromArgb(0, 0, 0, 0));

					// Create a texture.
					_textureSettings.Width = Settings.TextureSize.Width;
					_textureSettings.Height = Settings.TextureSize.Height;
					currentTexture = GorgonTexture2D.CreateTexture(Graphics, "GorgonFont." + Name + ".InternalTexture_" + Guid.NewGuid().ToString(), _textureSettings);
					currentTexture.Initialize(null);

					GorgonGlyphPacker.CreateRoot(Settings.TextureSize);

					// Begin rasterization.
					foreach (char c in characters)
					{
						Tuple<Rectangle, Vector2, char> charBounds = GetCharRect(graphics, newFont, stringFormat, drawFormat, c);
						Rectangle? placement = null;
						int packingSpace = Settings.PackingSpacing > 0 ? Settings.PackingSpacing * 2 : 1;

						if (charBounds.Item1 == RectangleF.Empty)
						{
							availableCharacters.Remove(c);
							continue;
						}
						
						Size size = charBounds.Item1.Size;
						size.Width += 1;
						size.Height += 1;

						// Don't add whitespace, we can auto calculate that.
						if (!Char.IsWhiteSpace(charBounds.Item3))
						{
							placement = GorgonGlyphPacker.Add(new Size(charBounds.Item1.Size.Width + packingSpace, charBounds.Item1.Size.Height + packingSpace));
							if (placement != null)
							{
								Point location = placement.Value.Location;

								location.X += Settings.PackingSpacing;
								location.Y += Settings.PackingSpacing;
								availableCharacters.Remove(c);

								graphics.DrawImage(_charBitmap, new Rectangle(location, size), new Rectangle(charBounds.Item1.Location, size), GraphicsUnit.Pixel);
								ABC advanceData = default(ABC);
								if (charABC.ContainsKey(charBounds.Item3))
									advanceData = charABC[charBounds.Item3];
								Glyphs.Add(new GorgonGlyph(charBounds.Item3, currentTexture, new Rectangle(location, size), charBounds.Item2, new Vector3(advanceData.A, advanceData.B, advanceData.C)));								
							}
						}
						else
						{
							// Add whitespace glyph, this will never be rendered, but we need the size in order to determine how much space is required.
							availableCharacters.Remove(c);
							if (!Glyphs.Contains(settings.DefaultCharacter))
								Glyphs.Add(new GorgonGlyph(settings.DefaultCharacter, currentTexture, new Rectangle(0, 0, size.Width, size.Height), Vector2.Zero, Vector3.Zero));
						}
					}

					// Copy the data to the texture.
					CopyBitmap(tempBitmap, currentTexture);
					Textures.Add(currentTexture);
					// Add to internal texture list.
					_textures.Add(currentTexture);
				}

				HasChanged = true;
			}
			finally
			{
				Win32API.RestoreActiveObject();

				if (_charBitmap != null)
					_charBitmap.Dispose();
				if (stringFormat != null)
					stringFormat.Dispose();
				if (drawFormat != null)
					drawFormat.Dispose();
				if (tempBitmap != null)
					tempBitmap.Dispose();
				if (newFont != null)
					newFont.Dispose();
				if (graphics != null)
					graphics.Dispose();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFont"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that created this object.</param>
		/// <param name="name">The name of the font.</param>
		/// <param name="settings">Settings to apply to the font.</param>
		internal GorgonFont(GorgonGraphics graphics, string name, GorgonFontSettings settings)
			: base(name)
		{
			Graphics = graphics;
			Settings = settings;
			Textures = new FontTextureCollection(this);
			Glyphs = new GlyphCollection(this);
			_textures = new List<GorgonTexture2D>();
			_textureSettings = new GorgonTexture2DSettings()
			{
				Width = Settings.TextureSize.Width,
				Height = Settings.TextureSize.Height,
				Format = BufferFormat.R8G8B8A8_UIntNormal,
				ArrayCount = 1,
				IsTextureCube = false,
				MipCount = 1,
				Multisampling = new GorgonMultisampling(1, 0),
				Usage = BufferUsage.Default,
				ViewFormat = BufferFormat.Unknown
			};
			KerningPairs = new Dictionary<GorgonKerningPair, int>();
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
					Graphics.RemoveTrackedObject(this);

					if (Glyphs != null)
						Glyphs.Clear();
					if (Textures != null)
						Textures.Clear();
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

		#region INotifier Members
		/// <summary>
		/// Property to set or return whether an object has been updated.
		/// </summary>
		/// <value></value>
		public bool HasChanged
		{
			get;
			set;
		}
		#endregion
	}
}