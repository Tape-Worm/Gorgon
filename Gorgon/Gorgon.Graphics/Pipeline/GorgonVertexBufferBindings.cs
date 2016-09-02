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
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
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
		#region Variables.
		// The list of D3D11 vertex buffer bindings.
		private D3D11.VertexBufferBinding[] _actualBindings = new D3D11.VertexBufferBinding[1];
		// The list of vertex buffer bindings.
		private readonly GorgonVertexBufferBinding[] _bindings = new GorgonVertexBufferBinding[D3D11.InputAssemblerStage.VertexInputResourceSlotCount];
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the number of binding slots actually used.
		/// </summary>
		/// <remarks>
		/// This will return the total count from the start to the last <b>non-null</b> entry.  For example, if index 0 is <b>non-null</b>, index 1 is <b>null</b> and index 2 is <b>non-null</b>, then this 
		/// property would return 3 because the item at index 2 is <b>non-null</b>, regardless of whether index 1 is <b>null</b> or not.
		/// </remarks>
		public int BindCount
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the D3D11 vertex buffer binding array.
		/// </summary>
		internal D3D11.VertexBufferBinding[] D3DBindings => _actualBindings;

		/// <summary>
		/// Property to set or return the <see cref="GorgonVertexBufferBinding"/> at the given index.
		/// </summary>
		public GorgonVertexBufferBinding this[int index]
		{
			get
			{
				return _bindings[index];
			}

			set
			{
				GorgonVertexBufferBinding binding = _bindings[index];

				if (binding.Equals(ref value))
				{
					return;
				}
				
				_bindings[index] = value;

				BindCount = 0;
				for (int i = 0; i < _bindings.Length; ++i)
				{
					binding = _bindings[i];
#if DEBUG
					// Do not allow us have the same binding in more than one slot.
					if ((i != index) && (binding.VertexBuffer != null) && (GorgonVertexBufferBinding.Equals(ref binding, ref value)))
					{
						throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_VERTEXBUFFER_ALREADY_BOUND, value.VertexBuffer.Name, i));
					}
#endif

					if (binding.VertexBuffer != null)
					{
						BindCount = i + 1;
					}
				}

				// This creates garbage, but there's not a whole lot we can do because the SetVertexBuffers method on the IA does not allow us to specify a count.
				// If we had proper array slicing, this wouldn't be a problem as we'd be able to pass back the array as a slice of itself (aliased).
				// Maybe .NET 4.7/5.0/whatever will bring this in?
				if (_actualBindings.Length != BindCount)
				{
					_actualBindings = new D3D11.VertexBufferBinding[BindCount];
				}

				_actualBindings[index] = value.ToVertexBufferBinding();
			}
		}

		/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		public int Count => _bindings.Length;

		/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
		bool ICollection<GorgonVertexBufferBinding>.IsReadOnly => false;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to copy the vertex buffer binding states from another list into this one.
		/// </summary>
		/// <param name="bindings">The bindings to copy.</param>
		internal void CopyFrom(GorgonVertexBufferBindings bindings)
		{
			if (bindings == null)
			{
				Clear();
				return;
			}

			BindCount = bindings.BindCount;

			for (int i = 0; i < BindCount; ++i)
			{
				_actualBindings[i] = bindings._actualBindings[i];
				_bindings[i] = bindings._bindings[i];
			}
		}

		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="left">The left instance to compare.</param>
		/// <param name="right">The right instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool Equals(GorgonVertexBufferBindings left, GorgonVertexBufferBindings right)
		{
			if ((left == null) || (right == null) || (left.BindCount != right.BindCount))
			{
				return false;
			}

			for (int i = 0; i < left.BindCount; ++i)
			{
				GorgonVertexBufferBinding rightBuffer = right[i];
				if (!left[i].Equals(ref rightBuffer))
				{
					return false;
				}
			}

			return true;
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
			for (int i = 0; i < _bindings.Length; ++i)
			{
				_bindings[i] = default(GorgonVertexBufferBinding);
			}

			BindCount = 0;
		}

		/// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
		/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		public bool Contains(GorgonVertexBufferBinding item)
		{
			return Array.IndexOf(_bindings, item) != -1;
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

		/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.</summary>
		/// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		public int IndexOf(GorgonVertexBufferBinding item)
		{
			return Array.IndexOf(_bindings, item);
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<GorgonVertexBufferBinding> GetEnumerator()
		{
			for (int i = 0; i < _bindings.Length; ++i)
			{
				yield return _bindings[i];
			}
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

		/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
		bool ICollection<GorgonVertexBufferBinding>.Remove(GorgonVertexBufferBinding item)
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

		/// <summary>Returns an enumerator that iterates through a collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
		/// <filterpriority>2</filterpriority>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _bindings.GetEnumerator();
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexBufferBindings"/> class.
		/// </summary>
		public GorgonVertexBufferBindings()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexBufferBindings"/> class.
		/// </summary>
		/// <param name="viewports">The buffers.</param>
		public GorgonVertexBufferBindings(IEnumerable<GorgonVertexBufferBinding> viewports)
		{
			if (viewports == null)
			{
				return;
			}

			int index = 0;

			foreach (GorgonVertexBufferBinding viewport in viewports)
			{
				if (index > _actualBindings.Length)
				{
					break;
				}

				this[index++] = viewport;
			}
		}
		#endregion
	}
}
