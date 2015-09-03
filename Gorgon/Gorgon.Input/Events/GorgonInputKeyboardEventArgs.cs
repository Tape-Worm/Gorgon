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
// Created: Wednesday, July 15, 2015 7:00:06 PM
// 
#endregion

using System;

namespace Gorgon.Input
{
	/// <summary>
	/// Event arguments for the various events triggered on the <see cref="GorgonKeyboard2"/> interface.
	/// </summary>
	public class GorgonInputKeyboardEventArgs
		: EventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return the character that the key represents.
		/// </summary>
		public GorgonKeyCharMap CharacterMapping
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return key that is pressed.
		/// </summary>
		public KeyboardKey Key
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the keys that are being held down during the event.
		/// </summary>
		public KeyboardKey ModifierKeys
		{
			get;
		}

		/// <summary>
		/// Property to return if ALT is pressed or not.
		/// </summary>
		public bool Alt => (ModifierKeys & KeyboardKey.Alt) == KeyboardKey.Alt;

		/// <summary>
		/// Property to return if Ctrl is pressed or not.
		/// </summary>
		public bool Ctrl => (ModifierKeys & KeyboardKey.Control) == KeyboardKey.Control;

		/// <summary>
		/// Property to return if Shift is pressed or not.
		/// </summary>
		public bool Shift => (ModifierKeys & KeyboardKey.Shift) == KeyboardKey.Shift;

		/// <summary>
		/// Property to return the scan code data.
		/// </summary>
		public int ScanCodeData
		{
			get;
			private set;
		}

		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputKeyboardEventArgs"/> class.
		/// </summary>
		/// <param name="key">Key that is pressed.</param>
		/// <param name="modifierKey">Keys that are held down during the event.</param>
		/// <param name="character">Character that the key represents.</param>
		/// <param name="scanData">Scan code data.</param>
		public GorgonInputKeyboardEventArgs(KeyboardKey key, KeyboardKey modifierKey, GorgonKeyCharMap character, int scanData)
		{
			Key = key;
			ModifierKeys = modifierKey;
			CharacterMapping = character;
			ScanCodeData = scanData;
		}
		#endregion

	}
}
