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
// Created: Monday, July 13, 2015 8:20:13 PM
// 
#endregion

using System;
using System.Windows.Forms;

namespace Gorgon.Input
{
	/// <summary>
	/// Represents common functionality across all input device types.
	/// </summary>
	public interface IGorgonInputDevice
	{
		#region Properties.
		/// <summary>
		/// Property to return the window that this input device is bound with.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This is the window that will receive data from the input device after a call to <see cref="BindWindow"/>.
		/// </para>
		/// </remarks>
		Control Window
		{
			get;
		}

		/// <summary>
		/// Property to set or return whether the device is acquired or not.
		/// </summary>
		/// <remarks>
		/// <para>
		/// When the device is acquired, the <see cref="Window"/> bound to the device will receive input data from the device. When it is not, no data will be received from the device and if in exclusive mode, 
		/// then the device will begin broadcasting data to other applications again.
		/// </para>
		/// <para>
		/// The device may lose acquisition after its <see cref="Window"/> loses focus. Users will have to set this property to <b>true</b> when the <see cref="Window"/> regains focus. 
		/// </para>
		/// </remarks>
		bool IsAcquired
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return whether the device is exclusive to this application.
		/// </summary>
		/// <remarks>
		/// <para>
		/// When a device is exclusive, only this application will receive data from it. No other applications will be able to receive messages until 
		/// the <see cref="UnbindWindow"/> method is called, or the <see cref="IsAcquired"/> property is set to <b>false</b>.
		/// </para>
		/// <para>
		/// Not all devices support exclusive mode, and as such this property may always return <b>false</b>, regardless of what is passed to the <c>exclusive</c> parameter of the <see cref="BindWindow"/> 
		/// method.
		/// </para>
		/// <para>
		/// <note type="tip">
		/// <para>
		/// When a <see cref="IGorgonMouse"/> is exclusive, it will turn off the system mouse cursor.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		bool IsExclusive
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to bind the input device to the specified window.
		/// </summary>
		/// <param name="window">The window that will receive input device data.</param>
		/// <param name="exclusive">[Optional] <b>true</b> to bind this device exclusively to the window, <b>false</b> to allow the device to be shared with other applications.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="window"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>
		/// <para>
		/// This method will bind the <paramref name="window"/> to the input device, and will allow the capture of input data from that window. When the device is first created, this method must be the first 
		/// one called before working with the device. Otherwise, the device interface will not function correctly.
		/// </para>
		/// <para>
		/// When the <paramref name="exclusive"/> parameter is set to <b>true</b>, it will allow for exclusive ownership of the device data. That is, only this application will receive data from the device and 
		/// no other applications will be able to receive input. This will remain true until either the <see cref="UnbindWindow"/> method is called, or the <see cref="IsAcquired"/> flag is set to <b>false</b>.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// It is very important to call <see cref="UnbindWindow"/> when finished with the device, and before closing the window that is bound. Failure to do so will have unpredictable results.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		void BindWindow(Control window, bool exclusive = false);

		/// <summary>
		/// Function to unbind from the window that the device is bound with.
		/// </summary>
		/// <remarks>
		/// This method must be called when finished with the device, and before the <see cref="Window"/> is destroyed. Failure to do so may result in unpredictable results.
		/// </remarks>
		void UnbindWindow();
		#endregion
	}
}
