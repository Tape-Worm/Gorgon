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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GorgonLibrary.Data
{
	/// <summary>
	/// Extensions for manipulating memory via an Intptr.
	/// </summary>
	public static class GorgonIntPtrExtensions
	{
		/// <summary>
		/// Function to perform a fast copy between two pointers.
		/// </summary>
		/// <param name="sourcePointer">Source pointer.</param>
		/// <param name="destPointer">Destination pointer.</param>
		/// <param name="size">Number of bytes to copy.</param>
		private unsafe static void FastCopy(byte* sourcePointer, byte* destPointer, int size)
		{
			// Copy 8 bytes at a time if we're on x64.
			if ((size >= 8) && (Gorgon.PlatformArchitecture == PlatformArchitecture.x64))
			{
				int copy8 = size / 8;

				for (int i = 0; i < copy8; i++)
				{
					*((long*)destPointer) = *((long*)sourcePointer);
					sourcePointer += 8;
					destPointer += 8;
					size -= 8;
				}
			}

			if (size >= 4)
			{
				int copy4 = size / 4;

				// Copy 4 bytes at a time.
				for (int i = 0; i < copy4; i++)
				{
					*((int*)destPointer) = *((int*)sourcePointer);
					sourcePointer += 4;
					destPointer += 4;
					size -= 4;
				}
			}

			if (size > 0)
			{
				// Copy remaining bytes.
				for (int i = 0; i < size; i++)
				{
					*destPointer = *sourcePointer;
					sourcePointer++;
					destPointer++;
				}
			}
		}

		/// <summary>
		/// Function to copy the memory contents from this pointer to another.
		/// </summary>
		/// <param name="destination">Destination buffer.</param>
		/// <param name="source">Source buffer.</param>
		/// <param name="size">Size of the data to copy, in bytes.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the destination pointer is NULL (Nothing in VB.Net).</exception>
		public unsafe static void CopyTo(this IntPtr source, void* destination, int size)
		{
			if (source == IntPtr.Zero)
				return;

			if (destination == null)
				throw new ArgumentNullException("destination");

			FastCopy((byte*)source, (byte *)destination, size);
		}

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
			CopyTo(source, (void *)destination, size);
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

			if (destinationIndex + size > destination.Length * Marshal.SizeOf(typeof(T)))
				throw new ArgumentOutOfRangeException("destinationIndex", "Index and size cannot be larger than the array.");

			unsafe
			{
				GCHandle destHandle = GCHandle.Alloc(destination, GCHandleType.Pinned);

				try
				{
					byte* destPtr = (byte *)Marshal.UnsafeAddrOfPinnedArrayElement(destination, destinationIndex);
					FastCopy((byte*)source, destPtr, size);
				}
				finally
				{
					if (destHandle.IsAllocated)
						destHandle.Free();
				}
			}
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

			CopyTo<T>(source, destination, 0, destination.Length * Marshal.SizeOf(typeof(T)));
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
		public unsafe static void CopyFrom(this IntPtr destination, void *source, int size)
		{
			if (destination == IntPtr.Zero)
				return;

			if (source == null)
				throw new ArgumentNullException("destination");

			FastCopy((byte*)source, (byte*)destination, size);
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
			CopyFrom(source, destination, size);
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
			if (destination == IntPtr.Zero)
				return;

			if (source == null)
				throw new ArgumentNullException("destination");

			if (sourceIndex < 0)
				throw new ArgumentOutOfRangeException("sourceIndex", "Index cannot be less than zero.");

			if (sourceIndex + size > source.Length * Marshal.SizeOf(typeof(T)))
				throw new ArgumentOutOfRangeException("sourceIndex", "Index and size cannot be larger than the array.");

			unsafe
			{
				GCHandle destHandle = GCHandle.Alloc(source, GCHandleType.Pinned);

				try
				{
					byte* srcPtr = (byte *)Marshal.UnsafeAddrOfPinnedArrayElement(source, sourceIndex);
					FastCopy(srcPtr, (byte *)destination, size);
				}
				finally
				{
					if (destHandle.IsAllocated)
						destHandle.Free();
				}
			}
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

			CopyFrom(destination, source, 0, source.Length * Marshal.SizeOf(typeof(T)));
		}

		/// <summary>
		/// Function to zero out the memory pointed to by this pointer.
		/// </summary>
		/// <param name="source">Source pointer to zero out.</param>
		/// <param name="size">Amount of memory to zero out.</param>
		/// <remarks>Since a pointer doesn't have a size associated with it, care must be taken to not overstep the bounds of the data pointed at by the pointer.</remarks>
		public unsafe static void ZeroMemory(this IntPtr source, int size)
		{
			if (source == IntPtr.Zero)
				return;

			byte* thisPtr = (byte*)source;

			// Copy 8 bytes at a time if we're on x64.
			if ((size >= 8) && (Gorgon.PlatformArchitecture == PlatformArchitecture.x64))
			{
				int copy8 = size / 8;

				for (int i = 0; i < copy8; i++)
				{
					*((long*)thisPtr) = 0;
					thisPtr += 8;
					size -= 8;
				}
			}

			if (size >= 4)
			{
				int copy4 = size / 4;

				// Copy 4 bytes at a time.
				for (int i = 0; i < copy4; i++)
				{
					*((int*)thisPtr) = 0;
					thisPtr += 4;
					size -= 4;
				}
			}

			if (size > 0)
			{
				// Copy remaining bytes.
				for (int i = 0; i < size; i++)
				{
					*thisPtr = 0;
					thisPtr++;
				}
			}
		}
	}
}
