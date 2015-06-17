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
using Microsoft.Win32;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// Object representing the main interface to the input library.
	/// </summary>
	internal class GorgonRawInputService
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
		private IEnumerable<RAWINPUTDEVICELIST> _enumeratedDevices;
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
		/// Function to retrieve the device name.
		/// </summary>
		/// <param name="deviceHandle">Handle to the device.</param>
		/// <param name="deviceType">Type of device.</param>
		/// <param name="deviceList">List of devices.</param>
		/// <returns>A device name structure.</returns>
		private unsafe static void GetRawInputDeviceInfo(IntPtr deviceHandle, InputDeviceType deviceType, GorgonNamedObjectDictionary<GorgonInputDeviceInfo> deviceList)
		{
			int dataSize = 0;
			RegistryKey deviceKey = null;
			RegistryKey classKey = null;

			// If we're running under a terminal server session, then throw an exception.
			// At this point, Gorgon's raw input does not run very well under RDP (especially the mouse), so we'll disable it.
			if (SystemInformation.TerminalServerSession)
			{
				throw new GorgonException(GorgonResult.CannotEnumerate, Resources.GORINP_RAW_NOT_UNDER_RDP);
			}

		    if (Win32API.GetRawInputDeviceInfo(deviceHandle, RawInputCommand.DeviceName, IntPtr.Zero, ref dataSize) < 0)
		    {
				throw new Win32Exception(Resources.GORINP_RAW_CANNOT_READ_DATA);
		    }

			// Do nothing if we have no data.
			if (dataSize == 0)
			{
				return;
			}

            char *data = stackalloc char[dataSize];

		    try
		    {
		        if (Win32API.GetRawInputDeviceInfo(deviceHandle, RawInputCommand.DeviceName, (IntPtr)data, ref dataSize) < 0)
		        {
					throw new Win32Exception(Resources.GORINP_RAW_CANNOT_READ_DATA);
		        }

				// The strings that come back from native land will end with a NULL terminator, so crop that off.
		        var regPath = new string(data, 0, dataSize - 1);

		        if (regPath.Length == 0)
		        {
					throw new Win32Exception(Resources.GORINP_RAW_CANNOT_READ_DATA);
		        }

		        string[] regValue = regPath.Split('#');
		        string className = string.Empty;

		        regValue[0] = regValue[0].Substring(4);

		        // Don't add RDP devices.
		        if ((regValue.Length > 0) &&
		            (regValue[1].StartsWith("RDP_", StringComparison.OrdinalIgnoreCase)))
		        {
		            return;
		        }

		        deviceKey =
		            Registry.LocalMachine.OpenSubKey(
		                string.Format(@"System\CurrentControlSet\Enum\{0}\{1}\{2}", regValue[0], regValue[1],
		                                regValue[2]),
		                false);

		        if (deviceKey == null)
		        {
		            return;
		        }

		        int counter = 0;

		        regValue = deviceKey.GetValue("DeviceDesc").ToString().Split(';');

		        if (deviceKey.GetValue("Class") == null)
		        {
		            // Windows 8 no longer has a "Class" value in this area, so we need to go elsewhere to get it.
		            if (deviceKey.GetValue("ClassGUID") != null)
		            {
		                var classGUID = deviceKey.GetValue("ClassGUID").ToString();

		                if (!string.IsNullOrWhiteSpace(classGUID))
		                {
		                    classKey =
		                        Registry.LocalMachine.OpenSubKey(
		                            string.Format(@"System\CurrentControlSet\Control\Class\{0}", classGUID));

		                    if (classKey != null)
		                    {
		                        if (classKey.GetValue("Class") != null)
		                        {
		                            className = classKey.GetValue("Class").ToString();
		                        }
		                    }
		                }
		            }
		        }
		        else
		        {
		            className = deviceKey.GetValue("Class").ToString();
		        }

		        string baseName;
		        string name = baseName = regValue[regValue.Length - 1];

			    while (deviceList.Contains(name))
			    {
		            counter++;
		            name = baseName + " #" + counter;
		        }

		        deviceList.Add(new GorgonRawInputDeviceInfo(name, deviceType, className, regPath, deviceHandle));
		    }
		    finally
		    {
		        if (deviceKey != null)
		        {
		            deviceKey.Dispose();
		        }

		        if (classKey != null)
		        {
		            classKey.Dispose();
		        }
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
			RegistryKey rootKey = null;			// Root registry key.
			RegistryKey lookup = null;			// Look up key.
			RegistryKey nameKey = null;			// Name key.

		    try
			{
				string defaultName = joystickData.AxisCount + "-axis, " + joystickData.ButtonCount + "-button joystick.";	// Default name.
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
			        nameKey =
			            rootKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\" +
			                               key);

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
	            if ((ExclusiveDevices & InputDeviceType.PointingDevice) == InputDeviceType.PointingDevice)
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
	            if ((ExclusiveDevices & InputDeviceType.Keyboard) == InputDeviceType.Keyboard)
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
	            if (GorgonApplication.ApplicationForm == null)
	            {
		            throw new ArgumentException(Resources.GORINP_RAW_NO_WINDOW_TO_BIND, "windowHandle");
	            }

	            windowHandle = GorgonApplication.ApplicationForm.Handle;
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

		/// <summary>
		/// Function to enumerate the pointing devices on the system.
		/// </summary>
		/// <returns>A list of pointing device names.</returns>
		protected override IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> EnumeratePointingDevices()
		{
		    IEnumerable<RAWINPUTDEVICELIST> devices = _enumeratedDevices.Where(item => item.DeviceType == RawInputType.Mouse);
			var result = new GorgonNamedObjectDictionary<GorgonInputDeviceInfo>(false);

		    foreach (var pointingDevice in devices)
		    {
		        GetRawInputDeviceInfo(pointingDevice.Device, InputDeviceType.PointingDevice, result);
		    }

		    return result;
		}

		/// <summary>
		/// Function to enumerate the keyboard devices on the system.
		/// </summary>
		/// <returns>A list of keyboard device names.</returns>
		protected override IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> EnumerateKeyboardDevices()
		{
		    IEnumerable<RAWINPUTDEVICELIST> devices = _enumeratedDevices.Where(item => item.DeviceType == RawInputType.Keyboard);
			var result = new GorgonNamedObjectDictionary<GorgonInputDeviceInfo>(false);

		    foreach (var keyboardDevice in devices)
		    {
		        GetRawInputDeviceInfo(keyboardDevice.Device, InputDeviceType.Keyboard, result);
		    }

		    return result;
		}

		/// <summary>
		/// Function to enumerate the joystick devices attached to the system.
		/// </summary>
		/// <returns>A list of joystick device names.</returns>
		protected override IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> EnumerateJoysticksDevices()
		{
		    var capabilities = new JOYCAPS();	    // Joystick capabilities.
		    int nameCount = 0;						// Name counter.

			var result = new GorgonNamedObjectDictionary<GorgonInputDeviceInfo>(false);
			int deviceCount = Win32API.joyGetNumDevs();
			int capsSize = Marshal.SizeOf(typeof(JOYCAPS));

			GorgonApplication.Log.Print("Enumerating joysticks...", LoggingLevel.Intermediate);

			for (int i = 0; i < deviceCount; i++)
			{
			    int error = Win32API.joyGetDevCaps(i, ref capabilities, capsSize);							// Error code.

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
			        keyName = name + " " + nameCount;
			    }

			    result.Add(new GorgonMultimediaDeviceInfo(keyName, "Game device", "N/A", i));
			}

		    GorgonApplication.Log.Print("{0} joysticks found.", LoggingLevel.Intermediate, result.Count);

			return result;
		}

		/// <summary>
		/// Function called before enumeration begins.
		/// </summary>
		/// <remarks>
		/// Implementors can use this method to cache enumeration data.
		/// </remarks>
		protected override void OnBeforeEnumerate()
		{
			_enumeratedDevices = Win32API.EnumerateInputDevices();
		}

		/// <summary>
		/// Function to enumerate device types for which there is no class wrapper and will return data in a custom property collection.
		/// </summary>
		/// <returns>
		/// A list of custom HID types.
		/// </returns>
		protected override IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> EnumerateCustomHIDs()
		{
			IEnumerable<RAWINPUTDEVICELIST> devices =
				_enumeratedDevices.Where(item => item.DeviceType != RawInputType.Keyboard && item.DeviceType != RawInputType.Mouse);
			var result = new GorgonNamedObjectDictionary<GorgonInputDeviceInfo>(false);

		    foreach (var hidDevice in devices)
		    {
		        GetRawInputDeviceInfo(hidDevice.Device, InputDeviceType.HID, result);
		    }

		    return result;
		}

		/// <summary>
		/// Function to create a custom HID interface.
		/// </summary>
		/// <param name="hidInfo">A <see cref="Gorgon.Input.GorgonInputDeviceInfo">GorgonDeviceName</see> object containing the HID information.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>
		/// A new custom HID interface.
		/// </returns>
		protected override GorgonCustomHID CreateCustomHIDImpl(Control window, GorgonInputDeviceInfo hidInfo)
		{
			HookWindowProc(window.Handle);

            var rawInfo = hidInfo as GorgonRawInputDeviceInfo;

		    if (rawInfo == null)
		    {
                throw new InvalidCastException(Resources.GORINP_RAW_HIDINFO_NOT_RAW);
		    }

			return new RawHIDDevice(this, rawInfo);
		}

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="keyboardInfo">Name of the keyboard device to create.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new keyboard interface.</returns>
		/// <remarks>Passing NULL for <paramref name="keyboardInfo"/> will use the system keyboard.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</para></remarks>
		protected override GorgonKeyboard CreateKeyboardImpl(Control window, GorgonInputDeviceInfo keyboardInfo)
		{
            HookWindowProc(window.Handle);

		    RawKeyboard keyboard = keyboardInfo == null
		                               ? new RawKeyboard(this, "System Keyboard", IntPtr.Zero)
		                               : new RawKeyboard(this, keyboardInfo.Name, ((GorgonRawInputDeviceInfo)keyboardInfo).Handle);
			return keyboard;
		}

		/// <summary>
		/// Function to create a pointing device interface.
		/// </summary>
		/// <param name="pointingDeviceInfo">Name of the pointing device device to create.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new pointing device interface.</returns>
		/// <remarks>Passing NULL for <paramref name="pointingDeviceInfo"/> will use the system pointing device.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</para>
		/// </remarks>
		protected override GorgonPointingDevice CreatePointingDeviceImpl(Control window, GorgonInputDeviceInfo pointingDeviceInfo)
		{
            HookWindowProc(window.Handle);

		    RawPointingDevice mouse = pointingDeviceInfo == null
		                                  ? new RawPointingDevice(this, "System Mouse", IntPtr.Zero)
		                                  : new RawPointingDevice(this, pointingDeviceInfo.Name,
		                                                          ((GorgonRawInputDeviceInfo)pointingDeviceInfo).Handle);
			return mouse;
		}

		/// <summary>
		/// Function to create a joystick interface.
		/// </summary>
		/// <param name="joystickInfo">A <see cref="Gorgon.Input.GorgonInputDeviceInfo">GorgonDeviceName</see> object containing the joystick information.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new joystick interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</remarks>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="joystickInfo"/> is NULL.</exception>
		protected override GorgonJoystick CreateJoystickImpl(Control window, GorgonInputDeviceInfo joystickInfo)
		{
			var mmJoystick = joystickInfo as GorgonMultimediaDeviceInfo;

			if (mmJoystick == null)
			{
				throw new InvalidCastException(Resources.GORINP_RAW_NOT_MM_JOYSTICK);
			}

			return new MultimediaJoystick(this, mmJoystick.JoystickID, joystickInfo.Name);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
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
			: base("Gorgon Raw Input")
		{
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="GorgonRawInputService"/> class.
		/// </summary>
		~GorgonRawInputService()
		{
			Dispose(false);
		}
		#endregion
	}
}
