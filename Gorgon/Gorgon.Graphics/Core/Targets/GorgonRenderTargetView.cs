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
    : GorgonResourceView, IGorgonImageInfo
{
    private CpuDescriptorAllocation _allocation;
    private readonly IGorgonImageInfo _textureImageInfo;

    /// <summary>
    /// Property to return the format of view data.
    /// </summary>
    public BufferFormat Format
    {
        get;
    } = BufferFormat.Unknown;

    /// <summary>
    /// Property to return the information about the buffer <see cref="Format"/>.
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
    } = 1;

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
    } = 1;

    /// <inheritdoc/>
    ImageDataType IGorgonImageInfo.ImageType => _textureImageInfo.ImageType;

    /// <inheritdoc/>
    int IGorgonImageInfo.Width => Texture.GetMipWidth(MipLevel);

    /// <inheritdoc/>
    int IGorgonImageInfo.Height => Texture.GetMipHeight(MipLevel);

    /// <inheritdoc/>
    int IGorgonImageInfo.Depth => Texture.GetMipDepth(MipLevel);

    /// <inheritdoc/>
    BufferFormat IGorgonImageInfo.Format => Format;

    /// <inheritdoc/>
    bool IGorgonImageInfo.HasPremultipliedAlpha => _textureImageInfo.HasPremultipliedAlpha;

    /// <inheritdoc/>
    int IGorgonImageInfo.MipCount => 1;

    /// <inheritdoc/>
    bool IGorgonImageInfo.IsPowerOfTwo => _textureImageInfo.IsPowerOfTwo;

    /// <inheritdoc/>
    int IGorgonImageInfo.ArrayCount => ArrayCount;

    /// <inheritdoc/>
    private protected sealed override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Graphics.Log.Print($"Destroying view '{Name}'...", LoggingLevel.Simple);

            CpuDescriptorAllocation allocation = _allocation;
            _allocation = CpuDescriptorAllocation.Null;

            if (!allocation.Equals(in CpuDescriptorAllocation.Null))
            {
                CpuDescriptorHeap? heap = allocation.Heap;
                Graphics.Log.Print($"Freeing CPU handle allocation for {Name}.", LoggingLevel.Verbose);
                heap?.Free(ref allocation);
            }

            this.UnregisterDisposable(Graphics);
        }

        base.Dispose(disposing);
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
                MipSlice = (uint)MipLevel
            };
            return;
        }

        desc.Texture2D = new D3D12_TEX2D_RTV
        {
            MipSlice = (uint)MipLevel
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

    /// <inheritdoc/>
    private protected sealed override (D3D12_CPU_DESCRIPTOR_HANDLE CpuHandle, D3D12_GPU_DESCRIPTOR_HANDLE GpuHandle) OnCreateViewHandles()
    {
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

        return (_allocation.CpuHandle, D3D12_GPU_DESCRIPTOR_HANDLE.DEFAULT);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTextureView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
    /// <param name="texture">The texture for the view.</param>
    /// <param name="format">The format for the view.</param>
    /// <param name="formatInfo">Information about the view format.</param>
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    internal GorgonRenderTargetView(GorgonGraphics graphics, string name, GorgonTexture texture, BufferFormat format, GorgonFormatInfo formatInfo, bool owned)
        : base(graphics, name, texture, owned)
    {
        _textureImageInfo = Texture = texture;

        Format = format;
        FormatInfo = formatInfo;

        // Default the view depth slice count to match the actual depth.
        DepthCount = texture.Depth;

        this.RegisterDisposable(Graphics);

        CreateNative();
    }
}
