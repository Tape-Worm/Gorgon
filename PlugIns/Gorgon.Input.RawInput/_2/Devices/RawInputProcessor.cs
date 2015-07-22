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

using System;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Input.Raw.Properties;
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// The raw input keyboard hook used to capture and translate raw input messages.
	/// </summary>
	class RawInputProcessor
	{
		#region Methods.
		/// <summary>
		/// Function to process a raw input input message and forward it to the correct device.
		/// </summary>
		/// <param name="keyboardData">The raw input keyboard data.</param>
		/// <param name="processedData">The gorgon keyboard data processed from the raw input data.</param>
		/// <returns><b>true</b> if the message was processed, <b>false</b> if not.</returns>
		public bool ProcessRawInputMessage(ref RAWINPUTKEYBOARD keyboardData, out GorgonKeyboardData processedData)
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

			processedData = new GorgonKeyboardData
			{
				ScanCode = keyboardData.MakeCode,
				Key = (Keys)keyboardData.VirtualKey,
				Flags = flags
			};

			return true;
		}
		#endregion
	}
}
