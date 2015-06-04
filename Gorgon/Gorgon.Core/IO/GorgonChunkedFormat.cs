﻿#region MIT.
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
    /// <remarks>This object will take data and turn it into chunks of data.  This is similar to the old IFF format in that 
    /// it allows Gorgon's file formats to be future proof.  That is, if a later version of Gorgon has support for a feature
    /// that does not exist in a previous version, then the older version will be able to read the file and skip the 
    /// unnecessary parts.</remarks>
    public abstract class GorgonChunkedFormat
        : IDisposable
    {
        #region Constants.
        /// <summary>
        /// The size of the temporary buffer for large data reads/writes.
        /// </summary>
        protected internal const int TempBufferSize = 65536;
        #endregion

        #region Variables.
        private bool _disposed;                                                 // Flag to indicate that the object was disposed.
		private ulong _currentChunk;											// Our current chunk.
		private long _chunkStart;												// The start of the current chunk.
		private long _chunkEnd;													// The end of the current chunk.
		private uint _chunkSize;												// Size of the chunk.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the temporary buffer for large reads/writes.
        /// </summary>
		protected byte[] TempBuffer
        {
            get;
            set;
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
        /// Function to convert a chunk name into a code.
        /// </summary>
        /// <param name="chunkName">The string containing the code to use.</param>
        /// <returns>The name encoded as an 8 byte unsigned long value.</returns>
        protected ulong GetChunkCode(string chunkName)
        {
            if (chunkName.Length != 8)
            {
                throw new ArgumentException(Resources.GOR_CHUNK_NAME_TOO_SMALL, "chunkName");
            }
            chunkName = chunkName.ToUpper();

            return ((ulong)chunkName[7] << 56) | ((ulong)chunkName[6] << 48) | ((ulong)chunkName[5] << 40) | ((ulong)chunkName[4] << 32)
                   | ((ulong)chunkName[3] << 24) | ((ulong)chunkName[2] << 16) | ((ulong)chunkName[1] << 8) | chunkName[0];
        }

	    /// <summary>
		/// Function to begin reading/writing the chunk
		/// </summary>
		/// <param name="chunkName">The name of the chunk.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="chunkName"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the chunkName parameter is empty.
		/// <para>-or-</para>
		/// <para>Thrown when the chunkName parameter is less than 8 characters.</para>
		/// </exception>
		/// <exception cref="System.IO.InvalidDataException">Thrown when reading a chunk ID and it does not match the requested chunk name.</exception>
		/// <remarks>
		/// Use this to begin a chunk in the stream.  This method must be called before using any of the read/write methods.
		/// <para>The <paramref name="chunkName" /> parameter must be 8 characters in length, otherwise an exception will be thrown.  If the name is longer than 8 characters,
		/// then only the first 8 characters will be used.</para>
		/// </remarks>
		public void Begin(string chunkName)
		{
			if (chunkName == null)
			{
				throw new ArgumentNullException("chunkName");
			}

            chunkName = chunkName.Trim();

			if (chunkName.Length < 8)
			{
				throw new ArgumentException(Resources.GOR_CHUNK_NAME_TOO_SMALL, "chunkName");
			}

            // Truncate to 8 characters.
            if (chunkName.Length > 8)
            {
                chunkName = chunkName.Substring(0, 8);
            }

            ulong newChunkID = GetChunkCode(chunkName);

            // Return if we're using the same chunk.
            if (newChunkID == _currentChunk)
            {
                return;
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
		}

		/// <summary>
		/// Function to end the chunk stream.
		/// </summary>
		/// <exception cref="System.IO.IOException">Thrown when the stream cannot seek and reading has prematurely ended before getting the end of the chunk.</exception>
		public void End()
		{
			// We don't have a chunk being processed right now, so leave.
			if (_currentChunk == 0)
			{
				return;
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
				long skipAmount = _chunkEnd - (_chunkStart + _chunkSize);

				// Skip ahead.
				if (skipAmount > 0)
				{
					Reader.BaseStream.Seek(skipAmount, SeekOrigin.Current);
				}
			}

			// Reset the chunk status.
			_currentChunk = 0;
			_chunkStart = 0;
			_chunkSize = 0;
			_chunkEnd = 0;
		}

        /// <summary>
        /// Function to skip the specified number of bytes in the stream.
        /// </summary>
        /// <param name="byteCount">Number of bytes in the stream to skip.</param>
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
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
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
