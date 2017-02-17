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
using System.Linq;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Pipeline;
using Gorgon.Graphics.Properties;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A list of constant buffers used for shaders.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A <see cref="GorgonConstantBuffer"/> is used to pass data into a shader.
	/// </para>
	/// <para>
	/// This object is immutable, and as such, cannot be modified after creation. 
	/// </para>
	/// </remarks>
	public sealed class GorgonConstantBuffers
		: IGorgonBoundList<GorgonConstantBuffer>, IReadOnlyList<GorgonConstantBuffer>
	{
		#region Variables.
		// The native Direct3D 11 constant buffer list.
		private readonly D3D11.Buffer[] _nativeBuffers = new D3D11.Buffer[D3D11.CommonShaderStage.ConstantBufferApiSlotCount];
		// The list of buffers that are bound to this list.
		private readonly GorgonConstantBuffer[] _buffers = new GorgonConstantBuffer[D3D11.CommonShaderStage.ConstantBufferApiSlotCount];
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the D3D11 constant buffers.
		/// </summary>
		internal D3D11.Buffer[] D3DConstantBuffers => _nativeBuffers;

		/// <summary>
		/// Property to return the maximum number of <see cref="GorgonConstantBuffer"/> objects that can be bound in this list.
		/// </summary>
		public int Count => _buffers.Length;

		/// <summary>
		/// Property to return the number of buffers that are bound.
		/// </summary>
		public int BindCount
		{
			get;
			private set;
		}

		/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
		bool ICollection<GorgonConstantBuffer>.IsReadOnly => false;

		/// <summary>
		/// Property to return the starting index begin binding at.
		/// </summary>
		int IGorgonBoundList<GorgonConstantBuffer>.BindIndex => 0;

		/// <summary>
		/// Property to return whether there are items to bind in this list.
		/// </summary>
		public bool IsEmpty => BindCount == 0;


		/// <summary>Gets the element at the specified index in the read-only list.</summary>
		/// <returns>The element at the specified index in the read-only list.</returns>
		/// <param name="index">The zero-based index of the element to get. </param>
		public GorgonConstantBuffer this[int index]
		{
			get
			{
				return _buffers[index];
			}
			set
			{
				_buffers[index] = value;
				_nativeBuffers[index] = value?.D3DBuffer;
				BindCount = value == null ? 0 : 1;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set multiple <see cref="GorgonConstantBuffer"/> objects at once.
		/// </summary>
		/// <param name="startSlot">The starting slot to assign.</param>
		/// <param name="buffers">The buffers to assign.</param>
		/// <param name="count">[Optional] The number of items to copy from <paramref name="buffers"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffers"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="startSlot"/> is less than 0.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="startSlot"/> plus the number of <paramref name="buffers"/> exceeds the size of this list.</exception>
		/// <remarks>
		/// <para>
		/// Use this method to set a series of objects of type <see cref="GorgonConstantBuffer"/> at once. This will yield better performance than attempting to assign a single item 
		/// at a time via the indexer.
		/// </para>
		/// <para>
		/// This implementation ignores the <paramref name="startSlot"/> parameter.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// Any exceptions thrown by this method will only be thrown when Gorgon is compiled as <b>DEBUG</b>.
		/// </note>
		/// </para>
		/// </remarks>
		void IGorgonBoundList<GorgonConstantBuffer>.SetRange(int startSlot, IReadOnlyList<GorgonConstantBuffer> buffers, int? count)
		{
			SetRange(buffers, count);
		}

		/// <summary>
		/// Function to set multiple <see cref="GorgonConstantBuffer"/> objects at once.
		/// </summary>
		/// <param name="buffers">The buffers to assign.</param>
		/// <param name="count">[Optional] The number of items to copy from <paramref name="buffers"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffers"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the number of <paramref name="buffers"/> exceeds the size of this list.</exception>
		/// <remarks>
		/// <para>
		/// Use this method to set a series of objects of type <see cref="GorgonConstantBuffer"/> at once. This will yield better performance than attempting to assign a single item 
		/// at a time via the indexer.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// Any exceptions thrown by this method will only be thrown when Gorgon is compiled as <b>DEBUG</b>.
		/// </note>
		/// </para>
		/// </remarks>
		public void SetRange(IReadOnlyList<GorgonConstantBuffer> buffers, int? count = null)
		{
			buffers.ValidateObject(nameof(buffers));

			if (count == null)
			{
				count = buffers.Count;
			}

#if DEBUG
			if (count > _buffers.Length)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TOO_MANY_ITEMS, 0, count.Value, _buffers.Length));
			}

			if (count > buffers.Count)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TOO_MANY_ITEMS, 0, count.Value, buffers.Count));
			}
#endif

			if (count == 0)
			{
				Clear();
				return;
			}

			for (int i = 0; i < count.Value; ++i)
			{
				GorgonConstantBuffer buffer = buffers[i];
#if DEBUG
				// Validate to ensure that this buffer is not bound anywhere else within this binding list.
				if (buffer != null)
				{
					int existingItem = Array.IndexOf(_buffers, buffer);

					if ((existingItem != -1) && (existingItem != i))
					{
						throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_RESOURCE_BOUND, buffer.Name, i));
					}
				}
#endif
				_buffers[i] = buffers[i];
				_nativeBuffers[i] = buffers[i]?.D3DBuffer;
			}

			BindCount = count.Value;
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<GorgonConstantBuffer> GetEnumerator()
		{
			for (int i = 0; i < Count; ++i)
			{
				yield return _buffers[i];
			}
		}

		/// <summary>Returns an enumerator that iterates through a collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
		/// <filterpriority>2</filterpriority>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.</summary>
		/// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		public int IndexOf(GorgonConstantBuffer item)
		{
			return Array.IndexOf(_buffers, item);
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

		void IList<GorgonConstantBuffer>.RemoveAt(int index)
		{
			throw new NotSupportedException();
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
				_nativeBuffers[i] = null;
			}

			BindCount = 0;
		}

		/// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
		/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		public bool Contains(GorgonConstantBuffer item)
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
		public void CopyTo(GorgonConstantBuffer[] array, int arrayIndex)
		{
			_buffers.CopyTo(array, arrayIndex);
		}

		/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
		bool ICollection<GorgonConstantBuffer>.Remove(GorgonConstantBuffer item)
		{
			throw new NotSupportedException();
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBuffers"/> class.
		/// </summary>
		/// <param name="buffers">The list of <see cref="GorgonConstantBuffer"/> objects to copy into this list.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffers"/> parameter is <b>null</b>.</exception>
		/// <exception cref="GorgonException">Thrown when a <see cref="GorgonConstantBuffer"/> is bound more than once in the <paramref name="buffers"/> list.</exception>
		public GorgonConstantBuffers(IEnumerable<GorgonConstantBuffer> buffers)
		{
			if (buffers == null)
			{
				throw new ArgumentNullException(nameof(buffers));
			}

			SetRange(buffers.ToArray());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBuffers"/> class.
		/// </summary>
		public GorgonConstantBuffers()
		{
		}
		#endregion
	}
}
