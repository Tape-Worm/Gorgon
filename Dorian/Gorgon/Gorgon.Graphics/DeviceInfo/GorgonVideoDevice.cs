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
using GI = SharpDX.DXGI;
using D3DCommon = SharpDX.Direct3D;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Available feature levels for the video device.
	/// </summary>
	public enum DeviceFeatureLevel
	{
		/// <summary>
		/// Gorgon does not support any feature level for the video device.
		/// </summary>
		/// <remarks>This value is exclusive.</remarks>
		Unsupported = 0,
		/// <summary>
		/// Shader model 2.0, with a 2.0a vertex shader profile and a 2.0b pixel shader profile.
		/// </summary>
		/// <remarks>
		/// This the equivalent of a Direct 3D 9.0c video device.
		/// <para>Please note that this is for video cards that support a vertex shader model of 2.0a and a pixel shader model of 2.0b.  Nothing below that (i.e. vanilla SM 2.0) will work.  This is a limitation
		/// imposed by Gorgon to keep the code paths smaller.</para>
		/// <para>The actual restriction of shader model 2.0 is from the Direct 3D 11 API itself.  There is no way around this except to upgrade the hardware to shader model 4 hardware.</para>
		/// </remarks>
		SM2_a_b = 1,
		/// <summary>
		/// Shader model 4.0
		/// </summary>
		/// <remarks>This the equivalent of a Direct 3D 10.0 video device.</remarks>
		SM4 = 2,
		/// <summary>
		/// Shader model 4.0 with a 4.1 profile.
		/// </summary>
		/// <remarks>This the equivalent of a Direct 3D 10.1 video device.</remarks>
		SM4_1 = 3,
		/// <summary>
		/// Shader model 5.0.
		/// </summary>
		SM5 = 4,
	}

	/// <summary>
	/// Video device types.
	/// </summary>
	public enum VideoDeviceType
	{
		/// <summary>
		/// Hardware video device.
		/// </summary>
		Hardware = 0,
		/// <summary>
		/// Software video device (WARP).
		/// </summary>
		Software = 1,
		/// <summary>
		/// Reference rasterizer video device.
		/// </summary>
		ReferenceRasterizer = 2
	}

	/// <summary>
	/// Contains information about a video device.
	/// </summary>
	public class GorgonVideoDevice
		: INamedObject, IDisposable
	{
		#region Variables.
		private bool _disposed = false;														// Flag to indicate that the object was disposed.
		private D3D.Device _tempDevice = null;												// Temporary device.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the DX GI factory interface.
		/// </summary>
		internal GI.Factory1 GIFactory
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the DX GI adapter interface for this video device.
		/// </summary>
		internal GI.Adapter1 GIAdapter
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the graphics interface bound with this device.
		/// </summary>
		internal GorgonGraphics Graphics
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the type of video device.
		/// </summary>
		public VideoDeviceType VideoDeviceType
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the highest feature level that the hardware can support.
		/// </summary>
		/// <remarks>This is independant of the <see cref="P:GorgonLibrary.Graphics.GorgonGraphics.RequestedFeatureLevel">GorgonGraphics.RequestedFeatureLevel</see> property and will always return the true hardware feature level.</remarks>
		public DeviceFeatureLevel HardwareFeatureLevel
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the supported feature levels.
		/// </summary>
		/// <remarks>
		/// This property will show either the requested feature level passed into the <see cref="GorgonLibrary.Graphics.GorgonGraphics.GorgonGraphics(GorgonVideoDevice, DeviceFeatureLevel)">GorgonGraphics constructor</see>, or the 
		/// <see cref="GorgonLibrary.Graphics.GorgonVideoDevice.HardwareFeatureLevel">GorgonVideoDevice.HardwareFeatureLevel</see> property, depending on which is higher.
		/// <para>Due to the restrictions that may be imposed by specifying a feature level, the return value may differ from the <see cref="P:GorgonLibrary.GorgonGraphics.GorgonVideoDevice.HardwareFeatureLevels">HardwareFeatureLevels</see> property.</para>
		/// </remarks>
		public DeviceFeatureLevel SupportedFeatureLevel
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
		/// Function to enumerate a D3D feature level to a Gorgon device feature level.
		/// </summary>
		/// <param name="featureLevel">D3D Feature level to enumerate.</param>
		/// <returns>Gorgon device feature level.</returns>
		private void EnumerateFeatureLevels(D3DCommon.FeatureLevel featureLevel)
		{
			switch (featureLevel)
			{
				case D3DCommon.FeatureLevel.Level_11_0:
					SupportedFeatureLevel = HardwareFeatureLevel = DeviceFeatureLevel.SM5;
					break;
				case D3DCommon.FeatureLevel.Level_10_1:
					SupportedFeatureLevel = HardwareFeatureLevel = DeviceFeatureLevel.SM4_1;
					break;
				case D3DCommon.FeatureLevel.Level_10_0:
					SupportedFeatureLevel = HardwareFeatureLevel = DeviceFeatureLevel.SM4;
					break;
				case D3DCommon.FeatureLevel.Level_9_3:
					SupportedFeatureLevel = HardwareFeatureLevel = DeviceFeatureLevel.SM2_a_b;
					break;
				default:
					SupportedFeatureLevel = HardwareFeatureLevel = DeviceFeatureLevel.Unsupported;
					break;
			}
		}

		/// <summary>
		/// Function to convert a Gorgon feature level into a D3D feature level.
		/// </summary>
		/// <param name="featureLevel">Feature level to convert.</param>
		/// <returns>The D3D feature level.</returns>
		private D3DCommon.FeatureLevel[] Convert(DeviceFeatureLevel featureLevel)
		{
			if (HardwareFeatureLevel < featureLevel)
				featureLevel = HardwareFeatureLevel;

			SupportedFeatureLevel = featureLevel;

			switch (featureLevel)
			{
				case DeviceFeatureLevel.SM5:
					return new[] {
							D3DCommon.FeatureLevel.Level_11_0,
							D3DCommon.FeatureLevel.Level_10_1,
							D3DCommon.FeatureLevel.Level_10_0,
							D3DCommon.FeatureLevel.Level_9_3,
							D3DCommon.FeatureLevel.Level_9_2,
							D3DCommon.FeatureLevel.Level_9_1
					};
				case DeviceFeatureLevel.SM4_1:
					return new[] {
							D3DCommon.FeatureLevel.Level_10_1,
							D3DCommon.FeatureLevel.Level_10_0,
							D3DCommon.FeatureLevel.Level_9_3,
							D3DCommon.FeatureLevel.Level_9_2,
							D3DCommon.FeatureLevel.Level_9_1
					};
				case DeviceFeatureLevel.SM4:
					return new[] {
							D3DCommon.FeatureLevel.Level_10_0,
							D3DCommon.FeatureLevel.Level_9_3,
							D3DCommon.FeatureLevel.Level_9_2,
							D3DCommon.FeatureLevel.Level_9_1
					};
				case DeviceFeatureLevel.SM2_a_b:
					return new[] {
							D3DCommon.FeatureLevel.Level_9_3,
							D3DCommon.FeatureLevel.Level_9_2,
							D3DCommon.FeatureLevel.Level_9_1
					};
				default:
					throw new GorgonException(GorgonResult.CannotCreate, "Cannot create device.  Device is not supported.");
			}			
		}

		/// <summary>
		/// Function to retrieve the Direct3D device object.
		/// </summary>
		private void GetDevice()
		{
			// If we've assigned a device externally, then return it.
			if ((Graphics != null) && (Graphics.D3DDevice != null))
			{
				_tempDevice = Graphics.D3DDevice;
				return;
			}

			_tempDevice = CreateD3DDeviceNoLogging(HardwareFeatureLevel);
		}

		/// <summary>
		/// Function to release the temporary device object.
		/// </summary>
		private void ReleaseTempDevice()
		{
			if ((_tempDevice != null) && (Graphics != null) && (_tempDevice != Graphics.D3DDevice))
				_tempDevice.Dispose();
			_tempDevice = null;
		}

		/// <summary>
		/// Function to create the Direct3D device object.
		/// </summary>
		/// <param name="maxFeatureLevel">Maximum feature level to support.</param>
		/// <returns>The Direct3D 11 device object.</returns>
		internal D3D.Device CreateD3DDeviceNoLogging(DeviceFeatureLevel maxFeatureLevel)
		{
			D3D.Device device = null;

			D3D.DeviceCreationFlags flags = D3D.DeviceCreationFlags.None;

#if DEBUG
			flags = D3D.DeviceCreationFlags.Debug;
#endif

			device = new D3D.Device(GIAdapter, flags, Convert(maxFeatureLevel));
			device.DebugName = Name + " D3D11Device";
			device.ImmediateContext.ClearState();

			return device;
		}

		/// <summary>
		/// Function to retrieve the D3D 11 device object associated with this video device.
		/// </summary>
		/// <param name="maxFeatureLevel">Maximum feature level to support.</param>
		/// <returns>The Direct3D 11 device object.</returns>
		internal D3D.Device CreateD3DDevice(DeviceFeatureLevel maxFeatureLevel)
		{
			Gorgon.Log.Print("Creating D3D 11 device for video device '{0}'...", LoggingLevel.Verbose, Name);
			return CreateD3DDeviceNoLogging(maxFeatureLevel);
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
		public bool SupportsDisplayFormat(BufferFormat format)
		{
			try
			{
				GetDevice();
				return ((_tempDevice.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.Display) == D3D.FormatSupport.Display);
			}
			finally
			{
				ReleaseTempDevice();
			}
		}

		/// <summary>
		/// Function to determine if the specified format is supported for a 2D texture.
		/// </summary>
		/// <param name="format">Format to check.</param>
		/// <returns>TRUE if the format is supported for displaying on the video device, FALSE if not.</returns>
		public bool Supports2DTextureFormat(BufferFormat format)
		{
			try
			{
				GetDevice();
				return ((_tempDevice.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.Texture2D) == D3D.FormatSupport.Texture2D);
			}
			finally
			{
				ReleaseTempDevice();
			}
		}

		/// <summary>
		/// Function to determine if the specified depth buffer format is supported.
		/// </summary>
		/// <param name="format">Format to check.</param>
		/// <returns>TRUE if the format is supported as a depth/stencil buffer, FALSE if not.</returns>
		public bool SupportsDepthFormat(BufferFormat format)
		{
			try
			{
				GetDevice();
				return ((_tempDevice.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.DepthStencil) == D3D.FormatSupport.DepthStencil);
			}
			finally
			{
				ReleaseTempDevice();
			}
		}

		/// <summary>
		/// Function to return the maximum number of quality levels supported by the device for multisampling.
		/// </summary>
		/// <param name="format">Format to test.</param>
		/// <param name="count">Number of multisamples.</param>
		/// <returns>The maximum quality level for the format, or 0 if not supported.</returns>
		public int GetMultiSampleQuality(BufferFormat format, int count)
		{
			if (format == BufferFormat.Unknown)
				return 0;

			if (count < 1)
				count = 1;

			try
			{
				GetDevice();
				return _tempDevice.CheckMultisampleQualityLevels((GI.Format)format, count);
			}
			finally
			{
				ReleaseTempDevice();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoDevice"/> class.
		/// </summary>
		/// <param name="adapter">DXGI video adapter.</param>
		/// <param name="deviceType">Type of video device.</param>
		internal GorgonVideoDevice(GI.Adapter1 adapter, VideoDeviceType deviceType)
		{
			VideoDeviceType = deviceType;
			GIAdapter = adapter;
			GIFactory = adapter.GetParent<GI.Factory1>();
			EnumerateFeatureLevels(D3D.Device.GetSupportedFeatureLevel(adapter));
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
					// If we've assigned a device object externally, then do not destroy this object.
					if (Graphics != null)
						return;

					if (GIFactory != null)
					{
						GIFactory.Dispose();
						GIFactory = null;
					}

					if (_tempDevice != null)
						_tempDevice.Dispose();

					Outputs.ClearOutputs();

					Gorgon.Log.Print("Removing DXGI adapter interface...", Diagnostics.LoggingLevel.Verbose);
					if (GIAdapter != null)
					{
						GIAdapter.Dispose();
						GIAdapter = null;
					}
				}

				_tempDevice = null;
				Graphics = null;
				GIFactory = null;
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
			if (Graphics == null)
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
				switch(VideoDeviceType)
				{
					case VideoDeviceType.ReferenceRasterizer:
						return "Reference rasterizer";
					case VideoDeviceType.Software:
						return "WARP software rasterizer";
					default:
						return GIAdapter.Description1.Description;
				}
			}
		}
		#endregion
	}
}
