#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Wednesday, September 18, 2013 8:26:39 PM
// 
#endregion

using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Renderers.Properties;
using SlimMath;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A point for a polygon or triangle.
    /// </summary>
    /// <remarks>
    /// Points in a triangle or polygon use relative coordinates from the upper left corner of the object, that is, they are defined as an offset from the position of the triangle.  
    /// So passing a point of 30x50 with a position of 70x50 and an anchor of 0x0 will create that point 100 units to the right, and 100 units down.
    /// </remarks>
    public struct GorgonPolygonPoint
        : IEquatableByRef<GorgonPolygonPoint>
    {
        /// <summary>
        /// Position of the point.
        /// </summary>
        /// <remarks>The position is relative.</remarks>
        public readonly Vector2 Position;
        /// <summary>
        /// The texture coordinate of the point.
        /// </summary>
        /// <remarks>This texture value is in texel space (0..1).</remarks>
        public readonly Vector2 TextureCoordinate;
        /// <summary>
        /// The color of the point.
        /// </summary>
        public readonly GorgonColor Color;

        /// <summary>
        /// Function to convert a polygon point to a Gorgon2D vertex type.
        /// </summary>
        /// <param name="point">Point to convert.</param>
        /// <returns>The converted vertex.</returns>
        public static Gorgon2DVertex ToGorgon2DVertex(ref GorgonPolygonPoint point)
        {
            return new Gorgon2DVertex
            {
                Color = point.Color,
                Position = new Vector4(point.Position, 0, 1),
                UV = point.TextureCoordinate
            };
        }

        /// <summary>
        /// Function to convert a Gorgon2D vertex to a polygon point.
        /// </summary>
        /// <param name="vertex">Vertex to convert.</param>
        /// <returns>The converted point.</returns>
        public static GorgonPolygonPoint FromGorgon2DVertex(ref Gorgon2DVertex vertex)
        {
            return new GorgonPolygonPoint((Vector2)vertex.Position, vertex.Color, vertex.UV);
        }

        /// <summary>
        /// Explicit conversion for a polygon point to a Gorgon2D vertex.
        /// </summary>
        /// <param name="vertex">Vertex to convert.</param>
        /// <returns>The converted point.</returns>
        public static explicit operator GorgonPolygonPoint(Gorgon2DVertex vertex)
        {
            return new GorgonPolygonPoint((Vector2)vertex.Position, vertex.Color, vertex.UV);
        }

        /// <summary>
        /// Implicit conversion for a Gorgon 2D vertex and a gorgon polygon point.
        /// </summary>
        /// <param name="point">Point to convert.</param>
        /// <returns>The converted vertex.</returns>
        public static implicit operator Gorgon2DVertex(GorgonPolygonPoint point)
        {
            return new Gorgon2DVertex
                   {
                       Color = point.Color,
                       Position = new Vector4(point.Position, 0, 1),
                       UV = point.TextureCoordinate
                   };
        }

        /// <summary>
        /// Function to determine if two instances are equal.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><c>true</c> if equal, <c>false</c> if not.</returns>
        public static bool Equals(ref GorgonPolygonPoint left, ref GorgonPolygonPoint right)
        {
	        // ReSharper disable CompareOfFloatsByEqualityOperator
            return left.Position.X == right.Position.X && left.Position.Y == right.Position.Y
                   && left.Color.Equals(right.Color)
                   && left.TextureCoordinate.X == right.TextureCoordinate.X
                   && left.TextureCoordinate.Y == right.TextureCoordinate.Y;
			// ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is GorgonPolygonPoint)
            {
                ((GorgonPolygonPoint)obj).Equals(ref this);
            }

            return base.Equals(obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return 281.GenerateHash(Position).GenerateHash(Color).GenerateHash(TextureCoordinate);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(Resources.GOR2D_POLY_POINT_TOSTR,
                                 Position.X,
                                 Position.Y,
                                 Color.Red,
                                 Color.Green,
                                 Color.Blue,
                                 Color.Alpha,
                                 TextureCoordinate.X,
                                 TextureCoordinate.Y);
        }

        /// <summary>
        /// Operator to compare two instances for equality.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><c>true</c> if equal, <c>false</c> if not.</returns>
        public static bool operator ==(GorgonPolygonPoint left, GorgonPolygonPoint right)
        {
            return Equals(ref left, ref right);
        }

        /// <summary>
        /// Operator to compare two instances for inequality.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><c>true</c> if not equal, <c>false</c> if equal.</returns>
        public static bool operator !=(GorgonPolygonPoint left, GorgonPolygonPoint right)
        {
            return !Equals(ref left, ref right);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonPolygonPoint"/> struct.
        /// </summary>
        /// <param name="position">The position of the point.</param>
        /// <param name="color">Color for the point.</param>
        /// <param name="textureCoordinate">The texture coordinate for the point.</param>
        public GorgonPolygonPoint(Vector2 position, GorgonColor color, Vector2 textureCoordinate)
        {
            Position = position;
            Color = color;
            TextureCoordinate = textureCoordinate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonPolygonPoint"/> struct.
        /// </summary>
        /// <param name="position">The position of the point.</param>
        /// <param name="color">Color for the point.</param>
        public GorgonPolygonPoint(Vector2 position, GorgonColor color)
        {
            Position = position;
            Color = color;
            TextureCoordinate = Vector2.Zero;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonPolygonPoint"/> struct.
        /// </summary>
        /// <param name="position">The position of the point.</param>
        public GorgonPolygonPoint(Vector2 position)
        {
            Position = position;
            Color = new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f);
            TextureCoordinate = Vector2.Zero;
        }

        #region IEquatableByRef<GorgonTrianglePoint> Members
        /// <summary>
        /// Function to determine if this instance is equal to another by reference.
        /// </summary>
        /// <param name="other">The other instance to compare against this one.</param>
        /// <returns><c>true</c> if equal, <c>false</c> if not.</returns>
        public bool Equals(ref GorgonPolygonPoint other)
        {
            return Equals(ref this, ref other);
        }
        #endregion

        #region IEquatable<GorgonTrianglePoint> Members
        /// <summary>
        /// Function to determine if this instance is equal to another by value.
        /// </summary>
        /// <param name="other">The other instance to compare against this one.</param>
        /// <returns><c>true</c> if equal, <c>false</c> if not.</returns>
        public bool Equals(GorgonPolygonPoint other)
        {
            return Equals(ref this, ref other);
        }
        #endregion
    }
}