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
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A camera that performs orthographic (2D) projection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The orthographic camera is the default camera used in Gorgon.  This camera performs 2D projection of sprites and other renderables on to a target. 
    /// By default, the camera will use absolute screen space coordinates e.g. 160x120 will be the center of a 320x240 render target.  The user may define their own  
    /// coordinate system to apply to the projection.
    /// </para>
    /// </remarks>
    public class Gorgon2DOrthoCamera
        : IGorgon2DCamera
    {
        #region Variables.
        // View projection dimensions.
        private DX.Size2F _viewDimensions;
        // Maximum depth.
        private float _maxDepth;
        // Minimum depth value.
        private float _minDepth;
        // The raw projection matrix.
        private DX.Matrix _projectionMatrix = DX.Matrix.Identity;
        // The raw view matrix.
        private DX.Matrix _viewMatrix = DX.Matrix.Identity;
        // The position of the camera.
        private DX.Vector2 _position;
        // Scale.
        private DX.Vector2 _zoom = new DX.Vector2(1.0f);
        // Target position.
        private DX.Vector2 _anchor = DX.Vector2.Zero;
        // Angle of rotation.
        private float _angle;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether the projection changed or not.
        /// </summary>
        public bool ProjectionChanged
        {
            get;
            private set;
        } = true;

		/// <summary>
		/// Property to return whether the projection changed or not.
		/// </summary>
		public bool ViewChanged
        {
            get;
            private set;
        } = true;

        /// <summary>
        /// Property to return the viewable region for the camera.
        /// </summary>
        /// <remarks>
        /// This represents the boundaries of viewable space for the camera using its coordinate system. The upper left of the region corresponds with the upper left of the active render target at minimum 
        /// Z depth, and the lower right of the region corresponds with the lower right of the active render target at minimum Z depth.
        /// </remarks>
        public DX.RectangleF ViewableRegion => new DX.RectangleF(-_viewDimensions.Width * _anchor.X, -_viewDimensions.Height * _anchor.Y, _viewDimensions.Width, _viewDimensions.Height);

        /// <summary>
        /// Property to set or return the position of the camera.
        /// </summary>
        DX.Vector3 IGorgon2DCamera.Position
        {
            get => new DX.Vector3(Position, 0);
            set => Position = (DX.Vector2)value;
        }

        /// <summary>
        /// Property to set or return the camera position.
        /// </summary>
        public DX.Vector2 Position
        {
            get => _position;
            set
            {
                if (value == _position)
                {
                    return;
                }

                _position = value;
                ViewChanged = true;
            }
        }


        /// <summary>
        /// Property to set or return the angle of rotation in degrees.
        /// </summary>
        /// <remarks>
        /// An orthographic camera can only rotate around a Z-Axis.
        /// </remarks>
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
                ViewChanged = true;
            }
        }

        /// <summary>
        /// Property to set or return the zoom for the camera.
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
                ViewChanged = true;
            }
        }

        /// <summary>
        /// Property to set or return an anchor for rotation, scaling and positioning.
        /// </summary>
        /// <remarks>
        /// This value is in relative coordinates. That is, 0,0 would be the upper left corner of the <see cref="ViewDimensions"/>, and 1,1 would be lower right corner of the <see cref="ViewDimensions"/>.
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
                ProjectionChanged = true;
            }
        }

        /// <summary>
        /// Property to return whether or not the camera needs its data updated.
        /// </summary>
        public bool NeedsUpdate => (ProjectionChanged) || (ViewChanged);

        /// <summary>
        /// Property to return the horizontal and vertical aspect ratio for the camera view area.
        /// </summary>
        public DX.Vector2 AspectRatio => TargetWidth > TargetHeight
                    ? new DX.Vector2((float)TargetWidth / TargetHeight, 1)
                    : new DX.Vector2(1.0f, (float)TargetHeight / TargetWidth);

        /// <summary>
        /// Property to set or return the projection view dimensions for the camera.
        /// </summary>
        public DX.Size2F ViewDimensions
		{
			get => _viewDimensions;
		    set
			{
				if (_viewDimensions.Equals(value))
				{
					return;
				}

				_viewDimensions = value;
				ProjectionChanged = true;
			}
		}

		/// <summary>
		/// Property to set or return the minimum depth for the camera.
		/// </summary>
		public float MinimumDepth
		{
			get => _minDepth;
		    set
			{
			    if (_minDepth == value)
			    {
			        return;
			    }

				_minDepth = value;
				ProjectionChanged = true;
			}
		}

		/// <summary>
		/// Property to set or return the maximum depth for the camera.
		/// </summary>
		public float MaximumDepth
		{
			get => _maxDepth;
		    set
			{
				if (value < 1.0f)
				{
					value = 1.0f;
				}

			    if (_maxDepth == value)
			    {
			        return;
			    }

				_maxDepth = value;
				ProjectionChanged = true;
			}
		}

	    /// <summary>
	    /// Property to set or return a flag to indicate that the renderer should automatically update this camera when the render target size changes.
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// When this flag is set to <b>true</b>, the renderer will automatically update the <see cref="ViewDimensions"/> for this camera when the current render target is resized (typically in response
	    /// to a window resize event). However, this is not always desirable, and when set to <b>false</b>, the camera <see cref="ViewDimensions"/> will not be resized.
	    /// </para>
	    /// <para>
	    /// If this value is set to <b>false</b>, then it is the responsibility of the developer to update the camera manually when required.
	    /// </para>
	    /// </remarks>
	    public bool AllowUpdateOnResize
	    {
	        get;
	        set;
	    } = true;

	    /// <summary>
	    /// Property to return the width of the current target.
	    /// </summary>
	    public int TargetWidth => Renderer.Graphics.RenderTargets[0]?.Width ?? 0;

	    /// <summary>
	    /// Property to return the height of the current target.
	    /// </summary>
	    public int TargetHeight => Renderer.Graphics.RenderTargets[0]?.Height ?? 0;

		/// <summary>
		/// Property to return the 2D renderer assigned to this camera.
		/// </summary>
		public Gorgon2D Renderer
		{
			get;
		}

	    /// <summary>
	    /// Property to return the name of this object.
	    /// </summary>
	    public string Name
	    {
	        get;
	    }
        #endregion

		#region Methods.
		/// <summary>
		/// Function to update the view matrix.
		/// </summary>
		private void UpdateViewMatrix()
		{
			DX.Matrix center = DX.Matrix.Identity;	// Centering matrix.

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
			    // This is the inversion of the rotation matrix.
			    // We need to invert the matrix in order to apply the correct transformation to the world data.
                rotation.Transpose();
				DX.Matrix.Multiply(ref rotation, ref center, out center);
			}
			// ReSharper restore CompareOfFloatsByEqualityOperator

		    // Pass in the inversion of the positioning.
		    DX.Matrix.Translation(-_position.X, -_position.Y, 0.0f, out DX.Matrix translation);
			DX.Matrix.Multiply(ref translation, ref center, out _viewMatrix);
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
        public void Project(ref DX.Vector3 screenPosition, out DX.Vector3 result, bool includeViewTransform = true) =>
            Project(ref screenPosition, out result, new DX.Size2(TargetWidth, TargetHeight), includeViewTransform);

        /// <summary>
        /// Function to project a screen position into camera space.
        /// </summary>
        /// <param name="screenPosition">3D Position on the screen.</param>
        /// <param name="result">The resulting projected position.</param>
        /// <param name="targetSize">The size of the render target.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <remarks>Use this to convert a position in screen space into the camera view/projection space.  If the <paramref name="includeViewTransform"/> is set to 
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        public void Project(ref DX.Vector3 screenPosition, out DX.Vector3 result, DX.Size2 targetSize, bool includeViewTransform = true)
        {
            DX.Matrix transformMatrix;

            if (ViewChanged)
            {
                GetViewMatrix(out _viewMatrix);
                // Reset the flag so we can actually use the camera when rendering.
                ViewChanged = true;
            }

            if (ProjectionChanged)
            {
                GetProjectionMatrix(out _projectionMatrix);
                ProjectionChanged = true;
            }

            if (includeViewTransform)
            {
                DX.Matrix.Multiply(ref _viewMatrix, ref _projectionMatrix, out DX.Matrix viewProjection);
                DX.Matrix.Invert(ref viewProjection, out transformMatrix);
            }
            else
            {
                DX.Matrix.Invert(ref _projectionMatrix, out transformMatrix);
            }

            // Calculate relative position of our screen position.
            var relativePosition = new DX.Vector2((2.0f * screenPosition.X / targetSize.Width) - 1.0f,
                                               1.0f - (screenPosition.Y / targetSize.Height * 2.0f));

            // Transform our screen position by our inverse matrix.
            DX.Vector2.Transform(ref relativePosition, ref transformMatrix, out DX.Vector4 transformed);

            result = (DX.Vector3)transformed;

	        // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (transformed.W != 1.0f)
            {
                DX.Vector3.Divide(ref result, transformed.W, out result);
            }
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
        public void Unproject(ref DX.Vector3 worldSpacePosition, out DX.Vector3 result, bool includeViewTransform = true) =>
            Unproject(ref worldSpacePosition, out result, new DX.Size2(TargetWidth, TargetHeight), includeViewTransform);
        

        /// <summary>
        /// Function to unproject a world space position into screen space.
        /// </summary>
        /// <param name="worldSpacePosition">A position in world space.</param>
        /// <param name="result">The resulting projected position.</param>
        /// <param name="targetSize">The size of the render target.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <returns>The unprojected world space coordinates.</returns>
        /// <remarks>Use this to convert a position in world space into the screen space.  If the <paramref name="includeViewTransform"/> is set to 
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        public void Unproject(ref DX.Vector3 worldSpacePosition, out DX.Vector3 result, DX.Size2 targetSize, bool includeViewTransform = true)
        {
            if (ViewChanged)
            {
                GetViewMatrix(out _viewMatrix);
                // Reset the flag so we can actually use the camera when rendering.
                ViewChanged = true;
            }

            if (ProjectionChanged)
            {
                GetProjectionMatrix(out _projectionMatrix);
                ProjectionChanged = true;
            }

            DX.Matrix transformMatrix;

            if (includeViewTransform)
            {
                DX.Matrix.Multiply(ref _viewMatrix, ref _projectionMatrix, out transformMatrix);
            }
            else
            {
                transformMatrix = _viewMatrix;
            }

            DX.Vector3.Transform(ref worldSpacePosition, ref transformMatrix, out DX.Vector4 transform);

	        // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (transform.W != 1.0f)
            {
                DX.Vector4.Divide(ref transform, transform.W, out transform);
            }

            result = new DX.Vector3((transform.X + 1.0f) * 0.5f * targetSize.Width,
                (1.0f - transform.Y) * 0.5f * targetSize.Height, 0);
        }

        /// <summary>
        /// Function to project a screen position into camera space.
        /// </summary>
        /// <param name="screenPosition">3D Position on the screen.</param>
        /// <param name="targetSize">The size of the render target</param>.
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <returns>
        /// The projected 3D position of the screen.
        /// </returns>
        /// <remarks>
        /// Use this to convert a position in screen space into the camera view/projection space.  If the <paramref name="includeViewTransform" /> is set to
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.
        /// </remarks>
        public DX.Vector3 Project(DX.Vector3 screenPosition, DX.Size2 targetSize, bool includeViewTransform = true)
        {
            Project(ref screenPosition, out DX.Vector3 result, targetSize, includeViewTransform);

            return result;
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
        public DX.Vector3 Project(DX.Vector3 screenPosition, bool includeViewTransform = true)
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
        public DX.Vector3 Unproject(DX.Vector3 worldSpacePosition, bool includeViewTransform = true)
        {
            Unproject(ref worldSpacePosition, out DX.Vector3 result, includeViewTransform);

            return result;
        }

        /// <summary>
        /// Function to unproject a world space position into screen space.
        /// </summary>
        /// <param name="worldSpacePosition">A position in world space.</param>
        /// <param name="targetSize">The size of the render target.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <returns>The unprojected world space coordinates.</returns>
        /// <remarks>Use this to convert a position in world space into the screen space.  If the <paramref name="includeViewTransform"/> is set to 
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        public DX.Vector3 Unproject(DX.Vector3 worldSpacePosition, DX.Size2 targetSize, bool includeViewTransform = true)
        {
            Unproject(ref worldSpacePosition, out DX.Vector3 result, targetSize, includeViewTransform);

            return result;
        }

        /// <summary>
        /// Function to retrieve the view matrix for the camera.
        /// </summary>
        /// <param name="view">The view matrix.</param>
        public void GetViewMatrix(out DX.Matrix view)
	    {
	        if (ViewChanged)
	        {
	            UpdateViewMatrix();
	        }
            
	        view = _viewMatrix;
	        ViewChanged = false;
	    }

	    /// <summary>
	    /// Function to retrieve the projection matrix for the camera type.
	    /// </summary>
	    /// <param name="projection">The project matrix.</param>
	    public void GetProjectionMatrix(out DX.Matrix projection)
	    {
	        if (ProjectionChanged)
	        {
	            var anchor = new DX.Vector2(Anchor.X * ViewDimensions.Width, Anchor.Y * ViewDimensions.Height);
	            DX.Matrix.OrthoOffCenterLH(-anchor.X,
	                                       ViewDimensions.Width - anchor.X,
	                                       ViewDimensions.Height - anchor.Y,
	                                       -anchor.Y,
	                                       MinimumDepth,
	                                       MaximumDepth,
	                                       out _projectionMatrix);
	        }

	        projection = _projectionMatrix;
	        ProjectionChanged = false;
	    }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DOrthoCamera"/> class.
		/// </summary>
		/// <param name="gorgon2D">The 2D renderer to use with this object.</param>
		/// <param name="viewDimensions">The view dimensions.</param>
		/// <param name="minDepth">[Optional] The minimum depth value.</param>
		/// <param name="maximumDepth">[Optional] The maximum depth value.</param>
		/// <param name="name">[Optional] The name of the camera.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="gorgon2D"/> parameter is <b>null</b>.</exception>
		public Gorgon2DOrthoCamera(Gorgon2D gorgon2D, DX.Size2F viewDimensions,float minDepth = 0.0f, float maximumDepth = 1.0f, string name = null)
		{
            Renderer = gorgon2D ?? throw new ArgumentNullException(nameof(gorgon2D));
		    ViewDimensions = viewDimensions;
		    Name = string.IsNullOrWhiteSpace(name) ? $"Gorgon2D.OrthoCamera.{Guid.NewGuid():N}" : name;
		    MinimumDepth = minDepth;
		    MaximumDepth = maximumDepth.Max(1.0f);
		}
		#endregion
	}
}