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

using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Input.WinForms.Properties;

namespace Gorgon.Input.WinForms
{
	/// <summary>
	/// Object representing keyboard data.
	/// </summary>
	internal class WinFormsKeyboard
		: GorgonKeyboard
	{
		#region Variables.
		// Key mappings.
		private readonly KeyMapper _mapper = new KeyMapper();		
		#endregion

		#region Methods.
		/// <summary>
		/// Function to get the state of a key.
		/// </summary>
		/// <param name="nVirtKey">Virtual key code to retrieve.</param>
		// <returns>A bit mask containing the state of the virtual key.</returns>
		[DllImport("User32.dll"), SuppressUnmanagedCodeSecurity]
		private static extern short GetKeyState(Keys nVirtKey);

		/// <summary>
		/// Function to retrieve the scan code for a virtual key.
		/// </summary>
		/// <param name="uCode">Virtual key code</param>
		/// <param name="uMapType">Mapping type.</param>
		/// <returns>The scan code.</returns>
		[DllImport("user32.dll", CharSet = CharSet.Auto), SuppressUnmanagedCodeSecurity]
		private static extern int MapVirtualKey(KeyboardKeys uCode, int uMapType);

		/// <summary>
		/// Function to process the keyboard state.
		/// </summary>
		/// <param name="keyEventArgs">Event parameters from the keyboard events.</param>
		/// <param name="state">State of the key, up or down.</param>
		private void ProcessKeys(KeyEventArgs keyEventArgs, KeyState state)
		{
		    if ((BoundControl == null) || (BoundControl.Disposing))
		    {
		        return;
		    }

		    KeyboardKeys keyCode;
            if (!_mapper.KeyMapping.TryGetValue(keyEventArgs.KeyCode, out keyCode))
		    {
		        return;
		    }

		    // Check for modifiers.
			switch(keyCode)
			{
				case KeyboardKeys.ControlKey:
			        if ((GetKeyState(Keys.LControlKey) & 0x80) == 0x80)
			        {
			            keyCode = KeyboardKeys.LControlKey;
			        }

			        if ((GetKeyState(Keys.RControlKey) & 0x80) == 0x80)
			        {
			            keyCode = KeyboardKeys.RControlKey;
			        }

			        KeyStates[KeyboardKeys.ControlKey] = state;
					break;
				case KeyboardKeys.Menu:
			        if ((GetKeyState(Keys.LMenu) & 0x80) == 0x80)
			        {
			            keyCode = KeyboardKeys.LMenu;
			        }

			        if ((GetKeyState(Keys.RMenu) & 0x80) == 0x80)
			        {
			            keyCode = KeyboardKeys.RMenu;
			        }

			        KeyStates[KeyboardKeys.Menu] = state;
					break;
				case KeyboardKeys.ShiftKey:
			        if ((GetKeyState(Keys.LShiftKey) & 0x80) == 0x80)
			        {
			            keyCode = KeyboardKeys.LShiftKey;
			        }

			        if ((GetKeyState(Keys.RShiftKey) & 0x80) == 0x80)
			        {
			            keyCode = KeyboardKeys.RShiftKey;
			        }

			        KeyStates[KeyboardKeys.ShiftKey] = state;
					break;
			}

			KeyStates[keyCode] = state;

		    if (state == KeyState.Down)
		    {
		        OnKeyDown(keyCode, MapVirtualKey(keyCode, 0));
		    }
		    else
		    {
		        OnKeyUp(keyCode, MapVirtualKey(keyCode, 0));
		    }
		}

		/// <summary>
		/// Handles the KeyDown event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_KeyDown(object sender, KeyEventArgs e)
		{
			ProcessKeys(e, KeyState.Down);
		}

		/// <summary>
		/// Handles the KeyUp event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_KeyUp(object sender, KeyEventArgs e)
		{
			ProcessKeys(e, KeyState.Up);
		}

		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected override void BindDevice()
		{
			UnbindDevice();

			// If the bound control is a panel, then we'll need to bind to the form since it 
			// doesn't support key down/key up events natively.
			if (BoundControl is Panel)
			{
				BoundTopLevelForm.KeyDown += BoundWindow_KeyDown;
				BoundTopLevelForm.KeyUp += BoundWindow_KeyUp;
			}
			else
			{
				BoundControl.KeyUp += BoundWindow_KeyUp;
				BoundControl.KeyDown += BoundWindow_KeyDown;
			}
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
			if (BoundControl is Panel)
			{
				BoundTopLevelForm.KeyDown -= BoundWindow_KeyDown;
				BoundTopLevelForm.KeyUp -= BoundWindow_KeyUp;
			}
			else
			{
				BoundControl.KeyUp -= BoundWindow_KeyUp;
				BoundControl.KeyDown -= BoundWindow_KeyDown;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="WinFormsKeyboard"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		internal WinFormsKeyboard(GorgonInputService owner)
			: base(owner, Resources.GORINP_WIN_KEYBOARD_DESC)
		{
			GorgonApplication.Log.Print("Win Forms input keyboard interface created.", LoggingLevel.Verbose);
		}
		#endregion
	}
}
