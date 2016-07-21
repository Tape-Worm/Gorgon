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
// Created: July 9, 2016 3:47:30 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using Gorgon.Core;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A list of constant buffers used for shaders.
	/// </summary>
	public class GorgonConstantBuffers
		: IList<GorgonConstantBuffer>, IReadOnlyList<GorgonConstantBuffer>
	{
		#region Variables.
		// The list of constant buffers.
		private readonly GorgonConstantBuffer[] _buffers = new GorgonConstantBuffer[D3D11.CommonShaderStage.ConstantBufferApiSlotCount];
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the number of buffers actually bound.
		/// </summary>
		internal int D3DConstantBufferBindCount
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the D3D11 constant buffers.
		/// </summary>
		internal D3D11.Buffer[] D3DConstantBuffers
		{
			get;
		}

		/// <summary>
		/// Property to set or return the constant buffer at the given index.
		/// </summary>
		public GorgonConstantBuffer this[int index]
		{
			get
			{
				return _buffers[index];
			}

			set
			{
				if (_buffers[index] == value)
				{
					return;
				}

				_buffers[index] = value;
				D3DConstantBuffers[index] = value?.D3DBuffer;
				D3DConstantBufferBindCount = 0;

				for (int i = 0; i < _buffers.Length; ++i)
				{
					if (_buffers[i] != null)
					{
						D3DConstantBufferBindCount = i + 1;
					}
#if DEBUG
					if ((value != null) && (_buffers[i] == value) && (i != index))
					{
						_buffers[index] = null;
						throw new GorgonException(GorgonResult.CannotBind, string.Format(Properties.Resources.GORGFX_CONSTANTBUFFER_ALREADY_BOUND, value.Name, i));
					}
#endif
				}
			}
		}

		/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		public int Count => _buffers.Length;

		/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
		bool ICollection<GorgonConstantBuffer>.IsReadOnly => false;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="left">The left instance to compare.</param>
		/// <param name="right">The right instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool Equals(GorgonConstantBuffers left, GorgonConstantBuffers right)
		{
			if ((left == null) || (right == null) || (left.D3DConstantBufferBindCount != right.D3DConstantBufferBindCount))
			{
				return false;
			}

			for (int i = 0; i < left.D3DConstantBufferBindCount; ++i)
			{
				if (left[i] != right[i])
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
		void ICollection<GorgonConstantBuffer>.Add(GorgonConstantBuffer item)
		{
			throw new NotSupportedException();
		}

		/// <summary>Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only. </exception>
		public void Clear()
		{
			for (int i = 0; i < _buffers.Length; ++i)
			{
				_buffers[i] = null;
			}
		}

		/// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
		/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		public bool Contains(GorgonConstantBuffer item)
		{
			return Array.IndexOf(_buffers, item) != -1;
		}

		/// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex" /> is less than 0.</exception>
		/// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
		public void CopyTo(GorgonConstantBuffer[] array, int arrayIndex)
		{
			_buffers.CopyTo(array, arrayIndex);
		}

		/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.</summary>
		/// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		public int IndexOf(GorgonConstantBuffer item)
		{
			return Array.IndexOf(_buffers, item);
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<GorgonConstantBuffer> GetEnumerator()
		{
			for (int i = 0; i < _buffers.Length; ++i)
			{
				yield return _buffers[i];
			}
		}

		/// <summary>Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.</summary>
		/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
		/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
		void IList<GorgonConstantBuffer>.Insert(int index, GorgonConstantBuffer item)
		{
			throw new NotSupportedException();
		}

		/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
		bool ICollection<GorgonConstantBuffer>.Remove(GorgonConstantBuffer item)
		{
			throw new NotSupportedException();
		}

		/// <summary>Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.</summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
		void IList<GorgonConstantBuffer>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		/// <summary>Returns an enumerator that iterates through a collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
		/// <filterpriority>2</filterpriority>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _buffers.GetEnumerator();
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBuffers"/> class.
		/// </summary>
		public GorgonConstantBuffers()
		{
			D3DConstantBuffers = new D3D11.Buffer[_buffers.Length];			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBuffers"/> class.
		/// </summary>
		/// <param name="buffers">The buffers.</param>
		public GorgonConstantBuffers(IEnumerable<GorgonConstantBuffer> buffers)
			: this()
		{
			if (buffers == null)
			{
				return;
			}

			int index = 0;

			foreach (GorgonConstantBuffer buffer in buffers)
			{
				if (index >= _buffers.Length)
				{
					break;
				}

				this[index++] = buffer;
			}
		}
		#endregion
	}
}
