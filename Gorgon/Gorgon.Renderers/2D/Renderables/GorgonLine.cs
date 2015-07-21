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
// Created: Sunday, February 26, 2012 8:53:52 AM
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
	/// Points on the line.
	/// </summary>
	public enum LineEndPoints
	{
		/// <summary>
		/// Starting line point.
		/// </summary>
		Start = 0,
		/// <summary>
		/// Ending line point.
		/// </summary>
		End = 1
	}

	/// <summary>
	/// A renderable object for drawing a line on the screen.
	/// </summary>
	public class GorgonLine
		: GorgonRenderable, IMoveable
	{
		#region Variables.
		private readonly float[] _corners;												// Corners for the line.
		private RectangleF _line = RectangleF.Empty;									// Line dimensions.
		private Vector2 _textureEnd = Vector2.Zero;										// Texture offset for the end point.
		private Vector2 _textureStart = Vector2.Zero;									// Texture offset for the start point.
		private Vector2 _anchor = Vector2.Zero;											// Anchor point for rotation and scaling.
		private Vector2 _lineThickness = new Vector2(1.0f);								// Thickness for the line.
		private Vector2 _crossProduct = Vector2.Zero;									// Cross product of the line.
		private bool _isUnitLine = true;												// Flag to indicate that the line is using a single pixel for thickness.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the type of primitive for the renderable.
		/// </summary>
		protected internal override PrimitiveType PrimitiveType => _isUnitLine ? PrimitiveType.LineList : PrimitiveType.TriangleList;

		/// <summary>
		/// Property to return the number of indices that make up this renderable.
		/// </summary>
		protected internal override int IndexCount => _isUnitLine ? 0 : 6;

		/// <summary>
		/// Property to set or return the index buffer for this renderable.
		/// </summary>
		public override GorgonIndexBuffer IndexBuffer => _isUnitLine ? null : Gorgon2D.DefaultIndexBuffer;

		/// <summary>
		/// Property to set or return the thickness for the line.
		/// </summary>
		/// <remarks>This value cannot be less than 1.</remarks>
		[AnimatedProperty]
		public Vector2 LineThickness
		{
			get
			{
				return _lineThickness;
			}
			set
			{
				if (_lineThickness == value)
				{
					return;
				}

				if (value.X < 1.0f)
				{
					value.X = 1.0f;
				}

				if (value.Y < 1.0f)
				{
					value.Y = 1.0f;
				}

				_lineThickness = value;
				_isUnitLine = _lineThickness.X.EqualsEpsilon(1.0f) && _lineThickness.Y.EqualsEpsilon(1.0f);
				NeedsTextureUpdate = true;
				NeedsVertexUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the color for the start point.
		/// </summary>
		[AnimatedProperty]
		public GorgonColor StartColor
		{
			get
			{
				return Vertices[0].Color;
			}
			set
			{
				if ((GorgonColor.Equals(ref value, ref Vertices[2].Color)) && (GorgonColor.Equals(ref value, ref Vertices[0].Color)))
				{
					return;
				}
					
				Vertices[2].Color = Vertices[0].Color = value;
			}
		}

		/// <summary>
		/// Property to set or return the color for the end point.
		/// </summary>
		[AnimatedProperty]
		public GorgonColor EndColor
		{
			get
			{
				return Vertices[1].Color;
			}
			set
			{
				if ((GorgonColor.Equals(ref value, ref Vertices[3].Color)) && (GorgonColor.Equals(ref value, ref Vertices[1].Color)))
				{
					return;
				}

				Vertices[3].Color = Vertices[1].Color = value;
			}
		}

		/// <summary>
		/// Property to set or return the position of the line.
		/// </summary>
		[AnimatedProperty]
		public Vector2 StartPoint
		{
			get
			{
				return _line.Location;
			}
			set
			{
				LineRegion = RectangleF.FromLTRB(value.X, value.Y, EndPoint.X, EndPoint.Y);
			}
		}

		/// <summary>
		/// Property to set or return the absolute end point for the line.
		/// </summary>
		[AnimatedProperty]
		public Vector2 EndPoint
		{
			get
			{
				return new Vector2(_line.Right, _line.Bottom);
			}
			set
			{
				LineRegion = RectangleF.FromLTRB(StartPoint.X, StartPoint.Y, value.X, value.Y);
			}
		}

		/// <summary>
		/// Property to set or return the size of the renderable.
		/// </summary>
		[AnimatedProperty]
		public Vector2 Size
		{
			get
			{
				return _line.Size;
			}
			set
			{
				if (value.Equals(_line.Size))
				{
					return;
				}

				_line.Size = value;
				NeedsVertexUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the line dimensions for this line object.
		/// </summary>
		public RectangleF LineRegion
		{
			get
			{
				return _line;
			}
			set
			{
				if (_line == value)
				{
					return;
				}

				_line = value;
				NeedsVertexUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the texture region.
		/// </summary>
		/// <remarks>This texture value is in texel space (0..1).</remarks>
		public override RectangleF TextureRegion
		{
			get
			{
				return RectangleF.FromLTRB(_textureStart.X, _textureStart.Y, _textureEnd.X, _textureEnd.Y);
			}
			set
			{
				if ((_textureStart.Equals(value.Location))
					&& (_textureEnd.Equals(new Vector2(value.Right, value.Bottom))))
				{
					return;
				}

				_textureStart = value.Location;
				_textureEnd = new Vector2(value.Right, value.Bottom);
				NeedsTextureUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the coordinates in the texture to use as a starting point for drawing.
		/// </summary>
		[AnimatedProperty]
		public override Vector2 TextureOffset
		{
			get
			{
				return _textureStart;
			}
			set
			{
				if (TextureStart == value)
				{
					return;
				}
				
				TextureStart = value;
				NeedsTextureUpdate = true;
			}
		}

        /// <summary>
        /// Property to set or return the scaling of the texture width and height.
        /// </summary>
        /// <remarks>
        /// This texture value is in texel space (0..1).
        /// </remarks>
		[AnimatedProperty]
		public override Vector2 TextureSize
		{
			get
			{
				return Vector2.Subtract(_textureEnd, _textureStart);
			}
			set
			{
				value = Vector2.Add(_textureStart, value);
				if (value == _textureEnd)
				{
					return;
				}

				_textureEnd = Vector2.Add(_textureStart, _textureEnd);
				NeedsTextureUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the texture offset for the start point.
		/// </summary>
		/// <remarks>This texture value is in texel space (0..1).</remarks>
		[AnimatedProperty]
		public Vector2 TextureStart
		{
			get
			{
				return _textureStart;
			}
			set
			{
				if (value == _textureStart)
				{
					return;
				}

				_textureStart = value;
				NeedsTextureUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the texture offset for the end point.
		/// </summary>
		/// <remarks>This texture value is in texel space (0..1) and is an absolute value (i.e. not relative to the starting position).</remarks>
		[AnimatedProperty]
		public Vector2 TextureEnd
		{
			get
			{
				return _textureEnd;
			}
			set
			{
				if (value == _textureEnd)
				{
					return;
				}

				_textureEnd = value;
				NeedsTextureUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the "depth" of the renderable in a depth buffer.
		/// </summary>
		[AnimatedProperty]
		public float Depth
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the angle of rotation (in degrees) for the line.
		/// </summary>
		[AnimatedProperty]
		public float Angle
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to transform the line quad.
		/// </summary>
		private void TransformQuad()
		{
			Vector2 corner1 = Vector2.Zero;											// 1st corner.
			Vector2 corner2 = Vector2.Zero;											// 2nd corner.
			Vector2 corner3 = Vector2.Zero;											// 3rd corner.
			Vector2 corner4 = Vector2.Zero;											// 4th corner.

			BaseVertexCount = 0;
			VertexCount = 4;

			corner1.X = _corners[0] + _crossProduct.X;
			corner1.Y = _corners[1] + _crossProduct.Y;

			corner2.X = _corners[0] - _crossProduct.X;
			corner2.Y = _corners[1] - _crossProduct.Y;

			corner3.X = _corners[2] + _crossProduct.X;
			corner3.Y = _corners[3] + _crossProduct.Y;

			corner4.X = _corners[2] - _crossProduct.X;
			corner4.Y = _corners[3] - _crossProduct.Y;
            
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (Angle != 0.0f)
			{
				float angle = Angle.ToRadians();						// Angle in radians.
				float cosVal = angle.Cos();							// Cached cosine.
				float sinVal = angle.Sin();							// Cached sine.

				Vertices[0].Position.X = (corner1.X * cosVal - corner1.Y * sinVal) + _line.Location.X;
				Vertices[0].Position.Y = (corner1.X * sinVal + corner1.Y * cosVal) + _line.Location.Y;
				Vertices[1].Position.X = (corner3.X * cosVal - corner3.Y * sinVal) + _line.Location.X;
				Vertices[1].Position.Y = (corner3.X * sinVal + corner3.Y * cosVal) + _line.Location.Y;
				Vertices[2].Position.X = (corner2.X * cosVal - corner2.Y * sinVal) + _line.Location.X;
				Vertices[2].Position.Y = (corner2.X * sinVal + corner2.Y * cosVal) + _line.Location.Y;
				Vertices[3].Position.X = (corner4.X * cosVal - corner4.Y * sinVal) + _line.Location.X;
				Vertices[3].Position.Y = (corner4.X * sinVal + corner4.Y * cosVal) + _line.Location.Y;
			}
			else
			{
				Vertices[0].Position.X = corner1.X + _line.Location.X;
				Vertices[0].Position.Y = corner1.Y + _line.Location.Y;
				Vertices[1].Position.X = corner3.X + _line.Location.X;
                Vertices[1].Position.Y = corner3.Y + _line.Location.Y;
                Vertices[2].Position.X = corner2.X + _line.Location.X;
                Vertices[2].Position.Y = corner2.Y + _line.Location.Y;
                Vertices[3].Position.X = corner4.X + _line.Location.X;
                Vertices[3].Position.Y = corner4.Y + _line.Location.Y;
			}

			Vertices[0].Position.Z = Depth;
			Vertices[1].Position.Z = Depth;
			Vertices[2].Position.Z = Depth;
			Vertices[3].Position.Z = Depth;
		}

		/// <summary>
		/// Function to transform the vertices.
		/// </summary>
		private void TransformVertices()
		{
			if ((_lineThickness.X > 1.0f) || (_lineThickness.Y > 1.0f))
			{
				TransformQuad();
				return;
			}

			BaseVertexCount = 2;
			VertexCount = 2;

			float posX1 = _corners[0];
			float posX2 = _corners[2];
			float posY1 = _corners[1];
			float posY2 = _corners[3];

			// Calculate rotation if necessary.
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (Angle != 0.0f)
			{
				float angle = Angle.ToRadians();						// Angle in radians.
				float cosVal = angle.Cos();							// Cached cosine.
				float sinVal = angle.Sin();							// Cached sine.

				Vertices[0].Position.X = (posX1 * cosVal - posY1 * sinVal) + _line.Location.X;
				Vertices[0].Position.Y = (posX1 * sinVal + posY1 * cosVal) + _line.Location.Y;

				Vertices[1].Position.X = (posX2 * cosVal - posY2 * sinVal) + _line.Location.X;
				Vertices[1].Position.Y = (posX2 * sinVal + posY2 * cosVal) + _line.Location.Y;
			}
			else
			{
				Vertices[0].Position.X = posX1 + _line.Location.X;
				Vertices[0].Position.Y = posY1 + _line.Location.Y;
				Vertices[1].Position.X = posX2 + _line.Location.X;
				Vertices[1].Position.Y = posY2 + _line.Location.Y;
			}

			Vertices[0].Position.Z = Depth;
			Vertices[1].Position.Z = Depth;
		}

        /// <summary>
        /// Function to update the texture coordinates.
        /// </summary>
        protected override void UpdateTextureCoordinates()
        {
            Vector2 textureDims;
	        Vector2 textureCrossProduct = Vector2.Zero;

	        if (Texture == null)
            {
                Vertices[3].UV = Vertices[2].UV = Vertices[0].UV = Vertices[1].UV = Vector2.Zero;
                return;
            }

            Vector2.Subtract(ref _textureEnd, ref _textureStart, out textureDims);
            float length = textureDims.Length;

            if (length > 0)
            {
                Vector2 textureNormal = textureDims * (1.0f / length);
                textureCrossProduct = new Vector2(textureNormal.Y, -textureNormal.X);
                textureCrossProduct.X *= LineThickness.X / Texture.Settings.Width;
                textureCrossProduct.Y *= LineThickness.Y / Texture.Settings.Height;
            }

            if (_isUnitLine)
            {
                Vector2.Add(ref _textureStart, ref textureCrossProduct, out Vertices[0].UV);
                Vector2.Add(ref _textureEnd, ref textureCrossProduct, out Vertices[1].UV);
            }
            else
            {
                Vector2.Add(ref _textureStart, ref textureCrossProduct, out Vertices[0].UV);
                Vector2.Add(ref _textureEnd, ref textureCrossProduct, out Vertices[1].UV);
                Vector2.Subtract(ref _textureStart, ref textureCrossProduct, out Vertices[2].UV);
                Vector2.Subtract(ref _textureEnd, ref textureCrossProduct, out Vertices[3].UV);
            }
        }

        /// <summary>
        /// Function to update the vertices for the renderable.
        /// </summary>
        protected override void UpdateVertices()
        {
            var lineDims = new Vector2(_line.Right - _line.Left, _line.Bottom - _line.Top);
            float lineLength = lineDims.Length;

	        if (lineLength > 0)
	        {
		        Vector2 lineNormal = lineDims * (1.0f / lineDims.Length);

		        _crossProduct = new Vector2(lineNormal.Y, -lineNormal.X);
		        _crossProduct.X *= LineThickness.X;
		        _crossProduct.Y *= LineThickness.Y;
	        }
	        else
	        {
		        _crossProduct = Vector2.Zero;
	        }

	        _corners[0] = -Anchor.X;
            _corners[1] = -Anchor.Y;
            _corners[2] = _line.Width - Anchor.X;
            _corners[3] = _line.Height - Anchor.Y;
        }
        
        /// <summary>
		/// Function to set the color for a specific vertex on the line.
		/// </summary>
		/// <param name="point">Point on the line to set.</param>
		/// <param name="color">Color to set.</param>
		/// <remarks>The <paramref name="point"/> parameter is </remarks>
		public void SetVertexColor(LineEndPoints point, GorgonColor color)
		{
	        if (point == LineEndPoints.Start)
	        {
		        Vertices[0].Color = color;
	        }
	        else
	        {
		        Vertices[1].Color = color;
	        }
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		public override void Draw()
		{
			if (NeedsVertexUpdate)
			{
				UpdateVertices();
				NeedsVertexUpdate = false;
			}

			if (NeedsTextureUpdate)
			{
				UpdateTextureCoordinates();
				NeedsTextureUpdate = false;
			}

			TransformVertices();

			AddToRenderQueue();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonLine"/> class.
		/// </summary>
		/// <param name="gorgon2D">Gorgon interface that owns this renderable.</param>
		/// <param name="name">The name of the line.</param>
		internal GorgonLine(Gorgon2D gorgon2D, string name)
			: base(gorgon2D, name)
		{
			_corners = new float[4];
			InitializeVertices(4);
			LineThickness = new Vector2(1);
		}
		#endregion

		#region IMoveable Members
		/// <summary>
		/// Property to set or return the position of the renderable.
		/// </summary>
		Vector2 IMoveable.Position
		{
			get
			{
				return StartPoint;
			}
			set
			{
				StartPoint = value;
			}
		}

		/// <summary>
		/// Property to set or return the scale of the renderable.
		/// </summary>
		Vector2 IMoveable.Scale
		{
			get
			{
				return new Vector2(1);
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the anchor point of the line.
		/// </summary>
		[AnimatedProperty]
		public Vector2 Anchor
		{
			get
			{
				return _anchor;
			}
			set
			{
				if (_anchor == value)
				{
					return;
				}

				_anchor = value;
				NeedsVertexUpdate = true;
			}
		}
		#endregion
	}
}