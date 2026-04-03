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
// Created: January 11, 2026 11:49:16 AM
//

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
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
/// A texture used to project an image onto a graphic primitive such as a triangle
/// </summary>
public sealed unsafe class GorgonTexture
    : GorgonGpuResource, IGorgonTextureInfo, IGorgonImageInfo
{
    /// <summary>
    /// A unique key for a view.
    /// </summary>
    /// <param name="Format">The format of the view.</param>
    /// <param name="Value1">The first key value.</param>
    /// <param name="Value2">The second key value.</param>
    /// <param name="Value3">The third key value.</param>
    private readonly record struct RtViewKey(BufferFormat Format, short Value1, short Value2, short Value3);

    /// <summary>
    /// A unique key for a view.
    /// </summary>
    /// <param name="Format">The format of the view.</param>
    /// <param name="Value1">The first key value.</param>
    /// <param name="Value2">The second key value.</param>
    /// <param name="Value3">The third key value.</param>
    /// <param name="Value4">The fourth key value.</param>
    /// <param name="Value5">The fifth key value.</param>
    private readonly record struct SrViewKey(BufferFormat Format, short Value1, short Value2, short Value3, short Value4, int Value5);

    private ComPtr<D3D12MA_Allocation> _resourceAllocation;

    private readonly Lock _viewLock = new();
    private readonly Dictionary<RtViewKey, GorgonTextureRenderTargetView> _rtvs = [];
    private readonly Dictionary<RtViewKey, GorgonResourceView> _dsvs = [];
    private readonly Dictionary<SrViewKey, GorgonTextureView> _srvs = [];
    private readonly Dictionary<RtViewKey, GorgonResourceView> _uavs = [];
    private readonly GorgonTextureInfo _info;
    private GorgonSubResourceInfoList _subResourceInfo = GorgonSubResourceInfoList.Empty;

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
    public bool IsUnorderedAccess => _info.IsUnorderedAccess;

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
    public GorgonSubResourceInfoList SubResources => _subResourceInfo;

    /// <summary>
    /// Function to populate the sub resource information for the texture.
    /// </summary>
    /// <param name="desc">The resource description.</param>
    /// <returns>The list of sub resources for the texture.</returns>
    private List<GorgonSubResourceInfo> PopulateSubResourceInfo(ref readonly D3D12_RESOURCE_DESC1 desc)
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

            return result;
        }
        finally
        {
            ArrayPool<uint>.Shared.Return(rows, true);
            ArrayPool<ulong>.Shared.Return(rowSizes, true);
            ArrayPool<D3D12_PLACED_SUBRESOURCE_FOOTPRINT>.Shared.Return(footPrints, true);
        }
    }

    /// <inheritdoc/>
    private protected sealed override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Remove all the child views.
            foreach (GorgonResourceView view in _rtvs.Values.Cast<GorgonResourceView>()
                                                            .Concat(_dsvs.Values.Cast<GorgonResourceView>())
                                                            .Concat(_srvs.Values.Cast<GorgonResourceView>())
                                                            .Concat(_uavs.Values.Cast<GorgonResourceView>())
                                                            .Where(v => !v.OwnsResource))
            {
                view.Dispose();
            }
                        
            _rtvs.Clear();
            _dsvs.Clear();
            _srvs.Clear();
            _uavs.Clear();

            this.UnregisterDisposable(Graphics);
        }

        _resourceAllocation.Dispose();
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    private protected override void OnGetResourceInfo(out GpuResourceInfo resourceInfo) 
    {
        D3D12_RESOURCE_DESC1 desc = D3DResource.Get()->GetDesc1();
        resourceInfo = GpuResourceInfo.FromD3D(in desc);
    }

    /// <inheritdoc/>
    private protected override ComPtr<ID3D12Resource2> OnCreateNative(out D3D12_RESOURCE_DESC1 desc)
    {
        ComPtr<ID3D12Resource2> result = default;
        ComPtr<D3D12MA_Allocation> resourcePtr = default;
        D3D12_RESOURCE_FLAGS flags = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE;

        if (!IsShaderResource)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_DENY_SHADER_RESOURCE;
        }

        if (IsUnorderedAccess)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
        }

        if (IsRenderTarget)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET;
        }

        if (IsDepthStencil)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL;
        }

        if ((Graphics.Adapter.HasTightAlignmentSupport) && (!IsRenderTarget) && (!IsDepthStencil))
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_USE_TIGHT_ALIGNMENT;
        }

        byte planeCount = Graphics.FormatSupport[Format].PlaneCount;

        desc = Type switch
        {
            TextureType.Texture1D => D3D12_RESOURCE_DESC1.Tex1D((DXGI_FORMAT)Format, (ulong)Width, (ushort)ArrayCount, (ushort)MipCount, flags),
            TextureType.Texture2D => D3D12_RESOURCE_DESC1.Tex2D((DXGI_FORMAT)Format, (ulong)Width, (uint)Height, (ushort)ArrayCount, (ushort)MipCount, (uint)MultisampleInfo.Count, (uint)MultisampleInfo.Quality, flags,
                                        alignment: (Graphics.Adapter.HasTightAlignmentSupport || MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling)) ? 0UL : D3D12.D3D12_DEFAULT_MSAA_RESOURCE_PLACEMENT_ALIGNMENT),
            TextureType.Texture3D => D3D12_RESOURCE_DESC1.Tex3D((DXGI_FORMAT)Format, (ulong)Width, (uint)Height, (ushort)Depth, (ushort)MipCount, flags),
            _ => throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_TEXTURE_UNKNOWN_TYPE, Type))
        };

        string textureName = $"D3D12 {Type} {Name}";
        
        Graphics.Log.Print($"Created D3D 12 {Type} resource object for '{Name}'.", LoggingLevel.Verbose);
    
        D3D12MA_ALLOCATION_DESC allocDesc = new(D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT);

        fixed (D3D12_RESOURCE_DESC1* descPtr = &desc)
        {
            Graphics.Allocator.Get()->CreateResource3(&allocDesc, descPtr,
                D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_UNDEFINED,
                null, 0, null,
                resourcePtr.GetAddressOf(), null, null)
                .ThrowIfFailed(GorgonResult.CannotCreate, () => Resources.GORGFX_ERR_CANNOT_CREATE_RESOURCE);

            _resourceAllocation = resourcePtr;

            using ComPtr<ID3D12Resource> newRes = new(_resourceAllocation.Get()->GetResource());
            newRes.As(ref result)
                .ThrowIfFailed(hr => throw new InvalidCastException());

            result.SetD3DDebugName(textureName);
        }

        _subResourceInfo = new GorgonSubResourceInfoList(this, PopulateSubResourceInfo(in desc));

        return result;
    }

    /// <summary>
    /// Function to create a 2D render target from a swap chain.
    /// </summary>
    /// <param name="swapChain">The swap chain to retrieve the render target view fromn.</param>
    /// <param name="index">The index of the back buffer for the render target view.</param>
    /// <returns>A new render target view.</returns>
    internal static GorgonTexture FromSwapChain(GorgonSwapChain swapChain, uint index)
    {        
        ComPtr<ID3D12Resource2> d3dResource = default;

        string backBufferName = $"{swapChain.Name} D3D 12 Backbuffer Resource #{index}";

        swapChain.Graphics.Log.Print($"Retrieving D3D 12 Resource (Swap chain back buffer): {backBufferName}.", LoggingLevel.Verbose);

        swapChain.DXGISwapChain.Get()->GetBuffer(index, Win32.__uuidof<ID3D12Resource2>(), (void**)d3dResource.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_RETRIEVE_BACKBUFFER, index, swapChain.Name));

        d3dResource.SetD3DDebugName(backBufferName);

        D3D12_RESOURCE_DESC1 desc = d3dResource.Get()->GetDesc1();
        GpuResourceInfo info = GpuResourceInfo.FromD3D(in desc);

        GorgonTexture resource = new(swapChain.Graphics, $"{swapChain.Name}: Render target texture #{index}", d3dResource, new GorgonTextureInfo(TextureType.Texture2D, info.Format)
        {
            ArrayCount = 1,
            Depth = 1,
            Height = info.Texture2D.Height,
            Width = info.Texture2D.Width,
            IsCube = false,
            IsDepthStencil = false,
            IsRenderTarget = true,
            IsShaderResource = (info.Usage & GraphicsResourceUsage.ShaderResource) == GraphicsResourceUsage.ShaderResource,
            IsUnorderedAccess = (info.Usage & GraphicsResourceUsage.UnorderedAccess) == GraphicsResourceUsage.UnorderedAccess,
            MipCount = 1,
            MultisampleInfo = GorgonMultisampleInfo.NoMultisampling
        });

        if (resource.IsShaderResource)
        {
            resource.GetTextureView();
        }

        GorgonTextureRenderTargetView view = new(swapChain.Graphics, $"{backBufferName} render target view", resource, resource.Format, resource.FormatInfo, true);
        RtViewKey key = new(view.Format, 0, 0, 1);
        resource._rtvs[key] = view;

        return resource;
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
    public int GetMipHeight(short mipLevel) => Type == TextureType.Texture1D ? 1 :  Height >> mipLevel.Min((short)(MipCount - 1)).Max(0);

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
    public Vector2 ToTexel(GorgonPoint point, short mipLevel = 0) => new(point.X / (float)GetMipWidth(mipLevel),  Type == TextureType.Texture1D  ? 0.0f : point.Y / (float)GetMipHeight(mipLevel));

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
    public Vector3 ToPixel(Vector3 point, short mipLevel = 0) => new(point.X * GetMipWidth(mipLevel), Type == TextureType.Texture1D ? 0.0f : point.Y * GetMipHeight(mipLevel), Type == TextureType.Texture3D ? point.Z * GetMipDepth(mipLevel): 0.0f);

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
    /// TODO:
    /// </summary>
    /// <param name="format"></param>
    /// <param name="mipLevel"></param>
    /// <param name="arrayIndexOrDepthSlice"></param>
    /// <param name="arrayCountOrDepthCount"></param>
    /// <returns></returns>
    /// <exception cref="GorgonException"></exception>
#warning FINISHME: This is not complete, just here as a marker/template.  Needs validation and documentation.
    public GorgonTextureRenderTargetView GetRenderTargetView(BufferFormat format = BufferFormat.Unknown, short mipLevel = 0, short arrayIndexOrDepthSlice = 0, short arrayCountOrDepthCount = 1)
    {
        using (_viewLock.EnterScope())
        {
            GorgonFormatInfo formatInfo;

            if (format == BufferFormat.Unknown)
            {
                format = Format;
                formatInfo = FormatInfo;
            }
            else
            {
                formatInfo = new GorgonFormatInfo(format);
            }

            if (!Graphics.FormatSupport[format].IsRenderTargetFormat)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_RENDER_TARGET_FORMAT, format));
            }

            RtViewKey key = new(format, mipLevel, arrayIndexOrDepthSlice, arrayCountOrDepthCount);

            if ((_rtvs.TryGetValue(key, out GorgonTextureRenderTargetView? result))
                && (result.D3DCpuHandle != D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT))
            {
                return result;
            }

            return _rtvs[key] = new(Graphics, Name, this, format, formatInfo, false);
        }
    }

    /// <summary>
    /// TODO:
    /// </summary>
    /// <param name="format"></param>
    /// <param name="mipLevel"></param>
    /// <param name="mipCount"></param>
    /// <param name="arrayIndex"></param>
    /// <param name="arrayCount"></param>
    /// <param name="resourceMinLodClamp"></param>
    /// <returns></returns>
#warning FINISHME: This is not complete, just here as a marker/template.  Needs validation and documentation.
    public GorgonTextureView GetTextureView(BufferFormat format = BufferFormat.Unknown, short mipLevel = 0, short mipCount = 1, short arrayIndex = 0, short arrayCount = 1, float resourceMinLodClamp = 0)
    {
        using (_viewLock.EnterScope())
        {
            GorgonFormatInfo formatInfo;

            if (format == BufferFormat.Unknown)
            {
                format = Format;
                formatInfo = FormatInfo;
            }
            else
            {
                formatInfo = new GorgonFormatInfo(format);
            }

            int lodClamp = *((int*)(&resourceMinLodClamp));

            SrViewKey key = new(format, mipLevel, mipCount, arrayIndex, arrayCount, lodClamp);

            if ((_srvs.TryGetValue(key, out GorgonTextureView? result))
                && (result.D3DCpuHandle != D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT))
            {
                return result;
            }

            return _srvs[key] = new(Graphics, Name, this, format, arrayIndex, arrayCount, mipLevel, mipCount, resourceMinLodClamp, false);
        }
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
    /// Function to validate the information passed to the constructor.
    /// </summary>
    /// <param name="info">The texture creation information to validate.</param>
    /// <returns>The updated creation information if default values need changing, otherwise the <paramref name="info"/> parameter.</returns>
    /// <exception cref="GorgonException">Thrown when TODO</exception>
    private static GorgonTextureInfo ValidateInfo(GorgonTextureInfo info)
    {
        GorgonTextureInfo result = info;

        switch (info.Type)
        {
            case TextureType.Texture3D:
                if ((info.ArrayCount != 1) || (!info.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling)))
                {
                    result = info with
                    {
                        ArrayCount = 1,
                        MultisampleInfo = GorgonMultisampleInfo.NoMultisampling
                    };
                }
                break;
            case TextureType.Texture2D:
                if ((info.Depth != 1) || (!info.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling)))
                {
                    result = info with
                    {
                        Depth = 1,
                        MultisampleInfo = GorgonMultisampleInfo.NoMultisampling
                    };
                }
                break;
            case TextureType.Texture1D:
                if ((info.Depth != 1) || (!info.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling)))
                {
                    result = info with
                    {
                        Depth = 1,
                        MultisampleInfo = GorgonMultisampleInfo.NoMultisampling
                    };
                }
                break;
            default:
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_TEXTURE_UNKNOWN_TYPE, info.Type));
        }

        return result;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTexture"/> resource.
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
    internal GorgonTexture(GorgonGraphics graphics, string name, ComPtr<ID3D12Resource2> resource, GorgonTextureInfo info)
        : base(graphics, name, resource)
    {
        D3D12_RESOURCE_DESC1 desc = resource.Get()->GetDesc1();

        _info = new GorgonTextureInfo(info);
        FormatInfo = new GorgonFormatInfo(info.Format);

        _subResourceInfo = new GorgonSubResourceInfoList(this, PopulateSubResourceInfo(in desc));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTexture"/> resource.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='name']"/></param>
    /// <param name="info">Information used to create the texture.</param>
    public GorgonTexture(GorgonGraphics graphics, string name, GorgonTextureInfo info)
        : base(graphics, name)
    {
        _info = new GorgonTextureInfo(ValidateInfo(info));
        FormatInfo = new GorgonFormatInfo(info.Format);        

        CreateNative_OLDE();
        
        Graphics.InitializeTexture(this);

        if (IsShaderResource)
        {
            GetTextureView();
        }
    }
}
