#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Monday, June 27, 2011 8:57:11 AM
// 
#endregion

using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.Properties;

namespace Gorgon.IO
{
    /// <summary>
    /// An extended binary writer class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This object extends the functionality of the <see cref="BinaryWriter"/> type by adding extra functions to write to a pointer (or <see cref="IntPtr"/>), and to generic value types.
    /// </para>
    /// </remarks>
    public class GorgonBinaryWriter
        : BinaryWriter
    {
        #region Variables.
        // The size of the temporary buffer used to stream data out.
        private int _bufferSize = 65536;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the size of the buffer, in bytes, used to stream the data out.
        /// </summary>
        /// <remarks>
        /// This value is meant to help in buffering data to the data source if the source data is large. It will only accept a value between 128 and 81920.  The upper bound is 
        /// to ensure that the temporary buffer is not pushed into the Large Object Heap.
        /// </remarks>
        public int BufferSize
        {
            get => _bufferSize;
            set
            {
                if (value < 128)
                {
                    value = 128;
                }

                if (value > 81920)
                {
                    value = 81920;
                }

                _bufferSize = value;
            }
        }

        /// <summary>
        /// Property to set or return whether to keep the underlying stream open or not after the writer is closed.
        /// </summary>
        public bool KeepStreamOpen
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to write data from a <see cref="GorgonNativeBuffer{T}"/> to a stream.
        /// </summary>
        /// <typeparam name="T">The type of data in the buffer. Must be an unmanaged value type.</typeparam>
        /// <param name="buffer">The buffer to write to the stream.</param>
        /// <param name="index">[Optional] The index in the buffer to start copying from.</param>
        /// <param name="count">[Optional] The number of items in the buffer to copy.</param>
        /// <remarks>
        /// <para>
        /// If the <paramref name="count"/> is omitted, then the <see cref="GorgonNativeBuffer{T}.Length"/> of the buffer minus the index is used.
        /// </para>
        /// <para>
        /// This method will constrain the <paramref name="index"/> and <paramref name="count"/> parameters to ensure they do not go out of bounds in the buffer.
        /// </para>
        /// </remarks>
        public void WriteRange<T>(GorgonNativeBuffer<T> buffer, int index = 0, int? count = null)
            where T : unmanaged
        {
            if (buffer == null)
            {
                return;
            }

            // Constrain the start index to within the length of the buffer.
            index = index.Max(0).Min(buffer.Length);

            if (count == null)
            {
                count = buffer.Length - index;
            }

            if ((count + index) > buffer.Length)
            {
                count = buffer.Length - index;
            }

            if (count < 1)
            {
                return;
            }

            unsafe
            {
                int typeSize = Unsafe.SizeOf<T>();
                void* ptr = (byte*)buffer + (typeSize * index);
                Write(ptr, count.Value * typeSize);
            }
        }

        /// <summary>
        /// Function to write the bytes pointed at by the pointer into the stream.
        /// </summary>
        /// <param name="pointer">Pointer to the buffer containing the data.</param>
        /// <param name="size">Number of bytes to write.</param>
        /// <remarks>
        /// <para>
        /// This method will write the number of bytes specified by the <paramref name="size"/> parameter from the data pointed at by the raw <paramref name="pointer"/>.
        /// </para>
        /// <note type="caution">
        /// This method is unsafe, therefore a proper <paramref name="size"/> must be passed to the method.  Failure to do so can lead to memory corruption.  Use this method at your own peril.
        /// </note>
        /// </remarks>
        public unsafe void Write(void* pointer, int size)
        {
            if ((pointer == null) || (size < 1))
            {
                return;
            }

            var data = (byte*)pointer;

            while (size > 0)
            {
                if (size >= sizeof(long))
                {
                    Write(*((long*)data));
                    size -= sizeof(long);
                    data += sizeof(long);
                }

                if (size >= sizeof(int))
                {
                    Write(*((int*)data));
                    size -= sizeof(int);
                    data += sizeof(int);
                }

                if (size >= 2)
                {
                    Write(*((short*)data));
                    size -= sizeof(short);
                    data += sizeof(short);
                }

                if (size <= 0)
                {
                    return;
                }

                Write(*data);
                size--;
                data++;
            }
        }

        /// <summary>
        /// Function to write a generic value to the stream.
        /// </summary>
        /// <typeparam name="T">Type of value to write.  Must be an unmanaged value type.</typeparam>
        /// <param name="value">Value to write to the stream.</param>
        /// <remarks>
        /// <para>
        /// This method will write the data to the binary stream from the <paramref name="value"/> of type <typeparamref name="T"/>. The amount of data written will be dependant upon the size of 
        /// <typeparamref name="T"/>, and any packing rules applied.
        /// </para>
        /// <note type="important">
        /// <para>
        /// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
        /// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
        /// </para>
        /// <para>
        /// Value types with marshalling attributes (<see cref="MarshalAsAttribute"/>) are <i>not</i> supported and will not be read correctly.
        /// </para>
        /// </note>
        /// </remarks>
        /// <exception cref="IOException">Thrown when the stream is read-only.</exception>
        public void WriteValue<T>(ref T value)
            where T : unmanaged
        {
            unsafe
            {
                void* ptr = Unsafe.AsPointer(ref value);
                Write(ptr, Unsafe.SizeOf<T>());
            }
        }

        /// <summary>
        /// Function to write a generic value to the stream.
        /// </summary>
        /// <typeparam name="T">Type of value to write.  Must be an unmanaged value type.</typeparam>
        /// <param name="value">Value to write to the stream.</param>
        /// <remarks>
        /// <para>
        /// This method will write the data to the binary stream from the <paramref name="value"/> of type <typeparamref name="T"/>. The amount of data written will be dependant upon the size of 
        /// <typeparamref name="T"/>, and any packing rules applied.
        /// </para>
        /// <note type="important">
        /// <para>
        /// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
        /// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
        /// </para>
        /// <para>
        /// Value types with marshalling attributes (<see cref="MarshalAsAttribute"/>) are <i>not</i> supported and will not be read correctly.
        /// </para>
        /// </note>
        /// </remarks>
        /// <exception cref="IOException">Thrown when the stream is read-only.</exception>
        public void WriteValue<T>(T value)
            where T : unmanaged
        {
            unsafe
            {
                void* ptr = Unsafe.AsPointer(ref value);
                Write(ptr, Unsafe.SizeOf<T>());
            }
        }

        /// <summary>
        /// Function to write a range of generic values.
        /// </summary>
        /// <typeparam name="T">Type of value to write.  Must be an unmanaged value type.</typeparam>
        /// <param name="value">Array of values to write.</param>
        /// <param name="startIndex">[Optional] Starting index in the array.</param>
        /// <param name="count">[Optional] Number of array elements to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="startIndex"/> parameter is less than 0.
        /// <para>-or-</para>
        /// <para>Thrown when the startIndex parameter is equal to or greater than the number of elements in the value parameter.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the sum of startIndex and <paramref name="count"/> is greater than the number of elements in the value parameter.</para>
        /// </exception>
        /// <exception cref="IOException">Thrown when the stream is read-only.</exception>
        /// <remarks>
        /// <para>
        /// This will write data into the binary stream from the specified array of values of type <typeparamref name="T"/>. The values will start at the <paramref name="startIndex"/> in the array up to 
        /// the <paramref name="count"/> specified. If the <paramref name="count"/> is not specified (i.e. it is <b>null</b>), then the entire array minus the <paramref name="startIndex"/> will be used.
        /// </para>
        /// <para>
        /// The amount of data written will be dependant upon the size of type <typeparamref name="T"/> <c>* (</c><paramref name="count"/>-<paramref name="startIndex"/><c>)</c>. 
        /// Packing rules on type <typeparamref name="T"/> will affect the size of the type.
        /// </para>
        /// <note type="important">
        /// <para>
        /// The type referenced by <typeparamref name="T"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
        /// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
        /// </para>
        /// <para>
        /// Value types with marshalling attributes (<see cref="MarshalAsAttribute"/>) are <i>not</i> supported and will not be read correctly.
        /// </para>
        /// </note>
        /// </remarks>
        public void WriteRange<T>(T[] value, int startIndex = 0, int? count = null)
            where T : unmanaged
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (count == null)
            {
                count = value.Length - startIndex;
            }

            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, startIndex, 0));
            }

            if (startIndex >= value.Length)
            {
                throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_VALUE_IS_GREATER_THAN, startIndex, value.Length));
            }

            if ((value.Length == 0) || (count <= 0))
            {
                return;
            }

            if ((startIndex + count) > value.Length)
            {
                throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, startIndex + count, value.Length));
            }

            byte[] tempBuffer = ArrayPool<byte>.Shared.Rent(_bufferSize);

            try
            {
                int typeSize = Unsafe.SizeOf<T>();
                int totalSize = count.Value * typeSize;
                int blockSize = _bufferSize;
                int offset = 0;
                ref byte valueRef = ref Unsafe.As<T, byte>(ref value[startIndex]);
                ref byte bufferRef = ref tempBuffer[0];

                while (totalSize > 0)
                {
                    if (blockSize > totalSize)
                    {
                        blockSize = totalSize;
                    }

                    ref byte srcRef = ref Unsafe.Add(ref valueRef, offset);

                    // Not a fan of doing it this way. This extra indirection is kind of annoying, but we have no real way to cast T[] into byte[] and passing that 
                    // to the Stream Write method.  This may be solvable with Span<T>, but it's still in preview and I won't touch that with a 10,000 foot pole 
                    // until it's ready for use.
                    Unsafe.CopyBlock(ref bufferRef, ref srcRef, (uint)blockSize);
                    Write(tempBuffer, 0, blockSize);

                    totalSize -= blockSize;
                    offset += blockSize;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(tempBuffer);
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBinaryWriter"/> class.
        /// </summary>
        /// <param name="output">Output stream.</param>
        /// <param name="encoder">Encoding for the binary writer.</param>
        /// <param name="keepStreamOpen">[Optional] <b>true</b> to keep the underlying stream open when the writer is closed, <b>false</b> to close when done.</param>
        public GorgonBinaryWriter(Stream output, Encoding encoder, bool keepStreamOpen = false)
            : base(output, encoder, keepStreamOpen) => KeepStreamOpen = keepStreamOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBinaryWriter"/> class.
        /// </summary>
        /// <param name="output">Output stream.</param>
        /// <param name="keepStreamOpen">[Optional] <b>true</b> to keep the underlying stream open when the writer is closed, <b>false</b> to close when done.</param>
        public GorgonBinaryWriter(Stream output, bool keepStreamOpen = false)
            : this(output, Encoding.UTF8, keepStreamOpen)
        {
        }
        #endregion
    }
}
