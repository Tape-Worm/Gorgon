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
// Created: January 16, 2021 10:47:14 PM
// 
#endregion

using System.Numerics;
using System.Runtime.InteropServices;
using DX = SharpDX;
using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.Math
{
    /// <summary>
    /// Represents a 3 dimensional line composed of a starting point and a direction.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is adapted from the code in SharpDX (<a href="https://github.com/sharpdx/SharpDX">https://github.com/sharpdx/SharpDX</a>).
    /// </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public readonly struct GorgonRay
        : IGorgonEquatableByRef<GorgonRay>
    {
        #region Variables.
        /// <summary>
        /// The position in three dimensional space where the ray starts.
        /// </summary>
        public readonly Vector3 Position;

        /// <summary>
        /// The normalized direction in which the ray points.
        /// </summary>
        public readonly Vector3 Direction;
        #endregion

        #region Methods.
        /// <summary>Returns a <see cref="string" /> that represents this instance.</summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString() => string.Format(Resources.GOR_TOSTR_RAY, Position.X, Position.Y, Position.Z, Direction.X, Direction.Y, Direction.Z);

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() => 281.GenerateHash(Position).GenerateHash(Direction);

        /// <summary>Function to compare this instance with another.</summary>
        /// <param name="other">The other instance to use for comparison.</param>
        /// <returns>
        ///   <b>true</b> if equal, <b>false</b> if not.</returns>
        public bool Equals(in GorgonRay other) => (other.Position.Equals(Position)) && (other.Direction.Equals(Direction));

        /// <summary>Function to compare this instance with another.</summary>
        /// <param name="other">The other instance to use for comparison.</param>
        /// <returns>
        ///   <b>true</b> if equal, <b>false</b> if not.</returns>
        public bool Equals(GorgonRay other) => Equals(in other);

        /// <summary>Determines whether the specified <see cref="object" /> is equal to this instance.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj) => (obj is GorgonRay ray) ? ray.Equals(in this) : base.Equals(obj);

        /// <summary>
        /// Operator to determine if two rays are equal.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if the two instances are equal, <b>false</b> if not.</returns>
        public static bool operator ==(in GorgonRay left, in GorgonRay right) => left.Equals(in right);

        /// <summary>
        /// Operator to determine if two rays are not equal.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if the two instances are not equal, <b>false</b> if they are.</returns>
        public static bool operator !=(in GorgonRay left, in GorgonRay right) => !left.Equals(in right);

        /// <summary>
        /// Function to calculate a world space <see cref="GorgonRay"/> from 2d screen coordinates.
        /// </summary>
        /// <param name="x">X coordinate on 2d screen.</param>
        /// <param name="y">Y coordinate on 2d screen.</param>
        /// <param name="viewport">The viewport.</param>
        /// <param name="worldViewProjection">Transformation matrix.</param>
        /// <param name="result">The resulting ray.</param>
        public static void GetPickRay(int x, int y, in DX.ViewportF viewport, in Matrix4x4 worldViewProjection, out GorgonRay result)
        {
            var nearPoint = new Vector3(x, y, 0);
            var farPoint = new Vector3(x, y, 1);

            nearPoint.Unproject(viewport.X, viewport.Y, viewport.Width, viewport.Height, viewport.MinDepth, viewport.MaxDepth, in worldViewProjection, out nearPoint);
            farPoint.Unproject(viewport.X, viewport.Y, viewport.Width, viewport.Height, viewport.MinDepth, viewport.MaxDepth, in worldViewProjection, out farPoint);

            var direction = Vector3.Normalize(farPoint - nearPoint);

            result = new GorgonRay(nearPoint, direction);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRay"/> struct.
        /// </summary>
        /// <param name="position">The position in three dimensional space of the origin of the ray.</param>
        /// <param name="direction">The normalized direction of the ray.</param>
        public GorgonRay(Vector3 position, Vector3 direction)
        {
            Position = position;
            Direction = direction;
        }
        #endregion
    }
}
