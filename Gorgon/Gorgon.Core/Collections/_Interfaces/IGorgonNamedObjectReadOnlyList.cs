
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
// Created: Thursday, May 21, 2015 9:44:12 PM
// 

using Gorgon.Core;

namespace Gorgon.Collections;

/// <summary>
/// A generic interface for a read only list of named objects that can be indexed by name and numeric index
/// </summary>
/// <typeparam name="T">The type of object stored in the collection. Must implement the <see cref="IGorgonNamedObject"/> interface.</typeparam>
public interface IGorgonNamedObjectReadOnlyList<T>
    : IReadOnlyList<T>
    where T : IGorgonNamedObject
{
    /// <summary>
    /// Property to return whether the keys are case sensitive.
    /// </summary>
    bool KeysAreCaseSensitive
    {
        get;
    }

    /// <summary>
    /// Property to return an item in this list by its name.
    /// </summary>
    T this[string name]
    {
        get;
    }

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

    /// <summary>
    /// Determines the index of a specific item in the list.
    /// </summary>
    /// <param name="item">The object to locate in the list.</param>
    /// <returns>
    /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
    /// </returns>
    int IndexOf(T item);

    /// <summary>
    /// Determines whether the list contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the list.</param>
    /// <returns>
    /// true if <paramref name="item"/> is found in the list; otherwise, false.
    /// </returns>
    bool Contains(T item);

}
