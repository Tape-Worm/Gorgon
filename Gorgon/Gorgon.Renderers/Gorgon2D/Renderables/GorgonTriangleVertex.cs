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
// Created: July 2, 2018 10:29:39 AM
// 
#endregion

using System;
using System.Numerics;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Math;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A value that defines a vertex for a triangle draw using the <see cref="Gorgon2D.DrawTriangle"/> method.
    /// </summary>
    public readonly struct GorgonTriangleVertex
        : IGorgonEquatableByRef<GorgonTriangleVertex>
    {
        #region Variables.
        /// <summary>
        /// The horizontal and vertical position of the point.
        /// </summary>
        public readonly Vector2 Position;
        /// <summary>
        /// The texture coordinate, in texels, to map to the point. 
        /// </summary>
        /// <remarks>
        /// If no texture is assigned to the triangle, this member is ignored.
        /// </remarks>
        public readonly Vector2 TextureCoordinate;
        /// <summary>
        /// The texture array index to map to the point.
        /// </summary>
        /// <remarks>
        /// If no texture is assigned to the triangle, or the texture is not a texture array, then this member is ignored.
        /// </remarks>
        public readonly int TextureArrayIndex;
        /// <summary>
        /// The color of the point.
        /// </summary>
        public readonly GorgonColor Color;
        #endregion

        #region Methods.
        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode() => HashCode.Combine(Color, TextureArrayIndex, TextureCoordinate, Position);

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance. </param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />. </returns>
        public override bool Equals(object obj) => obj is GorgonTriangleVertex point ? point.Equals(in this) : base.Equals(obj);

        /// <summary>
        /// Function to compare two points for equality.
        /// </summary>
        /// <param name="left">The left point to compare.</param>
        /// <param name="right">The right point to compare.</param>
        /// <returns><b>true</b> if the points are equal, <b>false</b> if not.</returns>
        public static bool Equals(in GorgonTriangleVertex left, in GorgonTriangleVertex right) => GorgonColor.Equals(in left.Color, in right.Color)
                   && left.TextureArrayIndex == right.TextureArrayIndex
                   && left.TextureCoordinate.X.EqualsEpsilon(right.TextureCoordinate.X)
                   && left.TextureCoordinate.Y.EqualsEpsilon(right.TextureCoordinate.Y)
                   && left.Position.X.EqualsEpsilon(right.Position.X)
                   && left.Position.Y.EqualsEpsilon(right.Position.Y);

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(GorgonTriangleVertex other) => Equals(in this, in other);

        /// <summary>
        /// Function to compare this instance with another.
        /// </summary>
        /// <param name="other">The other instance to use for comparison.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public bool Equals(in GorgonTriangleVertex other) => Equals(in this, in other);

        /// <summary>
        /// Operator to determine if two instances are equal.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if both instances are equal, <b>false</b> if not.</returns>
        public static bool operator ==(in GorgonTriangleVertex left, in GorgonTriangleVertex right) => Equals(in left, in right);

        /// <summary>
        /// Operator to determine if two instances are not equal.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if both instances are not equal, <b>false</b> if equal.</returns>
        public static bool operator !=(in GorgonTriangleVertex left, in GorgonTriangleVertex right) => !Equals(in left, in right);
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTriangleVertex" /> struct.
        /// </summary>
        /// <param name="position">The position of the triangle point.</param>
        /// <param name="color">The color of the triangle pint.</param>
        /// <param name="textureCoordinate">[Optional] The texture coordinate to map to the point.</param>
        /// <param name="textureArrayIndex">[Optional] The index in a texture array to map to the point.</param>
        public GorgonTriangleVertex(Vector2 position, GorgonColor color, Vector2? textureCoordinate = null, int textureArrayIndex = 0)
        {
            Position = position;
            Color = color;
            TextureCoordinate = textureCoordinate ?? Vector2.Zero;
            TextureArrayIndex = textureArrayIndex;
        }
        #endregion

    }
}
