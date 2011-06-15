#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, April 19, 2008 12:25:49 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace GorgonLibrary
{
	/// <summary>
	/// An abstract class for representing a dynamic array of objects.
	/// </summary>
	/// <remarks>
	/// This was designed as a point of convenience to take some of the annoyance out of inheriting a collection.
	/// <para>
	/// This class provides a simplified method of inheriting a basic list collection.  It holds no benefit over the System.Collections.Generic.List object, which it uses internally.
	/// </para>
	/// 	<para>
	/// This class, like the other collection classes, implements the IEnumerable interface already to return an iterator interface for enumeration.
	/// </para>
	/// </remarks>
	/// <typeparam name="T">Type of data to store.</typeparam>
	public abstract class BaseDynamicArray<T> 
		: IEnumerable<T>
	{
		#region Variables.
		private List<T> _items = null;		// Container for the collection data.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the internal list of items.
		/// </summary>
		protected List<T> Items
		{
			get
			{
				return _items;
			}
		}

		/// <summary>
		/// Property to return the number of items in the collection.
		/// </summary>
		public int Count
		{
			get
			{
				return _items.Count;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve an item by its index.
		/// </summary>
		/// <param name="index">Index of the item to retrieve.</param>
		protected virtual T GetItem(int index)
		{
			if ((index < 0) || (index >= _items.Count))
                throw new IndexOutOfRangeException("The index " + index.ToString() + " is not valid for this collection.");

			return _items[index];
		}

		/// <summary>
		/// Function to update an item in the list by its index.
		/// </summary>
		/// <param name="index">Index of the item to set.</param>
		/// <param name="item">Item to set.</param>
		protected virtual void SetItem(int index, T item)
		{
			if ((index < 0) || (index >= _items.Count))
                throw new IndexOutOfRangeException("The index " + index.ToString() + " is not valid for this collection.");

			_items[index] = item;
		}

		/// <summary>
		/// Function to remove an object from the list by index.
		/// </summary>
		/// <param name="index">Index to remove at.</param>
		protected virtual void RemoveItem(int index)
		{
			if ((index<0) || (index>=_items.Count))
                throw new IndexOutOfRangeException("The index " + index.ToString() + " is not valid for this collection.");

			_items.RemoveAt(index);
		}

		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		protected virtual void ClearItems()
		{
			_items.Clear();
		}

		/// <summary>
		/// Function to return whether or not the array contains an instance of the specified object.
		/// </summary>
		/// <param name="item">Object to check for.</param>
		/// <returns>TRUE if object exists, FALSE if not.</returns>
		public virtual bool Contains(T item)
		{
			return _items.Contains(item);
		}

		/// <summary>
		/// Function to return the items in the dynamic array as a static array.
		/// </summary>
		/// <param name="start">Starting index of the array.</param>
		/// <param name="count">Number of items to copy.</param>
		/// <returns>A static array containing a copy of this array.</returns>
		public T[] StaticArray(int start, int count)
		{
			if ((start >= _items.Count) || (start + count > _items.Count) || (start < 0))
				throw new ArgumentOutOfRangeException("start + count", "The starting index and item count cannot be greater than the length of the collection.");

			if (count < 1)
				throw new ArgumentOutOfRangeException("count", "Need to have at least 1 element to copy.");

			T[] newArray = new T[count];		// Static array.

			_items.CopyTo(start, newArray, 0, count);
			return newArray;
		}

		/// <summary>
		/// Function to return the items in the dynamic array as a static array.
		/// </summary>
		/// <returns>A static array containing a copy of this array.</returns>
		public T[] StaticArray()
		{
			return StaticArray(0, _items.Count);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="defaultsize">Default size of the collection.</param>
		protected BaseDynamicArray(int defaultsize)
		{
			_items = new List<T>(defaultsize);
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Function to return a new enumerator object.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		/// <summary>
		/// Function to return a new enumerator object.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			foreach (T item in _items)
				yield return item;
		}
		#endregion
	}

	/// <summary>
	/// Object representing a more concrete version of the dynamic array.
	/// </summary>
	/// <typeparam name="T">Type of object to store.</typeparam>
	public abstract class DynamicArray<T> 
		: BaseDynamicArray<T>
	{
		#region Properties.
		/// <summary>
		/// Property to get or set the item at the specified index.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		public virtual T this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to remove an object from the list by index.
		/// </summary>
		/// <param name="index">Index to remove at.</param>
		public virtual void Remove(int index)
		{
			RemoveItem(index);
		}

		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		public virtual void Clear()
		{
			ClearItems();
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="defaultsize">Default size of the collection.</param>
		protected DynamicArray(int defaultsize) : base(defaultsize)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		protected DynamicArray() : this(16)
		{
		}
		#endregion
	}
}
