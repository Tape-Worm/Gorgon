#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Tuesday, July 19, 2011 8:55:06 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Native;
using SharpDX.Mathematics.Interop;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using GI = SharpDX.DXGI;

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
    /// Typically, a graphics object is assigned to a single <see cref="IGorgonVideoDeviceInfo"/> to provide access to the functionality of that video device. If the system has more than once video device 
    /// installed then access to subsequent devices can be given by creating a new instance of this object with the appropriate <see cref="IGorgonVideoDeviceInfo"/>.
    /// </para>
    /// <para>
    /// <note type="tip">
    /// <para>
    /// To determine what devices are attached to the system, use a <see cref="IGorgonVideoDeviceList"/> to retreive a list of applicable video devices. This will contain a list of 
    /// <see cref="IGorgonVideoDeviceInfo"/> objects suitable for construction of the graphics object.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// When creating a graphics object, the user can choose which feature level they will support for a given <see cref="IGorgonVideoDevice"/> so that older devices may be used. The actual feature level 
    /// support is provided by the <see cref="IGorgonVideoDeviceInfo.SupportedFeatureLevel"/> on the <see cref="IGorgonVideoDeviceInfo"/> interface.
    /// </para>
    /// <para>
    /// This object is quite simple in its functionality. It provides some state assignment, and a means to submit a <see cref="GorgonDrawCallBase">draw call</see> so that graphics information can be 
    /// rendered.
    /// </para>
    /// <para><h3>Rendering</h3></para>
    /// <para>
    /// Through the use of <see cref="GorgonDrawCallBase">draw call types</see>, this object will send data in a stateless fashion. This differs from Direct 3D and other traditional APIs where states are 
    /// set until they're changed (stateful). The approach provided by this object avoids a common problem called state-leakage where a state may have been set prior to drawing, but was forgotten about. 
    /// This can lead to artifacts or can disable rendering entirely and consequently can be quite difficult to track. 
    /// </para>
    /// <para>
    /// When a draw call is sent, it carries all of the required state information (with the exception of a view resource types). This ensures that if a draw call doesn't need a state at a specific time, 
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
    /// <para>
    /// <h3>Requirements</h3>
    /// </para>
    /// <para>
    /// This object enforces a minimum of <b>Windows 10, Build 15063 (aka Creators Update)</b>. If this object is instanced on a lower version of operating system, an exception will be thrown. It also 
    /// requires a minimum Direct 3D version of 11.1 (this may be pushed up to 11.2/11.3 or <i>maybe</i> 12 in the future).
    /// </para>
    /// <para>
    /// <h3>Some rationale for Windows 10 is warranted here.</h3>
    /// </para>
    /// Since this requirement may prove somewhat unpopular, here's some reasoning behind it:
    /// <para>
    /// <list type="bullet">
    ///     <item>The author feels that Windows 10 adoption is significant enough to warrant its use as a baseline version.</item>
    ///     <item>Direct 3D 12 support is only available on Windows 10. While Gorgon only uses Direct 3D 11.1, moving to 12 may happen in the future and this will help facilitate that.</item>
    ///     <item>Windows 10 (specifically the Creators Update, build 15063 in conjunction with .NET 4.7) provides new WinForms DPI-aware functionality. This one is kind of a big deal.</item>
    ///     <item>The author actually likes Windows 10. Your opinion may vary.</item>
    /// </list>
    /// So that's the reasoning for now. Users who don't wish to use Windows 10 are free to use other, potentially better, libraries/engines such as 
    /// <a target="_blank" href="http://www.monogame.net/">MonoGame</a> or <a target="_blank" href="https://unity3d.com/">Unity</a> which should work fine on older versions.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonVideoDeviceInfo"/>
    /// <seealso cref="IGorgonVideoDeviceList"/>
    /// <seealso cref="GorgonDrawCall"/>
    /// <seealso cref="GorgonDrawIndexedCall"/>
    /// <seealso cref="GorgonDrawInstancedCall"/>
    /// <seealso cref="GorgonDrawIndexedInstancedCall"/>
    public sealed class GorgonGraphics
        : IDisposable
    {
        #region Constants.
        /// <summary>
        /// The name of the shader file data used for include files that wish to use the include shader.
        /// </summary>
        public const string BlitterShaderIncludeFileName = "__Gorgon_TextureBlitter_Shader__";
        #endregion

        #region Variables.
        // The log interface used to log debug messages.
        private readonly IGorgonLog _log;

        // The video device to use for this graphics object.
        private VideoDevice _videoDevice;

        // The current device context.
        private D3D11.DeviceContext1 _deviceContext;

        // The last used draw call.
        private GorgonDrawCallBase _currentDrawCall;

        // Pipeline state cache.
        private readonly List<GorgonPipelineState> _stateCache = new List<GorgonPipelineState>();

        // A group of pipeline state caches that applications can use for caching their own pipeline states.
        private readonly Dictionary<string, IPipelineStateGroup> _groupedCache = new Dictionary<string, IPipelineStateGroup>(StringComparer.OrdinalIgnoreCase);

        // Synchronization lock for creating new pipeline cache entries.
        private readonly object _stateCacheLock = new object();

        // The list of cached scissor rectangles to keep allocates sane.
        private DX.Rectangle[] _cachedScissors = new DX.Rectangle[1];

        // The current depth/stencil view.
        private GorgonDepthStencilView _depthStencilView;

        // Flag to indicate that the depth/stencil view has been changed.
        private bool _depthStencilChanged;

        // The list of render targets currently assigned.
        private readonly GorgonRenderTargetViews _renderTargets;

        // The states used for texture samplers.  Used as a transitional buffer between D3D11 and Gorgon.
        private readonly D3D11.SamplerState[] _samplerStates = new D3D11.SamplerState[GorgonSamplerStates.MaximumSamplerStateCount];

        // The texture blitter used to render a single texture.
        private readonly Lazy<TextureBlitter> _textureBlitter;

        // An unordered access view buffer (other shader stages).
        private D3D11.UnorderedAccessView[] _uavBuffer = new D3D11.UnorderedAccessView[0];

        // A buffer for uav counters (other shader stages).
        private int[] _uavCounters = new int[0];

        // A buffer used to apply stream out buffers.
        private D3D11.StreamOutputBufferBinding[] _streamOutBuffer;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the Direct 3D 11.1 device context for this graphics instance.
        /// </summary>
        internal D3D11.DeviceContext1 D3DDeviceContext => _deviceContext;

        /// <summary>
        /// Property to return the list of cached pipeline states.
        /// </summary>
        public IReadOnlyList<GorgonPipelineState> CachedPipelineStates
        {
            get
            {
                lock (_stateCacheLock)
                {
                    return _stateCache;
                }
            }
        }

        /// <summary>
        /// Property to set or return the video device to use for this graphics interface.
        /// </summary>
        public IGorgonVideoDevice VideoDevice => _videoDevice;

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
        /// Property to set or return the currently active depth/stencil view.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is read only, but can be assigned via one of the <see cref="O:Gorgon.Graphics.Core.GorgonGraphics.SetRenderTarget"/> or 
        /// <see cref="SetRenderTargets(GorgonRenderTargetView[], GorgonDepthStencilView)"/> methods.
        /// </para>
        /// <para>
        /// If a render target is bound using <see cref="SetRenderTarget(GorgonRenderTargetView)"/> and it has an attached depth/stencil view, then that view will automatically be assigned to this value.
        /// </para>
        /// </remarks>
        /// <seealso cref="O:Gorgon.Graphics.Core.GorgonGraphics.SetRenderTarget"/>
        /// <seealso cref="SetRenderTargets"/>
        /// <seealso cref="GorgonDepthStencilView"/>
        public GorgonDepthStencilView DepthStencilView
        {
            get => _depthStencilView;
            private set
            {
                if (_depthStencilView == value)
                {
                    return;
                }

                _depthStencilView = value;
                _depthStencilChanged = true;
            }
        }

        /// <summary>
        /// Property to return the current list of render targets assigned to the renderer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When a render target is assigned to this list, the depth/stencil buffer will be replaced with the associated depth/stencil for the assigned <see cref="GorgonRenderTargetView"/>. If the 
        /// <see cref="GorgonRenderTargetView"/> does not have an associated depth/stencil buffer, then the current <see cref="DepthStencilView"/> buffer will be set to <b>null</b>. 
        /// If a user wishes to assign their own depth/stencil buffer, then they must assign the <see cref="DepthStencilView"/> after assigning a <see cref="GorgonRenderTargetView"/>.
        /// </para>
        /// <para>
        /// Unlike most states/resources, this property is stateful, that is, it will retain its state until changed by the user and will not be overwritten by a draw call.
        /// </para>
        /// <para>
        /// This list is read only. To assign a render target, use the <see cref="O:Gorgon.Graphics.Core.GorgonGraphics.SetRenderTarget"/> or the 
        /// <see cref="SetRenderTargets(GorgonRenderTargetView[], GorgonDepthStencilView)"/> methods.
        /// </para>
        /// </remarks>
        /// <seealso cref="SetRenderTargets(GorgonRenderTargetView[], GorgonDepthStencilView)"/>
        /// <seealso cref="O:Gorgon.Graphics.Core.GorgonGraphics.SetRenderTarget"/>
        /// <seealso cref="GorgonRenderTargetView"/>
        public IReadOnlyList<GorgonRenderTargetView> RenderTargets => _renderTargets;

        /// <summary>
        /// Property to return the scissor rectangles currently active for rendering.
        /// </summary>
        public GorgonMonitoredValueTypeArray<DX.Rectangle> ScissorRectangles
        {
            get;
        }

        /// <summary>
        /// Property to return the viewports used to render to the <see cref="RenderTargets"/>.
        /// </summary>
        public GorgonMonitoredValueTypeArray<DX.ViewportF> Viewports
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to merge the previous draw call vertex buffers with new ones.
        /// </summary>
        /// <param name="vertexBuffers">The vertex buffers to merge in.</param>
        /// <param name="currentChanges">The current changes on the pipeline.</param>
        /// <returns>A <see cref="PipelineResourceChange"/> indicating whether or not the state has changed.</returns>
        private PipelineResourceChange MergeVertexBuffers(GorgonVertexBufferBindings vertexBuffers, PipelineResourceChange currentChanges)
        {
            if (_currentDrawCall.VertexBuffers?.InputLayout != vertexBuffers?.InputLayout)
            {
                currentChanges |= PipelineResourceChange.InputLayout;
            }

            if (_currentDrawCall.VertexBuffers == vertexBuffers)
            {
                if ((_currentDrawCall.VertexBuffers == null)
                    || (!_currentDrawCall.VertexBuffers.IsDirty))
                {
                    return currentChanges;
                }

                return currentChanges | PipelineResourceChange.VertexBuffers;
            }

            // If we're tranferring into an uninitialized vertex buffer list, then allocate new vertex buffers and copy.
            if ((vertexBuffers != null)
                && (_currentDrawCall.VertexBuffers == null))
            {
                _currentDrawCall.VertexBuffers = new GorgonVertexBufferBindings(vertexBuffers.InputLayout);
                vertexBuffers.CopyTo(_currentDrawCall.VertexBuffers);
                return currentChanges | PipelineResourceChange.VertexBuffers;
            }

            // If we're removing a set of vertex buffers, then get rid of our current set as well.
            if (vertexBuffers == null)
            {
                _currentDrawCall.VertexBuffers = null;
                return currentChanges | PipelineResourceChange.VertexBuffers;
            }

            ref (int StartSlot, int Count) newItems = ref vertexBuffers.GetDirtyItems();

            // There's nothing going on in this list, so we can just leave now.
            if (newItems.Count == 0)
            {
                return currentChanges;
            }

            int endSlot = newItems.StartSlot + newItems.Count;

            if ((currentChanges & PipelineResourceChange.InputLayout) == PipelineResourceChange.InputLayout)
            {
                _currentDrawCall.VertexBuffers.InputLayout = vertexBuffers.InputLayout;
            }

            for (int i = newItems.StartSlot; i < endSlot; ++i)
            {
                _currentDrawCall.VertexBuffers[i] = vertexBuffers[i];
            }

            if (_currentDrawCall.VertexBuffers.IsDirty)
            {
                currentChanges |= PipelineResourceChange.VertexBuffers;
            }

            return currentChanges;
        }

        /// <summary>
        /// Function to merge the previous draw call stream output buffers with new ones.
        /// </summary>
        /// <param name="streamOutBuffers">The stream output buffers to merge in.</param>
        /// <param name="currentChanges">The current changes on the pipeline.</param>
        /// <returns>A <see cref="PipelineResourceChange"/> indicating whether or not the state has changed.</returns>
        private PipelineResourceChange MergeStreamOutBuffers(GorgonStreamOutBindings streamOutBuffers, PipelineResourceChange currentChanges)
        {
            if (_currentDrawCall.StreamOutBuffers == streamOutBuffers)
            {
                if ((_currentDrawCall.StreamOutBuffers == null)
                    || (!_currentDrawCall.StreamOutBuffers.IsDirty))
                {
                    return currentChanges;
                }

                return currentChanges | PipelineResourceChange.StreamOut;
            }

            // If we're tranferring into an uninitialized stream output buffer list, then allocate new stream output buffers and copy.
            if ((streamOutBuffers != null)
                && (_currentDrawCall.StreamOutBuffers == null))
            {
                _currentDrawCall.StreamOutBuffers = new GorgonStreamOutBindings();
                streamOutBuffers.CopyTo(_currentDrawCall.StreamOutBuffers);
                return currentChanges | PipelineResourceChange.StreamOut;
            }

            // If we're removing a set of stream output buffers, then get rid of our current set as well.
            if (streamOutBuffers == null)
            {
                _currentDrawCall.StreamOutBuffers = null;
                return currentChanges | PipelineResourceChange.StreamOut;
            }

            ref (int StartSlot, int Count) newItems = ref streamOutBuffers.GetDirtyItems();

            // There's nothing going on in this list, so we can just leave now.
            if (newItems.Count == 0)
            {
                return currentChanges;
            }

            int endSlot = newItems.StartSlot + newItems.Count;

            for (int i = newItems.StartSlot; i < endSlot; ++i)
            {
                _currentDrawCall.StreamOutBuffers[i] = streamOutBuffers[i];
            }

            if (_currentDrawCall.StreamOutBuffers.IsDirty)
            {
                currentChanges |= PipelineResourceChange.StreamOut;
            }

            return currentChanges;
        }

        /// <summary>
        /// Function to merge the previous shader constant buffers with new ones.
        /// </summary>
        /// <param name="shaderType">The type of shader to work with.</param>
        /// <param name="buffers">The constant buffers to merge in.</param>
        /// <param name="currentChanges">The current changes on the pipeline.</param>
        /// <returns>A <see cref="PipelineResourceChange"/> indicating whether or not the state has changed.</returns>
        private PipelineResourceChange MergeConstantBuffers(ShaderType shaderType, GorgonConstantBuffers buffers, PipelineResourceChange currentChanges)
        {
            ref (int StartSlot, int Count) newItems = ref buffers.GetDirtyItems();

            if (newItems.Count == 0)
            {
                return currentChanges;
            }

            PipelineResourceChange desiredStateBit;
            GorgonConstantBuffers destBuffers;

            switch (shaderType)
            {
                case ShaderType.Vertex:
                    desiredStateBit = PipelineResourceChange.VertexShaderConstantBuffers;
                    destBuffers = _currentDrawCall.VertexShaderConstantBuffers;
                    break;
                case ShaderType.Pixel:
                    desiredStateBit = PipelineResourceChange.PixelShaderConstantBuffers;
                    destBuffers = _currentDrawCall.PixelShaderConstantBuffers;
                    break;
                case ShaderType.Geometry:
                    desiredStateBit = PipelineResourceChange.GeometryShaderConstantBuffers;
                    destBuffers = _currentDrawCall.GeometryShaderConstantBuffers;
                    break;
                case ShaderType.Hull:
#if DEBUG
                    if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
                    {
                        _log.Print($"Error: Assigning a constant buffer to a hull shader requires feature level {FeatureLevelSupport.Level_11_0} or better.  The device '{VideoDevice.Info.Name}' only supports feature level {VideoDevice.RequestedFeatureLevel}",
                                   LoggingLevel.All);
                        throw new GorgonException(GorgonResult.CannotBind,
                                                  string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
                    }
#endif

                    desiredStateBit = PipelineResourceChange.HullShaderConstantBuffers;
                    destBuffers = _currentDrawCall.HullShaderConstantBuffers;
                    break;
                case ShaderType.Domain:
#if DEBUG
                    if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
                    {
                        _log.Print($"Error: Assigning a constant buffer to a domain shader requires feature level {FeatureLevelSupport.Level_11_0} or better.  The device '{VideoDevice.Info.Name}' only supports feature level {VideoDevice.RequestedFeatureLevel}",
                                   LoggingLevel.All);
                        throw new GorgonException(GorgonResult.CannotBind,
                                                  string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
                    }
#endif
                    desiredStateBit = PipelineResourceChange.DomainShaderConstantBuffers;
                    destBuffers = _currentDrawCall.DomainShaderConstantBuffers;
                    break;
                default:
                    throw new NotSupportedException();
            }

            return CopyToLastDrawCall(newItems.StartSlot,
                                      newItems.StartSlot + newItems.Count,
                                      destBuffers,
                                      buffers,
                                      currentChanges,
                                      desiredStateBit);

            // Local functions are neat.
            PipelineResourceChange CopyToLastDrawCall(int newStart,
                                                      int newEnd,
                                                      GorgonConstantBuffers lastDrawConstants,
                                                      GorgonConstantBuffers newBuffers,
                                                      PipelineResourceChange changes,
                                                      PipelineResourceChange desiredBit)
            {
                for (int i = newStart; i < newEnd; ++i)
                {
                    lastDrawConstants[i] = newBuffers[i];
                }

                if (lastDrawConstants.IsDirty)
                {
                    changes |= desiredBit;
                }

                return changes;
            }
        }

        /// <summary>
        /// Function to merge the previous shader resource views with new ones.
        /// </summary>
        /// <param name="shaderType">The type of shader to work with.</param>
        /// <param name="srvs">The shader resource views to merge in.</param>
        /// <param name="currentChanges">The current changes on the pipeline.</param>
        /// <returns>A <see cref="PipelineResourceChange"/> indicating whether or not the state has changed.</returns>
        private PipelineResourceChange MergeShaderResources(ShaderType shaderType, GorgonShaderResourceViews srvs, PipelineResourceChange currentChanges)
        {
            ref (int StartSlot, int Count) newItems = ref srvs.GetDirtyItems();

            if (newItems.Count == 0)
            {
                return currentChanges;
            }

            PipelineResourceChange desiredStateBit;
            GorgonShaderResourceViews destSrvs;

            // Ensure that no input is currently bound as an output.
#if DEBUG
            ValidateSrvBinding(shaderType, srvs, newItems.StartSlot, newItems.StartSlot + newItems.Count);
#endif

            switch (shaderType)
            {
                case ShaderType.Vertex:
                    desiredStateBit = PipelineResourceChange.VertexShaderResources;
                    destSrvs = _currentDrawCall.VertexShaderResourceViews;
                    break;
                case ShaderType.Pixel:
                    desiredStateBit = PipelineResourceChange.PixelShaderResources;
                    destSrvs = _currentDrawCall.PixelShaderResourceViews;
                    break;
                case ShaderType.Geometry:
                    desiredStateBit = PipelineResourceChange.GeometryShaderResources;
                    destSrvs = _currentDrawCall.GeometryShaderResourceViews;
                    break;
                case ShaderType.Hull:
#if DEBUG
                    if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
                    {
                        _log.Print($"Error: Assigning a resource to a hull shader requires feature level {FeatureLevelSupport.Level_11_0} or better.  The device '{VideoDevice.Info.Name}' only supports feature level {VideoDevice.RequestedFeatureLevel}",
                                   LoggingLevel.All);
                        throw new GorgonException(GorgonResult.CannotBind,
                                                  string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
                    }
#endif
                    desiredStateBit = PipelineResourceChange.HullShaderResources;
                    destSrvs = _currentDrawCall.HullShaderResourceViews;
                    break;
                case ShaderType.Domain:
#if DEBUG
                    if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
                    {
                        _log.Print($"Error: Assigning a resource to a domain shader requires feature level {FeatureLevelSupport.Level_11_0} or better.  The device '{VideoDevice.Info.Name}' only supports feature level {VideoDevice.RequestedFeatureLevel}",
                                   LoggingLevel.All);
                        throw new GorgonException(GorgonResult.CannotBind,
                                                  string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
                    }
#endif
                    desiredStateBit = PipelineResourceChange.DomainShaderResources;
                    destSrvs = _currentDrawCall.DomainShaderResourceViews;
                    break;
                default:
                    throw new NotSupportedException();
            }

            return CopyToLastDrawCall(newItems.StartSlot,
                                      newItems.Count + newItems.StartSlot,
                                      destSrvs,
                                      srvs,
                                      currentChanges,
                                      desiredStateBit);

            // Local functions are neat.
            PipelineResourceChange CopyToLastDrawCall(int newStart,
                                                      int newEnd,
                                                      GorgonShaderResourceViews lastDrawSrvs,
                                                      GorgonShaderResourceViews newSrvs,
                                                      PipelineResourceChange changes,
                                                      PipelineResourceChange desiredBit)
            {
                for (int i = newStart; i < newEnd; ++i)
                {
                    lastDrawSrvs[i] = newSrvs[i];
                }

                if (lastDrawSrvs.IsDirty)
                {
                    changes |= desiredBit;
                }

                return changes;
            }
        }

        /// <summary>
        /// Function to merge the unordered access views.
        /// </summary>
        /// <param name="uavs">The unordered access views to merge.</param>
        /// <param name="currentChanges">The current changes on the pipeline.</param>
        /// <returns>A <see cref="PipelineResourceChange"/> indicating whether or not the state has changed and a flag indicating that render targets were unbound.</returns>
        private PipelineResourceChange MergeUavs(GorgonUavBindings uavs, PipelineResourceChange currentChanges)
        {
            ref (int StartSlot, int Count) newItems = ref uavs.GetDirtyItems();

            if (newItems.Count == 0)
            {
                return currentChanges;
            }

            // Unbind any render targets at this point.
            int newEnd = newItems.Count + newItems.StartSlot;

#if DEBUG
            if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
            {
                throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
            }

            ref (int, int Count) rtvItems = ref _renderTargets.GetDirtyItems();

            for (int i = 0; (i < rtvItems.Count) && (newItems.Count > 0); ++i)
            {
                GorgonRenderTargetView rtv = _renderTargets[i];

                if ((rtv == null) || ((rtv.Texture.Info.Binding & TextureBinding.UnorderedAccess) != TextureBinding.UnorderedAccess))
                {
                    continue;
                }

                for (int j = newItems.StartSlot; j < newEnd; ++j)
                {
                    GorgonUavBinding uavBinding = uavs[i];

                    if ((uavBinding.Uav == null) || (uavBinding.Uav.Resource != rtv.Texture))
                    {
                        continue;
                    }

                    throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_CONFLICT_UAV_RTV, uavBinding.Uav.Resource.Name, i));
                }
            }
#endif

            UnbindUavInputs(ref newItems, _currentDrawCall?.UnorderedAccessViews, false);

            for (int i = newItems.StartSlot; i < newEnd; ++i)
            {
                _renderTargets[i] = null;
                _currentDrawCall.UnorderedAccessViews[i] = uavs[i];
            }

            if (_currentDrawCall.UnorderedAccessViews.IsDirty)
            {
                currentChanges |= PipelineResourceChange.Uavs;
            }

            return currentChanges;
        }

        /// <summary>
        /// Function to merge the previous shader samplers with new ones.
        /// </summary>
        /// <param name="shaderType">The type of shader to work with.</param>
        /// <param name="samplers">The shader samplers to merge in.</param>
        /// <param name="currentChanges">The current changes on the pipeline.</param>
        /// <returns>A <see cref="PipelineResourceChange"/> indicating whether or not the state has changed.</returns>
        private PipelineResourceChange MergeShaderSamplers(ShaderType shaderType, GorgonSamplerStates samplers, PipelineResourceChange currentChanges)
        {
            ref (int StartSlot, int Count) newItems = ref samplers.GetDirtyItems();

            if (newItems.Count == 0)
            {
                return currentChanges;
            }

            PipelineResourceChange desiredStateBit;
            GorgonSamplerStates destSamplers;

            switch (shaderType)
            {
                case ShaderType.Vertex:
#if DEBUG
                    // If the device doesn't support this, we need to crash out so the dev can fix it.
                    if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
                    {
                        _log.Print($"Error: Assigning a sampler to a vertex shader requires feature level {FeatureLevelSupport.Level_11_0} or better.  The device '{VideoDevice.Info.Name}' only supports feature level {VideoDevice.RequestedFeatureLevel}",
                                   LoggingLevel.All);
                        throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
                    }
#endif

                    desiredStateBit = PipelineResourceChange.VertexShaderSamplers;
                    destSamplers = _currentDrawCall.VertexShaderSamplers;
                    break;
                case ShaderType.Pixel:
                    desiredStateBit = PipelineResourceChange.PixelShaderSamplers;
                    destSamplers = _currentDrawCall.PixelShaderSamplers;
                    break;
                case ShaderType.Geometry:
                    desiredStateBit = PipelineResourceChange.GeometryShaderSamplers;
                    destSamplers = _currentDrawCall.GeometryShaderSamplers;
                    break;
                case ShaderType.Hull:
#if DEBUG
                    _log.Print($"Error: Assigning a sampler to a hull shader requires feature level {FeatureLevelSupport.Level_11_0} or better.  The device '{VideoDevice.Info.Name}' only supports feature level {VideoDevice.RequestedFeatureLevel}",
                               LoggingLevel.All);
                    if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
                    {
                        throw new GorgonException(GorgonResult.CannotBind,
                                                  string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
                    }
#endif
                    desiredStateBit = PipelineResourceChange.HullShaderSamplers;
                    destSamplers = _currentDrawCall.HullShaderSamplers;
                    break;
                case ShaderType.Domain:
#if DEBUG
                    _log.Print($"Error: Assigning a sampler to a domain shader requires feature level {FeatureLevelSupport.Level_11_0} or better.  The device '{VideoDevice.Info.Name}' only supports feature level {VideoDevice.RequestedFeatureLevel}",
                               LoggingLevel.All);
                    if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
                    {
                        throw new GorgonException(GorgonResult.CannotBind,
                                                  string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
                    }
#endif
                    desiredStateBit = PipelineResourceChange.DomainShaderSamplers;
                    destSamplers = _currentDrawCall.DomainShaderSamplers;
                    break;
                default:
                    throw new NotSupportedException();
            }

            return CopyToLastDrawCall(newItems.StartSlot,
                                      newItems.Count + newItems.StartSlot,
                                      destSamplers,
                                      samplers,
                                      currentChanges,
                                      desiredStateBit);

            // Local functions are neat.
            PipelineResourceChange CopyToLastDrawCall(int newStart,
                                                      int newEnd,
                                                      GorgonSamplerStates lastDrawSamplers,
                                                      GorgonSamplerStates newSamplers,
                                                      PipelineResourceChange changes,
                                                      PipelineResourceChange desiredBit)
            {
                for (int i = newStart; i < newEnd; ++i)
                {
                    lastDrawSamplers[i] = newSamplers[i];
                }

                if (lastDrawSamplers.IsDirty)
                {
                    changes |= desiredBit;
                }

                return changes;
            }
        }

        /// <summary>
        /// Function to compare this pipeline state with another pipeline state.
        /// </summary>
        /// <param name="state">The state to compare.</param>
        /// <returns>The states that have been changed between this state and the other <paramref name="state"/>.</returns>
        private PipelineStateChange GetPipelineStateChange(GorgonPipelineState state)
        {
            PipelineStateChange pipelineFlags = PipelineStateChange.None;

            if ((_currentDrawCall.PipelineState == null) && (state == null))
            {
                return pipelineFlags;
            }

            if (((_currentDrawCall.PipelineState == null) && (state != null))
                || (state == null))
            {
                pipelineFlags |= PipelineStateChange.BlendState
                                 | PipelineStateChange.DepthStencilState
                                 | PipelineStateChange.RasterState
                                 | PipelineStateChange.PixelShader
                                 | PipelineStateChange.VertexShader
                                 | PipelineStateChange.GeometryShader;

                if (VideoDevice.RequestedFeatureLevel >= FeatureLevelSupport.Level_11_0)
                {
                    pipelineFlags |= PipelineStateChange.HullShader
                                     | PipelineStateChange.DomainShader;
                }

                _currentDrawCall.PipelineState = state;
                return pipelineFlags;
            }

            if (_currentDrawCall.PipelineState.Info.PixelShader != state.Info.PixelShader)
            {
                pipelineFlags |= PipelineStateChange.PixelShader;
            }

            if (_currentDrawCall.PipelineState.Info.VertexShader != state.Info.VertexShader)
            {
                pipelineFlags |= PipelineStateChange.VertexShader;
            }


            if (_currentDrawCall.PipelineState.Info.GeometryShader != state.Info.GeometryShader)
            {
                pipelineFlags |= PipelineStateChange.GeometryShader;
            }

            if (_currentDrawCall.PipelineState.Info.HullShader != state.Info.HullShader)
            {
#if DEBUG
                _log.Print($"Error: Assigning a hull shader requires feature level {FeatureLevelSupport.Level_11_0} or better.  The device '{VideoDevice.Info.Name}' only supports feature level {VideoDevice.RequestedFeatureLevel}",
                           LoggingLevel.All);
                if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
                {
                    throw new GorgonException(GorgonResult.CannotBind,
                                              string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
                }
#endif

                pipelineFlags |= PipelineStateChange.HullShader;
            }

            if (_currentDrawCall.PipelineState.Info.DomainShader != state.Info.DomainShader)
            {
#if DEBUG
                _log.Print($"Error: Assigning a domain shader requires feature level {FeatureLevelSupport.Level_11_0} or better.  The device '{VideoDevice.Info.Name}' only supports feature level {VideoDevice.RequestedFeatureLevel}",
                           LoggingLevel.All);
                if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
                {
                    throw new GorgonException(GorgonResult.CannotBind,
                                              string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
                }
#endif
                pipelineFlags |= PipelineStateChange.DomainShader;
            }

            if (_currentDrawCall.PipelineState.D3DRasterState != state.D3DRasterState)
            {
                pipelineFlags |= PipelineStateChange.RasterState;
            }

            if (_currentDrawCall.PipelineState.D3DDepthStencilState != state.D3DDepthStencilState)
            {
                pipelineFlags |= PipelineStateChange.DepthStencilState;
            }

            if (_currentDrawCall.PipelineState.D3DBlendState != state.D3DBlendState)
            {
                pipelineFlags |= PipelineStateChange.BlendState;
            }

            if (pipelineFlags != PipelineStateChange.None)
            {
                _currentDrawCall.PipelineState = state;
            }

            return pipelineFlags;
        }

        /// <summary>
        /// Function to merge the current and previous draw call in order to reduce state change.
        /// </summary>
        /// <param name="sourceDrawCall">The draw call that is currently being executed.</param>
        /// <returns>The changes to the resources and the state for the pipeline.</returns>
        private (PipelineResourceChange, PipelineStateChange) MergeDrawCall(GorgonDrawCallBase sourceDrawCall)
        {
            if (_currentDrawCall == null)
            {
                _currentDrawCall = new GorgonDrawCallBase
                                   {
                                       PrimitiveTopology = D3D.PrimitiveTopology.Undefined
                                   };
            }

            PipelineResourceChange stateChanges = PipelineResourceChange.None;

            if (_currentDrawCall.PrimitiveTopology != sourceDrawCall.PrimitiveTopology)
            {
                _currentDrawCall.PrimitiveTopology = sourceDrawCall.PrimitiveTopology;
                stateChanges |= PipelineResourceChange.PrimitiveTopology;
            }

            if (_currentDrawCall.IndexBuffer != sourceDrawCall.IndexBuffer)
            {
                _currentDrawCall.IndexBuffer = sourceDrawCall.IndexBuffer;
                stateChanges |= PipelineResourceChange.IndexBuffer;
            }

            if (_currentDrawCall.BlendSampleMask != sourceDrawCall.BlendSampleMask)
            {
                _currentDrawCall.BlendSampleMask = sourceDrawCall.BlendSampleMask;
                stateChanges |= PipelineResourceChange.BlendSampleMask;
            }

            if (_currentDrawCall.DepthStencilReference != sourceDrawCall.DepthStencilReference)
            {
                _currentDrawCall.DepthStencilReference = sourceDrawCall.DepthStencilReference;
                stateChanges |= PipelineResourceChange.DepthStencilReference;
            }

            if (!_currentDrawCall.BlendFactor.Equals(sourceDrawCall.BlendFactor))
            {
                _currentDrawCall.BlendFactor = sourceDrawCall.BlendFactor;
                stateChanges |= PipelineResourceChange.BlendFactor;
            }

            stateChanges |= MergeUavs(sourceDrawCall.UnorderedAccessViews, stateChanges);
            stateChanges |= MergeVertexBuffers(sourceDrawCall.VertexBuffers, stateChanges);
            stateChanges |= MergeStreamOutBuffers(sourceDrawCall.StreamOutBuffers, stateChanges);

            stateChanges |= MergeConstantBuffers(ShaderType.Pixel, sourceDrawCall.PixelShaderConstantBuffers, stateChanges);
            stateChanges |= MergeShaderResources(ShaderType.Pixel, sourceDrawCall.PixelShaderResourceViews, stateChanges);
            stateChanges |= MergeShaderSamplers(ShaderType.Pixel, sourceDrawCall.PixelShaderSamplers, stateChanges);

            stateChanges |= MergeConstantBuffers(ShaderType.Vertex, sourceDrawCall.VertexShaderConstantBuffers, stateChanges);
            stateChanges |= MergeShaderSamplers(ShaderType.Vertex, sourceDrawCall.VertexShaderSamplers, stateChanges);
            stateChanges |= MergeShaderResources(ShaderType.Vertex, sourceDrawCall.VertexShaderResourceViews, stateChanges);

            stateChanges |= MergeConstantBuffers(ShaderType.Geometry, sourceDrawCall.GeometryShaderConstantBuffers, stateChanges);
            stateChanges |= MergeShaderResources(ShaderType.Geometry, sourceDrawCall.GeometryShaderResourceViews, stateChanges);
            stateChanges |= MergeShaderSamplers(ShaderType.Geometry, sourceDrawCall.GeometryShaderSamplers, stateChanges);

            stateChanges |= MergeConstantBuffers(ShaderType.Hull, sourceDrawCall.HullShaderConstantBuffers, stateChanges);
            stateChanges |= MergeShaderResources(ShaderType.Hull, sourceDrawCall.HullShaderResourceViews, stateChanges);
            stateChanges |= MergeShaderSamplers(ShaderType.Hull, sourceDrawCall.HullShaderSamplers, stateChanges);

            stateChanges |= MergeConstantBuffers(ShaderType.Domain, sourceDrawCall.DomainShaderConstantBuffers, stateChanges);
            stateChanges |= MergeShaderResources(ShaderType.Domain, sourceDrawCall.DomainShaderResourceViews, stateChanges);
            stateChanges |= MergeShaderSamplers(ShaderType.Domain, sourceDrawCall.DomainShaderSamplers, stateChanges);

            return (stateChanges, GetPipelineStateChange(sourceDrawCall.PipelineState));
        }

        /// <summary>
        /// Function to initialize a <see cref="GorgonPipelineState" /> object with Direct 3D 11 state objects by creating new objects for the unassigned values.
        /// </summary>
        /// <param name="info">The information used to create the pipeline state.</param>
        /// <param name="blendState">An existing blend state to use.</param>
        /// <param name="depthStencilState">An existing depth/stencil state to use.</param>
        /// <param name="rasterState">An existing rasterizer state to use.</param>
        /// <returns>A new <see cref="GorgonPipelineState"/>.</returns>
        private GorgonPipelineState InitializePipelineState(IGorgonPipelineStateInfo info,
                                                            D3D11.BlendState1 blendState,
                                                            D3D11.DepthStencilState depthStencilState,
                                                            D3D11.RasterizerState1 rasterState)
        {
            D3D11.Device1 videoDevice = VideoDevice.D3DDevice();
            GorgonPipelineState result = new GorgonPipelineState(info, _stateCache.Count)
                         {
                             D3DBlendState = blendState,
                             D3DDepthStencilState = depthStencilState,
                             D3DRasterState = rasterState
                         };

            if ((rasterState == null) && (info.RasterState != null))
            {
                result.D3DRasterState = new D3D11.RasterizerState1(videoDevice, info.RasterState.ToRasterStateDesc1())
                                        {
                                            DebugName = "Gorgon D3D11RasterState"
                                        };
            }

            if ((depthStencilState == null) && (info.DepthStencilState != null))
            {
                result.D3DDepthStencilState = new D3D11.DepthStencilState(videoDevice, info.DepthStencilState.ToDepthStencilStateDesc())
                                              {
                                                  DebugName = "Gorgon D3D11DepthStencilState"
                                              };
            }

            if ((blendState != null) || (info.BlendStates == null) || (info.BlendStates.Count == 0))
            {
                return result;
            }

            int maxStates = info.BlendStates.Count.Min(D3D11.OutputMergerStage.SimultaneousRenderTargetCount);

            D3D11.BlendStateDescription1 desc = new D3D11.BlendStateDescription1
                       {
                           IndependentBlendEnable = info.IsIndependentBlendingEnabled,
                           AlphaToCoverageEnable = info.IsAlphaToCoverageEnabled
                       };

            for (int i = 0; i < maxStates; ++i)
            {
                desc.RenderTarget[i] = info.BlendStates[i].ToRenderTargetBlendStateDesc1();
            }

            result.D3DBlendState = new D3D11.BlendState1(videoDevice, desc)
                                   {
                                       DebugName = "Gorgon D3D11BlendState"
                                   };

            return result;
        }

        /// <summary>
        /// Function to build up a <see cref="GorgonPipelineState"/> object with Direct 3D 11 state objects by either creating new objects, or inheriting previous ones.
        /// </summary>
        /// <param name="newState">The new state to initialize.</param>
        /// <returns>An existing pipeline state if no changes are found, or a new pipeline state otherwise.</returns>
        private (GorgonPipelineState state, bool isNew) SetupPipelineState(IGorgonPipelineStateInfo newState)
        {
            // Existing states.
            D3D11.DepthStencilState depthStencilState = null;
            D3D11.BlendState1 blendState = null;
            D3D11.RasterizerState1 rasterState = null;
            (GorgonPipelineState State, bool IsNew) result;

            IGorgonPipelineStateInfo newStateInfo = newState;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < _stateCache.Count; ++i)
            {
                int blendStateEqualCount = 0;
                PipelineStateChange inheritedState = PipelineStateChange.None;
                GorgonPipelineState cachedState = _stateCache[i];
                IGorgonPipelineStateInfo cachedStateInfo = _stateCache[i].Info;

                if (cachedStateInfo.PixelShader == newStateInfo.PixelShader)
                {
                    inheritedState |= PipelineStateChange.PixelShader;
                }

                if (cachedStateInfo.VertexShader == newStateInfo.VertexShader)
                {
                    inheritedState |= PipelineStateChange.VertexShader;
                }

                if (cachedStateInfo.GeometryShader == newStateInfo.GeometryShader)
                {
                    inheritedState |= PipelineStateChange.GeometryShader;
                }

                if (cachedStateInfo.HullShader == newStateInfo.HullShader)
                {
                    if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
                    {
                        _log.Print($"Error: A hull shader requires feature level {FeatureLevelSupport.Level_11_0} or better.  The device '{VideoDevice.Info.Name}' only supports feature level {VideoDevice.RequestedFeatureLevel}",
                                   LoggingLevel.All);
                        throw new GorgonException(GorgonResult.CannotCreate,
                                                  string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
                    }

                    inheritedState |= PipelineStateChange.HullShader;
                }

                if (cachedStateInfo.DomainShader == newStateInfo.DomainShader)
                {
                    if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
                    {
                        _log.Print($"Error: A domain shader requires feature level {FeatureLevelSupport.Level_11_0} or better.  The device '{VideoDevice.Info.Name}' only supports feature level {VideoDevice.RequestedFeatureLevel}",
                                   LoggingLevel.All);
                        throw new GorgonException(GorgonResult.CannotCreate,
                                                  string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
                    }

                    inheritedState |= PipelineStateChange.DomainShader;
                }

                if ((cachedStateInfo.RasterState != null) &&
                    (cachedStateInfo.RasterState.Equals(newStateInfo.RasterState)))
                {
                    rasterState = cachedState.D3DRasterState;
                    inheritedState |= PipelineStateChange.RasterState;
                }

                if ((cachedStateInfo.DepthStencilState != null) &&
                    (cachedStateInfo.DepthStencilState.Equals(newStateInfo.DepthStencilState)))
                {
                    depthStencilState = cachedState.D3DDepthStencilState;
                    inheritedState |= PipelineStateChange.DepthStencilState;
                }

                if (ReferenceEquals(newStateInfo.BlendStates, cachedStateInfo.BlendStates))
                {
                    blendState = cachedState.D3DBlendState;
                    inheritedState |= PipelineStateChange.BlendState;
                }
                else
                {
                    if ((newStateInfo.BlendStates != null)
                        && (cachedStateInfo.BlendStates != null)
                        && (newStateInfo.BlendStates.Count == cachedStateInfo.BlendStates.Count))
                    {
                        for (int j = 0; j < newStateInfo.BlendStates.Count; ++j)
                        {
                            if (cachedStateInfo.BlendStates[j]?.Equals(newStateInfo.BlendStates[j]) ?? false)
                            {
                                blendStateEqualCount++;
                            }
                        }

                        if (blendStateEqualCount == newStateInfo.BlendStates.Count)
                        {
                            blendState = cachedState.D3DBlendState;
                            inheritedState |= PipelineStateChange.BlendState;
                        }
                    }
                }

                // We've copied all the states, so just return the existing pipeline state.
                // ReSharper disable once InvertIf
                if (inheritedState == (PipelineStateChange.VertexShader
                                       | PipelineStateChange.PixelShader
                                       | PipelineStateChange.GeometryShader
                                       | PipelineStateChange.HullShader
                                       | PipelineStateChange.DomainShader
                                       | PipelineStateChange.BlendState
                                       | PipelineStateChange.RasterState
                                       | PipelineStateChange.DepthStencilState))
                {
                    result.State = _stateCache[i];
                    result.IsNew = false;
                    return result;
                }
            }

            // Setup any uninitialized states.
            result.State = InitializePipelineState(newState, blendState, depthStencilState, rasterState);
            result.IsNew = true;
            return result;
        }

        /// <summary>
        /// Function to bind an index buffer to the pipeline.
        /// </summary>
        /// <param name="indexBuffer">The index buffer to bind.</param>
        private void SetIndexbuffer(GorgonIndexBuffer indexBuffer)
        {
            D3D11.Buffer buffer = null;
            GI.Format format = GI.Format.Unknown;

            if (indexBuffer != null)
            {
                buffer = indexBuffer.NativeBuffer;
                format = indexBuffer.IndexFormat;
            }

            D3DDeviceContext.InputAssembler.SetIndexBuffer(buffer, format, 0);
        }

        /// <summary>
        /// Function to apply a pipeline state to the pipeline.
        /// </summary>
        /// <param name="state">A <see cref="GorgonPipelineState"/> to apply to the pipeline.</param>
        /// <param name="changes">The changes to the pipeline state to apply.</param>
        /// <remarks>
        /// <para>
        /// This is responsible for setting all the states for a pipeline at once. This has the advantage of ensuring that duplicate states do not get set so that performance is not impacted. 
        /// </para>
        /// </remarks>
        private void ApplyPipelineState(GorgonPipelineState state, PipelineStateChange changes)
        {
            if (changes == PipelineStateChange.None)
            {
                return;
            }

            if ((changes & PipelineStateChange.RasterState) == PipelineStateChange.RasterState)
            {
                D3DDeviceContext.Rasterizer.State = state?.D3DRasterState;
            }

            if ((changes & PipelineStateChange.DepthStencilState) == PipelineStateChange.DepthStencilState)
            {
                D3DDeviceContext.OutputMerger.DepthStencilState = state?.D3DDepthStencilState;
            }

            if ((changes & PipelineStateChange.BlendState) == PipelineStateChange.BlendState)
            {
                D3DDeviceContext.OutputMerger.BlendState = state?.D3DBlendState;
            }

            if ((changes & PipelineStateChange.VertexShader) == PipelineStateChange.VertexShader)
            {
                D3DDeviceContext.VertexShader.Set(state?.Info.VertexShader?.NativeShader);
            }

            if ((changes & PipelineStateChange.PixelShader) == PipelineStateChange.PixelShader)
            {
                D3DDeviceContext.PixelShader.Set(state?.Info.PixelShader?.NativeShader);
            }

            if ((changes & PipelineStateChange.GeometryShader) == PipelineStateChange.GeometryShader)
            {
                D3DDeviceContext.GeometryShader.Set(state?.Info.GeometryShader?.NativeShader);
            }

            if ((changes & PipelineStateChange.HullShader) == PipelineStateChange.HullShader)
            {
                D3DDeviceContext.HullShader.Set(state?.Info.HullShader?.NativeShader);
            }

            if ((changes & PipelineStateChange.DomainShader) == PipelineStateChange.DomainShader)
            {
                D3DDeviceContext.DomainShader.Set(state?.Info.DomainShader?.NativeShader);
            }
        }

        /// <summary>
        /// Function to assign viewports.
        /// </summary>
        private unsafe void SetViewports()
        {
            if (!Viewports.IsDirty)
            {
                return;
            }

            ref (int Start, int Count) viewports = ref Viewports.GetDirtyItems();
            RawViewportF* rawViewports = stackalloc RawViewportF[viewports.Count];

            for (int i = 0; i < viewports.Count; ++i)
            {
                rawViewports[i] = Viewports[i];
            }

            D3DDeviceContext.Rasterizer.SetViewports(rawViewports, viewports.Count);
        }

        /// <summary>
        /// Function to assign scissor rectangles.
        /// </summary>
        private void SetScissorRects()
        {
            // If there's been no change to the scissor rectangles, then we do nothing as the state should be the same as last time.
            if (!ScissorRectangles.IsDirty)
            {
                return;
            }

            ref (int Start, int Count) scissors = ref ScissorRectangles.GetDirtyItems();

            if (scissors.Count != _cachedScissors.Length)
            {
                if ((scissors.Count == 0) && (_cachedScissors.Length != 1))
                {
                    _cachedScissors = new[]
                                      {
                                          DX.Rectangle.Empty
                                      };
                }
                else if (scissors.Count != 0)
                {
                    _cachedScissors = new DX.Rectangle[scissors.Count];
                }
            }

            for (int i = 0; i < _cachedScissors.Length; ++i)
            {
                _cachedScissors[i] = ScissorRectangles[i];
            }

            D3DDeviceContext.Rasterizer.SetScissorRectangles(_cachedScissors);
        }

        /// <summary>
        /// Function to assign the vertex buffers.
        /// </summary>
        /// <param name="vertexBuffers">The vertex buffers to assign.</param>
        private void SetVertexBuffers(GorgonVertexBufferBindings vertexBuffers)
        {
            if (vertexBuffers == null)
            {
                D3DDeviceContext.InputAssembler.SetVertexBuffers(0);
                return;
            }

            ref (int StartSlot, int Count) bindings = ref vertexBuffers.GetDirtyItems();
            D3DDeviceContext.InputAssembler.SetVertexBuffers(bindings.StartSlot, vertexBuffers.Native);
        }

        /// <summary>
        /// Function to assign the stream output buffers.
        /// </summary>
        /// <param name="streamOutBuffers">The stream output buffers to assign.</param>
        private void SetStreamOutBuffers(GorgonStreamOutBindings streamOutBuffers)
        {
            if (streamOutBuffers == null)
            {
                if (_streamOutBuffer != null)
                {
                    Array.Clear(_streamOutBuffer, 0, _streamOutBuffer.Length);
                }
                D3DDeviceContext.StreamOutput.SetTargets(null);
                return;
            }

            (int, int Count) bindings = streamOutBuffers.GetDirtyItems();

            if ((_streamOutBuffer == null) || (_streamOutBuffer.Length != bindings.Count))
            {
                _streamOutBuffer = new D3D11.StreamOutputBufferBinding[bindings.Count];
            }

            for (int i = 0; i < bindings.Count; ++i)
            {
                _streamOutBuffer[i] = streamOutBuffers.Native[i];
            }

            D3DDeviceContext.StreamOutput.SetTargets(_streamOutBuffer);
        }

        /// <summary>
        /// Function to unbind a render target that is bound as a shader input.
        /// </summary>
        /// <param name="rtBindings">The render target view bindings.</param>
        private void UnbindRtvInputs(ref (int Start, int Count) rtBindings)
        {
            // This can happen quite easily due to how we're handling draw calls (i.e. stateless).  So we won't log anything here and we'll just unbind for the time being.
            // This may have a small performance penalty.

            if (_currentDrawCall == null)
            {
                return;
            }

            ref (int, int) psSrvBindings = ref _currentDrawCall.PixelShaderResourceViews.GetDirtyItems();
            ref (int, int) gsSrvBindings = ref _currentDrawCall.GeometryShaderResourceViews.GetDirtyItems();
            ref (int, int) vsSrvBindings = ref _currentDrawCall.VertexShaderResourceViews.GetDirtyItems();
            ref (int, int) hsSrvBindings = ref _currentDrawCall.HullShaderResourceViews.GetDirtyItems();
            ref (int, int) dsSrvBindings = ref _currentDrawCall.DomainShaderResourceViews.GetDirtyItems();

            // Unbind any depth/stencil bound as input.
            if ((_depthStencilView != null)
                && ((_depthStencilView.Texture.Info.Binding & TextureBinding.ShaderResource) == TextureBinding.ShaderResource)
                && (_depthStencilView.Flags == D3D11.DepthStencilViewFlags.None))
            {
                UnbindFromShader(ShaderType.Pixel, _depthStencilView.Texture, ref psSrvBindings, _currentDrawCall.PixelShaderResourceViews);
                UnbindFromShader(ShaderType.Geometry, _depthStencilView.Texture, ref gsSrvBindings, _currentDrawCall.GeometryShaderResourceViews);

                if (VideoDevice.RequestedFeatureLevel >= FeatureLevelSupport.Level_11_0)
                {
                    UnbindFromShader(ShaderType.Vertex, _depthStencilView.Texture, ref vsSrvBindings, _currentDrawCall.VertexShaderResourceViews);
                    UnbindFromShader(ShaderType.Hull, _depthStencilView.Texture, ref hsSrvBindings, _currentDrawCall.HullShaderResourceViews);
                    UnbindFromShader(ShaderType.Domain, _depthStencilView.Texture, ref dsSrvBindings, _currentDrawCall.DomainShaderResourceViews);
                }
            }

            for (int i = 0; i < rtBindings.Start + rtBindings.Count; ++i)
            {
                GorgonRenderTargetView view = _renderTargets[i];

                if ((view == null) || ((view.Texture.Info.Binding & TextureBinding.ShaderResource) != TextureBinding.ShaderResource))
                {
                    continue;
                }

                UnbindFromShader(ShaderType.Pixel, view.Texture, ref psSrvBindings, _currentDrawCall.PixelShaderResourceViews);
                UnbindFromShader(ShaderType.Geometry, view.Texture, ref gsSrvBindings, _currentDrawCall.GeometryShaderResourceViews);

                if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
                {
                    continue;
                }

                UnbindFromShader(ShaderType.Vertex, view.Texture, ref vsSrvBindings, _currentDrawCall.VertexShaderResourceViews);
                UnbindFromShader(ShaderType.Hull, view.Texture, ref hsSrvBindings, _currentDrawCall.HullShaderResourceViews);
                UnbindFromShader(ShaderType.Domain, view.Texture, ref dsSrvBindings, _currentDrawCall.DomainShaderResourceViews);
            }

            void UnbindFromShader(ShaderType shaderType, GorgonTexture renderTarget, ref (int Start, int Count) bindings, GorgonShaderResourceViews srvs)
            {
                if (bindings.Count == 0)
                {
                    return;
                }

                bool unbound = false;
                for (int i = bindings.Start; i < bindings.Start + bindings.Count; ++i)
                {
                    GorgonShaderResourceView srv = srvs[i];

                    if ((srv == null) || (renderTarget != srv.Resource))
                    {
                        continue;
                    }

                    srvs[i] = null;
                    unbound = true;
                }

                if (unbound)
                {
                    SetShaderResourceViews(shaderType, srvs);
                }
            }
        }

        /// <summary>
        /// Function to assign the render targets.
        /// </summary>
        private void SetRenderTargetAndDepthViews()
        {
            if ((!_renderTargets.IsDirty) && (!_depthStencilChanged))
            {
                return;
            }

            ref (int StartSlot, int Count) bindings = ref _renderTargets.GetDirtyItems();

#if DEBUG
            if (_currentDrawCall != null)
            {
                ref (int, int Count) uavBindings = ref _currentDrawCall.UnorderedAccessViews.GetDirtyItems();

                // If we attempt to bind a UAV with the resource already being bound, then that's a validation error.
                for (int i = 0; i < bindings.Count; ++i)
                {
                    GorgonRenderTargetView rtv = _renderTargets[i];

                    if ((rtv == null) || ((rtv.Texture.Info.Binding & TextureBinding.UnorderedAccess) != TextureBinding.UnorderedAccess))
                    {
                        continue;
                    }

                    for (int j = 0; j < uavBindings.Count; ++j)
                    {
                        GorgonUavBinding uavBinding = _currentDrawCall.UnorderedAccessViews[j];

                        if ((uavBinding.Uav == null) || (uavBinding.Uav.Resource != rtv.Texture))
                        {
                            continue;
                        }

                        throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_CONFLICT_RTV_UAV, rtv.Texture.Name, j));
                    }
                }
            }

            GorgonRenderTargetViews.ValidateDepthStencilView(DepthStencilView, RenderTargets.FirstOrDefault(item => item != null));
#endif

            // If we have any resources assigned to an RTV/DSV output, and they're already assigned to shader resource inputs, then we need to unbind them.
            UnbindRtvInputs(ref bindings);

            D3DDeviceContext.OutputMerger.SetTargets(DepthStencilView?.Native, bindings.Count, _renderTargets.Native);
        }

        /// <summary>
        /// Function to assign the constant buffers to the resource list.
        /// </summary>
        /// <param name="shaderType">The type of shader to set the resources on.</param>
        /// <param name="buffers">The constant buffers to assign.</param>
        private void SetShaderConstantBuffers(ShaderType shaderType, GorgonConstantBuffers buffers)
        {
            ref (int StartSlot, int Count) bindings = ref buffers.GetDirtyItems();

            switch (shaderType)
            {
                case ShaderType.Pixel:
                    D3DDeviceContext.PixelShader.SetConstantBuffers(bindings.StartSlot, bindings.Count, buffers.Native);
                    break;
                case ShaderType.Vertex:
                    D3DDeviceContext.VertexShader.SetConstantBuffers(bindings.StartSlot, bindings.Count, buffers.Native);
                    break;
                case ShaderType.Geometry:
                    D3DDeviceContext.GeometryShader.SetConstantBuffers(bindings.StartSlot, bindings.Count, buffers.Native);
                    break;
                case ShaderType.Hull:
                    D3DDeviceContext.HullShader.SetConstantBuffers(bindings.StartSlot, bindings.Count, buffers.Native);
                    break;
                case ShaderType.Domain:
                    D3DDeviceContext.DomainShader.SetConstantBuffers(bindings.StartSlot, bindings.Count, buffers.Native);
                    break;
            }
        }

        /// <summary>
        /// Function to bind unordered access views to the resource list.
        /// </summary>
        /// <param name="uavs">The unordered access views to bind.</param>
        private void SetUavs(GorgonUavBindings uavs)
        {
            ref (int StartSlot, int Count) bindings = ref uavs.GetDirtyItems();

            if (_uavBuffer.Length != bindings.Count)
            {
                _uavBuffer = new D3D11.UnorderedAccessView[bindings.Count];
                _uavCounters = new int[_uavBuffer.Length];
            }

            for (int i = 0; i < _uavBuffer.Length; ++i)
            {
                _uavBuffer[i] = uavs.Native[i];
                _uavCounters[i] = uavs.Counts[i];
            }

            D3DDeviceContext.OutputMerger.SetUnorderedAccessViews(bindings.StartSlot, _uavBuffer, _uavCounters);
        }

        /// <summary>
        /// Function to validate a shader resource input to ensure the resource it is connected with is not bound to a UAV, RTV or DSV output.
        /// </summary>
        /// <param name="shaderType">The type of shader being bound.</param>
        /// <param name="srvs">The list of shader resource views.</param>
        /// <param name="start">The starting index to validate from.</param>
        /// <param name="end">The ending index to validate to.</param>
        private void ValidateSrvBinding(ShaderType shaderType, GorgonShaderResourceViews srvs, int start, int end)
        {
            ref (int, int Count) rtBindings = ref _renderTargets.GetDirtyItems();
            ref (int Start, int Count) uavBindings = ref _currentDrawCall.UnorderedAccessViews.GetDirtyItems();

            for (int i = start; i < end; ++i)
            {
                GorgonShaderResourceView srv = srvs[i];

                if (srv == null)
                {
                    continue;
                }

                // Check to see if we're bound as a DSV.
                if (srv.Resource == DepthStencilView?.Texture)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_CONFLICT_SRV_DSV, srv.Resource.Name, shaderType));
                }

                for (int j = 0; j < rtBindings.Count; ++j)
                {
                    GorgonRenderTargetView rtv = _renderTargets[j];

                    // If we're trying to bind a shader resource while it's held as a render target, then reset the render target to null.
                    if ((rtv == null) || (rtv.Texture != srv.Resource))
                    {
                        continue;
                    }

                    throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_CONFLICT_SRV_RTV, srv.Resource.Name, shaderType));
                }

                // Evaluate unordered access views.
                for (int j = uavBindings.Start; j < uavBindings.Start + uavBindings.Count; ++j)
                {
                    GorgonUavBinding uav = _currentDrawCall.UnorderedAccessViews[j];

                    if ((uav.Uav == null)
                        || (uav.Uav.Resource != srv.Resource))
                    {
                        continue;
                    }

                    throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_CONFLICT_SRV_UAV, srv.Resource.Name, j, shaderType));
                }
            }
        }

        /// <summary>
        /// Function to assign the shader resource views to the resource list.
        /// </summary>
        /// <param name="shaderType">The type of shader to set the resources on.</param>
        /// <param name="srvs">The shader resource views to assign.</param>
        private void SetShaderResourceViews(ShaderType shaderType, GorgonShaderResourceViews srvs)
        {
            ref (int StartSlot, int Count) bindings = ref srvs.GetDirtyItems();

            switch (shaderType)
            {
                case ShaderType.Pixel:
                    D3DDeviceContext.PixelShader.SetShaderResources(bindings.StartSlot, bindings.Count, srvs.Native);
                    break;
                case ShaderType.Vertex:
                    D3DDeviceContext.VertexShader.SetShaderResources(bindings.StartSlot, bindings.Count, srvs.Native);
                    break;
                case ShaderType.Geometry:
                    D3DDeviceContext.GeometryShader.SetShaderResources(bindings.StartSlot, bindings.Count, srvs.Native);
                    break;
                case ShaderType.Hull:
                    D3DDeviceContext.HullShader.SetShaderResources(bindings.StartSlot, bindings.Count, srvs.Native);
                    break;
                case ShaderType.Domain:
                    D3DDeviceContext.DomainShader.SetShaderResources(bindings.StartSlot, bindings.Count, srvs.Native);
                    break;
            }
        }

        /// <summary>
        /// Function to assign the shader samplers to the resource list.
        /// </summary>
        /// <param name="shaderType">The type of shader to set the resources on.</param>
        /// <param name="samplers">The samplers to assign.</param>
        private void SetShaderSamplers(ShaderType shaderType, GorgonSamplerStates samplers)
        {
            ref (int StartSlot, int Count) bindings = ref samplers.GetDirtyItems();

            for (int i = bindings.StartSlot; i < bindings.StartSlot + bindings.Count; ++i)
            {
                GorgonSamplerState state = samplers[i];

                if (state == null)
                {
                    _samplerStates[i - bindings.StartSlot] = null;
                    continue;
                }

                if (state.Native == null)
                {
                    state.Native = SamplerStateFactory.GetSamplerState(this, state, _log);
                }

                _samplerStates[i - bindings.StartSlot] = state.Native;
            }

            switch (shaderType)
            {
                case ShaderType.Pixel:
                    D3DDeviceContext.PixelShader.SetSamplers(bindings.StartSlot, bindings.Count, _samplerStates);
                    break;
                case ShaderType.Vertex:
                    D3DDeviceContext.VertexShader.SetSamplers(bindings.StartSlot, bindings.Count, _samplerStates);
                    break;
                case ShaderType.Geometry:
                    D3DDeviceContext.GeometryShader.SetSamplers(bindings.StartSlot, bindings.Count, _samplerStates);
                    break;
                case ShaderType.Hull:
                    D3DDeviceContext.HullShader.SetSamplers(bindings.StartSlot, bindings.Count, _samplerStates);
                    break;
                case ShaderType.Domain:
                    D3DDeviceContext.DomainShader.SetSamplers(bindings.StartSlot, bindings.Count, _samplerStates);
                    break;
            }
        }

        /// <summary>
        /// Function to apply states that can be changed per draw call.
        /// </summary>
        /// <param name="drawCall">The draw call containing the direct states to change.</param>
        /// <param name="newState">The current pipeline state settings for the new draw call.</param>
        /// <param name="resourceChanges">The resource changes to apply.</param>
        /// <param name="stateChanges">The pipeline state changes to apply.</param>
        private void ApplyPerDrawStates(GorgonDrawCallBase drawCall,
                                        GorgonPipelineState newState,
                                        PipelineResourceChange resourceChanges,
                                        PipelineStateChange stateChanges)
        {
            if ((resourceChanges & PipelineResourceChange.PrimitiveTopology) == PipelineResourceChange.PrimitiveTopology)
            {
                D3DDeviceContext.InputAssembler.PrimitiveTopology = drawCall.PrimitiveTopology;
            }

            // Bind the scissor rectangles.
            SetScissorRects();
            // Bind the active viewports.
            SetViewports();

            if ((resourceChanges & PipelineResourceChange.Uavs) == PipelineResourceChange.Uavs)
            {
                SetUavs(drawCall.UnorderedAccessViews);
            }

            if ((resourceChanges & PipelineResourceChange.InputLayout) == PipelineResourceChange.InputLayout)
            {
                D3DDeviceContext.InputAssembler.InputLayout = drawCall.VertexBuffers?.InputLayout.D3DInputLayout;
            }

            if ((resourceChanges & PipelineResourceChange.StreamOut) == PipelineResourceChange.StreamOut)
            {
                SetStreamOutBuffers(drawCall.StreamOutBuffers);
            }

            if ((resourceChanges & PipelineResourceChange.VertexBuffers) == PipelineResourceChange.VertexBuffers)
            {
                SetVertexBuffers(drawCall.VertexBuffers);
            }

            if ((resourceChanges & PipelineResourceChange.IndexBuffer) == PipelineResourceChange.IndexBuffer)
            {
                SetIndexbuffer(drawCall.IndexBuffer);
            }

            if ((resourceChanges & PipelineResourceChange.VertexShaderConstantBuffers) == PipelineResourceChange.VertexShaderConstantBuffers)
            {
                SetShaderConstantBuffers(ShaderType.Vertex, drawCall.VertexShaderConstantBuffers);
            }

            if ((resourceChanges & PipelineResourceChange.PixelShaderConstantBuffers) == PipelineResourceChange.PixelShaderConstantBuffers)
            {
                SetShaderConstantBuffers(ShaderType.Pixel, drawCall.PixelShaderConstantBuffers);
            }

            if ((resourceChanges & PipelineResourceChange.GeometryShaderConstantBuffers) == PipelineResourceChange.GeometryShaderConstantBuffers)
            {
                SetShaderConstantBuffers(ShaderType.Geometry, drawCall.GeometryShaderConstantBuffers);
            }

            if ((resourceChanges & PipelineResourceChange.HullShaderConstantBuffers) == PipelineResourceChange.HullShaderConstantBuffers)
            {
                SetShaderConstantBuffers(ShaderType.Hull, drawCall.HullShaderConstantBuffers);
            }

            if ((resourceChanges & PipelineResourceChange.DomainShaderConstantBuffers) == PipelineResourceChange.DomainShaderConstantBuffers)
            {
                SetShaderConstantBuffers(ShaderType.Domain, drawCall.DomainShaderConstantBuffers);
            }

            if ((resourceChanges & PipelineResourceChange.VertexShaderResources) == PipelineResourceChange.VertexShaderResources)
            {
                SetShaderResourceViews(ShaderType.Vertex, drawCall.VertexShaderResourceViews);
            }

            if ((resourceChanges & PipelineResourceChange.PixelShaderResources) == PipelineResourceChange.PixelShaderResources)
            {
                SetShaderResourceViews(ShaderType.Pixel, drawCall.PixelShaderResourceViews);
            }

            if ((resourceChanges & PipelineResourceChange.GeometryShaderResources) == PipelineResourceChange.GeometryShaderResources)
            {
                SetShaderResourceViews(ShaderType.Geometry, drawCall.GeometryShaderResourceViews);
            }

            if ((resourceChanges & PipelineResourceChange.HullShaderResources) == PipelineResourceChange.HullShaderResources)
            {
                SetShaderResourceViews(ShaderType.Hull, drawCall.HullShaderResourceViews);
            }

            if ((resourceChanges & PipelineResourceChange.DomainShaderResources) == PipelineResourceChange.DomainShaderResources)
            {
                SetShaderResourceViews(ShaderType.Domain, drawCall.DomainShaderResourceViews);
            }

            if ((resourceChanges & PipelineResourceChange.VertexShaderSamplers) == PipelineResourceChange.VertexShaderSamplers)
            {
                SetShaderSamplers(ShaderType.Vertex, drawCall.VertexShaderSamplers);
            }

            if ((resourceChanges & PipelineResourceChange.PixelShaderSamplers) == PipelineResourceChange.PixelShaderSamplers)
            {
                SetShaderSamplers(ShaderType.Pixel, drawCall.PixelShaderSamplers);
            }

            if ((resourceChanges & PipelineResourceChange.GeometryShaderSamplers) == PipelineResourceChange.GeometryShaderSamplers)
            {
                SetShaderSamplers(ShaderType.Geometry, drawCall.GeometryShaderSamplers);
            }

            if ((resourceChanges & PipelineResourceChange.HullShaderSamplers) == PipelineResourceChange.HullShaderSamplers)
            {
                SetShaderSamplers(ShaderType.Hull, drawCall.HullShaderSamplers);
            }

            if ((resourceChanges & PipelineResourceChange.DomainShaderSamplers) == PipelineResourceChange.DomainShaderSamplers)
            {
                SetShaderSamplers(ShaderType.Domain, drawCall.DomainShaderSamplers);
            }

            if ((resourceChanges & PipelineResourceChange.BlendFactor) == PipelineResourceChange.BlendFactor)
            {
                D3DDeviceContext.OutputMerger.BlendFactor = drawCall.BlendFactor.ToRawColor4();
            }

            if ((resourceChanges & PipelineResourceChange.BlendSampleMask) == PipelineResourceChange.BlendSampleMask)
            {
                D3DDeviceContext.OutputMerger.BlendSampleMask = drawCall.BlendSampleMask;
            }

            if ((resourceChanges & PipelineResourceChange.DepthStencilReference) == PipelineResourceChange.DepthStencilReference)
            {
                D3DDeviceContext.OutputMerger.DepthStencilReference = drawCall.DepthStencilReference;
            }

            if (stateChanges != PipelineStateChange.None)
            {
                ApplyPipelineState(newState, stateChanges);
            }
        }

        /// <summary>
        /// Function to force UAVs that are bound as inputs to be unbound from the input stages.
        /// </summary>
        /// <param name="uavBindings">The binding range.</param>
        /// <param name="uavs">The list of bindings.</param>
        /// <param name="forceUnbind"><b>true</b> to force the slots to unbind from the GPU, or <b>false</b> to delay the unbinding until later.</param>
        private void UnbindUavInputs(ref (int Start, int Count) uavBindings, GorgonUavBindings uavs, bool forceUnbind)
        {
            if ((uavs == null) || (_currentDrawCall == null))
            {
                return;
            }

            ref (int, int) psSrvBindings = ref _currentDrawCall.PixelShaderResourceViews.GetDirtyItems();
            ref (int, int) gsSrvBindings = ref _currentDrawCall.GeometryShaderResourceViews.GetDirtyItems();
            ref (int, int) vsSrvBindings = ref _currentDrawCall.VertexShaderResourceViews.GetDirtyItems();
            ref (int, int) hsSrvBindings = ref _currentDrawCall.HullShaderResourceViews.GetDirtyItems();
            ref (int, int) dsSrvBindings = ref _currentDrawCall.DomainShaderResourceViews.GetDirtyItems();

            for (int i = uavBindings.Start; i < uavBindings.Start + uavBindings.Count; ++i)
            {
                GorgonUavBinding uavBinding = uavs[i];

                if ((uavBinding.Uav == null) || (!uavBinding.Uav.Resource.IsShaderResource))
                {
                    continue;
                }

                UnbindFromShader(ShaderType.Pixel, uavBinding.Uav.Resource, ref psSrvBindings, _currentDrawCall.PixelShaderResourceViews);
                UnbindFromShader(ShaderType.Geometry, uavBinding.Uav.Resource, ref gsSrvBindings, _currentDrawCall.GeometryShaderResourceViews);

                if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
                {
                    continue;
                }

                UnbindFromShader(ShaderType.Vertex, uavBinding.Uav.Resource, ref vsSrvBindings, _currentDrawCall.VertexShaderResourceViews);
                UnbindFromShader(ShaderType.Hull, uavBinding.Uav.Resource, ref hsSrvBindings, _currentDrawCall.HullShaderResourceViews);
                UnbindFromShader(ShaderType.Domain, uavBinding.Uav.Resource, ref dsSrvBindings, _currentDrawCall.DomainShaderResourceViews);
            }

            void UnbindFromShader(ShaderType shaderType, GorgonGraphicsResource resource, ref (int Start, int Count) bindings, GorgonShaderResourceViews srvs)
            {
                bool unbound = false;

                if (bindings.Count == 0)
                {
                    return;
                }

                for (int i = bindings.Start; i < bindings.Start + bindings.Count; ++i)
                {
                    GorgonShaderResourceView srv = srvs[i];

                    if ((srv == null) || (resource != srv.Resource))
                    {
                        continue;
                    }

                    srvs[i] = null;
                    unbound = true;
                }

                if ((forceUnbind) && (unbound))
                {
                    SetShaderResourceViews(shaderType, srvs);
                }
            }
        }

        /// <summary>
        /// Function to validate the compute pipeline state against the graphics pipeline state.
        /// </summary>
        internal void ValidateComputeWork(GorgonUavBindings uavBindings, ref (int Start, int Count) uavRange)
        {
            // Unbind any UAVs bound as input.
            if (uavRange.Count > 0)
            {
                UnbindUavInputs(ref uavRange, uavBindings, true);
            }
        }

        /// <summary>
        /// Function to assign a single render target to the first slot.
        /// </summary>
        /// <param name="renderTarget">The render target view to assign.</param>
        /// <remarks>
        /// <para>
        /// This will assign a render target in slot 0 of the <see cref="RenderTargets"/> list. If the <paramref name="renderTarget"/> has an associated depth/stencil buffer, then that will be assigned to the 
        /// <see cref="DepthStencilView"/>. If it does not, the <see cref="DepthStencilView"/> will be set to <b>null</b>.
        /// </para>
        /// <para>
        /// When a render target is set, the first scissor rectangle in the <see cref="ScissorRectangles"/> list and the first viewport in the <see cref="Viewports"/> list will be reset to the size of the 
        /// render target. The user is responsible for restoring these to their intended values after assigning the target.
        /// </para>
        /// <para>
        /// <note type="information">
        /// <para>
        /// If the <see cref="RenderTargets"/> list contains other render target views at different slots, they will be unbound.
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// If a user wishes to assign their own <see cref="GorgonDepthStencilView"/>, then use the <see cref="SetRenderTarget(GorgonRenderTargetView, GorgonDepthStencilView)"/> overload.
        /// </para>
        /// <para>
        /// If multiple render targets need to be assigned at the same time, use the <see cref="SetRenderTargets(GorgonRenderTargetView[], GorgonDepthStencilView)"/> overload.
        /// </para>
        /// </remarks>
        /// <seealso cref="SetRenderTarget(GorgonRenderTargetView, GorgonDepthStencilView)"/>
        /// <seealso cref="SetRenderTargets(GorgonRenderTargetView[], GorgonDepthStencilView)"/>
        /// <seealso cref="RenderTargets"/>
        /// <seealso cref="DepthStencilView"/>
        /// <seealso cref="ScissorRectangles"/>
        /// <seealso cref="Viewports"/>
        public void SetRenderTarget(GorgonRenderTargetView renderTarget)
        {
            SetRenderTarget(renderTarget, renderTarget?.DepthStencilView);
        }

        /// <summary>
        /// Function to assign a render target to the first slot and a custom depth/stencil view.
        /// </summary>
        /// <param name="renderTarget">The render target view to assign.</param>
        /// <param name="depthStencil">The depth/stencil view to assign.</param>
        /// <remarks>
        /// <para>
        /// This will assign a render target in slot 0 of the <see cref="RenderTargets"/> list. Any associated depth/stencil buffer for the render target will be ignored and the value assigned to 
        /// <paramref name="depthStencil"/> will be used instead.
        /// </para>
        /// <para>
        /// <note type="information">
        /// <para>
        /// If the <see cref="RenderTargets"/> list contains other render target views at different slots, they will be unbound.
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// If a user wishes to use the associated <see cref="GorgonDepthStencilView"/> for the <paramref name="renderTarget"/>, then use the <see cref="SetRenderTarget(GorgonRenderTargetView)"/> overload.
        /// </para>
        /// <para>
        /// If multiple render targets need to be assigned at the same time, use the <see cref="SetRenderTargets(GorgonRenderTargetView[], GorgonDepthStencilView)"/> overload.
        /// </para>
        /// <para>
        /// When binding a <see cref="GorgonDepthStencilView"/>, the resource must be of the same type as other resources for other views in this list. If they do not match, an exception will be thrown.
        /// </para>
        /// <para>
        /// The <see cref="GorgonDepthStencilView"/> values for the <paramref name="depthStencil"/> (such as array (or depth) index and array (or depth) count) must be the same as the other views in this list. 
        /// If they are not, an exception will be thrown. Mip slices may be different. An exception will also be raised if the resources assigned to the <paramref name="renderTarget"/> do not have the same 
        /// array/depth count.
        /// </para>
        /// <para>
        /// If the <see cref="GorgonRenderTargetView">GorgonRenderTargetViews</see> are attached to resources with multisampling enabled through <see cref="GorgonMultisampleInfo"/>, then the 
        /// <see cref="GorgonMultisampleInfo"/> of the resource attached to the <see cref="GorgonDepthStencilView"/> being assigned must match, or an exception will be thrown.
        /// </para>
        /// <para>
        /// When a render target is set, the first scissor rectangle in the <see cref="ScissorRectangles"/> list and the first viewport in the <see cref="Viewports"/> list will be reset to the size of the 
        /// render target. The user is responsible for restoring these to their intended values after assigning the target.
        /// </para>
        /// <para>
        /// <note type="information">
        /// <para>
        /// The exceptions raised when validating a view against other views in this list are only thrown when Gorgon is compiled as <b>DEBUG</b>.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="SetRenderTarget(GorgonRenderTargetView)"/>
        /// <seealso cref="SetRenderTargets(GorgonRenderTargetView[], GorgonDepthStencilView)"/>
        /// <seealso cref="RenderTargets"/>
        /// <seealso cref="DepthStencilView"/>
        /// <seealso cref="ScissorRectangles"/>
        /// <seealso cref="RenderTargets"/>
        public void SetRenderTarget(GorgonRenderTargetView renderTarget, GorgonDepthStencilView depthStencil)
        {
            if ((_renderTargets[0] == renderTarget) && (depthStencil == DepthStencilView))
            {
                return;
            }

            _renderTargets.Clear();
            _renderTargets[0] = renderTarget;

            if (_renderTargets[0] != null)
            {
                ScissorRectangles[0] = _renderTargets[0].Bounds;
                Viewports[0] = new DX.ViewportF(0, 0, _renderTargets[0].Bounds.Width, _renderTargets[0].Bounds.Height);
            }
            else
            {
                ScissorRectangles[0] = DX.Rectangle.Empty;
                Viewports[0] = default(DX.ViewportF);
            }

            DepthStencilView = depthStencil;
            SetRenderTargetAndDepthViews();
        }

        /// <summary>
        /// Function to assign multiple render targets to the first slot and a custom depth/stencil view.
        /// </summary>
        /// <param name="renderTargets">The list of render target views to assign.</param>
        /// <param name="depthStencil">The depth/stencil view to assign.</param>
        /// <remarks>
        /// <para>
        /// This will assign multiple render targets to the corresponding slots in the <see cref="RenderTargets"/> list. Any associated depth/stencil buffer for the render target will be ignored and the value 
        /// assigned to <paramref name="depthStencil"/> will be used instead.
        /// </para>
        /// <para>
        /// If a user wishes to use the associated <see cref="GorgonDepthStencilView"/> for the <paramref name="renderTargets"/>, then use the <see cref="SetRenderTarget(GorgonRenderTargetView)"/> overload.
        /// </para>
        /// <para>
        /// When binding a <see cref="GorgonDepthStencilView"/>, the resource must be of the same type as other resources for other views in this list. If they do not match, an exception will be thrown.
        /// </para>
        /// <para>
        /// The <see cref="GorgonDepthStencilView"/> values for the <paramref name="depthStencil"/> (such as array (or depth) index and array (or depth) count) must be the same as the other views in this list. 
        /// If they are not, an exception will be thrown. Mip slices may be different. An exception will also be raised if the resources assigned to the <paramref name="renderTargets"/> do not have the same 
        /// array/depth count.
        /// </para>
        /// <para>
        /// If the <see cref="GorgonRenderTargetView">GorgonRenderTargetViews</see> are attached to resources with multisampling enabled through <see cref="GorgonMultisampleInfo"/>, then the 
        /// <see cref="GorgonMultisampleInfo"/> of the resource attached to the <see cref="GorgonDepthStencilView"/> being assigned must match, or an exception will be thrown.
        /// </para>
        /// <para>
        /// The format for the <paramref name="renderTargets"/> and <paramref name="depthStencil"/> may differ from the formats of other views passed in.
        /// </para>
        /// <para>
        /// When a render target is set, the first scissor rectangle in the <see cref="ScissorRectangles"/> list and the first viewport in the <see cref="Viewports"/> list will be reset to the size of the 
        /// first render target. The user is responsible for restoring these to their intended values after assigning the targets.
        /// </para>
        /// <para>
        /// <note type="information">
        /// <para>
        /// The exceptions raised when validating a view against other views in this list are only thrown when Gorgon is compiled as <b>DEBUG</b>.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="SetRenderTarget(GorgonRenderTargetView)"/>
        /// <seealso cref="SetRenderTarget(GorgonRenderTargetView, GorgonDepthStencilView)"/>
        /// <seealso cref="RenderTargets"/>
        /// <seealso cref="DepthStencilView"/>
        /// <seealso cref="ScissorRectangles"/>
        /// <seealso cref="Viewports"/>
        public void SetRenderTargets(GorgonRenderTargetView[] renderTargets, GorgonDepthStencilView depthStencil = null)
        {
            _renderTargets.Clear();

            if ((renderTargets == null)
                || (renderTargets.Length == 0))
            {
                ScissorRectangles[0] = DX.Rectangle.Empty;
                DepthStencilView = depthStencil;
                SetRenderTargetAndDepthViews();
                return;
            }

            for (int i = 0; i < renderTargets.Length.Min(_renderTargets.Count); ++i)
            {
                _renderTargets[i] = renderTargets[i];
            }

            if (_renderTargets[0] != null)
            {
                ScissorRectangles[0] = renderTargets[0].Bounds;
                Viewports[0] = new DX.ViewportF(0, 0, renderTargets[0].Bounds.Width, renderTargets[0].Bounds.Height);
            }
            else
            {
                ScissorRectangles[0] = DX.Rectangle.Empty;
                Viewports[0] = default(DX.ViewportF);
            }

            if ((!_renderTargets.IsDirty) && (DepthStencilView == depthStencil))
            {
                return;
            }

            DepthStencilView = depthStencil;
            SetRenderTargetAndDepthViews();
        }

        /// <summary>
        /// Function to clear the cached pipeline states.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will destroy any previously cached pipeline states. Because of this, any states that were previously created must be re-created using the <seealso cref="GetPipelineState"/> method.
        /// </para>
        /// </remarks>
        public void ClearStateCache()
        {
            if (D3DDeviceContext != null)
            {
                ClearState();
            }

            lock (_stateCacheLock)
            {
                // Ensure that all groups lose their reference to the cached pipeline states first.
                foreach (KeyValuePair<string, IPipelineStateGroup> cacheGroup in _groupedCache)
                {
                    cacheGroup.Value.Invalidate();
                }

                // Wipe out the state cache.
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < _stateCache.Count; ++i)
                {
                    _stateCache[i].D3DRasterState?.Dispose();
                    _stateCache[i].D3DDepthStencilState?.Dispose();
                    _stateCache[i].D3DBlendState?.Dispose();
                }

                _stateCache.Clear();

                SamplerStateFactory.ClearCache();
            }
        }

        /// <summary>
        /// Function to submit a <see cref="GorgonDrawIndexedInstancedCall"/> to the GPU using a <see cref="GorgonIndirectArgumentBuffer"/> to pass in variable sized arguments.
        /// </summary>
        /// <param name="drawCall">The draw call to submit.</param>
        /// <param name="indirectArgs">The buffer containing the draw call arguments to pass.</param>
        /// <param name="argumentOffset">[Optional] The offset, in bytes, within the buffer to start reading the arguments from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/>, or the <paramref name="indirectArgs"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="argumentOffset"/> parameter is less than 0.</exception>
        /// <remarks>
        /// <para>
        /// This allows submitting a <see cref="GorgonDrawIndexedInstancedCall"/> with variable arguments without having to perform a read back of that data from the GPU and therefore avoid a stall. 
        /// </para>
        /// <para>
        /// Like the <see cref="SubmitStreamOut"/> method, this is useful when a shader generates an arbitrary amount of data within a buffer. To get the size, or the data itself out of the buffer will 
        /// cause a stall when swtiching back to the CPU. This is obviously not good for performance. So, to counter this, this method will pass the buffer with the arguments for the draw call straight 
        /// through without having to get the CPU to read the data back, thus avoiding the stall.
        /// </para>
        /// <para>
        /// <note type="caution">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonDrawIndexedInstancedCall"/>
        public void SubmitIndirect(GorgonDrawIndexedInstancedCall drawCall, GorgonIndirectArgumentBuffer indirectArgs, int argumentOffset = 0)
        {
            drawCall.ValidateObject(nameof(drawCall));
            indirectArgs.ValidateObject(nameof(indirectArgs));

            // Merge this draw call with our previous one (if available).
            (PipelineResourceChange ChangedResources, PipelineStateChange ChangedStates) stateChange = MergeDrawCall(drawCall);

            ApplyPerDrawStates(_currentDrawCall, drawCall.PipelineState, stateChange.ChangedResources, stateChange.ChangedStates);

            D3DDeviceContext.DrawIndexedInstancedIndirect(indirectArgs.NativeBuffer, argumentOffset);
        }

        /// <summary>
        /// Function to submit a <see cref="GorgonDrawInstancedCall"/> to the GPU using a <see cref="GorgonIndirectArgumentBuffer"/> to pass in variable sized arguments.
        /// </summary>
        /// <param name="drawCall">The draw call to submit.</param>
        /// <param name="indirectArgs">The buffer containing the draw call arguments to pass.</param>
        /// <param name="argumentOffset">[Optional] The offset, in bytes, within the buffer to start reading the arguments from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/>, or the <paramref name="indirectArgs"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="argumentOffset"/> parameter is less than 0.</exception>
        /// <remarks>
        /// <para>
        /// This allows submitting a <see cref="GorgonDrawInstancedCall"/> with variable arguments without having to perform a read back of that data from the GPU and therefore avoid a stall. 
        /// </para>
        /// <para>
        /// Like the <see cref="SubmitStreamOut"/> method, this is useful when a shader generates an arbitrary amount of data within a buffer. To get the size, or the data itself out of the buffer will 
        /// cause a stall when swtiching back to the CPU. This is obviously not good for performance. So, to counter this, this method will pass the buffer with the arguments for the draw call straight 
        /// through without having to get the CPU to read the data back, thus avoiding the stall.
        /// </para>
        /// <para>
        /// <note type="caution">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonDrawIndexedInstancedCall"/>
        public void SubmitIndirect(GorgonDrawInstancedCall drawCall, GorgonIndirectArgumentBuffer indirectArgs, int argumentOffset = 0)
        {
            drawCall.ValidateObject(nameof(drawCall));
            indirectArgs.ValidateObject(nameof(indirectArgs));

            // Merge this draw call with our previous one (if available).
            (PipelineResourceChange ChangedResources, PipelineStateChange ChangedStates) stateChange = MergeDrawCall(drawCall);

            ApplyPerDrawStates(_currentDrawCall, drawCall.PipelineState, stateChange.ChangedResources, stateChange.ChangedStates);

            D3DDeviceContext.DrawInstancedIndirect(indirectArgs.NativeBuffer, argumentOffset);
        }

        /// <summary>
        /// Function to submit a <see cref="GorgonDrawCallBase"/> to the GPU.
        /// </summary>
        /// <param name="drawCall">The draw call to submit.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This method sends a series of state changes and resource bindings to the GPU. However, unlike the <see cref="O:Gorgon.Graphics.Core.GorgonGraphics.Submit"/> commands, this command uses 
        /// pre-processed data from the vertex and stream out stages. This means that the <see cref="GorgonVertexBuffer"/> attached to the draw call must have been assigned to the 
        /// <see cref="GorgonDrawCallBase.StreamOutBuffers"/> and had data deposited into it from the stream out stage. After that, it should be removed from the 
        /// <see cref="GorgonDrawCallBase.StreamOutBuffers"/> and assigned to the <see cref="GorgonDrawCallBase.VertexBuffers"/> on the <paramref name="drawCall"/> passed to this method.
        /// </para>
        /// <para>
        /// To render data with this method, the <see cref="GorgonVertexBuffer"/> being rendered must be at slot 0 in the <see cref="GorgonDrawCallBase.VertexBuffers"/> list on the 
        /// <paramref name="drawCall"/> passed to the method. This buffer must be created with the <see cref="VertexIndexBufferBinding.StreamOut"/> flag set.
        /// </para>
        /// <para>
        /// Draw calls with a start and count property (for indices, vertices, etc...) will work with this method, but those properties are ignored because the actual size of the data being sent is unknown 
        /// at the application level. The GPU will track the size of the buffer being rendered. The <see cref="IGorgonPipelineStateInfo.VertexShader"/> of the <see cref="GorgonDrawCallBase.PipelineState"/> 
        /// will be ignored as well as the vertex data being passed is already processed by a vertex shader.
        /// </para>
        /// <para>
        /// This method does not support the use of a <see cref="GorgonIndexBuffer"/>, if one is bound to the draw call, it will be unbound and a warning will go to the log.
        /// </para>
        /// <para>
        /// <note type="caution">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void SubmitStreamOut(GorgonDrawCallBase drawCall)
        {
            drawCall.ValidateObject(nameof(drawCall));

            // Merge this draw call with our previous one (if available).
            (PipelineResourceChange ChangedResources, PipelineStateChange ChangedStates) stateChange = MergeDrawCall(drawCall);

            // Unbind the index buffer if one is present.
            if (_currentDrawCall.IndexBuffer != null)
            {
                _log.Print("The SubmitStreamOut method does not support the use of index buffers. The current index buffer will be reset to NULL.",
                           LoggingLevel.Verbose);
                _currentDrawCall.IndexBuffer = null;
                stateChange.ChangedResources |= PipelineResourceChange.IndexBuffer;
            }

#if DEBUG
            if (_currentDrawCall.PipelineState.Info.VertexShader != null)
            {
                _log.Print("The SubmitStreamOut method has a vertex shader bound to its pipeline state. This may have unintended effects on the rendered geometry.",
                           LoggingLevel.Verbose);
            }
#endif

            ApplyPerDrawStates(_currentDrawCall, drawCall.PipelineState, stateChange.ChangedResources, stateChange.ChangedStates);

            D3DDeviceContext.DrawAuto();
        }

        /// <summary>
        /// Function to submit a <see cref="GorgonDrawIndexedCall"/> to the GPU.
        /// </summary>
        /// <param name="drawCall">The draw call to submit.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This method sends a series of state changes and resource bindings to the GPU along with a command to render primitive data with a <see cref="GorgonIndexBuffer"/>.
        /// </para>
        /// <para>
        /// <note type="caution">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonIndexBuffer"/>
        public void Submit(GorgonDrawIndexedCall drawCall)
        {
            drawCall.ValidateObject(nameof(drawCall));

            // Merge this draw call with our previous one (if available).
            (PipelineResourceChange ChangedResources, PipelineStateChange ChangedStates) stateChange = MergeDrawCall(drawCall);

            ApplyPerDrawStates(_currentDrawCall, drawCall.PipelineState, stateChange.ChangedResources, stateChange.ChangedStates);

            D3DDeviceContext.DrawIndexed(drawCall.IndexCount, drawCall.IndexStart, drawCall.BaseVertexIndex);
        }

        /// <summary>
        /// Function to submit a <see cref="GorgonDrawCall"/> to the GPU.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/> parameter is <b>null</b>.</exception>
        /// <param name="drawCall">The draw call to submit.</param>
        /// <remarks>
        /// <para>
        /// This method sends a series of state changes and resource bindings to the GPU along with a command to render primitive data.
        /// </para>
        /// <para>
        /// <note type="caution">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void Submit(GorgonDrawCall drawCall)
        {
            drawCall.ValidateObject(nameof(drawCall));

            // Merge this draw call with our previous one (if available).
            (PipelineResourceChange ChangedResources, PipelineStateChange ChangedStates) stateChange = MergeDrawCall(drawCall);

            ApplyPerDrawStates(_currentDrawCall, drawCall.PipelineState, stateChange.ChangedResources, stateChange.ChangedStates);

            D3DDeviceContext.Draw(drawCall.VertexCount, drawCall.VertexStartIndex);
        }

        /// <summary>
        /// Function to submit a <see cref="GorgonDrawInstancedCall"/> to the GPU.
        /// </summary>
        /// <param name="drawCall">The draw call to submit.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This method sends a series of state changes and resource bindings to the GPU along with a command to render instanced primitive data.
        /// </para>
        /// <para>
        /// <note type="caution">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void Submit(GorgonDrawInstancedCall drawCall)
        {
            drawCall.ValidateObject(nameof(drawCall));

            // Merge this draw call with our previous one (if available).
            (PipelineResourceChange ChangedResources, PipelineStateChange ChangedStates) stateChange = MergeDrawCall(drawCall);

            ApplyPerDrawStates(_currentDrawCall, drawCall.PipelineState, stateChange.ChangedResources, stateChange.ChangedStates);

            D3DDeviceContext.DrawInstanced(drawCall.VertexCountPerInstance, drawCall.InstanceCount, drawCall.VertexStartIndex, drawCall.StartInstanceIndex);
        }

        /// <summary>
        /// Function to submit a <see cref="GorgonDrawIndexedInstancedCall"/> to the GPU.
        /// </summary>
        /// <param name="drawCall">The draw call to submit.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This method sends a series of state changes and resource bindings to the GPU along with a command to render instanced primitive data with a <see cref="GorgonIndexBuffer"/>.
        /// </para>
        /// <para>
        /// <note type="caution">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonIndexBuffer"/>
        public void Submit(GorgonDrawIndexedInstancedCall drawCall)
        {
            drawCall.ValidateObject(nameof(drawCall));

            // Merge this draw call with our previous one (if available).
            (PipelineResourceChange ChangedResources, PipelineStateChange ChangedStates) stateChange = MergeDrawCall(drawCall);

            ApplyPerDrawStates(_currentDrawCall, drawCall.PipelineState, stateChange.ChangedResources, stateChange.ChangedStates);

            D3DDeviceContext.DrawIndexedInstanced(drawCall.IndexCountPerInstance,
                                                  drawCall.InstanceCount,
                                                  drawCall.IndexStart,
                                                  drawCall.BaseVertexIndex,
                                                  drawCall.StartInstanceIndex);
        }

        /// <summary>
        /// Function to render a texture to the current <see cref="GorgonRenderTargetView"/>.
        /// </summary>
        /// <param name="texture">The texture to render.</param>
        /// <param name="x">The horizontal destination position to render into.</param>
        /// <param name="y">The vertical destination position to render into.</param>
        /// <param name="width">[Optional] The destination width to render.</param>
        /// <param name="height">[Optional] The destination height to render.</param>
        /// <param name="srcX">[Optional] The horizontal pixel position in the texture to start rendering from.</param>
        /// <param name="srcY">[Optional] The vertical pixel position in the texture to start rendering from.</param>
        /// <param name="color">[Optional] The color used to tint the diffuse of the texture.</param>
        /// <param name="clip">[Optional] <b>true</b> to clip the texture if the destination width or height are larger or smaller than the original texture size, or <b>false</b> to scale the texture to meet the new size.</param>
        /// <param name="blendState">[Optional] The blending state to apply when rendering.</param>
        /// <param name="samplerState">[Optional] The sampler state to apply when rendering.</param>
        /// <param name="pixelShader">[Optional] A pixel shader used to override the default pixel shader for the rendering.</param>
        /// <param name="pixelShaderConstants">[Optional] Pixel shader constants to apply when rendering with a custom pixel shader.</param>
        /// <remarks>
        /// <para>
        /// This will render a <see cref="GorgonTexture"/> to the current <see cref="GorgonRenderTargetView"/> in slot 0 of the <see cref="RenderTargets"/> list. This is used a quick means to send graphics 
        /// data to the display without having to set up a <see cref="GorgonDrawCallBase">draw call</see> and submitting it.
        /// </para>
        /// <para>
        /// There are many optional parameters for this method that can alter how the texture is rendered. If there are no extra parameters supplied beyond the <paramref name="texture"/> and position, then 
        /// the texture will render as-is at the desired location. This is sufficient for most use cases. However, this method provides a lot of functionality to allow a user to quickly test out various 
        /// effects when rendering a texture:
        /// <list type="bullet">
        ///     <item>
        ///         Providing a width and height will render the texture at that width and height, or will clip to that width and height depending on the state of the <paramref name="clip"/> parameter.
        ///     </item>
        ///     <item>
        ///         Providing the source offset parameters will make the texture start rendering from that pixel coordinate within the texture itself. Omitting these coordinates will mean 
        ///         that rendering starts at (0, 0).
        ///     </item>
        ///     <item>
        ///         Providing a <paramref name="color"/> will render the texture with a tint of the color specified to the diffuse channels of the texture. If omitted, then the 
        ///         <see cref="GorgonColor.White"/> color will be used.
        ///     </item>
        ///     <item>
        ///         Providing a <paramref name="clip"/> value of <b>true</b> will tell the renderer to clip to the width and height specified. If omitted, then the texture will scale to the width and 
        ///         height provided.
        ///     </item>
        ///     <item>
        ///         Providing a <paramref name="blendState"/> will define how the texture is rendered against the render target and can allow for transparency effects. If omitted, then the 
        ///         <see cref="GorgonBlendState.NoBlending"/> state will be used and the texture will be rendered as opaque.
        ///     </item>
        ///     <item>
        ///         Providing a <paramref name="samplerState"/> will define how to smooth a scaled texture. If omitted, then the <see cref="GorgonSamplerState.Default"/> will be used and the texture 
        ///         will be rendered with bilinear filtering.
        ///     </item>
        ///     <item>
        ///         If a <paramref name="pixelShader"/> is defined, then the texture will be rendered with the specified pixel shader instead of the default one. This will allow for a variety of effects 
        ///         to be applied to the texture while rendering. The companion parameter <paramref name="pixelShaderConstants"/> will also be used to control how the pixel shader renders data based on 
        ///         user input. If the <paramref name="pixelShader"/> parameter is omitted, then a default pixel shader is used and the <paramref name="pixelShaderConstants"/> parameter is ignored.
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonTexture"/>
        /// <seealso cref="GorgonRenderTargetView"/>
        /// <seealso cref="RenderTargets"/>
        public void DrawTexture(GorgonTextureView texture,
                                int x,
                                int y,
                                int width = -1,
                                int height = -1,
                                int srcX = 0,
                                int srcY = 0,
                                GorgonColor? color = null,
                                bool clip = false,
                                GorgonBlendState blendState = null,
                                GorgonSamplerState samplerState = null,
                                GorgonPixelShader pixelShader = null,
                                GorgonConstantBuffers pixelShaderConstants = null)
        {
            DrawTexture(texture,
                        new DX.Rectangle(x, y, width == -1 ? texture.Width : width, height == -1 ? texture.Height : height),
                        new DX.Point(srcX, srcY),
                        color,
                        clip,
                        blendState,
                        samplerState,
                        pixelShader,
                        pixelShaderConstants);
        }

        /// <summary>
        /// Function to render a texture to the current <see cref="GorgonRenderTargetView"/>.
        /// </summary>
        /// <param name="texture">The texture to render.</param>
        /// <param name="destRect">The destination rectangle representing the coordinates to render into.</param>
        /// <param name="sourceOffset">The source horizontal and vertical offset within the source texture to start rendering from.</param>
        /// <param name="color">[Optional] The color used to tint the diffuse of the texture.</param>
        /// <param name="clip">[Optional] <b>true</b> to clip the texture if the destination width or height are larger or smaller than the original texture size, or <b>false</b> to scale the texture to meet the new size.</param>
        /// <param name="blendState">[Optional] The blending state to apply when rendering.</param>
        /// <param name="samplerState">[Optional] The sampler state to apply when rendering.</param>
        /// <param name="pixelShader">[Optional] A pixel shader used to override the default pixel shader for the rendering.</param>
        /// <param name="pixelShaderConstants">[Optional] Pixel shader constants to apply when rendering with a custom pixel shader.</param>
        /// <remarks>
        /// <para>
        /// This will render a <see cref="GorgonTexture"/> to the current <see cref="GorgonRenderTargetView"/> in slot 0 of the <see cref="RenderTargets"/> list. This is used a quick means to send graphics 
        /// data to the display without having to set up a <see cref="GorgonDrawCallBase">draw call</see> and submitting it.
        /// </para>
        /// <para>
        /// There are many optional parameters for this method that can alter how the texture is rendered. If there are no extra parameters supplied beyond the <paramref name="texture"/> and 
        /// <paramref name="destRect"/>, then the texture will render, scaled, to the destination rectangle. This is sufficient for most use cases. However, this method provides a lot of functionality to 
        /// allow a user to quickly test out various effects when rendering a texture:
        /// <list type="bullet">
        ///     <item>
        ///         Providing the <paramref name="sourceOffset"/> parameters will make the texture start rendering from that pixel coordinate within the texture itself. Omitting these coordinates will 
        ///         mean that rendering starts at (0, 0).
        ///     </item>
        ///     <item>
        ///         Providing a <paramref name="color"/> will render the texture with a tint of the color specified to the diffuse channels of the texture. If omitted, then the 
        ///         <see cref="GorgonColor.White"/> color will be used.
        ///     </item>
        ///     <item>
        ///         Providing a <paramref name="clip"/> value of <b>true</b> will tell the renderer to clip to the width and height specified. If omitted, then the texture will scale to the width and 
        ///         height provided.
        ///     </item>
        ///     <item>
        ///         Providing a <paramref name="blendState"/> will define how the texture is rendered against the render target and can allow for transparency effects. If omitted, then the 
        ///         <see cref="GorgonBlendState.NoBlending"/> state will be used and the texture will be rendered as opaque.
        ///     </item>
        ///     <item>
        ///         Providing a <paramref name="samplerState"/> will define how to smooth a scaled texture. If omitted, then the <see cref="GorgonSamplerState.Default"/> will be used and the texture 
        ///         will be rendered with bilinear filtering.
        ///     </item>
        ///     <item>
        ///         If a <paramref name="pixelShader"/> is defined, then the texture will be rendered with the specified pixel shader instead of the default one. This will allow for a variety of effects 
        ///         to be applied to the texture while rendering. The companion parameter <paramref name="pixelShaderConstants"/> will also be used to control how the pixel shader renders data based on 
        ///         user input. If the <paramref name="pixelShader"/> parameter is omitted, then a default pixel shader is used and the <paramref name="pixelShaderConstants"/> parameter is ignored.
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonTexture"/>
        /// <seealso cref="GorgonRenderTargetView"/>
        /// <seealso cref="RenderTargets"/>
        public void DrawTexture(GorgonTextureView texture,
                                DX.Rectangle destRect,
                                DX.Point? sourceOffset = null,
                                GorgonColor? color = null,
                                bool clip = false,
                                GorgonBlendState blendState = null,
                                GorgonSamplerState samplerState = null,
                                GorgonPixelShader pixelShader = null,
                                GorgonConstantBuffers pixelShaderConstants = null)
        {
            TextureBlitter blitter = _textureBlitter.Value;

            blitter.Blit(texture,
                         destRect,
                         sourceOffset ?? DX.Point.Zero,
                         color ?? GorgonColor.White,
                         clip,
                         blendState,
                         samplerState,
                         pixelShader,
                         pixelShaderConstants);
        }

        /// <summary>
        /// Function to retrieve cached states, segregated by group names.
        /// </summary>
        /// <typeparam name="TKey">The type of data used to represent a unique key for a <see cref="GorgonPipelineState"/>.</typeparam>
        /// <param name="groupName">The name of the grouping.</param>
        /// <returns>A <see cref="GorgonPipelineStateGroup{TKey}"/> that will contain the cached <see cref="GorgonPipelineState"/> objects.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="groupName"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="groupName"/> parameter is empty.</exception>
        /// <remarks>
        /// <para>
        /// While the <see cref="GetPipelineState"/> method is much quicker than creating a pipeline state over and over, it still has a fair bit of overhead when calculating which cached 
        /// <see cref="GorgonPipelineState"/> to bring back (or whether to create a new one). 
        /// </para>
        /// <para>
        /// To counter this, applications may use this functionality to create groups of <see cref="GorgonPipelineState"/> objects so they will not need to create them over and over (and thus impair 
        /// performance) or hit the primary <see cref="CachedPipelineStates"/> list via the <see cref="GetPipelineState"/> method.  
        /// </para>
        /// <para>
        /// Note that when the <see cref="ClearStateCache"/> method is called, the cache groupings will be preserved, but any pipeline states contained within it will be cleared and must be recreated by 
        /// the application if they're needed again.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonPipelineStateGroup{TKey}"/>
        /// <seealso cref="GorgonPipelineState"/>
        /// <seealso cref="CachedPipelineStates"/>
        /// <seealso cref="GetPipelineState"/>
        /// <seealso cref="ClearStateCache"/>
        public GorgonPipelineStateGroup<TKey> GetPipelineStateGroup<TKey>(string groupName)
        {
            GorgonPipelineStateGroup<TKey> cacheGroup;

            if (!_groupedCache.TryGetValue(groupName, out IPipelineStateGroup groupObject))
            {
                _groupedCache[groupName] = cacheGroup = new GorgonPipelineStateGroup<TKey>(groupName);
            }
            else
            {
                cacheGroup = (GorgonPipelineStateGroup<TKey>)groupObject;
            }

            return cacheGroup;
        }

        /// <summary>
        /// Function to retrieve a pipeline state.
        /// </summary>
        /// <param name="info">Information used to define the pipeline state.</param>
        /// <returns>A new <see cref="GorgonPipelineState"/>, or an existing one if one was already created.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="info"/> has no <see cref="IGorgonPipelineStateInfo.VertexShader"/>.</exception>
        /// <exception cref="GorgonException">Thrown if a pipeline state requires a higher feature level than supported by the <see cref="VideoDevice"/>.</exception>
        /// <remarks>
        /// <para>
        /// This method will create a new pipeline state, or retrieve an existing state if a cached state already exists that exactly matches the information passed to the <paramref name="info"/> parameter. 
        /// </para>
        /// <para>
        /// When a new pipeline state is created, sub-states like blending, depth/stencil, etc... may be reused from previously cached pipeline states and other uncached sub-states will be created anew. This 
        /// new pipeline state is then cached for reuse later in order to speed up the process of creating a series of states.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonPipelineState"/>
        /// <seealso cref="IGorgonPipelineStateInfo"/>
        /// <seealso cref="IGorgonVideoDevice"/>
        public GorgonPipelineState GetPipelineState(IGorgonPipelineStateInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            (GorgonPipelineState State, bool IsNew) result;

            // Threads have to wait their turn.
            lock (_stateCacheLock)
            {
                result = SetupPipelineState(info);

                if (result.IsNew)
                {
                    _stateCache.Add(result.State);
                }
            }

            return result.State;
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

            _renderTargets.Clear();
            ScissorRectangles.Clear();
            Viewports.Clear();
            _currentDrawCall?.Reset();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            IGorgonVideoDevice device = Interlocked.Exchange(ref _videoDevice, null);
            D3D11.DeviceContext context = Interlocked.Exchange(ref _deviceContext, null);

            if ((device == null)
                || (context == null))
            {
                return;
            }

            // If we ever created a blitter on this interface, then we need to clean up the common data for all blitter instances.
            if (_textureBlitter.IsValueCreated)
            {
                _textureBlitter.Value.Dispose();
            }

            ClearStateCache();

            // Disconnect from the context.
            _log.Print($"Destroying GorgonGraphics interface for device '{device.Info.Name}'...", LoggingLevel.Simple);

            // Reset the state for the context. This will ensure we don't have anything bound to the pipeline when we shut down.
            context.ClearState();
            device.Dispose();
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonGraphics"/> class.
        /// </summary>
        /// <param name="videoDeviceInfo">[Optional] A <see cref="VideoDeviceInfo"/> to specify the video device to use for this instance.</param>
        /// <param name="featureLevel">[Optional] The requested feature level for the video device used with this object.</param>
        /// <param name="log">[Optional] The log to use for debugging.</param>
        /// <exception cref="GorgonException">Thrown when the <paramref name="featureLevel"/> is unsupported.</exception>
        /// <remarks>
        /// <para>
        /// When the <paramref name="videoDeviceInfo"/> is set to <b>null</b>, Gorgon will use the first video device with feature level specified by <paramref name="featureLevel"/>  
        /// will be used. If the feature level requested is higher than what any device in the system can support, then the first device with the highest feature level will be used.
        /// </para>
        /// <para>
        /// When specifying a feature level, the device with the closest matching feature level will be used. If the <paramref name="videoDeviceInfo"/> is specified, then that device will be used at the 
        /// requested <paramref name="featureLevel"/>. If the requested <paramref name="featureLevel"/> is higher than what the <paramref name="videoDeviceInfo"/> will support, then Gorgon will use the 
        /// highest feature of the specified <paramref name="videoDeviceInfo"/>. 
        /// </para>
        /// <para>
        /// If Gorgon is compiled in DEBUG mode, and <see cref="VideoDeviceInfo"/> is <b>null</b>, then it will attempt to find the most appropriate hardware video device, and failing that, will fall 
        /// back to a software device (WARP).
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// The Gorgon Graphics library only works on Windows 7 (with the Platform Update for Direct 3D 11.1) or better. No lesser operating system version is supported.
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
        /// // Create using the first video device with the highest feature level:
        /// var graphics = new GorgonGraphics();
        /// 
        /// // Create using a specific video device and use the highest feature level supported by that device:
        /// // Get a list of available video devices.
        /// IGorgonVideoDeviceList videoDevices = new GorgonVideoDeviceList(log);
        /// videoDevices.Enumerate(true);
        /// var graphics = new GorgonGraphics(videoDevices[0]);
        /// 
        /// // Create using the requested feature level and the first adapter that supports the nearest feature level requested:
        /// // If the device does not support 11.0, then the device with the nearest feature level (e.g. 10.1) will be used instead.
        /// var graphics = new GorgonGraphics(null, FeatureLevel.Level_11_0);
        /// 
        /// // Create using the requested device and the requested feature level:
        /// // If the device does not support 11.0, then the highest feature level supported by the device will be used (e.g. 10.1).
        /// IGorgonVideoDeviceList videoDevices = new GorgonVideoDeviceList(log);
        /// videoDevices.Enumerate(true);
        /// var graphics = new GorgonGraphics(videoDevices[0], FeatureLevel.Level_11_0); 
        /// ]]>
        /// </code>
        /// </example>
        /// <seealso cref="VideoDeviceInfo"/>
        public GorgonGraphics(IGorgonVideoDeviceInfo videoDeviceInfo, FeatureLevelSupport? featureLevel = null, IGorgonLog log = null)
        {
            if (!Win32API.IsWindows10OrGreater(15063))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_INVALID_OS);
            }

            // If we've not specified a feature level, or the feature level exceeds the requested device feature level, then 
            // fall back to the device feature level.
            if ((featureLevel == null) || (videoDeviceInfo.SupportedFeatureLevel < featureLevel.Value))
            {
                featureLevel = videoDeviceInfo.SupportedFeatureLevel;
            }

            // We only support feature level 10 and greater.
            if (!Enum.IsDefined(typeof(FeatureLevelSupport), featureLevel.Value))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FEATURE_LEVEL_INVALID, featureLevel));
            }

            _log = log ?? GorgonLogDummy.DefaultInstance;

            _log.Print("Gorgon Graphics initializing...", LoggingLevel.Simple);
            _log.Print($"Using video device '{videoDeviceInfo.Name}' at feature level [{featureLevel.Value}] for Direct 3D 11.1.", LoggingLevel.Simple);

            _videoDevice = new VideoDevice(videoDeviceInfo, featureLevel.Value, _log);

            ScissorRectangles = new GorgonMonitoredValueTypeArray<DX.Rectangle>(_videoDevice.MaxScissorCount);
            Viewports = new GorgonMonitoredValueTypeArray<DX.ViewportF>(_videoDevice.MaxViewportCount);

            _deviceContext = _videoDevice.D3DDevice.ImmediateContext1;
            _renderTargets = new GorgonRenderTargetViews();

            // Assign common sampler states to the factory cache.
            SamplerStateFactory.GetSamplerState(this, GorgonSamplerState.Default, _log);
            SamplerStateFactory.GetSamplerState(this, GorgonSamplerState.AnisotropicFiltering, _log);
            SamplerStateFactory.GetSamplerState(this, GorgonSamplerState.PointFiltering, _log);

            // Register texture blitter shader code to the shader factory so it can be used to include the blitter.
            GorgonShaderFactory.Includes[BlitterShaderIncludeFileName] = new GorgonShaderInclude(BlitterShaderIncludeFileName, Resources.GraphicsShaders);

            _textureBlitter = new Lazy<TextureBlitter>(() => new TextureBlitter(this), LazyThreadSafetyMode.ExecutionAndPublication);

            _log.Print("Gorgon Graphics initialized.", LoggingLevel.Simple);
        }

        /// <summary>
        /// Initializes the <see cref="GorgonGraphics"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage",
            "CA1806:DoNotIgnoreMethodResults",
            MessageId = "Gorgon.Native.Win32API.DwmIsCompositionEnabled(System.Boolean@)")]
        static GorgonGraphics()
        {
            DX.Configuration.ThrowOnShaderCompileError = false;

#if DEBUG
            IsDebugEnabled = true;
#endif
        }
        #endregion
    }
}
