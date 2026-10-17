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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: October 6, 2026 9:00:00 AM
//

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides a read/write view of a <see cref="GorgonTexture"/> or <see cref="GorgonVirtualTexture"/>.
/// </summary>
/// <remarks>
/// <para>
/// A read/write view allows shaders to read from, and write to, a texture. The texture must be created with read/write access (see <see cref="IGorgonTextureInfo.HasReadWriteAccess"/>), otherwise the view 
/// cannot be created. Depth/stencil textures, and multisampled textures, cannot have read/write access.
/// </para>
/// <para>
/// The view format must not be a typeless format, and must be usable as a typed read/write view format. This can be determined by reading the <see cref="GorgonBufferFormatSupport.IsTypedReadWriteViewFormat"/> 
/// property on the <see cref="GorgonBufferFormatSupport"/> object returned by the <see cref="GorgonGraphics.FormatSupport"/> property.
/// </para>
/// <inheritdoc cref="GorgonGpuBuffer" path="/remarks/para[@type='uav_readwrite']"/>
/// </remarks>
/// <seealso cref="GorgonTextureCommon.GetTextureReadWriteView(BufferFormat, short, short, short, byte)"/>
/// <seealso cref="GetViewHandle"/>
public unsafe sealed class GorgonTextureRwView
    : GorgonResourceView
{
    private GpuDescriptorAllocation _allocation = GpuDescriptorAllocation.Null;
    private CpuDescriptorAllocation _cpuAllocation = CpuDescriptorAllocation.Null;

    /// <summary>
    /// Property to return the descriptor allocation for this view in the heap that is not visible to shaders.
    /// </summary>
    internal ref readonly CpuDescriptorAllocation CpuAllocation => ref _cpuAllocation;

    /// <inheritdoc cref="GorgonRenderTargetView.Format"/>
    public BufferFormat Format
    {
        get;
    }

    /// <inheritdoc cref="GorgonRenderTargetView.FormatInfo"/>
    public GorgonFormatInfo FormatInfo
    {
        get;
    }

    /// <inheritdoc cref="GorgonRenderTargetView.Texture"/>
    public GorgonTextureCommon Texture
    {
        get;
    }

    /// <inheritdoc cref="GorgonRenderTargetView.StartDepth"/>
    public short StartDepth
    {
        get;
    }

    /// <inheritdoc cref="GorgonRenderTargetView.DepthCount"/>
    public short DepthCount
    {
        get;
    }

    /// <summary>
    /// Property to return the mip level in the view.
    /// </summary>
    public short MipLevel
    {
        get;
    }

    /// <inheritdoc cref="GorgonRenderTargetView.ArrayIndex"/>
    public short ArrayIndex
    {
        get;
    }

    /// <inheritdoc cref="GorgonRenderTargetView.ArrayCount"/>
    public short ArrayCount
    {
        get;
    }

    /// <summary>
    /// Property to return the plane index in the view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value only applies to <see cref="TextureType.Texture2D"/> textures.
    /// </para>
    /// </remarks>
    public byte PlaneIndex
    {
        get;
    }

    /// <summary>
    /// Function to build the unordered access view description.
    /// </summary>
    /// <returns>The unordered access view description used to create the descriptor.</returns>
    private D3D12_UNORDERED_ACCESS_VIEW_DESC GetDesc()
    {
        bool hasArrays = Texture.ArrayCount > 1;
        D3D12_UNORDERED_ACCESS_VIEW_DESC desc = new()
        {
            Format = (DXGI_FORMAT)Format
        };

        switch (Texture.Type)
        {
            case TextureType.Texture1D when hasArrays:
                desc.ViewDimension = D3D12_UAV_DIMENSION.D3D12_UAV_DIMENSION_TEXTURE1DARRAY;
                desc.Texture1DArray = new D3D12_TEX1D_ARRAY_UAV
                {
                    MipSlice = (uint)MipLevel,
                    FirstArraySlice = (uint)ArrayIndex,
                    ArraySize = (uint)ArrayCount
                };
                break;
            case TextureType.Texture1D:
                desc.ViewDimension = D3D12_UAV_DIMENSION.D3D12_UAV_DIMENSION_TEXTURE1D;
                desc.Texture1D = new D3D12_TEX1D_UAV
                {
                    MipSlice = (uint)MipLevel
                };
                break;
            case TextureType.Texture2D when hasArrays:
                desc.ViewDimension = D3D12_UAV_DIMENSION.D3D12_UAV_DIMENSION_TEXTURE2DARRAY;
                desc.Texture2DArray = new D3D12_TEX2D_ARRAY_UAV
                {
                    MipSlice = (uint)MipLevel,
                    FirstArraySlice = (uint)ArrayIndex,
                    ArraySize = (uint)ArrayCount,
                    PlaneSlice = PlaneIndex
                };
                break;
            case TextureType.Texture2D:
                desc.ViewDimension = D3D12_UAV_DIMENSION.D3D12_UAV_DIMENSION_TEXTURE2D;
                desc.Texture2D = new D3D12_TEX2D_UAV
                {
                    MipSlice = (uint)MipLevel,
                    PlaneSlice = PlaneIndex
                };
                break;
            case TextureType.Texture3D:
                desc.ViewDimension = D3D12_UAV_DIMENSION.D3D12_UAV_DIMENSION_TEXTURE3D;
                desc.Texture3D = new D3D12_TEX3D_UAV
                {
                    MipSlice = (uint)MipLevel,
                    FirstWSlice = (uint)StartDepth,
                    WSize = (uint)DepthCount
                };
                break;
            default:
                throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_VIEW_UNKNOWN_TYPE, Texture.Type));
        }

        return desc;
    }

    /// <summary>
    /// Function to allocate the view descriptors.
    /// </summary>
    private void AllocateDescriptors()
    {
        ReadWriteViewDescriptors.Allocate(Graphics, (PID3D12Resource2)Texture.D3DResource.Get(), null, GetDesc(), ref _cpuAllocation, ref _allocation);

        D3DCpuHandle = _cpuAllocation.CpuHandle;
    }

    /// <inheritdoc/>
    private protected sealed override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.UnregisterDisposable(Graphics);

            Graphics.Log.Print($"Freeing descriptor handle allocations for '{Name}'.", LoggingLevel.Verbose);
            ReadWriteViewDescriptors.Free(Graphics, ref _cpuAllocation, ref _allocation);
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc cref="GorgonConstantBufferView.GetViewHandle()"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetViewHandle()
    {
        if (_allocation.Equals(in GpuDescriptorAllocation.Null))
        {
            AllocateDescriptors();
        }

        return _allocation.Offset;
    }

    /// <summary>
    /// Function to determine if the view settings are valid for a read/write view.
    /// </summary>
    /// <param name="name">The name of the texture.</param>
    /// <param name="formatSupport">The support for the view format.</param>
    /// <param name="textureFormatInfo">The information about the texture format.</param>
    /// <param name="viewFormatInfo">The information about the view format.</param>
    /// <param name="formats">The list of formats that the texture format can cast to.</param>
    /// <param name="isRwResource"><b>true</b> if the texture was created with read/write access, <b>false</b> if not.</param>
    /// <exception cref="GorgonException"><para type="norw">
    /// Thrown if the texture was not created with <see cref="GorgonTextureCommon.HasReadWriteAccess">read/write access</see>.
    /// </para>
    /// <para type="typeless">
    /// Thrown if the format is a typeless format.
    /// </para>
    /// <para type="format">
    /// Thrown if the format is not supported by read/write views.
    /// </para>
    /// <para type="cast">
    /// Thrown if the texture format cannot be cast to the view format.
    /// </para>
    /// </exception>
    internal static void ValidateReadWriteView(string name, GorgonBufferFormatSupport formatSupport, GorgonFormatInfo textureFormatInfo, GorgonFormatInfo viewFormatInfo, IReadOnlyList<BufferFormat> formats, bool isRwResource)
    {
        if (!isRwResource)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_RESOURCE_NO_READ_WRITE_ACCESS, name));
        }

        if (viewFormatInfo.IsTypeless)
        {
            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_TEXTURE_RWVIEW_FORMAT_TYPELESS_NOT_SUPPORTED);
        }

        if (!formatSupport.IsTypedReadWriteViewFormat)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_RWVIEW_NOT_RWVIEW_FORMAT, viewFormatInfo.Format, name));
        }

        if ((textureFormatInfo.Format != viewFormatInfo.Format) && ((formats.Count == 0) || (!formats.Contains(viewFormatInfo.Format))))
        {
            // This is a fallback to the old way of casting. If we have a castable format that isn't part of the same group,
            // then we should not test this.
            if (textureFormatInfo.Group != viewFormatInfo.Group)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_CANNOT_BE_CAST, name, textureFormatInfo.Format, viewFormatInfo.Format));
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTextureRwView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
    /// <param name="texture">The texture for the view.</param>
    /// <param name="formatInfo">Information about the view format.</param>
    /// <param name="mipLevel">The mip level to view.</param>
    /// <param name="firstArrayOrDepth">The first array index or depth slice to view.</param>
    /// <param name="arrayOrDepthCount">The number of array indices/depth slices to view.</param>
    /// <param name="planeIndex">The plane index in the view.</param>
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    internal GorgonTextureRwView(GorgonGraphics graphics, string name, GorgonTextureCommon texture, GorgonFormatInfo formatInfo, short mipLevel, short firstArrayOrDepth, short arrayOrDepthCount, byte planeIndex, bool owned)
        : base(graphics, $"{GorgonGraphicsFactory.GenerateName(name, nameof(GorgonTextureRwView))} - Texture Read Write (UAV) View", texture, owned)
    {
        Texture = texture;
        Format = formatInfo.Format;
        FormatInfo = formatInfo;
        MipLevel = mipLevel;
        ArrayIndex = texture.Type != TextureType.Texture3D ? firstArrayOrDepth : (short)0;
        ArrayCount = texture.Type != TextureType.Texture3D ? arrayOrDepthCount : (short)1;
        StartDepth = texture.Type == TextureType.Texture3D ? firstArrayOrDepth : (short)0;
        DepthCount = texture.Type == TextureType.Texture3D ? arrayOrDepthCount : (short)1;
        PlaneIndex = planeIndex;

        AllocateDescriptors();
    }
}
