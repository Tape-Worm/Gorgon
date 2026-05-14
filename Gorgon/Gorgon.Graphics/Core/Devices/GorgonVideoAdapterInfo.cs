
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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

using System.ComponentModel;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines the type of video adapter
/// </summary>
public enum VideoDeviceType
{
    /// <summary>
    /// Hardware video adapter.
    /// </summary>
    Hardware = 0,
    /// <summary>
    /// Software video adapter.
    /// </summary>
    Software = 1
}

/// <summary>
/// Identifies the granularity at which GPU can be preempted from performing its current compute task.
/// </summary>
public enum ComputePreemptionGranularity
{
    /// <summary>
    /// Indicates the preemption granularity as a compute packet.
    /// </summary>
    DMABufferBoundary = 0,

    /// <summary>
    /// Indicates the preemption granularity as a dispatch. A dispatch is a part of a compute packet.
    /// </summary>
    DispatchBoundary = 1,

    /// <summary>
    /// Indicates the preemption granularity as a thread group. A thread group is a part of a dispatch.
    /// </summary>
    ThreadGroupBoundary = 2,

    /// <summary>
    /// Indicates the preemption granularity as a thread in a thread group. A thread is a part of a thread group.
    /// </summary>
    ThreadBoundary = 3,

    /// <summary>
    /// Indicates the preemption granularity as a compute instruction in a thread.
    /// </summary>
    InstructionBoundary = 4
}

/// <summary>
/// The tiers available for resource heaps.
/// </summary>
public enum ResourceHeapTier
{
    /// <summary>
    /// Allows only a single resource category (buffers, textures, render targets/depth stencil textures) in a heap.
    /// </summary>
    Tier1 = D3D12_RESOURCE_HEAP_TIER.D3D12_RESOURCE_HEAP_TIER_1,
    /// <summary>
    /// Allows multiple resource categories (buffers, textures, render targets/depth stencil textures) in a heap.
    /// </summary>
    Tier2 = D3D12_RESOURCE_HEAP_TIER.D3D12_RESOURCE_HEAP_TIER_2
}

/// <summary>
/// The tiers available for tiled resources.
/// </summary>
public enum TiledResourcesTier
{
    /// <summary>
    /// No support for tiled resources. This adaper will not work with Gorgon.
    /// </summary>
    None = D3D12_TILED_RESOURCES_TIER.D3D12_TILED_RESOURCES_TIER_NOT_SUPPORTED,
    /// <summary>
    /// <para>
    /// Indicates that 2D textures can be created with an undefined swizzle layout. Limitations exist for certain resource formats and properties.
    /// </para>
    /// <para>
    /// GPU reads or writes to NULL mappings are undefined. Applications are encouraged to workaround this limitation by repeatedly mapping the same page to everywhere a NULL mapping would've been used.
    /// </para>
    /// </summary>
    Tier1 = D3D12_TILED_RESOURCES_TIER.D3D12_TILED_RESOURCES_TIER_1,
    /// <summary>
    /// <para>
    /// Indicates that a superset of Tier_1 functionality is supported, including this additional support:
    /// </para>
    /// <para>
    /// When the size of a texture mipmap level is at least one standard tile shape for its format, the mipmap level is guaranteed to be nonpacked.
    /// </para>
    /// <para>
    /// Shader instructions are available for clamping level-of-detail (LOD) and for obtaining status about the shader operation.
    /// </para>
    /// </summary>
    Tier2 = D3D12_TILED_RESOURCES_TIER.D3D12_TILED_RESOURCES_TIER_2,
    /// <summary>
    /// Indicates that a superset of Tier 2 is supported, with the addition that 3D textures (Volume Tiled Resources) are supported.
    /// </summary>
    Tier3 = D3D12_TILED_RESOURCES_TIER.D3D12_TILED_RESOURCES_TIER_3,
    /// <summary>
    /// Undefined 64K swizzle texture arrays can be created with a full mip chain.
    /// </summary>
    Tier4 = D3D12_TILED_RESOURCES_TIER.D3D12_TILED_RESOURCES_TIER_4,
}

/// <summary>
/// Identifies the granularity at which the GPU can be preempted from performing its current graphics rendering task.
/// </summary>
public enum GraphicsPreemptionGranularity
{
    /// <summary>
    /// Indicates the preemption granularity as a DMA buffer.
    /// </summary>
    DMABufferBoundary = 0,

    /// <summary>
    /// Indicates the preemption granularity as a graphics primitive. A primitive is a section in a DMA buffer and can be a group of triangles.
    /// </summary>
    PrimitiveBoundary = 1,

    /// <summary>
    /// Indicates the preemption granularity as a triangle. A triangle is a part of a primitive.
    /// </summary>
    TriangleBoundary = 2,

    /// <summary>
    /// Indicates the preemption granularity as a pixel. A pixel is a part of a triangle.
    /// </summary>
    PixelBoundary = 3,

    /// <summary>
    /// Indicates the preemption granularity as a graphics instruction. A graphics instruction operates on a pixel.
    /// </summary>
    InstructionBoundary = 4
}

/// <summary>
/// Provides information about a video adapter in the system
/// </summary>
/// <param name="Name">The friendly name of the video adapter.</param>
/// <param name="Index">The index of the video adapter within a list returned by <see cref="GorgonGraphicsFactory.EnumerateAdapters(bool)"/>.</param>
/// <param name="VideoDeviceType">The type of video adapter.</param>
/// <param name="Luid">The unique identifier for the adapter.</param>
/// <param name="Outputs">The outputs on this device</param>
/// <param name="Memory">The amount of memory for the adapter, in bytes.</param>
/// <param name="MaxAddressBitsPerProcess">The number of addressable bits of GPU memory on a process level.</param>
/// <param name="MaxAddressBitsPerResource">The number of addressable bits of GPU memory on a resource level.</param>
/// <param name="PciInfo">The PCI bus information for the adapter.</param>
/// <param name="AcgCompatible">The value that indicates whether the adapter's driver has been confirmed to work in an OS process where Arbitrary Code Guard (ACG) is enabled (i.e. dynamic code generation is disallowed).</param>
/// <param name="SupportsMonitoredFences">The value that indicates whether the adapter supports monitored fences.</param>
/// <param name="SupportsNonMonitoredFences">The value that indicates whether the adapter supports non-monitored fences.
/// <para>
/// Monitored fences should always be used by supporting adapters unless communicating with an adapter that only supports non-monitored fences.
/// </para>
/// </param>
/// <param name="SupportsKeyedMutexConformance">The value that indicates whether the adapter claims keyed mutex conformance. This signals a stronger guarantee that the keyed mutex interface behaves correctly.</param>
/// <param name="AllowTearing">The value that indicates whether the adapter allows for tearing of the image when presenting without v-sync.
/// <para>
/// When a device presents its frame to the screen, it will typically wait until v-sync is done and then display. However, by doing an immediate presentation and allowing the image to display, even if 
/// in the middle of a v-sync operation, giving a "torn" look to the image.
/// </para>
/// <para>
/// Applications that wish to enable this, can do so when presenting a swap chain, but should check this property first to ensure the system supports it. The swap chain must be full screen, in that the 
/// back buffer dimensions must match the width and height of the owning window, and the window client area should cover the width and height of the display output.
/// </para>
/// <para>
/// Applications must use tearing in order to make use of variable refresh rate displays.
/// </para>
/// </param>
/// <param name="HasGpuUploadSupport">The value that indicates whether the GPU supports direct GPU upload support.</param>
/// <param name="ShaderModelSupport">The value that indicates the highest level of shader model supported by this adapter.</param>
/// <param name="HasTightAlignmentSupport">The value that indicates whether the adapter supports tight packing of resource data within heaps.</param>
/// <param name="ResourceHeapTier">The value that indicates the tier support for resource heaps.
/// <para>
/// Resource heap tier 1 support indicates that a heap can only support resources of a single category (Buffers, non depth/non render target textures, and depth and render target textures). Resource 
/// heap tier 2 supports a heap that contain any or all of the categories of resources.
/// </para>
/// </param>
/// <param name="TiledResourcesTier">The value that indicates the tier support for tiled resources.</param>
/// <param name="Architecture">The architectural information about the adapter.</param>
/// <param name="GraphicsPreemptionGranularity">The graphics preemption granularity level at which the GPU can be preempted from performing its current graphics rendering task.</param>
/// <param name="ComputePreemptionGranularity">The compute preemption granularity level at which the GPU can be preempted from performing its current compute task.</param>
/// <remarks>
/// <para>
/// This information may be for a physical hardware device, or a software rasterizer. To determine which type this device falls under, se the <see cref="VideoDeviceType"/> property to determine the type of device
/// </para>
/// </remarks>
public record class GorgonVideoAdapterInfo(string Name, 
                                           int Index, 
                                           VideoDeviceType VideoDeviceType, 
                                           (int HighValue, uint LowValue) Luid, 
                                           GorgonVideoAdapterOutputList Outputs, 
                                           GorgonVideoAdapterMemory Memory,
                                           int MaxAddressBitsPerProcess,
                                           int MaxAddressBitsPerResource,
                                           GorgonVideoAdapterPciInfo PciInfo,
                                           bool AcgCompatible,
                                           bool SupportsMonitoredFences,
                                           bool SupportsNonMonitoredFences,
                                           bool SupportsKeyedMutexConformance,
                                           bool AllowTearing,
                                           bool HasGpuUploadSupport,
                                           ShaderModel ShaderModelSupport,
                                           bool HasTightAlignmentSupport,
                                           ResourceHeapTier ResourceHeapTier,
                                           TiledResourcesTier TiledResourcesTier,
                                           GorgonVideoAdapterArchitecture Architecture,
                                           GraphicsPreemptionGranularity GraphicsPreemptionGranularity,
                                           ComputePreemptionGranularity ComputePreemptionGranularity)
    : IGorgonNamedObject
{
    /// <summary>
    /// Property to return the maximum number of render targets allow to be assigned at the same time.
    /// </summary>
    public static int MaxRenderTargetCount => D3D12.D3D12_SIMULTANEOUS_RENDER_TARGET_COUNT;

    /// <summary>
    /// Property to return the maximum number of array indices for 1D and 2D textures.
    /// </summary>
    public static int MaxTextureArrayCount => D3D12.D3D12_REQ_TEXTURE2D_ARRAY_AXIS_DIMENSION;

    /// <summary>
    /// Property to return the maximum width of a 1D or 2D texture.
    /// </summary>
    public static int MaxTextureWidth => D3D12.D3D12_REQ_TEXTURE2D_U_OR_V_DIMENSION;

    /// <summary>
    /// Property to return the maximum height of a 2D texture.
    /// </summary>
    public static int MaxTextureHeight => D3D12.D3D12_REQ_TEXTURE2D_U_OR_V_DIMENSION;

    /// <summary>
    /// Property to return the maximum width of a 3D texture.
    /// </summary>
    public static int MaxTexture3DWidth => D3D12.D3D12_REQ_TEXTURE3D_U_V_OR_W_DIMENSION;

    /// <summary>
    /// Property to return the maximum height of a 3D texture.
    /// </summary>
    public static int MaxTexture3DHeight => D3D12.D3D12_REQ_TEXTURE3D_U_V_OR_W_DIMENSION;

    /// <summary>
    /// Property to return the maximum depth of a 3D texture.
    /// </summary>
    public static int MaxTexture3DDepth => D3D12.D3D12_REQ_TEXTURE3D_U_V_OR_W_DIMENSION;

    /// <summary>
    /// Property to return the maximum size, in bytes, for a constant buffer.
    /// </summary>
    public static int MaxConstantBufferSize => int.MaxValue;

    /// <summary>
    /// Property to return the maximum number of allowed scissor rectangles.
    /// </summary>
    public static int MaxScissorCount => D3D12.D3D12_VIEWPORT_AND_SCISSORRECT_MAX_INDEX + 1;

    /// <summary>
    /// Property to return the maximum number of allowed viewports.
    /// </summary>
    public static int MaxViewportCount => D3D12.D3D12_VIEWPORT_AND_SCISSORRECT_MAX_INDEX + 1;

    /// <summary>
    /// Property to return whether the GPU driver supports enhanced barriers (introduced in the 1.608.0 Agilty SDK), and on Windows 11 after release 22H2.
    /// </summary>
    public static bool HasEnhancedBarrierSupport => true;

    /// <summary>
    /// Function to enumerate the outputs for this video adapter.
    /// </summary>
    /// <param name="adapter">The adapter being evaluated.</param>
    /// <param name="d3dDevice">The Direct3D device for the adapter.</param>
    /// <returns>The list of outputs on the adapter.</returns>
    private static unsafe Dictionary<string, GorgonVideoOutputInfo> EnumerateOutputs(ComPtr<IDXGIAdapter4> adapter, ComPtr<ID3D12Device14> d3dDevice)
    {
        uint count = 0;
        Dictionary<string, GorgonVideoOutputInfo> outputs = new(StringComparer.OrdinalIgnoreCase);

        do
        {
            int currentIndex = (int)count;
            using ComPtr<IDXGIOutput> output0 = new();
            using ComPtr<IDXGIOutput6> output6 = new();

            HRESULT err = adapter.Get()->EnumOutputs(count++, output0.GetAddressOf());

            if (err.Value == DXGI.DXGI_ERROR_NOT_FOUND)
            {
                break;
            }
            else if (err.FAILED)
            {
                continue;
            }

            if (output0.As(&output6).FAILED)
            {
                continue;
            }

            GorgonVideoOutputInfo outputInfo = GorgonVideoOutputInfo.FromD3D(currentIndex, output6, d3dDevice);
            outputs[outputInfo.Name] = outputInfo;
        } while (true);

        return outputs;
    }

    /// <summary>
    /// Function to create a new <see cref="GorgonVideoAdapterInfo"/> record from the provided Direct 3D information.
    /// </summary>
    /// <param name="index">The index of the video adapter.</param>
    /// <param name="name">The name of the video adapter.</param>
    /// <param name="desc">The Direct 3D video adapter description.</param>
    /// <param name="adapter">The COM interface for the video adapter.</param>
    /// <param name="d3dDevice">The Direct 3D 12 device for the video adapter.</param>
    /// <param name="allowTearing"><b>true</b> if the device allows tearing when presenting, <b>false</b> if not.</param>
    /// <param name="shaderModel">The highest supported shader model for the adapter.</param>
    /// <param name="deviceType">The type of video adapter.</param>
    /// <exception cref="Win32Exception">Thrown if the adapter description would not be retrieved.</exception>
    /// <returns>A new <see cref="GorgonVideoAdapterInfo"/> filled with information about the video adapter.</returns>
    internal unsafe static GorgonVideoAdapterInfo FromD3D(int index, string name, ref readonly DXGI_ADAPTER_DESC3 desc, ComPtr<IDXGIAdapter4> adapter, ComPtr<ID3D12Device14> d3dDevice, bool allowTearing, ShaderModel shaderModel, VideoDeviceType deviceType)
    {
        LUID luid = d3dDevice.Get()->GetAdapterLuid();

        D3D12_FEATURE_DATA_D3D12_OPTIONS options = default;
        D3D12_FEATURE_DATA_ARCHITECTURE1 arch = default;
        D3D12_FEATURE_DATA_D3D12_OPTIONS16 options16 = default;
        D3D12_FEATURE_DATA_TIGHT_ALIGNMENT tightAlignment = default;
        D3D12_FEATURE_DATA_GPU_VIRTUAL_ADDRESS_SUPPORT addressSupport = default;

        d3dDevice.Get()->CheckFeatureSupport(D3D12_FEATURE.D3D12_FEATURE_D3D12_OPTIONS, &options, (uint)sizeof(D3D12_FEATURE_DATA_D3D12_OPTIONS))
            .ThrowIfFailed(GorgonResult.CannotEnumerate, () => string.Format(Resources.GORGFX_ERR_CANNOT_ENUMERATE_GPU, name));

        d3dDevice.Get()->CheckFeatureSupport(D3D12_FEATURE.D3D12_FEATURE_ARCHITECTURE1, &arch, (uint)sizeof(D3D12_FEATURE_DATA_ARCHITECTURE1))
            .ThrowIfFailed(GorgonResult.CannotEnumerate, () => string.Format(Resources.GORGFX_ERR_CANNOT_ENUMERATE_GPU, name));

        d3dDevice.Get()->CheckFeatureSupport(D3D12_FEATURE.D3D12_FEATURE_D3D12_OPTIONS16, &options16, (uint)sizeof(D3D12_FEATURE_DATA_D3D12_OPTIONS16))
            .ThrowIfFailed(GorgonResult.CannotEnumerate, () => string.Format(Resources.GORGFX_ERR_CANNOT_ENUMERATE_GPU, name));

        d3dDevice.Get()->CheckFeatureSupport(D3D12_FEATURE.D3D12_FEATURE_D3D12_TIGHT_ALIGNMENT, &tightAlignment, (uint)sizeof(D3D12_FEATURE_DATA_TIGHT_ALIGNMENT))
            .ThrowIfFailed(GorgonResult.CannotEnumerate, () => string.Format(Resources.GORGFX_ERR_CANNOT_ENUMERATE_GPU, name));

        d3dDevice.Get()->CheckFeatureSupport(D3D12_FEATURE.D3D12_FEATURE_GPU_VIRTUAL_ADDRESS_SUPPORT, &addressSupport, (uint)sizeof(D3D12_FEATURE_DATA_GPU_VIRTUAL_ADDRESS_SUPPORT))
            .ThrowIfFailed(GorgonResult.CannotEnumerate, () => string.Format(Resources.GORGFX_ERR_CANNOT_ENUMERATE_GPU, name));

        GorgonVideoAdapterOutputList outputs = new(EnumerateOutputs(adapter, d3dDevice));

        
        return new(string.IsNullOrWhiteSpace(name) ? $"{Resources.GORGFX_STR_ADAPTER} #{index}" : name, index, deviceType, (luid.HighPart, luid.LowPart), outputs,
            new GorgonVideoAdapterMemory((long)desc.DedicatedSystemMemory, (long)desc.SharedSystemMemory, (long)desc.DedicatedVideoMemory), 
            (int)addressSupport.MaxGPUVirtualAddressBitsPerProcess, (int)addressSupport.MaxGPUVirtualAddressBitsPerResource,
            new GorgonVideoAdapterPciInfo((int)desc.DeviceId, (int)desc.Revision, (int)desc.SubSysId, (int)desc.VendorId),
            (desc.Flags & DXGI_ADAPTER_FLAG3.DXGI_ADAPTER_FLAG3_ACG_COMPATIBLE) == DXGI_ADAPTER_FLAG3.DXGI_ADAPTER_FLAG3_ACG_COMPATIBLE,
            (desc.Flags & DXGI_ADAPTER_FLAG3.DXGI_ADAPTER_FLAG3_SUPPORT_MONITORED_FENCES) == DXGI_ADAPTER_FLAG3.DXGI_ADAPTER_FLAG3_SUPPORT_MONITORED_FENCES,
            (desc.Flags & DXGI_ADAPTER_FLAG3.DXGI_ADAPTER_FLAG3_SUPPORT_NON_MONITORED_FENCES) == DXGI_ADAPTER_FLAG3.DXGI_ADAPTER_FLAG3_SUPPORT_NON_MONITORED_FENCES,
            (desc.Flags & DXGI_ADAPTER_FLAG3.DXGI_ADAPTER_FLAG3_KEYED_MUTEX_CONFORMANCE) == DXGI_ADAPTER_FLAG3.DXGI_ADAPTER_FLAG3_KEYED_MUTEX_CONFORMANCE,
            allowTearing,
            options16.GPUUploadHeapSupported,
            shaderModel,
            tightAlignment.SupportTier != D3D12_TIGHT_ALIGNMENT_TIER.D3D12_TIGHT_ALIGNMENT_TIER_NOT_SUPPORTED,
            (ResourceHeapTier)options.ResourceHeapTier,
            (TiledResourcesTier)options.TiledResourcesTier,
            new GorgonVideoAdapterArchitecture((int)arch.NodeIndex, arch.TileBasedRenderer, arch.UMA, arch.CacheCoherentUMA, arch.IsolatedMMU),
            (GraphicsPreemptionGranularity)desc.GraphicsPreemptionGranularity,
            (ComputePreemptionGranularity)desc.ComputePreemptionGranularity);
    }
}
