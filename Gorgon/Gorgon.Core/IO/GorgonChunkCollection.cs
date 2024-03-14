#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Sunday, June 14, 2015 9:42:32 PM
// 
#endregion

using System.Collections;

namespace Gorgon.IO;

/// <summary>
/// A collection of chunks within a chunked file.
/// </summary>
internal class GorgonChunkCollection
    : IList<GorgonChunk>, IGorgonReadOnlyChunkCollection
{
    #region Variables.
    // The backing store for the chunks.
    private readonly List<GorgonChunk> _list = [];
    #endregion

    #region IList<GorgonChunk> Members
    #region Properties.
    /// <summary>
    /// Property to set or return a chunk at the specified index.
    /// </summary>
    public GorgonChunk this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Determines the index of a specific item in the <see cref="IList{T}" />.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="IList{T}" />.</param>
    /// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
    public int IndexOf(GorgonChunk item) => _list.IndexOf(item);

    /// <summary>
    /// Inserts an item to the <see cref="IList{T}" /> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
    /// <param name="item">The object to insert into the <see cref="IList{T}" />.</param>
    public void Insert(int index, GorgonChunk item) => _list.Insert(index, item);

    /// <summary>
    /// Removes the <see cref="IList{T}" /> item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    public void RemoveAt(int index) => _list.RemoveAt(index);
    #endregion
    #endregion

    #region ICollection<GorgonChunk> Members
    #region Properties.
    /// <summary>
    /// Gets the number of elements contained in the <see cref="ICollection{T}" />.
    /// </summary>
    /// <value>The count.</value>
    public int Count => _list.Count;

    /// <summary>
    /// Gets a value indicating whether the <see cref="ICollection{T}" /> is read-only.
    /// </summary>
    /// <value><b>true</b> if this instance is read only; otherwise, <b>false</b>.</value>
    bool ICollection<GorgonChunk>.IsReadOnly => false;

    #endregion

    #region Methods.
    /// <summary>
    /// Adds an item to the <see cref="ICollection{T}" />.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="ICollection{T}" />.</param>
    public void Add(GorgonChunk item) => _list.Add(item);

    /// <summary>
    /// Removes all items from the <see cref="ICollection{T}" />.
    /// </summary>
    public void Clear() => _list.Clear();

    /// <summary>
    /// Determines whether the <see cref="ICollection{T}" /> contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="ICollection{T}" />.</param>
    /// <returns>true if <paramref name="item" /> is found in the <see cref="ICollection{T}" />; otherwise, false.</returns>
    public bool Contains(GorgonChunk item) => IndexOf(item) != -1;

    /// <summary>
    /// Copies to.
    /// </summary>
    /// <param name="array">The array.</param>
    /// <param name="arrayIndex">Index of the array.</param>
    public void CopyTo(GorgonChunk[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}" />.
    /// </summary>
    /// <param name="item">The object to remove from the <see cref="ICollection{T}" />.</param>
    /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="ICollection{T}" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="ICollection{T}" />.</returns>
    public bool Remove(GorgonChunk item) => _list.Remove(item);
    #endregion
    #endregion

    #region IEnumerable<GorgonChunk> Members
    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>A <see cref="IEnumerator{T}" /> that can be used to iterate through the collection.</returns>
    public IEnumerator<GorgonChunk> GetEnumerator() => _list.GetEnumerator();
    #endregion

    #region IEnumerable Members
    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>An <see cref="IEnumerator" /> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();
    #endregion

    #region IGorgonReadOnlyChunkCollection Members
    #region Properties.
    /// <summary>
    /// Property to return a chunk by a string identifier.
    /// </summary>
    /// <remarks>
    /// If the chunk is not found, then this property will return <b>null</b>.
    /// </remarks>
    public GorgonChunk this[string chunkName]
    {
        get
        {
            int index = IndexOf(chunkName);

            return index == -1 ? default : _list[index];
        }
    }

    /// <summary>
    /// Property to return a chunk by its <see cref="ulong"/> ID.
    /// </summary>
    /// <remarks>
    /// If the chunk is not found, then this property will return <b>null</b>.
    /// </remarks>
    public GorgonChunk this[ulong id]
    {
        get
        {
            int index = IndexOf(id);

            return index == -1 ? default : _list[index];
        }
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to return the index of a chunk by its name.
    /// </summary>
    /// <param name="chunkName">A text representation of the <see cref="ulong"/> chunk ID.</param>
    /// <returns>The index of the chunk with the specific <paramref name="chunkName"/>, or -1 if not found.</returns>
    public int IndexOf(string chunkName)
    {
        if (string.IsNullOrEmpty(chunkName))
        {
            return -1;
        }

        ulong id = chunkName.ChunkID();

        return IndexOf(id);
    }

    /// <summary>
    /// Function to return whether a chunk exists in this collection or not.
    /// </summary>
    /// <param name="chunkName">A text representation of the <see cref="ulong"/> chunk ID.</param>
    /// <returns><b>true</b> if a chunk exists with the specified <paramref name="chunkName"/>, <b>false</b> if not.</returns>
    public bool Contains(string chunkName) => IndexOf(chunkName) != -1;

    /// <summary>
    /// Function to return the index of a chunk by its name.
    /// </summary>
    /// <param name="chunkID">The <see cref="ulong"/> ID of the chunk.</param>
    /// <returns>The index of the chunk with the specific <paramref name="chunkID"/>, or -1 if not found.</returns>
    public int IndexOf(ulong chunkID)
    {
        for (int i = 0; i < _list.Count; ++i)
        {
            if (_list[i].ID == chunkID)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Function to return whether a chunk exists in this collection or not.
    /// </summary>
    /// <param name="chunkID">The <see cref="ulong"/> ID of the chunk.</param>
    /// <returns><b>true</b> if a chunk exists with the specified <paramref name="chunkID"/>, <b>false</b> if not.</returns>
    public bool Contains(ulong chunkID) => IndexOf(chunkID) != -1;
    #endregion
    #endregion
}
