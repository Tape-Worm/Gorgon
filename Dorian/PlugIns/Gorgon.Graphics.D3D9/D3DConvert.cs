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
// Created: Friday, July 22, 2011 3:32:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D9;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// Utility to convert values between Gorgon and Direct 3D.
	/// </summary>
	class D3DConvert
	{
		#region Methods.
		/// <summary>
		/// Function to convert Gorgon comparison flags to the D3D compare value.
		/// </summary>
		/// <param name="compareFlags">Comparison flags to convert.</param>
		/// <returns>The Direct3D comparison flag.</returns>
		public Compare Convert(GorgonCompareFlags compareFlags)
		{
			switch (compareFlags)
			{
				case GorgonCompareFlags.Equal:
					return Compare.Equal;
				case GorgonCompareFlags.Greater:
					return Compare.Greater;
				case GorgonCompareFlags.GreaterEqual:
					return Compare.GreaterEqual;
				case GorgonCompareFlags.Less:
					return Compare.Less;
				case GorgonCompareFlags.LessEqual:
					return Compare.LessEqual;
				case GorgonCompareFlags.Never:
					return Compare.Never;
				case GorgonCompareFlags.NotEqual:
					return Compare.NotEqual;
				default:
					return Compare.Always;
			}
		}
		#endregion
	}
}
