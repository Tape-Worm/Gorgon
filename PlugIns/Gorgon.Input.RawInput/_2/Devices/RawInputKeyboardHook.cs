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
// Created: Thursday, July 16, 2015 10:23:36 PM
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Input.Raw.Properties;
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// The raw input keyboard hook used to capture and translate raw input messages.
	/// </summary>
	class RawInputKeyboardHook
		: IDisposable
	{
		#region Properties.
		/// <summary>
		/// Property to return the window handle that was registered with raw input.
		/// </summary>
		public IntPtr WindowHandle
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of keyboards using this hook.
		/// </summary>
		public int Keyboards
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function unregister the raw input keyboard data from the specified window.
		/// </summary>
		private void UnregisterRawInput()
		{
			if (WindowHandle == IntPtr.Zero)
			{
				return;
			}

			Keyboards = 0;
			WindowHandle = IntPtr.Zero;

			var rawDevice = new RAWINPUTDEVICE
			                {
				                Usage = (ushort)HIDUsage.Keyboard,
				                UsagePage = HIDUsagePage.Generic,
				                Flags = RawInputDeviceFlags.Remove,
				                WindowHandle = IntPtr.Zero
			                };

			// Attempt to register the device.
			if (!Win32API.RegisterRawInputDevices(new[]
			                                      {
				                                      rawDevice
			                                      },
			                                      1,
			                                      DirectAccess.SizeOf<RAWINPUTDEVICE>()))
			{
				throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_ERR_CANNOT_UNBIND_KEYBOARD);
			}
		}

		/// <summary>
		/// Function to process a raw input input message and forward it to the correct device.
		/// </summary>
		/// <param name="keyboardData">The raw input keyboard data.</param>
		/// <param name="processedData">The gorgon keyboard data processed from the raw input data.</param>
		/// <returns><b>true</b> if the message was processed, <b>false</b> if not.</returns>
		public bool ProcessRawInputMessage(ref RAWINPUTKEYBOARD keyboardData, out GorgonKeyboardData processedData)
		{
			KeyboardDataFlags flags = KeyboardDataFlags.KeyDown;

			if ((keyboardData.Flags & RawKeyboardFlags.KeyBreak) == RawKeyboardFlags.KeyBreak)
			{
				flags = KeyboardDataFlags.KeyUp;
			}

			if ((keyboardData.Flags & RawKeyboardFlags.KeyE0) == RawKeyboardFlags.KeyE0)
			{
				flags |= KeyboardDataFlags.LeftKey;
			}

			if ((keyboardData.Flags & RawKeyboardFlags.KeyE1) == RawKeyboardFlags.KeyE1)
			{
				flags |= KeyboardDataFlags.RightKey;
			}

			// Shift has to be handled in a special case since it doesn't actually detect left/right from raw input.
			if (keyboardData.VirtualKey == VirtualKeys.Shift)
			{
				flags |= keyboardData.MakeCode == 0x36 ? KeyboardDataFlags.RightKey : KeyboardDataFlags.LeftKey;
			}

			processedData = new GorgonKeyboardData
			{
				ScanCode = keyboardData.MakeCode,
				Key = (Keys)keyboardData.VirtualKey,
				Flags = flags
			};

			return true;
		}

		/// <summary>
		/// Function to register a keyboard hook with this window.
		/// </summary>
		/// <param name="exclusive"><b>true</b> to exclusively capture keyboard data, <b>false</b> to allow all messages to go through.</param>
		public void Register(bool exclusive)
		{
			// We've already registered at least one keyboard with this handler, so no need to do it again.
			if ((Keyboards++) > 0)
			{
				return;
			}

			var rawDevice = new RAWINPUTDEVICE
			{
				Usage = (ushort)HIDUsage.Keyboard,
				UsagePage = HIDUsagePage.Generic,
				Flags = exclusive ? RawInputDeviceFlags.NoLegacy : RawInputDeviceFlags.None,
				WindowHandle = WindowHandle
			};

			if (!Win32API.RegisterRawInputDevices(new[]
			                                      {
				                                      rawDevice
			                                      },
			                                      1,
			                                      DirectAccess.SizeOf<RAWINPUTDEVICE>()))
			{
				throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_ERR_CANNOT_BIND_KEYBOARD);
			}
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawInputKeyboardHook"/> class.
		/// </summary>
		/// <param name="windowHandle">The window handle to hook up with the raw input system.</param>
		public RawInputKeyboardHook(IntPtr windowHandle)
		{
			WindowHandle = windowHandle;
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="RawInputKeyboardHook"/> class.
		/// </summary>
		~RawInputKeyboardHook()
		{
			UnregisterRawInput();
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Keyboards--;

			// If the number of keyboards relying on this hook is larger than 0, then do not unhook the handler.
			if (Keyboards.Max(0) > 0)
			{
				return;
			}

			UnregisterRawInput();
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
