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
// Created: April 6, 2026 12:12:41 PM
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Gorgon.Graphics.Core;

/// <summary>
/// The intended usage for a buffer when used with a command list.
/// </summary>
public enum BufferUsage
{
    /// <summary>
    /// No usage.
    /// </summary>
    None,

    /// <summary>
    /// Buffer will be used for vertex data.
    /// </summary>
    VertexBuffer,

    /// <summary>
    /// Buffer will be used to hold shader constants.
    /// </summary>
    ConstantBuffer,

    /// <summary>
    /// Buffer will store arguments for indirect execution.
    /// </summary>
    IndirectArguments,

    /// <summary>
    /// Buffer is read through a read only view, such as a <see cref="GorgonStructuredBufferView"/>.
    /// </summary>
    ReadOnly,

    /// <summary>
    /// Buffer is read from, and written to, through a read/write view, such as a <see cref="GorgonStructuredBufferRwView"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the usage for a buffer that is only accessed through read/write views in the draw, and is the usage assigned by the <see cref="GorgonUsedBuffer(GorgonShaderBufferRwView, ShaderStage)"/> 
    /// constructor.
    /// </para>
    /// <para>
    /// If the same buffer is also read through a read only view in the same draw, use <see cref="ReadOnlyAndReadWrite"/> instead.
    /// </para>
    /// <inheritdoc cref="GorgonGraphicsCallCommon.AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/remarks/para[@type='declare_resources']"/>
    /// </remarks>
    ReadWrite,

    /// <summary>
    /// Buffer is accessed through a read/write view, and read through a read only view, in the same draw.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this when a single buffer is bound through both a read/write view (e.g. <see cref="GorgonStructuredBufferRwView"/>) and a read only view (e.g. <see cref="GorgonStructuredBufferView"/>) in one draw. 
    /// A buffer can be read by any number of operations while a single operation writes to it.
    /// </para>
    /// <para>
    /// <see cref="TextureUsage"/> has no equivalent value. A texture sub resource can only be in one layout at a time, so it cannot be accessed through a read/write view and a read only view at the same time. 
    /// Buffers have no layouts, so this restriction does not apply to them.
    /// </para>
    /// </remarks>
    ReadOnlyAndReadWrite
}

/// <summary>
/// The intended usage for a texture when used with a command list.
/// </summary>
public enum TextureUsage
{
    /// <summary>
    /// No usage.
    /// </summary>
    None,

    /// <summary>
    /// Texture is read through a read only view, such as an <see cref="IGorgonTextureView{T}"/>.
    /// </summary>
    ReadOnly,

    /// <summary>
    /// Texture is read from, and written to, through a <see cref="GorgonTextureRwView"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the usage assigned by the <see cref="GorgonUsedTexture(GorgonTextureRwView, ShaderStage)"/> constructor.
    /// </para>
    /// <inheritdoc cref="GorgonGraphicsCallCommon.AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})" path="/remarks/para[@type='declare_resources']"/>
    /// <inheritdoc cref="GorgonUsedTexture.SubResources" path="/remarks/para[@type='subresource_usage']"/>
    /// <para>
    /// Unlike <see cref="BufferUsage"/> (see <see cref="BufferUsage.ReadOnlyAndReadWrite"/>), there is no value for reading a texture sub resource through a read only view while writing to the same sub 
    /// resource through a read/write view. A texture sub resource can only be in one layout at a time, and the read/write layout does not allow access through a read only view.
    /// </para>
    /// </remarks>
    ReadWrite
}

/// <summary>
/// Indicates which shader stage(s) the resource should be accessed by.
/// </summary>
[Flags]
public enum ShaderStage
{
    /// <summary>
    /// No shaders.
    /// </summary>
    None = 0,
    /// <summary>
    /// Vertex shaders.
    /// </summary>
    Vertex = 1,
    /// <summary>
    /// Pixel shaders.
    /// </summary>
    Pixel = 2,
    /// <summary>
    /// Geometry shaders.
    /// </summary>
    Geometry = 4,
    /// <summary>
    /// Hull shaders.
    /// </summary>
    Hull = 8,
    /// <summary>
    /// Domain shaders.
    /// </summary>
    Domain = 16,
    /// <summary>
    /// Mesh shaders.
    /// </summary>
    Mesh = 32,
    /// <summary>
    /// Amplification shaders.
    /// </summary>
    Amplification = 64,
    /// <summary>
    /// Compute shaders.
    /// </summary>
    Compute = 128
}
