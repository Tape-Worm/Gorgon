#region MIT
// 
// Gorgon.
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
// Created: Tuesday, December 15, 2015 7:24:16 PM
// 
#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using D3D = SharpDX.Direct3D;
using DXGI = SharpDX.DXGI;
using D3D12 = SharpDX.Direct3D12;
using Gorgon.Graphics.Properties;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Functionality to provide access to a video device.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A video device can be a physical video card installed within the system, or an integrated video sub system. In some cases, as with the WARP video device, the video device may be a software construct.
	/// </para>
	/// <para>
	/// This object makes the video device available for use for rendering graphics, compute operations, and provides functionality to query information about support for particular features supplied by the 
	/// video device.
	/// </para>
	/// </remarks>
	class VideoDevice
		: IGorgonVideoDevice
	{
		#region Variables.
		// The SharpDX Direct3D device object.
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		private D3D12.Device _d3dDevice;
		// The SharpDX DXGI adapter for the video device.
		private DXGI.Adapter3 _adapter;
		// The log used for debug output.
		private readonly IGorgonLog _log;
		#endregion

		#region Properties.
		/// <summary>
		/// Function to retrieve the underlying Direct3D 12 device object.
		/// </summary>
		/// <returns>The underlying Direct3D 12 device object.</returns>
		internal D3D12.Device D3DDevice => _d3dDevice;

		/// <summary>
		/// Function to retrieve the underlying DXGI adapter object.
		/// </summary>
		/// <returns>The underlying DXGI adapter object.</returns>
		internal DXGI.Adapter3 DXGIAdapter => _adapter;

		/// <summary>
		/// Property to return information about this video device.
		/// </summary>
		public GorgonVideoDeviceInfo Info
		{
			get;
		}

		/// <summary>
		/// Property to return the feature level requested by the user.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A user may request a lower feature level than what is supported by the device to allow the application to run on older video devices that lack support for newer functionality. This requested feature 
		/// level will be returned by this property.
		/// </para>
		/// <para>
		/// If the user does not request a feature level, or has specified one higher than what the video device supports, then the highest feature level supported by the video device 
		/// (indicated by the <see cref="GorgonVideoDeviceInfo.SupportedFeatureLevel"/> property in the <see cref="IGorgonVideoDevice.Info"/> property) will be returned.
		/// </para>
		/// </remarks>
		public DeviceFeatureLevel RequestedFeatureLevel
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create the Direct 3D device object.
		/// </summary>
		private void CreateD3DDevice()
		{
			DXGI.Factory4 factory4 = null;
			DXGI.Adapter adapter = null;

			try
			{
				factory4 = new DXGI.Factory4();

				// Get the adapter for this device.
				adapter = Info.VideoDeviceType == VideoDeviceType.Software ? factory4.GetWarpAdapter() : factory4.GetAdapter(Info.Index);
				_adapter = adapter.QueryInterface<DXGI.Adapter3>();

				D3D.FeatureLevel desiredFeatureLevel = (D3D.FeatureLevel)RequestedFeatureLevel;

				if ((Info.SupportedFeatureLevel < RequestedFeatureLevel)
					|| (!Enum.IsDefined(typeof(DeviceFeatureLevel), RequestedFeatureLevel)))
				{
					_adapter?.Dispose();
					_adapter = null;
					throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FEATURE_LEVEL_NOT_SUPPORTED, RequestedFeatureLevel, Info.Name));
				}

				// Create the Direct3D device object.
				_d3dDevice = new D3D12.Device(_adapter, desiredFeatureLevel)
				             {
					             Name = $"Gorgon Direct3D 12 device for: {Info.Name}"
				             };

				// Double check the feature level support for this device to ensure that we can actually use what it has.
				unsafe
				{
					D3D.FeatureLevel* featureLevels = stackalloc D3D.FeatureLevel[4];

					featureLevels[0] = D3D.FeatureLevel.Level_12_1;
					featureLevels[1] = D3D.FeatureLevel.Level_12_0;
					featureLevels[2] = D3D.FeatureLevel.Level_11_1;
					featureLevels[3] = D3D.FeatureLevel.Level_11_0;

					var supportedFeatureLevels = new D3D12.FeatureDataFeatureLevels
					                             {
						                             FeatureLevelCount = 4,
						                             FeatureLevelsRequestedPointer = new IntPtr(featureLevels)
					                             };

					if ((!_d3dDevice.CheckFeatureSupport(D3D12.Feature.FeatureLevels, ref supportedFeatureLevels))
						|| (supportedFeatureLevels.MaxSupportedFeatureLevel < desiredFeatureLevel)
						|| (!Enum.IsDefined(typeof(D3D.FeatureLevel), supportedFeatureLevels.MaxSupportedFeatureLevel)))
					{
						_adapter?.Dispose();
						_adapter = null;
						throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FEATURE_LEVEL_NOT_SUPPORTED, RequestedFeatureLevel, Info.Name));
					}
				}

				_log.Print($"Direct 3D 12 device created for video device '{Info.Name}' at feature level [{RequestedFeatureLevel}]", LoggingLevel.Simple);
			}
			finally
			{
				adapter?.Dispose();
				factory4?.Dispose();
			}
		}

		/// <summary>
		/// Function to retrieve the supported functionality for a given format.
		/// </summary>
		/// <param name="format">The format to evaluate.</param>
		/// <returns>A <see cref="BufferFormatSupport"/> containing OR'd values representing the functionality supported by the format.</returns>
		/// <remarks>
		/// Use this method to determine if a format can be used with a specific resource type (e.g. a 2D texture, vertex buffer, etc...). The value returned will of the <see cref="BufferFormatSupport"/> 
		/// value enumeration type and will contain the supported functionality represented as OR'd values.
		/// </remarks>
		public BufferFormatSupport GetBufferFormatSupport(BufferFormat format)
		{
			var support = new D3D12.FeatureDataFormatSupport
			              {
				              Format = (DXGI.Format)format
			              };

			if (!_d3dDevice.CheckFeatureSupport(D3D12.Feature.FormatSupport, ref support))
			{
				return BufferFormatSupport.None;
			}

			return (BufferFormatSupport)support.Support1;
		}

		/// <summary>
		/// Function to retrieve the supported unordered access view functionality for a given format.
		/// </summary>
		/// <param name="format">The format to evaluate.</param>
		/// <returns>A <see cref="BufferFormatUavSupport"/> containing OR'd values representing the functionality supported by the format.</returns>
		/// <remarks>
		/// Use this method to determine if a format can be used with specific unordered access view operations in a shader. The value returned will of the <see cref="BufferFormatUavSupport"/> value 
		/// enumeration type and will contain the supported functionality represented as OR'd values.
		/// </remarks>
		public BufferFormatUavSupport GetBufferFormatUavSupport(BufferFormat format)
		{
			var support = new D3D12.FeatureDataFormatSupport
			{
				Format = (DXGI.Format)format
			};			

			if (!_d3dDevice.CheckFeatureSupport(D3D12.Feature.FormatSupport, ref support))
			{
				return BufferFormatUavSupport.None;
			}

			return (BufferFormatUavSupport)support.Support2;
		}

		/// <summary>
		/// Function to return the maximum number of quality levels supported by the device for multi sampling.
		/// </summary>
		/// <param name="format">A <see cref="BufferFormat"/> to evaluate.</param>
		/// <param name="count">Number of multi samples.</param>
		/// <param name="forTiledResources">[Optional] <b>true</b> to check for tiled resource multiple sample quality, <b>false</b> to exclude tiled resources.</param>
		/// <returns>The maximum quality level for the format, or 0 if not supported.</returns>
		public GorgonMultiSampleInfo GetMultiSampleQuality(BufferFormat format, int count, bool forTiledResources = false)
		{
			var multiSampleLevels = new D3D12.FeatureDataMultisampleQualityLevels
			                        {
				                        Format = (DXGI.Format)format,
				                        Flags = forTiledResources ? D3D12.MultisampleQualityLevelFlags.SFlagsTiledResource : D3D12.MultisampleQualityLevelFlags.None,
				                        SampleCount = count
			                        };

			return !_d3dDevice.CheckFeatureSupport(D3D12.Feature.MultisampleQualityLevels, ref multiSampleLevels)
				       ? GorgonMultiSampleInfo.NoMultiSampling
				       : new GorgonMultiSampleInfo(count, multiSampleLevels.QualityLevelCount);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			D3D12.Device device = Interlocked.Exchange(ref _d3dDevice, null);
			DXGI.Adapter3 adapter = Interlocked.Exchange(ref _adapter, null);

			device?.Dispose();
			adapter?.Dispose();
		}

		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="VideoDevice" /> class.
		/// </summary>
		/// <param name="deviceInfo">A <see cref="GorgonVideoDeviceInfo"/> containing information about which video device to use.</param>
		/// <param name="requestedFeatureLevel">The desired feature level for the device.</param>
		/// <param name="log">[Optional] A <see cref="IGorgonLog"/> used for logging debug output.</param>
		public VideoDevice(GorgonVideoDeviceInfo deviceInfo, DeviceFeatureLevel requestedFeatureLevel, IGorgonLog log = null)
		{
			if ((requestedFeatureLevel == DeviceFeatureLevel.Unsupported) || (!Enum.IsDefined(typeof(DeviceFeatureLevel), requestedFeatureLevel)))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FEATURE_LEVEL_INVALID, requestedFeatureLevel), nameof(requestedFeatureLevel));
			}

			RequestedFeatureLevel = requestedFeatureLevel;
			Info = deviceInfo;
			_log = log ?? GorgonLogDummy.DefaultInstance;

			CreateD3DDevice();
		}
		#endregion
	}
}
