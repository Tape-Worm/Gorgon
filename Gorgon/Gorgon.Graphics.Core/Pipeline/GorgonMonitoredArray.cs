#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: April 2, 2017 2:04:59 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// An array that will monitor and track changes.
	/// </summary>
	/// <typeparam name="T">The type of data in the array. Must be a reference type.</typeparam>
	public class GorgonMonitoredArray<T>
		: IList<T>, IReadOnlyList<T>
		where T : class
	{
		#region Variables.
		// The backing store for the array.
		private readonly T[] _backingStore;
		// The indices that are dirty.
		private int _dirtyIndices;
		// The last set of dirty items.
		private (int Start, int Count, T[] Items) _dirtyItems;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the backing store to objects that need it.
		/// </summary>
		protected T[] BackingArray => _backingStore;
		
		/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		public int Count => _backingStore.Length;

		/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
		bool ICollection<T>.IsReadOnly => false;

		/// <summary>
		/// Property to return whether or not the list is dirty.
		/// </summary>
		public bool IsDirty => _dirtyIndices != 0;

		/// <summary>Gets or sets the element at the specified index.</summary>
		/// <returns>The element at the specified index.</returns>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
		/// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
		public T this[int index]
		{
			get => _backingStore[index];
			set
			{
				if (ReferenceEquals(_backingStore[index], value))
				{
					return;
				}

				_backingStore[index] = value;
				_dirtyIndices |= 1 << index;

				OnItemSet(index, value);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function called when an item is assigned to an index.
		/// </summary>
		/// <param name="index">The index of the item that was assigned.</param>
		/// <param name="value">The value that was assigned.</param>
		protected virtual void OnItemSet(int index, T value)
		{
		}

		/// <summary>
		/// Function called when the array is cleared.
		/// </summary>
		protected virtual void OnClear()
		{
		}

		/// <summary>
		/// Function to validate an item being assigned to a slot.
		/// </summary>
		protected virtual void OnValidate()
		{
			
		}

		/// <summary>
		/// Function to perform validation on this list prior to applying it.
		/// </summary>
		[Conditional("DEBUG")]
		internal void Validate()
		{
#if DEBUG
			OnValidate();
#endif
		}

		/// <summary>
		/// Function to retrieve the dirty items in this list.
		/// </summary>
		/// <param name="peek">[Optional] <b>true</b> if the dirty state should not be modified by calling this method, or <b>false</b> if it should be.</param>
		/// <remarks>
		/// <para>
		/// This will return a tuple that contains the start index, and count of the items that have been changed in this collection.  
		/// </para>
		/// <para>
		/// If the <paramref name="peek"/> parameter is set to <b>true</b>, then the state of this collection is not changed when retrieving the modified objects. Otherwise, the state will be reset and a 
		/// subsequent call to this method will result in a tuple that does not contain any changed values (i.e. the start and count will be 0) until the collection is modified again.
		/// </para>
		/// </remarks>
		public ref (int Start, int Count, T[] Items) GetDirtyItems(bool peek = false)
		{
		    int startSlot = -1;
		    int count = 0;

		    if (_dirtyIndices == 0)
		    {
		        if (_dirtyItems.Items == null)
		        {
		            _dirtyItems = (0, 0, _backingStore);
		        }
		        return ref _dirtyItems;
		    }

		    int dirtyState = _dirtyIndices;

		    for (int i = 0; dirtyState != 0 && i < _backingStore.Length; ++i)
		    {
		        int dirtyMask = 1 << i;

		        if ((dirtyState & dirtyMask) != dirtyMask)
		        {
		            continue;
		        }

		        if (startSlot == -1)
		        {
		            startSlot = i;
		        }

		        ++count;

		        // Remove this bit.
		        dirtyState &= ~dirtyMask;
		    }

		    if (!peek)
		    {
		        _dirtyIndices = dirtyState;
		    }

		    _dirtyItems = (startSlot == -1 ? 0 : startSlot, count, BackingArray);
		    return ref _dirtyItems;
		}

		/// <summary>Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.</summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
		void IList<T>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		/// <summary>Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
		void ICollection<T>.Add(T item)
		{
			throw new NotSupportedException();
		}

		/// <summary>Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.</summary>
		/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
		/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
		void IList<T>.Insert(int index, T item)
		{
			throw new NotSupportedException();
		}

		/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
		bool ICollection<T>.Remove(T item)
		{
			throw new NotSupportedException();
		}

		/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.</summary>
		/// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		public int IndexOf(T item)
		{
			return Array.IndexOf(_backingStore, item);
		}

		/// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
		/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		public bool Contains(T item)
		{
			return IndexOf(item) != -1;
		}

		/// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex" /> is less than 0.</exception>
		/// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
		public void CopyTo(T[] array, int arrayIndex)
		{
			_backingStore.CopyTo(array, arrayIndex);
		}
		
		/// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
		/// <param name="destIndex">[Optional] The destination index in this array to start writing into.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="array" /> is null.</exception>
		public void CopyTo(GorgonMonitoredArray<T> array, int destIndex = 0)
		{
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			if (array == this)
			{
				return;
			}

			if (destIndex < 0)
			{
				destIndex = 0;
			}

			if (destIndex >= array.Count)
			{
				destIndex = array.Count - 1;
			}

			for (int i = 0; i < Count; ++i)
			{
				int index = i + destIndex;

				if (index >= array.Count)
				{
					break;
				}

				array[destIndex] = this[i];
			}
		}

		/// <summary>Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only. </exception>
		public void Clear()
		{
			Array.Clear(_backingStore, 0, _backingStore.Length);
			// Mark all indices as dirty.
			_dirtyIndices = 0;
			_dirtyItems = (0, 0, BackingArray);

			OnClear();
		}
		
		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<T> GetEnumerator()
		{
			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < _backingStore.Length; ++i)
			{
				yield return _backingStore[i];
			}
		}

		/// <summary>Returns an enumerator that iterates through a collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
		/// <filterpriority>2</filterpriority>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _backingStore.GetEnumerator();
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMonitoredValueTypeArray{T}"/> class.
		/// </summary>
		/// <param name="maxSize">The maximum size.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">maxSize</exception>
		public GorgonMonitoredArray(int maxSize)
		{
			_backingStore = new T[maxSize];
		}
		#endregion
	}
}
