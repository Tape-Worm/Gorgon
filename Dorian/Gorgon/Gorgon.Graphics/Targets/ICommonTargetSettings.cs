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
// Created: Tuesday, November 22, 2011 11:45:13 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Interface for common settings for swap chains, render targets and depth buffers.
	/// </summary>
	public interface ICommonTargetSettings
	{
		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the buffer format for the target
		/// </summary>
		GorgonBufferFormat Format
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the multisampling values for the target.
		/// </summary>
		GorgonMultiSampling MultiSample
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the width of the target.
		/// </summary>
		int Width
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the height of the target.
		/// </summary>
		int Height
		{
			get;
			set;
		}
		#endregion
	}
}
