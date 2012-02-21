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
	/// The corners of a sprite.
	/// </summary>
	public enum SpriteCorner
	{
		/// <summary>
		/// Upper left hand corner of the sprite.
		/// </summary>
		/// <remarks>This equates to vertex #0 in the sprite.</remarks>
		UpperLeft = 0,
		/// <summary>
		/// Upper right hand corner of the sprite.
		/// </summary>
		/// <remarks>This equates to vertex #1 in the sprite.</remarks>
		UpperRight = 1,
		/// <summary>
		/// Lower left hand corner of the sprite.
		/// </summary>
		/// <remarks>This equates to vertex #2 in the sprite.</remarks>
		LowerLeft = 2,
		/// <summary>
		/// Lower right hand corner of the sprite.
		/// </summary>
		/// <remarks>This equates to vertex #3 in the sprite.</remarks>
		LowerRight = 3
	}

	/// <summary>
	/// A sprite object.
	/// </summary>
	public class GorgonSprite
		: GorgonMoveable, IDeferredTextureLoad
	{
		#region Variables.
		private string _textureName = string.Empty;										// Name of the texture for the sprite.
		private Matrix _rotation = Matrix.Identity;										// Rotation matrix.
		private Matrix _scaling = Matrix.Identity;										// Scaling matrix.
		private Matrix _worldMatrix = Matrix.Identity;									// World matrix for the sprite.
		private Vector4[] _offsets = null;												// A list of vertex offsets.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the vertices need to be updated due to an offset.
		/// </summary>
		protected bool NeedsVertexOffsetUpdate
		{
			get;
			set;
		}
		
		/// <summary>
		/// Property to return the number of indices that make up this renderable.
		/// </summary>
		protected internal override int IndexCount
		{
			get 
			{
				return 6;
			}
		}

		/// <summary>
		/// Property to return the type of primitive for the renderable.
		/// </summary>
		protected internal override PrimitiveType PrimitiveType
		{
			get 
			{
				return Graphics.PrimitiveType.TriangleList;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to transform the vertices.
		/// </summary>
		protected override void TransformVertices()
		{
			Vector4 anchoredPosition = Vector4.Zero;
			Vector4 anchor = new Vector4(Anchor, 0.0f);

			if (NeedsRotation)
			{
				Quaternion rotationAngle = Quaternion.Identity;

				Quaternion.RotationYawPitchRoll(GorgonMathUtility.Radians(Angle.Y), GorgonMathUtility.Radians(Angle.X), GorgonMathUtility.Radians(Angle.Z), out rotationAngle);
				Matrix.RotationQuaternion(ref rotationAngle, out _rotation);
			}
						
			if (NeedsScaling)
				_scaling.ScaleVector = new Vector3(Scale, 1.0f);

			if ((NeedsScaling) || (NeedsRotation))
				Matrix.Multiply(ref _scaling, ref _rotation, out _worldMatrix);

			_worldMatrix.TranslationVector = Position;

			for (int i = 0; i < Vertices.Length; i++)
			{
				Gorgon2D.Vertex transformedVertex = Vertices[i];
				
				if (!anchor.Equals(Vector4.Zero))
					Vector4.Subtract(ref transformedVertex.Position, ref anchor, out transformedVertex.Position);

				Vector4.Transform(ref transformedVertex.Position, ref _worldMatrix, out transformedVertex.Position);

				if (NeedsVertexOffsetUpdate)
					Vector4.Add(ref _offsets[i], ref transformedVertex.Position, out transformedVertex.Position);

				TransformedVertices[i] = transformedVertex;
			}

			NeedsVertexOffsetUpdate = false;
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

			if ((TextureScale.X != 1.0f) || (TextureScale.Y != 1.0f))
				scaledTexture = Vector2.Modulate(new Vector2(Texture.Settings.Width, Texture.Settings.Height), TextureScale);
			else
				scaledTexture = new Vector2(Texture.Settings.Width, Texture.Settings.Height);

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
			Vertices[0].Position = new Vector4(0, 0, 0.0f, 1.0f);
			Vertices[1].Position = new Vector4(Size.X, 0, 0.0f, 1.0f);
			Vertices[2].Position = new Vector4(0, Size.Y, 0.0f, 1.0f);
			Vertices[3].Position = new Vector4(Size, 0.0f, 1.0f);
		}

		/// <summary>
		/// Function to set an offset for a vertex.
		/// </summary>
		/// <param name="corner">Corner of the sprite to set.</param>
		/// <param name="offset">Offset for the vertex.</param>
		public void SetVertexOffset(SpriteCorner corner, Vector3 offset)
		{
			int index = (int)corner;
			Vector4 vector = new Vector4(offset , 0.0f);

			if (_offsets[index] != vector)
			{
				_offsets[(int)corner] = vector;
				NeedsVertexOffsetUpdate = true;
			}
		}

		/// <summary>
		/// Function to set the color for a specific vertex on the sprite.
		/// </summary>
		/// <param name="corner">Corner of the sprite to set.</param>
		/// <param name="color">Color to set.</param>
		public void SetVertexColor(SpriteCorner corner, GorgonColor color)
		{
			Vertices[(int)corner].Color = color;
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

			_offsets = new [] { 
				Vector4.Zero, 
				Vector4.Zero, 
				Vector4.Zero, 
				Vector4.Zero, 
			};
		}
		#endregion

		#region IDeferredTextureLoad Members
		/// <summary>
		/// Property to return the name of the texture bound to this image.
		/// </summary>
		/// <remarks>This is used to defer the texture assignment until it the texture with the specified name is loaded.</remarks>
		public string DeferredTextureName
		{
			get
			{
				return _textureName;
			}
			set
			{
				if (value == null)
					value = string.Empty;

				if (string.Compare(_textureName, value, true) != 0)
				{
					_textureName = value;
					GetDeferredTexture();
				}
			}
		}

		/// <summary>
		/// Function to assign a deferred texture.
		/// </summary>
		/// <remarks>If there are multiple textures with the same name, then the first texture will be chosen.</remarks>
		public virtual void GetDeferredTexture()
		{
			if (string.IsNullOrEmpty(_textureName))
			{
				base.Texture = null;
				return;
			}

			// Look through the tracked objects in the graphics object.
			// FYI, LINQ is fucking awesome (if a little slow)...
			base.Texture = (from texture in Gorgon2D.Graphics.GetGraphicsObjectOfType<GorgonTexture2D>()
							where (texture != null) && (string.Compare(texture.Name, _textureName, true) == 0)
							select texture).FirstOrDefault();
			NeedsTextureUpdate = true;
		}
		#endregion
	}
}
