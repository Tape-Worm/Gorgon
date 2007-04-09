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
// Created: Tuesday, July 12, 2005 12:34:12 AM
// 
#endregion

using System;
using SharpUtilities;
using SharpUtilities.Utility;
using SharpUtilities.Collections;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Class to handle device resets and loss.
	/// </summary>
	/// <remarks>
	/// When the app is in fullscreen mode, a task switch will cause the device to be put into 
	/// a lost state.  Some resources (e.g. resources created in D3DPOOL_DEFAULT) will end up being
	/// lost as well, and we will need a way to release these objects and restore them when necessary.  
	/// This class will contain all objects that can be lost and reset when such the above situation occours.  
	/// This class should only be used internally and never called upon by the calling application.
	/// </remarks>
	public class DeviceStateList : DynamicArray<IDeviceStateObject>, IDisposable
	{
		#region Methods.
		/// <summary>
		/// Function to add a device state object to the list.
		/// </summary>
		/// <param name="deviceStateObject">Device state object to add.</param>
		public void Add(IDeviceStateObject deviceStateObject)
		{
			Gorgon.Log.Print("DeviceStateList", "Adding state object: {0}: {1}", LoggingLevel.Verbose, deviceStateObject.GetType().Name,deviceStateObject.ToString());
			_items.Add(deviceStateObject);
		}

		/// <summary>
		/// Function to remove a device state object from the list.
		/// </summary>
		/// <param name="deviceStateObject">Object to remove.</param>
		public void Remove(IDeviceStateObject deviceStateObject)
		{
			Gorgon.Log.Print("DeviceStateList", "Removing state object: {0}", LoggingLevel.Verbose, deviceStateObject.GetType().Name);
			_items.Remove(deviceStateObject);
		}

		/// <summary>
		/// Function to call when the device is lost.
		/// </summary>
		public void DeviceWasLost()
		{
			int i;	// Loop.

			for (i = 0; i < _items.Count; i++)
				_items[i].DeviceLost();		
		}

		/// <summary>
		/// Function to call when the device is reset.
		/// </summary>
		public void DeviceWasReset()
		{
			int i;	// Loop.

			for (i = 0; i < _items.Count; i++)
				_items[i].DeviceReset();
		}

		/// <summary>
		/// Function to force the release of data from all device state objects.
		/// </summary>
		public void ForceRelease()
		{
			int i;	// Loop.

			for (i = 0; i < _items.Count; i++)
				_items[i].ForceRelease();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal DeviceStateList()
		{		
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			_items.Clear();
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
