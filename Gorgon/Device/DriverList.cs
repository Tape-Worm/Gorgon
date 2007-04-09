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
// Created: Monday, July 11, 2005 2:34:03 PM
// 
#endregion

// Undefine this to speed up driver enumerator.
#define WHQL

using System;
using SharpUtilities;
using SharpUtilities.Utility;
using SharpUtilities.Collections;
using DX = Microsoft.DirectX;
using D3D = Microsoft.DirectX.Direct3D;

namespace GorgonLibrary
{
	/// <summary>
	/// Class holding a list of available video drivers.
	/// </summary>
	public class DriverList : BaseDynamicArray<Driver>
	{
		#region Properties.
		/// <summary>
		/// Property to return a driver by its name.
		/// </summary>
		/// <remarks>This property can come in handy when restoring a driver selection from the registry.</remarks>
		/// <param name="driverName">Name of the driver to return.</param>
		/// <returns>A value type representing driver information.</returns>
		public Driver this[string driverName]
		{
			get
			{
				// Look for the driver.
				foreach (Driver drv in this)
				{
					if (driverName.ToLower() == drv.DriverName.ToLower())
						return drv;
				}
				throw new KeyNotFoundException(driverName);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to enumerate drivers and get capabilities.
		/// </summary>
		public void Refresh()
		{
			Driver driver;						// New driver query.
			D3D.Caps driverCapabilities;			// Driver capabilities.
			int i;									// Loop.

			Gorgon.Log.Print("DriverList", "Retrieving driver list...", LoggingLevel.Intermediate);

			try
			{
				_items.Clear();

				// Loop through list and build.
				for (i = 0; i < D3D.Manager.Adapters.Count; i++)
				{
					driverCapabilities = D3D.Manager.GetDeviceCaps(i, RenderWindow.DeviceType);

					// Only add drivers that support hardware rendering, or if we've chosen the 
					// reference device upon startup, then allow all drivers.
#if INCLUDE_D3DREF
					if ((driverCapabilities.DeviceCaps.SupportsHardwareRasterization) || (Gorgon.UseReferenceDevice))
#else
				if (driverCapabilities.DeviceCaps.SupportsHardwareRasterization)
#endif
					{
						Gorgon.Log.Print("DriverList", "Getting video adapter information for device #{0}...", LoggingLevel.Intermediate, i);
						driver = new Driver(i);
						_items.Add(driver);
						Gorgon.Log.Print("DriverList", "Driver information for {0} retrieved.", LoggingLevel.Intermediate, _items[_items.Count - 1].Description);
					}
				}

				// If we didn't find any adapters that met the hardware only criteria, then quit out.
				if (_items.Count == 0)
					throw new NoHWAcceleratedDevicesException(null);

				Gorgon.Log.Print("DriverList", "{0} drivers enumerated.", LoggingLevel.Simple, _items.Count);
			}
			catch (SharpException sEx)
			{
				throw sEx;
			}
			catch (Exception ex)
			{
				throw new CannotEnumerateVideoDevicesException(ex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal DriverList() : base(16)
		{
			Refresh();
		}
		#endregion
	}
}
