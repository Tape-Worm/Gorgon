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
using Gorgon.Diagnostics;
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
	public sealed class GorgonJoystick2
		: GorgonInputDevice2
	{
		#region Properties.
		/// <inheritdoc/>
		public override bool IsPolled => true;

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
		public IGorgonJoystickInfo2 Info
		{
			get;
		}

		/// <summary>
		/// Property to return the list of axes available to this gaming device.
		/// </summary>
		public GorgonJoystickAxisList Axis
		{
			get;
		}

		/// <summary>
		/// Property to return the point of view value for continuous bearing.
		/// </summary>
		/// <remarks>This will return an integer value of -1 for center, or 0 to 35999.  The user must divide by 100 to get the angle in degrees for a continuous bearing POV.</remarks>
		public int POV
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return a button state.
		/// </summary>
		public IList<JoystickButtonState> Button
		{
			get;
		}

		/// <summary>
		/// Property to return whether the joystick is connected or not.
		/// </summary> 
		/// <remarks>
		/// <para>
		/// Joysticks may be registered with the system, and appear in the enumeration list provided by <see cref="GorgonInputService2.EnumerateJoysticks"/>, but they may not be connected to the system 
		/// at the time of enumeration. Thus, we have this property to ensure that we know when a joystick is connected to the system or not. 
		/// </para>
		/// <para>
		/// This property will update itself when a joystick is connected or disconnected.
		/// </para>
		/// </remarks>
		public bool IsConnected
		{
			get;
			private set;
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

		/// <inheritdoc/>
		protected override void OnAcquiredStateChanged()
		{
			if (IsAcquired)
			{
				Reset();
			}
		}

		/// <summary>
		/// Function to reset the various joystick axes and buttons and POV settings to default values.
		/// </summary>
		public void Reset()
		{
			POV = 0;
			POVDirection = POVDirection.Center;

			foreach (GorgonJoystickAxis axis in Axis)
			{
				axis.Value = 0;
			}

			for (int i = 0; i < Button.Count; ++i)
			{
				Button[i] = JoystickButtonState.Up;
			}
		}

		/// <summary>
		/// Function to perform device vibration.
		/// </summary>
		/// <param name="motorIndex">Index of the motor to start.</param>
		/// <param name="value">Value to set.</param>
		/// <remarks>Implementors should implement this method if the device supports vibration.</remarks>
		/*protected virtual void VibrateDevice(int motorIndex, int value)
		{
		}*/

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
		        //VibrateDevice(motorIndex, value);
		    }
		}

		/// <summary>
		/// Function to read the joystick state.
		/// </summary>
		/// <remarks>Users must call this in order to retrieve the state of the joystick/game pad at any given time.</remarks>
		public void Poll()
		{
			if ((!IsAcquired) || (Window == null) || (Window.Disposing) || (Window.IsDisposed))
			{
				return;
			}
		    
		    // Set the values back to their defaults.
			Reset();

			GorgonJoystickData data;
			if (!Service.Coordinator.GetJoystickStateData(this, out data))
			{
				return;
			}

			// Get the data.


			// Apply dead zones and get directions.
			//for (int i = 0; i < Axis.Count; ++i)
			//{
				//Axis[i] = DeadZoneValue(Axis[i], DeadZones[i], _midRanges[i]);
			//}

			UpdatePOVDirection();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonJoystick"/> class.
		/// </summary>
		/// <param name="owner"><inheritdoc/></param>
		/// <param name="deviceInfo">Information about the joystick.</param>
		/// <param name="log"><inheritdoc/></param>
		/// <exception cref="System.ArgumentNullException"><inheritdoc/></exception>
		public GorgonJoystick2(GorgonInputService2 owner, IGorgonJoystickInfo2 deviceInfo, IGorgonLog log = null)
			: base(owner, deviceInfo, log)
		{
			Button = new List<JoystickButtonState>();
			Info = deviceInfo;
			Axis = new GorgonJoystickAxisList(deviceInfo);
		}
		#endregion
	}
}
