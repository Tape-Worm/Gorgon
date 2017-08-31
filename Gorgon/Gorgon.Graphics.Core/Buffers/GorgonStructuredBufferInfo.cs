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
// Created: July 4, 2017 11:44:03 PM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Provides the necessary information required to set up a structured buffer.
    /// </summary>
    public class GorgonStructuredBufferInfo
        : IGorgonStructuredBufferInfo
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the intended usage for binding to the GPU.
        /// </summary>
        /// <remarks>
        /// The default value is <c>Default</c>.
        /// </remarks>
        public D3D11.ResourceUsage Usage
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the size of the buffer, in bytes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value should be larger than 0, or else an exception will be thrown when the buffer is created.
        /// </para>
        /// </remarks>
        public int SizeInBytes
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the type of binding for the GPU.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The type of binding should be used to determine what type of view to apply to the buffer when accessing it from shaders. This will also help determine how data will be interpreted.
        /// </para>
        /// <para>
        /// Different bindings may be applied at the same time by OR'ing the <see cref="BufferBinding"/> flags together.
        /// </para>
        /// <para>
        /// If the <see cref="IGorgonStructuredBufferInfo.Usage"/> is set to <c>Staging</c>, then this value must be set to <see cref="BufferBinding.None"/>, otherwise an exception will be raised when the buffer is created.
        /// </para>
        /// <para>
        /// The default value is <see cref="BufferBinding.Shader"/>
        /// </para>
        /// </remarks>
        public BufferBinding Binding
        {
            get;
            set;
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
        public int StructureSize
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
        public bool UseDefaultShaderView
        {
            get;
            set;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonStructuredBufferInfo"/> class.
        /// </summary>
        /// <param name="info">The information to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
        public GorgonStructuredBufferInfo(IGorgonStructuredBufferInfo info)
        {
            Usage = info?.Usage ?? throw new ArgumentNullException(nameof(info));
            Binding = info.Binding;
            SizeInBytes = info.SizeInBytes;
            StructureSize = info.StructureSize;
            UseDefaultShaderView = info.UseDefaultShaderView;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonStructuredBufferInfo"/> class.
        /// </summary>
        public GorgonStructuredBufferInfo()
        {
            Usage = D3D11.ResourceUsage.Default;
            Binding = BufferBinding.Shader;
        }
        #endregion
    }
}
