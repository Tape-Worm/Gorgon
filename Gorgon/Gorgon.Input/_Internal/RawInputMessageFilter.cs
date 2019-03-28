#region MIT
// 
// Gorgon
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
// Created: Tuesday, September 08, 2015 12:31:48 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Input;

namespace Gorgon.Native
{
	/// <summary>
	/// Provides a window hook in order to filter raw input messages.
	/// </summary>
	internal class RawInputMessageFilter
		: IDisposable, IMessageFilter
	{
        #region Constants.
        // Mouse messages.
#pragma warning disable IDE0051 // Remove unused private members
        private const int WM_XBUTTONDBLCLK = 0x020D;
	    private const int WM_XBUTTONDOWN = 0x020B;
		private const int WM_XBUTTONUP = 0x020C;
		private const int WM_RBUTTONDBLCLK = 0x0206;
		private const int WM_RBUTTONDOWN = 0x0204;
		private const int WM_RBUTTONUP = 0x0205;
		private const int WM_NCRBUTTONDBLCLK = 0x00A6;
		private const int WM_NCRBUTTONDOWN = 0x00A4;
		private const int WM_NCRBUTTONUP = 0x00A5;
		private const int WM_NCXBUTTONDBLCLK = 0x00AD;
		private const int WM_NCXBUTTONDOWN = 0x00AB;
		private const int WM_NCXBUTTONUP = 0x00AC;
		private const int WM_NCMOUSEHOVER = 0x02A0;
		private const int WM_NCMOUSELEAVE = 0x02A2;
		private const int WM_NCMOUSEMOVE = 0x00A0;
		private const int WM_NCLBUTTONDBLCLK = 0x00A3;
		private const int WM_NCLBUTTONDOWN = 0x00A1;
		private const int WM_NCLBUTTONUP = 0x00A2;
		private const int WM_NCMBUTTONDBLCLK = 0x00A9;
		private const int WM_NCMBUTTONDOWN = 0x00A7;
		private const int WM_NCMBUTTONUP = 0x00A8;
		private const int WM_MOUSELEAVE = 0x02A3;
		private const int WM_MOUSEMOVE = 0x0200;
		private const int WM_MOUSEWHEEL = 0x020A;
		private const int WM_MOUSEHWHEEL = 0x020E;
		private const int WM_MOUSEHOVER = 0x02A1;
		private const int WM_MOUSEACTIVATE = 0x0021;
		private const int WM_LBUTTONDBLCLK = 0x0203;
		private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_MBUTTONDBLCLK = 0x0209;
		private const int WM_MBUTTONDOWN = 0x0207;
		private const int WM_MBUTTONUP = 0x0208;
		private const int WM_CAPTURECHANGED = 0x0215;
#pragma warning restore IDE0051 // Remove unused private members
        #endregion

        #region Variables.
        // Flag to indicate that the message hook has been installed.
        private static int _hookInstalled;
		// Flag to indicate that we've directly hooked the windows message procedure.
		private bool _directHook;
		// The window handle to hook into.
		private static IntPtr _hwnd = IntPtr.Zero;
	    // The devices that are registered with the raw input provider.
	    private readonly Dictionary<DeviceKey, IRawInputDeviceData<GorgonRawKeyboardData>> _keyboardDevices;
	    // The devices that are registered with the raw input provider.
	    private readonly Dictionary<DeviceKey, IRawInputDeviceData<GorgonRawMouseData>> _mouseDevices;
		// The devices that are registered with the raw input provider.
		private readonly Dictionary<DeviceKey, IRawInputDeviceData<GorgonRawHIDData>> _hidDevices;
		#endregion

		#region Properties.

		/// <summary>
		/// Property to indicate that the mouse will not receive legacy messages.
		/// </summary>
		public static bool NoLegacyMouse
		{
			get;
			set;
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function to retrieve a raw input HID.
        /// </summary>
        /// <param name="key">The key for the raw input device.</param>
        /// <param name="deviceHandle">The device handle.</param>
        /// <returns></returns>
	    private IRawInputDeviceData<GorgonRawHIDData> GetRawInputHid(DeviceKey key, IntPtr deviceHandle)
	    {
	        if (_hidDevices.TryGetValue(key, out IRawInputDeviceData<GorgonRawHIDData> result))
	        {
	            return result;
	        }

	        key.DeviceHandle = deviceHandle;

	        return !_hidDevices.TryGetValue(key, out result) ? null : result;
	    }

	    /// <summary>
	    /// Function to retrieve a raw input mouse.
	    /// </summary>
	    /// <param name="key">The key for the raw input device.</param>
	    /// <param name="deviceHandle">The device handle.</param>
	    /// <returns></returns>
	    private IRawInputDeviceData<GorgonRawMouseData> GetRawInputMouseDevice(DeviceKey key, IntPtr deviceHandle)
	    {
	        if (_mouseDevices.TryGetValue(key, out IRawInputDeviceData<GorgonRawMouseData> result))
	        {
	            return result;
	        }

	        key.DeviceHandle = deviceHandle;

	        return !_mouseDevices.TryGetValue(key, out result) ? null : result;
	    }

	    /// <summary>
	    /// Function to retrieve a raw input keyboard.
	    /// </summary>
	    /// <param name="key">The key for the raw input device.</param>
	    /// <param name="deviceHandle">The device handle.</param>
	    /// <returns></returns>
	    private IRawInputDeviceData<GorgonRawKeyboardData> GetRawInputKeyboardDevice(DeviceKey key, IntPtr deviceHandle)
	    {
	        if (_keyboardDevices.TryGetValue(key, out IRawInputDeviceData<GorgonRawKeyboardData> result))
	        {
	            return result;
	        }

	        key.DeviceHandle = deviceHandle;

	        return !_keyboardDevices.TryGetValue(key, out result) ? null : result;
	    }

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (!_directHook)
			{
				Application.RemoveMessageFilter(this);
			}
			else
			{
				if (_hwnd != IntPtr.Zero)
				{
					MessageFilterHook.RemoveFilter(_hwnd, this);
				}
			}

			_directHook = false;

			if (Interlocked.Decrement(ref _hookInstalled) == 0)
			{
				_hwnd = IntPtr.Zero;
			}

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Function to provide filtering of window messages for the application.
		/// </summary>
		/// <param name="m">Window message to filter.</param>
		/// <returns><b>true</b> if the message is processed by this method, or <b>false</b> if not.</returns>
		public bool PreFilterMessage(ref Message m)
		{
			if ((m.Msg != RawInputApi.WmRawInput) || (m.HWnd != _hwnd))
			{
				if (!NoLegacyMouse)
				{
					return false;
				}

				// If we have no legacy messages turned on for the mouse, then do not process them.
				switch (m.Msg)
				{
					/*case WM_XBUTTONDBLCLK:
					case WM_XBUTTONDOWN:
					case WM_XBUTTONUP:
					case WM_RBUTTONDBLCLK:
					case WM_RBUTTONDOWN:*/
					case WM_RBUTTONUP:
					/*case WM_NCRBUTTONDBLCLK:
					case WM_NCRBUTTONDOWN:
					case WM_NCRBUTTONUP:
					case WM_NCXBUTTONDBLCLK:
					case WM_NCXBUTTONDOWN:
					case WM_NCXBUTTONUP:
					case WM_NCMOUSEHOVER:
					case WM_NCMOUSELEAVE:
					case WM_NCMOUSEMOVE:
					case WM_NCLBUTTONDBLCLK:
					case WM_NCLBUTTONDOWN:
					case WM_NCLBUTTONUP:
					case WM_NCMBUTTONDBLCLK:
					case WM_NCMBUTTONDOWN:
					case WM_NCMBUTTONUP:
					case WM_MOUSELEAVE:
					case WM_MOUSEMOVE:
					case WM_MOUSEWHEEL:
					case WM_MOUSEHWHEEL:
					case WM_MOUSEHOVER:
					case WM_MOUSEACTIVATE:
					case WM_LBUTTONDBLCLK:
					case WM_LBUTTONDOWN:
					case WM_LBUTTONUP:
					case WM_MBUTTONDBLCLK:
					case WM_MBUTTONDOWN:
					case WM_MBUTTONUP:
					case WM_CAPTURECHANGED:*/
						return true;
					default:
						return false;
				}
			}

			RAWINPUT data = RawInputApi.GetRawInputData(m.LParam);
			var key = new DeviceKey
			{
				DeviceType = data.Header.Type,
				DeviceHandle = IntPtr.Zero
			};

			switch (data.Header.Type)
			{
				case RawInputType.Keyboard:
				    IRawInputDeviceData<GorgonRawKeyboardData> keyboard = GetRawInputKeyboardDevice(key, data.Header.Device);
                    
				    if (keyboard == null)
				    {
				        return false;
				    }

					RawInputDispatcher.Dispatch(keyboard, ref data.Union.Keyboard);
					break;
				case RawInputType.Mouse:
				    IRawInputDeviceData<GorgonRawMouseData> mouse = GetRawInputMouseDevice(key, data.Header.Device);

				    if (mouse == null)
				    {
				        return false;
				    }

					RawInputDispatcher.Dispatch(mouse, ref data.Union.Mouse);
					break;
				case RawInputType.HID:
				    IRawInputDeviceData<GorgonRawHIDData> hid = GetRawInputHid(key, data.Header.Device);

				    if (hid == null)
				    {
				        return false;
				    }

					RawInputDispatcher.Dispatch(hid, ref data.Union.HID);
					break;
			}

			return true;
		}

		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="RawInputMessageFilter"/> class.
		/// </summary>
		/// <param name="keyboardDevices">The keyboard devices that are registered with the raw input provider.</param>
		/// <param name="pointingDevices">The pointing devices that are registered with the raw input provider.</param>
		/// <param name="hidDevices">The HID devices that are registered with the raw input provider.</param>
		/// <param name="hwnd">The window handle to the main application window.</param>
		/// <param name="hookDirectly"><b>true</b> to hook the windows message procedure directly, or <b>false</b> to install a WinForms message filter.</param>
		/// <exception cref="System.ComponentModel.Win32Exception">Thrown when the message hook could be applied to the window.</exception>
		public RawInputMessageFilter(
		    Dictionary<DeviceKey, IRawInputDeviceData<GorgonRawKeyboardData>> keyboardDevices,
		    Dictionary<DeviceKey, IRawInputDeviceData<GorgonRawMouseData>> pointingDevices,
		    Dictionary<DeviceKey, IRawInputDeviceData<GorgonRawHIDData>> hidDevices, IntPtr hwnd, bool hookDirectly)
		{
		    _keyboardDevices = keyboardDevices;
		    _mouseDevices = pointingDevices;
			_hidDevices = hidDevices;

			if ((Interlocked.Increment(ref _hookInstalled) == 1) && (_hwnd == IntPtr.Zero))
			{
			    _hwnd = hwnd;
			}
			
			_directHook = hookDirectly;

			// Add a message filter to allow for processing of raw input.
			if (!hookDirectly)
			{
				Application.AddMessageFilter(this);
			}
			else
			{
			    MessageFilterHook.AddFilter(hwnd, this);	    
			}
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="RawInputMessageFilter"/> class.
		/// </summary>
		~RawInputMessageFilter()
		{
			Dispose();
		}
		#endregion
	}
}
