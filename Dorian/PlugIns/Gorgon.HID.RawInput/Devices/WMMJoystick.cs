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
// Created: Friday, June 24, 2011 10:02:29 AM
// 
#endregion

using System.Runtime.InteropServices;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Win32;
using GorgonLibrary.Math;
using GorgonLibrary.HID;

namespace GorgonLibrary.HID.RawInput
{
	/// <summary>
	/// Windows Multimedia based joystick interface.
	/// </summary>
	internal class WMMJoystick
		: GorgonJoystick
	{
		#region Variables.
		private int _joystickID = 0;			// ID of the joystick.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the device is acquired or not.
		/// </summary>		
		public override bool Acquired
		{
			get
			{
				return base.Acquired;
			}
			set
			{
				base.Acquired = value;

				if (value)
					DeviceLost = false;
			}
		}

		/// <summary>
		/// Property to set or return whether the window has exclusive access or not.  For joysticks, this is always TRUE.
		/// </summary>		
		public override bool Exclusive
		{
			get
			{
				return true;
			}
			set
			{				
			}
		}

		/// <summary>
		/// Property to return the x coordinate for the joystick.
		/// </summary>
		/// <value></value>
		public override int X
		{
			get 
			{
				return Axes[0];
			}
		}

		/// <summary>
		/// Property to return the y coordinate for the joystick.
		/// </summary>
		/// <value></value>
		public override int Y
		{
			get 
			{
				return Axes[1];
			}
		}

		/// <summary>
		/// Property to return the secondary X axis for the joystick (if applicable).
		/// </summary>
		public override int SecondaryX
		{
			get
			{
				if (Capabilities.AxisCount > 3)
					return Axes[4];
				else
					return 0;
			}
		}

		/// <summary>
		/// Property to return the secondary Y axis for the joystick (if applicable).
		/// </summary>
		public override int SecondaryY
		{
			get
			{
				if (Capabilities.AxisCount > 4)
					return Axes[5];
				else
					return 0;
			}
		}


		/// <summary>
		/// Property to return the z coordinate for the joystick.
		/// </summary>
		/// <value></value>
		public override int ThrottleZ
		{
			get 
			{
				if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsThrottle) == JoystickCapabilityFlags.SupportsThrottle)
					return Axes[2];
				else
					return 0;
			}
		}

		/// <summary>
		/// Property to return the rudder coordinate for the joystick.
		/// </summary>
		/// <value></value>
		public override float Rudder
		{
			get 
			{
				if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsRudder) == JoystickCapabilityFlags.SupportsRudder)
					return Axes[3];
				else
					return 0;
			}
		}

		/// <summary>
		/// Property to return whether the joystick is connected.
		/// </summary>
		public override bool IsConnected
		{
			get
			{
				JOYINFO info = new JOYINFO();		// Joystick information.
				
				return Win32API.joyGetPos(_joystickID, ref info) == 0;
			}
		}
		#endregion

		#region Methods.
		private JOYINFOEX GetWin32JoystickData()
		{
			JOYINFOEX joyInfo = new JOYINFOEX();		// Joystick information.
			int error = 0;								// Error code.
			
			// Set up joystick info.
			joyInfo.Size = Marshal.SizeOf(typeof(JOYINFOEX));
			joyInfo.Flags = JoystickInfoFlags.ReturnButtons | JoystickInfoFlags.ReturnX | JoystickInfoFlags.ReturnY;

			// Determine which data we want to return.
			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsThrottle) == JoystickCapabilityFlags.SupportsThrottle)
				joyInfo.Flags |= JoystickInfoFlags.ReturnZ;
			if (Capabilities.AxisCount > 4)
				joyInfo.Flags |= JoystickInfoFlags.ReturnAxis5;
			if (Capabilities.AxisCount > 5)
				joyInfo.Flags |= JoystickInfoFlags.ReturnAxis6;
			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsRudder) == JoystickCapabilityFlags.SupportsRudder)
				joyInfo.Flags |= JoystickInfoFlags.ReturnRudder;
			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsPOV) == JoystickCapabilityFlags.SupportsPOV)
			{
				joyInfo.Flags |= JoystickInfoFlags.ReturnPOV;
				if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsContinuousPOV) == JoystickCapabilityFlags.SupportsContinuousPOV)
					joyInfo.Flags |= JoystickInfoFlags.ReturnPOVContinuousDegreeBearings;
			}

			error = Win32API.joyGetPosEx(_joystickID, ref joyInfo);
			if (error > 0)
				return default(JOYINFOEX);

			return joyInfo;
		}

		/// <summary>
		/// Function to parse the POV data.
		/// </summary>
		/// <param name="POVdata">POV data to parse.</param>
		private void GetPOVData(int POVdata)
		{
			// Get POV data.
			POV[0] = POVdata;

			if (POVdata == -1)
				POVDirection[0] = JoystickDirections.Center;

			// Determine direction.
			if ((POVdata < 18000) && (POVdata > 9000))
				POVDirection[0] = JoystickDirections.Down | JoystickDirections.Right;
			if ((POVdata > 18000) && (POVdata < 27000))
				POVDirection[0] = JoystickDirections.Down | JoystickDirections.Left;
			if ((POVdata > 27000) && (POVdata < 36000))
				POVDirection[0] = JoystickDirections.Up | JoystickDirections.Left;
			if ((POVdata > 0) && (POVdata < 9000))
				POVDirection[0] = JoystickDirections.Up | JoystickDirections.Right;

			if (POVdata == 18000)
				POVDirection[0] = JoystickDirections.Down;
			if (POVdata == 0)
				POVDirection[0] = JoystickDirections.Up;
			if (POVdata == 9000)
				POVDirection[0] = JoystickDirections.Right;
			if (POVdata == 27000)
				POVDirection[0] = JoystickDirections.Left;
		}

		/// <summary>
		/// Function to initalize the data for the joystick.
		/// </summary>
		protected override void InitializeData()
		{
			JOYCAPS caps = default(JOYCAPS);									// Joystick capabilities.
			int error = 0;
			GorgonMinMax[] axisRanges = null;									// Axis ranges.
			int axisCount = 0;													// Axis count.
			JoystickCapabilityFlags capsFlags = JoystickCapabilityFlags.None;	// Extra capability flags.

			error = Win32API.joyGetDevCaps(_joystickID, ref caps, Marshal.SizeOf(typeof(JOYCAPS)));

			if (error != 0)
				throw new GorgonException(GorgonResult.DriverError, "Cannot create the joystick interface.");

			axisCount = 2;

			// Gather device info.
			if ((caps.Capabilities & JoystickCaps.HasZ) == JoystickCaps.HasZ)
			{
				capsFlags |= JoystickCapabilityFlags.SupportsThrottle;
				axisCount++;
			}
			if ((caps.Capabilities & JoystickCaps.HasRudder) == JoystickCaps.HasRudder)
			{
				capsFlags |= JoystickCapabilityFlags.SupportsRudder;
				axisCount++;
			}
			if ((caps.Capabilities & JoystickCaps.HasU) == JoystickCaps.HasU)
				axisCount++;
			if ((caps.Capabilities & JoystickCaps.HasV) == JoystickCaps.HasV)
				axisCount++;

			if ((caps.Capabilities & JoystickCaps.HasPOV))
			{
				capsFlags |= JoystickCapabilityFlags.SupportsPOV;
				if ((caps.Capabilities & JoystickCaps.POV4Directions) == JoystickCaps.POV4Directions)
					capsFlags |= JoystickCapabilityFlags.SupportsDiscreetPOV;
				if ((caps.Capabilities & JoystickCaps.POVContinuousDegreeBearings) == JoystickCaps.POVContinuousDegreeBearings)
					capsFlags |= JoystickCapabilityFlags.SupportsContinuousPOV;
			}				

			axisRanges = new GorgonMinMax[caps.MaximumAxes];
			// X axis range.
			axisRanges[0] = new GorgonMinMax((int)caps.MinimumX, (int)caps.MaximumX);
			// Y axis range.
			axisRanges[1] = new GorgonMinMax((int)caps.MinimumY, (int)caps.MaximumY);
			// Throttle (Z) axis range.
			if (axisCount > 2)
				axisRanges[2] = new GorgonMinMax((int)caps.MinimumZ, (int)caps.MaximumZ);
			// Rudder range.
			if (axisCount > 3)
				axisRanges[3] = new GorgonMinMax((int)caps.MinimumRudder, (int)caps.MaximumRudder);
			// U axis range.
			if (axisCount > 4)
				axisRanges[4] = new GorgonMinMax((int)caps.Axis5Minimum, (int)caps.Axis5Maximum);
			// V axis range.
			if (axisCount > 5)
				axisRanges[5] = new GorgonMinMax((int)caps.Axis6Minimum, (int)caps.Axis6Maximum);

			// Define joystick capabilities.
			this.SetJoystickCapabilities((int)caps.ButtonCount, 1, axisRanges, new GorgonMinMax[0], caps.ManufacturerID, caps.ProductID, capsFlags);
		}

		/// <summary>
		/// Function to poll the joystick for data.
		/// </summary>
		protected override void PollJoystick()
		{
			JOYINFOEX joyInfo = new JOYINFOEX();		// Joystick information.

			// Reset the data.
			SetDefaults();

			joyInfo = GetWin32JoystickData();

			// Get axis data.
			SetAxisData(0, joyInfo.X, JoystickDirections.Horizontal);
			SetAxisData(1, joyInfo.Y, JoystickDirections.Vertical);

			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsThrottle) == JoystickCapabilityFlags.SupportsThrottle)
				SetAxisData(2, joyInfo.Z, JoystickDirections.Vector);
			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsRudder) == JoystickCapabilityFlags.SupportsRudder)
				DeadZoneAxis(3, joyInfo.Rudder);
			if (Capabilities.AxisCount > 4)
				DeadZoneAxis(4, joyInfo.Axis5);
			if (Capabilities.AxisCount > 5)
				DeadZoneAxis(5, joyInfo.Axis6);
						
			// Get POV data.
			if ((Capabilities.ExtraCapabilities & JoystickCapabilityFlags.SupportsPOV) == JoystickCapabilityFlags.SupportsPOV)
				GetPOVData(joyInfo.POV);

			// Update buttons.
			for (int i = 0; i < Capabilities.ButtonCount; i++)
			{
				if ((joyInfo.Buttons & (JoystickButtons)(1 << i)) != 0)
					SetButtonValue(i, KeyState.Down);
				else
					SetButtonValue(i, KeyState.Down);
			}
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
		}

		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected override void BindDevice()
		{
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="WMMJoystick"/> class.
		/// </summary>
		/// <param name="owner">The input factory that owns this device.</param>
		/// <param name="ID">The ID of the joystick.</param>
		/// <param name="name">The name of the joystick.</param>
		/// <param name="boundWindow">The window to bind the joystick with.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</remarks>
		internal WMMJoystick(GorgonRawInputDeviceFactory owner, int ID, string name, Control boundWindow)
			: base(owner, name, boundWindow)
		{
			_joystickID = ID;
			InitializeData();
			Gorgon.Log.Print("Windows multimedia joystick device ID 0x{0} interface created.", GorgonLoggingLevel.Verbose, ID.ToString("x").PadLeft(8, '0'));
		}
		#endregion
	}
}
