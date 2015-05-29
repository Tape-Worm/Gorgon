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
using Gorgon.Core;
using Gorgon.Core.Collections;
using Gorgon.Core.Collections.Specialized;
using Gorgon.Graphics.Properties;
using GI = SharpDX.DXGI;
using D3DCommon = SharpDX.Direct3D;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Available feature levels for the video device.
	/// </summary>
	// ReSharper disable InconsistentNaming
	public enum DeviceFeatureLevel
	{
		/// <summary>
		/// Gorgon does not support any feature level for the video device.
		/// </summary>
		/// <remarks>This value is exclusive.</remarks>
		Unsupported = 0,
		/// <summary>
		/// Shader model 4.0
		/// </summary>
		/// <remarks>This the equivalent of a Direct 3D 10.0 video device.</remarks>
		SM4 = 1,
		/// <summary>
		/// Shader model 4.0 with a 4.1 profile.
		/// </summary>
		/// <remarks>This the equivalent of a Direct 3D 10.1 video device.</remarks>
		SM4_1 = 2,
		/// <summary>
		/// Shader model 5.0.
		/// </summary>
		SM5 = 3
	}
	// ReSharper restore InconsistentNaming

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
		: INamedObject
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the graphics interface that is using this video device.
		/// </summary>
		internal GorgonGraphics Graphics
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the index of the video device.
		/// </summary>
		internal int Index
		{
			get;
			private set;
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
		/// <remarks>This is independant of the <see cref="Gorgon.Graphics.GorgonVideoDevice.SupportedFeatureLevel">SupportedFeatureLevel</see> property and will always return the true hardware feature level.</remarks>
		public DeviceFeatureLevel HardwareFeatureLevel
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the supported feature levels.
		/// </summary>
		/// <remarks>
		/// This property will show either the requested feature level passed into the <see cref="Gorgon.Graphics.GorgonGraphics(GorgonVideoDevice, DeviceFeatureLevel)">GorgonGraphics constructor</see>, or the 
		/// <see cref="Gorgon.Graphics.GorgonVideoDevice.HardwareFeatureLevel">GorgonVideoDevice.HardwareFeatureLevel</see> property, depending on which is higher.
		/// <para>Due to the restrictions that may be imposed by specifying a feature level, the return value may differ from the <see cref="Gorgon.Graphics.GorgonVideoDevice.HardwareFeatureLevel">HardwareFeatureLevels</see> property.</para>
		/// </remarks>
		public DeviceFeatureLevel SupportedFeatureLevel
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the device ID.
		/// </summary>
		public int DeviceID
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the unique identifier for the device.
		/// </summary>
		public long UUID
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the revision for the device.
		/// </summary>
		public int Revision
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the sub system ID for the device.
		/// </summary>
		public int SubSystemID
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the vendor ID for the device.
		/// </summary>
		public int VendorID
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the amount of dedicated system memory for the device, in bytes.
		/// </summary>
		public long DedicatedSystemMemory
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the amount of dedicated video memory for the device, in bytes.
		/// </summary>
		public long DedicatedVideoMemory
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the amount of shared system memory for the device.
		/// </summary>
		public long SharedSystemMemory
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the outputs on this device.
		/// </summary>
		/// <remarks>The outputs are typically monitors attached to the device.</remarks>
		public IGorgonNamedObjectReadOnlyList<GorgonVideoOutput> Outputs
		{
			get;
			internal set;
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
		private D3DCommon.FeatureLevel[] GetFeatureLevel(DeviceFeatureLevel featureLevel)
		{
		    if (HardwareFeatureLevel < featureLevel)
		    {
		        featureLevel = HardwareFeatureLevel;
		    }

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
				default:
					throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_DEVICE_CANNOT_FIND_DEVICES);
			}
		}

        /// <summary>
        /// Function to release any temporary interfaces.
        /// </summary>
        /// <param name="interfaces">Interfaces to release.</param>
        private static void ReleaseInterfaces(Tuple<GI.Factory1, GI.Adapter1, D3D.Device> interfaces)
        {
            if (interfaces == null)
            {
                return;
            }

            if (interfaces.Item1 != null)
            {
                interfaces.Item1.Dispose();
            }

            if (interfaces.Item2 != null)
            {
                interfaces.Item2.Dispose();
            }

            if (interfaces.Item3 == null)
            {
                return;
            }

            interfaces.Item3.Dispose();
        }

        /// <summary>
        /// Function to return a device object for this video device.
        /// </summary>
        /// <returns>The new device object, adapter and factory.</returns>
        internal Tuple<GI.Factory1, GI.Adapter1, D3D.Device> GetDevice(VideoDeviceType deviceType, DeviceFeatureLevel featureLevel)
        {
            GI.Factory1 factory;
            GI.Adapter1 adapter;
            D3D.Device device;

            switch (deviceType)
            {
#if DEBUG
                case VideoDeviceType.ReferenceRasterizer:
                    device = new D3D.Device(D3DCommon.DriverType.Reference,
                                            D3D.DeviceCreationFlags.Debug,
                                            D3DCommon.FeatureLevel.Level_11_0)
                    {
                        DebugName = string.Format("{0} D3D11 Device", Name)
                    };

                    using (var giDevice = device.QueryInterface<GI.Device1>())
                    {
                        adapter = giDevice.GetParent<GI.Adapter1>();//giDevice.Adapter;
                        factory = adapter.GetParent<GI.Factory1>();
                    }
                    break;
#endif
                case VideoDeviceType.Software:
                    // WARP devices can only do SM4_1 or lower.
                    if (featureLevel >= DeviceFeatureLevel.SM5)
                    {
                        featureLevel = DeviceFeatureLevel.SM4_1;
                    }
#if DEBUG
                    device = new D3D.Device(D3DCommon.DriverType.Warp,
                                            D3D.DeviceCreationFlags.Debug,
                                            GetFeatureLevel(featureLevel))
                    {
                        DebugName = string.Format("{0} D3D11 Device", Name)
                    };
#else
		            device = new D3D.Device(D3DCommon.DriverType.Warp,
		                                    D3D.DeviceCreationFlags.None,
		                                    GetFeatureLevel(featureLevel));
#endif
                    using (var giDevice = device.QueryInterface<GI.Device1>())
                    {
                        adapter = giDevice.GetParent<GI.Adapter1>();
                        factory = adapter.GetParent<GI.Factory1>();
                    }
                    break;
                default:
                    factory = new GI.Factory1();
                    adapter = factory.GetAdapter1(Index);
#if DEBUG
                    device = new D3D.Device(adapter,
                                            D3D.DeviceCreationFlags.Debug,
                                            GetFeatureLevel(featureLevel))
                    {
                        DebugName = string.Format("{0} D3D11 Device", Name)
                    };
#else
                    device = new D3D.Device(adapter, D3D.DeviceCreationFlags.None, GetFeatureLevel(HardwareFeatureLevel));
#endif
                    break;
            }

            return new Tuple<GI.Factory1, GI.Adapter1, D3D.Device>(factory, adapter, device);
        }

        /// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GORGFX_DEVICE_TOSTR, Name);
		}

		/// <summary>
		/// Function to determine if the specified format is supported for display.
		/// </summary>
		/// <param name="format">Format to check.</param>
		/// <returns><c>true</c> if the format is supported for displaying on the video device, <c>false</c> if not.</returns>
		public bool SupportsDisplayFormat(BufferFormat format)
		{
			D3D.Device device = (Graphics != null) ? Graphics.D3DDevice : null;
			Tuple<GI.Factory1, GI.Adapter1, D3D.Device> tempInterfaces = null;

			try
			{
			    if (device != null)
			    {
			        return ((device.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.Display)
			                == D3D.FormatSupport.Display);
			    }

			    tempInterfaces = GetDevice(VideoDeviceType, HardwareFeatureLevel);
			    device = tempInterfaces.Item3;

			    return ((device.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.Display) == D3D.FormatSupport.Display);
			}
			finally
			{
                ReleaseInterfaces(tempInterfaces);
			}
		}

		/// <summary>
		/// Function to determine if the specified format is supported for a 3D texture.
		/// </summary>
		/// <param name="format">Format to check.</param>
		/// <returns><c>true</c> if the format is supported for the texture, <c>false</c> if not.</returns>
		public bool Supports3DTextureFormat(BufferFormat format)
		{
			D3D.Device device = (Graphics != null) ? Graphics.D3DDevice : null;
			Tuple<GI.Factory1, GI.Adapter1, D3D.Device> tempInterfaces = null;

			try
			{
			    if (device != null)
			    {
			        return ((device.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.Texture3D)
			                == D3D.FormatSupport.Texture3D);
			    }

			    tempInterfaces = GetDevice(VideoDeviceType, HardwareFeatureLevel);
			    device = tempInterfaces.Item3;

			    return ((device.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.Texture3D) == D3D.FormatSupport.Texture3D);
			}
			finally
			{
                ReleaseInterfaces(tempInterfaces);
			}
		}

		/// <summary>
		/// Function to determine if the specified format is supported for a render target.
		/// </summary>
		/// <param name="format">Format to check.</param>
		/// <param name="isMultiSampled"><c>true</c> if using a multisampled render target, <c>false</c> if not.</param>
		/// <returns><c>true</c> if the format is supported for the render target, <c>false</c> if not.</returns>
		public bool SupportsRenderTargetFormat(BufferFormat format, bool isMultiSampled)
		{
			D3D.Device device = (Graphics != null) ? Graphics.D3DDevice : null;
			Tuple<GI.Factory1, GI.Adapter1, D3D.Device> tempInterfaces = null;

			try
			{
			    if (device != null)
			    {
			        return (((device.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.RenderTarget)
			                 == D3D.FormatSupport.RenderTarget) &&
			                ((isMultiSampled)
			                 && ((device.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.MultisampleRenderTarget)
			                     == D3D.FormatSupport.MultisampleRenderTarget) ||
			                 (!isMultiSampled)));
			    }

			    tempInterfaces = GetDevice(VideoDeviceType, HardwareFeatureLevel);
			    device = tempInterfaces.Item3;

			    return (((device.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.RenderTarget) == D3D.FormatSupport.RenderTarget) &&
					((isMultiSampled) && ((device.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.MultisampleRenderTarget) == D3D.FormatSupport.MultisampleRenderTarget) ||
					(!isMultiSampled)));
			}
			finally
			{
                ReleaseInterfaces(tempInterfaces);
			}
		}

		/// <summary>
		/// Function to determine if the specified format is supported for a 2D texture.
		/// </summary>
		/// <param name="format">Format to check.</param>
		/// <returns><c>true</c> if the format is supported for the texture, <c>false</c> if not.</returns>
		public bool Supports2DTextureFormat(BufferFormat format)
		{
			D3D.Device device = (Graphics != null) ? Graphics.D3DDevice : null;
			Tuple<GI.Factory1, GI.Adapter1, D3D.Device> tempInterfaces = null;

			try
			{
			    if (device != null)
			    {
			        return ((device.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.Texture2D)
			                == D3D.FormatSupport.Texture2D);
			    }

			    tempInterfaces = GetDevice(VideoDeviceType, HardwareFeatureLevel);
			    device = tempInterfaces.Item3;

			    return ((device.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.Texture2D) == D3D.FormatSupport.Texture2D);
			}
			finally
			{
                ReleaseInterfaces(tempInterfaces);
			}
		}

		/// <summary>
		/// Function to determine if the specified format is supported for a 1D texture.
		/// </summary>
		/// <param name="format">Format to check.</param>
		/// <returns><c>true</c> if the format is supported for the texture, <c>false</c> if not.</returns>
		public bool Supports1DTextureFormat(BufferFormat format)
		{
			D3D.Device device = (Graphics != null) ? Graphics.D3DDevice : null;
			Tuple<GI.Factory1, GI.Adapter1, D3D.Device> tempInterfaces = null;

			try
			{
			    if (device != null)
			    {
			        return ((device.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.Texture1D)
			                == D3D.FormatSupport.Texture1D);
			    }

			    tempInterfaces = GetDevice(VideoDeviceType, HardwareFeatureLevel);
			    device = tempInterfaces.Item3;

			    return ((device.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.Texture1D) == D3D.FormatSupport.Texture1D);
			}
			finally
			{
                ReleaseInterfaces(tempInterfaces);
			}
		}

        /// <summary>
        /// Function to determine if the device supports a given format for unordered access views.
        /// </summary>
        /// <param name="format">Format to evaluate.</param>
        /// <returns><c>true</c> if the format is supported, <c>false</c> if not.</returns>
        /// <remarks>This is meant for UAVs that are used with a texture/buffer.  For structured buffers only Unknown is supported and for raw buffers only R32 (typeless) is supported.</remarks>
        public bool SupportsUnorderedAccessViewFormat(BufferFormat format)
        {
            D3D.Device device = (Graphics != null) ? Graphics.D3DDevice : null;
            Tuple<GI.Factory1, GI.Adapter1, D3D.Device> tempInterfaces = null;

            try
            {
                if (device == null)
                {
                    tempInterfaces = GetDevice(VideoDeviceType, HardwareFeatureLevel);
                    device = tempInterfaces.Item3;
                }

                switch (SupportedFeatureLevel)
                {
                    case DeviceFeatureLevel.SM4:
                    case DeviceFeatureLevel.SM4_1:
                        return false;
                    default:
                        return ((device.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.TypedUnorderedAccessView) == D3D.FormatSupport.TypedUnorderedAccessView);
                }
            }
            finally
            {
                ReleaseInterfaces(tempInterfaces);
            }
        }

        /// <summary>
        /// Function to determine if a device supports using rendering commands from multiple threads.
        /// </summary>
        /// <returns><c>true</c> if support is available, <c>false</c> if not.</returns>
        public bool SupportsMultithreadedCommands()
        {
            if (SupportedFeatureLevel < DeviceFeatureLevel.SM5)
            {
                return false;
            }

            D3D.Device device = (Graphics != null) ? Graphics.D3DDevice : null;
            Tuple<GI.Factory1, GI.Adapter1, D3D.Device> tempInterfaces = null;

            try
            {
                if (device == null)
                {
                    tempInterfaces = GetDevice(VideoDeviceType, HardwareFeatureLevel);
                    device = tempInterfaces.Item3;
                }

                bool result;
                bool dummy;

                device.CheckThreadingSupport(out dummy, out result);

                return result;
            }
            finally
            {
                ReleaseInterfaces(tempInterfaces);
            }
        }

        /// <summary>
        /// Function to determine if a device supports creating resources from multiple threads.
        /// </summary>
        /// <returns><c>true</c> if support is available, <c>false</c> if not.</returns>
        public bool SupportsMultithreadedCreation()
        {
            if (SupportedFeatureLevel < DeviceFeatureLevel.SM5)
            {
                return false;
            }

            D3D.Device device = (Graphics != null) ? Graphics.D3DDevice : null;
            Tuple<GI.Factory1, GI.Adapter1, D3D.Device> tempInterfaces = null;

            try
            {
                if (device == null)
                {
                    tempInterfaces = GetDevice(VideoDeviceType, HardwareFeatureLevel);
                    device = tempInterfaces.Item3;
                }

                bool result;
                bool dummy;

                device.CheckThreadingSupport(out result, out dummy);

                return result;
            }
            finally
            {
                ReleaseInterfaces(tempInterfaces);
            }
        }

		/// <summary>
		/// Function to determine if the specified format supports mip maps.
		/// </summary>
		/// <param name="format">Format to check.</param>
		/// <returns><c>true</c> if mipmaps are supported for the format, <c>false</c> if not.</returns>
		public bool SupportsMipMaps(BufferFormat format)
		{
			D3D.Device device = (Graphics != null) ? Graphics.D3DDevice : null;
			Tuple<GI.Factory1, GI.Adapter1, D3D.Device> tempInterfaces = null;

			try
			{
				if (device != null)
				{
					return ((device.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.Mip)
							== D3D.FormatSupport.Mip);
				}

				tempInterfaces = GetDevice(VideoDeviceType, HardwareFeatureLevel);
				device = tempInterfaces.Item3;

				return ((device.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.Mip) == D3D.FormatSupport.Mip);
			}
			finally
			{
				ReleaseInterfaces(tempInterfaces);
			}
		}

		/// <summary>
		/// Function to determine if the specified depth buffer format is supported.
		/// </summary>
		/// <param name="format">Format to check.</param>
		/// <returns><c>true</c> if the format is supported as a depth/stencil buffer, <c>false</c> if not.</returns>
		public bool SupportsDepthFormat(BufferFormat format)
		{
			D3D.Device device = (Graphics != null) ? Graphics.D3DDevice : null;
			Tuple<GI.Factory1, GI.Adapter1, D3D.Device> tempInterfaces = null;

			try
			{
			    if (device != null)
			    {
			        return ((device.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.DepthStencil)
			                == D3D.FormatSupport.DepthStencil);
			    }

			    tempInterfaces = GetDevice(VideoDeviceType, HardwareFeatureLevel);
			    device = tempInterfaces.Item3;

			    return ((device.CheckFormatSupport((GI.Format)format) & D3D.FormatSupport.DepthStencil) == D3D.FormatSupport.DepthStencil);
			}
			finally
			{
                ReleaseInterfaces(tempInterfaces);
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

			D3D.Device device = (Graphics != null) ? Graphics.D3DDevice : null;
			Tuple<GI.Factory1, GI.Adapter1, D3D.Device> tempInterfaces = null;

			try
			{
			    if (device != null)
			    {
			        return device.CheckMultisampleQualityLevels((GI.Format)format, count);
			    }

			    tempInterfaces = GetDevice(VideoDeviceType, HardwareFeatureLevel);
			    device = tempInterfaces.Item3;

			    return device.CheckMultisampleQualityLevels((GI.Format)format, count);
			}
			finally
			{
				ReleaseInterfaces(tempInterfaces);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoDevice"/> class.
		/// </summary>
		/// <param name="adapter">DXGI video adapter.</param>
		/// <param name="deviceType">Type of video device.</param>
		/// <param name="index">Index of the device.</param>
		internal GorgonVideoDevice(GI.Adapter1 adapter, VideoDeviceType deviceType, int index)
		{
			VideoDeviceType = deviceType;

			Index = index;
			DedicatedSystemMemory = adapter.Description1.DedicatedSystemMemory;
			DedicatedVideoMemory = adapter.Description1.DedicatedVideoMemory;
			DeviceID = adapter.Description1.DeviceId;
			HardwareFeatureLevel = DeviceFeatureLevel.Unsupported;
            UUID = adapter.Description1.Luid;
            Revision = adapter.Description1.Revision;
            SharedSystemMemory = adapter.Description1.SharedSystemMemory;
            SubSystemID = adapter.Description1.SubsystemId;
            VendorID = adapter.Description1.VendorId;

            switch (deviceType)
			{
				case VideoDeviceType.Software:
					Name = Resources.GORGFX_DEVICE_NAME_WARP;
			        HardwareFeatureLevel = SupportedFeatureLevel = DeviceFeatureLevel.SM4_1;
					break;
				case VideoDeviceType.ReferenceRasterizer:
					Name = Resources.GORGFX_DEVICE_NAME_REFRAST;
			        HardwareFeatureLevel = SupportedFeatureLevel = DeviceFeatureLevel.SM5;
					break;
				default:
					Name = adapter.Description1.Description;
                    EnumerateFeatureLevels(D3D.Device.GetSupportedFeatureLevel(adapter));
					break;
			}

			
			Outputs = new GorgonNamedObjectList<GorgonVideoOutput>();
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		public string Name
		{
			get;
			private set;
		}
		#endregion
	}
}
