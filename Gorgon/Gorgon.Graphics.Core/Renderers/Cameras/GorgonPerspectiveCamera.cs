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
        private DX.Matrix _rotation = DX.Matrix.Identity;
        // The translation matrix.
        private DX.Matrix _translate = DX.Matrix.Identity;
        // The quaternion representing the rotation of the camera.
        private DX.Quaternion _rotationQuat;
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
        public DX.Quaternion Rotation
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
        public override DX.RectangleF ViewableRegion => new DX.RectangleF(-1, -1, 2, 2);
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the view matrix.
        /// </summary>
        protected override void UpdateViewMatrix(ref DX.Matrix viewMatrix)
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
                DX.Matrix.RotationQuaternion(ref _rotationQuat, out _rotation);
                DX.Matrix.Transpose(ref _rotation, out _rotation);
                Changes &= ~CameraChange.Rotation;
            }

            // Translate it.
            if (hasTranslate)
            {
                DX.Matrix.Translation(-PositionRef.X, -PositionRef.Y, -PositionRef.Z, out _translate);                
                Changes &= ~CameraChange.Position;
            }

            DX.Matrix.Multiply(ref _translate, ref _rotation, out viewMatrix);
        }

        /// <summary>Function to update the projection matrix.</summary>
        /// <param name="projectionMatrix">The instance of the matrix to update.</param>
        protected override void UpdateProjectionMatrix(ref DX.Matrix projectionMatrix) 
            => DX.Matrix.PerspectiveFovLH(_fov.ToRadians(), (float)TargetWidth / TargetHeight, MinimumDepth, MaximumDepth, out projectionMatrix);

        /// <summary>
        /// Function to rotate the camera using euler angles.
        /// </summary>
        /// <param name="yaw">The yaw of the camera, in degrees.</param>
        /// <param name="pitch">The pitch of the camera, in degrees.</param>
        /// <param name="roll">The roll of the camera, in degrees.</param>
        public void RotateEuler(float yaw, float pitch, float roll)
        {
            DX.Quaternion.RotationYawPitchRoll(yaw.ToRadians(), pitch.ToRadians(), roll.ToRadians(), out _rotationQuat);
            Changes |= CameraChange.View | CameraChange.Rotation;
        }

        /// <summary>
        /// Function to rotate the camera around the specified axes.
        /// </summary>
        /// <param name="axis">The axes to rotate around.</param>
        /// <param name="angle">The angle of rotation, in degrees.</param>
        public void RotateAxis(ref DX.Vector3 axis, float angle)
        {            
            DX.Quaternion.RotationAxis(ref axis, angle.ToRadians(), out _rotationQuat);
            DX.Quaternion.Conjugate(ref _rotationQuat, out _rotationQuat);
            Changes |= CameraChange.View | CameraChange.Rotation;
        }

        /// <summary>
        /// Function to set the camera rotation using a rotation matrix.
        /// </summary>
        /// <param name="rotation">The matrix used for rotation.</param>
        public void AssignRotationMatrix(ref DX.Matrix rotation)
        {
            DX.Quaternion.RotationMatrix(ref rotation, out _rotationQuat);
            DX.Quaternion.Conjugate(ref _rotationQuat, out _rotationQuat);
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
        public void LookAt(ref DX.Vector3 target, DX.Vector3? upVector = null)
        {
            DX.Vector3 up = upVector ?? DX.Vector3.UnitY;

            DX.Matrix.LookAtLH(ref PositionRef, ref target, ref up, out DX.Matrix tempMatrix);
            DX.Quaternion.RotationMatrix(ref tempMatrix, out _rotationQuat);
            _rotationQuat = DX.Quaternion.Conjugate(_rotationQuat);
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