
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
// Created: February 12, 2021 6:35:57 PM
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
// THE SOFTWARE.

using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using Gorgon.Math;
using Gorgon.Memory;

namespace Gorgon.Graphics.Core;

/// <summary>
/// The names of the frustum planes
/// </summary>
public enum FrustumPlane
{
    /// <summary>
    /// The near plane.
    /// </summary>
    Near = 0,
    /// <summary>
    /// The far plane.
    /// </summary>
    Far = 1,
    /// <summary>
    /// The left plane.
    /// </summary>
    Left = 2,
    /// <summary>
    /// The right plane.
    /// </summary>
    Right = 3,
    /// <summary>
    /// The top plane.
    /// </summary>
    Top = 4,
    /// <summary>
    /// The bottom plane.
    /// </summary>
    Bottom = 5
}

/// <summary>
/// The identifiers for the frustum corners
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
/// Defines a frustum which can be used in frustum culling, zoom to Extents (zoom to fit) operations, 
/// (matrix, frustum, camera) interchange, and many kind of intersection testing
/// </summary>
public class GorgonBoundingFrustum
{
    /// <summary>
    /// The list of corners.
    /// </summary>
    public class CornerList
        : IEnumerable<Vector3>
    {
        // The vectors indicating the corners of the frustum.
        private readonly Vector3[] _corners = new Vector3[8];

        /// <summary>
        /// Property to return the number of corners in the frustum.
        /// </summary>
        public int Length => _corners.Length;

        /// <summary>
        /// Property to return a readonly reference to the plane.
        /// </summary>
        public Vector3 this[FrustumCorner plane] => _corners[(int)plane];

        /// <summary>
        /// Property to return a readonly reference to the plane by index.
        /// </summary>
        public Vector3 this[int index] => _corners[index];

        /// <summary>
        /// Function to assign a corner.
        /// </summary>
        /// <param name="frustumCorner">The corner to assign.</param>
        /// <param name="cornerPosition">The corner data.</param>
        internal void Set(FrustumCorner frustumCorner, Vector3 cornerPosition) => _corners[(int)frustumCorner] = cornerPosition;

        /// <summary>
        /// Function to retrieve the corners as a read only span.
        /// </summary>
        /// <returns>The corners in a read only span.</returns>
        public ReadOnlySpan<Vector3> GetReadOnlySpan() => _corners.AsSpan();

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

    }

    /// <summary>
    /// The list of planes.
    /// </summary>
    public class PlaneList
        : IEnumerable<Plane>
    {
        // The list of planes for the frustum.
        private readonly Plane[] _planes = new Plane[6];

        /// <summary>
        /// Property to return the number of planes in the frustum.
        /// </summary>
        public int Length => _planes.Length;

        /// <summary>
        /// Property to return a readonly reference to the plane.
        /// </summary>
        public Plane this[FrustumPlane plane] => _planes[(int)plane];

        /// <summary>
        /// Property to return a readonly reference to the plane by index.
        /// </summary>
        public Plane this[int index] => _planes[index];

        /// <summary>
        /// Function to assign a plane.
        /// </summary>
        /// <param name="frustumPlane">The orientation of the plane to assign.</param>
        /// <param name="plane">The plane data.</param>
        internal void Set(FrustumPlane frustumPlane, Plane plane) => _planes[(int)frustumPlane] = plane;

        /// <summary>
        /// Function to retrieve the corners as a read only span.
        /// </summary>
        /// <returns>The corners in a read only span.</returns>
        public ReadOnlySpan<Plane> GetReadOnlySpan() => _planes.AsSpan();

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
    }

    /// <summary>
    /// Property to return the plane list for this frustum.
    /// </summary>
    public PlaneList Planes
    {
        get;
    } = new PlaneList();

    /// <summary>
    /// Property to return the corner list for this frustum.
    /// </summary>
    public CornerList Corners
    {
        get;
    } = new CornerList();

    /// <summary>
    /// Indicate whether the current BoundingFrustrum is Orthographic.
    /// </summary>
    public bool IsOrthographic => (Planes[FrustumPlane.Left].Normal.Equals(-Planes[FrustumPlane.Right].Normal)) && ((-Planes[FrustumPlane.Top].Normal).Equals((-Planes[FrustumPlane.Bottom].Normal)));

    /// <summary>
    /// Function to create a plane based on 3 points.
    /// </summary>
    /// <param name="point1">The first point.</param>
    /// <param name="point2">The second point.</param>
    /// <param name="point3">The third point.</param>
    /// <returns>The new plane.</returns>
    private static Plane CreatePlaneFromPoints(Vector3 point1, Vector3 point2, Vector3 point3)
    {
        float x1 = point2.X - point1.X;
        float y1 = point2.Y - point1.Y;
        float z1 = point2.Z - point1.Z;
        float x2 = point3.X - point1.X;
        float y2 = point3.Y - point1.Y;
        float z2 = point3.Z - point1.Z;
        float yz = (y1 * z2) - (z1 * y2);
        float xz = (z1 * x2) - (x1 * z2);
        float xy = (x1 * y2) - (y1 * x2);
        float invPyth = 1.0f / ((yz * yz) + (xz * xz) + (xy * xy)).Sqrt();

        Plane plane = new()
        {
            Normal = new Vector3(yz * invPyth, xz * invPyth, xy * invPyth),
        };
        plane.D = -((plane.Normal.X * point1.X) + (plane.Normal.Y * point1.Y) + (plane.Normal.Z * point1.Z));

        return plane;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <returns></returns>
    private static Vector3 Get3PlanesInterPoint(Plane p1, Plane p2, Plane p3)
    {
        //P = -d1 * N2xN3 / N1.N2xN3 - d2 * N3xN1 / N2.N3xN1 - d3 * N1xN2 / N3.N1xN2 
        Vector3 v =
            -p1.D * Vector3.Cross(p2.Normal, p3.Normal) / Vector3.Dot(p1.Normal, Vector3.Cross(p2.Normal, p3.Normal))
            - p2.D * Vector3.Cross(p3.Normal, p1.Normal) / Vector3.Dot(p2.Normal, Vector3.Cross(p3.Normal, p1.Normal))
            - p3.D * Vector3.Cross(p1.Normal, p2.Normal) / Vector3.Dot(p3.Normal, Vector3.Cross(p1.Normal, p2.Normal));

        return v;
    }

    /// <summary>
    /// Function to create a new bounding frustum by building the planes based on the view and projection matrix.
    /// </summary>
    /// <param name="viewProjMatrix">The combined view and projection matrix.</param>
    /// <returns>The new bounding frustum.</returns>
    public static GorgonBoundingFrustum Create(ref readonly Matrix4x4 viewProjMatrix)
    {
        //http://www.chadvernon.com/blog/resources/directx9/frustum-culling/

        GorgonBoundingFrustum result = new();

        result.Update(in viewProjMatrix);

        return result;
    }

    /// <summary>
    /// Function to create a new bounding frustum by building the planes based on the view and projection matrix.
    /// </summary>
    /// <param name="viewMatrix">The view matrix.</param>
    /// <param name="projectionMatrix">The projection matrix.</param>
    /// <returns>The new bounding frustum.</returns>
    public static GorgonBoundingFrustum Create(ref readonly Matrix4x4 viewMatrix, ref readonly Matrix4x4 projectionMatrix)
    {
        Matrix4x4 result = Matrix4x4.Multiply(viewMatrix, projectionMatrix);
        return Create(in result);
    }

    /// <summary>
    /// Function to create an inverted (inside out) frustum.
    /// </summary>
    /// <param name="frustum">The frustum to invert.</param>
    /// <returns>The inverted frustum.</returns>
    public static GorgonBoundingFrustum CreateInverted(GorgonBoundingFrustum frustum)
    {
        GorgonBoundingFrustum result = new();
        result.Planes.Set(FrustumPlane.Near, new Plane(-frustum.Planes[FrustumPlane.Near].Normal, frustum.Planes[FrustumPlane.Near].D));
        result.Planes.Set(FrustumPlane.Far, new Plane(-frustum.Planes[FrustumPlane.Far].Normal, frustum.Planes[FrustumPlane.Far].D));
        result.Planes.Set(FrustumPlane.Left, new Plane(-frustum.Planes[FrustumPlane.Left].Normal, frustum.Planes[FrustumPlane.Left].D));
        result.Planes.Set(FrustumPlane.Right, new Plane(-frustum.Planes[FrustumPlane.Right].Normal, frustum.Planes[FrustumPlane.Right].D));
        result.Planes.Set(FrustumPlane.Top, new Plane(-frustum.Planes[FrustumPlane.Top].Normal, frustum.Planes[FrustumPlane.Top].D));
        result.Planes.Set(FrustumPlane.Bottom, new Plane(-frustum.Planes[FrustumPlane.Bottom].Normal, frustum.Planes[FrustumPlane.Bottom].D));
        return result;
    }

    /// <summary>
    /// Creates a new frustum relaying on perspective camera parameters
    /// </summary>
    /// <param name="cameraPos">The camera pos.</param>
    /// <param name="lookDir">The look dir.</param>
    /// <param name="upDir">Up dir.</param>
    /// <param name="fov">The fov.</param>
    /// <param name="znear">The znear.</param>
    /// <param name="zfar">The zfar.</param>
    /// <param name="aspect">The aspect.</param>
    /// <returns>The bounding frustum calculated from perspective camera</returns>
    public static GorgonBoundingFrustum FromCamera(Vector3 cameraPos, Vector3 lookDir, Vector3 upDir, float fov, float znear, float zfar, float aspect)
    {
        //http://knol.google.com/k/view-frustum

        lookDir = Vector3.Normalize(lookDir);
        upDir = Vector3.Normalize(upDir);

        Vector3 nearCenter = cameraPos + lookDir * znear;
        Vector3 farCenter = cameraPos + lookDir * zfar;
        float nearHalfHeight = (float)(znear * (fov / 2f).Tan());
        float farHalfHeight = (float)(zfar * (fov / 2f).Tan());
        float nearHalfWidth = nearHalfHeight * aspect;
        float farHalfWidth = farHalfHeight * aspect;

        Vector3 rightDir = Vector3.Normalize(Vector3.Cross(upDir, lookDir));
        Vector3 Near1 = nearCenter - nearHalfHeight * upDir + nearHalfWidth * rightDir;
        Vector3 Near2 = nearCenter + nearHalfHeight * upDir + nearHalfWidth * rightDir;
        Vector3 Near3 = nearCenter + nearHalfHeight * upDir - nearHalfWidth * rightDir;
        Vector3 Near4 = nearCenter - nearHalfHeight * upDir - nearHalfWidth * rightDir;
        Vector3 Far1 = farCenter - farHalfHeight * upDir + farHalfWidth * rightDir;
        Vector3 Far2 = farCenter + farHalfHeight * upDir + farHalfWidth * rightDir;
        Vector3 Far3 = farCenter + farHalfHeight * upDir - farHalfWidth * rightDir;
        Vector3 Far4 = farCenter - farHalfHeight * upDir - farHalfWidth * rightDir;

        GorgonBoundingFrustum result = new();
        Plane plane = CreatePlaneFromPoints(Near1, Near2, Near3);
        plane = Plane.Normalize(plane);
        result.Planes.Set(FrustumPlane.Near, plane);
        plane = CreatePlaneFromPoints(Far3, Far2, Far1);
        plane = Plane.Normalize(plane);
        result.Planes.Set(FrustumPlane.Far, plane);
        plane = CreatePlaneFromPoints(Near4, Near3, Far3);
        plane = Plane.Normalize(plane);
        result.Planes.Set(FrustumPlane.Left, plane);
        plane = CreatePlaneFromPoints(Far1, Far2, Near2);
        plane = Plane.Normalize(plane);
        result.Planes.Set(FrustumPlane.Right, plane);
        plane = CreatePlaneFromPoints(Near2, Far2, Far3);
        plane = Plane.Normalize(plane);
        result.Planes.Set(FrustumPlane.Top, plane);
        plane = CreatePlaneFromPoints(Far4, Far1, Near1);
        plane = Plane.Normalize(plane);
        result.Planes.Set(FrustumPlane.Bottom, plane);

        return result;
    }

    /// <summary>
    /// Function to update the frustum with a new view/projection matrix.
    /// </summary>
    /// <param name="viewProjection">The view and projection matrix used to calculate the planes and corners for the frustum.</param>
    public void Update(ref readonly Matrix4x4 viewProjection)
    {
        // Left plane
        Plane plane;
        Vector3 normal;
        float d;
        normal.X = viewProjection.M14 + viewProjection.M11;
        normal.Y = viewProjection.M24 + viewProjection.M21;
        normal.Z = viewProjection.M34 + viewProjection.M31;
        d = viewProjection.M44 + viewProjection.M41;

        plane = Plane.Normalize(new Plane(normal, d));
        Planes.Set(FrustumPlane.Left, plane);

        // Right plane
        normal.X = viewProjection.M14 - viewProjection.M11;
        normal.Y = viewProjection.M24 - viewProjection.M21;
        normal.Z = viewProjection.M34 - viewProjection.M31;
        d = viewProjection.M44 - viewProjection.M41;

        plane = Plane.Normalize(new Plane(normal, d));
        Planes.Set(FrustumPlane.Right, plane);

        // Top plane
        normal.X = viewProjection.M14 - viewProjection.M12;
        normal.Y = viewProjection.M24 - viewProjection.M22;
        normal.Z = viewProjection.M34 - viewProjection.M32;
        d = viewProjection.M44 - viewProjection.M42;

        plane = Plane.Normalize(new Plane(normal, d));
        Planes.Set(FrustumPlane.Top, plane);

        // Bottom plane
        normal.X = viewProjection.M14 + viewProjection.M12;
        normal.Y = viewProjection.M24 + viewProjection.M22;
        normal.Z = viewProjection.M34 + viewProjection.M32;
        d = viewProjection.M44 + viewProjection.M42;

        plane = Plane.Normalize(new Plane(normal, d));
        Planes.Set(FrustumPlane.Bottom, plane);

        // Near plane
        normal.X = viewProjection.M13;
        normal.Y = viewProjection.M23;
        normal.Z = viewProjection.M33;
        d = viewProjection.M43;

        plane = Plane.Normalize(new Plane(normal, d));
        Planes.Set(FrustumPlane.Near, plane);

        // Far plane
        normal.X = viewProjection.M14 - viewProjection.M13;
        normal.Y = viewProjection.M24 - viewProjection.M23;
        normal.Z = viewProjection.M34 - viewProjection.M33;
        d = viewProjection.M44 - viewProjection.M43;

        plane = Plane.Normalize(new Plane(normal, d));
        Planes.Set(FrustumPlane.Far, plane);

        Corners.Set(FrustumCorner.BottomRightNear, Get3PlanesInterPoint(Planes[FrustumPlane.Near], Planes[FrustumPlane.Bottom], Planes[FrustumPlane.Right]));
        Corners.Set(FrustumCorner.TopRightNear, Get3PlanesInterPoint(Planes[FrustumPlane.Near], Planes[FrustumPlane.Top], Planes[FrustumPlane.Right]));
        Corners.Set(FrustumCorner.TopLeftNear, Get3PlanesInterPoint(Planes[FrustumPlane.Near], Planes[FrustumPlane.Top], Planes[FrustumPlane.Left]));
        Corners.Set(FrustumCorner.BottomLeftNear, Get3PlanesInterPoint(Planes[FrustumPlane.Near], Planes[FrustumPlane.Bottom], Planes[FrustumPlane.Left]));

        Corners.Set(FrustumCorner.BottomRightFar, Get3PlanesInterPoint(Planes[FrustumPlane.Far], Planes[FrustumPlane.Bottom], Planes[FrustumPlane.Right]));
        Corners.Set(FrustumCorner.TopRightFar, Get3PlanesInterPoint(Planes[FrustumPlane.Far], Planes[FrustumPlane.Top], Planes[FrustumPlane.Right]));
        Corners.Set(FrustumCorner.TopLeftFar, Get3PlanesInterPoint(Planes[FrustumPlane.Far], Planes[FrustumPlane.Top], Planes[FrustumPlane.Left]));
        Corners.Set(FrustumCorner.BottomLeftFar, Get3PlanesInterPoint(Planes[FrustumPlane.Far], Planes[FrustumPlane.Bottom], Planes[FrustumPlane.Left]));
    }

    /// <summary>
    /// Function to update the frustum with a new view/projection matrix.
    /// </summary>
    /// <param name="view">The view matrix used to calculate the planes and corners for the frustum.</param>
    /// <param name="projection">The projection matrix used to calculate the planes and corners for the frustum.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(ref readonly Matrix4x4 view, ref readonly Matrix4x4 projection)
    {
        Matrix4x4 viewProj = Matrix4x4.Multiply(view, projection);
        Update(in viewProj);
    }

    /// <summary>
    /// Get the width of the frustum at specified depth.
    /// </summary>
    /// <param name="depth">the depth at which to calculate frustum width.</param>
    /// <returns>With of the frustum at the specified depth</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetWidthAtDepth(float depth)
    {
        float hAngle = (MathF.PI * 0.5f - (Vector3.Dot(Planes[FrustumPlane.Near].Normal, Planes[FrustumPlane.Left].Normal)).ACos());
        return hAngle.Tan() * depth * 2;
    }

    /// <summary>
    /// Get the height of the frustum at specified depth.
    /// </summary>
    /// <param name="depth">the depth at which to calculate frustum height.</param>
    /// <returns>Height of the frustum at the specified depth</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetHeightAtDepth(float depth)
    {
        float vAngle = (MathF.PI * 0.5f - (Vector3.Dot(Planes[FrustumPlane.Near].Normal, Planes[FrustumPlane.Top].Normal)).ACos());
        return vAngle.Tan() * depth * 2;
    }

    /// <summary>
    /// Get the distance which when added to camera position along the lookat direction will do the effect of zoom to extents (zoom to fit) operation,
    /// so all the passed points will fit in the current view.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <returns>if the returned value is positive, the camera will move toward the lookat direction (ZoomIn); otherwise if the returned value is negative, the camera will move in the reverse direction of the lookat direction (ZoomOut).</returns>
    public float GetZoomToExtentsShiftDistance(ReadOnlySpan<Vector3> points)
    {
        float vAngle = MathF.PI * 0.5f - (Vector3.Dot(Planes[FrustumPlane.Near].Normal, Planes[FrustumPlane.Top].Normal)).ACos();
        float vSin = vAngle.FastSin();
        float hAngle = MathF.PI * 0.5f - (Vector3.Dot(Planes[FrustumPlane.Near].Normal, Planes[FrustumPlane.Left].Normal)).ACos();
        float hSin = hAngle.FastSin();
        float horizontalToVerticalMapping = vSin / hSin;

        GorgonBoundingFrustum ioFrustrum = CreateInverted(this);

        float maxPointDist = float.MinValue;
        for (int i = 0; i < points.Length; i++)
        {
            float pointDist = GorgonIntersections.DistancePlanePoint(ioFrustrum.Planes[FrustumPlane.Top], points[i]);
            pointDist = pointDist.Max(GorgonIntersections.DistancePlanePoint(ioFrustrum.Planes[FrustumPlane.Bottom], points[i]));
            pointDist = pointDist.Max(GorgonIntersections.DistancePlanePoint(ioFrustrum.Planes[FrustumPlane.Left], points[i]) * horizontalToVerticalMapping);
            pointDist = pointDist.Max(GorgonIntersections.DistancePlanePoint(ioFrustrum.Planes[FrustumPlane.Right], points[i]) * horizontalToVerticalMapping);

            maxPointDist = maxPointDist.Max(pointDist);
        }
        return -maxPointDist / vSin;
    }

    /// <summary>
    /// Get the distance which when added to camera position along the lookat direction will do the effect of zoom to extents (zoom to fit) operation,
    /// so all the passed points will fit in the current view.
    /// if the returned value is positive, the camera will move toward the lookat direction (ZoomIn).
    /// if the returned value is negative, the camera will move in the reverse direction of the lookat direction (ZoomOut).
    /// </summary>
    /// <param name="boundingBox">The bounding box.</param>
    /// <returns>The zoom to fit distance</returns>
    public float GetZoomToExtentsShiftDistance(ref readonly GorgonBoundingBox boundingBox)
    {
        Vector3[] corners = GorgonArrayPool<Vector3>.SharedTiny.Rent(8);

        try
        {
            corners[0] = boundingBox.TopLeftBack;
            corners[1] = boundingBox.TopRightBack;
            corners[2] = boundingBox.BottomLeftBack;
            corners[3] = boundingBox.BottomRightBack;
            corners[4] = boundingBox.TopLeftFront;
            corners[5] = boundingBox.TopRightFront;
            corners[6] = boundingBox.BottomLeftFront;
            corners[7] = boundingBox.BottomRightFront;
            return GetZoomToExtentsShiftDistance(corners);
        }
        finally
        {
            GorgonArrayPool<Vector3>.SharedTiny.Return(corners);
        }
    }

    /// <summary>
    /// Get the vector shift which when added to camera position will do the effect of zoom to extents (zoom to fit) operation,
    /// so all the passed points will fit in the current view.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <returns>The zoom to fit vector</returns>
    public Vector3 GetZoomToExtentsShiftVector(ReadOnlySpan<Vector3> points) => GetZoomToExtentsShiftDistance(points) * Planes[FrustumPlane.Near].Normal;

    /// <summary>
    /// Get the vector shift which when added to camera position will do the effect of zoom to extents (zoom to fit) operation,
    /// so all the passed points will fit in the current view.
    /// </summary>
    /// <param name="boundingBox">The bounding box.</param>
    /// <returns>The zoom to fit vector</returns>
    public Vector3 GetZoomToExtentsShiftVector(ref readonly GorgonBoundingBox boundingBox) => GetZoomToExtentsShiftDistance(in boundingBox) * Planes[FrustumPlane.Near].Normal;

}
