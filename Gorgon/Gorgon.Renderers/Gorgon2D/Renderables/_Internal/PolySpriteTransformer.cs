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

using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// Provides functionality for transforming renderable vertices.
    /// </summary>
    internal class PolySpriteTransformer
    {
        /// <summary>
        /// Function to transform each vertex of the renderable to change its location, size and rotation.
        /// </summary>
        /// <param name="anchor">The anchor for the sprite.</param>
        /// <param name="bounds">The boundaries for the renderable.</param>
        /// <param name="scale">The scale of the renderable.</param>
        /// <param name="hasRotation"><b>true</b> if the renderable is rotated, <b>false</b> if not.</param>
        /// <param name="angleSin">The cached sine of the angle.</param>
        /// <param name="angleCos">The cached cosine of the angle.</param>
        /// <param name="depth">The depth value for the renderable.</param>
        /// <param name="worldMatrix">The world matrix.</param>
        private static void TransformVertices(ref DX.Vector2 anchor,
                                              ref DX.RectangleF bounds,
                                              ref DX.Vector2 scale,
                                              bool hasRotation,
                                              float angleSin,
                                              float angleCos,
                                              float depth,
                                              out DX.Matrix worldMatrix)
        {
            DX.Matrix anchorMat = DX.Matrix.Identity;
            DX.Matrix scaleMat = DX.Matrix.Identity;
            DX.Matrix rotMat = DX.Matrix.Identity;
            DX.Matrix transMat = DX.Matrix.Identity;

            anchorMat.M41 = -anchor.X * bounds.Width;
            anchorMat.M42 = -anchor.Y * bounds.Height;

            scaleMat.M11 = scale.X;
            scaleMat.M22 = scale.Y;

            if (hasRotation)
            {
                rotMat.M11 = angleCos;
                rotMat.M12 = angleSin;
                rotMat.M21 = -angleSin;
                rotMat.M22 = angleCos;
            }

            transMat.M41 = bounds.X;
            transMat.M42 = bounds.Y;
            transMat.M43 = depth;

            DX.Matrix.Multiply(ref anchorMat, ref scaleMat, out DX.Matrix tempMat);
            DX.Matrix.Multiply(ref tempMat, ref rotMat, out DX.Matrix tempMat2);
            DX.Matrix.Multiply(ref tempMat2, ref transMat, out worldMatrix);
        }

        /// <summary>
        /// Function to transform the vertices for a renderable.
        /// </summary>
        /// <param name="renderable">The renderable to transform.</param>
        public void Transform(PolySpriteRenderable renderable)
        {
            if (renderable.HasVertexChanges)
            {
                renderable.HasTransformChanges = true;
            }

            renderable.HasVertexColorChanges = false;
            renderable.HasTextureChanges = false;
            renderable.HasVertexChanges = false;

            if (!renderable.HasTransformChanges)
            {
                return;
            }

            TransformVertices(ref renderable.Anchor,
                              ref renderable.Bounds,
                              ref renderable.Scale,
                              !renderable.AngleRads.EqualsEpsilon(0.0f),
                              renderable.AngleSin,
                              renderable.AngleCos,
                              renderable.Depth,
                              out renderable.WorldMatrix);

            renderable.HasTransformChanges = false;

        }
    }
}
