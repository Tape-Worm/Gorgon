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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimMath;
using GorgonLibrary.Math;

namespace GorgonLibrary.Graphics.Renderers
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
		: GorgonRenderable
	{
		#region Variables.
		private float[] _corners = null;												// Corners for the line.
		private RectangleF _line = RectangleF.Empty;									// Line dimensions.
		private Vector2 _textureEnd = Vector2.Zero;										// Texture offset for the end point.
		private Vector2 _textureStart = Vector2.Zero;									// Texture offset for the start point.
		private Vector2 _anchor = Vector2.Zero;											// Anchor point for rotation and scaling.
		private Vector2 _penSize = new Vector2(1.0f);									// Pen size for line.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the type of primitive for the renderable.
		/// </summary>
		protected internal override PrimitiveType PrimitiveType
		{
			get 
			{
				if ((_penSize.X == 1.0f) && (_penSize.Y == 1.0f))
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
				if ((_penSize.X == 1.0f) && (_penSize.Y == 1.0f))
					return 0;
				else
					return 6;
			}
		}

		/// <summary>
		/// Property to set or return the index buffer for this renderable.
		/// </summary>
		protected internal override GorgonIndexBuffer IndexBuffer
		{
			get
			{
				if ((_penSize.X == 1.0f) && (_penSize.Y == 1.0f))
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
		/// Property to set or return the pen size for the line.
		/// </summary>
		public Vector2 PenSize
		{
			get
			{
				return _penSize;
			}
			set
			{
				if (_penSize != value)
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
		/// Property to set or return the anchor point of the sprite.
		/// </summary>
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
		/// Property to set or return the position of the line.
		/// </summary>
		public Vector2 StartPoint
		{
			get
			{
				return _line.Location;
			}
			set
			{
				_line.Location = value;
			}
		}

		/// <summary>
		/// Property to set or return the absolute end point for the line.
		/// </summary>
		public Vector2 EndPoint
		{
			get
			{
				return new Vector2(_line.Right, _line.Bottom);
			}
			set
			{
				Line = RectangleF.FromLTRB(StartPoint.X, StartPoint.Y, value.X, value.Y);
			}
		}

		/// <summary>
		/// Property to set or return the size of the renderable.
		/// </summary>
		public Vector2 Size
		{
			get
			{
				return _line.Size;
			}
			set
			{
				if (value != (Vector2)_line.Size)
				{
					_line.Size = value;
					NeedsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the line dimensions for this line object.
		/// </summary>
		public RectangleF Line
		{
			get
			{
				return _line;
			}
			set
			{
				if (_line != value)
				{
					_line = value;
					NeedsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the texture offset for the start point.
		/// </summary>
		public Vector2 TextureOffsetStart
		{
			get
			{
				return _textureStart;
			}
			set
			{
				if (value != _textureStart)
				{
					_textureStart = value;
					NeedsTextureUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the texture offset for the end point.
		/// </summary>
		public Vector2 TextureOffsetEnd
		{
			get
			{
				return _textureEnd;
			}
			set
			{
				if (value != _textureEnd)
				{
					_textureEnd = value;
					NeedsTextureUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the "depth" of the renderable in a depth buffer.
		/// </summary>
		public float Depth
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the angle of rotation (in degrees) for a given axis.
		/// </summary>
		public float Angle
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the texture coordinates.
		/// </summary>
		private void UpdateTextureCoordinates()
		{
			// Calculate texture coordinates.
			Vector2 scaleUV = Vector2.Zero;
			Vector2 offsetUV = Vector2.Zero;
			Vector2 scaledTexture = Vector2.Zero;

			if (Texture == null)
			{
				Vertices[3].UV = Vertices[2].UV = Vertices[0].UV = Vertices[1].UV = Vector2.Zero;
				return;
			}

			if ((PenSize.X == 1.0f) && (PenSize.Y == 1.0f))
			{
				Vertices[0].UV.X = TextureOffsetStart.X / Texture.Settings.Width;
				Vertices[1].UV.X = TextureOffsetEnd.X / Texture.Settings.Width;
				Vertices[0].UV.Y = TextureOffsetStart.Y / Texture.Settings.Height;
				Vertices[1].UV.Y = TextureOffsetEnd.Y / Texture.Settings.Height;
			}
			else
			{
				Vertices[2].UV.X = Vertices[0].UV.X = TextureOffsetStart.X / Texture.Settings.Width;
				Vertices[1].UV.Y = Vertices[0].UV.Y = TextureOffsetStart.Y / Texture.Settings.Height;
				Vertices[3].UV.X = Vertices[1].UV.X = TextureOffsetEnd.X / Texture.Settings.Width;
				Vertices[2].UV.Y = Vertices[3].UV.Y = TextureOffsetEnd.Y / Texture.Settings.Height;
			}
		}

		/// <summary>
		/// Function to update the vertices for the renderable.
		/// </summary>
		private void UpdateVertices()
		{
			_corners[0] = -Anchor.X;
			_corners[1] = -Anchor.Y;
			_corners[2] = Size.X - Anchor.X;
			_corners[3] = Size.Y - Anchor.Y;
		}

		/// <summary>
		/// Function to transform the vertices.
		/// </summary>
		private void TransformVertices()
		{
			float posX1;						// Horizontal position 1.
			float posX2;						// Horizontal position 2.
			float posY1;						// Vertical position 1.
			float posY2;						// Vertical position 2.			
			Vector2 penMid = Vector2.Zero;		// Pen mid point.
			bool updateForQuad = false;			// Flag to indicate that we're drawing a quad.

			posX1 = _corners[0];
			posX2 = _corners[2];
			posY1 = _corners[1];
			posY2 = _corners[3];

			if ((_penSize.X > 1.0f) || (_penSize.Y > 1.0f))
			{
				BaseVertexCount = 0;
				VertexCount = 4;

				Vector2.Divide(ref _penSize, 2.0f, out penMid);
				posX1 -= penMid.X;
				posX2 += penMid.X;
				posY1 -= penMid.Y;
				posY2 += penMid.Y;

				updateForQuad = true;
			}
			else
			{
				BaseVertexCount = 2;
				VertexCount = 2;
			}

			// Calculate rotation if necessary.
			if (Angle != 0.0f)
			{
				float angle = GorgonMathUtility.Radians(Angle);		// Angle in radians.
				float cosVal = (float)System.Math.Cos(angle);		// Cached cosine.
				float sinVal = (float)System.Math.Sin(angle);		// Cached sine.

				if (!updateForQuad)
				{
					Vertices[0].Position.X = (posX1 * cosVal - posY1 * sinVal);
					Vertices[0].Position.Y = (posX1 * sinVal + posY1 * cosVal);

					Vertices[1].Position.X = (posX2 * cosVal - posY2 * sinVal);
					Vertices[1].Position.Y = (posX2 * sinVal + posY2 * cosVal);
				}
				else
				{
					Vertices[0].Position.X = (posX1 * cosVal - posY1 * sinVal);
					Vertices[0].Position.Y = (posX1 * sinVal + posY1 * cosVal);
					Vertices[1].Position.X = (posX2 * cosVal - posY1 * sinVal);
					Vertices[1].Position.Y = (posX2 * sinVal + posY1 * cosVal);
					Vertices[2].Position.X = (posX1 * cosVal - posY2 * sinVal);
					Vertices[2].Position.Y = (posX1 * sinVal + posY2 * cosVal);
					Vertices[3].Position.X = (posX2 * cosVal - posY2 * sinVal);
					Vertices[3].Position.Y = (posX2 * sinVal + posY2 * cosVal);
				}
			}
			else
			{
				if (!updateForQuad)
				{
					Vertices[0].Position.X = posX1;
					Vertices[0].Position.Y = posY1;
					Vertices[1].Position.X = posX2;
					Vertices[1].Position.Y = posY2;
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
			}

			// Translate.
			if (StartPoint.X != 0.0f)
			{
				Vertices[0].Position.X += StartPoint.X;
				Vertices[1].Position.X += StartPoint.X;
				if (updateForQuad)
				{
					Vertices[2].Position.X += StartPoint.X;
					Vertices[3].Position.X += StartPoint.X;
				}
			}

			if (StartPoint.Y != 0.0f)
			{
				Vertices[0].Position.Y += StartPoint.Y;
				Vertices[1].Position.Y += StartPoint.Y;
				if (updateForQuad)
				{
					Vertices[2].Position.Y += StartPoint.Y;
					Vertices[3].Position.Y += StartPoint.Y;
				}
			}

			// Apply depth to the sprite.
			if (Depth != 0.0f)
			{
				Vertices[0].Position.Z = Depth;
				Vertices[1].Position.Z = Depth;
				Vertices[2].Position.Z = Depth;
				Vertices[3].Position.Z = Depth;
			}
		}

		/// <summary>
		/// Function to set up any additional information for the renderable.
		/// </summary>
		protected override void InitializeCustomVertexInformation()
		{
			UpdateVertices();
			UpdateTextureCoordinates();
			
			NeedsVertexUpdate = false;
			NeedsTextureUpdate = false;			
		}

		/// <summary>
		/// Function to set the color for a specific vertex on the sprite.
		/// </summary>
		/// <param name="point">Point on the line to set.</param>
		/// <param name="color">Color to set.</param>
		/// <remarks>The <paramref name="point"/> parameter is </remarks>
		public void SetVertexColor(LineEndPoints point, GorgonColor color)
		{
			if (point == LineEndPoints.Start)
				Vertices[0].Color = color;
			else
				Vertices[1].Color = color;
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		public override void Draw()
		{
			if (NeedsTextureUpdate)
			{
				UpdateTextureCoordinates();
				NeedsTextureUpdate = false;
			}

			if (NeedsVertexUpdate)
			{
				UpdateVertices();
				NeedsVertexUpdate = false;
			}

			TransformVertices();

			base.Draw();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonLine"/> class.
		/// </summary>
		/// <param name="gorgon2D">Gorgon interface that owns this renderable.</param>
		/// <param name="name">The name of the line.</param>
		/// <param name="start">Line starting point.</param>
		/// <param name="end">Line ending point.</param>
		internal GorgonLine(Gorgon2D gorgon2D, string name, Vector2 start, Vector2 end)
			: base(gorgon2D, name)
		{
			_corners = new float[4];
			_textureEnd = end;
			StartPoint = start;
			EndPoint = end;			
			InitializeVertices(4);
			PenSize = new Vector2(1);
		}
		#endregion
	}
}
