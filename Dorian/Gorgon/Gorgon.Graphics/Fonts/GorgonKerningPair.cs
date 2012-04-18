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
// Created: Tuesday, April 17, 2012 9:15:18 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A kerning pair value.
	/// </summary>
	/// <remarks>Kerning pairs are used to offset a pair of characters when they are next to each other.</remarks>
	public struct GorgonKerningPair
		: IEquatable<GorgonKerningPair>
	{
		#region Variables.
		/// <summary>
		/// Left character.
		/// </summary>
		public char LeftCharacter;
		/// <summary>
		/// Right character.
		/// </summary>
		public char RightCharacter;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if 2 kerning pairs are the same.
		/// </summary>
		/// <param name="left">Left kerning pair to compare.</param>
		/// <param name="right">Right kerning pair to compare.</param>
		/// <returns>TRUE if the same, FALSE if not.</returns>
		public static bool Equals(GorgonKerningPair left, GorgonKerningPair right)
		{
			return ((left.LeftCharacter == right.LeftCharacter) && (left.RightCharacter == right.RightCharacter));
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonKerningPair)
				return Equals((GorgonKerningPair)obj, this);

			return base.Equals(obj);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(LeftCharacter).GenerateHash(RightCharacter);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return "Kerning Pair: " + LeftCharacter.ToString() + ", " + RightCharacter.ToString();
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==(GorgonKerningPair left, GorgonKerningPair right)
		{
			return GorgonKerningPair.Equals(left, right);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(GorgonKerningPair left, GorgonKerningPair right)
		{
			return !GorgonKerningPair.Equals(left, right);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonKerningPair"/> struct.
		/// </summary>
		/// <param name="leftChar">The left char.</param>
		/// <param name="rightChar">The right char.</param>
		public GorgonKerningPair(char leftChar, char rightChar)
		{
			LeftCharacter = leftChar;
			RightCharacter = rightChar;
		}
		#endregion

		#region IEquatable<GorgonKerningPair> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonKerningPair other)
		{
			return GorgonKerningPair.Equals(this, other);
		}
		#endregion
	}
}
