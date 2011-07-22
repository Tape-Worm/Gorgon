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
		/// Property to return the device capabilities.
		/// </summary>
		public GorgonCapabilityCollection Capabilities
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the pixel shader version.
		/// </summary>
		public Version PixelShaderVersion
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the vertex shader version.
		/// </summary>
		public Version VertexShaderVersion
		{
			get;
			protected set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the device capabilities.
		/// </summary>
		/// <returns>An enumerable list of driver capabilities.</returns>
		protected abstract IEnumerable<KeyValuePair<string, string>> GetDeviceCapabilities();

		/// <summary>
		/// Function to retrieve the device capability information.
		/// </summary>
		internal void GetDeviceData()
		{
			Capabilities = new GorgonCapabilityCollection();
			Capabilities.AddCapabilities(GetDeviceCapabilities());
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
			protected set;
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
