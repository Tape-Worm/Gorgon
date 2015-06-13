#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, January 21, 2013 9:19:55 AM
// 
#endregion

using System;
using System.IO;
using System.Text;
using Gorgon.Core;
using Gorgon.Core.Properties;

namespace Gorgon.IO
{
	/// <summary>
	/// The access mode for the file chunking object.
	/// </summary>
	public enum ChunkAccessMode
	{
		/// <summary>
		/// Chunk object is in read only mode.
		/// </summary>
		Read = 0,
		/// <summary>
		/// Chunk object is in write only mode.
		/// </summary>
		Write = 1
	}

    /// <summary>
    /// Reads/writes Gorgon chunked formatted data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Gorgon uses the chunked file format for its own file serializing/deserializing of its objects that support persistence.
    /// </para>
    /// <para>
    /// This object is used to serialize or deserialize object data into a chunked file format. Essentially this takes the binary layout of the file, and makes it easier to process by putting identifiers 
    /// within the format. It is a base class to the <see cref="GorgonChunkWriter"/> and <see cref="GorgonChunkReader"/> objects which perform the actual serialization/deserialization.
	/// </para>
	/// <para>
	/// Since chunk files use identifiers to identify parts of the data, the format for a given piece of data should be fairly simple to parse as the identifiers are merely constant 64-bit <see cref="long"/> 
	/// values. The identifiers are built from an identifier string passed into the <see cref="Begin"/> method. This string must have 8 characters, 1 for each byte in a 64 bit <see cref="long"/> value.
	/// </para>
	/// <para>
	/// When writing out the binary data, the user should begin a new chunk by calling the <see cref="Begin"/> method, and passing in the 8 character string identifying that chunk. Then, they should write 
	/// the data as normal using the write methods. Then, a call to <see cref="End"/> should be made to close that chunk. Reading the binary data is pretty much the same thing:  call <see cref="Begin"/>, 
	/// read the data, call <see cref="End"/>.
	/// </para>
	/// <para>
	/// Each chunk will store a 32 bit integer representing the size of the chunk immediately after the chunk ID. This is used so the caller can <see cref="End"/> the reading of the chunk early, and 
	/// the <see cref="GorgonChunkReader"/> can skip to the next chunk. A typical case where this is needed is when an object needs to deserialize less data than actually exists in the chunk block, this 
	/// is handy for  forward compatibility. For example: Object Foo originally had 12 fields, and could read/write those 12 fields. Version 2 of Object Foo has 16 fields. The 1st version of Foo, as long as 
	/// the layout remains the same, can read those first 12 fields and then call <see cref="End"/> to move on to the next chunk block, skipping the extra 4 fields. Thus making Foo v1 compatible with Foo v2.
	/// </para>
	/// </remarks>
	/// <seealso cref="GorgonChunkReader"/>
	/// <seealso cref="GorgonChunkWriter"/>
    public abstract class GorgonChunkedFormat
        : IDisposable
    {
        #region Variables.
		// Flag to indicate that the object was disposed.
        private bool _disposed;
		// Our current chunk.
		private ulong _currentChunk;
		// The start of the current chunk.
		private long _chunkStart;
		// The end of the current chunk.
		private long _chunkEnd;
		// Size of the chunk.
		private uint _chunkSize;		
		// Byte representation of a chunk ID string.
		private	readonly byte[] _chunkBytes = new byte[8];
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the size of the current chunk when reading.
		/// </summary>
		/// <remarks>
		/// This value is not set until a call to <see cref="Begin"/> has been made and only applies to the reader. If accessed from a writer object, this property will return -1.
		/// </remarks>
	    protected long CurrentChunkSize
	    {
		    get
		    {
			    if (ChunkAccessMode == ChunkAccessMode.Write)
			    {
				    return -1;
			    }

			    return _chunkSize;
		    }
	    }

		/// <summary>
		/// Property to return the start of the chunk.
		/// </summary>
		/// <remarks>
		/// This is the start of the chunk <i>after</i> the chunk size value.
		/// </remarks>
	    protected long ChunkStart
	    {
		    get
		    {
			    return _chunkStart;
		    }
	    }

		/// <summary>
		/// Property to return the writer for our stream.
		/// </summary>
		protected GorgonBinaryWriter Writer
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the reade for our stream.
		/// </summary>
		protected GorgonBinaryReader Reader
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the current mode for the chunking object.
		/// </summary>
		public ChunkAccessMode ChunkAccessMode
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the underlying stream for the chunking object.
		/// </summary>
	    public Stream BaseStream
	    {
		    get
		    {
				return ChunkAccessMode == ChunkAccessMode.Write ? Writer.BaseStream : Reader.BaseStream;
		    }
	    }
        #endregion

        #region Methods.
		/// <summary>
		/// Function to validate the access mode.
		/// </summary>
		/// <param name="isWrite"><b>true</b> if writing, <b>false</b> if not.</param>
		protected void ValidateAccess(bool isWrite)
		{
			if ((isWrite) && (Writer == null))
			{
				throw new IOException(Resources.GOR_STREAM_IS_READONLY);
			}

			if ((!isWrite) && (Reader == null))
			{
                throw new IOException(Resources.GOR_STREAM_IS_READONLY);
			}
		}

        /// <summary>
        /// Function to convert a chunk name <see cref="string"/> into a <see cref="ulong"/> chunk code.
        /// </summary>
        /// <param name="chunkName">The string containing the code to use.</param>
        /// <returns>The name encoded as an 8 byte unsigned long value.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="chunkName"/> parameter is not exactly 8 characters in length.</exception>
        protected ulong GetChunkCode(string chunkName)
        {
            if (chunkName.Length != 8)
            {
                throw new ArgumentException(Resources.GOR_ERR_CHUNK_NAME_SIZE_MISMATCH, "chunkName");
            }
            chunkName = chunkName.ToUpper();

	        Encoding.ASCII.GetBytes(chunkName, 0, 8, _chunkBytes, 0);

            return ((ulong)_chunkBytes[7] << 56) | ((ulong)_chunkBytes[6] << 48) | ((ulong)_chunkBytes[5] << 40) | ((ulong)_chunkBytes[4] << 32)
                   | ((ulong)_chunkBytes[3] << 24) | ((ulong)_chunkBytes[2] << 16) | ((ulong)_chunkBytes[1] << 8) | _chunkBytes[0];
        }

	    /// <summary>
		/// Function to begin reading/writing the chunk
		/// </summary>
		/// <param name="chunkName">The name of the chunk.</param>
		/// <returns>The size of the chunk, in bytes, when reading the chunk data. When in write mode, this value returns 0.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="chunkName"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the chunkName parameter is empty.
		/// <para>-or-</para>
		/// <para>Thrown when the chunkName parameter length is not exactly 8 characters.</para>
		/// </exception>
		/// <exception cref="System.IO.InvalidDataException">Thrown when reading a chunk ID and it does not match the requested chunk name.</exception>
		/// <remarks>
		/// <para>
		/// Use this to begin a chunk in the stream.  This method must be called before using any of the read/write methods.
		/// </para>
		/// <para>
		/// When reading a file, this method will return the number of bytes that the chunk occupies. This can be used to determine if a chunk is compatible, or even valid. However, 
		/// when writing a file, this value returns 0 because there's no data written yet, and as such, a count is impossible to determine.
		/// </para>
		/// <para>
		/// The <paramref name="chunkName" /> parameter must be 8 characters in length, otherwise an exception will be thrown.  If the name is longer than 8 characters,
		/// then only the first 8 characters will be used.
		/// </para>
		/// <para>
		/// Always pair a call to <c>Begin</c> with a call to <see cref="End"/>, or else the file may get corrupted. If another call to <c>Begin</c> is made before <see cref="End"/> is called, 
		/// then the previous chunk will be closed automatically so the new one can begin.
		/// </para>
		/// </remarks>
		/// <seealso cref="End"/>
		public uint Begin(string chunkName)
		{
			if (chunkName == null)
			{
				throw new ArgumentNullException("chunkName");
			}

			if (chunkName.Length != 8)
			{
				throw new ArgumentException(Resources.GOR_ERR_CHUNK_NAME_SIZE_MISMATCH, "chunkName");
			}

            ulong newChunkID = GetChunkCode(chunkName);

            // Return if we're using the same chunk.
            if (newChunkID == _currentChunk)
            {
                return _chunkSize;
            }

			if (_currentChunk != 0) 
			{
				End();
			}
            
			_currentChunk = newChunkID;

			if (ChunkAccessMode == ChunkAccessMode.Write)
			{
				// Write out the chunk ID.
				Writer.Write(_currentChunk);

				// Write out the size value now, we'll come back to it later.
				Writer.Write((uint)0);

				// Record the current position in the stream.
				_chunkStart = Writer.BaseStream.Position;
			}
			else
			{
				// Get the current chunk.
				ulong chunkHeader = Reader.ReadUInt64();

				if (chunkHeader != _currentChunk)
				{
				    throw new InvalidDataException(string.Format(Resources.GOR_CHUNK_INVALID, _currentChunk.FormatHex(),
				                                                 chunkHeader.FormatHex()));
				}

				// Read the size of this chunk.
				_chunkSize = Reader.ReadUInt32();
								
				_chunkStart = Reader.BaseStream.Position;
				_chunkEnd += _chunkSize + _chunkStart;
			}

		    return _chunkSize;
		}

		/// <summary>
		/// Function to end the chunk stream.
		/// </summary>
		/// <returns>The size of the chunk, in bytes. Or 0 if no chunk has been started with <see cref="Begin"/>.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream cannot seek and reading has prematurely ended before getting the end of the chunk.</exception>
		/// <remarks>
		/// <para>
		/// This marks the end of a chunk in the stream. It writes out the total size of the chunk when writing a file, and when reading it uses the chunk size to skip 
		/// any unnecessary data if the object hasn't read the entire chunk.
		/// </para>
		/// <para>
		/// This method should always be paired with a call to <see cref="Begin"/>, otherwise nothing will happen.
		/// </para>
		/// <para>
		/// Like the <see cref="Begin"/> method, this method returns the number of bytes occupied by a chunk. This value may be used to validate a chunk.
		/// </para>
		/// </remarks>
		/// <seealso cref="Begin"/>
		public uint End()
		{
			// We don't have a chunk being processed right now, so leave.
			if (_currentChunk == 0)
			{
				return 0;
			}

			// Record the end of the chunk.			

			if (ChunkAccessMode == ChunkAccessMode.Write)
			{
				_chunkEnd = Writer.BaseStream.Position;
				_chunkSize = (uint)(_chunkEnd - _chunkStart);
				Writer.BaseStream.Position = _chunkStart - sizeof(uint);
				Writer.Write(_chunkSize);
				Writer.BaseStream.Position = _chunkEnd;
			}
			else
			{
				_chunkEnd = Reader.BaseStream.Position;
				// If we end the read prematurely, then just skip to the next chunk.
				long skipAmount = (_chunkStart + _chunkSize) - _chunkEnd;

				// Skip ahead.
				if (skipAmount > 0)
				{
					Reader.BaseStream.Seek(skipAmount, SeekOrigin.Current);
				}
			}

			// Record the chunk size so we can spit it out when we're done.
			uint chunkSize = _chunkSize;

			// Reset the chunk status.
			_currentChunk = 0;
			_chunkStart = 0;
			_chunkSize = 0;
			_chunkEnd = 0;

			return chunkSize;
		}

        /// <summary>
        /// Function to skip the specified number of bytes in the stream.
        /// </summary>
        /// <param name="byteCount">Number of bytes in the stream to skip.</param>
        /// <remarks>
        /// This method requires that the underlying <see cref="Stream"/> object have its <see cref="Stream.CanSeek"/> property return <b>true</b>, otherwise an exception will occur.
        /// </remarks>
        public void SkipBytes(long byteCount)
        {
            if (ChunkAccessMode == ChunkAccessMode.Read)
            {
                Reader.BaseStream.Seek(byteCount, SeekOrigin.Current);
            }
            else
            {
                Writer.BaseStream.Seek(byteCount, SeekOrigin.Current);
            }
        }
		#endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonChunkedFormat" /> class.
        /// </summary>
		/// <param name="stream">The stream to use to output the chunked data.</param>
		/// <param name="accessMode">Stream access mode for the chunk object.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="accessMode"/> parameter is set to read, but the stream cannot be read.
		/// <para>-or-</para>
		/// <para>Thrown when the accessMode parameter is set to write, but the stream cannot be written.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the stream can't perform seek operations.</para>
		/// </exception>
        protected GorgonChunkedFormat(Stream stream, ChunkAccessMode accessMode)
        {		
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			ChunkAccessMode = accessMode;

            if (!stream.CanSeek)
            {
                throw new ArgumentException(Resources.GOR_STREAM_NOT_SEEKABLE, "stream");
            }

			if (accessMode == ChunkAccessMode.Write)
			{
				if (!stream.CanWrite)
				{
					throw new ArgumentException(Resources.GOR_STREAM_IS_READONLY, "accessMode");
				}

				Writer = new GorgonBinaryWriter(stream, true);
			}
			else
			{
				if (!stream.CanRead)
				{
					throw new ArgumentException(Resources.GOR_STREAM_IS_WRITEONLY, "accessMode");
				}

				Reader = new GorgonBinaryReader(stream, true);
			}
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
	        if (_disposed)
	        {
		        return;
	        }

	        if (disposing)
	        {
		        if (Reader != null)
		        {
			        Reader.Dispose();						
		        }

		        if (Writer != null)
		        {
			        Writer.Dispose();
		        }                    
	        }

	        Writer = null;
	        Reader = null;
	        _disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
