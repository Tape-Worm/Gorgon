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

namespace GorgonLibrary.Input.Raw
{
	/// <summary>
	/// Windows Multimedia based joystick interface.
	/// </summary>
	internal class WMMJoystick
		: GorgonJoystick
	{
		#region Variables.
		private int _joystickID = 0;			// ID of the joystick.
		private bool _deviceLost = false;		// Flag to indicate that the device is lost.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return flag to indicate that the device is in a lost state.
		/// </summary>
		internal bool DeviceLost
		{
			get
			{
				if (!AllowBackground)
					return _deviceLost;
				else
					return false;
			}
			set
			{
				if (AllowBackground)
					_deviceLost = false;					
				else
					_deviceLost = value;
			}
		}

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
					_deviceLost = false;
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
		public override float X
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
		public override float Y
		{
			get 
			{
				return Axes[1];
			}
		}

		/// <summary>
		/// Property to return the z coordinate for the joystick.
		/// </summary>
		/// <value></value>
		public override float Z
		{
			get 
			{
				if (HasZAxis)
					return Axes[2];
				else
					return float.NaN;
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
				if (HasRudder)
					return Axes[3];
				else
					return float.NaN;
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
		/// <summary>
		/// Function to set dead zone limited axis data.
		/// </summary>
		/// <param name="axis">Axis to work with.</param>
		/// <param name="joyData">Joystick data for the axis.</param>
		private void DeadZoneAxis(int axis, int joyData)
		{
			float midrange = AxisRanges[axis].Minimum + (AxisRanges[axis].Range / 2);		// Mid range value.

			// The dead zone range needs to be within the range of the axis.
			if (!DeadZone[axis].Contains(joyData))
			{
				SetAxisData(axis, joyData);
				AxisDirection[axis] = JoystickDirections.Center;

				// Get direction.
				switch (axis)
				{
					case 0:
						if (joyData < midrange)
							AxisDirection[axis] |= JoystickDirections.Left;
						else
							AxisDirection[axis] |= JoystickDirections.Right;
						break;
					case 1:
						if (joyData < midrange)
							AxisDirection[axis] |= JoystickDirections.Up;
						else
							AxisDirection[axis] |= JoystickDirections.Down;
						break;
					default:
						if (joyData < midrange)
							AxisDirection[axis] |= JoystickDirections.LessThanCenter;
						else
							AxisDirection[axis] |= JoystickDirections.MoreThanCenter;
						break;
				}
			}
			else
			{
				SetAxisData(axis, midrange);
				AxisDirection[axis] = JoystickDirections.Center;
			}
		}

		/// <summary>
		/// Function to poll the joystick for data.
		/// </summary>
		protected override void PollJoystick()
		{
			JOYINFOEX joyInfo = new JOYINFOEX();		// Joystick information.
			int error = 0;								// Error code.

			if ((DeviceLost) && (!BoundWindow.Focused))
				return;
			else
				_deviceLost = false;

			if ((!Enabled) || (!Acquired))
				return;

			// Set up joystick info.
			joyInfo.Size = Marshal.SizeOf(typeof(JOYINFOEX));
			joyInfo.Flags = JoystickInfoFlags.ReturnButtons | JoystickInfoFlags.ReturnX | JoystickInfoFlags.ReturnY;

			if (HasZAxis)
				joyInfo.Flags |= JoystickInfoFlags.ReturnZ;
			if (AxisCount >= 4)
				joyInfo.Flags |= JoystickInfoFlags.ReturnAxis5;
			if (AxisCount >= 5)
				joyInfo.Flags |= JoystickInfoFlags.ReturnAxis6;
			if (HasRudder)
				joyInfo.Flags |= JoystickInfoFlags.ReturnRudder;
			if (HasPOV)
			{
				joyInfo.Flags |= JoystickInfoFlags.ReturnPOV;
				if (UnrestrictedPOV)
					joyInfo.Flags |= JoystickInfoFlags.ReturnPOVContinuousDegreeBearings;
			}
			error = Win32API.joyGetPosEx(_joystickID, ref joyInfo);
			if (error > 0)
				return;

			// Get X & Y values.
			DeadZoneAxis(0, joyInfo.X);
			DeadZoneAxis(1, joyInfo.Y);

			// Get additional axes.
			if (HasZAxis)
				DeadZoneAxis(2, joyInfo.Z);
			if (HasRudder)
				DeadZoneAxis(3, joyInfo.Rudder);
			if (AxisCount > 4)
				DeadZoneAxis(4, joyInfo.Axis5);
			if (AxisCount > 5)
				DeadZoneAxis(5, joyInfo.Axis6);

			if (HasPOV)
			{
				// Get POV data.
				POV[0] = joyInfo.POV;

				if (joyInfo.POV == -1)
					POVDirection[0] = JoystickDirections.Center;

				// Determine direction.
				if ((joyInfo.POV < 18000) && (joyInfo.POV > 9000))
					POVDirection[0] = JoystickDirections.Down | JoystickDirections.Right;
				if ((joyInfo.POV > 18000) && (joyInfo.POV < 27000))
					POVDirection[0] = JoystickDirections.Down | JoystickDirections.Left;
				if ((joyInfo.POV > 27000) && (joyInfo.POV < 36000))
					POVDirection[0] = JoystickDirections.Up | JoystickDirections.Left;
				if ((joyInfo.POV > 0) && (joyInfo.POV < 9000))
					POVDirection[0] = JoystickDirections.Up | JoystickDirections.Right;

				if (joyInfo.POV == 18000)
					POVDirection[0] = JoystickDirections.Down;
				if (joyInfo.POV == 0)
					POVDirection[0] = JoystickDirections.Up;
				if (joyInfo.POV == 9000)
					POVDirection[0] = JoystickDirections.Right;
				if (joyInfo.POV == 27000)
					POVDirection[0] = JoystickDirections.Left;
			}

			// Update buttons.
			for (int i = 0; i < ButtonCount; i++)
			{
				if ((joyInfo.Buttons & (JoystickButtons)(1 << i)) != 0)
					SetButtonValue(i, true);
				else
					SetButtonValue(i, false);
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
		internal WMMJoystick(GorgonRawInputFactory owner, int ID, string name, Control boundWindow)
			: base(owner, name, boundWindow)
		{
			JOYCAPS caps = default(JOYCAPS);		// Joystick capabilities.
			int threshold = 0;
			int error = 0;

			error = Win32API.joyGetDevCaps(ID, ref caps, Marshal.SizeOf(typeof(JOYCAPS)));

			if (error != 0)
				throw new GorgonException(GorgonResult.DriverError, "Cannot create the joystick interface.");

			error = Win32API.joyGetThreshold(ID, out threshold);

			if (error != 0)
				throw new GorgonException(GorgonResult.DriverError, "Cannot create the joystick interface.");
			
			// Get joystick info.
			_joystickID = ID;
			AxisCount = (int)caps.AxisCount;
			AxisRanges = new GorgonMinMax[AxisCount];

			// Initialize the ranges.
			for (int i = 0; i < AxisCount; i++)
				AxisRanges[i] = GorgonMinMax.Empty;

			// X axis range.
			AxisRanges[0] = new GorgonMinMax((int)caps.MinimumX, (int)caps.MaximumX);
			// Y axis range.
			AxisRanges[1] = new GorgonMinMax((int)caps.MinimumY, (int)caps.MaximumY);
			// Buttons.
			ButtonCount = (int)caps.ButtonCount;

			// Manufacturuer/product.
			ManufacturerID = caps.ManufacturerID;
			ProductID = caps.ProductID;

			// Determine capabilities.
			if ((caps.Capabilities & JoystickCapabilities.HasRudder) != 0)
			{
				AxisRanges[3] = new GorgonMinMax((int)caps.MinimumRudder, (int)caps.MaximumRudder);
				HasRudder = true;
			}
			if ((caps.Capabilities & JoystickCapabilities.HasPOV) != 0)
			{
				HasPOV = true;
				if ((caps.Capabilities & JoystickCapabilities.POVContinuousDegreeBearings) != 0)
					UnrestrictedPOV = true;
				if ((caps.Capabilities & JoystickCapabilities.POV4Directions) != 0)
					POVHas4Directions = true;
			}
			if ((caps.Capabilities & JoystickCapabilities.HasZ) != 0)
			{
				HasZAxis = true;
				AxisRanges[2] = new GorgonMinMax((int)caps.MinimumZ, (int)caps.MaximumZ);
			}
			if ((caps.Capabilities & JoystickCapabilities.HasU) != 0)
				AxisRanges[4] = new GorgonMinMax((int)caps.Axis5Minimum, (int)caps.Axis5Maximum);
			if ((caps.Capabilities & JoystickCapabilities.HasV) != 0)
				AxisRanges[5] = new GorgonMinMax((int)caps.Axis6Minimum, (int)caps.Axis6Maximum);

			// Create value arrays.
			Axes = new float[AxisCount];
			AxisDirection = new JoystickDirections[AxisCount];
			DeadZone = new GorgonMinMax[AxisCount];
			for (int i = 0; i < AxisCount; i++)
			{
				AxisDirection[i] = JoystickDirections.Center;
				DeadZone[i] = GorgonMinMax.Empty;
			}
			POVDirection = new JoystickDirections[1];
			POVCount = 1;
			POV = new int[1];
			POVDirection[0] = JoystickDirections.Center;			
			Button = new bool[ButtonCount];

			Gorgon.Log.Print("Windows multimedia joystick device ID 0x{0} interface created.", GorgonLoggingLevel.Verbose, ID.ToString("x").PadLeft(8, '0'));
		}
		#endregion
	}
}
