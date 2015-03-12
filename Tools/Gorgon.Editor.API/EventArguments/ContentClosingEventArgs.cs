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
// Created: Thursday, March 5, 2015 11:12:05 PM
// 
#endregion

using System;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Event arguments for the content closing event.
	/// </summary>
	public class ContentClosingEventArgs
		: EventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return the action taken by the user.
		/// </summary>
		/// <remarks>
		/// This event argument is the action to take as specified by the user.  This value is a <see cref="ConfirmationResult" /> value and will be
		/// "yes" if the user wishes to save unsaved changes, "no" if the user does not wish to save unsaved changes or "cancel" if the user
		/// cancels the operation.
		/// <para>
		/// If the value is "none", it indicates that no user action was necessary and the operation may continue.
		/// </para>
		/// </remarks>
		public ConfirmationResult Action
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentClosingEventArgs"/> class.
		/// </summary>
		public ContentClosingEventArgs()
		{
			Action = ConfirmationResult.None;
		}
		#endregion
	}
}
