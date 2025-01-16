// Gorgon.
// Copyright (C) 2024 Michael Winsor
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
// Created: February 9, 2024 2:14:59 PM
//

using Gorgon.Native;

namespace Gorgon.IO;

/// <summary>
/// Functionality to read data from a <see cref="GorgonChunk"/> within a <see cref="GorgonChunkFile"/>.
/// </summary>
public interface IGorgonChunkReader
    : IDisposable
{
    /// <summary>
    /// Property to return the information about the <see cref="GorgonChunk"/> that this reader has opened.
    /// </summary>
    public ref readonly GorgonChunk ChunkInfo
    {
        get;
    }

    /// <summary>
    /// Function to read a boolean byte value from the chunk.
    /// </summary>
    /// <returns>The boolean value read from the chunk.</returns>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    bool ReadBool();

    /// <summary>
    /// Function to read a single byte from the chunk.
    /// </summary>
    /// <returns>The byte value read from the chunk.</returns>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    byte ReadByte();

    /// <summary>
    /// Function to read a signed 16 bit value from the chunk.
    /// </summary>
    /// <returns>The signed 16 bit value read from the chunk.</returns>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    short ReadInt16();

    /// <summary>
    /// Function to read a signed 32 bit value from the chunk.
    /// </summary>
    /// <returns>The signed 32 bit value read from the chunk.</returns>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    int ReadInt32();

    /// <summary>
    /// Function to read a signed 64 bit value from the chunk.
    /// </summary>
    /// <returns>The signed 64 bit value read from the chunk.</returns>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    long ReadInt64();

    /// <summary>
    /// Function to read an unsigned 16 bit value from the chunk.
    /// </summary>
    /// <returns>The unsigned 16 bit value read from the chunk.</returns>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    ushort ReadUInt16();

    /// <summary>
    /// Function to read an unsigned 32 bit value from the chunk.
    /// </summary>
    /// <returns>The unsigned 32 bit value read from the chunk.</returns>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    uint ReadUInt32();

    /// <summary>
    /// Function to read an unsigned 64 bit value from the chunk.
    /// </summary>
    /// <returns>The unsigned 64 bit value read from the chunk.</returns>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    ulong ReadUInt64();

    /// <summary>
    /// Function to read a half precision floating point (<see cref="Half"/>) value from the chunk.
    /// </summary>
    /// <returns>The half precision floating point value read from the chunk.</returns>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    Half ReadHalf();

    /// <summary>
    /// Function to read an single precision floating point (<see cref="float"/>) value from the chunk.
    /// </summary>
    /// <returns>The single precision floating point value read from the chunk.</returns>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    float ReadSingle();

    /// <summary>
    /// Function to read a double precision floating point (<see cref="double"/>) value from the chunk.
    /// </summary>
    /// <returns>The double precision floating point value read from the chunk.</returns>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    double ReadDouble();

    /// <summary>
    /// Function to read a decimal value from the chunk.
    /// </summary>
    /// <returns>The decimal value read from the chunk.</returns>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    decimal ReadDecimal();

    /// <summary>
    /// Function to read a single character value from the chunk.
    /// </summary>
    /// <returns>The single character read from the chunk.</returns>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    char ReadChar();

    /// <summary>
    /// Function to read a string value from the chunk.
    /// </summary>
    /// <param name="encoding">[Optional] The encoding for the string.</param>
    /// <returns>The string value read from the chunk.</returns>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    /// <remarks>
    /// <para>
    /// If the <paramref name="encoding"/> value is <b>null</b>, then the default encoding will be UTF-8.
    /// </para>
    /// </remarks>
    string ReadString(Encoding? encoding = null);

    /// <summary>
    /// Function to read the chunk data into a <see cref="Span{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of data within the span. Must be an unmanged type.</typeparam>
    /// <param name="data">The span that will receive the data from the chunk.</param>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    void ReadSpan<T>(Span<T> data) where T : unmanaged;

    /// <summary>
    /// Function to read the chunk data into an array.
    /// </summary>
    /// <typeparam name="T">The type of data within the array. Must be an unmanged type.</typeparam>
    /// <param name="data">The array that will receive the data from the chunk.</param>
    /// <param name="index">[Optional] The index within the <paramref name="data"/> array to start filling with data.</param>
    /// <param name="count">[Optional] The number of items to read.</param>
    /// <exception cref="ArgumentException">The index is less than 0, or greater than/equal to the size of the <paramref name="data"/> array.
    /// <para>-or-</para>
    /// <para>The <paramref name="count"/> minus the <paramref name="index"/> is too large for the <paramref name="data"/> array.</para>
    /// </exception>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    void ReadArray<T>(T[] data, int index = 0, int? count = null) where T : unmanaged;

    /// <summary>
    /// Function to read the contents of a chunk into native memory pointed at by the <see cref="GorgonPtr{T}"/> value.
    /// </summary>
    /// <typeparam name="T">The type of data in native memory. Must be an unmanged type.</typeparam>
    /// <param name="pointer">The pointer that points to the native memory where the data will be stored.</param>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    void ReadPointer<T>(GorgonPtr<T> pointer) where T : unmanaged;

    /// <summary>
    /// Function to read a single value from the chunk.
    /// </summary>
    /// <typeparam name="T">The type of the value. Must be an unmanged type.</typeparam>
    /// <param name="value">The value read from the chunk.</param>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    /// <remarks>
    /// <para>
    /// This overload is meant to be used with large unmanaged value types (typically larger than 16 bytes).
    /// </para>
    /// </remarks>
    void ReadValue<T>(out T value) where T : unmanaged;

    /// <summary>
    /// Function to read a single value from the chunk.
    /// </summary>
    /// <typeparam name="T">The type of the value. Must be an unmanged type.</typeparam>
    /// <returns>The value read from the chunk.</returns>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    /// <remarks>
    /// <para>
    /// This overload is meant to be used with small unmanaged value types (typically small than or equal to 16 bytes).
    /// </para>
    /// </remarks>
    T ReadValue<T>() where T : unmanaged;

    /// <summary>
    /// Function to skip a number of bytes in the chunk.
    /// </summary>
    /// <param name="bytes">The number of bytes to skip, if this is less than or equal to 0, then nothing will happen.</param>
    /// <returns>The number of bytes skipped.</returns>
    int Skip(int bytes);

    /// <summary>
    /// Function to deserialize object data from the underlying file stream.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize.</typeparam>
    /// <param name="deserializeCallback">The callback used to perform the deserialization from the stream.</param>
    /// <exception cref="EndOfStreamException">Thrown if the reader tries to read beyond the size of the chunk.</exception>
    /// <exception cref="IOException">Thrown if the method is called while the <paramref name="deserializeCallback"/> is executing.</exception>    
    /// <remarks>
    /// <para>
    /// This method is used to deserialize objects that have their own deserialization mechanisms (e.g. images, compression, etc...).
    /// </para>
    /// <para>
    /// The <paramref name="deserializeCallback"/> method will pass a <see cref="Stream"/> object to the callback method. The application should then perform any deserialization and return the resulting 
    /// object back to the user.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// Under no circumstances should the callback call any of the <see cref="IGorgonChunkReader"/> methods. Doing so will throw an exception.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// For example:
    /// <code language="csharp">
    /// <![CDATA[
    /// // MyObjectWithALoadMethod has a Load method exposed that takes a stream.
    /// 
    /// MyObjectWithALoadMethod myCallback(Stream stream) => MyObjectWithALoadMethod.Load(stream);
    /// 
    /// MyObjectWithALoadMethod value = reader.Deserialize<MyObjectWithALoadMethod>(myCallback);
    /// 
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    public T Deserialize<T>(Func<Stream, T> deserializeCallback);

    /// <summary>
    /// Function to close the chunk.
    /// </summary>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Deserialize"/> is executing.</exception>
    void Close();
}
