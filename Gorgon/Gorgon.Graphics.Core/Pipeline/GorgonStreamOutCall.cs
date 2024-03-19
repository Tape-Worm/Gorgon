
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
// Created: May 29, 2018 1:31:23 PM
// 


using Gorgon.Collections;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A call used to stream data generated on the GPU via shaders
/// </summary>
/// <remarks>
/// <para>
/// A draw call is an immutable object that contains all of the state required to render mesh information. For each mesh an application needs to render, an single draw call should be issued via the
/// <see cref="GorgonGraphics.SubmitStreamOut(GorgonStreamOutCall, GorgonColor?, int, int)"/> method.  
/// </para>
/// <para>
/// State management is handled internally by Gorgon so that duplicate states are not set and thus, performance is not impacted by redundant states
/// </para>
/// <para>
/// Because a draw call is immutable, it is not possible to modify a draw call after it's been created. However, a copy of a draw call can be created using the
/// <see cref="GorgonDrawCallBuilderCommon{TB,TDc}.ResetTo"/> method on the <see cref="GorgonStreamOutCallBuilder"/> object. Or, the builder can be modified after the creation of your draw call 
/// that needs to be updated and a new call may be built then
/// </para>
/// <para>
/// This draw call type sends a series of state changes and resource bindings to the GPU. However, unlike the various <see cref="GorgonDrawCallCommon"/> objects, this object uses pre-processed data from 
/// the vertex and stream out stages. This means that the <see cref="GorgonVertexBuffer"/> attached to the a previous draw call must have been assigned to the 
/// <see cref="GorgonDrawCallCommon.StreamOutBufferBindings"/> and had data deposited into it from the stream out stage. After that, it should be removed from the 
/// <see cref="GorgonDrawCallCommon.StreamOutBufferBindings"/> and assigned to the <see cref="VertexBufferBinding"/> 
/// </para>
/// <para>
/// <note type="warning">
/// <para>
/// The <see cref="GorgonVertexBuffer"/> being rendered must have been created with the <see cref="VertexIndexBufferBinding.StreamOut"/> flag set
/// </para>
/// </note>
/// </para>
/// </remarks>
/// <seealso cref="VertexBufferBinding"/>
/// <seealso cref="GorgonGraphics"/>
/// <seealso cref="GorgonDrawIndexCallBuilder"/>
public class GorgonStreamOutCall
{

    // The current pipeline state.
    private GorgonStreamOutPipelineState _pipelineState;



    /// <summary>
    /// Property to return the internal D3D state.
    /// </summary>
    internal D3DState D3DState
    {
        get;
    } = new D3DState();

    /// <summary>
    /// Property to return the vertex buffer binding for the stream out call.
    /// </summary>
    public GorgonVertexBufferBinding VertexBufferBinding => D3DState.VertexBuffers[0];

    /// <summary>
    /// Property to return the pipeline state.
    /// </summary>
    public GorgonStreamOutPipelineState PipelineState
    {
        get => _pipelineState;
        internal set
        {
            _pipelineState = value;
            D3DState.PipelineState = _pipelineState?.PipelineState;
        }
    }

    /// <summary>
    /// Property to return the input layout for the draw call.
    /// </summary>
    public GorgonInputLayout InputLayout => D3DState.InputLayout;

    /// <summary>
    /// Property to return the resources for the pixel shader.
    /// </summary>
    public GorgonShaderResources PixelShader
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
        PixelShader.ShaderResources = D3DState.PsSrvs = new GorgonShaderResourceViews();
        D3DState.ReadWriteViews = new GorgonReadWriteViewBindings();
        D3DState.CsReadWriteViews = new GorgonReadWriteViewBindings();
        D3DState.DsSrvs = D3DState.HsSrvs = D3DState.VsSrvs = D3DState.GsSrvs = D3DState.CsSrvs = new GorgonShaderResourceViews();

    }

    /// <summary>
    /// Function to set up the samplers for each shader.
    /// </summary>
    internal void SetupSamplers()
    {
        PixelShader.Samplers = D3DState.PsSamplers = new GorgonSamplerStates();
        D3DState.HsSamplers = D3DState.DsSamplers = D3DState.VsSamplers = D3DState.GsSamplers = D3DState.CsSamplers = new GorgonSamplerStates();
    }

    /// <summary>
    /// Function to set up the constant buffers for each shader.
    /// </summary>
    internal void SetupConstantBuffers()
    {
        PixelShader.ConstantBuffers = D3DState.PsConstantBuffers = new GorgonConstantBuffers();
        D3DState.HsConstantBuffers = D3DState.DsConstantBuffers = D3DState.VsConstantBuffers = D3DState.GsConstantBuffers = D3DState.CsConstantBuffers = new GorgonConstantBuffers();
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonStreamOutCall"/> class.
    /// </summary>
    internal GorgonStreamOutCall()
    {
        PixelShader = new GorgonShaderResources();
        D3DState.StreamOutBindings = new GorgonStreamOutBindings();
    }

}
