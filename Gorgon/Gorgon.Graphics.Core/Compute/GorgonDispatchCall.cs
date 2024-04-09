
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: June 5, 2018 12:51:54 PM
// 

using Gorgon.Collections;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides information used to execute a call on the <see cref="GorgonComputeEngine"/>
/// </summary>
/// <seealso cref="GorgonComputeEngine"/>
public class GorgonDispatchCall
{
    /// <summary>
    /// Property to return the current Direct3D state.
    /// </summary>
    internal D3DState D3DState
    {
        get;
    } = new D3DState();

    /// <summary>
    /// Property to return the compute shader used for this dispatch.
    /// </summary>
    public GorgonComputeShader ComputeShader => D3DState.ComputeShader;

    /// <summary>
    /// Property to return the read/write (unordered access) views to use.
    /// </summary>
    public IGorgonReadOnlyArray<GorgonReadWriteViewBinding> ReadWriteViews => D3DState.CsReadWriteViews;

    /// <summary>
    /// Property to return the shader resource views to use.
    /// </summary>
    public IGorgonReadOnlyArray<GorgonShaderResourceView> ShaderResources => D3DState.CsSrvs;

    /// <summary>
    /// Property to return the texture samplers to use.
    /// </summary>
    public IGorgonReadOnlyArray<GorgonSamplerState> Samplers => D3DState.CsSamplers;

    /// <summary>
    /// Property to return the constant buffers to use.
    /// </summary>
    public IGorgonReadOnlyArray<GorgonConstantBufferView> ConstantBuffers => D3DState.CsConstantBuffers;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonDispatchCall"/> class.
    /// </summary>
    internal GorgonDispatchCall()
    {
        D3DState.VertexBuffers = new GorgonVertexBufferBindings();
        D3DState.StreamOutBindings = new GorgonStreamOutBindings();
        D3DState.PipelineState = new GorgonPipelineState
        {
            RasterState = GorgonRasterState.Default,
            DepthStencilState = GorgonDepthStencilState.Default
        };
        D3DState.PsConstantBuffers = D3DState.VsConstantBuffers =
                                           D3DState.GsConstantBuffers =
                                               D3DState.DsConstantBuffers = D3DState.HsConstantBuffers = new GorgonConstantBuffers();
        D3DState.PsSamplers = D3DState.VsSamplers =
                                    D3DState.GsSamplers =
                                        D3DState.DsSamplers = D3DState.HsSamplers = new GorgonSamplerStates();
        D3DState.PsSrvs = D3DState.VsSrvs =
                                D3DState.GsSrvs =
                                    D3DState.DsSrvs = D3DState.HsSrvs = new GorgonShaderResourceViews();
        D3DState.ReadWriteViews = new GorgonReadWriteViewBindings();

        D3DState.CsReadWriteViews = new GorgonReadWriteViewBindings();
        D3DState.CsSrvs = new GorgonShaderResourceViews();
        D3DState.CsSamplers = new GorgonSamplerStates();
        D3DState.CsConstantBuffers = new GorgonConstantBuffers();
    }
}
