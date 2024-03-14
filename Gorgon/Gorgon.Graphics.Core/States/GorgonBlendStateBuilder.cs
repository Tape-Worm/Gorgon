
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
// Created: May 24, 2018 4:10:30 PM
// 


namespace Gorgon.Graphics.Core;

/// <summary>
/// A builder for a <see cref="GorgonBlendState"/> object
/// </summary>
/// <remarks>
/// <para>
/// Use this builder to create a new immutable <see cref="GorgonBlendState"/> to pass to a <see cref="GorgonPipelineState"/>. This object provides a fluent interface to help build up a blend state
/// </para>
/// <para>
/// A blend state will define how rasterized data is blended with the current render target(s). The ability to disable blending, define how blending operations are performed, etc... are all done through 
/// this state. This state also defines how blending is performed between adjacent render target(s) in the <see cref="GorgonGraphics.RenderTargets"/>. This is controlled by the 
/// <see cref="GorgonPipelineState.IsIndependentBlendingEnabled"/> flag on the <see cref="GorgonPipelineState"/> object
/// </para>
/// <para>
/// A blend state is an immutable object, and as such can only be created by using this object
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphics"/>
/// <seealso cref="GorgonPipelineState"/>
/// <seealso cref="GorgonBlendState"/>
public class GorgonBlendStateBuilder
    : GorgonStateBuilderAllocator<GorgonBlendStateBuilder, GorgonBlendState>
{

    /// <summary>
    /// Function to copy the state settings from the source state into the destination.
    /// </summary>
    /// <param name="dest">The destination state.</param>
    /// <param name="src">The state to copy.</param>
    private static void CopyState(GorgonBlendState dest, GorgonBlendState src)
    {
        dest.AlphaBlendOperation = src.AlphaBlendOperation;
        dest.ColorBlendOperation = src.ColorBlendOperation;
        dest.DestinationAlphaBlend = src.DestinationAlphaBlend;
        dest.DestinationColorBlend = src.DestinationColorBlend;
        dest.IsBlendingEnabled = src.IsBlendingEnabled;
        dest.LogicOperation = src.LogicOperation;
        dest.SourceAlphaBlend = src.SourceAlphaBlend;
        dest.SourceColorBlend = src.SourceColorBlend;
        dest.WriteMask = src.WriteMask;
    }

    /// <summary>
    /// Function to update the properties of the state from the working copy to the final copy.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    protected override GorgonBlendState OnCreateState() => new(WorkingState);

    /// <summary>Function to update the properties of the state, allocated from an allocator, from the working copy.</summary>
    /// <param name="state">The state to update.</param>
    protected override void OnUpdate(GorgonBlendState state) => CopyState(WorkingState, state);

    /// <summary>
    /// Function to reset the builder to the specified state.
    /// </summary>
    /// <param name="state">The state to copy from.</param>
    /// <returns>The fluent builder interface.</returns>
    protected override GorgonBlendStateBuilder OnResetTo(GorgonBlendState state)
    {
        CopyState(WorkingState, state);
        return this;
    }

    /// <summary>
    /// Function to clear the working state for the builder.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    protected override GorgonBlendStateBuilder OnClearState()
    {
        CopyState(WorkingState, GorgonBlendState.Default);
        return this;
    }

    /// <summary>
    /// Function to enable blending.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    public GorgonBlendStateBuilder EnableBlending()
    {
        WorkingState.IsBlendingEnabled = true;
        return this;
    }

    /// <summary>
    /// Function to disable blending.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    public GorgonBlendStateBuilder DisableBlending()
    {
        WorkingState.IsBlendingEnabled = false;
        return this;
    }

    /// <summary>
    /// Function to set the logical blending operation.
    /// </summary>
    /// <param name="logicOperation">The operation to perform.</param>
    /// <returns>The fluent builder interface.</returns>
    public GorgonBlendStateBuilder LogicOperation(LogicOperation logicOperation)
    {
        WorkingState.LogicOperation = logicOperation;
        return this;
    }

    /// <summary>
    /// Function to set the write mask.
    /// </summary>
    /// <param name="writeMask">The write mask to apply.</param>
    /// <returns>The fluent builder interface.</returns>
    public GorgonBlendStateBuilder WriteMask(WriteMask writeMask)
    {
        WorkingState.WriteMask = writeMask;
        return this;
    }

    /// <summary>
    /// Function to set the color, and alpha blend operations.
    /// </summary>
    /// <param name="color">[Optional] The color blend operation to assign.</param>
    /// <param name="alpha">[Optional] The alpha blend operation to assign.</param>
    /// <returns>The fluent builder interface.</returns>
    public GorgonBlendStateBuilder BlendOperation(BlendOperation? color = null, BlendOperation? alpha = null)
    {
        if (color is not null)
        {
            WorkingState.ColorBlendOperation = color.Value;
        }

        if (alpha is not null)
        {
            WorkingState.AlphaBlendOperation = alpha.Value;
        }

        return this;
    }

    /// <summary>
    /// Function to set the color, and alpha source blend operation.
    /// </summary>
    /// <param name="color">[Optional] The color blend operation to assign.</param>
    /// <param name="alpha">[Optional] The alpha blend operation to assign.</param>
    /// <returns>The fluent builder interface.</returns>
    public GorgonBlendStateBuilder SourceBlend(Blend? color = null, Blend? alpha = null)
    {
        if (color is not null)
        {
            WorkingState.SourceColorBlend = color.Value;
        }

        if (alpha is not null)
        {
            WorkingState.SourceAlphaBlend = alpha.Value;
        }

        return this;
    }

    /// <summary>
    /// Function to set the color, and alpha destination blend operation.
    /// </summary>
    /// <param name="color">[Optional] The color blend operation to assign.</param>
    /// <param name="alpha">[Optional] The alpha blend operation to assign.</param>
    /// <returns>The fluent builder interface.</returns>
    public GorgonBlendStateBuilder DestinationBlend(Blend? color = null, Blend? alpha = null)
    {
        if (color is not null)
        {
            WorkingState.DestinationColorBlend = color.Value;
        }

        if (alpha is not null)
        {
            WorkingState.DestinationAlphaBlend = alpha.Value;
        }

        return this;
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonBlendStateBuilder"/> class.
    /// </summary>
    public GorgonBlendStateBuilder()
        : base(new GorgonBlendState())
    {

    }

}
