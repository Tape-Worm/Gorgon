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
// Created: January 3, 2026 1:54:46 PM
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A view for a render target texture resource.
/// </summary>
/// <remarks>
/// <para>
/// TODO: Write something here.
/// </para>
/// </remarks>
public unsafe sealed class GorgonRenderTargetView
    : GorgonResourceView
{
    private CpuDescriptorAllocation _allocation;

    /// <summary>
    /// Property to return the format of view data.
    /// </summary>
    public BufferFormat Format
    {
        get;
    } = BufferFormat.Unknown;

    /// <summary>
    /// Property to return the format information for the <see cref="Format"/>.
    /// </summary>
    public GorgonFormatInfo FormatInfo
    {
        get;
    }

    /// <summary>
    /// Property to return the texture associated with this view.
    /// </summary>
    /// <remarks>
    /// This value is a strongly typed version of the <see cref="GorgonResourceView.Resource"/> property and point to the same object.
    /// </remarks>
    public GorgonTexture Texture
    {
        get;
    }

    /// <summary>
    /// Property to return the first depth slice of the view.
    /// </summary>
    /// <remarks>
    /// For views attached to a <see cref="GraphicsResourceType.Texture1D"/> or <see cref="GraphicsResourceType.Texture2D"/> resource, this value will always 
    /// return 0.
    /// </remarks>
    public short StartDepth
    {
        get;
    }

    /// <summary>
    /// Property to return the number of depth slices in the view.
    /// </summary>
    /// <remarks>
    /// This only applies to views attached to a <see cref="GraphicsResourceType.Texture3D"/> resource.
    /// </remarks>
    public short DepthCount
    {
        get;
    }

    /// <summary>
    /// Property to return the first map level in the view.
    /// </summary>
    public short MipLevel
    {
        get;
    }

    /// <summary>
    /// Property to return the first array index in the view.
    /// </summary>
    /// <remarks>
    /// This only applies to views attached to a <see cref="GraphicsResourceType.Texture1D"/> or <see cref="GraphicsResourceType.Texture2D"/> resource with multiple array indices. For all other types this 
    /// will return 0.
    /// </remarks>
    public short ArrayIndex
    {
        get;
    }

    /// <summary>
    /// Property to return the number of array indices in the view.
    /// </summary>
    /// <remarks>
    /// This only applies to views attached to a <see cref="GraphicsResourceType.Texture1D"/> or <see cref="GraphicsResourceType.Texture2D"/> resource with multiple array indices. For all other types this 
    /// will return 1.
    /// </remarks>
    public short ArrayCount
    {
        get;
    }

    /// <summary>
    /// Property to return the plane index in the view.
    /// </summary>
    /// <remarks>
    /// This value only applies to <see cref="TextureType.Texture2D"/> textures that have a multi-sample value of <see cref="GorgonMultisampleInfo.NoMultisampling"/>.
    /// </remarks>
    public byte PlaneIndex
    {
        get;
    }

    /// <summary>
    /// Function to update the description with 1D texture information.
    /// </summary>
    /// <param name="desc">The description to update.</param>
    /// <param name="hasArrays"><b>true</b> if the resource has array slices, <b>false</b> if not.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetTexture1DInfo(ref D3D12_RENDER_TARGET_VIEW_DESC desc, bool hasArrays)
    {
        if (hasArrays)
        {
            desc.Texture1DArray = new D3D12_TEX1D_ARRAY_RTV
            {
                ArraySize = (uint)ArrayCount,
                FirstArraySlice = (uint)ArrayIndex,
                MipSlice = (uint)MipLevel
            };
            return;
        }

        desc.Texture1D = new D3D12_TEX1D_RTV
        {
            MipSlice = (uint)MipLevel
        };
    }

    /// <summary>
    /// Function to update the description with 2D texture information.
    /// </summary>
    /// <param name="desc">The description to update.</param>
    /// <param name="hasMultiSample"><b>true</b> if the resource is multisampled, <b>false</b> if not.</param>
    /// <param name="hasArrays"><b>true</b> if the resource has array slices, <b>false</b> if not.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetTexture2DInfo(ref D3D12_RENDER_TARGET_VIEW_DESC desc, bool hasMultiSample, bool hasArrays)
    {
        if (hasMultiSample)
        {
            if (hasArrays)
            {
                desc.Texture2DMSArray = new D3D12_TEX2DMS_ARRAY_RTV
                {
                    ArraySize = (uint)ArrayCount,
                    FirstArraySlice = (uint)ArrayIndex                    
                };
            }

            return;
        }

        if (hasArrays)
        {
            desc.Texture2DArray = new D3D12_TEX2D_ARRAY_RTV
            {
                ArraySize = (uint)ArrayCount,
                FirstArraySlice = (uint)ArrayIndex,
                MipSlice = (uint)MipLevel,
                PlaneSlice = PlaneIndex                
            };
            return;
        }

        desc.Texture2D = new D3D12_TEX2D_RTV
        {
            MipSlice = (uint)MipLevel,
            PlaneSlice = PlaneIndex
        };
    }

    /// <summary>
    /// Function to update the description with 3D texture information.
    /// </summary>
    /// <param name="desc">The description to update.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetTexture3DInfo(ref D3D12_RENDER_TARGET_VIEW_DESC desc) => desc.Texture3D = new D3D12_TEX3D_RTV
    {
        MipSlice = (uint)MipLevel,
        FirstWSlice = (uint)StartDepth,
        WSize = (uint)DepthCount        
    };

    /// <summary>
    /// Function to allocate a view descriptor from the descriptor heap.
    /// </summary>
    private void AllocateDescriptors()
    {
        if (!_allocation.Equals(CpuDescriptorAllocation.Null))
        {
            _allocation.Heap?.Free(ref _allocation);
        }

        bool hasMultiSample = !Texture.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling);
        bool hasArrays = Texture.ArrayCount > 1;
        D3D12_RTV_DIMENSION dimension;

        if (hasMultiSample)
        {
            // We should not need to check for the texture type here, it should have been done at texture creation.
            dimension = hasArrays ? D3D12_RTV_DIMENSION.D3D12_RTV_DIMENSION_TEXTURE2DMSARRAY : D3D12_RTV_DIMENSION.D3D12_RTV_DIMENSION_TEXTURE2DMS;
        }
        else
        {
            dimension = Texture.Type switch
            {
                TextureType.Texture1D => hasArrays ? D3D12_RTV_DIMENSION.D3D12_RTV_DIMENSION_TEXTURE1DARRAY : D3D12_RTV_DIMENSION.D3D12_RTV_DIMENSION_TEXTURE1D,
                TextureType.Texture2D => hasArrays ? D3D12_RTV_DIMENSION.D3D12_RTV_DIMENSION_TEXTURE2DARRAY : D3D12_RTV_DIMENSION.D3D12_RTV_DIMENSION_TEXTURE2D,
                TextureType.Texture3D => D3D12_RTV_DIMENSION.D3D12_RTV_DIMENSION_TEXTURE3D,
                _ => throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_VIEW_UNKNOWN_TYPE, Texture.Type))
            };
        }

        D3D12_RENDER_TARGET_VIEW_DESC desc = new()
        {
            ViewDimension = dimension,
            Format = (DXGI_FORMAT)Format
        };

        switch (Texture.Type)
        {
            case TextureType.Texture1D:
                GetTexture1DInfo(ref desc, hasArrays);
                break;
            case TextureType.Texture2D:
                GetTexture2DInfo(ref desc, hasMultiSample, hasArrays);
                break;
            case TextureType.Texture3D:
                GetTexture3DInfo(ref desc);
                break;
        }

        Graphics.Log.Print($"Allocating CPU handle for {Name}.", LoggingLevel.Verbose);
        Graphics.RtvDescriptors.Allocate(1, out _allocation);
        Graphics.D3DDevice.Get()->CreateRenderTargetView((PID3D12Resource2)Resource.D3DResource.Get(), &desc, _allocation.CpuHandle);

        SetHandles(_allocation.CpuHandle, D3D12_GPU_DESCRIPTOR_HANDLE.DEFAULT);
    }

    /// <inheritdoc/>
    private protected sealed override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_allocation.Equals(CpuDescriptorAllocation.Null))
            {
                Graphics.Log.Print($"Freeing descriptor handle allocation for '{Name}'.", LoggingLevel.Verbose);
                CpuDescriptorHeap? heap = _allocation.Heap;
                heap?.Free(ref _allocation);
            }
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected override void OnReset() => AllocateDescriptors();

    /// <summary>
    /// Function to determine if the view settings are valid for a render target.
    /// </summary>
    /// <param name="name">The name of the render target texture.</param>
    /// <param name="formatSupport">The support for the format.</param>
    /// <param name="textureFormatInfo">The information about the texture format.</param>
    /// <param name="viewFormatInfo">The information about the view format.</param>
    /// <param name="formats">The list of formats that the texture format can cast to.</param>
    /// <param name="isRenderTarget"><b>true</b> if the texture is capable of being used as a render target, <b>false</b> if not.</param>
    /// <exception cref="GorgonException"><para>Thrown if the texture is not a <see cref="GorgonTexture.IsRenderTarget">render target</see></para>
    /// <para>Thrown if the format is a typeless format.</para>
    /// <para>Thrown if the format is not supported by render targets.</para>
    /// <para>Thrown if the texture format cannot be casted to the view format.</para>
    /// </exception>
    internal static void ValidateRenderTargetView(string name, GorgonBufferFormatSupport formatSupport, GorgonFormatInfo textureFormatInfo, GorgonFormatInfo viewFormatInfo, IReadOnlyList<BufferFormat> formats, bool isRenderTarget)
    {
        if (!isRenderTarget)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_RTV_NOT_RENDER_TARGET, name));
        }

        if (viewFormatInfo.IsTypeless)
        {
            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_TEXTURE_RTV_FORMAT_TYPELESS_NOT_SUPPORTED);
        }

        if (!formatSupport.IsRenderTargetFormat)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_RTV_NOT_RENDERTARGET_FORMAT, viewFormatInfo.Format, name));
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
    /// Initializes a new instance of the <see cref="GorgonTextureView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
    /// <param name="texture">The texture for the view.</param>
    /// <param name="formatInfo">Information about the view format.</param>
    /// <param name="mipLevel">The first mip level to view.</param>
    /// <param name="firstArrayOrDepth">The first array index or depth slice to view.</param>
    /// <param name="arrayOrDepthCount">The number of array indices/depth slices to view.</param>
    /// <param name="planeIndex">The plane index in the view.</param>
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    internal GorgonRenderTargetView(GorgonGraphics graphics, string name, GorgonTexture texture, GorgonFormatInfo formatInfo, short mipLevel, short firstArrayOrDepth, short arrayOrDepthCount, byte planeIndex, bool owned)
        : base(graphics, $"{GorgonGraphicsFactory.GenerateName(name, nameof(GorgonRenderTargetView))} - Render Target View", texture, owned)
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
