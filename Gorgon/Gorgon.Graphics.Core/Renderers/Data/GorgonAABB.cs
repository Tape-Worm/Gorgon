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
// Created: January 14, 2021 12:30:56 PM
// 
#endregion

using System.Numerics;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Math;
using Gorgon.Graphics.Core.Properties;
using System.Diagnostics;

namespace Gorgon.Renderers.Data
{
    /// <summary>
    /// An axis-aligned bounding box used to determine the extents of a volume.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct GorgonAABB
        : IGorgonEquatableByRef<GorgonAABB>
    {
        #region Variables.
        /// <summary>
        /// Property to set or return the minimum bounding area.
        /// </summary>
        public readonly Vector3 Min;

        /// <summary>
        /// Property to set or return the maximum bounding area.
        /// </summary>
        public readonly Vector3 Max;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the left extent.
        /// </summary>
        public float Left => Min.X;

        /// <summary>
        /// Property to return the top extent.
        /// </summary>
        public float Top => Min.Y;

        /// <summary>
        /// Property to return the right extent.
        /// </summary>
        public float Right => Max.X;

        /// <summary>
        /// Property to return the bottom extent.
        /// </summary>
        public float Bottom => Max.Y;

        /// <summary>
        /// Property to return the front extent.
        /// </summary>
        public float Front => Min.Z;

        /// <summary>
        /// Property to return the back extent.
        /// </summary>
        public float Back => Max.Z;

        /// <summary>
        /// Property to return the width of the box.
        /// </summary>
        public float Width => (Max.X - Min.X).Abs();

        /// <summary>
        /// Property to return the height of the box.
        /// </summary>
        public float Height => (Max.Y - Min.Y).Abs();

        /// <summary>
        /// Property to return the depth of the box.
        /// </summary>
        public float Depth => (Max.Z - Min.Z).Abs();

        /// <summary>
        /// Property to return the top left point at the front of the AABB.
        /// </summary>
        public Vector3 TopLeftFront => Min;

        /// <summary>
        /// Property to return the top right point at the front of the AABB.
        /// </summary>
        public Vector3 TopRightFront => new Vector3(Max.X, Min.Y, Min.Z);

        /// <summary>
        /// Property to return the bottom left point at the front of the AABB.
        /// </summary>
        public Vector3 BottomLeftFront => new Vector3(Min.X, Max.Y, Min.Z);

        /// <summary>
        /// Property to return the bottom right point at the front of the AABB.
        /// </summary>
        public Vector3 BottomRightFront => new Vector3(Max.X, Max.Y, Min.Z);

        /// <summary>
        /// Property to return the top left point at the rear of the AABB.
        /// </summary>
        public Vector3 TopLeftBack => new Vector3(Min.X, Min.Y, Max.Z);

        /// <summary>
        /// Property to return the top right point at the rear of the AABB.
        /// </summary>
        public Vector3 TopRightBack => new Vector3(Max.X, Min.Y, Max.Z);

        /// <summary>
        /// Property to return the bottom left point at the rear of the AABB.
        /// </summary>
        public Vector3 BottomLeftBack => new Vector3(Min.X, Max.Y, Max.Z);

        /// <summary>
        /// Property to return the bottom right point at the rear of the AABB.
        /// </summary>
        public Vector3 BottomRightBack => Max;

        /// <summary>
        /// Property to return whether the AABB is empty or not.
        /// </summary>
        public bool IsEmpty => (Width == 0) || (Height == 0) || (Depth == 0);

        /// <summary>
        /// Property to return each of the 8 corners for the AABB.
        /// </summary>
        public Vector3 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Min;
                    case 1:
                        return TopRightFront;
                    case 2:
                        return BottomLeftFront;
                    case 3:
                        return BottomRightFront;
                    case 4:
                        return TopLeftBack;
                    case 5:
                        return TopRightBack;
                    case 6:
                        return BottomLeftBack;
                    case 7:
                        return Max;
                }

                return new Vector3(float.NaN);
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to transform an AABB by a world matrix.
        /// </summary>
        /// <param name="aabb">The axis aligned bounding box to transform.</param>
        /// <param name="worldMatrix">The world matrix to multiply by.</param>
        /// <param name="result">The new transformed axis aligned bounding box.</param>
        public static void Transform(in GorgonAABB aabb, in Matrix4x4 worldMatrix, out GorgonAABB result)
        {
            Matrix4x4 rot;
            rot.M11 = worldMatrix.M11;
            rot.M12 = worldMatrix.M12;
            rot.M13 = worldMatrix.M13;
            rot.M21 = worldMatrix.M21;
            rot.M22 = worldMatrix.M22;
            rot.M23 = worldMatrix.M23;
            rot.M31 = worldMatrix.M31;
            rot.M32 = worldMatrix.M32;
            rot.M33 = worldMatrix.M33;

            Vector3 max = worldMatrix.Translation;
            Vector3 min = worldMatrix.Translation;            

            float a1 = rot.M11 * aabb.Min.X;
            float b1 = rot.M11 * aabb.Max.X;
            float a2 = rot.M12 * aabb.Min.Y;
            float b2 = rot.M12 * aabb.Max.Y;
            float a3 = rot.M13 * aabb.Min.Z;
            float b3 = rot.M13 * aabb.Max.Z;
            float a4 = rot.M21 * aabb.Min.X;
            float b4 = rot.M21 * aabb.Max.X;
            float a5 = rot.M22 * aabb.Min.Y;
            float b5 = rot.M22 * aabb.Max.Y;
            float a6 = rot.M23 * aabb.Min.Z;
            float b6 = rot.M23 * aabb.Max.Z;
            float a7 = rot.M31 * aabb.Min.X;
            float b7 = rot.M31 * aabb.Max.X;
            float a8 = rot.M32 * aabb.Min.Y;
            float b8 = rot.M32 * aabb.Max.Y;
            float a9 = rot.M33 * aabb.Min.Z;
            float b9 = rot.M33 * aabb.Max.Z;

            min.X += a1.Min(b1);
            min.X += a4.Min(b4);
            min.X += a7.Min(b7);

            max.X += a1.Max(b1);
            max.X += a4.Max(b4);
            max.X += a7.Max(b7);

            min.Y += a2.Min(b2);
            min.Y += a5.Min(b5);
            min.Y += a8.Min(b8);

            max.Y += a2.Max(b2);
            max.Y += a5.Max(b5);
            max.Y += a8.Max(b8);

            min.Z += a3.Min(b3);
            min.Z += a6.Min(b6);
            min.Z += a9.Min(b9);

            max.Z += a3.Max(b3);
            max.Z += a6.Max(b6);
            max.Z += a9.Max(b9);

            result = new GorgonAABB(min, max);
        }

        /// <summary>Returns a <see cref="string" /> that represents this instance.</summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString() => string.Format(Resources.GORGFX_TOSTR_AABB, Min.X, Min.Y, Min.Z, Max.X, Max.Y, Max.Z);

        /// <summary>Returns a hash code for this instance.</summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() => 281.GenerateHash(Min).GenerateHash(Max);

        /// <summary>Determines whether the specified <see cref="object" /> is equal to this instance.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj) => obj is GorgonAABB other ? other.Equals(in this) : base.Equals(obj);

        /// <summary>Function to compare this instance with another.</summary>
        /// <param name="other">The other instance to use for comparison.</param>
        /// <returns>
        ///   <b>true</b> if equal, <b>false</b> if not.</returns>
        public bool Equals(in GorgonAABB other) => (Min.X == other.Min.X)
                && (Min.Y == other.Min.Y)
                && (Min.Z == other.Min.Z)
                && (Max.X == other.Max.X)
                && (Max.Y == other.Max.Y)
                && (Max.Z == other.Max.Z);

        /// <summary>Function to compare this instance with another.</summary>
        /// <param name="other">The other instance to use for comparison.</param>
        /// <returns>
        ///   <b>true</b> if equal, <b>false</b> if not.</returns>
        public bool Equals(GorgonAABB other) => Equals(in other);

        /// <summary>
        /// Operator to determine if two instances are equal.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool operator ==(in GorgonAABB left, in GorgonAABB right) => left.Equals(in right);

        /// <summary>
        /// Operator to determine if two instances are not equal.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
        public static bool operator !=(in GorgonAABB left, in GorgonAABB right) => !left.Equals(in right);

        /// <summary>Deconstructs this instance.</summary>
        /// <param name="min">The minimum extent.</param>
        /// <param name="max">The maximum extent.</param>
        public void Deconstruct(out Vector3 min, out Vector3 max)
        {
            min = Min;
            max = Max;
        }

        /// <summary>Deconstructs this instance.</summary>
        /// <param name="topLeftFront">The top left front corner.</param>
        /// <param name="topRightFront">The top right front corner.</param>
        /// <param name="bottomLeftFront">The bottom left front corner.</param>
        /// <param name="bottomRightFront">The bottom right front corner.</param>
        /// <param name="topLeftBack">The top left back corner.</param>
        /// <param name="topRightBack">The top right back corner.</param>
        /// <param name="bottomLeftBack">The bottom left back corner.</param>
        /// <param name="bottomRightBack">The bottom right back corner.</param>
        public void Deconstruct(out Vector3 topLeftFront, out Vector3 topRightFront, out Vector3 bottomLeftFront, out Vector3 bottomRightFront,
                                out Vector3 topLeftBack, out Vector3 topRightBack, out Vector3 bottomLeftBack, out Vector3 bottomRightBack)
        {
            topLeftFront = TopLeftFront;
            topRightFront = TopRightFront;
            bottomLeftFront = BottomLeftFront;
            bottomRightFront = BottomRightFront;
            topLeftBack = TopLeftBack;
            topRightBack = TopRightBack;
            bottomLeftBack = BottomLeftBack;
            bottomRightBack = BottomRightBack;
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="GorgonAABB" /> struct.</summary>
        /// <param name="minX">The minimum x.</param>
        /// <param name="minY">The minimum y.</param>
        /// <param name="minZ">The minimum z.</param>
        /// <param name="maxX">The maximum x.</param>
        /// <param name="maxY">The maximum y.</param>
        /// <param name="maxZ">The maximum z.</param>
        public GorgonAABB(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
        {
            Min = new Vector3(minX, minY, minZ);
            Max = new Vector3(maxX, maxY, maxZ);
        }

        /// <summary>Initializes a new instance of the <see cref="GorgonAABB" /> struct.</summary>
        /// <param name="min">The minimum extent.</param>
        /// <param name="max">The maximum extent.</param>
        public GorgonAABB(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }
        #endregion
    }
}
