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
// Created: March 30, 2018 4:36:10 PM
// 
#endregion

using System;
using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.Native
{
    /// <summary>
    /// A value type representing a read only pointer to native memory.
    /// </summary>
    public readonly unsafe struct GorgonReadOnlyPointer
        : IEquatable<GorgonReadOnlyPointer>, IComparable<GorgonReadOnlyPointer>
    {
        #region Variables.
        // The pointer to the memory.
        private readonly byte* _data;

        /// <summary>
        /// A null pointer.
        /// </summary>
        public static readonly GorgonReadOnlyPointer Null = new GorgonReadOnlyPointer();

        /// <summary>
        /// The amount of memory allocated.
        /// </summary>
        public readonly int SizeInBytes;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether or not this pointer is <b>null</b>.
        /// </summary>
        public bool IsNull => _data == null;
        #endregion

        #region Methods.
        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance. </param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />. </returns>
        public override bool Equals(object obj) => obj is GorgonReadOnlyPointer ptr ? ptr.Equals(this) : false;

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(GorgonReadOnlyPointer other) => other._data == _data;

        /// <summary>
        /// Function to read a value from the memory pointed at by this pointer.
        /// </summary>
        /// <typeparam name="T">The type of data to return. Must be an unmanaged value type.</typeparam>
        /// <param name="index">The index of the item in the pointer.</param>
        /// <returns>The value at the index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> parameter is less than 0.</exception>
        /// <exception cref="IndexOutOfRangeException">Thrown when the <paramref name="index"/> exceeds the <see cref="SizeInBytes"/>.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="index"/> is the index based on the size, in bytes, of <typeparamref name="T"/>. That is, if <typeparamref name="T"/> were 48 bytes in size, and the <paramref name="index"/> 
        /// was 10, then the data would be read from an offset of 480 bytes.  For example:
        /// <code lang="csharp">
        /// <![CDATA[
        /// // Allocate a buffer for 64 unsigned 32 bit integers.
        /// GorgonPtr ptr = new GorgonPtr(Marshal.AllocHGlobal(2048).ToPointer(), sizeof(uint) * 64);
        /// 
        /// // Get the 10th integer:
        /// uint valueAtIndex10 = ptr.Read<uint>(10);
        /// ]]>
        /// </code>
        /// </para>
        /// </remarks>
        public T Read<T>(int index)
            where T : unmanaged
        {
            int typeSize = Unsafe.SizeOf<T>();
            int byteOffset = typeSize * index;

            if (byteOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (typeSize + byteOffset > SizeInBytes)
            {
                throw new IndexOutOfRangeException(Resources.GOR_ERR_DATABUFF_BUFFER_OVERRUN);
            }

            return Unsafe.Read<T>(_data + byteOffset);
        }

        /// <summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object. </summary>
        /// <param name="other">An object to compare with this instance. </param>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="other" /> in the sort order.  Zero This instance occurs in the same position in the sort order as <paramref name="other" />. Greater than zero This instance follows <paramref name="other" /> in the sort order. </returns>
        int IComparable<GorgonReadOnlyPointer>.CompareTo(GorgonReadOnlyPointer other) => other._data == _data ? 0 : other._data < _data ? -1 : 1;

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode() => (int)_data;

        /// <summary>Returns the fully qualified type name of this instance.</summary>
        /// <returns>The fully qualified type name.</returns>
        public override string ToString() => Environment.Is64BitOperatingSystem
                ? _data == null ? 0ul.FormatHex() : ((ulong)_data).FormatHex()
                : _data == null ? 0.FormatHex() : ((uint)_data).FormatHex();

        /// <summary>
        /// Function to compare two pointer values.
        /// </summary>
        /// <param name="left">The left pointer to compare.</param>
        /// <param name="right">The right pointer to compare.</param>
        /// <returns>0 if the pointers are equal, -1 if left is less than right, or 1 if left is greater than right.</returns>
        public static int Compare(GorgonReadOnlyPointer left, GorgonReadOnlyPointer right) => ((IComparable<GorgonReadOnlyPointer>)left).CompareTo(right);

        /// <summary>
        /// Explicit operator to return the pointer to the underlying data pointed at by the pointer.
        /// </summary>
        /// <param name="pointer">The pointer to retrieve the native pointer from.</param>
        /// <returns>A void pointer.</returns>
        /// <remarks>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// This method returns the native pointer to the memory address of this pointer. Developers should only use this for interop scenarios where a native call needs a pointer. Manipulation of this 
        /// pointer is not advisable and may cause harm. 
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
        public static explicit operator void*(GorgonReadOnlyPointer pointer) => pointer._data;

        /// <summary>
        /// Operator to test if two pointers are at the same address.
        /// </summary>
        /// <param name="left">The left pointer to compare.</param>
        /// <param name="right">The right pointer to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool operator ==(GorgonReadOnlyPointer left, GorgonReadOnlyPointer right) => left._data == right._data;

        /// <summary>
        /// Operator to test if two pointers are not at the same address.
        /// </summary>
        /// <param name="left">The left pointer to compare.</param>
        /// <param name="right">The right pointer to compare.</param>
        /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
        public static bool operator !=(GorgonReadOnlyPointer left, GorgonReadOnlyPointer right) => left._data == right._data;

        /// <summary>
        /// Operator to test if one pointer is less than the other.
        /// </summary>
        /// <param name="left">The left pointer to compare.</param>
        /// <param name="right">The right pointer to compare.</param>
        /// <returns><b>true</b> if left is less than the right, <b>false</b> if not.</returns>
        public static bool operator <(GorgonReadOnlyPointer left, GorgonReadOnlyPointer right) => left._data < right._data;

        /// <summary>
        /// Operator to test if one pointer is greater than the other.
        /// </summary>
        /// <param name="left">The left pointer to compare.</param>
        /// <param name="right">The right pointer to compare.</param>
        /// <returns><b>true</b> if left is greater than the right, <b>false</b> if not.</returns>
        public static bool operator >(GorgonReadOnlyPointer left, GorgonReadOnlyPointer right) => left._data > right._data;

        /// <summary>
        /// Operator to test if one pointer is less than or equal to the other.
        /// </summary>
        /// <param name="left">The left pointer to compare.</param>
        /// <param name="right">The right pointer to compare.</param>
        /// <returns><b>true</b> if left is less than or equal to the right, <b>false</b> if not.</returns>
        public static bool operator <=(GorgonReadOnlyPointer left, GorgonReadOnlyPointer right) => left._data <= right._data;

        /// <summary>
        /// Operator to test if one pointer is greater than or equal to the other.
        /// </summary>
        /// <param name="left">The left pointer to compare.</param>
        /// <param name="right">The right pointer to compare.</param>
        /// <returns><b>true</b> if left is greater than or equal to the right, <b>false</b> if not.</returns>
        public static bool operator >=(GorgonReadOnlyPointer left, GorgonReadOnlyPointer right) => left._data >= right._data;

        /// <summary>
        /// Operator to convert the pointer to an int32 value.
        /// </summary>
        /// <param name="pointer">The pointer to convert.</param>
        /// <returns>The address as a <see cref="int"/> value.</returns>
        public static implicit operator int(GorgonReadOnlyPointer pointer) => (int)pointer._data;

        /// <summary>
        /// Operator to convert the pointer to an int64 value.
        /// </summary>
        /// <param name="pointer">The pointer to convert.</param>
        /// <returns>The address as a <see cref="long"/> value.</returns>
        public static implicit operator long(GorgonReadOnlyPointer pointer) => (long)pointer._data;

        /// <summary>
        /// Operator to convert the pointer to a uint32 value.
        /// </summary>
        /// <param name="pointer">The pointer to convert.</param>
        /// <returns>The address as a <see cref="uint"/> value.</returns>
        public static implicit operator uint(GorgonReadOnlyPointer pointer) => (uint)pointer._data;

        /// <summary>
        /// Operator to convert the pointer to a uint64 value.
        /// </summary>
        /// <param name="pointer">The pointer to convert.</param>
        /// <returns>The address as a <see cref="ulong"/> value.</returns>
        public static implicit operator ulong(GorgonReadOnlyPointer pointer) => (ulong)pointer._data;

        /// <summary>
        /// Function to return this pointer as a read only reference.
        /// </summary>
        /// <returns>A read only byte reference to the memory pointed at by this pointer.</returns>
        public ref readonly byte ToRef() => ref Unsafe.AsRef<byte>(_data);

        /// <summary>
        /// Function to return a pointer from a reference byte.
        /// </summary>
        /// <param name="refData">The reference to the byte data to wrap in this pointer.</param>
        /// <param name="sizeInBytes">The number of bytes allocated.</param>
        /// <returns>A new <see cref="GorgonReadOnlyPointer"/> pointing at the memory provided by the reference byte.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sizeInBytes"/> parameter is less than 1.</exception>
        public static GorgonReadOnlyPointer FromRef(ref byte refData, int sizeInBytes) => new GorgonReadOnlyPointer(Unsafe.AsPointer(ref refData), sizeInBytes);

        /// <summary>
        /// Function to copy the contents of the memory pointed at by this pointer into a <see cref="GorgonNativeBuffer{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of data in the buffer. Must be an unmanaged value type.</typeparam>
        /// <param name="buffer">The buffer to populate.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is <b>null</b>.</exception>
        public void CopyTo<T>(GorgonNativeBuffer<T> buffer)
            where T : unmanaged => Unsafe.CopyBlock((byte*)buffer, _data, (uint)SizeInBytes);
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonReadOnlyPointer"/> struct.
        /// </summary>
        /// <param name="pointer">The native pointer to memory.</param>
        /// <param name="sizeInBytes">The size in bytes of the memory that was allocated.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> parameter is <b>null</b>.</exception> 
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sizeInBytes"/> parameter is less than 1.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="sizeInBytes"/> must be within the bounds of memory pointed at by <paramref name="pointer"/>. There is no way to confirm if the size is correct, and as such, it is up to the 
        /// developer to ensure that the information provided is accurate or not.
        /// </para>
        /// </remarks>
        public GorgonReadOnlyPointer(void* pointer, int sizeInBytes)
        {
            if (pointer == null)
            {
                throw new ArgumentNullException(nameof(pointer));
            }

            if (sizeInBytes <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sizeInBytes), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
            }

            _data = (byte*)pointer;
            SizeInBytes = sizeInBytes;
        }
        #endregion

    }
}
