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
	/// The service for XInput controllers.
	/// </summary>
	class GorgonXInputService
		: GorgonInputService
	{
		#region Methods.
		/// <inheritdoc/>
		protected override IGorgonNamedObjectReadOnlyDictionary<IGorgonMouseInfo> OnEnumerateMice()
		{
			return new GorgonNamedObjectDictionary<IGorgonMouseInfo>();
		}

		/// <inheritdoc/>
		protected override IGorgonNamedObjectReadOnlyDictionary<IGorgonKeyboardInfo> OnEnumerateKeyboards()
		{
			return new GorgonNamedObjectDictionary<IGorgonKeyboardInfo>();
		}

		/// <inheritdoc/>
		protected override IGorgonNamedObjectReadOnlyDictionary<IGorgonJoystickInfo> OnEnumerateJoysticks()
		{
			// Enumerate all controllers.
			var result = new GorgonNamedObjectDictionary<IGorgonJoystickInfo>(false);
			result.AddRange((from xiDeviceIndex in (XI.UserIndex[])Enum.GetValues(typeof(XI.UserIndex))
			                 where xiDeviceIndex != XI.UserIndex.Any
			                 orderby xiDeviceIndex
			                 select
				                 new XInputJoystickInfo(Guid.NewGuid(),
				                                        string.Format(Resources.GORINP_XINP_DEVICE_NAME, (int)xiDeviceIndex + 1),
				                                        new XI.Controller(xiDeviceIndex),
				                                        (int)xiDeviceIndex))
				                .OrderBy(item => item.Name));

			return result;
		}

		/// <inheritdoc/>
		protected override IGorgonNamedObjectReadOnlyDictionary<IGorgonHumanInterfaceDeviceInfo> OnEnumerateHumanInterfaceDevices()
		{
			return new GorgonNamedObjectDictionary<IGorgonHumanInterfaceDeviceInfo>();
		}

		/// <inheritdoc/>
		protected override GorgonCustomHID OnCreateHumanInterfaceDevice(Control window, IGorgonHumanInterfaceDeviceInfo deviceInfo)
		{
			throw new NotSupportedException(Resources.GORINP_XINP_ONLY_360_CONTROLLERS);
		}

		/// <inheritdoc/>
		protected override GorgonKeyboard OnCreateKeyboard(Control window, IGorgonKeyboardInfo keyboardInfo)
		{
            throw new NotSupportedException(Resources.GORINP_XINP_ONLY_360_CONTROLLERS);
		}

		/// <inheritdoc/>
		protected override GorgonPointingDevice OnCreateMouse(Control window, IGorgonMouseInfo pointingDeviceInfo)
		{
            throw new NotSupportedException(Resources.GORINP_XINP_ONLY_360_CONTROLLERS);
		}

		/// <inheritdoc/>
		protected override GorgonJoystick OnCreateJoystick(Control window, IGorgonJoystickInfo deviceInfo)
		{
		    var xinputDeviceInfo = deviceInfo as IXInputJoystickInfo;

		    if (xinputDeviceInfo == null)
		    {
                throw new InvalidCastException(Resources.GORINP_XINP_NOT_XINPUT_JOYSTICK);
		    }

			return new XInputController(this, xinputDeviceInfo);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonXInputService"/> class.
		/// </summary>
		public GorgonXInputService()
			: base(Resources.GORINP_XINP_SERVICEDESC)
		{
		}
		#endregion
	}
}
