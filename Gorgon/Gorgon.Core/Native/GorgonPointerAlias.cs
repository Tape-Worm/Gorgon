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
// Created: Sunday, June 28, 2015 11:56:49 AM
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
	/// <remarks>
	/// <para>
	/// This aliases an unsafe <c>void *</c> pointer or <see cref="IntPtr"/> to unmanaged native memory. It provides better thread safety than the <see cref="Gorgon.IO.GorgonDataStream"/> object and gives safe access 
	/// when dereferencing the pointer to read or write data.
	/// </para>
	/// <para>
	/// Since this pointer type only aliases a source pointer, it will not take ownership of that pointer. Therefore, the responsibility of cleaning up the memory allocated for the pointer lies solely with the code 
	/// that created the memory in the first place. That is, a call to <see cref="GorgonPointerBase.Dispose()"/> has no effect on this type, and as such, is not necessary.
	/// </para>
	/// <para>
	/// This type is more dangerous than other types since it does not know the size of the allocated memory referenced by the pointer, and relies on the programmer to give this information. Thus, it is important that 
	/// the size information be correct when passing it to this type. For a safer version of this pointer type, use the <see cref="GorgonPointerAliasTyped{T}"/> object. 
	/// </para>
	/// <para>
	/// Like the <see cref="Gorgon.IO.GorgonDataStream"/> object, it provides several methods to <see cref="O:Gorgon.Native.IGorgonPointer.Read">read</see>/<see cref="O:Gorgon.Native.IGorgonPointer.Write">write</see> 
	/// data to the memory pointed at by the internal pointer. With these methods the object can read and write primitive types, arrays, and generic value types.
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
	/// <seealso cref="GorgonPointerAliasTyped{T}"/>
	public class GorgonPointerAlias
		: GorgonPointerBase
	{
		#region Methods.
		/// <summary>
		/// Function to call when the <see cref="IGorgonPointer"/> needs to deallocate memory or release handles.
		/// </summary>
		protected override unsafe void Cleanup()
		{
			// This is just here to shut the compiler up.
			DataPointer = null;
			Size = 0;
		}

		/// <summary>
		/// Function to alias a pointer.
		/// </summary>
		/// <param name="pointer">The pointer to alias.</param>
		/// <param name="size">The size of the data being pointed at, in bytes.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="size"/> parameter is less than 1.</exception>
		/// <remarks>
		/// <para>
		/// Use this method to re-alias a new pointer without having to recreate an instance of this object.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// Care must be taken to ensure that the <paramref name="size"/> is less than, or equal to the amount of memory being pointed at by the <paramref name="pointer"/>. If the size is larger, memory could 
		/// be corrupted.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public unsafe void AliasPointer(void* pointer, long size)
		{
			if (pointer == null)
			{
				throw new ArgumentNullException(nameof(pointer));
			}

			if (size < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(size), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
			}

			DataPointer = (byte*)pointer;
			Size = size;
		}

		/// <summary>
		/// Function to alias a pointer.
		/// </summary>
		/// <param name="pointer">The pointer to alias.</param>
		/// <param name="size">The size of the data being pointed at, in bytes.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="size"/> parameter is less than 1.</exception>
		/// <remarks>
		/// <para>
		/// Use this method to re-alias a new pointer without having to recreate an instance of this object.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// Care must be taken to ensure that the <paramref name="size"/> is less than, or equal to the amount of memory being pointed at by the <paramref name="pointer"/>. If the size is larger, memory could 
		/// be corrupted.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void AliasPointer(IntPtr pointer, long size)
		{
			if (pointer == IntPtr.Zero)
			{
				throw new ArgumentNullException(nameof(pointer));
			}

			if (size < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(size), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
			}

			unsafe
			{
				DataPointer = (byte*)pointer;
				Size = size;
			}
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPointerBase" /> class.
		/// </summary>
		/// <param name="pointer">The native pointer to unmanaged native memory.</param>
		/// <param name="size">Size of the data allocated to the unmanaged memory, in bytes.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer" /> is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="size" /> parameter is less than one.</exception>
		/// <remarks>
		/// <para>
		/// This takes a <c>void *</c> pointer to unmanaged memory, and aliases it to this object. This allows for safe use of the pointer when reading and writing the memory being pointed at.
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointerAlias" /> aliases an existing pointer, it merely provides access to the pointer. It is not responsible for the pointer, and thus will not free or allocate memory on its
		/// behalf. The responsibility of freeing memory assigned to a pointer is placed on the code that allocated the memory in the first place.
		/// </para>
		/// <para>
		/// The memory pointed at by the pointer should be at least 1 byte in length and this should be reflected in the <paramref name="size" /> parameter, otherwise an exception will be thrown.
		/// </para>
		/// <para>
		///   <note type="warning">
		///     <para>
		///		Ensure that the <paramref name="size" /> parameter is correct when using this method. The object has no way to determine how much memory is allocated and therefore, if the size is too large, a
		///		buffer overrun may occur when reading or writing.
		///		</para>
		///   </note>
		/// </para>
		/// </remarks>
		public unsafe GorgonPointerAlias(void* pointer, long size)
		{
			if (pointer == null)
			{
				throw new ArgumentNullException(nameof(pointer));
			}

			if (size < 1)
			{
				throw new ArgumentException(Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL, nameof(size));
			}

			AliasPointer(pointer, size);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPointerBase" /> class.
		/// </summary>
		/// <param name="pointer">The <see cref="IntPtr"/> to unmanaged native memory.</param>
		/// <param name="size">Size of the data allocated to the unmanaged memory, in bytes.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer"/> is set to <see cref="IntPtr.Zero"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="size" /> parameter is less than one.</exception>
		/// <remarks>
		/// <para>
		/// This takes an <see cref="IntPtr"/> to unmanaged memory, and aliases it to this object. This allows for safe use of the pointer when reading and writing the memory being pointed at.
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointerAlias" /> aliases an existing pointer, it merely provides access to the pointer. It is not responsible for the pointer, and thus will not free or allocate memory on its
		/// behalf. The responsibility of freeing memory assigned to a pointer is placed on the code that allocated the memory in the first place.
		/// </para>
		/// <para>
		/// The memory pointed at by the pointer should be at least 1 byte in length and this should be reflected in the <paramref name="size" /> parameter, otherwise an exception will be thrown.
		/// </para>
		/// <para>
		///   <note type="warning">
		///     <para>
		///		Ensure that the <paramref name="size" /> parameter is correct when using this method. The object has no way to determine how much memory is allocated and therefore, if the size is too large, a
		///		buffer overrun may occur when reading or writing.
		///		</para>
		///   </note>
		/// </para>
		/// </remarks>
		public GorgonPointerAlias(IntPtr pointer, long size)
		{
			if (pointer == IntPtr.Zero)
			{
				throw new ArgumentNullException(nameof(pointer));
			}

			if (size < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(size), Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL);
			}
			
			AliasPointer(pointer, size);
		}
		#endregion
	}
}
