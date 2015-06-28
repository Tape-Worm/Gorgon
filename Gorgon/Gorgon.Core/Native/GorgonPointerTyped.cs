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

using System.Runtime.InteropServices;

namespace Gorgon.Native
{
	/// <summary>
	/// Provides safe pointer access to a newly allocated block of unmanaged memory.
	/// </summary>
	/// <typeparam name="T">The type of data used to determine how much memory to allocate. Must be a value or primitive type.</typeparam>
	/// <remarks>
	/// <para>
	/// This wraps a pointer to unmanaged native memory that has been allocated by this object. It provides better thread safety than the <see cref="Gorgon.IO.GorgonDataStream"/> object and gives safe access when  
	/// dereferencing the pointer to read or write data.
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
	/// The constraints for the type parameter, <typeparamref name="T"/>, apply as well when reading or writing generic value types, or arrays with value types as elements.
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
	/// <para>
	/// This also applies to the type, <typeparamref name="T"/>, passed to the object upon creation.
	/// </para>
	/// </note>
	/// </para>
	/// </remarks>
	/// <seealso cref="Gorgon.IO.GorgonDataStream"/>
	/// <seealso cref="GorgonPointer"/>
	/// <seealso cref="GorgonPointerPinned{T}"/>
	/// <seealso cref="GorgonPointerAlias"/>
	/// <seealso cref="GorgonPointerAliasTyped{T}"/>
	public sealed class GorgonPointerTyped<T>
		: GorgonPointer
		where T : struct
	{
		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPointerTyped{T}"/> class.
		/// </summary>
		/// <param name="alignment">[Optional] The alignment of the memory, in bytes.</param>
		/// <remarks>
		/// <para>
		/// This creates a <see cref="GorgonPointer"/> that points to a new block of aligned unmanaged memory.
		/// </para>
		/// <para>
		/// The <paramref name="alignment" /> parameter is meant to offset the block of memory so that it is set up for optimal access for the CPU. This value should be a power of two.
		/// </para>
		/// <para>
		/// The type indicated by <typeparamref name="T" /> is used to determine the amount of memory to allocate to store a single item of that type. This type is subject to the following constraints:
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
		///   <note type="important">
		///     <para>
		///		Creating a pointer with this method allocates unmanaged memory. The .NET garbage collector is unable to track this memory, and will not free it until the pointer object is ready for finalization.
		///		This can lead to memory leaks if handled improperly. The best practice is to allocate the memory using this method, and then call <see cref="GorgonPointerBase.Dispose" /> on the pointer object when done 
		///		with it.
		///		</para>
		///   </note>
		/// </para>
		/// <para>
		/// This code was derived from the <c>Utilities.AllocateMemory</c> function from <a href="https://github.com/sharpdx/SharpDX/blob/master/Source/SharpDX/Utilities.cs">SharpDX</a> by Alexandre Mutel.
		/// </para>
		/// </remarks>
		public GorgonPointerTyped(int alignment = 16)
			: base(DirectAccess.SizeOf<T>(), alignment)
		{
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="GorgonPointer"/> class.
		/// </summary>
		~GorgonPointerTyped()
		{
			Dispose(false);
		}
		#endregion
	}
}
