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

using System.Runtime.InteropServices;

namespace Gorgon.Native
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    #region Value types.
	/// <summary>
	/// Value type containing joystick position information.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	struct JOYINFO
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
	struct JOYINFOEX
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
	struct JOYCAPS
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

	#endregion
    // ReSharper restore InconsistentNaming
    // ReSharper restore FieldCanBeMadeReadOnly.Local
}
