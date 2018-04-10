#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: April 8, 2018 8:17:22 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;


namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A texture used to project an image onto a graphic primitive such as a triangle.
    /// </summary>
    public class GorgonTexture2D
        : GorgonGraphicsResource, IGorgonTexture2DInfo
    {
		#region Variables.
		// The ID number of the texture.
		private static int _textureID;
	    // The list of cached texture unordered access views.
	    //private readonly Dictionary<TextureViewKey, GorgonTextureUav> _cachedUavs = new Dictionary<TextureViewKey, GorgonTextureUav>();
        // The list of cached texture shader resource views.
        //private readonly Dictionary<TextureViewKey, GorgonTextureView> _cachedSrvs = new Dictionary<TextureViewKey, GorgonTextureView>();
        // The list of cached render target resource views.
        private Dictionary<TextureViewKey, WeakReference<GorgonRenderTarget2DView>> _cachedRtvs = new Dictionary<TextureViewKey, WeakReference<GorgonRenderTarget2DView>>();
	    // The list of cached depth/stencil resource views.
	    //private readonly Dictionary<TextureViewKey, GorgonDepthStencilView> _cachedDsvs = new Dictionary<TextureViewKey, GorgonDepthStencilView>();
		// The logging interface used for debug logging.
        private readonly IGorgonLog _log;
		// The information used to create the texture.
		private readonly GorgonTexture2DInfo _info;
		// The texture lock cache.
		private readonly TextureLockCache _lockCache;
		// List of typeless formats that are compatible with a depth view format.
		private static readonly HashSet<BufferFormat> _typelessDepthFormats = new HashSet<BufferFormat>
		                                                                     {
			                                                                     BufferFormat.R16_Typeless,
			                                                                     BufferFormat.R32_Typeless,
			                                                                     BufferFormat.R24G8_Typeless,
			                                                                     BufferFormat.R32G8X24_Typeless
		                                                                     };
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the ID for this texture.
		/// </summary>
		public int TextureID
		{
			get;
		}

		/// <summary>
		/// Property to return the information about the format of the texture.
		/// </summary>
		public GorgonFormatInfo FormatInformation
		{
			get;
			private set;
		}

        /// <summary>
        /// Property to return the type of data in the resource.
        /// </summary>
        public override GraphicsResourceType ResourceType => GraphicsResourceType.Texture2D;

		/*/// <summary>
		/// Property to return the default shader view for this texture.
		/// </summary>
		/// <remarks>
		/// If the <see cref="Binding"/> property does not have a flag of <see cref="TextureBinding.ShaderResource"/>, then this value will return <b>null</b>.
		/// </remarks>
		public GorgonTextureView DefaultShaderResourceView
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the default depth/stencil view for this texture.
		/// </summary>
		/// <remarks>
		/// If the <see cref="Binding"/> property does not have a flag of <see cref="TextureBinding.DepthStencil"/>, then this value will return <b>null</b>.
		/// </remarks>
		public GorgonDepthStencilView DefaultDepthStencilView
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the default render target view for this texture.
		/// </summary>
		/// <remarks>
		/// If the <see cref="Binding"/> property does not have a flag of <see cref="TextureBinding.RenderTarget"/>, then this value will return <b>null</b>.
		/// </remarks>
		public GorgonRenderTargetView DefaultRenderTargetView
		{
			get;
			private set;
		}*/

        /// <summary>
        /// Property to return the width of the texture, in pixels.
        /// </summary>
        public int Width => _info.Width;

        /// <summary>
        /// Property to return the height of the texture, in pixels.
        /// </summary>
        public int Height => _info.Height;

        /// <summary>
        /// Property to return the number of array levels for a 1D or 2D texture.
        /// </summary>
        /// <remarks>
        /// For textures with a <see cref="IsCubeMap"/> set to <b>true</b>, this this value will return a multiple of 6.
        /// </remarks>
        public int ArrayCount => _info.ArrayCount;

        /// <summary>
        /// Property to return whether this 2D texture is a cube map.
        /// </summary>
        /// <remarks>
        /// When this value returns <b>true</b>, then the texture is defined as a cube map using the <see cref="ArrayCount"/> as the number of faces. Because of this, the <see cref="ArrayCount"/> value 
        /// will be a multiple of 6. 
        /// </remarks>
        public bool IsCubeMap => _info.IsCubeMap;

        /// <summary>
        /// Property to return the format of the texture.
        /// </summary>
        public BufferFormat Format => _info.Format;

        /// <summary>
        /// Property to set or return the format for a depth buffer that will be associated with this render target texture.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the <see cref="Binding"/> is not set to <see cref="TextureBinding.RenderTarget"/> then this property will return <see cref="BufferFormat.Unknown"/>.
        /// </para>
        /// </remarks>
        BufferFormat IGorgonTexture2DInfo.DepthStencilFormat => _info.DepthStencilFormat;

        /// <summary>
        /// Property to return the number of mip-map levels for the texture.
        /// </summary>
        /// <remarks>
        /// If the texture is multisampled, this value will return 1.
        /// </remarks>
        public int MipLevels => _info.MipLevels;

        /// <summary>
        /// Property to return the multisample quality and count for this texture.
        /// </summary>
        public GorgonMultisampleInfo MultisampleInfo => _info.MultisampleInfo;

        /// <summary>
        /// Property to return the flags to determine how the texture will be bound with the pipeline when rendering.
        /// </summary>
        /// <remarks>
        /// If the <see cref="GorgonGraphicsResource.Usage"/> property is set to <see cref="ResourceUsage.Staging"/>, then the texture binding will a value of <see cref="TextureBinding.None"/> as 
        /// staging textures do not support bindings of any kind. 
        /// </remarks>
        public TextureBinding Binding => _info.Binding;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to transfer texture data into an image buffer.
        /// </summary>
        /// <param name="texture">The texture to copy from.</param>
        /// <param name="arrayIndex">The index of the array to copy from.</param>
        /// <param name="mipLevel">The mip level to copy from.</param>
        /// <param name="buffer">The buffer to copy into.</param>
        private static unsafe void GetTextureData(GorgonTexture2D texture, int arrayIndex, int mipLevel, IGorgonImageBuffer buffer)
		{
			int depthCount = 1.Max(buffer.Depth);
			int height = 1.Max(buffer.Height);
			int rowStride = buffer.PitchInformation.RowPitch;
			int sliceStride = buffer.PitchInformation.SlicePitch;
			D3D11.MapMode flags = D3D11.MapMode.ReadWrite;

			// If this image is compressed, then use the block height information.
			if (buffer.PitchInformation.VerticalBlockCount > 0)
			{
				height = buffer.PitchInformation.HorizontalBlockCount;
			}

			// Copy the texture data into the buffer.
			GorgonTextureLockData textureLock = texture.Lock(flags, mipLevel, arrayIndex);

			byte* bufferPtr = (byte*)buffer.Data;

			using (textureLock)
			{
				// If the strides don't match, then the texture is using padding, so copy one scanline at a time for each depth index.
				if ((textureLock.PitchInformation.RowPitch != rowStride)
					|| (textureLock.PitchInformation.SlicePitch != sliceStride))
				{
					byte* destData = bufferPtr;
					byte* sourceData = (byte*)textureLock.Data;

					for (int depth = 0; depth < depthCount; depth++)
					{
						// Restart at the padded slice size.
						byte* sourceStart = sourceData;

						for (int row = 0; row < height; row++)
						{
                            Unsafe.CopyBlock(destData, sourceStart, (uint)rowStride);
							sourceStart += textureLock.PitchInformation.RowPitch;
							destData += rowStride;
						}

						sourceData += textureLock.PitchInformation.SlicePitch;
					}
				}
				else
				{
					// Since we have the same row and slice stride, copy everything in one shot.
                    Unsafe.CopyBlock(bufferPtr, (byte *)textureLock.Data, (uint)sliceStride);
				}
			}
		}

		/// <summary>
		/// Function to validate an unordered access binding for a texture.
		/// </summary>
		/// <param name="support">Format support.</param>
		// ReSharper disable once UnusedParameter.Local
		private void ValidateUnorderedAccess(BufferFormatSupport support)
		{
			if ((Binding & TextureBinding.UnorderedAccess) != TextureBinding.UnorderedAccess)
			{
				return;
			}

			if ((!FormatInformation.IsTypeless) && (support & BufferFormatSupport.TypedUnorderedAccessView) != BufferFormatSupport.TypedUnorderedAccessView)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_FORMAT_INVALID, Format));
			}

			if ((Usage == ResourceUsage.Dynamic) || (Usage == ResourceUsage.Staging))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_UNORDERED_RES_NOT_DEFAULT);
			}
			
			if (!MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultiSampling))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_TEXTURE_UNORDERED_NO_MULTISAMPLE);
			}
		}

		/// <summary>
		/// Function to validate a depth/stencil binding for a texture.
		/// </summary>
		private void ValidateDepthStencil()
		{
			if ((Binding & TextureBinding.DepthStencil) != TextureBinding.DepthStencil)
			{
				return;
			}

			// We can only use this as a shader resource if we've specified one of the known typeless formats.
			if ((Binding & TextureBinding.ShaderResource) == TextureBinding.ShaderResource)
			{
				if (!_typelessDepthFormats.Contains(Format))
				{
					throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_DEPTHSTENCIL_TYPED_SHADER_RESOURCE);
				}
			}
			else 
			{
				// Otherwise, we'll validate the format.
				if ((Format == BufferFormat.Unknown) || (!Graphics.FormatSupport[Format].IsDepthBufferFormat))
				{
					throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_DEPTHSTENCIL_FORMAT_INVALID, Format));
				}
			}

			if (Usage != ResourceUsage.Default)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_DEPTHSTENCIL_NOT_DEFAULT);
			}
		}

		/// <summary>
		/// Function to validate a render target binding for a texture.
		/// </summary>
		private void ValidateRenderTarget()
		{
			if ((Binding & TextureBinding.RenderTarget) != TextureBinding.RenderTarget)
			{
				return;
			}

			// Otherwise, we'll validate the format.
			if ((Format == BufferFormat.Unknown) || (!Graphics.FormatSupport[Format].IsRenderTargetFormat))
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_RENDERTARGET_FORMAT_NOT_VALID, Format));
			}

			if (Usage != ResourceUsage.Default)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_RENDERTARGET_NOT_DEFAULT);
			}
		}

		/// <summary>
		/// Function to validate the settings for a texture.
		/// </summary>
		private void ValidateTextureSettings()
		{
		    GorgonFormatInfo formatInfo = new GorgonFormatInfo(Format);

			// For texture arrays, bump the value up to be a multiple of 6 if we want a cube map.
			if ((IsCubeMap) && ((ArrayCount % 6) != 0))
			{
				while ((ArrayCount % 6) != 0)
				{
					_info.ArrayCount++;
				}
			}

			// Ensure that we can actually use our requested format as a texture.
			if ((Format == BufferFormat.Unknown) || (!Graphics.FormatSupport[Format].IsTextureFormat(ImageType.Image2D)))
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_FORMAT_NOT_SUPPORTED, Format, @"2D"));
			}

			// Validate depth/stencil binding.
			ValidateDepthStencil();

			// Validate unordered access binding.
			ValidateUnorderedAccess(Graphics.FormatSupport[Format].FormatSupport);

			// Validate render target binding.
			ValidateRenderTarget();

			if (IsCubeMap)
			{
				if (!MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultiSampling))
				{
					throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_CANNOT_MULTISAMPLE_CUBE);
				}
			}

			if ((ArrayCount > Graphics.VideoAdapter.MaxTextureArrayCount) || (ArrayCount < 1))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
					                        string.Format(Resources.GORGFX_ERR_TEXTURE_ARRAYCOUNT_INVALID, Graphics.VideoAdapter.MaxTextureArrayCount));
			}

			if ((Width > Graphics.VideoAdapter.MaxTextureWidth) || (Width < 1))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
					                        string.Format(Resources.GORGFX_ERR_TEXTURE_WIDTH_INVALID, @"2D", Graphics.VideoAdapter.MaxTextureWidth));
			}

			if ((Height > Graphics.VideoAdapter.MaxTextureHeight) || (Height < 1))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
					                        string.Format(Resources.GORGFX_ERR_TEXTURE_HEIGHT_INVALID, @"2D", Graphics.VideoAdapter.MaxTextureWidth));
			}

			// Ensure the number of mip levels is not outside of the range for the width/height.
			_info.MipLevels = MipLevels.Min(GorgonImage.CalculateMaxMipCount(Width, Height, 1)).Max(1);
			
			if (MipLevels > 1)
			{
				if ((Graphics.FormatSupport[Format].FormatSupport & BufferFormatSupport.Mip) != BufferFormatSupport.Mip)
				{
					throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_NO_MIP_SUPPORT, Format));
				}

				if (!MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultiSampling))
				{
					throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_MULTISAMPLE_INVALID_MIP));
				}
			}

			if ((formatInfo.IsCompressed) && (((Width % 4) != 0)
			                                  || ((Height % 4) != 0)))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_TEXTURE_BC_SIZE_NOT_MOD_4);
			}

		    GorgonMultisampleInfo maxMultiSample = Graphics.FormatSupport[Format].MaxMultisampleCountQuality;

			if ((!MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultiSampling)) 
			    && (MultisampleInfo.Quality > maxMultiSample.Quality)
			    && (MultisampleInfo.Count > maxMultiSample.Count))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
				                          string.Format(Resources.GORGFX_ERR_MULTISAMPLE_INVALID,
				                                        Graphics.VideoAdapter.Name,
				                                        MultisampleInfo.Count,
				                                        MultisampleInfo.Quality,
				                                        Format));
			}
		}

		/// <summary>
		/// Function to initialize a 2D texture.
		/// </summary>
		/// <param name="image">The image data used to populate the texture.</param>
		private void InitializeD3DTexture(IGorgonImage image)
		{
			ValidateTextureSettings();

			D3D11.CpuAccessFlags cpuFlags = D3D11.CpuAccessFlags.None;

			switch (Usage)
			{
			    case ResourceUsage.Staging:
			        cpuFlags = D3D11.CpuAccessFlags.Read | D3D11.CpuAccessFlags.Write;
			        break;
			    case ResourceUsage.Dynamic:
			        cpuFlags = D3D11.CpuAccessFlags.Write;
			        break;
			}

			if (((Binding & TextureBinding.UnorderedAccess) == TextureBinding.UnorderedAccess)
                && (MultisampleInfo != GorgonMultisampleInfo.NoMultiSampling))
		    {
		        throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_TEXTURE_MULTISAMPLED);
		    }

		    D3D11.Texture2DDescription tex2DDesc = new D3D11.Texture2DDescription
		                                           {
		                                               Format = (DXGI.Format)Format,
		                                               Width = Width,
		                                               Height = Height,
		                                               ArraySize = ArrayCount,
		                                               Usage = (D3D11.ResourceUsage)Usage,
		                                               BindFlags = (D3D11.BindFlags)Binding,
		                                               CpuAccessFlags = cpuFlags,
		                                               OptionFlags = IsCubeMap ? D3D11.ResourceOptionFlags.TextureCube : D3D11.ResourceOptionFlags.None,
		                                               SampleDescription = MultisampleInfo.ToSampleDesc(),
		                                               MipLevels = MipLevels
		                                           };

			if (image == null)
			{
				D3DResource = new D3D11.Texture2D(Graphics.D3DDevice, tex2DDesc)
					            {
					                DebugName = $"{Name}: Direct 3D 11 2D texture"
					            };
				return;
			}

			// Upload the data to the texture.
			DX.DataBox[] dataBoxes = new DX.DataBox[GorgonImage.CalculateDepthSliceCount(1, MipLevels) * ArrayCount];

		    unsafe
		    {
		        for (int arrayIndex = 0; arrayIndex < ArrayCount; ++arrayIndex)
		        {
		            for (int mipIndex = 0; mipIndex < MipLevels; ++mipIndex)
		            {
		                int boxIndex = mipIndex + (arrayIndex * MipLevels);
		                IGorgonImageBuffer buffer = image.Buffers[mipIndex, arrayIndex];
		                dataBoxes[boxIndex] = new DX.DataBox(new IntPtr((void*)buffer.Data), buffer.PitchInformation.RowPitch, buffer.PitchInformation.RowPitch);
		            }
		        }
		    }

			D3DResource = new D3D11.Texture2D(Graphics.D3DDevice, tex2DDesc, dataBoxes)
				            {
				                DebugName = $"{Name}: Direct 3D 11 2D texture"
				            };
		}

		/// <summary>
		/// Function to create the default views for the texture.
		/// </summary>
		private void InitializeDefaultViews()
		{
            
			if (Usage == ResourceUsage.Staging)
			{
				return;
			}
            /*
			if ((Binding & TextureBinding.ShaderResource) == TextureBinding.ShaderResource)
			{
				// For depth/stencil bindings that have a shader resource binding, find the correct depth/stencil format for the view.
				if ((Binding & TextureBinding.DepthStencil) == TextureBinding.DepthStencil)
				{
					switch (Format)
					{
						case BufferFormat.R32G8X24_Typeless:
							// We'll only default to the depth portion of the view, we'd need to create a separate view to read the stencil component.
							DefaultShaderResourceView = GetShaderResourceView(BufferFormat.R32_Float_X8X24_Typeless);
							DefaultDepthStencilView = GetDepthStencilView(BufferFormat.D32_Float_S8X24_UInt, 0, 0, 0, D3D11.DepthStencilViewFlags.ReadOnlyDepth);
							break;
						case BufferFormat.R24G8_Typeless:
							// We'll only default to the depth portion of the view, we'd need to create a separate view to read the stencil component.
							DefaultShaderResourceView = GetShaderResourceView(BufferFormat.R24_UNorm_X8_Typeless);
							DefaultDepthStencilView = GetDepthStencilView(BufferFormat.D24_UNorm_S8_UInt, 0, 0, 0, D3D11.DepthStencilViewFlags.ReadOnlyDepth);
							break;
						case BufferFormat.R16_Typeless:
							DefaultShaderResourceView = GetShaderResourceView(BufferFormat.R16_Float);
							DefaultDepthStencilView = GetDepthStencilView(BufferFormat.D16_UNorm, 0, 0, 0, D3D11.DepthStencilViewFlags.ReadOnlyDepth);
							break;
						case BufferFormat.R32_Typeless:
							DefaultShaderResourceView = GetShaderResourceView(BufferFormat.R32_Float);
							DefaultDepthStencilView = GetDepthStencilView(BufferFormat.D32_Float, 0, 0, 0, D3D11.DepthStencilViewFlags.ReadOnlyDepth);
							break;
					}
					return;
				}

                // If we don't have any format type, then we cannot retrieve a default view.
			    if (!FormatInformation.IsTypeless)
			    {
			        DefaultShaderResourceView = GetShaderResourceView(Format);
			    }
			}

			// Create the default depth/stencil view.
			if ((Binding & TextureBinding.DepthStencil) == TextureBinding.DepthStencil)
			{
				DefaultDepthStencilView = GetDepthStencilView();
			    return;
			}

			if ((Binding & TextureBinding.RenderTarget) != TextureBinding.RenderTarget)
			{
                _info.DepthStencilFormat = BufferFormat.Unknown;
				return;
			}

		    if ((DepthStencilFormat != BufferFormat.Unknown) && (TextureType != TextureType.Texture3D))
		    {
		        _log.Print($"Creating associated [{DepthStencilFormat}] format depth buffer for the render target '{Name}'.", LoggingLevel.Verbose);

		        if ((DepthStencilFormat != BufferFormat.D16_UNorm)
		            && (DepthStencilFormat != BufferFormat.D24_UNorm_S8_UInt)
		            && (DepthStencilFormat != BufferFormat.D32_Float)
		            && (DepthStencilFormat != BufferFormat.D32_Float_S8X24_UInt))
		        {
		            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_DEPTHSTENCIL_FORMAT_INVALID, DepthStencilFormat));
		        }

		        _ownsDepthStencil = true;
		        AssociatedDepthStencil = new GorgonTexture($"{Name}_RenderTarget_DepthStencil_{Guid.NewGuid():N}",
		                                                   Graphics,
		                                                   new GorgonTextureInfo(Info)
		                                                   {
		                                                       Format = DepthStencilFormat,
		                                                       Binding = TextureBinding.DepthStencil,
		                                                       Usage = ResourceUsage.Default,
		                                                       DepthStencilFormat = BufferFormat.Unknown
		                                                   });
            }
            else if (TextureType == TextureType.Texture3D)
		    {
		        _log.Print($"An associated depth buffer could not be created for the render target '{Name}' because it is a 3D texture.", LoggingLevel.Verbose);
                _info.DepthStencilFormat = BufferFormat.Unknown;
            }

            // We cannot define a default render target view if we have no type for our format.
		    if (FormatInformation.IsTypeless)
		    {
		        return;
		    }

		    DefaultRenderTargetView = GetRenderTargetView();*/
		}

		/// <summary>
		/// Function to initialize the texture.
		/// </summary>
		/// <param name="image">The image used to initialize the texture.</param>
		private void Initialize(IGorgonImage image)
		{
			if ((Usage == ResourceUsage.Immutable) && (image == null))
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_IMMUTABLE_REQUIRES_DATA, Name));
			}

			FormatInformation = new GorgonFormatInfo(Format);

			InitializeD3DTexture(image);

			InitializeDefaultViews();
		}

	    /// <summary>
	    /// Function to calculate the size of a texture, in bytes with the given parameters.
	    /// </summary>
	    /// <param name="width">The width of the texture.</param>
	    /// <param name="height">The height of the texture.</param>
	    /// <param name="depthOrArrayCount">The number of depth slices if using a 3D texture, or the number of array indices.</param>
	    /// <param name="format">The format for the texture.</param>
	    /// <param name="mipCount">The number of mip map levels.</param>
	    /// <param name="isCubeMap"><b>true</b> if the texture is meant to be used as a cube map, or <b>false</b> if not.</param>
	    /// <returns>The number of bytes for the texture.</returns>
	    public static int CalculateSizeInBytes(int width, int height, int depthOrArrayCount, BufferFormat format, int mipCount, bool isCubeMap)
	    {
	        return GorgonImage.CalculateSizeInBytes(ImageType.Image2D,
	                                                width,
	                                                height,
	                                                depthOrArrayCount,
	                                                format,
	                                                mipCount);
	    }

	    /// <summary>
	    /// Function to calculate the size of a texture, in bytes with the given parameters.
	    /// </summary>
	    /// <param name="info">The <see cref="IGorgonTexture2DInfo"/> used to define a texture.</param>
	    /// <returns>The number of bytes for the texture.</returns>
	    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
	    public static int CalculateSizeInBytes(IGorgonTexture2DInfo info)
	    {
	        if (info == null)
	        {
	            throw new ArgumentNullException(nameof(info));
	        }

	        return CalculateSizeInBytes(info.Width,
	                                    info.Height,
	                                    info.ArrayCount,
	                                    info.Format,
	                                    info.MipLevels,
	                                    info.IsCubeMap);
	    }

        /// <summary>
        /// Function to lock a CPU accessible texture sub resource for reading/writing.
        /// </summary>
        /// <param name="lockFlags">Flags used to lock.</param>
        /// <param name="arrayIndex">Array index of the sub resource to lock.</param>
        /// <param name="mipLevel">The mip-map level of the sub resource to lock.</param>
        /// <returns>A <see cref="GorgonTextureLockData"/> object representing the lock on the texture.</returns>
        /// <exception cref="ArgumentException">Thrown when the texture is not a dynamic or staging texture.</exception>
        /// <exception cref="NotSupportedException">Thrown when this texture is a depth/stencil texture.
        /// <para>-or-</para>
        /// <para>Thrown when the texture is not a staging texture and the Read flag has been specified.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the texture is not a dynamic texture and the discard flag has been specified.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the texture has a <see cref="TextureBinding"/> of <see cref="TextureBinding.DepthStencil"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method is used to lock down a sub resource in the texture for reading/writing (depending on <see cref="GorgonGraphicsResource.Usage"/>). When locking a texture, the entire texture sub resource 
        /// is locked and returned.  There is no setting to return a portion of the texture subresource.
        /// </para>
        /// <para>
        /// This method is only works for textures with a <see cref="GorgonGraphicsResource.Usage"/> of <c>Dynamic</c> or <c>Staging</c>. If the usage is not either of those values, then an exception will be thrown. 
        /// If the texture has a <see cref="TextureBinding"/> of <see cref="TextureBinding.DepthStencil"/>, then this method will throw an exception.
        /// </para>
        /// <para>
        /// When the texture usage is set to <c>Dynamic</c>, the lock will be write-only, but when the usage is set to <c>Staging</c>, then the lock will allow reading and writing of the texture data. This is 
        /// specified by the <paramref name="lockFlags"/> parameter. If the <c>WriteNoOverwrite</c> flag is supplied, it will be ignored and treated as a <c>Write</c> flag.
        /// </para> 
        /// <para>
        /// <note type="warning">
        /// <para>
        /// All exceptions raised by this method will only be done so when Gorgon is compiled in DEBUG mode.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <returns>This method will return a <see cref="GorgonTextureLockData"/> containing information about the locked sub resource and a pointer to the texture data in memory.</returns>
        public GorgonTextureLockData Lock(D3D11.MapMode lockFlags, int mipLevel = 0, int arrayIndex = 0)
		{
#if DEBUG
			if ((Usage != ResourceUsage.Staging) && (Usage != ResourceUsage.Dynamic))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_USAGE_CANT_LOCK, Usage));
			}

			if ((Binding & TextureBinding.DepthStencil) == TextureBinding.DepthStencil)
			{
				throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_BINDING_TYPE_CANNOT_BE_USED, TextureBinding.DepthStencil));
			}

			if ((Usage == ResourceUsage.Dynamic) &&
			    ((lockFlags == D3D11.MapMode.Read)
			     || (lockFlags == D3D11.MapMode.ReadWrite)))
			{
				throw new NotSupportedException(Resources.GORGFX_ERR_LOCK_CANNOT_READ_NON_STAGING);
			}

			if (lockFlags == D3D11.MapMode.WriteNoOverwrite)
			{
				lockFlags = D3D11.MapMode.Write;
			}
#endif

			mipLevel = mipLevel.Min(MipLevels - 1).Max(0);
			arrayIndex = arrayIndex.Min(ArrayCount - 1).Max(0);

			return _lockCache.Lock(lockFlags, mipLevel, arrayIndex);
		}

		/// <summary>
		/// Function to copy this texture into another <see cref="GorgonTexture2D"/>.
		/// </summary>
		/// <param name="destTexture">The <see cref="GorgonTexture2D"/> that will receive a copy of this texture.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="destTexture"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">
		/// <para>Thrown when the <see cref="IGorgonTexture2DInfo.MultisampleInfo"/>.<see cref="GorgonMultisampleInfo.Count"/> is not the same for the source <paramref name="destTexture"/> and this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture sizes are not the same.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture types are not the same.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> setting of <see cref="ResourceUsage.Immutable"/>.
		/// <para>-or-</para>
		/// <para>This texture has a lock, or the <paramref name="destTexture"/> is locked.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// This method copies the contents of this texture into the texture specified by the <paramref name="destTexture"/> parameter. If a sub resource for the <paramref name="destTexture"/> must be 
		/// copied, use the <see cref="CopySubResource"/> method.
		/// </para>
		/// <para>
		/// This method does not perform stretching, filtering or clipping.
		/// </para>
		/// <para>
		/// The <paramref name="destTexture"/> dimensions must be have the same dimensions, and <see cref="IGorgonTexture2DInfo.MultisampleInfo"/> as this texture. As well, the destination texture must not 
		/// have a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Immutable"/>. If these contraints are violated, then an exception will be thrown.
		/// </para>
		/// <para>
		/// Limited format conversion will be performed if the two textures are within the same bit group (e.g. <see cref="BufferFormat.R8G8B8A8_SInt"/> is convertible to <see cref="BufferFormat.R8G8B8A8_UNorm"/> 
		/// and so on, since they are both <c>R8G8B8A8</c>). If the bit group does not match, then an exception will be thrown.
		/// </para>
		/// <para>
		/// This texture, and the <paramref name="destTexture"/> must not have any locks open prior to copying. If either resource has a lock, then an exception is thrown.
		/// </para>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		/// <seealso cref="CopySubResource"/>
		public void CopyTo(GorgonTexture2D destTexture)
		{
			destTexture.ValidateObject(nameof(destTexture));

#if DEBUG
		    if ((_lockCache?.HasLocks ?? false)
		        || (destTexture._lockCache?.HasLocks ?? false))
		    {
                throw new InvalidOperationException(Resources.GORGFX_ERR_CANNOT_COPY_LOCKED);
		    }

			if (destTexture.ResourceType != ResourceType)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_NOT_SAME_TYPE, destTexture.Name, destTexture.ResourceType, ResourceType), nameof(destTexture));
			}

			if (Usage == ResourceUsage.Immutable)
			{
				throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_IMMUTABLE);
			}

			if ((MultisampleInfo.Count != destTexture.MultisampleInfo.Count) || (MultisampleInfo.Quality != destTexture.MultisampleInfo.Quality))
			{
				throw new InvalidOperationException(Resources.GORGFX_ERR_TEXTURE_MULTISAMPLE_PARAMS_MISMATCH);
			}

			// If the format is different, then check to see if the format group is the same.
			if ((destTexture.Format != Format) && ((destTexture.FormatInformation.Group != FormatInformation.Group)))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_COPY_CANNOT_CONVERT, destTexture.Format, Format), nameof(destTexture));
			}

			if ((destTexture.Width != Width) || (destTexture.Height != Height))
			{
				throw new ArgumentException(Resources.GORGFX_ERR_TEXTURE_MUST_BE_SAME_SIZE, nameof(destTexture));
			}
#endif

			Graphics.D3DDeviceContext.CopyResource(D3DResource, destTexture.D3DResource);
		}

		/// <summary>
		/// Function to copy a texture subresource from another <see cref="GorgonTexture2D"/> into this texture.
		/// </summary>
		/// <param name="sourceTexture">The texture to copy.</param>
		/// <param name="sourceBox">[Optional] The dimensions of the source area to copy.</param>
		/// <param name="sourceArrayIndex">[Optional] The array index of the sub resource to copy (for 1D/2D textures only).</param>
		/// <param name="sourceMipLevel">[Optional] The mip map level of the sub resource to copy.</param>
		/// <param name="destX">[Optional] Horizontal offset into the destination texture to place the copied data.</param>
		/// <param name="destY">[Optional] Vertical offset into the destination texture to place the copied data (for 2D/3D textures only).</param>
		/// <param name="destArrayIndex">[Optional] The array index of the destination sub resource to copy into (for 1D/2D textures only).</param>
		/// <param name="destMipLevel">[Optional] The mip map level of the destination sub resource to copy into.</param>
		/// <param name="copyMode">[Optional] Defines how data should be copied into the texture.</param>
		/// <exception cref="ArgumentNullException">Thrown when the texture parameter is <b>null</b>.</exception>
		/// <exception cref="NotSupportedException">Thrown when the formats cannot be converted because they're not of the same group.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="sourceTexture"/> is the same as this texture, and the <paramref name="sourceArrayIndex"/>, <paramref name="destArrayIndex"/>, <paramref name="sourceMipLevel"/> and the <paramref name="destMipLevel"/> 
		/// specified are pointing to the same subresource.</para>
		/// <para>-or-</para>
		/// <para>Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Immutable"/>.</para>
		/// </exception>
		/// <exception cref="InvalidOperationException"></exception>
		/// <remarks>
		/// <para>
		/// Use this method to copy a specific sub resource of a <see cref="GorgonTexture2D"/> to another sub resource of this <see cref="GorgonTexture2D"/>, or to a different sub resource of the same texture.  
		/// The <paramref name="sourceBox"/> coordinates must be inside of the destination, if it is not, then the source data will be clipped against the destination region. No stretching or filtering is 
		/// supported by this method. If the entire texture needs to be copied, then use the <see cref="CopyTo"/> method.
		/// </para>
		/// <para>
		/// Limited format conversion will be performed if the two textures are within the same bit group (e.g. <see cref="BufferFormat.R8G8B8A8_SInt"/> is convertible to 
		/// <see cref="BufferFormat.R8G8B8A8_UNorm"/> and so on, since they are both <c>R8G8B8A8</c>). If the bit group does not match, then an exception will be thrown.
		/// </para>
		/// <para>
		/// When copying sub resources (e.g. mip levels, array indices, etc...), the mip levels and array indices must be different if copying to the same texture.  If they are not, an exception will be thrown.
		/// </para>
		/// <para>
		/// The destination texture must not have a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Immutable"/>.
		/// </para>
		/// <para>
		/// The <paramref name="copyMode"/> flag defines how data will be copied into this texture.  See the <see cref="CopyMode"/> enumeration for a description of the values.
		/// </para>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		/// <seealso cref="CopyTo"/>
		public void CopySubResource(GorgonTexture2D sourceTexture, GorgonBox? sourceBox = null, int sourceArrayIndex = 0, int sourceMipLevel = 0, int destX = 0, int destY = 0, int destArrayIndex = 0, int destMipLevel = 0, CopyMode copyMode = CopyMode.None)
		{
			sourceTexture.ValidateObject(nameof(sourceTexture));

			// If we're trying to place the image data outside of this texture, then leave.
			if ((destX >= Width)
				|| (destY >= Height))
			{
				return;
			}

			GorgonBox box;

			// If we didn't specify a box to copy from, then create one.
			if (sourceBox == null)
			{
				box = new GorgonBox
				            {
					            X = 0,
					            Y = 0,
					            Z = 0,
					            Width = (sourceTexture.Width.Max(1)).Min(sourceTexture.Width),
					            Height = (sourceTexture.Height.Max(1)).Min(sourceTexture.Height),
					            Depth = 1
				            };
			}
			else
			{
				box = new GorgonBox
				      {
					      Left = (sourceBox.Value.Left.Min(Height - 1).Max(0)).Min(sourceTexture.Width - 1),
					      Top = (sourceBox.Value.Top.Min(Height - 1).Max(0)).Min(sourceTexture.Height - 1),
					      Front = 0,
					      Width = (sourceBox.Value.Width.Min(Width).Max(1)).Min(sourceTexture.Width),
					      Height = (sourceBox.Value.Height.Min(Height).Max(1)).Min(sourceTexture.Height),
					      Depth = 1
				      };
			}

			// Ensure the indices are clipped to our settings.
			sourceArrayIndex = sourceArrayIndex.Min(sourceTexture.ArrayCount - 1).Max(0);
			sourceMipLevel = sourceMipLevel.Min(sourceTexture.MipLevels - 1).Max(0);
			destArrayIndex = destArrayIndex.Min(ArrayCount - 1).Max(0);
			destMipLevel = destMipLevel.Min(MipLevels - 1).Max(0);

			int sourceResource = D3D11.Resource.CalculateSubResourceIndex(sourceMipLevel, sourceArrayIndex, MipLevels);
			int destResource = D3D11.Resource.CalculateSubResourceIndex(destMipLevel, destArrayIndex, MipLevels);

#if DEBUG
			// If the format is different, then check to see if the format group is the same.
			if ((sourceTexture.Format != Format)
				&& ((sourceTexture.FormatInformation.Group != FormatInformation.Group)))
			{
				throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_TEXTURE_COPY_CANNOT_CONVERT, sourceTexture.Format, Format));
			}

			if (Usage == ResourceUsage.Immutable)
			{
				throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_IMMUTABLE);
			}


			if ((this == sourceTexture) && (sourceResource == destResource))
			{
				throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_CANNOT_COPY_SAME_SUBRESOURCE);
			}
#endif

			// Clip off any overlap if the destination is outside of the destination texture.
			if (destX < 0)
			{
				box.X -= destX;
				box.Width += destX;
			}

			if (destY < 0)
			{
				box.Y -= destY;
				box.Height += destY;
			}

			// Clip source box.
			int left = box.Left.Min(sourceTexture.Width - 1).Max(0);
			int top = box.Top.Min(sourceTexture.Height - 1).Max(0);
			int right = box.Right.Min(sourceTexture.Width + left).Max(1);
			int bottom = box.Bottom.Min(sourceTexture.Height + top).Max(1);

			box = GorgonBox.FromTLFRBB(left, top, 0, right, bottom, 1);

			// Adjust source box to fit within our destination.
			destX = destX.Min(Width - 1).Max(0);
			destY = destY.Min(Height - 1).Max(0);

			box.Width = (destX + box.Width).Min(Width - destX).Max(1);
			box.Height = (destY + box.Height).Min(Height - destY).Max(1);
			box.Depth = 1;

			// Nothing to copy, so get out.
			if ((box.Width == 0)
				|| (box.Height == 0)
				|| (box.Depth == 0))
			{
				return;
			}

		    Graphics.D3DDeviceContext.CopySubresourceRegion1(D3DResource,
		                                                     destResource,
		                                                     destX,
		                                                     destY,
		                                                     0,
		                                                     sourceTexture.D3DResource,
		                                                     sourceResource,
		                                                     box.ToResourceRegion(),
		                                                     (int)copyMode);
		}

		/// <summary>
		/// Function to resolve a multisampled 2D <see cref="GorgonTexture2D"/> into a non-multisampled <see cref="GorgonTexture2D"/>.
		/// </summary>
		/// <param name="destination">The <see cref="GorgonTexture2D"/> that will receive the resolved texture.</param>
		/// <param name="resolveFormat">[Optional] A format that will determine how to resolve the multisampled texture into a non-multisampled texture.</param>
		/// <param name="destArrayIndex">[Optional] Index in the array that will receive the resolved texture data (for 1D/2D textures only).</param>
		/// <param name="destMipLevel">[Optional] The mip map level that will receive the resolved texture data.</param>
		/// <param name="srcArrayIndex">[Optional] The array index in the source to resolve (for 1D/2D textures only).</param>
		/// <param name="srcMipLevel">[Optional] The source mip level to resolve.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="destination"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the format of this texture, and the format <paramref name="destination"/> texture are not typeless, and are not the same format.
		/// <para>-or-</para>
		/// <para>Thrown when the format of both this texture and the <paramref name="destination"/> texture are typeless, but the resolve format is not set to a bit group compatible format, or the textures do not have bit group compatible formats.</para>
		/// <para>-or-</para>
		/// <para>Thrown when either the format of this texture or the <paramref name="destination"/> texture is typless, and the other is not, and the resolve format is not set to a bit group compatible format, or the textures do not have bit group compatible formats.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">Thrown when the source texture is not multisampled or the destination texture is multisampled or has a non default usage.</exception>
		/// <remarks>Use this method to resolve a multisampled texture into a non multisampled texture.  This is most useful when transferring a multisampled render target pass as an input to 
		/// a secondary pass.
		/// <para>The <paramref name="resolveFormat"/> parameter is used to determine how to interpret the data in the texture.  There are 3 ways this data may be interpreted:  
		/// <list type="number">
		/// <item><description>If both textures have a typed format, then the resolve format must be the same as the format of the textures.  Both textures must have the same format.</description></item>
		/// <item><description>If one of the textures have a typeless format and one has a typed format, then the resolve format must be in the same group as the typed format.</description></item>
		/// <item><description>If the textures both have a typeless format, then the resolve format must be in the same group as the typeless format.</description></item>
		/// </list>
		/// Leaving the resolve format as Unknown will automatically use the format of the source texture.
		/// </para>
		/// </remarks>
		public void ResolveTo(GorgonTexture2D destination, BufferFormat resolveFormat = BufferFormat.Unknown, int destArrayIndex = 0, int destMipLevel = 0, int srcArrayIndex = 0, int srcMipLevel = 0)
		{
			destination.ValidateObject(nameof(destination));

			destArrayIndex = destArrayIndex.Min(destination.ArrayCount - 1).Max(0);
			destMipLevel = destMipLevel.Min(destination.MipLevels - 1).Max(0);
			srcArrayIndex = srcArrayIndex.Min(ArrayCount - 1).Max(0);
			srcMipLevel = srcMipLevel.Min(MipLevels - 1).Max(0);

			// If the formats for the textures are identical, and we've not specified a format, then we need to 
			// tell the resolve function that we have to use the format of the textures.
			if ((resolveFormat == BufferFormat.Unknown) && (destination.Format == Format))
			{
				resolveFormat = Format;
			}

#if DEBUG
			if (MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultiSampling))
			{
				throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_TEXTURE_NOT_MULTISAMPLED, Name));
			}

			if (destination.Usage != ResourceUsage.Default)
			{
				throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_TEXTURE_RESOLVE_DEST_NOT_DEFAULT, destination.Name));
			}

			GorgonFormatInfo resolveFormatInfo = new GorgonFormatInfo(resolveFormat);

			// If we have typed formats, and they're not the same, then that's an error according to the D3D docs.
			if ((!FormatInformation.IsTypeless) && (!destination.FormatInformation.IsTypeless))
			{
				if (Format != destination.Format)
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_RESOLVE_FORMATS_NOT_SAME, Format), nameof(destination));
				}
			}

			// If both formats are typeless, then both formats must be the same and the resolve format must be set to a compatible format.
			if ((FormatInformation.IsTypeless) && (destination.FormatInformation.IsTypeless))
			{
				if (Format != destination.Format)
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_RESOLVE_FORMATS_NOT_SAME, Format), nameof(destination));
				}

				if (resolveFormatInfo.Group != FormatInformation.Group)
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_RESOLVE_FORMAT_NOT_SAME_GROUP, Format), nameof(resolveFormat));
				}
			}

			// If one format is typeless, and the other is not, then the formats must be compatible and the resolve format must be specified.
			if ((FormatInformation.IsTypeless) || (destination.FormatInformation.IsTypeless))
			{
				if (resolveFormatInfo.IsTypeless)
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_RESOLVE_FORMAT_CANNOT_BE_TYPELESS), nameof(resolveFormat));
				}

				if ((FormatInformation.Group != destination.FormatInformation.Group) 
					|| ((resolveFormatInfo.Group != FormatInformation.Group) && (resolveFormatInfo.Group != destination.FormatInformation.Group)))
				{
					throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_RESOLVE_SRC_DEST_NOT_SAME_GROUP, Format, destination.Format),
					                            nameof(destination));
				}
			}
#endif

			int sourceIndex = D3D11.Resource.CalculateSubResourceIndex(srcMipLevel, srcArrayIndex, MipLevels);
			int destIndex = D3D11.Resource.CalculateSubResourceIndex(destMipLevel, destArrayIndex, destination.MipLevels);

			Graphics.D3DDeviceContext.ResolveSubresource(D3DResource, sourceIndex, destination.D3DResource, destIndex, (DXGI.Format)resolveFormat);
		}

		/// <summary>
		/// Function to get a staging texture from this texture.
		/// </summary>
		/// <returns>A new <see cref="GorgonTexture2D"/> containing a copy of the data in this texture, with a usage of <c>Staging</c>.</returns>
		/// <exception cref="GorgonException">Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> of <c>Immutable</c>.</exception>
		/// <remarks>
		/// <para>
		/// This allows an application to make a copy of the texture for editing on the CPU. The resulting staging texture, once edited, can then be reuploaded to the same texture, or another texture.
		/// </para>
		/// <para>
		/// The staging texture returned from this texture is tracked by this texture, and will be destroyed when this texture is destroyed. This also means that the instance returned will be the same instance 
		/// as the original staging teunless the result staging texture is disposed.
		/// </para>
		/// </remarks>
		public GorgonTexture2D GetStagingTexture()
		{
			if (Usage == ResourceUsage.Immutable)
			{
				throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_ERR_TEXTURE_IMMUTABLE));
			}

			IGorgonTexture2DInfo info = new GorgonTexture2DInfo(_info, $"{Name}_[Staging]")
			                          {
				                          Usage = ResourceUsage.Staging,
				                          Binding = TextureBinding.None
			                          };
			GorgonTexture2D staging = new GorgonTexture2D(Graphics, info);

			// Copy the data from this texture into the new staging texture.
			staging.CopyTo(this);

			return staging;
		}

		/// <summary>
		/// Function to copy data from the CPU using a <see cref="IGorgonImageBuffer"/> to this texture on the GPU.
		/// </summary>
		/// <param name="buffer">A <see cref="IGorgonImageBuffer"/> containing the image data to copy.</param>
		/// <param name="destBox">[Optional] A <see cref="GorgonBox"/> that will specify the region that will receive the data.</param>
		/// <param name="destArrayIndex">[Optional] The array index that will receive the data (1D/2D textures only).</param>
		/// <param name="destMipLevel">[Optional] The mip map level that will receive the data.</param>
		/// <exception cref="NotSupportedException">Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Dynamic"/> or <see cref="ResourceUsage.Immutable"/>.
		/// <para>-or-</para>
		/// <para>Thrown when this texture has <see cref="IGorgonTexture2DInfo.MultisampleInfo"/>.</para>
		/// <para>-or-</para>
		/// <para>Thrown when this texture has a <see cref="Binding"/> with the <see cref="TextureBinding.DepthStencil"/> flag set.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// Use this to copy data into a texture with a <see cref="GorgonGraphicsResource.Usage"/> of <c>Staging</c> or <c>Default</c>.  If the texture does not have a <c>Staging</c> or <c>Default</c> usage, then an 
		/// exception will be thrown.
		/// </para>
		/// <para>
		/// If the <paramref name="destBox"/> is specified, then a portion of the texture will be written into.  If the dimensions are larger than that of this texture, then the dimensions will be clipped to the 
		/// dimensions of the texture if they are larger. If this parameter is omitted, then the entire texture will be used (or up to the size of the <paramref name="buffer"/>).
		/// </para>
		/// <para>
		/// If the <paramref name="destArrayIndex"/>, or the <paramref name="destMipLevel"/> values are specified, then the data will be written into the array index if the texture is a 1D or 2D texture, otherwise 
		/// this value is ignored.
		/// </para>
		/// <para>
		/// This method will not work with textures that have a <see cref="Binding"/> that includes the <see cref="TextureBinding.DepthStencil"/> flag. If this texture has the 
		/// <see cref="TextureBinding.DepthStencil"/> flag, then an exception will be thrown.
		/// </para>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void UpdateSubResource(IGorgonImageBuffer buffer, GorgonBox? destBox = null, int destArrayIndex = 0, int destMipLevel = 0)
		{
			buffer.ValidateObject(nameof(buffer));

#if DEBUG
			if ((Binding & TextureBinding.DepthStencil) == TextureBinding.DepthStencil)
			{
				throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_BINDING_TYPE_CANNOT_BE_USED, TextureBinding.DepthStencil));
			}

			if ((Usage == ResourceUsage.Dynamic) || (Usage == ResourceUsage.Immutable))
			{
				throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_IS_DYNAMIC_OR_IMMUTABLE);
			}

			if ((MultisampleInfo.Count > 1) || (MultisampleInfo.Quality > 0))
			{
				throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_MULTISAMPLED);
			}
#endif

			destMipLevel = destMipLevel.Min(MipLevels - 1).Max(0);
			destArrayIndex = destArrayIndex.Min(ArrayCount - 1).Max(0);

			GorgonBox box;

			if (destBox == null)
			{
				box = new GorgonBox
				      {
					      Front = 0,
					      Top = 0,
					      Left = 0,
					      Width = (Width.Max(1)).Min(buffer.Width),
					      Height = (Height.Max(1)).Min(buffer.Height),
					      Depth = 1
				      };
			}
			else
			{
				// Ensure the box fits the source.
				box = new GorgonBox
				      {
					      Front = 0,
					      Left = (destBox.Value.Left.Min(Width - 1).Max(0)).Min(buffer.Width - 1),
					      Top = (destBox.Value.Top.Min(Height - 1).Max(0)).Min(buffer.Height - 1),
					      Depth = 1,
					      Width = (destBox.Value.Width.Min(Width).Max(1)).Min(buffer.Width),
					      Height = (destBox.Value.Height.Min(Height).Max(1)).Min(buffer.Height)
				      };
			}

		    unsafe
		    {
		        DX.DataBox boxPtr = new DX.DataBox
		                            {
		                                DataPointer = new IntPtr((void *)buffer.Data),
		                                RowPitch = buffer.PitchInformation.RowPitch,
		                                SlicePitch = buffer.PitchInformation.SlicePitch
		                            };

		        Graphics.D3DDeviceContext.UpdateSubresource(boxPtr,
		                                                    D3DResource,
		                                                    D3D11.Resource.CalculateSubResourceIndex(destMipLevel, destArrayIndex, MipLevels),
		                                                    box.ToResourceRegion());
		    }
		}

		/// <summary>
		/// Function to convert this texture to a <see cref="IGorgonImage"/>.
		/// </summary>
		/// <returns>A new <see cref="IGorgonImage"/> containing the texture data.</returns>
		/// <exception cref="GorgonException">Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> set to <see cref="ResourceUsage.Immutable"/>
		/// <para>-or-</para>
		/// <para>Thrown when the type of texture is not supported.</para>
		/// </exception>
		public IGorgonImage ToImage()
		{
			if (Usage == ResourceUsage.Immutable)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_IMMUTABLE));
			}

			GorgonTexture2D stagingTexture = this;
			GorgonImage image = null;

			try
			{
				if (Usage != ResourceUsage.Staging)
				{
					stagingTexture = GetStagingTexture();
				}

				ImageType imageType = stagingTexture.IsCubeMap ? ImageType.ImageCube : ImageType.Image2D;

				image = new GorgonImage(new GorgonImageInfo(imageType, stagingTexture.Format)
				{
					Width = Width,
					Height = Height,
					Depth = 1,
					ArrayCount = ArrayCount,
					MipCount = MipLevels
				});

				for (int array = 0; array < stagingTexture.ArrayCount; array++)
				{
					for (int mipLevel = 0; mipLevel < stagingTexture.MipLevels; mipLevel++)
					{
						// Get the buffer for the array and mip level.
						IGorgonImageBuffer buffer = image.Buffers[mipLevel, array];

						// Copy the data from the texture.
						GetTextureData(stagingTexture, array, mipLevel, buffer);
					}
				}

				return image;
			}
			catch
			{
				image?.Dispose();
				throw;
			}
			finally
			{
				if (stagingTexture != this)
				{
					stagingTexture?.Dispose();
				}
			}
		}

		/// <summary>
		/// Function to convert a texel coordinate into a pixel coordinate.
		/// </summary>
		/// <param name="texelCoordinates">The texel coordinates to convert.</param>
		/// <returns>The pixel coordinates.</returns>
		public DX.Point ToPixel(DX.Vector2 texelCoordinates)
		{
			return new DX.Point((int)(texelCoordinates.X * Width), (int)(texelCoordinates.Y * Height));
		}

		/// <summary>
		/// Function to convert a pixel coordinate into a texel coordinate.
		/// </summary>
		/// <param name="pixelCoordinates">The pixel coordinate to convert.</param>
		/// <returns>The texel coordinates.</returns>
		public DX.Vector2 ToTexel(DX.Point pixelCoordinates)
		{
			return new DX.Vector2(pixelCoordinates.X / (float)Width, pixelCoordinates.Y / (float)Height);
		}

		/// <summary>
		/// Function to convert a pixel coordinate into a texel coordinate.
		/// </summary>
		/// <param name="pixelCoordinates">The pixel coordinate to convert.</param>
		/// <returns>The texel coordinates.</returns>
		public DX.Vector2 ToTexel(DX.Vector2 pixelCoordinates)
		{
			return new DX.Vector2(pixelCoordinates.X / Width, pixelCoordinates.Y / Height);
		}

		/// <summary>
		/// Function to convert a texel size into a pixel size.
		/// </summary>
		/// <param name="texelCoordinates">The texel size to convert.</param>
		/// <returns>The pixel size.</returns>
		public DX.Size2 ToPixel(DX.Size2F texelCoordinates)
		{
			return new DX.Size2((int)(texelCoordinates.Width * Width), (int)(texelCoordinates.Height * Height));
		}

		/// <summary>
		/// Function to convert a pixel size into a texel size.
		/// </summary>
		/// <param name="pixelCoordinates">The pixel size to convert.</param>
		/// <returns>The texel size.</returns>
		public DX.Size2F ToTexel(DX.Size2 pixelCoordinates)
		{
			return new DX.Size2F(pixelCoordinates.Width / (float)Width, pixelCoordinates.Height / (float)Height);
		}

		/// <summary>
		/// Function to convert a texel rectangle into a pixel rectangle.
		/// </summary>
		/// <param name="texelCoordinates">The texel rectangle to convert.</param>
		/// <returns>The pixel rectangle.</returns>
		public DX.Rectangle ToPixel(DX.RectangleF texelCoordinates)
		{
			return new DX.Rectangle((int)(texelCoordinates.Left * Width),
			                        (int)(texelCoordinates.Top * Height),
			                        (int)(texelCoordinates.Width * Width),
			                        (int)(texelCoordinates.Height * Height));
		}

		/// <summary>
		/// Function to convert a pixel rectangle into a texel rectangle.
		/// </summary>
		/// <param name="pixelCoordinates">The pixel rectangle to convert.</param>
		/// <returns>The texel rectangle.</returns>
		public DX.RectangleF ToTexel(DX.Rectangle pixelCoordinates)
		{
			return new DX.RectangleF(pixelCoordinates.Left / (float)Width,
			                         pixelCoordinates.Top / (float)Height,
			                         pixelCoordinates.Width / (float)Width,
			                         pixelCoordinates.Height / (float)Height);
		}

        /*
        /// <summary>
        /// Function to create a new <see cref="GorgonTextureView"/> for this texture.
        /// </summary>
        /// <param name="format">[Optional] The format for the view.</param>
        /// <param name="firstMipLevel">[Optional] The first mip map level (slice) to start viewing from.</param>
        /// <param name="mipCount">[Optional] The number of mip map levels to view.</param>
        /// <param name="arrayIndex">[Optional] The array index to start viewing from.</param>
        /// <param name="arrayCount">[Optional] The number of array indices to view.</param>
        /// <returns>A <see cref="GorgonTextureView"/> used to bind the texture to a shader.</returns>
        /// <exception cref="ArgumentException">Thrown when this texture does not have a <see cref="TextureBinding"/> of <see cref="TextureBinding.ShaderResource"/>.
        /// <para>-or-</para>
        /// <para>Thrown when this texture has a usage of <c>Staging</c>.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="format"/> is typeless or cannot be determined from the this texture, or the <paramref name="format"/> is not in the same group as the texture format.</para>
        /// </exception>
        /// <exception cref="GorgonException">Thrown when this texture is a 2D cube texture, but is multiple sampled, or the <paramref name="arrayCount"/> is not a multiple of 6.</exception>
        /// <remarks>
        /// <para>
        /// This will create a view that makes a texture accessible to shaders. This allows viewing of the texture data in a different format, or even a subsection of the texture from within the shader.
        /// </para>
        /// <para>
        /// The <paramref name="format"/> parameter is used present the texture data as another format type to the shader. If this value is left at the default of <c>Unknown</c>, then the format from the 
        /// this texture is used. The <paramref name="format"/> must be castable to the format of this texture. If it is not, an exception will be thrown.
        /// </para>
        /// <para>
        /// The <paramref name="firstMipLevel"/> and <paramref name="mipCount"/> parameters define the starting mip level and the number of mip levels to allow access to within the shader. If these values fall 
        /// outside of the range of available mip levels, then they will be clipped to the upper and lower bounds of the mip chain. If these values are left at 0, then all mip levels will be accessible.
        /// </para>
        /// <para>
        /// The <paramref name="arrayIndex"/> and <paramref name="arrayCount"/> parameters define the starting array index and the number of array indices to allow access to within the shader. If these values 
        /// are left at 0, then all array indices will be accessible. If this texture is a <see cref="TextureType.Texture3D"/> type, then these parameters are ignored since 3D textures cannot have array indices.
        /// </para>
        /// </remarks>
	    public GorgonTextureView GetShaderResourceView(BufferFormat format = BufferFormat.Unknown, int firstMipLevel = 0, int mipCount = 0, int arrayIndex = 0, int arrayCount = 0)
	    {
	        if (format == BufferFormat.Unknown)
	        {
	            format = _info.Format;
	        }

	        firstMipLevel = firstMipLevel.Max(0).Min(MipLevels - 1);
	        arrayIndex = arrayIndex.Max(0).Min(ArrayCount - 1);

	        if (mipCount <= 0)
	        {
	            mipCount = _info.MipLevels - firstMipLevel;
	        }

	        mipCount = mipCount.Min(_info.MipLevels - firstMipLevel).Max(1);

	        if (arrayCount <= 0)
	        {
	            if (_info.TextureType == TextureType.Texture3D)
	            {
	                arrayIndex = 0;
	                arrayCount = 1;
	            }
	            else
	            {
	                arrayCount = _info.ArrayCount - arrayIndex;
	            }
	        }

	        arrayCount = (arrayCount.Min(ArrayCount - arrayIndex)).Max(1);

            TextureViewKey key = new TextureViewKey(format, firstMipLevel, mipCount, arrayIndex, arrayCount);

            if (_cachedSrvs.TryGetValue(key, out GorgonTextureView view))
            {
                return view;
            }

            view = new GorgonTextureView(this, format, firstMipLevel, mipCount, arrayIndex, arrayCount, _log);
	        view.CreateNativeView();
	        _cachedSrvs[key] = view;
            
	        return view;
	    }

        /// <summary>
        /// Function to create a new <see cref="GorgonTextureUav"/> for this texture.
        /// </summary>
        /// <param name="format">[Optional] The format for the view.</param>
        /// <param name="firstMipLevel">[Optional] The first mip map level (slice) to start viewing from.</param>
        /// <param name="arrayOrDepthIndex">[Optional] The array index or depth slice to start viewing from.</param>
        /// <param name="arrayOrDepthCount">[Optional] The number of array indices or depth slices to view.</param>
        /// <returns>A <see cref="GorgonTextureUav"/> used to bind the texture to a shader.</returns>
        /// <exception cref="GorgonException">Thrown if the video adapter does not support feature level 11 or better.
        /// <para>-or-</para>
        /// <para>Thrown when this texture does not have a <see cref="TextureBinding"/> of <see cref="TextureBinding.UnorderedAccess"/>.</para>
        /// <para>-or-</para>
        /// <para>Thrown when this texture has a usage of <c>Staging</c>.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="format"/> is typeless or is not a supported format for unordered access views.</para>
        /// <para>-or-</para>
        /// <para>Thrown if the this texture uses multisampling.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will create a unordered access view that makes a texture accessible to compute shaders (or pixel shaders) using unordered access to the data. This allows viewing of the texture data in a different 
        /// format, or even a subsection of the texture from within the shader.
        /// </para>
        /// <para>
        /// The <paramref name="format"/> parameter is used present the texture data as another format type to the shader. If this parameter is omitted, then the format of the texture will be used.
        /// </para>
        /// <para>
        /// The <paramref name="firstMipLevel"/> parameter defines the starting mip level to allow access to within the shader. If this value falls outside of the range of available mip levels, then it will be 
        /// clipped to the upper and lower bounds of the mip chain. If this value is left at 0, then only the first mip level is used.
        /// </para>
        /// <para>
        /// The <paramref name="arrayOrDepthIndex"/> and <paramref name="arrayOrDepthCount"/> parameters define the starting array index and the number of array indices to allow access to within the shader. If 
        /// these values are left at 0, then all array indices will be accessible. If this texture is a <see cref="TextureType.Texture3D"/> type, then these parameters represent the depth slices available 
        /// within the texture.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// This method requires a video adapter capable of supporting feature level 11 or better. If the current video adapter does not support feature level 11, an exception will be thrown.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public GorgonTextureUav GetUnorderedAccessView(BufferFormat format = BufferFormat.Unknown, int firstMipLevel = 0, int arrayOrDepthIndex = 0, int arrayOrDepthCount = 0)
	    {
	        if (Graphics.VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
	        {
	            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_UAV_REQUIRES_SM5);
	        }

	        if ((Usage == ResourceUsage.Staging)
                || ((Binding & TextureBinding.UnorderedAccess) != TextureBinding.UnorderedAccess))
	        {
	            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_RESOURCE_NOT_VALID, Name));
	        }

	        if (MultisampleInfo != GorgonMultisampleInfo.NoMultiSampling)
	        {
	            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_TEXTURE_MULTISAMPLED);
	        }

	        if (format == BufferFormat.Unknown)
	        {
	            format = Format;
	        }

	        if ((Graphics.VideoDevice.GetBufferFormatSupport(format) & BufferFormatSupport.TypedUnorderedAccessView) !=
	             BufferFormatSupport.TypedUnorderedAccessView)
	        {
	            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_FORMAT_INVALID, format));
	        }

	        // Ensure the size of the data type fits the requested format.
	        GorgonFormatInfo info = new GorgonFormatInfo(format);

	        if (((info.Group != BufferFormat.R32_Typeless) && (FormatInformation.Group != info.Group))
	            || (info.SizeInBytes != FormatInformation.SizeInBytes))
	        {
	            throw new GorgonException(GorgonResult.CannotCreate,
	                                      string.Format(Resources.GORGFX_ERR_VIEW_CANNOT_CAST_FORMAT,
	                                                    Format,
	                                                    format));
	        }

	        firstMipLevel = firstMipLevel.Max(0).Min(MipLevels - 1);
            arrayOrDepthIndex = arrayOrDepthIndex.Max(0).Min(TextureType == TextureType.Texture3D ? (Depth - 1) : (ArrayCount - 1));
	        
            if (arrayOrDepthCount <= 0)
            {
                arrayOrDepthCount = (_info.TextureType == TextureType.Texture3D ? _info.Depth : _info.ArrayCount) - arrayOrDepthIndex;
            }

	        arrayOrDepthCount = arrayOrDepthCount.Min((TextureType == TextureType.Texture3D ? (Depth) : (ArrayCount)) - arrayOrDepthIndex).Max(1);

	        TextureViewKey key = new TextureViewKey(format, firstMipLevel, _info.MipLevels, arrayOrDepthIndex, arrayOrDepthCount);

	        if (_cachedUavs.TryGetValue(key, out GorgonTextureUav view))
	        {
	            return view;
	        }

	        view = new GorgonTextureUav(this, format, info, firstMipLevel, arrayOrDepthIndex, arrayOrDepthCount, _log);
	        view.CreateNativeView();
	        _cachedUavs[key] = view;

	        return view;
        }

        /// <summary>
        /// Function to create a new <see cref="GorgonDepthStencilView"/> for this texture.
        /// </summary>
        /// <param name="format">[Optional] The format for the view.</param>
        /// <param name="firstMipLevel">[Optional] The first mip map level (slice) to start viewing from.</param>
        /// <param name="arrayIndex">[Optional] The array or depth index to start viewing from.</param>
        /// <param name="arrayCount">[Optional] The number of array indices or depth slices to view.</param>
        /// <param name="flags">[Optional] Flags to define how this view should be accessed by the shader.</param>
        /// <returns>A <see cref="GorgonTextureView"/> used to bind the texture to a shader.</returns>
        /// <exception cref="ArgumentException">Thrown when this texture does not have a <see cref="TextureBinding"/> of <see cref="TextureBinding.DepthStencil"/>.
        /// <para>-or-</para>
        /// <para>Thrown when this texture has a usage of <c>Staging</c>.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="format"/> is not supported as a depth/stencil format.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="arrayIndex"/> plus the <paramref name="arrayCount"/> is larger than the number of array indices/depth slices for this texture.</para>
        /// <para>-or-</para>
        /// <para>Thrown if the <paramref name="flags"/> parameter was set to value other than <c>None</c>, and the current video adapter does not support feature level 11 or better.</para>
        /// </exception>
        /// <exception cref="GorgonException">Thrown if this texture type is <see cref="TextureType.Texture3D"/>.</exception>
        public GorgonDepthStencilView GetDepthStencilView(BufferFormat format = BufferFormat.Unknown, int firstMipLevel = 0, int arrayIndex = 0, int arrayCount = 0, D3D11.DepthStencilViewFlags flags = D3D11.DepthStencilViewFlags.None)
	    {
	        if (_info.TextureType == TextureType.Texture3D)
	        {
	            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_VIEW_DEPTH_STENCIL_NO_3D);
	        }

	        if ((flags != D3D11.DepthStencilViewFlags.None) && (Graphics.VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0))
	        {
	            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0), nameof(flags));
	        }

	        if (format == BufferFormat.Unknown)
	        {
	            format = _info.Format;
	        }

            // Validate the format for the view.
            // If we have a typeless format for the texture, then it's likely we want to read it using a shader resource view.
	        switch (format)
	        {
	            case BufferFormat.R32G8X24_Typeless:
	            case BufferFormat.D32_Float_S8X24_UInt:
	                format = BufferFormat.D32_Float_S8X24_UInt;
	                break;
	            case BufferFormat.R24G8_Typeless:
	            case BufferFormat.D24_UNorm_S8_UInt:
	                format = BufferFormat.D24_UNorm_S8_UInt;
	                break;
	            case BufferFormat.R16_Typeless:
	            case BufferFormat.D16_UNorm:
	                format = BufferFormat.D16_UNorm;
	                break;
	            case BufferFormat.R32_Typeless:
	            case BufferFormat.D32_Float:
	                format = BufferFormat.D32_Float;
	                break;
	            default:
	                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FORMAT_NOT_SUPPORTED, format));
	        }

            firstMipLevel = firstMipLevel.Max(0).Min(MipLevels - 1);
	        arrayIndex = arrayIndex.Max(0).Min(ArrayCount - 1);

	        if (arrayCount <= 0)
	        {
	            arrayCount = _info.ArrayCount - arrayIndex;
	        }

	        arrayCount = arrayCount.Min(_info.ArrayCount - arrayIndex).Max(1);

            // Since we don't use the mip count, we can repurpose it to store the flag settings.
	        TextureViewKey key = new TextureViewKey(format, firstMipLevel, (int)flags, arrayIndex, arrayCount);

            if (_cachedDsvs.TryGetValue(key, out GorgonDepthStencilView view))
            {
                return view;
            }

            view = new GorgonDepthStencilView(this, format, firstMipLevel, arrayIndex, arrayCount, flags, _log);
	        view.CreateNativeView();
	        _cachedDsvs[key] = view;

	        return view;
	    }*/

        /// <summary>
        /// Function to create a new <see cref="GorgonRenderTarget2DView"/> for this texture.
        /// </summary>
        /// <param name="format">[Optional] The format for the view.</param>
        /// <param name="firstMipLevel">[Optional] The first mip map level (slice) to start viewing from.</param>
        /// <param name="arrayOrDepthIndex">[Optional] The array or depth index to start viewing from.</param>
        /// <param name="arrayOrDepthCount">[Optional] The number of array indices or depth slices to view.</param>
        /// <returns>A <see cref="GorgonTextureView"/> used to bind the texture to a shader.</returns>
        /// <exception cref="ArgumentException">Thrown when this texture does not have a <see cref="TextureBinding"/> of <see cref="TextureBinding.RenderTarget"/>.
        /// <para>-or-</para>
        /// <para>Thrown when this texture has a usage of <c>Staging</c>.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="format"/> is typeless or cannot be determined from the this texture, or the <paramref name="format"/> is not in the same group as the texture format.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="arrayOrDepthIndex"/> plus the <paramref name="arrayOrDepthCount"/> is larger than the number of array indices/depth slices for this texture.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will create a view that allows a texture to become a render target. This allows rendering into texture data in a different format, or even a subsection of the texture.
        /// </para>
        /// <para>
        /// The <paramref name="format"/> parameter is used present the texture data as another format type to the shader. If this value is left at the default of <c>Unknown</c>, then the format from the 
        /// this texture is used. The <paramref name="format"/> must be castable to the format of this texture. If it is not, an exception will be thrown.
        /// </para>
        /// <para>
        /// The <paramref name="firstMipLevel"/> parameter will be constrained to the number of mip levels for the texture should it be set to less than 0 or greater than the number of mip levels. If this 
        /// value is left at 0, then only the top mip level is used.
        /// </para>
        /// <para>
        /// The <paramref name="arrayOrDepthIndex"/> and <paramref name="arrayOrDepthCount"/> parameters will either indicate the array indices for a 1D/2D texture, or the depth slices for a 3D texture. These 
        /// parameters define the starting array index/depth slice and the number of array indices/depth slices to render into. If these values are left at 0, then the entire array/depth is used to receive 
        /// rendering data.
        /// </para>
        /// </remarks>
        public GorgonRenderTarget2DView GetRenderTargetView(BufferFormat format = BufferFormat.Unknown, int firstMipLevel = 0, int arrayOrDepthIndex = 0, int arrayOrDepthCount = 0)
	    {
	        if (format == BufferFormat.Unknown)
	        {
	            format = _info.Format;
	        }

	        firstMipLevel = firstMipLevel.Max(0).Min(MipLevels - 1);
	        arrayOrDepthIndex = arrayOrDepthIndex.Max(0);

	        if (arrayOrDepthCount <= 0)
	        {
	            arrayOrDepthCount = _info.ArrayCount - arrayOrDepthIndex;
	        }

	        arrayOrDepthCount = arrayOrDepthCount.Min(_info.ArrayCount - arrayOrDepthIndex).Max(1);

	        TextureViewKey key = new TextureViewKey(format, firstMipLevel, 1, arrayOrDepthIndex, arrayOrDepthCount);
	        GorgonRenderTarget2DView view;
            
            if (_cachedRtvs.TryGetValue(key, out WeakReference<GorgonRenderTarget2DView> viewRef))
            {
                if ((viewRef.TryGetTarget(out view))
                    && (!view.IsDisposed))
                {
                    return view;
                }

                // The view is dead, recreate it.
                _cachedRtvs.Remove(key);
            }

            view = new GorgonRenderTarget2DView(this, format, firstMipLevel, arrayOrDepthIndex, arrayOrDepthCount, _log);
	        view.CreateNativeView();
	        _cachedRtvs[key] = new WeakReference<GorgonRenderTarget2DView>(view);

	        return view;
	    }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
		{
            // Destroy all cached views.
            /*
		    foreach (KeyValuePair<TextureViewKey, GorgonTextureView> view in _cachedSrvs)
		    {
		        view.Value.Dispose();
		    }*/

		    Dictionary<TextureViewKey, WeakReference<GorgonRenderTarget2DView>> cachedRtvs = Interlocked.Exchange(ref _cachedRtvs, null);

		    if (cachedRtvs != null)
		    {
		        foreach (KeyValuePair<TextureViewKey, WeakReference<GorgonRenderTarget2DView>> viewRef in cachedRtvs)
		        {
		            if ((viewRef.Value.TryGetTarget(out GorgonRenderTarget2DView view))
		                && (!view.IsDisposed))
		            {
		                view.Dispose();
		            }
		        }
		    }

		    /*
            foreach (KeyValuePair<TextureViewKey, GorgonDepthStencilView> view in _cachedDsvs)
            {
                view.Value.Dispose();
            }

            foreach (KeyValuePair<TextureViewKey, GorgonTextureUav> view in _cachedUavs)
            {
                view.Value.Dispose();
            }*/

			_log.Print($"'{Name}': Destroying D3D11 Texture.", LoggingLevel.Simple);
            
			base.Dispose();
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2D"/> class.
		/// </summary>
		/// <param name="swapChain">The swap chain that holds the back buffers to retrieve.</param>
		/// <param name="index">The index of the back buffer to retrieve.</param>
		/// <param name="log">The log used for debug output.</param>
		/// <remarks>
		/// <para>
		/// This constructor is used internally to create a render target texture from a swap chain.
		/// </para>
		/// </remarks>
		internal GorgonTexture2D(GorgonSwapChain swapChain, int index, IGorgonLog log)
			: base(swapChain.Graphics, $"Swap Chain '{swapChain.Name}': Back buffer texture #{index}.")
		{
			_log = log;

			_log.Print($"Swap Chain '{swapChain.Name}': Creating texture from back buffer index {index}.", LoggingLevel.Simple);
			
			D3D11.Texture2D texture;
			
			// Get the resource from the swap chain.
			D3DResource = texture = D3D11.Resource.FromSwapChain<D3D11.Texture2D>(swapChain.DXGISwapChain, index);
			D3DResource.DebugName = $"Swap Chain '{swapChain.Name}': Back buffer texture #{index}.";

			// Get the info from the back buffer texture.
			_info = new GorgonTexture2DInfo
			        {
				        Format = (BufferFormat)texture.Description.Format,
				        Width = texture.Description.Width,
				        Height = texture.Description.Height,
				        Usage = (ResourceUsage)texture.Description.Usage,
				        ArrayCount = texture.Description.ArraySize,
				        MipLevels = texture.Description.MipLevels,
				        IsCubeMap = false,
				        MultisampleInfo = GorgonMultisampleInfo.NoMultiSampling,
				        Binding = (TextureBinding)texture.Description.BindFlags,
                        DepthStencilFormat = swapChain.DepthStencilFormat
			        };

			FormatInformation = new GorgonFormatInfo(Format);
			TextureID = Interlocked.Increment(ref _textureID);

            // Only create an RTV on the first buffer.
		    if (index == 0)
		    {
                // TODO: Fix
		        //DefaultRenderTargetView = GetRenderTargetView();
		    }

		    SizeInBytes = CalculateSizeInBytes(_info);
		    Usage = _info.Usage;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTexture2D"/> class.
        /// </summary>
        /// <param name="name">The name of the texture.</param>
        /// <param name="graphics">The graphics interface used to create this texture.</param>
        /// <param name="image">The image to copy into the texture.</param>
        /// <param name="usage">The defined usage for the texture.</param>
        /// <param name="binding">The allowed bindings for the texture.</param>
        /// <param name="multiSampleInfo">The multisample level to apply.</param>
        /// <param name="log">The log interface used for debugging.</param>
        /// <remarks>
        /// <para>
        /// This constructor is used when converting an image to a texture.
        /// </para>
        /// </remarks>
        internal GorgonTexture2D(string name, GorgonGraphics graphics, IGorgonImage image, ResourceUsage usage, TextureBinding binding, GorgonMultisampleInfo multiSampleInfo, IGorgonLog log)
			: base(graphics, name)
		{
			_log = log ?? GorgonLog.NullLog;

			_info = new GorgonTexture2DInfo
			        {
				        Format = image.Info.Format,
				        Width = image.Info.Width,
				        Height = image.Info.Height,
				        Usage = usage,
				        ArrayCount = image.Info.ArrayCount,
				        Binding = binding,
				        IsCubeMap = image.Info.ImageType == ImageType.ImageCube,
				        MipLevels = image.Info.MipCount,
				        MultisampleInfo = multiSampleInfo
			        };

			Initialize(image);
			TextureID = Interlocked.Increment(ref _textureID);
		    SizeInBytes = CalculateSizeInBytes(_info);
		    _lockCache = new TextureLockCache(this, _info);
		    Usage = _info.Usage;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2D"/> class.
		/// </summary>
		/// <param name="graphics">The <see cref="GorgonGraphics"/> interface that created this texture.</param>
		/// <param name="textureInfo">A <see cref="IGorgonTexture2DInfo"/> object describing the properties of this texture.</param>
		/// <param name="log">[Optional] The logging interface used for debugging.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="textureInfo"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <see cref="GorgonGraphicsResource.Usage"/> is set to <c>Immutable</c>.</exception>
		/// <exception cref="GorgonException">Thrown when the texture could not be created due to misconfiguration.</exception>
		/// <remarks>
		/// <para>
		/// This constructor creates an empty texture. Data may be uploaded to the texture at a later time if its <see cref="GorgonGraphicsResource.Usage"/> is not set to 
		/// <see cref="ResourceUsage.Immutable"/>. If the <see cref="GorgonGraphicsResource.Usage"/> is set to <see cref="ResourceUsage.Immutable"/> with this constructor, then an exception will be thrown. 
		/// To use an immutable texture, use the <see cref="O:Gorgon.Graphics.Core.GorgonImageTextureExtensions.ToTexture"/> extension method on the <see cref="IGorgonImage"/> type.
		/// </para>
		/// </remarks>
		public GorgonTexture2D(GorgonGraphics graphics, IGorgonTexture2DInfo textureInfo, IGorgonLog log = null)
			: base(graphics, textureInfo?.Name ?? throw new ArgumentNullException(nameof(textureInfo)))
		{
			_log = log ?? GorgonLog.NullLog;
			
			_info = new GorgonTexture2DInfo(textureInfo);

			Initialize(null);

			_lockCache = new TextureLockCache(this, _info);

			TextureID = Interlocked.Increment(ref _textureID);

		    SizeInBytes = CalculateSizeInBytes(_info);
		    Usage = _info.Usage;

            this.RegisterDisposable(graphics);
		}
        #endregion
    }
}
