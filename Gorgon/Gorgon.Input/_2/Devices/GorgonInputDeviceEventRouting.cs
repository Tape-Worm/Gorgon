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
// Created: Wednesday, July 22, 2015 11:08:46 PM
// 
#endregion

namespace Gorgon.Input
{
	/// <summary>
	/// Routes events to the appropriate device when an event is handled on the device.
	/// </summary>
	/// <remarks>
	/// This object is meant for developers and is exposed via the <see cref="GorgonInputService2.EventRouter"/> property. End users should not use this object.
	/// </remarks>
	public class GorgonInputDeviceEventRouting
	{
		/// <summary>
		/// Function to route unprocessed mouse event data to a mouse device.
		/// </summary>
		/// <param name="device">The device that will receive the mouse event data.</param>
		/// <param name="data">The unprocessed mouse data to route to the device.</param>
		/// <returns><b>true</b> if the call was successfully routed, <b>false</b> if not.</returns>
		/// <remarks>
		/// <para>
		/// This method will send a packet of <see cref="GorgonMouseData"/> on to the <paramref name="device"/> specified. 
		/// </para>
		/// <para>
		/// Input service implementors will take the native device event data and convert it into the <see cref="GorgonMouseData"/> so that the input devices will know how to parse the data and provide 
		/// events to end user objects.
		/// </para>
		/// </remarks>
		public bool RouteToDevice(IGorgonInputDevice device, ref GorgonMouseData data)
		{
			if (device == null)
			{
				return false;
			}

			IGorgonDeviceRouting<GorgonMouseData> router = device as IGorgonDeviceRouting<GorgonMouseData>;

			// If the device has become unacquired by this point, then do not forward the data.
			if (!device.IsAcquired)
			{
				return false;
			}

			return router?.ParseData(ref data) ?? false;
		}

		/// <summary>
		/// Function to route unprocessed keyboard event data to a keyboard device.
		/// </summary>
		/// <param name="device">The device that will receive the keyboard event data.</param>
		/// <param name="data">The unprocessed keyboard data to route to the device.</param>
		/// <returns><b>true</b> if the call was successfully routed, <b>false</b> if not.</returns>
		/// <remarks>
		/// <para>
		/// This method will send a packet of <see cref="GorgonKeyboardData"/> on to the <paramref name="device"/> specified. 
		/// </para>
		/// <para>
		/// Input service implementors will take the native device event data and convert it into the <see cref="GorgonKeyboardData"/> so that the input devices will know how to parse the data and provide 
		/// events to end user objects.
		/// </para>
		/// </remarks>
		public bool RouteToDevice(IGorgonInputDevice device, ref GorgonKeyboardData data)
		{
			if (device == null)
			{
				return false;
			}

			IGorgonDeviceRouting<GorgonKeyboardData> router = device as IGorgonDeviceRouting<GorgonKeyboardData>;

			// If the device has become unacquired by this point, then do not forward the data.
			if (!device.IsAcquired)
			{
				return false;
			}

			return router?.ParseData(ref data) ?? false;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputDeviceEventRouting" /> class.
		/// </summary>
		internal GorgonInputDeviceEventRouting()
		{
			// Internal because this object is not meant to be used by end users.
		}
	}
}
