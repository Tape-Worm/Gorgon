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
// Created: March 29, 2018 11:54:42 AM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Math;
using Gorgon.Properties;
using DX = SharpDX;

namespace Gorgon.Native;

/// <summary>
/// Provides a buffer that uses native (unmanaged) memory to store its data.
/// </summary>
/// <typeparam name="T">The type of data to store in the buffer. Must be an unmanaged value type.</typeparam>
/// <remarks>
/// <para>
/// This buffer works similarly to an array or span type, only it is backed by unmanaged memory. This allows applications to use it for things like interop between native and managed code, or to have 
/// a buffer that allows the developer to control its lifetime. And, unlike a native pointer, it is safe for use because it will handle out of bounds errors correctly instead of just trashing memory 
/// arbitrarily.
/// </para>
/// <para>
/// Because this buffer is backed by native memory and the developer is repsonsible for its lifetime, it is important to ensure that the <see cref="IDisposable.Dispose"/> method is called on this 
/// object when it is no longer needed. Failure to do so may result in a memory leak.
/// <note type="tip">
/// The object will be finalized and the memory will ultimately be freed then, but this can still have a negative impact on the application. 
/// </note>
/// </para>
/// <para>
/// Accessing data in the buffer is done through an indexer on the object like an array or other <see cref="IList{T}"/> types. This indexer performs a <see langword="ref return">ref return</see> so 
/// its contents can be updated using references for optimal performance. For example:
/// <code lang="CSharp">
/// <![CDATA[
/// // Create a buffer of 10 DateTime objects
/// GorgonNativeBuffer<DateTime> dateInBuffer = new GorgonNativeBuffer<DateTime>(10);
/// 
/// // This will write today's date to the buffer at the second date/time index.
/// dateInBuffer[1] = DateTime.Now;
/// 
/// ref DateTime currentDateTime = ref dateInBuffer[1];
/// // This will write back today's date plus 5 years to the buffer.
/// currentDateTime = DateTime.Now.AddYears(5);
/// ]]>
/// </code>
/// </para>
/// <para>
/// The buffer can also be used to pin an array or value type and act on those items as native memory. Please note that the standard disclaimers about pinning still apply.
/// </para>
/// </remarks>
public sealed class GorgonNativeBuffer<T>
    : IDisposable, IReadOnlyList<T>
    where T : unmanaged
{
    #region Variables.
    // The pointer to unmanaged memory.
    private GorgonPtr<T> _memoryBlock;
    // A pinned array.
    private GCHandle _pinnedArray;
    // Flag to indicate that we allocated this memory ourselves.
    private readonly bool _ownsMemory;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return whether this buffer is an alias of a pointer.
    /// </summary>
    /// <remarks>
    /// This property will return <b>true</b> if the memory pointed at is not owned by this buffer. This is also the case when the <see cref="IsAlias"/> property is <b>true</b>.
    /// </remarks>
    public bool IsAlias => !_ownsMemory;

    /// <summary>
    /// Property to return whether this buffer is for a pinned type.
    /// </summary>
    public bool IsPinned => _pinnedArray.IsAllocated;

    /// <summary>
    /// Property to return the size, in bytes, for the type parameter <typeparamref name="T"/>.
    /// </summary>
    public int TypeSize
    {
        get;
    }

    /// <summary>
    /// Property to return the number of bytes allocated for this buffer.
    /// </summary>
    public int SizeInBytes => _memoryBlock.SizeInBytes;

    /// <summary>
    /// Property to return the number of items of type <typeparamref name="T"/> stored in this buffer.
    /// </summary>
    public int Length => _memoryBlock.Length;

    /// <summary>
    /// Property to return the pointer to the block of memory allocated to the buffer.
    /// </summary>
    public ref readonly GorgonPtr<T> Pointer => ref _memoryBlock;

    /// <summary>Gets the number of elements in the collection.</summary>
    int IReadOnlyCollection<T>.Count => Length;

    /// <summary>Gets the <typeparamref name="T"/> at the specified index.</summary>
    T IReadOnlyList<T>.this[int index] => this[index];

    /// <summary>
    /// Property to return a reference to the item located at the specified index.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is less than 0, or greater than/equal to <see cref="Length"/>.</exception>
    public ref T this[int index] => ref _memoryBlock[index];
    #endregion

    #region Methods.
    /// <summary>
    /// Function to validate parameters used to create a native buffer from a managed array.
    /// </summary>
    /// <typeparam name="TArrayType">The type of element in the array.</typeparam>
    /// <param name="array">The array to validate.</param>
    /// <param name="index">The index within in the array to map to the buffer.</param>
    /// <param name="count">The number of items in the array to map to the buffer.</param>
    internal static void ValidateArrayParams<TArrayType>(TArrayType[] array, int index, int count)
    {
        if (array is null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        if ((index + count) > array.Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, index, count));
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is <b>not</b> thread safe.
    /// </para>
    /// </remarks>
    public void Dispose()
    {
        // Unpin the type we've pinned.
        if (_pinnedArray.IsAllocated)
        {
            _pinnedArray.Free();
            _memoryBlock = GorgonPtr<T>.NullPtr;
        }

        // Deallocate memory that we own.
        if ((!_ownsMemory)
            || (_memoryBlock == GorgonPtr<T>.NullPtr))
        {
            return;
        }

        DX.Utilities.FreeMemory(_memoryBlock);
        GC.RemoveMemoryPressure(SizeInBytes);
        _memoryBlock = GorgonPtr<T>.NullPtr;
    }

    /// <summary>
    /// Function to interpret a reference to the value at the index as the specified type.
    /// </summary>
    /// <typeparam name="TCastType">The type to cast to. Must be an unmanaged value type.</typeparam>
    /// <param name="offset">[Optional] The offset, in bytes, within the memory pointed at this pointer to start at.</param>
    /// <returns>The value in the buffer, casted to the required type.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> parameter is less than 0, or greater than/equal to <see cref="SizeInBytes"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method returns a reference to the value in the buffer, so applications can immediately write back to the value and have it reflected in the buffer:
    /// <code lang="csharp">
    /// <![CDATA[
    /// GorgonNativeBuffer<int> buffer = ...;
    /// byte newValue = 123;
    /// 
    /// // This will write the byte value 123 at the 2nd byte in the first integer (since the buffer expects a int values).
    /// buffer.AsRef<byte>(1) = newValue;
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    public ref TCastType AsRef<TCastType>(int offset = 0)
        where TCastType : unmanaged => ref _memoryBlock.AsRef<TCastType>(offset);

    /// <summary>
    /// Function to copy the contents of this buffer into other.
    /// </summary>
    /// <param name="destination">The destination buffer that will receive the data.</param>
    /// <param name="sourceIndex">[Optional] The first index to start copying from.</param>
    /// <param name="count">[Optional] The number of items to copy.</param>
    /// <param name="destIndex">[Optional] The destination index in the destination buffer to start copying into.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destination"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sourceIndex"/>, or the <paramref name="destIndex"/> parameter is less than 0.</exception>
    /// <exception cref="ArgumentException">
    /// <para>Thrown when the <paramref name="sourceIndex"/> + <paramref name="count"/> is too big for this buffer.</para>
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="destIndex"/> + <paramref name="count"/> is too big for the <paramref name="destination"/> buffer.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// If the <paramref name="count"/> parameter is ommitted, then the full length of the source buffer, minus the <paramref name="sourceIndex"/> is used. Ensure that there is enough space in the 
    /// <paramref name="destination"/> buffer to accomodate the amount of data required.
    /// </para>
    /// </remarks>
    public void CopyTo(GorgonNativeBuffer<T> destination, int sourceIndex = 0, int? count = null, int destIndex = 0) 
    {
        if (destination is null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        _memoryBlock.CopyTo(destination._memoryBlock, sourceIndex, count, destIndex);
    }

    /// <summary>
    /// Function to copy the contents of this buffer into an array of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="destination">The destination array that will receive the data.</param>
    /// <param name="sourceIndex">[Optional] The first index to start copying from.</param>
    /// <param name="count">[Optional] The number of items to copy.</param>
    /// <param name="destIndex">[Optional] The destination index in the destination array to start copying into.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destination"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sourceIndex"/>, or the <paramref name="destIndex"/> parameter is less than 0.</exception>
    /// <exception cref="ArgumentException">
    /// <para>Thrown when the <paramref name="sourceIndex"/> + <paramref name="count"/> is too big for this buffer.</para>
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="destIndex"/> + <paramref name="count"/> is too big for the <paramref name="destination"/> buffer.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// If the <paramref name="count"/> parameter is ommitted, then the full length of the source buffer, minus the <paramref name="sourceIndex"/> is used. Ensure that there is enough space in the 
    /// <paramref name="destination"/> buffer to accomodate the amount of data required.
    /// </para>
    /// </remarks>
    public void CopyTo(T[] destination, int sourceIndex = 0, int? count = null, int destIndex = 0)
    {
        if (destination is null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        _memoryBlock.CopyTo(destination, sourceIndex, count, destIndex);
    }

    /// <summary>
    /// Function to pin an array and access its contents natively.
    /// </summary>
    /// <param name="array">The array to pin.</param>
    /// <param name="index">[Optional] The starting index in the array to pin.</param>
    /// <param name="count">[Optional] The number of items in the array to pin.</param>
    /// <returns>A new <see cref="GorgonNativeBuffer{T}"/> containing the pinned contents of the array.</returns>
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
    public static GorgonNativeBuffer<T> Pin(T[] array, int index = 0, int? count = null)
    {
        if (count is null)
        {
            count = array.Length - index;
        }
        
        ValidateArrayParams(array, index, count.Value);

        int typeSize = Unsafe.SizeOf<T>();
        var handle = GCHandle.Alloc(array, GCHandleType.Pinned);

        return new GorgonNativeBuffer<T>(handle, index * typeSize, count.Value * typeSize);
    }

    /// <summary>
    /// Function to pin a value type of type <typeparamref name="T"/>, and access its contents as a byte buffer.
    /// </summary>
    /// <param name="valueType">The value to pin</param>
    /// <returns>A new <see cref="GorgonNativeBuffer{T}"/> containing the contents of the value as a series of byte values.</returns>
    /// <remarks>
    /// <para>
    /// This allows access to the value passed to <paramref name="valueType"/> as a series bytes for native manipulation.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// This method <b>pins</b> the <paramref name="valueType"/>, which can cause performance issues with the garbage collector. Applications should only pin their objects for a very short time for 
    /// best performance.
    /// </note>
    /// </para>
    /// </remarks>
    public static GorgonNativeBuffer<byte> PinAsByteBuffer(ref T valueType)
    {
        int srcSize = Unsafe.SizeOf<T>();
        var handle = GCHandle.Alloc(valueType, GCHandleType.Pinned);

        return new GorgonNativeBuffer<byte>(handle, 0, srcSize);
    }

    /// <summary>
    /// Function to copy the contents of this buffer into a stream.
    /// </summary>
    /// <param name="stream">The stream to write into.</param>
    /// <param name="startIndex">[Optional] The index in the buffer to start copying from.</param>
    /// <param name="count">[Optional] The maximum number of items to read from the stream.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
    /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is read only.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="startIndex"/> is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="startIndex"/> + <paramref name="count"/> are equal to or greater than the <see cref="Length"/>.</exception>
    /// <remarks>
    /// <para>
    /// If the <paramref name="count"/> parameter is ommitted, then the full length of the source buffer, minus the <paramref name="startIndex"/> is used. Ensure that there is enough space in the 
    /// <paramref name="stream"/> to accomodate the amount of data required.
    /// </para>
    /// </remarks>
    /// <seealso cref="ToStream(int, int?)"/>
    public void CopyTo(Stream stream, int startIndex = 0, int? count = null) => _memoryBlock.CopyTo(stream, startIndex, count);

    /// <summary>
    /// Function to copy the contents of this buffer into a span.
    /// </summary>
    /// <param name="span">The span to write into.</param>
    /// <param name="index">[Optional] The offset within the buffer to start reading from.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is less than 0.</exception>
    public void CopyTo(Span<T> span, int index = 0) => _memoryBlock.CopyTo(span, index);

    /// <summary>
    /// Function to copy the contents of this buffer into a span.
    /// </summary>
    /// <param name="memory">The span to write into.</param>
    /// <param name="index">[Optional] The offset within the buffer to start reading from.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is less than 0.</exception>
    public void CopyTo(Memory<T> memory, int index = 0) => CopyTo(memory.Span, index);

    /// <summary>
    /// Function to cast the type in this buffer to another type.
    /// </summary>
    /// <typeparam name="TCastType">The type to cast to. Must be an unmanaged value type.</typeparam>
    /// <returns>A new <see cref="GorgonNativeBuffer{TCastType}"/> pointing at the same memory.</returns>
    /// <remarks>
    /// <para>
    /// This method will cast the contents of the buffer from their original type to another type specified by <typeparamref name="TCastType"/>. 
    /// </para>
    /// <para>
    /// The returned buffer will be a new buffer object, but it will point at the same memory location. It will have the same <see cref="SizeInBytes"/> as the original buffer, but its 
    /// <see cref="Length"/> will be updated to reflect the number of items at the specified <typeparamref name="TCastType"/>.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// If the size (in bytes) of the <typeparamref name="TCastType"/> is not evenly divisble by the <see cref="SizeInBytes"/>, then the buffer returned will not cover the entire size of the original 
    /// buffer.
    /// </note>
    /// </para>
    /// </remarks>
    public GorgonNativeBuffer<TCastType> To<TCastType>()
        where TCastType : unmanaged => new(_memoryBlock.To<TCastType>());

    /// <summary>
    /// Implicit operator to return the pointer to the underlying data in the buffer.
    /// </summary>
    /// <param name="buffer">The buffer to retrieve the native pointer from.</param>
    /// <returns>The void pointer to the underlying data in the buffer.</returns>
    /// <remarks>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// This operator returns the pointer to the memory address of this buffer. Developers should only use this for interop scenarios where a native call needs a pointer. Manipulation of this pointer is 
    /// not advisable and may cause harm. 
    /// </para>
    /// <para>
    /// No safety checks are done on this pointer, and as such, memory corruption is possible if the pointer is used without due care.
    /// </para>
    /// <para>
    /// <h2><font color="#FF0000">Use this at your own risk.</font></h2>
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public static unsafe explicit operator void*(GorgonNativeBuffer<T> buffer) => ToPointer(buffer);

    /// <summary>
    /// Function to return the pointer to the underlying data in the buffer.
    /// </summary>
    /// <param name="buffer">The buffer to retrieve the native pointer from.</param>
    /// <returns>The void pointer to the underlying data in the buffer.</returns>
    /// <remarks>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// This method returns the pointer to the memory address of this buffer. Developers should only use this for interop scenarios where a native call needs a pointer. Manipulation of this pointer is 
    /// not advisable and may cause harm. 
    /// </para>
    /// <para>
    /// No safety checks are done on this pointer, and as such, memory corruption is possible if the pointer is used without due care.
    /// </para>
    /// <para>
    /// <h2><font color="#FF0000">Use this at your own risk.</font></h2>
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public static unsafe void* ToPointer(GorgonNativeBuffer<T> buffer) => buffer is null ? null : (void *)buffer._memoryBlock;

    /// <summary>
    /// Implicit operator to return the pointer to the underlying data in the buffer.
    /// </summary>
    /// <param name="buffer">The buffer to retrieve the native pointer from.</param>
    /// <returns>The void pointer to the underlying data in the buffer.</returns>
    /// <remarks>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// This operator returns the pointer to the memory address of this buffer. Developers should only use this for interop scenarios where a native call needs a pointer. Manipulation of this pointer is 
    /// not advisable and may cause harm. 
    /// </para>
    /// <para>
    /// No safety checks are done on this pointer, and as such, memory corruption is possible if the pointer is used without due care.
    /// </para>
    /// <para>
    /// <h2><font color="#FF0000">Use this at your own risk.</font></h2>
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public static explicit operator nint(GorgonNativeBuffer<T> buffer) => ToNint(buffer);

    /// <summary>
    /// Function to return the pointer to the underlying data in the buffer.
    /// </summary>
    /// <param name="buffer">The buffer to retrieve the native pointer from.</param>
    /// <returns>The void pointer to the underlying data in the buffer.</returns>
    /// <remarks>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// This method returns the pointer to the memory address of this buffer. Developers should only use this for interop scenarios where a native call needs a pointer. Manipulation of this pointer is 
    /// not advisable and may cause harm. 
    /// </para>
    /// <para>
    /// No safety checks are done on this pointer, and as such, memory corruption is possible if the pointer is used without due care.
    /// </para>
    /// <para>
    /// <h2><font color="#FF0000">Use this at your own risk.</font></h2>
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public static nint ToNint(GorgonNativeBuffer<T> buffer) => buffer is null ? IntPtr.Zero : buffer._memoryBlock;

    /// <summary>
    /// Operator to implicitly convert this buffer to a span.
    /// </summary>
    /// <param name="buffer">The buffer to convert.</param>
    public static explicit operator Span<T>(GorgonNativeBuffer<T> buffer) => ToSpan(buffer);

    /// <summary>
    /// Function to access a native buffer as a span.
    /// </summary>
    /// <param name="buffer">The buffer to access.</param>
    /// <returns>A span for the buffer.</returns>
    public static Span<T> ToSpan(GorgonNativeBuffer<T> buffer) => buffer is null ? default : buffer._memoryBlock.ToSpan();

    /// <summary>
    /// Operator to implicitly convert this buffer to a span.
    /// </summary>
    /// <param name="buffer">The buffer to convert.</param>
    public static explicit operator ReadOnlySpan<T>(GorgonNativeBuffer<T> buffer) => ToReadOnlySpan(buffer);

    /// <summary>
    /// Function to access a native buffer as a read only span.
    /// </summary>
    /// <param name="buffer">The buffer to access.</param>
    /// <returns>A span for the buffer.</returns>
    public static ReadOnlySpan<T> ToReadOnlySpan(GorgonNativeBuffer<T> buffer) => buffer is null ? default : buffer.ToSpan();

    /// <summary>
    /// Function to access a native buffer as a span slice.
    /// </summary>
    /// <param name="index">[Optional] The index of the item to start slicing at.</param>
    /// <param name="count">[Optional] The number of items to slice.</param>
    /// <returns>A span for the buffer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameter is less than 0.</exception>
    public Span<T> ToSpan(int index = 0, int? count = null) => _memoryBlock.ToSpan(index, count);

    /// <summary>
    /// Function to access a native buffer as a read only span slice.
    /// </summary>
    /// <param name="index">The index of the item to start slicing at.</param>
    /// <param name="count">[Optional] The number of items to slice.</param>
    /// <returns>A span for the buffer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameter is less than 0.</exception>
    public ReadOnlySpan<T> ToReadOnlySpan(int index, int? count = null) => _memoryBlock.ToReadOnlySpan(index, count);

    /// <summary>
    /// Function to fill the buffer with a specific value.
    /// </summary>
    /// <param name="clearValue">The value used to fill the buffer.</param>
    public void Fill(byte clearValue) => _memoryBlock.Fill(clearValue);

    /// <summary>
    /// Operator to convert this buffer to a <see cref="GorgonPtr{T}"/>.
    /// </summary>
    /// <param name="buffer">The buffer to convert.</param>
    /// <returns>The <see cref="GorgonPtr{T}"/> wrapping the buffer data.</returns>
    public static implicit operator GorgonPtr<T>(GorgonNativeBuffer<T> buffer) => buffer?._memoryBlock ?? GorgonPtr<T>.NullPtr;

    /// <summary>
    /// Function to return the underlying <see cref="GorgonPtr{T}"/> for this buffer.
    /// </summary>
    /// <param name="buffer">The buffer to containing the pointer.</param>
    /// <returns>The underlying <see cref="GorgonPtr{T}"/> for the buffer.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is <b>null</b>.</exception>
    public static GorgonPtr<T> ToGorgonPtr(GorgonNativeBuffer<T> buffer) => buffer is null
            ? throw new ArgumentNullException(nameof(buffer))
            : buffer._memoryBlock;

    /// <summary>
    /// Function to return a stream wrapping this buffer.
    /// </summary>
    /// <param name="index">[Optional] The index in the buffer to map to the beginning of the stream.</param>
    /// <param name="count">[Optional] The number of items to wrap in the stream.</param>
    /// <returns>A new <see cref="Stream"/> which will wrap the contents of the buffer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameter is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> plus the <paramref name="count"/> exceeds the size of the buffer.</exception>
    /// <remarks>
    /// <para>
    /// This method takes the unmanaged memory used by the buffer and wraps it in a <see cref="Stream"/>. The stream returned is not a copy of the memory used by the buffer, so it is important to 
    /// ensure that the stream lifetime is managed in conjunction with the lifetime of the buffer. Disposing of the buffer and using the returned stream will result in undefined behavior and potential 
    /// memory access violations.
    /// </para>
    /// <para>
    /// A portion of the buffer can be wrapped by the stream by supplying a value for <paramref name="index"/> and/or <paramref name="count"/>.  If the count is omitted, all data from 
    /// <paramref name="index"/> up to the end of the buffer is wrapped by the stream.  Likewise, if <paramref name="index"/> is omitted, all data from the beginning of the buffer up to the 
    /// <paramref name="count"/> is wrapped. If no parameters are supplied, then the entire buffer is wrapped.
    /// </para>
    /// </remarks>        
    public Stream ToStream(int index = 0, int? count = null) => _memoryBlock.ToStream(index, count);

    /// <summary>Returns an enumerator that iterates through the collection.</summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Length; ++i)
        {
            yield return this[i];
        }
    }

    /// <summary>Returns an enumerator that iterates through a collection.</summary>
    /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonNativeBuffer{T}" /> class.
    /// </summary>
    /// <param name="pinnedData">The handle to the pinned data.</param>
    /// <param name="offset">The offset within the pinned data to access.</param>
    /// <param name="size">The size, in bytes, of the pinned data.</param>
    private GorgonNativeBuffer(GCHandle pinnedData, int offset, int size)
    {
        TypeSize = Unsafe.SizeOf<T>();
        _pinnedArray = pinnedData;
        _memoryBlock = new GorgonPtr<T>(_pinnedArray.AddrOfPinnedObject() + offset, size);            
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonNativeBuffer{T}" /> class.
    /// </summary>
    /// <param name="count">The number of items of type <typeparamref name="T"/> to allocate in the buffer.</param>
    /// <param name="alignment">[Optional] The alignment of the buffer, in bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="count"/> is less than 0.</exception>
    /// <remarks>
    /// <para>
    /// Use this constructor to create a new buffer backed by native memory of a given size and aligned to a boundary for the most efficient memory access. The contents of this memory are 
    /// automatically cleared on allocation.
    /// </para>
    /// </remarks>
    public GorgonNativeBuffer(int count, int alignment = 16)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
        }

        TypeSize = Unsafe.SizeOf<T>();
        _memoryBlock = new GorgonPtr<T>(DX.Utilities.AllocateClearedMemory(TypeSize * count, align: alignment.Max(0)), count);
        _ownsMemory = true;

        GC.AddMemoryPressure(SizeInBytes);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonNativeBuffer{T}" /> class.
    /// </summary>
    /// <param name="pointer">The pointer to wrap with the buffer interface.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This construct is used to alias a <see cref="GorgonPtr{T}"/>. Because <paramref name="pointer"/> is aliased, the lifetime for the pointer is not controlled by the buffer and therefore 
    /// is the responsibility of the user. If the <paramref name="pointer"/> memory is freed before the buffer is disposed, then undefined behavior will occur.
    /// </para>
    /// </remarks>
    public GorgonNativeBuffer(GorgonPtr<T> pointer)
    {
        if (pointer == GorgonPtr<T>.NullPtr)
        {
            throw new ArgumentNullException(nameof(pointer));
        }

        _memoryBlock = pointer;
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="GorgonNativeBuffer{T}" /> class.
    /// </summary>
    ~GorgonNativeBuffer()
    {
        Dispose();
    }
    #endregion
}
