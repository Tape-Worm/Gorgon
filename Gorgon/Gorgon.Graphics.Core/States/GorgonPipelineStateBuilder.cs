
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
// Created: May 24, 2018 7:24:06 PM
// 


using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Patterns;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A builder used to create pipeline render state objects
/// </summary>
/// <remarks>
/// <para>
/// Use this builder to create a new <see cref="GorgonPipelineState"/> object to pass to a draw call. This object provides a fluent interface to help build up a pipeline state
/// </para>
/// <para>
/// A pipeline state object is used to define the state of the pipeline prior to drawing anything. It can be used to assign shaders, and various other states that will affect how data is rasterized on 
/// the GPU
/// </para>
/// <para>
/// The pipeline state object is immutable, and as such cannot be changed directly. To create a new pipeline state object, use this object to define the parameters for the pipeline state
/// </para>
/// <para>
/// Pipeline states are assigned to a draw call via one of the draw call builders
/// </para>
/// <para>
/// </para>
/// </remarks>
/// <seealso cref="GorgonPipelineState"/>
/// <seealso cref="GorgonPipelineStateBuilder"/>
/// <seealso cref="GorgonDrawCallBuilder"/>
/// <seealso cref="GorgonDrawIndexCallBuilder"/>
/// <seealso cref="GorgonStreamOutCallBuilder"/>
public class GorgonPipelineStateBuilder
    : IGorgonGraphicsObject, IGorgonFluentBuilder<GorgonPipelineStateBuilder, GorgonPipelineState>
{

    // The working state.
    private readonly GorgonPipelineState _workState = new();



    /// <summary>
    /// Property to return the graphics interface used to build the pipeline state.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }



    /// <summary>
    /// Function to add a rasterizer state to this pipeline state.
    /// </summary>
    /// <param name="state">The rasterizer state to apply.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonPipelineStateBuilder RasterState(GorgonRasterStateBuilder state) => RasterState(state?.Build());

    /// <summary>
    /// Function to add a rasterizer state to this pipeline state.
    /// </summary>
    /// <param name="state">The rasterizer state to apply.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonPipelineStateBuilder RasterState(GorgonRasterState state)
    {
        _workState.RasterState = state ?? GorgonRasterState.Default;
        return this;
    }

    /// <summary>
    /// Function to add a depth/stencil state to this pipeline state.
    /// </summary>
    /// <param name="state">The depth/stencil state to apply.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonPipelineStateBuilder DepthStencilState(GorgonDepthStencilStateBuilder state) => DepthStencilState(state?.Build());

    /// <summary>
    /// Function to add a depth/stencil state to this pipeline state.
    /// </summary>
    /// <param name="state">The depth/stencil state to apply.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonPipelineStateBuilder DepthStencilState(GorgonDepthStencilState state)
    {
        _workState.DepthStencilState = state ?? GorgonDepthStencilState.Default;
        return this;
    }

    /// <summary>
    /// Function to set the current pixel shader on the pipeline.
    /// </summary>
    /// <param name="pixelShader">The pixel shader to assign.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonPipelineStateBuilder PixelShader(GorgonPixelShader pixelShader)
    {
        _workState.PixelShader = pixelShader;
        return this;
    }

    /// <summary>
    /// Function to set the current vertex shader on the pipeline.
    /// </summary>
    /// <param name="vertexShader">The vertex shader to assign.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonPipelineStateBuilder VertexShader(GorgonVertexShader vertexShader)
    {
        _workState.VertexShader = vertexShader;
        return this;
    }

    /// <summary>
    /// Function to set the current geometry shader on the pipeline.
    /// </summary>
    /// <param name="geometryShader">The geometry shader to assign.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonPipelineStateBuilder GeometryShader(GorgonGeometryShader geometryShader)
    {
        _workState.GeometryShader = geometryShader;
        return this;
    }

    /// <summary>
    /// Function to set the current domain shader on the pipeline.
    /// </summary>
    /// <param name="domainShader">The domain shader to assign.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonPipelineStateBuilder DomainShader(GorgonDomainShader domainShader)
    {
        _workState.DomainShader = domainShader;
        return this;
    }

    /// <summary>
    /// Function to set the current hull shader on the pipeline.
    /// </summary>
    /// <param name="hullShader">The domain shader to assign.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonPipelineStateBuilder HullShader(GorgonHullShader hullShader)
    {
        _workState.HullShader = hullShader;
        return this;
    }

    /// <summary>
    /// Function to set primitive topology for the draw call.
    /// </summary>
    /// <param name="primitiveType">The type of primitive to render.</param>
    /// <returns>The fluent builder interface.</returns>
    public GorgonPipelineStateBuilder PrimitiveType(PrimitiveType primitiveType)
    {
        _workState.PrimitiveType = primitiveType;
        return this;
    }

    /// <summary>
    /// Function to enable alpha coverage for blending.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    public GorgonPipelineStateBuilder EnableAlphaCoverage()
    {
        _workState.IsAlphaToCoverageEnabled = true;
        return this;
    }

    /// <summary>
    /// Function to disable alpha coverage for blending.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    public GorgonPipelineStateBuilder DisableAlphaCoverage()
    {
        _workState.IsAlphaToCoverageEnabled = false;
        return this;
    }

    /// <summary>
    /// Function to enable independent render target blending.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    public GorgonPipelineStateBuilder EnableIndependentBlending()
    {
        _workState.IsIndependentBlendingEnabled = true;
        return this;
    }

    /// <summary>
    /// Function to disable independent render target blending
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    public GorgonPipelineStateBuilder DisableIndependentBlending()
    {
        _workState.IsIndependentBlendingEnabled = false;
        return this;
    }

    /// <summary>
    /// Function to assign a <see cref="GorgonBlendState"/> to the pipeline.
    /// </summary>
    /// <param name="state">The state to apply to the pipeline</param>
    /// <param name="slot">[Optional] The slot to assign the states into.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="slot"/> is less than 0, or greater than/equal to 8.</exception>
    public GorgonPipelineStateBuilder BlendState(GorgonBlendState state, int slot = 0)
    {
        if (slot is < 0 or >= D3D11.OutputMergerStage.SimultaneousRenderTargetCount)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_BLEND_SLOT_INVALID, D3D11.OutputMergerStage.SimultaneousRenderTargetCount));
        }

        _workState.RwBlendStates[slot] = state;
        return this;
    }

    /// <summary>
    /// Function to assign a list of <see cref="GorgonBlendState"/> objects to the pipeline.
    /// </summary>
    /// <param name="states">The states to apply to the pipeline</param>
    /// <param name="startSlot">[Optional] The first slot to assign the states into.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="startSlot"/> is less than 0, or greater than/equal to 8.</exception>
    public GorgonPipelineStateBuilder BlendStates(IReadOnlyList<GorgonBlendState> states, int startSlot = 0)
    {
        if (startSlot is < 0 or >= D3D11.OutputMergerStage.SimultaneousRenderTargetCount)
        {
            throw new ArgumentOutOfRangeException(nameof(startSlot), string.Format(Resources.GORGFX_ERR_BLEND_SLOT_INVALID, D3D11.OutputMergerStage.SimultaneousRenderTargetCount));
        }

        StateCopy.CopyBlendStates(_workState.RwBlendStates, states, startSlot);
        return this;
    }

    /// <summary>
    /// Function to reset the pipeline state to the specified state passed in to the method.
    /// </summary>
    /// <param name="pipeState">The pipeline state to copy.</param>
    /// <returns>The fluent interface for the builder.</returns>
    public GorgonPipelineStateBuilder ResetTo(GorgonPipelineState pipeState)
    {
        if (pipeState is null)
        {
            Clear();
            return this;
        }

        _workState.RasterState = pipeState.RasterState;
        _workState.DepthStencilState = pipeState.DepthStencilState;
        _workState.PixelShader = pipeState.PixelShader;
        _workState.VertexShader = pipeState.VertexShader;
        _workState.GeometryShader = pipeState.GeometryShader;
        _workState.DomainShader = pipeState.DomainShader;
        _workState.HullShader = pipeState.HullShader;
        _workState.PrimitiveType = pipeState.PrimitiveType;
        _workState.IsIndependentBlendingEnabled = pipeState.IsIndependentBlendingEnabled;
        _workState.IsAlphaToCoverageEnabled = pipeState.IsAlphaToCoverageEnabled;
        StateCopy.CopyBlendStates(_workState.RwBlendStates, pipeState.RwBlendStates, 0);

        return this;
    }

    /// <summary>
    /// Function to clear the current pipeline state.
    /// </summary>
    /// <returns>The fluent interface for the builder.</returns>
    public GorgonPipelineStateBuilder Clear()
    {
        _workState.Clear();
        _workState.RwBlendStates[0] = GorgonBlendState.Default;
        _workState.RasterState = GorgonRasterState.Default;
        _workState.DepthStencilState = GorgonDepthStencilState.Default;
        _workState.PrimitiveType = Core.PrimitiveType.TriangleList;
        return this;
    }

    /// <summary>
    /// Function to build a pipeline state.
    /// </summary>
    /// <returns>A new pipeline state.</returns>
    public GorgonPipelineState Build() =>
        // Build the actual state.
        Graphics.PipelineStateCache.Cache(_workState);



    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonPipelineStateBuilder"/> class.
    /// </summary>
    /// <param name="graphics">The graphics object that will build the pipeline state.</param>
    public GorgonPipelineStateBuilder(GorgonGraphics graphics)
    {
        Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        _workState.RwBlendStates[0] = GorgonBlendState.Default;
        _workState.RasterState = GorgonRasterState.Default;
        _workState.DepthStencilState = GorgonDepthStencilState.Default;
        _workState.PrimitiveType = Core.PrimitiveType.TriangleList;
    }

}
