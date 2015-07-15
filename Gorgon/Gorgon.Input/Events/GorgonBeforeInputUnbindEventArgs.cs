#region MIT
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
// Created: Sunday, July 5, 2015 10:03:51 PM
// 
#endregion

using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Gorgon.Input.Events
{
	/// <summary>
	/// Event arguments for the <see cref="IGorgonInputService.BeforeUnbind"/> event.
	/// </summary>
	public class GorgonBeforeInputUnbindEventArgs
		: EventArgs
	{
		/// <summary>
		/// Property to return the control bound to the input device.
		/// </summary>
		public Control BoundControl
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the form that holds the <see cref="BoundControl"/>
		/// </summary>
		public Form BoundForm
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBeforeInputUnbindEventArgs"/> class.
		/// </summary>
		/// <param name="boundControl">The bound control.</param>
		internal GorgonBeforeInputUnbindEventArgs(Control boundControl)
		{
			Debug.Assert(boundControl != null, "No bound control found!");

			BoundControl = boundControl;
			BoundForm = boundControl.FindForm();
		}
	}
}
