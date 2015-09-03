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

using System.Collections.Generic;
using Gorgon.Core;

namespace Gorgon.Input
{
	/// <summary>
	/// Contains information for a gaming device (joystick, game pad, etc...) device.
	/// </summary>
	public interface IGorgonJoystickInfo
		: IGorgonInputDeviceInfo
	{
		/// <summary>
		/// Property to return whether the joystick is connected or not.
		/// </summary> 
		/// <remarks>
		/// <para>
		/// Joysticks may be registered with the system, and appear in the enumeration list provided by <see cref="GorgonInputService2.EnumerateJoysticks"/>, but they may not be connected to the 
		/// system at the time of enumeration. Thus, we have this property to ensure that we know when a joystick is connected to the system or not. 
		/// </para>
		/// <para>
		/// This property will update itself when a joystick is connected or disconnected.
		/// </para>
		/// </remarks>
		bool IsConnected
		{
			get;
		}

		/// <summary>
		/// Property to return the number of buttons available on the gaming device.
		/// </summary>
		int ButtonCount
		{
			get;
		}

		/// <summary>
		/// Property to return the ID for the manufacturer of the gaming device.
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
		/// Property to return the minimum and maximum range values for each axis on the gaming device.
		/// </summary>
		/// <remarks>
		/// Use this value to retrieve the number of axes the gaming device supports by checking its <see cref="GorgonJoystickAxisRangeList.Count"/> property.
		/// </remarks>
		IGorgonJoystickAxisRangeList AxisRanges
		{
			get;
		}

		/// <summary>
		/// Property to return the tolerances for each of the vibration motors in the gaming device.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Use this value to retrieve the number of vibration motors the gaming device supports by checking its <see cref="IReadOnlyCollection{T}.Count"/> property.
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
		/// Property to return the capabilities supported by the gaming device.
		/// </summary>
		JoystickCapabilityFlags Capabilities
		{
			get;
		}
	}
}
