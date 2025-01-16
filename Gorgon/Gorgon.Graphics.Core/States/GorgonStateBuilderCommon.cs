
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
/// Common functionality for a render state fluent builder, which allows creation of state objects using a <see cref="IGorgonAllocator{TRs}"/>
/// </summary>
/// <typeparam name="TB">The type of builder.</typeparam>
/// <typeparam name="TRs">The type of state.</typeparam>
/// <remarks>
/// <para>
/// The provides efficient reuse of objects to minimize garbage collection and improve performance. All state builder classes will descend from this unless they implement their own caching strategy.
/// </para>
/// </remarks>
/// <seealso cref="IGorgonAllocator{TRs}"/>
public abstract class GorgonStateBuilderCommon<TB, TRs>
    : IGorgonFluentBuilder<TB, TRs, IGorgonAllocator<TRs>>
    where TB : GorgonStateBuilderCommon<TB, TRs>
    where TRs : class, IEquatable<TRs>
{
    /// <summary>
    /// Property to set or return the state being edited.
    /// </summary>
    protected TRs WorkingState
    {
        get;
    }

    /// <summary>
    /// Function to create a new state object.
    /// </summary>
    /// <param name="allocator">The allocator used to create the object.</param>
    /// <returns>The new render state.</returns>
    /// <remarks>
    /// <para>
    /// This method should be used to create the object only, the state information will be copied into the object by the <see cref="OnUpdate"/> method.
    /// </para>
    /// <para>
    /// If the <paramref name="allocator"/> is null, the application should create the object using the <c>new</c> keyword. Otherwise, the <paramref name="allocator"/> should be used to create the object.
    /// </para>
    /// </remarks>
    protected abstract TRs OnCreate(IGorgonAllocator<TRs>? allocator);

    /// <summary>
    /// Function to reset the builder to the specified state.
    /// </summary>
    /// <param name="state">The state to copy from.</param>
    /// <returns>The fluent builder interface.</returns>
    protected abstract TB OnResetTo(TRs state);

    /// <summary>
    /// Function to clear the working state for the builder.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    protected abstract TB OnClear();

    /// <summary>
    /// Function to reset the builder to the specified state.
    /// </summary>
    /// <param name="state">[Optional] The specified state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    public TB ResetTo(TRs state = null) => state is null ? Clear() : OnResetTo(state);

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    public TB Clear() => OnClear();

    /// <summary>
    /// Function to update the properties of the state, allocated from an allocator, from the working copy.
    /// </summary>
    /// <param name="state">The state to update.</param>
    protected abstract void OnUpdate(TRs state);

    /// <summary>Function to return the object.</summary>
    /// <param name="allocator">[Optional] The allocator used to create an instance of the object.</param>
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
    public TRs Build(IGorgonAllocator<TRs>? allocator = null)
    {
        TRs state = OnCreate(allocator);

        OnUpdate(state);

        return state;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonStateBuilderCommon{TB,TRs}"/> class.
    /// </summary>
    /// <param name="renderState">The render state to use as a worker.</param>
    private protected GorgonStateBuilderCommon(TRs renderState) => WorkingState = renderState;
}
