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
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Contains information about a video device.
	/// </summary>
	public abstract class GorgonVideoDevice
		: GorgonNamedObject, IDisposable
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
		internal void GetDeviceCapabilities()
		{
			Capabilities = CreateDeviceCapabilities();
			Capabilities.EnumerateCapabilities();

			Outputs = new GorgonVideoOutputCollection(GetOutputs());

			foreach(var head in Outputs)
				head.GetOutputModes();
		}

		/// <summary>
		/// Function to return the highest multi sample/AA quality for a given MSAA level.
		/// </summary>
		/// <param name="level">The multi sample level to test.</param>
		/// <param name="format">Format to test for multi sampling capabilities.</param>
		/// <param name="windowed">TRUE if testing for windowed mode, FALSE if not.</param>
		/// <returns>The highest quality for the given level or NULL (Nothing in VB.Net) if the level is not supported.</returns>
		/// <remarks>This method should be used to determine the maximum anti aliasing quality when setting up a <see cref="GorgonLibrary.Graphics.GorgonDeviceWindow">Device Window</see>.</remarks>
		public abstract int? GetMultiSampleQuality(GorgonMSAALevel level, GorgonBufferFormat format, bool windowed);
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoDevice"/> class.
		/// </summary>
		/// <param name="name">Name of the device.</param>
		/// <param name="index">Index of the driver in the collection.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		protected GorgonVideoDevice(string name, int index)
			: base(name)
		{
			GorgonDebug.AssertParamString(name, "name");

			Index = index;
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
