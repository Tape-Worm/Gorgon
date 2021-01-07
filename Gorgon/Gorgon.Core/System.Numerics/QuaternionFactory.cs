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
// Created: January 7, 2021 1:14:50 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace System.Numerics
{
    /// <summary>
    /// A factory for producing Quaternions.
    /// </summary>
    public static class QuaternionFactory
    {
        /// <summary>
        /// Function to create a Quaternion from yaw, pitch and roll angles.
        /// </summary>
        /// <param name="yaw">The yaw angle, in radians.</param>
        /// <param name="pitch">The pitch angle, in radians.</param>
        /// <param name="roll">The roll angle, in radians.</param>
        /// <param name="result">The new quaternion.</param>
        public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out Quaternion result) => result = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);

        /// <summary>
        /// Function to create a Quaternion from a rotation matrix.
        /// </summary>
        /// <param name="matrix">The rotation matrix.</param>
        /// <param name="result">The new quaternion.</param>
        public static void CreateFromRotationMatrix(in Matrix4x4 matrix, out Quaternion result) => result = Quaternion.CreateFromRotationMatrix(matrix);

        /// <summary>
        /// Function to create a Quaternion from an axis angle.
        /// </summary>
        /// <param name="vector">The vector to rotate around.</param>
        /// <param name="angle">The angle, in radians.</param>
        /// <param name="result">The new quaternion.</param>
        public static void CreateFromAxisAngle(in Vector3 vector, float angle, out Quaternion result) => result = Quaternion.CreateFromAxisAngle(vector, angle);
    }
}
