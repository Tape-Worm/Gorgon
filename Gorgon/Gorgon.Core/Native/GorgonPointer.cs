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
// Created: Sunday, June 28, 2015 11:26:02 AM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using Gorgon.Core.Properties;
using Gorgon.Diagnostics;
using Gorgon.Math;

namespace Gorgon.Native
{
	/// <summary>
	/// Provides safe pointer access to a newly allocated block of unmanaged memory.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This wraps a pointer to unmanaged native memory that has been allocated by this object. It provides better thread safety than the <see cref="Gorgon.IO.GorgonDataStream"/> object and gives safe access when  
	/// dereferencing the pointer to read or write data.
	/// </para>
	/// <para>
	/// When this object allocates memory, it will add pressure to the .NET garbage collector to ensure that it is aware that this object is consuming a much larger portion of memory than can be deduced by the 
	/// garbage collector. This allows the GC to pick up the object for finalization when memory pressure is too high. The memory pressure is reset when this object is disposed or finalized.
	/// </para>
	/// <para>
	/// Like the <see cref="Gorgon.IO.GorgonDataStream"/> object, it provides several methods to <see cref="O:Gorgon.Native.IGorgonPointer.Read">read</see>/<see cref="O:Gorgon.Native.IGorgonPointer.Write">write</see> 
	/// data to the memory pointed at by the internal pointer. With these methods the object can read and write primitive types, arrays, and generic value types.
	/// </para>
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
	/// <para>
	/// <note type="important">
	/// <para>
	/// This object allocates unmanaged memory. Since this memory is outside of the scope of the .NET garbage collector, it will be kept around until this object is finalized and could lead to memory leakage. Always 
	/// call the <see cref="GorgonPointerBase.Dispose"/> method when an instance of this object is no longer needed.
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
	/// <seealso cref="GorgonPointerTyped{T}"/>
	/// <seealso cref="GorgonPointerPinned{T}"/>
	/// <seealso cref="GorgonPointerAlias"/>
	/// <seealso cref="GorgonPointerAliasTyped{T}"/>
	public class GorgonPointer
		: GorgonPointerBase
	{
		#region Methods.
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected unsafe override void Dispose(bool disposing)
		{
			if (SetDisposed())
			{
				return;
			}
			
			Marshal.FreeHGlobal(((IntPtr*)DataPointer)[-1]);
			GC.RemoveMemoryPressure(Size);

			DataPointer = null;
			Size = 0;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPointer"/> class.
		/// </summary>
		/// <param name="size">Size of the memory to allocate, in bytes.</param>
		/// <param name="alignment">[Optional] The alignment of the memory, in bytes.</param>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="size" /> is larger than <see cref="int.MaxValue" /> when the application is running as x86.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="size" /> parameter is less than, or equal to zero.</exception>
		/// <remarks>
		/// <para>
		/// This creates a <see cref="GorgonPointer"/> that points to a new block of aligned unmanaged memory.
		/// </para>
		/// <para>
		/// The <paramref name="alignment" /> parameter is meant to offset the block of memory so that it is set up for optimal access for the CPU. This value should be a power of two.
		/// </para>
		/// <para>
		///   <note type="important">
		///     <para>
		///		Creating a pointer with this method allocates unmanaged memory. The .NET garbage collector is unable to track this memory, and will not free it until the pointer object is ready for finalization.
		///		This can lead to memory leaks if handled improperly. The best practice is to allocate the memory using this method, and then call <see cref="GorgonPointerBase.Dispose" /> on the pointer object when 
		///		done with it.
		///		</para>
		///   </note>
		/// </para>
		/// <para>
		/// This code was derived from the <c>Utilities.AllocateMemory</c> function from <a href="https://github.com/sharpdx/SharpDX/blob/master/Source/SharpDX/Utilities.cs">SharpDX</a> by Alexandre Mutel.
		/// </para>
		/// </remarks>
		public GorgonPointer(long size, int alignment = 16)
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

				Size = size;
				DataPointer = (byte*)alignedAddr;

				GC.AddMemoryPressure(size);
			}
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="GorgonPointer"/> class.
		/// </summary>
		~GorgonPointer()
		{
			Dispose(false);
		}
		#endregion
	}
}
