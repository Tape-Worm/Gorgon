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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;


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
        /// The rasterizer state was modified.
        /// </summary>
        RasterState = 0x40,
        /// <summary>
        /// The pixel shader was changed.
        /// </summary>
        PixelShader = 0x80,
        /// <summary>
        /// Sampler state has changed, combined with <see cref="PixelShaderMask"/>.
        /// </summary>
        Samplers = 0x0100_0000,
        /// <summary>
        /// Mask for pixel shader states.
        /// </summary>
        PixelShaderMask = 0x1FF_0000_0000_0000,
        /// <summary>
        /// Mask for vertex shader states.
        /// </summary>
        VertexShaderMask = 0x2FF_0000_0000_0000,
        /// <summary>
        /// Mask for geometry shader states.
        /// </summary>
        GeometryShaderMask = 0x3FF_0000_0000_0000,
        /// <summary>
        /// Mask for hull shader states.
        /// </summary>
        HullShaderMask = 0x4FF_0000_0000_0000,
        /// <summary>
        /// Mask for domain shader states.
        /// </summary>
        DomainShaderMask = 0x5FF_0000_0000_0000,
        /// <summary>
        /// Mask for compute shader states.
        /// </summary>
        ComputeShaderMask = 0x6FF_0000_0000_0000,
        /// <summary>
        /// All pipeline states.
        /// </summary>
        AllPipelineState = RasterState 
                           | PixelShader  
                           | (PixelShaderMask | Samplers) 
                           | (VertexShaderMask | Samplers) 
                           | (GeometryShaderMask | Samplers) 
                           | (HullShaderMask | Samplers) 
                           | (DomainShaderMask | Samplers) 
                           | (ComputeShaderMask | Samplers),
        /// <summary>
        /// Everything changed.
        /// </summary>
        All = VertexBuffers 
              | InputLayout 
              | IndexBuffer 
              | Topology 
              | AllPipelineState
    }

    /// <summary>
    /// Defines a means to record and compare state.
    /// </summary>
    class D3DState
    {
        #region Properties.
        /// <summary>
        /// Property to return the current list of vertex buffers.
        /// </summary>
        public GorgonVertexBufferBindings VertexBuffers
        {
            get;
            set;
        } = new GorgonVertexBufferBindings();

        /// <summary>
        /// Property to set or return the index buffer.
        /// </summary>
        public GorgonIndexBuffer IndexBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the current primitive topology.
        /// </summary>
        public D3D.PrimitiveTopology Topology
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the current rasterizer state.
        /// </summary>
        public GorgonPipelineState PipelineState
        {
            get;
        } = new GorgonPipelineState();

        /// <summary>
        /// Property to return the current input layout.
        /// </summary>
        public GorgonInputLayout InputLayout => VertexBuffers?.InputLayout;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to copy this state into another state object.
        /// </summary>
        /// <param name="state">The state to copy into.</param>
        public void CopyTo(D3DState state)
        {
            state.IndexBuffer = IndexBuffer;
            state.Topology = Topology;
            VertexBuffers.CopyTo(state.VertexBuffers);
            PipelineState.CopyTo(state.PipelineState);
        }
        #endregion
    }
}
