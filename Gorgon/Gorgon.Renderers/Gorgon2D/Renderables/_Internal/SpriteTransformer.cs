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
// Created: June 9, 2018 10:57:03 AM
// 
#endregion

using System.Runtime.CompilerServices;
using Gorgon.Math;
using Gorgon.Graphics;
using Gorgon.Renderers.Geometry;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// Provides functionality for transforming renderable vertices.
    /// </summary>
    internal class SpriteTransformer
    {
        /// <summary>
        /// Function to build up the renderable vertices.
        /// </summary>
        /// <param name="bounds">The bounds of the renderable.</param>
        /// <param name="anchor">The anchor point for the renderable.</param>
        /// <param name="corners">The corners of the renderable.</param>
        private static void BuildRenderable(ref DX.RectangleF bounds, ref DX.Vector2 anchor, out DX.Vector4 corners)
        {
            var vectorSize = new DX.Vector2(bounds.Width, bounds.Height);
            DX.Vector2 axisOffset = default;

            if ((anchor.X != 0) || (anchor.Y != 0))
            {
                DX.Vector2.Multiply(ref anchor, ref vectorSize, out axisOffset);
            }

            corners = new DX.Vector4(-axisOffset.X, -axisOffset.Y, vectorSize.X - axisOffset.X, vectorSize.Y - axisOffset.Y);
        }

        /// <summary>
        /// Function to update the colors for each corner of the renderable.
        /// </summary>
        /// <param name="vertices">The vertices for the renderable.</param>
        /// <param name="upperLeft">The color of the upper left corner.</param>
        /// <param name="upperRight">The color of the upper right corner.</param>
        /// <param name="lowerLeft">The color of the lower left corner.</param>
        /// <param name="lowerRight">The color of the lower right corner.</param>
        private static void UpdateVertexColors(Gorgon2DVertex[] vertices, ref GorgonColor upperLeft, ref GorgonColor upperRight, ref GorgonColor lowerLeft, ref GorgonColor lowerRight)
        {
            vertices[0].Color = upperLeft;
            vertices[1].Color = upperRight;
            vertices[2].Color = lowerLeft;
            vertices[3].Color = lowerRight;
        }

        /// <summary>
        /// Function to update the texture coordinates for the renderable.
        /// </summary>
        /// <param name="vertices">The vertices for the renderable.</param>
        /// <param name="textureRegion">The texture coordinates.</param>
        /// <param name="textureArrayIndex">The index into a texture array.</param>
        /// <param name="horizontalFlip"><b>true</b> if the texture is flipped horizontally.</param>
        /// <param name="verticalFlip"><b>true</b> if the texture is flipped vertically.</param>
        /// <param name="perspectCorrect"><b>true</b> to apply perspective correction to sprites with corner offsets, <b>false</b> to use standard affine texturing.</param>
        private static void UpdateTextureCoordinates(Gorgon2DVertex[] vertices,
                                                     ref DX.RectangleF textureRegion,
                                                     int textureArrayIndex,
                                                     bool horizontalFlip,
                                                     bool verticalFlip,
                                                     bool perspectCorrect)
        {
            var rightBottom = new DX.Vector4(textureRegion.Right, textureRegion.Bottom, textureArrayIndex, 1.0f);
            var leftTop = new DX.Vector4(textureRegion.Left, textureRegion.Top, textureArrayIndex, 1.0f);

            if (horizontalFlip)
            {
                leftTop.X = textureRegion.Right;
                rightBottom.X = textureRegion.Left;
            }

            if (verticalFlip)
            {
                leftTop.Y = textureRegion.Bottom;
                rightBottom.Y = textureRegion.Top;
            }

            if (perspectCorrect)
            {
                BuildPerspectiveModifier(vertices, ref leftTop, ref rightBottom, textureArrayIndex);
                return;
            }
            
            vertices[0].UV = leftTop;
            vertices[1].UV = new DX.Vector4(rightBottom.X, leftTop.Y, textureArrayIndex, 1.0f);
            vertices[2].UV = new DX.Vector4(leftTop.X, rightBottom.Y, textureArrayIndex, 1.0f);
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
        private static void BuildPerspectiveModifier(Gorgon2DVertex[] vertices, ref DX.Vector4 leftTop, ref DX.Vector4 rightBottom, int arrayIndex)
        {
            var v0 = new DX.Vector2(vertices[0].Position.X, vertices[0].Position.Y);
            var v1 = new DX.Vector2(vertices[1].Position.X, vertices[1].Position.Y);
            var v2 = new DX.Vector2(vertices[2].Position.X, vertices[2].Position.Y);
            var v3 = new DX.Vector2(vertices[3].Position.X, vertices[3].Position.Y);
            ref DX.Vector4 uv0 = ref vertices[0].UV;
            ref DX.Vector4 uv1 = ref vertices[1].UV;
            ref DX.Vector4 uv2 = ref vertices[2].UV;
            ref DX.Vector4 uv3 = ref vertices[3].UV;

            uv0.Z = uv1.Z = uv2.Z = uv3.Z = arrayIndex;
            uv0.W = uv1.W = uv2.W = uv3.W = 1.0f;

            DX.Vector2.Subtract(ref v3, ref v0, out DX.Vector2 va);
            DX.Vector2.Subtract(ref v2, ref v1, out DX.Vector2 vb);
            float cross = va.X * vb.Y - va.Y * vb.X;

            if (cross == 0)
            {                                
                return;
            }

            DX.Vector2.Subtract(ref v0, ref v1, out DX.Vector2 vc);            
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
        /// <param name="vertices">The vertices for the renderable.</param>
        /// <param name="corners">The corners of the renderable.</param>
        /// <param name="bounds">The boundaries for the renderable.</param>
        /// <param name="scale">The scale of the renderable.</param>
        /// <param name="angleRads">The cached angle, in radians.</param>
        /// <param name="angleSin">The cached sine of the angle.</param>
        /// <param name="angleCos">The cached cosine of the angle.</param>
        /// <param name="depth">The depth value for the renderable.</param>
        /// <param name="cornerUpperLeft">The upper left corner offset.</param>
        /// <param name="cornerUpperRight">The upper right corner offset.</param>
        /// <param name="cornerLowerLeft">The lower left corner offset.</param>
        /// <param name="cornerLowerRight">The lower right corner offset.</param>
        private static void TransformVertices(Gorgon2DVertex[] vertices,
                                              ref DX.Vector4 corners,
                                              ref DX.RectangleF bounds,
                                              ref DX.Vector2 scale,
                                              float angleRads,
                                              float angleSin,
                                              float angleCos,
                                              float depth,
                                              ref DX.Vector3 cornerUpperLeft,
                                              ref DX.Vector3 cornerUpperRight,
                                              ref DX.Vector3 cornerLowerLeft,
                                              ref DX.Vector3 cornerLowerRight)
        {
            var upperLeft = new DX.Vector2(corners.X + cornerUpperLeft.X, corners.Y + cornerUpperLeft.Y);
            var upperRight = new DX.Vector2(corners.Z + cornerUpperRight.X, corners.Y + cornerUpperRight.Y);
            var lowerRight = new DX.Vector2(corners.Z + cornerLowerRight.X, corners.W + cornerLowerRight.Y);
            var lowerLeft = new DX.Vector2(corners.X + cornerLowerLeft.X, corners.W + cornerLowerLeft.Y);

            if ((scale.X != 1.0f) || (scale.Y != 1.0f))
            {
                DX.Vector2.Multiply(ref upperLeft, ref scale, out upperLeft);
                DX.Vector2.Multiply(ref upperRight, ref scale, out upperRight);
                DX.Vector2.Multiply(ref lowerRight, ref scale, out lowerRight);
                DX.Vector2.Multiply(ref lowerLeft, ref scale, out lowerLeft);
            }

            ref Gorgon2DVertex v1 = ref vertices[0];
            ref Gorgon2DVertex v2 = ref vertices[1];
            ref Gorgon2DVertex v3 = ref vertices[2];
            ref Gorgon2DVertex v4 = ref vertices[3];

            if (angleRads != 0.0f)
            {
                v1.Position = new DX.Vector4(((upperLeft.X * angleCos) - (upperLeft.Y * angleSin)) + bounds.Left,
                                    ((upperLeft.X * angleSin) + (upperLeft.Y * angleCos)) + bounds.Top,
                                    depth + cornerUpperLeft.Z, 1);
                v1.Angle = new DX.Vector2(angleCos, angleSin);

                v2.Position = new DX.Vector4(((upperRight.X * angleCos) - (upperRight.Y * angleSin)) + bounds.Left,
                                     ((upperRight.X * angleSin) + (upperRight.Y * angleCos)) + bounds.Top,
                                      depth + cornerUpperRight.Z, 1);
                v2.Angle = new DX.Vector2(angleCos, angleSin);

                v3.Position = new DX.Vector4(((lowerLeft.X * angleCos) - (lowerLeft.Y * angleSin)) + bounds.Left,
                                     ((lowerLeft.X * angleSin) + (lowerLeft.Y * angleCos)) + bounds.Top,
                                      depth + cornerLowerLeft.Z, 1);
                v3.Angle = new DX.Vector2(angleCos, angleSin);

                v4.Position = new DX.Vector4(((lowerRight.X * angleCos) - (lowerRight.Y * angleSin)) + bounds.Left,
                                     ((lowerRight.X * angleSin) + (lowerRight.Y * angleCos)) + bounds.Top,
                                      depth + cornerLowerRight.Z, 1);
                v4.Angle = new DX.Vector2(angleCos, angleSin);
            }
            else
            {
                v1.Position  = new DX.Vector4(upperLeft.X + bounds.Left,
                                     upperLeft.Y + bounds.Top,
                                     depth + cornerUpperLeft.Z, 1);
                v1.Angle = DX.Vector2.UnitX;

                v2.Position = new DX.Vector4(upperRight.X + bounds.Left,
                                     upperRight.Y + bounds.Top,
                                     depth + cornerUpperRight.Z, 1);
                v2.Angle = DX.Vector2.UnitX;

                v3.Position = new DX.Vector4(lowerLeft.X + bounds.Left,
                                     lowerLeft.Y + bounds.Top,
                                     depth + cornerLowerLeft.Z, 1);
                v3.Angle = DX.Vector2.UnitX;

                v4.Position = new DX.Vector4(lowerRight.X + bounds.Left,
                                     lowerRight.Y + bounds.Top,
                                     depth + cornerLowerRight.Z, 1);
                v4.Angle = DX.Vector2.UnitX;
            }
        }

        /// <summary>
        /// Function to transform the vertices for a renderable.
        /// </summary>
        /// <param name="renderable">The renderable to transform.</param>
        public void Transform(BatchRenderable renderable)
        {
            if (renderable.HasVertexChanges)
            {
                BuildRenderable(ref renderable.Bounds, ref renderable.Anchor, out renderable.Corners);

                // If we've updated the physical dimensions for the renderable, then we need to update the transform as well.
                renderable.HasTransformChanges = true;
                renderable.HasVertexChanges = false;
            }

            if (renderable.HasTransformChanges)
            {
                float rads = 0;
                float sin = 0;
                float cos = 1;

                if (renderable.AngleDegs != 0)
                {
                    rads = renderable.AngleDegs.ToRadians();
                    sin = rads.FastSin();
                    cos = rads.FastCos();
                }

                TransformVertices(renderable.Vertices,
                                  ref renderable.Corners,
                                  ref renderable.Bounds,
                                  ref renderable.Scale,
                                  rads,
                                  sin,
                                  cos,
                                  renderable.Depth,
                                  ref renderable.UpperLeftOffset,
                                  ref renderable.UpperRightOffset,
                                  ref renderable.LowerLeftOffset,
                                  ref renderable.LowerRightOffset);

                renderable.HasTransformChanges = false;                
            }

            if (renderable.HasVertexColorChanges)
            {
                UpdateVertexColors(renderable.Vertices, ref renderable.UpperLeftColor, ref renderable.UpperRightColor, ref renderable.LowerLeftColor, ref renderable.LowerRightColor);
                renderable.HasVertexColorChanges = false;
            }

            if (renderable.HasTextureChanges)
            {
                UpdateTextureCoordinates(renderable.Vertices, ref renderable.TextureRegion, renderable.TextureArrayIndex, renderable.HorizontalFlip, renderable.VerticalFlip,
                                       (!renderable.LowerLeftOffset.IsZero) 
                                    || (!renderable.UpperLeftOffset.IsZero)
                                    || (!renderable.LowerRightOffset.IsZero)
                                    || (!renderable.UpperRightOffset.IsZero));
                renderable.HasTextureChanges = false;
            }            
        }
    }
}
