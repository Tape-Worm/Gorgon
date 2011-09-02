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
// Created: Thursday, September 01, 2011 8:15:19 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// This is used to pass multi sample information around for creating multi sample/anti-aliased graphics.
	/// </summary>
	public struct GorgonMSAAQualityLevel
	{
		/// <summary>
		/// Multi sample level to assign.
		/// </summary>
		public GorgonMSAALevel Level;
		/// <summary>
		/// Quality of the multi sample level.
		/// </summary>
		public int Quality;

		/// <summary>
		/// Gorgons the multi sample level.
		/// </summary>
		/// <param name="level">The level.</param>
		/// <param name="quality">The quality.</param>
		public GorgonMSAAQualityLevel(GorgonMSAALevel level, int quality)
		{
			Level = level;
			Quality = quality;
		}
	}
}
