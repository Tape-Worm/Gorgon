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
// Created: Sunday, February 26, 2012 9:31:44 AM
// 
#endregion

// Portions of this code were adapted from the Vortex 2D graphics library by Alex Khomich.
// Vortex2D.Net is available from http://www.vortex2d.net/

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
	/// A renderable object for drawing an ellipse on the screen.
	/// </summary>
	public class GorgonEllipse
		: GorgonMoveable
	{
		#region Variables.
		private int _quality = 0;								// Quality for the ellipse rendering.
		private Vector2 _center = Vector2.Zero;					// Center point for the ellipse.
		private Vector2[] _offsets = null;						// Offsets for the ellipse points.
		private Vector2[] _points = null;						// List of points for the ellipse.
		private GorgonColor[] _colors = null;					// Colors for points.
		private GorgonLine _line = null;						// List of lines to draw.
		private bool _isFilled = false;							// Flag to indicate whether to draw the ellipse as filled or as an outline.
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
		/// Property to set or return whether the ellipse should be drawn as filled or as an outline.
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

		/// <summary>
		/// Property to set or return the quality level for the ellipse.
		/// </summary>
		/// <remarks>The quality level cannot be less than 4 or greater than 256.</remarks>
		public int Quality
		{
			get
			{
				return _quality;
			}
			set
			{
				if ((value < 4) || (value > 256))
					return;

				if (_quality != value)
				{
					_quality = value;
					BaseVertexCount = _quality * 3;
					VertexCount = _quality * 3;
					InitializeVertices(_quality * 3);
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
				for (int i = 0; i < _colors.Length; i++)
					_colors[i] = value;
			}
		}

		/// <summary>
		/// Property to set or return the thickness of the lines for an outlined ellipse.
		/// </summary>
		/// <remarks>These values cannot be less than 1.</remarks>
		public Vector2 LineThickness
		{
			get
			{
				return _line.LineThickness;
			}
			set
			{
				if (_line.LineThickness != value)
					_line.LineThickness = value;
			}
		}

		/// <summary>
		/// Property to set or return a texture for the renderable.
		/// </summary>
		/// <value></value>
		public override GorgonTexture2D Texture
		{
			get
			{
				return base.Texture;
			}
			set
			{
				if (base.Texture != value)
				{
					base.Texture = value;
					_line.Texture = value;
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the texture coordinates for the unfilled ellipse.
		/// </summary>
		/// <param name="index">Index of the point.</param>
		/// <param name="nextIndex">Index of the next point.</param>
		/// <param name="startUV">Starting UV coordinate.</param>
		/// <param name="endUV">Ending UV coordinate.</param>
		private void UpdateUnfilledTextureCoordinates(int index, int nextIndex, out Vector2 startUV, out Vector2 endUV)
		{
			Vector2 scaledTexture = Vector2.Zero;
			Vector2 scaledPos = Vector2.Zero;

			if (Texture == null)
			{
				startUV = Vector2.Zero;
				endUV = Vector2.Zero;
				return;
			}

			scaledPos = TextureOffset;
			scaledTexture = TextureRegion.Size;

			Vector2.Modulate(ref _offsets[index], ref scaledTexture, out startUV);
			Vector2.Modulate(ref _offsets[nextIndex], ref scaledTexture, out endUV);

			Vector2.Add(ref startUV, ref scaledPos, out startUV);
			Vector2.Add(ref endUV, ref scaledPos, out endUV);

			//startUV.X *= Texture.Settings.Width;
			//startUV.Y *= Texture.Settings.Height;
			//endUV.X *= Texture.Settings.Width;
			//endUV.Y *= Texture.Settings.Height;
		}

		/// <summary>
		/// Function to update the texture coordinates.
		/// </summary>
		protected override void UpdateTextureCoordinates()
		{
			// Calculate texture coordinates.
			Vector2 scaleUV = Vector2.Zero;
			Vector2 offsetUV = Vector2.Zero;
			Vector2 midPoint = Vector2.Zero;
			Vector2 scaledPos = Vector2.Zero;
			Vector2 scaledTexture = Vector2.Zero;
			int vertexIndex = 0;

			if (Texture == null)
			{
				for (int i = 0; i < Vertices.Length; i++)
					Vertices[i].UV = Vector2.Zero;
				return;
			}

			midPoint = new Vector2(TextureRegion.Width / 2.0f, TextureRegion.Height / 2.0f);
			scaledPos = TextureOffset;
			scaledTexture = TextureRegion.Size;
			for (int i = 0; i < _offsets.Length; i++)
			{
				Vector2.Modulate(ref _offsets[i], ref scaledTexture, out offsetUV);

				if (i + 1 < _offsets.Length)
					Vector2.Modulate(ref _offsets[i + 1], ref scaledTexture, out scaleUV);
				else
					Vector2.Modulate(ref _offsets[0], ref scaledTexture, out scaleUV);

				Vector2.Add(ref scaleUV, ref scaledPos, out scaleUV);
				Vector2.Add(ref offsetUV, ref scaledPos, out offsetUV);

				// Set center coordinate.
				Vertices[vertexIndex].UV.X = (TextureOffset.X + midPoint.X);
				Vertices[vertexIndex].UV.Y = (TextureOffset.Y + midPoint.Y);
				Vertices[vertexIndex + 1].UV = offsetUV;
				Vertices[vertexIndex + 2].UV = scaleUV;
				vertexIndex += 3;
			}
		}

		/// <summary>
		/// Function to update the vertices for the renderable.
		/// </summary>
		protected override void UpdateVertices()
		{
			// Set center point.
			_center.X = (0.5f * Size.X - Anchor.X) / 2.0f;
			_center.Y = (0.5f * Size.Y - Anchor.Y) / 2.0f;

			for (int i = 0; i < _points.Length; i++)
			{
				_points[i].X = (_offsets[i].X * (Size.X * 2.0f) - Anchor.X) / 2.0f;
				_points[i].Y = (_offsets[i].Y * (Size.Y * 2.0f) - Anchor.Y) / 2.0f;
			}
		}

		/// <summary>
		/// Function to transform the unfilled vertices.
		/// </summary>
		private Vector2 TransformUnfilled(ref Vector2 vector)
		{
			Vector2 result = vector;

			if (Scale.X != 1.0f)
				result.X *= Scale.X;

			if (Scale.Y != 1.0f)
				result.Y *= Scale.Y;

			if (Angle != 0.0f)
			{
				Vector2 rotVector = result;
				float angle = Angle.Radians();						// Angle in radians.
				float cosVal = angle.Cos();							// Cached cosine.
				float sinVal = angle.Sin();							// Cached sine.

				result.X = (rotVector.X * cosVal - rotVector.Y * sinVal);
				result.Y = (rotVector.X * sinVal + rotVector.Y * cosVal);
			}

			if (Position.X != 0.0f)
				result.X += Position.X;

			if (Position.Y != 0.0f)
				result.Y += Position.Y;

			return result;
		}

		/// <summary>
		/// Function to transform the vertices.
		/// </summary>
		protected override void TransformVertices()
		{
			int vertexIndex = 0;
			Vector2 center = _center;

			for (int i = 0; i < _points.Length; i++)
			{
				Vector2 startPosition = _points[i];
				Vector2 endPosition = Vector2.Zero;
				if (i + 1 < _points.Length)
					endPosition = _points[i + 1];
				else
					endPosition = _points[0];

				if (Scale.X != 1.0f)
				{
					startPosition.X *= Scale.X;
					endPosition.X *= Scale.X;
					center.X *= Scale.X;
				}

				if (Scale.Y != 1.0f)
				{
					startPosition.Y *= Scale.Y;
					endPosition.Y *= Scale.Y;
					center.Y *= Scale.Y;
				}

				if (Angle != 0.0f)
				{
					float angle = Angle.Radians();		// Angle in radians.
					float cosVal = angle.Cos();		// Cached cosine.
					float sinVal = angle.Sin();		// Cached sine.

					Vertices[vertexIndex].Position.X = (center.X * cosVal - center.Y * sinVal);
					Vertices[vertexIndex].Position.Y = (center.X * sinVal + center.Y * cosVal);

					Vertices[vertexIndex + 1].Position.X = (startPosition.X * cosVal - startPosition.Y * sinVal);
					Vertices[vertexIndex + 1].Position.Y = (startPosition.X * sinVal + startPosition.Y * cosVal);

					Vertices[vertexIndex + 2].Position.X = (endPosition.X * cosVal - endPosition.Y * sinVal);
					Vertices[vertexIndex + 2].Position.Y = (endPosition.X * sinVal + endPosition.Y * cosVal);
				}
				else
				{
					Vertices[vertexIndex].Position.X = center.X;
					Vertices[vertexIndex].Position.Y = center.Y;
					Vertices[vertexIndex + 1].Position.X = startPosition.X;
					Vertices[vertexIndex + 1].Position.Y = startPosition.Y;
					Vertices[vertexIndex + 2].Position.X = endPosition.X;
					Vertices[vertexIndex + 2].Position.Y = endPosition.Y;
				}

				if (Position.X != 0.0f)
				{
					Vertices[vertexIndex].Position.X += Position.X;
					Vertices[vertexIndex + 1].Position.X += Position.X;
					Vertices[vertexIndex + 2].Position.X += Position.X;
				}

				if (Position.Y != 0.0f)
				{
					Vertices[vertexIndex].Position.Y += Position.Y;
					Vertices[vertexIndex + 1].Position.Y += Position.Y;
					Vertices[vertexIndex + 2].Position.Y += Position.Y;
				}

				if (Depth != 0.0f)
				{
					Vertices[vertexIndex].Position.Z = Depth;
					Vertices[vertexIndex + 1].Position.Z = Depth;
					Vertices[vertexIndex + 2].Position.Z = Depth;
				}

				Vertices[vertexIndex + 2].Color = Vertices[vertexIndex + 1].Color = Vertices[vertexIndex].Color = _colors[i];

				vertexIndex += 3;
			}
		}

		/// <summary>
		/// Function to set up any additional information for the renderable.
		/// </summary>
		protected override void InitializeCustomVertexInformation()
		{
			float angle = 0.0f;
			float step = (float)System.Math.PI * 2 / (_quality);

			_offsets = new Vector2[_quality];
			_points = new Vector2[_offsets.Length];
			_colors = new GorgonColor[_offsets.Length];

			for (int i = 0; i < _offsets.Length; i++)
			{
				_colors[i] = new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f);
				_offsets[i] = new Vector2(angle.Cos(), angle.Sin()) * 0.5f;
				_offsets[i] += new Vector2(0.5f, 0.5f);
				angle += step;
			}

			UpdateVertices();
			UpdateTextureCoordinates();

			NeedsTextureUpdate = false;
			NeedsVertexUpdate = false;
		}

		/// <summary>
		/// Function to set a color for an individual point on the ellipse.
		/// </summary>
		/// <param name="pointIndex">Index of the point (0 - (Quality-1)).</param>
		/// <param name="color">Color to set.</param>
		/// <remarks>The <paramref name="pointIndex"/> must be between 0 and <see cref="P:GorgonLibrary.Renderers.GorgonEllipse.Quality">Quality - 1</see>.</remarks>
		public void SetPointColor(int pointIndex, GorgonColor color)
		{
			_colors[pointIndex] = color;
		}

		/// <summary>
		/// Function to draw the ellipse.
		/// </summary>
		public override void Draw()
		{
			if (_isFilled)
			{
				base.Draw();
				return;
			}

			// Draw unfilled with a line object.
			if (NeedsVertexUpdate)
			{
				UpdateVertices();
				NeedsVertexUpdate = false;
			}

			_line.Blending = Blending;
			_line.DepthStencil = DepthStencil;
			_line.TextureSampler = TextureSampler;

			for (int i = 0; i < _points.Length; i++)
			{
				int endPointIndex = i + 1;

				if (endPointIndex >= _points.Length)
					endPointIndex = 0;

				Vector2 start = TransformUnfilled(ref _points[i]);
				Vector2 end = TransformUnfilled(ref _points[endPointIndex]);

				Vector2 uvStart = Vector2.Zero;
				Vector2 uvEnd = Vector2.Zero;

				if (Texture != null)
					UpdateUnfilledTextureCoordinates(i, endPointIndex, out uvStart, out uvEnd);
				_line.TextureStart = uvStart;
				_line.TextureEnd = uvEnd;

				_line.StartColor = _colors[i];
				_line.EndColor = _colors[endPointIndex];
				_line.StartPoint = start;
				_line.EndPoint = end;
				_line.Depth = Depth;

				_line.Draw();
			}

			NeedsTextureUpdate = false;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonEllipse"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that created this object.</param>
		/// <param name="name">The name of the object.</param>
		/// <param name="position">The position of the ellipse.</param>
		/// <param name="size">The size of the ellipse.</param>
		/// <param name="color">Color of the ellipse.</param>
		/// <param name="quality">Quality of the ellipse.</param>
		/// <param name="isFilled">TRUE if the ellipse should be filled.</param>
		internal GorgonEllipse(Gorgon2D gorgon2D, string name, Vector2 position, Vector2 size, GorgonColor color, int quality, bool isFilled)
			: base(gorgon2D, name)
		{
			TextureRegion = new System.Drawing.RectangleF(0, 0, size.X, size.Y);
			Position = position;
			Size = size;
			Quality = quality;
			IsFilled = isFilled;
			_line = new GorgonLine(gorgon2D, name + ".Line", Vector2.Zero, Vector2.Zero, color);
			Color = color;
		}
		#endregion
	}
}