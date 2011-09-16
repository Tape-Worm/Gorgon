#pragma region MIT.
// 
// GorgonMemory.
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
// Created: Friday, September 16, 2011 6:44:33 AM
// 
#pragma endregion

// GorgonMemory.h

#pragma once

#include"Stdafx.h"

using namespace System;

namespace Memory {
	/// <summary>
	/// Functionality to write directly into unmanaged memory.
	/// </summary>
	public ref class GorgonMemory
	{
	public:
		/// <summary>
		/// Function to write an array of value types into unmanaged memory.
		/// </summary>
		/// <param name="destination">Pointer to write into.</param>
		/// <param name="buffer">Array to copy from.</param>
		/// <param name="offset">Index in the array to start copying from.</param>
		/// <param name="size>Number of bytes to copy.</param>
		/// <returns>The number of bytes written.</returns>		
		generic<typename T> where T : value class
		static void Write(IntPtr destination, array<T>^ buffer, int offset, int size)
		{
			pin_ptr<T> pinned = &buffer[offset];
			memcpy(destination.ToPointer(), pinned, static_cast<size_t>(size));
		}

		/// <summary>
		/// Function to write an array of value types into unmanaged memory.
		/// </summary>
		/// <param name="destination">Pointer to write into.</param>
		/// <param name="buffer">Array to copy from.</param>
		/// <param name="offset">Index in the array to start copying from.</param>
		/// <param name="size>Number of bytes to copy.</param>
		/// <returns>The number of bytes written.</returns>		
		generic<typename T> where T : value class
		static void Read(IntPtr source, array<T>^ buffer, int offset, int size)
		{
			pin_ptr<T> pinned = &buffer[offset];
			memcpy(pinned, source.ToPointer(), static_cast<size_t>(size));
		}

		/// <summary>
		/// Function to write a single value directly into unmanaged memory.
		/// </summary>
		/// <param name="destination">Pointer to write into.</param>
		/// <param name="value">Value to write.</param>
		/// <returns>The number of bytes written.</returns>
		generic<typename T> where T : value class
		static int Write(IntPtr destination, T value)
		{
			int size = static_cast<int>(sizeof(T));
			memcpy(destination.ToPointer(), &value, static_cast<size_t>(size));
			return size;
		}

		/// <summary>
		/// Function to read a single value directly from unmanaged memory.
		/// </summary>
		/// <param name="source">Pointer to read from.</param>
		/// <param name="value">Value read from the pointer.</param>
		/// <returns>The number of bytes read.</returns>
		generic<typename T> where T : value class
		static int Read(IntPtr source, T %value)
		{
			int size = static_cast<int>(sizeof(T));

			pin_ptr<T> pinned = &value;
			memcpy(pinned, source.ToPointer(), static_cast<size_t>(size));

			return size;
		}
	};
}
