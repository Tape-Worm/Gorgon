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
using GorgonLibrary.IO;
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
		#region Variables.
		private readonly GorgonGraphics _graphics;
		private D3D.RenderTargetView[] _D3DViews;
		private GorgonRenderTargetView[] _targetViews;
		private GorgonDepthStencilView _depthView;
	    private GorgonUnorderedAccessView[] _unorderedViews;
	    private D3D.UnorderedAccessView[] _D3DUnorderedViews;
	    private int _uavStartSlot = -1;
	    private int[] _uavCounts;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the maximum number of slots available for render targets.
		/// </summary>
		public int MaxRenderTargetViewSlots
		{
			get
			{
				return _graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b ? 4 : 8;
			}
		}

		/// <summary>
		/// Property to set or return the depth/stencil buffer view that is bound to the pipeline.
		/// </summary>
		public GorgonDepthStencilView DepthStencilView
		{
			get
			{
				return _depthView;
			}
			set
			{
				if (_depthView != value)
				{
					return;
				}

				SetRenderTargets(_targetViews, value);
			}
		}

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
		#endregion

		#region Methods.
        /// <summary>
        /// Function to validate the depth buffer.
        /// </summary>
        /// <param name="depthStencilView">The depth/stencil buffer to validate.</param>
        private void ValidateDepthBufferBinding(GorgonDepthStencilView depthStencilView)
        {
            if ((depthStencilView == null) || (_targetViews == null) || (_targetViews.Length == 0))
            {
                return;
            }

	        var viewResource = depthStencilView.Resource as GorgonTexture;

            // This should never happen.
            if (viewResource == null)
			{
				// Don't bother to localize this guy, this is for developers.  It'll happen if we modify the render target type but don't
				// handle related code.
				throw new GorgonException(GorgonResult.CannotBind, "View is bound to an unknown resource type.  That shouldn't happen.");
			}

            foreach (var view in _targetViews.Where(item => item != null))
            {
                if ((view.Resource.ResourceType != ResourceType.Texture1D)
                    && (view.Resource.ResourceType != ResourceType.Texture2D))
                {
                    throw new GorgonException(GorgonResult.CannotBind,
                                              string.Format(Resources.GORGFX_RTV_DEPTH_RT_TYPE_INVALID,
                                                  view.Resource.ResourceType));
                }

                if ((view.Resource.ResourceType != viewResource.ResourceType))
                {
                    throw new GorgonException(GorgonResult.CannotBind,
                                              string.Format(Resources.GORGFX_RTV_DEPTH_RESOURCE_TYPE_INVALID,
                                                  view.Resource.ResourceType,
                                                  viewResource.ResourceType));
                }

                var resTexture = view.Resource as GorgonTexture;

                if (resTexture == null)
                {
                    continue;
                }

                if (viewResource.Settings.ArrayCount != resTexture.Settings.ArrayCount)
                {
                    throw new GorgonException(GorgonResult.CannotBind,
                                              string.Format(Resources.GORGFX_RTV_DEPTH_ARRAYCOUNT_MISMATCH,
                                                  resTexture.Name));
                }

                if (viewResource.Settings.MipCount != resTexture.Settings.MipCount)
                {
                    throw new GorgonException(GorgonResult.CannotBind,
                                              string.Format(Resources.GORGFX_RTV_DEPTH_MIPCOUNT_MISMATCH,
                                                  resTexture.Name));
                }

                if ((viewResource.Settings.Multisampling.Count != resTexture.Settings.Multisampling.Count)
					|| (viewResource.Settings.Multisampling.Quality != resTexture.Settings.Multisampling.Quality))
                {
                    throw new GorgonException(GorgonResult.CannotBind,
                                              string.Format(Resources.GORGFX_RTV_DEPTH_MULTISAMPLE_MISMATCH,
                                                  resTexture.Name, resTexture.Settings.Multisampling.Count, resTexture.Settings.Multisampling.Quality,
                                                  viewResource.Settings.Multisampling.Count, viewResource.Settings.Multisampling.Quality));
                }
            }
        }

        /// <summary>
        /// Function to validate a render target.
        /// </summary>
        /// <param name="view">View to validate.</param>
        /// <param name="slot">Slot being bound.</param>
        private void ValidateRenderTargetBinding(GorgonRenderTargetView view, int slot)
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

            // We haven't set any render targets yet.
            if ((_targetViews == null) || (_targetViews.Length == 0))
            {
                return;
            }

            for (int i = 0; i < _targetViews.Length; i++)
            {
                var target = _targetViews[i];

                if ((target == null) || (i == slot))
                {
                    continue;
                }

                if (view == target)
                {
                    throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_RTV_ALREADY_BOUND, view.Resource.Name, i));
                }

                if (target.Resource == view.Resource)
                {
                    throw new GorgonException(GorgonResult.CannotBind,
                                              string.Format(Resources.GORGFX_VIEW_RESOURCE_ALREADY_BOUND,
                                                            view.Resource.Name,
                                                            i,
                                                            view.GetType().FullName));
                }

                // Ensure the unordered access views and resource views don't have the same resource bound.
                if ((_unorderedViews != null) && (_unorderedViews.Length > 0))
                {
                    for (int j = 0; j < _unorderedViews.Length; j++)
                    {
                        if (_unorderedViews[j] == null)
                        {
                            continue;
                        }

                        if (_unorderedViews[j].Resource == view.Resource)
                        {
                            throw new GorgonException(GorgonResult.CannotBind,
                                                      string.Format(Resources.GORGFX_VIEW_RESOURCE_ALREADY_BOUND,
                                                                    view.Resource.Name,
                                                                    j,
                                                                    typeof(GorgonUnorderedAccessView).FullName));
                        }
                    }
                }

                if ((_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
                    && (target.Format != view.Format)
                    && (GorgonBufferFormatInfo.GetInfo(target.Format).BitDepth != GorgonBufferFormatInfo.GetInfo(view.Format).BitDepth))
                {
                    throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_RTV_BIT_DEPTH_MISMATCH,
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

        /// <summary>
        /// Function to perform the actual binding of the targets, uavs and depth/stencil buffers.
        /// </summary>
        /// <param name="depthView">The Direct 3D depth/stencil view to set.</param>
        private void SetTargets(D3D.DepthStencilView depthView)
        {
            // If we have no views to set, then get out.
            if (((_D3DViews == null) || (_D3DViews.Length == 0))
                && ((_D3DUnorderedViews == null) || (_D3DUnorderedViews.Length == 0)))
            {
                return;
            }

            // We have UAV views, so we need to use the proper function.
            if ((_graphics.VideoDevice.SupportedFeatureLevel >= DeviceFeatureLevel.SM5) && (_D3DUnorderedViews != null) && (_D3DUnorderedViews.Length > 0) && (_uavStartSlot > -1))
            {
                if ((_unorderedViews != null) && (_unorderedViews.Length != 0))
                {
                    _graphics.Context.OutputMerger.SetTargets(depthView,
                                                              _uavStartSlot,
                                                              _D3DUnorderedViews,
                                                              _uavCounts,
                                                              _D3DViews);
                    return;
                }

                _graphics.Context.OutputMerger.SetUnorderedAccessViews(0, _D3DUnorderedViews);
                _uavCounts = null;
                _uavStartSlot = -1;
            }

            _graphics.Context.OutputMerger.SetTargets(depthView, _D3DViews);
        }

        /// <summary>
        /// Function to perform the binding of unordered access views.
        /// </summary>
        /// <param name="startSlot">Starting slot for the unordered access views.</param>
        /// <param name="views">Views to bind.</param>
        /// <returns>TRUE if the UAV bindings have changed, FALSE if not.</returns>
        private bool BindUnorderedAccessViews(int startSlot, GorgonUnorderedAccessView[] views)
        {
            bool hasChanged = false;

#if DEBUG
            if ((startSlot < 0) || (startSlot >= 8))
            {
                throw new ArgumentOutOfRangeException("startSlot", Resources.GORGFX_UAV_SLOT_OUT_OF_RANGE);
            }

            if (views.Length > 8)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_RTV_TOO_MANY, MaxRenderTargetViewSlots));
            }

            if (startSlot + views.Length > 8)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_UAV_TOO_MANY, startSlot, views.Length));
            }
#endif
            _uavStartSlot = startSlot;

            // Allocate the arrays.
            if ((_unorderedViews == null) || (_unorderedViews.Length != views.Length))
            {
                _unorderedViews = new GorgonUnorderedAccessView[views.Length];
                _D3DUnorderedViews = new D3D.UnorderedAccessView[_unorderedViews.Length];
                _uavCounts = new int[_unorderedViews.Length];
                hasChanged = true;
            }

            // If we only have a single view, then only bind the first view.
            if ((views.Length == 1) && (views[0] != _unorderedViews[0]))
            {
                var view = views[0];

#if DEBUG
                ValidateUnorderedAccessViewBinding(view, 0);
#endif

                var structView = view as GorgonStructuredBufferUnorderedAccessView;

                _unorderedViews[0] = view;

                if (structView != null)
                {
                    _uavCounts[0] = structView.InitialCount;
                }
                else
                {
                    _uavCounts[0] = -1;
                }

                _D3DUnorderedViews[0] = view.D3DView;

                // Ensure this view is not bound to another part of the pipeline.
                if (view != null)
                {
                    _graphics.Shaders.UnbindResource(view.Resource);
                }

                return true;
            }

            // Copy views.
            for (int i = 0; i < views.Length; i++)
            {
                var view = views[i];

                if (view == _unorderedViews[i])
                {
                    continue;
                }

#if DEBUG
                ValidateUnorderedAccessViewBinding(view, i);
#endif

                if (view == null)
                {
                    _unorderedViews[i] = null;
                    _uavCounts[i] = -1;
                    _D3DUnorderedViews[i] = null;
                }
                else
                {
                    var structView = view as GorgonStructuredBufferUnorderedAccessView;

                    if (structView != null)
                    {
                        _uavCounts[i] = structView.InitialCount;
                    }
                    else
                    {
                        _uavCounts[i] = -1;
                    }

                    _unorderedViews[i] = view;
                    _D3DUnorderedViews[i] = view.D3DView;

                    // Ensure this view is not bound to another part of the pipeline.
                    _graphics.Shaders.UnbindResource(view.Resource);
                }

                hasChanged = true;
            }

            return hasChanged;
        }

        /// <summary>
        /// Function to validate an unordered access view binding.
        /// </summary>
        /// <param name="view">Unordered access view to evaluate.</param>
        /// <param name="slot">Slot for the view.</param>
        private void ValidateUnorderedAccessViewBinding(GorgonUnorderedAccessView view, int slot)
        {
            if ((view == null) || (_unorderedViews == null) || (_unorderedViews.Length == 0))
            {
                return;
            }

            for (int i = 0; i < _unorderedViews.Length; i++)
            {
                var otherView = _unorderedViews[i];

                if ((slot != i) || (otherView == null))
                {
                    continue;
                }

                if (otherView == view)
                {
                    throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_VIEW_ALREADY_BOUND, i));
                }

                if (otherView.Resource == view.Resource)
                {
                    throw new GorgonException(GorgonResult.CannotBind,
                                              string.Format(Resources.GORGFX_VIEW_RESOURCE_ALREADY_BOUND,
                                                            view.Resource.Name,
                                                            i,
                                                            view.GetType().FullName));
                }

                // Ensure the unordered access views and resource views don't have the same resource bound.
                if ((_targetViews != null) && (_targetViews.Length > 0))
                {
                    for (int j = 0; j < _targetViews.Length; j++)
                    {
                        if (_targetViews[j] == null)
                        {
                            continue;
                        }

                        if (_targetViews[j].Resource == view.Resource)
                        {
                            throw new GorgonException(GorgonResult.CannotBind,
                                                      string.Format(Resources.GORGFX_VIEW_RESOURCE_ALREADY_BOUND,
                                                                    view.Resource.Name,
                                                                    j,
                                                                    typeof(GorgonRenderTargetView).FullName));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Function to validate the settings for a render target.
        /// </summary>
        /// <param name="settings">Settings to validate.</param>
        private void ValidateRenderTargetSettings(GorgonRenderTargetBufferSettings settings)
        {
            if (settings.RenderTargetType != RenderTargetType.Buffer)
            {
                throw new GorgonException(GorgonResult.CannotCreate, "Cannot use 1D, 2D or 3D settings for buffer render targets.");
            }

            if (_graphics.VideoDevice == null)
            {
                throw new GorgonException(GorgonResult.CannotCreate,
                                          "Cannot create the render target, no video device was selected.");
            }

            // Ensure the dimensions are valid.
            if (settings.SizeInBytes <= 4)
            {
                throw new GorgonException(GorgonResult.CannotCreate,
                                          "Render target must have a size of 4 or more bytes.");
            }

            if (settings.Format == BufferFormat.Unknown)
            {
                throw new GorgonException(GorgonResult.CannotCreate, "Render target must have a known buffer format.");
            }

            // Ensure that the selected video format can be used.
            if (!_graphics.VideoDevice.SupportsRenderTargetFormat(settings.Format, false))
            {
                throw new GorgonException(GorgonResult.CannotCreate, "Cannot use the format '" + settings.Format
                                            + "' for a render target on the video device '" + _graphics.VideoDevice.Name + "'.");
            }
        }
        
        /// <summary>
	    /// Function to validate the settings for a render target.
	    /// </summary>
        /// <param name="settings">Settings to validate.</param>
	    internal void ValidateRenderTargetSettings(IRenderTargetTextureSettings settings)
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
                throw new ArgumentException("Cannot use the format '" + settings.Format
                                            + "' for a render target on the video device '" + _graphics.VideoDevice.Name + "'.");
            }
        }

		/// <summary>
		/// Function to validate the settings for this depth/stencil buffer.
		/// </summary>
		/// <param name="settings">Settings to validate.</param>
		internal void ValidateDepthStencilSettings(IDepthStencilSettings settings)
		{
			ITextureSettings textureSettings = settings;

			// Validate texture settings first.
			_graphics.Textures.ValidateTexture2D(ref textureSettings);

			if (settings.Format == BufferFormat.Unknown)
			{
				throw new ArgumentException("The format for the depth buffer ('" + settings.Format + "') is not valid.");
			}

			if (!_graphics.VideoDevice.SupportsDepthFormat(settings.Format))
			{
				throw new ArgumentException("Video device '" + _graphics.VideoDevice.Name + "' does not support '"
											+ settings.Format + "' as a depth/stencil buffer format.");
			}

			// Make sure we can use the same multi-sampling with our depth buffer.
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

			if (!settings.AllowShaderView)
			{
				if (!_graphics.VideoDevice.Supports2DTextureFormat(settings.Format))
				{
					throw new ArgumentException("Video device '" + _graphics.VideoDevice.Name + "' does not support '"
												+ settings.Format + "' as texture format for the depth buffer.");
				}
			}
			else
			{
                if (_graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM4)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, "Depth/stencil buffers cannot be bound to the shader pipeline with video devices that don't support SM4 or better.");
                }

                if ((_graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM4_1)
                    && ((settings.Multisampling.Count > 1) || (settings.Multisampling.Quality > 0)))
                {
                    throw new GorgonException(GorgonResult.CannotCreate, "Multisampled Depth/stencil buffers cannot be bound to the shader pipeline if the video device does not support SM4_1 or better.");
                }

				if (settings.TextureFormat == BufferFormat.Unknown)
				{
					throw new GorgonException(GorgonResult.CannotCreate, "The texture format must not be Unknown.");
				}

				var formatInfo = GorgonBufferFormatInfo.GetInfo(settings.TextureFormat);

				if (!formatInfo.IsTypeless)
				{
					throw new GorgonException(GorgonResult.CannotCreate, "The texture format must be typeless.");
				}

				if (!_graphics.VideoDevice.Supports2DTextureFormat(settings.TextureFormat))
				{
					throw new ArgumentException("Video device '" + _graphics.VideoDevice.Name + "' does not support '"
												+ settings.TextureFormat + "' as texture format for the depth buffer.");
				}
			}
		}

		/// <summary>
		/// Function to unbind a target view.
		/// </summary>
		/// <param name="view">View to unbind.</param>
		internal void Unbind(GorgonRenderTargetView view)
		{
			if ((_targetViews == null) || (_targetViews.Length == 0))
			{
				return;
			}

		    int index = Array.IndexOf(_targetViews, view);

            if (index == -1)
            {
                return;
            }

		    _targetViews[index] = null;
		    _D3DViews[index] = null;

            SetTargets(_depthView == null ? null : _depthView.D3DView);

		    // If all of the views are nulled out, then reset the arrays.
		    if (_targetViews.Any(item => item != null))
		    {
		        return;
		    }

		    _D3DViews = null;
		    _targetViews = null;
		}

        /// <summary>
        /// Function to unbind an unordered access view.
        /// </summary>
        /// <param name="view">Unordered access view to unbind.</param>
        internal void Unbind(GorgonUnorderedAccessView view)
        {
            if ((_unorderedViews == null) || (_unorderedViews.Length == 0))
            {
                return;
            }

            int index = Array.IndexOf(_unorderedViews, view);

            if (index == -1)
            {
                return;
            }

            _unorderedViews[index] = null;
            _D3DUnorderedViews[index] = null;
            _uavCounts[index] = -1;

            SetTargets(_depthView == null ? null : _depthView.D3DView);

            if (_unorderedViews.Any(item => item != null))
            {
                // Update the starting slot.
                if (index == _uavStartSlot)
                {
                    _uavStartSlot = Array.IndexOf(_unorderedViews, _unorderedViews.First(item => item != null));
                }

                return;
            }

            _unorderedViews = null;
            _uavCounts = null;
            _D3DUnorderedViews = null;
            _uavStartSlot = -1;
        }

        /// <summary>
        /// Function to unbind targets after the starting slot specified.
        /// </summary>
        /// <param name="startSlot">Starting slot to unbind.</param>
        internal void UnbindSlots(int startSlot)
        {
            if ((_targetViews == null)
                || (startSlot >= _targetViews.Length))
            {
                return;
            }

            for (int i = startSlot; i < _targetViews.Length; i++)
            {
                _targetViews[i] = null;
            }
        }

        /// <summary>
		/// Function to clean up resources used by the interface.
		/// </summary>
		internal void CleanUp()
        {
			// Unbind all of the views.
	        if (_D3DViews != null)
	        {
		        for (int i = 0; i < _D3DViews.Length; i++)
		        {
			        _D3DViews[i] = null;
		        }
		        _graphics.Context.OutputMerger.SetTargets(null, _D3DViews);
		        _D3DViews = null;
		        _targetViews = null;
	        }

            // Unbind any unordered access views (assuming we didn't have a target bound).
            if ((_D3DUnorderedViews != null) && (_uavStartSlot != -1))
            {
                _graphics.Context.OutputMerger.SetUnorderedAccessViews(_uavStartSlot, _D3DUnorderedViews);
                _unorderedViews = null;
                _uavCounts = null;
                _D3DUnorderedViews = null;
            }

            _uavStartSlot = -1;

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
        /// Function to retrieve an unordered access view bound at the specified index.
        /// </summary>
        /// <param name="index">The index of the view to retrieve.</param>
        /// <returns>The view at the index, or NULL if not view was bound at the index.</returns>
        public GorgonUnorderedAccessView GetUnorderedAccessView(int index)
        {
            if (_unorderedViews == null)
            {
                return null;
            }

            if ((index >= 0) && (index < _unorderedViews.Length))
            {
                return _unorderedViews[index];
            }

            return null;
        }

		/// <summary>
		/// Function to retrieve a view bound at the specified index.
		/// </summary>
		/// <param name="index">Index of the view to retrieve.</param>
		/// <returns>The view at the index, or NULL if no view was bound at the index.</returns>
		public GorgonRenderTargetView GetRenderTarget(int index)
		{
			if (_targetViews == null)
			{
				return null;
			}

			if ((index >= 0) && (index < _targetViews.Length))
			{
				return _targetViews[index];
			}

			return null;
		}

        /// <summary>
        /// Function to bind a an array of unordered access views to the pipeline at the specified render target slot.
        /// </summary>
        /// <param name="startSlot">Starting slot to bind at.</param>
        /// <param name="views">Unordered access views to bind.</param>
        /// <remarks>This will bind multiple unordered access views (or a single view) to the pipeline.  This function will preserve any render targets that are already bound before the <paramref name="startSlot"/>.
        /// <para>Unordered access views may be bound with the render target to give access to read/write resources in a pixel shader.  These views must be set at the same time as the 
        /// render targets because UAVs occupy the same slots as render target views.  This means that any slots including and after the <paramref name="startSlot"/> 
        /// will unbind any existing render targets.  This also means that the number unordered access views must not be greater than 8.</para>
        /// <para>Setting the <paramref name="views"/> parameter to NULL or empty will set unbind all the current unordered access views.</para>
        /// <para>A video device with a feature level of SM5 or better is required in order to use the unordered access views.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when then <paramref name="startSlot"/> is less than 0, or greater than 8.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the number of <paramref name="views"/> is greater than 8.</exception>
        /// <exception cref="GorgonLibrary.GorgonException">
        /// Thrown when the current video device feature level is not SM5 or better.
        /// <para>-or-</para>
        /// <para>Thrown when the render target view, depth/stencil view, or the unordered access views could not be bound to the pipeline.</para></exception>
        public void SetUnorderedAccessViews(int startSlot, params GorgonUnorderedAccessView[] views)
        {
            bool hasChanged = false;

            if ((views != null)
                && (views.Length > 0))
            {
                hasChanged = BindUnorderedAccessViews(startSlot, views);

                // Unbind any render targets that occupy the same slots.
                if (hasChanged)
                {
                    UnbindSlots(startSlot);    
                }
            }
            else
            {
                if (_unorderedViews != null)
                {
                    _unorderedViews = null;
                    hasChanged = true;
                }
            }

            if (!hasChanged)
            {
                return;
            }

            SetTargets(_depthView == null ? null : _depthView.D3DView);
        }

		/// <summary>
		/// Function to bind a a single render target view and a depth/stencil view to the pipeline.
		/// </summary>
		/// <param name="view">Render target view to set.</param>
		/// <param name="depthStencilView">[Optional] Depth/stencil view to set.</param>
        /// <param name="startSlot">[Optional] The starting slot for the unordered access views.</param>
        /// <param name="unorderedAccessViews">[Optional] Unordered access views to bind to the current pixel shader.</param>
        /// <remarks>This will bind a single render target to the pipeline.  A render target is one of the GorgonRenderTargetViews, GorgonRenderTarget types (Buffer, Texture1D/2D/3D) or 
        /// a <see cref="GorgonLibrary.Graphics.GorgonSwapChain">GorgonSwapChain</see>. The latter types are all convertable to a <see cref="GorgonLibrary.Graphics.GorgonRenderTargetView">GorgonRenderTargetView</see>.
        /// <para>When binding, ensure that all the render targets match the dimensions, format, array count and mip level count of the render target views that are already bound to the pipeline.  This 
        /// applies to the depth/stencil buffer as well.</para>
        /// <para>Unordered access views may be bound with the render target to give access to read/write resources in a pixel shader.  These views must be set at the same time as the 
        /// render targets because UAVs occupy the same slots as render target views.  This means that any slots including and after the <paramref name="startSlot"/> 
        /// will unbind any existing render targets.  This also means that the total number of render target views and unordered access views must not be greater than 8.</para>
        /// <para>A video device with a feature level of SM5 or better is required in order to use the unordered access views.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when then <paramref name="startSlot"/> is less than 0, or greater than 8.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the number of render target views plus the number of <paramref name="unorderedAccessViews"/> are greater than 8 (or 4 render targets on a 
        /// video device with a feature level of SM2_a_b, UAVs are not supported).</exception>
        /// <exception cref="GorgonLibrary.GorgonException">
        /// Thrown when the <paramref name="unorderedAccessViews"/> parameter is non NULL (and has 1 or more elements), and the video device feature level is not SM5 or better.
        /// <para>-or-</para>
        /// <para>Thrown when the render target view, depth/stencil view, or the unordered access views could not be bound to the pipeline.</para></exception>
        public void SetRenderTarget(GorgonRenderTargetView view, GorgonDepthStencilView depthStencilView = null, int startSlot = 1, params GorgonUnorderedAccessView[] unorderedAccessViews)
		{
		    bool uavsChanged = false;
			D3D.DepthStencilView depthView = depthStencilView == null ? null : depthStencilView.D3DView;

            // Set up UAVs for binding.
            if ((unorderedAccessViews != null) && (unorderedAccessViews.Length > 0))
            {
#if DEBUG
                if (_graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
                {
                    throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM5));
                }

                if ((view != null) && (unorderedAccessViews.Length + 1 > MaxRenderTargetViewSlots))
                {
                    throw new ArgumentException(string.Format(Resources.GORGFX_RTV_TOO_MANY, MaxRenderTargetViewSlots));
                }
#endif

                uavsChanged = BindUnorderedAccessViews(startSlot, unorderedAccessViews);
            }
            else
            {
                _unorderedViews = null;

                // Reset all the views.
                if (_D3DUnorderedViews != null)
                {
                    for (int i = 0; i < _D3DUnorderedViews.Length; i++)
                    {
                        _D3DUnorderedViews[i] = null;
                    }
                }
            }


            // Don't rebind the same target/depth/stencil.
            if ((!uavsChanged) && (_targetViews != null) && (_targetViews.Length > 0) && (_targetViews[0] == view) && (depthStencilView == _depthView))
            {
                return;
            }

#if DEBUG
            ValidateRenderTargetBinding(view, 0);
#endif

			if (view == null)
			{
				_D3DViews = null;
				_targetViews = null;

                SetTargets(depthView);
				return;
			}


			if ((_targetViews == null) || (_targetViews.Length != 1))
			{
				_targetViews = new[]
					{
						view
					};
				_D3DViews = new[]
					{
						view.D3DView
					};
			}
			else
			{
				_targetViews[0] = view;
				_D3DViews[0] = view.D3DView;
			}

#if DEBUG
            // Validate the depth/stencil buffer here because we need to have the current render target set before
            // evaluation.
            ValidateDepthBufferBinding(depthStencilView);
#endif
			_depthView = depthStencilView; 
            SetTargets(depthView);
		}

		/// <summary>
		/// Function to bind a depth/stencil view and a list of render target views to the pipeline.
		/// </summary>
		/// <param name="views">List of render target views to set.</param>
		/// <param name="depthStencilBuffer">[Optional] Depth/stencil view to set.</param>
        /// <param name="startSlot">[Optional] The starting slot for the unordered access views.</param>
        /// <param name="unorderedAccessViews">[Optional] Unordered access views to bind to the current pixel shader.</param>
		/// <remarks>This will bind multiple render targets to the pipeline at the same time.  A render target is one of the GorgonRenderTargetViews, GorgonRenderTarget types (Buffer, Texture1D/2D/3D) or 
		/// a <see cref="GorgonLibrary.Graphics.GorgonSwapChain">GorgonSwapChain</see>. The latter types are all convertable to a <see cref="GorgonLibrary.Graphics.GorgonRenderTargetView">GorgonRenderTargetView</see>.
		/// <para>When binding, ensure that all the render targets match the dimensions, format, array count and mip level count of the render target views that are already bound to the pipeline.  This 
		/// applies to the depth/stencil buffer as well.</para>
		/// <para>Unordered access views may be bound with the render target to give access to read/write resources in a pixel shader.  These views must be set at the same time as the 
		/// render targets because UAVs occupy the same slots as render target views.  This means that any slots including and after the <paramref name="startSlot"/> 
		/// will unbind any existing render targets.  This also means that the total number of render target views and unordered access views must not be greater than 8.</para>
		/// <para>A video device with a feature level of SM5 or better is required in order to use the unordered access views.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when then <paramref name="startSlot"/> is less than 0, or greater than 8.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the number of <paramref name="views"/> and/or <paramref name="unorderedAccessViews"/> are greater than 8 (or 4 render targets on a 
		/// video device with a feature level of SM2_a_b, UAVs are not supported).</exception>
		/// <exception cref="GorgonLibrary.GorgonException">
		/// Thrown when the <paramref name="unorderedAccessViews"/> parameter is non NULL (and has 1 or more elements), and the video device feature level is not SM5 or better.
		/// <para>-or-</para>
        /// <para>Thrown when the render target view, depth/stencil view, or the unordered access views could not be bound to the pipeline.</para></exception>
		public void SetRenderTargets(GorgonRenderTargetView[] views, GorgonDepthStencilView depthStencilBuffer = null, int startSlot = 1, params GorgonUnorderedAccessView[] unorderedAccessViews)
		{
		    bool hasChanged = false;

		    D3D.DepthStencilView depthView = depthStencilBuffer == null ? null : depthStencilBuffer.D3DView;
			_depthView = depthStencilBuffer;

            // Set up UAVs for binding.
            if ((unorderedAccessViews != null) && (unorderedAccessViews.Length > 0))
            {
#if DEBUG
                if (_graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
                {
                    throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM5));    
                }

                if ((views != null) && (views.Length + unorderedAccessViews.Length > MaxRenderTargetViewSlots))
                {
                    throw new ArgumentException(string.Format(Resources.GORGFX_RTV_TOO_MANY, MaxRenderTargetViewSlots));
                }
#endif

                hasChanged = BindUnorderedAccessViews(startSlot, unorderedAccessViews);    
            }
            else
            {
                _unorderedViews = null;
            }

			// If we didn't pass any views, then unbind all the views.
			if ((views == null) || (views.Length == 0))
			{
			    if ((_targetViews != null) && (_targetViews.Length != 0))
			    {
			        _D3DViews = null;
			        _targetViews = null;

#if DEBUG
                    // Validate the depth/stencil buffer here because we need to have the current render target set before
                    // evaluation.
                    ValidateDepthBufferBinding(depthStencilBuffer);
#endif

                    SetTargets(depthView);
			    }

			    return;
			}

#if DEBUG
            // Ensure that the number of render targets is less than the maximum available render target slot count.
            if (views.Length > MaxRenderTargetViewSlots)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_RTV_TOO_MANY, MaxRenderTargetViewSlots),"views");
            }
#endif

            // Have we changed the array structure or depth/stencil view?
		    hasChanged = (!hasChanged) && ((_targetViews == null) || (_targetViews.Length != views.Length)
		                      || (depthStencilBuffer != _depthView));

			// Update the current view list.
			if ((_targetViews == null) || (views.Length != _targetViews.Length))
			{
                _targetViews = new GorgonRenderTargetView[views.Length];
				_D3DViews = new D3D.RenderTargetView[_targetViews.Length];
			    hasChanged = true;
			}

		    int lastSlot = views.Length;

            // Determine the last slot for the render targets.
            if ((unorderedAccessViews != null) && (unorderedAccessViews.Length > 0) && (views.Length > startSlot))
            {
                lastSlot = startSlot - 1;
                hasChanged = true;
            }

			// If we have more than one view to set, then blast them all out.
			for (int i = 0; i < lastSlot; i++)
			{
				var view = views[i];

                // If the targets in the slots have not changed, then don't bother.
                if ((!hasChanged) && (view == _targetViews[i]))
                {
                    continue;
                }
			    
#if DEBUG
                ValidateRenderTargetBinding(view, i);
#endif

				if (view != null)
				{
				    _targetViews[i] = view;
					_D3DViews[i] = view.D3DView;
				}
				else
				{
				    _targetViews[i] = null;
					_D3DViews[i] = null;
				}

                hasChanged = true;
			}

            // Do nothing if there's been no change.
            if (!hasChanged)
            {
                return;
            }

#if DEBUG
            // Validate the depth/stencil buffer here because we need to have the current render target set before
            // evaluation.
            ValidateDepthBufferBinding(depthStencilBuffer);
#endif
            SetTargets(depthView);
        }

        /// <summary>
        /// Function to retrieve the list of bound unordered access views.
        /// </summary>
        /// <returns>An array of bound unordered access views.</returns>
        public GorgonUnorderedAccessView[] GetUnorderedAccessViews()
        {
            var result = new GorgonUnorderedAccessView[_unorderedViews == null ? 0 : _unorderedViews.Length];

            if ((_unorderedViews != null) && (_unorderedViews.Length > 0))
            {
                _targetViews.CopyTo(result, 0);
            }

            return result;
        }

		/// <summary>
		/// Function to retrieve the list of render targets.
		/// </summary>
		/// <returns>An array of render targets.</returns>
		public GorgonRenderTargetView[] GetRenderTargets()
		{
			var result = new GorgonRenderTargetView[_targetViews == null ? 0 : _targetViews.Length];

			if ((_targetViews != null) && (_targetViews.Length > 0))
			{
				_targetViews.CopyTo(result, 0);
			}

			return result;
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
            {
                throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM2_a_b));
            }
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
		/// <param name="alignedByteOffset">Number of bytes to start at within the buffer.</param>
		/// <param name="isIndexed">TRUE if the data is indexed, FALSE if not.</param>
		/// <remarks>This method is not supported by SM2_a_b or SM_4.x video devices.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="buffer"/> passed to the parameter was not set up to have indirect arguments.</exception>
		/// <exception cref="System.InvalidOperationException">Thrown if the current video device is a SM2_a_b or SM4_x device.</exception>
		public void DrawInstancedIndirect(GorgonBaseBuffer buffer, int alignedByteOffset, bool isIndexed)
		{
			GorgonDebug.AssertNull(buffer, "buffer");

#if DEBUG
            if (_graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
            {
                throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM5));
            }

			if (!buffer.Settings.AllowIndirectArguments)
			{
				throw new ArgumentException("Cannot call DrawInstancedIndirect with a buffer than does not contain indirect arguments.", "buffer");
			}
#endif
			GorgonRenderStatistics.DrawCallCount++;
			if (isIndexed)
				_graphics.Context.DrawIndexedInstancedIndirect((D3D.Buffer)buffer.D3DResource, alignedByteOffset);
			else
				_graphics.Context.DrawInstancedIndirect((D3D.Buffer)buffer.D3DResource, alignedByteOffset);
		}

		/// <summary>
		/// Function to create a 2D depth/stencil buffer.
		/// </summary>
		/// <param name="name">Name of the depth/stencil buffer.</param>
		/// <param name="settings">Settings to apply to the depth/stencil buffer.</param>
		/// <param name="initialData">[Optional] Data used to initialize the depth buffer.</param>
		/// <returns>A new depth/stencil buffer.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the depth/stencil buffer could not be created.</exception>
		/// <remarks>
		/// A depth buffer or its corresponding view(s) may set by assigning it to the <see cref="GorgonLibrary.Graphics.GorgonOutputMerger.DepthStencilView">DepthStencilView</see> property.   
		/// <para>The texture for a depth/stencil may be used in a shader for cards that have a feature level of SM_4_0 or better to allow for reading of the depth/stencil.
        /// To achieve this, create the depth/stencil with <see cref="P:GorgonLibrary.Graphics.GorgonDepthStencil2DSettings.AllowShaderView">GorgonDepthStencilSettings.AllowShaderView</see> set to TRUE, and 
        /// give the <see cref="P:GorgonLibrary.Graphics.GorgonDepthStencil2DSettings.TextureFormat">GorgonDepthStencilSettings.TextureFormat</see> property a typeless format that matches the size, in bytes, 
        /// of the depth/stencil format (e.g. a depth buffer with D32_Float as its format, could use a texture format of R32).  This is required because a depth/stencil format can't be used in a shader view.
        /// </para>
        /// <para>A <see cref="GorgonLibrary.Graphics.GorgonDepthStencilView">depth/stencil view</see> can be bound in read-only mode to the depth/stencil and the shader view if the current video device has 
        /// a feature level of SM5 or better. To set this up, create the depth/stencil view by setting the flags parameter appropriately.</para>
		/// <para>Binding to a shader view requires video device that has a feature level of SM_4_0 or below.  If the depth/stencil is multisampled, then a feature level of SM4_1 is required.</para>
		/// <para></para>
		/// </remarks>
		public GorgonDepthStencil2D CreateDepthStencil(string name, GorgonDepthStencil2DSettings settings, GorgonImageData initialData = null)
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
			
			ValidateDepthStencilSettings(settings);

			var depthBuffer = new GorgonDepthStencil2D(_graphics, name, settings);
			_graphics.AddTrackedObject(depthBuffer);
			depthBuffer.Initialize(initialData);

			return depthBuffer;
		}

        /// <summary>
        /// Function to create a 1D depth/stencil buffer.
        /// </summary>
        /// <param name="name">Name of the depth/stencil buffer.</param>
        /// <param name="settings">Settings to apply to the depth/stencil buffer.</param>
        /// <param name="initialData">[Optional] Data used to initialize the depth buffer.</param>
        /// <returns>A new depth/stencil buffer.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the depth/stencil buffer could not be created.</exception>
        /// <remarks>
        /// A depth buffer or its corresponding view(s) may set by assigning it to the <see cref="GorgonLibrary.Graphics.GorgonOutputMerger.DepthStencilView">DepthStencilView</see> property.   
        /// <para>The texture for a depth/stencil may be used in a shader for cards that have a feature level of SM_4_0 or better to allow for reading of the depth/stencil.
        /// To achieve this, create the depth/stencil with <see cref="P:GorgonLibrary.Graphics.GorgonDepthStencil1DSettings.AllowShaderView">GorgonDepthStencilSettings.AllowShaderView</see> set to TRUE, and 
        /// give the <see cref="P:GorgonLibrary.Graphics.GorgonDepthStencil1DSettings.TextureFormat">GorgonDepthStencilSettings.TextureFormat</see> property a typeless format that matches the size, in bytes, 
        /// of the depth/stencil format (e.g. a depth buffer with D32_Float as its format, could use a texture format of R32).  This is required because a depth/stencil format can't be used in a shader view.
        /// </para>
        /// <para>A <see cref="GorgonLibrary.Graphics.GorgonDepthStencilView">depth/stencil view</see> can be bound in read-only mode to the depth/stencil and the shader view if the current video device has 
        /// a feature level of SM5 or better. To set this up, create the depth/stencil view by setting the flags parameter appropriately.</para>
        /// <para>Binding to a shader view requires video device that has a feature level of SM_4_0 or below.  If the depth/stencil is multisampled, then a feature level of SM4_1 is required.</para>
        /// <para></para>
        /// </remarks>
        public GorgonDepthStencil1D CreateDepthStencil(string name, GorgonDepthStencil1DSettings settings, GorgonImageData initialData = null)
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

            ValidateDepthStencilSettings(settings);

            var depthBuffer = new GorgonDepthStencil1D(_graphics, name, settings);
            _graphics.AddTrackedObject(depthBuffer);
            depthBuffer.Initialize(initialData);

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
        /// Function to create a buffer render target.
        /// </summary>
        /// <param name="name">Name of the render target.</param>
        /// <param name="settings">Settings for the render target.</param>
        /// <param name="initialData">[Optional] Data used to initialize the underlying buffer.</param>
        /// <returns>A new render target object.</returns>
        /// <remarks>This allows graphics data to be rendered to a <see cref="GorgonLibrary.Graphics.GorgonBuffer">buffer</see>.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when there is no <see cref="P:GorgonLibrary.Graphics.GorgonGraphics.VideoDevice">video device present on the graphics interface</see>.
        /// <para>-or-</para>
        /// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonRenderTargetBufferSettings.SizeInBytes">SizeInBytes</see> property is less than 4.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonRenderTargetBufferSettings.Format">Format</see> property is unknown or is not a supported render target format.</para>
        /// </exception>
        public GorgonRenderTargetBuffer CreateRenderTarget(string name, GorgonRenderTargetBufferSettings settings, GorgonDataStream initialData = null)
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

            var target = new GorgonRenderTargetBuffer(_graphics, name, settings);

            _graphics.AddTrackedObject(target);
            target.Initialize(initialData);

            return target;
        }

        /// <summary>
        /// Function to create a 1D render target.
        /// </summary>
        /// <param name="name">Name of the render target.</param>
        /// <param name="settings">Settings for the render target.</param>
        /// <param name="initialData">[Optional] Image data used to initialize the render target.</param>
        /// <returns>A new render target object.</returns>
        /// <remarks>This allows graphics data to be rendered on to a <see cref="GorgonLibrary.Graphics.GorgonTexture1D">1D texture</see>.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when there is no <see cref="P:GorgonLibrary.Graphics.GorgonGraphics.VideoDevice">video device present on the graphics interface</see>.
        /// <para>-or-</para>
        /// <para>Thrown when the Width value is 0 or greater than the maximum size for a texture that a video device can support.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the Format is unknown or is not a supported render target format.</para>
        /// </exception>
        public GorgonRenderTarget1D CreateRenderTarget(string name, GorgonRenderTarget1DSettings settings, GorgonImageData initialData = null)
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

            var target = new GorgonRenderTarget1D(_graphics, name, settings);

            _graphics.AddTrackedObject(target);
            target.Initialize(initialData);

            return target;
        }

		/// <summary>
		/// Function to create a 2D render target.
		/// </summary>
		/// <param name="name">Name of the render target.</param>
		/// <param name="settings">Settings for the render target.</param>
		/// <param name="initialData">[Optional] Image data used to initialize the render target.</param>
		/// <returns>A new render target object.</returns>
		/// <remarks>This allows graphics data to be rendered on to a <see cref="GorgonLibrary.Graphics.GorgonTexture2D">2D texture</see>.
		/// <para>If the multisampling quality in the <see cref="GorgonLibrary.Graphics.GorgonRenderTarget2DSettings.Multisampling">GorgonRenderTarget2D.Multisampling.Quality</see> property is higher than what the video device can support, an exception will be raised.  To determine 
		/// what the maximum quality for the sample count for the video device should be, call the <see cref="GorgonLibrary.Graphics.GorgonVideoDevice.GetMultiSampleQuality">GorgonVideoDevice.GetMultiSampleQuality</see> method.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when there is no <see cref="P:GorgonLibrary.Graphics.GorgonGraphics.VideoDevice">video device present on the graphics interface</see>.
		/// <para>-or-</para>
        /// <para>-or-</para>
        /// <para>Thrown when the Width or Height values are 0 or greater than the maximum size for a texture that a video device can support.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the Format is unknown or is not a supported render target format.</para>
        /// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonRenderTarget2DSettings.Multisampling">GorgonRenderTarget2DSettings.Multisampling.Quality</see> property is less than 0 or not less than the value returned by <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice">GorgonVideoDevice.GetMultiSampleQuality</see>.</para>
		/// </exception>
		public GorgonRenderTarget2D CreateRenderTarget(string name, GorgonRenderTarget2DSettings settings, GorgonImageData initialData = null)
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

        /// <summary>
        /// Function to create a 3D render target.
        /// </summary>
        /// <param name="name">Name of the render target.</param>
        /// <param name="settings">Settings for the render target.</param>
        /// <param name="initialData">[Optional] Image data used to initialize the render target.</param>
        /// <returns>A new render target object.</returns>
        /// <remarks>This allows graphics data to be rendered on to a <see cref="GorgonLibrary.Graphics.GorgonTexture3D">3D texture</see>.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when there is no <see cref="P:GorgonLibrary.Graphics.GorgonGraphics.VideoDevice">video device present on the graphics interface</see>.
        /// <para>-or-</para>
        /// <para>Thrown when the Width, Height or Depth values is 0 or greater than the maximum size for a texture that a video device can support.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the Format is unknown or is not a supported render target format.</para>
        /// </exception>
        public GorgonRenderTarget3D CreateRenderTarget(string name, GorgonRenderTarget3DSettings settings, GorgonImageData initialData = null)
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

            var target = new GorgonRenderTarget3D(_graphics, name, settings);

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
			
			BlendingState = new GorgonBlendRenderState(_graphics)
				{
					States = GorgonBlendStates.DefaultStates
				};
			DepthStencilState = new GorgonDepthStencilRenderState(_graphics)
				{
					States = GorgonDepthStencilStates.DefaultStates
				};
		}
		#endregion
	}
}
