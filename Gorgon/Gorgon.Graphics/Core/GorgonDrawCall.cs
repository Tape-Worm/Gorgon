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
// Created: June 25, 2026 12:15:44 AM
//

using Gorgon.Core;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines parameters for drawing primivites.
/// </summary>
/// <param name="indexBuffer">The index buffer to use for drawing.</param>
/// <param name="indexCount">The number of indices to draw.</param>
/// <param name="pso">The pipeline state object to use when drawing.</param>
public sealed class GorgonDrawCall(GorgonIndexBuffer indexBuffer, int indexCount, GorgonGraphicsPso pso)
{
    private (GorgonGpuBuffer Buffer, ShaderStage Shader, BufferUsage BufferUsage)[] _buffers = new (GorgonGpuBuffer Buffer, ShaderStage Shader, BufferUsage BufferUsage)[32];
    private int _bufferCount;
    private (IGorgonTextureView<GorgonTextureCommon> TextureView, ShaderStage Shader, TextureUsage TextureUsage)[] _textures = new (IGorgonTextureView<GorgonTextureCommon> Buffer, ShaderStage Shader, TextureUsage TextureUsage)[16];
    private int _textureCount;

    /// <summary>
    /// Property to set or return the blending factor used in blend operations.
    /// </summary>
    public GorgonColor BlendFactor
    {
        get;
        set;
    } = GorgonColors.White;

    /// <summary>
    /// Property to set or return the value used when a stencil operation is set to <see cref="StencilOperation.Replace"/>.
    /// </summary>
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
    /// Property to set or return the number of indices to read from the index buffer for each instance.
    /// </summary>
    /// <remarks>
    /// This requires that the <see cref="IndexBuffer"/> is not <b>null</b>.
    /// </remarks>
    /// <seealso cref="GorgonCommandList"/>
    /// <seealso cref="GorgonIndexBuffer"/>
    public int IndexCount
    {
        get;
        set;
    } = indexCount;

    /// <summary>
    /// Property to return the index buffer used to draw.
    /// </summary>
    public GorgonIndexBuffer IndexBuffer
    {
        get;
        internal set;
    } = indexBuffer;

    /// <summary>
    /// Property to set or return the number of instances to draw.
    /// </summary>
    public int InstanceCount
    {
        get;
        set;
    } = 1;

    /// <summary>
    /// Property to set or return the starting location of the first index read by the GPU in the index buffer.
    /// </summary>
    public int StartIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the value added to each index value before reading from the vertex buffer.
    /// </summary>
    public int BaseVertex
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the value added to each index before reading per instance data from the vertex buffer.
    /// </summary>
    public int StartInstance
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the pipeline state object to use when drawing.
    /// </summary>
    public GorgonGraphicsPso Pso
    {
        get;
        internal set;
    } = pso;

    /// <summary>
    /// Property to return the list of buffers used in this draw call.
    /// </summary>
    public ReadOnlySpan<(GorgonGpuBuffer Buffer, ShaderStage Shader, BufferUsage Usage)> UsedBuffers
    {
        get => _buffers.AsSpan(0, _bufferCount);
#warning Make internal.
        set
        {
            if (value.Length == 0)
            {
                _bufferCount = 0;
                Array.Fill(_buffers, (null!, ShaderStage.None, BufferUsage.None));
                return;
            }

            if (_buffers.Length < value.Length)
            {
                _buffers = new (GorgonGpuBuffer, ShaderStage, BufferUsage)[value.Length];
            }
            else if (_buffers.Length > value.Length)
            {
                Array.Fill(_buffers, (null!, ShaderStage.None, BufferUsage.None), value.Length, _buffers.Length - value.Length);
            }

            value.CopyTo(_buffers);
            _bufferCount = value.Length;
        }
    }

    /// <summary>
    /// Property to return the list of textures used in this draw call.
    /// </summary>
    public ReadOnlySpan<(IGorgonTextureView<GorgonTextureCommon> Texture, ShaderStage Shader, TextureUsage TextureUsage)> UsedTextures
    {
#warning This should be the resource, not the view.
        get => _textures.AsSpan(0, _textureCount);
#warning Make internal.
        set
        {
            if (value.Length == 0)
            {
                _textureCount = 0;
                Array.Fill(_textures, (null!, ShaderStage.None, TextureUsage.None));
                return;
            }

            if (_textures.Length < value.Length)
            {
                _textures = new (IGorgonTextureView<GorgonTextureCommon>, ShaderStage, TextureUsage)[value.Length];
            }
            else if (_textures.Length > value.Length)
            {
                Array.Fill(_textures, (null!, ShaderStage.None, TextureUsage.None), value.Length, _textures.Length - value.Length);
            }

            value.CopyTo(_textures);
            _textureCount = value.Length;
        }
    }
}
