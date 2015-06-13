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
// Created: Thursday, June 11, 2015 10:29:03 PM
// 
#endregion

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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
	public interface IGorgonChunkReader
		: IDisposable
	{
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
		bool HasChunk(string chunkName);

		/// <summary>
		/// Function to read a signed byte from the stream.
		/// </summary>
		/// <returns>The signed byte in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		sbyte ReadSByte();

		/// <summary>
		/// Function to read an unsigned byte from the stream.
		/// </summary>
		/// <returns>Unsigned byte in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		byte ReadByte();

		/// <summary>
		/// Function to read a signed 16 bit integer from the stream.
		/// </summary>
		/// <returns>The signed 16 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		Int16 ReadInt16();

		/// <summary>
		/// Function to read an unsigned 16 bit integer from the stream.
		/// </summary>
		/// <returns>The unsigned 16 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		UInt16 ReadUInt16();

		/// <summary>
		/// Function to read a signed 32 bit integer from the stream.
		/// </summary>
		/// <returns>The signed 32 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		Int32 ReadInt32();

		/// <summary>
		/// Function to read a unsigned 32 bit integer from the stream.
		/// </summary>
		/// <returns>The unsigned 32 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		UInt32 ReadUInt32();

		/// <summary>
		/// Function to read a signed 64 bit integer from the stream.
		/// </summary>
		/// <returns>The signed 64 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		Int64 ReadInt64();

		/// <summary>
		/// Function to read an unsigned 64 bit integer from the stream.
		/// </summary>
		/// <returns>The unsigned 64 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		UInt64 ReadUInt64();

		/// <summary>
		/// Function to read a boolean value from the stream.
		/// </summary>
		/// <returns>The boolean value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		Boolean ReadBoolean();

		/// <summary>
		/// Function to read a single character from the stream.
		/// </summary>
		/// <returns>The single character in the stream.</returns>
		/// <remarks>This will read 2 bytes for the character since the default encoding for .NET is UTF-16.</remarks>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		char ReadChar();

		/// <summary>
		/// Function to read a string from the stream with the specified encoding.
		/// </summary>
		/// <param name="encoding">The encoding to use.</param>
		/// <returns>The string value in the stream.</returns>
		/// <remarks>If the <paramref name="encoding"/> is <b>null</b> (<i>Nothing</i> in VB.Net), UTF-8 encoding will be used instead.</remarks>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		string ReadString(Encoding encoding);

		/// <summary>
		/// Function to read a string from the stream.
		/// </summary>
		/// <returns>The string value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		string ReadString();

		/// <summary>
		/// Function to read double precision value from the stream.
		/// </summary>
		/// <returns>The double precision value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		double ReadDouble();

		/// <summary>
		/// Function to read a single precision floating point value from the stream.
		/// </summary>
		/// <returns>The single precision floating point value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		float ReadFloat();

		/// <summary>
		/// Function to read a decimal value from the stream.
		/// </summary>
		/// <returns>The decimal value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		decimal ReadDecimal();

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
		void Read(byte[] data, int startIndex, int count);

		/// <inheritdoc cref="Gorgon.IO.GorgonBinaryReader.ReadRange{T}(T[], int, int)"/>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		void ReadRange<T>(T[] value, int startIndex, int count)
			where T : struct;

		/// <inheritdoc cref="Gorgon.IO.GorgonBinaryReader.ReadRange{T}(T[], int)"/>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		void ReadRange<T>(T[] value, int count)
			where T : struct;

		/// <inheritdoc cref="Gorgon.IO.GorgonBinaryReader.ReadRange{T}(T[])"/>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		void ReadRange<T>(T[] value)
			where T : struct;

		/// <inheritdoc cref="Gorgon.IO.GorgonBinaryReader.ReadRange{T}(int)"/>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		T[] ReadRange<T>(int count)
			where T : struct;

		/// <inheritdoc cref="Gorgon.IO.GorgonBinaryReader.ReadValue{T}()"/>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The size of the value being read plus the current position in the stream exceeds the size of the chunk.</para>
		/// </exception>
		T Read<T>()
			where T : struct;

		/// <summary>
		/// Function to read a point from the stream.
		/// </summary>
		/// <returns>The point in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		Point ReadPoint();

		/// <summary>
		/// Function to read a point from the stream.
		/// </summary>
		/// <returns>The point in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		PointF ReadPointF();

		/// <summary>
		/// Function to read a point from the stream.
		/// </summary>
		/// <returns>The point in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		Size ReadSize();

		/// <summary>
		/// Function to read a point from the stream.
		/// </summary>
		/// <returns>The point in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		SizeF ReadSizeF();

		/// <summary>
		/// Function to read a rectangle from the stream.
		/// </summary>
		/// <returns>The rectangle in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		Rectangle ReadRectangle();

		/// <summary>
		/// Function to read a rectangle from the stream.
		/// </summary>
		/// <returns>The rectangle in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		RectangleF ReadRectangleF();

		/// <summary>
		/// Property to return the current mode for the chunking object.
		/// </summary>
		ChunkAccessMode ChunkAccessMode
		{
			get;
		}

		/// <summary>
		/// Property to return the underlying stream for the chunking object.
		/// </summary>
		Stream BaseStream
		{
			get;
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
		/// Always pair a call to <c>Begin</c> with a call to <see cref="GorgonChunkedFormat.End"/>, or else the file may get corrupted. If another call to <c>Begin</c> is made before <see cref="GorgonChunkedFormat.End"/> is called, 
		/// then the previous chunk will be closed automatically so the new one can begin.
		/// </para>
		/// </remarks>
		/// <seealso cref="GorgonChunkedFormat.End"/>
		uint Begin(string chunkName);

		/// <summary>
		/// Function to end the chunk stream.
		/// </summary>
		/// <returns>The size of the chunk, in bytes. Or 0 if no chunk has been started with <see cref="GorgonChunkedFormat.Begin"/>.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream cannot seek and reading has prematurely ended before getting the end of the chunk.</exception>
		/// <remarks>
		/// <para>
		/// This marks the end of a chunk in the stream. It writes out the total size of the chunk when writing a file, and when reading it uses the chunk size to skip 
		/// any unnecessary data if the object hasn't read the entire chunk.
		/// </para>
		/// <para>
		/// This method should always be paired with a call to <see cref="GorgonChunkedFormat.Begin"/>, otherwise nothing will happen.
		/// </para>
		/// <para>
		/// Like the <see cref="GorgonChunkedFormat.Begin"/> method, this method returns the number of bytes occupied by a chunk. This value may be used to validate a chunk.
		/// </para>
		/// </remarks>
		/// <seealso cref="GorgonChunkedFormat.Begin"/>
		uint End();

		/// <summary>
		/// Function to skip the specified number of bytes in the stream.
		/// </summary>
		/// <param name="byteCount">Number of bytes in the stream to skip.</param>
		/// <remarks>
		/// This method requires that the underlying <see cref="Stream"/> object have its <see cref="Stream.CanSeek"/> property return <b>true</b>, otherwise an exception will occur.
		/// </remarks>
		void SkipBytes(long byteCount);
	}
}