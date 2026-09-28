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
/// A record defining a buffer used in a draw call.
/// </summary>
/// <param name="Buffer">The buffer being used.</param>
/// <param name="Shader">The shader stage to use the buffer with.</param>
/// <param name="Usage">The intended usage for the buffer.</param>
public readonly record struct GorgonDrawCallBuffer(GorgonGpuBuffer Buffer, ShaderStage Shader, BufferUsage Usage)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonDrawCallBuffer"/> record.
    /// </summary>
    /// <param name="view">The view containing the buffer being used.</param>
    /// <param name="stage"><inheritdoc cref="GorgonDrawCallBuffer(GorgonGpuBuffer, ShaderStage, BufferUsage)" path="/param[@name='Shader']"/></param>
    /// <param name="usage"><inheritdoc cref="GorgonDrawCallBuffer(GorgonGpuBuffer, ShaderStage, BufferUsage)" path="/param[@name='Usage']"/></param>
    public GorgonDrawCallBuffer(GorgonGpuBufferView view, ShaderStage stage, BufferUsage usage)
        : this(view.Buffer, stage, usage)
    {
    }
}

/// <summary>
/// A record defining a texture used in a draw call.
/// </summary>
/// <param name="Texture">The texture being used.</param>
/// <param name="Shader">The shader stage to use the texture with.</param>
/// <param name="Usage">The intended usage for the texture.</param>
public readonly record struct GorgonDrawCallTexture(GorgonTextureCommon Texture, ShaderStage Shader, TextureUsage Usage)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonDrawCallTexture"/> record.
    /// </summary>
    /// <param name="view">The view containing the texture being used.</param>
    /// <param name="stage"><inheritdoc cref="GorgonDrawCallTexture(GorgonTextureCommon, ShaderStage, TextureUsage)" path="/param[@name='Shader']"/></param>
    /// <param name="usage"><inheritdoc cref="GorgonDrawCallTexture(GorgonTextureCommon, ShaderStage, TextureUsage)" path="/param[@name='Usage']"/></param>
    public GorgonDrawCallTexture(IGorgonTextureView<GorgonTextureCommon> view, ShaderStage stage, TextureUsage usage)
        : this(view.Texture, stage, usage)
    {
    }
}

/// <summary>
/// Common values for draw calls.
/// </summary>
public abstract class GorgonDrawCallCommon
{
    /// <summary>
    /// Property to return the list of buffers used in this draw call.
    /// </summary>
    public List<GorgonDrawCallBuffer> UsedBuffers
    {
        get;
    } = new List<GorgonDrawCallBuffer>(128);

    /// <summary>
    /// Property to return the list of textures used in this draw call.
    /// </summary>
    public List<GorgonDrawCallTexture> UsedTextures
    {
        get;
    } = new List<GorgonDrawCallTexture>(128);

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
    /// The default value is between 0 and 1.0f.
    /// </para>
    /// </remarks>
    public GorgonRange<float> DepthBoundsTestRange
    {
        get;
        set;
    } = new GorgonRange<float>(0, 1.0f);

    /// <summary>
    /// Property to set or return the value added to each index before reading per instance data from the vertex buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public int StartInstance
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the number of instances to draw.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is 1.
    /// </para>
    /// </remarks>
    public int InstanceCount
    {
        get;
        set;
    } = 1;

    /// <summary>
    /// Property to return the pipeline state object to use when drawing.
    /// </summary>
    public required GorgonGraphicsPso Pso
    {
        get;
        set;
    }

    /// <summary>
    /// Function to assign the buffers to be used in the draw call.
    /// </summary>
    /// <param name="buffers">The buffers to assign to the draw call.</param>
    /// <exception cref="NullReferenceException">Thrown if one of the elements in the list is not initialized with a buffer.</exception>
    /// <remarks>
    /// <para>
    /// This method is used to assign any buffers that are required by the draw call. For example, if a draw call  requires a vertex buffer, then the vertex buffer needs to be assigned to draw call using 
    /// this method.
    /// </para>
    /// <para type="common">
    /// Gorgon needs to know which resources are going to be used while rendering so it can be sure everything is synchronized, and that everything is configured to be used by the shaders assigned on the 
    /// <see cref="GorgonGraphicsPso"/>. Applications that do not assign the expected resources may crash at worst, or have corrupted rendering at best.
    /// </para>
    /// <para type="common">
    /// This value can be set on either the builder for the draw call type, or later, directly on the draw call. The former is best used for one-time setup, while the latter is in rare cases where a resource 
    /// needs to swapped out dynamically (e.g. a render target texture is rebuilt and needs to be assigned to the call). When calling this method, all buffers are replaced with the value passed in, there is 
    /// no mixing of buffer references.
    /// </para>
    /// <para type="common">
    /// <b>Note:</b> The order of the resources is irrelevant.
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
    public void AssignBuffers(ReadOnlySpan<GorgonDrawCallBuffer> buffers)
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

            UsedBuffers[i] = buffers[i];
        }
    }

    /// <summary>
    /// Function to assign a single buffer to be used in the draw call.
    /// </summary>
    /// <param name="buffer">The buffer to assign.</param>
    /// <remarks>
    /// <para>
    /// This method is used to assign a single buffer that is required by the draw call. For example, if a draw call  requires a vertex buffer, then the vertex buffer needs to be assigned to draw call using 
    /// this method.
    /// </para>
    /// <para>
    /// Passing <b>null</b> for the <paramref name="buffer"/> will clear any buffers assigned to this draw call.
    /// </para>
    /// <inheritdoc cref="AssignBuffers(ReadOnlySpan{GorgonDrawCallBuffer})" path="/remarks/para[@type='common']"/>
    /// </remarks>
    /// <inheritdoc cref="AssignBuffers(ReadOnlySpan{GorgonDrawCallBuffer})" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssignBuffer(in GorgonDrawCallBuffer? buffer)
    {
        UsedBuffers.Clear();

        if (buffer?.Buffer is not null)
        {
            UsedBuffers.Add(buffer.Value);
        }
    }

    /// <summary>
    /// Function to assign the textures to be used in the draw call.
    /// </summary>
    /// <param name="textures">The textures to assign to the draw call.</param>
    /// <exception cref="NullReferenceException">Thrown if one of the elements in the list is not initialized with a texture.</exception>
    /// <remarks>
    /// <para>
    /// This method is used to assign any textures that are required by the draw call. For example, if a draw call requires a diffuse texture, then the texture needs to be assigned to draw call using 
    /// this method.
    /// </para>
    /// <inheritdoc cref="AssignBuffers(ReadOnlySpan{GorgonDrawCallBuffer})" path="/remarks/para[@type='common']"/>
    /// </remarks>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonVirtualTexture"/>
    /// <seealso cref="GorgonGraphicsPso"/>
    public void AssignTextures(ReadOnlySpan<GorgonDrawCallTexture> textures)
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

            UsedTextures[i] = textures[i];
        }
    }

    /// <summary>
    /// Function to assign a single texture to be used in the draw call.
    /// </summary>
    /// <param name="texture">The texture to assign.</param>
    /// <remarks>
    /// <para>
    /// This method is used to assign a single texture that is required by the draw call. For example, if a draw call requires a diffuse texture, then the texture needs to be assigned to draw call using 
    /// this method.
    /// </para>
    /// <para>
    /// Passing <b>null</b> for the <paramref name="texture"/> will clear any textures assigned to this draw call.
    /// </para>
    /// <inheritdoc cref="AssignTextures(ReadOnlySpan{GorgonDrawCallTexture})" path="/remarks"/>
    /// </remarks>
    /// <inheritdoc cref="AssignTextures(ReadOnlySpan{GorgonDrawCallTexture})" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AssignTexture(in GorgonDrawCallTexture? texture)
    {
        UsedTextures.Clear();

        if (texture?.Texture is not null)
        {
            UsedTextures.Add(texture.Value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonDrawCallCommon"/> class.
    /// </summary>
    protected GorgonDrawCallCommon()
    {
    }
}
