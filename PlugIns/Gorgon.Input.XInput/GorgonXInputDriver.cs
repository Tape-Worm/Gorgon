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
// Created: Friday, July 15, 2011 6:22:48 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Diagnostics;
using Gorgon.Input.XInput.Properties;
using XI = SharpDX.XInput;

namespace Gorgon.Input.XInput
{
	/// <summary>
	/// The driver for XInput functionality.
	/// </summary>
	public class GorgonXInputDriver
		: GorgonGamingDeviceDriver
	{
		#region Methods.
		/// <inheritdoc/>
		public override IReadOnlyList<IGorgonGamingDeviceInfo> EnumerateGamingDevices(bool connectedOnly = false)
		{
			Log.Print("Enumerating XInput controllers...", LoggingLevel.Verbose);

			// Enumerate all controllers.
			IReadOnlyList<XInputDeviceInfo> result =
				(from deviceIndex in (XI.UserIndex[])Enum.GetValues(typeof(XI.UserIndex))
				 where deviceIndex != XI.UserIndex.Any
				 let controller = new XI.Controller(deviceIndex)
				 where !connectedOnly || controller.IsConnected
				 orderby deviceIndex
				 select
					 new XInputDeviceInfo(string.Format(Resources.GORINP_XINP_DEVICE_NAME, (int)deviceIndex + 1), deviceIndex))
					.ToArray();

			foreach (XInputDeviceInfo info in result)
			{
				Log.Print("Found XInput controller {0}", LoggingLevel.Verbose, info.Description);
				info.GetCaps(new XI.Controller(info.ID));
			}

			return result;
		}

		/// <inheritdoc/>
		public override IGorgonGamingDevice CreateGamingDevice(IGorgonGamingDeviceInfo gamingDeviceInfo)
		{
			XInputDeviceInfo xInputInfo = gamingDeviceInfo as XInputDeviceInfo;

			if (xInputInfo == null)
			{
				throw new ArgumentException(Resources.GORINP_ERR_XINP_NOT_AN_XINPUT_DEVICE_INFO, nameof(gamingDeviceInfo));
			}

			return new XInputDevice(xInputInfo);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonXInputDriver"/> class.
		/// </summary>
		public GorgonXInputDriver()
			: base(Resources.GORINP_XINP_SERVICEDESC)
		{
		}
		#endregion
	}
}
