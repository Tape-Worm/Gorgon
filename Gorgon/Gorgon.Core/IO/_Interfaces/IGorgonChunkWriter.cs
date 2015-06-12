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
// Created: Thursday, June 11, 2015 10:28:58 PM
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
	/// Writes Gorgon chunked formatted data.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This object is used to serialize object data to a chunked file format. Essentially this takes the binary layout of the file, and marks data blocks in your object with identifier values when writing 
	/// to the file.
	/// </para>
	/// <para>
	/// Since chunk files use identifiers to identify parts of the data, the format for a given piece of data in an object should output a 64-bit <see cref="long"/> value used to identify the chunk. The 
	/// identifiers are built from an identifier string passed into the <see cref="GorgonChunkedFormat.Begin"/> method. This string must have 8 characters, 1 for each byte in a 64 bit <see cref="long"/> value.
	/// </para>
	/// <para>
	/// When writing the binary data, the user must call <see cref="GorgonChunkedFormat.Begin"/> for each logical grouping in your object structure. Then, using the writer methods write the parts that belong 
	/// to that grouping. When done, the user must call <see cref="GorgonChunkedFormat.End"/>.
	/// </para> 
	/// </remarks>
	/// <example>
	/// This code will write a file formatted with 3 chunks: a header, a list of strings, and a list of integer values:
	/// <code language="csharp"> 
	/// const string HeaderChunk = "HEAD_CHK"
	/// const string StringsChunk = "STRNGLST"
	/// const string IntChunk = "INTGRLST" 
	/// const uint head = 0xBAADBEEF;
	/// 
	/// string[] strings = { "Cow", "Pig", "Dog", "Cat", "Slagathor" };
	/// int[] ints { 1, 2, 9, 100, 122, 129, 882, 82, 62, 42 };
	/// 
	/// using (Stream myStream = File.Open("Some binary file you made.", FileMode.Create))
	/// {
	///		using (GorgonChunkWriter writer = new GorgonChunkWriter())
	///		{
	///			writer.Begin(HeaderChunk);
	/// 
	///			uint myHeader = writer.WriteUInt32(head);
	/// 
	///			writer.End();
	///	
	///			writer.Begin(StringsChunk);
	/// 
	///			writer.Write(strings.Length);
	///			for (int i = 0; i &lt; strings.Length; ++i)
	///			{
	///				writer.WriteString(strings[i]);
	///			}
	/// 
	///			writer.End();
	/// 
	///			writer.Begin(IntChunk);
	/// 
	///			writer.Write(ints.Length);
	///			for (int i = 0; i &lt; ints.Length; ++i)
	/// 		{
	///				writer.WriteInt32(ints[i]);
	///			} 
	///			writer.End();
	///		}
	/// } 
	/// </code>
	/// </example>
	public interface IGorgonChunkWriter
		: IDisposable
	{
		/// <summary>
		/// Function to write a signed byte to the stream.
		/// </summary>
		/// <param name="value">Value to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteSByte(sbyte value);

		/// <summary>
		/// Function to write an unsigned byte to the stream.
		/// </summary>
		/// <param name="value">Unsigned byte to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteByte(byte value);

		/// <summary>
		/// Function to write a signed 16 bit integer to the stream.
		/// </summary>
		/// <param name="value">The signed 16 bit integer to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteInt16(Int16 value);

		/// <summary>
		/// Function to write an unsigned 16 bit integer to the stream.
		/// </summary>
		/// <param name="value">The unsigned 16 bit integer to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteUInt16(UInt16 value);

		/// <summary>
		/// Function to write a signed 32 bit integer to the stream.
		/// </summary>
		/// <param name="value">The signed 32 bit integer to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteInt32(Int32 value);

		/// <summary>
		/// Function to write a unsigned 32 bit integer to the stream.
		/// </summary>
		/// <param name="value">The unsigned 32 bit integer to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteUInt32(UInt32 value);

		/// <summary>
		/// Function to write a signed 64 bit integer to the stream.
		/// </summary>
		/// <param name="value">The signed 64 bit integer to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteInt64(Int64 value);

		/// <summary>
		/// Function to write an unsigned 64 bit integer to the stream.
		/// </summary>
		/// <param name="value">The unsigned 64 bit integer to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteUInt64(UInt64 value);

		/// <summary>
		/// Function to write a boolean value to the stream.
		/// </summary>
		/// <param name="value">The boolean value to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteBoolean(Boolean value);

		/// <summary>
		/// Function to write a single character to the stream.
		/// </summary>
		/// <param name="value">The single character to write to the stream.</param>
		/// <remarks>This will write 2 bytes for the character since the default encoding for .NET is UTF-16.  Surrogate characters are not allowed, and will throw an exception.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="value"/> parameter is a surrogate character.</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteChar(char value);

		/// <summary>
		/// Function to write a string to the stream with the specified encoding.
		/// </summary>
		/// <param name="value">The string value to write to the stream.</param>
		/// <param name="encoding">The encoding to use.</param>
		/// <remarks>If the <paramref name="encoding"/> is <b>null</b> (<i>Nothing</i> in VB.Net), UTF-8 encoding will be used instead.</remarks>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteString(string value, Encoding encoding);

		/// <summary>
		/// Function to write a string to the stream.
		/// </summary>
		/// <param name="value">The string value to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteString(string value);

		/// <summary>
		/// Function to write double precision value to the stream.
		/// </summary>
		/// <param name="value">The double precision value to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteDouble(double value);

		/// <summary>
		/// Function to write a single precision floating point value to the stream.
		/// </summary>
		/// <param name="value">The single precision floating point value to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteFloat(float value);

		/// <summary>
		/// Function to write a decimal value to the stream.
		/// </summary>
		/// <param name="value">The decimal value to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteDecimal(decimal value);

		/// <summary>
		/// Function to write an array of bytes to the stream.
		/// </summary>
		/// <param name="data">Array of bytes to write to the stream.</param>
		/// <param name="startIndex">Starting index in the array.</param>
		/// <param name="count">Number of bytes in the array to write.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="data"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="startIndex"/> parameter is less than 0.
		/// <para>-or-</para>
		/// <para>Thrown when the startIndex parameter is equal to or greater than the number of elements in the value parameter.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the sum of startIndex and <paramref name="count"/> is greater than the number of elements in the value parameter.</para>
		/// </exception>
		void Write(byte[] data, int startIndex, int count);

		/// <summary>
		/// Function to write a point to the stream.
		/// </summary>
		/// <param name="value">The point to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WritePoint(Point value);

		/// <summary>
		/// Function to write a point to the stream.
		/// </summary>
		/// <param name="value">The point to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WritePoint(PointF value);

		/// <summary>
		/// Function to write a point to the stream.
		/// </summary>
		/// <param name="value">The point to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteSize(Size value);

		/// <summary>
		/// Function to write a point to the stream.
		/// </summary>
		/// <param name="value">The point to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteSize(SizeF value);

		/// <summary>
		/// Function to write a rectangle to the stream.
		/// </summary>
		/// <param name="value">The rectangle to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteRectangle(Rectangle value);

		/// <summary>
		/// Function to write a rectangle to the stream.
		/// </summary>
		/// <param name="value">The rectangle to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		void WriteRectangle(RectangleF value);

		/// <summary>
		/// Function to write a range of generic values.
		/// </summary>
		/// <typeparam name="T">Type of value to write.  Must be a value type.</typeparam>
		/// <param name="value">Array of values to write.</param>
		/// <param name="startIndex">Starting index in the array.</param>
		/// <param name="count">Number of array elements to copy.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="value"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="startIndex"/> parameter is less than 0.
		/// <para>-or-</para>
		/// <para>Thrown when the startIndex parameter is equal to or greater than the number of elements in the value parameter.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the sum of startIndex and <paramref name="count"/> is greater than the number of elements in the value parameter.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		/// <remarks>
		/// <para>
		/// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
		/// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
		/// </para>
		/// <para>
		/// Value types with marshalling attributes are <i>not</i> supported and will not be written correctly.
		/// </para>
		/// </remarks>
		void WriteRange<T>(T[] value, int startIndex, int count)
			where T : struct;

		/// <summary>
		/// Function to write a range of generic values.
		/// </summary>
		/// <typeparam name="T">Type of value to write.  Must be a value type.</typeparam>
		/// <param name="value">Array of values to write.</param>
		/// <param name="count">Number of array elements to copy.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="value"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when <paramref name="count"/> parameter is greater than the number of elements in the value parameter.
		/// </exception>
		/// <remarks>
		/// <para>
		/// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
		/// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
		/// </para>
		/// <para>
		/// Value types with marshalling attributes are <i>not</i> supported and will not be written correctly.
		/// </para>
		/// </remarks>
		void WriteRange<T>(T[] value, int count)
			where T : struct;

		/// <summary>
		/// Functio to write a range of generic values.
		/// </summary>
		/// <typeparam name="T">Type of value to write.  Must be a value type.</typeparam>
		/// <param name="value">Array of values to write.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="value"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>
		/// <para>
		/// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
		/// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
		/// </para>
		/// <para>
		/// Value types with marshalling attributes are <i>not</i> supported and will not be written correctly.
		/// </para>
		/// </remarks>
		void WriteRange<T>(T[] value)
			where T : struct;

		/// <summary>
		/// Function to write a generic value to the stream.
		/// </summary>
		/// <typeparam name="T">Type of value to write.  Must be a value type.</typeparam>
		/// <param name="value">Value to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		/// <remarks>
		/// <para>
		/// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
		/// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
		/// </para>
		/// <para>
		/// Value types with marshalling attributes are <i>not</i> supported and will not be written correctly.
		/// </para>
		/// </remarks>
		void Write<T>(T value)
			where T : struct;

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