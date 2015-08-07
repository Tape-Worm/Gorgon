﻿#region MIT
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
// Created: Saturday, August 1, 2015 12:29:34 PM
// 
#endregion

using System;
using System.Drawing;

namespace Gorgon.Input
{
	/// <summary>
	/// Mouse event arguments.
	/// </summary>
	public class GorgonMouseEventArgs
		: EventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return the buttons that were held down during a <see cref="IGorgonMouse"/> event.
		/// </summary>
		public MouseButtons Buttons
		{
			get;
		}

		/// <summary>
		/// Property to return the buttons that were being held down in conjunction with the <see cref="Buttons"/> during a <see cref="IGorgonMouse"/> event.
		/// </summary>
		public MouseButtons ShiftButtons
		{
			get;
		}

		/// <summary>
		/// Property to return the position of the mouse.
		/// </summary>
		public Point Position
		{
			get;
		}

		/// <summary>
		/// Property to return the wheel position.
		/// </summary>
		/// <remarks>
		/// If there is no mouse wheel present, this value will return 0.
		/// </remarks>
		public int WheelPosition
		{
			get;
		}

		/// <summary>
		/// Property to return the amount that the mouse has moved since the last <see cref="IGorgonMouse"/> event.
		/// </summary>
		public Point RelativePosition
		{
			get;
		}

		/// <summary>
		/// Property to return the amount that the wheel has moved since the last <see cref="IGorgonMouse"/> event.
		/// </summary>
		/// <remarks>
		/// If this is no mouse wheel present, this value will return 0.
		/// </remarks>
		public int WheelDelta
		{
			get;
		}

		/// <summary>
		/// Property to return if a double click caused the event to trigger.
		/// </summary>
		public bool DoubleClick => (ClickCount > 1);

		/// <summary>
		/// Property to return the number of full clicks.
		/// </summary>
		public int ClickCount
		{
			get;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMouseEventArgs" /> class.
		/// </summary>
		/// <param name="buttons">Buttons that are pressed during mouse event.</param>
		/// <param name="shiftButtons">Buttons that are held down during the mouse event.</param>
		/// <param name="position">Position of the mouse.</param>
		/// <param name="wheelPosition">Position of the wheel.</param>
		/// <param name="relativePosition">Relative position of the mouse.</param>
		/// <param name="wheelDelta">Relative position of the wheel.</param>
		/// <param name="clickCount">Number of clicks in a timed period.</param>
		public GorgonMouseEventArgs(MouseButtons buttons, MouseButtons shiftButtons, Point position, int wheelPosition, Point relativePosition, int wheelDelta, int clickCount)
		{
			Buttons = buttons;
			ShiftButtons = shiftButtons;
			Position = position;
			WheelPosition = wheelPosition;
			RelativePosition = relativePosition;
			WheelDelta = wheelDelta;
			ClickCount = clickCount;
		}
		#endregion
	}
}
