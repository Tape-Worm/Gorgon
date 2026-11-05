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
// Created: November 4, 2026 3:36:36 PM
//

using System;
using System.Runtime.CompilerServices;
using Gorgon.Core;

namespace Gorgon.Graphics.Core.Codecs;

/// <summary>
/// A compute pipeline state object cache codec base class containing common functionality used to define codecs for storing and loading compute pipeline state object cache data.
/// </summary>
/// <param name="graphics">The graphics interface that is associated with the codec.</param>
/// <remarks>
/// <para type="preamble">
/// Pipeline state object codecs allow for persisting pipeline states and their associated <see cref="GorgonShader"/> objects to a file to be loaded at a later time. This allows applications to build a set of 
/// <see cref="GorgonComputePso"/> objects once, and reuse them as long as the GPU, its driver, and the version of Gorgon do not change.
/// </para>
/// <inheritdoc cref="GorgonCodecPsoCacheCommon{TFactory}" path="/remarks/para[@type='common']"/>
/// </remarks>
/// <seealso cref="GorgonComputePso"/>
/// <seealso cref="GorgonComputePsoFactory"/>
/// <seealso cref="GorgonShader"/>
public abstract class GorgonCodecComputePsoCacheCommon(GorgonGraphics graphics)
    : GorgonCodecPsoCacheCommon<GorgonComputePsoFactory>(graphics)
{
    /// <summary>
    /// Function to build a <see cref="GorgonComputePso"/> using cached data.
    /// </summary>
    /// <param name="name">The name of the PSO.</param>
    /// <param name="psoBlob">The compiled binary data for the PSO that was stored in the cache data, or an empty array if no compiled data is available.</param>
    /// <param name="computeShader">The compute shader for the PSO.</param>
    /// <param name="cache">The cache object that will contain the newly created PSO, or the existing PSO in the cache if one with the same name exists.</param>
    /// <returns>The newly created <see cref="GorgonComputePso"/>, or the existing <see cref="GorgonComputePso"/> in the <paramref name="cache"/> with the same <paramref name="name"/>.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="name"/> is empty.</exception>
    /// <exception cref="GorgonException">Thrown if the pipeline state object could not be created by the video adapter.</exception>
    /// <remarks>
    /// <para>
    /// Implementors <b>must</b> use this method to create the <see cref="GorgonComputePso"/> after its data has been read from the stream. A <see cref="GorgonComputePso"/> cannot be created directly, so this 
    /// method must be used for Gorgon to populate the <see cref="GorgonComputePso"/> with the correct information.
    /// </para>
    /// <para>
    /// If the <paramref name="psoBlob"/> cannot be used by the current video adapter or driver, then it is discarded, and the <see cref="GorgonComputePso"/> is compiled from its shader instead.
    /// </para>
    /// <para>
    /// If the <paramref name="cache"/> already contains a PSO with the same <paramref name="name"/>, then the existing PSO is returned, and the <paramref name="computeShader"/> is ignored. If the existing PSO 
    /// uses a different compute shader, then a warning will be written to the log.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonComputePso"/>
    /// <seealso cref="GorgonComputePsoFactory"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static GorgonComputePso BuildAndCachePso(string name, byte[] psoBlob, GorgonShader computeShader, GorgonComputePsoFactory cache) => cache.CreateOrGetPso(name, computeShader, psoBlob);

    /// <inheritdoc cref="GorgonCodecGraphicsPsoCacheCommon.GetShaderInCache(GorgonGraphicsPsoFactory, ReadOnlySpan{byte})" path="/summary"/>
    /// <inheritdoc cref="GorgonCodecGraphicsPsoCacheCommon.GetShaderInCache(GorgonGraphicsPsoFactory, ReadOnlySpan{byte})" path="/param"/>
    /// <inheritdoc cref="GorgonCodecGraphicsPsoCacheCommon.GetShaderInCache(GorgonGraphicsPsoFactory, ReadOnlySpan{byte})" path="/returns"/>
    /// <inheritdoc cref="GorgonCodecGraphicsPsoCacheCommon.GetShaderInCache(GorgonGraphicsPsoFactory, ReadOnlySpan{byte})" path="/remarks"/>
    /// <seealso cref="GorgonComputePsoFactory"/>
    /// <seealso cref="GorgonShader"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static GorgonShader? GetShaderInCache(GorgonComputePsoFactory cache, ReadOnlySpan<byte> shaderBlobData)
    {
        int hashCode = GorgonShader.CreateNewShaderHashCode(shaderBlobData);
        return cache.FindShaderWithSignature(hashCode, shaderBlobData);
    }

    /// <inheritdoc/>
    private protected sealed override GorgonComputePsoFactory CreateFactory() => new(Graphics);

    /// <inheritdoc/>
    private protected sealed override int GetPsoCount(GorgonComputePsoFactory factory) => factory.PipelineStateObjects.Count;
}
