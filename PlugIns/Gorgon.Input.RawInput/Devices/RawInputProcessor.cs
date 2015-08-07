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
// Created: Thursday, July 16, 2015 10:23:36 PM
// 
#endregion

using System.Drawing;
using Forms = System.Windows.Forms;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// The raw input keyboard hook used to capture and translate raw input messages.
	/// </summary>
	class RawInputProcessor
	{
		#region Variables.
		// Router used to send the event data to the appropriate devices.
		private readonly GorgonInputDeviceEventRouting _router;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to process a raw input message and forward it to the correct device.
		/// </summary>
		/// <param name="device">Device to the forward the event data on to.</param>
		/// <param name="mouseData">The raw input mouse data.</param>
		public void ProcessRawInputMessage(IGorgonInputDevice device, ref RAWINPUTMOUSE mouseData)
		{
			short wheelDelta = 0;
			MouseButtonState state = MouseButtonState.None;

			if ((mouseData.ButtonFlags & RawMouseButtons.MouseWheel) == RawMouseButtons.MouseWheel)
			{
				wheelDelta = (short)mouseData.ButtonData;
			}

			if ((mouseData.ButtonFlags & RawMouseButtons.LeftDown) == RawMouseButtons.LeftDown)
			{
				state = MouseButtonState.ButtonLeftDown;
			}

			if ((mouseData.ButtonFlags & RawMouseButtons.RightDown) == RawMouseButtons.RightDown)
			{
				state = MouseButtonState.ButtonRightDown;
			}

			if ((mouseData.ButtonFlags & RawMouseButtons.MiddleDown) == RawMouseButtons.MiddleDown)
			{
				state = MouseButtonState.ButtonMiddleDown;
			}

			if ((mouseData.ButtonFlags & RawMouseButtons.Button4Down) == RawMouseButtons.Button4Down)
			{
				state = MouseButtonState.Button4Down;
			}

			if ((mouseData.ButtonFlags & RawMouseButtons.Button5Down) == RawMouseButtons.Button5Down)
			{
				state = MouseButtonState.Button5Down;
			}

			if ((mouseData.ButtonFlags & RawMouseButtons.LeftUp) == RawMouseButtons.LeftUp)
			{
				state = MouseButtonState.ButtonLeftUp;
			}

			if ((mouseData.ButtonFlags & RawMouseButtons.RightUp) == RawMouseButtons.RightUp)
			{
				state = MouseButtonState.ButtonRightUp;
			}

			if ((mouseData.ButtonFlags & RawMouseButtons.MiddleUp) == RawMouseButtons.MiddleUp)
			{
				state = MouseButtonState.ButtonMiddleUp;
			}

			if ((mouseData.ButtonFlags & RawMouseButtons.Button4Up) == RawMouseButtons.Button4Up)
			{
				state = MouseButtonState.Button4Up;
			}

			if ((mouseData.ButtonFlags & RawMouseButtons.Button5Up) == RawMouseButtons.Button5Up)
			{
				state = MouseButtonState.Button5Up;
			}

			Point lastRelativeMovement = new Point(mouseData.LastX, mouseData.LastY);

			var processedData = new GorgonMouseData
			                    {
				                    ButtonState = state,
				                    MouseWheelDelta = wheelDelta,
				                    Position = lastRelativeMovement,
									IsRelative = ((mouseData.Flags & RawMouseFlags.MoveAbsolute) != RawMouseFlags.MoveAbsolute)
			                    };

			_router.RouteToDevice(device, ref processedData);
		}

		/// <summary>
		/// Function to process a raw input input message and forward it to the correct device.
		/// </summary>
		/// <param name="device">Device to forward the event data on to.</param>
		/// <param name="keyboardData">The raw input keyboard data.</param>
		public void ProcessRawInputMessage(IGorgonInputDevice device, ref RAWINPUTKEYBOARD keyboardData)
		{
			KeyboardDataFlags flags = KeyboardDataFlags.KeyDown;

			if ((keyboardData.Flags & RawKeyboardFlags.KeyBreak) == RawKeyboardFlags.KeyBreak)
			{
				flags = KeyboardDataFlags.KeyUp;
			}

			if ((keyboardData.Flags & RawKeyboardFlags.KeyE0) == RawKeyboardFlags.KeyE0)
			{
				flags |= KeyboardDataFlags.LeftKey;
			}

			if ((keyboardData.Flags & RawKeyboardFlags.KeyE1) == RawKeyboardFlags.KeyE1)
			{
				flags |= KeyboardDataFlags.RightKey;
			}

			// Shift has to be handled in a special case since it doesn't actually detect left/right from raw input.
			if (keyboardData.VirtualKey == VirtualKeys.Shift)
			{
				flags |= keyboardData.MakeCode == 0x36 ? KeyboardDataFlags.RightKey : KeyboardDataFlags.LeftKey;
			}

			var processedData = new GorgonKeyboardData
			                    {
				                    ScanCode = keyboardData.MakeCode,
				                    Key = (Forms.Keys)keyboardData.VirtualKey,
				                    Flags = flags
			                    };

			_router.RouteToDevice(device, ref processedData);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawInputProcessor"/> class.
		/// </summary>
		/// <param name="router">The router for the event data.</param>
		public RawInputProcessor(GorgonInputDeviceEventRouting router)
		{
			_router = router;
		}
		#endregion
	}
}
