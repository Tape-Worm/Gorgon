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
// Created: Thursday, September 3, 2015 9:58:54 PM
// 
#endregion

using System.Collections.Generic;

namespace Gorgon.Input.Raw
{
	/// <summary>
	/// A raw input specific concrete implementation of the <see cref="IGorgonInputDeviceCoordinator"/> interface.
	/// </summary>
	class RawInputDeviceCoordinator
		: GorgonInputDeviceDefaultCoordinator
	{
		#region Variables.
		// Queued joystick events.
		private readonly Dictionary<GorgonJoystick2, GorgonJoystickData> _joystickData = new Dictionary<GorgonJoystick2, GorgonJoystickData>();
		#endregion

		#region Methods.
		/// <inheritdoc/>
		/// <remarks>
		/// <inheritdoc cref="IGorgonInputDeviceCoordinator.GetJoystickStateData"/>
		/// </remarks>
		public override GorgonJoystickData GetJoystickStateData(GorgonJoystick2 device)
		{
			GorgonJoystickData deviceData;

			return _joystickData.TryGetValue(device, out deviceData) ? deviceData : null;
		}

		/// <summary>
		/// Function to set the current state for the joystick.
		/// </summary>
		/// <param name="device">The joystick device.</param>
		/// <param name="data">The data to store.</param>
		public void SetJoystickState(GorgonJoystick2 device, GorgonJoystickData data)
		{
			_joystickData[device] = data;
		}
		#endregion
	}
}
