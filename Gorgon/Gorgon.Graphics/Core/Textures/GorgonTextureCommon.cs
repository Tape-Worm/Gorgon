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
    /// <summary>
    /// Maximum width of a 1D texture.
    /// </summary>
    public const int Max1DTextureWidth = D3D12.D3D12_REQ_TEXTURE1D_U_DIMENSION;
    /// <summary>
    /// Maximum number of array indices for a 1D texture.
    /// </summary>
    public const int Max1DArraySize = D3D12.D3D12_REQ_TEXTURE1D_ARRAY_AXIS_DIMENSION;
    /// <summary>
    /// Maximum width of a 2D texture.
    /// </summary>
    public const int Max2DTextureWidth = D3D12.D3D12_REQ_TEXTURE2D_U_OR_V_DIMENSION;
    /// <summary>
    /// Maximum height of a 2D texture.
    /// </summary>
    public const int Max2DTextureHeight = D3D12.D3D12_REQ_TEXTURE2D_U_OR_V_DIMENSION;
    /// <summary>
    /// Maximum number of array indices for a 2D texture.
    /// </summary>
    public const int Max2DArraySize = D3D12.D3D12_REQ_TEXTURE2D_ARRAY_AXIS_DIMENSION;
    /// <summary>
    /// Maximum width of a 3D texture.
    /// </summary>
    public const int Max3DTextureWidth = D3D12.D3D12_REQ_TEXTURE3D_U_V_OR_W_DIMENSION;
    /// <summary>
    /// Maximum height of a 3D texture.
    /// </summary>
    public const int Max3DTextureHeight = D3D12.D3D12_REQ_TEXTURE3D_U_V_OR_W_DIMENSION;
    /// <summary>
    /// Maximum depth of a 3D texture.
    /// </summary>
    public const int Max3DTextureDepth = D3D12.D3D12_REQ_TEXTURE3D_U_V_OR_W_DIMENSION;

    /// <summary>
    /// A unique key for a view.
    /// </summary>
    /// <param name="Format">The format of the view.</param>
    /// <param name="Value1">The first key value.</param>
    /// <param name="Value2">The second key value.</param>
    /// <param name="Value3">The third key value.</param>
    /// <param name="Value4">The fourth key value.</param>
    /// <param name="Value5">The fifth key value.</param>
    /// <param name="Value6">The sixth key value.</param>
    protected private readonly record struct ViewKey(short Format, short Value1, short Value2, short Value3, short Value4, int Value5, byte Value6);

    private readonly GorgonTextureInfo _info;
    private D3D12_RESOURCE_DESC1 _d3dDesc;
    private readonly Lock _viewLock = new();
    private readonly Dictionary<ViewKey, GorgonRenderTargetView> _rtvs = [];
    private readonly Dictionary<ViewKey, IGorgonTextureView<GorgonTextureCommon>> _srvs = [];
    private readonly Dictionary<ViewKey, GorgonResourceView> _uavs = [];

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
    /// Property to return the list of formats that can be casted to in a <see cref="IGorgonTextureView{GorgonTexture}"/>.
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
    /// <exception cref="GorgonException"><para>Thrown when the <paramref name="info"/> <see cref="GorgonTextureInfo.MipCount"/> is less than 1, or greater than the number of supported mip levels for the texture.</para>
    /// <para>Thrown if the texture type is <see cref="TextureType.Texture3D"/> and the <paramref name="info"/> <see cref="GorgonTextureInfo.Width"/> is less than 1, or greater than <see cref="Max3DTextureWidth"/>, or <see cref="GorgonTextureInfo.Height"/> is less than 1, or greater than <see cref="Max3DTextureHeight"/>, or the <see cref="GorgonTextureInfo.Depth"/> is less than 1, or greater than <see cref="Max3DTextureDepth"/>, or the <see cref="GorgonTextureInfo.Format"/> doesn't support 3D textures, or the <see cref="GorgonTextureInfo.IsCube"/> is set to <b>true</b>.</para>
    /// <para>Thrown if the texture type is <see cref="TextureType.Texture2D"/> and the <paramref name="info"/> <see cref="GorgonTextureInfo.Width"/> is less than 1, or greater than <see cref="Max2DTextureWidth"/>, or <see cref="GorgonTextureInfo.Height"/> is less than 1, or greater than <see cref="Max2DTextureHeight"/>, or the <see cref="GorgonTextureInfo.ArrayCount"/> is less than 1, or greater than <see cref="Max2DArraySize"/>, or the <see cref="GorgonTextureInfo.Format"/> doesn't support 2D textures, or the <see cref="GorgonTextureInfo.IsCube"/> is set to <b>true</b> and the <see cref="GorgonTextureInfo.ArrayCount"/> is not a multiple of 6.</para>
    /// <para>Thrown if the texture type is <see cref="TextureType.Texture1D"/> and the <paramref name="info"/> <see cref="GorgonTextureInfo.Width"/> is less than 1, or greater than <see cref="Max1DTextureWidth"/> or the <see cref="GorgonTextureInfo.ArrayCount"/> is less than 1, or greater than <see cref="Max1DArraySize"/>, or the <see cref="GorgonTextureInfo.Format"/> doesn't support 1D textures, or the <see cref="GorgonTextureInfo.IsCube"/> is set to <b>true</b>.</para>
    /// <para>Thrown if the texture is a depth/stencil and the <paramref name="info"/> <see cref="GorgonTextureInfo.Format"/> does not support depth/stencil, or <see cref="GorgonTextureInfo.HasReadWriteAccess"/> is set to <b>true</b>, 
    /// or the <paramref name="info"/> <see cref="GorgonTextureInfo.Type"/> is not set to <see cref="TextureType.Texture2D"/>.</para>
    /// <para>Thrown if the texture is a render target and the <paramref name="info"/> <see cref="GorgonTextureInfo.Format"/> does not support render targets or MSAA render targets.</para>
    /// <para>Thrown if the texture is an read/write resource and the <paramref name="info"/> <see cref="GorgonTextureInfo.MultisampleInfo"/> is not set to <see cref="GorgonMultisampleInfo.NoMultisampling"/>.</para>
    /// <para>Thrown if the texture has a mip count greater than 1 and the <paramref name="info"/> <see cref="GorgonTextureInfo.MultisampleInfo"/> is not set to <see cref="GorgonMultisampleInfo.NoMultisampling"/>, or the <see cref="GorgonTextureInfo.Format"/> does not support mip maps.</para>
    /// <para>Thrown if the texture is a cube map and the <paramref name="info"/> <see cref="GorgonTextureInfo.Format"/> does not support cube maps.</para>
    /// <para>Thrown if the texture uses a compressed format, and does not have dimensions that are a multiple of 4.</para>
    /// </exception>
    private protected virtual GorgonTextureInfo ValidateInfo(GorgonTextureInfo info)
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

                if (info.Height < 1)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_LESS_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.Height), 1));
                }

                if (info.Depth < 1)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_LESS_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.Depth), 1));
                }

                if (info.Width > D3D12.D3D12_REQ_TEXTURE3D_U_V_OR_W_DIMENSION)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_GREATER_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.Width), D3D12.D3D12_REQ_TEXTURE3D_U_V_OR_W_DIMENSION));
                }

                if (info.Height > D3D12.D3D12_REQ_TEXTURE3D_U_V_OR_W_DIMENSION)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_GREATER_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.Height), D3D12.D3D12_REQ_TEXTURE3D_U_V_OR_W_DIMENSION));
                }

                if (info.Depth > D3D12.D3D12_REQ_TEXTURE3D_U_V_OR_W_DIMENSION)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_GREATER_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.Depth), D3D12.D3D12_REQ_TEXTURE3D_U_V_OR_W_DIMENSION));
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

                if (info.ArrayCount > D3D12.D3D12_REQ_TEXTURE2D_ARRAY_AXIS_DIMENSION)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_GREATER_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.ArrayCount), D3D12.D3D12_REQ_TEXTURE2D_ARRAY_AXIS_DIMENSION));
                }

                if ((info.IsCube) && ((info.ArrayCount % 6) != 0))
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CUBEMAP_NOT_MULTIPLE_OF_SIX, info.ArrayCount));
                }

                if (info.Height < 1)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_LESS_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.Height), 1));
                }

                if (info.Width > D3D12.D3D12_REQ_TEXTURE2D_U_OR_V_DIMENSION)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_GREATER_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.Width), D3D12.D3D12_REQ_TEXTURE2D_U_OR_V_DIMENSION));
                }

                if (info.Height > D3D12.D3D12_REQ_TEXTURE2D_U_OR_V_DIMENSION)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_GREATER_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.Height), D3D12.D3D12_REQ_TEXTURE2D_U_OR_V_DIMENSION));
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

                if (info.Width > D3D12.D3D12_REQ_TEXTURE1D_U_DIMENSION)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_GREATER_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.Width), D3D12.D3D12_REQ_TEXTURE1D_U_DIMENSION));
                }

                if (info.ArrayCount < 1)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_LESS_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.ArrayCount), 1));
                }

                if (info.ArrayCount > D3D12.D3D12_REQ_TEXTURE1D_ARRAY_AXIS_DIMENSION)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_GREATER_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.ArrayCount), D3D12.D3D12_REQ_TEXTURE1D_ARRAY_AXIS_DIMENSION));
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
            short maxMipLevel = GorgonTextureInfo.GetMaximumMipCount(info.Width, info.Height, info.Depth);

            if (info.MipCount > maxMipLevel)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_PARAMETER_GREATER_THAN_VALUE, nameof(GorgonTextureInfo) + "." + nameof(info.MipCount), maxMipLevel));
            }

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
    private protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Remove all the child views.
            foreach (GorgonResourceView view in _rtvs.Values.Cast<GorgonResourceView>()
                                                            .Concat(_srvs.Values.Cast<GorgonResourceView>())
                                                            .Concat(_uavs.Values.Cast<GorgonResourceView>())
                                                            .Where(v => !v.OwnsResource))
            {
                view.Dispose();
            }

            _rtvs.Clear();
            _srvs.Clear();
            _uavs.Clear();
        }

        base.Dispose(disposing);
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
                            int r = GetSubResourceIndex((short)m, (short)a, (byte)p);
                            D3D12_PLACED_SUBRESOURCE_FOOTPRINT footPrint = footPrints[r];
                            GorgonSubResourceInfo info = new(r, (int)footPrint.Footprint.Width, (short)footPrint.Footprint.Height, (short)footPrint.Footprint.Depth,
                                (short)a, (short)m, (byte)p,
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
    /// Function to retrieve a <see cref="IGorgonTextureView{T}"/> to pass to shaders.
    /// </summary>
    /// <param name="format">[Optional] The format to view the texture with.</param>
    /// <param name="mipLevel">[Optional] The first mip level in the view.</param>
    /// <param name="mipCount">[Optional] The number of mip levels to view.</param>
    /// <param name="arrayIndex">[Optional] The first array index in the view.</param>
    /// <param name="arrayCount">[Optional] The number of array indices in the view.</param>
    /// <param name="resourceMinLodClamp">[Optional] The minimum mip level that you can access.</param>
    /// <param name="planeIndex">[Optional] The index of the format plane to use.</param>
    /// <param name="owned"><b>true</b> if the texture is owned by the view, or <b>false</b> if not.</param>
    /// <returns>The <see cref="IGorgonTextureView{GorgonTexture}"/> for the texture.</returns>
    /// <inheritdoc cref="TextureView{GorgonTexture}.ValidateTextureView(string, GorgonFormatInfo, GorgonFormatInfo, IReadOnlyList{BufferFormat}, byte, bool, bool)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This allows access to textures, in shaders, as a resource. The view allows for accessing a piece of the texture, like a single or multiple mip map levels, array indices, and format planes. It also 
    /// allows for accessing the entire texture.
    /// </para>
    /// <para type="param_constraints">
    /// All parameters that access a portion of the physical texture, are clamped against the texture's minimum and maximum values for mip levels, array indices, and format plane count. 
    /// </para>
    /// <para>
    /// If the <paramref name="arrayCount"/>, or <paramref name="mipCount"/> are less than 1, then this will ensure the view uses the remainder (starting from <paramref name="mipLevel"/> or 
    /// <paramref name="arrayIndex"/>) of the texture for the view.
    /// </para>
    /// <para type="format_casting">
    /// The <paramref name="format"/> can be any format that the texture's <see cref="Format"/> can cast to. To determine which formats the texture 
    /// <see cref="Format"/> can be casted into, check the <see cref="CompatibleFormats"/> list. Even if this list is empty, as long as the texture and view format 
    /// belong to the same <see cref="GorgonFormatInfo.Group"/> the format can be casted. To check whether the formats belong to the same group, use the <see cref="GorgonFormatInfo.Group"/> property on the 
    /// <see cref="GorgonFormatInfo"/> object. The <see cref="GorgonTexture"/> object already has information about its format via the <see cref="FormatInfo"/> property.
    /// </para>
    /// <para>
    /// <h3>Special Considerations</h3>
    /// </para>
    /// <para>
    /// <h3><see cref="IsDepthStencil">Depth/Stencil</see></h3>
    /// </para>
    /// <para>
    /// A <see cref="IGorgonTextureView{GorgonTexture}"/> cannot be created for a texture that has a depth/stencil format of:
    /// <list type="bullet">
    ///     <item>
    ///         <description><see cref="BufferFormat.D16_UNorm"/></description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="BufferFormat.D32_Float"/></description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="BufferFormat.D24_UNorm_S8_UInt"/></description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="BufferFormat.D32_Float_S8X24_UInt"/></description>
    ///     </item>
    /// </list>
    /// Attempting to retrieve a view with a texture created with these formats will throw an exception.
    /// </para>
    /// <para>
    /// If a view is required for a depth/stencil texture, the texture must be created with one of the following <b>typeless</b> formats:
    /// <list type="bullet">
    ///     <item>
    ///         <description><see cref="BufferFormat.R16_Typeless"/></description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="BufferFormat.R32_Typeless"/></description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="BufferFormat.R24G8_Typeless"/></description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="BufferFormat.R32G8X24_Typeless"/></description>
    ///     </item>
    /// </list>
    /// Plus, the view <paramref name="format"/> must be one of the following formats, and plane indices (0 for the depth portion, 1 for the stencil portion):
    /// <list type="table">
    ///     <listheader>
    ///         <term>Texture Format</term>
    ///         <term>Depth Format</term>
    ///         <term>Stencil Format</term>
    ///         <term><paramref name="planeIndex"/></term>
    ///     </listheader>
    ///     <item>
    ///         <description><see cref="BufferFormat.R16_Typeless"/></description>
    ///         <description><see cref="BufferFormat.R16_UNorm"/></description>
    ///         <description>N/A</description>
    ///         <description>0</description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="BufferFormat.R32_Typeless"/></description>
    ///         <description><see cref="BufferFormat.R32_Float"/></description>
    ///         <description>N/A</description>
    ///         <description>0</description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="BufferFormat.R24G8_Typeless"/></description>
    ///         <description><see cref="BufferFormat.R24_UNorm_X8_Typeless"/></description>
    ///         <description><see cref="BufferFormat.X24_Typeless_G8_UInt"/></description>
    ///         <description>1</description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="BufferFormat.R32G8X24_Typeless"/></description>
    ///         <description><see cref="BufferFormat.R32_Float_X8X24_Typeless"/></description>
    ///         <description><see cref="BufferFormat.X32_Typeless_G8X24_UInt"/></description>
    ///         <description>1</description>
    ///     </item>
    /// </list>
    /// If any of these requirements are not met, then an exception will be thrown.
    /// </para>
    /// <para>
    /// <h3>Planar Formats</h3>
    /// </para>
    /// <para>
    /// The <paramref name="format"/> must not be a planar format, if it is, then an exception will be thrown. If the texture <see cref="Format"/> is planar, then no validation is 
    /// performed by Gorgon. However, the underlying Direct3D 12 runtime will show an error if a constraint is violated. Applications can receive debug information from the underlying runtime by creating the 
    /// <see cref="GorgonGraphicsFactory"/> with debugging flags pass to the constructor, and calling the <see cref="GorgonGraphics.RegisterDebugInformationCallback(GorgonDebugInformationCallback)"/> 
    /// method to receive the debug messages.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonTextureView{GorgonTexture}"/>
    /// <seealso cref="GorgonFormatInfo"/>
    /// <seealso cref="GorgonGraphicsFactory"/>
    /// <seealso cref="GorgonGraphics.RegisterDebugInformationCallback(GorgonDebugInformationCallback)"/>
    /// <seealso cref="BufferFormat"/>
    /// <seealso cref="CompatibleFormats"/>
    internal IGorgonTextureView<T> GetTextureView<T>(BufferFormat format, short mipLevel, short mipCount, short arrayIndex, short arrayCount, float resourceMinLodClamp, byte planeIndex, bool owned)
        where T : GorgonTextureCommon
    {
        using (_viewLock.EnterScope())
        {
            if (format == BufferFormat.Unknown)
            {
                format = Format;
            }

            byte planeCount = Graphics.FormatSupport[format].PlaneCount;

            mipLevel = mipLevel.Max(0).Min((short)(MipCount - 1));
            arrayIndex = arrayIndex.Max(0).Min((short)(ArrayCount - 1));

            if (mipCount <= 0)
            {
                mipCount = (short)(MipCount - mipLevel);
            }
            else
            {
                mipCount = mipCount.Min((short)(MipCount - mipLevel)).Max(1);
            }

            if (arrayCount <= 0)
            {
                arrayCount = (short)(ArrayCount - arrayIndex);
            }
            else
            {
                arrayCount = arrayCount.Min((short)(ArrayCount - arrayIndex)).Max(1);
            }

            planeIndex = planeIndex.Min((byte)(planeCount - 1));

            int lodClamp = *((int*)(&resourceMinLodClamp));

            ViewKey key = new((short)format, mipLevel, mipCount, arrayIndex, arrayCount, lodClamp, planeIndex);

            if (_srvs.TryGetValue(key, out IGorgonTextureView<GorgonTextureCommon>? result))
            {
                return (IGorgonTextureView<T>)result;
            }

            GorgonFormatInfo viewFormatInfo = format == Format ? FormatInfo : new GorgonFormatInfo(format);
            TextureView<T>.ValidateTextureView(Name, FormatInfo, viewFormatInfo, CompatibleFormats, planeIndex, IsShaderResource, IsDepthStencil);
            IGorgonTextureView<T> view = new TextureView<T>(Graphics, Name, (T)this, viewFormatInfo, mipLevel, mipCount, arrayIndex, arrayCount, resourceMinLodClamp, planeIndex, owned);

            _srvs[key] = view;

            return view;
        }
    }

    /// <summary>
    /// Function to retrieve a <see cref="GorgonRenderTargetView"/> to allow this texture to be used as a render target.
    /// </summary>
    /// <param name="format"><inheritdoc cref="GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='format']"/></param>
    /// <param name="mipLevel">[Optional] The mip map level in the texture to view.</param>
    /// <param name="arrayIndexOrDepthSlice">[Optional] For 1D and 2D textures, the first array index to view. For 3D textures, the first depth slice to view.</param>
    /// <param name="arrayCountOrDepthCount">[Optional] For 1D and 2D textures, the number of array indices to view. For 3D textures, the number of depth slices to view.</param>
    /// <param name="planeIndex"><inheritdoc cref="GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='planeIndex']"/></param>
    /// <param name="owned"><inheritdoc cref="GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='owned']"/></param>
    /// <returns>The <see cref="GorgonRenderTargetView"/> for the texture.</returns>
    /// <inheritdoc cref="GorgonRenderTargetView.ValidateRenderTargetView(string, GorgonBufferFormatSupport, GorgonFormatInfo, GorgonFormatInfo, IReadOnlyList{BufferFormat}, bool)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This allows textures to be used as a render target output. 
    /// </para>
    /// <inheritdoc cref="GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/remarks/para[@type='param_constraints']"/>
    /// <para>
    /// If the <see cref="Type"/> is <see cref="TextureType.Texture3D"/>, then <paramref name="arrayIndexOrDepthSlice"/> and <paramref name="arrayCountOrDepthCount"/> will indicate the depth slice; otherwise 
    /// it will indicate the array indices.
    /// </para>
    /// <para>
    /// If the <paramref name="arrayCountOrDepthCount"/> is less than 1, then this will ensure the view uses the remainder (starting from <paramref name="arrayIndexOrDepthSlice"/>) of the texture for the 
    /// view.
    /// </para>
    /// <inheritdoc cref="GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/remarks/para[@type='format_casting']"/>
    /// <para>
    /// The texture must have been created as a render target by passing the <see cref="GorgonTextureInfo"/> with its <see cref="GorgonTextureInfo.IsRenderTarget"/> set to <b>true</b> when creating the 
    /// texture. If the texture was not created as a render target, then an exception will be thrown.
    /// </para>
    /// <para>
    /// The <paramref name="format"/> cannot be a typeless format, and must be supported by render targets. This can be found on the <see cref="GorgonBufferFormatSupport.IsRenderTargetFormat"/> property on 
    /// the <see cref="GorgonBufferFormatSupport"/> object on the <see cref="GorgonGraphics.FormatSupport"/> property. If these conditions are not met, then an exception will be thrown.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonRenderTargetView"/>
    /// <seealso cref="GorgonFormatInfo"/>
    /// <seealso cref="GorgonTextureInfo"/>
    /// <seealso cref="BufferFormat"/>
    /// <seealso cref="CompatibleFormats"/>
    internal GorgonRenderTargetView GetRenderTargetView(BufferFormat format, short mipLevel, short arrayIndexOrDepthSlice, short arrayCountOrDepthCount, byte planeIndex, bool owned)
    {
        using (_viewLock.EnterScope())
        {
            if (format == BufferFormat.Unknown)
            {
                format = Format;
            }

            byte planeCount = Graphics.FormatSupport[format].PlaneCount;

            mipLevel = mipLevel.Max(0).Min((short)(MipCount - 1));
            arrayIndexOrDepthSlice = arrayIndexOrDepthSlice.Max(0).Min((short)(Type == TextureType.Texture3D ? Depth - 1 : ArrayCount - 1));
            planeIndex = planeIndex.Max(0).Min((byte)(planeCount - 1));

            if (arrayCountOrDepthCount <= 0)
            {
                arrayCountOrDepthCount = (short)(Type == TextureType.Texture3D ? Depth - arrayIndexOrDepthSlice : ArrayCount - arrayIndexOrDepthSlice);
            }
            else
            {
                arrayCountOrDepthCount = arrayCountOrDepthCount.Min((short)(Type == TextureType.Texture3D ? Depth - arrayIndexOrDepthSlice : ArrayCount - arrayIndexOrDepthSlice)).Max(1);
            }

            ViewKey key = new((short)format, mipLevel, arrayIndexOrDepthSlice, arrayCountOrDepthCount, planeIndex, 0, 0);

            if (_rtvs.TryGetValue(key, out GorgonRenderTargetView? result))
            {
                return result;
            }

            GorgonFormatInfo formatInfo = format == Format ? FormatInfo : new GorgonFormatInfo(format);
            GorgonRenderTargetView.ValidateRenderTargetView(Name, Graphics.FormatSupport[format], FormatInfo, formatInfo, CompatibleFormats, IsRenderTarget);

            return _rtvs[key] = new(Graphics, Name, this, formatInfo, mipLevel, arrayIndexOrDepthSlice, arrayCountOrDepthCount, planeIndex, owned);
        }
    }

    /// <summary>
    /// <inheritdoc cref="GetRenderTargetView(BufferFormat, short, short, short, byte, bool)" path="/summary"/>
    /// </summary>
    /// <param name="format"><inheritdoc cref="GetRenderTargetView(BufferFormat, short, short, short, byte, bool)" path="/param[@name='format']"/></param>
    /// <param name="mipLevel"><inheritdoc cref="GetRenderTargetView(BufferFormat, short, short, short, byte, bool)" path="/param[@name='mipLevel']"/></param>
    /// <param name="arrayIndexOrDepthSlice"><inheritdoc cref="GetRenderTargetView(BufferFormat, short, short, short, byte, bool)" path="/param[@name='arrayIndexOrDepthSlice']"/></param>
    /// <param name="arrayCountOrDepthCount"><inheritdoc cref="GetRenderTargetView(BufferFormat, short, short, short, byte, bool)" path="/param[@name='arrayCountOrDepthCount']"/></param>
    /// <param name="planeIndex"><inheritdoc cref="GetRenderTargetView(BufferFormat, short, short, short, byte, bool)" path="/param[@name='planeIndex']"/></param>
    /// <inheritdoc cref="GetRenderTargetView(BufferFormat, short, short, short, byte, bool)" path="/returns"/>
    /// <inheritdoc cref="GetRenderTargetView(BufferFormat, short, short, short, byte, bool)" path="/exception"/>
    /// <inheritdoc cref="GetRenderTargetView(BufferFormat, short, short, short, byte, bool)" path="/remarks"/>
    /// <inheritdoc cref="GetRenderTargetView(BufferFormat, short, short, short, byte, bool)" path="/seealso"/>
    public GorgonRenderTargetView GetRenderTargetView(BufferFormat format = BufferFormat.Unknown, short mipLevel = 0, short arrayIndexOrDepthSlice = 0, short arrayCountOrDepthCount = 0, byte planeIndex = 0)
        => GetRenderTargetView(format, mipLevel, arrayIndexOrDepthSlice, arrayCountOrDepthCount, planeIndex, false);

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
        ToTexelBox(in box, out GorgonBoxF texels, mipLevel);
        return texels;
    }

    /// <summary>
    /// Function to convert 3D box pixel coordinates into 3D box texel coordinates.
    /// </summary>
    /// <param name="box">The pixel coorindates.</param>
    /// <param name="texels">The texel coordinates.</param>
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
    public void ToTexelBox(ref readonly GorgonBox box, out GorgonBoxF texels, short mipLevel = 0)
    {
        float mipWidth = GetMipWidth(mipLevel);
        float mipHeight = GetMipHeight(mipLevel);
        float mipDepth = GetMipDepth(mipLevel);

        texels = new(box.X / mipWidth,
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
        ToPixelBox(in texels, out GorgonBox pixels, mipLevel);
        return pixels;
    }

    /// <summary>
    /// Function to convert 3D Box texel coordinates into 3D Box pixel coordinates.
    /// </summary>
    /// <param name="texels">The texel coorindates.</param>
    /// <param name="pixels">The pixel coordinates.</param>
    /// <param name="mipLevel">[Optional] The mip level for the texture.</param>
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
    public void ToPixelBox(ref readonly GorgonBoxF texels, out GorgonBox pixels, short mipLevel = 0)
    {
        float mipWidth = GetMipWidth(mipLevel);
        float mipHeight = GetMipHeight(mipLevel);
        float mipDepth = GetMipDepth(mipLevel);

        pixels = new((int)(texels.X * mipWidth),
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
    public int GetSubResourceIndex(short mipLevel, short arrayIndex, byte plane = 0)
    {
        mipLevel = mipLevel.Min((short)(MipCount - 1)).Max(0);
        arrayIndex = (short)(Type != TextureType.Texture3D ? arrayIndex.Min((short)(ArrayCount - 1)).Max(0) : 0);

        return (int)DX.D3D12CalcSubresource((uint)mipLevel, (uint)arrayIndex, plane, (uint)MipCount, (uint)ArrayCount);
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
