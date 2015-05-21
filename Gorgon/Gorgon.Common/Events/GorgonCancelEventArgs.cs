﻿#region MIT.
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
// Created: Wednesday, November 02, 2011 6:19:58 AM
// 
#endregion

using System;

namespace Gorgon
{
	/// <summary>
	/// A base cancel event argument.
	/// </summary>
	public class GorgonCancelEventArgs
		: EventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to set or return whether an operation should be canceled or not.
		/// </summary>
		public bool Cancel
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCancelEventArgs"/> class.
		/// </summary>
		/// <param name="cancel">TRUE if the operation should be canceled, FALSE if not.</param>
		public GorgonCancelEventArgs(bool cancel)
		{
			Cancel = cancel;
		}
		#endregion
	}
}
