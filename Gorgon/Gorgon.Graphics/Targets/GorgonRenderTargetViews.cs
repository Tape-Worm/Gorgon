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
using System.Diagnostics;
using System.Linq;
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A list of render target states to apply to the pipeline.
	/// </summary>
	public class GorgonRenderTargetViews
		: IList<GorgonRenderTargetView>, IReadOnlyList<GorgonRenderTargetView>
	{
		#region Variables.
		// Render target views to apply to the pipeline.
		private readonly GorgonRenderTargetView[] _views = new GorgonRenderTargetView[8];
		// Actual direct 3D render target views to bind.
		private readonly D3D11.RenderTargetView[] _actualViews = new D3D11.RenderTargetView[8];
		// The currently active depth stencil view.
		private GorgonDepthStencilView _depthStencilView;
		// The number of slots bound.
		private int _bindCount;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of actual Direct 3D 11 views.
		/// </summary>
		internal D3D11.RenderTargetView[] D3DRenderTargetViews => _actualViews;

		/// <summary>
		/// Property to return the number of render target views actually bound.
		/// </summary>
		internal int D3DRenderTargetViewBindCount => _bindCount;

		/// <summary>
		/// Property to return whether there are any target or depth stencil views set in this list.
		/// </summary>
		public bool IsEmpty => _actualViews.Length == 0 && _depthStencilView == null;

		/// <summary>Gets or sets the element at the specified index.</summary>
		/// <returns>The element at the specified index.</returns>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
		public GorgonRenderTargetView this[int index]
		{
			get
			{
				return _views[index];
			}
			set
			{
#if DEBUG
				ValidateRenderTargetDepthStencilViews(value);
#endif

				_views[index] = value;
				_actualViews[index] = value?.D3DRenderTargetView;

				// Update the last slot that was bound.
				for (int i = 0; i < _views.Length; ++i)
				{
					if (_views[i] != null)
					{
						_bindCount = i + 1;
					}
				}
			}
		}

		/// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		public int Count => _views.Length;

		/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
		bool ICollection<GorgonRenderTargetView>.IsReadOnly => false;

		/// <summary>
		/// Property to set or return the currently active depth/stencil view.
		/// </summary>
		public GorgonDepthStencilView DepthStencilView
		{
			get
			{
				return _depthStencilView;
			}
			set
			{
#if DEBUG
				GorgonRenderTargetView firstTarget = this.FirstOrDefault(item => item != null);
				ValidateDepthStencilView(value, firstTarget);
#endif

				_depthStencilView = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to validate the depth/stencil view.
		/// </summary>
		/// <param name="view">The depth/stencil view to evaluate.</param>
		/// <param name="firstTarget">The first non-null target.</param>
		private static void ValidateDepthStencilView(GorgonDepthStencilView view, GorgonRenderTargetView firstTarget)
		{
#if DEBUG
			if ((firstTarget == null)
				|| (view == null))
			{
				return;
			}

			var targetTexture = firstTarget.Resource as GorgonTexture;

			Debug.Assert(targetTexture != null, "Render target view not bound to a texture.");

			// Ensure all resources are the same type.
			if (view.Resource.ResourceType != firstTarget.Resource.ResourceType)
			{
				throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_TYPE_MISMATCH, view.Resource.ResourceType));
			}

			// Ensure the depth stencil array/depth counts match for all resources.
			if (view.ArrayCount != firstTarget.ArrayOrDepthCount)
			{
				throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_ARRAYCOUNT_MISMATCH, view.Resource.Name));
			}

			var dsTexture = view.Resource as GorgonTexture;

			Debug.Assert(dsTexture != null, "Depth/stencil view not bound to a texture.");

			// Check to ensure that multisample info matches.
			if (dsTexture.Info.MultiSampleInfo.Equals(targetTexture.Info.MultiSampleInfo))
			{
				throw new GorgonException(GorgonResult.CannotBind,
					string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_MULTISAMPLE_MISMATCH, dsTexture.Info.MultiSampleInfo.Quality, dsTexture.Info.MultiSampleInfo.Count));
			}

			if ((dsTexture.Info.Width != targetTexture.Info.Width)
				|| (dsTexture.Info.Height != targetTexture.Info.Height)
				|| ((dsTexture.Info.TextureType != TextureType.Texture3D) && (dsTexture.Info.ArrayCount != targetTexture.Info.ArrayCount))
				|| ((dsTexture.Info.TextureType == TextureType.Texture3D) && (dsTexture.Info.Depth != targetTexture.Info.Depth)))
			{
				throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_RESOURCE_MISMATCH);
			}
#endif
		}

		/// <summary>
		/// Function to validate the render target views and depth/stencil view being assigned.
		/// </summary>
		/// <param name="target">The target view to evaluate.</param>
		private void ValidateRenderTargetDepthStencilViews(GorgonRenderTargetView target)
		{
#if DEBUG
			if (target == null)
			{
				return;
			}
			
			GorgonRenderTargetView startView = this.FirstOrDefault(item => item != null);

			// If no other targets are assigned, then check the depth stencil and leave.
			if (startView == null)
			{
				if (DepthStencilView != null)
				{
					ValidateDepthStencilView(DepthStencilView, target);
				}
				return;
			}

			// If we don't have a render target view, we don't need to check anything, even if we have a depth/stencil.
			// Begin checking resource data.
			var rtvTexture = target.Resource as GorgonTexture;

			Debug.Assert(rtvTexture != null, "Render target view resource is not a texture.");

			// Only check if we have more than 1 render target view being applied.
			foreach (GorgonRenderTargetView other in _views.Where(item => item != null))
			{
				if (other == target)
				{
					throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_RTV_ALREADY_BOUND, other.Resource.Name));
				}

				if (other.Resource.ResourceType != target.Resource.ResourceType)
				{
					throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_RTV_NOT_SAME_TYPE, other.Resource.Name));
				}

				var texture = other.Resource as GorgonTexture;

				Debug.Assert(texture != null, $"Render target view '{other.Resource.Name}' resource is not a texture.");

				if (!texture.Info.MultiSampleInfo.Equals(rtvTexture.Info.MultiSampleInfo))
				{
					throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_RTV_MULTISAMPLE_MISMATCH,
															  rtvTexture.Info.MultiSampleInfo.Quality,
															  rtvTexture.Info.MultiSampleInfo.Count));
				}

				if ((rtvTexture.Info.TextureType != TextureType.Texture3D && texture.Info.ArrayCount != rtvTexture.Info.ArrayCount)
				    || ((rtvTexture.Info.TextureType == TextureType.Texture2D && texture.Info.Depth != rtvTexture.Info.Depth))
				    || (rtvTexture.Info.Width != texture.Info.Width)
				    || (rtvTexture.Info.Height != texture.Info.Height))
				{
					throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_RESOURCE_MISMATCH);
				}
			}
#endif
		}

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="NotSupportedException">This method is not supported by this type.</exception>
		void ICollection<GorgonRenderTargetView>.Add(GorgonRenderTargetView item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
		/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		/// <exception cref="NotSupportedException">This method is not supported by this type.</exception>
		void IList<GorgonRenderTargetView>.Insert(int index, GorgonRenderTargetView item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		/// <exception cref="NotSupportedException">This method is not supported by this type.</exception>
		bool ICollection<GorgonRenderTargetView>.Remove(GorgonRenderTargetView item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="NotSupportedException">This method is not supported by this type.</exception>
		void IList<GorgonRenderTargetView>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _views.GetEnumerator();
		}

		/// <summary>
		/// Function to unbind all the render target views and depth/stencil views.
		/// </summary>
		public void Clear()
		{
			for (int i = 0; i < _views.Length; ++i)
			{
				_views[i] = null;
				_actualViews[i] = null;
			}

			DepthStencilView = null;
		}

		/// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
		/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		public bool Contains(GorgonRenderTargetView item)
		{
			return Array.IndexOf(_views, item) != -1;
		}

		/// <summary>Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.</summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex" /> is less than 0.</exception>
		/// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
		public void CopyTo(GorgonRenderTargetView[] array, int arrayIndex)
		{
			_views.CopyTo(array, arrayIndex);
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		/// <filterpriority>1</filterpriority>
		public IEnumerator<GorgonRenderTargetView> GetEnumerator()
		{
			foreach (GorgonRenderTargetView view in _views)
			{
				yield return view;
			}
		}

		/// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.</summary>
		/// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		public int IndexOf(GorgonRenderTargetView item)
		{
			return Array.IndexOf(_views, item);
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTargetViews"/> class.
		/// </summary>
		public GorgonRenderTargetViews()
		{
			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTargetViews"/> class.
		/// </summary>
		/// <param name="renderTargetViews">The render target views to assign.</param>
		/// <param name="depthView">The depth/stencil view to assign.</param>
		public GorgonRenderTargetViews(IEnumerable<GorgonRenderTargetView> renderTargetViews, GorgonDepthStencilView depthView = null)
		{
			if (renderTargetViews != null)
			{
				int index = 0;

				foreach (GorgonRenderTargetView view in renderTargetViews)
				{
					if (index > 7)
					{
						break;
					}

					this[index++] = view;
				}
			}

			DepthStencilView = depthView;
		}
		#endregion
	}
}
