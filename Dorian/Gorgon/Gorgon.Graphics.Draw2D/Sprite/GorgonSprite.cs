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
// Created: Thursday, February 16, 2012 9:19:30 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMath;
using GorgonLibrary.Math;

namespace GorgonLibrary.Graphics.Renderers
{
	/// <summary>
	/// A sprite object.
	/// </summary>
	public class GorgonSprite
		: GorgonNamedObject
	{
		#region Variables.
		private GorgonTexture2D _texture = null;										// Texture for the sprite.
		private Gorgon2D.Vertex[] _vertices = new Gorgon2D.Vertex[4];					// Vertices for the sprite.
		private Gorgon2D.Vertex[] _transformedVertices = new Gorgon2D.Vertex[4];		// Vertices for the sprite.
		private Vector3 _angle = Vector3.Zero;											// Angle of rotation.
		private Vector3 _position = Vector3.Zero;										// Position of the sprite.
		private Vector2 _scale = new Vector2(1);										// Scale for the sprite.
		private Vector4 _anchor = Vector3.Zero;											// Anchor point.
		private Vector2 _size = Vector2.Zero;											// Size of the sprite.
		private Vector2 _textureSize = Vector2.Zero;									// Size of the sprite on the texture.
		private Vector2 _textureOffset = Vector2.Zero;									// Offset of the sprite on the texture.
		private Matrix _translation = Matrix.Identity;									// Translation matrix.
		private Matrix _rotation = Matrix.Identity;										// Rotation matrix.
		private Matrix _scaling = Matrix.Identity;										// Scaling matrix.
		private Matrix _worldMatrix = Matrix.Identity;									// World matrix for the sprite.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the interface that owns this object.
		/// </summary>
		public Gorgon2D Gorgon2D
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the position of the sprite.
		/// </summary>
		public Vector3 Position
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of the sprite.
		/// </summary>
		public Vector2 Size
		{
			get
			{
				return _size;
			}
			set
			{
				_size = value;
				CreateVertices();
			}
		}

		/// <summary>
		/// Property to set or return the offset in the texture for the sprite.
		/// </summary>
		public Vector2 TextureOffset
		{
			get
			{
				return _textureOffset;
			}
			set
			{
				_textureOffset = value;
				CreateVertices();
			}
		}

		/// <summary>
		/// Property to set or return the size on the texture that the sprite will cover.
		/// </summary>
		public Vector2 TextureSize
		{
			get
			{
				return _textureSize;
			}
			set
			{
				_textureSize = value;
				CreateVertices();
			}
		}

		/// <summary>
		/// Property to set or return the texture for the sprite.
		/// </summary>
		public GorgonTexture2D Texture
		{
			get
			{
				return _texture;
			}
			set
			{
				_texture = value;
				if (value != null)
				{
					TextureOffset = Vector2.Zero;
					TextureSize = new Vector2(_texture.Settings.Width, _texture.Settings.Height);
				}
				else
				{
					TextureOffset = Vector2.Zero;
					TextureSize = Vector2.Zero;
				}

				CreateVertices();
			}
		}

		/// <summary>
		/// Property to set or return the angle of rotation (in degrees) for a given axis.
		/// </summary>
		public Vector3 Angle
		{
			get
			{
				return _angle;
			}
			set
			{
				_angle = value;
				TransformVertices();
			}
		}

		/// <summary>
		/// Property to set or return the scale of the sprite.
		/// </summary>
		public Vector2 Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scale = value;
				TransformVertices();
			}
		}

		/// <summary>
		/// Property to set or return the anchor point of the sprite.
		/// </summary>
		public Vector3 Anchor
		{
			get
			{
				return (Vector3)_anchor;
			}			
			set
			{
				_anchor = value;
				TransformVertices();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create the vertices for the sprite.
		/// </summary>
		private void CreateVertices()
		{
			// Calculate texture coordinates.
			Vector2 scaleUV = Vector2.Zero;
			Vector2 offsetUV = Vector2.Zero;

			if (Texture != null)
			{
				if (_textureSize.X > 0)
				{
					offsetUV.X = TextureOffset.X / TextureSize.X;
					scaleUV.X = (TextureOffset.X + Size.X) / TextureSize.X;
				}

				if (_textureSize.Y > 0)
				{
					offsetUV.Y = TextureOffset.Y / TextureSize.Y;
					scaleUV.Y = (TextureOffset.Y + Size.Y) / TextureSize.Y;
				}
			}
			
			// Set up vertices.
			_vertices[0] = new Gorgon2D.Vertex(new Vector4(0, 0, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), offsetUV);
			_vertices[1] = new Gorgon2D.Vertex(new Vector4(Size.X, 0, 0.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f), new Vector2(scaleUV.X, offsetUV.Y));
			_vertices[2] = new Gorgon2D.Vertex(new Vector4(0, Size.Y, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), new Vector2(offsetUV.X, scaleUV.Y));
			_vertices[3] = new Gorgon2D.Vertex(new Vector4(Size.X, Size.Y, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), scaleUV);
		}

		/// <summary>
		/// Function to transform the vertices.
		/// </summary>
		private void TransformVertices()
		{
			Vector4 anchoredPosition = Vector4.Zero;

			if (!GorgonMathUtility.EqualVector3(_angle, Vector3.Zero))
			{
				Quaternion rotationAngle = Quaternion.Identity;

				Quaternion.RotationYawPitchRoll(GorgonMathUtility.Radians(Angle.Y), GorgonMathUtility.Radians(Angle.X), GorgonMathUtility.Radians(Angle.Z), out rotationAngle);
				Matrix.RotationQuaternion(ref rotationAngle, out _rotation);
			}
			else
				_rotation = Matrix.Identity;

			_worldMatrix = _rotation;

			if (!GorgonMathUtility.EqualVector2(_scale, new Vector2(1.0f)))
			{
				_scaling.ScaleVector = new Vector3(Scale, 1.0f);
				Matrix.Multiply(ref _scaling, ref _rotation, out _worldMatrix);
			}
			else
				_scaling = Matrix.Identity;

			_translation.TranslationVector = Position;
			Matrix.Multiply(ref _worldMatrix, ref _translation, out _worldMatrix);

			for (int i = 0; i < _vertices.Length; i++)
			{
				_transformedVertices[i] = _vertices[i];

				if (_anchor != Vector4.Zero)
					Vector4.Subtract(ref _transformedVertices[i].Position, ref _anchor, out anchoredPosition);
				Vector4.Transform(ref anchoredPosition, ref _worldMatrix, out _transformedVertices[i].Position);
				if (_anchor != Vector4.Zero)
					Vector4.Add(ref _anchor, ref _transformedVertices[i].Position, out _transformedVertices[i].Position);
			}
		}

		/// <summary>
		/// Function to draw the sprite.
		/// </summary>
		public void Draw()
		{			
			if (Gorgon2D.Shaders.Current.Textures[0] != Texture)
			{
				if (Texture != null)
				{
					Gorgon2D.Shaders.Current = null;
					Gorgon2D.Shaders.Current.Textures[0] = Texture;
					Gorgon2D.Shaders.Current.Samplers[0] = GorgonTextureSamplerStates.DefaultStates;					
				}
				else
					Gorgon2D.Shaders.Current.Textures[0] = null;

				Gorgon2D.Shaders.UpdateShaders();
			}

			TransformVertices();
			Gorgon2D.AddVertices(_transformedVertices);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSprite"/> class.
		/// </summary>
		/// <param name="gorgon2D">The interface that owns this object.</param>
		/// <param name="name">The name of the sprite.</param>
		/// <param name="width">The width of the sprite.</param>
		/// <param name="height">The height of the sprite.</param>
		internal GorgonSprite(Gorgon2D gorgon2D, string name, float width, float height)
			: base(name)
		{
			Gorgon2D = gorgon2D;
			Size = new Vector2(width, height);
			TextureSize = Size;			
			CreateVertices();
		}
		#endregion
	}
}
