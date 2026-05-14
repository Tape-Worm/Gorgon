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
using Gorgon.Collections;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A view for a texture resource.
/// </summary>
internal unsafe sealed class TextureView<T>
    : GorgonResourceView, IGorgonTextureView<T>
    where T : GorgonTextureCommon
{
    private GpuDescriptorAllocation _allocation = GpuDescriptorAllocation.Null;
    private GpuDescriptorHeap _descriptors;

    /// <inheritdoc/>
    public T Texture
    {
        get;
    }

    /// <inheritdoc/>
    public GorgonFormatInfo FormatInfo
    {
        get;
    }

    /// <inheritdoc/>
    public BufferFormat Format
    {
        get;
    }

    /// <inheritdoc/>
    public short MipLevel
    {
        get;
    }

    /// <inheritdoc/>
    public short MipCount
    {
        get;
    }

    /// <inheritdoc/>
    public short ArrayIndex
    {
        get;
    }

    /// <inheritdoc/>
    public short ArrayCount
    {
        get;
    }

    /// <inheritdoc/>
    public float MinimumLodClamp
    {
        get;
    }

    /// <inheritdoc/>
    public byte PlaneIndex
    {
        get;
    }

    /// <summary>
    /// Function to create a 2D texture view description.
    /// </summary>
    /// <param name="isMultiSampled"><b>true</b> if the texture is multisampled, <b>false</b> if not.</param>
    /// <param name="isArray"><b>true</b> if the texture is an array, <b>false</b> if not.</param>
    /// <param name="isCube"><b>true</b> if the texture is a cube map, <b>false</b> if not.</param>
    /// <returns>A populated 2D texture view description.</returns>
    /// <remarks>
    /// <para>
    /// When the <paramref name="isCube"/> is <b>true</b>, then <paramref name="isArray"/> is implied to be true as well.
    /// </para>
    /// </remarks>
    private D3D12_SHADER_RESOURCE_VIEW_DESC Create2DDesc(bool isMultiSampled, bool isArray, bool isCube)
    {
        if (isCube)
        {
            uint cubeCount = (uint)(ArrayCount / 6).Max(1);

            if (cubeCount > 1)
            {
                return D3D12_SHADER_RESOURCE_VIEW_DESC.TexCubeArray((DXGI_FORMAT)Format, cubeCount, (uint)MipCount, (uint)ArrayIndex, (uint)MipLevel, MinimumLodClamp, D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING);
            }

            return D3D12_SHADER_RESOURCE_VIEW_DESC.TexCube((DXGI_FORMAT)Format, (uint)MipCount, (uint)MipLevel, MinimumLodClamp, D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING);
        }

        if (isMultiSampled)
        {
            if (isArray)
            {
                return D3D12_SHADER_RESOURCE_VIEW_DESC.Tex2DMSArray((DXGI_FORMAT)Format, (uint)ArrayCount, (uint)ArrayIndex, D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING);
            }

            return D3D12_SHADER_RESOURCE_VIEW_DESC.Tex2DMS((DXGI_FORMAT)Format, D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING);
        }

        if (isArray)
        {
            return D3D12_SHADER_RESOURCE_VIEW_DESC.Tex2DArray((DXGI_FORMAT)Format, (uint)ArrayCount, (uint)MipCount, (uint)ArrayIndex, (uint)MipLevel, PlaneIndex, MinimumLodClamp, D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING);
        }

        return D3D12_SHADER_RESOURCE_VIEW_DESC.Tex2D((DXGI_FORMAT)Format, (uint)MipCount, (uint)MipLevel, PlaneIndex, MinimumLodClamp, D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING);
    }

    /// <summary>
    /// Function to allocate a view descriptor from the descriptor heap.
    /// </summary>
    private void AllocateDescriptors()
    {
        if (!_allocation.Equals(GpuDescriptorAllocation.Null))
        {
            _descriptors.Free(ref _allocation);
        }

        bool isMultiSampled = !Texture.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling);
        bool isArray = Texture.ArrayCount > 1;

        D3D12_SHADER_RESOURCE_VIEW_DESC desc = Texture.Type switch
        {
            TextureType.Texture1D => isArray ? D3D12_SHADER_RESOURCE_VIEW_DESC.Tex1DArray((DXGI_FORMAT)Format, (uint)ArrayCount, (uint)MipCount, (uint)ArrayIndex, (uint)MipLevel, MinimumLodClamp, D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING)
                                             : D3D12_SHADER_RESOURCE_VIEW_DESC.Tex1D((DXGI_FORMAT)Format, (uint)MipCount, (uint)MipLevel, MinimumLodClamp, D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING),
            TextureType.Texture2D => Create2DDesc(isMultiSampled, isArray, Texture.IsCube),
            TextureType.Texture3D => D3D12_SHADER_RESOURCE_VIEW_DESC.Tex3D((DXGI_FORMAT)Format, (uint)MipCount, (uint)MipLevel, MinimumLodClamp, D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING),
            _ => throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_VIEW_UNKNOWN_TYPE, Texture.Type))
        };

        Graphics.Log.Print($"Allocating CPU handle for '{Name}'.", LoggingLevel.Verbose);
        _descriptors.Allocate(1, out _allocation);

        D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle = _descriptors.D3DCpuHandle;

        cpuHandle.Offset(_allocation.Offset, _descriptors.DescriptorSize);

        Graphics.D3DDevice.Get()->CreateShaderResourceView((PID3D12Resource2)Resource.D3DResource.Get(), &desc, cpuHandle);

        D3DCpuHandle = cpuHandle;
    }

    /// <inheritdoc/>
    private protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_allocation.Equals(GpuDescriptorAllocation.Null))
            {
                Graphics.Log.Print($"Freeing descriptor handle allocation for '{Name}'.", LoggingLevel.Verbose);
                _descriptors.Free(ref _allocation);
            }
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Function to validate the settings for a texture view.
    /// </summary>
    /// <param name="name">The name of the texture.</param>
    /// <param name="textureFormatInfo">The format information for the texture.</param>
    /// <param name="viewFormatInfo">The format information for the view.</param>
    /// <param name="formats">The list of formats that the texture format can cast to.</param>
    /// <param name="plane">The format plane to view.</param>
    /// <param name="isShaderResource"><b>true</b> if the texture can be used as a shader resource, <b>false</b> if not.</param>
    /// <param name="isDepthStencil"><b>true</b> if the texture is a depth/stencil texture.</param>
    /// <exception cref="GorgonException"><para>Thrown if the texture is not a <see cref="GorgonTextureCommon.IsShaderResource">shader resource</see>.</para>
    /// <para>Thrown if the format is for a depth/stencil format.</para>
    /// <para>Thrown if either the texture <see cref="GorgonTextureCommon.Format"/> or view format is for a compressed texture, and the other is not.</para>
    /// <para>Thrown if the texture is a depth/stencil texture using a <see cref="BufferFormat.D16_UNorm"/>, <see cref="BufferFormat.D32_Float"/>, <see cref="BufferFormat.D24_UNorm_S8_UInt"/> or <see cref="BufferFormat.D32_Float_S8X24_UInt"/> format.</para>
    /// <para>Thrown if the format is a planar format.</para>
    /// <para>Thrown if the texture <see cref="GorgonTextureCommon.Format"/> is a typeless format, and view format is not using the correct view format.</para>
    /// <para>Thrown if the texture format cannot be casted to the view format.</para>
    /// </exception>
    internal static void ValidateTextureView(string name, GorgonFormatInfo textureFormatInfo, GorgonFormatInfo viewFormatInfo, IReadOnlyList<BufferFormat> formats, byte plane, bool isShaderResource, bool isDepthStencil)
    {
        if (!isShaderResource)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_NOT_SHADER_RESOURCE, name));
        }

        if (viewFormatInfo.IsTypeless)
        {
            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_TEXTURE_VIEW_FORMAT_TYPELESS_NOT_SUPPORTED);
        }

        if ((viewFormatInfo.HasDepth) || (viewFormatInfo.HasStencil))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_FORMAT_DEPTHSTENCIL_NOT_SUPPORTED, name, viewFormatInfo.Format));
        }

        if (viewFormatInfo.IsPlanar)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_FORMAT_PLANAR_NOT_SUPPORTED, name, viewFormatInfo.Format));
        }

        if ((textureFormatInfo.IsCompressed != viewFormatInfo.IsCompressed))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_COMPRESSED_REQUIRED_COMPRESSED, textureFormatInfo.Format, name, viewFormatInfo.Format));
        }

        if (isDepthStencil)
        {
            if (textureFormatInfo.Format is BufferFormat.D16_UNorm or BufferFormat.D32_Float or BufferFormat.D24_UNorm_S8_UInt or BufferFormat.D32_Float_S8X24_UInt)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_NO_DEPTH_STENCIL_TEXTURE, name, textureFormatInfo.Format));
            }

            // This value is already validated against the plane info for the format before it gets here.
            switch (plane)
            {
                case 0:
                    if ((textureFormatInfo.Format == BufferFormat.R16_Typeless) && (viewFormatInfo.Format != BufferFormat.R16_UNorm))
                    {
                        throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_DEPTH_FORMAT_INVALID, name, textureFormatInfo.Format, viewFormatInfo.Format, BufferFormat.R16_UNorm));
                    }

                    if ((textureFormatInfo.Format == BufferFormat.R32_Typeless) && (viewFormatInfo.Format != BufferFormat.R32_Float))
                    {
                        throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_DEPTH_FORMAT_INVALID, name, textureFormatInfo.Format, viewFormatInfo.Format, BufferFormat.R32_Float));
                    }

                    if ((textureFormatInfo.Format == BufferFormat.R24G8_Typeless) && (viewFormatInfo.Format != BufferFormat.R24_UNorm_X8_Typeless))
                    {
                        throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_DEPTH_FORMAT_INVALID, name, textureFormatInfo.Format, viewFormatInfo.Format, BufferFormat.R24_UNorm_X8_Typeless));
                    }

                    if ((textureFormatInfo.Format == BufferFormat.R32G8X24_Typeless) && (viewFormatInfo.Format != BufferFormat.R32_Float_X8X24_Typeless))
                    {
                        throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_DEPTH_FORMAT_INVALID, name, textureFormatInfo.Format, viewFormatInfo.Format, BufferFormat.R32_Float_X8X24_Typeless));
                    }
                    break;
                case 1:
                    if ((textureFormatInfo.Format == BufferFormat.R24G8_Typeless) && (viewFormatInfo.Format != BufferFormat.X24_Typeless_G8_UInt))
                    {
                        throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_STENCIL_FORMAT_INVALID, name, textureFormatInfo.Format, viewFormatInfo.Format, BufferFormat.X24_Typeless_G8_UInt));
                    }

                    if ((textureFormatInfo.Format == BufferFormat.R32G8X24_Typeless) && (viewFormatInfo.Format != BufferFormat.X32_Typeless_G8X24_UInt))
                    {
                        throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_STENCIL_FORMAT_INVALID, name, textureFormatInfo.Format, viewFormatInfo.Format, BufferFormat.X32_Typeless_G8X24_UInt));
                    }
                    break;
            }

            return;
        }

        if (textureFormatInfo.IsPlanar)
        {
            // Planar modes get tricky to sort out, so we'll let the D3D debug layer handle these.
            return;
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

    /// <inheritdoc/>
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
    /// Initializes a new instance of the <see cref="IGorgonTextureView{GorgonTexture}"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
    /// <param name="texture"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='resource']"/></param>
    /// <param name="viewFormatInfo">The format information for the view format.</param>
    /// <param name="mipLevel">The first mip level to view.</param>
    /// <param name="mipCount">The number of mip levels to view.</param>
    /// <param name="arrayIndex">The first array index to view.</param>
    /// <param name="arrayCount">The number of array indices to view.</param>
    /// <param name="minLodClamp">The minimum LOD resource clamp.</param>
    /// <param name="planeIndex">The index of the plane in a planar format.</param>
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    internal TextureView(GorgonGraphics graphics, string name, T texture, GorgonFormatInfo viewFormatInfo, short mipLevel, short mipCount, short arrayIndex, short arrayCount, float minLodClamp, byte planeIndex, bool owned)
        : base(graphics, $"{GorgonGraphicsFactory.GenerateName(name, nameof(IGorgonTextureView<>))} - Shader Resource View", texture, owned)
    {
        _descriptors = graphics.Descriptors.GpuViewDescriptors;

        Texture = texture;
        Format = viewFormatInfo.Format;
        MipLevel = mipLevel;
        MipCount = mipCount;
        ArrayIndex = arrayIndex;
        ArrayCount = arrayCount;
        MinimumLodClamp = minLodClamp;
        PlaneIndex = planeIndex;
        FormatInfo = viewFormatInfo;

        AllocateDescriptors();
    }
}