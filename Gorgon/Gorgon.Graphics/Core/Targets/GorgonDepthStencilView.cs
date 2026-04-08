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
/// The flags used to determine if parts of the view are read only or not.
/// </summary>
[Flags]
public enum DepthStencilViewAccess
{
    /// <summary>
    /// A default view.
    /// </summary>
    None = D3D12_DSV_FLAGS.D3D12_DSV_FLAG_NONE,    
    /// <summary>
    /// Depth values are read only.
    /// </summary>
    ReadOnlyDepth = D3D12_DSV_FLAGS.D3D12_DSV_FLAG_READ_ONLY_DEPTH,
    /// <summary>
    /// Stencil values are read only.
    /// </summary>
    ReadOnlyStencil = D3D12_DSV_FLAGS.D3D12_DSV_FLAG_READ_ONLY_STENCIL
}

/// <summary>
/// A view for a depth/stencil texture resource.
/// </summary>
/// <remarks>
/// <para>
/// TODO: Write something here.
/// </para>
/// </remarks>
public unsafe sealed class GorgonDepthStencilView
    : GorgonResourceView
{
    private CpuDescriptorAllocation _allocation;

    /// <summary>
    /// Property to return the format of view data.
    /// </summary>
    public BufferFormat Format
    {
        get;
    }

    /// <summary>
    /// Property to return the format information for the <see cref="Format"/>.
    /// </summary>
    public GorgonFormatInfo FormatInfo
    {
        get;
    }

    /// <summary>
    /// Property to return the access state for the view.
    /// </summary>
    public DepthStencilViewAccess DepthStencilViewAccess
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
    /// Property to return the first map level in the view.
    /// </summary>
    public short MipLevel
    {
        get;
    }

    /// <summary>
    /// Property to return the first array index in the view.
    /// </summary>
    public short ArrayIndex
    {
        get;
    }

    /// <summary>
    /// Property to return the number of array indices in the view.
    /// </summary>
    public short ArrayCount
    {
        get;
    }

    /// <summary>
    /// Function to update the description with 2D texture information.
    /// </summary>
    /// <param name="desc">The description to update.</param>
    /// <param name="hasMultiSample"><b>true</b> if the resource is multisampled, <b>false</b> if not.</param>
    /// <param name="hasArrays"><b>true</b> if the resource has array slices, <b>false</b> if not.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetTexture2DInfo(ref D3D12_DEPTH_STENCIL_VIEW_DESC desc, bool hasMultiSample, bool hasArrays)
    {
        if (hasMultiSample)
        {
            if (hasArrays)
            {
                desc.Texture2DMSArray = new D3D12_TEX2DMS_ARRAY_DSV
                {
                    ArraySize = (uint)ArrayCount,
                    FirstArraySlice = (uint)ArrayIndex                    
                };
            }

            return;
        }

        if (hasArrays)
        {
            desc.Texture2DArray = new D3D12_TEX2D_ARRAY_DSV
            {
                ArraySize = (uint)ArrayCount,
                FirstArraySlice = (uint)ArrayIndex,
                MipSlice = (uint)MipLevel             
            };
            return;
        }

        desc.Texture2D = new D3D12_TEX2D_DSV
        {
            MipSlice = (uint)MipLevel
        };
    }

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
        D3D12_DSV_DIMENSION dimension;

        if (hasMultiSample)
        {
            dimension = hasArrays ? D3D12_DSV_DIMENSION.D3D12_DSV_DIMENSION_TEXTURE2DMSARRAY : D3D12_DSV_DIMENSION.D3D12_DSV_DIMENSION_TEXTURE2DMS;
        }
        else
        {
            dimension = hasArrays ? D3D12_DSV_DIMENSION.D3D12_DSV_DIMENSION_TEXTURE2DARRAY : D3D12_DSV_DIMENSION.D3D12_DSV_DIMENSION_TEXTURE2D;
        };        

        D3D12_DEPTH_STENCIL_VIEW_DESC desc = new()
        {
            ViewDimension = dimension,
            Format = (DXGI_FORMAT)Format,
            Flags = (D3D12_DSV_FLAGS)DepthStencilViewAccess
        };

        GetTexture2DInfo(ref desc, hasMultiSample, hasArrays);

        Graphics.Log.Print($"Allocating CPU handle for {Name}.", LoggingLevel.Verbose);
        Graphics.RtvDescriptors.Allocate(1, out _allocation);
        Graphics.D3DDevice.Get()->CreateDepthStencilView((PID3D12Resource2)Resource.D3DResource.Get(), &desc, _allocation.CpuHandle);

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
    /// Function to determine if the view settings are valid for a depth/stencil buffer.
    /// </summary>
    /// <param name="name">The name of the depth/stencil texture.</param>
    /// <param name="formatSupport">The support for the format.</param>
    /// <param name="textureFormatInfo">The information about the texture format.</param>
    /// <param name="viewFormatInfo">The information about the view format.</param>
    /// <param name="isDepthStencil"><b>true</b> if the texture is capable of being used as a depth/stencil buffer, <b>false</b> if not.</param>
    /// <exception cref="GorgonException"><para>Thrown if the texture is not a <see cref="GorgonTexture.IsDepthStencil">depth stencil</see> buffer.</para>
    /// <para>Thrown if the format is not supported by depth/stencil buffers.</para>
    /// <para>Thrown if the texture format is not the same as the view format, and the texture format is not one of the required typeless formats.</para>
    /// </exception>
    internal static void ValidateDepthStencilView(string name, GorgonBufferFormatSupport formatSupport, GorgonFormatInfo textureFormatInfo, GorgonFormatInfo viewFormatInfo, bool isDepthStencil)
    {
        if (!isDepthStencil)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_DSV_NOT_DEPTHSTENCIL, name));
        }

        if (!formatSupport.IsDepthStencilFormat)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_DSV_FORMAT_DEPTHSTENCIL_FORMAT, viewFormatInfo.Format));
        }

        // If the formats match (i.e. one of the depth formats), then we're good (we don't cast these).
        if (textureFormatInfo.Format == viewFormatInfo.Format)
        {
            return;
        }

        switch (formatSupport.Format)
        {
            case BufferFormat.D16_UNorm:
                if (textureFormatInfo.Format is not BufferFormat.R16_Typeless)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_DSV_FORMAT_MISMATCH, name, textureFormatInfo.Format, viewFormatInfo.Format, BufferFormat.R16_Typeless));
                }
                break;
            case BufferFormat.D32_Float:
                if (textureFormatInfo.Format is not BufferFormat.R32_Typeless)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_DSV_FORMAT_MISMATCH, name, textureFormatInfo.Format, viewFormatInfo.Format, BufferFormat.R32_Typeless));
                }
                break;
            case BufferFormat.D24_UNorm_S8_UInt:
                if (textureFormatInfo.Format is not BufferFormat.R24G8_Typeless)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_DSV_FORMAT_MISMATCH, name, textureFormatInfo.Format, viewFormatInfo.Format, BufferFormat.R24G8_Typeless));
                }
                break;
            case BufferFormat.D32_Float_S8X24_UInt:
                if (textureFormatInfo.Format is not BufferFormat.R32G8X24_Typeless)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_DSV_FORMAT_MISMATCH, name, textureFormatInfo.Format, viewFormatInfo.Format, BufferFormat.R32G8X24_Typeless));
                }
                break;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonDepthStencilView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
    /// <param name="texture">The texture for the view.</param>
    /// <param name="formatInfo">Information about the view format.</param>
    /// <param name="access">The access state for the view.</param>
    /// <param name="mipLevel">The first mip level to view.</param>
    /// <param name="arrayIndex">The first array index to view.</param>
    /// <param name="arrayCount">The number of array indices to view.</param>
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    internal GorgonDepthStencilView(GorgonGraphics graphics, string name, GorgonTexture texture, GorgonFormatInfo formatInfo, DepthStencilViewAccess access, short mipLevel, short arrayIndex, short arrayCount, bool owned)
        : base(graphics, $"{GorgonGraphicsFactory.GenerateName(name, nameof(GorgonDepthStencilView))} - Depth Stencil View", texture, owned)
    {
        Texture = texture;
        Format = formatInfo.Format;
        FormatInfo = formatInfo;
        DepthStencilViewAccess = access;
        MipLevel = mipLevel;
        ArrayIndex = arrayIndex;
        ArrayCount = arrayCount;

        AllocateDescriptors();
    }
}
