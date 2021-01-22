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
// Created: January 14, 2021 10:29:24 PM
// 
#endregion

using System.Numerics;
using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.Math
{
    /// <summary>
    /// A bounding sphere used to determine the extent of an object in spherical space.
    /// </summary>
    public readonly struct GorgonBoundingSphere
        : IGorgonEquatableByRef<GorgonBoundingSphere>
    {
        #region Variables.
        /// <summary>
        /// An empty instance of the bounding sphere.
        /// </summary>
        public static readonly GorgonBoundingSphere Empty = default;

        /// <summary>
        /// The center of the sphere.
        /// </summary>
        public readonly Vector3 Center;
        /// <summary>
        /// The radius of the sphere.
        /// </summary>
        public readonly float Radius;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether this bounding sphere is empty or not.
        /// </summary>
        public bool IsEmpty => Radius.EqualsEpsilon(0);
        #endregion

        #region Methods.
        /// <summary>Returns a <see cref="string" /> that represents this instance.</summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString() => string.Format(Resources.GOR_TOSTR_BOUNDING_SPHERE, Center.X, Center.Y, Center.Z, Radius);

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() => 281.GenerateHash(Center).GenerateHash(Radius);

        /// <summary>Function to compare this instance with another.</summary>
        /// <param name="other">The other instance to use for comparison.</param>
        /// <returns>
        ///   <b>true</b> if equal, <b>false</b> if not.</returns>
        public bool Equals(in GorgonBoundingSphere other) => (Center.Equals(other.Center)) && (Radius == other.Radius);

        /// <summary>Determines whether the specified <see cref="object" /> is equal to this instance.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj) => (obj is GorgonBoundingSphere sphere) ? sphere.Equals(in this) : base.Equals(obj);

        /// <summary>Function to compare this instance with another.</summary>
        /// <param name="other">The other instance to use for comparison.</param>
        /// <returns>
        ///   <b>true</b> if equal, <b>false</b> if not.</returns>
        public bool Equals(GorgonBoundingSphere other) => Equals(in other);

        /// <summary>
        /// Operator to determine if two instances are equal.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if both instances are equal, <b>false</b> if not.</returns>
        public static bool operator ==(in GorgonBoundingSphere left, in GorgonBoundingSphere right) => left.Equals(in right);

        /// <summary>
        /// Operator to determine if two instances are not equal.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if both instances are not equal, <b>false</b> if they are.</returns>
        public static bool operator !=(in GorgonBoundingSphere left, in GorgonBoundingSphere right) => !left.Equals(in right);
        
        /// <summary>Deconstructs this instance.</summary>
        /// <param name="center">The center of the sphere.</param>
        /// <param name="radius">The radius of the sphere.</param>
        public void Deconstruct(out Vector3 center, out float radius)
        {
            center = Center;
            radius = Radius;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="GorgonBoundingSphere" /> struct.</summary>
        /// <param name="center">The center of sphere.</param>
        /// <param name="radius">The radius of the sphere.</param>
        public GorgonBoundingSphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
        #endregion
    }
}
