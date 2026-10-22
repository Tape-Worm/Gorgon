//
// Gorgon
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
// Created: October 22, 2026 3:47:14 PM
//

using System.Runtime.InteropServices;

namespace Gorgon.Examples;

/// <summary>
/// A single command in the execute buffer.
/// </summary>
/// <remarks>
/// The execute constants come first, followed by the arguments for the draw.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
internal struct SliceCommand
{
    /// <summary>
    /// The index of the slice to draw. This is sent to the shaders as an execute constant.
    /// </summary>
    public uint SliceIndex;
    /// <summary>
    /// The number of vertices to draw.
    /// </summary>
    public uint VertexCount;
    /// <summary>
    /// The number of instances to draw.
    /// </summary>
    public uint InstanceCount;
    /// <summary>
    /// The first vertex to draw.
    /// </summary>
    public uint StartVertex;
    /// <summary>
    /// The first instance to draw.
    /// </summary>
    public uint StartInstance;
}
