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
using System.Linq;
using System.Drawing;
using GorgonLibrary.Graphics;
using GorgonLibrary.Math;
using SlimMath;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// An orthographic camera for Gorgon.
	/// </summary>
	public class GorgonOrthoCamera
		: GorgonNamedObject, ICamera
	{
		#region Variables.
		private Matrix _viewProjecton = Matrix.Identity;				// Projection view matrix.
		private Matrix _projection = Matrix.Identity;					// Projection matrix.
		private Matrix _view = Matrix.Identity;							// View matrix.
		private Vector2 _viewDimensions = Vector2.Zero;					// View projection dimensions.
		private Vector2 _viewOffset = Vector2.Zero;						// Offset for the view projection.
		private float _maxDepth;										// Maximum depth.
		private readonly GorgonSprite _cameraIcon;						// Camera icon.
		private float _angle;											// Angle of rotation.
		private Vector2 _scale = new Vector2(1.0f);						// Scale.
		private Vector2 _position = Vector2.Zero;						// Position.
		private Vector2 _anchor = Vector2.Zero;							// Target position.
		private bool _needsProjectionUpdate = true;						// Flag to indicate that the projection matrix needs updating.
		private bool _needsViewUpdate = true;							// Flag to indicate that the view matrix needs updating.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the interface that created this camera.
		/// </summary>
		public Gorgon2D Gorgon2D
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the projection view dimensions for the camera.
		/// </summary>
		public Vector2 ViewDimensions
		{
			get
			{
				return _viewDimensions;
			}
			set
			{
				UpdateRegion(_viewOffset, value);
			}
		}

		/// <summary>
		/// Property to set or return the projection view offset for the camera.
		/// </summary>
		public Vector2 ViewOffset
		{
			get
			{
				return _viewOffset;
			}
			set
			{
				UpdateRegion(value, _viewDimensions);
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
		public Vector2 Position
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
		/// Property to set or return the zoom for the camera.
		/// </summary>
		public Vector2 Zoom
		{
			get
			{
				return _scale;
			}
			set
			{
				if (value == _scale)
				{
					return;
				}

				_scale = value;
				_needsViewUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return an anchor for rotation, scaling and positioning.
		/// </summary>
		public Vector2 Anchor
		{
			get
			{
				return _anchor;
			}
			set
			{
				if (_anchor == value)
				{
					return;
				}

				_anchor = value;
				_needsViewUpdate = true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the view matrix.
		/// </summary>
		private void UpdateViewMatrix()
		{
			Matrix center;						// Centering matrix.
			Matrix translation;					// Translation matrix.

			// Anchor the view.
			Matrix.Translation(_anchor.X, _anchor.Y, 0.0f, out center);

			// Scale it.
			if ((!_scale.X.EqualsEpsilon(1.0f)) || (!_scale.Y.EqualsEpsilon(1.0f)))
			{
				center.M11 = _scale.X;
				center.M22 = _scale.Y;
				center.M33 = 1.0f;
			}

			if (!_angle.EqualsEpsilon(0.0f))
			{
				Matrix rotation;						// Rotation matrix.

				Matrix.RotationZ(_angle.Radians(), out rotation);
				Matrix.Multiply(ref rotation, ref center, out center);
			}

			Matrix.Translation(_position.X, _position.Y, 0.0f, out translation);
			Matrix.Multiply(ref translation, ref center, out center);

			_view = center;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonOrthoCamera"/> class.
		/// </summary>
		/// <param name="gorgon2D">The 2D interface that created this object.</param>
		/// <param name="name">The name.</param>
		/// <param name="viewDimensions">The view dimensions.</param>
		/// <param name="maximumDepth">The maximum depth.</param>
		internal GorgonOrthoCamera(Gorgon2D gorgon2D, string name, Vector2 viewDimensions, float maximumDepth)
			: base(name)
		{
			Gorgon2D = gorgon2D;
			_maxDepth = maximumDepth;
			_viewDimensions = viewDimensions;
			_cameraIcon = new GorgonSprite(gorgon2D, "GorgonCamera.OrthoIcon", new GorgonSpriteSettings
			{
				Size = new Vector2(64, 50),
				Texture = gorgon2D.Graphics.GetTrackedObjectsOfType<GorgonTexture2D>()
							.FirstOrDefault(item => item.Name.Equals("Gorgon2D.Icons", StringComparison.OrdinalIgnoreCase)) ??
						gorgon2D.Graphics.Textures.CreateTexture<GorgonTexture2D>("Gorgon2D.Icons", Properties.Resources.Icons),
				TextureRegion = new RectangleF(0.253906f, 0, 0.253906f, 0.195313f),
				Anchor = new Vector2(32.5f, 25),
				InitialScale = new Vector2(1.0f),
				Color = Color.White
			});
		}
		#endregion

		#region ICamera Members
		#region Properties.
		/// <summary>
		/// Property to return whether the camera needs updating.
		/// </summary>
		public bool NeedsUpdate
		{
			get
			{
				return _needsProjectionUpdate || _needsViewUpdate;
			}
		}

        /// <summary>
        /// Property to return the 
        /// </summary>
        public Matrix ViewProjection
        {
            get
            {
                return _viewProjecton;
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
		/// Property to set or return whether to allow the renderer to automatically update this camera.
		/// </summary>
		/// <remarks>
		/// Setting this to TRUE and setting this camera as the active camera for the renderer will allow the renderer to update the camera projection when the render target is resized.
		/// </remarks>
		public bool AutoUpdate
		{
			get;
			set;
		}
		#endregion

		#region ICamera Implementation.
		/// <summary>
		/// Function to update the view projection matrix for the camera and populate a view/projection constant buffer.
		/// </summary>
		public void Update()
		{
			if (_needsProjectionUpdate)
			{
				Matrix.OrthoOffCenterLH(_viewOffset.X, _viewDimensions.X, _viewDimensions.Y, _viewOffset.Y, 0.0f, _maxDepth, out _projection);
			}
			
			if (_needsViewUpdate)
			{
				UpdateViewMatrix();
			}

			if ((_needsProjectionUpdate) || (_needsViewUpdate))
			{
				Matrix.Multiply(ref _view, ref _projection, out _viewProjecton);
			}

			// Update the projection view matrix on the vertex shader.
			Gorgon2D.VertexShader.TransformBuffer.Update(ref _viewProjecton);

			_needsProjectionUpdate = false;
			_needsViewUpdate = false;
		}

		/// <summary>
		/// Function to update the projection matrix from a specified region.
		/// </summary>
		/// <param name="offset">The offset of the region.</param>
		/// <param name="size">The width and the height for the region.</param>
		public void UpdateRegion(Vector2 offset, Vector2 size)
		{
			_viewOffset = offset;
			_viewDimensions = size;

			if (AutoUpdate)
			{
				Vector2.Multiply(ref size, 0.5f, out _anchor);
				Vector2.Negate(ref _anchor, out _position);
			}

			_needsProjectionUpdate = true;
			_needsViewUpdate = true;
		}

		/// <summary>
		/// Function to draw the camera icon.
		/// </summary>
		public void Draw()
		{
			if ((!_scale.X.EqualsEpsilon(0.0f)) && (!_scale.Y.EqualsEpsilon(0.0f)))
			{
				_cameraIcon.Scale = new Vector2(1.0f / _scale.X, 1.0f / _scale.Y);
			}

			// Highlight current camera.
			_cameraIcon.Color = Gorgon2D.Camera == this ? Color.Green : Color.White;

			_cameraIcon.Position = -Position;
			_cameraIcon.Angle = -Angle;
			_cameraIcon.Draw();
		}
		#endregion
		#endregion
	}
}