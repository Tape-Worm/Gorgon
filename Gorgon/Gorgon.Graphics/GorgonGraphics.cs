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
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.Native;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using GI = SharpDX.DXGI;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Operators used for comparison operations.
	/// </summary>
	public enum ComparisonOperator
	{
		/// <summary>
		/// Unknown.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Never pass the comparison.
		/// </summary>
		Never = 1,
		/// <summary>
		/// If the source data is less than the destination data, the comparison passes.
		/// </summary>
		Less = 2,
		/// <summary>
		/// If the source data is equal to the destination data, the comparison passes.
		/// </summary>
		Equal = 3,
		/// <summary>
		/// If the source data is less than or equal to the destination data, the comparison passes.
		/// </summary>
		LessEqual = 4,
		/// <summary>
		/// If the source data is greater than the destination data, the comparison passes.
		/// </summary>
		Greater = 5,
		/// <summary>
		/// If the source data is not equal to the destination data, the comparison passes.
		/// </summary>
		NotEqual = 6,
		/// <summary>
		/// If the source data is greater than or equal to the destination data, the comparison passes.
		/// </summary>
		GreaterEqual = 7,
		/// <summary>
		/// Always pass the comparison.
		/// </summary>
		Always = 8
	}

	/// <summary>
	/// The primary object for the graphics sub system.
	/// </summary>
	/// <remarks>This interface is used to create all objects (buffers, shaders, etc...) that are to be used for graphics.  An interface is tied to a single physical video device, to use 
	/// multiple video devices, create additional graphics interfaces and assign the device to the <see cref="Gorgon.Graphics.GorgonGraphics.VideoDevice">VideoDevice</see> property.
	/// <para>The constructor for this object can take a value known as a device feature level to specify the base line video device capabilities to use.  This feature level value specifies 
	/// what capabilities we have available. To have Gorgon use the best available feature level for your video device, you may call the GorgonGraphics constructor 
	/// without any parameters and it will use the best available feature level for your device.</para>
	/// <para>Along with the feature level, the graphics object can also take a <see cref="Gorgon.Graphics.VideoDevice">Video Device</see> object as a parameter.  Specifying a 
	/// video device will force Gorgon to use that video device for rendering. If a video device is not specified, then the first detected video device will be used.</para>
	/// <para>Please note that graphics objects cannot be shared between devices and must be duplicated.</para>
	/// <para>Objects created by this interface will be automatically tracked and disposed when this interface is disposed.  This is meant to help handle memory leak problems.  However, 
	/// it is important to note that this is not a good practice and the developer is responsible for calling Dispose on all objects that they create, graphics or otherwise.</para>
	/// <para>This object will enumerate video devices, monitor outputs (for multi-head adapters), and video modes for each of the video devices in the system upon creation.  These
    /// items are accessible from the <see cref="Gorgon.Graphics.GorgonVideoDeviceList">GorgonVideoDeviceEnumerator</see> class.</para>
    /// <para>These objects can also be used in a deferred context.  This means that when a graphics object is deferred, it can be used in a multi threaded environment to allow set up of 
    /// a scene by recording commands sent to the video device for execution later on the rendering process.  This is handy where multiple passes for the same scene are required (e.g. a deferred renderer).</para>
	/// <para>Please note that this object requires Direct3D 11 (but not necessarily a Direct3D 11 video card) and at least Windows Vista Service Pack 2 or higher.  
	/// Windows XP and operating systems before it will not work, and an exception will be thrown if this object is created on those platforms.</para>
    /// <para>Deferred graphics contexts require a video device with a feature level of SM5 or better.</para>
	/// </remarks>
    public sealed class GorgonGraphics
        : IDisposable
    {
		#region Variables.
		// Flag to indicate that the desktop window manager compositor is enabled.
		private static bool _isDWMEnabled;                                                  
		// Flag to indicate that we should not enable the DWM.
		private static readonly bool _dontEnableDWM;                                        
		// The log interface used to log debug messages.
		private readonly IGorgonLog _log;
		// The video device to use for this graphics object.
		private VideoDevice _videoDevice;
		// Previously changed states.
		private PipelineStateChangeFlags _previousStates;
		// The current pipeline state.
		private readonly GorgonPipelineState _currentState;
		// The default pipeline state.
		private readonly GorgonPipelineState _defaultState = new GorgonPipelineState();
		// The current device context.
		private D3D11.DeviceContext1 _deviceContext;
		#endregion

        #region Properties.

		/// <summary>
		/// Property to return the Direct 3D 11.1 device context for this graphics instance.
		/// </summary>
		internal D3D11.DeviceContext1 D3DDeviceContext => _deviceContext;

		/// <summary>
		/// Property to set or return the current render target views.
		/// </summary>
		// TODO: This needs to be put into renderers.
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public GorgonRenderTargetViews RenderTargetViews
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether DWM composition is enabled or not.
		/// </summary>
		/// <remarks>This property will have no effect on systems that initially have the desktop window manager compositor disabled.</remarks>
		public static bool IsDWMCompositionEnabled
        {
            get
            {
                return _isDWMEnabled;
            }
            set
            {
                if (!value)
                {
	                if (!_isDWMEnabled)
	                {
		                return;
	                }

	                if (Win32API.DwmEnableComposition(0) != 0)
	                {
		                return;
	                }

	                _isDWMEnabled = false;
                }
                else
                {
	                if ((_isDWMEnabled) || (_dontEnableDWM))
	                {
		                return;
	                }

	                if (Win32API.DwmEnableComposition(1) != 0)
	                {
		                return;
	                }

	                _isDWMEnabled = true;
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
            get
            {
                return DX.Configuration.EnableObjectTracking;
            }
            set
            {
                DX.Configuration.EnableObjectTracking = value;
            }
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
		#endregion

        #region Methods.
		/// <summary>
		/// Function to bind the vertex shader state to the pipeline.
		/// </summary>
		/// <param name="shaderState">The state to bind to the pipeline.</param>
		/// <param name="stateChange">The states to change.</param>
		private void BindVertexShaderState(GorgonVertexShaderState shaderState, ShaderStateChanges stateChange)
		{
			if (shaderState == null)
			{
				D3DDeviceContext.VertexShader.Set(null);

				for (int i = 0; i < D3D11.CommonShaderStage.ConstantBufferApiSlotCount; ++i)
				{
					D3DDeviceContext.VertexShader.SetConstantBuffer(i, null);
				}
				
				return;
			}

			if ((stateChange & ShaderStateChanges.Shader) == ShaderStateChanges.Shader)
			{
				D3DDeviceContext.VertexShader.Set(shaderState.Shader.D3DShader);
			}

			if ((stateChange & ShaderStateChanges.Constants) == ShaderStateChanges.Constants)
			{
				D3DDeviceContext.VertexShader.SetConstantBuffers(0, shaderState.ConstantBuffers.D3DConstantBufferBindCount, shaderState.ConstantBuffers.D3DConstantBuffers);
			}

			if ((stateChange & ShaderStateChanges.ShaderResourceViews) == ShaderStateChanges.ShaderResourceViews)
			{
				D3DDeviceContext.VertexShader.SetShaderResources(0, shaderState.ResourceViews.D3DShaderResourceViewBindCount, shaderState.ResourceViews.D3DShaderResourceViews);
			}
		}

		/// <summary>
		/// Function to bind the pixel shader state to the pipeline.
		/// </summary>
		/// <param name="shaderState">The state to bind to the pipeline.</param>
		/// <param name="stateChange">The states to change.</param>
		private void BindPixelShaderState(GorgonPixelShaderState shaderState, ShaderStateChanges stateChange)
		{
			if (shaderState == null)
			{
				D3DDeviceContext.PixelShader.Set(null);
				return;
			}

			if ((stateChange & ShaderStateChanges.Shader) == ShaderStateChanges.Shader)
			{
				D3DDeviceContext.PixelShader.Set(shaderState.Shader.D3DShader);
			}

			if ((stateChange & ShaderStateChanges.Constants) == ShaderStateChanges.Constants)
			{
				D3DDeviceContext.PixelShader.SetConstantBuffers(0, shaderState.ConstantBuffers.D3DConstantBufferBindCount, shaderState.ConstantBuffers.D3DConstantBuffers);
			}

			if ((stateChange & ShaderStateChanges.ShaderResourceViews) == ShaderStateChanges.ShaderResourceViews)
			{
				D3DDeviceContext.PixelShader.SetShaderResources(0, shaderState.ResourceViews.D3DShaderResourceViewBindCount, shaderState.ResourceViews.D3DShaderResourceViews);
			}
		}

		/// <summary>
		/// Function to bind the render target views and depth/stencil view to the pipeline.
		/// </summary>
		/// <param name="views">The render target views and depth/stencil view to apply.</param>
		private void BindRtvs(GorgonRenderTargetViews views)
		{
			if (_currentState.RenderTargetViews == views)
			{
				return;
			}

			// Disable the render targets if we have none.
			if (views == null)
			{
				D3DDeviceContext.OutputMerger.SetTargets();
				_currentState.RenderTargetViews.Clear();
				return;
			}

			// Only set the depth/stencil.
			if (views.D3DRenderTargetViewBindCount == 0)
			{
				D3DDeviceContext.OutputMerger.SetTargets(views.DepthStencilView?.D3DView);
				_currentState.RenderTargetViews = views;
				return;
			}

			D3DDeviceContext.OutputMerger.SetTargets(views.DepthStencilView?.D3DView, views.D3DRenderTargetViewBindCount, views.D3DRenderTargetViews);
			_currentState.RenderTargetViews = views;
		}

		/// <summary>
		/// Function to retrieve the current set of changed states from the supplied state.
		/// </summary>
		/// <param name="state">The state used to determine what's been changed since the last state.</param>
		/// <param name="vertexShaderChanges">Any vertex shader changes required.</param>
		/// <param name="pixelShaderChanges">Any pixel shader changes required.</param>
		/// <returns>A <see cref="PipelineStateChangeFlags"/> type indicating which pipeline states have been changed.</returns>
		private PipelineStateChangeFlags GetPipelineStateChanges(GorgonPipelineState state, out ShaderStateChanges vertexShaderChanges, out ShaderStateChanges pixelShaderChanges)
		{
			var result = PipelineStateChangeFlags.None;

			if (_currentState == null)
			{
				pixelShaderChanges = vertexShaderChanges = ShaderStateChanges.Constants | ShaderStateChanges.Shader;
				return PipelineStateChangeFlags.InputLayout | PipelineStateChangeFlags.PixelShader | PipelineStateChangeFlags.VertexShader
				       | PipelineStateChangeFlags.RenderTargetViews | PipelineStateChangeFlags.Viewport;
			}

			if (state.InputLayout != _currentState.InputLayout)
			{
				result |= PipelineStateChangeFlags.InputLayout;
			}

			if (!GorgonRenderTargetViews.Equals(state.RenderTargetViews,  _currentState.RenderTargetViews))
			{
				result |= PipelineStateChangeFlags.RenderTargetViews;
			}

			if (!GorgonViewports.Equals(state.Viewports, _currentState.Viewports))
			{
				result |= PipelineStateChangeFlags.Viewport;
			}

			if (!GorgonVertexBufferBindings.Equals(state.VertexBuffers, _currentState.VertexBuffers))
			{
				result |= PipelineStateChangeFlags.VertexBuffers;
			}

			if (state.IndexBuffer != _currentState.IndexBuffer)
			{
				result |= PipelineStateChangeFlags.IndexBuffer;
			}
			
			if ((state.PixelShader == null) && (_currentState.PixelShader != null))
			{
				pixelShaderChanges = ShaderStateChanges.Shader | ShaderStateChanges.Constants;
			}
			else if (state.PixelShader != null)
			{
				pixelShaderChanges = state.PixelShader.GetChanges(_currentState.PixelShader);
			}
			else
			{
				pixelShaderChanges = ShaderStateChanges.None;
			}

			if (pixelShaderChanges != ShaderStateChanges.None)
			{
				result |= PipelineStateChangeFlags.PixelShader;
			}

			if ((state.VertexShader == null) && (_currentState.VertexShader != null))
			{
				vertexShaderChanges = ShaderStateChanges.Shader | ShaderStateChanges.Constants;
			}
			else if (state.VertexShader != null)
			{
				vertexShaderChanges = state.VertexShader.GetChanges(_currentState.VertexShader);
			}
			else
			{
				vertexShaderChanges = ShaderStateChanges.None;
			}

			if (vertexShaderChanges != ShaderStateChanges.None)
			{
				result |= PipelineStateChangeFlags.VertexShader;
			}

			return result;
		}

		/// <summary>
		/// Function to apply a pipeline state to the pipeline.
		/// </summary>
		/// <param name="state">A <see cref="GorgonPipelineState"/> to apply to the pipeline.</param>
		/// <remarks>
		/// <para>
		/// This is responsible for setting all the states for a pipeline at once. This has the advantage of ensuring that duplicate states do not get set so that performance is not impacted, but most importantly, 
		/// it also responsible for ensuring the state leakage does not occur. 
		/// </para>
		/// <para>
		/// An application may experience state leakage in the following scenario:
		/// <para>
		/// <list type="number">
		///		<item>
		///			<description>The initial scene state is set. For example, alpha blending is enabled, a pixel shader view for a texture is set.</description>
		///		</item>
		///		<item>
		///			<description>Objects that need alpha blending are rendered.</description>
		///		</item>
		///		<item>
		///			<description>A new pixel shader view for the texture is set.</description>
		///		</item>
		/// </list>
		/// </para>
		/// </para>
		/// </remarks>
		private void ApplyPipelineState(GorgonPipelineState state)
		{
			state.ValidateObject(nameof(state));

			ShaderStateChanges pixelShaderState;
			ShaderStateChanges vertexShaderState;
			PipelineStateChangeFlags newState = GetPipelineStateChanges(state, out vertexShaderState, out pixelShaderState);

			// No change since the last state, so leave.
			SetStates(newState, state, vertexShaderState, pixelShaderState);

			if (state == _defaultState)
			{
				_previousStates = PipelineStateChangeFlags.None;
				return;
			}

			// Reset unused states.
			PipelineStateChangeFlags resetFlags = _previousStates & ~newState;
			_previousStates = newState;

			SetStates(resetFlags, _defaultState, vertexShaderState, pixelShaderState);
		}

		/// <summary>
		/// Function to bind various states to the pipeline.
		/// </summary>
		/// <param name="stateChange">The type of state change taking place.</param>
		/// <param name="state">The state to apply.</param>
		/// <param name="vertexShaderChanges">The changes to apply to the vertex shader.</param>
		/// <param name="pixelShaderChanges">The changes to apply to the pixel shader.</param>
		private void SetStates(PipelineStateChangeFlags stateChange, GorgonPipelineState state, ShaderStateChanges vertexShaderChanges, ShaderStateChanges pixelShaderChanges)
		{
			if (stateChange == PipelineStateChangeFlags.None)
			{
				return;
			}

			// Change render target views and/or the depth stencil view.
			if ((stateChange & PipelineStateChangeFlags.RenderTargetViews) == PipelineStateChangeFlags.RenderTargetViews)
			{
				BindRtvs(state.RenderTargetViews);
			}

			if ((stateChange & PipelineStateChangeFlags.InputLayout) == PipelineStateChangeFlags.InputLayout)
			{
				D3DDeviceContext.InputAssembler.InputLayout = state.InputLayout?.D3DLayout;
				_currentState.InputLayout = state.InputLayout;
			}

			if ((stateChange & PipelineStateChangeFlags.Viewport) == PipelineStateChangeFlags.Viewport)
			{
				D3DDeviceContext.Rasterizer.SetViewports(state.Viewports.DXViewports, state.Viewports.DXViewportBindCount);
				_currentState.Viewports = state.Viewports;
			}

			if ((stateChange & PipelineStateChangeFlags.VertexBuffers) == PipelineStateChangeFlags.VertexBuffers)
			{
				D3DDeviceContext.InputAssembler.SetVertexBuffers(0, state.VertexBuffers.D3DBindings);
				_currentState.VertexBuffers = state.VertexBuffers;
			}

			if ((stateChange & PipelineStateChangeFlags.IndexBuffer) == PipelineStateChangeFlags.IndexBuffer)
			{
				GI.Format format = GI.Format.Unknown;
				if (state.IndexBuffer != null)
				{
					format = state.IndexBuffer.IndexFormat;
				}

				D3DDeviceContext.InputAssembler.SetIndexBuffer(state.IndexBuffer?.D3DBuffer, format, 0);
			}

			if (((stateChange & PipelineStateChangeFlags.VertexShader) == PipelineStateChangeFlags.VertexShader)
				&& (vertexShaderChanges != ShaderStateChanges.None))
			{
				BindVertexShaderState(state.VertexShader, vertexShaderChanges);
				_currentState.VertexShader = state.VertexShader;
			}

			if (((stateChange & PipelineStateChangeFlags.PixelShader) == PipelineStateChangeFlags.PixelShader)
				&& (pixelShaderChanges != ShaderStateChanges.None))
			{
				BindPixelShaderState(state.PixelShader, pixelShaderChanges);
				_currentState.PixelShader = state.PixelShader;
			}
		}

		/// <summary>
		/// Function to submit recorded states to the GPU for rendering.
		/// </summary>
		public void Submit(GorgonDrawIndexedCall drawCall)
		{
			if (drawCall == null)
			{
				throw new ArgumentNullException(nameof(drawCall));
			}

			ApplyPipelineState(drawCall.State);

			// Change the topology if necessary.
			if (drawCall.PrimitiveTopology != D3DDeviceContext.InputAssembler.PrimitiveTopology)
			{
				D3DDeviceContext.InputAssembler.PrimitiveTopology = drawCall.PrimitiveTopology;
			}

			D3DDeviceContext.DrawIndexed(drawCall.IndexCount, drawCall.IndexStart, drawCall.BaseVertexIndex);
		}

		/// <summary>
		/// Function to clear a specific render target view.
		/// </summary>
		/// <param name="view">The <see cref="GorgonRenderTargetView"/> to clear.</param>
		/// <param name="color">The color used to fill the view with.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="view"/> parameter is <b>null</b>. This is only thrown when Gorgon is compiled in DEBUG mode.</exception>
		// TODO: Where should this go? Maybe on the RTV/DSV?
		public void ClearRenderTargetView(GorgonRenderTargetView view, GorgonColor color)
		{
			view.ValidateObject(nameof(view));

			D3DDeviceContext.ClearRenderTargetView(view.D3DRenderTargetView, color.ToRawColor4());
		}

        /// <summary>
        /// Function to clear the states for the graphics object.
        /// </summary>
        /// <param name="flush">[Optional] <b>true</b> to flush the queued graphics object commands, <b>false</b> to leave as is.</param>
        /// <remarks>If <paramref name="flush"/> is set to <b>true</b>, then a performance penalty is incurred.</remarks>
        public void ClearState(bool flush = false)
        {
            if (flush)
            {
				D3DDeviceContext.Flush();
            }

			// Set default states.
			D3DDeviceContext.ClearState();

			ApplyPipelineState(_defaultState);
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

			// Disconnect from the context.
			_log.Print($"Destroying GorgonGraphics interface for device '{0}'...", LoggingLevel.Simple, device.Info.Name);

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
		/// When the <paramref name="videoDeviceInfo"/> is set to <b>null</b> (<i>Nothing</i> in VB.Net), Gorgon will use the first video device with feature level specified by <paramref name="featureLevel"/>  
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
		/// The Gorgon Graphics library only works on Windows 7 (with the Platform Update for Direct 3D 11.1) or better. No other operating system is supported at this time.
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
			if (!Win32API.IsWindows7SP1OrGreater())
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

			_currentState = _defaultState;
			_log = log ?? GorgonLogDummy.DefaultInstance;
			
			_log.Print("Gorgon Graphics initializing...", LoggingLevel.Simple);
			_log.Print($"Using video device '{videoDeviceInfo.Name}' at feature level [{featureLevel.Value}] for Direct 3D 11.1.", LoggingLevel.Simple);

			_videoDevice = new VideoDevice(videoDeviceInfo, featureLevel.Value, _log);
			_deviceContext = _videoDevice.D3DDevice.ImmediateContext1;

			_log.Print("Gorgon Graphics initialized.", LoggingLevel.Simple);
		}

		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Gorgon.Native.Win32API.DwmIsCompositionEnabled(System.Boolean@)")]
		static GorgonGraphics()
		{
			// Only do this on Windows 7.  On Windows 8 this value could be wrong and is not needed.
			if ((GorgonComputerInfo.OperatingSystemVersion.Major == 6)
			    || (GorgonComputerInfo.OperatingSystemVersion.Minor == 1))
			{
				if (Win32API.DwmIsCompositionEnabled(out _isDWMEnabled) != 0) // S_OK
				{
					_isDWMEnabled = false;
				}
			}

			if (!_isDWMEnabled)
			{
				_dontEnableDWM = true;
			}

			DX.Configuration.ThrowOnShaderCompileError = false;

#if DEBUG
			IsDebugEnabled = true;
#endif
		}
		#endregion
	}
}
