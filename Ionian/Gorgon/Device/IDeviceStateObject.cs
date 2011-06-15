#region MIT.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Tuesday, July 12, 2005 12:34:02 AM
// 
#endregion

using System;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Interface for objects so they can handle device resets and loss.
	/// </summary>
	/// <remarks>
	/// When the app is in fullscreen mode, a task switch will cause the device to be put into 
	/// a lost state.  Some resources (e.g. resources created in D3DPOOL_DEFAULT) will end up being
	/// lost as well, and we will need a way to release these objects and restore them when necessary.  
	/// Objects should only implement this interface when a device loss/reset affects the resources
	/// contained within the object.
	/// </remarks>
	public interface IDeviceStateObject
	{
		/// <summary>
		/// Function called when the device is in a lost state.
		/// </summary>
		void DeviceLost();
		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		void DeviceReset();
		/// <summary>
		/// Function to force the loss of the objects data.
		/// </summary>
		void ForceRelease();
	}
}
