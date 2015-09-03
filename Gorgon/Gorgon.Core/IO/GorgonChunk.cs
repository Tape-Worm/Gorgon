#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Sunday, June 14, 2015 5:59:28 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Core.Properties;

namespace Gorgon.IO
{
	/// <summary>
	/// A chunk for the chunked file format.
	/// </summary>
	public struct GorgonChunk
		: IEquatable<GorgonChunk>
	{
		#region Variables.
		/// <summary>
		/// The ID for the chunk.
		/// </summary>
		public readonly ulong ID;

		/// <summary>
		/// The size of the chunk, in bytes.
		/// </summary>
		public readonly int Size;

		/// <summary>
		/// The offset, in bytes, of the chunk within the chunked file.
		/// </summary>
		/// <remarks>This is relative to the header of the file.</remarks>
		public readonly ulong FileOffset;
		#endregion

		#region Methods.
		/// <summary>
		/// Returns a <see cref="string" /> that represents this instance.
		/// </summary>
		/// <returns>A <see cref="string" /> that represents this instance.</returns>
		public override string ToString()
		{
			return string.Format(Resources.GOR_TOSTR_GORGONCHUNK, ID.FormatHex(), FileOffset.FormatHex(), Size.FormatHex());
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(ID);
		}

		/// <summary>
		/// Function to compare two instances for equality.
		/// </summary>
		/// <param name="left">The first object of type <see cref="GorgonChunk"/> to compare.</param>
		/// <param name="right">The second object of type <see cref="GorgonChunk"/> to compare.</param>
		/// <returns><b>true</b> if the specified objects are equal; otherwise, <b>false</b> if not.</returns>
		public static bool Equals(ref GorgonChunk left, ref GorgonChunk right)
		{
			return left.ID == right.ID;
		}

		/// <summary>
		/// Function to compare two instances for equality.
		/// </summary>
		/// <param name="other">The object of type <see cref="GorgonChunk"/> to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> otherwise.</returns>
		public bool Equals(GorgonChunk other)
		{
			return Equals(ref this, ref other);
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.</returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonChunk)
			{
				return ((GorgonChunk)obj).Equals(this);
			}
			return base.Equals(obj);
		}
		#endregion

		#region Operators.
		/// <summary>
		/// Operator used to compare two instances for equality.
		/// </summary>
		/// <param name="left">The left instance to compare.</param>
		/// <param name="right">The right instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> otherwise.</returns>
		public static bool operator ==(GorgonChunk left, GorgonChunk right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Operator used to compare two instances for inequality.
		/// </summary>
		/// <param name="left">The left instance to compare.</param>
		/// <param name="right">The right instance to compare.</param>
		/// <returns><b>true</b> if not equal, <b>false</b> otherwise.</returns>
		public static bool operator !=(GorgonChunk left, GorgonChunk right)
		{
			return !Equals(ref left, ref right);
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonChunk"/> struct.
		/// </summary>
		/// <param name="id">The identifier for the chunk.</param>
		/// <param name="size">The size of the chunk, in bytes.</param>
		/// <param name="offset">The offset within the file, in bytes.</param>
		public GorgonChunk(ulong id, int size, ulong offset)
		{
			ID = id;
			Size = size;
			FileOffset = offset;
		}
		#endregion
	}
}
