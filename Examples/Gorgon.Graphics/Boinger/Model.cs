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
using System;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Graphics.Example
{
	/// <summary>
	/// The base model object.
	/// </summary>
	abstract class Model
		: IDisposable
	{
		#region Variables.
		// Our world matrix.
		private DX.Matrix _worldMatrix = DX.Matrix.Identity;
		// Our position matrix.
		private DX.Matrix _positionMatrix = DX.Matrix.Identity;         
		// Our scale matrix.
		private DX.Matrix _scaleMatrix = DX.Matrix.Identity;
		// Our rotation matrix.
		private DX.Matrix _rotationMatrix = DX.Matrix.Identity;
		// Our position.
		private DX.Vector3 _position = DX.Vector3.Zero;                 
		// Our scale.
		private DX.Vector3 _scale = DX.Vector3.One;
		// Our rotation.
		private DX.Vector3 _rotation = DX.Vector3.Zero;
		// Flag to indicate that our position has been updated.	
		private bool _isPositionChanged;
		// Flag to indicate that our scale has been updated.
		private bool _isScaleChanged;
		// Flag to inidcate that our rotation has been updated.
		private bool _isRotationChanged;					        
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the world matrix.
		/// </summary>
		protected DX.Matrix WorldMatrix => _worldMatrix;

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
		/// Property to return the draw call for this model.
		/// </summary>
		protected GorgonDrawIndexedCall DrawCall
		{
			get;
		} = new GorgonDrawIndexedCall();

		/// <summary>
		/// Property to return the resources on the pipeline for this object.
		/// </summary>
		public GorgonPipelineResources Resources => DrawCall.Resources;

		/// <summary>
		/// Property to set or return the world position of the object.
		/// </summary>
		public DX.Vector3 Position
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
		public DX.Vector3 Scale
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
		public DX.Vector3 Rotation
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
                    DX.Quaternion quatRotation;		// Quaternion for rotation.

				    // Convert degrees to radians.
					var rotRads = new DX.Vector3(_rotation.X.ToRadians(), _rotation.Y.ToRadians(), _rotation.Z.ToRadians());
				    
				    DX.Quaternion.RotationYawPitchRoll(rotRads.Y, rotRads.X, rotRads.Z, out quatRotation);
					DX.Matrix.RotationQuaternion(ref quatRotation, out _rotationMatrix);
				}

				DX.Matrix temp;

				// Build our world matrix.
				DX.Matrix.Multiply(ref _scaleMatrix, ref _rotationMatrix, out temp);
				DX.Matrix.Multiply(ref temp, ref _positionMatrix, out _worldMatrix);
			}

			Program.UpdateWVP(ref _worldMatrix);
		}		

		/// <summary>
		/// Function to draw the model.
		/// </summary>
		/// <param name="viewPort">The viewport to draw into.</param>
		/// <param name="state">The pipeline state to apply when drawing.</param>
		public abstract void Draw(DX.ViewportF[] viewPort, GorgonPipelineState state);

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		public void Dispose()
		{
			VertexBuffer?.Dispose();
			IndexBuffer?.Dispose();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Model" /> class.
		/// </summary>
		protected Model()
		{
			Scale = new DX.Vector3(1.0f);
		}
		#endregion
	}
}
