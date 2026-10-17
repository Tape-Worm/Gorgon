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
/// Information that was used to build a <see cref="GorgonExecuteBuffer"/>.
/// </summary>
public interface IGorgonExecuteBufferInfo
    : IGorgonCommonBufferInfo
{
    /// <summary>
    /// Property to return the number of commands the buffer holds.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value must be greater than 0.
    /// </para>
    /// </remarks>
    int CommandCount
    {
        get;
    }

    /// <summary>
    /// Property to return the size, in bytes, of a single command.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value must be a multiple of 4, and must be at least 16 bytes. It must also be at least the minimum size of a command for the execute call that uses the buffer: 4 bytes for each root constant the 
    /// commands set, plus 16 bytes for draw arguments, or 20 bytes for indexed draw arguments. A larger size leaves space after the arguments in each command, which the application can use for its own data.
    /// </para>
    /// </remarks>
    int CommandSizeInBytes
    {
        get;
    }

    /// <summary>
    /// Property to return the number of counters the buffer holds, after the commands.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each counter is a 32-bit unsigned integer that a shader uses to store the number of commands to execute. Counters are only used when the commands are written by shaders on the GPU (see 
    /// <see cref="ExecuteUsage.GpuRendering"/>), and an execute call created for GPU rendering needs at least 1. This value cannot be negative.
    /// </para>
    /// </remarks>
    int CounterCount
    {
        get;
    }
}
