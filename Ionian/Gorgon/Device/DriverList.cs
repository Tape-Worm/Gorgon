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

using System;
using System.Collections;
using System.Collections.Generic;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Graphics;

namespace GorgonLibrary
{
	/// <summary>
	/// A list of available video drivers.
	/// </summary>
	public class DriverList : BaseDynamicArray<Driver>
	{
		#region Properties.
		/// <summary>
		/// Property to return a driver by its index.
		/// </summary>
		/// <exception cref="System.IndexOutOfRangeException">Thrown when the index of the driver falls outside of the list range.</exception>
		public Driver this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return a driver by its name.
		/// </summary>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the name of the driver does not exist within the list.</exception>
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

                throw new KeyNotFoundException("The driver '" + driverName + "' was not found in this list.");
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to enumerate drivers and get capabilities.
		/// </summary>
		public void Refresh()
		{
			Driver driver;							// New driver query.
			D3D9.Capabilities driverCapabilities;	// Driver capabilities.
			int i;									// Loop.

			Gorgon.Log.Print("DriverList", "Retrieving driver list...", LoggingLevel.Intermediate);

			try
			{
				ClearItems();

				// Loop through list and build.
				for (i = 0; i < D3D9.Direct3D.Adapters.Count; i++)
				{
					driverCapabilities = D3D9.Direct3D.GetDeviceCaps(i, Driver.DeviceType);

					// Only add drivers that support hardware rendering, or if we've chosen the 
					// reference device upon startup, then allow all drivers.
#if INCLUDE_D3DREF
					if (((driverCapabilities.DeviceCaps & D3D9.DeviceCaps.HWRasterization) == D3D9.DeviceCaps.HWRasterization) || (Gorgon.UseReferenceDevice))
#else
					if ((driverCapabilities.DeviceCaps & D3D9.DeviceCaps.HWRasterization) == D3D9.DeviceCaps.HWRasterization)
#endif
					{
						Gorgon.Log.Print("DriverList", "Getting video adapter information for device #{0}...", LoggingLevel.Intermediate, i);
						driver = new Driver(i);						
						Items.Add(driver);
						Gorgon.Log.Print("DriverList", "Driver information for {0} retrieved.", LoggingLevel.Intermediate, Items[Count - 1].Description);
					}
				}
				Gorgon.Log.Print("DriverList", "{0} drivers enumerated.", LoggingLevel.Simple, Count);
			}
			catch (Exception ex)
			{
				throw new CannotEnumerateException("video devices", ex);
			}

            // If we didn't find any adapters that met the hardware only criteria, then quit out.
            if (Count == 0)
                throw new CannotEnumerateException("video devices", "No hardware accelerated devices present in the system.");
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal DriverList()
			: base(16)
		{
		}
		#endregion
	}
}
