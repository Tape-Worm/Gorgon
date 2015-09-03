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
using Gorgon.Input.Raw.Properties;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// Hooks a window procedure with a custom message filter.
	/// </summary>
	class RawInputMessageHandler
		: IDisposable
	{
		#region Delegates.
		/// <summary>
		/// Delegate for our custom window proc.
		/// </summary>
		/// <param name="hWnd">Window handle.</param>
		/// <param name="msg">Message.</param>
		/// <param name="wParam">Parameter</param>
		/// <param name="lParam">Parameter.</param>
		/// <returns>Result.</returns>
		private delegate IntPtr WndProcHandler(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
		#endregion

		#region Constants.
		// Retrieves a window procedure.
		private const int WindowLongWndProc = -4;
		// Mouse move message.
		private const int WmMouseMove = 0x0200;
		// Left mouse button down message.
		private const int WmLeftButtonDown = 0x0201;
		// Left mouse button up message.
		private const int WmLeftButtonUp = 0x0202;
		// Left mouse button double click message.
        private const int WmLeftButtonDoubleClick = 0x0203;
		// Right mouse button down message.
		private const int WmRightButtonDown = 0x0204;
		// Right mouse button up message.
		private const int WmRightButtonUp = 0x0205;
		// Right mouse button double click message.
		private const int WmRightButtonDoubleClick = 0x0206;
		// Middle mouse button down message.
		private const int WmMiddleButtonDown = 0x0207;
		// Middle mouse button up message.
		private const int WmMiddleButtonUp = 0x0208;
		// Middle mouse button double click message.
		private const int WmMiddleButtonDoubleClick = 0x0209;
		// Mouse wheel move message.
		private const int WmMouseWheel = 0x020A;
		// X button down message.
		private const int WmXButtonDown = 0x20B;
		// X button up message.
		private const int WmXButtonUp = 0x20C;
		// X button double click message.
		private const int WmXButtonDoubleClick = 0x20D;
		// Horizontal mouse wheel move message.
		private const int WmMouseHWheel = 0x20E;
		// Mouse hover message.
		private const int WmMouseHover = 0x02A1;
		// Mouse leave message.
		private const int WmMouseLeave = 0x02A3;
		// Raw input message.
		private const int WmRawInput = 0x00FF;
		#endregion

		#region Variables.
		// Previous window procedure.
		private IntPtr _oldWndProc;
		// New window procedure.
		private IntPtr _newWndProc;
		// The window handle to hook.
		private IntPtr _windowHandle;
		// Reference to the new window procedure delegate. This will keep the proc from being GC'd.
		private WndProcHandler _wndProcRef;
		// A list of devices registered with the service.
		private readonly IReadOnlyList<Tuple<Guid, IntPtr>> _deviceHandles;
		// The raw input processor for device data.
		private readonly IReadOnlyDictionary<Control, RawInputProcessor> _rawInputProcessors;
		// A list of registered devices.
		private readonly IReadOnlyDictionary<Guid, IGorgonInputDevice> _registeredDevices;
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
		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
		{
			if (_oldWndProc == IntPtr.Zero)
			{
				// This shouldn't happen, but just in case we forgot to clean up or something, or the 
				// finalizer didn't run, then ensure that this method is dead for sure.
				throw new ObjectDisposedException(Resources.GORINP_RAW_HOOK_STILL_ACTIVE);
			}
			
			if ((msg != WmRawInput) || (hwnd != _windowHandle))
			{
				if (!RawInputApi.IsMouseExclusive)
				{
					return CallPreviousWndProc(hwnd, msg, wParam, lParam);
				}

				switch (msg)
				{
					case WmXButtonDoubleClick:
					case WmLeftButtonDoubleClick:
					case WmRightButtonDoubleClick:
					case WmMiddleButtonDoubleClick:
					case WmLeftButtonDown:
					case WmLeftButtonUp:
					case WmRightButtonDown:
					case WmRightButtonUp:
					case WmMiddleButtonDown:
					case WmMiddleButtonUp:
					case WmXButtonDown:
					case WmXButtonUp:
					case WmMouseMove:
					case WmMouseWheel:
					case WmMouseHWheel:
					case WmMouseHover:
					case WmMouseLeave:
						return IntPtr.Zero;
				}

				return CallPreviousWndProc(hwnd, msg, wParam, lParam);
			}

			// Get the device data from the raw input system.
			RAWINPUT data = RawInputApi.GetRawInputData(lParam);

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
						var keyboardDevice = device as IGorgonInputEventDrivenDevice<GorgonKeyboardData>;

						// There's no processor on this window, so we can't send anything.
						if ((keyboardDevice == null) || (!_rawInputProcessors.TryGetValue(device.Window, out processor)))
						{
							continue;
						}

						processor.ProcessRawInputMessage(keyboardDevice, ref data.Union.Keyboard);
						break;
					case RawInputType.Mouse:
						var mouseDevice = device as IGorgonInputEventDrivenDevice<GorgonMouseData>;

						// There's no processor on this window, so we can't send anything.
						if ((mouseDevice == null) || (!_rawInputProcessors.TryGetValue(device.Window, out processor)))
						{
							continue;
						}

						processor.ProcessRawInputMessage(mouseDevice, ref data.Union.Mouse);
						break;
					case RawInputType.HID:
						GorgonJoystick2 joystick = device as GorgonJoystick2;
						
						if ((joystick == null) || (!_rawInputProcessors.TryGetValue(device.Window, out processor)))
						{
							continue;
						}

						processor.ProcessRawInputMessage(joystick, ref data.Union.HID);
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
			RawInputApi.UnregisterRawInputDevices();

			if ((_windowHandle == IntPtr.Zero)
				|| (_newWndProc == IntPtr.Zero)
				|| (_oldWndProc == IntPtr.Zero))
			{
				return;
			}

			IntPtr currentWndProc = UserApi.GetWindowLong(new HandleRef(this, _windowHandle), WindowLongWndProc);

			// We only unhook our own procedure, if someone else hooks after us, and does not clean up, then there's nothing 
			// we can do, and rather than bring down a mess of procedures inappropriately, it's best if we just leave it be.
			// It may still be active on the window, but for all intents and purposes, it should be harmless and cause a tiny 
			// bit of unnecessary overhead in the worst case.
			if (currentWndProc == _newWndProc)
			{
				UserApi.SetWindowLong(new HandleRef(this, _windowHandle), WindowLongWndProc, _oldWndProc);
			}
			
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
		private IntPtr CallPreviousWndProc(IntPtr handle, int message, IntPtr wParam, IntPtr lParam)
		{
			return _oldWndProc == IntPtr.Zero ? IntPtr.Zero : UserApi.CallWindowProc(_oldWndProc, handle, message, wParam, lParam);
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
			_oldWndProc = UserApi.GetWindowLong(new HandleRef(this, _windowHandle), WindowLongWndProc);
			_wndProcRef = WndProc;
			_newWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcRef);

			if (UserApi.SetWindowLong(new HandleRef(this, _windowHandle), WindowLongWndProc, _newWndProc) == IntPtr.Zero)
			{
				throw new Win32Exception(Resources.GORINP_RAW_ERR_CANNOT_HOOK_RAWINPUT_MSG);
			}

			RawInputApi.RegisterRawInputDevices(_windowHandle);
		}
		#endregion

		#region Constructors/Finalizers
		/// <summary>
		/// Initializes a new instance of the <see cref="RawInputMessageHandler"/> class.
		/// </summary>
		/// <param name="windowHandle">The window handle to hook.</param>
		/// <param name="deviceHandles">A list of device handles and device object GUIDs.</param>
		/// <param name="rawInputProcessors">The list of input processors.</param>
		/// <param name="registeredDevices">The list of registered devices.</param>
		public RawInputMessageHandler(IntPtr windowHandle,
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
		/// Initializes static members of the <see cref="RawInputMessageHandler"/> class.
		/// </summary>
		static RawInputMessageHandler()
		{
			Marshal.PrelinkAll(typeof(RawInputMessageHandler));
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="RawInputMessageHandler"/> class.
		/// </summary>
		~RawInputMessageHandler()
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
