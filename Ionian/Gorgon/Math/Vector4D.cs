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
// Created: Saturday, April 19, 2008 11:45:03 AM
// 
#endregion

using System;

namespace GorgonLibrary
{
	/// <summary>
	/// Value type to represent a 4 dimensional vector.
	/// </summary>
	/// <remarks>
	/// Vector mathematics are commonly used in graphical 3D applications.  And other
	/// spatial related computations.
	/// This valuetype provides us a convienient way to use vectors and their operations.
	/// </remarks>
	public struct Vector4D
	{
		#region Variables.
		// Static vector declarations to minimize object creation.
		private readonly static Vector4D _zero = new Vector4D(0,0,0,1.0f);
		private readonly static Vector4D _unit = new Vector4D(1.0f,1.0f,1.0f,1.0f);
		private readonly static Vector4D _unitX = new Vector4D(1.0f,0,0,1.0f);
		private readonly static Vector4D _unitY = new Vector4D(0,1.0f,0,1.0f);
		private readonly static Vector4D _unitZ = new Vector4D(0,0,1.0f,1.0f);

		/// <summary>
		/// Horizontal position of the vector.
		/// </summary>
		public float X;
		/// <summary>
		/// Vertical position of the vector.
		/// </summary>
		public float Y;
		/// <summary>
		/// Depth position of the vector.
		/// </summary>
		public float Z;
		/// <summary>
		/// Homogeneous unit.
		/// </summary>
		public float W;
		#endregion

		#region Properties.	
		/// <summary>
		/// Property to return the length of this vector.
		/// </summary>
		public float Length
		{
			get
			{
				return MathUtility.Sqrt(X * X + Y * Y + Z * Z + W * W);
			}
		}

		/// <summary>
		/// Property to return the length of this vector squared.
		/// </summary>
		public float LengthSquare
		{
			get
			{
				return X * X + Y * Y + Z * Z + W * W;
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
		public static Vector4D Zero
		{
			get
			{
				return _zero;
			}
		}

		/// <summary>
		/// Property to return a unit vector (1,1,1,1).
		/// </summary>
		public static Vector4D Unit
		{
			get
			{
				return _unit;
			}
		}
		
		/// <summary>
		/// Property to return a unit vector for the X axis (1,0,0,1).
		/// </summary>
		public static Vector4D UnitX
		{
			get
			{
				return _unitX;
			}
		}

		/// <summary>
		/// Property to return a unit vector for the Y axis (0,1,0,1).
		/// </summary>
		public static Vector4D UnitY
		{
			get
			{
				return _unitY;
			}
		}

		/// <summary>
		/// Property to return a unit vector for the X axis (0,0,1,1).
		/// </summary>
		public static Vector4D UnitZ
		{
			get
			{
				return _unitZ;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform a dot product between this and another vector.
		/// </summary>
		/// <param name="vector">Vector to get the dot product against.</param>
		/// <returns>The dot product.</returns>
		public float DotProduct(Vector4D vector)
		{
			double result = 0.0;		// Resultant.

			result += X * vector.X;
			result += Y * vector.Y;
			result += Z * vector.Z;
			result += W * vector.W;

			return (float)result; 
		}

		/// <summary>
		/// Function to perform a cross product between this and another vector.
		/// </summary>
		/// <param name="vector">Vector to perform the cross product against.</param>
		/// <returns>Vector containin the cross product.</returns>
		public Vector4D CrossProduct(Vector4D vector)
		{
			Vector4D cross = new Vector4D();

			cross.X = ((X * vector.Y) - (Y * vector.X)) + ((X * vector.Z) - (Z * vector.X)) + ((Y * vector.Z) - (Z * vector.Y));
			cross.Y = ((Z * vector.Y) - (Y * vector.Z)) + ((Y * vector.W) - (W * vector.Y)) + ((Z * vector.W) - (W * vector.Z));
			cross.Z = ((X * vector.Z) - (Z * vector.X)) + ((W * vector.X) - (X * vector.W)) + ((Z * vector.W) - (W * vector.Z));
			cross.W = ((Y * vector.X) - (X * vector.Y)) + ((W * vector.X) - (X * vector.W)) + ((W * vector.Y) - (Y * vector.W));

			return cross;
		}

		/// <summary>
		/// Function to normalize the vector.
		/// </summary>
		public void Normalize()
		{
			if (Length > float.Epsilon)
			{
				float invLen = InverseLength;

				X *= invLen;
				Y *= invLen;
				Z *= invLen;
				W *= invLen;
			}
		}

		/// <summary>
		/// Function to compare this vector to another object.
		/// </summary>
		/// <param name="obj">Object to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public override bool Equals(object obj)
		{
			if (obj is Vector4D)
				return (this == (Vector4D)obj);
			else
				return false;
		}

		/// <summary>
		/// Function to return the hash code for this object.
		/// </summary>
		/// <returns>The hash code for this object.</returns>
		public override int GetHashCode()
		{
			return X.GetHashCode() ^ (Y.GetHashCode() ^ (Z.GetHashCode() ^ W.GetHashCode()));
		}

		/// <summary>
		/// Function to return the textual representation of this object.
		/// </summary>
		/// <returns>A string containing the type and values of the object.</returns>
		public override string ToString()
		{
			return string.Format("4D Vector:\n\tX:{0}, Y:{1}, Z:{2}, W:{3}",X,Y,Z,W);
		}
		#endregion

		#region Operators
		/// <summary>
		/// Function to perform addition upon two vectors.
		/// </summary>
		/// <param name="left">Vector to add to.</param>
		/// <param name="right">Vector to add with.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D Add(Vector4D left,Vector4D right)
		{
			return new Vector4D(left.X + right.X,left.Y + right.Y,left.Z + right.Z,left.W + right.W);
		}

		/// <summary>
		/// Function to perform subtraction upon two vectors.
		/// </summary>
		/// <param name="left">Vector to subtract.</param>
		/// <param name="right">Vector to subtract with.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D Subtract(Vector4D left,Vector4D right)
		{
			return new Vector4D(left.X - right.X,left.Y - right.Y,left.Z - right.Z,left.W - right.W);
		}

		/// <summary>
		/// Function to return a negated vector.
		/// </summary>
		/// <param name="left">Vector to negate.</param>
		/// <returns>A negated vector.</returns>
		public static Vector4D Negate(Vector4D left)
		{
			return new Vector4D(-left.X,-left.Y,-left.Z,-left.W);
		}

		/// <summary>
		/// Function to divide a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="scalar">Scalar value to divide by.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D Divide(Vector4D left,float scalar)
		{
			if (scalar == 0.0)
				throw new DivideByZeroException();				

			// Do this so we don't have to do multiple divides.
			float inverse = 1.0f / scalar;

			return new Vector4D(left.X * inverse,left.Y * inverse,left.Z * inverse,left.W * inverse);
		}

		/// <summary>
		/// Function to divide a vector by another vector.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="right">Vector to divide by.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D Divide(Vector4D left,Vector4D right)
		{
			if ((right.X == 0.0) || (right.Y == 0.0) || (right.Z == 0.0) || (right.W == 0.0))
				throw new DivideByZeroException();				

			return new Vector4D(left.X / right.X,left.Y / right.Y,left.Z / right.Z,left.W / right.W);				
		}
		
		/// <summary>
		/// Function to multiply two vectors together.
		/// </summary>
		/// <param name="left">Vector to multiply.</param>
		/// <param name="right">Vector to multiply by.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D Multiply(Vector4D left,Vector4D right)
		{
			return new Vector4D(left.X * right.X,left.Y * right.Y,left.Z * right.Z,left.W * right.W);
		}

		/// <summary>
		/// Function to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to multiply with.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D Multiply(Vector4D left,float scalar)
		{
			return new Vector4D(left.X * scalar,left.Y * scalar,left.Z * scalar,left.W * scalar);
		}

		/// <summary>
		/// Function to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <param name="right">Vector to multiply with.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D Multiply(float scalar,Vector4D right)
		{
			return new Vector4D(right.X * scalar,right.Y * scalar,right.Z * scalar,right.W * scalar);
		}

		/// <summary>
		/// Operator to perform equality tests.
		/// </summary>
		/// <param name="left">Vector to compare.</param>
		/// <param name="right">Vector to compare with.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(Vector4D left,Vector4D right)
		{
			return ((left.X == right.X) && (left.Y == right.Y) && (left.Z == right.Z) && (left.W == right.W));
		}

		/// <summary>
		/// Operator to perform inequality tests.
		/// </summary>
		/// <param name="left">Vector to compare.</param>
		/// <param name="right">Vector to compare with.</param>
		/// <returns>TRUE if not equal, FALSE if they are.</returns>
		public static bool operator !=(Vector4D left,Vector4D right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Operator to perform a test to see if the left is less than the right.
		/// </summary>
		/// <param name="left">Left vector to compare.</param>
		/// <param name="right">Right vector to compare.</param>
		/// <returns>TRUE if left is less than right, FALSE if not.</returns>
		public static bool operator <(Vector4D left,Vector4D right)
		{
			return ((left.X < right.X) && (left.Y < right.Y) && (left.Z < right.Z) && (left.W < right.W));
		}

		/// <summary>
		/// Operator to perform a test to see if the left is greater than the right.
		/// </summary>
		/// <param name="left">Left vector to compare.</param>
		/// <param name="right">Right vector to compare.</param>
		/// <returns>TRUE if left is greater than right, FALSE if not.</returns>
		public static bool operator >(Vector4D left,Vector4D right)
		{
			return ((left.X > right.X) && (left.Y > right.Y) && (left.Z > right.Z) && (left.W > right.W));
		}

		/// <summary>
		/// Operator to perform a test to see if the left is less or equal to the right.
		/// </summary>
		/// <param name="left">Left vector to compare.</param>
		/// <param name="right">Right vector to compare.</param>
		/// <returns>TRUE if left is less than right, FALSE if not.</returns>
		public static bool operator <=(Vector4D left,Vector4D right)
		{
			return ((left.X <= right.X) && (left.Y <= right.Y) && (left.Z <= right.Z) && (left.W <= right.W));
		}

		/// <summary>
		/// Operator to perform a test to see if the left is greater or equal to the right.
		/// </summary>
		/// <param name="left">Left vector to compare.</param>
		/// <param name="right">Right vector to compare.</param>
		/// <returns>TRUE if left is greater than right, FALSE if not.</returns>
		public static bool operator >=(Vector4D left,Vector4D right)
		{
			return ((left.X >= right.X) && (left.Y >= right.Y) && (left.Z >= right.Z) && (left.W >= right.W));
		}

		/// <summary>
		/// Operator to perform addition upon two vectors.
		/// </summary>
		/// <param name="left">Vector to add to.</param>
		/// <param name="right">Vector to add with.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D operator +(Vector4D left,Vector4D right)
		{
			return Add(left,right);
		}
		
		/// <summary>
		/// Operator to perform subtraction upon two vectors.
		/// </summary>
		/// <param name="left">Vector to subtract.</param>
		/// <param name="right">Vector to subtract with.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D operator -(Vector4D left,Vector4D right)
		{
			return Subtract(left,right);
		}

		/// <summary>
		/// Operator to negate a vector.
		/// </summary>
		/// <param name="left">Vector to negate.</param>
		/// <returns>A negated vector.</returns>
		public static Vector4D operator -(Vector4D left)
		{
			return Negate(left);
		}

		/// <summary>
		/// Operator to multiply two vectors together.
		/// </summary>
		/// <param name="left">Vector to multiply.</param>
		/// <param name="right">Vector to multiply by.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D operator *(Vector4D left,Vector4D right)
		{
			return Multiply(left,right);
		}

		/// <summary>
		/// Operator to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to multiply with.</param>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D operator *(Vector4D left,float scalar)
		{
			return Multiply(left,scalar);
		}

		/// <summary>
		/// Operator to multiply a vector by a scalar value.
		/// </summary>
		/// <param name="scalar">Scalar value to multiply by.</param>
		/// <param name="right">Vector to multiply with.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D operator *(float scalar,Vector4D right)
		{
			return Multiply(scalar,right);
		}

		/// <summary>
		/// Operator to divide a vector by a scalar value.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="scalar">Scalar value to divide by.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D operator /(Vector4D left,float scalar)
		{
			return Divide(left,scalar);
		}		

		/// <summary>
		/// Operator to divide a vector by another vector.
		/// </summary>
		/// <param name="left">Vector to divide.</param>
		/// <param name="right">Vector to divide by.</param>
		/// <returns>A new vector.</returns>
		public static Vector4D operator /(Vector4D left,Vector4D right)
		{
			return Divide(left,right);
		}

		/// <summary>
		/// Operator to convert a 2D vector into a 4D vector.
		/// </summary>
		/// <param name="vector">2D vector.</param>
		/// <returns>4D vector.</returns>
		public static implicit operator Vector4D(Vector2D vector)
		{
			return new Vector4D(vector.X,vector.Y,0,1.0f);
		}


		/// <summary>
		/// Operator to convert a 3D vector into a 4D vector.
		/// </summary>
		/// <param name="vector">3D vector</param>
		/// <returns>4D vector.</returns>
		public static implicit operator Vector4D(Vector3D vector)
		{
			return new Vector4D(vector.X,vector.Y,vector.Z,1.0f);
		}

        /// <summary>
        /// Operator to convert a 4D vector into a 3D vector.
        /// </summary>
        /// <param name="vector">4D vector to convert.</param>
        /// <returns>3D vector.</returns>
        public static explicit operator Vector3D(Vector4D vector)
        {
            return new Vector3D(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Operator to convert a 4D vector into a 2D vector.
        /// </summary>
        /// <param name="vector">4D vector to convert.</param>
        /// <returns>2D vector.</returns>
        public static explicit operator Vector2D(Vector4D vector)
        {
            return new Vector2D(vector.X, vector.Y);
        }
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="x">Horizontal position of the vector.</param>
		/// <param name="y">Vertical posiition of the vector.</param>
		/// <param name="z">Depth position of the vector.</param>
		/// <param name="w">W component of the vector.</param>
		public Vector4D(float x,float y,float z,float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}
		#endregion
	}
}
