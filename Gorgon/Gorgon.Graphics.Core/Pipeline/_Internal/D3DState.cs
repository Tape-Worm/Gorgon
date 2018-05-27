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
using D3D = SharpDX.Direct3D;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Defines what has been changed on a draw call.
    /// </summary>
    [Flags]
    enum DrawCallChanges
        : ulong
    {
        /// <summary>
        /// No changes.
        /// </summary>
        None = 0,
        /// <summary>
        /// The list of vertex buffers used by a draw call.
        /// </summary>
        VertexBuffers = 0x1,
        /// <summary>
        /// The input layout for the vertex buffers.
        /// </summary>
        InputLayout = 0x2,
        /// <summary>
        /// The index buffer used by a draw call.
        /// </summary>
        IndexBuffer = 0x4,
        /// <summary>
        /// The primtive topology.
        /// </summary>
        Topology = 0x8,
        /// <summary>
        /// The vertex shader was changed.
        /// </summary>
        VertexShader = 0x10,
        /// <summary>
        /// The pixel shader was changed.
        /// </summary>
        PixelShader = 0x20,
        /// <summary>
        /// The geometry shader was changed.
        /// </summary>
        GeometryShader = 0x40,
        /// <summary>
        /// The domain shader was changed.
        /// </summary>
        DomainShader = 0x80,
        /// <summary>
        /// The hull shader was changed.
        /// </summary>
        HullShader = 0x100,
        /// <summary>
        /// The compute shader was changed.
        /// </summary>
        ComputeShader = 0x200,
        /// <summary>
        /// The rasterizer state was modified.
        /// </summary>
        RasterState = 0x400,
        /// <summary>
        /// The depth/stencil state was modified.
        /// </summary>
        DepthStencilState = 0x800,
        /// <summary>
        /// The blend state was modified.
        /// </summary>
        BlendState = 0x1000,
        /// <summary>
        /// Scissor rectangles were modified.
        /// </summary>
        Scissors = 0x2000,
        /// <summary>
        /// Sampler state has changed, combined with one of the shader mask values.
        /// </summary>
        Samplers = 0x0100_0000,
        /// <summary>
        /// Constant buffers have changed, combined with one of the shader mask values.
        /// </summary>
        Constants = 0x0200_0000,
        /// <summary>
        /// Resource views have changed, combined with one of the shader mask values.
        /// </summary>
        ResourceViews = 0x0400_0000,
        /// <summary>
        /// Mask for pixel shader states.
        /// </summary>
        PixelShaderMask = 0x100_0000_0000_0000,
        /// <summary>
        /// Mask for vertex shader states.
        /// </summary>
        VertexShaderMask = 0x200_0000_0000_0000,
        /// <summary>
        /// Mask for geometry shader states.
        /// </summary>
        GeometryShaderMask = 0x400_0000_0000_0000,
        /// <summary>
        /// Mask for hull shader states.
        /// </summary>
        HullShaderMask = 0x800_0000_0000_0000,
        /// <summary>
        /// Mask for domain shader states.
        /// </summary>
        DomainShaderMask = 0x1000_0000_0000_0000,
        /// <summary>
        /// Mask for compute shader states.
        /// </summary>
        ComputeShaderMask = 0x2000_0000_0000_0000,
        /// <summary>
        /// All pipeline states.
        /// </summary>
        AllPipelineState = RasterState 
                           | DepthStencilState
                           | BlendState
                           | PixelShader
                           | VertexShader
                           | GeometryShader
                           | DomainShader
                           | HullShader
                           | ComputeShader
                           | Scissors,
        /// <summary>
        /// Everything changed.
        /// </summary>
        All = VertexBuffers 
              | InputLayout 
              | IndexBuffer 
              | Topology 
              | (PixelShaderMask | Samplers) 
              | (VertexShaderMask | Samplers) 
              | (GeometryShaderMask | Samplers) 
              | (HullShaderMask | Samplers) 
              | (DomainShaderMask | Samplers) 
              | (ComputeShaderMask | Samplers)
              | (PixelShaderMask | Constants) 
              | (VertexShaderMask | Constants) 
              | (GeometryShaderMask | Constants) 
              | (HullShaderMask | Constants) 
              | (DomainShaderMask | Constants) 
              | (ComputeShaderMask | Constants)
              | (PixelShaderMask | ResourceViews) 
              | (VertexShaderMask | ResourceViews) 
              | (GeometryShaderMask | ResourceViews) 
              | (HullShaderMask | ResourceViews) 
              | (DomainShaderMask | ResourceViews) 
              | (ComputeShaderMask | ResourceViews)
              | AllPipelineState
    }

    /// <summary>
    /// Defines a means to record and compare state.
    /// </summary>
    class D3DState
    {
        /// <summary>
        /// Property to return the pixel shader sampler states.
        /// </summary>
        public GorgonSamplerStates PsSamplers
        {
            get; 
            set;
        }

        /// <summary>
        /// Property to return the vertex shader sampler states.
        /// </summary>
        public GorgonSamplerStates VsSamplers
        {
            get; 
            set;
        }

        /// <summary>
        /// Property to return the geometry shader sampler states.
        /// </summary>
        public GorgonSamplerStates GsSamplers
        {
            get; 
            set;
        }

        /// <summary>
        /// Property to return the domain shader sampler states.
        /// </summary>
        public GorgonSamplerStates DsSamplers
        {
            get; 
            set;
        }

        /// <summary>
        /// Property to return the hull shader sampler states.
        /// </summary>
        public GorgonSamplerStates HsSamplers
        {
            get; 
            set;
        }

        /// <summary>
        /// Property to return the compute shader sampler states.
        /// </summary>
        public GorgonSamplerStates CsSamplers
        {
            get; 
            set;
        }

        /// <summary>
        /// Property to set or return constant buffers for a pixel shader.
        /// </summary>
        public GorgonConstantBuffers PsConstantBuffers
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return constant buffers for a vertex shader.
        /// </summary>
        public GorgonConstantBuffers VsConstantBuffers
        {
            get;
            set;
        }
        
        /// <summary>
        /// Property to set or return constant buffers for a geometry shader.
        /// </summary>
        public GorgonConstantBuffers GsConstantBuffers
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return constant buffers for a hull shader.
        /// </summary>
        public GorgonConstantBuffers HsConstantBuffers
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return constant buffers for a domain shader.
        /// </summary>
        public GorgonConstantBuffers DsConstantBuffers
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return constant buffers for a compute shader.
        /// </summary>
        public GorgonConstantBuffers CsConstantBuffers
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return resource views for a pixel shader.
        /// </summary>
        public GorgonShaderResourceViews PsSrvs
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return resource views for a vertex shader.
        /// </summary>
        public GorgonShaderResourceViews VsSrvs
        {
            get;
            set;
        }
        
        /// <summary>
        /// Property to set or return resource views for a geometry shader.
        /// </summary>
        public GorgonShaderResourceViews GsSrvs
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return resource views for a hull shader.
        /// </summary>
        public GorgonShaderResourceViews HsSrvs
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return resource views for a domain shader.
        /// </summary>
        public GorgonShaderResourceViews DsSrvs
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return resource views for a compute shader.
        /// </summary>
        public GorgonShaderResourceViews CsSrvs
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the current list of vertex buffers.
        /// </summary>
        public GorgonVertexBufferBindings VertexBuffers
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the index buffer.
        /// </summary>
        public GorgonIndexBuffer IndexBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the current pipeline state.
        /// </summary>
        public GorgonPipelineState PipelineState
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the current input layout.
        /// </summary>
        public GorgonInputLayout InputLayout => VertexBuffers?.InputLayout;
    }
}
