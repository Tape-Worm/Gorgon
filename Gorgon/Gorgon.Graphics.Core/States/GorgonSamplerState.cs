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
using System.Threading;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Describes how texture sampling should be performed when a texture is sampled in a shader.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This will define how a texture is sampled inside of a shader when rendering. Filtering, addressing, etc... are all defined in this state.
	/// </para>
	/// <para>
	/// This type is mutable until it is assigned to a sampler slot in a <see cref="GorgonSamplerStates"/> list. After it is assigned, it is immutable and will throw an exception if any changes are made 
	/// to the state after assignment.
	/// </para>
	/// <para>
	/// Because of the state locking and for performance, it is best practice to pre-create the required states ahead of time.
	/// </para>
	/// <para>
	/// The sampler state contains 3 common samplers used by applications: <see cref="Default"/> (bilinear filtering), <see cref="PointFiltering"/> (point or "pixelated" filtering), and 
	/// <see cref="AnisotropicFiltering"/>. These states are always locked, and cannot be changed, but can be used as a base for a new state by using the <see cref="GorgonSamplerState(GorgonSamplerState)"/> 
	/// copy constructor and then modifying as needed.
	/// </para>
	/// </remarks>
	/// <seealso cref="GorgonSamplerStates"/>
	/// <seealso cref="Default"/>
	/// <seealso cref="PointFiltering"/>
	/// <seealso cref="AnisotropicFiltering"/>
	public class GorgonSamplerState
        : IEquatable<GorgonSamplerState>
	{
		#region Common sampler states.
	    /// <summary>
	    /// A default sampler state.
	    /// </summary>
	    public static readonly GorgonSamplerState Default = new GorgonSamplerState
	                                                        {
	                                                            IsLocked = true
	                                                        };


	    /// <summary>
	    /// A sampler state that provides point filtering for the complete texture.
	    /// </summary>
	    public static readonly GorgonSamplerState PointFiltering = new GorgonSamplerState
	                                                               {
	                                                                   Filter = D3D11.Filter.MinMagMipPoint,
	                                                                   IsLocked = true
	                                                               };

	    /// <summary>
	    /// A sampler state that provides anisotropic filtering for the complete texture.
	    /// </summary>
	    public static readonly GorgonSamplerState AnisotropicFiltering = new GorgonSamplerState
	                                                                     {
	                                                                         Filter = D3D11.Filter.Anisotropic,
	                                                                         IsLocked = true
	                                                                     };
        #endregion

        #region Variables.
	    // The state ID.
	    private static long _stateID;
        // The texture filter.
        private D3D11.Filter _filter;
        // The addressing mode for moving beyond texture horizontal extents.
	    private D3D11.TextureAddressMode _addressU;
	    // The addressing mode for moving beyond texture vertical extents.
        private D3D11.TextureAddressMode _addressV;
	    // The addressing mode for moving beyond texture depth extents.
        private D3D11.TextureAddressMode _addressW;
        // Current mip map level of detail bias.
	    private float _mipLevelOfDetailBias;
        // Maxium anisotropy value.
	    private int _maxAnisotropy;
        // The comparsion function for filtering.
	    private D3D11.Comparison _comparisonFunction;
        // The border color for bordered addressing.
	    private GorgonColor _borderColor;
        // The minimum LOD for a mip mapped texture.
	    private float _minimumLevelOfDetail;
        // The maximum LOD for a mip mapped texture.
	    private float _maximumLevelOfDetail;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the native D3D11 sampler state.
        /// </summary>
        internal D3D11.SamplerState Native
	    {
	        get;
	        set;
	    }

        /// <summary>
        /// Property to set or return whether the sampler state is locked for writing.
        /// </summary>
	    internal bool IsLocked
	    {
	        get;
	        set;
	    }

	    /// <summary>
	    /// Property to return the state ID.
	    /// </summary>
	    public long ID
	    {
	        get;
	    }

        /// <summary>
        /// Property to set or return the type of filtering to apply to the texture.
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
        /// The default value is <c>MinMagMipLinear</c>.
        /// </para>
        /// </remarks>
        public D3D11.Filter Filter
	    {
	        get => _filter;
	        set
	        {
                CheckLocked();
	            _filter = value;
	        }
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
	        get => _addressU;
	        set
	        {
                CheckLocked();
	            _addressU = value;
            }
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
	        get => _addressV;
	        set
	        {
                CheckLocked();
	            _addressV = value;
	        }
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
	        get => _addressW;
	        set
	        {
                CheckLocked();
	            _addressW = value;
	        }
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
	        get => _mipLevelOfDetailBias;
	        set
	        {
                CheckLocked();
	            _mipLevelOfDetailBias = value;
	        }
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
	        get => _maxAnisotropy;
	        set
	        {
                CheckLocked();
	            _maxAnisotropy = value;
	        }
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
	        get => _comparisonFunction;
	        set
	        {
                CheckLocked();
	            _comparisonFunction = value;
	        }
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
	        get => _borderColor;
	        set
	        {
                CheckLocked();
	            _borderColor = value;
	        }
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
	        get => _minimumLevelOfDetail;
	        set
	        {
                CheckLocked();
	            _minimumLevelOfDetail = value;
	        }
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
	        get => _maximumLevelOfDetail;
	        set
	        {
                CheckLocked();
	            _maximumLevelOfDetail = value;
	        }
	    }
	    #endregion

        #region Methods.
        /// <summary>
        /// Function to determine if the state is locked for read-only access.
        /// </summary>
	    private void CheckLocked()
	    {
	        if (IsLocked)
	        {
	            throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_ERR_STATE_IMMUTABLE, GetType().Name));
	        }

            // If we make a change, and we had a native sampler assigned (from the copy constructor), then reset it here so Gorgon will assign a new native state upon rendering.
	        Native = null;
	    }

	    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	    /// <param name="other">An object to compare with this object.</param>
	    /// <returns>
	    /// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
	    public bool Equals(GorgonSamplerState other)
	    {
	        return (this == other) || (other != null
	                                   && other.AddressU == AddressU
	                                   && other.AddressV == AddressV
	                                   && other.AddressW == AddressW
	                                   && other.BorderColor.Equals(BorderColor)
	                                   && other.ComparisonFunction == ComparisonFunction
	                                   && other.Filter == Filter
	                                   && other.MaxAnisotropy == MaxAnisotropy
	                                   && other.MinimumLevelOfDetail.EqualsEpsilon(MinimumLevelOfDetail)
	                                   && other.MaximumLevelOfDetail.EqualsEpsilon(MaximumLevelOfDetail)
	                                   && other.MipLevelOfDetailBias.EqualsEpsilon(MipLevelOfDetailBias));
	    }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonSamplerState"/> class.
        /// </summary>
        /// <param name="info">A <see cref="GorgonSamplerState"/> to copy the settings from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
        public GorgonSamplerState(GorgonSamplerState info)
		{
		    // Copy the original native state reference, this will make any duplicated (i.e. no changes)
            // state a little faster to assign when rendering.
		    ID = Interlocked.Increment(ref _stateID);
            Native = info?.Native ?? throw new ArgumentNullException(nameof(info));
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
		/// Initializes a new instance of the <see cref="GorgonSamplerState"/> class.
		/// </summary>
		public GorgonSamplerState()
		{
		    ID = Interlocked.Increment(ref _stateID);
            Filter = D3D11.Filter.MinMagMipLinear;
			AddressU = D3D11.TextureAddressMode.Clamp;
			AddressV = D3D11.TextureAddressMode.Clamp;
			AddressW = D3D11.TextureAddressMode.Clamp;
			MaxAnisotropy = 1;
			BorderColor = GorgonColor.White;
			MinimumLevelOfDetail = float.MinValue;
			MaximumLevelOfDetail = float.MaxValue;
			ComparisonFunction = D3D11.Comparison.Never;
			MipLevelOfDetailBias = 0.0f;
		}
        #endregion
    }
}
