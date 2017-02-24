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
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A list of texture sampler states to apply to the pipeline.
	/// </summary>
	public sealed class GorgonSamplerStates
		: IGorgonBoundList<GorgonSamplerState>, IReadOnlyList<GorgonSamplerState>
	{
		#region Variables.
		// Sampler states to apply to the pipeline.
		private readonly GorgonSamplerState[] _states = new GorgonSamplerState[D3D11.CommonShaderStage.SamplerSlotCount];
		// Actual direct 3D sampler states to bind.
		private readonly D3D11.SamplerState[] _nativeStates = new D3D11.SamplerState[D3D11.CommonShaderStage.SamplerSlotCount];
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of actual Direct 3D 11 states.
		/// </summary>
		internal D3D11.SamplerState[] D3DSamplerStates => _nativeStates;

		/// <summary>
		/// Property to return the starting index begin binding at.
		/// </summary>
		public int BindIndex
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of binding slots actually used.
		/// </summary>
		public int BindCount
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether there are any target or depth stencil views set in this list.
		/// </summary>
		public bool IsEmpty => BindCount == 0;

		/// <summary>Gets or sets the element at the specified index.</summary>
		/// <returns>The element at the specified index.</returns>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
		public GorgonSamplerState this[int index]
		{
			get
			{
				return _states[index];
			}
			set
			{
				_states[index] = value;
				_nativeStates[index] = value?.D3DState;
				BindIndex = value == null ? 0 : index;
				BindCount = value == null ? 0 : 1;
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
				_nativeStates[i] = null;
			}

			BindIndex = 0;
			BindCount = 0;
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
			for (int i = 0; i < _states.Length; ++i)
			{
				yield return _states[i];
			}
		}

		/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.</summary>
		/// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		public int IndexOf(GorgonSamplerState item)
		{
			return Array.IndexOf(_states, item);
		}

		/// <summary>
		/// Function to set multiple objects of type <see cref="GorgonSamplerState"/> at once.
		/// </summary>
		/// <param name="startSlot">The starting slot to assign.</param>
		/// <param name="samplerStates">The samplers to assign.</param>
		/// <param name="count">[Optional] The number of items to copy from <paramref name="samplerStates"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="samplerStates"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="startSlot"/> is less than 0.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="startSlot"/> plus the number of <paramref name="samplerStates"/> exceeds the size of this list.</exception>
		/// <remarks>
		/// <para>
		/// Use this method to set a series of objects of type <see cref="GorgonSamplerState"/> at once. This will yield better performance than attempting to assign a single item 
		/// at a time via the indexer.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// Any exceptions thrown by this method will only be thrown when Gorgon is compiled as <b>DEBUG</b>.
		/// </note>
		/// </para>
		/// </remarks>
		public void SetRange(int startSlot, IReadOnlyList<GorgonSamplerState> samplerStates, int? count = null)
		{
			samplerStates.ValidateObject(nameof(samplerStates));

			if (count == null)
			{
				count = samplerStates.Count;
			}

#if DEBUG
			if (startSlot < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(startSlot));
			}

			if (count > samplerStates.Count)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TOO_MANY_ITEMS, 0, count.Value, samplerStates.Count));
			}

			if (startSlot + count > _states.Length)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TOO_MANY_ITEMS, startSlot, count.Value, _states.Length));
			}
#endif

			for (int i = startSlot; i < startSlot + count.Value; ++i)
			{
				_states[i] = samplerStates[i];
				_nativeStates[i] = samplerStates[i]?.D3DState;
			}

			BindIndex = startSlot;
			BindCount = count.Value;
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
		/// <param name="startSlot">[Optional] The starting slot to use for the sampler states.</param>
		public GorgonSamplerStates(IEnumerable<GorgonSamplerState> shaderResourceViews, int startSlot = 0)
		{
			if (shaderResourceViews == null)
			{
				return;
			}

			int index = startSlot;
			
			foreach (GorgonSamplerState view in shaderResourceViews)
			{
				if (index >= _states.Length)
				{
					break;
				}

				this[index++] = view;
			}

			BindIndex = startSlot;
		}
		#endregion
	}
}
