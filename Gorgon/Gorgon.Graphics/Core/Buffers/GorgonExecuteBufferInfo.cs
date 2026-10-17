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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// Created: October 9, 2026 11:49:15 PM
//

namespace Gorgon.Graphics.Core;

/// <summary>
/// Settings used for creating a <see cref="GorgonExecuteBuffer"/>.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="GorgonCommonBufferInfo.SizeInBytes"/> is calculated from the <see cref="CommandCount"/>, <see cref="CommandSizeInBytes"/> and <see cref="CounterCount"/>.
/// </para>
/// </remarks>
public record class GorgonExecuteBufferInfo
    : GorgonCommonBufferInfo
{
    // Recalculates the total size whenever one of the values that make it up changes.
    private long CalculateSizeInBytes() => ((long)CommandCount * CommandSizeInBytes) + ((long)CounterCount * sizeof(uint));

    /// <inheritdoc cref="IGorgonExecuteBufferInfo.CommandCount"/>
    public int CommandCount
    {
        get;
        init
        {
            field = value;
            SizeInBytes = CalculateSizeInBytes();
        }
    }

    /// <inheritdoc cref="IGorgonExecuteBufferInfo.CommandSizeInBytes"/>
    public int CommandSizeInBytes
    {
        get;
        init
        {
            field = value;
            SizeInBytes = CalculateSizeInBytes();
        }
    }

    /// <inheritdoc cref="IGorgonExecuteBufferInfo.CounterCount" path="/summary"/>
    /// <remarks>
    /// <inheritdoc cref="IGorgonExecuteBufferInfo.CounterCount" path="/remarks/para"/>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public int CounterCount
    {
        get;
        init
        {
            field = value;
            SizeInBytes = CalculateSizeInBytes();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonExecuteBufferInfo"/> class.
    /// </summary>
    /// <param name="info">The buffer creation information to copy.</param>
    public GorgonExecuteBufferInfo(GorgonExecuteBufferInfo info)
        : base(info)
    {
        CommandCount = info.CommandCount;
        CommandSizeInBytes = info.CommandSizeInBytes;
        CounterCount = info.CounterCount;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonExecuteBufferInfo"/> class.
    /// </summary>
    /// <param name="info">The buffer creation information to copy.</param>
    public GorgonExecuteBufferInfo(IGorgonExecuteBufferInfo info)
        : this(info.CommandCount, info.CommandSizeInBytes)
    {
        CounterCount = info.CounterCount;
        HasReadWriteAccess = info.HasReadWriteAccess;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonExecuteBufferInfo"/> class.
    /// </summary>
    /// <param name="commandCount">The number of commands the buffer holds.</param>
    /// <param name="commandSizeInBytes">The size, in bytes, of a single command.</param>
    public GorgonExecuteBufferInfo(int commandCount, int commandSizeInBytes)
        : base(0)
    {
        CommandCount = commandCount;
        CommandSizeInBytes = commandSizeInBytes;
    }
}
