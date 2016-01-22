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
	/// Type of video device.
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
		Software = 1
	}

	/// <summary>
	/// Determines the granularity at which the GPU can be preempted from performing a graphics rendering task.
	/// </summary>
	public enum GraphicsPreemptionGranularity
	{
		/// <summary>
		/// Preemption will be at the DMA buffer boundary.
		/// </summary>
		DmaBuffer = DXGI.GraphicsPreemptionGranularity.DmaBufferBoundary,
		/// <summary>
		/// <para>
		/// Preemption will be at the primitive boundary. 
		/// </para>
		/// <para>
		/// A primitive is a section within a DMA buffer and can be a group of triangles.
		/// </para>
		/// </summary>
		Primitive = DXGI.GraphicsPreemptionGranularity.PrimitiveBoundary,
		/// <summary>
		/// Preemption will be at the triangle boundary.
		/// </summary>
		Triangle = DXGI.GraphicsPreemptionGranularity.TriangleBoundary,
		/// <summary>
		/// Preemption will be at the pixel boundary.
		/// </summary>
		Pixel = DXGI.GraphicsPreemptionGranularity.PixelBoundary,
		/// <summary>
		/// Preemption will be at a graphics instruction boundary for a pixel.
		/// </summary>
		Instruction = DXGI.GraphicsPreemptionGranularity.InstructionBoundary
	}

	/// <summary>
	/// Determines the granularity at which the GPU can be preempted from performing a compute task.
	/// </summary>
	public enum ComputePreemptionGranularity
	{
		/// <summary>
		/// Preemption will be at the compute packet boundary.
		/// </summary>
		ComputePacket = DXGI.ComputePreemptionGranularity.DmaBufferBoundary,
		/// <summary>
		/// <para>
		/// Preemption will be at the dispatch boundary.
		/// </para>
		/// <para>
		/// A dispatch is part of a compute packet.
		/// </para>
		/// </summary>
		Dispatch = DXGI.ComputePreemptionGranularity.DispatchBoundary,
		/// <summary>
		/// Preemption will be at the thread group boundary.
		/// </summary>
		ThreadGroup = DXGI.ComputePreemptionGranularity.ThreadGroupBoundary,
		/// <summary>
		/// Preemption will be at the thread boundary.
		/// </summary>
		Thread = DXGI.ComputePreemptionGranularity.ThreadBoundary,
		/// <summary>
		/// Preemption will be at the compute instruction boundary within a thread.
		/// </summary>
		Instruction = DXGI.ComputePreemptionGranularity.InstructionBoundary
	}

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
		/// Feature level 11.0
		/// </summary>
		/// <remarks>This the equivalent of a Direct 3D 11.0 video device.</remarks>
		FeatureLevel11_0 = D3D.FeatureLevel.Level_11_0,
		/// <summary>
		/// Feature level 11.1
		/// </summary>
		/// <remarks>This the equivalent of a Direct 3D 11.1 video device.</remarks>
		FeatureLevel11_1 = D3D.FeatureLevel.Level_11_1,
		/// <summary>
		/// Feature level 12.0
		/// </summary>
		/// <remarks>This is the equivalent of a Direct 3D 12.0 video device.</remarks>
		FeatureLevel12_0 = D3D.FeatureLevel.Level_12_0,
		/// <summary>
		/// Feature level 12.1
		/// </summary>
		/// <remarks>This is the equivalent of a Direct 3D 12.1 video device.</remarks>
		FeatureLevel12_1 = D3D.FeatureLevel.Level_12_1
	}

	/// <summary>
	/// Video device information.
	/// </summary>
	public class GorgonVideoDeviceInfo
		: IGorgonNamedObject
	{
		#region Variables.
		// The DXGI adapter description
		private readonly DXGI.AdapterDescription2 _adapterDesc;
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

		/// <summary>
		/// Property to return the granularity at which the GPU can be preempted from its current graphics rendering task.
		/// </summary>
		public GraphicsPreemptionGranularity GraphicsPreemptionGranularity => (GraphicsPreemptionGranularity)_adapterDesc.GraphicsPreemptionGranularity;

		/// <summary>
		/// Property to return the granularity at which the GPU can be preempted from its current compute task.
		/// </summary>
		public ComputePreemptionGranularity ComputePreemptionGranularity => (ComputePreemptionGranularity)_adapterDesc.ComputePreemptionGranularity;
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
		                       DXGI.Adapter3 adapter,
		                       DeviceFeatureLevel featureLevel,
		                       IGorgonNamedObjectReadOnlyList<GorgonVideoOutputInfo> outputs,
		                       VideoDeviceType deviceType)
		{
			_adapterDesc = adapter.Description2;
			// For some reason, the description has multiple NULL terminators at the end of the string.
			Name = _adapterDesc.Description.Replace("\0", string.Empty);
			Index = index;
			VideoDeviceType = deviceType;
			Outputs = outputs;
			SupportedFeatureLevel = featureLevel;
		}
		#endregion
	}
}
