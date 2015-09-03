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
// Created: Thursday, September 3, 2015 9:32:22 PM
// 
#endregion

namespace Gorgon.Input
{
	/// <summary>
	/// A default concrete implementation of the <see cref="IGorgonInputDeviceCoordinator"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This default coordinator will direct mouse and keyboard event data to the appropriate device.
	/// </para>
	/// <para>
	/// Plug in implementors should inherit this class if they wish to provide more functionality for polled devices, or customized device data interpretation.
	/// </para>
	/// </remarks>
	public class GorgonInputDeviceDefaultCoordinator
		: IGorgonInputDeviceCoordinator
	{
		/// <inheritdoc/>
		public bool DispatchEvent<T>(IGorgonInputEventDrivenDevice<T> eventDevice, ref T deviceData) where T : struct
		{
			if ((eventDevice == null) || (!eventDevice.IsAcquired))
			{
				return false;
			}

			return eventDevice.ParseData(ref deviceData);
		}

		/// <inheritdoc/>
		/// <remarks>
		/// In this implementation, this method will return <b>false</b>, and will send back default data for the <see cref="GorgonJoystickData"/> type.
		/// </remarks>
		public virtual bool GetJoystickStateData(GorgonJoystick2 device, out GorgonJoystickData deviceData)
		{
			deviceData = default(GorgonJoystickData);
			return false;
		}
	}
}
