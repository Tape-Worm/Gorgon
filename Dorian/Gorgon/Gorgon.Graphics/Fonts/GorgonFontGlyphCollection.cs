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
// Created: Sunday, September 22, 2013 11:35:47 AM
// 
#endregion

using System;
using System.Collections.Generic;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A collection of glyphs for the font.
	/// </summary>
	public sealed class GorgonGlyphCollection
		: IDictionary<char, GorgonGlyph>, IEnumerable<GorgonGlyph>
	{
		#region Variables.
		private readonly SortedList<char, GorgonGlyph> _list;
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
			if (glyph == null)
			{
				throw new ArgumentNullException("glyph");
			}

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
			if (glyph == null)
			{
				throw new ArgumentNullException("glyph");
			}

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
			if (glyph == null)
			{
				throw new ArgumentNullException("glyph");
			}

			if (Contains(glyph.Character))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_FONT_GLYPH_EXISTS, glyph.Character), "glyph");
			}

			if (glyph.Texture == null)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_FONT_GLYPH_HAS_NO_TEXTURE, glyph.Character), "glyph");
			}

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
			if (glyph == null)
			{
				throw new ArgumentNullException("glyph");
			}

			if (!Contains(glyph.Character))
			{
				throw new KeyNotFoundException(string.Format(Resources.GORGFX_FONT_GLYPH_NOT_FOUND, glyph.Character));
			}

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
			if ((index < 0)
				|| (index >= _list.Count))
			{
				throw new IndexOutOfRangeException(string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, index, 0, _list.Count));
			}

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
		/// Initializes a new instance of the <see cref="GorgonGlyphCollection"/> class.
		/// </summary>
		internal GorgonGlyphCollection()
		{
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
			// ReSharper disable LoopCanBeConvertedToQuery
			foreach (KeyValuePair<char, GorgonGlyph> item in _list)
			{
				yield return item.Value;
			}
			// ReSharper restore LoopCanBeConvertedToQuery
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
			throw new NotSupportedException();
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
			return _list.GetEnumerator();
		}
		#endregion
	}
}
