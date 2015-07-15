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
// Created: Friday, July 15, 2011 6:24:19 AM
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Collections;
using Gorgon.Collections.Specialized;
using Gorgon.Input.WinForms.Properties;

namespace Gorgon.Input.WinForms
{
	/// <summary>
	/// Object representing the main interface to the input library.
	/// </summary>
	internal class GorgonWinFormsInputService
		: GorgonInputService
	{
		#region Methods.
		/// <inheritdoc/>
		protected override IGorgonNamedObjectReadOnlyDictionary<IGorgonMouseInfo> OnEnumerateMice()
		{
			return new GorgonNamedObjectDictionary<IGorgonMouseInfo>(false)
			       {
					   new WinFormsMouseInfo()
			       };
		}

		/// <inheritdoc/>
		protected override IGorgonNamedObjectReadOnlyDictionary<IGorgonKeyboardInfo> OnEnumerateKeyboards()
		{
			return new GorgonNamedObjectDictionary<IGorgonKeyboardInfo>(false)
			       {
					   new WinFormsKeyboardInfo()
			       };
		}

		/// <inheritdoc/>
		protected override IGorgonNamedObjectReadOnlyDictionary<IGorgonJoystickInfo> OnEnumerateJoysticks()
		{
			return new GorgonNamedObjectDictionary<IGorgonJoystickInfo>();
		}

		/// <inheritdoc/>
		protected override IGorgonNamedObjectReadOnlyDictionary<IGorgonHumanInterfaceDeviceInfo> OnEnumerateHumanInterfaceDevices()
		{
			return new GorgonNamedObjectDictionary<IGorgonHumanInterfaceDeviceInfo>();
		}

		/// <inheritdoc/>
		protected override GorgonCustomHID OnCreateHumanInterfaceDevice(Control window, IGorgonHumanInterfaceDeviceInfo hidInfo)
		{
			throw new NotSupportedException(Resources.GORINP_ERR_KEYBOARD_MOUSE_ONLY);
		}

		/// <inheritdoc/>
		protected override GorgonKeyboard OnCreateKeyboard(Control window, IGorgonKeyboardInfo keyboardInfo)
		{
			return new WinFormsKeyboard(this, keyboardInfo);
		}

		/// <inheritdoc/>
		protected override GorgonPointingDevice OnCreateMouse(Control window, IGorgonMouseInfo pointingDeviceInfo)
		{
			return new WinFormsPointingDevice(this, pointingDeviceInfo);
		}

		/// <inheritdoc/>
		protected override GorgonJoystick OnCreateJoystick(Control window, IGorgonJoystickInfo joystickInfo)
		{
            throw new NotSupportedException(Resources.GORINP_ERR_KEYBOARD_MOUSE_ONLY);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonWinFormsInputService"/> class.
		/// </summary>
		public GorgonWinFormsInputService()
			: base("Gorgon Windows Forms Input")
		{
		}
		#endregion
	}
}
