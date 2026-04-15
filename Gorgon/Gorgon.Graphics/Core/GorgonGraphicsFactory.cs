// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: October 3, 2025 3:07:13 PM
//

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using DX = TerraFX.Interop.DirectX.DirectX;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Flags used to determine the level of debugging required.
/// </summary>
[Flags]
public enum GorgonGraphicsDebugFlags
{
    /// <summary>
    /// No debugging.
    /// </summary>
    None = 0,
    /// <summary>
    /// Enables tracking of objects that need to be released.
    /// </summary>
    ObjectTracking = 1,
    /// <summary>
    /// Enables synchronized command queue validation.
    /// </summary>
    SynchronizedCommandQueueValidation = 2,
    /// <summary>
    /// Enables GPU based validation. Warning, this has a large performance hit. Use with care.
    /// </summary>
    GpuBasedValidation = 4,
    /// <summary>
    /// Enables GPU based validation and state tracking validation. This implies <see cref="GpuBasedValidation"/>.
    /// </summary>
    GpuBasedStateTrackingValidation = 8
}

/// <summary>
/// A factory used to create <see cref="GorgonGraphics"/> objects.
/// </summary>
/// <remarks>
/// <para>
/// This factory is used to enumerate video devices, activate debug mode, build <see cref="GorgonGraphics"/> objects, and perform general setup for the 
/// <a href="https://devblogs.microsoft.com/directx/directx12agility/" target="_blank">Agility SDK</a>.
/// </para>
/// <para>
/// Unlike previous versions of Gorgon, applications no longer create the primary <see cref="GorgonGraphics"/> object by instancing the class. Instead this factory should be used to create the object. The 
/// reason for this change is to better handle the evolving distribution of Direct 3D 12 via the Agility SDK, and to load any required native DLLs. This will help keep Gorgon up to date with the latest 
/// version of Direct 3D 12 and its ancilliary functionality (e.g. WARP, Dxcompiler, etc...).
/// </para>
/// <para>
/// Objects created with this factory are tracked, and as such will be destroyed when the factory's <see cref="Dispose()"/> method is called. However, users should follow best practices and dispose of any 
/// object created manually when no longer required.
/// </para>
/// <para>
/// <h3>Debugging Support</h3>
/// </para>
/// <para>
/// Applications can enable Direct 3D debugging by passing a <see cref="GorgonGraphicsDebug"/> object to the factory constructor. This will allow developers to examine underlying failures when rendering 
/// using Direct 3D. This is helpful to determine if a <see cref="IDisposable.Dispose"/> call was forgotten by a developer.
/// </para>
/// <para>
/// In addtion to passing a <see cref="GorgonGraphicsDebug"/>, users must also use the DirectX control panel (<c>Debug -> Graphics -> DirectX Control Panel</c>) provided by Visual Studio in order to turn on 
/// debugging. Finally, the user must then turn on Native debugging in the Project properties of their application (under the <b>Debug</b> tab) so that any debug output can be seen in the Output window 
/// while running the application.
/// </para>
/// <para>
/// <h3>Requirements</h3>
/// To create a Gorgon Graphics object the following criteria must be met:
/// <list type="bullet">
/// <item><term>Operating System</term><description>Windows 10 version 10.0.19043.0 (21H1) or later version (Windows 11 24H2 or later version preferred).</description></item>
/// <item><term>Direct 3D Version</term><description>12, feature level 12_2.</description></item>
/// <item><term>Shader Model</term><description>6.6.</description></item>
/// <item><term>GPU Support</term><description>Enhanced barrier support is required.</description></item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphicsDebug"/>
/// <seealso cref="GorgonGraphics"/>
public unsafe sealed class GorgonGraphicsFactory
    : IDisposable
{
    private static ComPtr<ID3D12SDKConfiguration1> _sdkConfig;
    private static ComPtr<ID3D12DeviceFactory> _deviceFactory;
    private static ComPtr<IDXGIDebug> _dxgiDebug;
    private static ComPtr<ID3D12Debug6> _d3dDebug;
    private static ComPtr<ID3D12DeviceRemovedExtendedDataSettings2> _dred;

    private static int _debugCreateCounter;
    private static int _factoryCreateCounter;
    private static readonly Lock _objectCreationLock = new();
    private static readonly Lock _dllLoadLock = new();
    private static readonly Lock _debugCreationLock = new();
    private static readonly Lock _factoryCreationLock = new();
    private readonly IGorgonLog _log;
    private readonly List<GorgonGraphics> _graphicsObjects = [];

    /// <summary>
    /// Property to return the debugging flags specified on factory creation.
    /// </summary>
    public GorgonGraphicsDebugFlags DebuggingFlags
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the debugging interface.
    /// </summary>
    /// <remarks>
    /// If this value is <b>null</b>, then debugging is disabled.
    /// </remarks>
    public GorgonGraphicsDebug? Debugging
    {
        get;
        private set;
    }

    /// <summary>
    /// Function called to dispose of unmanaged and managed resources.
    /// </summary>
    /// <param name="disposing"><b>true</b> to dispose of unmanaged and managed resources, <b>false</b> to dispose of unmanaged resources only.</param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            using (_objectCreationLock.EnterScope())
            {
                if (_graphicsObjects.Count > 0)
                {
                    _log.Print("Destroying graphics interfaces...", LoggingLevel.Intermediate);

                    for (int i = _graphicsObjects.Count - 1; i >= 0; --i)
                    {
                        _graphicsObjects[i].Dispose();
                    }
                }
            }

            if (_debugCreateCounter == 1)
            {
                _log.Print($"[DEBUG] Destroying {nameof(IDXGIDebug)}, {nameof(ID3D12Debug6)} and {nameof(ID3D12DeviceRemovedExtendedData2)} interfaces...", LoggingLevel.Verbose);
            }

            if (_factoryCreateCounter == 1)
            {
                _log.Print($"Destroying {nameof(ID3D12DeviceFactory)}...", LoggingLevel.Verbose);
            }
        }

        Debugging = null;

        if ((!disposing) || (Interlocked.Decrement(ref _debugCreateCounter) <= 0))
        {
            _dred.Dispose();
            _d3dDebug.Dispose();
            _dxgiDebug.Dispose();
        }

        if ((!disposing) || (Interlocked.Decrement(ref _factoryCreateCounter) <= 0))
        {
            _deviceFactory.Dispose();
            _sdkConfig.Dispose();
        }
    }

    /// <summary>
    /// Function to create the Agility SDK device factory.
    /// </summary>
    private void CreateAgilityDeviceFactory()
    {
        using (_factoryCreationLock.EnterScope())
        {
            if ((_factoryCreateCounter++) > 0)
            {
                return;
            }

            ComPtr<ID3D12SDKConfiguration1> config = default;
            string path = Path.Combine("runtimes", Environment.Is64BitProcess ? "win-x64" : "win-x86", "native");
            nint pathPtr = Marshal.StringToHGlobalAnsi(path);

            try
            {
                DX.D3D12GetInterface((Guid*)Unsafe.AsPointer(ref Unsafe.AsRef(in CLSID.CLSID_D3D12SDKConfiguration)), Win32.__uuidof<ID3D12SDKConfiguration1>(), (void**)config.GetAddressOf())
                    .ThrowIfFailed(GorgonResult.CannotCreate, () => Resources.GORGFX_ERR_LOAD_AGILITY_FAILED);

                ComPtr<ID3D12DeviceFactory> factory = default;

                config.Get()->CreateDeviceFactory(CommonConstants.AgilitySDKVersion, (sbyte*)pathPtr, Win32.__uuidof<ID3D12DeviceFactory>(), (void**)factory.GetAddressOf())
                    .ThrowIfFailed(GorgonResult.CannotCreate, () => Resources.GORGFX_ERR_LOAD_AGILITY_FAILED);

                _sdkConfig = config;
                _deviceFactory = factory;
            }
            finally
            {
                Marshal.FreeHGlobal(pathPtr);
            }
        }
    }

    /// <summary>
    /// Function to create the debug interfaces.
    /// </summary>
    /// <param name="debug">The flags passed from the constructor.</param>
    private void CreateDebugInterface(GorgonGraphicsDebugFlags debug)
    {
        if (debug == GorgonGraphicsDebugFlags.None)
        {
            return;
        }

        using (_debugCreationLock.EnterScope())
        {
            if ((++_debugCreateCounter) > 1)
            {
                return;
            }

            debug |= GorgonGraphicsDebugFlags.ObjectTracking;

            _log.Print("[DEBUG] Enabling the Gorgon debug interface for DXGI and D3D12", LoggingLevel.Simple);

            if ((debug & GorgonGraphicsDebugFlags.GpuBasedStateTrackingValidation) == GorgonGraphicsDebugFlags.GpuBasedStateTrackingValidation)
            {
                _log.Print("[DEBUG] GPU based validation is enabled.", LoggingLevel.Intermediate);
                _log.Print("[DEBUG] GPU based validation resource state tracking is enabled.", LoggingLevel.Intermediate);

                debug |= GorgonGraphicsDebugFlags.GpuBasedValidation;
            }
            
            if ((debug & GorgonGraphicsDebugFlags.GpuBasedValidation) == GorgonGraphicsDebugFlags.GpuBasedValidation)
            {
                _log.Print("[DEBUG] GPU based validation is enabled.", LoggingLevel.Intermediate);
            }

            if ((debug & GorgonGraphicsDebugFlags.SynchronizedCommandQueueValidation) == GorgonGraphicsDebugFlags.SynchronizedCommandQueueValidation)
            {
                _log.Print("[DEBUG] Synchronized dependent command queue validation is enabled.", LoggingLevel.Intermediate);
            }

            _log.PrintWarning(DX.DXGIGetDebugInterface(Win32.__uuidof<IDXGIDebug>(), (void**)_dxgiDebug.GetAddressOf()),
                "Unable to retrieve the DXGI debug interface. Debug messaging will be limited.",
                LoggingLevel.Simple);

            HRESULT err = _deviceFactory.Get()->GetConfigurationInterface((Guid*)Unsafe.AsPointer(ref Unsafe.AsRef(in CLSID.CLSID_D3D12Debug)), Win32.__uuidof<ID3D12Debug6>(), (void**)_d3dDebug.GetAddressOf());

            if (err.FAILED)
            {
                _log.PrintWarning(err, "Unable to retrieve the D3D 12 debug interface. Debug messaging will be limited.", LoggingLevel.Simple);
            }
            else
            {
                _d3dDebug.Get()->EnableDebugLayer();
                _d3dDebug.Get()->SetEnableSynchronizedCommandQueueValidation((debug & GorgonGraphicsDebugFlags.SynchronizedCommandQueueValidation) == GorgonGraphicsDebugFlags.SynchronizedCommandQueueValidation);
                _d3dDebug.Get()->SetEnableGPUBasedValidation((debug & GorgonGraphicsDebugFlags.GpuBasedValidation) == GorgonGraphicsDebugFlags.GpuBasedValidation);
                _d3dDebug.Get()->SetGPUBasedValidationFlags(((debug & GorgonGraphicsDebugFlags.GpuBasedStateTrackingValidation) == GorgonGraphicsDebugFlags.GpuBasedStateTrackingValidation)
                    ? D3D12_GPU_BASED_VALIDATION_FLAGS.D3D12_GPU_BASED_VALIDATION_FLAGS_NONE
                    : D3D12_GPU_BASED_VALIDATION_FLAGS.D3D12_GPU_BASED_VALIDATION_FLAGS_DISABLE_STATE_TRACKING);
            }

            err = _deviceFactory.Get()->GetConfigurationInterface((Guid*)Unsafe.AsPointer(ref Unsafe.AsRef(in CLSID.CLSID_D3D12DeviceRemovedExtendedData)), Win32.__uuidof<ID3D12DeviceRemovedExtendedDataSettings2>(), (void**)_dred.GetAddressOf());

            if (err.FAILED)
            {
                _log.PrintWarning(err, "Unable to retrieve the D3D 12 DRED settings. Debug messaging will be limited.", LoggingLevel.Simple);
            }
            else
            {
                _dred.Get()->SetAutoBreadcrumbsEnablement(D3D12_DRED_ENABLEMENT.D3D12_DRED_ENABLEMENT_FORCED_ON);
                _dred.Get()->SetBreadcrumbContextEnablement(D3D12_DRED_ENABLEMENT.D3D12_DRED_ENABLEMENT_FORCED_ON);
                _dred.Get()->SetPageFaultEnablement(D3D12_DRED_ENABLEMENT.D3D12_DRED_ENABLEMENT_FORCED_ON);
            }

            DebuggingFlags = debug;
            Debugging = new GorgonGraphicsDebug(_log, _dxgiDebug, _dred);
        }
    }

    /// <summary>
    /// Function to create the COM interfaces used for the Direct 3D 12 device.
    /// </summary>
    /// <param name="adapter">The adapter used to render.</param>
    /// <returns>The pointers to the DXGI factory, and adapter, and the Direct3D 12 device object.</returns>
    private (ComPtr<IDXGIFactory7> dxgiFactory, ComPtr<IDXGIAdapter4> dxgiAdapter, ComPtr<ID3D12Device14> d3dDevice) CreateDeviceObjects(GorgonVideoAdapterInfo adapter)
    {
        Guid datGuid = DX.WKPDID_D3DDebugObjectNameW;
        ComPtr<IDXGIFactory7> factory7 = default;
        ComPtr<IDXGIAdapter4> adapter4 = default;
        ComPtr<ID3D12Device14> d3dDevice14 = default;
        string factoryName = "Gorgon DXGI Factory";
        string adapterName = $"Gorgon DXGI Adapter ({adapter.Name})";
        string d3dDeviceName = $"Gorgon D3D12 Device on {adapter.Name}";

        uint flags = Debugging is not null ? DXGI.DXGI_CREATE_FACTORY_DEBUG : 0U;

        _log.Print($"Creating {nameof(IDXGIFactory7)} '{factoryName}'.", LoggingLevel.Verbose);

        DX.CreateDXGIFactory2(flags, Win32.__uuidof<IDXGIFactory7>(), (void**)factory7.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotEnumerate, () => Resources.GORGFX_ERR_CANNOT_CREATE_FACTORY);

        factory7.SetDXGIDebugName(factoryName);

        _log.Print($"Creating {nameof(IDXGIAdapter4)} '{adapter.Name}'.", LoggingLevel.Verbose);

        if (adapter.VideoDeviceType == VideoDeviceType.Hardware)
        {
            using ComPtr<IDXGIAdapter1> adapter1 = new();

            factory7.Get()->EnumAdapters1((uint)adapter.Index, adapter1.ReleaseAndGetAddressOf())
                .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_ADAPTER, adapter.Index));

            adapter1.As(&adapter4)
                .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_ADAPTER, adapter.Index));
        }
        else
        {
            _log.PrintWarning($"NOTE: {adapter.Name} is a WARP Software adapter! WARP Software adapters are much slower than hardware devices, and should only be used for diagnostics or experimental code.", LoggingLevel.All);

            factory7.Get()->EnumWarpAdapter(Win32.__uuidof<IDXGIAdapter4>(), (void**)adapter4.GetAddressOf())
                .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_ADAPTER, "WARP"));
        }

        adapter4.SetDXGIDebugName(adapterName);

        _log.Print($"Creating {nameof(ID3D12Device14)} '{d3dDeviceName}' (Feature Level 12.2).", LoggingLevel.Verbose);

        _deviceFactory.Get()->CreateDevice((PIDXGIAdapter4)adapter4.Get(), D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_12_2, Win32.__uuidof<ID3D12Device14>(), (void**)d3dDevice14.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_D3D12_DEVICE, adapterName));
        d3dDevice14.SetD3DDebugName(d3dDeviceName);

        return (factory7, adapter4, d3dDevice14);
    }

    /// <summary>
    /// Function to unregister a graphics object instance.
    /// </summary>
    /// <param name="graphics">The graphics object instance to remove.</param>
    internal void Unregister(GorgonGraphics graphics)
    {
        using (_objectCreationLock.EnterScope())
        {
            _graphicsObjects.Remove(graphics);
        }
    }

    /// <summary>
    /// Function used to generate a name for an object.
    /// </summary>
    /// <param name="proposedName">The proposed name to apply to the object.</param>
    /// <param name="prefix">The prefix to for the name.</param>
    /// <returns>The name for the object.</returns>
    internal static string GenerateName(string proposedName, string prefix) => string.IsNullOrWhiteSpace(proposedName) ? $"{prefix} {Guid.NewGuid():N}" : proposedName;


    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// Function to retrieve information about the installed video adapters on the system.
    /// </summary>
    /// <param name="includeSoftwareDevice">[Optional] <b>true</b> to retrieve a software rendering device, or <b>false</b> to exclude it.</param>
    /// <returns>A list of installed adapters on the system.</returns>
    /// <remarks>
    /// <para>
    /// Use this to retrieve a list of video adapters available on the system. A video adapter may be a discrete video card, a device on the motherboard, or a software video adapter.
    /// </para>
    /// <para>
    /// This resulting list will contain <see cref="GorgonVideoAdapterInfo"/> objects which can then be passed to a <see cref="GorgonGraphics"/> instance. This allows applications or users to pick and choose which 
    /// adapter they wish to use for rendering.
    /// </para>
    /// <para>
    /// If the user specifies <b>true</b> for the <paramref name="includeSoftwareDevice"/> parameter, then the video adapter supplied will be much slower than an actual hardware video adapter. However, 
    /// this adapter can be helpful in debugging scenarios where issues with the hardware device driver may be causing incorrect rendering.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonVideoAdapterInfo"/>
    /// <seealso cref="GorgonGraphics"/>
    public IReadOnlyList<GorgonVideoAdapterInfo> EnumerateAdapters(bool includeSoftwareDevice = false)
    {
        IReadOnlyList<GorgonVideoAdapterInfo> result = VideoAdapterEnumerator.Enumerate(includeSoftwareDevice, _deviceFactory, _dxgiDebug.Get() is not null, _log, false);
        Debugging?.Report();

        return result;
    }

    /// <summary>
    /// Function to create a new <see cref="GorgonGraphics"/> object.
    /// </summary>
    /// <param name="adapter">[Optional] A <see cref="GorgonVideoAdapterInfo"/> to specify the video adapter to use with the resulting graphics object.</param>
    /// <param name="inFlightFrameCount">[Optional] The number of frames that can be in flight when the GPU is rendering.</param>
    /// <param name="vramReservedPercent">[Optional] The percentage of VRAM on the GPU to reserve for buffer storage.</param>
    /// <returns>A new graphics object.</returns>
    /// <exception cref="GorgonException">Thrown when an error occurs when trying to create the object.</exception>
    /// <remarks>
    /// <para>
    /// This function will create an instance of the root graphics object from which all other objects are connected with. Multiple objects may be created, this is useful in cases where the system has 
    /// multiple GPUs installed. 
    /// </para>
    /// <para>
    /// If the <paramref name="adapter"/> is not specified, then the system will enumerate the highest performing GPU as reported by the operating system and use that. If no applicable adapter can be found 
    /// then an exception will be thrown.
    /// </para>
    /// <para>
    /// Applications should call <see cref="EnumerateAdapters(bool)"/> prior to calling this method, that method will return a list of <see cref="GorgonVideoAdapterInfo"/> values that are passed to this 
    /// method. This allows an application to use a specific video adapter for rendering.
    /// </para>
    /// <para>
    /// When the factory has debugging turned on (<see cref="DebuggingFlags"/>), then the underlying API (Direct 3D 12/DXGI) will track and report on various objects that may be left alive if they were not 
    /// properly disposed. 
    /// </para>
    /// <para>
    /// The <paramref name="inFlightFrameCount"/> value is used to determine how many frames can be in flight at any given time while the GPU is working. The lower the value, the less latency introduced during 
    /// frame presentations to a <see cref="GorgonSwapChain"/>, but more frame spikes will be introduced. This value cannot be less than 1.
    /// </para>
    /// <para>
    /// If the <paramref name="vramReservedPercent"/> is specified, it allows users to specify how much VRAM can be used to store buffers for use as bindless data. The default value is 13%. The maximum 
    /// amount of VRAM that will be reserved is 2GB, and the minimum is 256 MB. This value is may change depending on the amount of VRAM available for the application.
    /// <note type="important">
    /// This feature is meant for advanced usage and should only be used if memory constraints cause an issue with buffer usage/storage. Do not change this without a very good reason for doing so.
    /// </note>
    /// </para>
    /// <para>
    /// <note type="information">
    /// <para>
    /// To create a Gorgon Graphics object the following criteria must be met:
    /// <list type="bullet">
    /// <item><term>Operating System</term><description>Windows 10 version 10.0.19043.0 (21H1) or later version (Windows 11 24H2 or later version preferred).</description></item>
    /// <item><term>Direct 3D Version</term><description>12, feature level 12_2.</description></item>
    /// <item><term>Shader Model</term><description>6.6.</description></item>
    /// <item><term>GPU Support</term><description>Enhanced barrier support is required.</description></item>
    /// </list>
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// The following example shows how to create the graphics object using this factory.
    /// </para>
    /// <code language="csharp">
    /// <![CDATA[
    /// GorgonGraphicsFactory factory = new(log: log);
    /// 
    /// // Create using a specific video adapter and use the highest feature set supported by that device:
    /// // Get a list of available video adapters.
    /// IReadOnlyList<IGorgonVideoAdapterInfo> videoAdapters = factory.EnumerateAdapters(false);
    /// 
    /// if (videoAdapters.Count == 0)
    ///   throw new Exception("No suitable video adapters found.");
    /// 
    /// GorgonGraphics graphics = factory.CreateGraphics(videoAdapters[0]);
    /// ]]>
    /// </code>
    /// </example>
    /// <seealso cref="GorgonGraphics"/>
    /// <seealso cref="GorgonVideoAdapterInfo"/>
    /// <seealso cref="EnumerateAdapters"/>
    /// <seealso cref="DebuggingFlags"/>
    public GorgonGraphics CreateGraphics(GorgonVideoAdapterInfo? adapter = null, int inFlightFrameCount = 2, int vramReservedPercent = 13)
    {
        if (adapter is null)
        {
            IReadOnlyList<GorgonVideoAdapterInfo> adapters = VideoAdapterEnumerator.Enumerate(false, _deviceFactory, Debugging is not null, _log, true);
            Debugging?.Report();

            if (adapters.Count == 0)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_NO_SUITABLE_ADAPTER);
            }
            
            adapter = adapters[0];            
        }

        (ComPtr<IDXGIFactory7> dxgiFactory, ComPtr<IDXGIAdapter4> dxgiAdapter, ComPtr<ID3D12Device14> d3dDevice) = CreateDeviceObjects(adapter);

        GorgonGraphics result = new(adapter, this, inFlightFrameCount.Max(1), vramReservedPercent, dxgiFactory, dxgiAdapter, d3dDevice, Debugging, _log);

        using (_objectCreationLock.EnterScope())
        {
            _graphicsObjects.Add(result);
        }

        return result;
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~GorgonGraphicsFactory() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGraphicsFactory"/> interface.
    /// </summary>
    /// <param name="debug">[Optional] Flags used to indicate the level of debugging.</param>
    /// <param name="log">[Optional] The log used for debug messaging.</param>
    /// <remarks>
    /// <para>
    /// When the <paramref name="debug"/> parameter is set to any value other than <see cref="GorgonGraphicsDebugFlags.None"/>, then debugging is turned on for the lifetime of the application and cannot be 
    /// disabled until the application exits. Also, when debugging is enabled a significant performance hit may be noticable when rendering.
    /// </para>
    /// </remarks>
    public GorgonGraphicsFactory(GorgonGraphicsDebugFlags debug = GorgonGraphicsDebugFlags.None, IGorgonLog? log = null)
    {
        _log = log ?? GorgonLog.NullLog;

        CreateAgilityDeviceFactory();
        CreateDebugInterface(debug);
    }

    /// <summary>
    /// Initializes the <see cref="GorgonGraphicsFactory"/> static class 
    /// </summary>
    static GorgonGraphicsFactory()
    {
        HMODULE warpDLL = NativeLoader.Load("d3d10warp", false);

        if ((warpDLL == HMODULE.NULL) || (warpDLL == HMODULE.INVALID_VALUE))
        {
            Debug.Print("WARP adapter DLL not found.  WARP adapter may not be available.");
        }

        try
        {
            NativeLoader.Load("dxcompiler", true);
        }
        catch (Exception e)
        {
            throw new GorgonException(GorgonResult.CannotInitialize, Resources.GORGFX_ERR_CANNOT_LOAD_DXCOMPILERDLL, e);
        }
    }
}
