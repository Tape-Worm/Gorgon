#region MIT
// 
// Gorgon
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
// Created: Sunday, September 13, 2015 1:47:57 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using Gorgon.Diagnostics;
using Gorgon.Input.DirectInput.Properties;
using DI = SharpDX.DirectInput;

namespace Gorgon.Input.DirectInput
{
	/// <summary>
	/// The driver for DirectInput functionality.
	/// </summary>
	/// <remarks>
	/// This driver will enumerate all gaming devices except those covered by the XInput driver. To use those devices, use the XInput driver directly.
	/// </remarks>
	public sealed class GorgonDirectInputDriver
		: GorgonGamingDeviceDriver
	{
		#region Variables.
		// Primary direct input interface.
		private Lazy<DI.DirectInput> _directInput;
		// List of xinput device ID values.
		private readonly Lazy<IEnumerable<string>> _xinputDeviceIDs;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to build the device ID for an XInput device.
		/// </summary>
		/// <param name="deviceName">The device name from WMI.</param>
		/// <param name="pidIndex">The index of the PID in the WMI device ID.</param>
		/// <param name="vidIndex">The index of the VID in the WMI device ID.</param>
		/// <returns>A string containing the PID and VID portions of the device ID.</returns>
		private static string GetXInputDeviceID(string deviceName, int pidIndex, int vidIndex)
		{
			var buffer = new StringBuilder();
			string pidValue = deviceName.Substring(pidIndex + 4, 4);
			string vidValue = deviceName.Substring(vidIndex + 4, 4);

			buffer.Append(pidValue);
			buffer.Append(vidValue);
			
			return buffer.ToString();
		}

		/// <summary>
		/// Function to build a list of XInput device ID values.
		/// </summary>
		/// <returns>A list of XInput device ID values.</returns>
		private static IEnumerable<string> GetXInputDeviceIDs()
		{
			// This monstrosity is based on the code at:
			// https://msdn.microsoft.com/en-ca/library/windows/desktop/ee417014(v=vs.85).aspx
			// 
			using (var search = new ManagementObjectSearcher("SELECT DeviceID FROM Win32_PnPEntity"))
			{
				IEnumerable<string> xinputDevices = (from pnpDevice in search.Get().Cast<ManagementBaseObject>()
				                                     let deviceID = pnpDevice.GetPropertyValue("DeviceID")
				                                     where deviceID is string
				                                     let deviceName = deviceID.ToString()
				                                     let pidIndex = deviceName.IndexOf("PID_", StringComparison.OrdinalIgnoreCase)
				                                     let vidIndex = deviceName.IndexOf("VID_", StringComparison.OrdinalIgnoreCase)
				                                     where deviceName.IndexOf("IG_", StringComparison.OrdinalIgnoreCase) != -1
				                                           && pidIndex != -1
				                                           && vidIndex != -1
				                                     select GetXInputDeviceID(deviceName, pidIndex, vidIndex))
					.ToArray();

				return xinputDevices;
			}
		}

		/// <summary>
		/// Function to determine if the device is an XInput controller.
		/// </summary>
		/// <param name="device">Device to evaluate.</param>
		/// <returns><b>true</b> if the device is an xinput controller, <b>false</b> if not.</returns>
		private bool IsXInputController(DI.DeviceInstance device)
		{
			var buffer = new StringBuilder(device.ProductGuid.ToString()).Remove(0, 8);

			foreach (string deviceID in _xinputDeviceIDs.Value)
			{
				buffer.Insert(0, deviceID);

				if (new Guid(buffer.ToString()) == device.ProductGuid)
				{
					return true;
				}

				buffer.Remove(0, 8);
			}

			return false;
		}

		/// <inheritdoc/>
		public override IReadOnlyList<IGorgonGamingDeviceInfo> EnumerateGamingDevices(bool connectedOnly = false)
		{
			IList<DI.DeviceInstance> devices = _directInput.Value.GetDevices(DI.DeviceClass.GameControl, DI.DeviceEnumerationFlags.AttachedOnly);

			Log.Print("Enumerating DirectInput gaming devices...", LoggingLevel.Verbose);

			// Enumerate all controllers.
			IReadOnlyList<DirectInputDeviceInfo> result = devices
				.Where(item =>
				       {
					       bool notXInputController = !IsXInputController(item);

					       if (!notXInputController)
					       {
						       Log.Print("WARNING: Found XInput controller.  The Direct Input driver does not support this type of controller.  Skipping...", LoggingLevel.Verbose);
						       return false;
					       }

					       bool isAttached = _directInput.Value.IsDeviceAttached(item.InstanceGuid);

					       if ((connectedOnly) || (isAttached))
					       {
						       return true;
					       }

					       Log.Print("WARNING: Found gaming device '{0}', but it is not attached and enumeration is filtered for attached devices only.  Skipping...",
					                 LoggingLevel.Verbose, item.ProductName);

					       return false;
				       })
				.Select(item =>
				        {
					        var info = new DirectInputDeviceInfo(item);

					        using (DI.Joystick joystick = new DI.Joystick(_directInput.Value, info.InstanceGuid))
					        {
						        info.GetDeviceCaps(joystick);

						        Log.Print("Found DirectInput gaming device \"{0}\"",
						                  LoggingLevel.Verbose, info.Description);
					        }

					        return info;
				        })
				.ToArray();

			return result;
		}

		/// <inheritdoc/>
		public override IGorgonGamingDevice CreateGamingDevice(IGorgonGamingDeviceInfo gamingDeviceInfo)
		{
			DirectInputDeviceInfo deviceInfo = gamingDeviceInfo as DirectInputDeviceInfo;

			if (deviceInfo == null)
			{
				throw new ArgumentException(Resources.GORINP_ERR_DI_NOT_A_DI_DEVICE_INFO, nameof(gamingDeviceInfo));
			}

			return new DirectInputDevice(deviceInfo, _directInput.Value);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			if (_directInput.IsValueCreated)
			{
				_directInput.Value.Dispose();
			}

			_directInput = null;

			base.Dispose();
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDirectInputDriver"/> class.
		/// </summary>
		public GorgonDirectInputDriver()
			: base(Resources.GORINP_DI_DESC)
		{
			_directInput = new Lazy<DI.DirectInput>(() => new DI.DirectInput(), true);
			_xinputDeviceIDs = new Lazy<IEnumerable<string>>(GetXInputDeviceIDs, true);
		}
		#endregion
	}
}
