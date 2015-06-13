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
using System.Runtime.InteropServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Core.Properties;
using Gorgon.Native;

namespace Gorgon.IO
{
	/// <summary>
	/// Reads Gorgon chunked formatted data.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This object is used to deserialize object data from a chunked file format. Essentially this takes the binary layout of the file, and makes it easier to process by reading the identifiers within the 
	/// format. 
	/// </para>
	/// <para>
	/// Since chunk files use identifiers to identify parts of the data, the format for a given piece of data should be fairly simple to parse as the identifiers are merely constant 64-bit <see cref="long"/> 
	/// values. The identifiers are built from an identifier string passed into the <see cref="GorgonChunkedFormat.Begin"/> method. This string must have 8 characters, 1 for each byte in a 64 bit <see cref="long"/> value.
	/// </para>
	/// <para>
	/// When reading the binary data, the user should check for the existence of a chunk with <see cref="HasChunk"/>, and then make a call to <see cref="GorgonChunkedFormat.Begin"/>. Then, using the reader methods read 
	/// the data in to the object. When done, the user must call <see cref="GorgonChunkedFormat.End"/>.
	/// </para> 
	/// </remarks>
	/// <example>
	/// This code will read a file formatted with 3 chunks: a header, a list of strings, and a list of integer values:
	/// <code language="csharp"> 
	/// const string HeaderChunk = "HEAD_CHK"
	/// const string StringsChunk = "STRNGLST"
	/// const string IntChunk = "INTGRLST" 
	/// const uint head = 0xBAADBEEF;
	/// 
	/// string[] strings;
	/// int[] ints;
	/// 
	/// using (Stream myStream = File.Open("Some binary file you made.", FileMode.Open))
	/// {
	///		using (GorgonChunkReader reader = new GorgonChunkReader())
	///		{
	///			if (!reader.HasChunk(HeaderChunk))
	///			{
	///				throw new Exception("This is not the correct file type!");
	///			}
	/// 
	///			reader.Begin(HeaderChunk);
	/// 
	///			uint myHeader = reader.ReadUInt32();
	/// 
	///			if (myHeader != head)
	///			{
	///				throw new Exception("This is not the correct file type!");
	///			}
	/// 
	///			reader.End();
	/// 
	///			if (!reader.HasChunk(StringsChunk))
	///			{
	///				throw new Exception("This is not the correct file type!");
	///			}
	///	
	///			reader.Begin(StringsChunk);
	///			int strCount = reader.ReadInt32();
	///			
	///			if (strCount > 0)
	///			{
	///				strings = new string[strCount];
	/// 
	///				for (int i = 0; i &lt; strCount; ++i)
	///				{
	///					strings[i] = reader.ReadString();		
	///				}
	///			}
	///			reader.End();
	/// 
	///			if (!reader.HasChunk(IntChunk))
	///			{
	///				throw new Exception("This is not the correct file type!");
	///			}
	///			
	///			reader.Begin(IntChunk);
	///			int intCount = reader.ReadInt32();
	///			
	///			if (intCount > 0)
	///			{
	///				ints = new int[intCount];
	/// 
	///				for (int i = 0; i &lt; intCount; ++i)
	///				{
	///					ints[i] = reader.ReadInt32();		
	///				}
	///			} 
	///			reader.End();
	///		}
	/// } 
	/// </code>
	/// </example>
	public class GorgonChunkReader
		: GorgonChunkedFormat, IGorgonChunkReader
	{
        #region Methods.
		/// <summary>
		/// Function to return the formatted exception message for the out of bounds exception.
		/// </summary>
		/// <param name="size">Size of the requested data, in bytes.</param>
		/// <returns>The exception message.</returns>
		private string GetOutOfBoundsExceptionMessage(long size)
		{
			return string.Format(Resources.GOR_ERR_CHUNK_OUT_OF_BOUNDS, size.FormatMemory(), CurrentChunkSize.FormatMemory());
		}

		/// <summary>
		/// Function to determine if size requested for a read is within the chunk boundaries.
		/// </summary>
		/// <returns><b>true</b> if within the chunk, <b>false</b> if not.</returns>
		private bool InChunkBounds(long size)
		{
			if (size > CurrentChunkSize)
			{
				return false;
			}

			long length = CurrentChunkSize - (Reader.BaseStream.Position - ChunkStart);

			return size <= length;
		}

        /// <summary>
        /// Function to determine if the next 8 bytes indicate match the chunk ID.
        /// </summary>
        /// <param name="chunkName">Name of the chunk.</param>
        /// <returns><b>true</b> if the next bytes are a the specified chunk ID, <b>false</b> if not.</returns>
        /// <remarks>The <paramref name="chunkName"/> parameter must be at least 8 characters in length, if it is not, then an exception will be thrown. 
        /// If the chunkName parameter is longer than 8 characters, then it will be truncated to 8 characters.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="chunkName"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="chunkName"/> parameter is not equal to exactly 8 characters.</exception>
        public bool HasChunk(string chunkName)
        {
            if (chunkName.Length != 8)
            {
                throw new ArgumentException(Resources.GOR_ERR_CHUNK_NAME_SIZE_MISMATCH, "chunkName");
            }

            // If we're at the end of the stream, then obviously we don't have the chunk ID.
            if (Reader.BaseStream.Position + 8 >= Reader.BaseStream.Length)
            {
                return false;
            }

            long streamPosition = Reader.BaseStream.Position;
            ulong chunkID = GetChunkCode(chunkName);
            ulong streamData = Reader.ReadUInt64();

            // Reset the stream position.
            Reader.BaseStream.Position = streamPosition;

            return chunkID == streamData;
        }

        /// <summary>
		/// Function to read a signed byte from the stream.
		/// </summary>
		/// <returns>The signed byte in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public sbyte ReadSByte()
		{
			if (!InChunkBounds(sizeof(sbyte)))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(sbyte)));
			}

			ValidateAccess(false);
			return Reader.ReadSByte();
		}

		/// <summary>
		/// Function to read an unsigned byte from the stream.
		/// </summary>
		/// <returns>Unsigned byte in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public byte ReadByte()
		{
			if (!InChunkBounds(sizeof(byte)))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(byte)));
			}

			ValidateAccess(false);
			return Reader.ReadByte();
		}

		/// <summary>
		/// Function to read a signed 16 bit integer from the stream.
		/// </summary>
		/// <returns>The signed 16 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public Int16 ReadInt16()
		{
			if (!InChunkBounds(sizeof(Int16)))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(Int16)));
			}

			ValidateAccess(false);
			return Reader.ReadInt16();
		}

		/// <summary>
		/// Function to read an unsigned 16 bit integer from the stream.
		/// </summary>
		/// <returns>The unsigned 16 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public UInt16 ReadUInt16()
		{
			if (!InChunkBounds(sizeof(UInt16)))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(UInt16)));
			}

			ValidateAccess(false);
			return Reader.ReadUInt16();
		}

		/// <summary>
		/// Function to read a signed 32 bit integer from the stream.
		/// </summary>
		/// <returns>The signed 32 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public Int32 ReadInt32()
		{
			if (!InChunkBounds(sizeof(Int32)))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(Int32)));
			}

			ValidateAccess(false);
			return Reader.ReadInt32();
		}

		/// <summary>
		/// Function to read a unsigned 32 bit integer from the stream.
		/// </summary>
		/// <returns>The unsigned 32 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public UInt32 ReadUInt32()
		{
			if (!InChunkBounds(sizeof(UInt32)))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(UInt32)));
			}

			ValidateAccess(false);
			return Reader.ReadUInt32();
		}

		/// <summary>
		/// Function to read a signed 64 bit integer from the stream.
		/// </summary>
		/// <returns>The signed 64 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public Int64 ReadInt64()
		{
			if (!InChunkBounds(sizeof(Int64)))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(Int64)));
			}

			ValidateAccess(false);
			return Reader.ReadInt64();
		}

		/// <summary>
		/// Function to read an unsigned 64 bit integer from the stream.
		/// </summary>
		/// <returns>The unsigned 64 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public UInt64 ReadUInt64()
		{
			if (!InChunkBounds(sizeof(UInt64)))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(UInt64)));
			}

			ValidateAccess(false);
			return Reader.ReadUInt64();
		}

		/// <summary>
		/// Function to read a boolean value from the stream.
		/// </summary>
		/// <returns>The boolean value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public Boolean ReadBoolean()
		{
			if (!InChunkBounds(sizeof(bool)))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(bool)));
			}

			ValidateAccess(false);
			return Reader.ReadBoolean();
		}

		/// <summary>
		/// Function to read a single character from the stream.
		/// </summary>
		/// <returns>The single character in the stream.</returns>
		/// <remarks>This will read 2 bytes for the character since the default encoding for .NET is UTF-16.</remarks>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public char ReadChar()
		{
			if (!InChunkBounds(sizeof(Int16)))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(Int16)));
			}

			ValidateAccess(false);

			return (char)ReadInt16();
		}

		/// <summary>
		/// Function to read a string from the stream with the specified encoding.
		/// </summary>
		/// <param name="encoding">The encoding to use.</param>
		/// <returns>The string value in the stream.</returns>
		/// <remarks>If the <paramref name="encoding"/> is <b>null</b> (<i>Nothing</i> in VB.Net), UTF-8 encoding will be used instead.</remarks>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public string ReadString(Encoding encoding)
		{
			if (!InChunkBounds(sizeof(byte)))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(byte)));
			}

			ValidateAccess(false);

			return Reader.BaseStream.ReadString(encoding);
		}

		/// <summary>
		/// Function to read a string from the stream.
		/// </summary>
		/// <returns>The string value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public string ReadString()
		{
			if (!InChunkBounds(sizeof(byte)))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(byte)));
			}

			ValidateAccess(false);
			return Reader.BaseStream.ReadString(null);
		}

		/// <summary>
		/// Function to read double precision value from the stream.
		/// </summary>
		/// <returns>The double precision value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public double ReadDouble()
		{
			if (!InChunkBounds(sizeof(double)))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(double)));
			}

			ValidateAccess(false);
			return Reader.ReadDouble();
		}

		/// <summary>
		/// Function to read a single precision floating point value from the stream.
		/// </summary>
		/// <returns>The single precision floating point value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public float ReadFloat()
		{
			if (!InChunkBounds(sizeof(Single)))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(Single)));
			}

			ValidateAccess(false);
			return Reader.ReadSingle();
		}

		/// <summary>
		/// Function to read a decimal value from the stream.
		/// </summary>
		/// <returns>The decimal value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public decimal ReadDecimal()
		{
			if (!InChunkBounds(sizeof(decimal)))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(decimal)));
			}

			ValidateAccess(false);
			return Reader.ReadDecimal();
		}

		/// <summary>
		/// Function to read an array of bytes from the stream.
		/// </summary>
		/// <param name="data">Array of bytes in the stream.</param>
        /// <param name="startIndex">Starting index in the array.</param>
        /// <param name="count">Number of bytes in the array to read.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="data"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="startIndex"/> parameter is less than 0.
        /// <para>-or-</para>
        /// <para>Thrown when the startIndex parameter is equal to or greater than the number of elements in the value parameter.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the sum of startIndex and <paramref name="count"/> is greater than the number of elements in the value parameter.</para>
        /// </exception>
        public void Read(byte[] data, int startIndex, int count)
		{
			if (!InChunkBounds(count))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(count));
			}

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

		/// <inheritdoc cref="Gorgon.IO.GorgonBinaryReader.ReadRange{T}(T[], int, int)"/>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public void ReadRange<T>(T[] value, int startIndex, int count)
            where T : struct
        {
			if (!InChunkBounds(DirectAccess.SizeOf<T>() * count))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(DirectAccess.SizeOf<T>() * count));
			}

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

			Reader.ReadRange(value, startIndex, count);
        }

		/// <inheritdoc cref="Gorgon.IO.GorgonBinaryReader.ReadRange{T}(T[], int)"/>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public void ReadRange<T>(T[] value, int count)
            where T : struct
        {
            ReadRange(value, 0, count);
        }

		/// <inheritdoc cref="Gorgon.IO.GorgonBinaryReader.ReadRange{T}(T[])"/>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public void ReadRange<T>(T[] value)
            where T : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            ReadRange(value, 0, value.Length);
        }

		/// <inheritdoc cref="Gorgon.IO.GorgonBinaryReader.ReadRange{T}(int)"/>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public T[] ReadRange<T>(int count)
            where T : struct
        {
			if (!InChunkBounds(DirectAccess.SizeOf<T>() * count))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(DirectAccess.SizeOf<T>() * count));
			}

            var array = new T[count];

            ReadRange(array, 0, count);

            return array;
        }

		/// <inheritdoc cref="Gorgon.IO.GorgonBinaryReader.ReadValue{T}()"/>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public T Read<T>()
			where T : struct
        {
			if (!InChunkBounds(DirectAccess.SizeOf<T>()))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(DirectAccess.SizeOf<T>()));
			}

			ValidateAccess(false);

			return Reader.ReadValue<T>();
		}

		/// <summary>
		/// Function to read a point from the stream.
		/// </summary>
		/// <returns>The point in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public Point ReadPoint()
		{
			if (!InChunkBounds(sizeof(Int32) * 2))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(Int32) * 2));
			}

			ValidateAccess(false);

			return new Point(Reader.ReadInt32(), Reader.ReadInt32());
		}

		/// <summary>
		/// Function to read a point from the stream.
		/// </summary>
		/// <returns>The point in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public PointF ReadPointF()
		{
			if (!InChunkBounds(sizeof(Single) * 2))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(Single) * 2));
			}

			ValidateAccess(false);

			return new PointF(Reader.ReadSingle(), Reader.ReadSingle());
		}

		/// <summary>
		/// Function to read a point from the stream.
		/// </summary>
		/// <returns>The point in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public Size ReadSize()
		{
			if (!InChunkBounds(sizeof(Int32) * 2))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(Int32) * 2));
			}

			ValidateAccess(false);

			return new Size(Reader.ReadInt32(), Reader.ReadInt32());
		}

		/// <summary>
		/// Function to read a point from the stream.
		/// </summary>
		/// <returns>The point in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public SizeF ReadSizeF()
		{
			if (!InChunkBounds(sizeof(Single) * 2))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(Single) * 2));
			}

			ValidateAccess(false);

			return new SizeF(Reader.ReadSingle(), Reader.ReadSingle());
		}

		/// <summary>
		/// Function to read a rectangle from the stream.
		/// </summary>
		/// <returns>The rectangle in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public Rectangle ReadRectangle()
		{
			if (!InChunkBounds(sizeof(Int32) * 4))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(Int32) * 4));
			}

			ValidateAccess(false);

			return new Rectangle(Reader.ReadInt32(), Reader.ReadInt32(), Reader.ReadInt32(), Reader.ReadInt32());
		}

		/// <summary>
		/// Function to read a rectangle from the stream.
		/// </summary>
		/// <returns>The rectangle in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		public RectangleF ReadRectangleF()
		{
			if (!InChunkBounds(sizeof(Single) * 2))
			{
				throw new IOException(GetOutOfBoundsExceptionMessage(sizeof(Single) * 2));
			}

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
