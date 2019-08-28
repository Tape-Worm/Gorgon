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
using Gorgon.Diagnostics;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// The type of unordered access view for a <see cref="GorgonStructuredReadWriteView"/>.
    /// </summary>
    [Flags]
    public enum StructuredBufferReadWriteViewType
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
    /// Provides a read/write (unordered access) view for a <see cref="GorgonBuffer"/> containing structured data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type of view allows for unordered access to a <see cref="GorgonBuffer"/>. The buffer must have been created with the <see cref="BufferBinding.ReadWrite"/> flag in its 
    /// <see cref="IGorgonBufferInfo.Binding"/> property, and have a <see cref="IGorgonBufferInfo.StructureSize"/> greater than 0.
    /// </para>
    /// <para>
    /// The unordered access allows a shader to read/write any part of a <see cref="GorgonGraphicsResource"/> by multiple threads without memory contention. This is done through the use of 
    /// <a target="_blank" href="https://msdn.microsoft.com/en-us/library/windows/desktop/ff476334(v=vs.85).aspx">atomic functions</a>.
    /// </para>
    /// <para>
    /// These types of views are most useful for <see cref="GorgonComputeShader"/> shaders, but can also be used by a <see cref="GorgonPixelShader"/> by passing a list of these views in to a 
    /// <see cref="GorgonDrawCallCommon">draw call</see>.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// Unordered access views do not support multisampled <see cref="GorgonTexture2D"/>s.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsResource"/>
    /// <seealso cref="GorgonComputeShader"/>
    /// <seealso cref="GorgonPixelShader"/>
    /// <seealso cref="GorgonDrawCallCommon"/>
    public sealed class GorgonStructuredReadWriteView
        : GorgonBufferReadWriteViewCommon<GorgonBuffer>, IGorgonBufferInfo
    {
        #region Properties.
        /// <summary>
        /// Property to return the size of an element, in bytes.
        /// </summary>
        public override int ElementSize => Buffer?.StructureSize ?? 0;

        /// <summary>
        /// Property to return the type of view.
        /// </summary>
        public StructuredBufferReadWriteViewType ReadWriteViewType
        {
            get;
        }

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
        public int StructureSize => Buffer?.StructureSize ?? 0;

        /// <summary>
        /// Property to return whether to allow raw unordered views of the buffer.
        /// </summary>
        /// <remarks>
        /// This value is always <b>false</b> for this type of view.
        /// </remarks>
        bool IGorgonBufferInfo.AllowRawView => false;

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
        /// <summary>
        /// Function to initialize the unordered access view.
        /// </summary>
        private protected override D3D11.ResourceView OnCreateNativeView()
        {
            Graphics.Log.Print($"Creating D3D11 structured buffer unordered access view for {Buffer.Name}.", LoggingLevel.Verbose);

            var desc = new D3D11.UnorderedAccessViewDescription1
            {
                Dimension = D3D11.UnorderedAccessViewDimension.Buffer,
                Buffer =
                           {
                               FirstElement = StartElement,
                               ElementCount = ElementCount,
                               Flags = (D3D11.UnorderedAccessViewBufferFlags)ReadWriteViewType
                           },
                Format = Format.Unknown
            };

            Native = new D3D11.UnorderedAccessView1(Resource.Graphics.D3DDevice, Resource.D3DResource, desc)
            {
                DebugName = $"'{Buffer.Name}'_D3D11UnorderedAccessView1_Structured"
            };

            Graphics.Log.Print($"Unordered Access Structured Buffer View '{Buffer.Name}': {Buffer.ResourceType} -> Start: {StartElement}, Count: {ElementCount}, Element Size: {ElementSize}, Type: {ReadWriteViewType}",
                               LoggingLevel.Verbose);

            return Native;
        }

        /// <summary>
        /// Function to copy the structure count from this view into a buffer.
        /// </summary>
        /// <param name="buffer">The buffer that will receive the data.</param>
        /// <param name="offset">[Optional] The offset, in bytes, within the buffer attached to this view to start reading from.</param>
        /// <remarks>
        /// <para>
        /// When the structure unordered access view is set up with a <see cref="StructuredBufferReadWriteViewType.Append"/>, or <see cref="StructuredBufferReadWriteViewType.Counter"/>, the values updated by these flags are 
        /// not readily accessible from the CPU. To retrieve these values, this method must be called to retrieve the values. These values are copied into the <paramref name="buffer"/> provided to the 
        /// method so that applications can make use of data generated on the GPU. Note that this value will be written out as a 32 bit unsigned integer.
        /// </para>
        /// <para>
        /// If the unordered access view does not specify the appropriate values on the <see cref="ReadWriteViewType"/>, then this method will do nothing.
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

            buffer.Graphics.D3DDeviceContext.CopyStructureCount(buffer.Native, offset, Native);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonStructuredReadWriteView"/> class.
        /// </summary>
        /// <param name="buffer">The buffer to assign to the view.</param>
        /// <param name="elementStart">The first element in the buffer to view.</param>
        /// <param name="elementCount">The number of elements in the view.</param>
        /// <param name="totalElementCount">The total number of elements in the buffer.</param>
        /// <param name="uavType">Flags used to indicate the purpose of this view.</param>
        internal GorgonStructuredReadWriteView(GorgonBuffer buffer,
                                     StructuredBufferReadWriteViewType uavType,
                                     int elementStart,
                                     int elementCount,
                                     int totalElementCount)
            : base(buffer, elementStart, elementCount, totalElementCount) => ReadWriteViewType = uavType;
        #endregion
    }
}
