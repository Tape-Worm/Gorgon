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

using System;
using System.Runtime.InteropServices;
using Gorgon.Core.Properties;

namespace Gorgon.Native
{
	/// <summary>
	/// Extensions for manipulating memory via an <see cref="IntPtr"/>.
	/// </summary>
	/// <remarks>Great care must be exercised when using these methods.  They allow access to native memory, so it's entirely possible 
	/// to corrupt memory very easily with these methods.  Be sure that you are familiar with how pointers work and how unmanaged memory
	/// works before using these methods.</remarks>
	public static class GorgonIntPtrExtensions
	{
		/// <summary>
		/// Function to copy the memory contents from this pointer to another.
		/// </summary>
		/// <param name="destination">Destination buffer.</param>
		/// <param name="source">Source buffer.</param>
		/// <param name="size">Size of the data to copy, in bytes.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the destination pointer is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public static void CopyTo(this IntPtr source, IntPtr destination, int size)
		{
			DirectAccess.MemoryCopy(destination, source, size);
		}

		/// <summary>
		/// Function to copy the contents of the pointer to a byte array.
		/// </summary>
		/// <param name="source">Source pointer.</param>
		/// <param name="destination">Destination array of bytes.</param>
		/// <param name="destinationIndex">Index in the array to start writing at.</param>
		/// <param name="size">Size of the data to copy.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="destination"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + destinationIndex is greater than the number of elements in the destination parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the destinationIndex is less than 0.</para>
		/// </exception>
		public static void CopyTo(this IntPtr source, byte[] destination, int destinationIndex, int size)
		{
		    if (source == IntPtr.Zero)
		    {
		        return;
		    }

#if DEBUG
		    if (destination == null)
		    {
		        throw new ArgumentNullException("destination");
		    }

		    if (destinationIndex < 0)
		    {
		        throw new ArgumentOutOfRangeException("destinationIndex", 
                    string.Format(Resources.GOR_INDEX_OUT_OF_RANGE, destinationIndex, destination.Length));
		    }

		    if (destinationIndex + size > destination.Length)
		    {
                throw new ArgumentOutOfRangeException("destinationIndex",
                    string.Format(Resources.GOR_INDEX_OUT_OF_RANGE, destinationIndex + size, destination.Length));
            }
#endif
			DirectAccess.ReadArray(source, destination, destinationIndex, size);
		}

		/// <summary>
		/// Function to copy the contents of the pointer to a byte array.
		/// </summary>
		/// <param name="source">Source pointer.</param>
		/// <param name="destination">Destination array of bytes.</param>
		/// <param name="size">Size of the data to copy.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="destination"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
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
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="destination"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + destinationIndex is greater than the number of elements in the destination parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the destinationIndex is less than 0.</para>
		/// </exception>
		public static void CopyTo(this IntPtr source, byte[] destination)
		{
#if DEBUG
			if (destination == null)
				throw new ArgumentNullException("destination");
#endif

			CopyTo(source, destination, 0, destination.Length);
		}

		/// <summary>
		/// Function to copy the contents of the pointer to a byte array.
		/// </summary>
		/// <typeparam name="T">Type of data in the array.</typeparam>
		/// <param name="source">Source pointer.</param>
		/// <param name="destination">Destination array of bytes.</param>
		/// <param name="destinationIndex">Index in the array to start writing at.</param>
		/// <param name="size">Size of the data to copy, in bytes.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="destination"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + destinationIndex is greater than the number of elements in the destination parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the destinationIndex is less than 0.</para>
		/// </exception>
		public static void CopyTo<T>(this IntPtr source, T[] destination, int destinationIndex, int size)
			where T : struct
		{
#if DEBUG
		    int sizeInBytes = DirectAccess.SizeOf<T>();

		    if (source == IntPtr.Zero)
		    {
		        throw new ArgumentNullException("source");
		    }

		    if (destination == null)
		    {
		        throw new ArgumentNullException("destination");
		    }

		    if (destinationIndex < 0)
            {
                throw new ArgumentOutOfRangeException("destinationIndex",
                    string.Format(Resources.GOR_INDEX_OUT_OF_RANGE, destinationIndex, destination.Length * sizeInBytes));
            }

            if ((destinationIndex * sizeInBytes) + size > destination.Length * DirectAccess.SizeOf<T>())
            {
                throw new ArgumentOutOfRangeException("destinationIndex",
                    string.Format(Resources.GOR_INDEX_OUT_OF_RANGE, destinationIndex + size, destination.Length * sizeInBytes));
            }
#endif

			DirectAccess.ReadArray(source, destination, destinationIndex, size);
		}

		/// <summary>
		/// Function to copy the contents of the pointer to a byte array.
		/// </summary>
		/// <typeparam name="T">Type of data in the array.</typeparam>
		/// <param name="source">Source pointer.</param>
		/// <param name="destination">Destination array.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="destination"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + destinationIndex is greater than the number of elements in the destination parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the destinationIndex is less than 0.</para>
		/// </exception>
		public static void CopyTo<T>(this IntPtr source, T[] destination)
			where T : struct
		{
#if DEBUG
		    if (source == IntPtr.Zero)
		    {
                throw new ArgumentNullException("source");
		    }

		    if (destination == null)
		    {
		        throw new ArgumentNullException("destination");
		    }
#endif

			CopyTo(source, destination, 0, destination.Length * DirectAccess.SizeOf<T>());
		}

		/// <summary>
		/// Function to copy the contents of the pointer to a byte array.
		/// </summary>
		/// <typeparam name="T">Type of data in the array.</typeparam>
		/// <param name="source">Source pointer.</param>
		/// <param name="destination">Destination array of bytes.</param>
		/// <param name="size">Size of the data to copy in bytes.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="destination"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + destinationIndex is greater than the number of elements in the destination parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the destinationIndex is less than 0.</para>
		/// </exception>
		public static void CopyTo<T>(this IntPtr source, T[] destination, int size)
			where T : struct
		{
			CopyTo(source, destination, 0, size);
		}

		/// <summary>
		/// Function to copy the contents of a byte array into the memory pointed at by the pointer.
		/// </summary>
		/// <param name="destination">Destination pointer.</param>
		/// <param name="source">Source pointer to copy from.</param>
		/// <param name="size">Number of bytes to copy.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the source pointer is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		public static void CopyFrom(this IntPtr destination, IntPtr source, int size)
		{
			DirectAccess.MemoryCopy(destination, source, size);
		}

		/// <summary>
		/// Function to copy the contents of a byte array into the memory pointed at by the pointer.
		/// </summary>
		/// <param name="destination">Destination pointer.</param>
		/// <param name="source">Source array to copy from.</param>
		/// <param name="sourceIndex">Index to start copying from in the source array.</param>
		/// <param name="size">Number of bytes to copy.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="source"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + sourceIndex is greater than the number of elements in the source parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the sourceIndex is less than 0.</para>
		/// </exception>
		public static void CopyFrom(this IntPtr destination, byte[] source, int sourceIndex, int size)
		{
			if (destination == IntPtr.Zero)
				return;

#if DEBUG
            if (destination == IntPtr.Zero)
            {
                throw new ArgumentNullException("destination");    
            }

		    if (source == null)
		    {
		        throw new ArgumentNullException("destination");
		    }

		    if (sourceIndex < 0)
		    {
		        throw new ArgumentOutOfRangeException("sourceIndex",
		                                              string.Format(Resources.GOR_INDEX_OUT_OF_RANGE, sourceIndex,
		                                                            source.Length));
		    }

		    if (sourceIndex + size > source.Length)
		    {
		        throw new ArgumentOutOfRangeException("sourceIndex",
		                                              string.Format(Resources.GOR_INDEX_OUT_OF_RANGE, sourceIndex + size,
		                                                            source.Length));
		    }
#endif

			DirectAccess.WriteArray(destination, source, sourceIndex, size);
		}

		/// <summary>
		/// Function to copy the contents of a byte array into the memory pointed at by the pointer.
		/// </summary>
		/// <param name="destination">Destination pointer.</param>
		/// <param name="source">Source array to copy from.</param>
		/// <param name="size">Number of bytes to copy.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="source"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
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
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="source"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + sourceIndex is greater than the number of elements in the source parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the sourceIndex is less than 0.</para>
		/// </exception>
		public static void CopyFrom(this IntPtr destination, byte[] source)
		{
#if DEBUG
			if (source == null)
				throw new ArgumentNullException("source");
#endif
			
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
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="source"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + sourceIndex is greater than the number of elements in the source parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the sourceIndex is less than 0.</para>
		/// </exception>
		public static void CopyFrom<T>(this IntPtr destination, T[] source, int sourceIndex, int size)
			where T : struct
		{

#if DEBUG
            int typeSize = DirectAccess.SizeOf<T>();
            
            if (destination == IntPtr.Zero)
		    {
		        throw new ArgumentNullException("destination");
		    }

		    if (source == null)
		    {
		        throw new ArgumentNullException("source");
		    }

		    if (sourceIndex < 0)
		    {
                throw new ArgumentOutOfRangeException("sourceIndex",
                                                      string.Format(Resources.GOR_INDEX_OUT_OF_RANGE, sourceIndex,
                                                                    source.Length));
            }

		    if ((sourceIndex*typeSize) + size > source.Length*typeSize)
		    {
		        throw new ArgumentOutOfRangeException("sourceIndex",
		                                              string.Format(Resources.GOR_INDEX_OUT_OF_RANGE,
		                                                            sourceIndex*typeSize + size,
		                                                            source.Length));
		    }
#endif

			DirectAccess.WriteArray(destination, source, sourceIndex, size);
		}

		/// <summary>
		/// Function to copy the contents of a byte array into the memory pointed at by the pointer.
		/// </summary>
		/// <typeparam name="T">Type of data in the array.</typeparam>
		/// <param name="destination">Destination pointer.</param>
		/// <param name="source">Source array to copy from.</param>
		/// <param name="size">Number of bytes to copy.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="source"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + sourceIndex is greater than the number of elements in the source parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the sourceIndex is less than 0.</para>
		/// </exception>
		public static void CopyFrom<T>(this IntPtr destination, T[] source, int size)
			where T : struct
		{
			CopyFrom(destination, source, 0, size);
		}

		/// <summary>
		/// Function to copy the contents of a byte array into the memory pointed at by the pointer.
		/// </summary>
		/// <typeparam name="T">Type of data in the array.</typeparam>
		/// <param name="destination">Destination pointer.</param>
		/// <param name="source">Source array to copy from.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="source"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if the size + sourceIndex is greater than the number of elements in the source parameter.
		/// <para>-or-</para>
		/// <para>Thrown if the sourceIndex is less than 0.</para>
		/// </exception>
		public static void CopyFrom<T>(this IntPtr destination, T[] source)
			where T : struct
		{
#if DEBUG
			if (source == null)
				throw new ArgumentNullException("source");
#endif

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
		/// Function to fill with a specific
		/// </summary>
		/// <param name="destination">Destination pointer to zero out.</param>
		/// <param name="fillValue">Value to fill the memory with.</param>
		/// <param name="size">Amount of memory to fill.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		public static void FillMemory(this IntPtr destination, byte fillValue, int size)
		{
			DirectAccess.FillMemory(destination, fillValue, size);
		}

		/// <summary>
		/// Function to marshal an object or value type into unmanaged memory.
		/// </summary>
		/// <param name="destination">Pointer to marshal the data into.</param>
		/// <param name="value">Object or value type to marshal.</param>
		/// <param name="deleteContents"><b>true</b> to remove any pre-allocated data, <b>false</b> to leave alone.</param>
		/// <remarks>This method will marshal a structure (object or value type) into unmanaged memory.
		/// <para>Passing <b>false</b> to <paramref name="deleteContents"/> may result in a memory leak if the data was previously initialized.</para>
		/// <para>For more information, see the <see cref="System.Runtime.InteropServices.Marshal.StructureToPtr">Marshal.StructureToPtr</see> method.</para>
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
		/// <para>For more information, see the <see cref="System.Runtime.InteropServices.Marshal.PtrToStructure(IntPtr, Type)">Marshal.PtrToStructure</see> method.</para>
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
		/// <para>For more information, see the <see cref="System.Runtime.InteropServices.Marshal.PtrToStructure(IntPtr, Type)">Marshal.PtrToStructure</see> method.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="value"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public static void MarshalTo<T>(this IntPtr source, ref T value)
		{
			Marshal.PtrToStructure(source, value);
		}

		/// <summary>
		/// Function to write a specific value type to the memory pointed at by the pointer.
		/// </summary>
		/// <typeparam name="T">Type of value to write.</typeparam>
		/// <param name="destination">The destination pointer.</param>
		/// <param name="value">The value to write.</param>
		/// <remarks>This method can only write value types composed of primitives, reference objects will not work.
		/// <para>There is no way to determine the size of the data pointed at by the pointer, so the user must take care not to write outside the bounds of the memory.</para>
		/// </remarks>
		public static void Write<T>(this IntPtr destination, ref T value)
			where T : struct
		{
			DirectAccess.WriteValue(destination, ref value);
		}

		/// <summary>
		/// Function to read a specific value from the memory pointed at by the pointer.
		/// </summary>
		/// <typeparam name="T">Type of value to read.</typeparam>
		/// <param name="source">The source pointer.</param>
		/// <param name="value">The value that was read from memory.</param>
		/// <returns>The value at the pointer.</returns>
		/// <remarks>This method can only write value types composed of primitives, reference objects will not work.
		/// <para>There is no way to determine the size of the data pointed at by the pointer, so the user must take care not to write outside the bounds of the memory.</para>
		/// </remarks>
		public static void Read<T>(this IntPtr source, out T value)
			where T : struct
		{
			DirectAccess.ReadValue(source, out value);
		}
	}
}
