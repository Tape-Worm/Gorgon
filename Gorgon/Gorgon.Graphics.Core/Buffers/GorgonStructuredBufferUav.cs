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
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using Gorgon.Diagnostics;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// The type of unordered access view for a <see cref="GorgonStructuredBufferUav"/>.
    /// </summary>
    [Flags]
    public enum StructuredBufferUavType
    {
        /// <summary>
        /// A regular unordered access view for structured buffers.
        /// </summary>
        None = D3D11.UnorderedAccessViewBufferFlags.None,
        /// <summary>
        /// An append/consume unordered access view.
        /// </summary>
        Append = D3D11.UnorderedAccessViewBufferFlags.Append,
        /// <summary>
        /// A counter unordered access view.
        /// </summary>
        Counter = D3D11.UnorderedAccessViewBufferFlags.Counter
    }

    /// <summary>
    /// Provides an unordered access view for a <see cref="GorgonStructuredBuffer"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type of view allows for unordered access to a <see cref="GorgonStructuredBuffer"/>. The buffer must have been created with the <see cref="BufferBinding.UnorderedAccess"/> flag in its 
    /// <see cref="IGorgonStructuredBufferInfo.Binding"/> property.
    /// </para>
    /// <para>
    /// The unordered access allows a shader to read/write any part of a <see cref="GorgonResource"/> by multiple threads without memory contention. This is done through the use of 
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
    /// <seealso cref="GorgonResource"/>
    /// <seealso cref="GorgonTexture"/>
    /// <seealso cref="GorgonComputeShader"/>
    /// <seealso cref="GorgonPixelShader"/>
    /// <seealso cref="GorgonDrawCallBase"/>
    public sealed class GorgonStructuredBufferUav
        : GorgonBufferUavBase<GorgonStructuredBuffer>
    {
        #region Properties.
        /// <summary>
        /// Property to return the type of view.
        /// </summary>
        public StructuredBufferUavType UavType
        {
            get;
        }

        /// <summary>
        /// Property to return the size of an element.
        /// </summary>
        public override int ElementSize => Buffer.StructureSize;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to initialize the unordered access view.
        /// </summary>
        protected internal override void CreateNativeView()
        {
            Log.Print("Creating structured buffer unordered access view for {0}.", LoggingLevel.Verbose, Resource.Name);

            var desc = new D3D11.UnorderedAccessViewDescription
                       {
                           Dimension = D3D11.UnorderedAccessViewDimension.Buffer,
                           Buffer =
                           {
                               FirstElement = ElementStart,
                               ElementCount = ElementCount,
                               Flags = (D3D11.UnorderedAccessViewBufferFlags)UavType
                           },
                           Format = DXGI.Format.Unknown
                       };

            NativeView = new D3D11.UnorderedAccessView(Resource.Graphics.VideoDevice.D3DDevice(), Resource.D3DResource, desc)
                         {
                             DebugName = $"'{Buffer.Name}': D3D 11 Unordered access view"
                         };
        }

        /// <summary>
        /// Function to copy the structure count from this view into a buffer.
        /// </summary>
        /// <param name="buffer">The buffer that will receive the data.</param>
        /// <param name="offset">[Optional] The offset, in bytes, within the buffer attached to this view to start reading from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> is less than 0, or not less than the size of the buffer minus 4 bytes.</exception>
        /// <remarks>
        /// <para>
        /// When the structure unordered access view is set up with a <see cref="StructuredBufferUavType.Append"/>, or <see cref="StructuredBufferUavType.Counter"/>, the values updated by these flags are 
        /// not readily accessible from the CPU. To retrieve these values, this method must be called to retrieve the values. These values are copied into the <paramref name="buffer"/> provided to the 
        /// method so that applications can make use of data generated on the GPU. Note that this value will be written out as a 32 bit unsigned integer.
        /// </para>
        /// <para>
        /// If the unordered access view does not specify the appropriate values on the <see cref="UavType"/>, then this method will do nothing.
        /// </para>
        /// <para> 
        /// <note type="important">
        /// <para>
        /// For performance reasons, exceptions will only be thrown from this method when Gorgon is compiled as <b>DEBUG</b>.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void CopyStructureCount(GorgonBufferCommon buffer, int offset = 0)
        {
            buffer.ValidateObject(nameof(buffer));
            offset.ValidateRange(nameof(offset), 0, Buffer.SizeInBytes - 4);

            buffer.Graphics.D3DDeviceContext.CopyStructureCount(buffer.NativeBuffer, offset, NativeView);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonStructuredBufferUav"/> class.
        /// </summary>
        /// <param name="buffer">The buffer to assign to the view.</param>
        /// <param name="elementStart">The first element in the buffer to view.</param>
        /// <param name="elementCount">The number of elements in the view.</param>
        /// <param name="uavType">Flags used to indicate the purpose of this view.</param>
        /// <param name="log">The log used for debug information.</param>
        internal GorgonStructuredBufferUav(GorgonStructuredBuffer buffer,
                                                     int elementStart,
                                                     int elementCount,
                                                     StructuredBufferUavType uavType,
                                                     IGorgonLog log)
            : base(buffer, elementStart, elementCount, log)
        {
            UavType = uavType;
        }
        #endregion
    }
}
