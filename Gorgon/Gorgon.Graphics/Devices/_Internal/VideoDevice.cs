#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Saturday, February 23, 2013 4:33:38 PM
// 
#endregion

using System;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.Math;
using SharpDX;
using DXGI = SharpDX.DXGI;
using D3DCommon = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A wrapper for a Direct 3D 11 device object and adapter.
	/// </summary>
	public class VideoDevice
		: IGorgonVideoDevice
	{
		#region Variables.
		// The Direct 3D 11 device object.
		private D3D11.Device _device;
		// The DXGI adapter.
		private DXGI.Adapter1 _adapter;
		// The logging interface for debug logging.
		private readonly IGorgonLog _log;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct 3D 11 device object associated with the video device.
		/// </summary>
		internal D3D11.Device Device => _device;

		/// <summary>
		/// Property to return the DXGI Adapter object associated with the video device.
		/// </summary>
		internal DXGI.Adapter1 Adapter => _adapter;

		/// <summary>
		/// Property to return the <see cref="GorgonVideoDeviceInfo"/> used to create this device.
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
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert a Gorgon feature level into a D3D feature level.
		/// </summary>
		/// <returns>The D3D feature level.</returns>
		private D3DCommon.FeatureLevel[] GetFeatureLevel()
		{
			switch (RequestedFeatureLevel)
			{
				case DeviceFeatureLevel.Sm5:
					return new[] {
							D3DCommon.FeatureLevel.Level_11_0,
							D3DCommon.FeatureLevel.Level_10_1,
							D3DCommon.FeatureLevel.Level_10_0
					};
				case DeviceFeatureLevel.Sm41:
					return new[] {
							D3DCommon.FeatureLevel.Level_10_1,
							D3DCommon.FeatureLevel.Level_10_0
					};
				case DeviceFeatureLevel.Sm4:
					return new[] {
							D3DCommon.FeatureLevel.Level_10_0
					};
				default:
					throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FEATURE_LEVEL_INVALID, RequestedFeatureLevel));
			}
		}

		/// <summary>
		/// Function to create the Direct 3D device and Adapter for use with Gorgon.
		/// </summary>
		private void CreateDevice()
		{
			DXGI.Factory1 factory1 = null;
			D3D11.DeviceCreationFlags flags = GorgonGraphics.IsDebugEnabled ? D3D11.DeviceCreationFlags.Debug : D3D11.DeviceCreationFlags.None;

			try
			{
				if (Info.SupportedFeatureLevel < RequestedFeatureLevel)
				{
					throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FEATURE_LEVEL_NOT_SUPPORTED, RequestedFeatureLevel, Info.Name));
				}

				factory1 = new DXGI.Factory1();

				switch (Info.VideoDeviceType)
				{
#if DEBUG
					case VideoDeviceType.ReferenceRasterizer:
						_device = new D3D11.Device(D3DCommon.DriverType.Reference, D3D11.DeviceCreationFlags.Debug, GetFeatureLevel())
						          {
							          DebugName = $"{Info.Name} D3D11 Reference Device"
						          };

						using (var giDevice = _device.QueryInterface<DXGI.Device1>())
						{
							_adapter = giDevice.GetParent<DXGI.Adapter1>();
						}
						break;
#else
					case VideoDeviceType.ReferenceRasterizer:
#endif
					case VideoDeviceType.Software:
						// WARP devices can only do SM4_1 or lower.
						if (RequestedFeatureLevel >= DeviceFeatureLevel.Sm5)
						{
							RequestedFeatureLevel = DeviceFeatureLevel.Sm41;
						}

						_device = new D3D11.Device(D3DCommon.DriverType.Warp, flags, GetFeatureLevel())
						          {
							          DebugName = $"{Info.Name} D3D11 Software Device"
						          };

						using (var giDevice = _device.QueryInterface<DXGI.Device1>())
						{
							_adapter = giDevice.GetParent<DXGI.Adapter1>();
						}
						break;
					default:
						_adapter = factory1.GetAdapter1(Info.Index);
						_device = new D3D11.Device(_adapter, flags, GetFeatureLevel())
						          {
							          DebugName = $"{Info.Name} D3D 11 Device"
						          };
						break;
				}

				if (!Enum.IsDefined(typeof(DeviceFeatureLevel), (DeviceFeatureLevel)_device.FeatureLevel))
				{
					throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FEATURE_LEVEL_NOT_SUPPORTED, RequestedFeatureLevel, Info.Name));
				}

				_log.Print($"Direct 3D 12 device created for video device '{Info.Name}' at feature level [{RequestedFeatureLevel}]", LoggingLevel.Simple);
			}
			finally
			{
				factory1?.Dispose();
			}
		}

		/// <summary>
		/// Returns a <see cref="string"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="string"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GORGFX_TOSTR_DEVICE, Info.Name);
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
			return (BufferFormatSupport)_device.CheckFormatSupport((DXGI.Format)format);
		}

		/// <summary>
		/// Function to retrieve the supported unordered access compute resource functionality for a given format.
		/// </summary>
		/// <param name="format">The format to evaluate.</param>
		/// <returns>A <see cref="BufferFormatComputeSupport"/> containing OR'd values representing the functionality supported by the format.</returns>
		/// <remarks>
		/// <para>
		/// Use this method to determine if a format can be used with specific unordered access view operations in a compute shader. The value returned will of the <see cref="BufferFormatComputeSupport"/> value 
		/// enumeration type and will contain the supported functionality represented as OR'd values.
		/// </para>
		/// <para>
		/// This will always return <see cref="BufferFormatComputeSupport.None"/> on devices without <see cref="DeviceFeatureLevel.Sm5"/> feature level support.
		/// </para>
		/// </remarks>
		public BufferFormatComputeSupport GetBufferFormatComputeSupport(BufferFormat format)
		{
			if (RequestedFeatureLevel < DeviceFeatureLevel.Sm5)
			{
				return BufferFormatComputeSupport.None;
			}

			return (BufferFormatComputeSupport)_device.CheckComputeShaderFormatSupport((DXGI.Format)format);
		}

		/// <summary>
		/// Function to determine if a device supports using rendering commands from multiple threads.
		/// </summary>
		/// <returns><b>true</b> if support is available, <b>false</b> if not.</returns>
		/// <remarks>
		/// This will always return <b>false</b> on devices without <see cref="DeviceFeatureLevel.Sm5"/> feature level support.
		/// </remarks>
		public bool SupportsMultiThreadedCommands()
		{
			if (RequestedFeatureLevel < DeviceFeatureLevel.Sm5)
			{
				return false;
			}

			bool result;
			bool dummy;

			return _device.CheckThreadingSupport(out dummy, out result) == Result.Ok && result;
		}

		/// <summary>
		/// Function to determine if a device supports creating resources from multiple threads.
		/// </summary>
		/// <returns><b>true</b> if support is available, <b>false</b> if not.</returns>
		/// <remarks>
		/// This will always return <b>false</b> on devices without <see cref="DeviceFeatureLevel.Sm5"/> feature level support.
		/// </remarks>
		public bool SupportsMultiThreadedCreation()
		{
			if (RequestedFeatureLevel < DeviceFeatureLevel.Sm5)
			{
				return false;
			}

			bool result;
			bool dummy;

			return _device.CheckThreadingSupport(out result, out dummy) == Result.Ok && result;
		}

		/// <summary>
		/// Function to return the maximum number of quality levels supported by the device for multi sampling.
		/// </summary>
		/// <param name="format">A <see cref="BufferFormat"/> to evaluate.</param>
		/// <param name="count">Number of multi samples.</param>
		/// <returns>A <see cref="GorgonMultiSampleInfo"/> containing the quality count and sample count for multi-sampling.</returns>
		/// <remarks>
		/// Use this to return the quality count for a given multi-sample sample count. This method will return a <see cref="GorgonMultiSampleInfo"/> value type that contains both the sample count passed 
		/// to this method, and the quality count for that sample count. If the <see cref="GorgonMultiSampleInfo.Quality"/> is less than 1, then the sample count is not supported by this video device.
		/// </remarks>
		public GorgonMultiSampleInfo GetMultiSampleQuality(BufferFormat format, int count)
		{
			if (format == BufferFormat.Unknown)
			{
				return GorgonMultiSampleInfo.NoMultiSampling;
			}

			if (count < 1)
			{
				count = 1;
			}

			return new GorgonMultiSampleInfo(count, _device.CheckMultisampleQualityLevels((DXGI.Format)format, count));
		}

		/// <summary>
		/// Function to find a display mode supported by the Gorgon.
		/// </summary>
		/// <param name="output">The output to use when looking for a video mode.</param>
		/// <param name="videoMode">The <see cref="GorgonVideoMode"/> used to find the closest match.</param>
		/// <returns>A <see cref="GorgonVideoMode"/> that is the nearest match for the provided video mode.</returns>
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
		public GorgonVideoMode FindNearestVideoMode(GorgonVideoOutputInfo output, ref GorgonVideoMode videoMode)
		{
			using (DXGI.Output giOutput = _adapter.GetOutput(output.Index))
			{
				DXGI.ModeDescription mode;

				giOutput.GetClosestMatchingMode(_device, videoMode.ToModeDesc(), out mode);
				
				return new GorgonVideoMode(mode);
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			D3D11.Device device = Interlocked.Exchange(ref _device, null);
			DXGI.Adapter1 adapter = Interlocked.Exchange(ref _adapter, null);

			device?.Dispose();
			adapter?.Dispose();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="VideoDevice"/> class.
		/// </summary>
		/// <param name="deviceInfo">A <see cref="GorgonVideoDeviceInfo"/> containing information about which video device to use.</param>
		/// <param name="requestedFeatureLevel">The desired feature level for the device.</param>
		/// <param name="log">[Optional] A <see cref="IGorgonLog"/> used for logging debug output.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="deviceInfo"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="requestedFeatureLevel"/> is set to <see cref="DeviceFeatureLevel.Unsupported"/> or an unknown value.</exception>
		public VideoDevice(GorgonVideoDeviceInfo deviceInfo, DeviceFeatureLevel requestedFeatureLevel, IGorgonLog log = null)
		{
			if (deviceInfo == null)
			{
				throw new ArgumentNullException(nameof(deviceInfo));
			}

			if ((requestedFeatureLevel == DeviceFeatureLevel.Unsupported)
			    || (!Enum.IsDefined(typeof(DeviceFeatureLevel), requestedFeatureLevel)))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FEATURE_LEVEL_INVALID, requestedFeatureLevel), nameof(requestedFeatureLevel));
			}

			Info = deviceInfo;
			RequestedFeatureLevel = requestedFeatureLevel;
			_log = log ?? GorgonLogDummy.DefaultInstance;

			CreateDevice();
		}
		#endregion
	}
}
