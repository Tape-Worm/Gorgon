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
#if NET5_0_OR_GREATER
    /// <summary>
    /// Provides the necessary information required to set up a generic unstructured buffer.
    /// </summary>
    /// <param name="SizeInBytes">The size of the buffer, in bytes.</param>
    /// <remarks>
    /// <para>
    /// For buffers that set <see cref="IGorgonBufferInfo.AllowRawView"/> to <b>true</b>, the <see cref="SizeInBytes"/> value will be rounded up to the nearest multiple of 4 at buffer creation time.
    /// </para>
    /// </remarks>
    public record GorgonBufferInfo(int SizeInBytes)
        : IGorgonBufferInfo
    {
    #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBufferInfo"/> class.
        /// </summary>
        /// <param name="info">The buffer information to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
        public GorgonBufferInfo(IGorgonBufferInfo info)
            : this(info?.SizeInBytes ?? throw new ArgumentNullException(nameof(info)))
        {
            Name = info.Name;
            Usage = info.Usage;
            Binding = info.Binding;
            AllowCpuRead = info.AllowCpuRead;
            StructureSize = info.StructureSize;
            AllowRawView = info.AllowRawView;
            IndirectArgs = info.IndirectArgs;
        }
    #endregion

    #region Properties.
        /// <summary>
        /// Property to return whether to allow the CPU read access to the buffer.
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
        /// The default for this value is <b>false</b>.
        /// </para>
        /// </remarks>
        public bool AllowCpuRead
        {
            get;
            init;
        }

        /// <summary>
        /// Property to return the intended usage for binding to the GPU.
        /// </summary>
        public ResourceUsage Usage
        {
            get;
            init;
        } = ResourceUsage.Default;

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
        /// The default value is <see cref="BufferBinding.None"/>
        /// </para>
        /// </remarks>
        public BufferBinding Binding
        {
            get;
            init;
        } = BufferBinding.None;

        /// <summary>
        /// Property to return whether the buffer will contain indirect argument data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This flag only applies to buffers with a <see cref="IGorgonBufferInfo.Binding"/> of <see cref="BufferBinding.ReadWrite"/>, and/or <see cref="BufferBinding.Shader"/>. If the binding is set to anything else, 
        /// then this flag is ignored.
        /// </para>
        /// <para>
        /// The default value is <b>false</b>.
        /// </para>
        /// </remarks>
        public bool IndirectArgs
        {
            get;
            init;
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
            init;
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
        /// This value is only used when the <see cref="IGorgonBufferInfo.Binding"/> property has the <see cref="BufferBinding.ReadWrite"/> flag. 
        /// </para>
        /// <para>
        /// The default value is <b>false</b>.
        /// </para>
        /// </remarks>
        public bool AllowRawView
        {
            get;
            init;
        }

        /// <summary>
        /// Property to return the name of this object.
        /// </summary>
        /// <remarks>
        /// For best practice, the name should only be set once during the lifetime of an object. Hence, this interface only provides a read-only implementation of this 
        /// property.
        /// </remarks>
        public string Name
        {
            get;
            init;
        } = GorgonGraphicsResource.GenerateName(GorgonBuffer.NamePrefix);
    #endregion
    }
#else
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
        public int SizeInBytes
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the type of binding for the GPU.
        /// </summary>
        public BufferBinding Binding
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return whether the buffer will contain indirect argument data.
        /// </summary>
        public bool IndirectArgs
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the size, in bytes, of an individual structure in a structured buffer.
        /// </summary>
        public int StructureSize
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return whether to allow raw unordered views of the buffer.
        /// </summary>
        public bool AllowRawView
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the name of this object.
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBufferInfo"/> class.
        /// </summary>
        /// <param name="info">The buffer information to copy.</param>
        /// <param name="newName">[Optional] The new name for the buffer.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
        public GorgonBufferInfo(IGorgonBufferInfo info, string newName = null)
        {
            Name = newName;
            Usage = info.Usage;
            SizeInBytes = info.SizeInBytes;
            Binding = info.Binding;
            AllowCpuRead = info.AllowCpuRead;
            StructureSize = info.StructureSize;
            AllowRawView = info.AllowRawView;
            IndirectArgs = info.IndirectArgs;
        }
        #endregion
    }
#endif
}
