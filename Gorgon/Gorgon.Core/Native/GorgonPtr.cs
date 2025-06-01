// 
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
// Created: December 1, 2020 9:29:00 PM
// 

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using Gorgon.Core;
using Gorgon.Math;
using Gorgon.Properties;

namespace Gorgon.Native;

/// <summary>
/// A value type representing a pointer to native (unmanaged) memory.
/// </summary>
/// <typeparam name="T">The type of data stored in the memory pointed at by the pointer. Must be an unmanaged value type.</typeparam>
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
/// This type is suitable when the type of data stored in native memory is known. If the type of data in memory is not known (e.g. <c>void *</c>), or refers to an opaque handle (e.g. <c>HWND</c>), 
/// an <c>nint</c> should be used instead. 
/// </para>
/// <para>
/// This pointer type only wraps a native pointer to previously allocated memory, therefore it does <b>not</b> perform any memory allocation on its own. Gorgon includes the 
/// <see cref="GorgonNativeBuffer{T}"/> for that purpose. The <see cref="GorgonNativeBuffer{T}"/> will implicitly convert to this type, so it can be used in situations where this type is required.
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
/// <note type="important">
/// <para>
/// This type is a little slower for access than a regular native pointer (x64). This is due to the safety features available to ensure the pointer does not cause a buffer over/underrun. For pure speed, 
/// nothing beats a native pointer (or <c>nint</c>) and if your code is sensitive to microsecond timings (i.e. it needs to be blazing fast), then use a native pointer instead 
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
    /// <summary>
    /// Represents a null pointer.
    /// </summary>
    public static readonly GorgonPtr<T> NullPtr = default;

    // The actual unsafe pointer to the memory.
    private readonly T* _ptr;

    // The offset from the start of the memory block that this pointer is pointing at.
    private readonly long _indexOffset;

    /// <summary>
    /// The number of items of type <typeparamref name="T"/> stored within the memory block. 
    /// </summary>
    public readonly long Length;

    /// <summary>
    /// Property to return the size, in bytes, of the type represented by <typeparamref name="T"/>.
    /// </summary>
    public int TypeSize => sizeof(T);

    /// <summary>
    /// Property to return the total size, in bytes, of the memory pointed at by this pointer.
    /// </summary>
    public long SizeInBytes => Length * TypeSize;

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
    /// GorgonPtr_Long<int> ptr = ...;
    /// int newValue = 123;
    /// 
    /// ptr[2] = newValue;
    /// 
    /// // or
    /// 
    /// ref int oldValue = ref ptr[2];
    /// oldValue = 456;
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    public ref T this[long index]
    {
        get
        {
            if (_ptr is null)
            {
                throw new NullReferenceException();
            }

            if ((index < 0) || (index >= Length))
            {
                throw new IndexOutOfRangeException(string.Format(Resources.GOR_ERR_INDEX_OUT_OF_RANGE, index, 0, Length));
            }

            return ref Unsafe.AsRef<T>(_ptr + index);
        }
    }

    /// <summary>
    /// Property to return a reference to the first value pointed at by this pointer.
    /// </summary>
    public ref T Value => ref Unsafe.AsRef<T>(_ptr);

    /// <summary>
    /// Property to return a slice of this pointer's memory block by a given range.
    /// </summary>
    /// <param name="range">The range of indices to cover.</param>
    /// <returns>The slice of the memory as a <see cref="GorgonPtr{T}"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="range"/> is less than 0, or greater than/equal to <see cref="Length"/>.</exception>
    /// <remarks>
    /// <para>
    /// This indexer can be used to create a slice of a pointer. For example, if a pointer points to 10 items, and we want to create a pointer that points to the 3rd item and 4 items in the memory block 
    /// pointed at by this pointer, we can do the following:
    /// <code lang="csharp">
    /// <![CDATA[
    /// int[] data = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    /// 
    /// unsafe
    /// {
    ///     fixed(int* dataPtr = data)
    ///     {
    ///         GorgonPtr_Long<int> tenItems = new(data);    
    ///         // Add 3 items to the pointer, and then take 4 items from that point.
    ///         GorgonPtr_Long<int> sliced = tenItems[3..7];
    /// 
    ///         // 'sliced' will now point to: 4, 5, 6, 7
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// If the data is larger than 2 GB, only 2 GB will be sliced using the <see cref="Range"/> because the <see cref="Range"/> type uses a 32 bit <see cref="int"/> for its start and end values.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public GorgonPtr<T> this[Range range]
    {
        get
        {
            if (_ptr is null)
            {
                return NullPtr;
            }

            (int offset, int length) = range.GetOffsetAndLength((int)(Length.Min(int.MaxValue)));

            if ((offset < 0) || (offset >= Length))
            {
                throw new ArgumentOutOfRangeException(nameof(range), string.Format(Resources.GOR_ERR_INDEX_OUT_OF_RANGE, offset, 0, Length));
            }

            return new GorgonPtr<T>(_ptr + offset, length);
        }
    }

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
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{Byte}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="byte"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<byte>(GorgonPtr<T> ptr) => ptr.ToPointerType<byte>();

    /// <summary>
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{SByte}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="sbyte"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<sbyte>(GorgonPtr<T> ptr) => ptr.ToPointerType<sbyte>();

    /// <summary>
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{Int16}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="short"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<short>(GorgonPtr<T> ptr) => ptr.ToPointerType<short>();

    /// <summary>
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{UInt16}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="ushort"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<ushort>(GorgonPtr<T> ptr) => ptr.ToPointerType<ushort>();

    /// <summary>
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{Int32}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="int"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<int>(GorgonPtr<T> ptr) => ptr.ToPointerType<int>();

    /// <summary>
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{UInt32}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="uint"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<uint>(GorgonPtr<T> ptr) => ptr.ToPointerType<uint>();

    /// <summary>
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{Int64}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="long"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<long>(GorgonPtr<T> ptr) => ptr.ToPointerType<long>();

    /// <summary>
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{UInt64}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="ulong"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<ulong>(GorgonPtr<T> ptr) => ptr.ToPointerType<ulong>();

    /// <summary>
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{Half}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="Half"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<Half>(GorgonPtr<T> ptr) => ptr.ToPointerType<Half>();

    /// <summary>
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{Single}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="float"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<float>(GorgonPtr<T> ptr) => ptr.ToPointerType<float>();

    /// <summary>
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{Double}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="double"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<double>(GorgonPtr<T> ptr) => ptr.ToPointerType<double>();

    /// <summary>
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{Matrix4x4}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="Matrix4x4"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<Matrix4x4>(GorgonPtr<T> ptr) => ptr.ToPointerType<Matrix4x4>();

    /// <summary>
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{Matrix3x2}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="Matrix3x2"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<Matrix3x2>(GorgonPtr<T> ptr) => ptr.ToPointerType<Matrix3x2>();

    /// <summary>
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{Quaternion}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="Quaternion"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<Quaternion>(GorgonPtr<T> ptr) => ptr.ToPointerType<Quaternion>();

    /// <summary>
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{Vector4}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="Vector4"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<Vector4>(GorgonPtr<T> ptr) => ptr.ToPointerType<Vector4>();

    /// <summary>
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{Vector3}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="Vector3"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<Vector3>(GorgonPtr<T> ptr) => ptr.ToPointerType<Vector3>();

    /// <summary>
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{Vector2}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="Vector2"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<Vector2>(GorgonPtr<T> ptr) => ptr.ToPointerType<Vector2>();

    /// <summary>
    /// Operator to explicitly convert this pointer to a <see cref="GorgonPtr{Plane}"/> type.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <see cref="Plane"/> is larger than <see cref="SizeInBytes"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator GorgonPtr<Plane>(GorgonPtr<T> ptr) => ptr.ToPointerType<Plane>();

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
    public static explicit operator void*(GorgonPtr<T> ptr) => ToPointer(ptr);

    /// <summary>
    /// Operator to convert this pointer to a <see cref="long"/> value representing the memory address of the block of memory being pointed at.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <returns>The memory address as a <see cref="long"/> value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator long(GorgonPtr<T> ptr) => ToInt64(ptr);

    /// <summary>
    /// Operator to convert this pointer to a <see cref="ulong"/> value representing the memory address of the block of memory being pointed at.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <returns>The memory address as a <see cref="ulong"/> value.</returns>    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator ulong(GorgonPtr<T> ptr) => ToUInt64(ptr);

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
    public static explicit operator nint(GorgonPtr<T> ptr) => ToIntPtr(ptr);

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
    public static explicit operator nuint(GorgonPtr<T> ptr) => ToUIntPtr(ptr);

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
    public static GorgonPtr<T> operator +(GorgonPtr<T> ptr, long indexOffset) => Add(ptr, indexOffset);

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
    public static GorgonPtr<T> operator +(GorgonPtr<T> ptr, int indexOffset) => Add(ptr, indexOffset);

    /// <summary>
    /// Operator to increment the pointer by the given index offset.
    /// </summary>
    /// <param name="indexOffset">The number of indices to offset by.</param>
    /// <param name="ptr">The pointer to increment.</param>
    /// <returns>A new <see cref="GorgonPtr{T}"/> starting at the updated index offset.</returns>
    /// <remarks>
    /// <para>
    /// If the pointer is incremented beyond the beginning, or end of the memory block that it points at, then the return value will be <see cref="GorgonPtr{T}.NullPtr"/> rather than throw an exception. 
    /// This is done this way for performance reasons. So users should check that their pointer is not <b>null</b> when iterating to ensure that the pointer is still valid.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPtr<T> operator +(long indexOffset, GorgonPtr<T> ptr) => Add(ptr, indexOffset);

    /// <summary>
    /// Operator to increment the pointer by the given index offset.
    /// </summary>
    /// <param name="indexOffset">The number of indices to offset by.</param>
    /// <param name="ptr">The pointer to increment.</param>
    /// <returns>A new <see cref="GorgonPtr{T}"/> starting at the updated index offset.</returns>
    /// <remarks>
    /// <para>
    /// If the pointer is incremented beyond the beginning, or end of the memory block that it points at, then the return value will be <see cref="GorgonPtr{T}.NullPtr"/> rather than throw an exception. 
    /// This is done this way for performance reasons. So users should check that their pointer is not <b>null</b> when iterating to ensure that the pointer is still valid.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPtr<T> operator +(int indexOffset, GorgonPtr<T> ptr) => Add(ptr, indexOffset);

    /// <summary>
    /// Function to subtract two pointers to return the number of bytes between them.
    /// </summary>
    /// <param name="left">The left pointer to subtract.</param>
    /// <param name="right">The right pointer to subtract.</param>
    /// <returns>The difference in bytes between the two pointers.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long operator -(GorgonPtr<T> left, GorgonPtr<T> right) => Subtract(left, right);

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
    public static GorgonPtr<T> operator -(GorgonPtr<T> ptr, long indexOffset) => Subtract(ptr, indexOffset);

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
    public static GorgonPtr<T> operator -(GorgonPtr<T> ptr, int indexOffset) => Subtract(ptr, indexOffset);

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
    public static GorgonPtr<T> operator ++(GorgonPtr<T> ptr) => Add(ptr, 1);

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
    public static GorgonPtr<T> operator --(GorgonPtr<T> ptr) => Subtract(ptr, 1);

    /// <summary>
    /// Function to subtract two pointers to return the number of bytes between them.
    /// </summary>
    /// <param name="left">The left pointer to subtract.</param>
    /// <param name="right">The right pointer to subtract.</param>
    /// <returns>The difference in bytes between the two pointers.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Subtract(GorgonPtr<T> left, GorgonPtr<T> right)
    {
        if ((left == NullPtr) || (right == NullPtr))
        {
            return long.MaxValue;
        }

        return ((byte*)left._ptr) - ((byte*)right._ptr);
    }

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
    public static GorgonPtr<T> Add(GorgonPtr<T> ptr, long indexOffset)
    {
        if (ptr == NullPtr)
        {
            return NullPtr;
        }

        long newIndex = ptr._indexOffset + indexOffset;

        if ((newIndex < 0) || (newIndex >= ptr.Length + ptr._indexOffset))
        {
            return NullPtr;
        }

        return new(ptr._ptr + indexOffset, newIndex, ptr.Length - indexOffset);
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
    public static GorgonPtr<T> Subtract(GorgonPtr<T> ptr, long indexOffset)
    {
        if (ptr == NullPtr)
        {
            return NullPtr;
        }

        long newIndex = ptr._indexOffset - indexOffset;

        if ((newIndex < 0) || (newIndex >= ptr.Length + ptr._indexOffset))
        {
            return NullPtr;
        }

        return new(ptr._ptr - indexOffset, newIndex, ptr.Length + indexOffset);
    }

    /// <summary>
    /// Function to convert a pointer to another pointer type.
    /// </summary>
    /// <typeparam name="TTo">The type to convert to. Must be an unmanaged value type, and can be different than <typeparamref name="T"/>.</typeparam>
    /// <param name="ptr">The pointer to convert.</param>
    /// <exception cref="NullReferenceException">Thrown when the pointer is <b>null</b>.</exception>
    /// <exception cref="InvalidCastException">Thrown if the <see cref="SizeInBytes"/> is smaller than the size (in bytes) of the type <typeparamref name="TTo"/>.</exception>
    /// <returns>The casted pointer.</returns>
    /// <remarks>
    /// <para>
    /// This is the equivalent of casting the pointer to another pointer type (e.g. <c>byte* ptr = (byte *)ptr2</c>). This is useful for remapping data types within the memory pointed at by the pointer.
    /// </para>
    /// <para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The type referenced by <typeparamref name="TTo"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
    /// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
    /// </para>
    /// <para>
    /// Value types with marshalling attributes (<see cref="MarshalAsAttribute"/>) are <i>not</i> supported and will not be read correctly.
    /// </para>
    /// </note>
    /// </para>
    /// <note type="warning">
    /// <para>
    /// Note that this method does not perform widening. That is, converting a pointer that points to a memory block that has a size of 4 bytes, to a pointer with a type that has a size of 8 bytes (e.g. 
    /// converting a <see cref="int"/> to <see cref="double"/>) will throw an exception. 
    /// </para>
    /// <para>
    /// Because of this, it is best practice that the pointer memory size be evenly divisible by the size of the <typeparamref name="TTo"/> type when converting.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// For example:
    /// <code lang="csharp">
    /// <![CDATA[
    /// GorgonPtr_Long<byte> ptr = ...;
    /// GorgonPtr_Long<int> ptr2 = ...;
    /// 
    /// // This is the same as converting byte* ptr = (byte *)ptr2.
    /// ptr = ptr2.To<byte>();
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    public static GorgonPtr<TTo> To<TTo>(GorgonPtr<T> ptr)
        where TTo : unmanaged
    {
        if (ptr._ptr is null)
        {
            throw new NullReferenceException();
        }

        long toSize = Unsafe.SizeOf<TTo>();
        long newCount = ptr.SizeInBytes / toSize;

        if ((newCount == 0) || ((newCount * toSize) > ptr.SizeInBytes))
        {
            throw new InvalidCastException(string.Format(Resources.GOR_ERR_PTR_CONVERT_WIDENING, toSize, ptr.SizeInBytes));
        }

        return new((TTo*)ptr._ptr, newCount);
    }

    /// <summary>
    /// Function to return a reference, of the specified type, to the memory pointed at by the pointer.
    /// </summary>
    /// <typeparam name="TTo">The type of reference value to interpret the data as. Must be an unmanaged value type, and can be different than pointer type <typeparamref name="T"/>.</typeparam>
    /// <param name="offset">[Optional] The offset, in bytes, within the memory pointed at this pointer.</param>
    /// <exception cref="NullReferenceException">Thrown when the pointer is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> is less than 0, or greater than, or equal to, the pointer <see cref="SizeInBytes"/>.</exception>
    /// <returns>The refernce to the value at the specified index.</returns>
    /// <remarks>
    /// <para>
    /// This is meant for converting the data to another type while accessing memory. If the type of data specified by <typeparamref name="T"/> is the same as <typeparamref name="TTo"/>, then use the 
    /// indexing property on the pointer for better performance.
    /// </para>
    /// <para>
    /// The <paramref name="offset"/> parameter allows the reference the value at the specified byte offset within the memory pointed at by the pointer. For example, if the <paramref name="offset"/> 
    /// is 4, then the returned reference value will be the value at 4 bytes into the memory pointed at by the pointer.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// The type referenced by <typeparamref name="TTo"/> type parameter must have a <see cref="StructLayoutAttribute"/> with a <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/> 
    /// struct layout. Otherwise, .NET may rearrange the members and the data may not appear in the correct place.
    /// </para>
    /// <para>
    /// Value types with marshalling attributes (<see cref="MarshalAsAttribute"/>) are <i>not</i> supported and will not be read correctly.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// This value is returned as a reference, and as such, it can be assigned to as well. For example:
    /// <code lang="csharp">
    /// <![CDATA[
    /// GorgonPtr_Long<int> ptr = ...;
    /// byte newValue = 0x7f;
    /// 
    /// // This will write the byte value 0x7f at the 2nd byte in the integer data stored in 
    /// // the memory pointed at by the pointer. (e.g. the memory will contain 0x00007F00).
    /// ref byte oldValue = ref ptr.AsRef<byte>(1);
    /// oldValue = newValue;
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TTo AsRef<TTo>(long offset = 0)
        where TTo : unmanaged
    {
        if (_ptr is null)
        {
            throw new NullReferenceException();
        }

        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
        }

        if (offset >= SizeInBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), string.Format(Resources.GOR_ERR_INDEX_OUT_OF_RANGE, 0, SizeInBytes));
        }

        return ref Unsafe.AsRef<TTo>(offset + (byte*)_ptr);
    }

    /// <summary>
    /// Function to take a slice of a <see cref="GorgonPtr{T}"/>.
    /// </summary>
    /// <param name="index">The index into the pointer data to start slicing.</param>
    /// <param name="count">The number of elements to slice.</param>
    /// <returns>A new pointer to the slice of data.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="index"/> is less than 0.</exception>
    /// <remarks>
    /// <para>
    /// This slices a section of the data pointed at by this pointer, into a new pointer starting at the address of the original pointer + the <paramref name="index"/>, up to the <paramref name="count"/> 
    /// elements. This new pointer still points at the memory pointed at the original pointer.
    /// </para>
    /// </remarks>
    public GorgonPtr<T> Slice(long index, long count)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Length);

        if ((count < 0) || (index >= Length))
        {
            return NullPtr;
        }

        long actualCount = count.Min(Length - index);

        if (actualCount <= 0)
        {
            return NullPtr;
        }

        return new(_ptr + index, actualCount);
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
    public static long ToInt64(GorgonPtr<T> ptr) => (long)ptr._ptr;

    /// <summary>
    /// Function to convert a pointer to a <see cref="ulong"/> value representing the memory address of the block of memory being pointed at.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <returns>The memory address as a <see cref="ulong"/> value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ToUInt64(GorgonPtr<T> ptr) => (ulong)ptr._ptr;

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
    public static nint ToIntPtr(GorgonPtr<T> ptr) => (nint)ptr._ptr;

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
    public static nuint ToUIntPtr(GorgonPtr<T> ptr) => (nuint)ptr._ptr;

    /// <summary>
    /// Function to access native data as a span slice.
    /// </summary>
    /// <param name="ptr">The pointer to convert.</param>
    /// <returns>A span for the pointer.</returns>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <see cref="GorgonPtr{T}.NullPtr"/>.</exception>
    /// <remarks>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// The <see cref="Span{T}"/>/<see cref="ReadOnlySpan{T}"/> types are limited to 32 bit index and size values. If the buffer is over 2 GB in size, then only the 1st 2 GB will be returned.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public static Span<T> ToSpan(GorgonPtr<T> ptr)
    {
        if (ptr._ptr is null)
        {
            throw new NullReferenceException();
        }

        return new(ptr._ptr, (int)(ptr.Length.Min(int.MaxValue)));
    }

    /// <summary>
    /// Function to convert a pointer into a pointer with a different data type.
    /// </summary>
    /// <typeparam name="K">The new type to interpret the data as.</typeparam>
    /// <param name="count">[Optional] The length of data for the new pointer.</param>
    /// <returns>A new pointer starting at the pointer address.</returns>
    /// <exception cref="OverflowException">Thrown if the size, in bytes, of <typeparamref name="K"/> is larger than <see cref="SizeInBytes"/>.</exception>
    /// <remarks>
    /// <para>
    /// If the <paramref name="count"/> is 0, then the entire length is covered. Otherwise, only the specified length is covered, up to the <see cref="Length"/>.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonPtr<K> ToPointerType<K>(long count = 0)
        where K : unmanaged
    {
        if (_ptr is null)
        {
            return GorgonPtr<K>.NullPtr;
        }

        long dataSize = sizeof(K);

        if (count <= 0)
        {
            count = SizeInBytes;
        }
        else
        {
            count *= dataSize;
        }

        if (dataSize > SizeInBytes)
        {
            throw new OverflowException(string.Format(Resources.GOR_ERR_BUFFER_OVERFLOW, typeof(K).FullName ?? string.Empty));
        }

        return new((K*)_ptr, count / dataSize);
    }

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
    public override bool Equals(object? obj) => (obj is GorgonPtr<T> ptr) ? Equals(ptr) : base.Equals(obj);

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(GorgonPtr<T> other) => _ptr == other._ptr;

    /// <summary>
    /// Function to fill the memory pointed at by a pointer with a specific byte value.
    /// </summary>
    /// <param name="value">The value used to fill the memory.</param>
    /// <exception cref="NullReferenceException">Thrown when the pointer is <b>null</b>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Fill(byte value)
    {
        if (_ptr is null)
        {
            throw new NullReferenceException();
        }
        
        NativeMemory.Fill(_ptr, (nuint)SizeInBytes, value);
    }

    /// <summary>
    /// Function to copy the memory pointed at by this pointer into another pointer.
    /// </summary>
    /// <param name="destination">The destination pointer that will receive the data.</param>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <see cref="GorgonPtr{T}.NullPtr"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destination"/> parameter is <see cref="GorgonPtr{T}.NullPtr"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the memory pointed at by <paramref name="destination"/> is smaller than <see cref="SizeInBytes"/>.</exception>
    /// <remarks>
    /// <para>
    /// Use this method to copy memory from one location to another. When the copy is performed, the entire memory block in this pointer will be copied to the <paramref name="destination"/> pointer. If the 
    /// <see cref="Length"/> is too large to accomodate the memory block in the <paramref name="destination"/> pointer, then an exception will be thrown.
    /// </para>
    /// <para>
    /// In order to copy a specified portion to the <paramref name="destination"/>, one may use the range operator like so:
    /// <code lang="csharp">
    /// <![CDATA[
    /// GorgonPtr_Long<byte> source = ...;
    /// GorgonPtr_Long<byte> destination = ...;
    /// 
    /// src[..10].CopyTo(destination);
    /// 
    /// // Or even a portion of the source buffer to an offset within the destination buffer.
    /// src[5..10].CopyTo(destination[10..]);
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(GorgonPtr<T> destination)
    {
        if (_ptr is null)
        {
            throw new NullReferenceException();
        }

        ArgumentNullException.ThrowIfNull(destination._ptr, nameof(destination));

        if (SizeInBytes > destination.SizeInBytes)
        {
            throw new ArgumentException(Resources.GOR_ERR_DESTINATION_TOO_SMALL, nameof(destination));
        }

        NativeMemory.Copy(_ptr, destination._ptr, (nuint)SizeInBytes);
    }

    /// <summary>
    /// Function to copy the memory pointed at by this pointer into a <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="destination">The destination pointer that will receive the data.</param>
    /// <exception cref="NullReferenceException">Thrown when this pointer is <see cref="GorgonPtr{T}.NullPtr"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the <see cref="Length"/> is too big for the memory block of the memory pointed at by the <paramref name="destination"/>.</exception>
    /// <remarks>
    /// <para>
    /// Use this method to copy memory from one location to another. When the copy is performed, the entire memory block in this pointer will be copied to the <paramref name="destination"/> span. If the 
    /// <see cref="Length"/> is too large to accomodate the memory block in the <paramref name="destination"/> span, then an exception will be thrown.
    /// </para>
    /// <para>
    /// In order to copy a specified portion to the <paramref name="destination"/>, one may use the range operator like so:
    /// <code lang="csharp">
    /// <![CDATA[
    /// GorgonPtr_Long<byte> source = ...;
    /// Span<byte> destination = new byte[512];
    /// 
    /// src[..10].CopyTo(destination);
    /// 
    /// // Or even a portion of the source buffer to an offset within the destination buffer.
    /// src[5..10].CopyTo(destination[10..]);
    /// ]]>
    /// </code>
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// If the data pointed to is larger than 2 GB, then an exception is thrown because the <see cref="Span{T}"/> type only supports 32 bit <see cref="int"/> values for its <see cref="Span{T}.Length"/>.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(Span<T> destination)
    {
        if (_ptr is null)
        {
            throw new NullReferenceException();
        }

        if (Length > destination.Length)
        {
            throw new ArgumentException(Resources.GOR_ERR_DESTINATION_TOO_SMALL, nameof(destination));
        }

        NativeMemory.Copy(_ptr, Unsafe.AsPointer(ref destination[0]), (nuint)(SizeInBytes.Min(int.MaxValue)));
    }

    /// <summary>
    /// Function to create a new array and copy the contents of the memory pointed at by this pointer into it.
    /// </summary>
    /// <returns>The populated array.</returns>
    /// <remarks>
    /// <para>
    /// If this pointer is <b>null</b>, then the resulting array will be empty.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] ToArray()
    {
        if (_ptr is null)
        {
            return [];
        }

        T[] result = new T[Length];

        fixed (T* ptr = result)
        {
            NativeMemory.Copy(_ptr, ptr, (nuint)(TypeSize * result.LongLength));
        }
        return result;
    }

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
    public bool CompareMemory(GorgonPtr<T> other)
    {
        if (_ptr == other._ptr)
        {
            return true;
        }

        if ((_ptr is null) || (other._ptr is null))
        {
            return false;
        }

        long dataLength = SizeInBytes;

        if (dataLength != other.SizeInBytes)
        {
            return false;
        }

        byte* leftData = (byte*)_ptr;
        byte* rightData = (byte*)other._ptr;

        if ((Vector512.IsHardwareAccelerated) && (dataLength >= 64))
        {
            do
            {
                Vector512<ulong> l = Unsafe.ReadUnaligned<Vector512<ulong>>((ulong*)leftData);
                Vector512<ulong> r = Unsafe.ReadUnaligned<Vector512<ulong>>((ulong*)rightData);

                if (l != r)
                {
                    return false;
                }

                leftData += 64;
                rightData += 64;
                dataLength -= 64;
            }
            while (dataLength >= 64);

            if (dataLength == 0)
            {
                return true;
            }
        }

        if ((Vector256.IsHardwareAccelerated) && (dataLength >= 32))
        {
            do
            {
                Vector256<ulong> l = Unsafe.ReadUnaligned<Vector256<ulong>>((ulong*)leftData);
                Vector256<ulong> r = Unsafe.ReadUnaligned<Vector256<ulong>>((ulong*)rightData);

                if (l != r)
                {
                    return false;
                }

                leftData += 32;
                rightData += 32;
                dataLength -= 32;
            }
            while (dataLength >= 32);

            if (dataLength == 0)
            {
                return true;
            }
        }

        if ((Vector128.IsHardwareAccelerated) && (dataLength >= 16))
        {
            do
            {
                Vector128<ulong> l = Unsafe.ReadUnaligned<Vector128<ulong>>((ulong*)leftData);
                Vector128<ulong> r = Unsafe.ReadUnaligned<Vector128<ulong>>((ulong*)rightData);

                if (l != r)
                {
                    return false;
                }

                leftData += 16;
                rightData += 16;
                dataLength -= 16;
            }
            while (dataLength >= 16);

            if (dataLength == 0)
            {
                return true;
            }
        }

        while (dataLength > 0)
        {
            if (dataLength >= sizeof(long))
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

            if (dataLength >= sizeof(int))
            {
                int left = *((int*)leftData);
                int right = *((int*)rightData);

                if (left != right)
                {
                    return false;
                }

                leftData += sizeof(int);
                rightData += sizeof(int);
                dataLength -= sizeof(int);
                continue;
            }

            if (dataLength >= sizeof(short))
            {
                short left = *((short*)leftData);
                short right = *((short*)rightData);

                if (left != right)
                {
                    return false;
                }

                leftData += sizeof(short);
                rightData += sizeof(short);
                dataLength -= sizeof(short);
                continue;
            }

            if ((*leftData) != (*rightData))
            {
                return false;
            }

            leftData++;
            rightData++;
            dataLength--;
        }

        return true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonPtr{T}" /> struct.
    /// </summary>
    /// <param name="ptr">The pointer to memory to wrap with this pointer.</param>
    /// <param name="indexOffset">The offset within the memory block to point at.</param>
    /// <param name="count">The number of items of type <typeparamref name="T"/> to wrap.</param>
    private GorgonPtr(T* ptr, long indexOffset, long count)
    {
        _ptr = ptr;
        _indexOffset = indexOffset;
        Length = count;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonPtr{T}"/> struct.
    /// </summary>
    /// <param name="pointer">The original pointer to memory.</param>
    /// <param name="count">The number of items of type <typeparamref name="T"/> to cover.</param>
    /// <exception cref="NullReferenceException">Thrown if the <paramref name="pointer"/> parameter is equal to <see cref="NullPtr"/>.</exception>
    /// <remarks>
    /// <para>
    /// This is a copy constructor for the <see cref="GorgonPtr{T}"/> type, it also allows making a slice of the pointer by specifying a <paramref name="count"/> value. If the <paramref name="count"/> value 
    /// is 0, then the entirety of the <paramref name="pointer"/> is covered, otherwise only the <paramref name="count"/> is covered, up to the <see cref="Length"/> of <paramref name="pointer"/>.
    /// </para>
    /// </remarks>
    public GorgonPtr(GorgonPtr<T> pointer, long count = 0)
    {
        if (pointer == NullPtr)
        {
            throw new NullReferenceException(Resources.GOR_ERR_PTR_NULL);
        }

        if (count <= 0)
        {
            count = pointer.Length;
        }
        else
        {
            count = pointer.Length.Min(count);
        }

        _indexOffset = 0;
        _ptr = pointer._ptr;
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
    public GorgonPtr(nint pointer, long count)
    {
        if (pointer == IntPtr.Zero)
        {
            throw new NullReferenceException(Resources.GOR_ERR_PTR_NULL);
        }

        if (count < 1)
        {
            throw new ArgumentException(Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL, nameof(count));
        }

        _indexOffset = 0;
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
    public GorgonPtr(nuint pointer, long count)
    {
        if (pointer == UIntPtr.Zero)
        {
            throw new NullReferenceException(Resources.GOR_ERR_PTR_NULL);
        }

        if (count < 1)
        {
            throw new ArgumentException(Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL, nameof(count));
        }

        _indexOffset = 0;
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
    public GorgonPtr(T* pointer, long count)
    {
        if (pointer is null)
        {
            throw new NullReferenceException(Resources.GOR_ERR_PTR_NULL);
        }

        if (count < 1)
        {
            throw new ArgumentException(Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL, nameof(count));
        }

        _indexOffset = 0;
        _ptr = pointer;
        Length = count;
    }
}
