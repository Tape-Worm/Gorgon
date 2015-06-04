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
	internal class GorgonWinFormsInputFactory
		: GorgonInputFactory
	{
		#region Methods.
		/// <summary>
		/// Function to enumerate the pointing devices on the system.
		/// </summary>
		/// <returns>A list of pointing device names.</returns>
		protected override IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> EnumeratePointingDevices()
		{
			return new GorgonNamedObjectDictionary<GorgonInputDeviceInfo>(false)
			       {
				       new GorgonWinFormsInputDeviceInfo("System Mouse", InputDeviceType.PointingDevice, "SysMouse", "SysMouse")
			       };
		}

		/// <summary>
		/// Function to enumerate the keyboard devices on the system.
		/// </summary>
		/// <returns>A list of keyboard device names.</returns>
		protected override IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> EnumerateKeyboardDevices()
		{
			return new GorgonNamedObjectDictionary<GorgonInputDeviceInfo>(false)
			       {
				       new GorgonWinFormsInputDeviceInfo("System Keyboard", InputDeviceType.Keyboard, "SysKeyboard", "SysKeyboard")
			       };
		}

		/// <summary>
		/// Function to enumerate the joystick devices attached to the system.
		/// </summary>
		/// <returns>A list of joystick device names.</returns>
		protected override IGorgonNamedObjectReadOnlyDictionary<GorgonInputDeviceInfo> EnumerateJoysticksDevices()
		{
			return new GorgonNamedObjectDictionary<GorgonInputDeviceInfo>();
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
		/// Function to create a custom HID interface.
		/// </summary>
		/// <param name="window">Window to bind with.</param>
		/// <param name="hidInfo">A <see cref="Gorgon.Input.GorgonInputDeviceInfo">GorgonDeviceName</see> object containing the HID information.</param>
		/// <returns>
		/// A new custom HID interface.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="hidInfo"/> is NULL.</exception>
		protected override GorgonCustomHID CreateCustomHIDImpl(Control window, GorgonInputDeviceInfo hidInfo)
		{
			throw new NotSupportedException(Resources.GORINP_WIN_KEYBOARD_MOUSE_ONLY);
		}

		/// <summary>
		/// Function to create a keyboard interface.
		/// </summary>
		/// <param name="keyboardInfo">Name of the keyboard device to create.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new keyboard interface.</returns>
		/// <remarks>Passing NULL for <paramref name="keyboardInfo"/> will use the system keyboard.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</para></remarks>
		protected override GorgonKeyboard CreateKeyboardImpl(Control window, GorgonInputDeviceInfo keyboardInfo)
		{
			return new WinFormsKeyboard(this);
		}

		/// <summary>
		/// Function to create a pointing device interface.
		/// </summary>
		/// <param name="pointingDeviceInfo">Name of the pointing device device to create.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new pointing device interface.</returns>
		/// <remarks>Passing NULL for <paramref name="pointingDeviceInfo"/> will use the system pointing device.
		/// <para>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</para>
		/// </remarks>
		protected override GorgonPointingDevice CreatePointingDeviceImpl(Control window, GorgonInputDeviceInfo pointingDeviceInfo)
		{
			return new WinFormsPointingDevice(this);
		}

		/// <summary>
		/// Function to create a joystick interface.
		/// </summary>
		/// <param name="joystickInfo">A <see cref="Gorgon.Input.GorgonInputDeviceInfo">GorgonDeviceName</see> object containing the joystick information.</param>
		/// <param name="window">Window to bind with.</param>
		/// <returns>A new joystick interface.</returns>
		/// <remarks>Pass NULL to the <paramref name="window"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</remarks>
		/// <exception cref="System.ArgumentNullException">The <paramRef name="joystickInfo"/> is NULL.</exception>
		protected override GorgonJoystick CreateJoystickImpl(Control window, GorgonInputDeviceInfo joystickInfo)
		{
            throw new NotSupportedException(Resources.GORINP_WIN_KEYBOARD_MOUSE_ONLY);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonWinFormsInputFactory"/> class.
		/// </summary>
		public GorgonWinFormsInputFactory()
			: base("Gorgon Windows Forms Input")
		{
		}
		#endregion
	}
}
