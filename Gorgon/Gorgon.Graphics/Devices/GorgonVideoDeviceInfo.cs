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
// Created: Friday, December 11, 2015 9:55:34 PM
// 
#endregion

using DXGI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D;
using Gorgon.Collections;
using Gorgon.Core;

namespace Gorgon.Graphics
{
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
	/// Available feature levels for the video device.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Feature levels define what the video device is capable of. If a device supports a feature level of 10, then it means that the device is only capable of supporting features supplied by Direct 3D 10. 
	/// This allows older video devices to use the Gorgon Graphics library by scaling back functionality. 
	/// </para>
	/// <para>
	/// <note type="important">
	/// <para>
	/// Gorgon only supports devices that are feature level 10 or better. Anything less than this will flag the feature level as <see cref="Unsupported"/>. This means if your video card is not capapble of 
	/// natively working with Direct 3D 10, then it will be unsupported and users of your application must be informed of this prior to running your application.
	/// </para>  
	/// </note>
	/// </para>
	/// </remarks>
	public enum DeviceFeatureLevel
	{
		/// <summary>
		/// Gorgon does not support any feature level for the video device.
		/// </summary>
		Unsupported = 0,
		/// <summary>
		/// <para>
		/// Shader model 4.0
		/// </para>
		/// <para>
		/// This the equivalent of a Direct 3D 10.0 video device.
		/// </para>
		/// </summary>
		Sm4 = 1,
		/// <summary>
		/// <para>
		/// Shader model 4.0 with a 4.1 profile.
		/// </para>
		/// <para>
		/// This is the equivalent of a Direct 3D 10.1 video device.
		/// </para>
		/// </summary>
		/// <remarks></remarks>
		Sm41 = 2,
		/// <summary>
		/// <para>
		/// Shader model 5.0.
		/// </para>
		/// <para>
		/// This is the equivalent of a Direct 3D 11 or better video device.
		/// </para>
		/// </summary>
		Sm5 = 3
	}

	/// <summary>
	/// Video device information.
	/// </summary>
	public class GorgonVideoDeviceInfo
		: IGorgonNamedObject
	{
		#region Variables.
		// The DXGI adapter description
		private readonly DXGI.AdapterDescription1 _adapterDesc;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the name of the video device.
		/// </summary>
		public string Name
		{
			get;
		}

		/// <summary>
		/// Property to return the index of the video device within a <see cref="GorgonVideoDeviceList"/>.
		/// </summary>
		public int Index
		{
			get;
		}

		/// <summary>
		/// Property to return the type of video device.
		/// </summary>
		public VideoDeviceType VideoDeviceType
		{
			get;
		}

		/// <summary>
		/// Property to return the highest feature level that the hardware can support.
		/// </summary>
		public DeviceFeatureLevel SupportedFeatureLevel
		{
			get;
		}

		/// <summary>
		/// Property to return the device ID.
		/// </summary>
		public int DeviceID => _adapterDesc.DeviceId;

		/// <summary>
		/// Property to return the unique identifier for the device.
		/// </summary>
		public long Luid => _adapterDesc.Luid;

		/// <summary>
		/// Property to return the revision for the device.
		/// </summary>
		public int Revision => _adapterDesc.Revision;

		/// <summary>
		/// Property to return the sub system ID for the device.
		/// </summary>
		public int SubSystemID => _adapterDesc.SubsystemId;

		/// <summary>
		/// Property to return the vendor ID for the device.
		/// </summary>
		public int VendorID => _adapterDesc.VendorId;

		/// <summary>
		/// Property to return the amount of dedicated system memory for the device, in bytes.
		/// </summary>
		/// <remarks>
		/// If the application is running as an x86 application, this value may report an incorrect value.
		/// </remarks>
		public long DedicatedSystemMemory => _adapterDesc.DedicatedSystemMemory;

		/// <summary>
		/// Property to return the amount of dedicated video memory for the device, in bytes.
		/// </summary>
		/// <remarks>
		/// If the application is running as an x86 application, this value may report an incorrect value.
		/// </remarks>
		public long DedicatedVideoMemory => _adapterDesc.DedicatedVideoMemory;

		/// <summary>
		/// Property to return the amount of shared system memory for the device.
		/// </summary>
		/// <remarks>
		/// If the application is running as an x86 application, this value may report an incorrect value.
		/// </remarks>
		public long SharedSystemMemory => _adapterDesc.SharedSystemMemory;

		/// <summary>
		/// Property to return the outputs on this device.
		/// </summary>
		/// <remarks>The outputs are typically monitors attached to the device.</remarks>
		public IGorgonNamedObjectReadOnlyList<GorgonVideoOutputInfo> Outputs
		{
			get;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVideoDeviceInfo" /> class.
		/// </summary>
		/// <param name="index">The index of the video device within a list.</param>
		/// <param name="adapter">The DXGI adapter from which to retrieve all information.</param>
		/// <param name="featureLevel">The supported feature level for the video device.</param>
		/// <param name="outputs">The list of outputs attached to the video device.</param>
		/// <param name="deviceType">The type of video device.</param>
		internal GorgonVideoDeviceInfo(int index,
		                       DXGI.Adapter1 adapter,
		                       DeviceFeatureLevel featureLevel,
		                       IGorgonNamedObjectReadOnlyList<GorgonVideoOutputInfo> outputs,
		                       VideoDeviceType deviceType)
		{
			_adapterDesc = adapter.Description1;
			// Ensure that any trailing NULLs are removed. This is unlikely to happen with D3D 11.x, but if we ever jump up to 12, we have to 
			// watch out for this as SharpDX does not strip the NULLs.
			Name = _adapterDesc.Description.Replace("\0", string.Empty);
			Index = index;
			VideoDeviceType = deviceType;
			Outputs = outputs;
			SupportedFeatureLevel = featureLevel;
		}
		#endregion
	}
}
