// 
// Gorgon.
// Copyright (C) 2024 Michael Winsor
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
// Created: September 24, 2019 9:34:01 AM
// 

namespace Gorgon.Memory;

/// <summary>
/// Defines a memory allocator strategy.
/// </summary>
/// <typeparam name="T">The type of object allocated by this allocator. Must be a reference type.</typeparam>
/// <remarks>
/// <para>
/// This interface is meant to be implemented by objects that will perform custom memory allocations using a specific strategy (e.g. a pool allocator, or a stack allocator). 
/// </para>
/// </remarks>
public interface IGorgonAllocator<T>
    where T : class
{
    /// <summary>
    /// Function to allocate a new object from the pool.
    /// </summary>
    /// <param name="initializer">[Optional] A function used to initialize the object returned by the allocator.</param>
    /// <returns>The newly allocated object.</returns>
    /// <remarks>
    /// <para>
    /// If the <paramref name="initializer"/> parameter is supplied, then this callback method can be used to initialize the new object before returning it from the allocator. If the object returned 
    /// is <b>null</b>, then this parameter will be ignored.
    /// </para>
    /// </remarks>
    T Allocate(Action<T>? initializer = null);
}
