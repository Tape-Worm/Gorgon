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
// Created: Tuesday, January 10, 2012 3:46:00 PM
// 
#endregion

using System;

namespace GorgonLibrary.Math
{
	/// <summary>
	/// A quaternion.
	/// </summary>
	/// <remarks>
	/// A quaternion is a 4 dimensional complex number.  It is often used in 3D graphics to
	/// represent a rotation.  It has an advantage over Euler angles in that it gets around
	/// a flaw called Gimbal Lock whereby an axis "locks" and rotation is impossible around
	/// that axis (there are better explanations at wikipedia).
	/// </remarks>
	public struct GorgonQuaternion
	{
		#region Variables.
		/// <summary>
		/// Scalar component for quaternion.
		/// </summary>
		public float W;
		/// <summary>
		/// X component for quaternion.
		/// </summary>
		public float X;
		/// <summary>
		/// Y component for quaternion.
		/// </summary>
		public float Y;
		/// <summary>
		/// Z component for quaternion.		
		/// </summary>
		public float Z;
		/// <summary>
		/// An identity quaternion.
		/// </summary>
		public static readonly GorgonQuaternion Identity = new GorgonQuaternion(0,0,0,1.0f);		
		/// <summary>
		/// An empty quaternion.
		/// </summary>
		public static readonly GorgonQuaternion Zero = new GorgonQuaternion(0,0,0,0);
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the inverse of this quaternion.
		/// </summary>
		public GorgonQuaternion Inverse
		{
			get
			{
				float norm = DotProduct(this);		// Get dot product.

				if (norm > 0.0f)
				{
					float invNorm = 1.0f / norm;	// Inverse normal.                    
					return new GorgonQuaternion(-X * invNorm,-Y * invNorm,-Z * invNorm,W * invNorm);
				}
				else
					return GorgonQuaternion.Zero;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to build a rotation matrix from a quaternion.
		/// </summary>
		/// <param name="q">Quaternion to represent the rotation.</param>
		/// <param name="result">The resulting rotation matrix.</param>
		public static void RotationMatrix(ref GorgonQuaternion q, out GorgonMatrix result)
		{
			// I can't even begin to explain this.
			float tx = 2.0f * q.X;
			float ty = 2.0f * q.Y;
			float tz = 2.0f * q.Z;
			float twx = tx * q.W;
			float twy = ty * q.W;
			float twz = tz * q.W;
			float txx = tx * q.X;
			float txy = ty * q.X;
			float txz = tz * q.X;
			float tyy = ty * q.Y;
			float tyz = tz * q.Y;
			float tzz = tz * q.Z;

			// Create matrix.
			result.m41 = result.m42 = result.m43 = result.m34 = result.m24 = result.m14 = 0.0f;
			result.m44 = 1.0f;

			result.m11 = 1.0f - (tyy + tzz);
			result.m12 = txy - twz;
			result.m13 = txz + twy;

			result.m21 = txy + twz;
			result.m22 = 1.0f - (txx + tzz);
			result.m23 = tyz - twx;

			result.m31 = txz - twy;
			result.m32 = tyz + twx;
			result.m33 = 1.0f - (txx + tyy);
		}

		/// <summary>
		/// Function to add two quaternions together.
		/// </summary>
		/// <param name="left">Left value to add.</param>
		/// <param name="right">Right value to add.</param>
		/// <param name="result">The combined quaternion.</param>
		public static void Add(ref GorgonQuaternion left,ref GorgonQuaternion right, out GorgonQuaternion result)
		{
			result.X = left.X + right.X;
			result.Y = left.Y + right.Y;
			result.Z = left.Z + right.Z;
			result.W = left.W + right.W;
		}

		/// <summary>
		/// Function to subtract two quaternions.
		/// </summary>
		/// <param name="left">Left value to subtract.</param>
		/// <param name="right">Right value to subtract.</param>
		/// <param name="result">The difference between the quaternions.</param>
		public static void Subtract(ref GorgonQuaternion left,ref GorgonQuaternion right, out GorgonQuaternion result)
		{
			result.X = left.X - right.X;
			result.Y = left.Y - right.Y;
			result.Z = left.Z - right.Z;
			result.W = left.W - right.W;
		}

		/// <summary>
		/// Function to negate a quaternion.
		/// </summary>
		/// <param name="qvalue">Quaternion to negate.</param>
		/// <param name="result">The negated quaternion.</param>
		/// <returns></returns>
		public static void Negate(ref GorgonQuaternion qvalue, out GorgonQuaternion result)
		{
			result.X = -qvalue.X;
			result.Y = -qvalue.Y;
			result.Z = -qvalue.Z;
			result.W = -qvalue.W;
		}

		/// <summary>
		/// Function to multiply two quaternions together.
		/// </summary>
		/// <param name="left">Left value to multiply.</param>
		/// <param name="right">Right value to multiply.</param>
		/// <param name="result">The product of the two quaternions.</param>
		public static void Multiply(ref GorgonQuaternion left,ref GorgonQuaternion right, out GorgonQuaternion result)
		{
			result.X = left.W * right.X + left.X * right.W + left.Y * right.Z - left.Z * right.Y;
			result.Y = left.W * right.Y + left.Y * right.W + left.Z * right.X - left.X * right.Z;
			result.Z = left.W * right.Z + left.Z * right.W + left.X * right.Y - left.Y * right.X;
			result.W = left.W * right.W - left.X * right.X - left.Y * right.Y - left.Z * right.Z;
		}

		/// <summary>
		/// Function to multiply a quaternion by a scalar value.
		/// </summary>
		/// <param name="left">Quaternion to multiply with.</param>
		/// <param name="scalar">Value to multiply with.</param>
		/// <param name="result">Scaled quaternion.</param>
		public static void Multiply(ref GorgonQuaternion left, float scalar, out GorgonQuaternion result)
		{
			result.X = left.X * scalar;
			result.Y = left.Y * scalar;
			result.Z = left.Z * scalar;
			result.W = left.W * scalar;
		}

		/// <summary>
		/// Function to divide a quaternion by a scalar value.
		/// </summary>
		/// <param name="left">Quaternion to divide.</param>
		/// <param name="scalar">Scalar to divide by.</param>
		/// <param name="result">The quotient of the quaternion and scalar.</param>
		/// <exception cref="System.DivideByZeroException">Throw when <paramref name="scalar"/> is 0.0.</exception>
		public static void Divide(ref GorgonQuaternion left,float scalar, out GorgonQuaternion result)
		{
			float scale = 1.0f / scalar;

			result.X = left.X * scale;
			result.Y = left.Y * scale;
			result.Z = left.Z * scale;
			result.W = left.W * scale;
		}

		/// <summary>
		/// Function to create a quaternion from an angle and an axis.
		/// </summary>
		/// <param name="angle">Angle in degrees.</param>
		/// <param name="axis">Axis to use.</param>
		/// <param name="result">Result of the function.</param>
		/// <returns>A new quaternion.</returns>
		public static void FromAxisAngle(float angle, ref GorgonVector3 axis, out GorgonQuaternion result)
		{
			result = GorgonQuaternion.Identity;							// Result.
			float radAngle = GorgonMathUtility.Radians(angle) * 0.5f;	// Angle in radians.
			float sin = GorgonMathUtility.Sin(radAngle);				// Sine of angle.

			result.W = GorgonMathUtility.Cos(radAngle);
			result.X = sin * axis.X;
			result.Y = sin * axis.Y;
			result.Z = sin * axis.Z;
		}

		/// <summary>
		/// Function to create a quaternion from an angle and an axis.
		/// </summary>
		/// <param name="angle">Angle in degrees.</param>
		/// <param name="axis">Axis to use.</param>
		/// <returns>A new quaternion.</returns>
		public static GorgonQuaternion FromAxisAngle(float angle,GorgonVector3 axis)
		{
			GorgonQuaternion result = GorgonQuaternion.Identity;
			FromAxisAngle(angle, ref axis, out result);
			return result;
		}

		/// <summary>
		/// Function to calculate the spherical linear interpolation between two quaternions.
		/// </summary>
		/// <param name="time">Time to retrieve quaternion at.</param>
		/// <param name="q1">Starting quaternion.</param>
		/// <param name="q2">Ending quaternion.</param>
		/// <param name="optimizepath">TRUE to use the shortest path, FALSE otherwise.</param>
		/// <param name="result">The result of the function.</param>
		/// <returns>Quaternion rotation at time specified.</returns>
		public static void SLERP(float time,ref GorgonQuaternion q1, ref GorgonQuaternion q2, bool optimizepath, out GorgonQuaternion result)
		{
			float cos = q1.DotProduct(q2);										// Cosine of the angle between both Quaternion.
			float angle = GorgonMathUtility.ACos(cos);							// Angle in radians between both Quaternion.
			float sin = GorgonMathUtility.Sin(angle);							// Sine of the angle.
			float invSin = 1.0f / sin;											// Inverse of the sine.
			float c1 = GorgonMathUtility.Sin((1.0f - time) * angle) * invSin;	// Co-efficient 1.
			float c2 = GorgonMathUtility.Sin(time * angle) * invSin;			// Co-efficient 2.

			result = GorgonQuaternion.Zero;					// Resultant quaternion.

			if (GorgonMathUtility.Abs(angle) < 1e-03f)
			{
				result = q1;
				return;
			}

			if (cos < 0.0f && optimizepath)
			{
				c1 = -c1;
				GorgonQuaternion temp = c1 * q1 + c2 * q2;
				temp.Normalize();
				result = temp;
			} 
			else
				result = c1 * q1 + c2 * q2;
		}

		/// <summary>
		/// Function to calculate the spherical linear interpolation between two quaternions.
		/// </summary>
		/// <param name="time">Time to retrieve quaternion at.</param>
		/// <param name="q1">Starting quaternion.</param>
		/// <param name="q2">Ending quaternion.</param>
		/// <param name="optimizepath">TRUE to use the shortest path, FALSE otherwise.</param>
		/// <returns>Quaternion rotation at time specified.</returns>
		public static GorgonQuaternion SLERP(float time, GorgonQuaternion q1, GorgonQuaternion q2, bool optimizepath)
		{
			GorgonQuaternion result = GorgonQuaternion.Zero;					

			SLERP(time, ref q1, ref q2, optimizepath, out result);

			return result;
		}

		/// <summary>
		/// Function to calculate spherical quadratic interpolation.
		/// </summary>
		/// <param name="time">Time to retrieve quaternion at.</param>
		/// <param name="q1">Quaternion start.</param>
		/// <param name="q2">Quaternion point 2.</param>
		/// <param name="q3">Quaternion point 3.</param>
		/// <param name="q4">Quaternion end.</param>
		/// <param name="optimizepath">TRUE to use shortest path, FALSE otherwise.</param>
		/// <param name="result">The result of the function.</param>
		/// <returns>Quaternion rotation at time specified.</returns>
		public static void SQUAD(float time, ref GorgonQuaternion q1, ref GorgonQuaternion q2, ref GorgonQuaternion q3, ref GorgonQuaternion q4, bool optimizepath, out GorgonQuaternion result)
		{
			float slerpTime = 2.0f * time * (1.0f - time);		// Time.
			GorgonQuaternion qp = GorgonQuaternion.Identity;
			GorgonQuaternion qq = GorgonQuaternion.Identity;
			
			result = GorgonQuaternion.Zero;

			SLERP(time, ref q1, ref q4, optimizepath, out qp);
			SLERP(time, ref q2, ref q3, false, out qq);
			SLERP(slerpTime, ref qp, ref qq, false, out result);
		}


		/// <summary>
		/// Function to calculate spherical quadratic interpolation.
		/// </summary>
		/// <param name="time">Time to retrieve quaternion at.</param>
		/// <param name="q1">Quaternion start.</param>
		/// <param name="q2">Quaternion point 2.</param>
		/// <param name="q3">Quaternion point 3.</param>
		/// <param name="q4">Quaternion end.</param>
		/// <param name="optimizepath">TRUE to use shortest path, FALSE otherwise.</param>
		/// <returns>Quaternion rotation at time specified.</returns>
		public static GorgonQuaternion SQUAD(float time,GorgonQuaternion q1,GorgonQuaternion q2,GorgonQuaternion q3,GorgonQuaternion q4,bool optimizepath)
		{
			GorgonQuaternion result = GorgonQuaternion.Zero;

			SQUAD(time, ref q1, ref q2, ref q3, ref q4, optimizepath, out result);

			return result;
		}

		/// <summary>
		/// Function to compare this object with another and see if they're the same type.
		/// </summary>
		/// <param name="obj">Object to compare</param>
		/// <returns>TRUE if object is the same as this one.</returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonQuaternion)
				return (((GorgonQuaternion)obj) == this);
			else
				return false;
		}

		/// <summary>
		/// Function to return the string representation of this object.
		/// </summary>
		/// <returns>A string containing info about this quaternion.</returns>
		public override string ToString()
		{
			return string.Format("Quaternion:\n\tX:{0}, Y:{1}, Z:{2}, W:{3}",X,Y,Z,W);
		}

		/// <summary>
		/// Function to return the hash code for this object.
		/// </summary>
		/// <returns>New hash object.</returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(X).GenerateHash(Y).GenerateHash(Z).GenerateHash(W);
		}

		/// <summary>
		/// Function to retrieve the dot product between this and another quaternion.
		/// </summary>
		/// <param name="q">Quaternion to get the dot product with.</param>
		/// <returns>Dot product between the two quaternions.</returns>
		public float DotProduct(GorgonQuaternion q)
		{
			return this.W * q.W + this.X * q.X + this.Y * q.Y + this.Z * q.Z;
		}

		/// <summary>
		/// Function to normalize the quaternion.
		/// </summary>
		public void Normalize()
		{
			float factor = 1.0f /GorgonMathUtility.Sqrt(X * X + Y * Y + Z * Z + W * W);

			this = this * factor;
		}

		/// <summary>
		/// Function to do a rotation about Euler angles.
		/// </summary>
		/// <param name="yaw">Rotation around the Y axis.</param>		
		/// <param name="pitch">Rotation around the X axis.</param>		
		/// <param name="roll">Rotation around the Z axis.</param>
		public void RotateEuler(float yaw,float pitch,float roll)
		{
			float radYaw = (GorgonMathUtility.Radians(yaw) * 0.5f);			// 1/2 Yaw in radians.
			float radPitch = (GorgonMathUtility.Radians(pitch) * 0.5f);		// 1/2 Pitch in radians.
			float radRoll = (GorgonMathUtility.Radians(roll) * 0.5f);			// 1/2 Roll in radians.
			GorgonQuaternion x,y,z;											// Quaternion used in calculations.

			x = new GorgonQuaternion(GorgonMathUtility.Sin(radPitch),0,0,GorgonMathUtility.Cos(radPitch));
			y = new GorgonQuaternion(0,GorgonMathUtility.Sin(radYaw),0,GorgonMathUtility.Cos(radYaw));
			z = new GorgonQuaternion(0,0,GorgonMathUtility.Sin(radRoll),GorgonMathUtility.Cos(radRoll));

			this = (x * y) * z;
		}

		/// <summary>
		/// Function to get values from a rotation matrix.
		/// </summary>
		/// <param name="matrix">GorgonMatrix to extract quaternion from.</param>
		public void FromRotationGorgonMatrix(GorgonMatrix matrix)
		{
		}

		/// <summary>
		/// Function to extract quaternion from axis vector values.
		/// </summary>
		/// <param name="x">X axis.</param>
		/// <param name="y">Y axis.</param>
		/// <param name="z">Z axis.</param>
		public void FromAxes(GorgonVector3 x,GorgonVector3 y,GorgonVector3 z)
		{
			GorgonMatrix matrix = GorgonMatrix.Identity;		// Intermediate matrix.

			matrix.m11 = x.X;
			matrix.m21 = x.Y;
			matrix.m31 = x.Z;

			matrix.m12 = y.X;
			matrix.m22 = y.Y;
			matrix.m32 = y.Z;

			matrix.m13 = z.X;
			matrix.m23 = z.Y;
			matrix.m33 = z.Z;

			FromRotationGorgonMatrix(matrix);
		}

		/// <summary>
		/// Function to convert the quaternion to an angle and an axis.
		/// </summary>
		/// <param name="angle">Angle in degrees.</param>
		/// <param name="axis">Axis for rotation.</param>
		public void ToAngleAxis(ref float angle,ref GorgonVector3 axis)
		{
			float len = X * X + Y * Y + Z * Z;				// Squared length.
			float invLen = GorgonMathUtility.InverseSqrt(len);	// Inversed square length.

			if (len > 0.0f)
			{
				angle = GorgonMathUtility.Degrees(2.0f *GorgonMathUtility.Cos(W));
				axis.X = X * invLen;
				axis.Y = Y * invLen;
				axis.Z = Z * invLen;
			} 
			else
			{
				angle = 0.0f;
				axis.X = 1.0f;
				axis.Y = 0.0f;
				axis.Z = 0.0f;
			}
		}		

		/// <summary>
		/// Function to convert the quaternion to rotation axes.
		/// </summary>
		/// <param name="x">Rotation about X axis.</param>
		/// <param name="y">Rotation about Y axis.</param>
		/// <param name="z">Rotation about Z axis.</param>
		public void ToAxes(out GorgonVector3 x,out GorgonVector3 y,out GorgonVector3 z)
		{
			GorgonMatrix rot;	// Rotation matrix of this quaternion.

			RotationMatrix(ref this, out rot);

			// Extract rotation vectors.
			x = new GorgonVector3(rot.m11, rot.m12, rot.m13);
			y = new GorgonVector3(rot.m21, rot.m22, rot.m23);
			z = new GorgonVector3(rot.m31, rot.m32, rot.m33);
		}		
		#endregion

		#region Operators.
		/// <summary>
		/// Operator to test for equality between two quaternions.
		/// </summary>
		/// <param name="left">Left quaternion value.</param>
		/// <param name="right">Right quaternion value.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(GorgonQuaternion left,GorgonQuaternion right)
		{
			return (GorgonMathUtility.EqualFloat(left.X, right.X) &&GorgonMathUtility.EqualFloat(left.Y, right.Y) &&
					GorgonMathUtility.EqualFloat(left.Z, right.Z) &&GorgonMathUtility.EqualFloat(left.W, right.W));
		}

		/// <summary>
		/// Operator to test for inequality between two quaternions.
		/// </summary>
		/// <param name="left">Left quaternion value.</param>
		/// <param name="right">Right quaternion value.</param>
		/// <returns>TRUE if not equal, FALSE if they are.</returns>
		public static bool operator !=(GorgonQuaternion left,GorgonQuaternion right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Operator to add two quaternions together.
		/// </summary>
		/// <param name="left">Left value to add.</param>
		/// <param name="right">Right value to add.</param>
		/// <returns>Total of both Quaternion.</returns>
		public static GorgonQuaternion operator +(GorgonQuaternion left,GorgonQuaternion right)
		{
			GorgonQuaternion result;
			Add(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Operator to subtract two quaternions.
		/// </summary>
		/// <param name="left">Left value to subtract.</param>
		/// <param name="right">Right value to subtract.</param>
		/// <returns>Difference between both Quaternion.</returns>
		public static GorgonQuaternion operator -(GorgonQuaternion left,GorgonQuaternion right)
		{
			GorgonQuaternion result;
			Subtract(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Operator to negate a quaternion.
		/// </summary>
		/// <param name="qvalue">Quaternion to negate.</param>
		/// <returns>A negative quaternion.</returns>
		public static GorgonQuaternion operator -(GorgonQuaternion qvalue)
		{
			GorgonQuaternion result;
			Negate(ref qvalue, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply two quaternions together.
		/// </summary>
		/// <param name="left">Left value to multiply.</param>
		/// <param name="right">Right value to multiply.</param>
		/// <returns>Product of the two quaternions.</returns>
		public static GorgonQuaternion operator *(GorgonQuaternion left,GorgonQuaternion right)
		{
			GorgonQuaternion result;
			Multiply(ref left, ref right, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply a quaternion by a scalar value.
		/// </summary>
		/// <param name="scalar">Value to multiply with.</param>
		/// <param name="right">Quaternion to multiply with.</param>
		/// <returns>Scaled quaternion.</returns>
		public static GorgonQuaternion operator *(float scalar,GorgonQuaternion right)
		{
			GorgonQuaternion result;
			Multiply(ref right, scalar, out result);
			return result;
		}

		/// <summary>
		/// Operator to multiply a quaternion by a scalar value.
		/// </summary>
		/// <param name="left">Quaternion to multiply with.</param>
		/// <param name="scalar">Value to multiply with.</param>/// 
		/// <returns>Scaled quaternion.</returns>
		public static GorgonQuaternion operator *(GorgonQuaternion left,float scalar)
		{
			GorgonQuaternion result;
			Multiply(ref left, scalar, out result);
			return result;
		}

		/// <summary>
		/// Operator to divide a quaternion by a scalar value.
		/// </summary>
		/// <param name="left">Quaternion to divide.</param>
		/// <param name="scalar">Scalar to divide by.</param>
		/// <returns>Divided quaternion.</returns>
		public static GorgonQuaternion operator /(GorgonQuaternion left,float scalar)
		{
			GorgonQuaternion result;
			Divide(ref left, scalar, out result);
			return result;
		}

		/// <summary>
		/// Operator to convert a 4D vector into a quaternion.
		/// </summary>
		/// <param name="vec">4D vector to convert.</param>
		/// <returns>Quaternion.</returns>
		public static implicit operator GorgonQuaternion(GorgonVector4 vec)
		{
			return new GorgonQuaternion(vec.X,vec.Y,vec.Z,vec.W);
		}

		/// <summary>
		/// Operator to convert a 3D vector into a quaternion.
		/// </summary>
		/// <param name="vec">3D vector to convert.</param>
		/// <returns>Quaternion.</returns>
		public static implicit operator GorgonQuaternion(GorgonVector3 vec)
		{
			return new GorgonQuaternion(vec.X,vec.Y,vec.Z,1.0f);
		}
		#endregion

		#region Constructors.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonQuaternion"/> struct.
		/// </summary>
		/// <param name="x">X component.</param>
		/// <param name="y">Y component.</param>
		/// <param name="z">Z component.</param>
		/// <param name="w">W component.</param>
		public GorgonQuaternion(float x,float y,float z,float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonQuaternion"/> struct.
		/// </summary>
		/// <param name="vector">The vector to copy.</param>
		public GorgonQuaternion(GorgonVector4 vector)
		{
			X = vector.X;
			Y = vector.Y;
			Z = vector.Z;
			W = vector.W;
		}
		#endregion
	}
}
