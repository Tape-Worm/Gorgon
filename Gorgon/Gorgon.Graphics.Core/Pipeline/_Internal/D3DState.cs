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
// Created: May 23, 2018 10:33:36 PM
// 
#endregion

using System;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Defines what resource state has been changed on a draw call.
    /// </summary>
    [Flags]
    internal enum ResourceStateChanges
    {
        /// <summary>
        /// No changes.
        /// </summary>
        None = 0,
        /// <summary>
        /// The input layout for the vertex buffers.
        /// </summary>
        InputLayout = 0x1,
        /// <summary>
        /// The list of vertex buffers used by a draw call.
        /// </summary>
        VertexBuffers = 0x2,
        /// <summary>
        /// The index buffer used by a draw call.
        /// </summary>
        IndexBuffer = 0x4,
        /// <summary>
        /// Stream out buffers were modified.
        /// </summary>
        StreamOutBuffers = 0x8,
        /// <summary>
        /// General unordered access views were modified.
        /// </summary>
        Uavs = 0x10,
        /// <summary>
        /// Sampler state has changed.
        /// </summary>
        PsSamplers = 0x20,
        /// <summary>
        /// Constant buffers have changed.
        /// </summary>
        PsConstants = 0x40,
        /// <summary>
        /// Resource views have changed.
        /// </summary>
        PsResourceViews = 0x80,
        /// <summary>
        /// Sampler state has changed.
        /// </summary>
        VsSamplers = 0x100,
        /// <summary>
        /// Constant buffers have changed.
        /// </summary>
        VsConstants = 0x200,
        /// <summary>
        /// Resource views have changed.
        /// </summary>
        VsResourceViews = 0x400,
        /// <summary>
        /// Sampler state has changed.
        /// </summary>
        GsSamplers = 0x800,
        /// <summary>
        /// Constant buffers have changed.
        /// </summary>
        GsConstants = 0x1_000,
        /// <summary>
        /// Resource views have changed.
        /// </summary>
        GsResourceViews = 0x2_000,
        /// <summary>
        /// Sampler state has changed.
        /// </summary>
        DsSamplers = 0x4_000,
        /// <summary>
        /// Constant buffers have changed.
        /// </summary>
        DsConstants = 0x8_000,
        /// <summary>
        /// Resource views have changed.
        /// </summary>
        DsResourceViews = 0x10_000,
        /// <summary>
        /// Sampler state has changed.
        /// </summary>
        HsSamplers = 0x20_000,
        /// <summary>
        /// Constant buffers have changed.
        /// </summary>
        HsConstants = 0x40_000,
        /// <summary>
        /// Resource views have changed.
        /// </summary>
        HsResourceViews = 0x80_000,
        /// <summary>
        /// Sampler state has changed.
        /// </summary>
        CsSamplers = 0x100_000,
        /// <summary>
        /// Constant buffers have changed.
        /// </summary>
        CsConstants = 0x200_000,
        /// <summary>
        /// Resource views have changed.
        /// </summary>
        CsResourceViews = 0x400_000,
        /// <summary>
        /// Unordered access views have changed.
        /// </summary>
        CsUavs = 0x800_000,
        /// <summary>
        /// The compute shader was changed.
        /// </summary>
        ComputeShader = 0x1_000_000,
        /// <summary>
        /// Everything changed.
        /// </summary>
        All = VertexBuffers
              | InputLayout
              | IndexBuffer
              | Uavs
              | PsSamplers
              | VsSamplers
              | GsSamplers
              | DsSamplers
              | HsSamplers
              | CsSamplers
              | PsConstants
              | VsConstants
              | GsConstants
              | DsConstants
              | HsConstants
              | CsConstants
              | PsResourceViews
              | VsResourceViews
              | GsResourceViews
              | DsResourceViews
              | HsResourceViews
              | CsResourceViews
              | CsUavs
              | ComputeShader
    }

    /// <summary>
    /// Defines what pipeline state has been changed on a draw call.
    /// </summary>
    [Flags]
    internal enum PipelineStateChanges
    {
        /// <summary>
        /// No changes.
        /// </summary>
        None = 0,
        /// <summary>
        /// The primtive topology.
        /// </summary>
        Topology = 0x1,
        /// <summary>
        /// The vertex shader was changed.
        /// </summary>
        VertexShader = 0x2,
        /// <summary>
        /// The pixel shader was changed.
        /// </summary>
        PixelShader = 0x4,
        /// <summary>
        /// The geometry shader was changed.
        /// </summary>
        GeometryShader = 0x8,
        /// <summary>
        /// The domain shader was changed.
        /// </summary>
        DomainShader = 0x10,
        /// <summary>
        /// The hull shader was changed.
        /// </summary>
        HullShader = 0x20,
        /// <summary>
        /// The rasterizer state was modified.
        /// </summary>
        RasterState = 0x40,
        /// <summary>
        /// The depth/stencil state was modified.
        /// </summary>
        DepthStencilState = 0x80,
        /// <summary>
        /// The blend state was modified.
        /// </summary>
        BlendState = 0x100,
        /// <summary>
        /// The blend factor color has changed.
        /// </summary>
        BlendFactor = 0x200,
        /// <summary>
        /// The blending sample mask has changed.
        /// </summary>
        BlendSampleMask = 0x400,
        /// <summary>
        /// The stencil reference value has changed.
        /// </summary>
        StencilReference = 0x800,
        /// <summary>
        /// All pipeline states without blend flags.
        /// </summary>
        AllWithoutBlendFlags = Topology
            | RasterState
            | DepthStencilState
            | BlendState
            | PixelShader
            | VertexShader
            | GeometryShader
            | DomainShader
            | HullShader,
        /// <summary>
        /// All pipeline states.
        /// </summary>
        All = Topology
            | RasterState
            | DepthStencilState
            | BlendState
            | PixelShader
            | VertexShader
            | GeometryShader
            | DomainShader
            | HullShader
            | BlendFactor
            | BlendSampleMask
            | StencilReference
    }

    /// <summary>
    /// Defines a means to record and compare state.
    /// </summary>
    class D3DState
    {
        /// <summary>
        /// Property to return the stream out bindings.
        /// </summary>
        public GorgonStreamOutBindings StreamOutBindings;

        /// <summary>
        /// Property to return the unordered access views.
        /// </summary>
        public GorgonReadWriteViewBindings ReadWriteViews;

        /// <summary>
        /// Property to return the compute shader unordered access views.
        /// </summary>
        public GorgonReadWriteViewBindings CsReadWriteViews;

        /// <summary>
        /// Property to return the pixel shader sampler states.
        /// </summary>
        public GorgonSamplerStates PsSamplers;

        /// <summary>
        /// Property to return the vertex shader sampler states.
        /// </summary>
        public GorgonSamplerStates VsSamplers;

        /// <summary>
        /// Property to return the geometry shader sampler states.
        /// </summary>
        public GorgonSamplerStates GsSamplers;

        /// <summary>
        /// Property to return the domain shader sampler states.
        /// </summary>
        public GorgonSamplerStates DsSamplers;

        /// <summary>
        /// Property to return the hull shader sampler states.
        /// </summary>
        public GorgonSamplerStates HsSamplers;

        /// <summary>
        /// Property to return the compute shader sampler states.
        /// </summary>
        public GorgonSamplerStates CsSamplers;

        /// <summary>
        /// Property to set or return constant buffers for a pixel shader.
        /// </summary>
        public GorgonConstantBuffers PsConstantBuffers;

        /// <summary>
        /// Property to set or return constant buffers for a vertex shader.
        /// </summary>
        public GorgonConstantBuffers VsConstantBuffers;

        /// <summary>
        /// Property to set or return constant buffers for a geometry shader.
        /// </summary>
        public GorgonConstantBuffers GsConstantBuffers;

        /// <summary>
        /// Property to set or return constant buffers for a hull shader.
        /// </summary>
        public GorgonConstantBuffers HsConstantBuffers;

        /// <summary>
        /// Property to set or return constant buffers for a domain shader.
        /// </summary>
        public GorgonConstantBuffers DsConstantBuffers;

        /// <summary>
        /// Property to set or return constant buffers for a compute shader.
        /// </summary>
        public GorgonConstantBuffers CsConstantBuffers;

        /// <summary>
        /// Property to set or return resource views for a pixel shader.
        /// </summary>
        public GorgonShaderResourceViews PsSrvs;

        /// <summary>
        /// Property to set or return resource views for a vertex shader.
        /// </summary>
        public GorgonShaderResourceViews VsSrvs;

        /// <summary>
        /// Property to set or return resource views for a geometry shader.
        /// </summary>
        public GorgonShaderResourceViews GsSrvs;

        /// <summary>
        /// Property to set or return resource views for a hull shader.
        /// </summary>
        public GorgonShaderResourceViews HsSrvs;

        /// <summary>
        /// Property to set or return resource views for a domain shader.
        /// </summary>
        public GorgonShaderResourceViews DsSrvs;

        /// <summary>
        /// Property to set or return resource views for a compute shader.
        /// </summary>
        public GorgonShaderResourceViews CsSrvs;

        /// <summary>
        /// Property to return the current list of vertex buffers.
        /// </summary>
        public GorgonVertexBufferBindings VertexBuffers;

        /// <summary>
        /// Property to set or return the index buffer.
        /// </summary>
        public GorgonIndexBuffer IndexBuffer;

        /// <summary>
        /// Property to set or return the current pipeline state.
        /// </summary>
        public GorgonPipelineState PipelineState;

        /// <summary>
        /// Property to set or return the current compute shader.
        /// </summary>
        public GorgonComputeShader ComputeShader;

        /// <summary>
        /// Property to return the current input layout.
        /// </summary>
        public GorgonInputLayout InputLayout => VertexBuffers?.InputLayout;
    }
}
