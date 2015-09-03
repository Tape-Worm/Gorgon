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
// Created: Thursday, September 3, 2015 9:19:29 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Gorgon.Input
{
	/// <summary>
	/// Coordinates sending and receiving device data from an input device interface to a <see cref="IGorgonInputDevice"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// For event driven devices (e.g. mouse, keyboard), the coordinator will ensure that devices that use events for signalling state have their respective <see cref="IGorgonInputDevice"/> state updated. 
	/// This allows for automatic state updates on a device object without the user needing to poll the device in a tight loop or on a separate thread.
	/// </para>
	/// <para>
	/// For devices that require polling (e.g. joysticks), this type can be used to retrieve the current state of the device, or even send state back to the device.
	/// </para>
	/// </remarks>
	public interface IGorgonInputDeviceCoordinator
	{
		/// <summary>
		/// Function to dispatch data to a device that uses events to receive its data.
		/// </summary>
		/// <typeparam name="T">The type of data to send to the device.</typeparam>
		/// <param name="eventDevice">The device that supports events.</param>
		/// <param name="deviceData">The data to send to the device.</param>
		/// <returns><b>true</b> if the data was dispatched successfully, <b>false</b> if not.</returns>
		/// <remarks>
		/// This will send device data to the appropriate event driven device. Typically this is used by devices such as the <see cref="GorgonMouse"/> or the <see cref="GorgonKeyboard2"/>, however if a custom 
		/// event driven device is set up, this can also be used to capture events to that device as well.
		/// </remarks>
		bool DispatchEvent<T>(IGorgonInputEventDrivenDevice<T> eventDevice, ref T deviceData) where T : struct;

		/// <summary>
		/// Function to retrieve state for a joystick device.
		/// </summary>
		/// <param name="device">The joystick to retrieve the state from.</param>
		/// <param name="deviceData">The data for the device.</param>
		/// <returns><b>true</b> if the data was retrieved successfully, <b>false</b> if not.</returns>
		/// <remarks>
		/// This will retrieve the current state for a joystick device. The <see cref="GorgonJoystick2"/> device is polled instead of event driven, and this will allow the joystick to retrieve its daat from 
		/// the native device data.
		/// </remarks>
		bool GetJoystickStateData(GorgonJoystick2 device, out GorgonJoystickData deviceData);
	}
}
