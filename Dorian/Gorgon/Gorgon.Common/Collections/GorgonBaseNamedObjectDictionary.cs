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
using System.Collections.Generic;
using GorgonLibrary.Properties;

namespace GorgonLibrary.Collections
{
	/// <summary>
	/// Base dictionary for Gorgon library named objects.
	/// </summary>
	/// <typeparam name="T">Type of object, must implement <see cref="GorgonLibrary.INamedObject">INamedObject</see>.</typeparam>
	public abstract class GorgonBaseNamedObjectDictionary<T>
		: IDictionary<string, T>, IEnumerable<T>
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

		    _list.Add(!KeysAreCaseSensitive ? value.Name.ToLower() : value.Name, value);
		}

		/// <summary>
		/// Function to add several items to the list.
		/// </summary>
		/// <param name="items">IEnumerable containing the items to copy.</param>
		protected virtual void AddItems(IEnumerable<T> items)
		{
			foreach (T item in items)
				AddItem(item);
		}

		/// <summary>
		/// Function to retrieve an item with the specified name.
		/// </summary>
		/// <param name="name">Name of the item to retrieve.</param>
		/// <returns>Item with the specified key.</returns>
		protected virtual T GetItem(string name)
		{
			return !KeysAreCaseSensitive ? _list[name.ToLower()] : _list[name];
		}

		/// <summary>
		/// Fnction to set an item with the specified name.
		/// </summary>
		/// <param name="name">Name of the object to set.</param>
		/// <param name="value">Value to set to the item.</param>
		protected virtual void SetItem(string name, T value)
		{
			if (string.Compare(name, value.Name, !KeysAreCaseSensitive) == 0)
			{
				if (KeysAreCaseSensitive)
				{
					_list[name] = value;
				}
				else
				{
					_list[name.ToLower()] = value;
				}
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
		    _list.Remove(!KeysAreCaseSensitive ? name.ToLower() : name);
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
			return _list.ContainsKey(!KeysAreCaseSensitive ? name.ToLower() : name);
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
			_list = new Dictionary<string, T>(53);
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
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ((System.Collections.IEnumerable)_list).GetEnumerator();
		}
		#endregion

		#region IDictionary<string,T> Members
		/// <summary>
		/// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </summary>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="key"/> is null.
		/// </exception>
		/// <exception cref="T:System.ArgumentException">
		/// An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
		/// </exception>
		void IDictionary<string, T>.Add(string key, T value)
		{
		    if (IsReadOnly)
		    {
		        throw new NotSupportedException(Resources.GOR_COLLECTION_READ_ONLY);
		    }

		    AddItem(value);
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</param>
		/// <returns>
		/// true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="key"/> is null.
		/// </exception>
		bool IDictionary<string, T>.ContainsKey(string key)
		{
			return Contains(key);
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </returns>
		public ICollection<string> Keys
		{
			get 
			{
				return _list.Keys;
			}
		}

		/// <summary>
		/// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns>
		/// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="key"/> is null.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
		/// </exception>
		bool IDictionary<string, T>.Remove(string key)
		{
		    if (IsReadOnly)
		    {
                throw new NotSupportedException(Resources.GOR_COLLECTION_READ_ONLY);
		    }

		    RemoveItem(key);
			return true;
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param>
		/// <returns>
		/// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="key"/> is null.
		/// </exception>
		public bool TryGetValue(string key, out T value)
		{
			if (!Contains(key))
			{
				value = default(T);
				return false;
			}

			value = GetItem(key);
			return true;
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </returns>
		public ICollection<T> Values
		{
			get 
			{
				return _list.Values;
			}
		}

		/// <summary>
		/// Gets or sets the item with the specified key.
		/// </summary>
		T IDictionary<string, T>.this[string key]
		{
			get
			{
				return GetItem(key);
			}
			set
			{
			    if (IsReadOnly)
			    {
                    throw new NotSupportedException(Resources.GOR_COLLECTION_READ_ONLY);
			    }

			    SetItem(key, value);
			}
		}
		#endregion

		#region ICollection<KeyValuePair<string,T>> Members
		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </exception>
		void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> item)
		{
		    if (IsReadOnly)
		    {
                throw new NotSupportedException(Resources.GOR_COLLECTION_READ_ONLY);
		    }

		    AddItem(item.Value);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </exception>
		void ICollection<KeyValuePair<string, T>>.Clear()
		{
		    if (IsReadOnly)
		    {
                throw new NotSupportedException(Resources.GOR_COLLECTION_READ_ONLY);
		    }

		    ClearItems();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
		/// </returns>
		bool ICollection<KeyValuePair<string, T>>.Contains(KeyValuePair<string, T> item)
		{
			return Contains(item.Key);
		}

	    /// <summary>
	    /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
	    /// </summary>
	    /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
	    /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
	    /// <exception cref="NotSupportedException">This method is not implemented.</exception>
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
	    void ICollection<KeyValuePair<string, T>>.CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </exception>
		bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> item)
		{
		    if (IsReadOnly)
		    {
		        throw new NotSupportedException(Resources.GOR_COLLECTION_READ_ONLY);
		    }

		    RemoveItem(item.Key);
			return true;
		}
		#endregion

		#region IEnumerable<KeyValuePair<string,T>> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator()
		{
		    return _list.GetEnumerator();
		}
		#endregion
	}
}
