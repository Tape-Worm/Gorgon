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
// Created: Saturday, September 12, 2015 1:47:55 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Input.Properties;
using Gorgon.PlugIns;

namespace Gorgon.Input
{
	/// <summary>
	/// A factory used to load gaming device drivers.
	/// </summary>
	public sealed class GorgonGamingDeviceDriverFactory 
		: IGorgonGamingDeviceDriverFactory
	{
		#region Variables.
		// The logger used for debugging.
		private readonly IGorgonLog _log;
		// The plug in service to use when loading drivers.
		private readonly IGorgonPlugInService _plugInService;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to load all drivers from the plug in assemblies that are currently loaded.
        /// </summary>
        /// <returns>A read only list containing an instance of each driver.</returns>
        public IReadOnlyList<IGorgonGamingDeviceDriver> LoadAllDrivers() => _plugInService.GetPlugIns<GorgonGamingDeviceDriver>();

        /// <summary>
        /// Function to load a gaming device driver from any loaded plug in assembly.
        /// </summary>
        /// <param name="driverType">The fully qualified type name of the driver to load.</param>
        /// <returns>The gaming device driver plug in.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="driverType"/> parameter is <b>null</b></exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="driverType"/> parameter is empty.</exception>
        /// <exception cref="ArgumentException">Thrown when the driver type name specified by <paramref name="driverType"/> was not found in any of the loaded plug in assemblies.</exception>
        public IGorgonGamingDeviceDriver LoadDriver(string driverType)
		{
			if (driverType == null)
			{
				throw new ArgumentNullException(nameof(driverType));
			}

			if (string.IsNullOrWhiteSpace(driverType))
			{
				throw new ArgumentEmptyException(nameof(driverType));
			}

			GorgonGamingDeviceDriver result = _plugInService.GetPlugIn<GorgonGamingDeviceDriver>(driverType);

			if (result == null)
			{
				throw new ArgumentException(string.Format(Resources.GORINP_ERR_DRIVER_NOT_FOUND, driverType));
			}

			result.Log = _log;

			return result;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGamingDeviceDriverFactory"/> class.
		/// </summary>
		/// <param name="plugInService">The plug in service to use when loading drivers.</param>
		/// <param name="log">[Optional] The logger used for debugging.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugInService"/> is <b>null</b>.</exception>
		public GorgonGamingDeviceDriverFactory(IGorgonPlugInService plugInService, IGorgonLog log = null)
		{
			_log = log ?? GorgonLog.NullLog;
			_plugInService = plugInService;
		}
		#endregion
	}
}
