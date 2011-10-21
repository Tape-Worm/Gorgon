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
using GI = SlimDX.DXGI;
using D3D = SlimDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Available D3D feature levels for the video device.
	/// </summary>
	[Flags()]
	public enum DeviceFeatureLevel
	{
		/// <summary>
		/// Gorgon does not support this direct 3D level.
		/// </summary>
		Unsupported = 0,
		/// <summary>
		/// Direct 3D 11, shader model 5.0.
		/// </summary>
		Level11_0_SM5 = 1,
		/// <summary>
		/// Direct 3D 10.1, shader model 4.0.
		/// </summary>
		Level10_1_SM4 = 2,
		/// <summary>
		/// Direct 3D 10, shader model 4.0.
		/// </summary>
		Level10_0_SM4 = 4,
		/// <summary>
		/// Direct 3D 9, shader model 3.0.
		/// </summary>
		Level9_0_SM3 = 8
	}

	/// <summary>
	/// Contains information about a video device.
	/// </summary>
	public class GorgonVideoDevice
		: INamedObject, IDisposable
	{
		#region Variables.
		private bool _disposed = false;														// Flag to indicate that the object was disposed.		
		private DeviceFeatureLevel _currentFeatureLevel = DeviceFeatureLevel.Unsupported;	// Currently active feature level.
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
		/// Property to return the D3D device interface for this video device.
		/// </summary>
		internal D3D.Device D3DDevice
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the highest feature level that the hardware can support.
		/// </summary>
		public DeviceFeatureLevel HardwareFeatureLevels
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
		/// Function to convert a D3D feature level to a Gorgon device feature level.
		/// </summary>
		/// <param name="featureLevel">D3D Feature level to convert.</param>
		/// <returns>Gorgon device feature level.</returns>
		private void Convert(D3D.FeatureLevel featureLevel)
		{			
			switch (featureLevel)
			{
				case D3D.FeatureLevel.Level_11_0:
					HardwareFeatureLevels = DeviceFeatureLevel.Level11_0_SM5 | DeviceFeatureLevel.Level10_1_SM4  | DeviceFeatureLevel.Level10_0_SM4 | DeviceFeatureLevel.Level9_0_SM3;
					break;
				case D3D.FeatureLevel.Level_10_1:
					HardwareFeatureLevels = DeviceFeatureLevel.Level10_1_SM4 | DeviceFeatureLevel.Level10_0_SM4 | DeviceFeatureLevel.Level9_0_SM3;
					break;
				case D3D.FeatureLevel.Level_10_0:
					HardwareFeatureLevels = DeviceFeatureLevel.Level10_0_SM4 | DeviceFeatureLevel.Level9_0_SM3;
					break;
				case D3D.FeatureLevel.Level_9_3:
					HardwareFeatureLevels = DeviceFeatureLevel.Level9_0_SM3;
					break;
				default:
					HardwareFeatureLevels = DeviceFeatureLevel.Unsupported;
					break;
			}
		}

		/// <summary>
		/// Function to convert a Gorgon feature level into an array of applicable D3D feature levels.
		/// </summary>
		/// <param name="featureLevel">Gorgon feature level to convert.</param>
		/// <returns>An array of D3D feature levels.</returns>
		private D3D.FeatureLevel[] Convert(DeviceFeatureLevel featureLevel)
		{
			List<D3D.FeatureLevel> featureLevels = new List<D3D.FeatureLevel>();

			if ((featureLevel & DeviceFeatureLevel.Level11_0_SM5) == DeviceFeatureLevel.Level11_0_SM5)
				featureLevels.Add(D3D.FeatureLevel.Level_11_0);

			if ((featureLevel & DeviceFeatureLevel.Level10_1_SM4) == DeviceFeatureLevel.Level10_1_SM4)
				featureLevels.Add(D3D.FeatureLevel.Level_10_1);

			if ((featureLevel & DeviceFeatureLevel.Level10_0_SM4) == DeviceFeatureLevel.Level10_0_SM4)
				featureLevels.Add(D3D.FeatureLevel.Level_10_0);

			if ((featureLevel & DeviceFeatureLevel.Level9_0_SM3) == DeviceFeatureLevel.Level9_0_SM3)
				featureLevels.Add(D3D.FeatureLevel.Level_9_3);

			if (featureLevels.Count > 0)
			{
				featureLevels.Add(D3D.FeatureLevel.Level_9_2);
				featureLevels.Add(D3D.FeatureLevel.Level_9_1);
			}
			else
				throw new NotSupportedException("The video device '" + Name + "' is not supported by Gorgon.");

			return featureLevels.ToArray();
		}

		/// <summary>
		/// Function to retrieve the D3D 11 device object associated with this video device.
		/// </summary>
		internal void GetDevice()
		{
			D3D.DeviceCreationFlags flags = D3D.DeviceCreationFlags.None;

#if DEBUG
			flags = D3D.DeviceCreationFlags.Debug;
#endif
			Gorgon.Log.Print("Creating D3D 11 device for video device '{0}'...", GorgonLoggingLevel.Verbose, Name);
			D3DDevice = new D3D.Device(GIAdapter, flags, Convert(_currentFeatureLevel));
			D3DDevice.DebugName = Name + " D3D11Device";

			Outputs = new GorgonVideoOutputCollection(this);
		}

		/// <summary>
		/// Function to reset the device.
		/// </summary>
		internal void Reset()
		{
			if (D3DDevice != null)
			{
				Gorgon.Log.Print("Removing D3D 11 device for video device '{0}'.", GorgonLoggingLevel.Verbose, Name);
				D3DDevice.Dispose();
				D3DDevice = null;
			}

			GetDevice();
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

		/// <summary>
		/// Function to determine if the specified format is supported for display.
		/// </summary>
		/// <param name="format">Format to check.</param>
		/// <returns>TRUE if the format is supported for displaying on the video device, FALSE if not.</returns>
		public bool SupportsDisplayFormat(GorgonBufferFormat format)
		{
			return ((D3DDevice.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.FormatDisplaySupport) == D3D.FormatSupport.FormatDisplaySupport);
		}

		/// <summary>
		/// Function to return the maximum number of quality levels supported by the device for multisampling.
		/// </summary>
		/// <param name="format">Format to test.</param>
		/// <param name="count">Number of multisamples.</param>
		/// <returns>The maximum quality level for the format, or 0 if not supported.</returns>
		public int GetMultiSampleQuality(GorgonBufferFormat format, int count)
		{
			if (format == GorgonBufferFormat.Unknown)
				return 0;

			if (count < 1)
				count = 1;

			return D3DDevice.CheckMultisampleQualityLevels((GI.Format)format, count);
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
			Convert(D3D.Device.GetSupportedFeatureLevel(adapter));						
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
					if (D3DDevice != null)
					{
						Gorgon.Log.Print("Removing D3D 11 device for video device '{0}'.", GorgonLoggingLevel.Verbose, Name);
						D3DDevice.Dispose();
					}

					Outputs.ClearOutputs();

					Gorgon.Log.Print("Removing DXGI adapter interface...", Diagnostics.GorgonLoggingLevel.Verbose);
					GIAdapter.Dispose();
				}

				D3DDevice = null;
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
