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
// Created: Tuesday, June 14, 2011 10:12:12 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using Gorgon.Core;
using Gorgon.Core.Properties;

namespace GorgonLibrary.Collections
{
	/// <summary>
	/// Base dictionary for Gorgon library named objects.
	/// </summary>
	/// <typeparam name="T">Type of object, must implement <see cref="INamedObject">INamedObject</see>.</typeparam>
	public abstract class GorgonBaseNamedObjectDictionary<T>
		: ICollection<T>
		where T : INamedObject
	{
		#region Variables.
		private readonly Dictionary<string, T> _list;			// Internal collection to hold our objects.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of items in the underlying collection.
		/// </summary>
		protected IDictionary<string, T> Items
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

		/// <summary>
		/// Property to return whether the collection is read-only or not.
		/// </summary>
		public virtual bool IsReadOnly
		{
			get
			{
				return false;
			}
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
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add an item to the collection.
		/// </summary>
		/// <param name="value">Value to add.</param>
		protected virtual void AddItem(T value)
		{
		    if (Contains(value.Name))
		    {
                throw new ArgumentException(string.Format(Resources.GOR_ITEM_ALREADY_EXISTS, value.Name), "value");
		    }

		    if (string.IsNullOrEmpty(value.Name))
		    {
		        throw new ArgumentException(Resources.GOR_PARAMETER_MUST_NOT_BE_EMPTY, "value");
		    }

		    _list.Add(value.Name, value);
		}

		/// <summary>
		/// Function to add several items to the list.
		/// </summary>
		/// <param name="items">IEnumerable containing the items to copy.</param>
		protected virtual void AddItems(IEnumerable<T> items)
		{
		    foreach (T item in items)
		    {
		        AddItem(item);
		    }
		}

		/// <summary>
		/// Function to retrieve an item with the specified name.
		/// </summary>
		/// <param name="name">Name of the item to retrieve.</param>
		/// <returns>Item with the specified key.</returns>
		protected virtual T GetItem(string name)
		{
			return _list[name];
		}

		/// <summary>
		/// Fnction to set an item with the specified name.
		/// </summary>
		/// <param name="name">Name of the object to set.</param>
		/// <param name="value">Value to set to the item.</param>
		protected virtual void SetItem(string name, T value)
		{
			if (string.Equals(name, value.Name, !KeysAreCaseSensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
			{
			    _list[name] = value;
			}
			else
			{
				RemoveItem(name);
				AddItem(value);
			}
		}

		/// <summary>
		/// Function to remove an item from the collection.
		/// </summary>
		/// <param name="name">Name of the item to remove.</param>
		protected virtual void RemoveItem(string name)
		{
		    _list.Remove(name);
		}

	    /// <summary>
		/// Function to remove an item from the collection.
		/// </summary>
		/// <param name="item">Item to remove.</param>
		protected virtual void RemoveItem(T item)
		{
			RemoveItem(item.Name);
		}

		/// <summary>
		/// Function to remove all the items from the collection.
		/// </summary>
		protected virtual void ClearItems()
		{
			_list.Clear();
		}

		/// <summary>
		/// Function to return whether an item with the specified name exists in this collection.
		/// </summary>
		/// <param name="name">Name of the item to find.</param>
		/// <returns>TRUE if found, FALSE if not.</returns>
		public virtual bool Contains(string name)
		{
			return _list.ContainsKey(name);
		}

		/// <summary>
		/// Function to return whether the specified object exists in the collection.
		/// </summary>
		/// <param name="value">The value to find.</param>
		/// <returns>TRUE if found, FALSE if not.</returns>
		public virtual bool Contains(T value)
		{
			return _list.ContainsValue(value);
		}

		/// <summary>
		/// Function to copy the contents of the collection to an array.
		/// </summary>
		/// <returns>Array containing the contents of this collection.</returns>
		public T[] ToArray()
		{
			var array = new T[Count];
			int i = 0;

			foreach(T item in this)
			{
				array[i] = item;
				i++;
			}

			return array;
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBaseNamedObjectDictionary&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="caseSensitive">TRUE if the key names are case sensitive, FALSE if not.</param>
		protected GorgonBaseNamedObjectDictionary(bool caseSensitive)
		{
		    _list = caseSensitive
		                ? new Dictionary<string, T>()
		                : new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

		    KeysAreCaseSensitive = caseSensitive;
		}
		#endregion

		#region IEnumerable<T> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public virtual IEnumerator<T> GetEnumerator()
		{
			// ReSharper disable once LoopCanBeConvertedToQuery
		    foreach (KeyValuePair<string, T> item in _list)
		    {
		        yield return item.Value;
		    }
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
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (KeyValuePair<string, T> item in _list)
			{
				yield return item.Value;
			}
		}
		#endregion

		#region ICollection<T> Members
		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		void ICollection<T>.Add(T item)
		{
			AddItem(item);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		void ICollection<T>.Clear()
		{
			ClearItems();
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
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}

			if ((array.Rank > 1)
				|| (arrayIndex >= array.Length)
				|| (Count > (array.Length - arrayIndex)))
			{
				throw new ArgumentException();
			}

			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException();
			}

			foreach (var item in _list)
			{
				array[arrayIndex++] = item.Value;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		bool ICollection<T>.Remove(T item)
		{
			if (!Contains(item))
			{
				return false;
			}

			RemoveItem(item);
			return true;
		}
		#endregion
	}
}
