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
// Created: January 30, 2024 3:25:14 PM
//

using System.Buffers;
using System.Globalization;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.Properties;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;

namespace Gorgon.IO;

/// <summary>
/// Extension methods for IO operations and IO related string formatting.
/// </summary>
public static class GorgonIOExtensions
{
    // The system directory path separator.
    private static readonly string _directoryPathSeparator = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
    // The system alternate path separator.
    private static readonly string _altPathSeparator = Path.AltDirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
    // Illegal path characters.
    private static readonly char[] _illegalPathChars = Path.GetInvalidPathChars();
    // Illegal file name characters.
    private static readonly char[] _illegalFileChars = Path.GetInvalidFileNameChars();

    /// <summary>
    /// Function to write the contents of the memory pointed at by a <see cref="GorgonPtr{T}"/> to a stream.
    /// </summary>
    /// <param name="writer">The binary writer that will write the data to the stream.</param>
    /// <param name="pointer">The pointer containing the data to write in bytes.</param>
    /// <param name="size">The size, in bytes, of data to write.</param>
    private static void WritePtr(BinaryWriter writer, GorgonPtr<byte> pointer, int size)
    {
        int offset = 0;

        while (size > 0)
        {
            if (size >= sizeof(long))
            {
                ref long ptrValue = ref pointer.AsRef<long>(offset);
                writer.Write(ptrValue);

                size -= sizeof(long);
                offset += sizeof(long);
                continue;
            }

            if (size >= sizeof(int))
            {
                ref int ptrValue = ref pointer.AsRef<int>(offset);
                writer.Write(ptrValue);

                size -= sizeof(int);
                offset += sizeof(int);
                continue;
            }

            if (size >= sizeof(short))
            {
                ref short ptrValue = ref pointer.AsRef<short>(offset);
                writer.Write(ptrValue);

                size -= sizeof(short);
                offset += sizeof(short);
                continue;
            }

            ref byte ptrValueByte = ref pointer.AsRef<byte>(offset);
            writer.Write(ptrValueByte);
            --size;
            ++offset;
        }
    }

    /// <summary>
    /// Function to read data from a stream into a <see cref="GorgonPtr{T}"/> pointer.
    /// </summary>
    /// <param name="reader">The binary reader that will read the stream data.</param>
    /// <param name="pointer">The byte pointer that will receive the data.</param>
    /// <param name="size">The size, in bytes, of the data to read.</param>
    private static void ReadPtr(this BinaryReader reader, GorgonPtr<byte> pointer, int size)
    {
        int offset = 0;

        while (size > 0)
        {
            if (size >= sizeof(long))
            {
                ref long ptrValue = ref pointer.AsRef<long>(offset);
                ptrValue = reader.ReadInt64();

                size -= sizeof(long);
                offset += sizeof(long);
                continue;
            }

            if (size >= sizeof(int))
            {
                ref int ptrValue = ref pointer.AsRef<int>(offset);
                ptrValue = reader.ReadInt32();

                size -= sizeof(int);
                offset += sizeof(int);
                continue;
            }

            if (size >= sizeof(short))
            {
                ref short ptrValue = ref pointer.AsRef<short>(offset);
                ptrValue = reader.ReadInt16();

                size -= sizeof(short);
                offset += sizeof(short);
                continue;
            }

            ref byte ptrValueByte = ref pointer.AsRef<byte>(offset);
            ptrValueByte = reader.ReadByte();
            --size;
            ++offset;
        }
    }

    /// <summary>
    /// Function to copy the contents of this stream into another stream, up to a specified byte count.
    /// </summary>
    /// <param name="stream">The source stream that will be copied from.</param>
    /// <param name="destination">The stream that will receive the copy of the data.</param>
    /// <param name="count">The number of bytes to copy.</param>
    /// <param name="bufferSize">[Optional] The size of the temporary buffer used to buffer the data between streams.</param>
    /// <returns>The number of bytes copied, or 0 if no data was copied or at the end of a stream.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="stream"/> is write-only.
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="destination"/> is read-only.</para>
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="bufferSize"/> is less than 1.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is an extension of the <see cref="Stream.CopyTo(Stream,int)"/> method. But unlike that method, it will copy up to the number of bytes specified by <paramref name="count"/>. 
    /// </para>
    /// <para>
    /// The <paramref name="bufferSize"/> is used to copy data in blocks, rather than attempt to copy byte-by-byte. This may improve performance significantly. 
    /// </para>
    /// </remarks>
    public static int CopyToStream(this Stream stream, Stream destination, int count, int bufferSize = 131072)
    {
        if (stream.Length <= stream.Position)
        {
            return 0;
        }

        byte[] buffer = bufferSize > 0 ? ArrayPool<byte>.Shared.Rent(bufferSize) : throw new ArgumentException(Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL, nameof(bufferSize));

        try
        {
            return count < 1 ? 0 : CopyToStream(stream, destination, count, buffer);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, true);
        }
    }

    /// <summary>
    /// Function to asynchronously copy the contents of this stream into another stream, up to a specified byte count.
    /// </summary>
    /// <param name="stream">The source stream that will be copied from.</param>
    /// <param name="destination">The stream that will receive the copy of the data.</param>
    /// <param name="count">The number of bytes to copy.</param>
    /// <param name="bufferSize">[Optional] The size of the temporary buffer used to buffer the data between streams.</param>
    /// <param name="cancelToken">[Optional] The token used to cancel the operation.</param>
    /// <returns>The number of bytes copied, or 0 if no data was copied or at the end of a stream.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="stream"/> is write-only.
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="destination"/> is read-only.</para>
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="bufferSize"/> is less than 1.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is an extension of the <see cref="Stream.CopyTo(Stream,int)"/> method. But unlike that method, it will copy up to the number of bytes specified by <paramref name="count"/>. 
    /// </para>
    /// <para>
    /// The <paramref name="bufferSize"/> is used to copy data in blocks, rather than attempt to copy byte-by-byte. This may improve performance significantly.
    /// </para>
    /// </remarks>
    public static async Task<int> CopyToStreamAsync(this Stream stream, Stream destination, int count, int bufferSize = 131072, CancellationToken? cancelToken = null)
    {
        if (stream.Length <= stream.Position)
        {
            return 0;
        }

        byte[] buffer = bufferSize > 0 ? ArrayPool<byte>.Shared.Rent(bufferSize) : throw new ArgumentException(Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL, nameof(bufferSize));

        ArgumentEmptyException.ThrowIfNullOrEmpty(buffer, nameof(buffer));

        try
        {
            if (count < 1)
            {
                return 0;
            }

            int result = await CopyToStreamAsync(stream, destination, count, buffer, cancelToken);

            return result;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, true);
        }
    }

    /// <summary>
    /// Function to copy the contents of this stream into another stream, up to a specified byte count.
    /// </summary>
    /// <param name="stream">The source stream that will be copied from.</param>
    /// <param name="destination">The stream that will receive the copy of the data.</param>
    /// <param name="count">The number of bytes to copy.</param>
    /// <param name="buffer">The buffer to use for reading and writing the chunks of the file.</param>
    /// <returns>The number of bytes copied, or 0 if no data was copied or at the end of a stream.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="buffer"/> size is less than 1 byte.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="stream"/> is write-only.
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="destination"/> is read-only.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is an extension of the <see cref="Stream.CopyTo(Stream,int)"/> method. But unlike that method, it will copy up to the number of bytes specified by <paramref name="count"/>. 
    /// </para>
    /// <para>
    /// The <paramref name="buffer"/> is used to copy data in blocks, rather than attempt to copy byte-by-byte. This may improve performance significantly. 
    /// </para>
    /// </remarks>
    public static int CopyToStream(this Stream stream, Stream destination, int count, byte[] buffer)
    {
        if (!stream.CanRead)
        {
            throw new ArgumentException(Resources.GOR_ERR_STREAM_IS_WRITEONLY, nameof(stream));
        }

        if (!destination.CanWrite)
        {
            throw new ArgumentException(Resources.GOR_ERR_STREAM_IS_READONLY, nameof(destination));
        }

        ArgumentEmptyException.ThrowIfNullOrEmpty(buffer, nameof(buffer));

        if (stream.Length <= stream.Position)
        {
            return 0;
        }

        if (count < 1)
        {
            return 0;
        }

        int result = 0;
        int bytesRead;
        Span<byte> bufferSpan = buffer.AsSpan(0, buffer.Length.Min(count));

        while ((count > 0) && ((bytesRead = stream.Read(bufferSpan)) != 0))
        {
            destination.Write(bufferSpan[..bytesRead]);
            result += bytesRead;
            count -= bytesRead;
        }

        return result;
    }

    /// <summary>
    /// Function to copy the contents of this stream into another stream, up to a specified byte count.
    /// </summary>
    /// <param name="stream">The source stream that will be copied from.</param>
    /// <param name="destination">The stream that will receive the copy of the data.</param>
    /// <param name="count">The number of bytes to copy.</param>
    /// <param name="buffer">The buffer to use for reading and writing the chunks of the file.</param>
    /// <param name="cancelToken">[Optional] The token used to cancel the operation.</param>
    /// <returns>The number of bytes copied, or 0 if no data was copied or at the end of a stream.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="buffer"/> size is less than 1 byte.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="stream"/> is write-only.
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="destination"/> is read-only.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is an extension of the <see cref="Stream.CopyTo(Stream,int)"/> method. But unlike that method, it will copy up to the number of bytes specified by <paramref name="count"/>. 
    /// </para>
    /// <para>
    /// The <paramref name="buffer"/> is used to copy data in blocks, rather than attempt to copy byte-by-byte. This may improve performance significantly.
    /// </para>
    /// </remarks>
    public static async Task<int> CopyToStreamAsync(this Stream stream, Stream destination, int count, byte[] buffer, CancellationToken? cancelToken = null)
    {
        if (!stream.CanRead)
        {
            throw new ArgumentException(Resources.GOR_ERR_STREAM_IS_WRITEONLY, nameof(stream));
        }

        if (!destination.CanWrite)
        {
            throw new ArgumentException(Resources.GOR_ERR_STREAM_IS_READONLY, nameof(destination));
        }

        ArgumentEmptyException.ThrowIfNullOrEmpty(buffer, nameof(buffer));

        if (stream.Length <= stream.Position)
        {
            return 0;
        }

        if (count < 1)
        {
            return 0;
        }

        cancelToken ??= CancellationToken.None;

        int result = 0;
        int bytesRead;
        Memory<byte> bufferMemory = buffer.AsMemory(0, buffer.Length.Min(count));

        while ((count > 0) && ((bytesRead = await stream.ReadAsync(bufferMemory, cancelToken.Value).ConfigureAwait(false)) != 0))
        {
            if (cancelToken.Value.IsCancellationRequested)
            {
                return result;
            }

            await destination.WriteAsync(bufferMemory[..bytesRead], cancelToken.Value).ConfigureAwait(false);
            result += bytesRead;
            count -= bytesRead;
        }

        return result;
    }

    /// <summary>
    /// Function to write the data in a <see cref="GorgonPtr{T}"/> to a stream with a <see cref="BinaryWriter"/>.
    /// </summary>
    /// <typeparam name="T">The type of data in the pointer. Must be an unmanaged value type.</typeparam>
    /// <param name="writer">The binary writer used to write into the stream.</param>
    /// <param name="pointer">The pointer to the memory that will be written into the stream.</param>
    /// <exception cref="IOException">Thrown when the underlying stream is read only.</exception>
    /// <remarks>
    /// <para>
    /// This adds a write method to the <see cref="BinaryWriter"/> object that will write the contents of a <see cref="GorgonPtr{T}"/> into the stream. This allows direct dumping of raw memory into any kind 
    /// of stream (e.g. a file).
    /// </para>
    /// <para>
    /// If the pointer is <see cref="GorgonPtr{T}.NullPtr"/>, or has no <see cref="GorgonPtr{T}.Length"/> then this method will do nothing.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
    /// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
    /// </para>
    /// <para>
    /// Value types with marshalling attributes (<see cref="MarshalAsAttribute"/>) are <i>not</i> supported and will not be read correctly.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// For example:
    /// <code language="csharp">
    /// <![CDATA[
    /// byte[] data = Encoding.ASCII.GetBytes("Test a string that we can write to a stream.");
    /// 
    /// using MemoryStream stream = new();
    /// using GorgonNativeBuffer<byte> srcBuffer = data.PinAsNativeBuffer<byte>();
    /// using BinaryWriter writer = new (stream);
    /// 
    /// // This writes the contents of the byte array into the stream.
    /// writer.Write<byte>(srcBuffer);
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteFromPointer<T>(this BinaryWriter writer, GorgonPtr<T> pointer)
        where T : unmanaged
    {
        if ((pointer == GorgonPtr<T>.NullPtr) || (pointer.Length == 0))
        {
            return;
        }

        if (!writer.BaseStream.CanWrite)
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_READONLY);
        }

#warning This is not here yet, we need to implement it.
        GorgonPtr<byte> bytePtr = GorgonPtr<byte>.NullPtr;
        //var bytePtr = GorgonPtr<T>.ToBytePointer(pointer);
        WritePtr(writer, bytePtr, bytePtr.Length);
    }

    /// <summary>
    /// Function to read the data from a stream with a <see cref="BinaryReader"/> into the memory pointed at by a <see cref="GorgonPtr{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of data in the pointer. Must be an unmanaged value type.</typeparam>
    /// <param name="reader">The binary reader used to reader from the stream.</param>
    /// <param name="pointer">The pointer to the memory that will receive data from the stream.</param>
    /// <exception cref="IOException">Thrown when the underlying stream is write only.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the <paramref name="pointer"/> memory is too large for the remaining stream length.</exception>
    /// <exception cref="NullReferenceException">Thrown if the <paramref name="pointer"/> is <see cref="GorgonPtr{T}.NullPtr"/>.</exception>
    /// <remarks>
    /// <para>
    /// This adds a read method to the <see cref="BinaryReader"/> object that will reader the contents of a stream into the memory pointed at by a <see cref="GorgonPtr{T}"/>. This allows direct reading of 
    /// data into raw memory from any kind of stream (e.g. a file).
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
    /// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
    /// </para>
    /// <para>
    /// Value types with marshalling attributes (<see cref="MarshalAsAttribute"/>) are <i>not</i> supported and will not be read correctly.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// For example:
    /// <code language="csharp">
    /// <![CDATA[
    /// using GorgonNativeBuffer<byte> buffer = new(256);
    /// using BinaryReader reader = new(streamContainingData);
    /// 
    /// // This reads the contents of the stream into the buffer.
    /// reader.Read<byte>(buffer);
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadToPointer<T>(this BinaryReader reader, GorgonPtr<T> pointer)
        where T : unmanaged
    {
        if (pointer == GorgonPtr<T>.NullPtr)
        {
            throw new NullReferenceException();
        }

        if (!reader.BaseStream.CanRead)
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
        }

        if ((reader.BaseStream.Length - reader.BaseStream.Position) < pointer.SizeInBytes)
        {
            throw new EndOfStreamException(Resources.GOR_ERR_STREAM_EOS);
        }

#warning This is not here yet, we need to implement it.
        GorgonPtr<byte> bytePtr = GorgonPtr<byte>.NullPtr;
        //var bytePtr = GorgonPtr<T>.ToBytePointer(pointer);
        ReadPtr(reader, bytePtr, bytePtr.Length);
    }

    /// <summary>
    /// Function to read a generic value from the stream.
    /// </summary>
    /// <typeparam name="T">Type of value to read.  Must be an unmanaged value type.</typeparam>
    /// <param name="reader">The binary reader used to read the stream.</param>
    /// <param name="result">The value from the stream.</param>
    /// <returns>The value in the stream.</returns>
    /// <exception cref="IOException">Thrown if the underlying stream is write only.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the size of <typeparamref name="T"/> is larger than the available room in the stream.</exception>
    /// <remarks>
    /// <para>
    /// This method will read the data from the binary stream into a value of type <typeparamref name="T"/>, and return that value.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
    /// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
    /// </para>
    /// <para>
    /// Value types with marshalling attributes (<see cref="MarshalAsAttribute"/>) are <i>not</i> supported and will not be read correctly.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// Here is an example of how to use this method:
    /// <code language="csharp">
    /// <![CDATA[
    /// Point point;
    /// using MemoryStream stream = ... // stream containing data.
    /// using BinaryReader reader = new BinaryReader(stream);
    /// 
    /// reader.ReadValue<Point>(out point);
    /// 
    /// // or
    /// 
    /// point = reader.ReadValue<Point>();
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    public static void ReadValue<T>(this BinaryReader reader, out T result)
        where T : unmanaged
    {
        if (!reader.BaseStream.CanRead)
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
        }

        result = default;

        unsafe
        {
            int size = sizeof(T);

            if ((reader.BaseStream.Length - reader.BaseStream.Position) < size)
            {
                throw new EndOfStreamException(Resources.GOR_ERR_STREAM_EOS);
            }

            fixed (T* ptr = &result)
            {
                GorgonPtr<byte> pointer = new((byte*)ptr, size);
                ReadPtr(reader, pointer, size);
            }
        }
    }

    /// <summary>
    /// Function to read a generic value from the stream.
    /// </summary>
    /// <typeparam name="T">Type of value to read.  Must be an unmanaged value type.</typeparam>
    /// <param name="reader">The binary reader used to read the stream.</param>
    /// <returns>The value in the stream.</returns>
    /// <exception cref="IOException">Thrown if the underlying stream is write only.</exception>
    /// <exception cref="EndOfStreamException">Thrown if the size of <typeparamref name="T"/> is larger than the available room in the stream.</exception>
    /// <remarks>
    /// <para>
    /// This method will read the data from the binary stream into a value of type <typeparamref name="T"/>, and return that value.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
    /// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
    /// </para>
    /// <para>
    /// Value types with marshalling attributes (<see cref="MarshalAsAttribute"/>) are <i>not</i> supported and will not be read correctly.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// Here is an example of how to use this method:
    /// <code language="csharp">
    /// <![CDATA[
    /// Point point;
    /// using MemoryStream stream = ... // stream containing data.
    /// using BinaryReader reader = new BinaryReader(stream);
    /// 
    /// reader.ReadValue<Point>(out point);
    /// 
    /// // or
    /// 
    /// point = reader.ReadValue<Point>();
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    public static T ReadValue<T>(this BinaryReader reader)
        where T : unmanaged
    {
        ReadValue(reader, out T result);
        return result;
    }

    /// <summary>
    /// Function to write a generic value to the stream.
    /// </summary>
    /// <typeparam name="T">Type of value to write.  Must be an unmanaged value type.</typeparam>
    /// <param name="writer">The binary writer used to write to the stream.</param>
    /// <param name="value">Value to write to the stream.</param>
    /// <exception cref="IOException">Thrown if the underlying stream is read only.</exception>
    /// <remarks>
    /// <para>
    /// This method will write the data to the binary stream from the <paramref name="value"/> of type <typeparamref name="T"/>. The amount of data written will be dependant upon the size of 
    /// <typeparamref name="T"/>, and any packing rules applied.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
    /// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
    /// </para>
    /// <para>
    /// Value types with marshalling attributes (<see cref="MarshalAsAttribute"/>) are <i>not</i> supported and will not be read correctly.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// Here is an example of how to use this method:
    /// <code language="csharp">
    /// <![CDATA[
    /// Point point = new Point(1, 2);
    /// using MemoryStream stream = ... // stream containing data.
    /// using BinaryWriter writer = new BinaryReader(stream);
    /// 
    /// writer.WriteValue<Point>(in point);
    /// 
    /// // or for values smaller or equal to than 16 bytes.
    /// 
    /// writer.WriteValue<Point>(point);
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    /// <exception cref="IOException">Thrown when the stream is read-only.</exception>
    public static void WriteValue<T>(this BinaryWriter writer, ref readonly T value)
        where T : unmanaged
    {
        if (!writer.BaseStream.CanWrite)
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_READONLY);
        }

        unsafe
        {
            int size = sizeof(T);

            fixed (T* ptr = &value)
            {
                GorgonPtr<byte> pointer = new((byte*)ptr, size);
                WritePtr(writer, pointer, size);
            }
        }
    }

    /// <summary>
    /// Function to write a generic value to the stream.
    /// </summary>
    /// <typeparam name="T">Type of value to write.  Must be an unmanaged value type.</typeparam>
    /// <param name="writer">The binary writer used to write to the stream.</param>
    /// <param name="value">Value to write to the stream.</param>
    /// <exception cref="IOException">Thrown if the underlying stream is read only.</exception>
    /// <remarks>
    /// <para>
    /// This method will write the data to the binary stream from the <paramref name="value"/> of type <typeparamref name="T"/>. The amount of data written will be dependant upon the size of 
    /// <typeparamref name="T"/>, and any packing rules applied.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
    /// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
    /// </para>
    /// <para>
    /// Value types with marshalling attributes (<see cref="MarshalAsAttribute"/>) are <i>not</i> supported and will not be read correctly.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// Here is an example of how to use this method:
    /// <code language="csharp">
    /// <![CDATA[
    /// Point point = new Point(1, 2);
    /// using MemoryStream stream = ... // stream containing data.
    /// using BinaryWriter writer = new BinaryReader(stream);
    /// 
    /// writer.WriteValue<Point>(in point);
    /// 
    /// // or for values smaller or equal to than 16 bytes.
    /// 
    /// writer.WriteValue<Point>(point);
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    /// <exception cref="IOException">Thrown when the stream is read-only.</exception>
    public static void WriteValue<T>(this BinaryWriter writer, T value)
        where T : unmanaged => WriteValue(writer, in value);

    /// <summary>
    /// Function to write a range of generic values into a stream via a <see cref="BinaryWriter"/>.
    /// </summary>
    /// <typeparam name="T">Type of value to write.  Must be an unmanaged value type.</typeparam>
    /// <param name="writer">The binary writer that will copy the data into the stream.</param>
    /// <param name="values">Array of values to write.</param>
    /// <exception cref="IOException">Thrown when the stream is read-only.</exception>
    /// <remarks>
    /// <para>
    /// This will write data into the binary stream from the specified array of values of type <typeparamref name="T"/>. 
    /// </para>
    /// <para>
    /// The amount of data written will be dependant upon the size of type <typeparamref name="T"/> with packing rules on type <typeparamref name="T"/> will affect the size of the type.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
    /// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
    /// </para>
    /// <para>
    /// Value types with marshalling attributes (<see cref="MarshalAsAttribute"/>) are <i>not</i> supported and will not be read correctly.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// Here is an example of how to use this method:
    /// <code language="csharp">
    /// <![CDATA[
    /// int[] values = { 1, 2, 3, 4, 5 };
    /// using MemoryStream stream = new();
    /// using BinaryWriter writer = new(stream);
    /// 
    /// writer.WriteRange(values);
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    public static void WriteRange<T>(this BinaryWriter writer, ReadOnlySpan<T> values)
        where T : unmanaged
    {
        if (!writer.BaseStream.CanWrite)
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_READONLY);
        }

        if (values.IsEmpty)
        {
            return;
        }

        unsafe
        {
            int size = values.Length * sizeof(T);

            fixed (T* ptr = &values[0])
            {
                GorgonPtr<byte> pointer = new((byte*)ptr, size);
                WritePtr(writer, pointer, size);
            }
        }
    }

    /// <summary>
    /// Function to read a range of generic values from a stream via a <see cref="BinaryReader"/> and into a span.
    /// </summary>
    /// <typeparam name="T">Type of value to read. Must be an unmanaged value type.</typeparam>
    /// <param name="reader">The binary reader that will copy the data into the span from the stream.</param>
    /// <param name="values">The span buffer to read data into.</param>
    /// <exception cref="IOException">Thrown when the stream is read-only.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="values"/> is empty.</exception>
    /// <remarks>
    /// <para>
    /// This will read data from the binary stream into the specified span of type <typeparamref name="T"/>. 
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
    /// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
    /// </para>
    /// <para>
    /// Value types with marshalling attributes (<see cref="MarshalAsAttribute"/>) are <i>not</i> supported and will not be read correctly.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// Here is an example of how to use this method:
    /// <code language="csharp">
    /// <![CDATA[
    /// int[] values = { 1, 2, 3, 4, 5 };
    /// using MemoryStream stream = new(values);
    /// using BinaryReader reader = new(stream);
    /// 
    /// int[] valuesCopy = new[values.Length];
    /// 
    /// reader.ReadRange(valuesCopy);
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    public static void ReadRange<T>(this BinaryReader reader, Span<T> values)
        where T : unmanaged
    {
        if (!reader.BaseStream.CanRead)
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
        }

        if (values.IsEmpty)
        {
            throw new ArgumentEmptyException(nameof(values));
        }

        unsafe
        {
            int size = values.Length * sizeof(T);

            fixed (T* ptr = &values[0])
            {
                GorgonPtr<byte> pointer = new((byte*)ptr, size);
                ReadPtr(reader, pointer, size);
            }
        }
    }

    /// <summary>
    /// Function to write a string to a stream with the specified encoding.
    /// </summary>
    /// <param name="stream">The stream to write the string into.</param>
    /// <param name="value">The string to write.</param>
    /// <param name="encoding">[Optional] The encoding for the string.</param>
    /// <exception cref="IOException">Thrown when the <paramref name="stream"/> parameter is read-only.</exception>
    /// <returns>The number of bytes written to the stream.</returns>
    /// <remarks>
    /// <para>
    /// Gorgon stores its strings in a stream by prefixing the string data with the length of the string, in bytes.  This length is encoded as a series of 7-bit bytes.
    /// </para>
    /// <para>
    /// If the <paramref name="encoding"/> parameter is <b>null</b>, then UTF-8 encoding is used.
    /// </para>
    /// </remarks>
    public static int WriteString(this Stream stream, string value, Encoding? encoding = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            return 0;
        }

        if (!stream.CanWrite)
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_READONLY);
        }

        encoding ??= Encoding.UTF8;

        byte[] stringData = encoding.GetBytes(value);
        int size = stringData.Length;
        int result = size + 1;

        // Build the 7 bit encoded length.
        while (size >= 0x80)
        {
            stream.WriteByte((byte)((size | 0x80) & 0xFF));
            size >>= 7;
            result++;
        }

        stream.WriteByte((byte)size);
        stream.Write(stringData, 0, stringData.Length);

        return result;
    }

    /// <summary>
    /// Function to read a string from a stream with the specified encoding.
    /// </summary>
    /// <param name="stream">The stream to read the string from.</param>
    /// <param name="encoding">[Optional] The encoding for the string.</param>
    /// <returns>The string in the stream.</returns>
    /// <exception cref="IOException">Thrown when an attempt to read beyond the end of the <paramref name="stream"/> is made.</exception>
    /// <remarks>
    /// <para>
    /// Gorgon stores its strings in a stream by prefixing the string data with the length of the string.  This length is encoded as a series of 7-bit bytes.
    /// </para>
    /// <para>
    /// If the <paramref name="encoding"/> parameter is <b>null</b>, then UTF-8 encoding is used.
    /// </para>
    /// </remarks>
    public static string ReadString(this Stream stream, Encoding? encoding = null)
    {
        int stringLength = 0;

        encoding ??= Encoding.UTF8;

        // String length is encoded in a 7 bit integer.
        // We have to get each byte and shift it until there are no more high bits set, or the counter becomes larger than 32 bits.
        int counter = 0;
        while (true)
        {
            int value = stream.ReadByte();

            if (value == -1)
            {
                throw new IOException(Resources.GOR_ERR_STREAM_EOS);
            }

            stringLength |= (value & 0x7F) << counter;
            counter += 7;
            if (((value & 0x80) == 0) || (counter > 32))
            {
                break;
            }
        }

        if (stringLength == 0)
        {
            return string.Empty;
        }

        // Find the number of bytes required for 4096 characters.
        int maxByteCount = encoding.GetMaxByteCount(4096);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(maxByteCount);
        char[] charBuffer = ArrayPool<char>.Shared.Rent(4096);

        try
        {
            Decoder decoder = encoding.GetDecoder();
            StringBuilder? result = null;
            counter = 0;

            // Buffer the string in, just in case it's super long.
            while ((stream.Position < stream.Length) && (counter < stringLength))
            {
                // Fill the byte buffer.
                int bytesRead = stream.Read(buffer, 0, stringLength <= buffer.Length ? stringLength : buffer.Length);

                if (bytesRead == 0)
                {
                    throw new EndOfStreamException(Resources.GOR_ERR_STREAM_EOS);
                }

                // Get the characters.
                int charsRead = decoder.GetChars(buffer, 0, bytesRead, charBuffer, 0);

                // If we've already read the entire string, just dump it back out now.
                if ((counter == 0) && (bytesRead == stringLength))
                {
                    return new string(charBuffer, 0, charsRead);
                }

                // We'll need a bigger string. So allocate a string builder and use that.
                // Try to max out the string builder size by the length of our string, in characters.
                result ??= new StringBuilder(encoding.GetMaxCharCount(stringLength));

                result.Append(charBuffer, 0, charsRead);

                counter += bytesRead;
            }

            return result?.ToString() ?? string.Empty;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, true);
            ArrayPool<char>.Shared.Return(charBuffer, true);
        }
    }

    /// <summary>
    /// Function to format a filename with safe characters.
    /// </summary>
    /// <param name="path">The path containing the filename to evaluate.</param>
    /// <returns>A safe filename formatted with placeholder characters if invalid characters are found.</returns>
    /// <remarks>
    /// <para>
    /// This will replace any illegal filename characters with the underscore character.
    /// </para>
    /// <para>
    /// If <b>null</b> or <see cref="string.Empty"/> are passed to this method, then an empty string will be returned. If the path does not contain a 
    /// filename, then an empty string will be returned as well.
    /// </para>
    /// </remarks>
    public static string FormatFileName(this string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var output = new StringBuilder(path);

        output = _illegalPathChars.Aggregate(output, (current, illegalChar) => current.Replace(illegalChar, '_'));

        string fileName = Path.GetFileName(output.ToString());

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        output.Length = 0;
        output.Append(fileName);

        output = _illegalFileChars.Aggregate(output, (current, illegalChar) => current.Replace(illegalChar, '_'));

        return output.ToString();
    }

    /// <summary>
    /// Function to format a directory path with safe characters.
    /// </summary>
    /// <param name="path">The directory path to evaluate..</param>
    /// <param name="directorySeparator">Directory separator character to use.</param>
    /// <returns>A safe directory path formatted with placeholder characters if invalid characters are found. Directory separators will be replaced with the specified separator passed 
    /// to <paramref name="directorySeparator"/>.</returns> 
    /// <remarks>
    /// <para>
    /// This will replace any illegal path characters with the underscore character. Any doubled up directory separators (e.g. // or \\) will be replaced with the directory separator 
    /// passed to <paramref name="directorySeparator"/>.
    /// </para>
    /// <para>
    /// If <b>null</b> or <see cref="string.Empty"/> are passed to this method, then an empty string will be returned. If the path contains only a filename, 
    /// that string will be formatted as though it were a directory path.
    /// </para>
    /// </remarks>
    public static string FormatDirectory(this string? path, char directorySeparator)
    {
        string directorySep = _directoryPathSeparator;
        string doubleSeparator = new(new[] { directorySeparator, directorySeparator });

        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        if ((char.IsWhiteSpace(directorySeparator)) || (_illegalPathChars.Contains(directorySeparator)))
        {
            directorySeparator = Path.DirectorySeparatorChar;
        }
        string? pathRoot = Path.GetPathRoot(path) ?? string.Empty;
        var output = new StringBuilder(path[pathRoot.Length..]);

        output = _illegalPathChars.Concat(_illegalFileChars)
                                  .Distinct()
                                  .Where(c => c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar)
                                  .Aggregate(output, (current, illegalChar) => current.Replace(illegalChar, '_'));

        output.Insert(0, pathRoot);

        if (directorySeparator != Path.AltDirectorySeparatorChar)
        {
            output = output.Replace(Path.AltDirectorySeparatorChar, directorySeparator);
        }
        else
        {
            output = output.Replace(Path.DirectorySeparatorChar, directorySeparator);
            directorySep = _altPathSeparator;
        }

        if (output[^1] != directorySeparator)
        {
            output.Append(directorySeparator);
        }

        // Remove doubled up separators.
        int i = 0;

        while (i < output.Length)
        {
            if (output[i] != directorySeparator)
            {
                ++i;
                continue;
            }

            if (i == output.Length - 1)
            {
                break;
            }

            if (output[i + 1] == directorySeparator)
            {
                output.Remove(i, 1);
                continue;
            }

            ++i;
        }

        return output.ToString();
    }

    /// <summary>
    /// Function to format a specific piece of a path.
    /// </summary>
    /// <param name="path">The path part to evaluate and repair.</param>
    /// <returns>A safe path part with placeholder characters if invalid characters are found.</returns>
    /// <remarks>
    /// <para>
    /// This method removes illegal symbols from the <paramref name="path"/> and replaces them with an underscore character. It will not respect path separators and will consider those characters 
    /// as illegal if provided in the <paramref name="path"/> parameter.
    /// </para>
    /// </remarks>
    public static string FormatPathPart(this string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var output = new StringBuilder(path);

        output = _illegalPathChars.Concat(_illegalFileChars)
                                  .Distinct()
                                  .Aggregate(output, (current, illegalChar) => current.Replace(illegalChar, '_'));

        return output.ToString();
    }

    /// <summary>
    /// Function to split a path into component parts.
    /// </summary>
    /// <param name="path">The path to split.</param>
    /// <param name="directorySeparator">The separator to split the path on.</param>
    /// <returns>An array containing the parts of the path, or an empty array if the path is <b>null</b> or empty.</returns>
    /// <remarks>
    /// This will take a path a split it into individual pieces for evaluation. The <paramref name="directorySeparator"/> parameter will be the character 
    /// used to determine how to split the path. For example:
    /// <code language="csharp">
    ///		string myPath = @"C:\Windows\System32\ddraw.dll";
    ///		string[] parts = myPath.GetPathParts(Path.DirectorySeparatorChar);
    ///		
    ///		foreach(string part in parts)
    ///     {
    ///			Console.WriteLine(part);
    ///		}
    /// 
    ///		/* Output should be:
    ///		 * C:
    ///		 * Windows
    ///		 * System32
    ///		 * ddraw.dll
    ///		 */
    /// </code>
    /// </remarks>
    public static string[] GetPathParts(this string? path, char directorySeparator)
    {
        path = path.FormatPath(directorySeparator);

        return path.Split(new[]
                          {
                              directorySeparator
                          },
                          StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Function to format a path with safe characters.
    /// </summary>
    /// <param name="path">Path to the file or folder to format.</param>
    /// <param name="directorySeparator">Directory separator character to use.</param>
    /// <returns>A safe path formatted with placeholder characters if invalid characters are found.</returns>
    /// <remarks>
    /// <para>
    /// If the path contains directories, they will be formatted according to the formatting applied by <see cref="FormatDirectory"/>, and if the path contains a filename, it will be 
    /// formatted according to the formatting applied by the <see cref="FormatFileName"/> method.
    /// </para>
    /// <para>
    /// If the last character in <paramref name="path"/> is not the same as the <paramref name="directorySeparator"/> parameter, then that last part of the path will be treated as a file. 
    /// </para>
    /// <para>
    /// If no directories are present in the path, then the see <paramref name="directorySeparator"/> is ignored.
    /// </para>
    /// </remarks>
    public static string FormatPath(this string? path, char directorySeparator)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        // Filter out bad characters.
        StringBuilder output = new(path);
        output = _illegalPathChars.Aggregate(output, (current, illegalChar) => current.Replace(illegalChar, '_'));

        StringBuilder filePath = new(FormatDirectory(Path.GetDirectoryName(output.ToString()), directorySeparator));

        path = output.ToString();

        // Try to get the filename portion.
        output.Length = 0;
        output.Append(Path.GetFileName(path));

        if (output.Length == 0)
        {
            return filePath.ToString();
        }

        output = _illegalFileChars.Aggregate(output, (current, illegalChar) => current.Replace(illegalChar, '_'));

        filePath.Append(output);

        return filePath.ToString();
    }

    /// <summary>
    /// Function to return the chunk ID based on the name of the chunk passed to this method.
    /// </summary>
    /// <param name="chunkName">The name of the chunk.</param>
    /// <returns>A <see cref="ulong"/> value representing the chunk ID of the name.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="chunkName"/> is empty.</exception>
    /// <remarks>
    /// <para>
    /// This method is used to generate a new chunk ID for the <conceptualLink target="7b81343e-e2fc-4f0f-926a-d9193ae481fe">Gorgon chunked file format</conceptualLink>. It converts the characters in the string to their ASCII byte 
    /// equivalents, and then builds a <see cref="ulong"/> value from those bytes.
    /// </para>
    /// <para>
    /// Since the size of an <see cref="ulong"/> is 8 bytes, then the string should contain 8 characters. If it does not, then the ID will be padded with 0's on the right to take up the remaining 
    /// bytes. If the string is larger than 8 characters, then it will be truncated to the 8 character limit from left to right (e.g. STRINGVALUE will be processed as STRINGVA).
    /// </para>
    /// <para>
    /// The format of the long value is not endian specific and is encoded in the same order as the characters in the string.  For example, encoding the string 'TESTVALU' produces:<br/>
    /// <list type="table">
    /// <listheader>
    ///		<term>Byte</term>
    ///		<term>1</term>
    ///		<term>2</term>
    ///		<term>3</term>
    ///		<term>4</term>
    ///		<term>5</term>
    ///		<term>6</term>
    ///		<term>7</term>
    ///		<term>8</term>
    /// </listheader>
    ///		<item>
    ///			<description>Character</description>
    ///			<description>'T' (0x54)</description>
    ///			<description>'E' (0x45)</description>
    ///			<description>'S' (0x53)</description>
    ///			<description>'T' (0x54)</description>
    ///			<description>'V' (0x56)</description>
    ///			<description>'A' (0x41)</description>
    ///			<description>'L' (0x4C)</description>
    ///			<description>'U' (0x55)</description>
    ///		</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static ulong ChunkID(this string chunkName)
    {
        if (chunkName.Length == 0)
        {
            throw new ArgumentEmptyException(nameof(chunkName));
        }

        if (chunkName.Length > 8)
        {
            chunkName = chunkName[..8];
        }
        else if (chunkName.Length < 8)
        {
            chunkName = chunkName.PadRight(8, '\0');
        }

        return (((ulong)((byte)chunkName[7]) << 56)
               | ((ulong)((byte)chunkName[6]) << 48)
               | ((ulong)((byte)chunkName[5]) << 40)
               | ((ulong)((byte)chunkName[4]) << 32)
               | ((ulong)((byte)chunkName[3]) << 24)
               | ((ulong)((byte)chunkName[2]) << 16)
               | ((ulong)((byte)chunkName[1]) << 8)
               | (byte)chunkName[0]);
    }
}
