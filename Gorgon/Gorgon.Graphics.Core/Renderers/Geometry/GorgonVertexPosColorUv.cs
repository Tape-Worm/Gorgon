#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: May 31, 2018 1:14:58 PM
// 
#endregion

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;

namespace Gorgon.Renderers.Geometry;

/// <summary>
/// A vertex with a position, diffuse color, and UV texture coordinate.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct GorgonVertexPosColorUv
{
    #region Variables.
    /// <summary>
    /// The size of the vertex, in bytes.
    /// </summary>
    public static readonly int SizeInBytes = Unsafe.SizeOf<GorgonVertexPosColorUv>();

    /// <summary>
    /// The position of the vertex.
    /// </summary>
    [InputElement(0, "SV_POSITION")]
    public Vector4 Position;

    /// <summary>
    /// The color of the vertex.
    /// </summary>
    [InputElement(2, "COLOR")]
    public GorgonColor Color;

    /// <summary>
    /// The texture coordinate for the vertex.
    /// </summary>
    [InputElement(1, "TEXCOORD")]
    public Vector2 UV;
    #endregion

    #region Methods.
    /// <summary>
    /// Deconstructs this instance into individual position values.
    /// </summary>
    /// <param name="x">The X coordinate of the <see cref="Position"/>.</param>
    /// <param name="y">The Y coordinate of the <see cref="Position"/>.</param>
    /// <param name="z">The Z coordinate of the <see cref="Position"/>.</param>
    public readonly void Deconstruct(out float x, out float y, out float z)
    {
        x = Position.X;
        y = Position.Y;
        z = Position.Z;
    }

    /// <summary>Deconstructs this instance into a tuple.</summary>
    /// <param name="position">The position.</param>
    /// <param name="color">The color.</param>
    /// <param name="uv">The texture coordinate.</param>
    public readonly void Deconstruct(out Vector4 position, out GorgonColor color, out Vector2 uv)
    {
        position = Position;
        color = Color;
        uv = UV;
    }
    #endregion

    #region Constructor.
    /// <summary>Initializes a new instance of the <see cref="GorgonVertexPosColorUv" /> struct.</summary>
    /// <param name="pos">The position.</param>
    /// <param name="color">The diffuse color for the vertex.</param>
    /// <param name="uv">The texture coordinates.</param>
    public GorgonVertexPosColorUv(Vector3 pos, GorgonColor color, Vector2 uv)
    {
        Position = new Vector4(pos, 1);
        UV = uv;
        Color = color;
    }

    /// <summary>Initializes a new instance of the <see cref="GorgonVertexPosColorUv" /> struct.</summary>
    /// <param name="pos">The position.</param>
    /// <param name="color">The diffuse color for the vertex.</param>
    /// <param name="uv">The texture coordinates.</param>
    public GorgonVertexPosColorUv(Vector4 pos, GorgonColor color, Vector2 uv)
    {
        Position = pos;
        UV = uv;
        Color = color;
    }
    #endregion
}
