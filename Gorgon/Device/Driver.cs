#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Monday, July 11, 2005 2:24:06 PM
// 
#endregion

using System;
using SharpUtilities;
using SharpUtilities.Utility;
using DX = Microsoft.DirectX;
using D3D = Microsoft.DirectX.Direct3D;
using GorgonLibrary.Internal;

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
		private D3D.Caps _caps;									// Device capabilties.
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
		public bool Hal
		{
			get
			{
				return _caps.DeviceCaps.SupportsHardwareRasterization;
			}
		}

		/// <summary>
		/// Property to return whether this device has transform and lighting acceleration.
		/// </summary>
		public bool TnL
		{
			get
			{
				return _caps.DeviceCaps.SupportsHardwareTransformAndLight;
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
				return _caps.RasterCaps.SupportsWBuffer;
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
				if (TnL)
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
				return _caps.RasterCaps.SupportsZBufferTest;
			}
		}

		/// <summary>
		/// Property to return whether alternate hidden surface removal is supported.
		/// </summary>
		public bool SupportAlternateHSR
		{
			get
			{
				return _caps.RasterCaps.SupportsZBufferLessHsr;
			}
		}

		/// <summary>
		/// Property to return whether dithering is supported.
		/// </summary>
		public bool SupportDithering
		{
			get
			{
				return _caps.RasterCaps.SupportsDither;
			}
		}

		/// <summary>
		/// Property to return whether dynamic textures are supported.
		/// </summary>
		public bool SupportDynamicTextures
		{
			get
			{
				return _caps.DriverCaps.SupportsDynamicTextures;
			}
		}

		/// <summary>
		/// Property to return whether mip maps can be automatically generated.
		/// </summary>
		public bool SupportAutoGeneratingMipMaps
		{
			get
			{
				return _caps.DriverCaps.CanAutoGenerateMipMap;
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

				if (((_caps.PresentationIntervals & D3D.PresentInterval.Default) != 0) || ((_caps.PresentationIntervals & D3D.PresentInterval.Immediate) != 0))
					result |= VSyncIntervals.IntervalNone;

				if ((_caps.PresentationIntervals & D3D.PresentInterval.One) != 0)
					result |= VSyncIntervals.IntervalOne;

				if ((_caps.PresentationIntervals & D3D.PresentInterval.Two) != 0)
					result |= VSyncIntervals.IntervalTwo;

				if ((_caps.PresentationIntervals & D3D.PresentInterval.Three) != 0)
					result |= VSyncIntervals.IntervalThree;

				if ((_caps.PresentationIntervals & D3D.PresentInterval.Four) != 0)
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
				return _caps.DriverCaps.CanManageResource;
			}
		}

		/// <summary>
		/// Property to return whether the anisotropic filtering is supported.
		/// </summary>
		public bool SupportAnisotropicFiltering
		{
			get
			{
				return _caps.RasterCaps.SupportsAnisotropy;
			}
		}

		/// <summary>
		/// Property to return if vertex buffers in system memory can received TnL acceleration.
		/// </summary>
		public bool SupportVertexBufferInSystemMemory
		{
			get
			{
				return _caps.DeviceCaps.SupportsTransformedVertexSystemMemory;
			}
		}

		/// <summary>
		/// Property to return if vertex buffers in video memory can received TnL acceleration.
		/// </summary>
		public bool SupportVertexBufferInVideoMemory
		{
			get
			{
				return _caps.DeviceCaps.SupportsTransformedVertexVideoMemory;
			}
		}

		/// <summary>
		/// Property to return if seperate texture memory can be used.
		/// </summary>
		public bool SupportSeperateTextureMemories
		{
			get
			{
				return _caps.DeviceCaps.SupportsSeparateTextureMemories;
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
				return !_caps.TextureCaps.SupportsSquareOnly;
			}
		}

		/// <summary>
		/// Property to return whether this device supports non power of two textures.
		/// </summary>
		public bool SupportNonPowerOfTwoTexture
		{
			get
			{
				return !_caps.TextureCaps.SupportsPower2;
			}
		}

		/// <summary>
		/// Property to return if this device supports non power of two textures (with conditions).
		/// </summary>
		public bool SupportNonPowerOfTwoTextureConditional
		{
			get
			{
				return _caps.TextureCaps.SupportsNonPower2Conditional;
			}
		}

		/// <summary>
		/// Property to return whether the device supports alpha in textures.
		/// </summary>
		public bool SupportTextureAlpha
		{
			get
			{
				return _caps.TextureCaps.SupportsAlpha;
			}
		}

		/// <summary>
		/// Property to return whether the device supports texture wrapping.
		/// </summary>
		public bool SupportWrappingAddress
		{
			get
			{
				return _caps.TextureAddressCaps.SupportsWrap;
			}
		}

		/// <summary>
		/// Property to return whether the device supports texture clamping.
		/// </summary>
		public bool SupportClampingAddress
		{
			get
			{
				return _caps.TextureAddressCaps.SupportsClamp;
			}
		}

		/// <summary>
		/// Property to return whether the device supports texture mirroring.
		/// </summary>
		public bool SupportMirrorAddress
		{
			get
			{
				return _caps.TextureAddressCaps.SupportsMirror;
			}
		}

		/// <summary>
		/// Property to return whether the device supports texture mirroring once.
		/// </summary>
		public bool SupportMirrorOnceAddress
		{
			get
			{
				return _caps.TextureAddressCaps.SupportsMirrorOnce;
			}
		}

		/// <summary>
		/// Property to return whether the device supports texture borders.
		/// </summary>
		public bool SupportBorderAddress
		{
			get
			{
				return _caps.TextureAddressCaps.SupportsBorder;
			}
		}

		/// <summary>
		/// Property to return whether the device supports independent UV addressing.
		/// </summary>
		public bool SupportIndependentUVAddressing
		{
			get
			{
				return _caps.TextureAddressCaps.SupportsIndependentUV;
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
		/// Property to return whether the driver supports blending factors or not.
		/// </summary>
		public bool SupportBlendingFactor
		{
			get
			{
				return (_caps.SourceBlendCaps.SupportsBlendFactor && _caps.DestinationBlendCaps.SupportsBlendFactor);
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending inverse source alpha.
		/// </summary>
		public bool SupportBlendingInverseSourceAlpha
		{
			get
			{
				return (_caps.SourceBlendCaps.SupportsBothInverseSourceAlpha && _caps.DestinationBlendCaps.SupportsBothInverseSourceAlpha);
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the destination alpha.
		/// </summary>
		public bool SupportBlendingDestinationAlpha
		{
			get
			{
				return (_caps.SourceBlendCaps.SupportsDestinationAlpha && _caps.DestinationBlendCaps.SupportsDestinationAlpha);
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the destination color.
		/// </summary>
		public bool SupportBlendingDestinationColor
		{
			get
			{
				return (_caps.SourceBlendCaps.SupportsDestinationColor && _caps.DestinationBlendCaps.SupportsDestinationColor);
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the inverse destination alpha.
		/// </summary>
		public bool SupportBlendingInverseDestinationAlpha
		{
			get
			{
				return (_caps.SourceBlendCaps.SupportsInverseDestinationAlpha && _caps.DestinationBlendCaps.SupportsInverseDestinationAlpha);
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the inverse destination color.
		/// </summary>
		public bool SupportBlendingInverseDestinationColor
		{
			get
			{
				return (_caps.SourceBlendCaps.SupportsInverseDestinationColor && _caps.DestinationBlendCaps.SupportsInverseDestinationColor);
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the inverse source color.
		/// </summary>
		public bool SupportBlendingInverseSourceColor
		{
			get
			{
				return (_caps.SourceBlendCaps.SupportsInverseSourceColor && _caps.DestinationBlendCaps.SupportsInverseSourceColor);
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the constant color.
		/// </summary>
		public bool SupportBlendingOne
		{
			get
			{
				return (_caps.SourceBlendCaps.SupportsOne && _caps.DestinationBlendCaps.SupportsOne);
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending passthrough.
		/// </summary>
		public bool SupportBlendingZero
		{
			get
			{
				return (_caps.SourceBlendCaps.SupportsZero && _caps.DestinationBlendCaps.SupportsZero);
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the source alpha.
		/// </summary>
		public bool SupportBlendingSourceAlpha
		{
			get
			{
				return (_caps.SourceBlendCaps.SupportsSourceAlpha && _caps.DestinationBlendCaps.SupportsSourceAlpha);
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the source color.
		/// </summary>
		public bool SupportBlendingSourceColor
		{
			get
			{
				return (_caps.SourceBlendCaps.SupportsSourceColor && _caps.DestinationBlendCaps.SupportsSourceColor);
			}
		}

		/// <summary>
		/// Property to return whether the card supports blending the source alpha saturation..
		/// </summary>
		public bool SupportBlendingSourceAlphaSaturation
		{
			get
			{
				return (_caps.SourceBlendCaps.SupportsSourceAlphaSat && _caps.DestinationBlendCaps.SupportsSourceAlphaSat);
			}
		}

		/// <summary>
		/// Property to return whether the card supports scissor tests.
		/// </summary>
		public bool SupportScissorTesting
		{
			get
			{
				return _caps.RasterCaps.SupportsScissorTest;
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
				return _caps.StencilCaps.SupportsKeep;
			}
		}

		/// <summary>
		/// Property to return whether the device supports the stencil decrement operation.
		/// </summary>
		public bool SupportStencilDecrement
		{
			get
			{
				return _caps.StencilCaps.SupportsDecrement;
			}
		}

		/// <summary>
		/// Property to return whether the device supports the stencil decrement/saturate operation.
		/// </summary>
		public bool SupportStencilDecrementSaturate
		{
			get
			{
				return _caps.StencilCaps.SupportsDecrementSaturation;
			}
		}

		/// <summary>
		/// Property to return whether the device supports the stencil increment operation.
		/// </summary>
		public bool SupportStencilIncrement
		{
			get
			{
				return _caps.StencilCaps.SupportsIncrement;
			}
		}

		/// <summary>
		/// Property to return whether the device supports the stencil increment/saturate operation.
		/// </summary>
		public bool SupportStencilIncrementSaturate
		{
			get
			{
				return _caps.StencilCaps.SupportsIncrementSaturation;
			}
		}

		/// <summary>
		/// Property to return whether the device supports the stencil invert operation.
		/// </summary>
		public bool SupportStencilInvert
		{
			get
			{
				return _caps.StencilCaps.SupportsInvert;
			}
		}

		/// <summary>
		/// Property to return whether the device supports the stencil replace operation.
		/// </summary>
		public bool SupportStencilReplace
		{
			get
			{
				return _caps.StencilCaps.SupportsReplace;
			}
		}

		/// <summary>
		/// Property to return whether the device supports the two sided stencil operations.
		/// </summary>
		public bool SupportTwoSidedStencil
		{
			get
			{
				return _caps.StencilCaps.SupportsTwoSided;
			}
		}

		/// <summary>
		/// Property to return whether the device supports the stencil zero operation.
		/// </summary>
		public bool SupportStencilZero
		{
			get
			{
				return _caps.StencilCaps.SupportsZero;
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
		#endregion

		#region Methods.
		/// <summary>
		/// Function to find out if there's a stencil buffer or not.
		/// </summary>		
		/// <returns>TRUE if a stencil was found, FALSE if not.</returns>
		private bool FindStencil()
		{
			D3D.DepthFormat[] depthFormats = null;		// Depth buffer formats.
			BackBufferFormats[] formats = null;			// Back buffer formats.

			_supportStencil = false;
			depthFormats = (D3D.DepthFormat[])Enum.GetValues(typeof(D3D.DepthFormat));
			formats = (BackBufferFormats[])Enum.GetValues(typeof(BackBufferFormats));

			// Examine each format.
			foreach (BackBufferFormats format in formats)
			{
				foreach (D3D.DepthFormat depthFormat in depthFormats)
				{
					if (D3D.Manager.CheckDeviceFormat(_driverOrdinal, RenderWindow.DeviceType, Converter.Convert(format), D3D.Usage.DepthStencil, D3D.ResourceType.Surface, depthFormat))
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
			D3D.AdapterInformation adapterData;		// Adapter information.

			// Get adapter information.
			adapterData = D3D.Manager.Adapters[_driverOrdinal];

			// Get capabilities.
			_caps = D3D.Manager.GetDeviceCaps(_driverOrdinal, RenderWindow.DeviceType);

			// Get standard information.			
			_deviceName = adapterData.Information.DeviceName;
			_description = adapterData.Information.Description;
			_GUID = adapterData.Information.DeviceIdentifier;
			_deviceID = adapterData.Information.DeviceId;
			_driverName = adapterData.Information.DriverName;
			_version = adapterData.Information.DriverVersion;
			_revision = adapterData.Information.Revision;
			_subSystem = adapterData.Information.SubSystemId;
			_vendor = adapterData.Information.VendorId;
			_supportStencil = FindStencil();

			// Get WHQL data.
			if ((adapterData.Information.WhqlLevel != 0) && (adapterData.Information.WhqlLevel != 1))
				_WHQL = "WHQL Certified on " + ((adapterData.Information.WhqlLevel >> 8) & 0xFF) + "/" + (adapterData.Information.WhqlLevel & 0xFF) + "/" + ((adapterData.Information.WhqlLevel >> 16) & 0xFFFF);
			else
			{
				if (adapterData.Information.WhqlLevel == 1)
					_WHQL = "Driver is WHQL certified.";
				else
					_WHQL = "Driver is not WHQL certified.";
			}

			// Log card information.			
#if WHQL
			Gorgon.Log.Print("Driver","WHQL Data enumerated.",LoggingLevel.Verbose);
#else
			Gorgon.Log.Print("Driver", "WHQL Data not enumerated.", LoggingLevel.Verbose);
#endif
			Gorgon.Log.Print("Driver", "Driver Name: {0}", LoggingLevel.Verbose, _driverName);
			Gorgon.Log.Print("Driver", "Device Name: {0}", LoggingLevel.Verbose, _deviceName);
			Gorgon.Log.Print("Driver", "Description: {0}", LoggingLevel.Intermediate, _description);
			Gorgon.Log.Print("Driver", "GUID: {{{0}}}", LoggingLevel.Verbose, _GUID);
			Gorgon.Log.Print("Driver", "Version: {0}", LoggingLevel.Verbose, _version);
			Gorgon.Log.Print("Driver", "Vendor ID: 0x{0}", LoggingLevel.Verbose, _vendor.ToString("x").PadLeft(8, '0'));
			Gorgon.Log.Print("Driver", "Device ID: 0x{0}", LoggingLevel.Verbose, _deviceID.ToString("x").PadLeft(8, '0'));
			Gorgon.Log.Print("Driver", "Sub-System ID: 0x{0}", LoggingLevel.Verbose, _subSystem.ToString("x").PadLeft(8, '0'));
			Gorgon.Log.Print("Driver", "Revision: {0}", LoggingLevel.Verbose, _revision);
#if WHQL
			Gorgon.Log.Print("Driver", "WHQL: {0}",LoggingLevel.Verbose,_WHQL);
#endif
			Gorgon.Log.Print("Driver", "Capabilities...", LoggingLevel.Intermediate);
			Gorgon.Log.Print("Driver", "Is hardware accelerated: {0}", LoggingLevel.Intermediate, _caps.DeviceCaps.SupportsHardwareRasterization);
			Gorgon.Log.Print("Driver", "Is transform and lighting accelerated: {0}", LoggingLevel.Intermediate, _caps.DeviceCaps.SupportsHardwareTransformAndLight);

			// Get video mode list.
			_videoModes = new VideoModeList(_driverOrdinal);
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
