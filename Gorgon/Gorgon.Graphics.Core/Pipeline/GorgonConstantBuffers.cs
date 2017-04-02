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
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A list of constant buffers used for shaders.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is used to pass a set of <see cref="GorgonConstantBuffer"/> objects to a <see cref="GorgonPipelineResources"/> object. This allows an application to set a single or multiple constant buffers at 
	/// the same time and thus provides a performance boost over setting them individually. 
	/// </para>
	/// </remarks>
	public sealed class GorgonConstantBuffers
		: GorgonResourceBindingList<GorgonConstantBuffer>
	{
		#region Constants.
		/// <summary>
		/// The maximum size for a constant buffer binding list.
		/// </summary>
		public const int MaximumConstantBufferCount = D3D11.CommonShaderStage.ConstantBufferApiSlotCount;
		#endregion

		#region Variables.
		// The native binding.
		private NativeBinding<D3D11.Buffer> _native = new NativeBinding<D3D11.Buffer>
		                                              {
			                                              Bindings = new D3D11.Buffer[MaximumConstantBufferCount]
		                                              };
		// Flag to indicate which indices are changed.
		private int _dirty;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the list is in a dirty state or not.
		/// </summary>
		internal bool IsDirty => _dirty != 0;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clear the list of native binding objects.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The implementing class must implement this in order to unassign items from the native binding object list when the <see cref="GorgonResourceBindingList{T}.Clear"/> method is called.
		/// </para>
		/// </remarks>
		protected override void OnClearItems()
		{
			Array.Clear(_native.Bindings, 0, MaximumConstantBufferCount);
			_native.StartSlot = _native.Count = 0;

			unchecked
			{
				// Max of 14 slots = 2^14.
				_dirty = 0x3fff;
			}
		}

		/// <summary>
		/// Function called when an item is assigned to a slot in the binding list.
		/// </summary>
		/// <param name="index">The index of the slot being assigned.</param>
		/// <param name="item">The item being assigned.</param>
		protected override void OnSetItem(int index, GorgonConstantBuffer item)
		{
			_dirty |= 1 << index;
		}

		/// <summary>
		/// Function to retrieve the native bindings.
		/// </summary>
		/// <returns>A native bindings item.</returns>
		internal ref NativeBinding<D3D11.Buffer> GetNativeBindings()
		{
			// Nothing's been changed, so send back our array.
			if (_dirty == 0)
			{
				return ref _native;
			}

			int firstSlot = -1;
			int slotCount = 0;
			D3D11.Buffer[] buffers = _native.Bindings;

			for (int i = 0; i < MaximumConstantBufferCount; ++i)
			{
				int dirtyMask = 1 << i;

				// Skip this index if we don't have a slot assigned.
				if ((_dirty & dirtyMask) != dirtyMask)
				{
					continue;
				}

				// Record our first slot used.
				if (firstSlot == -1)
				{
					firstSlot = i;
				}

				buffers[i] = this[i]?.D3DBuffer;

				// Remove this bit.
				_dirty &= ~dirtyMask;

				++slotCount;

				if (_dirty == 0)
				{
					break;
				}
			}

			_native = new NativeBinding<D3D11.Buffer>
			                 {
				                 Bindings = buffers,
				                 Count = slotCount,
				                 StartSlot = firstSlot == -1 ? 0 : firstSlot
			                 };

			return ref _native;
		}

		/// <summary>
		/// Function to copy the elements from a resource binding list to this list.
		/// </summary>
		/// <param name="source">The source list to copy.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="source"/> parameter is <b>null</b>.</exception>
		public void CopyFrom(GorgonConstantBuffers source)
		{
			int dirty = 0;

			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			ref NativeBinding<D3D11.Buffer> bindings = ref source.GetNativeBindings();

			for (int i = bindings.StartSlot; i < bindings.StartSlot + bindings.Count; ++i)
			{
				int mask = 1 << i;

				if (this[i] == source[i])
				{
					// If this index was already dirty, then we need to ensure it stays that way.
					if ((_dirty & mask) == mask)
					{
						dirty |= mask;
					}
					continue;
				}

				this[i] = source[i];
				dirty |= mask;
			}

			// Update dirty flags.
			_dirty = dirty;
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBuffers"/> class.
		/// </summary>
		public GorgonConstantBuffers()
			: base(MaximumConstantBufferCount, MaximumConstantBufferCount)
		{
		}
		#endregion
	}
}
