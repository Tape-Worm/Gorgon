
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: June 9, 2018 10:57:03 AM
// 


using System.Numerics;
using System.Runtime.CompilerServices;
using Gorgon.Math;
using Gorgon.Renderers.Geometry;

namespace Gorgon.Renderers;


/// <summary>
/// Provides functionality for transforming renderable vertices
/// </summary>
internal class SpriteTransformer
{
    /// <summary>
    /// Function to build up the renderable vertices.
    /// </summary>
    /// <param name="renderable">The sprite renderable to evaluate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void BuildRenderable(BatchRenderable renderable)
    {
        Vector2 vectorSize = new(renderable.Bounds.Width, renderable.Bounds.Height);
        Vector2 axisOffset = Vector2.Multiply(renderable.Anchor, vectorSize);
        renderable.Corners = new Vector4(-axisOffset.X, -axisOffset.Y, vectorSize.X - axisOffset.X, vectorSize.Y - axisOffset.Y);
    }

    /// <summary>
    /// Function to update the texture coordinates for the renderable.
    /// </summary>
    /// <param name="vertices">The vertices for the renderable.</param>
    /// <param name="renderable">The sprite renderable to evaluate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdateTextureCoordinates(Gorgon2DVertex[] vertices, BatchRenderable renderable)
    {
        Vector4 rightBottom = new(renderable.HorizontalFlip ? renderable.TextureRegion.Left : renderable.TextureRegion.Right,
                                             renderable.VerticalFlip ? renderable.TextureRegion.Top : renderable.TextureRegion.Bottom,
                                             renderable.TextureArrayIndex, 1.0f);
        Vector4 leftTop = new(renderable.HorizontalFlip ? renderable.TextureRegion.Right : renderable.TextureRegion.Left,
                                         renderable.VerticalFlip ? renderable.TextureRegion.Bottom : renderable.TextureRegion.Top,
                                         renderable.TextureArrayIndex, 1.0f);

        if ((!renderable.LowerLeftOffset.Equals(Vector3.Zero))
            || (!renderable.UpperLeftOffset.Equals(Vector3.Zero))
            || (!renderable.LowerRightOffset.Equals(Vector3.Zero))
            || (!renderable.UpperRightOffset.Equals(Vector3.Zero)))
        {
            BuildPerspectiveModifier(vertices, ref leftTop, ref rightBottom, renderable.TextureArrayIndex);
            return;
        }

        vertices[0].UV = leftTop;
        vertices[1].UV = new Vector4(rightBottom.X, leftTop.Y, renderable.TextureArrayIndex, 1.0f);
        vertices[2].UV = new Vector4(leftTop.X, rightBottom.Y, renderable.TextureArrayIndex, 1.0f);
        vertices[3].UV = rightBottom;
    }

    /// <summary>
    /// Function to build the modifier value for perspective warping when vertex offsets are assigned.
    /// </summary>
    /// <param name="vertices">The vertices to update.</param>
    /// <param name="leftTop">The upper left texture coordinate.</param>
    /// <param name="rightBottom">The lower right texture coordinate.</param>
    /// <param name="arrayIndex">Th texture array index.</param>
    /// <remarks>
    /// <para>
    /// Adapted from code at <a href="https://pumpkin-games.net/wp/?p=215"/>
    /// </para>
    /// </remarks>
    private static void BuildPerspectiveModifier(Gorgon2DVertex[] vertices, ref Vector4 leftTop, ref Vector4 rightBottom, int arrayIndex)
    {
        Vector2 v0 = new(vertices[0].Position.X, vertices[0].Position.Y);
        Vector2 v1 = new(vertices[1].Position.X, vertices[1].Position.Y);
        Vector2 v2 = new(vertices[2].Position.X, vertices[2].Position.Y);
        Vector2 v3 = new(vertices[3].Position.X, vertices[3].Position.Y);
        ref Vector4 uv0 = ref vertices[0].UV;
        ref Vector4 uv1 = ref vertices[1].UV;
        ref Vector4 uv2 = ref vertices[2].UV;
        ref Vector4 uv3 = ref vertices[3].UV;

        uv0.Z = uv1.Z = uv2.Z = uv3.Z = arrayIndex;
        uv0.W = uv1.W = uv2.W = uv3.W = 1.0f;

        Vector2 va = Vector2.Subtract(v3, v0);
        Vector2 vb = Vector2.Subtract(v2, v1);
        float cross = va.X * vb.Y - va.Y * vb.X;

        if (cross == 0)
        {
            return;
        }

        Vector2 vc = Vector2.Subtract(v0, v1);
        float u = (va.X * vc.Y - va.Y * vc.X) / cross;
        float v = (vb.X * vc.Y - vb.Y * vc.X) / cross;

        if ((u < 0) || (u >= 1) || (v < 0) || (v >= 1))
        {
            return;
        }

        float qv0 = 1 / (1 - v);
        float qu0 = 1 / (1 - u);
        float qu1 = 1 / u;
        float qv1 = 1 / v;

        uv0.X = leftTop.X * qv0;
        uv0.Y = leftTop.Y * qv0;
        uv0.W = qv0;

        uv1.X = rightBottom.X * qu0;
        uv1.Y = leftTop.Y * qu0;
        uv1.W = qu0;

        uv2.X = leftTop.X * qu1;
        uv2.Y = rightBottom.Y * qu1;
        uv2.W = qu1;

        uv3.X = rightBottom.X * qv1;
        uv3.Y = rightBottom.Y * qv1;
        uv3.W = qv1;
    }

    /// <summary>
    /// Function to transform each vertex of the renderable to change its location, size and rotation.
    /// </summary>
    /// <param name="renderable">The sprite renderable to render.</param>
    /// <param name="angleSin">The sine of the rotation angle.</param>
    /// <param name="angleCos">The cosine of the rotation angle.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TransformVertices(BatchRenderable renderable,
                                          float angleSin,
                                          float angleCos)
    {
        Vector2 upperLeft = new(renderable.Corners.X + renderable.UpperLeftOffset.X, renderable.Corners.Y + renderable.UpperLeftOffset.Y);
        Vector2 upperRight = new(renderable.Corners.Z + renderable.UpperRightOffset.X, renderable.Corners.Y + renderable.UpperRightOffset.Y);
        Vector2 lowerRight = new(renderable.Corners.Z + renderable.LowerRightOffset.X, renderable.Corners.W + renderable.LowerRightOffset.Y);
        Vector2 lowerLeft = new(renderable.Corners.X + renderable.LowerLeftOffset.X, renderable.Corners.W + renderable.LowerLeftOffset.Y);

        ref Gorgon2DVertex v1 = ref renderable.Vertices[0];
        ref Gorgon2DVertex v2 = ref renderable.Vertices[1];
        ref Gorgon2DVertex v3 = ref renderable.Vertices[2];
        ref Gorgon2DVertex v4 = ref renderable.Vertices[3];

        v1.Angle =
        v2.Angle =
        v3.Angle =
        v4.Angle = new Vector2(angleCos, angleSin);

        upperLeft = Vector2.Multiply(upperLeft, renderable.Scale);
        upperRight = Vector2.Multiply(upperRight, renderable.Scale);
        lowerRight = Vector2.Multiply(lowerRight, renderable.Scale);
        lowerLeft = Vector2.Multiply(lowerLeft, renderable.Scale);

        v1.Position = new Vector4(((upperLeft.X * angleCos) - (upperLeft.Y * angleSin)) + renderable.Bounds.Left,
                            ((upperLeft.X * angleSin) + (upperLeft.Y * angleCos)) + renderable.Bounds.Top,
                            renderable.Depth + renderable.UpperLeftOffset.Z, 1);

        v2.Position = new Vector4(((upperRight.X * angleCos) - (upperRight.Y * angleSin)) + renderable.Bounds.Left,
                                ((upperRight.X * angleSin) + (upperRight.Y * angleCos)) + renderable.Bounds.Top,
                                renderable.Depth + renderable.UpperRightOffset.Z, 1);

        v3.Position = new Vector4(((lowerLeft.X * angleCos) - (lowerLeft.Y * angleSin)) + renderable.Bounds.Left,
                                ((lowerLeft.X * angleSin) + (lowerLeft.Y * angleCos)) + renderable.Bounds.Top,
                                renderable.Depth + renderable.LowerLeftOffset.Z, 1);

        v4.Position = new Vector4(((lowerRight.X * angleCos) - (lowerRight.Y * angleSin)) + renderable.Bounds.Left,
                                ((lowerRight.X * angleSin) + (lowerRight.Y * angleCos)) + renderable.Bounds.Top,
                                renderable.Depth + renderable.LowerRightOffset.Z, 1);
    }

    /// <summary>
    /// Function to transform the vertices for a renderable.
    /// </summary>
    /// <param name="renderable">The renderable to transform.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Transform(BatchRenderable renderable)
    {
        if (renderable.HasVertexChanges)
        {
            BuildRenderable(renderable);

            // If we've updated the physical dimensions for the renderable, then we need to update the transform as well.
            renderable.HasTransformChanges = true;
            renderable.HasVertexChanges = false;
        }

        if (renderable.HasTransformChanges)
        {
            float rads = renderable.AngleDegs.ToRadians();

            TransformVertices(renderable, rads.FastSin(), rads.FastCos());
            renderable.HasTransformChanges = false;
        }

        if (renderable.HasTextureChanges)
        {
            UpdateTextureCoordinates(renderable.Vertices, renderable);
            renderable.HasTextureChanges = false;
        }
    }
}
