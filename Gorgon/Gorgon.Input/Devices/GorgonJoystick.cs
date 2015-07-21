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
using Gorgon.Core;
using Gorgon.Input.Properties;

namespace Gorgon.Input
{
	/// <summary>
	/// Enumeration for point-of-view hat directions.
	/// </summary>
	[Flags]
	public enum POVDirection
	{
		/// <summary>
		/// Axis is centered.
		/// </summary>
		Center = 1,
		/// <summary>
		/// Axis is up.
		/// </summary>
		Up = 2,
		/// <summary>
		/// Axis is down.
		/// </summary>
		Down = 4,
		/// <summary>
		/// Axis is left.
		/// </summary>
		Left = 8,
		/// <summary>
		/// Axis is right.
		/// </summary>
		Right = 16
	}

	/// <summary>
	/// The state of a joystick button.
	/// </summary>
	public enum JoystickButtonState
	{
		/// <summary>
		/// The joystick button is not pressed.
		/// </summary>
		Up = 0,
		/// <summary>
		/// The joystick button is pressed.
		/// </summary>
		Down = 1
	}

	/// <summary>
	/// A joystick/game pad interface.
	/// </summary>
	/// <remarks>This is not like the other input interfaces in that the data in this object is a snapshot of its state and not polled automatically.  
	/// The user must call the <see cref="M:Gorgon.Input.GorgonJoystick.Poll">Poll</see> method in order to update the state of the device.
	/// <para>Note that while this object supports 6 axis devices, it will be limited to the number of axes present on the physical device, thus some values will always return 0.</para>
	/// </remarks>
	public abstract class GorgonJoystick
		: GorgonInputDevice
	{
		#region Classes.
		/// <summary>
		/// Defines the dead zone for a given axis.
		/// </summary>
		[Obsolete("Nope")]
		public class JoystickDeadZoneAxes
		{
			#region Properties.
			/// <summary>
			/// Property to set or return the dead zone for the X axis.
			/// </summary>
			public GorgonRange X
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the dead zone for the Y axis.
			/// </summary>
			public GorgonRange Y
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the dead zone for the secondary X axis.
			/// </summary>
			public GorgonRange SecondaryX
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the dead zone for the secondary Y axis.
			/// </summary>
			public GorgonRange SecondaryY
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the dead zone for the throttle axis.
			/// </summary>
			public GorgonRange Throttle
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the dead zone for the rudder axis.
			/// </summary>
			public GorgonRange Rudder
			{
				get;
				set;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="JoystickDeadZoneAxes"/> class.
			/// </summary>
			internal JoystickDeadZoneAxes()
			{
				X = GorgonRange.Empty;
				Y = GorgonRange.Empty;
				SecondaryX = GorgonRange.Empty;
				SecondaryY = GorgonRange.Empty;
				Throttle = GorgonRange.Empty;
				Rudder = GorgonRange.Empty;
			}
			#endregion
		}
		#endregion

		#region Variables.
		// Flag to indicate that the device was in a lost state.
		private bool _deviceLost;
		// List of mid range values for dead-zones.
		private IGorgonJoystickAxisValueList _midRanges;
	    #endregion

		#region Properties.
		/// <summary>
		/// Property to set or return flag to indicate that the device is in a lost state.
		/// </summary>
		protected bool DeviceLost
		{
			get
			{
				return !AllowBackground && _deviceLost;
			}
			set
			{
			    _deviceLost = !AllowBackground && value;
			}
		}

		/// <summary>
		/// Property to return the current direction for the point-of-view hat.
		/// </summary>
		/// <remarks>
		/// If the gaming device does not support a point-of-view axis, then this value will always return <see cref="Input.POVDirection.Center"/>.
		/// </remarks>
		public POVDirection POVDirection
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return information about the gaming device.
		/// </summary>
		public IGorgonJoystickInfo Info
		{
			get;
		}

		/// <summary>
		/// Property to return the list of dead zones for each axis.
		/// </summary>
		public IGorgonJoystickDeadZoneList DeadZones
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the list of axes available to this gaming device.
		/// </summary>
		public IGorgonJoystickAxisValueList Axis
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the point of view value for continuous bearing.
		/// </summary>
		/// <remarks>This will return an integer value of -1 for center, or 0 to 35999.  The user must divide by 100 to get the angle in degrees for a continuous bearing POV.</remarks>
		public int POV
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return a button state.
		/// </summary>
		public IList<JoystickButtonState> Button
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to apply a dead zone to a value.
		/// </summary>
		/// <param name="value">Value to dead zone.</param>
		/// <param name="deadZone">Dead zone range.</param>
		/// <param name="midRange">Mid point for the range.</param>
		/// <returns>The actual axis data if it falls outside of the dead zone, or the center position value.</returns>
		/// <exception cref="GorgonException">Thrown when the joystick has not been initialized.</exception>
		private static int DeadZoneValue(int value, GorgonRange deadZone, int midRange)
		{
		    // The dead zone range needs to be within the range of the axis.
		    if ((!deadZone.Contains(value)) || (deadZone == GorgonRange.Empty))
		    {
		        return value;
		    }

		    return midRange;
		}

		/// <summary>
		/// Function to create a list of axis values.
		/// </summary>
		private void CreateAxesValues()
		{
			var deadZones = new List<KeyValuePair<int, GorgonRange>>
			                {
								new KeyValuePair<int, GorgonRange>((int)JoystickAxis.XAxis, GorgonRange.Empty),
								new KeyValuePair<int, GorgonRange>((int)JoystickAxis.YAxis, GorgonRange.Empty)
			                };

			var midRanges = new List<KeyValuePair<int, int>>
			                {
				                new KeyValuePair<int, int>((int)JoystickAxis.XAxis,
				                                           (Info.AxisRanges[JoystickAxis.XAxis].Range / 2) - Info.AxisRanges[JoystickAxis.XAxis].Maximum),
				                new KeyValuePair<int, int>((int)JoystickAxis.YAxis,
				                                           (Info.AxisRanges[JoystickAxis.YAxis].Range / 2) - Info.AxisRanges[JoystickAxis.YAxis].Maximum)
			                };

			var axes = new List<KeyValuePair<int, int>>
			           {
				           new KeyValuePair<int, int>((int)JoystickAxis.XAxis, 0),
				           new KeyValuePair<int, int>((int)JoystickAxis.YAxis, 0)
			           };

			if ((Info.Capabilities & JoystickCapabilityFlags.SupportsSecondaryXAxis) == JoystickCapabilityFlags.SupportsSecondaryXAxis)
			{
				const int axis = (int)JoystickAxis.XAxis2;
				axes.Add(new KeyValuePair<int, int>(axis, 0));
				deadZones.Add(new KeyValuePair<int, GorgonRange>(axis, GorgonRange.Empty));
				midRanges.Add(new KeyValuePair<int, int>(axis,
				                                         (Info.AxisRanges[JoystickAxis.XAxis2].Range / 2) - Info.AxisRanges[JoystickAxis.XAxis2].Maximum));
			}

			if ((Info.Capabilities & JoystickCapabilityFlags.SupportsSecondaryYAxis) == JoystickCapabilityFlags.SupportsSecondaryYAxis)
			{
				const int axis = (int)JoystickAxis.YAxis2;
				axes.Add(new KeyValuePair<int, int>(axis, 0));
				deadZones.Add(new KeyValuePair<int, GorgonRange>(axis, GorgonRange.Empty));
				midRanges.Add(new KeyValuePair<int, int>(axis,
														 (Info.AxisRanges[JoystickAxis.YAxis2].Range / 2) - Info.AxisRanges[JoystickAxis.YAxis2].Maximum));
			}

			if ((Info.Capabilities & JoystickCapabilityFlags.SupportsThrottle) == JoystickCapabilityFlags.SupportsThrottle)
			{
				const int axis = (int)JoystickAxis.Throttle;
				axes.Add(new KeyValuePair<int, int>(axis, 0));
				deadZones.Add(new KeyValuePair<int, GorgonRange>(axis, GorgonRange.Empty));
			}

			if ((Info.Capabilities & JoystickCapabilityFlags.SupportsRudder) == JoystickCapabilityFlags.SupportsRudder)
			{
				const int axis = (int)JoystickAxis.Rudder;
				axes.Add(new KeyValuePair<int, int>(axis, 0));
				deadZones.Add(new KeyValuePair<int, GorgonRange>(axis, GorgonRange.Empty));
			}

			Axis = new GorgonJoystickAxisValueList(axes);
			DeadZones = new GorgonJoystickDeadZoneList(deadZones);
			_midRanges = new GorgonJoystickAxisValueList(midRanges);
		}

		/// <summary>
		/// Function to update the direction flag for the POV.
		/// </summary>
		private void UpdatePOVDirection()
		{
			if ((Info.Capabilities & JoystickCapabilityFlags.SupportsPOV) != JoystickCapabilityFlags.SupportsPOV)
			{
				return;
			}

			// Wrap POV if it's higher than 359.99 degrees.
			if (POV > 35999)
			{
				POV = -1;
			}
			
			// Get POV direction.
			if (POV != -1)
			{
				if ((POV < 18000) && (POV > 9000))
				{
					POVDirection = POVDirection.Down | POVDirection.Right;
				}
				if ((POV > 18000) && (POV < 27000))
				{
					POVDirection = POVDirection.Down | POVDirection.Left;
				}
				if ((POV > 27000) && (POV < 36000))
				{
					POVDirection = POVDirection.Up | POVDirection.Left;
				}
				if ((POV > 0) && (POV < 9000))
				{
					POVDirection = POVDirection.Up | POVDirection.Right;
				}
			}

			switch (POV)
			{
				case 18000:
					POVDirection = POVDirection.Down;
					break;
				case 0:
					POVDirection = POVDirection.Up;
					break;
				case 9000:
					POVDirection = POVDirection.Right;
					break;
				case 27000:
					POVDirection = POVDirection.Left;
					break;
				case -1:
					POVDirection = POVDirection.Center;
					break;
			}
		}

		/// <summary>
		/// Function to reset the various joystick axes and buttons and POV settings to default values.
		/// </summary>
		public void Reset()
		{
			POV = 0;
			POVDirection = POVDirection.Center;

			for (int i = 0; i < Axis.Count; ++i)
			{
				Axis[i] = 0;
			}

			for (int i = 0; i < Button.Count; ++i)
			{
				Button[i] = JoystickButtonState.Up;
			}
		}

		/// <summary>
		/// Function to poll the joystick for data.
		/// </summary>
		/// <remarks>Implementors must override this to return data from the joystick.</remarks>
		protected abstract void PollJoystick();

		/// <summary>
		/// Function to perform device vibration.
		/// </summary>
		/// <param name="motorIndex">Index of the motor to start.</param>
		/// <param name="value">Value to set.</param>
		/// <remarks>Implementors should implement this method if the device supports vibration.</remarks>
		protected virtual void VibrateDevice(int motorIndex, int value)
		{
		}

		/// <summary>
		/// Function to set the device to vibrate.
		/// </summary>
		/// <param name="motorIndex">Index of the motor to start.</param>
		/// <param name="value">Value to set.</param>
		/// <remarks>This will activate the vibration motor(s) in the joystick/game pad.  The <paramref name="motorIndex"/> should be within the 
		/// <see cref="P:Gorgon.Input.GorgonJoystick.JoystickCapabilities.VibrationMotorCount">VibrationMotorCount</see>, or else an exception will be thrown.  Check the
		/// <see cref="P:Gorgon.Input.GorgonJoystick.JoystickCapabilities.ExtraCapabilities">ExtraCapabilities</see> property to see if vibration is supported by the device.
		/// </remarks>
		/// <exception cref="GorgonException">Thrown when the device has not been initialized.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the motorIndex parameter is less than 0 or greater than or equal to the VibrationMotorCount range.</exception>
		public void Vibrate(int motorIndex, int value)
		{
			if (((Info.Capabilities & JoystickCapabilityFlags.SupportsVibration) != JoystickCapabilityFlags.SupportsVibration) 
				|| (motorIndex < 0) 
				|| (motorIndex >= Info.VibrationMotorRanges.Count))
			{
				throw new ArgumentOutOfRangeException(nameof(motorIndex), string.Format(Resources.GORINP_JOYSTICK_MOTOR_NOT_FOUND, motorIndex));
			}

			if (Info.VibrationMotorRanges[motorIndex].Contains(value))
		    {
		        VibrateDevice(motorIndex, value);
		    }
		}

		/// <summary>
		/// Function to read the joystick state.
		/// </summary>
		/// <remarks>Users must call this in order to retrieve the state of the joystick/game pad at any given time.</remarks>
		public void Poll()
		{
		    if ((DeviceLost) && (!BoundControl.Focused))
		    {
		        return;
		    }
		    
		    _deviceLost = false;

		    if ((!Enabled) || (!Acquired))
		    {
		        return;
		    }

		    // Set the values back to their defaults.
			Reset();
			
			// Get the data.
			PollJoystick();

			// Apply dead zones and get directions.
			for (int i = 0; i < Axis.Count; ++i)
			{
				Axis[i] = DeadZoneValue(Axis[i], DeadZones[i], _midRanges[i]);
			}

			UpdatePOVDirection();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonJoystick"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="deviceInfo">Information about the gaming device.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="owner"/> parameter is <b>null</b> (<i>Nothing</i> in VB.NET).</exception>
		protected GorgonJoystick(GorgonInputService owner, IGorgonJoystickInfo deviceInfo)
			: base(owner, deviceInfo)
		{
			Button = new List<JoystickButtonState>();
			Info = deviceInfo;

			CreateAxesValues();
		}
		#endregion
	}
}
