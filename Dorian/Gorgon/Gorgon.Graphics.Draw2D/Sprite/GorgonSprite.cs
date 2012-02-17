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
		: GorgonRenderable
	{
		#region Variables.
		private Vector3 _angle = Vector3.Zero;											// Angle of rotation.
		private Vector3 _position = Vector3.Zero;										// Position of the sprite.
		private Vector2 _scale = new Vector2(1);										// Scale for the sprite.
		private Vector4 _anchor = Vector3.Zero;											// Anchor point.
		private Matrix _translation = Matrix.Identity;									// Translation matrix.
		private Matrix _rotation = Matrix.Identity;										// Rotation matrix.
		private Matrix _scaling = Matrix.Identity;										// Scaling matrix.
		private Matrix _worldMatrix = Matrix.Identity;									// World matrix for the sprite.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return an index buffer.
		/// </summary>
		protected internal override bool UseIndexBuffer
		{
			get 
			{
				return true;
			}
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

			for (int i = 0; i < Vertices.Length; i++)
			{
				Gorgon2D.Vertex transformedVertex = Vertices[i];				

				if (_anchor != Vector4.Zero)
					Vector4.Subtract(ref transformedVertex.Position, ref _anchor, out anchoredPosition);
				Vector4.Transform(ref anchoredPosition, ref _worldMatrix, out transformedVertex.Position);
				if (_anchor != Vector4.Zero)
					Vector4.Add(ref _anchor, ref transformedVertex.Position, out transformedVertex.Position);

				TransformedVertices[i] = transformedVertex;
			}
		}

		/// <summary>
		/// Function to update the texture coordinates.
		/// </summary>
		protected override void UpdateTextureCoordinates()
		{
			// Calculate texture coordinates.
			Vector2 scaleUV = Vector2.Zero;
			Vector2 offsetUV = Vector2.Zero;
			Vector2 scaledTexture = Vector2.Zero;

			if (Texture == null)
			{
				Vertices[0].UV = Vertices[1].UV = Vertices[2].UV = Vertices[3].UV = Vector2.Zero;
				return;
			}

			scaledTexture = Vector2.Modulate(new Vector2(Texture.Settings.Width, Texture.Settings.Height), TextureScale);

			offsetUV.X = TextureOffset.X / scaledTexture.X;
			scaleUV.X = (TextureOffset.X + Size.X) / scaledTexture.X;

			offsetUV.Y = TextureOffset.Y / scaledTexture.Y;
			scaleUV.Y = (TextureOffset.Y + Size.Y) / scaledTexture.Y;
			
			Vertices[0].UV = offsetUV;
			Vertices[1].UV = new Vector2(scaleUV.X, offsetUV.Y);
			Vertices[2].UV = new Vector2(offsetUV.X, scaleUV.Y);
			Vertices[3].UV = scaleUV;
		}

		/// <summary>
		/// Function to update the vertices for the renderable.
		/// </summary>
		protected override void UpdateVertices()
		{
			// Set up vertices.
			// TODO: Set only object space vertex position in here.   Put color into an UpdateColors() method.
			Vertices[0] = new Gorgon2D.Vertex(new Vector4(0, 0, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), Vertices[0].UV);
			Vertices[1] = new Gorgon2D.Vertex(new Vector4(Size.X, 0, 0.0f, 1.0f), new Vector4(0.0f, 1.0f, 1.0f, 1.0f), Vertices[1].UV);
			Vertices[2] = new Gorgon2D.Vertex(new Vector4(0, Size.Y, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f), Vertices[2].UV);
			Vertices[3] = new Gorgon2D.Vertex(new Vector4(Size.X, Size.Y, 0.0f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f), Vertices[3].UV);
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <remarks>Please note that this doesn't draw the object to the target right away, but queues it up to be
		/// drawn when <see cref="M:GorgonLibrary.Graphics.Renderers.Gorgon2D.Render">Render</see> is called.
		/// </remarks>
		public override void Draw()
		{			
			TransformVertices();

			base.Draw();
		}

		/// <summary>
		/// Function to clone this renderable object.
		/// </summary>
		/// <returns>The clone of the renderable object.</returns>
		public override GorgonRenderable Clone()
		{
			// TODO: Add clone.
			return null;
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
			: base(gorgon2D, name)
		{
			Size = new Vector2(width, height);
			InitializeVertices(4);
		}
		#endregion
	}
}
