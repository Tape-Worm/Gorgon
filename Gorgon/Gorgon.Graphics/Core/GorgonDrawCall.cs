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
/// Defines parameters for drawing primivites.
/// </summary>
public sealed class GorgonDrawCall
    : GorgonDrawCallCommon
{
    /// <summary>
    /// Property to set or return the number of vertices to read from the vertex buffer for each instance.
    /// </summary>
    /// <seealso cref="GorgonCommandList"/>
    public required int VertexCount
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the starting location of the first vertex read by the GPU in the vertex buffer.
    /// </summary>
    public int StartVertex
    {
        get;
        set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonDrawCall"/> class.
    /// </summary>
    /// <param name="vertexCount">The number of vertices to draw.</param>
    /// <param name="pso">The graphics pipeline state object to use when drawing.</param>
    [SetsRequiredMembers]
    public GorgonDrawCall(int vertexCount, GorgonGraphicsPso pso)        
    {
        VertexCount = vertexCount;
        Pso = pso;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonDrawCall"/> class.
    /// </summary>
    public GorgonDrawCall()
    {
    }
}
