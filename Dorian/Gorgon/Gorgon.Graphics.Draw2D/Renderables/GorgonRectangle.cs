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
	/// <remarks>This is similar to a sprite only that the rectangle can be filled or unfilled and does not have some of the same properties of a sprite.</remarks>
	public class GorgonRectangle
		: GorgonMoveable
	{
		#region Variables.		
		private GorgonColor[] _colors = null;				// Colors for each corner.
		private GorgonLine _line = null;					// Line used for outlined drawing.
		private GorgonSprite _filled = null;				// Sprite used for rectangle drawing.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the type of primitive for the renderable.
		/// </summary>
		protected internal override PrimitiveType PrimitiveType
		{
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Property to return the number of indices that make up this renderable.
		/// </summary>
		protected internal override int IndexCount
		{
			get { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Property to set or return the position of the sprite.
		/// </summary>
		public override Vector2 Position
		{
			get
			{
				return Rectangle.Location;
			}
			set
			{
				Rectangle = new RectangleF(value, Size);
			}
		}

		/// <summary>
		/// Property to set or return the size of the renderable.
		/// </summary>
		public override Vector2 Size
		{
			get
			{
				return Rectangle.Size;
			}
			set
			{
				Rectangle = new RectangleF(Position, value);
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
			throw new NotImplementedException();
		}

		/// <summary>
		/// Function to update the vertices for the renderable.
		/// </summary>
		protected override void UpdateVertices()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Function to transform the vertices.
		/// </summary>
		protected override void TransformVertices()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Property to set or return the thickness of the lines for a rectangle outline.
		/// </summary>
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
			get;
			set;
		}

		/// <summary>
		/// Function to draw an unfilled rectangle.
		/// </summary>
		private void DrawUnfilled()
		{
			Vector2 penMid = Vector2.Zero;

			// Set global line state.
			_line.AlphaTestValues = AlphaTestValues;
			_line.Anchor = Anchor;
			_line.Angle = Angle;
			_line.Texture = Texture;
			_line.LineThickness = LineThickness;
			_line.CullingMode = CullingMode;
			_line.Depth = Depth;

			if ((LineThickness.X > 1.0f) || (LineThickness.Y > 1.0f))
				penMid = Vector2.Divide(LineThickness, 2.0f);

			_line.TextureStart = TextureRegion.Location;
			_line.TextureEnd = new Vector2(TextureRegion.Right, TextureRegion.Top);
			_line.StartColor = _colors[0];
			_line.EndColor = _colors[1];
			_line.StartPoint = new Vector2(Rectangle.Left - penMid.X, Rectangle.Top + 1 - penMid.Y);
			_line.EndPoint = new Vector2(Rectangle.Right + penMid.Y, Rectangle.Top + 1 - penMid.Y);
			_line.Draw();

			_line.TextureStart = new Vector2(TextureRegion.Right, TextureRegion.Top);
			_line.TextureEnd = new Vector2(TextureRegion.Right, TextureRegion.Bottom);
			_line.StartColor = _colors[1];
			_line.EndColor = _colors[2];
			_line.StartPoint = new Vector2(Rectangle.Right - penMid.X, Rectangle.Top - penMid.Y);
			_line.EndPoint = new Vector2(Rectangle.Right - penMid.X, Rectangle.Bottom + penMid.Y);
			_line.Draw();

			_line.TextureStart = new Vector2(TextureRegion.Left, TextureRegion.Bottom);
			_line.TextureEnd = new Vector2(TextureRegion.Right, TextureRegion.Bottom);
			_line.StartColor = _colors[2];
			_line.EndColor = _colors[3];
			_line.StartPoint = new Vector2(Rectangle.Left - penMid.X, Rectangle.Bottom - penMid.Y);
			_line.EndPoint = new Vector2(Rectangle.Right, Rectangle.Bottom - penMid.Y);
			_line.Draw();

			_line.TextureStart = new Vector2(TextureRegion.Left, TextureRegion.Top);
			_line.TextureEnd = new Vector2(TextureRegion.Left, TextureRegion.Bottom);
			_line.StartColor = _colors[3];
			_line.EndColor = _colors[0];
			_line.StartPoint = new Vector2(Rectangle.Left + 1, Rectangle.Top);
			_line.EndPoint = new Vector2(Rectangle.Left + 1, Rectangle.Bottom);
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

			_filled.Draw();
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		public override void Draw()
		{
			if (!IsFilled)
				DrawUnfilled();
			else
				DrawFilled();
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
			IsFilled = filled;
			_filled = new GorgonSprite(gorgon2D, "Rectangle.Sprite", Size.X, Size.Y);
			_line = new GorgonLine(gorgon2D, "Rectangle.Line", Position, new Vector2(rectangle.Right, rectangle.Bottom));

			_line.Blending = _filled.Blending = this.Blending;
			_line.DepthStencil = _filled.DepthStencil = this.DepthStencil;
			_line.TextureSampler = _filled.TextureSampler = this.TextureSampler;			
		}		
		#endregion
	}
}
