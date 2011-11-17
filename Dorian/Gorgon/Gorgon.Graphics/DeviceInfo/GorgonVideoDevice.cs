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
	/// Available feature levels for the video device.
	/// </summary>
	[Flags()]
	public enum DeviceFeatureLevel
	{
		/// <summary>
		/// Gorgon does not support any feature level for the video device.
		/// </summary>
		/// <remarks>This value is exclusive.</remarks>
		Unsupported = 0,
		/// <summary>
		/// Shader model 5.0.
		/// </summary>
		SM5 = 1,
		/// <summary>
		/// Shader model 4.0 with a 4.1 profile.
		/// </summary>
		/// <remarks>This the equivalent of a Direct 3D 10.1 video device.</remarks>
		SM4_1 = 2,
		/// <summary>
		/// Shader model 4.0
		/// </summary>
		/// <remarks>This the equivalent of a Direct 3D 10.0 video device.</remarks>
		SM4 = 4,
		/// <summary>
		/// Shader model 2.0, with a 2.0a vertex shader profile and a 2.0b pixel shader profile.
		/// </summary>
		/// <remarks>
		/// This the equivalent of a Direct 3D 9.0c video device.
		/// <para>Please note that this is for video cards that support a vertex shader model of 2.0a and a pixel shader model of 2.0b.  Nothing below that (i.e. vanilla SM 2.0) will work.  This is a limitation
		/// imposed by Gorgon to keep the code paths smaller.</para>
		/// <para>The actual restriction of shader model 2.0 is from the Direct 3D 11 API itself.  There is no way around this except to upgrade the hardware to shader model 4 hardware.</para>
		/// </remarks>
		SM2_a_b = 8
	}

	/// <summary>
	/// Contains information about a video device.
	/// </summary>
	public class GorgonVideoDevice
		: INamedObject, IDisposable
	{
		#region Variables.
		private bool _disposed = false;														// Flag to indicate that the object was disposed.
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
		/// <remarks>This is independant of the <see cref="P:GorgonLibrary.Graphics.GorgonGraphics.MaxFeatureLevel">GorgonGraphics.MaxFeatureLevel</see> property and will always return the true hardware feature level.</remarks>
		public DeviceFeatureLevel HardwareFeatureLevels
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the supported feature levels.
		/// </summary>
		/// <remarks>If the <see cref="P:GorgonLibrary.Graphics.GorgonGraphics.MaxFeatureLevel">GorgonGraphics.MaxFeatureLevel</see> property has been set, this will return the actual available feature levels.
		/// <para>Due to the restrictions that may be imposed by specifying a feature level, the return value may differ from the <see cref="P:GorgonLibrary.GorgonGraphics.GorgonVideoDevice.HardwareFeatureLevels">HardwareFeatureLevels</see> property.</para>
		/// </remarks>
		public DeviceFeatureLevel SupportedFeatureLevels
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
		private void EnumerateFeatureLevels(D3D.FeatureLevel featureLevel)
		{
			DeviceFeatureLevel featureLevels = DeviceFeatureLevel.Unsupported;

			switch (featureLevel)
			{
				case D3D.FeatureLevel.Level_11_0:
					featureLevels = DeviceFeatureLevel.SM5 | DeviceFeatureLevel.SM4_1  | DeviceFeatureLevel.SM4 | DeviceFeatureLevel.SM2_a_b;
					break;
				case D3D.FeatureLevel.Level_10_1:
					featureLevels = DeviceFeatureLevel.SM4_1 | DeviceFeatureLevel.SM4 | DeviceFeatureLevel.SM2_a_b;
					break;
				case D3D.FeatureLevel.Level_10_0:
					featureLevels = DeviceFeatureLevel.SM4 | DeviceFeatureLevel.SM2_a_b;
					break;
				case D3D.FeatureLevel.Level_9_3:
					featureLevels = DeviceFeatureLevel.SM2_a_b;
					break;
			}

			HardwareFeatureLevels = featureLevels;

			// Default to the hardware levels.
			SupportedFeatureLevels = HardwareFeatureLevels;
		}

		/// <summary>
		/// Function to convert a Gorgon feature level into a D3D feature level.
		/// </summary>
		/// <param name="featureLevel">Feature level to convert.</param>
		/// <returns>The D3D feature level.</returns>
		private D3D.FeatureLevel Convert(DeviceFeatureLevel featureLevel)
		{
			switch (featureLevel)
			{
				case DeviceFeatureLevel.SM5:
					return D3D.FeatureLevel.Level_11_0;
				case DeviceFeatureLevel.SM4_1:
					return D3D.FeatureLevel.Level_10_1;
				case DeviceFeatureLevel.SM4:
					return D3D.FeatureLevel.Level_10_0;
				default:
					return D3D.FeatureLevel.Level_9_3;
			}
		}

		/// <summary>
		/// Function to convert a Gorgon feature level into an array of applicable D3D feature levels.
		/// </summary>
		/// <param name="featureLevels">Gorgon feature level to convert.</param>
		/// <returns>An array of D3D feature levels.</returns>
		private D3D.FeatureLevel[] GetFeatureLevels(DeviceFeatureLevel featureLevels)
		{
			List<D3D.FeatureLevel> D3DfeatureLevels = new List<D3D.FeatureLevel>();
			DeviceFeatureLevel supported = DeviceFeatureLevel.Unsupported;

			foreach (var featureLevel in GorgonGraphics.GorgonFeatureLevels)
			{
				if (((featureLevels & featureLevel) == featureLevel) && (SupportsFeatureLevels(featureLevel)))
				{
					supported |= featureLevel;
					D3DfeatureLevels.Add(Convert(featureLevel));					
				}
			}

			if (D3DfeatureLevels.Count > 0)
			{				
				// Add 9.0 SM2 and SM1 just to be complete.
				D3DfeatureLevels.Add(D3D.FeatureLevel.Level_9_2);
				D3DfeatureLevels.Add(D3D.FeatureLevel.Level_9_1);
			}
			else
				throw new NotSupportedException("The video device '" + Name + "' is not supported by Gorgon.");

			SupportedFeatureLevels = supported;
			return D3DfeatureLevels.ToArray();
		}

		/// <summary>
		/// Function to retrieve the D3D 11 device object associated with this video device.
		/// </summary>
		/// <param name="maxFeatureLevel">Maximum feature level to support.</param>
		internal void CreateDevice(DeviceFeatureLevel maxFeatureLevel)
		{
			// Do not re-create the device.
			if (D3DDevice != null)
				return;

			D3D.DeviceCreationFlags flags = D3D.DeviceCreationFlags.None;

#if DEBUG
			flags = D3D.DeviceCreationFlags.Debug;
#endif
			Gorgon.Log.Print("Creating D3D 11 device for video device '{0}'...", GorgonLoggingLevel.Verbose, Name);
			D3DDevice = new D3D.Device(GIAdapter, flags, GetFeatureLevels(maxFeatureLevel));
			D3DDevice.DebugName = Name + " D3D11Device";
			D3DDevice.ImmediateContext.ClearState();
		}

		/// <summary>
		/// Function to release the Direct 3D device.
		/// </summary>
		internal void ReleaseDevice()
		{
			Gorgon.Log.Print("Removing D3D 11 device for video device '{0}'.", GorgonLoggingLevel.Verbose, Name);
			if (D3DDevice != null)
			{
				D3DDevice.ImmediateContext.ClearState();
				D3DDevice.Dispose();
			}

			D3DDevice = null;
		}

		/// <summary>
		/// Function to determine if the specified feature levels are supported by the hardware.
		/// </summary>
		/// <param name="featureLevels">Feature levels to test.</param>
		/// <returns>TRUE if supported, FALSE if not.</returns>
		/// <remarks>The <paramref name="featureLevels"/> parameter can take multiple feature levels by OR'ing together the requested feature levels.
		/// <para>Note that if one or more of the feature levels are supported, then this method will return TRUE.</para>
		/// </remarks>
		public bool SupportsFeatureLevels(DeviceFeatureLevel featureLevels)
		{
			if (featureLevels == DeviceFeatureLevel.Unsupported)
				return false;

			foreach (var featureLevel in GorgonGraphics.GorgonFeatureLevels)
			{
				if (((HardwareFeatureLevels & featureLevel) == featureLevel) && ((featureLevels & featureLevel) == featureLevel))
					return true;
			}

			return false;
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
			if (D3DDevice == null)
				return false;

			return ((D3DDevice.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.FormatDisplaySupport) == D3D.FormatSupport.FormatDisplaySupport);
		}

		/// <summary>
		/// Function to determine if the specified format is supported for a 2D texture.
		/// </summary>
		/// <param name="format">Format to check.</param>
		/// <returns>TRUE if the format is supported for displaying on the video device, FALSE if not.</returns>
		public bool Supports2DTextureFormat(GorgonBufferFormat format)
		{
			if (D3DDevice == null)
				return false;

			return ((D3DDevice.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.Texture2D) == D3D.FormatSupport.Texture2D);
		}

		/// <summary>
		/// Function to determine if the specified depth buffer format is supported.
		/// </summary>
		/// <param name="format">Format to check.</param>
		/// <returns>TRUE if the format is supported as a depth/stencil buffer, FALSE if not.</returns>
		public bool SupportsDepthFormat(GorgonBufferFormat format)
		{
			if (D3DDevice == null)
				return false;

			return ((D3DDevice.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.DepthStencil) == D3D.FormatSupport.DepthStencil);
		}

		/// <summary>
		/// Function to return the maximum number of quality levels supported by the device for multisampling.
		/// </summary>
		/// <param name="format">Format to test.</param>
		/// <param name="count">Number of multisamples.</param>
		/// <returns>The maximum quality level for the format, or 0 if not supported.</returns>
		public int GetMultiSampleQuality(GorgonBufferFormat format, int count)
		{
			if ((format == GorgonBufferFormat.Unknown) || (D3DDevice == null))
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
					ReleaseDevice();
					Outputs.ClearOutputs();

					Gorgon.Log.Print("Removing DXGI adapter interface...", Diagnostics.GorgonLoggingLevel.Verbose);
					GIAdapter.Dispose();
				}

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
