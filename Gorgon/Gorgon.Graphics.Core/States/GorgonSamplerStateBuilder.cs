
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: July 28, 2016 11:49:51 PM
// 

using Gorgon.Patterns;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A builder for a <see cref="GorgonSamplerState"/>
/// </summary>
/// <remarks>
/// <para>
/// Use this builder to create a new immutable <see cref="GorgonSamplerState"/> to pass to a <see cref="GorgonPipelineState"/>. This object provides a fluent interface to help build up a sampler state
/// </para>
/// <para>
/// This will define how a texture is sampled inside of a shader when rendering. Filtering, addressing, etc... are all defined in this state
/// </para>
/// <para>
/// A sampler state is an immutable object, and as such can only be created by using this object
/// </para>
/// <para>
/// Unlike the other state builders, this builder does not offer the optional use of a <see cref="Memory.IGorgonAllocator{T}"/>. This is because the state is cached internally and does not need 
/// an allocator pool
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphics"/>
/// <seealso cref="GorgonPipelineState"/>
/// <seealso cref="GorgonSamplerState"/>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonSamplerStateBuilder" /> class
/// </remarks>
/// <param name="graphics">The graphics interface used to build sampler states.</param>
public class GorgonSamplerStateBuilder(GorgonGraphics graphics)
        : IGorgonFluentBuilder<GorgonSamplerStateBuilder, GorgonSamplerState>, IGorgonGraphicsObject
{
    // The state being edited.
    private readonly GorgonSamplerState _workingState = new();

    /// <summary>
    /// Property to return the graphics interface that built this object.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    } = graphics ?? throw new ArgumentNullException(nameof(graphics));

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
    /// Function to copy this sampler state into another sampler state.
    /// </summary>
    /// <param name="state">A <see cref="GorgonSamplerState"/> to copy the settings from.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="state"/> parameter is <b>null</b>.</exception>
    internal void CopyTo(GorgonSamplerState state)
    {
        _workingState.Filter = state.Filter;
        _workingState.WrapU = state.WrapU;
        _workingState.WrapV = state.WrapV;
        _workingState.WrapW = state.WrapW;
        _workingState.MaxAnisotropy = state.MaxAnisotropy;
        _workingState.BorderColor = state.BorderColor;
        _workingState.MinimumLevelOfDetail = state.MinimumLevelOfDetail;
        _workingState.MaximumLevelOfDetail = state.MaximumLevelOfDetail;
        _workingState.ComparisonFunction = state.ComparisonFunction;
        _workingState.MipLevelOfDetailBias = state.MipLevelOfDetailBias;
    }

    /// <summary>
    /// Function to set the type of filtering to apply to the texture.
    /// </summary>
    /// <param name="filter">The filter to apply.</param>
    /// <returns>The fluent builder interface.</returns>
    public GorgonSamplerStateBuilder Filter(SampleFilter filter)
    {
        _workingState.Filter = filter;
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
    public GorgonSamplerStateBuilder Wrapping(TextureWrap? wrapU = null, TextureWrap? wrapV = null, TextureWrap? wrapW = null, GorgonColor? borderColor = null)
    {
        if ((wrapW is null) && (wrapU is null) && (wrapV is null) && (borderColor is null))
        {
            _workingState.WrapU = _workingState.WrapV = _workingState.WrapW = TextureWrap.Clamp;
            _workingState.BorderColor = GorgonColors.Transparent;
            return this;
        }

        if (wrapU is not null)
        {
            _workingState.WrapU = wrapU.Value;
        }

        if (wrapV is not null)
        {
            _workingState.WrapV = wrapV.Value;
        }

        if (wrapW is not null)
        {
            _workingState.WrapW = wrapW.Value;
        }

        if (borderColor is not null)
        {
            _workingState.BorderColor = borderColor.Value;
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
        _workingState.MaxAnisotropy = maxAnisotropy;
        return this;
    }

    /// <summary>
    /// Function to set the function to compare sampled data.
    /// </summary>
    /// <param name="compare">The function used for comparison.</param>
    /// <returns>The fluent builder interface.</returns>
    public GorgonSamplerStateBuilder ComparisonFunction(Comparison compare)
    {
        _workingState.ComparisonFunction = compare;
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
            _workingState.MinimumLevelOfDetail = float.MinValue;
            _workingState.MaximumLevelOfDetail = float.MaxValue;
            _workingState.MipLevelOfDetailBias = 0;
            return this;
        }

        if (min is not null)
        {
            _workingState.MinimumLevelOfDetail = min.Value;
        }

        if (max is not null)
        {
            _workingState.MaximumLevelOfDetail = max.Value;
        }

        if (mipLodBias is not null)
        {
            _workingState.MipLevelOfDetailBias = mipLodBias.Value;
        }

        return this;
    }

    /// <summary>
    /// Function to return the state.
    /// </summary>
    /// <returns>The state created or updated by this builder.</returns>
    public GorgonSamplerState Build() => Graphics.SamplerStateCache.Cache(_workingState);

    /// <summary>
    /// Function to reset the builder to the specified state.
    /// </summary>
    /// <param name="state">[Optional] The specified state to copy.</param>
    /// <returns>The fluent builder interface.</returns>
    public GorgonSamplerStateBuilder ResetTo(GorgonSamplerState? state = null)
    {
        if (state is null)
        {
            Clear();
            return this;
        }

        CopyState(_workingState, state);
        return this;
    }

    /// <summary>
    /// Function to clear the builder to a default state.
    /// </summary>
    /// <returns>The fluent builder interface.</returns>
    public GorgonSamplerStateBuilder Clear()
    {
        CopyState(_workingState, GorgonSamplerState.Default);
        return this;
    }
}
