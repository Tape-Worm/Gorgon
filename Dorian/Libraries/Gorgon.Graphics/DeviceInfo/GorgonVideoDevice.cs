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

// Uncomment this line to force DX10.1 feature levels.
//#define DX10_1_CARD
// Uncomment this line to force DX10.0 feature levels.
//#define DX10_CARD
// Uncomment this line to force DX9 feature levels.
//#define DX9_CARD

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Collections;
using GorgonLibrary.Diagnostics;
using GI = SlimDX.DXGI;
using D3D = SlimDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Contains information about a video device.
	/// </summary>
	public class GorgonVideoDevice
		: INamedObject, IDisposable
	{
		#region Variables.
		private bool _disposed = false;						// Flag to indicate that the object was disposed.		
		private D3D.Device _device = null;					// D3D 11 device object.
		private D3D.FeatureLevel _highestFeatureLevel;		// Highest feature level.
		private static object _lockSync = new object();		// Lock sync for thread locking.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the DX GI adapter interface for this video device.
		/// </summary>
		internal GI.Adapter1 GIAdapter
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the feature level that this device supports.
		/// </summary>
		/// <remarks>This is used to determine whether a device is a DirectX 11/10/10.1/9 capable device.</remarks>
		public Version FeatureLevelVersion
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the device ID.
		/// </summary>
		public int DeviceID
		{
			get
			{
				return GIAdapter.Description1.DeviceId;
			}
		}

		/// <summary>
		/// Property to return the unique identifier for the device.
		/// </summary>
		public long UUID
		{
			get
			{
				return GIAdapter.Description1.Luid;
			}
		}

		/// <summary>
		/// Property to return the revision for the device.
		/// </summary>
		public int Revision
		{
			get
			{
				return GIAdapter.Description1.Revision;
			}
		}

		/// <summary>
		/// Property to return the sub system ID for the device.
		/// </summary>
		public int SubSystemID
		{
			get
			{
				return GIAdapter.Description1.SubsystemId;
			}
		}

		/// <summary>
		/// Property to return the vendor ID for the device.
		/// </summary>
		public int VendorID
		{
			get
			{
				return GIAdapter.Description1.VendorId;
			}
		}

		/// <summary>
		/// Property to return the amount of dedicated system memory for the device, in bytes.
		/// </summary>
		public long DedicatedSystemMemory
		{
			get
			{
				return GIAdapter.Description1.DedicatedSystemMemory;
			}
		}

		/// <summary>
		/// Property to return the amount of dedicated video memory for the device, in bytes.
		/// </summary>
		public long DedicatedVideoMemory
		{
			get
			{
				return GIAdapter.Description1.DedicatedVideoMemory;
			}
		}

		/// <summary>
		/// Property to return the amount of shared system memory for the device.
		/// </summary>
		public long SharedSystemMemory
		{
			get
			{
				return GIAdapter.Description1.SharedSystemMemory;
			}
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
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve feature levels for the hardware.
		/// </summary>
		/// <returns>An array of feature levels to use.</returns>
		private D3D.FeatureLevel[] GetFeatureLevels()
		{
			switch (_highestFeatureLevel)
			{
				case D3D.FeatureLevel.Level_11_0:
					return new D3D.FeatureLevel[] { D3D.FeatureLevel.Level_11_0, D3D.FeatureLevel.Level_10_1, D3D.FeatureLevel.Level_10_0, D3D.FeatureLevel.Level_9_3, D3D.FeatureLevel.Level_9_2, D3D.FeatureLevel.Level_9_1 };
				case D3D.FeatureLevel.Level_10_1:
					return new D3D.FeatureLevel[] { D3D.FeatureLevel.Level_10_1, D3D.FeatureLevel.Level_10_0, D3D.FeatureLevel.Level_9_3, D3D.FeatureLevel.Level_9_2, D3D.FeatureLevel.Level_9_1 };
				case D3D.FeatureLevel.Level_10_0:
					return new D3D.FeatureLevel[] { D3D.FeatureLevel.Level_10_0, D3D.FeatureLevel.Level_9_3, D3D.FeatureLevel.Level_9_2, D3D.FeatureLevel.Level_9_1 };
				case D3D.FeatureLevel.Level_9_3:
					return new D3D.FeatureLevel[] { D3D.FeatureLevel.Level_9_3, D3D.FeatureLevel.Level_9_2, D3D.FeatureLevel.Level_9_1 };
				default:
					return null;
			}
		}

		/// <summary>
		/// Function to retrieve the D3D 11 device object associated with this video device.
		/// </summary>
		/// <returns>A D3D 11 device interface.</returns>
		internal D3D.Device GetDevice()
		{
			lock (_lockSync)
			{
				D3D.DeviceCreationFlags flags = D3D.DeviceCreationFlags.None;

				if (_device == null)
				{
#if DEBUG
					flags = D3D.DeviceCreationFlags.Debug;
#endif
					Gorgon.Log.Print("Creating D3D 11 device for video device '{0}'...", GorgonLoggingLevel.Verbose, Name);
					_device = new D3D.Device(GIAdapter, flags, GetFeatureLevels());
				}

				return _device;
			}
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format("Gorgon Graphics Device: {0}", Name);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoDevice"/> class.
		/// </summary>
		/// <param name="adapter">DXGI video adapter.</param>
		internal GorgonVideoDevice(GI.Adapter1 adapter)
		{
			if (adapter == null)
				throw new ArgumentNullException("adapter");

			GIAdapter = adapter;

#if DX10_1_CARD
			_highestFeatureLevel = D3D.FeatureLevel.Level_10_1;
#elif DX10_CARD
			_highestFeatureLevel = D3D.FeatureLevel.Level_10_0;
#elif DX9_CARD
			_highestFeatureLevel = D3D.FeatureLevel.Level_9_3;
#else
			_highestFeatureLevel = D3D.Device.GetSupportedFeatureLevel(adapter);
#endif

			switch (_highestFeatureLevel)
			{
				case D3D.FeatureLevel.Level_11_0:
					FeatureLevelVersion = new Version(11, 0);
					break;
				case D3D.FeatureLevel.Level_10_1:
					FeatureLevelVersion = new Version(10, 1);
					break;
				case D3D.FeatureLevel.Level_10_0:
					FeatureLevelVersion = new Version(10, 0);
					break;
				case D3D.FeatureLevel.Level_9_3:
					FeatureLevelVersion = new Version(9, 3);
					break;
				default:
					FeatureLevelVersion = new Version(0, 0);
					break;
			}

			Outputs = new GorgonVideoOutputCollection(this);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_device != null)
					{
						Gorgon.Log.Print("Removing D3D 11 device for video device '{0}'.", GorgonLoggingLevel.Verbose, Name); 
						_device.Dispose();
					}

					Outputs.ClearOutputs();

					Gorgon.Log.Print("Removing DXGI adapter interface...", Diagnostics.GorgonLoggingLevel.Verbose);
					GIAdapter.Dispose();
				}

				_device = null;
				GIAdapter = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		public string Name
		{
			get 
			{
				return GIAdapter.Description1.Description;
			}
		}
		#endregion
	}
}
