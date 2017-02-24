﻿#region MIT.
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
using System.Collections;
using System.Collections.Generic;

namespace Gorgon.Graphics.Fonts
{
	/// <summary>
	/// A collection of glyphs for a <see cref="GorgonFont"/>.
	/// </summary>
	public sealed class GorgonGlyphCollection
		: IReadOnlyList<GorgonGlyph>
	{
		#region Variables.
		// The list of glyphs.
		private readonly SortedList<char, GorgonGlyph> _list;
		#endregion

		#region Properties.
		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <returns>
		/// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		///   </returns>
		public int Count => _list.Count;

		/// <summary>
		/// Gets the element at the specified index.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when an attempt to set this property is made.</exception>
		public GorgonGlyph this[int index] => _list.Values[index];

		/// <summary>
		/// Property to set or return a glyph in the collection by its character representation.
		/// </summary>
		public GorgonGlyph this[char character]
		{
			get
			{
				return _list[character];
			}
			internal set
			{
				_list[character] = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="glyph">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="glyph" /> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="glyph" /> parameter already exists in this collection.</exception>
		internal void Add(GorgonGlyph glyph)
		{
			_list.Add(glyph.Character, glyph);
		}

		/// <summary>
		/// Function to clear the glyphs.
		/// </summary>
		internal void Clear()
		{
			_list.Clear();
		}

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

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (KeyValuePair<char, GorgonGlyph> item in _list)
			{
				yield return item.Value;
			}
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
		/// </summary>
		/// <param name="glyph">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="glyph"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="glyph"/> parameter is <b>null</b>.</exception>
		public bool Contains(GorgonGlyph glyph)
		{
			if (glyph == null)
			{
				throw new ArgumentNullException(nameof(glyph));
			}

			return _list.ContainsValue(glyph);
		}

		/// <summary>
		/// Function to return whether the character exists in this collection.
		/// </summary>
		/// <param name="character">The character to find.</param>
		/// <returns><b>true</b> if found, <b>false</b> if not.</returns>
		public bool Contains(char character)
		{
			return _list.ContainsKey(character);
		}

		/// <summary>
		/// Function to return the index of a character in the collection.
		/// </summary>
		/// <param name="character">Character to find.</param>
		/// <returns>The index of the character if found, -1 if not.</returns>
		public int IndexOf(char character)
		{
			return _list.IndexOfKey(character);
		}

		/// <summary>
		/// Function to return the index of a glyph in the collection.
		/// </summary>
		/// <param name="glyph">Glyph to find.</param>
		/// <returns>The index of the glyph if found, -1 if not.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="glyph"/> parameter is <b>null</b>.</exception>
		public int IndexOf(GorgonGlyph glyph)
		{
			if (glyph == null)
			{
				throw new ArgumentNullException(nameof(glyph));
			}

			return _list.IndexOfValue(glyph);
		}

        /// <summary>
        /// Function to attempt to retrieve a glyph from the list.
        /// </summary>
        /// <param name="character">Character for the glyph.</param>
        /// <param name="glyph">The glyph in the list.</param>
        /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
	    public bool TryGetValue(char character, out GorgonGlyph glyph)
        {
            return _list.TryGetValue(character, out glyph);
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
	}
}
