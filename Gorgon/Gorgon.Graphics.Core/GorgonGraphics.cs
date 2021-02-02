#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: April 6, 2018 8:15:10 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Memory;
using Gorgon.Native;
using Gorgon.Timing;
using SharpDX.DXGI;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using DX = SharpDX;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// The primary object for the Gorgon Graphics system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is used to initialize the functionality available for rendering hardware accelerated graphics for applications. It is also used in the initialization of other objects used to create graphics. 
    /// </para>
    /// <para>
    /// Typically, a graphics object is assigned to a single <see cref="IGorgonVideoAdapterInfo"/> to provide access to the functionality of that video adapter. If the system has more than once video adapter 
    /// installed then access to subsequent devices can be given by creating a new instance of this object with the appropriate <see cref="IGorgonVideoAdapterInfo"/>.
    /// </para>
    /// <para>
    /// <note type="tip">
    /// <para>
    /// To determine what devices are attached to the system, use a <see cref="EnumerateAdapters"/> method to retreive a list of applicable video adapters. This will contain a list of 
    /// <see cref="IGorgonVideoAdapterInfo"/> objects suitable for construction of the graphics object.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// When creating a graphics object, the user can choose which feature set they will support for a given <see cref="IGorgonVideoAdapterInfo"/> so that older devices may be used. The actual feature set 
    /// support is provided by the <see cref="IGorgonVideoAdapterInfo.FeatureSet"/> on the <see cref="IGorgonVideoAdapterInfo"/> interface.
    /// </para>
    /// <para>
    /// This object is quite simple in its functionality. It provides some state assignment, and a means to submit a <see cref="GorgonDrawCallCommon">draw call</see> so that graphics information can be 
    /// rendered.
    /// </para>
    /// <para><h3>Rendering</h3></para>
    /// <para>
    /// Through the use of <see cref="GorgonDrawCallCommon">draw call types</see>, this object will send data in a stateless fashion. This differs from Direct 3D and other traditional APIs where states are 
    /// set until they're changed (stateful). The approach provided by this object avoids a common problem called state-leakage where a state may have been set prior to drawing, but was forgotten about. 
    /// This can lead to artifacts or can disable rendering entirely and consequently can be quite difficult to track. 
    /// </para>
    /// <para>
    /// When a draw call is sent, it carries all of the required state information (with the exception of resource types). This ensures that if a draw call doesn't need a state at a specific time, 
    /// it will be reset to a sensible default (as defined by the developer). 
    /// </para>
    /// <para>
    /// When drawing, Gorgon will determine the minimum required state to send with the final draw call, ensuring no redundant states are set. This type of rendering provides a performance gain since it will 
    /// only set the absolute minimum unique state it needs when the draw call is actually sent to the GPU. This means the user can set the state for a draw call as much as they want without that state being 
    /// sent to the GPU.
    /// </para>
    /// <para>
    /// <h3>Debugging Support</h3>
    /// </para>
    /// <para>
    /// Applications can enable Direct 3D debugging by setting to the <see cref="IsDebugEnabled"/> property to <b>true</b>. This will allow developers to examine underlying failures when rendering using 
    /// Direct 3D. Gorgon also provides memory tracking for any underlying Direct 3D objects when the <see cref="IsObjectTrackingEnabled"/> is set to <b>true</b>. This is useful if a 
    /// <see cref="IDisposable.Dispose"/> call was forgotten by the developer.
    /// </para>
    /// <para>
    /// However, it is not enough to just set these flags to <b>true</b> to enable debugging. Users must also use the DirectX control panel (<c>Debug -> Graphics -> DirectX Control Panel</c>) provided by 
    /// Visual Studio in order to turn on debugging. Finally, the user must then turn on Native debugging in the Project properties of their application (under the <b>Debug</b> tab) so that any debug 
    /// output can be seen in the Output window while running the application.
    /// </para>
    /// <para>
    /// If using a <b>DEBUG</b> compiled version of Gorgon (recommended for development), then the <see cref="IsDebugEnabled"/> property will automatically be set to <b>true</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonVideoAdapterInfo"/>
    /// <seealso cref="GorgonDrawCall"/>
    /// <seealso cref="GorgonDrawIndexCall"/>
    /// <seealso cref="GorgonInstancedCall"/>
    /// <seealso cref="GorgonInstancedIndexCall"/>
    public sealed class GorgonGraphics
        : IDisposable
    {
        #region Events.
        /// <summary>
        /// Event triggered before a render target is changed.
        /// </summary>
        public event CancelEventHandler RenderTargetChanging;

        /// <summary>
        /// Event triggered when the render target has been changed.
        /// </summary>
        public event EventHandler RenderTargetChanged;

        /// <summary>
        /// Event triggered before a viewport is changed.
        /// </summary>
        public event CancelEventHandler ViewportChanging;

        /// <summary>
        /// Event triggered when a viewport is changed.
        /// </summary>
        public event EventHandler ViewportChanged;

        /// <summary>
        /// Event triggered before a depth/stencil buffer is changed.
        /// </summary>
        public event CancelEventHandler DepthStencilChanging;

        /// <summary>
        /// Event triggered when the depth/stencil buffer has been changed.
        /// </summary>
        public event EventHandler DepthStencilChanged;
        #endregion

        #region Constants.
        /// <summary>
        /// The minimum build number required for the Windows 10 operating system.
        /// </summary>
        internal const int MinWin10Build = 15603;

        /// <summary>
        /// The name of the shader file data used for include files that wish to use the include shader.
        /// </summary>
        public const string BlitterShaderIncludeFileName = "__Gorgon_TextureBlitter_Shader__";
        #endregion

        #region Variables.

        // The D3D 11.x device context.
        private D3D11.DeviceContext4 _deviceContext;
        // The D3D 11.x device.
        private D3D11.Device5 _device;
        // The DXGI adapter.
        private Adapter4 _dxgiAdapter;
        // The DXGI factory
        private Factory5 _dxgiFactory;

        // A factory for creating temporary render targets.
        private RenderTargetFactory _rtvFactory;

        // The statistics for rendering.
        private GorgonGraphicsStatistics _stats;

        // The evaluator used to determine pipeline and resource state changes.
        private readonly StateEvaluator _stateEvaluator;
        // The applicator that will assign state and resource binding.
        private readonly D3D11StateApplicator _stateApplicator;

        // The cache used to hold pipeline states.
        private PipelineStateCache _pipelineStateCache;
        // The cache used to hold sampler states.
        private SamplerCache _samplerCache;

        // The timer used to trigger a clean up of cached render targets.
        private readonly GorgonTimerQpc _rtExpireTimer = new GorgonTimerQpc();
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the Direct 3D 11.x device context for this graphics instance.
        /// </summary>
        internal D3D11.DeviceContext4 D3DDeviceContext => _deviceContext;

        /// <summary>
        /// Property to return the Direct 3D 11.x device for this graphics instance.
        /// </summary>
        internal D3D11.Device5 D3DDevice => _device;

        /// <summary>
        /// Property to return the selected DXGI video adapter for this graphics instance.
        /// </summary>
        internal Adapter4 DXGIAdapter => _dxgiAdapter;

        /// <summary>
        /// Property to return the DXGI factory used to create DXGI objects.
        /// </summary>
        internal Factory5 DXGIFactory => _dxgiFactory;

        /// <summary>
        /// Property to return a read/write version of the statisics for Gorgon objects to update.
        /// </summary>
        internal ref GorgonGraphicsStatistics RwStatistics => ref _stats;

        /// <summary>
        /// Property to return the cache used to hold pipeline state objects.
        /// </summary>
        internal PipelineStateCache PipelineStateCache => _pipelineStateCache;

        /// <summary>
        /// Property to return the cache used to hold sampler state objects.
        /// </summary>
        internal SamplerCache SamplerStateCache => _samplerCache;

        /// <summary>
        /// Property to return the statistics generated while rendering.
        /// </summary>
        public ref readonly GorgonGraphicsStatistics Statistics => ref _stats;

        /// <summary>
        /// Property to return the logging interface used to write out debug messages.
        /// </summary>
        public IGorgonLog Log
        {
            get;
        }

        /// <summary>
        /// Property to return the current depth/stencil view.
        /// </summary>
        public GorgonDepthStencil2DView DepthStencilView => _stateEvaluator.DepthStencil;

        /// <summary>
        /// Property to return the currently assigned render targets.
        /// </summary>
        public IReadOnlyList<GorgonRenderTargetView> RenderTargets => _stateEvaluator.RenderTargets;

        /// <summary>
        /// Property to return the list of currently assigned viewports.
        /// </summary>
        public IReadOnlyList<DX.ViewportF> Viewports => _stateEvaluator.Viewports;

        /// <summary>
        /// Property to set or return the video adapter to use for this graphics interface.
        /// </summary>
        public IGorgonVideoAdapterInfo VideoAdapter
        {
            get;
        }

        /// <summary>
        /// Property to return the support available to each format.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will return the options available to a <see cref="BufferFormat"/>.
        /// </para>
        /// <para>
        /// The format support and compute shader/uav support value returned will be a bit mask of values from the <see cref="BufferFormatSupport"/> and the <see cref="ComputeShaderFormatSupport"/> 
        /// enumeration respectively.
        /// </para>
        /// </remarks>
        public IReadOnlyDictionary<BufferFormat, IGorgonFormatSupportInfo> FormatSupport
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether object tracking is disabled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will enable SharpDX's object tracking to ensure references are destroyed upon application exit.
        /// </para>
        /// <para>
        /// The default value for DEBUG mode is <b>true</b>, and for RELEASE it is set to <b>false</b>.  Disabling object tracking will give a slight performance increase.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// This flag <i>must</i> be set prior to creating any <see cref="GorgonGraphics"/> object, or else the flag will not take effect.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public static bool IsObjectTrackingEnabled
        {
            get => DX.Configuration.EnableObjectTracking;
            set => DX.Configuration.EnableObjectTracking = value;
        }

        /// <summary>
        /// Property to set or return whether debug output is enabled for the underlying graphics API.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will enable debug output for the underlying graphics API that Gorgon uses to render (Direct 3D 11 at this time). When this is enabled, all functionality will have debugging information that will 
        /// output to the debug output console (Output window in Visual Studio) if the <c>Debug -> Enable Native Debugging</c> is turned on in the application project settings <i>and</i> the DirectX control panel 
        /// is set up to debug the application under Direct 3D 10/11(/12 for Windows 10) application list.
        /// </para>
        /// <para>
        /// When Gorgon is compiled in DEBUG mode, this flag defaults to <b>true</b>, otherwise it defaults to <b>false</b>.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// This flag <i>must</i> be set prior to creating any <see cref="GorgonGraphics"/> object, or else the flag will not take effect.
        /// </para>
        /// <para>
        /// The D3D11 SDK Layers DLL must be installed in order for this flag to work. If it is not, then the application may crash.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public static bool IsDebugEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the actual supported <see cref="Core.FeatureSet"/> for this graphics instance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A user may request a lower <see cref="Core.FeatureSet"/> than what is supported by the device to allow the application to run on older video adapters that lack support for newer functionality. 
        /// This requested feature set will be returned by this property if supported by the device. 
        /// </para>
        /// <para>
        /// If the user does not request a feature set, or has specified one higher than what the video adapter supports, then the highest feature set supported by the video adapter 
        /// (indicated by the <see cref="IGorgonVideoAdapterInfo.FeatureSet"/> property in the <see cref="IGorgonVideoAdapterInfo"/> class) will be returned.
        /// </para>
        /// </remarks>
        /// <seealso cref="Core.FeatureSet"/>
        public FeatureSet FeatureSet
        {
            get;
        }

        /// <summary>
        /// Property to return a factory for creating/retrieving render targets for temporary use.
        /// </summary>
        /// <remarks>
        /// <para>
        /// During the lifecycle of an application, many render targets may be required. In most instances, creating a render target for a scene and disposing of it when done is all that is required. But, when 
        /// dealing with effects, shaders, etc... render targets may need to be created and released during a frame and doing this several times in a frame can be costly.
        /// </para>
        /// <para>
        /// This factory allows applications to "rent" a render target and return it when done with only an initial cost when creating a target for the first time. This way, temporary render targets can be reused 
        /// when needed.
        /// </para>
        /// <para>
        /// When the factory retrieves a target, it will check its internal pool and see if a render target already exists. If it exists, and is not in use, it will return the existing target. If the target 
        /// does not exist, or is being used elsewhere, then a new target is created and added to the pool.
        /// </para>
        /// <para>
        /// Targets retrieved by this factory must be returned when they are no longer needed, otherwise the purpose of the factory is defeated. 
        /// </para>
        /// <note type="warning">
        /// <para>
        /// This factory is <b>not</b> thread safe.
        /// </para>
        /// </note>
        /// </remarks>
        public IGorgonRenderTargetFactory TemporaryTargets => _rtvFactory;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the multi sample maximum quality level support for a given format.
        /// </summary>
        /// <param name="device">The D3D 11 device to use.</param>
        /// <param name="format">The DXGI format support to evaluate.</param>
        /// <returns>A <see cref="GorgonMultisampleInfo"/> value containing the max count and max quality level.</returns>
        private GorgonMultisampleInfo GetMultisampleSupport(D3D11.Device5 device, Format format)
        {
            try
            {
                for (int count = D3D11.Device.MultisampleCountMaximum; count >= 1; count /= 2)
                {
                    int quality = device.CheckMultisampleQualityLevels1(format, count, D3D11.CheckMultisampleQualityLevelsFlags.None);

                    if ((quality < 1) || (count == 1))
                    {
                        continue;
                    }

                    return new GorgonMultisampleInfo(count, quality - 1);
                }
            }
            catch (DX.SharpDXException sdEx)
            {
                Log.Print($"ERROR: Could not retrieve a multisample quality level max for format: [{format}]. Exception: {sdEx.Message}", LoggingLevel.Verbose);
            }

            return GorgonMultisampleInfo.NoMultiSampling;
        }

        /// <summary>
        /// Function to create the Direct 3D device and Adapter for use with Gorgon.
        /// </summary>
        /// <param name="adapterInfo">The adapter to use.</param>
        /// <param name="requestedFeatureLevel">The requested feature set for the device.</param>
        /// <returns>A tuple containing the Direct3D device object, DXGI factory, DXGI video adapter, and actual feature set.</returns>
        private (D3D11.Device5, Factory5, Adapter4) CreateDevice(IGorgonVideoAdapterInfo adapterInfo, D3D.FeatureLevel requestedFeatureLevel)
        {
            D3D11.DeviceCreationFlags flags = IsDebugEnabled ? D3D11.DeviceCreationFlags.Debug : D3D11.DeviceCreationFlags.None;
            Factory5 resultFactory;
            Adapter4 resultAdapter;
            D3D11.Device5 resultDevice;

            using (var factory2 = new Factory2(IsDebugEnabled))
            {
                resultFactory = factory2.QueryInterface<Factory5>();

                using (Adapter adapter = (adapterInfo.VideoDeviceType == VideoDeviceType.Hardware
                                              ? resultFactory.GetAdapter1(adapterInfo.Index)
                                              : resultFactory.GetWarpAdapter()))
                {
                    resultAdapter = adapter.QueryInterface<Adapter4>();

                    using (var device = new D3D11.Device(resultAdapter, flags, requestedFeatureLevel)
                    {
                        DebugName = $"'{adapterInfo.Name}' D3D {requestedFeatureLevel.D3DVersion()} {(adapterInfo.VideoDeviceType == VideoDeviceType.Software ? "Software Adapter" : "Adapter")}"
                    })
                    {
                        resultDevice = device.QueryInterface<D3D11.Device5>();

                        Log.Print($"Direct 3D {requestedFeatureLevel.D3DVersion()} device created for video adapter '{adapterInfo.Name}' at feature set [{(FeatureSet)resultDevice.FeatureLevel}]",
                                  LoggingLevel.Simple);
                    }
                }
            }

            return (resultDevice, resultFactory, resultAdapter);
        }

        /// <summary>
        /// Function to enumerate format support for all <see cref="BufferFormat"/> values.
        /// </summary>
        /// <param name="device">The D3D11 device object.</param>
        /// <returns>The list of format support information objects for each <see cref="BufferFormat"/>.</returns>
        private IReadOnlyDictionary<BufferFormat, IGorgonFormatSupportInfo> EnumerateFormatSupport(D3D11.Device5 device)
        {
            IEnumerable<BufferFormat> formats = (BufferFormat[])Enum.GetValues(typeof(BufferFormat));
            var result = new Dictionary<BufferFormat, IGorgonFormatSupportInfo>();

            // Get support values for each format.
            foreach (BufferFormat format in formats)
            {
                var dxgiFormat = (Format)format;

                // NOTE: NV12 seems to come back as value of -92093664, no idea what the extra flags might be, the documentation for D3D doesn't
                //       specify the flags.
                D3D11.FormatSupport formatSupport = device.CheckFormatSupport(dxgiFormat);
                D3D11.ComputeShaderFormatSupport computeSupport = device.CheckComputeShaderFormatSupport(dxgiFormat);

                GorgonMultisampleInfo msInfo = GorgonMultisampleInfo.NoMultiSampling;

                if (((formatSupport & D3D11.FormatSupport.MultisampleRenderTarget) == D3D11.FormatSupport.MultisampleRenderTarget)
                    || ((formatSupport & D3D11.FormatSupport.MultisampleLoad) == D3D11.FormatSupport.MultisampleLoad))
                {
                    msInfo = GetMultisampleSupport(device, dxgiFormat);
                }

                result[format] = new FormatSupportInfo(format, formatSupport, computeSupport, msInfo);
            }

            return result;
        }

        /// <summary>
        /// Function to fire the <see cref="ViewportChanging"/> event.
        /// </summary>
        /// <returns><b>true</b> to continue, <b>false</b> to cancel.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool OnViewportChanging()
        {
            CancelEventHandler cancelHandler = ViewportChanging;

            if (cancelHandler == null)
            {
                return true;
            }

            var cancelArgs = new CancelEventArgs();
            cancelHandler(this, cancelArgs);

            return !cancelArgs.Cancel;
        }

        /// <summary>
        /// Function to fire the <see cref="RenderTargetChanging"/> event.
        /// </summary>
        /// <param name="rtvsUpdated"><b>true</b> if the rtvs needed updating, <b>false</b> if not.</param>
        /// <returns><b>true</b> if the user will continue with the change, <b>false</b> if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool OnRenderTargetChanging(bool rtvsUpdated)
        {
            if (!rtvsUpdated)
            {
                return false;
            }

            CancelEventHandler cancelHandler = RenderTargetChanging;

            if (cancelHandler == null)
            {
                return rtvsUpdated;
            }

            var cancelArgs = new CancelEventArgs();
            cancelHandler(this, cancelArgs);

            return !cancelArgs.Cancel;
        }

        /// <summary>
        /// Function called when the render target(s) have changed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnRenderTargetChanged() => RenderTargetChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Function to fire the <see cref="DepthStencilChanging"/> event.
        /// </summary>
        /// <param name="dsvUpdated"><b>true</b> if the dsv needed updating, <b>false</b> if not.</param>
        /// <returns><b>true</b> if the user will continue with the change, <b>false</b> if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool OnDepthStencilChanging(bool dsvUpdated)
        {
            if (!dsvUpdated)
            {
                return false;
            }

            CancelEventHandler cancelHandler = DepthStencilChanging;

            if (cancelHandler == null)
            {
                return dsvUpdated;
            }

            var cancelArgs = new CancelEventArgs();
            cancelHandler(this, cancelArgs);

            return !cancelArgs.Cancel;
        }

        /// <summary>
        /// Function called when the depth/stencil has changed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnDepthStencilChanged() => DepthStencilChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Function to set up drawing states.
        /// </summary>
        /// <param name="state">The state to evaluate and apply.</param>
        /// <param name="factor">The current blend factor.</param>
        /// <param name="blendSampleMask">The blend sample mask.</param>
        /// <param name="depthStencilReference">The depth stencil reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetDrawStates(D3DState state, in GorgonColor factor, int blendSampleMask, int depthStencilReference)
        {
            // Before we draw, flush any expired render targets that are cached in the system.
            if ((_rtExpireTimer.Seconds > 30) && (_rtvFactory.AvailableCount > 0))
            {
                _rtvFactory.ExpireTargets();
                _rtExpireTimer.Reset();
            }

            PipelineStateChanges stateChanges = _stateEvaluator.GetPipelineStateChanges(state.PipelineState, in factor, blendSampleMask, depthStencilReference);
            _stateApplicator.ApplyPipelineState(state.PipelineState, stateChanges, in factor, blendSampleMask, depthStencilReference);

            ResourceRanges resourceChanges = _stateEvaluator.GetResourceStateChanges(state, stateChanges);
            _stateApplicator.BindResourceState(resourceChanges, state);
        }

        /// <summary>
        /// Function to check for the minimum windows 10 build that Gorgon Graphics supports.
        /// </summary>
        internal static void CheckMinimumOperatingSystem()
        {
            if (!Win32API.IsWindows10OrGreater(MinWin10Build))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_OS, MinWin10Build));
            }
        }

        /// <summary>
        /// Function to execute a dispatch call for a compute shader.
        /// </summary>
        /// <param name="dispatchCall">The call to execute.</param>
        /// <param name="threadGroupCountX">The number of thread groups to dispatch in the X direction.</param>
        /// <param name="threadGroupCountY">The number of thread groups to dispatch in the Y direction.</param>
        /// <param name="threadGroupCountZ">The number of thread groups to dispatch in the Z direction.</param>
        internal void Dispatch(GorgonDispatchCall dispatchCall, int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
        {
            dispatchCall.ValidateObject(nameof(dispatchCall));
            threadGroupCountX.ValidateRange(nameof(threadGroupCountX), 0, GorgonComputeEngine.MaxThreadGroupCount);
            threadGroupCountY.ValidateRange(nameof(threadGroupCountY), 0, GorgonComputeEngine.MaxThreadGroupCount);
            threadGroupCountZ.ValidateRange(nameof(threadGroupCountZ), 0, GorgonComputeEngine.MaxThreadGroupCount);

            SetDrawStates(dispatchCall.D3DState, GorgonColor.White, int.MinValue, 0);
            D3DDeviceContext.Dispatch(threadGroupCountX, threadGroupCountY, threadGroupCountZ);
        }

        /// <summary>
        /// Function to execute a dispatch call for a compute shader.
        /// </summary>
        /// <param name="dispatchCall">The call to execute.</param>
        /// <param name="indirectArgs">The buffer containing the arguments for the compute shader.</param>
        /// <param name="threadGroupOffset">[Optional] The offset within the buffer, in bytes, to where the arguments are stored.</param>
        internal void Dispatch(GorgonDispatchCall dispatchCall, GorgonBufferCommon indirectArgs, int threadGroupOffset = 0)
        {
            dispatchCall.ValidateObject(nameof(dispatchCall));
            indirectArgs.ValidateObject(nameof(indirectArgs));
            threadGroupOffset.ValidateRange(nameof(threadGroupOffset), 0, int.MaxValue);

            SetDrawStates(dispatchCall.D3DState, GorgonColor.White, int.MinValue, 0);
            D3DDeviceContext.DispatchIndirect(indirectArgs.Native, threadGroupOffset);
        }

        /// <summary>
        /// Function to clear the states for the graphics object.
        /// </summary>
        /// <param name="flush">[Optional] <b>true</b> to flush the queued graphics object commands, <b>false</b> to leave as is.</param>
        /// <remarks>
        /// <para>
        /// This method will reset all current states to an uninitialized state.
        /// </para>
        /// <para>
        /// If the <paramref name="flush"/> parameter is set to <b>true</b>, then any commands on the GPU that are pending will be flushed.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// This method will cause a significant performance hit if the <paramref name="flush"/> parameter is set to <b>true</b>, so its use is generally discouraged in performance sensitive situations.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearState(bool flush = false)
        {
            // Reset state on the device context.
            D3DDeviceContext.ClearState();

            if (flush)
            {
                D3DDeviceContext.Flush();
            }

            _stateEvaluator.ResetState();
        }

        /// <summary>
        /// Function to flush the rendering commands.
        /// </summary>
        public void Flush() => D3DDeviceContext.Flush();

        /// <summary>
        /// Function to assign a depth/stencil view.
        /// </summary>
        /// <param name="depthStencil">The depth/stencil to assign.</param>
        /// <remarks>
        /// <para>
        /// This depth/stencil have the same dimensions, array size, and multisample values as the currently assigned <see cref="RenderTargets"/>. 
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// When changing a depth/stencil, the state of the GPU is reset and may impact performance. This is done to avoid resource hazards (e.g. depth/stencil is set as a shader resource). 
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonDepthStencil2DView"/>
        /// <seealso cref="GorgonTexture2D"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetDepthStencil(GorgonDepthStencil2DView depthStencil)
        {
            (bool _, bool DsvChanged) changes = _stateEvaluator.GetRtvDsvChanges(_stateEvaluator.RenderTargets, depthStencil);

            if ((!changes.DsvChanged) || (!OnDepthStencilChanging(changes.DsvChanged)))
            {
                return;
            }

            _stateApplicator.BindRenderTargets(_stateEvaluator.RenderTargets, depthStencil, in changes);

            OnDepthStencilChanged();
        }

        /// <summary>
        /// Function to assign a single render target to the first slot.
        /// </summary>
        /// <param name="renderTarget">The render target view to assign.</param>
        /// <param name="depthStencil">[Optional] The depth/stencil to assign with the render target.</param>
        /// <remarks>
        /// <para>
        /// This will assign a render target in slot 0 of the <see cref="RenderTargets"/> list. All other render targets bound in other slots will be unbound. If multiple render targets need to be set,
        /// then call the <see cref="SetRenderTargets"/> method.
        /// </para>
        /// <para>
        /// If the <paramref name="depthStencil"/> parameter is used, then a <see cref="GorgonDepthStencil2DView"/> is assigned in conjunction with the render target. This depth/stencil have the same 
        /// dimensions, array size, and multisample values as the render target. When specifying a depth/stencil, the render targets must be a <see cref="GorgonTexture2D"/>.
        /// </para>
        /// <para>
        /// When a render target is set, the first viewport in the <see cref="Viewports"/> list will be reset to the size of the render target. The user is responsible for restoring these to their intended
        /// values after assigning the target if a different viewport region is required.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// When changing a render target or depth/stencil, the state of the GPU is reset and may impact performance. This is done to avoid resource hazards (e.g. target is set as a shader resource). 
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonDepthStencil2DView"/>
        /// <seealso cref="GorgonTexture2D"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRenderTarget(GorgonRenderTargetView renderTarget, GorgonDepthStencil2DView depthStencil = null)
        {
            GorgonRenderTargetView[] renderTargets = GorgonArrayPool<GorgonRenderTargetView>.SharedTiny.Rent(1);

            try
            {
                renderTargets[0] = renderTarget;
                SetRenderTargets(renderTargets.AsSpan(0, 1), depthStencil);
            }
            finally
            {
                GorgonArrayPool<GorgonRenderTargetView>.SharedTiny.Return(renderTargets);
            }
        }

        /// <summary>
        /// Function to assign multiple render targets to the first slot and a custom depth/stencil view.
        /// </summary>
        /// <param name="renderTargets">The list of render target views to assign.</param>
        /// <param name="depthStencil">The depth/stencil view to assign.</param>
        /// <remarks>
        /// <para>
        /// This will assign multiple render targets to the corresponding slots in the <see cref="RenderTargets"/> list. 
        /// </para>
        /// <para>
        /// If the <paramref name="depthStencil"/> parameter is used, then a <see cref="GorgonDepthStencil2DView"/> is assigned in conjunction with the render target. This depth/stencil have the same 
        /// dimensions, array size, and multisample values as the render target. When specifying a depth/stencil, the render targets must be a <see cref="GorgonTexture2D"/>.
        /// </para>
        /// <para>
        /// If the <see cref="GorgonRenderTargetView">GorgonRenderTargetViews</see> are attached to resources with multisampling enabled through <see cref="GorgonMultisampleInfo"/>, then the 
        /// <see cref="GorgonMultisampleInfo"/> of the resource attached to the <see cref="GorgonDepthStencil2DView"/> being assigned must match, or an exception will be thrown.
        /// </para>
        /// <para>
        /// The format for the <paramref name="renderTargets"/> and <paramref name="depthStencil"/> may differ from the formats of other views passed in.
        /// </para>
        /// <para>
        /// When a render target is set, the first viewport in the <see cref="Viewports"/> list will be reset to the size of the render target. The user is responsible for restoring these to their intended
        /// values after assigning the target if a different viewport region is required.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// When changing a render target or depth/stencil, the state of the GPU is reset and may impact performance. This is done to avoid resource hazards (e.g. target is set as a shader resource). 
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// <note type="information">
        /// <para>
        /// The exceptions raised when validating a view against other views in this list are only thrown when Gorgon is compiled as <b>DEBUG</b>.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonDepthStencil2DView"/>
        /// <seealso cref="GorgonTexture2D"/>
        public void SetRenderTargets(ReadOnlySpan<GorgonRenderTargetView> renderTargets, GorgonDepthStencil2DView depthStencil = null)
        {
            (bool RtvsUpdated, bool DsvUpdated) updates = _stateEvaluator.GetRtvDsvChanges(renderTargets, depthStencil);

            if ((!updates.RtvsUpdated) && (!updates.DsvUpdated))
            {
                return;
            }

            updates = (OnRenderTargetChanging(updates.RtvsUpdated), updates.DsvUpdated);
            updates = (updates.RtvsUpdated, OnDepthStencilChanging(updates.DsvUpdated));

            if ((!updates.RtvsUpdated) && (!updates.DsvUpdated))
            {
                return;
            }

            _stateApplicator.BindRenderTargets(renderTargets, depthStencil, in updates);

            if (updates.DsvUpdated)
            {
                OnDepthStencilChanged();
            }

            if (!updates.RtvsUpdated)
            {
                return;
            }

            OnRenderTargetChanged();
        }

        /// <summary>
        /// Function to set a scissor rectangle for defining a clipping area on the current render target.
        /// </summary>
        /// <param name="rect">The region, in screen coordinates, to clip.</param>
        /// <remarks>
        /// <para>
        /// These are used for defining a clipping area on the current render target, pixels rendered outside of the area defined will not appear in the render target.
        /// </para>
        /// <para>
        /// The rasterizer state does not have <see cref="GorgonRasterState.ScissorRectsEnabled"/> set to <b>true</b>, this method will have no effect. If a scissor rectangle is assigned while a raster 
        /// state with <see cref="GorgonRasterState.ScissorRectsEnabled"/> is set to <b>false</b>, it will remain assigned and will be applied if a raster state with 
        /// <see cref="GorgonRasterState.ScissorRectsEnabled"/> is set to <b>true</b>.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonRasterState"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetScissorRect(DX.Rectangle rect)
        {
            DX.Rectangle[] rects = GorgonArrayPool<DX.Rectangle>.SharedTiny.Rent(1);

            try
            {
                rects[0] = rect;
                SetScissorRects(rects.AsSpan(0, 1));
            }
            finally
            {
                GorgonArrayPool<DX.Rectangle>.SharedTiny.Return(rects);
            }
        }

        /// <summary>
        /// Function to set multple scissor rectangles for defining a clipping area on the current render target.
        /// </summary>
        /// <param name="rects">The regions, in screen coordinates, to clip.</param>
        /// <remarks>
        /// <para>
        /// These are used for defining clipping areas on the current render target, pixels rendered outside of the areas defined will not appear in the render target.
        /// </para>
        /// <para>
        /// This Which scissor rectangle to use is determined by the <b>SV_ViewportArrayIndex</b> semantic output by a geometry shader (see shader semantic syntax). If a geometry shader does not make use 
        /// of the <b>SV_ViewportArrayIndex</b> semantic then the first scissor rectangle in the array will be used.
        /// </para>
        /// <para>
        /// The rasterizer state does not have <see cref="GorgonRasterState.ScissorRectsEnabled"/> set to <b>true</b>, this method will have no effect. If a scissor rectangle is assigned while a raster 
        /// state with <see cref="GorgonRasterState.ScissorRectsEnabled"/> is set to <b>false</b>, it will remain assigned and will be applied if a raster state with 
        /// <see cref="GorgonRasterState.ScissorRectsEnabled"/> is set to <b>true</b>.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonRasterState"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetScissorRects(ReadOnlySpan<DX.Rectangle> rects)
        {
            if (!_stateEvaluator.GetScissorRectChange(rects))
            {
                return;
            }

            _stateApplicator.BindScissorRectangles(rects);
        }

        /// <summary>
        /// Function to set a viewport to define the area to render on the <see cref="RenderTargets"/>.
        /// </summary>
        /// <param name="viewport">The viewport to assign.</param>
        /// <remarks>
        /// <para>
        /// This will define the area to render into on the current <see cref="RenderTargets"/>. This method will set the first viewport at index 0 only, any other viewports assigned will be unassigned.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetViewport(DX.ViewportF viewport)
        {
            DX.ViewportF[] viewports = GorgonArrayPool<DX.ViewportF>.SharedTiny.Rent(1);

            try
            {
                viewports[0] = viewport;
                SetViewports(viewports.AsSpan(0, 1));
            }
            finally
            {
                GorgonArrayPool<DX.ViewportF>.SharedTiny.Return(viewports);
            }
        }

        /// <summary>
        /// Function to set multiple viewports to define the area to render on the <see cref="RenderTargets"/>.
        /// </summary>
        /// <param name="viewports">The viewports to assign.</param>
        /// <remarks>
        /// <para>
        /// This will define the area to render into on the current <see cref="RenderTargets"/>. This method will set the first viewport at index 0 only, any other viewports assigned will be unassigned.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetViewports(ReadOnlySpan<DX.ViewportF> viewports)
        {
            if (!_stateEvaluator.GetViewportChange(viewports))
            {
                return;
            }

            if (!OnViewportChanging())
            {
                return;
            }

            _stateApplicator.BindViewports(viewports);

            EventHandler handler = ViewportChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function to retrieve information about the installed video adapters on the system.
        /// </summary>
        /// <param name="includeSoftwareDevice">[Optional] <b>true</b> to retrieve a software rendering device, or <b>false</b> to exclude it.</param>
        /// <param name="log">[Optional] The logging interface used to capture debug messages.</param>
        /// <returns>A list of installed adapters on the system.</returns>
        /// <remarks>
        /// <para>
        /// Use this to retrieve a list of video adapters available on the system. A video adapter may be a discreet video card, a device on the motherboard, or a software video adapter.
        /// </para>
        /// <para>
        /// This resulting list will contain <see cref="IGorgonVideoAdapterInfo"/> objects which can then be passed to a <see cref="GorgonGraphics"/> instance. This allows applications or users to pick and choose which 
        /// adapter they wish to use for rendering.
        /// </para>
        /// <para>
        /// If the user specifies <b>true</b> for the <paramref name="includeSoftwareDevice"/> parameter, then the video adapter supplied will be much slower than an actual hardware video adapter. However, 
        /// this adapter can be helpful in debugging scenarios where issues with the hardware device driver may be causing incorrect rendering.
        /// </para>
        /// </remarks>
        public static IReadOnlyList<IGorgonVideoAdapterInfo> EnumerateAdapters(bool includeSoftwareDevice = false, IGorgonLog log = null) =>
            VideoAdapterEnumerator.Enumerate(includeSoftwareDevice, log);

        /// <summary>
        /// Function to clear the cached pipeline states and sampler states
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will destroy any previously cached pipeline states and sampler states.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearStateCache()
        {
            if (D3DDeviceContext != null)
            {
                ClearState();
            }

            _samplerCache.Clear();
            _samplerCache.WarmCache();
            PipelineStateCache.Clear();

            ClearState();
        }

        /// <summary>
        /// Function to submit a basic draw call to the GPU.
        /// </summary>
        /// <param name="drawCall">The draw call to execute.</param>
        /// <param name="blendFactor">[Optional] The factor used to modulate the pixel shader, render target or both.</param>
        /// <param name="blendSampleMask">[Optional] The mask used to define which samples get updated in the active render targets.</param>
        /// <param name="depthStencilReference">[Optional] The depth/stencil reference value used when performing a depth/stencil test.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/> parameter is <b>null</b>.</exception>
        public void Submit(GorgonDrawCall drawCall, in GorgonColor? blendFactor = null, int blendSampleMask = int.MinValue, int depthStencilReference = 0)
        {
            drawCall.ValidateObject(nameof(drawCall));
            SetDrawStates(drawCall.D3DState, blendFactor ?? GorgonColor.White, blendSampleMask, depthStencilReference);
            D3DDeviceContext.Draw(drawCall.VertexCount, drawCall.VertexStartIndex);
            unchecked
            {
                ++_stats._drawCallCount;
                _stats._triangleCount += drawCall.VertexCount / 3;
            }
        }

        /// <summary>
        /// Function to submit a basic, instanced, draw call to the GPU.
        /// </summary>
        /// <param name="drawCall">The draw call to execute.</param>
        /// <param name="blendFactor">[Optional] The factor used to modulate the pixel shader, render target or both.</param>
        /// <param name="blendSampleMask">[Optional] The mask used to define which samples get updated in the active render targets.</param>
        /// <param name="depthStencilReference">[Optional] The depth/stencil reference value used when performing a depth/stencil test.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/> parameter is <b>null</b>.</exception>
        public void Submit(GorgonInstancedCall drawCall, in GorgonColor? blendFactor = null, int blendSampleMask = int.MinValue, int depthStencilReference = 0)
        {
            drawCall.ValidateObject(nameof(drawCall));
            SetDrawStates(drawCall.D3DState, blendFactor ?? GorgonColor.White, blendSampleMask, depthStencilReference);
            D3DDeviceContext.DrawInstanced(drawCall.VertexCountPerInstance, drawCall.InstanceCount, drawCall.VertexStartIndex, drawCall.StartInstanceIndex);
            unchecked
            {
                ++_stats._drawCallCount;
                _stats._triangleCount += (drawCall.VertexCountPerInstance * drawCall.InstanceCount) / 3;
            }
        }

        /// <summary>
        /// Function to submit a draw call with indices to the GPU.
        /// </summary>
        /// <param name="drawIndexCall">The draw call to execute.</param>
        /// <param name="blendFactor">[Optional] The factor used to modulate the pixel shader, render target or both.</param>
        /// <param name="blendSampleMask">[Optional] The mask used to define which samples get updated in the active render targets.</param>
        /// <param name="depthStencilReference">[Optional] The depth/stencil reference value used when performing a depth/stencil test.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawIndexCall"/> parameter is <b>null</b>.</exception>
        public void Submit(GorgonDrawIndexCall drawIndexCall,
                           in GorgonColor? blendFactor = null,
                           int blendSampleMask = int.MinValue,
                           int depthStencilReference = 0)
        {
            drawIndexCall.ValidateObject(nameof(drawIndexCall));
            SetDrawStates(drawIndexCall.D3DState, blendFactor ?? GorgonColor.White, blendSampleMask, depthStencilReference);
            D3DDeviceContext.DrawIndexed(drawIndexCall.IndexCount, drawIndexCall.IndexStart, drawIndexCall.BaseVertexIndex);
            unchecked
            {
                ++_stats._drawCallCount;
                _stats._triangleCount += drawIndexCall.IndexCount / 3;
            }
        }

        /// <summary>
        /// Function to submit a draw call with indices to the GPU.
        /// </summary>
        /// <param name="drawIndexCall">The draw call to execute.</param>
        /// <param name="blendFactor">[Optional] The factor used to modulate the pixel shader, render target or both.</param>
        /// <param name="blendSampleMask">[Optional] The mask used to define which samples get updated in the active render targets.</param>
        /// <param name="depthStencilReference">[Optional] The depth/stencil reference value used when performing a depth/stencil test.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawIndexCall"/> parameter is <b>null</b>.</exception>
        public void Submit(GorgonInstancedIndexCall drawIndexCall,
                           in GorgonColor? blendFactor = null,
                           int blendSampleMask = int.MinValue,
                           int depthStencilReference = 0)
        {
            drawIndexCall.ValidateObject(nameof(drawIndexCall));
            SetDrawStates(drawIndexCall.D3DState, blendFactor ?? GorgonColor.White, blendSampleMask, depthStencilReference);
            D3DDeviceContext.DrawIndexedInstanced(drawIndexCall.IndexCountPerInstance,
                                                  drawIndexCall.InstanceCount,
                                                  drawIndexCall.IndexStart,
                                                  drawIndexCall.BaseVertexIndex,
                                                  drawIndexCall.IndexStart);
            unchecked
            {
                ++_stats._drawCallCount;
                _stats._triangleCount += (drawIndexCall.IndexCountPerInstance * drawIndexCall.InstanceCount) / 3;
            }
        }

        /// <summary>
        /// Function to submit a <see cref="GorgonInstancedIndexCall"/> to the GPU using a <see cref="GorgonBuffer"/> to pass in variable sized arguments.
        /// </summary>
        /// <param name="drawIndexCall">The draw call to submit.</param>
        /// <param name="indirectArgs">The buffer containing the draw call arguments to pass.</param>
        /// <param name="argumentOffset">[Optional] The offset, in bytes, within the buffer to start reading the arguments from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawIndexCall"/>, or the <paramref name="indirectArgs"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="argumentOffset"/> parameter is less than 0.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="indirectArgs"/> was not created with the <see cref="IGorgonBufferInfo.IndirectArgs"/> flag set to <b>true</b>.</exception>
        /// <remarks>
        /// <para>
        /// This allows submitting a <see cref="GorgonInstancedIndexCall"/> with variable arguments without having to perform a read back of that data from the GPU and therefore avoid a stall. 
        /// </para>
        /// <para>
        /// Like the <see cref="SubmitStreamOut"/> method, this is useful when a shader generates an arbitrary amount of data within a buffer. To get the size, or the data itself out of the buffer will 
        /// cause a stall when swtiching back to the CPU. This is obviously not good for performance. So, to counter this, this method will pass the buffer with the arguments for the draw call straight 
        /// through without having to get the CPU to read the data back, thus avoiding the stall.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonInstancedIndexCall"/>
        public void SubmitIndirect(GorgonInstancedIndexCall drawIndexCall, GorgonBuffer indirectArgs, int argumentOffset = 0)
        {
            drawIndexCall.ValidateObject(nameof(drawIndexCall));
            indirectArgs.ValidateObject(nameof(indirectArgs));
            SetDrawStates(drawIndexCall.D3DState, GorgonColor.White, int.MinValue, 0);
            D3DDeviceContext.DrawIndexedInstancedIndirect(indirectArgs.Native, argumentOffset);
            unchecked
            {
                ++_stats._indirectCount;
            }
        }

        /// <summary>
        /// Function to submit a <see cref="GorgonInstancedCall"/> to the GPU using a <see cref="GorgonBuffer"/> to pass in variable sized arguments.
        /// </summary>
        /// <param name="drawCall">The draw call to submit.</param>
        /// <param name="indirectArgs">The buffer containing the draw call arguments to pass.</param>
        /// <param name="argumentOffset">[Optional] The offset, in bytes, within the buffer to start reading the arguments from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/>, or the <paramref name="indirectArgs"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="argumentOffset"/> parameter is less than 0.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="indirectArgs"/> was not created with the <see cref="IGorgonBufferInfo.IndirectArgs"/> flag set to <b>true</b>.</exception>
        /// <remarks>
        /// <para>
        /// This allows submitting a <see cref="GorgonInstancedCall"/> with variable arguments without having to perform a read back of that data from the GPU and therefore avoid a stall. 
        /// </para>
        /// <para>
        /// Like the <see cref="SubmitStreamOut"/> method, this is useful when a shader generates an arbitrary amount of data within a buffer. To get the size, or the data itself out of the buffer will 
        /// cause a stall when swtiching back to the CPU. This is obviously not good for performance. So, to counter this, this method will pass the buffer with the arguments for the draw call straight 
        /// through without having to get the CPU to read the data back, thus avoiding the stall.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonInstancedCall"/>
        public void SubmitIndirect(GorgonInstancedCall drawCall, GorgonBuffer indirectArgs, int argumentOffset = 0)
        {
            drawCall.ValidateObject(nameof(drawCall));
            indirectArgs.ValidateObject(nameof(indirectArgs));

#if DEBUG
            if (argumentOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(argumentOffset), Resources.GORGFX_ERR_PARAMETER_LESS_THAN_ZERO);
            }

            if (!indirectArgs.IndirectArgs)
            {
                throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_ERR_BUFFER_NOT_INDIRECTARGS, indirectArgs.Name));
            }
#endif

            SetDrawStates(drawCall.D3DState, GorgonColor.White, int.MinValue, 0);
            D3DDeviceContext.DrawInstancedIndirect(indirectArgs.Native, argumentOffset);
            unchecked
            {
                ++_stats._indirectCount;
            }
        }

        /// <summary>
        /// Function to submit a <see cref="GorgonDrawCallCommon"/> to the GPU.
        /// </summary>
        /// <param name="drawCall">The draw call to submit.</param>
        /// <param name="blendFactor">[Optional] The factor used to modulate the pixel shader, render target or both.</param>
        /// <param name="blendSampleMask">[Optional] The mask used to define which samples get updated in the active render targets.</param>
        /// <param name="depthStencilReference">[Optional] The depth/stencil reference value used when performing a depth/stencil test.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This method sends a series of state changes and resource bindings to the GPU. However, unlike the <see cref="Submit(GorgonDrawIndexCall, in GorgonColor?, int, int)"/> command, this command uses 
        /// pre-processed data from the vertex and stream out stages. This means that the <see cref="GorgonVertexBuffer"/> attached to the draw call must have been assigned to the  previous
        /// <see cref="GorgonDrawCallCommon.StreamOutBufferBindings"/> and had data deposited into it from the stream out stage. After that, it should be be assigned to a <see cref="GorgonStreamOutCall"/>
        /// passed to this method.
        /// </para>
        /// <para>
        /// To render data with this method, the <see cref="GorgonVertexBufferBinding"/> being rendered must have been be created with the <see cref="VertexIndexBufferBinding.StreamOut"/> flag set.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void SubmitStreamOut(GorgonStreamOutCall drawCall,
                                    in GorgonColor? blendFactor = null,
                                    int blendSampleMask = int.MinValue,
                                    int depthStencilReference = 0)
        {
            drawCall.ValidateObject(nameof(drawCall));

#if DEBUG
            if (drawCall.PipelineState.VertexShader == null)
            {
                throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_NO_VERTEX_SHADER);
            }
#endif

            SetDrawStates(drawCall.D3DState, blendFactor ?? GorgonColor.White, blendSampleMask, depthStencilReference);
            D3DDeviceContext.DrawAuto();
            unchecked
            {
                ++_stats._streamOutCount;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            RenderTargetFactory rtvFactory = Interlocked.Exchange(ref _rtvFactory, null);
            D3D11.DeviceContext4 context = Interlocked.Exchange(ref _deviceContext, null);
            D3D11.Device5 device = Interlocked.Exchange(ref _device, null);
            Adapter4 adapter = Interlocked.Exchange(ref _dxgiAdapter, null);
            Factory5 factory = Interlocked.Exchange(ref _dxgiFactory, null);
            PipelineStateCache pipeCache = Interlocked.Exchange(ref _pipelineStateCache, null);
            SamplerCache samplerCache = Interlocked.Exchange(ref _samplerCache, null);

            // If these are all gone, then we've already disposed.
            if ((factory == null)
                && (adapter == null)
                && (device == null)
                && (context == null)
                && (pipeCache == null))
            {
                return;
            }

            rtvFactory?.Dispose();

            // TODO:
            samplerCache.Dispose();
            pipeCache.Dispose();

            // Dispose all objects created from this interface.
            this.DisposeAll();

            // Disconnect from the context.
            Log.Print($"Destroying GorgonGraphics interface for device '{VideoAdapter.Name}'...", LoggingLevel.Simple);

            // Reset the state for the context. This will ensure we don't have anything bound to the pipeline when we shut down.
            context?.ClearState();
            context?.Dispose();
            device?.Dispose();
            adapter?.Dispose();
            factory?.Dispose();
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonGraphics"/> class.
        /// </summary>
        /// <param name="videoAdapterInfo">A <see cref="IGorgonVideoAdapterInfo"/> to specify the video adapter to use for this instance.</param>
        /// <param name="featureSet">[Optional] The requested feature set for the video adapter used with this object.</param>
        /// <param name="log">[Optional] The log to use for debugging.</param>
        /// <exception cref="GorgonException">Thrown when the <paramref name="featureSet"/> is unsupported.</exception>
        /// <remarks>
        /// <para>
        /// When the <paramref name="videoAdapterInfo"/> is set to <b>null</b>, Gorgon will use the first video adapter with feature level specified by <paramref name="featureSet"/>  
        /// will be used. If the feature level requested is higher than what any device in the system can support, then the first device with the highest feature level will be used.
        /// </para>
        /// <para>
        /// When specifying a feature set, the device with the closest matching feature set will be used. If the <paramref name="videoAdapterInfo"/> is specified, then that device will be used at the 
        /// requested <paramref name="featureSet"/>. If the requested <paramref name="featureSet"/> is higher than what the <paramref name="videoAdapterInfo"/> will support, then Gorgon will use the 
        /// highest feature of the specified <paramref name="videoAdapterInfo"/>. 
        /// </para>
        /// <para>
        /// If Gorgon is compiled in DEBUG mode, and <paramref name="videoAdapterInfo"/> is <b>null</b>, then it will attempt to find the most appropriate hardware video adapter, and failing that, will fall 
        /// back to a software device.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// The Gorgon Graphics library only works on Windows 10 v1703 Build 15603 (Creators Update) or better. No lesser operating system version is supported.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>
        /// The following examples show the various ways the object can be configured:
        /// </para>
        /// <code lang="csharp">
        /// <![CDATA[
        /// // Create using a specific video adapter and use the highest feature set supported by that device:
        /// // Get a list of available video adapters.
        /// IReadOnlyList<IGorgonVideoAdapterInfo> videoAdapters = GorgonGraphics.EnumerateAdapters(false, log);
        ///
        /// // In real code, you should always check for more than 0 devices in the resulting list.
        /// GorgonGraphics graphics = new GorgonGraphics(videoAdapters[0]);
        /// 
        /// // Create using the requested feature set and the first adapter that supports the nearest feature set requested:
        /// // If the device does not support 12.1, then the device with the nearest feature set (e.g. 12.0) will be used instead.
        /// IReadOnlyList<IGorgonVideoAdapterInfo> videoAdapters = GorgonGraphics.EnumerateAdapters(false, log);
        /// 
        /// // In real code, you should always check for more than 0 devices in the resulting list.
        /// GorgonGraphics graphics = new GorgonGraphics(videoAdapters[0], FeatureSet.Level_12_1);
        /// ]]>
        /// </code>
        /// </example>
        /// <seealso cref="IGorgonVideoAdapterInfo"/>
        public GorgonGraphics(IGorgonVideoAdapterInfo videoAdapterInfo,
                              FeatureSet? featureSet = null,
                              IGorgonLog log = null)
        {
            VideoAdapter = videoAdapterInfo ?? throw new ArgumentNullException(nameof(videoAdapterInfo));
            Log = log ?? GorgonLog.NullLog;

            // If we've not specified a feature level, or the feature level exceeds the requested device feature level, then 
            // fall back to the device feature level.
            if ((featureSet == null) || (videoAdapterInfo.FeatureSet < featureSet.Value))
            {
                featureSet = videoAdapterInfo.FeatureSet;
            }

            // We only support feature set 12 and greater.
            if (!Enum.IsDefined(typeof(FeatureSet), featureSet.Value))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FEATURE_LEVEL_INVALID, featureSet));
            }

            FeatureSet = featureSet.Value;

            Log.Print("Gorgon Graphics initializing...", LoggingLevel.Simple);
            Log.Print($"Using video adapter '{videoAdapterInfo.Name}' at {featureSet.Value.Description()} for Direct 3D {featureSet.Value.D3DVersion()}.", LoggingLevel.Simple);

            // Build up the required device objects to pass in to the constructor.
            (_device, _dxgiFactory, _dxgiAdapter) = CreateDevice(videoAdapterInfo, (D3D.FeatureLevel)featureSet.Value);
            _deviceContext = _device.ImmediateContext.QueryInterface<D3D11.DeviceContext4>();

            FormatSupport = EnumerateFormatSupport(_device);

            _rtvFactory = new RenderTargetFactory(this);

            _pipelineStateCache = new PipelineStateCache(_device);
            _samplerCache = new SamplerCache(_device);
            _samplerCache.WarmCache();
            _stateEvaluator = new StateEvaluator(this);
            _stateApplicator = new D3D11StateApplicator(this, _stateEvaluator.RenderTargets);

            Log.Print("Gorgon Graphics initialized.", LoggingLevel.Simple);
        }

        /// <summary>
        /// Initializes the <see cref="GorgonGraphics"/> class.
        /// </summary>
        static GorgonGraphics()
        {
            CheckMinimumOperatingSystem();

            DX.Configuration.ThrowOnShaderCompileError = false;

#if DEBUG   
            IsDebugEnabled = true;
#endif
        }
        #endregion
    }
}
