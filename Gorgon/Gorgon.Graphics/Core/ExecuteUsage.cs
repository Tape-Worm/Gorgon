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
// Created: October 8, 2026 3:23:52 PM
//

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines how the commands for a <see cref="GorgonExecuteCall"/> or <see cref="GorgonIndexedExecuteCall"/> are written.
/// </summary>
/// <remarks>
/// <para>
/// With CPU rendering, the application decides what to draw. It writes the commands into a <see cref="GorgonExecuteBuffer"/> from the CPU, like any other buffer data, and sets the number of commands to 
/// execute with the <see cref="GorgonExecuteCallCommon.CommandExecutionCount"/> property. This sends many draws to the GPU in a single call when the application already knows what to draw.
/// </para>
/// <para>
/// With GPU rendering, a shader decides what to draw. It writes the commands, and a count of the commands it wrote, into the <see cref="GorgonExecuteBuffer"/> through read/write views. When the call is 
/// executed, the GPU reads the count that the shader wrote, so the results never travel back to the CPU. This lets the GPU do work such as culling, and draw only what is left, without waiting on the CPU. The 
/// application only sets the maximum number of commands to execute, and the count written by the shader decides how many of those are executed.
/// </para>
/// </remarks>
/// <seealso cref="GorgonExecuteCall"/>
/// <seealso cref="GorgonIndexedExecuteCall"/>
/// <seealso cref="GorgonExecuteBuffer"/>
public enum ExecuteUsage
{
    /// <summary>
    /// The application writes the commands on the CPU, and decides how many of them are executed.
    /// </summary>
    CpuRendering = 0,
    /// <summary>
    /// A shader writes the commands, and the number of commands to execute, on the GPU.
    /// </summary>
    GpuRendering = 1
}
