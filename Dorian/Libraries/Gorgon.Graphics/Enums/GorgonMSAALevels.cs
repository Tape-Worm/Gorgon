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
// Created: Saturday, August 06, 2011 9:29:18 AM
// 
#endregion

using System;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Values to specify the level of multi-sampling to perform.
	/// </summary>
	[Flags()]
	public enum GorgonMSAALevel
	{
		/// <summary>
		/// No multi-sampling.
		/// </summary>
		None = 0,
		/// <summary>
		/// Enables multi-sampling quality.
		/// </summary>
		NonMasked = 1,
		/// <summary>
		/// Level 2 sampling.
		/// </summary>
		Level2 = 2,
		/// <summary>
		/// Level 3 sampling.
		/// </summary>
		Level3 = 4,
		/// <summary>
		/// Level 4 sampling.
		/// </summary>
		Level4 = 8,
		/// <summary>
		/// Level 5 sampling.
		/// </summary>
		Level5 = 16,
		/// <summary>
		/// Level 6 sampling.
		/// </summary>
		Level6 = 32,
		/// <summary>
		/// Level 7 sampling.
		/// </summary>
		Level7 = 64,
		/// <summary>
		/// Level 8 sampling.
		/// </summary>
		Level8 = 128,
		/// <summary>
		/// Level 9 sampling.
		/// </summary>
		Level9 = 256,
		/// <summary>
		/// Level 10 sampling.
		/// </summary>
		Level10 = 512,
		/// <summary>
		/// Level 11 sampling.
		/// </summary>
		Level11 = 1024,
		/// <summary>
		/// Level 12 sampling.
		/// </summary>
		Level12 = 2048,
		/// <summary>
		/// Level 13 sampling.
		/// </summary>
		Level13 = 4096,
		/// <summary>
		/// Level 14 sampling.
		/// </summary>
		Level14 = 8192,
		/// <summary>
		/// Level 15 sampling.
		/// </summary>
		Level15 = 16384,
		/// <summary>
		/// Level 16 sampling.
		/// </summary>
		Level16 = 32768
	}
}