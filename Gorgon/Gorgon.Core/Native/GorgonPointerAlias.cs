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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Gorgon.Core.Properties;

namespace Gorgon.Native
{
	/// <inheritdoc/>
	public class GorgonPointerAlias
		: GorgonPointerBase
	{
		#region Methods.
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected unsafe override void Dispose(bool disposing)
		{
			DataPointer = null;
			Size = 0;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Nothing to dispose. Doesn't own pointer"), EditorBrowsable(EditorBrowsableState.Never)]
		public new void Dispose()
		{
			GC.SuppressFinalize(this);
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPointerBase" /> class.
		/// </summary>
		/// <param name="pointer">The pointer to unmanaged native memory.</param>
		/// <param name="size">Size of the data allocated to the unmanaged memory, in bytes.</param>
		/// <exception cref="System.ArgumentNullException">pointer</exception>
		/// <exception cref="System.ArgumentException">size</exception>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer" /> is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="size" /> parameter is less than one.</exception>
		/// <remarks>
		/// <para>
		/// This takes a <c>void *</c> pointer to unmanaged memory, and aliases it to this object. This allows for safe use of the pointer when reading and writing the memory being pointed at.
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointerBase" /> aliases an existing pointer, it merely provides access to the pointer. It is not responsible for the pointer, and thus will not free or allocate memory on its
		/// behalf. The responsibility of freeing memory assigned to a pointer is placed on the caller of this object.
		/// </para>
		/// <para>
		/// The memory pointed at by the pointer should be at least 1 byte in length and this should be reflected in the <paramref name="size" /> parameter, otherwise an exception will be thrown.
		/// </para>
		/// <para>
		///   <note type="warning">
		///     <para>
		/// Ensure that the <paramref name="size" /> parameter is correct when using this method. The object has no way to determine how much memory is allocated and therefore, if the size is too large, a
		/// buffer overrun may occur when reading or writing.
		/// </para>
		///   </note>
		/// </para></remarks>
		public unsafe GorgonPointerAlias(void* pointer, int size)
		{
			if (pointer == null)
			{
				throw new ArgumentNullException("pointer");
			}

			if (size < 1)
			{
				throw new ArgumentException(Resources.GOR_ERR_DATABUFF_SIZE_TOO_SMALL, "size");
			}

			DataPointer = (byte*)pointer;
			Size = size;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPointerAlias"/> class.
		/// </summary>
		/// <param name="pointer">The <see cref="IntPtr" /> representing a pointer to unmanaged native memory.</param>
		/// <param name="size">Size of the data allocated to the unmanaged memory, in bytes.</param>
		/// <exception cref="System.ArgumentNullException">pointer</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">size</exception>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pointer" /> is equal to <see cref="IntPtr.Zero" />.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="size" /> parameter is less than one.</exception>
		/// <remarks>
		/// <para>
		/// This takes a <see cref="IntPtr" /> representing a pointer to unmanaged memory, and aliases it to this object. This allows for safe use of the pointer when reading and writing the memory being
		/// pointed at.
		/// </para>
		/// <para>
		/// When the <see cref="GorgonPointerBase" /> aliases an existing pointer, it merely provides access to the pointer. It is not responsible for the pointer, and thus will not free or allocate memory on its
		/// behalf. The responsibility of freeing memory assigned to a pointer is placed on the caller of this object.
		/// </para>
		/// <para>
		/// The memory pointed at by the <see cref="IntPtr" /> should be at least 1 byte in length and this should be reflected in the <paramref name="size" /> parameter, otherwise an exception will be thrown.
		/// </para>
		/// <para>
		///   <note type="warning">
		///     <para>
		/// Ensure that the <paramref name="size" /> parameter is correct when using this method. The object has no way to determine how much memory is allocated and therefore, if the size is too large, a
		/// buffer overrun may occur when reading or writing.
		/// </para>
		///   </note>
		/// </para></remarks>
		public GorgonPointerAlias(IntPtr pointer, int size)
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
				DataPointer = (byte*)pointer.ToPointer();
				Size = size;
			}
		}
		#endregion
	}
}
