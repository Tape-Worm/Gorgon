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

using System;
using System.Collections.Generic;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Memory;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Common functionality for the a draw call fluent builder.
    /// </summary>
    /// <typeparam name="TB">The type of builder.</typeparam>
    /// <typeparam name="TDc">The type of draw call.</typeparam>
    /// <remarks>
    /// <para>
    /// A draw call is an immutable object that contains all of the state required to render mesh information. For each mesh an application needs to render, an single draw call should be issued via the
    /// <see cref="GorgonGraphics.Submit(GorgonDrawCall, in GorgonColor?, int, int)"/> methods.  
    /// </para>
    /// <para>
    /// State management is handled internally by Gorgon so that duplicate states are not set and thus, performance is not impacted by redundant states.
    /// </para>
    /// <para>
    /// This builder type uses a fluent interface to assemble the draw call, its resources and its <see cref="GorgonPipelineState"/>. 
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics"/>
    public abstract class GorgonDrawCallBuilderCommon<TB, TDc>
        : IGorgonFluentBuilderAllocator<TB, TDc, IGorgonAllocator<TDc>>
        where TB : GorgonDrawCallBuilderCommon<TB, TDc>
        where TDc : GorgonDrawCallCommon
    {
        #region Properties.
        /// <summary>
        /// Property to return the draw call being edited.
        /// </summary>
        protected TDc DrawCall
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create a new draw call.
        /// </summary>
        /// <param name="allocator">The allocator to use when creating draw call objects.</param>
        /// <returns>A new draw call.</returns>
        protected abstract TDc OnCreate(IGorgonAllocator<TDc> allocator);

        /// <summary>
        /// Function to update the properties of the draw call from the working copy to the final copy.
        /// </summary>
        /// <param name="finalCopy">The object representing the finalized copy.</param>
        protected abstract void OnUpdate(TDc finalCopy);

        /// <summary>
        /// Function to reset the properties of the draw call to the draw call passed in.
        /// </summary>
        /// <param name="drawCall">The draw call to copy from.</param>
        /// <returns>The fluent builder interface.</returns>
        protected abstract TB OnResetTo(TDc drawCall);

        /// <summary>
        /// Function to clear the draw call.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        protected abstract TB OnClear();

        /// <summary>
        /// Function to assign a list of samplers to a shader on the pipeline.
        /// </summary>
        /// <param name="shaderType">The type of shader to update.</param>
        /// <param name="samplers">The samplers to assign.</param>
        /// <param name="index">[Optional] The index to use when copying the list.</param>
        /// <returns>The fluent interface for this builder.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> parameter is less than 0, or greater than/equal to <see cref="GorgonSamplerStates.MaximumSamplerStateCount"/>.</exception>
        /// <exception cref="NotSupportedException">Thrown if the <paramref name="shaderType"/> is not valid.</exception>
        /// <remarks>
        /// <para>
        /// <see cref="ShaderType.Compute"/> shaders are not supported in this method will throw an exception.
        /// </para>
        /// </remarks>
        public TB SamplerStates(ShaderType shaderType, IReadOnlyList<GorgonSamplerState> samplers, int index = 0)
        {
            if (index is < 0 or >= GorgonSamplerStates.MaximumSamplerStateCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index), string.Format(Resources.GORGFX_ERR_INVALID_SAMPLER_INDEX, GorgonSamplerStates.MaximumSamplerStateCount));
            }

            switch (shaderType)
            {
                case ShaderType.Pixel:
                    StateCopy.CopySamplers(DrawCall.D3DState.PsSamplers, samplers, index);
                    break;
                case ShaderType.Vertex:
                    StateCopy.CopySamplers(DrawCall.D3DState.VsSamplers, samplers, index);
                    break;
                case ShaderType.Geometry:
                    StateCopy.CopySamplers(DrawCall.D3DState.GsSamplers, samplers, index);
                    break;
                case ShaderType.Domain:
                    StateCopy.CopySamplers(DrawCall.D3DState.DsSamplers, samplers, index);
                    break;
                case ShaderType.Hull:
                    StateCopy.CopySamplers(DrawCall.D3DState.VsSamplers, samplers, index);
                    break;
                default:
                    throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_SHADER_UNKNOWN_TYPE, shaderType));
            }

            return (TB)this;
        }

        /// <summary>
        /// Function to assign a sampler to a shader on the pipeline.
        /// </summary>
        /// <param name="shaderType">The type of shader to update.</param>
        /// <param name="sampler">The sampler to assign.</param>
        /// <param name="index">[Optional] The index of the sampler.</param>
        /// <returns>The fluent interface for this builder.</returns>
        /// <exception cref="NotSupportedException">Thrown if the <paramref name="shaderType"/> is not valid.</exception>
        /// <remarks>
        /// <para>
        /// <see cref="ShaderType.Compute"/> shaders are not supported in this method will throw an exception.
        /// </para>
        /// </remarks>
        public TB SamplerState(ShaderType shaderType, GorgonSamplerStateBuilder sampler, int index = 0) => SamplerState(shaderType, sampler.Build(), index);

        /// <summary>
        /// Function to assign a sampler to a shader on the pipeline.
        /// </summary>
        /// <param name="shaderType">The type of shader to update.</param>
        /// <param name="sampler">The sampler to assign.</param>
        /// <param name="index">[Optional] The index of the sampler.</param>
        /// <returns>The fluent interface for this builder.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> parameter is less than 0, or greater than/equal to <see cref="GorgonSamplerStates.MaximumSamplerStateCount"/>.</exception>
        /// <exception cref="NotSupportedException">Thrown if the <paramref name="shaderType"/> is not valid.</exception>
        /// <remarks>
        /// <para>
        /// <see cref="ShaderType.Compute"/> shaders are not supported in this method will throw an exception.
        /// </para>
        /// </remarks>
        public TB SamplerState(ShaderType shaderType, GorgonSamplerState sampler, int index = 0)
        {
            if (index is < 0 or >= GorgonSamplerStates.MaximumSamplerStateCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index), string.Format(Resources.GORGFX_ERR_INVALID_SAMPLER_INDEX, GorgonSamplerStates.MaximumSamplerStateCount));
            }

            switch (shaderType)
            {
                case ShaderType.Pixel:
                    DrawCall.D3DState.PsSamplers[index] = sampler;
                    break;
                case ShaderType.Vertex:
                    DrawCall.D3DState.VsSamplers[index] = sampler;
                    break;
                case ShaderType.Geometry:
                    DrawCall.D3DState.GsSamplers[index] = sampler;
                    break;
                case ShaderType.Domain:
                    DrawCall.D3DState.DsSamplers[index] = sampler;
                    break;
                case ShaderType.Hull:
                    DrawCall.D3DState.HsSamplers[index] = sampler;
                    break;
                default:
                    throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_SHADER_UNKNOWN_TYPE, shaderType));
            }

            return (TB)this;
        }

        /// <summary>
        /// Function to set the pipeline state for this draw call.
        /// </summary>
        /// <param name="pipelineState">The pipeline state to assign.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pipelineState"/> parameter is <b>null</b>.</exception>
        public TB PipelineState(GorgonPipelineState pipelineState)
        {
            DrawCall.D3DState.PipelineState = pipelineState ?? throw new ArgumentNullException(nameof(pipelineState));
            return (TB)this;
        }

        /// <summary>
        /// Function to set the pipeline state for this draw call.
        /// </summary>
        /// <param name="pipelineState">The pipeline state to assign.</param>
        /// <returns>The fluent builder interface.</returns>
        public TB PipelineState(GorgonPipelineStateBuilder pipelineState) => PipelineState(pipelineState?.Build());

        /// <summary>
        /// Function to set a stream out binding for the draw call.
        /// </summary>
        /// <param name="binding">The stream out binding to use.</param>
        /// <param name="slot">[Optional] The slot for the binding.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> parameter is less than 0, or greater than/equal to <see cref="GorgonStreamOutBindings.MaximumStreamOutCount"/>.</exception>
        public TB StreamOutBuffer(in GorgonStreamOutBinding binding, int slot = 0)
        {
            if (slot is < 0 or >= GorgonStreamOutBindings.MaximumStreamOutCount)
            {
                throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_SO_SLOT_INVALID, GorgonStreamOutBindings.MaximumStreamOutCount));
            }

            if (DrawCall.D3DState.StreamOutBindings is null)
            {
                DrawCall.D3DState.StreamOutBindings = new GorgonStreamOutBindings();
            }

            DrawCall.D3DState.StreamOutBindings[slot] = binding;
            return (TB)this;
        }

        /// <summary>
        /// Function to set a list of stream out bindings for the draw call.
        /// </summary>
        /// <param name="bindings">The stream out bindings to use.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="bindings"/> parameter is larger than the <see cref="GorgonStreamOutBindings.MaximumStreamOutCount"/>.</exception>
        public TB StreamOutBuffers(IReadOnlyList<GorgonStreamOutBinding> bindings)
        {
            if (bindings.Count > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(bindings), string.Format(Resources.GORGFX_ERR_SO_SLOT_INVALID, GorgonStreamOutBindings.MaximumStreamOutCount));
            }

            if (DrawCall.D3DState.StreamOutBindings is null)
            {
                DrawCall.D3DState.StreamOutBindings = new GorgonStreamOutBindings();
            }

            DrawCall.D3DState.StreamOutBindings.Clear();
            if (bindings.Count == 0)
            {
                return (TB)this;
            }

            for (int i = 0; i < bindings.Count; ++i)
            {
                DrawCall.D3DState.StreamOutBindings[i] = bindings[i];
            }
            return (TB)this;
        }

        /// <summary>
        /// Function to set a vertex buffer binding for the draw call.
        /// </summary>
        /// <param name="layout">The input layout to use.</param>
        /// <param name="binding">The vertex buffer binding to set.</param>
        /// <param name="slot">[Optional] The slot for the binding.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="layout"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> parameter is less than 0, or greater than/equal to <see cref="GorgonVertexBufferBindings.MaximumVertexBufferCount"/>.</exception>
        public TB VertexBuffer(GorgonInputLayout layout, in GorgonVertexBufferBinding binding, int slot = 0)
        {
            if (slot is < 0 or >= GorgonVertexBufferBindings.MaximumVertexBufferCount)
            {
                throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_INVALID_VERTEXBUFFER_SLOT, GorgonVertexBufferBindings.MaximumVertexBufferCount));
            }

            if (DrawCall.D3DState.VertexBuffers is null)
            {
                DrawCall.D3DState.VertexBuffers = new GorgonVertexBufferBindings();
            }

            DrawCall.D3DState.VertexBuffers[slot] = binding;
            DrawCall.D3DState.VertexBuffers.InputLayout = layout ?? throw new ArgumentNullException(nameof(layout));
            return (TB)this;
        }

        /// <summary>
        /// Function to set the vertex buffer bindings for the draw call.
        /// </summary>
        /// <param name="layout">The input layout to use.</param>
        /// <param name="bindings">The vertex buffer bindings to set.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="layout"/> parameter is <b>null</b>.</exception>
        public TB VertexBuffers(GorgonInputLayout layout, IReadOnlyList<GorgonVertexBufferBinding> bindings)
        {
            StateCopy.CopyVertexBuffers(DrawCall.D3DState.VertexBuffers, bindings, layout ?? throw new ArgumentNullException(nameof(layout)));
            return (TB)this;
        }

        /// <summary>
        /// Function to set a constant buffer for a specific shader stage.
        /// </summary>
        /// <param name="shaderType">The shader stage to use.</param>
        /// <param name="constantBuffer">The constant buffer to assign.</param>
        /// <param name="slot">The slot for the constant buffer.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="slot"/> is less than 0, or greater than/equal to <see cref="GorgonConstantBuffers.MaximumConstantBufferCount"/>.</exception>
        /// <exception cref="NotSupportedException">Thrown if the <paramref name="shaderType"/> is not valid.</exception>
        /// <remarks>
        /// <para>
        /// <see cref="ShaderType.Compute"/> shaders are not supported in this method will throw an exception.
        /// </para>
        /// </remarks>
        public TB ConstantBuffer(ShaderType shaderType, GorgonConstantBufferView constantBuffer, int slot = 0)
        {
            if (slot is < 0 or >= GorgonConstantBuffers.MaximumConstantBufferCount)
            {
                throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_CBUFFER_SLOT_INVALID, GorgonConstantBuffers.MaximumConstantBufferCount));
            }

            switch (shaderType)
            {
                case ShaderType.Pixel:
                    DrawCall.D3DState.PsConstantBuffers[slot] = constantBuffer;
                    break;
                case ShaderType.Vertex:
                    DrawCall.D3DState.VsConstantBuffers[slot] = constantBuffer;
                    break;
                case ShaderType.Geometry:
                    DrawCall.D3DState.GsConstantBuffers[slot] = constantBuffer;
                    break;
                case ShaderType.Domain:
                    DrawCall.D3DState.DsConstantBuffers[slot] = constantBuffer;
                    break;
                case ShaderType.Hull:
                    DrawCall.D3DState.HsConstantBuffers[slot] = constantBuffer;
                    break;
                default:
                    throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_SHADER_UNKNOWN_TYPE, shaderType));
            }

            return (TB)this;
        }

        /// <summary>
        /// Function to set the constant buffers for a specific shader stage.
        /// </summary>
        /// <param name="shaderType">The shader stage to use.</param>
        /// <param name="constantBuffers">The constant buffers to copy.</param>
        /// <param name="startSlot">[Optional] The starting slot to use when copying the list.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="startSlot"/> is less than 0, or greater than/equal to <see cref="GorgonConstantBuffers.MaximumConstantBufferCount"/>.</exception>
        /// <exception cref="NotSupportedException">Thrown if the <paramref name="shaderType"/> is not valid.</exception>
        /// <remarks>
        /// <para>
        /// <see cref="ShaderType.Compute"/> shaders are not supported in this method will throw an exception.
        /// </para>
        /// </remarks>
        public TB ConstantBuffers(ShaderType shaderType, IReadOnlyList<GorgonConstantBufferView> constantBuffers, int startSlot = 0)
        {
            if (startSlot is < 0 or >= GorgonConstantBuffers.MaximumConstantBufferCount)
            {
                throw new ArgumentOutOfRangeException(nameof(startSlot), string.Format(Resources.GORGFX_ERR_CBUFFER_SLOT_INVALID, GorgonConstantBuffers.MaximumConstantBufferCount));
            }

            switch (shaderType)
            {
                case ShaderType.Pixel:
                    StateCopy.CopyConstantBuffers(DrawCall.D3DState.PsConstantBuffers, constantBuffers, startSlot);
                    break;
                case ShaderType.Vertex:
                    StateCopy.CopyConstantBuffers(DrawCall.D3DState.VsConstantBuffers, constantBuffers, startSlot);
                    break;
                case ShaderType.Geometry:
                    StateCopy.CopyConstantBuffers(DrawCall.D3DState.GsConstantBuffers, constantBuffers, startSlot);
                    break;
                case ShaderType.Domain:
                    StateCopy.CopyConstantBuffers(DrawCall.D3DState.DsConstantBuffers, constantBuffers, startSlot);
                    break;
                case ShaderType.Hull:
                    StateCopy.CopyConstantBuffers(DrawCall.D3DState.HsConstantBuffers, constantBuffers, startSlot);
                    break;
                default:
                    throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_SHADER_UNKNOWN_TYPE, shaderType));
            }

            return (TB)this;
        }

        /// <summary>
        /// Function to assign a single shader resource view to the draw call.
        /// </summary>
        /// <param name="shaderType">The shader stage to use.</param>
        /// <param name="resourceView">The shader resource view to assign.</param>
        /// <param name="slot">[Optional] The slot used to asign the view.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="slot"/> is less than 0, or greater than/equal to <see cref="GorgonShaderResourceViews.MaximumShaderResourceViewCount"/>.</exception>
        /// <exception cref="NotSupportedException">Thrown if the <paramref name="shaderType"/> is not valid.</exception>
        /// <remarks>
        /// <para>
        /// <see cref="ShaderType.Compute"/> shaders are not supported in this method will throw an exception.
        /// </para>
        /// </remarks>
        public TB ShaderResource(ShaderType shaderType, GorgonShaderResourceView resourceView, int slot = 0)
        {
            if (slot is < 0 or >= GorgonShaderResourceViews.MaximumShaderResourceViewCount)
            {
                throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_SRV_SLOT_INVALID, GorgonShaderResourceViews.MaximumShaderResourceViewCount));
            }

            switch (shaderType)
            {
                case ShaderType.Pixel:
                    DrawCall.D3DState.PsSrvs[slot] = resourceView;
                    break;
                case ShaderType.Vertex:
                    DrawCall.D3DState.VsSrvs[slot] = resourceView;
                    break;
                case ShaderType.Geometry:
                    DrawCall.D3DState.GsSrvs[slot] = resourceView;
                    break;
                case ShaderType.Domain:
                    DrawCall.D3DState.DsSrvs[slot] = resourceView;
                    break;
                case ShaderType.Hull:
                    DrawCall.D3DState.HsSrvs[slot] = resourceView;
                    break;
                default:
                    throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_SHADER_UNKNOWN_TYPE, shaderType));
            }

            return (TB)this;
        }

        /// <summary>
        /// Function to assign the list of shader resource views to the draw call.
        /// </summary>
        /// <param name="shaderType">The shader stage to use.</param>
        /// <param name="resourceViews">The shader resource views to copy.</param>
        /// <param name="startSlot">[Optional] The starting slot to use when copying the list.</param>
        /// <returns>The fluent builder interface .</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="startSlot"/> is less than 0, or greater than/equal to <see cref="GorgonShaderResourceViews.MaximumShaderResourceViewCount"/>.</exception>
        /// <exception cref="NotSupportedException">Thrown if the <paramref name="shaderType"/> is not valid.</exception>
        /// <remarks>
        /// <para>
        /// <see cref="ShaderType.Compute"/> shaders are not supported in this method will throw an exception.
        /// </para>
        /// </remarks>
        public TB ShaderResources(ShaderType shaderType, IReadOnlyList<GorgonShaderResourceView> resourceViews, int startSlot = 0)
        {
            if (startSlot is < 0 or >= GorgonShaderResourceViews.MaximumShaderResourceViewCount)
            {
                throw new ArgumentOutOfRangeException(nameof(startSlot), string.Format(Resources.GORGFX_ERR_SRV_SLOT_INVALID, GorgonShaderResourceViews.MaximumShaderResourceViewCount));
            }

            switch (shaderType)
            {
                case ShaderType.Pixel:
                    StateCopy.CopySrvs(DrawCall.D3DState.PsSrvs, resourceViews, startSlot);
                    break;
                case ShaderType.Vertex:
                    StateCopy.CopySrvs(DrawCall.D3DState.VsSrvs, resourceViews, startSlot);
                    break;
                case ShaderType.Geometry:
                    StateCopy.CopySrvs(DrawCall.D3DState.GsSrvs, resourceViews, startSlot);
                    break;
                case ShaderType.Domain:
                    StateCopy.CopySrvs(DrawCall.D3DState.DsSrvs, resourceViews, startSlot);
                    break;
                case ShaderType.Hull:
                    StateCopy.CopySrvs(DrawCall.D3DState.HsSrvs, resourceViews, startSlot);
                    break;
                default:
                    throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_SHADER_UNKNOWN_TYPE, shaderType));
            }

            return (TB)this;
        }

        /// <summary>
        /// Function to assign a single read/write (unordered access) view to the draw call.
        /// </summary>
        /// <param name="resourceView">The shader resource view to assign.</param>
        /// <param name="slot">[Optional] The slot used to asign the view.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="slot"/> is less than 0, or greater than/equal to <see cref="GorgonShaderResourceViews.MaximumShaderResourceViewCount"/>.</exception>
        public TB ReadWriteView(in GorgonReadWriteViewBinding resourceView, int slot = 0)
        {
            if (slot is < 0 or >= GorgonShaderResourceViews.MaximumShaderResourceViewCount)
            {
                throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_ERR_SRV_SLOT_INVALID, GorgonShaderResourceViews.MaximumShaderResourceViewCount));
            }

            DrawCall.D3DState.ReadWriteViews[slot] = resourceView;
            return (TB)this;
        }

        /// <summary>
        /// Function to assign the list of read/write (unordered access) views to the draw call.
        /// </summary>
        /// <param name="resourceViews">The shader resource views to copy.</param>
        /// <param name="startSlot">[Optional] The starting slot to use when copying the list.</param>
        /// <returns>The fluent builder interface .</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="startSlot"/> is less than 0, or greater than/equal to <see cref="GorgonShaderResourceViews.MaximumShaderResourceViewCount"/>.</exception>
        public TB ReadWriteViews(IReadOnlyList<GorgonReadWriteViewBinding> resourceViews, int startSlot = 0)
        {
            if (startSlot is < 0 or >= GorgonShaderResourceViews.MaximumShaderResourceViewCount)
            {
                throw new ArgumentOutOfRangeException(nameof(startSlot), string.Format(Resources.GORGFX_ERR_SRV_SLOT_INVALID, GorgonShaderResourceViews.MaximumShaderResourceViewCount));
            }

            StateCopy.CopyReadWriteViews(DrawCall.D3DState.ReadWriteViews, resourceViews, startSlot);
            return (TB)this;
        }

        /// <summary>
        /// Function to reset the builder to the specified draw call state.
        /// </summary>
        /// <param name="drawCall">[Optional] The specified draw call state to copy.</param>
        /// <returns>The fluent builder interface.</returns>
        public TB ResetTo(TDc drawCall = null)
        {
            if (drawCall is null)
            {
                return Clear();
            }

            VertexBuffers(drawCall.InputLayout, drawCall.VertexBufferBindings);
            StreamOutBuffers(drawCall.StreamOutBufferBindings);

            // Copy over the available constants.
            ConstantBuffers(ShaderType.Pixel, drawCall.D3DState.PsConstantBuffers);
            ConstantBuffers(ShaderType.Vertex, drawCall.D3DState.VsConstantBuffers);
            ConstantBuffers(ShaderType.Geometry, drawCall.D3DState.GsConstantBuffers);
            ConstantBuffers(ShaderType.Domain, drawCall.D3DState.DsConstantBuffers);
            ConstantBuffers(ShaderType.Hull, drawCall.D3DState.HsConstantBuffers);

            SamplerStates(ShaderType.Pixel, drawCall.D3DState.PsSamplers);
            SamplerStates(ShaderType.Vertex, drawCall.D3DState.VsSamplers);
            SamplerStates(ShaderType.Geometry, drawCall.D3DState.GsSamplers);
            SamplerStates(ShaderType.Domain, drawCall.D3DState.DsSamplers);
            SamplerStates(ShaderType.Hull, drawCall.D3DState.HsSamplers);

            StateCopy.CopySrvs(DrawCall.D3DState.PsSrvs, drawCall.D3DState.PsSrvs);
            StateCopy.CopySrvs(DrawCall.D3DState.VsSrvs, drawCall.D3DState.VsSrvs);
            StateCopy.CopySrvs(DrawCall.D3DState.GsSrvs, drawCall.D3DState.GsSrvs);
            StateCopy.CopySrvs(DrawCall.D3DState.DsSrvs, drawCall.D3DState.DsSrvs);
            StateCopy.CopySrvs(DrawCall.D3DState.HsSrvs, drawCall.D3DState.HsSrvs);

            ReadWriteViews(drawCall.D3DState.ReadWriteViews);

            DrawCall.D3DState.PipelineState = new GorgonPipelineState(drawCall.PipelineState);
            // We need to copy the D3D states as well as they won't be updated unless we rebuild the pipeline state.
            DrawCall.D3DState.PipelineState.D3DBlendState = drawCall.D3DState.PipelineState.D3DBlendState;
            DrawCall.D3DState.PipelineState.D3DRasterState = drawCall.D3DState.PipelineState.D3DRasterState;
            DrawCall.D3DState.PipelineState.D3DDepthStencilState = drawCall.D3DState.PipelineState.D3DDepthStencilState;

            return OnResetTo(drawCall);
        }

        /// <summary>
        /// Function to clear the builder to a default state.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        public TB Clear()
        {
            DrawCall.D3DState.VertexBuffers.Clear();
            DrawCall.D3DState.StreamOutBindings.Clear();

            DrawCall.D3DState.PsConstantBuffers.Clear();
            DrawCall.D3DState.VsConstantBuffers.Clear();
            DrawCall.D3DState.GsConstantBuffers.Clear();
            DrawCall.D3DState.HsConstantBuffers.Clear();
            DrawCall.D3DState.DsConstantBuffers.Clear();

            DrawCall.D3DState.PsSamplers.Clear();
            DrawCall.D3DState.VsSamplers.Clear();
            DrawCall.D3DState.GsSamplers.Clear();
            DrawCall.D3DState.DsSamplers.Clear();
            DrawCall.D3DState.HsSamplers.Clear();

            DrawCall.D3DState.PsSrvs.Clear();
            DrawCall.D3DState.VsSrvs.Clear();
            DrawCall.D3DState.GsSrvs.Clear();
            DrawCall.D3DState.DsSrvs.Clear();
            DrawCall.D3DState.HsSrvs.Clear();

            DrawCall.D3DState.ReadWriteViews.Clear();

            DrawCall.D3DState.PipelineState = null;

            return OnClear();
        }

        /// <summary>
        /// Function to return the draw call.
        /// </summary>
        /// <param name="allocator">The allocator used to create an instance of the object</param>
        /// <returns>The draw call created or updated by this builder.</returns>
        /// <exception cref="GorgonException">Thrown if a <see cref="GorgonVertexShader"/> is not assigned to the <see cref="GorgonPipelineState.VertexShader"/> property with the <see cref="PipelineState(GorgonPipelineStateBuilder)"/> command.</exception>
        /// <remarks>
        /// <para>
        /// Using an <paramref name="allocator"/> can provide different strategies when building draw calls.  If omitted, the draw call will be created using the standard <see langword="new"/> keyword.
        /// </para>
        /// <para>
        /// A custom allocator can be beneficial because it allows us to use a pool for allocating the objects, and thus allows for recycling of objects. This keeps the garbage collector happy by keeping objects
        /// around for as long as we need them, instead of creating objects that can potentially end up in the large object heap or in Gen 2.
        /// </para>
        /// <para>
        /// A draw call requires that at least a vertex shader be bound. If none is present, then the method will throw an exception.
        /// </para>
        /// </remarks>
        public TDc Build(IGorgonAllocator<TDc> allocator)
        {
            TDc final = OnCreate(allocator);

            if ((allocator is null)
                || (final.VertexShader.ConstantBuffers is null)
                || (final.PixelShader.Samplers is null)
                || (final.PixelShader.ShaderResources is null))
            {
                final.SetupConstantBuffers();
                final.SetupSamplers();
                final.SetupViews();
            }

            if (final.D3DState.VertexBuffers is null)
            {
                final.D3DState.VertexBuffers = new GorgonVertexBufferBindings();
            }

            if (final.D3DState.StreamOutBindings is null)
            {
                final.D3DState.StreamOutBindings = new GorgonStreamOutBindings();
            }

            StateCopy.CopyVertexBuffers(final.D3DState.VertexBuffers, DrawCall.VertexBufferBindings, DrawCall.InputLayout);
            StateCopy.CopyStreamOutBuffers(final.D3DState.StreamOutBindings, DrawCall.StreamOutBufferBindings);

            // Copy over the shader resources.
            if (DrawCall.D3DState.PipelineState?.PixelShader is not null)
            {
                StateCopy.CopyConstantBuffers(final.D3DState.PsConstantBuffers, DrawCall.D3DState.PsConstantBuffers, 0);
                StateCopy.CopySamplers(final.D3DState.PsSamplers, DrawCall.D3DState.PsSamplers, 0);
                StateCopy.CopySrvs(final.D3DState.PsSrvs, DrawCall.D3DState.PsSrvs);
            }
            else
            {
                final.D3DState.PsConstantBuffers.Clear();
                final.D3DState.PsSamplers.Clear();
                final.D3DState.PsSrvs.Clear();
            }

            if (DrawCall.D3DState.PipelineState?.VertexShader is not null)
            {
                StateCopy.CopyConstantBuffers(final.D3DState.VsConstantBuffers, DrawCall.D3DState.VsConstantBuffers, 0);
                StateCopy.CopySamplers(final.D3DState.VsSamplers, DrawCall.D3DState.VsSamplers, 0);
                StateCopy.CopySrvs(final.D3DState.VsSrvs, DrawCall.D3DState.VsSrvs);
            }
            else
            {
                final.D3DState.VsConstantBuffers.Clear();
                final.D3DState.VsSamplers.Clear();
                final.D3DState.VsSrvs.Clear();
            }

            if (DrawCall.D3DState.PipelineState?.GeometryShader is not null)
            {
                StateCopy.CopyConstantBuffers(final.D3DState.GsConstantBuffers, DrawCall.D3DState.GsConstantBuffers, 0);
                StateCopy.CopySamplers(final.D3DState.GsSamplers, DrawCall.D3DState.GsSamplers, 0);
                StateCopy.CopySrvs(final.D3DState.GsSrvs, DrawCall.D3DState.GsSrvs);
            }
            else
            {
                final.D3DState.GsConstantBuffers.Clear();
                final.D3DState.GsSamplers.Clear();
                final.D3DState.GsSrvs.Clear();
            }

            if (DrawCall.D3DState.PipelineState?.DomainShader is not null)
            {
                StateCopy.CopyConstantBuffers(final.D3DState.DsConstantBuffers, DrawCall.D3DState.DsConstantBuffers, 0);
                StateCopy.CopySamplers(final.D3DState.DsSamplers, DrawCall.D3DState.DsSamplers, 0);
                StateCopy.CopySrvs(final.D3DState.DsSrvs, DrawCall.D3DState.DsSrvs);
            }
            else
            {
                final.D3DState.DsConstantBuffers.Clear();
                final.D3DState.DsSamplers.Clear();
                final.D3DState.DsSrvs.Clear();
            }

            if (DrawCall.D3DState.PipelineState?.HullShader is not null)
            {
                StateCopy.CopyConstantBuffers(final.D3DState.HsConstantBuffers, DrawCall.D3DState.HsConstantBuffers, 0);
                StateCopy.CopySamplers(final.D3DState.HsSamplers, DrawCall.D3DState.HsSamplers, 0);
                StateCopy.CopySrvs(final.D3DState.HsSrvs, DrawCall.D3DState.HsSrvs);
            }
            else
            {
                final.D3DState.HsConstantBuffers.Clear();
                final.D3DState.HsSamplers.Clear();
                final.D3DState.HsSrvs.Clear();
            }

            // Copy over unordered access views.
            StateCopy.CopyReadWriteViews(final.D3DState.ReadWriteViews, DrawCall.D3DState.ReadWriteViews, 0);

            final.D3DState.PipelineState = DrawCall.PipelineState;

            // Copy the cached states.
            final.PipelineState.D3DBlendState = DrawCall.PipelineState.D3DBlendState;
            final.PipelineState.D3DDepthStencilState = DrawCall.PipelineState.D3DDepthStencilState;
            final.PipelineState.D3DRasterState = DrawCall.PipelineState.D3DRasterState;

            OnUpdate(final);

            return final.PipelineState.VertexShader is null
                ? throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_NO_VERTEX_SHADER)
                : final;
        }

        /// <summary>
        /// Function to return the draw call.
        /// </summary>
        /// <returns>The draw call created or updated by this builder.</returns>
        /// <exception cref="GorgonException">Thrown if a <see cref="GorgonVertexShader"/> is not assigned to the <see cref="GorgonPipelineState.VertexShader"/> property with the <see cref="PipelineState(GorgonPipelineStateBuilder)"/> command.</exception>
        public TDc Build() => Build(null);
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonDrawCallBuilder"/> class.
        /// </summary>
        /// <param name="drawCall">The worker draw call.</param>
        private protected GorgonDrawCallBuilderCommon(TDc drawCall)
        {
            DrawCall = drawCall;
            DrawCall.SetupConstantBuffers();
            DrawCall.SetupSamplers();
            DrawCall.SetupViews();
            DrawCall.D3DState.PsSamplers[0] = GorgonSamplerState.Default;
            DrawCall.D3DState.VertexBuffers = new GorgonVertexBufferBindings();
            DrawCall.D3DState.StreamOutBindings = new GorgonStreamOutBindings();
            DrawCall.D3DState.PipelineState = new GorgonPipelineState();
        }
        #endregion
    }
}
