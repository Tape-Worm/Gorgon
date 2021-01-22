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
    /// A camera that performs orthographic (2D) projection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This camera performs 2D projection of sprites and other renderables on to a target. By default, the camera will use absolute screen space coordinates e.g. 160x120 will be the center of a 
    /// 320x240 render target.  The user may define their own coordinate system to apply to the projection.
    /// </para>
    /// </remarks>
    public class GorgonOrthoCamera
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
        private Vector2 _zoom = new Vector2(1.0f);
        // Target position.
        private Vector2 _anchor = Vector2.Zero;
        // Angle of rotation on the Z axis.
        private float _angleZ;
        #endregion

        #region Properties.
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
        /// Property to set or return the rotation on the Z axis, in degrees.
        /// </summary>
        public float Angle
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
        public Vector2 Zoom
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

        /// <summary>
        /// Property to set or return an anchor for rotation, scaling and positioning.
        /// </summary>
        /// <remarks>
        /// This value is in relative coordinates. That is, 0,0 would be the upper left corner of the <see cref="GorgonCameraCommon.ViewDimensions"/>, and 1,1 would be lower right corner of the <see cref="GorgonCameraCommon.ViewDimensions"/>.
        /// </remarks>
        public Vector2 Anchor
        {
            get => _anchor;
            set
            {
                if (_anchor == value)
                {
                    return;
                }

                _anchor = value;
                Changes |= CameraChange.Projection;
            }
        }

        /// <summary>
        /// Property to return the viewable region for the camera.
        /// </summary>
        /// <remarks>
        /// This represents the boundaries of viewable space for the camera using its coordinate system. The upper left of the region corresponds with the upper left of the active render target at minimum 
        /// Z depth, and the lower right of the region corresponds with the lower right of the active render target at minimum Z depth.
        /// </remarks>
        public DX.RectangleF ViewableRegion => new DX.RectangleF(-ViewDimensions.Width * _anchor.X, -ViewDimensions.Height * _anchor.Y, ViewDimensions.Width, ViewDimensions.Height);
        #endregion

        #region Methods.
        /// <summary>Function to update the projection matrix.</summary>
        /// <param name="projectionMatrix">The instance of the matrix to update.</param>
        protected override void UpdateProjectionMatrix(ref Matrix4x4 projectionMatrix)
        {
            var anchor = new Vector2(Anchor.X * ViewDimensions.Width, Anchor.Y * ViewDimensions.Height);
            MatrixFactory.CreateOrthographicOffCenterLH(-anchor.X,
                                                        ViewDimensions.Width - anchor.X,
                                                        ViewDimensions.Height - anchor.Y,
                                                        -anchor.Y,
                                                        MinimumDepth,
                                                        MaximumDepth,
                                                        out projectionMatrix);
        }

        /// <summary>
        /// Function to update the view matrix.
        /// </summary>
        /// <param name="viewMatrix">The instance of the matrix to update.</param>
        protected override void UpdateViewMatrix(ref Matrix4x4 viewMatrix)
        {
            bool hasTranslate = (Changes & CameraChange.Position) == CameraChange.Position;
            bool hasScale = (Changes & CameraChange.Scale) == CameraChange.Scale;
            bool hasRotation = (Changes & CameraChange.Rotation) == CameraChange.Rotation;

            if ((!hasScale) && (!hasRotation) && (!hasTranslate))
            {
                return;
            }

            // Scale it.
            if (hasScale)
            {
                _scale.M11 = _zoom.X;
                _scale.M22 = _zoom.Y;
                _scale.M33 = 1.0f;
                Changes &= ~CameraChange.Scale;
            }

            // Rotate it.
            if (hasRotation)
            {
                MatrixFactory.CreateRotationZ(_angleZ.ToRadians(), out _rotation);
                
                // We need to invert the matrix in order to apply the correct transformation to the world data.
                _rotation.Transpose(out _rotation);
                Changes &= ~CameraChange.Rotation;
            }

            // Translate it.
            if (hasTranslate)
            {
                MatrixFactory.CreateTranslation(new Vector3(-_position.X, -_position.Y, 0.0f), out _translate);
                Changes &= ~CameraChange.Position;
            }

            _rotation.Multiply(in _scale, out Matrix4x4 temp);
            _translate.Multiply(in temp, out viewMatrix);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonOrthoCamera"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface to use with this object.</param>
        /// <param name="viewDimensions">The view dimensions.</param>
        /// <param name="minDepth">[Optional] The minimum depth value.</param>
        /// <param name="maximumDepth">[Optional] The maximum depth value.</param>
        /// <param name="name">[Optional] The name of the camera.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
        public GorgonOrthoCamera(GorgonGraphics graphics, DX.Size2F viewDimensions, float minDepth = 0.0f, float maximumDepth = 1.0f, string name = null)
            : base(graphics, viewDimensions, minDepth, maximumDepth, name)
        {
        }
        #endregion
    }
}