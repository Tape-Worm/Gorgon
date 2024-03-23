
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: May 23, 2018 12:18:45 PM
// 

using Gorgon.Memory;
using Gorgon.Patterns;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Common functionality for the a state fluent builder, which allows creation of state objects using <see cref="GorgonStateBuilderPoolAllocator{T}"/>
/// </summary>
/// <typeparam name="TB">The type of builder.</typeparam>
/// <typeparam name="TRs">The type of state.</typeparam>
/// <remarks>
/// <para>
/// This is the same as the <see cref="GorgonStateBuilderCommon{TB, TRs}"/> base class, only it exposes functionality to provide object allocation via a <see cref="GorgonStateBuilderPoolAllocator{T}"/>. 
/// The provides efficient reuse of objects to minimize garbage collection and improve performance. All state builder classes will descend from this unless they implement their own caching strategy
/// </para>
/// </remarks>
/// <seealso cref="GorgonStateBuilderCommon{TB, TRs}"/>
/// <seealso cref="GorgonStateBuilderPoolAllocator{T}"/>
public abstract class GorgonStateBuilderAllocator<TB, TRs>
    : GorgonStateBuilderCommon<TB, TRs>, IGorgonFluentBuilderAllocator<TB, TRs, IGorgonAllocator<TRs>>
    where TB : GorgonStateBuilderCommon<TB, TRs>
    where TRs : class, IEquatable<TRs>
{
    /// <summary>
    /// Function to update the properties of the state, allocated from an allocator, from the working copy.
    /// </summary>
    /// <param name="state">The state to update.</param>
    protected abstract void OnUpdate(TRs state);

    /// <summary>Function to return the object.</summary>
    /// <param name="allocator">The allocator used to create an instance of the object</param>
    /// <returns>The object created or updated by this builder.</returns>
    /// <remarks>
    ///   <para>
    /// Using an <paramref name="allocator" /> can provide different strategies when building objects.  If omitted, the object will be created using the standard <span class="keyword">new</span> keyword.
    /// </para>
    ///   <para>
    /// A custom allocator can be beneficial because it allows us to use a pool for allocating the objects, and thus allows for recycling of objects. This keeps the garbage collector happy by keeping objects
    /// around for as long as we need them, instead of creating objects that can potentially end up in the large object heap or in Gen 2.
    /// </para>
    /// </remarks>
    public TRs Build(IGorgonAllocator<TRs> allocator)
    {
        if (allocator is null)
        {
            return OnCreateState();
        }

        TRs state = allocator.Allocate();

        OnUpdate(state);

        return state;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonStateBuilderAllocator{TB,TRs}"/> class.
    /// </summary>
    /// <param name="renderState">The render state to use as a worker.</param>
    private protected GorgonStateBuilderAllocator(TRs renderState)
        : base(renderState)
    {
    }
}
