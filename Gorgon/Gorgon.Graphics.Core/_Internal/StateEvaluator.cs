
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: December 30, 2020 8:59:46 PM
// 

using System.Diagnostics;
using Gorgon.Collections;
using Gorgon.Diagnostics;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;
using DX = SharpDX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Functionality for evaluating state changes
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="StateEvaluator" /> class.</remarks>
/// <param name="graphics">The graphics interface that owns this evaluator.</param>
internal class StateEvaluator(GorgonGraphics graphics)
{
    // The previous stencil reference value.
    private int _stencilReference;
    // The previous blend sample mask.
    private int _blendSampleMask = int.MinValue;
    // The previous blend factor.
    private GorgonColor _blendFactor = GorgonColors.White;
    // The log for debug messages.
    private readonly IGorgonLog _log = graphics.Log;
    // The device context.
    private readonly GorgonGraphics _graphics = graphics;

    // The previously assigned pipeline state.
    private readonly GorgonPipelineState _prevPipelineState = new()
    {
        PrimitiveType = PrimitiveType.None
    };

    // The ranges of resource arrays that were updated.
    private readonly ResourceRanges _ranges = new();

    // The previously assigned resource state.
    private readonly D3DState _prevResourceState = D3DState.Create();

    // The viewports used to define the area to render into.
    public DX.ViewportF[] Viewports = new DX.ViewportF[16];

    // The rectangles used to define the clipping area.
    public GorgonRectangle[] Scissors = new GorgonRectangle[16];

    // The currently bound render targets.
    public GorgonRenderTargetView[] RenderTargets = new GorgonRenderTargetView[D3D11.OutputMergerStage.SimultaneousRenderTargetCount];

    // The currently bound depth/stencil buffer.
    public GorgonDepthStencil2DView DepthStencil;

    /// <summary>
    /// Function to compare the index buffer binding with stream out bindings.
    /// </summary>
    /// <param name="state">The state containing the index buffer to evaluate.</param>
    /// <param name="streamOut">The current list of stream out buffers.</param>
    private void CheckIndexBufferStreamOut(D3DState state, GorgonStreamOutBindings streamOut)
    {
        void ScanStreamOut(ReadOnlySpan<GorgonStreamOutBinding> soBindings)
        {
            for (int s = 0; s < soBindings.Length; ++s)
            {
                GorgonBufferCommon soBuffer = soBindings[s].Buffer;

                if (((soBuffer is null) || (soBuffer.BindFlags != D3D11.BindFlags.VertexBuffer))
                    || (soBuffer != state.IndexBuffer))
                {
                    continue;
                }

                if (GorgonGraphics.IsDebugEnabled)
                {
                    _log.Print($"WARNING: The index buffer '{state.IndexBuffer.Name}' is bound for input and stream out. It will be unbound from the input assembler.", LoggingLevel.Verbose);
                }

                state.IndexBuffer = null;
            }
        }

        if ((state.IndexBuffer is null) || ((state.IndexBuffer.BindFlags & D3D11.BindFlags.StreamOutput) != D3D11.BindFlags.StreamOutput))
        {
            return;
        }

        ReadOnlySpan<GorgonStreamOutBinding> newStreamOut = streamOut.GetDirtySpan();

        if (!newStreamOut.IsEmpty)
        {
            ScanStreamOut(newStreamOut);
        }

        ReadOnlySpan<GorgonStreamOutBinding> prevStreamOut = _prevResourceState.StreamOutBindings.GetDirtySpan();

        if (prevStreamOut.IsEmpty)
        {
            return;
        }

        ScanStreamOut(prevStreamOut);
    }

    /// <summary>
    /// Function to compare vertex buffer bindings with stream out bindings.
    /// </summary>
    /// <param name="vertexBuffers">The current list of vertex buffers.</param>
    /// <param name="streamOut">The current list of stream out buffers.</param>
    private void CheckVertexBufferStreamOut(GorgonVertexBufferBindings vertexBuffers, GorgonStreamOutBindings streamOut)
    {
        ReadOnlySpan<GorgonVertexBufferBinding> newVertexBuffers = vertexBuffers.GetDirtySpan();
        ReadOnlySpan<GorgonStreamOutBinding> streamOutBuffers = streamOut.GetDirtySpan();

        if ((newVertexBuffers.IsEmpty) || (streamOutBuffers.IsEmpty))
        {
            return;
        }

        (int start, _) = vertexBuffers.GetDirtyStartIndexAndCount();

        void ScanVertexBuffers(GorgonBufferCommon soBuffer, ReadOnlySpan<GorgonVertexBufferBinding> vBuffers)
        {
            for (int v = 0; v < vBuffers.Length; ++v)
            {
                GorgonBufferCommon vBuffer = vBuffers[v].VertexBuffer;

                if (((vBuffer is null) || ((vBuffer.BindFlags & D3D11.BindFlags.StreamOutput) != D3D11.BindFlags.StreamOutput))
                    || (soBuffer != vBuffer))
                {
                    continue;
                }

                if (GorgonGraphics.IsDebugEnabled)
                {
                    _log.Print($"WARNING: The vertex buffer '{vBuffer.Name}' is bound for input and stream out. It will be unbound from the vertex buffers.", LoggingLevel.Verbose);
                }

                vertexBuffers.MarkClean(start + v);
            }
        }

        for (int s = 0; s < streamOutBuffers.Length; ++s)
        {
            GorgonBufferCommon soBuffer = streamOutBuffers[s].Buffer;

            if ((soBuffer is null) || ((soBuffer.BindFlags & D3D11.BindFlags.VertexBuffer) != D3D11.BindFlags.VertexBuffer))
            {
                continue;
            }

            ScanVertexBuffers(soBuffer, newVertexBuffers);
        }

        streamOutBuffers = _prevResourceState.StreamOutBindings.GetDirtySpan();

        if (streamOutBuffers.IsEmpty)
        {
            return;
        }

        for (int s = 0; s < streamOutBuffers.Length; ++s)
        {
            GorgonBufferCommon soBuffer = streamOutBuffers[s].Buffer;

            if ((soBuffer is null) || ((soBuffer.BindFlags & D3D11.BindFlags.VertexBuffer) != D3D11.BindFlags.VertexBuffer))
            {
                continue;
            }

            ScanVertexBuffers(soBuffer, newVertexBuffers);
        }
    }

    /// <summary>
    /// Function to compare shader resource views with render target views.
    /// </summary>
    /// <param name="srvs">The shader resource views to compare.</param>
    private void CheckSrvsRtvs(GorgonArray<GorgonShaderResourceView> srvs)
    {
        ReadOnlySpan<GorgonShaderResourceView> dirtySrvs = srvs.GetDirtySpan();

        if (dirtySrvs.IsEmpty)
        {
            return;
        }

        (int start, int count) = srvs.GetDirtyStartIndexAndCount();
        Range dirtyRange = start..(start + count);

        if ((_graphics.DepthStencilView is not null) && ((_graphics.DepthStencilView.Binding & TextureBinding.ShaderResource) == TextureBinding.ShaderResource))
        {
            for (int s = 0; s < dirtySrvs.Length; ++s)
            {
                GorgonGraphicsResource srvResource = dirtySrvs[s]?.Resource;

                if ((srvResource is null) || ((srvResource.BindFlags & D3D11.BindFlags.DepthStencil) != D3D11.BindFlags.DepthStencil)
                    || (srvResource != _graphics.DepthStencilView.Resource))
                {
                    continue;
                }

                if (GorgonGraphics.IsDebugEnabled)
                {
                    _log.Print($"WARNING: The shader resource '{srvResource.Name}' is bound for input and as a depth/stencil. It will be unbound from the shader resources.", LoggingLevel.Verbose);
                }

                srvs[start + s] = null;
            }

            srvs.MarkDirty(dirtyRange);
        }

        for (int r = 0; r < _graphics.RenderTargets.Count; ++r)
        {
            GorgonGraphicsResource rtvResource = _graphics.RenderTargets[r]?.Resource;

            if ((rtvResource is null) || ((rtvResource.BindFlags & D3D11.BindFlags.ShaderResource) != D3D11.BindFlags.ShaderResource))
            {
                continue;
            }

            for (int s = 0; s < dirtySrvs.Length; ++s)
            {
                GorgonGraphicsResource srvResource = dirtySrvs[s]?.Resource;

                if ((srvResource is null)
                    || ((srvResource.BindFlags & D3D11.BindFlags.RenderTarget) != D3D11.BindFlags.RenderTarget)
                    || (srvResource != rtvResource))
                {
                    continue;
                }

                if (GorgonGraphics.IsDebugEnabled)
                {
                    _log.Print($"WARNING: The shader resource '{srvResource.Name}' is bound for input and as a render target. It will be unbound from the shader resources.", LoggingLevel.Verbose);
                }

                srvs[start + s] = null;
            }

            srvs.MarkDirty(dirtyRange);
        }
    }

    /// <summary>
    /// Function to compare shader resource views with unordered access views.
    /// </summary>
    /// <param name="srvs">The shader resource views to compare.</param>
    /// <param name="uavs">The unordered access views to compare.</param>
    private void CheckSrvsUavs(GorgonArray<GorgonShaderResourceView> srvs, GorgonArray<GorgonReadWriteViewBinding> uavs)
    {
        ReadOnlySpan<GorgonReadWriteViewBinding> dirtyUavs = uavs.GetDirtySpan();
        ReadOnlySpan<GorgonShaderResourceView> dirtySrvs = srvs.GetDirtySpan();

        if ((dirtySrvs.IsEmpty) || (dirtyUavs.IsEmpty))
        {
            return;
        }

        (int start, int count) = srvs.GetDirtyStartIndexAndCount();
        Range dirtyRange = start..(start + count);

        for (int u = 0; u < dirtyUavs.Length; ++u)
        {
            GorgonGraphicsResource uavResource = dirtyUavs[u].ReadWriteView?.Resource;

            if ((uavResource is null) || ((uavResource.BindFlags & D3D11.BindFlags.ShaderResource) != D3D11.BindFlags.ShaderResource))
            {
                continue;
            }

            for (int s = 0; s < dirtySrvs.Length; ++s)
            {
                GorgonGraphicsResource srvResource = dirtySrvs[s]?.Resource;

                if ((srvResource is null)
                    || ((srvResource.BindFlags & D3D11.BindFlags.UnorderedAccess) != D3D11.BindFlags.UnorderedAccess)
                    || (srvResource != uavResource))
                {
                    continue;
                }

                if (GorgonGraphics.IsDebugEnabled)
                {
                    _log.Print($"WARNING: The shader resource '{srvResource.Name}' is bound for input and as an unordered access view. It will be unbound from the shader resources.", LoggingLevel.Verbose);
                }

                srvs[start + s] = null;
            }

            // Mark the same item(s) as dirty again.
            srvs.MarkDirty(dirtyRange);
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
        ReadOnlySpan<GorgonShaderResourceView> dirtySrvs = srvs.GetDirtySpan();

        if (dirtySrvs.IsEmpty)
        {
            return;
        }

        GorgonGraphicsResource depthResource = depth?.Resource;

        if ((depthResource is null) || ((depthResource.BindFlags & D3D11.BindFlags.ShaderResource) != D3D11.BindFlags.ShaderResource))
        {
            return;
        }

        (int start, int count) = srvs.GetDirtyStartIndexAndCount();
        Range dirtyRange = start..(start + count);

        for (int s = 0; s < dirtySrvs.Length; ++s)
        {
            GorgonGraphicsResource srv = dirtySrvs[s]?.Resource;

            if ((srv is null) || ((srv.BindFlags & D3D11.BindFlags.DepthStencil) == D3D11.BindFlags.DepthStencil) || (srv != depthResource))
            {
                continue;
            }

            if (GorgonGraphics.IsDebugEnabled)
            {
                _log.Print($"WARNING: The depth buffer resource '{depth.Resource.Name}' is bound as an input. Unbinding...", LoggingLevel.Verbose);
            }

            int arrayIndex = start + s;
            srvs[arrayIndex] = null;
            shaderStage.SetShaderResource(arrayIndex, null);
        }

        // Mark the same item(s) as dirty.
        srvs.MarkDirty(dirtyRange);
    }

    /// <summary>
    /// Function to check the new render target views against any bound shader resource views.
    /// </summary>
    /// <param name="srvs">The shader resource views to evaluate.</param>
    /// <param name="rtv">The new render target view to evaluate.</param>
    /// <param name="shaderStage">The current shader stage being evaluated.</param>
    private void CheckRtvSrvsHazards(GorgonArray<GorgonShaderResourceView> srvs, GorgonRenderTargetView rtv, D3D11.CommonShaderStage shaderStage)
    {
        ReadOnlySpan<GorgonShaderResourceView> dirtySrvs = srvs.GetDirtySpan();

        if (dirtySrvs.IsEmpty)
        {
            return;
        }

        GorgonGraphicsResource rtvResource = rtv?.Resource;

        if ((rtvResource is null) || ((rtvResource.BindFlags & D3D11.BindFlags.ShaderResource) != D3D11.BindFlags.ShaderResource))
        {
            return;
        }

        (int start, int count) = srvs.GetDirtyStartIndexAndCount();
        Range dirtyRange = start..(start + count);

        for (int s = 0; s < dirtySrvs.Length; ++s)
        {
            GorgonGraphicsResource srvResource = dirtySrvs[s]?.Resource;

            if ((srvResource is null) || ((srvResource.BindFlags & D3D11.BindFlags.RenderTarget) != D3D11.BindFlags.RenderTarget) || (srvResource != rtvResource))
            {
                continue;
            }

            int arrayIndex = start + s;
            srvs[arrayIndex] = null;
            shaderStage.SetShaderResource(arrayIndex, null);
        }

        // Mark the same item(s) as dirty.
        srvs.MarkDirty(dirtyRange);
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
    private void CheckArray<T>(GorgonArray<T> left, GorgonArray<T> right, ResourceStateChanges change, ref ResourceStateChanges changes, out (int Start, int Count) range)
        where T : IEquatable<T>
    {
        Debug.Assert(left.Length == right.Length, $"Both arrays must be of the same length. Left: {left.Length}, Right: {right.Length}");

        (int leftStart, int leftCount) = left.GetDirtyStartIndexAndCount();
        (int rightStart, int rightCount) = right.GetDirtyStartIndexAndCount();

        int leftEnd = leftStart + leftCount;
        int rightEnd = rightStart + rightCount;

        int start = leftStart.Min(rightStart);
        int end = leftEnd.Max(rightEnd);

        for (int i = start; i < end; ++i)
        {
            left[i] = right[i];
        }

        if (!left.IsDirty)
        {
            range = (0, 0);
            return;
        }

        changes |= change;
        range = new(start, end - start);
    }

    /// <summary>
    /// Function to fix the render target view resource sharing hazards prior to setting the rtv.
    /// </summary>
    /// <param name="rtViews">The render target views being assigned.</param>
    /// <param name="depth">The depth stencil being assigned.</param>
    private void CheckRtvsForSrvHazards(ReadOnlySpan<GorgonRenderTargetView> rtViews, GorgonDepthStencil2DView depth)
    {
        if (depth is not null)
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
        CheckArray(_prevResourceState.VertexBuffers, newState.VertexBuffers, ResourceStateChanges.VertexBuffers, ref result, out _ranges.VertexBuffers);
        CheckArray(_prevResourceState.StreamOutBindings, newState.StreamOutBindings, ResourceStateChanges.StreamOutBuffers, ref result, out _ranges.StreamOutBuffers);

        if ((newState.PipelineState.VertexShader is not null) || ((pipelineStateChanges & PipelineStateChanges.VertexShader) == PipelineStateChanges.VertexShader))
        {
            CheckArray(_prevResourceState.VsConstantBuffers, newState.VsConstantBuffers, ResourceStateChanges.VsConstants, ref result, out _ranges.VertexShaderConstants);
            CheckArray(_prevResourceState.VsSrvs, newState.VsSrvs, ResourceStateChanges.VsResourceViews, ref result, out _ranges.VertexShaderResources);
            CheckArray(_prevResourceState.VsSamplers, newState.VsSamplers, ResourceStateChanges.VsSamplers, ref result, out _ranges.VertexShaderSamplers);
        }

        if ((newState.PipelineState.PixelShader is not null) || ((pipelineStateChanges & PipelineStateChanges.PixelShader) == PipelineStateChanges.PixelShader))
        {
            CheckArray(_prevResourceState.PsConstantBuffers, newState.PsConstantBuffers, ResourceStateChanges.PsConstants, ref result, out _ranges.PixelShaderConstants);
            CheckArray(_prevResourceState.PsSrvs, newState.PsSrvs, ResourceStateChanges.PsResourceViews, ref result, out _ranges.PixelShaderResources);
            CheckArray(_prevResourceState.PsSamplers, newState.PsSamplers, ResourceStateChanges.PsSamplers, ref result, out _ranges.PixelShaderSamplers);
        }

        if ((newState.PipelineState.GeometryShader is not null) || ((pipelineStateChanges & PipelineStateChanges.GeometryShader) == PipelineStateChanges.GeometryShader))
        {
            CheckArray(_prevResourceState.GsConstantBuffers, newState.GsConstantBuffers, ResourceStateChanges.GsConstants, ref result, out _ranges.GeometryShaderConstants);
            CheckArray(_prevResourceState.GsSrvs, newState.GsSrvs, ResourceStateChanges.GsResourceViews, ref result, out _ranges.GeometryShaderResources);
            CheckArray(_prevResourceState.GsSamplers, newState.GsSamplers, ResourceStateChanges.GsSamplers, ref result, out _ranges.GeometryShaderSamplers);
        }

        if ((newState.PipelineState.HullShader is not null) || ((pipelineStateChanges & PipelineStateChanges.HullShader) == PipelineStateChanges.HullShader))
        {
            CheckArray(_prevResourceState.HsConstantBuffers, newState.HsConstantBuffers, ResourceStateChanges.HsConstants, ref result, out _ranges.HullShaderConstants);
            CheckArray(_prevResourceState.HsSrvs, newState.HsSrvs, ResourceStateChanges.HsResourceViews, ref result, out _ranges.HullShaderResources);
            CheckArray(_prevResourceState.HsSamplers, newState.HsSamplers, ResourceStateChanges.HsSamplers, ref result, out _ranges.HullShaderSamplers);
        }

        if ((newState.PipelineState.DomainShader is not null) || ((pipelineStateChanges & PipelineStateChanges.DomainShader) == PipelineStateChanges.DomainShader))
        {
            CheckArray(_prevResourceState.DsConstantBuffers, newState.DsConstantBuffers, ResourceStateChanges.DsConstants, ref result, out _ranges.DomainShaderConstants);
            CheckArray(_prevResourceState.DsSrvs, newState.DsSrvs, ResourceStateChanges.DsResourceViews, ref result, out _ranges.DomainShaderResources);
            CheckArray(_prevResourceState.DsSamplers, newState.DsSamplers, ResourceStateChanges.DsSamplers, ref result, out _ranges.DomainShaderSamplers);
        }

        CheckArray(_prevResourceState.ReadWriteViews, newState.ReadWriteViews, ResourceStateChanges.Uavs, ref result, out _ranges.Uavs);

        if ((newState.ComputeShader is null) && ((result & ResourceStateChanges.ComputeShader) != ResourceStateChanges.ComputeShader))
        {
            return _ranges;
        }

        CheckArray(_prevResourceState.CsConstantBuffers, newState.CsConstantBuffers, ResourceStateChanges.CsConstants, ref result, out _ranges.ComputeShaderConstants);
        CheckArray(_prevResourceState.CsSrvs, newState.CsSrvs, ResourceStateChanges.CsResourceViews, ref result, out _ranges.ComputeShaderResources);
        CheckArray(_prevResourceState.CsReadWriteViews, newState.CsReadWriteViews, ResourceStateChanges.CsUavs, ref result, out _ranges.ComputeShaderUavs);
        CheckArray(_prevResourceState.CsSamplers, newState.CsSamplers, ResourceStateChanges.CsSamplers, ref result, out _ranges.ComputeShaderSamplers);

        return _ranges;
    }

    /// <summary>
    /// Function to retrieve the changed pipeline state from the previous pipeline state applied by a draw call.
    /// </summary>
    /// <param name="newState">The new pipeline state to apply</param>
    /// <param name="blendFactor">The factor used to modulate the pixel shader, render target or both.</param>
    /// <param name="blendSampleMask">The mask used to define which samples get updated in the active render targets.</param>
    /// <param name="stencilReference">The stencil reference value used when performing a stencil test.</param>
    /// <returns>The changed individual states as a combined set of flags.</returns>
    public PipelineStateChanges GetPipelineStateChanges(GorgonPipelineState newState, GorgonColor blendFactor, int blendSampleMask, int stencilReference)
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

        if (!blendFactor.Equals(_blendFactor))
        {
            _blendFactor = blendFactor;
            changes |= PipelineStateChanges.BlendFactor;
        }

        if (blendSampleMask != _blendSampleMask)
        {
            _blendSampleMask = blendSampleMask;
            changes |= PipelineStateChanges.BlendSampleMask;
        }

        if (stencilReference != _stencilReference)
        {
            _stencilReference = stencilReference;
            changes |= PipelineStateChanges.StencilReference;
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
    public bool GetScissorRectChange(ReadOnlySpan<GorgonRectangle> scissors)
    {
        int length = scissors.Length.Min(Scissors.Length);
        bool hasChanges = false;

        for (int i = 0; i < Scissors.Length; ++i)
        {
            ref GorgonRectangle scissor = ref Scissors[i];

            if (i >= length)
            {
                scissor = default;
                continue;
            }

            GorgonRectangle newScissor = scissors[i];

            if (newScissor.Equals(scissor))
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

        _prevResourceState.Reset();

        _ranges.Reset();

        Array.Clear(RenderTargets, 0, RenderTargets.Length);
        DepthStencil = null;

        _stencilReference = 0;
        _blendFactor = GorgonColors.White;
        _blendSampleMask = int.MinValue;
    }
}
