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
		// TODO: If we create multiple keyboard objects and bind them to different windows, this will probably fail.
		// TODO: Make a dictionary, keyed by the window we're hooking, and use that to determine where we're coming from.
		private readonly WinFormsKeyboardHook _keyboardHook = new WinFormsKeyboardHook();

		// TODO: Because the hook above needs to be segregated to handle multiple instances of keyboard objects, we'll need
		// TODO: keep track of devices here somehow. Even though Instance 1 and Instance 2 use the same device, they're still 
		// TODO: different objects and must be routed properly.
		#endregion

		#region Methods.
		/// <inheritdoc/>
		protected override void AcquireDevice(IGorgonInputDevice device, bool acquisitionState)
		{
			if (acquisitionState)
			{
				_keyboardHook.RegisterEvents(device.Window);
			}
			else
			{
				_keyboardHook.UnregisterEvents(device.Window);
			}
		}

		/// <inheritdoc/>
		protected override void RegisterDevice(IGorgonInputDevice device, IGorgonInputDeviceInfo2 deviceInfo, Form parentForm, Control window, bool exclusive)
		{
			// Register the exclusive devices with the service so it knows how to handle messages.
			switch (deviceInfo.InputDeviceType)
			{
				case InputDeviceType.Keyboard:
					_keyboardHook.KeyboardEvent = e =>
					                              {
						                              SendKeyboardData(null, ref e);
					                              };
					break;
				case InputDeviceType.Mouse:
					break;
			}
		}

		/// <inheritdoc/>
		protected override void UnregisterDevice(IGorgonInputDevice device, IGorgonInputDeviceInfo2 deviceInfo)
		{
			switch (deviceInfo.InputDeviceType)
			{
				case InputDeviceType.Keyboard:
					_keyboardHook.KeyboardEvent = null;
					break;
				case InputDeviceType.Mouse:
					break;
			}
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
