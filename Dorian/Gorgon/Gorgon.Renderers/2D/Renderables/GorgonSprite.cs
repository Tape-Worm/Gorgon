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
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// The corners of a rectangle.
	/// </summary>
	public enum RectangleCorner
	{
		/// <summary>
		/// Upper left hand corner of the rectangle.
		/// </summary>
		/// <remarks>This equates to vertex #0 in a sprite.</remarks>
		UpperLeft = 0,
		/// <summary>
		/// Upper right hand corner of the sprite.
		/// </summary>
		/// <remarks>This equates to vertex #1 in a sprite.</remarks>
		UpperRight = 1,
		/// <summary>
		/// Lower left hand corner of the sprite.
		/// </summary>
		/// <remarks>This equates to vertex #2 in a sprite.</remarks>
		LowerLeft = 2,
		/// <summary>
		/// Lower right hand corner of the sprite.
		/// </summary>
		/// <remarks>This equates to vertex #3 in a sprite.</remarks>
		LowerRight = 3
	}

	/// <summary>
	/// A sprite object.
	/// </summary>
	public class GorgonSprite
		: GorgonMoveable, IDeferredTextureLoad, I2DCollisionObject
	{
		#region Variables.
		private float[] _corners = new float[4];										// Corners for the sprite.
		private string _textureName = string.Empty;										// Name of the texture for the sprite.
		private Vector2[] _offsets = null;												// A list of vertex offsets.
		private bool _horizontalFlip = false;											// Flag to indicate that the sprite is flipped horizontally.
		private bool _verticalFlip = false;												// Flag to indicate that the sprite is flipped vertically.
		private bool _needsColliderUpdate = false;										// Flag to indicate that the collider needs to be updated.
		private I2DCollider _collider = null;											// Collider for the sprite.
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

		/// <summary>
		/// Property to set or return whether the sprite texture is flipped horizontally.
		/// </summary>
		/// <remarks>This only flips the texture region mapped to the sprite.  It does not affect the positioning or axis of the sprite.</remarks>
		public bool HorizontalFlip
		{
			get
			{
				return _horizontalFlip;
			}
			set
			{
				if (value == _horizontalFlip)
					return;

				_horizontalFlip = value;
				NeedsTextureUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the sprite texture is flipped vertically.
		/// </summary>
		/// <remarks>This only flips the texture region mapped to the sprite.  It does not affect the positioning or axis of the sprite.</remarks>
		public bool VerticalFlip
		{
			get
			{
				return _verticalFlip;
			}
			set
			{
				if (value == _verticalFlip)
					return;

				_verticalFlip = value;
				NeedsTextureUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return a texture for the renderable.
		/// </summary>
		public override GorgonTexture2D Texture
		{
			get
			{
				return base.Texture;
			}
			set
			{
				if (value != base.Texture)
				{
					base.Texture = value;

					// Assign the texture name.
					if (Texture != null)
						_textureName = Texture.Name;
					else
						_textureName = string.Empty;


					NeedsTextureUpdate = true;
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to transform the vertices.
		/// </summary>
		protected override void TransformVertices()
		{
			float posX1;		// Horizontal position 1.
			float posX2;		// Horizontal position 2.
			float posY1;		// Vertical position 1.
			float posY2;		// Vertical position 2.			

			posX1 = _corners[0];
			posX2 = _corners[2];
			posY1 = _corners[1];
			posY2 = _corners[3];

			// Scale horizontally if necessary.
			if (Scale.X != 1.0f)
			{
				posX1 *= Scale.X;
				posX2 *= Scale.X;
			}

			// Scale vertically.
			if (Scale.Y != 1.0f)
			{
				posY1 *= Scale.Y;
				posY2 *= Scale.Y;
			}

			// Calculate rotation if necessary.
			if (Angle != 0.0f)
			{
				float angle = Angle.Radians();						// Angle in radians.
				float cosVal = angle.Cos();							// Cached cosine.
				float sinVal = angle.Sin();							// Cached sine.

				Vertices[0].Position.X = (posX1 * cosVal - posY1 * sinVal);
				Vertices[0].Position.Y = (posX1 * sinVal + posY1 * cosVal);

				Vertices[1].Position.X = (posX2 * cosVal - posY1 * sinVal);
				Vertices[1].Position.Y = (posX2 * sinVal + posY1 * cosVal);

				Vertices[2].Position.X = (posX1 * cosVal - posY2 * sinVal);
				Vertices[2].Position.Y = (posX1 * sinVal + posY2 * cosVal);

				Vertices[3].Position.X = (posX2 * cosVal - posY2 * sinVal);
				Vertices[3].Position.Y = (posX2 * sinVal + posY2 * cosVal);
			}
			else
			{
				Vertices[0].Position.X = posX1;
				Vertices[0].Position.Y = posY1;
				Vertices[1].Position.X = posX2;
				Vertices[1].Position.Y = posY1;
				Vertices[2].Position.X = posX1;
				Vertices[2].Position.Y = posY2;
				Vertices[3].Position.X = posX2;
				Vertices[3].Position.Y = posY2;
			}

			// Translate.
			if (Position.X != 0.0f)
			{
				Vertices[0].Position.X += Position.X;
				Vertices[1].Position.X += Position.X;
				Vertices[2].Position.X += Position.X;
				Vertices[3].Position.X += Position.X;
			}

			if (Position.Y != 0.0f)
			{
				Vertices[0].Position.Y += Position.Y;
				Vertices[1].Position.Y += Position.Y;
				Vertices[2].Position.Y += Position.Y;
				Vertices[3].Position.Y += Position.Y;
			}

			// Apply depth to the sprite.
			if (Depth != 0.0f)
			{
				Vertices[0].Position.Z = Depth;
				Vertices[1].Position.Z = Depth;
				Vertices[2].Position.Z = Depth;
				Vertices[3].Position.Z = Depth;
			}

			Vertices[0].Position.X += _offsets[0].X;
			Vertices[0].Position.Y += _offsets[0].Y;
			Vertices[1].Position.X += _offsets[1].X;
			Vertices[1].Position.Y += _offsets[1].Y;
			Vertices[2].Position.X += _offsets[2].X;
			Vertices[2].Position.Y += _offsets[2].Y;
			Vertices[3].Position.X += _offsets[3].X;
			Vertices[3].Position.Y += _offsets[3].Y;

			// Flag to indicate that the collider needs to be updated.
			_needsColliderUpdate = true;
		}

		/// <summary>
		/// Function to update the texture coordinates.
		/// </summary>
		protected override void UpdateTextureCoordinates()
		{
			// Calculate texture coordinates.
			Vector2 rightBottom = Vector2.Zero;
			Vector2 leftTop = Vector2.Zero;

			if (Texture == null)
			{
				Vertices[0].UV = Vertices[1].UV = Vertices[2].UV = Vertices[3].UV = Vector2.Zero;
				return;
			}

			if (!_horizontalFlip)
			{
				leftTop.X = TextureRegion.Left;
				rightBottom.X = TextureRegion.Right;
			}
			else
			{
				leftTop.X = TextureRegion.Right;
				rightBottom.X = TextureRegion.Left;
			}

			if (!_verticalFlip)
			{
				leftTop.Y = TextureRegion.Top;
				rightBottom.Y = TextureRegion.Bottom;
			}
			else
			{
				leftTop.Y = TextureRegion.Bottom;
				rightBottom.Y = TextureRegion.Top;
			}
			
			Vertices[0].UV = leftTop;
			Vertices[1].UV = new Vector2(rightBottom.X, leftTop.Y);
			Vertices[2].UV = new Vector2(leftTop.X, rightBottom.Y);
			Vertices[3].UV = rightBottom;
		}

		/// <summary>
		/// Function to update the vertices for the renderable.
		/// </summary>
		protected override void UpdateVertices()
		{
			_corners[0] = -Anchor.X;
			_corners[1] = -Anchor.Y;
			_corners[2] = Size.X - Anchor.X;
			_corners[3] = Size.Y - Anchor.Y;
		}

		/// <summary>
		/// Function to set an offset for a corner.
		/// </summary>
		/// <param name="corner">Corner of the sprite to set.</param>
		/// <param name="offset">Offset for the corner.</param>
		public void SetCornerOffset(RectangleCorner corner, Vector2 offset)
		{
			int index = (int)corner;

			if (_offsets[index] != offset)
			{
				_offsets[index] = offset;
				NeedsVertexOffsetUpdate = true;
			}
		}

		/// <summary>
		/// Function to retrieve an offset for a corner.
		/// </summary>
		/// <param name="corner">Corner of the sprite to retrieve the offset from.</param>
		/// <returns>The offset of the corner.</returns>
		public Vector2 GetCornerOffset(RectangleCorner corner)
		{
			return _offsets[(int)corner];
		}

		/// <summary>
		/// Function to set the color for a specific corner on the sprite.
		/// </summary>
		/// <param name="corner">Corner of the sprite to set.</param>
		/// <param name="color">Color to set.</param>
		public void SetCornerColor(RectangleCorner corner, GorgonColor color)
		{
			Vertices[(int)corner].Color = color;
		}

		/// <summary>
		/// Function to retrieve the color for a specific corner on the sprite.
		/// </summary>
		/// <param name="corner">Corner of the sprite to retrieve the color from.</param>
		/// <returns>The color on the specified corner of the sprite.</returns>
		public GorgonColor GetCornerColor(RectangleCorner corner)
		{
			return Vertices[(int)corner].Color;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSprite"/> class.
		/// </summary>
		/// <param name="gorgon2D">The interface that owns this object.</param>
		/// <param name="name">Name of the sprite.</param>
		/// <param name="settings">Settings for the sprite.</param>
		internal GorgonSprite(Gorgon2D gorgon2D, string name, GorgonSpriteSettings settings)
			: base(gorgon2D, name)
		{
			InitializeVertices(4);

			Size = settings.Size;
			Color = settings.Color;
			Angle = settings.InitialAngle;
			// Ensure scale is not set to 0.
			if (settings.InitialScale.X == 0.0f)
				settings.InitialScale.X = 1.0f;
			if (settings.InitialScale.Y == 0.0f)
				settings.InitialScale.Y = 1.0f;
			Scale = settings.InitialScale;
			Position = settings.InitialPosition;
			Texture = settings.Texture;
			TextureRegion = settings.TextureRegion;
			Anchor = settings.Anchor;

			_offsets = new [] { 
				Vector2.Zero, 
				Vector2.Zero, 
				Vector2.Zero, 
				Vector2.Zero, 
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

		#region I2DCollisionObject Members
		/// <summary>
		/// Property to set or return the collider that is assigned to the object.
		/// </summary>
		public I2DCollider Collider
		{
			get
			{
				return _collider;
			}
			set
			{
				if (value == _collider)
					return;

				if (value == null)
				{
					if (_collider != null)
						_collider.CollisionObject = null;

					return;
				}
				
				value.CollisionObject = this;
				NeedsColliderUpdate = true;
				value.UpdateFromCollisionObject();
			}
		}

		/// <summary>
		/// Property to set or return the collider that is assigned to the object.
		/// </summary>
		I2DCollider I2DCollisionObject.Collider
		{
			get
			{
				return _collider;
			}
			set
			{
				_collider = value;
			}
		}

		/// <summary>
		/// Property to return a list of vertices to render.
		/// </summary>
		Gorgon2DVertex[] I2DCollisionObject.Vertices
		{
			get 
			{
				return this.Vertices;
			}
		}

		/// <summary>
		/// Property to return whether the collider needs to be updated.
		/// </summary>
		public bool NeedsColliderUpdate
		{
			get
			{
				if (Collider == null)
					return false;

				return _needsColliderUpdate;
			}
			set
			{
				if (Collider != null)
					_needsColliderUpdate = value;
			}
		}
		#endregion
	}
}
