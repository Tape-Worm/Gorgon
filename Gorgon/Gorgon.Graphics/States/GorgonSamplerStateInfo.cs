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
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Information used to create a <see cref="GorgonSamplerState"/>.
	/// </summary>
	public class GorgonSamplerStateInfo
		: IGorgonSamplerStateInfo
	{
		#region Variables.
		/// <summary>
		/// A default sampler state.
		/// </summary>
		public static readonly IGorgonSamplerStateInfo Default = new GorgonSamplerStateInfo();

		/// <summary>
		/// A sampler state that provides point filtering for the complete texture.
		/// </summary>
		public static readonly IGorgonSamplerStateInfo PointFiltering = new GorgonSamplerStateInfo
		                                                                {
			                                                                Filter = D3D11.Filter.MinMagMipPoint
		                                                                };

		/// <summary>
		/// A sampler state that provides anisotropic filtering for the complete texture.
		/// </summary>
		public static readonly IGorgonSamplerStateInfo AnisotropicFiltering = new GorgonSamplerStateInfo
		                                                                      {
			                                                                      Filter = D3D11.Filter.Anisotropic
		                                                                      };
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the type of filtering to apply to the texture.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This applies a filter when the texture is zoomed in, or out so that edges appear more smooth when magnified, and have less shimmer effect when minified. Filters can be applied to mip levels as well 
		/// as the overall texture.
		/// </para>
		/// <para>
		/// Click this <a target="_blank" href="https://msdn.microsoft.com/en-us/library/windows/desktop/ff476132(v=vs.85).aspx">link</a> for a full description of each filter type.
		/// </para>
		/// <para>
		/// The default value is <c>MinMagMipLinear</c>.
		/// </para>
		/// </remarks>
		public D3D11.Filter Filter
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the horizontal-U direction addressing for a texture.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This tells the sampler how to resolve texture data outside of the 0..1.0f range.
		/// </para>
		/// <para>
		/// The default value is <c>Clamp</c>.
		/// </para>
		/// </remarks>
		public D3D11.TextureAddressMode AddressU
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the verical-V direction addressing for a texture.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This tells the sampler how to resolve texture data outside of the 0..1.0f range.
		/// </para>
		/// <para>
		/// The default value is <c>Clamp</c>.
		/// </para>
		/// </remarks>
		public D3D11.TextureAddressMode AddressV
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the depth-W direction addressing for a texture.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This tells the sampler how to resolve texture data outside of the 0..1.0f range.
		/// </para>
		/// <para>
		/// The default value is <c>Clamp</c>.
		/// </para>
		/// </remarks>
		public D3D11.TextureAddressMode AddressW
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the offset for a calculated mip map level.
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
			set;
		}

		/// <summary>
		/// Property to set or return the value used to clamp an anisotropic texture filter.
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
			set;
		}

		/// <summary>
		/// Property to set or return the function to compare sampled data.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This sets how a comparison between current sampled data and existing sampled data is handled.
		/// </para>
		/// <para>
		/// The default value is <c>Never</c>.
		/// </para>
		/// </remarks>
		public D3D11.Comparison ComparisonFunction
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the color to use when using border addressing.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value defines the color to use when the <see cref="AddressU"/>, <see cref="AddressV"/> or <see cref="AddressW"/> is set to <c>Border</c>. When those address modes are defined, the renderer 
		/// will draw a border in the color specified when the addressing exceeds 0.0f and 1.0f.
		/// </para>
		/// <para>
		/// The default value is <see cref="GorgonColor.Transparent"/>.
		/// </para>
		/// </remarks>
		public GorgonColor BorderColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the minimum mip level of detail to use.
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
			set;
		}

		/// <summary>
		/// Property to set or return the minimum mip level of detail to use.
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
			set;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSamplerStateInfo"/> class.
		/// </summary>
		/// <param name="info">A <see cref="IGorgonSamplerStateInfo"/> to copy the settings from.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
		public GorgonSamplerStateInfo(IGorgonSamplerStateInfo info)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			Filter = info.Filter;
			AddressU = info.AddressU;
			AddressV = info.AddressV;
			AddressW = info.AddressW;
			MaxAnisotropy = info.MaxAnisotropy;
			BorderColor = info.BorderColor;
			MinimumLevelOfDetail = info.MinimumLevelOfDetail;
			MaximumLevelOfDetail = info.MaximumLevelOfDetail;
			ComparisonFunction = info.ComparisonFunction;
			MipLevelOfDetailBias = info.MipLevelOfDetailBias;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSamplerStateInfo"/> class.
		/// </summary>
		public GorgonSamplerStateInfo()
		{
			Filter = D3D11.Filter.MinMagMipLinear;
			AddressU = D3D11.TextureAddressMode.Clamp;
			AddressV = D3D11.TextureAddressMode.Clamp;
			AddressW = D3D11.TextureAddressMode.Clamp;
			MaxAnisotropy = 1;
			BorderColor = GorgonColor.Transparent;
			MinimumLevelOfDetail = float.MinValue;
			MaximumLevelOfDetail = float.MaxValue;
			ComparisonFunction = D3D11.Comparison.Never;
			MipLevelOfDetailBias = 0.0f;
		}
		#endregion
	}
}
