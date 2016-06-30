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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Gorgon.Core.Properties;

namespace Gorgon.Native
{
	/// <summary>
	/// The base class for the pointer types used by Gorgon.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Please refer to one of the <c>GorgonPointer</c> types in the <b>see also</b> section for more information.
	/// </para>
	/// <note type="warning"> 
	/// <para>
	/// <b><u>Do not</u></b> use this class to build custom pointer types. This class may change at any time in the future and cause major problems. Instead, use the <see cref="IGorgonPointer"/> interface if 
	/// implementing a custom pointer type.  
	/// </para>
	/// <para>
	/// <b><i><u>If this class is used for inheritance, then no support will be given if something breaks.</u></i></b>
	/// </para>
	/// </note>
	/// </remarks>
	/// <seealso cref="IGorgonPointer"/>
	/// <seealso cref="GorgonPointer"/>
	/// <seealso cref="GorgonPointerTyped{T}"/>
	/// <seealso cref="GorgonPointerPinned{T}"/>
	/// <seealso cref="GorgonPointerAlias"/>
	/// <seealso cref="GorgonPointerAliasTyped{T}"/>
	public abstract class GorgonPointerBase
		: IGorgonPointer
	{
		#region Variables.
		// Flag to indicate that the object is not disposed.
		private int _notDisposed = -1;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the pointer to unmanaged memory.
		/// </summary>
		/// <remarks>
		/// Implementors should use this to assign or remove the pointer that the object uses to read/write unmanaged memory. Care should be taken to ensure that this only happens 
		/// at object construction and during either disposal, or during finalization.
		/// </remarks>
		protected unsafe byte* DataPointer
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return whether the pointer has been disposed or not.
		/// </summary>
		public unsafe bool IsDisposed
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return _notDisposed == 0 || DataPointer == null;
			}
		}

		/// <summary>
		/// Property to return the address represented by the <see cref="IGorgonPointer"/>.
		/// </summary>
		/// <remarks>
		/// This is the address to the unmanaged memory block. It is provided for information only, and should not be manipulated directly as memory corruption could occur.
		/// </remarks>
		public long Address
		{
			get
			{
				unsafe
				{
					return (long)DataPointer;
				}
			}
		}

		/// <summary>
		/// Property to return the size, in bytes, of the unmanaged memory block that is pointed at by this <see cref="IGorgonPointer"/>.
		/// </summary>
		public long Size
		{
			get;
			protected set;
		}
		#endregion
		
		#region Methods.
		/// <summary>
		/// Function to safely disable the pointer when disposing, even from multiple threads.
		/// </summary>
		/// <returns><b>true</b> when the object is already disposed, <b>false</b> if not.</returns>
		/// <remarks>
		/// Implementors should use this to indicate whether the object is already in the process of being disposed or not.
		/// </remarks>
		private bool SetDisposed()
		{
			return Interlocked.Exchange(ref _notDisposed, 0) == 0;
		}

		/// <summary>
		/// Function to call when the <see cref="IGorgonPointer"/> needs to deallocate memory or release handles.
		/// </summary>
		protected abstract void Cleanup();

		/// <summary>
		/// Function to read a value from the unmanaged memory at the given offset.
		/// </summary>
		/// <typeparam name="T">Type of value to retrieve. Must be a value or primitive type.</typeparam>
		/// <param name="offset">The offset within unmanaged memory to start reading from, in bytes.</param>
		/// <param name="value">The value retrieved from unmanaged memory.</param>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="IGorgonPointer.Size"/> of the unmanaged memory represented by this <see cref="IGorgonPointer"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> value is less than 0.</exception>
		/// <remarks>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to read, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void Read<T>(long offset, out T value)
			where T : struct
		{
#if DEBUG
			int typeSize = DirectAccess.SizeOf<T>();

			if (_notDisposed == 0)
			{
				throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (typeSize + offset > Size)
			{
				throw new InvalidOperationException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, offset, typeSize));
			}
#endif
			unsafe
			{
				byte* ptr = DataPointer + offset;
				DirectAccess.ReadValue(ptr, out value);
			}
		}

		/// <summary>
		/// Function to read a value from the unmanaged memory at the given offset.
		/// </summary>
		/// <typeparam name="T">Type of value to retrieve. Must be a value or primitive type.</typeparam>
		/// <param name="offset">The offset within unmanaged memory to start reading from, in bytes.</param>
		/// <returns>The value from unmanaged memory.</returns>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="IGorgonPointer.Size"/> of the unmanaged memory represented by this <see cref="IGorgonPointer"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> value is less than 0.</exception>
		/// <remarks>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to read, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public T Read<T>(long offset)
			where T : struct
		{
			T value;

			Read(offset, out value);

			return value;
		}

		/// <summary>
		/// Function to read a value from the beginning of the unmanaged memory pointed at by this object.
		/// </summary>
		/// <typeparam name="T">Type of value to retrieve. Must be a value or primitive type.</typeparam>
		/// <param name="value">The value retrieved from unmanaged memory.</param>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="IGorgonPointer.Size"/> of the unmanaged memory represented by this <see cref="IGorgonPointer"/>.</exception>
		/// <remarks>
		/// <para>
		/// This is equivalent to calling <c>Read&lt;T&gt;(0, out value)</c>, except that it does not perform any addition to the pointer with an offset. This may make this method <i>slightly</i> more 
		/// efficient.
		/// </para>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to read, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void Read<T>(out T value)
			where T : struct
		{
#if DEBUG
			if (_notDisposed == 0)
			{
				throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
			}

			int typeSize = DirectAccess.SizeOf<T>();

			if (typeSize > Size)
			{
				throw new InvalidOperationException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, 0, typeSize));
			}
#endif
			unsafe
			{
				DirectAccess.ReadValue(DataPointer, out value);
			}
		}

		/// <summary>
		/// Function to read a value from the beginning of the unmanaged memory pointed at by this object.
		/// </summary>
		/// <typeparam name="T">Type of value to retrieve. Must be a value or primitive type.</typeparam>
		/// <returns>The value from unmanaged memory.</returns>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="IGorgonPointer.Size"/> of the unmanaged memory represented by this <see cref="IGorgonPointer"/>.</exception>
		/// <remarks>
		/// <para>
		/// This is equivalent to calling <c>T value = Read&lt;T&gt;(0)</c>, except that it does not perform any addition to the pointer with an offset. This may make this method <i>slightly</i> more 
		/// efficient.
		/// </para>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to read, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public T Read<T>()
			where T : struct
		{
#if DEBUG
			if (_notDisposed == 0)
			{
				throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
			}

			int typeSize = DirectAccess.SizeOf<T>();

			if (typeSize > Size)
			{
				throw new InvalidOperationException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, 0, typeSize));
			}
#endif
			unsafe
			{
				T value;

				DirectAccess.ReadValue(DataPointer, out value);

				return value;
			}
		}

		/// <summary>
		/// Function to write a value to unmanaged memory at the given offset.
		/// </summary>
		/// <typeparam name="T">Type of value to write. Must be a value or primitive type.</typeparam>
		/// <param name="offset">The offset within unmanaged memory to start writing into, in bytes.</param>
		/// <param name="value">The value to write to unmanaged memory.</param>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="IGorgonPointer.Size"/> of the unmanaged memory represented by this <see cref="IGorgonPointer"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> value is less than 0.</exception>
		/// <remarks>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to write, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void Write<T>(long offset, ref T value)
			where T : struct
		{
#if DEBUG
			if (_notDisposed == 0)
			{
				throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
			}

			int typeSize = DirectAccess.SizeOf<T>();

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (typeSize + offset > Size)
			{
				throw new InvalidOperationException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, offset, typeSize));
			}
#endif
			unsafe
			{
				byte* ptr = DataPointer + offset;
				DirectAccess.WriteValue(ptr, ref value);
			}
		}

		/// <summary>
		/// Function to write a value to unmanaged memory at the given offset.
		/// </summary>
		/// <typeparam name="T">Type of value to write. Must be a value or primitive type.</typeparam>
		/// <param name="offset">The offset within unmanaged memory to start writing into, in bytes.</param>
		/// <param name="value">The value to write to unmanaged memory.</param>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="IGorgonPointer.Size"/> of the unmanaged memory represented by this <see cref="IGorgonPointer"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> value is less than 0.</exception>
		/// <remarks>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to write, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void Write<T>(long offset, T value)
			where T : struct
		{
			Write(offset, ref value);
		}

		/// <summary>
		/// Function to write a value into the beginning of the unmanaged memory pointed at by this object.
		/// </summary>
		/// <typeparam name="T">Type of value to write. Must be a value or primitive type.</typeparam>
		/// <param name="value">The value to write to unmanaged memory.</param>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="IGorgonPointer.Size"/> of the unmanaged memory represented by this <see cref="IGorgonPointer"/>.</exception>
		/// <remarks>
		/// <para>
		/// This is equivalent to calling <c>Write&lt;T&gt;(0, value)</c>, except that it does not perform any addition to the pointer with an offset. This may make this method <i>slightly</i> more 
		/// efficient.
		/// </para>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to write, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void Write<T>(ref T value)
			where T : struct
		{
#if DEBUG
			if (_notDisposed == 0)
			{
				throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
			}

			int typeSize = DirectAccess.SizeOf<T>();

			if (typeSize > Size)
			{
				throw new InvalidOperationException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, 0, typeSize));
			}
#endif
			unsafe
			{
				DirectAccess.WriteValue(DataPointer, ref value);
			}
		}

		/// <summary>
		/// Function to write a value into the beginning of the unmanaged memory pointed at by this object.
		/// </summary>
		/// <typeparam name="T">Type of value to write. Must be a value or primitive type.</typeparam>
		/// <param name="value">The value to write to unmanaged memory.</param>
		/// <exception cref="InvalidOperationException">Thrown when the the size of the type exceeds the <see cref="IGorgonPointer.Size"/> of the unmanaged memory represented by this <see cref="IGorgonPointer"/>.</exception>
		/// <remarks>
		/// <para>
		/// This is equivalent to calling <c>Write&lt;T&gt;(0, value)</c>, except that it does not perform any addition to the pointer with an offset. This may make this method <i>slightly</i> more 
		/// efficient.
		/// </para>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to write, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void Write<T>(T value)
			where T : struct
		{
			Write(ref value);
		}

		/// <summary>
		/// Function to read a range of values from unmanaged memory, at the specified offset, into an array.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="offset">Offset within the unmanaged memory to start reading from, in bytes.</param>
		/// <param name="array">The array that will receive the data from unmanaged memory.</param>
		/// <param name="index">The index in the array to start at.</param>
		/// <param name="count">The number of items to fill the array with.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameters are less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> + the <paramref name="count"/> is larger than the total length of <paramref name="array"/>.
		/// <para>-or-</para>
		/// <para>Thrown when a buffer overrun is detected because the size, in bytes, of the <paramref name="count"/> plus the <paramref name="offset"/> is too large for the unmanaged memory represented by this <see cref="IGorgonPointer"/>.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="offset"/> is less than 0.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to read, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void ReadRange<T>(long offset, T[] array, int index, int count)
			where T : struct
		{
			int arrayByteSize = DirectAccess.SizeOf<T>() * count;

#if DEBUG
			if (_notDisposed == 0)
			{
				throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
			}

			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), string.Format(Resources.GOR_ERR_DATABUFF_COUNT_TOO_SMALL, 0));
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (index < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(index), Resources.GOR_ERR_DATABUFF_INDEX_LESS_THAN_ZERO);
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

				byte* ptr = DataPointer + offset;

				DirectAccess.ReadArray(ptr, array, index, arrayByteSize);
			}
		}

		/// <summary>
		/// Function to read a range of values into an array from the beginning of unmanaged memory pointed at by this object.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="array">The array that will receive the data from unmanaged memory.</param>
		/// <param name="index">The index in the array to start at.</param>
		/// <param name="count">The number of items to fill the array with.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameters are less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> + the <paramref name="count"/> is larger than the total length of <paramref name="array"/>.</exception>
		/// <remarks>
		/// <para>
		/// This is equivalent to calling <c>ReadRange&lt;T&gt;(0, array, index, count)</c>, except that it does not perform any addition to the pointer with an offset. This may make this method <i>slightly</i> more 
		/// efficient.
		/// </para>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to read, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void ReadRange<T>(T[] array, int index, int count)
			where T : struct
		{
			int arrayByteSize = DirectAccess.SizeOf<T>() * count;

#if DEBUG
			if (_notDisposed == 0)
			{
				throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
			}

			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), string.Format(Resources.GOR_ERR_DATABUFF_COUNT_TOO_SMALL, 0));
			}

			if (index < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(index), Resources.GOR_ERR_DATABUFF_INDEX_LESS_THAN_ZERO);
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

				DirectAccess.ReadArray(DataPointer, array, index, arrayByteSize);
			}
		}

		/// <summary>
		/// Function to read a range of values from unmanaged memory, at the specified offset, into an array.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="offset">Offset within the unmanaged memory to start reading from, in bytes.</param>
		/// <param name="array">The array that will receive the data from unmanaged memory.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when a buffer overrun is detected because the size, in bytes, of the array length plus the <paramref name="offset"/> is too large for the unmanaged memory represented by this <see cref="IGorgonPointer"/>.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="offset"/> is less than 0.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to read, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void ReadRange<T>(long offset, T[] array)
			where T : struct
		{
#if DEBUG
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}
#endif
			ReadRange(offset, array, 0, array.Length);
		}

		/// <summary>
		/// Function to read a range of values into an array from the beginning of unmanaged memory pointed at by this object.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="array">The array that will receive the data from unmanaged memory.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when a buffer overrun is detected because the size, in bytes, of the array length is too large for the unmanaged memory represented by this <see cref="IGorgonPointer"/>.</exception>
		/// <remarks>
		/// <para>
		/// This is equivalent to calling <c>ReadRange&lt;T&gt;(0, array, index, count)</c>, except that it does not perform any addition to the pointer with an offset. This may make this method <i>slightly</i> more 
		/// efficient.
		/// </para>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to read, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void ReadRange<T>(T[] array)
			where T : struct
		{
#if DEBUG
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}
#endif
			ReadRange(array, 0, array.Length);
		}

		/// <summary>
		/// Function to write a range of values into unmanaged memory, at the specified offset, from an array.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="offset">Offset, in bytes, within the unmanaged memory to start writing into.</param>
		/// <param name="array">The array that will be copied into unmanaged memory.</param>
		/// <param name="index">The index in the array to start copying from.</param>
		/// <param name="count">The number of items to in the array to copy.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameters are less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> + the <paramref name="count"/> is larger than the total length of <paramref name="array"/>.
		/// <para>-or-</para>
		/// <para>Thrown when a buffer overrun is detected because the size, in bytes, of the <paramref name="count"/> plus the <paramref name="offset"/> is too large for the unmanaged memory represented by this <see cref="IGorgonPointer"/>.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="offset"/> is less than 0.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to read, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void WriteRange<T>(long offset, T[] array, int index, int count)
			where T : struct
		{
			int arrayByteSize = DirectAccess.SizeOf<T>() * count;

#if DEBUG
			if (_notDisposed == 0)
			{
				throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
			}

			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), string.Format(Resources.GOR_ERR_DATABUFF_COUNT_TOO_SMALL, 0));
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (index < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(index), Resources.GOR_ERR_DATABUFF_INDEX_LESS_THAN_ZERO);
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

				byte* ptr = DataPointer + offset;
				DirectAccess.WriteArray(ptr, array, index, arrayByteSize);
			}
		}

		/// <summary>
		/// Function to write a range of values into unmanaged memory, at the specified offset, from an array.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="offset">Offset, in bytes, within the unmanaged memory to start writing into.</param>
		/// <param name="array">The array that will be copied into unmanaged memory.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when a buffer overrun is detected because the size, in bytes, of the array length plus the <paramref name="offset"/> is too large for the unmanaged memory represented by this <see cref="IGorgonPointer"/>.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="offset"/> is less than zero.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to read, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void WriteRange<T>(long offset, T[] array)
			where T : struct
		{
#if DEBUG
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}
#endif
			WriteRange(offset, array, 0, array.Length);
		}

		/// <summary>
		/// Function to write a range of values into unmanaged memory, at the specified offset, from an array.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="array">The array that will be copied into unmanaged memory.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when a buffer overrun is detected because the size, in bytes, of the array length is too large for the unmanaged memory represented by this <see cref="IGorgonPointer"/>.</exception>
		/// <remarks>
		/// <para>
		/// This is equivalent to calling <c>WriteRange&lt;T&gt;(0, array)</c>, except that it does not perform any addition to the pointer with an offset. This may make this method <i>slightly</i> more 
		/// efficient.
		/// </para>
		/// <para>
		/// This is equivalent to calling <c>WriteRange&lt;T&gt;(0, array, index, count)</c>, except that it does not perform any addition to the pointer with an offset. This may make this method <i>slightly</i> more 
		/// efficient.
		/// </para>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to read, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void WriteRange<T>(T[] array)
			where T : struct
		{
#if DEBUG
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}
#endif
			WriteRange(array, 0, array.Length);
		}

		/// <summary>
		/// Function to write a range of values into unmanaged memory, at the specified offset, from an array.
		/// </summary>
		/// <typeparam name="T">Type of value in the array. Must be a value or primitive type.</typeparam>
		/// <param name="array">The array that will be copied into unmanaged memory.</param>
		/// <param name="index">The index in the array to start copying from.</param>
		/// <param name="count">The number of items to in the array to copy.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="array"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/>, or the <paramref name="count"/> parameters are less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="index"/> + the <paramref name="count"/> is larger than the total length of <paramref name="array"/>.
		/// <para>-or-</para>
		/// <para>Thrown when a buffer overrun is detected because the size, in bytes, of the <paramref name="count"/> is too large for the unmanaged memory represented by this <see cref="IGorgonPointer"/>.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// This is equivalent to calling <c>WriteRange&lt;T&gt;(0, array, index, count)</c>, except that it does not perform any addition to the pointer with an offset. This may make this method <i>slightly</i> more 
		/// efficient.
		/// </para>
		/// <para>
		/// The type indicated by <typeparamref name="T"/> is used to determine the amount of data to read, in bytes. This type is subject to the following constraints:
		/// </para>
		/// <list type="bullet">
		///		<item><description>The type must be decorated with the <see cref="StructLayoutAttribute"/>.</description></item>
		///		<item><description>The layout for the value type must be <see cref="LayoutKind.Sequential"/>, or <see cref="LayoutKind.Explicit"/>.</description></item>
		/// </list>
		/// <para>
		/// Failure to adhere to these criteria will result in undefined behavior. This must be done because the .NET memory management system may rearrange members of the type for optimal layout, and as such when 
		/// reading/writing from the raw memory behind the type, the values may not be the expected places.
		/// </para>
		/// <note type="caution">
		/// <para>
		/// This method does <b>not</b> support marshalled data (i.e. types with fields decorated with the <see cref="MarshalAsAttribute"/>). Usage of marshalled data with this type will give undefined 
		/// results.
		/// </para> 
		/// </note>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// For performance reasons, exceptions are only thrown from this method when the library is compiled as <b>DEBUG</b>. In <b>RELEASE</b> mode, no inputs will be validated and no exceptions will be thrown.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void WriteRange<T>(T[] array, int index, int count)
			where T : struct
		{
			int arrayByteSize = DirectAccess.SizeOf<T>() * count;

#if DEBUG
			if (_notDisposed == 0)
			{
				throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
			}

			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), string.Format(Resources.GOR_ERR_DATABUFF_COUNT_TOO_SMALL, 0));
			}

			if (index < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(index), Resources.GOR_ERR_DATABUFF_INDEX_LESS_THAN_ZERO);
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

				DirectAccess.WriteArray(DataPointer, array, index, arrayByteSize);
			}
		}

		/// <summary>
		/// Function to copy data from the specified <see cref="IGorgonPointer"/> into this <see cref="IGorgonPointer"/> 
		/// </summary>
		/// <param name="source">The <see cref="IGorgonPointer"/> to copy data from.</param>
		/// <param name="sourceOffset">The offset, in bytes, within the source to start copying from.</param>
		/// <param name="sourceSize">The number of bytes to copy from the source.</param>
		/// <param name="destinationOffset">[Optional] The offset, in bytes, within this <see cref="IGorgonPointer"/> to start copying to.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="source"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sourceOffset"/>, <paramref name="sourceSize"/>, or the <paramref name="destinationOffset"/> parameters are less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="sourceOffset"/> plus the <paramref name="sourceSize"/> exceeds the size of the source <paramref name="source"/>.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="destinationOffset"/> plus the <paramref name="sourceSize"/> exceeds the size of the destination pointer.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// This performs a straight memory block transfer from one <see cref="IGorgonPointer"/> into this <see cref="IGorgonPointer"/>. 
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// The <paramref name="sourceSize"/> is an <see cref="int"/> and not a <see cref="long"/> value (which this object typically supports). This is because the <c>cpblk</c> instruction used to transfer the data is 
		/// limited to an (unsigned) int value, which is about 2GB. In most cases, this should not be an issue. 
		/// </para>
		/// <para>
		/// To mitigate this problem when dealing with blocks of memory larger than 2GB, try copying the data in batches of 2GB within a loop while incrementing the <paramref name="sourceOffset"/> and <paramref name="destinationOffset"/>.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void CopyFrom(IGorgonPointer source, long sourceOffset, int sourceSize, long destinationOffset = 0)
		{
#if DEBUG
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if ((IsDisposed) || (source.IsDisposed))
			{
				throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
			}

			if (sourceSize < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(sourceSize), string.Format(Resources.GOR_ERR_DATABUFF_COUNT_TOO_SMALL, 0));				
			}

			if (sourceOffset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(sourceOffset), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (destinationOffset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(destinationOffset), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (sourceOffset + sourceSize > source.Size)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, sourceOffset, sourceSize), nameof(sourceOffset));
			}

			if (destinationOffset + sourceSize > Size)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, destinationOffset, sourceSize), nameof(destinationOffset));
			}
#endif
			unsafe
			{
				if (sourceSize == 0)
				{
					return;
				}

				byte* srcPtr = (byte *)(source.Address + sourceOffset);
				byte* destPtr = DataPointer + destinationOffset;

				DirectAccess.MemoryCopy(destPtr, srcPtr, sourceSize);
			}
		}

		/// <summary>
		/// Function to copy data from the specified <see cref="IGorgonPointer"/> into this <see cref="IGorgonPointer"/> 
		/// </summary>
		/// <param name="source">The <see cref="IGorgonPointer"/> to copy data from.</param>
		/// <param name="sourceSize">The number of bytes to copy from the source.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="source"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sourceSize"/> parameter is less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="sourceSize"/> will exceeds the size of the source <paramref name="source"/> or the destination pointer.</exception>
		/// <remarks>
		/// <para>
		/// This performs a straight memory block transfer from one <see cref="IGorgonPointer"/> into this <see cref="IGorgonPointer"/>. 
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// The <paramref name="sourceSize"/> is an <see cref="int"/> and not a <see cref="long"/> value (which this object typically supports). This is because the <c>cpblk</c> instruction used to transfer the data is 
		/// limited to an (unsigned) int value, which is about 2GB. In most cases, this should not be an issue. 
		/// </para>
		/// <para>
		/// To mitigate this problem when dealing with blocks of memory larger than 2GB, see the <see cref="IGorgonPointer.CopyFrom(Gorgon.Native.IGorgonPointer,long,int,long)"/> overload.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void CopyFrom(IGorgonPointer source, int sourceSize)
		{
#if DEBUG
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if ((IsDisposed) || (source.IsDisposed))
			{
				throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
			}

			if (sourceSize < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(sourceSize), string.Format(Resources.GOR_ERR_DATABUFF_COUNT_TOO_SMALL, 0));
			}

			if ((sourceSize > source.Size) || (sourceSize > Size))
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, 0, sourceSize), nameof(sourceSize));
			}
#endif
			unsafe
			{
				if (sourceSize == 0)
				{
					return;
				}

				DirectAccess.MemoryCopy(DataPointer, (byte *)source.Address, sourceSize);
			}
		}

		/// <summary>
		/// Function to copy data from unmanaged memory pointed at by a <see cref="IntPtr"/> into the unmanaged memory represented by this <see cref="IGorgonPointer"/>.
		/// </summary>
		/// <param name="source">The <see cref="IntPtr"/> to unmanaged memory to copy the data from.</param>
		/// <param name="size">The number of bytes to copy from the source.</param>
		/// <param name="destinationOffset">[Optional] The offset, in bytes, within this <see cref="IGorgonPointer"/> to start copying to.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="source"/> parameter is <see cref="IntPtr.Zero"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="destinationOffset"/> parameter is less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="destinationOffset"/> plus the <paramref name="size"/> exceeds the size of the destination pointer.</exception>
		/// <remarks>
		/// <para>
		/// This copies memory directly from unmanaged memory represented by an <see cref="IntPtr"/> into this <see cref="IGorgonPointer"/>.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// The <paramref name="size"/> is an <see cref="int"/> and not a <see cref="long"/> value (which this object typically supports). This is because the <c>cpblk</c> instruction used to transfer the data is 
		/// limited to an (unsigned) int value, which is about 2GB. In most cases, this should not be an issue. 
		/// </para>
		/// <para>
		/// To mitigate this problem when dealing with blocks of memory larger than 2GB, try copying the data in batches of 2GB within a loop while incrementing the <paramref name="destinationOffset"/> and the 
		/// <paramref name="source"/>.
		/// </para>
		/// </note>
		/// </para>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// The <paramref name="size"/> parameter is provided by the programmer, and should be less than, or equal to (in bytes) the actual size of unmanaged memory pointed at by <paramref name="source"/>. If 
		/// this value is too large, memory corruption will happen.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void CopyMemory(IntPtr source, int size, long destinationOffset = 0)
		{
#if DEBUG
			if (_notDisposed == 0)
			{
				throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
			}

			if (source == IntPtr.Zero)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (destinationOffset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(destinationOffset), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (destinationOffset + size > Size)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, destinationOffset, size), nameof(destinationOffset));
			}
#endif
			unsafe
			{
				if (size == 0)
				{
					return;
				}

				byte* srcPtr = (byte *)source.ToPointer();
				byte* destPtr = DataPointer + destinationOffset;

				DirectAccess.MemoryCopy(destPtr, srcPtr, size);
			}
		}

		/// <summary>
		/// Function to copy data from unmanaged memory pointed at by an unsafe pointer into the unmanaged memory represented by this <see cref="IGorgonPointer"/>.
		/// </summary>
		/// <param name="source">The unsafe pointer to unmanaged memory to copy the data from.</param>
		/// <param name="size"><see cref="IGorgonPointer.CopyMemory(System.IntPtr,int,long)"/></param>
		/// <param name="destinationOffset"><see cref="IGorgonPointer.CopyMemory(System.IntPtr,int,long)"/></param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="source"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="destinationOffset"/> parameter is less than zero.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="destinationOffset"/> plus the <paramref name="size"/> exceeds the size of the destination pointer.</exception>
		/// <remarks>
		/// <para>
		/// This copies memory directly from unmanaged memory represented by an unsafe pointer (<c>void *</c>) into this <see cref="IGorgonPointer"/>.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// The <paramref name="size"/> is an <see cref="int"/> and not a <see cref="long"/> value (which this object typically supports). This is because the <c>cpblk</c> instruction used to transfer the data is 
		/// limited to an (unsigned) int value, which is about 2GB. In most cases, this should not be an issue. 
		/// </para>
		/// <para>
		/// To mitigate this problem when dealing with blocks of memory larger than 2GB, try copying the data in batches of 2GB within a loop while incrementing the <paramref name="destinationOffset"/> and the 
		/// <paramref name="source"/>.
		/// </para>
		/// </note>
		/// </para>
		/// <para>
		/// <note type="caution">
		/// <para>
		/// The <paramref name="size"/> parameter is provided by the programmer, and should be less than, or equal to (in bytes) the actual size of unmanaged memory pointed at by <paramref name="source"/>. If 
		/// this value is too large, memory corruption will happen.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public unsafe void CopyMemory(void* source, int size, long destinationOffset = 0)
		{
#if DEBUG
			if (_notDisposed == 0)
			{
				throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
			}

			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (destinationOffset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(destinationOffset), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (destinationOffset + size > Size)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, destinationOffset, size), nameof(destinationOffset));
			}
#endif
			if (size == 0)
			{
				return;
			}

			byte* destPtr = DataPointer + destinationOffset;

			DirectAccess.MemoryCopy(destPtr, source, size);
		}

		/// <summary>
		/// Function to fill the unmanaged memory pointed at by this <see cref="IGorgonPointer"/> with the specified byte value.
		/// </summary>
		/// <param name="value">The value to fill the unmanaged memory with.</param>
		/// <param name="size">The number of bytes to fill.</param>
		/// <param name="offset">[Optional] The offset, in bytes, within unmanaged memory to start filling.</param>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="offset"/> is less than zero.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> + the <paramref name="size"/> exceeds the <see cref="IGorgonPointer.Size"/> of the unmanaged memory represented by this <see cref="IGorgonPointer"/></exception>
		public void Fill(byte value, int size, long offset = 0)
		{
#if DEBUG
			if (_notDisposed == 0) 
			{
				throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (offset + size > Size)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, offset, size), nameof(offset));
			}
#endif

			unsafe
			{
				if (size <= 0)
				{
					return;
				}

				byte* ptr = DataPointer + offset;
				DirectAccess.FillMemory(ptr, value, size);
			}
		}

		/// <summary>
		/// Function to fill all the unmanaged memory pointed at by this <see cref="IGorgonPointer"/> with the specified byte value.
		/// </summary>
		/// <param name="value">The value to fill the unmanaged memory with.</param>
		public void Fill(byte value)
		{
			unsafe
			{
#if DEBUG
				if (_notDisposed == 0)
				{
					throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
				}
#endif

				// If the buffer is larger than what we can accommodate, then we'll fill it in chunks.
				if (Size > int.MaxValue)
				{
					byte* ptr = DataPointer;
					long size = Size;
					int fillAmount = int.MaxValue;

					while (size > 0)
					{
						DirectAccess.FillMemory(ptr, value, fillAmount);

						size -= fillAmount;
						ptr += fillAmount;

						if (size < int.MaxValue)
						{
							fillAmount = (int)(int.MaxValue - size);
						}
					}

					return;
				}

				DirectAccess.FillMemory(DataPointer, value, (int)Size);
			}
		}

		/// <summary>
		/// Function to fill the unmanaged memory pointed at by this <see cref="IGorgonPointer"/> with a zero value.
		/// </summary>
		/// <param name="size">The number of bytes to fill.</param>
		/// <param name="offset">[Optional] The offset, in bytes, within unmanaged memory to start filling with zeroes.</param>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="offset"/> is less than zero.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> + the <paramref name="size"/> exceeds the <see cref="IGorgonPointer.Size"/> of the unmanaged memory represented by this <see cref="IGorgonPointer"/></exception>
		public void Zero(int size, long offset = 0)
		{
#if DEBUG
			if (_notDisposed == 0)
			{
				throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset), Resources.GOR_ERR_DATABUFF_OFFSET_TOO_SMALL);
			}

			if (offset + size > Size)
			{
				throw new ArgumentException(string.Format(Resources.GOR_ERR_DATABUFF_SIZE_OFFSET_TOO_LARGE, offset, size), nameof(offset));
			}
#endif

			unsafe
			{
				if (size <= 0)
				{
					return;
				}

				byte* ptr = DataPointer + offset;
				DirectAccess.ZeroMemory(ptr, size);
			}
		}

		/// <summary>
		/// Function to fill all the unmanaged memory pointed at by this <see cref="IGorgonPointer"/> with a zero value.
		/// </summary>
		public void Zero()
		{
			unsafe
			{
#if DEBUG
				if (_notDisposed == 0)
				{
					throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
				}
#endif

				// If the buffer is larger than what we can accommodate, then we'll fill it in chunks.
				if (Size > int.MaxValue)
				{
					byte* ptr = DataPointer;
					long size = Size;
					int fillAmount = int.MaxValue;

					while (size > 0)
					{
						DirectAccess.ZeroMemory(ptr, fillAmount);

						size -= fillAmount;
						ptr += fillAmount;

						if (size < int.MaxValue)
						{
							fillAmount = (int)(int.MaxValue - size);
						}
					}

					return;
				}

				DirectAccess.ZeroMemory(DataPointer, (int)Size);
			}
		}

		/// <summary>
		/// Function to copy the unmanaged memory pointed at by this <see cref="IGorgonPointer"/> into a <see cref="MemoryStream"/>
		/// </summary>
		/// <returns>The <see cref="MemoryStream"/> containing a copy of the data in memory that is pointed at by this <see cref="IGorgonPointer"/>.</returns>
		public MemoryStream CopyToMemoryStream()
		{
			if (_notDisposed == 0)
			{
				throw new ObjectDisposedException(Resources.GOR_ERR_DATABUFF_PTR_DISPOSED);
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

		#region IDisposable Members
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (SetDisposed())
			{
				return;
			}

			Cleanup();
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
