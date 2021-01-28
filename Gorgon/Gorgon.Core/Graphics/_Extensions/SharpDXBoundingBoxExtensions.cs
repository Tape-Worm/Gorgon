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
// Created: January 30, 2021 9:33:08 PM
// 
#endregion

using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Graphics
{
    /// <summary>
    /// Extension methods for the sharp DX bounding box type.
    /// </summary>
    public static class SharpDXBoundingBoxExtensions
    {
        /// <summary>
        /// Function to intersect two Axis Aligned Bounding Boxes.
        /// </summary>
        /// <param name="aabb1">The first axis aligned bounding box.</param>
        /// <param name="aabb2">The second axis aligned bounding box.</param>
        /// <param name="result">The intersection of both bounding boxes.</param>
        public static void Intersect(ref this DX.BoundingBox aabb1, ref DX.BoundingBox aabb2, out DX.BoundingBox result)
        {
            float left = aabb2.Minimum.X.Max(aabb1.Minimum.X);
            float top = aabb2.Minimum.Y.Max(aabb1.Minimum.Y);
            float front = aabb2.Minimum.Z.Max(aabb1.Minimum.Z);

            float right = aabb2.Maximum.X.Min(aabb1.Maximum.X);
            float bottom = aabb2.Maximum.Y.Min(aabb1.Maximum.Y);
            float back = aabb2.Maximum.Z.Min(aabb1.Maximum.Z);

            if ((right < left) || (bottom < top) || (back < front))
            {
                result = default;
                return;
            }

            result = new DX.BoundingBox(new DX.Vector3(left, top, front), 
                                        new DX.Vector3(right, bottom, back));
        }

        /// <summary>
        /// Function to transform an AABB by a world matrix.
        /// </summary>
        /// <param name="aabb">The axis aligned bounding box to transform.</param>
        /// <param name="worldMatrix">The world matrix to multiply by.</param>
        /// <param name="result">The new transformed axis aligned bounding box.</param>
        public static void Transform(ref this DX.BoundingBox aabb, ref DX.Matrix worldMatrix, out DX.BoundingBox result)
        {
            DX.Matrix rot;
            rot.M11 = worldMatrix.M11;
            rot.M12 = worldMatrix.M12;
            rot.M13 = worldMatrix.M13;
            rot.M21 = worldMatrix.M21;
            rot.M22 = worldMatrix.M22;
            rot.M23 = worldMatrix.M23;
            rot.M31 = worldMatrix.M31;
            rot.M32 = worldMatrix.M32;
            rot.M33 = worldMatrix.M33;

            DX.Vector3 max = worldMatrix.TranslationVector;
            DX.Vector3 min = worldMatrix.TranslationVector;

            float a1 = rot.M11 * aabb.Minimum.X;
            float b1 = rot.M11 * aabb.Maximum.X;
            float a2 = rot.M12 * aabb.Minimum.Y;
            float b2 = rot.M12 * aabb.Maximum.Y;
            float a3 = rot.M13 * aabb.Minimum.Z;
            float b3 = rot.M13 * aabb.Maximum.Z;
            float a4 = rot.M21 * aabb.Minimum.X;
            float b4 = rot.M21 * aabb.Maximum.X;
            float a5 = rot.M22 * aabb.Minimum.Y;
            float b5 = rot.M22 * aabb.Maximum.Y;
            float a6 = rot.M23 * aabb.Minimum.Z;
            float b6 = rot.M23 * aabb.Maximum.Z;
            float a7 = rot.M31 * aabb.Minimum.X;
            float b7 = rot.M31 * aabb.Maximum.X;
            float a8 = rot.M32 * aabb.Minimum.Y;
            float b8 = rot.M32 * aabb.Maximum.Y;
            float a9 = rot.M33 * aabb.Minimum.Z;
            float b9 = rot.M33 * aabb.Maximum.Z;

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

            result = new DX.BoundingBox(min, max);
        }
    }
}
