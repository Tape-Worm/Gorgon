#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Sunday, May 04, 2008 5:13:19 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GorgonLibrary
{
	/// <summary>
	/// Value type representing a bounding circle.
	/// </summary>
	public struct BoundingCircle
	{
		#region Variables.
		
		private bool _empty;																// Flag to indicate that the circle is empty.
		private float _radius;																// Radius of the bounding circle.
		private Vector2D _center;															// Center of the bounding circle.

		/// <summary>
		/// Property to return an empty bounding circle.
		/// </summary>
		public static readonly BoundingCircle Empty = new BoundingCircle(true);
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the bounding circle is empty or not.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return _empty;
			}
		}

		/// <summary>
		/// Property to set or return the radius of the bounding circle.
		/// </summary>
		public float Radius
		{
			get
			{
				return _radius;
			}
			set
			{
				_radius = value;
				_empty = false;
			}
		}

		/// <summary>
		/// Property to set or return the center of the bounding circle.
		/// </summary>
		public Vector2D Center
		{
			get
			{
				return _center;
			}
			set
			{
				_center = value;
				_empty = false;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if a point intersects the bounding circle.
		/// </summary>
		/// <param name="point">Point to test.</param>
		/// <returns>TRUE if the point is inside the circle, FALSE if not.</returns>
		public bool Intersects(Vector2D point)
		{
			Vector2D intersectPoint = Vector2D.Subtract(point,Center);		// Point of intersection.

			return ((!_empty) &&  (intersectPoint.Length <= Radius));
		}

		/// <summary>
		/// Function to determine if a rectangle intersects with the circle.
		/// </summary>
		/// <param name="rect">Rectangle to test.</param>
		/// <returns>TRUE if an intersection occured, FALSE if not.</returns>
		public bool Intersects(Rectangle rect)
		{
			// This uses an axis seperation axis method of checking for rectangle/circle collision.
			Vector2D[] points = { new Vector2D(rect.Left, rect.Top),
								new Vector2D(rect.Right, rect.Top),
								new Vector2D(rect.Left, rect.Bottom),
								new Vector2D(rect.Right, rect.Bottom)};											// Points.
			Vector2D[] axis = new Vector2D[6];																	// Axis;

			// Don't test empty structures.
			if ((_empty) || (rect.IsEmpty))
				return false;

			for (int i = 0; i < points.Length; i++)
			{
				axis[i] = Vector2D.Normalize(Vector2D.Subtract(points[i],Center));
				points[i] = Vector2D.Subtract(points[i], Center);
			}

			axis[points.Length] = Vector2D.UnitX;
			axis[points.Length + 1] = Vector2D.UnitY;

			for (int i=0; i < axis.Length; i++)
			{
				Vector2D minMax = new Vector2D(float.MaxValue, float.MinValue);		// Min/max value.				

				for (int j = 0; j < points.Length; j++)
				{
					float projected = Vector2D.DotProduct(points[j], axis[i]);	// Calculate the angle between the axis and lines.

					if (projected < minMax.X)
						minMax.X = projected;
					if (projected > minMax.Y)
						minMax.Y = projected;
				}

				if ((minMax.X > Radius) || (minMax.Y < -Radius))
					return false;
			}
		
			return true;
		}

		/// <summary>
		/// Function to determine if a bound circle intersects with this circle.
		/// </summary>
		/// <param name="circle">Circle to test.</param>
		/// <returns>TRUE if an intersection occured, FALSE if not.</returns>
		public bool Intersects(BoundingCircle circle)
		{
			Vector2D intersectPoint = Vector2D.Subtract(circle.Center, _center);		// Point of intersection.

			if (circle.IsEmpty)
				return false;

			return ((!_empty) && (intersectPoint.Length <= Radius + circle.Radius));
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">Another object to compare to.</param>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is BoundingCircle)
			{
				BoundingCircle circle = (BoundingCircle)obj;		// Bounding circle.

				if ((circle._empty) && (_empty))
					return true;

				if ((circle._empty) || (_empty))
					return false;

				if ((circle._center == _center) && (circle._radius == _radius))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> containing a fully qualified type name.
		/// </returns>
		public override string ToString()
		{
			return string.Format("Bounding Circle:\n\tCenter: {0},{1}\n\tRadius: {2}", _center.X, _center.Y, _radius);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return _center.GetHashCode() ^ _radius.GetHashCode();
		}		
		#endregion

		#region Operators.
		/// <summary>
		/// Operator to test bounding circles for equality.
		/// </summary>
		/// <param name="left">Left circle to test.</param>
		/// <param name="right">Right circle to test.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(BoundingCircle left, BoundingCircle right)
		{
			if ((left._empty) && (right._empty))
				return true;

			if ((left._empty) || (right._empty))
				return false;

			if ((left.Center == right.Center) && (MathUtility.EqualFloat(left.Radius, right.Radius)))
				return true;

			return false;
		}

		/// <summary>
		/// Operator to test bounding circles for inequality.
		/// </summary>
		/// <param name="left">Left circle to test.</param>
		/// <param name="right">Right circle to test.</param>
		/// <returns>TRUE if not equal, FALSE if not.</returns>
		public static bool operator !=(BoundingCircle left, BoundingCircle right)
		{
			return !(left == right);
		}	
		#endregion

		#region Constructor/Destructor.

		/// <summary>
		/// Initializes a new instance of the <see cref="BoundingCircle"/> struct.
		/// </summary>
		/// <param name="empty">TRUE to mark as empty, FALSE for non-empty.</param>
		private BoundingCircle(bool empty)
		{
			_empty = empty;
			_center = Vector2D.Zero;
			_radius = float.MinValue;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BoundingCircle"/> struct.
		/// </summary>
		/// <param name="center">The center of the bounding circle.</param>
		/// <param name="radius">The radius of the bounding circle.</param>
		public BoundingCircle(Vector2D center, float radius)
		{
			_empty = false;
			_center = center;
			_radius = radius;
		}
		#endregion
	}
}
