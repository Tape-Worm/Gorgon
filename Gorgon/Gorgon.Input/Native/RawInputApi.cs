﻿#region MIT
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
// Created: Tuesday, September 08, 2015 1:26:21 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Input;
using Gorgon.Input.Properties;

namespace Gorgon.Native
{
	#region Enumerations.
	/// <summary>
	/// Enumeration containing HID usage page flags.
	/// </summary>
	// ReSharper disable InconsistentNaming
	public enum HIDUsagePage
		: ushort
	{
		/// <summary>Unknown usage page.</summary>
		Undefined = 0x00,
		/// <summary>Generic desktop controls.</summary>
		Generic = 0x01,
		/// <summary>Simulation controls.</summary>
		Simulation = 0x02,
		/// <summary>Virtual reality controls.</summary>
		VR = 0x03,
		/// <summary>Sports controls.</summary>
		Sport = 0x04,
		/// <summary>Games controls.</summary>
		Game = 0x05,
		/// <summary>Keyboard controls.</summary>
		Keyboard = 0x07,
		/// <summary>LED controls.</summary>
		LED = 0x08,
		/// <summary>Button.</summary>
		Button = 0x09,
		/// <summary>Ordinal.</summary>
		Ordinal = 0x0A,
		/// <summary>Telephony.</summary>
		Telephony = 0x0B,
		/// <summary>Consumer.</summary>
		Consumer = 0x0C,
		/// <summary>Digitizer.</summary>
		Digitizer = 0x0D,
		/// <summary>Physical interface device.</summary>
		PID = 0x0F,
		/// <summary>Unicode.</summary>
		Unicode = 0x10,
		/// <summary>Alphanumeric display.</summary>
		AlphaNumeric = 0x14,
		/// <summary>Medical instruments.</summary>
		Medical = 0x40,
		/// <summary>Monitor page 0.</summary>
		MonitorPage0 = 0x80,
		/// <summary>Monitor page 1.</summary>
		MonitorPage1 = 0x81,
		/// <summary>Monitor page 2.</summary>
		MonitorPage2 = 0x82,
		/// <summary>Monitor page 3.</summary>
		MonitorPage3 = 0x83,
		/// <summary>Power page 0.</summary>
		PowerPage0 = 0x84,
		/// <summary>Power page 1.</summary>
		PowerPage1 = 0x85,
		/// <summary>Power page 2.</summary>
		PowerPage2 = 0x86,
		/// <summary>Power page 3.</summary>
		PowerPage3 = 0x87,
		/// <summary>Bar code scanner.</summary>
		BarCode = 0x8C,
		/// <summary>Scale page.</summary>
		Scale = 0x8D,
		/// <summary>Magnetic strip reading devices.</summary>
		MSR = 0x8E
	}

	/// <summary>
	/// Enumeration for HID usage flags.
	/// </summary>
	public enum HIDUsage
		: ushort
	{
		/// <summary></summary>
		None = 0,
		/// <summary></summary>
		Pointer = 0x01,
		/// <summary></summary>
		Mouse = 0x02,
		/// <summary></summary>
		Joystick = 0x04,
		/// <summary></summary>
		Gamepad = 0x05,
		/// <summary></summary>
		Keyboard = 0x06,
		/// <summary></summary>
		Keypad = 0x07,
		/// <summary></summary>
		SystemControl = 0x80,
		/// <summary></summary>
		X = 0x30,
		/// <summary></summary>
		Y = 0x31,
		/// <summary></summary>
		Z = 0x32,
		/// <summary></summary>
		RelativeX = 0x33,
		/// <summary></summary>		
		RelativeY = 0x34,
		/// <summary></summary>
		RelativeZ = 0x35,
		/// <summary></summary>
		Slider = 0x36,
		/// <summary></summary>
		Dial = 0x37,
		/// <summary></summary>
		Wheel = 0x38,
		/// <summary></summary>
		HatSwitch = 0x39,
		/// <summary></summary>
		CountedBuffer = 0x3A,
		/// <summary></summary>
		ByteCount = 0x3B,
		/// <summary></summary>
		MotionWakeup = 0x3C,
		/// <summary></summary>
		VX = 0x40,
		/// <summary></summary>
		VY = 0x41,
		/// <summary></summary>
		VZ = 0x42,
		/// <summary></summary>
		VBRX = 0x43,
		/// <summary></summary>
		VBRY = 0x44,
		/// <summary></summary>
		VBRZ = 0x45,
		/// <summary></summary>
		VNO = 0x46,
		/// <summary></summary>
		SystemControlPower = 0x81,
		/// <summary></summary>
		SystemControlSleep = 0x82,
		/// <summary></summary>
		SystemControlWake = 0x83,
		/// <summary></summary>
		SystemControlContextMenu = 0x84,
		/// <summary></summary>
		SystemControlMainMenu = 0x85,
		/// <summary></summary>
		SystemControlApplicationMenu = 0x86,
		/// <summary></summary>
		SystemControlHelpMenu = 0x87,
		/// <summary></summary>
		SystemControlMenuExit = 0x88,
		/// <summary></summary>
		SystemControlMenuSelect = 0x89,
		/// <summary></summary>
		SystemControlMenuRight = 0x8A,
		/// <summary></summary>
		SystemControlMenuLeft = 0x8B,
		/// <summary></summary>
		SystemControlMenuUp = 0x8C,
		/// <summary></summary>
		SystemControlMenuDown = 0x8D,
		/// <summary></summary>
		DPadUp = 0x90,
		/// <summary></summary>
		DPadDown = 0x91,
		/// <summary></summary>
		DPadRight = 0x92,
		/// <summary></summary>
		DPadLeft = 0x93
	}
	// ReSharper restore InconsistentNaming

	/// <summary>
	/// Enumeration containing the command types to issue.
	/// </summary>
	enum RawInputCommand
	{
		/// <summary>
		/// Get input data.
		/// </summary>
		Input = 0x10000003,
		/// <summary>
		/// Get header data.
		/// </summary>
		Header = 0x10000005,
		/// <summary>
		/// Previously parsed data.
		/// </summary>
		PreparsedData = 0x20000005,
		/// <summary>
		/// Only return the device name, return value means number of characters, not bytes.
		/// </summary>
		DeviceName = 0x20000007,
		/// <summary>
		/// Return RAWINPUTDEVICEINFO data.
		/// </summary>
		DeviceInfo = 0x2000000B
	}

	/// <summary>
	/// Enumeration containing flags for a raw input device.
	/// </summary>
	[Flags]
	enum RawInputDeviceFlags
	{
		/// <summary>No flags.</summary>
		None = 0,
		/// <summary>If set, this removes the top level collection from the inclusion list. This tells the operating system to stop reading from a device which matches the top level collection.</summary>
		Remove = 0x00000001,
		/// <summary>If set, this specifies the top level collections to exclude when reading a complete usage page. This flag only affects a TLC whose usage page is already specified with PageOnly.</summary>
		Exclude = 0x00000010,
		/// <summary>If set, this specifies all devices whose top level collection is from the specified usUsagePage. Note that Usage must be zero. To exclude a particular top level collection, use Exclude.</summary>
		PageOnly = 0x00000020,
		/// <summary>If set, this prevents any devices specified by UsagePage or Usage from generating legacy messages. This is only for the mouse and keyboard.</summary>
		NoLegacy = 0x00000030,
		/// <summary>If set, this enables the caller to receive the input even when the caller is not in the foreground. Note that WindowHandle must be specified.</summary>
		InputSink = 0x00000100,
		/// <summary>If set, the mouse button click does not activate the other window.</summary>
		CaptureMouse = 0x00000200,
		/// <summary>If set, the application-defined keyboard device hot keys are not handled. However, the system hot keys; for example, ALT+TAB and CTRL+ALT+DEL, are still handled. By default, all keyboard hotkeys are handled. NoHotKeys can be specified even if NoLegacy is not specified and WindowHandle is <b>null</b>.</summary>
		NoHotKeys = 0x00000200,
		/// <summary>If set, application keys are handled.  NoLegacy must be specified.  Keyboard only.</summary>
		AppKeys = 0x00000400,
		/// <summary>If set, this enables the caller to receive input in the background only if the foreground application does not process it. In other words, if the foreground application is not registered for raw input, then the background application that is registered will receive the input.</summary>
		InputSinkEx = 0x00001000
	}
	#endregion

	/// <summary>
	/// Native raw Input API functionality
	/// </summary>
	static class RawInputApi
	{
		#region Variables.
		// The size of the raw input data header.
		private static readonly int _headerSize = DirectAccess.SizeOf<RAWINPUTHEADER>();
		#endregion

		#region Constants.
		/// <summary>
		/// The window message for Raw Input.
		/// </summary>
		public const int WmRawInput = 0x00FF;
		#endregion

		#region Methods.
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
		private static extern unsafe int GetRawInputData(IntPtr hRawInput, RawInputCommand uiCommand, void* pData, ref int pcbSize, int cbSizeHeader);

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
		private static extern int GetRawInputDeviceInfo(IntPtr hDevice, RawInputCommand uiCommand, IntPtr pData, ref int pcbSize);

		/// <summary>
		/// Function to register a raw input device.
		/// </summary>
		/// <param name="pRawInputDevices">Array of raw input devices.</param>
		/// <param name="uiNumDevices">Number of devices.</param>
		/// <param name="cbSize">Size of the RAWINPUTDEVICE structure.</param>
		/// <returns><b>true</b> if successful, <b>false</b> if not.</returns>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool RegisterRawInputDevices([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] RAWINPUTDEVICE[] pRawInputDevices, int uiNumDevices, int cbSize);

		/// <summary>
		/// Function to retrieve the raw input devices registered to this application.
		/// </summary>
		/// <param name="pRawInputDevices">The buffer to hold the list of raw input devices.</param>
		/// <param name="puiNumDevices">The number of devices.</param>
		/// <param name="cbSize">The size of the raw input device struct, in bytes.</param>
		/// <returns>0 if successful, -1 if not.</returns>
		[DllImport("user32.dll")]
		private static extern int GetRegisteredRawInputDevices(IntPtr pRawInputDevices, ref uint puiNumDevices, uint cbSize);

		/// <summary>
		/// Function to perform a buffered read of raw input data.
		/// </summary>
		/// <param name="pData">Raw input data structures.</param>
		/// <param name="pcbSize">Size of the raw input structure.</param>
		/// <param name="cbSizeHeader">Size of the raw input header value.</param>
		/// <returns>0 if successful, -1 if not.</returns>
		[DllImport("user32.dll")]
		private static extern unsafe int GetRawInputBuffer(RAWINPUT* pData, uint* pcbSize, uint cbSizeHeader);


		/// <summary>
		/// Function to retrieve the preparsed HID data for a given device.
		/// </summary>
		/// <param name="deviceHandle">The handle to the device.</param>
		/// <returns>A pointer to the block of memory holding the HID preparsed data.</returns>
		public static GorgonPointer GetPreparsedDeviceInfoData(IntPtr deviceHandle)
		{
			int dataSize = 0;
			int win32Error;

			// Get pre-parsed joystick data.
			if (GetRawInputDeviceInfo(deviceHandle, RawInputCommand.PreparsedData, IntPtr.Zero, ref dataSize) != 0)
			{
				win32Error = Marshal.GetLastWin32Error();
				throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, win32Error));
			}

			if (dataSize == 0)
			{
				return null;
			}

			GorgonPointer result = new GorgonPointer(dataSize);

			if (GetRawInputDeviceInfo(deviceHandle, RawInputCommand.PreparsedData, new IntPtr(result.Address), ref dataSize) >= 0)
			{
				return result;
			}

			win32Error = Marshal.GetLastWin32Error();
			throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, win32Error));
		}

		/// <summary>
		/// Function to perform a buffered read of raw input data.
		/// </summary>
		/// <returns>An array of raw input data structures.</returns>
		public static RAWINPUT[] GetBufferedInput()
		{
			RAWINPUT[] result;
			uint headerSize = (uint)DirectAccess.SizeOf<RAWINPUTHEADER>();
			uint rawInputSize = (uint)DirectAccess.SizeOf<RAWINPUT>();
            uint bufferSize = 0;

			unsafe
			{
				if (GetRawInputBuffer(null, &bufferSize, headerSize) == -1)
				{
					throw new Win32Exception(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
				}

				if (bufferSize == 0)
				{
					return new RAWINPUT[0];
				}

				uint resultLength = bufferSize / rawInputSize;
				bufferSize *= 8;

				using (GorgonPointer data = new GorgonPointer(bufferSize, 8))
				{
					if (GetRawInputBuffer((RAWINPUT*)data.Address, &bufferSize, headerSize) == -1)
					{
						throw new Win32Exception(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
					}

					result = new RAWINPUT[resultLength];

					for (int i = 0; i < resultLength; ++i)
					{
						result[i] = data.Read<RAWINPUT>(i * rawInputSize);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Function to register a raw input device.
		/// </summary>
		/// <param name="usage">The HID usage code to register.</param>
		/// <param name="targetWindow">The target window for the device.</param>
		/// <param name="flags">Flags used in device registration.</param>
		public static void RegisterRawInputDevice(HIDUsage usage, IntPtr targetWindow, RawInputDeviceFlags flags)
		{
			var devices = new []
			              {
				              new RAWINPUTDEVICE
				              {
					              Flags = flags,
								  Usage = (ushort)usage,
					              UsagePage = HIDUsagePage.Generic,
					              WindowHandle = targetWindow
				              }
			              };

			if (!RegisterRawInputDevices(devices, devices.Length, DirectAccess.SizeOf<RAWINPUTDEVICE>()))
			{
				throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_ERR_CANNOT_REGISTER);
			}
		}

		/// <summary>
		/// Function to retrieve an existing device type registration.
		/// </summary>
		/// <param name="usage">Device usage to filter.</param>
		/// <returns>The raw input device structure if the device type was previously registered. Or <b>null</b> if not.</returns>
		public static RAWINPUTDEVICE? GetDeviceRegistration(HIDUsage usage)
		{
			uint deviceCount = 0;
			uint structSize = (uint)DirectAccess.SizeOf<RAWINPUTDEVICE>();

			unsafe
			{
				if (GetRegisteredRawInputDevices(IntPtr.Zero, ref deviceCount, structSize) == -1)
				{
					throw new Win32Exception(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
				}

				RAWINPUTDEVICE* buffer = stackalloc RAWINPUTDEVICE[(int)deviceCount];

				if (GetRegisteredRawInputDevices(new IntPtr(buffer), ref deviceCount, structSize) == -1)
				{
					throw new Win32Exception(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
				}

				for (int i = 0; i < deviceCount; ++i)
				{
					if (buffer[i].Usage == (ushort)usage)
					{
						return *buffer;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Function to unregister a raw input device.
		/// </summary>
		/// <param name="usage">The HID usage code to unregister.</param>
		public static void UnregisterRawInputDevice(HIDUsage usage)
		{
			var devices = new[]
			              {
				              new RAWINPUTDEVICE
				              {
					              Flags = RawInputDeviceFlags.Remove,
					              Usage = (ushort)usage,
					              UsagePage = HIDUsagePage.Generic,
					              WindowHandle = IntPtr.Zero
				              }
			              };

			if (!RegisterRawInputDevices(devices, devices.Length, DirectAccess.SizeOf<RAWINPUTDEVICE>()))
			{
				throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_ERR_CANNOT_REGISTER);
			}
		}

		/// <summary>
		/// Function to perform the enumeration and translation of raw input native structures.
		/// </summary>
		/// <param name="deviceType">The type of device to filter out.</param>
		/// <returns>The raw input device list.</returns>
		public static RAWINPUTDEVICELIST[] EnumerateRawInputDevices(RawInputType deviceType)
		{
			int deviceCount = 0;
			int structSize = DirectAccess.SizeOf<RAWINPUTDEVICELIST>();

			// Define how large the buffer needs to be.
			if (GetRawInputDeviceList(IntPtr.Zero, ref deviceCount, structSize) < 0)
			{
				int win32Error = Marshal.GetLastWin32Error();
				throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_ENUMERATE_WIN32_ERR, win32Error));
			}

			if (deviceCount == 0)
			{
				return new RAWINPUTDEVICELIST[0];
			}

			unsafe
			{
				RAWINPUTDEVICELIST* deviceListPtr = stackalloc RAWINPUTDEVICELIST[deviceCount];

				if (GetRawInputDeviceList((IntPtr)deviceListPtr, ref deviceCount, structSize) < 0)
				{
					int win32Error = Marshal.GetLastWin32Error();
					throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_ENUMERATE_WIN32_ERR, win32Error));
				}

				int resultCount = 0;

				// Find out how many devices of the specified type exist.
				for (int i = 0; i < deviceCount; ++i)
				{
					if (deviceListPtr[i].DeviceType != deviceType)
					{
						continue;
					}

					++resultCount;
				}

				if (resultCount == 0)
				{
					return new RAWINPUTDEVICELIST[0];
				}

				// Send back a copy of our stack array.
				var result = new RAWINPUTDEVICELIST[resultCount];

				resultCount = 0;
				for (int i = 0; i < deviceCount; ++i)
				{
					if (deviceListPtr[i].DeviceType != deviceType)
					{
						continue;
					}

					result[resultCount++] = deviceListPtr[i];
				}

				return result;
			}
		}

		/// <summary>
		/// Function to retrieve information about a specific device.
		/// </summary>
		/// <param name="device">The device to retrieve information about.</param>
		/// <returns>The device information for the device.</returns>
		public static RID_DEVICE_INFO GetDeviceInfo(ref RAWINPUTDEVICELIST device)
		{
			int dataSize = 0;
			int errCode = GetRawInputDeviceInfo(device.Device, RawInputCommand.DeviceInfo, IntPtr.Zero, ref dataSize);

			if ((errCode != -1) && (errCode != 0))
			{
				int win32Error = Marshal.GetLastWin32Error();
				throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, win32Error));
			}

			if (errCode == -1)
			{
				throw new InternalBufferOverflowException(string.Format(Resources.GORINP_RAW_ERR_BUFFER_TOO_SMALL, dataSize));
			}

			unsafe
			{
				byte* data = stackalloc byte[dataSize];
				errCode = GetRawInputDeviceInfo(device.Device, RawInputCommand.DeviceInfo, (IntPtr)data, ref dataSize);

				if (errCode < -1)
				{
					int win32Error = Marshal.GetLastWin32Error();
					throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, win32Error));
				}

				if (errCode == -1)
				{
					throw new InternalBufferOverflowException(string.Format(Resources.GORINP_RAW_ERR_BUFFER_TOO_SMALL, dataSize));
				}

				return *((RID_DEVICE_INFO*)data);
			}
		}

		/// <summary>
		/// Function to retrieve the name for the specified device.
		/// </summary>
		/// <param name="device">Device to retrieve the name for.</param>
		/// <returns>A string containing the device name.</returns>
		public static string GetDeviceName(ref RAWINPUTDEVICELIST device)
		{
			int dataSize = 0;

			if (GetRawInputDeviceInfo(device.Device, RawInputCommand.DeviceName, IntPtr.Zero, ref dataSize) < 0)
			{
				int win32Error = Marshal.GetLastWin32Error();
				throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, win32Error));
			}

			// Do nothing if we have no data.
			if (dataSize < 4)
			{
				return null;
			}

			unsafe
			{
				char* data = stackalloc char[dataSize];

				// ReSharper disable once InvertIf
				if (GetRawInputDeviceInfo(device.Device, RawInputCommand.DeviceName, (IntPtr)data, ref dataSize) < 0)
				{
					int win32Error = Marshal.GetLastWin32Error();
					throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, win32Error));
				}

				// The strings that come back from native land will end with a null terminator, so crop that off.
				return new string(data, 0, dataSize - 1);
			}
		}

		/// <summary>
		/// Function to retrieve data for the raw input device message.
		/// </summary>
		/// <param name="rawInputStructHandle">Handle to a HRAWINPUT structure..</param>
		public static RAWINPUT GetRawInputData(IntPtr rawInputStructHandle)
		{
			int dataSize = 0;

			unsafe
			{
				// Get data size.			
				int retVal = GetRawInputData(rawInputStructHandle, RawInputCommand.Input, null, ref dataSize, _headerSize);

				if (retVal == -1)
				{
					throw new GorgonException(GorgonResult.CannotRead, Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
				}

				RAWINPUT rawInput;

				// Get actual data.
				retVal = GetRawInputData(rawInputStructHandle, RawInputCommand.Input, &rawInput, ref dataSize, _headerSize);

				if ((retVal == -1)
					|| (retVal != dataSize))
				{
					throw new GorgonException(GorgonResult.CannotRead, Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
				}

				return rawInput;
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes static members of the <see cref="RawInputApi"/> class.
		/// </summary>
		static RawInputApi()
		{
			Marshal.PrelinkAll(typeof(RawInputApi));
		}
		#endregion
	}
}
