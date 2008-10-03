#region MIT.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Wednesday, November 30, 2005 2:13:13 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Internal
{
    /// <summary>
    /// Interface defining a cache of data.
    /// </summary>
    /// <remarks>
    /// This is a common interface for objects that implement data caching, such as vertex cache or index cache.  The implementor object will use this to keep a copy of object data in memory for update before sending to a destination buffer (e.g. a <seealso cref="GorgonLibrary.Internal.VertexBuffer">VertexBuffer</seealso>).
    /// <para>
    /// While it is possible to read data back from a <see cref="GorgonLibrary.Internal.VertexBuffer">vertex</see>/<see cref="GorgonLibrary.Internal.IndexBuffer">index buffer</see>, it is complex and not a recommended procedure for performance reasons.
    /// </para>
    /// 	<para>
    /// Objects that implement this interface should have a buffer of data that will receive the contents of the cache.  This keeps the cache atomic ensuring that we either get the data to its destination or not at all.
    /// </para>
    /// 	<para>
    /// The usefulness of this type of object is apparent when one needs to update a batch of vertices (e.g. software skinning) and send those to a write-only buffer.  This will avoid the overhead of having to open the <see cref="GorgonLibrary.Internal.VertexBuffer">vertex buffer</see> for reading (if it's even allowed to do so), read the data and modify it, then write the data back to the buffer.
    /// </para>
    /// 	<para>
    /// Note that there is a performance penalty in that your data size will be larger since it has to hold a seperate copy of the data.
    /// </para>
    /// </remarks>
    public interface IDataCache<T>
	{
		#region Properties.
		/// <summary>
        /// Property to set or return the number of items within the cache.
        /// </summary>
        /// <remarks>
        /// Use this to set the size of the data cache (size is the number of elements, e.g. vertices, and NOT bytes).
        /// <para>For a vertex cache, which will have multiple streams, this will represent the number of vertices, and not the exact size of the stream.</para>
        /// </remarks>
        /// <value>An integer containing a total count of the items within the cache.</value>
        int Count
        {
            get;
            set;
        }

        /// <summary>
        /// Read only property to determine if the underlying buffers need to be updated.
        /// </summary>
        /// <value>Returns TRUE if an update is necessary, FALSE if not.</value>
        bool NeedsUpdate
        {
            get;
		}

		/// <summary>
		/// Property to set or return the usage of the buffer.
		/// </summary>
		BufferUsages BufferUsage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the array of cached data.
		/// </summary>
		T[] CachedData
		{
			get;
		}

		/// <summary>
		/// Property to return the amount of data written to the buffer already.
		/// </summary>
		int DataWritten
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the start of the data in the buffer.
		/// </summary>
		int DataOffset
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
        /// Function to update the necessary buffers with the current information held in the cache.
        /// </summary>
        /// <remarks>This function is used to send the data to the destination buffer(s).</remarks>
        /// <param name="isRestoring">TRUE if this is being called due to a device restoration, FALSE if not.</param>
        void Update(bool isRestoring);

        /// <summary>
        /// Function to release the underlying data buffers.
        /// </summary>
        /// <remarks>
        /// Use this function to clear out any of the destination data buffers.
        /// <para>
        /// Use this with care as it will invalidate any data buffers that are being referenced elsewhere.
        /// </para>
        /// </remarks>
        void Release();

		/// <summary>
		/// Function to write data to the cache.
		/// </summary>
		/// <param name="sourceStart">Start of the data in the source array of data.</param>
		/// <param name="destinationStart">Start in the destination buffer to write at.</param>
		/// <param name="length">Length of data to write.</param>
		/// <param name="data">Data to write.</param>
		void WriteData(int sourceStart, int destinationStart, int length, T[] data);
		#endregion
	}
}
