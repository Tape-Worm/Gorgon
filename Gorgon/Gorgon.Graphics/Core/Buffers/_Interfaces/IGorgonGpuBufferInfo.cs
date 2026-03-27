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
// Created: January 14, 2026 9:41:53 PM
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Information that was used to build a <see cref="GorgonGpuBuffer_OLDE"/>.
/// </summary>
public interface IGorgonGpuBufferInfo
{
    /// <summary>
    /// Property to return the size of the buffer, in bytes.
    /// </summary>
    long SizeInBytes
    {
        get;
    }

    /// <summary>
    /// Property to return whether the buffer can be used as a render target.
    /// </summary>
    bool IsRenderTarget
    {
        get;
    }

    /// <summary>
    /// Property to return whether the buffer can be used as an unordered access resource.
    /// </summary>
    bool IsUnorderedAccess
    {
        get;
    }

    /// <summary>
    /// Property to return whether the buffer can be used as a constant buffer.
    /// </summary>
    [Obsolete("We may not need this going forward.")]
    bool IsConstantBuffer
    {
        get;
    }

    /// <summary>
    /// Property to return the intended usage for the buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value can be one of <see cref="BufferUsage.Default"/>, <see cref="BufferUsage.Upload"/>, <see cref="BufferUsage.DynamicPerFrame"/>. These usages correspond to the update frequency of the 
    /// underlying buffer. 
    /// <list type="table">
    ///     <listheader>
    ///         <term>Usage</term>
    ///         <term>Use Case</term>
    ///     </listheader>
    ///     <item>
    ///         <description><see cref="BufferUsage.Default"/></description>
    ///         <description>Writes to the buffer should only be done very infrequently, or one time only. Writing more than once per frame will only use the last write to the buffer when rendering.</description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="BufferUsage.Upload"/></description>
    ///         <description>Writes can only be done once per frame or less. Writing more than once per frame will only use the last write to the buffer when rendering.</description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="BufferUsage.DynamicPerFrame"/></description>
    ///         <description>Writes can be done multiple times per frame. Writes will be committed per draw in a command list.</description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="BufferUsage.Download"/></description>
    ///         <description>The data in the buffer is meant to be read, and never written to by the CPU. Not all buffer views support this usage type.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    BufferUsage Usage
    {
        get;
    }

    /// <summary>
    /// Property to return the number of bytes to align the buffer by.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is used to pad the buffer alignment so that it, and its data, starts on a power of 2 boundary.
    /// </para>
    /// <para>
    /// Buffers used in a <see cref="GorgonStructuredBufferView"/> should set this value to the size of the data element within the buffer, especially if the element size is not a power of 2.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// For buffers used a <see cref="GorgonConstantBufferView"/>, this value <b>MUST</b> be set to 256.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    int Alignment
    {
        get;
    }
}