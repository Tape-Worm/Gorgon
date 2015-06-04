﻿#region MIT.
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
// Created: Sunday, January 27, 2013 9:41:28 AM
// 
#endregion

using System;

namespace Gorgon.Core
{
	/// <summary>
	/// Extends the IEquatable{T} type to pass by reference.
	/// </summary>	
	/// <typeparam name="T">The type to use for comparison.  Must be a value type.</typeparam>
	/// <remarks>This interface extends the IEquatable{T} interface to use references in the Equals parameter.  Passing values by reference is much faster than passing by value on the stack 
	/// (if the value is a value type).
	/// <para>This is here to optimize passing value types to methods, therefore it is only suitable for value types.</para>
	/// </remarks>
	public interface IEquatableByRef<T>
		: IEquatable<T>
		where T : struct
	{
		#region Methods.
		/// <summary>
		/// Function to compare this instance with another.
		/// </summary>
		/// <param name="other">The other instance to use for comparison.</param>
		/// <returns><c>true</c> if equal, <c>false</c> if not.</returns>
		bool Equals(ref T other);
		#endregion
	}
}
