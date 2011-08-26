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
// Created: Thursday, August 25, 2011 8:17:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Diagnostics
{
	/// <summary>
	/// Debugging utility methods.
	/// </summary>
	public static class GorgonDebug
	{
		#region Methods.
		/// <summary>
		/// Function to throw an exception if a string is null or empty.
		/// </summary>
		/// <param name="value">The value being passed.</param>
		/// <param name="paramName">The name of the parameter.</param>
		/// <remarks>This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.</remarks>
		public static void AssertParamString(string value, string paramName)
		{
#if DEBUG
			if (value == null)
				throw new ArgumentNullException(paramName);
			if (value == string.Empty)
				throw new ArgumentException("The parameter must not be a zero-length string.", paramName);
#endif
		}
		#endregion
	}
}
