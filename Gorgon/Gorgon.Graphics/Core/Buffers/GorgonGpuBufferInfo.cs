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
/// Settings used for creating a <see cref="GorgonGpuBuffer"/>.
/// </summary>
/// <param name="SizeInBytes">The size of the buffer, in bytes.</param>
/// <param name="Usage">The intended usage for the buffer.</param>
/// <remarks>
/// <para>
/// The <paramref name="SizeInBytes"/> parameter must be greater than 0.
/// </para>
/// </remarks>
public record class GorgonGpuBufferInfo(long SizeInBytes, BufferUsage Usage)
    : GorgonCommonBufferInfo(SizeInBytes, Usage)
{
    /// <summary>
    /// An empty instance of the <see cref="GorgonGpuBuffer"/> type.
    /// </summary>
    public static readonly GorgonGpuBufferInfo Empty = new(0, BufferUsage.Default);

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
    /// Initializes a new instance of the <see cref="GorgonGpuBufferInfo"/> class.
    /// </summary>
    /// <param name="info">The buffer creation information to copy.</param>
    public GorgonGpuBufferInfo(GorgonGpuBufferInfo info)
        : base(info)
    {
        IsUnorderedAccess = info.IsUnorderedAccess;
        Alignment = info.Alignment;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IGorgonGpuBufferInfo"/> class.
    /// </summary>
    /// <param name="info">The buffer creation information to copy.</param>
    public GorgonGpuBufferInfo(IGorgonGpuBufferInfo info)
        : this(info.SizeInBytes, info.Usage)
    {
        IsUnorderedAccess = info.IsUnorderedAccess;
        Alignment = info.Alignment;
    }
}

