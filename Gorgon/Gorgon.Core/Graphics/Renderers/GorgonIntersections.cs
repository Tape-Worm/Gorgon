
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: February 12, 2021 8:21:53 PM
// 


// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// -----------------------------------------------------------------------------
// Original code from SlimMath project. http://code.google.com/p/slimmath/
// Greetings to SlimDX Group. Original code published with the following license:
// -----------------------------------------------------------------------------
/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/


using System.Numerics;
using System.Runtime.CompilerServices;
using Gorgon.Math;
using Gorgon.Renderers.Data;

namespace Gorgon.Renderers;

/// <summary>
/// Defines the type of intersection on a plane
/// </summary>
public enum PlaneIntersection
{
    /// <summary>
    /// The object is behind the plane.
    /// </summary>
    Back,
    /// <summary>
    /// The object is in front of the plane.
    /// </summary>
    Front,
    /// <summary>
    /// The object is intersecting with the plane.
    /// </summary>
    Intersecting
}

/// <summary>
/// Describes how one bounding volume contains another
/// </summary>
public enum Containment
{
    /// <summary>
    /// The two bounding volumes don't intersect at all.
    /// </summary>
    Disjoint,
    /// <summary>
    /// One bounding volume completely contains another.
    /// </summary>
    Contains,
    /// <summary>
    /// The two bounding volumes overlap.
    /// </summary>
    Intersects
}

/*
 * This class is organized so that the least complex objects come first so that the least
 * complex objects will have the most methods in most cases. Note that not all shapes exist
 * at this time and not all shapes have a corresponding struct. Only the objects that have
 * a corresponding struct should come first in naming and in parameter order. The order of
 * complexity is as follows:
 * 
 * 1. Point
 * 2. Ray
 * 3. Segment
 * 4. Plane
 * 5. Triangle
 * 6. Polygon
 * 7. Box
 * 8. Sphere
 * 9. Ellipsoid
 * 10. Cylinder
 * 11. Cone
 * 12. Capsule
 * 13. Torus
 * 14. Polyhedron
 * 15. Frustum
*/

/// <summary>
/// Contains static methods to help in determining intersections, containment, etc
/// </summary>
public static class GorgonIntersections
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="box"></param>
    /// <param name="planeNormal"></param>
    /// <param name="p"></param>
    /// <param name="n"></param>
    private static void GetBoxToPlanePVertexNVertex(ref readonly GorgonBoundingBox box, ref readonly Vector3 planeNormal, out Vector3 p, out Vector3 n)
    {
        p = box.Minimum;
        if (planeNormal.X >= 0)
        {
            p.X = box.Maximum.X;
        }

        if (planeNormal.Y >= 0)
        {
            p.Y = box.Maximum.Y;
        }

        if (planeNormal.Z >= 0)
        {
            p.Z = box.Maximum.Z;
        }

        n = box.Maximum;
        if (planeNormal.X >= 0)
        {
            n.X = box.Minimum.X;
        }

        if (planeNormal.Y >= 0)
        {
            n.Y = box.Minimum.Y;
        }

        if (planeNormal.Z >= 0)
        {
            n.Z = box.Minimum.Z;
        }
    }

    /// <summary>
    /// Determines the closest point between a point and a triangle.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <param name="vertex1">The first vertex to test.</param>
    /// <param name="vertex2">The second vertex to test.</param>
    /// <param name="vertex3">The third vertex to test.</param>
    /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
    public static void ClosestPointPointTriangle(ref readonly Vector3 point, ref readonly Vector3 vertex1, ref readonly Vector3 vertex2, ref readonly Vector3 vertex3, out Vector3 result)
    {
        //Source: Real-Time Collision Detection by Christer Ericson
        //Reference: Page 136

        //Check if P in vertex region outside A
        Vector3 ab = vertex2 - vertex1;
        Vector3 ac = vertex3 - vertex1;
        Vector3 ap = point - vertex1;

        float d1 = Vector3.Dot(ab, ap);
        float d2 = Vector3.Dot(ac, ap);
        if (d1 <= 0.0f && d2 <= 0.0f)
        {
            result = vertex1; //Barycentric coordinates (1,0,0)
            return;
        }

        //Check if P in vertex region outside B
        Vector3 bp = point - vertex2;
        float d3 = Vector3.Dot(ab, bp);
        float d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0.0f && d4 <= d3)
        {
            result = vertex2; // Barycentric coordinates (0,1,0)
            return;
        }

        //Check if P in edge region of AB, if so return projection of P onto AB
        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
        {
            float v = d1 / (d1 - d3);
            result = vertex1 + v * ab; //Barycentric coordinates (1-v,v,0)
            return;
        }

        //Check if P in vertex region outside C
        Vector3 cp = point - vertex3;
        float d5 = Vector3.Dot(ab, cp);
        float d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0.0f && d5 <= d6)
        {
            result = vertex3; //Barycentric coordinates (0,0,1)
            return;
        }

        //Check if P in edge region of AC, if so return projection of P onto AC
        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
        {
            float w = d2 / (d2 - d6);
            result = vertex1 + w * ac; //Barycentric coordinates (1-w,0,w)
            return;
        }

        //Check if P in edge region of BC, if so return projection of P onto BC
        float va = d3 * d6 - d5 * d4;
        if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
        {
            float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            result = vertex2 + w * (vertex3 - vertex2); //Barycentric coordinates (0,1-w,w)
            return;
        }

        //P inside face region. Compute Q through its Barycentric coordinates (u,v,w)
        float denom = 1.0f / (va + vb + vc);
        float v2 = vb * denom;
        float w2 = vc * denom;
        result = vertex1 + ab * v2 + ac * w2; //= u*vertex1 + v*vertex2 + w*vertex3, u = va * denom = 1.0f - v - w
    }

    /// <summary>
    /// Determines the closest point between a <see cref="Plane"/> and a point.
    /// </summary>
    /// <param name="plane">The plane to test.</param>
    /// <param name="point">The point to test.</param>
    /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClosestPointPlanePoint(ref readonly Plane plane, ref readonly Vector3 point, out Vector3 result)
    {
        //Source: Real-Time Collision Detection by Christer Ericson
        //Reference: Page 126

        float dot = Vector3.Dot(plane.Normal, point);
        float t = dot - plane.D;

        result = point - (t * plane.Normal);
    }

    /// <summary>
    /// Determines the closest point between a <see cref="GorgonBoundingBox"/> and a point.
    /// </summary>
    /// <param name="box">The box to test.</param>
    /// <param name="point">The point to test.</param>
    /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClosestPointBoxPoint(ref readonly GorgonBoundingBox box, ref readonly Vector3 point, out Vector3 result)
    {
        //Source: Real-Time Collision Detection by Christer Ericson
        //Reference: Page 130

        Vector3 temp = Vector3.Max(point, box.Minimum);
        result = Vector3.Min(temp, box.Maximum);
    }

    /// <summary>
    /// Determines the closest point between a <see cref="GorgonBoundingSphere"/> and a point.
    /// </summary>
    /// <param name="sphere"></param>
    /// <param name="point">The point to test.</param>
    /// <param name="result">When the method completes, contains the closest point between the two objects;
    /// or, if the point is directly in the center of the sphere, contains <see cref="Vector3.Zero"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClosestPointSpherePoint(ref readonly GorgonBoundingSphere sphere, ref readonly Vector3 point, out Vector3 result)
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
    /// Determines the closest point between a <see cref="GorgonBoundingSphere"/> and a <see cref="GorgonBoundingSphere"/>.
    /// </summary>
    /// <param name="sphere1">The first sphere to test.</param>
    /// <param name="sphere2">The second sphere to test.</param>
    /// <param name="result">When the method completes, contains the closest point between the two objects;
    /// or, if the point is directly in the center of the sphere, contains <see cref="Vector3.Zero"/>.</param>
    /// <remarks>
    /// If the two spheres are overlapping, but not directly on top of each other, the closest point
    /// is the 'closest' point of intersection. This can also be considered is the deepest point of
    /// intersection.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClosestPointSphereSphere(ref readonly GorgonBoundingSphere sphere1, ref readonly GorgonBoundingSphere sphere2, out Vector3 result)
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
    /// Determines the distance between a <see cref="Plane"/> and a point.
    /// </summary>
    /// <param name="plane">The plane to test.</param>
    /// <param name="point">The point to test.</param>
    /// <returns>The distance between the two objects.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistancePlanePoint(ref readonly Plane plane, ref readonly Vector3 point)
    {
        //Source: Real-Time Collision Detection by Christer Ericson
        //Reference: Page 127

        float dot = Vector3.Dot(plane.Normal, point);
        return dot - plane.D;
    }

    /// <summary>
    /// Determines the distance between a <see cref="GorgonBoundingBox"/> and a point.
    /// </summary>
    /// <param name="box">The box to test.</param>
    /// <param name="point">The point to test.</param>
    /// <returns>The distance between the two objects.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceBoxPoint(ref readonly GorgonBoundingBox box, ref readonly Vector3 point)
    {
        //Source: Real-Time Collision Detection by Christer Ericson
        //Reference: Page 131

        float distance = 0f;

        if (point.X < box.Minimum.X)
        {
            distance += (box.Minimum.X - point.X) * (box.Minimum.X - point.X);
        }

        if (point.X > box.Maximum.X)
        {
            distance += (point.X - box.Maximum.X) * (point.X - box.Maximum.X);
        }

        if (point.Y < box.Minimum.Y)
        {
            distance += (box.Minimum.Y - point.Y) * (box.Minimum.Y - point.Y);
        }

        if (point.Y > box.Maximum.Y)
        {
            distance += (point.Y - box.Maximum.Y) * (point.Y - box.Maximum.Y);
        }

        if (point.Z < box.Minimum.Z)
        {
            distance += (box.Minimum.Z - point.Z) * (box.Minimum.Z - point.Z);
        }

        if (point.Z > box.Maximum.Z)
        {
            distance += (point.Z - box.Maximum.Z) * (point.Z - box.Maximum.Z);
        }

        return distance.Sqrt();
    }

    /// <summary>
    /// Determines the distance between a <see cref="GorgonBoundingBox"/> and a <see cref="GorgonBoundingBox"/>.
    /// </summary>
    /// <param name="box1">The first box to test.</param>
    /// <param name="box2">The second box to test.</param>
    /// <returns>The distance between the two objects.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceBoxBox(ref readonly GorgonBoundingBox box1, ref readonly GorgonBoundingBox box2)
    {
        //Source:
        //Reference:

        float distance = 0f;

        //Distance for X.
        if (box1.Minimum.X > box2.Maximum.X)
        {
            float delta = box2.Maximum.X - box1.Minimum.X;
            distance += delta * delta;
        }
        else if (box2.Minimum.X > box1.Maximum.X)
        {
            float delta = box1.Maximum.X - box2.Minimum.X;
            distance += delta * delta;
        }

        //Distance for Y.
        if (box1.Minimum.Y > box2.Maximum.Y)
        {
            float delta = box2.Maximum.Y - box1.Minimum.Y;
            distance += delta * delta;
        }
        else if (box2.Minimum.Y > box1.Maximum.Y)
        {
            float delta = box1.Maximum.Y - box2.Minimum.Y;
            distance += delta * delta;
        }

        //Distance for Z.
        if (box1.Minimum.Z > box2.Maximum.Z)
        {
            float delta = box2.Maximum.Z - box1.Minimum.Z;
            distance += delta * delta;
        }
        else if (box2.Minimum.Z > box1.Maximum.Z)
        {
            float delta = box1.Maximum.Z - box2.Minimum.Z;
            distance += delta * delta;
        }

        return distance.Sqrt();
    }

    /// <summary>
    /// Determines the distance between a <see cref="GorgonBoundingSphere"/> and a point.
    /// </summary>
    /// <param name="sphere">The sphere to test.</param>
    /// <param name="point">The point to test.</param>
    /// <returns>The distance between the two objects.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSpherePoint(ref readonly GorgonBoundingSphere sphere, ref readonly Vector3 point)
    {
        //Source: Jorgy343
        //Reference: None

        float distance = Vector3.Distance(sphere.Center, point);
        distance -= sphere.Radius;

        return distance.Max(0);
    }

    /// <summary>
    /// Determines the distance between a <see cref="GorgonBoundingSphere"/> and a <see cref="GorgonBoundingSphere"/>.
    /// </summary>
    /// <param name="sphere1">The first sphere to test.</param>
    /// <param name="sphere2">The second sphere to test.</param>
    /// <returns>The distance between the two objects.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSphereSphere(ref readonly GorgonBoundingSphere sphere1, ref readonly GorgonBoundingSphere sphere2)
    {
        //Source: Jorgy343
        //Reference: None

        float distance = Vector3.Distance(sphere1.Center, sphere2.Center);
        distance -= sphere1.Radius + sphere2.Radius;

        return distance.Max(0);
    }

    /// <summary>
    /// Determines whether there is an intersection between a <see cref="GorgonRay"/> and a point.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="point">The point to test.</param>
    /// <returns>Whether the two objects intersect.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool RayIntersectsPoint(ref readonly GorgonRay ray, ref readonly Vector3 point)
    {
        //Source: RayIntersectsSphere
        //Reference: None

        Vector3 m = Vector3.Subtract(ray.Position, point);

        //Same thing as RayIntersectsSphere except that the radius of the sphere (point)
        //is the epsilon for zero.
        float b = Vector3.Dot(m, ray.Direction);
        float c = Vector3.Dot(m, m) - SharpDX.MathUtil.ZeroTolerance;

        if (c > 0f && b > 0f)
        {
            return false;
        }

        float discriminant = b * b - c;

        return discriminant >= 0f;
    }

    /// <summary>
    /// Determines whether there is an intersection between a <see cref="GorgonRay"/> and a <see cref="GorgonRay"/>.
    /// </summary>
    /// <param name="ray1">The first ray to test.</param>
    /// <param name="ray2">The second ray to test.</param>
    /// <param name="point">When the method completes, contains the point of intersection,
    /// or <see cref="Vector3.Zero"/> if there was no intersection.</param>
    /// <returns>Whether the two objects intersect.</returns>
    /// <remarks>
    /// This method performs a ray vs ray intersection test based on the following formula
    /// from Goldman.
    /// <code>s = det([o_2 - o_1, d_2, d_1 x d_2]) / ||d_1 x d_2||^2</code>
    /// <code>t = det([o_2 - o_1, d_1, d_1 x d_2]) / ||d_1 x d_2||^2</code>
    /// Where o_1 is the position of the first ray, o_2 is the position of the second ray,
    /// d_1 is the normalized direction of the first ray, d_2 is the normalized direction
    /// of the second ray, det denotes the determinant of a matrix, x denotes the cross
    /// product, [ ] denotes a matrix, and || || denotes the length or magnitude of a vector.
    /// </remarks>
    public static bool RayIntersectsRay(ref readonly GorgonRay ray1, ref readonly GorgonRay ray2, out Vector3 point)
    {
        //Source: Real-Time Rendering, Third Edition
        //Reference: Page 780

        Vector3 cross = Vector3.Cross(ray1.Direction, ray2.Direction);
        float denominator = cross.Length();

        //Lines are parallel.
        if (SharpDX.MathUtil.IsZero(denominator))
        {
            //Lines are parallel and on top of each other.
            if (SharpDX.MathUtil.NearEqual(ray2.Position.X, ray1.Position.X) &&
                SharpDX.MathUtil.NearEqual(ray2.Position.Y, ray1.Position.Y) &&
                SharpDX.MathUtil.NearEqual(ray2.Position.Z, ray1.Position.Z))
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
        if (!SharpDX.MathUtil.NearEqual(point2.X, point1.X) ||
            !SharpDX.MathUtil.NearEqual(point2.Y, point1.Y) ||
            !SharpDX.MathUtil.NearEqual(point2.Z, point1.Z))
        {
            point = Vector3.Zero;
            return false;
        }

        point = point1;
        return true;
    }

    /// <summary>
    /// Determines whether there is an intersection between a <see cref="GorgonRay"/> and a <see cref="Plane"/>.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="plane">The plane to test.</param>
    /// <param name="distance">When the method completes, contains the distance of the intersection,
    /// or 0 if there was no intersection.</param>
    /// <returns>Whether the two objects intersect.</returns>
    public static bool RayIntersectsPlane(ref readonly GorgonRay ray, ref readonly Plane plane, out float distance)
    {
        //Source: Real-Time Collision Detection by Christer Ericson
        //Reference: Page 175

        float direction = Vector3.Dot(plane.Normal, ray.Direction);

        if (SharpDX.MathUtil.IsZero(direction))
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
    /// Determines whether there is an intersection between a <see cref="GorgonRay"/> and a <see cref="Plane"/>.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="plane">The plane to test</param>
    /// <param name="point">When the method completes, contains the point of intersection,
    /// or <see cref="Vector3.Zero"/> if there was no intersection.</param>
    /// <returns>Whether the two objects intersected.</returns>
    public static bool RayIntersectsPlane(ref readonly GorgonRay ray, ref readonly Plane plane, out Vector3 point)
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
    /// Determines whether there is an intersection between a <see cref="GorgonRay"/> and a triangle.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="vertex1">The first vertex of the triangle to test.</param>
    /// <param name="vertex2">The second vertex of the triangle to test.</param>
    /// <param name="vertex3">The third vertex of the triangle to test.</param>
    /// <param name="distance">When the method completes, contains the distance of the intersection,
    /// or 0 if there was no intersection.</param>
    /// <returns>Whether the two objects intersected.</returns>
    /// <remarks>
    /// This method tests if the ray intersects either the front or back of the triangle.
    /// If the ray is parallel to the triangle's plane, no intersection is assumed to have
    /// happened. If the intersection of the ray and the triangle is behind the origin of
    /// the ray, no intersection is assumed to have happened. In both cases of assumptions,
    /// this method returns false.
    /// </remarks>
    public static bool RayIntersectsTriangle(ref readonly GorgonRay ray, ref readonly Vector3 vertex1, ref readonly Vector3 vertex2, ref readonly Vector3 vertex3, out float distance)
    {
        //Source: Fast Minimum Storage Ray / Triangle Intersection
        //Reference: http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf

        //Compute vectors along two edges of the triangle.
        Vector3 edge1, edge2;

        //Edge 1
        edge1.X = vertex2.X - vertex1.X;
        edge1.Y = vertex2.Y - vertex1.Y;
        edge1.Z = vertex2.Z - vertex1.Z;

        //Edge2
        edge2.X = vertex3.X - vertex1.X;
        edge2.Y = vertex3.Y - vertex1.Y;
        edge2.Z = vertex3.Z - vertex1.Z;

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
        if (SharpDX.MathUtil.IsZero(determinant))
        {
            distance = 0f;
            return false;
        }

        float inversedeterminant = 1.0f / determinant;

        //Calculate the U parameter of the intersection point.
        Vector3 distanceVector;
        distanceVector.X = ray.Position.X - vertex1.X;
        distanceVector.Y = ray.Position.Y - vertex1.Y;
        distanceVector.Z = ray.Position.Z - vertex1.Z;

        float triangleU;
        triangleU = (distanceVector.X * directioncrossedge2.X) + (distanceVector.Y * directioncrossedge2.Y) + (distanceVector.Z * directioncrossedge2.Z);
        triangleU *= inversedeterminant;

        //Make sure it is inside the triangle.
        if (triangleU is < 0f or > 1f)
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
        if (triangleV < 0f || triangleU + triangleV > 1f)
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
    /// Determines whether there is an intersection between a <see cref="GorgonRay"/> and a triangle.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="vertex1">The first vertex of the triangle to test.</param>
    /// <param name="vertex2">The second vertex of the triangle to test.</param>
    /// <param name="vertex3">The third vertex of the triangle to test.</param>
    /// <param name="point">When the method completes, contains the point of intersection,
    /// or <see cref="Vector3.Zero"/> if there was no intersection.</param>
    /// <returns>Whether the two objects intersected.</returns>
    public static bool RayIntersectsTriangle(ref readonly GorgonRay ray, ref readonly Vector3 vertex1, ref readonly Vector3 vertex2, ref readonly Vector3 vertex3, out Vector3 point)
    {
        if (!RayIntersectsTriangle(in ray, in vertex1, in vertex2, in vertex3, out float distance))
        {
            point = Vector3.Zero;
            return false;
        }

        point = ray.Position + (ray.Direction * distance);
        return true;
    }

    /// <summary>
    /// Determines whether there is an intersection between a <see cref="GorgonRay"/> and a <see cref="GorgonBoundingBox"/>.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="box">The box to test.</param>
    /// <param name="distance">When the method completes, contains the distance of the intersection,
    /// or 0 if there was no intersection.</param>
    /// <returns>Whether the two objects intersected.</returns>
    public static bool RayIntersectsBox(ref readonly GorgonRay ray, ref readonly GorgonBoundingBox box, out float distance)
    {
        //Source: Real-Time Collision Detection by Christer Ericson
        //Reference: Page 179

        distance = 0f;
        float tmax = float.MaxValue;

        if (SharpDX.MathUtil.IsZero(ray.Direction.X))
        {
            if (ray.Position.X < box.Minimum.X || ray.Position.X > box.Maximum.X)
            {
                distance = 0f;
                return false;
            }
        }
        else
        {
            float inverse = 1.0f / ray.Direction.X;
            float t1 = (box.Minimum.X - ray.Position.X) * inverse;
            float t2 = (box.Maximum.X - ray.Position.X) * inverse;

            if (t1 > t2)
            {
                (t2, t1) = (t1, t2);
            }

            distance = t1.Max(distance);
            tmax = t2.Min(tmax);

            if (distance > tmax)
            {
                distance = 0f;
                return false;
            }
        }

        if (SharpDX.MathUtil.IsZero(ray.Direction.Y))
        {
            if (ray.Position.Y < box.Minimum.Y || ray.Position.Y > box.Maximum.Y)
            {
                distance = 0f;
                return false;
            }
        }
        else
        {
            float inverse = 1.0f / ray.Direction.Y;
            float t1 = (box.Minimum.Y - ray.Position.Y) * inverse;
            float t2 = (box.Maximum.Y - ray.Position.Y) * inverse;

            if (t1 > t2)
            {
                (t2, t1) = (t1, t2);
            }

            distance = t1.Max(distance);
            tmax = t2.Min(tmax);

            if (distance > tmax)
            {
                distance = 0f;
                return false;
            }
        }

        if (SharpDX.MathUtil.IsZero(ray.Direction.Z))
        {
            if (ray.Position.Z < box.Minimum.Z || ray.Position.Z > box.Maximum.Z)
            {
                distance = 0f;
                return false;
            }
        }
        else
        {
            float inverse = 1.0f / ray.Direction.Z;
            float t1 = (box.Minimum.Z - ray.Position.Z) * inverse;
            float t2 = (box.Maximum.Z - ray.Position.Z) * inverse;

            if (t1 > t2)
            {
                (t2, t1) = (t1, t2);
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
    /// Determines whether there is an intersection between a <see cref="GorgonRay"/> and a <see cref="Plane"/>.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="box">The box to test.</param>
    /// <param name="point">When the method completes, contains the point of intersection,
    /// or <see cref="Vector3.Zero"/> if there was no intersection.</param>
    /// <returns>Whether the two objects intersected.</returns>
    public static bool RayIntersectsBox(ref readonly GorgonRay ray, ref readonly GorgonBoundingBox box, out Vector3 point)
    {
        if (!RayIntersectsBox(in ray, in box, out float distance))
        {
            point = Vector3.Zero;
            return false;
        }

        point = ray.Position + (ray.Direction * distance);
        return true;
    }

    /// <summary>
    /// Determines whether there is an intersection between a <see cref="GorgonRay"/> and a <see cref="GorgonBoundingSphere"/>.
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="sphere">The sphere to test.</param>
    /// <param name="distance">When the method completes, contains the distance of the intersection,
    /// or 0 if there was no intersection.</param>
    /// <returns>Whether the two objects intersected.</returns>
    public static bool RayIntersectsSphere(ref readonly GorgonRay ray, ref readonly GorgonBoundingSphere sphere, out float distance)
    {
        //Source: Real-Time Collision Detection by Christer Ericson
        //Reference: Page 177

        Vector3 m = Vector3.Subtract(ray.Position, sphere.Center);

        float b = Vector3.Dot(m, ray.Direction);
        float c = Vector3.Dot(m, m) - (sphere.Radius * sphere.Radius);

        if (c > 0f && b > 0f)
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
    /// Determines whether there is an intersection between a <see cref="GorgonRay"/> and a <see cref="GorgonBoundingSphere"/>. 
    /// </summary>
    /// <param name="ray">The ray to test.</param>
    /// <param name="sphere">The sphere to test.</param>
    /// <param name="point">When the method completes, contains the point of intersection,
    /// or <see cref="Vector3.Zero"/> if there was no intersection.</param>
    /// <returns>Whether the two objects intersected.</returns>
    public static bool RayIntersectsSphere(ref readonly GorgonRay ray, ref GorgonBoundingSphere sphere, out Vector3 point)
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
    /// Determines whether there is an intersection between a <see cref="Plane"/> and a point.
    /// </summary>
    /// <param name="plane">The plane to test.</param>
    /// <param name="point">The point to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PlaneIntersection PlaneIntersectsPoint(ref readonly Plane plane, ref readonly Vector3 point)
    {
        float distance = Vector3.Dot(plane.Normal, point);
        distance += plane.D;

        if (distance > 0f)
        {
            return PlaneIntersection.Front;
        }

        return distance < 0f ? PlaneIntersection.Back : PlaneIntersection.Intersecting;
    }

    /// <summary>
    /// Determines whether there is an intersection between a <see cref="Plane"/> and a <see cref="Plane"/>.
    /// </summary>
    /// <param name="plane1">The first plane to test.</param>
    /// <param name="plane2">The second plane to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PlaneIntersectsPlane(ref readonly Plane plane1, ref readonly Plane plane2)
    {
        Vector3 direction = Vector3.Cross(plane1.Normal, plane2.Normal);

        //If direction is the zero vector, the planes are parallel and possibly
        //coincident. It is not an intersection. The dot product will tell us.
        float denominator = Vector3.Dot(direction, direction);

        return !SharpDX.MathUtil.IsZero(denominator);
    }

    /// <summary>
    /// Determines whether there is an intersection between a <see cref="Plane"/> and a <see cref="Plane"/>.
    /// </summary>
    /// <param name="plane1">The first plane to test.</param>
    /// <param name="plane2">The second plane to test.</param>
    /// <param name="line">When the method completes, contains the line of intersection
    /// as a <see cref="GorgonRay"/>, or a zero ray if there was no intersection.</param>
    /// <returns>Whether the two objects intersected.</returns>
    /// <remarks>
    /// Although a ray is set to have an origin, the ray returned by this method is really
    /// a line in three dimensions which has no real origin. The ray is considered valid when
    /// both the positive direction is used and when the negative direction is used.
    /// </remarks>        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PlaneIntersectsPlane(ref readonly Plane plane1, ref readonly Plane plane2, out GorgonRay line)
    {
        //Source: Real-Time Collision Detection by Christer Ericson
        //Reference: Page 207

        Vector3 direction = Vector3.Cross(plane1.Normal, plane2.Normal);

        //If direction is the zero vector, the planes are parallel and possibly
        //coincident. It is not an intersection. The dot product will tell us.
        float denominator = Vector3.Dot(direction, direction);

        //We assume the planes are normalized, therefore the denominator
        //only serves as a parallel and coincident check. Otherwise we need
        //to divide the point by the denominator.
        if (SharpDX.MathUtil.IsZero(denominator))
        {
            line = default;
            return false;
        }

        Vector3 temp = plane1.D * plane2.Normal - plane2.D * plane1.Normal;
        Vector3 point = Vector3.Cross(temp, direction);

        line.Position = point;
        line.Direction = Vector3.Normalize(direction);

        return true;
    }

    /// <summary>
    /// Determines whether there is an intersection between a <see cref="Plane"/> and a triangle.
    /// </summary>
    /// <param name="plane">The plane to test.</param>
    /// <param name="vertex1">The first vertex of the triangle to test.</param>
    /// <param name="vertex2">The second vertex of the triangle to test.</param>
    /// <param name="vertex3">The third vertex of the triangle to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PlaneIntersection PlaneIntersectsTriangle(ref readonly Plane plane, ref readonly Vector3 vertex1, ref readonly Vector3 vertex2, ref readonly Vector3 vertex3)
    {
        //Source: Real-Time Collision Detection by Christer Ericson
        //Reference: Page 207

        PlaneIntersection test1 = PlaneIntersectsPoint(in plane, in vertex1);
        PlaneIntersection test2 = PlaneIntersectsPoint(in plane, in vertex2);
        PlaneIntersection test3 = PlaneIntersectsPoint(in plane, in vertex3);

        if ((test1 == PlaneIntersection.Front) && (test2 == PlaneIntersection.Front) && (test3 == PlaneIntersection.Front))
        {
            return PlaneIntersection.Front;
        }

        return (test1 == PlaneIntersection.Back) && (test2 == PlaneIntersection.Back) && (test3 == PlaneIntersection.Back)
            ? PlaneIntersection.Back
            : PlaneIntersection.Intersecting;
    }

    /// <summary>
    /// Determines whether there is an intersection between a <see cref="Plane"/> and a <see cref="GorgonBoundingBox"/>.
    /// </summary>
    /// <param name="plane">The plane to test.</param>
    /// <param name="box">The box to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PlaneIntersection PlaneIntersectsBox(ref readonly Plane plane, ref readonly GorgonBoundingBox box)
    {
        //Source: Real-Time Collision Detection by Christer Ericson
        //Reference: Page 161

        Vector3 min;
        Vector3 max;

        max.X = (plane.Normal.X >= 0.0f) ? box.Minimum.X : box.Maximum.X;
        max.Y = (plane.Normal.Y >= 0.0f) ? box.Minimum.Y : box.Maximum.Y;
        max.Z = (plane.Normal.Z >= 0.0f) ? box.Minimum.Z : box.Maximum.Z;
        min.X = (plane.Normal.X >= 0.0f) ? box.Maximum.X : box.Minimum.X;
        min.Y = (plane.Normal.Y >= 0.0f) ? box.Maximum.Y : box.Minimum.Y;
        min.Z = (plane.Normal.Z >= 0.0f) ? box.Maximum.Z : box.Minimum.Z;

        float distance = Vector3.Dot(plane.Normal, max);

        if (distance + plane.D > 0.0f)
        {
            return PlaneIntersection.Front;
        }

        distance = Vector3.Dot(plane.Normal, min);

        return distance + plane.D < 0.0f ? PlaneIntersection.Back : PlaneIntersection.Intersecting;
    }

    /// <summary>
    /// Determines whether there is an intersection between a <see cref="Plane"/> and a <see cref="GorgonBoundingSphere"/>.
    /// </summary>
    /// <param name="plane">The plane to test.</param>
    /// <param name="sphere">The sphere to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PlaneIntersection PlaneIntersectsSphere(ref readonly Plane plane, ref readonly GorgonBoundingSphere sphere)
    {
        //Source: Real-Time Collision Detection by Christer Ericson
        //Reference: Page 160

        float distance = Vector3.Dot(plane.Normal, sphere.Center);
        distance += plane.D;


        if (distance > sphere.Radius)
        {
            return PlaneIntersection.Front;
        }

        return distance < -sphere.Radius ? PlaneIntersection.Back : PlaneIntersection.Intersecting;

    }

    /// <summary>
    /// Determines whether there is an intersection between a <see cref="GorgonBoundingBox"/> and a <see cref="GorgonBoundingBox"/>.
    /// </summary>
    /// <param name="box1">The first box to test.</param>
    /// <param name="box2">The second box to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool BoxIntersectsBox(ref GorgonBoundingBox box1, ref GorgonBoundingBox box2)
    {
        if (box1.Minimum.X > box2.Maximum.X || box2.Minimum.X > box1.Maximum.X)
        {
            return false;
        }


        if (box1.Minimum.Y > box2.Maximum.Y || box2.Minimum.Y > box1.Maximum.Y)
        {
            return false;
        }

        return box1.Minimum.Z <= box2.Maximum.Z && box2.Minimum.Z <= box1.Maximum.Z;

    }

    /// <summary>
    /// Determines whether there is an intersection between a <see cref="GorgonBoundingBox"/> and a <see cref="GorgonBoundingSphere"/>.
    /// </summary>
    /// <param name="box">The box to test.</param>
    /// <param name="sphere">The sphere to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool BoxIntersectsSphere(ref readonly GorgonBoundingBox box, ref readonly GorgonBoundingSphere sphere)
    {
        //Source: Real-Time Collision Detection by Christer Ericson
        //Reference: Page 166

        Vector3 vector = Vector3.Clamp(sphere.Center, box.Minimum, box.Maximum);
        float distance = Vector3.DistanceSquared(sphere.Center, vector);

        return distance <= sphere.Radius * sphere.Radius;
    }

    /// <summary>
    /// Determines whether there is an intersection between a <see cref="GorgonBoundingSphere"/> and a triangle.
    /// </summary>
    /// <param name="sphere">The sphere to test.</param>
    /// <param name="vertex1">The first vertex of the triangle to test.</param>
    /// <param name="vertex2">The second vertex of the triangle to test.</param>
    /// <param name="vertex3">The third vertex of the triangle to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SphereIntersectsTriangle(ref readonly GorgonBoundingSphere sphere, ref readonly Vector3 vertex1, ref readonly Vector3 vertex2, ref readonly Vector3 vertex3)
    {
        //Source: Real-Time Collision Detection by Christer Ericson
        //Reference: Page 167

        ClosestPointPointTriangle(in sphere.Center, in vertex1, in vertex2, in vertex3, out Vector3 point);
        Vector3 v = point - sphere.Center;

        float dot = Vector3.Dot(v, v);

        return dot <= sphere.Radius * sphere.Radius;
    }

    /// <summary>
    /// Determines whether there is an intersection between a <see cref="GorgonBoundingSphere"/> and a <see cref="GorgonBoundingSphere"/>.
    /// </summary>
    /// <param name="sphere1">First sphere to test.</param>
    /// <param name="sphere2">Second sphere to test.</param>
    /// <returns>Whether the two objects intersected.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SphereIntersectsSphere(ref readonly GorgonBoundingSphere sphere1, ref readonly GorgonBoundingSphere sphere2)
    {
        float radiisum = sphere1.Radius + sphere2.Radius;
        return Vector3.DistanceSquared(sphere1.Center, sphere2.Center) <= radiisum * radiisum;
    }

    /// <summary>
    /// Determines whether a <see cref="GorgonBoundingBox"/> contains a point.
    /// </summary>
    /// <param name="box">The box to test.</param>
    /// <param name="point">The point to test.</param>
    /// <returns>The type of containment the two objects have.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Containment BoxContainsPoint(ref readonly GorgonBoundingBox box, ref readonly Vector3 point)
        => ((box.Minimum.X <= point.X) && (box.Maximum.X >= point.X) &&
            (box.Minimum.Y <= point.Y) && (box.Maximum.Y >= point.Y) &&
            (box.Minimum.Z <= point.Z) && (box.Maximum.Z >= point.Z))
            ? Containment.Contains
            : Containment.Disjoint;

    /// <summary>
    /// Determines whether a <see cref="GorgonBoundingBox"/> contains a <see cref="GorgonBoundingBox"/>.
    /// </summary>
    /// <param name="box1">The first box to test.</param>
    /// <param name="box2">The second box to test.</param>
    /// <returns>The type of containment the two objects have.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Containment BoxContainsBox(ref readonly GorgonBoundingBox box1, ref readonly GorgonBoundingBox box2)
    {
        if (box1.Maximum.X < box2.Minimum.X || box1.Minimum.X > box2.Maximum.X)
        {
            return Containment.Disjoint;
        }

        if (box1.Maximum.Y < box2.Minimum.Y || box1.Minimum.Y > box2.Maximum.Y)
        {
            return Containment.Disjoint;
        }


        if (box1.Maximum.Z < box2.Minimum.Z || box1.Minimum.Z > box2.Maximum.Z)
        {
            return Containment.Disjoint;
        }

        return box1.Minimum.X <= box2.Minimum.X && (box2.Maximum.X <= box1.Maximum.X &&
            box1.Minimum.Y <= box2.Minimum.Y && box2.Maximum.Y <= box1.Maximum.Y) &&
            box1.Minimum.Z <= box2.Minimum.Z && box2.Maximum.Z <= box1.Maximum.Z
            ? Containment.Contains
            : Containment.Intersects;

    }

    /// <summary>
    /// Determines whether a <see cref="GorgonBoundingBox"/> contains a <see cref="GorgonBoundingSphere"/>.
    /// </summary>
    /// <param name="box">The box to test.</param>
    /// <param name="sphere">The sphere to test.</param>
    /// <returns>The type of containment the two objects have.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Containment BoxContainsSphere(ref readonly GorgonBoundingBox box, ref readonly GorgonBoundingSphere sphere)
    {
        Vector3 vector = Vector3.Clamp(sphere.Center, box.Minimum, box.Maximum);
        float distance = Vector3.DistanceSquared(sphere.Center, vector);

        if (distance > sphere.Radius * sphere.Radius)
        {
            return Containment.Disjoint;
        }


        if ((((box.Minimum.X + sphere.Radius <= sphere.Center.X) && (sphere.Center.X <= box.Maximum.X - sphere.Radius)) && ((box.Maximum.X - box.Minimum.X > sphere.Radius) &&
            (box.Minimum.Y + sphere.Radius <= sphere.Center.Y))) && (((sphere.Center.Y <= box.Maximum.Y - sphere.Radius) && (box.Maximum.Y - box.Minimum.Y > sphere.Radius)) &&
            (((box.Minimum.Z + sphere.Radius <= sphere.Center.Z) && (sphere.Center.Z <= box.Maximum.Z - sphere.Radius)) && (box.Maximum.Z - box.Minimum.Z > sphere.Radius))))
        {
            return Containment.Contains;
        }

        return Containment.Intersects;

    }

    /// <summary>
    /// Determines whether a <see cref="GorgonBoundingSphere"/> contains a point.
    /// </summary>
    /// <param name="sphere">The sphere to test.</param>
    /// <param name="point">The point to test.</param>
    /// <returns>The type of containment the two objects have.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Containment SphereContainsPoint(ref readonly GorgonBoundingSphere sphere, ref readonly Vector3 point) => Vector3.DistanceSquared(point, sphere.Center) <= sphere.Radius * sphere.Radius ? Containment.Contains : Containment.Disjoint;

    /// <summary>
    /// Determines whether a <see cref="GorgonBoundingSphere"/> contains a triangle.
    /// </summary>
    /// <param name="sphere">The sphere to test.</param>
    /// <param name="vertex1">The first vertex of the triangle to test.</param>
    /// <param name="vertex2">The second vertex of the triangle to test.</param>
    /// <param name="vertex3">The third vertex of the triangle to test.</param>
    /// <returns>The type of containment the two objects have.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Containment SphereContainsTriangle(ref readonly GorgonBoundingSphere sphere, ref readonly Vector3 vertex1, ref readonly Vector3 vertex2, ref readonly Vector3 vertex3)
    {
        //Source: Jorgy343
        //Reference: None

        Containment test1 = SphereContainsPoint(in sphere, in vertex1);
        Containment test2 = SphereContainsPoint(in sphere, in vertex2);
        Containment test3 = SphereContainsPoint(in sphere, in vertex3);


        if (test1 == Containment.Contains && test2 == Containment.Contains && test3 == Containment.Contains)
        {
            return Containment.Contains;
        }

        return SphereIntersectsTriangle(in sphere, in vertex1, in vertex2, in vertex3) ? Containment.Intersects : Containment.Disjoint;

    }

    /// <summary>
    /// Determines whether a <see cref="GorgonBoundingSphere"/> contains a <see cref="GorgonBoundingBox"/>.
    /// </summary>
    /// <param name="sphere">The sphere to test.</param>
    /// <param name="box">The box to test.</param>
    /// <returns>The type of containment the two objects have.</returns>
    public static Containment SphereContainsBox(ref readonly GorgonBoundingSphere sphere, ref readonly GorgonBoundingBox box)
    {
        Vector3 vector;

        if (!BoxIntersectsSphere(in box, in sphere))
        {
            return Containment.Disjoint;
        }

        float radiussquared = sphere.Radius * sphere.Radius;
        vector.X = sphere.Center.X - box.Minimum.X;
        vector.Y = sphere.Center.Y - box.Maximum.Y;
        vector.Z = sphere.Center.Z - box.Maximum.Z;

        if (vector.LengthSquared() > radiussquared)
        {
            return Containment.Intersects;
        }

        vector.X = sphere.Center.X - box.Maximum.X;
        vector.Y = sphere.Center.Y - box.Maximum.Y;
        vector.Z = sphere.Center.Z - box.Maximum.Z;

        if (vector.LengthSquared() > radiussquared)
        {
            return Containment.Intersects;
        }

        vector.X = sphere.Center.X - box.Maximum.X;
        vector.Y = sphere.Center.Y - box.Minimum.Y;
        vector.Z = sphere.Center.Z - box.Maximum.Z;

        if (vector.LengthSquared() > radiussquared)
        {
            return Containment.Intersects;
        }

        vector.X = sphere.Center.X - box.Minimum.X;
        vector.Y = sphere.Center.Y - box.Minimum.Y;
        vector.Z = sphere.Center.Z - box.Maximum.Z;

        if (vector.LengthSquared() > radiussquared)
        {
            return Containment.Intersects;
        }

        vector.X = sphere.Center.X - box.Minimum.X;
        vector.Y = sphere.Center.Y - box.Maximum.Y;
        vector.Z = sphere.Center.Z - box.Minimum.Z;

        if (vector.LengthSquared() > radiussquared)
        {
            return Containment.Intersects;
        }

        vector.X = sphere.Center.X - box.Maximum.X;
        vector.Y = sphere.Center.Y - box.Maximum.Y;
        vector.Z = sphere.Center.Z - box.Minimum.Z;

        if (vector.LengthSquared() > radiussquared)
        {
            return Containment.Intersects;
        }

        vector.X = sphere.Center.X - box.Maximum.X;
        vector.Y = sphere.Center.Y - box.Minimum.Y;
        vector.Z = sphere.Center.Z - box.Minimum.Z;

        if (vector.LengthSquared() > radiussquared)
        {
            return Containment.Intersects;
        }

        vector.X = sphere.Center.X - box.Minimum.X;
        vector.Y = sphere.Center.Y - box.Minimum.Y;
        vector.Z = sphere.Center.Z - box.Minimum.Z;

        return vector.LengthSquared() > radiussquared ? Containment.Intersects : Containment.Contains;
    }

    /// <summary>
    /// Determines whether a <see cref="GorgonBoundingSphere"/> contains a <see cref="GorgonBoundingSphere"/>.
    /// </summary>
    /// <param name="sphere1">The first sphere to test.</param>
    /// <param name="sphere2">The second sphere to test.</param>
    /// <returns>The type of containment the two objects have.</returns>
    public static Containment SphereContainsSphere(ref readonly GorgonBoundingSphere sphere1, ref readonly GorgonBoundingSphere sphere2)
    {
        float distance = Vector3.Distance(sphere1.Center, sphere2.Center);


        if (sphere1.Radius + sphere2.Radius < distance)
        {
            return Containment.Disjoint;
        }

        return sphere1.Radius - sphere2.Radius < distance ? Containment.Intersects : Containment.Contains;

    }

    /// <summary>
    /// Checks whether a point lay inside, intersects or lay outside the frustum.
    /// </summary>
    /// <param name="frustum">The frustum to evaluate.</param>
    /// <param name="point">The point to evaluate.</param>
    /// <returns>Type of the containment.</returns>
    public static Containment FrustumContainsPoint(GorgonBoundingFrustum frustum, ref readonly Vector3 point)
    {
        PlaneIntersection result = PlaneIntersection.Front;

        for (int i = 0; i < 6; i++)
        {
            PlaneIntersection planeResult = PlaneIntersectsPoint(in frustum.Planes[i], in point);

            switch (planeResult)
            {
                case PlaneIntersection.Back:
                    return Containment.Disjoint;
                case PlaneIntersection.Intersecting:
                    result = PlaneIntersection.Intersecting;
                    break;
            }
        }

        return result switch
        {
            PlaneIntersection.Intersecting => Containment.Intersects,
            _ => Containment.Contains,
        };
    }

    /// <summary>
    /// Determines the intersection relationship between the frustum and a bounding box.
    /// </summary>
    /// <param name="frustum">The frustum to evalate.</param>
    /// <param name="box">The bounding box to evaluate.</param>
    /// <returns>Type of the containment</returns>
    public static Containment FrustumContainsBox(GorgonBoundingFrustum frustum, ref readonly GorgonBoundingBox box)
    {
        Containment result = Containment.Contains;

        for (int i = 0; i < 6; i++)
        {
            ref readonly Plane plane = ref frustum.Planes[i];

            GetBoxToPlanePVertexNVertex(in box, in plane.Normal, out Vector3 p, out Vector3 n);

            if (PlaneIntersectsPoint(in plane, in p) == PlaneIntersection.Back)
            {
                return Containment.Disjoint;
            }

            if (PlaneIntersectsPoint(in plane, in n) == PlaneIntersection.Back)
            {
                result = Containment.Intersects;
            }
        }
        return result;
    }

    /// <summary>
    /// Determines the intersection relationship between the frustum and a bounding sphere.
    /// </summary>
    /// <param name="frustum">The frustum to evaluate.</param>
    /// <param name="sphere">The sphere to evaluate.</param>
    /// <returns>Type of the containment</returns>
    public static Containment FrustumContainsSphere(GorgonBoundingFrustum frustum, ref readonly GorgonBoundingSphere sphere)
    {
        PlaneIntersection result = PlaneIntersection.Front;

        for (int i = 0; i < 6; i++)
        {
            PlaneIntersection planeResult = PlaneIntersectsSphere(in frustum.Planes[i], in sphere);

            switch (planeResult)
            {
                case PlaneIntersection.Back:
                    return Containment.Disjoint;
                case PlaneIntersection.Intersecting:
                    result = PlaneIntersection.Intersecting;
                    break;
            }
        }

        return result == PlaneIntersection.Intersecting ? Containment.Intersects : Containment.Contains;
    }

    /// <summary>
    /// Checks whether the current BoundingFrustum intersects the specified Plane.
    /// </summary>
    /// <param name="frustum">The frustum to evaluate.</param>
    /// <param name="plane">The plane.</param>
    /// <returns><b>true</b> if the frustum and plane intersect, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool FrustumIntersectsPlane(GorgonBoundingFrustum frustum, ref readonly Plane plane)
    {
        FrustumIntersectsPlane(frustum, in plane, out PlaneIntersection result);
        return result == PlaneIntersection.Intersecting;
    }

    /// <summary>
    /// Checks whether the current frustum intersects a bounding box.
    /// </summary>
    /// <param name="frustum">The frustum to evaluate.</param>
    /// <param name="box">The bounding box to evaluate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool FrustumIntersectsBox(GorgonBoundingFrustum frustum, ref readonly GorgonBoundingBox box) => FrustumContainsBox(frustum, in box) != Containment.Disjoint;

    /// <summary>
    /// Checks whether the current BoundingFrustum intersects the specified Plane.
    /// </summary>
    /// <param name="frustum">The frustum to evaluate.</param>
    /// <param name="plane">The plane.</param>
    /// <param name="result">Plane intersection type.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FrustumIntersectsPlane(GorgonBoundingFrustum frustum, ref readonly Plane plane, out PlaneIntersection result)
    {
        result = PlaneIntersectsPoint(in plane, in frustum.Corners[0]);

        for (int i = 1; i < frustum.Corners.Length; i++)
        {
            if (PlaneIntersectsPoint(in plane, in frustum.Corners[i]) != result)
            {
                result = PlaneIntersection.Intersecting;
                return;
            }
        }
    }

    /// <summary>
    /// Checks whether the current BoundingFrustum intersects the specified Ray.
    /// </summary>
    /// <param name="frustum">The frustum to evaluate.</param>
    /// <param name="ray">The Ray to check for intersection with.</param>
    /// <param name="inDistance">The distance at which the ray enters the frustum if there is an intersection and the ray starts outside the frustum.</param>
    /// <param name="outDistance">The distance at which the ray exits the frustum if there is an intersection.</param>
    /// <returns><c>true</c> if the current BoundingFrustum intersects the specified Ray.</returns>
    public static bool FrustumIntersectsRay(GorgonBoundingFrustum frustum, ref readonly GorgonRay ray, out float? inDistance, out float? outDistance)
    {
        if (FrustumContainsPoint(frustum, in ray.Position) != Containment.Disjoint)
        {
            float nearstPlaneDistance = float.MaxValue;
            for (int i = 0; i < 6; i++)
            {
                Plane plane = frustum.Planes[i];
                if ((RayIntersectsPlane(in ray, in plane, out float distance)) && (distance < nearstPlaneDistance))
                {
                    nearstPlaneDistance = distance;
                }
            }

            inDistance = nearstPlaneDistance;
            outDistance = null;
            return true;
        }

        //We will find the two points at which the ray enters and exists the frustum
        //These two points make a line which center inside the frustum if the ray intersects it
        //Or outside the frustum if the ray intersects frustum planes outside it.
        float minDist = float.MaxValue;
        float maxDist = float.MinValue;
        for (int i = 0; i < 6; i++)
        {
            Plane plane = frustum.Planes[i];
            if (RayIntersectsPlane(in ray, in plane, out float distance))
            {
                minDist = minDist.Min(distance);
                maxDist = maxDist.Max(distance);
            }
        }

        Vector3 minPoint = ray.Position + ray.Direction * minDist;
        Vector3 maxPoint = ray.Position + ray.Direction * maxDist;
        Vector3 center = (minPoint + maxPoint) / 2f;
        if (FrustumContainsPoint(frustum, in center) != Containment.Disjoint)
        {
            inDistance = minDist;
            outDistance = maxDist;
            return true;
        }

        inDistance = null;
        outDistance = null;
        return false;
    }
}
