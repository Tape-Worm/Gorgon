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
// Created: June 25, 2026 12:15:44 AM
//

using System.Diagnostics.CodeAnalysis;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines parameters for drawing indexed primivites.
/// </summary>
public sealed class GorgonIndexedDrawCall
    : GorgonDrawCallCommon
{
    /// <summary>
    /// Property to return the index buffer used to draw.
    /// </summary>
    public required GorgonIndexBuffer IndexBuffer
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the starting location of the first index read by the GPU in the index buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public int StartIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the value added to each index value before reading from the vertex buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public int BaseVertex
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the number of indices to read from the index buffer for each instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This requires that the <see cref="IndexBuffer"/> is not <b>null</b>.
    /// </para>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonCommandList"/>
    /// <seealso cref="GorgonIndexBuffer"/>
    public required int IndexCount
    {
        get;
        set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonIndexedDrawCall"/> class.
    /// </summary>
    /// <param name="indexCount">The number of indices to draw.</param>
    /// <param name="indexBuffer">The index buffer to use when drawing.</param>
    /// <param name="pso">The graphics pipeline state object to use when drawing.</param>
    [SetsRequiredMembers]
    public GorgonIndexedDrawCall(int indexCount, GorgonIndexBuffer indexBuffer, GorgonGraphicsPso pso)
    {
        IndexCount = indexCount;
        IndexBuffer = indexBuffer;
        Pso = pso;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonIndexedDrawCall"/> class.
    /// </summary>
    public GorgonIndexedDrawCall()
    {
    }
}
