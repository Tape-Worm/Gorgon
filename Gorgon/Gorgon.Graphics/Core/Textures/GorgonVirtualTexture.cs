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
// Created: April 13, 2026 4:55:42 PM
//

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Native;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A virtual texture that can have sections mapped and unmapped from physical memory on demand.
/// </summary>
/// <remarks>
/// <para>
/// A virtual texture is like a standard <see cref="GorgonTexture"/>, except that its storage is divided up into tiles that can be mapped into or unmapped from physical memory. This allows creation of an 
/// extremely large texture, but without consuming all the memory that such a texture would require. Because the texture can be allocated in portions as needed, a virtual texture can be used to stream data 
/// or swap image data in and out like sprite image frames. 
/// </para>
/// <h3>Texture layout</h3>
/// <para>
/// While a <see cref="GorgonTexture"/> is managed by texels, which is a grouping of bytes per texel (1 byte/texel, 4 bytes/texel, etc...), virtual textures are managed by tile maps where texture data is 
/// stored within 64KB tiles. Because of this, it's recommended to handle virtual textures in terms of tiles instead of bytes. For example, if you have a 128x128 texture, with a format of 
/// <see cref="BufferFormat.R8G8B8A8_UNorm"/>, then that would occupy a single tile because the data size would be equal to 65,536 bytes (64 KB). See the image below:
/// </para>
/// <para>
/// <img src="/images/tiletexture.png"/>
/// </para>
/// <para>
/// In the image above, you'll notice that the texture straddles 4 tiles in the texture. So the entire texture will be 262,144 bytes (4 tiles * 65,536 bytes), even though the actual image does not cover all 
/// tiles in the texture. While this may seem wasteful, if the texture were 16,384x16,384 <see cref="BufferFormat.R8G8B8A8_UNorm"/>, it would still only be 262,144 bytes, because the virtual texture only 
/// allocates 4 of the 16,384 tiles in the texture. A regular <see cref="GorgonTexture"/> of the same dimensions and format would allocate 1,073,741,824 bytes upon creation. 
/// </para>
/// <para>
/// This gives us a major advantage because we can use a single texture to potentially represent the image data needed in the scene (assuming the images have a compatible format) and swap in other images when 
/// needed and swap out old ones that we don't need anymore, all without changing the texture.
/// </para>
/// <h3>Data Management</h3>
/// <para>
/// Users can allocate portions of the texture by allocating regions using texel coordinates (i.e. UV for <see cref="TextureType.Texture2D"/> textures, or UVW for <see cref="TextureType.Texture3D"/> textures). 
/// This results in the creation of a unique handle identifer that the user should hold on to and use in operations such as copying texture data. 
/// </para>
/// <para>
/// Conversely, when users are finished with a region on the texture, they use the aforementioned handle to deallocate the region to make it available for other purposes.
/// </para>
/// <para type="overlap_warn">
/// <note type="important">
/// <para>
/// When allocating regions on a virtual texture, care must be taken to ensure they do not overlap. If there is an overlapping (by containment or intersection) region that's already been allocated, any attempt 
/// to allocate that intersecting region will fail. 
/// </para>
/// </note>
/// </para>
/// <para>
/// When the texture is disposed, all allocations are automatically cleaned up and all handles will no longer be valid.
/// </para>
/// </remarks>
/// <seealso cref="GorgonTexture"/>
/// <seealso cref="BufferFormat"/>
/// <seealso cref="TextureType"/>
public sealed unsafe class GorgonVirtualTexture
    : GorgonTextureCommon
{
    private ComPtr<ID3D12Resource2> _d3dResource;

    private readonly Lock _allocLock = new();
    private short _packedMipStartIndex;
    private D3D12_TILE_SHAPE _tileShape;
    private readonly Dictionary<ulong, VirtualTextureAllocation> _textureAllocations = [];
    private readonly ulong[][] _allocationMasks = [];

    /// <inheritdoc/>
    internal override bool IsMegaBufferResource => false;

    /// <summary>
    /// Property to return tile information about the sub resources in the texture.
    /// </summary>
    public GorgonSubResourceTileInfoList SubResourceTileInformation
    {
        get;
        private set;
    } = GorgonSubResourceTileInfoList.Empty;

    /// <summary>
    /// Property to return the width of the texture, in tiles.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A tile is 64 KB (65536 bytes) in size.
    /// </para>
    /// </remarks>
    public int TileWidth
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the height of the texture, in tiles.
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="TileWidth" path="/remarks/para"/>
    /// <para>
    /// This value only applies to <see cref="TextureType.Texture2D"/> or <see cref="TextureType.Texture3D"/> textures.
    /// </para>
    /// </remarks>
    public int TileHeight
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the depth of the texture, in tiles.
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="TileWidth" path="/remarks/para"/>
    /// <para>
    /// This value only applies to <see cref="TextureType.Texture3D"/> textures.
    /// </para>
    /// </remarks>
    public int TileDepth
    {
        get;
        private set;
    }

    /// <summary>
    /// Function to build the flags for the resource description.
    /// </summary>
    /// <returns>The flags for the resource description.</returns>
    private D3D12_RESOURCE_FLAGS BuildFlags()
    {
        D3D12_RESOURCE_FLAGS flags = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE;

        if (HasReadWriteAccess)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
        }

        if (!IsShaderResource)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_DENY_SHADER_RESOURCE;
        }

        if (IsRenderTarget)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET;
        }

        return flags;
    }

    /// <summary>
    /// Function to build a list of castable formats.
    /// </summary>
    /// <returns>The buffer containing the list of castable formats.</returns>
    private GorgonNativeBuffer<DXGI_FORMAT> BuildCastList()
    {
        GorgonNativeBuffer<DXGI_FORMAT> castable = [];

        if ((FormatGroups.TryGetValue(FormatInfo.SizeInBytes, out List<BufferFormat>? compatibleFormats))
            && (compatibleFormats.Count > 0))
        {
            castable = new GorgonNativeBuffer<DXGI_FORMAT>(compatibleFormats.Count);

            for (int i = 0; i < compatibleFormats.Count; ++i)
            {
                castable[i] = (DXGI_FORMAT)compatibleFormats[i];
            }
        }

        return castable;
    }

    /// <summary>
    /// Function to retrieve the details about the reserved resource.
    /// </summary>
    private void GetReservedDetails()
    {
        List<GorgonSubResourceTileInfo> result = [];

        uint totalTileCount = 0;
        uint subResourceCount = (uint)SubResources.Count;
        D3D12_PACKED_MIP_INFO packedMipInfo = default;
        D3D12_TILE_SHAPE shape = default;
        D3D12_SUBRESOURCE_TILING* subResources = stackalloc D3D12_SUBRESOURCE_TILING[SubResources.Count];

        Graphics.D3DDevice.Get()->GetResourceTiling((PID3D12Resource2)D3DResource.Get(), &totalTileCount, &packedMipInfo, &shape, &subResourceCount, 0, subResources);

        _tileShape = shape;

        for (int a = 0; a < ArrayCount; ++a)
        {
            for (int m = 0; m < packedMipInfo.NumStandardMips; ++m)
            {
                int subResIndex = GetSubResourceIndex((short)m, (short)a);
                D3D12_SUBRESOURCE_TILING data = subResources[subResIndex];

                result.Add(new GorgonSubResourceTileInfo(subResIndex, 
                    (int)data.WidthInTiles, (short)data.HeightInTiles, (short)data.DepthInTiles, 
                    (short)a, (short)m, 1));
            }

            if (packedMipInfo.NumPackedMips > 0)
            {
                int subResIndex = GetSubResourceIndex(packedMipInfo.NumStandardMips, (short)a);
                D3D12_SUBRESOURCE_TILING data = subResources[subResIndex];
                
                result.Add(new GorgonSubResourceTileInfo(subResIndex,
                    (int)packedMipInfo.NumTilesForPackedMips, 1, 1,
                    (short)a, packedMipInfo.NumStandardMips, packedMipInfo.NumPackedMips));                
            }
        }

        _packedMipStartIndex = packedMipInfo.NumStandardMips;
        TileWidth = result[0].Width;
        TileHeight = result[0].Height;
        TileDepth = result[0].Depth;

        SubResourceTileInformation = new GorgonSubResourceTileInfoList(this, result);
    }

    /// <summary>
    /// Function to map an allocation to the sub resource bitmap.
    /// </summary>
    /// <param name="tileRegion">The tile region to map.</param>
    /// <param name="subresourceIndex">The sub resource index to map.</param>
    /// <param name="tileInfo">Sub resource tile information.</param>
    /// <param name="operation">The function callback used to manage the bitmap data.</param>
    private void SetAllocationBitmap(ref readonly GorgonBox tileRegion, int subresourceIndex, GorgonSubResourceTileInfo tileInfo, Func<ulong, ulong, ulong> operation)
    {
        ulong[]? bitmap = _allocationMasks[subresourceIndex];

        bitmap ??= _allocationMasks[subresourceIndex] = new ulong[(tileInfo.SizeInTiles + 63) / 64];

        for (int z = tileRegion.Front; z < tileRegion.Back; ++z)
        {
            int tileSlice = z * tileInfo.Width * tileInfo.Height;

            for (int y = tileRegion.Top; y < tileRegion.Bottom; ++y)
            {
                // Set the number of bits on a row to match the horizontal values we've allocated.
                // This may be a single qword (64 bit value), or multiple. In most cases, this will be 
                // larger than 64 bits, and offset by a non-aligned by 64 value, so we have to to break 
                // it up into chunks and set each qword individually.

                int tileRow = tileSlice + tileInfo.Width * y;
                int rowStart = tileRow + tileRegion.Left;
                int rowEnd = (tileRow + tileRegion.Right) - 1;

                // Get the first and last qword from the row range.
                int qwordStart = rowStart >> 6;
                int qwordEnd = rowEnd >> 6;
                // Extract which bit each piece starts and ends on.
                int firstBit = rowStart & 63;
                int lastBit = (rowEnd & 63) + 1;
                // Build our masks for the start and end of our bitmap.
                ulong headMask = ulong.MaxValue << firstBit;
                ulong tailMask = lastBit == 64 ? ulong.MaxValue : (1UL << lastBit) - 1;

                // If the start and end are within the same qword, then we only need to set the begin and end mask values.
                if (qwordStart == qwordEnd)
                {
                    bitmap[qwordStart] = operation(bitmap[qwordStart], headMask & tailMask);
                    continue;
                }

                // Otherwise, start with our partial start qword, fill in the middle with the full 64 bits set (or unset) 
                // and then fill out the ending qword.
                bitmap[qwordStart] = operation(bitmap[qwordStart], headMask);
                for (int x = qwordStart + 1; x < qwordEnd; ++x)
                {
                    bitmap[x] = operation(bitmap[x], ulong.MaxValue);
                }
                bitmap[qwordEnd] = operation(bitmap[qwordEnd], tailMask);
            }
        }
    }

    /// <summary>
    /// Function to determine if a region of tiles intersects (is already allocated) with our requested region.
    /// </summary>
    /// <param name="tileRegion">The tile region to evaluate.</param>
    /// <param name="subresourceIndex">The sub resource index to evaluate.</param>
    /// <param name="tileInfo">Sub resource tile information.</param>
    private bool HasAllocationBitmap(ref readonly GorgonBox tileRegion, int subresourceIndex, GorgonSubResourceTileInfo tileInfo)
    {
        ulong[]? bitmap = _allocationMasks[subresourceIndex];

        if (bitmap is null)
        {
            return false;
        }

        for (int z = tileRegion.Front; z < tileRegion.Back; ++z)
        {
            int tileSlice = z * tileInfo.Width * tileInfo.Height;

            for (int y = tileRegion.Top; y < tileRegion.Bottom; ++y)
            {
                int tileRow = tileSlice + tileInfo.Width * y;
                int rowStart = tileRow + tileRegion.Left;
                int rowEnd = (tileRow + tileRegion.Right) - 1;

                // Get the first and last qword from the row range.
                int qwordStart = rowStart >> 6;
                int qwordEnd = rowEnd >> 6;
                // Extract which bit each piece starts and ends on.
                int firstBit = rowStart & 63;
                int lastBit = (rowEnd & 63) + 1;
                // Build our masks for the start and end of our bitmap.
                ulong headMask = ulong.MaxValue << firstBit;
                ulong tailMask = lastBit == 64 ? ulong.MaxValue : (1UL << lastBit) - 1;

                // If the start and end are within the same qword, then we only need to check the current bits for allocation overlap.
                if (qwordStart == qwordEnd)
                {
                    if ((bitmap[qwordStart] & (headMask & tailMask)) != 0)
                    {
                        return true;
                    }

                    continue;
                }

                // If the head or tail are intersecting with our region, then this region is allocated.
                if (((bitmap[qwordStart] & headMask) != 0) || ((bitmap[qwordEnd] & tailMask) !=  0))
                {
                    return true;
                }

                // Otherwise, walk through and see if any of the other chunks are allocated.
                for (int x = qwordStart + 1; x < qwordEnd; ++x)
                {
                    if (bitmap[x] != 0)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <inheritdoc/>
    private protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            FreeAll();
        }

        _d3dResource.Dispose();
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    private protected override ComPtr<ID3D12Resource2> OnCreateNative()
    {
        using ComPtr<ID3D12Resource2> result = default;
        D3D12_CLEAR_VALUE* clearValue = null;
        D3D12_RESOURCE_FLAGS flags = BuildFlags();

        if (IsRenderTarget)
        {
            D3D12_CLEAR_VALUE clear = new()
            {
                Format = (DXGI_FORMAT)Format,
            };
            clear.Color[0] = 0;
            clear.Color[1] = 0;
            clear.Color[2] = 0;
            clear.Color[3] = 0;
            clearValue = &clear;
        }
        else if (IsDepthStencil)
        {
            D3D12_CLEAR_VALUE clear = new()
            {
                Format = (DXGI_FORMAT)Format,
                DepthStencil = new D3D12_DEPTH_STENCIL_VALUE
                {
                    Depth = 1.0f,
                    Stencil = 0
                }
            };
            clearValue = &clear;
        }

        D3D12_RESOURCE_DESC desc = Type switch
        {
            TextureType.Texture2D => D3D12_RESOURCE_DESC.Tex2D((DXGI_FORMAT)Format, (ulong)Width, (uint)Height, (ushort)ArrayCount, (ushort)MipCount, 1, 0, flags,
                                        alignment: D3D12.D3D12_DEFAULT_RESOURCE_PLACEMENT_ALIGNMENT, layout: D3D12_TEXTURE_LAYOUT.D3D12_TEXTURE_LAYOUT_64KB_UNDEFINED_SWIZZLE),
            TextureType.Texture3D => D3D12_RESOURCE_DESC.Tex3D((DXGI_FORMAT)Format, (ulong)Width, (uint)Height, (ushort)Depth, (ushort)MipCount, flags, layout: D3D12_TEXTURE_LAYOUT.D3D12_TEXTURE_LAYOUT_64KB_UNDEFINED_SWIZZLE),
            _ => throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_TEXTURE_UNKNOWN_TYPE, Type))
        };

        string textureName = $"D3D12 {Type} {Name}";

        Graphics.Log.Print($"Created D3D 12 {Type} resource object for '{Name}'.", LoggingLevel.Verbose);

        D3D12MA_ALLOCATION_DESC allocDesc = new(D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT);

        using GorgonNativeBuffer<DXGI_FORMAT> castable = BuildCastList();

        DXGI_FORMAT* castPtr = castable.Length == 0 ? null : (DXGI_FORMAT*)castable;

        Graphics.D3DDevice.Get()->CreateReservedResource2(&desc, D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_UNDEFINED,
            clearValue, null, (uint)castable.Length, castPtr,
            Win32.__uuidof<ID3D12Resource2>(), (void**)result.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => Resources.GORGFX_ERR_CANNOT_CREATE_RESOURCE);

        result.SetD3DDebugName(textureName);

        _d3dResource = new ComPtr<ID3D12Resource2>(result);
        return _d3dResource;
    }

    /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/summary"/>
    /// <param name="info"><inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/param[@name='info']"/></param>
    /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/returns"/>
    /// <exception cref="GorgonException">
    /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para"/>
    /// <para>
    /// Thrown if the <c>info</c> <see cref="GorgonVirtualTextureInfo.Format"/> of the texture is not supported for virtual textures.
    /// </para>
    /// <para>
    /// Thrown if the type of texture is <see cref="TextureType.Texture3D"/>, but the GPU does not support <see cref="TiledResourcesTier.Tier3"/>.
    /// </para>
    /// <para>
    /// Thrown if the texture size, in bytes, is less than 4 MB (4,194,304 bytes).
    /// </para>
    /// </exception>
    private protected override GorgonTextureInfo ValidateInfo(GorgonTextureInfo info)
    {
        info = base.ValidateInfo(info);        

        if (!Graphics.FormatSupport[info.Format].IsTiledFormat)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_VIRTUAL_TEXTURE_FORMAT_INVALID, info.Format, Name));
        }

        if ((info.Type == TextureType.Texture3D) && (Graphics.Adapter.TiledResourcesTier < TiledResourcesTier.Tier3))
        {
            throw new GorgonException(GorgonResult.CannotEnumerate, string.Format(Resources.GORGFX_ERR_VIRTUAL_TEXTURE_3D_NOT_SUPPORTED, Name, nameof(TiledResourcesTier) + "." + Graphics.Adapter.TiledResourcesTier));
        }

        // Ancient devices before 2016 can't support packed mips on array textures. 
        // Instead of dealing with the packed tail bullshit, just limit them to having one or the other.
        // The odds of this being a problem are pretty much near 0.
        if ((Graphics.Adapter.TiledResourcesTier != TiledResourcesTier.Tier4)
            && (info.MipCount > 1) && (info.ArrayCount > 1))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_VIRTUAL_TEXTURE_ARRAY_MIP_NOT_SUPPORTED, Name, info.MipCount, info.ArrayCount,
                Graphics.Adapter.Name, Graphics.Adapter.TiledResourcesTier));
        }

        D3D12_RESOURCE_DESC desc = info.Type switch
        {
            TextureType.Texture2D => D3D12_RESOURCE_DESC.Tex2D((DXGI_FORMAT)info.Format, (ulong)info.Width, (uint)info.Height, (ushort)info.ArrayCount, (ushort)info.MipCount, 1, 0,
                                                                alignment: D3D12.D3D12_DEFAULT_RESOURCE_PLACEMENT_ALIGNMENT, layout: D3D12_TEXTURE_LAYOUT.D3D12_TEXTURE_LAYOUT_64KB_UNDEFINED_SWIZZLE),
            TextureType.Texture3D => D3D12_RESOURCE_DESC.Tex3D((DXGI_FORMAT)info.Format, (ulong)info.Width, (uint)info.Height, (ushort)info.Depth, (ushort)info.MipCount, layout: D3D12_TEXTURE_LAYOUT.D3D12_TEXTURE_LAYOUT_64KB_UNDEFINED_SWIZZLE),
            _ => throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_TEXTURE_UNKNOWN_TYPE, info.Type))
        };

        // A single heap is only 4 MB in size, so a texture with less than that is just a waste for a virtual texture.
        // These things are advanced constructs and meant to hold large swaths of texture data, using them for trivial
        // texture sizes is wasteful.
        D3D12_RESOURCE_ALLOCATION_INFO resInfo = Graphics.D3DDevice.Get()->GetResourceAllocationInfo(0, 1, &desc);

        if (resInfo.SizeInBytes < 4UL * 1024UL * 1024UL)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_VIRTUAL_TEXTURE_TOO_SMALL, Name, resInfo.SizeInBytes.FormatMemory(), resInfo.SizeInBytes));
        }

        return info;
    }

    /// <summary>
    /// Function to convert a 2D texel coordinate into a 2D tile coordinate.
    /// </summary>
    /// <param name="texel">The 2D texel coordinate to convert.</param>
    /// <param name="mipLevel">The mip level to use.</param>
    /// <returns>The texel value, converted to tile coordinates.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonPoint ToTile(Vector2 texel, short mipLevel = 0)
    {
        GorgonPoint pixels = ToPixel(texel, mipLevel);

        return new ((int)((double)pixels.X / _tileShape.WidthInTexels).FastFloor(),
                   (int)((double)pixels.Y / _tileShape.HeightInTexels).FastFloor());
    }

    /// <summary>
    /// Function to convert a 2D region of texel coordinates into a 2D region of tile coordinates.
    /// </summary>
    /// <param name="texels">The 2D texel region to convert.</param>
    /// <param name="mipLevel"><inheritdoc cref="ToTile(Vector2, short)" path="/param[@name='mipLevel']"/></param>
    /// <inheritdoc cref="ToTile(Vector2, short)" path="/returns"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonRectangle ToTiles(GorgonRectangleF texels, short mipLevel = 0)
    {
        GorgonRectangle pixels = ToPixelRectangle(texels, mipLevel);

        return GorgonRectangle.FromLTRB((int)((double)pixels.Left / _tileShape.WidthInTexels).FastFloor(),
                                        (int)((double)pixels.Top / _tileShape.HeightInTexels).FastFloor(),
                                        (int)((double)pixels.Right / _tileShape.WidthInTexels).FastCeiling(),
                                        (int)((double)pixels.Bottom / _tileShape.HeightInTexels).FastCeiling());
    }

    /// <summary>
    /// Function to convert a 2D tile coordinate into a 2D texel coordinate.
    /// </summary>
    /// <param name="tile">The 2D tile coordinate to convert.</param>
    /// <param name="mipLevel"><inheritdoc cref="ToTile(Vector2, short)" path="/param[@name='mipLevel']"/></param>
    /// <returns>The tile value converted, to texel coordinates.</returns>
    /// <remarks>
    /// <para>
    /// Due to the imprecision of a tile, this method will only return the texel coordinate of the upper left corner of the tile.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToTexelOrigin(GorgonPoint tile, short mipLevel = 0)
    {
        GorgonPoint pixels = new(tile.X * (int)_tileShape.WidthInTexels, tile.Y * (int)_tileShape.HeightInTexels);

        return ToTexel(pixels, mipLevel);
    }

    /// <summary>
    /// Function to convert a 2D region of tile coordinates into a region of 2D texel coordinates.
    /// </summary>
    /// <param name="tiles">The 2D tile region to convert.</param>
    /// <param name="mipLevel"><inheritdoc cref="ToTile(Vector2, short)" path="/param[@name='mipLevel']"/></param>
    /// <inheritdoc cref="ToTexelOrigin(GorgonPoint, short)" path="/returns"/>
    /// <remarks>
    /// <para>
    /// Due to the imprecision of a tile, this method will only return the texel coordinate of the whole tile.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonRectangleF FromTiles(GorgonRectangle tiles, short mipLevel = 0)
    {
        GorgonRectangle pixels = GorgonRectangle.FromLTRB(tiles.Left * (int)_tileShape.WidthInTexels,
                                                          tiles.Top * (int)_tileShape.HeightInTexels,
                                                          tiles.Right * (int)_tileShape.WidthInTexels,
                                                          tiles.Bottom * (int)_tileShape.HeightInTexels);

        return ToTexelRectangle(pixels, mipLevel);
    }

    /// <summary>
    /// Function to convert a 3D texel coordinate into a 3D tile coordinate.
    /// </summary>
    /// <param name="texel">The 3D texel value to convert.</param>
    /// <param name="mipLevel"><inheritdoc cref="ToTile(Vector2, short)" path="/param[@name='mipLevel']"/></param>
    /// <inheritdoc cref="ToTile(Vector2, short)" path="/returns"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ToTile(Vector3 texel, short mipLevel = 0)
    {
        Vector3 pixels = ToPixel(texel, mipLevel);

        return new ((int)((double)pixels.X / _tileShape.WidthInTexels).FastFloor(),
                   (int)((double)pixels.Y / _tileShape.HeightInTexels).FastFloor(),
                   (int)((double)pixels.Z / _tileShape.DepthInTexels).FastFloor());
    }

    /// <summary>
    /// Function to convert a 3D region of texel coordinates into a 3D region of tile coordinates.
    /// </summary>
    /// <param name="texels">The 3D texel region to convert.</param>
    /// <param name="tiles">The 3D tile region.</param>
    /// <param name="mipLevel"><inheritdoc cref="ToTile(Vector2, short)" path="/param[@name='mipLevel']"/></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ToTiles(ref readonly GorgonBoxF texels, out GorgonBox tiles, short mipLevel = 0)
    {
        GorgonBox pixels = ToPixelBox(texels, mipLevel);

        tiles = GorgonBox.FromLTFRBB((int)((double)pixels.Left / _tileShape.WidthInTexels).FastFloor(),
                                    (int)((double)pixels.Top / _tileShape.HeightInTexels).FastFloor(),
                                    (int)((double)pixels.Front / _tileShape.DepthInTexels).FastFloor(),
                                    (int)((double)pixels.Right / _tileShape.WidthInTexels).FastCeiling(),
                                    (int)((double)pixels.Bottom / _tileShape.HeightInTexels).FastCeiling(),
                                    (int)((double)pixels.Back / _tileShape.DepthInTexels).FastCeiling());
    }

    /// <summary>
    /// Function to convert a 3D region of texel coordinates into a 3D region of tile coordinates.
    /// </summary>
    /// <param name="texels">The 3D texel region to convert.</param>
    /// <param name="mipLevel"><inheritdoc cref="ToTile(Vector2, short)" path="/param[@name='mipLevel']"/></param>
    /// <inheritdoc cref="ToTile(Vector2, short)" path="/returns"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonBox ToTiles(GorgonBoxF texels, short mipLevel = 0)
    {
        ToTiles(in texels, out GorgonBox tiles, mipLevel);
        return tiles;
    }

    /// <summary>
    /// Function to convert a 3D tile coordinate into a 3D texel coordinate.
    /// </summary>
    /// <param name="tile">The 3D tile coordinate to convert.</param>
    /// <param name="mipLevel"><inheritdoc cref="ToTile(Vector2, short)" path="/param[@name='mipLevel']"/></param>
    /// <inheritdoc cref="ToTexelOrigin(GorgonPoint, short)" path="/returns"/>
    /// <remarks>
    /// <para>
    /// Due to the imprecision of a tile, this method will only return the texel coordinate of the upper left, front corner of the tile.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ToTexelOrigin(Vector3 tile, short mipLevel = 0)
    {
        Vector3 pixels = new(tile.X * (int)_tileShape.WidthInTexels, tile.Y * (int)_tileShape.HeightInTexels, tile.Z * (int)_tileShape.DepthInTexels);

        return ToTexel(pixels, mipLevel);
    }

    /// <summary>
    /// Function to convert a 3D region of tile coordinates into a region of 3D texel coordinates.
    /// </summary>
    /// <param name="tiles">The 3D tile region to convert.</param>
    /// <param name="texels">The texel values for the region.</param>
    /// <param name="mipLevel"><inheritdoc cref="ToTile(Vector2, short)" path="/param[@name='mipLevel']"/></param>
    /// <inheritdoc cref="ToTexelOrigin(GorgonPoint, short)" path="/returns"/>
    /// <inheritdoc cref="ToTexelOrigin(Vector3, short)" path="/remarks"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FromTiles(ref readonly GorgonBox tiles, out GorgonBoxF texels, short mipLevel = 0)
    {
        GorgonBox pixels = GorgonBox.FromLTFRBB(tiles.Left * (int)_tileShape.WidthInTexels,
                                                tiles.Top * (int)_tileShape.HeightInTexels,
                                                tiles.Front * (int)_tileShape.DepthInTexels,
                                                tiles.Right * (int)_tileShape.WidthInTexels,
                                                tiles.Bottom * (int)_tileShape.HeightInTexels,
                                                tiles.Back * (int)_tileShape.DepthInTexels);

        texels = ToTexelBox(pixels, mipLevel);
    }

    /// <summary>
    /// Function to convert a 3D region of tile coordinates into a region of 3D texel coordinates.
    /// </summary>
    /// <param name="tiles">The 3D tile region to convert.</param>
    /// <param name="mipLevel"><inheritdoc cref="ToTile(Vector2, short)" path="/param[@name='mipLevel']"/></param>
    /// <inheritdoc cref="ToTexelOrigin(GorgonPoint, short)" path="/returns"/>
    /// <inheritdoc cref="ToTexelOrigin(Vector3, short)" path="/remarks"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonBoxF FromTiles(GorgonBox tiles, short mipLevel = 0) 
    {
        FromTiles(in tiles, out GorgonBoxF texels, mipLevel);
        return texels;
    }

    /// <summary>
    /// Function to calculate the sub resource index for tiled sub resources.
    /// </summary>
    /// <param name="mipLevel">The mip level to look up.</param>
    /// <param name="arrayIndex">The array index to look up (for <see cref="TextureType.Texture2D"/> textures).</param>
    /// <returns>The sub resource tile index.</returns>
    /// <remarks>
    /// <para>
    /// Unlike the <see cref="GorgonTextureCommon.GetSubResourceIndex(short, short, byte)"/> method, this will return the sub resource index based on which tile the sub resource resides in. This means that 
    /// a sub resource can share a tile if it is small enough (e.g. multiple small mip maps in a single tile). Therefore, the index returned by this method is not the same as the aforementioned method, and 
    /// should not be used with <see cref="GorgonTextureCommon.SubResources"/>. Instead, use the index with the <see cref="SubResourceTileInformation"/> property instead.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonTextureCommon"/>
    /// <seealso cref="SubResourceTileInformation"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetTileSubResourceIndex(short mipLevel, short arrayIndex)
    {
        short mipsPerArray = (short)(_packedMipStartIndex.Min((short)(MipCount - 1)) + 1);
        mipLevel = mipLevel.Max(0).Min((short)(mipsPerArray - 1).Max(0));
        arrayIndex = arrayIndex.Max(0).Min((short)(Type == TextureType.Texture3D ? 0 : (ArrayCount - 1)));

        return arrayIndex * mipsPerArray + mipLevel;
    }

    /// <summary>
    /// Function to determine if a texel region on a sub resource has been allocated already.
    /// </summary>
    /// <param name="texels">The texel region to evaluate.</param>
    /// <param name="mipLevel">[Optional] The mip map level to evaluate.</param>
    /// <param name="arrayIndex">[Optional] The array index to evaluate.</param>
    /// <returns><b>true</b> if the region intersects or contains a previously allocated region, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// Users should use this method to determine if a region on the texture has already been allocated for use. The <paramref name="texels"/> region is intersection tested against other allocated regions 
    /// in the same sub resource. 
    /// </para>
    /// </remarks>
    /// <seealso cref="TryAllocate"/>    
    public bool IsAllocated(ref readonly GorgonBoxF texels, short mipLevel = 0, short arrayIndex = 0)
    {
        using (_allocLock.EnterScope())
        {
            short mipsPerArray = (short)(_packedMipStartIndex.Min((short)(MipCount - 1)) + 1);
            arrayIndex = arrayIndex.Max(0).Min((short)(Type == TextureType.Texture3D ? 0 : (ArrayCount - 1)));
            mipLevel = mipLevel.Max(0).Min((short)(mipsPerArray - 1).Max(0));
            int subResourceIndex = arrayIndex * mipsPerArray + mipLevel;

            float l = texels.Left.Max(0).Min(1.0f);
            float t = texels.Top.Max(0).Min(1.0f);
            float f = Type != TextureType.Texture3D ? 0 : texels.Front.Max(0).Min(1.0f);

            float r = texels.Right.Max(l).Min(1.0f);
            float b = texels.Bottom.Max(t).Min(1.0f);
            float d = Type != TextureType.Texture3D ? 1 : texels.Back.Max(f).Min(1.0f);

            GorgonBoxF texelBox = GorgonBoxF.FromLTFRBB(l, t, f, r, b, d);

            if (texelBox.IsEmpty)
            {
                return false;
            }

            ToTiles(in texelBox, out GorgonBox tileBox, mipLevel);
            GorgonSubResourceTileInfo info = SubResourceTileInformation[subResourceIndex];

            // We have a packed mipmap tile here, so we need to change how it's represented by covering the tiles used for the packed mip.
            if (info.MipCount > 1)
            {
                tileBox = new GorgonBox(0, 0, 0, info.Width, 1, 1);
            }

            return HasAllocationBitmap(in tileBox, subResourceIndex, info);
        }
    }

    /// <summary>
    /// Function to free all allocations from the virtual texture.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Users can use this to remove all texture allocations from the virtual texture. Doing so will make the entire texture available for use.
    /// </para>
    /// <para>
    /// Unlike the <see cref="TryDeallocate"/> method, this method immediately frees the memory for the texture by waiting for the GPU to finish its workload. This may impact performance if called on a hot path.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// Any handles allocated from this texture will be invalid after this method has finished execution.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="TryDeallocate"/>
    public void FreeAll()
    {
        using (_allocLock.EnterScope())
        {
            foreach (ulong key in _textureAllocations.Keys)
            {
                Graphics.Memory.TextureTilePool.Free(key);
            }

            _textureAllocations.Clear();

            for (int i = 0; i < _allocationMasks.Length; ++i)
            {
                if (_allocationMasks[i] is not null)
                {
                    Array.Clear(_allocationMasks[i]);
                }
            }           

            // Wait for the GPU to finish whatever it's doing so we can actually free all the tiles.
            Graphics.WaitForGpu();
            Graphics.Memory.TextureTilePool.Signal();
        }
    }

    /// <summary>
    /// Function to try and deallocate a virtual texture handle.
    /// </summary>
    /// <param name="handle">The handle to deallocate.</param>
    /// <returns><b>true</b> if the deallocation succeeded, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// This method schedules an allocation for deletion. When the deallocation is scheduled, the resource data will be freed when the GPU is no longer using it. If the <paramref name="handle"/> is no longer 
    /// valid, then this method will return <b>false</b> and the user may handle the situation however they please.
    /// </para>
    /// <para>
    /// To remove all allocated texture memory in a single call, use the <see cref="FreeAll"/> method.
    /// </para>
    /// <para>
    /// This method will set the <paramref name="handle"/> parameter to -1 (equivalent to <see cref="ulong.MaxValue"/>) to indicate an invalid handle when the deallocation call is successful.
    /// </para>
    /// </remarks>
    public bool TryDeallocate(ref long handle)
    {
        using (_allocLock.EnterScope())
        {
            static ulong Unset(ulong value, ulong bitmap) => value & ~bitmap;

            ulong textureHandle = (ulong)handle;

            if ((textureHandle == ulong.MaxValue) || (!_textureAllocations.Remove(textureHandle, out VirtualTextureAllocation allocation)))
            {
                return false;
            }

            Graphics.Memory.TextureTilePool.Free(textureHandle);

            int subTileIndex = GetTileSubResourceIndex(allocation.MipLevel, allocation.ArrayIndex);
            GorgonSubResourceTileInfo subInfo = SubResourceTileInformation[subTileIndex];

            SetAllocationBitmap(in allocation.TileRegion, subTileIndex, subInfo, Unset);

            handle = -1;

            return true;
        }
    }

    /// <summary>
    /// Function to try and allocate a region on a sub resource in this virtual texture for use.
    /// </summary>
    /// <param name="texels">The region of texels on the texture to allocate.</param>
    /// <param name="handle">The handle produced by the allocation.</param>
    /// <param name="mipLevel">[Optional] The mip level sub resource to use.</param>
    /// <param name="arrayIndex">[Optional] The index of the array sub resource to use (for <see cref="TextureType.Texture2D"/> textures).</param>
    /// <returns><b>true</b> if the allocation was successful, <b>false</b> if not.</returns>
    /// <exception cref="GorgonException">Thrown if the <paramref name="texels"/> results in an empty region.</exception>
    /// <remarks>
    /// <para>
    /// This method allocates a region on the texture for use by an application. The application specifies the region in normalized texel coordinates (i.e. UV for <see cref="TextureType.Texture2D"/> textures, 
    /// or UVW for <see cref="TextureType.Texture3D"/> textures). For example, a region of (0.5f, 0.25f, 0) - (1.0f, 0.5f, 0.25f) indicates that the region should start half way through the texture, a 
    /// quarter of the way down, and at the first depth slice and should cover half of the width of the texture (i.e. right - left = 0.5f), one quarter of the height, and one quarter of its depth. This can 
    /// be applied to any sub resource on the texture like the <paramref name="mipLevel"/>, or, for <see cref="TextureType.Texture2D"/> textures, the <paramref name="arrayIndex"/> (this parameter is ignored 
    /// for <see cref="TextureType.Texture3D"/> textures).
    /// </para>
    /// <inheritdoc cref="GorgonVirtualTexture" path="/remarks/para[@type='overlap_warn']"/>
    /// <para>
    /// As mentioned in the above warning, allocation fails if there is overlap with another region. When this happens, the application returns <b>false</b> and sets the <paramref name="handle"/> to -1 (the 
    /// equivalent of <see cref="ulong.MaxValue"/>). Developers should check the return value of this method and handle it appropriately.
    /// </para>
    /// <para>
    /// If a region is no longer required, then the application should call <see cref="TryDeallocate"/> to free it up for future usage.
    /// </para>
    /// <para>
    /// The <paramref name="texels"/> parameter is clamped to 0 and 1.0f respectively for their minimum and maximum values. If this produces an empty box, then an exception is thrown. 
    /// </para>
    /// <para>
    /// The <paramref name="mipLevel"/>, and <paramref name="arrayIndex"/> are clamped to 0 for the minimum values. And likewise, clamped to <see cref="GorgonTextureCommon.MipCount"/><c>-1</c>, and 
    /// <see cref="GorgonTextureCommon.ArrayCount"/><c>-1</c> for the maximum values.
    /// </para>
    /// </remarks>
    /// <seealso cref="TryDeallocate"/>
    public bool TryAllocate(ref readonly GorgonBoxF texels, out long handle, short mipLevel = 0, short arrayIndex = 0)
    {
        static ulong SetBits(ulong value, ulong bits) => value | bits;

        using (_allocLock.EnterScope())
        {
            mipLevel = mipLevel.Max(0).Min((short)(MipCount - 1));
            short mipsPerArray = (short)(_packedMipStartIndex.Min((short)(MipCount - 1)) + 1);
            short tileMipLevel = mipLevel.Max(0).Min((short)(mipsPerArray - 1).Max(0));
            arrayIndex = arrayIndex.Max(0).Min((short)(Type == TextureType.Texture3D ? 0 : (ArrayCount - 1)));
            int subResourceIndex = arrayIndex * mipsPerArray + tileMipLevel;
            

            float l = texels.Left.Max(0).Min(1.0f);
            float t = texels.Top.Max(0).Min(1.0f);
            float f = Type != TextureType.Texture3D ? 0 : texels.Front.Max(0).Min(1.0f);

            float r = texels.Right.Max(l).Min(1.0f);
            float b = texels.Bottom.Max(t).Min(1.0f);
            float d = Type != TextureType.Texture3D ? 1 : texels.Back.Max(f).Min(1.0f);

            GorgonBoxF texelBox = GorgonBoxF.FromLTFRBB(l, t, f, r, b, d);

            if (texelBox.IsEmpty)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_VIRTUAL_TEXTURE_ALLOCATE_INVALID_REGION, Name));
            }

            ToTiles(in texelBox, out GorgonBox tileBox, mipLevel);
            GorgonSubResourceTileInfo info = SubResourceTileInformation[subResourceIndex];

            // We have a packed mipmap tile here, so we need to change how it's represented by covering the tiles used for the packed mip.
            if (info.MipCount > 1)
            {
                tileBox = new GorgonBox(0, 0, 0, info.Width, 1, 1);
            }

            if (HasAllocationBitmap(in tileBox, subResourceIndex, info))
            {
                handle = -1;
                return false;
            }

            uint tileCount = (uint)tileBox.Width * (uint)tileBox.Height * (uint)tileBox.Depth;

            Debug.Assert(tileCount != 0, "0 tiles to allocate!");
            
            ulong actualHandle = Graphics.Memory.TextureTilePool.Allocate(in D3DResource, tileCount, arrayIndex * MipCount + tileMipLevel, in tileBox);

            _textureAllocations[actualHandle] = new VirtualTextureAllocation(tileBox, mipLevel, arrayIndex);

            SetAllocationBitmap(in tileBox, subResourceIndex, info, SetBits);

            handle = (long)actualHandle;

            return true;
        }
    }

    /// <summary>
    /// Function to try and retrieve the tile region for the specified allocation handle.
    /// </summary>
    /// <param name="handle">The allocation handle to evaluate.</param>
    /// <param name="tiles">The tiles that are allocated to that handle.</param>
    /// <returns><b>true</b> if the call succeeded, <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// If the method is given an invalid <paramref name="handle"/>, then the <paramref name="tiles"/> is set to <see cref="GorgonBox.Empty"/> and the method will return <b>false</b>.
    /// </para>
    /// </remarks>
    public bool TryGetAllocatedTileRegion(long handle, out GorgonBox tiles)
    {
        if (!_textureAllocations.TryGetValue((ulong)handle, out VirtualTextureAllocation allocation))
        {
            tiles = GorgonBox.Empty;
            return false;
        }

        tiles = allocation.TileRegion;
        return true;
    }

    /// <summary>
    /// Function to try and retrieve the mip level and array index for the specified allocation handle.
    /// </summary>
    /// <param name="handle">The allocation handle to evaluate.</param>
    /// <param name="mipLevel">The mip map level for the allocation.</param>
    /// <param name="arrayIndex">The array index for the allocation.</param>
    /// <inheritdoc cref="TryGetAllocatedTileRegion(long, out GorgonBox)" path="/returns"/>
    /// <remarks>
    /// <para>
    /// If the method is given an invalid <paramref name="handle"/>, then the <paramref name="mipLevel"/> and <paramref name="arrayIndex"/> are set to -1 and the method will return <b>false</b>.
    /// </para>
    /// </remarks>
    public bool TryGetSubResources(long handle, out short mipLevel, out short arrayIndex)
    {
        if (!_textureAllocations.TryGetValue((ulong)handle, out VirtualTextureAllocation allocation))
        {
            mipLevel = arrayIndex = -1;
            return false;
        }

        mipLevel = allocation.MipLevel;
        arrayIndex = allocation.ArrayIndex;
        return true;
    }

    /// <summary>
    /// <inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/summary"/>
    /// </summary>
    /// <param name="format"><inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='format']"/></param>
    /// <param name="mipLevel"><inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='mipLevel']"/></param>
    /// <param name="mipCount"><inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='mipCount']"/></param>
    /// <param name="arrayIndex"><inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='arrayIndex']"/></param>
    /// <param name="arrayCount"><inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='arrayCount']"/></param>
    /// <param name="resourceMinLodClamp"><inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='resourceMinLodClamp']"/></param>
    /// <param name="planeIndex"><inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/param[@name='planeIndex']"/></param>
    /// <inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/returns"/>
    /// <inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/exception"/>
    /// <inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/remarks"/>
    /// <inheritdoc cref="GorgonTextureCommon.GetTextureView(BufferFormat, short, short, short, short, float, byte, bool)" path="/seealso"/>
    public IGorgonTextureView<GorgonVirtualTexture> GetTextureView(BufferFormat format = BufferFormat.Unknown, short mipLevel = 0, short mipCount = 0, short arrayIndex = 0, short arrayCount = 0, float resourceMinLodClamp = 0, byte planeIndex = 0) =>
        GetTextureView<GorgonVirtualTexture>(format, mipLevel, mipCount, arrayIndex, arrayCount, resourceMinLodClamp, planeIndex, false);

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~GorgonVirtualTexture() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonVirtualTexture"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonTextureCommon(GorgonGraphics, string, GorgonTextureInfo)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonTextureCommon(GorgonGraphics, string, GorgonTextureInfo)" path="/param[@name='name']"/></param>
    /// <param name="info"><inheritdoc cref="GorgonTextureCommon(GorgonGraphics, string, GorgonTextureInfo)" path="/param[@name='info']"/></param>
    /// <exception cref="GorgonException">
    /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[1]"/>
    /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[2]"/>
    /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[3]"/>
    /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[6]"/>
    /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[9]"/>
    /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[10]"/>
    /// <inheritdoc cref="ValidateInfo(GorgonTextureInfo)" path="/exception/para[11]"/>
    /// <inheritdoc cref="ValidateInfo(GorgonTextureInfo)" path="/exception/para[12]"/>
    /// </exception>
    /// <remarks>
    /// <para>
    /// When applications create a texture, they have to pass in a <see cref="GorgonVirtualTextureInfo"/> object to define the layout of the virtual texture. Applications use this to define the number of 
    /// dimensions in the texture (<see cref="TextureType"/>) and its format (<see cref="BufferFormat"/>).
    /// </para>
    /// <para>
    /// The following is a list of dimensions and their required values:
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="TextureType.Texture1D"/></term>
    ///         <description>Not supported by virtual textures..</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TextureType.Texture2D"/></term>
    ///         <description><see cref="GorgonTextureInfo.Width"/> and <see cref="GorgonTextureInfo.Height"/> are required, <see cref="GorgonTextureInfo.Depth"/> should be set to 1.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TextureType.Texture3D"/></term>
    ///         <description><see cref="GorgonTextureInfo.Width"/>, <see cref="GorgonTextureInfo.Height"/>, and <see cref="GorgonTextureInfo.Depth"/> are required.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// <para>
    /// The <see cref="GorgonTextureInfo.Format"/> should be a supported format for virtual textures. This can be determined by checking the <see cref="GorgonGraphics.FormatSupport"/> property on the 
    /// <see cref="GorgonGraphics"/> object to determine if the format is supported for a given texture type.
    /// </para>
    /// <para>
    /// The <see cref="GorgonTextureInfo.ArrayCount"/> should be set to 1 for <see cref="TextureType.Texture3D"/> textures. It only applies to 2D textures. If the <see cref="GorgonTextureInfo.IsCube"/> is 
    /// set to <b>true</b>, then this value <b>must</b> be a multiple of 6.
    /// </para>
    /// <para>
    /// The <see cref="GorgonTextureInfo.MipCount"/> value should be at least 1. <see cref="GorgonVirtualTextureInfo.GetMaximumMipCount(int, int, short)"/> can be used to determine the maximum number of mip 
    /// levels for the texture.
    /// </para>
    /// <inheritdoc cref="GorgonTexture" path="/remarks/para[@type='max_dimensions']"/>
    /// <para>
    /// A virtual texture must be no less than 4 MB (4,194,304 bytes) in size. Virtual textures are meant to store large amounts of texture data, and the minimum allocated physical size of a virtual texture is 
    /// 4 MB, which is large enough to hold a single array and mip level for a 1024x1024 32 bit texture. If the texture size is not large enough, then an exception will be thrown when one is created. To 
    /// compute the size of a texture, users may take the requested <c>width x <see cref="GorgonFormatInfo.SizeInBytes">format size</see> x height x depth (3D only) x array count (2D only) x 1.34 (for a full 
    /// 2D mip chain, 1.14 for a full 3D mip chain, or x 1 for a single mip level)</c> to roughly determine the texture size. 
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonVirtualTextureInfo"/>
    public GorgonVirtualTexture(GorgonGraphics graphics, string name, GorgonVirtualTextureInfo info)
        : base(graphics, name, new GorgonTextureInfo(info))
    {
        CreateNative();
        GetReservedDetails();

        _allocationMasks = new ulong[SubResourceTileInformation.Count][];
    }
}