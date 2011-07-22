#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Friday, July 22, 2011 3:24:42 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Comparison operation.
	/// </summary>
	[Flags()]
	public enum GorgonCompareFlags
	{
		/// <summary>
		/// Never allow the value.
		/// </summary>
		Never = 1,
		/// <summary>
		/// Allow if the value is less than.
		/// </summary>
		Less = 2,
		/// <summary>
		/// Allow if the value is equal.
		/// </summary>
		Equal = 4,
		/// <summary>
		/// Allow if the value is less than or equal.
		/// </summary>
		LessEqual = 8,
		/// <summary>
		/// Allow if the value is greater than.
		/// </summary>
		Greater = 16,
		/// <summary>
		/// Allow if the values are not equal.
		/// </summary>
		NotEqual = 32,
		/// <summary>
		/// Allow if the value is greater than.
		/// </summary>
		GreaterEqual = 64,
		/// <summary>
		/// Always allow the value.
		/// </summary>
		Always = 128
	}
}
