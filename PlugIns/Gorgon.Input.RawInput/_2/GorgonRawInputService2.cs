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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Diagnostics;
using Gorgon.Input.Raw.Properties;
using Gorgon.Native;
using Microsoft.Win32;

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
		private RawInputMessageHooker _messageHook;

		// The raw input hook into keyboard data.
		private readonly Dictionary<Control, RawInputProcessor> _rawInputProcessors = new Dictionary<Control, RawInputProcessor>();

		// A list of registered devices.
		private readonly Dictionary<Guid, IGorgonInputDevice> _registeredDevices = new Dictionary<Guid, IGorgonInputDevice>();

		// Handles for registered devices.
		private readonly List<Tuple<Guid, IntPtr>> _deviceHandles = new List<Tuple<Guid, IntPtr>>();

		// List of enumerated devices.
		private readonly Lazy<IEnumerable<RAWINPUTDEVICELIST>> _enumeratedDevices;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the class name for the device.
		/// </summary>
		/// <param name="registryPath">Path to the class information in the registry.</param>
		/// <returns>The class name for the device.</returns>
		private string GetClassName(string registryPath)
		{
			if (string.IsNullOrWhiteSpace(registryPath))
			{
				throw new ArgumentException(Resources.GORINP_RAW_ERR_CANNOT_READ_DATA, nameof(registryPath));
			}

			string[] regValue = registryPath.Split('#');

			regValue[0] = regValue[0].Substring(4);

			// Don't add RDP devices.
			if ((regValue.Length > 0) &&
				(regValue[1].StartsWith("RDP_", StringComparison.OrdinalIgnoreCase)))
			{
				_log.Print("WARNING: This is an RDP device.  Raw input in Gorgon is not supported under RDP.  Skipping this device.", LoggingLevel.Verbose);
				return null;
			}

			using (RegistryKey deviceKey = Registry.LocalMachine.OpenSubKey($@"System\CurrentControlSet\Enum\{regValue[0]}\{regValue[1]}\{regValue[2]}",
																			false))
			{
				if (deviceKey?.GetValue("DeviceDesc") == null)
				{
					return null;
				}

				if (deviceKey.GetValue("Class") != null)
				{
					return deviceKey.GetValue("Class").ToString();
				}

				// Windows 8 no longer has a "Class" value in this area, so we need to go elsewhere to get it.
				if (deviceKey.GetValue("ClassGUID") == null)
				{
					return null;
				}

				string classGUID = deviceKey.GetValue("ClassGUID").ToString();

				if (string.IsNullOrWhiteSpace(classGUID))
				{
					return null;
				}

				using (RegistryKey classKey = Registry.LocalMachine.OpenSubKey($@"System\CurrentControlSet\Control\Class\{classGUID}"))
				{
					return classKey?.GetValue("Class") == null ? null : classKey.GetValue("Class").ToString();
				}
			}
		}

		/// <summary>
		/// Function to retrieve the description of the raw input device from the registry.
		/// </summary>
		/// <param name="registryPath">Path to the registry key that holds the device description.</param>
		/// <returns>The device description.</returns>
		private string GetDeviceDescription(string registryPath)
		{
			if (string.IsNullOrWhiteSpace(registryPath))
			{
				throw new ArgumentException(Resources.GORINP_RAW_ERR_CANNOT_READ_DATA, nameof(registryPath));
			}

			string[] regValue = registryPath.Split('#');

			regValue[0] = regValue[0].Substring(4);

			// Don't add RDP devices.
			if ((regValue.Length > 0) &&
				(regValue[1].StartsWith("RDP_", StringComparison.OrdinalIgnoreCase)))
			{
				_log.Print("WARNING: This is an RDP device.  Raw input in Gorgon is not supported under RDP.  Skipping this device.", LoggingLevel.Verbose);
				return null;
			}

			using (RegistryKey deviceKey = Registry.LocalMachine.OpenSubKey($@"System\CurrentControlSet\Enum\{regValue[0]}\{regValue[1]}\{regValue[2]}",
																			false))
			{
				if (deviceKey?.GetValue("DeviceDesc") == null)
				{
					return null;
				}

				regValue = deviceKey.GetValue("DeviceDesc").ToString().Split(';');

				return regValue[regValue.Length - 1];
			}

		}

		/// <summary>
		/// Function to retrieve the device name.
		/// </summary>
		/// <param name="deviceHandle">Handle to the device.</param>
		/// <param name="deviceType">Type of device.</param>
		/// <returns>A device name structure.</returns>
		private T GetRawInputDeviceInfo<T>(IntPtr deviceHandle, InputDeviceType deviceType)
			where T : class, IGorgonInputDeviceInfo2
		{
			int dataSize = 0;

			// If we're running under a terminal server session, then throw an exception.
			// At this point, Gorgon's raw input does not run very well under RDP (especially the mouse), so we'll disable it.
			if (SystemInformation.TerminalServerSession)
			{
				return null;
			}

			if (Win32API.GetRawInputDeviceInfo(deviceHandle, RawInputCommand.DeviceName, IntPtr.Zero, ref dataSize) < 0)
			{
				throw new Win32Exception(Resources.GORINP_RAW_ERR_CANNOT_READ_DATA);
			}

			// Do nothing if we have no data.
			if (dataSize == 0)
			{
				return null;
			}

			string regPath;

			unsafe
			{
				char* data = stackalloc char[dataSize];

				if (Win32API.GetRawInputDeviceInfo(deviceHandle, RawInputCommand.DeviceName, (IntPtr)data, ref dataSize) < 0)
				{
					throw new Win32Exception(Resources.GORINP_RAW_ERR_CANNOT_READ_DATA);
				}

				// The strings that come back from native land will end with a NULL terminator, so crop that off.
				regPath = new string(data, 0, dataSize - 1);
			}

			string className = GetClassName(regPath);
			string deviceDescription = GetDeviceDescription(regPath);

			switch (deviceType)
			{
				case InputDeviceType.Keyboard:
					return (new RawInputKeyboardInfo2(deviceDescription, className, regPath, deviceHandle)) as T;
				case InputDeviceType.Mouse:
					return (new RawInputMouseInfo2(deviceDescription, className, regPath, deviceHandle)) as T;
				default:
					return null;
			}
		}

		/// <summary>
		/// Function to handle raw input messages on the window.
		/// </summary>
		/// <param name="hwnd">The window handle.</param>
		/// <param name="msg">The message.</param>
		/// <param name="wParam">The parameter.</param>
		/// <param name="lParam">The parameter.</param>
		/// <returns>The result of the procedure.</returns>
		private IntPtr RawWndProc(IntPtr hwnd, WindowMessages msg, IntPtr wParam, IntPtr lParam)
		{
			if (_messageHook == null)
			{
				// If this happens, something got messed up (maybe devices weren't unbound?).
				throw new ObjectDisposedException(Resources.GORINP_RAW_HOOK_STILL_ACTIVE);
			}

			if (msg != WindowMessages.RawInput)
			{
				return _messageHook.CallPreviousWndProc(hwnd, msg, wParam, lParam);
			}
			
			bool callDefault = true;

			// Find our device GUID and route appropriately.
			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < _deviceHandles.Count; ++i)
			{
				if ((_deviceHandles[i].Item2 != lParam) && (_deviceHandles[i].Item2 != IntPtr.Zero))
				{
					continue;
				}

				IGorgonInputDevice device;

				// The device requested is not registered, so we can skip it.
				if ((!_registeredDevices.TryGetValue(_deviceHandles[i].Item1, out device))
					|| (!device.IsAcquired))
				{
					continue;
				}

				RAWINPUT data = _messageHook.GetRawInputData(lParam);

				switch (data.Header.Type)
				{
					case RawInputType.Keyboard:
						RawInputProcessor processor;

						// There's no hook on this window, so we can't send anything.
						if (!_rawInputProcessors.TryGetValue(device.Window, out processor))
						{
							continue;
						}

						GorgonKeyboardData keyboardData;

						if (!processor.ProcessRawInputMessage(ref data.Union.Keyboard, out keyboardData))
						{
							continue;
						}
						
						RouteKeyboardData(device.UUID, ref keyboardData);

						callDefault = false;
						break;
				}
			}

			return callDefault ? _messageHook.CallPreviousWndProc(hwnd, msg, wParam, lParam) : IntPtr.Zero;
			/*
			// If we have a pointing device set as exclusive, then block all WM_MOUSE messages.
			if ((_exclusiveDevices & InputDeviceExclusivity.Mouse) == InputDeviceExclusivity.Mouse)
			{
				switch (msg)
				{
					case WindowMessages.XButtonDoubleClick:
					case WindowMessages.LeftButtonDoubleClick:
					case WindowMessages.RightButtonDoubleClick:
					case WindowMessages.MiddleButtonDoubleClick:
					case WindowMessages.LeftButtonDown:
					case WindowMessages.LeftButtonUp:
					case WindowMessages.RightButtonDown:
					case WindowMessages.RightButtonUp:
					case WindowMessages.MiddleButtonDown:
					case WindowMessages.MiddleButtonUp:
					case WindowMessages.XButtonDown:
					case WindowMessages.XButtonUp:
					case WindowMessages.MouseMove:
					case WindowMessages.MouseWheel:
						return IntPtr.Zero;
				}
			}
			*/
		}

		/// <inheritdoc/>
		protected override IReadOnlyList<IGorgonKeyboardInfo2> OnEnumerateKeyboards()
		{
			IEnumerable<RAWINPUTDEVICELIST> devices = _enumeratedDevices.Value.Where(item => item.DeviceType == RawInputType.Keyboard);

			// Put the system keyboard first.
			var result = new List<IGorgonKeyboardInfo2>
			             {
				             new RawInputKeyboardInfo2(Resources.GORINP_RAW_SYSTEM_KEYBOARD, "Keyboard", "SystemKeyboard", IntPtr.Zero)
			             };

			foreach (var keyboardDevice in devices)
			{
				IRawInputKeyboardInfo2 info = GetRawInputDeviceInfo<IRawInputKeyboardInfo2>(keyboardDevice.Device, InputDeviceType.Keyboard);

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
			IEnumerable<RAWINPUTDEVICELIST> devices = _enumeratedDevices.Value.Where(item => item.DeviceType == RawInputType.Mouse);
			// Put the system mouse first.
			var result = new List<IGorgonMouseInfo2>
			             {
				             new RawInputMouseInfo2(Resources.GORINP_RAW_SYSTEM_MOUSE, "Mouse", "SystemMouse", IntPtr.Zero)
			             };

			foreach (RAWINPUTDEVICELIST mouse in devices)
			{
				IRawInputMouseInfo2 info = GetRawInputDeviceInfo<IRawInputMouseInfo2>(mouse.Device, InputDeviceType.Mouse);

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
		protected override void RegisterDevice(IGorgonInputDevice device, IGorgonInputDeviceInfo2 deviceInfo, Form parentForm, Control window, bool exclusive)
		{
			IntPtr deviceHandle;
		
			// Register the exclusive devices with the service so it knows how to handle messages.
			switch (deviceInfo.InputDeviceType)
			{
				case InputDeviceType.Keyboard:
					// Keep track of the keyboard we're registering so we can forward data to it.
					deviceHandle = ((IRawInputKeyboardInfo2)deviceInfo).Handle;

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
					//RegisterRawInputMouse(window.Handle);

					// Keep track of the mice we're registering.
					deviceHandle = ((IRawInputMouseInfo2)deviceInfo).Handle;
					break;
				default:
					return;
			}

			if (!_rawInputProcessors.ContainsKey(window))
			{
				_rawInputProcessors[window] = new RawInputProcessor();
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
				_messageHook = new RawInputMessageHooker(parentForm.Handle, RawWndProc);
				_messageHook.HookWindow();
				_messageHook.RegisterRawInputDevices();
			}

			_messageHook.SetExclusiveState(deviceInfo.InputDeviceType, exclusive);
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
			_enumeratedDevices = new Lazy<IEnumerable<RAWINPUTDEVICELIST>>(Win32API.EnumerateInputDevices);
		}
		#endregion
	}
}
