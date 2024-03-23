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
// Created: June 7, 2018 3:55:47 PM
// 

using Gorgon.Memory;

namespace Gorgon.Patterns;

/// <summary>
/// An interface that defines a standard set of functionality for a builder pattern object.
/// </summary>
/// <typeparam name="TB">The type of builder. Used to return a fluent interface for the builder.</typeparam>
/// <typeparam name="TBo">The type of object produced by the builder.</typeparam>
/// <remarks>
/// <para>
/// This interface is used to define a pattern for a fluent builder used to create objects.  
/// </para>
/// </remarks>
public interface IGorgonFluentBuilder<out TB, TBo>
    where TB : class
    where TBo : class
{
    /// <summary>
    /// Function to return the object that is being built.
    /// </summary>
    /// <returns>The object being built by this fluent builder.</returns>
    /// <remarks>
    /// <para>
    /// Use this method to create a brand new instance of the object being built up by this builder.
    /// </para>
    /// <para>
    /// If memory pressure becomes a concern, then users can build a custom memory allocator by implementing a <see cref="IGorgonAllocator{T}"/> and use that in their implementation of the builder.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonAllocator{T}"/>
    TBo Build();

    /// <summary>
    /// Function to reset the builder to the specified state provided by the object passed in.
    /// </summary>
    /// <param name="builderObject">The specified object containing the state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <remarks>
    /// <para>
    /// Implementations can use this to make a copy of the settings for a previous object in the builder instead of manually setting all the state.
    /// </para>
    /// </remarks>
    TB ResetTo(TBo builderObject);

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    TB Clear();
}
