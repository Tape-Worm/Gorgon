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
    /// <seealso cref="GorgonDrawCallInstanced"/>
    /// <seealso cref="GorgonDrawIndexedInstancedCall"/>
    public sealed class GorgonGraphics
        : IDisposable
    {
        #region Variables.
		// The log interface used to log debug messages.
		private readonly IGorgonLog _log;
		// The video device to use for this graphics object.
		private VideoDevice _videoDevice;
		// The current device context.
		private D3D11.DeviceContext1 _deviceContext;
		// The last used draw call.
		private GorgonDrawCallBase _lastDrawCall;
		// Pipeline state cache.
	    private readonly List<GorgonPipelineState> _stateCache = new List<GorgonPipelineState>();
		// Synchronization lock for creating new pipeline cache entries.
		private readonly object _stateCacheLock = new object();
		// The list of cached scissor rectangles to keep allocates sane.
	    private DX.Rectangle[] _cachedScissors = new DX.Rectangle[1];
        // The currently assigned render target views.
        private D3D11.RenderTargetView[] _nativeRenderTargetViews;
        // The current depth/stencil view.
        private GorgonDepthStencilView _depthStencilView;
        // Flag to indicate that the depth/stencil view has been changed.
        private bool _depthStencilChanged;
        // The list of render targets currently assigned.
        private readonly GorgonRenderTargetViews _renderTargets;
        #endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct 3D 11.1 device context for this graphics instance.
		/// </summary>
		internal D3D11.DeviceContext1 D3DDeviceContext => _deviceContext;

        /// <summary>
        /// Property to return the list of cached pipeline states.
        /// </summary>
        public IReadOnlyList<GorgonPipelineState> CachedPipelineStates => _stateCache;

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
        #endregion

		#region Methods.
	    /// <summary>
	    /// Function to merge the previous draw call viewports to this one.
	    /// </summary>
	    /// <param name="newViewports">The new viewports to assign.</param>
	    /// <param name="currentChanges">The current changes on the pipeline.</param>
	    /// <returns>A <see cref="PipelineResourceChange"/> indicating whether or not the state has changed.</returns>
	    private PipelineResourceChange MergeViewPorts(GorgonMonitoredValueTypeArray<DX.ViewportF> newViewports, PipelineResourceChange currentChanges)
	    {
		    ref (int, int Count, DX.ViewportF[]) current = ref _lastDrawCall.Viewports.GetDirtyItems();
		    ref (int, int Count, DX.ViewportF[] Viewports) newItems = ref newViewports.GetDirtyItems();

		    int maxItems = current.Count.Max(newItems.Count);

		    for (int i = 0; i < maxItems; ++i)
		    {
			    _lastDrawCall.Viewports[i] = newItems.Viewports[i];
		    }

		    if (_lastDrawCall.Viewports.IsDirty)
		    {
			    currentChanges |= PipelineResourceChange.Viewports;
		    }

		    return currentChanges;
	    }

	    /// <summary>
	    /// Function to merge the previous draw call scissor rectangles to this one.
	    /// </summary>
	    /// <param name="newScissorRects">The new scissor rectangles to assign.</param>
	    /// <param name="currentChanges">The current changes on the pipeline.</param>
	    /// <returns>A <see cref="PipelineResourceChange"/> indicating whether or not the state has changed.</returns>
	    private PipelineResourceChange MergeScissorRects(GorgonMonitoredValueTypeArray<DX.Rectangle> newScissorRects, PipelineResourceChange currentChanges)
	    {
		    ref (int, int Count, DX.Rectangle[]) current = ref _lastDrawCall.ScissorRectangles.GetDirtyItems();
		    ref (int, int Count, DX.Rectangle[] Scissors) newItems = ref newScissorRects.GetDirtyItems();

		    int maxItems = current.Count.Max(newItems.Count);

		    for (int i = 0; i < maxItems; ++i)
		    {
			    _lastDrawCall.ScissorRectangles[i] = newItems.Scissors[i];
		    }

		    if (_lastDrawCall.ScissorRectangles.IsDirty)
		    {
			    currentChanges |= PipelineResourceChange.ScissorRectangles;
		    }

		    return currentChanges;
	    }

	    /// <summary>
	    /// Function to merge the previous draw call vertex buffers with new ones.
	    /// </summary>
	    /// <param name="vertexBuffers">The vertex buffers to merge in.</param>
	    /// <param name="currentChanges">The current changes on the pipeline.</param>
	    /// <returns>A <see cref="PipelineResourceChange"/> indicating whether or not the state has changed.</returns>
	    private PipelineResourceChange MergeVertexBuffers(GorgonVertexBufferBindings vertexBuffers, PipelineResourceChange currentChanges)
	    {
		    if (_lastDrawCall.VertexBuffers?.InputLayout != vertexBuffers?.InputLayout)
		    {
			    currentChanges |= PipelineResourceChange.InputLayout;
		    }

		    if (_lastDrawCall.VertexBuffers == vertexBuffers)
		    {
			    if ((_lastDrawCall.VertexBuffers == null)
			        || (!_lastDrawCall.VertexBuffers.IsDirty))
			    {
				    return currentChanges;
			    }

			    return currentChanges | PipelineResourceChange.VertexBuffers;
		    }

		    // If we're tranferring into an uninitialized vertex buffer list, then allocate new vertex buffers and copy.
		    if ((vertexBuffers != null)
		        && (_lastDrawCall.VertexBuffers == null))
		    {
			    _lastDrawCall.VertexBuffers = new GorgonVertexBufferBindings(vertexBuffers.InputLayout, vertexBuffers.Count);
			    vertexBuffers.CopyTo(_lastDrawCall.VertexBuffers);
			    return currentChanges | PipelineResourceChange.VertexBuffers;
		    }

		    // If we're removing a set of vertex buffers, then get rid of our current set as well.
		    if (vertexBuffers == null)
		    {
			    _lastDrawCall.VertexBuffers = null;
			    return currentChanges | PipelineResourceChange.VertexBuffers;
		    }

		    ref (int StartSlot, int Count, GorgonVertexBufferBinding[] Bindings) current = ref _lastDrawCall.VertexBuffers.GetDirtyItems();
		    ref (int StartSlot, int Count, GorgonVertexBufferBinding[] Bindings) newItems = ref vertexBuffers.GetDirtyItems();

		    int maxItems = current.Count.Max(newItems.Count);
		    int start = current.StartSlot.Min(newItems.StartSlot);

		    for (int i = start; i < start + maxItems; ++i)
		    {
			    _lastDrawCall.VertexBuffers[i] = newItems.Bindings[i];
		    }

		    if (_lastDrawCall.VertexBuffers.IsDirty)
		    {
			    currentChanges |= PipelineResourceChange.VertexBuffers;
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
		    ref (int StartSlot, int Count, GorgonConstantBuffer[] Bindings) current = ref buffers.GetDirtyItems();
		    ref (int StartSlot, int Count, GorgonConstantBuffer[] Bindings) newItems = ref buffers.GetDirtyItems();

		    int startSlot = current.StartSlot.Min(newItems.StartSlot);
		    int lastSlot = startSlot + current.Count.Max(newItems.Count);
		    PipelineResourceChange desiredStateBit;
		    GorgonConstantBuffers destBuffers;

		    switch (shaderType)
		    {
			    case ShaderType.Vertex:
				    desiredStateBit = PipelineResourceChange.VertexShaderConstantBuffers;
				    destBuffers = _lastDrawCall.VertexShaderConstantBuffers;
				    break;
			    case ShaderType.Pixel:
				    desiredStateBit = PipelineResourceChange.PixelShaderConstantBuffers;
				    destBuffers = _lastDrawCall.PixelShaderConstantBuffers;
				    break;
			    default:
				    return currentChanges;
		    }

		    return CopyToLastDrawCall(startSlot,
		                              lastSlot,
		                              destBuffers,
		                              newItems.Bindings,
		                              currentChanges,
		                              desiredStateBit);

		    // Local functions are neat.
		    PipelineResourceChange CopyToLastDrawCall(int start, int end, GorgonConstantBuffers lastDrawConstants, GorgonConstantBuffer[] newBuffers, PipelineResourceChange changes, PipelineResourceChange desiredBit)
		    {
			    for (int i = start; i < end; ++i)
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
		    ref (int StartSlot, int Count, GorgonShaderResourceView[] Bindings) current = ref srvs.GetDirtyItems();
		    ref (int StartSlot, int Count, GorgonShaderResourceView[] Bindings) newItems = ref srvs.GetDirtyItems();

		    int startSlot = current.StartSlot.Min(newItems.StartSlot);
		    int lastSlot = startSlot + current.Count.Max(newItems.Count);
		    PipelineResourceChange desiredStateBit;
		    GorgonShaderResourceViews destSrvs;

		    switch (shaderType)
		    {
			    case ShaderType.Vertex:
				    desiredStateBit = PipelineResourceChange.VertexShaderResources;
				    destSrvs = _lastDrawCall.VertexShaderResourceViews;
				    break;
			    case ShaderType.Pixel:
				    desiredStateBit = PipelineResourceChange.PixelShaderResources;
				    destSrvs = _lastDrawCall.PixelShaderResourceViews;
				    break;
			    default:
				    return currentChanges;
		    }

		    return CopyToLastDrawCall(startSlot,
		                              lastSlot,
		                              destSrvs,
		                              newItems.Bindings,
		                              currentChanges,
		                              desiredStateBit);

		    // Local functions are neat.
		    PipelineResourceChange CopyToLastDrawCall(int start, int end, GorgonShaderResourceViews lastDrawSrvs, GorgonShaderResourceView[] newSrvs, PipelineResourceChange changes, PipelineResourceChange desiredBit)
		    {
			    for (int i = start; i < end; ++i)
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
	    /// Function to merge the previous shader samplers with new ones.
	    /// </summary>
	    /// <param name="shaderType">The type of shader to work with.</param>
	    /// <param name="samplers">The shader samplers to merge in.</param>
	    /// <param name="currentChanges">The current changes on the pipeline.</param>
	    /// <returns>A <see cref="PipelineResourceChange"/> indicating whether or not the state has changed.</returns>
	    private PipelineResourceChange MergeShaderSamplers(ShaderType shaderType, GorgonSamplerStates samplers, PipelineResourceChange currentChanges)
	    {
		    ref (int StartSlot, int Count, GorgonSamplerState[] Bindings) current = ref samplers.GetDirtyItems();
		    ref (int StartSlot, int Count, GorgonSamplerState[] Bindings) newItems = ref samplers.GetDirtyItems();

		    int startSlot = current.StartSlot.Min(newItems.StartSlot);
		    int lastSlot = startSlot + current.Count.Max(newItems.Count);
		    PipelineResourceChange desiredStateBit;
		    GorgonSamplerStates destSamplers;

		    switch (shaderType)
		    {
			    case ShaderType.Vertex:
#if DEBUG
				    // If the device doesn't support this, we need to crash out so the dev can fix it.
				    if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
				    {
					    _log.Print($"Error: Assigning a sampler to a vertex shader requires feature level {FeatureLevelSupport.Level_11_0} or better.  The device '{VideoDevice.Info.Name}' only supports feature level {VideoDevice.RequestedFeatureLevel}", LoggingLevel.All);
						throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
				    }
#endif

					desiredStateBit = PipelineResourceChange.VertexShaderResources;
				    destSamplers = _lastDrawCall.VertexShaderSamplers;
				    break;
			    case ShaderType.Pixel:
				    desiredStateBit = PipelineResourceChange.PixelShaderResources;
				    destSamplers = _lastDrawCall.PixelShaderSamplers;
				    break;
			    default:
				    return currentChanges;
		    }

		    return CopyToLastDrawCall(startSlot,
		                              lastSlot,
		                              destSamplers,
		                              newItems.Bindings,
		                              currentChanges,
		                              desiredStateBit);

		    // Local functions are neat.
		    PipelineResourceChange CopyToLastDrawCall(int start, int end, GorgonSamplerStates lastDrawSamplers, GorgonSamplerState[] newSamplers, PipelineResourceChange changes, PipelineResourceChange desiredBit)
		    {
			    for (int i = start; i < end; ++i)
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
		    var pipelineFlags = PipelineStateChange.None;

		    if ((_lastDrawCall.PipelineState == null) && (state == null))
		    {
			    return pipelineFlags;
		    }

		    if (((_lastDrawCall.PipelineState == null) && (state != null))
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
				                     | PipelineStateChange.DomainShader
				                     | PipelineStateChange.ComputeShader;
			    }

			    _lastDrawCall.PipelineState = state;
			    return pipelineFlags;
		    }

			if (_lastDrawCall.PipelineState.Info.PixelShader != state.Info.PixelShader)
			{
			    pipelineFlags |= PipelineStateChange.PixelShader;
		    }

		    if (_lastDrawCall.PipelineState.Info.VertexShader != state.Info.VertexShader)
		    {
			    pipelineFlags |= PipelineStateChange.VertexShader;
		    }

		    if (_lastDrawCall.PipelineState.D3DRasterState != state.D3DRasterState)
		    {
			    pipelineFlags |= PipelineStateChange.RasterState;
		    }

		    if (_lastDrawCall.PipelineState.D3DDepthStencilState != state.D3DDepthStencilState)
		    {
			    pipelineFlags |= PipelineStateChange.DepthStencilState;
		    }

		    if (_lastDrawCall.PipelineState.D3DBlendState != state.D3DBlendState)
		    {
			    pipelineFlags |= PipelineStateChange.BlendState;
		    }

		    if (pipelineFlags != PipelineStateChange.None)
		    {
			    _lastDrawCall.PipelineState = state;
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
		    if (_lastDrawCall == null)
		    {
			    _lastDrawCall = new GorgonDrawCallBase();
		    }

		    PipelineResourceChange stateChanges = PipelineResourceChange.None;
			
		    if (_lastDrawCall.PrimitiveTopology != sourceDrawCall.PrimitiveTopology)
		    {
			    _lastDrawCall.PrimitiveTopology = sourceDrawCall.PrimitiveTopology;
			    stateChanges |= PipelineResourceChange.PrimitiveTopology;
		    }

		    if (_lastDrawCall.IndexBuffer != sourceDrawCall.IndexBuffer)
		    {
			    _lastDrawCall.IndexBuffer = sourceDrawCall.IndexBuffer;
			    stateChanges |= PipelineResourceChange.IndexBuffer;
		    }

		    if (_lastDrawCall.BlendSampleMask != sourceDrawCall.BlendSampleMask)
		    {
			    _lastDrawCall.BlendSampleMask = sourceDrawCall.BlendSampleMask;
				stateChanges |= PipelineResourceChange.BlendSampleMask;
		    }

		    if (_lastDrawCall.DepthStencilReference != sourceDrawCall.DepthStencilReference)
		    {
			    _lastDrawCall.DepthStencilReference = sourceDrawCall.DepthStencilReference;
				stateChanges |= PipelineResourceChange.DepthStencilReference;
		    }

		    if (!_lastDrawCall.BlendFactor.Equals(sourceDrawCall.BlendFactor))
		    {
			    _lastDrawCall.BlendFactor = sourceDrawCall.BlendFactor;
			    stateChanges |= PipelineResourceChange.BlendFactor;
			}

			stateChanges |= MergeViewPorts(sourceDrawCall.Viewports, stateChanges);
		    stateChanges |= MergeScissorRects(sourceDrawCall.ScissorRectangles, stateChanges);
		    stateChanges |= MergeVertexBuffers(sourceDrawCall.VertexBuffers, stateChanges);
		    stateChanges |= MergeConstantBuffers(ShaderType.Vertex, sourceDrawCall.VertexShaderConstantBuffers, stateChanges);
		    stateChanges |= MergeConstantBuffers(ShaderType.Pixel, sourceDrawCall.PixelShaderConstantBuffers, stateChanges);
		    stateChanges |= MergeShaderResources(ShaderType.Vertex, sourceDrawCall.VertexShaderResourceViews, stateChanges);
		    stateChanges |= MergeShaderResources(ShaderType.Pixel, sourceDrawCall.PixelShaderResourceViews, stateChanges);
		    stateChanges |= MergeShaderSamplers(ShaderType.Vertex, sourceDrawCall.VertexShaderSamplers, stateChanges);
		    stateChanges |= MergeShaderSamplers(ShaderType.Pixel, sourceDrawCall.PixelShaderSamplers, stateChanges);

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
		private GorgonPipelineState InitializePipelineState(IGorgonPipelineStateInfo info, D3D11.BlendState1 blendState, D3D11.DepthStencilState depthStencilState, D3D11.RasterizerState1 rasterState)
		{
			D3D11.Device1 videoDevice = VideoDevice.D3DDevice();
			var result = new GorgonPipelineState(info, _stateCache.Count)
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

			if ((blendState != null) || (info.RenderTargetBlendState == null) || (info.RenderTargetBlendState.Count == 0))
			{
				return result;
			}

			int maxStates = info.RenderTargetBlendState.Count.Min(D3D11.OutputMergerStage.SimultaneousRenderTargetCount);

			var desc = new D3D11.BlendStateDescription1
			{
				IndependentBlendEnable = info.IsIndependentBlendingEnabled,
				AlphaToCoverageEnable = info.IsAlphaToCoverageEnabled
			};

			for (int i = 0; i < maxStates; ++i)
			{
				desc.RenderTarget[i] = info.RenderTargetBlendState[i].ToRenderTargetBlendStateDesc1();
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

			    if (ReferenceEquals(newStateInfo.RenderTargetBlendState, cachedStateInfo.RenderTargetBlendState))
			    {
				    blendState = cachedState.D3DBlendState;
				    inheritedState |= PipelineStateChange.BlendState;
			    }
			    else
			    {
			        if ((newStateInfo.RenderTargetBlendState != null)
			            && (cachedStateInfo.RenderTargetBlendState != null)
			            && (newStateInfo.RenderTargetBlendState.Count == cachedStateInfo.RenderTargetBlendState.Count))
			        {
			            for (int j = 0; j < newStateInfo.RenderTargetBlendState.Count; ++j)
			            {
			                if (cachedStateInfo.RenderTargetBlendState[j].Equals(newStateInfo.RenderTargetBlendState[j]))
			                {
			                    blendStateEqualCount++;
			                }
			            }

			            if (blendStateEqualCount == newStateInfo.RenderTargetBlendState.Count)
			            {
			                blendState = cachedState.D3DBlendState;
			                inheritedState |= PipelineStateChange.BlendState;
			            }
			        }
			    }

			    // We've copied all the states, so just return the existing pipeline state.
		        if (inheritedState == (PipelineStateChange.VertexShader
		                               | PipelineStateChange.PixelShader
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
				buffer = indexBuffer.D3DBuffer;
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
				D3DDeviceContext.VertexShader.Set(state?.Info.VertexShader?.D3DShader);
			}

			if ((changes & PipelineStateChange.PixelShader) == PipelineStateChange.PixelShader)
			{
				D3DDeviceContext.PixelShader.Set(state?.Info.PixelShader?.D3DShader);
			}
		}

		/// <summary>
		/// Function to assign viewports.
		/// </summary>
		/// <param name="viewPorts">The list of viewports to assign.</param>
	    private unsafe void SetViewports(GorgonMonitoredValueTypeArray<DX.ViewportF> viewPorts)
	    {
		    ref (int Start, int Count, DX.ViewportF[] Viewports) viewports = ref viewPorts.GetDirtyItems();
		    RawViewportF* rawViewports = stackalloc RawViewportF[viewports.Count];

		    for (int i = 0; i < viewports.Count; ++i)
		    {
			    rawViewports[i] = viewports.Viewports[i];
		    }

		    D3DDeviceContext.Rasterizer.SetViewports(rawViewports, viewports.Count);
	    }

		/// <summary>
		/// Function to assign scissor rectangles.
		/// </summary>
		/// <param name="scissorRects">The scissor rectangles to assign.</param>
	    private void SetScissorRects(GorgonMonitoredValueTypeArray<DX.Rectangle> scissorRects)
	    {
		    ref (int Start, int Count, DX.Rectangle[] Scissors) scissors = ref scissorRects.GetDirtyItems();

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
			    _cachedScissors[i] = scissors.Scissors[i];
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

		    ref (int StartSlot, int Count, GorgonVertexBufferBinding[] Bindings) bindings = ref vertexBuffers.GetDirtyItems();
		    D3DDeviceContext.InputAssembler.SetVertexBuffers(bindings.StartSlot, vertexBuffers.Native);
		}


        /// <summary>
        /// Function to unbind a render target that is bound as a shader input.
        /// </summary>
        private void UnbindShaderInputs()
        {
            if (_lastDrawCall == null)
            {
                return;
            }

            // This can happen quite easily due to how we're handling draw calls (i.e. stateless).  So we won't log anything here and we'll just unbind for the time being.
            // This may have a small performance penalty.
            ref (int Start, int Count, GorgonRenderTargetView[] Bindings) rtBindings = ref _renderTargets.GetDirtyItems();

            for (int i = 0; i < rtBindings.Start + rtBindings.Count; ++i)
            {
                GorgonRenderTargetView view = rtBindings.Bindings[i];

                if ((view == null) || (view.Texture.Info.Binding & TextureBinding.ShaderResource) != TextureBinding.ShaderResource)
                {
                    continue;
                }

                UnbindFromShader(view.Texture, ref _lastDrawCall.VertexShaderResourceViews.GetDirtyItems());
                UnbindFromShader(view.Texture, ref _lastDrawCall.PixelShaderResourceViews.GetDirtyItems());
            }

            void UnbindFromShader(GorgonTexture renderTarget, ref (int Start, int Count, GorgonShaderResourceView[] Bindings) bindings)
            {
                for (int i = bindings.Start; i < bindings.Start + bindings.Count; ++i)
                {
                    GorgonShaderResourceView srv = bindings.Bindings[i];
                    
                    if ((srv == null) || (renderTarget != srv.Resource))
                    {
                        continue;
                    }
                    
                    _lastDrawCall.PixelShaderResourceViews[i] = null;
                    SetShaderResourceViews(ShaderType.Pixel, _lastDrawCall.PixelShaderResourceViews);
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

#if DEBUG
	        GorgonRenderTargetViews.ValidateDepthStencilView(DepthStencilView, RenderTargets.FirstOrDefault(item => item != null));
#endif

            UnbindShaderInputs();

            ref (int StartSlot, int Count, GorgonRenderTargetView[] Bindings) bindings = ref _renderTargets.GetDirtyItems();

	        _nativeRenderTargetViews = _renderTargets.Native;
			D3DDeviceContext.OutputMerger.SetTargets(DepthStencilView?.Native, bindings.Count, _nativeRenderTargetViews);
	    }
		
	    /// <summary>
	    /// Function to assign the constant buffers to the resource list.
	    /// </summary>
	    /// <param name="shaderType">The type of shader to set the resources on.</param>
	    /// <param name="buffers">The constant buffers to assign.</param>
	    private void SetShaderConstantBuffers(ShaderType shaderType, GorgonConstantBuffers buffers)
	    {
		    ref (int StartSlot, int Count, GorgonConstantBuffer[] Bindings) bindings = ref buffers.GetDirtyItems();
			
		    switch (shaderType)
		    {
			    case ShaderType.Pixel:
				    D3DDeviceContext.PixelShader.SetConstantBuffers(bindings.StartSlot, bindings.Count, buffers.Native);
				    break;
			    case ShaderType.Vertex:
				    D3DDeviceContext.VertexShader.SetConstantBuffers(bindings.StartSlot, bindings.Count, buffers.Native);
				    break;
		    }
	    }

		/// <summary>
		/// Function to assign the shader resource views to the resource list.
		/// </summary>
		/// <param name="shaderType">The type of shader to set the resources on.</param>
		/// <param name="srvs">The shader resource views to assign.</param>
		private void SetShaderResourceViews(ShaderType shaderType, GorgonShaderResourceViews srvs)
		{
		    bool invalidSlotsDetected = false;
		    ref (int StartSlot, int Count, GorgonShaderResourceView[] Bindings) bindings = ref srvs.GetDirtyItems();
	        ref (int StartSlot, int Count, GorgonRenderTargetView[] Bindings) rtBindings = ref _renderTargets.GetDirtyItems();

	        for (int i = bindings.StartSlot; i < bindings.StartSlot + bindings.Count; ++i)
	        {
	            GorgonShaderResourceView srv = bindings.Bindings[i];

	            if (srv == null)
	            {
	                continue;
	            }

	            for (int j = rtBindings.StartSlot; j < rtBindings.StartSlot + rtBindings.Count; ++j)
	            {
	                GorgonRenderTargetView rtv = rtBindings.Bindings[j];

                    // If we're trying to bind a shader resource while it's held as a render target, then reset the render target to null.
	                if ((rtv == null) || (rtv.Texture != srv.Resource))
	                {
	                    continue;
	                }

	                _log.Print($"[ERROR] {srv.Resource.Name} is already bound as a render target in slot {j}. Please unbind the render target before assigning it to a {shaderType}. This resource will not be assigned.", LoggingLevel.Verbose);

                    // Do not allow this item to be bound as a shader resource.
	                srvs[i] = null;
	                invalidSlotsDetected = true;
	            }
	        }
            
		    if (invalidSlotsDetected)
		    {
		        // Update the resource bindings again.
		        bindings = srvs.GetDirtyItems();
		    }

		    switch (shaderType)
		    {
			    case ShaderType.Pixel:
				    D3DDeviceContext.PixelShader.SetShaderResources(bindings.StartSlot, bindings.Count, srvs.Native);
				    break;
			    case ShaderType.Vertex:
				    D3DDeviceContext.VertexShader.SetShaderResources(bindings.StartSlot, bindings.Count, srvs.Native);
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
		    ref (int StartSlot, int Count, GorgonSamplerState[] Bindings) bindings = ref samplers.GetDirtyItems();

		    switch (shaderType)
		    {
			    case ShaderType.Pixel:
				    D3DDeviceContext.PixelShader.SetSamplers(bindings.StartSlot, bindings.Count, samplers.Native);
				    break;
			    case ShaderType.Vertex:
				    D3DDeviceContext.VertexShader.SetSamplers(bindings.StartSlot, bindings.Count, samplers.Native);
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
		private void ApplyPerDrawStates(GorgonDrawCallBase drawCall, GorgonPipelineState newState, PipelineResourceChange resourceChanges, PipelineStateChange stateChanges)
		{
            if ((resourceChanges & PipelineResourceChange.PrimitiveTopology) == PipelineResourceChange.PrimitiveTopology)
			{
				D3DDeviceContext.InputAssembler.PrimitiveTopology = drawCall.PrimitiveTopology;
			}

			if ((resourceChanges & PipelineResourceChange.Viewports) == PipelineResourceChange.Viewports)
			{
				SetViewports(drawCall.Viewports);
			}

			if ((resourceChanges & PipelineResourceChange.ScissorRectangles) == PipelineResourceChange.ScissorRectangles)
			{
				SetScissorRects(drawCall.ScissorRectangles);
			}

			if ((resourceChanges & PipelineResourceChange.InputLayout) == PipelineResourceChange.InputLayout)
			{
				D3DDeviceContext.InputAssembler.InputLayout = drawCall.VertexBuffers?.InputLayout.D3DInputLayout;
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

			if ((resourceChanges & PipelineResourceChange.VertexShaderResources) == PipelineResourceChange.VertexShaderResources)
			{
				SetShaderResourceViews(ShaderType.Vertex, drawCall.VertexShaderResourceViews);
			}

			if ((resourceChanges & PipelineResourceChange.PixelShaderResources) == PipelineResourceChange.PixelShaderResources)
			{
				SetShaderResourceViews(ShaderType.Pixel, drawCall.PixelShaderResourceViews);
			}

			if ((resourceChanges & PipelineResourceChange.VertexShaderResources) == PipelineResourceChange.VertexShaderResources)
			{
				SetShaderSamplers(ShaderType.Vertex, drawCall.VertexShaderSamplers);
			}

			if ((resourceChanges & PipelineResourceChange.PixelShaderResources) == PipelineResourceChange.PixelShaderResources)
			{
				SetShaderSamplers(ShaderType.Pixel, drawCall.PixelShaderSamplers);
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
        /// Function to assign a single render target to the first slot.
        /// </summary>
        /// <param name="renderTarget">The render target view to assign.</param>
        /// <remarks>
        /// <para>
        /// This will assign a render target in slot 0 of the <see cref="RenderTargets"/> list. If the <paramref name="renderTarget"/> has an associated depth/stencil buffer, then that will be assigned to the 
        /// <see cref="DepthStencilView"/>. If it does not, the <see cref="DepthStencilView"/> will be set to <b>null</b>.
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
        public void SetRenderTarget(GorgonRenderTargetView renderTarget, GorgonDepthStencilView depthStencil)
        {
            _renderTargets.Clear();

            if ((_renderTargets[0] == renderTarget) && (depthStencil == DepthStencilView))
            {
                return;
            }

            _renderTargets[0] = renderTarget;

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
        public void SetRenderTargets(GorgonRenderTargetView[] renderTargets, GorgonDepthStencilView depthStencil = null)
        {
            _renderTargets.Clear();

            if (renderTargets == null)
            {
                DepthStencilView = depthStencil;
                SetRenderTargetAndDepthViews();
                return;
            }

            for (int i = 0; i < renderTargets.Length.Min(_renderTargets.Count); ++i)
            {
                _renderTargets[i] = renderTargets[i];
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
				// Wipe out the state cache.
				// ReSharper disable once ForCanBeConvertedToForeach
				for (int i = 0; i < _stateCache.Count; ++i)
				{
					_stateCache[i].D3DRasterState?.Dispose();
					_stateCache[i].D3DDepthStencilState?.Dispose();
					_stateCache[i].D3DBlendState?.Dispose();
				}

				_stateCache.Clear();
			}
		}

        /// <summary>
        /// Function to submit a <see cref="GorgonDrawIndexedCall"/> to the GPU.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This method sends a series of state changes and resource bindings to the GPU along with a command to render primitive data.
        /// </para>
        /// <para>
        /// For performance, Gorgon keeps track of the previous draw call, and if they're the same reference, then nothing is done. A new <see cref="GorgonDrawIndexedCall"/> must be sent to this method in 
        /// order for changes to be seen.
        /// </para>
        /// <para>
        /// <note type="caution">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void Submit(GorgonDrawIndexedCall drawCall)
		{
			drawCall.ValidateObject(nameof(drawCall));

			// Merge this draw call with our previous one (if available).
			(PipelineResourceChange ChangedResources, PipelineStateChange ChangedStates) stateChange = MergeDrawCall(drawCall);

			ApplyPerDrawStates(_lastDrawCall, drawCall.PipelineState, stateChange.ChangedResources, stateChange.ChangedStates);

			D3DDeviceContext.DrawIndexed(drawCall.IndexCount, drawCall.IndexStart, drawCall.BaseVertexIndex);
		}

		/// <summary>
		/// Function to submit a <see cref="GorgonDrawCall"/> to the GPU.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/> parameter is <b>null</b>.</exception>
		/// <remarks>
		/// <para>
		/// This method sends a series of state changes and resource bindings to the GPU along with a command to render primitive data.
		/// </para>
		/// <para>
		/// For performance, Gorgon keeps track of the previous draw call, and if they're the same reference, then nothing is done. A new <see cref="GorgonDrawCall"/> must be sent to this method in 
		/// order for changes to be seen.
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

			ApplyPerDrawStates(_lastDrawCall, drawCall.PipelineState, stateChange.ChangedResources, stateChange.ChangedStates);

			D3DDeviceContext.Draw(drawCall.VertexCount, drawCall.VertexStartIndex);
		}

		/// <summary>
		/// Function to submit a <see cref="GorgonDrawCallInstanced"/> to the GPU.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/> parameter is <b>null</b>.</exception>
		/// <remarks>
		/// <para>
		/// This method sends a series of state changes and resource bindings to the GPU along with a command to render primitive data.
		/// </para>
		/// <para>
		/// For performance, Gorgon keeps track of the previous draw call, and if they're the same reference, then nothing is done. A new <see cref="GorgonDrawCall"/> must be sent to this method in 
		/// order for changes to be seen.
		/// </para>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void Submit(GorgonDrawCallInstanced drawCall)
		{
			drawCall.ValidateObject(nameof(drawCall));

			// Merge this draw call with our previous one (if available).
			(PipelineResourceChange ChangedResources, PipelineStateChange ChangedStates) stateChange = MergeDrawCall(drawCall);

			ApplyPerDrawStates(_lastDrawCall, drawCall.PipelineState, stateChange.ChangedResources, stateChange.ChangedStates);

			D3DDeviceContext.DrawInstanced(drawCall.VertexCountPerInstance, drawCall.InstanceCount, drawCall.VertexStartIndex, drawCall.StartInstanceIndex);
		}

		/// <summary>
		/// Function to submit a <see cref="GorgonDrawIndexedInstancedCall"/> to the GPU.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="drawCall"/> parameter is <b>null</b>.</exception>
		/// <remarks>
		/// <para>
		/// This method sends a series of state changes and resource bindings to the GPU along with a command to render primitive data.
		/// </para>
		/// <para>
		/// For performance, Gorgon keeps track of the previous draw call, and if they're the same reference, then nothing is done. A new <see cref="GorgonDrawCall"/> must be sent to this method in 
		/// order for changes to be seen.
		/// </para>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void Submit(GorgonDrawIndexedInstancedCall drawCall)
		{
			drawCall.ValidateObject(nameof(drawCall));

			// Merge this draw call with our previous one (if available).
			(PipelineResourceChange ChangedResources, PipelineStateChange ChangedStates) stateChange = MergeDrawCall(drawCall);

			ApplyPerDrawStates(_lastDrawCall, drawCall.PipelineState, stateChange.ChangedResources, stateChange.ChangedStates);

			D3DDeviceContext.DrawIndexedInstanced(drawCall.IndexCountPerInstance, drawCall.InstanceCount, drawCall.IndexStart, drawCall.BaseVertexIndex, drawCall.StartInstanceIndex);
		}

		/// <summary>
		/// Function to retrieve a pipeline state.
		/// </summary>
		/// <param name="info">Information used to define the pipeline state.</param>
		/// <returns>A new <see cref="GorgonPipelineState"/>, or an existing one if one was already created.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="info"/> has no <see cref="IGorgonPipelineStateInfo.VertexShader"/>.</exception>
		/// <remarks>
		/// <para>
		/// This method will create a new pipeline state, or retrieve an existing state if a cached state already exists that exactly matches the information passed to the <paramref name="info"/> parameter. 
		/// </para>
		/// <para>
		/// When a new pipeline state is created, sub-states like blending, depth/stencil, etc... may be reused from previously cached pipeline states and other uncached sub-states will be created anew. This 
		/// new pipeline state is then cached for reuse later in order to speed up the process of creating a series of states.
		/// </para>
		/// </remarks>
		public GorgonPipelineState GetPipelineState(IGorgonPipelineStateInfo info)
	    {
		    if (info == null)
		    {
			    throw new ArgumentNullException(nameof(info));
		    }

		    (GorgonPipelineState State, bool IsNew) result;

			// Threads have to wait their turn.
			lock(_stateCacheLock)
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
	        _lastDrawCall.Reset();
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
            _nativeRenderTargetViews = new D3D11.RenderTargetView[_videoDevice.MaximumRenderTargetCount];
			_deviceContext = _videoDevice.D3DDevice.ImmediateContext1;
            _renderTargets = new GorgonRenderTargetViews();
			
			_log.Print("Gorgon Graphics initialized.", LoggingLevel.Simple);
		}

		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Gorgon.Native.Win32API.DwmIsCompositionEnabled(System.Boolean@)")]
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
