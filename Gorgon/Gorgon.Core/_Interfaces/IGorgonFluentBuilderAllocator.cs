#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: June 7, 2018 3:43:19 PM
// 
#endregion

using Gorgon.Memory;

namespace Gorgon.Core
{
    /// <summary>
    /// An interface that defines a standard set of functionality for a builder pattern object.
    /// </summary>
    /// <typeparam name="TB">The type of builder. Used to return a fluent interface for the builder.</typeparam>
    /// <typeparam name="TBo">The type of object produced by the builder.</typeparam>
    /// <typeparam name="TBa">The type of optional allocator to use for building objects. Must derive from <see cref="GorgonRingPool{T}"/>.</typeparam>
    /// <remarks>
    /// <para>
    /// This interface is used to define a fluent builder pattern for creating objects.  
    /// </para>
    /// <para>
    /// Unlike the <see cref="IGorgonFluentBuilder{TB, TBo}"/> interface, this one defines an allocator type <typeparamref name="TBa"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonRingPool{T}"/>
    public interface IGorgonFluentBuilderAllocator<out TB, TBo, in TBa>
        : IGorgonFluentBuilder<TB, TBo>
        where TB : class
        where TBo : class
        where TBa : IGorgonAllocator<TBo>
    {
        /// <summary>
        /// Function to return the object.
        /// </summary>
        /// <param name="allocator">The allocator used to create an instance of the object</param>
        /// <returns>The object created or updated by this builder.</returns>
        /// <remarks>
        /// <para>
        /// Using an <paramref name="allocator"/> can provide different strategies when building objects.  If omitted, the object will be created using the standard <see langword="new"/> keyword.
        /// </para>
        /// <para>
        /// A custom allocator can be beneficial because it allows us to use a pool for allocating the objects, and thus allows for recycling of objects. This keeps the garbage collector happy by keeping objects
        /// around for as long as we need them, instead of creating objects that can potentially end up in the large object heap or in Gen 2.
        /// </para>
        /// </remarks>
        TBo Build(TBa allocator);
    }
}
