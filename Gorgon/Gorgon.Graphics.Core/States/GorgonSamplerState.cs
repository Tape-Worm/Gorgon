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
using System.Diagnostics;
using Gorgon.Core;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Describes how texture sampling should be performed when a texture is sampled in a shader.
/// </summary>
/// <remarks>
/// <para>
/// This will define how a texture is sampled inside of a shader when rendering. Filtering, addressing, etc... are all defined in this state.
/// </para>
/// <para>
/// The sampler state contains 4 common samplers used by applications: <see cref="Default"/> (bilinear filtering), <see cref="PointFiltering"/> (point or "pixelated" filtering), 
/// <see cref="AnisotropicFiltering"/>, and <see cref="Wrapping"/>. 
/// </para>
/// <para>
/// A sampler state is an immutable object, and as such can only be created by using a <see cref="GorgonSamplerStateBuilder"/>.
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphics"/>
/// <seealso cref="GorgonPipelineState"/>
/// <seealso cref="GorgonSamplerStateBuilder"/>
public class GorgonSamplerState
    : IEquatable<GorgonSamplerState>
{
    #region Common sampler states.
    /// <summary>
    /// A default sampler state.
    /// </summary>
    public static readonly GorgonSamplerState Default = new();

    /// <summary>
    /// A sampler state that turns on texture wrapping when texture coordinates exceed the range of the texture size.
    /// </summary>
        public static readonly GorgonSamplerState Wrapping = new()
    {
        WrapU = TextureWrap.Wrap,
        WrapV = TextureWrap.Wrap,
        WrapW = TextureWrap.Wrap
    };

    /// <summary>
    /// A sampler state that provides point filtering for the complete texture.
    /// </summary>
    public static readonly GorgonSamplerState PointFiltering = new()
    {
        Filter = SampleFilter.MinMagMipPoint
    };

    /// <summary>
    /// A sampler state that provides anisotropic filtering for the complete texture.
    /// </summary>
    public static readonly GorgonSamplerState AnisotropicFiltering = new()
    {
        Filter = SampleFilter.Anisotropic
    };

    /// <summary>
    /// A sampler state that turns on texture wrapping when texture coordinates exceed the range of the texture size.
    /// </summary>
    public static readonly GorgonSamplerState PointFilteringWrapping = new()
    {
        Filter = SampleFilter.MinMagMipPoint,
        WrapU = TextureWrap.Wrap,
        WrapV = TextureWrap.Wrap,
        WrapW = TextureWrap.Wrap
    };
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the Direct3D native sampler state.
    /// </summary>
    internal D3D11.SamplerState Native
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the state ID.
    /// </summary>
    public int ID
    {
        get;
        internal set;
    } = int.MinValue;

    /// <summary>
    /// Property to return the type of filtering to apply to the texture.
    /// </summary>
    /// <exception cref="GorgonException">Thrown if the state has been assigned to a sampler slot.</exception>
    /// <remarks>
    /// <para>
    /// This applies a filter when the texture is zoomed in, or out so that edges appear more smooth when magnified, and have less shimmer effect when minified. Filters can be applied to mip levels as well 
    /// as the overall texture.
    /// </para>
    /// <para>
    /// Click this <a target="_blank" href="https://msdn.microsoft.com/en-us/library/windows/desktop/ff476132(v=vs.85).aspx">link</a> for a full description of each filter type.
    /// </para>
    /// <para>
    /// The default value is <see cref="SampleFilter.MinMagMipLinear"/>.
    /// </para>
    /// </remarks>
    public SampleFilter Filter
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the horizontal-U direction wrapping mode for a texture.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This tells the sampler how to resolve texture data outside of the 0..1.0f range.
    /// </para>
    /// <para>
    /// The default value is <c>Clamp</c>.
    /// </para>
    /// </remarks>
    public TextureWrap WrapU
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the verical-V direction wrapping mode for a texture.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This tells the sampler how to resolve texture data outside of the 0..1.0f range.
    /// </para>
    /// <para>
    /// The default value is <c>Clamp</c>.
    /// </para>
    /// </remarks>
    public TextureWrap WrapV
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the depth-W direction wrapping mode for a texture.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This tells the sampler how to resolve texture data outside of the 0..1.0f range.
    /// </para>
    /// <para>
    /// The default value is <c>Clamp</c>.
    /// </para>
    /// </remarks>
    public TextureWrap WrapW
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the offset for a calculated mip map level.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This adds the specified value to the mip map level that was calculated by the renderer.
    /// </para>
    /// <para>
    /// The default value is 0.0f.
    /// </para>
    /// </remarks>
    public float MipLevelOfDetailBias
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the value used to clamp an anisotropic texture filter.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is used to clamp the <see cref="Filter"/> of the texture when it is set to an anisotropic value.
    /// </para>
    /// <para>
    /// The default value is 1.
    /// </para>
    /// </remarks>
    public int MaxAnisotropy
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the function to compare sampled data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This sets how a comparison between current sampled data and existing sampled data is handled.
    /// </para>
    /// <para>
    /// The default value is <see cref="Comparison.Never"/>.
    /// </para>
    /// </remarks>
    public Comparison ComparisonFunction
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the color to use when using border addressing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value defines the color to use when the <see cref="WrapU"/>, <see cref="WrapV"/> or <see cref="WrapW"/> is set to <c>Border</c>. When those address modes are defined, the renderer 
    /// will draw a border in the color specified when the addressing exceeds 0.0f and 1.0f.
    /// </para>
    /// <para>
    /// The default value is <see cref="GorgonColor.Transparent"/>.
    /// </para>
    /// </remarks>
    public GorgonColor BorderColor
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the minimum mip level of detail to use.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This defines the lower end of the mip map range to clamp access to. If this value is 0, then the largest and most detailed mip level is used, and any value higher results in less detailed mip 
    /// levels.
    /// </para>
    /// <para>
    /// The default value is <see cref="float.MinValue"/> (-3.40282346638528859e+38).
    /// </para>
    /// </remarks>
    public float MinimumLevelOfDetail
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the minimum mip level of detail to use.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This defines the upper end of the mip map range to clamp access to. If this value is 0, then the largest and most detailed mip level is used, and any value higher results in less detailed mip 
    /// levels. 
    /// </para>
    /// <para>
    /// This value should be greater than <see cref="MinimumLevelOfDetail"/>, if it is not, then Gorgon will swap these values upon state creation.
    /// </para>
    /// <para>
    /// The default value is <see cref="float.MinValue"/> (-3.40282346638528859e+38).
    /// </para>
    /// </remarks>
    public float MaximumLevelOfDetail
    {
        get;
        internal set;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to build the D3D11 sampler state.
    /// </summary>
    /// <param name="device">The D3D11 device to use to create the sampler.</param>
    internal void BuildD3D11SamplerState(D3D11.Device5 device)
    {
        Debug.Assert(Native is null, "Already have a native sampler state.");

        var desc = new D3D11.SamplerStateDescription
        {
            BorderColor = BorderColor.ToRawColor4(),
            ComparisonFunction = (D3D11.Comparison)ComparisonFunction,
            Filter = (D3D11.Filter)Filter,
            AddressW = (D3D11.TextureAddressMode)WrapW,
            AddressV = (D3D11.TextureAddressMode)WrapV,
            AddressU = (D3D11.TextureAddressMode)WrapU,
            MipLodBias = MipLevelOfDetailBias,
            MinimumLod = MinimumLevelOfDetail,
            MaximumLod = MaximumLevelOfDetail,
            MaximumAnisotropy = MaxAnisotropy
        };

        Native = new D3D11.SamplerState(device, desc);
    }

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
    public bool Equals(GorgonSamplerState other) => (this == other) || ((other is not null)
                                   && (other.WrapU == WrapU)
                                   && (other.WrapV == WrapV)
                                   && (other.WrapW == WrapW)
                                   && (other.BorderColor.Equals(BorderColor))
                                   && (other.ComparisonFunction == ComparisonFunction)
                                   && (other.Filter == Filter)
                                   && (other.MaxAnisotropy == MaxAnisotropy)
                                   && (other.MinimumLevelOfDetail.EqualsEpsilon(MinimumLevelOfDetail))
                                   && (other.MaximumLevelOfDetail.EqualsEpsilon(MaximumLevelOfDetail))
                                   && (other.MipLevelOfDetailBias.EqualsEpsilon(MipLevelOfDetailBias)));

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="obj">An object to compare with this object.</param>
    /// <returns><see langword="true" /> if the current object is equal to the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object obj) => Equals(obj as GorgonSamplerState);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
        HashCode hashCode = new();
        hashCode.Add(WrapU);
        hashCode.Add(WrapV);
        hashCode.Add(WrapW);
        hashCode.Add(BorderColor);
        hashCode.Add(ComparisonFunction);
        hashCode.Add(Filter);
        hashCode.Add(MaxAnisotropy);
        hashCode.Add(MinimumLevelOfDetail);
        hashCode.Add(MaximumLevelOfDetail);
        hashCode.Add(MipLevelOfDetailBias);
        return hashCode.ToHashCode();            
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonSamplerState"/> class.
    /// </summary>
    /// <param name="state">The state to copy.</param>
    internal GorgonSamplerState(GorgonSamplerState state)
    {
        Filter = state.Filter;
        WrapU = state.WrapU;
        WrapV = state.WrapV;
        WrapW = state.WrapW;
        MaxAnisotropy = state.MaxAnisotropy;
        BorderColor = state.BorderColor;
        MinimumLevelOfDetail = state.MinimumLevelOfDetail;
        MaximumLevelOfDetail = state.MaximumLevelOfDetail;
        ComparisonFunction = state.ComparisonFunction;
        MipLevelOfDetailBias = state.MipLevelOfDetailBias;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonSamplerState"/> class.
    /// </summary>
    internal GorgonSamplerState()
    {
        Filter = SampleFilter.MinMagMipLinear;
        WrapU = TextureWrap.Clamp;
        WrapV = TextureWrap.Clamp;
        WrapW = TextureWrap.Clamp;
        MaxAnisotropy = 1;
        BorderColor = GorgonColor.White;
        MinimumLevelOfDetail = float.MinValue;
        MaximumLevelOfDetail = float.MaxValue;
        ComparisonFunction = Comparison.Never;
        MipLevelOfDetailBias = 0.0f;
    }
    #endregion
}
