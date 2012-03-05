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
// Created: Sunday, March 04, 2012 4:18:43 PM
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
	/// A renderable object for drawing a triangle on the target.
	/// </summary>
	public class GorgonTriangle
		: GorgonMoveable
	{
		#region Variables.
		private GorgonLine _line = null;												// Line used to draw outlined triangle.
		private GorgonColor[] _colors = null;											// Colors for each point.
		private Vector2[] _points = null;												// Points for the triangle.
		#endregion

		#region Properties.
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
		/// Property to return the number of indices that make up this renderable.
		/// </summary>
		protected internal override int IndexCount
		{
			get 
			{
				return 0;
			}
		}

		/// <summary>
		/// Property to set or return the index buffer for this renderable.
		/// </summary>
		protected internal override GorgonIndexBuffer IndexBuffer
		{
			get
			{
				return null;
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
				_colors[0] = _colors[1] = _colors[2] = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the triangle is filled or not.
		/// </summary>
		public bool IsFilled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the thickness of the lines for an unfilled triangle.
		/// </summary>
		public Vector2 LineThickness
		{
			get
			{
				return _line.LineThickness;
			}
			set
			{
				_line.LineThickness = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the texture coordinates.
		/// </summary>
		protected override void UpdateTextureCoordinates()
		{
			Vector2 uvScale = Vector2.Zero;
			Vector2 uvOffset = Vector2.Zero;

			if (Texture == null)
			{
				Vertices[0].UV = Vertices[1].UV = Vertices[2].UV = Vector2.Zero;
				return;
			}

			// TODO: Need a way to convert points into unit vectors.

			for (int i = 0; i < 3; i++)
			{
				int endIndex = i + 1;

				if (i + 1 > 2)
					endIndex = 0;

				uvOffset = new Vector2(TextureOffset.X + _points[i].X, TextureOffset.Y + _points[i].Y);
				uvScale = new Vector2(TextureRegion.Width + _points[endIndex].X, TextureRegion.Height + _points[endIndex].Y);

				Vertices[i].UV = new Vector2(uvOffset.X / Texture.Settings.Width, uvOffset.Y / Texture.Settings.Height);
				Vertices[endIndex].UV = new Vector2(uvScale.X / Texture.Settings.Width, uvScale.Y / Texture.Settings.Height);
			}
		}

		/// <summary>
		/// Function to update the vertices for the renderable.
		/// </summary>
		protected override void UpdateVertices()
		{
			// TODO: Update Size property and add Get/SetPointOffset methods.
		}

		/// <summary>
		/// Function to transform the vertices.
		/// </summary>
		protected override void TransformVertices()
		{
			Vector2 point1 = Vector2.Zero;
			Vector2 point2 = Vector2.Zero;
			Vector2 point3 = Vector2.Zero;

			point1 = Vector2.Subtract(_points[0], Anchor);
			point2 = Vector2.Subtract(_points[1], Anchor);
			point3 = Vector2.Subtract(_points[2], Anchor);

			// Scale horizontally if necessary.
			if (Scale.X != 1.0f)
			{
				point1.X *= Scale.X;
				point2.X *= Scale.X;
				point3.X *= Scale.X;
			}

			// Scale vertically.
			if (Scale.Y != 1.0f)
			{
				point1.Y *= Scale.Y;
				point2.Y *= Scale.Y;
				point3.Y *= Scale.Y;
			}

			// Calculate rotation if necessary.
			if (Angle != 0.0f)
			{
				float angle = GorgonMathUtility.Radians(Angle);		// Angle in radians.
				float cosVal = (float)System.Math.Cos(angle);		// Cached cosine.
				float sinVal = (float)System.Math.Sin(angle);		// Cached sine.

				Vertices[0].Position.X = (point1.X * cosVal) - (point1.Y * sinVal);
				Vertices[0].Position.Y = (point1.X * sinVal) + (point1.Y * cosVal);

				Vertices[1].Position.X = (point2.X * cosVal) - (point2.Y * sinVal);
				Vertices[1].Position.Y = (point2.X * sinVal) + (point2.Y * cosVal);

				Vertices[2].Position.X = (point3.X * cosVal) - (point3.Y * sinVal);
				Vertices[2].Position.Y = (point3.X * sinVal) + (point3.Y * cosVal);
			}
			else
			{
				Vertices[0].Position = new Vector4(point1, 0, 1.0f);
				Vertices[1].Position = new Vector4(point2, 0, 1.0f);
				Vertices[2].Position = new Vector4(point3, 0, 1.0f);
			}

			// Translate.
			if (Position.X != 0.0f)
			{
				Vertices[0].Position.X += Position.X;
				Vertices[1].Position.X += Position.X;
				Vertices[2].Position.X += Position.X;
			}

			if (Position.Y != 0.0f)
			{
				Vertices[0].Position.Y += Position.Y;
				Vertices[1].Position.Y += Position.Y;
				Vertices[2].Position.Y += Position.Y;
			}

			if (Depth != 0.0f)
			{
				Vertices[0].Position.Z = Depth;
				Vertices[1].Position.Z = Depth;
				Vertices[2].Position.Z = Depth;
			}

			Vertices[0].Color = _colors[0];
			Vertices[1].Color = _colors[1];
			Vertices[2].Color = _colors[2];
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		public override void Draw()
		{
			Vector2 offset = Vector2.Zero;

			if (IsFilled)
			{
				base.Draw();
				return;
			}

			offset.X = LineThickness.X - 1.0f;
			offset.Y = LineThickness.Y - 1.0f;

			TransformVertices();

			// Set global line state.
			_line.AlphaTestValues = AlphaTestValues;
			_line.Texture = Texture;
			_line.CullingMode = CullingMode;
			_line.Depth = Depth;

			_line.TextureStart = TextureRegion.Location;
			_line.TextureEnd = new Vector2(TextureRegion.Right, TextureRegion.Top + offset.Y);
			_line.StartColor = _colors[0];
			_line.EndColor = _colors[1];
			_line.StartPoint = new Vector2(Vertices[0].Position.X, Vertices[0].Position.Y);
			_line.EndPoint = new Vector2(Vertices[1].Position.X, Vertices[1].Position.Y);
			_line.Draw();

			_line.TextureStart = new Vector2(TextureRegion.Right, TextureRegion.Top);
			_line.TextureEnd = new Vector2(TextureRegion.Right, TextureRegion.Bottom);
			_line.StartColor = _colors[1];
			_line.EndColor = _colors[2];
			_line.StartPoint = new Vector2(Vertices[1].Position.X, Vertices[1].Position.Y);
			_line.EndPoint = new Vector2(Vertices[2].Position.X, Vertices[2].Position.Y);
			_line.Draw();

			_line.TextureStart = new Vector2(TextureRegion.Left, TextureRegion.Bottom - offset.Y);
			_line.TextureEnd = new Vector2(TextureRegion.Right, TextureRegion.Bottom);
			_line.StartColor = _colors[0];
			_line.EndColor = _colors[2];
			_line.StartPoint = (Vector2)Vertices[0].Position;
			_line.EndPoint = (Vector2)Vertices[2].Position;
			_line.Draw();
		}

		/// <summary>
		/// Function to set a color for a specific point on the triangle.
		/// </summary>
		/// <param name="pointIndex">Index of the point.</param>
		/// <param name="color">Color to set.</param>
		/// <remarks>The <paramref name="pointIndex"/> must be between 0 and 2 or an exception will be raised.</remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the pointIndex parameter is less than 0 or greater than 2.</exception>
		public void SetPointColor(int pointIndex, GorgonColor color)
		{
			GorgonDebug.AssertParamRange(pointIndex, 0, 2, true, true, "pointIndex");

			_colors[pointIndex] = color;
		}

		/// <summary>
		/// Function to get a color from a specific point on the triangle.
		/// </summary>
		/// <param name="pointIndex">Index of the point.</param>
		/// <remarks>The <paramref name="pointIndex"/> must be between 0 and 2 or an exception will be raised.</remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the pointIndex parameter is less than 0 or greater than 2.</exception>
		/// <returns>The color of the specified point.</returns>
		public GorgonColor GetPointColor(int pointIndex)
		{
			GorgonDebug.AssertParamRange(pointIndex, 0, 2, true, true, "pointIndex");

			return _colors[pointIndex];
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTriangle"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that created this object.</param>
		/// <param name="name">The name of the triangle object.</param>
		/// <param name="point1">The first point.</param>
		/// <param name="point2">The second point.</param>
		/// <param name="point3">The third point.</param>
		/// <param name="color">The color of the triangle.</param>
		/// <param name="isFilled">TRUE to create a filled triangle, FALSE to create an unfilled triangle.</param>
		internal GorgonTriangle(Gorgon2D gorgon2D, string name, Vector2 point1, Vector2 point2, Vector2 point3, GorgonColor color, bool isFilled)
			: base(gorgon2D, name)
		{
			InitializeVertices(3);
			VertexCount = 3;
			BaseVertexCount = 3;
			IsFilled = isFilled;

			_points = new[]
				{
					point1,
					point2,
					point3
				};

			_colors = new[]
				{
					color,
					color,
					color
				};
			_line = new GorgonLine(gorgon2D, "Triangle.Line", Vector2.Zero, Vector2.Zero, color);
		}
		#endregion
	}
}
