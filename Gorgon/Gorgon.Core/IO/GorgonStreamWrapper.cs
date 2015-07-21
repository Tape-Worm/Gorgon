#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Sunday, June 14, 2015 11:15:04 AM
// 
#endregion

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core.Properties;

namespace Gorgon.IO
{
    /// <summary>
    /// Wraps a parent stream object into a stream object with a smaller boundary.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sometimes it's necessary take a <see cref="Stream"/> and divide it into sections to ensure there are no overruns. This object will take an existing stream object and allows the user to 
    /// set a start position within that stream, and give a range of readable bytes to break the parent stream into a smaller stream. This gives the protection that a stream provides against 
    /// buffer overruns all while working within the confines of a smaller part of the parent stream.
	/// </para>
	/// <para>
	/// When reading or writing, the parent stream will have its position manipulated by this stream to locate the start of this stream plus the current position of this stream. Once these operations 
	/// are complete, the parent stream <see cref="P:System.IO.Stream.Position"/> will be set back to the original position it was in prior to the read/write operation. This ensures that when this 
	/// object is finished its work, the original parent position will remain unaffected by this stream.
	/// </para>
	/// <note>
	/// It is not necessary to call the <see cref="M:System.IO.Stream.Dispose"/> method or the <see cref="M:System.IO.Stream.Close"/> method on this object since there is nothing to close 
	/// and ownership of the parent stream resides with the creator of that stream (i.e. this type does <i>not</i> take ownership of a parent stream). In fact, closing or disposing of this object 
	/// does nothing.
	/// </note>
	/// <note type="caution">
	/// This object is <i>not</i> thread safe. If multiple wrappers are pointing to the same parent stream, and multiple threads use these wrappers, the read/write cursor will be desynchronized. 
	/// Because of this limitation, all the asynchronous I/O operations will throw an exception.
	/// </note> 
    /// </remarks>
    public class GorgonStreamWrapper
        : Stream
    {
        #region Variables.
	    // Position within the parent stream, offset by the number of bytes specified in the constructor.
        private readonly long _parentOffset;
		// The current position within this stream, relative to the parent position.
        private long _currentPosition;
		// The length of this child stream.
		private long _streamLength;
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the parent of this stream.
		/// </summary>
		public Stream ParentStream
		{
			get;
		}

		/// <inheritdoc/>
		public override bool CanTimeout => ParentStream.CanTimeout;

	    /// <inheritdoc/>
		public override bool CanRead => ParentStream.CanRead;

	    /// <inheritdoc/>
		public override bool CanSeek => true;

	    /// <inheritdoc/>
		public override bool CanWrite => ParentStream.CanWrite;

	    /// <inheritdoc/>
		public override long Length => _streamLength;

	    /// <inheritdoc/>
		public override long Position
        {
            get
            {
                return _currentPosition;
            }
            set
            {
                _currentPosition = value;

	            if (_currentPosition < 0)
	            {
		            _currentPosition = 0;
	            }

	            if (_currentPosition > _streamLength)
	            {
		            _currentPosition = _streamLength;
	            }
            }
        }
        #endregion

        #region Methods.		
		/// <inheritdoc/>
		/// <exception cref="NotSupportedException">This stream does not support asynchronous I/O.</exception>
	    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	    {
		    throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);
	    }

		/// <inheritdoc/>
		/// <exception cref="NotSupportedException">This stream does not support asynchronous I/O.</exception>
		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	    {
			throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);
	    }

		/// <inheritdoc/>
		/// <exception cref="NotSupportedException">This stream does not support asynchronous I/O.</exception>
		public override int EndRead(IAsyncResult asyncResult)
	    {
			throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);
	    }

		/// <inheritdoc/>
		/// <exception cref="NotSupportedException">This stream does not support asynchronous I/O.</exception>
		public override void EndWrite(IAsyncResult asyncResult)
	    {
			throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);
		}

		/// <inheritdoc/>
		/// <exception cref="NotSupportedException">This stream does not support asynchronous I/O.</exception>
		public override Task FlushAsync(CancellationToken cancellationToken)
	    {
		    throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);
	    }

		/// <inheritdoc/>
		/// <exception cref="NotSupportedException">This stream does not support asynchronous I/O.</exception>
		public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
	    {
			throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);
	    }

		/// <inheritdoc/>
		/// <exception cref="NotSupportedException">This stream does not support asynchronous I/O.</exception>
		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	    {
			throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);
	    }

		/// <inheritdoc/>
		/// <exception cref="NotSupportedException">This stream does not support asynchronous I/O.</exception>
		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
	    {
			throw new NotSupportedException(Resources.GOR_ERR_STREAM_DOES_NOT_SUPPORT_ASYNC);
	    }

		/// <inheritdoc/>
		public override void WriteByte(byte value)
	    {
			if (!ParentStream.CanWrite)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
			}

			long lastPosition = ParentStream.Position;

			try
			{
				ParentStream.Position = _parentOffset + _currentPosition;

				ParentStream.WriteByte(value);

				++_currentPosition;

				// Expand the size of the stream if larger than what we currently have.
				if (_currentPosition > _streamLength)
				{
					++_streamLength;
				}
			}
			finally
			{
				ParentStream.Position = lastPosition;
			}
	    }

		/// <inheritdoc/>
		public override void Flush()
        {
            ParentStream.Flush();
        }

		/// <inheritdoc/>
	    public override int ReadByte()
	    {
			if (!ParentStream.CanRead)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);	
			}

			// Don't advance any further than our boundaries.
			if ((_currentPosition + 1) >= _streamLength)
			{
				return -1;
			}

			long lastPosition = ParentStream.Position;

			try
			{
				ParentStream.Position = _currentPosition + _parentOffset;

				int result = ParentStream.ReadByte();

				_currentPosition++;

				return result;
			}
			finally
			{
				ParentStream.Position = lastPosition;
			}
	    }

		/// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
		    if (buffer == null)
		    {
			    throw new ArgumentNullException(nameof(buffer));
		    }

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if ((offset + count) > buffer.Length)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_OFFSET_AND_SIZE_ARE_LARGER_THAN_ARRAY, offset, count, buffer.Length));
			}

			if (!ParentStream.CanRead)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
			}

			int actualCount = count;
			
			if (actualCount > _streamLength - _currentPosition)
			{
				actualCount = (int)(_streamLength - _currentPosition);
			}

			if (actualCount <= 0)
			{
				return 0;
			}

			long lastPosition = ParentStream.Position;

	        try
	        {
		        ParentStream.Position = _currentPosition + _parentOffset;
				
		        int result = ParentStream.Read(buffer, offset, actualCount);

		        _currentPosition += result;

		        return result;
	        }
	        finally
	        {
				ParentStream.Position = lastPosition;
	        }
        }

		/// <summary>
		/// When overridden in a derived class, sets the position within the current stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
		/// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
		/// <returns>The new position within the current stream.</returns>
		public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    _currentPosition = offset;
                    break;
                case SeekOrigin.End:
					// We're adding the offset because offset should be a negative value.
                    _currentPosition = _streamLength + offset;
                    break;
                case SeekOrigin.Current:
                    _currentPosition += offset;
                    break;
            }

	        if (_currentPosition < 0)
	        {
		        _currentPosition = 0;
	        }

	        if (_currentPosition > _streamLength)
	        {
		        _currentPosition = _streamLength;
	        }

            return _currentPosition;
        }

		/// <inheritdoc/>
		public override void SetLength(long value)
        {
			if (!CanWrite)
			{
				throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
			}

	        if (value < 0)
	        {
		        value = 0;
	        }

	        if (value > (ParentStream.Length - ParentStream.Position))
	        {
				value = ParentStream.Length - ParentStream.Position;
	        }

	        _streamLength = value;
        }

		/// <inheritdoc/>
		public override void Write(byte[] buffer, int offset, int count)
        {
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if ((offset + count) > buffer.Length)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_OFFSET_AND_SIZE_ARE_LARGER_THAN_ARRAY, offset, count, buffer.Length));
			}

			if (!ParentStream.CanWrite)
	        {
		        throw new NotSupportedException(Resources.GOR_ERR_STREAM_IS_READONLY);
	        }

			long lastPosition = ParentStream.Position;

	        try
	        {
		        ParentStream.Position = _parentOffset + _currentPosition;

		        ParentStream.Write(buffer, offset, count);
		        _currentPosition += count;

		        if (_currentPosition > _streamLength)
		        {
			        _streamLength += count;
		        }
	        }
	        finally
	        {
				ParentStream.Position = lastPosition;
	        }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonStreamWrapper"/> class.
        /// </summary>
        /// <param name="parentStream">The parent of this stream.</param>
        /// <param name="streamStart">The position in the parent stream to start at, in bytes.</param>
        /// <param name="streamSize">The number of bytes to partition from the parent stream.</param>
        /// <remarks>
        /// If writing to the stream, the <paramref name="streamSize"/> is ignored.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="parentStream"/> is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="streamStart"/> or the <paramref name="streamSize"/> parameters are less than 0</exception>
        /// <exception cref="ArgumentException">Thrown when the <see cref="Stream.CanSeek"/> property on the parent stream is <b>false</b>.</exception>
        public GorgonStreamWrapper(Stream parentStream, long streamStart, long streamSize)
        {
	        if (parentStream == null)
	        {
		        throw new ArgumentNullException(nameof(parentStream));
	        }

	        if ((streamStart < 0) || (streamSize < 0))
	        {
		        throw new ArgumentOutOfRangeException(nameof(streamStart), Resources.GOR_ERR_STREAM_POS_OUT_OF_RANGE);
	        }

	        if (!parentStream.CanSeek)
	        {
		        throw new ArgumentException(Resources.GOR_ERR_STREAM_PARENT_NEEDS_SEEK, nameof(parentStream));
	        }

			_streamLength = streamSize;
            ParentStream = parentStream;
            _parentOffset = ParentStream.Position + streamStart;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonStreamWrapper"/> class.
		/// </summary>
		/// <param name="parentStream">The parent of this stream.</param>
		/// <param name="streamStart">The position in the parent stream to start at, in bytes.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="parentStream"/> is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="streamStart"/> parameter is less than 0.
		/// <para>-or-</para>
		/// <para>The <paramref name="streamStart"/> is larger than or equal to the size of the <paramref name="parentStream"/>.</para>
		/// </exception>
		/// <exception cref="ArgumentException">Thrown when the <see cref="Stream.CanSeek"/> property on the parent stream is <b>false</b>.</exception>
		public GorgonStreamWrapper(Stream parentStream, long streamStart)
			: this(parentStream, streamStart, parentStream?.Length - streamStart ?? 0)
	    {
	    }

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonStreamWrapper"/> class.
		/// </summary>
		/// <param name="parentStream">The parent of this stream.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="parentStream"/> is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the <see cref="Stream.CanSeek"/> property on the parent stream is <b>false</b>.</exception>
		public GorgonStreamWrapper(Stream parentStream)
			: this(parentStream, 0, parentStream?.Length ?? 0)
	    {
	    }
        #endregion
    }
}
