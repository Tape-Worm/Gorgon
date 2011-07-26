#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Thursday, July 21, 2011 3:17:20 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Collections;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Contains information about a video device.
	/// </summary>
	public abstract class GorgonVideoDevice
		: INamedObject, IDisposable
	{
		#region Properties.
		/// <summary>
		/// Property to return the index of the driver in the collection.
		/// </summary>
		public int Index
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the device ID.
		/// </summary>
		public int DeviceID
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the name of the driver for the device.
		/// </summary>
		public string DriverName
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the device name for the device.
		/// </summary>
		public string DeviceName
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the driver version for the device.
		/// </summary>
		public Version DriverVersion
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the revision for the device.
		/// </summary>
		public int Revision
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the sub system ID for the device.
		/// </summary>
		public int SubSystemID
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the vendor ID for the device.
		/// </summary>
		public int VendorID
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the GUID for the device.
		/// </summary>
		public Guid DeviceGUID
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the outputs on this device.
		/// </summary>
		/// <remarks>The outputs are typically monitors attached to the device.</remarks>
		public GorgonVideoOutputCollection Outputs
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the device capabilities.
		/// </summary>
		public GorgonVideoDeviceCapabilities Capabilities
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve and build the device capability information.
		/// </summary>
		/// <returns>The device capabilities.</returns>
		private GorgonVideoDeviceCapabilities GetCaps()
		{
			GorgonVideoDeviceCapabilities result = CreateDeviceCapabilities();
			result.EnumerateCapabilities();
			return result;
		}

		/// <summary>
		/// Function to retrieve device specific information.
		/// </summary>
		/// <remarks>Implementors should use this method to fill in properties like <see cref="GorgonLibrary.Graphics.GorgonVideoDevice.DriverVersion">DriverVersion</see>.</remarks>
		protected abstract void GetDeviceInfo();

		/// <summary>
		/// Function to create a renderer specific device capabilities object.
		/// </summary>
		/// <returns>A video device capabilities object.</returns>
		protected abstract GorgonVideoDeviceCapabilities CreateDeviceCapabilities();

		/// <summary>
		/// Function to retrieve the outputs attached to the device.
		/// </summary>
		/// <returns>An enumerable list of video outputs.</returns>
		protected abstract IEnumerable<GorgonVideoOutput> GetOutputs();

		/// <summary>
		/// Function to retrieve the device capability information.
		/// </summary>
		internal void GetDeviceData()
		{	
			GetDeviceInfo();
			Capabilities = GetCaps();
			Outputs = new GorgonVideoOutputCollection(GetOutputs());

			for (int head = 0; head < Outputs.Count; head++)
				Outputs[head].GetOutputModes();

			Gorgon.Log.Print("Info for Video Device {0}:", Diagnostics.GorgonLoggingLevel.Simple,Name);
			Gorgon.Log.Print("\tDevice Name: {0}", Diagnostics.GorgonLoggingLevel.Simple, DeviceName);
			Gorgon.Log.Print("\tDriver Name: {0}", Diagnostics.GorgonLoggingLevel.Simple, DriverName);
			Gorgon.Log.Print("\tDriver Version: {0}", Diagnostics.GorgonLoggingLevel.Simple, DriverVersion.ToString());
			Gorgon.Log.Print("\tRevision: {0}", Diagnostics.GorgonLoggingLevel.Simple, Revision);
#if DEBUG
			Gorgon.Log.Print("\tDevice ID: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, GorgonUtility.FormatHex(DeviceID));
			Gorgon.Log.Print("\tSub System ID: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, GorgonUtility.FormatHex(SubSystemID));
			Gorgon.Log.Print("\tVendor ID: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, GorgonUtility.FormatHex(VendorID));
			Gorgon.Log.Print("\tDevice GUID: {0}", Diagnostics.GorgonLoggingLevel.Verbose, DeviceGUID);
#endif
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoDevice"/> class.
		/// </summary>
		/// <param name="index">Index of the driver in the collection.</param>
		protected GorgonVideoDevice(int index)
		{
			Index = index;
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		public string Name
		{
			get;
			protected internal set;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			// The base object has nothing to clean up.
			// Dispose is only here for the implementations that require it.
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
