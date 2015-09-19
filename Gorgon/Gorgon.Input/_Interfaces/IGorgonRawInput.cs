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
// Created: Thursday, September 17, 2015 8:15:46 PM
// 
#endregion

using System;
using System.Collections.Generic;

namespace Gorgon.Input
{
	/// <summary>
	/// Raw Input functionality for keyboards, mice and human interface devices.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This enables use of the <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms645536(v=vs.85).aspx">Raw Input</a> functionality provided by windows. 
	/// </para>
	/// <para>
	/// This object will allow for enumeration of multiple keyboard, mouse and human interface devices attached to the system and will allow an application to register these types of devices for use with the 
	/// application.  
	/// </para>
	/// <para>
	/// The <see cref="GorgonRawInput"/> object will also coordinate <c>WM_INPUT</c> messages and forward Raw Input data to an appropriate raw input device. This is done to allow multiple devices of the same 
	/// type (e.g. multiple mice) to be used individually.
	/// </para>
	/// </remarks>
	/// <example>
	/// The following code shows how to create a mouse device and register it with the <see cref="GorgonRawInput"/> object for use in an application:
	/// <code language="csharp">
	/// <![CDATA[
	/// private GorgonRawMouse _mouse;
	/// private GorgonRawInput _rawInput;
	/// 
	/// private void CreateRawMouse(Control yourMainApplicationWindow)
	/// {
	///    // The 'yourMainApplicationWindow' is the primary window used by your application.
	///    _rawInput = new GorgonRawInput(yourMainApplicationWindow);
	/// 
	///    _mouse = new GorgonRawMouse();
	/// 
	///    _rawInput.RegisterDevice(_mouse);
	/// 
	///	   // Configure your mouse object for events here...
	/// }
	/// 
	/// private void ApplicationShutDown()
	/// {
	///		// The device should be unregistered as soon as it's no longer needed.
	///     _rawInput.UnregisterDevice(_mouse);
	/// 
	///		// Always dispose this object, otherwise message hooks may still persist and cause issues.
	///     _rawInput.Dispose();
	/// }
	/// ]]>
	/// </code>
	/// </example>
	public interface IGorgonRawInput 
		: IDisposable, IGorgonKeyboardAndMouseInput
	{
		/// <summary>
		/// Function to register the device with the raw input provider.
		/// </summary>
		/// <param name="device">The device to register with the raw input provider.</param>
		/// <param name="settings">[Optional] Settings for the device type.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="device"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>
		/// <para>
		/// This will register the <see cref="IGorgonRawInputDevice"/> with the application. For the very first device of a specific type (e.g. a mouse, keyboard, etc...) the Raw Input object will set up 
		/// the device type registration for the device. This enables an application to start receiving Raw Input messages from a device type.
		/// </para>
		/// <para>
		/// The optional <paramref name="settings"/> parameter allows an application change how raw input handles the device being registered. It can be used to set up background input monitoring, or a 
		/// target window for raw input messages (which must be set if the background option is turned on). By default, there is no background message processing and no target window (messages go to 
		/// whichever window has focus).
		/// </para>
		/// <para>
		/// Every call to this method should be paired with a call to <see cref="GorgonRawInput.UnregisterDevice"/> when the device(s) are no longer needed.
		/// </para>
		/// </remarks>
		void RegisterDevice(IGorgonRawInputDevice device, GorgonRawInputSettings? settings = null);

		/// <summary>
		/// Function to unregister the device from the raw input provider.
		/// </summary>
		/// <param name="device">The device to unregister from the raw input provider.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="device"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>
		/// This will unregister a previously registered <see cref="IGorgonRawInputDevice"/>. When the last device of a specific type (e.g. a mouse, keyboard, etc...) is unregistered, then the 
		/// Raw Input messages for that device type will also be unregistered and the application will no longer receive messages from that type of device.
		/// </remarks>
		void UnregisterDevice(IGorgonRawInputDevice device);

		/// <summary>
		/// Function to retrieve a list of human interface devices (HID).
		/// </summary>
		/// <returns>A read only list containing information about each human interface device.</returns>
		IReadOnlyList<GorgonRawHIDInfo> EnumerateHumanInterfaceDevices();
	}
}