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
// Created: Saturday, April 19, 2008 12:32:41 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace GorgonLibrary
{
	/// <summary>
	/// An abstract class representing a hash table of objects.
	/// </summary>
	/// <remarks>
	/// This was designed as a point of convenience to take some of the annoyance out of inheriting a hash table.
	/// <para>
	/// This class provides a simplified method of inheriting a basic list collection.  It holds no benefit over the System.Collections.Generic.Dictionary object, which it uses internally.
	/// </para>
	/// 	<para>
	/// This class, like the other collection classes, implements the IEnumerable interface already to return an iterator interface for enumeration.
	/// </para>
	/// </remarks>
	/// <typeparam name="T">Type of data to store.</typeparam>
	public abstract class BaseDictionaryCollection<T> 
		: IEnumerable<T>
	{
		#region Variables.
		private Dictionary<string, T> _items = null;		// Container for the collection data.
		private bool _caseSensitive = true;					// Case sensitive flag.
		#endregion

		#region Properties.
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

		/// <summary>
		/// Property to return whether the collection keys are case sensitive or not.
		/// </summary>
		public bool IsCaseSensitive
		{
			get
			{
				return _caseSensitive;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return an item with the specified key.
		/// </summary>
		/// <param name="key">Key of the item in the hash map.</param>
		protected virtual T GetItem(string key)
		{
			if (key == null)
				throw new ArgumentNullException("key");
			else if (!Contains(key))
				throw new KeyNotFoundException(key);

			if (!_caseSensitive)
				return _items[key.ToLowerInvariant()];
			else
				return _items[key];
		}

		/// <summary>
		/// Function to add an item with the specified key.
		/// </summary>
		/// <param name="key">Key for the item.</param>
		/// <param name="item">Item to add.</param>
		protected virtual void AddItem(string key, T item)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			string itemKey = key;
			if (Contains(key))
			{
				if (!_caseSensitive)
					_items[key.ToLowerInvariant()] = item;
				else
					_items[key] = item;
			}
			else
			{
				if (!_caseSensitive)
					_items.Add(key.ToLowerInvariant(), item);
				else
					_items.Add(key, item);
			}
		}

		/// <summary>
		/// Function to remove an object from the list by key.
		/// </summary>
		/// <param name="key">Key of the object to remove.</param>
		protected virtual void RemoveItem(string key)
		{
			if (key == null)
				throw new ArgumentNullException("key");
			else if (!Contains(key))
				throw new KeyNotFoundException(key);

			if (!_caseSensitive)
				_items.Remove(key.ToLowerInvariant());
			else
				_items.Remove(key);
		}

		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		protected virtual void ClearItems()
		{
			_items.Clear();
		}

		/// <summary>
		/// Function to return whether a key exists in the collection or not.
		/// </summary>
		/// <param name="key">Key of the object in the collection.</param>
		/// <returns>TRUE if the object exists, FALSE if not.</returns>
		public virtual bool Contains(string key)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			if (!_caseSensitive)
				return _items.ContainsKey(key.ToLowerInvariant());
			else
				return _items.ContainsKey(key);
		}
        
		/// <summary>
		/// Function to return the items in the hash list as a static array.
		/// </summary>
		/// <param name="count">Number of items to copy.</param>
		/// <returns>A static array containing a copy of this hash list.</returns>
		public T[] StaticArray(int count)
		{
			if (count > _items.Count)
				throw new ArgumentOutOfRangeException("count", "The count cannot be greater than that of the collection length.");

			if (count < 1)
				throw new ArgumentOutOfRangeException("count", "Need to have at least 1 element to copy.");

			T[] newArray = new T[count];		// Static array.
			int i = 0;							// Loop.

			// Copy to array.
			foreach (T element in this)
			{
				newArray[i] = element;
				i++;
			}

			return newArray;
		}

		/// <summary>
		/// Function to return the items in the hash table as a static array.
		/// </summary>
		/// <returns>A static array containing a copy of this hash table.</returns>
		public T[] StaticArray()
		{
			return StaticArray(_items.Count);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="BaseDictionaryCollection&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="defaultsize">Default size of the collection.</param>
		/// <param name="caseSensitive">TRUE if keys are case sensitive, FALSE if not.</param>
		protected BaseDictionaryCollection(int defaultsize, bool caseSensitive)
		{
			_items = new Dictionary<string,T>(defaultsize);
			_caseSensitive = caseSensitive;
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
			foreach (KeyValuePair<string, T> item in _items)
				yield return item.Value;
		}
		#endregion
	}

	/// <summary>
	/// Object representing a more concrete version of the dynamic array.
	/// </summary>
	/// <typeparam name="T">Type of object to store.</typeparam>
	public class DictionaryCollection<T> 
		: BaseDictionaryCollection<T>
	{
		#region Properties.
		/// <summary>
		/// Property to return the item with the specified key.
		/// </summary>
		public T this[string key]
		{
			get
			{
				return GetItem(key);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to remove an object from the list by key.
		/// </summary>
		/// <param name="key">Key of the object to remove.</param>
		public void Remove(string key)
		{
			RemoveItem(key);
		}

		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		public void Clear()
		{
			ClearItems();
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="defaultsize">Default size of the collection.</param>
		/// <param name="caseSensitiveKeys">TRUE for case sensitive keys, FALSE for case insensitive keys.</param>
		public DictionaryCollection(int defaultsize, bool caseSensitiveKeys) 
			: base(defaultsize, caseSensitiveKeys)
		{			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DictionaryCollection&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="caseSensitiveKeys">TRUE for case sensitive keys, FALSE for case insensitive keys.</param>
		public DictionaryCollection(bool caseSensitiveKeys)
			: this(51, caseSensitiveKeys)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public DictionaryCollection() 
			: this(51, true)
		{
		}
		#endregion
	}
}
