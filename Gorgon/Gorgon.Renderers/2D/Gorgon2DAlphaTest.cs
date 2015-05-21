#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Friday, August 9, 2013 3:16:00 PM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using Gorgon.Core.Extensions;

namespace Gorgon.Renderers
{
	/// <summary>
	/// An immutable value for alpha testing.
	/// </summary>
	/// <remarks>This will define the range of alpha values to clip.  An alpha value that falls between the lower and upper range will not be rendered.</remarks>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
	struct Gorgon2DAlphaTest
		: IEquatable<Gorgon2DAlphaTest>
	{
		#region Variables.
		/// <summary>
		/// 4 byte compatiable flag for constant buffer.
		/// </summary>
		public readonly int IsEnabled;			

		/// <summary>
		/// Lower alpha value.
		/// </summary>
		/// <remarks>If the alpha is higher than this value, it will be clipped.</remarks>
		public readonly float LowerAlpha;

		/// <summary>
		/// Upper alpha value.
		/// </summary>
		/// <remarks>If the alpha is lower than this value, it will be clipped.</remarks>
		public readonly float UpperAlpha;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine equality between two instances.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool Equals(ref Gorgon2DAlphaTest left, ref Gorgon2DAlphaTest right)
		{
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return ((left.IsEnabled == right.IsEnabled) &&
			        (left.UpperAlpha == right.UpperAlpha) && (left.LowerAlpha == right.LowerAlpha));
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is Gorgon2DAlphaTest)
			{
				return ((Gorgon2DAlphaTest)obj).Equals(this);
			}

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
			return 281.GenerateHash(IsEnabled).GenerateHash(LowerAlpha).GenerateHash(UpperAlpha);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DAlphaTest"/> struct.
		/// </summary>
		/// <param name="isEnabled">TRUE to enable alpha testing, FALSE to disable.</param>
		/// <param name="alphaRange">The alpha range to clip.</param>
		public Gorgon2DAlphaTest(bool isEnabled, GorgonRangeF alphaRange)
		{
			IsEnabled = isEnabled ? 1 : 0;
			LowerAlpha = alphaRange.Minimum;
			UpperAlpha = alphaRange.Maximum;
		}
		#endregion

		#region IEquatable<Gorgon2DAlphaTest> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		public bool Equals(Gorgon2DAlphaTest other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}
}
