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

using Gorgon.Diagnostics;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A shader resource view for <see cref="GorgonStructuredBuffer"/>.
    /// </summary>
    public class GorgonStructuredBufferView
        : GorgonBufferViewBase<GorgonStructuredBuffer>
    {
        #region Properties.
        /// <summary>
        /// Property to return the size of an element, in bytes based on the <see cref="GorgonStructuredBuffer.StructureSize"/> defined.
        /// </summary>
        public override int ElementSize => Buffer.StructureSize;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to initialize the view.
        /// </summary>
        protected internal override void CreateNativeView()
        {
            Log.Print($"Creating structured buffer shader view for {Buffer.Name}.", LoggingLevel.Verbose);

            var desc = new D3D11.ShaderResourceViewDescription
                       {
                           Format = DXGI.Format.Unknown,
                           Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.ExtendedBuffer,
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
        /// Initializes a new instance of the <see cref="GorgonStructuredBufferView"/> class.
        /// </summary>
        /// <param name="buffer">The buffer to bind to the view.</param>
        /// <param name="startingElement">The starting element in the buffer to view.</param>
        /// <param name="elementCount">The number of elements in the buffer to view.</param>
        /// <param name="totalElementCount">The total number of elements in the buffer.</param>
        /// <param name="log">The logging interface used for debug logging.</param>
        internal GorgonStructuredBufferView(GorgonStructuredBuffer buffer,
                                                  int startingElement,
                                                  int elementCount,
                                                  int totalElementCount,
                                                  IGorgonLog log)
            : base(buffer, startingElement, elementCount, totalElementCount, log)
        {
        }
        #endregion
    }
}
