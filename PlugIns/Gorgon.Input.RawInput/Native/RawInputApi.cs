using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using Gorgon.Core;
using Gorgon.Input;
using Gorgon.Input.Raw.Properties;

namespace Gorgon.Native
{
	#region Enumerations.
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
		/// <summary>If set, the application-defined keyboard device hot keys are not handled. However, the system hot keys; for example, ALT+TAB and CTRL+ALT+DEL, are still handled. By default, all keyboard hotkeys are handled. NoHotKeys can be specified even if NoLegacy is not specified and WindowHandle is NULL.</summary>
		NoHotKeys = 0x00000200,
		/// <summary>If set, application keys are handled.  NoLegacy must be specified.  Keyboard only.</summary>
		AppKeys = 0x00000400,
		/// <summary>If set, this enables the caller to receive input in the background only if the foreground application does not process it. In other words, if the foreground application is not registered for raw input, then the background application that is registered will receive the input.</summary>
		InputSinkEx = 0x00001000
	}
	#endregion

	/// <summary>
	/// Raw input native functionality.
	/// </summary>
	[SuppressUnmanagedCodeSecurity]
	static class RawInputApi
	{
		#region Variables.
		// The size of the raw input data header.
		private static readonly int _headerSize = DirectAccess.SizeOf<RAWINPUTHEADER>();
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the mouse is registered as exclusive.
		/// </summary>
		public static bool IsMouseExclusive
		{
			get;
			private set;
		}
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
		private unsafe static extern int GetRawInputData(IntPtr hRawInput, RawInputCommand uiCommand, void *pData, ref int pcbSize, int cbSizeHeader);

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

				RID_DEVICE_INFO result = *((RID_DEVICE_INFO*)data);

				return result;
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
				return string.Empty;
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

				// The strings that come back from native land will end with a NULL terminator, so crop that off.
				return new string(data, 0, dataSize - 1);
			}
		}

		/// <summary>
		/// Function to retrieve a list of registered raw input devices.
		/// </summary>
		/// <returns>An array of raw input device data.</returns>
		public static RAWINPUTDEVICE[] GetRegisteredDevices()
		{
			uint deviceCount = 0;
			uint structSize = (uint)DirectAccess.SizeOf<RAWINPUTDEVICE>();
			RAWINPUTDEVICE[] result;

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

				result = new RAWINPUTDEVICE[(int)deviceCount];

				for (int i = 0; i < result.Length; ++i)
				{
					result[i] = buffer[i];
				}
			}

			return result;
		}

		/// <summary>
		/// Function to retrieve data for the raw input device message.
		/// </summary>
		/// <param name="deviceHandle">Raw input device handle.</param>
		public static RAWINPUT GetRawInputData(IntPtr deviceHandle)
		{
			int dataSize = 0;

			unsafe
			{
				// Get data size.			
				int retVal = GetRawInputData(deviceHandle, RawInputCommand.Input, null, ref dataSize, _headerSize);

				if (retVal == -1)
				{
					throw new GorgonException(GorgonResult.CannotRead, Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
				}

				RAWINPUT rawInput;

				// Get actual data.
				retVal = GetRawInputData(deviceHandle, RawInputCommand.Input, &rawInput, ref dataSize, _headerSize);

				if ((retVal == -1)
				    || (retVal != dataSize))
				{
					throw new GorgonException(GorgonResult.CannotRead, Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA);
				}

				return rawInput;
			}
		}

		/// <summary>
		/// Function to register a raw input de
		/// </summary>
		/// <param name="windowHandle">Handle to the window to register with.</param>
		public static void RegisterRawInputDevices(IntPtr windowHandle)
		{
			RAWINPUTDEVICE[] devices = GetRegisteredDevices();

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
							  WindowHandle = windowHandle
						  },
						  new RAWINPUTDEVICE
						  {
							  Usage = (ushort)HIDUsage.Mouse,
							  UsagePage = HIDUsagePage.Generic,
							  Flags = RawInputDeviceFlags.InputSink,
							  WindowHandle = windowHandle
						  },
						  new RAWINPUTDEVICE
						  {
							  Usage = (ushort)HIDUsage.Joystick,
							  UsagePage = HIDUsagePage.Generic,
							  Flags = RawInputDeviceFlags.InputSink,
							  WindowHandle = windowHandle
						  },
						  new RAWINPUTDEVICE
						  {
							  Usage = (ushort)HIDUsage.Gamepad,
							  UsagePage = HIDUsagePage.Generic,
							  Flags = RawInputDeviceFlags.InputSink,
							  WindowHandle = windowHandle
						  }
					  };

			if (!RegisterRawInputDevices(devices, devices.Length, DirectAccess.SizeOf<RAWINPUTDEVICE>()))
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
		public static bool SetExclusiveState(InputDeviceType deviceType, bool exclusive)
		{
			RAWINPUTDEVICE[] devices = GetRegisteredDevices();

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
					IsMouseExclusive = exclusive;
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

			if (!RegisterRawInputDevices(devices, devices.Length, DirectAccess.SizeOf<RAWINPUTDEVICE>()))
			{
				throw new GorgonException(GorgonResult.CannotBind, Resources.GORINP_RAW_ERR_CANNOT_BIND_INPUT_DEVICES);
			}

			return result;
		}

		/// <summary>
		/// Function to unregister all raw input devices.
		/// </summary>
		public static void UnregisterRawInputDevices()
		{
			RAWINPUTDEVICE[] devices = GetRegisteredDevices();

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

			if (!RegisterRawInputDevices(devices, devices.Length, DirectAccess.SizeOf<RAWINPUTDEVICE>()))
			{
				throw new GorgonException(GorgonResult.DriverError, Resources.GORINP_RAW_ERR_CANNOT_UNBIND_INPUT_DEVICES);
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
					throw new Win32Exception();
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
