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
        // The scaling matrix.
        private Matrix4x4 _scale = Matrix4x4.Identity;
        // The translation matrix.
        private Matrix4x4 _translate = Matrix4x4.Identity;
        // The position of the camera.
        private Vector3 _position;
        // Scale.
        private Vector3 _zoom = new Vector3(1.0f);
        // Angle of rotation on the X axis.
        private float _angleX;
        // Angle of rotation on the Y axis.
        private float _angleY;
        // Angle of rotation on the Z axis.
        private float _angleZ;
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
        /// Property to set or return the camera position.
        /// </summary>
        public Vector3 Position
        {
            get => _position;
            set
            {
                if (value == _position)
                {
                    return;
                }

                _position = value;
                Changes |= CameraChange.View | CameraChange.Position;
            }
        }

        /// <summary>
        /// Property to set or return the pitch in degrees.
        /// </summary>
        public float PitchAngle
        {
            get => _angleX;
            set
            {
                if (_angleX == value)
                {
                    return;
                }

                _angleX = value;
                Changes |= CameraChange.View | CameraChange.Rotation;
            }
        }

        /// <summary>
        /// Property to set or return the yaw in degrees.
        /// </summary>
        public float YawAngle
        {
            get => _angleY;
            set
            {
                if (_angleY == value)
                {
                    return;
                }

                _angleY = value;
                Changes |= CameraChange.View | CameraChange.Rotation;
            }
        }

        /// <summary>
        /// Property to set or return the roll in degrees.
        /// </summary>
        public float RollAngle
        {
            get => _angleZ;
            set
            {
                if (_angleZ == value)
                {
                    return;
                }

                _angleZ = value;
                Changes |= CameraChange.View | CameraChange.Rotation;
            }
        }

        /// <summary>
        /// Property to set or return the zoom for the camera.
        /// </summary>
        public Vector3 Zoom
        {
            get => _zoom;
            set
            {
                if (value == _zoom)
                {
                    return;
                }

                _zoom = value;
                Changes |= CameraChange.View | CameraChange.Scale;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the view matrix.
        /// </summary>
        protected override void UpdateViewMatrix(ref Matrix4x4 viewMatrix)
        {            
            bool hasScale = (Changes & CameraChange.Scale) == CameraChange.Scale;
            bool hasRotation = (Changes & CameraChange.Rotation) == CameraChange.Rotation;
            bool hasTranslate = (Changes & CameraChange.Position) == CameraChange.Position;

            if ((!hasScale) && (!hasRotation) && (!hasTranslate))
            {
                return;
            }

            // Scale it.
            if (hasScale)                
            {
                _scale.M11 = _zoom.X;
                _scale.M22 = _zoom.Y;
                _scale.M33 = _zoom.Z;
                Changes &= ~CameraChange.Scale;
            }

            // Rotate it.
            if (hasRotation)
            {
                QuaternionFactory.CreateFromYawPitchRoll(_angleY.ToRadians(), _angleX.ToRadians(), _angleZ.ToRadians(), out Quaternion quat);
                MatrixFactory.CreateFromQuaternion(in quat, out _rotation);
                
                // We need to invert the matrix in order to apply the correct transformation to the world data.
                _rotation.Transpose(out _rotation);
                Changes &= ~CameraChange.Rotation;
            }

            // Translate it.
            if (hasTranslate)
            {
                MatrixFactory.CreateTranslation(new Vector3(-_position.X, -_position.Y, _position.Z), out _translate);
                Changes &= ~CameraChange.Position;
            }

            // Combine.
            Matrix4x4 temp;

            if (hasRotation)
            {
                _rotation.Multiply(in _scale, out temp);
            }
            else
            {
                temp = _scale;
            }

            if (!hasTranslate)
            {
                viewMatrix = temp;
                return;
            }

            _translate.Multiply(in temp, out viewMatrix);
        }

        /// <summary>Function to update the projection matrix.</summary>
        /// <param name="projectionMatrix">The instance of the matrix to update.</param>
        protected override void UpdateProjectionMatrix(ref Matrix4x4 projectionMatrix) 
            => MatrixFactory.CreatePerspectiveFovLH(_fov.ToRadians(), (float)TargetWidth / TargetHeight, MinimumDepth, MaximumDepth, out projectionMatrix);
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
        public GorgonPerspectiveCamera(GorgonGraphics graphics, DX.Size2F viewDimensions, float minDepth = 0.0f, float maximumDepth = 1.0f, string name = null)
            : base(graphics, viewDimensions, minDepth, maximumDepth, name)
        {
        }
        #endregion
    }
}