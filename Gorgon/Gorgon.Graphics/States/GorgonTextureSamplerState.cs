#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Monday, December 19, 2011 9:43:57 AM
// 
#endregion

using System;
using System.Drawing;
using Gorgon.Core;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Texture addressing flags.
	/// </summary>
	public enum TextureAddressing
	{
		/// <summary>
		/// Unknown.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Wrap and repeat the texture at the texture boundary.
		/// </summary>
		Wrap = 1,
		/// <summary>
		/// Wrap and mirror the texture at the texture boundary.
		/// </summary>
		Mirror = 2,
		/// <summary>
		/// Clamp the texture at the texture boundary.
		/// </summary>
		Clamp = 3,
		/// <summary>
		/// Assign a border color at the texture boundary.
		/// </summary>
		Border = 4,
		/// <summary>
		/// Wrap and mirror the texture only once at the texture boundary.
		/// </summary>
		MirrorOnce = 5
	}

	/// <summary>
	/// Filtering to apply to a texture.
	/// </summary>
	[Flags]
	public enum TextureFilter
	{
		/// <summary>
		/// No filter.  This is equivalent to the Point value.
		/// </summary>
		None = 0,
		/// <summary>
		/// Point minification filtering.
		/// </summary>
		MinPoint = 1,
		/// <summary>
		/// Point magnifcation filtering.
		/// </summary>
		MagPoint = 2,
		/// <summary>
		/// Linear minification filtering.
		/// </summary>
		MinLinear = 4,
		/// <summary>
		/// Linear magnifcation filtering.
		/// </summary>
		MagLinear = 8,
		/// <summary>
		/// Mip map point sampling.
		/// </summary>
		MipPoint = 16,
		/// <summary>
		/// Mip map linear sampling.
		/// </summary>
		MipLinear = 32,
		/// <summary>
		/// Compare the result to the comparison value.
		/// </summary>
		Comparison = 64,
		/// <summary>
		/// Linear filtering.
		/// </summary>
		Linear = MinLinear | MagLinear | MipLinear,
		/// <summary>
		/// Point filtering.
		/// </summary>
		Point = MinPoint | MagPoint | MipPoint,
		/// <summary>
		/// Point filtering with comparison.
		/// </summary>
		ComparePoint = MinPoint | MagPoint | MipPoint | Comparison,
		/// <summary>
		/// Linear filtering with comparison.
		/// </summary>
		CompareLinear = MinLinear | MagLinear | MipLinear | Comparison,
		/// <summary>
		/// 1 bit texture for text.
		/// </summary>
		Text1Bit = 32768,
		/// <summary>
		/// Anisotropic filtering.
		/// </summary>
		/// <remarks>This flag is mutually exclusive and applies to minification, magnification and mip mapping.</remarks>
		Anisotropic = 65536,
		/// <summary>
		/// Anisotropic filtering with comparison.
		/// </summary>
		/// <remarks>This flag is mutually exclusive and applies to minification, magnification and mip mapping.</remarks>
		CompareAnisotropic = 131072
	}

	/// <summary>
	/// States for the texture samplers.
	/// </summary>
	public struct GorgonTextureSamplerStates
		: IEquatableByRef<GorgonTextureSamplerStates>
	{
		#region Variables.
		/// <summary>
		/// Texture sampler state with linear filtering.
		/// </summary>
		public static readonly GorgonTextureSamplerStates LinearFilter = new GorgonTextureSamplerStates
			{
				TextureFilter = TextureFilter.Linear,
				HorizontalAddressing = TextureAddressing.Clamp,
				VerticalAddressing = TextureAddressing.Clamp,
				DepthAddressing = TextureAddressing.Clamp,
				MipLODBias = 0.0f,
				MaxAnisotropy = 1,
				ComparisonFunction = ComparisonOperator.Never,
				BorderColor = Color.White,
				MinLOD = -3.402823466e+38f,
				MaxLOD = 3.402823466e+38f
			};

        /// <summary>
        /// Texture sampler state with point filtering.
        /// </summary>
        public static readonly GorgonTextureSamplerStates PointFilter = new GorgonTextureSamplerStates
        {
            TextureFilter = TextureFilter.Point,
            HorizontalAddressing = TextureAddressing.Clamp,
            VerticalAddressing = TextureAddressing.Clamp,
            DepthAddressing = TextureAddressing.Clamp,
            MipLODBias = 0.0f,
            MaxAnisotropy = 1,
            ComparisonFunction = ComparisonOperator.Never,
            BorderColor = Color.White,
            MinLOD = -3.402823466e+38f,
            MaxLOD = 3.402823466e+38f
        };

		/// <summary>
		/// Filtering to apply to the texture.
		/// </summary>
		public TextureFilter TextureFilter;
		/// <summary>
		/// Horizontal texture addressing (U).
		/// </summary>
		public TextureAddressing HorizontalAddressing;
		/// <summary>
		/// Vertical texture addressing (V).
		/// </summary>
		public TextureAddressing VerticalAddressing;
		/// <summary>
		/// Depth texture addressing (W).
		/// </summary>
		public TextureAddressing DepthAddressing;
		/// <summary>
		/// Offset from the calculated mipmap level.
		/// </summary>
		public float MipLODBias;
		/// <summary>
		/// Clamping value used when texture filtering is set to anisotropic.
		/// </summary>
		/// <remarks>This value should only be from 1 to 16.</remarks>
		public int MaxAnisotropy;
		/// <summary>
		/// Function to compare sampled data against existing data.
		/// </summary>
		public ComparisonOperator ComparisonFunction;
		/// <summary>
		/// Color used when the horizontal, vertical or depth addressing are set to border.
		/// </summary>
		public GorgonColor BorderColor;
		/// <summary>
		/// Lowest level of detail to use in the mip map range.
		/// </summary>
		public float MinLOD;
		/// <summary>
		/// Highest level of detail to use int he mip map range.
		/// </summary>
		public float MaxLOD;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if two texture sampler states are equal or not.
		/// </summary>
		/// <param name="left">Left side to evaluate.</param>
		/// <param name="right">Right side to evaluate.</param>
		/// <returns><c>true</c> if equal, <c>false</c> if not.</returns>
		public static bool Equals(ref GorgonTextureSamplerStates left, ref GorgonTextureSamplerStates right)
		{
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return ((GorgonColor.Equals(ref left.BorderColor, ref right.BorderColor)) && (left.ComparisonFunction == right.ComparisonFunction) && (left.DepthAddressing == right.DepthAddressing) &&
				(left.HorizontalAddressing == right.HorizontalAddressing) && (left.MaxAnisotropy == right.MaxAnisotropy) && (left.MaxLOD == right.MaxLOD) &&
				(left.MinLOD == right.MinLOD) && (left.MipLODBias == right.MipLODBias) && (left.TextureFilter == right.TextureFilter) &&
				(left.VerticalAddressing == right.VerticalAddressing));
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonTextureSamplerStates)
			{
				return Equals((GorgonTextureSamplerStates)obj);
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			unchecked
			{
				// ReSharper disable NonReadonlyFieldInGetHashCode
				return 281.GenerateHash(BorderColor).GenerateHash(ComparisonFunction).GenerateHash(DepthAddressing).
					GenerateHash(HorizontalAddressing).GenerateHash(MaxAnisotropy).GenerateHash(MaxLOD).
					GenerateHash(MinLOD).GenerateHash(MipLODBias).GenerateHash(TextureFilter).GenerateHash(VerticalAddressing);
				// ReSharper restore NonReadonlyFieldInGetHashCode
			}
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==(GorgonTextureSamplerStates left, GorgonTextureSamplerStates right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(GorgonTextureSamplerStates left, GorgonTextureSamplerStates right)
		{
			return !Equals(ref left, ref right);
		}
		#endregion

		#region IEquatable<GorgonTextureSamplerStates> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the other parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonTextureSamplerStates other)
		{
			return Equals(ref this, ref other);
		}
		#endregion

		#region IEquatableByRef<GorgonTextureSamplerStates> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(ref GorgonTextureSamplerStates other)
		{
			return Equals(ref this, ref other);			
		}
		#endregion
	}
}
