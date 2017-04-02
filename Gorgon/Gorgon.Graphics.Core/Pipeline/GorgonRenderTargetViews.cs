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
using System.Linq;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A list of <see cref="GorgonRenderTargetView"/> objects, and optionally, a <see cref="GorgonDepthStencilView"/>, to apply to the pipeline.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is used to pass a set of <see cref="GorgonRenderTargetView"/> objects to a <see cref="GorgonPipelineResources"/> object. This allows an application to set a single or multiple render target views 
	/// at the same time and thus provides a performance boost over setting them individually. 
	/// </para>
	/// <para>
	/// While the render target views and the optional depth/stencil view assigned to this object can use differing formats, they must conform to a series of restrictions when assigning multiple items at once. 
	/// <list type="bullet">
	/// <item>
	/// A <see cref="GorgonRenderTargetView"/> can only be assigned to a single slot, if the same <see cref="GorgonRenderTargetView"/> is assigned to multiple slots, an exception will be thrown.
	/// </item>
	/// <item>
	/// When binding a <see cref="GorgonRenderTargetView"/>, the resource must be of the same type as other resources for other views in this list. If they do not match, an exception will be thrown. For 
	/// example, if a render target and depth stencil view are linked to a texture, but one is a 2D texture, and the other is 1D texture, then this would violate this restriction.
	/// </item>
	/// <item>
	/// All <see cref="GorgonRenderTargetView"/> parameters, such as array (or depth) index and array (or depth) count must be the same as the other views in this list. If they are not, an 
	/// exception will be thrown. Mip slices may be different. An exception will also be raised if the resources attached to views in this list do not have the same array/depth count.
	/// </item>
	/// <item>
	/// If the views are attached to resources with multisampling enabled through <see cref="GorgonMultisampleInfo"/>, then the <see cref="GorgonMultisampleInfo"/> of the resource attached to the view 
	/// being assigned must match, or an exception will be thrown.
	/// </item>
	/// <item>
	/// These limitations also apply to the <see cref="DepthStencilView"/> property. All views must match the mip slice, array (or depth) index, and array (or depth) count, and the <see cref="ResourceType"/> 
	/// for the resources attached to the views must be the same.
	/// </item>
	/// </list> 
	/// </para>
	/// </remarks>
	public sealed class GorgonRenderTargetViews
		: GorgonResourceBindingList<GorgonRenderTargetView>
	{
		#region Constants.
		/// <summary>
		/// The maximum number of allowed render targets that can be set at once, and the maximum size for this list type.
		/// </summary>
		public const int MaximumRenderTargetCount = D3D11.OutputMergerStage.SimultaneousRenderTargetCount;
		#endregion

		#region Variables.
		// The flags to indicate whether the first bank of shader resource views are dirty or not.
		private int _dirty;
		// The native bindings.
		private NativeBinding<D3D11.RenderTargetView> _native = new NativeBinding<D3D11.RenderTargetView>
		                                                        {
			                                                        Bindings = new D3D11.RenderTargetView[MaximumRenderTargetCount]
		                                                        };
		// The currently active depth stencil view.
		private GorgonDepthStencilView _depthStencilView;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the list is in a dirty state or not.
		/// </summary>
		internal bool IsDirty => _dirty != 0;

		/// <summary>
		/// Property to set or return the currently active depth/stencil view.
		/// </summary>
		/// <exception cref="GorgonException">Thrown when the resource type for the resource bound to the <see cref="GorgonDepthStencilView"/> being assigned does not match the resource type for resources 
		/// attached to other views in this list.
		/// <para>-or-</para>
		/// <para>Thrown when the array (or depth) index, or the array (or depth) count does not match the other views in this list.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the resource <see cref="GorgonMultisampleInfo"/> does not match the <see cref="GorgonMultisampleInfo"/> for other resources bound to other views on this list.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// When binding a <see cref="GorgonDepthStencilView"/>, the resource must be of the same type as other resources for other views in this list. If they do not match, an exception will be thrown.
		/// </para>
		/// <para>
		/// All <see cref="GorgonDepthStencilView"/> parameters, such as array (or depth) index and array (or depth) count must be the same as the other views in this list. If they are not, an exception 
		/// will be thrown. Mip slices may be different. An exception will also be raised if the resources attached to <see cref="GorgonRenderTargetView">GorgonRenderTargetViews</see> in this list do not 
		/// have the same array/depth count.
		/// </para>
		/// <para>
		/// If the <see cref="GorgonRenderTargetView">GorgonRenderTargetViews</see> are attached to resources with multisampling enabled through <see cref="GorgonMultisampleInfo"/>, then the 
		/// <see cref="GorgonMultisampleInfo"/> of the resource attached to the <see cref="GorgonDepthStencilView"/> being assigned must match, or an exception will be thrown.
		/// </para>
		/// <para>
		/// These limitations also apply to the <see cref="DepthStencilView"/> property. All views must match the mip slice, array (or depth) index, and array (or depth) count, and the <see cref="ResourceType"/> 
		/// for the resources attached to the <see cref="GorgonRenderTargetView">GorgonRenderTargetViews</see> must be the same.
		/// </para>
		/// <para>
		/// The format for the view may differ from the formats of other views in this list.
		/// </para>
		/// <para>
		/// <note type="information">
		/// <para>
		/// The exceptions raised when validating a view against other views in this list are only thrown when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public GorgonDepthStencilView DepthStencilView
		{
			get
			{
				return _depthStencilView;
			}
			set
			{
				if (_depthStencilView == value)
				{
					return;
				}

#if DEBUG
				GorgonRenderTargetView firstTarget = this.FirstOrDefault(item => item != null);
				ValidateDepthStencilView(value, firstTarget);
#endif

				_depthStencilView = value;
			}
		}
		#endregion

		#region Methods.
#if DEBUG
		/// <summary>
		/// Function to validate the depth/stencil view.
		/// </summary>
		/// <param name="view">The depth/stencil view to evaluate.</param>
		/// <param name="firstTarget">The first non-null target.</param>
		private static void ValidateDepthStencilView(GorgonDepthStencilView view, GorgonRenderTargetView firstTarget)
		{
			if ((firstTarget == null)
			    || (view == null))
			{
				return;
			}

			// Ensure all resources are the same type.
			if (view.Texture.ResourceType != firstTarget.Texture.ResourceType)
			{
				throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_TYPE_MISMATCH, view.Texture.ResourceType));
			}

			// Ensure the depth stencil array/depth counts match for all resources.
			if (view.ArrayCount != firstTarget.ArrayOrDepthCount)
			{
				throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_ARRAYCOUNT_MISMATCH, view.Texture.Name));
			}

			// Check to ensure that multisample info matches.
			if (!view.Texture.Info.MultisampleInfo.Equals(firstTarget.Texture.Info.MultisampleInfo))
			{
				throw new GorgonException(GorgonResult.CannotBind,
				                          string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_MULTISAMPLE_MISMATCH,
				                                        view.Texture.Info.MultisampleInfo.Quality,
				                                        view.Texture.Info.MultisampleInfo.Count));
			}

			if ((view.Texture.Info.Width != firstTarget.Texture.Info.Width)
			    || (view.Texture.Info.Height != firstTarget.Texture.Info.Height)
			    || ((view.Texture.Info.TextureType != TextureType.Texture3D) && (view.Texture.Info.ArrayCount != firstTarget.Texture.Info.ArrayCount))
			    || ((view.Texture.Info.TextureType == TextureType.Texture3D) && (view.Texture.Info.Depth != firstTarget.Texture.Info.Depth)))
			{
				throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_RESOURCE_MISMATCH);
			}
		}
#endif

#if DEBUG
		/// <summary>
		/// Function to validate an item being assigned to a slot.
		/// </summary>
		/// <param name="item">The item to validate.</param>
		/// <param name="index">The index of the slot being assigned.</param>
		protected override void OnValidate(GorgonRenderTargetView item, int index)
		{
			if (item == null)
			{
				return;
			}

			GorgonRenderTargetView startView = this.FirstOrDefault(target => target != null);

			// If no other targets are assigned, then check the depth stencil and leave.
			if (startView == null)
			{
				if (DepthStencilView != null)
				{
					ValidateDepthStencilView(DepthStencilView, item);
				}
				return;
			}

			// Only check if we have more than 1 render target view being applied.
			for (int i = 0; i < Count; i++)
			{
				GorgonRenderTargetView other = this[i];

				if (other == null)
				{
					continue;
				}

				if ((other == item) && (i != index))
				{
					throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_RTV_ALREADY_BOUND, other.Texture.Name));
				}

				if (other.Texture.ResourceType != item.Texture.ResourceType)
				{
					throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_RTV_NOT_SAME_TYPE, other.Texture.Name));
				}

				if (!startView.Texture.Info.MultisampleInfo.Equals(other.Texture.Info.MultisampleInfo))
				{
					throw new GorgonException(GorgonResult.CannotBind,
											  string.Format(Resources.GORGFX_ERR_RTV_MULTISAMPLE_MISMATCH,
															other.Texture.Info.MultisampleInfo.Quality,
															other.Texture.Info.MultisampleInfo.Count));
				}

				if ((other.Texture.Info.TextureType != TextureType.Texture3D && startView.Texture.Info.ArrayCount != other.Texture.Info.ArrayCount)
					|| ((other.Texture.Info.TextureType == TextureType.Texture3D && startView.Texture.Info.Depth != other.Texture.Info.Depth))
					|| (other.Texture.Info.Width != startView.Texture.Info.Width)
					|| (other.Texture.Info.Height != startView.Texture.Info.Height))
				{
					throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_RESOURCE_MISMATCH);
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
			Array.Clear(_native.Bindings, 0, _native.Count);
			DepthStencilView = null;

			unchecked
			{
				_dirty = 0xff;
			}
		}

		/// <summary>
		/// Function called when an item is assigned to a slot in the binding list.
		/// </summary>
		/// <param name="index">The index of the slot being assigned.</param>
		/// <param name="item">The item being assigned.</param>
		protected override void OnSetItem(int index, GorgonRenderTargetView item)
		{
			_dirty |= 1 << index;
		}

		/// <summary>
		/// Function to retrieve the native bindings.
		/// </summary>
		/// <returns>A native bindings item.</returns>
		internal ref NativeBinding<D3D11.RenderTargetView> GetNativeBindings()
		{
			// Nothing's been changed, so send back our array.
			if (_dirty == 0)
			{
				return ref _native;
			}

			int firstSlot = -1;
			int slotCount = 0;

			D3D11.RenderTargetView[] rtvs = _native.Bindings;

			for (int i = 0; i < MaximumRenderTargetCount; ++i)
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

				rtvs[i] = this[i]?.D3DRenderTargetView;

				// Remove this bit.
				_dirty &= ~dirtyMask;

				++slotCount;

				if (_dirty == 0)
				{
					break;
				}
			}

			_native = new NativeBinding<D3D11.RenderTargetView>
			          {
				          Bindings = rtvs,
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
		public void CopyFrom(GorgonRenderTargetViews source)
		{
			int dirty = 0;

			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			ref NativeBinding<D3D11.RenderTargetView> bindings = ref source.GetNativeBindings();

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

			DepthStencilView = source.DepthStencilView;

			// Update dirty flags.
			_dirty = dirty;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTargetViews"/> class.
		/// </summary>
		public GorgonRenderTargetViews()
			: base(MaximumRenderTargetCount, MaximumRenderTargetCount)
		{
		}
		#endregion
	}
}
