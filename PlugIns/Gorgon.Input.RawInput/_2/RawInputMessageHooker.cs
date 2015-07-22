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
// Created: Tuesday, July 14, 2015 10:50:38 PM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Input.Raw.Properties;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// Delete for our custom window proc.
	/// </summary>
	/// <param name="hWnd">Window handle.</param>
	/// <param name="msg">Message.</param>
	/// <param name="wParam">Parameter</param>
	/// <param name="lParam">Parameter.</param>
	/// <returns>Result.</returns>
	delegate IntPtr WndProc(IntPtr hWnd, WindowMessages msg, IntPtr wParam, IntPtr lParam);

	/// <summary>
	/// Hooks a window procedure with a custom message filter.
	/// </summary>
	class RawInputMessageHooker
		: IDisposable
	{
		#region Variables.
		// The size of the raw input data header.
		private readonly int _headerSize = DirectAccess.SizeOf<RAWINPUTHEADER>();
		// Previous window procedure.
		private IntPtr _oldWndProc;
		// New window procedure.
		private IntPtr _newWndProc;
		// New window procedure.
		private WndProc _wndProc;
		// The window handle to hook.
		private IntPtr _windowHandle;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to unhook the window procedure.
		/// </summary>
		private void UnhookWindowProc()
		{
			UnregisterRawInputDevices();

			if ((_windowHandle == IntPtr.Zero)
				|| (_newWndProc == IntPtr.Zero)
				|| (_oldWndProc == IntPtr.Zero))
			{
				return;
			}

			IntPtr currentWndProc = Win32API.GetWindowLong(new HandleRef(this, _windowHandle), WindowLongType.WndProc);

			// We only unhook our own procedure, if someone else hooks after us, and does not clean up, then there's nothing 
			// we can do, and rather than bring down a mess of procedures inappropriately, it's best if we just leave it be.
			// It may still be active on the window, but for all intents and purposes, it should be harmless and cause a tiny 
			// bit of unnecessary overhead in the worst case.
			if (currentWndProc == _newWndProc)
			{
				Win32API.SetWindowLong(new HandleRef(this, _windowHandle), WindowLongType.WndProc, _oldWndProc);
			}

			_wndProc = null;
			_windowHandle = IntPtr.Zero;
			_newWndProc = IntPtr.Zero;
			_oldWndProc = IntPtr.Zero;
		}

		/// <summary>
		/// Function to call the previous window procedure.
		/// </summary>
		/// <param name="handle">The handle.</param>
		/// <param name="message">The message.</param>
		/// <param name="wParam">The w parameter.</param>
		/// <param name="lParam">The l parameter.</param>
		/// <returns>
		/// The return value from the previous window handler.
		/// </returns>
		public IntPtr CallPreviousWndProc(IntPtr handle, WindowMessages message, IntPtr wParam, IntPtr lParam)
		{
			return _oldWndProc == IntPtr.Zero ? IntPtr.Zero : Win32API.CallWindowProc(_oldWndProc, handle, message, wParam, lParam);
		}

		/// <summary>
		/// Function to retrieve data for the raw input device message.
		/// </summary>
		/// <param name="deviceHandle">Raw input device handle.</param>
		public RAWINPUT GetRawInputData(IntPtr deviceHandle)
		{
			int dataSize = 0;

			// Get data size.			
			int retVal = Win32API.GetRawInputData(deviceHandle, RawInputCommand.Input, IntPtr.Zero, ref dataSize, _headerSize);

			if (retVal == -1)
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GORINP_RAW_ERR_CANNOT_READ_DATA);
			}

			// Get actual data.
			unsafe
			{
				var rawInputPtr = stackalloc byte[dataSize];
				retVal = Win32API.GetRawInputData(deviceHandle,
				                                  RawInputCommand.Input,
				                                  (IntPtr)rawInputPtr,
				                                  ref dataSize,
				                                  _headerSize);

				if ((retVal == -1)
				    || (retVal != dataSize))
				{
					throw new GorgonException(GorgonResult.CannotRead, Resources.GORINP_RAW_ERR_CANNOT_READ_DATA);
				}

				RAWINPUT result = *((RAWINPUT*)rawInputPtr);
				return result;
			}
		}

		/// <summary>
		/// Function to unregister all raw input devices.
		/// </summary>
		public void UnregisterRawInputDevices()
		{
			RAWINPUTDEVICE[] devices = Win32API.GetRegisteredDevices();

			if (devices.Length == 0)
			{
				return;
			}

			for (int i = 0; i < devices.Length; ++i)
			{
				RAWINPUTDEVICE device = devices[i];

				device.Flags = RawInputDeviceFlags.Remove;
				device.WindowHandle = IntPtr.Zero;

				devices[i] = device;
			}

			if (!Win32API.RegisterRawInputDevices(devices, devices.Length, DirectAccess.SizeOf<RAWINPUTDEVICE>()))
			{
				throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_ERR_CANNOT_UNBIND_INPUT_DEVICES);
			}
		}

		/// <summary>
		/// Function to register a raw input de
		/// </summary>
		public void RegisterRawInputDevices()
		{
			RAWINPUTDEVICE[] devices = Win32API.GetRegisteredDevices();

			if (devices.Length != 0)
			{
				return;
			}
			// If we haven't previously registered devices, then do so now.
			devices = new[]
			          {
				          new RAWINPUTDEVICE
				          {
					          Usage = (ushort)HIDUsage.Keyboard,
					          UsagePage = HIDUsagePage.Generic,
					          Flags = RawInputDeviceFlags.InputSink,
					          WindowHandle = _windowHandle
				          },
				          new RAWINPUTDEVICE
				          {
					          Usage = (ushort)HIDUsage.Mouse,
					          UsagePage = HIDUsagePage.Generic,
					          Flags = RawInputDeviceFlags.InputSink,
					          WindowHandle = _windowHandle
				          }
			          };

			if (!Win32API.RegisterRawInputDevices(devices, devices.Length, DirectAccess.SizeOf<RAWINPUTDEVICE>()))
			{
				throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_ERR_CANNOT_UNBIND_INPUT_DEVICES);
			}
		}

		/// <summary>
		/// Function to set the exclusivity state for a device.
		/// </summary>
		/// <param name="deviceType">Type of device to set as exclusive.</param>
		/// <param name="exclusive"><b>true</b> to make the raw input for the device type exclusive to the application, <b>false</b> to make it shared.</param>
		/// <returns><b>true</b> if the device was made exclusive, <b>false</b> if not.</returns>
		public bool SetExclusiveState(InputDeviceType deviceType, bool exclusive)
		{
			RAWINPUTDEVICE[] devices = Win32API.GetRegisteredDevices();

			if (devices.Length == 0)
			{
				return false;
			}

			ushort usageFlag = 0;

			// Find the appropriate HID usage flag.
			switch (deviceType)
			{
				case InputDeviceType.Keyboard:
					usageFlag = (ushort)HIDUsage.Keyboard;
					break;
				case InputDeviceType.Mouse:
					usageFlag = (ushort)HIDUsage.Mouse;
					break;
			}

			bool result = false;
			RawInputDeviceFlags flags = exclusive ? RawInputDeviceFlags.InputSink | RawInputDeviceFlags.NoLegacy : RawInputDeviceFlags.InputSink;

			if (deviceType == InputDeviceType.Mouse)
			{
				flags |= RawInputDeviceFlags.CaptureMouse;
			}

			// Find the device we're looking for, and alter its settings.
			for (int i = 0; i < devices.Length; ++i)
			{
				RAWINPUTDEVICE device = devices[i];

                if (device.Usage != usageFlag)
				{
					continue;
				}

				device.Flags = flags;

				devices[i] = device;
				result = true;
				break;
			}

			if (!Win32API.RegisterRawInputDevices(devices, devices.Length, DirectAccess.SizeOf<RAWINPUTDEVICE>()))
			{
				throw new GorgonException(GorgonResult.CannotBind, Resources.GORINP_RAW_ERR_CANNOT_BIND_INPUT_DEVICES);
			}

			return result;
		}

		/// <summary>
		/// Function to hook the raw input functionality to the application.
		/// </summary>
		public void HookWindow()
		{
			// Do not do this more than once.
			if (_oldWndProc != IntPtr.Zero)
			{
				return;
			}

			// Hook the window procedure.
			_oldWndProc = Win32API.GetWindowLong(new HandleRef(this, _windowHandle), WindowLongType.WndProc);
			_newWndProc = Marshal.GetFunctionPointerForDelegate(_wndProc);

			Win32API.SetWindowLong(new HandleRef(this, _windowHandle), WindowLongType.WndProc, _newWndProc);
		}
		#endregion

		#region Constructors/Finalizers
		/// <summary>
		/// Initializes a new instance of the <see cref="RawInputMessageHooker"/> class.
		/// </summary>
		/// <param name="windowHandle">The window handle to hook.</param>
		/// <param name="newProc">The new window procedure to install.</param>
		public RawInputMessageHooker(IntPtr windowHandle, WndProc newProc)
		{
			_windowHandle = windowHandle;
			_wndProc = newProc;
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="RawInputMessageHooker"/> class.
		/// </summary>
		~RawInputMessageHooker()
		{
			UnhookWindowProc();
		}
		#endregion

		#region IDisposable Members
		/// <inheritdoc/>
		public void Dispose()
		{
			UnhookWindowProc();
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
