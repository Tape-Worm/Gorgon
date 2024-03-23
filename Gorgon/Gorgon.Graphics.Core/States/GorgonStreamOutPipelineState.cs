
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
// Created: May 29, 2018 3:09:46 PM
// 

using Gorgon.Collections;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A pipeline state for stream out buffers
/// </summary>
/// <remarks>
/// <para>
/// A pipeline state object is used to define the state of the pipeline prior to drawing anything. It can be used to assign shaders, and various other states that will affect how data is rasterized on 
/// the GPU
/// </para>
/// <para>
/// This pipeline state is similar to a <see cref="GorgonPipelineState"/>, except that it only contains the necessary states that it can use when rendering a stream out buffer
/// </para>
/// <para>
/// The pipeline state object is immutable, and as such cannot be changed directly. To create a new pipeline state object, a <see cref="GorgonStreamOutPipelineStateBuilder"/> must be used
/// </para>
/// <para>
/// Pipeline states are assigned to a <see cref="GorgonStreamOutCall"/>
/// </para>
/// </remarks>
/// <seealso cref="GorgonPipelineStateBuilder"/>
/// <seealso cref="GorgonPipelineState"/>
/// <seealso cref="GorgonStreamOutCall"/>
/// <seealso cref="GorgonStreamOutPipelineStateBuilder"/>
public class GorgonStreamOutPipelineState
{
    /// <summary>
    /// Property to set or return the pipeline state.
    /// </summary>
    internal GorgonPipelineState PipelineState
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the type of primitive to use when drawing.
    /// </summary>
    public PrimitiveType PrimitiveType => PipelineState.PrimitiveType;

    /// <summary>
    /// Property to return the pixel shader.
    /// </summary>
    public GorgonPixelShader PixelShader => PipelineState.PixelShader;

    /// <summary>
    /// Property to return the vertex shader.
    /// </summary>
    public GorgonVertexShader VertexShader => PipelineState.VertexShader;

    /// <summary>
    /// Property to return the rasterizer state for the pipeline.
    /// </summary>
    public GorgonRasterState RasterState => PipelineState.RasterState;

    /// <summary>
    /// Property to return the depth/stencil state for the pipeline.
    /// </summary>
    public GorgonDepthStencilState DepthStencilState => PipelineState.DepthStencilState;

    /// <summary>
    /// Property to return whether alpha to coverage is enabled or not for blending.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will use alpha to coverage as a multisampling technique when writing a pixel to a render target. Alpha to coverage is useful in situations where there are multiple overlapping polygons 
    /// that use transparency to define edges.
    /// </para>
    /// </remarks>
    public bool IsAlphaToCoverageEnabled => PipelineState.IsAlphaToCoverageEnabled;

    /// <summary>
    /// Property to return whether independent render target blending is enabled or not.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will specify whether to use different blending states for each render target. When this value is set to <b>true</b>, each render target blend state will be independent of other render 
    /// target blend states. When this value is set to <b>false</b>, then only the blend state of the first render target is used.
    /// </para>
    /// </remarks>
    public bool IsIndependentBlendingEnabled => PipelineState.IsIndependentBlendingEnabled;

    /// <summary>
    /// Property to return the list of blending states for each render target.
    /// </summary>
    public IGorgonReadOnlyArray<GorgonBlendState> BlendStates => PipelineState.RwBlendStates;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonStreamOutPipelineState" /> class.
    /// </summary>
    /// <param name="pipelineState">The pipeline state to copy.</param>
    internal GorgonStreamOutPipelineState(GorgonStreamOutPipelineState pipelineState) => PipelineState = new GorgonPipelineState(pipelineState.PipelineState);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonStreamOutPipelineState"/> class.
    /// </summary>
    /// <param name="pipelineState">State of the pipeline.</param>
    internal GorgonStreamOutPipelineState(GorgonPipelineState pipelineState) => PipelineState = pipelineState;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonStreamOutPipelineState"/> class.
    /// </summary>
    internal GorgonStreamOutPipelineState()
    {
    }
}
