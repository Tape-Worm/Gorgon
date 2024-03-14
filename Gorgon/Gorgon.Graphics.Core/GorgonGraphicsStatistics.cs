
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: September 7, 2020 11:53:49 PM
// 


namespace Gorgon.Graphics.Core;

/// <summary>
/// Statistics gathered while rendering
/// </summary>
public struct GorgonGraphicsStatistics
{

    // The number of draw calls.
    internal long _drawCallCount;

    // The number of calls to clear render targets or depth buffer.
    internal long _clearCount;

    // The number of calls to clear the stencil buffer.
    internal long _stencilClearCount;

    // The number of triangles being drawn.
    internal long _triangleCount;

    // The number of indirect draw calls.
    internal long _indirectCount;

    // The number of stream out calls.
    internal long _streamOutCount;



    /// <summary>
    /// Property to return the number of calls to clear render targets or depth buffers.
    /// </summary>
    public readonly long ClearCount => _clearCount;

    /// <summary>
    /// Property to return the number of calls to clear a stencil buffer.
    /// </summary>
    public readonly long StencilClearCount => _stencilClearCount;

    /// <summary>
    /// Property to return the number of draw calls submitted.
    /// </summary>
    public readonly long DrawCallCount => _drawCallCount;

    /// <summary>
    /// Property to return the number of indirect calls submitted.
    /// </summary>
    public readonly long IndirectCount => _indirectCount;

    /// <summary>
    /// Property to return the number of stream out calls submitted.
    /// </summary>
    public readonly long StreamOutCount => _streamOutCount;

    /// <summary>
    /// Property to return the number of triangles submitted in the frame.
    /// </summary>
    public readonly long TriangleCount => _triangleCount;

}
