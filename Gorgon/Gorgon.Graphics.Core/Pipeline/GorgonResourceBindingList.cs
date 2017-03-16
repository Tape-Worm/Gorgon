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
// Created: March 10, 2017 12:21:00 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Math;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// The base class from which other binding lists are derived.
	/// </summary>
	/// <typeparam name="T">The type of data being assigned, must be a reference type.</typeparam>
	/// <remarks>
	/// <para>
	/// The binding list is used to bind objects to the GPU pipeline. This allows an application to bind multiple items at once (which is more efficient), or a single item as needed. 
	/// </para>
	/// <para>
	/// Implementors of this class must create an internal native list that can be sent to the native GPU interfaces, and this list must be also be synchronized.
	/// </para>
	/// </remarks>
	public abstract class GorgonResourceBindingList<T>
		: IList<T>, IReadOnlyList<T>
		where T : class
	{
		#region Variables.
		// The items to bind.
		private T[] _bindingItems;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether or not this list of views is locked for writing.
		/// </summary>
		/// <remarks>
		/// This property will return <b>true</b> when the list is bound to a <see cref="GorgonPipelineResources"/> object. It will return <b>false</b> when it is unbound.
		/// </remarks>
		public bool IsLocked
		{
			get;
			internal set;
		}

		/// <summary>Gets or sets the element at the specified index.</summary>
		/// <exception cref="GorgonException">Thrown when the list is locked for writing.</exception>
		/// <remarks>
		/// <para>
		/// This property will call the <seealso cref="OnValidate"/> method before assigning the item. This is used to ensure the item can be assigned in its current state.
		/// </para>
		/// </remarks>
		public T this[int index]
		{
			get
			{
				return _bindingItems[index];
			}
			set
			{
				if (_bindingItems[index] == value)
				{
					return;
				}

#if DEBUG
				OnValidate(value, index);
#endif

				_bindingItems[index] = value;
				OnSetNativeItem(index, value);
			}
		}

		/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		public int Count => _bindingItems.Length;

		/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
		bool ICollection<T>.IsReadOnly => IsLocked;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to resize the list if needed.
		/// </summary>
		/// <param name="newSize">The new size for the list.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="newSize"/> parameter is less than 0.</exception>
		/// <remarks>
		/// <para>
		/// This method should be called to resize the internal backing array for the binding list. A native item list must be manually resized after calling this to match the new size of the list.
		/// </para>
		/// <para>
		/// When this method is called, the <see cref="OnResizeNativeList"/> method is called to allow the implementing class an opportunity to resize the native binding list.
		/// </para>
		/// </remarks>
		protected void Resize(int newSize)
		{
			if (newSize < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(newSize));
			}

			Array.Resize(ref _bindingItems, newSize);
			OnResizeNativeList(newSize);
		}

		/// <summary>
		/// Function to resize the native binding object list if needed.
		/// </summary>
		/// <param name="newSize">The new size for the list.</param>
		/// <remarks>
		/// <para>
		/// This method must be overridden by the implementing class so that the native list is resized along with this list after calling <see cref="Resize"/>.
		/// </para>
		/// </remarks>
		protected abstract void OnResizeNativeList(int newSize);

		/// <summary>
		/// Function to validate an item being assigned to a slot.
		/// </summary>
		/// <param name="item">The item to validate.</param>
		/// <param name="index">The index of the slot being assigned.</param>
		/// <remarks>
		/// <para>
		/// Implementors can override this method to validate an <paramref name="item"/> prior to assigning it to the list. Exceptions should be thrown when the validation of the item has failed.
		/// </para>
		/// </remarks>
		protected virtual void OnValidate(T item, int index)
		{
		}

		/// <summary>
		/// Function called when an item is assigned to a slot in the binding list.
		/// </summary>
		/// <param name="index">The index of the slot being assigned.</param>
		/// <param name="item">The item being assigned.</param>
		/// <remarks>
		/// <para>
		/// Implementors must override this method to assign the native version of the object to bind. 
		/// </para>
		/// </remarks>
		protected abstract void OnSetNativeItem(int index, T item);

		/// <summary>
		/// Function to clear the list of native binding objects.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The implementing class must implement this in order to unassign items from the native binding object list when the <see cref="Clear"/> method is called.
		/// </para>
		/// </remarks>
		protected abstract void OnClearNativeItems();

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="NotSupportedException">This method is not supported by this type.</exception>
		void ICollection<T>.Add(T item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
		/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		/// <exception cref="NotSupportedException">This method is not supported by this type.</exception>
		void IList<T>.Insert(int index, T item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		/// <exception cref="NotSupportedException">This method is not supported by this type.</exception>
		bool ICollection<T>.Remove(T item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="NotSupportedException">This method is not supported by this type.</exception>
		void IList<T>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _bindingItems.GetEnumerator();
		}

		/// <summary>
		/// Function to unbind all the render target views and depth/stencil views.
		/// </summary>
		/// <exception cref="GorgonException">Thrown when the list is locked for writing.</exception>
		/// <remarks>
		/// <para>
		/// Unlike the <see cref="ICollection{T}.Clear"/> method for an object that implements <see cref="ICollection{T}"/>, this method will not resize the list. It will merely unassign all items bound to the 
		/// list.
		/// </para>
		/// <para>
		/// When the method is called, the <see cref="OnClearNativeItems"/> is called to allow the implementing class an opportunity to remove the native binding objects from the native list.
		/// </para>
		/// </remarks>
		public void Clear()
		{
			Array.Clear(_bindingItems, 0, _bindingItems.Length);
			OnClearNativeItems();
		}

		/// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
		/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		public bool Contains(T item)
		{
			return Array.IndexOf(_bindingItems, item) != -1;
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
			_bindingItems.CopyTo(array, arrayIndex);
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<T> GetEnumerator()
		{
			foreach (T item in _bindingItems)
			{
				yield return item;
			}
		}

		/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.</summary>
		/// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		public int IndexOf(T item)
		{
			return Array.IndexOf(_bindingItems, item);
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonResourceBindingList{T}"/> class.
		/// </summary>
		/// <param name="size">The number of render targets to hold in this list.</param>
		/// <param name="maxSize">The maximum size of the binding list.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than 0.</exception>
		protected GorgonResourceBindingList(int size, int maxSize)
		{
			size.ValidateRange(nameof(size), 0, int.MaxValue);
			_bindingItems = new T[size.Min(maxSize)];
		}
		#endregion
	}
}
