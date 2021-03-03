#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: December 30, 2020 8:59:46 PM
// 
#endregion

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Gorgon.Collections;
using Gorgon.Diagnostics;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;
using DX = SharpDX;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Functionality for evaluating state changes.
    /// </summary>
    internal class StateEvaluator
    {
		#region Variables.
		// The previous depth/stencil reference value.
		private int _depthStencilReference;
		// The previous blend sample mask.
		private int _blendSampleMask = int.MinValue;
		// The previous blend factor.
		private GorgonColor _blendFactor = GorgonColor.White;
		// The log for debug messages.
		private readonly IGorgonLog _log;
		// The device context.
		private readonly GorgonGraphics _graphics;

		// The previously assigned pipeline state.
		private readonly GorgonPipelineState _prevPipelineState = new GorgonPipelineState
        {
            PrimitiveType = PrimitiveType.None
        };

		// The ranges of resource arrays that were updated.
		private readonly ResourceRanges _ranges = new ResourceRanges();

        // The previously assigned resource state.
        private readonly D3DState _prevResourceState = new D3DState
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
			VsSamplers = new GorgonSamplerStates()
		};
		#endregion

		#region Properties.
		// The viewports used to define the area to render into.
		public DX.ViewportF[] Viewports = new DX.ViewportF[16];

		// The rectangles used to define the clipping area.
		public DX.Rectangle[] Scissors = new DX.Rectangle[16];

		// The currently bound render targets.
		public GorgonRenderTargetView[] RenderTargets = new GorgonRenderTargetView[D3D11.OutputMergerStage.SimultaneousRenderTargetCount];

		// The currently bound depth/stencil buffer.
		public GorgonDepthStencil2DView DepthStencil;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to compare the index buffer binding with stream out bindings.
        /// </summary>
        /// <param name="state">The state containing the index buffer to evaluate.</param>
        /// <param name="streamOut">The current list of stream out buffers.</param>
        private void CheckIndexBufferStreamOut(D3DState state, GorgonStreamOutBindings streamOut)
		{
			void ScanStreamOut(GorgonStreamOutBindings bindings, in (int Start, int Count) streamOutIndices)
			{
				for (int s = streamOutIndices.Start; s < streamOutIndices.Count + streamOutIndices.Start; ++s)
				{
					GorgonBufferCommon soBuffer = bindings[s].Buffer;

					if (((soBuffer is null) || (soBuffer.BindFlags != D3D11.BindFlags.VertexBuffer))
						|| (soBuffer != state.IndexBuffer))
					{
						continue;
					}

					if (GorgonGraphics.IsDebugEnabled)
					{
						_log.Print($"[Warning] The index buffer '{state.IndexBuffer.Name}' is bound for input and stream out. It will be unbound from the input assembler.", LoggingLevel.Verbose);
					}

					state.IndexBuffer = null;
				}
			}

			if ((state.IndexBuffer is null) || ((state.IndexBuffer.BindFlags & D3D11.BindFlags.StreamOutput) != D3D11.BindFlags.StreamOutput))
			{
				return;
			}

			ref readonly (int Start, int Count) newStreamOut = ref streamOut.GetDirtyItems();

			if (newStreamOut.Count == 0)
			{
				return;
			}

			ScanStreamOut(streamOut, in newStreamOut);

			ref readonly (int Start, int Count) prevStreamOut = ref _prevResourceState.StreamOutBindings.GetDirtyItems();

			if (prevStreamOut.Count == 0)
			{
				return;
			}

			ScanStreamOut(_prevResourceState.StreamOutBindings, in prevStreamOut);
		}

		/// <summary>
		/// Function to compare vertex buffer bindings with stream out bindings.
		/// </summary>
		/// <param name="vertexBuffers">The current list of vertex buffers.</param>
		/// <param name="streamOut">The current list of stream out buffers.</param>
		private void CheckVertexBufferStreamOut(GorgonVertexBufferBindings vertexBuffers, GorgonStreamOutBindings streamOut)
		{
			void ScanVertexBuffers(GorgonBufferCommon soBuffer, in (int Start, int Count) vBufferIndices)
			{
				for (int v = vBufferIndices.Start; v < vBufferIndices.Count + vBufferIndices.Start; ++v)
				{
					GorgonBufferCommon vBuffer = vertexBuffers[v].VertexBuffer;

					if (((vBuffer is null) || ((vBuffer.BindFlags & D3D11.BindFlags.StreamOutput) != D3D11.BindFlags.StreamOutput))
						|| (soBuffer != vBuffer))
					{
						continue;
					}

					if (GorgonGraphics.IsDebugEnabled)
					{
						_log.Print($"[Warning] The vertex buffer '{vBuffer.Name}' is bound for input and stream out. It will be unbound from the vertex buffers.", LoggingLevel.Verbose);
					}
					vertexBuffers.ResetAt(v);
				}
			}

			ref readonly (int Start, int Count) newVertexBuffers = ref vertexBuffers.GetDirtyItems();
			ref readonly (int Start, int Count) streamOutBuffers = ref streamOut.GetDirtyItems();

			if ((newVertexBuffers.Count == 0) || (streamOutBuffers.Count == 0))
			{
				return;
			}

			for (int s = streamOutBuffers.Start; s < streamOutBuffers.Count + streamOutBuffers.Start; ++s)
			{
				GorgonBufferCommon soBuffer = streamOut[s].Buffer;

				if ((soBuffer is null) || ((soBuffer.BindFlags & D3D11.BindFlags.VertexBuffer) != D3D11.BindFlags.VertexBuffer))
				{
					continue;
				}

				ScanVertexBuffers(soBuffer, in newVertexBuffers);
			}

			streamOutBuffers = ref _prevResourceState.StreamOutBindings.GetDirtyItems();

			if (streamOutBuffers.Count == 0)
			{
				return;
			}

			for (int s = streamOutBuffers.Start; s < streamOutBuffers.Count + streamOutBuffers.Start; ++s)
			{
				GorgonBufferCommon soBuffer = streamOut[s].Buffer;

				if ((soBuffer is null) || ((soBuffer.BindFlags & D3D11.BindFlags.VertexBuffer) != D3D11.BindFlags.VertexBuffer))
				{
					continue;
				}

				ScanVertexBuffers(soBuffer, in newVertexBuffers);
			}
		}

		/// <summary>
        /// Function to compare shader resource views with render target views.
        /// </summary>
        /// <param name="srvs">The shader resource views to compare.</param>
		private void CheckSrvsRtvs(GorgonArray<GorgonShaderResourceView> srvs)
		{
			ref readonly (int Start, int Count) indices = ref srvs.GetDirtyItems();

			if (indices.Count == 0)
			{
				return;
			}

			if ((_graphics.DepthStencilView != null) && ((_graphics.DepthStencilView.Binding & TextureBinding.ShaderResource) == TextureBinding.ShaderResource))
			{
				for (int s = indices.Start; s < indices.Count + indices.Start; ++s)
				{
					GorgonGraphicsResource srvResource = srvs[s]?.Resource;

					if ((srvResource is null) || ((srvResource.BindFlags & D3D11.BindFlags.DepthStencil) != D3D11.BindFlags.DepthStencil)
						|| (srvResource != _graphics.DepthStencilView.Resource))
					{
						continue;
					}

					if (GorgonGraphics.IsDebugEnabled)
					{
						_log.Print($"[Warning] The shader resource '{srvResource.Name}' is bound for input and as a depth/stencil. It will be unbound from the shader resources.", LoggingLevel.Verbose);
					}

					srvs[s] = null;
				}
			}

			for (int r = 0; r < _graphics.RenderTargets.Count; ++r)
			{
				GorgonGraphicsResource rtvResource = _graphics.RenderTargets[r]?.Resource;

				if ((rtvResource is null) || ((rtvResource.BindFlags & D3D11.BindFlags.ShaderResource) != D3D11.BindFlags.ShaderResource))
				{
					continue;
				}

				for (int s = indices.Start; s < indices.Count + indices.Start; ++s)
				{
					GorgonGraphicsResource srvResource = srvs[s]?.Resource;

					if ((srvResource is null) 
						|| ((srvResource.BindFlags & D3D11.BindFlags.RenderTarget) != D3D11.BindFlags.RenderTarget)
						|| (srvResource != rtvResource))
					{
						continue;
					}

					if (GorgonGraphics.IsDebugEnabled)
					{
						_log.Print($"[Warning] The shader resource '{srvResource.Name}' is bound for input and as a render target. It will be unbound from the shader resources.", LoggingLevel.Verbose);
					}
					srvs[s] = null;
				}
			}
		}

		/// <summary>
		/// Function to compare shader resource views with unordered access views.
		/// </summary>
		/// <param name="srvs">The shader resource views to compare.</param>
        /// <param name="uavs">The unordered access views to compare.</param>
		private void CheckSrvsUavs(GorgonArray<GorgonShaderResourceView> srvs, GorgonArray<GorgonReadWriteViewBinding> uavs)
		{
			ref readonly (int Start, int Count) uavIndices = ref uavs.GetDirtyItems();
			ref readonly (int Start, int Count) srvIndices = ref srvs.GetDirtyItems();

			if ((srvIndices.Count == 0) || (uavIndices.Count == 0))
			{
				return;
			}

			for (int u = uavIndices.Start; u < uavIndices.Count + uavIndices.Start; ++u)
			{
				GorgonGraphicsResource uavResource = uavs[u].ReadWriteView?.Resource;

				if ((uavResource is null) || ((uavResource.BindFlags & D3D11.BindFlags.ShaderResource) != D3D11.BindFlags.ShaderResource))
				{
					continue;
				}

				for (int s = srvIndices.Start; s < srvIndices.Count + srvIndices.Start; ++s)
				{
					GorgonGraphicsResource srvResource = srvs[s]?.Resource;

					if ((srvResource is null)
						|| ((srvResource.BindFlags & D3D11.BindFlags.UnorderedAccess) != D3D11.BindFlags.UnorderedAccess)
						|| (srvResource != uavResource))
					{
						continue;
					}

					if (GorgonGraphics.IsDebugEnabled)
					{
						_log.Print($"[Warning] The shader resource '{srvResource.Name}' is bound for input and as an unordered access view. It will be unbound from the shader resources.", LoggingLevel.Verbose);
					}

					srvs[s] = null;
				}
			}
		}

		/// <summary>
		/// Function to check the resources to ensure they're not bound on output and input at the same time.
		/// </summary>
		/// <param name="state">The state that will be assigned.</param>
		private void CheckHazards(D3DState state)
		{
			CheckVertexBufferStreamOut(state.VertexBuffers, state.StreamOutBindings);
			CheckIndexBufferStreamOut(state, state.StreamOutBindings);
			
			CheckSrvsRtvs(state.VsSrvs);
			CheckSrvsRtvs(state.PsSrvs);
			CheckSrvsRtvs(state.GsSrvs);
			CheckSrvsRtvs(state.HsSrvs);
			CheckSrvsRtvs(state.DsSrvs);
			CheckSrvsRtvs(state.CsSrvs);

			CheckSrvsUavs(state.VsSrvs, state.ReadWriteViews);
			CheckSrvsUavs(state.PsSrvs, state.ReadWriteViews);
			CheckSrvsUavs(state.GsSrvs, state.ReadWriteViews);
			CheckSrvsUavs(state.HsSrvs, state.ReadWriteViews);
			CheckSrvsUavs(state.DsSrvs, state.ReadWriteViews);
			CheckSrvsUavs(state.CsSrvs, state.ReadWriteViews);
		}

		/// <summary>
        /// Function to check the depth stencil view resource for bindings in the shader resource views.
        /// </summary>
        /// <param name="srvs">The list of shader resource views to evaluate.</param>
        /// <param name="depth">The depth/stencil view to evaluate.</param>
        /// <param name="shaderStage">The current shader stage being evaluated.</param>
		private void CheckDsvSrvsHazards(GorgonArray<GorgonShaderResourceView> srvs, GorgonDepthStencil2DView depth, D3D11.CommonShaderStage shaderStage)
		{
			ref readonly (int Start, int Count) indices = ref srvs.GetDirtyItems();

			if (indices.Count == 0)
			{
				return;
			}

			GorgonGraphicsResource depthResource = depth?.Resource;

			if ((depthResource is null) || ((depthResource.BindFlags & D3D11.BindFlags.ShaderResource) != D3D11.BindFlags.ShaderResource))
			{
				return;
			}

			for (int s = indices.Start; s < indices.Count + indices.Start; ++s)
			{
				GorgonGraphicsResource srv = srvs[s]?.Resource;

				if ((srv is null) || ((srv.BindFlags & D3D11.BindFlags.DepthStencil) == D3D11.BindFlags.DepthStencil) || (srv != depthResource))
				{
					continue;
				}

				if (GorgonGraphics.IsDebugEnabled)
				{
					_log.Print($"[Warning] The depth buffer resource '{depth.Resource.Name}' is bound as an input. Unbinding...", LoggingLevel.Verbose);
				}

				srvs[s] = null;
				shaderStage.SetShaderResource(s, null);
			}
		}

		/// <summary>
		/// Function to check the new render target views against any bound shader resource views.
		/// </summary>
		/// <param name="srvs">The shader resource views to evaluate.</param>
		/// <param name="rtv">The new render target view to evaluate.</param>
		/// <param name="shaderStage">The current shader stage being evaluated.</param>
		private void CheckRtvSrvsHazards(GorgonArray<GorgonShaderResourceView> srvs, GorgonRenderTargetView rtv, D3D11.CommonShaderStage shaderStage)
		{
			ref readonly (int Start, int Count) indices = ref srvs.GetDirtyItems();

			if (indices.Count == 0)
			{
				return;
			}

			GorgonGraphicsResource rtvResource = rtv?.Resource;			

			if ((rtvResource is null) || ((rtvResource.BindFlags & D3D11.BindFlags.ShaderResource) != D3D11.BindFlags.ShaderResource))
			{
				return;
			}

			for (int s = indices.Start; s < indices.Count + indices.Start; ++s)
			{
				GorgonGraphicsResource srv = srvs[s]?.Resource;

				if ((srv is null) || ((srv.BindFlags & D3D11.BindFlags.RenderTarget) != D3D11.BindFlags.RenderTarget) || (srv != rtvResource))
				{
					continue;
				}

				srvs[s] = null;
				shaderStage.SetShaderResource(s, null);
			}
		}

		/// <summary>
		/// Function to evaluate two arrays of resources to determine the differences between them.
		/// </summary>
		/// <typeparam name="T">The type of element in the array. Must implement <see cref="IEquatable{T}"/>.</typeparam>
		/// <param name="left">The left array to compare.</param>
		/// <param name="right">The right array to compare.</param>
		/// <param name="change">The type of change being evaluated.</param>
		/// <param name="changes">The current set of changes to be updated.</param>
		/// <param name="range">The unioned range of indices that have changed between the two arrays.</param>
		private void CheckArray<T>(GorgonArray<T> left, GorgonArray<T> right, ResourceStateChanges change, ref ResourceStateChanges changes, ref (int start, int count) range)
			where T : IEquatable<T>
		{
			Debug.Assert(left.Length == right.Length, $"Both arrays must be of the same length. Left: {left.Length}, Right: {right.Length}");

			ref readonly (int start, int count) leftIndices = ref left.GetDirtyItems();
			ref readonly (int start, int count) rightIndices = ref right.GetDirtyItems();

			int leftEnd = (leftIndices.count + leftIndices.start);
			int rightEnd = (rightIndices.count + rightIndices.start);
			int start = leftIndices.start.Min(rightIndices.start);
			int end = leftEnd.Max(rightEnd);

			for (int i = start; i < end; ++i)
			{
				left[i] = right[i];				
			}

			if (!left.IsDirty)
			{
				return;
			}					

			changes |= change;
			range = (start, end - start);
		}

		/// <summary>
		/// Function to fix the render target view resource sharing hazards prior to setting the rtv.
		/// </summary>
		/// <param name="rtViews">The render target views being assigned.</param>
		/// <param name="depth">The depth stencil being assigned.</param>
		private void CheckRtvsForSrvHazards(ReadOnlySpan<GorgonRenderTargetView> rtViews, GorgonDepthStencil2DView depth)
		{
			if (depth != null)
			{
				CheckDsvSrvsHazards(_prevResourceState.VsSrvs, depth, _graphics.D3DDeviceContext.VertexShader);
				CheckDsvSrvsHazards(_prevResourceState.PsSrvs, depth, _graphics.D3DDeviceContext.PixelShader);
				CheckDsvSrvsHazards(_prevResourceState.GsSrvs, depth, _graphics.D3DDeviceContext.GeometryShader);
				CheckDsvSrvsHazards(_prevResourceState.HsSrvs, depth, _graphics.D3DDeviceContext.HullShader);
				CheckDsvSrvsHazards(_prevResourceState.DsSrvs, depth, _graphics.D3DDeviceContext.DomainShader);
				CheckDsvSrvsHazards(_prevResourceState.CsSrvs, depth, _graphics.D3DDeviceContext.ComputeShader);
			}

			if (rtViews.Length == 0)
			{
				return;
			}

			for (int i = 0; i < rtViews.Length; ++i)
			{
				CheckRtvSrvsHazards(_prevResourceState.VsSrvs, rtViews[i], _graphics.D3DDeviceContext.VertexShader);
				CheckRtvSrvsHazards(_prevResourceState.PsSrvs, rtViews[i], _graphics.D3DDeviceContext.PixelShader);
				CheckRtvSrvsHazards(_prevResourceState.GsSrvs, rtViews[i], _graphics.D3DDeviceContext.GeometryShader);
				CheckRtvSrvsHazards(_prevResourceState.HsSrvs, rtViews[i], _graphics.D3DDeviceContext.HullShader);
				CheckRtvSrvsHazards(_prevResourceState.DsSrvs, rtViews[i], _graphics.D3DDeviceContext.DomainShader);
				CheckRtvSrvsHazards(_prevResourceState.CsSrvs, rtViews[i], _graphics.D3DDeviceContext.ComputeShader);
			}
		}

		/// <summary>
		/// Function to retrieve the changes for all resources bound to the pipeline since the last draw call.
		/// </summary>
		/// <param name="newState">The new resource state.</param>
		/// <param name="pipelineStateChanges">The current pipeline state change set.</param>
		/// <returns>A <see cref="ResourceRanges"/> object containing the list of changed resource array indices and the set of change flags.</returns>
		/// <remarks>
		/// <para>
		/// This method is <b>not</b> thread safe. 
		/// </para>
		/// </remarks>
		public ResourceRanges GetResourceStateChanges(D3DState newState, PipelineStateChanges pipelineStateChanges)
		{
			ref ResourceStateChanges result = ref _ranges.Changes;
			result = ResourceStateChanges.None;

			// Unbind any resources that are in conflict with input/output.
			CheckHazards(newState);

			// Handle simple resource changes first.
			if (_prevResourceState.InputLayout != newState.InputLayout)
			{
				_prevResourceState.VertexBuffers.InputLayout = newState.InputLayout;
				result |= ResourceStateChanges.InputLayout;
			}

			if (_prevResourceState.IndexBuffer != newState.IndexBuffer)
			{
				_prevResourceState.IndexBuffer = newState.IndexBuffer;
				result |= ResourceStateChanges.IndexBuffer;
			}

			if (_prevResourceState.ComputeShader != newState.ComputeShader)
			{
				_prevResourceState.ComputeShader = newState.ComputeShader;
				result |= ResourceStateChanges.ComputeShader;
			}

			// Handle array resources.
			CheckArray(_prevResourceState.VertexBuffers, newState.VertexBuffers, ResourceStateChanges.VertexBuffers, ref result, ref _ranges.VertexBuffers);
			CheckArray(_prevResourceState.StreamOutBindings, newState.StreamOutBindings, ResourceStateChanges.StreamOutBuffers, ref result, ref _ranges.StreamOutBuffers);

			if ((newState.PipelineState.VertexShader != null) || ((pipelineStateChanges & PipelineStateChanges.VertexShader) == PipelineStateChanges.VertexShader))
			{
				CheckArray(_prevResourceState.VsConstantBuffers, newState.VsConstantBuffers, ResourceStateChanges.VsConstants, ref result, ref _ranges.VertexShaderConstants);
				CheckArray(_prevResourceState.VsSrvs, newState.VsSrvs, ResourceStateChanges.VsResourceViews, ref result, ref _ranges.VertexShaderResources);
				CheckArray(_prevResourceState.VsSamplers, newState.VsSamplers, ResourceStateChanges.VsSamplers, ref result, ref _ranges.VertexShaderSamplers);
			}

			if ((newState.PipelineState.PixelShader != null) || ((pipelineStateChanges & PipelineStateChanges.PixelShader) == PipelineStateChanges.PixelShader))
			{
				CheckArray(_prevResourceState.PsConstantBuffers, newState.PsConstantBuffers, ResourceStateChanges.PsConstants, ref result, ref _ranges.PixelShaderConstants);
				CheckArray(_prevResourceState.PsSrvs, newState.PsSrvs, ResourceStateChanges.PsResourceViews, ref result, ref _ranges.PixelShaderResources);
				CheckArray(_prevResourceState.PsSamplers, newState.PsSamplers, ResourceStateChanges.PsSamplers, ref result, ref _ranges.PixelShaderSamplers);
			}

			if ((newState.PipelineState.GeometryShader != null) || ((pipelineStateChanges & PipelineStateChanges.GeometryShader) == PipelineStateChanges.GeometryShader))
			{
				CheckArray(_prevResourceState.GsConstantBuffers, newState.GsConstantBuffers, ResourceStateChanges.GsConstants, ref result, ref _ranges.GeometryShaderConstants);
				CheckArray(_prevResourceState.GsSrvs, newState.GsSrvs, ResourceStateChanges.GsResourceViews, ref result, ref _ranges.GeometryShaderResources);
				CheckArray(_prevResourceState.GsSamplers, newState.GsSamplers, ResourceStateChanges.GsSamplers, ref result, ref _ranges.GeometryShaderSamplers);
			}

			if ((newState.PipelineState.HullShader != null) || ((pipelineStateChanges & PipelineStateChanges.HullShader) == PipelineStateChanges.HullShader))
			{
				CheckArray(_prevResourceState.HsConstantBuffers, newState.HsConstantBuffers, ResourceStateChanges.HsConstants, ref result, ref _ranges.HullShaderConstants);
				CheckArray(_prevResourceState.HsSrvs, newState.HsSrvs, ResourceStateChanges.HsResourceViews, ref result, ref _ranges.HullShaderResources);
				CheckArray(_prevResourceState.HsSamplers, newState.HsSamplers, ResourceStateChanges.HsSamplers, ref result, ref _ranges.HullShaderSamplers);
			}

			if ((newState.PipelineState.DomainShader != null) || ((pipelineStateChanges & PipelineStateChanges.DomainShader) == PipelineStateChanges.DomainShader))
			{
				CheckArray(_prevResourceState.DsConstantBuffers, newState.DsConstantBuffers, ResourceStateChanges.DsConstants, ref result, ref _ranges.DomainShaderConstants);
				CheckArray(_prevResourceState.DsSrvs, newState.DsSrvs, ResourceStateChanges.DsResourceViews, ref result, ref _ranges.DomainShaderResources);
				CheckArray(_prevResourceState.DsSamplers, newState.DsSamplers, ResourceStateChanges.DsSamplers, ref result, ref _ranges.DomainShaderSamplers);
			}

			CheckArray(_prevResourceState.ReadWriteViews, newState.ReadWriteViews, ResourceStateChanges.Uavs, ref result, ref _ranges.Uavs);

			if ((newState.ComputeShader is null) && ((result & ResourceStateChanges.ComputeShader) != ResourceStateChanges.ComputeShader))
			{
				return _ranges;
			}

			CheckArray(_prevResourceState.CsConstantBuffers, newState.CsConstantBuffers, ResourceStateChanges.CsConstants, ref result, ref _ranges.ComputeShaderConstants);
			CheckArray(_prevResourceState.CsSrvs, newState.CsSrvs, ResourceStateChanges.CsResourceViews, ref result, ref _ranges.ComputeShaderResources);
			CheckArray(_prevResourceState.CsReadWriteViews, newState.CsReadWriteViews, ResourceStateChanges.CsUavs, ref result, ref _ranges.ComputeShaderUavs);
			CheckArray(_prevResourceState.CsSamplers, newState.CsSamplers, ResourceStateChanges.CsSamplers, ref result, ref _ranges.ComputeShaderSamplers);

			return _ranges;
		}

		/// <summary>
		/// Function to retrieve the changed pipeline state from the previous pipeline state applied by a draw call.
		/// </summary>
		/// <param name="newState">The new pipeline state to apply</param>
		/// <param name="blendFactor">The factor used to modulate the pixel shader, render target or both.</param>
		/// <param name="blendSampleMask">The mask used to define which samples get updated in the active render targets.</param>
		/// <param name="depthStencilReference">The depth/stencil reference value used when performing a depth/stencil test.</param>
		/// <returns>The changed individual states as a combined set of flags.</returns>
		public PipelineStateChanges GetPipelineStateChanges(GorgonPipelineState newState, in GorgonColor blendFactor, int blendSampleMask, int depthStencilReference)
        {
			PipelineStateChanges changes = PipelineStateChanges.None;

			changes |= _prevPipelineState.PrimitiveType == newState.PrimitiveType ? PipelineStateChanges.None : PipelineStateChanges.Topology;
			changes |= _prevPipelineState.D3DRasterState == newState.D3DRasterState ? PipelineStateChanges.None : PipelineStateChanges.RasterState;
			changes |= _prevPipelineState.D3DBlendState == newState.D3DBlendState ? PipelineStateChanges.None : PipelineStateChanges.BlendState;
			changes |= _prevPipelineState.D3DDepthStencilState == newState.D3DDepthStencilState ? PipelineStateChanges.None : PipelineStateChanges.DepthStencilState;
			changes |= _prevPipelineState.VertexShader == newState.VertexShader ? PipelineStateChanges.None : PipelineStateChanges.VertexShader;
			changes |= _prevPipelineState.PixelShader == newState.PixelShader ? PipelineStateChanges.None : PipelineStateChanges.PixelShader;
			changes |= _prevPipelineState.GeometryShader == newState.GeometryShader ? PipelineStateChanges.None : PipelineStateChanges.GeometryShader;
			changes |= _prevPipelineState.HullShader == newState.HullShader ? PipelineStateChanges.None : PipelineStateChanges.HullShader;
			changes |= _prevPipelineState.DomainShader == newState.DomainShader ? PipelineStateChanges.None : PipelineStateChanges.DomainShader;

			if (changes != PipelineStateChanges.None)
			{
				newState.CopyTo(_prevPipelineState);
			}

			if (!blendFactor.Equals(in _blendFactor))
			{
				_blendFactor = blendFactor;
				changes |= PipelineStateChanges.BlendFactor;
			}

			if (blendSampleMask != _blendSampleMask)
			{
				_blendSampleMask = blendSampleMask;
				changes |= PipelineStateChanges.BlendSampleMask;
			}

			if (depthStencilReference != _depthStencilReference)
			{
				_depthStencilReference = depthStencilReference;
				changes |= PipelineStateChanges.DepthStencilReference;
			}
			
			return changes;
		}

		/// <summary>
        /// Function to determine if there are any changes between the current render target and depth/stencil buffer and new views.
        /// </summary>
        /// <param name="targets">The previous depth stencil to evaluate.</param>
        /// <param name="depthStencil">The new depth stencil to evaluate.</param>
        /// <returns>A tuple indicating whether rtvs/dsv has been changed.</returns>
		public (bool RtvsUpdated, bool DsvUpdated) GetRtvDsvChanges(ReadOnlySpan<GorgonRenderTargetView> targets, GorgonDepthStencil2DView depthStencil)
		{			
			bool needsDepthStencilUpdate = depthStencil != DepthStencil;
			bool needsRtvUpdate = false;
			int length = targets.Length.Min(D3D11.OutputMergerStage.SimultaneousRenderTargetCount);

			for (int i = 0; i < D3D11.OutputMergerStage.SimultaneousRenderTargetCount; ++i)
			{
				ref GorgonRenderTargetView prevRtv = ref RenderTargets[i];

				if (i >= length)
				{
					prevRtv = null;
					continue;
				}

				GorgonRenderTargetView newRtv = targets[i];				

				if (newRtv == prevRtv)
				{
					continue;
				}

				prevRtv = newRtv;
				needsRtvUpdate = true;				
			}

			CheckRtvsForSrvHazards(RenderTargets, depthStencil);

			if (needsDepthStencilUpdate)
			{				
				DepthStencil = depthStencil;
			}

			return (needsRtvUpdate, needsDepthStencilUpdate);
		}

		/// <summary>
        /// Function to determine if the viewports have changed.
        /// </summary>
        /// <param name="viewports">The new viewports to evaluate.</param>
        /// <returns><b>true</b> if the view ports have changed, <b>false</b> if not.</returns>
		public bool GetViewportChange(ReadOnlySpan<DX.ViewportF> viewports)
		{
			int length = viewports.Length.Min(Viewports.Length);
			bool hasChanges = false;

			for (int i = 0; i < Viewports.Length; ++i)
			{
				ref DX.ViewportF viewport = ref Viewports[i];
				if (i >= length)
				{
					viewport = default;
					continue;
				}

				DX.ViewportF newViewport = viewports[i];
				
				if (viewports[i].Equals(ref viewport))
				{
					continue;
				}

				viewport = newViewport;
				hasChanges = true;
			}

			return hasChanges;
		}

		/// <summary>
		/// Function to determine if the scissor rectangles have changed.
		/// </summary>
		/// <param name="scissors">The new scissor rectangles to evaluate.</param>
		/// <returns><b>true</b> if the scissor rectangles have changed, <b>false</b> if not.</returns>
		public bool GetScissorRectChange(ReadOnlySpan<DX.Rectangle> scissors)
		{
			int length = scissors.Length.Min(Scissors.Length);
			bool hasChanges = false;

			for (int i = 0; i < Scissors.Length; ++i)
			{
				ref DX.Rectangle scissor = ref Scissors[i];

				if (i >= length)
				{
					scissor = default;
					continue;
				}

				DX.Rectangle newScissor = scissors[i];
				
				if (newScissor.Equals(ref scissor))
				{
					continue;
				}

				scissor = newScissor;
				hasChanges = true;
			}

			return hasChanges;
		}

		/// <summary>
		/// Function to reset all stored state.
		/// </summary>
		public void ResetState()
		{
			_prevPipelineState.Clear();

			_prevResourceState.VertexBuffers.Clear();
			_prevResourceState.StreamOutBindings.Clear();
			_prevResourceState.IndexBuffer = null;
			_prevResourceState.ComputeShader = null;
			_prevResourceState.ReadWriteViews.Clear();
			_prevResourceState.PsSamplers.Clear();
			_prevResourceState.VsSamplers.Clear();
			_prevResourceState.GsSamplers.Clear();
			_prevResourceState.DsSamplers.Clear();
			_prevResourceState.HsSamplers.Clear();
			_prevResourceState.CsSamplers.Clear();
			_prevResourceState.VsSrvs.Clear();
			_prevResourceState.PsSrvs.Clear();
			_prevResourceState.GsSrvs.Clear();
			_prevResourceState.DsSrvs.Clear();
			_prevResourceState.HsSrvs.Clear();
			_prevResourceState.CsSrvs.Clear();
			_prevResourceState.VsConstantBuffers.Clear();
			_prevResourceState.PsConstantBuffers.Clear();
			_prevResourceState.GsConstantBuffers.Clear();
			_prevResourceState.DsConstantBuffers.Clear();
			_prevResourceState.HsConstantBuffers.Clear();
			_prevResourceState.CsConstantBuffers.Clear();
			_prevResourceState.CsReadWriteViews.Clear();

			_ranges.Changes = ResourceStateChanges.None;
			_ranges.ComputeShaderConstants = (int.MaxValue, int.MinValue);
			_ranges.ComputeShaderResources = (int.MaxValue, int.MinValue);
			_ranges.ComputeShaderResources = (int.MaxValue, int.MinValue);
			_ranges.ComputeShaderUavs = (int.MaxValue, int.MinValue);
			_ranges.DomainShaderConstants = (int.MaxValue, int.MinValue);
			_ranges.DomainShaderResources = (int.MaxValue, int.MinValue);
			_ranges.DomainShaderSamplers = (int.MaxValue, int.MinValue);
			_ranges.GeometryShaderConstants = (int.MaxValue, int.MinValue);
			_ranges.GeometryShaderResources = (int.MaxValue, int.MinValue);
			_ranges.GeometryShaderSamplers = (int.MaxValue, int.MinValue);
			_ranges.HullShaderConstants = (int.MaxValue, int.MinValue);
			_ranges.HullShaderResources = (int.MaxValue, int.MinValue);
			_ranges.HullShaderSamplers = (int.MaxValue, int.MinValue);
			_ranges.PixelShaderConstants = (int.MaxValue, int.MinValue);
			_ranges.PixelShaderResources = (int.MaxValue, int.MinValue);
			_ranges.PixelShaderSamplers = (int.MaxValue, int.MinValue);
			_ranges.StreamOutBuffers = (int.MaxValue, int.MinValue);
			_ranges.VertexBuffers = (int.MaxValue, int.MinValue);
			_ranges.VertexShaderConstants = (int.MaxValue, int.MinValue);
			_ranges.VertexShaderResources = (int.MaxValue, int.MinValue);
			_ranges.VertexShaderSamplers = (int.MaxValue, int.MinValue);

			Array.Clear(RenderTargets, 0, RenderTargets.Length);
			DepthStencil = null;

			_depthStencilReference = 0;
			_blendFactor = GorgonColor.White;
			_blendSampleMask = int.MinValue;
		}
		#endregion

		#region Constructor.
		/// <summary>Initializes a new instance of the <see cref="StateEvaluator" /> class.</summary>
		/// <param name="graphics">The graphics interface that owns this evaluator.</param>
		public StateEvaluator(GorgonGraphics graphics)
		{
			_log = graphics.Log;
			_graphics = graphics;
		}
		#endregion
	}
}
