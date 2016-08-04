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
// Created: July 4, 2016 1:05:13 AM
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
	/// A list of texture sampler states to apply to the pipeline.
	/// </summary>
	public sealed class GorgonSamplerStates
		: IList<GorgonSamplerState>, IReadOnlyList<GorgonSamplerState>
	{
		#region Variables.
		// Sampler states to apply to the pipeline.
		private readonly GorgonSamplerState[] _states = new GorgonSamplerState[D3D11.CommonShaderStage.SamplerSlotCount];
		// Actual direct 3D sampler states to bind.
		private readonly D3D11.SamplerState[] _actualStates = new D3D11.SamplerState[D3D11.CommonShaderStage.SamplerSlotCount];
		// The number of slots bound.
		private int _bindCount;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of actual Direct 3D 11 states.
		/// </summary>
		internal D3D11.SamplerState[] D3DSamplerStates => _actualStates;

		/// <summary>
		/// Property to return the number of sampler states actually bound.
		/// </summary>
		internal int D3DSamplerStateBindCount => _bindCount;

		/// <summary>
		/// Property to return whether there are any target or depth stencil views set in this list.
		/// </summary>
		public bool IsEmpty => _actualStates.Length == 0;

		/// <summary>Gets or sets the element at the specified index.</summary>
		/// <returns>The element at the specified index.</returns>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
		/// <exception cref="GorgonException">Thrown when the <see cref="GorgonSamplerState"/> being assigned is already assigned in this list.</exception>
		/// <remarks>
		/// <para>
		/// A <see cref="GorgonSamplerState"/> can only be assigned to one slot at a time. If the view is assigned to multiple slots, an exception will be raised.
		/// </para>
		/// <para>
		/// <note type="information">
		/// <para>
		/// The exceptions raised when validating a view against other views in this list are only thrown when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public GorgonSamplerState this[int index]
		{
			get
			{
				return _states[index];
			}
			set
			{
				_states[index] = value;
				_actualStates[index] = value?.D3DState;
				_bindCount = 0;

				// Update the last slot that was bound.
				for (int i = 0; i < _states.Length; ++i)
				{
					if (_states[i] != null)
					{
						_bindCount = i + 1;
					}
				}
			}
		}

		/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		public int Count => _states.Length;

		/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
		bool ICollection<GorgonSamplerState>.IsReadOnly => false;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to copy the states from another set of states.
		/// </summary>
		/// <param name="states">The states to copy.</param>
		public void CopyFrom(GorgonSamplerStates states)
		{
			if (states == null)
			{
				Clear();
				return;
			}

			_bindCount = states.D3DSamplerStateBindCount;

			for (int i = 0; i < _bindCount; ++i)
			{
				_states[i] = states._states[i];
				_actualStates[i] = states._actualStates[i];
			}
		}

		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="left">The left instance to compare.</param>
		/// <param name="right">The right instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool Equals(GorgonSamplerStates left, GorgonSamplerStates right)
		{
			if ((left == null) || (right == null) || (left.D3DSamplerStateBindCount != right.D3DSamplerStateBindCount))
			{
				return false;
			}

			for (int i = 0; i < left.D3DSamplerStateBindCount; ++i)
			{
				if (left[i] != right[i])
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="NotSupportedException">This method is not supported by this type.</exception>
		void ICollection<GorgonSamplerState>.Add(GorgonSamplerState item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
		/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		/// <exception cref="NotSupportedException">This method is not supported by this type.</exception>
		void IList<GorgonSamplerState>.Insert(int index, GorgonSamplerState item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		/// <exception cref="NotSupportedException">This method is not supported by this type.</exception>
		bool ICollection<GorgonSamplerState>.Remove(GorgonSamplerState item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="NotSupportedException">This method is not supported by this type.</exception>
		void IList<GorgonSamplerState>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _states.GetEnumerator();
		}

		/// <summary>
		/// Function to unbind all the shader resource views and depth/stencil views.
		/// </summary>
		public void Clear()
		{
			for (int i = 0; i < _states.Length; ++i)
			{
				_states[i] = null;
				_actualStates[i] = null;
			}

			_bindCount = 0;
		}

		/// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
		/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		public bool Contains(GorgonSamplerState item)
		{
			return Array.IndexOf(_states, item) != -1;
		}

		/// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex" /> is less than 0.</exception>
		/// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
		public void CopyTo(GorgonSamplerState[] array, int arrayIndex)
		{
			_states.CopyTo(array, arrayIndex);
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<GorgonSamplerState> GetEnumerator()
		{
			foreach (GorgonSamplerState view in _states)
			{
				yield return view;
			}
		}

		/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.</summary>
		/// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		public int IndexOf(GorgonSamplerState item)
		{
			return Array.IndexOf(_states, item);
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSamplerStates"/> class.
		/// </summary>
		public GorgonSamplerStates()
		{
			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSamplerStates"/> class.
		/// </summary>
		/// <param name="shaderResourceViews">The shader resource views to assign.</param>
		public GorgonSamplerStates(IEnumerable<GorgonSamplerState> shaderResourceViews)
		{
			if (shaderResourceViews == null)
			{
				return;
			}
			int index = 0;

			foreach (GorgonSamplerState view in shaderResourceViews)
			{
				if (index >= _states.Length)
				{
					break;
				}

				this[index++] = view;
			}
		}
		#endregion
	}
}
