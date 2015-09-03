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
// Created: Monday, July 20, 2015 11:55:23 PM
// 
#endregion

using System.Windows.Forms;

namespace Gorgon.Input
{
	/// <summary>
	/// Sends device event data to the appropriate <see cref="IGorgonInputDevice"/> for processing.
	/// </summary>
	/// <remarks>
	/// <para>
	/// When a Gorgon input plug in receives event data from an event driven device (e.g. keyboard, mouse, etc...), it will receive the data from the event in the native format of the input provider 
	/// (e.g. raw input). This data must be formatted into the appropriate data structure (e.g. <see cref="GorgonMouseData"/>) for a given device type so that the input objects can predictably read 
	/// the data and update its state. 
	/// </para>
	/// <para>
	/// To ensure that the correct data is forwarded to the correct device object during an input event, this interface should be applied to the device object so that it will receive data for the native 
	/// event and update state accordingly. 
	/// </para>
	/// <para>
	/// This interface only provides a one way communication from the plug in to the device object. Unidirectional communication is not supported.
	/// </para>
	/// <para>
	/// <note type="important">
	/// <para>
	/// If a user builds a custom device object, and wants it to work with a <see cref="GorgonInputService2"/>, then this interface must be applied so that device events are received.
	/// </para>
	/// <para>
	/// This only applies to devices that receive event data. That is, the device will generate an event when it registers a change. For devices that require the user to capture its current state, see 
	/// the <see cref="IGorgonInputDeviceState"/> interface.
	/// </para>
	/// </note>
	/// </para>
	/// </remarks>
	/// <typeparam name="T">The type of input data that will be received from the plug in.</typeparam>
	/// <seealso cref="IGorgonInputDeviceState"/>
	/// <seealso cref="GorgonKeyboardData"/>
	/// <seealso cref="GorgonMouseData"/>
	/// <seealso cref="GorgonJoystickData"/>
	public interface IGorgonInputEventDrivenDevice<T>
		where T : struct
	{
		/// <summary>
		/// Property to return the window that is bound with the device.
		/// </summary>
		Control Window
		{
			get;
		}

		/// <summary>
		/// Property to return whether the device is in an acquired state or not.
		/// </summary>
		bool IsAcquired
		{
			get;
		}

		/// <summary>
		/// Property to return the type of device we're routing.
		/// </summary>
		InputDeviceType DeviceType
		{
			get;
		}

		/// <summary>
		/// Function to route input device data into events for the device.
		/// </summary>
		/// <param name="data">The data to route.</param>
		/// <returns><b>true</b> if the data was parsed successfully, <b>false</b> if not.</returns>
		bool ParseData(ref T data);
	}
}
