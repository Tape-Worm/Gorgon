// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// Created: January 11, 2026 12:06:54 PM
//

using System.Diagnostics.CodeAnalysis;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Settings used for creating a <see cref="GorgonTexture"/>.
/// </summary>
/// <param name="Type">The type of texture to create.</param>
/// <param name="Format">The format of the texel data in the texture.</param>
/// <seealso cref="TextureType"/>
/// <seealso cref="BufferFormat"/>
public record class GorgonTextureInfo(TextureType Type, BufferFormat Format)
{
    /// <summary>
    /// An empty <see cref="GorgonTextureInfo"/> object.
    /// </summary>
    public readonly static GorgonTextureInfo Empty = new(TextureType.Unknown, BufferFormat.Unknown)
    {
        Width = 0
    };

    /// <inheritdoc cref="IGorgonTextureInfo.Width" path="/summary"/>
    /// <remarks>
    /// This value must be between 1 and <see cref="GorgonVideoAdapterInfo.MaxTextureWidth"/> or <see cref="GorgonVideoAdapterInfo.MaxTexture3DWidth"/>.
    /// </remarks>
    public required int Width
    {
        get;
        init;
    }

    /// <inheritdoc cref="IGorgonTextureInfo.Height" path="/summary"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonTextureInfo.Height" path="/remarks/para"/>
    /// <para>
    /// This value must be between 1 and <see cref="GorgonVideoAdapterInfo.MaxTextureHeight"/> or <see cref="GorgonVideoAdapterInfo.MaxTexture3DHeight"/>.
    /// </para>
    /// <para>
    /// The default value is 1.
    /// </para>
    /// </remarks>
    public int Height
    {
        get;
        init;
    } = 1;

    /// <inheritdoc cref="IGorgonTextureInfo.Depth" path="/summary"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonTextureInfo.Depth" path="/remarks/para"/>
    /// <para>
    /// This value must be between 1 and <see cref="GorgonVideoAdapterInfo.MaxTexture3DDepth"/>.
    /// </para>
    /// <para>
    /// The default value is 1.
    /// </para>
    /// </remarks>
    public short Depth
    {
        get;
        init;
    } = 1;

    /// <inheritdoc cref="IGorgonTextureInfo.MipCount" path="/summary"/>
    /// <remarks>
    /// <para>
    /// This value must be at least 1.
    /// </para>
    /// <para>
    /// The default value is 1.
    /// </para>
    /// </remarks>
    public short MipCount
    {
        get;
        init;
    } = 1;

    /// <inheritdoc cref="IGorgonTextureInfo.ArrayCount" path="/summary"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonTextureInfo.ArrayCount" path="/remarks/para"/>
    /// <para>
    /// If the <see cref="IsCube"/> property is set to <b>true</b>, then this value <b>must</b> be a multiple of 6. If it is not, an exception will be thrown upon texture creation.
    /// </para>
    /// <para>
    /// This value must be at least 1.
    /// </para>
    /// <para>
    /// The default value is 1.
    /// </para>
    /// </remarks>
    public short ArrayCount
    {
        get;
        init;
    } = 1;

    /// <inheritdoc cref="IGorgonTextureInfo.IsCube" path="/summary"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonTextureInfo.IsCube" path="/remarks/para"/>
    /// <para>
    /// If this property is set to <b>true</b>, then the <see cref="ArrayCount"/> property <b>must</b> be a multiple of 6. If it is not, an exception will be thrown upon texture creation.
    /// </para>
    /// <para>
    /// If the <see cref="Type"/> is not set to <see cref="TextureType.Texture2D"/>, then an exception will be thrown upon texture creation.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    public bool IsCube
    {
        get;
        init;
    } = false;

    /// <inheritdoc cref="IGorgonTextureInfo.IsRenderTarget" path="/summary"/>
    /// <remarks>
    /// <para>
    /// If this value is set to <b>true</b>, and <see cref="IsDepthStencil"/> is <b>true</b>, then an exception will be thrown upon texture creation.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    public bool IsRenderTarget
    {
        get;
        init;
    } = false;

    /// <inheritdoc cref="IGorgonTextureInfo.IsDepthStencil" path="/summary"/>
    /// <remarks>
    /// <para>
    /// This flag must not be <b>true</b> when <see cref="IsRenderTarget"/>, or <see cref="IsUnorderedAccess"/> is <b>true</b>, otherwise an exception will be thrown on texture creation.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// <inheritdoc cref="IGorgonTextureInfo.IsDepthStencil" path="/remarks/para"/>
    /// </remarks>
    public bool IsDepthStencil
    {
        get;
        init;
    } = false;

    /// <inheritdoc cref="IGorgonTextureInfo.IsShaderResource" path="/summary"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonTextureInfo.IsShaderResource" path="/remarks/para"/>
    /// <para>
    /// The default value is <b>true</b>.
    /// </para>
    /// </remarks>
    public bool IsShaderResource
    {
        get;
        init;
    } = true;

    /// <inheritdoc cref="IGorgonTextureInfo.IsUnorderedAccess" path="/summary"/>
    /// <remarks>
    /// <para>
    /// This value must be <b>false</b> if <see cref="MultisampleInfo"/> is not set to <see cref="GorgonMultisampleInfo.NoMultisampling"/>, or <see cref="IsDepthStencil"/> is set to <b>true</b>, otherwise an 
    /// exception is thrown upon texture creation.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    public bool IsUnorderedAccess
    {
        get;
        init;
    } = false;

    /// <inheritdoc cref="IGorgonTextureInfo.MultisampleInfo" path="/summary"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonTextureInfo.MultisampleInfo" path="/remarks/para"/>
    /// <para>
    /// This value must be <see cref="GorgonMultisampleInfo.NoMultisampling"/> if the <see cref="MipCount"/> is greater than 1, or <see cref="IsUnorderedAccess"/> is set to <b>true</b>. If this is not the 
    /// case an exception will be thrown on texture creation.
    /// </para>
    /// <para>
    /// The default value is <see cref="GorgonMultisampleInfo.NoMultisampling"/>.
    /// </para>
    /// </remarks>
    public GorgonMultisampleInfo MultisampleInfo
    {
        get;
        init;
    } = GorgonMultisampleInfo.NoMultisampling;

    /// <summary>
    /// Function to create a <see cref="IGorgonTextureInfo"/> that will build a 2D texture object.
    /// </summary>
    /// <param name="format">The format of the texture data.</param>
    /// <param name="width">The width of the texture, in pixels.</param>
    /// <param name="height">The height of the texture, in pixels.</param>
    /// <param name="mipCount">[Optional] The number of mip levels contained within the texture.</param>
    /// <param name="arrayCount">[Optional] The number of array indices contained within the texture.</param>
    /// <param name="isShaderResource"><b>true</b> to use this texture as a shader resource, <b>false</b> to deny shader access.</param>
    /// <param name="isUnorderedResource"><b>true</b> to use this texture as an unordered access resource, <b>false</b> to deny unordered access.</param>
    /// <param name="multiSampleInfo">[Optional] Multisample information for the texture.</param>
    /// <returns>A new <see cref="IGorgonTextureInfo"/>.</returns>
    public static GorgonTextureInfo Create2DTextureInfo(BufferFormat format, int width, int height, short mipCount = 1, short arrayCount = 1, bool isShaderResource = true, bool isUnorderedResource = false, GorgonMultisampleInfo? multiSampleInfo = null) => new(TextureType.Texture2D, format)
    {
        Width = width,
        Height = height,
        Depth = 1,
        MipCount = mipCount,
        ArrayCount = arrayCount,
        IsShaderResource = isShaderResource,
        IsUnorderedAccess = isUnorderedResource,
        MultisampleInfo = multiSampleInfo ?? GorgonMultisampleInfo.NoMultisampling
    };

    /// <summary>
    /// Function to create a <see cref="IGorgonTextureInfo"/> that will build a 2D texture object for render target usage.
    /// </summary>
    /// <param name="format">The format of the texture data.</param>
    /// <param name="width">The width of the texture, in pixels.</param>
    /// <param name="height">The height of the texture, in pixels.</param>
    /// <param name="mipCount">[Optional] The number of mip levels contained within the texture.</param>
    /// <param name="arrayCount">[Optional] The number of array indices contained within the texture.</param>
    /// <param name="isShaderResource"><b>true</b> to use this texture as a shader resource, <b>false</b> to deny shader access.</param>
    /// <param name="isUnorderedResource"><b>true</b> to use this texture as an unordered access resource, <b>false</b> to deny unordered access.</param>
    /// <param name="multiSampleInfo">[Optional] Multisample information for the texture.</param>
    /// <returns>A new <see cref="IGorgonTextureInfo"/>.</returns>
    public static GorgonTextureInfo Create2DRenderTargetInfo(BufferFormat format, int width, int height, short mipCount = 1, short arrayCount = 1, bool isShaderResource = true, bool isUnorderedResource = false, GorgonMultisampleInfo? multiSampleInfo = null) => new(TextureType.Texture2D, format)
    {
        Width = width,
        Height = height,
        Depth = 1,
        MipCount = mipCount,
        ArrayCount = arrayCount,
        IsShaderResource = isShaderResource,
        IsUnorderedAccess = isUnorderedResource,
        IsRenderTarget = true,
        MultisampleInfo = multiSampleInfo ?? GorgonMultisampleInfo.NoMultisampling
    };

    /// <summary>
    /// Function to create a <see cref="IGorgonTextureInfo"/> that will build a 2D texture object for depth/stencil usage.
    /// </summary>
    /// <param name="format">The format of the texture data.</param>
    /// <param name="width">The width of the texture, in pixels.</param>
    /// <param name="height">The height of the texture, in pixels.</param>
    /// <param name="mipCount">[Optional] The number of mip levels contained within the texture.</param>
    /// <param name="arrayCount">[Optional] The number of array indices contained within the texture.</param>
    /// <param name="isShaderResource"><b>true</b> to use this texture as a shader resource, <b>false</b> to deny shader access.</param>
    /// <returns>A new <see cref="IGorgonTextureInfo"/>.</returns>
    public static GorgonTextureInfo Create2DDepthStencilInfo(BufferFormat format, int width, int height, short mipCount = 1, short arrayCount = 1, bool isShaderResource = true) => new(TextureType.Texture2D, format)
    {
        Width = width,
        Height = height,
        Depth = 1,
        MipCount = mipCount,
        ArrayCount = arrayCount,
        IsShaderResource = isShaderResource,
        IsDepthStencil = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTextureInfo"/> class.
    /// </summary>
    /// <param name="info">The <see cref="IGorgonImageInfo"/> to derive the texture settings from.</param>
    public static GorgonTextureInfo FromImageInfo(IGorgonImageInfo info) => new(info.ImageType.ToTextureType(), info.Format)
    {
        Width = info.Width,
        Height = info.Height,
        Depth = (short)info.Depth,
        MipCount = (short)info.MipCount,
        ArrayCount = (short)info.ArrayCount,
        IsCube = info.ImageType == ImageDataType.ImageCube,
        IsRenderTarget = false,
        IsDepthStencil = false,
        IsUnorderedAccess = false,
        IsShaderResource = true,
        MultisampleInfo = GorgonMultisampleInfo.NoMultisampling
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTextureInfo"/> class.
    /// </summary>
    /// <param name="info">The texture information to copy.</param>
    [SetsRequiredMembers()]
    internal GorgonTextureInfo(ref readonly GpuResourceInfo info)
        : this(info.ResourceType.ToTextureType(), info.Format)
    {
        switch (Type)
        {
            case TextureType.Texture1D:
                Width = info.Texture1D.Width;
                Height = 1;
                Depth = 1;
                ArrayCount = info.Texture1D.ArrayCount;
                MipCount = info.Texture1D.MipCount;
                MultisampleInfo = GorgonMultisampleInfo.NoMultisampling;
                break;
            case TextureType.Texture2D:
                Width = info.Texture2D.Width;
                Height = info.Texture2D.Height;
                Depth = 1;
                ArrayCount = info.Texture2D.ArrayCount;
                MipCount = info.Texture2D.MipCount;
                MultisampleInfo = info.Texture2D.MultisampleInfo;
                break;
            case TextureType.Texture3D:
                Width = info.Texture3D.Width;
                Height = info.Texture3D.Height;
                Depth = info.Texture3D.Depth;
                ArrayCount = 1;
                MipCount = info.Texture3D.MipCount;
                MultisampleInfo = GorgonMultisampleInfo.NoMultisampling;
                break;
        }

        IsRenderTarget = (info.Usage & GraphicsResourceUsage.RenderTarget) == GraphicsResourceUsage.RenderTarget;
        IsDepthStencil = (info.Usage & GraphicsResourceUsage.DepthStencil) == GraphicsResourceUsage.DepthStencil;
        IsUnorderedAccess = (info.Usage & GraphicsResourceUsage.UnorderedAccess) == GraphicsResourceUsage.UnorderedAccess;
        IsShaderResource = (info.Usage & GraphicsResourceUsage.ShaderResource) == GraphicsResourceUsage.ShaderResource;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTextureInfo"/> class.
    /// </summary>
    /// <param name="info">The texture information to copy.</param>
    [SetsRequiredMembers()]
    public GorgonTextureInfo(GorgonTextureInfo info)
    {
        Type = info.Type;
        Format = info.Format;
        Width = info.Width;
        Height = info.Height;
        Depth = info.Depth;
        MipCount = info.MipCount;
        ArrayCount = info.ArrayCount;
        IsCube = info.IsCube;
        IsRenderTarget = info.IsRenderTarget;
        IsDepthStencil = info.IsDepthStencil;
        IsUnorderedAccess = info.IsUnorderedAccess;
        IsShaderResource = info.IsShaderResource;
        MultisampleInfo = info.MultisampleInfo;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTextureInfo"/> class.
    /// </summary>
    /// <param name="info">The texture information to copy.</param>
    [SetsRequiredMembers()]
    public GorgonTextureInfo(IGorgonTextureInfo info)
        : this(info.Type, info.Format)
    {
        Width = info.Width;
        Height = info.Height;
        Depth = info.Depth;
        MipCount = info.MipCount;
        ArrayCount = info.ArrayCount;
        IsCube = info.IsCube;
        IsRenderTarget = info.IsRenderTarget;
        IsDepthStencil = info.IsDepthStencil;
        IsUnorderedAccess = info.IsUnorderedAccess;
        IsShaderResource = info.IsShaderResource;
        MultisampleInfo = info.MultisampleInfo;
    }
}
