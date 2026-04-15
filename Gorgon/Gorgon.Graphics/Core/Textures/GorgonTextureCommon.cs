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
// Created: April 13, 2026 4:57:30 PM
//

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.Native;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using DX = TerraFX.Interop.DirectX.DirectX;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Common functionality for <see cref="GorgonTexture"/> and <see cref="GorgonVirtualTexture"/>.
/// </summary>
public unsafe abstract class GorgonTextureCommon
    : GorgonGpuResource, IGorgonTextureInfo, IGorgonImageInfo
{
    private readonly GorgonTextureInfo _info;
    private D3D12_RESOURCE_DESC1 _d3dDesc;

    /// <summary>
    /// Property to return the groups of formats that are compatible with specific bit widths for a format.
    /// </summary>
    protected static Dictionary<int, List<BufferFormat>> FormatGroups
    {
        get;
    } = [];

    /// <inheritdoc/>
    internal override bool IsMegaBufferResource => false;

    /// <inheritdoc/>
    public short ArrayCount => _info.ArrayCount;

    /// <inheritdoc/>
    int IGorgonImageInfo.ArrayCount => ArrayCount;

    /// <inheritdoc/>
    public short Depth => _info.Depth;

    /// <inheritdoc/>
    int IGorgonImageInfo.Depth => Depth;

    /// <inheritdoc/>
    public BufferFormat Format => _info.Format;

    /// <inheritdoc/>
    public int Height => _info.Height;

    /// <inheritdoc/>
    public bool IsCube => _info.IsCube;

    /// <inheritdoc/>
    public bool IsDepthStencil => _info.IsDepthStencil;

    /// <inheritdoc/>
    public bool IsRenderTarget => _info.IsRenderTarget;

    /// <inheritdoc/>
    public bool IsShaderResource => _info.IsShaderResource;

    /// <inheritdoc/>
    public bool HasReadWriteAccess => _info.HasReadWriteAccess;

    /// <inheritdoc/>
    public short MipCount => _info.MipCount;

    /// <inheritdoc/>
    int IGorgonImageInfo.MipCount => MipCount;

    /// <inheritdoc/>
    public GorgonMultisampleInfo MultisampleInfo => _info.MultisampleInfo;

    /// <inheritdoc/>
    public TextureType Type => _info.Type;

    /// <inheritdoc/>
    public int Width => _info.Width;

    /// <summary>
    /// Property to return the list of formats that can be casted to in a <see cref="GorgonTextureView"/>.
    /// </summary>
    public IReadOnlyList<BufferFormat> CompatibleFormats
    {
        get;
    }

    /// <summary>
    /// Property to return the size of this texture, in bytes.
    /// </summary>
    public long SizeInBytes
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the information about the buffer <see cref="Format"/>.
    /// </summary>
    public GorgonFormatInfo FormatInfo
    {
        get;
    }

    /// <inheritdoc/>
    ImageDataType IGorgonImageInfo.ImageType => Type.ToImageDataType(IsCube);

    /// <inheritdoc/>
    bool IGorgonImageInfo.HasPremultipliedAlpha => false;

    /// <inheritdoc/>
    bool IGorgonImageInfo.IsPowerOfTwo => ((Width == 0) || (Width & (Width - 1)) == 0)
                                          && ((Height == 0) || (Height & (Height - 1)) == 0);

    /// <inheritdoc/>
    public GorgonRectangle Bounds => new(0, 0, Width, Height);

    /// <summary>
    /// Property to return information about each sub resource in the texture.
    /// </summary>
    /// <remarks>
    /// The first sub resource will always have the same information as the texture.
    /// </remarks>
    public GorgonSubResourceInfoList SubResources
    {
        get;
        private set;
    }

    /// <summary>
    /// Function to validate the information passed to the constructor.
    /// </summary>
    /// <param name="info">The texture creation information to validate.</param>
    /// <returns>The updated creation information if default values need changing, otherwise the <paramref name="info"/> parameter.</returns>
    /// <exception cref="GorgonException"><para>Thrown when the <paramref name="info"/> <see cref="GorgonTextureInfo.MipCount"/>, or <see cref="GorgonTextureInfo.Width"/> is less than 1.</para>
    /// <para>Thrown if the texture type is <see cref="TextureType.Texture3D"/> and the <paramref name="info"/> <see cref="GorgonTextureInfo.Depth"/> or <see cref="GorgonTextureInfo.Height"/> is less than 1, or the <see cref="GorgonTextureInfo.Format"/> doesn't support 3D textures, or the <see cref="GorgonTextureInfo.IsCube"/> is set to <b>true</b>.</para>
    /// <para>Thrown if the texture type is <see cref="TextureType.Texture2D"/> and the <paramref name="info"/> <see cref="GorgonTextureInfo.Height"/> or <see cref="GorgonTextureInfo.ArrayCount"/> is less than 1, or the <see cref="GorgonTextureInfo.Format"/> doesn't support 2D textures, or the <see cref="GorgonTextureInfo.IsCube"/> is set to <b>true</b> and the <see cref="GorgonTextureInfo.ArrayCount"/> is not a multiple of 6.</para>
    /// <para>Thrown if the texture type is <see cref="TextureType.Texture1D"/> and the <paramref name="info"/> <see cref="GorgonTextureInfo.ArrayCount"/> is less than 1, or the <see cref="GorgonTextureInfo.Format"/> doesn't support 1D textures, or the <see cref="GorgonTextureInfo.IsCube"/> is set to <b>true</b>.</para>
    /// <para>Thrown if the texture is a depth/stencil and the <paramref name="info"/> <see cref="GorgonTextureInfo.Format"/> does not support depth/stencil, or <see cref="GorgonTextureInfo.HasReadWriteAccess"/> is set to <b>true</b>, 
    /// or the <paramref name="info"/> <see cref="GorgonTextureInfo.Type"/> is not set to <see cref="TextureType.Texture2D"/>.</para>
    /// <para>Thrown if the texture is a render target and the <paramref name="info"/> <see cref="GorgonTextureInfo.Format"/> does not support render targets or MSAA render targets.</para>
    /// <para>Thrown if the texture is an read/write resource and the <paramref name="info"/> <see cref="GorgonTextureInfo.MultisampleInfo"/> is not set to <see cref="GorgonMultisampleInfo.NoMultisampling"/>.</para>
    /// <para>Thrown if the texture has a mip count greater than 1 and the <paramref name="info"/> <see cref="GorgonTextureInfo.MultisampleInfo"/> is not set to <see cref="GorgonMultisampleInfo.NoMultisampling"/>, or the <see cref="GorgonTextureInfo.Format"/> does not support mip maps.</para>
    /// <para>Thrown if the texture is a cube map and the <paramref name="info"/> <see cref="GorgonTextureInfo.Format"/> does not support cube maps.</para>
    /// <para>Thrown if the texture uses a compressed format, and does not have dimensions that are a multiple of 4.</para>
    /// </exception>
    private GorgonTextureInfo ValidateInfo(GorgonTextureInfo info)
    {
        GorgonTextureInfo result = info;
        bool isMultisampled = !info.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling);

        if (info.MipCount < 1)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_LESS_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.MipCount), 1));
        }

        if (info.Width < 1)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_LESS_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.Width), 1));
        }

        if (info.IsDepthStencil)
        {
            if (info.Type != TextureType.Texture2D)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_DEPTH_STENCIL_MUST_BE_2D);
            }

            if (info.Format is not BufferFormat.D16_UNorm and not BufferFormat.D24_UNorm_S8_UInt
                            and not BufferFormat.D32_Float and not BufferFormat.D32_Float_S8X24_UInt
                            and not BufferFormat.R16_Typeless and not BufferFormat.R32_Typeless
                            and not BufferFormat.R24G8_Typeless and not BufferFormat.R32G8X24_Typeless)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_DEPTH_STENCIL_FORMAT, info.Format));
            }
        }

        switch (info.Type)
        {
            case TextureType.Texture3D:
                if (!Graphics.FormatSupport[info.Format].Is3DTextureFormat)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_FORMAT_INVALID, info.Format, info.Type));
                }

                if (info.Depth < 1)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_LESS_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.Depth), 1));
                }

                if (info.Height < 1)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_LESS_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.Height), 1));
                }

                if (info.IsCube)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_CUBEMAP_ONLY_2DTEX);
                }

                if ((FormatInfo.IsCompressed)
                    && (((info.Width % 4) != 0) || ((info.Height % 4) != 0)))
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_COMPRESSED_NOT_MULTIPLE_OF_FOUR, Name, Format, Width, Height));
                }

                if ((info.ArrayCount != 1) || (isMultisampled))
                {
                    result = info with
                    {
                        ArrayCount = 1,
                        MultisampleInfo = GorgonMultisampleInfo.NoMultisampling
                    };
                }
                break;
            case TextureType.Texture2D:
                if (!Graphics.FormatSupport[info.Format].Is2DTextureFormat)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_FORMAT_INVALID, info.Format, info.Type));
                }

                if (info.ArrayCount < 1)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_LESS_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.ArrayCount), 1));
                }

                if ((info.IsCube) && ((info.ArrayCount % 6) != 0))
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CUBEMAP_NOT_MULTIPLE_OF_SIX, info.ArrayCount));
                }

                if (info.Height < 1)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_LESS_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.Height), 1));
                }

                if ((FormatInfo.IsCompressed)
                    && (((info.Width % 4) != 0) || ((info.Height % 4) != 0)))
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_COMPRESSED_NOT_MULTIPLE_OF_FOUR, Name, Format, Width, Height));
                }

                if (info.Depth != 1)
                {
                    result = info with
                    {
                        Depth = 1
                    };
                }
                break;
            case TextureType.Texture1D:
                if ((!Graphics.FormatSupport[info.Format].Is1DTextureFormat) || (FormatInfo.IsCompressed))
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_FORMAT_INVALID, info.Format, info.Type));
                }

                if (info.ArrayCount < 1)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_LESS_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.ArrayCount), 1));
                }

                if (info.IsCube)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_CUBEMAP_ONLY_2DTEX);
                }

                if ((info.Depth != 1) || (info.Height != 1) || (isMultisampled))
                {
                    result = info with
                    {
                        Depth = 1,
                        Height = 1,
                        MultisampleInfo = GorgonMultisampleInfo.NoMultisampling
                    };
                }
                break;
            default:
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_TEXTURE_UNKNOWN_TYPE, info.Type));
        }

        if (result.IsDepthStencil)
        {
            if (result.IsRenderTarget)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_CANNOT_BE_RT_AND_DS, Name));
            }

            if (result.HasReadWriteAccess)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_CANNOT_BE_UNORDERED_AND_DS, Name));
            }
        }

        if (result.IsRenderTarget)
        {
            if (!Graphics.FormatSupport[result.Format].IsRenderTargetFormat)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_RENDER_TARGET_FORMAT, result.Format));
            }

            if ((isMultisampled) && (!Graphics.FormatSupport[result.Format].IsMultisampleRenderTargetFormat))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_MSAA_FORMAT_INVALID, result.Format));
            }
        }

        if ((isMultisampled) && (result.HasReadWriteAccess))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_CANNOT_BE_MSAA_AND_UNORDERED, Name));
        }

        if (result.MipCount > 1)
        {
            if (isMultisampled)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_MSAA_MIP_COUNT_INVALID);
            }

            if (!Graphics.FormatSupport[result.Format].SupportsMipMaps)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_MIP_FORMAT_INVALID, result.Format));
            }
        }

        if ((result.IsCube) && (!Graphics.FormatSupport[result.Format].IsCubeTextureFormat))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CUBE_TEXTURE_FORMAT_INVALID, result.Format));
        }

        if ((result.IsDepthStencil) && (!Graphics.FormatSupport[result.Format].IsDepthStencilFormat))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_DEPTH_STENCIL_FORMAT, result.Format));
        }

        return result;
    }

    /// <inheritdoc/>
    private protected sealed override void OnGetResourceInfo(out GpuResourceInfo resourceInfo) => resourceInfo = GpuResourceInfo.FromD3D(in _d3dDesc);

    /// <summary>
    /// Function called when the resource is initialized.
    /// </summary>
    /// <returns>The resource for the texture.</returns>
    private protected abstract ComPtr<ID3D12Resource2> OnCreateNative();

    /// <summary>
    /// Function to populate the sub resource information for the texture.
    /// </summary>
    /// <param name="desc">The resource description.</param>
    /// <returns>The list of sub resources for the texture.</returns>
    [MemberNotNull(nameof(SubResources))]
    protected void PopulateSubResourceInfo(ref readonly D3D12_RESOURCE_DESC1 desc)
    {
        uint planeCount = Graphics.FormatSupport[Format].PlaneCount;
        uint resourceCount = planeCount * (uint)(_info.Type != TextureType.Texture3D ? desc.DepthOrArraySize : 1) * desc.MipLevels;
        uint[] rows = ArrayPool<uint>.Shared.Rent((int)resourceCount);
        ulong[] rowSizes = ArrayPool<ulong>.Shared.Rent((int)resourceCount);
        D3D12_PLACED_SUBRESOURCE_FOOTPRINT[] footPrints = ArrayPool<D3D12_PLACED_SUBRESOURCE_FOOTPRINT>.Shared.Rent((int)resourceCount);
        List<GorgonSubResourceInfo> result = [];

        try
        {
            fixed (D3D12_PLACED_SUBRESOURCE_FOOTPRINT* footPrintPtr = footPrints)
            fixed (ulong* rowSizesPtr = rowSizes)
            fixed (uint* rowsPtr = rows)
            fixed (D3D12_RESOURCE_DESC1* descPtr = &desc)
            {
                ulong sizeInBytes = 0;

                Graphics.D3DDevice.Get()->GetCopyableFootprints1(descPtr, 0, resourceCount, 0, footPrintPtr, rowsPtr, rowSizesPtr, &sizeInBytes);

                for (int a = 0; a < ArrayCount; ++a)
                {
                    for (int m = 0; m < MipCount; ++m)
                    {
                        for (int p = 0; p < planeCount; ++p)
                        {
                            int r = GetSubResourceIndex(m, a, p);
                            D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = footPrints[r];
                            GorgonSubResourceInfo info = new(r, (int)footPrint.Footprint.Width, (int)footPrint.Footprint.Height, (int)footPrint.Footprint.Depth,
                                a, m, p,
                                (int)footPrint.Footprint.RowPitch, (long)rowSizes[r], (int)rows[r], (long)footPrint.Offset);

                            result.Add(info);
                        }
                    }
                }

                SizeInBytes = (long)sizeInBytes;
            }

            SubResources = new GorgonSubResourceInfoList(this, result);
        }
        finally
        {
            ArrayPool<uint>.Shared.Return(rows, true);
            ArrayPool<ulong>.Shared.Return(rowSizes, true);
            ArrayPool<D3D12_PLACED_SUBRESOURCE_FOOTPRINT>.Shared.Return(footPrints, true);
        }
    }

    /// <summary>
    /// Function to call to create the backing resource.
    /// </summary>
    protected void CreateNative()
    {
        ComPtr<ID3D12Resource2> result = OnCreateNative();

        Debug.Assert(!result.IsNull, $"Resource pointer for '{Name}' is NULL");

        _d3dDesc = result.Get()->GetDesc1();

        if ((_d3dDesc.Flags & D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_USE_TIGHT_ALIGNMENT) == D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_USE_TIGHT_ALIGNMENT)
        {
            _d3dDesc.Alignment = 0;
        }

        PopulateSubResourceInfo(in _d3dDesc);
        
        AssignResource(in result);        
    }

    /// <summary>
    /// Function to return the width of the texture, in pixels, at the specified mip level
    /// </summary>
    /// <param name="mipLevel">The mip level to evaluate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetMipWidth(short mipLevel) => Width >> mipLevel.Min((short)(MipCount - 1)).Max(0);

    /// <summary>
    /// Function to return the height of the texture, in pixels, at the specified mip level
    /// </summary>
    /// <param name="mipLevel">The mip level to evaluate.</param>
    /// <remarks>
    /// <para>
    /// This only applies to textures with a <see cref="Type"/> of <see cref="TextureType.Texture2D"/> and <see cref="TextureType.Texture3D"/>, otherwise the method will return 1.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetMipHeight(short mipLevel) => Type == TextureType.Texture1D ? 1 : Height >> mipLevel.Min((short)(MipCount - 1)).Max(0);

    /// <summary>
    /// Function to return the depth of the texture, in depth slices, at the specified mip level.
    /// </summary>
    /// <param name="mipLevel">The mip level to evaluate.</param>
    /// <remarks>
    /// <para>
    /// This only applies to textures with a <see cref="Type"/> of <see cref="TextureType.Texture3D"/>, otherwise the method will return 1.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short GetMipDepth(short mipLevel) => (short)(Type == TextureType.Texture3D ? Depth >> mipLevel.Min((short)(MipCount - 1)).Max(0) : 1);

    /// <summary>
    /// Function to convert a single horizontal pixel value to a texel coordinate.
    /// </summary>
    /// <param name="x">The horizontal pixel coordinate to covnert.</param>
    /// <param name="mipLevel">[Optional] The mip level for the texture.</param>
    /// <returns>The horizontal texel coordinate.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ToTexel(int x, short mipLevel = 0) => x / (float)GetMipWidth(mipLevel);

    /// <summary>
    /// Function to convert a 2D pixel coordinate value to a 2D texel coordinate.
    /// </summary>
    /// <param name="point">The pixel coordinate to covnert.</param>
    /// <param name="mipLevel">[Optional] The mip level for the texture.</param>
    /// <returns>The texel coordinate.</returns>
    /// <remarks>
    /// <para>
    /// For <see cref="TextureType.Texture1D"/> resources, this method will return a <see cref="Vector2.Y"/> value of 0.0f.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToTexel(GorgonPoint point, short mipLevel = 0) => new(point.X / (float)GetMipWidth(mipLevel), Type == TextureType.Texture1D ? 0.0f : point.Y / (float)GetMipHeight(mipLevel));

    /// <summary>
    /// Function to convert a 3D pixel coordinate value to a 3D texel coordinate.
    /// </summary>
    /// <param name="point">The pixel coordinate to covnert.</param>
    /// <param name="mipLevel">[Optional] The mip level for the texture.</param>
    /// <returns>The texel coordinate.</returns>
    /// <remarks>
    /// <para>
    /// For <see cref="TextureType.Texture1D"/> resources, this method will return a <see cref="Vector3.Y"/> and <see cref="Vector3.Z"/> value of 0.0f.
    /// </para>
    /// <para>
    /// For <see cref="TextureType.Texture2D"/> resources, this method will return a <see cref="Vector3.Z"/> value of 0.0f.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ToTexel(Vector3 point, short mipLevel = 0) => new(point.X / GetMipWidth(mipLevel), Type == TextureType.Texture1D ? 0.0f : point.Y / GetMipHeight(mipLevel), Type == TextureType.Texture3D ? point.Z / GetMipDepth(mipLevel) : 0.0f);

    /// <summary>
    /// Function to convert a single horizontal texel value to a pixel coordinate.
    /// </summary>
    /// <param name="tx">The horizontal texel coordinate to covnert.</param>
    /// <param name="mipLevel">[Optional] The mip level for the texture.</param>
    /// <returns>The horizontal pixel coordinate.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ToPixel(int tx, short mipLevel = 0) => tx * (float)GetMipWidth(mipLevel);

    /// <summary>
    /// Function to convert a 2D texel coordinate value to a 2D pixel coordinate.
    /// </summary>
    /// <param name="texel">The texel coordinate to covnert.</param>
    /// <param name="mipLevel">[Optional] The mip level for the texture.</param>
    /// <returns>The pixel coordinate.</returns>
    /// <remarks>
    /// <para>
    /// For <see cref="TextureType.Texture1D"/> resources, this method will return a <see cref="GorgonPoint.Y"/> value of 0.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonPoint"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonPoint ToPixel(Vector2 texel, short mipLevel = 0) => new((int)(texel.X * GetMipWidth(mipLevel)), Type == TextureType.Texture1D ? 0 : (int)(texel.Y * GetMipHeight(mipLevel)));

    /// <summary>
    /// Function to convert a 3D texel coordinate value to a 3D pixel coordinate.
    /// </summary>
    /// <param name="point">The texel coordinate to covnert.</param>
    /// <param name="mipLevel">[Optional] The mip level for the texture.</param>
    /// <returns>The pixel coordinate.</returns>
    /// <remarks>
    /// <para>
    /// For <see cref="TextureType.Texture1D"/> resources, this method will return a <see cref="Vector3.Y"/> and <see cref="Vector3.Z"/> value of 0.
    /// </para>
    /// <para>
    /// For <see cref="TextureType.Texture2D"/> resources, this method will return a <see cref="Vector3.Z"/> value of 0.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ToPixel(Vector3 point, short mipLevel = 0) => new(point.X * GetMipWidth(mipLevel), Type == TextureType.Texture1D ? 0.0f : point.Y * GetMipHeight(mipLevel), Type == TextureType.Texture3D ? point.Z * GetMipDepth(mipLevel) : 0.0f);

    /// <summary>
    /// Function to convert 2D rectangle pixel coordinates into 2D rectangle texel coordinates.
    /// </summary>
    /// <param name="rect">The pixel coorindates.</param>
    /// <param name="mipLevel">[Optional] The mip level for the texture.</param>
    /// <returns>The converted texel coordinates.</returns>
    /// <remarks>
    /// <para>
    /// For <see cref="TextureType.Texture1D"/> resources, this method will return a <see cref="GorgonRectangleF.Y"/> value of 0, and a <see cref="GorgonRectangleF.Height"/> value of 1.0f.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonRectangleF"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonRectangleF ToTexelRectangle(GorgonRectangle rect, short mipLevel = 0)
    {
        float mipWidth = GetMipWidth(mipLevel);
        float mipHeight = Type == TextureType.Texture1D ? 1.0f : GetMipHeight(mipLevel);
        return new(rect.X / mipWidth, Type == TextureType.Texture1D ? 0 : rect.Y / mipHeight, rect.Width / mipWidth, Type == TextureType.Texture1D ? 1.0f : rect.Height / mipHeight);
    }

    /// <summary>
    /// Function to convert 2D rectangle texel coordinates into 2D rectangle pixel coordinates.
    /// </summary>
    /// <param name="texels">The texel coorindates.</param>
    /// <param name="mipLevel">[Optional] The mip level for the texture.</param>
    /// <returns>The converted pixel coordinates.</returns>
    /// <remarks>
    /// <para>
    /// For <see cref="TextureType.Texture1D"/> resources, this method will return a <see cref="GorgonRectangle.Y"/> value of 0, and a <see cref="GorgonRectangle.Height"/> value of 1.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonRectangle"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonRectangle ToPixelRectangle(GorgonRectangleF texels, short mipLevel = 0)
    {
        float mipWidth = GetMipWidth(mipLevel);
        float mipHeight = GetMipHeight(mipLevel);
        return new((int)(texels.X * mipWidth), Type == TextureType.Texture1D ? 0 : (int)(texels.Y * mipHeight), (int)(texels.Width * mipWidth), Type == TextureType.Texture1D ? 1 : (int)(texels.Height * mipHeight));
    }

    /// <summary>
    /// Function to convert 3D box pixel coordinates into 3D box texel coordinates.
    /// </summary>
    /// <param name="box">The pixel coorindates.</param>
    /// <param name="mipLevel">[Optional] The mip level for the texture.</param>
    /// <returns>The converted texel coordinates.</returns>
    /// <remarks>
    /// <para>
    /// For <see cref="TextureType.Texture1D"/> resources, this method will return a <see cref="GorgonBox.Y"/> and <see cref="GorgonBox.Z"/> value of 0, and a <see cref="GorgonBox.Height"/> and a 
    /// <see cref="GorgonBox.Depth"/> value of 1.0f.
    /// </para>
    /// <para>
    /// For <see cref="TextureType.Texture2D"/> resources, this method will return a <see cref="GorgonBox.Z"/> value of 0, and a <see cref="GorgonBox.Depth"/> value of 1.0f.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonBoxF"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonBoxF ToTexelBox(GorgonBox box, short mipLevel = 0)
    {
        float mipWidth = GetMipWidth(mipLevel);
        float mipHeight = GetMipHeight(mipLevel);
        float mipDepth = GetMipDepth(mipLevel);

        return new(box.X / mipWidth,
                            Type == TextureType.Texture1D ? 0 : box.Y / mipHeight,
                            Type == TextureType.Texture3D ? box.Z / mipDepth : 0,
                            box.Width / mipWidth,
                            Type == TextureType.Texture1D ? 1.0f : box.Height / mipHeight,
                            Type == TextureType.Texture3D ? box.Depth / mipDepth : 1.0f);
    }

    /// <summary>
    /// Function to convert 3D Box texel coordinates into 3D Box pixel coordinates.
    /// </summary>
    /// <param name="texels">The texel coorindates.</param>
    /// <param name="mipLevel">[Optional] The mip level for the texture.</param>
    /// <returns>The converted pixel coordinates.</returns>
    /// <remarks>
    /// <para>
    /// For <see cref="TextureType.Texture1D"/> resources, this method will return a <see cref="GorgonBox.Y"/> and <see cref="GorgonBox.Z"/> value of 0, and a <see cref="GorgonBox.Height"/> and a 
    /// <see cref="GorgonBox.Depth"/> value of 1.
    /// </para>
    /// <para>
    /// For <see cref="TextureType.Texture2D"/> resources, this method will return a <see cref="GorgonBox.Z"/> value of 0, and a <see cref="GorgonBox.Depth"/> value of 1.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonBoxF"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonBox ToPixelBox(GorgonBoxF texels, short mipLevel = 0)
    {
        float mipWidth = GetMipWidth(mipLevel);
        float mipHeight = GetMipHeight(mipLevel);
        float mipDepth = GetMipDepth(mipLevel);

        return new((int)(texels.X * mipWidth),
                                Type == TextureType.Texture1D ? 0 : (int)(texels.Y * mipHeight),
                                Type == TextureType.Texture3D ? (int)(texels.Z * mipDepth) : 0,
                                (int)(texels.Width * mipWidth),
                                Type == TextureType.Texture1D ? 1 : (int)(texels.Height * mipHeight),
                                Type == TextureType.Texture3D ? (int)(texels.Depth * mipDepth) : 1);
    }

    /// <summary>
    /// Function to calculate the sub resource index for a texture.
    /// </summary>
    /// <param name="mipLevel">The mip map level for the texture sub resource..</param>
    /// <param name="arrayIndex">The array index (for 1D and 2D textures). For 3D textures, this value should be 0.</param>
    /// <param name="plane">[Optional] The plane slice for the format.</param>
    /// <returns>The texture sub resource index.</returns>
    /// <remarks>
    /// <para>
    /// Use this method to retrieve the index of a sub resource within the texture for the given <paramref name="mipLevel"/> and <paramref name="arrayIndex"/> values. For a 
    /// <see cref="TextureType.Texture1D"/> or <see cref="TextureType.Texture2D"/> the <paramref name="arrayIndex"/> value is for array slice of the texture. Otherwise, it will represent the depth 
    /// slice.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetSubResourceIndex(int mipLevel, int arrayIndex, int plane = 0)
    {
        mipLevel = mipLevel.Min(MipCount - 1).Max(0);
        arrayIndex = Type != TextureType.Texture3D ? arrayIndex.Min(ArrayCount - 1).Max(0) : 0;

        return (int)DX.D3D12CalcSubresource((uint)mipLevel, (uint)arrayIndex, (uint)plane, (uint)MipCount, (uint)ArrayCount);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTextureCommon"/> resource.
    /// </summary>
    /// <param name="graphics">The graphics interface associated with this texture.</param>
    /// <param name="name">The name of the texture.</param>
    /// <param name="resource">The predefined D3D12 resource that this texture is wrapping.</param>
    /// <param name="info">The texture information.</param>
    /// <remarks>
    /// <para>
    /// This is used for swap chain creation.
    /// </para>
    /// </remarks>
    protected GorgonTextureCommon(GorgonGraphics graphics, string name, ComPtr<ID3D12Resource2> resource, GorgonTextureInfo info)
        : base(graphics, name, resource)
    {
        D3D12_RESOURCE_DESC1 desc = resource.Get()->GetDesc1();

        _info = new GorgonTextureInfo(info);
        FormatInfo = new GorgonFormatInfo(info.Format);

        if (!FormatGroups.TryGetValue(FormatInfo.SizeInBytes, out List<BufferFormat>? compatibleFormats))
        {
            compatibleFormats = [];
        }

        CompatibleFormats = compatibleFormats;

        PopulateSubResourceInfo(in desc);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTextureCommon"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string)" path="/param[@name='name']"/></param>
    /// <param name="info">Information used to create the texture.</param>
    protected GorgonTextureCommon(GorgonGraphics graphics, string name, GorgonTextureInfo info)
        : base(graphics, name)
    {
        FormatInfo = new GorgonFormatInfo(info.Format);
        _info = new GorgonTextureInfo(ValidateInfo(info));

        if (!FormatGroups.TryGetValue(FormatInfo.SizeInBytes, out List<BufferFormat>? compatibleFormats))
        {
            compatibleFormats = [];
        }

        CompatibleFormats = compatibleFormats;
        SubResources = GorgonSubResourceInfoList.Empty;
    }

    /// <summary>
    /// Initializes the <see cref="GorgonTextureCommon"/> class.
    /// </summary>
    static GorgonTextureCommon()
    {
        BufferFormat[] formats = Enum.GetValues<BufferFormat>();

        for (int i = 0; i < formats.Length; ++i)
        {
            GorgonFormatInfo info = new(formats[i]);

            if ((info.IsTypeless) || (info.SizeInBytes < 1) || (info.IsPlanar) || (info.IsCompressed))
            {
                continue;
            }

            if (!FormatGroups.TryGetValue(info.SizeInBytes, out List<BufferFormat>? formatList))
            {
                FormatGroups[info.SizeInBytes] = formatList = [];
            }

            formatList.Add(info.Format);
        }
    }
}
