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
// Created: Friday, July 15, 2011 6:22:54 AM
// 
#endregion

using System;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Collections;
using Gorgon.Collections.Specialized;
using Gorgon.Input.XInput.Properties;
using XI = SharpDX.XInput;

namespace Gorgon.Input.XInput
{
	/// <summary>
	/// Object representing the main interface to the input library.
	/// </summary>
	internal class GorgonXInputService
		: GorgonInputService
	{
		#region Methods.
		/// <summary>
		/// Function to enumerate the pointing devices on the system.
		/// </summary>
		/// <returns>A list of pointing device names.</returns>
		protected override IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> EnumeratePointingDevices()
		{
			return new GorgonNamedObjectDictionary<GorgonInputDeviceInfo>();
		}

		/// <summary>
		/// Function to enumerate the keyboard devices on the system.
		/// </summary>
		/// <returns>A list of keyboard device names.</returns>
		protected override IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> EnumerateKeyboardDevices()
		{
			return new GorgonNamedObjectDictionary<GorgonInputDeviceInfo>();
		}

		/// <summary>
		/// Function to enumerate the joystick devices attached to the system.
		/// </summary>
		/// <returns>A list of joystick device names.</returns>
		protected override IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> EnumerateJoysticksDevices()
		{
			// Enumerate all controllers.
			var result = new GorgonNamedObjectDictionary<GorgonInputDeviceInfo>(false);
			result.AddRange((from xiDeviceIndex in (XI.UserIndex[])Enum.GetValues(typeof(XI.UserIndex))
			                 where xiDeviceIndex != XI.UserIndex.Any
			                 orderby xiDeviceIndex
			                 select new GorgonXInputDeviceInfo(string.Format("{0}: XInput Controller", (int)xiDeviceIndex + 1),
			                                                   string.Format("XInput_{0}", xiDeviceIndex),
			                                                   new XI.Controller(xiDeviceIndex),
			                                                   (int)xiDeviceIndex))
				                .OrderBy(item => item.Name));

			return result;
		}

		/// <summary>
		/// Function to enumerate device types for which there is no class wrapper and will return data in a custom property collection.
		/// </summary>
		/// <returns>
		/// A list of custom HID types.
		/// </returns>
		protected override IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> EnumerateCustomHIDs()
		{
			return new GorgonNamedObjectDictionary<GorgonInputDeviceInfo>();
		}

		/// <summary>
		/// Creates the custom HID impl.
		/// </summary>
		/// <param name="window">The window.</param>
		/// <param name="hidName">Name of the hid.</param>
		/// <returns></returns>
		protected override GorgonCustomHID CreateCustomHIDImpl(Control window, GorgonInputDeviceInfo hidName)
		{
			throw new NotSupportedException(Resources.GORINP_XINP_ONLY_360_CONTROLLERS);
		}

		/// <summary>
		/// Creates the keyboard impl.
		/// </summary>
		/// <param name="window">The window.</param>
		/// <param name="keyboardName">Name of the keyboard.</param>
		/// <returns></returns>
		protected override GorgonKeyboard CreateKeyboardImpl(Control window, GorgonInputDeviceInfo keyboardName)
		{
            throw new NotSupportedException(Resources.GORINP_XINP_ONLY_360_CONTROLLERS);
		}

		/// <summary>
		/// Creates the pointing device impl.
		/// </summary>
		/// <param name="window">The window.</param>
		/// <param name="pointingDeviceName">Name of the pointing device.</param>
		/// <returns></returns>
		protected override GorgonPointingDevice CreatePointingDeviceImpl(Control window, GorgonInputDeviceInfo pointingDeviceName)
		{
            throw new NotSupportedException(Resources.GORINP_XINP_ONLY_360_CONTROLLERS);
		}

		/// <summary>
		/// Function to create a joystick interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>
		/// <param name="joystickName">A <see cref="Gorgon.Input.GorgonInputDeviceInfo">GorgonInputDeviceInfo</see> object containing the joystick information.</param>
		/// <returns>A new joystick interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:Gorgon.Gorgon.ApplicationForm">Gorgon application form</see>.</remarks>
		protected override GorgonJoystick CreateJoystickImpl(Control window, GorgonInputDeviceInfo joystickName)
		{
		    var deviceName = joystickName as GorgonXInputDeviceInfo;

		    if (deviceName == null)
		    {
                throw new InvalidCastException(Resources.GORINP_XINP_NOT_XINPUT_JOYSTICK);
		    }

		    return new XInputController(this, deviceName.Index, joystickName.Name, deviceName.Controller);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonXInputService"/> class.
		/// </summary>
		public GorgonXInputService()
			: base("Gorgon XBox 360 Controller Input")
		{
		}
		#endregion
	}
}
