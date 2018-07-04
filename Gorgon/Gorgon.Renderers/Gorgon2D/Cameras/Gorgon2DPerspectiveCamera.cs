#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, September 09, 2013 8:08:48 PM
// 
#endregion

using System;
using DX = SharpDX;
using Gorgon.Math;

namespace Gorgon.Renderers
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
    public class Gorgon2DPerspectiveCamera
        : Gorgon2DCamera
    {
        #region Variables.
        // Angle of rotation.
        private float _angle;											
        // Position.
        private DX.Vector3 _position = DX.Vector3.Zero;						
        // Zoom.
		private DX.Vector2 _zoom = new DX.Vector2(1);							
        // Anchor point.
	    private DX.Vector2 _anchor = DX.Vector2.Zero;					
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the angle of rotation in degrees.
        /// </summary>
        /// <remarks>An orthographic camera can only rotate around a Z-Axis.</remarks>
        public float Angle
        {
            get => _angle;
            set
            {
	            // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_angle == value)
                {
                    return;
                }

                _angle = value;
                NeedsViewUpdate = true;
            }
        }

        /// <summary>
        /// Property to set or return the camera position.
        /// </summary>
        public DX.Vector3 Position
        {
            get => _position;
            set
            {
                if (value == _position)
                {
                    return;
                }

                _position = value;
                NeedsViewUpdate = true;
            }
        }

		/// <summary>
		/// Property to set or return the camera zoom.
		/// </summary>
	    public DX.Vector2 Zoom
	    {
		    get => _zoom;
		    set
		    {
			    if (value == _zoom)
			    {
				    return;
			    }

			    _zoom = value;
			    NeedsViewUpdate = true;
		    }
	    }

        /// <summary>
        /// Property to set or return an anchor for rotation, scaling and positioning.
        /// </summary>
        /// <remarks>
        /// This value is in relative coordinates. That is, 0,0 would be the upper left corner of the <see cref="Gorgon2DCamera.ViewDimensions"/>, and 1,1 would be lower right corner of the <see cref="Gorgon2DCamera.ViewDimensions"/>.
        /// </remarks>
        public DX.Vector2 Anchor
        {
            get => _anchor;
            set
            {
                if (_anchor == value)
                {
                    return;
                }

                _anchor = value;
                NeedsProjectionUpdate = true;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the view matrix.
        /// </summary>
        private void UpdateViewMatrix()
        {
            DX.Matrix center = DX.Matrix.Identity;

			// Scale it.
	        // ReSharper disable CompareOfFloatsByEqualityOperator
			if ((_zoom.X != 1.0f) || (_zoom.Y != 1.0f))
			{
				center.M11 = _zoom.X;
				center.M22 = _zoom.Y;
				center.M33 = 1.0f;
			}

			if (_angle != 0.0f)
			{
			    DX.Matrix.RotationZ(_angle.ToRadians(), out DX.Matrix rotation);
				DX.Matrix.Multiply(ref rotation, ref center, out center);
			}
			// ReSharper restore CompareOfFloatsByEqualityOperator

			DX.Matrix.Translation(ref _position, out DX.Matrix translation);
			DX.Matrix.Multiply(ref translation, ref center, out ViewMatrix);
		}

        /// <summary>
        /// Function to project a screen position into camera space.
        /// </summary>
        /// <param name="screenPosition">3D Position on the screen.</param>
        /// <param name="result">The resulting projected position.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <remarks>Use this to convert a position in screen space into the camera view/projection space.  If the <paramref name="includeViewTransform"/> is set to 
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        public override void Project(ref DX.Vector3 screenPosition, out DX.Vector3 result, bool includeViewTransform = true)
        {
            DX.Matrix transformMatrix;

            Update();

            if (includeViewTransform)
            {
                DX.Matrix.Invert(ref ViewProjectionMatrix, out transformMatrix);
            }
            else
            {
                DX.Matrix.Invert(ref ProjectionMatrix, out transformMatrix);
            }

            // Calculate relative position of our screen position.
            var relativePosition = new DX.Vector3(2.0f * screenPosition.X / TargetWidth - 1.0f,
                                               1.0f - screenPosition.Y / TargetHeight * 2.0f,
                                               screenPosition.Z / (MaximumDepth - MinimumDepth));

            // Transform our screen position by our inverse matrix.
            DX.Vector3.Transform(ref relativePosition, ref transformMatrix, out DX.Vector4 transformed);

            result = (DX.Vector3)transformed;

            DX.Vector3.Divide(ref result, transformed.W, out result);
        }

        /// <summary>
        /// Function to unproject a world space position into screen space.
        /// </summary>
        /// <param name="worldSpacePosition">A position in world space.</param>
        /// <param name="result">The resulting projected position.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <returns>The unprojected world space coordinates.</returns>
        /// <remarks>Use this to convert a position in world space into the screen space.  If the <paramref name="includeViewTransform"/> is set to 
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        public override void Unproject(ref DX.Vector3 worldSpacePosition, out DX.Vector3 result, bool includeViewTransform = true)
        {
            Update();

            DX.Matrix transformMatrix = includeViewTransform ? ViewProjectionMatrix : ProjectionMatrix;

            DX.Vector3.Transform(ref worldSpacePosition, ref transformMatrix, out DX.Vector4 transform);

            DX.Vector4.Divide(ref transform, transform.W, out transform);

            result = new DX.Vector3((transform.X + 1.0f) * 0.5f * TargetWidth,
                (1.0f - transform.Y) * 0.5f * TargetHeight,
                transform.Z * (MaximumDepth - MinimumDepth) + MinimumDepth);
        }

        /// <summary>
        /// Function to project a screen position into camera space.
        /// </summary>
        /// <param name="screenPosition">3D Position on the screen.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <returns>
        /// The projected 3D position of the screen.
        /// </returns>
        /// <remarks>
        /// Use this to convert a position in screen space into the camera view/projection space.  If the <paramref name="includeViewTransform" /> is set to
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.
        /// </remarks>
        public override DX.Vector3 Project(DX.Vector3 screenPosition, bool includeViewTransform = true)
        {
            Project(ref screenPosition, out DX.Vector3 result, includeViewTransform);

            return result;
        }

        /// <summary>
        /// Function to unproject a world space position into screen space.
        /// </summary>
        /// <param name="worldSpacePosition">A position in world space.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <returns>The unprojected world space coordinates.</returns>
        /// <remarks>Use this to convert a position in world space into the screen space.  If the <paramref name="includeViewTransform"/> is set to 
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        public override DX.Vector3 Unproject(DX.Vector3 worldSpacePosition, bool includeViewTransform = true)
        {
            Unproject(ref worldSpacePosition, out DX.Vector3 result, includeViewTransform);

            return result;
        }

        /// <summary>
        /// Function to update the view projection matrix for the camera and populate a view/projection constant buffer.
        /// </summary>
        public override void Update()
        {
            if (NeedsProjectionUpdate)
            {
                var anchor = new DX.Vector2(Anchor.X * ViewDimensions.Width, Anchor.Y * ViewDimensions.Height);
                DX.Matrix.PerspectiveOffCenterLH(ViewDimensions.Left - anchor.X,
                                                 ViewDimensions.Right - anchor.X,
                                                 ViewDimensions.Bottom - anchor.Y,
                                                 ViewDimensions.Top - anchor.Y,
                                                 MinimumDepth,
                                                 MaximumDepth,
                                                 out ProjectionMatrix);
            }

            if (NeedsViewUpdate)
            {
                UpdateViewMatrix();
            }

            if ((NeedsProjectionUpdate) || (NeedsViewUpdate))
            {
                DX.Matrix.Multiply(ref ViewMatrix, ref ProjectionMatrix, out ViewProjectionMatrix);
                NeedsUpload = true;
            }
			
            NeedsProjectionUpdate = false;
            NeedsViewUpdate = false;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DPerspectiveCamera"/> class.
        /// </summary>
        /// <param name="gorgon2D">The 2D renderer to use with this object.</param>
        /// <param name="viewDimensions">The view dimensions.</param>
        /// <param name="minDepth">[Optional] The minimum depth value.</param>
        /// <param name="maximumDepth">[Optional] The maximum depth value.</param>
        /// <param name="name">[Optional] The name of the camera.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="gorgon2D"/> parameter is <b>null</b>.</exception>
        public Gorgon2DPerspectiveCamera(Gorgon2D gorgon2D, DX.RectangleF viewDimensions, float minDepth = 0.1f, float maximumDepth = 1000.0f, string name = null)
            : base(gorgon2D, name)
        {
            MaximumDepth = maximumDepth.Max(1.0f);
	        MinimumDepth = minDepth;
	        ViewDimensions = viewDimensions;
        }
        #endregion
	}
}
