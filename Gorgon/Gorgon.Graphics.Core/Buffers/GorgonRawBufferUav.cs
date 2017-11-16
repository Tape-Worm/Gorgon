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
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Provides an unordered access view for a <see cref="GorgonRawBuffer"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type of view allows for unordered access to a <see cref="GorgonRawBuffer"/>. The buffer must have been created with the <see cref="BufferBinding.UnorderedAccess"/> flag in its 
    /// <see cref="IGorgonRawBufferInfo.Binding"/> property.
    /// </para>
    /// <para>
    /// The unordered access allows a shader to read/write any part of a <see cref="GorgonGraphicsResource"/> by multiple threads without memory contention. This is done through the use of 
    /// <a target="_blank" href="https://msdn.microsoft.com/en-us/library/windows/desktop/ff476334(v=vs.85).aspx">atomic functions</a>.
    /// </para>
    /// <para>
    /// These types of views are most useful for <see cref="GorgonComputeShader"/> shaders, but can also be used by a <see cref="GorgonPixelShader"/> by passing a list of these views in to a 
    /// <see cref="GorgonDrawCallBase">draw call</see>.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// Unordered access views do not support multisampled <see cref="GorgonTexture"/>s.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsResource"/>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonComputeShader"/>
    /// <seealso cref="GorgonPixelShader"/>
    /// <seealso cref="GorgonDrawCallBase"/>
    public sealed class GorgonRawBufferUav
        : GorgonBufferUavBase<GorgonRawBuffer>
    {
        #region Properties.
        /// <summary>
        /// Property to return the format for the view.
        /// </summary>
        public BufferFormat Format
        {
            get;
        }

        /// <summary>
        /// Property to return information about the <see cref="Format"/> assigned to this view.
        /// </summary>
        public GorgonFormatInfo FormatInformation
        {
            get;
        }

        /// <summary>
        /// Property to return the type of element stored in the buffer.
        /// </summary>
        public RawBufferElementType ElementType
        {
            get;
        }

        /// <summary>
        /// Property to return the size of the buffer, in bytes.
        /// </summary>
        public int SizeInBytes => Buffer.SizeInBytes;

        /// <summary>
        /// Property to return the size of an element.
        /// </summary>
        public override int ElementSize => 4;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to initialize the unordered access view.
        /// </summary>
        protected internal override void CreateNativeView()
        {
            Log.Print("Creating raw buffer unordered access view for {0}.", LoggingLevel.Verbose, Resource.Name);
            D3D11.UnorderedAccessViewDescription desc = new D3D11.UnorderedAccessViewDescription
                       {
                           Dimension = D3D11.UnorderedAccessViewDimension.Buffer,
                           Buffer =
                           {
                               FirstElement = ElementStart,
                               ElementCount = ElementCount,
                               Flags = D3D11.UnorderedAccessViewBufferFlags.Raw
                           },
                           Format = (DXGI.Format)Format
                       };

            NativeView = new D3D11.UnorderedAccessView(Resource.Graphics.VideoDevice.D3DDevice(), Resource.D3DResource, desc)
                         {
                             DebugName = $"'{Buffer.Name}': D3D 11 Unordered access view (raw)"
                         };
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRawBufferUav"/> class.
        /// </summary>
        /// <param name="buffer">The buffer to assign to the view.</param>
        /// <param name="elementStart">The first element in the buffer to view.</param>
        /// <param name="elementCount">The number of elements in the view.</param>
        /// <param name="elementType">The type of element data in the buffer.</param>
        /// <param name="log">The log used for debug information.</param>
        internal GorgonRawBufferUav(GorgonRawBuffer buffer, int elementStart, int elementCount, RawBufferElementType elementType, IGorgonLog log)
            : base(buffer, elementStart, elementCount, log)
        {
            ElementType = elementType;

            switch (elementType)
            {
                case RawBufferElementType.Int32:
                    Format = BufferFormat.R32_SInt;
                    break;
                case RawBufferElementType.UInt32:
                    Format = BufferFormat.R32_UInt;
                    break;
                case RawBufferElementType.Single:
                    Format = BufferFormat.R32_Float;
                    break;
                default:
                    throw new ArgumentException(string.Format(Resources.GORGFX_ERR_VIEW_UNKNOWN_FORMAT, BufferFormat.Unknown), nameof(elementType));
            }

            FormatInformation = new GorgonFormatInfo(Format);

            if ((buffer.Graphics.VideoDevice.GetBufferFormatSupport(Format) & D3D11.FormatSupport.TypedUnorderedAccessView) != D3D11.FormatSupport.TypedUnorderedAccessView)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_FORMAT_INVALID, Format));
            }
        }
        #endregion
    }
}
