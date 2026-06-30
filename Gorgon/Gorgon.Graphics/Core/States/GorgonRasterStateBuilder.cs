
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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

using Gorgon.Memory;
using Gorgon.Patterns;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A builder for a <see cref="GorgonRasterState"/> object.
/// </summary>
/// <remarks>
/// <para>
/// Use this builder to create a new immutable <see cref="GorgonRasterState"/> to pass to a <see cref="GorgonGraphicsPsoBuilder"/>. This will define to rasterize primitives while rendering.
/// </para>
/// <para>
/// A <see cref="GorgonRasterState"/> is an immutable object, it can only be created through this builder factory type.
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphics"/>
/// <seealso cref="GorgonGraphicsPsoBuilder"/>
/// <seealso cref="GorgonGraphicsPso"/>
/// <seealso cref="GorgonRasterState"/>
public class GorgonRasterStateBuilder
    : IGorgonFluentBuilder<GorgonRasterStateBuilder, GorgonRasterState, IGorgonAllocator<GorgonRasterState>>
{
    /// <summary>
    /// The default allocator for generating blend states.
    /// </summary>
    private class DefaultAllocator
        : IGorgonAllocator<GorgonRasterState>
    {
        /// <inheritdoc/>
        public GorgonRasterState Allocate(Action<GorgonRasterState>? initializer = null)
        {
            GorgonRasterState result = new();
            initializer?.Invoke(result);
            return result;
        }
    }

    private readonly GorgonRasterState _worker = new();
    private readonly DefaultAllocator _allocator = new();

    /// <summary>
    /// Function to copy the state settings from the source state into the destination.
    /// </summary>
    /// <param name="src">The state to copy.</param>
    /// <param name="dest">The destination state.</param>
    private static void Copy(GorgonRasterState src, GorgonRasterState dest)
    {
        dest.CullMode = src.CullMode;
        dest.FillMode = src.FillMode;
        dest.DepthBias = src.DepthBias;
        dest.DepthBiasClamp = src.DepthBiasClamp;
        dest.IsDepthClippingEnabled = src.IsDepthClippingEnabled;
        dest.ForcedReadWriteViewSampleCount = src.ForcedReadWriteViewSampleCount;
        dest.IsFrontCounterClockwise = src.IsFrontCounterClockwise;
        dest.LineRasterizationMode = src.LineRasterizationMode;
        dest.UseConservativeRasterization = src.UseConservativeRasterization;
        dest.SlopeScaledDepthBias = src.SlopeScaledDepthBias;
    }

    /// <summary>
    /// Function to turn on conservative rasterization.
    /// </summary>
    /// <param name="enabled"><b>true</b> to enable conservative rasterization, <b>false</b> to disable.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <inheritdoc cref="GorgonRasterState.UseConservativeRasterization" path="/remarks"/>
    public GorgonRasterStateBuilder ConservativeRasterizationEnabled(bool enabled)
    {
        _worker.UseConservativeRasterization = enabled;
        return this;
    }

    /// <summary>
    /// Function to set the culling mode.
    /// </summary>
    /// <param name="cullMode">The current culling mode.</param>
    /// <param name="isFrontCounterClockwise">[Optional] <b>true</b> if vertices that are ordered counter-clockwise are considered front facing, <b>false</b> if not.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <remarks>
    /// <para>
    /// The default values are <see cref="CullingMode.Back"/>, and <b>false</b>.
    /// </para>
    /// </remarks>
    public GorgonRasterStateBuilder CullMode(CullingMode cullMode, bool? isFrontCounterClockwise = null)
    {
        _worker.CullMode = cullMode;
        if (isFrontCounterClockwise is not null)
        {
            _worker.IsFrontCounterClockwise = isFrontCounterClockwise.Value;
        }

        return this;
    }

    /// <summary>
    /// Function to set the primitive fill mode.
    /// </summary>
    /// <param name="fillMode">The current primitive fill mode.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <inheritdoc cref="GorgonRasterState.FillMode" path="/remarks"/>
    public GorgonRasterStateBuilder FillMode(FillMode fillMode)
    {
        _worker.FillMode = fillMode;
        return this;
    }

    /// <summary>
    /// Function to set the depth bias parameters.
    /// </summary>
    /// <param name="depthBias">The depth bias.</param>
    /// <param name="depthBiasClamp">The depth bias clamping value.</param>
    /// <param name="slopeScaledDepthBias">The slope scaled depth bias value.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <remarks>
    /// <para>
    /// The default values are 0, 0 and 0.
    /// </para>
    /// </remarks>
    public GorgonRasterStateBuilder DepthBias(int depthBias, float depthBiasClamp, float slopeScaledDepthBias)
    {
        _worker.DepthBias = depthBias;
        _worker.DepthBiasClamp = depthBiasClamp;
        _worker.SlopeScaledDepthBias = slopeScaledDepthBias;
        return this;
    }

    /// <summary>
    /// Function to set the forced unordered access view count.
    /// </summary>
    /// <param name="sampleCount">The sample count to set.</param>
    /// <returns>The fluent interface for this builder.</returns>
    /// <inheritdoc cref="GorgonRasterState.ForcedReadWriteViewSampleCount" path="/remarks"/>
    public GorgonRasterStateBuilder ForcedReadWriteViewSampleCount(int sampleCount)
    {
        _worker.ForcedReadWriteViewSampleCount = sampleCount;
        return this;
    }

    /// <summary>
    /// Function to set the rasterization mode for line primitives.
    /// </summary>
    /// <returns>The fluent interface for this builder.</returns>
    /// <inheritdoc cref="GorgonRasterState.LineRasterizationMode" path="/remarks"/>
    public GorgonRasterStateBuilder LineRasterizationMode(LineRasterizationMode mode)
    {
        _worker.LineRasterizationMode = mode;
        return this;
    }

    /// <inheritdoc/>
    public GorgonRasterState Build(IGorgonAllocator<GorgonRasterState>? allocator = null)
    {
        allocator ??= _allocator;
        GorgonRasterState result = allocator.Allocate(rs => Copy(_worker, rs));
        return result;
    }

    /// <inheritdoc/>
    public GorgonRasterStateBuilder ResetTo(GorgonRasterState builderObject)
    {
        Copy(builderObject, _worker);
        return this;
    }

    /// <inheritdoc/>
    public GorgonRasterStateBuilder Clear()
    {
        Copy(GorgonRasterState.Default, _worker);
        return this;
    }
}
