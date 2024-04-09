
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

using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Memory;
using Gorgon.Patterns;
using SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A builder used to create stream out draw call objects
/// </summary>
/// <remarks>
/// <para>
/// The draw call builder object allow applications to build the immutable draw call objects needed to send data and state information to the GPU
/// </para>
/// <para>
/// A draw call is an immutable object that contains all of the state required to render mesh information. For each mesh an application needs to render, an single draw call should be issued via the
/// <see cref="GorgonGraphics.SubmitStreamOut(GorgonStreamOutCall, GorgonColor?, int, int)"/> method.  
/// </para>
/// <para>
/// State management is handled internally by Gorgon so that duplicate states are not set and thus, performance is not impacted by redundant states
/// </para>
/// <para>
/// Because a draw call is immutable, it is not possible to modify a draw call after it's been created. However, a copy of a draw call can be created using the
/// <see cref="GorgonDrawCallBuilderCommon{TB,TDc}.ResetTo"/> method on this object. Or, the builder can be modified after the creation of your draw call that needs to be updated and a new call may be
/// built then
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
/// <seealso cref="GorgonGraphics"/>
/// <seealso cref="GorgonPipelineState"/>
/// <seealso cref="GorgonStreamOutCall"/>
/// <seealso cref="GorgonVertexBuffer"/>
/// <seealso cref="VertexBufferBinding"/>
public sealed class GorgonStreamOutCallBuilder
    : IGorgonFluentBuilder<GorgonStreamOutCallBuilder, GorgonStreamOutCall, IGorgonAllocator<GorgonStreamOutCall>>
{

    // The worker call used to build up the object.
    private readonly GorgonStreamOutCall _workerCall;

    /// <summary>
    /// Function to assign a list of samplers to a shader on the pipeline.
    /// </summary>
    /// <param name="samplers">The samplers to assign.</param>
    /// <param name="index">[Optional] The index to use when copying the list.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> parameter is less than 0, or greater than/equal to <see cref="GorgonSamplerStates.MaximumSamplerStateCount"/>.</exception>
    public GorgonStreamOutCallBuilder SamplerStates(IReadOnlyList<GorgonSamplerState> samplers, int index = 0)
    {
        if (index is < 0 or >= GorgonSamplerStates.MaximumSamplerStateCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index), string.Format(Resources.GORGFX_ERR_INVALID_SAMPLER_INDEX, GorgonSamplerStates.MaximumSamplerStateCount));
        }

        StateCopy.CopySamplers(_workerCall.D3DState.PsSamplers, samplers, index);
        return this;
    }

    /// <summary>
    /// Function to assign a sampler to a pixel shader on the pipeline.
    /// </summary>
    /// <param name="sampler">The sampler to assign.</param>
    /// <param name="index">[Optional] The index of the sampler.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonStreamOutCallBuilder SamplerState(GorgonSamplerStateBuilder sampler, int index = 0) => SamplerState(sampler.Build(), index);

    /// <summary>
    /// Function to assign a sampler to a pixel shader on the pipeline.
    /// </summary>
    /// <param name="sampler">The sampler to assign.</param>
    /// <param name="index">[Optional] The index of the sampler.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> parameter is less than 0, or greater than/equal to <see cref="GorgonSamplerStates.MaximumSamplerStateCount"/>.</exception>
    public GorgonStreamOutCallBuilder SamplerState(GorgonSamplerState sampler, int index = 0)
    {
        if (index is < 0 or >= GorgonSamplerStates.MaximumSamplerStateCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index), string.Format(Resources.GORGFX_ERR_INVALID_SAMPLER_INDEX, GorgonSamplerStates.MaximumSamplerStateCount));
        }

        _workerCall.D3DState.PsSamplers[index] = sampler;
        return this;
    }

    /// <summary>
    /// Function to set the pipeline state for this draw call.
    /// </summary>
    /// <param name="pipelineState">The pipeline state to assign.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pipelineState"/> parameter is <b>null</b>.</exception>
    public GorgonStreamOutCallBuilder PipelineState(GorgonStreamOutPipelineState pipelineState)
    {
        _workerCall.PipelineState = pipelineState ?? throw new ArgumentNullException(nameof(pipelineState));
        return this;
    }

    /// <summary>
    /// Function to set the pipeline state for this draw call.
    /// </summary>
    /// <param name="pipelineState">The pipeline state to assign.</param>
    /// <returns>The fluent builder interface.</returns>
    public GorgonStreamOutCallBuilder PipelineState(GorgonStreamOutPipelineStateBuilder pipelineState) => PipelineState(pipelineState?.Build());

    /// <summary>
    /// Function to set a vertex buffer binding for the draw call in slot 0.
    /// </summary>
    /// <param name="layout">The input layout to use.</param>
    /// <param name="binding">The vertex buffer binding to set.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="layout"/> parameter is <b>null</b>.</exception>
    public GorgonStreamOutCallBuilder VertexBuffer(GorgonInputLayout layout, ref readonly GorgonVertexBufferBinding binding)
    {
        if (_workerCall.D3DState.VertexBuffers is null)
        {
            _workerCall.D3DState.VertexBuffers = new GorgonVertexBufferBindings();
        }

        _workerCall.D3DState.VertexBuffers.Clear();
        _workerCall.D3DState.VertexBuffers[0] = binding;
        _workerCall.D3DState.VertexBuffers.InputLayout = layout ?? throw new ArgumentNullException(nameof(layout));
        return this;
    }

    /// <summary>
    /// Function to set a constant buffer for a pixel shader.
    /// </summary>
    /// <param name="constantBuffer">The constant buffer to assign.</param>
    /// <param name="slot">The slot for the constant buffer.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="slot"/> is less than 0, or greater than/equal to <see cref="GorgonConstantBuffers.MaximumConstantBufferCount"/>.</exception>
    public GorgonStreamOutCallBuilder ConstantBuffer(GorgonConstantBufferView constantBuffer, int slot = 0)
    {
        if (slot is < 0 or >= GorgonConstantBuffers.MaximumConstantBufferCount)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_CBUFFER_SLOT_INVALID, 0));
        }

        _workerCall.D3DState.PsConstantBuffers[slot] = constantBuffer;
        return this;
    }

    /// <summary>
    /// Function to set the constant buffers for a pixel shader.
    /// </summary>
    /// <param name="constantBuffers">The constant buffers to copy.</param>
    /// <param name="startSlot">[Optional] The starting slot to use when copying the list.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="startSlot"/> is less than 0, or greater than/equal to <see cref="GorgonConstantBuffers.MaximumConstantBufferCount"/>.</exception>
    public GorgonStreamOutCallBuilder ConstantBuffers(IReadOnlyList<GorgonConstantBufferView> constantBuffers, int startSlot = 0)
    {
        if (startSlot is < 0 or >= GorgonConstantBuffers.MaximumConstantBufferCount)
        {
            throw new ArgumentOutOfRangeException(nameof(startSlot), string.Format(Resources.GORGFX_ERR_CBUFFER_SLOT_INVALID, GorgonConstantBuffers.MaximumConstantBufferCount));
        }

        StateCopy.CopyConstantBuffers(_workerCall.D3DState.PsConstantBuffers, constantBuffers, startSlot);
        return this;
    }

    /// <summary>
    /// Function to assign a single pixel shader resource view to the draw call.
    /// </summary>
    /// <param name="resourceView">The shader resource view to assign.</param>
    /// <param name="slot">[Optional] The slot used to asign the view.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="slot"/> is less than 0, or greater than/equal to <see cref="GorgonShaderResourceViews.MaximumShaderResourceViewCount"/>.</exception>
    public GorgonStreamOutCallBuilder ShaderResource(GorgonShaderResourceView resourceView, int slot = 0)
    {
        if (slot is < 0 or >= GorgonShaderResourceViews.MaximumShaderResourceViewCount)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_SRV_SLOT_INVALID, GorgonShaderResourceViews.MaximumShaderResourceViewCount));
        }

        _workerCall.D3DState.PsSrvs[slot] = resourceView;
        return this;
    }

    /// <summary>
    /// Function to assign the list of pixel shader resource views to the draw call.
    /// </summary>
    /// <param name="resourceViews">The shader resource views to copy.</param>
    /// <param name="startSlot">[Optional] The starting slot to use when copying the list.</param>
    /// <returns>The fluent builder interface .</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="startSlot"/> is less than 0, or greater than/equal to <see cref="GorgonShaderResourceViews.MaximumShaderResourceViewCount"/>.</exception>
    public GorgonStreamOutCallBuilder ShaderResources(IReadOnlyList<GorgonShaderResourceView> resourceViews, int startSlot = 0)
    {
        if (startSlot is < 0 or >= GorgonShaderResourceViews.MaximumShaderResourceViewCount)
        {
            throw new ArgumentOutOfRangeException(nameof(startSlot), string.Format(Resources.GORGFX_ERR_SRV_SLOT_INVALID, GorgonShaderResourceViews.MaximumShaderResourceViewCount));
        }

        StateCopy.CopySrvs(_workerCall.D3DState.PsSrvs, resourceViews, startSlot);
        return this;
    }

    /// <summary>
    /// Function to assign a single read/write (unordered access) view to the draw call.
    /// </summary>
    /// <param name="resourceView">The shader resource view to assign.</param>
    /// <param name="slot">[Optional] The slot used to asign the view.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="slot"/> is less than 0, or greater than/equal to <see cref="GorgonShaderResourceViews.MaximumShaderResourceViewCount"/>.</exception>
    public GorgonStreamOutCallBuilder ReadWriteView(ref readonly GorgonReadWriteViewBinding resourceView, int slot = 0)
    {
        if (slot is < 0 or >= GorgonShaderResourceViews.MaximumShaderResourceViewCount)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_SRV_SLOT_INVALID, GorgonShaderResourceViews.MaximumShaderResourceViewCount));
        }

        _workerCall.D3DState.ReadWriteViews[slot] = resourceView;
        return this;
    }

    /// <summary>
    /// Function to assign the list of read/write (unordered access) views to the draw call.
    /// </summary>
    /// <param name="resourceViews">The shader resource views to copy.</param>
    /// <param name="startSlot">[Optional] The starting slot to use when copying the list.</param>
    /// <returns>The fluent builder interface .</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="startSlot"/> is less than 0, or greater than/equal to <see cref="GorgonShaderResourceViews.MaximumShaderResourceViewCount"/>.</exception>
    public GorgonStreamOutCallBuilder ReadWriteViews(IReadOnlyList<GorgonReadWriteViewBinding> resourceViews, int startSlot = 0)
    {
        if (startSlot is < 0 or >= GorgonShaderResourceViews.MaximumShaderResourceViewCount)
        {
            throw new ArgumentOutOfRangeException(nameof(startSlot), string.Format(Resources.GORGFX_ERR_SRV_SLOT_INVALID, GorgonShaderResourceViews.MaximumShaderResourceViewCount));
        }

        StateCopy.CopyReadWriteViews(_workerCall.D3DState.ReadWriteViews, resourceViews, startSlot);
        return this;
    }

    /// <summary>
    /// Function to return the draw call.
    /// </summary>
    /// <param name="allocator">The allocator used to create an instance of the object</param>
    /// <returns>The draw call created or updated by this builder.</returns>
    /// <exception cref="GorgonException">Thrown if a <see cref="GorgonVertexShader"/> is not assigned to the <see cref="GorgonPipelineState.VertexShader"/> property with the <see cref="PipelineState(GorgonStreamOutPipelineState)"/> command.</exception>
    /// <remarks>
    /// <para>
    /// Using an <paramref name="allocator"/> can provide different strategies when building draw calls.  If omitted, the draw call will be created using the standard <see langword="new"/> keyword.
    /// </para>
    /// <para>
    /// A custom allocator can be beneficial because it allows us to use a pool for allocating the objects, and thus allows for recycling of objects. This keeps the garbage collector happy by keeping objects
    /// around for as long as we need them, instead of creating objects that can potentially end up in the large object heap or in Gen 2.
    /// </para>
    /// <para>
    /// A stream out call requires that at least a vertex shader be bound. If none is present, then the method will throw an exception.
    /// </para>
    /// </remarks>
    public GorgonStreamOutCall Build(IGorgonAllocator<GorgonStreamOutCall> allocator)
    {
        GorgonStreamOutCall final = allocator is null ? new GorgonStreamOutCall() : allocator.Allocate();
        final.SetupConstantBuffers();
        final.SetupSamplers();
        final.SetupViews();

        if (final.D3DState.VertexBuffers is null)
        {
            final.D3DState.VertexBuffers = new GorgonVertexBufferBindings();
        }

        final.D3DState.VertexBuffers.InputLayout = _workerCall.InputLayout;
        final.D3DState.VertexBuffers[0] = _workerCall.VertexBufferBinding;

        // Copy over the available constants.
        StateCopy.CopyConstantBuffers(final.D3DState.PsConstantBuffers, _workerCall.D3DState.PsConstantBuffers, 0);

        // Copy over samplers.
        StateCopy.CopySamplers(final.D3DState.PsSamplers, _workerCall.D3DState.PsSamplers, 0);

        // Copy over shader resource views.
        StateCopy.CopySrvs(final.D3DState.PsSrvs, _workerCall.D3DState.PsSrvs);

        // Copy over uavs.
        StateCopy.CopyReadWriteViews(final.D3DState.ReadWriteViews, _workerCall.D3DState.ReadWriteViews, 0);

        final.PipelineState = _workerCall.PipelineState;

        return final.PipelineState.VertexShader is null
            ? throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_NO_VERTEX_SHADER)
            : final;
    }

    /// <summary>
    /// Function to reset the builder to the specified draw call state.
    /// </summary>
    /// <param name="drawCall">[Optional] The specified draw call state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    public GorgonStreamOutCallBuilder ResetTo(GorgonStreamOutCall drawCall = null)
    {
        if (drawCall is null)
        {
            return Clear();
        }

        GorgonVertexBufferBinding vertexBufferBinding = drawCall.VertexBufferBinding;
        VertexBuffer(drawCall.InputLayout, in vertexBufferBinding);

        // Copy over the available constants.
        ConstantBuffers(drawCall.D3DState.PsConstantBuffers);
        SamplerStates(drawCall.D3DState.PsSamplers);
        StateCopy.CopySrvs(_workerCall.D3DState.PsSrvs, drawCall.D3DState.PsSrvs);
        ReadWriteViews(drawCall.ReadWriteViews);

        _workerCall.PipelineState = new GorgonStreamOutPipelineState(drawCall.PipelineState.PipelineState);

        // We need to copy the D3D states as well as they won't be updated unless we rebuild the pipeline state.
        _workerCall.D3DState.PipelineState.D3DBlendState = drawCall.D3DState.PipelineState.D3DBlendState;
        _workerCall.D3DState.PipelineState.D3DRasterState = drawCall.D3DState.PipelineState.D3DRasterState;
        _workerCall.D3DState.PipelineState.D3DDepthStencilState = drawCall.D3DState.PipelineState.D3DDepthStencilState;

        return this;
    }

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    public GorgonStreamOutCallBuilder Clear()
    {
        _workerCall.D3DState.VertexBuffers.Clear();

        _workerCall.D3DState.PsConstantBuffers.Clear();
        _workerCall.D3DState.PsSamplers.Clear();
        _workerCall.D3DState.PsSrvs.Clear();
        _workerCall.D3DState.ReadWriteViews.Clear();

        _workerCall.D3DState.PipelineState.Clear();

        return this;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonDrawCallBuilder"/> class.
    /// </summary>
    public GorgonStreamOutCallBuilder()
    {
        _workerCall = new GorgonStreamOutCall
        {
            PipelineState = new GorgonStreamOutPipelineState(new GorgonPipelineState())
        };
        _workerCall.SetupConstantBuffers();
        _workerCall.SetupSamplers();
        _workerCall.SetupViews();
        _workerCall.D3DState.VertexBuffers = new GorgonVertexBufferBindings();
    }
}
