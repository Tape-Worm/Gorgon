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
// Created: Saturday, July 18, 2015 4:38:03 PM
// 
#endregion

using System;
using System.Windows.Forms;

namespace Gorgon.Input
{
	/// <summary>
	/// Flags to help determine the type of data within the <see cref="GorgonKeyboardData"/> interface.
	/// </summary>
	[Flags]
	public enum KeyboardDataFlags
	{
		/// <summary>
		/// The key is being held down.
		/// </summary>
		KeyDown = 0,
		/// <summary>
		/// The key has been released.
		/// </summary>
		KeyUp = 1,
		/// <summary>
		/// The left version of the key (for Alt, Control, Shift, etc...)
		/// </summary>
		LeftKey = 2,
		/// <summary>
		/// The right version of the key (for Alt, Control, Shift, etc...)
		/// </summary>
		RightKey = 4
	}

	/// <summary>
	/// Data received from the device layer and transformed into a common data set so that the <see cref="IGorgonInputService"/> can decipher it.
	/// </summary>
	public struct GorgonKeyboardData
	{
		/// <summary>
		/// Property to return the scan code for the key.
		/// </summary>
		public int ScanCode;

		/// <summary>
		/// Property to return flags for the scan code information.
		/// </summary>
		public KeyboardDataFlags Flags;

		/// <summary>
		/// Property to return the key being used.
		/// </summary>
		public Keys Key;
	}
}
