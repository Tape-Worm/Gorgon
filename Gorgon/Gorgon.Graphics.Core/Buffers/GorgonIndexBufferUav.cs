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

using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Provides an unordered access view for a <see cref="GorgonIndexBuffer"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type of view allows for unordered access to a <see cref="GorgonIndexBuffer"/>. The buffer must have been created with the <see cref="BufferBinding.UnorderedAccess"/> flag in its 
    /// <see cref="IGorgonIndexBufferInfo.Binding"/> property.
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
    public sealed class GorgonIndexBufferUav
        : GorgonBufferUavBase<GorgonIndexBuffer>
    {
        #region Properties.
        /// <summary>
        /// Property to return the format for the view.
        /// </summary>
        public DXGI.Format Format
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
        /// Property to return the size of an element.
        /// </summary>
        public override int ElementSize => FormatInformation.SizeInBytes;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to initialize the unordered access view.
        /// </summary>
        protected internal override void CreateNativeView()
        {
            Log.Print("Creating buffer unordered access view for {0}.", LoggingLevel.Verbose, Resource.Name);

            var desc = new D3D11.UnorderedAccessViewDescription
                       {
                           Dimension = D3D11.UnorderedAccessViewDimension.Buffer,
                           Buffer =
                           {
                               FirstElement = ElementStart,
                               ElementCount = ElementCount,
                               Flags = D3D11.UnorderedAccessViewBufferFlags.None
                           },
                           Format = Format
                       };

            NativeView = new D3D11.UnorderedAccessView(Resource.Graphics.VideoDevice.D3DDevice(), Resource.D3DResource, desc)
                         {
                             DebugName = $"'{Buffer.Name}': D3D 11 Unordered access view"
                         };
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonIndexBufferUav"/> class.
        /// </summary>
        /// <param name="buffer">The buffer to assign to the view.</param>
        /// <param name="elementStart">The first element in the buffer to view.</param>
        /// <param name="elementCount">The number of elements in the view.</param>
        /// <param name="format">The format of the view.</param>
        /// <param name="formatInfo">Information about the format.</param>
        /// <param name="log">The log used for debug information.</param>
        internal GorgonIndexBufferUav(GorgonIndexBuffer buffer, int elementStart, int elementCount, DXGI.Format format, GorgonFormatInfo formatInfo, IGorgonLog log)
            : base(buffer, elementStart, elementCount, log)
        {
            Format = format;
            FormatInformation = formatInfo;

            if (FormatInformation.IsTypeless)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_VIEW_NO_TYPELESS);
            }
        }
        #endregion
    }
}
