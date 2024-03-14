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
// Created: May 24, 2018 7:24:06 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Memory;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A builder used to create pipeline render state objects.
/// </summary>
/// <remarks>
/// <para>
/// Use this builder to create an immutable <see cref="GorgonStreamOutPipelineState"/>. This object will provide a fluent interface to build up the pipeline state.
/// </para>
/// <para>
/// A pipeline state object is used to define the state of the pipeline prior to drawing anything. It can be used to assign shaders, and various other states that will affect how data is rasterized on 
/// the GPU.
/// </para>
/// <para>
/// This pipeline state is similar to a <see cref="GorgonPipelineState"/>, except that it only contains the necessary states that it can use when rendering a stream out buffer.
/// </para>
/// <para>
/// The pipeline state object is immutable, and as such cannot be changed directly. To create a new pipeline state object, this object must be used.
/// </para>
/// <para>
/// Pipeline states are assigned to a <see cref="GorgonStreamOutCall"/>.
/// </para>
/// </remarks>
/// <seealso cref="GorgonPipelineStateBuilder"/>
/// <seealso cref="GorgonPipelineState"/>
/// <seealso cref="GorgonStreamOutCall"/>
/// <seealso cref="GorgonStreamOutPipelineState"/>
public class GorgonStreamOutPipelineStateBuilder
    : IGorgonGraphicsObject, IGorgonFluentBuilderAllocator<GorgonStreamOutPipelineStateBuilder, GorgonStreamOutPipelineState, IGorgonAllocator<GorgonStreamOutPipelineState>>
{
    #region Variables.
    // The working state.
    private readonly GorgonStreamOutPipelineState _workState = new(new GorgonPipelineState
    {
        PrimitiveType = Core.PrimitiveType.TriangleList
    });
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the graphics interface used to build the pipeline state.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to set primitive topology for the draw call.
    /// </summary>
    /// <param name="primitiveType">The type of primitive to render.</param>
    /// <returns>The fluent builder interface.</returns>
    public GorgonStreamOutPipelineStateBuilder PrimitiveType(PrimitiveType primitiveType)
    {
        _workState.PipelineState.PrimitiveType = primitiveType;
        return this;
    }

    /// <summary>
    /// Function to add a rasterizer state to this pipeline state.
    /// </summary>
    /// <param name="state">The rasterizer state to apply.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonStreamOutPipelineStateBuilder RasterState(GorgonRasterStateBuilder state) => RasterState(state?.Build());

    /// <summary>
    /// Function to add a rasterizer state to this pipeline state.
    /// </summary>
    /// <param name="state">The rasterizer state to apply.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonStreamOutPipelineStateBuilder RasterState(GorgonRasterState state)
    {
        _workState.PipelineState.RasterState = state ?? GorgonRasterState.Default;
        return this;
    }

    /// <summary>
    /// Function to add a depth/stencil state to this pipeline state.
    /// </summary>
    /// <param name="state">The depth/stencil state to apply.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonStreamOutPipelineStateBuilder DepthStencilState(GorgonDepthStencilStateBuilder state) => DepthStencilState(state?.Build());

    /// <summary>
    /// Function to add a depth/stencil state to this pipeline state.
    /// </summary>
    /// <param name="state">The depth/stencil state to apply.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonStreamOutPipelineStateBuilder DepthStencilState(GorgonDepthStencilState state)
    {
        _workState.PipelineState.DepthStencilState = state ?? GorgonDepthStencilState.Default;
        return this;
    }

    /// <summary>
    /// Function to set the current pixel shader on the pipeline.
    /// </summary>
    /// <param name="pixelShader">The pixel shader to assign.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonStreamOutPipelineStateBuilder PixelShader(GorgonPixelShader pixelShader)
    {
        _workState.PipelineState.PixelShader = pixelShader;
        return this;
    }

    /// <summary>
    /// Function to set the current vertex shader on the pipeline.
    /// </summary>
    /// <param name="vertexShader">The vertex shader to assign.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonStreamOutPipelineStateBuilder VertexShader(GorgonVertexShader vertexShader)
    {
        _workState.PipelineState.VertexShader = vertexShader;
        return this;
    }

    /// <summary>
    /// Function to enable alpha coverage for blending.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    public GorgonStreamOutPipelineStateBuilder EnableAlphaCoverage()
    {
        _workState.PipelineState.IsAlphaToCoverageEnabled = true;
        return this;
    }

    /// <summary>
    /// Function to disable alpha coverage for blending.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    public GorgonStreamOutPipelineStateBuilder DisableAlphaCoverage()
    {
        _workState.PipelineState.IsAlphaToCoverageEnabled = false;
        return this;
    }

    /// <summary>
    /// Function to enable independent render target blending.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    public GorgonStreamOutPipelineStateBuilder EnableIndependentBlending()
    {
        _workState.PipelineState.IsIndependentBlendingEnabled = true;
        return this;
    }

    /// <summary>
    /// Function to disable independent render target blending
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    public GorgonStreamOutPipelineStateBuilder DisableIndependentBlending()
    {
        _workState.PipelineState.IsIndependentBlendingEnabled = false;
        return this;
    }

    /// <summary>
    /// Function to assign a <see cref="GorgonBlendState"/> to the pipeline.
    /// </summary>
    /// <param name="state">The state to apply to the pipeline</param>
    /// <param name="slot">[Optional] The slot to assign the states into.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="slot"/> is less than 0, or greater than/equal to 8.</exception>
    public GorgonStreamOutPipelineStateBuilder BlendState(GorgonBlendState state, int slot = 0)
    {
        if (slot is < 0 or >= D3D11.OutputMergerStage.SimultaneousRenderTargetCount)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_BLEND_SLOT_INVALID, D3D11.OutputMergerStage.SimultaneousRenderTargetCount));
        }

        _workState.PipelineState.RwBlendStates[slot] = state;
        return this;
    }

    /// <summary>
    /// Function to assign a list of <see cref="GorgonBlendState"/> objects to the pipeline.
    /// </summary>
    /// <param name="states">The states to apply to the pipeline</param>
    /// <param name="startSlot">[Optional] The first slot to assign the states into.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="startSlot"/> is less than 0, or greater than/equal to 8.</exception>
    public GorgonStreamOutPipelineStateBuilder BlendStates(IReadOnlyList<GorgonBlendState> states, int startSlot = 0)
    {
        if (startSlot is < 0 or >= D3D11.OutputMergerStage.SimultaneousRenderTargetCount)
        {
            throw new ArgumentOutOfRangeException(nameof(startSlot), string.Format(Resources.GORGFX_ERR_BLEND_SLOT_INVALID, D3D11.OutputMergerStage.SimultaneousRenderTargetCount));
        }

        StateCopy.CopyBlendStates(_workState.PipelineState.RwBlendStates, states, startSlot);
        return this;
    }

    /// <summary>
    /// Function to reset the pipeline state to the specified state passed in to the method.
    /// </summary>
    /// <param name="pipeState">The pipeline state to copy.</param>
    /// <returns>The fluent interface for the builder.</returns>
    public GorgonStreamOutPipelineStateBuilder ResetTo(GorgonStreamOutPipelineState pipeState)
    {
        if (pipeState is null)
        {
            Clear();
            return this;
        }

        _workState.PipelineState.PrimitiveType = pipeState.PrimitiveType;
        _workState.PipelineState.RasterState = pipeState.RasterState;
        _workState.PipelineState.DepthStencilState = pipeState.DepthStencilState;
        _workState.PipelineState.PixelShader = pipeState.PixelShader;
        _workState.PipelineState.VertexShader = pipeState.VertexShader;
        _workState.PipelineState.IsIndependentBlendingEnabled = pipeState.IsIndependentBlendingEnabled;
        _workState.PipelineState.IsAlphaToCoverageEnabled = pipeState.IsAlphaToCoverageEnabled;
        pipeState.PipelineState.RwBlendStates.CopyTo(_workState.PipelineState.RwBlendStates);

        return this;
    }

    /// <summary>
    /// Function to clear the current pipeline state.
    /// </summary>
    /// <returns>The fluent interface for the builder.</returns>
    public GorgonStreamOutPipelineStateBuilder Clear()
    {
        _workState.PipelineState.Clear();
        return this;
    }

    /// <summary>
    /// Function to build a pipeline state.
    /// </summary>
    /// <param name="allocator">The allocator used to create an instance of the object</param>
    /// <returns>The object created or updated by this builder.</returns>
    /// <remarks>
    ///   <para>
    /// Using an <paramref name="allocator" /> can provide different strategies when building objects.  If omitted, the object will be created using the standard <span class="keyword">new</span> keyword.
    /// </para>
    ///   <para>
    /// A custom allocator can be beneficial because it allows us to use a pool for allocating the objects, and thus allows for recycling of objects. This keeps the garbage collector happy by keeping objects
    /// around for as long as we need them, instead of creating objects that can potentially end up in the large object heap or in Gen 2.
    /// </para>
    /// </remarks>
    public GorgonStreamOutPipelineState Build(IGorgonAllocator<GorgonStreamOutPipelineState> allocator)
    {
        if (allocator is null)
        {
            return new GorgonStreamOutPipelineState(Graphics.PipelineStateCache.Cache(_workState.PipelineState));
        }

        // Caches the state info.
        void CacheState(GorgonStreamOutPipelineState state) => Graphics.PipelineStateCache.Cache(state.PipelineState);

        return allocator.Allocate(CacheState);
    }

    /// <summary>
    /// Function to build a pipeline state.
    /// </summary>
    /// <returns>A new pipeline state.</returns>
    public GorgonStreamOutPipelineState Build() => Build(null);
    #endregion

    #region Constructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonStreamOutPipelineStateBuilder"/> class.
    /// </summary>
    /// <param name="graphics">The graphics object that will build the pipeline state.</param>
    public GorgonStreamOutPipelineStateBuilder(GorgonGraphics graphics)
    {
        Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));

        _workState.PipelineState.RasterState = GorgonRasterState.Default;
        _workState.PipelineState.RwBlendStates[0] = GorgonBlendState.Default;
        _workState.PipelineState.DepthStencilState = GorgonDepthStencilState.Default;
        _workState.PipelineState.PrimitiveType = Core.PrimitiveType.TriangleList;
    }
    #endregion
}
