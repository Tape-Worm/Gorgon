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
	/// The windows forms keyboard processor used to capture and translate standard win forms events.
	/// </summary>
	class WinformsInputProcessor
	{
		#region Variables.
		// A list of registered devices from the service.
		private readonly IReadOnlyList<IGorgonInputDevice> _devices;
		// Flag to indicate that the events are already registered.
		private bool _isRegistered;
		// The input device coordinator from the input service.
		private readonly GorgonInputDeviceDefaultCoordinator _coordinator;
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
		/// Handles the KeyUp event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			ProcessEvent(e, KeyState.Up);
		}

		/// <summary>
		/// Handles the KeyDown event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			ProcessEvent(e, KeyState.Down);
		}

		/// <summary>
		/// Handles the MouseWheel event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void Window_MouseWheel(object sender, MouseEventArgs e)
		{
			ProcessEvent(e, false);
		}

		/// <summary>
		/// Handles the MouseUp event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void Window_MouseUp(object sender, MouseEventArgs e)
		{
			ProcessEvent(e, true);
		}

		/// <summary>
		/// Handles the MouseDown event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void Window_MouseDown(object sender, MouseEventArgs e)
		{
			ProcessEvent(e, false);
		}

		/// <summary>
		/// Handles the MouseMove event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void Window_MouseMove(object sender, MouseEventArgs e)
		{
			ProcessEvent(e, false);
		}

		/// <summary>
		/// Function to retrieve the mouse button states from an event.
		/// </summary>
		/// <param name="button">The current button to evaluate.</param>
		/// <param name="isUp"><b>true</b> if the button was released, <b>false</b> if not.</param>
		/// <returns>The current button state for the mouse.</returns>
		private static MouseButtonState GetMouseButtonState(System.Windows.Forms.MouseButtons button, bool isUp)
		{
			MouseButtonState state = MouseButtonState.None;

			if (isUp)
			{
				switch (button)
				{
					case System.Windows.Forms.MouseButtons.Left:
						state |= MouseButtonState.ButtonLeftUp;
						break;
					case System.Windows.Forms.MouseButtons.Middle:
						state |= MouseButtonState.ButtonMiddleUp;
						break;
					case System.Windows.Forms.MouseButtons.Right:
						state |= MouseButtonState.ButtonRightUp;
						break;
					case System.Windows.Forms.MouseButtons.XButton1:
						state |= MouseButtonState.Button4Up;
						break;
					case System.Windows.Forms.MouseButtons.XButton2:
						state |= MouseButtonState.Button5Up;
						break;
				}

				return state;
			}

			switch (button)
			{
				case System.Windows.Forms.MouseButtons.Left:
					state |= MouseButtonState.ButtonLeftDown;
					break;
				case System.Windows.Forms.MouseButtons.Middle:
					state |= MouseButtonState.ButtonMiddleDown;
					break;
				case System.Windows.Forms.MouseButtons.Right:
					state |= MouseButtonState.ButtonRightDown;
					break;
				case System.Windows.Forms.MouseButtons.XButton1:
					state |= MouseButtonState.Button4Down;
					break;
				case System.Windows.Forms.MouseButtons.XButton2:
					state |= MouseButtonState.Button5Down;
					break;
			}

			return state;
		}

		/// <summary>
		/// Function to process windows forms mouse events, and return data Gorgon can parse.
		/// </summary>
		/// <param name="e">Event parameters.</param>
		/// <param name="buttonUp"><b>true</b> if a button was released, <b>false</b> if not.</param>
		private void ProcessEvent(MouseEventArgs e, bool buttonUp)
		{
			var data = new GorgonMouseData
			           {
				           MouseWheelDelta = (short)e.Delta,
				           ButtonState = GetMouseButtonState(e.Button, buttonUp),
						   Position = e.Location
			           };

			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < _devices.Count; ++i)
			{
				var device = _devices[i] as IGorgonInputEventDrivenDevice<GorgonMouseData>;

				if ((device == null) || (!device.IsAcquired) || (device.Window != Window))
				{
					continue;
				}

				_coordinator.DispatchEvent(device, ref data);
			}
		}

		/// <summary>
		/// Function to process windows forms keyboard events, and return data Gorgon can parse.
		/// </summary>
		/// <param name="e">Event parameters.</param>
		/// <param name="state">The state of the key, up or down.</param>
		/// <returns>The parsed data for the keyboard event.</returns>
		private void ProcessEvent(KeyEventArgs e, KeyState state)
		{
			KeyboardDataFlags flags = state == KeyState.Down ? KeyboardDataFlags.KeyDown : KeyboardDataFlags.KeyUp;
			
			// Check for modifiers.
			switch (e.KeyCode)
			{
				case Keys.ControlKey:
					if (UserApi.CheckKeyDown(Keys.LControlKey))
					{
						flags |= KeyboardDataFlags.LeftKey;
					}

					if (UserApi.CheckKeyDown(Keys.RControlKey))
					{
						flags |= KeyboardDataFlags.RightKey;
					}
					break;
				case Keys.Menu:
					if (UserApi.CheckKeyDown(Keys.LMenu))
					{
						flags |= KeyboardDataFlags.LeftKey;
					}

					if (UserApi.CheckKeyDown(Keys.RMenu))
					{
						flags |= KeyboardDataFlags.RightKey;
					}
					break;
				case Keys.ShiftKey:
					if (UserApi.CheckKeyDown(Keys.LShiftKey))
					{
						flags |= KeyboardDataFlags.LeftKey;
					}

					if (UserApi.CheckKeyDown(Keys.RShiftKey))
					{
						flags |= KeyboardDataFlags.RightKey;
					}
					break;
			}

			var data = new GorgonKeyboardData
			           {
				           ScanCode = UserApi.GetScancode(e.KeyCode),
				           Key = e.KeyCode,
				           Flags = flags
			           };

			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < _devices.Count; ++i)
			{
				var device = _devices[i] as IGorgonInputEventDrivenDevice<GorgonKeyboardData>;

				if ((device == null) || (!device.IsAcquired) || (device.Window != Window))
				{
					continue;
				}

				_coordinator.DispatchEvent(device, ref data);
			}
		}

		/// <summary>
		/// Function to register windows forms events to the appropriate window.
		/// </summary>
		public void RegisterEvents()
		{
			if (_isRegistered)
			{
				return;
			}

			Window.KeyDown += Window_KeyDown;
			Window.KeyUp += Window_KeyUp;
			Window.MouseMove += Window_MouseMove;
			Window.MouseDown += Window_MouseDown;
			Window.MouseUp += Window_MouseUp;
			Window.MouseWheel += Window_MouseWheel;

			_isRegistered = true;
		}


		/// <summary>
		/// Function to unregister windows forms event from the specified window.
		/// </summary>
		public void UnregisterEvents()
		{
			if (!_isRegistered)
			{
				return;
			}

			Window.KeyDown -= Window_KeyDown;
			Window.KeyUp -= Window_KeyUp;
			Window.MouseMove -= Window_MouseMove;
			Window.MouseDown -= Window_MouseDown;
			Window.MouseUp -= Window_MouseUp;
			Window.MouseWheel -= Window_MouseWheel;

			_isRegistered = false;
		}
		#endregion

		#region Constructor/Finalizer
		/// <summary>
		/// Initializes a new instance of the <see cref="WinformsInputProcessor" /> class.
		/// </summary>
		/// <param name="window">The window that the processor is bound with.</param>
		/// <param name="coordinator">The input device event coordinator.</param>
		/// <param name="devices">The list of registered devices in the service.</param>
		public WinformsInputProcessor(Control window, GorgonInputDeviceDefaultCoordinator coordinator, IReadOnlyList<IGorgonInputDevice> devices)
		{
			Window = window;
			_coordinator = coordinator;
			_devices = devices;
		}
		#endregion
	}
}
