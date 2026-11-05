// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: July 20, 2026 6:37:57 PM
//

using System;
using System.Runtime.CompilerServices;
using Gorgon.Core;

namespace Gorgon.Graphics.Core.Codecs;

/// <summary>
/// A graphics pipeline state object cache codec base class containing common functionality used to define codecs for storing and loading graphics pipeline state object cache data.
/// </summary>
/// <param name="graphics">The graphics interface that is associated with the codec.</param>
/// <remarks>
/// <para type="preamble">
/// Pipeline state object codecs allow for persisting pipeline states and their associated <see cref="GorgonShader"/> objects to a file to be loaded at a later time. This allows applications to build a set of 
/// <see cref="GorgonGraphicsPso"/> objects once, and reuse them as long as the GPU, its driver, and the version of Gorgon do not change.
/// </para>
/// <inheritdoc cref="GorgonCodecPsoCacheCommon{TFactory}" path="/remarks/para[@type='common']"/>
/// </remarks>
/// <seealso cref="GorgonGraphicsPso"/>
/// <seealso cref="GorgonGraphicsPsoFactory"/>
/// <seealso cref="GorgonShader"/>
public abstract class GorgonCodecGraphicsPsoCacheCommon(GorgonGraphics graphics)
    : GorgonCodecPsoCacheCommon<GorgonGraphicsPsoFactory>(graphics)
{
    /// <summary>
    /// Property to return the builder used to define the state for the pipeline state objects read by the codec.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="GorgonGraphicsPso"/> cannot be created directly. Implementors will assign the state read from the stream to this builder, and then pass it to the 
    /// <see cref="BuildAndCachePso(string, byte[], GorgonShader, GorgonGraphicsPsoFactory)"/> method to create the <see cref="GorgonGraphicsPso"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPsoBuilder"/>
    /// <seealso cref="GorgonGraphicsPso"/>
    protected GorgonGraphicsPsoBuilder PsoBuilder
    {
        get;
    } = new(graphics);

    /// <summary>
    /// Function to build a <see cref="GorgonGraphicsPso"/> using cached data.
    /// </summary>
    /// <param name="name">The name of the PSO.</param>
    /// <param name="psoBlob">The compiled binary data for the PSO that was stored in the cache data, or an empty array if no compiled data is available.</param>
    /// <param name="vertexShader">The required vertex shader for the PSO.</param>
    /// <param name="cache">The cache object that will contain the newly created PSO, or the existing PSO in the cache if one with the same name exists.</param>
    /// <returns>The newly created <see cref="GorgonGraphicsPso"/>, or the existing <see cref="GorgonGraphicsPso"/> in the <paramref name="cache"/> with the same <paramref name="name"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="name"/> is empty.</exception>
    /// <exception cref="GorgonException">Thrown if the pipeline state defined in the <see cref="PsoBuilder"/> is not valid, or is not supported by the video adapter.</exception>
    /// <remarks>
    /// <para>
    /// Implementors <b>must</b> use this method to create the <see cref="GorgonGraphicsPso"/> after its state has been read from the stream and assigned to the <see cref="PsoBuilder"/>. A 
    /// <see cref="GorgonGraphicsPso"/> cannot be created directly, so this method must be used for Gorgon to populate the <see cref="GorgonGraphicsPso"/> with the correct information.
    /// </para>
    /// <para>
    /// If the <paramref name="psoBlob"/> cannot be used by the current video adapter or driver, then it is discarded, and the <see cref="GorgonGraphicsPso"/> is compiled from its state instead.
    /// </para>
    /// <para>
    /// If the <paramref name="cache"/> already contains a PSO with the same <paramref name="name"/>, then the existing PSO is returned, and the state in the <see cref="PsoBuilder"/> is ignored. If the 
    /// existing PSO has different settings than the <see cref="PsoBuilder"/>, then a warning will be written to the log.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPsoBuilder"/>
    /// <seealso cref="GorgonGraphicsPso"/>
    /// <seealso cref="GorgonGraphicsPsoFactory"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected GorgonGraphicsPso BuildAndCachePso(string name, byte[] psoBlob, GorgonShader vertexShader, GorgonGraphicsPsoFactory cache) => cache.CreateOrGetPso(name, vertexShader, PsoBuilder, psoBlob);

    /// <summary>
    /// Function to retrieve the <see cref="GorgonShader"/> object from the cache that matches the shader blob loaded from the stream.
    /// </summary>
    /// <param name="cache">The cache to evaluate.</param>
    /// <param name="shaderBlobData">The binary data for the shader.</param>
    /// <returns>The <see cref="GorgonShader"/> if one was found matching the data, or <b>null</b> if no shader was in the cache.</returns>
    /// <remarks>
    /// <para>
    /// Use this method to determine if a <see cref="GorgonShader"/> with the same <paramref name="shaderBlobData"/> already exists in the cache. This avoids loading the shader data again if the cache already 
    /// has the same shader.
    /// </para>
    /// <para>
    /// This method should be called prior to reading any shader data in the cache file. If the cache file does not contain shader data (i.e. it's stored externally), then the user can load in the binary data 
    /// for the shader from their preferred storage and pass it through the <paramref name="shaderBlobData"/> parameter. The <see cref="GorgonShader.ShaderData"/> property on the <see cref="GorgonShader"/> 
    /// contains the shader binary data in all cases.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsPsoFactory"/>
    /// <seealso cref="GorgonShader"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static GorgonShader? GetShaderInCache(GorgonGraphicsPsoFactory cache, ReadOnlySpan<byte> shaderBlobData)
    {
        int hashCode = GorgonShader.CreateNewShaderHashCode(shaderBlobData);
        return cache.FindShaderWithSignature(hashCode, shaderBlobData);
    }

    /// <inheritdoc/>
    private protected sealed override GorgonGraphicsPsoFactory CreateFactory() => new(Graphics);

    /// <inheritdoc/>
    private protected sealed override int GetPsoCount(GorgonGraphicsPsoFactory factory) => factory.PipelineStateObjects.Count;
}
