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
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Properties;
using DX = SharpDX;

namespace Gorgon.Native
{
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
    /// dateInBuffer[1] = DateTime.Now;
    /// ref DateTime currentDateTime = ref dateInBuffer[1];
    /// ]]>
    /// </code>
    /// </para>
    /// <para>
    /// The buffer can also be used to pin an array or value type and act on those items as native memory. Please note that the standard disclaimers about pinning still apply.
    /// </para>
    /// </remarks>
    public sealed unsafe class GorgonNativeBuffer<T>
        : IDisposable, IEquatable<GorgonNativeBuffer<T>>
        where T : unmanaged
    {
        #region Variables.
        // The size, in bytes, of the data type stored in the buffer.
        private readonly int _typeSize;
        // A pointer to native memory.
        private byte* _memoryBlock;
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
        /// Property to return the number of bytes allocated for this buffer.
        /// </summary>
        public int SizeInBytes
        {
            get;
        }

        /// <summary>
        /// Property to return the number of items of type <typeparamref name="T"/> stored in this buffer.
        /// </summary>
        public int Length
        {
            get;
        }

        /// <summary>
        /// Property to return a reference to the item located at the specified index.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">Thrown if the index is less than 0, or greater than/equal to <see cref="Length"/>.</exception>
        public ref T this[int index]
        {
            get
            {
                if ((index < 0) || (index >= Length))
                {
                    throw new ArgumentOutOfRangeException(nameof(index), string.Format(Resources.GOR_ERR_INDEX_OUT_OF_RANGE, index, 0, Length));
                }

                return ref Unsafe.AsRef<T>(_memoryBlock + (index * _typeSize));
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to read a value from the buffer as the specified type.
        /// </summary>
        /// <typeparam name="TCastType">The type to cast to. Must be an unmanaged value type.</typeparam>
        /// <param name="index">The index of the item in buffer to cast.</param>
        /// <returns>The value in the buffer, casted to the required type.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> parameter is less than 0, or greater than/equal to <see cref="Length"/>.</exception>
        /// <remarks>
        /// <para>
        /// This method returns a reference to the value in the buffer, so applications can immediately write back to the value and have it reflected in the buffer. This negates the need for a similar 
        /// Write method.
        /// </para>
        /// </remarks>
        public ref TCastType ReadAs<TCastType>(int index)
            where TCastType : unmanaged
        {
            if ((index < 0) || (index >= Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index), string.Format(Resources.GOR_ERR_INDEX_OUT_OF_RANGE, index, Length));
            }

            return ref Unsafe.AsRef<TCastType>(_memoryBlock + (index * _typeSize));
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
                return;
            }

            // Deallocate memory that we own.
            if ((!_ownsMemory)
                || (_memoryBlock == null))
            {
                return;
            }

            DX.Utilities.FreeMemory((IntPtr)_memoryBlock);
            GC.RemoveMemoryPressure(SizeInBytes);
            _memoryBlock = null;
        }

        /// <summary>
        /// Function to validate parameters used to create a native buffer from a managed array.
        /// </summary>
        /// <typeparam name="TArrayType">The type of element in the array.</typeparam>
        /// <param name="array">The array to validate.</param>
        /// <param name="index">The index within in the array to map to the buffer.</param>
        /// <param name="count">The number of items in the array to map to the buffer.</param>
        internal static void ValidateArrayParams<TArrayType>(TArrayType[] array, int index, int count)
        {
            if (array == null)
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

            if (count == null)
            {
                count = Length - sourceIndex;
            }

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

            Unsafe.CopyBlock(destination._memoryBlock + (destIndex * _typeSize), _memoryBlock + (sourceIndex * _typeSize), (uint)(count * _typeSize));
        }

        /// <summary>
        /// Function to copy the contents of this buffer into an array of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="destination">The destination array that will receive the data.</param>
        /// <param name="sourceIndex">[Optional] The first index to start copying from.</param>
        /// <param name="count">[Optional] The number of items to copy.</param>
        /// <param name="destIndex">[Optional] The destination index in the destination arrayto start copying into.</param>
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

            if (count == null)
            {
                count = Length - sourceIndex;
            }

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
                Unsafe.CopyBlock(destPtr, _memoryBlock + (sourceIndex * _typeSize), (uint)(count * _typeSize));
            }
        }

        /// <summary>
        /// Function to copy the contents of this native buffer into a managed array.
        /// </summary>
        /// <param name="startIndex">[Optional] The index to start copying from.</param>
        /// <param name="count">[Optional] The number of items to copy.</param>
        /// <returns>A new array, containing a copy of the contents of this buffer.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="startIndex"/> parameter is less than 0.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="startIndex"/> + <paramref name="count"/> is too big for this buffer.</exception>
        /// <remarks>
        /// <para>
        /// If the <paramref name="count"/> method is ommitted, then the full length of the source buffer, minus the <paramref name="startIndex"/> is used. 
        /// </para>
        /// <para>
        /// <note type="warning">
        /// This makes a copy of the data and creates a new array object on the heap. This may impact performance if used heavily.
        /// </note>
        /// </para>
        /// </remarks>
        public T[] ToArray(int startIndex = 0, int? count = null)
        {
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
            }

            if (count == null)
            {
                count = Length - startIndex;
            }

            if (count < 0)
            {
                throw new ArgumentException(nameof(count), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
            }

            if (startIndex + count.Value > Length)
            {
                throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, startIndex, count.Value));
            }

            var result = new T[count.Value];

            fixed (T* destPtr = &result[0])
            {
                Unsafe.CopyBlock(destPtr, _memoryBlock + (startIndex * _typeSize), (uint)(count * _typeSize));
            }

            return result;
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
            if (count == null)
            {
                count = array.Length - index;
            }

            ValidateArrayParams(array, index, count.Value);

            int typeSize = Unsafe.SizeOf<T>();
            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);

            return new GorgonNativeBuffer<T>(handle, index * typeSize, count.Value * typeSize, count.Value, typeSize);
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

            return new GorgonNativeBuffer<byte>(handle, 0, srcSize, srcSize, sizeof(byte));
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
        public void CopyTo(Stream stream, int startIndex = 0, int? count = null)
        {
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

            if (count == null)
            {
                count = Length - startIndex;
            }


            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
            }

            if (startIndex + count.Value > Length)
            {
                throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, startIndex, count.Value));
            }

            using (var writer = new GorgonBinaryWriter(stream, true))
            {
                for (int i = 0; i < count.Value; ++i)
                {
                    writer.WriteValue(ref this[i + startIndex]);
                }
            }
        }

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
        public GorgonNativeBuffer<TCastType> Cast<TCastType>()
            where TCastType : unmanaged => new GorgonNativeBuffer<TCastType>(_memoryBlock, SizeInBytes);

        /// <summary>
        /// Function to determine if this buffer is equal to another.
        /// </summary>
        /// <param name="other">The other buffer to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// This method determines equality between two <see cref="GorgonNativeBuffer{T}"/> instances. The method of determining equality in this method is different from the standard <c>==</c> or <c>!=</c> 
        /// operators or even the <see cref="object.Equals(object)"/> method. To determine equality, the set of rules below will be used to tell if two instances are equal or not.
        /// </para>
        /// <para>
        /// <h3>Rules for determining equality</h3>
        /// <list type="bullet">
        /// <listheader>
        ///     <term>Condition</term>
        ///     <description>Returns</description>
        /// </listheader>
        /// <item>
        ///     <term>If <paramref name="other"/> is the same reference as this instance.</term>
        ///     <description><b>true</b></description>
        /// </item>
        /// <item>
        ///     <term>If <paramref name="other"/> <b>false</b>.</term>
        ///     <description><b>false</b></description>
        /// </item>
        /// <item>
        ///     <term>If <paramref name="other"/> is not the same reference as this instance, and but has the same location in memory, and has the same <see cref="SizeInBytes"/>.</term>
        ///     <description><b>true</b></description>
        /// </item>
        /// <item>
        ///     <term>If <paramref name="other"/> is not the same reference as this instance, and does not have the same location in memory, or does not have the same <see cref="SizeInBytes"/>.</term>
        ///     <description><b>false</b></description>
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        public bool Equals(GorgonNativeBuffer<T> other) => other == this ? true : (other != null) && (other._memoryBlock == _memoryBlock) && (SizeInBytes == other.SizeInBytes);

        /// <summary>
        /// Function to determine if this buffer is equal to another.
        /// </summary>
        /// <param name="obj">The other buffer to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// This method determines equality between two <see cref="GorgonNativeBuffer{T}"/> instances. The method of determining equality in this method is different from the standard <c>==</c> or <c>!=</c> 
        /// operators or even the <see cref="object.Equals(object)"/> method. To determine equality, the set of rules below will be used to tell if two instances are equal or not.
        /// </para>
        /// <para>
        /// <h3>Rules for determining equality</h3>
        /// <list type="bullet">
        /// <listheader>
        ///     <term>Condition</term>
        ///     <description>Returns</description>
        /// </listheader>
        /// <item>
        ///     <term>If <paramref name="obj"/> is the same reference as this instance.</term>
        ///     <description><b>true</b></description>
        /// </item>
        /// <item>
        ///     <term>If <paramref name="obj"/> <b>false</b>.</term>
        ///     <description><b>false</b></description>
        /// </item>
        /// <item>
        ///     <term>If <paramref name="obj"/> is not the same reference as this instance, and but has the same location in memory, and has the same <see cref="SizeInBytes"/>.</term>
        ///     <description><b>true</b></description>
        /// </item>
        /// <item>
        ///     <term>If <paramref name="obj"/> is not the same reference as this instance, and does not have the same location in memory, or does not have the same <see cref="SizeInBytes"/>.</term>
        ///     <description><b>false</b></description>
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        public override bool Equals(object obj) => Equals(obj as GorgonNativeBuffer<T>);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() => base.GetHashCode(); // Shut the analyzer up.

        /// <summary>
        /// Explicit operator to return a <see cref="GorgonReadOnlyPointer"/> to the underlying data in the buffer.
        /// </summary>
        /// <param name="buffer">The buffer to retrieve the native pointer from.</param>
        /// <returns>The void pointer to the underlying data in the buffer.</returns>
        /// <remarks>
        /// <para>
        /// The pointer returned is read only, and has bounds checking. It should be safe for normal usage.
        /// </para>
        /// </remarks>
        public static explicit operator GorgonReadOnlyPointer(GorgonNativeBuffer<T> buffer) => buffer._memoryBlock == null ? GorgonReadOnlyPointer.Null : new GorgonReadOnlyPointer(buffer._memoryBlock, buffer.SizeInBytes);

        /// <summary>
        /// Explicit operator to return the pointer to the underlying data in the buffer.
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
        public static explicit operator void*(GorgonNativeBuffer<T> buffer) => buffer == null ? null : buffer._memoryBlock;

        /// <summary>
        /// Function to fill the buffer with a specific value.
        /// </summary>
        /// <param name="clearValue">The value used to fill the buffer.</param>
        public void Fill(byte clearValue) => Unsafe.InitBlock(_memoryBlock, clearValue, (uint)SizeInBytes);

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
        public Stream ToStream(int index = 0, int? count = null)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
            }

            if (count == null)
            {
                count = Length - index;
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
            }

            if (index + count.Value > Length)
            {
                throw new ArgumentException(Resources.GOR_ERR_DATABUFF_BUFFER_OVERRUN);
            }

            count *= _typeSize;
            index *= _typeSize;

            return new UnmanagedMemoryStream(_memoryBlock + index, count.Value, count.Value, FileAccess.ReadWrite);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonNativeBuffer{T}" /> class.
        /// </summary>
        /// <param name="pinnedData">The handle to the pinned data.</param>
        /// <param name="offset">The offset within the pinned data to access.</param>
        /// <param name="size">The size, in bytes, of the pinned data.</param>
        /// <param name="count">The count of items that are pinned.</param>
        /// <param name="typeSize">The size of the data type.</param>
        private GorgonNativeBuffer(GCHandle pinnedData, int offset, int size, int count, int typeSize)
        {
            _typeSize = typeSize;
            SizeInBytes = size;
            Length = count;
            _pinnedArray = pinnedData;
            _memoryBlock = (byte*)(_pinnedArray.AddrOfPinnedObject() + offset);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonNativeBuffer{T}" /> class.
        /// </summary>
        /// <param name="count">The number of items of type <typeparamref name="T"/> to allocate in the buffer.</param>
        /// <param name="alignment">[Optiona] The alignment of the buffer, in bytes.</param>
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

            _typeSize = Unsafe.SizeOf<T>();
            Length = count;
            SizeInBytes = _typeSize * count;
            _memoryBlock = (byte*)DX.Utilities.AllocateClearedMemory(SizeInBytes, align: alignment.Max(0));
            _ownsMemory = true;

            GC.AddMemoryPressure(SizeInBytes);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonNativeBuffer{T}" /> class.
        /// </summary>
        /// <param name="pointer">The pointer to the memory to wrap in this buffer.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// Use this constructor to wrap an existing pointer to memory in a native buffer so it can safely accessed. 
        /// </para>
        /// <para>
        /// This constructor assumes that the developer has called <see cref="GC.AddMemoryPressure"/> prior to constructor. Failure to do so will cause the garbage collector to be unaware of the actual 
        /// memory usage of the application, so it is recommended that the developer call <see cref="GC.AddMemoryPressure"/> prior to creating this object and <see cref="GC.RemoveMemoryPressure"/> after 
        /// the object is disposed.
        /// </para>
        /// </remarks>
        public GorgonNativeBuffer(GorgonReadOnlyPointer pointer)
        {
            _typeSize = Unsafe.SizeOf<T>();
            _memoryBlock = (byte*)pointer;
            Length = pointer.SizeInBytes / _typeSize;
            SizeInBytes = pointer.SizeInBytes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonNativeBuffer{T}" /> class.
        /// </summary>
        /// <param name="pointer">The pointer to the memory to wrap in this buffer.</param>
        /// <param name="sizeInBytes">The size, in bytes, of the memory to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sizeInBytes"/> is less than 0.</exception>
        /// <remarks>
        /// <para>
        /// Use this constructor to wrap an existing pointer to memory in a native buffer so it can safely accessed. 
        /// </para>
        /// <para>
        /// Ensure the <paramref name="sizeInBytes"/> is evenly divisible by the number of bytes in the type <typeparamref name="T"/>. Otherwise, the buffer may be too small to accomodate your data.
        /// </para>
        /// <para>
        /// This constructor assumes that the developer has called <see cref="GC.AddMemoryPressure"/> prior to constructor. Failure to do so will cause the garbage collector to be unaware of the actual 
        /// memory usage of the application, so it is recommended that the developer call <see cref="GC.AddMemoryPressure"/> prior to creating this object and <see cref="GC.RemoveMemoryPressure"/> after 
        /// the object is disposed.
        /// </para>
        /// </remarks>
        public GorgonNativeBuffer(void* pointer, int sizeInBytes)
        {
            if (pointer == null)
            {
                throw new ArgumentNullException(nameof(pointer));
            }

            if (sizeInBytes <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sizeInBytes), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
            }

            _typeSize = Unsafe.SizeOf<T>();
            _memoryBlock = (byte*)pointer;
            Length = sizeInBytes / _typeSize;
            SizeInBytes = sizeInBytes;
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
}
