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

namespace Gorgon.Native
{
	/// <inheritdoc/>
	public sealed class GorgonPointerAliasTyped<T>
		: GorgonPointerAlias
		where T : struct
	{
		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPointerAliasTyped{T}"/> class.
		/// </summary>
		/// <param name="pointer">The <see cref="IntPtr" /> representing a pointer to unmanaged native memory.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer" /> is <b>null</b>.</exception>
		/// <typeparam name="T">A type to use to determine the amount of memory to allocate. Must be a value or primitive type.</typeparam>
		/// <remarks><para>
		/// This takes an <see cref="IntPtr" /> representing a pointer to unmanaged memory, and aliases it to this object. This allows for safe use of the pointer when reading and writing the memory being
		/// pointed at.
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointerBase" /> aliases an existing pointer, it merely provides access to the pointer. It is not responsible for the pointer, and thus will not free or allocate memory on its
		/// behalf. The responsibility of freeing memory assigned to a pointer is placed on the caller of this object.
		/// </para>
		/// <para>
		/// Unlike the <see cref="GorgonPointerAlias" /> type, this type will derive the size of the data pointed at by <paramref name="pointer" /> from the type indicated by <typeparamref name="T" />.
		/// </para>
		/// <para>
		///   <para>
		/// The type indicated by <typeparamref name="T" /> is used to determine the amount of memory used by the array. This type is subject to the following constraints:
		/// </para>
		///   <list type="bullet">
		///     <item>
		///       <description>The type must be decorated with the <see cref="StructLayoutAttribute" />.</description>
		///     </item>
		///     <item>
		///       <description>The layout for the value type must be <see cref="LayoutKind.Sequential" />, or <see cref="LayoutKind.Explicit" />.</description>
		///     </item>
		///   </list>
		///   <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		///   <note type="warning">
		///     <para>
		/// Ensure that the size, in bytes, of the type indicated by <typeparamref name="T" /> is not larger than the memory allocated. The object has no way to determine how much memory is allocated and therefore,
		/// if the size is too large, a buffer overrun may occur when reading or writing.
		/// </para>
		///   </note>
		/// </para>
		/// <para>
		///   <note type="caution">
		///     <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute" />). Usage of marshalled data with this type will give undefined
		/// results.
		/// </para>
		///   </note>
		/// </para></remarks>
		public GorgonPointerAliasTyped(IntPtr pointer)
			: base(pointer, DirectAccess.SizeOf<T>())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPointerAliasTyped{T}"/> class.
		/// </summary>
		/// <param name="pointer">The pointer to unmanaged native memory.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer" /> is <b>null</b>.</exception>
		/// <typeparam name="T">A type to use to determine the amount of memory to allocate. Must be a value or primitive type.</typeparam>
		/// <remarks><para>
		/// This takes a <c>void *</c> pointer to unmanaged memory, and aliases it to this object. This allows for safe use of the pointer when reading and writing the memory being pointed at.
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointerBase" /> aliases an existing pointer, it merely provides access to the pointer. It is not responsible for the pointer, and thus will not free or allocate memory on its
		/// behalf. The responsibility of freeing memory assigned to a pointer is placed on the caller of this object.
		/// </para>
		/// <para>
		/// Unlike the <see cref="GorgonPointerAlias"/> type, this type will derive the size of the data pointed at by <paramref name="pointer" /> from the type indicated by
		/// <typeparamref name="T" />.
		/// </para>
		/// <para>
		/// The type indicated by <typeparamref name="T" /> is used to determine the amount of memory used by the array. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///   <item>
		///     <description>The type must be decorated with the <see cref="StructLayoutAttribute" />.</description>
		///   </item>
		///   <item>
		///     <description>The layout for the value type must be <see cref="LayoutKind.Sequential" />, or <see cref="LayoutKind.Explicit" />.</description>
		///   </item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <para>
		///   <note type="warning">
		///     <para>
		/// Ensure that the size, in bytes, of the type indicated by <typeparamref name="T" /> is not larger than the memory allocated. The object has no way to determine how much memory is allocated and therefore,
		/// if the size is too large, a buffer overrun may occur when reading or writing.
		/// </para>
		///   </note>
		/// </para>
		/// <para>
		///   <note type="caution">
		///     <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute" />). Usage of marshalled data with this type will give undefined
		/// results.
		/// </para>
		///   </note>
		/// </para></remarks>
		public unsafe GorgonPointerAliasTyped(void* pointer)
			: base(pointer, DirectAccess.SizeOf<T>())
		{
		}
		#endregion
	}
}
