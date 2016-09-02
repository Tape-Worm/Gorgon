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
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.Math;
using Gorgon.Native;
using SharpDX.Mathematics.Interop;
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
		// The current device context.
		private D3D11.DeviceContext1 _deviceContext;
		// The last used draw call.
		private GorgonDrawIndexedCall _lastDrawCall;
		// Flags to indicate that all resources should be set.
		private static readonly PipelineResourceChangeFlags _allResourcesChanged;
		// Pipeline state cache.
	    private readonly List<GorgonPipelineState> _stateCache = new List<GorgonPipelineState>();
		// Synchronization lock for creating new pipeline cache entries.
		private readonly object _stateCacheLock = new object();
		#endregion

        #region Properties.
		/// <summary>
		/// Property to return the Direct 3D 11.1 device context for this graphics instance.
		/// </summary>
		internal D3D11.DeviceContext1 D3DDeviceContext => _deviceContext;

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
		/// Function to initialize a <see cref="GorgonPipelineState" /> object with Direct 3D 11 state objects by creating new objects for the unassigned values.
		/// </summary>
		/// <param name="newState">The new state.</param>
		private void InitializePipelineState(GorgonPipelineState newState)
		{
			D3D11.Device1 videoDevice = VideoDevice.D3DDevice();
			IGorgonPipelineStateInfo info = newState.Info;

			if ((newState.D3DInputLayout == null) && (newState.Info.InputLayout.Elements != null) && (info.InputLayout.Elements.Count > 0) && (info.VertexShader != null))
			{
				newState.D3DInputLayout = new D3D11.InputLayout(VideoDevice.D3DDevice(), info.VertexShader.D3DByteCode, info.InputLayout?.D3DInputElements)
				{
					DebugName = "Gorgon D3D11InputLayout"
				};
			}

			if ((newState.D3DRasterState == null) && (info.RasterState != null))
			{
				newState.D3DRasterState = new D3D11.RasterizerState1(videoDevice, info.RasterState.ToRasterStateDesc1())
				{
					DebugName = "Gorgon D3D11RasterState"
				};
			}

			if ((newState.D3DDepthStencilState == null) && (info.DepthStencilState != null))
			{
				newState.D3DDepthStencilState = new D3D11.DepthStencilState(videoDevice, info.DepthStencilState.ToDepthStencilStateDesc())
				{
					DebugName = "Gorgon D3D11DepthStencilState"
				};
			}

			if (info.ScissorRectangles != null)
			{
				newState.DXScissorRectangles = info.ScissorRectangles.Select(item => (RawRectangle)item).ToArray();
			}

			if (info.Viewports != null)
			{
				newState.DXViewports = info.Viewports.Select(item => (RawViewportF)item).ToArray();
			}

			if ((newState.D3DBlendState != null) || (info.RenderTargetBlendState == null) || (info.RenderTargetBlendState.Count == 0))
			{
				return;
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

			newState.D3DBlendState = new D3D11.BlendState1(videoDevice, desc)
			{
				DebugName = "Gorgon D3D11BlendState"
			};
	}

	/// <summary>
	/// Function to build up a <see cref="GorgonPipelineState"/> object with Direct 3D 11 state objects by either creating new objects, or inheriting previous ones.
	/// </summary>
	/// <param name="newState">The new state to initialize.</param>
	private void SetupPipelineState(GorgonPipelineState newState)
		{
			const PipelineStateChangeFlags allStatesInherited = PipelineStateChangeFlags.BlendState
																| PipelineStateChangeFlags.DepthStencilState
																| PipelineStateChangeFlags.RasterState
																| PipelineStateChangeFlags.InputLayout;

			D3D11.InputLayout inputLayout = null;
			D3D11.RasterizerState1 rasterState = null;
			D3D11.DepthStencilState depthStencilState = null;
			D3D11.BlendState1 blendState = null;
			int blendStateEqualCount = 0;
			PipelineStateChangeFlags inheritedState = PipelineStateChangeFlags.None;
			IGorgonPipelineStateInfo newStateInfo = newState.Info;

			for (int i = 0; i < _stateCache.Count; ++i)
			{
				GorgonPipelineState cachedState = _stateCache[i];

				if (cachedState == newState)
				{
					continue;
				}

				IGorgonPipelineStateInfo cachedStateInfo = _stateCache[i].Info;

				// Reuse the input layout if they're the same.
				if (cachedStateInfo.InputLayout.IsEqual(newStateInfo.InputLayout))
				{
					inputLayout = cachedState.D3DInputLayout;
					inheritedState |= PipelineStateChangeFlags.InputLayout;
				}

				if (cachedStateInfo.RasterState.IsEqual(newStateInfo.RasterState))
				{
					rasterState = cachedState.D3DRasterState;
					inheritedState |= PipelineStateChangeFlags.RasterState;
				}

				if (cachedStateInfo.DepthStencilState.IsEqual(newStateInfo.DepthStencilState))
				{
					depthStencilState = cachedState.D3DDepthStencilState;
					inheritedState |= PipelineStateChangeFlags.DepthStencilState;
				}

				// ReSharper disable once PossibleUnintendedReferenceComparison
				if (newStateInfo.RenderTargetBlendState == cachedStateInfo.RenderTargetBlendState)
				{
					blendState = cachedState.D3DBlendState;
					inheritedState |= PipelineStateChangeFlags.BlendState;
				}
				else
				{
					if (newStateInfo.RenderTargetBlendState.Count == cachedStateInfo.RenderTargetBlendState.Count)
					{
						for (int j = 0; j < newStateInfo.RenderTargetBlendState.Count; ++j)
						{
							if (cachedStateInfo.RenderTargetBlendState[j].IsEqual(newStateInfo.RenderTargetBlendState[j]))
							{
								blendStateEqualCount++;
							}
						}

						if (blendStateEqualCount == newStateInfo.RenderTargetBlendState.Count)
						{
							blendState = cachedState.D3DBlendState;
							inheritedState |= PipelineStateChangeFlags.BlendState;
						}
					}
				}

				// We've copied all the states.
				if (inheritedState == allStatesInherited)
				{
					break;
				}
			}

			newState.D3DBlendState = blendState;
			newState.D3DRasterState = rasterState;
			newState.D3DDepthStencilState = depthStencilState;
			newState.D3DInputLayout = inputLayout;

			// Setup any uninitialized states.
			InitializePipelineState(newState);
		}

		/// <summary>
		/// Function to apply resource bindings to the GPU pipeline.
		/// </summary>
		/// <param name="resources">The resources to bind to the pipeline.</param>
		private void ApplyResources(GorgonPipelineResources resources)
		{
			// If we didn't have any resources to bind (or unbind), then leave.  We'll keep the last state.
			if (resources == null)
			{
				return;
			}

			GorgonPipelineResources lastResources = _lastDrawCall?.Resources;
			PipelineResourceChangeFlags changes = lastResources?.GetChanges(resources) ?? _allResourcesChanged;

			if (changes == PipelineResourceChangeFlags.None)
			{
				return;
			}

			if ((changes & PipelineResourceChangeFlags.RenderTargets) == PipelineResourceChangeFlags.RenderTargets)
			{
				D3DDeviceContext.OutputMerger.SetTargets(resources.RenderTargets.DepthStencilView?.D3DView,
														 resources.RenderTargets.BindCount,
														 resources.RenderTargets.D3DRenderTargetViews);
			}

			if ((changes & PipelineResourceChangeFlags.VertexBuffer) == PipelineResourceChangeFlags.VertexBuffer)
			{
				D3DDeviceContext.InputAssembler.SetVertexBuffers(0, resources.VertexBuffers.D3DBindings);
			}

			if ((changes & PipelineResourceChangeFlags.PixelShaderConstantBuffer) == PipelineResourceChangeFlags.PixelShaderConstantBuffer)
			{
				D3DDeviceContext.PixelShader.SetConstantBuffers(0,
				                                                resources.PixelShaderConstantBuffers.BindCount,
				                                                resources.PixelShaderConstantBuffers.D3DConstantBuffers);
			}

			if ((changes & PipelineResourceChangeFlags.VertexShaderConstantBuffer) == PipelineResourceChangeFlags.VertexShaderConstantBuffer)
			{
				D3DDeviceContext.VertexShader.SetConstantBuffers(0,
				                                                 resources.VertexShaderConstantBuffers.BindCount,
				                                                 resources.VertexShaderConstantBuffers.D3DConstantBuffers);
			}

			if ((changes & PipelineResourceChangeFlags.PixelShaderResource) == PipelineResourceChangeFlags.PixelShaderResource)
			{
				D3DDeviceContext.PixelShader.SetShaderResources(resources.PixelShaderResources.BindIndex,
					                                            resources.PixelShaderResources.BindCount,
					                                            resources.PixelShaderResources.D3DShaderResourceViews);
			}

			if ((changes & PipelineResourceChangeFlags.VertexShaderResource) == PipelineResourceChangeFlags.VertexShaderResource)
			{
				D3DDeviceContext.VertexShader.SetShaderResources(resources.VertexShaderResources.BindIndex,
				                                                 resources.VertexShaderResources.BindCount,
				                                                 resources.VertexShaderResources.D3DShaderResourceViews);
			}

			if ((changes & PipelineResourceChangeFlags.PixelShaderSampler) == PipelineResourceChangeFlags.PixelShaderSampler)
			{
				D3DDeviceContext.PixelShader.SetSamplers(resources.PixelShaderSamplers.BindIndex, resources.PixelShaderSamplers.BindCount, resources.PixelShaderSamplers.D3DSamplerStates);
			}

			if ((changes & PipelineResourceChangeFlags.IndexBuffer) == PipelineResourceChangeFlags.IndexBuffer)
			{
				D3DDeviceContext.InputAssembler.SetIndexBuffer(resources.IndexBuffer?.D3DBuffer, resources.IndexBuffer?.IndexFormat ?? GI.Format.Unknown, 0);
			}
		}

		/// <summary>
		/// Function to apply a pipeline state to the pipeline.
		/// </summary>
		/// <param name="state">A <see cref="GorgonPipelineState"/> to apply to the pipeline.</param>
		/// <remarks>
		/// <para>
		/// This is responsible for setting all the states for a pipeline at once. This has the advantage of ensuring that duplicate states do not get set so that performance is not impacted. 
		/// </para>
		/// </remarks>
		private void ApplyPipelineState(GorgonPipelineState state)
		{
			if (state == null)
			{
				return;
			}

			GorgonPipelineState lastState = _lastDrawCall?.State;
			PipelineStateChangeFlags changes = state.GetChanges(lastState);

			if (changes == PipelineStateChangeFlags.None)
			{
				return;
			}
			
			if ((changes & PipelineStateChangeFlags.InputLayout) == PipelineStateChangeFlags.InputLayout)
			{
				D3DDeviceContext.InputAssembler.InputLayout = state.D3DInputLayout;
			}

			if (((changes & PipelineStateChangeFlags.Viewport) == PipelineStateChangeFlags.Viewport)
				&& (state.DXViewports != null))
			{
				D3DDeviceContext.Rasterizer.SetViewports(state.DXViewports, state.DXViewports.Length);
			}
			
			if (((changes & PipelineStateChangeFlags.ScissorRectangles) == PipelineStateChangeFlags.ScissorRectangles)
				&& (state.DXScissorRectangles != null))
			{
				D3DDeviceContext.Rasterizer.SetScissorRectangles(state.DXScissorRectangles);
			}

			if ((changes & PipelineStateChangeFlags.RasterState) == PipelineStateChangeFlags.RasterState)
			{
				D3DDeviceContext.Rasterizer.State = state.D3DRasterState;
			}

			if ((changes & PipelineStateChangeFlags.DepthStencilState) == PipelineStateChangeFlags.DepthStencilState)
			{
				D3DDeviceContext.OutputMerger.DepthStencilState = state.D3DDepthStencilState;
			}

			if ((changes & PipelineStateChangeFlags.BlendState) == PipelineStateChangeFlags.BlendState)
			{
				D3DDeviceContext.OutputMerger.BlendState = state.D3DBlendState;
			}

			if ((changes & PipelineStateChangeFlags.VertexShader) == PipelineStateChangeFlags.VertexShader)
			{
				D3DDeviceContext.VertexShader.Set(state.Info.VertexShader?.D3DShader);
			}

			if ((changes & PipelineStateChangeFlags.PixelShader) == PipelineStateChangeFlags.PixelShader)
			{
				D3DDeviceContext.PixelShader.Set(state.Info.PixelShader?.D3DShader);
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
		// TODO: Make this a lot more generic (or overloaded if we need to avoid virtual methods/properties) so we can submit stream out, instanceed, non-indexed, etc...
		public void Submit(GorgonDrawIndexedCall drawCall)
		{
			drawCall.ValidateObject(nameof(drawCall));

			ApplyResources(drawCall.Resources);
			ApplyPipelineState(drawCall.State);

			// We can just check the current primitive topology here and assign it rather than going through the pipeline state.
			// It's entirely possible that we may not be drawing primitives here, so having as "state" doesn't make a lot of sense.
			if ((_lastDrawCall == null) || (_lastDrawCall.PrimitiveTopology != drawCall.PrimitiveTopology))
			{
				D3DDeviceContext.InputAssembler.PrimitiveTopology = drawCall.PrimitiveTopology;
			}
			
			// These are immediate values which are set directly on the output merger.
			if ((_lastDrawCall == null) || (!_lastDrawCall.BlendFactor.Equals(drawCall.BlendFactor)))
			{
				D3DDeviceContext.OutputMerger.BlendFactor = drawCall.BlendFactor.ToRawColor4();
			}

			if ((_lastDrawCall == null) || (_lastDrawCall.BlendSampleMask != drawCall.BlendSampleMask))
			{
				D3DDeviceContext.OutputMerger.BlendSampleMask = drawCall.BlendSampleMask;
			}

			if ((_lastDrawCall == null) || (_lastDrawCall.DepthStencilReference != drawCall.DepthStencilReference))
			{
				D3DDeviceContext.OutputMerger.DepthStencilReference = drawCall.DepthStencilReference;
			}
			
			D3DDeviceContext.DrawIndexed(drawCall.IndexCount, drawCall.IndexStart, drawCall.BaseVertexIndex);

			_lastDrawCall = drawCall;
		}

		/// <summary>
		/// Function to create a pipeline state.
		/// </summary>
		/// <param name="info">Information used to define the pipeline state.</param>
		/// <returns>A new <see cref="GorgonPipelineState"/>, or an existing one if one was already created.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="info"/> has an <see cref="IGorgonPipelineStateInfo.InputLayout"/> defined, but no <see cref="IGorgonPipelineStateInfo.VertexShader"/>.</exception>
		public GorgonPipelineState CreatePipelineState(IGorgonPipelineStateInfo info)
	    {
		    if (info == null)
		    {
			    throw new ArgumentNullException(nameof(info));
		    }

			if ((info.VertexShader == null) && (info.InputLayout != null))
			{
				throw new ArgumentException(Resources.GORGFX_ERR_INPUT_LAYOUT_NEEDS_SHADER, nameof(info));
			}

			var result = new GorgonPipelineState(this, info, _stateCache.Count);

			// Threads have to wait their turn.
			lock(_stateCacheLock)
			{
				SetupPipelineState(result);
			    _stateCache.Add(result);
		    }

		    return result;
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
			_lastDrawCall = null;
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

			// Wipe out the state cache.
			for (int i = 0; i < _stateCache.Count; ++i)
			{
				_stateCache[i].D3DRasterState?.Dispose();
				_stateCache[i].D3DInputLayout?.Dispose();
				_stateCache[i].D3DDepthStencilState?.Dispose();
				_stateCache[i].D3DBlendState?.Dispose();
			}

			_stateCache.Clear();

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

			var flags = (PipelineResourceChangeFlags[])Enum.GetValues(typeof(PipelineResourceChangeFlags));

			foreach (PipelineResourceChangeFlags flag in flags.Where(item => item != PipelineResourceChangeFlags.None))
			{
				_allResourcesChanged |= flag;
			}

#if DEBUG
			IsDebugEnabled = true;
#endif
		}
		#endregion
	}
}
