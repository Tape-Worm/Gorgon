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
// Created: Friday, June 24, 2011 10:05:11 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Diagnostics;
using Gorgon.Input.Raw.Properties;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// Raw input/Windows Multimedia joystick service.
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Objects that depend on this class need the message proc it installs. Disposing it before those objects are dead will lead to problems later. We keep the proc alive until all devices are unregistered.")]
	class GorgonRawInputService2
		: GorgonInputService2
	{
/*		
		/// <inheritdoc/>
		protected override IGorgonNamedObjectReadOnlyDictionary<IGorgonJoystickInfo> OnEnumerateJoysticks()
		{
		    var capabilities = new JOYCAPS();	    // Joystick capabilities.
		    int nameCount = 0;						// Name counter.

			var result = new GorgonNamedObjectDictionary<IGorgonJoystickInfo>(false);
			int deviceCount = Win32API.joyGetNumDevs();
			int capsSize = Marshal.SizeOf(typeof(JOYCAPS));
			
			for (int i = 0; i < deviceCount; i++)
			{
				// Error code.
			    int error = Win32API.joyGetDevCaps(i, ref capabilities, capsSize);							

			    // If the joystick has no registry key, then skip it.
			    if ((string.IsNullOrEmpty(capabilities.RegistryKey)) || (string.IsNullOrEmpty(capabilities.Name)))
			    {
			        continue;
			    }

			    // Check for error, stop enumeration.
			    if (error > 0)
			    {
			        throw new GorgonException(GorgonResult.DriverError,
			                                  string.Format(Resources.GORINP_RAW_CANNOT_GET_JOYSTICK_CAPS, i, error.FormatHex()));
			    }

			    // Get the name.
			    string name = GetJoystickName(capabilities, i);				// Name of the joystick.

			    if (string.IsNullOrWhiteSpace(name))
			    {
			        continue;
			    }

			    string keyName = name;

				while (result.Contains(keyName))
				{ 
			        nameCount++;
			        keyName = name + " #" + nameCount;
			    }

				result.Add(new MultimediaJoystickInfo(Guid.NewGuid(), name, i));
			}

			return result;
		}
*/
		#region Variables.
		// The logging interface.
		private readonly IGorgonLog _log;
		// Hook into the window message procedure.
		private RawInputNative _messageHook;
		// The raw input processor for device data.
		private readonly Dictionary<Control, RawInputProcessor> _rawInputProcessors = new Dictionary<Control, RawInputProcessor>();
		// A list of registered devices.
		private readonly Dictionary<Guid, IGorgonInputDevice> _registeredDevices = new Dictionary<Guid, IGorgonInputDevice>();
		// Handles for registered devices.
		private readonly List<Tuple<Guid, IntPtr>> _deviceHandles = new List<Tuple<Guid, IntPtr>>();
		// The interface used to enumerate raw input devices.
		private readonly RawInputEnumerator _deviceEnumerator = new RawInputEnumerator();
		// The device information interface.
		private readonly RawInputDeviceInfo _deviceInfo;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the device name.
		/// </summary>
		/// <param name="device">Raw input device to gather information from.</param>
		/// <returns>A device name structure.</returns>
		private T GetDeviceInfo<T>(ref RAWINPUTDEVICELIST device)
			where T : class, IGorgonInputDeviceInfo2
		{
			// If we're running under a terminal server session, then throw an exception.
			// Raw input does not run very well under RDP (especially the mouse), so we'll disable it.
			if (SystemInformation.TerminalServerSession)
			{
				return null;
			}

			string deviceName = _deviceInfo.GetDeviceName(ref device);

			if (string.IsNullOrWhiteSpace(deviceName))
			{
				return null;
			}

			RID_DEVICE_INFO deviceInfo = _deviceInfo.GetDeviceInfo(ref device);
			string className = _deviceInfo.GetDeviceClass(deviceName);
			string deviceDescription = _deviceInfo.GetDeviceDescription(deviceName);
			
			switch (deviceInfo.dwType)
			{
				case RawInputType.Keyboard:
					var keyboardInfo = new RawInputKeyboardInfo2(deviceDescription, className, deviceName, device.Device);

					keyboardInfo.AssignRawInputDeviceInfo(ref deviceInfo.keyboard);

                    return keyboardInfo as T;
				case RawInputType.Mouse:
					var mouseInfo = new RawInputMouseInfo2(deviceDescription, className, deviceName, device.Device);

					mouseInfo.AssignRawInputDeviceInfo(ref deviceInfo.mouse);

					return mouseInfo as T;
				default:
					return null;
			}
		}

		/// <inheritdoc/>
		protected override IReadOnlyList<IGorgonKeyboardInfo2> OnEnumerateKeyboards()
		{
			RAWINPUTDEVICELIST[] devices = _deviceEnumerator.Enumerate(InputDeviceType.Keyboard);

			// Put the system keyboard first.
			var result = new List<IGorgonKeyboardInfo2>
			             {
				             new RawInputKeyboardInfo2(Resources.GORINP_RAW_SYSTEM_KEYBOARD, "Keyboard", "SystemKeyboard", IntPtr.Zero)
			             };

			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < devices.Length; ++i)
			{
				RawInputKeyboardInfo2 info = GetDeviceInfo<RawInputKeyboardInfo2>(ref devices[i]);

				if (info == null)
				{
					_log.Print("WARNING: Could not retrieve the class and device name.  Skipping this device.", LoggingLevel.Verbose);
					continue;
				}

				_log.Print("Found keyboard: '{0}' on HID path {1}, class {2}.", LoggingLevel.Verbose, info.Description, info.HumanInterfaceDevicePath, info.ClassName);

				result.Add(info);
			}

			return result;
		}

		/// <inheritdoc/>
		protected override IReadOnlyList<IGorgonMouseInfo2> OnEnumerateMice()
		{
			RAWINPUTDEVICELIST[] devices = _deviceEnumerator.Enumerate(InputDeviceType.Mouse);
			// Put the system mouse first.
			var result = new List<IGorgonMouseInfo2>
			             {
				             new RawInputMouseInfo2(Resources.GORINP_RAW_SYSTEM_MOUSE, "Mouse", "SystemMouse", IntPtr.Zero)
			             };

			for (int i = 0; i < devices.Length; ++i)
			{
				RawInputMouseInfo2 info = GetDeviceInfo<RawInputMouseInfo2>(ref devices[i]);

				if (info == null)
				{
					_log.Print("WARNING: Could not retrieve the class and device name.  Skipping this device.", LoggingLevel.Verbose);
					continue;
				}

				_log.Print("Found mouse: '{0}' on HID path {1}, class {2}.", LoggingLevel.Verbose, info.Description, info.HumanInterfaceDevicePath, info.ClassName);

				result.Add(info);
			}

			return result;
		}

		/// <inheritdoc/>
		protected override IReadOnlyList<IGorgonJoystickInfo2> OnEnumerateJoysticks()
		{
			return new IGorgonJoystickInfo2[0];
		}

		/// <inheritdoc/>
		protected override void AcquireDevice(IGorgonInputDevice device, bool acquisitionState)
		{
			
		}

		/// <inheritdoc/>
		protected override void RegisterDevice(IGorgonInputDevice device, IGorgonInputDeviceInfo2 deviceInfo, Form parentForm, Control window, ref bool exclusive)
		{
			IntPtr deviceHandle;
		
			// Register the exclusive devices with the service so it knows how to handle messages.
			switch (deviceInfo.InputDeviceType)
			{
				case InputDeviceType.Keyboard:
					// Keep track of the keyboard we're registering so we can forward data to it.
					deviceHandle = ((RawInputKeyboardInfo2)deviceInfo).Handle;

					// Find any other registered keyboards.
					var keyboards = from keyboardDevice in _registeredDevices
					                let keyboard = keyboardDevice.Value as IGorgonKeyboard
					                where keyboard != null
					                select keyboard;

					// ReSharper disable PossibleMultipleEnumeration
					if (((exclusive) && (keyboards.Any())) || (keyboards.Any(item => item.IsExclusive)))
					{
						throw new ArgumentException(Resources.GORINP_RAW_ERR_CANNOT_BIND_EXCLUSIVE_KEYBOARD, nameof(exclusive));
					}
					// ReSharper enable PossibleMultipleEnumeration
					break;
				case InputDeviceType.Mouse:
					// Keep track of the keyboard we're registering so we can forward data to it.
					deviceHandle = ((RawInputMouseInfo2)deviceInfo).Handle;

					// Find any other registered keyboards.
					var mice = from mouseDevice in _registeredDevices
					           let mouse = mouseDevice.Value as IGorgonMouse
					           where mouse != null
					           select mouse;

					// ReSharper disable PossibleMultipleEnumeration
					if (((exclusive) && (mice.Any())) || (mice.Any(item => item.IsExclusive)))
					{
						throw new ArgumentException(Resources.GORINP_RAW_ERR_CANNOT_BIND_EXCLUSIVE_KEYBOARD, nameof(exclusive));
					}
					// ReSharper enable PossibleMultipleEnumeration
					break;
				default:
					return;
			}

			if (!_rawInputProcessors.ContainsKey(window))
			{
				_rawInputProcessors[window] = new RawInputProcessor(EventRouter);
			}

			if (!_registeredDevices.ContainsKey(device.UUID))
			{
				_registeredDevices.Add(device.UUID, device);
				_deviceHandles.Add(new Tuple<Guid, IntPtr>(device.UUID, deviceHandle));
			}

			// We should only need to do this once for the window.
			if (_messageHook == null)
			{
				// Initialize our message hook if we haven't done so by now.
				_messageHook = new RawInputNative(parentForm.Handle, _deviceHandles, _rawInputProcessors, _registeredDevices);
				_messageHook.HookWindow();
			}

			exclusive = _messageHook.SetExclusiveState(deviceInfo.InputDeviceType, exclusive);
		}

		/// <inheritdoc/>
		protected override void UnregisterDevice(IGorgonInputDevice device)
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
		/// Initializes a new instance of the <see cref="GorgonRawInputService2"/> class.
		/// </summary>
		/// <param name="log">The log interface used for debug logging.</param>
		public GorgonRawInputService2(IGorgonLog log)
		{
			_log = log;
			_deviceInfo = new RawInputDeviceInfo(_log);
		}
		#endregion
	}
}
