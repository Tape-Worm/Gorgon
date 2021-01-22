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
// Created: January 16, 2021 11:12:48 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace Gorgon.Math
{
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
        Partial = 2,
        /// <summary>
        /// In front of an object.
        /// </summary>
        Front = 3,
        /// <summary>
        /// Behind an object.
        /// </summary>
        Back = 4,
        /// <summary>
        /// Intersecting with an object.
        /// </summary>
        Intersecting = 5
    }

    /// <summary>
    /// Result value for a contains test.
    /// </summary>
    public enum ContainsResult
    {
        /// <summary>
        /// The two bounding volumes don't intersect at all.
        /// </summary>
        None,
        /// <summary>
        /// One bounding volume completely contains another.
        /// </summary>
        Contains,
        /// <summary>
        /// The two bounding volumes overlap.
        /// </summary>        
        Intersects
    }

    /// <summary>
    /// A class providing functions to perform intersection tests against various various data structures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This code is adapted from SharpDX (<a href="https://github.com/sharpdx/SharpDX">https://github.com/sharpdx/SharpDX</a>>
    /// </para>
    /// </remarks>
    public static class GorgonIntersection
    {
        /// <summary>
        /// Function to determine the closest point between a point and a triangle.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="triangle">The triangle to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
        public static void ClosestPointPointTriangle(in Vector3 point, in (Vector3 vertex1, Vector3 vertex2, Vector3 vertex3) triangle, out Vector3 result)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 136

            //Check if P in vertex region outside A
            Vector3 ab = triangle.vertex2 - triangle.vertex1;
            Vector3 ac = triangle.vertex3 - triangle.vertex1;
            Vector3 ap = point - triangle.vertex1;

            float d1 = Vector3.Dot(ab, ap);
            float d2 = Vector3.Dot(ac, ap);
            if ((d1 <= 0.0f) && (d2 <= 0.0f))
            {
                result = triangle.vertex1; //Barycentric coordinates (1,0,0)
                return;
            }

            //Check if P in vertex region outside B
            Vector3 bp = point - triangle.vertex2;
            float d3 = Vector3.Dot(ab, bp);
            float d4 = Vector3.Dot(ac, bp);
            if ((d3 >= 0.0f) && (d4 <= d3))
            {
                result = triangle.vertex2; // Barycentric coordinates (0,1,0)
                return;
            }

            //Check if P in edge region of AB, if so return projection of P onto AB
            float vc = d1 * d4 - d3 * d2;
            if ((vc <= 0.0f) && (d1 >= 0.0f) && (d3 <= 0.0f))
            {
                float v = d1 / (d1 - d3);
                result = triangle.vertex1 + v * ab; //Barycentric coordinates (1-v,v,0)
                return;
            }

            //Check if P in vertex region outside C
            Vector3 cp = point - triangle.vertex3;
            float d5 = Vector3.Dot(ab, cp);
            float d6 = Vector3.Dot(ac, cp);
            if ((d6 >= 0.0f) && (d5 <= d6))
            {
                result = triangle.vertex3; //Barycentric coordinates (0,0,1)
                return;
            }

            //Check if P in edge region of AC, if so return projection of P onto AC
            float vb = d5 * d2 - d1 * d6;
            if ((vb <= 0.0f) && (d2 >= 0.0f) && (d6 <= 0.0f))
            {
                float w = d2 / (d2 - d6);
                result = triangle.vertex1 + w * ac; //Barycentric coordinates (1-w,0,w)
                return;
            }

            //Check if P in edge region of BC, if so return projection of P onto BC
            float va = d3 * d6 - d5 * d4;
            if ((va <= 0.0f) && ((d4 - d3) >= 0.0f) && ((d5 - d6) >= 0.0f))
            {
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                result = triangle.vertex2 + w * (triangle.vertex3 - triangle.vertex2); //Barycentric coordinates (0,1-w,w)
                return;
            }

            //P inside face region. Compute Q through its Barycentric coordinates (u,v,w)
            float denom = 1.0f / (va + vb + vc);
            float v2 = vb * denom;
            float w2 = vc * denom;
            result = triangle.vertex1 + ab * v2 + ac * w2; //= u*vertex1 + v*vertex2 + w*vertex3, u = va * denom = 1.0f - v - w
        }

        /// <summary>
        /// Function to determine the closest point between a <see cref="Plane"/> and a point.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="point">The point to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
        public static void ClosestPointPlanePoint(in Plane plane, in Vector3 point, out Vector3 result)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 126

            float dot = Vector3.Dot(plane.Normal, point);
            float t = dot - plane.D;

            result = point - (t * plane.Normal);
        }

        /// <summary>
        /// Function to determine the closest point between a <see cref="GorgonAABB"/> and a point.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="point">The point to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
        public static void ClosestPointBoxPoint(in GorgonAABB box, in Vector3 point, out Vector3 result)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 130

            var temp = Vector3.Max(point, box.Min);
            result = Vector3.Min(temp, box.Max);
        }

        /// <summary>
        /// Function to determine the closest point between a <see cref="GorgonBoundingSphere"/> and a point.
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="point">The point to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects; or, if the point is directly in the center of the sphere, contains <see cref="Vector3.Zero"/>.</param>
        public static void ClosestPointSpherePoint(in GorgonBoundingSphere sphere, in Vector3 point, out Vector3 result)
        {
            //Source: Jorgy343
            //Reference: None

            //Get the unit direction from the sphere's center to the point.
            result = Vector3.Subtract(point, sphere.Center);
            result = Vector3.Normalize(result);

            //Multiply the unit direction by the sphere's radius to get a vector
            //the length of the sphere.
            result *= sphere.Radius;

            //Add the sphere's center to the direction to get a point on the sphere.
            result += sphere.Center;
        }

        /// <summary>
        /// Function to determine the closest point between a <see cref="GorgonBoundingSphere"/> and a <see cref="GorgonBoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">The first sphere to test.</param>
        /// <param name="sphere2">The second sphere to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects; or, if the point is directly in the center of the sphere, contains <see cref="Vector3.Zero"/>.</param>
        /// <remarks>
        /// <para>
        /// If the two spheres are overlapping, but not directly on top of each other, the closest point is the 'closest' point of intersection. This can also be considered is the deepest point of
        /// intersection.
        /// </para>
        /// </remarks>
        public static void ClosestPointSphereSphere(in GorgonBoundingSphere sphere1, in GorgonBoundingSphere sphere2, out Vector3 result)
        {
            //Source: Jorgy343
            //Reference: None

            //Get the unit direction from the first sphere's center to the second sphere's center.
            result = Vector3.Subtract(sphere2.Center, sphere1.Center);
            result = Vector3.Normalize(result);

            //Multiply the unit direction by the first sphere's radius to get a vector
            //the length of the first sphere.
            result *= sphere1.Radius;

            //Add the first sphere's center to the direction to get a point on the first sphere.
            result += sphere1.Center;
        }

        /// <summary>
        /// Function to determines the distance between a <see cref="Plane"/> and a point.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistancePlanePoint(in Plane plane, in Vector3 point)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 127

            float dot = Vector3.Dot(plane.Normal, point);
            return dot - plane.D;
        }

        /// <summary>
        /// Function to determine the distance between a <see cref="GorgonAABB"/> and a point.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistanceBoxPoint(in GorgonAABB box, in Vector3 point)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 131

            float distance = 0f;

            if (point.X < box.Min.X)
            {
                distance += (box.Min.X - point.X) * (box.Min.X - point.X);
            }

            if (point.X > box.Max.X)
            {
                distance += (point.X - box.Max.X) * (point.X - box.Max.X);
            }

            if (point.Y < box.Min.Y)
            {
                distance += (box.Min.Y - point.Y) * (box.Min.Y - point.Y);
            }

            if (point.Y > box.Max.Y)
            {
                distance += (point.Y - box.Max.Y) * (point.Y - box.Max.Y);
            }

            if (point.Z < box.Min.Z)
            {
                distance += (box.Min.Z - point.Z) * (box.Min.Z - point.Z);
            }

            if (point.Z > box.Max.Z)
            {
                distance += (point.Z - box.Max.Z) * (point.Z - box.Max.Z);
            }

            return distance.Sqrt();
        }

        /// <summary>
        /// Function to determine the distance between a <see cref="GorgonAABB"/> and a <see cref="GorgonAABB"/>.
        /// </summary>
        /// <param name="box1">The first box to test.</param>
        /// <param name="box2">The second box to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistanceBoxBox(in GorgonAABB box1, in GorgonAABB box2)
        {
            //Source:
            //Reference:

            float distance = 0f;

            //Distance for X.
            if (box1.Min.X > box2.Max.X)
            {
                float delta = box2.Max.X - box1.Min.X;
                distance += delta * delta;
            }
            else if (box2.Min.X > box1.Max.X)
            {
                float delta = box1.Max.X - box2.Min.X;
                distance += delta * delta;
            }

            //Distance for Y.
            if (box1.Min.Y > box2.Max.Y)
            {
                float delta = box2.Max.Y - box1.Min.Y;
                distance += delta * delta;
            }
            else if (box2.Min.Y > box1.Max.Y)
            {
                float delta = box1.Max.Y - box2.Min.Y;
                distance += delta * delta;
            }

            //Distance for Z.
            if (box1.Min.Z > box2.Max.Z)
            {
                float delta = box2.Max.Z - box1.Min.Z;
                distance += delta * delta;
            }
            else if (box2.Min.Z > box1.Max.Z)
            {
                float delta = box1.Max.Z - box2.Min.Z;
                distance += delta * delta;
            }

            return distance.Sqrt();
        }

        /// <summary>
        /// Function to determine the distance between a <see cref="GorgonBoundingSphere"/> and a point.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistanceSpherePoint(in GorgonBoundingSphere sphere, in Vector3 point)
        {
            //Source: Jorgy343
            //Reference: None

            float distance = Vector3.Distance(sphere.Center, point);
            distance -= sphere.Radius;

            return distance.Max(0);
        }

        /// <summary>
        /// Function to determine the distance between a <see cref="GorgonBoundingSphere"/> and a <see cref="GorgonBoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">The first sphere to test.</param>
        /// <param name="sphere2">The second sphere to test.</param>
        /// <returns>The distance between the two objects.</returns>
        public static float DistanceSphereSphere(in GorgonBoundingSphere sphere1, in GorgonBoundingSphere sphere2)
        {
            //Source: Jorgy343
            //Reference: None

            float distance = Vector3.Distance(sphere1.Center, sphere2.Center);
            distance -= sphere1.Radius + sphere2.Radius;

            return distance.Max(0);
        }

        /// <summary>
        /// Function to determine whether there is an intersection between a <see cref="GorgonRay"/> and a <see cref="GorgonRay"/>.
        /// </summary>
        /// <param name="ray1">The first ray to test.</param>
        /// <param name="ray2">The second ray to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns><b>true</b> if the rays intersect, or <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// This method performs a ray vs ray intersection test based on the following formula from Goldman.
        /// </para>
        /// <para>
        /// <code>s = det([o_2 - o_1, d_2, d_1 x d_2]) / ||d_1 x d_2||^2</code>
        /// </para>
        /// <para>
        /// <code>t = det([o_2 - o_1, d_1, d_1 x d_2]) / ||d_1 x d_2||^2</code>
        /// </para>
        /// <para>
        /// Where o_1 is the position of the first ray, o_2 is the position of the second ray, d_1 is the normalized direction of the first ray, d_2 is the normalized direction of the second ray, 
        /// det denotes the determinant of a matrix, x denotes the cross product, [ ] denotes a matrix, and || || denotes the length or magnitude of a vector.
        /// </para>
        /// </remarks>
        public static bool RayIntersectsRay(in GorgonRay ray1, in GorgonRay ray2, out Vector3 point)
        {
            //Source: Real-Time Rendering, Third Edition
            //Reference: Page 780
            
            var cross = Vector3.Cross(ray1.Direction, ray2.Direction);
            float denominator = cross.Length();

            //Lines are parallel.
            if (denominator.EqualsEpsilon(0))
            {
                //Lines are parallel and on top of each other.
                if ((ray2.Position.X.EqualsEpsilon(ray1.Position.X))
                    && (ray2.Position.Y.EqualsEpsilon(ray1.Position.Y))
                    && (ray2.Position.Z.EqualsEpsilon(ray1.Position.Z)))
                {
                    point = Vector3.Zero;
                    return true;
                }
            }

            denominator *= denominator;

            //3x3 matrix for the first ray.
            float m11 = ray2.Position.X - ray1.Position.X;
            float m12 = ray2.Position.Y - ray1.Position.Y;
            float m13 = ray2.Position.Z - ray1.Position.Z;
            float m21 = ray2.Direction.X;
            float m22 = ray2.Direction.Y;
            float m23 = ray2.Direction.Z;
            float m31 = cross.X;
            float m32 = cross.Y;
            float m33 = cross.Z;

            //Determinant of first matrix.
            float dets =
                m11 * m22 * m33 +
                m12 * m23 * m31 +
                m13 * m21 * m32 -
                m11 * m23 * m32 -
                m12 * m21 * m33 -
                m13 * m22 * m31;

            //3x3 matrix for the second ray.
            m21 = ray1.Direction.X;
            m22 = ray1.Direction.Y;
            m23 = ray1.Direction.Z;

            //Determinant of the second matrix.
            float dett =
                m11 * m22 * m33 +
                m12 * m23 * m31 +
                m13 * m21 * m32 -
                m11 * m23 * m32 -
                m12 * m21 * m33 -
                m13 * m22 * m31;

            //t values of the point of intersection.
            float s = dets / denominator;
            float t = dett / denominator;

            //The points of intersection.
            Vector3 point1 = ray1.Position + (s * ray1.Direction);
            Vector3 point2 = ray2.Position + (t * ray2.Direction);

            //If the points are not equal, no intersection has occurred.
            if ((!point2.X.EqualsEpsilon(point1.X))
                || (!point2.Y.EqualsEpsilon(point1.Y))
                || (!point2.Z.EqualsEpsilon(point1.Z)))
            {
                point = Vector3.Zero;
                return false;
            }

            point = point1;
            return true;
        }

        /// <summary>
        /// Function to determine whether there is an intersection between a <see cref="GorgonRay"/> and a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns><b>true</b> if the ray intersects the point, <b>false</b> if not.</returns>
        public static bool RayIntersectsPoint(in GorgonRay ray, in Vector3 point)
        {
            //Source: RayIntersectsSphere
            //Reference: None

            var m = Vector3.Subtract(ray.Position, point);

            //Same thing as RayIntersectsSphere except that the radius of the sphere (point)
            //is the epsilon for zero.
            float b = Vector3.Dot(m, ray.Direction);
            float c = Vector3.Dot(m, m) - SharpDX.MathUtil.ZeroTolerance;

            if ((c > 0f) && (b > 0f))
            {
                return false;
            }

            float discriminant = b * b - c;

            return discriminant >= 0f;
        }

        /// <summary>
        /// Function to determine whether there is an intersection between a <see cref="GorgonRay"/> and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="plane">The plane to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns><b>true</b> if the ray intersects the plane, <b>false</b> if not.</returns>
        public static bool RayIntersectsPlane(in GorgonRay ray, in Plane plane, out Vector3 point)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 175

            if (!RayIntersectsPlane(in ray, in plane, out float distance))
            {
                point = Vector3.Zero;
                return false;
            }

            point = ray.Position + (ray.Direction * distance);
            return true;
        }

        /// <summary>
        /// Function to determine whether there is an intersection between a <see cref="GorgonRay"/> and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="plane">The plane to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection, or 0 if there was no intersection.</param>
        /// <returns><b>true</b> if the ray intersects the plane, <b>false</b> if not.</returns>
        public static bool RayIntersectsPlane(in GorgonRay ray, in Plane plane, out float distance)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 175

            float direction = Vector3.Dot(plane.Normal, ray.Direction);

            if (direction.EqualsEpsilon(0))
            {
                distance = 0f;
                return false;
            }

            float position = Vector3.Dot(plane.Normal, ray.Position);
            distance = (-plane.D - position) / direction;

            if (distance < 0f)
            {
                distance = 0f;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Function to determine whether there is an intersection between a <see cref="GorgonRay"/> and a triangle.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="triangle">The triangle to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection, or 0 if there was no intersection.</param>
        /// <returns><b>true</b> if the ray intersected with the triangle, <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// This method tests if the ray intersects either the front or back of the triangle. If the ray is parallel to the triangle's plane, no intersection is assumed to have happened. If the 
        /// intersection of the ray and the triangle is behind the origin of the ray, no intersection is assumed to have happened. In both cases of assumptions, this method returns <b>false</b>.
        /// </para>
        /// </remarks>
        public static bool RayIntersectsTriangle(in GorgonRay ray, in (Vector3 vertex1, Vector3 vertex2, Vector3 vertex3) triangle, out float distance)
        {
            //Source: Fast Minimum Storage Ray / Triangle Intersection
            //Reference: http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf

            //Compute vectors along two edges of the triangle.
            Vector3 edge1, edge2;

            //Edge 1
            edge1.X = triangle.vertex2.X - triangle.vertex1.X;
            edge1.Y = triangle.vertex2.Y - triangle.vertex1.Y;
            edge1.Z = triangle.vertex2.Z - triangle.vertex1.Z;

            //Edge2
            edge2.X = triangle.vertex3.X - triangle.vertex1.X;
            edge2.Y = triangle.vertex3.Y - triangle.vertex1.Y;
            edge2.Z = triangle.vertex3.Z - triangle.vertex1.Z;

            //Cross product of ray direction and edge2 - first part of determinant.
            Vector3 directioncrossedge2;
            directioncrossedge2.X = (ray.Direction.Y * edge2.Z) - (ray.Direction.Z * edge2.Y);
            directioncrossedge2.Y = (ray.Direction.Z * edge2.X) - (ray.Direction.X * edge2.Z);
            directioncrossedge2.Z = (ray.Direction.X * edge2.Y) - (ray.Direction.Y * edge2.X);

            //Compute the determinant.
            float determinant;
            //Dot product of edge1 and the first part of determinant.
            determinant = (edge1.X * directioncrossedge2.X) + (edge1.Y * directioncrossedge2.Y) + (edge1.Z * directioncrossedge2.Z);

            //If the ray is parallel to the triangle plane, there is no collision.
            //This also means that we are not culling, the ray may hit both the
            //back and the front of the triangle.
            if (determinant.EqualsEpsilon(0))
            {
                distance = 0f;
                return false;
            }

            float inversedeterminant = 1.0f / determinant;

            //Calculate the U parameter of the intersection point.
            Vector3 distanceVector;
            distanceVector.X = ray.Position.X - triangle.vertex1.X;
            distanceVector.Y = ray.Position.Y - triangle.vertex1.Y;
            distanceVector.Z = ray.Position.Z - triangle.vertex1.Z;

            float triangleU;
            triangleU = (distanceVector.X * directioncrossedge2.X) + (distanceVector.Y * directioncrossedge2.Y) + (distanceVector.Z * directioncrossedge2.Z);
            triangleU *= inversedeterminant;

            //Make sure it is inside the triangle.
            if (triangleU < 0f || triangleU > 1f)
            {
                distance = 0f;
                return false;
            }

            //Calculate the V parameter of the intersection point.
            Vector3 distancecrossedge1;
            distancecrossedge1.X = (distanceVector.Y * edge1.Z) - (distanceVector.Z * edge1.Y);
            distancecrossedge1.Y = (distanceVector.Z * edge1.X) - (distanceVector.X * edge1.Z);
            distancecrossedge1.Z = (distanceVector.X * edge1.Y) - (distanceVector.Y * edge1.X);

            float triangleV;
            triangleV = ((ray.Direction.X * distancecrossedge1.X) + (ray.Direction.Y * distancecrossedge1.Y)) + (ray.Direction.Z * distancecrossedge1.Z);
            triangleV *= inversedeterminant;

            //Make sure it is inside the triangle.
            if ((triangleV < 0f) || ((triangleU + triangleV) > 1f))
            {
                distance = 0f;
                return false;
            }

            //Compute the distance along the ray to the triangle.
            float raydistance;
            raydistance = (edge2.X * distancecrossedge1.X) + (edge2.Y * distancecrossedge1.Y) + (edge2.Z * distancecrossedge1.Z);
            raydistance *= inversedeterminant;

            //Is the triangle behind the ray origin?
            if (raydistance < 0f)
            {
                distance = 0f;
                return false;
            }

            distance = raydistance;
            return true;
        }

        /// <summary>
        /// Function to determine whether there is an intersection between a <see cref="GorgonRay"/> and a triangle.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="triangle">The triangle to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns><b>true</b> if the ray intersected with the triangle, <b>false</b> if not.</returns>
        public static bool RayIntersectsTriangle(in GorgonRay ray, in (Vector3 vertex1, Vector3 vertex2, Vector3 vertex3) triangle, out Vector3 point)
        {
            if (!RayIntersectsTriangle(in ray, in triangle, out float distance))
            {
                point = Vector3.Zero;
                return false;
            }

            point = ray.Position + (ray.Direction * distance);
            return true;
        }

        /// <summary>
        /// Function to determine whether there is an intersection between a <see cref="GorgonRay"/> and a <see cref="GorgonAABB"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="box">The box to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection, or 0 if there was no intersection.</param>
        /// <returns><b>true</b> if the ray intersects the box, or <b>false</b> if not.</returns>
        public static bool RayIntersectsAABB(in GorgonRay ray, in GorgonAABB box, out float distance)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 179

            distance = 0f;
            float tmax = float.MaxValue;

            if (ray.Direction.X.EqualsEpsilon(0))
            {
                if ((ray.Position.X < box.Min.X) || (ray.Position.X > box.Max.X))
                {
                    distance = 0f;
                    return false;
                }
            }
            else
            {
                float inverse = 1.0f / ray.Direction.X;
                float t1 = (box.Min.X - ray.Position.X) * inverse;
                float t2 = (box.Max.X - ray.Position.X) * inverse;

                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = t1.Max(distance);
                tmax = t2.Min(tmax);

                if (distance > tmax)
                {
                    distance = 0f;
                    return false;
                }
            }

            if (ray.Direction.Y.EqualsEpsilon(0))
            {
                if ((ray.Position.Y < box.Min.Y) || (ray.Position.Y > box.Max.Y))
                {
                    distance = 0f;
                    return false;
                }
            }
            else
            {
                float inverse = 1.0f / ray.Direction.Y;
                float t1 = (box.Min.Y - ray.Position.Y) * inverse;
                float t2 = (box.Max.Y - ray.Position.Y) * inverse;

                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = t1.Max(distance);
                tmax = t2.Min(tmax);

                if (distance > tmax)
                {
                    distance = 0f;
                    return false;
                }
            }

            if (ray.Direction.Z.EqualsEpsilon(0))
            {
                if ((ray.Position.Z < box.Min.Z) || (ray.Position.Z > box.Max.Z))
                {
                    distance = 0f;
                    return false;
                }
            }
            else
            {
                float inverse = 1.0f / ray.Direction.Z;
                float t1 = (box.Min.Z - ray.Position.Z) * inverse;
                float t2 = (box.Max.Z - ray.Position.Z) * inverse;

                if (t1 > t2)
                {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = t1.Max(distance);
                tmax = t2.Min(tmax);

                if (distance > tmax)
                {
                    distance = 0f;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Function to determine whether there is an intersection between a <see cref="GorgonRay"/> and a <see cref="GorgonAABB"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="box">The box to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns><b>true</b> if the ray intersects the box, or <b>false</b> if not.</returns>
        public static bool RayIntersectsAABB(in GorgonRay ray, in GorgonAABB box, out Vector3 point)
        {            
            if (!RayIntersectsAABB(in ray, in box, out float distance))
            {
                point = Vector3.Zero;
                return false;
            }

            point = ray.Position + (ray.Direction * distance);
            return true;
        }

        /// <summary>
        /// Function to determines whether there is an intersection between a <see cref="GorgonRay"/> and a <see cref="GorgonBoundingSphere"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="distance">When the method completes, contains the distance of the intersection, or 0 if there was no intersection.</param>
        /// <returns><b>true</b> if the ray intersects the sphere, or <b>false</b> if not.</returns>
        public static bool RayIntersectsSphere(in GorgonRay ray, in GorgonBoundingSphere sphere, out float distance)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 177

            var m = Vector3.Subtract(ray.Position, sphere.Center);

            float b = Vector3.Dot(m, ray.Direction);
            float c = Vector3.Dot(m, m) - (sphere.Radius * sphere.Radius);

            if ((c > 0f) && (b > 0f))
            {
                distance = 0f;
                return false;
            }

            float discriminant = b * b - c;

            if (discriminant < 0f)
            {
                distance = 0f;
                return false;
            }

            distance = -b - discriminant.Sqrt();

            if (distance < 0f)
            {
                distance = 0f;
            }

            return true;
        }

        /// <summary>
        /// Function to determines whether there is an intersection between a <see cref="GorgonRay"/> and a <see cref="GorgonBoundingSphere"/>.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">When the method completes, contains the point of intersection, or <see cref="Vector3.Zero"/> if there was no intersection.</param>
        /// <returns><b>true</b> if the ray intersects the sphere, or <b>false</b> if not.</returns>
        public static bool RayIntersectsSphere(in GorgonRay ray, in GorgonBoundingSphere sphere, out Vector3 point)
        {
            if (!RayIntersectsSphere(in ray, in sphere, out float distance))
            {
                point = Vector3.Zero;
                return false;
            }

            point = ray.Position + (ray.Direction * distance);
            return true;
        }

        /// <summary>
        /// Function to determine whether there is an intersection between a <see cref="Plane"/> and a point.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns><b>true</b> if the plane intersects the point, or <b>false</b> if not.</returns>
        public static IntersectionResult PlaneIntersectsPoint(in Plane plane, in Vector3 point)
        {
            float distance = Vector3.Dot(plane.Normal, point);
            distance += plane.D;

#pragma warning disable IDE0046 // Convert to conditional expression
            if (distance > 0f)
            {
                return IntersectionResult.Front;
            }

            return distance < 0f ? IntersectionResult.Back : IntersectionResult.Intersecting;
#pragma warning restore IDE0046 // Convert to conditional expression
        }

        /// <summary>
        /// Function to determine whether there is an intersection between a <see cref="Plane"/> and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane1">The first plane to test.</param>
        /// <param name="plane2">The second plane to test.</param>
        /// <returns><b>true</b> if the plane intersects the plane, or <b>false</b> if not.</returns>
        public static bool PlaneIntersectsPlane(in Plane plane1, in Plane plane2)
        {
            var direction = Vector3.Cross(plane1.Normal, plane2.Normal);

            //If direction is the zero vector, the planes are parallel and possibly
            //coincident. It is not an intersection. The dot product will tell us.
            float denominator = Vector3.Dot(direction, direction);

            return !denominator.EqualsEpsilon(0);
        }

        /// <summary>
        /// Function to determines whether there is an intersection between a <see cref="Plane"/> and a <see cref="Plane"/>.
        /// </summary>
        /// <param name="plane1">The first plane to test.</param>
        /// <param name="plane2">The second plane to test.</param>
        /// <param name="line">When the method completes, contains the line of intersection as a <see cref="GorgonRay"/>, or a zero ray if there was no intersection.</param>
        /// <returns>Whether the two objects intersected.</returns>
        /// <remarks>
        /// <para>
        /// Although a ray is set to have an origin, the ray returned by this method is really a line in three dimensions which has no real origin. The ray is considered valid when both the positive 
        /// direction is used and when the negative direction is used.
        /// </para>
        /// </remarks>
        public static bool PlaneIntersectsPlane(in Plane plane1, in Plane plane2, out GorgonRay line)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 207

            var direction = Vector3.Cross(plane1.Normal, plane2.Normal);

            //If direction is the zero vector, the planes are parallel and possibly
            //coincident. It is not an intersection. The dot product will tell us.
            float denominator = Vector3.Dot(direction, direction);

            //We assume the planes are normalized, therefore the denominator
            //only serves as a parallel and coincident check. Otherwise we need
            //to divide the point by the denominator.
            if (denominator.EqualsEpsilon(0))
            {
                line = default;
                return false;
            }

            Vector3 temp = plane1.D * plane2.Normal - plane2.D * plane1.Normal;
            var point = Vector3.Cross(temp, direction);
                        
            line = new GorgonRay(point, Vector3.Normalize(direction));
            return true;
        }

        /// <summary>
        /// Function to determine whether there is an intersection between a <see cref="Plane"/> and a triangle.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="triangle">The triangle to test.</param>
        /// <returns>An <see cref="IntersectionResult"/> indicating the intersection result.</returns>
        public static IntersectionResult PlaneIntersectsTriangle(in Plane plane, in (Vector3 vertex1, Vector3 vertex2, Vector3 vertex3) triangle)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 207

            IntersectionResult test1 = PlaneIntersectsPoint(in plane, in triangle.vertex1);
            IntersectionResult test2 = PlaneIntersectsPoint(in plane, in triangle.vertex2);
            IntersectionResult test3 = PlaneIntersectsPoint(in plane, in triangle.vertex3);

#pragma warning disable IDE0046 // Convert to conditional expression
            if ((test1 == IntersectionResult.Front) && (test2 == IntersectionResult.Front) && (test3 == IntersectionResult.Front))
            {
                return IntersectionResult.Front;
            }

            return (test1 == IntersectionResult.Back) && (test2 == IntersectionResult.Back) && (test3 == IntersectionResult.Back)
                ? IntersectionResult.Back
                : IntersectionResult.Intersecting;
#pragma warning restore IDE0046 // Convert to conditional expression
        }

        /// <summary>
        /// Function to determine whether there is an intersection between a <see cref="Plane"/> and a <see cref="GorgonAABB"/>.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="box">The AABB to test.</param>
        /// <returns>An <see cref="IntersectionResult"/> indicating the intersection result.</returns>
        public static IntersectionResult PlaneIntersectsBox(in Plane plane, in GorgonAABB box)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 161

            Vector3 min;
            Vector3 max;

            max.X = (plane.Normal.X >= 0.0f) ? box.Min.X : box.Max.X;
            max.Y = (plane.Normal.Y >= 0.0f) ? box.Min.Y : box.Max.Y;
            max.Z = (plane.Normal.Z >= 0.0f) ? box.Min.Z : box.Max.Z;
            min.X = (plane.Normal.X >= 0.0f) ? box.Max.X : box.Min.X;
            min.Y = (plane.Normal.Y >= 0.0f) ? box.Max.Y : box.Min.Y;
            min.Z = (plane.Normal.Z >= 0.0f) ? box.Max.Z : box.Min.Z;

            float distance = Vector3.Dot(plane.Normal, max);

            if (distance + plane.D > 0.0f)
            {
                return IntersectionResult.Front;
            }

            distance = Vector3.Dot(plane.Normal, min);

            return distance + plane.D < 0.0f ? IntersectionResult.Back : IntersectionResult.Intersecting;
        }

        /// <summary>
        /// Function to determine whether there is an intersection between a <see cref="Plane"/> and a <see cref="GorgonAABB"/>.
        /// </summary>
        /// <param name="plane">The plane to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>An <see cref="IntersectionResult"/> indicating the intersection result.</returns>
        public static IntersectionResult PlaneIntersectsSphere(in Plane plane, in GorgonBoundingSphere sphere)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 160

            float distance = Vector3.Dot(plane.Normal, sphere.Center);
            distance += plane.D;

#pragma warning disable IDE0046 // Convert to conditional expression
            if (distance > sphere.Radius)
            {
                return IntersectionResult.Front;
            }

            return distance < -sphere.Radius ? IntersectionResult.Back : IntersectionResult.Intersecting;
#pragma warning restore IDE0046 // Convert to conditional expression
        }

        /// <summary>
        /// Function to determine whether there is an intersection between a <see cref="GorgonAABB"/> and a <see cref="GorgonAABB"/>.
        /// </summary>
        /// <param name="box1">The first box to test.</param>
        /// <param name="box2">The second box to test.</param>
        /// <returns><b>true</b> if the boxes intersect, or <b>false</b> if not.</returns>
        public static bool BoxIntersectsBox(in GorgonAABB box1, in GorgonAABB box2)
        {
            if ((box1.Min.X > box2.Max.X) || (box2.Min.X > box1.Max.X))
            {
                return false;
            }

#pragma warning disable IDE0046 // Convert to conditional expression
            if ((box1.Min.Y > box2.Max.Y) || (box2.Min.Y > box1.Max.Y))
            {
                return false;
            }

            return box1.Min.Z <= box2.Max.Z && box2.Min.Z <= box1.Max.Z;
#pragma warning restore IDE0046 // Convert to conditional expression
        }

        /// <summary>
        /// Function to determine whether there is an intersection between a <see cref="GorgonAABB"/> and a <see cref="GorgonBoundingSphere"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns><b>true</b> if the box and sphere intersect, or <b>false</b> if not.</returns>
        public static bool BoxIntersectsSphere(in GorgonAABB box, in GorgonBoundingSphere sphere)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 166

            var vector = Vector3.Clamp(sphere.Center, box.Min, box.Max);
            float distance = Vector3.DistanceSquared(sphere.Center, vector);

            return distance <= sphere.Radius * sphere.Radius;
        }

        /// <summary>
        /// Function to determine whether there is an intersection between a <see cref="GorgonBoundingSphere"/> and a triangle.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="triangle">The triangle to test.</param>
        /// <returns><b>true</b> if the sphere intersects the triangle, or <b>false</b> if not.</returns>
        public static bool SphereIntersectsTriangle(in GorgonBoundingSphere sphere, in (Vector3 vertex1, Vector3 vertex2, Vector3 vertex3) triangle)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 167

            ClosestPointPointTriangle(in sphere.Center, in triangle, out Vector3 point);
            Vector3 v = point - sphere.Center;

            float dot = Vector3.Dot(v, v);

            return dot <= sphere.Radius * sphere.Radius;
        }

        /// <summary>
        /// Function to determine whether there is an intersection between a <see cref="GorgonBoundingSphere"/> and a <see cref="GorgonBoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">First sphere to test.</param>
        /// <param name="sphere2">Second sphere to test.</param>
        /// <returns><b>true</b> if the spheres intersect, or <b>false</b> if not.</returns>
        public static bool SphereIntersectsSphere(in GorgonBoundingSphere sphere1, in GorgonBoundingSphere sphere2)
        {
            float radiisum = sphere1.Radius + sphere2.Radius;
            return Vector3.DistanceSquared(sphere1.Center, sphere2.Center) <= radiisum * radiisum;
        }

        /// <summary>
        /// Function to determine whether a <see cref="GorgonAABB"/> contains a point.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>A <see cref="ContainsResult"/> indicating whether the point is contained or not.</returns>
        public static ContainsResult BoxContainsPoint(in GorgonAABB box, in Vector3 point) 
            => (box.Min.X <= point.X)
                && (box.Max.X >= point.X)
                && (box.Min.Y <= point.Y)
                && (box.Max.Y >= point.Y)
                && (box.Min.Z <= point.Z)
                && (box.Max.Z >= point.Z)
                ? ContainsResult.Contains : ContainsResult.None;

        /// <summary>
        /// Function to determine whether a <see cref="GorgonAABB"/> contains a <see cref="GorgonAABB"/>.
        /// </summary>
        /// <param name="box1">The first box to test.</param>
        /// <param name="box2">The second box to test.</param>
        /// <returns>A <see cref="ContainsResult"/> indicating whether the point is contained or not.</returns>
        public static ContainsResult BoxContainsBox(in GorgonAABB box1, in GorgonAABB box2)
        {
            if ((box1.Max.X < box2.Min.X) || (box1.Min.X > box2.Max.X))
            {
                return ContainsResult.None;
            }

            if ((box1.Max.Y < box2.Min.Y) || (box1.Min.Y > box2.Max.Y))
            {
                return ContainsResult.None;
            }

#pragma warning disable IDE0046 // Convert to conditional expression
            if ((box1.Max.Z < box2.Min.Z) || (box1.Min.Z > box2.Max.Z))
            {
                return ContainsResult.None;
            }

            return (box1.Min.X <= box2.Min.X)
                && ((box2.Max.X <= box1.Max.X) && (box1.Min.Y <= box2.Min.Y) && (box2.Max.Y <= box1.Max.Y))
                && (box1.Min.Z <= box2.Min.Z && box2.Max.Z <= box1.Max.Z)
                ? ContainsResult.Contains
                : ContainsResult.Intersects;
#pragma warning restore IDE0046 // Convert to conditional expression
        }

        /// <summary>
        /// Function to determine whether a <see cref="GorgonAABB"/> contains a <see cref="GorgonBoundingSphere"/>.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <param name="sphere">The sphere to test.</param>
        /// <returns>A <see cref="ContainsResult"/> indicating whether the point is contained or not.</returns>
        public static ContainsResult BoxContainsSphere(in GorgonAABB box, in GorgonBoundingSphere sphere)
        {
            var vector = Vector3.Clamp(sphere.Center, box.Min, box.Max);
            float distance = Vector3.DistanceSquared(sphere.Center, vector);

            if (distance > (sphere.Radius * sphere.Radius))
            {
                return ContainsResult.None;
            }

#pragma warning disable IDE0046 // Convert to conditional expression
            if ((((box.Min.X + sphere.Radius <= sphere.Center.X) && (sphere.Center.X <= box.Max.X - sphere.Radius)) && ((box.Max.X - box.Min.X > sphere.Radius) &&
                (box.Min.Y + sphere.Radius <= sphere.Center.Y))) && (((sphere.Center.Y <= box.Max.Y - sphere.Radius) && (box.Max.Y - box.Min.Y > sphere.Radius)) &&
                (((box.Min.Z + sphere.Radius <= sphere.Center.Z) && (sphere.Center.Z <= box.Max.Z - sphere.Radius)) && (box.Max.Z - box.Min.Z > sphere.Radius))))
            {
                return ContainsResult.Contains;
            }

            return ContainsResult.Intersects;
#pragma warning restore IDE0046 // Convert to conditional expression
        }

        /// <summary>
        /// Function to determine whether a <see cref="GorgonBoundingSphere"/> contains a point.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="point">The point to test.</param>
        /// <returns>A <see cref="ContainsResult"/> indicating whether the point is contained or not.</returns>
        public static ContainsResult SphereContainsPoint(in GorgonBoundingSphere sphere, in Vector3 point) => Vector3.DistanceSquared(point, sphere.Center) <= sphere.Radius * sphere.Radius ? ContainsResult.Contains : ContainsResult.None;

        /// <summary>
        /// Function to determine whether a <see cref="GorgonBoundingSphere"/> contains a triangle.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="triangle">The triangle to test.</param>
        /// <returns>A <see cref="ContainsResult"/> indicating whether the point is contained or not.</returns>
        public static ContainsResult SphereContainsTriangle(in GorgonBoundingSphere sphere, in (Vector3 vertex1, Vector3 vertex2, Vector3 vertex3) triangle)
        {
            //Source: Jorgy343
            //Reference: None

            ContainsResult test1 = SphereContainsPoint(in sphere, in triangle.vertex1);
            ContainsResult test2 = SphereContainsPoint(in sphere, in triangle.vertex2);
            ContainsResult test3 = SphereContainsPoint(in sphere, in triangle.vertex3);

#pragma warning disable IDE0046 // Convert to conditional expression
            if ((test1 == ContainsResult.Contains) && (test2 == ContainsResult.Contains) && (test3 == ContainsResult.Contains))
            {
                return ContainsResult.Contains;
            }

            return SphereIntersectsTriangle(in sphere, in triangle) ? ContainsResult.Intersects : ContainsResult.None;
#pragma warning restore IDE0046 // Convert to conditional expression
        }

        /// <summary>
        /// Function to determine whether a <see cref="GorgonBoundingSphere"/> contains a <see cref="GorgonAABB"/>.
        /// </summary>
        /// <param name="sphere">The sphere to test.</param>
        /// <param name="box">The box to test.</param>
        /// <returns>A <see cref="ContainsResult"/> indicating whether the point is contained or not.</returns>
        public static ContainsResult SphereContainsBox(in GorgonBoundingSphere sphere, in GorgonAABB box)
        {
            Vector3 vector;

            if (!BoxIntersectsSphere(in box, in sphere))
            {
                return ContainsResult.None;
            }

            float radiussquared = sphere.Radius * sphere.Radius;
            vector.X = sphere.Center.X - box.Min.X;
            vector.Y = sphere.Center.Y - box.Max.Y;
            vector.Z = sphere.Center.Z - box.Max.Z;

            if (vector.LengthSquared() > radiussquared)
            {
                return ContainsResult.Intersects;
            }

            vector.X = sphere.Center.X - box.Max.X;
            vector.Y = sphere.Center.Y - box.Max.Y;
            vector.Z = sphere.Center.Z - box.Max.Z;

            if (vector.LengthSquared() > radiussquared)
            {
                return ContainsResult.Intersects;
            }

            vector.X = sphere.Center.X - box.Max.X;
            vector.Y = sphere.Center.Y - box.Min.Y;
            vector.Z = sphere.Center.Z - box.Max.Z;

            if (vector.LengthSquared() > radiussquared)
            {
                return ContainsResult.Intersects;
            }

            vector.X = sphere.Center.X - box.Min.X;
            vector.Y = sphere.Center.Y - box.Min.Y;
            vector.Z = sphere.Center.Z - box.Max.Z;

            if (vector.LengthSquared() > radiussquared)
            {
                return ContainsResult.Intersects;
            }

            vector.X = sphere.Center.X - box.Min.X;
            vector.Y = sphere.Center.Y - box.Max.Y;
            vector.Z = sphere.Center.Z - box.Min.Z;

            if (vector.LengthSquared() > radiussquared)
            {
                return ContainsResult.Intersects;
            }

            vector.X = sphere.Center.X - box.Max.X;
            vector.Y = sphere.Center.Y - box.Max.Y;
            vector.Z = sphere.Center.Z - box.Min.Z;

            if (vector.LengthSquared() > radiussquared)
            {
                return ContainsResult.Intersects;
            }

            vector.X = sphere.Center.X - box.Max.X;
            vector.Y = sphere.Center.Y - box.Min.Y;
            vector.Z = sphere.Center.Z - box.Min.Z;

            if (vector.LengthSquared() > radiussquared)
            {
                return ContainsResult.Intersects;
            }

            vector.X = sphere.Center.X - box.Min.X;
            vector.Y = sphere.Center.Y - box.Min.Y;
            vector.Z = sphere.Center.Z - box.Min.Z;

            return vector.LengthSquared() > radiussquared ? ContainsResult.Intersects : ContainsResult.Contains;
        }

        /// <summary>
        /// Function to determine whether a <see cref="GorgonBoundingSphere"/> contains a <see cref="GorgonBoundingSphere"/>.
        /// </summary>
        /// <param name="sphere1">The first sphere to test.</param>
        /// <param name="sphere2">The second sphere to test.</param>
        /// <returns>A <see cref="ContainsResult"/> indicating whether the point is contained or not.</returns>
        public static ContainsResult SphereContainsSphere(in GorgonBoundingSphere sphere1, in GorgonBoundingSphere sphere2)
        {
            float distance = Vector3.Distance(sphere1.Center, sphere2.Center);

#pragma warning disable IDE0046 // Convert to conditional expression
            if (sphere1.Radius + sphere2.Radius < distance)
            {
                return ContainsResult.None;
            }

            return sphere1.Radius - sphere2.Radius < distance ? ContainsResult.Intersects : ContainsResult.Contains;
#pragma warning restore IDE0046 // Convert to conditional expression
        }
    }
}
