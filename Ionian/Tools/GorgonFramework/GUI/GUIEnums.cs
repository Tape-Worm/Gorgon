#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Tuesday, May 27, 2008 6:55:19 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.GUI
{
	/// <summary>
	/// Border style enumeration for panels.
	/// </summary>
	public enum PanelBorderStyle
	{
		/// <summary>
		/// No border.
		/// </summary>
		None = 0,
		/// <summary>
		/// Single thin line around the control.
		/// </summary>
		Single = 1
	}

	/// <summary>
	/// Enumeration containing mouse event types.
	/// </summary>
	public enum MouseEventType
	{
		/// <summary>
		/// Mouse was moved.
		/// </summary>
		MouseMoved = 1,
		/// <summary>
		/// Mouse button was released.
		/// </summary>
		MouseButtonUp = 2,
		/// <summary>
		/// Mouse button was pressed.
		/// </summary>
		MouseButtonDown = 3,
		/// <summary>
		/// Mouse wheel was moved.
		/// </summary>
		MouseWheelMove = 4
	}
}
