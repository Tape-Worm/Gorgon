
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Thursday, May 21, 2015 9:36:17 PM
// 

using System.ComponentModel;
using Gorgon.Core;

namespace Gorgon.Collections;

/// <summary>
/// A generic interface for a list of named objects that can be indexed by name and numeric index
/// </summary>
/// <typeparam name="T">The type of object to store in the collection. Must implement the <see cref="IGorgonNamedObject"/> interface.</typeparam>
public interface IGorgonNamedObjectList<T>
    : IList<T>
    where T : IGorgonNamedObject
{
    /// <summary>
    /// Gets a value indicating whether this instance is read only.
    /// </summary>
    /// <value>
    /// <b>true</b> if this instance is read only; otherwise, <b>false</b>.
    /// </value>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new bool IsReadOnly
    {
        get;
    }

    /// <summary>
    /// Property to return whether the keys are case sensitive.
    /// </summary>
    bool KeysAreCaseSensitive
    {
        get;
    }

    /// <summary>
    /// Property to return an item within this list by its name.
    /// </summary>
    T this[string name]
    {
        get;
    }

    /// <summary>
    /// Function to remove an item with the specified name from this list.
    /// </summary>
    /// <param name="name">Name of the item to remove.</param>
    /// <exception cref="KeyNotFoundException">Thrown when no item with the specified <paramref name="name"/> can be found.</exception>
    void Remove(string name);

    /// <summary>
    /// Function to add a list of items to the list.
    /// </summary>
    /// <param name="items">Items to add to the list.</param>
    void AddRange(IEnumerable<T> items);

    /// <summary>
    /// Function to remove an item at the specified index.
    /// </summary>
    /// <param name="index">Index to remove at.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    new void RemoveAt(int index);

    /// <summary>
    /// Function to remove an item by its index.
    /// </summary>
    /// <param name="index">The index of the item to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> parameter is less than 0, or greater than/equal to the Count</exception>
    void Remove(int index);

    /// <summary>
    /// Function to return whether an item with the specified name exists in this collection.
    /// </summary>
    /// <param name="name">Name of the item to find.</param>
    /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
    bool Contains(string name);

    /// <summary>
    /// Determines the index of a specific item in the list.
    /// </summary>
    /// <param name="name">Name of the item to find.</param>
    /// <returns>
    /// The index of <paramref name="name"/> if found in the list; otherwise, -1.
    /// </returns>
    int IndexOf(string name);

}
