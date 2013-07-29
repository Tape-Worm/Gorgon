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
// Created: Monday, February 13, 2012 7:48:30 AM
// 
#endregion

using System;
using System.Drawing;
using System.IO;
using GorgonLibrary.Math;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics.Properties;
using GorgonLibrary.IO;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// The base texture object for all textures.
	/// </summary>
	public abstract class GorgonTexture
		: GorgonResource
	{
		#region Variables.
	    private bool _disposed;                                 // Flag to indicate that the object was disposed.
		private GorgonViewCache _viewCache;						// Cache for views.
		private GorgonTextureShaderView _defaultShaderView;		// The default shader view for the texture.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return information about the format for the texture.
		/// </summary>
		public GorgonBufferFormatInfo.FormatData FormatInformation
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the size of the texture, in bytes.
		/// </summary>
		/// <remarks>This will take into account whether the texture is a packed format, or compressed.</remarks>
		public override int SizeInBytes
		{
			get
			{
				return GorgonImageData.GetSizeInBytes(Settings);
			}
		}

		/// <summary>
		/// Property to set or return the settings for the texture.
		/// </summary>
		public ITextureSettings Settings
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the mapping mode for locking a buffer.
		/// </summary>
		/// <param name="flags">Flags to use when locking.</param>
		/// <returns>The D3D mapping mode.</returns>
		private static D3D.MapMode GetMapMode(BufferLockFlags flags)
		{
			if ((flags & BufferLockFlags.Read) == BufferLockFlags.Read)
			{
				return (flags & BufferLockFlags.Write) == BufferLockFlags.Write ? D3D.MapMode.ReadWrite : D3D.MapMode.Read;
			}

			if ((flags & BufferLockFlags.Discard) == BufferLockFlags.Discard)
			{
				return D3D.MapMode.WriteDiscard;
			}

			return (flags & BufferLockFlags.NoOverwrite) == BufferLockFlags.NoOverwrite
				? D3D.MapMode.WriteNoOverwrite
				: D3D.MapMode.Write;
		}

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <exception cref="System.NotSupportedException">Thrown when the texture is attached to another object type.</exception>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Gorgon.Log.Print("Gorgon texture {0}: Unbound from shaders.", LoggingLevel.Verbose, Name);
                    Graphics.Shaders.UnbindResource(this);

                    // Unbind the view(s).
                    if (_viewCache != null)
                    {
                        _viewCache.Dispose();
                        _viewCache = null;
                    }
                }
                
                _disposed = true;
            }
            
            base.Dispose(disposing);
        }

	    /// <summary>
	    /// Function to create the resource objects.
	    /// </summary>
	    /// <returns>The new default shader view.</returns>
	    protected virtual void CreateDefaultResourceView()
	    {
	        if (Settings.Usage == BufferUsage.Staging)
            {
                return;
            }

	        BufferFormat format = Settings.ShaderViewFormat == BufferFormat.Unknown ? Settings.Format : Settings.ShaderViewFormat;

		    _defaultShaderView = OnGetShaderView(format, 0, Settings.MipCount, 0,
		                                         Settings.ImageType == ImageType.Image3D
			                                         ? Settings.Depth
			                                         : Settings.ArrayCount);
	    }

		/// <summary>
		/// Function to retrieve a new depth/stencil view object.
		/// </summary>
		/// <param name="format">The format of the depth/stencil view.</param>
		/// <param name="mipSlice">Starting mip map for the view.</param>
		/// <param name="arrayStart">Starting array index for the view.</param>
		/// <param name="arrayCount">Array index count for the view.</param>
		/// <param name="flags">Flags to determine how to treat the bound view.</param>
		/// <remarks>Use a depth/stencil view to bind a resource to the pipeline as a depth/stencil buffer.  A depth/stencil view can view a select portion of the texture, and the view <paramref name="format"/> can be used to 
		/// cast the format of the texture into another type (as long as the view format has the same bit depth as the depth/stencil format).  For example, a texture with a format of D32_Float could be cast 
		/// to R32_Uint, R32_Int or R32_Float formats.
        /// <para>The <paramref name="flags"/> parameter will allow the depth/stencil buffer to be read simultaneously from the depth/stencil view and from a shader view.  It is not normally possible to bind a view of a 
        /// resource to 2 parts of the pipeline at the same time.  However, using the flags provided, read-only access may be granted to a part of the resource (depth or stencil) or all of it for all parts of the pipline.  
        /// This would bind the depth/stencil as a read-only view and make it a read-only view accessible to shaders. If the flags are not set to None, then the depth/stencil buffer must allow shader access.</para>
        /// <para>Binding to simulatenous views require a video device with a feature level of SM5 or better.</para>
        /// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture has a usage of staging.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="format"/> is not valid for the view.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="arrayStart"/> and the <paramref name="arrayCount"/> are less than 0 or 1 respectively, or greater than the number of array indices in the texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="mipSlice"/> is less than 0 or greater than the number of mip levels in the texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="flags"/> property is not set to None and the depth buffer does not allow shader access, or if the current video device feature level is not SM5 or better.</para>
		/// </exception>
		/// <returns>A texture shader view object.</returns>
		protected GorgonDepthStencilView OnGetDepthStencilView(BufferFormat format,
													int mipSlice,
													int arrayStart,
													int arrayCount,
                                                    DepthStencilViewFlags flags)
		{
			if (Settings.Usage == BufferUsage.Staging)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_DRV_NO_STAGING);
			}

			if (format == BufferFormat.Unknown)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_UNKNOWN_FORMAT);
			}

			var formatInformation = GorgonBufferFormatInfo.GetInfo(format);

			if (formatInformation.IsTypeless)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_NO_TYPELESS);
			}


			if ((Settings.Format != format) && (FormatInformation.BitDepth != formatInformation.BitDepth))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
										  string.Format(Resources.GORGFX_VIEW_CANNOT_CAST_FORMAT,
														Settings.Format,
														format));
			}

			// 3D textures can't be depth/stencil buffers.
			if (ResourceType == ResourceType.Texture3D)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_CANNOT_BIND_UNKNOWN_RESOURCE);
			}

			if ((mipSlice >= Settings.MipCount)
				|| (mipSlice < 0))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
										  string.Format(Resources.GORGFX_VIEW_MIP_START_INVALID,
														Settings.MipCount));
			}

			if ((arrayCount > Settings.ArrayCount)
				|| (arrayCount + arrayStart > Settings.ArrayCount)
				|| (arrayCount < 1))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
										  string.Format(Resources.GORGFX_VIEW_ARRAY_COUNT_INVALID,
														Settings.ArrayCount));
			}

			if ((arrayStart >= Settings.ArrayCount)
				|| (arrayStart < 0))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
										  string.Format(Resources.GORGFX_VIEW_ARRAY_START_INVALID,
														Settings.ArrayCount));
			}

			if ((Settings.IsTextureCube)
				&& ((arrayCount % 6) != 0))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_CUBE_ARRAY_SIZE_INVALID);
			}

		    return _viewCache.GetDepthStencilView(format, mipSlice, arrayStart, arrayCount, flags);
		}

	    /// <summary>
	    /// Function to retrieve a new shader resource view object.
	    /// </summary>
	    /// <param name="format">The format of the resource view.</param>
	    /// <param name="mipStart">Starting mip map for the view.</param>
	    /// <param name="mipCount">Mip map count for the view.</param>
	    /// <param name="arrayStart">Starting array index for the view.</param>
	    /// <param name="arrayCount">Array index count for the view.</param>
        /// <remarks>Use a shader view to access a texture from a shader.  A shader view can view a select portion of the texture, and the view <paramref name="format"/> can be used to 
        /// cast the format of the texture into another type (as long as the view format is in the same group as the texture format).  For example, a texture with a format of R8G8B8A8 could be cast 
        /// to R8G8B8A8_UInt_Normal, or R8G8B8A8_UInt or any of the other R8G8B8A8 formats.
        /// <para>Multiple views of the texture can be bound to different parts of the shader pipeline.</para>
        /// <para>Textures that have a usage of staging cannot create shader views.</para>
        /// </remarks>
	    /// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture has a usage of staging.
	    /// <para>-or-</para>
	    /// <para>Thrown when the <paramref name="format"/> is not valid for the view.</para>
	    /// <para>-or-</para>
	    /// <para>Thrown when the <paramref name="arrayStart"/> and the <paramref name="arrayCount"/> are less than 0 or 1 respectively, or greater than the number of array indices in the texture.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="mipStart"/> and the <paramref name="mipCount"/> are less than 0 or 1 respectively, or greater than the number of mip levels in the texture.</para>
	    /// </exception>
	    /// <returns>A texture shader view object.</returns>
	    protected GorgonTextureShaderView OnGetShaderView(BufferFormat format,
                                                    int mipStart,
                                                    int mipCount,
                                                    int arrayStart,
                                                    int arrayCount)
        {
            if (Settings.Usage == BufferUsage.Staging)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_SRV_NO_STAGING_OR_DYNAMIC);
            }

            if (format == BufferFormat.Unknown)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_UNKNOWN_FORMAT);
            }

	        var formatInformation = GorgonBufferFormatInfo.GetInfo(format);

            if (formatInformation.IsTypeless)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_NO_TYPELESS);
            }


            if ((Settings.Format != format) && (FormatInformation.Group != formatInformation.Group))
            {
                throw new GorgonException(GorgonResult.CannotCreate,
                                          string.Format(Resources.GORGFX_VIEW_FORMAT_GROUP_INVALID,
                                                        Settings.Format,
                                                        format));
            }

            // 3D textures don't use arrays.
            if (ResourceType != ResourceType.Texture3D)
            {
                if ((arrayCount > Settings.ArrayCount)
                    || (arrayCount + arrayStart > Settings.ArrayCount)
                    || (arrayCount < 1))
                {
                    throw new GorgonException(GorgonResult.CannotCreate,
                                              string.Format(Resources.GORGFX_VIEW_ARRAY_COUNT_INVALID,
                                                            Settings.ArrayCount));
                }

                if ((arrayStart >= Settings.ArrayCount)
                    || (arrayStart < 0))
                {
                    throw new GorgonException(GorgonResult.CannotCreate,
                                              string.Format(Resources.GORGFX_VIEW_ARRAY_START_INVALID,
                                                            Settings.ArrayCount));
                }
            }

            if ((mipCount > Settings.MipCount) || (mipStart + mipCount > Settings.MipCount)
                || (mipCount < 1))
            {
                throw new GorgonException(GorgonResult.CannotCreate,
                                          string.Format(Resources.GORGFX_VIEW_MIP_COUNT_INVALID,
                                                        Settings.MipCount));
            }

            if ((mipStart >= Settings.MipCount)
                || (mipStart < 0))
            {
                throw new GorgonException(GorgonResult.CannotCreate,
                                          string.Format(Resources.GORGFX_VIEW_MIP_START_INVALID,
                                                        Settings.MipCount));
            }

            if ((Settings.IsTextureCube)
                && ((arrayCount % 6) != 0))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_CUBE_ARRAY_SIZE_INVALID);
            }

            return _viewCache.GetTextureView(format, mipStart, mipCount, arrayStart, arrayCount);
        }

		/// <summary>
		/// Function to retrieve a new render target view.
		/// </summary>
		/// <param name="format">Format of the new render target view.</param>
		/// <param name="mipSlice">Mip level index to use in the view.</param>
		/// <param name="arrayOrDepthIndex">Array or depth slice index to use in the view.</param>
		/// <param name="arrayOrDepthCount">Number of array indices or depth slices to use.</param>
		/// <returns>A render target view.</returns>
		/// <remarks>Use this to create/retrieve a render target view that can bind a portion of the target to the pipeline as a render target.
		/// <para>The <paramref name="format"/> for the render target view does not have to be the same as the render target backing texture, and if the format is set to Unknown, then it will 
		/// use the format from the texture.</para>
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the render target view could not be created.</exception>
		protected GorgonRenderTargetTextureView OnGetRenderTargetView(BufferFormat format, int mipSlice, int arrayOrDepthIndex,
															   int arrayOrDepthCount)
		{
			// If we pass unknown, use the format from the texture.
			if (format == BufferFormat.Unknown)
			{
				format = ((IRenderTargetTextureSettings)Settings).TextureFormat;
			}
			
			if (Settings.ImageType != ImageType.Image3D)
			{
				if ((arrayOrDepthIndex >= Settings.ArrayCount)
					|| (arrayOrDepthIndex < 0))
				{
					throw new GorgonException(GorgonResult.CannotCreate,
											  string.Format(Resources.GORGFX_VIEW_ARRAY_START_INVALID,
															Settings.ArrayCount));
				}

				if ((arrayOrDepthCount > Settings.ArrayCount)
				    || (arrayOrDepthCount + arrayOrDepthIndex > Settings.ArrayCount)
				    || (arrayOrDepthCount < 1))
				{
					throw new GorgonException(GorgonResult.CannotCreate,
					                          string.Format(Resources.GORGFX_VIEW_ARRAY_COUNT_INVALID,
					                                        Settings.ArrayCount));
				}
			}
			else
			{
				if ((arrayOrDepthIndex >= Settings.Depth)
					|| (arrayOrDepthIndex < 0))
				{
					throw new GorgonException(GorgonResult.CannotCreate,
											  string.Format(Resources.GORGFX_VIEW_DEPTH_START_INVALID,
															Settings.Depth));
				}

				if ((arrayOrDepthCount > Settings.Depth)
					|| (arrayOrDepthCount + arrayOrDepthIndex > Settings.Depth)
					|| (arrayOrDepthCount < 1))
				{
					throw new GorgonException(GorgonResult.CannotCreate,
											  string.Format(Resources.GORGFX_VIEW_DEPTH_COUNT_INVALID,
															Settings.Depth));
				}
				
			}

			if ((mipSlice < 0) && (mipSlice >= Settings.MipCount))
			{
				throw new GorgonException(GorgonResult.CannotCreate, 
														string.Format(Resources.GORGFX_VIEW_MIP_START_INVALID,
                                                        Settings.MipCount));
			}

			if ((mipSlice < 0) && (mipSlice >= Settings.MipCount))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
														string.Format(Resources.GORGFX_VIEW_MIP_START_INVALID,
														Settings.MipCount));
			}

			var info = GorgonBufferFormatInfo.GetInfo(format);

			if (info.IsTypeless)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_NO_TYPELESS);
			}

			return (GorgonRenderTargetTextureView)_viewCache.GetRenderTargetView(format, mipSlice, arrayOrDepthIndex, arrayOrDepthCount);
		}

		/// <summary>
		/// Function to retrieve an unordered access view for this texture.
		/// </summary>
		/// <param name="format">Format of the buffer.</param>
		/// <param name="mipStart">First mip map level to map to the view.</param>
		/// <param name="arrayStart">The first array index to map to the view.</param>
		/// <param name="arrayCount">The number of array indices to map to the view.</param>
		/// <returns>A new unordered access view for the texture.</returns>
		/// <remarks>Use this to create/retrieve an unordered access view that will allow shaders to access the view using multiple threads at the same time.  Unlike a shader view, only one 
		/// unordered access view can be bound to the pipeline at any given time.
		/// <para>Unordered access views require a video device feature level of SM_5 or better.</para>
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the usage for this texture is set to Staging.
		/// <para>-or-</para>
		/// <para>Thrown when the video device feature level is not SM_5 or better.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the resource settings do not allow unordered access views.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the view could not be created.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="mipStart"/>, <paramref name="arrayStart"/> or <paramref name="arrayCount"/> parameters are less than 0 or greater than or equal to the 
		/// number of mip levels and/or array levels in the texture.
		/// <para>-or-</para>
		/// <para>Thrown if the bit count of the <paramref name="format"/> and the texture format are different, or if format is not in the R32 group and is not in the same group as the texture format.</para>
		/// </exception>
		protected GorgonTextureUnorderedAccessView OnGetUnorderedAccessView(BufferFormat format, int mipStart, int arrayStart, int arrayCount)
		{
			if (Graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM5));
			}

            if (!Settings.AllowUnorderedAccessViews)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_VIEW_NO_SUPPORT, "GorgonUnorderedAccessView"));
            }

			if (!Graphics.VideoDevice.SupportsUnorderedAccessViewFormat(format))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
										  string.Format(Resources.GORGFX_VIEW_FORMAT_NOT_SUPPORTED, format));
			}
			
			// Ensure the size of the data type fits the requested format.
			var info = GorgonBufferFormatInfo.GetInfo(format);

			if (((info.Group != BufferFormat.R32) && (FormatInformation.Group != info.Group))
				|| (info.SizeInBytes != FormatInformation.SizeInBytes))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
										  string.Format(Resources.GORGFX_VIEW_FORMAT_GROUP_INVALID,
														Settings.Format,
														format));
			}

		    return (GorgonTextureUnorderedAccessView)_viewCache.GetUnorderedAccessView(format,
		                                                                               mipStart,
		                                                                               arrayStart,
		                                                                               arrayCount,
		                                                                               UnorderedAccessViewType.Standard,
		                                                                               false);
        }

		/// <summary>
		/// Function to clean up the resource object.
		/// </summary>
		protected override void CleanUpResource()
		{
			Gorgon.Log.Print("Gorgon texture {0}: Destroying D3D 11 texture resource.", LoggingLevel.Verbose, Name);

		    if (D3DResource == null)
		    {
		        return;
		    }

			// Release any cached resources.  Otherwise we'll be hold on to invalid resources.
			// This should only happen when we update a swap chain.
			if (_viewCache != null)
			{
				_viewCache.Clear();
			}

			GorgonRenderStatistics.TextureCount--;
		    GorgonRenderStatistics.TextureSize -= SizeInBytes;
		    D3DResource.Dispose();
		    D3DResource = null;
		}

		/// <summary>
		/// Function to return sub resource data for a lock operation.
		/// </summary>
		/// <param name="dataStream">Stream containing the data.</param>
		/// <param name="rowPitch">The number of bytes per row of the texture.</param>
		/// <param name="slicePitch">The number of bytes per depth slice of the texture.</param>
		/// <param name="context">The graphics context to use when locking the texture.</param>
		/// <returns>The sub resource data.</returns>
        /// <remarks>The <paramref name="context"/> allows a separate thread to access the resource at the same time as another thread.</remarks>
		protected abstract ISubResourceData OnGetLockSubResourceData(GorgonDataStream dataStream, int rowPitch, int slicePitch, GorgonGraphics context);

		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
		/// <param name="subResource">Sub resource index to use.</param>
		/// <param name="context">The graphics context to use when updating the texture.</param>
		/// <remarks>The <paramref name="context"/> allows a separate thread to access the resource at the same time as another thread.</remarks>
		protected abstract void OnUpdateSubResource(ISubResourceData data, int subResource, GorgonGraphics context);

		/// <summary>
		/// Function to copy this texture into a new staging texture.
		/// </summary>
		/// <returns>The new staging texture.</returns>
		protected abstract GorgonTexture OnGetStagingTexture();

		/// <summary>
		/// Function to create an image with initial data.
		/// </summary>
		/// <param name="initialData">Data to use when creating the image.</param>
		/// <remarks>The <paramref name="initialData"/> can be NULL (Nothing in VB.Net) IF the texture is not created with an Immutable usage flag.
		/// <para>To initialize the texture, create a new <see cref="GorgonLibrary.Graphics.GorgonImageData">GorgonImageData</see> object and fill it with image information.</para>
		/// </remarks>
		protected abstract void OnInitialize(GorgonImageData initialData);

        /// <summary>
        /// Funtion to retrieve the binding flags for a resource.
        /// </summary>
        /// <param name="isDepth">TRUE if the texture is meant for depth/stencil.</param>
        /// <param name="isTarget">TRUE if the texture is meant for use as a render target.</param>
        /// <returns>The Direct3D binding flags.</returns>
        internal D3D.BindFlags GetBindFlags(bool isDepth, bool isTarget)
        {
            var flags = D3D.BindFlags.None;

            if (!isDepth)
            {
                if (Settings.Usage != BufferUsage.Staging)
                {
                    flags |= D3D.BindFlags.ShaderResource;
                }

                if (Settings.AllowUnorderedAccessViews)
                {
                    flags |= D3D.BindFlags.UnorderedAccess;
                }

                if (isTarget)
                {
                    flags |= D3D.BindFlags.RenderTarget;
                }
            }
            else
            {
                flags |= D3D.BindFlags.DepthStencil;
            }

            return flags;
        }

		/// <summary>
		/// Function to create an image with initial data.
		/// </summary>
		/// <param name="initialData">Data to use when creating the image.</param>
		/// <remarks>The initial data can be a <see cref="GorgonLibrary.IO.GorgonDataStream">GorgonDataStream</see>, <see cref="GorgonLibrary.Graphics.GorgonTexture2DData">GorgonTexture2DData</see> or <see cref="GorgonLibrary.Graphics.GorgonTexture3DData">GorgonTexture3DData</see></remarks>
		internal void Initialize(GorgonImageData initialData)
		{
			try
			{
				Gorgon.Log.Print("{0} {1}: Creating D3D11 texture resource...", LoggingLevel.Verbose, GetType().Name, Name);
                
				OnInitialize(initialData);
				CreateDefaultResourceView();

				GorgonRenderStatistics.TextureCount++;
				GorgonRenderStatistics.TextureSize += SizeInBytes;
			}
			catch
			{
				if (D3DResource != null)
					D3DResource.Dispose();
				D3DResource = null;
				throw;
			}
		}

        /// <summary>
        /// Function to convert this texture into an array of System.Drawing.Images.
        /// </summary>
        /// <returns>A list of <see cref="System.Drawing.Image"/> image objects.</returns>
        public Image[] ToGDIImage()
        {
            return GorgonGDIImageConverter.CreateGDIImagesFromTexture(this);
        }

		/// <summary>
		/// Function to copy this texture into a staging texture.
		/// </summary>
		/// <returns>A new staging texture.</returns>
        /// <remarks>If the current video device has a feature level of SM2_a_b, and this texture is not a staging texture, then an exception will be thrown.</remarks>
        /// <exception cref="System.NotSupportedException">Thrown when a device with a feature level of SM2_a_b tries to create a staging texture from a texture that isn't already a staging texture.</exception>
		public T GetStagingTexture<T>()
			where T : GorgonTexture
		{
#if DEBUG
            if ((Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) && (Settings.Usage != BufferUsage.Staging))
            {
                throw new NotSupportedException(string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM4));
            }

            if (Settings.Usage == BufferUsage.Immutable)
            {
                throw new NotSupportedException(Resources.GORGFX_TEXTURE_IMMUTABLE);    
            }
#endif
			return (T)OnGetStagingTexture();
		}

        /// <summary>
        /// Function to save the texture to a byte array.
        /// </summary>
        /// <param name="codec">Codec used to encode the stream data.</param>
        /// <returns>A byte array containing the texture data.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="codec"/> parameter is NULL (Nothing in VB.Net).</exception>		
        /// <exception cref="System.IO.IOException">Thrown when there is an error when attempting to encode the image data.</exception>
        /// <exception cref="System.NotSupportedException">Thrown when the current video device has a feature level of SM2_a_b and the texture is not a staging texture or the texture is not 2D.</exception>
        /// <remarks>This will persist the contents of the texture into an array of bytes.  The data is encoded into various formats via the codec parameter.  Gorgon contains a 
        /// number of built-in codecs.  Currently, Gorgon supports the following formats:
        /// <list type="bullet">
        ///		<item>
        ///			<description>DDS</description>
        ///		</item>
        ///		<item>
        ///			<description>TGA</description>
        ///		</item>
        ///		<item>
        ///			<description>PNG (WIC)</description>
        ///		</item>
        ///		<item>
        ///			<description>BMP (WIC)</description>
        ///		</item>
        ///		<item>
        ///			<description>JPG (WIC)</description>
        ///		</item>
        ///		<item>
        ///			<description>WMP (WIC)</description>
        ///		</item>
        ///		<item>
        ///			<description>TIF (WIC)</description>
        ///		</item>
        /// </list>
        /// <para>The items with (WIC) indicate that the codec support is supplied by the Windows Imaging Component.  This component should be installed on most systems, but if it is not 
        /// then it is required in order to read/save the files in those formats.</para>
        /// <para>Note that devices with a feature level of SM2_a_b cannot save textures that don't have a usage of staging.  Also, these devices will only save 2D staging textures.  Attempting to 
        /// save 3D and 1D textures will throw an exception.</para>
        /// </remarks>
        public byte[] Save(GorgonImageCodec codec)
        {
            using (var memoryStream = new MemoryStream())
            {
                Save(memoryStream, codec);
                memoryStream.Position = 0;

                return memoryStream.ToArray();
            }
        }

		/// <summary>
		/// Function to save the texture to a stream with the specified codec.
		/// </summary>
		/// <param name="stream">Stream that will contain the texture information.</param>
		/// <param name="codec">Codec used to encode the stream data.</param>        
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> or the <paramref name="codec"/> parameter is NULL (Nothing in VB.Net).</exception>		
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.
		/// <para>-or-</para>
		/// <para>Thrown when there is an error when attempting to encode the image data.</para>
		/// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when the current video device has a feature level of SM2_a_b and the texture is not a staging texture or the texture is not 2D.</exception>
		/// <remarks>This will persist the contents of the texture into a stream.  The data is encoded into various formats via the codec parameter.  Gorgon contains a 
		/// number of built-in codecs.  Currently, Gorgon supports the following formats:
		/// <list type="bullet">
		///		<item>
		///			<description>DDS</description>
		///		</item>
		///		<item>
		///			<description>TGA</description>
		///		</item>
		///		<item>
		///			<description>PNG (WIC)</description>
		///		</item>
		///		<item>
		///			<description>BMP (WIC)</description>
		///		</item>
		///		<item>
		///			<description>JPG (WIC)</description>
		///		</item>
		///		<item>
		///			<description>WMP (WIC)</description>
		///		</item>
		///		<item>
		///			<description>TIF (WIC)</description>
		///		</item>
		/// </list>
		/// <para>The items with (WIC) indicate that the codec support is supplied by the Windows Imaging Component.  This component should be installed on most systems, but if it is not 
		/// then it is required in order to read/save the files in those formats.</para>
        /// <para>Note that devices with a feature level of SM2_a_b cannot save textures that don't have a usage of staging.  Also, these devices will only save 2D staging textures.  Attempting to 
        /// save 3D and 1D textures will throw an exception.</para>
		/// </remarks>
		public void Save(Stream stream, GorgonImageCodec codec)
		{
            if ((Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) 
				&& ((Settings.Usage != BufferUsage.Staging)
					|| (Settings.ImageType != ImageType.Image2D)))
            {
                throw new NotSupportedException(string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM4));
            }

            using (var textureData = GorgonImageData.CreateFromTexture(this))
			{
				textureData.Save(stream, codec);
			}
		}

		/// <summary>
		/// Function to save the image data to a file with the specified codec.
		/// </summary>
		/// <param name="filePath">Path to the file.</param>
		/// <param name="codec">Codec used to encode the file data.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="filePath"/> or the <paramref name="codec"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the filePath parameter is empty.</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.
		/// <para>-or-</para>
		/// <para>Thrown when there is an error when attempting to encode the image data.</para>
		/// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when the current video device has a feature level of SM2_a_b and the texture is not a staging texture or the texture is not 2D.</exception>
		/// <remarks>This will persist the contents of the image data object into a stream.  The data is encoded into various formats via the codec parameter.  Gorgon contains a 
		/// number of built-in codecs.  Currently, Gorgon supports the following formats:
		/// <list type="bullet">
		///		<item>
		///			<description>DDS</description>
		///		</item>
		///		<item>
		///			<description>TGA</description>
		///		</item>
		///		<item>
		///			<description>PNG (WIC)</description>
		///		</item>
		///		<item>
		///			<description>BMP (WIC)</description>
		///		</item>
		///		<item>
		///			<description>JPG (WIC)</description>
		///		</item>
		///		<item>
		///			<description>WMP (WIC)</description>
		///		</item>
		///		<item>
		///			<description>TIF (WIC)</description>
		///		</item>
		/// </list>
		/// <para>The items with (WIC) indicate that the codec support is supplied by the Windows Imaging Component.  This component should be installed on most systems, but if it is not 
		/// then it is required in order to read/save the files in those formats.</para>
        /// <para>Note that devices with a feature level of SM2_a_b cannot save textures that don't have a usage of staging.  Also, these devices will only save 2D staging textures.  Attempting to 
        /// save 3D and 1D textures will throw an exception.</para>
        /// </remarks>
		public void Save(string filePath, GorgonImageCodec codec)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "filePath");
			}

			if ((Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
				&& ((Settings.Usage != BufferUsage.Staging)
					|| (Settings.ImageType != ImageType.Image2D)))
			{
				throw new NotSupportedException(string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM4));
			}

			using (FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
				Save(stream, codec);
			}
		}

		/// <summary>
		/// Function to copy another texture into this texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <param name="deferred">[Optional] The deferred context to use when copying.</param>
		/// <remarks>
		/// This overload will copy the -entire- texture, including mipmaps, array levels, etc...  Use CopySubResource to copy a portion of the texture.
		/// <para>This method will -not- perform stretching, filtering or clipping.</para>
		/// <para>The <paramref name="texture"/> dimensions must be have the same dimensions as this texture.  If they do not, an exception will be thrown.</para>
		/// <para>If the this texture is multisampled, then the <paramref name="texture"/> must use the same multisampling parameters.</para>
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// <para>SM2_a_b devices may copy 2D textures, but there are format restrictions (must be compatible with a render target format).  3D textures can only be copied to textures that are in GPU memory, if either texture is a staging texture, then an exception will be thrown.</para>
		/// <para>If <paramref name="deferred"/> is NULL (Nothing in VB.Net), then the immediate context is used to copy the texture.  Specify a deferred context when accessing the resource is being accessed by a separate thread.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the multisampling count is not the same for the source texture and this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture sizes are not the same.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture types are not the same.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// <para>-or-</para>
		/// <para>Thrown if this texture is a 3D texture and is in CPU accessible memory and the video device is a SM2_a_b device.</para>
		/// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when the video device has a feature level of SM2_a_b, and the source texture 
        /// is not a staging texture and the this texture is a staging texture, or if the textures are 1D textures and neither texture is a staging texture.</exception>
		public void Copy(GorgonTexture texture, GorgonGraphics deferred = null)
		{
			GorgonDebug.AssertNull(texture, "texture");

#if DEBUG            
            if (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
            {
                if ((texture.Settings.Usage != BufferUsage.Staging) && (Settings.Usage == BufferUsage.Staging))
                {
                    throw new NotSupportedException(string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM4));
                }

                if ((texture.Settings.ImageType == ImageType.Image1D) && (Settings.Usage != BufferUsage.Staging) && (texture.Settings.Usage != BufferUsage.Staging))
                {
					throw new NotSupportedException(string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM4));
                }
            }

			if (texture.ResourceType != ResourceType)
			{
				throw new ArgumentException(
					string.Format(Resources.GORGFX_TEXTURE_NOT_SAME_TYPE, texture.Name, texture.ResourceType, ResourceType), "texture");
			}

			if (Settings.Usage == BufferUsage.Immutable)
			{
				throw new InvalidOperationException(Resources.GORGFX_TEXTURE_IMMUTABLE);
			}

		    if ((Settings.Multisampling.Count != texture.Settings.Multisampling.Count)
		        || (Settings.Multisampling.Quality != texture.Settings.Multisampling.Quality))
		    {
		        throw new InvalidOperationException(Resources.GORGFX_TEXTURE_MULTISAMPLE_PARAMS_MISMATCH);
		    }

			// If the format is different, then check to see if the format group is the same.
		    if ((texture.Settings.Format != Settings.Format)
		        && ((texture.FormatInformation.Group != FormatInformation.Group)
		            || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
		            || (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4)))
		    {
		        throw new ArgumentException(
		            string.Format(Resources.GORGFX_TEXTURE_COPY_CANNOT_CONVERT, texture.Settings.Format, Settings.Format),
		            "texture");
		    }

		    if ((texture.Settings.Width != Settings.Width)
		        || (texture.Settings.Height != Settings.Height))
		    {
		        throw new ArgumentException(Resources.GORGFX_TEXTURE_MUST_BE_SAME_SIZE, "texture");
		    }

		    // Ensure that the SM2_a_b devices don't try and copy between CPU and GPU accessible memory.
		    if ((this is GorgonTexture3D)
		        && (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
		        && (Settings.Usage == BufferUsage.Staging))
		    {
                throw new InvalidOperationException(string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM4));
		    }
#endif
            if (deferred == null)
            {
                deferred = Graphics;
            }

			deferred.Context.CopyResource(texture.D3DResource, D3DResource);
		}

		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
		/// <param name="subResource">Sub resource index to use.</param>
		/// <param name="deferred">[Optional] The deferred graphics context to use when updating the sub resource.</param>
		/// <remarks>Use this to copy data to this texture.
        /// <para>If <paramref name="deferred"/> is NULL (Nothing in VB.Net), then the immediate context is used to update the texture.  Specify a deferred context when accessing the resource is being accessed by a separate thread.</para>
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture has an Immutable, Dynamic or a Staging usage.
		/// <para>-or-</para>
		/// <para>Thrown when this texture has multisampling applied.</para>
		/// <para>-or-</para>
		/// <para>Thrown if this texture is a depth/stencil buffer texture.</para>
		/// </exception>
		public void UpdateSubResource(ISubResourceData data, int subResource, GorgonGraphics deferred = null)
		{
#if DEBUG
		    if ((Settings.Usage == BufferUsage.Dynamic)
		        || (Settings.Usage == BufferUsage.Immutable))
		    {
		        throw new InvalidOperationException(Resources.GORGFX_TEXTURE_IS_DYNAMIC_IMMUTABLE);
		    }

		    if ((Settings.Multisampling.Count > 1)
		        || (Settings.Multisampling.Quality > 0))
		    {
		        throw new InvalidOperationException(Resources.GORGFX_TEXTURE_MULTISAMPLED);
		    }
#endif
            if (deferred == null)
            {
                deferred = Graphics;
            }

			OnUpdateSubResource(data, subResource, deferred);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="destBox"></param>
        /// <param name="destArrayIndex"></param>
        /// <param name="destMipLevel"></param>
        /// <param name="deferred"></param>
	    public void UpdateSubResource(GorgonImageBuffer buffer,
	        GorgonBox destBox,
	        int? destArrayIndex = null,
	        int? destMipLevel = null,
            GorgonGraphics deferred = null)
	    {
#if DEBUG
            if ((Settings.Usage == BufferUsage.Dynamic)
                || (Settings.Usage == BufferUsage.Immutable))
            {
                throw new InvalidOperationException(Resources.GORGFX_TEXTURE_IS_DYNAMIC_IMMUTABLE);
            }

            if ((Settings.Multisampling.Count > 1)
                || (Settings.Multisampling.Quality > 0))
            {
                throw new InvalidOperationException(Resources.GORGFX_TEXTURE_MULTISAMPLED);
            }

            if ((destArrayIndex != null)
                && ((destArrayIndex < 0) || (destArrayIndex >= Settings.ArrayCount)))
            {
                throw new ArgumentOutOfRangeException("destArrayIndex",
                    string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, destArrayIndex, 0, Settings.ArrayCount));
            }

            if ((destMipLevel != null)
                && ((destMipLevel < 0) || (destMipLevel >= Settings.MipCount)))
            {
                throw new ArgumentOutOfRangeException("destMipLevel",
                    string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, destMipLevel, 0, Settings.MipCount));
            }
#endif
            // Ensure the box fits the source.
            destBox = new GorgonBox
            {
                Front = destBox.Front.Max(0).Min(buffer.Depth - 1).Min(Settings.Depth - 1),
                Left = destBox.Left.Max(0).Min(buffer.Width - 1).Min(Settings.Width - 1),
                Top = destBox.Top.Max(0).Min(buffer.Height - 1).Min(Settings.Height - 1),
                Depth = destBox.Depth.Max(1).Min(buffer.Depth).Min(Settings.Depth),
                Width = destBox.Width.Max(1).Min(buffer.Width).Min(Settings.Width),
                Height = destBox.Height.Max(1).Min(buffer.Height).Min(Settings.Height)
            };

            if (destArrayIndex == null)
            {
                destArrayIndex = buffer.ArrayIndex;
            }

            if (destMipLevel == null)
            {
                destMipLevel = buffer.MipLevel;
            }

            if (deferred == null)
            {
                deferred = Graphics;
            }

            var box = new DX.DataBox
            {
                DataPointer = buffer.Data.BasePointer,
                RowPitch = buffer.PitchInformation.RowPitch,
                SlicePitch = buffer.PitchInformation.SlicePitch
            };

            var region = destBox.Convert;

            if (!deferred.IsDeferred)
            {
                deferred.Context.UpdateSubresource(box,
                    D3DResource,
                    D3D.Resource.CalculateSubResourceIndex(destMipLevel.Value, destArrayIndex.Value, Settings.MipCount),
                    region);
            }
            else
            {
                deferred.Context.UpdateSubresourceSafe(box,
                    D3DResource,
                    FormatInformation.SizeInBytes,
                    D3D.Resource.CalculateSubResourceIndex(destMipLevel.Value, destArrayIndex.Value, Settings.MipCount),
                    region,
                    FormatInformation.IsCompressed);
            }
	    }

		/// <summary>
		/// Function to copy data from the CPU to a texture.
		/// </summary>
		/// <param name="data">Data to copy to the texture.</param>
        /// <param name="deferred">[Optional] The deferred graphics context to use when updating the sub resource.</param>
		/// <remarks>Use this to copy data to this texture.
        /// <para>If <paramref name="deferred"/> is NULL (Nothing in VB.Net), then the immediate context is used to update the texture.  Specify a deferred context when accessing the resource is being accessed by a separate thread.</para>
		/// </remarks>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture has an Immutable, Dynamic or a Staging usage.
		/// <para>-or-</para>
		/// <para>Thrown when this texture has multisampling applied.</para>
		/// <para>-or-</para>
		/// <para>Thrown if this texture is a depth/stencil buffer texture.</para>
		/// </exception>
		public void UpdateSubResource(ISubResourceData data, GorgonGraphics deferred = null)
		{
			UpdateSubResource(data, 0, deferred);
		}

        /// <summary>
		/// Function to lock a CPU accessible texture sub resource for reading/writing.
		/// </summary>
		/// <param name="subResource">Sub resource to lock.</param>
		/// <param name="lockFlags">Flags used to lock.</param>
		/// <param name="deferred">[Optional] The deferred graphics context used to lock the texture.</param>
		/// <returns>A stream used to write to the texture.</returns>
		/// <remarks>When locking a texture, the entire texture sub resource is locked and returned.  There is no setting to return a portion of the texture subresource.
		/// <para>This method is only available to textures created with a staging or dynamic usage setting.  Otherwise an exception will be raised.</para>
		/// <para>The NoOverwrite flag is not valid with texture locking and will be ignored.</para>
		/// <para>If the texture is not a staging texture and Read is specified, then an exception will be raised.</para>
		/// <para>Discard is only applied to dynamic textures.  If the texture is not dynamic, then an exception will be raised.</para>
		/// <para>If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net), then the immediate context is used.  Use a deferred context to allow multiple threads to lock the 
		/// texture at the same time.</para>
		/// </remarks>
		/// <returns>The locked data stream and information about the lock.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the texture is not a dynamic or staging texture.
		/// <para>-or-</para>
		/// <para>Thrown when the texture is not a staging texture and the Read flag has been specified.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture is not a dynamic texture and the discard flag has been specified.</para>
		/// </exception>
		/// <typeparam name="T">The type of locking data.  This must be one of <see cref="GorgonLibrary.Graphics.GorgonTexture1DData">GorgonTexture1DData</see>, <see cref="GorgonLibrary.Graphics.GorgonTexture2DData">GorgonTexture2DData</see> or <see cref="GorgonLibrary.Graphics.GorgonTexture3DData">GorgonTexture3DData</see></typeparam>
		public unsafe GorgonImageBuffer Lock(BufferLockFlags lockFlags, int arrayIndex = 0, int mipLevel = 0, GorgonGraphics deferred = null)
		{
			DX.DataStream lockStream;

#if DEBUG
		    if ((arrayIndex < 0) || (arrayIndex >= Settings.ArrayCount))
		    {
		        throw new ArgumentOutOfRangeException("arrayIndex",
		            string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, arrayIndex, 0, Settings.ArrayCount));
		    }

		    if ((mipLevel < 0) || (mipLevel >= Settings.MipCount))
		    {
		        throw new ArgumentOutOfRangeException("mipLevel",
		            string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, mipLevel, 0, Settings.MipCount));
		    }

			if ((Settings.Usage != BufferUsage.Staging) && (Settings.Usage != BufferUsage.Dynamic))
			{
				throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_BUFFER_USAGE_CANT_LOCK, Settings.Usage));
			}

			if ((Settings.Usage != BufferUsage.Staging) && ((lockFlags & BufferLockFlags.Read) == BufferLockFlags.Read))
			{
				throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_LOCK_CANNOT_READ_NON_STAGING);
			}

			if ((lockFlags & BufferLockFlags.NoOverwrite) == BufferLockFlags.NoOverwrite)
			{
				throw new ArgumentException(Resources.GORGFX_BUFFER_NO_OVERWRITE_NOT_VALID, "lockFlags");
			}

			if (((lockFlags & BufferLockFlags.Discard) == BufferLockFlags.Discard)
				&& ((lockFlags & BufferLockFlags.Read) == BufferLockFlags.Read))
			{
				throw new ArgumentException(Resources.GORGFX_LOCK_CANNOT_USE_WITH_READ, "lockFlags");
			}

			if ((deferred != null)
				&& ((lockFlags & BufferLockFlags.Discard) != BufferLockFlags.Discard))
			{
				throw new ArgumentException(Resources.GORGFX_LOCK_NEED_DISCARD_NOOVERWRITE, "lockFlags");
			}
#endif
            if (deferred == null)
            {
                deferred = Graphics;
            }

		    int subResource = D3D.Resource.CalculateSubResourceIndex(mipLevel, arrayIndex, Settings.MipCount);
			DX.DataBox box = deferred.Context.MapSubresource(D3DResource, subResource, GetMapMode(lockFlags), D3D.MapFlags.None, out lockStream);
		    var pitch = new GorgonFormatPitch(box.RowPitch, box.SlicePitch);

		    int width = Settings.Width;
		    int height = Settings.Height;
		    int depth = Settings.Depth;

		    for (int mip = 0; mip < mipLevel; ++mip)
		    {
		        if (width > 1)
		        {
		            width >>= 1;
		        }
		        if (height > 1)
		        {
		            height >>= 1;
		        }
		        if (depth > 1)
		        {
		            depth >>= 1;
		        }
		    }

		    return new GorgonImageBuffer(box.DataPointer.ToPointer(),
		        pitch,
		        mipLevel,
		        arrayIndex,
		        0,
		        width,
		        height,
		        depth,
		        Settings.Format);
		}

		/// <summary>
		/// Function to unlock a locked texture sub resource.
		/// </summary>
		/// <param name="subResource">The index of the sub resource to unlock.</param>
		/// <param name="deferred">[Optional] The deferred graphics context to use when unlocking the texture.</param>
		/// <remarks>If the <paramref name="deferred"/> parameter is NULL (Nothing in VB.Net), then the immediate context is used.  Ensure that the 
		/// correct context is used when unlocking a resource or an exception will be thrown.</remarks>
		public void Unlock(int arrayIndex = 0, int mipLevel = 0, GorgonGraphics deferred = null)
		{
			Graphics.Context.UnmapSubresource(D3DResource, D3D.Resource.CalculateSubResourceIndex(mipLevel, arrayIndex, Settings.MipCount));
		}

		/// <summary>
		/// Function to return the default shader view for a texture.
		/// </summary>
		/// <param name="texture">Texture that holds the default shader view.</param>
		/// <returns>The default shader view for the texture.</returns>
		public static GorgonTextureShaderView ToShaderView(GorgonTexture texture)
		{
			return texture == null ? null : texture._defaultShaderView;
		}

		/// <summary>
		/// Implicit operator to retrieve the default shader resource view for a texture.
		/// </summary>
		/// <param name="texture">The texture to retrieve the default shader view from.</param>
		/// <returns>The default shader view for the texture.</returns>
		public static implicit operator GorgonTextureShaderView(GorgonTexture texture)
		{
			return texture == null ? null : texture._defaultShaderView;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns the texture.</param>
		/// <param name="name">The name of the texture.</param>
		/// <param name="settings">Settings for the texture.</param>
		protected GorgonTexture(GorgonGraphics graphics, string name, ITextureSettings settings)
			: base(graphics, name)
		{
			Settings = settings;
            _viewCache = new GorgonViewCache(this);
            FormatInformation = GorgonBufferFormatInfo.GetInfo(Settings.Format);
		}
		#endregion
    }
}
