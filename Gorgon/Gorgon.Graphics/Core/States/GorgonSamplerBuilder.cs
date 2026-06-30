// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// Created: April 14, 2026 8:59:51 PM
//

using System.Runtime.CompilerServices;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Memory;
using Gorgon.Patterns;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A builder for a <see cref="GorgonSampler"/> object.
/// </summary>
/// <remarks>
/// <para>
/// Use this builder to create a new immutable <see cref="GorgonSampler"/> object to bindlessly pass to a shader. The sampler will be used by the shader to provide filtered samples of texel data.
/// </para>
/// <para>
/// A <see cref="GorgonSampler"/> is an immutable object, it can only be created through this builder factory type.
/// </para>
/// </remarks>
/// <seealso cref="GorgonSampler"/>
public sealed class GorgonSamplerBuilder
    : IGorgonFluentBuilder<GorgonSamplerBuilder, GorgonSampler, IGorgonAllocator<GorgonSampler>, string>
{
    private readonly GorgonSampler _worker;
    
    /// <summary>
    /// Property to return the graphics interface to associate with the built objects.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <summary>
    /// Function to copy the settings for a sampler.
    /// </summary>
    /// <param name="source">The source sampler to copy.</param>
    /// <param name="destination">The destination sampler to update.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy(GorgonSampler source, GorgonSampler destination)
    {
        destination.Name = source.Name;
        destination.Comparison = source.Comparison;
        destination.BorderColor = source.BorderColor;
        destination.MinimumLod = source.MinimumLod;
        destination.MaximumLod = source.MaximumLod;
        destination.MipLodBias = source.MipLodBias;
        destination.MaxAnisotropy = source.MaxAnisotropy;
        destination.BorderUsesIntegerColor = source.BorderUsesIntegerColor;
        destination.UAddressing = source.UAddressing;
        destination.VAddressing = source.VAddressing;
        destination.WAddressing = source.WAddressing;
        destination.Filter = source.Filter;
    }

    /// <inheritdoc/>
    public GorgonSamplerBuilder ResetTo(GorgonSampler builderObject)
    {
        Copy(builderObject, _worker);
        return this;
    }

    /// <inheritdoc/>
    public GorgonSamplerBuilder Clear()
    {
        Copy(GorgonSampler.Default(Graphics), _worker);
        _worker.Name = string.Empty;

        return this;
    }

    /// <inheritdoc/>
    /// <param name="name">The name of the sampler.</param>
    /// <param name="allocator"><inheritdoc cref="IGorgonFluentBuilder{TB, TBo, TBa, TP1}.Build" path="/param[@name='allocator']"/></param>
    /// <remarks>
    /// <inheritdoc path="/remarks/para"/>
    /// <para>
    /// The <paramref name="name"/> is required, if it is left empty, a name will be generated.
    /// </para>
    /// </remarks>
    public GorgonSampler Build(string name, IGorgonAllocator<GorgonSampler>? allocator = null)
    {        
        GorgonSampler result = allocator is null ? new GorgonSampler(Graphics, string.Empty) : allocator.Allocate(s => s.ResetDescriptor());
        _worker.Name = GorgonGraphicsFactory.GenerateName(name, nameof(GorgonSampler));
        Copy(_worker, result);
        return result;
    }

    /// <summary>
    /// Function to set the comparison function for the sampler.
    /// </summary>
    /// <param name="comparison">The comparison function to apply.</param>
    /// <inheritdoc cref="Clear" path="/returns"/>
    /// <inheritdoc cref="GorgonSampler.Comparison" path="/remarks"/>
    public GorgonSamplerBuilder Comparison(ComparisonFunction comparison)
    {
        _worker.Comparison = comparison;
        return this;
    }

    /// <summary>
    /// Function to set the border color for the sampler.
    /// </summary>
    /// <param name="color">The border color to apply.</param>
    /// <inheritdoc cref="Clear" path="/returns"/>
    /// <inheritdoc cref="GorgonSampler.BorderColor" path="/remarks"/>    
    /// <inheritdoc cref="GorgonSampler.BorderColor" path="/seealso"/>    
    public GorgonSamplerBuilder BorderColor(GorgonColor color)
    {
        _worker.BorderColor = color;
        return this;
    }

    /// <summary>
    /// Function to set the minimum and maximum level of detail for the sampler.
    /// </summary>
    /// <param name="minLod">The minimum level of detail value to apply.</param>
    /// <param name="maxLod">The maximum level of detail value to apply.</param>
    /// <inheritdoc cref="Clear" path="/returns"/>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="minLod"/>, or the <paramref name="maxLod"/> parameter is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="minLod"/> is greater than the <paramref name="maxLod"/> value.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="minLod"/> is the lower end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed.
    /// </para>
    /// <para>
    /// The <paramref name="maxLod"/> is the upper end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed. 
    /// </para>
    /// <para>
    /// The <paramref name="maxLod"/> must be greater than or equal to <paramref name="minLod"/>. To have no upper limit on LOD, set <paramref name="maxLod"/> to <see cref="float.MaxValue"/>.
    /// </para>
    /// <inheritdoc cref="GorgonSampler.MaximumLod" path="/remarks/para"/>
    /// </remarks>    
    public GorgonSamplerBuilder LodRange(float minLod, float maxLod)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minLod);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLod);

        if (minLod > maxLod)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_MIP_LOD_MIN_LARGER_THAN_MAX, minLod, maxLod));
        }

        _worker.MinimumLod = minLod;
        _worker.MaximumLod = maxLod;
        return this;
    }

    /// <summary>
    /// Function to set the offset to apply to the calculated mip level.
    /// </summary>
    /// <param name="lodOffset">The level of detail value offset to apply.</param>
    /// <inheritdoc cref="Clear" path="/returns"/>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="lodOffset"/> value is less than <see cref="GorgonSampler.MipLodBiasMinimum"/> or greater than <see cref="GorgonSampler.MipLodBiasMaximum"/>.</exception>
    /// <inheritdoc cref="GorgonSampler.MipLodBias" path="/remarks"/>
    public GorgonSamplerBuilder MipLodBias(float lodOffset)
    {        
        ArgumentOutOfRangeException.ThrowIfLessThan(lodOffset, GorgonSampler.MipLodBiasMinimum);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(lodOffset, GorgonSampler.MipLodBiasMaximum);

        _worker.MipLodBias = lodOffset;
        return this;
    }

    /// <summary>
    /// Function to set whether the <see cref="GorgonSampler.BorderColor"/> components are interpreted as 32-bit integer values.
    /// </summary>
    /// <param name="value"><b>true</b> to enable using 32-bit integer values for color components, <b>false</b> to use the standard floating point values for color components.</param>
    /// <inheritdoc cref="Clear" path="/returns"/>
    /// <inheritdoc cref="GorgonSampler.BorderUsesIntegerColor" path="/remarks"/>
    /// <inheritdoc cref="GorgonSampler.BorderUsesIntegerColor" path="/seealso"/>
    public GorgonSamplerBuilder BorderUsesIntegerColor(bool value)
    {
        _worker.BorderUsesIntegerColor = value;
        return this;
    }

    /// <summary>
    /// Function to set the addressing mode for the axes of the texture.
    /// </summary>
    /// <param name="u">The horizontal addressing mode.</param>
    /// <param name="v">The vertical addressing mode.</param>
    /// <param name="w">[Optional] The depth addressing mode.</param>
    /// <inheritdoc cref="Clear" path="/returns"/>
    /// <remarks>
    /// <para>
    /// All 3 values default to <see cref="TextureAddressing.Clamp"/>.
    /// </para>
    /// </remarks>
    public GorgonSamplerBuilder Addressing(TextureAddressing u, TextureAddressing v, TextureAddressing w = TextureAddressing.Clamp)
    {
        _worker.UAddressing = u;
        _worker.VAddressing = v;
        _worker.WAddressing = w;
        return this;
    }

    /// <summary>
    /// Function to set the filtering to perform when sampling the texture.
    /// </summary>
    /// <param name="filter">The type of filtering.</param>
    /// <inheritdoc cref="Clear" path="/returns"/>
    /// <inheritdoc cref="GorgonSampler.Filter" path="/remarks"/>
    public GorgonSamplerBuilder Filter(TextureFilter filter)
    {
        _worker.Filter = filter;
        return this;
    }

    /// <summary>
    /// Function to set the maximum clamping value for anisotropic filters.
    /// </summary>
    /// <param name="maxValue">The clamping value for anisotropic filters.</param>
    /// <inheritdoc cref="Clear" path="/returns"/>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="maxValue"/> value is less than 1 or greater than 16.</exception>
    /// <remarks>
    /// <inheritdoc cref="GorgonSampler.MaxAnisotropy" path="/remarks/para"/>
    /// <para>
    /// This value must be an integer value between 1 to 16.
    /// </para>
    /// </remarks>
    public GorgonSamplerBuilder MaxAnisotropy(int maxValue)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxValue, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(maxValue, 16);

        _worker.MaxAnisotropy = maxValue;
        return this;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonSamplerBuilder"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface to associate with the built objects.</param>
    public GorgonSamplerBuilder(GorgonGraphics graphics)
    {
        Graphics = graphics;
        
        _worker = new GorgonSampler(graphics, string.Empty)
        {
            Name = string.Empty
        };
        // Do not register this sampler, we don't need to dispose it.
        _worker.UnregisterDisposable(graphics);
    }
}
