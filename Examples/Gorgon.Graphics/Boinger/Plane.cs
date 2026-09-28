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
// Created: July 30, 2026 11:15:23 PM
//

using System.Numerics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;

namespace Gorgon.Examples;

/// <summary>
/// A plane object.
/// </summary>
internal class Plane
    : Model
{
    /// <summary>
    /// Property to return the size of the plane.
    /// </summary>
    public Vector2 Size
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Plane" /> class.
    /// </summary>
    /// <param name="graphics">The graphics interface used to create the buffers for this object.</param>
    /// <param name="size">The width and height of the plane.</param>
    /// <param name="textureCoordinates">Texture coordinates.</param>
    public Plane(GorgonGraphics graphics,Vector2 size, GorgonRectangleF textureCoordinates)
    {
        Size = size;

        // Create our vertices.
        Vertices = [
                       new Vertex() { Position = new Vector4(-size.X, size.Y, 0.0f, 1), UV = new Vector2(textureCoordinates.Left, textureCoordinates.Top) },
                       new Vertex() { Position = new Vector4(size.X, size.Y, 0.0f, 1), UV = new Vector2(textureCoordinates.Right, textureCoordinates.Top) },
                       new Vertex() { Position = new Vector4(size.X, -size.Y, 0.0f, 1), UV = new Vector2(textureCoordinates.Right, textureCoordinates.Bottom) },
                       new Vertex() { Position = new Vector4(-size.X, -size.Y, 0.0f, 1), UV = new Vector2(textureCoordinates.Left, textureCoordinates.Bottom) }
                   ];

        // Create our indices.
        Indices = [
                      0,
                      1,
                      2,
                      2,
                      3,
                      0
                  ];

        VertexBufferView = GorgonStructuredBufferView.CreateStructuredBuffer<Vertex>(graphics, "Boinger plane vertex buffer", Vertices.Length);
        IndexBuffer = new GorgonIndexBuffer(graphics, "Boinger plane index buffer", new GorgonIndexBufferInfo(Indices.Length * sizeof(ushort), false));
    }
}
