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
		#region Variables.
		private readonly GorgonGraphics _graphics;
		private D3D.RenderTargetView[] _D3DViews;
		private GorgonRenderTargetView[] _targetViews;
		private GorgonDepthStencilView _depthView;
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
	    /// Function to validate the settings for a render target.
	    /// </summary>
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

				if (_graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM4)
				{
					throw new GorgonException(GorgonResult.CannotCreate, "Depth/stencil buffers cannot be bound to the shader pipeline with video devices that don't support SM4 or better.");
				}

				if ((_graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM4_1)
					&& ((settings.Multisampling.Count > 1) || (settings.Multisampling.Quality > 0)))
				{
					throw new GorgonException(GorgonResult.CannotCreate, "Multisampled Depth/stencil buffers cannot be bound to the shader pipeline if the video device does not support SM4_1 or better.");
				}
			}
		}
		
		/// <summary>
		/// Function to unbind a target with the specified resource and the depth stencil.
		/// </summary>
		/// <param name="resource">Resource to look up.</param>
		/// <param name="depthStencil">Depth/stencil to unbind.</param>
		internal void Unbind(GorgonResource resource, GorgonDepthStencilView depthStencil)
		{
			if ((_targetViews == null) || (_targetViews.Length == 0))
			{
				return;
			}

			var indices =
				_targetViews.Where(item => item != null && item.Resource == resource)
				            .Select(item => Array.IndexOf(_targetViews, item))
				            .Where(item => item != -1);

			foreach (var index in indices)
			{
				_targetViews[index] = null;
			}

            if (depthStencil == _depthView)
            {
				_depthView = null;
            }

			SetRenderTargets(_targetViews, _depthView);
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
			SetRenderTargets(_targetViews, _depthView);
		}

		/// <summary>
		/// Function to bind a target at a specific index.
		/// </summary>
		/// <param name="index">Index of the target to bind.</param>
		/// <param name="view">View to bind.</param>
		internal void BindTarget(int index, GorgonRenderTargetView view)
		{
			if ((_targetViews == null) || (index < 0) || (index >= _targetViews.Length))
			{
				return;
			}

			_targetViews[index] = view;
			SetRenderTargets(_targetViews, _depthView);
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
		/// Function to bind a a single render target view and a depth/stencil view to the pipeline.
		/// </summary>
		/// <param name="view">Render target view to set.</param>
		/// <param name="depthStencilView">[Optional] Depth/stencil view to set.</param>
		public void SetRenderTarget(GorgonRenderTargetView view, GorgonDepthStencilView depthStencilView = null)
		{
			D3D.DepthStencilView depthView = depthStencilView == null ? null : depthStencilView.D3DView;

#if DEBUG
            ValidateRenderTargetBinding(view, 0);
#endif

			if (view == null)
			{
				if ((_D3DViews == null) || (_D3DViews.Length != 1))
				{
					_D3DViews = null;
					_targetViews = null;
				}

				_graphics.Context.OutputMerger.SetTargets(depthView, _D3DViews);
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
			_graphics.Context.OutputMerger.SetTargets(depthView, _D3DViews[0]);
		}

		/// <summary>
		/// Function to bind a depth/stencil view and a list of render target views to the pipeline.
		/// </summary>
		/// <param name="views">List of render target views to set.</param>
		/// <param name="depthStencilBuffer">[Optional] Depth/stencil view to set.</param>
		public void SetRenderTargets(GorgonRenderTargetView[] views, GorgonDepthStencilView depthStencilBuffer = null)
		{
			D3D.DepthStencilView depthView = depthStencilBuffer == null ? null : depthStencilBuffer.D3DView;
			_depthView = depthStencilBuffer;

			// If we didn't pass any views, then unbind all the views.
			if ((views == null) || (views.Length == 0))
			{
				_graphics.Context.OutputMerger.SetTargets(depthView, (D3D.RenderTargetView[])null);
				_D3DViews = null;
				_targetViews = null;
				return;
			}

            _targetViews = views;

			// Update the current view list.
			if ((_D3DViews == null) || (views.Length != _D3DViews.Length))
			{
				_D3DViews = new D3D.RenderTargetView[views.Length];
			}

			// If we have more than one view to set, then blast them all out.
			for (int i = 0; i < views.Length; i++)
			{
				var view = views[i];

#if DEBUG
                ValidateRenderTargetBinding(view, i);
#endif

				if (view != null)
				{
					_D3DViews[i] = view.D3DView;
				}
				else
				{
					_D3DViews[i] = null;
				}
			}

#if DEBUG
            // Validate the depth/stencil buffer here because we need to have the current render target set before
            // evaluation.
            ValidateDepthBufferBinding(depthStencilBuffer);
#endif
			_graphics.Context.OutputMerger.SetTargets(depthView, _D3DViews);
		}

		/// <summary>
		/// Function to retrieve the list of render targets.
		/// </summary>
		/// <returns>An array of render targets.</returns>
		/// <remarks>This will return a copy of the internal render target list.  Use this method sparingly as it will generate garbage.</remarks>
		public GorgonRenderTargetView[] GetRenderTargets()
		{
			var result = new GorgonRenderTargetView[_targetViews == null ? 0 : _targetViews.Length];

			if (_targetViews != null)
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
				throw new InvalidOperationException("Cannot call DrawInstancedIndirect without a SM5 or better video device.");
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
		/// Function to create a depth/stencil buffer.
		/// </summary>
		/// <param name="name">Name of the depth/stencil buffer.</param>
		/// <param name="settings">Settings to apply to the depth/stencil buffer.</param>
		/// <param name="initialData">[Optional] Data used to initialize the depth buffer.</param>
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
		public GorgonDepthStencil2D CreateDepthStencil2D(string name, GorgonDepthStencil2DSettings settings, GorgonImageData initialData = null)
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
