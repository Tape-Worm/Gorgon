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
/// Common settings used for creating buffer types.
/// </summary>
public abstract record class GorgonCommonBufferInfo
{
    /// <inheritdoc cref="IGorgonCommonBufferInfo.SizeInBytes"/>
    /// <remarks>
    /// <para>
    /// This value must be greater than 0.
    /// </para>
    /// </remarks>
    public long SizeInBytes
    {
        get;
        init;
    }

    /// <inheritdoc cref="IGorgonCommonBufferInfo.Usage"/>
    public BufferUsage Usage
    {
        get;
        init;
    }

    /// <inheritdoc cref="IGorgonCommonBufferInfo.IsUnorderedAccess"/>
    /// <remarks>
    /// The default value is <b>false</b>.
    /// </remarks>
    public bool IsUnorderedAccess
    {
        get;
        init;
    } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonCommonBufferInfo"/> class.
    /// </summary>
    /// <param name="sizeInBytes">The size of the buffer, in bytes.</param>
    /// <param name="usage">The intended usage for the buffer.</param>
    protected GorgonCommonBufferInfo(long sizeInBytes, BufferUsage usage)
    {
        SizeInBytes = sizeInBytes;
        Usage = usage;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonCommonBufferInfo"/> class.
    /// </summary>
    /// <param name="copy">The record to copy.</param>
    protected GorgonCommonBufferInfo(GorgonCommonBufferInfo copy)
    {
        SizeInBytes = copy.SizeInBytes;
        Usage = copy.Usage;
        IsUnorderedAccess = copy.IsUnorderedAccess;
    }
}

