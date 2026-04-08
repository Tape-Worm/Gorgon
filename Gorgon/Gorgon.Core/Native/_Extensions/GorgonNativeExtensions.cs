// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: January 22, 2024 4:19:42 PM
//

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Math;
using Gorgon.Properties;

namespace Gorgon.Native;

/// <summary>
/// Extension methods for native memory functionality.
/// </summary>
public static class GorgonNativeExtensions
{
    /// <summary>
    /// Function to write the contents of the memory pointed at by a <see cref="GorgonPtr{T}"/> to a stream.
    /// </summary>
    /// <param name="writer">The binary writer that will write the data to the stream.</param>
    /// <param name="pointer">The pointer containing the data to write in bytes.</param>
    /// <param name="size">The size, in bytes, of data to write.</param>
    private static void WritePtr(BinaryWriter writer, GorgonPtr<byte> pointer, long size)
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

    extension(BinaryReader reader)
    {
        /// <summary>
        /// Function to read data from a stream into a <see cref="GorgonPtr{T}"/> pointer.
        /// </summary>
        /// <param name="pointer">The byte pointer that will receive the data.</param>
        /// <param name="size">The size, in bytes, of the data to read.</param>
        private void ReadPtr(GorgonPtr<byte> pointer, long size)
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
        /// Function to read the data from a stream with a <see cref="BinaryReader"/> into the memory pointed at by a <see cref="GorgonPtr{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of data in the pointer. Must be an unmanaged value type.</typeparam>
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
        /// <inheritdoc cref="GorgonNativeBuffer{T}.AsRef{TTo}(long)" path="/remarks/para/note[@type='information']"/>
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
        public void ReadToPointer<T>(GorgonPtr<T> pointer)
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

            GorgonPtr<byte> bytePtr = (GorgonPtr<byte>)pointer;
            ReadPtr(reader, bytePtr, bytePtr.Length);
        }
    }

    extension(BinaryWriter writer)
    {
        /// <summary>
        /// Function to write the data in a <see cref="GorgonPtr{T}"/> to a stream with a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <typeparam name="T">The type of data in the pointer. Must be an unmanaged value type.</typeparam>
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
        /// <inheritdoc cref="GorgonNativeBuffer{T}.AsRef{TTo}(long)" path="/remarks/para/note[@type='information']"/>
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
        public void WriteFromPointer<T>(GorgonPtr<T> pointer)
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

            GorgonPtr<byte> bytePtr = (GorgonPtr<byte>)pointer;
            WritePtr(writer, bytePtr, bytePtr.Length);
        }
    }

    extension(Stream stream)
    {
        /// <summary>
        /// Function to copy the contents of a stream into the memory pointed at by a <see cref="GorgonPtr{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of data stored in the memory pointed at by the pointer. Must be an unmanaged value type.</typeparam>
        /// <param name="ptr">The pointer that points to the block of memory to write into the stream.</param>
        /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown when the stream is read only.</exception>
        /// <exception cref="EndOfStreamException">Thrown when the stream is at its end.</exception>"
        public void Read<T>(GorgonPtr<T> ptr)
            where T : unmanaged
        {
            if (ptr == GorgonPtr<T>.NullPtr)
            {
                throw new NullReferenceException();
            }

            if (!stream.CanRead)
            {
                throw new IOException(Resources.GOR_ERR_STREAM_IS_READONLY);
            }

            long byteSize = ptr.SizeInBytes;

            // Ensure the stream has enough room to read the data.
            if ((stream.Length - stream.Position) < byteSize)
            {
                throw new EndOfStreamException();
            }

            unsafe
            {
                byte* src = (byte*)ptr;
                int blockSize = (int)(byteSize.Min(int.MaxValue));

                while (byteSize > 0)
                {
                    int read = stream.Read(new Span<byte>(src, blockSize));

                    if (read == 0)
                    {
                        break;
                    }

                    src += read;
                    byteSize -= read;

                    if (byteSize < int.MaxValue)
                    {
                        blockSize = (int)byteSize;
                    }
                }
            }
        }

        /// <summary>
        /// Function to copy the contents of a stream into the memory pointed at by a <see cref="GorgonNativeBuffer{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of data stored in the memory pointed at by the pointer. Must be an unmanaged value type.</typeparam>
        /// <param name="buffer">The buffer that will receive the data.</param>
        /// <param name="index">The index in the buffer where the data will be stored.</param>
        /// <param name="count">[Optional] The number of items to read.</param>
        /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown when the stream is read only.</exception>
        /// <exception cref="EndOfStreamException">Thrown when the stream is at its end.</exception>"
        /// <exception cref="ArgumentException"><para>Thrown if the <paramref name="index"/> is larger than the <see cref="GorgonNativeBuffer{T}.Length"/> of the <see cref="GorgonNativeBuffer{T}"/>.</para>
            /// <para>Thrown if the <paramref name="index"/> is less than 0, or the <paramref name="count"/> parameter is less than 1.</para>
            /// <para>Thrown if the <paramref name="index"/> plus the <paramref name="count"/> is larger than the buffer or stream.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// When the <paramref name="count"/> parameter is specified, the buffer will be limited to that number of items. If the <paramref name="count"/> parameter is omitted, then the count will be sized to 
        /// the <see cref="GorgonNativeBuffer{T}.Length"/>.
        /// </para>
        /// </remarks>
        public void Read<T>(GorgonNativeBuffer<T> buffer, long index, long? count = null)
            where T : unmanaged
        {
            if (!stream.CanRead)
            {
                throw new IOException(Resources.GOR_ERR_STREAM_IS_READONLY);
            }

            if (index < 0)
            {
                throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL), nameof(index));
            }

            long size = count ?? (buffer.Length - index);

            if (size < 1)
            {
                throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL), nameof(count));
            }

            if (index + size > buffer.Length)
            {
                throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, index, size));
            }

            long byteSize = size * buffer.TypeSize;

            // Ensure the stream has enough room to read the data.
            if ((stream.Length - stream.Position) < byteSize)
            {
                throw new EndOfStreamException();
            }

            unsafe
            {
                byte* src = (byte*)Unsafe.AsPointer(ref buffer[index]);
                int blockSize = (int)(byteSize.Min(int.MaxValue));

                while (byteSize > 0)
                {
                    int read = stream.Read(new Span<byte>(src, blockSize));

                    if (read == 0)
                    {
                        break;
                    }

                    src += read;
                    byteSize -= read;

                    if (byteSize < int.MaxValue)
                    {
                        blockSize = (int)byteSize;
                    }
                }
            }
        }

        /// <summary>
        /// Function to copy the contents of the memory pointed at by a pointer into a stream.
        /// </summary>
        /// <typeparam name="T">The type of data stored in the memory pointed at by the pointer. Must be an unmanaged value type.</typeparam>
        /// <param name="ptr">The pointer that points to the block of memory to write into the stream.</param>
        /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown when the stream is read only.</exception>
        public void Write<T>(GorgonPtr<T> ptr)
            where T : unmanaged
        {
            if (ptr == GorgonPtr<T>.NullPtr)
            {
                throw new NullReferenceException();
            }

            if (!stream.CanWrite)
            {
                throw new IOException(Resources.GOR_ERR_STREAM_IS_READONLY);
            }

            long byteSize = ptr.SizeInBytes;

            if (byteSize == 0)
            {
                return;
            }

            unsafe
            {
                byte* src = (byte*)ptr;
                int blockSize = (int)(byteSize.Min(int.MaxValue));

                while (byteSize > 0)
                {
                    stream.Write(new ReadOnlySpan<byte>(src, blockSize));

                    src += blockSize;
                    byteSize -= blockSize;

                    if (byteSize < int.MaxValue)
                    {
                        blockSize = (int)byteSize;
                    }
                }
            }
        }

        /// <summary>
        /// Function to copy the contents of the memory pointed at by a pointer into a stream.
        /// </summary>
        /// <typeparam name="T">The type of data stored in the memory pointed at by the pointer. Must be an unmanaged value type.</typeparam>
        /// <param name="buffer">The pointer that points to the block of memory to write into the stream.</param>
        /// <param name="index">The index in the buffer where the data will be copied from.</param>
        /// <param name="count">[Optional] The number of items to read.</param>
        /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown when the stream is read only.</exception>
        /// <exception cref="ArgumentException"><para>Thrown if the <paramref name="index"/> is larger than the <see cref="GorgonNativeBuffer{T}.Length"/> of the <see cref="GorgonNativeBuffer{T}"/>.</para>
            /// <para>Thrown if the <paramref name="index"/> is less than 0, or the <paramref name="count"/> parameter is less than 1.</para>
            /// <para>Thrown if the <paramref name="index"/> plus the <paramref name="count"/> is larger than the buffer or stream.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// When the <paramref name="count"/> parameter is specified, the buffer will be limited to that number of items. If the <paramref name="count"/> parameter is omitted, then the count will be sized to 
        /// the <see cref="GorgonNativeBuffer{T}.Length"/>.
        /// </para>
        /// </remarks>
        public void Write<T>(GorgonNativeBuffer<T> buffer, long index, long? count = null)
            where T : unmanaged
        {
            if (!stream.CanWrite)
            {
                throw new IOException(Resources.GOR_ERR_STREAM_IS_READONLY);
            }

            if (index < 0)
            {
                throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL), nameof(index));
            }

            long size = count ?? (buffer.Length - index);

            if (size < 1)
            {
                throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL), nameof(count));
            }

            if (index + size > buffer.Length)
            {
                throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, index, size));
            }

            long byteSize = size * buffer.TypeSize;

            unsafe
            {
                byte* src = (byte*)Unsafe.AsPointer(ref buffer[index]);
                int blockSize = (int)(byteSize.Min(int.MaxValue));

                while (byteSize > 0)
                {
                    stream.Write(new Span<byte>(src, blockSize));

                    byteSize -= blockSize;
                    src += blockSize;

                    if (byteSize < int.MaxValue)
                    {
                        blockSize = (int)byteSize;
                    }
                }
            }
        }
    }

    /// <typeparam name="T">The type of data in the array. Must be an unmanaged value type.</typeparam>
    extension<T>(T[] array) where T : unmanaged
    {
        /// <summary>
        /// Function to pin an array and access its contents natively.
        /// </summary>
        /// <param name="index">[Optional] The starting index in the array to pin.</param>
        /// <param name="count">[Optional] The number of items in the array to pin.</param>
        /// <returns>A new <see cref="GorgonNativeBuffer{T}"/> containing the pinned contents of the array.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> or <paramref name="count"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> + <paramref name="count"/> are equal to or greater than the length of array.</exception>
        /// <remarks>
        /// <para>
        /// This method pins the supplied array and returns a new <see cref="GorgonNativeBuffer{T}"/> containing the pinned data.
        /// </para>
        /// <para>
        /// If the <paramref name="index"/> is not supplied, then the beginning of the array is used as the start of the buffer, and if the <paramref name="count"/> parameter is not 
        /// supplied, then the length of the array (minus the <paramref name="index"/>) is used. 
        /// </para>
        /// <para>
        /// When the <paramref name="count"/> parameter is specified, the buffer will be limited to that number of items. If the <paramref name="count"/> parameter is omitted, then the buffer will be sized to 
        /// the length of the array minus the index.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// This method <b>pins</b> the array, which can cause performance issues with the garbage collector. Applications should only pin their objects for a very short time for best 
        /// performance. Use the <see cref="GorgonNativeBuffer{T}.Dispose"/> method to unpin the array when it is no longer needed.
        /// </note>
        /// </para>
        /// <para>
        /// For example:
        /// <code lang="csharp">
        /// <![CDATA[
        /// using Gorgon.Native;
        /// 
        /// int[] data = { 1, 2, 3, 4, 5 };
        /// GorgonNativeBuffer<int> buffer = data.PinAsNativeBuffer<int>();
        /// 
        /// Console.WriteLine("{buffer[2]} = {data[2]}"); // This will print "3 = 3"
        /// 
        /// buffer.Dispose(); // Unpin the array.    
        /// ]]>
        /// </code>
        /// </para>
        /// </remarks>
        public GorgonNativeBuffer<T> PinAsNativeBuffer(long index = 0, long? count = null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);

            count ??= array.LongLength - index;

            ArgumentOutOfRangeException.ThrowIfLessThan(count.Value, 0);

            if ((index + count) > array.LongLength)
            {
                throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, index, count));
            }

            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);

            return new GorgonNativeBuffer<T>(handle, index, count.Value);
        }
    }

    /// <typeparam name="T">The type of value to convert. Must be an unmanaged type.</typeparam>
    extension<T>(ref T value) where T : unmanaged
    {
        /// <summary>
        /// Function to pin a value type of type <typeparamref name="T"/>, and access its contents as a byte buffer.
        /// </summary>
        /// <returns>A new <see cref="GorgonNativeBuffer{T}"/> containing the contents of the value as a series of byte values.</returns>
        /// <remarks>
        /// <para>
        /// This converts the value into a <see cref="GorgonNativeBuffer{T}"/> type containing the series of bytes for the value.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// This method <b>pins</b> the value, which can cause performance issues with the garbage collector. Applications should only pin their objects for a very short time for 
        /// best performance. Use the <see cref="GorgonNativeBuffer{T}.Dispose"/> method to unpin the array when it is no longer needed.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public GorgonNativeBuffer<byte> PinAsNativeByteBuffer()
        {
            int srcSize = Unsafe.SizeOf<T>();
            var handle = GCHandle.Alloc(value, GCHandleType.Pinned);

            return new GorgonNativeBuffer<byte>(handle, 0, srcSize);
        }
    }

    /// <typeparam name="T">The type of data in the span. Must be an unmanaged value type.</typeparam>
    extension<T>(Span<T> span) where T : unmanaged
    {
        /// <summary>
        /// Function to copy the contents of a <see cref="Span{T}"/> to the memory pointed at by a <see cref="GorgonPtr{T}"/>.
        /// </summary>
        
        /// <param name="ptr">The <see cref="GorgonPtr{T}"/> that points to the memory that will receive the data.</param>
        /// <param name="count">[Optional] The number of items to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="ptr"/> parameter is <see cref="GorgonPtr{T}.NullPtr"/>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the span is empty.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="count"/> parameter is larger than either the span, or <paramref name="ptr"/> parameter.</exception>
        /// <remarks>
        /// <para>
        /// This will copy the contents of a <see cref="Span{T}"/> to a block of memory pointed at by a <see cref="GorgonPtr{T}"/>. Because this memory is copied, changes to the memory pointed at by the 
        /// <paramref name="ptr"/> parameter will not be reflected in the span.
        /// </para>
        /// <para>
        /// When the <paramref name="count"/> parameter is specified, the copied amount will be limited to that number of items. If the <paramref name="count"/> parameter is omitted, then the items copied will 
        /// be the <see cref="Span{T}.Length"/> of the span.
        /// </para>
        /// <para>
        /// Example:
        /// <code lang="csharp">
        /// <![CDATA[
        /// int[] arr = { 1, 2, 3, 4, 5 };
        /// Span<int> span = arr.AsSpan();
        /// GorgonPtr<int> ptr = ... // Get a pointer to a block of memory.
        /// 
        /// span.CopyTo<int>(ptr);
        /// 
        /// ]]>
        /// </code>
        /// </para>
        /// </remarks>
        public void CopyTo(GorgonPtr<T> ptr, int? count = 0)
    => CopyTo((ReadOnlySpan<T>)span, ptr, count);
    }

    /// <typeparam name="T">The type of data in the span. Must be an unmanaged value type.</typeparam>
    extension<T>(ReadOnlySpan<T> span) where T : unmanaged
    {
        /// <summary>
        /// Function to copy the contents of a <see cref="ReadOnlySpan{T}"/> to the memory pointed at by a <see cref="GorgonPtr{T}"/>.
        /// </summary>        
        /// <param name="ptr">The <see cref="GorgonPtr{T}"/> that points to the memory that will receive the data.</param>
        /// <param name="count">[Optional] The number of items to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="ptr"/> parameter is <see cref="GorgonPtr{T}.NullPtr"/>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the span is empty.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="count"/> parameter is larger than either the span, or <paramref name="ptr"/> parameter.</exception>
        /// <remarks>
        /// <para>
        /// This will copy the contents of a <see cref="ReadOnlySpan{T}"/> to a block of memory pointed at by a <see cref="GorgonPtr{T}"/>. Because this memory is copied, changes to the memory pointed at by the 
        /// <paramref name="ptr"/> parameter will not be reflected in the span.
        /// </para>
        /// <para>
        /// When the <paramref name="count"/> parameter is specified, the copied amount will be limited to that number of items. If the <paramref name="count"/> parameter is omitted, then the items copied will 
        /// be the <see cref="ReadOnlySpan{T}.Length"/> of the span.
        /// </para>
        /// <para>
        /// <code lang="csharp">
        /// Example:
        /// <![CDATA[
        /// int[] arr = { 1, 2, 3, 4, 5 };
        /// ReadOnlySpan<int> span = arr.AsSpan();
        /// GorgonPtr<int> ptr = ... // Get a pointer to a block of memory.
        /// 
        /// span.CopyTo<int>(ptr);
        /// ]]>
        /// </code>
        /// </para>
        /// </remarks>
        public void CopyTo(GorgonPtr<T> ptr, int? count = 0)
        {
            ArgumentEmptyException.ThrowIfEmpty(span);

            if (ptr == GorgonPtr<T>.NullPtr)
            {
                throw new ArgumentNullException(nameof(ptr));
            }

            count ??= span.Length;

            if ((count > span.Length) || (count > ptr.Length))
            {
                throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, 0, count), nameof(count));
            }

            unsafe
            {
                fixed (T* srcPtr = span)
                {
                    NativeMemory.Copy(srcPtr, (void*)ptr, (nuint)(span.Length * ptr.TypeSize));
                }
            }
        }
    }

    extension<T>(T value) where T : IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IAdditiveIdentity<T, T>, IBinaryInteger<T>, IBinaryNumber<T>, IBitwiseOperators<T, T, T>
    {
        /// <summary>
        /// Function to change the number to align to a specific alignment value.
        /// </summary>
        /// <param name="alignment">The aligmnent amount.</param>
        /// <returns>The aligned value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T AlignUp(T alignment)
        {
            if (T.Zero == alignment)
            {
                return value;
            }

            T mask = alignment - T.One;
            return (value + mask) & ~mask;
        }

        /// <summary>
        /// Function to change the number to align to a specific alignment value.
        /// </summary>
        /// <param name="alignment">The aligmnent amount.</param>
        /// <returns>The aligned value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T AlignDown(T alignment)
        {
            if (T.Zero == alignment)
            {
                return value;
            }

            T mask = alignment - T.One;
            return value & ~mask;
        }
    }
}
