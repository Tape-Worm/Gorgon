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
// Created: Monday, July 20, 2015 10:22:43 PM
// 
#endregion

using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Gorgon.Input.WinForms
{
	/// <summary>
	/// Windows forms input service for keyboard and mouse.
	/// </summary>
	class GorgonWinFormsInputService2
		: GorgonInputService2
	{
		#region Variables.
		// The hook used to translate windows forms keyboard messages.
		private readonly Dictionary<Control, WinFormsKeyboardHook> _keyboardHooks = new Dictionary<Control, WinFormsKeyboardHook>();

		// A list of devices registered with the service.
		private readonly Dictionary<Guid, IGorgonInputDevice> _registeredDevices = new Dictionary<Guid, IGorgonInputDevice>();
		#endregion

		#region Methods.
		/// <summary>
		/// Function to route keyboard event messages to the proper object.
		/// </summary>
		/// <param name="hookWindow">The window broadcasting the event.</param>
		/// <param name="data">The keyboard data to send.</param>
		private void RouteKeyboardEvent(Control hookWindow, GorgonKeyboardData data)
		{
			IGorgonInputDevice device =
				_registeredDevices.First(item => item.Value.IsAcquired && item.Value.Window == hookWindow && item.Value is IGorgonKeyboard).Value;

			if (device == null)
			{
				return;
			}

			// Broadcast the message to all keyboard device objects hooked into the window being notified.
			RouteKeyboardData(device.UUID, ref data);
		}

		/// <inheritdoc/>
		protected override void AcquireDevice(IGorgonInputDevice device, bool acquisitionState)
		{
			WinFormsKeyboardHook hook;

			if (!_keyboardHooks.TryGetValue(device.Window, out hook))
			{
				return;
			}

			if (acquisitionState)
			{
				hook.RegisterEvents();
			}
			else
			{
				hook.UnregisterEvents();
			}
		}

		/// <inheritdoc/>
		protected override void RegisterDevice(IGorgonInputDevice device, IGorgonInputDeviceInfo2 deviceInfo, Form parentForm, Control window, bool exclusive)
		{
			// Register the exclusive devices with the service so it knows how to handle messages.
			switch (deviceInfo.InputDeviceType)
			{
				case InputDeviceType.Keyboard:

					if (!_keyboardHooks.ContainsKey(window))
					{
						var keyboardHook = _keyboardHooks[window] = new WinFormsKeyboardHook(window);

						keyboardHook.KeyboardEvent = RouteKeyboardEvent;
					}


					if (!_registeredDevices.ContainsKey(device.UUID))
					{
						_registeredDevices[device.UUID] = device;
					}
					break;
				case InputDeviceType.Mouse:
					break;
			}
		}

		/// <inheritdoc/>
		protected override void UnregisterDevice(IGorgonInputDevice device)
		{
			// TODO: Change to using message processors like in RawInput.
			//switch (deviceInfo.InputDeviceType)
			//{
//				case InputDeviceType.Keyboard:
					if (_keyboardHooks.Count(item => item.Key == device.Window) == 1)
					{
						_keyboardHooks[device.Window].KeyboardEvent = null;
					}

					_keyboardHooks.Remove(device.Window);

					if (_registeredDevices.ContainsKey(device.UUID))
					{
						_registeredDevices.Remove(device.UUID);
					}
//					break;
//				case InputDeviceType.Mouse:
//					break;
			//}
		}

		/// <inheritdoc/>
		protected override IReadOnlyList<IGorgonKeyboardInfo2> OnEnumerateKeyboards()
		{
			return new IGorgonKeyboardInfo2[]
			       {
				       new WinFormsKeyboardInfo2()
			       };
		}

		/// <inheritdoc/>
		protected override IReadOnlyList<IGorgonMouseInfo2> OnEnumerateMice()
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		protected override IReadOnlyList<IGorgonJoystickInfo2> OnEnumerateJoysticks()
		{
			return new IGorgonJoystickInfo2[0];
		}
		#endregion
	}
}
