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
using System.Linq;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using D3D11 = SharpDX.Direct3D11;

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
		#region Variables.
		// The list of D3D11 vertex buffer bindings.
		private D3D11.VertexBufferBinding[] _nativeBindings = new D3D11.VertexBufferBinding[0];
		// The list of bindings associated with this object.
		private readonly GorgonVertexBufferBinding[] _bindings = new GorgonVertexBufferBinding[D3D11.InputAssemblerStage.VertexInputResourceSlotCount];
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the D3D11 vertex buffer binding array.
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

		/// <summary>
		/// Property to return the number of binding slots actually used.
		/// </summary>
		public int BindCount => _nativeBindings.Length;

		/// <summary>
		/// Property to return whether there are items to bind in this list.
		/// </summary>
		public bool IsEmpty => _nativeBindings.Length == 0;

		/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
		bool ICollection<GorgonVertexBufferBinding>.IsReadOnly => false;

		/// <summary>Gets the element at the specified index in the read-only list.</summary>
		/// <returns>The element at the specified index in the read-only list.</returns>
		/// <param name="index">The zero-based index of the element to get. </param>
		public GorgonVertexBufferBinding this[int index]
		{
			get
			{
				return _bindings[index];
			}
			set
			{
				_bindings[index] = value;

				if (GorgonVertexBufferBinding.Empty.Equals(value))
				{
					_nativeBindings = new D3D11.VertexBufferBinding[0];
					return;
				}

				if ((_nativeBindings != null) && (_nativeBindings.Length == 1))
				{
					_nativeBindings[0] = value.ToVertexBufferBinding();
					return;
				}

				_nativeBindings = new D3D11.VertexBufferBinding[1];
				_nativeBindings[0] = value.ToVertexBufferBinding();
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

		/// <summary>
		/// Function to set multiple objects of type <see cref="GorgonVertexBufferBinding"/> at once.
		/// </summary>
		/// <param name="bindings">The views to assign.</param>
		/// <param name="count">[Optional] The number of items to copy from <paramref name="bindings"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="bindings"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the number of <paramref name="bindings"/> exceeds the size of this list.</exception>
		/// <remarks>
		/// <para>
		/// Use this method to set a series of objects of type <see cref="GorgonVertexBufferBinding"/> at once. This will yield better performance than attempting to assign a single item 
		/// at a time via the indexer.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// Any exceptions thrown by this method will only be thrown when Gorgon is compiled as <b>DEBUG</b>.
		/// </note>
		/// </para>
		/// </remarks>
		public void SetRange(IReadOnlyList<GorgonVertexBufferBinding> bindings, int? count = null)
		{
			bindings.ValidateObject(nameof(bindings));

			if (count == null)
			{
				count = bindings.Count;
			}

#if DEBUG
			if (count > _bindings.Length)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TOO_MANY_ITEMS, 0, count.Value, _bindings.Length));
			}

			if (count > bindings.Count)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TOO_MANY_ITEMS, 0, count.Value, bindings.Count));
			}
#endif

			if (count == 0)
			{
				_nativeBindings = new D3D11.VertexBufferBinding[0];
				return;
			}

			if ((_nativeBindings == null)
				|| (_nativeBindings.Length != count.Value))
			{
				_nativeBindings = new D3D11.VertexBufferBinding[count.Value];
			}

			for (int i = 0; i < count.Value; ++i)
			{
				GorgonVertexBufferBinding binding = bindings[i];
#if DEBUG
				// Validate to ensure that this buffer is not bound anywhere else within this binding list.
				if (binding.VertexBuffer != null)
				{
					int existingItem = Array.IndexOf(_bindings, binding);

					if ((existingItem != -1) && (existingItem != i))
					{
						throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_RESOURCE_BOUND, binding.VertexBuffer.Name, i));
					}
				}
#endif
				_nativeBindings[i] = binding.ToVertexBufferBinding();
				_bindings[i] = binding;
			}
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
			_nativeBindings = new D3D11.VertexBufferBinding[0];

			for (int i = 0; i < _bindings.Length; ++i)
			{
				_bindings[i] = GorgonVertexBufferBinding.Empty;
			}
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
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexBufferBindings"/> class.
		/// </summary>
		internal GorgonVertexBufferBindings()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexBufferBindings"/> class.
		/// </summary>
		/// <param name="inputLayout">The input layout that describes the arrangement of the vertex data within the buffers being bound.</param>
		/// <param name="bindings">[Optional] A list of <see cref="GorgonVertexBufferBinding"/> items to copy.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="inputLayout"/>, or the <paramref name="bindings"/> parameter is <b>null</b>.</exception>
		/// <exception cref="GorgonException">Thrown when a <see cref="GorgonVertexBufferBinding"/> is bound more than once in the <paramref name="bindings"/> list.</exception>
		public GorgonVertexBufferBindings(GorgonInputLayout inputLayout, IEnumerable<GorgonVertexBufferBinding> bindings)
			: this(inputLayout)
		{
			if (bindings == null)
			{
				throw new ArgumentNullException(nameof(bindings));
			}

			SetRange(bindings.ToArray());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexBufferBindings"/> class.
		/// </summary>
		/// <param name="inputLayout">The input layout that describes the arrangement of the vertex data within the buffers being bound.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="inputLayout"/> parameter is <b>null</b>.</exception>
		public GorgonVertexBufferBindings(GorgonInputLayout inputLayout)
		{
			InputLayout = inputLayout ?? throw new ArgumentNullException(nameof(inputLayout));
		}
		#endregion
	}
}
