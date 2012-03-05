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
// Created: Sunday, February 26, 2012 9:33:35 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimMath;

namespace GorgonLibrary.Graphics.Renderers
{
	/// <summary>
	/// Interface for renderable objects.
	/// </summary>
	/// <remarks>This interface is used to create renderable objects such as sprites and primitives.
	/// <para>Some primitives can be created as filled or unfilled, but each of those primitives have an IsFilled property which can toggle the fill state of the primitive.</para>
	/// </remarks>
	public class GorgonRenderables
	{
		#region Variables.
		private Gorgon2D _gorgon2D = null;			// Gorgon 2D interface.
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a new sprite object.
		/// </summary>
		/// <param name="name">Name of the sprite.</param>
		/// <param name="width">Width of the sprite.</param>
		/// <param name="height">Height of the sprite.</param>
		/// <returns>A new sprite.</returns>
		public GorgonSprite CreateSprite(string name, float width, float height)
		{
			return new GorgonSprite(_gorgon2D, name, width, height);
		}

		/// <summary>
		/// Function to create a new triangle object.
		/// </summary>
		/// <param name="name">Name of the triangle.</param>
		/// <param name="point1">First point in the triangle.</param>
		/// <param name="point2">Second point in the triangle.</param>
		/// <param name="point3">Third point in the triangle.</param>
		/// <param name="color">Color of the triangle.</param>
		/// <param name="filled">TRUE to create a filled triangle, FALSE to create an unfilled triangle.</param>
		/// <returns></returns>
		public GorgonTriangle CreateTriangle(string name, Vector2 point1, Vector2 point2, Vector2 point3, GorgonColor color, bool filled)
		{
			return new GorgonTriangle(_gorgon2D, name, point1, point2, point3, color, filled);
		}

		/// <summary>
		/// Function to create a rectangle object.
		/// </summary>
		/// <param name="name">Name of the rectangle.</param>
		/// <param name="rectangle">Rectangle dimensions.</param>
		/// <param name="color">Color of the rectangle.</param>
		/// <param name="filled">TRUE to create a filled rectangle, FALSE to create an empty rectangle.</param>
		/// <returns>A new rectangle primitive object.</returns>
		public GorgonRectangle CreateRectangle(string name, RectangleF rectangle, GorgonColor color, bool filled)
		{
			return new GorgonRectangle(_gorgon2D, name, rectangle, color, filled);
		}

		/// <summary>
		/// Function to create a rectangle object.
		/// </summary>
		/// <param name="name">Name of the rectangle.</param>
		/// <param name="rectangle">Rectangle dimensions.</param>
		/// <param name="color">Color of the rectangle.</param>
		/// <returns>A new rectangle primitive object.</returns>
		/// <remarks>This method will create an unfilled rectangle.</remarks>
		public GorgonRectangle CreateRectangle(string name, RectangleF rectangle, GorgonColor color)
		{
			return CreateRectangle(name, rectangle, color, false);
		}

		/// <summary>
		/// Function to create a line object.
		/// </summary>
		/// <param name="name">Name of the line.</param>
		/// <param name="startPosition">Starting point for the line.</param>
		/// <param name="endPosition">Ending point for the line.</param>
		/// <param name="color">Color of the line.</param>
		/// <returns>A new line primitive object.</returns>
		public GorgonLine CreateLine(string name, Vector2 startPosition, Vector2 endPosition, GorgonColor color)
		{
			return new GorgonLine(_gorgon2D, name, startPosition, endPosition, color);
		}

		/// <summary>
		/// Function to create a point object.
		/// </summary>
		/// <param name="name">Name of the point.</param>
		/// <param name="position">The position of the point.</param>
		/// <param name="color">Color of the point.</param>
		/// <returns>A new point primitive object.</returns>
		public GorgonPoint CreatePoint(string name, Vector2 position, GorgonColor color)
		{
			return new GorgonPoint(_gorgon2D, name, position, color);
		}

		/// <summary>
		/// Function to create an ellipse object.
		/// </summary>
		/// <param name="name">Name of the ellipse.</param>
		/// <param name="position">Position of the ellipse.</param>
		/// <param name="size">Size of the ellipse.</param>
		/// <param name="color">Color of the ellipse.</param>
		/// <param name="isFilled">TRUE if the ellipse should be filled, FALSE if not.</param>
		/// <param name="quality">Quality of the ellipse rendering.</param>
		/// <returns>A new ellipse object.</returns>
		public GorgonEllipse CreateEllipse(string name, Vector2 position, Vector2 size, GorgonColor color, bool isFilled, int quality)
		{
			return new GorgonEllipse(_gorgon2D, name, position, size, color, quality, isFilled);
		}

		/// <summary>
		/// Function to create an ellipse object.
		/// </summary>
		/// <param name="name">Name of the ellipse.</param>
		/// <param name="position">Position of the ellipse.</param>
		/// <param name="size">Size of the ellipse.</param>
		/// <param name="color">Color of the ellipse.</param>
		/// <param name="isFilled">TRUE if the ellipse should be filled, FALSE if not.</param>
		/// <returns>A new ellipse object.</returns>
		/// <remarks>This method creates an ellipse with a quality of 64 segments.</remarks>
		public GorgonEllipse CreateEllipse(string name, Vector2 position, Vector2 size, GorgonColor color, bool isFilled)
		{
			return CreateEllipse(name, position, size, color, isFilled, 64);
		}

		/// <summary>
		/// Function to create an ellipse object.
		/// </summary>
		/// <param name="name">Name of the ellipse.</param>
		/// <param name="position">Position of the ellipse.</param>
		/// <param name="size">Size of the ellipse.</param>
		/// <param name="color">Color of the ellipse.</param>
		/// <returns>A new ellipse object.</returns>
		/// <remarks>This method creates an unfilled ellipse with a quality of 64 segments.</remarks>
		public GorgonEllipse CreateEllipse(string name, Vector2 position, Vector2 size, GorgonColor color)
		{
			return CreateEllipse(name, position, size, color, false, 64);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderables"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that owns this object.</param>
		internal GorgonRenderables(Gorgon2D gorgon2D)
		{
			_gorgon2D = gorgon2D;
		}
		#endregion
	}
}
