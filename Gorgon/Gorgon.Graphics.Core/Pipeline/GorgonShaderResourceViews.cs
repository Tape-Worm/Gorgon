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
using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using Gorgon.Graphics.Core.Pipeline;
using Gorgon.Graphics.Core.Properties;
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
		private NativeSrvBinding _nativeViews = new NativeSrvBinding
		                                        {
			                                        Srvs = new D3D11.ShaderResourceView[MaximumShaderResourceViewCount]
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
		/// Function to resize the native binding object list if needed.
		/// </summary>
		/// <param name="newSize">The new size for the list.</param>
		/// <remarks>
		/// <para>
		/// This method must be overridden by the implementing class so that the native list is resized along with this list after calling <see cref="GorgonResourceBindingList{T}.Resize"/>.
		/// </para>
		/// </remarks>
		protected override void OnResizeNativeList(int newSize)
		{
			
		}

		/// <summary>
		/// Function to clear the list of native binding objects.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The implementing class must implement this in order to unassign items from the native binding object list when the <see cref="GorgonResourceBindingList{T}.Clear"/> method is called.
		/// </para>
		/// </remarks>
		protected override void OnClearNativeItems()
		{
			Array.Clear(_nativeViews.Srvs, 0, _nativeViews.Srvs.Length);
			_nativeViews.StartSlot = 0;
			_nativeViews.Count = 0;

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
		protected override void OnSetNativeItem(int index, GorgonShaderResourceView item)
		{
			_nativeViews.Srvs[index] = item?.D3DView;
			_dirty |= 1 << index;
		}

		/// <summary>
		/// Function to retrieve the native shader resource view binding.
		/// </summary>
		/// <returns>A native shader resource view binding.</returns>
		internal NativeSrvBinding GetNativeShaderResources()
		{
			// Nothing's been changed, so send back our array.
			if (_dirty == 0)
			{
				return _nativeViews;
			}

			int firstSlot = -1;
			int slotCount = 0;
			D3D11.ShaderResourceView[] srvs = _nativeViews.Srvs;

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

			_nativeViews = new NativeSrvBinding
			               {
				               Srvs = srvs,
				               Count = slotCount,
				               StartSlot = firstSlot
			               };

			return _nativeViews;
		}

		/// <summary>
		/// Function to set multiple <see cref="GorgonTextureShaderView" /> objects at once.
		/// </summary>
		/// <param name="startSlot">The starting slot to assign.</param>
		/// <param name="views">The views to assign.</param>
		/// <param name="count">[Optional] The number of items to copy from <paramref name="views"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="views" /> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="startSlot" /> is less than 0.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="startSlot" /> plus the number of <paramref name="views"/> exceeds the size of this list.</exception>
		/// <exception cref="GorgonException">Thrown when the same shader resource view is assigned to more than 1 slot.
		/// <para>-or-</para>
		/// <para>The list is locked for writing.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// Use this method to set a series of <see cref="GorgonShaderResourceView" /> items at once. This will yield better performance than attempting to assign a single <see cref="GorgonShaderResourceView" />
		/// at a time via the indexer.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// Any exceptions thrown by this method will only be thrown when Gorgon is compiled as <b>DEBUG</b>.
		/// </note>
		/// </para></remarks>
		public void SetRange(int startSlot, IReadOnlyList<GorgonShaderResourceView> views, int? count = null)
		{
			if (count == null)
			{
				count = views?.Count ?? 0;
			}

#if DEBUG
			if (startSlot < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(startSlot));
			}

			if (count.Value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if (count + startSlot > MaximumShaderResourceViewCount)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TOO_MANY_ITEMS, startSlot, count.Value, MaximumShaderResourceViewCount));
			}

			if (count > (views?.Count ?? 0))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TOO_MANY_ITEMS, 0, count.Value, views?.Count), nameof(count));
			}
#endif
			// Resize accordingly if we have a mismatch.
			if (count.Value != Count)
			{
				Resize(count.Value);
			}

			if ((views == null) || (count == 0))
			{
				Clear();
				return;
			}

			for (int i = 0; i < count.Value; ++i)
			{
				this[i + startSlot] = views[i];
			}
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

			for (int i = 0; i < source.Count; ++i)
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

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderResourceViews"/> class.
		/// </summary>
		/// <param name="shaderResourceViews">The shader resource views to assign.</param>
		/// <param name="startSlot">[Optional] The starting slot to use for the shader resource views.</param>
		/// <exception cref="ArgumentException">Thrown if the number of <paramref name="shaderResourceViews"/> exceeds the <see cref="MaximumShaderResourceViewCount"/>.</exception>
		/// <exception cref="GorgonException">Thrown when the same shader resource view is assigned to more than 1 slot.</exception>
		/// <remarks>
		/// <para>
		/// This overload will set many shader resource views at once.
		/// </para>
		/// <para>
		/// All <paramref name="shaderResourceViews"/> must be assigned to a separate slot, that is, the resource view cannot be assigned to more than 1 slot at a time.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// For the sake of performance, Exceptions thrown by this constructor will only be thrown when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public GorgonShaderResourceViews(IEnumerable<GorgonShaderResourceView> shaderResourceViews, int startSlot = 0)
			: this()
		{
			SetRange(startSlot, shaderResourceViews?.ToArray());
		}
		#endregion
	}
}
