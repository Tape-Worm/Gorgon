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
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.UI;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
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
	    private GorgonTextureLockCache _lockCache;              // Cache for locks.
		private GorgonTextureShaderView _defaultShaderView;		// The default shader view for the texture.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return information about the format for the texture.
		/// </summary>
		public GorgonBufferFormatInfo FormatInformation
		{
			get;
		}

		/// <summary>
		/// Property to return the size of the texture, in bytes.
		/// </summary>
		/// <remarks>This will take into account whether the texture is a packed format, or compressed.</remarks>
		public override int SizeInBytes => GorgonImageData.GetSizeInBytes(Settings);

		/// <summary>
		/// Property to set or return the settings for the texture.
		/// </summary>
		public ITextureSettings Settings
		{
			get;
		}
		#endregion

		#region Methods.
	    /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        /// <exception cref="System.NotSupportedException">Thrown when the texture is attached to another object type.</exception>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    GorgonApplication.Log.Print("Gorgon texture {0}: Unbound from shaders.", LoggingLevel.Verbose, Name);
                    Graphics.Shaders.UnbindResource(this);

                    if (_lockCache != null)
                    {
                        _lockCache.Dispose();
                        _lockCache = null;
                    }

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
		/// <exception cref="GorgonException">Thrown when the texture has a usage of staging.
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

			var formatInformation = new GorgonBufferFormatInfo(format);

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
	    /// <exception cref="GorgonException">Thrown when the texture has a usage of staging.
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

	        var formatInformation = new GorgonBufferFormatInfo(format);

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

            /*if ((Settings.IsTextureCube)
                && ((arrayCount % 6) != 0))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_CUBE_ARRAY_SIZE_INVALID);
            }*/

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
		/// <exception cref="GorgonException">Thrown when the render target view could not be created.</exception>
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

			var info = new GorgonBufferFormatInfo(format);

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
		/// <exception cref="GorgonException">Thrown when the usage for this texture is set to Staging.
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
			if (Graphics.VideoDevice.RequestedFeatureLevel < DeviceFeatureLevel.Sm5)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.Sm5));
			}

            if (!Settings.AllowUnorderedAccessViews)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_VIEW_NO_SUPPORT, "GorgonUnorderedAccessView"));
            }

			if ((Graphics.VideoDevice.GetBufferFormatSupport(format) & BufferFormatSupport.UnorderedAccess) != BufferFormatSupport.UnorderedAccess)
			{
				throw new GorgonException(GorgonResult.CannotCreate,
										  string.Format(Resources.GORGFX_VIEW_FORMAT_NOT_SUPPORTED, format));
			}
			
			// Ensure the size of the data type fits the requested format.
			var info = new GorgonBufferFormatInfo(format);

			if (((info.Group != BufferFormat.R32_Typeless) && (FormatInformation.Group != info.Group))
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
			GorgonApplication.Log.Print("Gorgon texture {0}: Destroying D3D 11 texture resource.", LoggingLevel.Verbose, Name);

		    if (D3DResource == null)
		    {
		        return;
		    }

			// Release any cached resources.  Otherwise we'll be hold on to invalid resources.
			// This should only happen when we update a swap chain.
			_viewCache?.Clear();

			GorgonRenderStatistics.TextureCount--;
		    GorgonRenderStatistics.TextureSize -= SizeInBytes;
		    D3DResource.Dispose();
		    D3DResource = null;

			Graphics.UnregisterResource(this);
		}

		/// <summary>
		/// Function to release any resource views after a resource has been altered.
		/// </summary>
		protected void ReleaseResourceViews()
		{
			_viewCache?.ReleaseResources();
		}

		/// <summary>
		/// Function to reinitialize any resource views after a resource has been altered.
		/// </summary>
		protected void InitializeResourceViews()
		{
			_viewCache?.InitializeResources();
		}

		/// <summary>
		/// Function to copy this texture into a new staging texture.
		/// </summary>
		/// <returns>The new staging texture.</returns>
		protected abstract GorgonTexture OnGetStagingTexture();

		/// <summary>
		/// Function to create an image with initial data.
		/// </summary>
		/// <param name="initialData">Data to use when creating the image.</param>
		/// <remarks>The <paramref name="initialData"/> can be NULL (<i>Nothing</i> in VB.Net) IF the texture is not created with an Immutable usage flag.
		/// <para>To initialize the texture, create a new <see cref="Gorgon.Graphics.GorgonImageData">GorgonImageData</see> object and fill it with image information.</para>
		/// </remarks>
		protected abstract void OnInitialize(GorgonImageData initialData);

        /// <summary>
        /// Function to copy a texture subresource from another texture.
        /// </summary>
        /// <param name="sourceTexture">Source texture to copy.</param>
        /// <param name="sourceBox">The dimensions of the source area to copy.</param>
        /// <param name="sourceArrayIndex">The array index of the sub resource to copy.</param>
        /// <param name="sourceMipLevel">The mip map level of the sub resource to copy.</param>
        /// <param name="destX">Horizontal offset into the destination texture to place the copied data.</param>
        /// <param name="destY">Vertical offset into the destination texture to place the copied data.</param>
        /// <param name="destZ">Depth offset into the destination texture to place the copied data.</param>
        /// <param name="destArrayIndex">The array index of the destination sub resource to copy into.</param>
        /// <param name="destMipLevel">The mip map level of the destination sub resource to copy into.</param>
        /// <param name="unsafeCopy"><b>true</b> to disable all range checking for coorindates, <b>false</b> to clip coorindates to safe ranges.</param>
        /// <param name="context">The deferred context to use when copying the sub resource.</param>
        /// <remarks>Use this method to copy a specific sub resource of a texture to another sub resource of another texture, or to a different sub resource of the same texture.  The <paramref name="sourceBox"/> 
        /// coordinates must be inside of the destination, if it is not, then the source data will be clipped against the destination region. No stretching or filtering is supported by this method.
        /// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UNorm, etc.. are part of the R8G8B8A8 group).  If the 
        /// video device is a SM_4 then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
        /// <para>When copying sub resources (e.g. mip-map levels), the mip levels and array indices must be different if copying to the same texture.  If they are not, an exception will be thrown.</para>
        /// <para>Pass NULL (<i>Nothing</i> in VB.Net) to the sourceRange parameter to copy the entire sub resource.</para>
        /// <para>Video devices that have a feature level of SM2_a_b cannot copy sub resource data in a 1D texture if the texture is not a staging texture.</para>
        /// <para>If the <paramref name="context"/> parameter is NULL (<i>Nothing</i> in VB.Net) then the immediate context will be used.  If this method is called from multiple threads, then a deferred context should be passed for each thread that is 
        /// accessing the sub resource.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_4 device.
        /// <para>-or-</para>
        /// <para>Thrown when the subResource and destSubResource are the same and the source texture is the same as this texture.</para>
        /// </exception>
        /// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
        /// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when the video device has a feature level of SM2_a_b and this texture or the source texture are not staging textures.</exception>
        protected void OnCopySubResource(GorgonTexture sourceTexture, GorgonBox sourceBox, int sourceArrayIndex, int sourceMipLevel, int destX, int destY, int destZ, int destArrayIndex, int destMipLevel, bool unsafeCopy, GorgonGraphics context)
        {
            sourceTexture.ValidateObject("texture");

            // If we're trying to place the image data outside of this texture, then leave.
            if ((destX >= Settings.Width)
                || (destY >= Settings.Height)
                || (destZ >= Settings.Depth))
            {
                return;
            }

            int sourceResource = D3D.Resource.CalculateSubResourceIndex(sourceMipLevel,
                sourceArrayIndex,
                Settings.MipCount);
            int destResource = D3D.Resource.CalculateSubResourceIndex(destMipLevel,
                destArrayIndex,
                Settings.MipCount);

#if DEBUG
            if (Settings.Usage == BufferUsage.Immutable)
            {
                throw new InvalidOperationException(Resources.GORGFX_TEXTURE_IMMUTABLE);
            }

            // If the format is different, then check to see if the format group is the same.
            if ((sourceTexture.Settings.Format != Settings.Format)
                && ((sourceTexture.FormatInformation.Group != FormatInformation.Group)
                    || (Graphics.VideoDevice.RequestedFeatureLevel == DeviceFeatureLevel.Sm4)))
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_TEXTURE_COPY_CANNOT_CONVERT,
                    sourceTexture.Settings.Format,
                    Settings.Format), nameof(sourceTexture));
            }

            if ((sourceArrayIndex < 0) || (sourceArrayIndex >= sourceTexture.Settings.ArrayCount))
            {
                throw new ArgumentOutOfRangeException(nameof(sourceArrayIndex),
                    string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, sourceArrayIndex, 0, sourceTexture.Settings.ArrayCount));
            }

            if ((sourceMipLevel < 0) || (sourceMipLevel >= sourceTexture.Settings.MipCount))
            {
                throw new ArgumentOutOfRangeException(nameof(sourceMipLevel),
                    string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, sourceMipLevel, 0, sourceTexture.Settings.MipCount));
            }

            if ((destArrayIndex < 0) || (destArrayIndex >= Settings.ArrayCount))
            {
                throw new ArgumentOutOfRangeException(nameof(destArrayIndex),
                    string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, destArrayIndex, 0, Settings.ArrayCount));
            }

            if ((destMipLevel < 0) || (destMipLevel >= Settings.MipCount))
            {
                throw new ArgumentOutOfRangeException(nameof(destMipLevel),
                    string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, destMipLevel, 0, Settings.MipCount));
            }

            if ((this == sourceTexture) && (sourceResource == destResource))
            {
                throw new ArgumentException(Resources.GORGFX_TEXTURE_CANNOT_COPY_SAME_SUBRESOURCE);
            }
#endif
            if (context == null)
            {
                context = Graphics;
            }           

            if (!unsafeCopy)
            {
                // Clip off any overlap if the destination is outside of the destination texture.
                if (destX < 0)
                {
                    sourceBox.X -= destX;
                    sourceBox.Width += destX;
                }

                if (destY < 0)
                {
                    sourceBox.Y -= destY;
                    sourceBox.Height += destY;
                }

                if (destZ < 0)
                {
                    sourceBox.Z -= destZ;
                    sourceBox.Depth += destZ;
                }

                // Clip source box.
                int left = sourceBox.Left.Min(sourceTexture.Settings.Width - 1).Max(0);
                int top = sourceBox.Top.Min(sourceTexture.Settings.Height - 1).Max(0);
                int front = sourceBox.Front.Min(sourceTexture.Settings.Depth - 1).Max(0);
                int right = sourceBox.Right.Min(sourceTexture.Settings.Width + left).Max(1);
                int bottom = sourceBox.Bottom.Min(sourceTexture.Settings.Height + top).Max(1);
                int back = sourceBox.Back.Min(sourceTexture.Settings.Depth + front).Max(1);

                sourceBox = GorgonBox.FromTLFRBB(left, top, front, right, bottom, back);

                // Adjust source box to fit within our destination.
                destX = destX.Min(Settings.Width - 1).Max(0);
                destY = destY.Min(Settings.Height - 1).Max(0);
                destZ = destZ.Min(Settings.Depth - 1).Max(0);

                sourceBox.Width = (destX + sourceBox.Width).Min(Settings.Width - destX).Max(1);
                sourceBox.Height = (destY + sourceBox.Height).Min(Settings.Height - destY).Max(1);
                sourceBox.Depth = (destZ + sourceBox.Depth).Min(Settings.Depth - destZ).Max(1);

                // Nothing to copy, so get out.
                if ((sourceBox.Width == 0)
                    || (sourceBox.Height == 0)
                    || (sourceBox.Depth == 0))
                {
                    return;
                }
            }

            context.Context.CopySubresourceRegion(sourceTexture.D3DResource,
                sourceResource,
                sourceBox.Convert,
                D3DResource,
                destResource,
                destX,
                destY,
                destZ);
        }

        /// <summary>
        /// Function to lock a CPU accessible texture sub resource for reading/writing.
        /// </summary>
        /// <param name="lockFlags">Flags used to lock.</param>
        /// <param name="arrayIndex">Array index of the sub resource to lock.</param>
        /// <param name="mipLevel">The mip-map level of the sub resource to lock.</param>
        /// <param name="deferred">The deferred graphics context used to lock the texture.</param>
        /// <returns>A stream used to write to the texture.</returns>
        /// <remarks>This method is used to lock down a sub resource in the texture for reading/writing. When locking a texture, the entire texture sub resource is locked and returned.  There is no setting to return a portion of the texture subresource.
        /// <para>This method is only available to textures created with a staging or dynamic usage setting.  Otherwise an exception will be raised.</para>
        /// <para>Only the Write, Discard (with the Write flag) and Read flags may be used in the <paramref name="lockFlags"/> parameter.  The Read flag can only be used with staging textures and is mutually exclusive.</para>
        /// <para>If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net), then the immediate context is used.  Use a deferred context to allow multiple threads to lock the 
        /// texture at the same time.</para>
        /// </remarks>
        /// <returns>This method will return a <see cref="Gorgon.Graphics.GorgonTextureLockData">GorgonTextureLockData</see> object containing information about the locked sub resource as well as 
        /// a <see cref="Gorgon.IO.GorgonDataStream">GorgonDataStream</see> that is used to access the locked sub resource data.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the texture is not a dynamic or staging texture.
        /// <para>-or-</para>
        /// <para>Thrown when the texture is not a staging texture and the Read flag has been specified.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the texture is not a dynamic texture and the discard flag has been specified.</para>
        /// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when this texture is a depth/stencil texture.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="arrayIndex"/> or the <paramref name="mipLevel"/> parameters are less than 0, or larger than their respective counts in the texture settings.</exception>
        protected GorgonTextureLockData OnLock(BufferLockFlags lockFlags, int arrayIndex, int mipLevel, GorgonGraphics deferred)
        {
#if DEBUG
            if ((this is GorgonDepthStencil1D)
                || (this is GorgonDepthStencil2D))
            {
                throw new NotSupportedException(string.Format(Resources.GORGFX_TYPE_CANNOT_BE_USED,
                    GetType().FullName));
            }

            if ((arrayIndex < 0) || (arrayIndex >= Settings.ArrayCount))
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex),
                    string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, arrayIndex, 0, Settings.ArrayCount));
            }

            if ((mipLevel < 0) || (mipLevel >= Settings.MipCount))
            {
                throw new ArgumentOutOfRangeException(nameof(mipLevel),
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
                throw new ArgumentException(Resources.GORGFX_BUFFER_NO_OVERWRITE_NOT_VALID, nameof(lockFlags));
            }

            if (((lockFlags & BufferLockFlags.Discard) == BufferLockFlags.Discard)
                && ((lockFlags & BufferLockFlags.Read) == BufferLockFlags.Read))
            {
                throw new ArgumentException(Resources.GORGFX_LOCK_CANNOT_USE_WITH_READ, nameof(lockFlags));
            }

	        if (((lockFlags & BufferLockFlags.Discard) == BufferLockFlags.Discard)
	            && (Settings.Usage == BufferUsage.Staging))
	        {
				throw new ArgumentException(Resources.GORGFX_TEXTURE_DISCARD_NEEDS_DYNAMIC);
	        }

	        if ((deferred != null) && (deferred.IsDeferred)
                && ((lockFlags & BufferLockFlags.Discard) != BufferLockFlags.Discard))
            {
                throw new ArgumentException(Resources.GORGFX_LOCK_NEED_DISCARD_NOOVERWRITE, nameof(lockFlags));
            }
#endif
            if (deferred == null)
            {
                deferred = Graphics;
            }

            return _lockCache.Lock(lockFlags, mipLevel, arrayIndex, deferred);
        }

        /// <summary>
        /// Funtion to retrieve the binding flags for a resource.
        /// </summary>
        /// <param name="isDepth"><b>true</b> if the texture is meant for depth/stencil.</param>
        /// <param name="isTarget"><b>true</b> if the texture is meant for use as a render target.</param>
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
		internal void Initialize(GorgonImageData initialData)
		{
			try
			{
				GorgonApplication.Log.Print("{0} {1}: Creating D3D11 texture resource...", LoggingLevel.Verbose, GetType().Name, Name);
                
				OnInitialize(initialData);
				CreateDefaultResourceView();

				GorgonRenderStatistics.TextureCount++;
				GorgonRenderStatistics.TextureSize += SizeInBytes;
			}
			catch
			{
				D3DResource?.Dispose();
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
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="codec"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>		
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
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> or the <paramref name="codec"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>		
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
		/// </remarks>
		public void Save(Stream stream, GorgonImageCodec codec)
		{
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
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="filePath"/> or the <paramref name="codec"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
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
        /// </remarks>
		public void Save(string filePath, GorgonImageCodec codec)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(filePath));
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
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UNorm, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// <para>SM2_a_b devices may copy 2D textures, but there are format restrictions (must be compatible with a render target format).  3D textures can only be copied to textures that are in GPU memory, if either texture is a staging texture, then an exception will be thrown.</para>
		/// <para>If <paramref name="deferred"/> is NULL (<i>Nothing</i> in VB.Net), then the immediate context is used to copy the texture.  Specify a deferred context when accessing the resource is being accessed by a separate thread.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_4 device.
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
			texture.ValidateObject("texture");

#if DEBUG            
			if (texture.ResourceType != ResourceType)
			{
				throw new ArgumentException(
					string.Format(Resources.GORGFX_TEXTURE_NOT_SAME_TYPE, texture.Name, texture.ResourceType, ResourceType), nameof(texture));
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
		            || (Graphics.VideoDevice.RequestedFeatureLevel == DeviceFeatureLevel.Sm4)))
		    {
		        throw new ArgumentException(
		            string.Format(Resources.GORGFX_TEXTURE_COPY_CANNOT_CONVERT, texture.Settings.Format, Settings.Format),
		            nameof(texture));
		    }

		    if ((texture.Settings.Width != Settings.Width)
		        || (texture.Settings.Height != Settings.Height))
		    {
		        throw new ArgumentException(Resources.GORGFX_TEXTURE_MUST_BE_SAME_SIZE, nameof(texture));
		    }
#endif
            if (deferred == null)
            {
                deferred = Graphics;
            }

			deferred.Context.CopyResource(texture.D3DResource, D3DResource);
		}

        /// <summary>
        /// Function to copy data from the CPU to the texture on the GPU.
        /// </summary>
        /// <param name="buffer">A buffer containing the image data to copy.</param>
        /// <param name="destBox">A 3D box that will specify the region that will receive the data.</param>
        /// <param name="destArrayIndex">The array index that will receive the data.</param>
        /// <param name="destMipLevel">The mip map level that will receive the data.</param>
        /// <param name="context">A deferred graphics context used to copy the data.</param>
        /// <exception cref="System.InvalidOperationException">Thrown when the texture is dynamic or immutable.
        /// <para>-or-</para>
        /// <para>Thrown when the texture is multisampled.</para>
        /// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when this texture is a depth/stencil texture.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="destArrayIndex"/> or the <paramref name="destMipLevel"/> is less than 0 or greater than/equal to the 
        /// number of array indices or mip levels in the texture.</exception>
        /// <remarks>
        /// Use this to copy data into a texture with a usage of staging or default.  If the <paramref name="destBox"/> values are larger than the dimensions of the texture, then the data will be clipped.
        /// <para>Passing NULL (<i>Nothing</i> in VB.Net) to the <paramref name="destArrayIndex"/> and/or the <paramref name="destMipLevel"/> parameters will use the first array index and/or mip map level.</para>
        /// <para>This method will not work with depth/stencil textures or with textures that have multisampling applied.</para>
        /// <para>If the <paramref name="context"/> parameter is NULL (<i>Nothing</i> in VB.Net) then the immediate context will be used.  If this method is called from multiple threads, then a deferred context should be passed for each thread that is 
        /// accessing the sub resource.</para>
        /// </remarks>
        protected void OnUpdateSubResource(GorgonImageBuffer buffer,
	        GorgonBox destBox,
	        int destArrayIndex,
	        int destMipLevel,
            GorgonGraphics context)
	    {
#if DEBUG
            if ((this is GorgonDepthStencil1D)
                || (this is GorgonDepthStencil2D))
            {
                throw new NotSupportedException(string.Format(Resources.GORGFX_TYPE_CANNOT_BE_USED,
                    GetType().FullName));
            }

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

            if ((destArrayIndex < 0) || (destArrayIndex >= Settings.ArrayCount))
            {
                throw new ArgumentOutOfRangeException(nameof(destArrayIndex),
                    string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, destArrayIndex, 0, Settings.ArrayCount));
            }

            if ((destMipLevel < 0) || (destMipLevel >= Settings.MipCount))
            {
                throw new ArgumentOutOfRangeException(nameof(destMipLevel),
                    string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, destMipLevel, 0, Settings.MipCount));
            }
#endif
            // Ensure the box fits the source.
            destBox = new GorgonBox
            {
                Front = destBox.Front.Max(0).Min(Settings.Depth - 1),
                Left = destBox.Left.Max(0).Min(Settings.Width - 1),
                Top = destBox.Top.Max(0).Min(Settings.Height - 1),
                Depth = destBox.Depth.Max(1).Min(Settings.Depth),
                Width = destBox.Width.Max(1).Min(Settings.Width),
                Height = destBox.Height.Max(1).Min(Settings.Height)
            };

            if (context == null)
            {
                context = Graphics;
            }

            var box = new DX.DataBox
            {
                DataPointer = new IntPtr(buffer.Data.Address),
                RowPitch = buffer.PitchInformation.RowPitch,
                SlicePitch = buffer.PitchInformation.SlicePitch
            };

            var region = destBox.Convert;

            if (!context.IsDeferred)
            {
                context.Context.UpdateSubresource(box,
                    D3DResource,
                    D3D.Resource.CalculateSubResourceIndex(destMipLevel, destArrayIndex, Settings.MipCount),
                    region);
            }
            else
            {
                context.Context.UpdateSubresourceSafe(box,
                    D3DResource,
                    FormatInformation.SizeInBytes,
                    D3D.Resource.CalculateSubResourceIndex(destMipLevel, destArrayIndex, Settings.MipCount),
                    region,
                    FormatInformation.IsCompressed);
            }
	    }

		/// <summary>
		/// Function to return the default shader view for a texture.
		/// </summary>
		/// <param name="texture">Texture that holds the default shader view.</param>
		/// <returns>The default shader view for the texture.</returns>
		public static GorgonTextureShaderView ToShaderView(GorgonTexture texture)
		{
			return texture?._defaultShaderView;
		}

		/// <summary>
		/// Implicit operator to retrieve the default shader resource view for a texture.
		/// </summary>
		/// <param name="texture">The texture to retrieve the default shader view from.</param>
		/// <returns>The default shader view for the texture.</returns>
		public static implicit operator GorgonTextureShaderView(GorgonTexture texture)
		{
			return texture?._defaultShaderView;
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
		    _lockCache = new GorgonTextureLockCache(this);
            FormatInformation = new GorgonBufferFormatInfo(settings.Format);
		}
		#endregion
    }
}
