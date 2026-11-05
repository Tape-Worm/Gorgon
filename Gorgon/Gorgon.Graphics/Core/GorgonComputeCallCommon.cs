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
// Created: November 4, 2026 2:01:20 PM
//

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Gorgon.Core;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Common values for the compute calls sent to a <see cref="GorgonCommandList"/> or a <see cref="GorgonComputeCommandList"/>.
/// </summary>
/// <param name="pso">The compute pipeline state object to use for the call.</param>
/// <remarks>
/// <para>
/// This holds the state shared by dispatch calls (<see cref="GorgonDispatchCall"/>) and execute calls (<see cref="GorgonComputeExecuteCall"/>): the pipeline state object, and the resources used by the 
/// compute shader.
/// </para>
/// <para>
/// A call can be kept and sent to a command list as many times as needed, and its values can be changed between calls.
/// </para>
/// </remarks>
/// <seealso cref="GorgonDispatchCall"/>
/// <seealso cref="GorgonComputeExecuteCall"/>
public abstract class GorgonComputeCallCommon(GorgonComputePso pso)
{
    /// <inheritdoc cref="GorgonGraphicsCallCommon.UsedBuffers"/>
    public List<GorgonUsedBuffer> UsedBuffers
    {
        get;
    } = new List<GorgonUsedBuffer>(128);

    /// <inheritdoc cref="GorgonGraphicsCallCommon.UsedTextures"/>
    public List<GorgonUsedTexture> UsedTextures
    {
        get;
    } = new List<GorgonUsedTexture>(128);

    /// <inheritdoc cref="GorgonGraphicsCallCommon.Pso"/>
    public GorgonComputePso Pso
    {
        get;
        set;
    } = pso;

    /// <inheritdoc cref="GorgonGraphicsCallCommon.AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/summary"/>
    /// <inheritdoc cref="GorgonGraphicsCallCommon.AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/param"/>
    /// <inheritdoc cref="GorgonGraphicsCallCommon.AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This method is used to assign any buffers that are required by the call. For example, if the compute shader reads its input from a buffer, then that buffer needs to be assigned to the call 
    /// using this method.
    /// </para>
    /// <para type="common">
    /// Gorgon needs to know which resources are going to be used by the call so it can be sure everything is synchronized, and that everything is configured to be used by the shader assigned on the 
    /// <see cref="GorgonComputePso"/>. Applications that do not assign the expected resources may crash at worst, or produce corrupted results at best.
    /// </para>
    /// <inheritdoc cref="GorgonGraphicsCallCommon.AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/remarks/para[@type='declare_resources']"/>
    /// <para type="common">
    /// The resources can be assigned when the call is set up, or later, directly on the call, in the rare cases where a resource needs to be swapped out dynamically (e.g. a texture is rebuilt 
    /// and needs to be assigned to the call). When calling this method, all buffers are replaced with the value passed in, there is no mixing of buffer references.
    /// </para>
    /// <para type="common">
    /// <b>Note:</b> The order of the resources is irrelevant.
    /// </para>
    /// <para type="common">
    /// The <see cref="GorgonComputeExecuteCall.Commands"/> buffer of an execute call is managed by the execute call itself, and must not be assigned with these methods.
    /// </para>
    /// <para type="common">
    /// <note type="information">
    /// <para>
    /// This is not the same as assigning a resource view to a shader value in the bindless model. That is a user operation telling the shader which view to use.
    /// </para>
    /// </note>
    /// <note type="warning">
    /// <para>
    /// These assign methods are <b>NOT</b> thread safe. Do not call them from across multiple threads.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGpuBuffer"/>
    /// <seealso cref="GorgonComputePso"/>
    public void AssignBuffers(ReadOnlySpan<GorgonUsedBuffer> buffers)
    {
        UsedBuffers.Clear();
        if (buffers.Length == 0)
        {
            return;
        }

        for (int i = 0; i < buffers.Length; i++)
        {
            if (buffers[i].Buffer is null)
            {
                throw new NullReferenceException();
            }

            UsedBuffers.Add(buffers[i]);
        }
    }

    /// <inheritdoc cref="GorgonGraphicsCallCommon.AssignBuffer(in GorgonUsedBuffer?)" path="/summary"/>
    /// <inheritdoc cref="GorgonGraphicsCallCommon.AssignBuffer(in GorgonUsedBuffer?)" path="/param"/>
    /// <remarks>
    /// <para>
    /// This method is used to assign a single buffer that is required by the call. For example, if the compute shader reads its input from a buffer, then that buffer needs to be assigned to the 
    /// call using this method.
    /// </para>
    /// <para>
    /// Passing <b>null</b> for the <paramref name="buffer"/> will clear any buffers assigned to this call.
    /// </para>
    /// <inheritdoc cref="AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/remarks/para[@type='common']"/>
    /// <inheritdoc cref="GorgonGraphicsCallCommon.AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/remarks/para[@type='declare_resources']"/>
    /// </remarks>
    /// <inheritdoc cref="AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssignBuffer(in GorgonUsedBuffer? buffer)
    {
        UsedBuffers.Clear();

        if (buffer?.Buffer is not null)
        {
            UsedBuffers.Add(buffer.Value);
        }
    }

    /// <inheritdoc cref="GorgonGraphicsCallCommon.AssignTextures(ReadOnlySpan{GorgonUsedTexture})" path="/summary"/>
    /// <inheritdoc cref="GorgonGraphicsCallCommon.AssignTextures(ReadOnlySpan{GorgonUsedTexture})" path="/param"/>
    /// <inheritdoc cref="GorgonGraphicsCallCommon.AssignTextures(ReadOnlySpan{GorgonUsedTexture})" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This method is used to assign any textures that are required by the call. For example, if the compute shader writes its output to a texture, then the texture needs to be assigned to the call 
    /// using this method.
    /// </para>
    /// <inheritdoc cref="AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/remarks/para[@type='common']"/>
    /// <inheritdoc cref="GorgonGraphicsCallCommon.AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/remarks/para[@type='declare_resources']"/>
    /// </remarks>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonVirtualTexture"/>
    /// <seealso cref="GorgonComputePso"/>
    public void AssignTextures(ReadOnlySpan<GorgonUsedTexture> textures)
    {
        UsedTextures.Clear();
        if (textures.Length == 0)
        {
            return;
        }

        for (int i = 0; i < textures.Length; ++i)
        {
            if (textures[i].Texture is null)
            {
                throw new NullReferenceException();
            }

            UsedTextures.Add(textures[i]);
        }
    }

    /// <inheritdoc cref="GorgonGraphicsCallCommon.AssignTexture(in GorgonUsedTexture?)" path="/summary"/>
    /// <inheritdoc cref="GorgonGraphicsCallCommon.AssignTexture(in GorgonUsedTexture?)" path="/param"/>
    /// <remarks>
    /// <para>
    /// This method is used to assign a single texture that is required by the call. For example, if the compute shader writes its output to a texture, then the texture needs to be assigned to the 
    /// call using this method.
    /// </para>
    /// <para>
    /// Passing <b>null</b> for the <paramref name="texture"/> will clear any textures assigned to this call.
    /// </para>
    /// <inheritdoc cref="AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/remarks/para[@type='common']"/>
    /// <inheritdoc cref="GorgonGraphicsCallCommon.AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/remarks/para[@type='declare_resources']"/>
    /// </remarks>
    /// <inheritdoc cref="AssignTextures(ReadOnlySpan{GorgonUsedTexture})" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssignTexture(in GorgonUsedTexture? texture)
    {
        UsedTextures.Clear();

        if (texture?.Texture is not null)
        {
            UsedTextures.Add(texture.Value);
        }
    }
}
