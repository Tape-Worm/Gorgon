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
// Created: Saturday, August 06, 2011 12:28:54 PM
// 
#endregion

using System;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Intervals used for waiting for vertical retraces (vsync).
	/// </summary>
	[Flags()]
	public enum GorgonVSyncInterval
	{
		/// <summary>
		/// Display immediately.
		/// </summary>
		None = 1,
		/// <summary>
		/// Wait for one vertical retrace.
		/// </summary>
		One = 2,
		/// <summary>
		/// Wait for two vertical retraces.
		/// </summary>
		Two = 4,
		/// <summary>
		/// Wait for three vertical retraces.
		/// </summary>
		Three = 8,
		/// <summary>
		/// Wait for four vertical retraces.
		/// </summary>
		Four = 16
	}
}