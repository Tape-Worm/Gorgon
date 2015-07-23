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
		// A list of devices registered with the service.
		private readonly List<IGorgonInputDevice> _registeredDevices = new List<IGorgonInputDevice>();
		// The raw input processor for device data.
		private readonly Dictionary<Control, WinFormsKeyboardHook> _winFormsProcessors = new Dictionary<Control, WinFormsKeyboardHook>();
		#endregion

		#region Methods.
		/// <inheritdoc/>
		protected override void AcquireDevice(IGorgonInputDevice device, bool acquisitionState)
		{
			WinFormsKeyboardHook processor;

			if (!_winFormsProcessors.TryGetValue(device.Window, out processor))
			{
				return;
			}

			if (acquisitionState)
			{
				processor.RegisterEvents();
			}
			else
			{
				processor.UnregisterEvents();
			}
		}

		/// <inheritdoc/>
		protected override void RegisterDevice(IGorgonInputDevice device, IGorgonInputDeviceInfo2 deviceInfo, Form parentForm, Control window, ref bool exclusive)
		{
			if (exclusive)
			{
				exclusive = false;
			}

			if (!_winFormsProcessors.ContainsKey(window))
			{
				_winFormsProcessors.Add(window, new WinFormsKeyboardHook(window, EventRouter, _registeredDevices));
			}

			if (!_registeredDevices.Contains(device))
			{
				_registeredDevices.Add(device);
			}
		}

		/// <inheritdoc/>
		protected override void UnregisterDevice(IGorgonInputDevice device)
		{
			if (_winFormsProcessors.Count(item => item.Value.Window == device.Window) == 1)
			{
				_winFormsProcessors.Remove(device.Window);
			}

			if (_registeredDevices.Contains(device))
			{
				_registeredDevices.Remove(device);
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
		
		/// <summary>
		/// Function to retrieve a read only list of the registered devices.
		/// </summary>
		/// <returns>A read only list of the registered devices.</returns>
		public IReadOnlyList<IGorgonInputDevice> GetInputDevice()
		{
			return _registeredDevices;
		}
		#endregion
	}
}
