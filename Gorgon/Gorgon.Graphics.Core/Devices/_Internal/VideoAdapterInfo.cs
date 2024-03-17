
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Friday, December 11, 2015 9:55:34 PM
// 


using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides information about a video adapter in the system
/// </summary>
/// <remarks>
/// <para>
/// This information may be for a physical hardware device, or a software rasterizer.  To determine which type this device falls under, se the <see cref="VideoDeviceType"/> property to determine the type of device
/// </para>
/// </remarks>
internal class VideoAdapterInfo
    : IGorgonVideoAdapterInfo
{

    // The DXGI adapter description
    private readonly AdapterDescription2 _adapterDesc;



    /// <summary>
    /// Property to return the maximum number of render targets allow to be assigned at the same time.
    /// </summary>
    public int MaxRenderTargetCount => D3D11.OutputMergerStage.SimultaneousRenderTargetCount;

    /// <summary>
    /// Property to return the maximum number of array indices for 1D and 2D textures.
    /// </summary>
    public int MaxTextureArrayCount => 2048;

    /// <summary>
    /// Property to return the maximum width of a 1D or 2D texture.
    /// </summary>
    public int MaxTextureWidth => 16384;

    /// <summary>
    /// Property to return the maximum height of a 2D texture.
    /// </summary>
    public int MaxTextureHeight => 16384;

    /// <summary>
    /// Property to return the maximum width of a 3D texture.
    /// </summary>
    public int MaxTexture3DWidth => 2048;

    /// <summary>
    /// Property to return the maximum height of a 3D texture.
    /// </summary>
    public int MaxTexture3DHeight => 2048;

    /// <summary>
    /// Property to return the maximum depth of a 3D texture.
    /// </summary>
    public int MaxTexture3DDepth => 2048;

    /// <summary>
    /// Property to return the maximum size, in bytes, for a constant buffer.
    /// </summary>
    public int MaxConstantBufferSize => int.MaxValue;

    /// <summary>
    /// Property to return the maximum number of allowed scissor rectangles.
    /// </summary>
    public int MaxScissorCount => 16;

    /// <summary>
    /// Property to return the maximum number of allowed viewports.
    /// </summary>
    public int MaxViewportCount => 16;

    /// <summary>
    /// Property to return the name of the video adapter.
    /// </summary>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Property to return the index of the video adapter within a list returned by <see cref="GorgonGraphics.EnumerateAdapters"/>.
    /// </summary>
    public int Index
    {
        get;
    }

    /// <summary>
    /// Property to return the type of video adapter.
    /// </summary>
    public VideoDeviceType VideoDeviceType
    {
        get;
    }

    /// <summary>
    /// Property to return the highest feature set that the hardware can support.
    /// </summary>
    public FeatureSet FeatureSet
    {
        get;
    }

    /// <summary>
    /// Property to return the unique identifier for the device.
    /// </summary>
    public long Luid => _adapterDesc.Luid;


    /// <summary>
    /// Property to return the outputs on this device.
    /// </summary>
    /// <remarks>The outputs are typically monitors attached to the device.</remarks>
    public GorgonVideoAdapterOutputList Outputs
    {
        get;
    }

    /// <summary>
    /// Property to return the amount of memory for the adapter, in bytes.
    /// </summary>
    public GorgonVideoAdapterMemory Memory
    {
        get;
    }

    /// <summary>
    /// Property to return the PCI bus information for the adapter.
    /// </summary>
    public GorgonVideoAdapterPciInfo PciInfo
    {
        get;
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="VideoAdapterInfo" /> class.
    /// </summary>
    /// <param name="index">The index of the video adapter within a list.</param>
    /// <param name="adapter">The DXGI adapter from which to retrieve all information.</param>
    /// <param name="featureSet">The supported feature set for the video adapter.</param>
    /// <param name="outputs">The list of outputs attached to the video adapter.</param>
    /// <param name="deviceType">The type of video adapter.</param>
    public VideoAdapterInfo(int index,
                           Adapter2 adapter,
                           FeatureSet featureSet,
                           Dictionary<string, VideoOutputInfo> outputs,
                           VideoDeviceType deviceType)
    {
        _adapterDesc = adapter.Description2;
        Memory = new GorgonVideoAdapterMemory(_adapterDesc.DedicatedSystemMemory, _adapterDesc.SharedSystemMemory, _adapterDesc.DedicatedVideoMemory);
        PciInfo = new GorgonVideoAdapterPciInfo(_adapterDesc.DeviceId, _adapterDesc.Revision, _adapterDesc.SubsystemId, _adapterDesc.VendorId);

        // Ensure that any trailing nulls are removed. This is unlikely to happen with D3D 11.x, but if we ever jump up to 12, we have to 
        // watch out for this as SharpDX does not strip the nulls.
        Name = _adapterDesc.Description.Replace("\0", string.Empty);
        Index = index;
        VideoDeviceType = deviceType;

        // Put a reference to the adapter on the output.
        // This will be handy for backtracking later.  Also it allows us to validate the output so that we are certain it's applied on the correct 
        // video adapter, allowing mixing & matching will likely end in tears.
        Dictionary<string, IGorgonVideoOutputInfo> finalOutputs = new(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, VideoOutputInfo> output in outputs)
        {
            output.Value.Adapter = this;
            finalOutputs[output.Key] = output.Value;
        }

        Outputs = new GorgonVideoAdapterOutputList(finalOutputs);
        FeatureSet = featureSet;
    }

}
