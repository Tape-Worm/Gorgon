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
// Created: Monday, March 2, 2015 8:16:17 PM
// 
#endregion

using System;

namespace Gorgon.Editor
{
	/// <summary>
	/// Event parameters used after content is renamed.
	/// </summary>
	public class AfterContentRenamedArgs
		: EventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return the old name of the content.
		/// </summary>
		public string OldName
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the new name of the content.
		/// </summary>
		public string NewName
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="AfterContentRenamedArgs"/> class.
		/// </summary>
		/// <param name="oldName">The old name of the content.</param>
		/// <param name="newName">The new name of the content.</param>
		public AfterContentRenamedArgs(string oldName, string newName)
		{
			OldName = oldName;
			NewName = newName;
		}
		#endregion
	}
}
