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
				if (string.IsNullOrEmpty(driverName))
					throw new ArgumentNullException("driverName");

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

			ClearItems();

			// Loop through list and build.
			for (i = 0; i < Gorgon.Direct3D.Adapters.Count; i++)
			{
				driverCapabilities = Gorgon.Direct3D.GetDeviceCaps(i, Driver.DeviceType);

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

            // If we didn't find any adapters that met the hardware only criteria, then quit out.
			if (Count == 0)
				throw new GorgonException(GorgonErrors.NoHALDevices);
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
