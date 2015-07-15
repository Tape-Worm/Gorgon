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
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Gorgon.Collections;
using Gorgon.Collections.Specialized;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Input.Raw.Properties;
using Gorgon.Native;
using Gorgon.UI;
using Microsoft.Win32;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// Raw input/Windows Multimedia joystick service.
	/// </summary>
	class GorgonRawInputService
		: GorgonInputService
	{
		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;			
		// Previous window procedure.
	    private IntPtr _oldWndProc;								
		// Window that has its wnd proc hooked up.
	    private IntPtr _hookedWindow;							
		// New window procedure.
	    private IntPtr _newWndProc;								
		// New window procedure.
	    private WndProc _wndProc;							
		// List of enumerated devices.
		private readonly Lazy<IEnumerable<RAWINPUTDEVICELIST>> _enumeratedDevices;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the filter used to forward raw input messages to the devices.
		/// </summary>
		public MessageFilter MessageFilter
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Delete for our custom window proc.
        /// </summary>
        /// <param name="hWnd">Window handle.</param>
        /// <param name="msg">Message.</param>
        /// <param name="wParam">Parameter</param>
        /// <param name="lParam">Parameter.</param>
        /// <returns>Result.</returns>
        private delegate IntPtr WndProc(IntPtr hWnd, WindowMessages msg, IntPtr wParam, IntPtr lParam);

		/// <summary>
		/// Function to retrieve the class name for the device.
		/// </summary>
		/// <param name="registryPath">Path to the class information in the registry.</param>
		/// <returns>The class name for the device.</returns>
		private string GetClassName(string registryPath)
		{
			if (string.IsNullOrWhiteSpace(registryPath))
			{
				throw new ArgumentException(Resources.GORINP_RAW_ERR_CANNOT_READ_DATA, "registryPath");
			}

			string[] regValue = registryPath.Split('#');

			regValue[0] = regValue[0].Substring(4);

			// Don't add RDP devices.
			if ((regValue.Length > 0) &&
				(regValue[1].StartsWith("RDP_", StringComparison.OrdinalIgnoreCase)))
			{
				Log.Print("WARNING: This is an RDP device.  Raw input in Gorgon is not supported under RDP.  Skipping this device.", LoggingLevel.Verbose);
				return null;
			}

			using (RegistryKey deviceKey = Registry.LocalMachine.OpenSubKey(string.Format(@"System\CurrentControlSet\Enum\{0}\{1}\{2}",
			                                                                              regValue[0],
			                                                                              regValue[1],
			                                                                              regValue[2]),
			                                                                false))
			{
				if ((deviceKey == null) || (deviceKey.GetValue("DeviceDesc") == null))
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

				using (RegistryKey classKey = Registry.LocalMachine.OpenSubKey(string.Format(@"System\CurrentControlSet\Control\Class\{0}", classGUID)))
				{
					if ((classKey == null) || (classKey.GetValue("Class") == null))
					{
						return null;
					}

					return classKey.GetValue("Class").ToString();
				}
			}
		}

		/// <summary>
		/// Function to retrieve the name of the raw input device from the registry.
		/// </summary>
		/// <param name="registryPath">Path to the registry key that holds the device name.</param>
		/// <returns>The device name.</returns>
		private string GetDeviceName(string registryPath)
		{
			if (string.IsNullOrWhiteSpace(registryPath))
			{
				throw new ArgumentException(Resources.GORINP_RAW_ERR_CANNOT_READ_DATA, "registryPath");
			}

			string[] regValue = registryPath.Split('#');

			regValue[0] = regValue[0].Substring(4);

			// Don't add RDP devices.
			if ((regValue.Length > 0) &&
				(regValue[1].StartsWith("RDP_", StringComparison.OrdinalIgnoreCase)))
			{
				Log.Print("WARNING: This is an RDP device.  Raw input in Gorgon is not supported under RDP.  Skipping this device.", LoggingLevel.Verbose);
				return null;
			}

			using (RegistryKey deviceKey = Registry.LocalMachine.OpenSubKey(string.Format(@"System\CurrentControlSet\Enum\{0}\{1}\{2}",
			                                                                              regValue[0],
			                                                                              regValue[1],
			                                                                              regValue[2]),
			                                                                false))
			{
				if ((deviceKey == null) || (deviceKey.GetValue("DeviceDesc") == null))
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
			where T : class, IGorgonInputDeviceInfo
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
			string name = GetDeviceName(regPath);

			switch (deviceType)
			{
				case InputDeviceType.Keyboard:
					return (new RawInputKeyboardInfo(Guid.NewGuid(), name, className, regPath, deviceHandle)) as T;
				case InputDeviceType.Mouse:
					return (new RawInputMouseInfo(Guid.NewGuid(), name, className, regPath, deviceHandle)) as T;
				case InputDeviceType.HumanInterfaceDevice:
					return (new RawInputHumanInterfaceDeviceInfo(Guid.NewGuid(), name, className, regPath, deviceHandle)) as T;
				default:
					return null;
			}
		}

		/// <summary>
		/// Function to retrieve a joystick name from the registry.
		/// </summary>
		/// <param name="joystickData">Joystick capability data.</param>
		/// <param name="joystickID">ID of the joystick to retrieve data for.</param>
		/// <returns>The name of the joystick.</returns>
		private static string GetJoystickName(JOYCAPS joystickData, int joystickID)
		{
			// Root registry key.
			RegistryKey rootKey = null;
			// Look up key.
			RegistryKey lookup = null;
			// Name key.
			RegistryKey nameKey = null;			

		    try
			{
				// Default name.
				string defaultName = joystickData.AxisCount + "-axis, " + joystickData.ButtonCount + "-button joystick.";	

				rootKey = Registry.CurrentUser;

				// Get the device ID.				
				lookup = rootKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\MediaResources\Joystick\" + joystickData.RegistryKey + @"\CurrentJoystickSettings");

				// Try the local machine key as a root if that lookup failed.
				if (lookup == null)
				{
					rootKey.Close();
					rootKey = Registry.LocalMachine;
					lookup = rootKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\MediaResources\Joystick\" + joystickData.RegistryKey + @"\CurrentJoystickSettings");
				}

			    if (lookup != null)
			    {
                    // Key name.
			        string key = lookup.GetValue("Joystick" + (joystickID + 1) + "OEMName", string.Empty).ToString();
			            
			        // If we have no name, then build one.
			        if (string.IsNullOrWhiteSpace(key))
			        {
			            return defaultName;
			        }

			        // Get the name.
			        nameKey = rootKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\" + key);

			        return nameKey != null ? nameKey.GetValue("OEMName", defaultName).ToString() : defaultName;
			    }
			    else
			    {
			        return defaultName;
			    }
			}
			finally
			{
			    if (nameKey != null)
			    {
			        nameKey.Close();
			    }
			    if (lookup != null)
			    {
			        lookup.Close();
			    }
			    if (rootKey != null)
			    {
			        rootKey.Close();
			    }
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
            if (_disposed)
            {
				throw new ObjectDisposedException(Resources.GORINP_RAW_HOOK_STILL_ACTIVE);
            }

            if (msg != WindowMessages.RawInput)
            {
				// If we have a pointing device set as exclusive, then block all WM_MOUSE messages.
	            if ((ExclusiveDevices & InputDeviceExclusivity.Mouse) == InputDeviceExclusivity.Mouse)
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

	            // ReSharper disable once InvertIf
				if ((ExclusiveDevices & InputDeviceExclusivity.Keyboard) == InputDeviceExclusivity.Keyboard)
	            {
		            switch (msg)
		            {
						case WindowMessages.KeyDown:
   						case WindowMessages.KeyUp:
						case WindowMessages.Char:
						case WindowMessages.UniChar:
						case WindowMessages.AppCommand:
						case WindowMessages.DeadChar:
						case WindowMessages.HotKey:
				            return IntPtr.Zero;
						case WindowMessages.SysKeyDown:
				            var vKey = (VirtualKeys)wParam;

							// Allow Alt+F4, Alt+Tab and Ctrl + Escape
				            return vKey == VirtualKeys.F4 ? Win32API.CallWindowProc(_oldWndProc, hwnd, msg, wParam, lParam) : IntPtr.Zero;
		            }
	            }

                return Win32API.CallWindowProc(_oldWndProc, hwnd, msg, wParam, lParam);
            }

            var inputMessage = new Message
                               {
                                   HWnd = hwnd,
                                   Msg = (int)msg,
                                   LParam = lParam,
                                   WParam = wParam
                               };

            return MessageFilter.PreFilterMessage(ref inputMessage)
                       ? inputMessage.Result
                       : Win32API.CallWindowProc(_oldWndProc, hwnd, msg, wParam, lParam);
	    }

        /// <summary>
        /// Function to hook the window procedure.
        /// </summary>
        /// <param name="windowHandle">Window handle to hook.</param>
	    private void HookWindowProc(IntPtr windowHandle)
	    {
            // Only need to hook the application window, but if that's not available for whatever reason
            // we can hook into the control being used to receive raw input events.  At the very minimum we 
            // need a window of some kind in order to process the WM_INPUT event.  If one is not available, 
            // then we must throw an exception.
            if (MessageFilter != null)
            {
                return;
            }

            if (windowHandle == IntPtr.Zero)
            {
	            if (GorgonApplication.MainForm == null)
	            {
		            throw new ArgumentException(Resources.GORINP_RAW_NO_WINDOW_TO_BIND, "windowHandle");
	            }

	            windowHandle = GorgonApplication.MainForm.Handle;
            }

			_hookedWindow = windowHandle;

            // Hook the window procedure.
            _oldWndProc = Win32API.GetWindowLong(new HandleRef(this, _hookedWindow), WindowLongType.WndProc);

            _wndProc = RawWndProc;
            _newWndProc = Marshal.GetFunctionPointerForDelegate(_wndProc);

            Win32API.SetWindowLong(new HandleRef(this, _hookedWindow), WindowLongType.WndProc, _newWndProc);

            MessageFilter = new MessageFilter();
	    }

        /// <summary>
        /// Function to unhook the window procedure.
        /// </summary>
	    private void UnhookWindowProc()
	    {
	        if ((MessageFilter == null)
                || (_hookedWindow == IntPtr.Zero))
	        {
	            return;
	        }
            
            MessageFilter = null;

            IntPtr currentWndProc = Win32API.GetWindowLong(new HandleRef(this, _hookedWindow), WindowLongType.WndProc);

            if (currentWndProc == _newWndProc)
            {
				Win32API.SetWindowLong(new HandleRef(this, _hookedWindow), WindowLongType.WndProc, _oldWndProc);
            }
            
			_hookedWindow = IntPtr.Zero;
			_newWndProc = IntPtr.Zero;
	        _oldWndProc = IntPtr.Zero;
	    }

		/// <inheritdoc/>
		protected override IGorgonNamedObjectReadOnlyDictionary<IGorgonMouseInfo> OnEnumerateMice()
		{
		    IEnumerable<RAWINPUTDEVICELIST> devices = _enumeratedDevices.Value.Where(item => item.DeviceType == RawInputType.Mouse);
			var result = new GorgonNamedObjectDictionary<IGorgonMouseInfo>(false);

		    foreach (RAWINPUTDEVICELIST mouse in devices)
		    {
			    IRawInputMouseInfo info = GetRawInputDeviceInfo<IRawInputMouseInfo>(mouse.Device, InputDeviceType.Mouse);

				if (info == null)
				{
					Log.Print("WARNING: Could not retrieve the class and device name.  Skipping this device.", LoggingLevel.Verbose);
					continue;
				}

				Log.Print("Found pointing device: '{0}' on HID path {1}, class {2}.", LoggingLevel.Verbose, info.Name, info.HumanInterfaceDevicePath, info.ClassName);

				int counter = 0;
				string name = info.Name;

				// If the name is not unique, then make it unique.
				while (result.Contains(name))
				{
					++counter;
					name = info.Name + " #" + counter;
				}

				if (counter > 0)
				{
					// Recreate the value with the new name.
					info = new RawInputMouseInfo(info.UUID, name, info.ClassName, info.HumanInterfaceDevicePath, info.Handle);
				}

				result.Add(info);
		    }

		    return result;
		}

		/// <inheritdoc/>
		protected override IGorgonNamedObjectReadOnlyDictionary<IGorgonKeyboardInfo> OnEnumerateKeyboards()
		{
		    IEnumerable<RAWINPUTDEVICELIST> devices = _enumeratedDevices.Value.Where(item => item.DeviceType == RawInputType.Keyboard);
			var result = new GorgonNamedObjectDictionary<IGorgonKeyboardInfo>(false);

		    foreach (var keyboardDevice in devices)
		    {
				IRawInputKeyboardInfo info = GetRawInputDeviceInfo<IRawInputKeyboardInfo>(keyboardDevice.Device, InputDeviceType.Keyboard);

			    if (info == null)
			    {
					Log.Print("WARNING: Could not retrieve the class and device name.  Skipping this device.", LoggingLevel.Verbose);
				    continue;
			    }

				Log.Print("Found keyboard: '{0}' on HID path {1}, class {2}.", LoggingLevel.Verbose, info.Name, info.HumanInterfaceDevicePath, info.ClassName);

			    int counter = 0;
			    string name = info.Name;

				// If the name is not unique, then make it unique.
			    while (result.Contains(name))
			    {
					++counter;
				    name = info.Name + " #" + counter;
			    }

			    if (counter > 0)
			    {
					// Recreate the value with the new name.
				    info = new RawInputKeyboardInfo(info.UUID, name, info.ClassName, info.HumanInterfaceDevicePath, info.Handle);
			    }

				result.Add(info);
		    }

		    return result;
		}

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
			                                  string.Format(Resources.GORINP_RAW_ERR_CANNOT_GET_JOYSTICK_CAPS, i, error.FormatHex()));
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

		/// <inheritdoc/>
		protected override IGorgonNamedObjectReadOnlyDictionary<IGorgonHumanInterfaceDeviceInfo> OnEnumerateHumanInterfaceDevices()
		{
			IEnumerable<RAWINPUTDEVICELIST> devices =
				_enumeratedDevices.Value.Where(item => item.DeviceType != RawInputType.Keyboard && item.DeviceType != RawInputType.Mouse);

			var result = new GorgonNamedObjectDictionary<IGorgonHumanInterfaceDeviceInfo>(false);

			foreach (var keyboardDevice in devices)
			{
				IRawInputHumanInterfaceDeviceInfo info = GetRawInputDeviceInfo<IRawInputHumanInterfaceDeviceInfo>(keyboardDevice.Device, InputDeviceType.HumanInterfaceDevice);

				if (info == null)
				{
					Log.Print("WARNING: Could not retrieve the class and device name.  Skipping this device.", LoggingLevel.Verbose);
					continue;
				}

				Log.Print("Found human interface device: '{0}' on HID path {1}, class {2}.", LoggingLevel.Verbose, info.Name, info.HumanInterfaceDevicePath, info.ClassName);

				int counter = 0;
				string name = info.Name;

				// If the name is not unique, then make it unique.
				while (result.Contains(name))
				{
					++counter;
					name = info.Name + " #" + counter;
				}

				if (counter > 0)
				{
					// Recreate the value with the new name.
					info = new RawInputHumanInterfaceDeviceInfo(info.UUID, name, info.ClassName, info.HumanInterfaceDevicePath, info.Handle);
				}

				result.Add(info);
			}

		    return result;
		}

		/// <inheritdoc/>
		protected override GorgonCustomHID OnCreateHumanInterfaceDevice(Control window, IGorgonHumanInterfaceDeviceInfo hidInfo)
		{
			HookWindowProc(window.Handle);

            IRawInputHumanInterfaceDeviceInfo rawInfo = hidInfo as IRawInputHumanInterfaceDeviceInfo;

		    if (rawInfo == null)
		    {
                throw new InvalidCastException(Resources.GORINP_RAW_HIDINFO_NOT_RAW);
		    }

			return new RawHIDDevice(this, rawInfo);
		}

		/// <inheritdoc/>
		protected override GorgonKeyboard OnCreateKeyboard(Control window, IGorgonKeyboardInfo keyboardInfo)
		{
            HookWindowProc(window.Handle);

			if (keyboardInfo == null)
			{
				return new RawKeyboard(this,
				                       new RawInputKeyboardInfo(Guid.Empty,
				                                                Resources.GORINP_RAW_SYSTEM_KEYBOARD,
				                                                "Keyboard",
																"SystemKeyboard",
				                                                IntPtr.Zero));
			}

			return new RawKeyboard(this, (IRawInputKeyboardInfo)keyboardInfo);
		}

		/// <inheritdoc/>
		protected override GorgonPointingDevice OnCreateMouse(Control window, IGorgonMouseInfo pointingDeviceInfo)
		{
            HookWindowProc(window.Handle);

			if (pointingDeviceInfo == null)
			{
				return new RawPointingDevice(this,
				                             new RawInputMouseInfo(Guid.Empty,
				                                                   Resources.GORINP_RAW_SYSTEM_MOUSE,
				                                                   "Mouse",
				                                                   "SystemMouse",
				                                                   IntPtr.Zero));
			}

			return new RawPointingDevice(this, (IRawInputMouseInfo)pointingDeviceInfo);
			
		}

		/// <inheritdoc/>
		protected override GorgonJoystick OnCreateJoystick(Control window, IGorgonJoystickInfo joystickInfo)
		{
			var multimediaJoystick = joystickInfo as IMultimediaJoystickInfo;

			if (multimediaJoystick == null)
			{
				throw new InvalidCastException(Resources.GORINP_RAW_NOT_MM_JOYSTICK);
			}

			return new MultimediaJoystick(this, multimediaJoystick);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			// TODO: Move the unmanaged window hook code into its own class. It shouldn't be in this one.
			if (!_disposed)
			{
				if (disposing)
				{
					UnbindAllDevices();
					UnhookWindowProc();
				}
				else
				{
					if ((_hookedWindow != IntPtr.Zero)
					    && (_oldWndProc != IntPtr.Zero))
					{
						IntPtr currentWndProc = Win32API.GetWindowLong(new HandleRef(this, _hookedWindow), WindowLongType.WndProc);

						// Ensure that we're unhooking our window hook.
						if (currentWndProc == _newWndProc)
						{
							// Attempt to unbind the window if we're being collected without being disposed.
							Win32API.SetWindowLong(new HandleRef(this, _hookedWindow), WindowLongType.WndProc, _oldWndProc);
						}
					}
				}

				_newWndProc = IntPtr.Zero;
				_hookedWindow = IntPtr.Zero;
				_oldWndProc = IntPtr.Zero;

				_disposed = true;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRawInputService"/> class.
		/// </summary>
		public GorgonRawInputService()
			: base(Resources.GORINP_RAW_SERVICEDESC)
		{
			_enumeratedDevices = new Lazy<IEnumerable<RAWINPUTDEVICELIST>>(Win32API.EnumerateInputDevices);
		}
		#endregion
	}
}
