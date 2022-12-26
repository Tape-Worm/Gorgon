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
// Created: June 6, 2018 2:34:12 PM
// 
#endregion

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;

namespace Gorgon.Renderers.Geometry;

/// <summary>
/// A vertex used for 2D rendering.
/// </summary>		
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Gorgon2DVertex
{
    #region Variables.
    /// <summary>
    /// The size of the vertex, in bytes.
    /// </summary>
    public static readonly int SizeInBytes = Unsafe.SizeOf<Gorgon2DVertex>();

    /// <summary>
    /// Position of the vertex.
    /// </summary>
    [InputElement(0, "SV_POSITION")]
    public Vector4 Position;
    /// <summary>
    /// Color of the vertex.
    /// </summary>
    [InputElement(1, "COLOR")]
    public GorgonColor Color;
    /// <summary>
    /// Texture coordinates for the vertex.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>Z</c> component is used to pass an index into a texture array.
    /// </para>
    /// <para>
    /// The <c>W</c> component is used to handle perspective correct rendering of a quad.
    /// </para>
    /// </remarks>
    [InputElement(2, "TEXCOORD")]
    public Vector4 UV;
    /// <summary>
    /// The Cosine and Sine for the angle of rotation.
    /// </summary>
    [InputElement(3, "ANGLE")]
    public Vector2 Angle;
    #endregion

    #region Methods.
    /// <summary>
    /// Deconstructs this instance into individual position values.
    /// </summary>
    /// <param name="x">The X coordinate of the <see cref="Position"/>.</param>
    /// <param name="y">The Y coordinate of the <see cref="Position"/>.</param>
    /// <param name="z">The Z coordinate of the <see cref="Position"/>.</param>
    public void Deconstruct(out float x, out float y, out float z)
    {
        x = Position.X;
        y = Position.Y;
        z = Position.Z;
    }

    /// <summary>Deconstructs this instance into a tuple.</summary>
    /// <param name="position">The position.</param>
    /// <param name="color">The color.</param>
    /// <param name="uv">The texture coordinates.</param>
    /// <param name="angle">The angle.</param>
    public void Deconstruct(out Vector4 position, out GorgonColor color, out Vector4 uv, out Vector2 angle)
    {
        position = Position;
        color = Color;
        uv = UV;
        angle = Angle;
    }
    #endregion

    #region Constructor.
    /// <summary>Initializes a new instance of the <see cref="Gorgon2DVertex" /> struct.</summary>
    /// <param name="pos">The position.</param>
    /// <param name="color">The color.</param>
    /// <param name="uv">The texture coordinates.</param>
    /// <param name="angle">The angle expressed as the cosine (x) and sine (y) of the angle.</param>
    public Gorgon2DVertex(Vector2 pos, GorgonColor color, Vector4 uv, Vector2 angle)
    {
        Position = new Vector4(pos, 0, 1);
        Color = color;
        UV = uv;
        Angle = angle;
    }

    /// <summary>Initializes a new instance of the <see cref="Gorgon2DVertex" /> struct.</summary>
    /// <param name="pos">The position.</param>
    /// <param name="color">The color.</param>
    /// <param name="uv">The texture coordinates.</param>
    /// <param name="angle">The angle expressed as the cosine (x) and sine (y) of the angle.</param>
    public Gorgon2DVertex(Vector3 pos, GorgonColor color, Vector4 uv, Vector2 angle)
    {
        Position = new Vector4(pos, 1);
        Color = color;
        UV = uv;
        Angle = angle;
    }

    /// <summary>Initializes a new instance of the <see cref="Gorgon2DVertex" /> struct.</summary>
    /// <param name="pos">The position.</param>
    /// <param name="color">The color.</param>
    /// <param name="uv">The texture coordinates.</param>
    /// <param name="angle">The angle expressed as the cosine (x) and sine (y) of the angle.</param>
    public Gorgon2DVertex(Vector4 pos, GorgonColor color, Vector4 uv, Vector2 angle)
    {
        Position = pos;
        Color = color;
        UV = uv;
        Angle = angle;
    }
    #endregion
}
