
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: March 2, 2017 7:36:41 PM
// 

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Gorgon.Examples;

/// <summary>
/// This represents a single vertex in our triangle
/// 
/// It will contain a position, and a texture coordinate. We have to specify the packing and the layout ordering so we can safely transfer the data from the managed 
/// environment of .NET into the unmanaged world of the GPU.
/// </summary>
/// <param name="position">The position of the vertex in object space.</param>
/// <param name="uv">The texture coordinate for the vertex.</param>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct MiniTriVertex(Vector4 position, Vector2 uv)
{
    /// <summary>
    /// This is the size of the vertex, in bytes. 
    /// 
    /// We'll need this to tell Direct 3D how many vertices are stored in a vertex buffer.
    /// </summary>
    public static int SizeInBytes = Unsafe.SizeOf<MiniTriVertex>();

    /// <summary>
    /// This will be the position of the vertex in object space.
    /// 
    /// Note that the member is decorated with an InputElement attribute. This tells the shader how to interpret the vertex data by the semantic provided in the string, and the order of the data
    /// as indicated by the integer parameter.
    /// </summary>
    public Vector4 Position = position;

    /// <summary>
    /// This is the texture coordinate for the vertex. It ranges from 0 to 1 to cover the whole size of the texture, where 0 is upper/left, and 1.0 is bottom/right.
    /// </summary>
    public Vector2 Uv = uv;
}
