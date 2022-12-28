#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: December 1, 2020 9:29:00 PM
// 
#endregion

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Properties;
using DX = SharpDX;

namespace Gorgon.Native;

/// <summary>
/// A value type representing a pointer to native (unmanaged) memory.
/// </summary>
/// <remarks>
/// <para>
/// This is a pointer access type that allows safe access to pre-existing blocks of native memory. It does this by wrapping other the native pointer to the memory block and provides safety checks to 
/// ensure nothing goes out of bounds when accessing the memory pointed at by the pointer. 
/// </para>
/// <para>
/// Developers may use this pointer type like a regular native pointer and increment or decrement it the usual way: <c>gorPtr++, gorPtr--</c>. And beyond that, many functions are available to allow 
/// copying of the memory pointed at by the pointer to another type of data. 
/// </para>
/// <para>
/// This pointer type only wraps a native pointer to previously allocated memory, therefore it does <b>not</b> perform any memory allocation on its own. Gorgon includes the 
/// <see cref="GorgonNativeBuffer{T}"/> for that purpose. The <see cref="GorgonNativeBuffer{T}"/> will implicitly convert to this type, so it can be used in situations where this type is required.
/// </para>
/// <para>
/// <note type="information">
/// <para>
/// This type is suitable when the type of data stored in native memory is known. If the type of data in memory is not known (e.g. <c>void *</c>), or refers to an opaque handle (e.g. <c>HWND</c>), 
/// an <c>nint</c> should be used instead. 
/// </para>
/// </note>
/// </para>
/// <para>
/// <note type="important">
/// <para>
/// This type is ~3x slower for access than a regular native pointer (x64). This is due to the safety features available to ensure the pointer does not cause a buffer over/underrun. For pure speed, 
/// nothing beats a native pointer (or <c>nint</c>) and if your code is sensitive to microsecond timings (i.e. it needs to be near realtime/blazing fast), then use a native pointer instead 
/// (developers can cast this type to a native pointer). But do so with the understanding that all safety is off and memory corruption is a very real possibility.
/// </para>
/// <para>
/// <h3>Before making a choice, ALWAYS profile your application with a profiler. Never assume that the fastest functionality is required when memory safety is on the line.</h3>
/// </para>
/// </note>
/// </para>
/// </remarks>
/// <seealso cref="GorgonNativeBuffer{T}"/>
[StructLayout(LayoutKind.Sequential, Size = 16)]
public unsafe readonly struct GorgonPtr<T>
    : IEquatable<GorgonPtr<T>>, IComparable<GorgonPtr<T>>
    where T : unmanaged
{
    #region Variables.
    /// <summary>
    /// Represents a null pointer.
    /// </summary>
    public static readonly GorgonPtr<T> NullPtr = default;

    // The actual unsafe pointer to the memory.
    private readonly T* _ptr;

    // The offset in indices for the pointer from the beginning of memory.
    private readonly int _index;

    /// <summary>
    /// The number of items of type <typeparamref name="T"/> stored within the memory block. 
    /// </summary>
    public readonly int Length;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the size, in bytes, of the type represented by <typeparamref name="T"/>.
    /// </summary>
    public int TypeSize => sizeof(T);

    /// <summary>
    /// Property to return the total size, in bytes, of the memory pointed at by this pointer.
    /// </summary>
    public int SizeInBytes => Length * TypeSize;

    /// <summary>
    /// Property to return a reference to the item located at the specified index.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is less than 0, or greater than/equal to <see cref="Length"/>.</exception>
    /// <remarks>
    /// <para>
    /// This property will return the value as a reference, and as such, it can be assigned to as well. For example:
    /// <code lang="csharp">
    /// <![CDATA[
    /// GorgonPtr<int> ptr = ...;
    /// int newValue = 123;
    /// 
    /// ptr[2] = newValue;
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    public ref T this[int index]
    {
        get
        {
            if (_ptr == null)
            {
                throw new NullReferenceException();
            }

            if ((index < 0) || (index >= Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index), string.Format(Resources.GOR_ERR_INDEX_OUT_OF_RANGE, index, 0, Length));
            }
            
            return ref Unsafe.AsRef<T>(_ptr + index);
        }
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Operator to determine if one pointer address is less than another.
    /// </summary>
    /// <param name="left">The left side pointer to compare.</param>
    /// <param name="right">The right side pointer to compare.</param>
    /// <returns><b>true</b> if left is less than right, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(GorgonPtr<T> left, GorgonPtr<T> right) => left._ptr < right._ptr;

    /// <summary>
    /// Operator to determine if one pointer address is greater than another.
    /// </summary>
    /// <param name="left">The left side pointer to compare.</param>
    /// <param name="right">The right side pointer to compare.</param>
    /// <returns><b>true</b> if left is greater than right, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(GorgonPtr<T> left, GorgonPtr<T> right) => left._ptr > right._ptr;

    /// <summary>
    /// Operator to determine if one pointer address is less than or equal to another.
    /// </summary>
    /// <param name="left">The left side pointer to compare.</param>
    /// <param name="right">The right side pointer to compare.</param>
    /// <returns><b>true</b> if left is less than or equal to right, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(GorgonPtr<T> left, GorgonPtr<T> right) => left._ptr <= right._ptr;

    /// <summary>
    /// Operator to determine if one pointer address is greater than or equal to another.
    /// </summary>
    /// <param name="left">The left side pointer to compare.</param>
    /// <param name="right">The right side pointer to compare.</param>
    /// <returns><b>true</b> if left is greater than or equal to right, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(GorgonPtr<T> left, GorgonPtr<T> right) => left._ptr >= right._ptr;

    /// <summary>
    /// Operator compare two pointer addresses for equality.
    /// </summary>
    /// <param name="left">The left side pointer to compare.</param>
    /// <param name="right">The right side pointer to compare.</param>
    /// <returns><b>true</b> if the two pointers point at the same address, <b>false</b> if not.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(GorgonPtr<T> left, GorgonPtr<T> right) => left._ptr == right._ptr;

    /// <summary>
    /// Operator compare two pointer addresses for inequality.
    /// </summary>
    /// <param name="left">The left side pointer to compare.</param>
    /// <param name="right">The right side pointer to compare.</param>
    /// <returns><b>true</b> if the two pointers do not point at the same address, <b>false</b> if they do.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(GorgonPtr<T> left, GorgonPtr<T> right) => left._ptr != right._ptr;

    /// <summary>
    /// Operator to implicitly convert this pointer to a span.
    /// </summary>
    /// <param name="ptr">The buffer to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Span<T>(GorgonPtr<T> ptr) => ToSpan(ptr);

    /// <summary>
    /// Operator to implicitly convert this pointer to a span.
    /// </summary>
    /// <param name="ptr">The buffer to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<T>(GorgonPtr<T> ptr) => ToSpan(ptr);

    /// <summary>
    /// Operator to convert this SharpDX data buffer into a <see cref="GorgonPtr{T}"/>.
    /// </summary>
    /// <param name="buffer">The data buffer to convert.</param>
    /// <returns>A pointer wrapping the buffer data.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator GorgonPtr<T>(DX.DataBuffer buffer) => new(buffer);

    /// <summary>
    /// Operator to convert this SharpDX data stream into a <see cref="GorgonPtr{T}"/>..
    /// </summary>
    /// <param name="stream">The data stream convert.</param>
    /// <returns>The pointer wrapping the stream data.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator GorgonPtr<T>(DX.DataStream stream) => new(stream);

    /// <summary>
    /// Operator to convert this SharpDX data pointer.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <returns>A pointer wrapping the SharpDX Pointer data.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator GorgonPtr<T>(DX.DataPointer ptr) => new(ptr);

    /// <summary>
    /// Operator to convert this buffer to a SharpDX data buffer.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <returns>A SharpDX DataBuffer wrapping the buffer data.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator DX.DataBuffer(GorgonPtr<T> ptr) => ToDataBuffer(ptr);

    /// <summary>
    /// Operator to convert this pointer to a SharpDX data stream.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <returns>The SharpDX DataStream wrapping the pointer data.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator DX.DataStream(GorgonPtr<T> ptr) => ToDataStream(ptr);

    /// <summary>
    /// Operator to convert this pointer to a SharpDX data pointer.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <returns>A SharpDX DataPointer wrapping the pointer data.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator DX.DataPointer(GorgonPtr<T> ptr) => ToDataPointer(ptr);

    /// <summary>
    /// Operator to convert this pointer to a native pointer.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <returns>A native pointer.</returns>
    /// <remarks>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// This operator returns the pointer to the memory address of this pointer. Developers should only use this for interop scenarios where a native call needs a pointer. Manipulation of this pointer is 
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator void*(GorgonPtr<T> ptr) => ToPointer(ptr);

    /// <summary>
    /// Operator to convert this pointer to a <see cref="long"/> value representing the memory address of the block of memory being pointed at.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <returns>The memory address as a <see cref="long"/> value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator long(GorgonPtr<T> ptr) => (long)ptr._ptr;

    /// <summary>
    /// Operator to convert this pointer to a <see cref="ulong"/> value representing the memory address of the block of memory being pointed at.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <returns>The memory address as a <see cref="ulong"/> value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator ulong(GorgonPtr<T> ptr) => (ulong)ptr._ptr;

    /// <summary>
    /// Operator to convert this pointer to a native pointer.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <returns>A native pointer.</returns>
    /// <remarks>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// This operator returns the pointer to the memory address of this pointer. Developers should only use this for interop scenarios where a native call needs a pointer. Manipulation of this pointer is 
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator nint(GorgonPtr<T> ptr) => Tonint(ptr);

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(GorgonPtr<T> other) => _ptr == other._ptr;

    /// <summary>
    /// Function to cast this pointer to another pointer type.
    /// </summary>
    /// <typeparam name="Tc">The type to convert to. Must be an unmanaged value type.</typeparam>
    /// <returns>The casted pointer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonPtr<Tc> To<Tc>() where Tc : unmanaged => new((Tc *)_ptr, SizeInBytes / Unsafe.SizeOf<Tc>());

    /// <summary>
    /// Operator to increment the pointer by the given index offset.
    /// </summary>
    /// <param name="ptr">The pointer to increment.</param>
    /// <param name="indexOffset">The number of indices to offset by.</param>
    /// <returns>A new <see cref="GorgonPtr{T}"/> starting at the updated index offset.</returns>
    /// <remarks>
    /// <para>
    /// If the pointer is incremented beyond the beginning, or end of the memory block that it points at, then the return value will be <see cref="GorgonPtr{T}.NullPtr"/> rather than throw an exception. 
    /// This is done this way for performance reasons. So users should check that their pointer is not <b>null</b> when iterating to ensure that the pointer is still valid.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPtr<T> operator +(in GorgonPtr<T> ptr, int indexOffset) => Add(in ptr, indexOffset);

    /// <summary>
    /// Function to subtract two pointers to return the number of bytes between them.
    /// </summary>
    /// <param name="left">The left pointer to subtract.</param>
    /// <param name="right">The right pointer to subtract.</param>
    /// <returns>The difference in bytes between the two pointers.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long operator -(in GorgonPtr<T> left, in GorgonPtr<T> right) => Subtract(in left, in right);

    /// <summary>
    /// Operator to decrement the pointer by the given index offset.
    /// </summary>
    /// <param name="ptr">The pointer to decrement.</param>
    /// <param name="indexOffset">The number of indices to offset by.</param>
    /// <returns>A new <see cref="GorgonPtr{T}"/> starting at the updated index offset.</returns>
    /// <remarks>
    /// <para>
    /// If the pointer is incremented beyond the beginning, or end of the memory block that it points at, then the return value will be <see cref="GorgonPtr{T}.NullPtr"/> rather than throw an exception. 
    /// This is done this way for performance reasons. So users should check that their pointer is not <b>null</b> when iterating to ensure that the pointer is still valid.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPtr<T> operator -(in GorgonPtr<T> ptr, int indexOffset) => Add(in ptr, -indexOffset);

    /// <summary>
    /// Operator to increment the pointer by one index.
    /// </summary>
    /// <param name="ptr">The pointer to increment.</param>
    /// <returns>A new <see cref="GorgonPtr{T}"/> starting at the updated index offset.</returns>
    /// <remarks>
    /// <para>
    /// If the pointer is incremented beyond the beginning, or end of the memory block that it points at, then the return value will be <see cref="GorgonPtr{T}.NullPtr"/> rather than throw an exception. 
    /// This is done this way for performance reasons. So users should check that their pointer is not <b>null</b> when iterating to ensure that the pointer is still valid.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPtr<T> operator ++(in GorgonPtr<T> ptr)
    {
        if (ptr == NullPtr)
        {
            return NullPtr;
        }

        int newIndex = ptr._index + 1;

        // If the new length is negative or zero, then we've extended beyond the length of the memory block.
        // For the sake of performance (yes, it's an actual hit), we'll return a null pointer instead of throwing.
        return (newIndex >= ptr.Length) ? NullPtr : new GorgonPtr<T>(ptr._ptr + 1, newIndex, ptr.Length - 1);
    }

    /// <summary>
    /// Operator to decrement the pointer by one index.
    /// </summary>
    /// <param name="ptr">The pointer to decrement.</param>
    /// <returns>A new <see cref="GorgonPtr{T}"/> starting at the updated index offset.</returns>
    /// <remarks>
    /// <para>
    /// If the pointer is incremented beyond the beginning, or end of the memory block that it points at, then the return value will be <see cref="GorgonPtr{T}.NullPtr"/> rather than throw an exception. 
    /// This is done this way for performance reasons. So users should check that their pointer is not <b>null</b> when iterating to ensure that the pointer is still valid.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPtr<T> operator --(in GorgonPtr<T> ptr)
    {
        if (ptr == NullPtr)
        {
            return NullPtr;
        }

        int newIndex = ptr._index - 1;

        // If the new length is negative or zero, then we've extended beyond the length of the memory block.
        // For the sake of performance (yes, it's an actual hit), we'll return a null pointer instead of throwing.
        return (newIndex < 0) ? NullPtr : new GorgonPtr<T>(ptr._ptr - 1, newIndex, ptr.Length + 1);
    }

    /// <summary>
    /// Function to subtract two pointers to return the number of bytes between them.
    /// </summary>
    /// <param name="left">The left pointer to subtract.</param>
    /// <param name="right">The right pointer to subtract.</param>
    /// <returns>The difference in bytes between the two pointers.</returns>
    public static long Subtract(in GorgonPtr<T> left, in GorgonPtr<T> right) => ((byte*)left._ptr) - ((byte*)right._ptr);

    /// <summary>
    /// Function to increment the pointer by the given index offset.
    /// </summary>
    /// <param name="ptr">The pointer to increment.</param>
    /// <param name="indexOffset">The number of indices to offset by.</param>
    /// <returns>A new <see cref="GorgonPtr{T}"/> starting at the updated index offset.</returns>
    /// <remarks>
    /// <para>
    /// If the pointer is incremented beyond the beginning, or end of the memory block that it points at, then the return value will be <see cref="GorgonPtr{T}.NullPtr"/> rather than throw an exception. 
    /// This is done this way for performance reasons. So users should check that their pointer is not <b>null</b> when iterating to ensure that the pointer is still valid.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPtr<T> Add(in GorgonPtr<T> ptr, int indexOffset)
    {
        if (ptr == NullPtr)
        {
            return NullPtr;
        }

        int newIndex = ptr._index + indexOffset;

        // If the new length is negative or zero, then we've extended beyond the length of the memory block.
        // For the sake of performance (yes, it's an actual hit), we'll return a null pointer instead of throwing.
        return (newIndex < 0) || (newIndex >= ptr.Length)
            ? NullPtr
            : new GorgonPtr<T>(ptr._ptr + indexOffset, newIndex, ptr.Length);
    }

    /// <summary>
    /// Function to decrement the pointer by the given index offset.
    /// </summary>
    /// <param name="ptr">The pointer to decrement.</param>
    /// <param name="indexOffset">The number of indices to offset by.</param>
    /// <returns>A new <see cref="GorgonPtr{T}"/> starting at the updated index offset.</returns>
    /// <remarks>
    /// <para>
    /// If the pointer is incremented beyond the beginning, or end of the memory block that it points at, then the return value will be <see cref="GorgonPtr{T}.NullPtr"/> rather than throw an exception. 
    /// This is done this way for performance reasons. So users should check that their pointer is not <b>null</b> when iterating to ensure that the pointer is still valid.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPtr<T> Subtract(in GorgonPtr<T> ptr, int indexOffset) => Add(ptr, -indexOffset);

    /// <summary>
    /// Function to return the pointer as a reference value.
    /// </summary>
    /// <typeparam name="Tc">The type of value. Must be an unmanaged value type, and can be different than <typeparamref name="T"/>.</typeparam>
    /// <param name="offset">[Optional] The offset, in bytes, within the memory pointed at this pointer to start at.</param>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> is less than 0, or greater than, or equal to, the <see cref="SizeInBytes"/>.</exception>
    /// <returns>The refernce to the value at the index.</returns>
    /// <remarks>
    /// <para>
    /// This is meant for converting the data to another type while accessing memory. If the type of data specified by <typeparamref name="T"/> is the same as <typeparamref name="Tc"/>, then use the 
    /// indexing property instead for better performance.
    /// </para>
    /// <para>
    /// This value is returned as a reference, and as such, it can be assigned to as well. For example:
    /// <code lang="csharp">
    /// <![CDATA[
    /// GorgonPtr<int> ptr = ...;
    /// byte newValue = 123;
    /// 
    /// // This will write the byte value 123 at the 2nd byte in the first integer (since the pointer expects a int values).
    /// ptr.AsRef<byte>(1) = newValue;
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Tc AsRef<Tc>(int offset = 0) where Tc : unmanaged
    {
        if (_ptr == null)
        {
            throw new NullReferenceException();
        }

        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        if (offset >= SizeInBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(SizeInBytes), string.Format(Resources.GOR_ERR_INDEX_OUT_OF_RANGE, 0, SizeInBytes));
        }

        return ref Unsafe.AsRef<Tc>(offset + (byte *)_ptr);
    }

    /// <summary>
    /// Function to fill the memory pointed at by this pointer with a specific value.
    /// </summary>
    /// <param name="clearValue">The value used to fill the memory.</param>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Fill(byte clearValue)
    {
        if (_ptr == null)
        {
            throw new NullReferenceException();
        }

        Unsafe.InitBlock(_ptr, clearValue, (uint)SizeInBytes);
    }

    /// <summary>
    /// Function to copy the memory pointed at by this pointer into another pointer.
    /// </summary>
    /// <param name="destination">The destination pointer that will receive the data.</param>
    /// <param name="sourceIndex">[Optional] The first index to start copying from.</param>
    /// <param name="count">[Optional] The number of items to copy.</param>
    /// <param name="destIndex">[Optional] The destination index in the destination pointer to start copying into.</param>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destination"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sourceIndex"/>, or the <paramref name="destIndex"/> parameter is less than 0.</exception>
    /// <exception cref="ArgumentException">
    /// <para>Thrown when the <paramref name="sourceIndex"/> + <paramref name="count"/> is too big for the memory block.</para>
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="destIndex"/> + <paramref name="count"/> is too big for the memory block.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// If the <paramref name="count"/> parameter is ommitted, then the full length of the memory block, minus the <paramref name="sourceIndex"/> is used. Ensure that there is enough space in the 
    /// <paramref name="destination"/> memory block to accomodate the amount of data required.
    /// </para>
    /// </remarks>
    public void CopyTo(in GorgonPtr<T> destination, int sourceIndex = 0, int? count = null, int destIndex = 0)
    {
        if (_ptr == null)
        {
            throw new NullReferenceException();
        }

        if (destination == NullPtr)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        if (sourceIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceIndex), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        if (destIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(destIndex), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        count ??= Length - sourceIndex;

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
        }

        if (sourceIndex + count.Value > Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, sourceIndex, count.Value));
        }

        if (destIndex + count.Value > destination.Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, destIndex, count.Value));
        }

        Unsafe.CopyBlock(destination._ptr + destIndex, _ptr + sourceIndex, (uint)(count * sizeof(T)));
    }

    /// <summary>
    /// Function to copy the contents of this buffer into other.
    /// </summary>
    /// <param name="destination">The destination buffer that will receive the data.</param>
    /// <param name="sourceIndex">[Optional] The first index to start copying from.</param>
    /// <param name="count">[Optional] The number of items to copy.</param>
    /// <param name="destIndex">[Optional] The destination index in the destination buffer to start copying into.</param>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
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
        if (_ptr == null)
        {
            throw new NullReferenceException();
        }

        if (destination == null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        if (sourceIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceIndex), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        if (destIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(destIndex), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        count ??= Length - sourceIndex;

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
        }

        if (sourceIndex + count.Value > Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, sourceIndex, count.Value));
        }

        if (destIndex + count.Value > destination.Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, destIndex, count.Value));
        }

        Unsafe.CopyBlock((T*)destination + destIndex, _ptr + sourceIndex, (uint)(count * sizeof(T)));
    }

    /// <summary>
    /// Function to copy the contents of memory pointed at by this pointer into an array of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="destination">The destination array that will receive the data.</param>
    /// <param name="sourceIndex">[Optional] The first index to start copying from.</param>
    /// <param name="count">[Optional] The number of items to copy.</param>
    /// <param name="destIndex">[Optional] The destination index in the destination array to start copying into.</param>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destination"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sourceIndex"/>, or the <paramref name="destIndex"/> parameter is less than 0.</exception>
    /// <exception cref="ArgumentException">
    /// <para>Thrown when the <paramref name="sourceIndex"/> + <paramref name="count"/> is too big for this memory block.</para>
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="destIndex"/> + <paramref name="count"/> is too big for the <paramref name="destination"/> memory block.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// If the <paramref name="count"/> parameter is ommitted, then the full length of the source memory block, minus the <paramref name="sourceIndex"/> is used. Ensure that there is enough 
    /// space in the <paramref name="destination"/> memory block to accomodate the amount of data required.
    /// </para>
    /// </remarks>
    public void CopyTo(T[] destination, int sourceIndex = 0, int? count = null, int destIndex = 0)
    {
        if (_ptr == null)
        {
            throw new NullReferenceException();
        }

        if (destination == null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        if (sourceIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceIndex), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        if (destIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(destIndex), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        count ??= Length - sourceIndex;

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
        }

        if (sourceIndex + count.Value > Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, sourceIndex, count.Value));
        }

        if (destIndex + count.Value > destination.Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, destIndex, count.Value));
        }

        fixed (T* destPtr = &destination[destIndex])
        {
            int typeSize = sizeof(T);
            Unsafe.CopyBlock(destPtr, _ptr + sourceIndex , (uint)(count * typeSize));
        }
    }

    /// <summary>
    /// Function to copy the contents of the memory pointed at this pointer into a stream.
    /// </summary>
    /// <param name="stream">The stream to write into.</param>
    /// <param name="startIndex">[Optional] The index in the pointer to start copying from.</param>
    /// <param name="count">[Optional] The maximum number of items to read.</param>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
    /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is read only.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="startIndex"/> is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="startIndex"/> + <paramref name="count"/> are equal to or greater than the <see cref="Length"/>.</exception>
    /// <remarks>
    /// <para>
    /// If the <paramref name="count"/> parameter is ommitted, then the full length of the memory block, minus the <paramref name="startIndex"/> is used. Ensure that there is enough space in the 
    /// <paramref name="stream"/> to accomodate the amount of data required.
    /// </para>
    /// </remarks>
    /// <seealso cref="ToStream(int, int?)"/>
    public void CopyTo(Stream stream, int startIndex = 0, int? count = null)
    {
        if (_ptr == null)
        {
            throw new NullReferenceException();
        }

        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanWrite)
        {
            throw new IOException(Resources.GOR_ERR_STREAM_IS_READONLY);
        }

        if (startIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        count ??= Length - startIndex;

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
        }

        if (startIndex + count.Value > Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, startIndex, count.Value));
        }

        using var writer = new GorgonBinaryWriter(stream, true);
        for (int i = 0; i < count.Value; ++i)
        {
            writer.WriteValue(ref this[i + startIndex]);
        }
    }

    /// <summary>
    /// Function to return a stream wrapping this pointer.
    /// </summary>
    /// <param name="index">[Optional] The index in the pointer memory to map to the beginning of the stream.</param>
    /// <param name="count">[Optional] The number of items to wrap in the stream.</param>
    /// <returns>A new <see cref="Stream"/> which will wrap the contents of the memory pointed at by this pointer.</returns>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameter is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> plus the <paramref name="count"/> exceeds the size of the memory block.</exception>
    /// <remarks>
    /// <para>
    /// This method takes the unmanaged memory pointed at by this pointer and wraps it in a <see cref="Stream"/>. The stream returned is not a copy of the memory, so it is important to 
    /// ensure that the stream lifetime is managed in conjunction with the lifetime of the memory. Disposing of the memory and using the returned stream will result in undefined behavior 
    /// and potential memory access violations.
    /// </para>
    /// <para>
    /// A portion of the buffer can be wrapped by the stream by supplying a value for <paramref name="index"/> and/or <paramref name="count"/>.  If the count is omitted, all data from 
    /// <paramref name="index"/> up to the end of the memory block is wrapped by the stream.  Likewise, if <paramref name="index"/> is omitted, all data from the beginning of the memory block up to the 
    /// <paramref name="count"/> is wrapped. If no parameters are supplied, then the entire memory block is wrapped.
    /// </para>
    /// </remarks>        
    public Stream ToStream(int index = 0, int? count = null)
    {
        if (_ptr == null)
        {
            throw new NullReferenceException();
        }

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        count ??= Length - index;

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
        }

        if (index + count.Value > Length)
        {
            throw new ArgumentException(Resources.GOR_ERR_DATABUFF_BUFFER_OVERRUN);
        }

        count *= sizeof(T);

        return new UnmanagedMemoryStream((byte *)(_ptr + index), count.Value, count.Value, FileAccess.ReadWrite);
    }

    /// <summary>
    /// Function to convert a pointer to a native pointer.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <returns>The native pointer.</returns>
    /// <remarks>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// This operator returns the pointer to the memory address of this pointer. Developers should only use this for interop scenarios where a native call needs a pointer. Manipulation of this pointer is 
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* ToPointer(GorgonPtr<T> ptr) => ptr._ptr;

    /// <summary>
    /// Function to convert a pointer to a <see cref="long"/> value representing the memory address of the block of memory being pointed at.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <returns>The memory address as a <see cref="long"/> value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToLong(GorgonPtr<T> ptr) => (long)ptr._ptr;

    /// <summary>
    /// Function to convert a pointer to a <see cref="ulong"/> value representing the memory address of the block of memory being pointed at.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <returns>The memory address as a <see cref="ulong"/> value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ToUnsignedLong(GorgonPtr<T> ptr) => (ulong)ptr._ptr;

    /// <summary>
    /// Function to convert pointer to a native pointer.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <returns>The native pointer.</returns>
    /// <remarks>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// This operator returns the pointer to the memory address of this pointer. Developers should only use this for interop scenarios where a native call needs a pointer. Manipulation of this pointer is 
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint Tonint(GorgonPtr<T> ptr) => (nint)ptr._ptr;

    /// <summary>
    /// Function to return a SharpDX DataStream wrapping the memory pointed at by this pointer.
    /// </summary>
    /// <param name="ptr">The pointer to wrap.</param>
    /// <returns>A new SharpDX DataStream which will wrap the contents of the memory block.</returns>
    /// <remarks>
    /// <para>
    /// This method takes the unmanaged memory pointed at by this pointer and wraps it in a SharpDX DataStream. The SharpDX DataStream returned is not a copy of the memory, so it is important to 
    /// ensure that the SharpDX DataStream lifetime is managed in conjunction with the lifetime of the memory. Disposing of the memory and using the returned SharpDX DataStream will result in 
    /// undefined behavior and potential memory access violations.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DX.DataStream ToDataStream(GorgonPtr<T> ptr) => ptr.ToDataStream();

    /// <summary>
    /// Function to return a SharpDX DataStream wrapping the memory pointed at by this pointer.
    /// </summary>
    /// <param name="index">[Optional] The index in the buffer to map to the beginning of the SharpDX DataStream.</param>
    /// <param name="count">[Optional] The number of items to wrap in the SharpDX DataStream.</param>
    /// <returns>A new SharpDX DataStream which will wrap the contents of the memory block.</returns>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameter is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> plus the <paramref name="count"/> exceeds the size of the memory block.</exception>
    /// <remarks>
    /// <para>
    /// This method takes the unmanaged memory pointed at by this pointer and wraps it in a SharpDX DataStream. The SharpDX DataStream returned is not a copy of the memory, so it is important to 
    /// ensure that the SharpDX DataStream lifetime is managed in conjunction with the lifetime of the memory. Disposing of the memory and using the returned SharpDX DataStream will result in 
    /// undefined behavior and potential memory access violations.
    /// </para>
    /// <para>
    /// A portion of the memory block can be wrapped by the SharpDX DataStream by supplying a value for <paramref name="index"/> and/or <paramref name="count"/>.  If the count is omitted, all data from 
    /// <paramref name="index"/> up to the end of the memory block is wrapped by the SharpDX DataStream.  Likewise, if <paramref name="index"/> is omitted, all data from the beginning of the memory block up to the 
    /// <paramref name="count"/> is wrapped. If no parameters are supplied, then the entire memory block is wrapped.
    /// </para>
    /// </remarks>        
    public DX.DataStream ToDataStream(int index = 0, int? count = null)
    {
        if (_ptr == null)
        {
            throw new NullReferenceException();
        }

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        count ??= Length - index;

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
        }

#pragma warning disable IDE0046 // Convert to conditional expression
        if (index + count.Value > Length)
        {
            throw new ArgumentException(Resources.GOR_ERR_DATABUFF_BUFFER_OVERRUN);
        }

        return new DX.DataStream((nint)(_ptr + index), count.Value * sizeof(T), true, true);
#pragma warning restore IDE0046 // Convert to conditional expression
    }

    /// <summary>
    /// Function to return a SharpDX DataBuffer wrapping this pointer.
    /// </summary>
    /// <returns>A new SharpDX DataBuffer which will wrap the contents of the pointer.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="ptr"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This method takes the unmanaged memory used by the buffer and wraps it in a SharpDX DataBuffer. The SharpDX DataBuffer returned is not a copy of the memory used by the buffer, so it is important to 
    /// ensure that the SharpDX DataBuffer lifetime is managed in conjunction with the lifetime of the buffer. Disposing of the buffer and using the returned SharpDX DataBuffer will result in undefined behavior and potential 
    /// memory access violations.
    /// </para>
    /// </remarks>        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DX.DataBuffer ToDataBuffer(GorgonPtr<T> ptr) => ptr.ToDataBuffer();

    /// <summary>
    /// Function to return a SharpDX DataBuffer wrapping the memory pointed at by this pointer.
    /// </summary>
    /// <param name="index">[Optional] The index in the buffer to map to the beginning of the SharpDX DataBuffer.</param>
    /// <param name="count">[Optional] The number of items to wrap in the SharpDX DataBuffer.</param>
    /// <returns>A new SharpDX DataBuffer which will wrap the contents of the memory block.</returns>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameter is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> plus the <paramref name="count"/> exceeds the size of the memory block.</exception>
    /// <remarks>
    /// <para>
    /// This method takes the unmanaged memory pointed at by this pointer and wraps it in a SharpDX DataBuffer. The SharpDX DataBuffer returned is not a copy of the memory, so it is important to 
    /// ensure that the SharpDX DataBuffer lifetime is managed in conjunction with the lifetime of the memory. Disposing of the memory and using the returned SharpDX DataBuffer will result in 
    /// undefined behavior and potential memory access violations.
    /// </para>
    /// <para>
    /// A portion of the memory block can be wrapped by the SharpDX DataBuffer by supplying a value for <paramref name="index"/> and/or <paramref name="count"/>.  If the count is omitted, all data from 
    /// <paramref name="index"/> up to the end of the memory block is wrapped by the SharpDX DataBuffer.  Likewise, if <paramref name="index"/> is omitted, all data from the beginning of the memory block up to the 
    /// <paramref name="count"/> is wrapped. If no parameters are supplied, then the entire memory block is wrapped.
    /// </para>
    /// </remarks>        
    public DX.DataBuffer ToDataBuffer(int index = 0, int? count = null)
    {
        if (_ptr == null)
        {
            throw new NullReferenceException();
        }

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        count ??= Length - index;

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
        }

#pragma warning disable IDE0046 // Convert to conditional expression
        if (index + count.Value > Length)
        {
            throw new ArgumentException(Resources.GOR_ERR_DATABUFF_BUFFER_OVERRUN);
        }

        return new DX.DataBuffer((nint)(_ptr + index), count.Value * sizeof(T));
#pragma warning restore IDE0046 // Convert to conditional expression
    }

    /// <summary>
    /// Function to return a SharpDX DataPointer wrapping the memory pointed at by this pointer.
    /// </summary>
    /// <param name="ptr">The pointer to wrap.</param>
    /// <returns>A new SharpDX DataPointer which will wrap the contents of the memory block.</returns>
    /// <remarks>
    /// <para>
    /// This method takes the unmanaged memory pointed at by this pointer and wraps it in a SharpDX DataPointer. The SharpDX DataPointer returned is not a copy of the memory, so it is important to 
    /// ensure that the SharpDX DataPointer lifetime is managed in conjunction with the lifetime of the memory. Disposing of the memory and using the returned SharpDX DataPointer will result in 
    /// undefined behavior and potential memory access violations.
    /// </para>
    /// </remarks>      
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DX.DataPointer ToDataPointer(GorgonPtr<T> ptr) => ptr.ToDataPointer();

    /// <summary>
    /// Function to return a SharpDX DataPointer wrapping the memory pointed at by this pointer.
    /// </summary>
    /// <param name="index">[Optional] The index in the buffer to map to the beginning of the SharpDX DataPointer.</param>
    /// <param name="count">[Optional] The number of items to wrap in the SharpDX DataPointer.</param>
    /// <returns>A new SharpDX DataPointer which will wrap the contents of the memory block.</returns>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameter is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> plus the <paramref name="count"/> exceeds the size of the memory block.</exception>
    /// <remarks>
    /// <para>
    /// This method takes the unmanaged memory pointed at by this pointer and wraps it in a SharpDX DataPointer. The SharpDX DataPointer returned is not a copy of the memory, so it is important to 
    /// ensure that the SharpDX DataPointer lifetime is managed in conjunction with the lifetime of the memory. Disposing of the memory and using the returned SharpDX DataPointer will result in 
    /// undefined behavior and potential memory access violations.
    /// </para>
    /// <para>
    /// A portion of the memory block can be wrapped by the SharpDX DataPointer by supplying a value for <paramref name="index"/> and/or <paramref name="count"/>.  If the count is omitted, all data from 
    /// <paramref name="index"/> up to the end of the memory block is wrapped by the SharpDX DataPointer.  Likewise, if <paramref name="index"/> is omitted, all data from the beginning of the memory block up to the 
    /// <paramref name="count"/> is wrapped. If no parameters are supplied, then the entire memory block is wrapped.
    /// </para>
    /// </remarks>        
    public DX.DataPointer ToDataPointer(int index = 0, int? count = null)
    {
        if (_ptr == null)
        {
            throw new NullReferenceException();
        }

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        count ??= Length - index;

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
        }

#pragma warning disable IDE0046 // Convert to conditional expression
        if (index + count.Value > Length)
        {
            throw new ArgumentException(Resources.GOR_ERR_DATABUFF_BUFFER_OVERRUN);
        }

        return new DX.DataPointer((nint)(_ptr + index), count.Value * sizeof(T));
#pragma warning restore IDE0046 // Convert to conditional expression
    }

    /// <summary>
    /// Function to access native data as a span slice.
    /// </summary>
    /// <returns>A span for the pointer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> ToSpan(GorgonPtr<T> ptr) => ptr.ToSpan();

    /// <summary>
    /// Function to access native data as a span slice.
    /// </summary>
    /// <param name="index">The index of the item to start slicing at.</param>
    /// <param name="count">[Optional] The number of items to slice.</param>
    /// <returns>A span for the pointer.</returns>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameter is less than 0.</exception>
    public Span<T> ToSpan(int index = 0, int? count = null)
    {
        if (_ptr == null)
        {
            throw new NullReferenceException();
        }

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        count ??= Length - index;

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
        }

#pragma warning disable IDE0046 // Convert to conditional expression
        if (index + count.Value > Length)
        {
            throw new ArgumentException(Resources.GOR_ERR_DATABUFF_BUFFER_OVERRUN);
        }

        return new Span<T>(_ptr + index, count.Value);
#pragma warning restore IDE0046 // Convert to conditional expression
    }

    /// <summary>
    /// Function to access native data as a read only span slice.
    /// </summary>
    /// <returns>A span for the pointer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> ToReadOnlySpan(GorgonPtr<T> ptr) => ToSpan(ptr);

    /// <summary>
    /// Function to access native data as a read only span slice.
    /// </summary>
    /// <param name="index">The index of the item to start slicing at.</param>
    /// <param name="count">[Optional] The number of items to slice.</param>
    /// <returns>A span for the pointer.</returns>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameter is less than 0.</exception>
    public ReadOnlySpan<T> ToReadOnlySpan(int index, int? count = null) => ToSpan(index, count);

    /// <summary>
    /// Function to convert this pointer into a byte based pointer.
    /// </summary>
    /// <param name="offset">[Optional] The number of bytes to offset by.</param>
    /// <returns>A new byte pointer starting at the pointer address with the offset applied.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="offset"/> would cause a buffer overrun.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonPtr<byte> ToBytePointer(int offset = 0)
    {
        if (_ptr == null)
        {
            return GorgonPtr<byte>.NullPtr;
        }

        if (offset == 0)
        {
            return new GorgonPtr<byte>((byte *)_ptr, SizeInBytes);
        }

        int sizeInBytes = SizeInBytes;

        return (offset < 0) || (offset >= sizeInBytes)
            ? throw new ArgumentOutOfRangeException(nameof(offset))
            : new GorgonPtr<byte>(((byte*)_ptr) + offset, SizeInBytes - offset);
    }

    /// <summary>
    /// Function to copy the contents of the memory pointed at by this pointer into a span.
    /// </summary>
    /// <param name="span">The span to write into.</param>
    /// <param name="index">[Optional] The index within the pointer to start reading from.</param>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is less than 0.</exception>
    public void CopyTo(Span<T> span, int index = 0)
    {
        if (_ptr == null)
        {
            throw new NullReferenceException();
        }

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        int count = span.Length.Min(Length - index);

        Unsafe.CopyBlock(ref Unsafe.As<T, byte>(ref span[0]), ref Unsafe.AsRef<byte>(_ptr), (uint)(count * sizeof(T)));
    }

    /// <summary>
    /// Function to copy the contents of this buffer into a span.
    /// </summary>
    /// <param name="memory">The span to write into.</param>
    /// <param name="srcOffset">[Optional] The offset within the buffer to start reading from.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="srcOffset"/> is less than 0.</exception>
    public void CopyTo(Memory<T> memory, int srcOffset = 0) => CopyTo(memory.Span, srcOffset);

    /// <summary>Returns a <see cref="string" /> that represents this instance.</summary>
    /// <returns>A <see cref="string" /> that represents this instance.</returns>
    public override string ToString() => string.Format(Resources.GOR_TOSTR_POINTER, _ptr == null ? @"NULL" : $"0x{(!Environment.Is64BitProcess ? ((int)_ptr).FormatHex() : ((long)_ptr).FormatHex())}");

    /// <summary>Returns a hash code for this instance.</summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    public override int GetHashCode() => _ptr == null ? 0 : (int)_ptr;

    /// <summary>Determines whether the specified <see cref="object" /> is equal to this instance.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object obj) => (obj is GorgonPtr<T> ptr) ? ptr.Equals(this) : base.Equals(obj);

    /// <summary>
    /// Function to compare this pointer with another.
    /// </summary>
    /// <param name="other">The other pointer to compare to.</param>
    /// <returns>0 if the two pointers point at the same memory address, -1 if the this pointer address is less than the other pointer address and 1 if this pointer address is greater than the other pointer address.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(GorgonPtr<T> other) => other == this ? 0 : this < other ? -1 : 1;

    /// <summary>
    /// Function to compare the data pointed at by this pointer and another pointer.
    /// </summary>
    /// <param name="other">The other pointer to compare with.</param>
    /// <returns><b>true</b> if the data is the same, or <b>false</b> if not.</returns>
    /// <remarks>
    /// <para>
    /// This method is the equivalent of a <c>memcmp</c> in C/C++. It takes two pointers and compares the byte data in memory pointed at by both pointers. If any data is different at the byte level 
    /// then the method will return <b>false</b>, otherwise, if all bytes are the same, then the method will return <b>true</b>. If both pointers point at the same memory address, then this method 
    /// will always return <b>true</b>.
    /// </para>
    /// </remarks>
    public bool CompareData(GorgonPtr<T> other)
    {
        if (other.Equals(this))
        {
            return true;
        }

        unsafe
        {
            if ((_ptr == null) || (other._ptr == null))
            {
                return false;
            }

            byte* leftData = (byte *)_ptr;
            byte* rightData = (byte*)other._ptr;
            int dataLength = SizeInBytes - (_index * TypeSize).Min(other.SizeInBytes - (other._index * TypeSize));

            while (dataLength > 0)
            { 
                if (dataLength > sizeof(long))
                {                        
                    long left = *((long*)leftData);
                    long right = *((long*)rightData);

                    if (left != right)
                    {
                        return false;
                    }

                    leftData += sizeof(long);
                    rightData += sizeof(long);
                    dataLength -= sizeof(long);
                    continue;
                }

                if (dataLength > sizeof(int))
                {
                    int left = *((int *)leftData);
                    int right = *((int *)rightData);

                    if (left != right)
                    {
                        return false;
                    }

                    leftData += sizeof(int);
                    rightData += sizeof(int);
                    dataLength -= sizeof(int);
                    continue;
                }

                if (dataLength > sizeof(short))
                {
                    short left = *((short *)leftData);
                    short right = *((short *)rightData);

                    if (left != right)
                    {
                        return false;
                    }

                    leftData += sizeof(short);
                    rightData += sizeof(short);
                    dataLength -= sizeof(short);
                    continue;
                }

                if (*leftData != *rightData)
                {
                    return false;
                }

                leftData++;
                rightData++;
                dataLength--;
            }
        }

        return true;
    }
    #endregion

    #region Constructor/Finalizer.       
    /// <summary>Initializes a new instance of the <see cref="GorgonPtr{T}" /> struct.</summary>
    /// <param name="ptr">The pointer to wrap.</param>
    /// <param name="indexOffset">The offset, in indices of this pointer within the memory block.</param>
    /// <param name="count">The number of items in the memory block.</param>
    private GorgonPtr(T* ptr, int indexOffset, int count)
    {
        _ptr = ptr;
        _index = indexOffset;
        Length = count;
    }

    /// <summary>Initializes a new instance of the <see cref="GorgonPtr{T}"/> struct by cloning another pointer.</summary>
    /// <param name="dataPtr">The pointer to clone.</param>
    public GorgonPtr(GorgonPtr<T> dataPtr)
    {
        _index = 0;
        _ptr = dataPtr._ptr;
        Length = dataPtr.Length / sizeof(T);
    }

    /// <summary>Initializes a new instance of the <see cref="GorgonPtr{T}" /> struct.</summary>
    /// <param name="dataPtr">The SharpDX pointer to wrap within this pointer.</param>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="dataPtr"/> is <b>NULL</b>.</exception>
    /// <remarks>
    /// <para>
    /// This constructor wraps the pointer provided by the SharpDX pointer <paramref name="dataPtr"/>. 
    /// </para>
    /// </remarks>
    public GorgonPtr(DX.DataPointer dataPtr)
    {
        if (dataPtr.IsEmpty)
        {
            throw new ArgumentNullException(nameof(dataPtr));
        }

        _index = 0;
        _ptr = (T*)dataPtr.Pointer;
        Length = dataPtr.Size / sizeof(T);
    }

    /// <summary>Initializes a new instance of the <see cref="GorgonPtr{T}" /> struct.</summary>
    /// <param name="dataBuffer">The SharpDX buffer to wrap within this pointer.</param>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="dataBuffer"/> is <b>NULL</b>.</exception>
    /// <remarks>
    /// <para>
    /// This constructor wraps the pointer provided by the SharpDX buffer <paramref name="dataBuffer"/>. 
    /// </para>
    /// </remarks>
    public GorgonPtr(DX.DataBuffer dataBuffer)
    {
        if (dataBuffer == null)
        {
            throw new ArgumentNullException(nameof(dataBuffer));
        }

        _index = 0;
        _ptr = (T*)dataBuffer.DataPointer;
        Length = dataBuffer.Size / sizeof(T);
    }

    /// <summary>Initializes a new instance of the <see cref="GorgonPtr{T}" /> struct.</summary>
    /// <param name="dataStream">The SharpDX stream to wrap within this pointer.</param>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="dataStream"/> is <b>NULL</b>.</exception>
    /// <remarks>
    /// <para>
    /// This constructor wraps the pointer provided by the SharpDX pointer <paramref name="dataStream"/>. This pointer will <b>not</b> change the stream position when read, written, 
    /// or incremented/decremented.
    /// </para>
    /// </remarks>
    public GorgonPtr(DX.DataStream dataStream)
    {
        if (dataStream == null)
        {
            throw new ArgumentNullException(nameof(dataStream));
        }

        _index = 0;
        _ptr = (T*)dataStream.DataPointer;            
        Length = (int)dataStream.Length / sizeof(T);
    }

    /// <summary>Initializes a new instance of the <see cref="GorgonPtr{T}" /> struct.</summary>
    /// <param name="buffer">The native buffer to wrap within this pointer.</param>
    /// <param name="index">[Optional] The index within the native buffer to start at.</param>
    /// <param name="count">[Optional] The number of items within the native buffer to point at.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or <paramref name="count"/> parameter is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> plus the <paramref name="count"/> is larger than the capacity of the <paramref name="buffer"/>.</exception>
    public GorgonPtr(GorgonNativeBuffer<T> buffer, int index = 0, int? count = null)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        count ??= buffer.Length - index;

        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if ((index + count) > buffer.Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, index, count));
        }

        _index = index;
        _ptr = ((T *)buffer + index);
        Length = count.Value;
    }

    /// <summary>Initializes a new instance of the <see cref="GorgonPtr{T}" /> struct.</summary>
    /// <param name="pointer">The pointer to memory to wrap with this pointer.</param>
    /// <param name="count">The number of items of type <typeparamref name="T"/> to wrap.</param>
    /// <exception cref="NullReferenceException">Thrown when the pointer is <b>NULL</b>.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="count"/> parameter is less than 1.</exception>
    /// <remarks>
    /// <para>
    /// <note type="important">
    /// This takes a native memory pointer and wraps it for safety. It is important that the <paramref name="count"/> is correct, otherwise memory access violations may occur when the pointer is 
    /// used beyond the memory region that the original <paramref name="pointer"/> is assigned to.
    /// </note>
    /// </para>
    /// </remarks>
    public GorgonPtr(nint pointer, int count)
    {
        if (pointer == IntPtr.Zero)
        {
            throw new NullReferenceException(Resources.GOR_ERR_PTR_NULL);
        }

        if (count <= 0)
        {
            throw new ArgumentException(Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL, nameof(count));
        }

        _index = 0;
        _ptr = (T*)pointer;
        Length = count;
    }

    /// <summary>Initializes a new instance of the <see cref="GorgonPtr{T}" /> struct.</summary>
    /// <param name="pointer">The pointer to memory to wrap with this pointer.</param>
    /// <param name="count">The number of items of type <typeparamref name="T"/> to wrap.</param>
    /// <exception cref="NullReferenceException">Thrown when the pointer is <b>NULL</b>.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="count"/> parameter is less than 1.</exception>
    /// <remarks>
    /// <para>
    /// <note type="important">
    /// This takes a native memory pointer and wraps it for safety. It is important that the <paramref name="count"/> is correct, otherwise memory access violations may occur when the pointer is 
    /// used beyond the memory region that the original <paramref name="pointer"/> is assigned to.
    /// </note>
    /// </para>
    /// </remarks>
    public GorgonPtr(T* pointer, int count)
    {
        if (pointer == null)
        {
            throw new NullReferenceException(Resources.GOR_ERR_PTR_NULL);
        }

        if (count <= 0)
        {
            throw new ArgumentException(Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL, nameof(count));
        }

        _index = 0;
        _ptr = pointer;
        Length = count;
    }
    #endregion       
}
