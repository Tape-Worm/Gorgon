#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Thursday, August 7, 2014 10:29:17 PM
// 
#endregion

using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Examples
{
    /// <summary>
    /// The world/view/projection matrix.
    /// </summary>
    internal class Camera
    {
        #region Variables.
        // The eye position.
        private DX.Vector3 _eye;
        // The look at position.
        private DX.Vector3 _lookAt;
        // The up vector for the camera.
        private DX.Vector3 _up;
        // The right vector for the camera.
        private DX.Vector3 _right;
        // The view matrix for this camera.
        private DX.Matrix _viewMatrix = DX.Matrix.Identity;
        // The projection matrix for this camera.
        private DX.Matrix _projectionMatrix = DX.Matrix.Identity;
        // The view * projection matrix for this camera.
        private DX.Matrix _viewProjectionMatrix = DX.Matrix.Identity;
        // The field of view for the camera.
        private float _fov;
        // The view width, in pixels.
        private int _viewWidth;
        // The view height, in pixels.
        private int _viewHeight;
        // The near Z plane.
        private float _nearZ = 0.25f;
        // The far Z plane.
        private float _farZ = 1000.0f;
        // Flag to indicate that the projection matrix is dirty.
        private bool _isProjectionDirty;
        // Flag to indicate that the view * projection matrix is dirty.
        private bool _isCombinedDirty;
        // Flag to indicate that the view matrix is dirty.
        private bool _isDirty;
        // The angle of rotation around each axis.
        private DX.Vector3 _rotation;
        // The velocity used to move the camera to another position.
        private DX.Vector3 _velocity;
        // A target position to look at.
        private DX.Vector3? _target;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the right side vector for the camera.
        /// </summary>
        public ref DX.Vector3 Right => ref _right;

        /// <summary>
        /// Property to set or return whether the projection for the camera has been updated or not.
        /// </summary>
        public bool IsProjectionDirty
        {
            get => _isProjectionDirty;
            private set
            {
                _isProjectionDirty = value;

                if (_isProjectionDirty)
                {
                    _isCombinedDirty = true;
                }
            }
        }

        /// <summary>
        /// Property to set or return the width of the view, in pixels.
        /// </summary>
        public int ViewWidth
        {
            get => _viewHeight;
            set
            {
                if (_viewWidth == value)
                {
                    return;
                }

                _viewWidth = value;
                IsProjectionDirty = true;
            }
        }

        /// <summary>
        /// Property to set or return the height of the view, in pixels.
        /// </summary>
        public int ViewHeight
        {
            get => _viewHeight;
            set
            {
                if (_viewHeight == value)
                {
                    return;
                }

                _viewHeight = value;
                IsProjectionDirty = true;
            }
        }

        /// <summary>
        /// Property to set or return the near Z plane value.
        /// </summary>
        public float NearZ
        {
            get => _nearZ;
            set
            {
                if (_nearZ.EqualsEpsilon(value))
                {
                    return;
                }

                _nearZ = value;
                IsProjectionDirty = true;
            }
        }

        /// <summary>
        /// Property to set or return the far Z plane value.
        /// </summary>
        public float FarZ
        {
            get => _farZ;
            set
            {
                if (_farZ.EqualsEpsilon(value))
                {
                    return;
                }

                _farZ = value;
                IsProjectionDirty = true;
            }
        }

        /// <summary>
        /// Property to set or return the vertical field of view for the camera.
        /// </summary>
        public float Fov
        {
            get => _fov;
            set
            {
                if (_fov.EqualsEpsilon(value))
                {
                    return;
                }

                _fov = value;
                IsProjectionDirty = true;
            }
        }

        /// <summary>
        /// Property to set or return whether the camera view has been changed.
        /// </summary>
        public bool IsViewDirty
        {
            get => _isDirty;
            private set
            {
                _isDirty = value;

                if (_isDirty)
                {
                    _isCombinedDirty = true;
                }
            }
        }

        /// <summary>
        /// Property to set or return the eye position of the camera.
        /// </summary>
        public DX.Vector3 EyePosition
        {
            get => _eye;
            set
            {
                if (_eye.Equals(ref value))
                {
                    return;
                }

                _eye = value;
                IsViewDirty = true;
            }
        }

        /// <summary>
        /// Property to set or return the look at vector for the camera.
        /// </summary>
        public DX.Vector3 LookAt => _lookAt;

        /// <summary>
        /// Property to set or return the up vector for the camera.
        /// </summary>
        public DX.Vector3 Up => _up;

        /// <summary>
        /// Property to set or return the angle of rotation around each axis, in degrees.
        /// </summary>
        public DX.Vector3 Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation.Equals(ref value))
                {
                    return;
                }

                _rotation = value;
                IsViewDirty = true;
            }
        }
        #endregion 

        #region Methods.
        public void Move(ref DX.Vector3 velocity)
        {
            _velocity = velocity;
            IsViewDirty = true;
        }

        public void Target(DX.Vector3? target)
        {
            _target = target;
            IsViewDirty = true;
        }

        /// <summary>
        /// Function to retrieve the view multiplied by the projection matrix.
        /// </summary>
        /// <returns>A reference to the combined matrix values.</returns>
        public ref DX.Matrix GetViewProjectionMatrix()
        {
            if (!_isCombinedDirty)
            {
                return ref _viewProjectionMatrix;
            }

            DX.Matrix.Multiply(ref GetViewMatrix(), ref GetProjectionMatrix(), out _viewProjectionMatrix);

            _isCombinedDirty = false;

            return ref _viewProjectionMatrix;
        }

        /// <summary>
        /// Function to retrieve the view matrix.
        /// </summary>
        /// <returns>The reference to the view matrix</returns>
        public ref DX.Matrix GetViewMatrix()
        {
            if (!IsViewDirty)
            {
                return ref _viewMatrix;
            }

            DX.Vector3 forward = _target ?? DX.Vector3.ForwardLH;
            DX.Vector3 right = DX.Vector3.Right;
            DX.Vector3 up = DX.Vector3.Up;

            if (_target == null)
            {
                DX.Matrix.RotationYawPitchRoll(_rotation.Y.ToRadians(), _rotation.X.ToRadians(), _rotation.Z.ToRadians(), out DX.Matrix rotation);
                DX.Vector3.TransformCoordinate(ref forward, ref rotation, out forward);
                DX.Vector3.TransformCoordinate(ref right, ref rotation, out right);
                DX.Vector3.Cross(ref forward, ref right, out up);
            }

            _lookAt = forward;
            _up = up;

            // Update our eye position by our velocity.
            if (_velocity != DX.Vector3.Zero)
            {
                DX.Vector3.Multiply(ref right, _velocity.X, out right);
                DX.Vector3.Add(ref _eye, ref right, out _eye);
                DX.Vector3.Multiply(ref forward, _velocity.Z, out forward);
                DX.Vector3.Add(ref _eye, ref forward, out _eye);
                DX.Vector3.Multiply(ref up, _velocity.Y, out up);
                DX.Vector3.Add(ref _eye, ref up, out _eye);
            }

            // Offset the look at position by our current eye position.
            if (_target == null)
            {
                DX.Vector3.Add(ref _eye, ref _lookAt, out _lookAt);
            }

            DX.Matrix.LookAtLH(ref _eye, ref _lookAt, ref _up, out _viewMatrix);

            IsViewDirty = false;

            return ref _viewMatrix;
        }

        /// <summary>
        /// Function to retrieve the projection matrix for the camera.
        /// </summary>
        /// <returns>A reference to the projection matrix for the camera.</returns>
        public ref DX.Matrix GetProjectionMatrix()
        {
            if (!IsProjectionDirty)
            {
                return ref _projectionMatrix;
            }

            float aspect = (float)_viewWidth / _viewHeight;
            DX.Matrix.PerspectiveFovLH(_fov.ToRadians(), aspect, _nearZ, _farZ, out _projectionMatrix);

            IsProjectionDirty = false;

            return ref _projectionMatrix;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class.
        /// </summary>
        public Camera()
        {
            _lookAt = new DX.Vector3(0, 0, -1.0f);
            _up = new DX.Vector3(0, 1, 0);
            _isDirty = true;
            _isProjectionDirty = true;
            _isCombinedDirty = true;
        }
        #endregion
    }
}
