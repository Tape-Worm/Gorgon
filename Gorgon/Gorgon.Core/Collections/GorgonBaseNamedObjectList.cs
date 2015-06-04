#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Tuesday, June 14, 2011 10:12:38 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Gorgon.Core;
using Gorgon.Core.Properties;

namespace Gorgon.Collections
{
	/// <summary>
	/// Base list type for Gorgon library named objects.
	/// </summary>
	/// <typeparam name="T">The type of object to store in the collection. Must implement the <see cref="INamedObject"/> interface.</typeparam>
	public abstract class GorgonBaseNamedObjectList<T>
		: IGorgonNamedObjectList<T>, IGorgonNamedObjectReadOnlyList<T>
		where T : INamedObject
	{
		#region Variables.
		private readonly List<T> _list;
        private readonly StringComparison _caseSensitivity;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of items in the underlying collection.
		/// </summary>
		protected IList<T> Items
		{
			get
			{
				return _list;
			}
		}

		/// <summary>
		/// Property to return whether the keys are case sensitive.
		/// </summary>
		public bool KeysAreCaseSensitive
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the item at the specified index by name.
		/// </summary>
		/// <param name="name">Name of the object to find.</param>
		/// <returns>The object with the specified name.</returns>
		/// <exception cref="KeyNotFoundException">Thrown when no object with the specified <paramref name="name"/> was found in the collection.</exception>
		protected T GetItemByName(string name)
		{
			int index = IndexOf(name);

			if (index == -1)
			{
				throw new KeyNotFoundException(string.Format(Resources.GOR_KEY_NOT_FOUND, "name"));
			}

			return _list[index];
		}

		/// <summary>
		/// Function to add items to the list.
		/// </summary>
		/// <param name="items">List of items to add.</param>
		protected void AddItems(IEnumerable<T> items)
		{
			_list.AddRange(items);
		}

		/// <summary>
		/// Function to insert items into the list at a given index.
		/// </summary>
		/// <param name="index">Index to insert at.</param>
		/// <param name="items">Items to add.</param>
		protected void InsertItems(int index, IEnumerable<T> items)
		{
			_list.InsertRange(index, items);			
		}

		/// <summary>
		/// Function to remove an item from the list by its name.
		/// </summary>
		/// <param name="name">Name of the item to remove.</param>
		protected void RemoveItemByName(string name)
		{
			int index = IndexOf(name);

			if (index == -1)
			{
				throw new KeyNotFoundException(string.Format(Resources.GOR_KEY_NOT_FOUND, name));
			}

			Items.RemoveAt(index);
		}

		/// <summary>
		/// Function to return whether an item with the specified name exists in this collection.
		/// </summary>
		/// <param name="name">Name of the item to find.</param>
		/// <returns><b>true</b> if found, <b>false</b> if not.</returns>
		public bool Contains(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return false;
			}

			// ReSharper disable once LoopCanBeConvertedToQuery
			// ReSharper disable once ForCanBeConvertedToForeach
		    for (int i = 0; i < _list.Count; i++)
		    {
		        if (string.Equals(_list[i].Name, name, _caseSensitivity))
		        {
    		        return true;
		        }
		    }
	
			return false;
		}

		/// <summary>
		/// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
		/// </summary>
		/// <param name="name">Name of the item to find.</param>
		/// <returns>
		/// The index of <paramref name="name"/> if found in the list; otherwise, -1.
		/// </returns>
		public int IndexOf(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return -1;
			}

			for (int i = 0; i < Count; i++)
			{
				T item = _list[i];

			    if (string.Equals(item.Name, name, _caseSensitivity))
			    {
			        return i;
			    }
			}

			return -1;
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBaseNamedObjectList&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="caseSensitive"><b>true</b> to use case sensitive keys, <b>false</b> to ignore casing.</param>
		protected GorgonBaseNamedObjectList(bool caseSensitive)			
		{
			KeysAreCaseSensitive = caseSensitive;

			_caseSensitivity = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

			_list = new List<T>();
		}
		#endregion

		#region IList<T> Members
		/// <summary>
		/// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
		/// <returns>
		/// The index of <paramref name="item"/> if found in the list; otherwise, -1.
		/// </returns>
		public int IndexOf(T item)
		{
			return _list.IndexOf(item);
		}

		/// <summary>
		/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
		/// </exception>
		void IList<T>.Insert(int index, T item)
		{
			_list.Insert(index, item);
		}

		/// <summary>
		/// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
		/// </exception>
		void IList<T>.RemoveAt(int index)
		{
			_list.RemoveAt(index);
		}

		/// <summary>
		/// Property to set or return the item at the specified index.
		/// </summary>
		T IList<T>.this[int index]
		{
			get
			{
				return _list[index];
			}
			set
			{
				_list[index] = value;
			}
		}
		#endregion

		#region ICollection<T> Members
		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		void ICollection<T>.Add(T item)
		{
			_list.Add(item);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		void ICollection<T>.Clear()
		{
			_list.Clear();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
		/// </returns>
		public bool Contains(T item)
		{
			return _list.Contains(item);
		}

		/// <summary>
		/// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="array"/> is null.
		/// </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="arrayIndex"/> is less than 0.
		/// </exception>
		/// <exception cref="T:System.ArgumentException">
		/// 	<paramref name="array"/> is multidimensional.
		/// -or-
		/// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
		/// -or-
		/// The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
		/// -or-
		/// Type <paramref name="array"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.
		/// </exception>
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (Count == 0)
			{
				return;
			}

			_list.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </returns>
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
		/// <value></value>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
		/// </returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsReadOnly
		{
			get 
			{
				return false;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </returns>
		bool ICollection<T>.Remove(T item)
		{
			return _list.Remove(item);
		}
		#endregion

		#region IEnumerable<T> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<T> GetEnumerator()
		{
		    return _list.GetEnumerator();
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_list).GetEnumerator();
		}
		#endregion

		#region IReadOnlyList<T> Members
		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		T IReadOnlyList<T>.this[int index]
		{
			get
			{
				return _list[index];
			}
		}
		#endregion

		#region IGorgonNamedObjectReadOnlyList<T> Members
		/// <summary>
		/// Property to return an item in this list by its name.
		/// </summary>
		T IGorgonNamedObjectReadOnlyList<T>.this[string name]
		{
			get
			{
				return GetItemByName(name);
			}
		}
		#endregion

		#region IGorgonNamedObjectList<T> Members
		#region Properties.
		/// <summary>
		/// Property to set or return an item within this list by its name.
		/// </summary>
		T IGorgonNamedObjectList<T>.this[string name]
		{
			get
			{
				return GetItemByName(name);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to remove an item with the specified name from this list.
		/// </summary>
		/// <param name="name">Name of the item to remove.</param>
		void IGorgonNamedObjectList<T>.Remove(string name)
		{
			RemoveItemByName(name);
		}

		/// <summary>
		/// Function to add a list of items to the list.
		/// </summary>
		/// <param name="items">Items to add to the list.</param>
		void IGorgonNamedObjectList<T>.AddRange(IEnumerable<T> items)
		{
			AddItems(items);
		}

		/// <summary>
		/// Function to remove an item at the specified index.
		/// </summary>
		/// <param name="index">Index to remove at.</param>
		void IGorgonNamedObjectList<T>.RemoveAt(int index)
		{
			_list.RemoveAt(index);
		}

		/// <summary>
		/// Function to remove an item with the specified name from this list.
		/// </summary>
		/// <param name="index">The index of the item to remove.</param>
		void IGorgonNamedObjectList<T>.Remove(int index)
		{
			_list.RemoveAt(index);
		}
		#endregion
		#endregion
	}
}
