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
// Created: July 19, 2016 8:49:29 AM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using DX = SharpDX;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A list of scissor rectangles.
	/// </summary>
	/// <remarks>
	/// <para>
	/// ScissorRectangles define a region of pixels to clip on the render target.
	/// </para>
	/// </remarks>
	public sealed class GorgonScissorRectangles
		: IList<DX.Rectangle>, IReadOnlyList<DX.Rectangle>
	{
		#region Variables.
		// The list of scissor rectangles.
		private readonly DX.Rectangle[] _scissorRectangles = new DX.Rectangle[16];
		// The list of actual rectangles to bind.
		private DX.Rectangle[] _actualRects = new DX.Rectangle[0];
		// The number of scissor rectangles bound.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the number of scissor rectangles actually bound to the pipeline.
		/// </summary>
		internal int D3DBindCount
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the DirectX viewport array.
		/// </summary>
		internal DX.Rectangle[] DXScissorRectangles
		{
			get
			{
				if (D3DBindCount != _actualRects.Length)
				{
					_actualRects = new DX.Rectangle[0];
				}

				for (int i = 0; i < D3DBindCount; ++i)
				{
					_actualRects[i] = _scissorRectangles[i];
				}

				return _actualRects;
			}
		}

		/// <summary>
		/// Property to set or return the viewport at the given index.
		/// </summary>
		public DX.Rectangle this[int index]
		{
			get
			{
				return _scissorRectangles[index];
			}

			set
			{
				DX.Rectangle rect = _scissorRectangles[index];
				if (rect.Equals(ref value))
				{
					return;
				}

				_scissorRectangles[index] = value;
				D3DBindCount = 0;

				for (int i = 0; i < _scissorRectangles.Length; ++i)
				{
					if (!_scissorRectangles[i].IsEmpty)
					{
						D3DBindCount = i + 1;
					}
				}
			}
		}

		/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		public int Count => _scissorRectangles.Length;

		/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
		bool ICollection<DX.Rectangle>.IsReadOnly => false;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to copy the scissor states from one to another.
		/// </summary>
		/// <param name="scissors">The list of scissor to copy from.</param>
		internal void CopyFrom(GorgonScissorRectangles scissors)
		{
			if (scissors == null)
			{
				Clear();
				return;
			}

			D3DBindCount = scissors.D3DBindCount;

			if ((_actualRects == null) || (_actualRects.Length != D3DBindCount))
			{
				_actualRects = new DX.Rectangle[D3DBindCount];
			}

			for (int i = 0; i < D3DBindCount; ++i)
			{
				_scissorRectangles[i] = scissors._scissorRectangles[i];
				_actualRects[i] = scissors._actualRects[i];
			}
		}

		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="left">The left instance to compare.</param>
		/// <param name="right">The right instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool Equals(GorgonScissorRectangles left, GorgonScissorRectangles right)
		{
			if ((left == null) || (right == null))
			{
				return false;
			}

			for (int i = 0; i < left.DXScissorRectangles.Length; ++i)
			{
				DX.Rectangle rightport = right[i];
				if (!left[i].Equals(ref rightport))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
		void ICollection<DX.Rectangle>.Add(DX.Rectangle item)
		{
			throw new NotSupportedException();
		}

		/// <summary>Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only. </exception>
		public void Clear()
		{
			for (int i = 0; i < _scissorRectangles.Length; ++i)
			{
				_scissorRectangles[i] = DX.Rectangle.Empty;
			}

			D3DBindCount = 0;
		}

		/// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
		/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		public bool Contains(DX.Rectangle item)
		{
			return Array.IndexOf(_scissorRectangles, item) != -1;
		}

		/// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex" /> is less than 0.</exception>
		/// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
		public void CopyTo(DX.Rectangle[] array, int arrayIndex)
		{
			_scissorRectangles.CopyTo(array, arrayIndex);
		}

		/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.</summary>
		/// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		public int IndexOf(DX.Rectangle item)
		{
			return Array.IndexOf(_scissorRectangles, item);
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<DX.Rectangle> GetEnumerator()
		{
			for (int i = 0; i < _scissorRectangles.Length; ++i)
			{
				yield return _scissorRectangles[i];
			}
		}

		/// <summary>Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.</summary>
		/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
		/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
		void IList<DX.Rectangle>.Insert(int index, DX.Rectangle item)
		{
			throw new NotSupportedException();
		}

		/// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
		bool ICollection<DX.Rectangle>.Remove(DX.Rectangle item)
		{
			throw new NotSupportedException();
		}

		/// <summary>Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.</summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
		void IList<DX.Rectangle>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		/// <summary>Returns an enumerator that iterates through a collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
		/// <filterpriority>2</filterpriority>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _scissorRectangles.GetEnumerator();
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonScissorRectangles"/> class.
		/// </summary>
		public GorgonScissorRectangles()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonScissorRectangles"/> class.
		/// </summary>
		/// <param name="scissorRectangles">The buffers.</param>
		public GorgonScissorRectangles(IEnumerable<DX.Rectangle> scissorRectangles)
		{
			if (scissorRectangles == null)
			{
				return;
			}

			int index = 0;

			foreach (DX.Rectangle viewport in scissorRectangles)
			{
				if (index > _scissorRectangles.Length)
				{
					break;
				}

				this[index++] = viewport;
			}
		}
		#endregion
	}
}
