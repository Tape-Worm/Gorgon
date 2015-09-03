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
// Created: Monday, August 24, 2015 8:17:23 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Diagnostics;
using Gorgon.Input.Raw.Properties;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// The raw input implementation of the <see cref="IGorgonInputDeviceRegistrar"/> interface.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Objects will be disposed when the last device is unregistered. This is the responsibility of the users, just like calling Dispose would be.")]
	class RawInputDeviceRegistrar
		: IGorgonInputDeviceRegistrar
	{
		#region Variables.
		// The logger used for debugging.
		private readonly IGorgonLog _log;
		// Hook into the window message procedure.
		private RawInputMessageHandler _messageHook;
		// The raw input processor for device data.
		private readonly Dictionary<Control, RawInputProcessor> _rawInputProcessors;
		// A list of registered devices.
		private readonly Dictionary<Guid, IGorgonInputDevice> _registeredDevices = new Dictionary<Guid, IGorgonInputDevice>();
		// Handles for registered devices.
		private readonly List<Tuple<Guid, IntPtr>> _deviceHandles = new List<Tuple<Guid, IntPtr>>();
		// The device data coordinator.
		private readonly RawInputDeviceCoordinator _coordinator;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the devices registered with the system.
		/// </summary>
		public IReadOnlyDictionary<Guid, IGorgonInputDevice> RegisteredDevices => _registeredDevices;
		#endregion

		#region Methods.
		/// <inheritdoc/>
		public bool RegisterDevice(IGorgonInputDevice device, IGorgonInputDeviceInfo2 info, Control window, bool exclusive = false)
		{
			IntPtr deviceHandle;

			// Register the exclusive devices with the service so it knows how to handle messages.
			switch (info.InputDeviceType)
			{
				case InputDeviceType.Keyboard:
					// Keep track of the keyboard we're registering so we can forward data to it.
					deviceHandle = ((RawInputKeyboardInfo)info).Handle;

					// Find any other registered keyboards.
					var keyboards = from keyboardDevice in _registeredDevices
					                let keyboard = keyboardDevice.Value as GorgonKeyboard2
					                where keyboard != null
					                select keyboard;

					// ReSharper disable PossibleMultipleEnumeration
					if (((exclusive) && (keyboards.Any())) || (keyboards.Any(item => item.IsExclusive)))
					{
						throw new ArgumentException(Resources.GORINP_RAW_ERR_CANNOT_BIND_EXCLUSIVE_KEYBOARD, nameof(exclusive));
					}

					break;
				case InputDeviceType.Mouse:
					// Keep track of the keyboard we're registering so we can forward data to it.
					deviceHandle = ((RawInputMouseInfo)info).Handle;

					// Find any other registered mice.
					var mice = from mouseDevice in _registeredDevices
					           let mouse = mouseDevice.Value as GorgonMouse
					           where mouse != null
					           select mouse;


					if (((exclusive) && (mice.Any())) || (mice.Any(item => item.IsExclusive)))
					{
						throw new ArgumentException(Resources.GORINP_RAW_ERR_CANNOT_BIND_EXCLUSIVE_MOUSE, nameof(exclusive));
					}
					// ReSharper enable PossibleMultipleEnumeration
					break;
				case InputDeviceType.Joystick:
					// Keep track of the keyboard we're registering so we can forward data to it.
					deviceHandle = ((RawInputMouseInfo)info).Handle;

					// We don't do exclusive on joysticks.  No need for it.
					exclusive = false;
					break;
				default:
					return false;
			}

			if (!_rawInputProcessors.ContainsKey(window))
			{
				_rawInputProcessors[window] = new RawInputProcessor(_coordinator);
			}

			if (!_registeredDevices.ContainsKey(device.UUID))
			{
				_registeredDevices.Add(device.UUID, device);
				_deviceHandles.Add(new Tuple<Guid, IntPtr>(device.UUID, deviceHandle));
			}

			_log.Print("{0} '{1}' on HID path {2} is registered.", LoggingLevel.Verbose, info.ClassName, info.Description, info.HumanInterfaceDevicePath);

			// We should only need to do this once for the window.
			if (_messageHook != null)
			{
				return RawInputApi.SetExclusiveState(info.InputDeviceType, exclusive);
			}

			// Bind the main window. This is done so the child windows can all receive raw input, instead of registering it 
			// to a single window handle.
			Form parentForm = window.FindForm() ?? window as Form;

			if (parentForm == null)
			{
				throw new ArgumentException(Resources.GORINP_RAW_ERR_CANNOT_BIND_INPUT_DEVICES, nameof(window));
			}

			// Initialize our message hook if we haven't done so by now.
			_messageHook = new RawInputMessageHandler(parentForm.Handle, _deviceHandles, _rawInputProcessors, _registeredDevices);
			_messageHook.HookWindow();

			_log.Print("Raw input window hook installed.", LoggingLevel.Verbose);

			return RawInputApi.SetExclusiveState(info.InputDeviceType, exclusive);
		}

		/// <inheritdoc/>
		public void UnregisterDevice(IGorgonInputDevice device)
		{
			if (_rawInputProcessors.Count(item => item.Key == device.Window) == 1)
			{
				_rawInputProcessors.Remove(device.Window);
			}

			// Unregister the device.
			if (_registeredDevices.ContainsKey(device.UUID))
			{
				_registeredDevices.Remove(device.UUID);

				var deviceHandle = _deviceHandles.Find(item => item.Item1 == device.UUID);
				_deviceHandles.Remove(deviceHandle);
			}

			if ((_registeredDevices.Count > 0) || (_messageHook == null))
			{
				return;
			}

			_messageHook.Dispose();
			_messageHook = null;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawInputDeviceRegistrar" /> class.
		/// </summary>
		/// <param name="log">The logger used for debugging.</param>
		/// <param name="coordinator">The device data coordinator.</param>
		/// <param name="inputProcessors">The input processors used to translate raw input data to Gorgon input data.</param>
		public RawInputDeviceRegistrar(IGorgonLog log, RawInputDeviceCoordinator coordinator, Dictionary<Control, RawInputProcessor> inputProcessors)
		{
			_coordinator = coordinator;
			_log = log;
			_rawInputProcessors = inputProcessors;
		}
		#endregion
	}
}
