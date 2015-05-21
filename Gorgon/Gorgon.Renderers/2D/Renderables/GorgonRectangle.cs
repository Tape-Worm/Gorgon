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
using System.Drawing;
using Gorgon.Animation;
using Gorgon.Graphics;
using Gorgon.Math;
using SlimMath;

namespace Gorgon.Renderers
{
	/// <summary>
	/// A renderable object for drawing a rectangle on the screen.
	/// </summary>
	/// <remarks>This is similar to a sprite only that the rectangle can be filled or unfilled and does not have some of the same properties of a sprite.</remarks>
	public class GorgonRectangle
		: GorgonMoveable
	{
		#region Variables.
		private readonly Vector2[] _corners;				// Corner points.
		private readonly GorgonColor[] _colors;				// Colors for each corner.
		private readonly GorgonLine _line;					// Line used for outlined drawing.
		private readonly GorgonSprite _filled;				// Sprite used for rectangle drawing.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the type of primitive for the renderable.
		/// </summary>
		protected internal override PrimitiveType PrimitiveType
		{
		    get
		    {
                return IsFilled ? PrimitiveType.TriangleList : PrimitiveType.LineList;
		    }
		}

		/// <summary>
		/// Property to return the number of indices that make up this renderable.
		/// </summary>
		protected internal override int IndexCount
		{
		    get
		    {
		        return IsFilled ? 6 : 0;
		    }
		}

		/// <summary>
		/// Property to set or return the thickness of the lines for a rectangle outline.
		/// </summary>
		[AnimatedProperty]
		public Vector2 LineThickness
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the rectangle dimensions.
		/// </summary>
		public RectangleF Rectangle
		{
			get
			{
				return new RectangleF(Position, Size);
			}
			set
			{
				if ((Position.Equals(value.Location))
					&& (Size.Equals(value.Size)))
				{
					return;
				}

				Position = value.Location;
				Size = value.Size;
				NeedsVertexUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return whether the rectangle should be filled or not.
		/// </summary>
		public bool IsFilled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the color for a renderable object.
		/// </summary>
		[AnimatedProperty]
		public override GorgonColor Color
		{
			get
			{
				return _colors[0];
			}
			set
			{
				_colors[3] = _colors[2] = _colors[1] = _colors[0] = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the texture coordinates.
		/// </summary>
		protected override void UpdateTextureCoordinates()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Function to update the vertices for the renderable.
		/// </summary>
		protected override void UpdateVertices()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Function to transform the vertices.
		/// </summary>
		protected override void TransformVertices()
		{
			_corners[0] = new Vector2(-Anchor.X, -Anchor.Y);
			_corners[1] = new Vector2(Size.X - Anchor.X, -Anchor.Y);
			_corners[2] = new Vector2(Size.X - Anchor.X, Size.Y - Anchor.Y);
			_corners[3] = new Vector2(-Anchor.X, Size.Y - Anchor.Y);
			
			float posX1 = _corners[0].X;
			float posX2 = _corners[1].X;
			float posY1 = _corners[0].Y;
			float posY2 = _corners[2].Y;

			// Scale horizontally if necessary.
			// ReSharper disable CompareOfFloatsByEqualityOperator
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

				_corners[0].X = (posX1 * cosVal - posY1 * sinVal) + Position.X;
				_corners[0].Y = (posX1 * sinVal + posY1 * cosVal) + Position.Y;

                _corners[1].X = (posX2 * cosVal - posY1 * sinVal) + Position.X;
                _corners[1].Y = (posX2 * sinVal + posY1 * cosVal) + Position.Y;

                _corners[2].X = (posX2 * cosVal - posY2 * sinVal) + Position.X;
                _corners[2].Y = (posX2 * sinVal + posY2 * cosVal) + Position.Y;

                _corners[3].X = (posX1 * cosVal - posY2 * sinVal) + Position.X;
                _corners[3].Y = (posX1 * sinVal + posY2 * cosVal) + Position.Y;
			}
			else
			{
                _corners[0].X = posX1 + Position.X;
                _corners[0].Y = posY1 + Position.Y;
                _corners[1].X = posX2 + Position.X;
                _corners[1].Y = posY1 + Position.Y;
                _corners[2].X = posX2 + Position.X;
                _corners[2].Y = posY2 + Position.Y;
                _corners[3].X = posX1 + Position.X;
                _corners[3].Y = posY2 + Position.Y;
			}
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		/// <summary>
		/// Function to draw an unfilled rectangle.
		/// </summary>
		private void DrawUnfilled()
		{
			TransformVertices();

			for (int i = 0; i < _corners.Length; i++)
			{
				_corners[i].X = (float)System.Math.Ceiling(_corners[i].X);
				_corners[i].Y = (float)System.Math.Ceiling(_corners[i].Y) + 1;
			}

			// Set global line state.
			_line.AlphaTestValues = AlphaTestValues;
			_line.Texture = Texture;
			_line.LineThickness = LineThickness;
			_line.CullingMode = CullingMode;
			_line.Depth = Depth;
			_line.Blending = Blending;
			_line.DepthStencil = DepthStencil;
			_line.TextureSampler = TextureSampler;

			_line.TextureStart = TextureRegion.Location;
			_line.TextureEnd = new Vector2(TextureRegion.Right, TextureRegion.Top);
			_line.StartColor = _colors[0];
			_line.EndColor = _colors[1];
			_line.StartPoint = new Vector2(_corners[0].X, _corners[0].Y);
			_line.EndPoint = new Vector2(_corners[1].X + 1.0f, _corners[1].Y);
			_line.Draw();

			_line.TextureStart = new Vector2(TextureRegion.Right, TextureRegion.Top);
			_line.TextureEnd = new Vector2(TextureRegion.Right, TextureRegion.Bottom);
			_line.StartColor = _colors[1];
			_line.EndColor = _colors[3];
			_line.StartPoint = new Vector2(_corners[1].X + 1, _corners[1].Y);
			_line.EndPoint = new Vector2(_corners[2].X + 1, _corners[2].Y);
			_line.Draw();

			_line.TextureStart = new Vector2(TextureRegion.Right, TextureRegion.Bottom);
			_line.TextureEnd = new Vector2(TextureRegion.Left, TextureRegion.Bottom);
			_line.StartColor = _colors[3];
			_line.EndColor = _colors[2];
			_line.StartPoint = new Vector2(_corners[2].X, _corners[2].Y);
			_line.EndPoint = new Vector2(_corners[3].X + 1.0f, _corners[3].Y);
			_line.Draw();

			_line.TextureStart = new Vector2(TextureRegion.Left, TextureRegion.Bottom);
			_line.TextureEnd = new Vector2(TextureRegion.Left, TextureRegion.Top);
			_line.StartColor = _colors[2];
			_line.EndColor = _colors[0];
			_line.StartPoint = new Vector2(_corners[3].X + 1.0f, _corners[3].Y);
			_line.EndPoint = new Vector2(_corners[0].X + 1.0f, _corners[0].Y);
			_line.Draw();
		}

		/// <summary>
		/// Function to draw a filled rectangle.
		/// </summary>
		private void DrawFilled()
		{
			_filled.AlphaTestValues = AlphaTestValues;
			_filled.Anchor = Anchor;
			_filled.Angle = Angle;
			_filled.SetCornerColor(RectangleCorner.UpperLeft, _colors[0]);
			_filled.SetCornerColor(RectangleCorner.UpperRight, _colors[1]);
			_filled.SetCornerColor(RectangleCorner.LowerLeft, _colors[2]);
			_filled.SetCornerColor(RectangleCorner.LowerRight, _colors[3]);
			_filled.CullingMode = CullingMode;
			_filled.Depth = Depth;
			_filled.Position = Rectangle.Location;
			_filled.Size = Rectangle.Size;
			_filled.Texture = Texture;
			_filled.TextureRegion = TextureRegion;
			_filled.Blending = Blending;
			_filled.DepthStencil = DepthStencil;
			_filled.TextureSampler = TextureSampler;

			_filled.Draw();
		}

		/// <summary>
		/// Function to set a color for a specific corner of the rectangle.
		/// </summary>
		/// <param name="corner">Corner to set the color on.</param>
		/// <param name="color">Color to set.</param>
		public void SetCornerColor(RectangleCorner corner, GorgonColor color)
		{
			_colors[(int)corner] = color;
		}

		/// <summary>
		/// Function to retrieve a color for a specific corner of the rectangle.
		/// </summary>
		/// <param name="corner">Corner to retrieve color from.</param>
		/// <returns>The color on the specified corner.</returns>
		public GorgonColor GetCornerColor(RectangleCorner corner)
		{
			return _colors[(int)corner];
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		public override void Draw()
		{
			if (!IsFilled)
			{
				DrawUnfilled();
			}
			else
			{
				DrawFilled();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRectangle"/> class.
		/// </summary>
		/// <param name="gorgon2D">Gorgon interface that owns this renderable.</param>
		/// <param name="name">The name of the rectangle.</param>
		/// <param name="filled">TRUE to draw a filled rectangle, FALSE to draw an outline.</param>
		internal GorgonRectangle(Gorgon2D gorgon2D, string name, bool filled)
			: base(gorgon2D, name)
		{
			_colors = new[]
			{
				GorgonColor.White,
				GorgonColor.White,
				GorgonColor.White,
				GorgonColor.White
			};

			_corners = new[]
			{
				Vector2.Zero,
				Vector2.Zero,
				Vector2.Zero,
				Vector2.Zero
			};

			IsFilled = filled;
			_filled = gorgon2D.Renderables.CreateSprite("Rectangle.Sprite", new Vector2(1), GorgonColor.White);
			_line = new GorgonLine(gorgon2D, "Rectangle.Line")
			{
				Color = GorgonColor.White,
                TextureEnd = new Vector2(1),
                StartPoint = Vector2.Zero,
                EndPoint = new Vector2(1)
			};
		}		
		#endregion
	}
}
