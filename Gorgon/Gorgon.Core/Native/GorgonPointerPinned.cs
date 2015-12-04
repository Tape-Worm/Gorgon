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
// Created: Sunday, June 28, 2015 11:38:39 AM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using Gorgon.Core.Properties;

namespace Gorgon.Native
{
	/// <summary>
	/// Wraps unmanaged native memory pointed at by a pointer and provides safe access to that pointer.
	/// </summary>
	/// <typeparam name="T">The type of value to pin. Must be a value or primitive type.</typeparam>
	/// <remarks>
	/// <para>
	/// This wraps a pointer to unmanaged native memory that has been pinned to a fixed location. It provides better thread safety than the <see cref="Gorgon.IO.GorgonDataStream"/> object and gives safe access when  
	/// dereferencing the pointer to read or write data.
	/// </para>
	/// <para>
	/// This pointer type takes a managed type, specified by the type parameter <typeparamref name="T"/>, and pins its location in memory to a fixed address so a pointer can be used to access the underlying data. 
	/// Because of this, performance issues for the garbage collector will arise if the pinned memory is held for too long. A value should only be pinned for the minimum amount of time needed and then released again.
	/// </para>
	/// <para>
	/// Like the <see cref="Gorgon.IO.GorgonDataStream"/> object, it provides several methods to <see cref="O:Gorgon.Native.IGorgonPointer.Read">read</see>/<see cref="O:Gorgon.Native.IGorgonPointer.Write">write</see> 
	/// data to the memory pointed at by the internal pointer. With these methods the object can read and write primitive types, arrays, and generic value types.
	/// </para>
	/// <para>
	/// <para>
	/// The type indicated by <typeparamref name="T" /> is used to determine the amount of memory, in bytes, that the type occupies. This type is subject to the following constraints:
	/// </para>
	/// <list type="bullet">
	///   <item>
	///     <description>The type must be decorated with the <see cref="LayoutKind" />.</description>
	///   </item>
	///   <item>
	///     <description>The layout for the value type must be <see cref="LayoutKind.Explicit" />, or <see cref="MarshalAsAttribute" />.</description>
	///   </item>
	/// </list>
	/// <para>
	/// Failure to adhere to these constraints will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when
	/// reading/writing from the raw memory behind the type, the values may not be the expected places.
	/// </para>
	/// <note type="important">
	/// <para>
	/// Since this object pins a managed value to a fixed location in memory, it is required that users call the <see cref="GorgonPointerBase.Dispose()"/> method as soon as the object no longer needs to be pinned. 
	/// Failure to dispose this object will result in a memory leak since the garbage collector can no longer move or reclaim the the value that is pinned.
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
	/// <seealso cref="GorgonPointer"/>
	/// <seealso cref="GorgonPointerTyped{T}"/>
	/// <seealso cref="GorgonPointerPinned{T}"/>
	/// <seealso cref="GorgonPointerAlias"/>
	/// <seealso cref="GorgonPointerAliasTyped{T}"/>
	public sealed class GorgonPointerPinned<T>
		: GorgonPointerBase
		where T : struct
	{
		#region Variables.
		// The handle used to pin an array or object.
		private GCHandle _pinHandle;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the offset into pinned unmanaged memory, in bytes.
		/// </summary>
		/// <remarks>
		/// This is only applicable to buffers created with the <see cref="GorgonPointerPinned{T}(T[], int, int)"/> method with an index that is greater than 0. For all other creation methods, this will return 0.
		/// </remarks>
		public long PinnedOffset
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to call when the <see cref="IGorgonPointer"/> needs to deallocate memory or release handles.
		/// </summary>
		protected override unsafe void Cleanup()
		{
			if (DataPointer != null)
			{
				return;
			}

			_pinHandle.Free();
			
			DataPointer = null;
			Size = 0;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPointerPinned{T}"/> class.
		/// </summary>
		/// <param name="array">Array containing the items to pin.</param>
		/// <param name="index">Index within the array to pin.</param>
		/// <param name="count">The number of items in the array to pin.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array" /> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="count" /> parameter is less than zero.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="count" /> parameter is less than one.</para>
		/// </exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="count" /> parameter plus the <paramref name="array" /> parameter exceeds the total length of the <paramref name="count" />.</exception>
		/// <remarks>
		/// <para>
		/// This constructor takes an array of items and pins the array to a fixed location to allow reading/writing that array via a pointer. The size of the memory pointed at will be dependant upon the number of items 
		/// specified in the <paramref name="count" /> parameter, and the size, in bytes, of the generic type parameter, <typeparamref name="T" />.
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointerPinned{T}" /> type pins a value, it takes that value type and locks it down so the garbage collector cannot move the item around in memory. If unmanaged access to the value 
		/// was taking place, and the value was not pinned, then the value could be freed while accessing it. By locking it down, we ensure that there's a fixed address for that data so that we can dereference it and 
		/// access it via a pointer.
		/// </para>
		/// <para>
		///   <note type="important">
		///     <para>
		///		A call to <see cref="GorgonPointerBase.Dispose()" /> is required when finished with this pointer. Failure to do so can lead to memory leaks.
		///		</para>
		///   </note>
		/// </para>
		/// </remarks>
		public GorgonPointerPinned(T[] array, int index, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			if (array.Length == 0)
			{
				throw new ArgumentException(Resources.GOR_ERR_DATABUFF_PINNED_ARRAY_NO_ELEMENTS, nameof(array));
			}

			if (index < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(index), Resources.GOR_ERR_DATABUFF_INDEX_LESS_THAN_ZERO);
			}

			if (count < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(count), String.Format(Resources.GOR_ERR_DATABUFF_COUNT_TOO_SMALL, 1));
			}

			if (index + count > array.Length)
			{
				throw new ArgumentException(String.Format(Resources.GOR_ERR_DATABUFF_INDEX_COUNT_TOO_LARGE, index, count));
			}

			int typeSize = DirectAccess.SizeOf<T>();

			_pinHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			PinnedOffset = typeSize * index;
			Size = typeSize * count;

			unsafe
			{
				DataPointer = ((byte*)_pinHandle.AddrOfPinnedObject().ToPointer()) + PinnedOffset;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPointerPinned{T}"/> class.
		/// </summary>
		/// <param name="array">Array containing the items to pin.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array" /> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the number of items in the array is less than one.</exception>
		/// <remarks>
		/// <para>
		/// This constructor takes an array of items and pins the array to a fixed location to allow reading/writing that array via a pointer. The size of the memory pointed at will be dependant upon the number of items 
		/// in the <paramref name="array"/> parameter, and the size, in bytes, of the generic type parameter, <typeparamref name="T" />.
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointerPinned{T}" /> type pins a value, it takes that value type and locks it down so the garbage collector cannot move the item around in memory. If unmanaged access to the value 
		/// was taking place, and the value was not pinned, then the value could be freed while accessing it. By locking it down, we ensure that there's a fixed address for that data so that we can dereference it and 
		/// access it via a pointer.
		/// </para>
		/// <para>
		/// The type indicated by <typeparamref name="T" /> is used to determine the amount of memory used by the entire array. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///   <item>
		///     <description>The type must be decorated with the <see cref="LayoutKind" />.</description>
		///   </item>
		///   <item>
		///     <description>The layout for the value type must be <see cref="LayoutKind.Explicit" />, or <see cref="MarshalAsAttribute" />.</description>
		///   </item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <para>
		///   <note type="important">
		///     <para>
		///		A call to <see cref="GorgonPointerBase.Dispose()" /> is required when finished with this pointer. Failure to do so can lead to memory leaks.
		///		</para>
		///   </note>
		/// </para>
		/// </remarks>
		public GorgonPointerPinned(T[] array)
			: this(array, 0, array.Length)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPointerPinned{T}"/> class.
		/// </summary>
		/// <param name="value">Reference to a value to pin.</param>
		/// <remarks>
		/// <para>
		/// This constructor takes a <paramref name="value"/> and pins it to a fixed location to allow reading/writing via a pointer. The size of the memory pointed at will be dependant upon the the size, in bytes, of 
		/// the generic type parameter, <typeparamref name="T" />.
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointerPinned{T}" /> type pins a value, it takes that value type and locks it down so the garbage collector cannot move the item around in memory. If unmanaged access to the value 
		/// was taking place, and the value was not pinned, then the value could be freed while accessing it. By locking it down, we ensure that there's a fixed address for that data so that we can dereference it and 
		/// access it via a pointer.
		/// </para>
		/// <para>
		/// The type indicated by <typeparamref name="T" /> is used to determine the amount of memory used by the type. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///   <item>
		///     <description>The type must be decorated with the <see cref="LayoutKind" />.</description>
		///   </item>
		///   <item>
		///     <description>The layout for the value type must be <see cref="LayoutKind.Explicit" />, or <see cref="MarshalAsAttribute" />.</description>
		///   </item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <para>
		///   <note type="important">
		///     <para>
		///		A call to <see cref="GorgonPointerBase.Dispose()" /> is required when finished with this pointer. Failure to do so can lead to memory leaks.
		///		</para>
		///   </note>
		/// </para>
		/// </remarks>
		public GorgonPointerPinned(ref T value)
		{
			int typeSize = DirectAccess.SizeOf<T>();

			_pinHandle = GCHandle.Alloc(value, GCHandleType.Pinned);
			PinnedOffset = 0;
			Size = typeSize;

			unsafe
			{
				DataPointer = ((byte*)_pinHandle.AddrOfPinnedObject().ToPointer()) + PinnedOffset;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPointerPinned{T}"/> class.
		/// </summary>
		/// <param name="value">Value to pin.</param>
		/// <remarks>
		/// <para>
		/// This constructor takes a <paramref name="value"/> and pins it to a fixed location to allow reading/writing via a pointer. The size of the memory pointed at will be dependant upon the the size, in bytes, of 
		/// the generic type parameter, <typeparamref name="T" />.
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointerPinned{T}" /> type pins a value, it takes that value type and locks it down so the garbage collector cannot move the item around in memory. If unmanaged access to the value 
		/// was taking place, and the value was not pinned, then the value could be freed while accessing it. By locking it down, we ensure that there's a fixed address for that data so that we can dereference it and 
		/// access it via a pointer.
		/// </para>
		/// <para>
		/// The type indicated by <typeparamref name="T" /> is used to determine the amount of memory used by the type. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///   <item>
		///     <description>The type must be decorated with the <see cref="LayoutKind" />.</description>
		///   </item>
		///   <item>
		///     <description>The layout for the value type must be <see cref="LayoutKind.Explicit" />, or <see cref="MarshalAsAttribute" />.</description>
		///   </item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <para>
		///   <note type="important">
		///     <para>
		///		A call to <see cref="GorgonPointerBase.Dispose()" /> is required when finished with this pointer. Failure to do so can lead to memory leaks.
		///		</para>
		///   </note>
		/// </para>
		/// </remarks>
		public GorgonPointerPinned(T value)
			: this(ref value)
		{
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="GorgonPointerPinned{T}"/> class.
		/// </summary>
		~GorgonPointerPinned()
		{
			Cleanup();
		}
		#endregion
	}
}
