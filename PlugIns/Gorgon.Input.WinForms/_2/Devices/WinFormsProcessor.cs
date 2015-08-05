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
// Created: Monday, July 20, 2015 10:33:04 PM
// 
#endregion

using System.Collections.Generic;
using System.Windows.Forms;
using Gorgon.Native;

namespace Gorgon.Input.WinForms
{
	/// <summary>
	/// The windows forms keyboard hook used to capture and translate standard windows keyboard messages.
	/// </summary>
	class WinFormsKeyboardHook
	{
		#region Variables.
		// The input service that owns this processor.
		private readonly GorgonInputDeviceEventRouting _router;
		// A list of registered devices from the service.
		private readonly IReadOnlyList<IGorgonInputDevice> _devices;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the window that this processor is bound with.
		/// </summary>
		public Control Window
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to route event data to the appropriate keyboard device object.
		/// </summary>
		/// <param name="data">The data to pass to the device.</param>
		private void RouteKeyboardEvent(ref GorgonKeyboardData data)
		{
			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < _devices.Count; ++i)
			{
				IGorgonInputDevice device = _devices[i];

				if ((!device.IsAcquired) || (device.Window != Window) || (!(device is IGorgonKeyboard))) 
				{
					continue;
				}

				_router.RouteToDevice(_devices[i], ref data);
			}
		}

		/// <summary>
		/// Handles the KeyUp event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			GorgonKeyboardData data = ProcessEvent(e, KeyState.Up);
			RouteKeyboardEvent(ref data);
		}

		/// <summary>
		/// Handles the KeyDown event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			GorgonKeyboardData data = ProcessEvent(e, KeyState.Down);
			RouteKeyboardEvent(ref data);
		}

		/// <summary>
		/// Function to process windows forms keyboard events, and return data Gorgon can parse.
		/// </summary>
		/// <param name="e">Event parameters.</param>
		/// <param name="state">The state of the key, up or down.</param>
		/// <returns>The parsed data for the keyboard event.</returns>
		private static GorgonKeyboardData ProcessEvent(KeyEventArgs e, KeyState state)
		{
			KeyboardDataFlags flags = state == KeyState.Down ? KeyboardDataFlags.KeyDown : KeyboardDataFlags.KeyUp;
			
			// Check for modifiers.
			switch (e.KeyCode)
			{
				case Keys.ControlKey:
					if ((Win32API.GetKeyState(Keys.LControlKey) & 0x80) == 0x80)
					{
						flags |= KeyboardDataFlags.LeftKey;
					}

					if ((Win32API.GetKeyState(Keys.RControlKey) & 0x80) == 0x80)
					{
						flags |= KeyboardDataFlags.RightKey;
					}
					break;
				case Keys.Menu:
					if ((Win32API.GetKeyState(Keys.LMenu) & 0x80) == 0x80)
					{
						flags |= KeyboardDataFlags.LeftKey;
					}

					if ((Win32API.GetKeyState(Keys.RMenu) & 0x80) == 0x80)
					{
						flags |= KeyboardDataFlags.RightKey;
					}
					break;
				case Keys.ShiftKey:
					if ((Win32API.GetKeyState(Keys.LShiftKey) & 0x80) == 0x80)
					{
						flags |= KeyboardDataFlags.LeftKey;
					}

					if ((Win32API.GetKeyState(Keys.RShiftKey) & 0x80) == 0x80)
					{
						flags |= KeyboardDataFlags.RightKey;
					}
					break;
			}

			return new GorgonKeyboardData
			       {
				       ScanCode = Win32API.MapVirtualKey(e.KeyCode, 0),
				       Key = e.KeyCode,
				       Flags = flags
			       };
		}

		/// <summary>
		/// Function to register windows forms events to the appropriate window.
		/// </summary>
		public void RegisterEvents()
		{
			Window.KeyDown += Window_KeyDown;
			Window.KeyUp += Window_KeyUp;
		}

		/// <summary>
		/// Function to unregister windows forms event from the specified window.
		/// </summary>
		public void UnregisterEvents()
		{
			Window.KeyDown -= Window_KeyDown;
			Window.KeyUp -= Window_KeyUp;
		}
		#endregion

		#region Constructor/Finalizer
		/// <summary>
		/// Initializes a new instance of the <see cref="WinFormsKeyboardHook" /> class.
		/// </summary>
		/// <param name="window">The window that the processor is bound with.</param>
		/// <param name="router">The router used to send the data to the device.</param>
		/// <param name="devices">The list of registered devices in the service.</param>
		public WinFormsKeyboardHook(Control window, GorgonInputDeviceEventRouting router, IReadOnlyList<IGorgonInputDevice> devices)
		{
			Window = window;
			_router = router;
			_devices = devices;
		}
		#endregion
	}
}
