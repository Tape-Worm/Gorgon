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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Graphics.Core;

namespace Gorgon.Graphics.Fonts;

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
    /// Gets the number of elements contained in the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <returns>
    /// The number of elements contained in the <see cref="ICollection{T}"/>.
    ///   </returns>
    public int Count => _list.Count;

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown when an attempt to set this property is made.</exception>
    public GorgonGlyph this[int index] => _list.Values[index];

    /// <summary>
    /// Property to set or return a glyph in the collection by its character representation.
    /// </summary>
    public GorgonGlyph this[char character]
    {
        get => _list[character];
        internal set => _list[character] = value;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Adds an item to the <see cref="ICollection{T}" />.
    /// </summary>
    /// <param name="glyph">The object to add to the <see cref="ICollection{T}" />.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="glyph" /> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="glyph" /> parameter already exists in this collection.</exception>
    internal void Add(GorgonGlyph glyph) => _list.Add(glyph.Character, glyph);

    /// <summary>
    /// Function to clear the glyphs.
    /// </summary>
    internal void Clear() => _list.Clear();

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
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
    /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
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
    /// Determines whether the <see cref="ICollection{T}"/> contains a specific value.
    /// </summary>
    /// <param name="glyph">The object to locate in the <see cref="ICollection{T}"/>.</param>
    /// <returns>
    /// true if <paramref name="glyph"/> is found in the <see cref="ICollection{T}"/>; otherwise, false.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="glyph"/> parameter is <b>null</b>.</exception>
    public bool Contains(GorgonGlyph glyph) => glyph is null ? throw new ArgumentNullException(nameof(glyph)) : _list.ContainsValue(glyph);

    /// <summary>
    /// Function to return whether the character exists in this collection.
    /// </summary>
    /// <param name="character">The character to find.</param>
    /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
    public bool Contains(char character) => _list.ContainsKey(character);

    /// <summary>
    /// Function to return the index of a character in the collection.
    /// </summary>
    /// <param name="character">Character to find.</param>
    /// <returns>The index of the character if found, -1 if not.</returns>
    public int IndexOf(char character) => _list.IndexOfKey(character);

    /// <summary>
    /// Function to return the index of a glyph in the collection.
    /// </summary>
    /// <param name="glyph">Glyph to find.</param>
    /// <returns>The index of the glyph if found, -1 if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="glyph"/> parameter is <b>null</b>.</exception>
    public int IndexOf(GorgonGlyph glyph) => glyph is null ? throw new ArgumentNullException(nameof(glyph)) : _list.IndexOfValue(glyph);

    /// <summary>
    /// Function to attempt to retrieve a glyph from the list.
    /// </summary>
    /// <param name="character">Character for the glyph.</param>
    /// <param name="glyph">The glyph in the list.</param>
    /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
        public bool TryGetValue(char character, out GorgonGlyph glyph) => _list.TryGetValue(character, out glyph);

    /// <summary>
    /// Function to retrieve the glyphs in this collection grouped by their respective textures.
    /// </summary>
    /// <returns>A grouping containing the texture and the list of glyphs associated with it.</returns>
    /// <remarks>
    /// <para>
    /// This will only return glyphs that are associated with a texture. Glyphs that do not have a texture (e.g. the glyph representing a space character) will not be included in this list.
    /// </para>
    /// </remarks>
    public IReadOnlyDictionary<GorgonTexture2D, IReadOnlyList<GorgonGlyph>> GetGlyphsByTexture()
    {
        IEnumerable<IGrouping<GorgonTexture2D, GorgonGlyph>> groupedGlyphs = from glyph in this
                                                                             where glyph.TextureView is not null
                                                                             group glyph by glyph.TextureView.Texture;

        return groupedGlyphs.ToDictionary(k => k.Key, v => (IReadOnlyList<GorgonGlyph>)v.ToArray());
    }
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGlyphCollection"/> class.
    /// </summary>
    /// <param name="glyphs">The glyphs to import into this collection.</param>
    internal GorgonGlyphCollection(IEnumerable<GorgonGlyph> glyphs)
        : this()
    {
        foreach (GorgonGlyph glyph in glyphs)
        {
            _list[glyph.Character] = glyph;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGlyphCollection"/> class.
    /// </summary>
    internal GorgonGlyphCollection() => _list = new SortedList<char, GorgonGlyph>(255);
    #endregion
}
