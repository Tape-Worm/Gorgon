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
// Created: January 3, 2026 2:26:29 AM
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Gorgon.Graphics.Imaging;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides information about a resource.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
internal readonly struct GpuResourceInfo
{
    /// <summary>
    /// Information about a buffer resource.
    /// </summary>
    /// <param name="Size">The size of the buffer, in bytes.</param>
    [StructLayout(LayoutKind.Sequential)]
    public record struct BufferResource(long Size);

    /// <summary>
    /// Information about a 1D texture resource.
    /// </summary>
    /// <param name="Width">The width of the texture, in pixels.</param>
    /// <param name="MipCount">The number of mip levels.</param>
    /// <param name="ArrayCount">The number of array levels.</param>
    /// <param name="InitialLayout">The initial layout for the texture data.</param>
    [StructLayout(LayoutKind.Sequential)]
    public record struct Texture1DResource(int Width, short ArrayCount, short MipCount, BarrierLayout InitialLayout);

    /// <summary>
    /// Information about a 2D texture resource.
    /// </summary>
    /// <param name="Width">The width of the texture, in pixels.</param>
    /// <param name="Height">The height of the texture, in pixels.</param>
    /// <param name="MipCount">The number of mip levels.</param>
    /// <param name="ArrayCount">The number of array levels.</param>
    /// <param name="MultisampleInfo">The multisample information for the texture.</param>
    /// <param name="InitialLayout">The initial layout for the texture data.</param>
    [StructLayout(LayoutKind.Sequential)]
    public record struct Texture2DResource(int Width, int Height, short ArrayCount, short MipCount, GorgonMultisampleInfo MultisampleInfo, BarrierLayout InitialLayout);

    /// <summary>
    /// Information about a 3D texture resource.
    /// </summary>
    /// <param name="Width">The width of the texture, in pixels.</param>
    /// <param name="Height">The height of the texture, in pixels.</param>
    /// <param name="Depth">The depth of the texture, in slices.</param>
    /// <param name="MipCount">The number of mip levels.</param>
    /// <param name="InitialLayout">The initial layout for the texture data.</param>
    [StructLayout(LayoutKind.Sequential)]
    public record struct Texture3DResource(int Width, int Height, short Depth, short MipCount, BarrierLayout InitialLayout);

    /// <summary>
    /// Property to return the type of data stored in the resource.
    /// </summary>
    [FieldOffset(0)]
    public readonly GraphicsResourceType ResourceType;

    /// <summary>
    /// Property to return the alignment of the data within the resource, in bytes.
    /// </summary>
    [FieldOffset(4)]
    public readonly long Alignment;

    /// <summary>
    /// Property to return the format of the data contained within the resource.
    /// </summary>
    [FieldOffset(12)]
    public readonly BufferFormat Format;

    /// <summary>
    /// Property to return the flags indicating the intended usage for the resource.
    /// </summary>
    [FieldOffset(16)]
    public readonly GraphicsResourceUsage Usage;

    /// <summary>
    /// Property to return information about a buffer resource.
    /// </summary>
    [FieldOffset(20)]
    public readonly BufferResource Buffer;

    /// <summary>
    /// Property to return information about a 1D texture resource.
    /// </summary>
    [FieldOffset(20)]
    public readonly Texture1DResource Texture1D;

    /// <summary>
    /// Property to return information about a 2D texture resource.
    /// </summary>
    [FieldOffset(20)]
    public readonly Texture2DResource Texture2D;

    /// <summary>
    /// Property to return information about a 3D texture resource.
    /// </summary>
    [FieldOffset(20)]
    public readonly Texture3DResource Texture3D;

    /// <summary>
    /// Function to convert a D3D 12 resource description into a <see cref="GpuResourceInfo"/>.
    /// </summary>
    /// <param name="desc">The description to convert.</param>
    /// <returns>The graphics resource information.</returns>
    internal static GpuResourceInfo FromD3D(ref readonly D3D12_RESOURCE_DESC1 desc)
    {
        GraphicsResourceUsage flags = (GraphicsResourceUsage)desc.Flags | 
            ((desc.Flags & D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_DENY_SHADER_RESOURCE) == D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_DENY_SHADER_RESOURCE ? GraphicsResourceUsage.None : GraphicsResourceUsage.ShaderResource);

        return desc.Dimension switch
        {
            D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_BUFFER => new((GraphicsResourceType)desc.Dimension, (long)desc.Alignment, (BufferFormat)desc.Format, flags, new BufferResource((long)desc.Width)),
            D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_TEXTURE1D => new((GraphicsResourceType)desc.Dimension, (long)desc.Alignment, (BufferFormat)desc.Format, flags, new Texture1DResource((int)desc.Width,
                                                                            (short)desc.DepthOrArraySize, (short)desc.MipLevels, (BarrierLayout)desc.Layout)),
            D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_TEXTURE2D => new((GraphicsResourceType)desc.Dimension, (long)desc.Alignment, (BufferFormat)desc.Format, flags, new Texture2DResource((int)desc.Width, (int)desc.Height,
                                                                            (short)desc.DepthOrArraySize, (short)desc.MipLevels, new GorgonMultisampleInfo((int)desc.SampleDesc.Count, (int)desc.SampleDesc.Quality), (BarrierLayout)desc.Layout)),
            D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_TEXTURE3D => new((GraphicsResourceType)desc.Dimension, (long)desc.Alignment, (BufferFormat)desc.Format, flags, new Texture3DResource((int)desc.Width, (int)desc.Height,
                                                                            (short)desc.DepthOrArraySize, (short)desc.MipLevels, (BarrierLayout)desc.Layout)),
            _ => default
        };
    }

    /// <summary>
    /// Function to convert a <see cref="GpuResourceInfo"/> into a D3D12 resource description.
    /// </summary>
    /// <param name="desc">The resource description to populate.</param>
    internal void ToD3DResourceDesc(out D3D12_RESOURCE_DESC1 desc)
    {
        D3D12_RESOURCE_FLAGS flags = (D3D12_RESOURCE_FLAGS)(Usage & ~GraphicsResourceUsage.ShaderResource);

        if ((Usage & GraphicsResourceUsage.ShaderResource) != GraphicsResourceUsage.ShaderResource)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_DENY_SHADER_RESOURCE;
        }

        desc = ResourceType switch
        {
            GraphicsResourceType.Texture1D => D3D12_RESOURCE_DESC1.Tex1D((DXGI_FORMAT)Format, (ulong)Texture1D.Width, (ushort)Texture1D.ArrayCount, (ushort)Texture1D.MipCount, flags, (D3D12_TEXTURE_LAYOUT)Texture1D.InitialLayout, (ulong)Alignment),
            GraphicsResourceType.Texture2D => D3D12_RESOURCE_DESC1.Tex2D((DXGI_FORMAT)Format, (ulong)Texture2D.Width, (uint)Texture2D.Height, (ushort)Texture2D.ArrayCount, (ushort)Texture2D.MipCount,
                                                              (uint)Texture2D.MultisampleInfo.Count, (uint)Texture2D.MultisampleInfo.Quality, flags, (D3D12_TEXTURE_LAYOUT)Texture2D.InitialLayout, (ulong)Alignment),
            GraphicsResourceType.Texture3D => D3D12_RESOURCE_DESC1.Tex3D((DXGI_FORMAT)Format, (ulong)Texture3D.Width, (uint)Texture3D.Height, (ushort)Texture3D.Depth, (ushort)Texture3D.MipCount,
                                                              flags, (D3D12_TEXTURE_LAYOUT)Texture3D.InitialLayout, (ulong)Alignment),
            _ => D3D12_RESOURCE_DESC1.Buffer((ulong)Buffer.Size, flags),
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GpuResourceInfo"/> struct.
    /// </summary>
    /// <param name="type">The type of resource.</param>
    /// <param name="alignment">The alignment of the data contained within the resource.</param>
    /// <param name="format">The format of the data in the resource.</param>
    /// <param name="usage">Flags indicating the intended usage for the resource.</param>
    /// <param name="bufferInfo">The information for a buffer resource.</param>
    public GpuResourceInfo(GraphicsResourceType type, long alignment, BufferFormat format, GraphicsResourceUsage usage, BufferResource bufferInfo)
    {
        ResourceType = type;
        Alignment = alignment;
        Format = format;
        Buffer = bufferInfo;
        Usage = usage;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GpuResourceInfo"/> struct.
    /// </summary>
    /// <param name="type">The type of resource.</param>
    /// <param name="alignment">The alignment of the data contained within the resource.</param>
    /// <param name="format">The format of the data in the resource.</param>
    /// <param name="usage">Flags indicating the intended usage for the resource.</param>
    /// <param name="textureInfo">The information for a 1D texture resource.</param>
    public GpuResourceInfo(GraphicsResourceType type, long alignment, BufferFormat format, GraphicsResourceUsage usage, Texture1DResource textureInfo)
    {
        ResourceType = type;
        Alignment = alignment;
        Format = format;
        Texture1D = textureInfo;
        Usage = usage;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GpuResourceInfo"/> struct.
    /// </summary>
    /// <param name="type">The type of resource.</param>
    /// <param name="alignment">The alignment of the data contained within the resource.</param>
    /// <param name="format">The format of the data in the resource.</param>
    /// <param name="usage">Flags indicating the intended usage for the resource.</param>
    /// <param name="textureInfo">The information for a 2D texture resource.</param>
    public GpuResourceInfo(GraphicsResourceType type, long alignment, BufferFormat format, GraphicsResourceUsage usage, Texture2DResource textureInfo)
    {
        ResourceType = type;
        Alignment = alignment;
        Format = format;
        Texture2D = textureInfo;
        Usage = usage;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GpuResourceInfo"/> struct.
    /// </summary>
    /// <param name="type">The type of resource.</param>
    /// <param name="alignment">The alignment of the data contained within the resource.</param>
    /// <param name="format">The format of the data in the resource.</param>
    /// <param name="usage">Flags indicating the intended usage for the resource.</param>
    /// <param name="textureInfo">The information for a 3D texture resource.</param>
    public GpuResourceInfo(GraphicsResourceType type, long alignment, BufferFormat format, GraphicsResourceUsage usage, Texture3DResource textureInfo)
    {
        ResourceType = type;
        Alignment = alignment;
        Format = format;
        Texture3D = textureInfo;
        Usage = usage;
    }
}
