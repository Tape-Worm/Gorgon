
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


using Gorgon.Core;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Common functionality for the a state fluent builder
/// </summary>
/// <typeparam name="TB">The type of builder.</typeparam>
/// <typeparam name="TRs">The type of state.</typeparam>
public abstract class GorgonStateBuilderCommon<TB, TRs>
    : IGorgonFluentBuilder<TB, TRs>
    where TB : GorgonStateBuilderCommon<TB, TRs>
    where TRs : class
{

    /// <summary>
    /// Property to set or return the state being edited.
    /// </summary>
    protected TRs WorkingState
    {
        get;
    }



    /// <summary>
    /// Function to create a new state with the properties copied from the working copy.
    /// </summary>
    /// <returns>The new render state.</returns>
    protected abstract TRs OnCreateState();

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
    protected abstract TB OnClearState();

    /// <summary>
    /// Function to return the state.
    /// </summary>
    /// <returns>The state created or updated by this builder.</returns>
    public TRs Build() => OnCreateState();

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
    public TB Clear() => OnClearState();



    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonStateBuilderCommon{TB,TRs}"/> class.
    /// </summary>
    /// <param name="renderState">The render state to use as a worker.</param>
    private protected GorgonStateBuilderCommon(TRs renderState) => WorkingState = renderState;

}
