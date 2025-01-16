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
// Created: January 22, 2024 4:19:42 PM
//

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.Native;

/// <summary>
/// Extension methods for native memory functionality.
/// </summary>
public static class GorgonNativeExtensions
{
    /// <summary>
    /// Function to copy the contents of a stream into the memory pointed at by a <see cref="GorgonPtr{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of data stored in the memory pointed at by the pointer. Must be an unmanaged value type.</typeparam>
    /// <param name="stream">The stream to write into.</param>
    /// <param name="ptr">The pointer that points to the block of memory to write into the stream.</param>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
    /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is read only.</exception>
    /// <exception cref="EndOfStreamException">Thrown when the <paramref name="stream"/> is at its end.</exception>"
    public static void Read<T>(this Stream stream, GorgonPtr<T> ptr)
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

        int size = ptr.SizeInBytes;

        // Ensure the stream has enough room to read the data.
        if ((stream.Length - stream.Position) < size * ptr.TypeSize)
        {
            throw new EndOfStreamException();
        }

        unsafe
        {
            fixed (T* srcPtr = &ptr[0])
            {
                stream.Read(new Span<byte>((byte*)srcPtr, size));
            }
        }
    }

    /// <summary>
    /// Function to copy the contents of a stream into the memory pointed at by a <see cref="GorgonNativeBuffer{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of data stored in the memory pointed at by the pointer. Must be an unmanaged value type.</typeparam>
    /// <param name="stream">The stream to write into.</param>
    /// <param name="buffer">The buffer that will receive the data.</param>
    /// <param name="index">The index in the buffer where the data will be stored.</param>
    /// <param name="count">[Optional] The number of items to read.</param>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
    /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is read only.</exception>
    /// <exception cref="EndOfStreamException">Thrown when the <paramref name="stream"/> is at its end.</exception>"
    /// <exception cref="ArgumentException">Thrown if the <paramref name="index"/> is larger than the <see cref="GorgonNativeBuffer{T}.Length"/> of the <see cref="GorgonNativeBuffer{T}"/>.
    /// <para>-or-</para>
    /// <para>Thrown if the <paramref name="index"/> is less than 0, or the <paramref name="count"/> parameter is less than 1.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the <paramref name="index"/> plus the <paramref name="count"/> is larger than the buffer or stream.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// When the <paramref name="count"/> parameter is specified, the buffer will be limited to that number of items. If the <paramref name="count"/> parameter is omitted, then the count will be sized to 
    /// the <see cref="GorgonNativeBuffer{T}.Length"/>.
    /// </para>
    /// </remarks>
    public static void Read<T>(this Stream stream, GorgonNativeBuffer<T> buffer, int index, int? count = null)
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

        int size = count ?? (buffer.Length - index);

        if (size < 1)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL), nameof(count));
        }

        if (index + size > buffer.Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, index, size));
        }

        // Ensure the stream has enough room to read the data.
        if ((stream.Length - stream.Position) < size * buffer.TypeSize)
        {
            throw new EndOfStreamException();
        }

        unsafe
        {
            fixed (T* srcPtr = &buffer[index])
            {
                stream.Read(new Span<byte>((byte*)srcPtr, size * buffer.TypeSize));
            }
        }
    }

    /// <summary>
    /// Function to copy the contents of the memory pointed at by a pointer into a stream.
    /// </summary>
    /// <typeparam name="T">The type of data stored in the memory pointed at by the pointer. Must be an unmanaged value type.</typeparam>
    /// <param name="stream">The stream to write into.</param>
    /// <param name="ptr">The pointer that points to the block of memory to write into the stream.</param>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
    /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is read only.</exception>
    public static void Write<T>(this Stream stream, GorgonPtr<T> ptr)
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

        unsafe
        {
            fixed (T* srcPtr = &ptr[0])
            {
                stream.Write(new ReadOnlySpan<byte>((byte*)srcPtr, ptr.SizeInBytes));
            }
        }
    }

    /// <summary>
    /// Function to copy the contents of the memory pointed at by a pointer into a stream.
    /// </summary>
    /// <typeparam name="T">The type of data stored in the memory pointed at by the pointer. Must be an unmanaged value type.</typeparam>
    /// <param name="stream">The stream to write into.</param>
    /// <param name="buffer">The pointer that points to the block of memory to write into the stream.</param>
    /// <param name="index">The index in the buffer where the data will be copied from.</param>
    /// <param name="count">[Optional] The number of items to read.</param>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
    /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is read only.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="index"/> is larger than the <see cref="GorgonNativeBuffer{T}.Length"/> of the <see cref="GorgonNativeBuffer{T}"/>.
    /// <para>-or-</para>
    /// <para>Thrown if the <paramref name="index"/> is less than 0, or the <paramref name="count"/> parameter is less than 1.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the <paramref name="index"/> plus the <paramref name="count"/> is larger than the buffer or stream.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// When the <paramref name="count"/> parameter is specified, the buffer will be limited to that number of items. If the <paramref name="count"/> parameter is omitted, then the count will be sized to 
    /// the <see cref="GorgonNativeBuffer{T}.Length"/>.
    /// </para>
    /// </remarks>
    public static void Write<T>(this Stream stream, GorgonNativeBuffer<T> buffer, int index, int? count = null)
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

        int size = count ?? (buffer.Length - index);

        if (size < 1)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL), nameof(count));
        }

        if (index + size > buffer.Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, index, size));
        }

        unsafe
        {
            fixed (T* srcPtr = &buffer[index])
            {
                stream.Write(new Span<byte>((byte*)srcPtr, size * buffer.TypeSize));
            }
        }
    }

    /// <summary>
    /// Function to pin an array and access its contents natively.
    /// </summary>
    /// <typeparam name="T">The type of data in the array. Must be an unmanaged value type.</typeparam>
    /// <param name="array">The array to pin.</param>
    /// <param name="index">[Optional] The starting index in the array to pin.</param>
    /// <param name="count">[Optional] The number of items in the array to pin.</param>
    /// <returns>A new <see cref="GorgonNativeBuffer{T}"/> containing the pinned contents of the array.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> or <paramref name="count"/> is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> + <paramref name="count"/> are equal to or greater than the length of <paramref name="array"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method pins the supplied <paramref name="array"/> and returns a new <see cref="GorgonNativeBuffer{T}"/> containing the pinned data.
    /// </para>
    /// <para>
    /// If the <paramref name="index"/> is not supplied, then the beginning of the <paramref name="array"/> is used as the start of the buffer, and if the <paramref name="count"/> parameter is not 
    /// supplied, then the length of the <paramref name="array"/> (minus the <paramref name="index"/>) is used. 
    /// </para>
    /// <para>
    /// When the <paramref name="count"/> parameter is specified, the buffer will be limited to that number of items. If the <paramref name="count"/> parameter is omitted, then the buffer will be sized to 
    /// the length of the array minus the index.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// This method <b>pins</b> the <paramref name="array"/>, which can cause performance issues with the garbage collector. Applications should only pin their objects for a very short time for best 
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
    public static GorgonNativeBuffer<T> PinAsNativeBuffer<T>(this T[] array, int index = 0, int? count = null)
        where T : unmanaged
    {
        count ??= array.Length - index;

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
        }

        if ((index + count) > array.Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, index, count));
        }

        var handle = GCHandle.Alloc(array, GCHandleType.Pinned);

        return new GorgonNativeBuffer<T>(handle, index, count.Value);
    }

    /// <summary>
    /// Function to pin a value type of type <typeparamref name="T"/>, and access its contents as a byte buffer.
    /// </summary>
    /// <param name="value">The value to pin.</param>
    /// <returns>A new <see cref="GorgonNativeBuffer{T}"/> containing the contents of the value as a series of byte values.</returns>
    /// <remarks>
    /// <para>
    /// This allows access to the value passed to <paramref name="value"/> as a series bytes for native manipulation.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// This method <b>pins</b> the <paramref name="value"/>, which can cause performance issues with the garbage collector. Applications should only pin their objects for a very short time for 
    /// best performance. Use the <see cref="GorgonNativeBuffer{T}.Dispose"/> method to unpin the array when it is no longer needed.
    /// </note>
    /// </para>
    /// </remarks>
    public static GorgonNativeBuffer<byte> PinAsNativeByteBuffer<T>(this ref T value)
        where T : unmanaged
    {
        int srcSize = Unsafe.SizeOf<T>();
        var handle = GCHandle.Alloc(value, GCHandleType.Pinned);

        return new GorgonNativeBuffer<byte>(handle, 0, srcSize);
    }

    /// <summary>
    /// Function to copy the contents of a <see cref="Span{T}"/> to the memory pointed at by a <see cref="GorgonPtr{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of data in the span. Must be an unmanaged value type.</typeparam>
    /// <param name="span">The span containing the data to copy.</param>
    /// <param name="ptr">The <see cref="GorgonPtr{T}"/> that points to the memory that will receive the data.</param>
    /// <param name="count">[Optional] The number of items to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="ptr"/> parameter is <see cref="GorgonPtr{T}.NullPtr"/>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="span"/> parameter is empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="count"/> parameter is larger than either the <paramref name="span"/>, or <paramref name="ptr"/> parameter.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This will copy the contents of a <see cref="Span{T}"/> to a block of memory pointed at by a <see cref="GorgonPtr{T}"/>. Because this memory is copied, changes to the memory pointed at by the 
    /// <paramref name="ptr"/> parameter will not be reflected in the <paramref name="span"/> parameter.
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
    public static void CopyTo<T>(this Span<T> span, GorgonPtr<T> ptr, int? count = 0)
        where T : unmanaged => CopyTo((ReadOnlySpan<T>)span, ptr, count);

    /// <summary>
    /// Function to copy the contents of a <see cref="ReadOnlySpan{T}"/> to the memory pointed at by a <see cref="GorgonPtr{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of data in the span. Must be an unmanaged value type.</typeparam>
    /// <param name="span">The span containing the data to copy.</param>
    /// <param name="ptr">The <see cref="GorgonPtr{T}"/> that points to the memory that will receive the data.</param>
    /// <param name="count">[Optional] The number of items to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="ptr"/> parameter is <see cref="GorgonPtr{T}.NullPtr"/>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="span"/> parameter is empty.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="count"/> parameter is larger than either the <paramref name="span"/>, or <paramref name="ptr"/> parameter.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This will copy the contents of a <see cref="ReadOnlySpan{T}"/> to a block of memory pointed at by a <see cref="GorgonPtr{T}"/>. Because this memory is copied, changes to the memory pointed at by the 
    /// <paramref name="ptr"/> parameter will not be reflected in the <paramref name="span"/> parameter.
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
    public static void CopyTo<T>(this ReadOnlySpan<T> span, GorgonPtr<T> ptr, int? count = 0)
        where T : unmanaged
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
            fixed (T* srcPtr = &span[0])
            fixed (T* destPtr = &ptr[0])
            {
                Unsafe.CopyBlock((byte*)destPtr, (byte*)srcPtr, (uint)(count.Value * Unsafe.SizeOf<T>()));
            }
        }
    }
}
