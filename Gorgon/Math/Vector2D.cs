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
// Created: Saturday, April 19, 2008 11:44:54 AM
// 
#endregion

using System;
using System.Drawing;

namespace GorgonLibrary
{
	/// <summary>
	/// Value type to represent a 2 dimensional vector.
	/// </summary>
	/// <remarks>
	/// Vector mathematics are commonly used in graphical 3D applications.  And other
	/// spatial related computations.
	/// This type provides us a convenient way to use vectors and their operations.
	/// </remarks>
	public struct Vector2D
	{
		#region Variables.
		// Static vector declarations to minimize object creation.
		private readonly static Vector2D _zero = new Vector2D(0,0);
		private readonly static Vector2D _unit = new Vector2D(1.0f,1.0f);
		private readonly static Vector2D _unitX = new Vector2D(1.0f,0);
		private readonly static Vector2D _unitY = new Vector2D(0,1.0f);

		/// <summary>
		/// Horizontal position of the vector.
		/// </summary>
		public float X;
		/// <summary>
		/// Vertical position of the vector.
		/// </summary>
		public float Y;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the length of this vector.
		/// </summary>
		public float Length
		{
			get
			{
				return MathUtility.Sqrt(X * X + Y * Y);
			}
		}

		/// <summary>
		/// Property to return the length of this vector squared.
		/// </summary>
		public float LengthSquare
		{
			get
			{
				return X * X + Y * Y;
			}
		}

		/// <summary>
		/// Property to return the inverse length of this vector.
		/// </summary>
		public float InverseLength
		{
			get
			{
				float len = Length;  // Length of vector.

				// If the length is EXACTLY zero, then with zero, however this shouldn't happen.
				if (len == 0.0) 
					return 0;

				return 1.0f/len;
			}
		}

		/// <summary>
		/// Property to return a zeroed vector.
		/// </summary>
		public static Vector2D Zero
		{
			get
			{
				return _zero;
			}
		}

		/// <summary>
		/// Property to return a unit vector (1,1,1).
		/// </summary>
		public static Vector2D Unit
		{
			get
			{
				return _unit;
			}
		}
		
		/// <summary>
		/// Property to return a unit vector for the X axis (1,0,0).
		/// </summary>
		public static Vector2D UnitX
		{
			get
			{
				return _unitX;
			}
		}

		/// <summary>
		/// Property to return a unit vector for the Y axis (0,1,0).
		/// </summary>
		public static Vector2D UnitY
		{
			get
			{
				return _unitY;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return the angle between two vectors in radians.
		/// </summary>
		/// <returns>The angle between the vector components in radians.</returns>
		public float Angle()
		{
			return MathUtility.ATan(Y, X);
		}

		/// <summary>
		/// Function to return the angle between two vectors in radians.
		/// </summary>
		/// <param name="vector1">First vector.</param>
		/// <param name="vector2">Second vector.</param>
		/// <returns>The angle between the vectors in radians.</returns>
		public static float Angle(Vector2D vector1, Vector2D vector2)
		{
			vector1.Normalize();
			vector2.Normalize();
			return MathUtility.ACos(Vector2D.DotProduct(vector1, vector2));
		}

		/// <summary>
		/// Function to return a negated vector.
		/// </summary>
		/// <param name="left">Vector to negate.</param>
		/// <returns>A negated vector.</returns>
		public static Vector2D Negate(Vector2D left)
		{
			return new Vector2D(-left.X,-left.Y);
		}
        
        /// <summary>
        /// Function to add two vectors together.
        /// </summary>
        /// <param name="left">Left vector to add.</param>
        /// <param name="right">Right vector to add.</param>
        /// <returns>The total of the two vectors.</returns>
        public static Vector2D Add(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.X + right.X, left.Y + right.Y);
        }

        /// <summary>
        /// Function to subtract two vectors from each other.
        /// </summary>
        /// <param name="left">Left vector to subtract.</param>
        /// <param name="right">Right vector to subtract.</param>
        /// <returns>The difference between the two vectors.</returns>
        public static Vector2D Subtract(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.X - right.X, left.Y - right.Y);
        }

        /// <summary>
        /// Function to divide a vector by another vector.
        /// </summary>
        /// <param name="dividend">The vector to be divided.</param>
        /// <param name="divisor">The vector that will divide the dividend.</param>
        /// <returns>The quotient vector.</returns>
        /// <exception cref="System.DivideByZeroException">One of the components of the divisor was 0.</exception>
        public static Vector2D Divide(Vector2D dividend, Vector2D divisor)
        {
            return new Vector2D(dividend.X/divisor.X,dividend.Y/divisor.Y);
        }

        /// <summary>
        /// Function to divide a vector by a floating point value.
        /// </summary>
        /// <param name="dividend">The vector to be divided.</param>
        /// <param name="divisor">The vector that will divide the dividend.</param>
        /// <returns>The quotient vector.</returns>
        /// <exception cref="System.DivideByZeroException">One of the divisor was 0.</exception>
        public static Vector2D Divide(Vector2D dividend, float divisor)
        {
            return new Vector2D(dividend.X/divisor, dividend.Y/divisor);
        }

        /// <summary>
        /// Function to multiply two vectors together.
        /// </summary>
        /// <param name="left">Left vector to multiply.</param>
        /// <param name="right">Right vector to multiply.</param>
        /// <returns>The product of the two vectors.</returns>
        public static Vector2D Multiply(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.X * right.X, left.Y * right.Y);
        }

        /// <summary>
        /// Function to multiply a vector by a floating point value.
        /// </summary>
        /// <param name="left">Left vector to multiply.</param>
        /// <param name="right">Value to multiply by.</param>
        /// <returns>The product of the two vectors.</returns>
        public static Vector2D Multiply(Vector2D left, float right)
        {
            return new Vector2D(left.X * right, left.Y * right);
        }

		/// <summary>
		/// Function to perform a dot product between this and another vector.
		/// </summary>
        /// <param name="left">Left vector to use in the dot product operation.</param>
        /// <param name="right">Right vector to use in the dot product operation.</param>
		/// <returns>The dot product of the two vectors.</returns>
		public static float DotProduct(Vector2D left, Vector2D right)
		{
			return (left.X * right.X) + (left.Y * right.Y);
		}

		/// <summary>
		/// Function to normalize the vector.
		/// </summary>
        /// <param name="vector">Vector to normalize.</param>
        /// <returns>A normalized vector.</returns>
		public static Vector2D Normalize(Vector2D vector)
		{
            Vector2D result = Vector2D.Zero;        // Result.

			if (vector.Length > float.Epsilon)
			{
				float invLen = vector.InverseLength;

				result.X = vector.X * invLen;
				result.Y = vector.Y * invLen;				
			}

            return result;
		}

        /// <summary>
        /// Function to normalize this vector.
        /// </summary>
        public void Normalize()
        {
            if (Length > float.Epsilon)
            {
                float invLen = InverseLength;

                X *= invLen;
                Y *= invLen;
            }
        }

		/// <summary>
		/// Function to perform a cross product between this and another vector.
		/// </summary>
		/// <param name="vector1">Vector to perform the cross product with.</param>
        /// <param name="vector2">Vector to perform the cross product with.</param>
		/// <returns>A new vector containing the cross product.</returns>
		public static Vector2D CrossProduct(Vector2D vector1, Vector2D vector2)
		{
            return new Vector2D(vector2.X * vector1.Y - vector2.Y * vector1.X, vector2.Y * vector1.X - vector2.X * vector1.Y);
		}

		/// <summary>
		/// Function to compare this vector to another object.
		/// </summary>
		/// <param name="obj">Object to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public override bool Equals(object obj)
		{
			if (obj is Vector2D)
				return (this == (Vector2D)obj);
			else
				return false;
		}

		/// <summary>
		/// Function to return the hash code for this object.
		/// </summary>
		/// <returns>The hash code for this object.</returns>
		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}

		/// <summary>
		/// Function to return the textual representation of this object.
		/// </summary>
		/// <returns>A string containing the type and values of the object.</returns>
		public override string ToString()
		{
			return string.Format("2D Vector:\n\tX:{0}, Y:{1}",X,Y);
		}
		#endregion

        #region Operators.
		/// <summary>
		/// Operator to perform equality tests.
		/// </summary>
		/// <param name="left">Vector to compare.</param>
		/// <param name="right">Vector to compare with.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(Vector2D left,Vector2D right)
		{
			return (MathUtility.EqualFloat(left.X, right.X) && MathUtility.EqualFloat(left.Y, right.Y));
		}

		/// <summary>
		/// Operator to perform inequality tests.
		/// </summary>
		/// <param name="left">Vector to compare.</param>
		/// <param name="right">Vector to compare with.</param>
		/// <returns>TRUE if not equal, FALSE if they are.</returns>
		public static bool operator !=(Vector2D left,Vector2D right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Operator to perform a test to see if the left is less than the right.
		/// </summary>
		/// <param name="left">Left vector to compare.</param>
		/// <param name="right">Right vector to compare.</param>
		/// <returns>TRUE if left is less than right, FALSE if not.</returns>
		public static bool operator <(Vector2D left,Vector2D right)
		{
			return ((left.X < right.X) && (left.Y < right.Y));
		}

		/// <summary>
		/// Operator to perform a test to see if the left is greater than the right.
		/// </summary>
		/// <param name="left">Left vector to compare.</param>
		/// <param name="right">Right vector to compare.</param>
		/// <returns>TRUE if left is greater than right, FALSE if not.</returns>
		public static bool operator >(Vector2D left,Vector2D right)
		{
			return ((left.X > right.X) && (left.Y > right.Y));
		}

		/// <summary>
		/// Operator to perform a test to see if the left is less or equal to the right.
		/// </summary>
		/// <param name="left">Left vector to compare.</param>
		/// <param name="right">Right vector to compare.</param>
		/// <returns>TRUE if left is less than right, FALSE if not.</returns>
		public static bool operator <=(Vector2D left,Vector2D right)
		{
			return ((left.X <= right.X) && (left.Y <= right.Y));
		}

		/// <summary>
		/// Operator to perform a test to see if the left is greater or equal to the right.
		/// </summary>
		/// <param name="left">Left vector to compare.</param>
		/// <param name="right">Right vector to compare.</param>
		/// <returns>TRUE if left is greater than right, FALSE if not.</returns>
		public static bool operator >=(Vector2D left,Vector2D right)
		{
			return ((left.X >= right.X) && (left.Y >= right.Y));
		}

		/// <summary>
		/// Operator to perform addition upon two vectors.
		/// </summary>
		/// <param name="left">Vector to add to.</param>
		/// <param name="right">Vector to add with.</param>
		/// <returns>A new vector.</returns>
		public static Vector2D operator +(Vector2D left, Vector2D right)
		{
			return Add(left, right);
		}

		/// <summary>
		/// Operator to perform subtraction upon two vectors.
		/// </summary>
		/// <param name="left">Vector to subtract.</param>
		/// <param name="right">Vector to subtract with.</param>
		/// <returns>A new vector.</returns>
		public static Vector2D operator -(Vector2D left, Vector2D right)
		{
			return Subtract(left, right);
		}

		/// <summary>
		/// Operator to negate a vector.
		/// </summary>
		/// <param name="left">Vector to negate.</param>
		/// <returns>A negated vector.</returns>
		public static Vector2D operator -(Vector2D left)
		{
			return Negate(left);
		}

		/// <summary>
		/// Operator to multiply two vectors together.
		/// </summary>
		/// <param name="left">Vector to multiply.</param>
		/// <param name="right">Vector to multiply by.</param>
		/// <returns>A new vector.</returns>
		public static Vector2D operator *(Vector2D left, Vector2D right)
		{
			return Multiply(left, right);
		}

		/// <summary>
		/// Operator to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to multiply with.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <returns>A new vector.</returns>
		public static Vector2D operator *(Vector2D left, float scalar)
		{
			return Multiply(left, scalar);
		}

		/// <summary>
		/// Operator to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <param name="right">Vector to multiply with.</param>
		/// <returns>A new vector.</returns>
		public static Vector2D operator *(float scalar, Vector2D right)
		{
			return Multiply(right, scalar);
		}

		/// <summary>
		/// Operator to divide a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="scalar">Scalar value to divide by.</param>
		/// <returns>A new vector.</returns>
		public static Vector2D operator /(Vector2D left, float scalar)
		{
			return Divide(left, scalar);
		}

		/// <summary>
		/// Operator to divide a vector by another vector.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="right">Vector to divide by.</param>
		/// <returns>A new vector.</returns>
		public static Vector2D operator /(Vector2D left, Vector2D right)
		{
			return Divide(left, right);
		}

		/// <summary>
		/// Operator to convert a 3D vector into a 2D vector.
		/// </summary>
		/// <param name="vector">3D gorgon vector.</param>
		/// <returns>A new 2D vector.</returns>
		public static implicit operator Vector2D(Vector3D vector)
		{
			return new Vector2D(vector.X,vector.Y);
		}

		/// <summary>
		/// Operator to convert a System.Drawing.Point to a 2D vector.
		/// </summary>
		/// <param name="point">System.Drawing.Point to convert.</param>
		/// <returns>A new 2D vector.</returns>
		public static implicit operator Vector2D(Point point)
		{
			return new Vector2D(point);
		}

		/// <summary>
		/// Operator to convert a System.Drawing.PointF to a 2D vector.
		/// </summary>
		/// <param name="point">System.Drawing.PointF to convert.</param>
		/// <returns>A new 2D vector.</returns>
		public static implicit operator Vector2D(PointF point)
		{
			return new Vector2D(point);
		}

		/// <summary>
		/// Operator to convert a System.Drawing.Size to a 2D vector.
		/// </summary>
		/// <param name="point">System.Drawing.Size to convert.</param>
		/// <returns>A new 2D vector.</returns>
		public static implicit operator Vector2D(Size point)
		{
			return new Vector2D(point);
		}

		/// <summary>
		/// Operator to convert a System.Drawing.SizeF to a 2D vector.
		/// </summary>
		/// <param name="point">System.Drawing.SizeF to convert.</param>
		/// <returns>A new 2D vector.</returns>
		public static implicit operator Vector2D(SizeF point)
		{
			return new Vector2D(point);
		}

		/// <summary>
		/// Operator to convert a 2D vector to a System.Drawing.Point.
		/// </summary>
		/// <param name="vector">2D gorgon vector.</param>
		/// <returns>A new point with the values from the vector.</returns>
		public static explicit operator Point(Vector2D vector)
		{
			return new Point((int)vector.X, (int)vector.Y);
		}

		/// <summary>
		/// Operator to convert a 2D vector to a System.Drawing.PointF.
		/// </summary>
		/// <param name="vector">2D gorgon vector.</param>
		/// <returns>A new point with the values from the vector.</returns>
		public static implicit operator PointF(Vector2D vector)
		{
			return new PointF(vector.X, vector.Y);
		}

		/// <summary>
		/// Operator to convert a 2D vector to a System.Drawing.Size.
		/// </summary>
		/// <param name="vector">2D gorgon vector.</param>
		/// <returns>A new point with the values from the vector.</returns>
		public static explicit operator Size(Vector2D vector)
		{
			return new Size((int)vector.X, (int)vector.Y);
		}

		/// <summary>
		/// Operator to convert a 2D vector to a System.Drawing.SizeF.
		/// </summary>
		/// <param name="vector">2D gorgon vector.</param>
		/// <returns>A new point with the values from the vector.</returns>
		public static implicit operator SizeF(Vector2D vector)
		{
			return new SizeF(vector.X, vector.Y);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="x">Horizontal position of the vector.</param>
		/// <param name="y">Vertical posiition of the vector.</param>
		public Vector2D(float x,float y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="point">System.Drawing.PointF to initialize with.</param>
		public Vector2D(PointF point)
		{
			X = point.X;
			Y = point.Y;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="point">System.Drawing.Point to initialize with.</param>
		public Vector2D(Point point)
		{
			X = point.X;
			Y = point.Y;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="size">System.Drawing.Size to initialize with.</param>
		public Vector2D(Size size)
		{
			X = size.Width;
			Y = size.Height;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="size">System.Drawing.SizeF to initialize with.</param>
		public Vector2D(SizeF size)
		{
			X = size.Width;
			Y = size.Height;
		}
		#endregion
	}
}
