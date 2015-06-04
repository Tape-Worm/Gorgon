#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, April 29, 2013 8:29:54 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Gorgon.Collections.Specialized
{
	/// <summary>
	/// A collection of objects that are implement the <see cref="IDisposable"/> interface.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This collection should be used by factory objects that manage the lifetimes of new objects that are created from those factory objects.
	/// </para>
	/// <para>
	/// This collection is <b><i>not</i></b> thread safe.
	/// </para>
	/// </remarks>
	public class GorgonDisposableObjectCollection
		: IList<IDisposable>
	{
		#region Variables.
		// List of tracked objects.
		private readonly IList<IDisposable> _objects;			
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return an object by its index.
		/// </summary>
		public IDisposable this[int index]
		{
			get
			{
				return _objects[index];
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDisposableObjectCollection"/> class.
		/// </summary>
		public GorgonDisposableObjectCollection()
		{
			_objects = new List<IDisposable>();
		}
		#endregion

		#region IList<IDisposable> Members
		#region Properties.
		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <returns>
		/// The element at the specified index.
		/// </returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the collection.</exception>
		/// <exception cref="T:System.NotSupportedException">The property setter is not supported on this type.</exception>
		IDisposable IList<IDisposable>.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Determines the index of a specific item in the collection.
		/// </summary>
		/// <param name="item">The object to locate in the collection.</param>
		/// <returns>
		/// The index of <paramref name="item"/> if found in the list; otherwise, -1.
		/// </returns>
		public int IndexOf(IDisposable item)
		{
			return _objects.IndexOf(item);
		}

		/// <summary>
		/// Inserts an item to the collection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The object to insert into the collection.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the collection.</exception>
		/// <exception cref="T:System.NotSupportedException">This method is not supported on this type.</exception>
		void IList<IDisposable>.Insert(int index, IDisposable item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes the collection item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the collection.</exception>
		/// <exception cref="T:System.NotSupportedException">This method is not supported on this type.</exception>
		void IList<IDisposable>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
		#endregion
		#endregion

		#region ICollection<IDisposable> Members
		#region Properties.
		/// <summary>
		/// Gets the number of elements contained in the collection.
		/// </summary>
		/// <returns>
		/// The number of elements contained in the collection.
		///   </returns>
		public int Count
		{
			get 
			{
				return _objects.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the collection is read-only.
		/// </summary>
		/// <returns>true if the collection is read-only; otherwise, false.</returns>
		bool ICollection<IDisposable>.IsReadOnly
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Adds an item to the collection.
		/// </summary>
		/// <param name="item">The object to add to the collection.</param>
		public void Add(IDisposable item)
		{
			if (_objects.Contains(item))
			{
				return;
			}

			_objects.Add(item);
		}

		/// <summary>
		/// Function to clear all the objects from this collection.
		/// </summary>
		/// <remarks>
		/// This will not only clear the collection, but will call the dispose method for each object in the collection.
		/// </remarks>
		public void Clear()
		{
			// Clone the object references so that we don't run into trouble if the 
			// object removes itself in their dispose method.
			var items = _objects.ToArray();

			_objects.Clear();

			foreach (var item in items)
			{
				item.Dispose();
			}
		}

		/// <summary>
		/// Determines whether the collection contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the collection.</param>
		/// <returns>
		/// true if <paramref name="item"/> is found in the collection; otherwise, false.
		/// </returns>
		public bool Contains(IDisposable item)
		{
			return _objects.Contains(item);
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		void ICollection<IDisposable>.CopyTo(IDisposable[] array, int arrayIndex)
		{
			_objects.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the collection.
		/// </summary>
		/// <param name="item">The object to remove from the collection.</param>
		/// <returns>
		/// true if <paramref name="item"/> was successfully removed from the collection; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original collection.
		/// </returns>
		public bool Remove(IDisposable item)
		{
			return _objects.Remove(item);
		}
		#endregion
		#endregion

		#region IEnumerable<IDisposable> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<IDisposable> GetEnumerator()
		{
		    return _objects.GetEnumerator();
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
			return GetEnumerator();
		}
		#endregion
	}
}