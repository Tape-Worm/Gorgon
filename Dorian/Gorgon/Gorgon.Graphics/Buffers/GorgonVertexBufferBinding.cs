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
// Created: Wednesday, February 01, 2012 9:40:15 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A vertex buffer binding.
	/// </summary>
	public struct GorgonVertexBufferBinding
		: IEquatable<GorgonVertexBufferBinding>
	{
		#region Variables.
		/// <summary>
		/// Empty vertex buffer binding.
		/// </summary>
		public static readonly GorgonVertexBufferBinding Empty = new GorgonVertexBufferBinding();

		/// <summary>
		/// The vertex buffer to bind.
		/// </summary>
		public GorgonVertexBuffer VertexBuffer;
		/// <summary>
		/// Stride of the items within the vertex buffer, in bytes.
		/// </summary>
		public int Stride;
		/// <summary>
		/// Offset within the buffer to start at, in bytes.
		/// </summary>
		public int Offset;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert this binding to a Direct3D vertex buffer binding.
		/// </summary>
		/// <returns>The vertex buffer binding.</returns>
		internal D3D.VertexBufferBinding Convert()
		{
			if (VertexBuffer != null)
				return new D3D.VertexBufferBinding((D3D.Buffer)VertexBuffer.D3DResource, Stride, Offset);
			else
				return new D3D.VertexBufferBinding(null, 0, 0);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return "Gorgon vertex buffer binding";
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			if (VertexBuffer != null)
				return 281.GenerateHash(Stride).GenerateHash(Offset);
			else
				return 281.GenerateHash(Stride).GenerateHash(Offset).GenerateHash(VertexBuffer.GetHashCode());
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonVertexBufferBinding)
				return this.Equals((GorgonVertexBufferBinding)obj);

			return base.Equals(obj);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(GorgonVertexBufferBinding left, GorgonVertexBufferBinding right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(GorgonVertexBufferBinding left, GorgonVertexBufferBinding right)
		{
			return !left.Equals(right);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexBufferBinding"/> struct.
		/// </summary>
		/// <param name="buffer">The buffer to bind.</param>
		/// <param name="stride">The stride of the data in the buffer, in bytes.</param>
		/// <param name="offset">The offset within the buffer to start at, in bytes.</param>
		public GorgonVertexBufferBinding(GorgonVertexBuffer buffer, int stride, int offset)
		{			
			VertexBuffer = buffer;
			Stride = stride;
			Offset = offset;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexBufferBinding"/> struct.
		/// </summary>
		/// <param name="buffer">The buffer.</param>
		/// <param name="stride">The stride.</param>
		public GorgonVertexBufferBinding(GorgonVertexBuffer buffer, int stride)
		{
			VertexBuffer = buffer;
			Stride = stride;
			Offset = 0;
		}
		#endregion

		#region IEquatable<GorgonVertexBufferBinding> Members
		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="other">The other instance.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public bool Equals(GorgonVertexBufferBinding other)
		{
			return ((other.VertexBuffer == VertexBuffer) && (other.Stride == Stride) && (other.Offset == Offset));
		}
		#endregion
	}
}
