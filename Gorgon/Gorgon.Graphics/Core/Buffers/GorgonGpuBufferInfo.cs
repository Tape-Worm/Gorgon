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
// Created: January 14, 2026 9:42:47 PM
//

using System;
using System.Collections.Generic;
using System.Text;
using Gorgon.Native;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines how the buffer should be used.
/// </summary>
public enum BufferUsage
{
    /// <summary>
    /// <b>Buffer is readable and writable by the GPU, but not by the CPU.</b>
    /// <para>
    /// The majority of resources are expected to use this value, and are typically populated by filling a <see cref="Upload"/> resource and copying to the <c>Default</c> resource.
    /// </para>
    /// </summary>
    Default = 0,
    /// <summary>
    /// <b>Buffer is readable by the GPU, and writable by the CPU.</b>
    /// <para>
    /// This usage type is meant for write once by the CPU, and read many by the GPU. Resources with this usage are typically not as fast as <see cref="Default"/> usage resources.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// Do not attempt to perform CPU reads from resources created with this usage. There will be severe performance penalties for attempting to do so.
    /// </para>
    /// </note>
    /// </para>
    /// </summary>
    Upload = 1,
    /// <summary>
    /// <b>Buffer is writable by the GPU, and readable by the CPU.</b>
    /// <para>
    /// This usage type is meant to allow the CPU to read data back from the GPU.
    /// </para>
    /// </summary>
    Download = 2,
    /// <summary>
    /// <b>Buffer is writable by the CPU, and readable by the GPU.</b>
    /// <para>
    /// This usage type is similar to the <see cref="Upload"/> usage, except that the contents will only be uploaded to the GPU when the buffer is used in a command list. 
    /// </para>
    /// <para>
    /// This is similar to the old Dynamic buffers in Direct 3D 11 with a DISCARD mapping. When the buffer is copied to the GPU, the entire buffer is copied from the CPU up to the the GPU on each frame. This 
    /// may not be ideal for performance, but allows for constant updating of the buffer from the CPU.
    /// </para>
    /// </summary>
    DynamicPerFrame = 3,
}

/// <summary>
/// Settings used for creating a <see cref="GorgonGpuBuffer_OLDE"/>.
/// </summary>
/// <param name="SizeInBytes">The size of the buffer, in bytes.</param>
/// <param name="Usage">The intended usage for the buffer.</param>
/// <remarks>
/// <para>
/// The <paramref name="SizeInBytes"/> parameter must be greater than 0.
/// </para>
/// </remarks>
public record class GorgonGpuBufferInfo(long SizeInBytes, BufferUsage Usage)
{
    /// <summary>
    /// An empty instance of the <see cref="GorgonGpuBuffer_OLDE"/> type.
    /// </summary>
    public static readonly GorgonGpuBufferInfo Empty = new(0, BufferUsage.Default);

    /// <inheritdoc cref="IGorgonGpuBufferInfo.IsRenderTarget"/>
    /// <remarks>
    /// The default value is <b>false</b>.
    /// </remarks>
    public bool IsRenderTarget
    {
        get;
        init;
    } = false;

    /// <inheritdoc cref="IGorgonGpuBufferInfo.IsUnorderedAccess"/>
    /// <remarks>
    /// The default value is <b>false</b>.
    /// </remarks>
    public bool IsUnorderedAccess
    {
        get;
        init;
    } = false;

    /// <inheritdoc cref="IGorgonGpuBufferInfo.IsConstantBuffer"/>
    /// <remarks>
    /// The default value is <b>false</b>.
    /// </remarks>
    public bool IsConstantBuffer
    {
        get;
        init;
    } = false;

    /// <summary>
    /// <inheritdoc cref="IGorgonGpuBufferInfo.Alignment"/>
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="IGorgonGpuBufferInfo.Alignment" path="/remarks/para[1]"/>
    /// <inheritdoc cref="IGorgonGpuBufferInfo.Alignment" path="/remarks/para[2]"/>
    /// <inheritdoc cref="IGorgonGpuBufferInfo.Alignment" path="/remarks/para[3]"/>
    /// <para>
    /// This value must be a power of 2, and non-negative. If these conditions are not met, then the buffer will not be created.
    /// </para>
    /// <para>
    /// The default value is 0, meaning no alignment (this is the same as an alignment of 1).
    /// </para>
    /// </remarks>
    public int Alignment
    {
        get;
        init;
    }

    /// <summary>
    /// Function to create a buffer information structure for a constant buffer.
    /// </summary>
    /// <param name="sizeInBytes">The size of the buffer, in bytes.</param>
    /// <param name="usage">The intended usage for the buffer.</param>
    /// <returns>A new <see cref="GorgonGpuBufferInfo"/> setup to build a constant buffer.</returns>
    /// <remarks>
    /// <para>
    /// Constant buffers require an alignment of 256 bytes. Because of this, the <paramref name="sizeInBytes"/> parameter will be adjusted to the nearest 256 bytes.
    /// </para>
    /// </remarks>
    public static GorgonGpuBufferInfo ConstantBuffer(int sizeInBytes, BufferUsage usage) => new(sizeInBytes.AlignUp(D3D12.D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT), usage)
    {
        IsConstantBuffer = true
    };    

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGpuBufferInfo"/> class.
    /// </summary>
    /// <param name="info">The buffer creation information to copy.</param>
    public GorgonGpuBufferInfo(GorgonGpuBufferInfo info)
    {
        SizeInBytes = info.SizeInBytes;
        Usage = info.Usage;
        IsRenderTarget = info.IsRenderTarget;
        IsUnorderedAccess = info.IsUnorderedAccess;
        IsConstantBuffer = info.IsConstantBuffer;
        Alignment = info.Alignment;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IGorgonGpuBufferInfo"/> class.
    /// </summary>
    /// <param name="info">The buffer creation information to copy.</param>
    public GorgonGpuBufferInfo(IGorgonGpuBufferInfo info)
        : this(info.SizeInBytes, info.Usage)
    {
        IsRenderTarget = info.IsRenderTarget;
        IsUnorderedAccess = info.IsUnorderedAccess;        
        IsConstantBuffer = info.IsConstantBuffer;
        Alignment = info.Alignment;
    }
}

