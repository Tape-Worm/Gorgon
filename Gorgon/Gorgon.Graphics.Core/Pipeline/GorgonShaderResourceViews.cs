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
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A list of shader resource views to apply to the pipeline.
	/// </summary>
	public sealed class GorgonShaderResourceViews
		: GorgonResourceBindingList<GorgonShaderResourceView>
	{
		#region Constants.
		/// <summary>
		/// The maximum number of allowed shader resources that can be bound at the same time.
		/// </summary>
		public const int MaximumShaderResourceViewCount = 32;
		#endregion

		#region Variables.
		// The flags to indicate whether the first bank of shader resource views are dirty or not.
		private int _dirty;
		// The native resource bindings.
		private NativeBinding<D3D11.ShaderResourceView> _nativeBinding = new NativeBinding<D3D11.ShaderResourceView>
																		{
																			Bindings = new D3D11.ShaderResourceView[MaximumShaderResourceViewCount]
																		};
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the list is in a dirty state or not.
		/// </summary>
		internal bool IsDirty => _dirty != 0;
		#endregion

		#region Methods.
#if DEBUG
		/// <summary>
		/// Function to validate an item being assigned to a slot.
		/// </summary>
		/// <param name="view">The view to validate.</param>
		/// <param name="index">The index of the slot being assigned.</param>
		protected override void OnValidate(GorgonShaderResourceView view, int index)
		{
			if (view == null)
			{
				return;
			}

			GorgonShaderResourceView startView = this.FirstOrDefault(item => item != null);

			// If no other views are assigned, then leave.
			if (startView == null)
			{
				return;
			}

			// Only check if we have more than 1 shader resource view being applied.
			for (int i = 0; i < Count; i++)
			{
				var other = this[i] as GorgonTextureShaderView;

				if (other == null)
				{
					continue;
				}

				if ((other == view) && (i != index))
				{
					throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_VIEW_ALREADY_BOUND, other.Texture.Name));
				}
			}
		}
#endif

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
			Array.Clear(_nativeBinding.Bindings, 0, _nativeBinding.Bindings.Length);
			_nativeBinding.StartSlot = 0;
			_nativeBinding.Count = 0;

			unchecked
			{
				// When clearing all slots are marked as dirty.
				_dirty = (int)0xffffffff;
			}
		}

		/// <summary>
		/// Function called when an item is assigned to a slot in the binding list.
		/// </summary>
		/// <param name="index">The index of the slot being assigned.</param>
		/// <param name="item">The item being assigned.</param>
		protected override void OnSetItem(int index, GorgonShaderResourceView item)
		{
			_dirty |= 1 << index;
		}

		/// <summary>
		/// Function to retrieve the native bindings.
		/// </summary>
		/// <returns>A native bindings item.</returns>
		internal ref NativeBinding<D3D11.ShaderResourceView> GetNativeBindings()
		{
			// Nothing's been changed, so send back our array.
			if (_dirty == 0)
			{
				return ref _nativeBinding;
			}

			int firstSlot = -1;
			int slotCount = 0;
			D3D11.ShaderResourceView[] srvs = _nativeBinding.Bindings;

			for (int i = 0; i < MaximumShaderResourceViewCount; ++i)
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

				srvs[i] = this[i]?.D3DView;

				// Remove this bit.
				_dirty &= ~dirtyMask;

				++slotCount;

				if (_dirty == 0)
				{
					break;
				}
			}

			_nativeBinding = new NativeBinding<D3D11.ShaderResourceView>
			                 {
				                 Bindings = srvs,
				                 Count = slotCount,
				                 StartSlot = firstSlot == -1 ? 0 : firstSlot
			                 };

			return ref _nativeBinding;
		}


		/// <summary>
		/// Function to copy the elements from a resource binding list to this list.
		/// </summary>
		/// <param name="source">The source list to copy.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="source"/> parameter is <b>null</b>.</exception>
		public void CopyFrom(GorgonShaderResourceViews source)
		{
			int dirty = 0;

			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			ref NativeBinding<D3D11.ShaderResourceView> bindings = ref source.GetNativeBindings();

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

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderResourceViews"/> class.
		/// </summary>
		public GorgonShaderResourceViews()
			: base(MaximumShaderResourceViewCount, MaximumShaderResourceViewCount)
		{
		}
		#endregion
	}
}
