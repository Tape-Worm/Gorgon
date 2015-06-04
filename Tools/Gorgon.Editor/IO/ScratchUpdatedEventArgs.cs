#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Thursday, March 12, 2015 11:53:19 AM
// 
#endregion

using System;

namespace Gorgon.Editor
{
	/// <summary>
	/// Event arguments for the ScratchUpdated event.
	/// </summary>
	public class ScratchUpdatedEventArgs
		: EventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return the path to the item that was updated.
		/// </summary>
		public string Path
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.

		/// <summary>
		/// Initializes a new instance of the <see cref="ScratchUpdatedEventArgs"/> class.
		/// </summary>
		/// <param name="path">Virtual file system path to the item that was updated.</param>
		public ScratchUpdatedEventArgs(string path)
		{
			Path = path;
		}
		#endregion
	}
}
