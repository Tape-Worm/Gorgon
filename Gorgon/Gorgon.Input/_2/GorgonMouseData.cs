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
// Created: Saturday, August 1, 2015 1:03:26 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gorgon.Input
{
	/// <summary>
	/// The current state for the mouse buttons.
	/// </summary>
	[Flags]
	public enum MouseButtonState
		: short
	{
		/// <summary>
		/// No button has a state.
		/// </summary>
		None = 0,
		/// <summary>
		/// The left button was held down (button 1).
		/// </summary>
		ButtonLeftDown = 1,
		/// <summary>
		/// The right button was held down (button 2).
		/// </summary>
		ButtonRightDown = 2,
		/// <summary>
		/// The middle button was held down (button 3).
		/// </summary>
		ButtonMiddleDown = 4,
		/// <summary>
		/// The fifth button was held down.
		/// </summary>
		Button4Down = 8,
		/// <summary>
		/// The fourth button was held down.
		/// </summary>
		Button5Down = 16,
		/// <summary>
		/// The left button was released (button 1).
		/// </summary>
		ButtonLeftUp = 32,
		/// <summary>
		/// The right button was released (button 2).
		/// </summary>
		ButtonRightUp = 64,
		/// <summary>
		/// The middle button was released (button 3).
		/// </summary>
		ButtonMiddleUp = 128,
		/// <summary>
		/// The fourth button was released.
		/// </summary>
		Button4Up = 256,
		/// <summary>
		/// The fifth button was released.
		/// </summary>
		Button5Up = 512
	}

	/// <summary>
	/// Data received from the device layer and transformed into a common data set so that the <see cref="IGorgonInputService"/> can decipher it.
	/// </summary>
	public struct GorgonMouseData
	{
		/// <summary>
		/// The direction and distance that the mouse has moved since the last event.
		/// </summary>
		public Point RelativeDirection;

		/// <summary>
		/// The change in the mouse wheel since the last event.
		/// </summary>
		public short MouseWheelDelta;

		/// <summary>
		/// The state of the mouse button
		/// </summary>
		public MouseButtonState ButtonState;
	}
}
