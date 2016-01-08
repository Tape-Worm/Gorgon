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
using Gorgon.Math;

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
	public class GorgonVideoDevice
		: IGorgonVideoDevice, ISdxVideoDevice
	{
		#region Variables.
		// The SharpDX Direct3D device object.
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		private D3D12.Device _d3dDevice;
		// The SharpDX DXGI adapter for the video device.
		private DXGI.Adapter3 _adapter;
		// The log used for debug output.
		private IGorgonLog _log;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return information about this video device.
		/// </summary>
		public IGorgonVideoDeviceInfo Info
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
		/// (indicated by the <see cref="IGorgonVideoDeviceInfo.SupportedFeatureLevel"/> property in the <see cref="IGorgonVideoDevice.Info"/> property) will be returned.
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
		/// Function to retrieve the underlying Direct3D 12 device object.
		/// </summary>
		/// <returns>The underlying Direct3D 12 device object.</returns>
		internal D3D12.Device GetD3DDevice() => _d3dDevice;

		/// <summary>
		/// Function to retrieve the underlying Direct3D 11 device object.
		/// </summary>
		/// <returns>The underlying Direct3D 11 device object.</returns>
		D3D12.Device ISdxVideoDevice.GetD3DDevice() => _d3dDevice;

		/// <summary>
		/// Function to retrieve the underlying DXGI adapter object.
		/// </summary>
		/// <returns>The underlying DXGI adapter object.</returns>
		internal DXGI.Adapter2 GetDXGIAdapter() => _adapter;

		/// <summary>
		/// Function to retrieve the underlying DXGI adapter object.
		/// </summary>
		/// <returns>The underlying DXGI adapter object.</returns>
		DXGI.Adapter3 ISdxVideoDevice.GetDXGIAdapter() => _adapter;

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
		/// Function to determine if a device supports using rendering commands from multiple threads.
		/// </summary>
		/// <returns><b>true</b> if support is available, <b>false</b> if not.</returns>
		public bool SupportsMultiThreadedCommands()
		{
/*			if (RequestedFeatureLevel < DeviceFeatureLevel.FeatureLevel11_0)
			{
				return false;
			}

			bool result;
			bool dummy;

			_d3dDevice.Value.CheckThreadingSupport(out dummy, out result);*/

			return false;
		}

		/// <summary>
		/// Function to determine if a device supports creating resources from multiple threads.
		/// </summary>
		/// <returns><b>true</b> if support is available, <b>false</b> if not.</returns>
		public bool SupportsMultiThreadedCreation()
		{
/*
			if (RequestedFeatureLevel < DeviceFeatureLevel.FeatureLevel11_0)
			{
				return false;
			}

			bool result;
			bool dummy;

			_d3dDevice.Value.CheckThreadingSupport(out result, out dummy);

			return result;
			*/
			return false;
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

		/// <summary>
		/// Function to find the full screen display mode the 
		/// </summary>
		/// <param name="output">The output to use when looking for a video mode.</param>
		/// <param name="videoMode">The <see cref="GorgonVideoMode"/> used to find the closest match.</param>
		/// <param name="newMode">A <see cref="GorgonVideoMode"/> that is the nearest match for the provided video mode.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="output"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>
		/// <para>
		/// Users may leave the <see cref="GorgonVideoMode"/> values at unspecified (either 0, or default enumeration values) to indicate that these values should not be used in the search.
		/// </para>
		/// <para>
		/// The following members in <see cref="GorgonVideoMode"/> may be skipped (if not listed, then this member must be specified):
		/// <list type="bullet">
		///		<item>
		///			<description><see cref="GorgonVideoMode.Width"/> and <see cref="GorgonVideoMode.Height"/>.  Both values must be set to 0 if not filtering by width or height.</description>
		///		</item>
		///		<item>
		///			<description><see cref="GorgonVideoMode.RefreshRate"/> should be set to <see cref="GorgonRationalNumber.Empty"/> in order to skip filtering by refresh rate.</description>
		///		</item>
		///		<item>
		///			<description><see cref="GorgonVideoMode.Scaling"/> should be set to <see cref="VideoModeDisplayModeScaling.Unspecified"/> in order to skip filtering by the scaling mode.</description>
		///		</item>
		///		<item>
		///			<description><see cref="GorgonVideoMode.ScanlineOrdering"/> should be set to <see cref="VideoModeScanlineOrder.Unspecified"/> in order to skip filtering by the scanline order.</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// The <see cref="GorgonVideoMode.Format"/> member must be one of the UNorm format types and cannot be set to <see cref="BufferFormat.Unknown"/>.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void FindClosestMode(IGorgonVideoOutputInfo output, ref GorgonVideoMode videoMode, out GorgonVideoMode newMode)
		{
			if (output == null)
			{
				throw new ArgumentNullException(nameof(output));
			}

			using (DXGI.Output output1 = _adapter.GetOutput(output.Index))
			{
				using (DXGI.Output4 output4 = output1.QueryInterface<DXGI.Output4>())
				{
					DXGI.ModeDescription1 newModeDesc;
					DXGI.ModeDescription1 oldModeDesc = videoMode.ToModeDesc();

					output4.FindClosestMatchingMode1(ref oldModeDesc, out newModeDesc, _d3dDevice);

					newMode = new GorgonVideoMode(newModeDesc);
				}
			}
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoDevice" /> class.
		/// </summary>
		/// <param name="deviceInfo">A <see cref="IGorgonVideoDeviceInfo"/> containing information about which video device to use.</param>
		/// <param name="requestedFeatureLevel">[Optional] A <see cref="DeviceFeatureLevel"/> representing the highest feature level to use for this video device.</param>
		/// <param name="log">[Optional] A <see cref="IGorgonLog"/> used for logging debug output.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="deviceInfo"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="deviceInfo"/> parameter has a <see cref="IGorgonVideoDeviceInfo.SupportedFeatureLevel"/> that is not supported.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="requestedFeatureLevel"/> is unsupported.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// If the <paramref name="requestedFeatureLevel"/> is set to <b>null</b> (<i>Nothing</i> in VB.Net), then the highest feature level supported by the device will be used.
		/// </para>
		/// <para>
		/// When the <paramref name="requestedFeatureLevel"/> is higher than that is supported by the video device, then the requested feature level will be set to the value specified in the 
		/// <see cref="IGorgonVideoDeviceInfo.SupportedFeatureLevel"/> of the <paramref name="deviceInfo"/> parameter.
		/// </para>
		/// </remarks>
		public GorgonVideoDevice(IGorgonVideoDeviceInfo deviceInfo, DeviceFeatureLevel? requestedFeatureLevel = null, IGorgonLog log = null)
		{
			if (deviceInfo == null)
			{
				throw new ArgumentNullException(nameof(deviceInfo));
			}

			if (deviceInfo.SupportedFeatureLevel == DeviceFeatureLevel.Unsupported)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_VIDEO_DEVICE_NOT_SUPPORTED, deviceInfo.Name), nameof(deviceInfo));
			}

			if (requestedFeatureLevel == null)
			{
				requestedFeatureLevel = deviceInfo.SupportedFeatureLevel;
			}

			if ((requestedFeatureLevel == DeviceFeatureLevel.Unsupported) || (!Enum.IsDefined(typeof(DeviceFeatureLevel), requestedFeatureLevel)))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FEATURE_LEVEL_INVALID, requestedFeatureLevel), nameof(requestedFeatureLevel));
			}

			if (deviceInfo.SupportedFeatureLevel < requestedFeatureLevel)
			{
				requestedFeatureLevel = deviceInfo.SupportedFeatureLevel;
			}

			RequestedFeatureLevel = requestedFeatureLevel.Value;
			Info = deviceInfo;
			_log = log ?? GorgonLogDummy.DefaultInstance;

			CreateD3DDevice();
		}
		#endregion
	}
}
