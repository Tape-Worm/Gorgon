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
// Created: Thursday, May 21, 2015 11:32:08 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Gorgon.Core;

namespace Gorgon.Collections
{
	/// <summary>
	/// A generic interface for a dictionary of named objects that can be indexed by name.
	/// </summary>
	/// <typeparam name="T">The type of object to store in the collection. Must implement the <see cref="IGorgonNamedObject"/> interface.</typeparam>
	public interface IGorgonNamedObjectDictionary<T> 
		: ICollection<T>
		where T : IGorgonNamedObject
	{
		#region Properties.
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
		/// Property to set or return an item in the dictionary by its name.
		/// </summary>
		T this[string name]
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to remove an item by its name.
		/// </summary>
		/// <param name="name">The name of the object to remove.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
		/// <exception cref="KeyNotFoundException">Thrown when no item with the name specified could be found in the dictionary.</exception>
		void Remove(string name);

		/// <summary>
		/// Function to return whether an item with the specified name exists in this collection.
		/// </summary>
		/// <param name="name">Name of the item to find.</param>
		/// <returns><b>true</b>if found, <b>false</b> if not.</returns>
		bool Contains(string name);

		/// <summary>
		/// Function to return an item from the collection.
		/// </summary>
		/// <param name="name">The name of the item to look up.</param>
		/// <param name="value">The item, if found, or the default value for the type if not.</param>
		/// <returns><b>true</b> if the item was found, <b>false</b> if not.</returns>
		bool TryGetValue(string name, out T value);
		#endregion
	}
}