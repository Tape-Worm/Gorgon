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
using System.Drawing;
using GorgonLibrary.Animation;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics;
using GorgonLibrary.Math;
using SlimMath;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// A renderable object for drawing a triangle.
	/// </summary>
	/// <remarks>Unlike other renderables, this object does not have a texture region, and instead takes a texture coordinate from the points passed in.</remarks>
	public class GorgonTriangle
		: GorgonRenderable, IMoveable
	{
		#region Value Types.
		/// <summary>
		/// A point for a triangle.
		/// </summary>
		/// <remarks>Points in a triangle use relative coordinates, that is, they are defined as an offset from 0,0.  So passing a point of 30,50 will create that point 30 units to the right, and 50 units down.</remarks>
		public struct TrianglePoint
		{
			/// <summary>
			/// Position of the point.
			/// </summary>
			/// <remarks>The position is relative.</remarks>
			public Vector2 Position;
			/// <summary>
			/// The texture coordinate of the point.
			/// </summary>
			/// <remarks>This texture value is in texel space (0..1).</remarks>
			public Vector2 TextureCoordinate;
			/// <summary>
			/// The color of the point.
			/// </summary>
			public GorgonColor Color;

			/// <summary>
			/// Initializes a new instance of the <see cref="TrianglePoint"/> struct.
			/// </summary>
			/// <param name="position">The position of the point.</param>
			/// <param name="color">Color for the point.</param>
			/// <param name="textureCoordinate">The texture coordinate for the point.</param>
			public TrianglePoint(Vector2 position, GorgonColor color, Vector2 textureCoordinate)
			{
				Position = position;
				Color = color;
				TextureCoordinate = textureCoordinate;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="TrianglePoint"/> struct.
			/// </summary>
			/// <param name="position">The position of the point.</param>
			/// <param name="color">Color for the point.</param>
			public TrianglePoint(Vector2 position, GorgonColor color)
			{
				Position = position;
				Color = color;
				TextureCoordinate = Vector2.Zero;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="TrianglePoint"/> struct.
			/// </summary>
			/// <param name="position">The position of the point.</param>
			public TrianglePoint(Vector2 position)
			{
				Position = position;
				Color = new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f);
				TextureCoordinate = Vector2.Zero;
			}
		}
		#endregion

		#region Variables.
		private readonly GorgonLine _line;					// Line used to draw outlined triangle.
		private readonly TrianglePoint[] _points;			// Points for the triangle.
		private Vector2 _anchor = Vector2.Zero;				// Anchor point.
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
		/// Property to set or return a texture for the renderable.
		/// </summary>
		public override GorgonTexture2D Texture
		{
			get
			{
				// We override this to remove the animated attribute.
				return base.Texture;
			}
			set
			{
				base.Texture = value;
			}
		}

		/// <summary>
		/// Property to set or return the color for a renderable object.
		/// </summary>
		[AnimatedProperty()]
		public override GorgonColor Color
		{
			get
			{
				return _points[0].Color;
			}
			set
			{
				_points[0].Color = _points[1].Color = _points[2].Color = value;
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
		[AnimatedProperty()]
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

		/// <summary>
		/// Property to return the texture region.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when a value is assigned to the property.</exception>
		public override RectangleF TextureRegion
		{
			get
			{
				var min = new Vector2(float.MaxValue, float.MaxValue);
				var max = new Vector2(float.MinValue, float.MinValue);

				for (int i = 0; i < _points.Length; i++)
				{
					min.X = _points[i].TextureCoordinate.X.Min(min.X);
					min.Y = _points[i].TextureCoordinate.Y.Min(min.Y);
					max.X = _points[i].TextureCoordinate.X.Max(max.X);
					max.Y = _points[i].TextureCoordinate.Y.Max(max.Y);
				}

				return RectangleF.FromLTRB(min.X, min.Y, max.X, max.Y);
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to return the coordinates in the texture to use as a starting point for drawing.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when a value is assigned to the property.</exception>
		public override Vector2 TextureOffset
		{
			get
			{
				return TextureRegion.Location;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to return the scaling of the texture width and height.
		/// </summary>
		/// <exception cref="System.NotSupportedException">Thrown when a value is assigned to the property.</exception>
		public override Vector2 TextureSize
		{
			get
			{
				return TextureRegion.Size;
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the vertices.
		/// </summary>
		protected override void UpdateVertices()
		{
			Vector2 point1 = _points[0].Position;
			Vector2 point2 = _points[1].Position;
			Vector2 point3 = _points[2].Position;

			if ((Anchor.X != 0) || (Anchor.Y != 0))
			{
				point1 = Vector2.Subtract(point1, Anchor);
				point2 = Vector2.Subtract(point2, Anchor);
				point3 = Vector2.Subtract(point3, Anchor);
			}

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
				float angle = Angle.Radians();						// Angle in radians.
				float cosVal = angle.Cos();							// Cached cosine.
				float sinVal = angle.Sin();							// Cached sine.

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

			Vertices[0].Color = _points[0].Color;
			Vertices[1].Color = _points[1].Color;
			Vertices[2].Color = _points[2].Color;
		}

        /// <summary>
        /// Function to update the texture coordinates.
        /// </summary>
        protected override void UpdateTextureCoordinates()
        {
            if (Texture == null)
            {
                Vertices[0].UV = Vertices[1].UV = Vertices[2].UV = Vector2.Zero;
                return;
            }

            Vertices[0].UV = _points[0].TextureCoordinate;
            Vertices[1].UV = _points[1].TextureCoordinate;
            Vertices[2].UV = _points[2].TextureCoordinate;
        }

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		public override void Draw()
		{
			Vector2 offset = Vector2.Zero;

			if (IsFilled)
			{
				if (NeedsTextureUpdate)
				{
					UpdateTextureCoordinates();
					NeedsTextureUpdate = false;
				}

				UpdateVertices();

				base.Draw();
				return;
			}

			offset.X = LineThickness.X - 1.0f;
			offset.Y = LineThickness.Y - 1.0f;

			UpdateVertices();

			// Set global line state.
			_line.AlphaTestValues = AlphaTestValues;
			_line.Texture = Texture;
			_line.CullingMode = CullingMode;
			_line.Depth = Depth;

			_line.TextureStart = _points[0].TextureCoordinate;
			_line.TextureEnd = _points[1].TextureCoordinate;
			_line.StartColor = _points[0].Color;
			_line.EndColor = _points[1].Color;
			_line.StartPoint = new Vector2(Vertices[0].Position.X, Vertices[0].Position.Y);
			_line.EndPoint = new Vector2(Vertices[1].Position.X, Vertices[1].Position.Y);
			_line.Draw();

			_line.TextureStart = _points[1].TextureCoordinate;
			_line.TextureEnd = _points[2].TextureCoordinate;
			_line.StartColor = _points[1].Color;
			_line.EndColor = _points[2].Color;
			_line.StartPoint = new Vector2(Vertices[1].Position.X, Vertices[1].Position.Y);
			_line.EndPoint = new Vector2(Vertices[2].Position.X, Vertices[2].Position.Y);
			_line.Draw();

			_line.TextureStart = _points[2].TextureCoordinate;
			_line.TextureEnd = _points[0].TextureCoordinate;
			_line.StartColor = _points[2].Color;
			_line.EndColor = _points[0].Color;
			_line.StartPoint = (Vector2)Vertices[2].Position;
			_line.EndPoint = (Vector2)Vertices[0].Position;
			_line.Draw();
		}

		/// <summary>
		/// Function to update a point in the triangle.
		/// </summary>
		/// <param name="pointIndex">Index of the point.</param>
		/// <param name="point">Point information.</param>
		/// <remarks>The <paramref name="pointIndex"/> must be between 0 and 2 (inclusive) or an exception will be raised.</remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the pointIndex parameter is less than 0 or greater than 2.</exception>
		public void SetPoint(int pointIndex, TrianglePoint point)
		{
			GorgonDebug.AssertParamRange(pointIndex, 0, 2, true, true, "pointIndex");

			_points[pointIndex] = point;
			NeedsTextureUpdate = true;
		}

		/// <summary>
		/// Function to retrieve a point in the triangle.
		/// </summary>
		/// <param name="pointIndex">Index of the point.</param>
		/// <remarks>The <paramref name="pointIndex"/> must be between 0 and 2 (inclusive) or an exception will be raised.</remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the pointIndex parameter is less than 0 or greater than 2.</exception>
		/// <returns>The point at the specified index.</returns>
		public TrianglePoint GetPoint(int pointIndex)
		{
			GorgonDebug.AssertParamRange(pointIndex, 0, 2, true, true, "pointIndex");

			return _points[pointIndex];
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
		/// <param name="isFilled">TRUE to create a filled triangle, FALSE to create an unfilled triangle.</param>
		internal GorgonTriangle(Gorgon2D gorgon2D, string name, TrianglePoint point1, TrianglePoint point2, TrianglePoint point3, bool isFilled)
			: base(gorgon2D, name)
		{
			Scale = new Vector2(1);
			VertexCount = 3;
			BaseVertexCount = 3;
			IsFilled = isFilled;

			_points = new[]
				{
					point1,
					point2,
					point3
				};

			_line = new GorgonLine(gorgon2D, "Triangle.Line", Vector2.Zero, Vector2.Zero, point1.Color);

            InitializeVertices(3);
		}
		#endregion

		#region IMoveable Members
		/// <summary>
		/// Property to set or return the size of the renderable.
		/// </summary>
		Vector2 IMoveable.Size
		{
			get
			{
				var min = new Vector2(float.MaxValue, float.MaxValue);
				var max = new Vector2(float.MinValue, float.MinValue);

				for (int i = 0; i < _points.Length; i++)
				{
					min.X = _points[i].Position.X.Min(min.X);
					min.Y = _points[i].Position.Y.Min(min.Y);
					max.X = _points[i].Position.X.Max(max.X);
					max.Y = _points[i].Position.Y.Max(max.Y);
				}

				return Vector2.Subtract(max, min);
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the anchor point for the triangle.
		/// </summary>
		[AnimatedProperty()]
		public Vector2 Anchor
		{
			get
			{
				return _anchor;
			}
			set
			{
				if (_anchor != value)
				{
					_anchor = value;
					NeedsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the angle of rotation (in degrees) for the triangle.
		/// </summary>
		[AnimatedProperty()]
		public float Angle
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the position of the triangle.
		/// </summary>
		[AnimatedProperty()]
		public Vector2 Position
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the relative scale of the triangle.
		/// </summary>
		/// <remarks>This property uses scalar values to provide a relative scale. 
		/// <para>Setting this value to a 0 vector will cause undefined behaviour and is not recommended.</para>
		/// </remarks>
		[AnimatedProperty()]
		public Vector2 Scale
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the "depth" of the renderable in a depth buffer.
		/// </summary>
		public float Depth
		{
			get;
			set;
		}
		#endregion
	}
}
