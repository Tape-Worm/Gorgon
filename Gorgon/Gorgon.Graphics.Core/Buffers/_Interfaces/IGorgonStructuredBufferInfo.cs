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

using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Provides the necessary information required to set up a structured buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This provides an immutable view of the buffer information so that it cannot be modified after the buffer is created.
    /// </para>
    /// </remarks>
    public interface IGorgonStructuredBufferInfo
    {
        /// <summary>
        /// Property to return the intended usage for binding to the GPU.
        /// </summary>
        /// <remarks>
        /// The default value is <c>Default</c>.
        /// </remarks>
        D3D11.ResourceUsage Usage
        {
            get;
        }

        /// <summary>
        /// Property to return the size of the buffer, in bytes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value should be larger than 0, or else an exception will be thrown when the buffer is created.
        /// </para>
        /// </remarks>
        int SizeInBytes
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
        /// If the <see cref="Usage"/> is set to <c>Staging</c>, then this value must be set to <see cref="BufferBinding.None"/>, otherwise an exception will be raised when the buffer is created.
        /// </para>
        /// <para>
        /// The default value is <see cref="BufferBinding.Shader"/>
        /// </para>
        /// </remarks>
        BufferBinding Binding
        {
            get;
        }

        /// <summary>
        /// Property to set or return the size, in bytes, of an individual item in a structured buffer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value must be between 1 and 2048 or else an exception will be raised when the buffer is created.
        /// </para>
        /// <para>
        /// This value should also be aligned to a 4 byte padding. If it is not, then Gorgon will resize to the nearest 4 byte boundary automatically. This may cause confusion when writing to the buffer, 
        /// so it is best practice to ensure a 4 byte alignment.
        /// </para>
        /// <para>
        /// The default value is 0.
        /// </para>
        /// </remarks>
        int StructureSize
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether a default shader resource view for this buffer should be created.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use this to define a default <see cref="GorgonStructuredBufferView"/> for the buffer. This default view will allow shaders to access the buffer without needing to create an additional 
        /// view. If this value is set to <b>false</b>, then no default shader view will be created.
        /// </para>
        /// <para>
        /// The default shader view will expose the entire buffer to the shader. To limit view to only a portion of the buffer, call the <see cref="GorgonStructuredBuffer.GetShaderResourceView"/> with the 
        /// appropriate element constraints.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// This property is only used if the <see cref="Binding"/> property has a <see cref="BufferBinding.Shader"/> flag.
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// The default value for this property is <b>false</b>.
        /// </para>
        /// </remarks>
        bool UseDefaultShaderView
        {
            get;
            set;
        }
    }
}
