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
// Created: Sunday, January 15, 2012 9:42:51 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DX = SlimDX;

namespace GorgonLibrary.Math
{
	/// <summary>
	/// Conversion utilities for math types.
	/// </summary>
	internal static class GorgonMathConvert
	{
		///// <summary>
		///// Function to convert a Gorgon vector into a DirectX vector type.
		///// </summary>
		///// <param name="vector">The Gorgon vector vector to convert from.</param>
		///// <returns>The DirectX vector.</returns>
		//public static DX.Vector2 Convert(this GorgonVector2 vector)
		//{
		//    return new DX.Vector2(vector.X, vector.Y);
		//}

		///// <summary>
		///// Function to convert a DirectX vector into a Gorgon vector type.
		///// </summary>
		///// <param name="vector">The DirectX vector vector to convert from.</param>
		///// <returns>The Gorgon vector.</returns>
		//public static GorgonVector2 Convert(this DX.Vector2 vector)
		//{
		//    return new GorgonVector2(vector.X, vector.Y);
		//}

		///// <summary>
		///// Function to convert a Gorgon vector into a DirectX vector type.
		///// </summary>
		///// <param name="vector">The Gorgon vector vector to convert from.</param>
		///// <returns>The DirectX vector.</returns>
		//public static DX.Vector3 Convert(this GorgonVector3 vector)
		//{
		//    return new DX.Vector3(vector.X, vector.Y, vector.Z);
		//}

		///// <summary>
		///// Function to convert a DirectX vector into a Gorgon vector type.
		///// </summary>
		///// <param name="vector">The DirectX vector vector to convert from.</param>
		///// <returns>The Gorgon vector.</returns>
		//public static GorgonVector3 Convert(this DX.Vector3 vector)
		//{
		//    return new GorgonVector3(vector.X, vector.Y, vector.Z);
		//}

		///// <summary>
		///// Function to convert a Gorgon vector into a DirectX vector type.
		///// </summary>
		///// <param name="vector">The Gorgon vector vector to convert from.</param>
		///// <returns>The DirectX vector.</returns>
		//public static DX.Vector4 Convert(this GorgonVector4 vector)
		//{
		//    return new DX.Vector4(vector.X, vector.Y, vector.Z, vector.W);
		//}

		///// <summary>
		///// Function to convert a DirectX vector into a Gorgon vector type.
		///// </summary>
		///// <param name="vector">The DirectX vector vector to convert from.</param>
		///// <returns>The Gorgon vector.</returns>
		//public static GorgonVector4 Convert(this DX.Vector4 vector)
		//{
		//    return new GorgonVector4(vector.X, vector.Y, vector.Z, vector.W);
		//}

		///// <summary>
		///// Function to convert a Gorgon matrix into a DirectX matrix type.
		///// </summary>
		///// <param name="matrix">The Gorgon matrix to convert.</param>
		///// <returns>The DirectX matrix.</returns>
		//public static DX.Matrix Convert(this GorgonMatrix matrix)
		//{
		//    return new DX.Matrix(	matrix.m11, matrix.m12, matrix.m13, matrix.m14,
		//                            matrix.m21, matrix.m22, matrix.m23, matrix.m24,
		//                            matrix.m31, matrix.m32, matrix.m33, matrix.m34,
		//                            matrix.m41, matrix.m42, matrix.m43, matrix.m44);
		//}

		///// <summary>
		///// Function to convert a DirectX matrix into a Gorgon matrix type.
		///// </summary>
		///// <param name="matrix">The DirectX matrix to convert.</param>
		///// <returns>The Gorgon matrix.</returns>
		//public static GorgonMatrix Convert(this DX.Matrix matrix)
		//{
		//    return new GorgonMatrix(matrix.M11, matrix.M12, matrix.M13, matrix.M14,
		//                            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
		//                            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
		//                            matrix.M41, matrix.M42, matrix.M43, matrix.M44);
		//}
	}
}
