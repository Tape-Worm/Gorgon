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
using System.Runtime.CompilerServices;
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
    /// This flag must not be <b>true</b> when <see cref="IsRenderTarget"/>, or <see cref="HasReadWriteAccess"/> is <b>true</b>, otherwise an exception will be thrown on texture creation.
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

    /// <inheritdoc cref="IGorgonTextureInfo.HasReadWriteAccess" path="/summary"/>
    /// <remarks>
    /// <para>
    /// This value must be <b>false</b> if <see cref="MultisampleInfo"/> is not set to <see cref="GorgonMultisampleInfo.NoMultisampling"/>, or <see cref="IsDepthStencil"/> is set to <b>true</b>, otherwise an 
    /// exception is thrown upon texture creation.
    /// </para>
    /// <inheritdoc cref="IGorgonTextureInfo.HasReadWriteAccess" path="/remarks/para"/>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    public bool HasReadWriteAccess
    {
        get;
        init;
    } = false;

    /// <inheritdoc cref="IGorgonTextureInfo.MultisampleInfo" path="/summary"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonTextureInfo.MultisampleInfo" path="/remarks/para"/>
    /// <para>
    /// This value must be <see cref="GorgonMultisampleInfo.NoMultisampling"/> if the <see cref="MipCount"/> is greater than 1, or <see cref="HasReadWriteAccess"/> is set to <b>true</b>. If this is not the 
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

    /// <inheritdoc cref="GorgonImageInfo.GetMaximumMipCount(int, int, int)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short GetMaximumMipCount(int width, int height, short depth) => (short)GorgonImageInfo.GetMaximumMipCount(width, height, depth);

    /// <summary>
    /// Function to create a <see cref="GorgonTextureInfo"/> that will build a 1D texture object.
    /// </summary>
    /// <param name="format">The format of the texture data.</param>
    /// <param name="width">The width of the texture, in pixels.</param>
    /// <param name="mipCount">[Optional] The number of mip levels contained within the texture.</param>
    /// <param name="arrayCount">[Optional] The number of array indices contained within the texture.</param>
    /// <param name="isShaderResource">[Optional] <b>true</b> to use this texture as a shader resource, <b>false</b> to deny shader access.</param>
    /// <param name="allowReadWriteAccess">[Optional] <b>true</b> to use this texture as a read/write access resource, <b>false</b> to deny read/write access.</param>
    /// <returns>A new <see cref="GorgonTextureInfo"/>.</returns>
    /// <remarks>
    /// <inheritdoc cref="GorgonGpuBuffer" path="/remarks/para[@type='uav_readwrite']"/>
    /// </remarks>
    public static GorgonTextureInfo Create1DTextureInfo(BufferFormat format, int width, short mipCount = 1, short arrayCount = 1, bool isShaderResource = true, bool allowReadWriteAccess = false) => new(TextureType.Texture1D, format)
    {
        Width = width,
        Height = 1,
        Depth = 1,
        MipCount = mipCount,
        ArrayCount = arrayCount,
        IsShaderResource = isShaderResource,
        HasReadWriteAccess = allowReadWriteAccess,
        MultisampleInfo = GorgonMultisampleInfo.NoMultisampling,
        IsCube = false
    };

    /// <summary>
    /// Function to create a <see cref="GorgonTextureInfo"/> that will build a 1D texture object for render target usage.
    /// </summary>
    /// <param name="format"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='format']"/></param>
    /// <param name="width"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='width']"/></param>
    /// <param name="mipCount"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='mipCount']"/></param>
    /// <param name="arrayCount"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='arrayCount']"/></param>
    /// <param name="isShaderResource"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='isShaderResource']"/></param>
    /// <param name="allowReadWriteAccess"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='allowReadWriteAccess']"/></param>
    /// <inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="GorgonGpuBuffer" path="/remarks/para[@type='uav_readwrite']"/>
    /// </remarks>
    public static GorgonTextureInfo Create1DRenderTargetInfo(BufferFormat format, int width, short mipCount = 1, short arrayCount = 1, bool isShaderResource = true, bool allowReadWriteAccess = false) => new(TextureType.Texture2D, format)
    {
        Width = width,
        Height = 1,
        Depth = 1,
        MipCount = mipCount,
        ArrayCount = arrayCount,
        IsShaderResource = isShaderResource,
        HasReadWriteAccess = allowReadWriteAccess,
        IsRenderTarget = true,
        MultisampleInfo = GorgonMultisampleInfo.NoMultisampling
    };

    /// <summary>
    /// Function to create a <see cref="GorgonTextureInfo"/> that will build a 2D texture object.
    /// </summary>
    /// <param name="format"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='format']"/></param>
    /// <param name="width"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='width']"/></param>
    /// <param name="height">The height of the texture, in pixels.</param>
    /// <param name="mipCount"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='mipCount']"/></param>
    /// <param name="arrayCount"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='arrayCount']"/></param>
    /// <param name="isShaderResource"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='isShaderResource']"/></param>
    /// <param name="allowReadWriteAccess"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='allowReadWriteAccess']"/></param>
    /// <inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="GorgonGpuBuffer" path="/remarks/para[@type='uav_readwrite']"/>
    /// </remarks>
    public static GorgonTextureInfo Create2DTextureInfo(BufferFormat format, int width, int height, short mipCount = 1, short arrayCount = 1, bool isShaderResource = true, bool allowReadWriteAccess = false) => new(TextureType.Texture2D, format)
    {
        Width = width,
        Height = height,
        Depth = 1,
        MipCount = mipCount,
        ArrayCount = arrayCount,
        IsShaderResource = isShaderResource,
        HasReadWriteAccess = allowReadWriteAccess,
        MultisampleInfo = GorgonMultisampleInfo.NoMultisampling,
        IsCube = false
    };

    /// <summary>
    /// Function to create a <see cref="GorgonTextureInfo"/> that will build a texture cube object.
    /// </summary>
    /// <param name="format"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='format']"/></param>
    /// <param name="width"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='width']"/></param>
    /// <param name="height"><inheritdoc cref="Create2DTextureInfo(BufferFormat, int, int, short, short, bool, bool)" path="/param[@name='height']"/></param>
    /// <param name="cubeCount">The number of cubes contained within the texture.</param>
    /// <param name="mipCount"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='mipCount']"/></param>
    /// <param name="isShaderResource"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='isShaderResource']"/></param>
    /// <param name="allowReadWriteAccess"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='allowReadWriteAccess']"/></param>
    /// <inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="GorgonGpuBuffer" path="/remarks/para[@type='uav_readwrite']"/>
    /// </remarks>
    public static GorgonTextureInfo CreateTextureCubeInfo(BufferFormat format, int width, int height, short cubeCount, short mipCount = 1, bool isShaderResource = true, bool allowReadWriteAccess = false) => new(TextureType.Texture2D, format)
    {
        Width = width,
        Height = height,
        Depth = 1,
        MipCount = mipCount,
        ArrayCount = (short)(cubeCount * 6),
        IsShaderResource = isShaderResource,
        HasReadWriteAccess = allowReadWriteAccess,
        MultisampleInfo = GorgonMultisampleInfo.NoMultisampling,
        IsCube = true
    };

    /// <summary>
    /// Function to create a <see cref="GorgonTextureInfo"/> that will build a 2D texture object for render target usage.
    /// </summary>
    /// <param name="format"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='format']"/></param>
    /// <param name="width"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='width']"/></param>
    /// <param name="height"><inheritdoc cref="Create2DTextureInfo(BufferFormat, int, int, short, short, bool, bool)" path="/param[@name='height']"/></param>
    /// <param name="mipCount"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='mipCount']"/></param>
    /// <param name="arrayCount"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='arrayCount']"/></param>
    /// <param name="isShaderResource"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='isShaderResource']"/></param>
    /// <param name="allowReadWriteAccess"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='allowReadWriteAccess']"/></param>
    /// <param name="multisampleInfo">[Optional] Multisample information for the texture.</param>
    /// <inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="GorgonGpuBuffer" path="/remarks/para[@type='uav_readwrite']"/>
    /// </remarks>
    public static GorgonTextureInfo Create2DRenderTargetInfo(BufferFormat format, int width, int height, short mipCount = 1, short arrayCount = 1, bool isShaderResource = true, bool allowReadWriteAccess = false, GorgonMultisampleInfo? multisampleInfo = null) => new(TextureType.Texture2D, format)
    {
        Width = width,
        Height = height,
        Depth = 1,
        MipCount = mipCount,
        ArrayCount = arrayCount,
        IsShaderResource = isShaderResource,
        HasReadWriteAccess = allowReadWriteAccess,
        IsRenderTarget = true,
        MultisampleInfo = multisampleInfo ?? GorgonMultisampleInfo.NoMultisampling
    };

    /// <summary>
    /// Function to create a <see cref="GorgonTextureInfo"/> that will build a cube map for render target usage.
    /// </summary>
    /// <param name="format"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='format']"/></param>
    /// <param name="width"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='width']"/></param>
    /// <param name="height"><inheritdoc cref="Create2DTextureInfo(BufferFormat, int, int, short, short, bool, bool)" path="/param[@name='height']"/></param>
    /// <param name="cubeCount"><inheritdoc cref="CreateTextureCubeInfo(BufferFormat, int, int, short, short, bool, bool)" path="/param[@name='cubeCount']"/></param>
    /// <param name="mipCount"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='mipCount']"/></param>
    /// <param name="isShaderResource"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='isShaderResource']"/></param>
    /// <param name="allowReadWriteAccess"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='allowReadWriteAccess']"/></param>
    /// <param name="multisampleInfo"><inheritdoc cref="Create2DRenderTargetInfo(BufferFormat, int, int, short, short, bool, bool, GorgonMultisampleInfo?)" path="/param[@name='multisampleInfo']"/></param>
    /// <inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="GorgonGpuBuffer" path="/remarks/para[@type='uav_readwrite']"/>
    /// </remarks>
    public static GorgonTextureInfo CreateRenderTargetCubeInfo(BufferFormat format, int width, int height, short cubeCount, short mipCount = 1, bool isShaderResource = true, bool allowReadWriteAccess = false, GorgonMultisampleInfo? multisampleInfo = null) => new(TextureType.Texture2D, format)
    {
        Width = width,
        Height = height,
        Depth = 1,
        MipCount = mipCount,
        ArrayCount = (short)(cubeCount * 6),
        IsShaderResource = isShaderResource,
        HasReadWriteAccess = allowReadWriteAccess,
        IsRenderTarget = true,
        IsCube =true,
        MultisampleInfo = multisampleInfo ?? GorgonMultisampleInfo.NoMultisampling
    };

    /// <summary>
    /// Function to create a <see cref="GorgonTextureInfo"/> that will build a 2D texture object for depth/stencil usage.
    /// </summary>
    /// <param name="format"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='format']"/></param>
    /// <param name="width"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='width']"/></param>
    /// <param name="height"><inheritdoc cref="Create2DTextureInfo(BufferFormat, int, int, short, short, bool, bool)" path="/param[@name='height']"/></param>
    /// <param name="mipCount"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='mipCount']"/></param>
    /// <param name="arrayCount"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='arrayCount']"/></param>
    /// <param name="isShaderResource"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='isShaderResource']"/></param>
    /// <param name="multisampleInfo"><inheritdoc cref="Create2DRenderTargetInfo(BufferFormat, int, int, short, short, bool, bool, GorgonMultisampleInfo?)" path="/param[@name='multisampleInfo']"/></param>
    /// <inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/returns"/>
    public static GorgonTextureInfo Create2DDepthStencilInfo(BufferFormat format, int width, int height, short mipCount = 1, short arrayCount = 1, bool isShaderResource = false, GorgonMultisampleInfo? multisampleInfo = null) => new(TextureType.Texture2D, format)
    {
        Width = width,
        Height = height,
        Depth = 1,
        MipCount = mipCount,
        ArrayCount = arrayCount,
        IsShaderResource = isShaderResource,
        IsDepthStencil = true,
        MultisampleInfo = multisampleInfo ?? GorgonMultisampleInfo.NoMultisampling
    };

    /// <summary>
    /// Function to create a <see cref="GorgonTextureInfo"/> that will build a cube map for depth/stencil usage.
    /// </summary>
    /// <param name="format"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='format']"/></param>
    /// <param name="width"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='width']"/></param>
    /// <param name="height"><inheritdoc cref="Create2DTextureInfo(BufferFormat, int, int, short, short, bool, bool)" path="/param[@name='height']"/></param>
    /// <param name="cubeCount"><inheritdoc cref="CreateTextureCubeInfo(BufferFormat, int, int, short, short, bool, bool)" path="/param[@name='cubeCount']"/></param>
    /// <param name="mipCount"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='mipCount']"/></param>
    /// <param name="isShaderResource"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='isShaderResource']"/></param>
    /// <param name="multisampleInfo"><inheritdoc cref="Create2DRenderTargetInfo(BufferFormat, int, int, short, short, bool, bool, GorgonMultisampleInfo?)" path="/param[@name='multisampleInfo']"/></param>
    /// <inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/returns"/>
    public static GorgonTextureInfo CreateDepthStencilCubeInfo(BufferFormat format, int width, int height, short cubeCount, short mipCount = 1, bool isShaderResource = false, GorgonMultisampleInfo? multisampleInfo = null) => new(TextureType.Texture2D, format)
    {
        Width = width,
        Height = height,
        Depth = 1,
        MipCount = mipCount,
        ArrayCount = (short)(cubeCount * 6),
        IsShaderResource = isShaderResource,
        IsDepthStencil = true,
        IsCube = true,
        MultisampleInfo = multisampleInfo ?? GorgonMultisampleInfo.NoMultisampling
    };

    /// <summary>
    /// Function to create a <see cref="GorgonTextureInfo"/> that will build a 3D texture object.
    /// </summary>
    /// <param name="format"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='format']"/></param>
    /// <param name="width"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='width']"/></param>
    /// <param name="height"><inheritdoc cref="Create2DTextureInfo(BufferFormat, int, int, short, short, bool, bool)" path="/param[@name='height']"/></param>
    /// <param name="depth">The depth of the texture, in depth slices.</param>
    /// <param name="mipCount"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='mipCount']"/></param>
    /// <param name="isShaderResource"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='isShaderResource']"/></param>
    /// <param name="allowReadWriteAccess"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='allowReadWriteAccess']"/></param>
    /// <inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="GorgonGpuBuffer" path="/remarks/para[@type='uav_readwrite']"/>
    /// </remarks>
    public static GorgonTextureInfo Create3DTextureInfo(BufferFormat format, int width, int height, short depth, short mipCount = 1, bool isShaderResource = true, bool allowReadWriteAccess = false) => new(TextureType.Texture1D, format)
    {
        Width = width,
        Height = height,
        Depth = depth,
        MipCount = mipCount,
        ArrayCount = 1,
        IsShaderResource = isShaderResource,
        HasReadWriteAccess = allowReadWriteAccess,
        MultisampleInfo = GorgonMultisampleInfo.NoMultisampling,
        IsCube = false
    };

    /// <summary>
    /// Function to create a <see cref="GorgonTextureInfo"/> that will build a 3D texture object for render target usage.
    /// </summary>
    /// <param name="format"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='format']"/></param>
    /// <param name="width"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='width']"/></param>
    /// <param name="height"><inheritdoc cref="Create2DTextureInfo(BufferFormat, int, int, short, short, bool, bool)" path="/param[@name='height']"/></param>
    /// <param name="depth"><inheritdoc cref="Create3DTextureInfo(BufferFormat, int, int, short, short, bool, bool)" path="/param[@name='depth']"/></param>
    /// <param name="mipCount"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='mipCount']"/></param>
    /// <param name="isShaderResource"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='isShaderResource']"/></param>
    /// <param name="allowReadWriteAccess"><inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/param[@name='allowReadWriteAccess']"/></param>
    /// <inheritdoc cref="Create1DTextureInfo(BufferFormat, int, short, short, bool, bool)" path="/returns"/>
    /// <remarks>
    /// <inheritdoc cref="GorgonGpuBuffer" path="/remarks/para[@type='uav_readwrite']"/>
    /// </remarks>
    public static GorgonTextureInfo Create3DRenderTargetInfo(BufferFormat format, int width, int height, short depth, short mipCount = 1, bool isShaderResource = true, bool allowReadWriteAccess = false) => new(TextureType.Texture2D, format)
    {
        Width = width,
        Height = height,
        Depth = depth,
        MipCount = mipCount,
        ArrayCount = 1,
        IsShaderResource = isShaderResource,
        HasReadWriteAccess = allowReadWriteAccess,
        IsRenderTarget = true,
        MultisampleInfo = GorgonMultisampleInfo.NoMultisampling
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
        HasReadWriteAccess = false,
        IsShaderResource = true,
        MultisampleInfo = GorgonMultisampleInfo.NoMultisampling
    };

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
        HasReadWriteAccess = info.HasReadWriteAccess;
        IsShaderResource = info.IsShaderResource;
        MultisampleInfo = info.MultisampleInfo;
    }

    /// <inheritdoc cref="GorgonTextureInfo(GorgonTextureInfo)"/>
    [SetsRequiredMembers()]
    public GorgonTextureInfo(GorgonVirtualTextureInfo info)
        : this(info.Type, info.Format)
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
        IsDepthStencil = false;
        HasReadWriteAccess = info.HasReadWriteAccess;
        IsShaderResource = info.IsShaderResource;
        MultisampleInfo = GorgonMultisampleInfo.NoMultisampling;
    }

    /// <inheritdoc cref="GorgonTextureInfo(GorgonTextureInfo)"/>
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
        HasReadWriteAccess = info.HasReadWriteAccess;
        IsShaderResource = info.IsShaderResource;
        MultisampleInfo = info.MultisampleInfo;
    }
}
