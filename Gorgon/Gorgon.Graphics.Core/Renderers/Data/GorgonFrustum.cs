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
// Created: January 14, 2021 10:38:56 AM
// 
#endregion


using System;
using System.Collections.Generic;
using System.Numerics;
using System.Collections;

namespace Gorgon.Renderers.Data
{
    /// <summary>
    /// The identifiers for the frustum planes.
    /// </summary>
    public enum FrustumPlane
    {
        /// <summary>
        /// The near plane of the frustum.
        /// </summary>
        Near = 0,
        /// <summary>
        /// The far plane of the frustum.
        /// </summary>
        Far = 1,
        /// <summary>
        /// The left plane of the frustum.
        /// </summary>
        Left = 2,
        /// <summary>
        /// The right plane of the frustum.
        /// </summary>
        Right = 3,
        /// <summary>
        /// The top plane of the frustum.
        /// </summary>
        Top = 4,
        /// <summary>
        /// The bottom plane of the frustum.
        /// </summary>
        Bottom = 5
    }

    /// <summary>
    /// The identifiers for the frustum corners.
    /// </summary>
    public enum FrustumCorner
    {
        /// <summary>
        /// Top left near corner.
        /// </summary>
        TopLeftNear = 0,
        /// <summary>
        /// Top right near corner.
        /// </summary>
        TopRightNear = 1,
        /// <summary>
        /// Bottom right near corner.
        /// </summary>
        BottomRightNear = 2,
        /// <summary>
        /// Bottom left near corner.
        /// </summary>
        BottomLeftNear = 3,
        /// <summary>
        /// Top left far corner.
        /// </summary>
        TopLeftFar = 4,
        /// <summary>
        /// Top right far corner.
        /// </summary>
        TopRightFar = 5,
        /// <summary>
        /// Bottom right far corner.
        /// </summary>
        BottomRightFar = 6,
        /// <summary>
        /// Bottom left far corner.
        /// </summary>
        BottomLeftFar = 7
    }

    /// <summary>
    /// A result indicating whether a bounding area is completely inside the view, partially inside, or not at all in the view.
    /// </summary>
    public enum IntersectionResult
    {
        /// <summary>
        /// Area is outside of the view.
        /// </summary>
        Outside = 0,
        /// <summary>
        /// Area is completely within the view.
        /// </summary>
        Full = 1,
        /// <summary>
        /// Area is partially inside the view.
        /// </summary>
        Partial = 2
    }

    /// <summary>
    /// A view frustum used to help cull objects from a view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A frustum is used to determine the extents of a view by extracting the near, far, left, right, top and bottom planes of a view volume. These planes can then be used to determine if an 
    /// object is within the view. This allows an application to cull out objects that are not visible in the view, and increase performance.
    /// </para>
    /// </remarks>
    public class GorgonFrustum
    {
        #region Classes.
        /// <summary>
        /// The list of corners.
        /// </summary>
        public class CornerList
            : IEnumerable<Vector3>
        {
            #region Variables.
            // The vectors indicating the corners of the frustum.
            private readonly Vector3[] _corners = new Vector3[8];
            #endregion

            #region Properties.
            /// <summary>
            /// Property to return the number of corners in the frustum.
            /// </summary>
            public int Length => _corners.Length;

            /// <summary>
            /// Property to return a readonly reference to the plane.
            /// </summary>
            public ref readonly Vector3 this[FrustumCorner plane] => ref _corners[(int)plane];

            /// <summary>
            /// Property to return a readonly reference to the plane by index.
            /// </summary>
            public ref readonly Vector3 this[int index] => ref _corners[index];
            #endregion

            #region Methods.
            /// <summary>
            /// Function to assign a corner.
            /// </summary>
            /// <param name="frustumCorner">The corner to assign.</param>
            /// <param name="cornerPosition">The corner data.</param>
            internal void Set(FrustumCorner frustumCorner, in Vector3 cornerPosition) => _corners[(int)frustumCorner] = cornerPosition;

            /// <summary>Returns an enumerator that iterates through the collection.</summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            public IEnumerator<Vector3> GetEnumerator()
            {
                foreach (Vector3 plane in _corners)
                {
                    yield return plane;
                }
            }

            /// <summary>Returns an enumerator that iterates through a collection.</summary>
            /// <returns>An <see cref="IEnumerator">IEnumerator</see> object that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator() => _corners.GetEnumerator();
            #endregion
        }

        /// <summary>
        /// The list of planes.
        /// </summary>
        public class PlaneList
            : IEnumerable<Plane>
        {
            #region Variables.
            // The list of planes for the frustum.
            private readonly Plane[] _planes = new Plane[6];            
            #endregion

            #region Properties.
            /// <summary>
            /// Property to return the number of planes in the frustum.
            /// </summary>
            public int Length => _planes.Length;

            /// <summary>
            /// Property to return a readonly reference to the plane.
            /// </summary>
            public ref readonly Plane this[FrustumPlane plane] => ref _planes[(int)plane];

            /// <summary>
            /// Property to return a readonly reference to the plane by index.
            /// </summary>
            public ref readonly Plane this[int index] => ref _planes[index];
            #endregion

            #region Methods.
            /// <summary>
            /// Function to assign a plane.
            /// </summary>
            /// <param name="frustumPlane">The orientation of the plane to assign.</param>
            /// <param name="plane">The plane data.</param>
            internal void Set(FrustumPlane frustumPlane, in Plane plane) => _planes[(int)frustumPlane] = plane;

            /// <summary>Returns an enumerator that iterates through the collection.</summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            public IEnumerator<Plane> GetEnumerator()
            {
                foreach (Plane plane in _planes)
                {
                    yield return plane;
                }
            }

            /// <summary>Returns an enumerator that iterates through a collection.</summary>
            /// <returns>An <see cref="IEnumerator">IEnumerator</see> object that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator() => _planes.GetEnumerator();
            #endregion
        }
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the corners for the frustum volume.
        /// </summary>
        public CornerList Corners
        {
            get;
        } = new CornerList();

        /// <summary>
        /// Property to return the planes for the frustum.
        /// </summary>
        /// <remarks>
        /// All planes are in world space.
        /// </remarks>
        public PlaneList Planes 
        { 
            get; 
        } = new PlaneList();
        #endregion

        #region Methods.
        /// <summary>
        /// Function to calculate the position of the corner for intersecting planes.
        /// </summary>
        /// <param name="a">The first plane in the calculation.</param>
        /// <param name="b">The second plane in the calculation.</param>
        /// <param name="c">The third plane in the calculation.</param>
        /// <remarks>
        /// <para>
        /// This is adapted from code posted on StackOverflow (<a href="https://stackoverflow.com/a/28995460/1045720"/>).
        /// </para>
        /// </remarks>
        private Vector3 CalculateCornerIntersection(in Plane a, in Plane b, in Plane c)
        {
            // Formula used
            //                d1 ( N2 * N3 ) + d2 ( N3 * N1 ) + d3 ( N1 * N2 )
            //P =   ---------------------------------------------------------------------
            //                             N1 . ( N2 * N3 )
            //
            // Note: N refers to the normal, d refers to the displacement. '.' means dot product. '*' means cross product

            Vector3 v1, v2, v3;
            float f = -Vector3.Dot(a.Normal, Vector3.Cross(b.Normal, c.Normal));

            v1 = (a.D * (Vector3.Cross(b.Normal, c.Normal)));
            v2 = (b.D * (Vector3.Cross(c.Normal, a.Normal)));
            v3 = (c.D * (Vector3.Cross(a.Normal, b.Normal)));

            return new Vector3((v1.X + v2.X + v3.X) / f, (v1.Y + v2.Y + v3.Y) / f, (v1.Z + v2.Z + v3.Z) / f);            
        }

        /// <summary>
        /// Function to update the frustum with a view matrix and a projection matrix.
        /// </summary>
        /// <param name="viewMatrix">The view matrix to use.</param>
        /// <param name="projectionMatrix">The projection matrix to use.</param>
        public void Update(in Matrix4x4 viewMatrix, in Matrix4x4 projectionMatrix)
        {
            Plane near;
            Plane far;
            Plane top;
            Plane bottom;
            Plane left;
            Plane right;
            
            // Create the matrix used to convert into clip space.
            viewMatrix.Multiply(in projectionMatrix, out Matrix4x4 clip);

            clip.GetColumn(0, out Vector4 column1);
            clip.GetColumn(1, out Vector4 column2);
            clip.GetColumn(2, out Vector4 column3);
            clip.GetColumn(3, out Vector4 column4);

            // Calculate near plane.
            near = Plane.Normalize(new Plane(column3));
            // Calculate near plane.
            far = Plane.Normalize(new Plane(column4 - column3));

            // Calculate left plane.
            left = Plane.Normalize(new Plane(column4 + column1));
            // Calculate left plane.
            right = Plane.Normalize(new Plane(column4 - column1));

            // Calculate top plane.
            top = Plane.Normalize(new Plane(column4 - column2));
            // Calculate bottom plane.
            bottom = Plane.Normalize(new Plane(column4 + column2));

            Planes.Set(FrustumPlane.Near, in near);
            Planes.Set(FrustumPlane.Far, in far);
            Planes.Set(FrustumPlane.Left, in left);
            Planes.Set(FrustumPlane.Right, in right);
            Planes.Set(FrustumPlane.Top, in top);
            Planes.Set(FrustumPlane.Bottom, in bottom);

            // Calculate the corners for the frustum.
            Corners.Set(FrustumCorner.TopLeftNear, CalculateCornerIntersection(in top, in left, in near));
            Corners.Set(FrustumCorner.TopRightNear, CalculateCornerIntersection(in top, in right, in near));
            Corners.Set(FrustumCorner.BottomLeftNear, CalculateCornerIntersection(in bottom, in left, in near));
            Corners.Set(FrustumCorner.BottomRightNear, CalculateCornerIntersection(in bottom, in right, in near));

            Corners.Set(FrustumCorner.TopLeftFar, CalculateCornerIntersection(in top, in left, in far));
            Corners.Set(FrustumCorner.TopRightFar, CalculateCornerIntersection(in top, in right, in far));
            Corners.Set(FrustumCorner.BottomLeftFar, CalculateCornerIntersection(in bottom, in left, in far));
            Corners.Set(FrustumCorner.BottomRightFar, CalculateCornerIntersection(in bottom, in right, in far));
        }

        /// <summary>
        /// Function to return whether the specified point, in world space, is inside the frustum.
        /// </summary>
        /// <param name="point">The point to evaluate.</param>
        /// <returns><b>true</b> if the point is inside the frustum, <b>false</b> if not.</returns>
        public bool Contains(Vector3 point)
        {
            for (int i = 0; i < Planes.Length; ++i)
            {
                ref readonly Plane plane = ref Planes[i];

                if ((Vector3.Dot(point, plane.Normal) + plane.D) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Function to determine if any of the points specified are contained within the frustum.
        /// </summary>
        /// <param name="points">The list of points to evaluate.</param>
        /// <returns><b>true</b> if any of the points are in the frustum, <b>false</b> if not.</returns>
        public bool ContainsAll(ReadOnlySpan<Vector3> points)
        {
            if (points.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < points.Length; ++i)
            {
                if (!Contains(points[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Function to determine if any of the points specified are contained within the frustum.
        /// </summary>
        /// <param name="points">The list of points to evaluate.</param>
        /// <returns><b>true</b> if any of the points are in the frustum, <b>false</b> if not.</returns>
        public bool ContainsAny(ReadOnlySpan<Vector3> points)
        {
            for (int i = 0; i < points.Length; ++i)
            {
                if (Contains(points[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Function to determine if an axis aligned bounding box interescts with the frustum.
        /// </summary>
        /// <param name="aabb">The axis aligned bounding box to evaluate.</param>
        /// <returns><b>true</b> if the AABB intersects with the frustum, <b>false</b> if not.</returns>
        public bool IntersectsAABB(in GorgonAABB aabb) => AABBIntersection(in aabb).Result != IntersectionResult.Outside;

        /// <summary>
        /// Function to test whether an axis aligned bounding box is fully, partially, or not inside of a frustum.
        /// </summary>
        /// <param name="aabb">The axis aligned bounding box to evaluate.</param>
        /// <returns>A <see cref="IntersectionResult"/> and a bitmask indicating which planes the bounding box is in front of, or intersects with.</returns>
        /// <remarks>
        /// <para>
        /// The Planes bitmask uses each bit to indicate a single plane index (bits 0 - 5). Each set bit means the box intersected with the plane index represented by bit, if all 6 bits are set 
        /// then the box is fully inside of the frustum. If no bits are set, then the box is completely outside of the frustum.
        /// </para>
        /// </remarks>
        public (IntersectionResult Result, byte Planes) AABBIntersection(in GorgonAABB aabb)
        {
            int planes = 0;            

            for (int i = 0; i < Planes.Length; ++i)
            {
                ref readonly Plane plane = ref Planes[i];
                int planeCount = 0;

                if ((Vector3.Dot(aabb.Min, plane.Normal) + plane.D) > 0)
                {
                    ++planeCount;
                }

                if ((Vector3.Dot(aabb.TopRightFront, plane.Normal) + plane.D) > 0)
                {
                    ++planeCount;
                }

                if ((Vector3.Dot(aabb.BottomLeftFront, plane.Normal) + plane.D) > 0)
                {
                    ++planeCount;
                }

                if ((Vector3.Dot(aabb.BottomRightFront, plane.Normal) + plane.D) > 0)
                {
                    ++planeCount;
                }

                if ((Vector3.Dot(aabb.TopLeftBack, plane.Normal) + plane.D) > 0)
                {
                    ++planeCount;
                }

                if ((Vector3.Dot(aabb.TopRightBack, plane.Normal) + plane.D) > 0)
                {
                    ++planeCount;
                }

                if ((Vector3.Dot(aabb.BottomLeftBack, plane.Normal) + plane.D) > 0)
                {
                    ++planeCount;
                }

                if ((Vector3.Dot(aabb.Max, plane.Normal) + plane.D) > 0)
                {
                    ++planeCount;
                }

                if (planeCount == 0)
                {
                    return (IntersectionResult.Outside, 0);
                }
                else if (planeCount == 8)
                {
                    planes |= 1 << i;
                }
            }

            return planes == 0
                ? ((IntersectionResult Result, byte Planes))(IntersectionResult.Outside, 0)
                : (planes >= 63) ? (IntersectionResult.Full, (byte)planes) : (IntersectionResult.Partial, (byte)planes);
        }

        /// <summary>
        /// Function to determine if the frustum intersects with a sphere.
        /// </summary>
        /// <param name="sphere">The bounding sphere to evaluate.</param>
        /// <returns><b>true</b> if the frustum intersects with the sphere, <b>false</b> if not.</returns>
        public bool IntersectsSphere(in GorgonBoundingSphere sphere) => SphereIntersection(sphere).Result != IntersectionResult.Outside;

        /// <summary>
        /// Function to test whether a sphere is fully, partially, or not inside of a frustum.
        /// </summary>
        /// <param name="sphere">The bounding sphere to evaluate.</param>        
        /// <returns>A <see cref="IntersectionResult"/> and a bitmask indicating which planes the sphere is in front of, or intersects with.</returns>
        /// <remarks>
        /// <para>
        /// The Planes bitmask uses each bit to indicate a single plane index (bits 0 - 5). Each set bit means the sphere intersected with the plane index represented by bit, if all 6 bits are set 
        /// then the sphere is fully inside of the frustum. If no bits are set, then the sphere is completely outside of the frustum.
        /// </para>
        /// </remarks>
        public (IntersectionResult Result, byte Planes) SphereIntersection(in GorgonBoundingSphere sphere)
        {
            int planes = 0;

            for (int i = 0; i < Planes.Length; ++i)
            {
                ref readonly Plane plane = ref Planes[i];

                float distance = Vector3.Dot(sphere.Center, plane.Normal) + plane.D;

                if (distance <= -sphere.Radius)
                {
                    return (IntersectionResult.Outside, 0);
                }
                else if (distance > sphere.Radius)
                {
                    planes |= 1 << i;                    
                }
            }

            return planes == 0
                ? ((IntersectionResult Result, byte Planes))(IntersectionResult.Outside, 0)
                : ((planes >= 63) ? IntersectionResult.Full : IntersectionResult.Partial, (byte)planes);
        }

        /// <summary>Deconstructs this instance.</summary>
        /// <param name="near">The near plane.</param>
        /// <param name="far">The far plane.</param>
        /// <param name="left">The left plane.</param>
        /// <param name="right">The right plane.</param>
        /// <param name="top">The top plane.</param>
        /// <param name="bottom">The bottom plane.</param>
        public void Deconstruct(out Plane near, out Plane far, out Plane left, out Plane right, out Plane top, out Plane bottom)
        {
            near = Planes[FrustumPlane.Near];
            far = Planes[FrustumPlane.Far];
            left = Planes[FrustumPlane.Left];
            right = Planes[FrustumPlane.Right];
            top = Planes[FrustumPlane.Top];
            bottom = Planes[FrustumPlane.Bottom];
        }
        #endregion
    }
}
