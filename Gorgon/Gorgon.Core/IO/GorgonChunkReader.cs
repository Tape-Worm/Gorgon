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
// Created: Wednesday, January 23, 2013 7:37:44 PM
// 
#endregion

using System;
using System.Drawing;
using System.IO;
using System.Text;
using Gorgon.Core.Properties;
using Gorgon.Native;

namespace Gorgon.IO
{
	/// <summary>
	/// Reads Gorgon chunked formatted data.
	/// </summary>
	/// <remarks>This object will take data and turn it into chunks of data.  This is similar to the old IFF format in that 
	/// it allows Gorgon's file formats to be future proof.  That is, if a later version of Gorgon has support for a feature
	/// that does not exist in a previous version, then the older version will be able to read the file and skip the 
	/// unnecessary parts.</remarks>
	public class GorgonChunkReader
		: GorgonChunkedFormat
    {
        #region Methods.
        /// <summary>
        /// Function to determine if the next bytes indicate match the chunk ID.
        /// </summary>
        /// <param name="chunkName">Name of the chunk.</param>
        /// <returns><b>true</b> if the next bytes are a the specified chunk ID, <b>false</b> if not.</returns>
        /// <remarks>The <paramref name="chunkName"/> parameter must be at least 8 characters in length, if it is not, then an exception will be thrown. 
        /// If the chunkName parameter is longer than 8 characters, then it will be truncated to 8 characters.
        /// </remarks>
        public bool HasChunk(string chunkName)
        {
            if (chunkName.Length < 8)
            {
                throw new ArgumentException(Resources.GOR_CHUNK_NAME_TOO_SMALL, "chunkName");
            }

            if (chunkName.Length > 8)
            {
                chunkName = chunkName.Substring(0, 8);
            }

            // If we're at the end of the stream, then obviously we don't have the chunk ID.
            if (Reader.BaseStream.Position + 8 > Reader.BaseStream.Length)
            {
                return false;
            }

            long streamPosition = Reader.BaseStream.Position;
            ulong chunkID = GetChunkCode(chunkName);
            ulong streamData = ReadUInt64();

            // Reset the stream position.
            Reader.BaseStream.Position = streamPosition;

            return chunkID == streamData;
        }

        /// <summary>
		/// Function to read a signed byte from the stream.
		/// </summary>
		/// <returns>The signed byte in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public sbyte ReadSByte()
		{
			ValidateAccess(false);

			return Reader.ReadSByte();
		}

		/// <summary>
		/// Function to read an unsigned byte from the stream.
		/// </summary>
		/// <returns>Unsigned byte in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public byte ReadByte()
		{
			ValidateAccess(false);
			return Reader.ReadByte();
		}

		/// <summary>
		/// Function to read a signed 16 bit integer from the stream.
		/// </summary>
		/// <returns>The signed 16 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public Int16 ReadInt16()
		{
			ValidateAccess(false);
			return Reader.ReadInt16();
		}

		/// <summary>
		/// Function to read an unsigned 16 bit integer from the stream.
		/// </summary>
		/// <returns>The unsigned 16 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public UInt16 ReadUInt16()
		{
			ValidateAccess(false);
			return Reader.ReadUInt16();
		}

		/// <summary>
		/// Function to read a signed 32 bit integer from the stream.
		/// </summary>
		/// <returns>The signed 32 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public Int32 ReadInt32()
		{
			ValidateAccess(false);
			return Reader.ReadInt32();
		}

		/// <summary>
		/// Function to read a unsigned 32 bit integer from the stream.
		/// </summary>
		/// <returns>The unsigned 32 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public UInt32 ReadUInt32()
		{
			ValidateAccess(false);
			return Reader.ReadUInt32();
		}

		/// <summary>
		/// Function to read a signed 64 bit integer from the stream.
		/// </summary>
		/// <returns>The signed 64 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public Int64 ReadInt64()
		{
			ValidateAccess(false);
			return Reader.ReadInt64();
		}

		/// <summary>
		/// Function to read an unsigned 64 bit integer from the stream.
		/// </summary>
		/// <returns>The unsigned 64 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public UInt64 ReadUInt64()
		{
			ValidateAccess(false);
			return Reader.ReadUInt64();
		}

		/// <summary>
		/// Function to read a boolean value from the stream.
		/// </summary>
		/// <returns>The boolean value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public Boolean ReadBoolean()
		{
			ValidateAccess(false);
			return Reader.ReadBoolean();
		}

		/// <summary>
		/// Function to read a single character from the stream.
		/// </summary>
		/// <returns>The single character in the stream.</returns>
		/// <remarks>This will read 2 bytes for the character since the default encoding for .NET is UTF-16.</remarks>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public char ReadChar()
		{
			ValidateAccess(false);

			return (char)ReadInt16();
		}

		/// <summary>
		/// Function to read a string from the stream with the specified encoding.
		/// </summary>
		/// <param name="encoding">The encoding to use.</param>
		/// <returns>The string value in the stream.</returns>
		/// <remarks>If the <paramref name="encoding"/> is <b>null</b> (<i>Nothing</i> in VB.Net), UTF-8 encoding will be used instead.</remarks>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public string ReadString(Encoding encoding)
		{
			ValidateAccess(false);

			return Reader.BaseStream.ReadString(encoding);
		}

		/// <summary>
		/// Function to read a string from the stream.
		/// </summary>
		/// <returns>The string value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public string ReadString()
		{
			ValidateAccess(false);
			return Reader.BaseStream.ReadString(null);
		}

		/// <summary>
		/// Function to read double precision value from the stream.
		/// </summary>
		/// <returns>The double precision value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public double ReadDouble()
		{
			ValidateAccess(false);
			return Reader.ReadDouble();
		}

		/// <summary>
		/// Function to read a single precision floating point value from the stream.
		/// </summary>
		/// <returns>The single precision floating point value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public float ReadFloat()
		{
			ValidateAccess(false);
			return Reader.ReadSingle();
		}

		/// <summary>
		/// Function to read a decimal value from the stream.
		/// </summary>
		/// <returns>The decimal value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public decimal ReadDecimal()
		{
			ValidateAccess(false);
			return Reader.ReadDecimal();
		}

		/// <summary>
		/// Function to read an array of bytes from the stream.
		/// </summary>
		/// <param name="data">Array of bytes in the stream.</param>
        /// <param name="startIndex">Starting index in the array.</param>
        /// <param name="count">Number of bytes in the array to read.</param>
        /// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="data"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="startIndex"/> parameter is less than 0.
        /// <para>-or-</para>
        /// <para>Thrown when the startIndex parameter is equal to or greater than the number of elements in the value parameter.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the sum of startIndex and <paramref name="count"/> is greater than the number of elements in the value parameter.</para>
        /// </exception>
        public void Read(byte[] data, int startIndex, int count)
		{
			ValidateAccess(false);

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if ((data.Length == 0) || (count <= 0))
            {
                return;
            }

            if ((startIndex < 0) || (startIndex >= data.Length))
            {
                throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_INDEX_OUT_OF_RANGE, startIndex,
                                                                    data.Length));
            }

            if (startIndex + count > data.Length)
            {
                throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_INDEX_OUT_OF_RANGE, startIndex + count,
                                                                    data.Length));
            }            

			Reader.Read(data, startIndex, count);
		}

        /// <summary>
        /// Function to read a range of generic values.
        /// </summary>
        /// <typeparam name="T">Type of value to read.  Must be a value type.</typeparam>
        /// <param name="value">Array of values to read.</param>
        /// <param name="startIndex">Starting index in the array.</param>
        /// <param name="count">Number of array elements to copy.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="value"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="startIndex"/> parameter is less than 0.
        /// <para>-or-</para>
        /// <para>Thrown when the startIndex parameter is equal to or greater than the number of elements in the value parameter.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the sum of startIndex and <paramref name="count"/> is greater than the number of elements in the value parameter.</para>
        /// </exception>
        /// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
        public unsafe void ReadRange<T>(T[] value, int startIndex, int count)
            where T : struct
        {
            ValidateAccess(false);

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if ((value.Length == 0) || (count <= 0))
            {
                return;
            }

            if ((startIndex < 0) || (startIndex >= value.Length))
            {
                throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_INDEX_OUT_OF_RANGE, startIndex,
                                                                    value.Length));
            }

            if (startIndex + count > value.Length)
            {
                throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_INDEX_OUT_OF_RANGE, startIndex + count,
                                                                    value.Length));
            }

            int typeSize = DirectAccess.SizeOf<T>();
            int size = typeSize * count;
            int offset = startIndex * typeSize;

            if (TempBuffer == null)
            {
                TempBuffer = new byte[TempBufferSize];
            }

            fixed (byte* tempBufferPointer = &TempBuffer[0])
            {
                while (size > 0)
                {
                    int blockSize = size > TempBufferSize ? TempBufferSize : size;

                    // Read the data from the stream as byte values.
                    Read(TempBuffer, 0, blockSize);
                    
                    // Copy into our array.
                    DirectAccess.ReadArray(tempBufferPointer, value, offset, blockSize);

                    offset += blockSize;
                    size -= blockSize;                    
                }
            }
        }

        /// <summary>
        /// Function to read a range of generic values.
        /// </summary>
        /// <typeparam name="T">Type of value to read.  Must be a value type.</typeparam>
        /// <param name="value">Array of values to read.</param>
        /// <param name="count">Number of array elements to copy.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="value"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="count"/> parameter is greater than the number of elements in the value parameter.
        /// </exception>
        /// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
        public void ReadRange<T>(T[] value, int count)
            where T : struct
        {
            ReadRange(value, 0, count);
        }

        /// <summary>
        /// Function to read a range of generic values.
        /// </summary>
        /// <typeparam name="T">Type of value to read.  Must be a value type.</typeparam>
        /// <param name="value">Array of values to read.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="value"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
        public void ReadRange<T>(T[] value)
            where T : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            ReadRange(value, 0, value.Length);
        }

        /// <summary>
        /// Function to read a range of generic values.
        /// </summary>
        /// <typeparam name="T">Type of value to read.  Must be a value type.</typeparam>
        /// <param name="count">Number of array elements to copy.</param>
        /// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
        public T[] ReadRange<T>(int count)
            where T : struct
        {
            var array = new T[count];

            ReadRange(array, 0, count);

            return array;
        }
                
        /// <summary>
		/// Function to read a generic value from the stream.
		/// </summary>
		/// <typeparam name="T">Type of value to read.  Must be a value type.</typeparam>
		/// <returns>The value in the stream.</returns>
		public unsafe T Read<T>()
			where T : struct
		{
			T returnVal;
			int size = DirectAccess.SizeOf<T>();
			byte* pointer = stackalloc byte[size];
			byte* bytes = pointer;

			while (size > 0)
			{
				if (size >= 8)
				{
					*((long*)bytes) = Reader.ReadInt64();
					bytes += 8;
					size -= 8;
				}
				else if (size >= 4)
				{
					*((int*)bytes) = Reader.ReadInt32();
					bytes += 4;
					size -= 4;
				}
				else if (size >= 2)
				{
					*((short*)bytes) = Reader.ReadInt16();
					bytes += 2;
					size -= 2;
				}
				else
				{
					*bytes = Reader.ReadByte();
					bytes++;
					size--;
				}
			}

			DirectAccess.ReadValue(pointer, out returnVal);

			return returnVal;
		}

		/// <summary>
		/// Function to read a point from the stream.
		/// </summary>
		/// <returns>The point in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public Point ReadPoint()
		{
			ValidateAccess(false);

			return new Point(Reader.ReadInt32(), Reader.ReadInt32());
		}

		/// <summary>
		/// Function to read a point from the stream.
		/// </summary>
		/// <returns>The point in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public PointF ReadPointF()
		{
			ValidateAccess(false);

			return new PointF(Reader.ReadSingle(), Reader.ReadSingle());
		}

		/// <summary>
		/// Function to read a point from the stream.
		/// </summary>
		/// <returns>The point in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public Size ReadSize()
		{
			ValidateAccess(false);

			return new Size(Reader.ReadInt32(), Reader.ReadInt32());
		}

		/// <summary>
		/// Function to read a point from the stream.
		/// </summary>
		/// <returns>The point in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public SizeF ReadSizeF()
		{
			ValidateAccess(false);

			return new SizeF(Reader.ReadSingle(), Reader.ReadSingle());
		}

		/// <summary>
		/// Function to read a rectangle from the stream.
		/// </summary>
		/// <returns>The rectangle in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public Rectangle ReadRectangle()
		{
			ValidateAccess(false);

			return new Rectangle(Reader.ReadInt32(), Reader.ReadInt32(), Reader.ReadInt32(), Reader.ReadInt32());
		}

		/// <summary>
		/// Function to read a rectangle from the stream.
		/// </summary>
		/// <returns>The rectangle in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public RectangleF ReadRectangleF()
		{
			ValidateAccess(false);

			return new RectangleF(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonChunkReader" /> class.
		/// </summary>
		/// <param name="stream">The stream.</param>
		public GorgonChunkReader(Stream stream)
			: base(stream, ChunkAccessMode.Read)
		{
		}
		#endregion
	}
}
