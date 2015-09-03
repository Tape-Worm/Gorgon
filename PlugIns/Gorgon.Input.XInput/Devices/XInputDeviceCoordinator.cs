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
// Created: Thursday, September 3, 2015 11:39:43 PM
// 
#endregion

using System.Collections.Generic;
using XI = SharpDX.XInput;

namespace Gorgon.Input.XInput
{
	/// <summary>
	/// An XInput specific concrete implementation of the <see cref="IGorgonInputDeviceCoordinator"/>
	/// </summary>
	class XInputDeviceCoordinator
		: IGorgonInputDeviceCoordinator
	{
		#region Classes.

		private class DeviceState
		{
			/// <summary>
			/// Property to set or return the controller for the device state.
			/// </summary>
			public XI.Controller Controller
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the last packet ID received.
			/// </summary>
			public int? LastPacket
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the last state received.
			/// </summary>
			public GorgonJoystickData? LastState
			{
				get;
				set;
			}
		}
		#endregion

		#region Variables.
		// The list of xinput controllers.
		private readonly Dictionary<IGorgonInputDevice, DeviceState> _controllers = new Dictionary<IGorgonInputDevice, DeviceState>();
		#endregion

		#region Methods.
		/// <inheritdoc/>
		bool IGorgonInputDeviceCoordinator.DispatchEvent<T>(IGorgonInputEventDrivenDevice<T> eventDevice, ref T deviceData)
		{
			// This is never used from this plug in.
			return false;
		}

		/// <inheritdoc/>
		public bool GetJoystickStateData(GorgonJoystick2 device, out GorgonJoystickData deviceData)
		{
			DeviceState controller;

			if (!_controllers.TryGetValue(device, out controller))
			{
				_controllers[device] = controller = new DeviceState
				                                    {
					                                    Controller = new XI.Controller(((XInputJoystickInfo)device.Info).ID),
					                                    LastPacket = null,
					                                    LastState = null
				                                    };
			}
			
			XI.State state;

			// Check for disconnected state.
			if ((!controller.Controller.IsConnected)
				|| (!controller.Controller.GetState(out state)))
            {
				deviceData = new GorgonJoystickData
				             {
					             IsConnected = false
				             };
				controller.LastPacket = null;
				controller.LastState = null;
				return false;
			}

			// If there's been no change to the device, or it's lost acquisition, then just receive the last known state.
			if (((controller.LastPacket.HasValue) && (controller.LastPacket == state.PacketNumber))
				|| (!device.IsAcquired))
			{
				deviceData = controller.LastState ?? new GorgonJoystickData
				                                     {
					                                     IsConnected = controller.Controller.IsConnected
				                                     };

				if (deviceData.IsConnected)
				{
					return true;
				}

				controller.LastPacket = null;
				controller.LastState = null;
				return false;
			}

			deviceData = new GorgonJoystickData
			             {
				             IsConnected = controller.Controller.IsConnected
			             };

			controller.LastState = deviceData;
			controller.LastPacket = state.PacketNumber;

			return true;
		}
		#endregion
	}
}
