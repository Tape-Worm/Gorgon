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

#pragma warning disable CA1067 // Override Object.Equals(object) when implementing IEquatable<T>. We don't need this, we're using IEquatable<T> to satisfy a generic constraint.

/// <summary>
/// A view for a texture resource.
/// </summary>
/// <remarks>
/// <para>
/// TODO: Write something here.
/// </para>
/// </remarks>
public unsafe sealed class GorgonTextureView
    : GorgonShaderResourceView, IGorgonImageInfo, IEquatable<GorgonTextureView?>
{
    private GpuDescriptorAllocation _allocation;
    private readonly IGorgonImageInfo _textureImageInfo;

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
    /// Property to return the number of mip levels in the view.
    /// </summary>
    public short MipCount
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

    /// <summary>
    /// Property to return the minimum mip level that can be accessed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Specifying 0.0f means that all mip levels can be accessed, while specifying 3.0f indicates that all levels from 3.0f up to <see cref="MipCount"/> are accessible.
    /// </para>
    /// <para>
    /// It is not recommended to set this value and <see cref="MipLevel"/> at the same time. Use one or the other.
    /// </para>
    /// </remarks>
    public float ResourceMinimumLODClamp
    {
        get;
    }

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
    private protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.UnregisterDisposable(Graphics);
        }

        base.Dispose(disposing);
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
                return D3D12_SHADER_RESOURCE_VIEW_DESC.TexCubeArray((DXGI_FORMAT)Format, cubeCount, (uint)MipCount, (uint)ArrayIndex, (uint)MipLevel, ResourceMinimumLODClamp, D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING);
            }

            return D3D12_SHADER_RESOURCE_VIEW_DESC.TexCube((DXGI_FORMAT)Format, (uint)MipCount, (uint)MipLevel, ResourceMinimumLODClamp, D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING);
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
            return D3D12_SHADER_RESOURCE_VIEW_DESC.Tex2DArray((DXGI_FORMAT)Format, (uint)ArrayCount, (uint)MipCount, (uint)ArrayIndex, (uint)MipLevel, 0, ResourceMinimumLODClamp, D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING);
        }

        return D3D12_SHADER_RESOURCE_VIEW_DESC.Tex2D((DXGI_FORMAT)Format, (uint)MipCount, (uint)MipLevel, 0, ResourceMinimumLODClamp, D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING);
    }

    /// <inheritdoc/>
    private protected sealed override (D3D12_CPU_DESCRIPTOR_HANDLE CpuHandle, D3D12_GPU_DESCRIPTOR_HANDLE GpuHandle) OnCreateViewHandles()
    {
        bool isMultiSampled = !Texture.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultisampling);
        bool isArray = Texture.ArrayCount > 1;

        D3D12_SHADER_RESOURCE_VIEW_DESC desc = Texture.Type switch
        {
            TextureType.Texture1D => isArray ? D3D12_SHADER_RESOURCE_VIEW_DESC.Tex1DArray((DXGI_FORMAT)Format, (uint)ArrayCount, (uint)MipCount, (uint)ArrayIndex, (uint)MipLevel, ResourceMinimumLODClamp, D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING)
                                             : D3D12_SHADER_RESOURCE_VIEW_DESC.Tex1D((DXGI_FORMAT)Format, (uint)MipCount, (uint)MipLevel, ResourceMinimumLODClamp, D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING),
            TextureType.Texture2D => Create2DDesc(isMultiSampled, isArray, Texture.IsCube),
            TextureType.Texture3D => D3D12_SHADER_RESOURCE_VIEW_DESC.Tex3D((DXGI_FORMAT)Format, (uint)MipCount, (uint)MipLevel, ResourceMinimumLODClamp, D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING),
            _ => throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_VIEW_UNKNOWN_TYPE, Texture.Type))
        };

        Graphics.Log.Print($"Allocating GPU/CPU handle for {Name}.", LoggingLevel.Verbose);
        Graphics.GpuViewDescriptors.Allocate(1, out _allocation);

        D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle = Graphics.GpuViewDescriptors.D3DCpuHandle;
        D3D12_GPU_DESCRIPTOR_HANDLE gpuHandle = Graphics.GpuViewDescriptors.D3DGpuHandle;

        cpuHandle.Offset(_allocation.Offset, Graphics.GpuViewDescriptors.DescriptorSize);
        gpuHandle.Offset(_allocation.Offset, Graphics.GpuViewDescriptors.DescriptorSize);

        Graphics.D3DDevice.Get()->CreateShaderResourceView((PID3D12Resource2)Resource.D3DResource.Get(), &desc, cpuHandle);

        return (cpuHandle, gpuHandle);
    }

    /// <inheritdoc/>
    bool IEquatable<GorgonTextureView?>.Equals(GorgonTextureView? other) => ReferenceEquals(this, other);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTextureView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonShaderResourceView(GorgonGraphics, string, GorgonGpuResource, BufferFormat, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonShaderResourceView(GorgonGraphics, string, GorgonGpuResource, BufferFormat, bool)" path="/param[@name='name']"/></param>
    /// <param name="texture">The texture for the view.</param>
    /// <param name="format"><inheritdoc cref="GorgonShaderResourceView(GorgonGraphics, string, GorgonGpuResource, BufferFormat, bool)" path="/param[@name='format']"/></param>
    /// <param name="arrayIndex">The starting array index for the view.</param>
    /// <param name="arrayCount">The number of array indices for the view.</param>
    /// <param name="mipLevel">The starting mip level for the view.</param>
    /// <param name="mipCount">The number of mip levels for the view.</param>
    /// <param name="resourceMinLODClamp">The minimum mip level for the view.</param>
    /// <param name="owned"><inheritdoc cref="GorgonShaderResourceView(GorgonGraphics, string, GorgonGpuResource, BufferFormat, bool)" path="/param[@name='owned']"/></param>
    internal GorgonTextureView(GorgonGraphics graphics, string name, GorgonTexture texture, BufferFormat format, short arrayIndex, short arrayCount, short mipLevel, short mipCount, float resourceMinLODClamp, bool owned)
        : base(graphics, name, texture, format, owned)
    {
        _textureImageInfo = Texture = texture;

        MipLevel = mipLevel;
        MipCount = mipCount;
        ArrayIndex = arrayIndex;
        ArrayCount = arrayCount;
        ResourceMinimumLODClamp = resourceMinLODClamp;

        this.RegisterDisposable(Graphics);

        CreateNative();
    }
}
