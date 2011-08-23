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

namespace GorgonLibrary.Input.WinFormsInput
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
		/// Function to process the keyboard state.
		/// </summary>
		/// <param name="keyEventArgs">Event parameters from the keyboard events.</param>
		/// <param name="state">State of the key, up or down.</param>
		private void ProcessKeys(Forms.KeyEventArgs keyEventArgs, KeyState state)
		{
			KeyboardKeys modifiers;	// Keyboard modifiers.
			bool left = false;
			bool right = false;

			if ((BoundWindow == null) || (BoundWindow.Disposing))
				return;

			KeyStates[KeyboardKeys.LMenu] = KeyState.Up;
			KeyStates[KeyboardKeys.RMenu] = KeyState.Up;
			KeyStates[KeyboardKeys.Menu] = KeyState.Up;
			KeyStates[KeyboardKeys.LControlKey] = KeyState.Up;
			KeyStates[KeyboardKeys.RControlKey] = KeyState.Up;
			KeyStates[KeyboardKeys.Control] = KeyState.Up;
			KeyStates[KeyboardKeys.LShiftKey] = KeyState.Up;
			KeyStates[KeyboardKeys.RShiftKey] = KeyState.Up;
			KeyStates[KeyboardKeys.Shift] = KeyState.Up;

			modifiers = KeyboardKeys.None;			

			// Check for modifiers.
			if ((GetKeyState(Forms.Keys.ControlKey) & 0x80) == 0x80)
			{
				if ((GetKeyState(Forms.Keys.LControlKey) & 0x80) == 0x80)
				{
					KeyStates[KeyboardKeys.LControlKey] = state;
					left = true;
				}
				if ((GetKeyState(Forms.Keys.RControlKey) & 0x80) == 0x80)
				{
					KeyStates[KeyboardKeys.RControlKey] = state;
					right = true;
				}
				modifiers |= KeyboardKeys.Control;
				KeyStates[KeyboardKeys.Control] = state;
				KeyStates[KeyboardKeys.ControlKey] = state;				
			}
			if ((GetKeyState(Forms.Keys.Menu) & 0x80) == 0x80)
			{
				if ((GetKeyState(Forms.Keys.LMenu) & 0x80) == 0x80)
				{
					KeyStates[KeyboardKeys.LMenu] = state;
					left = true;
				}
				if ((GetKeyState(Forms.Keys.RMenu) & 0x80) == 0x80)
				{
					KeyStates[KeyboardKeys.RMenu] = state;
					right = true;
				}
				modifiers |= KeyboardKeys.Alt;
				KeyStates[KeyboardKeys.Menu] = state;
				KeyStates[KeyboardKeys.Alt] = state;
			}
			if ((GetKeyState(Forms.Keys.ShiftKey) & 0x80) == 0x80)
			{
				if ((GetKeyState(Forms.Keys.LShiftKey) & 0x80) == 0x80)
				{
					KeyStates[KeyboardKeys.LShiftKey] = state;
					left = true;
				}
				if ((GetKeyState(Forms.Keys.RShiftKey) & 0x80) == 0x80)
				{
					KeyStates[KeyboardKeys.RShiftKey] = state;
					right = true;
				}
				modifiers |= KeyboardKeys.Shift;
				KeyStates[KeyboardKeys.Shift] = state;
				KeyStates[KeyboardKeys.ShiftKey] = state;				
			}

			if (_mapper.KeyMapping.ContainsKey(keyEventArgs.KeyCode))
			{
				KeyStates[_mapper.KeyMapping[keyEventArgs.KeyCode]] = state;

				if (state == KeyState.Down)
					OnKeyDown(_mapper.KeyMapping[keyEventArgs.KeyCode], modifiers, keyEventArgs.KeyValue, left, right);
				else
					OnKeyUp(_mapper.KeyMapping[keyEventArgs.KeyCode], modifiers, keyEventArgs.KeyValue, left, right);
			}
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
			this.BoundWindow.KeyUp += new Forms.KeyEventHandler(BoundWindow_KeyUp);
			this.BoundWindow.KeyDown += new Forms.KeyEventHandler(BoundWindow_KeyDown);
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
			this.BoundWindow.KeyUp -= new Forms.KeyEventHandler(BoundWindow_KeyUp);
			this.BoundWindow.KeyDown -= new Forms.KeyEventHandler(BoundWindow_KeyDown);
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
		internal WinFormsKeyboard(GorgonWinFormsInputDeviceFactory owner, Forms.Control boundWindow)
			: base(owner, "Win Forms Input Keyboard", boundWindow)
		{
			Gorgon.Log.Print("Win Forms input keyboard interface created.", GorgonLoggingLevel.Verbose);
		}
		#endregion
	}
}
