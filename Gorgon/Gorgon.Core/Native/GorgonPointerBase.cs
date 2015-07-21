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
		// The pointer to the memory.
		private unsafe byte* _pointer;
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
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return _pointer;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				if (_notDisposed == 0)
				{
					return;
				}

				_pointer = value;
			}
		}

		/// <summary>
		/// Property to return whether the pointer has been disposed or not.
		/// </summary>
		public unsafe bool IsDisposed
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return _notDisposed == 0 || DataPointer != null;
			}
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		/// <remarks>
		/// <inheritdoc/>
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
				byte* ptr = _pointer + offset;
				DirectAccess.ReadValue(ptr, out value);
			}
		}

		/// <inheritdoc/>
		/// <remarks>
		/// <inheritdoc/>
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

		/// <inheritdoc/>
		/// <remarks>
		/// <inheritdoc/>
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
				DirectAccess.ReadValue(_pointer, out value);
			}
		}

		/// <inheritdoc/>
		/// <remarks>
		/// <inheritdoc/>
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

				DirectAccess.ReadValue(_pointer, out value);

				return value;
			}
		}

		/// <inheritdoc/>
		/// <remarks>
		/// <inheritdoc/>
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
				byte* ptr = _pointer + offset;
				DirectAccess.WriteValue(ptr, ref value);
			}
		}

		/// <inheritdoc/>
		/// <remarks>
		/// <inheritdoc/>
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

		/// <inheritdoc/>
		/// <remarks>
		/// <inheritdoc/>
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
				DirectAccess.WriteValue(_pointer, ref value);
			}
		}

		/// <inheritdoc/>
		/// <remarks>
		/// <inheritdoc/>
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

		/// <inheritdoc/>
		/// <remarks>
		/// <inheritdoc/>
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

				byte* ptr = _pointer + offset;

				DirectAccess.ReadArray(ptr, array, index, arrayByteSize);
			}
		}

		/// <inheritdoc/>
		/// <remarks>
		/// <inheritdoc/>
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

				DirectAccess.ReadArray(_pointer, array, index, arrayByteSize);
			}
		}

		/// <inheritdoc/>
		/// <remarks>
		/// <inheritdoc/>
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

		/// <inheritdoc/>
		/// <remarks>
		/// <inheritdoc/>
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

		/// <inheritdoc/>
		/// <remarks>
		/// <inheritdoc/>
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

				byte* ptr = _pointer + offset;
				DirectAccess.WriteArray(ptr, array, index, arrayByteSize);
			}
		}

		/// <inheritdoc/>
		/// <remarks>
		/// <inheritdoc/>
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

		/// <inheritdoc/>
		/// <remarks>
		/// <inheritdoc/>
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

		/// <inheritdoc/>
		/// <remarks>
		/// <inheritdoc/>
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

				DirectAccess.WriteArray(_pointer, array, index, arrayByteSize);
			}
		}

		/// <inheritdoc/>
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
				byte* destPtr = _pointer + destinationOffset;

				DirectAccess.MemoryCopy(destPtr, srcPtr, sourceSize);
			}
		}

		/// <inheritdoc/>
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

				DirectAccess.MemoryCopy(_pointer, (byte *)source.Address, sourceSize);
			}
		}

		/// <inheritdoc/>
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
				byte* destPtr = _pointer + destinationOffset;

				DirectAccess.MemoryCopy(destPtr, srcPtr, size);
			}
		}

		/// <inheritdoc/>
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

			byte* destPtr = _pointer + destinationOffset;

			DirectAccess.MemoryCopy(destPtr, source, size);
		}

		/// <inheritdoc/>
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

				byte* ptr = _pointer + offset;
				DirectAccess.FillMemory(ptr, value, size);
			}
		}

		/// <inheritdoc/>
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
				if (Size > Int32.MaxValue)
				{
					byte* ptr = _pointer;
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

				DirectAccess.FillMemory(_pointer, value, (int)Size);
			}
		}

		/// <inheritdoc/>
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

				byte* ptr = _pointer + offset;
				DirectAccess.ZeroMemory(ptr, size);
			}
		}

		/// <inheritdoc/>
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
				if (Size > Int32.MaxValue)
				{
					byte* ptr = _pointer;
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

				DirectAccess.ZeroMemory(_pointer, (int)Size);
			}
		}

		/// <inheritdoc/>
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
