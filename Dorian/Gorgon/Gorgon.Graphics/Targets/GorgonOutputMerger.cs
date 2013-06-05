#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Tuesday, January 31, 2012 12:38:41 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using GorgonLibrary.Math;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics.Properties;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Manages the display of the graphics data.
	/// </summary>
	public sealed class GorgonOutputMerger
	{
        /// <summary>
        /// A list of render targets bound to the pipeline.
        /// </summary>
        /// <remarks>This list contains 8 slots for SM4 and better devices.  For SM2_a_b devices only 4 slots are available.</remarks>
        public sealed class RenderTargetList
            : ICollection<GorgonRenderTargetView>
        {
            #region Variables.
            private readonly GorgonGraphics _graphics;                          // Containing graphics interface.
			private readonly GorgonRenderTargetView[] _views;                   // List of views.
			private readonly D3D.RenderTargetView[] _D3Dviews;					// List of Direct 3D views.
            private GorgonDepthStencil _depthStencil;							// The current depth/stencil buffer.
            #endregion

            #region Properties.
            /// <summary>
            /// Property to set or return the current depth/stencil buffer.
            /// </summary>
            /// <remarks>
            /// This will bind a single depth/stencil buffer for all the render targets.  The depth/stencil buffer must use the same type of resource, and that resource must be a texture 
            /// (i.e. a 1D or 2D texture), and if the targets are multisampled, then the depth buffer must also be multisampled.  The depth/stencil buffer must also have the same number of array 
            /// indices and mip levels as the render target texture and it also must be the same width and height (for 2D textures).  If these conditions are not met, then an exception will be thrown.
            /// </remarks>
            /// <exception cref="GorgonLibrary.GorgonException">Thrown when the depth/stencil buffer could not be bound to the pipeline.</exception>
            public GorgonDepthStencil DepthStencilBuffer
            {
                get
                {
                    return _depthStencil;
                }
                set
                {
#if DEBUG
                    ValidateDepthBuffer(value);
#endif
                    _depthStencil = value;
                    BindResources();
                }
            }
            #endregion

            #region Methods.
#if DEBUG
            /// <summary>
            /// Function to validate the depth buffer.
            /// </summary>
            /// <param name="depthStencil">The depth/stencil buffer to validate.</param>
            private void ValidateDepthBuffer(GorgonDepthStencil depthStencil)
            {
                if (depthStencil == null)
                {
                    return;
                }

                foreach (var view in _views.Where(item => item != null))
                {
                    if ((view.Resource.ResourceType != ResourceType.Texture1D)
                        && (view.Resource.ResourceType != ResourceType.Texture2D))
                    {
                        throw new GorgonException(GorgonResult.CannotBind,
                                                  string.Format(Resources.GORGFX_RTV_DEPTH_RT_TYPE_INVALID,
                                                      view.Resource.ResourceType));
                    }

                    if ((view.Resource.ResourceType != depthStencil.Texture.ResourceType))
                    {
                        throw new GorgonException(GorgonResult.CannotBind,
                                                  string.Format(Resources.GORGFX_RTV_DEPTH_RESOURCE_TYPE_INVALID,
                                                      view.Resource.ResourceType,
                                                      depthStencil.Texture.ResourceType));
                    }

                    var resTexture = view.Resource as GorgonTexture;

                    if (resTexture == null)
                    {
                        continue;
                    }

                    if (depthStencil.Texture.Settings.ArrayCount != resTexture.Settings.ArrayCount)
                    {
                        throw new GorgonException(GorgonResult.CannotBind,
                                                  string.Format(Resources.GORGFX_RTV_DEPTH_ARRAYCOUNT_MISMATCH,
                                                      resTexture.Name));
                    }

                    if (depthStencil.Texture.Settings.MipCount != resTexture.Settings.MipCount)
                    {
                        throw new GorgonException(GorgonResult.CannotBind,
                                                  string.Format(Resources.GORGFX_RTV_DEPTH_MIPCOUNT_MISMATCH,
                                                      resTexture.Name));
                    }

                    if ((depthStencil.Settings.Multisampling.Count != resTexture.Settings.Multisampling.Count) 
                        || (depthStencil.Settings.Multisampling.Quality != resTexture.Settings.Multisampling.Quality))
                    {
                        throw new GorgonException(GorgonResult.CannotBind,
                                                  string.Format(Resources.GORGFX_RTV_DEPTH_MULTISAMPLE_MISMATCH,
                                                      resTexture.Name, resTexture.Settings.Multisampling.Count, resTexture.Settings.Multisampling.Quality,
                                                      depthStencil.Texture.Settings.Multisampling.Count, depthStencil.Texture.Settings.Multisampling.Quality));
                    }
                }
            }

            /// <summary>
            /// Function to validate a render target.
            /// </summary>
            /// <param name="view">View to validate.</param>
            /// <param name="slot">Slot being bound.</param>
            private void ValidateRenderTarget(GorgonRenderTargetView view, int slot)
            {
                if (view == null)
                {
                    return;
                }

                var viewTexture = view.Resource as GorgonTexture;
                var viewBuffer = view.Resource as GorgonBuffer;

                // This should never happen.
                if ((viewTexture == null) && (viewBuffer == null))
                {
                    // Don't bother to localize this guy, this is for developers.  It'll happen if we modify the render target type but don't
                    // handle related code.
                    throw new GorgonException(GorgonResult.CannotBind, "View is bound to an unknown resource type.  That shouldn't happen.");
                }

                for (int i = 0; i < _views.Length; i++)
                {
                    var target = _views[i];

                    if (target == null)
                    {
                        continue;
                    }

                    if ((view == target) && (slot != i))
                    {
                        throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_RTV_ALREADY_BOUND, view.Resource.Name, i));
                    }

                    if ((_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) 
                        && (target.Format != view.Format) 
                        && (GorgonBufferFormatInfo.GetInfo(target.Format).BitDepth != GorgonBufferFormatInfo.GetInfo(view.Format).BitDepth))
                    {
                        throw new GorgonException(GorgonResult.CannotBind,string.Format(Resources.GORGFX_RTV_BIT_DEPTH_MISMATCH,
                                                      view.Resource.Name));
                    }

                    if (target.Resource.ResourceType != view.Resource.ResourceType)
                    {
                        throw new GorgonException(GorgonResult.CannotBind,
                                                  string.Format(Resources.GORGFX_RTV_RESOURCE_TYPE_MISMATCH,
                                                      view.Resource.Name, target.Resource.ResourceType, view.Resource.ResourceType));
                    }

                    // Check for texture specific constraints.
                    if (viewTexture != null)
                    {
                        var targetTexture = (GorgonTexture)target.Resource;

                        if (targetTexture.Settings.ArrayCount != viewTexture.Settings.ArrayCount)
                        {
                            throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_RTV_ARRAY_COUNT_MISMATCH,
                                                            view.Resource.Name, viewTexture.Settings.ArrayCount, targetTexture.Settings.ArrayCount));
                        }

                        if (targetTexture.Settings.MipCount != viewTexture.Settings.MipCount)
                        {
                            throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_RTV_MIP_COUNT_MISMATCH,
                                                            view.Resource.Name, viewTexture.Settings.MipCount, targetTexture.Settings.MipCount));
                        }

                        if ((targetTexture.Settings.Width != viewTexture.Settings.Width)
                            || (targetTexture.Settings.Height != viewTexture.Settings.Height)
                            || (targetTexture.Settings.Depth != viewTexture.Settings.Depth))
                        {
                            throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_RTV_DIMENSIONS_MISMATCH,
                                                        view.Resource.Name, viewTexture.Settings.Width, viewTexture.Settings.Height, viewTexture.Settings.Depth,
                                                        targetTexture.Settings.Width, targetTexture.Settings.Height, targetTexture.Settings.Depth));
                        }

                        if ((targetTexture.Settings.Multisampling.Count != viewTexture.Settings.Multisampling.Count)
                            || (targetTexture.Settings.Multisampling.Quality != viewTexture.Settings.Multisampling.Quality))
                        {
                            throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_RTV_MULTISAMPLE_MISMATCH,
                                                          view.Resource.Name, viewTexture.Settings.Multisampling.Count, viewTexture.Settings.Multisampling.Quality,
                                                          targetTexture.Settings.Multisampling.Count, targetTexture.Settings.Multisampling.Quality));
                        }
                    }

                    if (viewBuffer == null)
                    {
                        continue;
                    }

                    var targetBuffer = (GorgonBuffer)target.Resource;

                    if (targetBuffer.Settings.SizeInBytes != viewBuffer.Settings.SizeInBytes)
                    {
                        throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_RTV_BUFFER_SIZE_MISMATCH,
                            view.Resource.Name, viewBuffer.SizeInBytes, targetBuffer.SizeInBytes));
                    }
                }
            }
#endif
            /// <summary>
            /// Function to perform the binding of the targets.
            /// </summary>
            private void BindResources()
            {
                D3D.DepthStencilView depthView = _depthStencil == null ? null : _depthStencil.D3DDepthStencilView;

                _graphics.Context.OutputMerger.SetTargets(depthView, _D3Dviews);
            }

            /// <summary>
            /// Function re-seat a render target.
            /// </summary>
            /// <param name="target">Target to reseat.</param>
            internal void Reseat(GorgonResource target)
            {
                var views = _views.Where(item => item != null && item.Resource == target);

                foreach (var view in views)
                {
                    int index = IndexOf(view);

                    if (index == -1)
                    {
                        continue;
                    }

                    SetView(index, null);
                    SetView(index, view);
                }
            }

			/// <summary>
			/// Function to unbind a render target view from the pipeline.
			/// </summary>
			/// <param name="view">View to unbind.</param>
			internal void Unbind(GorgonRenderTargetView view)
			{
				int index = IndexOf(view);

				if (index == -1)
				{
					return;
				}

				SetView(index, null);
			}

            /// <summary>
            /// Function to unbind a render target.
            /// </summary>
            /// <param name="target">Target to unbind.</param>
            internal void UnbindResource(GorgonResource target)
            {
                var indices =
                    _views.Where(item => item != null && item.Resource == target)
                          .Select(IndexOf)
                          .Where(item => item != -1);

                foreach (int index in indices)
                {
                    SetView(index, null);
                }
            }

            /// <summary>
            /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"></see>.
            /// </summary>
            /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
            /// <returns>
            /// The index of item if found in the list; otherwise, -1.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="item"/> parameter is NULL (Nothing in VB.Net).</exception>
            public int IndexOf(GorgonRenderTargetView item)
            {
                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }

                return Array.IndexOf(_views, item);
            }

            /// <summary>
            /// Function to determine the index of a view with a resource that has the specified name.
            /// </summary>
            /// <param name="name">Name of the resource to find.</param>
            /// <returns>The index of the view, or -1 if not found.</returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
            /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
            public int IndexOf(string name)
            {
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "name");
                }

                for (int i = 0; i < _views.Length; i++)
                {
                    if ((_views[i] != null) 
                        && (string.Compare(_views[i].Resource.Name, name, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        return i;
                    }
                }

                return -1;
            }

            /// <summary>
            /// Function to determine the index of a view that is attached to the specific render target view.
            /// </summary>
            /// <param name="target">Target to look up.</param>
            /// <returns>The index of the view, or -1 if not found.</returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="target"/> parameter is NULL (Nothing in VB.Net).</exception>
            public int IndexOf(GorgonResource target)
            {
                if (target == null)
                {
                    throw new ArgumentNullException("target");
                }

                for (int i = 0; i < _views.Length; i++)
                {
                    if ((_views[i] != null) && (_views[i].Resource == target))
                    {
                        return i;
                    }
                }

                return -1;
            }

            /// <summary>
            /// Function to return whether a resource with the specified name has a view bound to the pipeline.
            /// </summary>
            /// <param name="name">Name of the resource.</param>
            /// <returns>TRUE if found, FALSE if not.</returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
            /// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
            public bool Contains(string name)
            {
                return IndexOf(name) > -1;
            }
			
            /// <summary>
            /// Function to return whether a render target has a view that is bound to the pipeline.
            /// </summary>
            /// <param name="target">Target to look up.</param>
            /// <returns>TRUE if found, FALSE if not.</returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="target"/> parameter is NULL (Nothing in VB.Net).</exception>
            public bool Contains(GorgonResource target)
            {
                return IndexOf(target) > -1;
            }

            /// <summary>
            /// Function to set a render target view into a slot.
            /// </summary>
            /// <param name="slot">Slot index to set.</param>
            /// <param name="view">View to set.</param>
            public void SetView(int slot, GorgonRenderTargetView view)
            {
                GorgonDebug.AssertParamRange(slot, 0, _views.Length, true, false, "startIndex");

                // Don't set the same target.
                if (view == _views[slot])
                {
                    return;
                }

#if DEBUG
                ValidateRenderTarget(view, slot);
#endif

                _views[slot] = view;
                _D3Dviews[slot] = view == null ? null : view.D3DView;
                BindResources();
            }

            /// <summary>
            /// Function to return a render target view from the specified slot.
            /// </summary>
            /// <param name="slot">Slot containing the view to return.</param>
            /// <returns>The view in the specified slot, or NULL (Nothing in VB.Net) if there was not view bound to the slot.</returns>
            /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> parameter is less than 0 or not less than the number of slots available.</exception>
            public GorgonRenderTargetView GetView(int slot)
            {
                if ((slot < 0)
                    || (slot >= _views.Length))
                {
                    throw new ArgumentOutOfRangeException("slot");
                }

                return _views[slot];
            }

            /// <summary>
            /// Function to bind a series of render target views to the pipeline.
            /// </summary>
            /// <param name="slot">Slot to start binding at.</param>
            /// <param name="views">The views to bind.</param>
            /// <remarks>Use this to set a series of render target views all at the same time.  If the <paramref name="views"/> parameter is NULL (Nothing in VB.Net) then all render targets and 
            /// the depth/stencil buffer will be unbound from the pipeline.</remarks>
            /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> parameter is less than 0 or not less than the number of available slots.</exception>
            /// <exception cref="GorgonLibrary.GorgonException">Thrown when one or all of the views in the <paramref name="views"/> parameter can't be bound.</exception>
            public void SetRange(int slot, GorgonRenderTargetView[] views)
            {
                bool hasChanges = false;
                int count = _views.Length - slot;

                GorgonDebug.AssertParamRange(slot, 0, _views.Length, true, false, "startIndex");

                if (views != null)
                {
                    count = views.Length.Min(_views.Length);
                }
                else
                {
                    // If we're setting nothing, then unbind the depth/stencil as well.
                    _depthStencil = null;
                }

                for (int i = 0; i < count; i++)
                {
                    GorgonRenderTargetView target = null;
                    int targetIndex = i + slot;

                    if (views != null)
                    {
                        target = views[i];
                    }

                    if (target == _views[targetIndex])
                    {
                        continue;
                    }

#if DEBUG
                    ValidateRenderTarget(target, i);
#endif

                    hasChanges = true;
                    _views[targetIndex] = target;
                    // Unlike the other set functions that take arrays, this one is an all-or-nothing approach.  So, targets that aren't set in the array
                    // would be set to NULL (D3D automatically does this).  This is an annoying "feature" of D3D11 and makes automation of some things (like 
                    // auto-setting the render target after swap chain is resized) nearly impossible without unsetting every other target bound.  I'm sure 
                    // there's a performance penalty for this, but it's likely negligable and worth it for the trade off in convenience.
                    _D3Dviews[targetIndex] = (target == null ? null : target.D3DView);
                }

                if (hasChanges)
                {
                    BindResources();
                }
            }

            // TODO: Provide overloads for other render target types (1D/3D/Buffer).
            /// <summary>
            /// Function to bind the render target to the specified slot.
            /// </summary>
            /// <param name="slot">Slot to bind the render target with.</param>
            /// <param name="target">Target to bind.</param>
            /// <param name="depthStencilBuffer">[Optional] An alternative depth/stencil buffer to bind.</param>
            /// <remarks>Use this to directly set a render target to the pipeline at the specified slot.
            /// <para>The <paramref name="depthStencilBuffer"/> is an optional alternative buffer to set instead of the buffer that's attached to the render target.  If this value is NULL (Nothing in VB.Net) 
            /// then the depth/stencil buffer that's attached to the render target (if it exists) will be used instead.</para>
            /// </remarks>
            /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> parameter is less than 0 or not less than the number of available slots.</exception>
            /// <exception cref="GorgonLibrary.GorgonException">Thrown when render target in the <paramref name="target"/> parameter can't be bound.
            /// <para>-or-</para>
            /// <para>Thrown when the <paramref name="depthStencilBuffer"/> (or the attached depth/stencil on the render target) can't be bound.</para>
            /// </exception>
            public void SetRenderTarget(int slot, GorgonRenderTarget2D target, GorgonDepthStencil depthStencilBuffer = null)
            {
                if ((depthStencilBuffer == null) && (target != null))
                {
                    depthStencilBuffer = target.DepthStencilBuffer;
                }

                _depthStencil = depthStencilBuffer;
				SetView(slot, target);
            }

            /// <summary>
            /// Function to retrieve the specified render target resource from the view bound to the specified slot.
            /// </summary>
            /// <typeparam name="TR">Type of resource to look up.</typeparam>
            /// <param name="slot">Slot containing the render target resource.</param>
            /// <returns>The resource attached to the view at the specified slot, or NULL (Nothing in VB.Net) if there wasn't a view bound at that slot.</returns>
            /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> parameter is less than 0 or not less than the number of available slots.</exception>
            /// <exception cref="System.InvalidCastException">Thrown when the resource is not the type requested.</exception>
            public TR GetResource<TR>(int slot)
                where TR : GorgonResource
            {
                GorgonDebug.AssertParamRange(slot, 0, _views.Length, "index");

                var view = _views[slot];

#if DEBUG
                if ((view != null) && (view.Resource != null) && (!(view.Resource is TR)))
                {
                    throw new InvalidCastException(string.Format(Resources.GORGFX_VIEW_RESOURCE_NOT_TYPE, slot,
                                                                 typeof(TR).FullName));
                }
#endif

                if (view == null)
                {
                    return null;
                }

                return (TR)view.Resource;                
            }
            #endregion

            #region Constructor/Destructor.
            /// <summary>
            /// Initializes a new instance of the <see cref="RenderTargetList"/> class.
            /// </summary>
            /// <param name="graphics">The graphics interface that owns this interface.</param>
            internal RenderTargetList(GorgonGraphics graphics)
            {
                _views = new GorgonRenderTargetView[graphics.VideoDevice.SupportedFeatureLevel != DeviceFeatureLevel.SM2_a_b ? 8 : 4];
                _D3Dviews = new D3D.RenderTargetView[_views.Length];
                _graphics = graphics;
            }
            #endregion

            #region ICollection<GorgonRenderTargetView> Members
            #region Properties.
            /// <summary>
            /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
            public int Count
            {
                get
                {
                    return _views.Length;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
            /// </summary>
            /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }
            #endregion

            #region Methods.
            /// <summary>
            /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
            /// <returns>
            /// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </returns>
            /// <exception cref="System.NotSupportedException">This method is not supported.</exception>
            bool ICollection<GorgonRenderTargetView>.Remove(GorgonRenderTargetView item)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
            /// <exception cref="System.NotSupportedException">This method is not supported.</exception>
            void ICollection<GorgonRenderTargetView>.Add(GorgonRenderTargetView item)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
            /// </summary>
            /// <exception cref="System.NotSupportedException">This method is not supported.</exception>
            void ICollection<GorgonRenderTargetView>.Clear()
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
            /// </summary>
            /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
            /// <returns>
            /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="item"/> parameter is NULL (Nothing in VB.Net).</exception>
            public bool Contains(GorgonRenderTargetView item)
            {
                return IndexOf(item) > -1;
            }

            /// <summary>
            /// Function to copy the contents of this list to an array.
            /// </summary>
            /// <param name="array">Array to copy into.</param>
            /// <param name="arrayIndex">Index in the array to start writing at.</param>
            /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="array"/> parameter is NULL (Nothing in VB.Net).</exception>
            /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="arrayIndex"/> parameter is less than 0 or not less than the length of the array.</exception>
            public void CopyTo(GorgonRenderTargetView[] array, int arrayIndex)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }

                if ((arrayIndex < 0) || (arrayIndex >= array.Length))
                {
                    throw new ArgumentOutOfRangeException("arrayIndex");
                }

                int count = (array.Length - arrayIndex).Min(_views.Length);

                for (int i = 0; i < count; i++)
                {
                    array[i + arrayIndex] = _views[i];
                }
            }
            #endregion
            #endregion

            #region IEnumerable<GorgonRenderTargetView> Members
            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
            /// </returns>
            public IEnumerator<GorgonRenderTargetView> GetEnumerator()
            {
                for (int i = 0; i < _views.Length; i++)
                {
                    yield return _views[i];
                }
            }

            #endregion

            #region IEnumerable Members
            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
            /// </returns>
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _views.GetEnumerator();
            }
            #endregion
        }

		#region Variables.
		private readonly GorgonGraphics _graphics;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the blending render state interface.
		/// </summary>
		public GorgonBlendRenderState BlendingState
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the depth/stencil render state interface.
		/// </summary>
		public GorgonDepthStencilRenderState DepthStencilState
		{
			get;
			private set;
		}

        /// <summary>
        /// Property to returnt he render target bindings.
        /// </summary>
        public RenderTargetList RenderTargets
        {
            get;
            private set;
        }
		#endregion

		#region Methods.
	    /// <summary>
	    /// Function to validate the settings for a render target.
	    /// </summary>
	    private void ValidateRenderTargetSettings(IRenderTargetTextureSettings settings)
        {
            if (settings.RenderTargetType == RenderTargetType.Buffer)
            {
                throw new GorgonException(GorgonResult.CannotCreate, "Cannot use buffer settings for 1D, 2D or 3D render targets.");    
            }

            if (_graphics.VideoDevice == null)
            {
                throw new GorgonException(GorgonResult.CannotCreate,
                                          "Cannot create the render target, no video device was selected.");
            }

            // Ensure the dimensions are valid.
            if ((settings.Width <= 0)
                || (settings.Width >= _graphics.Textures.MaxWidth))
            {
                throw new GorgonException(GorgonResult.CannotCreate,
                                          "Render target must have a width greater than 0 or less than "
                                          + _graphics.Textures.MaxWidth + ".");
            }
            
            if ((settings.RenderTargetType > RenderTargetType.Target1D) && ((settings.Height <= 0)
                || (settings.Height >= _graphics.Textures.MaxHeight)))
            {
                throw new GorgonException(GorgonResult.CannotCreate,
                                          "Render target must have a height greater than 0 or less than "
                                          + _graphics.Textures.MaxHeight + ".");
            }

            if ((settings.RenderTargetType > RenderTargetType.Target3D) && ((settings.Depth <= 0)
                || (settings.Depth >= _graphics.Textures.MaxDepth)))
            {
                throw new GorgonException(GorgonResult.CannotCreate,
                                          "Render target must have a depth greater than 0 or less than "
                                          + _graphics.Textures.MaxDepth + ".");
            }

            if (settings.Format == BufferFormat.Unknown)
            {
                throw new GorgonException(GorgonResult.CannotCreate, "Render target must have a known buffer format.");
            }

            int quality = _graphics.VideoDevice.GetMultiSampleQuality(settings.Format, settings.Multisampling.Count);

            // Ensure that the quality of the sampling does not exceed what the card can do.
            if ((settings.Multisampling.Quality >= quality)
                || (settings.Multisampling.Quality < 0))
            {
                throw new ArgumentException("Video device '" + _graphics.VideoDevice.Name
                                            + "' does not support multisampling with a count of '"
                                            + settings.Multisampling.Count + "' and a quality of '"
                                            + settings.Multisampling.Quality + " with a format of '"
                                            + settings.Format + "'");
            }

            // Ensure that the selected video format can be used.
            if (!_graphics.VideoDevice.SupportsRenderTargetFormat(settings.Format,
                                                                 (settings.Multisampling.Quality > 0)
                                                                 || (settings.Multisampling.Count > 1)))
            {
                throw new ArgumentException("Cannot use the format '" + settings.Format.ToString()
                                            + "' for a render target on the video device '" + _graphics.VideoDevice.Name + "'.");
            }
        }

        /// <summary>
		/// Function to clean up resources used by the interface.
		/// </summary>
		internal void CleanUp()
		{
			for (int i = 0; i < RenderTargets.Count; i++)
			{
				RenderTargets.SetView(0, null);
			}

			if (BlendingState != null)
			{
				BlendingState.CleanUp();
			}

			if (DepthStencilState != null)
			{
				DepthStencilState.CleanUp();
			}

			BlendingState = null;
			DepthStencilState = null;
		}

		/// <summary>
		/// Function to draw polygons to the current render target.
		/// </summary>
		/// <param name="vertexStart">Vertex to start at.</param>
		/// <param name="vertexCount">Number of vertices to draw.</param>
		public void Draw(int vertexStart, int vertexCount)
		{
			GorgonRenderStatistics.DrawCallCount++;
			_graphics.Context.Draw(vertexCount, vertexStart);
		}

		/// <summary>
		/// Function to draw geometry of an unknown size.
		/// </summary>
		public void DrawAuto()
		{
#if DEBUG
			if (_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
				throw new GorgonException(GorgonResult.AccessDenied, "SM 2.0 video devices cannot draw auto-generated data.");
#endif
			GorgonRenderStatistics.DrawCallCount++;
			_graphics.Context.DrawAuto();
		}

		/// <summary>
		/// Function to draw indexed polygons.
		/// </summary>
		/// <param name="indexStart">Starting index to use.</param>
		/// <param name="baseVertex">Vertex index added to each index.</param>
		/// <param name="indexCount">Number of indices to use.</param>
		public void DrawIndexed(int indexStart, int baseVertex, int indexCount)
		{
			GorgonRenderStatistics.DrawCallCount++;
			_graphics.Context.DrawIndexed(indexCount, indexStart, baseVertex);
		}

		/// <summary>
		/// Function to draw indexed instanced polygons.
		/// </summary>
		/// <param name="startInstance">A value added to each index.</param>
		/// <param name="indexStart">Starting index to use.</param>
		/// <param name="baseVertex">Vertex index added to each index.</param>
		/// <param name="instanceCount">Number of indices to use.</param>
		/// <param name="indexCount">Number of indices to read per instance.</param>
		public void DrawIndexedInstanced(int startInstance, int indexStart, int baseVertex, int instanceCount, int indexCount)
		{
			GorgonRenderStatistics.DrawCallCount++;
			_graphics.Context.DrawIndexedInstanced(indexCount, instanceCount, indexStart, baseVertex, startInstance);
		}

		/// <summary>
		/// Function to draw instanced polygons.
		/// </summary>
		/// <param name="startInstance">Value added to each index.</param>
		/// <param name="startVertex">Vertex to start at.</param>
		/// <param name="instanceCount">Number of instances to draw.</param>
		/// <param name="vertexCount">Number of vertices to draw.</param>
		public void DrawInstanced(int startInstance, int startVertex, int instanceCount, int vertexCount)
		{
			GorgonRenderStatistics.DrawCallCount++;
			_graphics.Context.DrawInstanced(vertexCount, instanceCount, startVertex, startInstance);
		}

		/// <summary>
		/// Function to draw indexed, instanced GPU generated data.
		/// </summary>
		/// <param name="buffer">Buffer holding the GPU generated data.</param>
		/// <param name="alignedAyteOffset">Number of bytes to start at within the buffer.</param>
		/// <param name="isIndexed">TRUE if the data is indexed, FALSE if not.</param>
		/// <remarks>This method is not supported by SM2_a_b or SM_4.x video devices.</remarks>
		/// <exception cref="System.InvalidOperationException">Thrown if the current video device is a SM2_a_b or SM4_x device.</exception>
		public void DrawInstancedIndirect(GorgonBuffer buffer, int alignedAyteOffset, bool isIndexed)
		{
			GorgonDebug.AssertNull<GorgonBuffer>(buffer, "buffer");

#if DEBUG
			if ((_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) ||
				(_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4) ||
				(_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4_1))
				throw new InvalidOperationException("Cannot call DrawInstancedIndirect without a SM5 or better video device.");
#endif
			GorgonRenderStatistics.DrawCallCount++;
			if (isIndexed)
				_graphics.Context.DrawIndexedInstancedIndirect((D3D.Buffer)buffer.D3DResource, alignedAyteOffset);
			else
				_graphics.Context.DrawInstancedIndirect((D3D.Buffer)buffer.D3DResource, alignedAyteOffset);
		}

		/// <summary>
		/// Function to create a depth/stencil buffer.
		/// </summary>
		/// <param name="name">Name of the depth/stencil buffer.</param>
		/// <param name="settings">Settings to apply to the depth/stencil buffer.</param>
		/// <returns>A new depth/stencil buffer.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonDepthStencilSettings.Format">GorgonDepthStencilSettings.Format</see> property is set to Unknown or is unsupported.</para>
		/// </exception>
		/// <remarks>
		/// A depth buffer may be paired with a swapchain or render target through its DepthStencil property.  When pairing the depth/stencil to the render target, Ensure that the depth/stencil buffer width, height and multisample settings match that of the render target that it is paired with.
		/// <para>The texture for a depth/stencil may be used in a shader for cards that have a feature level of SM_4_1 or better, and can be set to do so by setting the <see cref="P:GorgonLibrary.Graphics.GorgonDepthStencilSettings.TextureFormat">GorgonDepthStencilSettings.TextureFormat</see> property to a typeless format. 
		/// If this is attempted on a video device that has a feature level of SM_4_0 or below, then an exception will be raised.</para>
		/// </remarks>
		public GorgonDepthStencil CreateDepthStencil(string name, GorgonDepthStencilSettings settings)
		{
			GorgonDepthStencil depthBuffer = null;

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The parameter must not be empty.", "name");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

			GorgonDepthStencil.ValidateSettings(_graphics, settings);

			depthBuffer = new GorgonDepthStencil(_graphics, name, settings);
			_graphics.AddTrackedObject(depthBuffer);
			depthBuffer.Initialize();

			return depthBuffer;
		}

		/// <summary>
		/// Function to create a swap chain.
		/// </summary>
		/// <param name="name">Name of the swap chain.</param>
		/// <param name="settings">Settings for the swap chain.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">GorgonSwapChainSettings.Window</see> property is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonSwapChainSettings.VideoMode.Format</see> property cannot be used by the video device for displaying data or for the depth/stencil buffer.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.MultiSamples.Quality</see> property is less than 0 or not less than the value returned by <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice">GorgonVideoDevice.GetMultiSampleQuality</see>.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the current video device is a SM2_a_b video device and the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Flags">Flags</see> property is not equal to RenderTarget.</para>
		/// </exception>
		/// <remarks>This will create our output swap chains for display to a window or control.  All functionality for sending or retrieving data from the video device can be accessed through the swap chain.
		/// <para>Passing default settings for the <see cref="GorgonLibrary.Graphics.GorgonSwapChainSettings">settings parameters</see> will make Gorgon choose the closest possible settings appropriate for the video device and output that the window is on.  For example, passing NULL (Nothing in VB.Net) to 
		/// the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.VideoMode">GorgonSwapChainSettings.VideoMode</see> parameter will make Gorgon find the closest video mode available to the current window size and desktop format (for the output).</para>
		/// <para>If the multisampling quality in the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultiSample.Quality">GorgonSwapChainSettings.MultiSample.Quality</see> property is higher than what the video device can support, an exception will be raised.  To determine 
		/// what the maximum quality for the sample count for the video device should be, call the <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice.GetMultiSampleQuality">GorgonVideoDevice.GetMultiSampleQuality</see> method.</para>
		/// </remarks>
		public GorgonSwapChain CreateSwapChain(string name, GorgonSwapChainSettings settings)
		{
			GorgonSwapChain swapChain = null;

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The parameter must not be empty.", "name");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

			GorgonSwapChain.ValidateSwapChainSettings(_graphics, settings);

			swapChain = new GorgonSwapChain(_graphics, name, settings);
			_graphics.AddTrackedObject(swapChain);
			swapChain.Initialize();

			return swapChain;
		}

		/// <summary>
		/// Function to create a render target.
		/// </summary>
		/// <param name="name">Name of the render target.</param>
		/// <param name="settings">Settings for the render target.</param>
		/// <param name="initialData">[Optional] Image data used to initialize the render target.</param>
		/// <returns>A new render target object.</returns>
		/// <remarks>This allows graphics data to be rendered on to a <see cref="GorgonLibrary.Graphics.GorgonTexture">texture (either 1D, 2D or 3D)</see> or a <see cref="GorgonLibrary.Graphics.GorgonBuffer">Buffer</see>.
		/// <para>Unlike the <see cref="GorgonLibrary.Graphics.GorgonSwapChain">GorgonSwapChain</see> object (which is also a render target), no defaults will be set for the <paramref name="settings"/> except multisampling, and DepthFormat (defaults to Unknown).</para>
		/// <para>If the multisampling quality in the <see cref="GorgonLibrary.Graphics.GorgonRenderTarget2DSettings.Multisampling">GorgonRenderTarget2D.Multisampling.Quality</see> property is higher than what the video device can support, an exception will be raised.  To determine 
		/// what the maximum quality for the sample count for the video device should be, call the <see cref="GorgonLibrary.Graphics.GorgonVideoDevice.GetMultiSampleQuality">GorgonVideoDevice.GetMultiSampleQuality</see> method.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when there is no <see cref="P:GorgonLibrary.Graphics.GorgonGraphics.VideoDevice">video device present on the graphics interface</see>.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonRenderTarget2DSettings.Width">Width</see> or <see cref="P:GorgonLibrary.Graphics.GorgonRenderTarget2DSettings.Width">Height</see> property is 0 or greater than the maximum size for a texture that a video device can support.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonRenderTarget2DSettings.Format">Format</see> property is unknown or is not a supported render target format.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonRenderTarget2DSettings.Multisampling">GorgonRenderTarget2DSettings.Multisampling.Quality</see> property is less than 0 or not less than the value returned by <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice">GorgonVideoDevice.GetMultiSampleQuality</see>.</para>
		/// </exception>
		public GorgonRenderTarget2D CreateRenderTarget2D(string name, GorgonRenderTarget2DSettings settings, GorgonImageData initialData = null)
		{
			if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "name");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

			ValidateRenderTargetSettings(settings);

			var target = new GorgonRenderTarget2D(_graphics, name, settings);

			_graphics.AddTrackedObject(target);
			target.Initialize(initialData);

			return target;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonOutputMerger"/> class.
		/// </summary>
		/// <param name="graphics">The graphics.</param>
		internal GorgonOutputMerger(GorgonGraphics graphics)
		{
			_graphics = graphics;

			BlendingState = new GorgonBlendRenderState(_graphics);
			DepthStencilState = new GorgonDepthStencilRenderState(_graphics);
            RenderTargets = new RenderTargetList(_graphics);
		}
		#endregion
	}
}
