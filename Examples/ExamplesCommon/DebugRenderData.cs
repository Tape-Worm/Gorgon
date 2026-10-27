#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: January 15, 2021 2:48:43 PM
// 
#endregion

using System.Numerics;
using System.Runtime.InteropServices;

namespace Gorgon.Examples;

/// <summary>
/// The render data to pass to the GPU.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct DebugRenderData
{
    /// <summary>
    /// The view projection for the debug visualization.
    /// </summary>
    public Matrix4x4 ViewProjection;
    /// <summary>
    /// The handle to the vertex buffer containing the vertices for the visualization.
    /// </summary>
    public int VertexBufferHandle;
}
