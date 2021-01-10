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

using System;
using Gorgon.Core;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Provides a read/write (unordered access) view for a <see cref="GorgonVertexBuffer"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type of view allows for unordered access to a <see cref="GorgonVertexBuffer"/>. The buffer must have been created with the <see cref="BufferBinding.ReadWrite"/> flag in its 
    /// <see cref="IGorgonVertexBufferInfo.Binding"/> property.
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
    public sealed class GorgonVertexBufferReadWriteView
        : GorgonBufferReadWriteViewCommon<GorgonVertexBuffer>, IGorgonVertexBufferInfo
    {
        #region Properties.
        /// <summary>
        /// Property to return the format used to interpret this view.
        /// </summary>
        public BufferFormat Format
        {
            get;
        }

        /// <summary>
        /// Property to return information about the <see cref="Format"/> used by this view.
        /// </summary>
        public GorgonFormatInfo FormatInformation
        {
            get;
        }

        /// <summary>
        /// Property to return the size of an element, in bytes.
        /// </summary>
        public override int ElementSize => FormatInformation.SizeInBytes;

        /// <summary>
        /// Property to return the size of the buffer, in bytes.
        /// </summary>
        int IGorgonVertexBufferInfo.SizeInBytes => Buffer?.SizeInBytes ?? 0;

        /// <summary>
        /// Property to return the binding used to bind this buffer to the GPU.
        /// </summary>
        public VertexIndexBufferBinding Binding => Buffer?.Binding ?? VertexIndexBufferBinding.None;

        /// <summary>
        /// Property to return the name of this object.
        /// </summary>
        string IGorgonNamedObject.Name => Buffer?.Name ?? string.Empty;
        #endregion

        #region Methods.
        /// <summary>Function to retrieve the necessary parameters to create the native view.</summary>
        /// <returns>The D3D11 UAV descriptor.</returns>
        private protected override ref readonly D3D11.UnorderedAccessViewDescription1 OnGetUavParams()
        {
            UavDesc = new D3D11.UnorderedAccessViewDescription1
            {
                Dimension = D3D11.UnorderedAccessViewDimension.Buffer,
                Buffer =
                {
                    FirstElement = StartElement,
                    ElementCount = ElementCount,
                    Flags = D3D11.UnorderedAccessViewBufferFlags.None
                },
                Format = (DXGI.Format)Format
            };

            return ref UavDesc;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonVertexBufferReadWriteView"/> class.
        /// </summary>
        /// <param name="buffer">The buffer to assign to the view.</param>
        /// <param name="format">The format of the view.</param>
        /// <param name="formatInfo">Information about the format.</param>
        /// <param name="elementStart">The first element in the buffer to view.</param>
        /// <param name="elementCount">The number of elements in the view.</param>
        /// <param name="totalElementCount">The total number of elements in the vertex buffer.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/>, or the <paramref name="formatInfo"/> parameter is <b>null</b>.</exception>
        internal GorgonVertexBufferReadWriteView(GorgonVertexBuffer buffer, BufferFormat format, GorgonFormatInfo formatInfo, int elementStart, int elementCount, int totalElementCount)
            : base(buffer, elementStart, elementCount, totalElementCount)
        {
            FormatInformation = formatInfo ?? throw new ArgumentNullException(nameof(formatInfo));
            Format = format;
        }
        #endregion
    }
}
