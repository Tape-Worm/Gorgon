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
// Created: Thursday, July 21, 2011 3:15:20 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Native;
using GorgonLibrary.Collections;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A collection of the video devices installed in the system and their outputs, and modes.
	/// </summary>
	/// <remarks>This object is used to enumerate all the video devices present within the system, and will provide a <see cref="GorgonLibrary.Graphics.GorgonVideoDevice">GorgonVideoDevice</see> object to use 
	/// when specific device selection is required.</remarks>
	public class GorgonVideoDeviceCollection
		: GorgonBaseNamedObjectList<GorgonVideoDevice>, IDisposable
	{
		#region Variables.
		private bool _disposed = false;			// Flag to indicate whether the device enumerator was disposed or not.
		private GI.Factory1 _factory = null;	// Factory object.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a video device by its index.
		/// </summary>
		public GorgonVideoDevice this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return a video device by its name.
		/// </summary>
		public GorgonVideoDevice this[string name]
		{
			get
			{
				return GetItem(name);
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		public override bool IsReadOnly
		{
			get
			{
				return true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to print device log information.
		/// </summary>
		/// <param name="device">Device to print.</param>
		private void PrintLog(GorgonVideoDevice device)
		{
			if (device.VideoDeviceType == VideoDeviceType.ReferenceRasterizer)
				Gorgon.Log.Print("Device found: {0} ---> !!!** WARNING:  A reference rasterizer has very poor performance.", Diagnostics.LoggingLevel.Simple, device.Name);
			else
				Gorgon.Log.Print("Device found: {0}", Diagnostics.LoggingLevel.Simple, device.Name);
			Gorgon.Log.Print("===================================================================", Diagnostics.LoggingLevel.Verbose);
			Gorgon.Log.Print("Hardware feature level: {0}", Diagnostics.LoggingLevel.Verbose, device.HardwareFeatureLevel);
			Gorgon.Log.Print("Limited to feature level: {0}", Diagnostics.LoggingLevel.Verbose, device.SupportedFeatureLevel);
			Gorgon.Log.Print("Video memory: {0}", Diagnostics.LoggingLevel.Verbose, device.DedicatedVideoMemory.FormatMemory());
			Gorgon.Log.Print("System memory: {0}", Diagnostics.LoggingLevel.Verbose, device.DedicatedSystemMemory.FormatMemory());
			Gorgon.Log.Print("Shared memory: {0}", Diagnostics.LoggingLevel.Verbose, device.SharedSystemMemory.FormatMemory());
			Gorgon.Log.Print("Device ID: 0x{0}", Diagnostics.LoggingLevel.Verbose, device.DeviceID.FormatHex());
			Gorgon.Log.Print("Sub-system ID: 0x{0}", Diagnostics.LoggingLevel.Verbose, device.SubSystemID.FormatHex());
			Gorgon.Log.Print("Vendor ID: 0x{0}", Diagnostics.LoggingLevel.Verbose, device.VendorID.FormatHex());
			Gorgon.Log.Print("Revision: {0}", Diagnostics.LoggingLevel.Verbose, device.Revision);
			Gorgon.Log.Print("Unique ID: 0x{0}", Diagnostics.LoggingLevel.Verbose, device.UUID.FormatHex());
			Gorgon.Log.Print("===================================================================", Diagnostics.LoggingLevel.Verbose);
		}

		/// <summary>
		/// Function to add the software devices.
		/// </summary>
		/// <param name="includeRef">TRUE to include the reference device, FALSE to exclude it.</param>
		/// <param name="includeWARP">TRUE to include the WARP device, FALSE to exclude it.</param>
		private void AddSoftwareDevices(bool includeRef, bool includeWARP)
		{
			D3D.Device d3dDevice = null;
			GI.Device1 giDevice = null;
			GI.Adapter1 adapter = null;
			GorgonVideoDevice device = null;

			try
			{
				if (includeWARP)
				{
					// Create the WARP rasterizer.
					d3dDevice = new D3D.Device(SharpDX.Direct3D.DriverType.Warp, D3D.DeviceCreationFlags.Debug);
					giDevice = d3dDevice.QueryInterface<GI.Device1>();
					adapter = giDevice.GetParent<GI.Adapter1>();
					device = new GorgonVideoDevice(adapter, VideoDeviceType.Software);
					giDevice.Dispose();

					this.AddItem(device);

					PrintLog(device);

					// Get the outputs for the device.
					device.Outputs.Refresh(d3dDevice);
				}

#if DEBUG
				if (includeRef)
				{
					// Create a reference rasterizer.
					d3dDevice = new D3D.Device(SharpDX.Direct3D.DriverType.Reference, D3D.DeviceCreationFlags.Debug);
					giDevice = d3dDevice.QueryInterface<GI.Device1>();
					adapter = giDevice.GetParent<GI.Adapter1>();
					device = new GorgonVideoDevice(adapter, VideoDeviceType.ReferenceRasterizer);
					giDevice.Dispose();

					this.AddItem(device);

					PrintLog(device);

					// Get the outputs for the device.
					device.Outputs.Refresh(d3dDevice);
				}
#endif
			}
			finally
			{
				if (d3dDevice != null)
					d3dDevice.Dispose();
			}
		}

		/// <summary>
		/// Function to enumerate the video devices attached to the computer.
		/// </summary>
		/// <param name="includeRef">TRUE to include the reference device, FALSE to exclude it.</param>
		/// <param name="includeWARP">TRUE to include the WARP device, FALSE to exclude it.</param>
		private void Enumerate(bool includeRef, bool includeWARP)
		{
			D3D.Device d3dDevice = null;

			ClearItems();

			Gorgon.Log.Print("Enumerating video devices...", Diagnostics.LoggingLevel.Simple);

			int adapterCount = 1;

			// Create our factory object.
			_factory = new GI.Factory1();
			adapterCount = _factory.GetAdapterCount1();

			for (int i = 0; i < adapterCount; i++)
			{
				GorgonVideoDevice device = null;
				Gorgon.Log.Print("Creating DXGI adapter interface...", Diagnostics.LoggingLevel.Verbose);
				GI.Adapter1 adapter = null;

				adapter = _factory.GetAdapter1(i);
				if (adapter.Description1.Flags != GI.AdapterFlags.Remote) 
				{
					device = new GorgonVideoDevice(adapter, VideoDeviceType.Hardware);

					// If this device is not supported, then leave.
					if (device.HardwareFeatureLevel == DeviceFeatureLevel.Unsupported)
						continue;

					d3dDevice = device.CreateD3DDeviceNoLogging(device.HardwareFeatureLevel);

					try
					{
						this.AddItem(device);

						PrintLog(device);

						// Get the outputs for the device.
						device.Outputs.Refresh(d3dDevice);
					}
					finally
					{
						if (d3dDevice != null)
							d3dDevice.Dispose();
					}
				}
			}

			AddSoftwareDevices(includeRef, includeWARP);

			Gorgon.Log.Print("Found {0} video devices.", Diagnostics.LoggingLevel.Simple, Count);
		}

		/// <summary>
		/// Function to clear the items from the collection.
		/// </summary>
		protected override void ClearItems()
		{
			foreach (var item in this)
				((IDisposable)item).Dispose();

			base.ClearItems();
		}		
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoDeviceCollection"/> class.
		/// </summary>
		/// <param name="includeReference">TRUE to include the reference rasterizer rendering device, FALSE to exclude it.</param>
		/// <param name="includeWARP">TRUE to include the WARP software rasterizer, FALSE to exclude it.</param>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown if no suitable video devices are available on the system.</exception>
		/// <remarks>If the <paramref name="includeReference"/> flag is TRUE, then the reference rasterizer will only appear in Debug mode.  If the library is in Release mode, then the reference rasterizer  
		/// will not be included.  If the Reference Rasterizer is to be used, then Direct3D 11 SDK -must- be installed as the reference rasterizer is only included with
		/// the SDK.
		/// <para>The reference rasterizer is very slow, and should only be used to locate an issue with a driver.</para></remarks>
		public GorgonVideoDeviceCollection(bool includeReference, bool includeWARP)
			: base(false)
		{
			Enumerate(includeReference, includeWARP);
			if (Count == 0)
				throw new GorgonException(GorgonResult.CannotCreate, "There were no video devices found on this system that can use Direct 3D 11/SM5, 10.x/SM4 or 9.0/SM3.");
		}
		#endregion
	
		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					ClearItems();

					if (_factory != null)
						_factory.Dispose();
				}

				_factory = null;
				_disposed = true;
			}
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
