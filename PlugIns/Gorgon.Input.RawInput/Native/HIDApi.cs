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
// Created: Wednesday, August 12, 2015 11:40:20 PM
// 
#endregion

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using Gorgon.Input.Raw.Properties;

namespace Gorgon.Native
{
	#region HID Usage enums.
	/// <summary>
	/// Enumeration containing HID usage page flags.
	/// </summary>
	// ReSharper disable InconsistentNaming
	enum HIDUsagePage
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
	/// Constants for HID usage flags.
	/// </summary>
	enum HIDUsage
		: ushort
	{
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
		DPadUp = 0x90,
		DPadDown = 0x91,
		DPadRight = 0x92,
		DPadLeft = 0x93
	}
	// ReSharper restore InconsistentNaming
	#endregion

	/// <summary>
	/// Native functionality for reading HID device data.
	/// </summary>
	[SuppressUnmanagedCodeSecurity]
	static class HIDApi
	{
		#region Constants.
		// Successful HID call.
		private const int HidStatusSuccess = 0x11 << 16;
		// Invalid preparsed HID data.
		private const int HidInvalidPreparsedData = 0x8 << 28 | 0x11 << 16 | 0x1;
		/*		// Invalid report length.
				private const int HidInvalidReportLength = 0xC << 28 | 0x11 << 16 | 0x3;
				// Invalid report type.
				private const int HidInvalidReportType = 0xC << 28 | 0x11 << 16 | 0x2;
				// Buffer too small.
				private const int HidStatusBufferTooSmall = 0xC << 28 | 0x11 << 16 | 0x7;
				// Incompatible report ID.
				private const int HidStatusIncompatibleReportID = 0xC << 28 | 0x11 << 16 | 0xA;
				// Usage not found.
				private const int HidStatusUsageNotFound = 0xC << 28 | 0x11 << 16 | 0x4;*/
		#endregion

		#region Enums.
		/// <summary>
		/// HID report type.
		/// </summary>
		private enum HIDReportType
		{
			Input,
			Output,
			Feature
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the capabilities for a device.
		/// </summary>
		/// <param name="preparsedData">Preparsed data to evaluate.</param>
		/// <param name="Capabilities">The capabilities for the device.</param>
		/// <returns>The success/fail return code.</returns>
		[DllImport("Hid.dll", CharSet = CharSet.Unicode)]
		private static extern unsafe int HidP_GetCaps(byte* preparsedData, HIDP_CAPS* Capabilities);

		/// <summary>
		/// Function to retrieve the button information for a device.
		/// </summary>
		/// <param name="reportType">Type of report.</param>
		/// <param name="buttonCaps">The button capabilities.</param>
		/// <param name="capsLength">The number of button capabilities.</param>
		/// <param name="preParsedData">The pre-parsed data to use.</param>
		/// <returns>The success/fail return code.</returns>
		[DllImport("Hid.Dll", CharSet = CharSet.Unicode)]
		private static extern unsafe int HidP_GetButtonCaps(HIDReportType reportType, HIDP_BUTTON_CAPS* buttonCaps, ref ushort capsLength, byte* preParsedData);

		/// <summary>
		/// Function to retrieve value capabilities of the device.
		/// </summary>
		/// <param name="reportType">Type of report.</param>
		/// <param name="caps">Caps data.</param>
		/// <param name="capsLength">Number of caps items.</param>
		/// <param name="preparsedData">Preparsed data to evaluate.</param>
		/// <returns>The success/fail return code.</returns>
		[DllImport("Hid.dll", CharSet = CharSet.Unicode)]
		private static extern unsafe int HidP_GetValueCaps(HIDReportType reportType, HIDP_VALUE_CAPS* caps, ref ushort capsLength, byte* preparsedData);


		/// <summary>
		/// Function to retrieve the capabilities for a HID.
		/// </summary>
		/// <param name="preParsedData">The pointer to the block of memory containing the pre-parsed HID data.</param>
		/// <returns>The capabilities for the device.</returns>
		public static unsafe HIDP_CAPS GetHIDCaps(GorgonPointer preParsedData)
		{
			HIDP_CAPS caps;

			byte* data = (byte*)preParsedData.Address;

			int retVal = HidP_GetCaps(data, &caps);

			if (retVal != HidStatusSuccess)
			{
				throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, HidInvalidPreparsedData));
			}

			return caps;
		}

		/// <summary>
		/// Function to retrieve the number of buttons on the joystick.
		/// </summary>
		/// <param name="preParsedData">The pointer to the block of memory containing the pre-parsed HID data.</param>
		/// <param name="caps">The HID caps for the device.</param>
		/// <returns>The number of buttons on the joystick.</returns>
		public static unsafe int GetJoystickButtonCount(GorgonPointer preParsedData, ref HIDP_CAPS caps)
		{
			byte* data = (byte*)preParsedData.Address;
			HIDP_BUTTON_CAPS* buttonCaps = stackalloc HIDP_BUTTON_CAPS[caps.NumberInputButtonCaps];
			int retVal = HidP_GetButtonCaps(HIDReportType.Input, buttonCaps, ref caps.NumberInputButtonCaps, data);
			if (retVal != HidStatusSuccess)
			{
				throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, HidInvalidPreparsedData));
			}

			return ((int)buttonCaps->Range.UsageMax - (int)buttonCaps->Range.UsageMin + 1);
		}

		/// <summary>
		/// Function to retrieve the ranges for joystick axes.
		/// </summary>
		/// <param name="preParsedData">The pre parsed HID data.</param>
		/// <param name="caps">Capabilities for the device.</param>
		/// <returns>An array of axis ranges.</returns>
		public static unsafe IReadOnlyList<HIDP_VALUE_CAPS> GetAxisRanges(GorgonPointer preParsedData, ref HIDP_CAPS caps)
		{
			byte* data = (byte*)preParsedData.Address;
			var result = new HIDP_VALUE_CAPS[caps.NumberInputValueCaps];
			HIDP_VALUE_CAPS* valueCaps = stackalloc HIDP_VALUE_CAPS[caps.NumberInputValueCaps];

			int retVal = HidP_GetValueCaps(HIDReportType.Input, valueCaps, ref caps.NumberInputValueCaps, data);

			if (retVal != HidStatusSuccess)
			{
				throw new Win32Exception(string.Format(Resources.GORINP_RAW_ERR_CANNOT_READ_DEVICE_DATA, HidInvalidPreparsedData));
			}

			fixed (HIDP_VALUE_CAPS* resultPtr = &result[0])
			{
				DirectAccess.MemoryCopy(resultPtr, valueCaps, DirectAccess.SizeOf<HIDP_VALUE_CAPS>() * caps.NumberInputValueCaps);
			}

			return result;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes static members of the <see cref="HIDApi"/> class.
		/// </summary>
		static HIDApi()
		{
			Marshal.PrelinkAll(typeof(HIDApi));
		}
		#endregion
	}
}
