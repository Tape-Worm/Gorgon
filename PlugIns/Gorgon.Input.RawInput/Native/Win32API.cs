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
// Created: Friday, June 24, 2011 10:04:54 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;

namespace Gorgon.Native
{
	/// <summary>
	/// Static class for Native Win32 methods and corresponding structures.
	/// </summary>
	/// <remarks>
	/// This is a grouping of any API calls regularly used by Gorgon.
	/// <para>
	/// This list is by no means complete.  The Win32 API is just massive and would probably take a lifetime to map.
	/// </para>
	/// 	<para>
	/// These calls are considered "unsafe" and thus should be used with care.  If you don't know how to use a function, or why you want it, you probably don't need to use them.
	/// </para>
	/// 	<para>
	/// Please note that a lot of the enumerators/structures have slightly different names than their Win32 counterparts.  This was done for the sake of readability.  This does NOT affect their results or their effect on the results of their related functionality.
	/// </para>
	/// </remarks>
	[SuppressUnmanagedCodeSecurity]
	internal static class Win32API
	{
		#region Methods.
		/// <summary>
		/// Function to retrieve the dead zone of the joystick.
		/// </summary>
		/// <param name="uJoyID">ID the joystick to query.</param>
		/// <param name="puThreshold">Threshold for the joystick.</param>
		/// <returns>0 if successful, non-zero if not.</returns>
		[DllImport("WinMM.dll")]
		public static extern int joyGetThreshold(int uJoyID, out int puThreshold);

		/// <summary>
		/// Function to signal that the joystick configuration has changed.
		/// </summary>
		/// <param name="doNotUse">Do not use.</param>
		/// <returns>0 if successful, non-zero if not.</returns>
		[DllImport("WinMM.dll")]
		public static extern int joyConfigChanged(int doNotUse);

		/// <summary>
		/// Function to return the number of joystick devices supported by the current driver.
		/// </summary>
		/// <returns>Number of joysticks supported, 0 if no driver is installed.</returns>
		[DllImport("WinMM.dll")]
		public static extern int joyGetNumDevs();

		/// <summary>
		/// Function to return the joystick device capabilities.
		/// </summary>
		/// <param name="uJoyID">ID of the joystick to return.  -1 will return registry key, whether a device exists or not.</param>
		/// <param name="pjc">Joystick capabilities.</param>
		/// <param name="cbjc">Size of the JOYCAPS structure in bytes.</param>
		/// <returns>0 if successful, non zero if not.</returns>
		[DllImport("WinMM.dll", CharSet = CharSet.Ansi)]
		public static extern int joyGetDevCaps(int uJoyID, ref JOYCAPS pjc, int cbjc);

		/// <summary>
		/// Function to retrieve joystick position information.
		/// </summary>
		/// <param name="uJoyID">ID the of joystick to query.</param>
		/// <param name="pji">Position information.</param>
		/// <returns>0 if successful, non zero if not.  JOYERR_UNPLUGGED if not connected.</returns>
		[DllImport("WinMM.dll", CharSet = CharSet.Ansi)]
		public static extern int joyGetPos(int uJoyID, ref JOYINFO pji);

		/// <summary>
		/// Function to retrieve joystick position information.
		/// </summary>
		/// <param name="uJoyID">ID the of joystick to query.</param>
		/// <param name="pji">Position information.</param>
		/// <returns>0 if successful, non zero if not.  JOYERR_UNPLUGGED if not connected.</returns>
		[DllImport("WinMM.dll", CharSet = CharSet.Ansi)]
		public static extern int joyGetPosEx(int uJoyID, ref JOYINFOEX pji);

		/// <summary>
		/// Function to retrieve raw input data.
		/// </summary>
		/// <param name="hRawInput">Handle to the raw input.</param>
		/// <param name="uiCommand">Command to issue when retrieving data.</param>
		/// <param name="pData">Raw input data.</param>
		/// <param name="pcbSize">Number of bytes in the array.</param>
		/// <param name="cbSizeHeader">Size of the header.</param>
		/// <returns>0 if successful if pData is null, otherwise number of bytes if pData is not null.</returns>
		[DllImport("user32.dll")]
		public static extern int GetRawInputData(IntPtr hRawInput, RawInputCommand uiCommand, IntPtr pData, ref int pcbSize, int cbSizeHeader);

		/// <summary>
		/// Function to enumerate raw input devices.
		/// </summary>
		/// <param name="pRawInputDeviceList">List of device handles.</param>
		/// <param name="puiNumDevices">Number of devices returned.</param>
		/// <param name="cbSize">Size of the raw input device struct.</param>
		/// <returns>0 if successful, otherwise an error code.</returns>
		[DllImport("user32.dll")]
		private static extern int GetRawInputDeviceList(IntPtr pRawInputDeviceList, ref int puiNumDevices, int cbSize);

		/// <summary>
		/// Function to retrieve information about a raw input device.
		/// </summary>
		/// <param name="hDevice">Handle to the device.</param>
		/// <param name="uiCommand">Type of information to return.</param>
		/// <param name="pData">Data returned.</param>
		/// <param name="pcbSize">Size of the data to return.</param>
		/// <returns>0 if successful, otherwise an error code.</returns>
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern int GetRawInputDeviceInfo(IntPtr hDevice, RawInputCommand uiCommand, IntPtr pData, ref int pcbSize);

		/// <summary>
		/// Function to register a raw input device.
		/// </summary>
		/// <param name="pRawInputDevices">Array of raw input devices.</param>
		/// <param name="uiNumDevices">Number of devices.</param>
		/// <param name="cbSize">Size of the RAWINPUTDEVICE structure.</param>
		/// <returns><c>true</c> if successful, <c>false</c> if not.</returns>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool RegisterRawInputDevices([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] RAWINPUTDEVICE[] pRawInputDevices, int uiNumDevices, int cbSize);

        /// <summary>
        /// Function to retrieve information about the specified window.
        /// </summary>
        /// <param name="hwnd">Window handle to retrieve information from.</param>
        /// <param name="index">Type of information.</param>
        /// <returns>A pointer to the information.</returns>
        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Unicode)]
	    private static extern IntPtr GetWindowLongx86(HandleRef hwnd, WindowLongType index);

        /// <summary>
        /// Function to retrieve information about the specified window.
        /// </summary>
        /// <param name="hwnd">Window handle to retrieve information from.</param>
        /// <param name="index">Type of information.</param>
        /// <returns>A pointer to the information.</returns>
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetWindowLongx64(HandleRef hwnd, WindowLongType index);

        /// <summary>
        /// Function to set information for the specified window.
        /// </summary>
        /// <param name="hwnd">Window handle to set information on.</param>
        /// <param name="index">Type of information.</param>
        /// <param name="info">Information to set.</param>
        /// <returns>A pointer to the previous information, or 0 if not successful.</returns>
        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Unicode)]
        private static extern IntPtr SetWindowLongx86(HandleRef hwnd, WindowLongType index, IntPtr info);

        /// <summary>
        /// Function to set information for the specified window.
        /// </summary>
        /// <param name="hwnd">Window handle to set information on.</param>
        /// <param name="index">Type of information.</param>
        /// <param name="info">Information to set.</param>
        /// <returns>A pointer to the previous information, or 0 if not successful.</returns>
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Unicode)]
        private static extern IntPtr SetWindowLongx64(HandleRef hwnd, WindowLongType index, IntPtr info);

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
        public static extern IntPtr CallWindowProc(IntPtr wndProc, IntPtr hwnd, WindowMessages msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Function to retrieve information about the specified window.
        /// </summary>
        /// <param name="hwnd">Window handle to retrieve information from.</param>
        /// <param name="index">Type of information.</param>
        /// <returns>A pointer to the information.</returns>
        public static IntPtr GetWindowLong(HandleRef hwnd, WindowLongType index)
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
        public static IntPtr SetWindowLong(HandleRef hwnd, WindowLongType index, IntPtr info)
        {
            return IntPtr.Size == 4 ? SetWindowLongx86(hwnd, index, info) : SetWindowLongx64(hwnd, index, info);
        }

	    /// <summary>
		/// Function to register a raw input device.
		/// </summary>
		/// <param name="device">Device information.</param>
		/// <returns><c>true</c> if successful, <c>false</c> if not.</returns>
		public static bool RegisterRawInputDevices(RAWINPUTDEVICE device)
		{
			var devices = new RAWINPUTDEVICE[1];		// Raw input devices.

			devices[0] = device;
			return RegisterRawInputDevices(devices, 1, DirectAccess.SizeOf<RAWINPUTDEVICE>());
		}

		/// <summary>
		/// Function to retrieve device information.
		/// </summary>
		/// <param name="deviceHandle">Device handle.</param>
		/// <returns>The device information structure.</returns>
		public static unsafe RID_DEVICE_INFO GetDeviceInfo(IntPtr deviceHandle)
		{
			int dataSize = DirectAccess.SizeOf<RID_DEVICE_INFO>();

			byte* data = stackalloc byte[dataSize];

			if (GetRawInputDeviceInfo(deviceHandle, RawInputCommand.DeviceInfo, (IntPtr)data, ref dataSize) < 0)
			{
				throw new Win32Exception();
			}

			RID_DEVICE_INFO result;

			DirectAccess.MemoryCopy(&result, data, dataSize);

			return result;
		}

		/// <summary>
		/// Function to enumerate raw input devices.
		/// </summary>
		/// <returns>An array of raw input device structures.</returns>
		public unsafe static RAWINPUTDEVICELIST[] EnumerateInputDevices()
		{
		    int deviceCount = 0;
			int structSize = DirectAccess.SizeOf<RAWINPUTDEVICELIST>();

			if (GetRawInputDeviceList(IntPtr.Zero, ref deviceCount, structSize) < 0)
			{
				throw new Win32Exception();
			}

			if (deviceCount == 0)
			{
				return new RAWINPUTDEVICELIST[0];
			}

			RAWINPUTDEVICELIST* deviceListPtr = stackalloc RAWINPUTDEVICELIST[deviceCount];
			
			if (GetRawInputDeviceList((IntPtr)deviceListPtr, ref deviceCount, structSize) < 0)
			{
				throw new Win32Exception();
			}

			var result = new RAWINPUTDEVICELIST[deviceCount];

			fixed (RAWINPUTDEVICELIST* resultPtr = &result[0])
			{
				DirectAccess.MemoryCopy(resultPtr, deviceListPtr, structSize * deviceCount);
			}

			return result;
		}

		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes the <see cref="Win32API"/> class.
		/// </summary>
		static Win32API()
		{
			Marshal.PrelinkAll(typeof(Win32API));
		}
		#endregion
	}
}
