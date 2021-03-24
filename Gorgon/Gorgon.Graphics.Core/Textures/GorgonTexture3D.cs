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
using System.Numerics;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using DX = SharpDX;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A texture used to project an image onto a graphic primitive such as a triangle.
    /// </summary>
    public sealed class GorgonTexture3D
        : GorgonGraphicsResource, IGorgonTexture3DInfo, IGorgonTextureResource
    {
        #region Constants.
        /// <summary>
        /// The prefix used for generated names.
        /// </summary>
        internal const string NamePrefix = nameof(GorgonTexture3D);
        #endregion

        #region Variables.
        // Default texture loading options.
        private static readonly GorgonTextureLoadOptions _defaultLoadOptions = new();
        // The ID number of the texture.
        private static int _textureID;
        // The list of cached texture unordered access views.
        private Dictionary<TextureViewKey, GorgonTexture3DReadWriteView> _cachedReadWriteViews = new();
        // The list of cached texture shader resource views.
        private Dictionary<TextureViewKey, GorgonTexture3DView> _cachedSrvs = new();
        // The list of cached render target resource views.
        private Dictionary<TextureViewKey, GorgonRenderTarget3DView> _cachedRtvs = new();
        // The information used to create the texture.
        private GorgonTexture3DInfo _info;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the bind flags used for the D3D 11 resource.
        /// </summary>
        internal override D3D11.BindFlags BindFlags => (D3D11.BindFlags)Binding;

        /// <summary>
        /// Property to return the type of image data.
        /// </summary>
        ImageType IGorgonImageInfo.ImageType => ImageType.Image3D;

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
        /// Property to return the total number of images there are in an image array.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This only applies to 1D and 2D images.  This parameter will be set to a value of 1 for a 3D image.
        /// </para>
        /// </remarks>
        int IGorgonImageInfo.ArrayCount => 1;

        /// <summary>
        /// Property to return whether the size of the texture is a power of 2 or not.
        /// </summary>
        bool IGorgonImageInfo.IsPowerOfTwo => ((Width == 0) || (Width & (Width - 1)) == 0)
                                              && ((Height == 0) || (Height & (Height - 1)) == 0)
                                              && ((Depth == 0) || (Depth & (Depth - 1)) == 0);


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
        public override GraphicsResourceType ResourceType => GraphicsResourceType.Texture3D;

        /// <summary>
        /// Property to return the width of the texture, in pixels.
        /// </summary>
        public int Width => _info.Width;

        /// <summary>
        /// Property to return the height of the texture, in pixels.
        /// </summary>
        public int Height => _info.Height;

        /// <summary>
        /// Property to return the depth of the texture, in slices.
        /// </summary>
        public int Depth => _info.Depth;

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
        #endregion

        #region Methods.
        /// <summary>
        /// Function to transfer texture data into an image buffer.
        /// </summary>
        /// <param name="texture">The texture to copy from.</param>
        /// <param name="depthSlice">The depth slice to copy from.</param>
        /// <param name="mipLevel">The mip level to copy from.</param>
        /// <param name="buffer">The buffer to copy into.</param>
        /// <param name="singleSlice"><b>true</b> to copy a single slice, <b>false</b> to copy the entire slice.</param>
        private static unsafe void GetTextureData(GorgonTexture3D texture, int depthSlice, int mipLevel, IGorgonImageBuffer buffer, bool singleSlice)
        {
            int depthCount = singleSlice ? depthSlice + 1 : 1.Max(buffer.Depth);
            int height = 1.Max(buffer.Height);
            int rowStride = buffer.PitchInformation.RowPitch;
            int sliceStride = buffer.PitchInformation.SlicePitch;

            // If this image is compressed, then use the block height information.
            if (buffer.PitchInformation.VerticalBlockCount > 0)
            {
                height = buffer.PitchInformation.VerticalBlockCount;
            }

            // Copy the texture data into the buffer.
            int subResource = D3D11.Resource.CalculateSubResourceIndex(mipLevel, 0, texture.MipLevels);
            DX.DataBox lockBox = texture.Graphics.D3DDeviceContext.MapSubresource(texture.D3DResource,
                                                                                  subResource,
                                                                                  D3D11.MapMode.Read,
                                                                                  D3D11.MapFlags.None);
            try
            {
                byte* bufferPtr = (byte*)buffer.Data;
                byte* sourceData = (byte*)lockBox.DataPointer + (depthSlice * lockBox.SlicePitch);

                // If the strides don't match, then the texture is using padding, so copy one scanline at a time for each depth index.
                if ((lockBox.RowPitch != rowStride)
                    || (lockBox.SlicePitch != sliceStride))
                {
                    for (int depth = 0; depth < depthCount; depth++)
                    {
                        // Restart at the padded slice size.
                        byte* destptr = bufferPtr + (depthSlice * sliceStride);

                        for (int row = 0; row < height; row++)
                        {
                            Unsafe.CopyBlock(destptr, sourceData, (uint)rowStride.Min(lockBox.RowPitch));
                            sourceData += lockBox.RowPitch;
                            destptr += rowStride;
                        }
                    }
                }
                else
                {
                    for (int depth = 0; depth < depthCount; depth++)
                    {
                        // Since we have the same row and slice stride, copy everything in one shot.
                        Unsafe.CopyBlock(bufferPtr, sourceData, (uint)sliceStride);
                        sourceData += lockBox.SlicePitch;
                        bufferPtr += sliceStride;
                    }
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
            if ((Binding & TextureBinding.DepthStencil) == TextureBinding.DepthStencil)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_DEPTH_STENCIL_NOT_SUPPORTED));
            }

            var formatInfo = new GorgonFormatInfo(Format);

            if (Usage == ResourceUsage.Dynamic)
            {
                if (MipLevels > 1)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_DYN_TEXTURE_MUST_HAVE_1_MIP, MipLevels));
                }
            }

            if ((Usage == ResourceUsage.Staging) && (Binding != TextureBinding.None))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_STAGING_NO_BINDINGS, Binding));
            }

            // Ensure that we can actually use our requested format as a texture.
            if ((Format == BufferFormat.Unknown) || (!Graphics.FormatSupport[Format].IsTextureFormat(ImageType.Image3D)))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_FORMAT_NOT_SUPPORTED, Format, @"3D"));
            }

            // Validate unordered access binding.
            ValidateUnorderedAccess(Graphics.FormatSupport[Format].FormatSupport);

            // Validate render target binding.
            ValidateRenderTarget();

            if ((Width > Graphics.VideoAdapter.MaxTexture3DWidth) || (Width < 1))
            {
                throw new GorgonException(GorgonResult.CannotCreate,
                                            string.Format(Resources.GORGFX_ERR_TEXTURE_WIDTH_INVALID, @"3D", Graphics.VideoAdapter.MaxTexture3DWidth));
            }

            if ((Height > Graphics.VideoAdapter.MaxTexture3DHeight) || (Height < 1))
            {
                throw new GorgonException(GorgonResult.CannotCreate,
                                            string.Format(Resources.GORGFX_ERR_TEXTURE_HEIGHT_INVALID, @"3D", Graphics.VideoAdapter.MaxTexture3DWidth));
            }

            if ((Depth > Graphics.VideoAdapter.MaxTexture3DDepth) || (Depth < 1))
            {
                throw new GorgonException(GorgonResult.CannotCreate,
                                          string.Format(Resources.GORGFX_ERR_TEXTURE_DEPTH_INVALID, @"3D", Graphics.VideoAdapter.MaxTexture3DDepth));
            }

            // Ensure the number of mip levels is not outside of the range for the width/height.
            int mipLevels = MipLevels.Min(GorgonImage.CalculateMaxMipCount(Width, Height, 1)).Max(1);

#if NET5_0_OR_GREATER
            if (mipLevels != _info.MipLevels)
            {
                _info = _info with
                {
                    MipLevels = mipLevels
                };
            }
#endif

            if (MipLevels > 1)
            {
                if ((Graphics.FormatSupport[Format].FormatSupport & BufferFormatSupport.Mip) != BufferFormatSupport.Mip)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_NO_MIP_SUPPORT, Format));
                }
            }

            if ((formatInfo.IsCompressed) && (((Width % 4) != 0)
                                              || ((Height % 4) != 0)))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_TEXTURE_BC_SIZE_NOT_MOD_4);
            }
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

            var tex3DDesc = new D3D11.Texture3DDescription1
            {
                Format = (Format)Format,
                Width = Width,
                Height = Height,
                Depth = Depth,
                Usage = (D3D11.ResourceUsage)Usage,
                BindFlags = (D3D11.BindFlags)Binding,
                CpuAccessFlags = cpuFlags,
                OptionFlags = D3D11.ResourceOptionFlags.None,
                MipLevels = MipLevels
            };

            D3DResource = ResourceFactory.Create(Graphics.D3DDevice, Name, TextureID, in tex3DDesc, image);
        }


        /// <summary>
        /// Function to copy this texture into another <see cref="GorgonTexture3D"/>.
        /// </summary>
        /// <param name="destTexture">The <see cref="GorgonTexture3D"/> that will receive a copy of this texture.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destTexture"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">
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
        /// copied, use the <see cref="CopyTo(GorgonTexture3D, in DX.Rectangle?, GorgonRange?, int, int, int, int, int, CopyMode)"/> method.
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
        private void CopyResource(GorgonTexture3D destTexture)
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
        /// <param name="depth">The depth of the texture.</param>
        /// <param name="format">The format for the texture.</param>
        /// <param name="mipCount">The number of mip map levels.</param>
        /// <returns>The number of bytes for the texture.</returns>
        public static int CalculateSizeInBytes(int width, int height, int depth, BufferFormat format, int mipCount) => GorgonImage.CalculateSizeInBytes(ImageType.Image3D,
                                                    width,
                                                    height,
                                                    depth,
                                                    format,
                                                    mipCount);

        /// <summary>
        /// Function to calculate the size of a texture, in bytes with the given parameters.
        /// </summary>
        /// <param name="info">The <see cref="IGorgonTexture3DInfo"/> used to define a texture.</param>
        /// <returns>The number of bytes for the texture.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
        public static int CalculateSizeInBytes(IGorgonTexture3DInfo info) => info is null
                ? throw new ArgumentNullException(nameof(info))
                : CalculateSizeInBytes(info.Width,
                                        info.Height,
                                        info.Depth,
                                        info.Format,
                                        info.MipLevels);

        /// <summary>
        /// Function to copy this texture into a <see cref="GorgonTexture1D"/>.
        /// </summary>
        /// <param name="destinationTexture">The texture to copy into.</param>
        /// <param name="sourceRange">[Optional] The dimensions of the source area to copy.</param>
        /// <param name="sourceY">[Optional] The vertical position in the texture to copy.</param>
        /// <param name="sourceDepthSlice">[Optional] The depth slice of the sub resource to copy.</param>
        /// <param name="sourceMipLevel">[Optional] The mip map level of the sub resource to copy.</param>
        /// <param name="destX">[Optional] Horizontal offset into the destination texture to place the copied data.</param>
        /// <param name="destArrayIndex">[Optional] The array index of the destination sub resource to copy into.</param>
        /// <param name="destMipLevel">[Optional] The mip map level of the destination sub resource to copy into.</param>
        /// <param name="copyMode">[Optional] Defines how data should be copied into the texture.</param>
        /// <exception cref="ArgumentNullException">Thrown when the texture parameter is <b>null</b>.</exception>
        /// <exception cref="NotSupportedException">Thrown when the formats cannot be converted because they're not of the same group.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="destinationTexture"/> is the same as this texture, and the <paramref name="sourceDepthSlice"/>, <paramref name="destArrayIndex"/>, <paramref name="sourceMipLevel"/> and the <paramref name="destMipLevel"/> 
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
        public void CopyTo(GorgonTexture1D destinationTexture, GorgonRange? sourceRange = null, int sourceY = 0, int sourceDepthSlice = 0, int sourceMipLevel = 0, int destX = 0, int destArrayIndex = 0, int destMipLevel = 0, CopyMode copyMode = CopyMode.None)
        {
            destinationTexture.ValidateObject(nameof(destinationTexture));

            // If we're trying to place the image data outside of this texture, then leave.
            if ((destX >= destinationTexture.Width)
                || (sourceY < 0)
                || (sourceY >= Height)
                || (sourceDepthSlice < 0)
                || (sourceDepthSlice >= Depth))
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
            var srcDepth = new GorgonRange(sourceDepthSlice.Min(Depth - 1).Max(0), (sourceDepthSlice + 1).Min(Depth).Max(1));
            sourceMipLevel = sourceMipLevel.Min(MipLevels - 1).Max(0);
            destArrayIndex = destArrayIndex.Min(destinationTexture.ArrayCount - 1).Max(0);
            destMipLevel = destMipLevel.Min(destinationTexture.MipLevels - 1).Max(0);

            int sourceResource = D3D11.Resource.CalculateSubResourceIndex(sourceMipLevel, 0, MipLevels);
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
                                                             new D3D11.ResourceRegion(rect.Left,
                                                                                      rect.Top,
                                                                                      srcDepth.Minimum,
                                                                                      rect.Right,
                                                                                      rect.Bottom,
                                                                                      srcDepth.Maximum),
                                                             (int)copyMode);
        }

        /// <summary>
        /// Function to copy this texture into a <see cref="GorgonTexture2D"/>.
        /// </summary>
        /// <param name="destinationTexture">The texture to copy into.</param>
        /// <param name="sourceRectangle">[Optional] The dimensions of the source area to copy.</param>
        /// <param name="sourceDepthSlice">[Optional] The depth slice of the sub resource to copy.</param>
        /// <param name="sourceMipLevel">[Optional] The mip map level of the sub resource to copy.</param>
        /// <param name="destX">[Optional] Horizontal offset into the destination texture to place the copied data.</param>
        /// <param name="destY">[Optional] Vertical offset into the destination texture to place the copied data.</param>
        /// <param name="destArrayIndex">[Optional] The array index of the destination sub resource to copy into.</param>
        /// <param name="destMipLevel">[Optional] The mip map level of the destination sub resource to copy into.</param>
        /// <param name="copyMode">[Optional] Defines how data should be copied into the texture.</param>
        /// <exception cref="ArgumentNullException">Thrown when the texture parameter is <b>null</b>.</exception>
        /// <exception cref="NotSupportedException">Thrown when the formats cannot be converted because they're not of the same group.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="destinationTexture"/> is the same as this texture, and the <paramref name="sourceDepthSlice"/>, <paramref name="destArrayIndex"/>, <paramref name="sourceMipLevel"/> and the <paramref name="destMipLevel"/> 
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
        public void CopyTo(GorgonTexture2D destinationTexture, in DX.Rectangle? sourceRectangle = null, int sourceDepthSlice = 0, int sourceMipLevel = 0, int destX = 0, int destY = 0, int destArrayIndex = 0, int destMipLevel = 0, CopyMode copyMode = CopyMode.None)
        {
            destinationTexture.ValidateObject(nameof(destinationTexture));

            // If we're trying to place the image data outside of the destination texture, then leave.
            if ((destX >= destinationTexture.Width)
                || (destY >= destinationTexture.Height)
                || (sourceDepthSlice < 0)
                || (sourceDepthSlice >= Depth))
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
            var srcDepth = new GorgonRange(sourceDepthSlice.Min(Depth - 1).Max(0), (sourceDepthSlice + 1).Min(Depth).Max(1));
            sourceMipLevel = sourceMipLevel.Min(MipLevels - 1).Max(0);
            destArrayIndex = destArrayIndex.Min(destinationTexture.ArrayCount - 1).Max(0);
            destMipLevel = destMipLevel.Min(destinationTexture.MipLevels - 1).Max(0);

            int sourceResource = D3D11.Resource.CalculateSubResourceIndex(sourceMipLevel, 0, MipLevels);
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
                                                             new D3D11.ResourceRegion(rect.Left,
                                                                                      rect.Top,
                                                                                      srcDepth.Minimum,
                                                                                      rect.Right,
                                                                                      rect.Bottom,
                                                                                      srcDepth.Maximum),
                                                             (int)copyMode);
        }

        /// <summary>
        /// Function to copy this texture into a <see cref="GorgonTexture3D"/>.
        /// </summary>
        /// <param name="destinationTexture">The texture to copy into.</param>
        /// <param name="sourceRectangle">[Optional] The dimensions of the source area to copy.</param>
        /// <param name="sourceDepthSliceRange">[Optional] The depth slice range of the sub resource to copy.</param>
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
        public void CopyTo(GorgonTexture3D destinationTexture, in DX.Rectangle? sourceRectangle = null, GorgonRange? sourceDepthSliceRange = null, int sourceMipLevel = 0, int destX = 0, int destY = 0, int destZ = 0, int destMipLevel = 0, CopyMode copyMode = CopyMode.None)
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

            if ((sourceRectangle is null) && (sourceDepthSliceRange is null) && (sourceMipLevel == 0)
                && (destX == 0) && (destY == 0) && (destZ == 0) && (destMipLevel == 0)
                && (MipLevels == destinationTexture.MipLevels)
                && (Width == destinationTexture.Width) && (Height == destinationTexture.Height) && (Depth == destinationTexture.Depth)
                && ((Format == destinationTexture.Format) || (FormatInformation.Group == destinationTexture.FormatInformation.Group)))
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
            GorgonRange srcDepth = sourceDepthSliceRange is null
                                       ? new GorgonRange(0, Depth)
                                       : new GorgonRange(sourceDepthSliceRange.Value.Minimum.Min(Depth - 1).Max(0), sourceDepthSliceRange.Value.Maximum.Min(Depth).Max(1));

            if (srcDepth.Maximum <= srcDepth.Minimum)
            {
                return;
            }

            // Ensure the indices are clipped to our settings.
            sourceMipLevel = sourceMipLevel.Min(MipLevels - 1).Max(0);
            srcDepth = new GorgonRange(srcDepth.Minimum.Min(destinationTexture.Depth - 1).Max(0), srcDepth.Maximum.Min(destinationTexture.Depth).Max(1));
            destMipLevel = destMipLevel.Min(MipLevels - 1).Max(0);
            destZ = destZ.Min(destinationTexture.Depth - 1).Max(0);

            int sourceResource = D3D11.Resource.CalculateSubResourceIndex(sourceMipLevel, 0, MipLevels);
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

            if ((this == destinationTexture) && (sourceResource == destResource))
            {
                throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_CANNOT_COPY_SAME_SUBRESOURCE);
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
                                                             new D3D11.ResourceRegion(rect.Left, rect.Top, srcDepth.Minimum, rect.Right, rect.Bottom, srcDepth.Maximum),
                                                             (int)copyMode);
        }

        /// <summary>
        /// Function to get a staging texture from this texture.
        /// </summary>
        /// <returns>A new <see cref="GorgonTexture3D"/> containing a copy of the data in this texture, with a usage of <c>Staging</c>.</returns>
        /// <exception cref="GorgonException">Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> of <c>Immutable</c>.</exception>
        /// <remarks>
        /// <para>
        /// This allows an application to make a copy of the texture for editing on the CPU. The resulting staging texture, once edited, can then be reuploaded to the same texture, or another texture.
        /// </para>
        /// </remarks>
        public GorgonTexture3D GetStagingTexture()
        {
            if (Usage == ResourceUsage.Immutable)
            {
                throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_ERR_TEXTURE_IMMUTABLE));
            }

#if NET5_0_OR_GREATER
            GorgonTexture3DInfo info = _info with
            {
                Name = $"{Name}_[Staging]",
                Usage = ResourceUsage.Staging,
                Binding = TextureBinding.None
            };
            var staging = new GorgonTexture3D(Graphics, info);

            // Copy the data from this texture into the new staging texture.
            CopyTo(staging);

            return staging;
#else
            return null;
#endif
        }

        /// <summary>
        /// Function to update the texture, or a sub section of the texture with data from a <see cref="IGorgonImageBuffer"/> contained within a <see cref="IGorgonImage"/>.
        /// </summary>
        /// <param name="imageBuffer">The image buffer that contains the data to copy.</param>
        /// <param name="destRectangle">[Optional] The region on the texture to update.</param>
        /// <param name="destSlice">[Optional] The depth slice on the texture to update.</param>
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
        /// If the user supplies a <paramref name="destRectangle"/> and/or <paramref name="destSlice"/>, then the data will be copied to the region in the texture specified by the first parameter and the
        /// depth slice specified by the second, otherwise if the parameter is omitted, the full size of the texture (depending on mip level) is used. This value is clipped against the width/height/depth
        /// of the mip level (e.g. A 256x256x64 image at mip level 2 would be 64x64x16).
        /// </para>
        /// <para>
        /// The <paramref name="destMipLevel"/> defines which mip map level will receive the data.  If omitted, the first mip level index is used. Like the <paramref name="destRectangle"/>, this value is
        /// clipped against the <see cref="MipLevels"/> value.
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
        /// var texture = new GorgonTexture3D(graphics, new GorgonTexture3DInfo
        /// {
        ///    Width = image.Width,
        ///    Height = image.Height,
        ///    Format = image.Format,
        ///    Depth = image.Depth,
        ///    MipLevels = 4,
        ///    // This will trigger a direct upload to the GPU, use Dynamic or Staging for CPU writable uploads.
        ///    // Dynamic is useful if the texture needs updating once or more per frame.
        ///    Usage = ResourceUsage.Default  
        /// });
        ///
        /// // Set the image to the first depth slice and mip level at the full size.
        /// texture.SetData(image.Buffers[0]);
        ///
        /// // Set the image to the 4th depth slice, and 2nd mip level, at position 10x10 on the texture, with a width and height of 50x50.
        /// // Also, set it so that we're copying to another
        /// texture.SetData(image.Buffers[0], new DX.Rectangle(10, 10, 50, 50), 3, 2, copyMode: CopyMode.NoOverwrite);
        /// 
        /// // Set a portion of the source image.
        /// texture.SetData(image.Buffers[0].GetRegion(new DX.Rectangle(10, 10, 50, 50));
        /// ]]>
        /// </code>
        /// </example>
        public void SetData(IGorgonImageBuffer imageBuffer, in DX.Rectangle? destRectangle = null, int destSlice = 0, int destMipLevel = 0, CopyMode copyMode = CopyMode.None)
        {
#if DEBUG
            if (Usage == ResourceUsage.Immutable)
            {
                throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_IS_DYNAMIC_OR_IMMUTABLE);
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
            destSlice = destSlice.Min((Depth >> destMipLevel).Max(1) - 1).Max(0);

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
                                                                 D3D11.Resource.CalculateSubResourceIndex(destMipLevel, destSlice, MipLevels),
                                                                 new D3D11.ResourceRegion
                                                                 {
                                                                     Front = destSlice,
                                                                     Left = finalBounds.Left,
                                                                     Top = finalBounds.Top,
                                                                     Back = destSlice + 1,
                                                                     Right = finalBounds.Right,
                                                                     Bottom = finalBounds.Bottom
                                                                 },
                                                                 new IntPtr(imageBuffer.Data),
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
                int subResource = D3DResource.CalculateSubResourceIndex(destMipLevel, 0, out int _);
                DX.DataBox mapBox = Graphics.D3DDeviceContext.MapSubresource(D3DResource, subResource, mapMode, D3D11.MapFlags.None);
                byte* src = (byte*)imageBuffer.Data;
                byte* dest = (byte*)mapBox.DataPointer + (destSlice * mapBox.SlicePitch);

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
        /// <param name="depthSlice">[Optional] The depthSlice in the texture to copy from.</param>
        /// <returns>A new <see cref="IGorgonImage"/> containing the data in the array index and mip level.</returns>
        /// <exception cref="GorgonException">Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> set to <see cref="ResourceUsage.Immutable"/>.</exception>
        /// <remarks>
        /// <para>
        /// If the <paramref name="depthSlice"/> is passed in, then only a single depth slice is created for the image. If it is omitted, then the entire slice for the <paramref name="mipLevel"/> is
        /// returned.
        /// </para>
        /// </remarks>
        public IGorgonImage ToImage(int mipLevel, int? depthSlice = null)
        {
            if (Usage == ResourceUsage.Immutable)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_IMMUTABLE));
            }

            GorgonTexture3D stagingTexture = this;
            GorgonImage image = null;

            try
            {
                if (Usage != ResourceUsage.Staging)
                {
                    stagingTexture = GetStagingTexture();
                }

                mipLevel = mipLevel.Min(MipLevels - 1).Max(0);
                int slice = 0;

                if (depthSlice is not null)
                {
                    slice = depthSlice.Value.Min(Depth - 1).Max(0);
                }

                image = new GorgonImage(new GorgonImageInfo(ImageType.Image3D, stagingTexture.Format)
                {
                    Width = (Width >> mipLevel).Max(1),
                    Height = (Height >> mipLevel).Max(1),
                    Depth = depthSlice is null ? 1 : ((Depth - slice) >> mipLevel).Max(1),
                    ArrayCount = 1,
                    MipCount = 1
                });

                // Copy the data from the texture.
                GetTextureData(stagingTexture, slice, mipLevel, image.Buffers[mipLevel], depthSlice is not null);

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

            GorgonTexture3D stagingTexture = this;
            GorgonImage image = null;

            try
            {
                if (Usage != ResourceUsage.Staging)
                {
                    stagingTexture = GetStagingTexture();
                }

                image = new GorgonImage(new GorgonImageInfo(ImageType.Image3D, stagingTexture.Format)
                {
                    Width = Width,
                    Height = Height,
                    Depth = Depth,
                    ArrayCount = 1,
                    MipCount = MipLevels
                });

                for (int mipLevel = 0; mipLevel < stagingTexture.MipLevels; mipLevel++)
                {
                    // Get the buffer for the array and mip level.
                    IGorgonImageBuffer buffer = image.Buffers[mipLevel];

                    // Copy the data from the texture.
                    GetTextureData(stagingTexture, 0, mipLevel, buffer, false);
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
        /// Function to convert a texel coordinate into a pixel coordinate and a depth slice.
        /// </summary>
        /// <param name="texelCoordinates">The texel coordinates to convert.</param>
        /// <returns>The pixel coordinates.</returns>
        public (DX.Point, int) ToPixel(Vector3 texelCoordinates) => (new DX.Point((int)(texelCoordinates.X * Width), (int)(texelCoordinates.Y * Height)), (int)(texelCoordinates.Z * Depth));

        /// <summary>
        /// Function to convert a pixel coordinate into a texel coordinate.
        /// </summary>
        /// <param name="pixelCoordinates">The pixel coordinate to convert.</param>
        /// <param name="depthSlice">The depth slice to convert.</param>
        /// <returns>The texel coordinates.</returns>
        public Vector3 ToTexel(DX.Point pixelCoordinates, int depthSlice) => new(pixelCoordinates.X / (float)Width, pixelCoordinates.Y / (float)Height, depthSlice / (float)Depth);

        /// <summary>
        /// Function to convert a pixel coordinate into a texel coordinate.
        /// </summary>
        /// <param name="pixelCoordinates">The pixel coordinate to convert.</param>
        /// <returns>The texel coordinates.</returns>
        public Vector3 ToTexel(Vector3 pixelCoordinates) => new(pixelCoordinates.X / Width, pixelCoordinates.Y / Height, pixelCoordinates.Z / Depth);

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
        /// Function to create a new <see cref="GorgonTexture3DView"/> for this texture.
        /// </summary>
        /// <param name="format">[Optional] The format for the view.</param>
        /// <param name="firstMipLevel">[Optional] The first mip map level (slice) to start viewing from.</param>
        /// <param name="mipCount">[Optional] The number of mip map levels to view.</param>
        /// <returns>A <see cref="GorgonTexture3DView"/> used to bind the texture to a shader.</returns>
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
        /// </remarks>
        public GorgonTexture3DView GetShaderResourceView(BufferFormat format = BufferFormat.Unknown, int firstMipLevel = 0, int mipCount = 0)
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

            if (mipCount <= 0)
            {
                mipCount = MipLevels - firstMipLevel;
            }

            mipCount = mipCount.Min(MipLevels - firstMipLevel).Max(1);

            var key = new TextureViewKey(format, firstMipLevel, mipCount, 0, 0);

            if ((_cachedSrvs.TryGetValue(key, out GorgonTexture3DView view))
                && (view.Native is not null))
            {
                return view;
            }

            if (view is not null)
            {
                _cachedSrvs.Remove(key);
            }

            view = new GorgonTexture3DView(this, format, formatInfo, firstMipLevel, mipCount);
            view.CreateNativeView();
            _cachedSrvs[key] = view;

            return view;
        }

        /// <summary>
        /// Function to create a new <see cref="GorgonTexture3DReadWriteView"/> for this texture.
        /// </summary>
        /// <param name="format">[Optional] The format for the view.</param>
        /// <param name="firstMipLevel">[Optional] The first mip map level (slice) to start viewing from.</param>
        /// <param name="startDepthSlice">[Optional] The array index or depth slice to start viewing from.</param>
        /// <param name="depthSliceCount">[Optional] The number of array indices or depth slices to view.</param>
        /// <returns>A <see cref="GorgonTexture3DReadWriteView"/> used to bind the texture to a shader.</returns>
        /// <exception cref="GorgonException">
        /// <para>Thrown when this texture does not have a <see cref="TextureBinding"/> of <see cref="TextureBinding.ReadWriteView"/>.</para>
        /// <para>-or-</para>
        /// <para>Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Staging"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="format"/> is typeless or is not a supported format for unordered access views.</exception>
        /// <remarks>
        /// <para>
        /// This will create an unordered access view that makes a texture accessible to shaders using unordered access to the data. This allows viewing of the texture data in a
        /// different format, or even a subsection of the texture from within the shader.
        /// </para>
        /// <para>
        /// The <paramref name="format"/> parameter is used present the texture data as another format type to the shader. If this parameter is omitted, then the format of the texture will be used.
        /// </para>
        /// <para>
        /// The <paramref name="firstMipLevel"/> parameter defines the starting mip level to allow access to within the shader. If this value falls outside of the range of available mip levels, then it
        /// will be clipped to the upper and lower bounds of the mip chain. If this value is left at 0, then only the first mip level is used.
        /// </para>
        /// <para>
        /// The <paramref name="startDepthSlice"/> and <paramref name="depthSliceCount"/> parameters define the starting depth slice and the number of slices to allow access to within the shader. If these
        /// values are left at 0, then all array indices will be accessible. 
        /// </para>
        /// </remarks>
        public GorgonTexture3DReadWriteView GetReadWriteView(BufferFormat format = BufferFormat.Unknown, int firstMipLevel = 0, int startDepthSlice = 0, int depthSliceCount = 0)
        {
            if ((Usage == ResourceUsage.Staging)
                || ((Binding & TextureBinding.ReadWriteView) != TextureBinding.ReadWriteView))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_RESOURCE_NOT_VALID, Name));
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

            if ((FormatInformation.Group != info.Group)
                || (info.SizeInBytes != FormatInformation.SizeInBytes))
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_VIEW_CANNOT_CAST_FORMAT,
                                                        Format,
                                                        format), nameof(format));
            }

            firstMipLevel = firstMipLevel.Max(0).Min(MipLevels - 1);
            startDepthSlice = startDepthSlice.Max(0).Min(Depth - 1);

            if (depthSliceCount <= 0)
            {
                depthSliceCount = _info.Depth - startDepthSlice;
            }

            depthSliceCount = depthSliceCount.Min(Depth - startDepthSlice).Max(1);

            var key = new TextureViewKey(format, firstMipLevel, _info.MipLevels, startDepthSlice, depthSliceCount);

            if ((_cachedReadWriteViews.TryGetValue(key, out GorgonTexture3DReadWriteView view))
                && (view.Native is not null))
            {
                return view;
            }

            if (view is not null)
            {
                _cachedReadWriteViews.Remove(key);
            }

            view = new GorgonTexture3DReadWriteView(this, format, info, firstMipLevel, startDepthSlice, depthSliceCount);
            view.CreateNativeView();
            _cachedReadWriteViews[key] = view;

            return view;
        }

        /// <summary>
        /// Function to create a new <see cref="GorgonRenderTarget3DView"/> for this texture.
        /// </summary>
        /// <param name="format">[Optional] The format for the view.</param>
        /// <param name="firstMipLevel">[Optional] The first mip map level (slice) to start viewing from.</param>
        /// <param name="startDepthSlice">[Optional] The depth index to start viewing from.</param>
        /// <param name="depthSliceCount">[Optional] The number of depth slices to view.</param>
        /// <returns>A <see cref="GorgonTexture3DView"/> used to bind the texture to a shader.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="format"/> is typeless or cannot be determined from the this texture.</exception>
        /// <exception cref="GorgonException">Thrown when this texture does not have a <see cref="TextureBinding"/> of <see cref="TextureBinding.RenderTarget"/>.
        /// <para>-or-</para>
        /// <para>Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Staging"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will create a view that allows a texture to become a render target. This allows rendering into texture data in a different format, or even a subsection of the texture.
        /// </para>
        /// <para>
        /// The <paramref name="format"/> parameter is used present the texture data as another format type to the shader. If this value is left at the default of <see cref="BufferFormat.Unknown"/>, then the format from the 
        /// this texture is used. The <paramref name="format"/> must be castable to the format of this texture. If it is not, an exception will be thrown.
        /// </para>
        /// <para>
        /// The <paramref name="firstMipLevel"/> parameter will be constrained to the number of mip levels for the texture should it be set to less than 0 or greater than the number of mip levels. If this 
        /// value is left at 0, then only the top mip level is used.
        /// </para>
        /// <para>
        /// The <paramref name="startDepthSlice"/> and <paramref name="depthSliceCount"/> parameters will define the starting depth slice and the number of depth slices to render into. If 
        /// these values are left at 0, then the entire depth is used to receive rendering data.
        /// </para>
        /// </remarks>
        public GorgonRenderTarget3DView GetRenderTargetView(BufferFormat format = BufferFormat.Unknown, int firstMipLevel = 0, int startDepthSlice = 0, int depthSliceCount = 0)
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
            startDepthSlice = startDepthSlice.Max(0);

            if (depthSliceCount <= 0)
            {
                depthSliceCount = Depth - startDepthSlice;
            }

            depthSliceCount = depthSliceCount.Min(Depth - startDepthSlice).Max(1);

            var key = new TextureViewKey(format, firstMipLevel, 1, startDepthSlice, depthSliceCount);

            if ((_cachedRtvs.TryGetValue(key, out GorgonRenderTarget3DView view))
                && (view.Native is not null))
            {
                return view;
            }

            if (view is not null)
            {
                _cachedRtvs.Remove(key);
            }

            view = new GorgonRenderTarget3DView(this, format, formatInfo, firstMipLevel, startDepthSlice, depthSliceCount);
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
        /// <returns>A new <see cref="GorgonTexture3D"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="stream"/>, or the <paramref name="codec"/> parameter is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown if the <paramref name="stream"/> is write only.</exception>
        /// <exception cref="EndOfStreamException">Thrown if reading the image would move beyond the end of the <paramref name="stream"/>.</exception>
        /// <remarks>
        /// <para>
        /// This will load an <see cref="IGorgonImage"/> from a <paramref name="stream"/> and put it into a <see cref="GorgonTexture3D"/> object.
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
        public static GorgonTexture3D FromStream(GorgonGraphics graphics, Stream stream, IGorgonImageCodec codec, long? size = null, GorgonTextureLoadOptions options = null)
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

            return new GorgonTexture3D(graphics, image, options);
        }

        /// <summary>
        /// Function to load a texture from a file..
        /// </summary>
        /// <param name="graphics">The graphics interface that will own the texture.</param>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="codec">The codec that is used to decode the the data in the stream.</param>
        /// <param name="options">[Optional] Options used to further define the texture.</param>
        /// <returns>A new <see cref="GorgonTexture3D"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="filePath"/>, or the <paramref name="codec"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
        /// <remarks>
        /// <para>
        /// This will load an <see cref="IGorgonImage"/> from a file on disk and put it into a <see cref="GorgonTexture3D"/> object.
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
        public static GorgonTexture3D FromFile(GorgonGraphics graphics, string filePath, IGorgonImageCodec codec, GorgonTextureLoadOptions options = null)
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

            return new GorgonTexture3D(graphics, image, options);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            // Destroy all cached views.
            Dictionary<TextureViewKey, GorgonTexture3DView> cachedSrvs = Interlocked.Exchange(ref _cachedSrvs, null);
            Dictionary<TextureViewKey, GorgonRenderTarget3DView> cachedRtvs = Interlocked.Exchange(ref _cachedRtvs, null);
            Dictionary<TextureViewKey, GorgonTexture3DReadWriteView> cachedReadWriteViews = Interlocked.Exchange(ref _cachedReadWriteViews, null);

            if (cachedSrvs is not null)
            {
                foreach (KeyValuePair<TextureViewKey, GorgonTexture3DView> view in cachedSrvs)
                {
                    view.Value.Dispose();
                }
            }

            if (cachedRtvs is not null)
            {
                foreach (KeyValuePair<TextureViewKey, GorgonRenderTarget3DView> view in cachedRtvs)
                {
                    view.Value.Dispose();
                }
            }

            if (cachedReadWriteViews is not null)
            {
                foreach (KeyValuePair<TextureViewKey, GorgonTexture3DReadWriteView> view in cachedReadWriteViews)
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
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTexture3D"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface used to create this texture.</param>
        /// <param name="image">The image to copy into the texture.</param>
        /// <param name="options">The options to use when loading the texture.</param>
        /// <remarks>
        /// <para>
        /// This constructor is used when converting an image to a texture.
        /// </para>
        /// </remarks>
        internal GorgonTexture3D(GorgonGraphics graphics, IGorgonImage image, GorgonTextureLoadOptions options)
            : base(graphics)
        {
            _info = new GorgonTexture3DInfo(image.Width, image.Height, image.Depth, image.Format)
            {
                Name = options.Name,
                Usage = options.Usage,
                Binding = options.Binding,
                MipLevels = image.MipCount
            };

            Initialize(image);
            TextureID = Interlocked.Increment(ref _textureID);
            SizeInBytes = CalculateSizeInBytes(_info);

            this.RegisterDisposable(graphics);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTexture3D"/> class.
        /// </summary>
        /// <param name="graphics">The <see cref="GorgonGraphics"/> interface that created this texture.</param>
        /// <param name="textureInfo">A <see cref="IGorgonTexture3DInfo"/> object describing the properties of this texture.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="textureInfo"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <see cref="GorgonGraphicsResource.Usage"/> is set to <c>Immutable</c>.</exception>
        /// <exception cref="GorgonException">Thrown when the texture could not be created due to misconfiguration.</exception>
        /// <remarks>
        /// <para>
        /// This constructor creates an empty texture. Data may be uploaded to the texture at a later time if its <see cref="GorgonGraphicsResource.Usage"/> is not set to 
        /// <see cref="ResourceUsage.Immutable"/>. If the <see cref="GorgonGraphicsResource.Usage"/> is set to <see cref="ResourceUsage.Immutable"/> with this constructor, then an exception will be thrown. 
        /// To use an immutable texture, use the <see cref="GorgonImageTextureExtensions.ToTexture3D(IGorgonImage, GorgonGraphics, GorgonTextureLoadOptions)"/> extension method on the <see cref="IGorgonImage"/> type.
        /// </para>
        /// </remarks>
        public GorgonTexture3D(GorgonGraphics graphics, GorgonTexture3DInfo textureInfo)
            : base(graphics)
        {
            _info = new GorgonTexture3DInfo(textureInfo ?? throw new ArgumentNullException(nameof(textureInfo)));

            Initialize(null);

            TextureID = Interlocked.Increment(ref _textureID);

            SizeInBytes = CalculateSizeInBytes(_info);


            this.RegisterDisposable(graphics);
        }
#endregion
    }
}
