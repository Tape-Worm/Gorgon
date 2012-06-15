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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimMath;
using GorgonLibrary.Math;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// An orthographic camera for Gorgon.
	/// </summary>
	public class GorgonOrthoCamera
		: GorgonNamedObject, ICamera
	{
		#region Variables.
		private Matrix _projection = Matrix.Identity;					// Projection matrix.
		private Matrix _view = Matrix.Identity;							// View matrix.
		private Vector2 _viewDimensions = Vector2.Zero;					// View dimensions.
		private float _maxDepth = 0.0f;									// Maximum depth.
		private GorgonSprite _cameraIcon = null;						// Camera icon.
		private float _angle = 0.0f;									// Angle of rotation.
		private Vector2 _scale = new Vector2(1.0f);						// Scale.
		private Vector2 _position = Vector2.Zero;						// Position.
		private Vector2 _anchor = Vector2.Zero;							// Target position.
		private bool _needsProjectionUpdate = true;						// Flag to indicate that the projection matrix needs updating.
		private bool _needsViewUpdate = true;							// Flag to indicate that the view matrix needs updating.
		private Matrix _viewProjection = Matrix.Identity;				// View/projection matrix.
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
		/// Property to set or return the view dimensions for the camera.
		/// </summary>
		public Vector2 ViewDimensions
		{
			get
			{
				return _viewDimensions;
			}
			set
			{
				_viewDimensions = value;
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
					value = 1.0f;
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
				if (_angle != value)
				{
					_angle = value;
					_needsViewUpdate = true;
				}
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
				if (value != _position)
				{
					_position = value;
					_needsViewUpdate = true;
				}
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
				if (value != _scale)
				{
					_scale = value;
					_needsViewUpdate = true;
				}
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
				if (_anchor != value)
				{
					_anchor = value;
					_needsViewUpdate = true;
				}
			}
		}		
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the view matrix.
		/// </summary>
		private void UpdateViewMatrix()
		{
			Matrix center = Matrix.Identity;						// Centering matrix.
			Matrix scale = Matrix.Identity;							// Scaling matrix.
			Matrix rotation = Matrix.Identity;						// Rotation matrix.
			Matrix translation = Matrix.Identity;					// Translation matrix.

			// Anchor the view.
			Matrix.Translation(_anchor.X, _anchor.Y, 0.0f, out center);

			// Scale it.
			if ((_scale.X != 1.0f) || (_scale.Y != 1.0f))
			{
				center.M11 = _scale.X;
				center.M22 = _scale.Y;
				center.M33 = 1.0f;
			}
						
			if (_angle != 0.0f)
			{
				Matrix.RotationZ(_angle.Radians(), out rotation);
				Matrix.Multiply(ref rotation, ref center, out center);
			}

			Matrix.Translation(_position.X, _position.Y, 0.0f, out translation);
			Matrix.Multiply(ref translation, ref center, out center);

			_view = center;
		}

		/// <summary>
		/// Function to update the camera if necessary.
		/// </summary>
		public void Update()
		{
			if (_needsProjectionUpdate)
				CalculateProjectionMatrix();
			else
			{
				if (_needsViewUpdate)
					UpdateViewMatrix();
			}

			if ((_needsProjectionUpdate) || (_needsViewUpdate))
				Matrix.Multiply(ref _view, ref _projection, out _viewProjection);

			_needsProjectionUpdate = false;
			_needsViewUpdate = false;
		}

		/// <summary>
		/// Function to draw the camera icon.
		/// </summary>
		public void Draw()
		{
			if ((_scale.X != 0.0f) && (_scale.Y != 0.0f))
				_cameraIcon.Scale = new Vector2(1.0f / _scale.X, 1.0f / _scale.Y);

			// Highlight current camera.
			if (Gorgon2D.Camera == this)
				_cameraIcon.Color = Color.Green;
			else
				_cameraIcon.Color = Color.White;

			_cameraIcon.Position = -Position;
			_cameraIcon.Angle = -Angle;
			_cameraIcon.Draw();
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
			_cameraIcon = new GorgonSprite(gorgon2D, "GorgonCamera.OrthoIcon", new GorgonSpriteSettings() 
				{
					Size = new Vector2(64, 50),
					Texture = gorgon2D.Icons,
					TextureRegion = new RectangleF(65, 0, 65, 50),
					Anchor = new Vector2(32.5f, 25),
					InitialScale = new Vector2(1.0f),
					Color = Color.White
				});
		}
		#endregion

		#region ICamera Members
		#region Properties.
		/// <summary>
		/// Property to return the pre-multiplied projection/view matrix.
		/// </summary>
		public Matrix ViewProjection
		{
			get
			{
				return _viewProjection;
			}
		}

		/// <summary>
		/// Property to return whether the view matrix needs updating.
		/// </summary>
		public bool NeedsViewUpdate
		{
			get
			{
				return _needsViewUpdate;
			}
		}

		/// <summary>
		/// Property to return whether the projection matrix needs updating.
		/// </summary>
		public bool NeedsProjectionUpdate
		{
			get
			{
				return _needsProjectionUpdate;
			}
		}

		/// <summary>
		/// Property to return the projection matrix for the camera.
		/// </summary>
		public SlimMath.Matrix Projection
		{
			get 
			{
				return _projection;
			}
		}

		/// <summary>
		/// Property to return the view matrix for the camera.
		/// </summary>
		public SlimMath.Matrix View
		{
			get 
			{
				return _view;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to calculate the projection matrix.
		/// </summary>
		public void CalculateProjectionMatrix()
		{
			Matrix.OrthoOffCenterLH(0.0f, _viewDimensions.X, _viewDimensions.Y, 0.0f, 0.0f, _maxDepth, out _projection);
			if (_needsViewUpdate)
				UpdateViewMatrix();
		}

		/// <summary>
		/// Function to update the projection matrix from the current target.
		/// </summary>
		/// <param name="target">Target to use when updating.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="target"/> parameter is NULL (Nothing in VB.Net).</exception>
		public void UpdateFromTarget(GorgonRenderTarget target)
		{
			_viewDimensions = new Vector2(target.Settings.Width, target.Settings.Height);
			_needsProjectionUpdate = true;
			_needsViewUpdate = true;
		}
		#endregion
		#endregion
	}
}
