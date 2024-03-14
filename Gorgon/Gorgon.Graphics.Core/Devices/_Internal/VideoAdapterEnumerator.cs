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
// Created: Saturday, February 23, 2013 4:00:19 PM
// 
#endregion

using Gorgon.Core;
using Gorgon.Diagnostics;
using SharpDX.DXGI;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Functionality to retrieve information about the installed video adapters on the system.
/// </summary>
internal class VideoAdapterEnumerator
{
    /// <summary>
    /// Function to add the WARP software device.
    /// </summary>
    /// <param name="index">Index of the device.</param>
    /// <param name="factory">The factory used to query the adapter.</param>
    /// <param name="log">The log interface used to send messages to a debug log.</param>
    /// <returns>The video adapter used for WARP software rendering.</returns>
    private static VideoAdapterInfo GetWARPSoftwareDevice(int index, Factory5 factory, IGorgonLog log)
    {
        D3D11.DeviceCreationFlags flags = D3D11.DeviceCreationFlags.None;

        if (GorgonGraphics.IsDebugEnabled)
        {
            flags = D3D11.DeviceCreationFlags.Debug;
        }

        using Adapter warp = factory.GetWarpAdapter();
        using Adapter4 warpAdapter4 = warp.QueryInterface<Adapter4>();
        using var D3DDevice = new D3D11.Device(warpAdapter4, flags);
        using D3D11.Device5 D3DDevice5 = D3DDevice.QueryInterface<D3D11.Device5>();
        FeatureSet? featureSet = GetFeatureLevel(D3DDevice5);

        if (featureSet is null)
        {
            log.Print("WARNING: The WARP software adapter does not support the minimum feature set of 12.0. This device will be excluded.", LoggingLevel.All);
            return null;
        }

        var result = new VideoAdapterInfo(index, warpAdapter4, featureSet.Value, [], VideoDeviceType.Software);

        PrintLog(result, log);

        return result;
    }

    /// <summary>
    /// Function to print device log information.
    /// </summary>
    /// <param name="device">Device to print.</param>
    /// <param name="log">The log interface to output debug messages.</param>
    private static void PrintLog(VideoAdapterInfo device, IGorgonLog log)
    {
        log.Print($"Device found: {device.Name}", LoggingLevel.Simple);
        log.Print("===================================================================", LoggingLevel.Simple);
        log.Print($"Supported feature set: {device.FeatureSet}", LoggingLevel.Simple);
        log.Print($"Video memory: {(device.Memory.Video).FormatMemory()}", LoggingLevel.Simple);
        log.Print($"System memory: {(device.Memory.System).FormatMemory()}", LoggingLevel.Intermediate);
        log.Print($"Shared memory: {(device.Memory.Shared).FormatMemory()}", LoggingLevel.Intermediate);
        log.Print($"Device ID: 0x{device.PciInfo.DeviceID.FormatHex()}", LoggingLevel.Verbose);
        log.Print($"Sub-system ID: 0x{device.PciInfo.SubSystemID.FormatHex()}", LoggingLevel.Verbose);
        log.Print($"Vendor ID: 0x{device.PciInfo.VendorID.FormatHex()}", LoggingLevel.Verbose);
        log.Print($"Revision: {device.PciInfo.Revision}", LoggingLevel.Verbose);
        log.Print($"Unique ID: 0x{device.Luid.FormatHex()}", LoggingLevel.Verbose);
        log.Print("===================================================================", LoggingLevel.Simple);

        foreach (IGorgonVideoOutputInfo output in device.Outputs)
        {
            log.Print($"Found output '{output.Name}'.", LoggingLevel.Simple);
            log.Print("===================================================================", LoggingLevel.Verbose);
            log.Print($"Output bounds: ({output.DesktopBounds.Left}x{output.DesktopBounds.Top})-({output.DesktopBounds.Right}x{output.DesktopBounds.Bottom})",
                       LoggingLevel.Verbose);
            log.Print($"Monitor handle: 0x{output.MonitorHandle.FormatHex()}", LoggingLevel.Verbose);
            log.Print($"Attached to desktop: {output.IsAttachedToDesktop}", LoggingLevel.Verbose);
            log.Print($"Monitor rotation: {output.Rotation}", LoggingLevel.Verbose);
            log.Print("===================================================================", LoggingLevel.Simple);

            log.Print($"Retrieving video modes for output '{output.Name}'...", LoggingLevel.Simple);
            log.Print("===================================================================", LoggingLevel.Simple);

            foreach (GorgonVideoMode mode in output.VideoModes)
            {
                log.Print($"{mode,70}\tScaling: {mode.Scaling,20}Scanline Order: {mode.ScanlineOrder,25}Stereo: {mode.SupportsStereo}",
                           LoggingLevel.Verbose);
            }

            log.Print("===================================================================", LoggingLevel.Verbose);
            log.Print($"Found {output.VideoModes.Count} video modes for output '{output.Name}'.", LoggingLevel.Simple);
            log.Print("===================================================================", LoggingLevel.Simple);
        }
    }

    /// <summary>
    /// Function to retrieve the video modes for an output.
    /// </summary>
    /// <param name="d3dDevice">D3D device for filtering supported display modes.</param>
    /// <param name="giOutput">Output that contains the video modes.</param>
    /// <returns>A list of display compatible full screen video modes.</returns>
    private static IEnumerable<ModeDescription1> GetVideoModes(D3D11.Device1 d3dDevice, Output1 giOutput)
    {
        Format[] formats = ((Format[])Enum.GetValues(typeof(Format)))
            .Where(item => (d3dDevice.CheckFormatSupport(item) & D3D11.FormatSupport.Display) == D3D11.FormatSupport.Display)
            .ToArray();

        IEnumerable<ModeDescription1> result = [];

        // Test each format for display compatibility.
        return formats.Aggregate(result,
                                 (current, format) =>
                                     current.Concat(giOutput.GetDisplayModeList1(format,
                                                                                 DisplayModeEnumerationFlags.Scaling |
                                                                                 DisplayModeEnumerationFlags.Stereo |
                                                                                 DisplayModeEnumerationFlags.DisabledStereo |
                                                                                 DisplayModeEnumerationFlags.Interlaced)
                                                            .Where(item => (d3dDevice.CheckFormatSupport(format) & D3D11.FormatSupport.Display) == D3D11.FormatSupport.Display)));
    }

    /// <summary>
    /// Function to retrieve the highest feature set for a video adapter.
    /// </summary>
    /// <param name="device">The D3D device to use.</param>
    /// <returns>The highest available feature set for the device.</returns>
    private static FeatureSet? GetFeatureLevel(D3D11.Device5 device)
    {
        D3D.FeatureLevel result = device.FeatureLevel;

        return ((Enum.IsDefined(typeof(D3D.FeatureLevel), (int)result))
            && (result >= D3D.FeatureLevel.Level_11_1))
            ? (FeatureSet?)result
            : null;
    }

    /// <summary>
    /// Function to retrieve the outputs attached to a video adapter.
    /// </summary>
    /// <param name="device">The Direct 3D device used to filter display modes.</param>
    /// <param name="adapter">The adapter to retrieve the outputs from.</param>
    /// <param name="outputCount">The number of outputs for the device.</param>
    /// <param name="log">The logging interface used to capture debug messages.</param>
    /// <returns>A list if video output info values.</returns>
    private static Dictionary<string, VideoOutputInfo> GetOutputs(D3D11.Device5 device, Adapter4 adapter, int outputCount, IGorgonLog log)
    {
        var result = new Dictionary<string, VideoOutputInfo>(StringComparer.OrdinalIgnoreCase);

        // Devices created under RDP/TS do not support output selection.
        if (SystemInformation.TerminalServerSession)
        {
            log.Print("Devices under terminal services and software devices devices do not use outputs, no outputs enumerated.", LoggingLevel.Intermediate);
            return result;
        }

        for (int i = 0; i < outputCount; ++i)
        {
            using Output output = adapter.GetOutput(i);
            using Output6 output6 = output.QueryInterface<Output6>();
            var outputInfo = new VideoOutputInfo(i, output6, GetVideoModes(device, output6));

            if (outputInfo.VideoModes.Count == 0)
            {
                log.Print($"Output '{output.Description.DeviceName}' on adapter '{adapter.Description1.Description}' has no full screen video modes.",
                            LoggingLevel.Intermediate);
            }

            result.Add(output.Description.DeviceName, outputInfo);
        }

        return result;
    }

    /// <summary>
    /// Function to perform an enumeration of the video adapters attached to the system and populate this list.
    /// </summary>
    /// <param name="enumerateWARPDevice"><b>true</b> to enumerate the WARP software device, or <b>false</b> to exclude it.</param>
    /// <param name="log">The log that will capture debug logging messages.</param>
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
    public static IReadOnlyList<IGorgonVideoAdapterInfo> Enumerate(bool enumerateWARPDevice, IGorgonLog log)
    {
        var devices = new List<IGorgonVideoAdapterInfo>();

        log ??= GorgonLog.NullLog;

        using (var factory2 = new Factory2(GorgonGraphics.IsDebugEnabled))
        using (Factory5 factory5 = factory2.QueryInterface<Factory5>())
        {
            int adapterCount = factory5.GetAdapterCount1();

            log.Print("Enumerating video adapters...", LoggingLevel.Simple);

            // Begin gathering device information.
            for (int i = 0; i < adapterCount; i++)
            {
                // Get the video adapter information.
                using Adapter1 adapter1 = factory5.GetAdapter1(i);
                using Adapter4 adapter = adapter1.QueryInterface<Adapter4>();
                // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
                if (((adapter.Desc3.Flags & AdapterFlags3.Remote) == AdapterFlags3.Remote)
                    || ((adapter.Desc3.Flags & AdapterFlags3.Software) == AdapterFlags3.Software))
                {
                    continue;
                }
                // ReSharper restore BitwiseOperatorOnEnumWithoutFlags

                D3D11.DeviceCreationFlags flags = D3D11.DeviceCreationFlags.None;

                if (GorgonGraphics.IsDebugEnabled)
                {
                    flags = D3D11.DeviceCreationFlags.Debug;
                }

                // We create a D3D device here to filter out unsupported video modes from the format list.
                using var D3DDevice = new D3D11.Device(adapter, flags, D3D.FeatureLevel.Level_12_1,
                                                                        D3D.FeatureLevel.Level_12_0,
                                                                        D3D.FeatureLevel.Level_11_1,
                                                                        D3D.FeatureLevel.Level_11_0,
                                                                        D3D.FeatureLevel.Level_10_1,
                                                                        D3D.FeatureLevel.Level_10_0,
                                                                        D3D.FeatureLevel.Level_9_3,
                                                                        D3D.FeatureLevel.Level_9_2,
                                                                        D3D.FeatureLevel.Level_9_1);
                using D3D11.Device5 D3DDevice5 = D3DDevice.QueryInterface<D3D11.Device5>();
                string adapterName = adapter.Description.Description.Replace("\0", string.Empty);
                D3DDevice5.DebugName = "Output enumerator device.";

                FeatureSet? featureSet = GetFeatureLevel(D3DDevice5);

                // Do not enumerate this device if its feature set is not supported.
                if (featureSet is null)
                {
                    log.Print($"WARNING: The video adapter '{adapterName}' (max. feature level [{D3DDevice5.FeatureLevel}]) is not supported by Gorgon and will be skipped.", LoggingLevel.Verbose);
                    continue;
                }

                Dictionary<string, VideoOutputInfo> outputs = GetOutputs(D3DDevice5, adapter, adapter.GetOutputCount(), log);

                if (outputs.Count <= 0)
                {
                    log.Print($"WARNING:  Video adapter '{adapterName}' has no outputs. Full screen mode will not be possible.", LoggingLevel.Verbose);
                }

                var videoAdapter = new VideoAdapterInfo(i, adapter, featureSet.Value, outputs, VideoDeviceType.Hardware);

                devices.Add(videoAdapter);
                PrintLog(videoAdapter, log);
            }

            // Get software devices.
            if (!enumerateWARPDevice)
            {
                return devices;
            }

            VideoAdapterInfo device = GetWARPSoftwareDevice(devices.Count, factory5, log);

            if (device is not null)
            {
                devices.Add(device);
            }
        }

        log.Print("Found {0} video adapters.", LoggingLevel.Simple, devices.Count);

        return devices;
    }
}
