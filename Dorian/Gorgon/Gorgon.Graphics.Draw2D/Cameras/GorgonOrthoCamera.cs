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

namespace GorgonLibrary.Graphics.Renderers
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
		private RectangleF _viewDimensions = RectangleF.Empty;			// View dimensions.
		private float _maxDepth = 0.0f;									// Maximum depth.
		private GorgonSprite _cameraIcon = null;						// Camera icon.
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
		public RectangleF ViewDimensions
		{
			get
			{
				return _viewDimensions;
			}
			set
			{
				_viewDimensions = value;
				CalculateProjectionMatrix();
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
				CalculateProjectionMatrix();
			}
		}
		#endregion

		#region Methods.
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonOrthoCamera"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="viewDimensions">The view dimensions.</param>
		/// <param name="maximumDepth">The maximum depth.</param>
		internal GorgonOrthoCamera(Gorgon2D gorgon2D, string name, RectangleF viewDimensions, float maximumDepth)
			: base(name)
		{
			Gorgon2D = gorgon2D;
			_maxDepth = maximumDepth;
			_viewDimensions = viewDimensions;
			_cameraIcon = new GorgonSprite(gorgon2D, "GorgonCamera.OrthoIcon", 64, 50);
			_cameraIcon.Texture = gorgon2D.Icons;
			_cameraIcon.TextureRegion = new RectangleF(0, 0, 64, 50);
		}
		#endregion

		#region ICamera Members
		#region Properties.
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
			Matrix.OrthoOffCenterLH(0, _viewDimensions.Width, _viewDimensions.Height, 0.0f, 0.0f, _maxDepth, out _projection);
		}

		/// <summary>
		/// Function to update the projection matrix from the current target.
		/// </summary>
		/// <param name="target">Target to use when updating.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="target"/> parameter is NULL (Nothing in VB.Net).</exception>
		public void UpdateFromTarget(GorgonSwapChain target)
		{
			CalculateProjectionMatrix();
		}
		#endregion
		#endregion
	}
}
