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
    /// Buffer will store arguments for indirection execution.
    /// </summary>
    IndirectArguments,

    /// <summary>
    /// Buffer is used for general read only data storage.
    /// </summary>
    ReadOnly,

    /// <summary>
    /// <para>
    /// Buffer is used for general writable data storage.
    /// </para>
    /// <para>
    /// This implies that the buffer can be written to, and read from, but only in a single pass.
    /// </para>
    /// </summary>    
    Writeable,

    /// <summary>
    /// Buffer is used for general data storage that can be read and write concurrently.
    /// </summary>
    ReadWrite
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
    /// Texture is general read only texel data.
    /// </summary>
    ReadOnly,

    /// <summary>
    /// <para>
    /// Texture is general writeable texel data.
    /// </para>
    /// <para>
    /// This implies that the texture can be written to, and read from, but only in a single pass.
    /// </para>
    /// </summary>
    Writeable
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
