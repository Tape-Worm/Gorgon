
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: April 8, 2018 8:17:22 PM
// 


using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using DX = SharpDX;


namespace Gorgon.Graphics.Core;

/// <summary>
/// A texture used to project an image onto a graphic primitive such as a triangle
/// </summary>
public sealed class GorgonTexture1D
    : GorgonGraphicsResource, IGorgonTexture1DInfo, IGorgonTextureResource
{

    /// <summary>
    /// The prefix used for generated names.
    /// </summary>
    internal const string NamePrefix = nameof(GorgonTexture1D);



    // Default texture loading options.
    private static readonly GorgonTextureLoadOptions _defaultLoadOptions = new();
    // The ID number of the texture.
    private static int _textureID;
    // The list of cached texture unordered access views.
    private Dictionary<TextureViewKey, GorgonTexture1DReadWriteView> _cachedReadWriteViews = [];
    // The list of cached texture shader resource views.
    private Dictionary<TextureViewKey, GorgonTexture1DView> _cachedSrvs = [];
    // The information used to create the texture.
    private GorgonTexture1DInfo _info;

    /// <summary>
    /// Property to return the bind flags used for the D3D 11 resource.
    /// </summary>
    internal override D3D11.BindFlags BindFlags => (D3D11.BindFlags)Binding;

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
    /// Property to return the number of array levels for a 1D or 2D texture.
    /// </summary>
    public int ArrayCount => _info.ArrayCount;

    /// <summary>
    /// Property to return the format of the texture.
    /// </summary>
    public BufferFormat Format => _info.Format;

    /// <summary>
    /// Property to return the number of mip-map levels for the texture.
    /// </summary>
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

    /// <summary>
    /// Property to return the type of image data.
    /// </summary>
    ImageDataType IGorgonImageInfo.ImageType => ImageDataType.Image1D;

    /// <summary>
    /// Property to return the height of an image, in pixels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This applies to 2D and 3D images only.  This parameter will be set to a value of 1 for a 1D image.
    /// </para>
    /// </remarks>
    int IGorgonImageInfo.Height => 1;

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
    bool IGorgonImageInfo.IsPowerOfTwo => ((Width == 0) || (Width & (Width - 1)) == 0);



    /// <summary>
    /// Function to transfer texture data into an image buffer.
    /// </summary>
    /// <param name="texture">The texture to copy from.</param>
    /// <param name="arrayIndex">The index of the array to copy from.</param>
    /// <param name="mipLevel">The mip level to copy from.</param>
    /// <param name="buffer">The buffer to copy into.</param>
    private static unsafe void GetTextureData(GorgonTexture1D texture, int arrayIndex, int mipLevel, IGorgonImageBuffer buffer)
    {
        int rowStride = buffer.PitchInformation.RowPitch;

        // Copy the texture data into the buffer.
        int subResource = D3D11.Resource.CalculateSubResourceIndex(mipLevel, arrayIndex, texture.MipLevels);
        DX.DataBox lockBox = texture.Graphics.D3DDeviceContext.MapSubresource(texture.D3DResource,
                                                                              subResource,
                                                                              D3D11.MapMode.Read,
                                                                              D3D11.MapFlags.None);
        try
        {
            byte* bufferPtr = (byte*)buffer.Data;

            if (lockBox.RowPitch != rowStride)
            {
                rowStride = rowStride.Min(lockBox.RowPitch);
            }

            Unsafe.CopyBlock(bufferPtr, (byte*)lockBox.DataPointer, (uint)rowStride);
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
    /// Function to validate the settings for a texture.
    /// </summary>
    private void ValidateTextureSettings()
    {
        if ((Binding & TextureBinding.DepthStencil) == TextureBinding.DepthStencil)
        {
            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_DEPTH_STENCIL_NOT_SUPPORTED);
        }

        if ((Binding & TextureBinding.RenderTarget) == TextureBinding.RenderTarget)
        {
            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_RENDER_TARGET_NOT_SUPPORTED);
        }

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

        // Ensure that we can actually use our requested format as a texture.
        if ((Format == BufferFormat.Unknown) || (!Graphics.FormatSupport[Format].IsTextureFormat(ImageDataType.Image1D)))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_FORMAT_NOT_SUPPORTED, Format, @"1D"));
        }

        // Validate unordered access binding.
        ValidateUnorderedAccess(Graphics.FormatSupport[Format].FormatSupport);

        if ((ArrayCount > Graphics.VideoAdapter.MaxTextureArrayCount) || (ArrayCount < 1))
        {
            throw new GorgonException(GorgonResult.CannotCreate,
                                        string.Format(Resources.GORGFX_ERR_TEXTURE_ARRAYCOUNT_INVALID, Graphics.VideoAdapter.MaxTextureArrayCount));
        }

        if ((Width > Graphics.VideoAdapter.MaxTextureWidth) || (Width < 1))
        {
            throw new GorgonException(GorgonResult.CannotCreate,
                                        string.Format(Resources.GORGFX_ERR_TEXTURE_WIDTH_INVALID, @"1D", Graphics.VideoAdapter.MaxTextureWidth));
        }

        // Ensure the number of mip levels is not outside of the range for the width/height.
        int mipLevels = MipLevels.Min(GorgonImage.CalculateMaxMipCount(Width, 1, 1)).Max(1);

        if (mipLevels != _info.MipLevels)
        {
            _info = _info with
            {
                MipLevels = mipLevels
            };
        }

        if (mipLevels <= 1)
        {
            return;
        }

        if ((Graphics.FormatSupport[Format].FormatSupport & BufferFormatSupport.Mip) != BufferFormatSupport.Mip)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_NO_MIP_SUPPORT, Format));
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

        if ((Binding & TextureBinding.ReadWriteView) == TextureBinding.ReadWriteView)
        {
            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_TEXTURE_MULTISAMPLED);
        }

        var tex1DDesc = new D3D11.Texture1DDescription
        {
            Format = (Format)Format,
            Width = Width,
            ArraySize = ArrayCount,
            Usage = (D3D11.ResourceUsage)Usage,
            BindFlags = (D3D11.BindFlags)Binding,
            CpuAccessFlags = cpuFlags,
            OptionFlags = D3D11.ResourceOptionFlags.None,
            MipLevels = MipLevels
        };

        D3DResource = ResourceFactory.Create(Graphics.D3DDevice, Name, TextureID, in tex1DDesc, image);
    }

    /// <summary>
    /// Function to copy this texture into another <see cref="GorgonTexture2D"/>.
    /// </summary>
    /// <param name="destTexture">The <see cref="GorgonTexture2D"/> that will receive a copy of this texture.</param>
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
    private void CopyResource(GorgonTexture1D destTexture)
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

        if (destTexture.Usage == ResourceUsage.Immutable)
        {
            throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_IMMUTABLE);
        }

        // If the format is different, then check to see if the format group is the same.
        if ((destTexture.Format != Format) && ((destTexture.FormatInformation.Group != FormatInformation.Group)))
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_COPY_CANNOT_CONVERT, destTexture.Format, Format), nameof(destTexture));
        }

        if (destTexture.Width != Width)
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
    /// <param name="arrayCount">The number of array indices.</param>
    /// <param name="format">The format for the texture.</param>
    /// <param name="mipCount">The number of mip map levels.</param>
    /// <returns>The number of bytes for the texture.</returns>
    public static int CalculateSizeInBytes(int width, int arrayCount, BufferFormat format, int mipCount) => GorgonImage.CalculateSizeInBytes(ImageDataType.Image1D,
                                                width,
                                                1,
                                                arrayCount,
                                                format,
                                                mipCount);

    /// <summary>
    /// Function to calculate the size of a texture, in bytes with the given parameters.
    /// </summary>
    /// <param name="info">The <see cref="IGorgonTexture1DInfo"/> used to define a texture.</param>
    /// <returns>The number of bytes for the texture.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
    public static int CalculateSizeInBytes(IGorgonTexture1DInfo info) => info is null
            ? throw new ArgumentNullException(nameof(info))
            : CalculateSizeInBytes(info.Width,
                                    info.ArrayCount,
                                    info.Format,
                                    info.MipLevels);

    /// <summary>
    /// Function to copy this texture into another <see cref="GorgonTexture1D"/>.
    /// </summary>
    /// <param name="destinationTexture">The texture to copy into.</param>
    /// <param name="sourceRange">[Optional] The dimensions of the source area to copy.</param>
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
    /// Use this method to copy a specific sub resource of this <see cref="GorgonTexture1D"/> to another sub resource of a <see cref="GorgonTexture1D"/>, or to a different sub resource of the same texture.
    /// The <paramref name="sourceRange"/> coordinates must be inside of the destination, if it is not, then the source data will be clipped against the destination region.
    /// No stretching or filtering is supported by this method.
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
    public void CopyTo(GorgonTexture1D destinationTexture, GorgonRange<int>? sourceRange = null, int sourceArrayIndex = 0, int sourceMipLevel = 0, int destX = 0, int destArrayIndex = 0, int destMipLevel = 0, CopyMode copyMode = CopyMode.None)
    {
        destinationTexture.ValidateObject(nameof(destinationTexture));

        // If we're trying to place the image data outside of this texture, then leave.
        if (destX >= destinationTexture.Width)
        {
            return;
        }

        // Copy the entire thing
        if ((sourceRange is null) && (sourceArrayIndex == 0) && (sourceMipLevel == 0) && (destX == 0) && (destArrayIndex == 0) && (destMipLevel == 0)
            && (Width == destinationTexture.Width) && (destinationTexture.MipLevels == MipLevels) && (destinationTexture.ArrayCount == ArrayCount)
            && ((Format == destinationTexture.Format) || (FormatInformation.Group == destinationTexture.FormatInformation.Group)))
        {
            CopyResource(destinationTexture);
            return;
        }

        DX.Rectangle rect;

        // If we didn't specify a box to copy from, then create one.
        if (sourceRange is null)
        {
            rect = new DX.Rectangle(0, 0, Width.Min(destinationTexture.Width).Max(1), 1);
        }
        else
        {
            rect = new DX.Rectangle((sourceRange.Value.Minimum.Min(destinationTexture.Width - 1).Max(0)).Min(Width - 1), 0,
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

        if ((this == destinationTexture) && (sourceResource == destResource))
        {
            throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_CANNOT_COPY_SAME_SUBRESOURCE);
        }

        if (destinationTexture.Usage == ResourceUsage.Immutable)
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
            Top = 0,
            Right = right,
            Bottom = 1
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
    /// Function to copy this texture into a <see cref="GorgonTexture2D"/>.
    /// </summary>
    /// <param name="destinationTexture">The texture to copy into.</param>
    /// <param name="sourceRange">[Optional] The dimensions of the source area to copy.</param>
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
    /// Use this method to copy a specific sub resource of this <see cref="GorgonTexture1D"/> to another sub resource of a <see cref="GorgonTexture2D"/>
    /// The <paramref name="sourceRange"/> coordinates must be inside of the destination, if it is not, then the source data will be clipped against the destination region. No stretching or filtering is 
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
    public void CopyTo(GorgonTexture2D destinationTexture, GorgonRange<int>? sourceRange = null, int sourceArrayIndex = 0, int sourceMipLevel = 0, int destX = 0, int destY = 0, int destArrayIndex = 0, int destMipLevel = 0, CopyMode copyMode = CopyMode.None)
    {
        destinationTexture.ValidateObject(nameof(destinationTexture));

        // If we're trying to place the image data outside of the destination texture, then leave.
        if ((destX >= destinationTexture.Width)
            || (destY >= destinationTexture.Height))
        {
            return;
        }

        DX.Rectangle rect;

        // If we didn't specify a box to copy from, then create one.
        if (sourceRange is null)
        {
            rect = new DX.Rectangle(0, 0, Width.Min(destinationTexture.Width).Max(1), 1);
        }
        else
        {
            rect = new DX.Rectangle((sourceRange.Value.Minimum.Min(destinationTexture.Width - 1).Max(0)).Min(Width - 1),
                                    0,
                                    (sourceRange.Value.Maximum.Min(destinationTexture.Width).Max(1)).Min(Width),
                                    1);
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

        if (destinationTexture.Usage == ResourceUsage.Immutable)
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
        int right = rect.Right.Min(Width + left).Max(1);

        rect = new DX.Rectangle
        {
            Left = left,
            Top = 0,
            Right = right,
            Bottom = 1
        };

        // Adjust source box to fit within our destination.
        destX = destX.Min(destinationTexture.Width - 1).Max(0);
        destY = destY.Min(destinationTexture.Height - 1).Max(0);

        rect.Width = (destX + rect.Width).Min(destinationTexture.Width - destX).Max(1);

        // Nothing to copy, so get out.
        if (rect.Width <= 0)
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
    /// <param name="sourceRange">[Optional] The dimensions of the source area to copy.</param>
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
    /// Use this method to copy a specific sub resource of this <see cref="GorgonTexture1D"/> to another sub resource of a <see cref="GorgonTexture3D"/>. The <paramref name="sourceRange"/>
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
    public void CopyTo(GorgonTexture3D destinationTexture, GorgonRange<int>? sourceRange = null, int sourceArrayIndex = 0, int sourceMipLevel = 0, int destX = 0, int destY = 0, int destZ = 0, int destMipLevel = 0, CopyMode copyMode = CopyMode.None)
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
        if (sourceRange is null)
        {
            rect = new DX.Rectangle(0, 0, Width.Min(destinationTexture.Width).Max(1), 1);
        }
        else
        {
            rect = new DX.Rectangle((sourceRange.Value.Minimum.Min(destinationTexture.Width - 1).Max(0)).Min(Width - 1),
                                   0,
                                   (sourceRange.Value.Maximum.Min(destinationTexture.Width).Max(1)).Min(Width),
                                   1);
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

        if (destinationTexture.Usage == ResourceUsage.Immutable)
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
        int right = rect.Right.Min(Width + left).Max(1);

        rect = new DX.Rectangle
        {
            Left = left,
            Top = 0,
            Right = right,
            Bottom = 1
        };

        // Adjust source box to fit within our destination.
        destX = destX.Min(destinationTexture.Width - 1).Max(0);
        destY = destY.Min(destinationTexture.Height - 1).Max(0);

        rect.Width = (destX + rect.Width).Min(destinationTexture.Width - destX).Max(1);
        rect.Height = (destY + rect.Height).Min(destinationTexture.Height - destY).Max(1);

        // Nothing to copy, so get out.
        if ((rect.IsEmpty)
            || (rect.Width == 0)
            || (rect.Height == 0))
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
    /// Function to get a staging texture from this texture.
    /// </summary>
    /// <returns>A new <see cref="GorgonTexture1D"/> containing a copy of the data in this texture, with a usage of <c>Staging</c>.</returns>
    /// <exception cref="GorgonException">Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> of <c>Immutable</c>.</exception>
    /// <remarks>
    /// <para>
    /// This allows an application to make a copy of the texture for editing on the CPU. The resulting staging texture, once edited, can then be reuploaded to the same texture, or another texture.
    /// </para>
    /// </remarks>
    public GorgonTexture1D GetStagingTexture()
    {
        var info = new GorgonTexture1DInfo(_info)
        {
            Name = $"{Name}_[Staging]",
            Usage = ResourceUsage.Staging,
            Binding = TextureBinding.None
        };
        var staging = new GorgonTexture1D(Graphics, info);

        // Copy the data from this texture into the new staging texture.
        CopyTo(staging);

        return staging;
    }

    /// <summary>
    /// Function to update the texture, or a sub section of the texture with data from a <see cref="IGorgonImageBuffer"/> contained within a <see cref="IGorgonImage"/>.
    /// </summary>
    /// <param name="imageBuffer">The image buffer that contains the data to copy.</param>
    /// <param name="destinationRange">[Optional] The region on the texture to update.</param>
    /// <param name="destArrayIndex">[Optional] The array index to update.</param>
    /// <param name="destMipLevel">[Optional] The mip map level to update.</param>
    /// <param name="copyMode">[Optional] Flags to indicate how to copy the data.</param>
    /// <exception cref="NotSupportedException">Thrown if this image has a usage of <see cref="ResourceUsage.Immutable"/>, or has a binding of <see cref="TextureBinding.DepthStencil"/>.</exception>
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
    /// If the user supplies a <paramref name="destinationRange"/>, then the data will be copied to the region in the texture specified by the parameter, otherwise if the parameter is omitted, the full
    /// size of the texture (depending on mip level) is used. This value is clipped against the width/height of the mip level (e.g. A 256x256 image at mip level 2 would be 64x64).
    /// </para>
    /// <para>
    /// The <paramref name="destMipLevel"/>, and <paramref name="destArrayIndex"/> define which mip map level, and/or array index will receive the data.  If omitted, the first mip level and/or array
    /// index is used. Like the <paramref name="destinationRange"/>, these values are clipped against the <see cref="MipLevels"/> and <see cref="ArrayCount"/> values respectively.
    /// </para>
    /// <para>
    /// The <paramref name="copyMode"/> parameter defines how the copy will be performed. If the texture has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Dynamic"/> or
    /// <see cref="ResourceUsage.Default"/> and the <paramref name="copyMode"/> is set to <see cref="CopyMode.Discard"/> then the contents of the texture are discarded before updating, if it is set to
    /// <see cref="CopyMode.NoOverwrite"/>, then the data will be copied to the destination if we know the GPU is not using the portion being updated. If the <paramref name="copyMode"/> is set to
    /// <see cref="CopyMode.None"/>, then <see cref="CopyMode.Discard"/> is used. For textures created with a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Staging"/>, the
    /// <see cref="CopyMode"/> will be ignored and act as though <see cref="CopyMode.None"/> were passed.
    /// </para>
    /// <para>
    /// Please note that no format conversion, or image manipulation (other than clipping against the <paramref name="destinationRange"/>) is performed by this method. So it is up to the user to ensure
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
    /// var texture = new GorgonTexture1D(graphics, new GorgonTexture1DInfo
    /// {
    ///    Width = image.Width,
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
    /// // Set the image to the 3rd array index, and 2nd mip level, at position 10 on the texture, with width of 50.
    /// // Also, set it so that we're copying to another
    /// texture.SetData(image.Buffers[0], new GorgonRange(10, 50), 2, 2, copyMode: CopyMode.NoOverwrite);
    /// 
    /// // Set a portion of the source image.
    /// texture.SetData(image.Buffers[0].GetRegion(new DX.Rectangle(10, 10, 50, 1));
    /// ]]>
    /// </code>
    /// </example>
    public void SetData(IGorgonImageBuffer imageBuffer, GorgonRange<int>? destinationRange = null, int destArrayIndex = 0, int destMipLevel = 0, CopyMode copyMode = CopyMode.None)
    {
#if DEBUG
        if (Usage == ResourceUsage.Immutable)
        {
            throw new NotSupportedException(Resources.GORGFX_ERR_TEXTURE_IS_DYNAMIC_OR_IMMUTABLE);
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

        // Clip the destination rectangle against our texture size.
        DX.Rectangle destRange = destinationRange is null ? new DX.Rectangle(0, 0, width, 1) : new DX.Rectangle(destinationRange.Value.Minimum, 0, destinationRange.Value.Maximum, 1);
        var maxRect = new DX.Rectangle(0, 0, width, 1);
        DX.Rectangle.Intersect(ref destRange, ref maxRect, out DX.Rectangle destBounds);

        var finalBounds = new DX.Rectangle(destBounds.X, 0, imageBuffer.Width.Min(destBounds.Width), 1);

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
                                                             imageBuffer.PitchInformation.RowPitch,
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
                if ((finalBounds.Left == 0) && (finalBounds.Width == width) && (mapBox.RowPitch == imageBuffer.PitchInformation.RowPitch))
                {
                    Unsafe.CopyBlock(dest, src, (uint)imageBuffer.PitchInformation.RowPitch);
                    return;
                }

                // Copy per-scanline if the width pitches do not match up.
                Unsafe.CopyBlock(dest + (finalBounds.Left * (mapBox.RowPitch / width)), src, (uint)(finalBounds.Width * FormatInformation.SizeInBytes));
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

        GorgonTexture1D stagingTexture = this;
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

            image = new GorgonImage(new GorgonImageInfo(ImageDataType.Image1D, stagingTexture.Format)
            {
                Width = (Width >> mipLevel).Max(1),
                Height = 1,
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
        GorgonTexture1D stagingTexture = this;
        GorgonImage image = null;

        try
        {
            if (Usage != ResourceUsage.Staging)
            {
                stagingTexture = GetStagingTexture();
            }

            image = new GorgonImage(new GorgonImageInfo(ImageDataType.Image1D, stagingTexture.Format)
            {
                Width = Width,
                Height = 1,
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
    public int ToPixel(float texelCoordinates) => (int)(texelCoordinates * Width);

    /// <summary>
    /// Function to convert a pixel coordinate into a texel coordinate.
    /// </summary>
    /// <param name="pixelCoordinates">The pixel coordinate to convert.</param>
    /// <returns>The texel coordinates.</returns>
    public float ToTexel(int pixelCoordinates) => pixelCoordinates / (float)Width;

    /// <summary>
    /// Function to convert a texel rectangle into a pixel rectangle.
    /// </summary>
    /// <param name="texelCoordinates">The texel rectangle to convert.</param>
    /// <returns>The pixel rectangle.</returns>
    public GorgonRange<int> ToPixel(GorgonRange<float> texelCoordinates) => new((int)(texelCoordinates.Minimum * Width),
                                                                                (int)(texelCoordinates.Maximum * Width));

    /// <summary>
    /// Function to convert a pixel rectangle into a texel rectangle.
    /// </summary>
    /// <param name="pixelCoordinates">The pixel rectangle to convert.</param>
    /// <returns>The texel rectangle.</returns>
    public GorgonRange<float> ToTexel(GorgonRange<int> pixelCoordinates) => new(pixelCoordinates.Minimum / (float)Width,
                                                                                pixelCoordinates.Maximum / (float)Width);

    /// <summary>
    /// Function to create a new <see cref="GorgonTexture1DView"/> for this texture.
    /// </summary>
    /// <param name="format">[Optional] The format for the view.</param>
    /// <param name="firstMipLevel">[Optional] The first mip map level (slice) to start viewing from.</param>
    /// <param name="mipCount">[Optional] The number of mip map levels to view.</param>
    /// <param name="arrayIndex">[Optional] The array index to start viewing from.</param>
    /// <param name="arrayCount">[Optional] The number of array indices to view.</param>
    /// <returns>A <see cref="GorgonTexture1DView"/> used to bind the texture to a shader.</returns>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="format"/> is a typeless format.</exception>
    /// <exception cref="GorgonException">
    /// Thrown when this texture does not have a <see cref="TextureBinding"/> of <see cref="TextureBinding.ShaderResource"/>.
    /// <para>-or-</para>
    /// <para>Thrown when this texture has a usage of <see cref="ResourceUsage.Staging"/>.</para>
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
    public GorgonTexture1DView GetShaderResourceView(BufferFormat format = BufferFormat.Unknown, int firstMipLevel = 0, int mipCount = 0, int arrayIndex = 0, int arrayCount = 0)
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
            mipCount = _info.MipLevels - firstMipLevel;
        }

        mipCount = mipCount.Min(_info.MipLevels - firstMipLevel).Max(1);

        if (arrayCount <= 0)
        {
            arrayCount = _info.ArrayCount - arrayIndex;
        }

        arrayCount = (arrayCount.Min(ArrayCount - arrayIndex)).Max(1);

        var key = new TextureViewKey(format, firstMipLevel, mipCount, arrayIndex, arrayCount);

        if ((_cachedSrvs.TryGetValue(key, out GorgonTexture1DView view))
            && (view.Native is not null))
        {
            return view;
        }

        if (view is not null)
        {
            _cachedSrvs.Remove(key);
        }

        view = new GorgonTexture1DView(this, format, formatInfo, firstMipLevel, mipCount, arrayIndex, arrayCount);
        view.CreateNativeView();
        _cachedSrvs[key] = view;

        return view;
    }

    /// <summary>
    /// Function to create a new <see cref="GorgonTexture1DReadWriteView"/> for this texture.
    /// </summary>
    /// <param name="format">[Optional] The format for the view.</param>
    /// <param name="firstMipLevel">[Optional] The first mip map level (slice) to start viewing from.</param>
    /// <param name="arrayIndex">[Optional] The array index to start viewing from.</param>
    /// <param name="arrayCount">[Optional] The number of array indices to view.</param>
    /// <returns>A <see cref="GorgonTexture1DReadWriteView"/> used to bind the texture to a shader.</returns>
    /// <exception cref="GorgonException">
    /// <para>Thrown when this texture does not have a <see cref="TextureBinding"/> of <see cref="TextureBinding.ReadWriteView"/>.</para>
    /// <para>-or-</para>
    /// <para>Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Staging"/>.</para>
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
    public GorgonTexture1DReadWriteView GetReadWriteView(BufferFormat format = BufferFormat.Unknown, int firstMipLevel = 0, int arrayIndex = 0, int arrayCount = 0)
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
        arrayIndex = arrayIndex.Max(0).Min(ArrayCount - 1);

        if (arrayCount <= 0)
        {
            arrayCount = _info.ArrayCount - arrayIndex;
        }

        arrayCount = arrayCount.Min(ArrayCount - arrayIndex).Max(1);

        var key = new TextureViewKey(format, firstMipLevel, _info.MipLevels, arrayIndex, arrayCount);

        if ((_cachedReadWriteViews.TryGetValue(key, out GorgonTexture1DReadWriteView view))
            && (view.Native is not null))
        {
            return view;
        }

        if (view is not null)
        {
            _cachedReadWriteViews.Remove(key);
        }

        view = new GorgonTexture1DReadWriteView(this, format, info, firstMipLevel, arrayIndex, arrayCount);

        view.CreateNativeView();
        _cachedReadWriteViews[key] = view;

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
    /// <returns>A new <see cref="GorgonTexture1D"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="stream"/>, or the <paramref name="codec"/> parameter is <b>null</b>.</exception>
    /// <exception cref="IOException">Thrown if the <paramref name="stream"/> is write only.</exception>
    /// <exception cref="EndOfStreamException">Thrown if reading the image would move beyond the end of the <paramref name="stream"/>.</exception>
    /// <remarks>
    /// <para>
    /// This will load an <see cref="IGorgonImage"/> from a <paramref name="stream"/> and put it into a <see cref="GorgonTexture1D"/> object.
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
    ///			<description>Not used by 1D textures and is ignored.</description>
    ///		</item>
    ///		<item>
    ///		    <term>ConvertToPremultipliedAlpha</term>
    ///		    <description>Converts the image to premultiplied alpha before uploading the image data to the texture.</description>
    ///		</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static GorgonTexture1D FromStream(GorgonGraphics graphics, Stream stream, IGorgonImageCodec codec, long? size = null, GorgonTextureLoadOptions options = null)
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

        size ??= stream.Length - stream.Position;

        if ((stream.Length - stream.Position) < size)
        {
            throw new EndOfStreamException();
        }

        options ??= _defaultLoadOptions;

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

        return new GorgonTexture1D(graphics, image, options);
    }

    /// <summary>
    /// Function to load a texture from a file..
    /// </summary>
    /// <param name="graphics">The graphics interface that will own the texture.</param>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="codec">The codec that is used to decode the the data in the stream.</param>
    /// <param name="options">[Optional] Options used to further define the texture.</param>
    /// <returns>A new <see cref="GorgonTexture1D"/></returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="filePath"/>, or the <paramref name="codec"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// This will load an <see cref="IGorgonImage"/> from a file on disk and put it into a <see cref="GorgonTexture1D"/> object.
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
    ///			<description>Not used by 1D textures and is ignored.</description>
    ///		</item>
    ///		<item>
    ///		    <term>ConvertToPremultipliedAlpha</term>
    ///		    <description>Converts the image to premultiplied alpha before uploading the image data to the texture.</description>
    ///		</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static GorgonTexture1D FromFile(GorgonGraphics graphics, string filePath, IGorgonImageCodec codec, GorgonTextureLoadOptions options = null)
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

        options ??= _defaultLoadOptions;

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

        return new GorgonTexture1D(graphics, image, options);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
        // Destroy all cached views.
        Dictionary<TextureViewKey, GorgonTexture1DView> cachedSrvs = Interlocked.Exchange(ref _cachedSrvs, null);
        Dictionary<TextureViewKey, GorgonTexture1DReadWriteView> cachedReadWriteViews = Interlocked.Exchange(ref _cachedReadWriteViews, null);

        if (cachedSrvs is not null)
        {
            foreach (KeyValuePair<TextureViewKey, GorgonTexture1DView> view in cachedSrvs)
            {
                view.Value.Dispose();
            }
        }

        if (cachedReadWriteViews is not null)
        {
            foreach (KeyValuePair<TextureViewKey, GorgonTexture1DReadWriteView> view in cachedReadWriteViews)
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



    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTexture1D"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface used to create this texture.</param>
    /// <param name="image">The image to copy into the texture.</param>
    /// <param name="options">The options to use when loading the texture.</param>
    /// <remarks>
    /// <para>
    /// This constructor is used when converting an image to a texture.
    /// </para>
    /// </remarks>
    internal GorgonTexture1D(GorgonGraphics graphics, IGorgonImage image, GorgonTextureLoadOptions options)
        : base(graphics)
    {
        _info = new GorgonTexture1DInfo(image.Width, image.Format)
        {
            Name = options.Name,
            Usage = options.Usage,
            ArrayCount = image.ArrayCount,
            Binding = options.Binding,
            MipLevels = image.MipCount
        };

        Initialize(image);
        TextureID = Interlocked.Increment(ref _textureID);
        SizeInBytes = CalculateSizeInBytes(_info);

        this.RegisterDisposable(graphics);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTexture1D"/> class.
    /// </summary>
    /// <param name="graphics">The <see cref="GorgonGraphics"/> interface that created this texture.</param>
    /// <param name="textureInfo">A <see cref="IGorgonTexture1DInfo"/> object describing the properties of this texture.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="textureInfo"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentException">Thrown when the <see cref="GorgonGraphicsResource.Usage"/> is set to <c>Immutable</c>.</exception>
    /// <exception cref="GorgonException">Thrown when the texture could not be created due to misconfiguration.</exception>
    /// <remarks>
    /// <para>
    /// This constructor creates an empty texture. Data may be uploaded to the texture at a later time if its <see cref="GorgonGraphicsResource.Usage"/> is not set to 
    /// <see cref="ResourceUsage.Immutable"/>. If the <see cref="GorgonGraphicsResource.Usage"/> is set to <see cref="ResourceUsage.Immutable"/> with this constructor, then an exception will be thrown. 
    /// To use an immutable texture, use the <see cref="GorgonImageTextureExtensions.ToTexture1D(IGorgonImage, GorgonGraphics, GorgonTextureLoadOptions)"/> extension method on the <see cref="IGorgonImage"/> type.
    /// </para>
    /// </remarks>
    public GorgonTexture1D(GorgonGraphics graphics, GorgonTexture1DInfo textureInfo)
        : base(graphics)
    {
        _info = new GorgonTexture1DInfo(textureInfo ?? throw new ArgumentNullException(nameof(textureInfo)));

        Initialize(null);

        TextureID = Interlocked.Increment(ref _textureID);

        SizeInBytes = CalculateSizeInBytes(_info);

        this.RegisterDisposable(graphics);
    }

}
