#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: December 4, 2020 3:03:13 PM
// 
#endregion

using System.Runtime.CompilerServices;
using Gorgon.Math;

namespace System.Numerics
{
    /// <summary>
    /// A factory for creating matrix objects by reference.
    /// </summary>
    public static class MatrixFactory
    {
        /// <summary>
        /// Function to create a billboard that rotates around a specific position.
        /// </summary>
        /// <param name="objectPosition">The position to rotate around.</param>
        /// <param name="cameraPosition">The position of the camera in world space.</param>
        /// <param name="cameraUpVector">The up vector for the camera.</param>
        /// <param name="cameraForwardVector">The forward vector for the camera.</param>
        /// <param name="result">The billboard matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateBillboard(in Vector3 objectPosition, in Vector3 cameraPosition, in Vector3 cameraUpVector, in Vector3 cameraForwardVector, out Matrix4x4 result)
                => result = Matrix4x4.CreateBillboard(objectPosition, cameraPosition, cameraUpVector, cameraForwardVector);

        /// <summary>
        /// Function to create a billboard that rotates around a specific axis.
        /// </summary>
        /// <param name="objectPosition">The position to rotate around.</param>
        /// <param name="cameraPosition">The position of the camera in world space.</param>
        /// <param name="rotateAxis">The axis to rotate around.</param>
        /// <param name="cameraForwardVector">The forward vector for the camera.</param>
        /// <param name="objectForwardVector">The forward vector for the object.</param>
        /// <param name="result">The billboard matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateConstrainedBillboard(in Vector3 objectPosition, in Vector3 cameraPosition, in Vector3 rotateAxis, in Vector3 cameraForwardVector, in Vector3 objectForwardVector, out Matrix4x4 result)
            => result = Matrix4x4.CreateConstrainedBillboard(objectPosition, cameraPosition, rotateAxis, cameraForwardVector, objectForwardVector);

        /// <summary>
        /// Function to create a rotation matrix based on an axis. 
        /// </summary>
        /// <param name="axis">The axis to rotate around.</param>
        /// <param name="angle">The angle, in radians.</param>
        /// <param name="result">The rotation matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateFromAxisAngle(in Vector3 axis, float angle, out Matrix4x4 result) => result = Matrix4x4.CreateFromAxisAngle(axis, angle);

        /// <summary>
        /// Function to create a rotation matrix from a quaternion.
        /// </summary>
        /// <param name="quat">The quaternion used to create the matrix.</param>
        /// <param name="result">The resulting rotation matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateFromQuaternion(in Quaternion quat, out Matrix4x4 result) => result = Matrix4x4.CreateFromQuaternion(quat);

        /// <summary>
        /// Function to create a rotation matrix from a quaternion.
        /// </summary>
        /// <param name="yaw">The yaw (y-axis), in radians.</param>
        /// <param name="pitch">The pitch (x-axis) in radians.</param>
        /// <param name="roll">The roll (z-axis) in radians.</param>
        /// <param name="result">The resulting rotation matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateFromQuaternion(float yaw, float pitch, float roll, out Matrix4x4 result) => result = Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll);

        /// <summary>
        /// Function to create a right handed look at matrix.
        /// </summary>
        /// <param name="cameraPosition">The position of the camera in the world.</param>
        /// <param name="cameraTarget">The look at target for the camera.</param>
        /// <param name="cameraUp">The up vector for the camera.</param>
        /// <param name="result">The look at matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateLookAtRH(in Vector3 cameraPosition, in Vector3 cameraTarget, in Vector3 cameraUp, out Matrix4x4 result) => result = Matrix4x4.CreateLookAt(cameraPosition, cameraTarget, cameraUp);

        /// <summary>
        /// Function to create a left handed look at matrix.
        /// </summary>
        /// <param name="cameraPosition">The position of the camera in the world.</param>
        /// <param name="cameraTarget">The look at target for the camera.</param>
        /// <param name="cameraUp">The up vector for the camera.</param>
        /// <param name="result">The look at matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateLookAtLH(in Vector3 cameraPosition, in Vector3 cameraTarget, in Vector3 cameraUp, out Matrix4x4 result)
        {
            Vector3 xaxis, yaxis, zaxis;

            zaxis = Vector3.Subtract(cameraTarget, cameraPosition);
            zaxis = Vector3.Normalize(zaxis);

            xaxis = Vector3.Cross(cameraUp, zaxis);
            xaxis = Vector3.Normalize(xaxis);

            yaxis = Vector3.Cross(zaxis, xaxis);

            result = Matrix4x4.Identity;
            result.M11 = xaxis.X;
            result.M21 = xaxis.Y;
            result.M31 = xaxis.Z;
            result.M12 = yaxis.X;
            result.M22 = yaxis.Y;
            result.M32 = yaxis.Z;
            result.M13 = zaxis.X;
            result.M23 = zaxis.Y;
            result.M33 = zaxis.Z;

            result.M41 = Vector3.Dot(xaxis, cameraPosition);
            result.M42 = Vector3.Dot(yaxis, cameraPosition);
            result.M43 = Vector3.Dot(zaxis, cameraPosition);

            result.M41 = -result.M41;
            result.M42 = -result.M42;
            result.M43 = -result.M43;
        }


        /// <summary>
        /// Function to create a right handed orthographic projection matrix.
        /// </summary>
        /// <param name="width">The width of the view.</param>
        /// <param name="height">The height of the view.</param>
        /// <param name="zNear">The near clipping plane on the z axis.</param>
        /// <param name="zFar">The far clipping plane on the z axis.</param>
        /// <param name="result">The orthographic projection matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateOrthographicRH(float width, float height, float zNear, float zFar, out Matrix4x4 result) => result = Matrix4x4.CreateOrthographic(width, height, zNear, zFar);

        /// <summary>
        /// Function to create a left handed orthographic projection matrix.
        /// </summary>
        /// <param name="width">The width of the view.</param>
        /// <param name="height">The height of the view.</param>
        /// <param name="zNear">The near clipping plane on the z axis.</param>
        /// <param name="zFar">The far clipping plane on the z axis.</param>
        /// <param name="result">The orthographic projection matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateOrthographicLH(float width, float height, float zNear, float zFar, out Matrix4x4 result)
        {
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            CreateOrthographicOffCenterLH(-halfWidth, halfWidth, -halfHeight, halfHeight, zNear, zFar, out result);
        }
    
        /// <summary>
        /// Function to create a right handed orthographic projection matrix that is centered around an arbitrary point.
        /// </summary>
        /// <param name="left">The left plane for the projection.</param>
        /// <param name="right">The right plane for the projection.</param>
        /// <param name="bottom">The bottom plane for the projection.</param>
        /// <param name="top">The top plane for the projection.</param>
        /// <param name="zNear">The near clipping plane on the z axis.</param>
        /// <param name="zFar">The far clipping plane on the z axis.</param>
        /// <param name="result">The orthographic projection matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateOrthographicOffCenterRH(float left, float right, float bottom, float top, float zNear, float zFar, out Matrix4x4 result) 
            => result = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, zNear, zFar);

        /// <summary>
        /// Function to create a left handed orthographic projection matrix that is centered around an arbitrary point.
        /// </summary>
        /// <param name="left">The left plane for the projection.</param>
        /// <param name="right">The right plane for the projection.</param>
        /// <param name="bottom">The bottom plane for the projection.</param>
        /// <param name="top">The top plane for the projection.</param>
        /// <param name="zNear">The near clipping plane on the z axis.</param>
        /// <param name="zFar">The far clipping plane on the z axis.</param>
        /// <param name="result">The orthographic projection matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateOrthographicOffCenterLH(float left, float right, float bottom, float top, float zNear, float zFar, out Matrix4x4 result)
        {
            float zRange = 1.0f / (zFar - zNear);

            result = Matrix4x4.Identity;
            result.M11 = 2.0f / (right - left);
            result.M22 = 2.0f / (top - bottom);
            result.M33 = zRange;
            result.M41 = (left + right) / (left - right);
            result.M42 = (top + bottom) / (bottom - top);
            result.M43 = -zNear * zRange;
        }

        /// <summary>
        /// Function to create a right handed perspective projection matrix.
        /// </summary>
        /// <param name="width">The width of the view.</param>
        /// <param name="height">The height of the view.</param>
        /// <param name="zNear">The near clipping plane on the z axis.</param>
        /// <param name="zFar">The far clipping plane on the z axis.</param>
        /// <param name="result">The perspective projection matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreatePerspectiveRH(float width, float height, float zNear, float zFar, out Matrix4x4 result) => result = Matrix4x4.CreatePerspective(width, height, zNear, zFar);

        /// <summary>
        /// Function to create a left handed perspective projection matrix.
        /// </summary>
        /// <param name="width">The width of the view.</param>
        /// <param name="height">The height of the view.</param>
        /// <param name="zNear">The near clipping plane on the z axis.</param>
        /// <param name="zFar">The far clipping plane on the z axis.</param>
        /// <param name="result">The perspective projection matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreatePerspectiveLH(float width, float height, float zNear, float zFar, out Matrix4x4 result)
        {
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            CreatePerspectiveOffCenterLH(-halfWidth, halfWidth, -halfHeight, halfHeight, zNear, zFar, out result);
        }
        /// <summary>
        /// Function to create a left handed perspective projection matrix using a field of view.
        /// </summary>
        /// <param name="fov">The field of view, in radians.</param>
        /// <param name="aspectRatio">The aspect ratio for the view.</param>
        /// <param name="zNear">The near clipping plane on the z axis.</param>
        /// <param name="zFar">The far clipping plane on the z axis.</param>
        /// <param name="result">The perspective projection matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreatePerspectiveFovLH(float fov, float aspectRatio, float zNear, float zFar, out Matrix4x4 result)
        {
            float num = 1.0f / (fov * 0.5f).Tan();
            float num2 = zFar / (zFar - zNear);
            result = default;
            result.M11 = num / aspectRatio;
            result.M22 = num;
            result.M33 = num2;
            result.M34 = 1f;
            result.M43 = (0f - num2) * zNear;
        }

        /// <summary>
        /// Function to create a right handed perspective projection matrix using a field of view.
        /// </summary>
        /// <param name="fov">The field of view, in radians.</param>
        /// <param name="aspectRatio">The aspect ratio for the view.</param>
        /// <param name="zNear">The near clipping plane on the z axis.</param>
        /// <param name="zFar">The far clipping plane on the z axis.</param>
        /// <param name="result">The perspective projection matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreatePerspectiveFovRH(float fov, float aspectRatio, float zNear, float zFar, out Matrix4x4 result) => result = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, zNear, zFar);

        /// <summary>
        /// Function to create a right handed perspective projection matrix that is centered around an arbitrary point.
        /// </summary>
        /// <param name="left">The left plane for the projection.</param>
        /// <param name="right">The right plane for the projection.</param>
        /// <param name="bottom">The bottom plane for the projection.</param>
        /// <param name="top">The top plane for the projection.</param>
        /// <param name="zNear">The near clipping plane on the z axis.</param>
        /// <param name="zFar">The far clipping plane on the z axis.</param>
        /// <param name="result">The perspective projection matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreatePerspectiveOffCenterRH(float left, float right, float bottom, float top, float zNear, float zFar, out Matrix4x4 result)
            => result = Matrix4x4.CreatePerspectiveOffCenter(left, right, bottom, top, zNear, zFar);

        /// <summary>
        /// Function to create a left handed perspective projection matrix that is centered around an arbitrary point.
        /// </summary>
        /// <param name="left">The left plane for the projection.</param>
        /// <param name="right">The right plane for the projection.</param>
        /// <param name="bottom">The bottom plane for the projection.</param>
        /// <param name="top">The top plane for the projection.</param>
        /// <param name="zNear">The near clipping plane on the z axis.</param>
        /// <param name="zFar">The far clipping plane on the z axis.</param>
        /// <param name="result">The perspective projection matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreatePerspectiveOffCenterLH(float left, float right, float bottom, float top, float zNear, float zFar, out Matrix4x4 result)
        {
            float zRange = zFar / (zFar - zNear);

            result = default;
            result.M11 = 2.0f * zNear / (right - left);
            result.M22 = 2.0f * zNear / (top - bottom);
            result.M31 = (left + right) / (left - right);
            result.M32 = (top + bottom) / (bottom - top);
            result.M33 = zRange;
            result.M34 = 1.0f;
            result.M43 = -zNear * zRange;
        }

        /// <summary>
        /// Function to create a reflection matrix.
        /// </summary>
        /// <param name="plane">The plane to reflect.</param>
        /// <param name="result">The reflection matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateReflection(in Plane plane, out Matrix4x4 result) => result = Matrix4x4.CreateReflection(plane);

        /// <summary>
        /// Function to create a rotation matrix around the X axis.
        /// </summary>
        /// <param name="angle">The angle of rotation, in radians.</param>
        /// <param name="result">The rotation matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateRotationX(float angle, out Matrix4x4 result) => result = Matrix4x4.CreateRotationX(angle);

        /// <summary>
        /// Function to create a rotation matrix around the Y axis.
        /// </summary>
        /// <param name="angle">The angle of rotation, in radians.</param>
        /// <param name="result">The rotation matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateRotationY(float angle, out Matrix4x4 result) => result = Matrix4x4.CreateRotationY(angle);

        /// <summary>
        /// Function to create a rotation matrix around the Z axis.
        /// </summary>
        /// <param name="angle">The angle of rotation, in radians.</param>
        /// <param name="result">The rotation matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateRotationZ(float angle, out Matrix4x4 result) => result = Matrix4x4.CreateRotationZ(angle);

        /// <summary>
        /// Function to create a scaling matrix.
        /// </summary>
        /// <param name="scale">The scaling values for the X, Y and Z axes.</param>
        /// <param name="result">The scaling matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScale(in Vector3 scale, out Matrix4x4 result) => result = Matrix4x4.CreateScale(scale);

        /// <summary>
        /// Function to create a matrix for projecting shadows.
        /// </summary>
        /// <param name="lightDirection">The direction of the light.</param>
        /// <param name="plane">The plane used to flatten the shadow.</param>
        /// <param name="result">The shadow matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScale(in Vector3 lightDirection, in Plane plane, out Matrix4x4 result) => result = Matrix4x4.CreateShadow(lightDirection, plane);

        /// <summary>
        /// Function to create a rotation matrix around the X axis.
        /// </summary>
        /// <param name="translation">The position to translate to.</param>
        /// <param name="result">The translation matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateTranslation(in Vector3 translation, out Matrix4x4 result) => result = Matrix4x4.CreateTranslation(translation);
    }
}
