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
// Created: Thursday, September 3, 2015 8:26:20 PM
// 
#endregion

using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using Gorgon.Diagnostics;

namespace Gorgon.Input.WinForms
{
	/// <summary>
	/// The windows forms input implementation of the <see cref="IGorgonInputDeviceRegistrar"/> interface.
	/// </summary>
	class WinFormsInputDeviceRegistrar
		: IGorgonInputDeviceRegistrar
	{
		#region Variables.
		// The logger used for debugging.
		private readonly IGorgonLog _log;
		// A list of devices registered with the service.
		private readonly List<IGorgonInputDevice> _registeredDevices = new List<IGorgonInputDevice>();
		// The raw input processor for device data.
		private readonly Dictionary<Control, WinformsInputProcessor> _winFormsInputProcessors;
		// The device event coordinator.
		private readonly GorgonInputDeviceDefaultCoordinator _coordinator;
		#endregion

		#region Methods.
		/// <summary>
		/// Function called when a device is acquired.
		/// </summary>
		/// <param name="sender">The object that sent the event.</param>
		/// <param name="e">Event parameters.</param>
		private void InputDevice_AcquireDevice(object sender, EventArgs e)
		{
			IGorgonInputDevice device = sender as IGorgonInputDevice;

			if (device == null)
			{
				return;
			}

			WinformsInputProcessor processor;

			if (!_winFormsInputProcessors.TryGetValue(device.Window, out processor))
			{
				return;
			}

			processor.RegisterEvents();
		}

		/// <summary>
		/// Function called when a device is unacquired.
		/// </summary>
		/// <param name="sender">The object that sent the event.</param>
		/// <param name="e">Event parameters.</param>
		private void InputDevice_UnacquireDevice(object sender, EventArgs e)
		{
			IGorgonInputDevice device = sender as IGorgonInputDevice;

			if (device == null)
			{
				return;
			}

			WinformsInputProcessor processor;

			if (!_winFormsInputProcessors.TryGetValue(device.Window, out processor))
			{
				return;
			}

			processor.UnregisterEvents();
		}

		/// <inheritdoc/>
		public bool RegisterDevice(IGorgonInputDevice device, IGorgonInputDeviceInfo2 info, Control window, bool exclusive = false)
		{
			if (exclusive)
			{
				_log.Print("WARNING! Devices in the windows forms input plug in cannot be set to exclusive mode.", LoggingLevel.Verbose);
			}

			if (!_winFormsInputProcessors.ContainsKey(window))
			{
				_winFormsInputProcessors[window] = new WinformsInputProcessor(window, _coordinator,_registeredDevices);
			}

			if (_registeredDevices.Contains(device))
			{
				return false;
			}

			_registeredDevices.Add(device);

			device.InputDeviceAcquired += InputDevice_AcquireDevice;
			device.InputDeviceUnacquired += InputDevice_UnacquireDevice;

			return false;
		}

		/// <inheritdoc/>
		public void UnregisterDevice(IGorgonInputDevice device)
		{
			if (_winFormsInputProcessors.Count(item => item.Value.Window == device.Window) == 1)
			{
				_winFormsInputProcessors.Remove(device.Window);
			}

			if (!_registeredDevices.Contains(device))
			{
				return;
			}

			_registeredDevices.Remove(device);

			device.InputDeviceAcquired -= InputDevice_AcquireDevice;
			device.InputDeviceUnacquired -= InputDevice_UnacquireDevice;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="WinFormsInputDeviceRegistrar"/> class.
		/// </summary>
		/// <param name="log">The logger used for debugging.</param>
		/// <param name="coordinator">The device event coordinator.</param>
		/// <param name="inputProcessors">The processors for the input devices.</param>
		public WinFormsInputDeviceRegistrar(IGorgonLog log, GorgonInputDeviceDefaultCoordinator coordinator, Dictionary<Control, WinformsInputProcessor> inputProcessors)
		{
			_coordinator = coordinator;
			_log = log;
			_winFormsInputProcessors = inputProcessors;
		}
		#endregion
	}
}
