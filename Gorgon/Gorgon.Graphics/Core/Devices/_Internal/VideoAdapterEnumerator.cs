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
// Created: Saturday, February 23, 2013 4:00:19 PM
// 

using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Functionality to retrieve information about the installed video adapters on the system
/// </summary>
internal unsafe class VideoAdapterEnumerator
{
    private static readonly Lock _lock = new();

    /// <summary>
    /// Function to determine if the adapter is valid for Gorgon or not.
    /// </summary>
    /// <param name="d3dDevice">The Direct3D device object to use.</param>
    /// <param name="name">The name of the video adapter.</param>
    /// <param name="log">The log for debug messages.</param>
    /// <param name="highestShaderModel">The highest shader model level for the adapter.</param>
    /// <returns><b>true</b> if the adapter is valid, <b>false</b> if not.</returns>
    private static bool ValidateAdapter(ComPtr<ID3D12Device14> d3dDevice, string name, IGorgonLog log, out ShaderModel highestShaderModel)
    {
        D3D12_FEATURE_DATA_SHADER_MODEL smData = new()
        {
            HighestShaderModel = D3D_SHADER_MODEL.D3D_SHADER_MODEL_6_8
        };

        HRESULT err = d3dDevice.Get()->CheckFeatureSupport(D3D12_FEATURE.D3D12_FEATURE_SHADER_MODEL, &smData, (uint)sizeof(D3D12_FEATURE_DATA_SHADER_MODEL));

        if (err.FAILED)
        {
            log.PrintError(err, $"There was an error retrieving the shader model information for the adapter '{name}'. This adapter will be skipped.", LoggingLevel.Verbose);
            highestShaderModel = ShaderModel.Unsupported;
            return false;
        }

        highestShaderModel = smData.HighestShaderModel.ToGorgonShaderModel();

        if (highestShaderModel == ShaderModel.Unsupported)
        {
            log.PrintWarning($"The shader model for the adapter '{name}' ({smData.HighestShaderModel}) is not supported. Shader model 6.6 or better is required. This adapter will be skipped.", LoggingLevel.Intermediate);
            return false;
        }

        D3D12_FEATURE_DATA_D3D12_OPTIONS options = default;

        err = d3dDevice.Get()->CheckFeatureSupport(D3D12_FEATURE.D3D12_FEATURE_D3D12_OPTIONS, &options, (uint)sizeof(D3D12_FEATURE_DATA_D3D12_OPTIONS));

        if (err.FAILED)
        {
            log.PrintError(err, $"There was an error retrieving the features for the adapter '{name}'. This adapter will be skipped.", LoggingLevel.Verbose);        
            return false;
        }

        if (options.ResourceBindingTier != D3D12_RESOURCE_BINDING_TIER.D3D12_RESOURCE_BINDING_TIER_3)
        {
            log.PrintWarning($"The resource binding tier for the adapter '{name}' is only {options.ResourceBindingTier}. Tier 3 resource binding support is required. This adapter will be skipped.", LoggingLevel.Intermediate);
            return false;
        }

        if (options.TiledResourcesTier == D3D12_TILED_RESOURCES_TIER.D3D12_TILED_RESOURCES_TIER_NOT_SUPPORTED)
        {
            log.PrintWarning($"There is no tiled resource tier support for the adpter '{name}'. Gorgon requires a minimum of Tier 1 for tiled resources. This adapter will be skipped.", LoggingLevel.Intermediate);
            return false;
        }

        D3D12_FEATURE_DATA_GPU_VIRTUAL_ADDRESS_SUPPORT addressSupport = default;

        err = d3dDevice.Get()->CheckFeatureSupport(D3D12_FEATURE.D3D12_FEATURE_GPU_VIRTUAL_ADDRESS_SUPPORT, &addressSupport, (uint)sizeof(D3D12_FEATURE_DATA_GPU_VIRTUAL_ADDRESS_SUPPORT));

        if (err.FAILED)
        {
            log.PrintError(err, $"There was an error retrieving the features for the adapter '{name}'. This adapter will be skipped.", LoggingLevel.Verbose);
            return false;
        }

        if (addressSupport.MaxGPUVirtualAddressBitsPerResource < 28)
        {
            log.PrintWarning(err, $"The adapter '{name}' only supports {addressSupport.MaxGPUVirtualAddressBitsPerResource} bits of address space. Gorgon requires at least 28 bits. This adapter will be skipped.", LoggingLevel.Intermediate);
            return false;
        }

        /* Intel Arc Axxx series only suppor Tier 1 (why the hell did they do this?? Previous gens support tier 2!)
        if (options.ResourceHeapTier != D3D12_RESOURCE_HEAP_TIER.D3D12_RESOURCE_HEAP_TIER_2)
        {
            log.PrintWarning($"The resource heap tier for the adapter '{name}' is only {options.ResourceHeapTier}. Tier 2 resource heap support is required. This adapter will be skipped.", LoggingLevel.Intermediate);
            return false;
        }
        */

        D3D12_FEATURE_DATA_D3D12_OPTIONS12 options12 = default;

        err = d3dDevice.Get()->CheckFeatureSupport(D3D12_FEATURE.D3D12_FEATURE_D3D12_OPTIONS12, &options12, (uint)sizeof(D3D12_FEATURE_DATA_D3D12_OPTIONS12));

        if (err.FAILED)
        {
            log.PrintError(err, $"There was an error retrieving the enhanced barrier support for the adapter '{name}'. This adapter will be skipped.", LoggingLevel.Verbose);        
            return false;
        }

        if (!options12.EnhancedBarriersSupported)
        {
            log.PrintWarning($"The adapter '{name}' does not support enhanced barriers. This adapter will be skipped.", LoggingLevel.Intermediate);
            return false;
        }

        D3D12_FEATURE_DATA_D3D12_OPTIONS19 options19 = default;

        err = d3dDevice.Get()->CheckFeatureSupport(D3D12_FEATURE.D3D12_FEATURE_D3D12_OPTIONS19, &options19, (uint)sizeof(D3D12_FEATURE_DATA_D3D12_OPTIONS19));

        if (err.FAILED)
        {
            log.PrintError(err, $"There was an error retrieving the depth stencil and rasterizer support for the adapter '{name}'. This adapter will be skipped.", LoggingLevel.Verbose);
            return false;
        }

        if (!options19.RasterizerDesc2Supported)
        {
            log.PrintWarning($"The adapter '{name}' does not support the most recent rasterizer state object. This means the drivers for the GPU are out of date. Please update to the latest drivers. This adapter will be skipped.", LoggingLevel.Intermediate);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Function to add the WARP software device.
    /// </summary>
    /// <param name="index">Index of the device.</param>
    /// <param name="factory">The factory used to query the adapter.</param>
    /// <param name="deviceFactory">The device factory used to create the D3D device.</param>
    /// <param name="allowTearing"><b>true</b> if the device allows tearing on present, <b>false</b> if not.</param>
    /// <param name="log">The log interface used to send messages to a debug log.</param>
    /// <returns>The video adapter used for WARP software rendering.</returns>
    private static GorgonVideoAdapterInfo? GetWARPSoftwareDevice(int index, ComPtr<IDXGIFactory7> factory, ComPtr<ID3D12DeviceFactory> deviceFactory, bool allowTearing, IGorgonLog log)
    {
        using ComPtr<IDXGIAdapter4> adapter = new();

        HRESULT err = factory.Get()->EnumWarpAdapter(Win32.__uuidof<IDXGIAdapter4>(), (void**)adapter.ReleaseAndGetAddressOf());

        if (err.FAILED)
        {
            log.PrintError(err, "Unable to enumerate the WARP adapter.", LoggingLevel.Simple);
            return null;
        }

        using ComPtr<ID3D12Device14> d3dDevice = CreateD3DDevice(adapter, deviceFactory, log);

        if (d3dDevice.IsNull)
        {
            log.PrintError("This WARP device does not support Direct3D 12 Tier 2. This is the minimum required version for Gorgon. Please use the latest WARP device from nuget.", LoggingLevel.Simple);
            return null;
        }

        DXGI_ADAPTER_DESC3 desc;

        err = adapter.Get()->GetDesc3(&desc);

        if (err.FAILED)
        {
            log.PrintError(err, "Could not retrieve the details for the WARP device. This adapter will not be available.", LoggingLevel.Simple);        
            return null;
        }

        if (!ValidateAdapter(d3dDevice, "WARP", log, out ShaderModel sm))
        {
            return null;
        }

        return GorgonVideoAdapterInfo.FromD3D(index, "WARP Software Adapter", in desc, adapter, d3dDevice, allowTearing, sm, VideoDeviceType.Software);
    }

    /// <summary>
    /// Function to print device log information.
    /// </summary>
    /// <param name="device">Device to print.</param>
    /// <param name="log">The log interface to output debug messages.</param>
    private static void PrintLog(GorgonVideoAdapterInfo device, IGorgonLog log)
    {
        log.Print($"Device found: {device.Name} ({device.VideoDeviceType})", LoggingLevel.Simple);
        log.Print("===================================================================", LoggingLevel.Simple);
        log.Print($"Video memory: {(device.Memory.Video).FormatMemory()}", LoggingLevel.Simple);
        log.Print($"System memory: {(device.Memory.System).FormatMemory()}", LoggingLevel.Intermediate);
        log.Print($"Shared memory: {(device.Memory.Shared).FormatMemory()}", LoggingLevel.Intermediate);
        log.Print($"Supports presentation tearing: {device.AllowTearing}", LoggingLevel.Simple);
        log.Print($"Device ID: 0x{device.PciInfo.DeviceID.FormatHex()}", LoggingLevel.Verbose);
        log.Print($"Sub-system ID: 0x{device.PciInfo.SubSystemID.FormatHex()}", LoggingLevel.Verbose);
        log.Print($"Vendor ID: 0x{device.PciInfo.VendorID.FormatHex()}", LoggingLevel.Verbose);
        log.Print($"Revision: {device.PciInfo.Revision}", LoggingLevel.Verbose);
        log.Print($"Unique ID: 0x{device.Luid.HighValue}, 0x{device.Luid.LowValue}", LoggingLevel.Verbose);
        log.Print($"Highest shader model supported: {device.ShaderModelSupport}", LoggingLevel.Verbose);
        log.Print($"Resource heap tier: {device.ResourceHeapTier}", LoggingLevel.Verbose);
        log.Print($"Tiled resources tier: {device.TiledResourcesTier}", LoggingLevel.Verbose);
        log.Print($"ACG Compatible: {device.AcgCompatible}", LoggingLevel.Verbose);
        log.Print($"Supports Alpha Factor blending: {device.SupportsAlphaBlendFactor}", LoggingLevel.Verbose);
        log.Print($"Supports GPU upload heaps: {device.HasGpuUploadSupport}", LoggingLevel.Verbose);
        log.Print($"Supports monitored fences: {device.SupportsMonitoredFences}", LoggingLevel.Verbose);
        log.Print($"Supports non-monitored fences: {device.SupportsMonitoredFences}", LoggingLevel.Verbose);
        log.Print($"Supports keyed mutex conformance: {device.SupportsKeyedMutexConformance}", LoggingLevel.Verbose);
        log.Print($"Graphics preemption granularity: {device.GraphicsPreemptionGranularity}", LoggingLevel.Verbose);
        log.Print($"Compute preemption granularity: {device.ComputePreemptionGranularity}", LoggingLevel.Verbose);
        log.Print("===================================================================", LoggingLevel.Simple);

        foreach (GorgonVideoOutputInfo output in device.Outputs)
        {
            log.Print($"Found output '{output.Name}'.", LoggingLevel.Simple);
            log.Print("===================================================================", LoggingLevel.Verbose);
            log.Print($"Output bounds: ({output.Bounds.Left}x{output.Bounds.Top})-({output.Bounds.Right}x{output.Bounds.Bottom})",
                       LoggingLevel.Verbose);
            log.Print($"Monitor handle: 0x{output.DeviceHandle.FormatHex()}", LoggingLevel.Verbose);
            log.Print($"Attached to desktop: {output.IsAttached}", LoggingLevel.Verbose);
            log.Print($"Monitor rotation: {output.Rotation}", LoggingLevel.Verbose);
            log.Print($"Bits per color channel: {output.BitsPerColorChannel}", LoggingLevel.Verbose);
            log.Print($"Color space: {output.ColorSpace}", LoggingLevel.Verbose);
            log.Print($"White point: {output.WhitePoint.X} x {output.WhitePoint.Y}", LoggingLevel.Verbose);
            log.Print($"Red primary: {output.RedPrimary.X} x {output.RedPrimary.Y}", LoggingLevel.Verbose);
            log.Print($"Green primary: {output.GreenPrimary.X} x {output.GreenPrimary.Y}", LoggingLevel.Verbose);
            log.Print($"Blue primary: {output.BluePrimary.X} x {output.BluePrimary.Y}", LoggingLevel.Verbose);
            log.Print($"Minimum luminance (nits): {output.MinimumLuminance}", LoggingLevel.Verbose);
            log.Print($"Maximum luminance (nits): {output.MaximumLuminance}", LoggingLevel.Verbose);
            log.Print($"Maximum full frame luminance (nits): {output.MaximumFullFrameLuminance}", LoggingLevel.Verbose);
            log.Print("===================================================================", LoggingLevel.Simple);
            log.Print($"Retrieving video modes for output '{output.Name}'...", LoggingLevel.Simple);
            log.Print($"Found {output.VideoModes.Count} video modes.", LoggingLevel.Simple);
            log.Print("===================================================================", LoggingLevel.Simple);
        }
    }

    /// <summary>
    /// Function to create a Direct3D 12 device with the best feature level.
    /// </summary>
    /// <param name="adapter">The adapter to create the device on.</param>
    /// <param name="deviceFactory">The device factory used to create the D3D device.</param>
    /// <param name="log">The log used for debug messages.</param>
    /// <returns>The D3D 12 device..</returns>
    private static ComPtr<ID3D12Device14> CreateD3DDevice(ComPtr<IDXGIAdapter4> adapter, ComPtr<ID3D12DeviceFactory> deviceFactory, IGorgonLog log)
    {
        ComPtr<ID3D12Device14> d3dDevice = default;
        Guid* riid = Win32.__uuidof<ID3D12Device14>();

        HRESULT err = deviceFactory.Get()->CreateDevice((PIDXGIAdapter4)adapter.Get(), D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_12_0, riid, (void**)d3dDevice.GetAddressOf());

        if (err.FAILED)
        {
            log.PrintError(err, "There was an error creating the Direct 3D 12 device object.", LoggingLevel.Simple);        
            return default;
        }

        D3D_FEATURE_LEVEL* featureLevels = stackalloc D3D_FEATURE_LEVEL[3]
        {
            D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_12_2,
            D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_12_1,
            D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_12_0,
        };
        D3D12_FEATURE_DATA_FEATURE_LEVELS levels = default;
        levels.NumFeatureLevels = 3;
        levels.pFeatureLevelsRequested = featureLevels;

        err = d3dDevice.Get()->CheckFeatureSupport(D3D12_FEATURE.D3D12_FEATURE_FEATURE_LEVELS, &levels, (uint)sizeof(D3D12_FEATURE_DATA_FEATURE_LEVELS));

        if (err.FAILED)
        {
            log.PrintError(err, "There was an error querying the Direct 3D 12 device object for feature level support.", LoggingLevel.Verbose);        
            return default;
        }

        if (levels.MaxSupportedFeatureLevel < D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_12_2)
        {
            log.PrintWarning("Device does not support feature level 12.2", LoggingLevel.Intermediate);
            return default;
        }

        // Recreate the device with the proper feature level so we can enumerate features correctly.
        err = deviceFactory.Get()->CreateDevice((PIDXGIAdapter4)adapter.Get(), levels.MaxSupportedFeatureLevel, riid, (void**)d3dDevice.ReleaseAndGetAddressOf());
        if (err.FAILED)
        {
            log.PrintError(err, $"There was an error creating the Direct 3D 12 device object for feature level {D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_12_2}.", LoggingLevel.Simple);        
            return default;
        }

        return d3dDevice;
    }

    /// <summary>
    /// Function to perform an enumeration of the video adapters attached to the system and populate this list.
    /// </summary>
    /// <param name="enumerateWARPDevice"><b>true</b> to enumerate the WARP software device, or <b>false</b> to exclude it.</param>
    /// <param name="deviceFactory">The device factory used to create the D3D device.</param>
    /// <param name="allowDebugging"><b>true</b> to create a debug DXGI factory, <b>false</b> to use the standard factory.</param>
    /// <param name="log">The log that will capture debug logging messages.</param>
    /// <param name="sortByPerformance"><b>true</b> to sort the resulting list by GPU performance, <b>false</b> to return the list in the order determined by the operating system.</param>
    /// <remarks>
    /// <para>
    /// Use this method to populate a list with information about the video adapters installed in the system.
    /// </para>
    /// <para>
    /// You may include the WARP device, which is a software based device that emulates most of the functionality of a video adapter, by setting the <paramref name="enumerateWARPDevice"/> to <b>true</b>.
    /// </para>
    /// <para>
    /// Gorgon requires a video adapter that is capable of supporting Direct 3D 12.0 at minimum. If no suitable devices are found installed in the computer, then the resulting list will be empty.
    /// </para>
    /// </remarks>
    public static IReadOnlyList<GorgonVideoAdapterInfo> Enumerate(bool enumerateWARPDevice, ComPtr<ID3D12DeviceFactory> deviceFactory, bool allowDebugging, IGorgonLog log, bool sortByPerformance)
    {
        int allowTearing = 0;
        uint index = 0;
        List<GorgonVideoAdapterInfo> devices = [];
        using ComPtr<IDXGIFactory7> factory = new();
        Guid adapterGuid = Win32.__uuidof<IDXGIAdapter1>();

        uint flags = allowDebugging ? DXGI.DXGI_CREATE_FACTORY_DEBUG : 0U;

        DirectX.CreateDXGIFactory2(flags, Win32.__uuidof<IDXGIFactory7>(), (void**)factory.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotEnumerate, () => Resources.GORGFX_ERR_CANNOT_CREATE_FACTORY);

        log.Print("Enumerating video adapters...", LoggingLevel.Simple);

        using ComPtr<IDXGIAdapter1> adapter1 = new();
        using ComPtr<IDXGIAdapter4> adapter4 = new();

        if (factory.Get()->CheckFeatureSupport(DXGI_FEATURE.DXGI_FEATURE_PRESENT_ALLOW_TEARING, &allowTearing, sizeof(int)).FAILED)
        {
            allowTearing = 0;
        }

        do
        {
            int currentIndex = (int)index;            

            HRESULT err = sortByPerformance ? factory.Get()->EnumAdapterByGpuPreference(index++, DXGI_GPU_PREFERENCE.DXGI_GPU_PREFERENCE_HIGH_PERFORMANCE, &adapterGuid, (void**)adapter1.ReleaseAndGetAddressOf()) 
                                            : factory.Get()->EnumAdapters1(index++, adapter1.ReleaseAndGetAddressOf());

            if (err.Value == DXGI.DXGI_ERROR_NOT_FOUND)
            {
                break;
            }

            if (err.FAILED)
            {
                log.PrintError(err, $"There was an error enumerating the video adapters.", LoggingLevel.Simple);
                continue;
            }

            err = adapter1.As(&adapter4);

            if (err.FAILED)                
            {
                log.PrintError(err, $"The adapter at index [{currentIndex}] does not support DXGI v1.6. This adapter will be skipped.", LoggingLevel.Simple);
                continue;
            }

            DXGI_ADAPTER_DESC3 desc;

            err = adapter4.Get()->GetDesc3(&desc);
            if (err.FAILED) 
            {
                log.PrintError(err, $"Could not retrieve the details for the adapter at index [{currentIndex}]. This adapter will be skipped.", LoggingLevel.Verbose);
                continue;
            }

            if (((desc.Flags & DXGI_ADAPTER_FLAG3.DXGI_ADAPTER_FLAG3_REMOTE) == DXGI_ADAPTER_FLAG3.DXGI_ADAPTER_FLAG3_REMOTE)
                || ((desc.Flags & DXGI_ADAPTER_FLAG3.DXGI_ADAPTER_FLAG3_SOFTWARE) == DXGI_ADAPTER_FLAG3.DXGI_ADAPTER_FLAG3_SOFTWARE))
            {
                continue;
            }

            string name = ((ReadOnlySpan<char>)desc.Description).Trim('\0').ToString();

            using ComPtr<ID3D12Device14> d3dDevice = CreateD3DDevice(adapter4, deviceFactory, log);

            if (d3dDevice.IsNull)
            {
                log.PrintWarning($"The adapter '{name}' (index: {currentIndex}) does not support Direct3D 12 Tier 2. This adapter will be skipped.", LoggingLevel.Intermediate);
                continue;
            }

            if (!ValidateAdapter(d3dDevice, name, log, out ShaderModel sm))
            {
                continue;
            }

            GorgonVideoAdapterInfo adapterInfo = GorgonVideoAdapterInfo.FromD3D(currentIndex,
                    name,
                    in desc,
                    adapter4,
                    d3dDevice,
                    allowTearing != 0,
                    sm,
                    VideoDeviceType.Hardware);
            devices.Add(adapterInfo);

            PrintLog(adapterInfo, log);
        } while (true);

        if (enumerateWARPDevice)
        {
            GorgonVideoAdapterInfo? warpDevice = GetWARPSoftwareDevice(devices.Count == 0 ? 1 : int.MaxValue, factory, deviceFactory, allowTearing != 0, log);

            if (warpDevice is not null)
            {
                PrintLog(warpDevice, log);
                devices.Add(warpDevice);
            }
        }

        log.Print($"Found {devices.Count} video adapters.", LoggingLevel.Simple);

        return devices;
    }
}
