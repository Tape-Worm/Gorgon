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

/* Unmerged change from project 'Gorgon.Graphics.Core (net5.0-windows)'
Before:
using System.Numerics;
using System.Collections.Generic;
After:
using System.Collections.Generic;
using System.ComponentModel;
*/
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A texture used to project an image onto a graphic primitive such as a triangle.
    /// </summary>
    public sealed class GorgonTexture2D
        : GorgonGraphicsResource, IGorgonTexture2DInfo, IGorgonTextureResource, IGorgonSharedResource
    {
        #region Constants.
        /// <summary>
        /// The prefix used for generated names.
        /// </summary>
        internal const string NamePrefix = nameof(GorgonTexture2D);
        #endregion

        #region Variables.
        // Default texture loading options.
        private static readonly GorgonTexture2DLoadOptions _defaultLoadOptions = new();
        // The ID number of the texture.
        private static int _textureID;
        // The list of cached texture unordered access views.
        private Dictionary<TextureViewKey, GorgonTexture2DReadWriteView> _cachedReadWriteViews = new();
        // The list of cached texture shader resource views.
        private Dictionary<TextureViewKey, GorgonTexture2DView> _cachedSrvs = new();
        // The list of cached render target resource views.
        private Dictionary<TextureViewKey, GorgonRenderTarget2DView> _cachedRtvs = new();
        // The list of cached depth/stencil resource views.
        private Dictionary<TextureViewKey, GorgonDepthStencil2DView> _cachedDsvs = new();
#if NET48_OR_GREATER
#pragma warning disable IDE0044 // Add readonly modifier
#endif
        // The information used to create the texture.
        private GorgonTexture2DInfo _info;
#if NET48_OR_GREATER
#pragma warning restore IDE0044 // Add readonly modifier
#endif
        // List of typeless formats that are compatible with a depth view format.
        private static readonly HashSet<BufferFormat> _typelessDepthFormats = new()
        {
            BufferFormat.R16_Typeless,
            BufferFormat.R32_Typeless,
            BufferFormat.R24G8_Typeless,
            BufferFormat.R32G8X24_Typeless
        };
        // The shared resource for this texture.
        private DXGI.Resource _sharedResource;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the bind flags used for the D3D 11 resource.
        /// </summary>
        internal override D3D11.BindFlags BindFlags => (D3D11.BindFlags)Binding;

        /// <summary>
        /// Property to return the type of image data.
        /// </summary>
        ImageType IGorgonImageInfo.ImageType => IsCubeMap ? ImageType.ImageCube : ImageType.Image2D;

        /// <summary>
        /// Property to return the depth of an image, in pixels.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This applies to 3D images only.  This parameter will be set to a value of 1 for a 1D or 2D image.
        /// </para>
        /// </remarks>
        int IGorgonImageInfo.Depth => 1;

        /// <summary>
        /// Property to return whether the image data is using premultiplied alpha.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value has no meaning for this type.
        /// </para>
        /// </remarks>
        bool IGorgonImageInfo.HasPreMultipliedAlpha => false;

        /// <summary>
        /// Property to return the number of mip map levels in the image.
        /// </summary>
        int IGorgonImageInfo.MipCount => MipLevels;

        /// <summary>
        /// Property to return whether the size of the texture is a power of 2 or not.
        /// </summary>
        bool IGorgonImageInfo.IsPowerOfTwo => ((Width == 0) || (Width & (Width - 1)) == 0)
                                              && ((Height == 0) || (Height & (Height - 1)) == 0);

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

        /// <summary>
        /// Property to return the width of the texture, in pixels.
        /// </summary>
        public int Width => _info.Width;

        /// <summary>
        /// Property to return the height of the texture, in pixels.
        /// </summary>
        public int Height => _info.Height;

        /// <summary>
        /// Property to return the number of array levels for a texture.
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

        /// <summary>
        /// Property to return the size, in bytes, of the resource.
        /// </summary>
        public override int SizeInBytes
        {
            get;
        }

        /// <summary>
        /// Property to return the usage for the resource.
        /// </summary>
        public override ResourceUsage Usage => _info.Usage;

        /// <summary>
        /// Property to return the name of this object.
        /// </summary>
        public override string Name => _info.Name;

        /// <summary>Property to return whether this texture can be shared with other graphics interfaces.</summary>
        /// <remarks>
        /// Settings this flag to <b>true</b> allows the texture to be used with external graphics interfaces such as a Direct3D device. This is useful for providing interoperation between systems.
        /// </remarks>
        public bool Shared =>_info.Shared;
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
            int height = 1.Max(buffer.Height);
            int rowStride = buffer.PitchInformation.RowPitch;
            int sliceStride = buffer.PitchInformation.SlicePitch;

            // If this image is compressed, then use the block height information.
            if (buffer.PitchInformation.VerticalBlockCount > 0)
            {
                height = buffer.PitchInformation.VerticalBlockCount;
            }

            // Copy the texture data into the buffer.
            int subResource = D3D11.Resource.CalculateSubResourceIndex(mipLevel, arrayIndex, texture.MipLevels);
            DX.DataBox lockBox = texture.Graphics.D3DDeviceContext.MapSubresource(texture.D3DResource,
                                                                                  subResource,
                                                                                  D3D11.MapMode.Read,
                                                                                  D3D11.MapFlags.None);
            try
            {
                byte* bufferPtr = (byte*)buffer.Data;

                if ((lockBox.RowPitch != rowStride)
                    || (lockBox.SlicePitch != sliceStride))
                {
                    byte* destData = bufferPtr;
                    byte* sourceData = (byte*)lockBox.DataPointer;

                    for (int row = 0; row < height; row++)
                    {
                        Unsafe.CopyBlock(destData, sourceData, (uint)rowStride);
                        sourceData += lockBox.RowPitch;
                        destData += rowStride;
                    }
                }
                else
                {
                    // Since we have the same row and slice stride, copy everything in one shot.
                    Unsafe.CopyBlock(bufferPtr, (byte*)lockBox.DataPointer, (uint)sliceStride);
                }
            }
            finally
            {
                texture.Graphics.D3DDeviceContext.UnmapSubresource(texture.D3DResource, subResource);
            }
        }

        /// <summary>
        /// Function to validate an unordered access binding for a texture.
        /// </summary>
        /// <param name="support">Format support.</param>
        // ReSharper disable once UnusedParameter.Local
        private void ValidateUnorderedAccess(BufferFormatSupport support)
        {
            if ((Binding & TextureBinding.ReadWriteView) != TextureBinding.ReadWriteView)
            {
                return;
            }

            if ((!FormatInformation.IsTypeless) && (support & BufferFormatSupport.TypedUnorderedAccessView) != BufferFormatSupport.TypedUnorderedAccessView)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_FORMAT_INVALID, Format));
            }

            if (Usage is ResourceUsage.Dynamic or ResourceUsage.Staging)
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
            var formatInfo = new GorgonFormatInfo(Format);

            if (Usage == ResourceUsage.Dynamic)
            {
                if (ArrayCount > 1)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_DYN_TEXTURE_MUST_HAVE_1_ARRAY, ArrayCount));
                }

                if (MipLevels > 1)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_DYN_TEXTURE_MUST_HAVE_1_MIP, MipLevels));
                }
            }

            if ((Usage == ResourceUsage.Staging) && (Binding != TextureBinding.None))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_STAGING_NO_BINDINGS, Binding));
            }

            // For texture arrays, bump the value up to be a multiple of 6 if we want a cube map.
            int arrayCount = ArrayCount;
            if ((IsCubeMap) && ((arrayCount % 6) != 0))
            {
                while ((arrayCount % 6) != 0)
                {
                    arrayCount++;
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
            int mipLevels = MipLevels.Min(GorgonImage.CalculateMaxMipCount(Width, Height, 1)).Max(1);

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

#if NET5_0_OR_GREATER
            if ((arrayCount == _info.ArrayCount) && (mipLevels == _info.MipLevels))
            {
                return;
            }

            _info = _info with
            {
                MipLevels = mipLevels,
                ArrayCount = arrayCount
            };
#endif
        }

        /// <summary>
        /// Function to initialize the texture.
        /// </summary>
        /// <param name="image">The image used to initialize the texture.</param>
        private void Initialize(IGorgonImage image)
        {
            if ((Usage == ResourceUsage.Immutable) && (image is null))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_IMMUTABLE_REQUIRES_DATA, Name));
            }

            FormatInformation = new GorgonFormatInfo(Format);

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

            if (((Binding & TextureBinding.ReadWriteView) == TextureBinding.ReadWriteView)
                && (MultisampleInfo != GorgonMultisampleInfo.NoMultiSampling))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_TEXTURE_MULTISAMPLED);
            }

            D3D11.ResourceOptionFlags options = D3D11.ResourceOptionFlags.None;

            if (IsCubeMap)
            {
                options |= D3D11.ResourceOptionFlags.TextureCube;
            }

            if (Shared)
            {
                options |= D3D11.ResourceOptionFlags.Shared;
            }
            
            var tex2DDesc = new D3D11.Texture2DDescription1
            {
                Format = (DXGI.Format)Format,
                Width = Width,
                Height = Height,
                ArraySize = ArrayCount,
                Usage = (D3D11.ResourceUsage)Usage,
                BindFlags = (D3D11.BindFlags)Binding,
                CpuAccessFlags = cpuFlags,
                OptionFlags = options,
                SampleDescription = MultisampleInfo.ToSampleDesc(),
                MipLevels = MipLevels
            };

            D3DResource = ResourceFactory.Create(Graphics.D3DDevice, Name, TextureID, in tex2DDesc, image);
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
        /// copied, use the <see cref="CopyTo(GorgonTexture2D, in DX.Rectangle?, int, int, int, int, int, int, CopyMode)"/> method.
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
        private void CopyResource(GorgonTexture2D destTexture)
        {
            if (destTexture == this)
            {
                return;
            }

            destTexture.ValidateObject(nameof(destTexture));

#if DEBUG
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
        /// Function to calculate the size of a texture, in bytes with the given parameters.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="arrayCount">The number of array indices.</param>
        /// <param name="format">The format for the texture.</param>
        /// <param name="mipCount">The number of mip map levels.</param>
        /// <param name="isCubeMap"><b>true</b> if the texture is meant to be used as a cube map, or <b>false</b> if not.</param>
        /// <returns>The number of bytes for the texture.</returns>
        public static int CalculateSizeInBytes(int width, int height, int arrayCount, BufferFormat format, int mipCount, bool isCubeMap) => GorgonImage.CalculateSizeInBytes(isCubeMap ? ImageType.ImageCube : ImageType.Image2D,
                                                    width,
                                                    height,
                                                    arrayCount,
                                                    format,
                                                    mipCount);

        /// <summary>
        /// Function to calculate the size of a texture, in bytes with the given parameters.
        /// </summary>
        /// <param name="info">The <see cref="IGorgonTexture2DInfo"/> used to define a texture.</param>
        /// <returns>The number of bytes for the texture.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
        public static int CalculateSizeInBytes(IGorgonTexture2DInfo info) => info is null
                ? throw new ArgumentNullException(nameof(info))
                : CalculateSizeInBytes(info.Width,
                                        info.Height,
                                        info.ArrayCount,
                                        info.Format,
                                        info.MipLevels,
                                        info.IsCubeMap);

        /// <summary>
        /// Function to retrieve the shared resource handle for this texture.
        /// </summary>
        /// <returns>A pointer representing a handle for sharing the texture data with other interfaces.</returns>
        /// <exception cref="GorgonException">Thrown if the shared texture could not be created.</exception>
        /// <remarks>
        /// <para>
        /// This is used to retrieve a handle to the shared resource that allows applications to share the texture with other APIs (e.g. Direct 2D). 
        /// </para>
        /// <para>
        /// To retrieve the shared resource handle, the texture must have set the <see cref="IGorgonTexture2DInfo.Shared"/> property on <see cref="GorgonTexture2DInfo"/> to <b>true</b>, otherwise the 
        /// method will throw an exception.
        /// </para>
        /// </remarks>        
        nint IGorgonSharedResource.GetSharedHandle()
        {
            if (!Shared)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_SHARED_RES, Name));
            }

            if (_sharedResource is not null)
            {
                return _sharedResource.SharedHandle;
            }
            
            DXGI.Resource resource = D3DResource.QueryInterface<DXGI.Resource>();

            if (resource is null)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_SHARED_RES_NOT_AVAILABLE, Name));
            }

            Interlocked.Exchange(ref _sharedResource, resource);
            return resource.SharedHandle;
        }

        /// <summary>
        /// Function to copy this texture into a <see cref="GorgonTexture1D"/>.
        /// </summary>
        /// <param name="destinationTexture">The texture to copy into.</param>
        /// <param name="sourceRange">[Optional] The dimensions of the source area to copy.</param>
        /// <param name="sourceY">[Optional] The vertical position in the texture to copy.</param>
        /// <param name="sourceArrayIndex">[Optional] The array index of the sub resource to copy.</param>
        /// <param name="sourceMipLevel">[Optional] The mip map level of the sub resource to copy.</param>
        /// <param name="destX">[Optional] Horizontal offset into the destination texture to place the copied data.</param>
        /// <param name="destArrayIndex">[Optional] The array index of the destination sub resource to copy into.</param>
        /// <param name="destMipLevel">[Optional] The mip map level of the destination sub resource to copy into.</param>
        /// <param name="copyMode">[Optional] Defines how data should be copied into the texture.</param>
        /// <exception cref="ArgumentNullException">Thrown when the texture parameter is <b>null</b>.</exception>
        /// <exception cref="NotSupportedException">Thrown when the formats cannot be converted because they're not of the same group.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="destinationTexture"/> is the same as this texture, and the <paramref name="sourceArrayIndex"/>, <paramref name="destArrayIndex"/>, <paramref name="sourceMipLevel"/> and the <paramref name="destMipLevel"/> 
        /// specified are pointing to the same subresource.</para>
        /// <para>-or-</para>
        /// <para>Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Immutable"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// Use this method to copy a specific sub resource of this <see cref="GorgonTexture2D"/> to another sub resource of a <see cref="GorgonTexture1D"/>. The <paramref name="sourceRange"/> coordinates
        /// must be inside of the destination, if it is not, then the source data will be clipped against the destination region. No stretching or filtering is supported by this method.
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
        public void CopyTo(GorgonTexture1D destinationTexture, GorgonRange? sourceRange = null, int sourceY = 0, int sourceArrayIndex = 0, int sourceMipLevel = 0, int destX = 0, int destArrayIndex = 0, int destMipLevel = 0, CopyMode copyMode = CopyMode.None)
        {
            destinationTexture.ValidateObject(nameof(destinationTexture));

            // If we're trying to place the image data outside of this texture, then leave.
            if ((destX >= destinationTexture.Width)
                || (sourceY < 0)
                || (sourceY >= Height))
            {
                return;
            }

            DX.Rectangle rect;

            // If we didn't specify a box to copy from, then create one.
            if (sourceRange is null)
            {
                rect = new DX.Rectangle(0, sourceY, Width.Min(destinationTexture.Width).Max(1), 1);
            }
            else
            {
                rect = new DX.Rectangle((sourceRange.Value.Minimum.Min(destinationTexture.Width - 1).Max(0)).Min(Width - 1), sourceY,
                                       (sourceRange.Value.Maximum.Min(destinationTexture.Width).Max(1)).Min(Width), 1);
            }

            // Ensure the indices are clipped to our settings.
            sourceArrayIndex = sourceArrayIndex.Min(ArrayCount - 1).Max(0);
            sourceMipLevel = sourceMipLevel.Min(MipLevels - 1).Max(0);
            destArrayIndex = destArrayIndex.Min(destinationTexture.ArrayCount - 1).Max(0);
            destMipLevel = destMipLevel.Min(destinationTexture.MipLevels - 1).Max(0);

            int sourceResource = D3D11.Resource.CalculateSubResourceIndex(sourceMipLevel, sourceArrayIndex, MipLevels);
            int destResource = D3D11.Resource.CalculateSubResourceIndex(destMipLevel, destArrayIndex, destinationTexture.MipLevels);

#if DEBUG
            // If the format is different, then check to see if the format group is the same.
            if ((destinationTexture.Format != Format)
                && ((destinationTexture.FormatInformation.Group != FormatInformation.Group)))
            {
                throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_TEXTURE_COPY_CANNOT_CONVERT, destinationTexture.Format, Format));
            }

            if (Usage == ResourceUsage.Immutable)
            {
                throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_IMMUTABLE);
            }

            if (MultisampleInfo != GorgonMultisampleInfo.NoMultiSampling)
            {
                throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_MULTISAMPLE_PARAMS_MISMATCH);
            }
#endif

            // Clip off any overlap if the destination is outside of the destination texture.
            if (destX < 0)
            {
                rect.X -= destX;
                rect.Width += destX;
            }

            // Clip source box.
            int left = rect.Left.Min(destinationTexture.Width - 1).Max(0);
            int right = rect.Right.Min(destinationTexture.Width + left).Max(1);

            rect = new DX.Rectangle
            {
                Left = left,
                Top = sourceY,
                Right = right,
                Bottom = sourceY + 1
            };

            // Adjust source box to fit within our destination.
            destX = destX.Min(destinationTexture.Width - 1).Max(0);

            rect.Width = (destX + rect.Width).Min(destinationTexture.Width - destX).Max(1);

            // Nothing to copy, so get out.
            if (rect.Width <= 0)
            {
                return;
            }

            Graphics.D3DDeviceContext.CopySubresourceRegion1(destinationTexture.D3DResource,
                                                             destResource,
                                                             destX,
                                                             0,
                                                             0,
                                                             D3DResource,
                                                             sourceResource,
                                                             new D3D11.ResourceRegion(rect.Left, rect.Top, 0, rect.Right, rect.Bottom, 1),
                                                             (int)copyMode);
        }

        /// <summary>
        /// Function to copy this texture into another <see cref="GorgonTexture2D"/>.
        /// </summary>
        /// <param name="destinationTexture">The texture to copy into.</param>
        /// <param name="sourceRectangle">[Optional] The dimensions of the source area to copy.</param>
        /// <param name="sourceArrayIndex">[Optional] The array index of the sub resource to copy.</param>
        /// <param name="sourceMipLevel">[Optional] The mip map level of the sub resource to copy.</param>
        /// <param name="destX">[Optional] Horizontal offset into the destination texture to place the copied data.</param>
        /// <param name="destY">[Optional] Vertical offset into the destination texture to place the copied data.</param>
        /// <param name="destArrayIndex">[Optional] The array index of the destination sub resource to copy into.</param>
        /// <param name="destMipLevel">[Optional] The mip map level of the destination sub resource to copy into.</param>
        /// <param name="copyMode">[Optional] Defines how data should be copied into the texture.</param>
        /// <exception cref="ArgumentNullException">Thrown when the texture parameter is <b>null</b>.</exception>
        /// <exception cref="NotSupportedException">Thrown when the formats cannot be converted because they're not of the same group.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="destinationTexture"/> is the same as this texture, and the <paramref name="sourceArrayIndex"/>, <paramref name="destArrayIndex"/>, <paramref name="sourceMipLevel"/> and the <paramref name="destMipLevel"/> 
        /// specified are pointing to the same subresource.</para>
        /// <para>-or-</para>
        /// <para>Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Immutable"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// Use this method to copy a specific sub resource of this <see cref="GorgonTexture2D"/> to another sub resource of a <see cref="GorgonTexture2D"/>, or to a different sub resource of the same texture.  
        /// The <paramref name="sourceRectangle"/> coordinates must be inside of the destination, if it is not, then the source data will be clipped against the destination region. No stretching or filtering is 
        /// supported by this method.
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
        public void CopyTo(GorgonTexture2D destinationTexture, in DX.Rectangle? sourceRectangle = null, int sourceArrayIndex = 0, int sourceMipLevel = 0, int destX = 0, int destY = 0, int destArrayIndex = 0, int destMipLevel = 0, CopyMode copyMode = CopyMode.None)
        {
            destinationTexture.ValidateObject(nameof(destinationTexture));

            // If we're trying to place the image data outside of the destination texture, then leave.
            if ((destX >= destinationTexture.Width)
                || (destY >= destinationTexture.Height))
            {
                return;
            }

            // If we ask to copy the entire thing, and our texture has identical sizes and format, then just use the copy method.
            if ((sourceRectangle is null) && (sourceArrayIndex == 0) && (sourceMipLevel == 0)
                && (destX == 0) && (destY == 0) && (destArrayIndex == 0) && (destMipLevel == 0)
                && (destinationTexture.Width == Width) && (destinationTexture.Height == Height) && (destinationTexture.MipLevels == MipLevels)
                && (destinationTexture.ArrayCount == ArrayCount) && (MultisampleInfo == destinationTexture.MultisampleInfo) &&
                ((Format == destinationTexture.Format) || (FormatInformation.Group == destinationTexture.FormatInformation.Group)))
            {
                CopyResource(destinationTexture);
                return;
            }

            DX.Rectangle rect;

            // If we didn't specify a box to copy from, then create one.
            if (sourceRectangle is null)
            {
                rect = new DX.Rectangle(0, 0, Width.Min(destinationTexture.Width).Max(1), Height.Min(destinationTexture.Height).Max(1));
            }
            else
            {
                rect = new DX.Rectangle((sourceRectangle.Value.Left.Min(destinationTexture.Width - 1).Max(0)).Min(Width - 1),
                                       (sourceRectangle.Value.Top.Min(destinationTexture.Height - 1).Max(0)).Min(Height - 1),
                                       (sourceRectangle.Value.Width.Min(destinationTexture.Width).Max(1)).Min(Width),
                                       (sourceRectangle.Value.Height.Min(destinationTexture.Height).Max(1)).Min(Height));
            }

            // Ensure the indices are clipped to our settings.
            sourceArrayIndex = sourceArrayIndex.Min(ArrayCount - 1).Max(0);
            sourceMipLevel = sourceMipLevel.Min(MipLevels - 1).Max(0);
            destArrayIndex = destArrayIndex.Min(destinationTexture.ArrayCount - 1).Max(0);
            destMipLevel = destMipLevel.Min(destinationTexture.MipLevels - 1).Max(0);

            int sourceResource = D3D11.Resource.CalculateSubResourceIndex(sourceMipLevel, sourceArrayIndex, MipLevels);
            int destResource = D3D11.Resource.CalculateSubResourceIndex(destMipLevel, destArrayIndex, destinationTexture.MipLevels);

#if DEBUG
            // If the format is different, then check to see if the format group is the same.
            if ((destinationTexture.Format != Format)
                && ((destinationTexture.FormatInformation.Group != FormatInformation.Group)))
            {
                throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_TEXTURE_COPY_CANNOT_CONVERT, destinationTexture.Format, Format));
            }

            if (Usage == ResourceUsage.Immutable)
            {
                throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_IMMUTABLE);
            }

            if ((this == destinationTexture) && (sourceResource == destResource))
            {
                throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_CANNOT_COPY_SAME_SUBRESOURCE);
            }

            if ((MultisampleInfo.Count != destinationTexture.MultisampleInfo.Count) || (MultisampleInfo.Quality != destinationTexture.MultisampleInfo.Quality))
            {
                throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_MULTISAMPLE_PARAMS_MISMATCH);
            }
#endif

            // Clip off any overlap if the destination is outside of the destination texture.
            if (destX < 0)
            {
                rect.X -= destX;
                rect.Width += destX;
            }

            if (destY < 0)
            {
                rect.Y -= destY;
                rect.Height += destY;
            }

            // Clip source box.
            int left = rect.Left.Min(Width - 1).Max(0);
            int top = rect.Top.Min(Height - 1).Max(0);
            int right = rect.Right.Min(Width + left).Max(1);
            int bottom = rect.Bottom.Min(Height + top).Max(1);

            rect = new DX.Rectangle
            {
                Left = left,
                Top = top,
                Right = right,
                Bottom = bottom
            };

            // Adjust source box to fit within our destination.
            destX = destX.Min(destinationTexture.Width - 1).Max(0);
            destY = destY.Min(destinationTexture.Height - 1).Max(0);

            rect.Width = (destX + rect.Width).Min(destinationTexture.Width - destX).Max(1);
            rect.Height = (destY + rect.Height).Min(destinationTexture.Height - destY).Max(1);

            // Nothing to copy, so get out.
            if ((rect.IsEmpty)
                || (rect.Width <= 0)
                || (rect.Height <= 0))
            {
                return;
            }

            Graphics.D3DDeviceContext.CopySubresourceRegion1(destinationTexture.D3DResource,
                                                             destResource,
                                                             destX,
                                                             destY,
                                                             0,
                                                             D3DResource,
                                                             sourceResource,
                                                             new D3D11.ResourceRegion(rect.Left, rect.Top, 0, rect.Right, rect.Bottom, 1),
                                                             (int)copyMode);
        }

        /// <summary>
        /// Function to copy this texture into a <see cref="GorgonTexture3D"/>.
        /// </summary>
        /// <param name="destinationTexture">The texture to copy into.</param>
        /// <param name="sourceRectangle">[Optional] The dimensions of the source area to copy.</param>
        /// <param name="sourceArrayIndex">[Optional] The array index of the sub resource to copy.</param>
        /// <param name="sourceMipLevel">[Optional] The mip map level of the sub resource to copy.</param>
        /// <param name="destX">[Optional] Horizontal offset into the destination texture to place the copied data.</param>
        /// <param name="destY">[Optional] Vertical offset into the destination texture to place the copied data.</param>
        /// <param name="destZ">[Optional] Depth offset into the destination texture to place the copied data.</param>
        /// <param name="destMipLevel">[Optional] The mip map level of the destination sub resource to copy into.</param>
        /// <param name="copyMode">[Optional] Defines how data should be copied into the texture.</param>
        /// <exception cref="ArgumentNullException">Thrown when the texture parameter is <b>null</b>.</exception>
        /// <exception cref="NotSupportedException">Thrown when the formats cannot be converted because they're not of the same group.
        /// <para>-or-</para>
        /// <para>Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Immutable"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// Use this method to copy a specific sub resource of this <see cref="GorgonTexture2D"/> to another sub resource of a <see cref="GorgonTexture3D"/>. The <paramref name="sourceRectangle"/>
        /// coordinates must be inside of the destination, if it is not, then the source data will be clipped against the destination region. No stretching or filtering is supported by this method.
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
        public void CopyTo(GorgonTexture3D destinationTexture, in DX.Rectangle? sourceRectangle = null, int sourceArrayIndex = 0, int sourceMipLevel = 0, int destX = 0, int destY = 0, int destZ = 0, int destMipLevel = 0, CopyMode copyMode = CopyMode.None)
        {
            destinationTexture.ValidateObject(nameof(destinationTexture));

            // If we're trying to place the image data outside of this texture, then leave.
            if ((destX >= destinationTexture.Width)
                || (destY >= destinationTexture.Height)
                || (destZ < 0)
                || (destZ >= destinationTexture.Depth))
            {
                return;
            }

            DX.Rectangle rect;

            // If we didn't specify a box to copy from, then create one.
            if (sourceRectangle is null)
            {
                rect = new DX.Rectangle(0, 0, Width.Min(destinationTexture.Width).Max(1), Height.Min(destinationTexture.Height).Max(1));
            }
            else
            {
                rect = new DX.Rectangle((sourceRectangle.Value.Left.Min(destinationTexture.Width - 1).Max(0)).Min(Width - 1),
                                       (sourceRectangle.Value.Top.Min(destinationTexture.Height - 1).Max(0)).Min(Height - 1),
                                       (sourceRectangle.Value.Width.Min(destinationTexture.Width).Max(1)).Min(Width),
                                       (sourceRectangle.Value.Height.Min(destinationTexture.Height).Max(1)).Min(Height));
            }

            // Ensure the indices are clipped to our settings.
            sourceMipLevel = sourceMipLevel.Min(MipLevels - 1).Max(0);
            sourceArrayIndex = sourceArrayIndex.Min(ArrayCount - 1).Max(0);
            destMipLevel = destMipLevel.Min(MipLevels - 1).Max(0);
            destZ = destZ.Min(destinationTexture.Depth - 1).Max(0);

            int sourceResource = D3D11.Resource.CalculateSubResourceIndex(sourceMipLevel, sourceArrayIndex, MipLevels);
            int destResource = D3D11.Resource.CalculateSubResourceIndex(destMipLevel, 0, MipLevels);

#if DEBUG
            // If the format is different, then check to see if the format group is the same.
            if ((destinationTexture.Format != Format)
                && ((destinationTexture.FormatInformation.Group != FormatInformation.Group)))
            {
                throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_TEXTURE_COPY_CANNOT_CONVERT, destinationTexture.Format, Format));
            }

            if (Usage == ResourceUsage.Immutable)
            {
                throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_IMMUTABLE);
            }

            if (MultisampleInfo != GorgonMultisampleInfo.NoMultiSampling)
            {
                throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_MULTISAMPLE_PARAMS_MISMATCH);
            }
#endif

            // Clip off any overlap if the destination is outside of the destination texture.
            if (destX < 0)
            {
                rect.X -= destX;
                rect.Width += destX;
            }

            if (destY < 0)
            {
                rect.Y -= destY;
                rect.Height += destY;
            }

            // Clip source box.
            int left = rect.Left.Min(Width - 1).Max(0);
            int top = rect.Top.Min(Height - 1).Max(0);
            int right = rect.Right.Min(Width + left).Max(1);
            int bottom = rect.Bottom.Min(Height + top).Max(1);

            rect = new DX.Rectangle
            {
                Left = left,
                Top = top,
                Right = right,
                Bottom = bottom
            };

            // Adjust source box to fit within our destination.
            destX = destX.Min(destinationTexture.Width - 1).Max(0);
            destY = destY.Min(destinationTexture.Height - 1).Max(0);

            rect.Width = (destX + rect.Width).Min(destinationTexture.Width - destX).Max(1);
            rect.Height = (destY + rect.Height).Min(destinationTexture.Height - destY).Max(1);

            // Nothing to copy, so get out.
            if ((rect.IsEmpty)
                || (rect.Width <= 0)
                || (rect.Height <= 0))
            {
                return;
            }

            Graphics.D3DDeviceContext.CopySubresourceRegion1(destinationTexture.D3DResource,
                                                             destResource,
                                                             destX,
                                                             destY,
                                                             destZ,
                                                             D3DResource,
                                                             sourceResource,
                                                             new D3D11.ResourceRegion(rect.Left, rect.Top, 0, rect.Right, rect.Bottom, 1),
                                                             (int)copyMode);
        }

        /// <summary>
        /// Function to resolve a multisampled 2D <see cref="GorgonTexture2D"/> into a non-multisampled <see cref="GorgonTexture2D"/>.
        /// </summary>
        /// <param name="destination">The <see cref="GorgonTexture2D"/> that will receive the resolved texture.</param>
        /// <param name="resolveFormat">[Optional] A format that will determine how to resolve the multisampled texture into a non-multisampled texture.</param>
        /// <param name="destArrayIndex">[Optional] Index in the array that will receive the resolved texture data.</param>
        /// <param name="destMipLevel">[Optional] The mip map level that will receive the resolved texture data.</param>
        /// <param name="srcArrayIndex">[Optional] The array index in the source to resolve.</param>
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

            var resolveFormatInfo = new GorgonFormatInfo(resolveFormat);

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
        /// </remarks>
        public GorgonTexture2D GetStagingTexture()
        {
            if (Usage == ResourceUsage.Immutable)
            {
                throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_ERR_TEXTURE_IMMUTABLE));
            }

            var info = new GorgonTexture2DInfo(_info)
            {
                Name = $"{Name}_[Staging]",
                Usage = ResourceUsage.Staging,
                Binding = TextureBinding.None
            };
            var staging = new GorgonTexture2D(Graphics, info);

            // Copy the data from this texture into the new staging texture.
            CopyTo(staging);

            return staging;
        }

        /// <summary>
        /// Function to update the texture, or a sub section of the texture with data from a <see cref="IGorgonImageBuffer"/> contained within a <see cref="IGorgonImage"/>.
        /// </summary>
        /// <param name="imageBuffer">The image buffer that contains the data to copy.</param>
        /// <param name="destRectangle">[Optional] The region on the texture to update.</param>
        /// <param name="destArrayIndex">[Optional] The array index to update.</param>
        /// <param name="destMipLevel">[Optional] The mip map level to update.</param>
        /// <param name="copyMode">[Optional] Flags to indicate how to copy the data.</param>
        /// <exception cref="NotSupportedException">Thrown if this image has a usage of <see cref="ResourceUsage.Immutable"/>, is multisampled, or has a binding of <see cref="TextureBinding.DepthStencil"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="imageBuffer"/> contains data in a <see cref="BufferFormat"/> that does not have the same size, in bytes, as the <see cref="Format"/> of this texture.
        /// <para>-or-</para>
        /// <para>Thrown if the <see cref="BufferFormat"/> of the <paramref name="imageBuffer"/> is compressed and the <see cref="Format"/> of this texture is not (or vice versa).</para>
        /// <para>-or-</para>
        /// <para>Thrown if the <see cref="BufferFormat"/> of the <paramref name="imageBuffer"/> is packed and the <see cref="Format"/> of this texture is not (or vice versa).</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will upload data from a <see cref="IGorgonImageBuffer"/> in a <see cref="IGorgonImage"/> to a sub section of the texture (e.g. a mip level, array index, etc...). The method will determine
        /// how to best upload the data depending on the <see cref="GorgonGraphicsResource.Usage"/> of the texture. For example, if the texture has a <see cref="GorgonGraphicsResource.Usage"/> of
        /// <see cref="ResourceUsage.Default"/>, then internally, this method will update to the GPU directly. Otherwise, if it is <see cref="ResourceUsage.Dynamic"/>, or <see cref="ResourceUsage.Staging"/>,
        /// it will use a locking pattern which uses the CPU to write data to the texture. The latter pattern is good if the texture has to change one or more times per frame, otherwise, the former is
        /// better where the texture is updated less than once per frame (i.e. Dynamic is good for multiple times per frame, Default is good for once per frame or less).
        /// </para>
        /// <para>
        /// Users who wish to capture a smaller portion of the source <paramref name="imageBuffer"/> can use the <see cref="IGorgonImageBuffer.GetRegion"/> method to extract a region from a buffer in a
        /// <see cref="IGorgonImage"/>.
        /// </para>
        /// <para>
        /// If the user supplies a <paramref name="destRectangle"/>, then the data will be copied to the region in the texture specified by the parameter, otherwise if the parameter is omitted, the full
        /// size of the texture (depending on mip level) is used. This value is clipped against the width/height of the mip level (e.g. A 256x256 image at mip level 2 would be 64x64).
        /// </para>
        /// <para>
        /// The <paramref name="destMipLevel"/>, and <paramref name="destArrayIndex"/> define which mip map level, and/or array index will receive the data.  If omitted, the first mip level and/or array
        /// index is used. Like the <paramref name="destRectangle"/>, these values are clipped against the <see cref="MipLevels"/> and <see cref="ArrayCount"/> values respectively.
        /// </para>
        /// <para>
        /// The <paramref name="copyMode"/> parameter defines how the copy will be performed. If the texture has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Dynamic"/> or
        /// <see cref="ResourceUsage.Default"/> and the <paramref name="copyMode"/> is set to <see cref="CopyMode.Discard"/> then the contents of the texture are discarded before updating, if it is set to
        /// <see cref="CopyMode.NoOverwrite"/>, then the data will be copied to the destination if we know the GPU is not using the portion being updated. If the <paramref name="copyMode"/> is set to
        /// <see cref="CopyMode.None"/>, then <see cref="CopyMode.Discard"/> is used. For textures created with a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Staging"/>, the
        /// <see cref="CopyMode"/> will be ignored and act as though <see cref="CopyMode.None"/> were passed.
        /// </para>
        /// <para>
        /// Please note that no format conversion, or image manipulation (other than clipping against the <paramref name="destRectangle"/>) is performed by this method. So it is up to the user to ensure
        /// that their source data is in the correct format and at the correct size.
        /// </para>
        /// <note type="caution">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </remarks>
        /// <seealso cref="IGorgonImage"/>
        /// <seealso cref="IGorgonImageBuffer"/>
        /// <seealso cref="BufferFormat"/>
        /// <seealso cref="CopyMode"/>
        /// <example>
        /// The following is an example showing how to upload an image into a texture using different techniques:
        /// <code lang="csharp">
        /// <![CDATA[
        /// using DX = SharpDX;
        ///
        /// IGorgonImage image = ... // Load an image from a source.
        /// var texture = new GorgonTexture2D(graphics, new GorgonTexture2DInfo
        /// {
        ///    Width = image.Width,
        ///    Height = image.Height,
        ///    Format = image.Format,
        ///    ArrayCount = 4,
        ///    MipLevels = 4,
        ///    // This will trigger a direct upload to the GPU, use Dynamic or Staging for CPU writable uploads.
        ///    // Dynamic is useful if the texture needs updating once or more per frame.
        ///    Usage = ResourceUsage.Default  
        /// });
        ///
        /// // Set the image to the first array and mip level at the full size.
        /// texture.SetData(image.Buffers[0]);
        ///
        /// // Set the image to the 3rd array index, and 2nd mip level, at position 10x10 on the texture, with a width and height of 50x50.
        /// // Also, set it so that we're copying to another
        /// texture.SetData(image.Buffers[0], new DX.Rectangle(10, 10, 50, 50), 2, 2, copyMode: CopyMode.NoOverwrite);
        /// 
        /// // Set a portion of the source image.
        /// texture.SetData(image.Buffers[0].GetRegion(new DX.Rectangle(10, 10, 50, 50));
        /// ]]>
        /// </code>
        /// </example>
        public void SetData(IGorgonImageBuffer imageBuffer, in DX.Rectangle? destRectangle = null, int destArrayIndex = 0, int destMipLevel = 0, CopyMode copyMode = CopyMode.None)
        {
#if DEBUG
            if (Usage == ResourceUsage.Immutable)
            {
                throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_IS_DYNAMIC_OR_IMMUTABLE);
            }

            if ((MultisampleInfo.Count > 1) || (MultisampleInfo.Quality > 0))
            {
                throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_MULTISAMPLED);
            }

            if ((Binding & TextureBinding.DepthStencil) == TextureBinding.DepthStencil)
            {
                throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_BINDING_TYPE_CANNOT_BE_USED, TextureBinding.DepthStencil));
            }

            if ((imageBuffer.FormatInformation.SizeInBytes != FormatInformation.SizeInBytes)
                || (imageBuffer.FormatInformation.IsCompressed != FormatInformation.IsCompressed)
                || (imageBuffer.FormatInformation.IsPacked != FormatInformation.IsPacked))
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FORMAT_MISMATCH, imageBuffer.Format, Format), nameof(imageBuffer));
            }
#endif

            destMipLevel = destMipLevel.Min(MipLevels - 1).Max(0);
            destArrayIndex = destArrayIndex.Min(ArrayCount - 1).Max(0);

            // Calculate mip width and height.
            int width = (Width >> destMipLevel).Max(1);
            int height = (Height >> destMipLevel).Max(1);

            // Clip the destination rectangle against our texture size.
            DX.Rectangle destRect = destRectangle ?? new DX.Rectangle(0, 0, width, height);
            var maxRect = new DX.Rectangle(0, 0, width, height);
            DX.Rectangle.Intersect(ref destRect, ref maxRect, out DX.Rectangle destBounds);

            var finalBounds = new DX.Rectangle(destBounds.X, destBounds.Y, imageBuffer.Width.Min(destBounds.Width), imageBuffer.Height.Min(destBounds.Height));

            unsafe
            {
                // If we have a default usage, then update using update subresource.
                if (Usage is not ResourceUsage.Dynamic and not ResourceUsage.Staging)
                {
                    Graphics.D3DDeviceContext.UpdateSubresource1(D3DResource,
                                                                 D3D11.Resource.CalculateSubResourceIndex(destMipLevel, destArrayIndex, MipLevels),
                                                                 new D3D11.ResourceRegion
                                                                 {
                                                                     Front = 0,
                                                                     Left = finalBounds.Left,
                                                                     Top = finalBounds.Top,
                                                                     Back = 1,
                                                                     Right = finalBounds.Right,
                                                                     Bottom = finalBounds.Bottom
                                                                 },
                                                                 imageBuffer.Data,
                                                                 imageBuffer.PitchInformation.RowPitch,
                                                                 imageBuffer.PitchInformation.SlicePitch,
                                                                 (int)copyMode);
                    return;
                }

                D3D11.MapMode mapMode = D3D11.MapMode.Write;

                if (Usage == ResourceUsage.Dynamic)
                {
                    mapMode = copyMode switch
                    {
                        CopyMode.NoOverwrite => D3D11.MapMode.WriteNoOverwrite,
                        _ => D3D11.MapMode.WriteDiscard,
                    };
                }

                // Otherwise we will map and write the data.
                int subResource = D3DResource.CalculateSubResourceIndex(destMipLevel, destArrayIndex, out int _);
                DX.DataBox mapBox = Graphics.D3DDeviceContext.MapSubresource(D3DResource, subResource, mapMode, D3D11.MapFlags.None);
                byte* src = (byte*)imageBuffer.Data;
                byte* dest = (byte*)mapBox.DataPointer;

                try
                {
                    // If we're copying the full size, then just copy the slice.
                    if ((mapBox.RowPitch == imageBuffer.PitchInformation.RowPitch)
                        && (mapBox.SlicePitch == imageBuffer.PitchInformation.SlicePitch)
                        && (finalBounds.Left == 0)
                        && (finalBounds.Top == 0)
                        && (finalBounds.Width == width)
                        && (finalBounds.Height == height))
                    {
                        Unsafe.CopyBlock(dest, src, (uint)imageBuffer.PitchInformation.SlicePitch);
                        return;
                    }

                    // Copy per-scanline if the width and height do not match up.
                    uint bytesCopy = (uint)(finalBounds.Width * FormatInformation.SizeInBytes);
                    int destOffset = finalBounds.Left * (mapBox.RowPitch / width);

                    for (int y = finalBounds.Top; y < finalBounds.Bottom; ++y)
                    {
                        byte* destPtr = dest + (y * mapBox.RowPitch) + destOffset;

                        Unsafe.CopyBlock(destPtr, src, bytesCopy);

                        src += imageBuffer.PitchInformation.RowPitch;
                    }
                }
                finally
                {
                    Graphics.D3DDeviceContext.UnmapSubresource(D3DResource, subResource);
                }
            }
        }

        /// <summary>
        /// Function to convert the texture data at the specified array index, and/or mip map level into a <see cref="GorgonImage"/>.
        /// </summary>
        /// <param name="mipLevel">The mip level in the texture to copy from.</param>
        /// <param name="arrayIndex">[Optional] The array index in the texture to copy from.</param>
        /// <returns>A new <see cref="IGorgonImage"/> containing the data in the array index and mip level.</returns>
        /// <exception cref="GorgonException">Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> set to <see cref="ResourceUsage.Immutable"/>.</exception>
        /// <remarks>
        /// <para>
        /// If the <paramref name="arrayIndex"/> is passed in, then only that array index will be copied to the resulting image. If it is omitted, then the entire array will be returned.
        /// </para>
        /// </remarks>
        public IGorgonImage ToImage(int mipLevel, int? arrayIndex = null)
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

                mipLevel = mipLevel.Min(MipLevels - 1).Max(0);
                int index = 0;

                if (arrayIndex is not null)
                {
                    index = arrayIndex.Value.Min(ArrayCount - 1).Max(0);
                }

                ImageType imageType = stagingTexture.IsCubeMap ? ImageType.ImageCube : ImageType.Image2D;

                image = new GorgonImage(new GorgonImageInfo(imageType, stagingTexture.Format)
                {
                    Width = (Width >> mipLevel).Max(1),
                    Height = (Height >> mipLevel).Max(1),
                    Depth = 1,
                    ArrayCount = arrayIndex is null ? 1 : ArrayCount - index,
                    MipCount = 1
                });


                // Copy the data from the texture.
                for (int i = index; i < image.ArrayCount; ++i)
                {
                    GetTextureData(stagingTexture, i, mipLevel, image.Buffers[mipLevel, i]);
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
		/// Function to convert this texture to a <see cref="IGorgonImage"/>.
		/// </summary>
		/// <returns>A new <see cref="IGorgonImage"/> containing the texture data.</returns>
		/// <exception cref="GorgonException">Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> set to <see cref="ResourceUsage.Immutable"/>.</exception>
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
        public DX.Point ToPixel(Vector2 texelCoordinates) => new((int)(texelCoordinates.X * Width), (int)(texelCoordinates.Y * Height));

        /// <summary>
        /// Function to convert a pixel coordinate into a texel coordinate.
        /// </summary>
        /// <param name="pixelCoordinates">The pixel coordinate to convert.</param>
        /// <returns>The texel coordinates.</returns>
        public Vector2 ToTexel(DX.Point pixelCoordinates) => new(pixelCoordinates.X / (float)Width, pixelCoordinates.Y / (float)Height);

        /// <summary>
        /// Function to convert a pixel coordinate into a texel coordinate.
        /// </summary>
        /// <param name="pixelCoordinates">The pixel coordinate to convert.</param>
        /// <returns>The texel coordinates.</returns>
        public Vector2 ToTexel(Vector2 pixelCoordinates) => new(pixelCoordinates.X / Width, pixelCoordinates.Y / Height);

        /// <summary>
        /// Function to convert a texel size into a pixel size.
        /// </summary>
        /// <param name="texelCoordinates">The texel size to convert.</param>
        /// <returns>The pixel size.</returns>
        public DX.Size2 ToPixel(DX.Size2F texelCoordinates) => new((int)(texelCoordinates.Width * Width), (int)(texelCoordinates.Height * Height));

        /// <summary>
        /// Function to convert a pixel size into a texel size.
        /// </summary>
        /// <param name="pixelCoordinates">The pixel size to convert.</param>
        /// <returns>The texel size.</returns>
        public DX.Size2F ToTexel(DX.Size2 pixelCoordinates) => new(pixelCoordinates.Width / (float)Width, pixelCoordinates.Height / (float)Height);

        /// <summary>
        /// Function to convert a texel rectangle into a pixel rectangle.
        /// </summary>
        /// <param name="texelCoordinates">The texel rectangle to convert.</param>
        /// <returns>The pixel rectangle.</returns>
        public DX.Rectangle ToPixel(DX.RectangleF texelCoordinates) => new()
        {
            Left = (int)(texelCoordinates.Left * Width),
            Top = (int)(texelCoordinates.Top * Height),
            Right = (int)(texelCoordinates.Right * Width),
            Bottom = (int)(texelCoordinates.Bottom * Height)
        };

        /// <summary>
        /// Function to convert a pixel rectangle into a texel rectangle.
        /// </summary>
        /// <param name="pixelCoordinates">The pixel rectangle to convert.</param>
        /// <returns>The texel rectangle.</returns>
        public DX.RectangleF ToTexel(DX.Rectangle pixelCoordinates) => new()
        {
            Left = pixelCoordinates.Left / (float)Width,
            Top = pixelCoordinates.Top / (float)Height,
            Right = pixelCoordinates.Right / (float)Width,
            Bottom = pixelCoordinates.Bottom / (float)Height
        };

        /// <summary>
        /// Function to create a new <see cref="GorgonTexture2DView"/> for this texture.
        /// </summary>
        /// <param name="format">[Optional] The format for the view.</param>
        /// <param name="firstMipLevel">[Optional] The first mip map level (slice) to start viewing from.</param>
        /// <param name="mipCount">[Optional] The number of mip map levels to view.</param>
        /// <param name="arrayIndex">[Optional] The array index to start viewing from.</param>
        /// <param name="arrayCount">[Optional] The number of array indices to view.</param>
        /// <returns>A <see cref="GorgonTexture2DView"/> used to bind the texture to a shader.</returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="format"/> is a typeless format.</exception>
        /// <exception cref="GorgonException">
        /// Thrown when this texture does not have a <see cref="TextureBinding"/> of <see cref="TextureBinding.ShaderResource"/>.
        /// <para>-or-</para>
        /// <para>Thrown when this texture has a usage of <see cref="ResourceUsage.Staging"/>.</para>
        /// <para>-or-</para>
        /// <para>Thrown if the texture <see cref="Format"/> is not typeless, and the <see cref="Binding"/> is set to <see cref="TextureBinding.DepthStencil"/>.</para>
        /// <para></para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will create a view that makes a texture accessible to shaders. This allows viewing of the texture data in a different format, or even a subsection of the texture from within the shader.
        /// </para>
        /// <para>
        /// The <paramref name="format"/> parameter is used present the texture data as another format type to the shader. If this value is left at the default of <see cref="BufferFormat.Unknown"/>, then 
        /// the format from the this texture is used. The <paramref name="format"/> must be castable to the format of this texture. If it is not, an exception will be thrown.
        /// </para>
        /// <para>
        /// The <paramref name="firstMipLevel"/> and <paramref name="mipCount"/> parameters define the starting mip level and the number of mip levels to allow access to within the shader. If these values fall 
        /// outside of the range of available mip levels, then they will be clipped to the upper and lower bounds of the mip chain. If these values are left at 0, then all mip levels will be accessible.
        /// </para>
        /// <para>
        /// The <paramref name="arrayIndex"/> and <paramref name="arrayCount"/> parameters define the starting array index and the number of array indices to allow access to within the shader. If these values 
        /// are left at 0, then all array indices will be accessible. 
        /// </para>
        /// </remarks>
	    public GorgonTexture2DView GetShaderResourceView(BufferFormat format = BufferFormat.Unknown, int firstMipLevel = 0, int mipCount = 0, int arrayIndex = 0, int arrayCount = 0)
        {
            if (format == BufferFormat.Unknown)
            {
                format = _info.Format;
            }

            if (format == BufferFormat.Unknown)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_VIEW_UNKNOWN_FORMAT, BufferFormat.Unknown), nameof(format));
            }

            if ((Usage == ResourceUsage.Staging)
                || ((Binding & TextureBinding.ShaderResource) != TextureBinding.ShaderResource))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_NOT_SHADER_RESOURCE, Name));
            }

            GorgonFormatInfo formatInfo = FormatInformation;

            if (format != Format)
            {
                formatInfo = new GorgonFormatInfo(format);
            }

            if (formatInfo.IsTypeless)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_VIEW_NO_TYPELESS, nameof(format));
            }

            if ((!FormatInformation.IsTypeless) && ((Binding & TextureBinding.DepthStencil) == TextureBinding.DepthStencil))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_DEPTHSTENCIL_TYPED_SHADER_RESOURCE);
            }

            firstMipLevel = firstMipLevel.Max(0).Min(MipLevels - 1);
            arrayIndex = arrayIndex.Max(0).Min(ArrayCount - 1);

            if (mipCount <= 0)
            {
                mipCount = MipLevels - firstMipLevel;
            }

            mipCount = mipCount.Min(MipLevels - firstMipLevel).Max(1);

            if (arrayCount <= 0)
            {
                arrayCount = ArrayCount - arrayIndex;
            }

            arrayCount = (arrayCount.Min(ArrayCount - arrayIndex)).Max(1);

            var key = new TextureViewKey(format, firstMipLevel, mipCount, arrayIndex, arrayCount);

            if ((_cachedSrvs.TryGetValue(key, out GorgonTexture2DView view))
                && (view.Native is not null))
            {
                return view;
            }

            if (view is not null)
            {
                _cachedSrvs.Remove(key);
            }

            view = new GorgonTexture2DView(this, format, formatInfo, firstMipLevel, mipCount, arrayIndex, arrayCount);
            view.CreateNativeView();
            _cachedSrvs[key] = view;

            return view;
        }

        /// <summary>
        /// Function to create a new <see cref="GorgonTexture2DReadWriteView"/> for this texture.
        /// </summary>
        /// <param name="format">[Optional] The format for the view.</param>
        /// <param name="firstMipLevel">[Optional] The first mip map level (slice) to start viewing from.</param>
        /// <param name="arrayIndex">[Optional] The array index to start viewing from.</param>
        /// <param name="arrayCount">[Optional] The number of array indices to view.</param>
        /// <returns>A <see cref="GorgonTexture2DReadWriteView"/> used to bind the texture to a shader.</returns>
        /// <exception cref="GorgonException">
        /// <para>Thrown when this texture does not have a <see cref="TextureBinding"/> of <see cref="TextureBinding.ReadWriteView"/>.</para>
        /// <para>-or-</para>
        /// <para>Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Staging"/>.</para>
        /// <para>-or-</para>
        /// <para>Thrown if the this texture uses multisampling.</para>
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="format"/> is typeless or is not a supported format for unordered access views.</exception>
        /// <remarks>
        /// <para>
        /// This will create an unordered access view that makes a texture accessible to shaders using unordered access to the data. This allows viewing of the texture data in a different 
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
        /// The <paramref name="arrayIndex"/> and <paramref name="arrayCount"/> parameters define the starting array index and the number of array indices to allow access to within the shader. If 
        /// these values are left at 0, then all array indices will be accessible. 
        /// </para>
        /// </remarks>
        public GorgonTexture2DReadWriteView GetReadWriteView(BufferFormat format = BufferFormat.Unknown, int firstMipLevel = 0, int arrayIndex = 0, int arrayCount = 0)
        {
            if ((Usage == ResourceUsage.Staging)
                || ((Binding & TextureBinding.ReadWriteView) != TextureBinding.ReadWriteView))
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

            if ((Graphics.FormatSupport[format].FormatSupport & BufferFormatSupport.TypedUnorderedAccessView) != BufferFormatSupport.TypedUnorderedAccessView)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_UAV_FORMAT_INVALID, format), nameof(format));
            }

            // Ensure the size of the data type fits the requested format.
            var info = new GorgonFormatInfo(format);

            if (info.IsTypeless)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_VIEW_NO_TYPELESS, nameof(format));
            }

            if (((info.Group != BufferFormat.R32_Typeless) && (FormatInformation.Group != info.Group))
                || (info.SizeInBytes != FormatInformation.SizeInBytes))
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_VIEW_CANNOT_CAST_FORMAT,
                                                        Format,
                                                        format), nameof(format));
            }

            firstMipLevel = firstMipLevel.Max(0).Min(MipLevels - 1);
            arrayIndex = arrayIndex.Max(0).Min(ArrayCount - 1);

            if (arrayCount <= 0)
            {
                arrayCount = _info.ArrayCount - arrayIndex;
            }

            arrayCount = arrayCount.Min(ArrayCount - arrayIndex).Max(1);

            var key = new TextureViewKey(format, firstMipLevel, _info.MipLevels, arrayIndex, arrayCount);

            if ((_cachedReadWriteViews.TryGetValue(key, out GorgonTexture2DReadWriteView view))
                && (view.Native is not null))
            {
                return view;
            }

            if (view is not null)
            {
                _cachedReadWriteViews.Remove(key);
            }

            view = new GorgonTexture2DReadWriteView(this, format, info, firstMipLevel, arrayIndex, arrayCount);
            view.CreateNativeView();
            _cachedReadWriteViews[key] = view;

            return view;
        }

        /// <summary>
        /// Function to create a new <see cref="GorgonDepthStencil2DView"/> for this texture.
        /// </summary>
        /// <param name="format">[Optional] The format for the view.</param>
        /// <param name="firstMipLevel">[Optional] The first mip map level (slice) to start viewing from.</param>
        /// <param name="arrayIndex">[Optional] The array index to start viewing from.</param>
        /// <param name="arrayCount">[Optional] The number of array indices to view.</param>
        /// <param name="flags">[Optional] Flags to define how this view should be accessed by the shader.</param>
        /// <returns>A <see cref="GorgonDepthStencil2DView"/> used to bind the texture as a depth/stencil buffer.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="format"/> is not supported as a depth/stencil format.</exception>
        /// <exception cref="GorgonException">Thrown when this texture does not have a <see cref="TextureBinding"/> of <see cref="TextureBinding.DepthStencil"/>.
        /// <para>-or-</para>
        /// <para>Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Staging"/>.</para>
        /// <para>-or-</para>
        /// <para>Thrown if this texture has a <see cref="Binding"/> of <see cref="TextureBinding.ShaderResource"/>, but the texture format is not a typeless format.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// The depth/stencil views take a <see cref="DepthStencilViewFlags"/> parameter that determine how a shader can access the depth buffer when it is bound to the pipeline for reading. By specifying 
        /// a single flag, the depth can write to the opposite plane (e.g. read only depth and write only stencil, write only depth and read only stencil) of the texture. This allows for multiple 
        /// depth/stencil views to be bound to the pipeline for reading and writing.
        /// </para>
        /// <para>
        /// If the <see cref="Binding"/> for the texture includes <see cref="TextureBinding.ShaderResource"/>, then the <paramref name="format"/> for the view and the <see cref="Format"/> for the texture
        /// must be specific values.  These values are listed below:
        /// <list type="table">
        ///     <listheader><term>Depth Format</term><term>Texture Format</term></listheader>
        ///     <item><term><see cref="BufferFormat.D32_Float_S8X24_UInt"/></term><term><see cref="BufferFormat.R32G8X24_Typeless"/></term></item>
        ///     <item><term><see cref="BufferFormat.D24_UNorm_S8_UInt"/></term><term><see cref="BufferFormat.R24G8_Typeless"/></term></item>
        ///     <item><term><see cref="BufferFormat.D32_Float"/></term><term><see cref="BufferFormat.R32_Typeless"/></term></item>
        ///     <item><term><see cref="BufferFormat.D16_UNorm"/></term><term><see cref="BufferFormat.R16_Typeless"/></term></item>
        /// </list>
        /// </para>
        /// </remarks>
        public GorgonDepthStencil2DView GetDepthStencilView(BufferFormat format = BufferFormat.Unknown, int firstMipLevel = 0, int arrayIndex = 0, int arrayCount = 0, DepthStencilViewFlags flags = DepthStencilViewFlags.None)
        {
            if (format == BufferFormat.Unknown)
            {
                format = _info.Format;
            }

            if ((Binding & TextureBinding.DepthStencil) != TextureBinding.DepthStencil)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_VIEW_RESOURCE_NOT_DEPTHSTENCIL, Name));
            }

            if ((!FormatInformation.IsTypeless) && ((Binding & TextureBinding.ShaderResource) == TextureBinding.ShaderResource))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_DEPTHSTENCIL_TYPED_SHADER_RESOURCE);
            }

            // Validate the format for the view.
            // If we have a typeless format for the texture, then it's likely we want to read it using a shader resource view.
            format = format switch
            {
                BufferFormat.R32G8X24_Typeless or BufferFormat.D32_Float_S8X24_UInt => BufferFormat.D32_Float_S8X24_UInt,
                BufferFormat.R24G8_Typeless or BufferFormat.D24_UNorm_S8_UInt => BufferFormat.D24_UNorm_S8_UInt,
                BufferFormat.R16_Typeless or BufferFormat.D16_UNorm => BufferFormat.D16_UNorm,
                BufferFormat.R32_Typeless or BufferFormat.D32_Float => BufferFormat.D32_Float,
                _ => throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FORMAT_NOT_SUPPORTED, format)),
            };
            GorgonFormatInfo formatInfo = FormatInformation;

            if (Format != format)
            {
                formatInfo = new GorgonFormatInfo(format);
            }

            firstMipLevel = firstMipLevel.Max(0).Min(MipLevels - 1);
            arrayIndex = arrayIndex.Max(0).Min(ArrayCount - 1);

            if (arrayCount <= 0)
            {
                arrayCount = _info.ArrayCount - arrayIndex;
            }

            arrayCount = arrayCount.Min(_info.ArrayCount - arrayIndex).Max(1);

            // Since we don't use the mip count, we can repurpose it to store the flag settings.
            var key = new TextureViewKey(format, firstMipLevel, (int)flags, arrayIndex, arrayCount);

            if ((_cachedDsvs.TryGetValue(key, out GorgonDepthStencil2DView view))
                && (view.Native is not null))
            {
                return view;
            }

            if (view is not null)
            {
                _cachedDsvs.Remove(key);
            }

            view = new GorgonDepthStencil2DView(this, format, formatInfo, firstMipLevel, arrayIndex, arrayCount, flags);
            view.CreateNativeView();
            _cachedDsvs[key] = view;

            return view;
        }

        /// <summary>
        /// Function to create a new <see cref="GorgonRenderTarget2DView"/> for this texture.
        /// </summary>
        /// <param name="format">[Optional] The format for the view.</param>
        /// <param name="firstMipLevel">[Optional] The first mip map level (slice) to start viewing from.</param>
        /// <param name="arrayIndex">[Optional] The array index start viewing from.</param>
        /// <param name="arrayCount">[Optional] The number of array indices to view.</param>
        /// <returns>A <see cref="GorgonTexture2DView"/> used to bind the texture to a shader.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="format"/> is typeless.</exception>
        /// <exception cref="GorgonException">Thrown when this texture does not have a <see cref="TextureBinding"/> of <see cref="TextureBinding.RenderTarget"/>.
        /// <para>-or-</para>
        /// <para>Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Staging"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will create a view that allows a texture to become a render target. This allows rendering into texture data in a different format, or even a subsection of the texture.
        /// </para>
        /// <para>
        /// The <paramref name="format"/> parameter is used present the texture data as another format type to the shader. If this value is left at the default of <see cref="BufferFormat.Unknown"/>, then
        /// the format from the this texture is used. The <paramref name="format"/> must be castable to the format of this texture. If it is not, an exception will be thrown.
        /// </para>
        /// <para>
        /// The <paramref name="firstMipLevel"/> parameter will be constrained to the number of mip levels for the texture should it be set to less than 0 or greater than the number of mip levels. If this 
        /// value is left at 0, then only the top mip level is used.
        /// </para>
        /// <para>
        /// The <paramref name="arrayIndex"/> and <paramref name="arrayCount"/> parameters will define the starting array index and the number of array indices to render into. If these values are left at 0,
        /// then the entire array is used to receive rendering data.
        /// </para>
        /// </remarks>
        public GorgonRenderTarget2DView GetRenderTargetView(BufferFormat format = BufferFormat.Unknown, int firstMipLevel = 0, int arrayIndex = 0, int arrayCount = 0)
        {
            if (format == BufferFormat.Unknown)
            {
                format = _info.Format;
            }

            if (format == BufferFormat.Unknown)
            {
                format = Format;
            }

            if ((Usage == ResourceUsage.Staging)
                || ((Binding & TextureBinding.RenderTarget) != TextureBinding.RenderTarget))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_RESOURCE_IS_NOT_RENDERTARGET, Name));
            }

            GorgonFormatInfo formatInfo = FormatInformation;
            if (format != Format)
            {
                formatInfo = new GorgonFormatInfo(Format);
            }

            if (formatInfo.IsTypeless)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_VIEW_NO_TYPELESS, nameof(format));
            }

            firstMipLevel = firstMipLevel.Max(0).Min(MipLevels - 1);
            arrayIndex = arrayIndex.Max(0);

            if (arrayCount <= 0)
            {
                arrayCount = _info.ArrayCount - arrayIndex;
            }

            arrayCount = arrayCount.Min(_info.ArrayCount - arrayIndex).Max(1);

            var key = new TextureViewKey(format, firstMipLevel, 1, arrayIndex, arrayCount);

            if ((_cachedRtvs.TryGetValue(key, out GorgonRenderTarget2DView view))
                && (view.Native is not null))
            {
                return view;
            }

            if (view is not null)
            {
                _cachedRtvs.Remove(key);
            }

            view = new GorgonRenderTarget2DView(this, format, formatInfo, firstMipLevel, arrayIndex, arrayCount);
            view.CreateNativeView();
            _cachedRtvs[key] = view;

            return view;
        }

        /// <summary>
        /// Function to load a texture from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="graphics">The graphics interface that will own the texture.</param>
        /// <param name="stream">The stream containing the texture image data.</param>
        /// <param name="codec">The codec that is used to decode the the data in the stream.</param>
        /// <param name="size">[Optional] The size of the image in the stream, in bytes.</param>
        /// <param name="options">[Optional] Options used to further define the texture.</param>
        /// <returns>A new <see cref="GorgonTexture2D"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="stream"/>, or the <paramref name="codec"/> parameter is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown if the <paramref name="stream"/> is write only.</exception>
        /// <exception cref="EndOfStreamException">Thrown if reading the image would move beyond the end of the <paramref name="stream"/>.</exception>
        /// <remarks>
        /// <para>
        /// This will load an <see cref="IGorgonImage"/> from a <paramref name="stream"/> and put it into a <see cref="GorgonTexture2D"/> object.
        /// </para>
        /// <para>
        /// If the <paramref name="size"/> option is specified, then the method will read from the stream up to that number of bytes, so it is up to the user to provide an accurate size. If it is omitted 
        /// then the <c>stream length - stream position</c> is used as the total size.
        /// </para>
        /// <para>
        /// If specified, the <paramref name="options"/>parameter will define how Gorgon and shaders should handle the texture.  The <see cref="GorgonTextureLoadOptions"/> type contains the following:
        /// <list type="bullet">
        ///		<item>
        ///			<term>Binding</term>
        ///			<description>When defined, will indicate the <see cref="TextureBinding"/> that defines how the texture will be bound to the graphics pipeline. If it is omitted, then the binding will be 
        ///         <see cref="TextureBinding.ShaderResource"/>.</description>
        ///		</item>
        ///		<item>
        ///			<term>Usage</term>
        ///			<description>When defined, will indicate the preferred usage for the texture. If it is omitted, then the usage will be set to <see cref="ResourceUsage.Default"/>.</description>
        ///		</item>
        ///		<item>
        ///			<term>Multisample info</term>
        ///			<description>When defined (i.e. not <b>null</b>), defines the multisampling to apply to the texture. If omitted, then the default is <see cref="GorgonMultisampleInfo.NoMultiSampling"/>.</description>
        ///		</item>
        ///		<item>
        ///		    <term>ConvertToPremultipliedAlpha</term>
        ///		    <description>Converts the image to premultiplied alpha before uploading the image data to the texture.</description>
        ///		</item>
        /// </list>
        /// </para>
        /// </remarks>
        public static GorgonTexture2D FromStream(GorgonGraphics graphics, Stream stream, IGorgonImageCodec codec, long? size = null, GorgonTexture2DLoadOptions options = null)
        {
            if (graphics is null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (codec is null)
            {
                throw new ArgumentNullException(nameof(codec));
            }

            if (!stream.CanRead)
            {
                throw new IOException(Resources.GORGFX_ERR_STREAM_WRITE_ONLY);
            }

            if (size is null)
            {
                size = stream.Length - stream.Position;
            }

            if ((stream.Length - stream.Position) < size)
            {
                throw new EndOfStreamException();
            }

            if (options is null)
            {
                options = _defaultLoadOptions;
            }

            if (string.IsNullOrEmpty(options.Name))
            {
                options.Name = GenerateName(NamePrefix);
            }

            using IGorgonImage image = codec.FromStream(stream, size);
            if (options.ConvertToPremultipliedAlpha)
            {
                image.BeginUpdate()
                     .ConvertToPremultipliedAlpha()
                     .EndUpdate();
            }

            return new GorgonTexture2D(graphics, image, options);
        }

        /// <summary>
        /// Function to load a texture from a file..
        /// </summary>
        /// <param name="graphics">The graphics interface that will own the texture.</param>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="codec">The codec that is used to decode the the data in the stream.</param>
        /// <param name="options">[Optional] Options used to further define the texture.</param>
        /// <returns>A new <see cref="GorgonTexture2D"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="filePath"/>, or the <paramref name="codec"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
        /// <remarks>
        /// <para>
        /// This will load an <see cref="IGorgonImage"/> from a file on disk and put it into a <see cref="GorgonTexture2D"/> object.
        /// </para>
        /// <para>
        /// If specified, the <paramref name="options"/>parameter will define how Gorgon and shaders should handle the texture.  The <see cref="GorgonTextureLoadOptions"/> type contains the following:
        /// <list type="bullet">
        ///		<item>
        ///			<term>Binding</term>
        ///			<description>When defined, will indicate the <see cref="TextureBinding"/> that defines how the texture will be bound to the graphics pipeline. If it is omitted, then the binding will be 
        ///         <see cref="TextureBinding.ShaderResource"/>.</description>
        ///		</item>
        ///		<item>
        ///			<term>Usage</term>
        ///			<description>When defined, will indicate the preferred usage for the texture. If it is omitted, then the usage will be set to <see cref="ResourceUsage.Default"/>.</description>
        ///		</item>
        ///		<item>
        ///			<term>Multisample info</term>
        ///			<description>When defined (i.e. not <b>null</b>), defines the multisampling to apply to the texture. If omitted, then the default is <see cref="GorgonMultisampleInfo.NoMultiSampling"/>.</description>
        ///		</item>
        ///		<item>
        ///		    <term>ConvertToPremultipliedAlpha</term>
        ///		    <description>Converts the image to premultiplied alpha before uploading the image data to the texture.</description>
        ///		</item>
        /// </list>
        /// </para>
        /// </remarks>
        public static GorgonTexture2D FromFile(GorgonGraphics graphics, string filePath, IGorgonImageCodec codec, GorgonTexture2DLoadOptions options = null)
        {
            if (graphics is null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (filePath is null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentEmptyException(nameof(filePath));
            }

            if (codec is null)
            {
                throw new ArgumentNullException(nameof(codec));
            }

            if (options is null)
            {
                options = _defaultLoadOptions;
            }

            if (string.IsNullOrEmpty(options.Name))
            {
                options.Name = GenerateName(NamePrefix);
            }

            using IGorgonImage image = codec.FromFile(filePath);
            if (options.ConvertToPremultipliedAlpha)
            {
                image.BeginUpdate()
                     .ConvertToPremultipliedAlpha()
                     .EndUpdate();
            }

            return new GorgonTexture2D(graphics, image, options);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            // We cannot dispose of a texture that has a render target owned by a factory. We'll let the factory deal with the destruction of this object.
            if ((_cachedRtvs is not null) && (_cachedRtvs.Values.Any(item => item.OwnerFactory is not null)))
            {
                return;
            }

            DXGI.Resource sharedRes = Interlocked.Exchange(ref _sharedResource, null);
            sharedRes?.Dispose();

            // Destroy all cached views.
            Dictionary<TextureViewKey, GorgonTexture2DView> cachedSrvs = Interlocked.Exchange(ref _cachedSrvs, null);
            Dictionary<TextureViewKey, GorgonRenderTarget2DView> cachedRtvs = Interlocked.Exchange(ref _cachedRtvs, null);
            Dictionary<TextureViewKey, GorgonDepthStencil2DView> cachedDsvs = Interlocked.Exchange(ref _cachedDsvs, null);
            Dictionary<TextureViewKey, GorgonTexture2DReadWriteView> cachedReadWriteViews = Interlocked.Exchange(ref _cachedReadWriteViews, null);

            if (cachedSrvs is not null)
            {
                foreach (KeyValuePair<TextureViewKey, GorgonTexture2DView> view in cachedSrvs)
                {
                    view.Value.Dispose();
                }
            }

            if (cachedRtvs is not null)
            {
                foreach (KeyValuePair<TextureViewKey, GorgonRenderTarget2DView> view in cachedRtvs)
                {
                    view.Value.Dispose();
                }
            }

            if (cachedDsvs is not null)
            {
                foreach (KeyValuePair<TextureViewKey, GorgonDepthStencil2DView> view in cachedDsvs)
                {
                    view.Value.Dispose();
                }
            }

            if (cachedReadWriteViews is not null)
            {
                foreach (KeyValuePair<TextureViewKey, GorgonTexture2DReadWriteView> view in cachedReadWriteViews)
                {
                    view.Value.Dispose();
                }
            }

            Graphics.Log.Print($"'{Name}': Destroying D3D11 Texture.", LoggingLevel.Simple);

            base.Dispose();
        }

        /// <summary>Function to retrieve a default shader resource view.</summary>
        /// <returns>The default shader resource view for the texture.</returns>
        GorgonShaderResourceView IGorgonTextureResource.GetShaderResourceView() => GetShaderResourceView();
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="GorgonTexture2D" /> class.</summary>
        /// <param name="graphics">The graphics interface used to create this texture.</param>
        /// <param name="surface">The pointer to an external rendering surface.</param>
        internal GorgonTexture2D(GorgonGraphics graphics, nint surface)
            : base(graphics)
        {
            D3D11.Texture2DDescription desc;

            using (var com = new DX.ComObject(surface))
            {
                if (com is null)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_POINTER_NOT_COM_OBJECT);
                }

                _sharedResource = com.QueryInterface<DXGI.Resource>();

                if (_sharedResource is null)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_POINTER_NOT_DXGI_RESOURCE);
                }

                D3DResource = graphics.D3DDevice.OpenSharedResource<D3D11.Resource>(_sharedResource.SharedHandle);

                if (D3DResource is null)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_DXGI_RESOURCE_IS_NOT_D3D_RESOURCE);
                }

                using D3D11.Texture2D texture = D3DResource.QueryInterface<D3D11.Texture2D>();
                if (texture is null)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_D3D_RESOURCE_IS_NOT_2D_TEXTURE);
                }

                desc = texture.Description;
            }

            // Get the info from the back buffer texture.
            _info = new GorgonTexture2DInfo(desc.Width, desc.Height, (BufferFormat)desc.Format)
            {
                Name = D3DResource.DebugName,
                Usage = (ResourceUsage)desc.Usage,
                ArrayCount = desc.ArraySize,
                MipLevels = desc.MipLevels,
                IsCubeMap = false,
                MultisampleInfo = GorgonMultisampleInfo.NoMultiSampling,
                Binding = (TextureBinding)desc.BindFlags
            };
            
            FormatInformation = new GorgonFormatInfo(Format);
            TextureID = Interlocked.Increment(ref _textureID);
            SizeInBytes = CalculateSizeInBytes(_info);

            this.RegisterDisposable(graphics);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTexture2D"/> class.
        /// </summary>
        /// <param name="swapChain">The swap chain that holds the back buffers to retrieve.</param>
        /// <param name="index">The index of the back buffer to retrieve.</param>
        /// <remarks>
        /// <para>
        /// This constructor is used internally to create a render target texture from a swap chain.
        /// </para>
        /// </remarks>
        internal GorgonTexture2D(GorgonSwapChain swapChain, int index)
            : base(swapChain.Graphics)
        {
            swapChain.Graphics.Log.Print($"Swap Chain '{swapChain.Name}': Creating texture from back buffer index {index}.", LoggingLevel.Simple);

            D3D11.Texture2D texture;

            // Get the resource from the swap chain.
            D3DResource = texture = D3D11.Resource.FromSwapChain<D3D11.Texture2D>(swapChain.DXGISwapChain, index);
            D3DResource.DebugName = $"Swap Chain '{swapChain.Name}' BackBufferTexture_ID3D11Texture2D1 #{index}.";

            // Get the info from the back buffer texture.
            _info = new GorgonTexture2DInfo(texture.Description.Width, texture.Description.Height, (BufferFormat)texture.Description.Format)
            {
                Name = D3DResource.DebugName,
                Usage = (ResourceUsage)texture.Description.Usage,
                ArrayCount = texture.Description.ArraySize,
                MipLevels = texture.Description.MipLevels,
                IsCubeMap = false,
                MultisampleInfo = GorgonMultisampleInfo.NoMultiSampling,
                Binding = (TextureBinding)texture.Description.BindFlags
            };

            FormatInformation = new GorgonFormatInfo(Format);
            TextureID = Interlocked.Increment(ref _textureID);
            SizeInBytes = CalculateSizeInBytes(_info);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTexture2D"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface used to create this texture.</param>
        /// <param name="image">The image to copy into the texture.</param>
        /// <param name="options">The options to use when loading the texture.</param>
        /// <remarks>
        /// <para>
        /// This constructor is used when converting an image to a texture.
        /// </para>
        /// </remarks>
        internal GorgonTexture2D(GorgonGraphics graphics, IGorgonImage image, GorgonTexture2DLoadOptions options)
            : base(graphics)
        {
            bool isCubeMap;
            if (options.IsTextureCube is null)
            {
                isCubeMap = image.ImageType == ImageType.ImageCube;
            }
            else
            {
                isCubeMap = options.IsTextureCube.Value;
            }

            _info = new GorgonTexture2DInfo(image.Width, image.Height, image.Format)
            {
                Name = options.Name,
                Format = image.Format,
                Width = image.Width,
                Height = image.Height,
                Usage = options.Usage,
                ArrayCount = image.ArrayCount,
                Binding = options.Binding,
                IsCubeMap = isCubeMap,
                MipLevels = image.MipCount,
                MultisampleInfo = options.MultisampleInfo
            };

            Initialize(image);
            TextureID = Interlocked.Increment(ref _textureID);
            SizeInBytes = CalculateSizeInBytes(_info);

            this.RegisterDisposable(graphics);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTexture2D"/> class.
        /// </summary>
        /// <param name="graphics">The <see cref="GorgonGraphics"/> interface that created this texture.</param>
        /// <param name="textureInfo">A <see cref="IGorgonTexture2DInfo"/> object describing the properties of this texture.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="textureInfo"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <see cref="GorgonGraphicsResource.Usage"/> is set to <c>Immutable</c>.</exception>
        /// <exception cref="GorgonException">Thrown when the texture could not be created due to misconfiguration.</exception>
        /// <remarks>
        /// <para>
        /// This constructor creates an empty texture. Data may be uploaded to the texture at a later time if its <see cref="GorgonGraphicsResource.Usage"/> is not set to 
        /// <see cref="ResourceUsage.Immutable"/>. If the <see cref="GorgonGraphicsResource.Usage"/> is set to <see cref="ResourceUsage.Immutable"/> with this constructor, then an exception will be thrown. 
        /// To use an immutable texture, use the <see cref="GorgonImageTextureExtensions.ToTexture2D(IGorgonImage, GorgonGraphics, GorgonTexture2DLoadOptions)"/> extension method on the <see cref="IGorgonImage"/> type.
        /// </para>
        /// </remarks>
        public GorgonTexture2D(GorgonGraphics graphics, GorgonTexture2DInfo textureInfo)
            : base(graphics)
        {
            _info = new GorgonTexture2DInfo(textureInfo ?? throw new ArgumentNullException(nameof(textureInfo)));

            Initialize(null);

            TextureID = Interlocked.Increment(ref _textureID);

            SizeInBytes = CalculateSizeInBytes(_info);


            this.RegisterDisposable(graphics);
        }
        #endregion
    }
}
