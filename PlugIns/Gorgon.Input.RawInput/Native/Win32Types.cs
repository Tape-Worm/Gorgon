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
// Created: Friday, June 24, 2011 10:05:05 AM
// 
#endregion

using System;
using System.Runtime.InteropServices;

namespace Gorgon.Native
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    #region Value types.
	/// <summary>
	/// HID device info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct RID_DEVICE_INFO_HID
	{
		/// <summary>
		/// Vendor
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]
		public int dwVendorId;
		/// <summary>
		/// Product
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]
		public int dwProductId;
		/// <summary>
		/// Version
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]
		public int dwVersionNumber;
		/// <summary>
		/// Usage page.
		/// </summary>
		[MarshalAs(UnmanagedType.U2)]
		public ushort usUsagePage;
		/// <summary>
		/// Usage flag.
		/// </summary>
		[MarshalAs(UnmanagedType.U2)]
		public ushort usUsage;
	}

	/// <summary>
	/// Keyboard device information.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct RID_DEVICE_INFO_KEYBOARD
	{
		/// <summary>
		/// Type.
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]
		public int dwType;
		/// <summary>
		/// Subtype.
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]
		public int dwSubType;
		/// <summary>
		/// Keyboard mode.
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]		
		public int dwKeyboardMode;
		/// <summary>
		/// Number of function keys.
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]
		public int dwNumberOfFunctionKeys;
		/// <summary>
		/// Number of indicators.
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]
		public int dwNumberOfIndicators;
		/// <summary>
		/// Number of total keys.
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]
		public int dwNumberOfKeysTotal;
	}

	/// <summary>
	/// Mouse device info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct RID_DEVICE_INFO_MOUSE
	{
		/// <summary>
		/// Mouse ID.
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]
		public int dwId;
		/// <summary>
		/// Number of buttons.
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]
		public int dwNumberOfButtons;
		/// <summary>
		/// Sample rate.
		/// </summary>
		[MarshalAs(UnmanagedType.U4)]
		public int dwSampleRate;
		/// <summary>
		/// Flag to indicate a horizontal wheel.
		/// </summary>
		[MarshalAs(UnmanagedType.Bool)]
		public bool fHasHorizontalWheel;
	}

	/// <summary>
	/// Device information.
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	internal struct RID_DEVICE_INFO
	{
		[FieldOffset(0)]
		public int cbSize;
		[FieldOffset(4)]
		public RawInputType dwType;
		[FieldOffset(8)]
		public RID_DEVICE_INFO_MOUSE mouse;
		[FieldOffset(8)]
		public RID_DEVICE_INFO_KEYBOARD keyboard;
		[FieldOffset(8)]
		public RID_DEVICE_INFO_HID hid;
	}
	
	/// <summary>
	/// Value type containing joystick position information.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct JOYINFO
	{
		/// <summary>X axis.</summary>
		public int X;
		/// <summary>Y axis.</summary>
		public int Y;
		/// <summary>Z axis.</summary>
		public int Z;
		/// <summary>State of buttons.</summary>
		public JoystickButton Buttons;
	}

	/// <summary>
	/// Value type containing joystick position information.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct JOYINFOEX
	{
		/// <summary>Size of structure, in bytes.</summary>
		public int Size;
		/// <summary>Flags to indicate what information is valid for the device.</summary>
		public JoystickInfoFlags Flags;
		/// <summary>X axis.</summary>
		public uint X;
		/// <summary>Y axis.</summary>
		public uint Y;
		/// <summary>Z axis.</summary>
		public uint Z;
		/// <summary>Rudder position.</summary>
		public uint Rudder;
		/// <summary>5th axis position.</summary>
		public uint Axis5;
		/// <summary>6th axis position.</summary>
		public uint Axis6;
		/// <summary>State of buttons.</summary>
		public JoystickButton Buttons;
		/// <summary>Currently pressed button.</summary>
		public uint ButtonNumber;
		/// <summary>Angle of the POV hat, in degrees (0 - 35900, divide by 100 to get 0 - 359 degrees.</summary>
		public uint POV;
		/// <summary>Reserved.</summary>
		int Reserved1;
		/// <summary>Reserved.</summary>
		int Reserved2;
	}

	/// <summary>
	/// Value type containing joystick capabilities.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	internal struct JOYCAPS
	{
		/// <summary>Manufacturer ID.</summary>
		public ushort ManufacturerID;
		/// <summary>Product ID.</summary>
		public ushort ProductID;
		/// <summary>Joystick name.</summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string Name;
		/// <summary>Minimum X coordinate.</summary>
		public uint MinimumX;
		/// <summary>Maximum X coordinate.</summary>
		public uint MaximumX;
		/// <summary>Minimum Y coordinate.</summary>
		public uint MinimumY;
		/// <summary>Maximum Y coordinate.</summary>
		public uint MaximumY;
		/// <summary>Minimum Z coordinate.</summary>
		public uint MinimumZ;
		/// <summary>Maximum Z coordinate.</summary>
		public uint MaximumZ;
		/// <summary>Number of buttons on the joystick.</summary>
		public uint ButtonCount;
		/// <summary>Smallest polling frequency (used by joySetCapture).</summary>
		public uint MinimumPollingFrequency;
		/// <summary>Largest polling frequency (used by joySetCapture).</summary>
		public uint MaximumPollingFrequency;
		/// <summary>Minimum rudder value.</summary>
		public uint MinimumRudder;
		/// <summary>Maximum rudder value.</summary>
		public uint MaximumRudder;
		/// <summary>Minimum 5th axis value.</summary>
		public uint Axis5Minimum;
		/// <summary>Maximum 5th axis value.</summary>
		public uint Axis5Maximum;
		/// <summary>Minimum 6th axis value.</summary>
		public uint Axis6Minimum;
		/// <summary>Maximum 6th axis value.</summary>
		public uint Axis6Maximum;
		/// <summary>Joystick capabilities.</summary>
		public JoystickCaps Capabilities;
		/// <summary>Maxmimum number of axes for the joystick.</summary>
		public uint MaximumAxes;
		/// <summary>Number of axes on the joystick.</summary>
		public uint AxisCount;
		/// <summary>Maximum buttons for the device.</summary>
		public uint MaximumButtons;
		/// <summary>Registry key for the joystick.</summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string RegistryKey;
		/// <summary>Driver name for the joystick.</summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string DriverName;
	}

	/// <summary>
	/// Value type for raw input device list.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct RAWINPUTDEVICELIST
	{
		/// <summary>Device handle.</summary>
		public IntPtr Device;
		/// <summary>Device type.</summary>
		public RawInputType DeviceType;
	}

	/// <summary>
	/// Value type for raw input devices.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct RAWINPUTDEVICE
	{
		/// <summary>Top level collection Usage page for the raw input device.</summary>
		public HIDUsagePage UsagePage;
		/// <summary>Top level collection Usage for the raw input device. </summary>
		public ushort Usage;
		/// <summary>Mode flag that specifies how to interpret the information provided by UsagePage and Usage.</summary>
		public RawInputDeviceFlags Flags;
		/// <summary>Handle to the target device. If NULL, it follows the keyboard focus.</summary>
		public IntPtr WindowHandle;
	}

	/// <summary>
	/// Value type for a raw input header.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct RAWINPUTHEADER
	{
		/// <summary>Type of device the input is coming from.</summary>
		public RawInputType Type;
		/// <summary>Size of the packet of data.</summary>
		public int Size;
		/// <summary>Handle to the device sending the data.</summary>
		public IntPtr Device;
		/// <summary>wParam from the window message.</summary>
		public IntPtr wParam;
	}

	/// <summary>
	/// Value type for raw input from a mouse.
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	internal struct RAWINPUTMOUSE
	{
		/// <summary>Flags for the event.</summary>
		[FieldOffset(0)]
		public RawMouseFlags Flags;
        /// <summary>Flags for the event.</summary>
        [FieldOffset(4)]
        public RawMouseButtons ButtonFlags;
        /// <summary>If the mouse wheel is moved, this will contain the delta amount.</summary>
        [FieldOffset(6)]
        public ushort ButtonData;
        /// <summary>Raw button data.</summary>
        [FieldOffset(8)]
		public uint RawButtons;
		/// <summary>Relative direction of motion, depending on flags.</summary>
        [FieldOffset(12)]
		public int LastX;
		/// <summary>Relative direction of motion, depending on flags.</summary>
        [FieldOffset(16)]
		public int LastY;
		/// <summary>Extra information.</summary>
        [FieldOffset(20)]
		public uint ExtraInformation;
	}

	/// <summary>
	/// Value type for raw input from a keyboard.
	/// </summary>	
	[StructLayout(LayoutKind.Explicit)]
	internal struct RAWINPUTKEYBOARD
	{
		/// <summary>Scan code for key depression.</summary>
        [FieldOffset(0)]
		public short MakeCode;
		/// <summary>Scan code information.</summary>
        [FieldOffset(2)]
		public RawKeyboardFlags Flags;
		/// <summary>Reserved.</summary>
        [FieldOffset(4)]
		public short Reserved;
		/// <summary>Virtual key code.</summary>
        [FieldOffset(6)]
		public VirtualKeys VirtualKey;
		/// <summary>Corresponding window message.</summary>
        [FieldOffset(8)]
		public WindowMessages Message;
		/// <summary>Extra information.</summary>
        [FieldOffset(12)]
		public int ExtraInformation;
	}

	/// <summary>
	/// Value type for raw input from a HID.
	/// </summary>	
	[StructLayout(LayoutKind.Explicit)]
	internal struct RAWINPUTHID
	{
		/// <summary>Size of the HID data in bytes.</summary>
		[FieldOffset(0)]
		public int Size;
		/// <summary>Number of HID in Data.</summary>
        [FieldOffset(4)]
		public int Count;
		/// <summary>HID data.</summary>
        [FieldOffset(8)]
		IntPtr Data;
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct RAWINPUT_UNION
	{
		/// <summary>Mouse raw input data.</summary>
		[FieldOffset(0)]
		public RAWINPUTMOUSE Mouse;
		/// <summary>Keyboard raw input data.</summary>
		[FieldOffset(0)]
		public RAWINPUTKEYBOARD Keyboard;
		/// <summary>HID raw input data.</summary>
		[FieldOffset(0)]
		public RAWINPUTHID HID;
	}

	/// <summary>
	/// Value type for raw input.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct RAWINPUT
	{
		/// <summary>Header for the data.</summary>
		public RAWINPUTHEADER Header;
		/// <summary>Union containing device data.</summary>
		public RAWINPUT_UNION Union;
	}		
	#endregion
    // ReSharper restore InconsistentNaming
    // ReSharper restore FieldCanBeMadeReadOnly.Local
}
