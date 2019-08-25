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
// Created: June 13, 2018 4:17:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers.Properties;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A fluent interface used to create shader states for use with a <see cref="Gorgon2DBatchState"/>.
    /// </summary>
    /// <typeparam name="T">The type of shader.</typeparam>
    /// <remarks>
    /// <para>
    /// This builder creates shader states for state information wrapped around shaders based on <see cref="GorgonShader"/>. States built by this type are used for passing shader programs and related states 
    /// to the <see cref="Gorgon2DBatchState"/> when setting up a batch render via <see cref="Gorgon2D.Begin(Gorgon2DBatchState, IGorgon2DCamera)"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonPipelineState"/>
    /// <seealso cref="GorgonDrawCall"/>
    /// <seealso cref="Gorgon2DBatchState"/>
    /// <seealso cref="GorgonShader"/>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", 
        Justification = "Seriously?  This doesn't even KEEP any IDisposable items, and there's nothing that can be disposed in the object itself. This suggestion is useless.")]
    public class Gorgon2DShaderStateBuilder<T>
        : IGorgonFluentBuilder<Gorgon2DShaderStateBuilder<T>, Gorgon2DShaderState<T>>
        where T : GorgonShader
    {
        #region Variables.
        // The shader to build.
        private readonly Gorgon2DShaderState<T> _workingShader = new Gorgon2DShaderState<T>();
        #endregion

        #region Methods.
        /// <summary>
        /// Function to copy a list of items.
        /// </summary>
        /// <typeparam name="TA">The type of array element.</typeparam>
        /// <param name="dest">The destination list.</param>
        /// <param name="src">The source list.</param>
        /// <param name="startSlot">The starting index.</param>
        private static void Copy<TA>(GorgonArray<TA> dest, IReadOnlyList<TA> src, int startSlot)
            where TA : IEquatable<TA>
        {
            dest.Clear();

            if (src == null)
            {
                return;
            }

            int length = src.Count.Min(dest.Length - startSlot);

            for (int i = 0; i < length; ++i)
            {
                dest[i + startSlot] = src[i];
            }
        }

        /// <summary>
        /// Function to assign the current shader.
        /// </summary>
        /// <param name="shader">The shader to assign.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <remarks>
        /// <para>
        /// This method is used to share an existing <see cref="GorgonShader"/> amongst many 2D shader instances. Shared shader objects are not owned by the resulting <see cref="Gorgon2DShaderState{T}"/>, 
        /// and must have their lifetimes managed by the user.
        /// </para>
        /// </remarks>
        public Gorgon2DShaderStateBuilder<T> Shader(T shader)
        {
            _workingShader.Shader = shader;
            return this;
        }

        /// <summary>
        /// Function to set a constant buffer for a specific shader stage.
        /// </summary>
        /// <param name="constantBuffer">The constant buffer to assign.</param>
        /// <param name="slot">The slot for the constant buffer.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="slot"/> is less than 0, or greater than/equal to <see cref="GorgonConstantBuffers.MaximumConstantBufferCount"/>.</exception>
        public Gorgon2DShaderStateBuilder<T> ConstantBuffer(GorgonConstantBufferView constantBuffer, int slot = 0)
        {
            if ((slot < 0) || (slot >= GorgonConstantBuffers.MaximumConstantBufferCount))
            {
                throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GOR2D_ERR_CBUFFER_SLOT_INVALID, GorgonConstantBuffers.MaximumConstantBufferCount));
            }

            _workingShader.RwConstantBuffers[slot] = constantBuffer;
            return this;
        }

        /// <summary>
        /// Function to set the constant buffers for a specific shader stage.
        /// </summary>
        /// <param name="constantBuffers">The constant buffers to copy.</param>
        /// <param name="startSlot">[Optional] The starting slot to use when copying the list.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="startSlot"/> is less than 0, or greater than/equal to <see cref="GorgonConstantBuffers.MaximumConstantBufferCount"/>.</exception>
        public Gorgon2DShaderStateBuilder<T> ConstantBuffers(IReadOnlyList<GorgonConstantBufferView> constantBuffers, int startSlot = 0)
        {
            if ((startSlot < 0) || (startSlot >= GorgonConstantBuffers.MaximumConstantBufferCount))
            {
                throw new ArgumentOutOfRangeException(nameof(startSlot), string.Format(Resources.GOR2D_ERR_CBUFFER_SLOT_INVALID, GorgonConstantBuffers.MaximumConstantBufferCount));
            }

            Copy(_workingShader.RwConstantBuffers, constantBuffers, startSlot);
            return this;
        }

        /// <summary>
        /// Function to assign a single shader resource view to the draw call.
        /// </summary>
        /// <param name="resourceView">The shader resource view to assign.</param>
        /// <param name="slot">[Optional] The slot used to asign the view.</param>
        /// <returns>The fluent builder interface.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="slot"/> is less than 0, or greater than/equal to 16.</exception>
        public Gorgon2DShaderStateBuilder<T> ShaderResource(GorgonShaderResourceView resourceView, int slot = 0)
        {
            if ((slot < 0) || (slot >= 16))
            {
                throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GOR2D_ERR_SRV_SLOT_INVALID, 16));
            }

            _workingShader.RwSrvs[slot] = resourceView;
            return this;
        }

        /// <summary>
        /// Function to assign the list of shader resource views to the shader.
        /// </summary>
        /// <param name="resourceViews">The shader resource views to copy.</param>
        /// <param name="startSlot">[Optional] The starting slot to use when copying the list.</param>
        /// <returns>The fluent builder interface .</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="startSlot"/> is less than 0, or greater than/equal to 16.</exception>
        public Gorgon2DShaderStateBuilder<T> ShaderResources(IReadOnlyList<GorgonShaderResourceView> resourceViews, int startSlot = 0)
        {
            if ((startSlot < 0) || (startSlot >= 16))
            {
                throw new ArgumentOutOfRangeException(nameof(startSlot), string.Format(Resources.GOR2D_ERR_SRV_SLOT_INVALID, 16));
            }

            Copy(_workingShader.RwSrvs, resourceViews, startSlot);

            return this;
        }

        /// <summary>
        /// Function to assign a list of samplers to a shader on the pipeline.
        /// </summary>
        /// <param name="samplers">The samplers to assign.</param>
        /// <param name="index">[Optional] The starting index to use when copying.</param>
        /// <returns>The fluent interface for this builder.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> parameter is less than 0, or greater than/equal to <see cref="GorgonSamplerStates.MaximumSamplerStateCount"/>.</exception>
        public Gorgon2DShaderStateBuilder<T> SamplerStates(IReadOnlyList<GorgonSamplerState> samplers, int index = 0)
        {
            if ((index < 0) || (index >= GorgonSamplerStates.MaximumSamplerStateCount))
            {
                throw new ArgumentOutOfRangeException(nameof(index), string.Format(Resources.GOR2D_ERR_INVALID_SAMPLER_INDEX, GorgonSamplerStates.MaximumSamplerStateCount));
            }

            Copy(_workingShader.RwSamplers, samplers, 0);
            return this;
        }

        /// <summary>
        /// Function to assign a sampler to a shader on the pipeline.
        /// </summary>
        /// <param name="sampler">The sampler to assign.</param>
        /// <param name="index">[Optional] The index of the sampler.</param>
        /// <returns>The fluent interface for this builder.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> parameter is less than 0, or greater than/equal to <see cref="GorgonSamplerStates.MaximumSamplerStateCount"/>.</exception>
        public Gorgon2DShaderStateBuilder<T> SamplerState(GorgonSamplerStateBuilder sampler, int index = 0) => SamplerState(sampler.Build(), index);

        /// <summary>
        /// Function to assign a sampler to a shader on the pipeline.
        /// </summary>
        /// <param name="sampler">The sampler to assign.</param>
        /// <param name="index">[Optional] The index of the sampler.</param>
        /// <returns>The fluent interface for this builder.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> parameter is less than 0, or greater than/equal to <see cref="GorgonSamplerStates.MaximumSamplerStateCount"/>.</exception>
        public Gorgon2DShaderStateBuilder<T> SamplerState(GorgonSamplerState sampler, int index = 0)
        {
            if ((index < 0) || (index >= GorgonSamplerStates.MaximumSamplerStateCount))
            {
                throw new ArgumentOutOfRangeException(nameof(index), string.Format(Resources.GOR2D_ERR_INVALID_SAMPLER_INDEX, GorgonSamplerStates.MaximumSamplerStateCount));
            }

            _workingShader.RwSamplers[index] = sampler;
            return this;
        }

        /// <summary>
        /// Function to return the object.
        /// </summary>
        /// <returns>The object created or updated by this builder.</returns>
        public Gorgon2DShaderState<T> Build()
        {
            var shader = new Gorgon2DShaderState<T>
                         {
                             Shader = _workingShader.Shader
                         };

            Copy(shader.RwConstantBuffers, _workingShader.RwConstantBuffers, 0);
            Copy(shader.RwSrvs, _workingShader.RwSrvs, 0);
            Copy(shader.RwSamplers, _workingShader.RwSamplers, 0);

            return shader;
        }

        /// <summary>
        /// Function to clear the builder to a default state.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        public Gorgon2DShaderStateBuilder<T> Clear()
        {
            _workingShader.RwConstantBuffers.Clear();
            _workingShader.RwSamplers.Clear();
            _workingShader.RwSrvs.Clear();
            _workingShader.Shader = null;

            return this;
        }

        /// <summary>
        /// Function to reset the builder to the specified object state.
        /// </summary>
        /// <param name="builderObject">[Optional] The specified object state to copy.</param>
        /// <returns>The fluent builder interface.</returns>
        public Gorgon2DShaderStateBuilder<T> ResetTo(Gorgon2DShaderState<T> builderObject = null)
        {
            if (builderObject == null)
            {
                Clear();
                return this;
            }

            Copy(_workingShader.RwConstantBuffers, builderObject.RwConstantBuffers, 0);
            Copy(_workingShader.RwSrvs, builderObject.RwSrvs, 0);
            Copy(_workingShader.RwSamplers, builderObject.RwSamplers, 0);
            _workingShader.Shader = builderObject.Shader;

            return this;
        }
        #endregion
    }
}
