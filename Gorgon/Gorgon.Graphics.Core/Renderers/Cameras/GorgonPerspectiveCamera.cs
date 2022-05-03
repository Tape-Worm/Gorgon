#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Monday, March 05, 2012 10:34:16 AM
// 
#endregion

using System;
using System.Numerics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Renderers.Cameras
{
    /// <summary>
    /// A camera that performs perspective (3D) projection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This camera is used to bring depth to a 2D scene.  Sprites and other renderables can use their Depth property to determine how far away the object is from the camera.
    /// </para>
    /// <para>
    /// This camera object works in 3 dimensions, so it can be moved further into a scene, and further out.  It also makes use of the near and far clip planes.  Note that the near clip plane should be as
    /// large as is tolerable because it will have the greatest impact on the depth precision.  For more information about depth clip planes, please consult
    /// http://www.sjbaker.org/steve/omniv/love_your_z_buffer.html.
    /// </para>
    /// </remarks>
    public class GorgonPerspectiveCamera
        : GorgonCameraCommon
    {
        #region Variables.
        // The rotation matrix.
        private Matrix4x4 _rotation = Matrix4x4.Identity;
        // The translation matrix.
        private Matrix4x4 _translate = Matrix4x4.Identity;
        // The quaternion representing the rotation of the camera.
        private Quaternion _rotationQuat;
        // The field of view for the camera.
        private float _fov = 75.0f;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the field of view, in degrees, for the camera.
        /// </summary>
        public float Fov
        {
            get => _fov;
            set
            {
                if (_fov == value)
                {
                    return;
                }

                _fov = value;
                Changes |= CameraChange.Projection;
            }
        }

        /// <summary>
        /// Property to set or return the quaternion used for rotation.
        /// </summary>
        public Quaternion Rotation
        {
            get => _rotationQuat;
            set
            {
                if (value == _rotationQuat)
                {
                    return;
                }

                _rotationQuat = value;
                Changes |= CameraChange.View | CameraChange.Rotation;
            }
        }

        /// <summary>Property to return the viewable region for the camera.</summary>
        /// <remarks>
        /// This represents the boundaries of viewable space for the camera using its coordinate system. The upper left of the region corresponds with the upper left of the active render target at minimum
        /// Z depth, and the lower right of the region corresponds with the lower right of the active render target at minimum Z depth.
        /// </remarks>
        public override DX.RectangleF ViewableRegion => new(-1, -1, 2, 2);
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the view matrix.
        /// </summary>
        protected override void UpdateViewMatrix(ref Matrix4x4 viewMatrix)
        {               
            bool hasRotation = (Changes & CameraChange.Rotation) == CameraChange.Rotation;
            bool hasTranslate = (Changes & CameraChange.Position) == CameraChange.Position;
            // There is no scaling for this camera type.
            Changes &= ~CameraChange.Scale;
            
            if ((!hasRotation) && (!hasTranslate))
            {
                return;
            }

            // Rotate it.
            if (hasRotation)
            {
                _rotation = Matrix4x4.CreateFromQuaternion(_rotationQuat);
                _rotation = Matrix4x4.Transpose(_rotation);
                Changes &= ~CameraChange.Rotation;
            }

            // Translate it.
            if (hasTranslate)
            {
                _translate = Matrix4x4.CreateTranslation(-PositionRef.X, -PositionRef.Y, -PositionRef.Z);
                Changes &= ~CameraChange.Position;
            }

            viewMatrix = Matrix4x4.Multiply(_translate, _rotation);
        }

        /// <summary>Function to update the projection matrix.</summary>
        /// <param name="projectionMatrix">The instance of the matrix to update.</param>
        protected override void UpdateProjectionMatrix(ref Matrix4x4 projectionMatrix)
        {
            float yScale = 1.0f / (_fov.ToRadians() * 0.5f).Tan();
            float q = MaximumDepth / (MaximumDepth - MinimumDepth);

            projectionMatrix = default;
            projectionMatrix.M11 = yScale / AspectRatio.X;
            projectionMatrix.M22 = yScale;
            projectionMatrix.M33 = q;
            projectionMatrix.M34 = 1.0f;
            projectionMatrix.M43 = -q * MinimumDepth;
        }

        /// <summary>
        /// Function to rotate the camera using euler angles.
        /// </summary>
        /// <param name="yaw">The yaw of the camera, in degrees.</param>
        /// <param name="pitch">The pitch of the camera, in degrees.</param>
        /// <param name="roll">The roll of the camera, in degrees.</param>
        public void RotateEuler(float yaw, float pitch, float roll)
        {
            _rotationQuat = Quaternion.CreateFromYawPitchRoll(yaw.ToRadians(), pitch.ToRadians(), roll.ToRadians());
            Changes |= CameraChange.View | CameraChange.Rotation;
        }

        /// <summary>
        /// Function to rotate the camera around the specified axes.
        /// </summary>
        /// <param name="axis">The axes to rotate around.</param>
        /// <param name="angle">The angle of rotation, in degrees.</param>
        public void RotateAxis(Vector3 axis, float angle)
        {
            _rotationQuat = Quaternion.Conjugate(Quaternion.CreateFromAxisAngle(axis, angle.ToRadians()));
            Changes |= CameraChange.View | CameraChange.Rotation;
        }

        /// <summary>
        /// Function to set the camera rotation using a rotation matrix.
        /// </summary>
        /// <param name="rotation">The matrix used for rotation.</param>
        public void AssignRotationMatrix(in Matrix4x4 rotation)
        {
            _rotationQuat = Quaternion.Conjugate(Quaternion.CreateFromRotationMatrix(rotation));
            Changes |= CameraChange.View | CameraChange.Rotation;
        }

        /// <summary>
        /// Function to point the camera at a point in space.
        /// </summary>
        /// <param name="target">The point in space to look at.</param>
        /// <param name="upVector">[Optional] The up vector for the camera.</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="upVector"/> parameter is <b>null</b>, then a default positive unit vector is used on the Y axis (0, 1, 0).
        /// </para>
        /// </remarks>
        public void LookAt(Vector3 target, Vector3? upVector = null)
        {
            Vector3 up = upVector ?? Vector3.UnitY;

            var zaxis = Vector3.Normalize(target - PositionRef);
            var xaxis = Vector3.Normalize(Vector3.Cross(up, zaxis));
            var yaxis = Vector3.Cross(zaxis, xaxis);
            var lookMatrix = default(Matrix4x4);
            lookMatrix.M11 = xaxis.X;
            lookMatrix.M12 = yaxis.X;
            lookMatrix.M13 = zaxis.X;
            lookMatrix.M21 = xaxis.Y;
            lookMatrix.M22 = yaxis.Y;
            lookMatrix.M23 = zaxis.Y;
            lookMatrix.M31 = xaxis.Z;
            lookMatrix.M32 = yaxis.Z;
            lookMatrix.M33 = zaxis.Z;
            lookMatrix.M41 = -Vector3.Dot(xaxis, PositionRef);
            lookMatrix.M42 = -Vector3.Dot(yaxis, PositionRef);
            lookMatrix.M43 = -Vector3.Dot(zaxis, PositionRef);
            lookMatrix.M44 = 1f;

            _rotationQuat = Quaternion.Conjugate(Quaternion.CreateFromRotationMatrix(lookMatrix));            
            Changes |= CameraChange.View | CameraChange.Rotation;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonPerspectiveCamera"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface to use with this object.</param>
        /// <param name="viewDimensions">The view dimensions.</param>
        /// <param name="minDepth">[Optional] The minimum depth value.</param>
        /// <param name="maximumDepth">[Optional] The maximum depth value.</param>
        /// <param name="name">[Optional] The name of the camera.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
        public GorgonPerspectiveCamera(GorgonGraphics graphics, DX.Size2F viewDimensions, float minDepth = 0.1f, float maximumDepth = 1000.0f, string name = null)
            : base(graphics, viewDimensions, minDepth, maximumDepth, name)
        {
        }
        #endregion
    }
}