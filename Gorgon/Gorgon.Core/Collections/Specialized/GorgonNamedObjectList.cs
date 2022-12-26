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
// Created: Thursday, May 21, 2015 11:21:36 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Core;

namespace Gorgon.Collections.Specialized;

/// <summary>
/// A list to contain <see cref="IGorgonNamedObject"/> types.
/// </summary>
/// <typeparam name="T">The type of object to store in the list. Must implement the <see cref="IGorgonNamedObject"/> interface.</typeparam>
/// <remarks>
/// <para>
/// This is a concrete implementation of the <see cref="GorgonBaseNamedObjectList{T}"/> type.
/// </para>
/// <para>
/// This collection is <b><i>not</i></b> thread safe.
/// </para>
/// </remarks>
public class GorgonNamedObjectList<T>
    : GorgonBaseNamedObjectList<T>
    where T : IGorgonNamedObject
{
    #region Properties.
    /// <summary>
    /// Property to set or return an item in this list by index.
    /// </summary>
    public T this[int index]
    {
        get => Items[index];
        set => Items[index] = value;
    }

    /// <summary>
    /// Property to return an item in this list by name.
    /// </summary>
    public T this[string name] => GetItemByName(name);

    #endregion

    #region Methods.
    /// <summary>
    /// Function to clear the items from the list.
    /// </summary>
    public void Clear() => Items.Clear();

    /// <summary>
    /// Function to add an item to the list.
    /// </summary>
    /// <param name="item">Item to add to the list.</param>
    public void Add(T item) => Items.Add(item);

    /// <summary>
    /// Function to add a list of items to this list.
    /// </summary>
    /// <param name="items">The items to add to this list.</param>
    public void AddRange(IEnumerable<T> items) => AddItems(items);

    /// <summary>
    /// Function to insert an item in the list at the specified index.
    /// </summary>
    /// <param name="index">Index to insert at.</param>
    /// <param name="item">Item to insert.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is less than 0.</exception>
    public void Insert(int index, T item)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        Items.Insert(index, item);
    }

    /// <summary>
    /// Function to insert a list of items at the specified index.
    /// </summary>
    /// <param name="index">Index to insert at.</param>
    /// <param name="items">The list of items to insert.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is less than 0, or greater than/equal to the <see cref="GorgonBaseNamedObjectList{T}.Count"/></exception>
    public void InsertRange(int index, IEnumerable<T> items)
    {
        if ((index < 0)
            || (index >= Count))
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        InsertItems(index, items);
    }

    /// <summary>
    /// Function to remove an item from this list.
    /// </summary>
    /// <param name="item">Item to remove from the list.</param>
    public void Remove(T item) => Items.Remove(item);

    /// <summary>
    /// Function to remove an item at the specified index.
    /// </summary>
    /// <param name="index">Index of the item to remove.</param>
    public void Remove(int index) => Items.RemoveAt(index);

    /// <summary>
    /// Function to remove an item with the specified name from this list.
    /// </summary>
    /// <param name="name">Name of the item to remove.</param>
    /// <exception cref="KeyNotFoundException">Thrown when no item with the specified <paramref name="name"/> can be found.</exception>
    public void Remove(string name) => RemoveItemByName(name);
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonNamedObjectList{T}"/> class.
    /// </summary>
    /// <param name="caseSensitive">[Optional] <b>true</b> to use case sensitive keys, <b>false</b> to ignore casing.</param>
    public GorgonNamedObjectList(bool caseSensitive = true)
        : base(caseSensitive)
    {

    }
    #endregion

}
