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
/// Functionality to write data into a <see cref="GorgonChunk"/> for a <see cref="GorgonChunkFile"/>.
/// </summary>
public interface IGorgonChunkWriter
    : IDisposable
{
    /// <summary>
    /// Property to return the ID of the chunk being written. 
    /// </summary>
    ulong ID
    {
        get;
    }

    /// <summary>
    /// Function to write a boolean byte value to the chunk.
    /// </summary>
    /// <param name="value">The boolean value to write into the chunk.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    void WriteBool(bool value);

    /// <summary>
    /// Function to write a single byte to the chunk.
    /// </summary>
    /// <param name="value">The byte value to write into the chunk.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    void WriteByte(byte value);

    /// <summary>
    /// Function to write a signed 16 bit value to the chunk.
    /// </summary>
    /// <param name="value">The signed 16 bit value to write into the chunk.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    void WriteInt16(short value);

    /// <summary>
    /// Function to write a signed 32 bit value to the chunk.
    /// </summary>
    /// <param name="value">The signed 32 bit value to write into the chunk.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    void WriteInt32(int value);

    /// <summary>
    /// Function to write a signed 64 bit value to the chunk.
    /// </summary>
    /// <param name="value">The signed 64 bit value to write into the chunk.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    void WriteInt64(long value);

    /// <summary>
    /// Function to write an unsigned 16 bit value to the chunk.
    /// </summary>
    /// <param name="value">The unsigned 16 bit value to write into the chunk.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    void WriteUInt16(ushort value);

    /// <summary>
    /// Function to write an unsigned 32 bit value to the chunk.
    /// </summary>
    /// <param name="value">The unsigned 32 bit value to write into the chunk.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    void WriteUInt32(uint value);

    /// <summary>
    /// Function to write an unsigned 64 bit value to the chunk.
    /// </summary>
    /// <param name="value">The unsigned 64 bit value to write into the chunk.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    void WriteUInt64(ulong value);

    /// <summary>
    /// Function to write a half precision floating point (<see cref="Half"/>) value to the chunk.
    /// </summary>
    /// <param name="value">The half precision floating point value to write to the chunk.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    void WriteHalf(Half value);

    /// <summary>
    /// Function to write an single precision floating point (<see cref="float"/>) value to the chunk.
    /// </summary>
    /// <param name="value">The single precision floating point value to write into the chunk.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    void WriteSingle(float value);

    /// <summary>
    /// Function to write a double precision floating point (<see cref="double"/>) value to the chunk.
    /// </summary>
    /// <param name="value">The double precision floating point value to write into the chunk.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    void WriteDouble(double value);

    /// <summary>
    /// Function to write a decimal value to the chunk.
    /// </summary>
    /// <param name="value">The decimal value to write into the chunk.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    void WriteDecimal(decimal value);

    /// <summary>
    /// Function to write a single character value to the chunk.
    /// </summary>
    /// <param name="value">The single character to write into the chunk.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    void WriteChar(char value);

    /// <summary>
    /// Function to write a string value to the chunk.
    /// </summary>
    /// <param name="value">The string value to write into the chunk.</param>
    /// <param name="encoding">[Optional] The encoding for the string.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    /// <remarks>
    /// <para>
    /// If the <paramref name="encoding"/> value is <b>null</b>, then the default encoding will be UTF-8.
    /// </para>
    /// </remarks>
    void WriteString(string value, Encoding? encoding = null);

    /// <summary>
    /// Function to write the contents of a <see cref="ReadOnlySpan{T}"/> to the chunk.
    /// </summary>
    /// <typeparam name="T">The type of data within the span. Must be an unmanged type.</typeparam>
    /// <param name="data">The span containing the data to write.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    void WriteSpan<T>(ReadOnlySpan<T> data) where T : unmanaged;

    /// <summary>
    /// Function to write the contents of an array to the chunk.
    /// </summary>
    /// <typeparam name="T">The type of data within the array. Must be an unmanged type.</typeparam>
    /// <param name="data">The array to write to the chunk.</param>
    /// <param name="index">[Optional] The index within the <paramref name="data"/> array to start at when writing to the chunk.</param>
    /// <param name="count">[Optional] The number of items to write.</param>
    /// <exception cref="ArgumentException">The index is less than 0, or greater than/equal to the size of the <paramref name="data"/> array.
    /// <para>-or-</para>
    /// <para>The <paramref name="count"/> minus the <paramref name="index"/> is too large for the <paramref name="data"/> array.</para>
    /// </exception>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    void WriteArray<T>(T[] data, int index = 0, int? count = null) where T : unmanaged;

    /// <summary>
    /// Function to write the contents of native memory pointed at by the <see cref="GorgonPtr{T}"/> value.
    /// </summary>
    /// <typeparam name="T">The type of data in native memory. Must be an unmanged type.</typeparam>
    /// <param name="pointer">The pointer that points to the native memory where the data is stored.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    void WritePointer<T>(GorgonPtr<T> pointer) where T : unmanaged;

    /// <summary>
    /// Function to write a single value to the chunk.
    /// </summary>
    /// <typeparam name="T">The type of the value. Must be an unmanged type.</typeparam>
    /// <param name="value">The value to write to the chunk.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    /// <remarks>
    /// <para>
    /// This overload is meant to be used with large unmanaged value types (typically larger than 16 bytes).
    /// </para>
    /// </remarks>
    void WriteValue<T>(ref readonly T value) where T : unmanaged;

    /// <summary>
    /// Function to write a single value to the chunk.
    /// </summary>
    /// <typeparam name="T">The type of the value. Must be an unmanged type.</typeparam>
    /// <param name="value">The value to write to the chunk.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    /// <remarks>
    /// <para>
    /// This overload is meant to be used with small unmanaged value types (typically small than or equal to 16 bytes).
    /// </para>
    /// </remarks>
    void WriteValue<T>(T value) where T : unmanaged;

    /// <summary>
    /// Function to serialize object data to the underlying file stream.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="objectToSerialize">The object that should be serialized.</param>
    /// <param name="serializeCallback">The callback used to perform the serialization into the stream.</param>
    /// <exception cref="IOException">Thrown if the method is called while the <paramref name="serializeCallback"/> is executing.</exception>
    /// <remarks>
    /// <para>
    /// This method is used to serialize objects that have their own serialization mechanisms (e.g. images, compression, etc...).
    /// </para>
    /// <para>
    /// The <paramref name="serializeCallback"/> method will pass the <paramref name="objectToSerialize"/> and a <see cref="Stream"/> object to the callback method. The application should then perform any 
    /// serialization for the <paramref name="objectToSerialize"/>. 
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// Under no circumstances should the callback call any of the <see cref="IGorgonChunkWriter"/> methods. Doing so will throw an exception that may end in file corruption.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// For example:
    /// <code language="csharp">
    /// <![CDATA[
    /// // MyObjectWithASaveMethod has a Save method exposed that takes a stream.
    /// 
    /// void myCallback(MyObjectWithASaveMethod obj, Stream stream) => obj.Save(stream);
    /// 
    /// writer.Serialize<MyObjectWithASaveMethod>(myObj, myCallback);
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    void Serialize<T>(T objectToSerialize, Action<T, Stream> serializeCallback);

    /// <summary>
    /// Function to finalize and close the chunk.
    /// </summary>
    /// <exception cref="IOException">Thrown if the method is called while the <see cref="Serialize{T}"/> method is executing.</exception>
    void Close();
}
