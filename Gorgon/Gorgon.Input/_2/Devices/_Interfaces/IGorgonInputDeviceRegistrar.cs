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
// Created: Wednesday, August 19, 2015 10:53:20 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gorgon.Input
{
	/// <summary>
	/// A device registration interface for a <see cref="GorgonInputService2"/>.
	/// </summary>
	public interface IGorgonInputDeviceRegistrar
	{
		#region Methods.
		/// <summary>
		/// Function to register a device when it binds with a window.
		/// </summary>
		/// <param name="device">The device that is being bound to the window.</param>
		/// <param name="info">Information about the device being bound to the window.</param>
		/// <param name="window">The window that the device is being bound with.</param>
		/// <param name="exclusive">[Optional] <b>true</b> if the device is being registered as exclusive, <b>false</b> if not.</param>
		/// <returns><b>true</b> if the <paramref name="exclusive"/> parameter was set to <b>true</b>, and the device was successfully registered as exclusive. <b>False</b> if the device did not register successfully as exclusive.</returns>
		/// <remarks>
		/// <para>
		/// This method is used to allow the service to track and coordinate sending/receiving data to/from <see cref="IGorgonInputDevice"/> objects. Input devices will register themselves with the appropriate 
		/// <see cref="GorgonInputService2"/> when binding to a window. 
		/// </para>
		/// <para>
		/// Because multiple device objects (regardless of whether there are multiple physical devices or not) can be created and bound to different windows, implementors of the input service plug ins must have 
		/// some method of tracking these device objects. This is required because input data received from a physical device must be routed to the proper device bound to a specific window. For example, creating 
		/// two instances of <see cref="GorgonMouse"/> with the system mouse, but binding them to two different <see cref="Panel"/> controls would require that when the mouse is moved over the first panel, that 
		/// only the first instance of <see cref="GorgonMouse"/> has its state updated, while the other should be ignored. How this is accomplished is left up to the implementor of the input service plug in.
		/// </para>
		/// </remarks>
		bool RegisterDevice(IGorgonInputDevice device, IGorgonInputDeviceInfo2 info, Control window, bool exclusive = false);

		/// <summary>
		/// Function to unregister a device when it unbinds from a window.
		/// </summary>
		/// <param name="device">The device that is being unbound from the window.</param>
		/// <remarks>
		/// This method is meant to clean up and unhook any <see cref="IGorgonInputDevice"/> object from the input service. This will ensure that unregistered devices do not call back into the service and update 
		/// state.
		/// </remarks>
		/// <seealso cref="RegisterDevice"/>
		void UnregisterDevice(IGorgonInputDevice device);
		#endregion
	}
}
