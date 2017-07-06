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
// Created: July 4, 2017 11:54:56 PM
// 
#endregion

using System;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using D3D = SharpDX.Direct3D;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A shader resource view for a <see cref="GorgonBuffer"/>.
    /// </summary>
    public class GorgonBufferView
        : GorgonBufferViewBase<GorgonBuffer>
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
        /// Function to initialize the view.
        /// </summary>
        protected internal override void CreateNativeView()
        {
            Log.Print($"Creating buffer shader view for {Buffer.Name}.", LoggingLevel.Verbose);

            var desc = new D3D11.ShaderResourceViewDescription
                                                 {
                                                     Format = Format,
                                                     Dimension = D3D.ShaderResourceViewDimension.ExtendedBuffer,
                                                     BufferEx = new D3D11.ShaderResourceViewDescription.ExtendedBufferResource
                                                                {
                                                                    FirstElement = StartElement,
                                                                    ElementCount = ElementCount,
                                                                    Flags = D3D11.ShaderResourceViewExtendedBufferFlags.None
                                                                }
                                                 };

            // Create our SRV.
            NativeView = new D3D11.ShaderResourceView(Buffer.Graphics.VideoDevice.D3DDevice(), Buffer.D3DResource, desc)
                         {
                             DebugName = $"'{Buffer.Name}': D3D 11 Shader resource view"
                         };
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBufferView"/> class.
        /// </summary>
        /// <param name="buffer">The buffer to bind to the view.</param>
        /// <param name="format">The format of the view into the buffer.</param>
        /// <param name="startingElement">The starting element in the buffer to view.</param>
        /// <param name="elementCount">The number of elements in the buffer to view.</param>
        /// <param name="totalElementCount">The total number of elements in the buffer.</param>
        /// <param name="formatInfo">The information about the format being used.</param>
        /// <param name="log">The logging interface used for debug logging.</param>
        internal GorgonBufferView(GorgonBuffer buffer,
                                      DXGI.Format format,
                                      int startingElement,
                                      int elementCount,
                                      int totalElementCount,
                                      GorgonFormatInfo formatInfo,
                                      IGorgonLog log)
            : base(buffer, startingElement, elementCount, totalElementCount, log)
        {
            if (format == DXGI.Format.Unknown)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_VIEW_UNKNOWN_FORMAT, Format), nameof(format));
            }

            if (formatInfo.IsTypeless)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_VIEW_NO_TYPELESS));
            }

            // Ensure that the elements are within the total size of the buffer.
            if (startingElement + elementCount > totalElementCount)
            {
                throw new ArgumentOutOfRangeException(string.Format(Resources.GORGFX_ERR_BUFFER_VIEW_START_COUNT_OUT_OF_RANGE, startingElement, elementCount, TotalElementCount));
            }
            
            Format = format;
            FormatInformation = formatInfo;
        }
        #endregion
    }
}
