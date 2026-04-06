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
/// A view for a texture resource.
/// </summary>
/// <remarks>
/// <para>
/// TODO: Write something here.
/// </para>
/// </remarks>
public unsafe sealed class GorgonTextureView
    : GorgonResourceView
{
    private GpuDescriptorAllocation _allocation = GpuDescriptorAllocation.Null;

    /// <summary>
    /// Property to return the texture used by this view.
    /// </summary>
    public GorgonTexture Texture
    {
        get;
    }

    /// <summary>
    /// Property to return the format for the view.
    /// </summary>
    public BufferFormat Format
    {
        get;
    }

    /// <summary>
    /// Property to return the first mip level in the view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="MinimumLodClamp"/> value should be set to zero if this value is non-zero.
    /// </para>
    /// </remarks>
    public short MipLevel
    {
        get;
    }

    /// <summary>
    /// Property to return the number of mip levels in the view.
    /// </summary>
    public short MipCount
    {
        get;
    }

    /// <summary>
    /// Property to return the first array index within the buffer to start the view at.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value only applies to textures that have a <see cref="GorgonTexture.Type"/> of <see cref="TextureType.Texture1D"/>, or <see cref="TextureType.Texture2D"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="TextureType"/>
    public short ArrayIndex
    {
        get;
    }

    /// <summary>
    /// Property to return the number of array indices in the view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value only applies to textures that have a <see cref="GorgonTexture.Type"/> of <see cref="TextureType.Texture1D"/>, or <see cref="TextureType.Texture2D"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="TextureType"/>
    public short ArrayCount
    {
        get;
    }

    /// <summary>
    /// Property to return the minimum LOD clamp that can be accessed by the view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A value of 0 indicates that the entire mip chain is accessible, specifying 3.0f means that mip map levels from 3.0 to <see cref="GorgonTexture.MipCount"/><c>-1</c> are accessible.
    /// </para>
    /// <para>
    /// The <see cref="MipLevel"/> value should be set to zero if this value is non-zero.
    /// </para>
    /// </remarks>
    public float MinimumLodClamp
    {
        get;
    }

    /// <summary>
    /// Property to return the the index of the plane in a planar format to use in the view.
    /// </summary>
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

        Graphics.Log.Print($"Allocating GPU/CPU handle for '{Name}'.", LoggingLevel.Verbose);
        Graphics.GpuViewDescriptors.Allocate(1, out _allocation);

        D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle = Graphics.GpuViewDescriptors.D3DCpuHandle;
        D3D12_GPU_DESCRIPTOR_HANDLE gpuHandle = Graphics.GpuViewDescriptors.D3DGpuHandle;

        cpuHandle.Offset(_allocation.Offset, Graphics.GpuViewDescriptors.DescriptorSize);
        gpuHandle.Offset(_allocation.Offset, Graphics.GpuViewDescriptors.DescriptorSize);

        Graphics.D3DDevice.Get()->CreateShaderResourceView((PID3D12Resource2)Resource.D3DResource.Get(), &desc, cpuHandle);

        SetHandles(cpuHandle, gpuHandle);
    }

    /// <inheritdoc/>
    private protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_allocation.Equals(GpuDescriptorAllocation.Null))
            {
                Graphics.Log.Print($"Freeing CPU descriptor handle allocation for '{Name}'.", LoggingLevel.Verbose);
                Graphics.GpuViewDescriptors.Free(ref _allocation);
                _allocation = GpuDescriptorAllocation.Null;
            }
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Function to retrieve the handle of the view, which is used to pass to a shader for resource heap indexing.
    /// </summary>
    /// <returns>The handle of the view.</returns>
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
    /// Initializes a new instance of the <see cref="GorgonTextureView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
    /// <param name="texture"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='resource']"/></param>
    /// <param name="format">The format for the view.</param>
    /// <param name="mipLevel">The first mip level to view.</param>
    /// <param name="mipCount">The number of mip levels to view.</param>
    /// <param name="arrayIndex">The first array index to view.</param>
    /// <param name="arrayCount">The number of array indices to view.</param>
    /// <param name="minLodClamp">The minimum LOD resource clamp.</param>
    /// <param name="planeIndex">The index of the plane in a planar format.</param>
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    internal GorgonTextureView(GorgonGraphics graphics, string name, GorgonTexture texture, BufferFormat format, short mipLevel, short mipCount, short arrayIndex, short arrayCount, float minLodClamp, byte planeIndex, bool owned)
        : base(graphics, $"{name} - Shader Resource View", texture, owned)
    {
        Texture = texture;
        Format = format;
        MipLevel = mipLevel;
        MipCount = mipCount;
        ArrayIndex = arrayIndex;
        ArrayCount = arrayCount;
        MinimumLodClamp = minLodClamp;
        PlaneIndex = planeIndex;

        AllocateDescriptors();
    }
}