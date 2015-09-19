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
using System.Linq;
using System.Windows.Forms;
using Gorgon.Input.Properties;
using Gorgon.Math;

namespace Gorgon.Input
{
	/// <inheritdoc cref="IGorgonGamingDevice"/>
	public abstract class GorgonGamingDevice
		: IGorgonGamingDevice
	{
		#region Variables.
		// The POV of view control directions.
		private readonly POVDirection[] _povDirections;
		// Flag to indicate whether the gaming device has been acquired or not.
		private bool _isAcquired;
		#endregion

		#region Properties.
		/// <inheritdoc/>
		public IReadOnlyList<POVDirection> POVDirection => _povDirections;

		/// <inheritdoc/>
		public IGorgonGamingDeviceInfo Info
		{
			get;
		}

		/// <inheritdoc/>
		public GorgonGamingDeviceAxisList<GorgonGamingDeviceAxis> Axis
		{
			get;
		}

		/// <inheritdoc/>
		public float[] POV
		{
			get;
		}

		/// <inheritdoc/>
		public GamingDeviceButtonState[] Button
		{
			get;
		}

		/// <inheritdoc/>
		public abstract bool IsConnected
		{
			get;
		}

		/// <inheritdoc/>
		public bool IsAcquired
		{
			get
			{
				return _isAcquired;
			}
			set
			{
				if (_isAcquired == value)
				{
					return;
				}

				_isAcquired = OnAcquire(value);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the direction flag for the POV.
		/// </summary>
		/// <param name="povIndex">The index of the point of view control to use.</param>
		private void UpdatePOVDirection(int povIndex)
		{
			if ((Info.Capabilities & GamingDeviceCapabilityFlags.SupportsPOV) != GamingDeviceCapabilityFlags.SupportsPOV)
			{
				return;
			}

			int pov = (int)((POV[povIndex] * 100.0f).Max(-1.0f));

			// Wrap POV if it's higher than 359.99 degrees.
			if (pov > 35999)
			{
				pov = -1;
			}
			
			// Get POV direction.
			if (pov != -1)
			{
				if ((pov < 18000) && (pov > 9000))
				{
					_povDirections[povIndex] = Input.POVDirection.Down | Input.POVDirection.Right;
				}
				if ((pov > 18000) && (pov < 27000))
				{
					_povDirections[povIndex] = Input.POVDirection.Down | Input.POVDirection.Left;
				}
				if ((pov > 27000) && (pov < 36000))
				{
					_povDirections[povIndex] = Input.POVDirection.Up | Input.POVDirection.Left;
				}
				if ((pov > 0) && (pov < 9000))
				{
					_povDirections[povIndex] = Input.POVDirection.Up | Input.POVDirection.Right;
				}
			}

			switch (pov)
			{
				case 18000:
					_povDirections[povIndex] = Input.POVDirection.Down;
					break;
				case 0:
					_povDirections[povIndex] = Input.POVDirection.Up;
					break;
				case 9000:
					_povDirections[povIndex] = Input.POVDirection.Right;
					break;
				case 27000:
					_povDirections[povIndex] = Input.POVDirection.Left;
					break;
				case -1:
					_povDirections[povIndex] = Input.POVDirection.Center;
					break;
			}
		}

		/// <summary>
		/// Function to acquire a gaming device.
		/// </summary>
		/// <param name="acquireState"><b>true</b> to acquire the device, <b>false</b> to unacquire it.</param>
		/// <returns><b>true</b> if the device was acquired successfully, <b>false</b> if not.</returns>
		/// <remarks>
		/// <para>
		/// Implementors of a <see cref="GorgonGamingDeviceDriver"/> should implement this on the concrete <see cref="GorgonGamingDevice"/>, even if the device does not use acquisition (in such a case, 
		/// always return <b>true</b> from this method).
		/// </para>
		/// <para>
		/// Some input providers, like Direct Input, have the concept of device acquisition. When a device is created it may not be usable until it has been acquired by the application. When a device is 
		/// acquired, it will be made available for use by the application. However, in some circumstances, another application may have exclusive control over the device, and as such, acquisition is not 
		/// possible. 
		/// </para>
		/// <para>
		/// A device may lose acquisition when the application goes into the background, and as such, the application will no longer receive information from the device. When this is the case, the 
		/// application should immediately check after it regains foreground focus to see whether the device is acquired or not. For a winforms application this can be achieved with the <see cref="Form.Activated"/> 
		/// event. When that happens, set this property to <b>true</b>.
		/// </para>
		/// <para>
		/// Note that some devices from other input providers (like XInput) will always be in an acquired state. Regardless, it is best to check this flag when the application becomes active.
		/// </para>
		/// </remarks>
		protected abstract bool OnAcquire(bool acquireState);

		/// <inheritdoc cref="Vibrate"/>
		/// <remarks>
		/// <inheritdoc cref="Vibrate"/>
		/// <para>
		/// Implementors of a <see cref="GorgonGamingDeviceDriver"/> plug in should ensure that devices that support vibration implement this method. Otherwise, if the device does not support the functionality 
		/// then this method can be left alone.
		/// </para>
		/// </remarks>
		protected virtual void OnVibrate(int motorIndex, int value)
		{
		}

		/// <summary>
		/// Function to retrieve data from the provider of the physical device.
		/// </summary>
		/// <remarks>
		/// Implementors of a <see cref="GorgonGamingDeviceDriver"/> plug in must implement this and format their data to populate the values of this object with correct state information.
		/// </remarks>
		protected abstract void OnGetData();

		/// <inheritdoc/>
		public virtual void Reset()
		{
			for (int i = 0; i < _povDirections.Length; ++i)
			{
				POV[i] = -1.0f;
				_povDirections[i] = Input.POVDirection.Center;
			}

			foreach (GorgonGamingDeviceAxis axis in Axis)
			{
				axis.Value = Info.AxisInfo[axis.Axis].DefaultValue;
			}

			for (int i = 0; i < Button.Length; ++i)
			{
				Button[i] = GamingDeviceButtonState.Up;
			}
		}

		/// <inheritdoc/>
		public void Vibrate(int motorIndex, int value)
		{
			if (((Info.Capabilities & GamingDeviceCapabilityFlags.SupportsVibration) != GamingDeviceCapabilityFlags.SupportsVibration) 
				|| (motorIndex < 0) 
				|| (motorIndex >= Info.VibrationMotorRanges.Count))
			{
				throw new ArgumentOutOfRangeException(nameof(motorIndex), string.Format(Resources.GORINP_ERR_JOYSTICK_MOTOR_NOT_FOUND, motorIndex));
			}

			if (Info.VibrationMotorRanges[motorIndex].Contains(value))
		    {
				OnVibrate(motorIndex, value);
		    }
		}

		/// <inheritdoc/>
		public void Poll()
		{
			if (!IsConnected)
			{
				return;
			}

			OnGetData();

			for (int i = 0; i < POV.Length; ++i)
			{
				UpdatePOVDirection(i);
			}
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		/// <remarks>
		/// <para>
		/// Some gaming devices use native resources to communicate with the physical device. Because of this, it is necessary to call this method to ensure those resources are freed.
		/// </para>
		/// <para>
		/// For implementors of a <see cref="GorgonGamingDeviceDriver"/>, this method should be overridden to free up any native resources required by the device. If the device does 
		/// not use any native resources, then it is safe to leave this method alone.
		/// </para>
		/// </remarks>
		public virtual void Dispose()
		{
			GC.SuppressFinalize(this);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGamingDevice"/> class.
		/// </summary>
		/// <param name="deviceInfo">Information about the specific gaming device to use.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="deviceInfo"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		protected GorgonGamingDevice(IGorgonGamingDeviceInfo deviceInfo)
		{
			if (deviceInfo == null)
			{
				throw new ArgumentNullException(nameof(deviceInfo));
			}

			_povDirections =
				new POVDirection[((deviceInfo.Capabilities & GamingDeviceCapabilityFlags.SupportsPOV) == GamingDeviceCapabilityFlags.SupportsPOV) ? deviceInfo.POVCount : 0];
			POV = new float[_povDirections.Length];
			Button = new GamingDeviceButtonState[deviceInfo.ButtonCount];
			Info = deviceInfo;
			Axis = new GorgonGamingDeviceAxisList<GorgonGamingDeviceAxis>(deviceInfo.AxisInfo.Select(item => new GorgonGamingDeviceAxis(item)));
		}
		#endregion
	}
}
