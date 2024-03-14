#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: July 4, 2017 10:06:48 PM
// 
#endregion

using Gorgon.Core;

namespace Gorgon.Graphics.Core;

/// <summary>
/// The type of binding to use when binding to the GPU.
/// </summary>
[Flags]
public enum BufferBinding
{
    /// <summary>
    /// <para>
    /// No GPU access. This buffer will only be used on the CPU.
    /// </para>
    /// <para>
    /// This flag is mutally exclusive.
    /// </para>
    /// </summary>
    None = 0,
    /// <summary>
    /// The GPU will have access via the shaders as a shader resource.
    /// </summary>
    Shader = 1,
    /// <summary>
    /// The GPU will have read/write access via the shaders using unordered access.
    /// </summary>
    ReadWrite = 2,
    /// <summary>
    /// The GPU will bind this buffer as a stream out buffer used to receive data.
    /// </summary>
    StreamOut = 4
}

/// <summary>
/// The type of data to be stored in the buffer.
/// </summary>
public enum BufferType
{
    /// <summary>
    /// A generic buffer filled with data to conforms to one of the <see cref="BufferFormat"/> types.
    /// </summary>
    Generic = 0,

    /// <summary>
    /// A constant buffer used to send data to a shader.
    /// </summary>
    Constant = 1,

    /// <summary>
    /// A vertex buffer used to hold vertex information.
    /// </summary>
    Vertex = 2,

    /// <summary>
    /// An index buffer used to hold index information.
    /// </summary>
    Index = 3,

    /// <summary>
    /// A structured buffer used to hold structured data.
    /// </summary>
    Structured = 4,

    /// <summary>
    /// A raw buffer used to hold raw byte data.
    /// </summary>
    Raw = 5,

    /// <summary>
    /// An indirect argument buffer.
    /// </summary>
    IndirectArgument = 6
}

/// <summary>
/// Provides the necessary information required to set up a generic unstructured buffer.
/// </summary>
/// <remarks>
/// <para>
/// This provides an immutable view of the buffer information so that it cannot be modified after the buffer is created.
/// </para>
/// </remarks>
public interface IGorgonBufferInfo
    : IGorgonNamedObject
{
    /// <summary>
    /// Property to set or return whether to allow the CPU read access to the buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value controls whether or not the CPU can directly access the buffer for reading. If this value is <b>false</b>, the buffer still can be read, but will be done through an intermediate
    /// staging buffer, which is obviously less performant. 
    /// </para>
    /// <para>
    /// This value is treated as <b>false</b> if the buffer does not have a <see cref="Binding"/> containing the <see cref="BufferBinding.Shader"/> flag, and does not have a <see cref="Usage"/> of
    /// <see cref="ResourceUsage.Default"/>. This means any reads will be done through an intermediate staging buffer, impacting performance.
    /// </para>
    /// <para>
    /// If the <see cref="Usage"/> property is set to <see cref="ResourceUsage.Staging"/>, then this value is treated as <b>true</b> because staging buffers are CPU only and as such, can be read
    /// directly by the CPU regardless of this value.
    /// </para>
    /// <para>
    /// The default for this value is <b>false</b>.
    /// </para>
    /// </remarks>
    bool AllowCpuRead
    {
        get;
    }

    /// <summary>
    /// Property to return the intended usage for binding to the GPU.
    /// </summary>
    ResourceUsage Usage
    {
        get;
    }

    /// <summary>
    /// Property to return the size of the buffer, in bytes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For buffers that set <see cref="AllowRawView"/> to <b>true</b>, then this value will be rounded up to the nearest multiple of 4 at buffer creation time.
    /// </para>
    /// <para>
    /// This value should be larger than 0, or else an exception will be thrown when the buffer is created.
    /// </para>
    /// </remarks>
    int SizeInBytes
    {
        get;
    }

    /// <summary>
    /// Property to return the size, in bytes, of an individual structure in a structured buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Set this value to a number larger than 0 to indicate that this buffer will contain structured data.
    /// </para>
    /// <para>
    /// This value should be larger than 0, and less than or equal to 2048 bytes.  The structure size should also be a multiple of 4, and will be rounded up at buffer creation if it is not.
    /// </para>
    /// <para>
    /// Vertex, Index and constant buffers will ignore this flag and will reset it to 0.
    /// </para>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    int StructureSize
    {
        get;
    }

    /// <summary>
    /// Property to return whether to allow raw unordered views of the buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this value is <b>true</b>, then unordered access views for this buffer can use byte addressing to read/write the buffer in a shader. If it is <b>false</b>, then the SRV format or 
    /// <see cref="StructureSize"/> will determine how to address data in the buffer.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    bool AllowRawView
    {
        get;
    }

    /// <summary>
    /// Property to return the type of binding for the GPU.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The type of binding should be used to determine what type of view to apply to the buffer when accessing it from shaders. This will also help determine how data will be interpreted.
    /// </para>
    /// <para>
    /// Different bindings may be applied at the same time by OR'ing the <see cref="BufferBinding"/> flags together.
    /// </para>
    /// <para>
    /// If the <see cref="Usage"/> is set to <see cref="ResourceUsage.Staging"/>, then this value must be set to <see cref="BufferBinding.None"/>, otherwise an exception will be raised when the buffer 
    /// is created.
    /// </para>
    /// <para>
    /// The default value is <see cref="BufferBinding.None"/>
    /// </para>
    /// </remarks>
    BufferBinding Binding
    {
        get;
    }

    /// <summary>
    /// Property to return whether the buffer will contain indirect argument data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This flag only applies to buffers with a <see cref="Binding"/> of <see cref="BufferBinding.ReadWrite"/>, and/or <see cref="BufferBinding.Shader"/>. If the binding is set to anything else, 
    /// then this flag is treated as being set to <b>false</b>.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    bool IndirectArgs
    {
        get;
    }
}
