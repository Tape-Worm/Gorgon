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
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
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
	class RawInputNative
		: IDisposable
	{
		#region Variables.
		// The size of the raw input data header.
		private readonly int _headerSize = DirectAccess.SizeOf<RAWINPUTHEADER>();
		// Previous window procedure.
		private IntPtr _oldWndProc;
		// New window procedure.
		private IntPtr _newWndProc;
		// The window handle to hook.
		private IntPtr _windowHandle;
		// Reference to the new window procedure delegate. This will keep the proc from being GC'd.
		private WndProc _wndProcRef;
		// A list of devices registered with the service.
		private readonly IReadOnlyList<Tuple<Guid, IntPtr>> _deviceHandles;
		// The raw input processor for device data.
		private readonly IReadOnlyDictionary<Control, RawInputProcessor> _rawInputProcessors;
		// A list of registered devices.
		private readonly IReadOnlyDictionary<Guid, IGorgonInputDevice> _registeredDevices;
		// Flag to indicate that the mouse is exclusive (used to correct an issue with raw input mice).
		private bool _isMouseExclusive;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to handle raw input messages from the window.
		/// </summary>
		/// <param name="hwnd">Window handle that received the message.</param>
		/// <param name="msg">The message to process.</param>
		/// <param name="lParam">Parameter 1</param>
		/// <param name="wParam">Parameter 2</param>
		/// <returns>The result of the message processing.</returns>
		private IntPtr WndProc(IntPtr hwnd, WindowMessages msg, IntPtr wParam, IntPtr lParam)
		{
			if (_oldWndProc == IntPtr.Zero)
			{
				// This shouldn't happen, but just in case we forgot to clean up or something, or the 
				// finalizer didn't run, then ensure that this method is dead for sure.
				throw new ObjectDisposedException(Resources.GORINP_RAW_HOOK_STILL_ACTIVE);
			}

			if ((msg != WindowMessages.RawInput) || (hwnd != _windowHandle))
			{
				if (!_isMouseExclusive)
				{
					return CallPreviousWndProc(hwnd, msg, wParam, lParam);
				}

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
					case WindowMessages.MouseHWheel:
					case WindowMessages.MouseHover:
					case WindowMessages.MouseLeave:
						return IntPtr.Zero;
				}

				return CallPreviousWndProc(hwnd, msg, wParam, lParam);
			}

			// Get the device data from the raw input system.
			RAWINPUT data = GetRawInputData(lParam);

			// Find our device GUID and route appropriately.
			// ReSharper disable once ForCanBeConvertedToForeach
			for (int i = 0; i < _deviceHandles.Count; ++i)
			{
				if ((_deviceHandles[i].Item2 != data.Header.Device) && (_deviceHandles[i].Item2 != IntPtr.Zero))
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

				RawInputProcessor processor;

				switch (data.Header.Type)
				{
					case RawInputType.Keyboard:
						if (!(device is IGorgonKeyboard))
						{
							continue;
						}

						// There's no processor on this window, so we can't send anything.
						if (!_rawInputProcessors.TryGetValue(device.Window, out processor))
						{
							continue;
						}

						processor.ProcessRawInputMessage(device, ref data.Union.Keyboard);
						break;
					case RawInputType.Mouse:
						if (!(device is IGorgonMouse))
						{
							continue;
						}

						// There's no processor on this window, so we can't send anything.
						if (!_rawInputProcessors.TryGetValue(device.Window, out processor))
						{
							continue;
						}

						processor.ProcessRawInputMessage(device, ref data.Union.Mouse);
						break;
				}
			}

			return CallPreviousWndProc(hwnd, msg, wParam, lParam);
		}

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
			
			_windowHandle = IntPtr.Zero;
			_newWndProc = IntPtr.Zero;
			_oldWndProc = IntPtr.Zero;
		}

		/// <summary>
		/// Function to retrieve data for the raw input device message.
		/// </summary>
		/// <param name="deviceHandle">Raw input device handle.</param>
		private RAWINPUT GetRawInputData(IntPtr deviceHandle)
		{
			int dataSize = 0;

			// Get data size.			
			int retVal = Win32API.GetRawInputData(deviceHandle, RawInputCommand.Input, IntPtr.Zero, ref dataSize, _headerSize);

			if (retVal == -1)
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
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
					throw new GorgonException(GorgonResult.CannotRead, Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
				}

				RAWINPUT result = *((RAWINPUT*)rawInputPtr);
				return result;
			}
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
		private IntPtr CallPreviousWndProc(IntPtr handle, WindowMessages message, IntPtr wParam, IntPtr lParam)
		{
			return _oldWndProc == IntPtr.Zero ? IntPtr.Zero : Win32API.CallWindowProc(_oldWndProc, handle, message, wParam, lParam);
		}

		/// <summary>
		/// Function to unregister all raw input devices.
		/// </summary>
		private static void UnregisterRawInputDevices()
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
		private void RegisterRawInputDevices()
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
			RawInputDeviceFlags flags = RawInputDeviceFlags.None;

			// Find the appropriate HID usage flag.
			switch (deviceType)
			{
				case InputDeviceType.Keyboard:
					usageFlag = (ushort)HIDUsage.Keyboard;
					flags = exclusive ? RawInputDeviceFlags.NoLegacy : RawInputDeviceFlags.None;
					break;
				case InputDeviceType.Mouse:
					// Unlike the keyboard, we only set a flag here.
					// This is because changing exclusivity on the mouse during a mouse down, followed by a mouse up event 
					// causes raw input to behave erratically (for me, this includes not being able to click on my window 
					// caption bar and drag the window until I click multiple times).  With this flag set, we will manually 
					// kill any legacy mouse messages and that will "fake" an exclusive mode.
					_isMouseExclusive = exclusive;
					return exclusive;
			}

			bool result = false;

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
				result = exclusive;
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
			_wndProcRef = WndProc;
			_newWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcRef);

			if (Win32API.SetWindowLong(new HandleRef(this, _windowHandle), WindowLongType.WndProc, _newWndProc) == IntPtr.Zero)
			{
				throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_HOOK_RAWINPUT_MSG, Marshal.GetLastWin32Error()));
			}

			RegisterRawInputDevices();
		}
		#endregion

		#region Constructors/Finalizers
		/// <summary>
		/// Initializes a new instance of the <see cref="RawInputNative"/> class.
		/// </summary>
		/// <param name="windowHandle">The window handle to hook.</param>
		/// <param name="deviceHandles">A list of device handles and device object GUIDs.</param>
		/// <param name="rawInputProcessors">The list of input processors.</param>
		/// <param name="registeredDevices">The list of registered devices.</param>
		public RawInputNative(IntPtr windowHandle,
		                      IReadOnlyList<Tuple<Guid, IntPtr>> deviceHandles,
		                      IReadOnlyDictionary<Control, RawInputProcessor> rawInputProcessors,
		                      IReadOnlyDictionary<Guid, IGorgonInputDevice> registeredDevices)
		{
			_windowHandle = windowHandle;
			_deviceHandles = deviceHandles;
			_rawInputProcessors = rawInputProcessors;
			_registeredDevices = registeredDevices;
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="RawInputNative"/> class.
		/// </summary>
		~RawInputNative()
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
