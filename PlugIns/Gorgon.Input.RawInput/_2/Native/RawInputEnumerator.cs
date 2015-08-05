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
// Created: Wednesday, July 22, 2015 12:24:32 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Diagnostics;
using Gorgon.Input.Raw.Properties;
using Gorgon.Native;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// Provides enumeration capabilities for raw input through native interfaces.
	/// </summary>
	class RawInputEnumerator
	{
		/// <summary>
		/// Function to perform the enumeration and translation of raw input native structures.
		/// </summary>
		/// <returns>The raw input device list.</returns>
		private static RAWINPUTDEVICELIST[] EnumerateRawInputDevices()
		{
			int deviceCount = 0;
			int structSize = DirectAccess.SizeOf<RAWINPUTDEVICELIST>();

			// Define how large the buffer needs to be.
			if (Win32API.GetRawInputDeviceList(IntPtr.Zero, ref deviceCount, structSize) < 0)
			{
				throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_ENUMERATE_WIN32_ERR, Marshal.GetLastWin32Error()));
			}

			if (deviceCount == 0)
			{
				return new RAWINPUTDEVICELIST[0];
			}

			unsafe
			{
				RAWINPUTDEVICELIST* deviceListPtr = stackalloc RAWINPUTDEVICELIST[deviceCount];

				if (Win32API.GetRawInputDeviceList((IntPtr)deviceListPtr, ref deviceCount, structSize) < 0)
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
		}

		/// <summary>
		/// Function to enumerate the available raw input devices.
		/// </summary>
		/// <param name="deviceType">The type of device to look for.</param>
		/// <returns>A list of raw input devices.</returns>
		public RAWINPUTDEVICELIST[] Enumerate(InputDeviceType deviceType)
		{
			RawInputType rawInputType;

			switch (deviceType)
			{
				case InputDeviceType.Keyboard:
					rawInputType = RawInputType.Keyboard;
					break;
				case InputDeviceType.Mouse:
					rawInputType = RawInputType.Mouse;
					break;
				default:
					rawInputType = RawInputType.HID;
					break;
			}

			// Get the full device list. This way we can refresh it each time we request an enumeration.
			RAWINPUTDEVICELIST[] devices = EnumerateRawInputDevices();

			// Filter our list to the device type that we want.
			return devices.Where(item => item.DeviceType == rawInputType).ToArray();
		}
	}
}
