#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Saturday, April 19, 2008 11:44:47 AM
// 
#endregion

using System;

namespace GorgonLibrary
{
	/// <summary>
	/// A value type representing a quaternion.
	/// </summary>
	/// <remarks>
	/// A quaternion is a 4 dimensional complex number.  It is often used in 3D graphics to
	/// represent a rotation.  It has an advantage over Euler angles in that it gets around
	/// a flaw called Gimbal Lock whereby an axis "locks" and rotation is impossible around
	/// that axis (there are better explanations at wikipedia).
	/// </remarks>
	public struct Quaternion
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

		// An identity quaternion.
		private static readonly Quaternion _identity = new Quaternion(0,0,0,1.0f);
		// An empty quaternion.
		private static readonly Quaternion _zero = new Quaternion(0,0,0,0);
		// Used to convert a rotation matrix to a quaternion.
		private static readonly int[] _next = new int[3] {2,3,1};
		#endregion

		#region Properties.
		/// <summary>
		/// Property to get a representation of an identity quaternion.
		/// </summary>
		public static Quaternion Identity
		{
			get
			{
				return _identity;
			}
		}

		/// <summary>
		/// Property to get a representation of an empty quaternion.
		/// </summary>
		public static Quaternion Zero
		{
			get
			{
				return _zero;
			}
		}


		/// <summary>
		/// Property to return the rotation matrix.
		/// </summary>
		public Matrix RotationMatrix
		{
			get
			{
				Matrix result = Matrix.Identity;		// Resultant matrix.

				// I can't even begin to explain this.
				float tx = 2.0f * X;
				float ty = 2.0f * Y;
				float tz = 2.0f * Z;
				float twx = tx * W;
				float twy = ty * W;
				float twz = tz * W;
				float txx = tx * X;
				float txy = ty * X;
				float txz = tz * X;
				float tyy = ty * Y;
				float tyz = tz * Y;
				float tzz = tz * Z;

				// Create matrix.
				result[1,1] = 1.0f - (tyy + tzz);
				result[1,2] = txy - twz;
				result[1,3] = txz + twy;
				result[2,1] = txy + twz;
				result[2,2] = 1.0f - (txx + tzz);
				result[2,3] = tyz - twx;
				result[3,1] = txz - twy;
				result[3,2] = tyz + twx;
				result[3,3] = 1.0f - (txx+tyy);

				return result;
			}
		}

		/// <summary>
		/// Property to return the inverse of this quaternion.
		/// </summary>
		public Quaternion Inverse
		{
			get
			{
				float norm = DotProduct(this);		// Get dot product.

				if (norm > 0.0f)
				{
					float invNorm = 1.0f / norm;	// Inverse normal.                    
					return new Quaternion(-X * invNorm,-Y * invNorm,-Z * invNorm,W * invNorm);
				}
				else
					return Quaternion.Zero;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add two quaternions together.
		/// </summary>
		/// <param name="left">Left value to add.</param>
		/// <param name="right">Right value to add.</param>
		/// <returns>Total of both Quaternion.</returns>
		public static Quaternion Add(Quaternion left,Quaternion right)
		{
			return new Quaternion(left.X + right.X,left.Y + right.Y,left.Z + right.Z,left.W + right.W);
		}

		/// <summary>
		/// Function to subtract two quaternions.
		/// </summary>
		/// <param name="left">Left value to subtract.</param>
		/// <param name="right">Right value to subtract.</param>
		/// <returns>Difference between both Quaternion.</returns>
		public static Quaternion Subtract(Quaternion left,Quaternion right)
		{
			return new Quaternion(left.X - right.X,left.Y - right.Y,left.Z - right.Z,left.W - right.W);
		}

		/// <summary>
		/// Function to negate a quaternion.
		/// </summary>
		/// <param name="qvalue">Quaternion to negate.</param>
		/// <returns>A negative quaternion.</returns>
		public static Quaternion Negate(Quaternion qvalue)
		{
			return new Quaternion(-qvalue.X,-qvalue.Y,-qvalue.Z,-qvalue.W);
		}

		/// <summary>
		/// Function to multiply two quaternions together.
		/// </summary>
		/// <param name="left">Left value to multiply.</param>
		/// <param name="right">Right value to multiply.</param>
		/// <returns>Product of the two quaternions.</returns>
		public static Quaternion Multiply(Quaternion left,Quaternion right)
		{
			Quaternion result = Quaternion.Zero;	// Resultant quaternion.

			result.X = left.W * right.X + left.X * right.W + left.Y * right.Z - left.Z * right.Y;
			result.Y = left.W * right.Y + left.Y * right.W + left.Z * right.X - left.X * right.Z;
			result.Z = left.W * right.Z + left.Z * right.W + left.X * right.Y - left.Y * right.X;
			result.W = left.W * right.W - left.X * right.X - left.Y * right.Y - left.Z * right.Z;

			return result;
		}

		/// <summary>
		/// Function to multiply a quaternion by a scalar value.
		/// </summary>
		/// <param name="scalar">Value to multiply with.</param>
		/// <param name="right">Quaternion to multiply with.</param>
		/// <returns>Scaled quaternion.</returns>
		public static Quaternion Multiply(float scalar,Quaternion right)
		{
			return new Quaternion(right.X * scalar,right.Y * scalar,right.Z * scalar,right.W * scalar);
		}

		/// <summary>
		/// Function to multiply a quaternion by a scalar value.
		/// </summary>
		/// <param name="left">Quaternion to multiply with.</param>
		/// <param name="scalar">Value to multiply with.</param>/// 
		/// <returns>Scaled quaternion.</returns>
		public static Quaternion Multiply(Quaternion left,float scalar)
		{
			return new Quaternion(left.X * scalar,left.Y * scalar,left.Z * scalar,left.W * scalar);
		}

		/// <summary>
		/// Function to multiply a quaternion by a 3D vector.
		/// </summary>
		/// <param name="q">Quaternion to multiply.</param>
		/// <param name="vector">Vector to multiply by.</param>
		/// <returns>A new vector with the product of the quaternion and the vector.</returns>
		public static Vector3D Multiply(Quaternion q,Vector3D vector)
		{
			// From NVIDIA SDK.
			Vector3D uv, uuv;
			Vector3D qVec = new Vector3D(q.X,q.Y,q.Z);

			uv = qVec.CrossProduct(vector);
			uuv = qVec.CrossProduct(uv);
			uv *= (2.0f * q.W);
			uuv *= 2.0f;

			return vector + uv + uuv;
		}

		/// <summary>
		/// Function to divide a quaternion by a scalar value.
		/// </summary>
		/// <param name="left">Quaternion to divide.</param>
		/// <param name="scalar">Scalar to divide by.</param>
		/// <returns>Divided quaternion.</returns>
		public static Quaternion Divide(Quaternion left,float scalar)
		{
			if (scalar == 0.0)
				throw new DivideByZeroException();				

			return Multiply(left,1.0f/scalar);
		}

		/// <summary>
		/// Function to create a quaternion from an angle and an axis.
		/// </summary>
		/// <param name="angle">Angle in degrees.</param>
		/// <param name="axis">Axis to use.</param>
		/// <returns>A new quaternion.</returns>
		public static Quaternion FromAxisAngle(float angle,Vector3D axis)
		{
			Quaternion result = Quaternion.Identity;			// Result.
			float radAngle = MathUtility.Radians(angle) * 0.5f;	// Angle in radians.
			float sin = MathUtility.Sin(radAngle);				// Sine of angle.

            result.W = MathUtility.Cos(radAngle);
			result.X = sin * axis.X;
			result.Y = sin * axis.Y;
			result.Z = sin * axis.Z;

			return result;
		}

		/// <summary>
		/// Function to calculate the spherical linear interpolation between two quaternions.
		/// </summary>
		/// <param name="time">Time to retrieve quaternion at.</param>
		/// <param name="q1">Starting quaternion.</param>
		/// <param name="q2">Ending quaternion.</param>
		/// <param name="optimizepath">TRUE to use the shortest path, FALSE otherwise.</param>
		/// <returns>Quaternion rotation at time specified.</returns>
		public static Quaternion SLERP(float time,Quaternion q1,Quaternion q2,bool optimizepath)
		{
			float cos = q1.DotProduct(q2);									// Cosine of the angle between both Quaternion.
			float angle = MathUtility.ACos(cos);							// Angle in radians between both Quaternion.
			float sin = MathUtility.Sin(angle);								// Sine of the angle.
			float invSin = 1.0f / sin;										// Inverse of the sine.
			float c1 = MathUtility.Sin((1.0f - time) * angle) * invSin;		// Co-efficient 1.
			float c2 = MathUtility.Sin(time * angle) * invSin;				// Co-efficient 2.
			Quaternion result = Quaternion.Zero;							// Resultant quaternion.

			if (MathUtility.Abs(angle) < 1e-03f)
				return q1;

			if (cos < 0.0f && optimizepath)
			{
				c1 = -c1;
				Quaternion temp = c1 * q1 + c2 * q2;
				temp.Normalize();
				result = temp;
			} 
			else
				result = c1 * q1 + c2 * q2;

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
		/// <returns>Quaternion rotation at time specified.</returns>
		public static Quaternion SQUAD(float time,Quaternion q1,Quaternion q2,Quaternion q3,Quaternion q4,bool optimizepath)
		{
			float slerpTime = 2.0f * time * (1.0f - time);		// Time.
            Quaternion qp = SLERP(time,q1,q4,optimizepath);
			Quaternion qq = SLERP(time,q2,q3);

			return SLERP(slerpTime,qp,qq);
		}

		/// <summary>
		/// Function to calculate spherical quadratic interpolation.
		/// </summary>
		/// <param name="time">Time to retrieve quaternion at.</param>
		/// <param name="q1">Quaternion start.</param>
		/// <param name="q2">Quaternion point 2.</param>
		/// <param name="q3">Quaternion point 3.</param>
		/// <param name="q4">Quaternion end.</param>
		/// <returns>Quaternion rotation at time specified.</returns>
		public static Quaternion SQUAD(float time,Quaternion q1,Quaternion q2,Quaternion q3,Quaternion q4)
		{
			return SQUAD(time,q1,q2,q3,q4,false);
		}

		/// <summary>
		/// Function to calculate the spherical linear interpolation between two quaternions.
		/// </summary>
		/// <param name="time">Time to retrieve quaternion at.</param>
		/// <param name="q1">Starting quaternion.</param>
		/// <param name="q2">Ending quaternion.</param>
		/// <returns>Quaternion rotation at time specified.</returns>
		public static Quaternion SLERP(float time,Quaternion q1,Quaternion q2)
		{
			return SLERP(time,q1,q2,false);
		}

		/// <summary>
		/// Function to compare this object with another and see if they're the same type.
		/// </summary>
		/// <param name="obj">Object to compare</param>
		/// <returns>TRUE if object is the same as this one.</returns>
		public override bool Equals(object obj)
		{
			if (obj is Quaternion)
				return (((Quaternion)obj) == this);
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
			return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode();
		}

		/// <summary>
		/// Function to retrieve the dot product between this and another quaternion.
		/// </summary>
		/// <param name="q">Quaternion to get the dot product with.</param>
		/// <returns>Dot product between the two quaternions.</returns>
		public float DotProduct(Quaternion q)
		{
			return this.W * q.W + this.X * q.X + this.Y * q.Y + this.Z * q.Z;
		}

		/// <summary>
		/// Function to normalize the quaternion.
		/// </summary>
		public void Normalize()
		{
			float factor = 1.0f / MathUtility.Sqrt(X * X + Y * Y + Z * Z + W * W);

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
			float radYaw = (MathUtility.Radians(yaw) * 0.5f);			// 1/2 Yaw in radians.
			float radPitch = (MathUtility.Radians(pitch) * 0.5f);		// 1/2 Pitch in radians.
			float radRoll = (MathUtility.Radians(roll) * 0.5f);			// 1/2 Roll in radians.
			Quaternion x,y,z;											// Quaternion used in calculations.

			x = new Quaternion(MathUtility.Sin(radPitch),0,0,MathUtility.Cos(radPitch));
			y = new Quaternion(0,MathUtility.Sin(radYaw),0,MathUtility.Cos(radYaw));
			z = new Quaternion(0,0,MathUtility.Sin(radRoll),MathUtility.Cos(radRoll));

			this = (x * y) * z;
		}

		/// <summary>
		/// Function to get values from a rotation matrix.
		/// </summary>
		/// <param name="matrix">Matrix to extract quaternion from.</param>
		public void FromRotationMatrix(Matrix matrix)
		{
			// Algorithm in Ken Shoemake's article in 1987 SIGGRAPH course notes
			// article "Quaternion Calculus and Fast Animation".

			float trace = matrix[1,1] + matrix[2,2] + matrix[3,3];	
			float root = 0.0f;

			if (trace > 0.000001f)
			{
				root = MathUtility.Sqrt(trace + 1.0f);
				W = 0.5f * root;
				root = 0.5f / root;
				X = (matrix[3,2] - matrix[2,3]) * root;
				Y = (matrix[1,3] - matrix[3,1]) * root;
				Z = (matrix[2,1] - matrix[1,2]) * root;
			} 
			else 
			{
				// The implementation from the matrix/quaternion FAQ didn't work (or I probably messed up more likely),
				// so I grabbed this (and many other ideas) from Ogre (http://www.ogre3d.org).  Steve Streeting is a god
				// or something.
				int i = 1;
				if (matrix[2,2] > matrix[1,1])
					i = 2;
				if (matrix[3,3] > matrix[i,i])
					i = 3;

				int j = _next[i-1];
				int k = _next[j-1];

				root = MathUtility.Sqrt(matrix[i,i] - matrix[j,j] - matrix[k,k] + 1.0f);
				unsafe 
				{
					fixed (float *qx = &this.X)
					{
						qx[i-1] = 0.5f * root;
						root = 0.5f / root;

						this.W = (matrix[k,j] - matrix[j,k]) * root;
						qx[j-1] = (matrix[j,i] + matrix[i,j]) * root;
						qx[k-1] = (matrix[k,i] + matrix[i,k]) * root;
					}
				}
			}
		}

		/// <summary>
		/// Function to extract quaternion from axis vector values.
		/// </summary>
		/// <param name="x">X axis.</param>
		/// <param name="y">Y axis.</param>
		/// <param name="z">Z axis.</param>
		public void FromAxes(Vector3D x,Vector3D y,Vector3D z)
		{
			Matrix matrix = Matrix.Identity;		// Intermediate matrix.

			matrix[1,1] = x.X;
			matrix[2,1] = x.Y;
			matrix[3,1] = x.Z;

			matrix[1,2] = y.X;
			matrix[2,2] = y.Y;
			matrix[3,2] = y.Z;

			matrix[1,3] = z.X;
			matrix[2,3] = z.Y;
			matrix[3,3] = z.Z;

			FromRotationMatrix(matrix);
		}

		/// <summary>
		/// Function to convert the quaternion to an angle and an axis.
		/// </summary>
		/// <param name="angle">Angle in degrees.</param>
		/// <param name="axis">Axis for rotation.</param>
		public void ToAngleAxis(ref float angle,ref Vector3D axis)
		{
			float len = X * X + Y * Y + Z * Z;				// Squared length.
			float invLen = MathUtility.InverseSqrt(len);	// Inversed square length.

			if (len > 0.0f)
			{
				angle = MathUtility.Degrees(2.0f * MathUtility.Cos(W));
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
		public void ToAxes(out Vector3D x,out Vector3D y,out Vector3D z)
		{
			Matrix rot = RotationMatrix;	// Rotation matrix of this quaternion.

            // Extract rotation vectors.
			x = rot.Column(1);
			y = rot.Column(2);
			z = rot.Column(3);
		}		
		#endregion

		#region Operators.
		/// <summary>
		/// Operator to test for equality between two quaternions.
		/// </summary>
		/// <param name="left">Left quaternion value.</param>
		/// <param name="right">Right quaternion value.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(Quaternion left,Quaternion right)
		{
			if ((left.X == right.X) && (left.Y == right.Y) && (left.Z == right.Z) && (left.W == right.W))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Operator to test for inequality between two quaternions.
		/// </summary>
		/// <param name="left">Left quaternion value.</param>
		/// <param name="right">Right quaternion value.</param>
		/// <returns>TRUE if not equal, FALSE if they are.</returns>
		public static bool operator !=(Quaternion left,Quaternion right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Operator to add two quaternions together.
		/// </summary>
		/// <param name="left">Left value to add.</param>
		/// <param name="right">Right value to add.</param>
		/// <returns>Total of both Quaternion.</returns>
		public static Quaternion operator +(Quaternion left,Quaternion right)
		{
			return Add(left,right);
		}

		/// <summary>
		/// Operator to subtract two quaternions.
		/// </summary>
		/// <param name="left">Left value to subtract.</param>
		/// <param name="right">Right value to subtract.</param>
		/// <returns>Difference between both Quaternion.</returns>
		public static Quaternion operator -(Quaternion left,Quaternion right)
		{
			return Subtract(left,right);
		}

		/// <summary>
		/// Operator to negate a quaternion.
		/// </summary>
		/// <param name="qvalue">Quaternion to negate.</param>
		/// <returns>A negative quaternion.</returns>
		public static Quaternion operator -(Quaternion qvalue)
		{
			return Negate(qvalue);
		}

		/// <summary>
		/// Operator to multiply two quaternions together.
		/// </summary>
		/// <param name="left">Left value to multiply.</param>
		/// <param name="right">Right value to multiply.</param>
		/// <returns>Product of the two quaternions.</returns>
		public static Quaternion operator *(Quaternion left,Quaternion right)
		{
			return Multiply(left,right);
		}

		/// <summary>
		/// Operator to multiply a quaternion by a scalar value.
		/// </summary>
		/// <param name="scalar">Value to multiply with.</param>
		/// <param name="right">Quaternion to multiply with.</param>
		/// <returns>Scaled quaternion.</returns>
		public static Quaternion operator *(float scalar,Quaternion right)
		{
			return Multiply(scalar,right);
		}

		/// <summary>
		/// Operator to multiply a quaternion by a scalar value.
		/// </summary>
		/// <param name="left">Quaternion to multiply with.</param>
		/// <param name="scalar">Value to multiply with.</param>/// 
		/// <returns>Scaled quaternion.</returns>
		public static Quaternion operator *(Quaternion left,float scalar)
		{
			return Multiply(left,scalar);
		}

		/// <summary>
		/// Operator to multiply a quaternion by a 3D vector.
		/// </summary>
		/// <param name="q">Quaternion to multiply.</param>
		/// <param name="vector">Vector to multiply by.</param>
		/// <returns>A new vector with the product of the quaternion and the vector.</returns>
		public static Vector3D operator *(Quaternion q,Vector3D vector)
		{
			return Multiply(q,vector);
		}

		/// <summary>
		/// Operator to divide a quaternion by a scalar value.
		/// </summary>
		/// <param name="left">Quaternion to divide.</param>
		/// <param name="scalar">Scalar to divide by.</param>
		/// <returns>Divided quaternion.</returns>
		public static Quaternion operator /(Quaternion left,float scalar)
		{
			return Divide(left,scalar);
		}

		/// <summary>
		/// Operator to convert a 4D vector into a quaternion.
		/// </summary>
		/// <param name="vec">4D vector to convert.</param>
		/// <returns>Quaternion.</returns>
		public static implicit operator Quaternion(Vector4D vec)
		{
			return new Quaternion(vec.X,vec.Y,vec.Z,vec.W);
		}

		/// <summary>
		/// Operator to convert a 3D vector into a quaternion.
		/// </summary>
		/// <param name="vec">3D vector to convert.</param>
		/// <returns>Quaternion.</returns>
		public static implicit operator Quaternion(Vector3D vec)
		{
			return new Quaternion(vec.X,vec.Y,vec.Z,1.0f);
		}
		#endregion

		#region Constructors.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="x">X component.</param>
		/// <param name="y">Y component.</param>
		/// <param name="z">Z component.</param>
		/// <param name="w">Scalar component.</param>
		public Quaternion(float x,float y,float z,float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}
		#endregion
	}
}
