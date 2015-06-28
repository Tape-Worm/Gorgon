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
// Created: Sunday, December 30, 2012 2:35:20 PM
// 
#endregion

using Gorgon.Math;
using SlimMath;

namespace Gorgon.Graphics.Example
{
	/// <summary>
	/// The base model object.
	/// </summary>
	abstract class Model
	{
		#region Variables.
		private Matrix _worldMatrix = Matrix.Identity;				// Our world matrix.
		private Matrix _positionMatrix = Matrix.Identity;			// Our position matrix.
		private Matrix _scaleMatrix = Matrix.Identity;				// Our scale matrix.
		private Matrix _rotationMatrix = Matrix.Identity;			// Our rotation matrix.
		private Vector3 _position = Vector3.Zero;					// Our position.
		private Vector3 _scale = new Vector3(1);					// Our scale.
		private Vector3 _rotation = Vector3.Zero;					// Our rotation.
		private bool _isPositionChanged;					        // Flag to indicate that our position has been updated.
		private bool _isScaleChanged;						        // Flag to indicate that our scale has been updated.
		private bool _isRotationChanged;					        // Flag to inidcate that our rotation has been updated.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the world matrix.
		/// </summary>
		protected Matrix WorldMatrix
		{
			get
			{
				return _worldMatrix;
			}			
		}

		/// <summary>
		/// Property to set or return the vertices for our object.
		/// </summary>
		protected BoingerVertex[] Vertices
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the indices for our object.
		/// </summary>
		protected int[] Indices
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the vertex buffer for this object.
		/// </summary>
		protected GorgonVertexBuffer VertexBuffer
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the index buffer for this object.
		/// </summary>
		protected GorgonIndexBuffer IndexBuffer
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the world position of the object.
		/// </summary>
		public Vector3 Position
		{
			get
			{
				return _position;
			}
			set
			{
				if (value.Equals(_position))
				{
					return;
				}

				_position = value;
				_isPositionChanged = true;
			}
		}

		/// <summary>
		/// Property to set or return the scale of the object.
		/// </summary>
		public Vector3 Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				if (value.Equals(_scale))
				{
					return;
				}

				_scale = value;
				_isScaleChanged = true;
			}
		}

		/// <summary>
		/// Property to set or return the rotation of the object, in degrees.
		/// </summary>
		public Vector3 Rotation
		{
			get
			{
				return _rotation;
			}
			set
			{
				if (value.Equals(_rotation))
				{
					return;
				}

				_rotation = value;
				_isRotationChanged = true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the transformation matrix of the plane.
		/// </summary>
		protected void UpdateTransform()
		{
			// Don't recalculate unless we have changes.
			if ((_isPositionChanged) || (_isScaleChanged) || (_isRotationChanged))
			{
				if (_isPositionChanged)
				{
					_positionMatrix.TranslationVector = Position;
				}

				if (_isScaleChanged)
				{
					_scaleMatrix.ScaleVector = Scale;
				}

			    if (_isRotationChanged)
				{
                    Quaternion quatRotation;		// Quaternion for rotation.

				    // Convert degrees to radians.
					var rotRads = new Vector3(_rotation.X.ToRadians(), _rotation.Y.ToRadians(), _rotation.Z.ToRadians());
				    
				    Quaternion.RotationYawPitchRoll(rotRads.Y, rotRads.X, rotRads.Z, out quatRotation);
					Matrix.RotationQuaternion(ref quatRotation, out _rotationMatrix);
				}

				Matrix temp;

				// Build our world matrix.
				Matrix.Multiply(ref _scaleMatrix, ref _rotationMatrix, out temp);
				Matrix.Multiply(ref temp, ref _positionMatrix, out _worldMatrix);
			}

			Program.UpdateWVP(ref _worldMatrix);
		}		

		/// <summary>
		/// Function to draw the model.
		/// </summary>
		public abstract void Draw();
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Model" /> class.
		/// </summary>
		protected Model()
		{
			Scale = new Vector3(1.0f);
		}
		#endregion
	}
}
