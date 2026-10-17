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
// Created: July 1, 2026 10:55:24 PM
//

using System.Runtime.CompilerServices;
using Gorgon.Core;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A record defining a buffer used in a draw or execute call.
/// </summary>
/// <param name="Buffer">The buffer being used.</param>
/// <param name="Shader">The shader stage to use the buffer with.</param>
/// <param name="Usage">The intended usage for the buffer.</param>
public readonly record struct GorgonUsedBuffer(GorgonGpuBufferCommon Buffer, ShaderStage Shader, BufferUsage Usage)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonUsedBuffer"/> record.
    /// </summary>
    /// <param name="view">The view containing the buffer being used.</param>
    /// <param name="stage"><inheritdoc cref="GorgonUsedBuffer(GorgonGpuBufferCommon, ShaderStage, BufferUsage)" path="/param[@name='Shader']"/></param>
    /// <param name="usage"><inheritdoc cref="GorgonUsedBuffer(GorgonGpuBufferCommon, ShaderStage, BufferUsage)" path="/param[@name='Usage']"/></param>
    public GorgonUsedBuffer(GorgonGpuBufferView view, ShaderStage stage, BufferUsage usage)
        : this(view.Buffer, stage, usage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonUsedBuffer"/> record.
    /// </summary>
    /// <param name="view">The read/write view containing the buffer being used.</param>
    /// <param name="stage"><inheritdoc cref="GorgonUsedBuffer(GorgonGpuBufferCommon, ShaderStage, BufferUsage)" path="/param[@name='Shader']"/></param>
    public GorgonUsedBuffer(GorgonShaderBufferRwView view, ShaderStage stage)
        : this(view.Buffer, stage, BufferUsage.ReadWrite)
    {
    }
}

/// <summary>
/// A record defining a texture used in a draw or execute call.
/// </summary>
/// <param name="Texture">The texture being used.</param>
/// <param name="Shader">The shader stage to use the texture with.</param>
/// <param name="Usage">The intended usage for the texture.</param>
public readonly record struct GorgonUsedTexture(GorgonTextureCommon Texture, ShaderStage Shader, TextureUsage Usage)
{
    /// <summary>
    /// Property to return the sub resources of the texture used by the shader.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the record is created from a view, this is the range of mip levels, array indices and the plane covered by that view. Otherwise, it covers the entire texture.
    /// </para>
    /// <para type="subresource_usage">
    /// Only these sub resources are prepared for the call. This allows different sub resources of the same texture to be used with different usages in one call. For example, a shader can read mip 0 through a 
    /// read only view while writing mip 1 through a read/write view, as long as each entry is created from its own view.
    /// </para>
    /// <para>
    /// The default value is <see cref="GorgonSubResourceRange.All"/>.
    /// </para>
    /// </remarks>
    public GorgonSubResourceRange SubResources
    {
        get;
        init;
    } = GorgonSubResourceRange.All;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonUsedTexture"/> record.
    /// </summary>
    /// <param name="view">The view containing the texture being used.</param>
    /// <param name="stage"><inheritdoc cref="GorgonUsedTexture(GorgonTextureCommon, ShaderStage, TextureUsage)" path="/param[@name='Shader']"/></param>
    /// <param name="usage"><inheritdoc cref="GorgonUsedTexture(GorgonTextureCommon, ShaderStage, TextureUsage)" path="/param[@name='Usage']"/></param>
    public GorgonUsedTexture(IGorgonTextureView<GorgonTextureCommon> view, ShaderStage stage, TextureUsage usage)
        : this(view.Texture, stage, usage) => SubResources = new(view.MipLevel, view.MipCount, view.ArrayIndex, view.ArrayCount, view.PlaneIndex, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonUsedTexture"/> record.
    /// </summary>
    /// <param name="view">The read/write view containing the texture being used.</param>
    /// <param name="stage"><inheritdoc cref="GorgonUsedTexture(GorgonTextureCommon, ShaderStage, TextureUsage)" path="/param[@name='Shader']"/></param>
    public GorgonUsedTexture(GorgonTextureRwView view, ShaderStage stage)
        : this(view.Texture, stage, TextureUsage.ReadWrite)=> SubResources = new(view.MipLevel, 1, view.ArrayIndex, view.ArrayCount, view.PlaneIndex, 1);
}

/// <summary>
/// Common values for the graphics calls sent to a <see cref="GorgonCommandList"/>.
/// </summary>
/// <param name="pso">The graphics pipeline state object to use for the call.</param>
/// <remarks>
/// <para>
/// This holds the state shared by draw calls (<see cref="GorgonDrawCall"/>, <see cref="GorgonIndexedDrawCall"/>) and execute calls (<see cref="GorgonExecuteCall"/>, <see cref="GorgonIndexedExecuteCall"/>): 
/// the pipeline state object, the resources used by the shaders, and the dynamic pipeline values.
/// </para>
/// <para>
/// A call can be kept and sent to a command list as many times as needed, and its values can be changed between calls.
/// </para>
/// </remarks>
/// <seealso cref="GorgonDrawCallCommon"/>
/// <seealso cref="GorgonExecuteCall"/>
/// <seealso cref="GorgonIndexedExecuteCall"/>
public abstract class GorgonGraphicsCallCommon(GorgonGraphicsPso pso)
{
    /// <summary>
    /// Property to return the list of buffers used in this call.
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/remarks/para[@type='declare_resources']"/>
    /// </remarks>
    public List<GorgonUsedBuffer> UsedBuffers
    {
        get;
    } = new List<GorgonUsedBuffer>(128);

    /// <summary>
    /// Property to return the list of textures used in this call.
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/remarks/para[@type='declare_resources']"/>
    /// </remarks>
    public List<GorgonUsedTexture> UsedTextures
    {
        get;
    } = new List<GorgonUsedTexture>(128);

    /// <summary>
    /// Property to set or return the blending factor used in blend operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="GorgonColors.White"/>.
    /// </para>
    /// </remarks>
    public GorgonColor BlendFactor
    {
        get;
        set;
    } = GorgonColors.White;

    /// <summary>
    /// Property to set or return the value used when a stencil operation is set to <see cref="StencilOperation.Replace"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public byte DepthStencilReplaceValue
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the depth value range that will be passed during a depth test operation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value determines whether a pixel/sample will pass if the depth buffer value falls within the specified range. This property is only used when the user has the 
    /// <see cref="GorgonDepthStencilState.IsDepthBoundsTestingEnabled"/> set to <b>true</b> on the active <see cref="GorgonGraphicsPso"/>.
    /// </para>
    /// <para>
    /// <c>NaN</c> values are treated as 0.
    /// </para>
    /// <para>
    /// The default value is a range of 0 to 1.
    /// </para>
    /// </remarks>
    public GorgonRange<float> DepthBoundsTestRange
    {
        get;
        set;
    } = new GorgonRange<float>(0, 1.0f);

    /// <summary>
    /// Property to set or return the pipeline state object to use for the call.
    /// </summary>
    public GorgonGraphicsPso Pso
    {
        get;
        set;
    } = pso;

    /// <summary>
    /// Function to assign the buffers to be used in the call.
    /// </summary>
    /// <param name="buffers">The buffers to assign to the call.</param>
    /// <exception cref="NullReferenceException">Thrown if one of the elements in the list is not initialized with a buffer.</exception>
    /// <remarks>
    /// <para>
    /// This method is used to assign any buffers that are required by the call. For example, if the vertex shader reads its vertices from a buffer, then that buffer needs to be assigned to the call 
    /// using this method.
    /// </para>
    /// <para type="common">
    /// Gorgon needs to know which resources are going to be used while rendering so it can be sure everything is synchronized, and that everything is configured to be used by the shaders assigned on the 
    /// <see cref="GorgonGraphicsPso"/>. Applications that do not assign the expected resources may crash at worst, or have corrupted rendering at best.
    /// </para>
    /// <para type="declare_resources">
    /// <note type="important">
    /// <para>
    /// Shaders reach resources through handles passed as constants, so Gorgon only knows about the resources that are declared on the call. Every resource a shader writes to through a read/write view, or 
    /// reads after an earlier call wrote to it, <b>must</b> be declared with the correct usage. An undeclared resource gets no barrier, so the shader may read stale or partially written data.
    /// </para>
    /// <para>
    /// The debug layer does not report a missing barrier. The only symptom is incorrect data, which may not appear on every run, or on every GPU.
    /// </para>
    /// </note>
    /// </para>
    /// <para type="common">
    /// The resources can be assigned when the call is set up, or later, directly on the call, in the rare cases where a resource needs to be swapped out dynamically (e.g. a texture is rebuilt 
    /// and needs to be assigned to the call). When calling this method, all buffers are replaced with the value passed in, there is no mixing of buffer references.
    /// </para>
    /// <para type="common">
    /// <b>Note:</b> The order of the resources is irrelevant.
    /// </para>
    /// <para type="common">
    /// The <see cref="GorgonExecuteCallCommon.Commands"/> buffer of an execute call is managed by the execute call itself, and must not be assigned with these methods.
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
    /// <seealso cref="GorgonGraphicsPso"/>    
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

    /// <summary>
    /// Function to assign a single buffer to be used in the call.
    /// </summary>
    /// <param name="buffer">The buffer to assign.</param>
    /// <remarks>
    /// <para>
    /// This method is used to assign a single buffer that is required by the call. For example, if the vertex shader reads its vertices from a buffer, then that buffer needs to be assigned to the 
    /// call using this method.
    /// </para>
    /// <para>
    /// Passing <b>null</b> for the <paramref name="buffer"/> will clear any buffers assigned to this call.
    /// </para>
    /// <inheritdoc cref="AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/remarks/para[@type='common']"/>
    /// <inheritdoc cref="AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/remarks/para[@type='declare_resources']"/>
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

    /// <summary>
    /// Function to assign the textures to be used in the call.
    /// </summary>
    /// <param name="textures">The textures to assign to the call.</param>
    /// <exception cref="NullReferenceException">Thrown if one of the elements in the list is not initialized with a texture.</exception>
    /// <remarks>
    /// <para>
    /// This method is used to assign any textures that are required by the call. For example, if a call requires a diffuse texture, then the texture needs to be assigned to the call using 
    /// this method.
    /// </para>
    /// <inheritdoc cref="AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/remarks/para[@type='common']"/>
    /// <inheritdoc cref="AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/remarks/para[@type='declare_resources']"/>
    /// </remarks>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonVirtualTexture"/>
    /// <seealso cref="GorgonGraphicsPso"/>
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

    /// <summary>
    /// Function to assign a single texture to be used in the call.
    /// </summary>
    /// <param name="texture">The texture to assign.</param>
    /// <remarks>
    /// <para>
    /// This method is used to assign a single texture that is required by the call. For example, if a call requires a diffuse texture, then the texture needs to be assigned to the call using 
    /// this method.
    /// </para>
    /// <para>
    /// Passing <b>null</b> for the <paramref name="texture"/> will clear any textures assigned to this call.
    /// </para>
    /// <inheritdoc cref="AssignTextures(ReadOnlySpan{GorgonUsedTexture})" path="/remarks"/>
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
