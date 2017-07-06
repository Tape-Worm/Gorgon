#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 9, 2016 3:54:15 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// The type of data to be stored in the buffer.
	/// </summary>
	public enum BufferType
	{
		/// <summary>
		/// A generic raw buffer filled with byte data.
		/// </summary>
		Generic = 0,
		/// <summary>
		/// A constant buffer used to send data to a shader.
		/// </summary>
		Constant = 1,
		/// <summary>
		/// A vertex buffer used to hold vertex information.
		/// </summary>
		Vertex = 2,
		/// <summary>
		/// An index buffer used to hold index information.
		/// </summary>
		Index = 3,
		/// <summary>
		/// A structured buffer used to hold structured data.
		/// </summary>
		Structured = 4,
        /// <summary>
        /// A raw buffer used to hold raw byte data.
        /// </summary>
        Raw = 5,
        /// <summary>
        /// An indirect argument buffer.
        /// </summary>
        IndirectArgument = 6
	}

	/// <summary>
	/// A base class for buffers.
	/// </summary>
	public abstract class GorgonBufferBase
		: GorgonResource
	{
		#region Properties.
		/// <summary>
		/// Property to return the log used for debugging.
		/// </summary>
		protected IGorgonLog Log
		{
			get;
		}

		/// <summary>
		/// Property to return the D3D 11 buffer.
		/// </summary>
		protected internal D3D11.Buffer D3DBuffer
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the type of data in the resource.
		/// </summary>
		public override ResourceType ResourceType => ResourceType.Buffer;

		/// <summary>
		/// Property to return the type of buffer.
		/// </summary>
		public abstract BufferType BufferType
		{
			get;
		}
        #endregion

        #region Methods.
        /// <summary>
        /// Function to copy the contents of this buffer into a destination buffer.
        /// </summary>
        /// <param name="buffer">The destination buffer that will receive the data.</param>
        /// <param name="sourceOffset">[Optional] Starting byte index to start copying from.</param>
        /// <param name="byteCount">[Optional] The number of bytes to copy.</param>
        /// <param name="destOffset">[Optional] The offset within the destination buffer.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sourceOffset"/> plus the <paramref name="byteCount"/> is less than 0 or larger than the size of this buffer.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="destOffset"/> plus the <paramref name="byteCount"/> is less than 0 or larger than the size of the destination <paramref name="buffer"/>.</para>
        /// </exception>
        /// <exception cref="GorgonException">Thrown when <paramref name="buffer"/> has a resource usage of <c>Immutable</c>.</exception>
        /// <remarks>
        /// <para>
        /// Use this method to copy the contents of this buffer into another.
        /// </para> 
        /// <para>
        /// The source and destination buffer offsets must fit within their range of their allocated space, as must the <paramref name="byteCount"/>. Otherwise, an exception will be thrown. Also, the 
        /// destination buffer must not be <c>Immutable</c>.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// For performance reasons, this method will only throw exceptions when Gorgon is compiled as <b>DEBUG</b>.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void CopyTo(GorgonBufferBase buffer, int sourceOffset = 0, int byteCount = 0, int destOffset = 0)
        {
            buffer.ValidateObject(nameof(buffer));

            sourceOffset = sourceOffset.Max(0);
            destOffset = destOffset.Max(0);

            if (byteCount < 1)
            {
                byteCount = SizeInBytes.Min(buffer.SizeInBytes);
            }

            int sourceByteIndex = sourceOffset + byteCount;
            int destByteIndex = destOffset + byteCount;

            sourceByteIndex.ValidateRange(nameof(sourceOffset), 0, buffer.SizeInBytes);
            destByteIndex.ValidateRange(nameof(destOffset), 0, buffer.SizeInBytes);

#if DEBUG
            if (buffer.D3DBuffer.Description.Usage == D3D11.ResourceUsage.Immutable)
            {
                throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_ERR_BUFFER_IS_IMMUTABLE);
            }
#endif
            Graphics.D3DDeviceContext.CopySubresourceRegion(buffer.D3DResource,
                                                            0,
                                                            new D3D11.ResourceRegion
                                                            {
                                                                Top = 0,
                                                                Bottom = 1,
                                                                Left = sourceOffset,
                                                                Right = sourceByteIndex,
                                                                Front = 0,
                                                                Back = 1
                                                            },
                                                            D3DResource,
                                                            0,
                                                            destOffset);
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBufferBase" /> class.
        /// </summary>
        /// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
        /// <param name="name">Name of this buffer.</param>
        /// <param name="log">[Optional] The log interface used for debug logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="name"/> parameters are <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        protected GorgonBufferBase(GorgonGraphics graphics, string name, IGorgonLog log)
			: base(graphics, name)
		{
			Log = log ?? GorgonLogDummy.DefaultInstance;
		}
		#endregion
	}
}
