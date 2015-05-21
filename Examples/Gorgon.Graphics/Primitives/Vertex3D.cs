#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Thursday, August 7, 2014 10:06:17 PM
// 
#endregion

using System.Runtime.InteropServices;
using Gorgon.Native;
using SlimMath;

namespace Gorgon.Graphics.Example
{
	/// <summary>
	/// The layout for our vertex.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Vertex3D
	{
		#region Variables.
		/// <summary>
		/// Size of the vertex, in bytes.
		/// </summary>
		public static readonly int Size = DirectAccess.SizeOf<Vertex3D>();

		/// <summary>
		/// The position of the vertex.
		/// </summary>
		[InputElement(0, "SV_POSITION")]
		public Vector4 Position;

		/// <summary>
		/// The position of the vertex normal.
		/// </summary>
		[InputElement(1, "NORMAL")]
		public Vector3 Normal;

		/// <summary>
		/// The texture coordinates.
		/// </summary>
		[InputElement(2, "TEXCOORD")]
		public Vector2 UV;
		
        /// <summary>
        /// The tangent vector.
        /// </summary>
        [InputElement(3, "TANGENT")]
	    public Vector4 Tangent;
	    #endregion
	}
}
