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
// Created: July 5, 2017 2:44:43 PM
// 
#endregion

using System;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Provides the necessary information required to set up a generic unstructured buffer.
    /// </summary>
    public class GorgonBufferInfo
        : IGorgonBufferInfo
    {
        #region Properties.
        /// <summary>
        /// Property to set or return whether to allow the CPU read access to the buffer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default for this value is <b>false</b>.
        /// </para>
        /// </remarks>
        public bool AllowCpuRead
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the intended usage for binding to the GPU.
        /// </summary>
        public ResourceUsage Usage
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the size of the buffer, in bytes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For buffers that set <see cref="IGorgonBufferInfo.AllowRawView"/> to <b>true</b>, then this value will be rounded up to the nearest multiple of 4 at buffer creation time.
        /// </para>
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
        /// If the <see cref="IGorgonBufferInfo.Usage"/> is set to <see cref="ResourceUsage.Staging"/>, then this value must be set to <see cref="BufferBinding.None"/>, otherwise an exception will be raised 
        /// when the buffer is created.
        /// </para>
        /// <para>
        /// This value must be set to <see cref="BufferBinding.None"/> for constant buffers. If it is not, it will be reset to <see cref="BufferBinding.None"/> upon buffer creation.
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
        /// Property to set or return the format for the default shader view.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use this to define a default <see cref="GorgonBufferView"/> for the buffer. This default view will allow shaders to access the buffer without needing to create an additional view. If this 
        /// value is set to <see cref="BufferFormat.Unknown"/>, then no default shader view will be created.
        /// </para>
        /// <para>
        /// The default shader view will expose the entire buffer to the shader. To limit view to only a portion of the buffer, call the <see cref="GorgonBuffer.GetShaderResourceView"/> with the appropriate 
        /// element constraints.
        /// </para>
        /// <para>
        /// The format must not be typeless, if it is, an exception will be thrown on buffer creation.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// This property is only used if the <see cref="IGorgonBufferInfo.Binding"/> property has a <see cref="BufferBinding.Shader"/> flag.
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// The default value for this property is <see cref="BufferFormat.Unknown"/>.
        /// </para>
        /// </remarks>
        public BufferFormat DefaultShaderViewFormat
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return whether the buffer will contain indirect argument data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This flag only applies to buffers with a <see cref="IGorgonBufferInfo.Binding"/> of <see cref="BufferBinding.UnorderedAccess"/>, and/or <see cref="BufferBinding.Shader"/>. If the binding is set to anything else, 
        /// then this flag is ignored.
        /// </para>
        /// <para>
        /// The default value is <b>false</b>.
        /// </para>
        /// </remarks>
        public bool IndirectArgs
        {
            get;
            set;
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
        /// Vertex and index buffers will ignore this flag and will reset it to 0.
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
        /// Property to return whether to allow raw unordered views of the buffer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When this value is <b>true</b>, then unordered access views for this buffer can use byte addressing to read/write the buffer in a shader. If it is <b>false</b>, then the SRV format or 
        /// <see cref="IGorgonBufferInfo.StructureSize"/> will determine how to address data in the buffer.
        /// </para>
        /// <para>
        /// This value is only used when the <see cref="IGorgonBufferInfo.Binding"/> property has the <see cref="BufferBinding.UnorderedAccess"/> flag. 
        /// </para>
        /// <para>
        /// The default value is <b>false</b>.
        /// </para>
        /// </remarks>
        public bool AllowRawView
        {
            get;
            set;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBufferInfo"/> class.
        /// </summary>
        /// <param name="bufferInfo">The buffer information to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="bufferInfo"/> parameter is <b>null</b>.</exception>
        public GorgonBufferInfo(IGorgonBufferInfo bufferInfo)
        {
            Usage = bufferInfo?.Usage ?? throw new ArgumentNullException(nameof(bufferInfo));
            SizeInBytes = bufferInfo.SizeInBytes;
            Binding = bufferInfo.Binding;
            DefaultShaderViewFormat = bufferInfo.DefaultShaderViewFormat;
            AllowCpuRead = bufferInfo.AllowCpuRead;
            StructureSize = bufferInfo.StructureSize;
            AllowRawView = bufferInfo.AllowRawView;
            IndirectArgs = bufferInfo.IndirectArgs;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBufferInfo"/> class.
        /// </summary>
        public GorgonBufferInfo()
        {
            Usage = ResourceUsage.Default;
            DefaultShaderViewFormat = BufferFormat.Unknown;
            Binding = BufferBinding.Shader;
        }
        #endregion
    }
}
