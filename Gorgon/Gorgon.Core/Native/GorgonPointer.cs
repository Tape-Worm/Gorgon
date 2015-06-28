#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Thursday, June 25, 2015 7:45:57 PM
// 
#endregion

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Gorgon.Core.Properties;
using Gorgon.Diagnostics;
using Gorgon.IO;
using Gorgon.Math;

namespace Gorgon.Native
{
	/// <summary>
	/// Wraps unmanaged native memory pointed at by a pointer and provides safe access to that pointer.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This wraps unmanaged native memory that has been allocated and pointed at by a pointer. It provides better thread safety than the <see cref="Gorgon.IO.GorgonDataStream"/> object and gives safe access when  
	/// dereferencing the pointer to read or write data.
	/// </para>
	/// <para>
	/// The object will allow for pinning a managed type or array, aliasing an existing pointer, or allocating memory and using the pointer to that memory. It does this by exposing static methods on the class to 
	/// allow for creation of a specific type of functionality. 
	/// </para>
	/// <para>
	/// Like the <see cref="Gorgon.IO.GorgonDataStream"/> object, it provides several methods to <see cref="O:Gorgon.Native.GorgonPointer.Read">read</see>/<see cref="O:Gorgon.Native.GorgonPointer.Write">write</see> 
	/// data to the memory pointed at by the internal pointer. With these methods the object can read and write primitive types, arrays, and generic value types.
	/// </para>
	/// <para>
	/// <note type="important">
	/// <para>
	/// <para>
	/// When reading or writing generic value types, or arrays with value types as elements, it is important to ensure that the value type meets the following criteria:
	/// </para>
	/// <list type="bullet">
	///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
	///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
	/// </list>
	/// <para>
	/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
	/// reading/writing from the raw memory behind the type, the values may not be the expected places.
	/// </para>
	/// </para>
	/// </note>
	/// </para>
	/// <para>
	/// As mentioned earlier, this object will allow native access to an <see cref="Array"/> type or generic value type. This is done through pinning the array at the specified element index, and returning a pointer 
	/// with the address of that element in the array, or to the location of the generic value type in memory. Because the object is pinned in memory at a fixed location, this may cause performance issues for the 
	/// garbage collector. To mitigate these issues, it is required that the work done with pinned data be done as quickly as possible and a call to the <see cref="Dispose()"/> method for this object is done immediately 
	/// after the work with the pinned data is complete.
	/// </para>
	/// <para>
	/// Also mentioned is the aliasing capability through one of the <see cref="O:Gorgon.Native.GorgonPointer.Alias">Alias</see> static methods. This takes an existing pointer (or <see cref="IntPtr"/>) and wraps it in this 
	/// object type to allow safe access. Because the object only takes a reference to the pointer, the ownership of the pointer still lies with the calling code that passed the pointer to this object. That is, this object 
	/// will <i>not</i> take ownership of the pointer, and as such, a call to <see cref="Dispose()"/> is not necessary in this case.
	/// </para>
	/// <para>
	/// <note type="important">
	/// <para>
	/// Because this object can be used to allocate native memory, it is important that the <see cref="Dispose()"/> method be called when done with this object if the <see cref="O:Gorgon.Native.GorgonPointer.Allocate">Allocate</see> 
	/// methods are used to create the object. Failure to dispose the object may result in a memory leak until the finalizer for the object can be run.
	/// </para>
	/// </note>
	/// </para>
	/// <para>
	/// <note type="caution">
	/// <para>
	/// None of the methods for this object allow for marshalling (i.e. members of a type with the <see cref="MarshalAsAttribute"/>). Passing data that requires marshalling, such as reference types, will not work and 
	/// will give undefined results. Only primitive types, or value types that have value types (this rule still applies to the value type fields as well) or primitive types for its fields should be used.
	/// </para>
	/// </note>
	/// </para>
	/// </remarks>
	/// <seealso cref="Gorgon.IO.GorgonDataStream"/>
	public sealed class GorgonPointer
		: IDisposable
	{
		#region Variables.
		// Flag to indicate that the object has been disposed.
		private bool _disposed;
		// Flag to indicate that the pointer is aliased (i.e. we don't own it).
		private readonly bool _aliased;
		// The pointer to the data.
		private unsafe byte* _data;
		// An atomic for use when multiple threads try to free the memory at once.
		private int _atomicDispose;
		// The handle used to pin an array or object.
		private GCHandle _pinHandle;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the size of the data pointed at by this pointer, in bytes.
		/// </summary>
		public long Size
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the offset into pinned unmanaged memory, in bytes.
		/// </summary>
		/// <remarks>
		/// This is only applicable to buffers created with the <see cref="PinRange{T}(T[],int,int)"/> method with an index that is greater than 0. For all other creation methods, this will return 0.
		/// </remarks>
		public long PinnedOffset
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to allocate unmanaged memory and return a <see cref="GorgonPointer"/> to it.
		/// </summary>
		/// <typeparam name="T">A type to use to determine the amount of memory to allocate. Must be a value or primitive type.</typeparam>
		/// <param name="alignment">[Optional] The alignment of the memory, in bytes.</param>
		/// <returns>The <see cref="GorgonPointer"/> to the newly allocated unmanaged memory.</returns>
		/// <remarks>
		/// <para>
		/// Use this to create a pointer that points to a new block of aligned unmanaged memory. 
		/// </para>
		/// <para>
		/// The <paramref name="alignment"/> parameter is meant to offset the block of memory so that it is set up for optimal access for the CPU. This value should be a power of two.
		/// </para>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of memory to allocate to store a single item of that type. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// Creating a pointer with this method allocates unmanaged memory. The .NET garbage collector is unable to track this memory, and will not free it until the pointer object is ready for finalization. 
		/// This can lead to memory leaks if handled improperly. The best practice is to allocate the memory using this method, and then call <see cref="Dispose()"/> on the pointer object when done with it.
		/// </para>
		/// </note>
		/// </para>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para>
		/// </note>
		/// </para>
		/// <para>
		/// This code was derived from the <c>Utilities.AllocateMemory</c> function from <a href="https://github.com/sharpdx/SharpDX/blob/master/Source/SharpDX/Utilities.cs">SharpDX</a> by Alexandre Mutel.
		/// </para>
		/// </remarks>
		public static GorgonPointer Allocate<T>(int alignment = 16)
			where T : struct
		{
			return Allocate(DirectAccess.SizeOf<T>(), alignment);
		}

		/// <summary>
		/// Function to allocate unmanaged memory and return a <see cref="GorgonPointer"/> to it.
		/// </summary>
		/// <param name="size">Size of the memory to allocate, in bytes.</param>
		/// <param name="alignment">[Optional] The alignment of the memory, in bytes.</param>
		/// <returns>The <see cref="GorgonPointer"/> to the newly allocated unmanaged memory.</returns>
		/// <remarks>
		/// <para>
		/// Use this to create a pointer that points to a new block of aligned unmanaged memory. 
		/// </para>
		/// <para>
		/// The <paramref name="alignment"/> parameter is meant to offset the block of memory so that it is set up for optimal access for the CPU. This value should be a power of two.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// Creating a pointer with this method allocates unmanaged memory. The .NET garbage collector is unable to track this memory, and will not free it until the pointer object is ready for finalization. 
		/// This can lead to memory leaks if handled improperly. The best practice is to allocate the memory using this method, and then call <see cref="Dispose()"/> on the pointer object when done with it.
		/// </para>
		/// </note>
		/// </para>
		/// <para>
		/// This code was derived from the <c>Utilities.AllocateMemory</c> function from <a href="https://github.com/sharpdx/SharpDX/blob/master/Source/SharpDX/Utilities.cs">SharpDX</a> by Alexandre Mutel.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="size"/> is larger than <see cref="int.MaxValue"/> when the application is running as x86.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than, or equal to zero.</exception>
		public static GorgonPointer Allocate(long size, int alignment = 16)
		{
			if (size <= 0)
			{
				throw new ArgumentOutOfRangeException("size", Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
			}
			
			int mask = alignment.Max(1) - 1;
			int ptrSize = IntPtr.Size;

			// Resize to the alignment + pointer offset.
			long alignedSize = size + mask + ptrSize;

			// On x86, we cannot allocate more than 2GB in one shot, so disallow it.
			if ((GorgonComputerInfo.PlatformArchitecture == PlatformArchitecture.x86) && (alignedSize > Int32.MaxValue))
			{
				throw new ArgumentException(Resources.GOR_ERR_DATABUFF_CANNOT_ALLOC_x86, "size");
			}

			// Get our memory.
			IntPtr pointer = Marshal.AllocHGlobal(new IntPtr(alignedSize));

			unsafe
			{
				// Fiddle with our pointer so that it's aligned on the boundary that we requested via the alignment parameter.
				long alignedAddr = (long)((byte*)pointer + sizeof(void*) + mask) & ~mask;

				// Store the actual pointer to the memory block so we know how to free it.
				((IntPtr*)alignedAddr)[-1] = pointer;
				
				return new GorgonPointer((byte *)alignedAddr, size);
			}
		}

		/// <summary>
		/// Function to pin a managed array at the specified index with the specified element count and return a <see cref="GorgonPointer"/> to the memory that was pinned.
		/// </summary>
		/// <typeparam name="T">The type of data in the array. Must be a value or primitive type.</typeparam>
		/// <param name="array">Array containing the items to pin.</param>
		/// <param name="index">Index within the array to pin.</param>
		/// <param name="count">The number of items in the array to pin.</param>
		/// <returns>A newly created <see cref="GorgonPointer"/> to the data for the pinned array.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> parameter is less than zero.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="count"/> parameter is less than one.</para> 
		/// </exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> parameter plus the <paramref name="count"/> parameter exceeds the total length of the <paramref name="array"/>.</exception>
		/// <remarks>
		/// <para>
		/// This takes an array of items and pins the array to a fixed location to allow reading/writing that array via a pointer. The size of the pointer will be dependant upon the number of items specified 
		/// in the <paramref name="count"/> parameter, and the size of the type indicated by <typeparamref name="T"/>, in bytes.
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointer"/> pins a value, it takes that value type and locks it down so the garbage collector cannot move the item around in memory. If unmanaged access to the value was taking 
		/// place, and the value was not pinned, then the value could be freed while accessing it. By locking it down, we ensure that there's a fixed address for that data so that we can dereference it and access 
		/// it via a pointer.
		/// </para>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of memory used by the array along with the <paramref name="count"/> parameter. This type is subject to the following 
		/// constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// Creating a pointer with this method locks a value down so the garbage collector cannot finalize it or move it. This can lead to performance issues with garbage collection. The rule of thumb is to pin 
		/// the value, quickly do the work required, and unpin it via the <see cref="Dispose()"/> method on this object.
		/// </para>
		/// <para>
		/// A call to <see cref="Dispose()"/> is required when finished with a pointer created by this method. Failure to do so can lead to memory leaks until the object is finalized.
		/// </para>
		/// </note>
		/// </para>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public static GorgonPointer PinRange<T>(T[] array, int index, int count)
			where T : struct
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}

			if (array.Length == 0)
			{
				throw new ArgumentException(Resources.GOR_ERR_DATABUFF_PINNED_ARRAY_NO_ELEMENTS, "array");
			}

			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", Resources.GOR_ERR_DATABUFF_INDEX_LESS_THAN_ZERO);
			}

			if (count < 1)
			{
				throw new ArgumentOutOfRangeException("count", string.Format(Resources.GOR_ERR_DATABUFF_COUNT_TOO_SMALL, 1));
			}

			if (index + count > array.Length)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_INDEX_COUNT_TOO_LARGE, index, count));
			}

			int typeSize = DirectAccess.SizeOf<T>();

			return new GorgonPointer(GCHandle.Alloc(array, GCHandleType.Pinned), typeSize * index, typeSize * count);
		}

		/// <summary>
		/// Function to pin an entire managed array and return a <see cref="GorgonPointer"/> to the memory that was pinned.
		/// </summary>
		/// <typeparam name="T">The type of data in the array. Must be a value or primitive type.</typeparam>
		/// <param name="array">Array containing the items to pin.</param>
		/// <returns>A newly created <see cref="GorgonPointer"/> to the data for the pinned array.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>
		/// <para>
		/// This takes an array of items and pins the array to a fixed location to allow reading/writing that array via a pointer. The size of the pointer will be dependant upon the number of items in the array, 
		/// and the size of the type indicated by <typeparamref name="T"/>, in bytes.
		/// </para>
		/// <para>
		/// An array should have at least 1 element before attempting to pin it, if it does not, then an exception will be thrown.
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointer"/> pins a value, it takes that value type and locks it down so the garbage collector cannot move the item around in memory. If unmanaged access to the value was taking 
		/// place, and the value was not pinned, then the value could be freed while accessing it. By locking it down, we ensure that there's a fixed address for that data so that we can dereference it and access 
		/// it via a pointer.
		/// </para>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of memory used by the array. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// Creating a pointer with this method locks a value down so the garbage collector cannot finalize it or move it. This can lead to performance issues with garbage collection. The rule of thumb is to pin 
		/// the value, quickly do the work required, and unpin it via the <see cref="Dispose()"/> method on this object.
		/// </para>
		/// <para>
		/// A call to <see cref="Dispose()"/> is required when finished with a pointer created by this method. Failure to do so can lead to memory leaks until the object is finalized.
		/// </para>
		/// </note>
		/// </para>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public static GorgonPointer PinRange<T>(T[] array)
			where T : struct
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}

			if (array.Length == 0)
			{
				throw new ArgumentException(Resources.GOR_ERR_DATABUFF_PINNED_ARRAY_NO_ELEMENTS, "array");
			}

			return PinRange(array, 0, array.Length);
		}

		/// <summary>
		/// Function to pin a value and return a <see cref="GorgonPointer"/> to the memory that was pinned.
		/// </summary>
		/// <typeparam name="T">The type of data to pin. Must be a value or primitive type.</typeparam>
		/// <param name="value">The value to pin.</param>
		/// <returns>A newly created <see cref="GorgonPointer"/> to the data for the pinned value.</returns>
		/// <remarks>
		/// <para>
		/// This takes a single value and pins it to a fixed location to allow reading/writing via a pointer. The size of the pointer will be dependant upon the size of the type indicated by 
		/// <typeparamref name="T"/>, in bytes.
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointer"/> pins a value, it takes that value type and locks it down so the garbage collector cannot move the item around in memory. If unmanaged access to the value was taking 
		/// place, and the value was not pinned, then the value could be freed while accessing it. By locking it down, we ensure that there's a fixed address for that data so that we can dereference it and access 
		/// it via a pointer.
		/// </para>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of memory used by the array. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// Creating a pointer with this method locks a value down so the garbage collector cannot finalize it or move it. This can lead to performance issues with garbage collection. The rule of thumb is to pin 
		/// the value, quickly do the work required, and unpin it via the <see cref="Dispose()"/> method on this object.
		/// </para>
		/// <para>
		/// A call to <see cref="Dispose()"/> is required when finished with a pointer created by this method. Failure to do so can lead to memory leaks until the object is finalized.
		/// </para>
		/// </note>
		/// </para>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public static GorgonPointer Pin<T>(T value)
			where T : struct
		{
			return new GorgonPointer(GCHandle.Alloc(value, GCHandleType.Pinned), 0, DirectAccess.SizeOf<T>());
		}

		/// <summary>
		/// Function to alias an <see cref="IntPtr"/> to a <see cref="GorgonPointer"/>.
		/// </summary>
		/// <param name="pointer">The <see cref="IntPtr"/> representing a pointer to unmanaged native memory.</param>
		/// <param name="size">Size of the data allocated to the unmanaged memory, in bytes.</param>
		/// <returns>A <see cref="GorgonPointer"/> aliasing the <paramref name="pointer"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> is equal to <see cref="IntPtr.Zero"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than one.</exception>
		/// <remarks>
		/// <para>
		/// This takes a <see cref="IntPtr"/> representing a pointer to unmanaged memory, and aliases it to this object. This allows for safe use of the pointer when reading and writing the memory being 
		/// pointed at. 
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointer"/> aliases an existing pointer, it merely provides access to the pointer. It is not responsible for the pointer, and thus will not free or allocate memory on its 
		/// behalf. The responsibility of freeing memory assigned to a pointer is placed on the caller of this object.
		/// </para>
		/// <para>
		/// The memory pointed at by the <see cref="IntPtr"/> should be at least 1 byte in length and this should be reflected in the <paramref name="size"/> parameter, otherwise an exception will be thrown.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// Ensure that the <paramref name="size"/> parameter is correct when using this method. The object has no way to determine how much memory is allocated and therefore, if the size is too large, a 
		/// buffer overrun may occur when reading or writing.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public static GorgonPointer Alias(IntPtr pointer, int size)
		{
			if (pointer == IntPtr.Zero)
			{
				throw new ArgumentNullException("pointer");
			}

			if (size < 1)
			{
				throw new ArgumentOutOfRangeException("size", Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
			}

			unsafe
			{
				return new GorgonPointer(pointer.ToPointer(), size);
			}
		}

		/// <summary>
		/// Function to alias an <see cref="IntPtr"/> to a <see cref="GorgonPointer"/>.
		/// </summary>
		/// <typeparam name="T">A type to use to determine the amount of memory to allocate. Must be a value or primitive type.</typeparam>
		/// <param name="pointer">The <see cref="IntPtr"/> representing a pointer to unmanaged native memory.</param>
		/// <returns>A <see cref="GorgonPointer"/> aliasing the <paramref name="pointer"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> is <b>null</b>.</exception>
		/// <remarks>
		/// <para>
		/// This takes an <see cref="IntPtr"/> representing a pointer to unmanaged memory, and aliases it to this object. This allows for safe use of the pointer when reading and writing the memory being 
		/// pointed at. 
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointer"/> aliases an existing pointer, it merely provides access to the pointer. It is not responsible for the pointer, and thus will not free or allocate memory on its 
		/// behalf. The responsibility of freeing memory assigned to a pointer is placed on the caller of this object.
		/// </para>
		/// <para>
		/// Unlike the <see cref="Alias(IntPtr, int)"/> method, this method will derive the size of the data pointed at by <paramref name="pointer"/> from the type indicated by <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// Ensure that the size, in bytes, of the type indicated by <typeparamref name="T"/> is not larger than the memory allocated. The object has no way to determine how much memory is allocated and therefore, 
		/// if the size is too large, a buffer overrun may occur when reading or writing.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public static GorgonPointer Alias<T>(IntPtr pointer)
			where T : struct
		{
			if (pointer == IntPtr.Zero)
			{
				throw new ArgumentNullException("pointer");
			}

			unsafe
			{
				return new GorgonPointer(pointer.ToPointer(), DirectAccess.SizeOf<T>());
			}
		}

		/// <summary>
		/// Function to alias an unsafe <c>void *</c> pointer to a <see cref="GorgonPointer"/>.
		/// </summary>
		/// <param name="pointer">The pointer to unmanaged native memory.</param>
		/// <param name="size">Size of the data allocated to the unmanaged memory, in bytes.</param>
		/// <returns>A <see cref="GorgonPointer"/> aliasing the <paramref name="pointer"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than one.</exception>
		/// <remarks>
		/// <para>
		/// This takes a <c>void *</c> pointer to unmanaged memory, and aliases it to this object. This allows for safe use of the pointer when reading and writing the memory being pointed at. 
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointer"/> aliases an existing pointer, it merely provides access to the pointer. It is not responsible for the pointer, and thus will not free or allocate memory on its 
		/// behalf. The responsibility of freeing memory assigned to a pointer is placed on the caller of this object.
		/// </para>
		/// <para>
		/// The memory pointed at by the pointer should be at least 1 byte in length and this should be reflected in the <paramref name="size"/> parameter, otherwise an exception will be thrown.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// Ensure that the <paramref name="size"/> parameter is correct when using this method. The object has no way to determine how much memory is allocated and therefore, if the size is too large, a 
		/// buffer overrun may occur when reading or writing.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public unsafe static GorgonPointer Alias(void* pointer, int size)
		{
			if (pointer == null)
			{
				throw new ArgumentNullException("pointer");
			}

			if (size < 1)
			{
				throw new ArgumentException(Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL, "size");
			}

			return new GorgonPointer(pointer, size);
		}

		/// <summary>
		/// Function to alias an unsafe <c>void *</c> pointer to a <see cref="GorgonPointer"/>.
		/// </summary>
		/// <typeparam name="T">A type to use to determine the amount of memory to allocate. Must be a value or primitive type.</typeparam>
		/// <param name="pointer">The pointer to unmanaged native memory.</param>
		/// <returns>A <see cref="GorgonPointer"/> aliasing the <paramref name="pointer"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> is <b>null</b>.</exception>
		/// <remarks>
		/// <para>
		/// This takes a <c>void *</c> pointer to unmanaged memory, and aliases it to this object. This allows for safe use of the pointer when reading and writing the memory being pointed at. 
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointer"/> aliases an existing pointer, it merely provides access to the pointer. It is not responsible for the pointer, and thus will not free or allocate memory on its 
		/// behalf. The responsibility of freeing memory assigned to a pointer is placed on the caller of this object.
		/// </para>
		/// <para>
		/// Unlike the <c>Alias(void*,int)</c> method, this method will derive the size of the data pointed at by <paramref name="pointer"/> from the type indicated by 
		/// <typeparamref name="T"/>.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// Ensure that the size, in bytes, of the type indicated by <typeparamref name="T"/> is not larger than the memory allocated. The object has no way to determine how much memory is allocated and therefore, 
		/// if the size is too large, a buffer overrun may occur when reading or writing.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public unsafe static GorgonPointer Alias<T>(void* pointer)
			where T : struct
		{
			if (pointer == null)
			{
				throw new ArgumentNullException("pointer");
			}

			return new GorgonPointer(pointer, DirectAccess.SizeOf<T>());
		}

		/// <summary>
		/// Function to read a value from the buffer at the given offset.
		/// </summary>
		/// <typeparam name="T">Type of value to retrieve. Must be a value or primitive type.</typeparam>
		/// <param name="offset">Offset within the buffer to start reading from, in bytes.</param>
		/// <param name="value">The value retrieved from the buffer.</param>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="Size"/> of the buffer.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> value is less than 0.</exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public void Read<T>(long offset, out T value)
			where T : struct
		{
#if DEBUG
			int typeSize = DirectAccess.SizeOf<T>();

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (typeSize + offset > Size)
			{
				throw new InvalidOperationException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, offset, typeSize));
			}
#endif
			unsafe
			{
				byte* ptr = _data + offset;
				DirectAccess.ReadValue(ptr, out value);
			}
		}

		/// <summary>
		/// Function to read a value from the buffer at the given offset.
		/// </summary>
		/// <typeparam name="T">Type of value to retrieve. Must be a value or primitive type.</typeparam>
		/// <param name="offset">Offset within the buffer to start reading from, in bytes.</param>
		/// <returns>The value retrieved from the buffer.</returns>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="Size"/> of the buffer.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> value is less than 0.</exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public T Read<T>(long offset)
			where T : struct
		{
			T value;

			Read(offset, out value);

			return value;
		}

		/// <summary>
		/// Function to read a value from the beginning of the buffer.
		/// </summary>
		/// <typeparam name="T">Type of value to retrieve. Must be a value or primitive type.</typeparam>
		/// <param name="value">The value retrieved from the buffer.</param>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="Size"/> of the buffer.</exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public void Read<T>(out T value)
			where T : struct
		{
#if DEBUG
			int typeSize = DirectAccess.SizeOf<T>();

			if (typeSize > Size)
			{
				throw new InvalidOperationException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, 0, typeSize));
			}
#endif
			unsafe
			{
				DirectAccess.ReadValue(_data, out value);
			}
		}

		/// <summary>
		/// Function to read a value from the beginning of the buffer.
		/// </summary>
		/// <typeparam name="T">Type of value to retrieve. Must be a value or primitive type.</typeparam>
		/// <returns>The value retrieved from the buffer.</returns>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="Size"/> of the buffer.</exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public T Read<T>()
			where T : struct
		{
#if DEBUG
			int typeSize = DirectAccess.SizeOf<T>();

			if (typeSize > Size)
			{
				throw new InvalidOperationException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, 0, typeSize));
			}
#endif
			unsafe
			{
				T value;

				DirectAccess.ReadValue(_data, out value);

				return value;
			}
		}

		/// <summary>
		/// Function to write a value to the buffer at the given offset.
		/// </summary>
		/// <typeparam name="T">Type of value to write. Must be a value or primitive type.</typeparam>
		/// <param name="offset">Offset within the buffer to write, in bytes.</param>
		/// <param name="value">The value to write.</param>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="Size"/> of the buffer.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> value is less than 0.</exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public void Write<T>(long offset, ref T value)
			where T : struct
		{
#if DEBUG
			int typeSize = DirectAccess.SizeOf<T>();

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (typeSize + offset > Size)
			{
				throw new InvalidOperationException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, offset, typeSize));
			}
#endif
			unsafe
			{
				byte* ptr = _data + offset;
				DirectAccess.WriteValue(ptr, ref value);
			}
		}

		/// <summary>
		/// Function to write a value to the buffer at the given offset.
		/// </summary>
		/// <typeparam name="T">Type of value to write. Must be a value or primitive type.</typeparam>
		/// <param name="offset">Offset within the buffer to write, in bytes.</param>
		/// <param name="value">The value to write.</param>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="offset"/> + the size of the type exceeds the <see cref="Size"/> of the buffer.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> value is less than 0.</exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public void Write<T>(long offset, T value)
			where T : struct
		{
			Write(offset, ref value);
		}

		/// <summary>
		/// Function to write a value to the beginning of the buffer.
		/// </summary>
		/// <typeparam name="T">Type of value to write. Must be a value or primitive type.</typeparam>
		/// <param name="value">The value to write.</param>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="Size"/> of the buffer.</exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public void Write<T>(ref T value)
			where T : struct
		{
#if DEBUG
			int typeSize = DirectAccess.SizeOf<T>();

			if (typeSize > Size)
			{
				throw new InvalidOperationException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, 0, typeSize));
			}
#endif
			unsafe
			{
				DirectAccess.WriteValue(_data, ref value);
			}
		}

		/// <summary>
		/// Function to write a value to the beginning of the buffer.
		/// </summary>
		/// <typeparam name="T">Type of value to write. Must be a value or primitive type.</typeparam>
		/// <param name="value">The value to write.</param>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="Size"/> of the buffer.</exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public void Write<T>(T value)
			where T : struct
		{
			Write(ref value);
		}

		/// <summary>
		/// Function to read a range of values from this buffer into an array.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="offset">Offset within the buffer to start reading from, in bytes.</param>
		/// <param name="array">The array that will receive the data from the buffer.</param>
		/// <param name="index">The index in the array to start at.</param>
		/// <param name="count">The number of items to fill the array with.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameters are less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> + the <paramref name="count"/> is larger than the total length of <paramref name="array"/>.
		/// <para>-or-</para>
		/// <para>Thrown when a buffer overrun is detected because the size, in bytes, of the <paramref name="count"/> plus the <paramref name="offset"/> is too large for the buffer.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="offset"/> is less than 0.</para>
		/// </exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public void ReadRange<T>(long offset, T[] array, int index, int count)
			where T : struct
		{
			int arrayByteSize = DirectAccess.SizeOf<T>() * count;

#if DEBUG
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", string.Format(Resources.GOR_ERR_DATABUFF_COUNT_TOO_SMALL, 0));
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", Resources.GOR_ERR_DATABUFF_INDEX_LESS_THAN_ZERO);
			}

			if (index + count > array.Length)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_INDEX_COUNT_TOO_LARGE, index, count));
			}

			if (arrayByteSize + offset > Size)
			{
				throw new ArgumentException(Resources.GOR_ERR_DATABUFF_BUFFER_OVERRUN);
			}
#endif
			unsafe
			{
				if (arrayByteSize <= 0)
				{
					return;
				}

				byte* ptr = _data + offset;

				DirectAccess.ReadArray(ptr, array, index, arrayByteSize);
			}
		}

		/// <summary>
		/// Function to read a range of values from beginning of this buffer into an array.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="array">The array that will receive the data from the buffer.</param>
		/// <param name="index">The index in the array to start at.</param>
		/// <param name="count">The number of items to fill the array with.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameters are less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> + the <paramref name="count"/> is larger than the total length of <paramref name="array"/>.
		/// <para>-or-</para>
		/// <para>Thrown when a buffer overrun is detected because the size, in bytes, of the <paramref name="count"/> is too large for the buffer.</para>
		/// </exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public void ReadRange<T>(T[] array, int index, int count)
			where T : struct
		{
			int arrayByteSize = DirectAccess.SizeOf<T>() * count;

#if DEBUG
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", string.Format(Resources.GOR_ERR_DATABUFF_COUNT_TOO_SMALL, 0));
			}

			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", Resources.GOR_ERR_DATABUFF_INDEX_LESS_THAN_ZERO);
			}

			if (index + count > array.Length)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_INDEX_COUNT_TOO_LARGE, index, count));
			}

			if (arrayByteSize > Size)
			{
				throw new ArgumentException(Resources.GOR_ERR_DATABUFF_BUFFER_OVERRUN);
			}
#endif
			unsafe
			{
				if (arrayByteSize <= 0)
				{
					return;
				}

				DirectAccess.ReadArray(_data, array, index, arrayByteSize);
			}
		}

		/// <summary>
		/// Function to read a range of values from this buffer into an array.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="offset">Offset within the buffer to start reading from, in bytes.</param>
		/// <param name="array">The array that will receive the data from the buffer.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when a buffer overrun is detected because the size, in bytes, of the array length plus the <paramref name="offset"/> is too large for the buffer.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="offset"/> is less than 0.</para>
		/// </exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public void ReadRange<T>(long offset, T[] array)
			where T : struct
		{
#if DEBUG
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
#endif
			ReadRange(offset, array, 0, array.Length);
		}

		/// <summary>
		/// Function to read a range of values from the beginning of this buffer into an array.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="array">The array that will receive the data from the buffer.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when a buffer overrun is detected because the size, in bytes, of the array length is too large for the buffer.</exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public void ReadRange<T>(T[] array)
			where T : struct
		{
#if DEBUG
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
#endif
			ReadRange(array, 0, array.Length);
		}

		/// <summary>
		/// Function to write a range of values from an array into this buffer.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="offset">Offset within the buffer to start writing, in bytes.</param>
		/// <param name="array">The array to copy into the buffer.</param>
		/// <param name="index">The index in the array to start copying from.</param>
		/// <param name="count">The number of items to in the array to copy into the buffer.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameters are less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> + the <paramref name="count"/> is larger than the total length of <paramref name="array"/>.
		/// <para>-or-</para>
		/// <para>Thrown when a buffer overrun is detected because the size, in bytes, of the <paramref name="count"/> plus the <paramref name="offset"/> is too large for the buffer.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="offset"/> is less than 0.</para>
		/// </exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public void WriteRange<T>(long offset, T[] array, int index, int count)
			where T : struct
		{
			int arrayByteSize = DirectAccess.SizeOf<T>() * count;

#if DEBUG
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", string.Format(Resources.GOR_ERR_DATABUFF_COUNT_TOO_SMALL, 0));
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", Resources.GOR_ERR_DATABUFF_INDEX_LESS_THAN_ZERO);
			}

			if (index + count > array.Length)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_INDEX_COUNT_TOO_LARGE, index, count));
			}

			if (arrayByteSize + offset > Size)
			{
				throw new ArgumentException(Resources.GOR_ERR_DATABUFF_BUFFER_OVERRUN);
			}
#endif
			unsafe
			{
				if (arrayByteSize == 0)
				{
					return;
				}

				byte* ptr = _data + offset;
				DirectAccess.WriteArray(ptr, array, index, arrayByteSize);
			}
		}

		/// <summary>
		/// Function to write a range of values from an array into this buffer.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="offset">Offset within the buffer to start writing, in bytes.</param>
		/// <param name="array">The array to copy into the buffer.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when a buffer overrun is detected because the size, in bytes, of the array length plus the <paramref name="offset"/> is too large for the buffer.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="offset"/> is less than zero.</para>
		/// </exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public void WriteRange<T>(long offset, T[] array)
			where T : struct
		{
#if DEBUG
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
#endif
			WriteRange(offset, array, 0, array.Length);
		}

		/// <summary>
		/// Function to write a range of values from an array into the beginning of this buffer.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="array">The array to copy into the buffer.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when a buffer overrun is detected because the size, in bytes, of the array length is too large for the buffer.</exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public void WriteRange<T>(T[] array)
			where T : struct
		{
#if DEBUG
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
#endif
			WriteRange(array, 0, array.Length);
		}

		/// <summary>
		/// Function to write a range of values from an array into the beginning of this buffer.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="array">The array to copy into the buffer.</param>
		/// <param name="index">The index in the array to start copying from.</param>
		/// <param name="count">The number of items to in the array to copy into the buffer.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameters are less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> + the <paramref name="count"/> is larger than the total length of <paramref name="array"/>.
		/// <para>-or-</para>
		/// <para>Thrown when a buffer overrun is detected because the size, in bytes, of the <paramref name="count"/> is too large for the buffer.</para>
		/// </exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public void WriteRange<T>(T[] array, int index, int count)
			where T : struct
		{
			int arrayByteSize = DirectAccess.SizeOf<T>() * count;

#if DEBUG
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", string.Format(Resources.GOR_ERR_DATABUFF_COUNT_TOO_SMALL, 0));
			}

			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", Resources.GOR_ERR_DATABUFF_INDEX_LESS_THAN_ZERO);
			}

			if (index + count > array.Length)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_INDEX_COUNT_TOO_LARGE, index, count));
			}

			if (arrayByteSize > Size)
			{
				throw new ArgumentException(Resources.GOR_ERR_DATABUFF_BUFFER_OVERRUN);
			}
#endif
			unsafe
			{
				if (arrayByteSize == 0)
				{
					return;
				}

				DirectAccess.WriteArray(_data, array, index, arrayByteSize);
			}
		}

		/// <summary>
		/// Function to copy data from the specified buffer into this buffer. 
		/// </summary>
		/// <param name="buffer">The buffer to copy the data from.</param>
		/// <param name="sourceOffset">The offset in the source buffer to start copying from, in bytes.</param>
		/// <param name="sourceSize">The number of bytes to copy from the source buffer.</param>
		/// <param name="destinationOffset">[Optional] The offset in this buffer to start copying to, in bytes.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sourceOffset"/>, <paramref name="sourceSize"/>, or the <paramref name="destinationOffset"/> parameters are less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="sourceOffset"/> plus the <paramref name="sourceSize"/> exceeds the size of the source <paramref name="buffer"/>.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="destinationOffset"/> plus the <paramref name="sourceSize"/> exceeds the size of the destination buffer.</para>
		/// </exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public void CopyFrom(GorgonPointer buffer, long sourceOffset, int sourceSize, long destinationOffset = 0)
		{
#if DEBUG
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}

			if (sourceSize < 0)
			{
				throw new ArgumentOutOfRangeException("sourceSize", string.Format(Resources.GOR_ERR_DATABUFF_COUNT_TOO_SMALL, 0));				
			}

			if (sourceOffset < 0)
			{
				throw new ArgumentOutOfRangeException("sourceOffset", Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (destinationOffset < 0)
			{
				throw new ArgumentOutOfRangeException("destinationOffset", Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (sourceOffset + sourceSize > buffer.Size)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, sourceOffset, sourceSize), "sourceOffset");
			}

			if (destinationOffset + sourceSize > Size)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, destinationOffset, sourceSize), "destinationOffset");
			}
#endif
			unsafe
			{
				if (sourceSize == 0)
				{
					return;
				}

				byte* srcPtr = buffer._data + sourceOffset;
				byte* destPtr = _data + destinationOffset;

				DirectAccess.MemoryCopy(destPtr, srcPtr, sourceSize);
			}
		}

		/// <summary>
		/// Function to copy data from the specified buffer into this buffer. 
		/// </summary>
		/// <param name="buffer">The buffer to copy the data from.</param>
		/// <param name="sourceSize">The number of bytes to copy from the source buffer.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sourceSize"/> parameter is less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="sourceSize"/> will exceeds the size of the source <paramref name="buffer"/> or the destination buffer.</exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public void CopyFrom(GorgonPointer buffer, int sourceSize)
		{
#if DEBUG
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}

			if (sourceSize < 0)
			{
				throw new ArgumentOutOfRangeException("sourceSize", string.Format(Resources.GOR_ERR_DATABUFF_COUNT_TOO_SMALL, 0));
			}

			if ((sourceSize > buffer.Size) || (sourceSize > Size))
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, 0, sourceSize), "sourceSize");
			}
#endif
			unsafe
			{
				if (sourceSize == 0)
				{
					return;
				}

				DirectAccess.MemoryCopy(_data, buffer._data, sourceSize);
			}
		}

		/// <summary>
		/// Function to copy data from unmanaged memory pointed at by the <see cref="IntPtr"/> into this buffer.
		/// </summary>
		/// <param name="source">The pointer to copy the data from.</param>
		/// <param name="size">The number of bytes to copy from the source buffer.</param>
		/// <param name="destinationOffset">[Optional] The offset in this buffer to start copying to, in bytes.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="source"/> parameter is <see cref="IntPtr.Zero"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="destinationOffset"/> parameter is less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="destinationOffset"/> plus the <paramref name="size"/> exceeds the size of the destination buffer.</exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public void CopyMemory(IntPtr source, int size, long destinationOffset = 0)
		{
#if DEBUG
			if (source == IntPtr.Zero)
			{
				throw new ArgumentNullException("source");
			}

			if (destinationOffset < 0)
			{
				throw new ArgumentOutOfRangeException("destinationOffset", Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (destinationOffset + size > Size)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, destinationOffset, size), "destinationOffset");
			}
#endif
			unsafe
			{
				if (size == 0)
				{
					return;
				}

				byte* srcPtr = (byte *)source.ToPointer();
				byte* destPtr = _data + destinationOffset;

				DirectAccess.MemoryCopy(destPtr, srcPtr, size);
			}
		}

		/// <summary>
		/// Function to copy data from unmanaged memory pointed at by the pointer into this buffer.
		/// </summary>
		/// <param name="source">The pointer to copy the data from.</param>
		/// <param name="size">The number of bytes to copy from the source buffer.</param>
		/// <param name="destinationOffset">[Optional] The offset in this buffer to start copying to, in bytes.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="source"/> parameter is <see cref="IntPtr.Zero"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="destinationOffset"/> parameter is less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="destinationOffset"/> plus the <paramref name="size"/> exceeds the size of the destination buffer.</exception>
		/// <remarks>
		/// TODO:
		/// </remarks>
		public unsafe void CopyMemory(void* source, int size, long destinationOffset = 0)
		{
#if DEBUG
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			if (destinationOffset < 0)
			{
				throw new ArgumentOutOfRangeException("destinationOffset", Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (destinationOffset + size > Size)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, destinationOffset, size), "destinationOffset");
			}
#endif
			if (size == 0)
			{
				return;
			}

			byte* destPtr = _data + destinationOffset;

			DirectAccess.MemoryCopy(destPtr, source, size);
		}

		/// <summary>
		/// Function to fill this buffer with the specified byte value.
		/// </summary>
		/// <param name="value">The value to fill the buffer with.</param>
		/// <param name="size">The number of bytes to fill.</param>
		/// <param name="offset">[Optional] The offset to start filling at, in bytes.</param>
		public void Fill(byte value, int size, long offset = 0)
		{
#if DEBUG
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (offset + size > Size)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, offset, size), "offset");
			}
#endif

			unsafe
			{
				if (size <= 0)
				{
					return;
				}

				byte* ptr = _data + offset;
				DirectAccess.FillMemory(ptr, value, size);
			}
		}

		/// <summary>
		/// Function to fill this buffer with the specified byte value.
		/// </summary>
		/// <param name="value">The value to fill the buffer with.</param>
		public void Fill(byte value)
		{
			unsafe
			{
				// If the buffer is larger than what we can accomodate, then we'll fill it in chunks.
				if (Size > Int32.MaxValue)
				{
					byte* ptr = _data;
					long size = Size;
					int fillAmount = Int32.MaxValue;

					while (size > 0)
					{
						DirectAccess.FillMemory(ptr, value, fillAmount);

						size -= fillAmount;
						ptr += fillAmount;

						if (size < Int32.MaxValue)
						{
							fillAmount = (int)(Int32.MaxValue - size);
						}
					}

					return;
				}

				DirectAccess.FillMemory(_data, value, (int)Size);
			}
		}

		/// <summary>
		/// Function to fill this buffer with zeroes.
		/// </summary>
		/// <param name="size">The number of bytes to fill.</param>
		/// <param name="offset">[Optional] The offset to start filling at, in bytes.</param>
		public void Zero(int size, long offset = 0)
		{
#if DEBUG
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (offset + size > Size)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, offset, size), "offset");
			}
#endif

			unsafe
			{
				if (size <= 0)
				{
					return;
				}

				byte* ptr = _data + offset;
				DirectAccess.ZeroMemory(ptr, size);
			}
		}

		/// <summary>
		/// Function to fill this buffer with zeroes.
		/// </summary>
		public void Zero()
		{
			unsafe
			{
				// If the buffer is larger than what we can accomodate, then we'll fill it in chunks.
				if (Size > Int32.MaxValue)
				{
					byte* ptr = _data;
					long size = Size;
					int fillAmount = Int32.MaxValue;

					while (size > 0)
					{
						DirectAccess.ZeroMemory(ptr, fillAmount);

						size -= fillAmount;
						ptr += fillAmount;

						if (size < Int32.MaxValue)
						{
							fillAmount = (int)(Int32.MaxValue - size);
						}
					}

					return;
				}

				DirectAccess.ZeroMemory(_data, (int)Size);
			}
		}

		/// <summary>
		/// Function to create a new <see cref="GorgonDataStream"/> from this pointer.
		/// </summary>
		/// <returns>The data stream wrapping this pointer.</returns>
		/// <exception cref="InvalidOperationException">Thrown when the <see cref="Size"/> is larger than 2GB when running as an x86 application.</exception>
		public GorgonDataStream ToDataStream()
		{
			unsafe
			{
				if (Size > Int32.MaxValue)
				{
					throw new InvalidOperationException(Resources.GOR_ERR_DATABUFF_SIZE_TOO_LARGE_FOR_CONVERT);	
				}

				return new GorgonDataStream(_data, (int)Size);
			}
		}

		/// <summary>
		/// Function to copy the memory pointed at by this object into a <see cref="MemoryStream"/>
		/// </summary>
		/// <returns>The <see cref="MemoryStream"/> containing a copy of the data in memory that is pointed at by this object.</returns>
		public MemoryStream CopyToMemoryStream()
		{
			if (Size > Int32.MaxValue)
			{
				throw new InvalidOperationException(Resources.GOR_ERR_DATABUFF_SIZE_TOO_LARGE_FOR_CONVERT);	
			}

			long size = Size;
			var buffer = new byte[80000];
			int bytesCopied = buffer.Length;
			long offset = 0;
			var result = new MemoryStream();

			while (size > 0)
			{
				ReadRange(offset, buffer, 0, bytesCopied);
				result.Write(buffer, 0, bytesCopied);

				size -= bytesCopied;
				offset += bytesCopied;

				if (size < buffer.Length)
				{
					bytesCopied = (int)size;
				}
			}

			return result;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPointer"/> class.
		/// </summary>
		/// <param name="pointer">The pointer to alias.</param>
		/// <param name="size">The size of the unmanaged data, in bytes.</param>
		private unsafe GorgonPointer(void* pointer, int size)
		{
			_data = (byte*)pointer;
			Size = size;
			_aliased = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPointer"/> class.
		/// </summary>
		/// <param name="pinHandle">The handle used to pin the data.</param>
		/// <param name="offset">The offset into the pointer, in bytes.</param>
		/// <param name="size">The size of the data, in bytes.</param>
		private GorgonPointer(GCHandle pinHandle, long offset, long size)
		{
			PinnedOffset = offset;
			_pinHandle = pinHandle;
			Size = size;
			unsafe
			{
				_data = ((byte*)_pinHandle.AddrOfPinnedObject().ToPointer()) + offset;
			}
		}

		/// <summary>
		/// Prevents a default instance of the <see cref="GorgonPointer"/> class from being created.
		/// </summary>
		/// <param name="data">The pointer to the data.</param>
		/// <param name="size">The size of the buffer, in bytes.</param>
		private unsafe GorgonPointer(byte *data, long size)
		{
			_data = data;
			Size = size;

			GC.AddMemoryPressure(size);
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="GorgonPointer"/> class.
		/// </summary>
		~GorgonPointer()
		{
			Dispose(false);
		}
		#endregion
		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			unsafe
			{
				if ((_disposed) || (_data == null) || (_aliased))
				{
					return;
				}

				try
				{
					// Don't care about thread safety if the object is being finalized, by then anything that's using the object 
					// has long since forgotten about it and thread safety shouldn't be a problem.
					if ((disposing) && (Interlocked.Increment(ref _atomicDispose) > 1))
					{
						return;
					}

					// Only clean up that which we've created.
					if (!_aliased) 
					{
						if (!_pinHandle.IsAllocated)
						{
							Marshal.FreeHGlobal(((IntPtr*)_data)[-1]);
							GC.RemoveMemoryPressure(Size);
						}
						else
						{
							_pinHandle.Free();
						}
					}

					_data = null;
					Size = 0;
					_disposed = true;
				}
				finally
				{
					if (disposing)
					{
						Interlocked.Decrement(ref _atomicDispose);
					}
				}
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
