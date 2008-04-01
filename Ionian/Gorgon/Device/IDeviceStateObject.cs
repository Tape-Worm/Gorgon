#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
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
