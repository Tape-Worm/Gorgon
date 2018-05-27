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
// Created: May 23, 2018 12:18:45 PM
// 
#endregion

using Gorgon.Collections;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Common values for a draw call.
    /// </summary>
    public abstract class GorgonDrawCallCommon
    {
        #region Properties.
        /// <summary>
        /// Property to return the internal D3D state.
        /// </summary>
        internal D3DState D3DState
        {
            get;
        } = new D3DState();

        /// <summary>
        /// Property to return the pipeline state for this draw call.
        /// </summary>
        public GorgonPipelineState PipelineState => D3DState.PipelineState;

        /// <summary>
        /// Property to return the vertex buffers bound to the draw call.
        /// </summary>
        public IGorgonReadOnlyArray<GorgonVertexBufferBinding> VertexBufferBindings => D3DState.VertexBuffers;

        /// <summary>
        /// Property to return the input layout for the draw call.
        /// </summary>
        /// <remarks>
        /// This is derived from the <see cref="VertexBufferBindings"/> passed to the call.
        /// </remarks>
        public GorgonInputLayout InputLayout => D3DState.VertexBuffers.InputLayout;

        /// <summary>
        /// Property to return the topology for a primitive.
        /// </summary>
        public PrimitiveType PrimitiveType => (PrimitiveType)D3DState.Topology;
        
        /// <summary>
        /// Property to return the resources for the pixel shader.
        /// </summary>
        public ShaderStates<GorgonPixelShader> PixelShader
        {
            get;
        }

        /// <summary>
        /// Property to return the resources for the vertex shader.
        /// </summary>
        public ShaderStates<GorgonVertexShader> VertexShader
        {
            get;
        }

        /// <summary>
        /// Property to return the resources for the geometry shader.
        /// </summary>
        public ShaderStates<GorgonGeometryShader> GeometryShader
        {
            get;
        }

        /// <summary>
        /// Property to return the resources for the domain shader.
        /// </summary>
        public ShaderStates<GorgonDomainShader> DomainShader
        {
            get;
        }

        /// <summary>
        /// Property to return the resources for the hull shader.
        /// </summary>
        public ShaderStates<GorgonHullShader> HullShader
        {
            get;
        }

        /// <summary>
        /// Property to return the resources for the compute shader.
        /// </summary>
        public ShaderStates<GorgonComputeShader> ComputeShader
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to set up shader resource views for each shader.
        /// </summary>
        internal void SetupViews()
        {
            // TODO:
        }

        /// <summary>
        /// Function to set up the samplers for each shader.
        /// </summary>
        internal void SetupSamplers()
        {
            PixelShader.Samplers = D3DState.PsSamplers = new GorgonSamplerStates();
            VertexShader.Samplers = D3DState.VsSamplers = new GorgonSamplerStates();
            GeometryShader.Samplers = D3DState.GsSamplers = new GorgonSamplerStates();
            HullShader.Samplers = D3DState.HsSamplers = new GorgonSamplerStates();
            DomainShader.Samplers = D3DState.DsSamplers = new GorgonSamplerStates();
            ComputeShader.Samplers = D3DState.CsSamplers = new GorgonSamplerStates();
        }

        /// <summary>
        /// Function to set up the constant buffers for each shader.
        /// </summary>
        internal void SetupConstantBuffers()
        {
            PixelShader.ConstantBuffers = D3DState.PsConstantBuffers = new GorgonConstantBuffers();
            VertexShader.ConstantBuffers = D3DState.VsConstantBuffers = new GorgonConstantBuffers();
            GeometryShader.ConstantBuffers = D3DState.GsConstantBuffers = new GorgonConstantBuffers();
            HullShader.ConstantBuffers = D3DState.HsConstantBuffers = new GorgonConstantBuffers();
            DomainShader.ConstantBuffers = D3DState.DsConstantBuffers = new GorgonConstantBuffers();
            ComputeShader.ConstantBuffers = D3DState.CsConstantBuffers = new GorgonConstantBuffers();
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonDrawCallCommon"/> class.
        /// </summary>
        protected GorgonDrawCallCommon()
        {
            PixelShader = new ShaderStates<GorgonPixelShader>();
            VertexShader = new ShaderStates<GorgonVertexShader>();
            GeometryShader = new ShaderStates<GorgonGeometryShader>();
            DomainShader = new ShaderStates<GorgonDomainShader>();
            HullShader = new ShaderStates<GorgonHullShader>();
            ComputeShader = new ShaderStates<GorgonComputeShader>();
        }
        #endregion
    }
}
