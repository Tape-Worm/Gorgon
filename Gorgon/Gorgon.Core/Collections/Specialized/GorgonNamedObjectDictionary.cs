
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
// Created: Thursday, May 21, 2015 11:21:36 PM
// 

using Gorgon.Core;

namespace Gorgon.Collections.Specialized;

/// <summary>
/// A dictionary to contain <see cref="IGorgonNamedObject"/> types
/// </summary>
/// <typeparam name="T">The type of object to store in the list. Must implement the <see cref="IGorgonNamedObject"/> interface.</typeparam>
/// <remarks>
/// <para>
/// This is a concrete implementation of the <see cref="GorgonBaseNamedObjectDictionary{T}"/> type
/// </para>
/// <para>
/// This collection is <b><i>not</i></b> thread safe
/// </para>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonNamedObjectDictionary{T}"/> class
/// </remarks>
/// <param name="caseSensitive">[Optional] <b>true</b> to use case sensitive keys, <b>false</b> to ignore casing.</param>
public class GorgonNamedObjectDictionary<T>(bool caseSensitive = true)
    : GorgonBaseNamedObjectDictionary<T>(caseSensitive)
    where T : IGorgonNamedObject
{
    /// <summary>
    /// Property to return an item in this list by name.
    /// </summary>
    public T this[string name]
    {
        get => Items[name];
        set
        {
            if (!Items.ContainsKey(name))
            {
                if (value is not null)
                {
                    Items[value.Name] = value;
                }

                return;
            }

            if (value == null)
            {
                Items.Remove(name);
                return;
            }

            UpdateItem(name, value);
        }
    }

    /// <summary>
    /// Function to clear the items from the list.
    /// </summary>
    public void Clear() => Items.Clear();

    /// <summary>
    /// Function to add an item to the list.
    /// </summary>
    /// <param name="item">Item to add to the list.</param>
    public void Add(T item) => Items.Add(item.Name, item);

    /// <summary>
    /// Function to add a list of items to this list.
    /// </summary>
    /// <param name="items">The items to add to this list.</param>
    public void AddRange(IEnumerable<T> items) => AddItems(items);

    /// <summary>
    /// Function to remove an item from this list.
    /// </summary>
    /// <param name="item">Item to remove from the list.</param>
    public void Remove(T item) => Items.Remove(item.Name);

    /// <summary>
    /// Function to remove an item with the specified name from this list.
    /// </summary>
    /// <param name="name">Name of the item to remove.</param>
    /// <exception cref="KeyNotFoundException">Thrown when no item with the specified <paramref name="name"/> can be found.</exception>
    public void Remove(string name) => Items.Remove(name);

}
