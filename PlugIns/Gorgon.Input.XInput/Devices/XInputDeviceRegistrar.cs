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
// Created: Thursday, September 3, 2015 12:39:00 AM
// 
#endregion

using System.Collections.Generic;
using System.Windows.Forms;
using Gorgon.Diagnostics;

namespace Gorgon.Input.XInput
{
	/// <summary>
	/// The XInput input implementation of the <see cref="IGorgonInputDeviceRegistrar"/> interface.
	/// </summary>
	class XInputDeviceRegistrar
		: IGorgonInputDeviceRegistrar
	{
		#region Variables.
		// The logger used for debugging.
		private readonly IGorgonLog _log;
		// A list of devices registered with the service.
		private readonly List<IGorgonInputDevice> _registeredDevices = new List<IGorgonInputDevice>();
		#endregion

		#region Methods.
		/// <inheritdoc/>
		public bool RegisterDevice(IGorgonInputDevice device, IGorgonInputDeviceInfo2 info, Control window, bool exclusive = false)
		{
			if (exclusive)
			{
				_log.Print("WARNING! Devices in the XInput input plug in cannot be set to exclusive mode.", LoggingLevel.Verbose);
			}

			if (_registeredDevices.Contains(device))
			{
				return false;
			}

			_registeredDevices.Add(device);

			return false;
		}

		/// <inheritdoc/>
		public void UnregisterDevice(IGorgonInputDevice device)
		{
			if (!_registeredDevices.Contains(device))
			{
				return;
			}

			_registeredDevices.Remove(device);
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="XInputDeviceRegistrar"/> class.
		/// </summary>
		/// <param name="log">The logger used for debugging.</param>
		public XInputDeviceRegistrar(IGorgonLog log)
		{
			_log = log;
		}
		#endregion

	}
}
