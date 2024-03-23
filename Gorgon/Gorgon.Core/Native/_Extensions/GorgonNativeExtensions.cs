
// 
// Gorgon
// Copyright (C) 2018 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: March 29, 2018 12:24:36 PM
// 

using System.Runtime.CompilerServices;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Properties;

namespace Gorgon.Native;

/// <summary>
/// Extension methods for native memory functionality
/// </summary>
public static class GorgonNativeExtensions
{
    /// <summary>
    /// Function to copy the contents of a stream into a <see cref="GorgonNativeBuffer{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of value in the buffer. Must be an unmanaged value type.</typeparam>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="count">[Optional] The maximum number of items to read from the stream.</param>
    /// <returns>A <see cref="GorgonNativeBuffer{T}"/> containing the contents of the stream.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
    /// <exception cref="EndOfStreamException">Thrown when the <paramref name="stream"/> is at its end.</exception>
    /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write only.</exception>
    public static GorgonNativeBuffer<T> ToNativeBuffer<T>(this Stream stream, int? count = null)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.Position == stream.Length)
        {
            throw new EndOfStreamException();
        }

        if (!stream.CanRead)
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
        }

        int typeSize = Unsafe.SizeOf<T>();

        if (count is null)
        {
            if (typeSize == 1)
            {
                count = (int)(stream.Length - stream.Position);
            }
            else
            {
                count = (int)((stream.Length - stream.Position) / typeSize);
            }
        }

        GorgonNativeBuffer<T> result = new(count.Value);

        using (GorgonBinaryReader reader = new(stream, true))
        {
            for (int i = 0; i < count.Value; ++i)
            {
                if (stream.Position + typeSize >= stream.Length)
                {
                    break;
                }

                reader.ReadValue(out result[i]);
            }
        }

        return result;
    }

    /// <summary>
    /// Function to copy the contents of a stream into a <see cref="GorgonPtr{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of value in the pointer. Must be an unmanaged value type.</typeparam>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="pointer">The pointer to copy the stream data into.</param>
    /// <param name="index">[Optional] The index within the pointer to start writing at.</param>
    /// <param name="count">[Optional] The maximum number of items to read from the stream.</param>
    /// <returns>A <see cref="GorgonNativeBuffer{T}"/> containing the contents of the stream.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/>, or the <paramref name="pointer"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="index"/>, <paramref name="count"/>, or both values added together will exceed the size of the <paramref name="pointer"/> or the <paramref name="stream"/>.</exception>
    /// <exception cref="EndOfStreamException">Thrown when the <paramref name="stream"/> is at its end.</exception>
    /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write only.</exception>
    public static void CopyTo<T>(this Stream stream, GorgonPtr<T> pointer, int index = 0, int? count = null)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.Position == stream.Length)
        {
            throw new EndOfStreamException();
        }

        if (!stream.CanRead)
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
        }

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        count ??= pointer.Length.Min((int)(stream.Length - stream.Position));

        if (count == 0)
        {
            return;
        }

        int countBytes = count.Value * Unsafe.SizeOf<T>();

        if (((index + count.Value) > pointer.Length)
            || ((stream.Position + countBytes) > stream.Length))
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, index, count));
        }

        GorgonNativeBuffer<T> result = new(count.Value);
        using GorgonBinaryReader reader = new(stream, true);
        for (int i = 0; i < count.Value; ++i)
        {
            reader.ReadValue(out result[i]);
        }
    }

    /// <summary>
    /// Function to copy the contents of a span into a <see cref="GorgonPtr{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of data in the span and pointer. Must be an unmanaged value type.</typeparam>
    /// <param name="span">The span to copy from.</param>
    /// <param name="pointer">The pointer that will receive the data.</param>
    /// <param name="pointerIndex">[Optional] The index in the pointer to start writing into.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="pointerIndex"/> parameter is less than 0.</exception>
    /// <exception cref="ArgumentException">
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="pointerIndex"/> + span length is too big for the <paramref name="pointer"/>.</para>
    /// </exception>
    public static void CopyTo<T>(this ReadOnlySpan<T> span, GorgonPtr<T> pointer, int pointerIndex = 0)
        where T : unmanaged
    {
        if (pointer == GorgonPtr<T>.NullPtr)
        {
            throw new ArgumentNullException(nameof(pointer));
        }

        if (pointerIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pointerIndex), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        if (pointerIndex + span.Length > pointer.Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, pointerIndex, span.Length));
        }

        unsafe
        {
            fixed (T* srcPtr = &span[0])
            fixed (T* destPtr = &pointer[pointerIndex])
            {
                Unsafe.CopyBlock(destPtr, srcPtr, (uint)(span.Length * Unsafe.SizeOf<T>()));
            }
        }
    }

    /// <summary>
    /// Function to copy the contents of a memory type into a <see cref="GorgonPtr{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of data in the memory type and pointer. Must be an unmanaged value type.</typeparam>
    /// <param name="memory">The memory type to copy from.</param>
    /// <param name="pointer">The pointer that will receive the data.</param>
    /// <param name="pointerIndex">[Optional] The index in the pointer to start writing into.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="pointerIndex"/> parameter is less than 0.</exception>
    /// <exception cref="ArgumentException">
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="pointerIndex"/> + memory type length is too big for the <paramref name="pointer"/>.</para>
    /// </exception>
    public static void CopyTo<T>(this ReadOnlyMemory<T> memory, GorgonPtr<T> pointer, int pointerIndex = 0)
        where T : unmanaged
    {
        if (pointer == GorgonPtr<T>.NullPtr)
        {
            throw new ArgumentNullException(nameof(pointer));
        }

        if (pointerIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pointerIndex), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        if (pointerIndex + memory.Length > pointer.Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, pointerIndex, memory.Length));
        }

        unsafe
        {
            fixed (T* srcPtr = &memory.Span[0])
            fixed (T* destPtr = &pointer[pointerIndex])
            {
                Unsafe.CopyBlock(destPtr, srcPtr, (uint)(memory.Length * Unsafe.SizeOf<T>()));
            }
        }
    }

    /// <summary>
    /// Function to copy the contents of an array into a <see cref="GorgonPtr{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of data in the array and pointer. Must be an unmanaged value type.</typeparam>
    /// <param name="array">The array to copy from.</param>
    /// <param name="pointer">The pointer that will receive the data.</param>
    /// <param name="arrayIndex">[Optional] The index in the array to start copying from.</param>
    /// <param name="count">[Optional] The number of items to copy.</param>
    /// <param name="pointerIndex">[Optional] The index in the pointer to start writing into.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/>, or <paramref name="pointer"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="arrayIndex"/>, or the <paramref name="pointerIndex"/> parameter is less than 0.</exception>
    /// <exception cref="ArgumentException">
    /// <para>Thrown when the <paramref name="arrayIndex"/> + <paramref name="count"/> is too big for the <paramref name="array"/>.</para>
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="pointerIndex"/> + <paramref name="count"/> is too big for the <paramref name="pointer"/>.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// If the <paramref name="count"/> parameter is ommitted, then the full length of the source pointer, minus the <paramref name="arrayIndex"/> is used. Ensure that there is enough space in the 
    /// <paramref name="pointer"/> to accomodate the amount of data required.
    /// </para>
    /// </remarks>        
    public static void CopyTo<T>(this T[] array, GorgonPtr<T> pointer, int arrayIndex = 0, int? count = null, int pointerIndex = 0)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(array);

        if (pointer == GorgonPtr<T>.NullPtr)
        {
            throw new ArgumentNullException(nameof(pointer));
        }

        count ??= array.Length - arrayIndex;

        GorgonNativeBuffer<T>.ValidateArrayParams(array, arrayIndex, count.Value);

        if (pointerIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pointerIndex), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        if (pointerIndex + count.Value > pointer.Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, pointerIndex, count));
        }

        unsafe
        {
            fixed (T* srcPtr = &array[arrayIndex])
            fixed (T* destPtr = &pointer[pointerIndex])
            {
                Unsafe.CopyBlock(destPtr, srcPtr, (uint)(count.Value * Unsafe.SizeOf<T>()));
            }
        }
    }

    /// <summary>
    /// Function to convert an array into a <see cref="GorgonNativeBuffer{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of data in the array. Must be an unmanaged value type.</typeparam>
    /// <param name="array">The array to turn into a native buffer.</param>
    /// <param name="index">[Optional] The index in the array that represents the beginning of the native buffer.</param>
    /// <param name="count">[Optional] The number of items in the array that will be contained in the buffer.</param>
    /// <returns>A new <see cref="GorgonNativeBuffer{T}"/> containing the <paramref name="array"/> data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> + <paramref name="count"/> are equal to or greater than the length of <paramref name="array"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method copies the contents of an array into a <see cref="GorgonNativeBuffer{T}"/> so that the data can be used with native code.
    /// </para>
    /// <para>
    /// If the <paramref name="index"/> is not supplied, then the beginning of the <paramref name="array"/> is used as the start of the buffer, and if the <paramref name="count"/> parameter is not 
    /// supplied, then the length of the <paramref name="array"/> (minus the <paramref name="index"/>) is used. 
    /// </para>
    /// <para>
    /// <note type="warning">
    /// This method <b>copies</b> the data from the array into the buffer. This may have a negative impact on performance and memory usage. 
    /// </note>
    /// </para>
    /// </remarks>        
    public static GorgonNativeBuffer<T> ToNativeBuffer<T>(this T[] array, int index = 0, int? count = null)
        where T : unmanaged
    {
        count ??= array.Length - index;

        GorgonNativeBuffer<T>.ValidateArrayParams(array, index, count.Value);

        GorgonNativeBuffer<T> result = new(count.Value);

        unsafe
        {
            fixed (T* srcPtr = &array[index])
            {
                Unsafe.CopyBlock((T*)result, srcPtr, (uint)(Unsafe.SizeOf<T>() * count.Value));
            }
        }

        return result;
    }

    /// <summary>
    /// Function to copy the contents of a span slice to a <see cref="GorgonNativeBuffer{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of data in the array. Must be an unmanaged value type.</typeparam>
    /// <param name="span">The span to copy from.</param>
    /// <returns>A native buffer containing the contents of the span.</returns>
    public static GorgonNativeBuffer<T> ToNativeBuffer<T>(this ReadOnlySpan<T> span)
        where T : unmanaged
    {
        if (span.Length == 0)
        {
            return null;
        }

        GorgonNativeBuffer<T> result = new(span.Length);
        span.CopyTo(result.Pointer);

        return result;
    }

    /// <summary>
    /// Function to copy the contents of a memory type to a <see cref="GorgonNativeBuffer{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of data in the array. Must be an unmanaged value type.</typeparam>
    /// <param name="memory">The memory type to copy from.</param>
    /// <returns>A native buffer containing the contents of the memory type.</returns>
    public static GorgonNativeBuffer<T> ToNativeBuffer<T>(this ReadOnlyMemory<T> memory)
        where T : unmanaged
    {
        if (memory.Length == 0)
        {
            return null;
        }

        GorgonNativeBuffer<T> result = new(memory.Length);
        memory.Span.CopyTo(result.Pointer);

        return result;
    }

    /// <summary>
    /// Function to pin an array and return a <see cref="GorgonNativeBuffer{T}"/> containing the pinned data.
    /// </summary>
    /// <typeparam name="T">The type of data in the array. Must be an unmanaged value type.</typeparam>
    /// <param name="array">The array to turn into a native buffer.</param>
    /// <param name="index">[Optional] The index in the array that represents the beginning of the native buffer.</param>
    /// <param name="count">[Optional] The number of items in the array that will be contained in the buffer.</param>
    /// <returns>A new <see cref="GorgonNativeBuffer{T}"/> containing the <paramref name="array"/> data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is less than 0.</exception>
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
    /// <note type="warning">
    /// This method <b>pins</b> the <paramref name="array"/>, which can cause performance issues with the garbage collector. Applications should only pin their objects for a very short time for best 
    /// performance.
    /// </note>
    /// </para>
    /// </remarks>        
    public static GorgonNativeBuffer<T> ToPinned<T>(this T[] array, int index = 0, int? count = null)
        where T : unmanaged
    {
        count ??= array.Length - index;

        return GorgonNativeBuffer<T>.Pin(array, index, count);
    }
}
