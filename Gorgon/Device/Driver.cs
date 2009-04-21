#region MIT.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Monday, July 11, 2005 2:24:06 PM
// 
#endregion

using System;
using System.Reflection;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Internal;
using GorgonLibrary.Graphics;

namespace GorgonLibrary
{
	/// <summary>
	/// Object containing information about a video driver.
	/// </summary>
	/// <remarks>
	/// Since there can possibly be more than one video device in your machine, Direct3D needs to know
	/// which device it'll be using for rendering and the capabilities of said driver.
	/// </remarks>
	public class Driver
	{
		#region Variables.
		private D3D9.Capabilities _caps;						// Device capabilties.
		private int _driverOrdinal;								// Ordinal index of the driver.		
		private string _deviceName;								// Device name.		
		private string _driverName;								// Driver name.		
		private string _description;							// Device description.		
		private Guid _GUID;										// Device GUID.		
		private int _deviceID;									// Device ID.		
		private Version _version;								// Device version.		
		private int _revision;									// Device revision.		
		private int _subSystem;									// Device sub-system.		
		private int _vendor;									// Device vendor ID.		
		private string _WHQL;									// Device WHQL status (if supported).		
		private VideoModeList _videoModes;						// Device video modes.
		private bool _supportStencil;							// Device supports a stencil buffer?
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the device type.
		/// </summary>
		internal static D3D9.DeviceType DeviceType
		{
			get
			{
#if INCLUDE_D3DREF
				return Gorgon.UseReferenceDevice ? D3D9.DeviceType.Reference : D3D9.DeviceType.Hardware;
#else
				return D3D9.DeviceType.Hardware;
#endif
			}
		}

		/// <summary>
		/// Property to return windows device name.
		/// </summary>
		public string DeviceName
		{
			get
			{
				return _deviceName;
			}
		}

		/// <summary>
		/// Property to return device description.
		/// </summary>
		public string Description
		{
			get
			{
				return _description;
			}
		}

		/// <summary>
		/// Property to return driver name.
		/// </summary>
		public string DriverName
		{
			get
			{
				return _driverName;
			}
		}

		/// <summary>
		/// Property to return GUID of the device.
		/// </summary>
		public Guid GUID
		{
			get
			{
				return _GUID;
			}
		}

		/// <summary>
		/// Property to return the device ID.
		/// </summary>
		public int ID
		{
			get
			{
				return _deviceID;
			}
		}

		/// <summary>
		/// Property to return the version of the driver.
		/// </summary>
		public Version Version
		{
			get
			{
				return _version;
			}
		}

		/// <summary>
		/// Property to return the revision of the device.
		/// </summary>
		public int Revision
		{
			get
			{
				return _revision;
			}
		}

		/// <summary>
		/// Property to return the vendor ID of the device.
		/// </summary>
		public int Vendor
		{
			get
			{
				return _vendor;
			}
		}

		/// <summary>
		/// Property to return the WHQL status.
		/// </summary>
		public string Whql
		{
			get
			{
				return _WHQL;
			}
		}

		/// <summary>
		/// Property to return the sub system ID.
		/// </summary>
		public int SubSystem
		{
			get
			{
				return _subSystem;
			}
		}

		/// <summary>
		/// Property to return the video mode list for this device.
		/// </summary>
		public VideoModeList VideoModes
		{
			get
			{
				return _videoModes;
			}
		}

		/// <summary>
		/// Property to return the ordinal index of this driver.
		/// </summary>
		public int DriverIndex
		{
			get
			{
				return _driverOrdinal;
			}
		}

		/// <summary>
		/// Property to return whether this device is hardware accelerated or not.
		/// </summary>
		public bool HardwareAccelerated
		{
			get
			{
				return (_caps.DeviceCaps & D3D9.DeviceCaps.HWRasterization) == D3D9.DeviceCaps.HWRasterization;
			}
		}

		/// <summary>
		/// Property to return whether this device has transform and lighting acceleration.
		/// </summary>
		public bool HardwareTransformAndLighting
		{
			get
			{
				return (_caps.DeviceCaps & D3D9.DeviceCaps.HWTransformAndLight) == D3D9.DeviceCaps.HWTransformAndLight;
			}
		}

		/// <summary>
		/// Property to return the number of maximum streams for this driver.
		/// </summary>
		public short MaximumStreamCount
		{
			get
			{
				return (short)_caps.MaxStreams;
			}
		}

		/// <summary>
		/// Property to return whether the driver supports 32 bit indices or not.
		/// </summary>
		public bool SupportIndex32
		{
			get
			{
				return (_caps.MaxVertexIndex > 0x0000FFFF) ? true : false;
			}
		}

		/// <summary>
		/// Property to return whether the driver supports W Buffers or not.
		/// </summary>
		public bool SupportWBuffer
		{
			get
			{
				return (_caps.RasterCaps & D3D9.RasterCaps.WBuffer) == D3D9.RasterCaps.WBuffer;
			}
		}

		/// <summary>
		/// Property to return the maximum number of simultaneous render targets.
		/// </summary>
		public int MaximumSimultaneousRenderTargets
		{
			get
			{
				return _caps.SimultaneousRTCount;
			}
		}

		/// <summary>
		/// Property to return the maximum number of texture stages.
		/// </summary>
		public int MaximumTextureStages
		{
			get
			{
				return _caps.MaxSimultaneousTextures;
			}
		}

		/// <summary>
		/// Property to return the maximum number of blending stages.
		/// </summary>
		public int MaximumBlendingStages
		{
			get
			{
				return _caps.MaxTextureBlendStages;
			}
		}

		/// <summary>
		/// Property to return the maximum size for vertex points.
		/// </summary>
		public float MaximumPointSize
		{
			get
			{
				return _caps.MaxPointSize;
			}
		}

		/// <summary>
		/// Property to return the maximum number of lights this device supports.
		/// </summary>
		public int MaximumLightCount
		{
			get
			{
				if (HardwareTransformAndLighting)
					return _caps.MaxActiveLights;
				else
					return int.MaxValue;
			}
		}

		/// <summary>
		/// Property to return the maximum texture width.
		/// </summary>
		public int MaximumTextureWidth
		{
			get
			{
				return _caps.MaxTextureWidth;
			}
		}

		/// <summary>
		/// Property to return the maximum texture height.
		/// </summary>
		public int MaximumTextureHeight
		{
			get
			{
				return _caps.MaxTextureHeight;
			}
		}

		/// <summary>
		/// Property to return whether z buffer testing is supported or not.
		/// </summary>
		public bool SupportZBufferTesting
		{
			get
			{
				return (_caps.RasterCaps & D3D9.RasterCaps.DepthTest) == D3D9.RasterCaps.DepthTest;
			}
		}

		/// <summary>
		/// Property to return whether alternate hidden surface removal is supported.
		/// </summary>
		public bool SupportAlternateHSR
		{
			get
			{
				return (_caps.RasterCaps & D3D9.RasterCaps.ZBufferLessHsr) == D3D9.RasterCaps.ZBufferLessHsr;
			}
		}

		/// <summary>
		/// Property to return whether dithering is supported.
		/// </summary>
		public bool SupportDithering
		{
			get
			{
				return (_caps.RasterCaps & D3D9.RasterCaps.Dither) == D3D9.RasterCaps.Dither;
			}
		}

		/// <summary>
		/// Property to return whether dynamic textures are supported.
		/// </summary>
		public bool SupportDynamicTextures
		{
			get
			{
				return (_caps.Caps2 & D3D9.Caps2.DynamicTextures) == D3D9.Caps2.DynamicTextures;
			}
		}

		/// <summary>
		/// Property to return whether mip maps can be automatically generated.
		/// </summary>
		public bool SupportAutoGeneratingMipMaps
		{
			get
			{
				return (_caps.Caps2 & D3D9.Caps2.CanAutoGenerateMipMap) == D3D9.Caps2.CanAutoGenerateMipMap;
			}
		}

		/// <summary>
		/// Property to return the level of presentation interval support.
		/// </summary>
		public VSyncIntervals PresentIntervalSupport
		{
			get
			{
				VSyncIntervals result = VSyncIntervals.IntervalNone;		// No support.

				if (((_caps.PresentationIntervals & D3D9.PresentInterval.Default) != 0) || ((_caps.PresentationIntervals & D3D9.PresentInterval.Immediate) != 0))
					result |= VSyncIntervals.IntervalNone;

				if ((_caps.PresentationIntervals & D3D9.PresentInterval.One) != 0)
					result |= VSyncIntervals.IntervalOne;

				if ((_caps.PresentationIntervals & D3D9.PresentInterval.Two) != 0)
					result |= VSyncIntervals.IntervalTwo;

				if ((_caps.PresentationIntervals & D3D9.PresentInterval.Three) != 0)
					result |= VSyncIntervals.IntervalThree;

				if ((_caps.PresentationIntervals & D3D9.PresentInterval.Four) != 0)
					result |= VSyncIntervals.IntervalFour;

				return result;
			}
		}

		/// <summary>
		/// Property to return whether the device supports resource management.
		/// </summary>
		public bool SupportResourceManagement
		{
			get
			{
				return (_caps.Caps2 & D3D9.Caps2.CanManageResource) == D3D9.Caps2.CanManageResource;
			}
		}

		/// <summary>
		/// Property to return whether the anisotropic filtering is supported.
		/// </summary>
		public bool SupportAnisotropicFiltering
		{
			get
			{
				return (_caps.RasterCaps & D3D9.RasterCaps.Anisotropy) == D3D9.RasterCaps.Anisotropy;
			}
		}

		/// <summary>
		/// Property to return if vertex buffers in system memory can received TnL acceleration.
		/// </summary>
		public bool SupportVertexBufferInSystemMemory
		{
			get
			{
				return (_caps.DeviceCaps & D3D9.DeviceCaps.TLVertexSystemMemory) == D3D9.DeviceCaps.TLVertexSystemMemory;
			}
		}

		/// <summary>
		/// Property to return if vertex buffers in video memory can received TnL acceleration.
		/// </summary>
		public bool SupportVertexBufferInVideoMemory
		{
			get
			{
				return (_caps.DeviceCaps & D3D9.DeviceCaps.TLVertexVideoMemory) == D3D9.DeviceCaps.TLVertexVideoMemory;
			}
		}

		/// <summary>
		/// Property to return if seperate texture memory can be used.
		/// </summary>
		public bool SupportSeperateTextureMemories
		{
			get
			{
				return (_caps.DeviceCaps & D3D9.DeviceCaps.SeparateTextureMemory) == D3D9.DeviceCaps.SeparateTextureMemory;
			}
		}

		/// <summary>
		/// Property to return if the device supports non-square textures.
		/// </summary>
		/// <value></value>
		public bool SupportNonSquareTexture
		{
			get
			{
				return (_caps.TextureCaps & D3D9.TextureCaps.SquareOnly) != D3D9.TextureCaps.SquareOnly;
			}
		}

		/// <summary>
		/// Property to return whether this device supports non power of two textures.
		/// </summary>
		public bool SupportNonPowerOfTwoTexture
		{
			get
			{
				return (_caps.TextureCaps & D3D9.TextureCaps.Pow2) != D3D9.TextureCaps.Pow2;
			}
		}

		/// <summary>
		/// Property to return if this device supports non power of two textures (with conditions).
		/// </summary>
		public bool SupportNonPowerOfTwoTextureConditional
		{
			get
			{
				return (_caps.TextureCaps & D3D9.TextureCaps.NonPow2Conditional) == D3D9.TextureCaps.NonPow2Conditional;
			}
		}

		/// <summary>
		/// Property to return whether the device supports alpha in textures.
		/// </summary>
		public bool SupportTextureAlpha
		{
			get
			{
				return (_caps.TextureCaps & D3D9.TextureCaps.Alpha) == D3D9.TextureCaps.Alpha;
			}
		}

		/// <summary>
		/// Property to return whether the device supports texture wrapping.
		/// </summary>
		public bool SupportWrappingAddress
		{
			get
			{
				return (_caps.TextureAddressCaps & D3D9.TextureAddressCaps.Wrap) == D3D9.TextureAddressCaps.Wrap;
			}
		}

		/// <summary>
		/// Property to return whether the device supports texture clamping.
		/// </summary>
		public bool SupportClampingAddress
		{
			get
			{
				return (_caps.TextureAddressCaps & D3D9.TextureAddressCaps.Clamp) == D3D9.TextureAddressCaps.Clamp;
			}
		}

		/// <summary>
		/// Property to return whether the device supports texture mirroring.
		/// </summary>
		public bool SupportMirrorAddress
		{
			get
			{
				return (_caps.TextureAddressCaps & D3D9.TextureAddressCaps.Mirror) == D3D9.TextureAddressCaps.Mirror;
			}
		}

		/// <summary>
		/// Property to return whether the device supports texture mirroring once.
		/// </summary>
		public bool SupportMirrorOnceAddress
		{
			get
			{
				return (_caps.TextureAddressCaps & D3D9.TextureAddressCaps.MirrorOnce) == D3D9.TextureAddressCaps.MirrorOnce;
			}
		}

		/// <summary>
		/// Property to return whether the device supports texture borders.
		/// </summary>
		public bool SupportBorderAddress
		{
			get
			{
				return (_caps.TextureAddressCaps & D3D9.TextureAddressCaps.Border) == D3D9.TextureAddressCaps.Border;
			}
		}

		/// <summary>
		/// Property to return whether the device supports independent UV addressing.
		/// </summary>
		public bool SupportIndependentUVAddressing
		{
			get
			{
				return (_caps.TextureAddressCaps & D3D9.TextureAddressCaps.IndependentUV) == D3D9.TextureAddressCaps.IndependentUV;
			}
		}

		/// <summary>
		/// Property to return whether the driver supports blending factors or not.
		/// </summary>
		public bool SupportBlendingFactor
		{
			get
			{
				return ((_caps.SourceBlendCaps & D3D9.BlendCaps.BlendFactor) & (_caps.DestinationBlendCaps & D3D9.BlendCaps.BlendFactor)) == D3D9.BlendCaps.BlendFactor;
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending inverse source alpha.
		/// </summary>
		public bool SupportBlendingInverseSourceAlpha
		{
			get
			{
				return ((_caps.SourceBlendCaps & D3D9.BlendCaps.InverseSourceAlpha) & (_caps.SourceBlendCaps & D3D9.BlendCaps.InverseSourceAlpha)) == D3D9.BlendCaps.InverseSourceAlpha;
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the destination alpha.
		/// </summary>
		public bool SupportBlendingDestinationAlpha
		{
			get
			{
				return ((_caps.SourceBlendCaps & D3D9.BlendCaps.DestinationAlpha) & (_caps.DestinationBlendCaps & D3D9.BlendCaps.DestinationAlpha)) == D3D9.BlendCaps.DestinationAlpha;
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending both inverse source alpha.
		/// </summary>
		public bool SupportBlendingBothInverseSourceAlpha
		{
			get
			{
				return (_caps.SourceBlendCaps & D3D9.BlendCaps.BothInverseSourceAlpha) == D3D9.BlendCaps.BothInverseSourceAlpha; 
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the destination color.
		/// </summary>
		public bool SupportBlendingDestinationColor
		{
			get
			{
				return ((_caps.SourceBlendCaps & D3D9.BlendCaps.DestinationColor) & (_caps.DestinationBlendCaps & D3D9.BlendCaps.DestinationColor)) == D3D9.BlendCaps.DestinationColor;
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the inverse destination alpha.
		/// </summary>
		public bool SupportBlendingInverseDestinationAlpha
		{
			get
			{
				return ((_caps.SourceBlendCaps & D3D9.BlendCaps.InverseDestinationAlpha) & (_caps.DestinationBlendCaps & D3D9.BlendCaps.InverseDestinationAlpha)) == D3D9.BlendCaps.InverseDestinationAlpha;
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the inverse destination color.
		/// </summary>
		public bool SupportBlendingInverseDestinationColor
		{
			get
			{
				return ((_caps.SourceBlendCaps & D3D9.BlendCaps.InverseDestinationColor) & (_caps.DestinationBlendCaps & D3D9.BlendCaps.InverseDestinationColor)) == D3D9.BlendCaps.InverseDestinationColor;
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the inverse source color.
		/// </summary>
		public bool SupportBlendingInverseSourceColor
		{
			get
			{
				return ((_caps.SourceBlendCaps & D3D9.BlendCaps.InverseSourceColor) & (_caps.DestinationBlendCaps & D3D9.BlendCaps.InverseSourceColor)) == D3D9.BlendCaps.InverseSourceColor;
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the constant color.
		/// </summary>
		public bool SupportBlendingOne
		{
			get
			{
				return ((_caps.SourceBlendCaps & D3D9.BlendCaps.One) & (_caps.DestinationBlendCaps & D3D9.BlendCaps.One)) == D3D9.BlendCaps.One;
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending passthrough.
		/// </summary>
		public bool SupportBlendingZero
		{
			get
			{
				return ((_caps.SourceBlendCaps & D3D9.BlendCaps.Zero) & (_caps.DestinationBlendCaps & D3D9.BlendCaps.Zero)) == D3D9.BlendCaps.Zero;
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the source alpha.
		/// </summary>
		public bool SupportBlendingSourceAlpha
		{
			get
			{
				return ((_caps.SourceBlendCaps & D3D9.BlendCaps.SourceAlpha) & (_caps.DestinationBlendCaps & D3D9.BlendCaps.SourceAlpha)) == D3D9.BlendCaps.SourceAlpha;
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the source color.
		/// </summary>
		public bool SupportBlendingSourceColor
		{
			get
			{
				return ((_caps.SourceBlendCaps & D3D9.BlendCaps.SourceColor) & (_caps.DestinationBlendCaps & D3D9.BlendCaps.SourceColor)) == D3D9.BlendCaps.SourceColor;
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the source alpha saturation..
		/// </summary>
		public bool SupportBlendingSourceAlphaSaturation
		{
			get
			{
				return ((_caps.SourceBlendCaps & D3D9.BlendCaps.SourceAlphaSaturated) & (_caps.DestinationBlendCaps & D3D9.BlendCaps.SourceAlphaSaturated)) == D3D9.BlendCaps.SourceAlphaSaturated;
			}
		}

		/// <summary>
		/// Property to return whether the card supports scissor tests.
		/// </summary>
		public bool SupportScissorTesting
		{
			get
			{
				return (_caps.RasterCaps & D3D9.RasterCaps.ScissorTest) == D3D9.RasterCaps.ScissorTest;
			}
		}

		/// <summary>
		/// Property to return what version of pixel shaders the card supports.
		/// </summary>
		public Version PixelShaderVersion
		{
			get
			{
				return _caps.PixelShaderVersion;
			}
		}

		/// <summary>
		/// Property to return what version of vertex shaders the card supports.
		/// </summary>
		public Version VertexShaderVersion
		{
			get
			{
				return _caps.VertexShaderVersion;
			}
		}

		/// <summary>
		/// Property to return whether the device supports the stencil keep operation.
		/// </summary>
		public bool SupportStencilKeep
		{
			get
			{
				return (_caps.StencilCaps & D3D9.StencilCaps.Keep) == D3D9.StencilCaps.Keep;
			}
		}

		/// <summary>
		/// Property to return whether the device supports the stencil decrement operation.
		/// </summary>
		public bool SupportStencilDecrement
		{
			get
			{
				return (_caps.StencilCaps & D3D9.StencilCaps.Decrement) == D3D9.StencilCaps.Decrement;
			}
		}

		/// <summary>
		/// Property to return whether the device supports the stencil decrement/saturate operation.
		/// </summary>
		public bool SupportStencilDecrementSaturate
		{
			get
			{
				return (_caps.StencilCaps & D3D9.StencilCaps.DecrementClamp) == D3D9.StencilCaps.DecrementClamp;
			}
		}

		/// <summary>
		/// Property to return whether the device supports the stencil increment operation.
		/// </summary>
		public bool SupportStencilIncrement
		{
			get
			{
				return (_caps.StencilCaps & D3D9.StencilCaps.Increment) == D3D9.StencilCaps.Increment;
			}
		}

		/// <summary>
		/// Property to return whether the device supports the stencil increment/saturate operation.
		/// </summary>
		public bool SupportStencilIncrementSaturate
		{
			get
			{
				return (_caps.StencilCaps & D3D9.StencilCaps.IncrementClamp) == D3D9.StencilCaps.IncrementClamp;
			}
		}

		/// <summary>
		/// Property to return whether the device supports the stencil invert operation.
		/// </summary>
		public bool SupportStencilInvert
		{
			get
			{
				return (_caps.StencilCaps & D3D9.StencilCaps.Invert) == D3D9.StencilCaps.Invert;
			}
		}

		/// <summary>
		/// Property to return whether the device supports the stencil replace operation.
		/// </summary>
		public bool SupportStencilReplace
		{
			get
			{
				return (_caps.StencilCaps & D3D9.StencilCaps.Replace) == D3D9.StencilCaps.Replace;
			}
		}

		/// <summary>
		/// Property to return whether the device supports the two sided stencil operations.
		/// </summary>
		public bool SupportTwoSidedStencil
		{
			get
			{
				return (_caps.StencilCaps & D3D9.StencilCaps.TwoSided) == D3D9.StencilCaps.TwoSided;
			}
		}

		/// <summary>
		/// Property to return whether the device supports the stencil zero operation.
		/// </summary>
		public bool SupportStencilZero
		{
			get
			{
				return (_caps.StencilCaps & D3D9.StencilCaps.Zero) == D3D9.StencilCaps.Zero;
			}
		}

		/// <summary>
		/// Property to return whether the device supports a stencil buffer or not.
		/// </summary>
		public bool SupportStencil
		{
			get
			{
				return _supportStencil;
			}
		}

		/// <summary>
		/// Property to return whether the device supports independent bit depths when rendering to multiple render targets.
		/// </summary>
		public bool SupportMRTIndependentBitDepths
		{
			get
			{
				return (_caps.PrimitiveMiscCaps & D3D9.PrimitiveMiscCaps.MrtIndependentBitDepths) == D3D9.PrimitiveMiscCaps.MrtIndependentBitDepths;
			}
		}

		/// <summary>
		/// Property to return whether the device supports post pixel shader blending operations.
		/// </summary>
		public bool SupportMRTPostPixelShaderBlending
		{
			get
			{
				return (_caps.PrimitiveMiscCaps & D3D9.PrimitiveMiscCaps.MrtPostPixelShaderBlending) == D3D9.PrimitiveMiscCaps.MrtPostPixelShaderBlending;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to find out if there's a stencil buffer or not.
		/// </summary>		
		/// <returns>TRUE if a stencil was found, FALSE if not.</returns>
		private bool FindStencil()
		{
			DepthBufferFormats[] depthFormats =			// Depth buffer/stencil formats.
					{ DepthBufferFormats.BufferDepth15Stencil1, DepthBufferFormats.BufferDepth24Stencil4, 
					DepthBufferFormats.BufferDepth24Stencil8, DepthBufferFormats.BufferDepth24Stencil8Lockable };
			BackBufferFormats[] formats = null;			// Back buffer formats.

			_supportStencil = false;
			formats = (BackBufferFormats[])Enum.GetValues(typeof(BackBufferFormats));

			// Examine each format.
			foreach (BackBufferFormats format in formats)
			{
				foreach (DepthBufferFormats depthFormat in depthFormats)
				{
					if (Gorgon.Direct3D.CheckDeviceFormat(_driverOrdinal, Driver.DeviceType, Converter.Convert(format), D3D9.Usage.DepthStencil, D3D9.ResourceType.Surface, Converter.ConvertDepthFormat(depthFormat)))
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Function to retrieve driver details.
		/// </summary>
		/// <summary>
		/// Function to return the driver details.
		/// </summary>
		private void GetDriverDetails()
		{
			D3D9.AdapterInformation adapterData;		// Adapter information.

			// Get adapter information.
			adapterData = Gorgon.Direct3D.Adapters[_driverOrdinal];

			// Get capabilities.
			_caps = Gorgon.Direct3D.GetDeviceCaps(_driverOrdinal, Driver.DeviceType);

			// Get standard information.			
			_deviceName = adapterData.Details.DeviceName.Trim();
			_description = adapterData.Details.Description.Trim();
			_GUID = adapterData.Details.DeviceIdentifier;
			_deviceID = adapterData.Details.DeviceId;
			_driverName = adapterData.Details.DriverName;
			_version = adapterData.Details.DriverVersion;
			_revision = adapterData.Details.Revision;
			_subSystem = adapterData.Details.SubsystemId;
			_vendor = adapterData.Details.VendorId;
			_supportStencil = FindStencil();

			// Get WHQL data.
			if ((adapterData.Details.WhqlLevel != 0) && (adapterData.Details.WhqlLevel != 1))
				_WHQL = "WHQL Certified on " + ((adapterData.Details.WhqlLevel >> 8) & 0xFF) + "/" + (adapterData.Details.WhqlLevel & 0xFF) + "/" + ((adapterData.Details.WhqlLevel >> 16) & 0xFFFF);
			else
			{
				if (adapterData.Details.WhqlLevel == 1)
					_WHQL = "Driver is WHQL certified.";
				else
					_WHQL = "Driver is not WHQL certified.";
			}

			// Log card information.			
			if (Gorgon.Direct3D.CheckWhql)
				Gorgon.Log.Print("Driver", "WHQL Data enumerated.", LoggingLevel.Verbose);
			else
				Gorgon.Log.Print("Driver", "WHQL Data not enumerated.", LoggingLevel.Verbose);

			Gorgon.Log.Print("Driver", "Driver Name: {0}", LoggingLevel.Verbose, _driverName);
			Gorgon.Log.Print("Driver", "Device Name: {0}", LoggingLevel.Verbose, _deviceName);
			Gorgon.Log.Print("Driver", "Description: {0}", LoggingLevel.Intermediate, _description);
			Gorgon.Log.Print("Driver", "GUID: {{{0}}}", LoggingLevel.Verbose, _GUID);
			Gorgon.Log.Print("Driver", "Version: {0}", LoggingLevel.Verbose, _version);
			Gorgon.Log.Print("Driver", "Vendor ID: 0x{0}", LoggingLevel.Verbose, _vendor.ToString("x").PadLeft(8, '0'));
			Gorgon.Log.Print("Driver", "Device ID: 0x{0}", LoggingLevel.Verbose, _deviceID.ToString("x").PadLeft(8, '0'));
			Gorgon.Log.Print("Driver", "Sub-System ID: 0x{0}", LoggingLevel.Verbose, _subSystem.ToString("x").PadLeft(8, '0'));
			Gorgon.Log.Print("Driver", "Revision: {0}", LoggingLevel.Verbose, _revision);

			if (Gorgon.Direct3D.CheckWhql)
				Gorgon.Log.Print("Driver", "WHQL: {0}", LoggingLevel.Verbose, _WHQL);

			PropertyInfo[] properties = null;		// Property list.

			// Get the properties for this object.
			properties = GetType().GetProperties();

			// Display capabilities.
			foreach (PropertyInfo property in properties)
			{
				// Skip out on these properties.
				if ((string.Compare(property.Name, "videomodes", true) != 0) &&
					(string.Compare(property.Name, "devicename", true) != 0) &&
					(string.Compare(property.Name, "description", true) != 0) &&
					(string.Compare(property.Name, "drivername", true) != 0) &&
					(string.Compare(property.Name, "guid", true) != 0) &&
					(string.Compare(property.Name, "driverindex", true) != 0) &&
					(string.Compare(property.Name, "version", true) != 0) &&
					(string.Compare(property.Name, "revision", true) != 0) &&
					(string.Compare(property.Name, "id", true) != 0) &&
					(string.Compare(property.Name, "subsystem", true) != 0) &&
					(string.Compare(property.Name, "vendor", true) != 0) &&
					(string.Compare(property.Name, "whql", true) != 0))
					Gorgon.Log.Print(Description + " Driver Capabilities", "{0}: {1}", LoggingLevel.Verbose, property.Name, property.GetValue(this, null));
			}

			// Get video mode list.
			_videoModes = new VideoModeList(this);
		}

		/// <summary>
		/// Function to return whether this format can be used for hardware accelerated rendering.
		/// </summary>
		/// <param name="display">Format of the display to check.</param>
		/// <param name="backbuffer">Back buffer format.</param>
		/// <param name="windowed">TRUE if we're going to be windowed, FALSE if not.</param>
		/// <returns>TRUE if this format can be used, FALSE if not.</returns>
		public bool ValidImageFormat(ImageBufferFormats display, BackBufferFormats backbuffer, bool windowed)
		{
			return Gorgon.Direct3D.CheckDeviceType(_driverOrdinal, Driver.DeviceType, Converter.Convert(display), Converter.Convert(backbuffer), windowed);
		}

		/// <summary>
		/// Function to return whether this back buffer format can be used for hardware accelerated rendering.
		/// </summary>
		/// <param name="display">Format of the display to check.</param>
		/// <param name="backbuffer">Back buffer format.</param>
		/// <param name="windowed">TRUE if we're going to be windowed, FALSE if not.</param>
		/// <returns>TRUE if this format can be used, FALSE if not.</returns>
		public bool ValidBackBufferFormat(BackBufferFormats display, BackBufferFormats backbuffer, bool windowed)
		{
			return Gorgon.Direct3D.CheckDeviceType(_driverOrdinal, Driver.DeviceType, Converter.Convert(display), Converter.Convert(backbuffer), windowed);
		}

		/// <summary>
		/// Function to return whether a image format is supported given a specific image type.
		/// </summary>
		/// <param name="format">Format to check.</param>
		/// <param name="type">Type of image.</param>
		/// <returns>TRUE if valid, FALSE if not.</returns>
		public bool ValidImageFormat(ImageBufferFormats format, ImageType type)
		{
			D3D9.Usage usage;					// Usage type.
			BackBufferFormats bufferFormat;		// Back buffer format.

			usage = D3D9.Usage.None;
			if (type == ImageType.RenderTarget)
				usage = D3D9.Usage.RenderTarget;
			if (type == ImageType.Dynamic)
				usage = D3D9.Usage.Dynamic;

			bufferFormat = Gorgon.CurrentVideoMode.Format;

			return Gorgon.Direct3D.CheckDeviceFormat(_driverOrdinal, Driver.DeviceType, Converter.Convert(bufferFormat), usage, D3D9.ResourceType.Texture, Converter.Convert(format));
		}

		/// <summary>
		/// Function to determine if we can convert between a specified format and the desktop format.
		/// </summary>
		/// <param name="sourceformat">Format to convert from.</param>
		/// <returns>TRUE if supported, FALSE if not.</returns>
		public bool ValidDesktopFormat(BackBufferFormats sourceformat)
		{
			return Gorgon.Direct3D.CheckDeviceFormatConversion(_driverOrdinal, Driver.DeviceType, Converter.Convert(sourceformat), Converter.Convert(Gorgon.DesktopVideoMode.Format));
		}

		/// <summary>
		/// Function to return whether rendering in the desktop format is allowed.
		/// </summary>
		/// <param name="mode">Backbuffer video mode.</param>
		/// <returns>TRUE if supported, FALSE if not.</returns>
		public bool DesktopFormatSupported(VideoMode mode)
		{
			return ((ValidBackBufferFormat(Gorgon.DesktopVideoMode.Format, mode.Format, true)) && (ValidDesktopFormat(mode.Format)));
		}

		/// <summary>
		/// Function to check for the availability of a particular depth/stencil buffer.
		/// </summary>
		/// <param name="backBuffer">Back buffer format.</param>
		/// <param name="depthBuffer">Depth buffer format.</param>
		/// <returns>TRUE if supported, FALSE if not.</returns>
		public bool DepthFormatSupported(BackBufferFormats backBuffer, DepthBufferFormats depthBuffer)
		{
			if (Gorgon.Direct3D.CheckDeviceFormat(_driverOrdinal, Driver.DeviceType, Converter.Convert(backBuffer), D3D9.Usage.DepthStencil, D3D9.ResourceType.Surface, Converter.ConvertDepthFormat(depthBuffer)))
				return Gorgon.Direct3D.CheckDepthStencilMatch(_driverOrdinal, Driver.DeviceType, Converter.Convert(backBuffer), Converter.Convert(backBuffer), Converter.ConvertDepthFormat(depthBuffer));

			return false;
		}

		/// <summary>
		/// Function to determine if a depth buffer can be used with a specific image format.
		/// </summary>
		/// <param name="format">Format to compare.</param>
		/// <returns>TRUE if available, FALSE if not.</returns>
		public bool DepthBufferAvailable(ImageBufferFormats format)
		{
			DepthBufferFormats[] depth = new DepthBufferFormats[] { 
								DepthBufferFormats.BufferDepth32Lockable, DepthBufferFormats.BufferDepth32,
								DepthBufferFormats.BufferDepth24Stencil8, DepthBufferFormats.BufferDepth24Stencil4,
								DepthBufferFormats.BufferDepth24, DepthBufferFormats.BufferDepth16Lockable, 
								DepthBufferFormats.BufferDepth16, DepthBufferFormats.BufferDepth15Stencil1};

			// Check format.
			for (int i = 0; i < depth.Length; i++)
			{
				if (DepthFormatSupported(format, depth[i]))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Function to determine if a stencil buffer can be used with a specific image format.
		/// </summary>
		/// <param name="format">Format to compare.</param>
		/// <returns>TRUE if available, FALSE if not.</returns>
		public bool StencilBufferAvailable(ImageBufferFormats format)
		{
			DepthBufferFormats[] depth = new DepthBufferFormats[] { 
								DepthBufferFormats.BufferDepth24Stencil8, DepthBufferFormats.BufferDepth24Stencil4,
								DepthBufferFormats.BufferDepth15Stencil1};

			// Check format.
			for (int i = 0; i < depth.Length; i++)
			{
				if (DepthFormatSupported(format, depth[i]))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Function to check for the availability of a particular depth/stencil buffer.
		/// </summary>
		/// <param name="backBuffer">Back buffer format.</param>
		/// <param name="depthBuffer">Depth buffer format.</param>
		/// <returns>TRUE if supported, FALSE if not.</returns>
		public bool DepthFormatSupported(ImageBufferFormats backBuffer, DepthBufferFormats depthBuffer)
		{
			if (Gorgon.Direct3D.CheckDeviceFormat(_driverOrdinal, Driver.DeviceType, Converter.Convert(backBuffer), D3D9.Usage.DepthStencil, D3D9.ResourceType.Surface, Converter.ConvertDepthFormat(depthBuffer)))
				return Gorgon.Direct3D.CheckDepthStencilMatch(_driverOrdinal, Driver.DeviceType, Converter.Convert(backBuffer), Converter.Convert(backBuffer), Converter.ConvertDepthFormat(depthBuffer));

			return false;
		}

		/// <summary>
		/// Function to return a hash code for this object.
		/// </summary>
		/// <returns>Hash code of the object.</returns>
		public override int GetHashCode()
		{
			return _GUID.GetHashCode() ^ _deviceID.GetHashCode() ^ _deviceName.GetHashCode() ^ _driverName.GetHashCode();
		}

		/// <summary>
		/// Function to return a string representation of this object.
		/// </summary>
		/// <returns>String representation.</returns>
		public override string ToString()
		{
			string result = string.Empty;		// Resultant string.

			result = "Driver:\n";
			result += "\tDriver Name: \"" + _driverName + "\"\n";
			result += "\tDevice Name: \"" + _deviceName + "\"\n";
			result += "\tDescription: \"" + _description + "\"\n";
			result += "\tGUID: {" + _GUID.ToString() + "}\n";
			result += "\tVersion: " + _version.ToString() + "\n";
			result += "\tVendor ID: 0x" + _vendor.ToString("x").PadLeft(8, '0') + "\n";
			result += "\tDevice ID: 0x" + _deviceID.ToString("x").PadLeft(8, '0') + "\n";
			result += "\tSub-System ID: 0x" + _subSystem.ToString("x").PadLeft(8, '0') + "\n";
			result += "\tRevision: " + _revision.ToString();

			return result;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="driverIndex">Index ordinal of the video adapter.</param>
		internal Driver(int driverIndex)
		{
			_driverOrdinal = driverIndex;
			GetDriverDetails();
		}
		#endregion
	}
}
