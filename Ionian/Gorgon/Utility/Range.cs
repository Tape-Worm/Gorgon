#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, May 24, 2008 1:08:50 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary
{
	/// <summary>
	/// Value type to indicate a range of values.
	/// </summary>
	/// <typeparam name="T">Type of data for the range.</typeparam>
	public struct Range<T>
	{
		/// <summary>
		/// Starting point in the range.
		/// </summary>
		public T Start;

		/// <summary>
		/// Ending point in the range.
		/// </summary>
		public T End;

		/// <summary>
		/// Initializes a new instance of the <see cref="Range&lt;T&gt;"/> struct.
		/// </summary>
		/// <param name="start">The starting value.</param>
		/// <param name="end">The ending value.</param>
		public Range(T start, T end)
		{
			Start = start;
			End = end;
		}
	}
}
