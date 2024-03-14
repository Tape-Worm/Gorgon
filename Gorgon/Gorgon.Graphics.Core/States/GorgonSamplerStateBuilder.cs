#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 28, 2016 11:49:51 PM
// 
#endregion

using System;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A builder for a <see cref="GorgonSamplerState"/>.
/// </summary>
/// <remarks>
/// <para>
/// Use this builder to create a new immutable <see cref="GorgonSamplerState"/> to pass to a <see cref="GorgonPipelineState"/>. This object provides a fluent interface to help build up a sampler state.
/// </para>
/// <para>
/// This will define how a texture is sampled inside of a shader when rendering. Filtering, addressing, etc... are all defined in this state.
/// </para>
/// <para>
/// A sampler state is an immutable object, and as such can only be created by using this object.
/// </para>
/// <para>
/// Unlike the other state builders, this builder does not offer the optional use of a <see cref="GorgonStateBuilderPoolAllocator{T}"/>. This is because the state is cached internally and does not need 
/// an allocator pool.
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphics"/>
/// <seealso cref="GorgonPipelineState"/>
/// <seealso cref="GorgonSamplerState"/>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonSamplerStateBuilder" /> class.
/// </remarks>
/// <param name="graphics">The graphics interface used to build sampler states.</param>
public class GorgonSamplerStateBuilder(GorgonGraphics graphics)
        : GorgonStateBuilderCommon<GorgonSamplerStateBuilder, GorgonSamplerState>(new GorgonSamplerState()), IGorgonGraphicsObject
{
    #region Properties.
    /// <summary>
    /// Property to return the graphics interface that built this object.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    } = graphics ?? throw new ArgumentNullException(nameof(graphics));
    #endregion

    #region Methods.
    /// <summary>
    /// Function to copy the values of one state into another.
    /// </summary>
    /// <param name="dest">The destination state.</param>
    /// <param name="src">The state to copy.</param>
    private static void CopyState(GorgonSamplerState dest, GorgonSamplerState src)
    {
        dest.Filter = src.Filter;
        dest.WrapU = src.WrapU;
        dest.WrapV = src.WrapV;
        dest.WrapW = src.WrapW;
        dest.MaxAnisotropy = src.MaxAnisotropy;
        dest.BorderColor = src.BorderColor;
        dest.MinimumLevelOfDetail = src.MinimumLevelOfDetail;
        dest.MaximumLevelOfDetail = src.MaximumLevelOfDetail;
        dest.ComparisonFunction = src.ComparisonFunction;
        dest.MipLevelOfDetailBias = src.MipLevelOfDetailBias;
    }

    /// <summary>
    /// Function to update the properties of the state from the working copy to the final copy.
    /// </summary>
    /// <returns>The new render state.</returns>
    protected override GorgonSamplerState OnCreateState() => Graphics.SamplerStateCache.Cache(WorkingState);

    /// <summary>
    /// Function to reset the builder to the specified state.
    /// </summary>
    /// <param name="state">The state to copy from.</param>
    /// <returns>The fluent builder interface.</returns>
    protected override GorgonSamplerStateBuilder OnResetTo(GorgonSamplerState state)
    {
        CopyState(WorkingState, state);
        return this;
    }

    /// <summary>
    /// Function to clear the working state for the builder.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    protected override GorgonSamplerStateBuilder OnClearState()
    {
        CopyState(WorkingState, GorgonSamplerState.Default);
        return this;
    }

    /// <summary>
    /// Function to copy this sampler state into another sampler state.
    /// </summary>
    /// <param name="state">A <see cref="GorgonSamplerState"/> to copy the settings from.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="state"/> parameter is <b>null</b>.</exception>
    internal void CopyTo(GorgonSamplerState state)
    {
        WorkingState.Filter = state.Filter;
        WorkingState.WrapU = state.WrapU;
        WorkingState.WrapV = state.WrapV;
        WorkingState.WrapW = state.WrapW;
        WorkingState.MaxAnisotropy = state.MaxAnisotropy;
        WorkingState.BorderColor = state.BorderColor;
        WorkingState.MinimumLevelOfDetail = state.MinimumLevelOfDetail;
        WorkingState.MaximumLevelOfDetail = state.MaximumLevelOfDetail;
        WorkingState.ComparisonFunction = state.ComparisonFunction;
        WorkingState.MipLevelOfDetailBias = state.MipLevelOfDetailBias;
    }

    /// <summary>
    /// Function to set the type of filtering to apply to the texture.
    /// </summary>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>The fluent builder interface.</returns>
    public GorgonSamplerStateBuilder Filter(SampleFilter filter)
    {
        WorkingState.Filter = filter;
        return this;
    }

    /// <summary>
    /// Function to set the wrapping mode for a texture.
    /// </summary>
    /// <param name="wrapU">[Optional] The horizontal wrapping type.</param>
    /// <param name="wrapV">[Optional] The vertical wrapping type.</param>
    /// <param name="wrapW">[Optional] The depth wrapping type.</param>
    /// <param name="borderColor">[Optional] The color of the border when the wrapping type for any axis is set to <see cref="TextureWrap.Border"/>.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <remarks>
    /// <para>
    /// If all parameters are set to <b>null</b> (i.e. omitted), then the corresponding values will be reset to their defaults.
    /// </para>
    /// </remarks>
    public GorgonSamplerStateBuilder Wrapping(TextureWrap? wrapU = null, TextureWrap? wrapV = null, TextureWrap? wrapW = null, in GorgonColor? borderColor = null)
    {
        if ((wrapW is null) && (wrapU is null) && (wrapV is null) && (borderColor is null))
        {
            WorkingState.WrapU = WorkingState.WrapV = WorkingState.WrapW = TextureWrap.Clamp;
            WorkingState.BorderColor = GorgonColor.Transparent;
            return this;
        }

        if (wrapU is not null)
        {
            WorkingState.WrapU = wrapU.Value;
        }

        if (wrapV is not null)
        {
            WorkingState.WrapV = wrapV.Value;
        }

        if (wrapW is not null)
        {
            WorkingState.WrapW = wrapW.Value;
        }

        if (borderColor is not null)
        {
            WorkingState.BorderColor = borderColor.Value;
        }
        return this;
    }

    /// <summary>
    /// Function to set the value used to clamp an anisotropic texture filter.
    /// </summary>
    /// <param name="maxAnisotropy">The maximum anisotropy value.</param>
    /// <returns>The fluent builder interface.</returns>
    public GorgonSamplerStateBuilder MaxAnisotropy(int maxAnisotropy)
    {
        WorkingState.MaxAnisotropy = maxAnisotropy;
        return this;
    }

    /// <summary>
    /// Function to set the function to compare sampled data.
    /// </summary>
    /// <param name="compare">The function used for comparison.</param>
    /// <returns>The fluent builder interface.</returns>
    public GorgonSamplerStateBuilder ComparisonFunction(Comparison compare)
    {
        WorkingState.ComparisonFunction = compare;
        return this;
    }

    /// <summary>
    /// Function to set the minimum/maximum mip level of detail to use.
    /// </summary>
    /// <param name="min">[Optional] The lower end of the mip map range to clamp access to</param>
    /// <param name="max">[Optional] the higher end of the mip map range to clamp access to</param>
    /// <param name="mipLodBias">[Optional] The mip map level of detail bias value.</param>
    /// <returns>The fluent builder interface.</returns>
    /// <remarks>
    /// <para>
    /// If all parameters are set to <b>null</b> (i.e. omitted), then the corresponding values will be reset to their defaults.
    /// </para>
    /// </remarks>
        public GorgonSamplerStateBuilder MipLevelOfDetail(float? min = null, float? max = null, float? mipLodBias = null)
    {
        if ((min is null) && (max is null) && (mipLodBias is null))
        {
            WorkingState.MinimumLevelOfDetail = float.MinValue;
            WorkingState.MaximumLevelOfDetail = float.MaxValue;
            WorkingState.MipLevelOfDetailBias = 0;
            return this;
        }

        if (min is not null)
        {
            WorkingState.MinimumLevelOfDetail = min.Value;
        }

        if (max is not null)
        {
            WorkingState.MaximumLevelOfDetail = max.Value;
        }

        if (mipLodBias is not null)
        {
            WorkingState.MipLevelOfDetailBias = mipLodBias.Value;
        }

        return this;
    }

    #endregion
    #region Constructor.
    #endregion
}
