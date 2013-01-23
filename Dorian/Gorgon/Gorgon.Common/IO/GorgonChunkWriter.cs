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
// Created: Wednesday, January 23, 2013 9:45:05 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GorgonLibrary.IO
{
    /// <summary>
    /// A binary chunked file writer.
    /// </summary>
    /// <remarks>This object will write out data as binary data into a chunked format.  The chunked format consists of 
    /// the size of the chunk, a chunk header, and the relevant chunked data.
    /// <para>Chunked files are useful in reading files from a newer version of the file by and older version of the reader. 
    /// Basically the older reader would read the chunk and its data, discarding chunks that it doesn't recognize.  It 
    /// also has the benefit of making the file format easier to understand.
    /// </para>
    /// <para>The writer object must have a stream that can seek, and that stream must be writable.  An exception will be thrown 
    /// if the previous conditions are not met.</para>
    /// </remarks>
    public class GorgonChunkWriter
    {
        #region Variables.
        private Stream _stream = null;                  // Our stream.
        private long _currentChunkPosition = -1;        // The current chunk position in the stream.
        private string _currentChunk = string.Empty;    // Current chunk name.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the current chunk size.
        /// </summary>
        public int CurrentChunkSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the current position of the stream.
        /// </summary>
        public long Position
        {
            get
            {
                return _stream.Position;
            }
        }

        /// <summary>
        /// Property to return the current length of the stream.
        /// </summary>
        public long Length
        {
            get
            {
                return _stream.Length;
            }
        }

        /// <summary>
        /// Property to set or return the current chunk name.
        /// </summary>
        public string CurrentChunk
        {
            get
            {
                return _currentChunk;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _currentChunk = string.Empty;
                    return;
                }

                _currentChunkPosition = _stream.Position;
                _currentChunk = value;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to validate if the current chunk has been set or not.
        /// </summary>
        private void ValidateChunk()
        {
            if (string.IsNullOrWhiteSpace(_currentChunk))
            {
                throw new InvalidDataException("The current chunk has not been set.  Please call SetChunk before writing data.");
            }
        }

        /// <summary>
        /// Function to write a single byte to the stream.
        /// </summary>
        /// <param name="value">Byte value to write.</param>
        public void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

        /// <summary>
        /// Function to write an array of bytes to the stream.
        /// </summary>
        /// <param name="value">Array of bytes to write.</param>
        /// <param name="startIndex">Starting index in the array to write.</param>
        /// <param name="length">Number of bytes in the array to write.</param>
        /// <exception cref="System.IO.InvalidDataException">Thrown if the <see cref="P:GorgonLibrary.IO.GorgonChunkWriter.CurrentChunk">CurrentChunk</see> property is not set.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="startIndex"/> parameter is less than 0.
        /// <para>-or-</para>
        /// <para>Thrown when the startingindex parameter is larger than or equal to the length of the <paramref name="value"/> parameter.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="length"/> paramer is less than 0.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the length parameter is larger than the length of the value parameter.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the sum of the start and length parameters is larger than the length of the value parameter</para>
        /// </exception>
        public void WriteBytes(byte[] value, int startIndex, int length)
        {
            ValidateChunk();

            if ((value == null) || (value.Length == 0))
            {
                return;
            }

            if (startIndex < 0)
            {
                throw new ArgumentException("The starting index must be greater than or equal to 0.", "startIndex");
            }

            if (length < 1)
            {
                throw new ArgumentException("The length must be greater than 0.", "length");
            }

            if (length > value.Length)
            {
                throw new ArgumentException("The length is larger than the length of the array.", "length");
            }

            if (startIndex >= value.Length)
            {
                throw new ArgumentException("Starting index is larger than the length of the array.", "startIndex");
            }

            if (startIndex + length > value.Length)
            {
                throw new ArgumentException("The length + starting index outside of the bounds of the array");
            }


        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonChunkWriter" /> class.
        /// </summary>
        /// <param name="stream">The stream to write into.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="System.IO.IOException">Thrown when tThe stream is read-only.
        /// <para>-or-</para>
        /// <para>Thrown when the stream cannot perform seek operations.</para>
        /// </exception>
        public GorgonChunkWriter(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanWrite)
            {
                throw new IOException("The stream is read-only.");
            }

            if (!stream.CanSeek)
            {
                throw new IOException("The stream is not seekable.");
            }

            _stream = stream;
        }
        #endregion
    }
}
