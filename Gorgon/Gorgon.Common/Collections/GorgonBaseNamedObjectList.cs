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
using Gorgon.Core;
using Gorgon.Core.Properties;

namespace GorgonLibrary.Collections
{
	/// <summary>
	/// Base list type for Gorgon library named objects.
	/// </summary>
	/// <typeparam name="T">Type of object, must implement <see cref="INamedObject">INamedObject</see>.</typeparam>
	public abstract class GorgonBaseNamedObjectList<T>
		: IList<T>, IReadOnlyList<T>
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
		/// Function to retrieve the item at the specified index.
		/// </summary>
		/// <param name="index">Index of the item to retrieve.</param>
		/// <returns>The item at the specified index.</returns>
		protected virtual T GetItem(int index)
		{
			return _list[index];
		}

		/// <summary>
		/// Function to retrieve the item by its name.
		/// </summary>
		/// <param name="name">Name of the item to find.</param>
		/// <returns>The item with the specified name.</returns>
        protected virtual T GetItem(string name)
		{
			for (int i = 0; i < Count; i++)
			{
				T item = GetItem(i);
			    if (string.Equals(name, item.Name, _caseSensitivity))
			    {
			        return item;
			    }
			}

			throw new KeyNotFoundException(string.Format(Resources.GOR_KEY_NOT_FOUND, name));
		}

		/// <summary>
		/// Function to set an item at the specified index.
		/// </summary>
		/// <param name="index">Index of the item to set.</param>
		/// <param name="value">Value to set the item.</param>
        protected virtual void SetItem(int index, T value)
		{
            _list[index] = value;
		}

		/// <summary>
		/// Function to set an item at the specified index.
		/// </summary>
		/// <param name="value">Value used to set the item with.</param>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the Name property of the <paramref name="value"/> parameter was not found in the collection.</exception>
		protected virtual void SetItem(T value)
		{
            int i = IndexOf(value.Name);

		    if (i == -1)
		    {
		        throw new KeyNotFoundException(string.Format(Resources.GOR_KEY_NOT_FOUND, value.Name));
		    }

		    SetItem(i, value);
		}

		/// <summary>
		/// Function to add an item to the collection.
		/// </summary>
		/// <param name="value">Value to add.</param>
		protected virtual void AddItem(T value)
		{
		    _list.Add(value);
		}

		/// <summary>
		/// Function to remove an item from the collection.
		/// </summary>
		/// <param name="index">The index of the item to remove.</param>
		protected virtual void RemoveItem(int index)
		{
			_list.RemoveAt(index);
		}

		/// <summary>
		/// Function to remove an item from the collection.
		/// </summary>
		/// <param name="item">Item to remove.</param>
		protected virtual void RemoveItem(T item)
		{
			_list.Remove(item);
		}

		/// <summary>
		/// Function to insert an item into the list.
		/// </summary>
		/// <param name="index">Index to insert at.</param>
		/// <param name="value">Value to insert.</param>
		protected virtual void InsertItem(int index, T value)
		{
			_list.Insert(index, value);
		}

		/// <summary>
		/// Function to remove all the items from the collection.
		/// </summary>
		protected virtual void ClearItems()
		{
			_list.Clear();
		}

		/// <summary>
		/// Function to add items to the list.
		/// </summary>
		/// <param name="items">List of items to add.</param>
		protected virtual void AddItems(IEnumerable<T> items)
		{
			_list.AddRange(items);
		}

		/// <summary>
		/// Function to insert items into the list at a given index.
		/// </summary>
		/// <param name="index">Index to insert at.</param>
		/// <param name="items">Items to add.</param>
		protected virtual void InsertItems(int index, IEnumerable<T> items)
		{
			_list.InsertRange(index, items);			
		}

		/// <summary>
		/// Function to return an array of the items in this collection.
		/// </summary>
		/// <returns>The array of items.</returns>
		public T[] ToArray()
		{
			return _list.ToArray();
		}

		/// <summary>
		/// Function to return whether an item with the specified name exists in this collection.
		/// </summary>
		/// <param name="name">Name of the item to find.</param>
		/// <returns>TRUE if found, FALSE if not.</returns>
		public virtual bool Contains(string name)
		{
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
		public virtual int IndexOf(string name)
		{
			for (int i = 0; i < Count; i++)
			{
				T item = GetItem(i);

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
		/// <param name="caseSensitive">TRUE to use case sensitive keys, FALSE to ignore casing.</param>
		protected GorgonBaseNamedObjectList(bool caseSensitive)			
		{
			KeysAreCaseSensitive = caseSensitive;

			_caseSensitivity = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

			_list = new List<T>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonLibrary.Collections.GorgonBaseNamedObjectCollection&lt;T&gt;"/> class.
		/// </summary>
		protected GorgonBaseNamedObjectList()
			: this(false)
		{			
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
		public virtual int IndexOf(T item)
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
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
		/// </exception>
		void IList<T>.Insert(int index, T item)
		{
		    if (IsReadOnly)
		    {
		        throw new NotSupportedException(Resources.GOR_COLLECTION_READ_ONLY);
		    }

		    InsertItem(index, item);
		}

		/// <summary>
		/// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
		/// </exception>
		void IList<T>.RemoveAt(int index)
		{
		    if (IsReadOnly)
		    {
		        throw new NotSupportedException(Resources.GOR_COLLECTION_READ_ONLY);
		    }

		    RemoveItem(index);
		}

		/// <summary>
		/// Property to set or return the item at the specified index.
		/// </summary>
		T IList<T>.this[int index]
		{
			get
			{
				return GetItem(index);
			}
			set
			{
			    if (IsReadOnly)
			    {
			        throw new NotSupportedException(Resources.GOR_COLLECTION_READ_ONLY);
			    }

			    SetItem(index, value);
			}
		}
		#endregion

		#region ICollection<T> Members
		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </exception>
		void ICollection<T>.Add(T item)
		{
			if (IsReadOnly)
			{
				throw new NotSupportedException(Resources.GOR_COLLECTION_READ_ONLY);
			}

			AddItem(item);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </exception>
		void ICollection<T>.Clear()
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
		public virtual bool IsReadOnly
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
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </exception>
		bool ICollection<T>.Remove(T item)
		{
			if (IsReadOnly)
			{
				throw new NotSupportedException(Resources.GOR_COLLECTION_READ_ONLY);
			}

			RemoveItem(item);
			return true;
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
				return GetItem(index);
			}
		}
		#endregion
	}
}
