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
// Created: Saturday, July 4, 2015 2:20:46 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Core;

namespace Gorgon.Input
{
	/// <summary>
	/// Flags to indicate which extra capabilities are supported by the joystick.
	/// </summary>
	/// <remarks>
	/// The values in this enumeration may be OR'd together to indicate support for multiple items, or it will return <see cref="None"/> if the device does not have any special capabilities.
	/// </remarks>
	[Flags]
	public enum JoystickCapabilityFlags
	{
		/// <summary>
		/// No extra capabilities.  This flag is mutually exclusive.
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
		SupportsVibration = 32,
		/// <summary>
		/// Supports a secondary X axis.
		/// </summary>
		SupportsSecondaryXAxis = 64,
		/// <summary>
		/// Supports a secondary Y axis.
		/// </summary>
		SupportsSecondaryYAxis = 128
	}

	/// <summary>
	/// Contains values for common, well known, joystick axes.
	/// </summary>
	public enum JoystickAxis
	{
		/// <summary>
		/// The primary horizontal axis.
		/// </summary>
		XAxis = 0,
		/// <summary>
		/// The primary vertical axis.
		/// </summary>
		YAxis = 1,
		/// <summary>
		/// The primary z axis.
		/// </summary>
		ZAxis = 2,
		/// <summary>
		/// The secondary horizontal axis.
		/// </summary>
		XAxis2 = 3,
		/// <summary>
		/// The secondary vertical axis.
		/// </summary>
		YAxis2 = 4,
		/// <summary>
		/// The secondary z axis.
		/// </summary>
		ZAxis2 = 5,
		/// <summary>
		/// The throttle axis. This maps to the <see cref="ZAxis"/> value.
		/// </summary>
		Throttle = 2,
		/// <summary>
		/// The rudder axis. This maps to the <see cref="ZAxis2"/> value.
		/// </summary>
		Rudder = 5
	}

	/// <summary>
	/// Contains information for a joystick (joystick, game pad, etc...) device.
	/// </summary>
	public interface IGorgonJoystickInfo2
		: IGorgonInputDeviceInfo2
	{
		/// <summary>
		/// Property to return the number of buttons available on the joystick.
		/// </summary>
		int ButtonCount
		{
			get;
		}

		/// <summary>
		/// Property to return the ID for the manufacturer of the joystick.
		/// </summary>
		int ManufacturerID
		{
			get;
		}

		/// <summary>
		/// Property to return the ID of the product.
		/// </summary>
		int ProductID
		{
			get;
		}

		/// <summary>
		/// Property to return the <see cref="GorgonJoystickAxisInfo"/> values for each axis on the joystick.
		/// </summary>
		/// <remarks>
		/// Use this value to retrieve the number of axes the joystick supports by checking its <see cref="GorgonJoystickAxisRangeList.Count"/> property.
		/// </remarks>
		IGorgonJoystickAxisInfoList AxisInfo
		{
			get;
		}

		/// <summary>
		/// Property to return the tolerances for each of the vibration motors in the joystick.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Use this value to retrieve the number of vibration motors the joystick supports by checking its <see cref="IReadOnlyCollection{T}.Count"/> property.
		/// </para>
		/// <para>
		/// If the device does not support vibration, then this list will be empty.
		/// </para>
		/// </remarks>
		IReadOnlyList<GorgonRange> VibrationMotorRanges
		{
			get;
		}

		/// <summary>
		/// Property to return the capabilities supported by the joystick.
		/// </summary>
		JoystickCapabilityFlags Capabilities
		{
			get;
		}
	}
}
