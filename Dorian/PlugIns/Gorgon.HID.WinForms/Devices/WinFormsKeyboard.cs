#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Friday, July 15, 2011 6:33:57 AM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using GorgonLibrary.Diagnostics;
using Forms = System.Windows.Forms;
using GorgonLibrary.Input;

namespace GorgonLibrary.Input.WinForms
{
	/// <summary>
	/// Object representing keyboard data.
	/// </summary>
	internal class WinFormsKeyboard
		: GorgonKeyboard
	{
		#region Variables.
		private KeyMapper _mapper = new KeyMapper();		// Key mappings.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to get the state of a key.
		/// </summary>
		/// <param name="nVirtKey">Virtual key code to retrieve.</param>
		// <returns>A bitmask containing the state of the virtual key.</returns>
		[DllImport("User32.dll"), System.Security.SuppressUnmanagedCodeSecurity()]
		private static extern short GetKeyState(Forms.Keys nVirtKey);

		/// <summary>
		/// Function to retrieve the scan code for a virtual key.
		/// </summary>
		/// <param name="uCode">Virtual key code</param>
		/// <param name="uMapType">Mapping type.</param>
		/// <returns>The scan code.</returns>
		[DllImport("user32.dll", CharSet = CharSet.Auto), System.Security.SuppressUnmanagedCodeSecurity()]
		private static extern int MapVirtualKey(KeyboardKeys uCode, int uMapType);

		/// <summary>
		/// Function to process the keyboard state.
		/// </summary>
		/// <param name="keyEventArgs">Event parameters from the keyboard events.</param>
		/// <param name="state">State of the key, up or down.</param>
		private void ProcessKeys(Forms.KeyEventArgs keyEventArgs, KeyState state)
		{
			KeyboardKeys keyCode;	// Code for the keyboard.

			if ((BoundControl == null) || (BoundControl.Disposing))
				return;

			if (_mapper.KeyMapping.ContainsKey(keyEventArgs.KeyCode))
				keyCode = _mapper.KeyMapping[keyEventArgs.KeyCode];
			else
				return;

			// Check for modifiers.
			switch(keyCode)
			{
				case KeyboardKeys.ControlKey:
					if ((GetKeyState(Forms.Keys.LControlKey) & 0x80) == 0x80)
						keyCode = KeyboardKeys.LControlKey;
					if ((GetKeyState(Forms.Keys.RControlKey) & 0x80) == 0x80)
						keyCode = KeyboardKeys.RControlKey;
					KeyStates[KeyboardKeys.ControlKey] = state;
					break;
				case KeyboardKeys.Menu:
					if ((GetKeyState(Forms.Keys.LMenu) & 0x80) == 0x80)
						keyCode = KeyboardKeys.LMenu;
					if ((GetKeyState(Forms.Keys.RMenu) & 0x80) == 0x80)
						keyCode = KeyboardKeys.RMenu;
					KeyStates[KeyboardKeys.Menu] = state;
					break;
				case KeyboardKeys.ShiftKey:
					if ((GetKeyState(Forms.Keys.LShiftKey) & 0x80) == 0x80)
						keyCode = KeyboardKeys.LShiftKey;
					if ((GetKeyState(Forms.Keys.RShiftKey) & 0x80) == 0x80)
						keyCode = KeyboardKeys.RShiftKey;
					KeyStates[KeyboardKeys.ShiftKey] = state;
					break;
			}

			KeyStates[keyCode] = state;

			if (state == KeyState.Down)
				OnKeyDown(keyCode, MapVirtualKey(keyCode, 0));
			else
				OnKeyUp(keyCode, MapVirtualKey(keyCode, 0));
		}

		/// <summary>
		/// Handles the KeyDown event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_KeyDown(object sender, Forms.KeyEventArgs e)
		{
			ProcessKeys(e, KeyState.Down);
		}

		/// <summary>
		/// Handles the KeyUp event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_KeyUp(object sender, Forms.KeyEventArgs e)
		{
			ProcessKeys(e, KeyState.Up);
		}

		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected override void BindDevice()
		{
			this.BoundControl.KeyUp += new Forms.KeyEventHandler(BoundWindow_KeyUp);
			this.BoundControl.KeyDown += new Forms.KeyEventHandler(BoundWindow_KeyDown);
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
			this.BoundControl.KeyUp -= new Forms.KeyEventHandler(BoundWindow_KeyUp);
			this.BoundControl.KeyDown -= new Forms.KeyEventHandler(BoundWindow_KeyDown);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="WinFormsKeyboard"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="boundWindow">The window to bind this device with.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</remarks>
		internal WinFormsKeyboard(GorgonWinFormsDeviceFactory owner, Forms.Control boundWindow)
			: base(owner, "Win Forms Input Keyboard", boundWindow)
		{
			Gorgon.Log.Print("Win Forms input keyboard interface created.", GorgonLoggingLevel.Verbose);
		}
		#endregion
	}
}
