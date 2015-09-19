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
// Created: Sunday, September 13, 2015 1:54:33 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Diagnostics;
using Gorgon.Plugins;

namespace Gorgon.Input
{
	/// <inheritdoc/>
	public abstract class GorgonGamingDeviceDriver
		: GorgonPlugin, IGorgonGamingDeviceDriver
	{
		#region Properties.
		/// <summary>
		/// Property to return the logger used for debugging.
		/// </summary>
		protected internal IGorgonLog Log
		{
			get;
			internal set;
		}
		#endregion

		#region Methods.
		/// <inheritdoc/>
		public abstract IReadOnlyList<IGorgonGamingDeviceInfo> EnumerateGamingDevices(bool connectedOnly = false);

		/// <inheritdoc/>
		public abstract IGorgonGamingDevice CreateGamingDevice(IGorgonGamingDeviceInfo gamingDeviceInfo);

		/// <inheritdoc/>
		public IReadOnlyList<IGorgonGamingDevice> CreateGamingDevices(bool connectedOnly = false)
		{
			IReadOnlyList<IGorgonGamingDeviceInfo> infoList = EnumerateGamingDevices(connectedOnly);

			if ((infoList == null) || (infoList.Count == 0))
			{
				return new GorgonGamingDevice[0];
			}

			var result = new List<IGorgonGamingDevice>();

			foreach (IGorgonGamingDeviceInfo deviceInfo in infoList)
			{
				IGorgonGamingDevice device = CreateGamingDevice(deviceInfo);

				if ((!device.IsConnected) && (connectedOnly))
				{
					device.Dispose();
					continue;
				}

				result.Add(device);
			}

			return result;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <remarks>
		/// For implementors of a <see cref="GorgonGamingDeviceDriver"/> this dispose method is used to clean up any native resources that may be allocated by the driver. In such a case, put the clean up code for 
		/// the native resources in an override of this method. If the driver does not use native resources, then this method should be left alone.
		/// </remarks>
		public virtual void Dispose()
		{
			GC.SuppressFinalize(this);
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGamingDeviceDriver"/> class.
		/// </summary>
		/// <param name="description">The human readable description of the driver.</param>
		protected GorgonGamingDeviceDriver(string description)
			: base(description)
		{
			Log = new GorgonLogDummy();
		}
		#endregion

	}
}
