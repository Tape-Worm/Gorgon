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
// Created: July 22, 2017 10:31:48 AM
// 
#endregion

using Gorgon.Core;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Provides a read/write (unordered access) view for a <see cref="GorgonBuffer"/>, exposing the data as raw byte data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type of view allows for unordered access to a <see cref="GorgonBuffer"/>. The buffer must have been created with the <see cref="BufferBinding.ReadWrite"/> flag in its 
    /// <see cref="IGorgonBufferInfo.Binding"/> property, and the <see cref="IGorgonBufferInfo.AllowRawView"/> flag must have been set to <b>true</b>.
    /// </para>
    /// <para>
    /// The unordered access allows a shader to read/write any part of a <see cref="GorgonGraphicsResource"/> by multiple threads without memory contention. This is done through the use of 
    /// <a target="_blank" href="https://msdn.microsoft.com/en-us/library/windows/desktop/ff476334(v=vs.85).aspx">atomic functions</a>.
    /// </para>
    /// <para>
    /// These types of views are most useful for <see cref="GorgonComputeShader"/> shaders, but can also be used by a <see cref="GorgonPixelShader"/> by passing a list of these views in to a 
    /// <see cref="GorgonDrawCallCommon">draw call</see>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsResource"/>
    /// <seealso cref="GorgonComputeShader"/>
    /// <seealso cref="GorgonPixelShader"/>
    /// <seealso cref="GorgonDrawCallCommon"/>
    public sealed class GorgonRawReadWriteView
        : GorgonBufferReadWriteViewCommon<GorgonBuffer>, IGorgonBufferInfo
    {
        #region Properties.
        /// <summary>
        /// Property to return the type of element stored in the buffer.
        /// </summary>
        public RawBufferElementType ElementType
        {
            get;
        }

        /// <summary>
        /// Property to return the size of an element, in bytes.
        /// </summary>
        /// <remarks>
        /// For a raw buffer view, this will always be 4.
        /// </remarks>
        public override int ElementSize => 4;

        /// <summary>
        /// Property to set or return whether to allow the CPU read access to the buffer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value indicates whether or not the CPU can directly access the buffer for reading. If this value is <b>false</b>, the buffer still can be read, but will be done through an intermediate
        /// staging buffer, which is obviously less performant. 
        /// </para>
        /// <para>
        /// This value is treated as <b>false</b> if the buffer does not have a <see cref="IGorgonBufferInfo.Binding"/> containing the <see cref="BufferBinding.Shader"/> flag, and does not have a
        /// <see cref="IGorgonBufferInfo.Usage"/> of <see cref="ResourceUsage.Default"/>. This means any reads will be done through an intermediate staging buffer, impacting performance.
        /// </para>
        /// <para>
        /// If the <see cref="IGorgonBufferInfo.Usage"/> property is set to <see cref="ResourceUsage.Staging"/>, then this value is treated as <b>true</b> because staging buffers are CPU only and as such,
        /// can be read directly by the CPU regardless of this value.
        /// </para>
        /// </remarks>
        public bool AllowCpuRead => Buffer?.IsCpuReadable ?? false;

        /// <summary>
        /// Property to return the size, in bytes, of an individual structure in a structured buffer.
        /// </summary>
        /// <remarks>
        /// This value will be rounded to the nearest multiple of 4.
        /// </remarks>
        int IGorgonBufferInfo.StructureSize => 0;

        /// <summary>
        /// Property to return whether to allow raw unordered views of the buffer.
        /// </summary>
        /// <remarks>
        /// This value is always <b>false</b> for this type of view.
        /// </remarks>
        public bool AllowRawView => true;

        /// <summary>
        /// Property to return the type of binding for the GPU.
        /// </summary>
        public BufferBinding Binding => Buffer?.Binding ?? BufferBinding.None;

        /// <summary>
        /// Property to return whether the buffer will contain indirect argument data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This flag only applies to buffers with a <see cref="IGorgonBufferInfo.Binding"/> of <see cref="BufferBinding.ReadWrite"/>, and/or <see cref="BufferBinding.Shader"/>. If the binding is set
        /// to anything else, then this flag is treated as being set to <b>false</b>.
        /// </para>
        /// <para>
        /// The default value is <b>false</b>.
        /// </para>
        /// </remarks>
        public bool IndirectArgs => Buffer?.IndirectArgs ?? false;

        /// <summary>
        /// Property to return the size of the buffer, in bytes.
        /// </summary>
        int IGorgonBufferInfo.SizeInBytes => Buffer?.SizeInBytes ?? 0;

        /// <summary>
        /// Property to return the name of this object.
        /// </summary>
        string IGorgonNamedObject.Name => Buffer?.Name;
        #endregion

        #region Methods.        
        /// <summary>Function to retrieve the necessary parameters to create the native view.</summary>
        /// <returns>The D3D11 UAV descriptor.</returns>
        private protected override ref readonly D3D11.UnorderedAccessViewDescription1 OnGetUavParams()
        {
            BufferFormat format = BufferFormat.Unknown;

            switch (ElementType)
            {
                case RawBufferElementType.Int32:
                    format = BufferFormat.R32_SInt;
                    break;
                case RawBufferElementType.UInt32:
                    format = BufferFormat.R32_UInt;
                    break;
                case RawBufferElementType.Single:
                    format = BufferFormat.R32_Float;
                    break;
            }

            UavDesc = new D3D11.UnorderedAccessViewDescription1
            {
                Dimension = D3D11.UnorderedAccessViewDimension.Buffer,
                Buffer =
                {
                    FirstElement = StartElement,
                    ElementCount = ElementCount,
                    Flags = D3D11.UnorderedAccessViewBufferFlags.Raw
                },
                Format = (DXGI.Format)format
            };

            return ref UavDesc;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRawReadWriteView"/> class.
        /// </summary>
        /// <param name="buffer">The buffer to assign to the view.</param>
        /// <param name="elementStart">The first element in the buffer to view.</param>
        /// <param name="elementCount">The number of elements in the view.</param>
        /// <param name="totalElementCount">The total number of elements in the buffer.</param>
        /// <param name="elementType">The type of element data in the buffer.</param>
        internal GorgonRawReadWriteView(GorgonBuffer buffer, int elementStart, int elementCount, int totalElementCount, RawBufferElementType elementType)
            : base(buffer, elementStart, elementCount, totalElementCount) => ElementType = elementType;
        #endregion
    }
}
