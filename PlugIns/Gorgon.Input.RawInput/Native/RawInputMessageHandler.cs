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
using System.Diagnostics.CodeAnalysis;
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
		private delegate IntPtr WndProcHandler(IntPtr hWnd, WindowMessages msg, IntPtr wParam, IntPtr lParam);
		#endregion

		#region Enums.
		/// <summary>
		/// Types of messages that passed to a window.
		/// </summary>
		/// <remarks>See the MSDN documentation for more detail.</remarks>
		private enum WindowMessages
		{
			/// <summary></summary>
			MouseMove = 0x0200,
			/// <summary></summary>
			LeftButtonDown = 0x0201,
			/// <summary></summary>
			LeftButtonUp = 0x0202,
			/// <summary></summary>
			LeftButtonDoubleClick = 0x0203,
			/// <summary></summary>
			RightButtonDown = 0x0204,
			/// <summary></summary>
			RightButtonUp = 0x0205,
			/// <summary></summary>
			RightButtonDoubleClick = 0x0206,
			/// <summary></summary>
			MiddleButtonDown = 0x0207,
			/// <summary></summary>
			MiddleButtonUp = 0x0208,
			/// <summary></summary>
			MiddleButtonDoubleClick = 0x0209,
			/// <summary></summary>
			MouseWheel = 0x020A,
			/// <summary>
			/// 
			/// </summary>
			XButtonDown = 0x20B,
			/// <summary>
			/// 
			/// </summary>
			XButtonUp = 0x20C,
			/// <summary>
			/// 
			/// </summary>
			XButtonDoubleClick = 0x20D,
			/// <summary>
			/// 
			/// </summary>
			MouseHWheel = 0x20E,
			/// <summary></summary>
			MouseHover = 0x02A1,
			/// <summary></summary>
			MouseLeave = 0x02A3,
			/// <summary></summary>
			RawInput = 0x00FF
		}
		#endregion

		#region Constants.
		/// <summary>
		/// Retrieves a window procedure.
		/// </summary>
		private int WindowLongWndProc = -4;
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
		/// Function to retrieve information about the specified window.
		/// </summary>
		/// <param name="hwnd">Window handle to retrieve information from.</param>
		/// <param name="index">Type of information.</param>
		/// <returns>A pointer to the information.</returns>
		[SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist", Justification = "Really now?  You couldn't check the ENTRYPOINT ATTRIBUTE!?!")]
		[SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return", Justification = "Not visible outside of assembly. Call platform is determined at runtime.")]
		[DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Unicode)]
		private static extern IntPtr GetWindowLongx86(HandleRef hwnd, int index);

		/// <summary>
		/// Function to retrieve information about the specified window.
		/// </summary>
		/// <param name="hwnd">Window handle to retrieve information from.</param>
		/// <param name="index">Type of information.</param>
		/// <returns>A pointer to the information.</returns>
		[SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist", Justification = "Really now?  You couldn't check the ENTRYPOINT ATTRIBUTE!?!")]
		[SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return", Justification = "Not visible outside of assembly. Call platform is determined at runtime.")]
		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Unicode)]
		private static extern IntPtr GetWindowLongx64(HandleRef hwnd, int index);

		/// <summary>
		/// Function to set information for the specified window.
		/// </summary>
		/// <param name="hwnd">Window handle to set information on.</param>
		/// <param name="index">Type of information.</param>
		/// <param name="info">Information to set.</param>
		/// <returns>A pointer to the previous information, or 0 if not successful.</returns>
		[SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "2", Justification = "Not visible outside of assembly. Call platform is determined at runtime.")]
		[SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist", Justification = "Really now?  You couldn't check the ENTRYPOINT ATTRIBUTE!?!")]
		[SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return", Justification = "Not visible outside of assembly. Call platform is determined at runtime.")]
		[DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Unicode)]
		private static extern IntPtr SetWindowLongx86(HandleRef hwnd, int index, IntPtr info);

		/// <summary>
		/// Function to set information for the specified window.
		/// </summary>
		/// <param name="hwnd">Window handle to set information on.</param>
		/// <param name="index">Type of information.</param>
		/// <param name="info">Information to set.</param>
		/// <returns>A pointer to the previous information, or 0 if not successful.</returns>
		[SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "2", Justification = "Not visible outside of assembly. Call platform is determined at runtime.")]
		[SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist", Justification = "Really now?  You couldn't check the ENTRYPOINT ATTRIBUTE!?!")]
		[SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return", Justification = "Not visible outside of assembly. Call platform is determined at runtime.")]
		[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Unicode)]
		private static extern IntPtr SetWindowLongx64(HandleRef hwnd, int index, IntPtr info);

		/// <summary>
		/// Function to call a window procedure.
		/// </summary>
		/// <param name="wndProc">Pointer to the window procedure function to call.</param>
		/// <param name="hwnd">Window handle to use.</param>
		/// <param name="msg">Message to send.</param>
		/// <param name="wParam">Parameter for the message.</param>
		/// <param name="lParam">Parameter for the message.</param>
		/// <returns>The return value specifies the result of the message processing and depends on the message sent.</returns>
		[DllImport("user32.dll", EntryPoint = "CallWindowProc", CharSet = CharSet.Unicode)]
		private static extern IntPtr CallWindowProc(IntPtr wndProc, IntPtr hwnd, WindowMessages msg, IntPtr wParam, IntPtr lParam);

		/// <summary>
		/// Function to retrieve information about the specified window.
		/// </summary>
		/// <param name="hwnd">Window handle to retrieve information from.</param>
		/// <param name="index">Type of information.</param>
		/// <returns>A pointer to the information.</returns>
		private static IntPtr GetWindowLong(HandleRef hwnd, int index)
		{
			return IntPtr.Size == 4 ? GetWindowLongx86(hwnd, index) : GetWindowLongx64(hwnd, index);
		}

		/// <summary>
		/// Function to set information for the specified window.
		/// </summary>
		/// <param name="hwnd">Window handle to set information on.</param>
		/// <param name="index">Type of information.</param>
		/// <param name="info">Information to set.</param>
		/// <returns>A pointer to the previous information, or 0 if not successful.</returns>
		private static IntPtr SetWindowLong(HandleRef hwnd, int index, IntPtr info)
		{
			return IntPtr.Size == 4 ? SetWindowLongx86(hwnd, index, info) : SetWindowLongx64(hwnd, index, info);
		}

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
				if (!RawInputApi.IsMouseExclusive)
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
			RawInputApi.UnregisterRawInputDevices();

			if ((_windowHandle == IntPtr.Zero)
				|| (_newWndProc == IntPtr.Zero)
				|| (_oldWndProc == IntPtr.Zero))
			{
				return;
			}

			IntPtr currentWndProc = GetWindowLong(new HandleRef(this, _windowHandle), WindowLongWndProc);

			// We only unhook our own procedure, if someone else hooks after us, and does not clean up, then there's nothing 
			// we can do, and rather than bring down a mess of procedures inappropriately, it's best if we just leave it be.
			// It may still be active on the window, but for all intents and purposes, it should be harmless and cause a tiny 
			// bit of unnecessary overhead in the worst case.
			if (currentWndProc == _newWndProc)
			{
				SetWindowLong(new HandleRef(this, _windowHandle), WindowLongWndProc, _oldWndProc);
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
		private IntPtr CallPreviousWndProc(IntPtr handle, WindowMessages message, IntPtr wParam, IntPtr lParam)
		{
			return _oldWndProc == IntPtr.Zero ? IntPtr.Zero : CallWindowProc(_oldWndProc, handle, message, wParam, lParam);
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
			_oldWndProc = GetWindowLong(new HandleRef(this, _windowHandle), WindowLongWndProc);
			_wndProcRef = WndProc;
			_newWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcRef);

			if (SetWindowLong(new HandleRef(this, _windowHandle), WindowLongWndProc, _newWndProc) == IntPtr.Zero)
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
