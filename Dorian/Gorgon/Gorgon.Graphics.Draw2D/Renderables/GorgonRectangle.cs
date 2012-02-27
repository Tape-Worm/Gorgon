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
// Created: Saturday, February 25, 2012 9:38:55 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimMath;
using GorgonLibrary.Math;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics.Renderers
{
	/// <summary>
	/// A renderable object for drawing a rectangle on the screen.
	/// </summary>
	/// <remarks>This is similar to a sprite only that the object can be filled or unfilled.</remarks>
	public class GorgonRectangle
		: GorgonMoveable
	{
		#region Variables.
		private float[] _corners = null;												// Corners for the rectangle.
		private bool _isFilled = false;													// Flag to indicate that the rectangle is filled.
		private RectangleF _rectangle = RectangleF.Empty;								// Rectangle dimensions.
		private Vector2 _penSize = new Vector2(1.0f);									// Pen size for the outline rectangle.
		private GorgonColor[] _colors = null;											// List of colors for each corner.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the type of primitive for the renderable.
		/// </summary>
		protected internal override PrimitiveType PrimitiveType
		{
			get 
			{
				if ((!_isFilled) && (_penSize.X == 1.0f) && (_penSize.Y == 1.0f))
					return Graphics.PrimitiveType.LineList;
				else
					return Graphics.PrimitiveType.TriangleList;
			}
		}

		/// <summary>
		/// Property to return the number of indices that make up this renderable.
		/// </summary>
		protected internal override int IndexCount
		{
			get 
			{
				if ((!_isFilled) && (_penSize.X == 1.0f) && (_penSize.Y == 1.0f))
					return 0;
				else
				{
					if (!_isFilled)
						return 24;
					else
						return 6;
				}
			}
		}

		/// <summary>
		/// Property to set or return the index buffer for this renderable.
		/// </summary>
		protected internal override GorgonIndexBuffer IndexBuffer
		{
			get
			{
				if ((!_isFilled) && (_penSize.X == 1.0f) && (_penSize.Y == 1.0f))
					return null;
				else
					return Gorgon2D.DefaultIndexBuffer;
			}
		}

		/// <summary>
		/// Property to return whether the vertices need to be updated due to an offset.
		/// </summary>
		protected bool NeedsVertexOffsetUpdate
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the position of the sprite.
		/// </summary>
		public override Vector2 Position
		{
			get
			{
				return _rectangle.Location;
			}
			set
			{
				_rectangle.Location = value;
			}
		}

		/// <summary>
		/// Property to set or return the pen size for an unfilled rectangle.
		/// </summary>
		public Vector2 PenSize
		{
			get
			{
				return _penSize;
			}
			set
			{
				if (value != _penSize)
				{
					if (value.X < 1.0f)
						value.X = 1.0f;
					if (value.Y < 1.0f)
						value.Y = 1.0f;

					_penSize = value;
				}
			}
		}

		/// <summary>
		/// Property to set or return the size of the renderable.
		/// </summary>
		public override Vector2 Size
		{
			get
			{
				return _rectangle.Size;
			}
			set
			{
				if (value != (Vector2)_rectangle.Size)
				{
					_rectangle.Size = value;
					NeedsVertexUpdate = true;
					NeedsTextureUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the color for a renderable object.
		/// </summary>
		public override GorgonColor Color
		{
			get
			{
				return _colors[0];
			}
			set
			{
				_colors[0] = _colors[1] = _colors[2] = _colors[3] = value;
			}
		}

		/// <summary>
		/// Property to set or return the rectangle dimensions for this rectangle object.
		/// </summary>
		public RectangleF Rectangle
		{
			get
			{
				return _rectangle;
			}
			set
			{
				if (_rectangle != value)
				{
					_rectangle = value;
					NeedsTextureUpdate = true;
					NeedsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return whether the rectangle is filled or not.
		/// </summary>
		public bool IsFilled
		{
			get
			{
				return _isFilled;
			}
			set
			{
				if (value != _isFilled)
				{
					_isFilled = value;								
					NeedsTextureUpdate = true;
					NeedsVertexUpdate = true;
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to transform an unfilled rectangle with a larger pen size.
		/// </summary>
		private void TransformUnfilledQuads()
		{
			float posX1;							// Horizontal position 1.
			float posX2;							// Horizontal position 2.
			float posY1;							// Vertical position 1.
			float posY2;							// Vertical position 2.
			Vector2 midPen = Vector2.Zero;			// Pen mid point.

			BaseVertexCount = 0;
			VertexCount = 16;
			Vector2.Divide(ref _penSize, 2.0f, out midPen);

			for (int i = 0; i < 16; i+=4)
			{
				switch(i)
				{
					case 0:
						posX1 = _corners[0] - midPen.X;
						posX2 = _corners[2] + midPen.X;
						posY1 = _corners[1] - midPen.Y;
						posY2 = _corners[1] + midPen.Y;

						Vertices[i].Color = _colors[0];
						Vertices[i + 2].Color = _colors[0];
						Vertices[i + 1].Color = _colors[1];
						Vertices[i + 3].Color = _colors[1];
						break;
					case 4:
						posX1 = _corners[2] - midPen.X;
						posX2 = _corners[2] + midPen.X;
						posY1 = _corners[1] - midPen.Y;
						posY2 = _corners[3] + midPen.Y;

						Vertices[i].Color = _colors[1];
						Vertices[i + 1].Color = _colors[1];
						Vertices[i + 2].Color = _colors[2];
						Vertices[i + 3].Color = _colors[2];
						break;
					case 8:
						posX1 = _corners[0] - midPen.X;
						posX2 = _corners[2] + midPen.X;
						posY1 = _corners[3] - midPen.Y;
						posY2 = _corners[3] + midPen.Y;

						Vertices[i].Color = _colors[3];
						Vertices[i + 1].Color = _colors[2];
						Vertices[i + 2].Color = _colors[3];
						Vertices[i + 3].Color = _colors[2];
						break;
					default:
						posX1 = _corners[0] - midPen.X;
						posX2 = _corners[0] + midPen.X;
						posY1 = _corners[1] - midPen.Y;
						posY2 = _corners[3] + midPen.Y;

						Vertices[i].Color = _colors[0];
						Vertices[i + 1].Color = _colors[0];
						Vertices[i + 2].Color = _colors[3];
						Vertices[i + 3].Color = _colors[3];
						break;
				}

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
					float angle = GorgonMathUtility.Radians(Angle);		// Angle in radians.
					float cosVal = (float)System.Math.Cos(angle);		// Cached cosine.
					float sinVal = (float)System.Math.Sin(angle);		// Cached sine.

					Vertices[i].Position.X = (posX1 * cosVal - posY1 * sinVal);
					Vertices[i].Position.Y = (posX1 * sinVal + posY1 * cosVal);

					Vertices[i + 1].Position.X = (posX2 * cosVal - posY1 * sinVal);
					Vertices[i + 1].Position.Y = (posX2 * sinVal + posY1 * cosVal);

					Vertices[i + 2].Position.X = (posX1 * cosVal - posY2 * sinVal);
					Vertices[i + 2].Position.Y = (posX1 * sinVal + posY2 * cosVal);

					Vertices[i + 3].Position.X = (posX2 * cosVal - posY2 * sinVal);
					Vertices[i + 3].Position.Y = (posX2 * sinVal + posY2 * cosVal);
				}
				else
				{
					Vertices[i].Position.X = posX1;
					Vertices[i].Position.Y = posY1;
					Vertices[i + 1].Position.X = posX2;
					Vertices[i + 1].Position.Y = posY1;
					Vertices[i + 2].Position.X = posX1;
					Vertices[i + 2].Position.Y = posY2;
					Vertices[i + 3].Position.X = posX2;
					Vertices[i + 3].Position.Y = posY2;
				}

				// Translate.
				if (Position.X != 0.0f)
				{
					Vertices[i].Position.X += Position.X;
					Vertices[i + 1].Position.X += Position.X;
					Vertices[i + 2].Position.X += Position.X;
					Vertices[i + 3].Position.X += Position.X;
				}

				if (Position.Y != 0.0f)
				{
					Vertices[i].Position.Y += Position.Y;
					Vertices[i + 1].Position.Y += Position.Y;
					Vertices[i + 2].Position.Y += Position.Y;
					Vertices[i + 3].Position.Y += Position.Y;
				}

				// Apply depth to the sprite.
				if (Depth != 0.0f)
				{
					Vertices[i].Position.Z = Depth;
					Vertices[i + 1].Position.Z = Depth;
					Vertices[i + 2].Position.Z = Depth;
					Vertices[i + 3].Position.Z = Depth;
				}
			}
		}

		/// <summary>
		/// Function to transform an unfilled rectangle.
		/// </summary>
		private void TransformUnfilled()
		{
			float posX1;							// Horizontal position 1.
			float posX2;							// Horizontal position 2.
			float posY1;							// Vertical position 1.
			float posY2;							// Vertical position 2.

			BaseVertexCount = 8;
			VertexCount = 8;

			posX1 = _corners[0];
			posX2 = _corners[2];
			posY1 = _corners[1] + 1;
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
				float angle = GorgonMathUtility.Radians(Angle);		// Angle in radians.
				float cosVal = (float)System.Math.Cos(angle);		// Cached cosine.
				float sinVal = (float)System.Math.Sin(angle);		// Cached sine.

				Vertices[0].Position.X = (posX1 * cosVal - posY1 * sinVal);
				Vertices[0].Position.Y = (posX1 * sinVal + posY1 * cosVal);

				Vertices[1].Position.X = (posX2 * cosVal - posY1 * sinVal);
				Vertices[1].Position.Y = (posX2 * sinVal + posY1 * cosVal);

				Vertices[3].Position.X = (posX2 * cosVal - posY2 * sinVal);
				Vertices[3].Position.Y = (posX2 * sinVal + posY2 * cosVal);

				Vertices[5].Position.X = (posX1 * cosVal - posY2 * sinVal);
				Vertices[5].Position.Y = (posX1 * sinVal + posY2 * cosVal);
			}
			else
			{
				Vertices[0].Position.X = posX1;
				Vertices[0].Position.Y = posY1;
				Vertices[1].Position.X = posX2;
				Vertices[1].Position.Y = posY1;
				Vertices[3].Position.X = posX2;
				Vertices[3].Position.Y = posY2;
				Vertices[5].Position.X = posX1;
				Vertices[5].Position.Y = posY2;
			}

			// Translate.
			if (Position.X != 0.0f)
			{
				Vertices[0].Position.X += Position.X;
				Vertices[1].Position.X += Position.X;
				Vertices[3].Position.X += Position.X;
				Vertices[5].Position.X += Position.X + 1;
			}

			if (Position.Y != 0.0f)
			{
				Vertices[0].Position.Y += Position.Y;
				Vertices[1].Position.Y += Position.Y;
				Vertices[3].Position.Y += Position.Y;
				Vertices[5].Position.Y += Position.Y;
			}

			// Apply depth to the sprite.
			if (Depth != 0.0f)
			{
				Vertices[0].Position.Z = Depth;
				Vertices[1].Position.Z = Depth;
				Vertices[3].Position.Z = Depth;
				Vertices[5].Position.Z = Depth;
			}

			Vertices[7] = Vertices[0];
			Vertices[2] = Vertices[1];
			Vertices[4] = Vertices[3];
			Vertices[6] = Vertices[5];

			Vertices[0].Color = Vertices[7].Color = _colors[0];
			Vertices[1].Color = Vertices[2].Color = _colors[1];
			Vertices[3].Color = Vertices[4].Color = _colors[3];
			Vertices[5].Color = Vertices[6].Color = _colors[2];
		}

		/// <summary>
		/// Function to transform a filled rectangle.
		/// </summary>
		private void TransformFilled()
		{
			float posX1;		// Horizontal position 1.
			float posX2;		// Horizontal position 2.
			float posY1;		// Vertical position 1.
			float posY2;		// Vertical position 2.			

			BaseVertexCount = 0;
			VertexCount = 4;

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
				float angle = GorgonMathUtility.Radians(Angle);		// Angle in radians.
				float cosVal = (float)System.Math.Cos(angle);		// Cached cosine.
				float sinVal = (float)System.Math.Sin(angle);		// Cached sine.

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

			Vertices[0].Color = _colors[0];
			Vertices[1].Color = _colors[1];
			Vertices[2].Color = _colors[2];
			Vertices[3].Color = _colors[3];
		}

		/// <summary>
		/// Function to set up texture coordinates for an unfilled rectangle.
		/// </summary>
		private void UpdateTextureCoordinatesUnfilled()
		{
			// Calculate texture coordinates.
			Vector2 scaleUV = Vector2.Zero;
			Vector2 offsetUV = Vector2.Zero;

			if (Texture == null)
			{
				for (int i = 0; i < 8; i++)
					Vertices[i].UV = Vector2.Zero;
				return;
			}

			offsetUV.X = TextureOffset.X / Texture.Settings.Width;
			scaleUV.X = (TextureOffset.X + TextureSize.X) / Texture.Settings.Width;

			offsetUV.Y = TextureOffset.Y / Texture.Settings.Height;
			scaleUV.Y = (TextureOffset.Y + TextureSize.Y) / Texture.Settings.Height;

			Vertices[0].UV = offsetUV;
			Vertices[1].UV = new Vector2(scaleUV.X, offsetUV.Y);
			Vertices[2].UV = Vertices[1].UV;
			Vertices[3].UV = scaleUV;
			Vertices[4].UV = Vertices[3].UV;
			Vertices[5].UV = new Vector2(offsetUV.X, scaleUV.Y);
			Vertices[6].UV = Vertices[5].UV;
			Vertices[7].UV = Vertices[0].UV;
		}

		/// <summary>
		/// Function to set up the texture coordinates for a filled rectangle.
		/// </summary>
		private void UpdatedTextureCoordinatesFilled()
		{
			// Calculate texture coordinates.
			Vector2 scaleUV = Vector2.Zero;
			Vector2 offsetUV = Vector2.Zero;

			if (Texture == null)
			{
				for (int i = 0; i < 4; i++)
					Vertices[i].UV = Vector2.Zero;
				return;
			}

			offsetUV.X = TextureOffset.X / Texture.Settings.Width;
			scaleUV.X = (TextureOffset.X + TextureSize.X) / Texture.Settings.Width;

			offsetUV.Y = TextureOffset.Y / Texture.Settings.Height;
			scaleUV.Y = (TextureOffset.Y + TextureSize.Y) / Texture.Settings.Height;

			Vertices[0].UV = offsetUV;
			Vertices[1].UV = new Vector2(scaleUV.X, offsetUV.Y);
			Vertices[2].UV = new Vector2(offsetUV.X, scaleUV.Y);
			Vertices[3].UV = scaleUV;
		}

		/// <summary>
		/// Function to update the texture coordinates for an unfilled rectangle using quads.
		/// </summary>
		private void UpdateTextureCoordinatesQuads()
		{
			Vector2 penMid = Vector2.Zero;
			Vector2 u = Vector2.Zero;
			Vector2 v = Vector2.Zero;
			Vector2 corner3 = Vector2.Zero;
			Vector2 corner4 = Vector2.Zero;

			if (Texture == null)
			{
				for (int i = 0; i < 16; i++)
					Vertices[i].UV = Vector2.Zero;
				return;
			}

			Vector2.Divide(ref _penSize, 2.0f, out penMid);

			// Top line.
			Vertices[0].UV.X = (TextureRegion.X) / Texture.Settings.Width;
			Vertices[0].UV.Y = (TextureRegion.Y) / Texture.Settings.Height;
			Vertices[1].UV.X = (TextureRegion.Right) / Texture.Settings.Width;
			Vertices[1].UV.Y = (TextureRegion.Y) / Texture.Settings.Height;
			Vertices[2].UV.X = (TextureRegion.X) / Texture.Settings.Width;
			Vertices[2].UV.Y = (TextureRegion.Y + _penSize.Y) / Texture.Settings.Height;
			Vertices[3].UV.X = (TextureRegion.Right) / Texture.Settings.Width;
			Vertices[3].UV.Y = (TextureRegion.Y + _penSize.Y) / Texture.Settings.Height;

			// Right line.
			Vertices[4].UV.X = (TextureRegion.Right - _penSize.X) / Texture.Settings.Width;
			Vertices[4].UV.Y = (TextureRegion.Y) / Texture.Settings.Height;
			Vertices[5].UV.X = (TextureRegion.Right) / Texture.Settings.Width;
			Vertices[5].UV.Y = (TextureRegion.Y) / Texture.Settings.Height;
			Vertices[6].UV.X = (TextureRegion.Right - _penSize.X) / Texture.Settings.Width;
			Vertices[6].UV.Y = (TextureRegion.Bottom) / Texture.Settings.Height;
			Vertices[7].UV.X = (TextureRegion.Right) / Texture.Settings.Width;
			Vertices[7].UV.Y = (TextureRegion.Bottom) / Texture.Settings.Height;

			// Bottom line.
			Vertices[8].UV.X = (TextureRegion.X) / Texture.Settings.Width;
			Vertices[8].UV.Y = (TextureRegion.Bottom - _penSize.Y) / Texture.Settings.Height;
			Vertices[9].UV.X = (TextureRegion.Right) / Texture.Settings.Width;
			Vertices[9].UV.Y = (TextureRegion.Bottom - _penSize.Y) / Texture.Settings.Height;
			Vertices[10].UV.X = (TextureRegion.X) / Texture.Settings.Width;
			Vertices[10].UV.Y = (TextureRegion.Bottom) / Texture.Settings.Height;
			Vertices[11].UV.X = (TextureRegion.Right) / Texture.Settings.Width;
			Vertices[11].UV.Y = (TextureRegion.Bottom) / Texture.Settings.Height;

			// Left line.
			Vertices[12].UV.X = (TextureRegion.X) / Texture.Settings.Width;
			Vertices[12].UV.Y = (TextureRegion.Y) / Texture.Settings.Height;
			Vertices[13].UV.X = (TextureRegion.X + _penSize.X) / Texture.Settings.Width;
			Vertices[13].UV.Y = (TextureRegion.Y) / Texture.Settings.Height;
			Vertices[14].UV.X = (TextureRegion.X) / Texture.Settings.Width;
			Vertices[14].UV.Y = (TextureRegion.Bottom) / Texture.Settings.Height;
			Vertices[15].UV.X = (TextureRegion.X + _penSize.X) / Texture.Settings.Width;
			Vertices[15].UV.Y = (TextureRegion.Bottom) / Texture.Settings.Height;
		}

		/// <summary>
		/// Function to update the texture coordinates.
		/// </summary>
		protected override void UpdateTextureCoordinates()
		{
			if (IsFilled)
				UpdatedTextureCoordinatesFilled();
			else
			{
				if ((_penSize.X == 1.0f) && (_penSize.Y == 1.0f))
					UpdateTextureCoordinatesUnfilled();
				else
					UpdateTextureCoordinatesQuads();
			}
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
		/// Function to transform the vertices.
		/// </summary>
		protected override void TransformVertices()
		{
			if (!_isFilled)
			{
				if ((_penSize.X == 1.0f) && (_penSize.Y == 1.0f))
					TransformUnfilled();
				else
					TransformUnfilledQuads();
			}
			else
				TransformFilled();
		}

		/// <summary>
		/// Function to set the color for a specific vertex on the sprite.
		/// </summary>
		/// <param name="corner">Corner of the rectangle to set.</param>
		/// <param name="color">Color to set.</param>
		public void SetVertexColor(RectangleCorner corner, GorgonColor color)
		{
			_colors[(int)corner] = color;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRectangle"/> class.
		/// </summary>
		/// <param name="gorgon2D">Gorgon interface that owns this renderable.</param>
		/// <param name="name">The name of the rectangle.</param>
		/// <param name="rectangle">The rectangle position and size.</param>
		/// <param name="filled">TRUE to have a filled rectangle, FALSE to have an outline.</param>
		internal GorgonRectangle(Gorgon2D gorgon2D, string name, RectangleF rectangle, bool filled)
			: base(gorgon2D, name)
		{
			_corners = new float[4];
			_colors = new GorgonColor[]
			{
				new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f),
				new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f),
				new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f),
				new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f)
			};
			TextureRegion = rectangle;
			Size = new Vector2(rectangle.Width, rectangle.Height);
			Position = new Vector2(rectangle.X, rectangle.Y);
			InitializeVertices(16);
			IsFilled = filled;
		}
		#endregion
	}
}
