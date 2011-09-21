#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Sunday, September 11, 2011 1:10:19 PM
// 
#endregion

//#define MEMCPY		// Use only memcpy.
//#define CPBLK			// Use only copy block IL.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;

namespace GorgonLibrary.Data
{
	/// <summary>
	/// Extensions for manipulating memory via an Intptr.
	/// </summary>
	public static class GorgonIntPtrExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dest"></param>
		/// <param name="src"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		[DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false), SuppressUnmanagedCodeSecurity]
		public static unsafe extern void* CopyMemory(void* dest, void* src, long count);		
		
		/// <summary>
		/// Function to copy the memory contents from this pointer to another.
		/// </summary>
		/// <param name="destination">Destination buffer.</param>
		/// <param name="source">Source buffer.</param>
		/// <param name="size">Size of the data to copy, in bytes.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the destination pointer is NULL (Nothing in VB.Net).</exception>
		public unsafe static void CopyTo(this IntPtr source, IntPtr destination, int size)
		{
#if !MEMCPY && !CPBLK
			if (Gorgon.PlatformArchitecture == PlatformArchitecture.x64)
				DirectAccess.Write(destination, source, size);
			else
				DirectAccess.Writex86(destination, source, size);
#else
#if MEMCPY
			DirectAccess.Writex86(destination, source, size);
#else
			DirectAccess.Write(destination, source, size);
#endif
#endif
		}

		/// <summary>
		/// Function to copy the contents of the pointer to a byte array.
		/// </summary>
		/// <param name="source">Source pointer.</param>
		/// <param name="destination">Destination array of bytes.</param>
		/// <param name="destinationIndex">Index in the array to start writing at.</param>
		/// <param name="size">Size of the data to copy.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="destination"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + destinationIndex is greater than the number of elements in the destination parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the destinationIndex is less than 0.</para>
		/// </exception>
		public static void CopyTo(this IntPtr source, byte[] destination, int destinationIndex, int size)
		{
			if (source == IntPtr.Zero)
				return;

			if (destination == null)
				throw new ArgumentNullException("destination");

			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException("destinationIndex", "Index cannot be less than zero.");

			if (destinationIndex + size > destination.Length)
				throw new ArgumentOutOfRangeException("destinationIndex", "Index and size cannot be larger than the array.");

			Marshal.Copy(source, destination, destinationIndex, size);
		}

		/// <summary>
		/// Function to copy the contents of the pointer to a byte array.
		/// </summary>
		/// <param name="source">Source pointer.</param>
		/// <param name="destination">Destination array of bytes.</param>
		/// <param name="size">Size of the data to copy.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="destination"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + destinationIndex is greater than the number of elements in the destination parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the destinationIndex is less than 0.</para>
		/// </exception>
		public static void CopyTo(this IntPtr source, byte[] destination, int size)
		{
			CopyTo(source, destination, 0, size);
		}

		/// <summary>
		/// Function to copy the contents of the pointer to a byte array.
		/// </summary>
		/// <param name="source">Source pointer.</param>
		/// <param name="destination">Destination array of bytes.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="destination"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + destinationIndex is greater than the number of elements in the destination parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the destinationIndex is less than 0.</para>
		/// </exception>
		public static void CopyTo(this IntPtr source, byte[] destination)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			CopyTo(source, destination, 0, destination.Length);
		}

		/// <summary>
		/// Function to copy the contents of the pointer to a byte array.
		/// </summary>
		/// <typeparam name="T">Type of data in the array.</typeparam>
		/// <param name="source">Source pointer.</param>
		/// <param name="destination">Destination array of bytes.</param>
		/// <param name="destinationIndex">Index in the array to start writing at.</param>
		/// <param name="size">Size of the data to copy in bytes.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="destination"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + destinationIndex is greater than the number of elements in the destination parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the destinationIndex is less than 0.</para>
		/// </exception>
		public static void CopyTo<T>(this IntPtr source, T[] destination, int destinationIndex, int size)
			where T : struct
		{
			if (source == IntPtr.Zero)
				return;

			if (destination == null)
				throw new ArgumentNullException("destination");

			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException("destinationIndex", "Index cannot be less than zero.");

			if (destinationIndex + size > destination.Length * DirectAccess.SizeOf<T>())
				throw new ArgumentOutOfRangeException("destinationIndex", "Index and size cannot be larger than the array.");

#if !MEMCPY && !CPBLK
			if (Gorgon.PlatformArchitecture == PlatformArchitecture.x64)
				DirectAccess.Read<T>(source, destination, destinationIndex, size);
			else
				DirectAccess.Readx86<T>(source, destination, destinationIndex, size);
#else
#if MEMCPY
			DirectAccess.Readx86<T>(source, destination, destinationIndex, size);
#else
			DirectAccess.Read<T>(source, destination, destinationIndex, size);
#endif
#endif
		}

		/// <summary>
		/// Function to copy the contents of the pointer to a byte array.
		/// </summary>
		/// <typeparam name="T">Type of data in the array.</typeparam>
		/// <param name="source">Source pointer.</param>
		/// <param name="destination">Destination array.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="destination"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + destinationIndex is greater than the number of elements in the destination parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the destinationIndex is less than 0.</para>
		/// </exception>
		public static void CopyTo<T>(this IntPtr source, T[] destination)
			where T : struct
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			CopyTo<T>(source, destination, 0, destination.Length * DirectAccess.SizeOf<T>());
		}

		/// <summary>
		/// Function to copy the contents of the pointer to a byte array.
		/// </summary>
		/// <typeparam name="T">Type of data in the array.</typeparam>
		/// <param name="source">Source pointer.</param>
		/// <param name="destination">Destination array of bytes.</param>
		/// <param name="size">Size of the data to copy in bytes.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="destination"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + destinationIndex is greater than the number of elements in the destination parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the destinationIndex is less than 0.</para>
		/// </exception>
		public static void CopyTo<T>(this IntPtr source, T[] destination, int size)
			where T : struct
		{
			CopyTo<T>(source, destination, 0, size);
		}

		/// <summary>
		/// Function to copy the contents of a byte array into the memory pointed at by the pointer.
		/// </summary>
		/// <param name="destination">Destination pointer.</param>
		/// <param name="source">Source pointer to copy from.</param>
		/// <param name="size">Number of bytes to copy.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the source pointer is NULL (Nothing in VB.Net).</exception>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		public static void CopyFrom(this IntPtr destination, IntPtr source, int size)
		{
#if !MEMCPY && !CPBLK
			if (Gorgon.PlatformArchitecture == PlatformArchitecture.x64)
				DirectAccess.Write(destination, source, size);
			else
				DirectAccess.Writex86(destination, source, size);
#else
#if MEMCPY
			DirectAccess.Writex86(destination, source, size);
#else
			DirectAccess.Write(destination, source, size);
#endif
#endif
		}

		/// <summary>
		/// Function to copy the contents of a byte array into the memory pointed at by the pointer.
		/// </summary>
		/// <param name="destination">Destination pointer.</param>
		/// <param name="source">Source array to copy from.</param>
		/// <param name="sourceIndex">Index to start copying from in the source array.</param>
		/// <param name="size">Number of bytes to copy.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="source"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + sourceIndex is greater than the number of elements in the source parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the sourceIndex is less than 0.</para>
		/// </exception>
		public static void CopyFrom(this IntPtr destination, byte[] source, int sourceIndex, int size)
		{
			if (destination == IntPtr.Zero)
				return;

			if (source == null)
				throw new ArgumentNullException("destination");

			if (sourceIndex < 0)
				throw new ArgumentOutOfRangeException("sourceIndex", "Index cannot be less than zero.");

			if (sourceIndex + size > source.Length)
				throw new ArgumentOutOfRangeException("sourceIndex", "Index and size cannot be larger than the array.");

			Marshal.Copy(source, sourceIndex, destination, size);
		}

		/// <summary>
		/// Function to copy the contents of a byte array into the memory pointed at by the pointer.
		/// </summary>
		/// <param name="destination">Destination pointer.</param>
		/// <param name="source">Source array to copy from.</param>
		/// <param name="size">Number of bytes to copy.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="source"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + sourceIndex is greater than the number of elements in the source parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the sourceIndex is less than 0.</para>
		/// </exception>
		public static void CopyFrom(this IntPtr destination, byte[] source, int size)
		{
			CopyFrom(destination, source, 0, size);
		}

		/// <summary>
		/// Function to copy the contents of a byte array into the memory pointed at by the pointer.
		/// </summary>
		/// <param name="destination">Destination pointer.</param>
		/// <param name="source">Source array to copy from.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="source"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + sourceIndex is greater than the number of elements in the source parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the sourceIndex is less than 0.</para>
		/// </exception>
		public static void CopyFrom(this IntPtr destination, byte[] source)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			
			CopyFrom(destination, source, 0, source.Length);
		}

		/// <summary>
		/// Function to copy the contents of a byte array into the memory pointed at by the pointer.
		/// </summary>
		/// <typeparam name="T">Type of data in the array.</typeparam>
		/// <param name="destination">Destination pointer.</param>
		/// <param name="source">Source array to copy from.</param>
		/// <param name="sourceIndex">Index to start copying from in the source array.</param>
		/// <param name="size">Number of bytes to copy.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="source"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + sourceIndex is greater than the number of elements in the source parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the sourceIndex is less than 0.</para>
		/// </exception>
		public static void CopyFrom<T>(this IntPtr destination, T[] source, int sourceIndex, int size)
			where T : struct
		{
			int typeSize = DirectAccess.SizeOf<T>();

			if (destination == IntPtr.Zero)
				return;

			if (source == null)
				throw new ArgumentNullException("destination");

			if (sourceIndex < 0)
				throw new ArgumentOutOfRangeException("sourceIndex", "Index cannot be less than zero.");

			if (sourceIndex + size > source.Length * typeSize)
				throw new ArgumentOutOfRangeException("sourceIndex", "Index and size cannot be larger than the array.");

#if !MEMCPY && !CPBLK
			if (Gorgon.PlatformArchitecture == PlatformArchitecture.x64)
				DirectAccess.Write<T>(destination, source, sourceIndex, size);
			else
				DirectAccess.Writex86<T>(destination, source, sourceIndex, size);
#else
#if MEMCPY
			DirectAccess.Writex86<T>(destination, source, sourceIndex, size);
#else
			DirectAccess.Write<T>(destination, source, sourceIndex, size);
#endif
#endif
		}

		/// <summary>
		/// Function to copy the contents of a byte array into the memory pointed at by the pointer.
		/// </summary>
		/// <typeparam name="T">Type of data in the array.</typeparam>
		/// <param name="destination">Destination pointer.</param>
		/// <param name="source">Source array to copy from.</param>
		/// <param name="size">Number of bytes to copy.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="source"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + sourceIndex is greater than the number of elements in the source parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the sourceIndex is less than 0.</para>
		/// </exception>
		public static void CopyFrom<T>(this IntPtr destination, T[] source, int size)
			where T : struct
		{
			CopyFrom<T>(destination, source, 0, size);
		}

		/// <summary>
		/// Function to copy the contents of a byte array into the memory pointed at by the pointer.
		/// </summary>
		/// <typeparam name="T">Type of data in the array.</typeparam>
		/// <param name="destination">Destination pointer.</param>
		/// <param name="source">Source array to copy from.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="source"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + sourceIndex is greater than the number of elements in the source parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the sourceIndex is less than 0.</para>
		/// </exception>
		public static void CopyFrom<T>(this IntPtr destination, T[] source)
			where T : struct
		{
			if (source == null)
				throw new ArgumentNullException("source");

			CopyFrom(destination, source, 0, source.Length * DirectAccess.SizeOf<T>());
		}

		/// <summary>
		/// Function to zero out the memory pointed to by this pointer.
		/// </summary>
		/// <param name="destination">Destination pointer to zero out.</param>
		/// <param name="size">Amount of memory to zero out.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		public static void ZeroMemory(this IntPtr destination, int size)
		{
			DirectAccess.ZeroMemory(destination, size);
		}

		/// <summary>
		/// Function to marshal an object or value type into unmanaged memory.
		/// </summary>
		/// <param name="destination">Pointer to marhsal the data into.</param>
		/// <param name="value">Object or value type to marshal.</param>
		/// <param name="deleteContents">TRUE to remove any pre-allocated, FALSE to leave alone.</param>
		/// <remarks>This method will marshal a structure (object or value type) into unmanaged memory.
		/// <para>Passing FALSE to <paramref name="deleteContents"/> may result in a memory leak if the data was previously initialized.</para>
		/// <para>For more information, see the <see cref="M:System.RunTime.InteropServices.Marshal.StructureToPtr">Marshal.StructureToPtr</see> method.</para>
		/// </remarks>
		public static void MarshalFrom(this IntPtr destination, object value, bool deleteContents)
		{
			Marshal.StructureToPtr(value, destination, deleteContents);
		}

		/// <summary>
		/// Function to marshal unmanaged data back to an object or value type.
		/// </summary>
		/// <typeparam name="T">Type of value type or object.</typeparam>
		/// <param name="source">Pointer to read from.</param>
		/// <returns>The data converted into a new value type or object.</returns>
		/// <remarks>This method will marshal unmanaged data back into a new structure (object or value type).
		/// <para>For more information, see the <see cref="M:System.RunTime.InteropServices.Marshal.PtrToStructure">Marshal.PtrToStructure</see> method.</para>
		/// </remarks>
		public static T MarshalTo<T>(this IntPtr source)
		{
			return (T)Marshal.PtrToStructure(source, typeof(T));
		}

		/// <summary>
		/// Function to marshal unmanaged data back to an existing object or value type.
		/// </summary>
		/// <typeparam name="T">Type of value type or object.</typeparam>
		/// <param name="source">Pointer to read from.</param>
		/// <param name="value">Value to copy the data into.</param>
		/// <returns>The data converted and copied into a value type or object.</returns>
		/// <remarks>This method will marshal unmanaged data back into an existing structure (object or value type).
		/// <para>The user must pre-allocate the object before calling this method.</para>
		/// <para>For more information, see the <see cref="M:System.RunTime.InteropServices.Marshal.PtrToStructure">Marshal.PtrToStructure</see> method.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="value"/> parameter is NULL (Nothing in VB.Net).</exception>
		public static void MarshalTo<T>(this IntPtr source, T value)
			where T : class
		{
			Marshal.PtrToStructure(source, value);
		}

		/// <summary>
		/// Function to write a specific value type to the memory pointed at by the pointer.
		/// </summary>
		/// <typeparam name="T">Type of value to write.</typeparam>
		/// <param name="destination">The destination pointer.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="size">Size of the data, in bytes.</param>
		/// <remarks>This method can only write value types composed of primitives, reference objects will not work.
		/// <para>There is no way to determine the size of the data pointed at by the pointer, so the user must take care not to write outside the bounds of the memory.</para>
		/// </remarks>
		public static void Write<T>(this IntPtr destination, T value, int size)
			where T : struct
		{
			DirectAccess.Write<T>(destination, value, size);
		}

		/// <summary>
		/// Function to read a specific value from the memory pointed at by the pointer.
		/// </summary>
		/// <typeparam name="T">Type of value to read.</typeparam>
		/// <param name="source">The source pointer.</param>
		/// <param name="size">Size of the data, in bytes.</param>
		/// <returns>The value at the pointer.</returns>
		/// <remarks>This method can only write value types composed of primitives, reference objects will not work.
		/// <para>There is no way to determine the size of the data pointed at by the pointer, so the user must take care not to write outside the bounds of the memory.</para>
		/// </remarks>
		public static T Read<T>(this IntPtr source, int size)
			where T : struct
		{
			return DirectAccess.Read<T>(source, size);
		}
	}
}
