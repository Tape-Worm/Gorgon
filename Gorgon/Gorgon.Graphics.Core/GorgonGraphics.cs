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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Native;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
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
        // The available shader types.
        private static readonly ShaderType[] _shaderTypes = (ShaderType[])Enum.GetValues(typeof(ShaderType));

        // An empty UAV count to get around an idiotic design decision (no count exposed on the method) for UAVs on compute shaders
        private static readonly int[] _emptyUavCounts = new int[GorgonReadWriteViewBindings.MaximumReadWriteViewCount];

        // An empty UAV to get around an idiotic design decision (no count exposed on the method) for UAVs on compute shaders
        private static readonly D3D11.UnorderedAccessView[] _emptyUavs = new D3D11.UnorderedAccessView[GorgonReadWriteViewBindings.MaximumReadWriteViewCount];

        // An empty SRV.
        private static readonly D3D11.ShaderResourceView[] _emptySrvs = new D3D11.ShaderResourceView[GorgonShaderResourceViews.MaximumShaderResourceViewCount];

        // An empty set of buffers.
        private static readonly D3D11.Buffer[] _emptyConstantBuffers = new D3D11.Buffer[GorgonConstantBuffers.MaximumConstantBufferCount];

        // Empty sampler states.
        private static readonly D3D11.SamplerState[] _emptySamplers = new D3D11.SamplerState[GorgonSamplerStates.MaximumSamplerStateCount];

        // The D3D 11.4 device context.
        private D3D11.DeviceContext4 _d3DDeviceContext;

        // The D3D 11.4 device.
        private D3D11.Device5 _d3DDevice;

        // The DXGI adapter.
        private Adapter4 _dxgiAdapter;

        // The DXGI factory
        private Factory5 _dxgiFactory;

        // The render targets currently bound to the pipeline.
        private readonly GorgonRenderTargetView[] _renderTargets = new GorgonRenderTargetView[D3D11.OutputMergerStage.SimultaneousRenderTargetCount];

        // The Native render targets.
        private readonly D3D11.RenderTargetView[] _d3DRtvs = new D3D11.RenderTargetView[D3D11.OutputMergerStage.SimultaneousRenderTargetCount];

        // The Native unordered access views.
        private (D3D11.UnorderedAccessView[], int[]) _d3DUavs = (new D3D11.UnorderedAccessView[1], new int[1]);

        // The viewports used to define the area to render into.
        private readonly DX.ViewportF[] _viewports = new DX.ViewportF[16];

        // The viewports used to define the area to render into.
        private DX.Rectangle[] _scissors = new DX.Rectangle[1];

        // The flag used to determine if a render target and/or depth/stencil is updated.
        private (bool RtvsChanged, bool DepthViewChanged) _isTargetUpdated;

        // The state of the previous draw call.
        // We do a lot of copying between our draw call data to keep it immutable, but this thing will store references
        // directly (for a tiny performance gain, hopefully).
        private readonly D3DState _lastState = new D3DState
        {
            CsReadWriteViews = new GorgonReadWriteViewBindings(),
            PsSamplers = new GorgonSamplerStates(),
            VsSrvs = new GorgonShaderResourceViews(),
            CsConstantBuffers = new GorgonConstantBuffers(),
            CsSamplers = new GorgonSamplerStates(),
            CsSrvs = new GorgonShaderResourceViews(),
            DsConstantBuffers = new GorgonConstantBuffers(),
            DsSamplers = new GorgonSamplerStates(),
            DsSrvs = new GorgonShaderResourceViews(),
            GsConstantBuffers = new GorgonConstantBuffers(),
            GsSamplers = new GorgonSamplerStates(),
            GsSrvs = new GorgonShaderResourceViews(),
            HsConstantBuffers = new GorgonConstantBuffers(),
            HsSamplers = new GorgonSamplerStates(),
            HsSrvs = new GorgonShaderResourceViews(),
            PsConstantBuffers = new GorgonConstantBuffers(),
            PsSrvs = new GorgonShaderResourceViews(),
            ReadWriteViews = new GorgonReadWriteViewBindings(),
            StreamOutBindings = new GorgonStreamOutBindings(),
            VertexBuffers = new GorgonVertexBufferBindings(),
            VsConstantBuffers = new GorgonConstantBuffers(),
            VsSamplers = new GorgonSamplerStates(),
            PipelineState = new GorgonPipelineState
            {
                PrimitiveType = PrimitiveType.None
            }
        };

        // A cache for holding vertex bindings.
        private D3D11.VertexBufferBinding[] _vertexBindingCache = new D3D11.VertexBufferBinding[1];

        // A cache for holding stream output bindings.
        private D3D11.StreamOutputBufferBinding[] _streamOutBindingCache = new D3D11.StreamOutputBufferBinding[1];

        // A list of cached pipeline states.
        private readonly List<GorgonPipelineState> _cachedPipelineStates = new List<GorgonPipelineState>();

        // A list of cached pipeline states.
        private readonly List<GorgonSamplerState> _cachedSamplers = new List<GorgonSamplerState>();

        // A syncrhonization lock for multiple thread when dealing with the pipeline state cache.
        private readonly object _stateLock = new object();

        // A syncrhonization lock for multiple thread when dealing with the sampler cache.
        private readonly object _samplerLock = new object();

        // The previous depth/stencil reference value.
        private int _depthStencilReference;

        // The previous blend sample mask.
        private int _blendSampleMask = int.MinValue;

        // The previous blend factor.
        private GorgonColor _blendFactor = GorgonColor.White;

        // The texture blitter used to draw 2D textures to the render target.
        private Lazy<TextureBlitter> _textureBlitter;

        // A factory for creating temporary render targets.
        private RenderTargetFactory _rtvFactory;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the Direct 3D 11.4 device context for this graphics instance.
        /// </summary>
        internal D3D11.DeviceContext4 D3DDeviceContext => _d3DDeviceContext;

        /// <summary>
        /// Property to return the Direct 3D 11.4 device for this graphics instance.
        /// </summary>
        internal D3D11.Device5 D3DDevice => _d3DDevice;

        /// <summary>
        /// Property to return the selected DXGI video adapter for this graphics instance.
        /// </summary>
        internal Adapter4 DXGIAdapter => _dxgiAdapter;

        /// <summary>
        /// Property to return the DXGI factory used to create DXGI objects.
        /// </summary>
        internal Factory5 DXGIFactory => _dxgiFactory;

        /// <summary>
        /// Property to return the number of draw calls since the last frame.
        /// </summary>
        public ulong DrawCallCount
        {
            get;
            private set;
        }

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
        public GorgonDepthStencil2DView DepthStencilView
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the currently assigned render targets.
        /// </summary>
        public IReadOnlyList<GorgonRenderTargetView> RenderTargets => _renderTargets;

        /// <summary>
        /// Property to return the list of currently assigned viewports.
        /// </summary>
        public IReadOnlyList<DX.ViewportF> Viewports => _viewports;

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
        /// Function to initialize the cached sampler states with the predefined states provided on the sampler state class.
        /// </summary>
        private void InitializeCachedSamplers()
        {
            lock (_samplerLock)
            {
                GorgonSamplerState.Default.BuildD3D11SamplerState(_d3DDevice);
                GorgonSamplerState.AnisotropicFiltering.BuildD3D11SamplerState(_d3DDevice);
                GorgonSamplerState.PointFiltering.BuildD3D11SamplerState(_d3DDevice);
                GorgonSamplerState.Wrapping.BuildD3D11SamplerState(_d3DDevice);
                GorgonSamplerState.PointFilteringWrapping.BuildD3D11SamplerState(_d3DDevice);

                _cachedSamplers.Add(GorgonSamplerState.Default);
                _cachedSamplers.Add(GorgonSamplerState.Wrapping);
                _cachedSamplers.Add(GorgonSamplerState.AnisotropicFiltering);
                _cachedSamplers.Add(GorgonSamplerState.PointFiltering);
                _cachedSamplers.Add(GorgonSamplerState.PointFilteringWrapping);
            }
        }

        /// <summary>
        /// Function to clear the resource caches.
        /// </summary>
        private void ClearResourceCaches()
        {
            DepthStencilView = null;
            Array.Clear(_renderTargets, 0, _renderTargets.Length);
            Array.Clear(_scissors, 0, _scissors.Length);
            Array.Clear(_viewports, 0, _viewports.Length);
            Array.Clear(_vertexBindingCache, 0, _vertexBindingCache.Length);
            Array.Clear(_streamOutBindingCache, 0, _streamOutBindingCache.Length);
            Array.Clear(_d3DRtvs, 0, _d3DRtvs.Length);
            Array.Clear(_d3DUavs.Item1, 0, _d3DUavs.Item1.Length);
            Array.Clear(_d3DUavs.Item2, 0, _d3DUavs.Item2.Length);
        }

        /// <summary>
        /// Function to clear the state cache for the pipeline states and sampler states.
        /// </summary>
        /// <param name="userCall"><b>true</b> if called by the user, <b>false</b> if called by Dispose.</param>
        private void ClearStateCache(bool userCall)
        {
            lock (_samplerLock)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < _cachedSamplers.Count; ++i)
                {
                    _cachedSamplers[i].ID = int.MinValue;
                    _cachedSamplers[i].Native?.Dispose();
                }

                _cachedSamplers.Clear();
            }

            if (userCall)
            {
                InitializeCachedSamplers();
            }

            lock (_stateLock)
            {
                // Wipe out the state cache.
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < _cachedPipelineStates.Count; ++i)
                {
                    _cachedPipelineStates[i].ID = int.MinValue;
                    _cachedPipelineStates[i].D3DRasterState?.Dispose();
                    _cachedPipelineStates[i].D3DDepthStencilState?.Dispose();
                    _cachedPipelineStates[i].D3DBlendState?.Dispose();
                }

                _cachedPipelineStates.Clear();
            }

            if (userCall)
            {
                ClearState();
            }
        }

        /// <summary>
        /// Function to bind a list of Direct3D vertex buffers to the pipeline.
        /// </summary>
        /// <param name="bindings">The vertex bindings.</param>
        private void BindVertexBuffers(GorgonVertexBufferBindings bindings)
        {
            (int start, int count) = bindings.GetDirtyItems();

            if (count == 0)
            {
                Array.Clear(_vertexBindingCache, 0, _vertexBindingCache.Length);
                D3DDeviceContext.InputAssembler.SetVertexBuffers(0, _vertexBindingCache);
                return;
            }

            if (_vertexBindingCache.Length != count)
            {
                Array.Resize(ref _vertexBindingCache, count);
            }

            for (int i = 0; i < count; ++i)
            {
                _vertexBindingCache[i] = bindings.Native[i];
            }

            D3DDeviceContext.InputAssembler.SetVertexBuffers(start, _vertexBindingCache);
        }

        /// <summary>
        /// Function to bind a list of Direct3D stream out buffers to the pipeline.
        /// </summary>
        /// <param name="bindings">The vertex bindings.</param>
        private void BindStreamOutBuffers(GorgonStreamOutBindings bindings)
        {
            (int _, int count) = bindings.GetDirtyItems();

            if (count == 0)
            {
                Array.Clear(_streamOutBindingCache, 0, _streamOutBindingCache.Length);
                D3DDeviceContext.StreamOutput.SetTargets(null);
                return;
            }

            if (_streamOutBindingCache.Length != count)
            {
                Array.Resize(ref _streamOutBindingCache, count);
            }

            for (int i = 0; i < count; ++i)
            {
                _streamOutBindingCache[i] = bindings.Native[i];
            }

            D3DDeviceContext.StreamOutput.SetTargets(bindings.Native);
        }

        /// <summary>
        /// Function to bind the index buffer to the pipeline.
        /// </summary>
        /// <param name="indexBuffer">The index buffer to bind.</param>
        private void BindIndexBuffer(GorgonIndexBuffer indexBuffer)
        {
            if (indexBuffer != null)
            {
                D3DDeviceContext.InputAssembler.SetIndexBuffer(indexBuffer.Native,
                                                               indexBuffer.Use16BitIndices ? Format.R16_UInt : Format.R32_UInt,
                                                               0);
            }
            else
            {
                D3DDeviceContext.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);
            }
        }

        /// <summary>
        /// Function to update a change enum to determine what states need setting.
        /// </summary>
        /// <param name="expressionResult">The result of an expression.</param>
        /// <param name="requestedChange">The change to make if the expression fails.</param>
        /// <param name="currentChanges">The current list of changes.</param>
        /// <returns><b>true</b> if a change was made, <b>false</b> if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool GetPipelineChanges(bool expressionResult, DrawCallChanges requestedChange, ref DrawCallChanges currentChanges)
        {
            if (!expressionResult)
            {
                currentChanges |= requestedChange;
            }

            return !expressionResult;
        }

        /// <summary>
        /// Function to assign sampler states to a shader
        /// </summary>
        /// <param name="shaderStage">The shader stage to use.</param>
        /// <param name="samplers">The samplers to apply.</param>
        private static void BindSamplers(D3D11.CommonShaderStage shaderStage, GorgonSamplerStates samplers)
        {
            if (samplers == null)
            {
                shaderStage.SetSamplers(0, _emptySamplers);
                return;
            }

            (int start, int count) = samplers.GetDirtyItems();

            D3D11.SamplerState[] states = samplers.Native;

            if (count == 0)
            {
                states = _emptySamplers;
                count = 1;
            }

            shaderStage.SetSamplers(start, count, states);
        }

        /// <summary>
        /// Function to set the current list of scissor rectangles.
        /// </summary>
        /// <param name="rectangles">The list of scissor rectangles to apply.</param>
        private void SetScissors(IReadOnlyList<DX.Rectangle> rectangles)
        {
            if ((rectangles == null) || (rectangles.Count == 0))
            {
                D3DDeviceContext.Rasterizer.SetScissorRectangles((DX.Rectangle[])null);
                return;
            }

            if (rectangles.Count == 1)
            {
                if (_scissors.Length < 1)
                {
                    _scissors = new DX.Rectangle[1];
                }

                _scissors[0] = rectangles[0];
                D3DDeviceContext.Rasterizer.SetScissorRectangle(rectangles[0].Left, rectangles[0].Top, rectangles[0].Right, rectangles[0].Bottom);
                return;
            }

            if (_scissors.Length != rectangles.Count)
            {
                _scissors = new DX.Rectangle[rectangles.Count];
            }


            for (int i = 0; i < rectangles.Count; ++i)
            {
                _scissors[i] = rectangles[i];
            }

            D3DDeviceContext.Rasterizer.SetScissorRectangles(_scissors);
        }

        /// <summary>
        /// Function to compare the two list of scissor rectangle lists for equality.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        private static bool CompareScissorRects(IReadOnlyList<DX.Rectangle> left, IReadOnlyList<DX.Rectangle> right)
        {
            if (left == right)
            {
                return true;
            }

            if (((left == null) && (right != null)) || (left == null))
            {
                return false;
            }

            if (left.Count != right.Count)
            {
                return false;
            }

            for (int i = 0; i < left.Count; ++i)
            {
                if (!left[i].Equals(right[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Function to apply the pipeline state.
        /// </summary>
        /// <param name="currentState">The state passed in from the draw call.</param>
        /// <param name="changes">The changes to make.</param>
        private void ApplyState(GorgonPipelineState currentState, DrawCallChanges changes)
        {
            if (changes == DrawCallChanges.None)
            {
                return;
            }

            if ((changes & DrawCallChanges.Topology) == DrawCallChanges.Topology)
            {
                D3DDeviceContext.InputAssembler.PrimitiveTopology = (D3D.PrimitiveTopology)_lastState.PipelineState.PrimitiveType;
            }

            if ((changes & DrawCallChanges.RasterState) == DrawCallChanges.RasterState)
            {
                D3DDeviceContext.Rasterizer.State = currentState.D3DRasterState;
            }

            if ((changes & DrawCallChanges.DepthStencilState) == DrawCallChanges.DepthStencilState)
            {
                D3DDeviceContext.OutputMerger.DepthStencilState = currentState.D3DDepthStencilState;
            }

            if ((changes & DrawCallChanges.BlendState) == DrawCallChanges.BlendState)
            {
                D3DDeviceContext.OutputMerger.BlendState = currentState.D3DBlendState;
            }

            if ((changes & DrawCallChanges.Scissors) == DrawCallChanges.Scissors)
            {
                SetScissors(currentState.RasterState.ScissorRectangles);
            }

            if ((changes & DrawCallChanges.PixelShader) == DrawCallChanges.PixelShader)
            {
                D3DDeviceContext.PixelShader.Set(currentState.PixelShader?.NativeShader);
            }

            if ((changes & DrawCallChanges.VertexShader) == DrawCallChanges.VertexShader)
            {
                D3DDeviceContext.VertexShader.Set(currentState.VertexShader?.NativeShader);
            }

            if ((changes & DrawCallChanges.GeometryShader) == DrawCallChanges.GeometryShader)
            {
                D3DDeviceContext.GeometryShader.Set(currentState.GeometryShader?.NativeShader);
            }

            if ((changes & DrawCallChanges.DomainShader) == DrawCallChanges.DomainShader)
            {
                D3DDeviceContext.DomainShader.Set(currentState.DomainShader?.NativeShader);
            }

            if ((changes & DrawCallChanges.HullShader) == DrawCallChanges.HullShader)
            {
                D3DDeviceContext.HullShader.Set(currentState.HullShader?.NativeShader);
            }
        }

        /// <summary>
        /// Function to build a merged render state.
        /// </summary>
        /// <param name="currentState">The current state.</param>
        /// <returns>A set of changes that need to be applied to the pipeline.</returns>
        private DrawCallChanges BuildStateChanges(GorgonPipelineState currentState)
        {
            GorgonPipelineState lastState = _lastState.PipelineState;
            DrawCallChanges changes = DrawCallChanges.None;

            if (GetPipelineChanges(lastState.PrimitiveType == currentState.PrimitiveType, DrawCallChanges.Topology, ref changes))
            {
                _lastState.PipelineState.PrimitiveType = currentState.PrimitiveType;
            }

            if (GetPipelineChanges(lastState.D3DRasterState == currentState.D3DRasterState, DrawCallChanges.RasterState, ref changes))
            {
                _lastState.PipelineState.D3DRasterState = currentState.D3DRasterState;
            }

            if (GetPipelineChanges(lastState.D3DBlendState == currentState.D3DBlendState, DrawCallChanges.BlendState, ref changes))
            {
                _lastState.PipelineState.IsAlphaToCoverageEnabled = currentState.IsAlphaToCoverageEnabled;
                _lastState.PipelineState.IsIndependentBlendingEnabled = currentState.IsIndependentBlendingEnabled;
                _lastState.PipelineState.D3DBlendState = currentState.D3DBlendState;
            }

            if (GetPipelineChanges(lastState.D3DDepthStencilState == currentState.D3DDepthStencilState, DrawCallChanges.DepthStencilState, ref changes))
            {
                _lastState.PipelineState.D3DDepthStencilState = currentState.D3DDepthStencilState;
            }

            if (((changes & DrawCallChanges.RasterState) != DrawCallChanges.RasterState)
                && (GetPipelineChanges(CompareScissorRects(lastState.RasterState?.ScissorRectangles, currentState.RasterState?.ScissorRectangles),
                                  DrawCallChanges.Scissors,
                                  ref changes)))
            {
                _lastState.PipelineState.RasterState = currentState.RasterState;
            }

            if (GetPipelineChanges(lastState.VertexShader == currentState.VertexShader, DrawCallChanges.VertexShader, ref changes))
            {
                _lastState.PipelineState.VertexShader = currentState.VertexShader;
            }

            if (GetPipelineChanges(lastState.PixelShader == currentState.PixelShader, DrawCallChanges.PixelShader, ref changes))
            {
                _lastState.PipelineState.PixelShader = currentState.PixelShader;
            }

            if (GetPipelineChanges(lastState.GeometryShader == currentState.GeometryShader, DrawCallChanges.GeometryShader, ref changes))
            {
                _lastState.PipelineState.GeometryShader = currentState.GeometryShader;
            }

            if (GetPipelineChanges(lastState.DomainShader == currentState.DomainShader, DrawCallChanges.DomainShader, ref changes))
            {
                _lastState.PipelineState.DomainShader = currentState.DomainShader;
            }

            if (GetPipelineChanges(lastState.HullShader == currentState.HullShader, DrawCallChanges.HullShader, ref changes))
            {
                _lastState.PipelineState.HullShader = currentState.HullShader;
            }

            return changes;
        }

        /// <summary>
        /// Function to bind the constant buffers to a specific shader stage.
        /// </summary>
        /// <param name="shaderType">The shader stage.</param>
        /// <param name="constantBuffers">The constant buffers to bind.</param>
        private void BindConstantBuffers(ShaderType shaderType, GorgonConstantBuffers constantBuffers)
        {
            if (constantBuffers == null)
            {
                switch (shaderType)
                {
                    case ShaderType.Vertex:
                        D3DDeviceContext.VSSetConstantBuffers1(0, _emptyConstantBuffers.Length, _emptyConstantBuffers, null, null);
                        break;
                    case ShaderType.Pixel:
                        D3DDeviceContext.PSSetConstantBuffers1(0, _emptyConstantBuffers.Length, _emptyConstantBuffers, null, null);
                        break;
                    case ShaderType.Geometry:
                        D3DDeviceContext.GSSetConstantBuffers1(0, _emptyConstantBuffers.Length, _emptyConstantBuffers, null, null);
                        break;
                    case ShaderType.Domain:
                        D3DDeviceContext.DSSetConstantBuffers1(0, _emptyConstantBuffers.Length, _emptyConstantBuffers, null, null);
                        break;
                    case ShaderType.Hull:
                        D3DDeviceContext.HSSetConstantBuffers1(0, _emptyConstantBuffers.Length, _emptyConstantBuffers, null, null);
                        break;
                    case ShaderType.Compute:
                        D3DDeviceContext.CSSetConstantBuffers1(0, _emptyConstantBuffers.Length, _emptyConstantBuffers, null, null);
                        break;
                }

                return;
            }

            (int start, int count) = constantBuffers.GetDirtyItems();

            // Ensure that we pick up changes to the constant buffers view range.
            for (int i = 0; i < count; ++i)
            {
                GorgonConstantBufferView view = constantBuffers[i + start];

                if ((view == null) || (!view.ViewAdjusted))
                {
                    continue;
                }

                view.ViewAdjusted = false;
                constantBuffers.ViewStart[i] = view.StartElement * 16;
                constantBuffers.ViewCount[i] = (view.ElementCount + 15) & ~15;
            }

            D3D11.Buffer[] buffers = constantBuffers.Native;
            int[] viewStarts = constantBuffers.ViewStart;
            int[] viewCounts = constantBuffers.ViewCount;

            // If we have no buffers, then reset to empty.
            if (count == 0)
            {
                buffers = _emptyConstantBuffers;
                viewStarts = null;
                viewCounts = null;
                count = 1;
            }

            switch (shaderType)
            {
                case ShaderType.Vertex:
                    D3DDeviceContext.VSSetConstantBuffers1(start, count, buffers, viewStarts, viewCounts);
                    break;
                case ShaderType.Pixel:
                    D3DDeviceContext.PSSetConstantBuffers1(start, count, buffers, viewStarts, viewCounts);
                    break;
                case ShaderType.Geometry:
                    D3DDeviceContext.GSSetConstantBuffers1(start, count, buffers, viewStarts, viewCounts);
                    break;
                case ShaderType.Domain:
                    D3DDeviceContext.DSSetConstantBuffers1(start, count, buffers, viewStarts, viewCounts);
                    break;
                case ShaderType.Hull:
                    D3DDeviceContext.HSSetConstantBuffers1(start, count, buffers, viewStarts, viewCounts);
                    break;
                case ShaderType.Compute:
                    D3DDeviceContext.CSSetConstantBuffers1(start, count, buffers, viewStarts, viewCounts);
                    break;
            }
        }

        /// <summary>
        /// Function to bind unordered access views to the pipeline.
        /// </summary>
        /// <param name="uavs">The unordered access views to bind.</param>
        /// <param name="useCs"><b>true</b> to set the uavs on the compute shader, <b>false</b> if they're being set on the draw interface.</param>
        private void BindUavs(GorgonReadWriteViewBindings uavs, bool useCs)
        {
            if (uavs == null)
            {
                if (!useCs)
                {
                    D3DDeviceContext.OutputMerger.SetUnorderedAccessViews(0, _emptyUavs, _emptyUavCounts);
                }
                else
                {
                    D3DDeviceContext.ComputeShader.SetUnorderedAccessViews(0, _emptyUavs, _emptyUavCounts);
                }

                return;
            }

            (int start, int count) = uavs.GetDirtyItems();

            if (_d3DUavs.Item1.Length != count)
            {
                _d3DUavs = (new D3D11.UnorderedAccessView[count], new int[count]);
            }

            for (int i = 0; i < count; ++i)
            {
                _d3DUavs.Item1[i] = uavs.Native[i];
                _d3DUavs.Item2[i] = uavs.Counts[i];
            }


            if (!useCs)
            {
                D3DDeviceContext.OutputMerger.SetUnorderedAccessViews(start, _d3DUavs.Item1, _d3DUavs.Item2);
            }
            else
            {
                if (_d3DUavs.Item1.Length == 0)
                {
                    D3DDeviceContext.ComputeShader.SetUnorderedAccessViews(0, _emptyUavs, _emptyUavCounts);
                }
                else
                {
                    D3DDeviceContext.ComputeShader.SetUnorderedAccessViews(start, _d3DUavs.Item1, _d3DUavs.Item2);
                }
            }
        }

        /// <summary>
        /// Function to fix the unordered access view resource sharing hazards prior to setting the uav.
        /// </summary>
        /// <param name="uav">The UAV to check.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "shader")]
        private void CheckUavForSrvHazards(GorgonGraphicsResource uav)
        {
            if ((_lastState == null) || (uav == null))
            {
                return;
            }

            for (int st = 0; st < _shaderTypes.Length; ++st)
            {
                ShaderType shaderType = _shaderTypes[st];

                // Check shader resources.
                D3D11.CommonShaderStage stage;
                GorgonShaderResourceViews lastSrvs;

                switch (shaderType)
                {
                    case ShaderType.Vertex:
                        stage = D3DDeviceContext.VertexShader;
                        lastSrvs = _lastState.VsSrvs;
                        break;
                    case ShaderType.Pixel:
                        stage = D3DDeviceContext.PixelShader;
                        lastSrvs = _lastState.PsSrvs;
                        break;
                    case ShaderType.Geometry:
                        stage = D3DDeviceContext.GeometryShader;
                        lastSrvs = _lastState.GsSrvs;
                        break;
                    case ShaderType.Domain:
                        stage = D3DDeviceContext.DomainShader;
                        lastSrvs = _lastState.DsSrvs;
                        break;
                    case ShaderType.Hull:
                        stage = D3DDeviceContext.HullShader;
                        lastSrvs = _lastState.HsSrvs;
                        break;
                    case ShaderType.Compute:
                        stage = D3DDeviceContext.ComputeShader;
                        lastSrvs = _lastState.CsSrvs;
                        break;
                    default:
                        throw new Exception(@"This should not happen.  But we cannot find an appropriate shader stage.");
                }

                if (lastSrvs == null)
                {
                    continue;
                }

                (int srvStart, int srvCount) = lastSrvs.GetDirtyItems(true);

                for (int j = srvStart; j < srvStart + srvCount; ++j)
                {
                    GorgonGraphicsResource resource = lastSrvs[j]?.Resource;

                    // If we have a resource, and it's attached to either the render target or depth/stencil being bound, 
                    // then reset it on the shader stage to avoid a hazard.
                    if ((resource == null) || (resource != uav))
                    {
                        continue;
                    }

                    stage.SetShaderResource(j, null);
                    lastSrvs.ResetAt(j);
                }
            }
        }

        /// <summary>
        /// Function to fix the vertex buffer resource sharing hazards prior to assigning the vertex buffer.
        /// </summary>
        /// <param name="bindings">The bindings to evaluate.</param>
        private void CheckStreamOutBuffersForVertexIndexBufferHazards(GorgonStreamOutBindings bindings)
        {
            if ((_lastState == null) || (bindings == null))
            {
                return;
            }

            (int soStart, int soCount) = bindings.GetDirtyItems();
            (int vbStart, int vbCount) = _lastState.VertexBuffers.GetDirtyItems();

            if ((vbCount == 0) || (soCount == 0))
            {
                return;
            }

            for (int i = soStart; i < soCount + soStart; ++i)
            {
                GorgonGraphicsResource so = bindings[i].Buffer;

                if (so == null)
                {
                    continue;
                }

                if (so == _lastState.IndexBuffer)
                {
                    D3DDeviceContext.StreamOutput.SetTarget(null, i);
                    _lastState.StreamOutBindings.ResetAt(i);

                    D3DDeviceContext.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);
                }

                for (int j = vbStart; j < vbCount + vbStart; ++j)
                {
                    GorgonGraphicsResource vb = _lastState.StreamOutBindings[j].Buffer;

                    if ((vb == null) || (vb != so))
                    {
                        continue;
                    }

                    D3DDeviceContext.InputAssembler.SetVertexBuffers(j, new D3D11.VertexBufferBinding());
                    _lastState.VertexBuffers.ResetAt(j);
                }
            }
        }

        /// <summary>
        /// Function to fix the vertex buffer resource sharing hazards prior to assigning the vertex buffer.
        /// </summary>
        /// <param name="bindings">The bindings to evaluate.</param>
        private void CheckVertexBuffersForStreamOutHazards(GorgonVertexBufferBindings bindings)
        {
            if ((_lastState == null) || (bindings == null))
            {
                return;
            }

            (int vbStart, int vbCount) = bindings.GetDirtyItems();
            (int soStart, int soCount) = _lastState.StreamOutBindings.GetDirtyItems();

            if ((vbCount == 0) || (soCount == 0))
            {
                return;
            }

            for (int i = vbStart; i < vbCount + vbStart; ++i)
            {
                GorgonGraphicsResource vb = bindings[i].VertexBuffer;

                if (vb == null)
                {
                    continue;
                }

                for (int j = soStart; j < soCount + soStart; ++j)
                {
                    GorgonGraphicsResource so = _lastState.StreamOutBindings[j].Buffer;

                    if ((so == null) || (so != vb))
                    {
                        continue;
                    }

                    D3DDeviceContext.StreamOutput.SetTarget(null, j);
                    _lastState.StreamOutBindings.ResetAt(j);
                }
            }
        }

        /// <summary>
        /// Function to fix the render target view resource sharing hazards prior to setting the rtv.
        /// </summary>
        /// <param name="rtViews">The render target views being assigned.</param>
        /// <param name="rtvCount">Number of render targets to process.</param>
        /// <param name="depth">The depth stencil being assigned.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "shader")]
        private void CheckRtvsForSrvUavHazards(GorgonRenderTargetView[] rtViews, int rtvCount, GorgonDepthStencil2DView depth)
        {
            if (_lastState == null)
            {
                return;
            }

            for (int i = 0; i < rtvCount; ++i)
            {
                GorgonGraphicsResource rt = rtViews[i]?.Resource;

                // If there's no resource and no depth/stencil, then skip.
                if ((rt == null) && (depth == null))
                {
                    continue;
                }

                // Check shader resources.
                for (int st = 0; st < _shaderTypes.Length; ++st)
                {
                    ShaderType shaderType = _shaderTypes[st];
                    D3D11.CommonShaderStage stage;
                    GorgonShaderResourceViews lastSrvs;
                    GorgonReadWriteViewBindings lastUavs;

                    switch (shaderType)
                    {
                        case ShaderType.Vertex:
                            stage = D3DDeviceContext.VertexShader;
                            lastSrvs = _lastState.VsSrvs;
                            lastUavs = _lastState.ReadWriteViews;
                            break;
                        case ShaderType.Pixel:
                            stage = D3DDeviceContext.PixelShader;
                            lastSrvs = _lastState.PsSrvs;
                            lastUavs = _lastState.ReadWriteViews;
                            break;
                        case ShaderType.Geometry:
                            stage = D3DDeviceContext.GeometryShader;
                            lastSrvs = _lastState.GsSrvs;
                            lastUavs = _lastState.ReadWriteViews;
                            break;
                        case ShaderType.Domain:
                            stage = D3DDeviceContext.DomainShader;
                            lastSrvs = _lastState.DsSrvs;
                            lastUavs = _lastState.ReadWriteViews;
                            break;
                        case ShaderType.Hull:
                            stage = D3DDeviceContext.HullShader;
                            lastSrvs = _lastState.HsSrvs;
                            lastUavs = _lastState.ReadWriteViews;
                            break;
                        case ShaderType.Compute:
                            stage = D3DDeviceContext.ComputeShader;
                            lastSrvs = _lastState.CsSrvs;
                            lastUavs = _lastState.CsReadWriteViews;
                            break;
                        default:
                            throw new Exception(@"This should not happen.  But we cannot find an appropriate shader stage.");
                    }

                    if (lastSrvs != null)
                    {
                        (int srvStart, int srvCount) = lastSrvs.GetDirtyItems(true);

                        for (int j = srvStart; j < srvStart + srvCount; ++j)
                        {
                            GorgonGraphicsResource resource = lastSrvs[j]?.Resource;

                            // Buffers and 1D textures cannot be render targets (imposed by Gorgon), so no need to check.
                            if ((resource == null) || (resource.ResourceType == GraphicsResourceType.Buffer) || (resource.ResourceType == GraphicsResourceType.Texture1D))
                            {
                                continue;
                            }

                            if (((rt == null) || (resource != rt))
                                && ((depth == null) || (depth.Resource != resource)))
                            {
                                continue;
                            }

                            // If we have a resource, and it's attached to either the render target or depth/stencil being bound, 
                            // then reset it on the shader stage to avoid a hazard.
                            stage.SetShaderResource(j, null);
                            lastSrvs.ResetAt(j);
                        }
                    }

                    if (lastUavs == null)
                    {
                        continue;
                    }

                    (int uavStart, int uavCount) = lastUavs.GetDirtyItems(true);

                    for (int j = uavStart; j < uavStart + uavCount; ++j)
                    {
                        GorgonGraphicsResource resource = lastUavs[j].ReadWriteView?.Resource;

                        // If we have a resource, and it's attached to either the render target or depth/stencil being bound, 
                        // then reset it on the shader stage to avoid a hazard.
                        if (resource == null)
                        {
                            continue;
                        }

                        if (((rt == null) || (resource != rt))
                            && ((depth == null) || (depth.Resource != resource)))
                        {
                            continue;
                        }

                        D3DDeviceContext.OutputMerger.SetUnorderedAccessView(j, null, -1);
                        lastUavs.ResetAt(j);
                    }
                }
            }
        }

        /// <summary>
        /// Function to check for shader resources that are bound as a input, as well as bound for output.
        /// </summary>
        /// <param name="uavs">The unordered access views to be assigned as inputs.</param>
        private void CheckUavsForRtvSrvHazards(GorgonReadWriteViewBindings uavs)
        {
            (int startIndex, int count) = uavs.GetDirtyItems();

            if (count == 0)
            {
                return;
            }

            for (int i = startIndex; i < startIndex + count; ++i)
            {
                GorgonGraphicsResource uav = uavs[i].ReadWriteView?.Resource;

                if (uav == null)
                {
                    continue;
                }

                // Check for depth/stencil output.
                if (DepthStencilView?.Resource == uav)
                {
                    throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_INPUT_BOUND_AS_DSV, DepthStencilView.Texture.Name));
                }

                // Check for rtv output.
                for (int j = 0; j < _renderTargets.Length; ++j)
                {
                    GorgonGraphicsResource rtv = _renderTargets[j]?.Resource;

                    if ((rtv == null) || (uav != rtv))
                    {
                        continue;
                    }

                    throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_INPUT_BOUND_AS_RTV, rtv.Name));
                }

                if (_lastState == null)
                {
                    continue;
                }

                CheckUavForSrvHazards(uav);
            }
        }

        /// <summary>
        /// Function to check for shader resources that are bound as a input, as well as bound for output.
        /// </summary>
        /// <param name="newSrvs">The shader resource views to be assigned as inputs.</param>
        private void CheckSrvsForRtvUavHazards(GorgonShaderResourceViews newSrvs)
        {
            (int startIndex, int count) = newSrvs.GetDirtyItems();

            if (count == 0)
            {
                return;
            }

            for (int i = startIndex; i < startIndex + count; ++i)
            {
                GorgonGraphicsResource srv = newSrvs[i]?.Resource;

                if (srv == null)
                {
                    continue;
                }

                // Check for depth/stencil output.
                if (DepthStencilView?.Resource == srv)
                {
                    throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_INPUT_BOUND_AS_DSV, DepthStencilView.Texture.Name));
                }

                // Check for rtv output.
                for (int j = 0; j < _renderTargets.Length; ++j)
                {
                    GorgonGraphicsResource rtv = _renderTargets[j]?.Resource;

                    if ((rtv == null) || (rtv.ResourceType == GraphicsResourceType.Buffer) || (rtv.ResourceType == GraphicsResourceType.Texture1D) || (srv != rtv))
                    {
                        continue;
                    }

                    throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_INPUT_BOUND_AS_RTV, rtv.Name));
                }

                // Scan for both the Compute Shader and general UAVs and disconnect if there's a match.
                for (int rw = 0; rw < 2; ++rw)
                {
                    GorgonReadWriteViewBindings uavs = i == 1 ? _lastState?.CsReadWriteViews : _lastState?.ReadWriteViews;

                    if (uavs == null)
                    {
                        continue;
                    }

                    (int uavStart, int uavCount) = uavs.GetDirtyItems();

                    for (int j = uavStart; j < uavStart + uavCount; ++j)
                    {
                        GorgonGraphicsResource uav = uavs[j].ReadWriteView?.Resource;

                        if ((uav == null) || (uav != srv))
                        {
                            continue;
                        }

                        uavs.ResetAt(i);

                        // Unbind from the unordered access views if we have the SRV already mapped as an output.
                        if (i == 0)
                        {
                            D3DDeviceContext.OutputMerger.SetUnorderedAccessView(i, null, -1);
                        }
                        else
                        {
                            D3DDeviceContext.ComputeShader.SetUnorderedAccessView(i, null, -1);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Function to bind the shader resource views to a specific shader stage.
        /// </summary>
        /// <param name="shaderType">The shader stage.</param>
        /// <param name="srvs">The shader resource views to bind.</param>
        private void BindSrvs(ShaderType shaderType, GorgonShaderResourceViews srvs)
        {
            if (srvs == null)
            {
                switch (shaderType)
                {
                    case ShaderType.Vertex:
                        D3DDeviceContext.VertexShader.SetShaderResources(0, _emptySrvs);
                        break;
                    case ShaderType.Pixel:
                        D3DDeviceContext.PixelShader.SetShaderResources(0, _emptySrvs);
                        break;
                    case ShaderType.Geometry:
                        D3DDeviceContext.GeometryShader.SetShaderResources(0, _emptySrvs);
                        break;
                    case ShaderType.Domain:
                        D3DDeviceContext.DomainShader.SetShaderResources(0, _emptySrvs);
                        break;
                    case ShaderType.Hull:
                        D3DDeviceContext.HullShader.SetShaderResources(0, _emptySrvs);
                        break;
                    case ShaderType.Compute:
                        D3DDeviceContext.ComputeShader.SetShaderResources(0, _emptySrvs);
                        break;
                }

                return;
            }

            (int start, int count) = srvs.GetDirtyItems();

            D3D11.ShaderResourceView[] states = srvs.Native;

            if (count == 0)
            {
                states = _emptySrvs;
                count = 1;
            }

            switch (shaderType)
            {
                case ShaderType.Vertex:
                    D3DDeviceContext.VertexShader.SetShaderResources(start, count, states);
                    break;
                case ShaderType.Pixel:
                    D3DDeviceContext.PixelShader.SetShaderResources(start, count, states);
                    break;
                case ShaderType.Geometry:
                    D3DDeviceContext.GeometryShader.SetShaderResources(start, count, states);
                    break;
                case ShaderType.Domain:
                    D3DDeviceContext.DomainShader.SetShaderResources(start, count, states);
                    break;
                case ShaderType.Hull:
                    D3DDeviceContext.HullShader.SetShaderResources(start, count, states);
                    break;
                case ShaderType.Compute:
                    D3DDeviceContext.ComputeShader.SetShaderResources(start, count, states);
                    break;
            }
        }

        /// <summary>
        /// Function to bind resources to the pipeline.
        /// </summary>
        /// <param name="resourceChanges">The changes for the resources.</param>
        private void BindResources(DrawCallChanges resourceChanges)
        {
            if (resourceChanges == DrawCallChanges.None)
            {
                return;
            }

            // This is ugly as sin, but there's no elegant way to do this that isn't another performance hit.
            if ((resourceChanges & DrawCallChanges.ComputeShader) == DrawCallChanges.ComputeShader)
            {
                D3DDeviceContext.ComputeShader.Set(_lastState.ComputeShader?.NativeShader);
            }

            if ((resourceChanges & DrawCallChanges.StreamOutBuffers) == DrawCallChanges.StreamOutBuffers)
            {
                BindStreamOutBuffers(_lastState.StreamOutBindings);
            }

            if ((resourceChanges & DrawCallChanges.InputLayout) == DrawCallChanges.InputLayout)
            {
                D3DDeviceContext.InputAssembler.InputLayout = _lastState.InputLayout?.D3DInputLayout;
            }

            if ((resourceChanges & DrawCallChanges.VertexBuffers) == DrawCallChanges.VertexBuffers)
            {
                BindVertexBuffers(_lastState.VertexBuffers);
            }

            if ((resourceChanges & DrawCallChanges.IndexBuffer) == DrawCallChanges.IndexBuffer)
            {
                BindIndexBuffer(_lastState.IndexBuffer);
            }

            if ((resourceChanges & DrawCallChanges.Uavs) == DrawCallChanges.Uavs)
            {
                BindUavs(_lastState.ReadWriteViews, false);
            }

            if ((resourceChanges & DrawCallChanges.PsSamplers) == DrawCallChanges.PsSamplers)
            {
                BindSamplers(D3DDeviceContext.PixelShader, _lastState.PsSamplers);
            }

            if ((resourceChanges & DrawCallChanges.VsSamplers) == DrawCallChanges.VsSamplers)
            {
                BindSamplers(D3DDeviceContext.VertexShader, _lastState.VsSamplers);
            }

            if ((resourceChanges & DrawCallChanges.GsSamplers) == DrawCallChanges.GsSamplers)
            {
                BindSamplers(D3DDeviceContext.GeometryShader, _lastState.GsSamplers);
            }

            if ((resourceChanges & DrawCallChanges.DsSamplers) == DrawCallChanges.DsSamplers)
            {
                BindSamplers(D3DDeviceContext.DomainShader, _lastState.DsSamplers);
            }

            if ((resourceChanges & DrawCallChanges.HsSamplers) == DrawCallChanges.HsSamplers)
            {
                BindSamplers(D3DDeviceContext.HullShader, _lastState.HsSamplers);
            }

            if ((resourceChanges & DrawCallChanges.CsSamplers) == DrawCallChanges.CsSamplers)
            {
                BindSamplers(D3DDeviceContext.ComputeShader, _lastState.CsSamplers);
            }

            if ((resourceChanges & DrawCallChanges.VsConstants) == DrawCallChanges.VsConstants)
            {
                BindConstantBuffers(ShaderType.Vertex, _lastState.VsConstantBuffers);
            }

            if ((resourceChanges & DrawCallChanges.PsConstants) == DrawCallChanges.PsConstants)
            {
                BindConstantBuffers(ShaderType.Pixel, _lastState.PsConstantBuffers);
            }

            if ((resourceChanges & DrawCallChanges.GsConstants) == DrawCallChanges.GsConstants)
            {
                BindConstantBuffers(ShaderType.Geometry, _lastState.GsConstantBuffers);
            }

            if ((resourceChanges & DrawCallChanges.DsConstants) == DrawCallChanges.DsConstants)
            {
                BindConstantBuffers(ShaderType.Domain, _lastState.DsConstantBuffers);
            }

            if ((resourceChanges & DrawCallChanges.HsConstants) == DrawCallChanges.HsConstants)
            {
                BindConstantBuffers(ShaderType.Hull, _lastState.HsConstantBuffers);
            }

            if ((resourceChanges & DrawCallChanges.CsConstants) == DrawCallChanges.CsConstants)
            {
                BindConstantBuffers(ShaderType.Compute, _lastState.CsConstantBuffers);
            }

            if ((resourceChanges & DrawCallChanges.VsResourceViews) == DrawCallChanges.VsResourceViews)
            {
                BindSrvs(ShaderType.Vertex, _lastState.VsSrvs);
            }

            if ((resourceChanges & DrawCallChanges.PsResourceViews) == DrawCallChanges.PsResourceViews)
            {
                BindSrvs(ShaderType.Pixel, _lastState.PsSrvs);
            }

            if ((resourceChanges & DrawCallChanges.GsResourceViews) == DrawCallChanges.GsResourceViews)
            {
                BindSrvs(ShaderType.Geometry, _lastState.GsSrvs);
            }

            if ((resourceChanges & DrawCallChanges.DsResourceViews) == DrawCallChanges.DsResourceViews)
            {
                BindSrvs(ShaderType.Domain, _lastState.DsSrvs);
            }

            if ((resourceChanges & DrawCallChanges.HsResourceViews) == DrawCallChanges.HsResourceViews)
            {
                BindSrvs(ShaderType.Hull, _lastState.HsSrvs);
            }

            if ((resourceChanges & DrawCallChanges.CsResourceViews) == DrawCallChanges.CsResourceViews)
            {
                BindSrvs(ShaderType.Compute, _lastState.CsSrvs);
            }

            if ((resourceChanges & DrawCallChanges.CsUavs) == DrawCallChanges.CsUavs)
            {
                BindUavs(_lastState.CsReadWriteViews, true);
            }
        }

        /// <summary>
        /// Function to update a shader resource list on the by copying the dirty index data from one list to another.
        /// </summary>
        /// <param name="destList">The list that will receive the dirty entries from the source.</param>
        /// <param name="srcList">The source list that will be copied.</param>
        private static void MergeShaderResourceList(GorgonShaderResourceViews destList, GorgonShaderResourceViews srcList)
        {
            Debug.Assert(srcList != null && destList != null, "One of the resource lists passed should not be NULL. Something's wrong here.");

            (int destStart, int destCount) = destList.GetDirtyItems();
            (int srcStart, int srcCount) = srcList.GetDirtyItems();

            int destEnd = destStart + destCount;
            int srcEnd = srcStart + srcCount;

            // Determine the largest range to copy.
            int end = destEnd.Max(srcEnd).Min(destList.Length).Min(srcList.Length);
            int start = destStart.Min(srcStart).Min(destList.Length - 1).Min(srcList.Length - 1).Max(0);

            for (int i = start; i < end; ++i)
            {
                destList[i] = srcList[i];
            }
        }

        /// <summary>
        /// Function to update a resource list on the by copying the dirty index data from one list to another.
        /// </summary>
        /// <typeparam name="T">The type of resource list, must inherit from <see cref="GorgonArray{TE}"/></typeparam> 
        /// <typeparam name="TE">The type of element in the resource list.</typeparam>
        /// <param name="destList">The list that will receive the dirty entries from the source.</param>
        /// <param name="srcList">The source list that will be copied.</param>
        private static void MergeResourceList<T, TE>(T destList, T srcList)
            where T : GorgonArray<TE>
            where TE : IEquatable<TE>
        {
            Debug.Assert(srcList != null && destList != null, "One of the resource lists passed should not be NULL. Something's wrong here.");

            (int destStart, int destCount) = destList.GetDirtyItems();
            (int srcStart, int srcCount) = srcList.GetDirtyItems();

            int destEnd = destStart + destCount;
            int srcEnd = srcStart + srcCount;

            // Determine the largest range to copy.
            int end = destEnd.Max(srcEnd).Min(destList.Length).Min(srcList.Length);
            int start = destStart.Min(srcStart).Min(destList.Length - 1).Min(srcList.Length - 1).Max(0);

            for (int i = start; i < end; ++i)
            {
                destList[i] = srcList[i];
            }
        }

        /// <summary>
        /// Function to retrieve whether the resources have any changes or not since the last draw call.
        /// </summary>
        /// <typeparam name="T">The type of resource in the list.</typeparam>
        /// <param name="array">The array containing the list.</param>
        /// <param name="changeType">The type of expected change.</param>
        /// <param name="currentChanges">The current set of changes.</param>
        /// <returns>An amended change set, or the changeset provided by <paramref name="currentChanges"/> if nothing was changed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DrawCallChanges GetResourceChanges<T>(IGorgonReadOnlyArray<T> array, DrawCallChanges changeType, DrawCallChanges currentChanges)
            where T : IEquatable<T> => array.IsDirty ? (currentChanges | changeType) : currentChanges;

        /// <summary>
        /// Function to build a merged draw state.
        /// </summary>
        /// <param name="currentState">The current state.</param>
        /// <param name="pipelineChanges">The pipeline state changes that were applied.</param>
        /// <returns>A set of changes that need to be applied to the pipeline.</returns>
        private DrawCallChanges BuildDrawCallResources(D3DState currentState, DrawCallChanges pipelineChanges)
        {
            DrawCallChanges changes = DrawCallChanges.None;

            Debug.Assert(currentState.HsSamplers != null, "HullShader samplers is null - This is not allowed.");
            Debug.Assert(currentState.HsSrvs != null, "HullShader srvs are null - This is now allowed.");
            Debug.Assert(currentState.HsConstantBuffers != null, "HullShader constants is null - This is now allowed.");
            Debug.Assert(currentState.DsSamplers != null, "DomainShader samplers is null - This is not allowed.");
            Debug.Assert(currentState.DsSrvs != null, "DomainShader srvs are null - This is now allowed.");
            Debug.Assert(currentState.DsConstantBuffers != null, "DomainShader constants is null - This is now allowed.");
            Debug.Assert(currentState.GsSamplers != null, "GeometryShader samplers is null - This is not allowed.");
            Debug.Assert(currentState.GsSrvs != null, "GeometryShader srvs are null - This is now allowed.");
            Debug.Assert(currentState.GsConstantBuffers != null, "GeometryShader constants is null - This is now allowed.");
            Debug.Assert(currentState.PsConstantBuffers != null, "PixelShader constants is null - This is now allowed.");
            Debug.Assert(currentState.PsSamplers != null, "PixelShader samplers is null - This is not allowed.");
            Debug.Assert(currentState.PsSrvs != null, "PixelShader srvs are null - This is now allowed.");
            Debug.Assert(currentState.VsSamplers != null, "VertexShader samplers is null - This is not allowed.");
            Debug.Assert(currentState.VsSrvs != null, "VertexShader srvs are null - This is not allowed.");
            Debug.Assert(currentState.VsConstantBuffers != null, "VertexShader constants is null - This is not allowed.");
            Debug.Assert(currentState.ReadWriteViews != null, "Read/write views is null - This is not allowed.");
            Debug.Assert(currentState.StreamOutBindings != null, "StreamOut bindings on current draw state is null - This is not allowed.");
            Debug.Assert(currentState.VertexBuffers != null, "VertexBuffers on current draw state is null - This is not allowed.");

            // Ensure we have an input layout.
            if (currentState.InputLayout != _lastState.InputLayout)
            {
                changes |= DrawCallChanges.InputLayout;
                _lastState.VertexBuffers.InputLayout = currentState.InputLayout;
            }

            // Copy the resources.
            CheckVertexBuffersForStreamOutHazards(currentState.VertexBuffers);
            CheckStreamOutBuffersForVertexIndexBufferHazards(currentState.StreamOutBindings);

            MergeResourceList<GorgonVertexBufferBindings, GorgonVertexBufferBinding>(_lastState.VertexBuffers, currentState.VertexBuffers);
            MergeResourceList<GorgonStreamOutBindings, GorgonStreamOutBinding>(_lastState.StreamOutBindings, currentState.StreamOutBindings);

            if (currentState.IndexBuffer != _lastState.IndexBuffer)
            {
                changes |= DrawCallChanges.IndexBuffer;
                _lastState.IndexBuffer = currentState.IndexBuffer;
            }

            changes = GetResourceChanges(_lastState.VertexBuffers, DrawCallChanges.VertexBuffers, changes);
            changes = GetResourceChanges(_lastState.StreamOutBindings, DrawCallChanges.StreamOutBuffers, changes);

            // Check vertex shader resources if we have a vertex shader assigned.
            if ((currentState.PipelineState.VertexShader != null)
                || ((pipelineChanges & DrawCallChanges.VertexShader) == DrawCallChanges.VertexShader))
            {
                CheckSrvsForRtvUavHazards(currentState.VsSrvs);

                MergeResourceList<GorgonConstantBuffers, GorgonConstantBufferView>(_lastState.VsConstantBuffers, currentState.VsConstantBuffers);
                MergeShaderResourceList(_lastState.VsSrvs, currentState.VsSrvs);
                MergeResourceList<GorgonSamplerStates, GorgonSamplerState>(_lastState.VsSamplers, currentState.VsSamplers);

                changes = GetResourceChanges(_lastState.VsConstantBuffers, DrawCallChanges.VsConstants, changes);
                changes = GetResourceChanges(_lastState.VsSrvs, DrawCallChanges.VsResourceViews, changes);
                changes = GetResourceChanges(_lastState.VsSamplers, DrawCallChanges.VsSamplers, changes);
            }

            // Check for pixel shader resources if we have or had a pixel shader assigned.
            if ((currentState.PipelineState.PixelShader != null)
                || ((pipelineChanges & DrawCallChanges.PixelShader) == DrawCallChanges.PixelShader))
            {
                CheckSrvsForRtvUavHazards(currentState.PsSrvs);

                MergeShaderResourceList(_lastState.PsSrvs, currentState.PsSrvs);
                MergeResourceList<GorgonConstantBuffers, GorgonConstantBufferView>(_lastState.PsConstantBuffers, currentState.PsConstantBuffers);
                MergeResourceList<GorgonSamplerStates, GorgonSamplerState>(_lastState.PsSamplers, currentState.PsSamplers);

                changes = GetResourceChanges(_lastState.PsSrvs, DrawCallChanges.PsResourceViews, changes);
                changes = GetResourceChanges(_lastState.PsConstantBuffers, DrawCallChanges.PsConstants, changes);
                changes = GetResourceChanges(_lastState.PsSamplers, DrawCallChanges.PsSamplers, changes);
            }

            // Check geometry shader resources if we have or had a geometry shader assigned.
            if ((currentState.PipelineState.GeometryShader != null)
                || ((pipelineChanges & DrawCallChanges.GeometryShader) == DrawCallChanges.GeometryShader))
            {
                CheckSrvsForRtvUavHazards(currentState.GsSrvs);

                MergeShaderResourceList(_lastState.GsSrvs, currentState.GsSrvs);
                MergeResourceList<GorgonConstantBuffers, GorgonConstantBufferView>(_lastState.GsConstantBuffers, currentState.GsConstantBuffers);
                MergeResourceList<GorgonSamplerStates, GorgonSamplerState>(_lastState.GsSamplers, currentState.GsSamplers);

                changes = GetResourceChanges(_lastState.GsSrvs, DrawCallChanges.GsResourceViews, changes);
                changes = GetResourceChanges(_lastState.GsConstantBuffers, DrawCallChanges.GsConstants, changes);
                changes = GetResourceChanges(_lastState.GsSamplers, DrawCallChanges.GsSamplers, changes);
            }


            // Check domain shader resources if we have or had a domain shader assigned.
            if ((currentState.PipelineState.DomainShader != null)
                || ((pipelineChanges & DrawCallChanges.DomainShader) == DrawCallChanges.DomainShader))
            {
                CheckSrvsForRtvUavHazards(currentState.DsSrvs);

                MergeShaderResourceList(_lastState.DsSrvs, currentState.DsSrvs);
                MergeResourceList<GorgonConstantBuffers, GorgonConstantBufferView>(_lastState.DsConstantBuffers, currentState.DsConstantBuffers);
                MergeResourceList<GorgonSamplerStates, GorgonSamplerState>(_lastState.DsSamplers, currentState.DsSamplers);

                changes = GetResourceChanges(_lastState.DsSrvs, DrawCallChanges.DsResourceViews, changes);
                changes = GetResourceChanges(_lastState.DsConstantBuffers, DrawCallChanges.DsConstants, changes);
                changes = GetResourceChanges(_lastState.DsSamplers, DrawCallChanges.DsSamplers, changes);
            }

            // Check hull shader resources if we have or had a hull shader assigned.
            if ((currentState.PipelineState.HullShader != null)
                || ((pipelineChanges & DrawCallChanges.HullShader) == DrawCallChanges.HullShader))
            {
                CheckSrvsForRtvUavHazards(currentState.HsSrvs);

                MergeShaderResourceList(_lastState.HsSrvs, currentState.HsSrvs);
                MergeResourceList<GorgonConstantBuffers, GorgonConstantBufferView>(_lastState.HsConstantBuffers, currentState.HsConstantBuffers);
                MergeResourceList<GorgonSamplerStates, GorgonSamplerState>(_lastState.HsSamplers, currentState.HsSamplers);

                changes = GetResourceChanges(_lastState.HsSrvs, DrawCallChanges.HsResourceViews, changes);
                changes = GetResourceChanges(_lastState.HsConstantBuffers, DrawCallChanges.HsConstants, changes);
                changes = GetResourceChanges(_lastState.HsSamplers, DrawCallChanges.HsSamplers, changes);
            }

            CheckUavsForRtvSrvHazards(currentState.ReadWriteViews);
            MergeResourceList<GorgonReadWriteViewBindings, GorgonReadWriteViewBinding>(_lastState.ReadWriteViews, currentState.ReadWriteViews);
            changes = GetResourceChanges(_lastState.ReadWriteViews, DrawCallChanges.Uavs, changes);

            // Check compute shader resources if we have or had a compute shader assigned.
            if (_lastState.ComputeShader != currentState.ComputeShader)
            {
                _lastState.ComputeShader = currentState.ComputeShader;
                changes |= DrawCallChanges.ComputeShader;
            }

            // If we turned off the compute shader, and it was done in a prior call, then there's no need to execute the code below (we hope).
            if ((currentState.ComputeShader == null) && ((changes & DrawCallChanges.ComputeShader) != DrawCallChanges.ComputeShader))
            {
                return changes;
            }

            CheckSrvsForRtvUavHazards(currentState.CsSrvs);
            CheckUavsForRtvSrvHazards(currentState.CsReadWriteViews);

            MergeResourceList<GorgonReadWriteViewBindings, GorgonReadWriteViewBinding>(_lastState.CsReadWriteViews, currentState.CsReadWriteViews);
            MergeResourceList<GorgonConstantBuffers, GorgonConstantBufferView>(_lastState.CsConstantBuffers, currentState.CsConstantBuffers);
            MergeResourceList<GorgonSamplerStates, GorgonSamplerState>(_lastState.CsSamplers, currentState.CsSamplers);
            MergeShaderResourceList(_lastState.CsSrvs, currentState.CsSrvs);

            changes = GetResourceChanges(_lastState.CsReadWriteViews, DrawCallChanges.CsUavs, changes);
            changes = GetResourceChanges(_lastState.CsReadWriteViews, DrawCallChanges.CsResourceViews, changes);
            changes = GetResourceChanges(_lastState.CsReadWriteViews, DrawCallChanges.CsConstants, changes);
            changes = GetResourceChanges(_lastState.CsReadWriteViews, DrawCallChanges.CsSamplers, changes);

            return changes;
        }

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
                        DebugName =
                                                         $"'{adapterInfo.Name}' D3D11.4 {(adapterInfo.VideoDeviceType == VideoDeviceType.Software ? "Software Adapter" : "Adapter")}"
                    })
                    {
                        resultDevice = device.QueryInterface<D3D11.Device5>();

                        Log.Print($"Direct 3D 11.4 device created for video adapter '{adapterInfo.Name}' at feature set [{(FeatureSet)resultDevice.FeatureLevel}]",
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
        /// Function to validate the depth/stencil view.
        /// </summary>
        /// <param name="view">The depth/stencil view to evaluate.</param>
        /// <param name="firstTarget">The first non-null target.</param>
        private void ValidateRtvAndDsv(GorgonDepthStencil2DView view, GorgonRenderTargetView firstTarget)
        {
            if ((firstTarget == null)
                && (view == null))
            {
                return;
            }

            if (firstTarget != null)
            {
                // Ensure that we are only bound once to the pipeline.
                if (RenderTargets.Count(item => item == firstTarget) > 1)
                {
                    throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_ALREADY_BOUND);
                }

                // Ensure our dimensions match, and multi-sample settings match.
                foreach (GorgonRenderTargetView rtv in RenderTargets.Where(item => (item != null) && (item != firstTarget)))
                {
                    if (rtv.Resource.ResourceType != firstTarget.Resource.ResourceType)
                    {
                        throw new GorgonException(GorgonResult.CannotBind,
                                                  string.Format(Resources.GORGFX_ERR_RTV_NOT_SAME_TYPE, firstTarget.Resource.ResourceType));
                    }

                    switch (firstTarget.Resource.ResourceType)
                    {
                        case GraphicsResourceType.Texture2D:
                            var left2D = (GorgonRenderTarget2DView)firstTarget;
                            var right2D = (GorgonRenderTarget2DView)rtv;

                            if ((left2D.Width != right2D.Width) && (left2D.Height != right2D.Height) && (left2D.ArrayCount != right2D.ArrayCount))
                            {
                                throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_RESOURCE_MISMATCH);
                            }

                            if (!left2D.MultisampleInfo.Equals(right2D.MultisampleInfo))
                            {
                                throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_MULTISAMPLE_MISMATCH);
                            }

                            break;
                        case GraphicsResourceType.Texture3D:
                            var left3D = (GorgonRenderTarget3DView)firstTarget;
                            var right3D = (GorgonRenderTarget3DView)rtv;

                            if ((left3D.Width != right3D.Width) && (left3D.Height != right3D.Height) && (left3D.Depth != right3D.Depth))
                            {
                                throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_RESOURCE_MISMATCH);
                            }

                            break;
                        default:
                            throw new GorgonException(GorgonResult.CannotBind,
                                                      string.Format(Resources.GORGFX_ERR_RTV_UNSUPPORTED_RESOURCE, firstTarget.Resource.ResourceType));
                    }
                }
            }

            if ((firstTarget == null)
                || (view == null))
            {
                return;
            }

            // Ensure all resources are the same type.
            if (view.Texture.ResourceType != firstTarget.Resource.ResourceType)
            {
                throw new GorgonException(GorgonResult.CannotBind,
                                          string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_TYPE_MISMATCH, view.Texture.ResourceType));
            }

            var rtv2D = (GorgonRenderTarget2DView)firstTarget;

            // Ensure the depth stencil array/depth counts match for all resources.
            if (view.ArrayCount != rtv2D.ArrayCount)
            {
                throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_ARRAYCOUNT_MISMATCH, view.Texture.Name));
            }

            // Check to ensure that multisample info matches.
            if (!view.Texture.MultisampleInfo.Equals(rtv2D.MultisampleInfo))
            {
                throw new GorgonException(GorgonResult.CannotBind,
                                          string.Format(Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_MULTISAMPLE_MISMATCH,
                                                        view.MultisampleInfo.Quality,
                                                        view.MultisampleInfo.Count));
            }

            if ((view.Width != rtv2D.Width)
                || (view.Height != rtv2D.Height))
            {
                throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_ERR_RTV_DEPTHSTENCIL_RESOURCE_MISMATCH);
            }
        }

        /// <summary>
        /// Function to initialize a <see cref="GorgonPipelineState" /> object with Direct 3D 11 state objects by creating new objects for the unassigned values.
        /// </summary>
        /// <param name="pipelineState">The pipeline state.</param>
        /// <param name="blendState">An existing blend state to use.</param>
        /// <param name="depthStencilState">An existing depth/stencil state to use.</param>
        /// <param name="rasterState">An existing rasterizer state to use.</param>
        /// <returns>A new <see cref="GorgonPipelineState"/>.</returns>
        private void InitializePipelineState(GorgonPipelineState pipelineState,
                                             D3D11.BlendState1 blendState,
                                             D3D11.DepthStencilState depthStencilState,
                                             D3D11.RasterizerState1 rasterState)
        {
            pipelineState.D3DRasterState = rasterState;
            pipelineState.D3DBlendState = blendState;
            pipelineState.D3DDepthStencilState = depthStencilState;

            if ((rasterState == null) && (pipelineState.RasterState != null))
            {
                pipelineState.D3DRasterState = pipelineState.RasterState.GetD3D11RasterState(_d3DDevice);
            }

            if ((depthStencilState == null) && (pipelineState.DepthStencilState != null))
            {
                pipelineState.D3DDepthStencilState = pipelineState.DepthStencilState.GetD3D11DepthStencilState(_d3DDevice);
            }

            if (blendState == null)
            {
                pipelineState.BuildD3D11BlendState(D3DDevice);
            }
        }

        /// <summary>
        /// Function to fire the <see cref="ViewportChanging"/> event.
        /// </summary>
        /// <returns><b>true</b> if cancelled, <b>false</b> if not.</returns>>
        private bool OnViewportChanging()
        {
            CancelEventHandler cancelHandler = ViewportChanging;

            if (cancelHandler == null)
            {
                return false;
            }

            var cancelArgs = new CancelEventArgs();
            cancelHandler(this, cancelArgs);

            return cancelArgs.Cancel;
        }

        /// <summary>
        /// Function to fire the <see cref="RenderTargetChanging"/> event.
        /// </summary>
        private void OnRenderTargetChanging()
        {
            CancelEventHandler cancelHandler = RenderTargetChanging;

            if ((!_isTargetUpdated.RtvsChanged) || (cancelHandler == null))
            {
                return;
            }

            var cancelArgs = new CancelEventArgs();
            cancelHandler(this, cancelArgs);

            if (cancelArgs.Cancel)
            {
                _isTargetUpdated.RtvsChanged = false;
            }
        }

        /// <summary>
        /// Function to fire the <see cref="DepthStencilChanging"/> event.
        /// </summary>
        private void OnDepthStencilChanging()
        {
            CancelEventHandler cancelHandler = DepthStencilChanging;

            if ((!_isTargetUpdated.DepthViewChanged) || (cancelHandler == null))
            {
                return;
            }

            var cancelArgs = new CancelEventArgs();
            cancelHandler(this, cancelArgs);

            if (cancelArgs.Cancel)
            {
                _isTargetUpdated.DepthViewChanged = false;
            }
        }

        /// <summary>
        /// Function to assign the render targets.
        /// </summary>
        /// <param name="rtvCount">The number of render targets to update.</param>
        private void SetRenderTargetAndDepthViews(int rtvCount)
        {
#if DEBUG
            ValidateRtvAndDsv(DepthStencilView, RenderTargets.FirstOrDefault(item => item != null));
#endif

            if (rtvCount == 0)
            {
                Array.Clear(_d3DRtvs, 0, _d3DRtvs.Length);
                rtvCount = _d3DRtvs.Length;
            }
            else
            {
                for (int i = 0; i < rtvCount; ++i)
                {
                    _d3DRtvs[i] = _renderTargets[i]?.Native;
                }
            }

            D3DDeviceContext.OutputMerger.SetTargets(DepthStencilView?.Native, rtvCount, _d3DRtvs);

            EventHandler handler;
            if (_isTargetUpdated.RtvsChanged)
            {
                handler = RenderTargetChanged;
                handler?.Invoke(this, EventArgs.Empty);
            }

            if (_isTargetUpdated.DepthViewChanged)
            {
                handler = DepthStencilChanged;
                handler?.Invoke(this, EventArgs.Empty);
            }

            _isTargetUpdated = (false, false);
        }

        /// <summary>
        /// Function to set up drawing states.
        /// </summary>
        /// <param name="state">The state to evaluate and apply.</param>
        /// <param name="factor">The current blend factor.</param>
        /// <param name="blendSampleMask">The blend sample mask.</param>
        /// <param name="depthStencilReference">The depth stencil reference.</param>
        private void SetDrawStates(D3DState state, GorgonColor factor, int blendSampleMask, int depthStencilReference)
        {
            if (!factor.Equals(in _blendFactor))
            {
                _blendFactor = factor;
                D3DDeviceContext.OutputMerger.BlendFactor = _blendFactor.ToRawColor4();
            }

            if (blendSampleMask != _blendSampleMask)
            {
                _blendSampleMask = blendSampleMask;
                D3DDeviceContext.OutputMerger.BlendSampleMask = _blendSampleMask;
            }

            if (depthStencilReference != _depthStencilReference)
            {
                _depthStencilReference = depthStencilReference;
                D3DDeviceContext.OutputMerger.DepthStencilReference = _depthStencilReference;
            }

            // If the pipeline is the same as last time, then don't even bother with changing states.
            DrawCallChanges stateChanges = DrawCallChanges.None;

            if (_lastState.PipelineState != state.PipelineState)
            {
                stateChanges = BuildStateChanges(state.PipelineState);
                ApplyState(_lastState.PipelineState, stateChanges);
            }

            DrawCallChanges changes = BuildDrawCallResources(state, stateChanges);
            BindResources(changes);
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
        /// Function to build up a <see cref="GorgonSamplerState"/> object with its corresponding Direct3D 11 state object by either creating a new object, or inheriting a previous one.
        /// </summary>
        /// <param name="newState">The new state to initialize.</param>
        /// <returns>If the state matches a cached state, then the cached state is returned, otherwise a copy of the the <paramref name="newState"/> is returned.</returns>
        internal GorgonSamplerState CacheSamplerState(GorgonSamplerState newState)
        {
            lock (_samplerLock)
            {
                if (newState.ID != int.MinValue)
                {
                    return _cachedSamplers[newState.ID];
                }

                for (int i = 0; i < _cachedSamplers.Count; ++i)
                {
                    GorgonSamplerState cached = _cachedSamplers[i];

                    if (cached?.Equals(newState) ?? false)
                    {
                        return cached;
                    }
                }

                // We didn't find what we wanted, so create a new one.
                var resultState = new GorgonSamplerState(newState)
                {
                    ID = _cachedSamplers.Count
                };
                resultState.BuildD3D11SamplerState(D3DDevice);
                _cachedSamplers.Add(resultState);

                return resultState;
            }
        }

        /// <summary>
        /// Function to build up a <see cref="GorgonPipelineState"/> object with Direct 3D 11 state objects by either creating new objects, or inheriting previous ones.
        /// </summary>
        /// <param name="newState">The new state to initialize.</param>
        /// <returns>If the pipeline state matches a cached pipeline state, then the cached state is returned, otherwise a copy of the <paramref name="newState"/> is returned.</returns>
        internal GorgonPipelineState CachePipelineState(GorgonPipelineState newState)
        {
            // Existing states.
            D3D11.DepthStencilState depthStencilState = null;
            D3D11.BlendState1 blendState = null;
            D3D11.RasterizerState1 rasterState = null;

            lock (_stateLock)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < _cachedPipelineStates.Count; ++i)
                {
                    // Compute shaders don't use pipeline state.  So, we assume the compute shader is equal at all times.
                    DrawCallChanges inheritedState = DrawCallChanges.None;
                    GorgonPipelineState cachedState = _cachedPipelineStates[i];

                    if (cachedState.PrimitiveType == newState.PrimitiveType)
                    {
                        inheritedState |= DrawCallChanges.Topology;
                    }

                    if (cachedState.VertexShader == newState.VertexShader)
                    {
                        inheritedState |= DrawCallChanges.VertexShader;
                    }

                    if (cachedState.PixelShader == newState.PixelShader)
                    {
                        inheritedState |= DrawCallChanges.PixelShader;
                    }

                    if (cachedState.GeometryShader == newState.GeometryShader)
                    {
                        inheritedState |= DrawCallChanges.GeometryShader;
                    }

                    if (cachedState.DomainShader == newState.DomainShader)
                    {
                        inheritedState |= DrawCallChanges.DomainShader;
                    }

                    if (cachedState.HullShader == newState.HullShader)
                    {
                        inheritedState |= DrawCallChanges.HullShader;
                    }

                    if (cachedState.RasterState.Equals(newState.RasterState))
                    {
                        rasterState = cachedState.D3DRasterState;
                        inheritedState |= DrawCallChanges.RasterState;
                    }

                    if ((cachedState.RwBlendStates.Equals(newState.RwBlendStates))
                        && (cachedState.IsAlphaToCoverageEnabled == newState.IsAlphaToCoverageEnabled)
                        && (cachedState.IsIndependentBlendingEnabled == newState.IsIndependentBlendingEnabled))
                    {
                        blendState = cachedState.D3DBlendState;
                        inheritedState |= DrawCallChanges.BlendState;
                    }

                    if ((cachedState.DepthStencilState != null) &&
                        (cachedState.DepthStencilState.Equals(newState.DepthStencilState)))
                    {
                        depthStencilState = cachedState.D3DDepthStencilState;
                        inheritedState |= DrawCallChanges.DepthStencilState;
                    }

                    if ((CompareScissorRects(cachedState.RasterState.ScissorRectangles, newState.RasterState.ScissorRectangles)))
                    {
                        inheritedState |= DrawCallChanges.Scissors;
                    }

                    // We've copied all the states, so just return the existing pipeline state.
                    // ReSharper disable once InvertIf
                    if (inheritedState == DrawCallChanges.AllPipelineState)
                    {
                        return cachedState;
                    }
                }

                // Setup any uninitialized states.
                var resultState = new GorgonPipelineState(newState);
                InitializePipelineState(resultState, blendState, depthStencilState, rasterState);
                resultState.ID = _cachedPipelineStates.Count;
                _cachedPipelineStates.Add(resultState);
                return resultState;
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

            SetDrawStates(dispatchCall.D3DState, _blendFactor, _blendSampleMask, _depthStencilReference);
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

            SetDrawStates(dispatchCall.D3DState, _blendFactor, _blendSampleMask, _depthStencilReference);
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
        public void ClearState(bool flush = false)
        {
            // Reset state on the device context.
            D3DDeviceContext.ClearState();

            if (flush)
            {
                D3DDeviceContext.Flush();
            }

            ClearResourceCaches();

            _lastState.VertexBuffers.Clear();
            _lastState.StreamOutBindings.Clear();
            _lastState.IndexBuffer = null;
            _lastState.PipelineState.Clear();
            _lastState.ReadWriteViews.Clear();
            _lastState.PsSamplers.Clear();
            _lastState.VsSamplers.Clear();
            _lastState.GsSamplers.Clear();
            _lastState.DsSamplers.Clear();
            _lastState.HsSamplers.Clear();
            _lastState.CsSamplers.Clear();
            _lastState.VsSrvs.Clear();
            _lastState.PsSrvs.Clear();
            _lastState.GsSrvs.Clear();
            _lastState.DsSrvs.Clear();
            _lastState.HsSrvs.Clear();
            _lastState.CsSrvs.Clear();
            _lastState.VsConstantBuffers.Clear();
            _lastState.PsConstantBuffers.Clear();
            _lastState.GsConstantBuffers.Clear();
            _lastState.DsConstantBuffers.Clear();
            _lastState.HsConstantBuffers.Clear();
            _lastState.CsConstantBuffers.Clear();
            _lastState.CsReadWriteViews.Clear();

            _depthStencilReference = 0;
            _blendFactor = GorgonColor.White;
            _blendSampleMask = int.MinValue;
        }

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
        public void SetDepthStencil(GorgonDepthStencil2DView depthStencil)
        {
            if (depthStencil == DepthStencilView)
            {
                return;
            }

            SetRenderTargets(_renderTargets, depthStencil);
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
        public void SetRenderTarget(GorgonRenderTargetView renderTarget, GorgonDepthStencil2DView depthStencil = null)
        {
            if ((_renderTargets[0] == renderTarget) && (depthStencil == DepthStencilView))
            {
                return;
            }

            _isTargetUpdated = (_renderTargets[0] != renderTarget, DepthStencilView != depthStencil);

            OnRenderTargetChanging();
            OnDepthStencilChanging();

            _renderTargets[0] = renderTarget;
            Array.Clear(_renderTargets, 1, _renderTargets.Length - 1);

            CheckRtvsForSrvUavHazards(_renderTargets, 1, depthStencil);

            DX.ViewportF viewport = default;
            if (_renderTargets[0] != null)
            {
                viewport = new DX.ViewportF(0, 0, _renderTargets[0].Width, _renderTargets[0].Height);
            }

            SetViewport(ref viewport);

            if (_isTargetUpdated.DepthViewChanged)
            {
                DepthStencilView = depthStencil;
            }

            SetRenderTargetAndDepthViews(1);
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
        public void SetRenderTargets(GorgonRenderTargetView[] renderTargets, GorgonDepthStencil2DView depthStencil = null)
        {
            if ((renderTargets == null)
                || (renderTargets.Length == 0))
            {
                ClearState();

                _isTargetUpdated = (true, true);

                OnRenderTargetChanging();
                OnDepthStencilChanging();

                if (_isTargetUpdated.RtvsChanged)
                {
                    Array.Clear(_renderTargets, 0, _renderTargets.Length);
                }

                if (_isTargetUpdated.DepthViewChanged)
                {
                    DepthStencilView = depthStencil;
                }

                SetRenderTargetAndDepthViews(0);
                return;
            }

            int rtvCount = renderTargets.Length.Min(_renderTargets.Length);
            _isTargetUpdated = (false, depthStencil != DepthStencilView);
            for (int i = 0; i < rtvCount; ++i)
            {
                if (_renderTargets[i] == renderTargets[i])
                {
                    continue;
                }

                _isTargetUpdated = (true, depthStencil != DepthStencilView);
                break;
            }

            OnRenderTargetChanging();

            if ((!_isTargetUpdated.DepthViewChanged) && (!_isTargetUpdated.RtvsChanged))
            {
                return;
            }

            // We have either a render target change or depth/stencil change.  Either way, we need to clear state to avoid hazards.
            CheckRtvsForSrvUavHazards(renderTargets, rtvCount, depthStencil);

            // These slots will now be empty.
            for (int i = 0; i < rtvCount; ++i)
            {
                _renderTargets[i] = i < rtvCount ? renderTargets[i] : null;
            }

            DX.ViewportF viewport = default;

            if (_renderTargets[0] != null)
            {
                viewport = new DX.ViewportF(0, 0, renderTargets[0].Width, renderTargets[0].Height);
            }

            SetViewport(ref viewport);

            OnDepthStencilChanging();

            if (_isTargetUpdated.DepthViewChanged)
            {
                DepthStencilView = depthStencil;
            }

            SetRenderTargetAndDepthViews(rtvCount);
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
        public void SetViewport(ref DX.ViewportF viewport)
        {
            ref DX.ViewportF firstViewport = ref _viewports[0];

            if (firstViewport.Equals(ref viewport))
            {
                return;
            }

            if (OnViewportChanging())
            {
                return;
            }

            _viewports[0] = viewport;
            Array.Clear(_viewports, 1, _viewports.Length - 1);
            D3DDeviceContext.Rasterizer.SetViewport(viewport.X, viewport.Y, viewport.Width, viewport.Height, viewport.MinDepth, viewport.MaxDepth);

            EventHandler handler = ViewportChanged;
            handler?.Invoke(this, EventArgs.Empty);
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
        public void SetViewports(DX.ViewportF[] viewports)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (viewports == null)
            {
                if (OnViewportChanging())
                {
                    return;
                }

                Array.Clear(_viewports, 0, _viewports.Length);
                D3DDeviceContext.Rasterizer.SetViewport(0, 0, 1, 1);

                EventHandler handler = ViewportChanged;
                handler?.Invoke(this, EventArgs.Empty);
                return;
            }

            bool isChanged = false;
            int viewportCount = viewports.Length.Min(_viewports.Length);

            unsafe
            {
                RawViewportF* viewportPtr = stackalloc RawViewportF[viewportCount];

                if (viewportCount < _viewports.Length)
                {
                    Array.Clear(_viewports, viewportCount, _viewports.Length - viewportCount);
                }

                for (int i = 0; i < viewportCount; ++i)
                {
                    ref DX.ViewportF cachedViewport = ref _viewports[i];
                    ref DX.ViewportF newViewport = ref viewports[i];

                    if (cachedViewport.Equals(ref newViewport))
                    {
                        continue;
                    }

                    viewportPtr[i] = cachedViewport = newViewport;
                    isChanged = true;
                }

                if (!isChanged)
                {
                    return;
                }

                if (OnViewportChanging())
                {
                    return;
                }

                D3DDeviceContext.Rasterizer.SetViewports(viewportPtr, viewportCount);
                EventHandler handler = ViewportChanged;
                handler?.Invoke(this, EventArgs.Empty);
            }
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
        public void ClearStateCache()
        {
            if (D3DDeviceContext != null)
            {
                ClearState();
            }

            ClearStateCache(true);
        }

        /// <summary>
        /// Function to reset the values for per/frame draw call statistics.
        /// </summary>
        /// <seealso cref="DrawCallCount"/>
        public void ResetDrawCallStatistics() => DrawCallCount = 0;

        /// <summary>
        /// Function to submit a basic draw call to the GPU.
        /// </summary>
        /// <param name="drawCall">The draw call to execute.</param>
        /// <param name="blendFactor">[Optional] The factor used to modulate the pixel shader, render target or both.</param>
        /// <param name="blendSampleMask">[Optional] The mask used to define which samples get updated in the active render targets.</param>
        /// <param name="depthStencilReference">[Optional] The depth/stencil reference value used when performing a depth/stencil test.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/> parameter is <b>null</b>.</exception>
        public void Submit(GorgonDrawCall drawCall, GorgonColor? blendFactor = null, int blendSampleMask = int.MinValue, int depthStencilReference = 0)
        {
            drawCall.ValidateObject(nameof(drawCall));
            SetDrawStates(drawCall.D3DState, blendFactor ?? GorgonColor.White, blendSampleMask, depthStencilReference);
            D3DDeviceContext.Draw(drawCall.VertexCount, drawCall.VertexStartIndex);
            ++DrawCallCount;
        }

        /// <summary>
        /// Function to submit a basic, instanced, draw call to the GPU.
        /// </summary>
        /// <param name="drawCall">The draw call to execute.</param>
        /// <param name="blendFactor">[Optional] The factor used to modulate the pixel shader, render target or both.</param>
        /// <param name="blendSampleMask">[Optional] The mask used to define which samples get updated in the active render targets.</param>
        /// <param name="depthStencilReference">[Optional] The depth/stencil reference value used when performing a depth/stencil test.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/> parameter is <b>null</b>.</exception>
        public void Submit(GorgonInstancedCall drawCall, GorgonColor? blendFactor = null, int blendSampleMask = int.MinValue, int depthStencilReference = 0)
        {
            drawCall.ValidateObject(nameof(drawCall));
            SetDrawStates(drawCall.D3DState, blendFactor ?? GorgonColor.White, blendSampleMask, depthStencilReference);
            D3DDeviceContext.DrawInstanced(drawCall.VertexCountPerInstance, drawCall.InstanceCount, drawCall.VertexStartIndex, drawCall.StartInstanceIndex);
            ++DrawCallCount;
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
                           GorgonColor? blendFactor = null,
                           int blendSampleMask = int.MinValue,
                           int depthStencilReference = 0)
        {
            drawIndexCall.ValidateObject(nameof(drawIndexCall));
            SetDrawStates(drawIndexCall.D3DState, blendFactor ?? GorgonColor.White, blendSampleMask, depthStencilReference);
            D3DDeviceContext.DrawIndexed(drawIndexCall.IndexCount, drawIndexCall.IndexStart, drawIndexCall.BaseVertexIndex);
            ++DrawCallCount;
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
                           GorgonColor? blendFactor = null,
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
            ++DrawCallCount;
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
            SetDrawStates(drawIndexCall.D3DState, _blendFactor, _blendSampleMask, _depthStencilReference);
            D3DDeviceContext.DrawIndexedInstancedIndirect(indirectArgs.Native, argumentOffset);
            ++DrawCallCount;
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

            SetDrawStates(drawCall.D3DState, _blendFactor, _blendSampleMask, _depthStencilReference);
            D3DDeviceContext.DrawInstancedIndirect(indirectArgs.Native, argumentOffset);
            ++DrawCallCount;
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
        /// This method sends a series of state changes and resource bindings to the GPU. However, unlike the <see cref="O:Gorgon.Graphics.Core.GorgonGraphics.Submit"/> commands, this command uses 
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
                                    GorgonColor? blendFactor = null,
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

            SetDrawStates(drawCall.D3DState, _blendFactor, _blendSampleMask, _depthStencilReference);
            D3DDeviceContext.DrawAuto();
            ++DrawCallCount;
        }

        /// <summary>
        /// Function to draw a texture to the current render target.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="destination">The location on the target to draw into.</param>
        /// <param name="color">[Optional] The color to apply to the texture when drawing.</param>
        /// <param name="blendState">[Optional] The type of blending to perform.</param>
        /// <param name="samplerState">[Optional] The sampler state used to define how to sample the texture.</param>
        /// <param name="pixelShader">[Optional] A pixel shader used to apply effects to the texture.</param>
        /// <param name="psConstantBuffers">[Optional] A list of constant buffers for the pixel shader if they're required.</param>
        /// <remarks>
        /// <para>
        /// This is a utility method used to draw a (2D) texture to the current render target.  This is handy for quick testing to ensure things are working as they should. 
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// This method, while quite handy, should not be used for performance sensitive work as it is not the most optimal means of displaying texture data.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonTexture2DView"/>
        public void DrawTexture(GorgonTexture2DView texture,
                                DX.Point destination,
                                GorgonColor? color = null,
                                GorgonBlendState blendState = null,
                                GorgonSamplerState samplerState = null,
                                GorgonPixelShader pixelShader = null,
                                GorgonConstantBuffers psConstantBuffers = null) => _textureBlitter.Value.Blit(texture,
                                       new DX.Rectangle(destination.X, destination.Y, texture.Width, texture.Height),
                                       DX.Point.Zero,
                                       color ?? GorgonColor.White,
                                       true,
                                       blendState,
                                       samplerState,
                                       pixelShader,
                                       psConstantBuffers);

        /// <summary>
        /// Function to draw a texture to the current render target.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="destinationRectangle">The location on the target to draw into, and the size of the area to draw.</param>
        /// <param name="sourceOffset">[Optional] The offset into the texture to start drawing from.</param>
        /// <param name="clipRectangle">[Optional] <b>true</b> to clip the contents of the texture if the size does not match, or <b>false</b> to stretch.</param>
        /// <param name="color">[Optional] The color to apply to the texture when drawing.</param>
        /// <param name="blendState">[Optional] The type of blending to perform.</param>
        /// <param name="samplerState">[Optional] The sampler state used to define how to sample the texture.</param>
        /// <param name="pixelShader">[Optional] A pixel shader used to apply effects to the texture.</param>
        /// <param name="psConstantBuffers">[Optional] A list of constant buffers for the pixel shader if they're required.</param>
        /// <remarks>
        /// <para>
        /// This is a utility method used to draw a (2D) texture to the current render target.  This is handy for quick testing to ensure things are working as they should. 
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// This method, while quite handy, should not be used for performance sensitive work as it is not the most optimal means of displaying texture data.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonTexture2DView"/>
        public void DrawTexture(GorgonTexture2DView texture,
                                DX.Rectangle destinationRectangle,
                                DX.Point sourceOffset = default,
                                bool clipRectangle = false,
                                GorgonColor? color = null,
                                GorgonBlendState blendState = null,
                                GorgonSamplerState samplerState = null,
                                GorgonPixelShader pixelShader = null,
                                GorgonConstantBuffers psConstantBuffers = null) => _textureBlitter.Value.Blit(texture,
                                       destinationRectangle,
                                       sourceOffset,
                                       color ?? GorgonColor.White,
                                       clipRectangle,
                                       blendState,
                                       samplerState,
                                       pixelShader,
                                       psConstantBuffers);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            RenderTargetFactory rtvFactory = Interlocked.Exchange(ref _rtvFactory, null);
            D3D11.DeviceContext4 context = Interlocked.Exchange(ref _d3DDeviceContext, null);
            D3D11.Device5 device = Interlocked.Exchange(ref _d3DDevice, null);
            Adapter4 adapter = Interlocked.Exchange(ref _dxgiAdapter, null);
            Factory5 factory = Interlocked.Exchange(ref _dxgiFactory, null);
            Lazy<TextureBlitter> blitter = Interlocked.Exchange(ref _textureBlitter, null);

            // If these are all gone, then we've already disposed.
            if ((factory == null)
                && (adapter == null)
                && (device == null)
                && (context == null)
                && (blitter == null))
            {
                return;
            }

            rtvFactory?.Dispose();

            if (blitter.IsValueCreated)
            {
                blitter.Value.Dispose();
            }

            ClearStateCache(false);

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
            Log.Print($"Using video adapter '{videoAdapterInfo.Name}' at feature set [{featureSet.Value}] for Direct 3D 11.4.", LoggingLevel.Simple);

            // Build up the required device objects to pass in to the constructor.
            (D3D11.Device5 device, Factory5 factory, Adapter4 adapter) = CreateDevice(videoAdapterInfo, (D3D.FeatureLevel)featureSet.Value);
            _dxgiFactory = factory;
            _dxgiAdapter = adapter;
            _d3DDevice = device;
            _d3DDeviceContext = device.ImmediateContext.QueryInterface<D3D11.DeviceContext4>();

            FormatSupport = EnumerateFormatSupport(_d3DDevice);

            InitializeCachedSamplers();

            _textureBlitter = new Lazy<TextureBlitter>(() =>
                                                       {
                                                           var blitter = new TextureBlitter(this);
                                                           blitter.Initialize();
                                                           return blitter;
                                                       });

            _rtvFactory = new RenderTargetFactory(this);

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
