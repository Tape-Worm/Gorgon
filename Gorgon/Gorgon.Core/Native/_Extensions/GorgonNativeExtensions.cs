#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: March 29, 2018 12:24:36 PM
// 
#endregion

using System;
using System.IO;
using System.Runtime.CompilerServices;
using Gorgon.IO;
using Gorgon.Properties;

namespace Gorgon.Native
{
    /// <summary>
    /// Extension methods for native memory functionality.
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
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (stream.Position == stream.Length)
            {
                throw new EndOfStreamException();
            }

            if (!stream.CanRead)
            {
                throw new IOException(Resources.GOR_ERR_STREAM_IS_WRITEONLY);
            }

            int typeSize = Unsafe.SizeOf<T>();

            if (count == null)
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

            var result = new GorgonNativeBuffer<T>(count.Value);

            using (var reader = new GorgonBinaryReader(stream, true))
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
        /// Function to copy the contents of an array into a <see cref="GorgonNativeBuffer{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of data in the array and buffer. Must be an unmanaged value type.</typeparam>
        /// <param name="array">The array to copy from.</param>
        /// <param name="buffer">The buffer that will receive the data.</param>
        /// <param name="arrayIndex">[Optional] The index in the array to start copying from.</param>
        /// <param name="count">[Optional] The number of items to copy.</param>
        /// <param name="bufferIndex">[Optional] The index in the buffer to start writing into.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/>, or <paramref name="buffer"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="arrayIndex"/>, or the <paramref name="bufferIndex"/> parameter is less than 0.</exception>
        /// <exception cref="ArgumentException">
        /// <para>Thrown when the <paramref name="arrayIndex"/> + <paramref name="count"/> is too big for the <paramref name="array"/>.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="bufferIndex"/> + <paramref name="count"/> is too big for the <paramref name="buffer"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// If the <paramref name="count"/> parameter is ommitted, then the full length of the source buffer, minus the <paramref name="arrayIndex"/> is used. Ensure that there is enough space in the 
        /// <paramref name="buffer"/> to accomodate the amount of data required.
        /// </para>
        /// </remarks>
        public static void CopyTo<T>(this T[] array, GorgonNativeBuffer<T> buffer, int arrayIndex = 0, int? count = null, int bufferIndex = 0)
            where T : unmanaged
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (count == null)
            {
                count = array.Length - arrayIndex;
            }

            GorgonNativeBuffer<T>.ValidateArrayParams(array, arrayIndex, count.Value);

            if (bufferIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferIndex), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
            }

            if (bufferIndex + count.Value > buffer.Length)
            {
                throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, bufferIndex, count));
            }

            unsafe
            {
                fixed (T* srcPtr = &array[arrayIndex])
                fixed (T* destPtr = &buffer[bufferIndex])
                {
                    Unsafe.CopyBlock(destPtr, srcPtr, (uint)count.Value);
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
            if (count == null)
            {
                count = array.Length - index;
            }

            GorgonNativeBuffer<T>.ValidateArrayParams(array, index, count.Value);

            var result = new GorgonNativeBuffer<T>(count.Value);

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
            if (count == null)
            {
                count = array.Length - index;
            }

            return GorgonNativeBuffer<T>.Pin(array, index, count);
        }
    }
}
