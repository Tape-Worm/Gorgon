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
using System.Drawing;
using System.Linq;
using GorgonLibrary.Graphics;
using GorgonLibrary.Math;
using SlimMath;

namespace GorgonLibrary.Renderers
{
    /// <summary>
    /// A camera that performs perspective (3D) projection.
    /// </summary>
    /// <remarks>This camera is used to bring depth to a 2D scene.  Sprites and other renderables can use their Depth property to determine how far away the renderable is 
    /// from the camera.
    /// <para>By default this camera type uses a relative coordinate system rather than an absolute one.  Therefore all renderables must have their coordinates use relative 
    /// coordinates from -1 to 1 (where 0 is a center value if depth is 0) for both horizontal, and vertical positioning. The user may define their own coordinate system to 
    /// apply to projection.
    /// </para>
    /// </remarks>
    public class GorgonPerspectiveCamera
        : GorgonNamedObject, ICamera
    {
        #region Variables.
        private Matrix _viewProjecton = Matrix.Identity;				// Projection view matrix.
        private Matrix _projection = Matrix.Identity;					// Projection matrix.
        private Matrix _view = Matrix.Identity;							// View matrix.
        private RectangleF _viewDimensions = RectangleF.Empty;			// View projection dimensions.
        private float _maxDepth;										// Maximum depth.
        private float _minDepth;                                        // Minimum depth.
        private readonly GorgonSprite _cameraIcon;						// Camera icon.
        private float _angle;											// Angle of rotation.
        private Vector3 _position = Vector2.Zero;						// Position.
		private Vector2 _zoom = new Vector2(1);							// Zoom.
	    private Vector2 _anchor = Vector2.Zero;							// Anchor point.
        private bool _needsProjectionUpdate = true;						// Flag to indicate that the projection matrix needs updating.
        private bool _needsViewUpdate = true;							// Flag to indicate that the view matrix needs updating.
	    private bool _needsUpload = true;								// Flag to indicate that the view/projection matrix needs uploading to the GPU.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the 2D interface that owns this camera.
        /// </summary>
        public Gorgon2D Gorgon2D
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to set or return the angle of rotation in degrees.
        /// </summary>
        /// <remarks>An orthographic camera can only rotate around a Z-Axis.</remarks>
        public float Angle
        {
            get
            {
                return _angle;
            }
            set
            {
                if (_angle.EqualsEpsilon(value))
                {
                    return;
                }

                _angle = value;
                _needsViewUpdate = true;
            }
        }

        /// <summary>
        /// Property to set or return the camera position.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (value == _position)
                {
                    return;
                }

                _position = value;
                _needsViewUpdate = true;
            }
        }

		/// <summary>
		/// Property to set or return the camera zoom.
		/// </summary>
	    public Vector2 Zoom
	    {
		    get
		    {
			    return _zoom;
		    }
		    set
		    {
			    if (value == _zoom)
			    {
				    return;
			    }

			    _zoom = value;
			    _needsViewUpdate = true;
		    }
	    }

		/// <summary>
		/// Property to set or return the anchor point for the camera.
		/// </summary>
	    public Vector2 Anchor
	    {
		    get
		    {
			    return _anchor;
		    }
		    set
		    {
			    if (value == _anchor)
			    {
				    return;
			    }

			    _anchor = value;
			    _needsProjectionUpdate = true;
		    }
	    }

		/// <summary>
		/// Property to set or return whether the dimensions of the camera should be automatically adjusted to match the current render target.
		/// </summary>
	    public bool AutoUpdate
	    {
		    get;
		    set;
	    }
        #endregion

        #region Methods.
		/// <summary>
		/// Function to perform calculations for the camera.
		/// </summary>
	    private void UpdateMatrices()
	    {
			if (_needsProjectionUpdate)
			{
				Matrix.PerspectiveOffCenterLH(_viewDimensions.Left - _anchor.X,
					_viewDimensions.Right - _anchor.X,
					_viewDimensions.Bottom - _anchor.Y,
					_viewDimensions.Top - _anchor.Y,
					_minDepth,
					_maxDepth,
					out _projection);
			}

			if (_needsViewUpdate)
			{
				UpdateViewMatrix();
			}

			if ((_needsProjectionUpdate) || (_needsViewUpdate))
			{
				_needsUpload = true;
				Matrix.Multiply(ref _view, ref _projection, out _viewProjecton);
			}
			
			_needsProjectionUpdate = false;
			_needsViewUpdate = false;
	    }

        /// <summary>
        /// Function to update the view matrix.
        /// </summary>
        private void UpdateViewMatrix()
        {
	        Matrix translation;					// Translation matrix.

	        Matrix center = Matrix.Identity;

			// Scale it.
			if ((!_zoom.X.EqualsEpsilon(1.0f)) || (!_zoom.Y.EqualsEpsilon(1.0f)))
			{
				center.M11 = _zoom.X;
				center.M22 = _zoom.Y;
				center.M33 = 1.0f;
			}

			if (!_angle.EqualsEpsilon(0.0f))
			{
				Matrix rotation;						// Rotation matrix.

				Matrix.RotationZ(_angle.Radians(), out rotation);
				Matrix.Multiply(ref rotation, ref center, out center);
			}

			Matrix.Translation(ref _position, out translation);
			Matrix.Multiply(ref translation, ref center, out center);

			_view = center;
		}

		/// <summary>
		/// Function to project a screen position into camera space.
		/// </summary>
		/// <param name="screenPosition">3D Position on the screen.</param>
		/// <param name="includeViewTransform">[Optional] TRUE to include the view transformation in the projection calculations, FALSE to only use the projection.</param>
		/// <returns>The projected 3D position of the screen.</returns>
		/// <remarks>Use this to convert a position in screen space into the camera view/projection space.  If the <paramref name="includeViewTransform"/> is set to 
		/// TRUE, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to FALSE only the projection will 
		/// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
		public Vector3 Project(Vector3 screenPosition, bool includeViewTransform = true)
		{
			Matrix transformMatrix;

			UpdateMatrices();

			if (includeViewTransform)
			{
				Matrix.Invert(ref _viewProjecton, out transformMatrix);
			}
			else
			{
				Matrix.Invert(ref _projection, out transformMatrix);
			}

			// Calculate relative position of our screen position.
			var relativePosition = new Vector3(2.0f * screenPosition.X / TargetWidth - 1.0f,
											   1.0f - screenPosition.Y / TargetHeight * 2.0f,
											   screenPosition.Z / (MaximumDepth - MinimumDepth));

			// Transform our screen position by our inverse matrix.
			Vector4 result;
			Vector3.Transform(ref relativePosition, ref transformMatrix, out result);

			Vector4.Divide(ref result, result.W, out result);

			return (Vector3)result;
		}

		/// <summary>
		/// Function to unproject a world space position into screen space.
		/// </summary>
		/// <param name="worldSpacePosition">A position in world space.</param>
		/// <param name="includeViewTransform">[Optional] TRUE to include the view transformation in the projection calculations, FALSE to only use the projection.</param>
		/// <returns>The unprojected world space coordinates.</returns>
		/// <remarks>Use this to convert a position in world space into the screen space.  If the <paramref name="includeViewTransform"/> is set to 
		/// TRUE, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to FALSE only the projection will 
		/// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
		public Vector3 Unproject(Vector3 worldSpacePosition, bool includeViewTransform = true)
		{
			UpdateMatrices();

			Matrix transformMatrix = includeViewTransform ? _viewProjecton : _projection;

			Vector4 transform;
			Vector3.Transform(ref worldSpacePosition, ref transformMatrix, out transform);

			Vector4.Divide(ref transform, transform.W, out transform);

			return new Vector3((transform.X + 1.0f) * 0.5f * TargetWidth,
				(1.0f - transform.Y) * 0.5f * TargetHeight,
				transform.Z * (MaximumDepth - MinimumDepth) + MinimumDepth);
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonPerspectiveCamera"/> class.
        /// </summary>
        /// <param name="gorgon2D">The gorgon 2D interface that owns the camera.</param>
        /// <param name="name">The name of the camera.</param>
        /// <param name="viewDimensions">The view dimensions.</param>
        /// <param name="minDepth">The minimum depth value.</param>
        /// <param name="maximumDepth">The maximum depth of the projection.</param>
        internal GorgonPerspectiveCamera(Gorgon2D gorgon2D, string name, RectangleF viewDimensions, float minDepth, float maximumDepth)
            : base(name)
        {
            Gorgon2D = gorgon2D;
            _maxDepth = maximumDepth;
	        _minDepth = minDepth;
	        _viewDimensions = viewDimensions;

            _cameraIcon = new GorgonSprite(gorgon2D, "GorgonCamera.PerspIcon", new GorgonSpriteSettings
            {
                Size = new Vector2(64, 50),
                Texture = gorgon2D.Graphics.GetTrackedObjectsOfType<GorgonTexture2D>()
                            .FirstOrDefault(item => item.Name.Equals("Gorgon2D.Icons", StringComparison.OrdinalIgnoreCase)) ??
                        gorgon2D.Graphics.Textures.CreateTexture<GorgonTexture2D>("Gorgon2D.Icons", Properties.Resources.Icons),
                TextureRegion = new RectangleF(0, 0, 0.25f, 0.195313f),
                Anchor = new Vector2(32f, 25),
                InitialScale = new Vector2(1.0f),
                Color = Color.White
            });
        }
        #endregion

        #region ICamera Members
        #region Properties.
		/// <summary>
		/// Property to return the horizontal and vertical aspect ratio for the camera view area.
		/// </summary>
		public Vector2 AspectRatio
		{
			get
			{
				return new Vector2(TargetWidth / (float)TargetHeight, TargetHeight / (float)TargetWidth);
			}
		}
		
		/// <summary>
        /// Property to return the projection matrix for the camera.
        /// </summary>
        public Matrix Projection
        {
            get
            {
                return _projection;
            }
        }

        /// <summary>
        /// Property to return the view matrix for the camera.
        /// </summary>
        public Matrix View
        {
            get
            {
                return _view;
            }
        }

        /// <summary>
        /// Property to return the combined view and projection matrix.
        /// </summary>
        public Matrix ViewProjection
        {
            get
            {
                return _viewProjecton;
            }
        }

        /// <summary>
        /// Property to return whether the camera needs updating.
        /// </summary>
        public bool NeedsUpdate
        {
            get
            {
                return _needsViewUpdate || _needsProjectionUpdate || _needsUpload;
            }
        }

		/// <summary>
		/// Property to set or return the projection view dimensions for the camera.
		/// </summary>
		public RectangleF ViewDimensions
		{
			get
			{
				return _viewDimensions;
			}
			set
			{
				if (_viewDimensions == value)
				{
					return;
				}

				_viewDimensions = value;
				_needsProjectionUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the minimum depth for the camera.
		/// </summary>
		public float MinimumDepth
		{
			get
			{
				return _minDepth;
			}
			set
			{
				_minDepth = value;
				_needsProjectionUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the maximum depth for the camera.
		/// </summary>
		public float MaximumDepth
		{
			get
			{
				return _maxDepth;
			}
			set
			{
				if (value < 1.0f)
				{
					value = 1.0f;
				}

				_maxDepth = value;
				_needsProjectionUpdate = true;
			}
		}

		/// <summary>
		/// Property to return the width of the current target.
		/// </summary>
		public int TargetWidth
		{
			get
			{
				switch (Gorgon2D.Target.Resource.ResourceType)
				{
					case ResourceType.Buffer:
						return Gorgon2D.Target.Resource.SizeInBytes / Gorgon2D.Target.FormatInformation.SizeInBytes;
					case ResourceType.Texture1D:
						return ((GorgonRenderTarget1D)Gorgon2D.Target.Resource).Settings.Width;
					case ResourceType.Texture2D:
						return ((GorgonRenderTarget2D)Gorgon2D.Target.Resource).Settings.Width;
					case ResourceType.Texture3D:
						return ((GorgonRenderTarget3D)Gorgon2D.Target.Resource).Settings.Width;
					default:
						return 0;
				}
			}
		}

		/// <summary>
		/// Property to return the height of the current target.
		/// </summary>
		public int TargetHeight
		{
			get
			{
				switch (Gorgon2D.Target.Resource.ResourceType)
				{
					case ResourceType.Buffer:
						return 1;
					case ResourceType.Texture1D:
						return 1;
					case ResourceType.Texture2D:
						return ((GorgonRenderTarget2D)Gorgon2D.Target.Resource).Settings.Height;
					case ResourceType.Texture3D:
						return ((GorgonRenderTarget3D)Gorgon2D.Target.Resource).Settings.Height;
					default:
						return 0;
				}
			}
		}
        #endregion

		#region Methods.
        /// <summary>
        /// Function to draw the camera icon.
        /// </summary>
        public void Draw()
        {
			// Calculate the size of the camera icon in our camera space.
			var newSize = new Vector2(64.0f * _viewDimensions.Width / TargetWidth, 50.0f * _viewDimensions.Height / TargetHeight);

			if ((!_zoom.X.EqualsEpsilon(1.0f)) || (!_zoom.Y.EqualsEpsilon(1.0f)))
			{
				_cameraIcon.Scale = new Vector2(1.0f / _zoom.X, 1.0f / _zoom.Y);
			}

            // Highlight current camera.
            _cameraIcon.Color = Gorgon2D.Camera == this ? Color.Green : Color.White;

			_cameraIcon.Depth = 0.0f;
            _cameraIcon.Position = new Vector2(-_position.X, -_position.Y);
            _cameraIcon.Angle = -_angle;
	        _cameraIcon.Size = newSize;
	        _cameraIcon.Opacity = 0.8f;
			_cameraIcon.Anchor = new Vector2(_cameraIcon.Size.X / 2.0f, _cameraIcon.Size.Y / 2.0f);

			// Draw the icon in our camera space, otherwise it won't look right.
			var prevCamera = Gorgon2D.Camera;

	        if (prevCamera != this)
	        {
		        Gorgon2D.Camera = this;
	        }

	        _cameraIcon.Draw();

	        if (prevCamera == this)
	        {
		        return;
	        }
			
	        Gorgon2D.Camera = prevCamera;
        }

        /// <summary>
        /// Function to update the view projection matrix for the camera and populate a view/projection constant buffer.
        /// </summary>
        public void Update()
        {
			UpdateMatrices();

            // Update the projection view matrix on the vertex shader.
            Gorgon2D.VertexShader.TransformBuffer.Update(ref _viewProjecton);

	        _needsUpload = false;
        }
		#endregion
		#endregion
	}
}
