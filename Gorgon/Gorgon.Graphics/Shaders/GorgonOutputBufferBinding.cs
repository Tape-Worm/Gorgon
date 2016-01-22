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
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// An ouput buffer binding.
	/// </summary>
	/// <remarks>This is used to bind a buffer as a stream output.</remarks>
	public struct GorgonOutputBufferBinding
		: IEquatable<GorgonOutputBufferBinding>
	{
		#region Variables.
		/// <summary>
		/// Empty buffer binding.
		/// </summary>
		public static readonly GorgonOutputBufferBinding Empty = new GorgonOutputBufferBinding();

		/// <summary>
		/// The buffer to bind.
		/// </summary>
		public readonly GorgonBaseBuffer OutputBuffer;
		/// <summary>
		/// Offset within the buffer to start at, in bytes.
		/// </summary>
		/// <remarks>If this value is set to -1, then data will be appended to the stream output buffer.</remarks>
		public readonly int Offset;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert this binding to a Direct3D vertex buffer binding.
		/// </summary>
		/// <returns>The vertex buffer binding.</returns>
		internal D3D.StreamOutputBufferBinding Convert()
		{
			return OutputBuffer != null
				       ? new D3D.StreamOutputBufferBinding(OutputBuffer.D3DBuffer, Offset)
				       : new D3D.StreamOutputBufferBinding(null, 0);
		}

		/// <summary>
		/// Returns a <see cref="string"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="string"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
		    return string.Format(Resources.GORGFX_OUTPUTBUFFER_BINDING_TOSTR, Offset,
		                         (OutputBuffer == null || OutputBuffer.D3DBuffer == null ||
		                          OutputBuffer.Name == null)
		                             ? "(NULL)"
		                             : OutputBuffer.Name);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
		    return OutputBuffer == null
		               ? 281.GenerateHash(Offset)
		               : 281.GenerateHash(Offset).GenerateHash(OutputBuffer.GetHashCode());
		}

	    /// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		/// 	<b>true</b> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <b>false</b>.
		/// </returns>
		public override bool Equals(object obj)
		{
		    if (obj is GorgonOutputBufferBinding)
		    {
		        return Equals((GorgonOutputBufferBinding)obj);
		    }

		    return base.Equals(obj);
		}

        /// <summary>
        /// Function to determine if two instances are equal.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool Equals(ref GorgonOutputBufferBinding left, ref GorgonOutputBufferBinding right)
        {
	        return ((left.OutputBuffer == right.OutputBuffer) && (left.Offset == right.Offset));
        }

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(GorgonOutputBufferBinding left, GorgonOutputBufferBinding right)
		{
		    return Equals(ref left, ref right);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(GorgonOutputBufferBinding left, GorgonOutputBufferBinding right)
		{
            return !Equals(ref left, ref right);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonOutputBufferBinding"/> struct.
		/// </summary>
		/// <param name="buffer">The buffer to bind.</param>
		/// <param name="offset">The offset within the buffer to start at, in bytes.</param>
		/// <remarks>If the <paramref name="offset"/> parameter is set to -1, then data will be appended to the buffer from the last write.  Otherwise, 
		/// writing will occur at the offset provided.</remarks>
		public GorgonOutputBufferBinding(GorgonBaseBuffer buffer, int offset = -1)
		{			
			if ((buffer != null) && (!buffer.Settings.IsOutput))
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_BUFFER_NOT_OUTPUT, buffer.Name));
			}

			OutputBuffer = buffer;
			Offset = offset;
		}
		#endregion

		#region IEquatable<GorgonOutputBufferBinding> Members
		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="other">The other instance.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public bool Equals(GorgonOutputBufferBinding other)
		{
		    return Equals(ref this, ref other);
		}
		#endregion
	}
}
