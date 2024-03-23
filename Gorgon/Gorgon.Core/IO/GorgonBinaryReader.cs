
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Monday, June 27, 2011 8:56:28 AM
// 

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Native;
using Gorgon.Properties;

namespace Gorgon.IO;

/// <summary>
/// An extended binary reader class
/// </summary>
/// <remarks>
/// <para>
/// This object extends the functionality of the <see cref="BinaryReader"/> type by adding extra functions to read from a pointer (or <c>nint</c>), and from generic value types
/// </para>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonBinaryReader"/> class
/// </remarks>
/// <param name="input">Input stream.</param>
/// <param name="encoder">Encoding for the binary reader.</param>
/// <param name="keepStreamOpen">[Optional] <b>true</b> to keep the underlying stream open when the writer is closed, <b>false</b> to close when done.</param>
public class GorgonBinaryReader(Stream input, Encoding encoder, bool keepStreamOpen = false)
        : BinaryReader(input, encoder, keepStreamOpen)
{

    // The size of the temporary buffer used to stream data in.
    private int _bufferSize = 65536;

    /// <summary>
    /// Property to set or return the size of the buffer, in bytes, used to stream the data in.
    /// </summary>
    /// <remarks>
    /// This value is meant to help in buffering data from the data source if the data is large. It will only accept a value between 128 and 81920.  The upper bound is 
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
    /// Property to return whether to keep the underlying stream open or not after the reader is closed.
    /// </summary>
    public bool KeepStreamOpen
    {
        get;
    } = keepStreamOpen;

    /// <summary>
    /// Function to read bytes from a stream into a buffer pointed at by the pointer.
    /// </summary>
    /// <param name="pointer">Pointer to the buffer to fill with data.</param>
    /// <param name="size">Number of bytes to read.</param>
    /// <remarks>
    /// <para>
    /// This method will read the number of bytes specified by the <paramref name="size"/> parameter into memory pointed at by the raw <paramref name="pointer"/>.
    /// </para>
    /// <note type="caution">
    /// This method is unsafe, therefore a proper <paramref name="size"/> must be passed to the method.  Failure to do so can lead to memory corruption.  Use this method at your own peril.
    /// </note>
    /// </remarks>
    public unsafe void Read(void* pointer, int size)
    {
        if ((pointer == null) || (size < 1))
        {
            return;
        }

        byte* bytePtr = (byte*)pointer;

        while (size > 0)
        {
            if (size >= sizeof(long))
            {
                *((long*)bytePtr) = ReadInt64();
                bytePtr += sizeof(long);
                size -= sizeof(long);
                continue;
            }

            if (size >= sizeof(int))
            {
                *((int*)bytePtr) = ReadInt32();
                bytePtr += sizeof(int);
                size -= sizeof(int);
                continue;
            }

            if (size >= sizeof(short))
            {
                *((short*)bytePtr) = ReadInt16();
                bytePtr += sizeof(short);
                size -= sizeof(short);
                continue;
            }

            *bytePtr = ReadByte();
            ++bytePtr;
            --size;
        }
    }

    /// <summary>
    /// Function to read bytes from a stream into a reference to a buffer.
    /// </summary>
    /// <param name="buffer">The reference to the buffer to fill with data.</param>
    /// <param name="size">Number of bytes to read.</param>
    /// <remarks>
    /// <para>
    /// This method will read the number of bytes specified by the <paramref name="size"/> parameter into memory referenced by <paramref name="buffer"/>.
    /// </para>
    /// <note type="caution">
    /// This method is unsafe, therefore a proper <paramref name="size"/> must be passed to the method.  Failure to do so can lead to memory corruption.  Use this method at your own peril.
    /// </note>
    /// </remarks>

    public void Read(ref byte buffer, int size)
    {
        if (size < 1)
        {
            return;
        }

        int byteCount = 0;

        while (size > 0)
        {
            if (size >= sizeof(long))
            {
                Unsafe.As<byte, long>(ref Unsafe.Add(ref buffer, byteCount)) = ReadInt64();
                byteCount += sizeof(long);
                size -= sizeof(long);
                continue;
            }

            if (size >= sizeof(int))
            {
                Unsafe.As<byte, int>(ref Unsafe.Add(ref buffer, byteCount)) = ReadInt32();
                byteCount += sizeof(int);
                size -= sizeof(int);
                continue;
            }

            if (size >= sizeof(short))
            {
                Unsafe.As<byte, short>(ref Unsafe.Add(ref buffer, byteCount)) = ReadInt16();
                byteCount += sizeof(short);
                size -= sizeof(short);
                continue;
            }

            Unsafe.Add(ref buffer, byteCount) = ReadByte();
            ++byteCount;
            --size;
        }
    }

    /// <summary>
    /// Function to read a generic value from the stream.
    /// </summary>
    /// <typeparam name="T">Type of value to read.  Must be an unmanaged value type.</typeparam>
    /// <returns>The value in the stream.</returns>
    /// <remarks>
    /// <para>
    /// This method will read the data from the binary stream into a value of type <typeparamref name="T"/>, and return that value.
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
    public T ReadValue<T>()
        where T : unmanaged
    {
        ReadValue(out T result);

        return result;
    }

    /// <summary>
    /// Function to read a generic value from the stream.
    /// </summary>
    /// <typeparam name="T">Type of value to read.  Must be an unmanaged value type.</typeparam>
    /// <param name="result">The value from the stream.</param>
    /// <returns>The value in the stream.</returns>
    /// <remarks>
    /// <para>
    /// This method will read the data from the binary stream into a value of type <typeparamref name="T"/>, and return that value.
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
    public void ReadValue<T>(out T result)
        where T : unmanaged
    {
        result = default;
        Read(ref Unsafe.As<T, byte>(ref result), Unsafe.SizeOf<T>());
    }

    /// <summary>
    /// Function to read a range of generic values into a span.
    /// </summary>
    /// <typeparam name="T">Type of value to read. Must be an unmanaged value type.</typeparam>
    /// <param name="buffer">The span buffer to read data into.</param>
    /// <remarks>
    /// <para>
    /// This will read data from the binary stream into the specified span of type <typeparamref name="T"/>. 
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
    public void ReadRange<T>(Span<T> buffer)
        where T : unmanaged => Read(ref Unsafe.As<T, byte>(ref buffer[0]), Unsafe.SizeOf<T>() * buffer.Length);

    /// <summary>
    /// Function to read a range of generic values.
    /// </summary>
    /// <typeparam name="T">Type of value to read.  Must be an unmanaged value type.</typeparam>
    /// <param name="pointer">Pointer to memory that will store the values.</param>
    /// <param name="startIndex">[Optional] Starting index in the pointer.</param>
    /// <param name="count">[Optional] Number of elements to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="startIndex"/> parameter is less than 0.
    /// <para>-or-</para>
    /// <para>Thrown when the startIndex parameter is equal to or greater than the number of elements in the value parameter.</para>
    /// <para>-or-</para>
    /// <para>Thrown when the sum of startIndex and <paramref name="count"/> is greater than the number of elements in the value parameter.</para>
    /// </exception>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    /// <remarks>
    /// <para>
    /// This will read data from the binary stream into the specified memory pointer containing values of type <typeparamref name="T"/>. The values will be populated starting at the 
    /// <paramref name="startIndex"/> up to the <paramref name="count"/> specified. If the <paramref name="count"/> is not specified (i.e. it is <b>null</b>), then the entire memory block minus 
    /// the <paramref name="startIndex"/> will be used.
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
    public void ReadRange<T>(GorgonPtr<T> pointer, int startIndex = 0, int? count = null)
        where T : unmanaged
    {
        if (pointer == GorgonPtr<T>.NullPtr)
        {
            throw new ArgumentNullException(nameof(pointer));
        }

        count ??= pointer.Length - startIndex;

        if (startIndex < 0)
        {
            throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, startIndex, 0));
        }

        if (startIndex >= pointer.Length)
        {
            throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_VALUE_IS_GREATER_THAN, startIndex, pointer.Length));
        }

        if ((pointer.Length == 0) || (count <= 0))
        {
            return;
        }

        if (startIndex + count > pointer.Length)
        {
            throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, startIndex + count, pointer.Length));
        }

        unsafe
        {
            Read((pointer + startIndex), Unsafe.SizeOf<T>() * count.Value);
        }
    }

    /// <summary>
    /// Function to read a range of generic values.
    /// </summary>
    /// <typeparam name="T">Type of value to read.  Must be an unmanaged value type.</typeparam>
    /// <param name="value">Array of values to read.</param>
    /// <param name="startIndex">[Optional] Starting index in the array.</param>
    /// <param name="count">[Optional] Number of array elements to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="startIndex"/> parameter is less than 0.
    /// <para>-or-</para>
    /// <para>Thrown when the startIndex parameter is equal to or greater than the number of elements in the value parameter.</para>
    /// <para>-or-</para>
    /// <para>Thrown when the sum of startIndex and <paramref name="count"/> is greater than the number of elements in the value parameter.</para>
    /// </exception>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    /// <remarks>
    /// <para>
    /// This will read data from the binary stream into the specified array of values of type <typeparamref name="T"/>. The values will be populated starting at the <paramref name="startIndex"/> up to 
    /// the <paramref name="count"/> specified. If the <paramref name="count"/> is not specified (i.e. it is <b>null</b>), then the entire array minus the <paramref name="startIndex"/> will be used.
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
    public void ReadRange<T>(T[] value, int startIndex = 0, int? count = null)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(value);

        count ??= value.Length - startIndex;

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

        if (startIndex + count > value.Length)
        {
            throw new ArgumentOutOfRangeException(string.Format(Resources.GOR_ERR_VALUE_IS_LESS_THAN, startIndex + count, value.Length));
        }

        Read(ref Unsafe.As<T, byte>(ref value[startIndex]), Unsafe.SizeOf<T>() * count.Value);
    }

    /// <summary>
    /// Function to read a range of generic values.
    /// </summary>
    /// <typeparam name="T">Type of value to read.  Must be an unmanaged value type.</typeparam>
    /// <param name="count">Number of array elements to copy.</param>
    /// <returns>An array filled with values of type <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// <para>
    /// This method will read the specified <paramref name="count"/> of values of type <typeparamref name="T"/> into an array from a binary data stream and return that array.
    /// </para>
    /// <note type="important">
    /// <para>
    /// The return value for this type will always create a new array of type <typeparamref name="T"/>. This may be inefficient depending on the use case.
    /// </para>
    /// </note>
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
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public T[] ReadRange<T>(int count)
        where T : unmanaged
    {
        T[] array = new T[count];

        ReadRange(array, 0, count);

        return array;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonBinaryReader"/> class.
    /// </summary>
    /// <param name="input">Input stream.</param>
    /// <param name="keepStreamOpen">[Optional] <b>true</b> to keep the underlying stream open when the writer is closed, <b>false</b> to close when done.</param>
    public GorgonBinaryReader(Stream input, bool keepStreamOpen = false)
        : this(input, Encoding.UTF8, keepStreamOpen)
    {
    }
}
