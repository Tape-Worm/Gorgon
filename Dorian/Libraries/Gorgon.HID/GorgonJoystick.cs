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
// Created: Friday, June 24, 2011 10:05:35 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using GorgonLibrary.Math;
using GorgonLibrary.Collections;

namespace GorgonLibrary.HID
{
	/// <summary>
	/// Enumeration for joystick axis directions.
	/// </summary>
	[Flags()]
	public enum JoystickDirections
	{
		/// <summary>
		/// Axis is centered.
		/// </summary>
		Center = 0,
		/// <summary>
		/// Axis is up.
		/// </summary>
		Up = 1,
		/// <summary>
		/// Axis is down.
		/// </summary>
		Down = 2,
		/// <summary>
		/// Axis is left.
		/// </summary>
		Left = 4,
		/// <summary>
		/// Axis is right.
		/// </summary>
		Right = 8,
		/// <summary>
		/// Axis is less than center position.
		/// </summary>
		LessThanCenter = 16,
		/// <summary>
		/// Axis is greater than center position.
		/// </summary>
		MoreThanCenter = 32,
		/// <summary>
		/// Axis is a horizontal axis.
		/// </summary>
		Horizontal = 64,
		/// <summary>
		/// Axis is a vertical axis.
		/// </summary>
		Vertical = 128,
		/// <summary>
		/// Axis is a vector.
		/// </summary>
		Vector = 256
	}

	/// <summary>
	/// Flags to indicate which extra capabilties are supported by the joystick.
	/// </summary>
	[Flags]
	public enum JoystickCapabilityFlags
	{
		/// <summary>
		/// No extra capabilities.  This flag is mutally exclusive.
		/// </summary>
		None = 0,
		/// <summary>
		/// Supports point of view controls.
		/// </summary>
		SupportsPOV = 1,
		/// <summary>
		/// Supports continuous degree bearings for the point of view controls.
		/// </summary>
		SupportsContinuousPOV = 2,
		/// <summary>
		/// Supports discreet values (up, down, left, right and center) for the point of view controls.
		/// </summary>
		SupportsDiscreetPOV = 4,
		/// <summary>
		/// Supports a rudder control.
		/// </summary>
		SupportsRudder = 8,
		/// <summary>
		/// Supports a throttle (or Z-axis) control.
		/// </summary>
		SupportsThrottle = 16,
		/// <summary>
		/// Supports vibration.
		/// </summary>
		SupportsVibration = 32
	}

	/// <summary>
	/// Object that will represent joystick data.
	/// </summary>
	public abstract class GorgonJoystick
		: GorgonInputDevice
	{
		#region Classes.
		/// <summary>
		/// Contains the capabilities of the joystick.
		/// </summary>
		public class JoystickCapabilities
		{
			#region Properties.
			/// <summary>
			/// Property to set or return the X axis mid range.
			/// </summary>
			internal int XAxisMidRange
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the Y axis mid range.
			/// </summary>
			internal int YAxisMidRange
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the throttle axis mid range.
			/// </summary>
			internal int ThrottleAxisMidRange
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the rudder axis mid range.
			/// </summary>
			internal int RudderAxisMidRange
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the secondary X axis mid range.
			/// </summary
			internal int SecondaryXAxisMidRange
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the secondary Y axis mid range.
			/// </summary>
			internal int SecondaryYAxisMidRange
			{
				get;
				set;
			}

			/// <summary>
			/// Property to return the extra capabilities of this device.
			/// </summary>
			public JoystickCapabilityFlags ExtraCapabilities
			{
				get;
				internal set;
			}

			/// <summary>
			/// Property to return the number of buttons for the device.
			/// </summary>
			public int ButtonCount
			{
				get;
				internal set;
			}

			/// <summary>
			/// Property to return the number of axes for the device.
			/// </summary>
			public int AxisCount
			{
				get;
				internal set;
			}

			/// <summary>
			/// Property to return the X axis range.
			/// </summary>
			public GorgonMinMax XAxisRange
			{
				get;
				internal set;
			}

			/// <summary>
			/// Property to return the Y axis range.
			/// </summary>
			public GorgonMinMax YAxisRange
			{
				get;
				internal set;
			}

			/// <summary>
			/// Property to return the secondary Y axis range.
			/// </summary>
			public GorgonMinMax SecondaryYAxisRange
			{
				get;
				internal set;
			}

			/// <summary>
			/// Property to return the secondary X axis range.
			/// </summary>
			public GorgonMinMax SecondaryXAxisRange
			{
				get;
				internal set;
			}

			/// <summary>
			/// Property to return the throttle axis range.
			/// </summary>
			public GorgonMinMax ThrottleAxisRange
			{
				get;
				internal set;
			}

			/// <summary>
			/// Property to return the rudder axis range.
			/// </summary>
			public GorgonMinMax RudderAxisRange
			{
				get;
				internal set;
			}

			/// <summary>
			/// Property to return the number of vibration motor controls for the device.
			/// </summary>
			public int VibrationMotorCount
			{
				get
				{
					return VibrationMotorRanges.Count;
				}
			}

			/// <summary>
			/// Property to return the vibration motor ranges.
			/// </summary>
			public IList<GorgonMinMax> VibrationMotorRanges
			{
				get;
				internal set;
			}

			/// <summary>
			/// Property to return the manufacturer ID.
			/// </summary>
			public int ManufacturerID
			{
				get;
				internal set;
			}

			/// <summary>
			/// Property to return the product ID.
			/// </summary>
			public int ProductID
			{
				get;
				internal set;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="JoystickCapabilities"/> class.
			/// </summary>
			internal JoystickCapabilities()
			{
				VibrationMotorRanges = new GorgonMinMax[0];
				AxisCount = 0;
				ButtonCount = 0;
				XAxisRange = GorgonMinMax.Empty;
				YAxisRange = GorgonMinMax.Empty;
				SecondaryXAxisRange = GorgonMinMax.Empty;
				SecondaryYAxisRange = GorgonMinMax.Empty;
				ThrottleAxisRange = GorgonMinMax.Empty;
				RudderAxisRange = GorgonMinMax.Empty;
				ManufacturerID = 0;
				ProductID = 0;

				// Reset internal values.
				XAxisMidRange = 0;
				YAxisMidRange = 0;
				SecondaryYAxisMidRange = 0;
				SecondaryXAxisMidRange = 0;
				ThrottleAxisMidRange = 0;
				RudderAxisMidRange = 0;

				ExtraCapabilities = JoystickCapabilityFlags.None;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private IList<KeyState> _buttons = null;			// Button values.
		private bool _deviceLost = false;					// Flag to indicate that the device was in a lost state.
		private int _xAxis = 0;								// X axis value.
		private int _yAxis = 0;								// Y axis value.
		private int _throttleAxis = 0;						// Throttle axis value.
		private int _rudderAxis = 0;						// Rudder axis value.
		private int _xAxis2 = 0;							// Secondary X axis value.
		private int _yAxis2 = 0;							// Secondary Y axis value.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return flag to indicate that the device is in a lost state.
		/// </summary>
		protected bool DeviceLost
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
		/// Property to return the joystick capabilities.
		/// </summary>
		public JoystickCapabilities Capabilities
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the dead zone range.
		/// </summary>
		/// <remarks>The dead zone should be within the range of the same axis.  That is, if the axis range is 
		/// between 0..32767, then the dead zone range should be within 0..32767.  Likewise if the axis has a 
		/// range of -32767..32767 then the dead zone value should correspond to the range of -32767..32767.  
		/// <para>Please note that by setting the dead zone to the exact size of the axis range the joystick 
		/// will not register any movement because the entire range is considered 'dead'.</para></remarks>
		public IList<GorgonMinMax> DeadZone
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the direction that an axis is pointed towards.
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.HID.GorgonJoystick.DeadZone">DeadZone</see> 
		/// property.  If the axis position is within the dead-zone range only the 
		/// <see cref="GorgonLibrary.HID.JoystickDirections">JoystickDirections.Center</see> position is returned.</remarks>
		public IList<JoystickDirections> AxisDirection
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the point of view value for continuous bearing.
		/// </summary>
		public int POV
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the POV direction for discreet POV values.
		/// </summary>
		public JoystickDirections POVDirection
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return a button state.
		/// </summary>
		public IList<KeyState> Button
		{
			get
			{
				PollJoystick();
				return _buttons;
			}
			private set
			{
				_buttons = value;
			}
		}

		/// <summary>
		/// Property to return the x coordinate for the joystick.
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.HID.GorgonJoystick.DeadZone">DeadZone</see> 
		/// property.  Any values that fall within the dead zone range are ignored and as such only the mid-point 
		/// of the X axis range will be returned (center position of the axis).</remarks>
		public int X
		{
			get
			{
				GetJoystickState();
				return _xAxis;
			}
			protected set
			{
				if (Capabilities == null)
					return;
				_xAxis = DeadZoneValue(value, Capabilities.XAxisRange);
			}
		}

		/// <summary>
		/// Property to return the y coordinate for the joystick.
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.HID.GorgonJoystick.DeadZone">DeadZone</see> 
		/// property.  Any values that fall within the dead zone range are ignored and as such only the mid-point 
		/// of the Y axis range will be returned (center position of the axis).</remarks>
		public int Y
		{
			get
			{
				GetJoystickState();
				return _yAxis;
			}
			protected set
			{
				if (Capabilities == null)
					return;
				_yAxis = DeadZoneValue(value, Capabilities.YAxisRange);
			}
		}

		/// <summary>
		/// Property to return the throttle coordinate for the joystick.
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.HID.GorgonJoystick.DeadZone">DeadZone</see> 
		/// property.  Any values that fall within the dead zone range are ignored and as such only the mid-point 
		/// of the throttle axis range will be returned (center position of the axis).</remarks>
		public int Throttle
		{
			get
			{
				GetJoystickState();
				return _throttleAxis;
			}
			protected set
			{
				if (Capabilities == null)
					return;
				_throttleAxis = DeadZoneValue(value, Capabilities.ThrottleAxisRange);
			}
		}

		/// <summary>
		/// Property to return the secondary X axis for the joystick (if applicable).
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.HID.GorgonJoystick.DeadZone">DeadZone</see> 
		/// property.  Any values that fall within the dead zone range are ignored and as such only the mid-point 
		/// of the Z axis range will be returned (center position of the axis).</remarks>
		public abstract int SecondaryX
		{
			get
			{
				GetJoystickState();
				return _xAxis2;
			}
			protected set
			{
				if (Capabilities == null)
					return;
				_xAxis2 = DeadZoneValue(value, Capabilities.SecondaryXAxisRange);
			}
		}


		/// <summary>
		/// Property to return the secondary Y axis for the joystick (if applicable).
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.HID.GorgonJoystick.DeadZone">DeadZone</see> 
		/// property.  Any values that fall within the dead zone range are ignored and as such only the mid-point 
		/// of the Z axis range will be returned (center position of the axis).</remarks>
		public int SecondaryY
		{
			get
			{
				GetJoystickState();
				return _xAxis2;
			}
			protected set
			{
				if (Capabilities == null)
					return;
				_xAxis2 = DeadZoneValue(value, Capabilities.SecondaryXAxisRange);
			}
		}

		/// <summary>
		/// Property to return the rudder coordinate for the joystick.
		/// </summary>
		/// <remarks>This is affected by the <see cref="GorgonLibrary.HID.GorgonJoystick.DeadZone">DeadZone</see> 
		/// property.  Any values that fall within the dead zone range are ignored and as such only the mid-point 
		/// of the rudder range will be returned (center position of the axis).</remarks>
		public int Rudder
		{
			get
			{
				GetJoystickState();
				return _rudderAxis;
			}
			protected set
			{
				if (Capabilities == null)
					return;
				_rudderAxis = DeadZoneValue(value, Capabilities.RudderAxisRange);
			}
		}

		/// <summary>
		/// Property to return whether the joystick is connected.
		/// </summary>
		public bool IsConnected
		{
			get;
			protected set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to read the joystick state.
		/// </summary>
		private void GetJoystickState()
		{
			if (Capabilities == null)
			{
				ResetData();
				return;
			}
			if ((DeviceLost) && (!BoundWindow.Focused))
				return;
			else
				_deviceLost = false;

			if ((!Enabled) || (!Acquired))
				return;

			PollJoystick();
		}

		/// <summary>
		/// Function to apply a deadzone to a value.
		/// </summary>
		/// <param name="value">Value to dead zone.</param>
		/// <param name="deadZone">Dead zone range.</param>
		/// <returns>The actual axis data if it falls outside of the dead zone, or the center position value.</returns>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the joystick has not been initialized.</exception>
		private int DeadZoneValue(int value, GorgonMinMax deadZone)
		{
			if (Capabilities == null)
				throw new GorgonException(GorgonResult.NotInitialized, "The joystick is not initialized.");

			int midrange = range.Minimum + (range.Range / 2);		// Mid range value.

			// The dead zone range needs to be within the range of the axis.
			if (!deadZone.Contains(value))
				return value;

			return midrange;
		}

		/// <summary>
		/// Function to get an orientation for a value.
		/// </summary>
		/// <param name="value">Value to dead zone.</param>
		/// <param name="orientation">Orientation of the axis.</param>
		/// <returns>The actual axis data if it falls outside of the dead zone, or the center position value.</returns>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the joystick has not been initialized.</exception>
		private int GetDirection(int value, JoystickDirections orientation)
		{
			if (Capabilities == null)
				throw new GorgonException(GorgonResult.NotInitialized, "The joystick is not initialized.");

			int midrange = deadZone.Minimum + (deadZone.Range / 2);		// Mid range value.

			// The dead zone range needs to be within the range of the axis.
			if (!deadZone.Contains(value))
				return value;

			return midrange;
		}

		/// <summary>
		/// Function to set the pressed value for a specific button.
		/// </summary>
		/// <param name="index">Index of the button to set.</param>
		/// <param name="value">Value to set.</param>
		protected void SetButtonValue(int index, KeyState value)
		{
			_buttons[index] = value;
		}

		/// <summary>
		/// Function to poll the joystick for data.
		/// </summary>
		protected abstract void PollJoystick();

		/// <summary>
		/// Function to reset the data for the device.
		/// </summary>
		protected void ResetData()
		{
			Capabilities = new JoystickCapabilities();

			Button = new KeyState[0];
			SetDefaults();
		}

		/// <summary>
		/// Function to reset the various joystick axes and buttons and POV settings to default values.
		/// </summary>
		protected void SetDefaults()
		{
			_xAxis = 0;
			_yAxis = 0;
			_xAxis2 = 0;
			_yAxis2 = 0;
			_throttleAxis = 0;
			_rudderAxis = 0;
			POV = 0;
			POVDirection = JoystickDirections.Center;
		}

		/// <summary>
		/// Function to set the joystick capabilities.
		/// </summary>
		/// <param name="buttonCount">Number of buttons.</param>
		/// <param name="povCount">Point of view hat count.</param>
		/// <param name="axisRanges">List of ranges for each axis.</param>
		/// <param name="vibrationMotorRanges">List of ranges for each vibration motor.</param>
		/// <param name="manufacturerID">Manufacturer ID.</param>
		/// <param name="productID">Product ID.</param>
		/// <param name="extraCapabilities">Extra capabilities of the device.</param>
		protected void SetJoystickCapabilities(int buttonCount, int povCount, IList<GorgonMinMax> axisRanges, IList<GorgonMinMax> vibrationMotorRanges, int manufacturerID, int productID, JoystickCapabilityFlags extraCapabilities)
		{
			_axisValues = new int[axisRanges.Count];
			AxisDirection = new JoystickDirections[axisRanges.Count];
			DeadZone = new GorgonMinMax[axisRanges.Count];
			Button = new KeyState[buttonCount];
			POV = new int[povCount];
			POVDirection = new JoystickDirections[povCount];

			Capabilities = new JoystickCapabilities(buttonCount, povCount, axisRanges, vibrationMotorRanges, manufacturerID, productID, extraCapabilities);

			SetDefaults();
		}

		/// <summary>
		/// Function to initalize the data for the joystick.
		/// </summary>
		protected abstract void InitializeData();
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonJoystick"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="deviceName">Name of the input device.</param>
		/// <param name="boundWindow">The window to bind this device with.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</remarks>
		protected GorgonJoystick(GorgonInputDeviceFactory owner, string deviceName, Control boundWindow)
			: base(owner, deviceName, boundWindow)
		{
			ResetData();
		}
		#endregion
	}
}
