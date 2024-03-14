
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
// Created: May 23, 2018 12:18:45 PM
// 


using Gorgon.Collections;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Common values for a draw call
/// </summary>
/// <remarks>
/// <para>
/// A draw call is an immutable object that contains all of the state required to render mesh information. For each mesh an application needs to render, an single draw call should be issued via the
/// <see cref="GorgonGraphics.Submit(GorgonDrawCall, in GorgonColor?, int, int)"/> method.  
/// </para>
/// <para>
/// State management is handled internally by Gorgon so that duplicate states are not set and thus, performance is not impacted by redundant states
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphics"/>
public abstract class GorgonDrawCallCommon
{

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
    /// Property to return the stream out buffers bound to the draw call.
    /// </summary>
    public IGorgonReadOnlyArray<GorgonStreamOutBinding> StreamOutBufferBindings => D3DState.StreamOutBindings;

    /// <summary>
    /// Property to return the input layout for the draw call.
    /// </summary>
    /// <remarks>
    /// This is derived from the <see cref="VertexBufferBindings"/> passed to the call.
    /// </remarks>
    public GorgonInputLayout InputLayout => D3DState.VertexBuffers.InputLayout;

    /// <summary>
    /// Property to return the resources for the pixel shader.
    /// </summary>
    public GorgonShaderResources PixelShader
    {
        get;
    }

    /// <summary>
    /// Property to return the resources for the vertex shader.
    /// </summary>
    public GorgonShaderResources VertexShader
    {
        get;
    }

    /// <summary>
    /// Property to return the resources for the geometry shader.
    /// </summary>
    public GorgonShaderResources GeometryShader
    {
        get;
    }

    /// <summary>
    /// Property to return the resources for the domain shader.
    /// </summary>
    public GorgonShaderResources DomainShader
    {
        get;
    }

    /// <summary>
    /// Property to return the resources for the hull shader.
    /// </summary>
    public GorgonShaderResources HullShader
    {
        get;
    }

    /// <summary>
    /// Property to return the list of unordered access views for the shader.
    /// </summary>
    public IGorgonReadOnlyArray<GorgonReadWriteViewBinding> ReadWriteViews => D3DState.ReadWriteViews;



    /// <summary>
    /// Function to set up shader resource views for each shader.
    /// </summary>
    internal void SetupViews()
    {
        VertexShader.ShaderResources = D3DState.VsSrvs = new GorgonShaderResourceViews();
        PixelShader.ShaderResources = D3DState.PsSrvs = new GorgonShaderResourceViews();
        GeometryShader.ShaderResources = D3DState.GsSrvs = new GorgonShaderResourceViews();
        DomainShader.ShaderResources = D3DState.DsSrvs = new GorgonShaderResourceViews();
        HullShader.ShaderResources = D3DState.HsSrvs = new GorgonShaderResourceViews();
        D3DState.ReadWriteViews = new GorgonReadWriteViewBindings();
        D3DState.CsSrvs = new GorgonShaderResourceViews();
        D3DState.CsReadWriteViews = new GorgonReadWriteViewBindings();
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
        D3DState.CsSamplers = new GorgonSamplerStates();
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
        D3DState.CsConstantBuffers = new GorgonConstantBuffers();
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonDrawCallCommon"/> class.
    /// </summary>
    protected GorgonDrawCallCommon()
    {
        PixelShader = new GorgonShaderResources();
        VertexShader = new GorgonShaderResources();
        GeometryShader = new GorgonShaderResources();
        DomainShader = new GorgonShaderResources();
        HullShader = new GorgonShaderResources();
    }

}
