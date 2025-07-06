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
// Created: January 26, 2024 6:50:27 PM
//

using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Math;
using Gorgon.Properties;

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
    : IDisposable, IEnumerable<T>
    where T : unmanaged
{
    // The pointer to unmanaged memory.
    private GorgonPtr<T> _memoryBlock;
    // A pinned array.
    private GCHandle _pinnedArray;
    // Flag to indicate that we allocated this memory ourselves.
    private readonly bool _ownsMemory;

    /// <summary>
    /// Property to return whether this buffer is an alias to a pointer.
    /// </summary>
    /// <remarks>
    /// This property will return <b>true</b> if the memory pointed at is not owned by this buffer. This value will also be <b>true</b> when an array is pinned and wrapped by a <see cref="GorgonNativeBuffer{T}"/>.
    /// </remarks>
    public bool IsAlias => !_ownsMemory;

    /// <summary>
    /// Property to return whether this buffer is for a pinned type.
    /// </summary>
    public bool IsPinned => _pinnedArray.IsAllocated;

    /// <summary>
    /// Property to return the size, in bytes, for the type parameter <typeparamref name="T"/>.
    /// </summary>
    public int TypeSize => _memoryBlock.TypeSize;

    /// <summary>
    /// Property to return the number of bytes allocated for this buffer.
    /// </summary>
    public long SizeInBytes => _memoryBlock.SizeInBytes;

    /// <summary>
    /// Property to return the number of items of type <typeparamref name="T"/> stored in this buffer.
    /// </summary>
    public long Length => _memoryBlock.Length;

    /// <summary>
    /// Property to return the location in memory where this buffer is located.
    /// </summary>
    public long Location => GorgonPtr<T>.ToInt64(_memoryBlock);

    /// <summary>
    /// Property to return the alignment of the memory stored in the buffer.
    /// </summary>
    /// <remarks>
    /// This value is only relevant if the memory was allocated by this buffer. If the memory was pinned, or wrapping previously allocated memory, then this value will be 0.
    /// </remarks>
    public int Alignment
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return a reference to the item located at the specified index.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is less than 0, or greater than/equal to <see cref="Length"/>.</exception>
    /// <remarks>
    /// <para>
    /// This property will return the value as a reference, and as such, it can be assigned to as well. For example:
    /// <code lang="csharp">
    /// <![CDATA[
    /// GorgonNativeBuffer<int> ptr = ...;
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
    public ref T this[long index] => ref _memoryBlock[index];

    /// <summary>
    /// Property to return a slice of this buffer by a given range.
    /// </summary>
    /// <returns>The slice of the buffer memory as a <see cref="GorgonPtr{T}"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="range"/> is less than 0, or greater than/equal to <see cref="Length"/>.</exception>
    /// <remarks>
    /// <para>
    /// This indexer can be used to create a slice of a buffer. For example, if a buffer has 10 items, and we want to create a pointer that points to the 3rd item and 4 items in the buffer, we can do the 
    /// following:
    /// <code lang="csharp">
    /// <![CDATA[
    /// GorgonNativeBuffer<int> data = new(10);
    /// 
    /// for (int i = 0; i < data.Length; ++i)
    /// {
    ///   data[i] = i;
    /// }
    /// 
    /// GorgonPtr<int> sliced = data[3..7];
    ///         
    /// // 'sliced' will now point to: 4, 5, 6, 7
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
    public GorgonPtr<T> this[Range range] => _memoryBlock[range];

    /// <summary>
    /// Function to allocate a block of memory for the buffer.
    /// </summary>
    /// <param name="count">The number of items in the buffer.</param>
    /// <param name="alignment">The alignment for the memory block.</param>    
    /// <param name="init"><b>true</b> to initialize the allocated memory, <b>false</b> to leave as-is.</param>
    private unsafe void Allocate(long count, int alignment, bool init)
    {
        if (!BitOperations.IsPow2(alignment))
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_ALIGNMENT_NOT_POW2, alignment), nameof(alignment));
        }

        nuint size = (nuint)(TypeSize * count);

        // Allocate our aligned block.
        void* ptr = NativeMemory.AlignedAlloc(size, (nuint)alignment);

        _memoryBlock = new GorgonPtr<T>((T*)ptr, count);

        Alignment = alignment;

        if (init)
        {
            NativeMemory.Clear(ptr, size);
        }

        GC.AddMemoryPressure(SizeInBytes);
    }

    /// <summary>
    /// Function to free the memory block for the buffer.
    /// </summary>
    private unsafe void Free() => NativeMemory.AlignedFree((void*)_memoryBlock);

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
            GC.SuppressFinalize(this);
            return;
        }

        Free();
        GC.RemoveMemoryPressure(SizeInBytes);
        _memoryBlock = GorgonPtr<T>.NullPtr;

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to resize the memory block for the buffer.
    /// </summary>
    /// <param name="length">The new numer of items of type T, for the buffer.</param>
    /// <param name="alignment">[Optional] The alignment for the memory block.</param>    
    /// <param name="preserve">[Optional] Flag that controls whether memory contents should be preserved or not.</param>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="length"/> parameter is less than 1.
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="alignment"/> parameter is not a power of two.</para>
    /// </exception>
    /// <exception cref="NullReferenceException">Thrown if the underlying memory was freed.</exception>
    /// <exception cref="InvalidOperationException">Thrown if this buffer was created from a pinned value or array.</exception>
    /// <remarks>
    /// <para>
    /// This will reallocate the underlying memory for the buffer to a new size, and, optionally, a new memory alignment. 
    /// </para>
    /// <para>
    /// The <paramref name="length"/> parameter is the number of items of the type <typeparamref name="T"/> to store in the buffer, it can be smaller or greater than the current buffer size. If it is smaller, 
    /// data loss is inevitable with data at the end of the buffer being truncated.
    /// </para>
    /// <para>
    /// The <paramref name="alignment"/> specifies the memory alignment for the buffer. This is useful for SIMD operations, or for ensuring that the memory is aligned to a specific boundary. If the alignment 
    /// is not a power of two, then an exception will be thrown.
    /// </para>
    /// <para>
    /// When the <paramref name="preserve"/> flag is <b>false</b>, memory is resized and the contents of the memory will no longer be valid. However, when set to <b>true</b>, the new memory is zeroed, and the 
    /// previous memory contents will be copied into the new memory. There will be a performance penalty because of the zeroing, and memory copy operations when this value is <b>true</b>.
    /// copyed
    /// </para>
    /// <para>
    /// If the <paramref name="length"/>, and memory <paramref name="alignment"/> are the same as the current buffer, then this method will do nothing.
    /// </para>
    /// <para>
    /// Do not attempt to resize a buffer that was created from a pinned array or other type. Attempts to do so will result in an exception.
    /// </para>
    /// </remarks>
    public void Resize(long length, int alignment = 16, bool preserve = false)
    {
        if (_memoryBlock == GorgonPtr<T>.NullPtr)
        {
            throw new NullReferenceException();
        }

        if (!_ownsMemory)
        {
            throw new InvalidOperationException(Resources.GOR_ERR_CANNOT_RESIZE_PINNED);
        }

        if ((Length == length) && (alignment == Alignment))
        {
            return;
        }

        if (length < 1)
        {
            throw new ArgumentException(Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL, nameof(length));
        }

        unsafe
        {
            GorgonPtr<T> oldBlock = _memoryBlock;
            long prevSize = SizeInBytes;
            long prevLength = Length;
            bool ownsMemory = _ownsMemory;

            Allocate(length, alignment, true);

            if (preserve)
            {
                long copyCount = length.Min(prevLength);
                oldBlock.Slice(0, copyCount).CopyTo(_memoryBlock);
            }

            // Unpin the type we've pinned.
            if (_pinnedArray.IsAllocated)
            {
                _pinnedArray.Free();
                return;
            }

            // If we did not allocate this memory, then do not free it because we have no idea who's using the original memory.
            if (!ownsMemory)
            {
                return;
            }

            NativeMemory.AlignedFree((void*)oldBlock);
            GC.RemoveMemoryPressure(prevSize);
        }
    }

    /// <summary>
    /// Function to return the underlying <see cref="GorgonPtr{T}"/> for this buffer.
    /// </summary>
    /// <param name="buffer">The buffer to containing the pointer.</param>
    /// <returns>The underlying <see cref="GorgonPtr{T}"/> for the buffer.</returns>
    /// <remarks>
    /// <para>
    /// Be careful that this object is not disposed or collected while using its <see cref="GorgonPtr{T}"/>. Doing so will result in undefined behavior.
    /// </para>
    /// <para>
    /// For example, do <b>NOT</b> do:
    /// <code lang="csharp">
    /// <![CDATA[
    /// GorgonNativeBuffer<int> buffer = new(10);
    /// GorgonPtr<T> ptr = GorgonNativeBuffer<int>.ToGorgonPtr(buffer);
    /// 
    /// buffer.Dispose();
    /// 
    /// Console.WriteLine(ptr[4]); // This may crash the application.
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GorgonPtr<T> ToGorgonPtr(GorgonNativeBuffer<T> buffer) => buffer._memoryBlock;

    /// <summary>
    /// Function to create a new <see cref="GorgonNativeBuffer{T}"/> from a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <param name="span">The span to copy from.</param>
    /// <returns>A new <see cref="GorgonNativeBuffer{T}"/> with the contents of the span.</returns>    
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="span"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// This method creates a new <see cref="GorgonNativeBuffer{T}"/> with its own memory block, and copies the data from the <paramref name="span"/> into the new buffer. Because this is a copy of the 
    /// data in the <paramref name="span"/>, the resulting buffer should call <see cref="Dispose"/> when no longer in use.
    /// </para>
    /// <para>
    /// Because arrays, and <see cref="GorgonPtr{T}"/> can be cast to spans, this method can be used to create a buffer from an array or a <see cref="GorgonPtr{T}"/>:
    /// <code lang="csharp">
    /// <![CDATA[
    /// int[] arr = { 1, 2, 3, 4, 5 };
    /// 
    /// // This will work.
    /// GorgonNativeBuffer<int> buffer = GorgonNativeBuffer<int>.FromSpan(arr);
    /// 
    /// buffer.Dispose();
    /// 
    /// fixed (int * ptr = arr)
    /// {
    ///     // This will work too.
    ///     GorgonPtr<int> gorgonPtr = new(ptr, arr.Length);
    ///     buffer = GorgonNativeBuffer<int>.FromSpan(gorgonPtr);
    /// }
    ///
    /// buffer.Dispose();
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    public unsafe static GorgonNativeBuffer<T> FromSpan(ReadOnlySpan<T> span)
    {
        ArgumentEmptyException.ThrowIfEmpty(span);

        GorgonNativeBuffer<T> result = new(span.Length);

        fixed(T* srcPtr = span)
        {
            NativeMemory.Copy(srcPtr, (byte*)result._memoryBlock, (nuint)result.SizeInBytes);
        }

        return result;
    }

    /// <summary>
    /// Function to fill the buffer with a specific byte value.
    /// </summary>
    /// <param name="clearValue">The byte value used to fill the buffer.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Fill(byte clearValue) => _memoryBlock.Fill(clearValue);

    /// <summary>
    /// Function to interpret a reference to the value at the index as the specified type.
    /// </summary>
    /// <typeparam name="TTo">The type to cast to. Must be an unmanaged value type.</typeparam>
    /// <param name="offset">[Optional] The offset, in bytes, within the memory pointed at this pointer to start at.</param>
    /// <returns>The value in the buffer, casted to the required type.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> parameter is less than 0, or greater than/equal to <see cref="SizeInBytes"/>.</exception>
    /// <remarks>
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
    /// This method returns a reference to the value in the buffer, so applications can immediately write back to the value and have it reflected in the buffer:
    /// <code lang="csharp">
    /// <![CDATA[
    /// GorgonNativeBuffer<int> buffer = ...;
    /// byte newValue = 123;
    /// 
    /// // This will write the byte value 123 at the 2nd byte in the first integer (since the buffer expects a int values).
    /// buffer.AsRef<byte>(1) = newValue;
    /// 
    /// // Get a reference to the 3rd byte in the first integer in the buffer.
    /// ref short intValue = ref buffer.AsRef<short>(2);
    /// 
    /// // This will write the short value 0x7F7F at the 3rd byte in the first integer.
    /// intValue = 0x7F7F;
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TTo AsRef<TTo>(long offset = 0)
        where TTo : unmanaged => ref _memoryBlock.AsRef<TTo>(offset);

    /// <summary>
    /// Function to copy this buffer into a new buffer that interprets the data as another type.
    /// </summary>
    /// <typeparam name="TTo">The type of the data in the new buffer. Must be an unmanaged value type.</typeparam>
    /// <exception cref="InvalidCastException">Thrown if the <see cref="SizeInBytes"/> is smaller than the size (in bytes) of the type <typeparamref name="TTo"/>.</exception>
    /// <returns>A new <see cref="GorgonNativeBuffer{TCastType}"/> containing a copy of the data from the original buffer.</returns>
    /// <remarks>
    /// <para>
    /// This method will copy the contents of the buffer from their original buffer. The type of the data copied will be changed to another type as specified by <typeparamref name="TTo"/>. 
    /// </para>
    /// <para>
    /// The returned buffer will be a new buffer object, with a new memory block. The data in the new buffer will be a copy of the data in the original buffer, but will match the type specified by 
    /// <typeparamref name="TTo"/>. Because this is a copy of the original buffer, changes will not be reflected in the original buffer and this buffer should call <see cref="Dispose"/> when not in use.
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
    /// Because of this, it is best practice that the pointer memory size be evenly divisible by the size of the <typeparamref name="TTo"/> type, and non-zero when converting.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// For example:
    /// <code lang="csharp">
    /// <![CDATA[
    /// GorgonNativeBuffer<int> b1 = new(128);        // Allocate 128 ints, 512 bytes.
    /// GorgonNativeBuffer<byte> b2;
    /// 
    /// // This will create a brand new native buffer with its own memory block.
    /// b2 = b1.ToNativeBuffer<byte>();
    /// 
    /// b1.Dispose(); // Dispose of the original buffer. This will not affect b1 because it owns the memory it is referencing.
    /// b2.Dispose();
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    public GorgonNativeBuffer<TTo> CopyTo<TTo>()
        where TTo : unmanaged
    {
        int toSize = Unsafe.SizeOf<TTo>();
        long newSize = _memoryBlock.SizeInBytes / toSize;

        if ((newSize == 0) || ((newSize * toSize) > _memoryBlock.SizeInBytes))
        {
            throw new InvalidCastException(string.Format(Resources.GOR_ERR_PTR_CONVERT_WIDENING, toSize, _memoryBlock.SizeInBytes));
        }

        GorgonNativeBuffer<TTo> result = new(newSize);

        try
        {
            long actualSize = _memoryBlock.SizeInBytes.Min(result._memoryBlock.SizeInBytes);
            GorgonPtr<byte> src = _memoryBlock.ToPointerType<byte>(actualSize);
            GorgonPtr<byte> dest = result._memoryBlock.ToPointerType<byte>();

            src.CopyTo(dest);
        }
        catch
        {
            result.Dispose();
            throw;
        }

        return result;
    }

    /// <summary>
    /// Function to copy the contents of this buffer into other.
    /// </summary>
    /// <param name="destination">The destination buffer that will receive the data.</param>
    /// <param name="sourceIndex">[Optional] The first index to start copying from.</param>
    /// <param name="count">[Optional] The number of items to copy.</param>
    /// <param name="destIndex">[Optional] The destination index in the destination buffer to start copying into.</param>
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
    /// <para>
    /// Example of copying one buffer to another.
    /// <code lang="csharp">
    /// <![CDATA[
    /// int[] array = { 1, 2, 3, 4, 5 };
    /// using GorgonNativeBuffer<int> sourceBuffer = array.PinAsNativeBuffer();
    /// using GorgonNativeBuffer<int> destBuffer = new(array.Length);
    /// 
    /// sourceBuffer.CopyTo(destBuffer);
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    public void CopyTo(GorgonNativeBuffer<T> destination, long sourceIndex = 0, long? count = null, long destIndex = 0)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sourceIndex, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(destIndex, 0);

        count ??= (Length - sourceIndex);        

        if (sourceIndex + count.Value > _memoryBlock.Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, sourceIndex, count.Value), nameof(sourceIndex));
        }

        if (destIndex + count.Value > destination.Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, destIndex, count.Value), nameof(destIndex));
        }

        GorgonPtr<T> src = _memoryBlock.Slice(sourceIndex, count.Value);
        GorgonPtr<T> dest = destination._memoryBlock.Slice(destIndex, count.Value);
        src.CopyTo(dest);
    }

    /// <summary>
    /// Function to copy the contents of this buffer into an array of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="destination">The destination array that will receive the data.</param>
    /// <param name="sourceIndex">[Optional] The first index to start copying from.</param>
    /// <param name="count">[Optional] The number of items to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destination"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sourceIndex"/> parameter is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="sourceIndex"/> + <paramref name="count"/> is too big for this buffer.</exception>
    /// <remarks>
    /// <para>
    /// If the <paramref name="count"/> parameter is ommitted, then the full length of the source buffer, minus the <paramref name="sourceIndex"/> is used. Ensure that there is enough space in the 
    /// <paramref name="destination"/> buffer to accomodate the amount of data required.
    /// </para>
    /// <para>
    /// Because arrays, and <see cref="GorgonPtr{T}"/> can be cast to spans, this method can be used to copy the buffer to an array or a <see cref="GorgonPtr{T}"/>:
    /// <code lang="csharp">
    /// <![CDATA[
    /// int[] arr = { 1, 2, 3, 4, 5 };
    /// 
    /// GorgonNativeBuffer<int> buffer = arr.PinAsNativeBuffer();
    /// 
    /// int[] newArray = new int[5];
    /// buffer.CopyTo(newArray);
    /// 
    /// GorgonNativeBufer<int> otherBuffer = new(5);
    /// GorgonPtr<int> bufferPtr = otherBuffer;
    /// buffer.CopyTo(bufferPtr);
    ///
    /// otherBuffer.Dispose();
    /// buffer.Dispose();
    /// ]]>
    /// </code>
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// If the data pointed to is larger than 2 GB, only the first 2 GB will be copied into the span. This is because the <see cref="Span{T}"/> type uses a 32 bit index, and length.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public void CopyTo(Span<T> destination, int sourceIndex = 0, int? count = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sourceIndex, 0);

        count ??= (int)((Length - sourceIndex).Min(int.MaxValue));

        if (sourceIndex + count.Value > _memoryBlock.Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, sourceIndex, count.Value));
        }

        if (count.Value > destination.Length)
        {
            throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, 0, count.Value), nameof(destination));
        }

        int destSize = count > int.MaxValue ? int.MaxValue : (int)count.Value;

        _memoryBlock.Slice(sourceIndex, count.Value).CopyTo(destination[..destSize]);
    }

    /// <summary>Returns an enumerator that iterates through the collection.</summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        if (_memoryBlock == GorgonPtr<T>.NullPtr)
        {
            yield break;
        }

        for (int i = 0; i < Length; ++i)
        {
            yield return _memoryBlock[i];
        }
    }

    /// <summary>Returns an enumerator that iterates through a collection.</summary>
    /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Operator to convert this buffer to a <see cref="GorgonPtr{T}"/>.
    /// </summary>
    /// <param name="buffer">The buffer to convert.</param>
    /// <returns>The <see cref="GorgonPtr{T}"/> wrapping the buffer data.</returns>
    /// <remarks>
    /// <para>
    /// This operator is provided as a convenience, but has potential for abuse. Be careful that this object is not disposed or collected while using its <see cref="GorgonPtr{T}"/>. Doing so will result in 
    /// undefined behavior.
    /// </para>
    /// <para>
    /// For example, do <b>NOT</b> do:
    /// <code lang="csharp">
    /// <![CDATA[
    /// GorgonPtr<T> ptr = new GorgonNativeBuffer<T>(10);
    /// 
    /// DoSomething();
    /// 
    /// // The native buffer may be collected at this point, and the memory will be freed.
    /// 
    /// Console.WriteLine(ptr[4]); // This may crash the application.
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    public static implicit operator GorgonPtr<T>(GorgonNativeBuffer<T>? buffer) => buffer == null ? GorgonPtr<T>.NullPtr : ToGorgonPtr(buffer);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonNativeBuffer{T}" /> class.
    /// </summary>
    /// <param name="pinnedData">The handle to the pinned data.</param>
    /// <param name="index">The starting index in the array.</param>
    /// <param name="count">The number of array items to cover.</param>
    internal GorgonNativeBuffer(GCHandle pinnedData, long index, long count)
    {
        _pinnedArray = pinnedData;
        nint ptr = _pinnedArray.AddrOfPinnedObject() + ((nint)(index * TypeSize));
        _memoryBlock = new GorgonPtr<T>(ptr, count);
        Alignment = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonNativeBuffer{T}" /> a <see cref="GorgonPtr{T}"/>.
    /// </summary>
    /// <param name="pointer">The pointer to the memory to wrap with this buffer.</param>
    /// <returns>A new <see cref="GorgonNativeBuffer{T}"/> that points to the same memory that the pointer points at.</returns>
    /// <exception cref="NullReferenceException">Thrown when the <paramref name="pointer"/> value is a <see cref="GorgonPtr{T}.NullPtr"/>.</exception>
    /// <remarks>
    /// <para>
    /// The memory pointed at by the <paramref name="pointer"/> is aliased. This means the memory is not owned by the buffer, and will not be freed when the buffer is disposed.
    /// </para>
    /// </remarks>    
    public GorgonNativeBuffer(GorgonPtr<T> pointer)
    {
        if (pointer == GorgonPtr<T>.NullPtr)
        {
            throw new NullReferenceException();
        }

        _memoryBlock = pointer;
        Alignment = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonNativeBuffer{T}" /> class.
    /// </summary>
    /// <param name="length">The number of items of type <typeparamref name="T"/> to allocate in the buffer.</param>
    /// <param name="alignment">[Optional] The alignment of the buffer, in bytes.</param>
    /// <param name="init">[Optional] <b>true</b> to initialize the buffer with a byte value of 0, or <b>false</b> to leave uninitialized.</param>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="length"/> is less than 0.
    /// <para>-or-</para>
    /// <para>Thrown when the <paramref name="alignment"/> parameter is not a power of two.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// Use this constructor to create a new buffer backed by native memory of a given size and aligned to a boundary for the most efficient memory access. The contents of this memory are 
    /// automatically cleared on allocation.
    /// </para>
    /// <para>
    /// This <paramref name="alignment"/> parameter is used to align the memory to a specific boundary. This can be useful for SIMD operations, or for ensuring that the memory is aligned to a cache line. 
    /// This value <b>must</b> be a power of two or an exception will be thrown.
    /// </para>
    /// <para>
    /// The <paramref name="init"/> parameter is used to clear the allocated memory before use when set to <b>true</b>. This is useful for debugging purposes, but can be a performance hit. If the value is 
    /// set to <b>false</b> then the memory will not be initialized and will contain random data. This is useful for performance, but can be a problem for debugging.
    /// </para>
    /// </remarks>
    public GorgonNativeBuffer(long length, int alignment = 16, bool init = true)
    {
        if (length < 1)
        {
            throw new ArgumentException(Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL, nameof(length));
        }

        Allocate(length, alignment, init);
        _ownsMemory = true;
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="GorgonNativeBuffer{T}" /> class.
    /// </summary>
    ~GorgonNativeBuffer()
    {
        Dispose();
    }
}
