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
/// Information common to multiple buffer types.
/// </summary>
public interface IGorgonCommonBufferInfo
{
    /// <summary>
    /// Property to return the size of the buffer, in bytes.
    /// </summary>
    long SizeInBytes
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
    /// Property to return the intended usage for the buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value can be one of <see cref="BufferUsage.Default"/>, or <see cref="BufferUsage.DynamicPerFrame"/>. These usages correspond to the update frequency of the 
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
    ///         <description><see cref="BufferUsage.DynamicPerFrame"/></description>
    ///         <description>Writes can be done multiple times per frame. Writes will be committed per draw in a command list.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    BufferUsage Usage
    {
        get;
    }
}