#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 26, 2016 10:35:58 PM
// 
#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Graphics.Core.Properties;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A list of <see cref="GorgonVertexBufferBinding"/> values.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A <see cref="GorgonVertexBufferBinding"/> is used to bind a vertex buffer to the GPU pipeline so that it may be used for rendering.
	/// </para>
	/// </remarks>
	public sealed class GorgonVertexBufferBindings
		: IList<GorgonVertexBufferBinding>, IReadOnlyList<GorgonVertexBufferBinding>
	{
		#region Constants.
		/// <summary>
		/// The maximum number of vertex buffers allow to be bound at the same time.
		/// </summary>
		public const int MaximumVertexBufferCount = D3D11.InputAssemblerStage.VertexInputResourceSlotCount;
		#endregion

		#region Variables.
		// The binding wrappers.
		private GorgonVertexBufferBinding[] _bindings;
		// The native resource bindings.
		private D3D11.VertexBufferBinding[] _nativeBindings;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the list is in a dirty state or not.
		/// </summary>
		internal bool IsDirty
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the native bindings.
		/// </summary>
		internal D3D11.VertexBufferBinding[] NativeBindings => _nativeBindings;

		/// <summary>
		/// Property to return the maximum number of bindings that can be held in this list.
		/// </summary>
		public int Count => _bindings.Length;

		/// <summary>
		/// Property to return the input layout assigned to the buffer bindings.
		/// </summary>
		/// <remarks>
		/// The input layout defines how the vertex data is arranged within the vertex buffers.
		/// </remarks>
		public GorgonInputLayout InputLayout
		{
			get;
			internal set;
		}

		/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
		bool ICollection<GorgonVertexBufferBinding>.IsReadOnly => false;

		/// <summary>Gets the element at the specified index in the read-only list.</summary>
		/// <returns>The element at the specified index in the read-only list.</returns>
		/// <param name="index">The zero-based index of the element to get. </param>
		public GorgonVertexBufferBinding this[int index]
		{
			get => _bindings[index];
			set
			{
				if (_bindings[index].Equals(ref value))
				{
					return;
				}

				_bindings[index] = value;
				_nativeBindings[index] = value.ToVertexBufferBinding();
				IsDirty = true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<GorgonVertexBufferBinding> GetEnumerator()
		{
			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < _bindings.Length; ++i)
			{
				yield return _bindings[i];
			}
		}

		/// <summary>Returns an enumerator that iterates through a collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
		/// <filterpriority>2</filterpriority>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _bindings.GetEnumerator();
		}

		/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.</summary>
		/// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		public int IndexOf(GorgonVertexBufferBinding item)
		{
			return Array.IndexOf(_bindings, item);
		}

		/// <summary>Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.</summary>
		/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
		/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
		void IList<GorgonVertexBufferBinding>.Insert(int index, GorgonVertexBufferBinding item)
		{
			throw new NotSupportedException();
		}

		/// <summary>Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.</summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
		void IList<GorgonVertexBufferBinding>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		/// <summary>Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
		void ICollection<GorgonVertexBufferBinding>.Add(GorgonVertexBufferBinding item)
		{
			throw new NotSupportedException();
		}

		/// <summary>Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only. </exception>
		public void Clear()
		{
			Array.Clear(_bindings, 0, Count);
			Array.Clear(_nativeBindings, 0, Count);
			IsDirty = true;
		}

		/// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
		/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		public bool Contains(GorgonVertexBufferBinding item)
		{
			return IndexOf(item) > -1;
		}

		/// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex" /> is less than 0.</exception>
		/// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
		public void CopyTo(GorgonVertexBufferBinding[] array, int arrayIndex)
		{
			_bindings.CopyTo(array, arrayIndex);
		}

		/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
		bool ICollection<GorgonVertexBufferBinding>.Remove(GorgonVertexBufferBinding item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Function to copy the elements from a resource binding list to this list.
		/// </summary>
		/// <param name="source">The source list to copy.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="source"/> parameter is <b>null</b>.</exception>
		public void CopyFrom(GorgonVertexBufferBindings source)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			// Resize the arrays if they differ.
			if ((source.Count > Count)
				|| (source.Count < Count))
			{
				Array.Resize(ref _bindings, source.Count);
				Array.Resize(ref _nativeBindings, source.Count);
			}
			
			for (int i = 0; i < Count; ++i)
			{
				_bindings[i] = source._bindings[i];
				_nativeBindings[i] = source._nativeBindings[i];
			}

			// Update dirty flags.
			IsDirty = true;
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexBufferBindings"/> class.
		/// </summary>
		/// <param name="inputLayout">The input layout that describes the arrangement of the vertex data within the buffers being bound.</param>
		/// <param name="size">The number of vertex buffers to store in this list.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="inputLayout"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than 0.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="size"/> is larger than the <see cref="MaximumVertexBufferCount"/>.</exception>
		public GorgonVertexBufferBindings(GorgonInputLayout inputLayout, int size)
		{
			if (size < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(size));
			}

			if (size > MaximumVertexBufferCount)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ARG_OUT_OF_RANGE, MaximumVertexBufferCount, 0));
			}

			InputLayout = inputLayout ?? throw new ArgumentNullException(nameof(inputLayout));
			_bindings = new GorgonVertexBufferBinding[size];
			_nativeBindings = new D3D11.VertexBufferBinding[size];
		}
		#endregion
	}
}
