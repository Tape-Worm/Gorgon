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
// Created: July 5, 2017 11:30:50 PM
// 
#endregion

using System;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// The types of elements that the view will interpret the raw data as.
    /// </summary>
    public enum RawBufferElementType
    {
        /// <summary>
        /// Each element is a 32 bit signed integer.
        /// </summary>
        Int32 = 0,
        /// <summary>
        /// Each element is a 32 bit unsigned integer.
        /// </summary>
        UInt32 = 1,
        /// <summary>
        /// Each element is a single precision floating point value.
        /// </summary>
        Single = 2
    }

    /// <summary>
    /// A shader resource view for a <see cref="GorgonRawBuffer"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a generic view to allow a <see cref="GorgonRawBuffer"/> to be bound to the GPU pipeline as a raw byte buffer resource.
    /// </para>
    /// <para>
    /// Use a resource view to allow a shader access to the contents of a resource (or sub resource).  When the resource is created with a typeless format, this will allow the resource to be cast to any 
    /// format within the same group.	
    /// </para>
    /// <para>
    /// This type of view will allow shaders to access the data in the buffer by treating each data item as an "element".  The size of these elements is always 4 bytes and can be interpreted as a specific 
    /// type of data (signed integer, unsigned integer, and single floating point precision) which is defined via the <see cref="RawBufferElementType"/> enumeration. 
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonRawBuffer"/>
    public sealed class GorgonRawBufferView
        : GorgonBufferViewBase<GorgonRawBuffer>
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
        /// Property to return the type of elements in the buffer.
        /// </summary>
        public RawBufferElementType ElementType
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
        /// Property to return the size of the buffer, in bytes.
        /// </summary>
        public int SizeInBytes => Buffer.SizeInBytes;

        /// <summary>
        /// Property to return the size of an element, in bytes based on the <see cref="Format"/> defined.
        /// </summary>
        public override int ElementSize => 4;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to initialize the buffer view.
        /// </summary>
        protected internal override void CreateNativeView()
        {
            Log.Print($"Creating raw buffer shader view for {Buffer.Name}.", LoggingLevel.Verbose);

            D3D11.ShaderResourceViewDescription desc = new D3D11.ShaderResourceViewDescription
                       {
                           Format = (DXGI.Format)Format,
                           Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.ExtendedBuffer,
                           BufferEx = new D3D11.ShaderResourceViewDescription.ExtendedBufferResource
                                      {
                                          FirstElement = StartElement,
                                          ElementCount = ElementCount,
                                          Flags = D3D11.ShaderResourceViewExtendedBufferFlags.Raw
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
        /// Initializes a new instance of the <see cref="GorgonRawBufferView"/> class.
        /// </summary>
        /// <param name="buffer">The buffer to bind to the view.</param>
        /// <param name="elementType">The type of the elements stored within the buffer.</param>
        /// <param name="startingElement">The starting element in the buffer to view.</param>
        /// <param name="elementCount">The number of elements in the buffer to view.</param>
        /// <param name="totalElementCount">The total number of elements in the buffer.</param>
        /// <param name="log">The logging interface used for debug logging.</param>
        internal GorgonRawBufferView(GorgonRawBuffer buffer,
                                      RawBufferElementType elementType,
                                      int startingElement,
                                      int elementCount,
                                      int totalElementCount,
                                      IGorgonLog log)
            : base(buffer, startingElement, elementCount, totalElementCount, log)
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
        }
        #endregion
    }
}
