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
// Created: Sunday, June 28, 2015 12:00:50 PM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using Gorgon.Math;

namespace Gorgon.Native
{
	/// <summary>
	/// Wraps unmanaged native memory pointed at by a pointer and provides safe access to that pointer.
	/// </summary>
	/// <typeparam name="T">The type of data used to determine how much memory to allocate. Must be a value or primitive type.</typeparam>
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
	/// The size of the memory allocated is determined by the size of the generic type parameter <typeparamref name="T"/>. This type must be a value or primitive type, and when using a value type it is subject to 
	/// the following constraints.
	/// </para>
	/// <list type="bullet">
	///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
	///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
	/// </list>
	/// <para>
	/// Failure to adhere to these constraints will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
	/// reading/writing from the raw memory behind the type, the values may not be the expected places.
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
	/// <seealso cref="GorgonPointerAlias"/>
	public sealed class GorgonPointerAliasTyped<T>
		: GorgonPointerAlias
		where T : struct
	{
		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPointerAliasTyped{T}" /> class.
		/// </summary>
		/// <param name="pointer">The <see cref="IntPtr"/> to unmanaged native memory.</param>
		/// <param name="count">[Optional] The number of items of type <typeparamref name="T"/> stored in unmanaged memory.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer" /> is <b>null</b>.</exception>
		/// <remarks>
		/// <para>
		/// This takes a <see cref="IntPtr"/> to unmanaged memory, and aliases it to this object. This allows for safe use of the pointer when reading and writing the memory being pointed at.
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointerAliasTyped{T}" /> aliases an existing pointer, it merely provides access to the pointer. It is not responsible for the pointer, and thus will not free or allocate memory on its
		/// behalf. The responsibility of freeing memory assigned to a pointer is placed on the code that allocated the memory in the first place.
		/// </para> 
		/// <para>
		/// The optional <paramref name="count"/> parameter determines the amount of data stored in unmanaged memory. The object will take this count and multiply it by the size of <typeparamref name="T"/> to get 
		/// the number of bytes held in unmanaged memory. 
		/// </para>
		/// <para>
		/// <note type="tip">
		/// <para>
		/// The <paramref name="count"/> parameter is the number of <i>items</i> of type <typeparamref name="T"/>, <u>not</u> the number of bytes. 
		/// </para>
		/// </note>
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// If the <paramref name="count"/> is set to greater than 1, then the programmer must ensure that the unmanaged memory is actually large enough to hold the data, otherwise a buffer overrun will occur and 
		/// memory corruption will follow.
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
		public GorgonPointerAliasTyped(IntPtr pointer, int count = 1)
			: base(pointer, DirectAccess.SizeOf<T>() * count.Max(1))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPointerAliasTyped{T}" /> class.
		/// </summary>
		/// <param name="pointer">The native pointer to unmanaged native memory.</param>
		/// <param name="count">[Optional] The number of items of type <typeparamref name="T"/> stored in unmanaged memory.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer" /> is <b>null</b>.</exception>
		/// <remarks>
		/// <para>
		/// This takes a <c>void *</c> pointer to unmanaged memory, and aliases it to this object. This allows for safe use of the pointer when reading and writing the memory being pointed at.
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointerAliasTyped{T}" /> aliases an existing pointer, it merely provides access to the pointer. It is not responsible for the pointer, and thus will not free or allocate memory on its
		/// behalf. The responsibility of freeing memory assigned to a pointer is placed on the code that allocated the memory in the first place.
		/// </para> 
		/// <para>
		/// The optional <paramref name="count"/> parameter determines the amount of data stored in unmanaged memory. The object will take this count and multiply it by the size of <typeparamref name="T"/> to get 
		/// the number of bytes held in unmanaged memory. 
		/// </para>
		/// <para>
		/// <note type="tip">
		/// <para>
		/// The <paramref name="count"/> parameter is the number of <i>items</i> of type <typeparamref name="T"/>, <u>not</u> the number of bytes. 
		/// </para>
		/// </note>
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// If the <paramref name="count"/> is set to greater than 1, then the programmer must ensure that the unmanaged memory is actually large enough to hold the data, otherwise a buffer overrun will occur and 
		/// memory corruption will follow.
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
		public unsafe GorgonPointerAliasTyped(void* pointer, int count = 1)
			: base(pointer, DirectAccess.SizeOf<T>() * count.Max(1))
		{
		}
		#endregion
	}
}
