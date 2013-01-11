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
using System.Runtime.InteropServices;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Native
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
	[System.Security.SuppressUnmanagedCodeSecurity]
	internal static class Win32API
	{
		#region Methods.
		/// <summary>
		/// Function to set the visibility of the mouse cursor.
		/// </summary>
		/// <param name="bShow">TRUE to show, FALSE to hide.</param>
		/// <returns>-1 if no mouse is installed, 0 or greater for the number of times this function has been called with TRUE.</returns>
		[DllImport("User32.dll")]
		public static extern int ShowCursor(bool bShow);

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
		[DllImport("user32.dll")]
		public static extern int GetRawInputDeviceInfo(IntPtr hDevice, int uiCommand, IntPtr pData, ref int pcbSize);

		/// <summary>
		/// Function to register a raw input device.
		/// </summary>
		/// <param name="pRawInputDevices">Array of raw input devices.</param>
		/// <param name="uiNumDevices">Number of devices.</param>
		/// <param name="cbSize">Size of the RAWINPUTDEVICE structure.</param>
		/// <returns>TRUE if successful, FALSE if not.</returns>
		[DllImport("user32.dll")]
		public static extern bool RegisterRawInputDevices([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] RAWINPUTDEVICE[] pRawInputDevices, int uiNumDevices, int cbSize);

		/// <summary>
		/// Function to register a raw input device.
		/// </summary>
		/// <param name="device">Device information.</param>
		/// <returns>TRUE if successful, FALSE if not.</returns>
		public static bool RegisterRawInputDevices(RAWINPUTDEVICE device)
		{
			RAWINPUTDEVICE[] devices = new RAWINPUTDEVICE[1];		// Raw input devices.

			devices[0] = device;
			return RegisterRawInputDevices(devices, 1, Marshal.SizeOf(typeof(RAWINPUTDEVICE)));
		}

		/// <summary>
		/// Function to retrieve device information.
		/// </summary>
		/// <param name="deviceHandle">Device handle.</param>
		/// <returns>The device information structure.</returns>
		public static RID_DEVICE_INFO GetDeviceInfo(IntPtr deviceHandle)
		{
			int dataSize = 0;

			if (GetRawInputDeviceInfo(deviceHandle, (int)RawInputDeviceInfo.DeviceInfo, IntPtr.Zero, ref dataSize) >= 0)
			{
				IntPtr data = Marshal.AllocHGlobal(dataSize * 2);

				try
				{
					if (GetRawInputDeviceInfo(deviceHandle, (int)RawInputCommand.DeviceInfo, data, ref dataSize) >= 0)
					{
						RID_DEVICE_INFO result = default(RID_DEVICE_INFO);
						result = (RID_DEVICE_INFO)(Marshal.PtrToStructure(data, typeof(RID_DEVICE_INFO)));
						return result;
					}
					else
						throw new System.ComponentModel.Win32Exception();
				}
				finally
				{
					if (data != IntPtr.Zero)
						Marshal.FreeHGlobal(data);
				}
			}
			else
				throw new System.ComponentModel.Win32Exception();
		}

		/// <summary>
		/// Function to enumerate raw input devices.
		/// </summary>
		/// <returns>An array of raw input device structures.</returns>
		public static RAWINPUTDEVICELIST[] EnumerateInputDevices()
		{
			RAWINPUTDEVICELIST[] result = null;
			int deviceCount = 0;
			int structSize = Marshal.SizeOf(typeof(RAWINPUTDEVICELIST));

			if (GetRawInputDeviceList(IntPtr.Zero, ref deviceCount, structSize) >= 0)
			{
				if (deviceCount != 0)
				{
					IntPtr deviceList = Marshal.AllocHGlobal(structSize * deviceCount);
					try
					{
						if (GetRawInputDeviceList(deviceList, ref deviceCount, structSize) >= 0)
						{
							result = new RAWINPUTDEVICELIST[deviceCount];

							for (int i = 0; i < result.Length; i++)
							{
								if (GorgonComputerInfo.PlatformArchitecture == PlatformArchitecture.x64)
									result[i] = (RAWINPUTDEVICELIST)(Marshal.PtrToStructure(new IntPtr(deviceList.ToInt64() + (structSize * i)), typeof(RAWINPUTDEVICELIST)));
								else
									result[i] = (RAWINPUTDEVICELIST)(Marshal.PtrToStructure(new IntPtr(deviceList.ToInt32() + (structSize * i)), typeof(RAWINPUTDEVICELIST)));
							}

							return result;
						}
						else
							throw new System.ComponentModel.Win32Exception();
					}
					finally
					{
						if (deviceList != IntPtr.Zero)
							Marshal.FreeHGlobal(deviceList);
					}
				}
				else
					return new RAWINPUTDEVICELIST[0];
			}
			else
				throw new System.ComponentModel.Win32Exception();
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
