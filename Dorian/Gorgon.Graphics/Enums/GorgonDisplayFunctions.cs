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
// Created: Saturday, August 06, 2011 12:47:51 PM
// 
#endregion

using System;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Functions for how the system should copy its data to the front buffer during a page flip.
	/// </summary>
	/// <remarks>Both the Flip and Copy values may cause a performance bottleneck, Discard is the recommended choice.</remarks>
	public enum GorgonDisplayFunction
	{
		/// <summary>
		/// Discard the buffer contents after a page flip.
		/// </summary>
		Discard = 0,
		/// <summary>
		/// Recycles each buffer in the swap chain after a display operation.
		/// </summary>
		Flip = 1,
		/// <summary>
		/// Reuse the buffer.  Note that this can only be used in a swap chain with exactly 1 buffer.
		/// </summary>
		Copy = 2
	}
}