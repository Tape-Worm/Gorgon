
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
// Created: June 5, 2018 1:00:31 PM
// 


using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Memory;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A builder used to create <see cref="GorgonDispatchCall"/> objects
/// </summary>
/// <seealso cref="GorgonDispatchCall"/>
public class GorgonDispatchCallBuilder
    : IGorgonFluentBuilderAllocator<GorgonDispatchCallBuilder, GorgonDispatchCall, IGorgonAllocator<GorgonDispatchCall>>
{

    // The dispatch call being edited.
    private readonly GorgonDispatchCall _worker;



    /// <summary>
    /// Function to assign a list of samplers to a compute shader on the pipeline.
    /// </summary>
    /// <param name="samplers">The samplers to assign.</param>
    /// <param name="index">[Optional] The starting index to use when copying the list.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> parameter is less than 0, or greater than/equal to <see cref="GorgonSamplerStates.MaximumSamplerStateCount"/>.</exception>
    public GorgonDispatchCallBuilder SamplerStates(IReadOnlyList<GorgonSamplerState> samplers, int index = 0)
    {
        if (index is < 0 or >= GorgonSamplerStates.MaximumSamplerStateCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index), string.Format(Resources.GORGFX_ERR_INVALID_SAMPLER_INDEX, GorgonSamplerStates.MaximumSamplerStateCount));
        }

        StateCopy.CopySamplers(_worker.D3DState.CsSamplers, samplers, index);
        return this;
    }

    /// <summary>
    /// Function to assign a sampler to a compute shader on the pipeline.
    /// </summary>
    /// <param name="sampler">The sampler to assign.</param>
    /// <param name="index">[Optional] The index of the sampler.</param>
    /// <returns>The fluent interface for this builder.</returns>
    public GorgonDispatchCallBuilder SamplerState(GorgonSamplerStateBuilder sampler, int index = 0) => SamplerState(sampler.Build(), index);

    /// <summary>
    /// Function to assign a sampler to a shader on the pipeline.
    /// </summary>
    /// <param name="sampler">The sampler to assign.</param>
    /// <param name="index">[Optional] The index of the sampler.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> parameter is less than 0, or greater than/equal to <see cref="GorgonSamplerStates.MaximumSamplerStateCount"/>.</exception>
    public GorgonDispatchCallBuilder SamplerState(GorgonSamplerState sampler, int index = 0)
    {
        if (index is < 0 or >= GorgonSamplerStates.MaximumSamplerStateCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index), string.Format(Resources.GORGFX_ERR_INVALID_SAMPLER_INDEX, GorgonSamplerStates.MaximumSamplerStateCount));
        }

        _worker.D3DState.CsSamplers[index] = sampler;
        return this;
    }

    /// <summary>
    /// Function to set a constant buffer for a compute shader stage.
    /// </summary>
    /// <param name="constantBuffer">The constant buffer to assign.</param>
    /// <param name="slot">The slot for the constant buffer.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="slot"/> is less than 0, or greater than/equal to <see cref="GorgonConstantBuffers.MaximumConstantBufferCount"/>.</exception>
    public GorgonDispatchCallBuilder ConstantBuffer(GorgonConstantBufferView constantBuffer, int slot = 0)
    {
        if (slot is < 0 or >= GorgonConstantBuffers.MaximumConstantBufferCount)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_CBUFFER_SLOT_INVALID, 0));
        }

        _worker.D3DState.CsConstantBuffers[slot] = constantBuffer;
        return this;
    }

    /// <summary>
    /// Function to set the constant buffers for a compute shader stage.
    /// </summary>
    /// <param name="constantBuffers">The constant buffers to copy.</param>
    /// <param name="startSlot">[Optional] The starting slot to use when copying the list.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="startSlot"/> is less than 0, or greater than/equal to <see cref="GorgonConstantBuffers.MaximumConstantBufferCount"/>.</exception>
    public GorgonDispatchCallBuilder ConstantBuffers(IReadOnlyList<GorgonConstantBufferView> constantBuffers, int startSlot = 0)
    {
        if (startSlot is < 0 or >= GorgonConstantBuffers.MaximumConstantBufferCount)
        {
            throw new ArgumentOutOfRangeException(nameof(startSlot), string.Format(Resources.GORGFX_ERR_CBUFFER_SLOT_INVALID, GorgonConstantBuffers.MaximumConstantBufferCount));
        }

        StateCopy.CopyConstantBuffers(_worker.D3DState.CsConstantBuffers, constantBuffers, startSlot);
        return this;
    }

    /// <summary>
    /// Function to assign a single shader resource view to the dispatch call.
    /// </summary>
    /// <param name="resourceView">The shader resource view to assign.</param>
    /// <param name="slot">[Optional] The slot used to asign the view.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="slot"/> is less than 0, or greater than/equal to <see cref="GorgonShaderResourceViews.MaximumShaderResourceViewCount"/>.</exception>
    public GorgonDispatchCallBuilder ShaderResource(GorgonShaderResourceView resourceView, int slot = 0)
    {
        if (slot is < 0 or >= GorgonShaderResourceViews.MaximumShaderResourceViewCount)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_SRV_SLOT_INVALID, GorgonShaderResourceViews.MaximumShaderResourceViewCount));
        }

        _worker.D3DState.CsSrvs[slot] = resourceView;
        return this;
    }

    /// <summary>
    /// Function to assign the list of shader resource views to the dispatch call.
    /// </summary>
    /// <param name="resourceViews">The shader resource views to copy.</param>
    /// <param name="startSlot">[Optional] The starting slot to use when copying the list.</param>
    /// <returns>The fluent builder interface .</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="startSlot"/> is less than 0, or greater than/equal to <see cref="GorgonShaderResourceViews.MaximumShaderResourceViewCount"/>.</exception>
    public GorgonDispatchCallBuilder ShaderResources(IReadOnlyList<GorgonShaderResourceView> resourceViews, int startSlot = 0)
    {
        if (startSlot is < 0 or >= GorgonShaderResourceViews.MaximumShaderResourceViewCount)
        {
            throw new ArgumentOutOfRangeException(nameof(startSlot), string.Format(Resources.GORGFX_ERR_SRV_SLOT_INVALID, GorgonShaderResourceViews.MaximumShaderResourceViewCount));
        }

        StateCopy.CopySrvs(_worker.D3DState.CsSrvs, resourceViews, startSlot);
        return this;
    }

    /// <summary>
    /// Function to assign a single read/write (unordered access) view to the dispatch call.
    /// </summary>
    /// <param name="resourceView">The shader resource view to assign.</param>
    /// <param name="slot">[Optional] The slot used to asign the view.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="slot"/> is less than 0, or greater than/equal to <see cref="GorgonShaderResourceViews.MaximumShaderResourceViewCount"/>.</exception>
    public GorgonDispatchCallBuilder ReadWriteView(in GorgonReadWriteViewBinding resourceView, int slot = 0)
    {
        if (slot is < 0 or >= GorgonShaderResourceViews.MaximumShaderResourceViewCount)
        {
            throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_SRV_SLOT_INVALID, GorgonShaderResourceViews.MaximumShaderResourceViewCount));
        }

        _worker.D3DState.CsReadWriteViews[slot] = resourceView;
        return this;
    }

    /// <summary>
    /// Function to assign the list of read/write (unordered access) views to the dispatch call.
    /// </summary>
    /// <param name="resourceViews">The shader resource views to copy.</param>
    /// <param name="startSlot">[Optional] The starting slot to use when copying the list.</param>
    /// <returns>The fluent builder interface .</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="startSlot"/> is less than 0, or greater than/equal to <see cref="GorgonShaderResourceViews.MaximumShaderResourceViewCount"/>.</exception>
    public GorgonDispatchCallBuilder ReadWriteViews(IReadOnlyList<GorgonReadWriteViewBinding> resourceViews, int startSlot = 0)
    {
        if (startSlot is < 0 or >= GorgonShaderResourceViews.MaximumShaderResourceViewCount)
        {
            throw new ArgumentOutOfRangeException(nameof(startSlot), string.Format(Resources.GORGFX_ERR_SRV_SLOT_INVALID, GorgonShaderResourceViews.MaximumShaderResourceViewCount));
        }

        StateCopy.CopyReadWriteViews(_worker.D3DState.CsReadWriteViews, resourceViews, startSlot);
        return this;
    }

    /// <summary>
    /// Function to assign a compute shader to the call.
    /// </summary>
    /// <param name="shader">The shader to assign.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="shader"/> parameter is <b>null</b>.</exception>
    public GorgonDispatchCallBuilder ComputeShader(GorgonComputeShader shader)
    {
        _worker.D3DState.ComputeShader = shader ?? throw new ArgumentNullException(nameof(shader));
        return this;
    }

    /// <summary>
    /// Function to return the dispatch call.
    /// </summary>
    /// <param name="allocator">The allocator used to create an instance of the object</param>
    /// <returns>The dispatch call created or updated by this builder.</returns>
    /// <exception cref="GorgonException">Thrown if a <see cref="GorgonComputeShader"/> is not assigned to the <see cref="GorgonComputeShader"/> property with the <see cref="ComputeShader"/> command.</exception>
    /// <remarks>
    /// <para>
    /// Using an <paramref name="allocator"/> can provide different strategies when building dispatch calls.  If omitted, the dispatch call will be created using the standard <see langword="new"/> keyword.
    /// </para>
    /// <para>
    /// A custom allocator can be beneficial because it allows us to use a pool for allocating the objects, and thus allows for recycling of objects. This keeps the garbage collector happy by keeping objects
    /// around for as long as we need them, instead of creating objects that can potentially end up in the large object heap or in Gen 2.
    /// </para>
    /// <para>
    /// A dispatch call requires that at least a vertex shader be bound. If none is present, then the method will throw an exception.
    /// </para>
    /// </remarks>
    public GorgonDispatchCall Build(IGorgonAllocator<GorgonDispatchCall> allocator)
    {
        var final = new GorgonDispatchCall();
        final.Setup();

        // Copy over the available constants.
        StateCopy.CopyConstantBuffers(final.D3DState.CsConstantBuffers, _worker.D3DState.CsConstantBuffers, 0);

        // Copy over samplers.
        StateCopy.CopySamplers(final.D3DState.CsSamplers, _worker.D3DState.CsSamplers, 0);

        // Copy over shader resource views.
        (int _, int _) = _worker.D3DState.CsSrvs.GetDirtyItems();

        StateCopy.CopySrvs(final.D3DState.CsSrvs, _worker.D3DState.CsSrvs);

        // Copy over unordered access views.
        StateCopy.CopyReadWriteViews(final.D3DState.CsReadWriteViews, _worker.D3DState.CsReadWriteViews, 0);

        if (_worker.D3DState.ComputeShader is null)
        {
            throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_NO_COMPUTE_SHADER);
        }

        final.D3DState.ComputeShader = _worker.D3DState.ComputeShader;

        return final;
    }

    /// <summary>
    /// Function to return the dispatch call.
    /// </summary>
    /// <returns>The dispatch call created or updated by this builder.</returns>
    /// <exception cref="GorgonException">Thrown if a <see cref="GorgonComputeShader"/> is not assigned to the <see cref="GorgonComputeShader"/> property with the <see cref="ComputeShader"/> command.</exception>
    public GorgonDispatchCall Build() => Build(null);

    /// <summary>
    /// Function to reset the builder to the specified dispatch call state.
    /// </summary>
    /// <param name="dispatchCall">[Optional] The specified dispatch call state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    public GorgonDispatchCallBuilder ResetTo(GorgonDispatchCall dispatchCall = null)
    {
        if (dispatchCall is null)
        {
            return Clear();
        }

        // Copy over the available constants.
        ConstantBuffers(dispatchCall.D3DState.CsConstantBuffers);
        SamplerStates(dispatchCall.D3DState.CsSamplers);
        ShaderResources(dispatchCall.D3DState.CsSrvs);
        ReadWriteViews(dispatchCall.D3DState.CsReadWriteViews);

        _worker.D3DState.ComputeShader = dispatchCall.ComputeShader;

        return this;
    }

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    public GorgonDispatchCallBuilder Clear()
    {
        _worker.D3DState.CsConstantBuffers.Clear();
        _worker.D3DState.CsSamplers.Clear();
        _worker.D3DState.CsSrvs.Clear();
        _worker.D3DState.CsReadWriteViews.Clear();
        _worker.D3DState.ComputeShader = null;

        return this;
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonDispatchCallBuilder"/> class.
    /// </summary>
    public GorgonDispatchCallBuilder()
    {
        _worker = new GorgonDispatchCall();
        _worker.Setup();
    }

}
